using System.Text;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Informes.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Consultas;
using AlxorCore.Organizacion.Aplicacion.Modelos;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Organizacion.Dominio;
using AlxorCore.Terceros.Aplicacion;
using AlxorCore.Terceros.Dominio;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Informes.Tests;

public class RetencionesIrpfTests
{
    private static readonly Guid Empresa = Guid.NewGuid();
    private static readonly Guid ProvA = Guid.NewGuid();
    private static readonly Guid ProvB = Guid.NewGuid();

    private static GastoDto Gasto(Guid? proveedorId, DateOnly fecha, decimal baseImp, decimal retencion, string estado = "Registrado", string? texto = null) =>
        new(Guid.NewGuid(), proveedorId, texto, "Servicios profesionales", fecha, baseImp, "IVA21", 21m, baseImp * 0.21m, 15m, retencion, baseImp, estado);

    private static ProveedorDto Prov(Guid id, string nombre, string? nif, string provincia = "Madrid") =>
        new(id, nombre, nif, null, "C/ Mayor 1", "28001", "Madrid", provincia, "ES", 15m, true, default, null, null, null, null, null);

    private sealed class FakeGastos(IReadOnlyList<GastoDto> lista) : IConsultaGastos
    {
        public Task<GastoDto?> ObtenerAsync(Guid gastoId, CancellationToken ct = default) => Task.FromResult<GastoDto?>(null);
        public Task<IReadOnlyList<GastoDto>> ListarAsync(Guid empresaId, CancellationToken ct = default) => Task.FromResult(lista);
        public Task<PaginaResultado<GastoDto>> BuscarAsync(Guid e, FiltroGastos f, Paginacion p, CancellationToken ct = default) => throw new NotImplementedException();
    }

    private sealed class FakeProveedores(IReadOnlyList<ProveedorDto> lista) : IConsultaProveedores
    {
        public Task<ProveedorDto?> ObtenerAsync(Guid proveedorId, CancellationToken ct = default) => Task.FromResult<ProveedorDto?>(null);
        public Task<IReadOnlyList<ProveedorDto>> ListarAsync(Guid empresaId, bool incluirInactivos = false, CancellationToken ct = default) => Task.FromResult(lista);
        public Task<PaginaResultado<ProveedorDto>> BuscarAsync(Guid e, FiltroTerceros f, Paginacion p, CancellationToken ct = default) => throw new NotImplementedException();
    }

    private sealed class FakeEmpresas(EmpresaDto? empresa) : IConsultaEmpresas
    {
        public Task<EmpresaDto?> ObtenerAsync(Guid empresaId, CancellationToken ct = default) => Task.FromResult(empresa);
    }

    private static GenerarRetencionesIrpf Caso(IReadOnlyList<GastoDto> gastos, IReadOnlyList<ProveedorDto> proveedores, EmpresaDto? empresa = null) =>
        new(new FakeGastos(gastos), new FakeProveedores(proveedores), new FakeEmpresas(empresa));

    private static EmpresaDto EmpresaDeclarante() =>
        new(Empresa, "B12345674", "Mi Empresa SL", default(RegimenIva), "EUR", "ES", null, null, MetodoValoracion.Estandar, ControlRiesgo.Aviso);

    [Fact]
    public async Task El_111_suma_las_retenciones_del_trimestre_y_cuenta_perceptores()
    {
        var caso = Caso(
            [
                Gasto(ProvA, new DateOnly(2026, 2, 10), 1000m, 150m),
                Gasto(ProvB, new DateOnly(2026, 3, 5), 500m, 75m),
                Gasto(ProvA, new DateOnly(2026, 5, 1), 800m, 120m),   // T2, no cuenta en T1
                Gasto(ProvA, new DateOnly(2026, 2, 20), 200m, 0m),    // sin retención, no cuenta
            ],
            [Prov(ProvA, "Ana Profesional", "11111111H"), Prov(ProvB, "Bea Consultora", "22222222J")]);

        var m = await caso.Modelo111Async(Empresa, 2026, 1);

        m.NumeroPerceptores.Should().Be(2);
        m.BasePercepciones.Should().Be(1500m);
        m.Retenciones.Should().Be(225m);
        m.TotalAIngresar.Should().Be(225m);
    }

