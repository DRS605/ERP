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

/// <summary>Cómo se traza el stock de un artículo.</summary>
public enum SeguimientoArticulo
{
    /// <summary>Sin trazabilidad por unidad: solo cantidades.</summary>
    Ninguno = 0,

    /// <summary>Por lote (varias unidades comparten un código de lote; útil para caducidades).</summary>
    Lote = 1,

    /// <summary>Por número de serie (cada unidad es única).</summary>
    Serie = 2,
}

/// <summary>Se ha creado un producto.</summary>
public sealed record ProductoCreado(Guid ProductoId, Guid GrupoId, DateTimeOffset OcurridoEn) : IEventoDominio;

/// <summary>
/// Componente de la lista de materiales de un artículo compuesto: qué artículo entra y en qué
/// cantidad (en la unidad base del componente) para fabricar una unidad del compuesto.
/// </summary>
public sealed class ComponenteArticulo
{
    private ComponenteArticulo() { }

    internal ComponenteArticulo(Guid id, Guid componenteId, decimal cantidad)
    {
        Id = id;
        ComponenteId = componenteId;
        Cantidad = cantidad;
    }

    public Guid Id { get; private set; }

    /// <summary>Artículo que actúa como componente.</summary>
    public Guid ComponenteId { get; private set; }

    /// <summary>Cantidad de componente (en su unidad base) por unidad del artículo compuesto.</summary>
    public decimal Cantidad { get; private set; }
}

/// <summary>Valor de un eje de variante (p. ej. Talla=M, Color=Rojo) de un artículo.</summary>
public sealed class AtributoVariante
{
    private AtributoVariante() { Nombre = null!; Valor = null!; }

    internal AtributoVariante(Guid id, string nombre, string valor)
    {
        Id = id;
        Nombre = nombre;
        Valor = valor;
    }

    public Guid Id { get; private set; }

    /// <summary>Nombre del eje (p. ej. «Talla»).</summary>
    public string Nombre { get; private set; }

    /// <summary>Valor del eje (p. ej. «M»).</summary>
    public string Valor { get; private set; }
}

/// <summary>
/// Producto o servicio del catálogo. Pertenece al <b>grupo</b> (holding): un artículo creado una vez
/// vale para todas las empresas del grupo (definición compartida: nombre, precio, IVA, familia,
/// unidades, composición, variantes…). Las <b>existencias</b> son por empresa (véase
/// <see cref="ExistenciaSimple"/> y el módulo de Inventario). Guarda su precio y el tipo de IVA por
/// defecto, que se prerrellenan al añadirlo a una factura.
/// </summary>
public sealed class Producto : RaizAgregadoGrupo<Guid>
{
    public const int LongitudMaximaNombre = 200;

    private readonly List<ComponenteArticulo> _componentes = new();
    private readonly List<AtributoVariante> _atributos = new();

    private Producto(Guid id)
        : base(id, Guid.Empty)
    {
        Nombre = null!;
        CodigoIva = null!;
        Unidad = null!;
        FactorCompra = 1m;
        FactorVenta = 1m;
    }

