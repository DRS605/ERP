using AlxorCore.Agro.Dominio;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Agro.Tests;

public sealed class GenealogiaTests
{
    [Fact]
    public void Reparte_lo_consumido_de_cada_origen_en_proporcion_a_las_salidas()
    {
        Guid a = Guid.NewGuid(), b = Guid.NewGuid(), s1 = Guid.NewGuid(), s2 = Guid.NewGuid(), s3 = Guid.NewGuid();
        var aristas = Genealogia.Repartir([(a, 6_000m), (b, 3_000m)], [(s1, 6_000m), (s2, 2_000m)]);

        aristas.Should().HaveCount(4);
        aristas.Where(x => x.Origen == a).Sum(x => x.Kilos).Should().Be(6_000m);
        aristas.Where(x => x.Origen == b).Sum(x => x.Kilos).Should().Be(3_000m);
        aristas.Single(x => x.Origen == a && x.Destino == s1).Kilos.Should().Be(4_500m);

        // Con redondeo, la última salida se lleva la diferencia: nunca se pierden ni se inventan gramos.
        var tres = Genealogia.Repartir([(a, 1_000m)], [(s1, 1m), (s2, 1m), (s3, 1m)]);
        tres.Sum(x => x.Kilos).Should().Be(1_000m);
        tres.Select(x => x.Kilos).Should().Equal(333.333m, 333.333m, 333.334m);
    }
}

public sealed class CertificacionTests
{
    [Fact]
    public void Mezclar_ecologico_con_convencional_da_convencional()
    {
        ReglasCertificacion.Comunes([Certificaciones.Ecologico | Certificaciones.GlobalGap, Certificaciones.GlobalGap]).Should().Be(Certificaciones.GlobalGap);
        ReglasCertificacion.Comunes([]).Should().Be(Certificaciones.Ninguna);
    }

    [Fact]
    public void El_articulo_exige_su_certificacion_y_descalificar_requiere_motivo()
    {
        ReglasCertificacion.Aplicar(Certificaciones.GlobalGap, Certificaciones.Ecologico, null, "Eco").Error.Codigo.Should().Be("certificacion.falta");
        ReglasCertificacion.Aplicar(Certificaciones.Ecologico, Certificaciones.Ninguna, null, "Conv").Error.Codigo.Should().Be("certificacion.descalificar");

        var r = ReglasCertificacion.Aplicar(Certificaciones.Ecologico | Certificaciones.GlobalGap, Certificaciones.Ninguna, "Pedido convencional", "Conv").Valor;
        r.Resultado.Should().Be(Certificaciones.GlobalGap, "se quita el ecológico; el GlobalG.A.P. sigue");
        r.Quitadas.Should().Be(Certificaciones.Ecologico);

        ReglasCertificacion.Aplicar(Certificaciones.Ecologico, null, null, "Sin declarar").Valor.Resultado.Should().Be(Certificaciones.Ecologico);
        ReglasCertificacion.Aplicar(Certificaciones.Ecologico, Certificaciones.Ecologico, null, "Eco").Valor.Quitadas.Should().Be(Certificaciones.Ninguna);
    }

    [Fact]
    public void El_certificado_vale_dentro_de_su_vigencia_y_sin_baja()
    {
        var c = CertificadoAgro.Crear(Guid.NewGuid(), Guid.NewGuid(), null, Certificaciones.Ecologico, "ES-ECO-001", null, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)).Valor;
        c.VigenteEl(new DateOnly(2026, 6, 1)).Should().BeTrue();
        c.VigenteEl(new DateOnly(2027, 1, 1)).Should().BeFalse();
        c.DarDeBaja(true);
        c.VigenteEl(new DateOnly(2026, 6, 1)).Should().BeFalse();
        CertificadoAgro.Crear(Guid.NewGuid(), Guid.NewGuid(), null, Certificaciones.Ecologico | Certificaciones.Grasp, "X", null, new DateOnly(2026, 1, 1), null)
            .Error.Codigo.Should().Be("certificado.tipo");
    }
}

public sealed class RecepcionCompletaTests
{
    private sealed class Reloj : AlxorCore.Nucleo.Tiempo.IReloj
    {
        public DateTimeOffset AhoraUtc { get; } = new(2026, 1, 10, 8, 0, 0, TimeSpan.Zero);
    }

