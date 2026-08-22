using AlxorCore.Catalogo.Dominio;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Tiempo;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Catalogo.Tests;

public sealed class RelojFijo : IReloj
{
    public DateTimeOffset AhoraUtc { get; init; } = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
}

public class ImpuestoTests
{
    [Theory]
    [InlineData("IVA21", 21)]
    [InlineData("IVA10", 10)]
    [InlineData("IVA4", 4)]
    [InlineData("IVA0", 0)]
    public void PorCodigo_resuelve_los_tipos_de_iva(string codigo, decimal porcentaje)
    {
        var impuesto = Impuesto.PorCodigoImpuesto(codigo);
        impuesto.EsCorrecto.Should().BeTrue();
        impuesto.Valor.Porcentaje.Should().Be(porcentaje);
    }

    [Fact]
    public void PorCodigo_falla_con_codigo_desconocido()
    {
        Impuesto.PorCodigoImpuesto("IVA99").EsFallo.Should().BeTrue();
    }

    [Theory]
    [InlineData(21, 5.2)]
    [InlineData(10, 1.4)]
    [InlineData(4, 0.5)]
    [InlineData(0, 0)]
    public void Recargo_de_equivalencia_por_tipo_de_iva(decimal iva, decimal recargo)
    {
        Impuesto.RecargoEquivalencia(iva).Should().Be(recargo);
    }
}

public class ProductoTests
{
    private static readonly IReloj Reloj = new RelojFijo();
    private static readonly Guid Empresa = Guid.NewGuid();

    [Fact]
    public void Crear_producto_valido_usa_iva_general_por_defecto()
    {
        var producto = Producto.Crear(Empresa, "REF1", "Servicio de consultoría", TipoProducto.Servicio, 100m, 40m, null, null, Reloj);

        producto.EsCorrecto.Should().BeTrue();
        producto.Valor.CodigoIva.Should().Be("IVA21");
        producto.Valor.Unidad.Should().Be("ud");
        producto.Valor.PrecioCompra.Should().Be(40m);
        producto.Valor.EventosDominio.Should().ContainSingle(e => e is ProductoCreado);
    }

