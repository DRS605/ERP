using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Asientos de la tesorería (modo Completo): cobros y pagos contra caja o banco, anticipos de clientes (438) y sus
/// anulaciones con contraasiento. Con los cobros, la cuenta del cliente queda saldada en el balance.
/// </summary>
public sealed class ContabilidadTesoreriaTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public ContabilidadTesoreriaTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record FacturaResp(Guid Id, decimal Total);
    private sealed record ApunteResp(string CuentaCodigo, decimal Debe, decimal Haber);
    private sealed record AsientoResp(Guid Id, string Origen, string Concepto, List<ApunteResp> Apuntes);
    private sealed record MovimientoResp(Guid Id);
    private sealed record SaldoResp(List<MovimientoResp> Movimientos);

    private static readonly int Anio = DateTime.UtcNow.Year;

    private static async Task<HttpClient> EmpresaCompletaAsync(FabricaApiPruebas fabrica)
    {
        var (c, _) = await Ayudas.ConEmpresaAsync(fabrica);
        (await c.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();
        (await c.PutAsJsonAsync("/contabilidad/contabilizacion-automatica", new { Automatica = true })).EnsureSuccessStatusCode();
        return c;
    }

    private static async Task<List<AsientoResp>> DiarioAsync(HttpClient c) =>
        (await c.GetFromJsonAsync<List<AsientoResp>>($"/contabilidad/diario?ejercicio={Anio}"))!;

    private static decimal Debe(AsientoResp a, string cuenta) => a.Apuntes.Where(p => p.CuentaCodigo.StartsWith(cuenta, StringComparison.Ordinal)).Sum(p => p.Debe);

    private static decimal Haber(AsientoResp a, string cuenta) => a.Apuntes.Where(p => p.CuentaCodigo.StartsWith(cuenta, StringComparison.Ordinal)).Sum(p => p.Haber);

    [Fact]
    public async Task Un_cobro_va_de_banco_o_caja_a_la_cuenta_del_cliente_y_su_anulacion_lo_revierte()
    {
        var c = await EmpresaCompletaAsync(_fabrica);
        var cliente = (await (await c.PostAsJsonAsync("/clientes", new { Nombre = "Bar Central", NifFiscal = Ayudas.GenerarNif() })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        var f = (await (await c.PostAsJsonAsync("/facturas", new { ClienteId = cliente, Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = 100m, CodigoIva = "IVA21" } } }))
            .Content.ReadFromJsonAsync<FacturaResp>())!;

        (await c.PostAsJsonAsync("/cobros", new { FacturaId = f.Id, Importe = 21m, Metodo = "Efectivo" })).IsSuccessStatusCode.Should().BeTrue();
        (await c.PostAsJsonAsync("/cobros", new { FacturaId = f.Id, Importe = 100m, Metodo = "Transferencia" })).IsSuccessStatusCode.Should().BeTrue();

        var cobros = (await DiarioAsync(c)).Where(a => a.Origen == "Cobro").ToList();
        cobros.Should().HaveCount(2);
        cobros.Sum(a => Debe(a, "570")).Should().Be(21m);
        cobros.Sum(a => Debe(a, "572")).Should().Be(100m);
        cobros.Sum(a => Haber(a, "430")).Should().Be(121m);

        // La cuenta del cliente queda a cero: la factura cargó 121 y los cobros los abonaron.
        var diario = await DiarioAsync(c);
        (diario.Sum(a => Debe(a, "430")) - diario.Sum(a => Haber(a, "430"))).Should().Be(0m);

        // Anular el cobro en efectivo: contraasiento (430 al debe, 570 al haber).
        var efectivo = (await c.GetFromJsonAsync<SaldoResp>($"/facturas/{f.Id}/saldo"))!.Movimientos.First();
        (await c.PostAsJsonAsync($"/tesoreria/movimientos/{efectivo.Id}/anular", new { })).StatusCode.Should().Be(HttpStatusCode.OK);
        var contra = (await DiarioAsync(c)).Where(a => a.Origen == "Cobro" && a.Concepto.StartsWith("Anulación", StringComparison.Ordinal)).Single();
        Debe(contra, "430").Should().Be(21m);
        Haber(contra, "570").Should().Be(21m);
    }

    [Fact]
    public async Task Un_pago_va_de_la_cuenta_del_proveedor_al_banco()
    {
        var c = await EmpresaCompletaAsync(_fabrica);
        var proveedor = (await (await c.PostAsJsonAsync("/proveedores", new { Nombre = "Suministros", NifFiscal = Ayudas.GenerarNif() })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        var gasto = (await (await c.PostAsJsonAsync("/gastos", new { Concepto = "Material", BaseImponible = 100m, CodigoIva = "IVA21", ProveedorId = proveedor }))
            .Content.ReadFromJsonAsync<IdResp>())!.Id;
        (await c.PostAsJsonAsync("/pagos", new { GastoId = gasto, Importe = 121m, Metodo = "Transferencia" })).IsSuccessStatusCode.Should().BeTrue();

        var pago = (await DiarioAsync(c)).Single(a => a.Origen == "Pago");
        Debe(pago, "400").Should().Be(121m);
        Haber(pago, "572").Should().Be(121m);
    }

    [Fact]
    public async Task Un_anticipo_va_a_438_y_al_aplicarse_pasa_a_la_cuenta_del_cliente()
    {
        var c = await EmpresaCompletaAsync(_fabrica);
        var cliente = (await (await c.PostAsJsonAsync("/clientes", new { Nombre = "Cliente con anticipo", NifFiscal = Ayudas.GenerarNif() })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        var anticipo = (await (await c.PostAsJsonAsync("/anticipos", new { ClienteId = cliente, Importe = 500m, Metodo = "Transferencia" })).Content.ReadFromJsonAsync<IdResp>())!.Id;

        var alta = (await DiarioAsync(c)).Single(a => a.Origen == "Cobro");
        Debe(alta, "572").Should().Be(500m);
        Haber(alta, "438").Should().Be(500m);

        var f = (await (await c.PostAsJsonAsync("/facturas", new { ClienteId = cliente, Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = 100m, CodigoIva = "IVA21" } } }))
            .Content.ReadFromJsonAsync<FacturaResp>())!;
        (await c.PostAsJsonAsync($"/anticipos/{anticipo}/aplicar", new { FacturaId = f.Id })).IsSuccessStatusCode.Should().BeTrue();

        var aplicacion = (await DiarioAsync(c)).Where(a => a.Origen == "Cobro").Single(a => a.Id != alta.Id);
        Debe(aplicacion, "438").Should().Be(121m);
        Haber(aplicacion, "430").Should().Be(121m);
        Debe(aplicacion, "57").Should().Be(0m, "la aplicación no mueve dinero");
    }

    [Fact]
    public async Task En_modo_simple_no_hay_asientos_de_tesoreria()
    {
        var (c, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var cliente = (await (await c.PostAsJsonAsync("/clientes", new { Nombre = "Simple", NifFiscal = Ayudas.GenerarNif() })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        var f = (await (await c.PostAsJsonAsync("/facturas", new { ClienteId = cliente, Lineas = new[] { new { Cantidad = 1m, Descripcion = "X", PrecioUnitario = 10m, CodigoIva = "IVA21" } } }))
            .Content.ReadFromJsonAsync<FacturaResp>())!;
        (await c.PostAsJsonAsync("/cobros", new { FacturaId = f.Id, Importe = 12.1m })).IsSuccessStatusCode.Should().BeTrue();
        (await DiarioAsync(c)).Should().BeEmpty();
    }
}
