namespace AlxorCore.Analisis.Aplicacion;

/// <summary>Tipo de un dato del análisis: decide el formato en pantalla y en Excel.</summary>
public enum TipoDato
{
    Texto,
    Fecha,
    Numero,
    Moneda,
    Porcentaje,
}

/// <summary>
/// Eje por el que se agrupa (cliente, artículo, mes…). <see cref="Sql"/> es una expresión SQL fija (nunca viene del
/// usuario) que se convierte a texto para agrupar, filtrar y profundizar con el mismo valor.
/// </summary>
public sealed record Dimension(string Clave, string Nombre, string Sql, string Grupo = "General", bool Ordenable = false);

/// <summary>
/// Cifra que se calcula (base, margen, kilos…). <see cref="Sql"/> es una expresión SQL de agregación fija. Las no
/// aditivas (porcentajes, medias) se recalculan en cada subtotal, nunca se suman.
/// </summary>
public sealed record Medida(string Clave, string Nombre, string Sql, TipoDato Tipo = TipoDato.Moneda, bool Aditiva = true, string? Descripcion = null);

/// <summary>Columna del detalle (los registros que forman una cifra, al profundizar).</summary>
public sealed record ColumnaDetalle(string Nombre, string Sql, TipoDato Tipo = TipoDato.Texto);

/// <summary>
/// Conjunto de datos analizable: de dónde salen los registros (<see cref="Desde"/>, con las uniones), la condición
/// que siempre se aplica (p. ej. sin anuladas), la fecha que acota el periodo, y sus dimensiones y medidas.
/// </summary>
public sealed record DatasetAnalisis(
    string Clave,
    string Nombre,
    string Descripcion,
    string? Modulo,
    string Desde,
    string CondicionBase,
    string ColumnaFecha,
    IReadOnlyList<Dimension> Dimensiones,
    IReadOnlyList<Medida> Medidas,
    IReadOnlyList<string> FilasDefecto,
    IReadOnlyList<string> MedidasDefecto,
    IReadOnlyList<ColumnaDetalle> Detalle,
    string? VistaDocumento = null,
    string? IdDocumentoSql = null)
{
    /// <summary>Dimensiones de fecha que tienen todos los conjuntos (sobre <see cref="ColumnaFecha"/>).</summary>
    public IReadOnlyList<Dimension> DimensionesCompletas =>
    [
        .. Dimensiones,
        new("anio", "Año", $"to_char({ColumnaFecha}, 'YYYY')", "Fecha", true),
        new("trimestre", "Trimestre", $"to_char({ColumnaFecha}, 'YYYY-\"T\"Q')", "Fecha", true),
        new("mes", "Mes", $"to_char({ColumnaFecha}, 'YYYY-MM')", "Fecha", true),
        new("mes_anio", "Mes (sin año)", $"to_char({ColumnaFecha}, 'MM')", "Fecha", true),
        new("semana", "Semana", $"to_char({ColumnaFecha}, 'IYYY-\"S\"IW')", "Fecha", true),
        new("dia", "Día", $"to_char({ColumnaFecha}, 'YYYY-MM-DD')", "Fecha", true),
        new("dia_semana", "Día de la semana", $"to_char({ColumnaFecha}, 'ID')", "Fecha", true),
    ];

    public Dimension? BuscarDimension(string clave) => DimensionesCompletas.FirstOrDefault(d => d.Clave == clave);

    public Medida? BuscarMedida(string clave) => Medidas.FirstOrDefault(m => m.Clave == clave);
}

/// <summary>
/// Catálogo de conjuntos de datos del análisis. Todo el SQL está aquí, fijo: el usuario solo elige claves
/// (dimensiones, medidas) y da valores, que van siempre como parámetros. La seguridad de fila (RLS) de cada tabla
/// limita los datos a la empresa (o al grupo, en los maestros) de quien consulta.
/// </summary>
public static class CatalogoDatasets
{
    private const string KilosLinea = "CASE WHEN lower(coalesce(p.unidad, '')) IN ('kg', 'kilo', 'kilos') THEN l.cantidad ELSE l.cantidad * coalesce(p.peso_kg, 0) END";

    public static IReadOnlyList<DatasetAnalisis> Todos { get; } =
    [
        Ventas(),
        Compras(),
        CarteraPedidos(),
        CobrosPagos(),
        Deuda(),
        Contabilidad(),
        Existencias(),
        RecepcionesAgro(),
        Albaranes(),
        Produccion(),
        Envases(),
    ];

    public static DatasetAnalisis? Buscar(string? clave) => Todos.FirstOrDefault(d => d.Clave == clave);

