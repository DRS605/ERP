using System.Text;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Informes.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Consultas;
using AlxorCore.Organizacion.Aplicacion.Modelos;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Organizacion.Dominio;
using AlxorCore.Terceros.Aplicacion;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Informes.Tests;

public class Modelo349Tests
{
    private static readonly Guid Empresa = Guid.NewGuid();
    private static readonly Guid ProvIntra = Guid.NewGuid();

    private sealed class FakeFacturas(IReadOnlyList<FacturaResumen> l) : IConsultaFacturas
    {
        public Task<FacturaDto?> ObtenerAsync(Guid id, CancellationToken ct = default) => Task.FromResult<FacturaDto?>(null);
        public Task<IReadOnlyList<FacturaResumen>> ListarAsync(Guid e, CancellationToken ct = default) => Task.FromResult(l);
        public Task<PaginaResultado<FacturaResumen>> BuscarAsync(Guid e, FiltroFacturas f, Paginacion p, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<LineaMargenDto>> ListarLineasMargenAsync(Guid e, DateOnly d, DateOnly h, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<LineaMargenDto>>([]);
    }

    private sealed class FakeGastos(IReadOnlyList<GastoDto> l) : IConsultaGastos
    {
        public Task<GastoDto?> ObtenerAsync(Guid id, CancellationToken ct = default) => Task.FromResult<GastoDto?>(null);
        public Task<IReadOnlyList<GastoDto>> ListarAsync(Guid e, CancellationToken ct = default) => Task.FromResult(l);
        public Task<PaginaResultado<GastoDto>> BuscarAsync(Guid e, FiltroGastos f, Paginacion p, CancellationToken ct = default) => throw new NotImplementedException();
    }

    private sealed class FakeClientes(IReadOnlyList<ClienteDto> l) : IConsultaClientes
    {
        public Task<ClienteDto?> ObtenerAsync(Guid id, CancellationToken ct = default) => Task.FromResult<ClienteDto?>(null);
        public Task<IReadOnlyList<ClienteDto>> ListarAsync(Guid e, bool inc = false, CancellationToken ct = default) => Task.FromResult(l);
        public Task<PaginaResultado<ClienteDto>> BuscarAsync(Guid e, FiltroTerceros f, Paginacion p, CancellationToken ct = default) => throw new NotImplementedException();
    }

    private sealed class FakeProveedores(IReadOnlyList<ProveedorDto> l) : IConsultaProveedores
    {
        public Task<ProveedorDto?> ObtenerAsync(Guid id, CancellationToken ct = default) => Task.FromResult<ProveedorDto?>(null);
        public Task<IReadOnlyList<ProveedorDto>> ListarAsync(Guid e, bool inc = false, CancellationToken ct = default) => Task.FromResult(l);
        public Task<PaginaResultado<ProveedorDto>> BuscarAsync(Guid e, FiltroTerceros f, Paginacion p, CancellationToken ct = default) => throw new NotImplementedException();
    }

    private sealed class FakeEmpresas(EmpresaDto e) : IConsultaEmpresas
    {
        public Task<EmpresaDto?> ObtenerAsync(Guid id, CancellationToken ct = default) => Task.FromResult<EmpresaDto?>(e);
    }

    private static FacturaResumen Factura(DateOnly fecha, string clienteNif, decimal baseImp) =>
        new(Guid.NewGuid(), "FA2026/0001", fecha, fecha, "Kunde GmbH", clienteNif, baseImp, 0m, 0m, baseImp, "Emitida", "Ordinaria", Guid.NewGuid());

    private static GastoDto Gasto(Guid provId, DateOnly fecha, decimal baseImp) =>
        new(Guid.NewGuid(), provId, "Fornitore", "Compra UE", fecha, baseImp, "IVA0", 0m, 0m, 0m, 0m, baseImp, "Registrado");

    private static ClienteDto ClienteIntra() =>
        new(Guid.NewGuid(), "Kunde GmbH", "DE-FISCAL", null, "", "", "", "", "DE", 0m, true, false, null, null, null, "DE123456789", null, null, null);

    private static ProveedorDto ProveedorIntra() =>
        new(ProvIntra, "Fornitore SRL", "IT-FISCAL", null, "", "", "", "", "IT", 0m, true, default, "IT99999999999", null, null, null, null);

    private static EmpresaDto EmpresaDeclarante() =>
        new(Empresa, "B12345674", "Mi Empresa SL", default(RegimenIva), "EUR", "ES", null, null, MetodoValoracion.Estandar, ControlRiesgo.Aviso, "", "", "", "", null, null, null, null, null, null);

    private static GenerarModelo349 Caso() => new(
        new FakeFacturas([Factura(new DateOnly(2026, 2, 1), "DE-FISCAL", 1000m)]),
        new FakeGastos([Gasto(ProvIntra, new DateOnly(2026, 3, 1), 500m)]),
        new FakeClientes([ClienteIntra()]),
        new FakeProveedores([ProveedorIntra()]),
        new FakeEmpresas(EmpresaDeclarante()));

    [Fact]
    public async Task El_349_separa_entregas_y_adquisiciones_intracomunitarias()
    {
        var m = await Caso().EjecutarAsync(Empresa, 2026, 1);

        m.Operadores.Should().HaveCount(2);
        m.Periodo.Should().Be("1T");
        m.Operadores.Single(o => o.Clave == "E").BaseImponible.Should().Be(1000m);
        m.Operadores.Single(o => o.Clave == "E").NifIva.Should().Be("DE123456789");
        m.Operadores.Single(o => o.Clave == "A").BaseImponible.Should().Be(500m);
        m.BaseTotal.Should().Be(1500m);
    }

    [Fact]
    public async Task Fuera_del_trimestre_no_hay_operaciones()
    {
        var m = await Caso().EjecutarAsync(Empresa, 2026, 4);
        m.Operadores.Should().BeEmpty();
    }

    [Fact]
    public async Task El_fichero_349_respeta_el_diseno_de_registro()
    {
        var bytes = await Caso().FicheroAsync(Empresa, 2026, 1);
        bytes.Should().NotBeNull();
        var lineas = Encoding.Latin1.GetString(bytes!).Split("\r\n");

        lineas.Should().HaveCount(3); // declarante + 2 operadores
        lineas.Should().OnlyContain(l => l.Length == 500);
        lineas[0][..4].Should().Be("1349");
        lineas[0].Substring(135, 2).Should().Be("1T");         // período (136-137)

        var entregas = lineas.Single(l => l.StartsWith("2349", System.StringComparison.Ordinal) && l[74] == 'E');
        entregas.Substring(17, 17).TrimEnd().Should().Be("DE123456789");  // NIF-IVA (18-34)
        entregas.Substring(75, 13).Should().Be("0000000100000");          // base 1000,00 (76-88)
    }
}
