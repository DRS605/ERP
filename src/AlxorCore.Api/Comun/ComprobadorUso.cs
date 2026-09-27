using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Organizacion.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace AlxorCore.Api.Comun;

/// <summary>
/// Mapa de referencias entre módulos: en qué columnas de otras tablas aparece cada tipo de maestro. Los módulos se
/// referencian por identificador, sin claves foráneas entre esquemas, así que este mapa es lo que permite saber si un
/// maestro se puede borrar. Una prueba recorre la base de datos y falla si aparece una columna que referencia a un
/// maestro y no está aquí (ni en <see cref="Propias"/>).
/// </summary>
public static class ReferenciasRegistros
{
    /// <summary>Referencias que impiden borrar: «esquema.tabla.columna» → descripción para el usuario.</summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<(string Referencia, string Descripcion)>> Mapa { get; } =
        new Dictionary<string, IReadOnlyList<(string, string)>>
        {
            [TiposRegistro.Cliente] =
            [
                ("facturacion.factura.cliente_id", "facturas"),
                ("facturacion.albaran_venta.cliente_id", "albaranes de venta"),
                ("facturacion.pedido_venta.cliente_id", "pedidos de venta"),
                ("facturacion.presupuesto.cliente_id", "presupuestos"),
                ("facturacion.factura_recurrente.cliente_id", "facturas periódicas"),
                ("facturacion.carta_porte.destinatario_cliente_id", "cartas de porte"),
                ("proyectos.proyecto.cliente_id", "proyectos"),
                ("tesoreria.anticipo.cliente_id", "anticipos"),
                ("tesoreria.efecto_cartera.tercero_id", "efectos de cartera"),
                ("agro.pale.cliente_id", "palés expedidos"),
                ("contabilidad.cuenta.tercero_id", "movimientos en su subcuenta contable"),
                ("contabilidad.documento_pendiente.tercero_id", "documentos pendientes de contabilizar"),
                ("contabilidad.regla_analitica.tercero_id", "reglas de analítica"),
                ("organizacion.asignacion_serie.tercero_id", "una serie de facturación asignada"),
            ],
            [TiposRegistro.Proveedor] =
            [
                ("gastos.gasto.proveedor_id", "facturas de gasto"),
                ("recepcion.factura_recibida.proveedor_id", "facturas recibidas"),
                ("compras.pedido_compra.proveedor_id", "pedidos de compra"),
                ("agro.agricultor.proveedor_id", "ficha de agricultor"),
                ("catalogo.producto.proveedor_habitual_id", "artículos de los que es proveedor habitual"),
                ("inventario.ubicacion_defecto.proveedor_id", "ubicaciones por defecto"),
                ("tesoreria.efecto_cartera.tercero_id", "efectos de cartera"),
                ("contabilidad.cuenta.tercero_id", "movimientos en su subcuenta contable"),
                ("contabilidad.documento_pendiente.tercero_id", "documentos pendientes de contabilizar"),
                ("contabilidad.regla_analitica.tercero_id", "reglas de analítica"),
                ("organizacion.asignacion_serie.tercero_id", "una serie de facturación asignada"),
            ],
            [TiposRegistro.Producto] =
            [
                ("facturacion.linea_factura.producto_id", "facturas"),
                ("facturacion.linea_albaran_venta.producto_id", "albaranes de venta"),
                ("facturacion.linea_pedido_venta.producto_id", "pedidos de venta"),
                ("facturacion.linea_presupuesto.producto_id", "presupuestos"),
                ("facturacion.linea_recurrente.producto_id", "facturas periódicas"),
                ("compras.linea_pedido.producto_id", "pedidos de compra"),
                ("compras.linea_albaran.producto_id", "albaranes de compra"),
                ("inventario.movimiento_inventario.producto_id", "movimientos de almacén"),
                ("inventario.existencia.producto_id", "existencias en almacén"),
                ("inventario.ubicacion_defecto.producto_id", "ubicaciones por defecto"),
                ("catalogo.movimiento_stock.producto_id", "movimientos de stock"),
                ("catalogo.linea_tarifa.producto_id", "tarifas de precios"),
                ("catalogo.componente_articulo.componente_id", "la composición de otro artículo"),
                ("catalogo.producto.producto_padre_id", "variantes"),
                ("produccion.orden_fabricacion.producto_id", "órdenes de fabricación"),
                ("produccion.componente_plan.componente_id", "órdenes de fabricación (como componente)"),
                ("contabilidad.documento_pendiente.producto_id", "documentos pendientes de contabilizar"),
                ("agro.articulo_campana.producto_id", "campañas agrícolas"),
                ("agro.linea_recepcion.producto_id", "recepciones de fruta"),
                ("agro.linea_recepcion.envase_producto_id", "recepciones de fruta (como envase)"),
                ("agro.movimiento_envase.envase_producto_id", "movimientos de envases"),
                ("agro.partida.producto_id", "partidas"),
                ("agro.parcela.producto_id", "parcelas"),
                ("agro.precio_liquidacion.producto_id", "precios de liquidación"),
                ("agro.material_parte.producto_id", "partes de confección (material)"),
                ("agro.salida_parte.producto_id", "partes de confección"),
            ],
            [TiposRegistro.Almacen] =
            [
                ("inventario.existencia.almacen_id", "existencias"),
                ("inventario.movimiento_inventario.almacen_id", "movimientos"),
                ("inventario.ubicacion_defecto.almacen_id", "ubicaciones por defecto de artículos"),
                ("produccion.orden_fabricacion.almacen_id", "órdenes de fabricación"),
            ],
            [TiposRegistro.Ubicacion] =
            [
                ("inventario.existencia.ubicacion_id", "existencias"),
                ("inventario.movimiento_inventario.ubicacion_id", "movimientos"),
                ("inventario.ubicacion_defecto.ubicacion_id", "artículos que la tienen por defecto"),
            ],
            [TiposRegistro.Gasto] =
            [
                ("agro.liquidacion.gasto_id", "la autofactura de una liquidación agrícola"),
            ],
            [TiposRegistro.Tarifa] =
            [
                ("terceros.cliente.tarifa_id", "clientes que la tienen asignada"),
            ],
        };

