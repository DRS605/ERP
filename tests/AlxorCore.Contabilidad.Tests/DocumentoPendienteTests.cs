using AlxorCore.Contabilidad.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Contabilidad.Tests;

public sealed class DocumentoPendienteTests
{
    private static readonly Guid Empresa = Guid.NewGuid();
    private static readonly DateTimeOffset Ahora = new(2026, 8, 22, 10, 0, 0, TimeSpan.Zero);

    private static DocumentoContabilizable Compra(DateOnly fecha) => new(
        SentidoContable.Compra, "Gasto", Guid.NewGuid(), "Factura P-1", null, "Proveedor SL",
        fecha, 300m, "IVA21", 63m, 0m, 0m, 363m);

    [Fact]
    public void La_fecha_de_registro_arranca_igual_a_la_del_documento()
    {
        var doc = DocumentoPendiente.Crear(Empresa, Compra(new DateOnly(2026, 8, 1)), Ahora);

        doc.Estado.Should().Be(EstadoContabilizacion.Pendiente);
        doc.FechaDocumento.Should().Be(new DateOnly(2026, 8, 1));
        doc.FechaRegistro.Should().Be(new DateOnly(2026, 8, 1));
    }

    [Fact]
    public void El_contable_puede_cambiar_la_fecha_de_registro_mientras_esta_pendiente()
    {
        var doc = DocumentoPendiente.Crear(Empresa, Compra(new DateOnly(2026, 8, 1)), Ahora);

        var r = doc.CambiarFechaRegistro(new DateOnly(2026, 9, 30));

        r.EsCorrecto.Should().BeTrue();
        doc.FechaRegistro.Should().Be(new DateOnly(2026, 9, 30));
    }

    [Fact]
    public void No_se_puede_recontabilizar_ni_cambiar_la_fecha_tras_contabilizar()
    {
        var doc = DocumentoPendiente.Crear(Empresa, Compra(new DateOnly(2026, 8, 1)), Ahora);
        var asientoId = Guid.NewGuid();

        doc.MarcarContabilizado(asientoId).EsCorrecto.Should().BeTrue();
        doc.Estado.Should().Be(EstadoContabilizacion.Contabilizado);
        doc.AsientoId.Should().Be(asientoId);

        doc.MarcarContabilizado(Guid.NewGuid()).EsFallo.Should().BeTrue();
        doc.CambiarFechaRegistro(new DateOnly(2026, 10, 1)).EsFallo.Should().BeTrue();
        doc.AsientoId.Should().Be(asientoId);
    }
}