    private static DatasetAnalisis Ventas() => new(
        "ventas",
        "Ventas",
        "Líneas de las facturas emitidas (sin anuladas ni sustituidas, ni anticipos): importes, cantidades, kilos, coste y margen.",
        null,
        """
        facturacion.linea_factura l
        JOIN facturacion.factura f ON f.id = l.factura_id
        LEFT JOIN catalogo.producto p ON p.id = l.producto_id
        LEFT JOIN catalogo.familia fa ON fa.id = p.familia_id
        LEFT JOIN terceros.cliente c ON c.id = f.cliente_id
        LEFT JOIN organizacion.actividad_negocio an ON an.id = f.actividad_negocio_id
        """,
        // Las líneas de anticipos (cuenta 438: la factura del anticipo y su descuento en la final) no son venta.
        "f.estado NOT IN ('Anulada', 'Rectificada') AND coalesce(l.cuenta_contable, '') <> '438'",
        "f.fecha_emision",
        [
            new("cliente", "Cliente", "f.cliente_nombre", "Cliente"),
            new("cliente_nif", "NIF del cliente", "f.cliente_nif", "Cliente"),
            new("pais", "País", "f.pais", "Cliente"),
            new("provincia", "Provincia", "f.cliente_provincia", "Cliente"),
            new("poblacion", "Población", "f.cliente_poblacion", "Cliente"),
            new("tipo_cliente", "Tipo de cliente", "c.tipo", "Cliente"),
            new("articulo", "Artículo", "coalesce(p.nombre, l.descripcion)", "Artículo"),
            new("referencia", "Referencia", "p.referencia", "Artículo"),
            new("familia", "Familia", "coalesce(fa.nombre, p.familia)", "Artículo"),
            new("tipo_articulo", "Tipo de artículo", "p.tipo", "Artículo"),
            new("serie", "Serie", "f.prefijo", "Documento"),
            new("tipo_factura", "Tipo de factura", "f.tipo_factura", "Documento"),
            new("factura", "Factura", "f.numero_completo", "Documento"),
            new("tipo_iva", "Tipo de IVA", "l.codigo_iva", "Documento"),
            new("actividad", "Actividad de negocio", "an.nombre", "Documento"),
        ],
        [
            new("facturas", "Nº facturas", "count(DISTINCT f.id)", TipoDato.Numero, false),
            new("lineas", "Nº líneas", "count(*)", TipoDato.Numero),
            new("cantidad", "Cantidad", "sum(l.cantidad)", TipoDato.Numero),
            new("kilos", "Kilos", $"sum({KilosLinea})", TipoDato.Numero),
            new("base", "Base imponible", "sum(l.base)"),
            new("cuota", "Cuota IVA", "sum(l.cuota_iva)"),
            new("recargo", "Recargo equivalencia", "sum(coalesce(l.cuota_recargo, 0))"),
            new("conceptos", "Conceptos en precio", "sum(coalesce(l.importe_conceptos, 0))"),
            new("coste", "Coste", "sum(round(coalesce(l.coste_unitario, 0) * l.cantidad, 2) + coalesce(l.coste_conceptos, 0))"),
            new("margen", "Margen", "sum(l.base - round(coalesce(l.coste_unitario, 0) * l.cantidad, 2) - coalesce(l.coste_conceptos, 0))"),
            new("margen_pct", "Margen %", "round(100 * sum(l.base - round(coalesce(l.coste_unitario, 0) * l.cantidad, 2) - coalesce(l.coste_conceptos, 0)) / nullif(sum(l.base), 0), 2)", TipoDato.Porcentaje, false),
            new("precio_medio", "Precio medio", "round(sum(l.base) / nullif(sum(l.cantidad), 0), 4)", TipoDato.Moneda, false),
            new("precio_kilo", "Precio por kilo", $"round(sum(l.base) / nullif(sum({KilosLinea}), 0), 4)", TipoDato.Moneda, false),
            new("ticket_medio", "Importe medio por factura", "round(sum(l.base) / nullif(count(DISTINCT f.id), 0), 2)", TipoDato.Moneda, false),
            new("clientes", "Nº clientes", "count(DISTINCT coalesce(f.cliente_id::text, f.cliente_nombre))", TipoDato.Numero, false),
        ],
        ["cliente"],
        ["base", "margen", "margen_pct"],
        [
            new("Factura", "f.numero_completo"),
            new("Fecha", "f.fecha_emision", TipoDato.Fecha),
            new("Cliente", "f.cliente_nombre"),
            new("Artículo", "coalesce(p.nombre, l.descripcion)"),
            new("Cantidad", "l.cantidad", TipoDato.Numero),
            new("Precio", "l.precio_unitario", TipoDato.Moneda),
            new("Dto. %", "l.descuento", TipoDato.Porcentaje),
            new("Base", "l.base", TipoDato.Moneda),
            new("Coste", "round(coalesce(l.coste_unitario, 0) * l.cantidad, 2) + coalesce(l.coste_conceptos, 0)", TipoDato.Moneda),
        ],
        "facturas",
        "f.id");

