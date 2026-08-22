using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas del «código siguiente» de subcuenta de clientes, proveedores y trabajadores.</summary>
public sealed class SubcuentasTercerosEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public SubcuentasTercerosEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record SubcuentaResp(string Tipo, Guid TerceroId, string CuentaCodigo, bool Individual);
    private sealed record IdResp(Guid Id);
    private sealed record ApunteResp(string CuentaCodigo, decimal Debe, decimal Haber);
    private sealed record AsientoResp(string Origen, List<ApunteResp> Apuntes);

    private static async Task ModoAsync(HttpClient c, string modo) =>
        (await c.PutAsJsonAsync("/contabilidad/modo", new { Modo = modo })).StatusCode.Should().Be(HttpStatusCode.OK);

    [Fact]
    public async Task En_modo_simple_los_terceros_comparten_la_raiz()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        // Por defecto el modo es Simple.
        var s = await cliente.GetFromJsonAsync<SubcuentaResp>("/contabilidad/subcuenta-siguiente?tipo=Cliente");
        s!.CuentaCodigo.Should().Be("430");
        s.Individual.Should().BeFalse();
    }

    [Fact]
    public async Task En_modo_completo_autonumera_la_subcuenta_por_tipo()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await ModoAsync(cliente, "Completo");

        (await cliente.GetFromJsonAsync<SubcuentaResp>("/contabilidad/subcuenta-siguiente?tipo=Cliente"))!.CuentaCodigo.Should().Be("43000001");
        (await cliente.GetFromJsonAsync<SubcuentaResp>("/contabilidad/subcuenta-siguiente?tipo=Proveedor"))!.CuentaCodigo.Should().Be("40000001");
        (await cliente.GetFromJsonAsync<SubcuentaResp>("/contabilidad/subcuenta-siguiente?tipo=Trabajador"))!.CuentaCodigo.Should().Be("46500001");
    }

    [Fact]
    public async Task Asignar_subcuentas_incrementa_el_codigo_y_se_puede_consultar()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await ModoAsync(cliente, "Completo");
        var t1 = Guid.NewGuid();
        var t2 = Guid.NewGuid();

        var a1 = await (await cliente.PutAsJsonAsync("/contabilidad/subcuenta", new { Tipo = "Cliente", TerceroId = t1, Nombre = "Cliente Uno" })).Content.ReadFromJsonAsync<SubcuentaResp>();
        a1!.CuentaCodigo.Should().Be("43000001");
        var a2 = await (await cliente.PutAsJsonAsync("/contabilidad/subcuenta", new { Tipo = "Cliente", TerceroId = t2, Nombre = "Cliente Dos" })).Content.ReadFromJsonAsync<SubcuentaResp>();
        a2!.CuentaCodigo.Should().Be("43000002");

        var consulta = await cliente.GetFromJsonAsync<SubcuentaResp>($"/contabilidad/subcuenta?tipo=Cliente&terceroId={t1}");
        consulta!.CuentaCodigo.Should().Be("43000001");
        consulta.Individual.Should().BeTrue();
    }

    [Fact]
    public async Task Se_puede_indicar_una_cuenta_manual_que_no_empieza_por_la_raiz()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await ModoAsync(cliente, "Completo");
        var t = Guid.NewGuid();

        var r = await (await cliente.PutAsJsonAsync("/contabilidad/subcuenta", new { Tipo = "Cliente", TerceroId = t, Nombre = "Especial", CuentaPreferida = "44000009" })).Content.ReadFromJsonAsync<SubcuentaResp>();
        r!.CuentaCodigo.Should().Be("44000009");
        r.Individual.Should().BeTrue();
    }

    [Fact]
    public async Task La_longitud_de_subcuenta_es_configurable()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await ModoAsync(cliente, "Completo");
        (await cliente.PutAsJsonAsync("/contabilidad/longitud-subcuenta", new { Longitud = 7 })).StatusCode.Should().Be(HttpStatusCode.OK);

        (await cliente.GetFromJsonAsync<SubcuentaResp>("/contabilidad/subcuenta-siguiente?tipo=Proveedor"))!.CuentaCodigo.Should().Be("4000001");
    }

    [Fact]
    public async Task La_venta_se_contabiliza_contra_la_subcuenta_del_cliente()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await ModoAsync(cliente, "Completo");

        var clienteId = (await (await cliente.PostAsJsonAsync("/clientes", new { Nombre = "Cliente Venta SL", NifFiscal = "B12345674" })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        // Asigna su subcuenta individual (código siguiente).
        var sub = await (await cliente.PutAsJsonAsync("/contabilidad/subcuenta", new { Tipo = "Cliente", TerceroId = clienteId, Nombre = "Cliente Venta SL" })).Content.ReadFromJsonAsync<SubcuentaResp>();
        sub!.CuentaCodigo.Should().Be("43000001");

        await cliente.PostAsJsonAsync("/facturas", new
        {
            ClienteId = clienteId,
            FechaEmision = "2026-08-10",
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = 1000m, CodigoIva = "IVA21" } },
        });

        var pendientes = await cliente.GetFromJsonAsync<List<System.Text.Json.JsonElement>>("/contabilidad/pendientes");
        var ventaId = pendientes!.First(p => p.GetProperty("sentido").GetString() == "Venta").GetProperty("id").GetGuid();
        (await cliente.PostAsJsonAsync("/contabilidad/pendientes/contabilizar", new { Ids = new[] { ventaId } })).StatusCode.Should().Be(HttpStatusCode.OK);

        var diario = await cliente.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2026");
        var asiento = diario!.Single(a => a.Origen == "Venta");
        // Usa la subcuenta del cliente (43000001), NO la raíz genérica 430.
        asiento.Apuntes.Should().Contain(x => x.CuentaCodigo == "43000001" && x.Debe == 1210m);
        asiento.Apuntes.Should().NotContain(x => x.CuentaCodigo == "430");
    }
}
