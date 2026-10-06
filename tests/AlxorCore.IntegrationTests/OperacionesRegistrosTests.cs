using FluentAssertions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Guardia de «se puede crear pero no corregir»: todo lo que la API permite dar de alta tiene que poder modificarse,
/// eliminarse, darse de baja o anularse. Si se añade un recurso nuevo solo con alta, esta prueba falla hasta que tenga
/// su forma de corregirlo o se declare aquí, con el motivo, por qué no la necesita.
/// </summary>
public sealed class OperacionesRegistrosTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public OperacionesRegistrosTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    /// <summary>Acciones que corrigen o retiran un registro existente.</summary>
    private static readonly string[] AccionesCorrectoras =
        ["anular", "cancelar", "baja", "alta", "rechazar", "estado", "activo", "cierre", "revocar", "reabrir", "despaletizar", "recalcular", "definitiva"];

    /// <summary>Altas que no crean un registro corregible (son operaciones, cálculos o ficheros), con su motivo.</summary>
    private static readonly Dictionary<string, string> SinCorreccion = new(StringComparer.OrdinalIgnoreCase)
    {
        ["/auth/registro"] = "alta de usuario; la cuenta se elimina en /cuenta",
        ["/auth/login"] = "sesión",
        ["/auth/refrescar"] = "sesión",
        ["/auth/recuperar"] = "sesión",
        ["/auth/restablecer"] = "sesión",
        ["/cobros"] = "se anula con /tesoreria/movimientos/{id}/anular",
        ["/pagos"] = "se anula con /tesoreria/movimientos/{id}/anular",
        ["/agro/campanas/{id}/precios/masivo"] = "cada precio se corrige en PUT /agro/precios/{id} o se borra en DELETE /agro/precios/{id}",
        ["/agro/campanas/{id}/precios/propuesta-ventas"] = "solo calcula: no registra nada",
        ["/integraciones/edi/orders"] = "crea un pedido de venta, que se corrige o anula como cualquier pedido",
        ["/integraciones/edi/recadv"] = "solo contrasta el aviso de recepción con el albarán: no registra nada",
        ["/pagos/liquidaciones/previsualizar"] = "solo calcula: no registra nada",
        ["/pagos/liquidaciones/masiva"] = "cada liquidación del lote se anula en /pagos/liquidaciones/{id}/anular (y la remesa en la suya)",
        ["/agro/envases/facturar"] = "cada facturación es un movimiento de envases que se anula en /agro/envases/movimientos/{id}/anular (y anula su albarán)",
        ["/pagos/entregas-cuenta/{id}/aplicar"] = "es un pago de la factura: se anula con /tesoreria/movimientos/{id}/anular",
        ["/tickets"] = "un ticket es una factura simplificada: se anula o rectifica en /facturas",
        ["/facturas/simular"] = "solo calcula: no guarda nada",
        ["/cooperativa/capital/suscripciones"] = "movimiento del libro de aportaciones: se anula en /cooperativa/capital/movimientos/{id}/anular",
        ["/cooperativa/capital/desembolsos"] = "movimiento del libro de aportaciones: se anula en /cooperativa/capital/movimientos/{id}/anular",
        ["/cooperativa/capital/reembolsos"] = "movimiento del libro de aportaciones: se anula en /cooperativa/capital/movimientos/{id}/anular",
        ["/cooperativa/capital/transmisiones"] = "dos movimientos del libro de aportaciones: se anulan juntos en /cooperativa/capital/movimientos/{id}/anular",
        ["/cooperativa/repartos/simular"] = "solo calcula: no guarda nada",
        ["/bodega/liquidaciones/simular"] = "solo calcula: no guarda nada",
        ["/bodega/operaciones/elaboracion"] = "operación de bodega: se deshace en /bodega/operaciones/{id}/anular",
        ["/bodega/operaciones/trasiego"] = "operación de bodega: se deshace en /bodega/operaciones/{id}/anular",
        ["/bodega/operaciones/coupage"] = "operación de bodega: se deshace en /bodega/operaciones/{id}/anular",
        ["/bodega/operaciones/merma"] = "operación de bodega: se deshace en /bodega/operaciones/{id}/anular",
        ["/bodega/operaciones/embotellado"] = "operación de bodega: se deshace en /bodega/operaciones/{id}/anular (las botellas salen de existencias)",
        ["/bodega/operaciones/granel"] = "operación de bodega: se deshace en /bodega/operaciones/{id}/anular (anula su albarán)",
        ["/extensiones/adjuntos/{entidad}/{id}"] = "adjunto: se quita con DELETE /extensiones/adjuntos/{id} (la ruta de alta lleva el registro, no el adjunto)",
        ["/seguro-credito/clasificaciones/{id}/comunicacion"] = "acción: anota lo que comunica la aseguradora en el historial de la clasificación",
        ["/seguro-credito/avisos/{id}/cierre"] = "acción: cierra un aviso de impago (cobrado, indemnizado o retirado)",
        ["/seguro-credito/avisos"] = "aviso de impago: se cierra en /seguro-credito/avisos/{id}/cierre (retirado si fue un error)",
        ["/seguro-credito/clasificaciones"] = "clasificación: se corrige comunicando su cambio (/clasificaciones/{id}/comunicacion)",
        ["/portal/entrar"] = "acción: canjea la clave del enlace del portal por una sesión (no da de alta nada)",
        ["/accesos-portal"] = "acceso al portal: se corrige regenerando la clave (/accesos-portal/{id}/regenerar) o revocándolo (/accesos-portal/{id}/revocar)",
        ["/accesos-portal/{id}/regenerar"] = "acción: nueva clave del acceso al portal",
        ["/accesos-portal/{id}/revocar"] = "acción: revoca el acceso al portal",
        ["/portal/planta/volcados"] = "volcados del terminal: se anulan en /agro/planta/volcados/{id}/anular (reenviar la misma lectura no la duplica)",
        ["/agro/planta/volcados"] = "volcados: se anulan en /agro/planta/volcados/{id}/anular",
        ["/agro/planta/volcados/parte"] = "crea un parte de confección en borrador, que se corrige o se elimina en /agro/partes/{id} (sus volcados vuelven a quedar pendientes)",
        ["/inventario/series/entrada"] = "entrada de unidades con número de serie: se corrige con una salida (/inventario/salida) de cada número",
        ["/inventario/recuentos"] = "recuento: se anula abierto (/inventario/recuentos/{id}/anular); cerrado, sus ajustes se corrigen con otro recuento o un ajuste",
        ["/compras/devoluciones"] = "devolución a proveedor: se anula pendiente (/compras/devoluciones/{id}/anular); abonada, se corrige anulando su abono (gasto)",
        ["/pagos/abonos/{abonoId}/aplicar"] = "acción: aplica un abono a una factura (sus movimientos se anulan en /tesoreria/movimientos/{id}/anular)",
        ["/compras/propuesta/pedidos"] = "acción: crea pedidos de compra en borrador, que se modifican o cancelan en /compras/pedidos",
        ["/inventario/reaprovisionamiento"] = "regla de stock mínimo: se cambia volviendo a fijarla y se quita con DELETE /inventario/reaprovisionamiento/{id}",
        ["/comisiones/agentes"] = "agente: se cambia en PUT /comisiones/agentes/{id} (o se da de baja con activo = false)",
        ["/comisiones/asignaciones"] = "asignación de agente: se corrige asignando otro (o ninguno) en la misma fecha",
        ["/comisiones/reglas"] = "regla de comisión: se quita con DELETE /comisiones/reglas/{id}",
        ["/comisiones/liquidaciones"] = "liquidación de comisiones: se anula en /comisiones/liquidaciones/{id}/anular",
        ["/extensiones/alertas/evaluar"] = "acción: evalúa las reglas, no crea un registro (las alertas se resuelven en /extensiones/alertas/{id}/resolver)",
        ["/albaranes-venta/facturar"] = "emite una factura, que se anula o rectifica en /facturas (y el albarán vuelve a quedar pendiente)",
        ["/albaranes-venta/facturacion-masiva"] = "emite facturas, que se anulan o rectifican en /facturas",
        ["/gastos/cargos-acreedores/liquidar"] = "registra la factura del acreedor (un gasto): se anula en /gastos/{id}/anular y los cargos vuelven a quedar pendientes",
        ["/informes/sii/enviar"] = "remite a la AEAT: lo enviado se corrige con otro envío (modificación A1 o baja), que el propio envío calcula",
        ["/exportar/xlsx"] = "genera un fichero Excel con lo que envía la interfaz: no guarda nada",
        ["/compras/pedidos/simular"] = "solo calcula: no guarda nada",
        ["/gastos/simular"] = "solo calcula: no guarda nada",
        ["/analisis/consulta"] = "consulta de solo lectura (POST por el tamaño de la definición): no guarda nada",
        ["/analisis/detalle"] = "consulta de solo lectura: no guarda nada",
        ["/inventario/entrada"] = "movimiento de almacén: se corrige con otro movimiento (ajuste o salida)",
        ["/inventario/salida"] = "movimiento de almacén: se corrige con otro movimiento",
        ["/inventario/ajuste"] = "es la propia corrección de existencias",
        ["/inventario/traspaso"] = "se deshace con el traspaso contrario",
        ["/inventario/montaje"] = "se deshace con un ajuste o una orden de fabricación",
        ["/inventario/ubicacion-defecto"] = "regla que se sustituye al volver a guardarla",
        ["/tesoreria/remesa"] = "compatibilidad: registra una remesa de adeudos que se anula con /tesoreria/remesas/{id}/anular",
        ["/tesoreria/transferencias"] = "compatibilidad: registra una remesa de transferencias que se anula con /tesoreria/remesas/{id}/anular",
        ["/tesoreria/devoluciones"] = "registro histórico de un recibo devuelto (anula el cobro): se corrige volviendo a cobrar o remesar el recibo",
        ["/tesoreria/cuaderno19"] = "genera un fichero",
        ["/tesoreria/confirming"] = "genera un fichero",
        ["/tesoreria/conciliacion"] = "cálculo de conciliación, no guarda un registro",
        ["/divisas/diferencias-cambio"] = "cálculo",
        ["/impagados/{facturaId}/reclamar"] = "registro de una reclamación enviada (histórico)",
        ["/importar/cartera"] = "importación: lo importado se corrige en su pantalla",
        ["/importar/saldos"] = "importación: el asiento se anula en contabilidad",
        ["/importar/stock"] = "importación: se corrige con ajustes",
        ["/clientes/importar"] = "importación: cada cliente se corrige en su ficha",
        ["/productos/importar"] = "importación: cada artículo se corrige en su ficha",
        ["/migracion/hispatec/validar"] = "validación",
        ["/migracion/hispatec/cargar"] = "carga idempotente: lo cargado se corrige en su pantalla",
        ["/facturas-recurrentes/procesar"] = "emite las facturas vencidas (se anulan en /facturas)",
        ["/integraciones/webhooks/procesar"] = "reintenta entregas",
        ["/recepcion/buzon/procesar"] = "lee el buzón de correo",
        ["/contabilidad/pendientes/contabilizar"] = "contabiliza (los asientos se anulan desde su documento)",
        ["/contabilidad/cierre"] = "cierre del ejercicio",
        ["/contabilidad/periodos/cerrar"] = "cierre mensual: se deshace con /contabilidad/periodos/reabrir",
        ["/contabilidad/periodos/reabrir"] = "es la corrección del cierre mensual",
        ["/agro/planta/calibrados/leer-fichero"] = "solo lee el fichero de la calibradora: no guarda nada",
        ["/agro/planta/ordenes/desde-plan"] = "crea órdenes, que se cambian, cancelan o eliminan una a una en /agro/planta/ordenes/{id}",
        ["/empresas/actual/copia/vista-previa"] = "solo calcula lo que se copiaría: no guarda nada",
        ["/grupos/actual/union/vista-previa"] = "solo calcula lo que pasaría al grupo: no guarda nada",
        ["/grupos/actual/union"] = "mueve la empresa y sus maestros al grupo; cada maestro se corrige o da de baja en su pantalla",
        ["/empresas/actual/copia"] = "da de alta cuentas, formas de pago, series, almacenes…, que se corrigen cada una en su pantalla",
        ["/contabilidad/existencias/{ejercicio}"] = "la regularización del ejercicio se anula en /contabilidad/existencias/{ejercicio}/anular",
        ["/contabilidad/periodificaciones/generar"] = "cuotas del periodo: se deshacen anulando la periodificación",
        ["/contabilidad/inmovilizado/amortizar"] = "dotaciones del periodo (asientos de amortización)",
        ["/contabilidad/analitica/imputar-pendientes"] = "imputa con las reglas (las imputaciones se corrigen una a una)",
        ["/contabilidad/analitica/repartos"] = "se deshace con DELETE /contabilidad/analitica/ejecuciones/{id}",
        ["/contabilidad/presupuestos/desde-real"] = "crea un presupuesto (se corrige en /contabilidad/presupuestos/{id})",
        ["/pedidos-venta/desde-presupuesto"] = "crea un pedido (se corrige en /pedidos-venta/{id})",
        ["/agro/liquidaciones/previsualizar"] = "cálculo sin guardar",
        ["/productos/composiciones/buscar"] = "consulta (compuestos con la misma lista de materiales)",
        ["/productos/composiciones/recalcular-precios"] = "recalcula precios (se corrigen en la ficha de cada artículo)",
        ["/agro/repaletizados"] = "operación de palés: se deshace con otro repaletizado en sentido contrario",
        ["/agro/etiquetas-campo"] = "números SSCC emitidos: como los de los palés, no se reutilizan; la que no se usa se queda libre",
        ["/logistica/paletizar"] = "monta palés de una vez: cada uno se corrige con /logistica/unidades/{id}/contenido o se desmonta con /logistica/unidades/{id}/anular",
        ["/logistica/paletizar-fabricacion"] = "monta palés de una vez: cada uno se corrige con /logistica/unidades/{id}/contenido o se desmonta con /logistica/unidades/{id}/anular",
        ["/agro/pales/montar"] = "monta palés de una vez: cada uno se corrige con /agro/pales/{id}/cajas (reabriéndolo si está cerrado)",
        ["/agro/expediciones"] = "marca palés como expedidos: cada uno se anula con /agro/pales/{id}/anular-expedicion",
        ["/usuarios/invitar"] = "se revoca con /usuarios/{usuarioId}/revocar",
        ["/aprobaciones/solicitudes"] = "se aprueba o rechaza",
        ["/empresas"] = "la empresa se elimina con la baja de la cuenta (/cuenta)",
        ["/integraciones/claves"] = "se revoca con DELETE",
        ["/api/v1/facturas"] = "API pública: las facturas se anulan en /facturas",
        ["/auth/verificar-email"] = "verificación de la cuenta",
        ["/auth/2fa/preparar"] = "configuración del doble factor (se desactiva en /auth/2fa/desactivar)",
        ["/auth/2fa/activar"] = "configuración del doble factor",
        ["/auth/2fa/desactivar"] = "es la propia desactivación",
        ["/productos/{id}/stock"] = "movimiento de stock simple: se corrige con otro movimiento",
        ["/agro/agricultores/{id}/envases"] = "movimiento de envases: se corrige con el movimiento contrario",
        ["/agro/partidas/{id}/ajustes"] = "ajuste de kilos: se corrige con el ajuste contrario",
        ["/agro/fitosanitarios/importar-mapa"] = "fichero del registro del ministerio: se corrige con el siguiente fichero (o editando el producto)",
        ["/agro/fitosanitarios/cargar"] = "carga del registro del ministerio: se corrige con la siguiente carga (o editando el producto)",
        ["/agro/partidas/{id}/clasificaciones"] = "se corrige con una clasificación nueva: la definitiva sustituye a la anterior (si la partida no está liquidada)",
    };

    /// <summary>Altas de registros hijos que se corrigen en otra ruta (que tiene que existir).</summary>
    private static readonly Dictionary<string, string> CorreccionEn = new(StringComparer.OrdinalIgnoreCase)
    {
        ["/productos/{id}/variantes"] = "PUT /productos/{id}",
        ["/proyectos/{id}/mano-obra"] = "DELETE /proyectos/{id}/imputaciones/{imputacionId}",
        ["/proyectos/{id}/material"] = "DELETE /proyectos/{id}/imputaciones/{imputacionId}",
        ["/proyectos/{id}/gasto"] = "DELETE /proyectos/{id}/imputaciones/{imputacionId}",
        ["/agro/campanas/{id}/precios"] = "PUT /agro/precios/{id}",
        ["/agro/agricultores/{id}/parcelas"] = "PUT /agro/parcelas/{id}",
        ["/agro/recepciones/{id}/lineas/{lineaId}/pesadas"] = "DELETE /agro/recepciones/{id}/pesadas/{pesadaId}",
        ["/cartera/{id}/movimientos"] = "POST /tesoreria/movimientos/{id}/anular",
    };

    [Fact]
    public void Todo_lo_que_se_da_de_alta_se_puede_corregir()
    {
        var rutas = _fabrica.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>()
            .Where(e => !(e.RoutePattern.RawText ?? string.Empty).Contains("{*", StringComparison.Ordinal))
            .SelectMany(e => (e.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? []).Select(m => (Metodo: m, Ruta: Normalizar(e.RoutePattern.RawText))))
            .Distinct().ToList();

        var sinCorreccion = new List<string>();
        foreach (var (metodo, ruta) in rutas.Where(r => r.Metodo == "POST"))
        {
            if (SinCorreccion.ContainsKey(ruta) || EsAccion(ruta))
            {
                continue;
            }

            if (CorreccionEn.TryGetValue(ruta, out var correccion))
            {
                if (!rutas.Contains((correccion.Split(' ')[0], correccion.Split(' ')[1])))
                {
                    sinCorreccion.Add($"{metodo} {ruta} (no existe su corrección «{correccion}»)");
                }

                continue;
            }

            // Registros hijos de la ruta de alta: «/x» → «/x/{id}…».
            var hijos = rutas.Where(r => r.Ruta.StartsWith(ruta + "/{", StringComparison.OrdinalIgnoreCase)).ToList();
            var corrige = hijos.Any(r => r.Metodo is "PUT" or "DELETE" or "PATCH")
                          || hijos.Any(r => r.Metodo == "POST" && AccionesCorrectoras.Any(a => r.Ruta.EndsWith("/" + a, StringComparison.OrdinalIgnoreCase)));
            if (!corrige)
            {
                sinCorreccion.Add($"{metodo} {ruta}");
            }
        }

        string.Join(" | ", sinCorreccion).Should().BeEmpty(
            "cada alta necesita su modificación, eliminación, baja o anulación (o declararse en SinCorreccion con el motivo)");
    }

    /// <summary>Una ruta de acción sobre un registro existente («/x/{id}/confirmar»), no un alta.</summary>
    private static bool EsAccion(string ruta)
    {
        var ultimo = ruta.TrimEnd('/').Split('/')[^1];
        var penultimo = ruta.TrimEnd('/').Split('/').Reverse().Skip(1).FirstOrDefault() ?? string.Empty;
        return penultimo.StartsWith('{') && !ultimo.StartsWith('{') && AltaDeHijo(ultimo) is false;
    }

    /// <summary>Altas de registros hijos que sí se comprueban («/agro/agricultores/{id}/parcelas»).</summary>
    private static bool AltaDeHijo(string segmento) => segmento is "parcelas" or "lineas" or "precios" or "clasificaciones" or "ajustes"
        or "variantes" or "pesadas" or "imputaciones" or "movimientos" or "envases" or "stock" or "mano-obra" or "material" or "gasto";

    private static string Normalizar(string? plantilla)
    {
        var r = "/" + (plantilla ?? string.Empty).Trim('/');
        return System.Text.RegularExpressions.Regex.Replace(r, @"\{(\w+)(:[^}]*)?\}", m => "{" + m.Groups[1].Value + "}");
    }
}