    private static readonly Guid Box = Guid.NewGuid();

    private static (Recepcion R, LineaRecepcion L) Borrador()
    {
        var r = Recepcion.Crear(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 1, 10), null, null, null, new Reloj()).Valor;
        var l = r.AgregarLinea(new DatosLineaRecepcion(Guid.NewGuid(), "Pimiento", EnvaseProductoId: Box)).Valor;
        return (r, l);
    }

    [Fact]
    public void La_pesada_completa_calcula_la_tara_y_se_retara_con_la_version_vigente()
    {
        var (r, l) = Borrador();
        var tara28 = Guid.NewGuid();
        var p = r.AgregarPesadaCompleta(l.Id, 18_000m, 5_000m, [new EnvaseContado(Box, 104, 28m, tara28), new EnvaseContado(Guid.NewGuid(), 26, 20m, null)], "B1").Valor;
        p.TaraKg.Should().Be(5_000m + 2_912m + 520m);
        p.NetoKg.Should().Be(9_568m);
        p.Envases.Should().Be(104, "cuentan los envases del envase de la línea, no los palés de madera");

        var tara33 = Guid.NewGuid();
        r.AplicarTaras(e => e == Box ? (33m, tara33) : null).EsCorrecto.Should().BeTrue("la tara indicada a mano no se retara");
        p.TaraEnvasesKg.Should().Be(104 * 33m + 520m);
        r.EnvasesPesadas.Single(e => e.EnvaseProductoId == Box).TaraEnvaseId.Should().Be(tara33);
        r.AplicarTaras(_ => null).Error.Codigo.Should().Be("tara.falta");
    }

    [Fact]
    public void Los_kilos_se_reparten_entre_los_pales_por_sus_envases_sin_perder_gramos()
    {
        var (r, l) = Borrador();
        r.AgregarPesada(l.Id, 1_000m, 0m, 10, null);
        r.AgregarPaleEntrada(l.Id, "A", Box, 3, null);
        r.AgregarPaleEntrada(l.Id, "B", Box, 3, null);
        r.AgregarPaleEntrada(l.Id, "C", Box, 4, null);
        var kilos = r.KilosPalesEntrada(l.Id);
        kilos.Select(k => k.Kilos).Should().Equal(300m, 300m, 400m);
        r.AgregarPaleEntrada(l.Id, "a", Box, 1, null).Error.Codigo.Should().Be("pale_entrada.repetido");
        r.ErroresConfirmacion().Should().BeEmpty();
    }

    [Fact]
    public void Una_version_nueva_de_tara_cierra_la_anterior_el_dia_antes()
    {
        var t = TaraEnvase.Crear(Guid.NewGuid(), Box, 28m, new DateOnly(2026, 1, 1), null, null).Valor;
        t.CerrarAntesDe(new DateOnly(2026, 2, 1)).EsCorrecto.Should().BeTrue();
        t.Hasta.Should().Be(new DateOnly(2026, 1, 31));
        t.VigenteEl(new DateOnly(2026, 2, 1)).Should().BeFalse();
        t.CerrarAntesDe(new DateOnly(2026, 1, 1)).Error.Codigo.Should().Be("tara.solapada");
        TaraEnvase.Crear(Guid.NewGuid(), Box, -1m, new DateOnly(2026, 1, 1), null, null).Error.Codigo.Should().Be("tara.kilos");
    }

    [Fact]
    public void Los_kilos_de_liquidacion_van_aparte_y_con_motivo()
    {
        var (r, l) = Borrador();
        r.FijarKilosLiquidacion(l.Id, 19_760m, null).Error.Codigo.Should().Be("recepcion.kilos_liquidacion_motivo");
        r.FijarKilosLiquidacion(l.Id, 19_760m, "Contrato").EsCorrecto.Should().BeTrue();
        r.AgregarPesada(l.Id, 16_100m, 5_000m, 104, null);
        r.NetoDe(l.Id).Should().Be(11_100m, "el neto es el de la báscula");
        l.KilosLiquidacion.Should().Be(19_760m);
    }
}
