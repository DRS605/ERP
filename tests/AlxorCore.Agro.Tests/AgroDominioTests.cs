using AlxorCore.Agro.Dominio;
using AlxorCore.Nucleo.Tiempo;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Agro.Tests;

public sealed class RelojFijo : IReloj
{
    public DateTimeOffset AhoraUtc { get; init; } = new(2026, 10, 15, 10, 0, 0, TimeSpan.Zero);
}

public class ReglasAgroTests
{
    [Fact]
    public void El_reparto_no_pierde_ni_un_gramo()
    {
        var partes = ReglasAgro.Repartir(1000m, [1m, 1m, 1m], 3);
        partes.Sum().Should().Be(1000m);
        partes.Should().Equal(333.334m, 333.333m, 333.333m);

        ReglasAgro.Repartir(100m, [33.33m, 33.33m, 33.34m], 2).Should().Equal(33.33m, 33.33m, 33.34m);
        ReglasAgro.Repartir(10m, [0m, 0m], 2).Should().Equal(0m, 0m);
    }

    [Theory]
    [InlineData("084000000000000017", true)]
    [InlineData("084000000000000018", false)]
    [InlineData("8400000000017", true)]
    [InlineData("12345abc", false)]
    public void Valida_el_digito_de_control_gs1(string codigo, bool valido) => ReglasAgro.Gs1Valido(codigo).Should().Be(valido);

    [Fact]
    public void Genera_sscc_con_su_digito_de_control()
    {
        var config = ConfiguracionAgro.Crear(Guid.NewGuid());
        config.Actualizar("8412345", 3).EsCorrecto.Should().BeTrue();
        var sscc = config.Sscc(1).Valor;
        sscc.Should().HaveLength(18).And.StartWith("38412345");
        ReglasAgro.Gs1Valido(sscc).Should().BeTrue();
        config.Sscc(1_000_000_000).EsFallo.Should().BeTrue("un prefijo de 7 dígitos deja 9 para la serie");
    }

    [Fact]
    public void La_referencia_sigpac_tiene_siete_campos()
    {
        var ok = Parcela.Crear(Guid.NewGuid(), Guid.NewGuid(), new DatosParcela("P1", "Finca", "30:15:0:0:12:45:1", 2.5m));
        ok.EsCorrecto.Should().BeTrue();
        Parcela.Crear(Guid.NewGuid(), Guid.NewGuid(), new DatosParcela("P2", "Finca", "30-15-12")).Error.Codigo.Should().Be("parcela.sigpac");
    }

    [Fact]
    public void En_el_reagp_la_autofactura_lleva_la_compensacion()
    {
        var reagp = Agricultor.Crear(Guid.NewGuid(), Guid.NewGuid(), "Juan", RegimenAgricultor.Reagp, 2m, null).Valor;
        reagp.CodigoImpuesto.Should().Be("REAGP12");
        var general = Agricultor.Crear(Guid.NewGuid(), Guid.NewGuid(), "Ana", RegimenAgricultor.General, 2m, null).Valor;
        general.CodigoImpuesto.Should().Be("IVA4");
        Agricultor.Crear(Guid.NewGuid(), Guid.NewGuid(), "X", RegimenAgricultor.Reagp, 2m, null, "IVA4").Error.Codigo.Should().Be("agricultor.impuesto");
        Agricultor.Crear(Guid.NewGuid(), Guid.NewGuid(), "X", RegimenAgricultor.General, 2m, null, "REAGP12").Error.Codigo.Should().Be("agricultor.impuesto");
    }
}

public class RecepcionTests
{
    private static readonly IReloj Reloj = new RelojFijo();

    [Fact]
    public void Suma_las_pesadas_y_devuelve_todos_los_errores_a_la_vez()
    {
        var r = Recepcion.Crear(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 10, 15), "1234 abc", null, null, Reloj).Valor;
        r.Matricula.Should().Be("1234 ABC");
        var l1 = r.AgregarLinea(new DatosLineaRecepcion(Guid.NewGuid(), "Naranja")).Valor;
        var l2 = r.AgregarLinea(new DatosLineaRecepcion(Guid.NewGuid(), "Limón")).Valor;
        r.AgregarPesada(l1.Id, 12_500m, 2_300m, 20, "B1").EsCorrecto.Should().BeTrue();
        r.AgregarPesada(l1.Id, 8_000m, 1_000m, 10, "B1").EsCorrecto.Should().BeTrue();
        r.AgregarPesada(l1.Id, 1_000m, 1_000m, 0, null).Error.Codigo.Should().Be("pesada.kilos");

        r.NetoDe(l1.Id).Should().Be(17_200m);
        r.EnvasesDe(l1.Id).Should().Be(30);

