using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Seguro de crédito: clasificación reducida, aviso (o bloqueo) al vender sin cobertura, cartera asegurada frente al
/// riesgo y avisos de impago dentro del plazo de la póliza.
/// </summary>
public sealed class SeguroCreditoTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public SeguroCreditoTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.UtcNow);

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo, string? Title);
    private sealed record FacturaResp(Guid Id, decimal Total, string? AvisoRiesgo);
    private sealed record ClasificacionResp(Guid Id, Guid ClienteId, string Estado, decimal Solicitado, decimal ConcedidoHoy, decimal Riesgo, decimal SinCobertura,
        decimal Indemnizable, int AvisosAbiertos, List<object> Historial);
    private sealed record SituacionResp(decimal Riesgo, decimal SinCobertura, int ClientesSinCobertura, List<ClasificacionResp> Clasificaciones);
    private sealed record ImpagoResp(Guid FacturaId, int DiasParaAvisar, string Situacion, DateOnly LimiteAviso);
    private sealed record AvisoResp(Guid Id, string Estado, decimal Importe, decimal? Indemnizacion);

    private static async Task<T> OkAsync<T>(Task<HttpResponseMessage> peticion)
    {
        var r = await peticion;
        r.StatusCode.Should().BeOneOf([HttpStatusCode.OK, HttpStatusCode.Created], await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<T>())!;
    }

    private static async Task<string> FalloAsync(Task<HttpResponseMessage> peticion, HttpStatusCode esperado)
    {
        var r = await peticion;
        r.StatusCode.Should().Be(esperado, await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo;
    }

    private static Task<HttpResponseMessage> Factura(HttpClient api, Guid cliente, decimal base_, int haceDias = 0) =>
        api.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente, FechaEmision = Hoy.AddDays(-haceDias), DiasVencimiento = 0,
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Fruta", PrecioUnitario = base_, CodigoIva = "IVA21" } },
        });

    [Fact]
    public async Task Clasificacion_avisos_al_vender_cartera_asegurada_y_avisos_de_impago()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var asegurado = (await OkAsync<IdResp>(api.PostAsJsonAsync("/clientes", new { Nombre = "Mayorista Asegurado SL", NifFiscal = Ayudas.GenerarNif() }))).Id;
        var sinClasificar = (await OkAsync<IdResp>(api.PostAsJsonAsync("/clientes", new { Nombre = "Nuevo Sin Clasificar SL", NifFiscal = Ayudas.GenerarNif() }))).Id;

        // Facturas antiguas, antes de tener póliza: sin avisos.
        var vieja = await OkAsync<FacturaResp>(Factura(api, asegurado, 100m, 70));
        vieja.AvisoRiesgo.Should().BeNull();
        var reciente = await OkAsync<FacturaResp>(Factura(api, asegurado, 100m, 50));

        var poliza = (await OkAsync<IdResp>(api.PostAsJsonAsync("/seguro-credito/polizas", new
        {
            Aseguradora = "Crédito y Caución", NumeroPoliza = "P-123", PorcentajeCobertura = 85m, PlazoAvisoDias = 60, Desde = Hoy.AddDays(-365),
        }))).Id;
        (await FalloAsync(api.PostAsJsonAsync("/seguro-credito/polizas", new { Aseguradora = "X", PorcentajeCobertura = 120m, PlazoAvisoDias = 60 }), HttpStatusCode.BadRequest))
            .Should().Be("seguro.cobertura");

        var clasificacion = await OkAsync<ClasificacionResp>(api.PostAsJsonAsync("/seguro-credito/clasificaciones", new { PolizaId = poliza, ClienteId = asegurado, Solicitado = 1000m, Fecha = Hoy.AddDays(-40) }));
        clasificacion.Estado.Should().Be("Solicitada");
        (await FalloAsync(api.PostAsJsonAsync("/seguro-credito/clasificaciones", new { PolizaId = poliza, ClienteId = asegurado, Solicitado = 500m }), HttpStatusCode.Conflict))
            .Should().Be("seguro.clasificacion_duplicada");
        var concedida = await OkAsync<ClasificacionResp>(api.PostAsJsonAsync($"/seguro-credito/clasificaciones/{clasificacion.Id}/comunicacion",
            new { Estado = "Concedida", Concedido = 600m, Efecto = Hoy.AddDays(-30) }));
        concedida.Estado.Should().Be("Reducida", "se concede menos de lo solicitado");
        concedida.ConcedidoHoy.Should().Be(600m);
        concedida.Historial.Should().HaveCount(2);

        // Riesgo 242 € + 121 € cabe en los 600 €; 363 + 484 = 847 €, no: 247 € sin cobertura.
        (await OkAsync<FacturaResp>(Factura(api, asegurado, 100m))).AvisoRiesgo.Should().BeNull();
        var excede = await OkAsync<FacturaResp>(Factura(api, asegurado, 400m));
        excede.AvisoRiesgo.Should().Contain("clasificación del seguro").And.Contain("247,00 €");
        (await OkAsync<FacturaResp>(Factura(api, sinClasificar, 50m))).AvisoRiesgo.Should().Contain("no tiene clasificación vigente");

        var situacion = await OkAsync<SituacionResp>(api.GetAsync("/seguro-credito/clasificaciones"));
        var fila = situacion.Clasificaciones.Single(c => c.ClienteId == asegurado);
        fila.Riesgo.Should().Be(847m);
        fila.SinCobertura.Should().Be(247m);
        fila.Indemnizable.Should().Be(510m);
        situacion.ClientesSinCobertura.Should().Be(1);

        // Impagos asegurados: vencida hace 70 días (plazo de 60: fuera) y hace 50 (quedan 10: avisar ya).
        var impagos = await OkAsync<List<ImpagoResp>>(api.GetAsync("/seguro-credito/impagos"));
        impagos.Single(i => i.FacturaId == vieja.Id).Situacion.Should().Be("FueraDePlazo");
        impagos.Single(i => i.FacturaId == reciente.Id).Should().Match<ImpagoResp>(i => i.Situacion == "AvisarYa" && i.DiasParaAvisar == 10);

        var aviso = await OkAsync<AvisoResp>(api.PostAsJsonAsync("/seguro-credito/avisos", new { FacturaId = reciente.Id, Referencia = "SIN-2026-77" }));
        aviso.Importe.Should().Be(121m);
        (await OkAsync<List<ImpagoResp>>(api.GetAsync("/seguro-credito/impagos"))).Should().NotContain(i => i.FacturaId == reciente.Id);
        (await FalloAsync(api.PostAsJsonAsync("/seguro-credito/avisos", new { FacturaId = reciente.Id }), HttpStatusCode.BadRequest)).Should().Be("seguro.aviso_sin_impago");
        (await FalloAsync(api.PostAsJsonAsync($"/seguro-credito/avisos/{aviso.Id}/cierre", new { Estado = "Indemnizado" }), HttpStatusCode.BadRequest))
            .Should().Be("seguro.indemnizacion");
        (await OkAsync<AvisoResp>(api.PostAsJsonAsync($"/seguro-credito/avisos/{aviso.Id}/cierre", new { Estado = "Indemnizado", Indemnizacion = 102.85m })))
            .Should().Match<AvisoResp>(a => a.Estado == "Indemnizado" && a.Indemnizacion == 102.85m);
        (await FalloAsync(api.PostAsJsonAsync($"/seguro-credito/avisos/{aviso.Id}/cierre", new { Estado = "Retirado" }), HttpStatusCode.Conflict)).Should().Be("seguro.aviso_cerrado");

        // La póliza pasa a bloquear la venta sin cobertura; la aseguradora anula la clasificación.
        (await api.PutAsJsonAsync($"/seguro-credito/polizas/{poliza}", new
        {
            Aseguradora = "Crédito y Caución", NumeroPoliza = "P-123", PorcentajeCobertura = 85m, PlazoAvisoDias = 60, Desde = Hoy.AddDays(-365), BloquearSinCobertura = true,
        })).EnsureSuccessStatusCode();
        (await FalloAsync(Factura(api, sinClasificar, 50m), HttpStatusCode.Conflict)).Should().Be("seguro.sin_cobertura");
        (await api.PostAsJsonAsync($"/seguro-credito/clasificaciones/{clasificacion.Id}/comunicacion", new { Estado = "Anulada" })).EnsureSuccessStatusCode();
        (await FalloAsync(Factura(api, asegurado, 10m), HttpStatusCode.Conflict)).Should().Be("seguro.sin_cobertura");
        (await FalloAsync(api.PostAsJsonAsync($"/seguro-credito/clasificaciones/{clasificacion.Id}/comunicacion", new { Estado = "Concedida", Concedido = 100m, Efecto = Hoy.AddDays(-100) }),
            HttpStatusCode.BadRequest)).Should().Be("seguro.efecto_anterior");
    }
}
