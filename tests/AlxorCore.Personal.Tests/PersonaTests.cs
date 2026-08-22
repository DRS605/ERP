using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Personal.Dominio;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Personal.Tests;

public sealed class RelojFijo : IReloj
{
    public DateTimeOffset AhoraUtc { get; init; } = new(2026, 8, 22, 10, 0, 0, TimeSpan.Zero);
}

public class PersonaTests
{
    private static readonly IReloj Reloj = new RelojFijo();
    private static readonly Guid Empresa = Guid.NewGuid();

    [Fact]
    public void Crear_valida_y_redondea_la_tarifa()
    {
        var r = Persona.Crear(Empresa, "  Ana Pérez ", "  Oficial 1ª  ", 18.005m, Reloj);

        r.EsCorrecto.Should().BeTrue();
        var p = r.Valor;
        p.EmpresaId.Should().Be(Empresa);
        p.Nombre.Should().Be("Ana Pérez");        // recortado
        p.Puesto.Should().Be("Oficial 1ª");        // recortado
        p.TarifaHora.Should().Be(18.01m);          // redondeo a 2 decimales
        p.Activo.Should().BeTrue();
    }

    [Fact]
    public void Puesto_en_blanco_queda_nulo()
    {
        var p = Persona.Crear(Empresa, "Luis", "   ", 20m, Reloj).Valor;
        p.Puesto.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rechaza_nombre_vacio(string? nombre)
    {
        Persona.Crear(Empresa, nombre, null, 15m, Reloj).EsFallo.Should().BeTrue();
    }

    [Fact]
    public void Rechaza_tarifa_negativa()
    {
        Persona.Crear(Empresa, "Ana", null, -1m, Reloj).EsFallo.Should().BeTrue();
    }

    [Fact]
    public void Actualizar_cambia_los_datos_y_permite_desactivar()
    {
        var p = Persona.Crear(Empresa, "Ana", "Oficial", 18m, Reloj).Valor;

        var r = p.Actualizar("Ana María", "Encargada", 25.5m, activo: false, Reloj);

        r.EsCorrecto.Should().BeTrue();
        p.Nombre.Should().Be("Ana María");
        p.Puesto.Should().Be("Encargada");
        p.TarifaHora.Should().Be(25.5m);
        p.Activo.Should().BeFalse();
    }

    [Fact]
    public void Actualizar_rechaza_datos_invalidos_sin_mutar()
    {
        var p = Persona.Crear(Empresa, "Ana", "Oficial", 18m, Reloj).Valor;

        p.Actualizar("", "Encargada", 25m, activo: true, Reloj).EsFallo.Should().BeTrue();
        p.Nombre.Should().Be("Ana");   // no cambió
    }
}
