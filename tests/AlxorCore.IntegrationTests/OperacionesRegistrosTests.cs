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
        ["/contabilidad/periodificaciones/generar"] = "cuotas del periodo: se deshacen anulando la periodificación",
        ["/contabilidad/inmovilizado/amortizar"] = "dotaciones del periodo (asientos de amortización)",
        ["/contabilidad/analitica/imputar-pendientes"] = "imputa con las reglas (las imputaciones se corrigen una a una)",
        ["/contabilidad/analitica/repartos"] = "se deshace con DELETE /contabilidad/analitica/ejecuciones/{id}",
        ["/contabilidad/presupuestos/desde-real"] = "crea un presupuesto (se corrige en /contabilidad/presupuestos/{id})",
        ["/pedidos-venta/desde-presupuesto"] = "crea un pedido (se corrige en /pedidos-venta/{id})",
        ["/agro/liquidaciones/previsualizar"] = "cálculo sin guardar",
        ["/productos/composiciones/buscar"] = "consulta (compuestos con la misma lista de materiales)",
        ["/productos/composiciones/recalcular-precios"] = "recalcula precios (se corrigen en la ficha de cada artículo)",
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