    /// <summary>
    /// Columnas que referencian a un maestro pero son parte de él (se borran con él) o son del propio maestro:
    /// no impiden el borrado.
    /// </summary>
    public static IReadOnlyCollection<string> Propias { get; } =
    [
        "catalogo.atributo_variante.producto_id",
        "catalogo.componente_articulo.producto_id",
        "catalogo.existencia_simple.producto_id",
        "catalogo.historico_precio.producto_id",
        "catalogo.linea_tarifa.tarifa_id",
        "inventario.ubicacion.almacen_id",
    ];
}

/// <summary>Implementación de <see cref="IComprobadorUso"/> con la función SQL <c>alxor_en_uso</c>.</summary>
public sealed class ComprobadorUso : IComprobadorUso
{
    private readonly OrganizacionDbContext _db;

    public ComprobadorUso(OrganizacionDbContext db) => _db = db;

    public async Task<string?> BuscarUsoAsync(string tipoRegistro, Guid id, CancellationToken ct = default)
    {
        if (!ReferenciasRegistros.Mapa.TryGetValue(tipoRegistro, out var referencias))
        {
            throw new ArgumentOutOfRangeException(nameof(tipoRegistro), tipoRegistro, "Tipo de registro sin mapa de referencias.");
        }

        var columnas = referencias.Select(r => r.Referencia).ToArray();
        var encontrada = await _db.Database
            .SqlQuery<string?>($"SELECT public.alxor_en_uso({columnas}, {id}) AS \"Value\"")
            .SingleAsync(ct).ConfigureAwait(false);
        return encontrada is null ? null : referencias.First(r => r.Referencia == encontrada).Descripcion;
    }
}
