using AlxorCore.Agro.Aplicacion;
using AlxorCore.Agro.Dominio;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Agro.Tests;

public sealed class SiexDominioTests
{
    private static readonly Guid Empresa = Guid.NewGuid();
    private static readonly DateOnly Dia = new(2026, 4, 10);

    [Theory]
    [InlineData("46:250:0:0:12:45:1", 46, 250, 1)]
    [InlineData("03:14:0:0:2:118:3", 3, 14, 3)]
    public void La_referencia_SIGPAC_se_separa_en_sus_siete_numeros(string referencia, int provincia, int municipio, int recinto) =>
        SiexAgro.Sigpac(referencia).Should().Match<SigpacDto>(s => s.Provincia == provincia && s.Municipio == municipio && s.Recinto == recinto);

    [Theory]
    [InlineData(null)]
    [InlineData("46:250:0:0:12:45")]
    [InlineData("46:250:0:0:12:45:X")]
    [InlineData("46:-250:0:0:12:45:1")]
    public void Una_referencia_incompleta_no_es_SIGPAC(string? referencia) => SiexAgro.Sigpac(referencia).Should().BeNull();

    [Fact]
    public void Cada_labor_solo_guarda_los_datos_SIEX_que_le_tocan()
    {
        var datos = new DatosSiexLabor("ROPO-1", "ROMA-1", "Asesora", EficaciaTratamiento.Buena, "Fertirrigación", TipoFertilizante.Organico);
        var fito = TratamientoParcela.Crear(Empresa, Guid.NewGuid(), Dia, "Cobre", 7, new RelojFijo()).Valor;
        fito.CompletarSiex(datos).EsCorrecto.Should().BeTrue();
        fito.Should().Match<TratamientoParcela>(t => t.CarneAplicador == "ROPO-1" && t.EquipoRoma == "ROMA-1" && t.Asesor == "Asesora" && t.Eficacia == EficaciaTratamiento.Buena
            && t.MetodoAplicacion == null && t.TipoFertilizante == null);

        var abono = TratamientoParcela.Crear(Empresa, Guid.NewGuid(), Dia, "Estiércol", 0, new RelojFijo(), tipo: TipoLabor.Abonado).Valor;
        abono.CompletarSiex(datos).EsCorrecto.Should().BeTrue();
        abono.Should().Match<TratamientoParcela>(t => t.CarneAplicador == null && t.Asesor == null && t.Eficacia == null && t.EquipoRoma == "ROMA-1"
            && t.TipoFertilizante == TipoFertilizante.Organico && t.MetodoAplicacion == "Fertirrigación");
        abono.EvaluarEficacia(EficaciaTratamiento.Mala).Error.Codigo.Should().Be("tratamiento.eficacia");

        fito.Anular("error");
        fito.EvaluarEficacia(EficaciaTratamiento.Mala).Error.Codigo.Should().Be("tratamiento.eficacia");
    }

    [Fact]
    public void El_analisis_y_el_plan_validan_sus_valores()
    {
        AnalisisAgro.Crear(Empresa, Guid.NewGuid(), new DatosAnalisis(Dia, TipoAnalisis.Suelo, " ")).Error.Codigo.Should().Be("analisis.laboratorio");
        AnalisisAgro.Crear(Empresa, Guid.NewGuid(), new DatosAnalisis(Dia, TipoAnalisis.Suelo, "Lab", Ph: 15m)).Error.Codigo.Should().Be("analisis.valores");
        AnalisisAgro.Crear(Empresa, Guid.NewGuid(), new DatosAnalisis(Dia, TipoAnalisis.Suelo, "Lab", SuperaLimites: true)).Valor.SuperaLimites
            .Should().BeNull("solo los análisis de residuos superan límites");
        PlanAbonado.Crear(Empresa, Guid.NewGuid(), 2026, new DatosPlanAbonado(-1m, 0m, 0m)).Error.Codigo.Should().Be("plan_abonado.valores");
        PlanAbonado.Crear(Empresa, Guid.NewGuid(), 1990, new DatosPlanAbonado(1m, 0m, 0m)).Error.Codigo.Should().Be("plan_abonado.anio");

        var e = ExplotacionSiex.Nueva(Empresa, Guid.NewGuid());
        e.Fijar(new DatosExplotacionSiex("ES 46 001")).Error.Codigo.Should().Be("explotacion.regepa");
        e.Fijar(new DatosExplotacionSiex("es46-001/2")).EsCorrecto.Should().BeTrue();
        e.CodigoRegepa.Should().Be("ES46-001/2");
    }
}