        var errores = r.ErroresConfirmacion().Select(e => e.Codigo).ToList();
        errores.Should().BeEquivalentTo(["recepcion.envase", "recepcion.sin_pesadas"], "la línea 1 trae envases sin artículo y la 2 no tiene pesadas");
        r.AgregarLinea(new DatosLineaRecepcion(Guid.NewGuid(), "Pomelo", FechaRecoleccion: new DateOnly(2026, 10, 16))).Error.Codigo.Should().Be("recepcion.fecha_recoleccion");
        _ = l2;
    }

    [Fact]
    public void Una_recepcion_confirmada_no_se_modifica()
    {
        var r = Recepcion.Crear(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 10, 15), null, null, null, Reloj).Valor;
        var l = r.AgregarLinea(new DatosLineaRecepcion(Guid.NewGuid(), "Naranja")).Valor;
        r.AgregarPesada(l.Id, 1_000m, 100m, 0, null);
        r.Confirmar(7, new Dictionary<Guid, Guid> { [l.Id] = Guid.NewGuid() }, Reloj).EsCorrecto.Should().BeTrue();
        r.NumeroCompleto.Should().Be("REC-2026-000007");
        l.NetoKg.Should().Be(900m);
        r.AgregarPesada(l.Id, 500m, 0m, 0, null).Error.Codigo.Should().Be("recepcion.no_borrador");
        r.Anular(" ", Reloj).Error.Codigo.Should().Be("recepcion.motivo");
        r.Anular("Error de báscula", Reloj).EsCorrecto.Should().BeTrue();
    }
}

public class ValoracionTests
{
    private sealed class Precios : IPreciosLiquidacion
    {
        public Dictionary<Guid, MetodoLiquidacion> Metodos { get; } = [];
        public Dictionary<Guid, IReadOnlyList<MuestraCategoria>> Muestras { get; } = [];
        public List<(Guid Producto, Guid? Categoria, DateOnly Desde, DateOnly Hasta, decimal Precio)> Tabla { get; } = [];

        public MetodoLiquidacion? Metodo(Guid productoId) => Metodos.TryGetValue(productoId, out var m) ? m : null;

        public IReadOnlyList<MuestraCategoria>? Clasificacion(Guid partidaId) => Muestras.GetValueOrDefault(partidaId);

        public PrecioAplicable? Precio(Guid productoId, Guid? categoriaId, DateOnly fecha) =>
            Tabla.Where(t => t.Producto == productoId && t.Categoria == categoriaId && fecha >= t.Desde && fecha <= t.Hasta)
                .Select(t => new PrecioAplicable(Guid.NewGuid(), t.Precio)).FirstOrDefault();
    }

    private static readonly DateOnly Dia = new(2026, 10, 15);