    private static DatasetAnalisis Compras() => new(
        "compras",
        "Compras y gastos",
        "Líneas de las facturas de proveedor registradas (las rectificativas restan): bases, cuotas, parte deducible.",
        null,
        """
        gastos.linea_gasto lg
        JOIN gastos.gasto g ON g.id = lg.gasto_id
        LEFT JOIN terceros.proveedor pr ON pr.id = g.proveedor_id
        LEFT JOIN organizacion.actividad_negocio an ON an.id = g.actividad_negocio_id
        LEFT JOIN contabilidad.cuenta cg ON cg.empresa_id = g.empresa_id AND cg.codigo = lg.cuenta_gasto
        """,
        "g.estado = 'Registrado'",
        "g.fecha",
        [
            new("proveedor", "Proveedor", "coalesce(pr.nombre, g.proveedor_texto, g.concepto)", "Proveedor"),
            new("proveedor_nif", "NIF del proveedor", "pr.nif_fiscal", "Proveedor"),
            new("pais", "País", "pr.direccion_pais", "Proveedor"),
            new("provincia", "Provincia", "pr.direccion_provincia", "Proveedor"),
            new("cuenta_gasto", "Cuenta de gasto", "coalesce(lg.cuenta_gasto || coalesce(' · ' || cg.nombre, ''), '(según regla)')", "Contabilidad"),
            new("concepto", "Concepto de línea", "coalesce(lg.descripcion, g.concepto)", "Documento"),
            new("tipo_iva", "Tipo de IVA", "lg.codigo_iva", "Documento"),
            new("factura", "Factura", "coalesce(g.numero_factura, g.concepto)", "Documento"),
            new("rectificativa", "Rectificativa", "CASE WHEN g.es_rectificativa THEN 'Sí' ELSE 'No' END", "Documento"),
            new("afectacion", "Afectación (prorrata)", "g.afectacion", "Documento"),
            new("actividad", "Actividad de negocio", "an.nombre", "Documento"),
        ],
        [
            new("facturas", "Nº facturas", "count(DISTINCT g.id)", TipoDato.Numero, false),
            new("base", "Base imponible", "sum(lg.base)"),
            new("cuota", "Cuota IVA", "sum(lg.cuota)"),
            new("deducible", "Cuota deducible", "sum(lg.cuota_deducible)"),
            new("no_deducible", "Cuota no deducible", "sum(lg.cuota - lg.cuota_deducible)"),
            new("recargo", "Recargo equivalencia", "sum(lg.cuota_recargo)"),
            new("total", "Total (base + IVA cobrado + recargo)", "sum(lg.base + CASE WHEN lg.autoliquidada THEN 0 ELSE lg.cuota END + lg.cuota_recargo)"),
            new("coste", "Coste (base + IVA no deducible + recargo)", "sum(lg.base + lg.cuota - lg.cuota_deducible + lg.cuota_recargo)"),
            new("proveedores", "Nº proveedores", "count(DISTINCT coalesce(g.proveedor_id::text, g.proveedor_texto))", TipoDato.Numero, false),
        ],
        ["proveedor"],
        ["base", "cuota", "total"],
        [
            new("Factura", "coalesce(g.numero_factura, g.concepto)"),
            new("Fecha", "g.fecha", TipoDato.Fecha),
            new("Proveedor", "coalesce(pr.nombre, g.proveedor_texto)"),
            new("Concepto", "coalesce(lg.descripcion, g.concepto)"),
            new("Cuenta", "lg.cuenta_gasto"),
            new("Base", "lg.base", TipoDato.Moneda),
            new("IVA", "lg.cuota", TipoDato.Moneda),
            new("Deducible", "lg.cuota_deducible", TipoDato.Moneda),
        ],
        "gastos",
        "g.id");

    private static DatasetAnalisis CarteraPedidos() => new(
        "pedidos",
        "Cartera de pedidos de venta",
        "Líneas de los pedidos de venta confirmados o servidos: pedido, servido, facturado y lo pendiente.",
        "ventas",
        """
        facturacion.linea_pedido_venta l
        JOIN facturacion.pedido_venta pv ON pv.id = l.pedido_venta_id
        LEFT JOIN catalogo.producto p ON p.id = l.producto_id
        LEFT JOIN catalogo.familia fa ON fa.id = p.familia_id
        """,
        "pv.estado NOT IN ('Borrador', 'Cancelado')",
        "pv.fecha",
        [
            new("cliente", "Cliente", "pv.cliente_nombre", "Cliente"),
            new("articulo", "Artículo", "coalesce(p.nombre, l.descripcion)", "Artículo"),
            new("familia", "Familia", "coalesce(fa.nombre, p.familia)", "Artículo"),
            new("estado", "Estado del pedido", "pv.estado", "Documento"),
            new("pedido", "Pedido", "pv.serie || '-' || pv.ejercicio || '/' || pv.numero", "Documento"),
        ],
        [
            new("pedidos", "Nº pedidos", "count(DISTINCT pv.id)", TipoDato.Numero, false),
            new("cantidad", "Cantidad pedida", "sum(l.cantidad)", TipoDato.Numero),
            new("servida", "Cantidad servida", "sum(l.cantidad_servida)", TipoDato.Numero),
            new("facturada", "Cantidad facturada", "sum(l.cantidad_facturada)", TipoDato.Numero),
            new("pendiente_servir", "Pendiente de servir", "sum(greatest(l.cantidad - l.cantidad_servida, 0))", TipoDato.Numero),
            new("importe", "Importe pedido", "sum(round(l.cantidad * l.precio_unitario * (1 - l.descuento / 100), 2) + coalesce(l.importe_conceptos, 0))"),
            new("importe_pendiente", "Importe pendiente de facturar", "sum(round(greatest(l.cantidad - l.cantidad_facturada, 0) * l.precio_unitario * (1 - l.descuento / 100), 2))"),
            new("servicio_pct", "Nivel de servicio %", "round(100 * sum(least(l.cantidad_servida, l.cantidad)) / nullif(sum(l.cantidad), 0), 2)", TipoDato.Porcentaje, false),
        ],
        ["cliente"],
        ["importe", "importe_pendiente", "servicio_pct"],
        [
            new("Pedido", "pv.serie || '-' || pv.ejercicio || '/' || pv.numero"),
            new("Fecha", "pv.fecha", TipoDato.Fecha),
            new("Cliente", "pv.cliente_nombre"),
            new("Artículo", "coalesce(p.nombre, l.descripcion)"),
            new("Pedida", "l.cantidad", TipoDato.Numero),
            new("Servida", "l.cantidad_servida", TipoDato.Numero),
            new("Facturada", "l.cantidad_facturada", TipoDato.Numero),
            new("Precio", "l.precio_unitario", TipoDato.Moneda),
        ],
        "pedidosventa",
        "pv.id");

