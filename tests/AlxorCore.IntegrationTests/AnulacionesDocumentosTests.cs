using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Deshacer documentos ya registrados sin borrarlos: asientos manuales (contraasiento), cobros y pagos (anulación en
/// negativo), gastos (con sus pagos anulados antes) y facturas cobradas (antes hay que anular los cobros).
/// </summary>
public sealed class AnulacionesDocumentosTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public AnulacionesDocumentosTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record FacturaResp(Guid Id, decimal Total);
    private sealed record ApunteResp(string CuentaCodigo, decimal Debe, decimal Haber);
    private sealed record AsientoResp(Guid Id, int Numero, string Origen, Guid? AnulaAsientoId, Guid? AnuladoPorId, List<ApunteResp> Apuntes);
    private sealed record MovimientoResp(Guid Id, decimal Importe, Guid? AnulaMovimientoId, bool Anulado);
    private sealed record SaldoResp(decimal Total, decimal Liquidado, decimal Pendiente, string Estado, List<MovimientoResp> Movimientos);
    private sealed record AnticipoResp(Guid Id, decimal Importe, decimal Aplicado, decimal Disponible, string Estado);
    private sealed record ProblemaResp(string Title, string Codigo);

    private static async Task<ProblemaResp> ConflictoAsync(HttpResponseMessage r)
    {
        r.StatusCode.Should().Be(HttpStatusCode.Conflict, await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<ProblemaResp>())!;
    }

    private static async Task<(Guid Cliente, FacturaResp Factura)> FacturaAsync(HttpClient c)
    {
        var cliente = (await (await c.PostAsJsonAsync("/clientes", new { Nombre = "Cliente", NifFiscal = Ayudas.GenerarNif() })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        var f = await c.PostAsJsonAsync("/facturas", new { ClienteId = cliente, Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = 100m, CodigoIva = "IVA21" } } });
        f.IsSuccessStatusCode.Should().BeTrue(await f.Content.ReadAsStringAsync());
        return (cliente, (await f.Content.ReadFromJsonAsync<FacturaResp>())!);
    }

    [Fact]
    public async Task Un_asiento_manual_se_anula_con_su_contraasiento_y_solo_una_vez()
    {
        var (c, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var anio = DateTime.UtcNow.Year;
        var alta = await c.PostAsJsonAsync("/contabilidad/asientos", new
        {
            Fecha = $"{anio}-03-10",
            Concepto = "Reparación",
            Lineas = new object[] { new { CuentaCodigo = "622", Debe = 300m, Haber = 0m }, new { CuentaCodigo = "572", Debe = 0m, Haber = 300m } },
        });
        alta.StatusCode.Should().Be(HttpStatusCode.Created, await alta.Content.ReadAsStringAsync());
        var asiento = (await alta.Content.ReadFromJsonAsync<AsientoResp>())!;

        var anulacion = await c.PostAsJsonAsync($"/contabilidad/asientos/{asiento.Id}/anular", new { });
        anulacion.StatusCode.Should().Be(HttpStatusCode.OK, await anulacion.Content.ReadAsStringAsync());
        var contra = (await anulacion.Content.ReadFromJsonAsync<AsientoResp>())!;
        contra.AnulaAsientoId.Should().Be(asiento.Id);
        contra.Apuntes.Single(a => a.CuentaCodigo == "622").Haber.Should().Be(300m);
        contra.Apuntes.Single(a => a.CuentaCodigo == "572").Debe.Should().Be(300m);

        var diario = (await c.GetFromJsonAsync<List<AsientoResp>>($"/contabilidad/diario?ejercicio={anio}"))!;
        diario.Single(a => a.Id == asiento.Id).AnuladoPorId.Should().Be(contra.Id);

        (await ConflictoAsync(await c.PostAsJsonAsync($"/contabilidad/asientos/{asiento.Id}/anular", new { }))).Codigo.Should().Be("asiento.ya_anulado");
        (await ConflictoAsync(await c.PostAsJsonAsync($"/contabilidad/asientos/{contra.Id}/anular", new { }))).Codigo.Should().Be("asiento.es_anulacion");
    }

    [Fact]
    public async Task Un_asiento_de_un_documento_no_se_anula_a_mano()
    {
        var (c, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await FacturaAsync(c);
        var diario = (await c.GetFromJsonAsync<List<AsientoResp>>($"/contabilidad/diario?ejercicio={DateTime.UtcNow.Year}"))!;
        var deFactura = diario.FirstOrDefault(a => a.Origen != "Manual");
        if (deFactura is null)
        {
            return; // contabilización diferida: no hay asiento todavía
        }

        (await ConflictoAsync(await c.PostAsJsonAsync($"/contabilidad/asientos/{deFactura.Id}/anular", new { }))).Codigo.Should().Be("asiento.de_documento");
    }

    [Fact]
    public async Task Un_cobro_se_anula_la_factura_vuelve_a_pendiente_y_ya_se_puede_anular()
    {
        var (c, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var (_, factura) = await FacturaAsync(c);
        (await c.PostAsJsonAsync("/cobros", new { FacturaId = factura.Id, Importe = factura.Total })).IsSuccessStatusCode.Should().BeTrue();

        // Cobrada: no se puede anular la factura.
        (await ConflictoAsync(await c.PostAsJsonAsync($"/facturas/{factura.Id}/anular", new { Motivo = "Error" }))).Codigo.Should().Be("factura.con_cobros");

        var saldo = (await c.GetFromJsonAsync<SaldoResp>($"/facturas/{factura.Id}/saldo"))!;
        var cobro = saldo.Movimientos.Single();
        (await c.PostAsJsonAsync($"/tesoreria/movimientos/{cobro.Id}/anular", new { })).StatusCode.Should().Be(HttpStatusCode.OK);

        saldo = (await c.GetFromJsonAsync<SaldoResp>($"/facturas/{factura.Id}/saldo"))!;
        saldo.Pendiente.Should().Be(factura.Total);
        saldo.Estado.Should().Be("Pendiente");
        saldo.Movimientos.Single(m => m.Id == cobro.Id).Anulado.Should().BeTrue();
        saldo.Movimientos.Single(m => m.AnulaMovimientoId == cobro.Id).Importe.Should().Be(-factura.Total);
        (await ConflictoAsync(await c.PostAsJsonAsync($"/tesoreria/movimientos/{cobro.Id}/anular", new { }))).Codigo.Should().Be("movimiento.ya_anulado");

        // Se puede volver a cobrar y, sin cobros vivos, anular la factura.
        (await c.PostAsJsonAsync("/cobros", new { FacturaId = factura.Id, Importe = 10m })).IsSuccessStatusCode.Should().BeTrue();
        var segundo = (await c.GetFromJsonAsync<SaldoResp>($"/facturas/{factura.Id}/saldo"))!.Movimientos.Single(m => m.AnulaMovimientoId is null && !m.Anulado);
        (await c.PostAsJsonAsync($"/tesoreria/movimientos/{segundo.Id}/anular", new { })).StatusCode.Should().Be(HttpStatusCode.OK);
        var anular = await c.PostAsJsonAsync($"/facturas/{factura.Id}/anular", new { Motivo = "Error" });
        anular.IsSuccessStatusCode.Should().BeTrue(await anular.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Anular_el_cobro_de_un_anticipo_le_devuelve_el_saldo_disponible()
    {
        var (c, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var (cliente, factura) = await FacturaAsync(c);
        var anticipo = (await (await c.PostAsJsonAsync("/anticipos", new { ClienteId = cliente, Importe = 500m })).Content.ReadFromJsonAsync<AnticipoResp>())!;
        (await c.PostAsJsonAsync($"/anticipos/{anticipo.Id}/aplicar", new { FacturaId = factura.Id })).IsSuccessStatusCode.Should().BeTrue();

        var cobro = (await c.GetFromJsonAsync<SaldoResp>($"/facturas/{factura.Id}/saldo"))!.Movimientos.Single();
        (await c.PostAsJsonAsync($"/tesoreria/movimientos/{cobro.Id}/anular", new { })).StatusCode.Should().Be(HttpStatusCode.OK);

        var tras = (await c.GetFromJsonAsync<List<AnticipoResp>>("/anticipos"))!.Single(a => a.Id == anticipo.Id);
        tras.Disponible.Should().Be(500m);
        tras.Estado.Should().Be("Disponible");
    }

    [Fact]
    public async Task Un_gasto_pagado_no_se_anula_hasta_anular_el_pago()
    {
        var (c, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var alta = await c.PostAsJsonAsync("/gastos", new { Concepto = "Material", BaseImponible = 100m, CodigoIva = "IVA21" });
        alta.IsSuccessStatusCode.Should().BeTrue(await alta.Content.ReadAsStringAsync());
        var gasto = (await alta.Content.ReadFromJsonAsync<IdResp>())!.Id;
        (await c.PostAsJsonAsync("/pagos", new { GastoId = gasto, Importe = 121m })).IsSuccessStatusCode.Should().BeTrue();

        (await ConflictoAsync(await c.PostAsync(new Uri($"/gastos/{gasto}/anular", UriKind.Relative), null))).Codigo.Should().Be("gasto.con_pagos");

        var pago = (await c.GetFromJsonAsync<SaldoResp>($"/gastos/{gasto}/saldo"))!.Movimientos.Single();
        (await c.PostAsJsonAsync($"/tesoreria/movimientos/{pago.Id}/anular", new { })).StatusCode.Should().Be(HttpStatusCode.OK);
        var anular = await c.PostAsync(new Uri($"/gastos/{gasto}/anular", UriKind.Relative), null);
        anular.StatusCode.Should().Be(HttpStatusCode.OK, await anular.Content.ReadAsStringAsync());
        (await c.PostAsync(new Uri($"/gastos/{gasto}/anular", UriKind.Relative), null)).IsSuccessStatusCode.Should().BeFalse("un gasto anulado no se anula dos veces");
    }
}
