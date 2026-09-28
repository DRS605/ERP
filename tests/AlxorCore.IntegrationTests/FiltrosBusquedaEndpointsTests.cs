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

    private sealed record TotalesResp(int Documentos, decimal BaseImponible, decimal Impuestos, decimal Retenciones, decimal Total, decimal Pendiente, decimal Vencido, int DocumentosVencidos);
    private sealed record PagTotResp<T>(IReadOnlyList<T> Elementos, int Total, int TotalPaginas, TotalesResp Totales, Dictionary<Guid, decimal> Pendientes);

    [Fact]
    public async Task Facturas_devuelven_totales_de_todo_el_filtro_y_filtran_por_serie_y_cobro()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var acme = await CrearClienteAsync(cliente, "ACME Ibérica SL", "B12345674");
        await CrearFacturaAsync(cliente, acme, "2026-01-10", 100m);   // 121
        await CrearFacturaAsync(cliente, acme, "2026-02-10", 1000m);  // 1210
        await CrearFacturaAsync(cliente, acme, "2026-03-10", 500m);   // 605

        // Totales de las 3 facturas aunque la página tenga 1.
        var pag = await cliente.GetFromJsonAsync<PagTotResp<FacturaResp>>("/facturas/buscar?pagina=1&tamanoPagina=1");
        pag!.Elementos.Should().HaveCount(1);
        pag.TotalPaginas.Should().Be(3);
        pag.Totales.Documentos.Should().Be(3);
        pag.Totales.BaseImponible.Should().Be(1600m);
        pag.Totales.Impuestos.Should().Be(336m);
        pag.Totales.Total.Should().Be(1936m);
        pag.Totales.Pendiente.Should().Be(1936m);
        pag.Pendientes.Should().ContainKey(pag.Elementos[0].Id);

        // Cobro total de la de 1210: deja de estar pendiente.
        var todas = await cliente.GetFromJsonAsync<PagTotResp<FacturaResp>>("/facturas/buscar?importeMin=1000");
        var grande = todas!.Elementos.Single();
        (await cliente.PostAsJsonAsync("/cobros", new { FacturaId = grande.Id, Importe = 1210m, Metodo = "Transferencia" })).IsSuccessStatusCode.Should().BeTrue();

        var pendientes = await cliente.GetFromJsonAsync<PagTotResp<FacturaResp>>("/facturas/buscar?cobro=pendiente");
        pendientes!.Total.Should().Be(2);
        pendientes.Totales.Total.Should().Be(726m);
        pendientes.Totales.Pendiente.Should().Be(726m);
        var cobradas = await cliente.GetFromJsonAsync<PagTotResp<FacturaResp>>("/facturas/buscar?cobro=cobrada");
        cobradas!.Elementos.Should().ContainSingle(f => f.Id == grande.Id);
        cobradas.Totales.Pendiente.Should().Be(0m);
        var vencidas = await cliente.GetFromJsonAsync<PagTotResp<FacturaResp>>("/facturas/buscar?cobro=vencida");
        vencidas!.Total.Should().Be(2); // al contado, vencidas desde su fecha
        vencidas.Totales.DocumentosVencidos.Should().Be(2);

        // Serie (prefijo): la de por defecto las recoge todas; otra, ninguna.
        var prefijo = grande.NumeroCompleto[..grande.NumeroCompleto.IndexOf("20", StringComparison.Ordinal)];
        (await cliente.GetFromJsonAsync<PagTotResp<FacturaResp>>($"/facturas/buscar?serie={prefijo}"))!.Total.Should().Be(3);
        (await cliente.GetFromJsonAsync<PagTotResp<FacturaResp>>("/facturas/buscar?serie=ZZ"))!.Totales.Documentos.Should().Be(0);

        (await cliente.GetAsync("/facturas/buscar?cobro=raro")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Gastos_devuelven_totales_de_todo_el_filtro()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await cliente.PostAsJsonAsync("/gastos", new { Concepto = "Luz enero", BaseImponible = 100m, CodigoIva = "IVA21", Fecha = "2026-01-05" });
        await cliente.PostAsJsonAsync("/gastos", new { Concepto = "Gasolina febrero", BaseImponible = 60m, CodigoIva = "IVA21", Fecha = "2026-02-05" });

        var pag = await cliente.GetFromJsonAsync<PagTotResp<GastoResp>>("/gastos/buscar?tamanoPagina=1");
        pag!.Elementos.Should().HaveCount(1);
        pag.Totales.Documentos.Should().Be(2);
        pag.Totales.BaseImponible.Should().Be(160m);
        pag.Totales.Total.Should().Be(193.6m);
        pag.Totales.Pendiente.Should().Be(193.6m);
        (await cliente.GetFromJsonAsync<PagTotResp<GastoResp>>("/gastos/buscar?cobro=pagada"))!.Total.Should().Be(0);
        (await cliente.GetFromJsonAsync<PagTotResp<GastoResp>>("/gastos/buscar?cobro=pendiente&texto=luz"))!.Totales.Total.Should().Be(121m);
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
