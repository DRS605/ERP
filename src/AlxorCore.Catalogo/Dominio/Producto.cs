using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Catalogo.Dominio;

/// <summary>Tipo de producto.</summary>
public enum TipoProducto
{
    /// <summary>Bien físico.</summary>
    Bien = 1,

    /// <summary>Servicio.</summary>
    Servicio = 2,
}

/// <summary>Se ha creado un producto.</summary>
public sealed record ProductoCreado(Guid ProductoId, Guid EmpresaId, DateTimeOffset OcurridoEn) : IEventoDominio;

/// <summary>
/// Producto o servicio del catálogo de una empresa. Guarda su precio y el tipo de IVA por defecto,
/// que se prerrellenan al añadirlo a una factura.
/// </summary>
public sealed class Producto : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMaximaNombre = 200;

    private Producto(Guid id)
        : base(id, Guid.Empty)
    {
        Nombre = null!;
        CodigoIva = null!;
        Unidad = null!;
        FactorCompra = 1m;
        FactorVenta = 1m;
    }

    private Producto(Guid id, Guid empresaId, string? referencia, string nombre, TipoProducto tipo, decimal precio, decimal precioCompra, string codigoIva, string unidad, Guid? proveedorHabitualId, bool controlarStock, decimal stockInicial, string? unidadCompra, decimal factorCompra, string? unidadVenta, decimal factorVenta, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Referencia = referencia;
        Nombre = nombre;
        Tipo = tipo;
        PrecioUnitario = precio;
        PrecioCompra = precioCompra;
        CodigoIva = codigoIva;
        Unidad = unidad;
        UnidadCompra = unidadCompra;
        FactorCompra = factorCompra;
        UnidadVenta = unidadVenta;
        FactorVenta = factorVenta;
        ProveedorHabitualId = proveedorHabitualId;
        ControlarStock = controlarStock;
        Stock = controlarStock ? stockInicial : 0m;
        Activo = true;
        CreadoEn = ahora;
        ActualizadoEn = ahora;
    }

    public string? Referencia { get; private set; }

    public string Nombre { get; private set; }

    public TipoProducto Tipo { get; private set; }

    /// <summary>Precio de venta unitario.</summary>
    public decimal PrecioUnitario { get; private set; }

    /// <summary>Precio de compra/coste unitario (para el cálculo de márgenes). 0 si no se conoce.</summary>
    public decimal PrecioCompra { get; private set; }

    /// <summary>Código del IVA por defecto (del catálogo <see cref="Impuesto"/>).</summary>
    public string CodigoIva { get; private set; }

    /// <summary>
    /// Unidad base del artículo: en ella se guardan las existencias y se expresan sus precios. Es la
    /// unidad canónica del sistema (compras, inventario, facturación y —a futuro— producción operan
    /// sobre ella). Las unidades de compra y venta son solo conversiones para entrada/lectura.
    /// </summary>
    public string Unidad { get; private set; }

    /// <summary>Unidad en la que se compra el artículo (p. ej. «caja»). Null = igual que la unidad base.</summary>
    public string? UnidadCompra { get; private set; }

    /// <summary>Cuántas unidades base equivalen a 1 unidad de compra (p. ej. 12 si la caja trae 12 ud). Siempre &gt; 0.</summary>
    public decimal FactorCompra { get; private set; }

    /// <summary>Unidad en la que se vende el artículo (p. ej. «botella»). Null = igual que la unidad base.</summary>
    public string? UnidadVenta { get; private set; }

    /// <summary>Cuántas unidades base equivalen a 1 unidad de venta. Siempre &gt; 0.</summary>
    public decimal FactorVenta { get; private set; }

    /// <summary>Unidad de compra efectiva (la definida o, si no, la base).</summary>
    public string UnidadCompraEfectiva => string.IsNullOrWhiteSpace(UnidadCompra) ? Unidad : UnidadCompra!;

    /// <summary>Unidad de venta efectiva (la definida o, si no, la base).</summary>
    public string UnidadVentaEfectiva => string.IsNullOrWhiteSpace(UnidadVenta) ? Unidad : UnidadVenta!;

    /// <summary>Precio de compra por unidad de compra (precio base × factor de compra), a 2 decimales.</summary>
    public decimal PrecioCompraPorUnidadCompra => Redondeo.Dos(PrecioCompra * FactorCompra);

    /// <summary>Precio de venta por unidad de venta (precio base × factor de venta), a 2 decimales.</summary>
    public decimal PrecioVentaPorUnidadVenta => Redondeo.Dos(PrecioUnitario * FactorVenta);

    /// <summary>Convierte una cantidad expresada en unidades de compra a unidades base.</summary>
    public decimal CompraABase(decimal cantidadCompra) => Math.Round(cantidadCompra * FactorCompra, 3, MidpointRounding.AwayFromZero);

    /// <summary>Convierte una cantidad expresada en unidades de venta a unidades base.</summary>
    public decimal VentaABase(decimal cantidadVenta) => Math.Round(cantidadVenta * FactorVenta, 3, MidpointRounding.AwayFromZero);

    /// <summary>Proveedor habitual del artículo (a quién se le compra normalmente). Referencia opcional a Terceros.</summary>
    public Guid? ProveedorHabitualId { get; private set; }

    /// <summary>Si se llevan existencias de este artículo (los servicios normalmente no).</summary>
    public bool ControlarStock { get; private set; }

    /// <summary>Existencias actuales. Solo tiene sentido si <see cref="ControlarStock"/> es cierto.</summary>
    public decimal Stock { get; private set; }

    public bool Activo { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset ActualizadoEn { get; private set; }

    public static Resultado<Producto> Crear(
        Guid empresaId, string? referencia, string? nombre, TipoProducto tipo, decimal precioUnitario, decimal precioCompra, string? codigoIva, string? unidad, IReloj reloj, Guid? proveedorHabitualId = null, bool controlarStock = false, decimal stockInicial = 0m,
        string? unidadCompra = null, decimal factorCompra = 1m, string? unidadVenta = null, decimal factorVenta = 1m)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        var error = Validar(nombre, precioUnitario, precioCompra, ref codigoIva) ?? ValidarFactores(factorCompra, factorVenta);
        if (error is not null)
        {
            return Resultado.Fallo<Producto>(error);
        }

        var producto = new Producto(
            Guid.NewGuid(), empresaId, Normalizar(referencia), nombre!.Trim(), tipo, precioUnitario, precioCompra, codigoIva!, NormalizarUnidad(unidad), proveedorHabitualId, controlarStock, stockInicial,
            NormalizarUnidadOpcional(unidadCompra), factorCompra, NormalizarUnidadOpcional(unidadVenta), factorVenta, reloj.AhoraUtc);
        producto.RegistrarEvento(new ProductoCreado(producto.Id, empresaId, reloj.AhoraUtc));
        return Resultado.Ok(producto);
    }

    public Resultado Actualizar(string? referencia, string? nombre, TipoProducto tipo, decimal precioUnitario, decimal precioCompra, string? codigoIva, string? unidad, IReloj reloj, Guid? proveedorHabitualId = null, bool controlarStock = false,
        string? unidadCompra = null, decimal factorCompra = 1m, string? unidadVenta = null, decimal factorVenta = 1m)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        var error = Validar(nombre, precioUnitario, precioCompra, ref codigoIva) ?? ValidarFactores(factorCompra, factorVenta);
        if (error is not null)
        {
            return Resultado.Fallo(error);
        }

        Referencia = Normalizar(referencia);
        Nombre = nombre!.Trim();
        Tipo = tipo;
        PrecioUnitario = precioUnitario;
        PrecioCompra = precioCompra;
        ProveedorHabitualId = proveedorHabitualId;
        ControlarStock = controlarStock;
        CodigoIva = codigoIva!;
        Unidad = NormalizarUnidad(unidad);
        UnidadCompra = NormalizarUnidadOpcional(unidadCompra);
        FactorCompra = factorCompra;
        UnidadVenta = NormalizarUnidadOpcional(unidadVenta);
        FactorVenta = factorVenta;
        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    /// <summary>
    /// Registra un movimiento de existencias y actualiza el stock. Requiere que el artículo tenga
    /// el control de stock activado. Un <see cref="TipoMovimientoStock.Ajuste"/> fija el stock al
    /// valor contado; el resto suman o restan la cantidad indicada.
    /// </summary>
    public Resultado<MovimientoStock> RegistrarMovimientoStock(TipoMovimientoStock tipo, decimal cantidad, string? motivo, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        if (!ControlarStock)
        {
            return Resultado.Fallo<MovimientoStock>(Error.Conflicto("producto.sin_control_stock", "Este artículo no lleva control de stock."));
        }

        if (cantidad < 0)
        {
            return Resultado.Fallo<MovimientoStock>(Error.Validacion("stock.cantidad_negativa", "La cantidad no puede ser negativa."));
        }

        var delta = tipo switch
        {
            TipoMovimientoStock.Entrada => cantidad,
            TipoMovimientoStock.Salida => -cantidad,
            TipoMovimientoStock.Venta => -cantidad,
            TipoMovimientoStock.Ajuste => cantidad - Stock,
            _ => 0m,
        };

        Stock += delta;
        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok(MovimientoStock.Registrar(EmpresaId, Id, tipo, delta, Stock, motivo, reloj.AhoraUtc));
    }

    public void Desactivar(IReloj reloj)
    {
        Activo = false;
        ActualizadoEn = reloj.AhoraUtc;
    }

    private static Error? Validar(string? nombre, decimal precio, decimal precioCompra, ref string? codigoIva)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return Error.Validacion("producto.nombre_vacio", "El nombre del producto es obligatorio.");
        }

        if (nombre.Trim().Length > LongitudMaximaNombre)
        {
            return Error.Validacion("producto.nombre_largo", "El nombre del producto es demasiado largo.");
        }

        if (precio < 0)
        {
            return Error.Validacion("producto.precio_negativo", "El precio no puede ser negativo.");
        }

        if (precioCompra < 0)
        {
            return Error.Validacion("producto.precio_compra_negativo", "El precio de compra no puede ser negativo.");
        }

        var codigo = string.IsNullOrWhiteSpace(codigoIva) ? Impuesto.IvaGeneral.Codigo : codigoIva.Trim();
        var impuesto = Impuesto.PorCodigoImpuesto(codigo);
        if (impuesto.EsFallo)
        {
            return impuesto.Error;
        }

        codigoIva = impuesto.Valor.Codigo;
        return null;
    }

    private static Error? ValidarFactores(decimal factorCompra, decimal factorVenta)
    {
        if (factorCompra <= 0m || factorVenta <= 0m)
        {
            return Error.Validacion("producto.factor_invalido", "Los factores de conversión de unidades deben ser mayores que cero.");
        }

        return null;
    }

    private static string? Normalizar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static string NormalizarUnidad(string? unidad) => string.IsNullOrWhiteSpace(unidad) ? "ud" : unidad.Trim();

    private static string? NormalizarUnidadOpcional(string? unidad) => string.IsNullOrWhiteSpace(unidad) ? null : unidad.Trim();
}
