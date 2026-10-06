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
                ("extensiones.plantilla_etiqueta.cliente_id", "plantillas de etiqueta"),
                ("extensiones.referencia_cliente.cliente_id", "referencias de artículos"),
                ("tesoreria.clasificacion_seguro.cliente_id", "clasificaciones del seguro de crédito"),
                ("tesoreria.aviso_impago.cliente_id", "avisos de impago al seguro de crédito"),
                ("facturacion.factura.cliente_id", "facturas"),
                ("facturacion.asignacion_agente.cliente_id", "asignación a un agente comercial"),
                ("facturacion.regla_comision.cliente_id", "reglas de comisión de agentes"),
                ("facturacion.albaran_venta.cliente_id", "albaranes de venta"),
                ("facturacion.pedido_venta.cliente_id", "pedidos de venta"),
                ("facturacion.presupuesto.cliente_id", "presupuestos"),
                ("facturacion.factura_recurrente.cliente_id", "facturas periódicas"),
                ("facturacion.carta_porte.destinatario_cliente_id", "cartas de porte"),
                ("proyectos.proyecto.cliente_id", "proyectos"),
                ("tesoreria.anticipo.cliente_id", "anticipos"),
                ("tesoreria.liquidacion_pagos.cliente_id", "compensaciones en liquidaciones de pagos"),
                ("tesoreria.efecto_cartera.tercero_id", "efectos de cartera"),
                ("tesoreria.renovacion_efecto.tercero_id", "renovaciones de efectos"),
                ("tesoreria.situacion_deuda.tercero_id", "deudas impagadas o dudosas"),
                ("agro.pale.cliente_id", "palés expedidos"),
                ("agro.plantilla_pale.cliente_id", "plantillas de palé"),
                ("agro.linea_plan.cliente_id", "planes comerciales"),
                ("agro.orden_linea.cliente_id", "órdenes de las líneas de la planta"),
                ("logistica.plantilla_paletizado.cliente_id", "plantillas de paletizado"),
                ("logistica.unidad_logistica.cliente_id", "palés y unidades logísticas"),
                ("integraciones.socio_edi.cliente_id", "socios EDI"),
                ("facturacion.devolucion_venta.cliente_id", "devoluciones de venta"),
                ("facturacion.liquidacion_comision.cliente_id", "liquidaciones de venta en comisión"),
                ("facturacion.reclamacion_venta.cliente_id", "reclamaciones"),
                ("bodega.operacion.cliente_id", "ventas a granel de la bodega"),
                ("agro.lote_subasta.comprador_id", "lotes comprados en subasta"),
                ("vivero.encargo.cliente_id", "encargos de planta del vivero"),
                ("agro.cuenta_envases.tercero_id", "cuenta de envases"),
                ("contabilidad.cuenta.tercero_id", "movimientos en su subcuenta contable"),
                ("contabilidad.documento_pendiente.tercero_id", "documentos pendientes de contabilizar"),
                ("contabilidad.regla_analitica.tercero_id", "reglas de analítica"),
                ("organizacion.asignacion_serie.tercero_id", "una serie de facturación asignada"),
                ("catalogo.asignacion_concepto.tercero_id", "conceptos de línea que se le ponen solos"),
            ],
            [TiposRegistro.Proveedor] =
            [
                ("gastos.gasto.proveedor_id", "facturas de gasto"),
                ("facturacion.liquidacion_comision.proveedor_id", "liquidaciones de venta en comisión (como comisionista)"),
                ("recepcion.factura_recibida.proveedor_id", "facturas recibidas"),
                ("compras.pedido_compra.proveedor_id", "pedidos de compra"),
                ("compras.devolucion_compra.proveedor_id", "devoluciones a proveedor"),
                ("inventario.regla_reaprovisionamiento.proveedor_id", "reglas de stock mínimo"),
                ("facturacion.agente_comercial.proveedor_id", "ficha de agente comercial"),
                ("agro.agricultor.proveedor_id", "ficha de agricultor"),
                ("cooperativa.socio.proveedor_id", "ficha de socio de la cooperativa"),
                ("bodega.entrada_uva.viticultor_id", "entradas de uva de la bodega"),
                ("tesoreria.entrega_cuenta_proveedor.proveedor_id", "entregas a cuenta"),
                ("tesoreria.liquidacion_pagos.proveedor_id", "liquidaciones de pagos"),
                ("agro.cuenta_envases.tercero_id", "cuenta de envases"),
                ("catalogo.producto.proveedor_habitual_id", "artículos de los que es proveedor habitual"),
                ("inventario.ubicacion_defecto.proveedor_id", "ubicaciones por defecto"),
                ("tesoreria.efecto_cartera.tercero_id", "efectos de cartera"),
                ("contabilidad.cuenta.tercero_id", "movimientos en su subcuenta contable"),
                ("contabilidad.documento_pendiente.tercero_id", "documentos pendientes de contabilizar"),
                ("contabilidad.regla_analitica.tercero_id", "reglas de analítica"),
                ("organizacion.asignacion_serie.tercero_id", "una serie de facturación asignada"),
                ("catalogo.asignacion_concepto.tercero_id", "conceptos de línea que se le ponen solos"),
            ],
            [TiposRegistro.Producto] =
            [
                ("extensiones.referencia_cliente.producto_id", "referencias en clientes"),
                ("facturacion.linea_factura.producto_id", "facturas"),
                ("agro.fitosanitario.producto_id", "productos del registro de fitosanitarios"),
                ("bodega.operacion.producto_id", "embotellados y ventas a granel de la bodega"),
                ("agro.lote_subasta.producto_id", "lotes de subasta"),
                ("vivero.lote_planta.producto_id", "lotes de planta del vivero"),
                ("vivero.encargo.producto_id", "encargos de planta del vivero"),
                ("agro.tratamiento_parcela.articulo_id", "tratamientos del cuaderno de campo"),
                ("inventario.lote_articulo.producto_id", "lotes con fechas"),
                ("catalogo.asignacion_concepto.producto_id", "conceptos de línea que se le ponen solos"),
                ("facturacion.linea_albaran_venta.producto_id", "albaranes de venta"),
                ("facturacion.linea_devolucion_venta.producto_id", "devoluciones de venta"),
                ("facturacion.linea_liquidacion_comision.producto_id", "liquidaciones de venta en comisión"),
                ("facturacion.linea_pedido_venta.producto_id", "pedidos de venta"),
                ("facturacion.linea_presupuesto.producto_id", "presupuestos"),
                ("facturacion.linea_recurrente.producto_id", "facturas periódicas"),
                ("compras.linea_pedido.producto_id", "pedidos de compra"),
                ("compras.linea_albaran.producto_id", "albaranes de compra"),
                ("inventario.movimiento_inventario.producto_id", "movimientos de almacén"),
                ("inventario.existencia.producto_id", "existencias en almacén"),
                ("inventario.linea_recuento.producto_id", "recuentos de inventario"),
                ("compras.linea_devolucion_compra.producto_id", "devoluciones a proveedor"),
                ("inventario.regla_reaprovisionamiento.producto_id", "reglas de stock mínimo"),
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
                ("agro.linea_movimiento_envases.envase_producto_id", "movimientos de envases con terceros"),
                ("agro.limite_envase.envase_producto_id", "límites de envases"),
                ("agro.envase_pool.envase_producto_id", "envases de pools"),
                ("agro.concepto_liquidacion.producto_id", "cargos y abonos de liquidación"),
                ("agro.concepto_liquidacion.envase_producto_id", "cargos y abonos de liquidación (como envase)"),
                ("agro.linea_orden_carga.producto_id", "órdenes de carga"),
                ("agro.precio_liquidacion.envase_producto_id", "precios de liquidación por envase"),
                ("agro.rendimiento_confeccion.envase_producto_id", "rendimientos de confección"),
                ("agro.plantilla_pale.envase_producto_id", "plantillas de palé (como caja)"),
                ("agro.plantilla_pale.pale_producto_id", "plantillas de palé (como palé)"),
                ("agro.declaracion_articulo.producto_id", "declaración de certificación del artículo"),
                ("agro.tara_envase.envase_producto_id", "taras de envases"),
                ("agro.plantilla_calidad.producto_id", "plantillas de control de calidad"),
                ("agro.linea_plan.producto_id", "planes comerciales, de producción o de entradas"),
                ("agro.orden_linea.producto_id", "órdenes de las líneas de la planta"),
                ("agro.regla_transformacion.producto_origen_id", "transformaciones permitidas (como origen)"),
                ("agro.regla_transformacion.producto_destino_id", "transformaciones permitidas (como destino)"),
                ("agro.pesada_envase.envase_producto_id", "pesadas (envases contados)"),
                ("agro.pale_entrada.envase_producto_id", "palés de entrada de recepciones"),
                ("logistica.ficha_logistica.producto_id", "ficha logística"),
                ("logistica.plantilla_paletizado.producto_id", "plantillas de paletizado"),
                ("logistica.linea_unidad_logistica.producto_id", "contenido de palés y unidades logísticas"),
                ("logistica.tipo_soporte.envase_producto_id", "soportes logísticos (como envase)"),
                ("agro.movimiento_envase.envase_producto_id", "movimientos de envases"),
                ("agro.partida.producto_id", "partidas"),
                ("agro.parcela.producto_id", "parcelas"),
                ("agro.precio_liquidacion.producto_id", "precios de liquidación"),
                ("agro.material_parte.producto_id", "partes de confección (material)"),
                ("agro.salida_parte.producto_id", "partes de confección"),
                ("agro.salida_parte.envase_producto_id", "partes de confección (envase)"),
                ("agro.rendimiento_confeccion.producto_id", "rendimientos de confección"),
                ("agro.plantilla_pale.producto_id", "plantillas de palé"),
            ],
            [TiposRegistro.Almacen] =
            [
                ("organizacion.centro.almacen_id", "centros que lo tienen por almacén habitual"),
                ("inventario.existencia.almacen_id", "existencias"),
                ("inventario.recuento_inventario.almacen_id", "recuentos de inventario"),
                ("compras.devolucion_compra.almacen_id", "devoluciones a proveedor"),
                ("inventario.regla_reaprovisionamiento.almacen_id", "reglas de stock mínimo"),
                ("agro.tratamiento_parcela.almacen_id", "tratamientos del cuaderno de campo"),
                ("inventario.movimiento_inventario.almacen_id", "movimientos"),
                ("inventario.ubicacion_defecto.almacen_id", "ubicaciones por defecto de artículos"),
                ("produccion.orden_fabricacion.almacen_id", "órdenes de fabricación"),
                ("compras.albaran_compra.almacen_id", "albaranes de compra"),
                ("compras.almacen_traspaso.almacen_id", "almacén de entrada de traspasos intragrupo"),
                ("logistica.unidad_logistica.almacen_id", "palés y unidades logísticas"),
            ],
            [TiposRegistro.Ubicacion] =
            [
                ("inventario.existencia.ubicacion_id", "existencias"),
                ("inventario.recuento_inventario.ubicacion_id", "recuentos de inventario"),
                ("inventario.linea_recuento.ubicacion_id", "recuentos de inventario"),
                ("logistica.unidad_logistica.ubicacion_id", "palés y unidades logísticas"),
                ("inventario.movimiento_inventario.ubicacion_id", "movimientos"),
                ("inventario.ubicacion_defecto.ubicacion_id", "artículos que la tienen por defecto"),
            ],
            [TiposRegistro.CentroCoste] =
            [
                ("contabilidad.imputacion_analitica.centro_id", "imputaciones"),
                ("contabilidad.regla_analitica.centro_id", "reglas de asignación"),
                ("contabilidad.linea_clave_reparto.centro_id", "claves de reparto"),
                ("contabilidad.linea_presupuesto.centro_id", "presupuestos"),
                ("contabilidad.ejecucion_analitica.centro_origen_id", "repartos"),
                ("contabilidad.centro_analitico.padre_id", "centros que cuelgan de él"),
                ("agro.parte_confeccion.centro_analitico_id", "partes de confección"),
                ("agro.parcela.centro_analitico_id", "parcelas"),
                ("agro.linea_planta.centro_analitico_id", "líneas de la planta"),
            ],
            [TiposRegistro.Centro] =
            [
                ("facturacion.factura.centro_id", "facturas y tickets"),
                ("facturacion.presupuesto.centro_id", "presupuestos"),
                ("facturacion.pedido_venta.centro_id", "pedidos de venta"),
                ("facturacion.albaran_venta.centro_id", "albaranes de venta"),
                ("gastos.gasto.centro_id", "facturas de proveedor"),
            ],
            [TiposRegistro.PartidaAnalitica] =
            [
                ("contabilidad.imputacion_analitica.partida_id", "imputaciones"),
                ("contabilidad.regla_analitica.partida_id", "reglas de asignación"),
                ("contabilidad.linea_presupuesto.partida_id", "presupuestos"),
                ("contabilidad.partida_analitica.padre_id", "partidas que cuelgan de ella"),
            ],
            [TiposRegistro.ClaveReparto] =
            [
                ("contabilidad.regla_analitica.clave_reparto_id", "reglas de asignación"),
                ("contabilidad.ejecucion_analitica.clave_reparto_id", "repartos"),
            ],
            [TiposRegistro.Campana] =
            [
                ("agro.recepcion.campana_id", "recepciones"),
                ("agro.partida.campana_id", "partidas"),
                ("agro.parte_confeccion.campana_id", "partes de confección"),
                ("agro.liquidacion.campana_id", "liquidaciones"),
            ],
            [TiposRegistro.Categoria] =
            [
                ("agro.linea_clasificacion.categoria_id", "clasificaciones"),
                ("agro.salida_calibradora.categoria_id", "salidas de calibradoras"),
                ("agro.linea_calibrado.categoria_id", "calibrados"),
                ("agro.salida_parte.categoria_id", "partes de confección"),
                ("agro.linea_liquidacion.categoria_id", "liquidaciones"),
                ("agro.precio_liquidacion.categoria_id", "precios de liquidación"),
            ],
            [TiposRegistro.Parcela] =
            [
                ("agro.linea_recepcion.parcela_id", "recepciones"),
                ("agro.tratamiento_parcela.parcela_id", "tratamientos en el cuaderno de campo"),
                ("agro.partida.parcela_id", "partidas"),
                ("agro.certificado_agro.parcela_id", "certificados de la parcela"),
                ("agro.analisis_agro.parcela_id", "análisis (suelo, agua, foliar…)"),
                ("agro.plan_abonado.parcela_id", "planes de abonado"),
            ],
            [TiposRegistro.Agricultor] =
            [
                ("agro.recepcion.agricultor_id", "recepciones"),
                ("agro.autoevaluacion.agricultor_id", "autoevaluaciones (GlobalG.A.P.)"),
                ("agro.partida.agricultor_id", "partidas"),
                ("agro.certificado_agro.agricultor_id", "certificados (ecológico, GlobalG.A.P.)"),
                ("agro.liquidacion.agricultor_id", "liquidaciones"),
                ("agro.movimiento_envase.agricultor_id", "movimientos de envases"),
                ("agro.explotacion_siex.agricultor_id", "datos de la explotación en el cuaderno digital (SIEX)"),
            ],
            [TiposRegistro.TarifaCoste] =
            [
                ("agro.mano_obra_parte.tarifa_id", "partes de confección (mano de obra)"),
                ("agro.maquina_parte.tarifa_id", "partes de confección (maquinaria)"),
            ],
            [TiposRegistro.Gasto] =
            [
                ("compras.devolucion_compra.gasto_abono_id", "devoluciones a proveedor abonadas"),
                ("compras.pedido_compra.gasto_id", "pedidos de compra facturados"),
                ("agro.liquidacion.gasto_id", "la autofactura de una liquidación agrícola"),
            ],
            [TiposRegistro.Actividad] =
            [
                ("catalogo.producto.actividad_negocio_id", "artículos"),
                ("terceros.cliente.actividad_negocio_id", "clientes"),
                ("terceros.proveedor.actividad_negocio_id", "proveedores"),
                ("facturacion.factura.actividad_negocio_id", "facturas"),
                ("gastos.gasto.actividad_negocio_id", "gastos"),
                ("contabilidad.documento_pendiente.actividad_negocio_id", "documentos pendientes de contabilizar"),
                ("contabilidad.regla_analitica.actividad_negocio_id", "reglas de imputación analítica"),
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
        "contabilidad.linea_clave_reparto.clave_reparto_id",
        "agro.articulo_campana.campana_id",
        "agro.precio_liquidacion.campana_id",
        "agro.parcela.agricultor_id",
        "organizacion.visibilidad_actividad.actividad_negocio_id",
        "organizacion.acceso_centro.centro_id",
        "organizacion.caja_centro.centro_id",
        "fiscal.ficha_plastico.producto_id",
        "extensiones.acceso_portal.tercero_id",
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
