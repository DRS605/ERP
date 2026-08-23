using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Fase 3 (documentos e informes): las facturas y los gastos guardan la actividad de negocio del
/// tercero al emitir/registrar, y el informe «por actividad» segmenta ventas y compras por ella.
/// </summary>
public sealed class InformePorActividadTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public InformePorActividadTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record ActividadResp(Guid Id, string Nombre, bool Activa);
    private sealed record IdResp(Guid Id);
    private sealed record ActividadResultadoDto(Guid? ActividadNegocioId, string Actividad, decimal Ventas, decimal Compras, decimal Resultado);
    private sealed record InformePorActividadDto(DateOnly Desde, DateOnly Hasta, decimal Ventas, decimal Compras, decimal Resultado, List<ActividadResultadoDto> Actividades);

    [Fact]
    public async Task El_informe_por_actividad_segmenta_ventas_y_compras()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);

        var comercial = (await (await cli.PostAsJsonAsync("/actividades", new { Nombre = "Comercial" }))
            .Content.ReadFromJsonAsync<ActividadResp>())!.Id;

        // Cliente y proveedor clasificados en «Comercial».
        var clienteCom = (await (await cli.PostAsJsonAsync("/clientes",
            new { Nombre = "Cliente Comercial", NifFiscal = "B12345674", ActividadNegocioId = comercial }))
            .Content.ReadFromJsonAsync<IdResp>())!.Id;
        var provCom = (await (await cli.PostAsJsonAsync("/proveedores",
            new { Nombre = "Proveedor Comercial", ActividadNegocioId = comercial }))
            .Content.ReadFromJsonAsync<IdResp>())!.Id;

        // Cliente sin actividad.
        var clienteSin = (await (await cli.PostAsJsonAsync("/clientes",
            new { Nombre = "Cliente General", NifFiscal = "B12345683" }))
            .Content.ReadFromJsonAsync<IdResp>())!.Id;

        // Factura de 200 (Comercial) + factura de 50 (sin actividad); gasto de 100 (Comercial).
        await cli.PostAsJsonAsync("/facturas", new { ClienteId = clienteCom, Lineas = new[] { new { Cantidad = 2m, Descripcion = "Servicio", PrecioUnitario = 100m, CodigoIva = "IVA21" } } });
        await cli.PostAsJsonAsync("/facturas", new { ClienteId = clienteSin, Lineas = new[] { new { Cantidad = 1m, Descripcion = "Otro", PrecioUnitario = 50m, CodigoIva = "IVA21" } } });
        await cli.PostAsJsonAsync("/gastos", new { Concepto = "Compra comercial", ProveedorId = provCom, BaseImponible = 100m, CodigoIva = "IVA21" });

        var informe = await cli.GetFromJsonAsync<InformePorActividadDto>("/informes/por-actividad");

        informe!.Ventas.Should().Be(250m);
        informe.Compras.Should().Be(100m);

        var filaComercial = informe.Actividades.Single(a => a.ActividadNegocioId == comercial);
        filaComercial.Actividad.Should().Be("Comercial");
        filaComercial.Ventas.Should().Be(200m);
        filaComercial.Compras.Should().Be(100m);
        filaComercial.Resultado.Should().Be(100m);

        var filaSin = informe.Actividades.Single(a => a.ActividadNegocioId == null);
        filaSin.Actividad.Should().Be("Sin actividad");
        filaSin.Ventas.Should().Be(50m);
        filaSin.Compras.Should().Be(0m);
    }
}