    [Fact]
    public async Task El_190_agrupa_por_perceptor_y_separa_los_que_no_tienen_nif()
    {
        var caso = Caso(
            [
                Gasto(ProvA, new DateOnly(2026, 2, 10), 1000m, 150m),
                Gasto(ProvA, new DateOnly(2026, 9, 1), 1000m, 150m),   // mismo perceptor, se agrega
                Gasto(null, new DateOnly(2026, 4, 4), 300m, 45m, texto: "Fontanero suelto"), // sin ficha → sin NIF
            ],
            [Prov(ProvA, "Ana Profesional", "11111111H")]);

        var m = await caso.Modelo190Async(Empresa, 2026);

        m.Perceptores.Should().ContainSingle();
        var ana = m.Perceptores[0];
        ana.Nif.Should().Be("11111111H");
        ana.BasePercepciones.Should().Be(2000m);
        ana.Retenciones.Should().Be(300m);
        ana.Clave.Should().Be("G");

        m.PerceptoresSinNif.Should().ContainSingle();
        m.PerceptoresSinNif[0].Nombre.Should().Be("Fontanero suelto");

        m.TotalRetenciones.Should().Be(345m); // incluye el que no tiene NIF en el total informativo
    }

    [Fact]
    public async Task El_fichero_190_respeta_el_diseno_de_registro_de_la_aeat()
    {
        var caso = Caso(
            [Gasto(ProvA, new DateOnly(2026, 2, 10), 1000m, 150m)],
            [Prov(ProvA, "Ánibal Núñez", "11111111H")],
            EmpresaDeclarante());

        var bytes = await caso.FicheroModelo190Async(Empresa, 2026);
        bytes.Should().NotBeNull();

        var texto = Encoding.Latin1.GetString(bytes!);
        var lineas = texto.Split("\r\n");
        lineas.Should().HaveCount(2); // 1 declarante + 1 perceptor
        lineas[0].Length.Should().Be(500);
        lineas[1].Length.Should().Be(500);

        // Cabecera (tipo 1).
        lineas[0][..8].Should().Be("11902026"); // tipo 1 + modelo 190 + ejercicio
        lineas[0].Substring(8, 9).Should().Be("B12345674"); // NIF declarante 9-17
        lineas[0][57].Should().Be('T'); // tipo de soporte (pos 58)

        // Perceptor (tipo 2).
        var p = lineas[1];
        p[..8].Should().Be("21902026");
        p.Substring(17, 9).Should().Be("11111111H");   // NIF perceptor 18-26
        p.Substring(35, 40).Trim().Should().Be("ANIBAL NUÑEZ"); // nombre 36-75, mayúsculas sin acentos (la Ñ se conserva)
        p.Substring(75, 2).Should().Be("28");          // provincia Madrid → 28
        p[77].Should().Be('G');                         // clave (pos 78)
        p.Substring(78, 2).Should().Be("01");          // subclave 79-80
        p.Substring(81, 13).Should().Be("0000000100000"); // percepción íntegra 82-94: 1000,00 → 100000
        p.Substring(94, 13).Should().Be("0000000015000"); // retenciones 95-107: 150,00 → 15000
    }

    [Fact]
    public void El_importe_negativo_lleva_el_signo_en_su_campo()
    {
        var r = new RegistroAeat(20).Importe(1, 2, 14, -1234.56m);
        var s = r.ToString();
        s[0].Should().Be('N');                       // signo
        s.Substring(1, 13).Should().Be("0000000123456"); // 1234,56 → 123456
    }

    [Fact]
    public void Provincia_desconocida_devuelve_99()
    {
        ProvinciasAeat.Codigo("Madrid").Should().Be("28");
        ProvinciasAeat.Codigo("Bizkaia").Should().Be("48");
        ProvinciasAeat.Codigo(null).Should().Be("99");
        ProvinciasAeat.Codigo("Ruritania").Should().Be("99");
    }
}