    private static DatasetAnalisis CobrosPagos() => new(
        "tesoreria",
        "Cobros y pagos",
        "Movimientos de tesorería (las anulaciones y devoluciones restan): por banco, forma, tercero y fecha.",
        null,
        """
        tesoreria.movimiento m
        LEFT JOIN tesoreria.cuenta_bancaria cb ON cb.id = m.cuenta_bancaria_id
        LEFT JOIN facturacion.factura f ON m.tipo_documento = 'Factura' AND f.id = m.documento_id
        LEFT JOIN gastos.gasto g ON m.tipo_documento = 'Gasto' AND g.id = m.documento_id
        LEFT JOIN terceros.proveedor pr ON pr.id = g.proveedor_id
        LEFT JOIN tesoreria.efecto_cartera ef ON m.tipo_documento = 'EfectoCartera' AND ef.id = m.documento_id
        """,
        "true",
        "m.fecha",
        [
            new("sentido", "Cobro / pago", "m.sentido", "Movimiento"),
            new("metodo", "Forma", "m.metodo", "Movimiento"),
            new("banco", "Banco o caja", "coalesce(cb.nombre, '(sin asignar)')", "Movimiento"),
            new("tipo_documento", "Tipo de documento", "m.tipo_documento", "Documento"),
            new("tercero", "Cliente o proveedor", "coalesce(f.cliente_nombre, pr.nombre, g.proveedor_texto, ef.tercero_nombre)", "Tercero"),
            new("documento", "Documento", "coalesce(f.numero_completo, g.numero_factura, g.concepto, ef.documento)", "Documento"),
        ],
        [
            new("movimientos", "Nº movimientos", "count(*)", TipoDato.Numero),
            new("cobros", "Cobrado", "sum(CASE WHEN m.sentido = 'Cobro' THEN m.importe ELSE 0 END)"),
            new("pagos", "Pagado", "sum(CASE WHEN m.sentido = 'Pago' THEN m.importe ELSE 0 END)"),
            new("neto", "Neto (cobros − pagos)", "sum(CASE WHEN m.sentido = 'Cobro' THEN m.importe ELSE -m.importe END)"),
        ],
        ["banco", "sentido"],
        ["cobros", "pagos", "neto"],
        [
            new("Fecha", "m.fecha", TipoDato.Fecha),
            new("Sentido", "m.sentido"),
            new("Tercero", "coalesce(f.cliente_nombre, pr.nombre, g.proveedor_texto, ef.tercero_nombre)"),
            new("Documento", "coalesce(f.numero_completo, g.numero_factura, g.concepto, ef.documento)"),
            new("Forma", "m.metodo"),
            new("Banco", "cb.nombre"),
            new("Importe", "m.importe", TipoDato.Moneda),
        ]);

