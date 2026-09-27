using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Tesoreria.Dominio;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Tesoreria.Tests;

/// <summary>Pruebas de anticipos de clientes y niveles de reclamación de impagados.</summary>
public class CobranzaTests
{
    private static readonly IReloj Reloj = new RelojFijo();
    private static readonly Guid Cliente = Guid.NewGuid();

    private static Anticipo Anticipo500() =>
        Anticipo.Registrar(Guid.NewGuid(), Cliente, 500m, new DateOnly(2026, 9, 1), null, "Transferencia", Reloj).Valor;

    [Fact]
    public void Un_anticipo_nuevo_esta_disponible_entero()
    {
        var a = Anticipo500();
        a.Disponible.Should().Be(500m);
        a.Estado.Should().Be(EstadoAnticipo.Disponible);
        a.Concepto.Should().Be("Entrega a cuenta");
    }

    [Fact]
    public void Al_aplicarse_baja_el_disponible_y_el_estado_se_deriva()
    {
        var a = Anticipo500();
        a.ValidarAplicacion(Cliente, 121m).Valor.Should().Be(121m);
        a.AnotarAplicacion(Guid.NewGuid(), 121m, new DateOnly(2026, 9, 10), Guid.NewGuid());
        a.Disponible.Should().Be(379m);
        a.Estado.Should().Be(EstadoAnticipo.Parcial);

        a.AnotarAplicacion(Guid.NewGuid(), 379m, new DateOnly(2026, 9, 20), Guid.NewGuid());
        a.Estado.Should().Be(EstadoAnticipo.Aplicado);
    }

    [Fact]
    public void No_se_aplica_a_otro_cliente_ni_por_encima_de_lo_disponible()
    {
        var a = Anticipo500();
        a.ValidarAplicacion(Guid.NewGuid(), 10m).Error.Codigo.Should().Be("anticipo.otro_cliente");
        a.ValidarAplicacion(Cliente, 500.01m).Error.Mensaje.Should().Be("El anticipo solo tiene 500,00 € disponibles.");
        a.ValidarAplicacion(Cliente, 0m).Error.Codigo.Should().Be("anticipo.importe_invalido");
    }

    [Fact]
    public void Un_anticipo_sin_importe_positivo_no_se_registra()
    {
        Anticipo.Registrar(Guid.NewGuid(), Cliente, 0m, new DateOnly(2026, 9, 1), null, null, Reloj).EsFallo.Should().BeTrue();
    }

    [Theory]
    [InlineData(3, null)]
    [InlineData(7, 1)]
    [InlineData(45, 2)]
    [InlineData(200, 3)]
    public void Toca_el_nivel_mas_alto_alcanzado_por_los_dias_de_retraso(int dias, int? nivel)
    {
        NivelReclamacion.QueToca(NivelReclamacion.PorDefecto, dias)?.Nivel.Should().Be(nivel);
        if (nivel is null)
        {
            NivelReclamacion.QueToca(NivelReclamacion.PorDefecto, dias).Should().BeNull();
        }
    }

    [Fact]
    public void Los_niveles_deben_ir_seguidos_y_con_dias_crecientes()
    {
        NivelReclamacion.Validar(NivelReclamacion.PorDefecto).Should().BeNull();
        NivelReclamacion.Validar([new(1, 10, "a", "b"), new(2, 10, "a", "b")])!.Mensaje
            .Should().Be("El nivel 2 debe llegar más días después del vencimiento que el anterior.");
        NivelReclamacion.Validar([new(2, 10, "a", "b")])!.Mensaje.Should().Be("Los niveles se numeran seguidos desde 1.");
        NivelReclamacion.Validar([new(1, 10, " ", "b")])!.Mensaje.Should().Be("El nivel 1 necesita asunto y texto.");
    }

    [Fact]
    public void El_texto_sustituye_las_variables()
    {
        NivelReclamacion.Componer("{cliente}: {factura} venció el {vencimiento}, {dias} días, {pendiente} €",
                "Bar Central", "FA2026/000012", new DateOnly(2026, 8, 1), 1234.5m, 56)
            .Should().Be("Bar Central: FA2026/000012 venció el 01/08/2026, 56 días, 1234,50 €");
    }
}
