using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de las formas de pago: vencimiento y pago automático (documentos «ya pagados»).</summary>
public sealed class FormasPagoEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public FormasPagoEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record FormaPagoResp(Guid Id, string Nombre, bool GeneraVencimiento, int DiasVencimiento, bool RegistrarPagoAutomatico, bool Activo);
    private sealed record IdResp(Guid Id);
    private sealed record FacturaResp(Guid Id, decimal Total, string? FechaVencimiento);
    private sealed record SaldoResp(decimal Total, decimal Liquidado, decimal Pendiente, string Estado);

    private static async Task<Guid> CrearFormaAsync(HttpClient c, string nombre, bool generaVencimiento, int dias, bool registrarPago)
    {
        var resp = await c.PostAsJsonAsync("/formas-pago", new { Nombre = nombre, GeneraVencimiento = generaVencimiento, DiasVencimiento = dias, RegistrarPagoAutomatico = registrarPago });
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await resp.Content.ReadFromJsonAsync<FormaPagoResp>())!.Id;
    }

    private static async Task<Guid> ClienteAsync(HttpClient c) =>
        (await (await c.PostAsJsonAsync("/clientes", new { Nombre = "Cliente FP SL", NifFiscal = "B12345674" })).Content.ReadFromJsonAsync<IdResp>())!.Id;

    [Fact]
    public async Task Una_forma_al_contado_con_pago_automatico_deja_la_factura_pagada_sin_vencimiento_abierto()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var formaId = await CrearFormaAsync(cliente, "Contado (pagado)", generaVencimiento: false, dias: 0, registrarPago: true);
        var clienteId = await ClienteAsync(cliente);

        var factura = await (await cliente.PostAsJsonAsync("/facturas", new
        {
            ClienteId = clienteId,
            FechaEmision = "2026-08-10",
            FormaPagoId = formaId,
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = 100m, CodigoIva = "IVA21" } },
        })).Content.ReadFromJsonAsync<FacturaResp>();

        // Sin vencimiento aplazado (vence el mismo día de emisión) y ya saldada por el cobro automático.
        factura!.FechaVencimiento.Should().StartWith("2026-08-10");
        var saldo = await cliente.GetFromJsonAsync<SaldoResp>($"/facturas/{factura.Id}/saldo");
        saldo!.Pendiente.Should().Be(0m);
        saldo.Liquidado.Should().Be(factura.Total);
        saldo.Estado.Should().Be("Liquidado");
    }

    [Fact]
    public async Task Una_forma_al_contado_sin_pago_automatico_no_registra_el_cobro()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var formaId = await CrearFormaAsync(cliente, "Contado (registro manual)", generaVencimiento: false, dias: 0, registrarPago: false);
        var clienteId = await ClienteAsync(cliente);

        var factura = await (await cliente.PostAsJsonAsync("/facturas", new
        {
            ClienteId = clienteId,
            FormaPagoId = formaId,
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = 100m, CodigoIva = "IVA21" } },
        })).Content.ReadFromJsonAsync<FacturaResp>();

        // No se registró cobro: queda pendiente (el usuario lo registrará a mano si procede).
        var saldo = await cliente.GetFromJsonAsync<SaldoResp>($"/facturas/{factura!.Id}/saldo");
        saldo!.Pendiente.Should().Be(factura.Total);
        saldo.Estado.Should().Be("Pendiente");
    }

    [Fact]
    public async Task Una_forma_aplazada_fija_el_vencimiento_a_los_dias_indicados()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var formaId = await CrearFormaAsync(cliente, "Transferencia 30 días", generaVencimiento: true, dias: 30, registrarPago: false);
        var clienteId = await ClienteAsync(cliente);

        var factura = await (await cliente.PostAsJsonAsync("/facturas", new
        {
            ClienteId = clienteId,
            FechaEmision = "2026-08-10",
            FormaPagoId = formaId,
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = 100m, CodigoIva = "IVA21" } },
        })).Content.ReadFromJsonAsync<FacturaResp>();

        factura!.FechaVencimiento.Should().StartWith("2026-09-09"); // 10 ago + 30 días
        var saldo = await cliente.GetFromJsonAsync<SaldoResp>($"/facturas/{factura.Id}/saldo");
        saldo!.Pendiente.Should().Be(factura.Total);
    }

    [Fact]
    public async Task La_forma_de_pago_por_defecto_del_cliente_se_aplica_sin_indicarla()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var formaId = await CrearFormaAsync(cliente, "Contado (pagado)", generaVencimiento: false, dias: 0, registrarPago: true);
        var clienteId = (await (await cliente.PostAsJsonAsync("/clientes", new { Nombre = "Contado SL", NifFiscal = "B12345674", FormaPagoDefectoId = formaId })).Content.ReadFromJsonAsync<IdResp>())!.Id;

        var factura = await (await cliente.PostAsJsonAsync("/facturas", new
        {
            ClienteId = clienteId,
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = 50m, CodigoIva = "IVA21" } },
        })).Content.ReadFromJsonAsync<FacturaResp>();

        var saldo = await cliente.GetFromJsonAsync<SaldoResp>($"/facturas/{factura!.Id}/saldo");
        saldo!.Estado.Should().Be("Liquidado");
    }

    [Fact]
    public async Task Un_gasto_con_forma_al_contado_queda_pagado_al_registrarlo()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var formaId = await CrearFormaAsync(cliente, "Contado (pagado)", generaVencimiento: false, dias: 0, registrarPago: true);

        var gasto = await (await cliente.PostAsJsonAsync("/gastos", new
        {
            Concepto = "Material de oficina",
            BaseImponible = 100m,
            CodigoIva = "IVA21",
            FormaPagoId = formaId,
        })).Content.ReadFromJsonAsync<IdResp>();

        var saldo = await cliente.GetFromJsonAsync<SaldoResp>($"/gastos/{gasto!.Id}/saldo");
        saldo!.Pendiente.Should().Be(0m);
        saldo.Estado.Should().Be("Liquidado");
    }
}