    private static DatasetAnalisis Deuda() => new(
        "deuda",
        "Deuda de clientes",
        "Facturas emitidas con lo cobrado y lo pendiente a hoy, por vencimiento y antigüedad de la deuda.",
        null,
        """
        facturacion.factura f
        LEFT JOIN LATERAL (
            SELECT coalesce(sum(m.importe), 0) AS cobrado
            FROM tesoreria.movimiento m
            WHERE m.tipo_documento = 'Factura' AND m.documento_id = f.id AND m.sentido = 'Cobro'
        ) mv ON true
        LEFT JOIN terceros.cliente c ON c.id = f.cliente_id
        """,
        "f.estado NOT IN ('Anulada', 'Rectificada')",
        "f.fecha_emision",
        [
            new("cliente", "Cliente", "f.cliente_nombre", "Cliente"),
            new("pais", "País", "f.pais", "Cliente"),
            new("situacion", "Situación", "CASE WHEN f.total - mv.cobrado <= 0 THEN 'Cobrada' WHEN coalesce(f.fecha_vencimiento, f.fecha_emision) < current_date THEN 'Vencida' ELSE 'Pendiente' END", "Deuda"),
            new("antiguedad", "Antigüedad de la deuda", "CASE WHEN f.total - mv.cobrado <= 0 THEN '0 · cobrada' WHEN coalesce(f.fecha_vencimiento, f.fecha_emision) >= current_date THEN '1 · sin vencer' WHEN current_date - coalesce(f.fecha_vencimiento, f.fecha_emision) <= 30 THEN '2 · 1-30 días' WHEN current_date - coalesce(f.fecha_vencimiento, f.fecha_emision) <= 60 THEN '3 · 31-60 días' WHEN current_date - coalesce(f.fecha_vencimiento, f.fecha_emision) <= 90 THEN '4 · 61-90 días' ELSE '5 · más de 90 días' END", "Deuda", true),
            new("mes_vencimiento", "Mes de vencimiento", "to_char(coalesce(f.fecha_vencimiento, f.fecha_emision), 'YYYY-MM')", "Deuda", true),
            new("factura", "Factura", "f.numero_completo", "Documento"),
        ],
        [
            new("facturas", "Nº facturas", "count(*)", TipoDato.Numero),
            new("total", "Facturado", "sum(f.total)"),
            new("cobrado", "Cobrado", "sum(mv.cobrado)"),
            new("pendiente", "Pendiente", "sum(greatest(f.total - mv.cobrado, 0))"),
            new("vencido", "Vencido", "sum(CASE WHEN coalesce(f.fecha_vencimiento, f.fecha_emision) < current_date THEN greatest(f.total - mv.cobrado, 0) ELSE 0 END)"),
            new("dias_medios", "Días medios de retraso (vencido)", "round(avg(CASE WHEN f.total - mv.cobrado > 0 AND coalesce(f.fecha_vencimiento, f.fecha_emision) < current_date THEN current_date - coalesce(f.fecha_vencimiento, f.fecha_emision) END), 1)", TipoDato.Numero, false),
            new("cobro_pct", "Cobrado %", "round(100 * sum(mv.cobrado) / nullif(sum(f.total), 0), 2)", TipoDato.Porcentaje, false),
        ],
        ["cliente"],
        ["total", "pendiente", "vencido"],
        [
            new("Factura", "f.numero_completo"),
            new("Fecha", "f.fecha_emision", TipoDato.Fecha),
            new("Vencimiento", "coalesce(f.fecha_vencimiento, f.fecha_emision)", TipoDato.Fecha),
            new("Cliente", "f.cliente_nombre"),
            new("Total", "f.total", TipoDato.Moneda),
            new("Cobrado", "mv.cobrado", TipoDato.Moneda),
            new("Pendiente", "greatest(f.total - mv.cobrado, 0)", TipoDato.Moneda),
        ],
        "facturas",
        "f.id");

    private static DatasetAnalisis Contabilidad() => new(
        "contabilidad",
        "Contabilidad (apuntes)",
        "Apuntes del diario: debe, haber y saldo por cuenta, grupo del plan, diario u origen.",
        "contabilidad",
        """
        contabilidad.apunte a
        JOIN contabilidad.asiento s ON s.id = a.asiento_id
        LEFT JOIN contabilidad.cuenta cu ON cu.empresa_id = s.empresa_id AND cu.codigo = a.cuenta_codigo
        LEFT JOIN contabilidad.cuenta c3 ON c3.empresa_id = s.empresa_id AND c3.codigo = left(a.cuenta_codigo, 3)
        """,
        "true",
        "s.fecha",
        [
            new("cuenta", "Cuenta", "a.cuenta_codigo || coalesce(' · ' || cu.nombre, '')", "Cuenta", true),
            new("cuenta3", "Cuenta (3 dígitos)", "left(a.cuenta_codigo, 3) || coalesce(' · ' || c3.nombre, '')", "Cuenta", true),
            new("subgrupo", "Subgrupo (2 dígitos)", "left(a.cuenta_codigo, 2)", "Cuenta", true),
            new("grupo", "Grupo del plan", "CASE left(a.cuenta_codigo, 1) WHEN '1' THEN '1 · Financiación básica' WHEN '2' THEN '2 · Activo no corriente' WHEN '3' THEN '3 · Existencias' WHEN '4' THEN '4 · Acreedores y deudores' WHEN '5' THEN '5 · Cuentas financieras' WHEN '6' THEN '6 · Compras y gastos' WHEN '7' THEN '7 · Ventas e ingresos' WHEN '8' THEN '8 · Gastos imputados al PN' WHEN '9' THEN '9 · Ingresos imputados al PN' ELSE left(a.cuenta_codigo, 1) END", "Cuenta", true),
            new("diario", "Diario", "coalesce(s.diario, 'General')", "Asiento"),
            new("origen", "Origen", "s.origen", "Asiento"),
            new("ejercicio", "Ejercicio", "s.ejercicio::text", "Asiento", true),
            new("concepto", "Concepto", "coalesce(a.concepto, s.concepto)", "Asiento"),
        ],
        [
            new("debe", "Debe", "sum(a.debe)"),
            new("haber", "Haber", "sum(a.haber)"),
            new("saldo", "Saldo (debe − haber)", "sum(a.debe - a.haber)"),
            new("saldo_acreedor", "Saldo (haber − debe)", "sum(a.haber - a.debe)"),
            new("apuntes", "Nº apuntes", "count(*)", TipoDato.Numero),
            new("asientos", "Nº asientos", "count(DISTINCT s.id)", TipoDato.Numero, false),
        ],
        ["grupo"],
        ["debe", "haber", "saldo"],
        [
            new("Asiento", "s.ejercicio || '/' || s.numero"),
            new("Fecha", "s.fecha", TipoDato.Fecha),
            new("Cuenta", "a.cuenta_codigo"),
            new("Concepto", "coalesce(a.concepto, s.concepto)"),
            new("Debe", "a.debe", TipoDato.Moneda),
            new("Haber", "a.haber", TipoDato.Moneda),
        ]);

