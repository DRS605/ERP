using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas del filtrado y la paginación en servidor de los listados principales.</summary>
public sealed class FiltrosBusquedaEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public FiltrosBusquedaEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record PagResp<T>(IReadOnlyList<T> Elementos, int Total, int Pagina, int TamanoPagina, int TotalPaginas, bool HayMas);
    private sealed record FacturaResp(Guid Id, string NumeroCompleto, string ClienteNombre, decimal Total, string Estado);
    private sealed record GastoResp(Guid Id, string Concepto, decimal Total, string Estado);
    private sealed record TerceroResp(Guid Id, string Nombre, string? NifFiscal);
    private sealed record ProductoResp(Guid Id, string Nombre, string? Familia, Guid? FamiliaId);

    private static async Task<Guid> CrearClienteAsync(HttpClient c, string nombre, string nif)
    {
        var r = await c.PostAsJsonAsync("/clientes", new { Nombre = nombre, NifFiscal = nif });
        return (await r.Content.ReadFromJsonAsync<IdResp>())!.Id;
    }

    private static Task CrearFacturaAsync(HttpClient c, Guid clienteId, string fecha, decimal precio) =>
        c.PostAsJsonAsync("/facturas", new
        {
            ClienteId = clienteId,
            FechaEmision = fecha,
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = precio, CodigoIva = "IVA21" } },
        });

    [Fact]
    public async Task Facturas_filtran_por_texto_fecha_e_importe_y_paginan()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var acme = await CrearClienteAsync(cliente, "ACME Ibérica SL", "B12345674");
        var otro = await CrearClienteAsync(cliente, "Distribuciones Sur SA", "A58818501");
        await CrearFacturaAsync(cliente, acme, "2026-01-10", 100m);   // total 121
        await CrearFacturaAsync(cliente, acme, "2026-02-10", 1000m);  // total 1210
        await CrearFacturaAsync(cliente, otro, "2026-03-10", 500m);   // total 605

        // Texto por nombre de cliente.
        var porTexto = await cliente.GetFromJsonAsync<PagResp<FacturaResp>>("/facturas/buscar?texto=ACME");
        porTexto!.Total.Should().Be(2);
        porTexto.Elementos.Should().OnlyContain(f => f.ClienteNombre.Contains("ACME"));

        // Rango de fechas.
        var porFecha = await cliente.GetFromJsonAsync<PagResp<FacturaResp>>("/facturas/buscar?desde=2026-02-01&hasta=2026-03-31");
        porFecha!.Total.Should().Be(2);

        // Rango de importe (total ≥ 600).
        var porImporte = await cliente.GetFromJsonAsync<PagResp<FacturaResp>>("/facturas/buscar?importeMin=600");
        porImporte!.Total.Should().Be(2); // 1210 y 605

        // Paginación: 3 facturas, 2 por página → 2 páginas.
        var pag1 = await cliente.GetFromJsonAsync<PagResp<FacturaResp>>("/facturas/buscar?pagina=1&tamanoPagina=2");
        pag1!.Total.Should().Be(3);
        pag1.Elementos.Should().HaveCount(2);
        pag1.TotalPaginas.Should().Be(2);
        pag1.HayMas.Should().BeTrue();
        var pag2 = await cliente.GetFromJsonAsync<PagResp<FacturaResp>>("/facturas/buscar?pagina=2&tamanoPagina=2");
        pag2!.Elementos.Should().HaveCount(1);
        pag2.HayMas.Should().BeFalse();
    }

    [Fact]
    public async Task Gastos_filtran_por_texto_y_fecha()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await cliente.PostAsJsonAsync("/gastos", new { Concepto = "Luz enero", BaseImponible = 100m, CodigoIva = "IVA21", Fecha = "2026-01-05" });
        await cliente.PostAsJsonAsync("/gastos", new { Concepto = "Gasolina febrero", BaseImponible = 60m, CodigoIva = "IVA21", Fecha = "2026-02-05" });

        var porTexto = await cliente.GetFromJsonAsync<PagResp<GastoResp>>("/gastos/buscar?texto=luz");
        porTexto!.Total.Should().Be(1);
        porTexto.Elementos[0].Concepto.Should().Be("Luz enero");

        var porFecha = await cliente.GetFromJsonAsync<PagResp<GastoResp>>("/gastos/buscar?desde=2026-02-01");
        porFecha!.Total.Should().Be(1);
        porFecha.Elementos[0].Concepto.Should().Be("Gasolina febrero");
    }

    [Fact]
    public async Task Clientes_y_proveedores_buscan_por_texto()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await CrearClienteAsync(cliente, "Panadería López", "B12345674");
        await CrearClienteAsync(cliente, "Cafetería Central", "A58818501");
        await cliente.PostAsJsonAsync("/proveedores", new { Nombre = "Harinas del Norte", NifFiscal = "B44531218" });

        var cli = await cliente.GetFromJsonAsync<PagResp<TerceroResp>>("/clientes/buscar?texto=panad");
        cli!.Total.Should().Be(1);
        cli.Elementos[0].Nombre.Should().Be("Panadería López");

        var porNif = await cliente.GetFromJsonAsync<PagResp<TerceroResp>>("/clientes/buscar?texto=A58818501");
        porNif!.Total.Should().Be(1);

        var prov = await cliente.GetFromJsonAsync<PagResp<TerceroResp>>("/proveedores/buscar?texto=harinas");
        prov!.Total.Should().Be(1);
        prov.Elementos[0].Nombre.Should().Be("Harinas del Norte");
    }

    [Fact]
    public async Task Productos_filtran_por_texto_y_familia()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var fam = (await (await cliente.PostAsJsonAsync("/familias", new { Nombre = "Bebidas" })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        await cliente.PostAsJsonAsync("/productos", new { Nombre = "Agua mineral", PrecioUnitario = 1m, CodigoIva = "IVA21", FamiliaId = fam });
        await cliente.PostAsJsonAsync("/productos", new { Nombre = "Tornillo M6", PrecioUnitario = 0.2m, CodigoIva = "IVA21" });

        var porTexto = await cliente.GetFromJsonAsync<PagResp<ProductoResp>>("/productos/buscar?texto=agua");
        porTexto!.Total.Should().Be(1);
        porTexto.Elementos[0].Nombre.Should().Be("Agua mineral");

        var porFamilia = await cliente.GetFromJsonAsync<PagResp<ProductoResp>>($"/productos/buscar?familiaId={fam}");
        porFamilia!.Total.Should().Be(1);
        porFamilia.Elementos[0].FamiliaId.Should().Be(fam);
    }

    [Fact]
    public async Task El_tamano_de_pagina_se_acota_al_maximo()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        // Pedir 99999 por página se normaliza a 200 (tope de seguridad).
        var pg = await cliente.GetFromJsonAsync<PagResp<FacturaResp>>("/facturas/buscar?tamanoPagina=99999");
        pg!.TamanoPagina.Should().Be(200);
    }
}
