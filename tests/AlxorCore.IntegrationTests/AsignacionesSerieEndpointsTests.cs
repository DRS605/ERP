using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de integración de las series asignables (empresa/cliente) y su resolución al emitir.</summary>
public sealed class AsignacionesSerieEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;
    public AsignacionesSerieEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record FacturaResp(Guid Id, string NumeroCompleto);
    private sealed record PedidoResp(Guid Id, string NumeroCompleto);
    private sealed record AsignacionResp(Guid Id, string TipoDocumento, string Ambito, Guid? TerceroId, string Prefijo);

    private static async Task<string> EmitirYNumero(HttpClient cliente, Guid clienteId)
    {
        var comando = new { ClienteId = clienteId, Lineas = new[] { new { Cantidad = 1m, Descripcion = "X", PrecioUnitario = 100m, CodigoIva = "IVA21" } } };
        var f = (await (await cliente.PostAsJsonAsync("/facturas", comando)).Content.ReadFromJsonAsync<FacturaResp>())!;
        return f.NumeroCompleto;
    }

    [Fact]
    public async Task La_serie_del_cliente_gana_a_la_de_la_empresa_al_emitir()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var cli1 = (await (await cliente.PostAsJsonAsync("/clientes", new { Nombre = "Cliente Especial", NifFiscal = "B12345674" })).Content.ReadFromJsonAsync<IdResp>())!;
        var cli2 = (await (await cliente.PostAsJsonAsync("/clientes", new { Nombre = "Cliente Normal", NifFiscal = "A11111111" })).Content.ReadFromJsonAsync<IdResp>())!;

        // Serie por defecto de la empresa para facturas: «EMP». Serie específica del cliente 1: «CLI».
        (await cliente.PostAsJsonAsync("/series/asignaciones", new { TipoDocumento = "Factura", Ambito = "Empresa", Prefijo = "EMP" })).StatusCode.Should().Be(HttpStatusCode.Created);
        (await cliente.PostAsJsonAsync("/series/asignaciones", new { TipoDocumento = "Factura", Ambito = "Cliente", TerceroId = cli1.Id, Prefijo = "CLI" })).StatusCode.Should().Be(HttpStatusCode.Created);

        // El cliente 1 usa su serie; el cliente 2 (sin asignación) usa la de la empresa.
        (await EmitirYNumero(cliente, cli1.Id)).Should().StartWith("CLI");
        (await EmitirYNumero(cliente, cli2.Id)).Should().StartWith("EMP");

        var lista = await cliente.GetFromJsonAsync<List<AsignacionResp>>("/series/asignaciones");
        lista!.Should().HaveCount(2);
    }

    [Fact]
    public async Task La_serie_del_proveedor_prefija_el_numero_del_pedido_de_compra()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var prov = (await (await cliente.PostAsJsonAsync("/proveedores", new { Nombre = "Suministros Ebro SL" })).Content.ReadFromJsonAsync<IdResp>())!;
        (await cliente.PostAsJsonAsync("/series/asignaciones", new { TipoDocumento = "PedidoCompra", Ambito = "Proveedor", TerceroId = prov.Id, Prefijo = "PC" })).StatusCode.Should().Be(HttpStatusCode.Created);

        var pedido = (await (await cliente.PostAsJsonAsync("/compras/pedidos", new
        {
            ProveedorId = prov.Id,
            Lineas = new[] { new { Descripcion = "Tornillos", Cantidad = 10m, PrecioUnitario = 2m } },
        })).Content.ReadFromJsonAsync<PedidoResp>())!;

        pedido.NumeroCompleto.Should().StartWith("PC");
    }

    [Fact]
    public async Task No_se_asigna_dos_veces_la_misma_serie_por_ambito()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await cliente.PostAsJsonAsync("/series/asignaciones", new { TipoDocumento = "Factura", Ambito = "Empresa", Prefijo = "EMP" })).StatusCode.Should().Be(HttpStatusCode.Created);
        (await cliente.PostAsJsonAsync("/series/asignaciones", new { TipoDocumento = "Factura", Ambito = "Empresa", Prefijo = "OTRA" })).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Se_puede_eliminar_una_asignacion()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var a = (await (await cliente.PostAsJsonAsync("/series/asignaciones", new { TipoDocumento = "Factura", Ambito = "Empresa", Prefijo = "EMP" })).Content.ReadFromJsonAsync<AsignacionResp>())!;
        (await cliente.DeleteAsync($"/series/asignaciones/{a.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var lista = await cliente.GetFromJsonAsync<List<AsignacionResp>>("/series/asignaciones");
        lista!.Should().BeEmpty();
    }
}
