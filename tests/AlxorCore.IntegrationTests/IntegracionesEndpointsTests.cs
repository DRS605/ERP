using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de integración de la API pública (claves de API) y de los webhooks salientes.</summary>
public sealed class IntegracionesEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public IntegracionesEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record ClaveCreada(Guid Id, string Nombre, string Prefijo, string Secreto);
    private sealed record SuscripcionCreada(Guid Id, string Url, string[] Eventos, string Secreto);
    private sealed record EntregaResp(Guid Id, string Evento, string Estado, int Intentos);
    private sealed record ClienteResp(Guid Id);
    private sealed record ProcesarResp(int Entregadas);

    [Fact]
    public async Task La_clave_de_api_autentica_la_api_publica_y_la_revocacion_la_invalida()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);

        var crear = await cli.PostAsJsonAsync("/integraciones/claves", new { Nombre = "Integración Tuday" });
        crear.StatusCode.Should().Be(HttpStatusCode.Created);
        var clave = (await crear.Content.ReadFromJsonAsync<ClaveCreada>())!;
        clave.Secreto.Should().StartWith("ak_");

        // Cliente sin JWT, autenticado solo por la clave de API.
        var publico = _fabrica.CreateClient();
        publico.DefaultRequestHeaders.Add("X-Api-Key", clave.Secreto);

        var ping = await publico.GetAsync(new Uri("/api/v1/ping", UriKind.Relative));
        ping.StatusCode.Should().Be(HttpStatusCode.OK);

        (await publico.GetAsync(new Uri("/api/v1/facturas", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.OK);
        (await publico.GetAsync(new Uri("/api/v1/clientes", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.OK);

        // Revocar la clave la deja inservible.
        (await cli.DeleteAsync(new Uri($"/integraciones/claves/{clave.Id}", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await publico.GetAsync(new Uri("/api/v1/facturas", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task La_api_publica_sin_clave_da_401()
    {
        var publico = _fabrica.CreateClient();
        (await publico.GetAsync(new Uri("/api/v1/facturas", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        publico.DefaultRequestHeaders.Add("X-Api-Key", "ak_claveinventada");
        (await publico.GetAsync(new Uri("/api/v1/facturas", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Emitir_una_factura_encola_y_entrega_el_webhook()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);

        var sus = await cli.PostAsJsonAsync("/integraciones/webhooks", new { Url = "https://ejemplo.test/hook", Eventos = new[] { "factura.emitida" } });
        sus.StatusCode.Should().Be(HttpStatusCode.Created);
        (await sus.Content.ReadFromJsonAsync<SuscripcionCreada>())!.Secreto.Should().StartWith("whsec_");

        // Emitir una factura dispara el evento factura.emitida.
        var clienteId = (await (await cli.PostAsJsonAsync("/clientes", new { Nombre = "Cliente Webhook", NifFiscal = "12345678Z" })).Content.ReadFromJsonAsync<ClienteResp>())!.Id;
        var factura = await cli.PostAsJsonAsync("/facturas", new
        {
            ClienteId = clienteId,
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = 200m, CodigoIva = "IVA21" } },
        });
        factura.StatusCode.Should().Be(HttpStatusCode.Created);

        // Se ha encolado una entrega pendiente para el evento.
        var pendientes = await cli.GetFromJsonAsync<List<EntregaResp>>("/integraciones/webhooks/entregas");
        pendientes!.Should().Contain(e => e.Evento == "factura.emitida" && e.Estado == "Pendiente");

        // Forzar el envío (cliente HTTP falso → éxito) marca la entrega como entregada.
        var procesar = await cli.PostAsync(new Uri("/integraciones/webhooks/procesar", UriKind.Relative), content: null);
        (await procesar.Content.ReadFromJsonAsync<ProcesarResp>())!.Entregadas.Should().BeGreaterThanOrEqualTo(1);

        var tras = await cli.GetFromJsonAsync<List<EntregaResp>>("/integraciones/webhooks/entregas");
        tras!.Should().Contain(e => e.Evento == "factura.emitida" && e.Estado == "Entregada");
    }

    [Fact]
    public async Task Rechaza_suscribir_un_evento_desconocido()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var r = await cli.PostAsJsonAsync("/integraciones/webhooks", new { Url = "https://ejemplo.test/hook", Eventos = new[] { "no.existe" } });
        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
