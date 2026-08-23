using AlxorCore.Aprobaciones.Dominio;
using AlxorCore.Nucleo.Tiempo;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Aprobaciones.Tests;

public sealed class RelojFijo : IReloj
{
    public DateTimeOffset AhoraUtc { get; init; } = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
}

public class ReglaAprobacionTests
{
    private static readonly IReloj Reloj = new RelojFijo();

    [Fact]
    public void Regla_activa_exige_a_partir_del_umbral()
    {
        var r = ReglaAprobacion.Crear(Guid.NewGuid(), "Factura", 1000m, activa: true, Reloj).Valor;
        r.Requiere(999.99m).Should().BeFalse();
        r.Requiere(1000m).Should().BeTrue();
        r.Requiere(5000m).Should().BeTrue();
    }

    [Fact]
    public void Regla_inactiva_no_exige()
    {
        var r = ReglaAprobacion.Crear(Guid.NewGuid(), "Factura", 0m, activa: false, Reloj).Valor;
        r.Requiere(1_000_000m).Should().BeFalse();
    }

    [Fact]
    public void Rechaza_tipo_vacio_y_umbral_negativo()
    {
        ReglaAprobacion.Crear(Guid.NewGuid(), "  ", 100m, true, Reloj).EsFallo.Should().BeTrue();
        ReglaAprobacion.Crear(Guid.NewGuid(), "Factura", -1m, true, Reloj).EsFallo.Should().BeTrue();
    }
}

public class SolicitudAprobacionTests
{
    private static readonly IReloj Reloj = new RelojFijo();
    private static readonly Guid Empresa = Guid.NewGuid();

    private static SolicitudAprobacion Nueva(Guid solicitante) =>
        SolicitudAprobacion.Crear(Empresa, "Factura", Guid.NewGuid(), "FA2026/0001", 5000m, solicitante, Reloj).Valor;

    [Fact]
    public void Un_aprobador_distinto_puede_aprobar()
    {
        var solicitante = Guid.NewGuid();
        var s = Nueva(solicitante);
        var aprobador = Guid.NewGuid();

        s.Aprobar(aprobador, Reloj).EsCorrecto.Should().BeTrue();
        s.Estado.Should().Be(EstadoAprobacion.Aprobada);
        s.AprobadorUsuarioId.Should().Be(aprobador);
        s.ResueltaEn.Should().NotBeNull();
        s.EventosDominio.OfType<SolicitudResuelta>().Should().ContainSingle();
    }

    [Fact]
    public void Segregacion_de_funciones_el_solicitante_no_puede_aprobar()
    {
        var solicitante = Guid.NewGuid();
        var s = Nueva(solicitante);

        var r = s.Aprobar(solicitante, Reloj);
        r.EsFallo.Should().BeTrue();
        r.Error.Codigo.Should().Be("aprobacion.segregacion");
        s.Estado.Should().Be(EstadoAprobacion.Pendiente);
    }

    [Fact]
    public void Segregacion_de_funciones_tambien_al_rechazar()
    {
        var solicitante = Guid.NewGuid();
        var s = Nueva(solicitante);
        s.Rechazar(solicitante, "no", Reloj).EsFallo.Should().BeTrue();
    }

    [Fact]
    public void No_se_puede_resolver_dos_veces()
    {
        var s = Nueva(Guid.NewGuid());
        s.Aprobar(Guid.NewGuid(), Reloj).EsCorrecto.Should().BeTrue();
        s.Aprobar(Guid.NewGuid(), Reloj).EsFallo.Should().BeTrue();          // ya resuelta
        s.Rechazar(Guid.NewGuid(), "tarde", Reloj).EsFallo.Should().BeTrue();
    }

    [Fact]
    public void Rechazar_guarda_el_motivo()
    {
        var s = Nueva(Guid.NewGuid());
        s.Rechazar(Guid.NewGuid(), "Fuera de presupuesto", Reloj).EsCorrecto.Should().BeTrue();
        s.Estado.Should().Be(EstadoAprobacion.Rechazada);
        s.Motivo.Should().Be("Fuera de presupuesto");
    }

    [Fact]
    public void Rechaza_referencia_vacia_y_solicitante_vacio()
    {
        SolicitudAprobacion.Crear(Empresa, "Factura", Guid.NewGuid(), "  ", 100m, Guid.NewGuid(), Reloj).EsFallo.Should().BeTrue();
        SolicitudAprobacion.Crear(Empresa, "Factura", Guid.NewGuid(), "FA1", 100m, Guid.Empty, Reloj).EsFallo.Should().BeTrue();
    }
}
