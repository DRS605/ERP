using AlxorCore.Catalogo.Dominio;
using AlxorCore.Informes.Aplicacion;
using AlxorCore.Organizacion.Dominio;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Informes.Tests;

/// <summary>Reglas de la prorrata del IVA/IGIC (arts. 102-106 Ley 37/1992).</summary>
public class ProrrataTests
{
    [Theory]
    [InlineData(800, 200, 80)]
    [InlineData(801, 200, 81)]      // 80,02 % → se redondea a la unidad superior
    [InlineData(1, 999, 1)]
    [InlineData(0, 500, 0)]
    [InlineData(0, 0, 100)]         // sin operaciones: se deduce todo
    public void El_porcentaje_se_redondea_a_la_unidad_superior(decimal conDerecho, decimal sinDerecho, int esperado) =>
        ReglaProrrata.Porcentaje(conDerecho, sinDerecho).Should().Be(esperado);

    [Fact]
    public void La_general_aplica_el_porcentaje_a_todo_y_la_especial_por_afectacion()
    {
        var cuotas = new CuotasSoportadas(0m, 600m, Comun: 200m, ConDerecho: 300m, SinDerecho: 100m);
        ReglaProrrata.Deducible(null, 50, cuotas).Should().Be(600m);
        ReglaProrrata.Deducible(RegimenProrrata.General, 50, cuotas).Should().Be(300m);
        ReglaProrrata.Deducible(RegimenProrrata.Especial, 50, cuotas).Should().Be(400m);   // 300 + 50 % de 200
    }

    [Theory]
    [InlineData(ClaseIva.Ordinario, DerechoDeduccion.ConDerecho)]
    [InlineData(ClaseIva.Exportacion, DerechoDeduccion.ConDerecho)]
    [InlineData(ClaseIva.Intracomunitario, DerechoDeduccion.ConDerecho)]
    [InlineData(ClaseIva.InversionSujetoPasivo, DerechoDeduccion.ConDerecho)]
    [InlineData(ClaseIva.Exento, DerechoDeduccion.SinDerecho)]
    [InlineData(ClaseIva.NoSujeto, DerechoDeduccion.Excluida)]
    public void Las_ventas_dan_o_no_derecho_a_deducir_segun_su_clase(ClaseIva clase, DerechoDeduccion esperado) =>
        ReglaProrrata.Derecho(clase).Should().Be(esperado);
}