    private Producto(Guid id, Guid grupoId, string? referencia, string nombre, TipoProducto tipo, decimal precio, decimal precioCompra, string codigoIva, string unidad, Guid? proveedorHabitualId, bool controlarStock, string? unidadCompra, decimal factorCompra, string? unidadVenta, decimal factorVenta, SeguimientoArticulo seguimiento, DateTimeOffset ahora)
        : base(id, grupoId)
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
        Seguimiento = seguimiento;
        ProveedorHabitualId = proveedorHabitualId;
        ControlarStock = controlarStock;
        Activo = true;
        CreadoEn = ahora;
        ActualizadoEn = ahora;
    }

    public const int LongitudMaximaFamilia = 80;

    public string? Referencia { get; private set; }

    public string Nombre { get; private set; }

    /// <summary>
    /// Familia o categoría del artículo (p. ej. «Mercaderías», «Servicios», «Suministros»). Sirve para
    /// elegir la cuenta contable de ingreso/gasto mediante reglas de contabilización. Null = sin familia.
    /// Cuando el artículo está enlazado a una <see cref="FamiliaId"/>, este texto refleja el nombre de
    /// esa familia; si no, es texto libre (compatibilidad con importaciones antiguas).
    /// </summary>
    public string? Familia { get; private set; }

    /// <summary>Familia del catálogo a la que pertenece el artículo (árbol de familias). Null = sin familia.</summary>
    public Guid? FamiliaId { get; private set; }

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

    /// <summary>Modo de trazabilidad del stock (ninguno, por lote o por número de serie).</summary>
    public SeguimientoArticulo Seguimiento { get; private set; }

    /// <summary>El artículo requiere indicar lote o número de serie en sus movimientos de stock.</summary>
    public bool RequiereLoteOSerie => Seguimiento != SeguimientoArticulo.Ninguno;

    /// <summary>El artículo se fabrica a partir de otros (tiene lista de materiales).</summary>
    public bool EsCompuesto { get; private set; }

    /// <summary>Lista de materiales (componentes) del artículo compuesto.</summary>
    public IReadOnlyList<ComponenteArticulo> Componentes => _componentes;

    /// <summary>
    /// Define la lista de materiales del artículo (lo convierte en compuesto). Cada componente es
    /// otro artículo con una cantidad &gt; 0; no puede incluirse a sí mismo.
    /// </summary>
    public Resultado DefinirComposicion(IReadOnlyList<(Guid ComponenteId, decimal Cantidad)> componentes, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        ArgumentNullException.ThrowIfNull(componentes);

        if (componentes.Count == 0)
        {
            return Resultado.Fallo(Error.Validacion("composicion.vacia", "Un artículo compuesto necesita al menos un componente."));
        }

        foreach (var (componenteId, cantidad) in componentes)
        {
            if (componenteId == Id)
            {
                return Resultado.Fallo(Error.Validacion("composicion.autorreferencia", "Un artículo no puede ser componente de sí mismo."));
            }

            if (cantidad <= 0m)
            {
                return Resultado.Fallo(Error.Validacion("composicion.cantidad", "La cantidad de cada componente debe ser mayor que cero."));
            }
        }

        if (componentes.Select(c => c.ComponenteId).Distinct().Count() != componentes.Count)
        {
            return Resultado.Fallo(Error.Validacion("composicion.duplicado", "Un componente no puede repetirse; suma la cantidad."));
        }

        _componentes.Clear();
        foreach (var (componenteId, cantidad) in componentes)
        {
            _componentes.Add(new ComponenteArticulo(Guid.NewGuid(), componenteId, cantidad));
        }

        EsCompuesto = true;
        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    /// <summary>Elimina la lista de materiales (deja de ser compuesto).</summary>
    public void QuitarComposicion(IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        _componentes.Clear();
        EsCompuesto = false;
        ActualizadoEn = reloj.AhoraUtc;
    }

    /// <summary>Explosiona la lista de materiales: componentes y cantidades necesarias para fabricar <paramref name="cantidad"/> unidades.</summary>
    public IReadOnlyList<(Guid ComponenteId, decimal Cantidad)> Explosionar(decimal cantidad) =>
        _componentes.Select(c => (c.ComponenteId, Math.Round(c.Cantidad * cantidad, 3, MidpointRounding.AwayFromZero))).ToList();

    /// <summary>Artículo «plantilla» del que este es variante (null si no lo es).</summary>
    public Guid? ProductoPadreId { get; private set; }

    /// <summary>Plantilla de variantes: no se vende directamente, agrupa a sus variantes.</summary>
    public bool EsPlantilla { get; private set; }

    /// <summary>Este artículo es una variante de otro.</summary>
    public bool EsVariante => ProductoPadreId is not null;

    /// <summary>Ejes de la variante (p. ej. Talla=M, Color=Rojo).</summary>
    public IReadOnlyList<AtributoVariante> Atributos => _atributos;

    /// <summary>Resumen legible de los atributos («M · Rojo»); vacío si no es variante.</summary>
    public string ResumenVariante => string.Join(" · ", _atributos.Select(a => a.Valor));

    /// <summary>Convierte este artículo en variante de <paramref name="padreId"/> con sus atributos.</summary>
    public void AsignarComoVariante(Guid padreId, IReadOnlyList<(string Nombre, string Valor)> atributos)
    {
        ProductoPadreId = padreId;
        _atributos.Clear();
        foreach (var (nombre, valor) in atributos)
        {
            _atributos.Add(new AtributoVariante(Guid.NewGuid(), nombre.Trim(), valor.Trim()));
        }
    }

    /// <summary>Marca (o desmarca) el artículo como plantilla de variantes.</summary>
    public void MarcarPlantilla(bool esPlantilla, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        EsPlantilla = esPlantilla;
        ActualizadoEn = reloj.AhoraUtc;
    }

    /// <summary>Proveedor habitual del artículo (a quién se le compra normalmente). Referencia opcional a Terceros.</summary>
    public Guid? ProveedorHabitualId { get; private set; }

    /// <summary>Si se llevan existencias de este artículo (los servicios normalmente no).</summary>
    public bool ControlarStock { get; private set; }

    /// <summary>
    /// Actividad de negocio (línea/división del grupo) con la que se clasifica el artículo. Segmenta
    /// el catálogo y controla qué usuarios lo ven en Artículos. Null = sin actividad (visible a todos).
    /// </summary>
    public Guid? ActividadNegocioId { get; private set; }

    public bool Activo { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset ActualizadoEn { get; private set; }

    public static Resultado<Producto> Crear(
        Guid grupoId, string? referencia, string? nombre, TipoProducto tipo, decimal precioUnitario, decimal precioCompra, string? codigoIva, string? unidad, IReloj reloj, Guid? proveedorHabitualId = null, bool controlarStock = false,
        string? unidadCompra = null, decimal factorCompra = 1m, string? unidadVenta = null, decimal factorVenta = 1m, SeguimientoArticulo seguimiento = SeguimientoArticulo.Ninguno)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        var error = Validar(nombre, precioUnitario, precioCompra, ref codigoIva) ?? ValidarFactores(factorCompra, factorVenta);
        if (error is not null)
        {
            return Resultado.Fallo<Producto>(error);
        }

        var producto = new Producto(
            Guid.NewGuid(), grupoId, Normalizar(referencia), nombre!.Trim(), tipo, precioUnitario, precioCompra, codigoIva!, NormalizarUnidad(unidad), proveedorHabitualId, controlarStock,
            NormalizarUnidadOpcional(unidadCompra), factorCompra, NormalizarUnidadOpcional(unidadVenta), factorVenta, seguimiento, reloj.AhoraUtc);
        producto.RegistrarEvento(new ProductoCreado(producto.Id, grupoId, reloj.AhoraUtc));
        return Resultado.Ok(producto);
    }

    /// <summary>Clasifica el artículo en una actividad de negocio (null o vacío = sin actividad).</summary>
    public void EstablecerActividad(Guid? actividadNegocioId) =>
        ActividadNegocioId = actividadNegocioId is { } a && a != Guid.Empty ? a : null;

    public Resultado Actualizar(string? referencia, string? nombre, TipoProducto tipo, decimal precioUnitario, decimal precioCompra, string? codigoIva, string? unidad, IReloj reloj, Guid? proveedorHabitualId = null, bool controlarStock = false,
        string? unidadCompra = null, decimal factorCompra = 1m, string? unidadVenta = null, decimal factorVenta = 1m, SeguimientoArticulo seguimiento = SeguimientoArticulo.Ninguno)
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
        Seguimiento = seguimiento;
        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    public void Desactivar(IReloj reloj)
    {
        Activo = false;
        ActualizadoEn = reloj.AhoraUtc;
    }

    /// <summary>Establece la familia/categoría del artículo (se recorta; vacío = sin familia).</summary>
    public void EstablecerFamilia(string? familia)
    {
        var limpia = string.IsNullOrWhiteSpace(familia) ? null : familia.Trim();
        if (limpia is not null && limpia.Length > LongitudMaximaFamilia)
        {
            limpia = limpia[..LongitudMaximaFamilia];
        }

        Familia = limpia;
    }

    /// <summary>
    /// Enlaza el artículo con una familia del catálogo (o lo desvincula si <paramref name="familiaId"/>
    /// es null). El texto <see cref="Familia"/> se mantiene sincronizado con el nombre de la familia
    /// enlazada, de modo que las reglas de contabilización (que casan por nombre de familia) siguen
    /// funcionando sin cambios.
    /// </summary>
    public void EstablecerFamiliaId(Guid? familiaId, string? nombreFamilia)
    {
        FamiliaId = familiaId;
        if (familiaId is not null)
        {
            EstablecerFamilia(nombreFamilia);
        }
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