    [Fact]
    public void Crear_producto_con_iva_reducido()
    {
        var producto = Producto.Crear(Empresa, null, "Libro", TipoProducto.Bien, 20m, 0m, "IVA4", "ud", Reloj);
        producto.Valor.CodigoIva.Should().Be("IVA4");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Crear_rechaza_nombre_vacio(string? nombre)
    {
        Producto.Crear(Empresa, null, nombre, TipoProducto.Servicio, 10m, 0m, null, null, Reloj).EsFallo.Should().BeTrue();
    }

    [Fact]
    public void Crear_rechaza_precio_negativo()
    {
        Producto.Crear(Empresa, null, "X", TipoProducto.Servicio, -1m, 0m, null, null, Reloj).EsFallo.Should().BeTrue();
    }

    [Fact]
    public void Crear_rechaza_precio_compra_negativo()
    {
        Producto.Crear(Empresa, null, "X", TipoProducto.Servicio, 10m, -5m, null, null, Reloj).EsFallo.Should().BeTrue();
    }

    [Fact]
    public void Crear_rechaza_iva_desconocido()
    {
        Producto.Crear(Empresa, null, "X", TipoProducto.Servicio, 10m, 0m, "IVA99", null, Reloj).EsFallo.Should().BeTrue();
    }

    [Fact]
    public void Unidades_por_defecto_son_la_base_con_factor_uno()
    {
        var p = Producto.Crear(Empresa, null, "Tornillo", TipoProducto.Bien, 0.5m, 0.2m, null, "ud", Reloj).Valor;
        p.FactorCompra.Should().Be(1m);
        p.FactorVenta.Should().Be(1m);
        p.UnidadCompraEfectiva.Should().Be("ud");
        p.UnidadVentaEfectiva.Should().Be("ud");
        p.CompraABase(3m).Should().Be(3m);
    }

    [Fact]
    public void Compra_en_cajas_convierte_a_unidades_base()
    {
        // Se compra en cajas de 12 ud; el precio base es por unidad.
        var p = Producto.Crear(Empresa, null, "Refresco", TipoProducto.Bien, 0.9m, 0.5m, null, "ud", Reloj,
            unidadCompra: "caja", factorCompra: 12m).Valor;

        p.UnidadCompraEfectiva.Should().Be("caja");
        p.CompraABase(2m).Should().Be(24m);                 // 2 cajas → 24 ud
        p.PrecioCompraPorUnidadCompra.Should().Be(6m);      // 0,5 €/ud × 12
    }

    [Fact]
    public void Venta_partida_convierte_a_unidades_base()
    {
        // Se compra en garrafas pero se vende por litro (unidad base = litro).
        var p = Producto.Crear(Empresa, null, "Aceite", TipoProducto.Bien, 4m, 3m, null, "l", Reloj,
            unidadVenta: "garrafa", factorVenta: 5m).Valor;

        p.VentaABase(2m).Should().Be(10m);                  // 2 garrafas → 10 l
        p.PrecioVentaPorUnidadVenta.Should().Be(20m);       // 4 €/l × 5
    }

    [Fact]
    public void Rechaza_factor_de_conversion_no_positivo()
    {
        Producto.Crear(Empresa, null, "X", TipoProducto.Bien, 1m, 0m, null, "ud", Reloj, factorCompra: 0m).EsFallo.Should().BeTrue();
        Producto.Crear(Empresa, null, "X", TipoProducto.Bien, 1m, 0m, null, "ud", Reloj, factorVenta: -1m).EsFallo.Should().BeTrue();
    }

    [Fact]
    public void Actualizar_cambia_las_unidades_y_factores()
    {
        var p = Producto.Crear(Empresa, null, "Papel", TipoProducto.Bien, 1m, 0.6m, null, "ud", Reloj).Valor;
        p.Actualizar(null, "Papel", TipoProducto.Bien, 1m, 0.6m, null, "ud", Reloj, unidadCompra: "palé", factorCompra: 500m).EsCorrecto.Should().BeTrue();
        p.UnidadCompraEfectiva.Should().Be("palé");
        p.CompraABase(1m).Should().Be(500m);
    }

    [Fact]
    public void Seguimiento_por_defecto_es_ninguno_y_se_puede_fijar()
    {
        var p = Producto.Crear(Empresa, null, "Genérico", TipoProducto.Bien, 1m, 0m, null, "ud", Reloj).Valor;
        p.Seguimiento.Should().Be(SeguimientoArticulo.Ninguno);
        p.RequiereLoteOSerie.Should().BeFalse();

        var s = Producto.Crear(Empresa, null, "Vacuna", TipoProducto.Bien, 1m, 0m, null, "ud", Reloj, seguimiento: SeguimientoArticulo.Lote).Valor;
        s.Seguimiento.Should().Be(SeguimientoArticulo.Lote);
        s.RequiereLoteOSerie.Should().BeTrue();

        p.Actualizar(null, "Portátil", TipoProducto.Bien, 1m, 0m, null, "ud", Reloj, seguimiento: SeguimientoArticulo.Serie).EsCorrecto.Should().BeTrue();
        p.Seguimiento.Should().Be(SeguimientoArticulo.Serie);
    }

    [Fact]
    public void Composicion_define_lista_de_materiales_y_explosiona()
    {
        var compuesto = Producto.Crear(Empresa, null, "Lote de bienvenida", TipoProducto.Bien, 30m, 0m, null, "ud", Reloj).Valor;
        var c1 = Guid.NewGuid(); var c2 = Guid.NewGuid();
        compuesto.DefinirComposicion(new[] { (c1, 2m), (c2, 3m) }, Reloj).EsCorrecto.Should().BeTrue();
        compuesto.EsCompuesto.Should().BeTrue();
        compuesto.Componentes.Should().HaveCount(2);

        var exp = compuesto.Explosionar(5m);
        exp.Single(e => e.ComponenteId == c1).Cantidad.Should().Be(10m); // 2 × 5
        exp.Single(e => e.ComponenteId == c2).Cantidad.Should().Be(15m); // 3 × 5

        compuesto.QuitarComposicion(Reloj);
        compuesto.EsCompuesto.Should().BeFalse();
        compuesto.Componentes.Should().BeEmpty();
    }

    [Fact]
    public void Composicion_valida_reglas()
    {
        var p = Producto.Crear(Empresa, null, "Compuesto", TipoProducto.Bien, 10m, 0m, null, "ud", Reloj).Valor;
        p.DefinirComposicion(Array.Empty<(Guid, decimal)>(), Reloj).EsFallo.Should().BeTrue();               // vacía
        p.DefinirComposicion(new[] { (Guid.NewGuid(), 0m) }, Reloj).EsFallo.Should().BeTrue();               // cantidad 0
        p.DefinirComposicion(new[] { (p.Id, 1m) }, Reloj).EsFallo.Should().BeTrue();                         // autorreferencia
        var dup = Guid.NewGuid();
        p.DefinirComposicion(new[] { (dup, 1m), (dup, 2m) }, Reloj).EsFallo.Should().BeTrue();               // duplicado
    }

    [Fact]
    public void Variante_hereda_del_padre_y_resume_sus_atributos()
    {
        var padre = Producto.Crear(Empresa, null, "Camiseta", TipoProducto.Bien, 15m, 6m, "IVA21", "ud", Reloj).Valor;
        var v = Producto.Crear(Empresa, "CAM-M-ROJO", "Camiseta M / Rojo", padre.Tipo, 15m, 6m, padre.CodigoIva, padre.Unidad, Reloj).Valor;
        v.AsignarComoVariante(padre.Id, new[] { ("Talla", "M"), ("Color", "Rojo") });

        v.EsVariante.Should().BeTrue();
        v.ProductoPadreId.Should().Be(padre.Id);
        v.Atributos.Should().HaveCount(2);
        v.ResumenVariante.Should().Be("M · Rojo");

        padre.MarcarPlantilla(true, Reloj);
        padre.EsPlantilla.Should().BeTrue();
        padre.EsVariante.Should().BeFalse();
    }

    [Fact]
    public void Producto_sin_control_de_stock_no_admite_movimientos()
    {
        var producto = Producto.Crear(Empresa, null, "Servicio", TipoProducto.Servicio, 10m, 0m, null, null, Reloj).Valor;
        var mov = producto.RegistrarMovimientoStock(TipoMovimientoStock.Entrada, 5m, null, Reloj);
        mov.EsFallo.Should().BeTrue();
    }

    [Fact]
    public void Entrada_y_salida_de_stock_actualizan_las_existencias()
    {
        var producto = Producto.Crear(Empresa, null, "Café 1kg", TipoProducto.Bien, 10m, 6m, null, null, Reloj, controlarStock: true, stockInicial: 20m).Valor;
        producto.Stock.Should().Be(20m);

        var entrada = producto.RegistrarMovimientoStock(TipoMovimientoStock.Entrada, 30m, "Compra", Reloj);
        entrada.EsCorrecto.Should().BeTrue();
        entrada.Valor.Cantidad.Should().Be(30m);
        producto.Stock.Should().Be(50m);

        var venta = producto.RegistrarMovimientoStock(TipoMovimientoStock.Venta, 12m, null, Reloj);
        venta.Valor.Cantidad.Should().Be(-12m);
        producto.Stock.Should().Be(38m);
    }

    [Fact]
    public void Ajuste_fija_el_stock_al_valor_contado()
    {
        var producto = Producto.Crear(Empresa, null, "Harina", TipoProducto.Bien, 2m, 1m, null, null, Reloj, controlarStock: true, stockInicial: 100m).Valor;
        var ajuste = producto.RegistrarMovimientoStock(TipoMovimientoStock.Ajuste, 90m, "Recuento", Reloj);
        ajuste.Valor.Cantidad.Should().Be(-10m);       // delta aplicado
        ajuste.Valor.StockResultante.Should().Be(90m);
        producto.Stock.Should().Be(90m);
    }
}
