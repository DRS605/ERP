using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de integración de los modelos 111 y 190 (retenciones de IRPF).</summary>
public sealed class RetencionesIrpfEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;
    public RetencionesIrpfEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record Modelo111Resp(int Anio, int Trimestre, int NumeroPerceptores, decimal BasePercepciones, decimal Retenciones, decimal TotalAIngresar);
    private sealed record PerceptorResp(string Clave, string Nombre, string? Nif, decimal BasePercepciones, decimal Retenciones);
    private sealed record Modelo190Resp(int Anio, int NumeroPerceptores, decimal TotalPercepciones, decimal TotalRetenciones, List<PerceptorResp> Perceptores, List<PerceptorResp> PerceptoresSinNif);

    [Fact]
    public async Task Modelos_111_y_190_de_retenciones_y_fichero_oficial()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);

        var prov = (await (await cliente.PostAsJsonAsync("/proveedores",
            new { Nombre = "Ana Profesional", NifFiscal = "11111111H", Provincia = "Madrid" })).Content.ReadFromJsonAsync<IdResp>())!;

        // Gasto con retención del 15% en el 1er trimestre: base 1000 → retención 150.
        await cliente.PostAsJsonAsync("/gastos", new
        {
            Concepto = "Servicios profesionales",
            BaseImponible = 1000m,
            CodigoIva = "IVA21",
            ProveedorId = prov.Id,
            PorcentajeIrpf = 15m,
            Fecha = "2026-02-10",
        });

        // Modelo 111 (T1).
        var m111 = await cliente.GetFromJsonAsync<Modelo111Resp>("/informes/modelo-111?anio=2026&trimestre=1");
        m111!.NumeroPerceptores.Should().Be(1);
        m111.BasePercepciones.Should().Be(1000m);
        m111.Retenciones.Should().Be(150m);
        m111.TotalAIngresar.Should().Be(150m);

        // En el 2º trimestre no hay retenciones.
        var m111t2 = await cliente.GetFromJsonAsync<Modelo111Resp>("/informes/modelo-111?anio=2026&trimestre=2");
        m111t2!.Retenciones.Should().Be(0m);

        // Modelo 190 (anual).
        var m190 = await cliente.GetFromJsonAsync<Modelo190Resp>("/informes/modelo-190?anio=2026");
        m190!.Perceptores.Should().ContainSingle();
        m190.Perceptores[0].Nif.Should().Be("11111111H");
        m190.Perceptores[0].Retenciones.Should().Be(150m);
        m190.TotalRetenciones.Should().Be(150m);

        // Fichero oficial del 190: registros de 500 posiciones.
        var fichero = await cliente.GetAsync("/informes/modelo-190/fichero?anio=2026");
        fichero.StatusCode.Should().Be(HttpStatusCode.OK);
        var texto = await fichero.Content.ReadAsStringAsync();
        var lineas = texto.Split("\r\n");
        lineas.Should().HaveCount(2);
        lineas[0].Length.Should().Be(500);
        lineas[1].Length.Should().Be(500);
        lineas[0][..4].Should().Be("1190"); // registro declarante del modelo 190
        lineas[1][..4].Should().Be("2190"); // registro perceptor
    }

    [Fact]
    public async Task El_fichero_190_sin_perceptores_con_nif_avisa()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        // Gasto con retención pero proveedor solo en texto (sin ficha → sin NIF).
        await cliente.PostAsJsonAsync("/gastos", new
        {
            Concepto = "Charla",
            BaseImponible = 200m,
            CodigoIva = "IVA21",
            ProveedorTexto = "Ponente ocasional",
            PorcentajeIrpf = 15m,
            Fecha = "2026-03-01",
        });

        var fichero = await cliente.GetAsync("/informes/modelo-190/fichero?anio=2026");
        fichero.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