    [Fact]
    public void Liquida_por_clasificacion_con_descuentos_compensacion_y_retencion()
    {
        var naranja = Guid.NewGuid();
        var (extra, primera, destrio) = (Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var partida = Guid.NewGuid();
        var precios = new Precios();
        precios.Metodos[naranja] = MetodoLiquidacion.PorClasificacion;
        precios.Muestras[partida] = [new(extra, "Extra", 60m), new(primera, "Primera", 30m), new(destrio, "Destrío", 10m)];
        precios.Tabla.Add((naranja, extra, Dia, Dia, 0.40m));
        precios.Tabla.Add((naranja, primera, Dia, Dia, 0.25m));
        precios.Tabla.Add((naranja, destrio, Dia, Dia, 0m));
        var transporte = ConceptoLiquidacion.Crear(Guid.NewGuid(), "TRANS", "Transporte", TipoConceptoLiquidacion.PorKilo, 0.01m).Valor;
        var cuota = ConceptoLiquidacion.Crear(Guid.NewGuid(), "CUOTA", "Cuota", TipoConceptoLiquidacion.Fijo, 5m).Valor;

        var r = Valoracion.Calcular([new LineaALiquidar(Guid.NewGuid(), Guid.NewGuid(), partida, "REC-1", naranja, Dia, 10_000.001m)],
            [transporte, cuota], 12m, 2m, precios, out var errores);

        errores.Should().BeEmpty();
        var v = r.Valor;
        v.Lineas.Sum(l => l.Kilos).Should().Be(10_000.001m, "el reparto por categorías no pierde gramos");
        v.Lineas.Should().HaveCount(3);
        v.Bruto.Should().Be(2_400m + 750m, "6.000 kg × 0,40 + 3.000 kg × 0,25; el destrío a 0");
        v.TotalDescuentos.Should().Be(100m + 5m);
        v.Base.Should().Be(3_045m);
        v.CuotaImpuesto.Should().Be(365.40m);
        v.Retencion.Should().Be(60.90m);
        v.TotalFactura.Should().Be(3_410.40m);
        v.APagar.Should().Be(3_349.50m);
    }

    [Fact]
    public void Sin_precio_o_sin_clasificacion_es_un_error_y_salen_todos_a_la_vez()
    {
        var (naranja, limon, pomelo) = (Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var precios = new Precios();
        precios.Metodos[naranja] = MetodoLiquidacion.PorClasificacion;
        precios.Metodos[limon] = MetodoLiquidacion.PorPeriodo;

        var r = Valoracion.Calcular(
            [
                new LineaALiquidar(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "A", naranja, Dia, 100m),
                new LineaALiquidar(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "B", limon, Dia, 100m),
                new LineaALiquidar(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "C", pomelo, Dia, 100m),
            ], [], 12m, 2m, precios, out var errores);

        r.EsFallo.Should().BeTrue();
        errores.Select(e => e.Codigo).Should().Equal("liquidacion.sin_clasificacion", "liquidacion.sin_precio", "liquidacion.metodo");
    }

    [Fact]
    public void Los_descuentos_no_pueden_superar_la_fruta()
    {
        var limon = Guid.NewGuid();
        var precios = new Precios();
        precios.Metodos[limon] = MetodoLiquidacion.PorPeriodo;
        precios.Tabla.Add((limon, null, Dia, Dia, 0.01m));
        var caro = ConceptoLiquidacion.Crear(Guid.NewGuid(), "X", "X", TipoConceptoLiquidacion.Fijo, 50m).Valor;
        Valoracion.Calcular([new LineaALiquidar(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "A", limon, Dia, 100m)], [caro], 12m, 0m, precios, out _)
            .Error.Codigo.Should().Be("liquidacion.base_negativa");
    }
}

public class ParteConfeccionTests
{
    private static readonly IReloj Reloj = new RelojFijo();
    private static readonly DateOnly Dia = new(2026, 10, 15);

    private static ParteConfeccion Parte(RepartoCoste reparto, params DatosManoObra[] manoObra)
    {
        var p = ParteConfeccion.Crear(Guid.NewGuid(), Dia, Reloj);
        p.Fijar(new DatosParte(Dia, null, "Línea 1", null, 10m, reparto,
            [new DatosConsumo(Guid.NewGuid(), 1_000m)], manoObra, [new DatosMaquina("Calibradora", "CALIBRADORA", 2m)],
            [new DatosMaterial(Guid.NewGuid(), "Caja", 100m)],
            [new DatosSalida(Guid.NewGuid(), "Naranja 1ª", 800m, 1m), new DatosSalida(Guid.NewGuid(), "Destrío", 150m, 0m)])).EsCorrecto.Should().BeTrue();
        return p;
    }

    private static List<TarifaCoste> Tarifas() =>
    [
        TarifaCoste.Crear(Guid.NewGuid(), RecursoCoste.ManoObra, "PEON", TipoHora.Normal, new DateOnly(2026, 1, 1), null, 12m).Valor,
        TarifaCoste.Crear(Guid.NewGuid(), RecursoCoste.ManoObra, "PEON", TipoHora.Destajo, new DateOnly(2026, 1, 1), null, 0.08m).Valor,
        TarifaCoste.Crear(Guid.NewGuid(), RecursoCoste.Maquina, "CALIBRADORA", TipoHora.Normal, new DateOnly(2026, 1, 1), null, 25m).Valor,
    ];

    [Fact]
    public void Valora_con_destajo_indirectos_y_reparte_por_factor()
    {
        var p = Parte(RepartoCoste.PorFactor, new DatosManoObra("Cuadrilla A", "peon", TipoHora.Normal, 8m), new DatosManoObra("Destajistas", "PEON", TipoHora.Destajo, 6m, 400m));
        var errores = p.Valorar(Tarifas(), _ => 0.30m, _ => 0.45m);

        errores.Should().BeEmpty();
        p.CosteFruta.Should().Be(300m);
        p.CosteManoObra.Should().Be(96m + 32m, "8 h × 12 € + 400 cajas × 0,08 € (a destajo se paga por pieza, no por hora)");
        p.CosteMaquinaria.Should().Be(50m);
        p.CosteMateriales.Should().Be(45m);
        p.CosteIndirectos.Should().Be(17.80m, "10 % de mano de obra y maquinaria");
        p.CosteTotal.Should().Be(540.80m);
        p.Salidas.Sum(s => s.Coste).Should().Be(p.CosteTotal);
        p.Salidas[1].Coste.Should().Be(0m, "el destrío tiene factor 0");
        p.Salidas[0].CosteKg.Should().Be(0.676m, "540,80 € entre 800 kg");
        p.Merma.Should().Be(50m);
    }

    [Fact]
    public void Sin_tarifa_o_sin_coste_nunca_valora_a_cero()
    {
        var p = Parte(RepartoCoste.PorKilos, new DatosManoObra("Encargado", "ENCARGADO", TipoHora.Normal, 8m), new DatosManoObra("Destajo sin piezas", "PEON", TipoHora.Destajo, 4m));
        var errores = p.Valorar(Tarifas(), _ => null, _ => null).Select(e => e.Codigo).ToList();
        errores.Should().BeEquivalentTo(["parte.coste_fruta", "parte.sin_tarifa", "parte.piezas", "parte.coste_material"]);
    }
}
