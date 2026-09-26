using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de cobranza: anticipos de clientes y reclamación de impagados.</summary>
public sealed class CobranzaEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public CobranzaEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record FacturaResp(Guid Id, string NumeroCompleto, decimal Total);
    private sealed record SaldoResp(decimal Total, decimal Liquidado, decimal Pendiente, string Estado);
    private sealed record AnticipoResp(Guid Id, decimal Importe, decimal Aplicado, decimal Disponible, string Estado);
    private sealed record ImpagadoResp(Guid FacturaId, int DiasRetraso, decimal Pendiente, int? NivelQueToca, int? UltimoNivelReclamado, bool PendienteDeReclamar);
    private sealed record ReclamacionResp(int Nivel, string Asunto, string Texto);
    private sealed record ReclamacionEnviadaResp(ReclamacionResp Reclamacion, bool CorreoEnviado, string? MotivoNoEnviado);
    private sealed record ProblemaResp(string Title, string Codigo);

    private static DateOnly Hoy => DateOnly.FromDateTime(DateTime.UtcNow);

    private static async Task<Guid> ClienteAsync(HttpClient c, string nombre, string? email = null) =>
        (await (await c.PostAsJsonAsync("/clientes", new { Nombre = nombre, NifFiscal = Ayudas.GenerarNif(), Email = email })).Content.ReadFromJsonAsync<IdResp>())!.Id;

    /// <summary>Factura de 100 € + IVA 21 % (121 €) emitida hace <paramref name="haceDias"/> días, al contado.</summary>
    private static async Task<FacturaResp> FacturaAsync(HttpClient c, Guid cliente, int haceDias = 0)
    {
        var resp = await c.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente,
            FechaEmision = Hoy.AddDays(-haceDias),
            DiasVencimiento = 0,
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = 100m, CodigoIva = "IVA21" } },
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Created, await resp.Content.ReadAsStringAsync());
        return (await resp.Content.ReadFromJsonAsync<FacturaResp>())!;
    }

    [Fact]
    public async Task Un_anticipo_se_aplica_a_facturas_del_cliente_y_las_cobra()
    {
        var (c, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var cliente = await ClienteAsync(c, "Bar Central SL");
        var alta = await c.PostAsJsonAsync("/anticipos", new { ClienteId = cliente, Importe = 500m, Metodo = "Transferencia" });
        alta.StatusCode.Should().Be(HttpStatusCode.Created);
        var anticipo = (await alta.Content.ReadFromJsonAsync<AnticipoResp>())!;

        var factura = await FacturaAsync(c, cliente);
        var aplicado = await c.PostAsJsonAsync($"/anticipos/{anticipo.Id}/aplicar", new { FacturaId = factura.Id });   // sin importe: lo máximo
        aplicado.StatusCode.Should().Be(HttpStatusCode.OK, await aplicado.Content.ReadAsStringAsync());
        (await aplicado.Content.ReadFromJsonAsync<AnticipoResp>()).Should().Be(new AnticipoResp(anticipo.Id, 500m, 121m, 379m, "Parcial"));

        var saldo = await c.GetFromJsonAsync<SaldoResp>($"/facturas/{factura.Id}/saldo");
        saldo.Should().Be(new SaldoResp(121m, 121m, 0m, "Liquidado"));
    }

    [Fact]
    public async Task Un_anticipo_no_se_aplica_a_otro_cliente_ni_por_encima_de_lo_disponible()
    {
        var (c, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var cliente = await ClienteAsync(c, "Bar Central SL");
        var otro = await ClienteAsync(c, "Otro SL");
        var anticipo = (await (await c.PostAsJsonAsync("/anticipos", new { ClienteId = cliente, Importe = 50m })).Content.ReadFromJsonAsync<AnticipoResp>())!;

        var ajena = await FacturaAsync(c, otro);
        (await c.PostAsJsonAsync($"/anticipos/{anticipo.Id}/aplicar", new { FacturaId = ajena.Id })).StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var propia = await FacturaAsync(c, cliente);
        var exceso = await c.PostAsJsonAsync($"/anticipos/{anticipo.Id}/aplicar", new { FacturaId = propia.Id, Importe = 60m });
        exceso.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await exceso.Content.ReadFromJsonAsync<ProblemaResp>())!.Title.Should().Be("El anticipo solo tiene 50.00 € disponibles.");
    }

    [Fact]
    public async Task Los_impagados_indican_el_nivel_que_toca_y_la_reclamacion_se_envia_con_la_factura()
    {
        var (c, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var cliente = await ClienteAsync(c, "Bar Central SL", "pagos@barcentral.es");
        var vencida = await FacturaAsync(c, cliente, haceDias: 40);
        var cobrada = await FacturaAsync(c, cliente, haceDias: 40);
        await c.PostAsJsonAsync("/cobros", new { FacturaId = cobrada.Id, Importe = cobrada.Total });
        await FacturaAsync(c, cliente);                                                        // de hoy: aún no vencida

        var lista = (await c.GetFromJsonAsync<List<ImpagadoResp>>("/impagados"))!;
        lista.Should().ContainSingle();
        lista[0].Should().Be(new ImpagadoResp(vencida.Id, 40, 121m, 2, null, true));

        var resp = await c.PostAsJsonAsync($"/impagados/{vencida.Id}/reclamar", new { Canal = "Email" });
        resp.StatusCode.Should().Be(HttpStatusCode.OK, await resp.Content.ReadAsStringAsync());
        var enviada = (await resp.Content.ReadFromJsonAsync<ReclamacionEnviadaResp>())!;
        enviada.CorreoEnviado.Should().BeTrue();
        enviada.Reclamacion.Nivel.Should().Be(2);
        enviada.Reclamacion.Asunto.Should().Be($"Segundo aviso: factura {vencida.NumeroCompleto} pendiente");
        enviada.Reclamacion.Texto.Should().Contain("lleva 40 días vencida (121,00 € pendientes)");

        var despues = (await c.GetFromJsonAsync<List<ImpagadoResp>>("/impagados"))!.Single();
        despues.UltimoNivelReclamado.Should().Be(2);
        despues.PendienteDeReclamar.Should().BeFalse();
    }

    [Fact]
    public async Task No_se_reclama_lo_que_no_esta_vencido_y_los_niveles_se_validan()
    {
        var (c, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var cliente = await ClienteAsync(c, "Bar Central SL");
        await FacturaAsync(c, cliente, haceDias: 3);      // primero la antigua: las fechas siguen a la numeración
        var alDia = await FacturaAsync(c, cliente);
        (await c.PostAsJsonAsync($"/impagados/{alDia.Id}/reclamar", new { Canal = "Telefono" })).StatusCode.Should().Be(HttpStatusCode.Conflict);

        var mal = await c.PutAsJsonAsync("/impagados/niveles", new[]
        {
            new { Nivel = 1, DiasTrasVencimiento = 15, Asunto = "a", Texto = "b" },
            new { Nivel = 2, DiasTrasVencimiento = 10, Asunto = "a", Texto = "b" },
        });
        mal.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await c.PutAsJsonAsync("/impagados/niveles", new[] { new { Nivel = 1, DiasTrasVencimiento = 1, Asunto = "Aviso {factura}", Texto = "Debe {pendiente} €" } }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        (await c.GetFromJsonAsync<List<ImpagadoResp>>("/impagados"))!.Single().NivelQueToca.Should().Be(1);
    }
}
