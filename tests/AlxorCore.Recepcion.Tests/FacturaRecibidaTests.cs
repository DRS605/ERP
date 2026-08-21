using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Recepcion.Dominio;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Recepcion.Tests;

public sealed class RelojFijo : IReloj
{
    public DateTimeOffset AhoraUtc { get; init; } = new(2026, 8, 21, 10, 0, 0, TimeSpan.Zero);
}

public sealed class FacturaRecibidaTests
{
    private static readonly Guid Empresa = Guid.NewGuid();
    private static readonly IReloj Reloj = new RelojFijo();
    private static readonly byte[] Pdf = { 0x25, 0x50, 0x44, 0x46 }; // "%PDF"

    private static FacturaRecibida Recibida() =>
        FacturaRecibida.Recibir(Empresa, OrigenRecepcion.Correo, "proveedor@correo.com", "Factura agosto",
            "factura.pdf", "application/pdf", Pdf, Reloj).Valor;

    [Fact]
    public void Recibir_deja_la_factura_en_bandeja()
    {
        var factura = Recibida();
        factura.Estado.Should().Be(EstadoRecepcion.Recibida);
        factura.NombreArchivo.Should().Be("factura.pdf");
        factura.Contenido.Should().Equal(Pdf);
        factura.EventosDominio.Should().ContainSingle(e => e is FacturaRecibidaRegistrada);
    }

    [Fact]
    public void Recibir_sin_contenido_falla()
    {
        var r = FacturaRecibida.Recibir(Empresa, OrigenRecepcion.Manual, null, null, "factura.pdf", "application/pdf", Array.Empty<byte>(), Reloj);
        r.EsFallo.Should().BeTrue();
        r.Error.Codigo.Should().Be("recepcion.contenido_vacio");
    }

    [Fact]
    public void No_se_puede_contabilizar_sin_validar()
    {
        var factura = Recibida();
        var r = factura.Contabilizar(Guid.NewGuid(), Reloj.AhoraUtc);
        r.EsFallo.Should().BeTrue();
        r.Error.Codigo.Should().Be("recepcion.no_validada");
    }

    [Fact]
    public void Validar_exige_proveedor_fecha_y_base()
    {
        var factura = Recibida();
        factura.Validar(null, null, "F-1", new DateOnly(2026, 8, 1), 100m, "IVA21", 0m).EsFallo.Should().BeTrue();
        factura.Validar(null, "Papelería SL", "F-1", null, 100m, "IVA21", 0m).EsFallo.Should().BeTrue();
        factura.Validar(null, "Papelería SL", "F-1", new DateOnly(2026, 8, 1), 0m, "IVA21", 0m).EsFallo.Should().BeTrue();
    }

    [Fact]
    public void Flujo_validar_y_contabilizar()
    {
        var factura = Recibida();
        factura.Validar(null, "Papelería SL", "F-2026-1", new DateOnly(2026, 8, 1), 200m, "IVA21", 15m).EsCorrecto.Should().BeTrue();
        factura.Estado.Should().Be(EstadoRecepcion.Validada);
        factura.CodigoIva.Should().Be("IVA21");
        factura.BaseImponible.Should().Be(200m);

        var gastoId = Guid.NewGuid();
        factura.Contabilizar(gastoId, Reloj.AhoraUtc).EsCorrecto.Should().BeTrue();
        factura.Estado.Should().Be(EstadoRecepcion.Contabilizada);
        factura.GastoId.Should().Be(gastoId);
        factura.EventosDominio.Should().Contain(e => e is FacturaRecibidaContabilizada);

        // Una vez contabilizada no se puede rechazar ni re-validar.
        factura.Rechazar("ya no").EsFallo.Should().BeTrue();
        factura.Validar(null, "Otro", "X", new DateOnly(2026, 8, 2), 50m, "IVA21", 0m).EsFallo.Should().BeTrue();
    }

    [Fact]
    public void Rechazar_marca_rechazada()
    {
        var factura = Recibida();
        factura.Rechazar("Duplicada").EsCorrecto.Should().BeTrue();
        factura.Estado.Should().Be(EstadoRecepcion.Rechazada);
        factura.MotivoRechazo.Should().Be("Duplicada");
    }
}