    private static DatasetAnalisis Existencias() => new(
        "almacen",
        "Movimientos de almacén",
        "Entradas, salidas, ajustes y traspasos de los almacenes: unidades y valor a coste.",
        "inventario",
        """
        inventario.movimiento_inventario mi
        JOIN inventario.almacen al ON al.id = mi.almacen_id
        LEFT JOIN catalogo.producto p ON p.id = mi.producto_id
        LEFT JOIN catalogo.familia fa ON fa.id = p.familia_id
        """,
        "true",
        "mi.fecha",
        [
            new("almacen", "Almacén", "al.nombre", "Almacén"),
            new("articulo", "Artículo", "p.nombre", "Artículo"),
            new("familia", "Familia", "coalesce(fa.nombre, p.familia)", "Artículo"),
            new("tipo", "Tipo de movimiento", "mi.tipo", "Movimiento"),
            new("motivo", "Motivo", "mi.motivo", "Movimiento"),
            new("lote", "Lote", "mi.lote", "Movimiento"),
        ],
        [
            new("movimientos", "Nº movimientos", "count(*)", TipoDato.Numero),
            new("entradas", "Entradas", "sum(CASE WHEN mi.tipo IN ('Entrada', 'TraspasoEntrada') THEN mi.cantidad WHEN mi.tipo = 'Ajuste' AND mi.cantidad > 0 THEN mi.cantidad ELSE 0 END)", TipoDato.Numero),
            new("salidas", "Salidas", "sum(CASE WHEN mi.tipo IN ('Salida', 'TraspasoSalida') THEN mi.cantidad WHEN mi.tipo = 'Ajuste' AND mi.cantidad < 0 THEN -mi.cantidad ELSE 0 END)", TipoDato.Numero),
            new("neto", "Variación de stock", "sum(CASE WHEN mi.tipo IN ('Salida', 'TraspasoSalida') THEN -mi.cantidad ELSE mi.cantidad END)", TipoDato.Numero),
            new("valor", "Variación de valor (a coste)", "sum(round((CASE WHEN mi.tipo IN ('Salida', 'TraspasoSalida') THEN -mi.cantidad ELSE mi.cantidad END) * coalesce(mi.coste_unitario, p.precio_compra, 0), 2))"),
        ],
        ["almacen", "articulo"],
        ["entradas", "salidas", "neto"],
        [
            new("Fecha", "mi.fecha", TipoDato.Fecha),
            new("Almacén", "al.nombre"),
            new("Artículo", "p.nombre"),
            new("Tipo", "mi.tipo"),
            new("Cantidad", "mi.cantidad", TipoDato.Numero),
            new("Coste", "mi.coste_unitario", TipoDato.Moneda),
            new("Referencia", "mi.referencia"),
        ]);

