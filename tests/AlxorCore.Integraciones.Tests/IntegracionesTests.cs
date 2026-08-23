using AlxorCore.Integraciones.Dominio;
using AlxorCore.Nucleo.Tiempo;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Integraciones.Tests;

public sealed class RelojFijo : IReloj
{
    public DateTimeOffset AhoraUtc { get; init; } = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
}

public class ClaveApiTests
{
    private static readonly IReloj Reloj = new RelojFijo();

    [Fact]
    public void Crear_devuelve_secreto_y_guarda_solo_su_hash()
    {
        var r = ClaveApi.Crear(Guid.NewGuid(), "Integración", Reloj);
        r.EsCorrecto.Should().BeTrue();
        var (clave, secreto) = r.Valor;

        secreto.Should().StartWith("ak_");
        clave.Activa.Should().BeTrue();
        clave.HashSecreto.Should().Be(ClaveApi.Hash(secreto));
        clave.HashSecreto.Should().NotContain(secreto); // no guarda el secreto en claro
        clave.Prefijo.Should().Be(secreto[..11]);
    }

    [Fact]
    public void Rechaza_nombre_vacio()
    {
        ClaveApi.Crear(Guid.NewGuid(), "  ", Reloj).EsFallo.Should().BeTrue();
    }

    [Fact]
    public void Revocar_desactiva_la_clave()
    {
        var clave = ClaveApi.Crear(Guid.NewGuid(), "X", Reloj).Valor.Clave;
        clave.Revocar(Reloj);
        clave.Activa.Should().BeFalse();
        clave.RevocadaEn.Should().NotBeNull();
    }

    [Fact]
    public void El_hash_es_estable_y_distingue_secretos()
    {
        ClaveApi.Hash("ak_uno").Should().Be(ClaveApi.Hash("ak_uno"));
        ClaveApi.Hash("ak_uno").Should().NotBe(ClaveApi.Hash("ak_dos"));
    }
}

public class SuscripcionWebhookTests
{
    private static readonly IReloj Reloj = new RelojFijo();

    [Fact]
    public void Crea_con_url_valida_y_eventos_normalizados()
    {
        var r = SuscripcionWebhook.Crear(Guid.NewGuid(), "https://ejemplo.com/hook", new[] { "Factura.Emitida", "factura.emitida" }, Reloj);
        r.EsCorrecto.Should().BeTrue();
        r.Valor.Secreto.Should().StartWith("whsec_");
        r.Valor.Suscrito("factura.emitida").Should().BeTrue();
        r.Valor.Eventos.Should().Be("factura.emitida"); // normaliza y deduplica
    }

    [Theory]
    [InlineData("no-es-url")]
    [InlineData("ftp://ejemplo.com")]
    [InlineData("")]
    public void Rechaza_url_no_http(string url)
    {
        SuscripcionWebhook.Crear(Guid.NewGuid(), url, new[] { "factura.emitida" }, Reloj).EsFallo.Should().BeTrue();
    }

    [Fact]
    public void Rechaza_sin_eventos_o_evento_desconocido()
    {
        SuscripcionWebhook.Crear(Guid.NewGuid(), "https://x.test", Array.Empty<string>(), Reloj).EsFallo.Should().BeTrue();
        SuscripcionWebhook.Crear(Guid.NewGuid(), "https://x.test", new[] { "no.existe" }, Reloj).EsFallo.Should().BeTrue();
    }
}

public class EntregaWebhookTests
{
    private static readonly IReloj Reloj = new RelojFijo();

    private static EntregaWebhook Nueva()
    {
        var s = SuscripcionWebhook.Crear(Guid.NewGuid(), "https://x.test/hook", new[] { "factura.emitida" }, Reloj).Valor;
        return EntregaWebhook.Crear(s.EmpresaId, s, "factura.emitida", "{\"a\":1}", Reloj);
    }

    [Fact]
    public void La_firma_es_hmac_estable_del_payload()
    {
        var e = Nueva();
        e.Firma().Should().Be(e.Firma()).And.HaveLength(64); // HMAC-SHA256 en hex
    }

    [Fact]
    public void Marcar_entregada_fija_estado_y_fecha()
    {
        var e = Nueva();
        e.MarcarEntregada(200, Reloj);
        e.Estado.Should().Be(EstadoEntrega.Entregada);
        e.EntregadaEn.Should().NotBeNull();
    }

    [Fact]
    public void Reintenta_con_backoff_y_falla_al_agotar_intentos()
    {
        var e = Nueva();
        for (var i = 0; i < EntregaWebhook.MaximoIntentos - 1; i++)
        {
            e.RegistrarFallo("error", Reloj);
            e.Estado.Should().Be(EstadoEntrega.Pendiente);
            e.ProximoIntento.Should().BeAfter(Reloj.AhoraUtc); // reprogramada
        }

        e.RegistrarFallo("último", Reloj);
        e.Estado.Should().Be(EstadoEntrega.Fallida);
        e.Intentos.Should().Be(EntregaWebhook.MaximoIntentos);
    }
}
