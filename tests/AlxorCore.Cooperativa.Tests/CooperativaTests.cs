using AlxorCore.Cooperativa.Dominio;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Cooperativa.Tests;

public sealed class RepartoTests
{
    private static readonly Guid Empresa = Guid.NewGuid();
    private static readonly DateTimeOffset Ahora = new(2026, 12, 31, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Fecha = new(2026, 12, 31);

    private static ConfiguracionCooperativa Config(FormaJuridica forma = FormaJuridica.Cooperativa, decimal fro = 20m, decimal fep = 5m)
    {
        var c = ConfiguracionCooperativa.PorDefecto(Empresa);
        c.Fijar(forma, 0m, fro, fep, 9.25m, 19m, forma == FormaJuridica.Sat ? BaseRetorno.Capital : BaseRetorno.Kilos, 30m, 20m, true, CuentasCooperativa.PorDefecto)
            .EsCorrecto.Should().BeTrue();
        return c;
    }

    [Fact]
    public void Reparte_fondos_intereses_y_retorno_al_centimo_y_cuadra()
    {
        var a = new SocioEnReparto(Guid.NewGuid(), "A", 1m, 1000m, false);
        var b = new SocioEnReparto(Guid.NewGuid(), "B", 1m, 0m, true);
        var c = new SocioEnReparto(Guid.NewGuid(), "C", 1m, 0m, false);
        var r = Reparto.Nuevo(Empresa, 2026, Ahora);

        r.Calcular(Config(), Fecha, null, 1000m, null, null, 0m, 5m, [a, b, c], null).EsCorrecto.Should().BeTrue();

        r.ImporteFro.Should().Be(200m);
        r.ImporteFep.Should().Be(50m);
        r.ImporteIntereses.Should().Be(50m);
        r.ImporteRetorno.Should().Be(700m);
        r.Lineas.Sum(l => l.Retorno).Should().Be(700m, "el redondeo se reparte sin perder céntimos");
        r.Lineas.Select(l => l.Retorno).Should().BeEquivalentTo([233.34m, 233.33m, 233.33m]);
        var lb = r.Lineas.Single(l => l.SocioId == b.SocioId);
        lb.Capitalizado.Should().Be(lb.Retorno);
        lb.Retencion.Should().Be(0m, "lo capitalizado no lleva retención");
        var la = r.Lineas.Single(l => l.SocioId == a.SocioId);
        la.Retencion.Should().Be(Math.Round((la.Retorno + 50m) * 0.19m, 2));
        r.Lineas.Should().OnlyContain(l => l.Retorno + l.Intereses == l.Neto + l.Retencion + l.Capitalizado);
        (r.ImporteFro + r.ImporteFep + r.ReservasVoluntarias + r.ImporteIntereses + r.ImporteRetorno).Should().Be(r.Excedente);
    }

    [Fact]
    public void No_baja_de_los_minimos_ni_reparte_sin_actividad()
    {
        var r = Reparto.Nuevo(Empresa, 2026, Ahora);
        var s = new SocioEnReparto(Guid.NewGuid(), "A", 0m, 0m, false);
        r.Calcular(Config(), Fecha, null, 1000m, 10m, null, 0m, 0m, [s], null).Error.Codigo.Should().Be("reparto.fondos");
        r.Calcular(Config(), Fecha, null, 1000m, null, null, 0m, 10m, [s], null).Error.Codigo.Should().Be("reparto.intereses");
        r.Calcular(Config(), Fecha, null, 1000m, null, null, 0m, 0m, [s], null).Error.Codigo.Should().Be("reparto.sin_actividad");
        r.Calcular(Config(), Fecha, null, 1000m, null, null, 800m, 0m, [s], null).Error.Codigo.Should().Be("reparto.insuficiente");
        r.Calcular(Config(), Fecha, null, -5m, null, null, 0m, 0m, [s], null).Error.Codigo.Should().Be("reparto.excedente");
        r.Calcular(Config(), new DateOnly(2025, 12, 31), null, 1000m, null, null, 750m, 0m, [s], null).Error.Codigo.Should().Be("reparto.fecha");
        r.Calcular(Config(), Fecha, null, 1000m, null, null, 750m, 0m, [s], null).EsCorrecto.Should().BeTrue("todo a fondos y reservas");
        r.Lineas.Should().BeEmpty();
    }

    [Fact]
    public void Contabilizado_no_se_recalcula_y_solo_se_anula_con_motivo()
    {
        var r = Reparto.Nuevo(Empresa, 2026, Ahora);
        r.Anular("x").Error.Codigo.Should().Be("reparto.no_contabilizado");
        r.Calcular(Config(), Fecha, null, 100m, null, null, 75m, 0m, [], null).EsCorrecto.Should().BeTrue();
        r.Contabilizar(Guid.NewGuid()).EsCorrecto.Should().BeTrue();
        r.Calcular(Config(), Fecha, null, 100m, null, null, 75m, 0m, [], null).Error.Codigo.Should().Be("reparto.no_borrador");
        r.Anular(" ").Error.Codigo.Should().Be("reparto.motivo");
        r.Anular("Acuerdo corregido").EsCorrecto.Should().BeTrue();
        r.Estado.Should().Be(EstadoReparto.Anulado);
    }
}

public sealed class CapitalTests
{
    private static readonly Guid Empresa = Guid.NewGuid();
    private static readonly Guid SocioId = Guid.NewGuid();
    private static readonly DateOnly Dia = new(2026, 3, 1);
    private static readonly DateTimeOffset Ahora = new(2026, 3, 1, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Los_movimientos_validan_sus_importes_y_el_saldo_se_calcula_por_clase()
    {
        MovimientoCapital.Suscripcion(Empresa, SocioId, Dia, ClaseAportacion.Obligatoria, 100m, 150m, null, Ahora).Error.Codigo.Should().Be("capital.importe");
        MovimientoCapital.Suscripcion(Empresa, SocioId, Dia, ClaseAportacion.Obligatoria, 100.001m, 0m, null, Ahora).Error.Codigo.Should().Be("capital.importe");
        MovimientoCapital.Reembolso(Empresa, SocioId, Dia, ClaseAportacion.Obligatoria, 100m, 100m, 101m, null, Ahora).Error.Codigo.Should().Be("capital.importe");

        var s = MovimientoCapital.Suscripcion(Empresa, SocioId, Dia, ClaseAportacion.Obligatoria, 600m, 200m, null, Ahora).Valor;
        var d = MovimientoCapital.Desembolso(Empresa, SocioId, Dia, ClaseAportacion.Obligatoria, 400m, null, Ahora).Valor;
        var v = MovimientoCapital.Suscripcion(Empresa, SocioId, Dia, ClaseAportacion.Voluntaria, 50m, 50m, null, Ahora).Valor;
        var saldo = SaldoCapital.De([s, d, v, v.Anulacion(Dia, "error", Ahora)]);
        saldo.Single(x => x.Clase == ClaseAportacion.Obligatoria).Should().Be(new SaldoCapital(ClaseAportacion.Obligatoria, 600m, 600m));
        saldo.Single(x => x.Clase == ClaseAportacion.Voluntaria).Should().Be(new SaldoCapital(ClaseAportacion.Voluntaria, 0m, 0m));

        var r = MovimientoCapital.Reembolso(Empresa, SocioId, Dia, ClaseAportacion.Obligatoria, 600m, 600m, 120m, null, Ahora).Valor;
        r.ADevolver.Should().Be(480m);
        r.Suscrito.Should().Be(-600m);
    }

    [Fact]
    public void La_transmision_une_dos_movimientos_de_signo_contrario()
    {
        var otro = Guid.NewGuid();
        MovimientoCapital.Transmision(Empresa, SocioId, SocioId, Dia, ClaseAportacion.Voluntaria, 10m, null, Ahora).Error.Codigo.Should().Be("capital.transmision");
        var (salida, entrada) = MovimientoCapital.Transmision(Empresa, SocioId, otro, Dia, ClaseAportacion.Voluntaria, 10m, null, Ahora).Valor;
        salida.GrupoId.Should().NotBeNull().And.Be(entrada.GrupoId);
        (salida.Suscrito + entrada.Suscrito).Should().Be(0m);
        entrada.SocioId.Should().Be(otro);
    }

    [Fact]
    public void La_baja_es_definitiva_y_fija_la_deduccion_maxima()
    {
        var socio = Socio.Crear(Empresa, 1, Guid.NewGuid(), "Rosa", null, TipoSocio.Comun, Dia, null).Valor;
        socio.DarDeBaja(Dia.AddDays(-1), MotivoBaja.Expulsion).Error.Codigo.Should().Be("socio.fecha_baja");
        socio.DarDeBaja(Dia, MotivoBaja.Expulsion).EsCorrecto.Should().BeTrue();
        socio.DarDeBaja(Dia, MotivoBaja.Fallecimiento).Error.Codigo.Should().Be("socio.ya_de_baja");

        var c = ConfiguracionCooperativa.PorDefecto(Empresa);
        c.DeduccionMaxima(MotivoBaja.Expulsion).Should().Be(30m);
        c.DeduccionMaxima(MotivoBaja.VoluntariaNoJustificada).Should().Be(20m);
        c.DeduccionMaxima(MotivoBaja.Fallecimiento).Should().Be(0m);
        socio.ConRetorno(FormaJuridica.Cooperativa).Should().BeTrue();
        Socio.Crear(Empresa, 2, Guid.NewGuid(), "Capital", null, TipoSocio.Colaborador, Dia, null).Valor.ConRetorno(FormaJuridica.Cooperativa).Should().BeFalse();
    }

    [Fact]
    public void La_configuracion_valida_porcentajes_y_cuentas()
    {
        var c = ConfiguracionCooperativa.PorDefecto(Empresa);
        c.Fijar(FormaJuridica.Cooperativa, 0m, 90m, 20m, 9m, 19m, BaseRetorno.Kilos, 30m, 20m, true, CuentasCooperativa.PorDefecto).Error.Codigo.Should().Be("cooperativa.fondos");
        c.Fijar(FormaJuridica.Cooperativa, 0m, 20m, 5m, 9m, 19m, BaseRetorno.Kilos, 30m, 20m, true, CuentasCooperativa.PorDefecto with { Fro = "11A" })
            .Error.Codigo.Should().Be("cooperativa.cuentas");
        c.Fijar(FormaJuridica.Cooperativa, -1m, 20m, 5m, 9m, 19m, BaseRetorno.Kilos, 30m, 20m, true, CuentasCooperativa.PorDefecto).Error.Codigo.Should().Be("cooperativa.aportacion");
    }

    [Fact]
    public void El_acta_se_numera_al_aprobarla()
    {
        var d = new DatosActa(Dia, CaracterSesion.Ordinaria, "Orden", "Acuerdos");
        Acta.Crear(Empresa, OrganoSocial.AsambleaGeneral, d with { Acuerdos = " " }).Error.Codigo.Should().Be("acta.texto");
        var a = Acta.Crear(Empresa, OrganoSocial.AsambleaGeneral, d).Valor;
        a.Numero.Should().BeNull();
        a.Aprobar(1, Ahora).EsCorrecto.Should().BeTrue();
        a.Numero.Should().Be(1);
        a.Cambiar(d).Error.Codigo.Should().Be("acta.aprobada");
    }
}
