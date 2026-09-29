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