    private static DatasetAnalisis RecepcionesAgro() => new(
        "agro",
        "Entradas de fruta",
        "Líneas de las recepciones confirmadas: kilos netos, envases e importe estimado por agricultor, producto y calibre.",
        "agro",
        """
        agro.linea_recepcion lr
        JOIN agro.recepcion r ON r.id = lr.recepcion_id
        LEFT JOIN agro.agricultor ag ON ag.id = r.agricultor_id
        LEFT JOIN agro.campana ca ON ca.id = r.campana_id
        LEFT JOIN agro.parcela pa ON pa.id = lr.parcela_id
        """,
        "r.estado = 'Confirmada'",
        "r.fecha",
        [
            new("agricultor", "Agricultor", "ag.nombre", "Agricultor"),
            new("producto", "Producto / variedad", "lr.producto_nombre", "Producto"),
            new("calibre", "Calibre", "lr.calibre", "Producto"),
            new("campana", "Campaña", "ca.nombre", "Campaña"),
            new("parcela", "Parcela", "pa.nombre", "Agricultor"),
        ],
        [
            new("recepciones", "Nº recepciones", "count(DISTINCT r.id)", TipoDato.Numero, false),
            new("kilos", "Kilos netos", "sum(lr.neto_kg)", TipoDato.Numero),
            new("envases", "Envases", "sum(lr.envases)", TipoDato.Numero),
            new("importe_estimado", "Importe estimado", "sum(round(lr.neto_kg * coalesce(lr.precio_estimado_kg, 0), 2))"),
            new("precio_medio", "Precio medio estimado €/kg", "round(sum(lr.neto_kg * coalesce(lr.precio_estimado_kg, 0)) / nullif(sum(lr.neto_kg), 0), 4)", TipoDato.Moneda, false),
            new("agricultores", "Nº agricultores", "count(DISTINCT r.agricultor_id)", TipoDato.Numero, false),
        ],
        ["agricultor"],
        ["kilos", "envases", "importe_estimado"],
        [
            new("Recepción", "r.ejercicio || '/' || r.numero"),
            new("Fecha", "r.fecha", TipoDato.Fecha),
            new("Agricultor", "ag.nombre"),
            new("Producto", "lr.producto_nombre"),
            new("Calibre", "lr.calibre"),
            new("Kilos", "lr.neto_kg", TipoDato.Numero),
            new("Envases", "lr.envases", TipoDato.Numero),
        ]);

    private const string ImporteAlbaran = "round(l.cantidad * l.precio_unitario * (1 - coalesce(l.porcentaje_descuento, 0) / 100), 2) + coalesce(l.importe_conceptos, 0)";

    private const string EstadoLineaAlbaran = "CASE WHEN a.factura_id IS NOT NULL THEN 'Facturado' WHEN l.precio_fijado THEN 'Pendiente de facturar' ELSE 'Precio por fijar' END";

    private static DatasetAnalisis Albaranes() => new(
        "albaranes",
        "Albaranes de venta (expediciones)",
        "Líneas de los albaranes de venta no anulados: lo expedido a cada cliente, con precio por fijar, pendiente de facturar o facturado.",
        "ventas",
        """
        facturacion.linea_albaran_venta l
        JOIN facturacion.albaran_venta a ON a.id = l.albaran_venta_id
        LEFT JOIN catalogo.producto p ON p.id = l.producto_id
        LEFT JOIN catalogo.familia fa ON fa.id = p.familia_id
        """,
        "a.anulado_en IS NULL",
        "a.fecha",
        [
            new("cliente", "Cliente", "a.cliente_nombre", "Cliente"),
            new("articulo", "Artículo", "coalesce(p.nombre, l.descripcion)", "Artículo"),
            new("familia", "Familia", "coalesce(fa.nombre, p.familia)", "Artículo"),
            new("estado", "Situación", EstadoLineaAlbaran, "Documento"),
            new("albaran", "Albarán", "coalesce(a.serie, '') || a.numero", "Documento"),
        ],
        [
            new("albaranes", "Nº albaranes", "count(DISTINCT a.id)", TipoDato.Numero, false),
            new("cantidad", "Cantidad", "sum(l.cantidad)", TipoDato.Numero),
            new("kilos", "Kilos", $"sum({KilosLinea})", TipoDato.Numero),
            new("importe", "Importe (estimado si está por fijar)", $"sum({ImporteAlbaran})"),
            new("sin_facturar", "Importe sin facturar", $"sum(CASE WHEN a.factura_id IS NULL THEN {ImporteAlbaran} ELSE 0 END)"),
            new("por_fijar", "Cantidad a precio por fijar", "sum(CASE WHEN a.factura_id IS NULL AND NOT l.precio_fijado THEN l.cantidad ELSE 0 END)", TipoDato.Numero),
            new("coste", "Coste", "sum(round(coalesce(l.coste_unitario, 0) * l.cantidad, 2) + coalesce(l.coste_conceptos, 0))"),
            new("precio_kilo", "Precio por kilo", $"round(sum({ImporteAlbaran}) / nullif(sum({KilosLinea}), 0), 4)", TipoDato.Moneda, false),
            new("clientes", "Nº clientes", "count(DISTINCT a.cliente_id)", TipoDato.Numero, false),
        ],
        ["cliente"],
        ["kilos", "importe", "sin_facturar"],
        [
            new("Albarán", "coalesce(a.serie, '') || a.numero"),
            new("Fecha", "a.fecha", TipoDato.Fecha),
            new("Cliente", "a.cliente_nombre"),
            new("Artículo", "coalesce(p.nombre, l.descripcion)"),
            new("Cantidad", "l.cantidad", TipoDato.Numero),
            new("Precio", "l.precio_unitario", TipoDato.Moneda),
            new("Situación", EstadoLineaAlbaran),
        ]);

    // Lo consumido en el parte se reparte entre sus salidas por kilos: así cada salida tiene su rendimiento y su merma.
    private const string ConsumoSalida =
        "coalesce((SELECT sum(c.kilos) FROM agro.consumo_parte c WHERE c.parte_id = pc.id), 0) * s.kilos / nullif((SELECT sum(x.kilos) FROM agro.salida_parte x WHERE x.parte_id = pc.id), 0)";

    private static DatasetAnalisis Produccion() => new(
        "produccion",
        "Producción (partes de confección)",
        "Salidas de los partes de confección validados: kilos y cajas obtenidos, lo consumido, el rendimiento, la merma y el coste por kilo.",
        "agro",
        """
        agro.salida_parte s
        JOIN agro.parte_confeccion pc ON pc.id = s.parte_id
        LEFT JOIN agro.categoria k ON k.id = s.categoria_id
        LEFT JOIN agro.campana ca ON ca.id = pc.campana_id
        """,
        "pc.estado = 'Validado'",
        "pc.fecha",
        [
            new("producto", "Producto confeccionado", "s.nombre", "Producto"),
            new("calibre", "Calibre", "s.calibre", "Producto"),
            new("categoria", "Categoría", "k.nombre", "Producto"),
            new("campana", "Campaña", "ca.nombre", "Campaña"),
            new("descripcion", "Línea o descripción del parte", "pc.descripcion", "Parte"),
            new("parte", "Parte", "'PC-' || pc.ejercicio || '-' || lpad(pc.numero::text, 6, '0')", "Parte"),
        ],
        [
            new("partes", "Nº partes", "count(DISTINCT pc.id)", TipoDato.Numero, false),
            new("kilos", "Kilos obtenidos", "sum(s.kilos)", TipoDato.Numero),
            new("cajas", "Cajas", "sum(coalesce(s.cajas, 0))", TipoDato.Numero),
            new("consumido", "Kilos consumidos", $"round(sum({ConsumoSalida}), 3)", TipoDato.Numero),
            new("merma", "Merma (kg)", $"round(sum({ConsumoSalida}) - sum(s.kilos), 3)", TipoDato.Numero),
            new("rendimiento", "Rendimiento %", $"round(100 * sum(s.kilos) / nullif(sum({ConsumoSalida}), 0), 2)", TipoDato.Porcentaje, false),
            new("coste", "Coste total", "sum(s.coste)"),
            new("coste_confeccion", "Coste de confección", "sum(coalesce(s.coste_confeccion, 0))"),
            new("coste_kg", "Coste por kilo", "round(sum(s.coste) / nullif(sum(s.kilos), 0), 4)", TipoDato.Moneda, false),
            new("coste_caja", "Coste por caja", "round(sum(s.coste) / nullif(sum(coalesce(s.cajas, 0)), 0), 4)", TipoDato.Moneda, false),
        ],
        ["producto"],
        ["kilos", "rendimiento", "coste_kg"],
        [
            new("Parte", "'PC-' || pc.ejercicio || '-' || lpad(pc.numero::text, 6, '0')"),
            new("Fecha", "pc.fecha", TipoDato.Fecha),
            new("Producto", "s.nombre"),
            new("Calibre", "s.calibre"),
            new("Kilos", "s.kilos", TipoDato.Numero),
            new("Cajas", "s.cajas", TipoDato.Numero),
            new("Coste", "s.coste", TipoDato.Moneda),
        ]);

    private static DatasetAnalisis Envases() => new(
        "envases",
        "Envases por tercero",
        "Libro de envases retornables de clientes, proveedores, transportistas y pools: entregados (+), recogidos (−) y el neto del periodo.",
        "agro",
        """
        agro.linea_movimiento_envases l
        JOIN agro.movimiento_envases m ON m.id = l.movimiento_envases_id
        JOIN agro.cuenta_envases c ON c.id = m.cuenta_id
        LEFT JOIN catalogo.producto p ON p.id = l.envase_producto_id
        """,
        "true",
        "m.fecha",
        [
            new("tercero", "Tercero", "c.nombre", "Tercero"),
            new("tipo_cuenta", "Tipo de tercero", "c.tipo", "Tercero"),
            new("envase", "Envase", "p.nombre", "Envase"),
            new("origen", "Origen del movimiento", "m.origen", "Documento"),
        ],
        [
            new("movimientos", "Nº movimientos", "count(DISTINCT m.id)", TipoDato.Numero, false),
            new("entregados", "Entregados", "sum(CASE WHEN l.cantidad > 0 THEN l.cantidad ELSE 0 END)", TipoDato.Numero),
            new("recogidos", "Recogidos", "sum(CASE WHEN l.cantidad < 0 THEN -l.cantidad ELSE 0 END)", TipoDato.Numero),
            new("neto", "Neto del periodo", "sum(l.cantidad)", TipoDato.Numero),
        ],
        ["tercero"],
        ["entregados", "recogidos", "neto"],
        [
            new("Movimiento", "'ENV-' || m.ejercicio || '-' || lpad(m.numero::text, 6, '0')"),
            new("Fecha", "m.fecha", TipoDato.Fecha),
            new("Tercero", "c.nombre"),
            new("Envase", "p.nombre"),
            new("Cantidad", "l.cantidad", TipoDato.Numero),
            new("Origen", "m.origen"),
        ]);
}
