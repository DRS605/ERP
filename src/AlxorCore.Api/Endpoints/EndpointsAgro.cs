using AlxorCore.Agro.Aplicacion;
using AlxorCore.Agro.Dominio;
using AlxorCore.Api.Comun;
using AlxorCore.Documentos.Aplicacion;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Organizacion.Aplicacion.Puertos;

namespace AlxorCore.Api.Endpoints;

/// <summary>
/// Módulo agro (hortofrutícola): maestros, recepciones con pesadas y envases, partidas, clasificación,
/// liquidaciones al agricultor con autofactura, confección, palés SSCC, expedición, trazabilidad e informe de campaña.
/// </summary>
public static class EndpointsAgro
{
    public sealed record PeticionMotivo(string? Motivo);

    public sealed record PeticionActivo(bool Activo);

    public static IEndpointRouteBuilder MapearAgro(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var g = rutas.MapGroup("/agro").WithTags("Agro");
        MapearMaestros(g);
        MapearRecepciones(g);
        MapearLiquidaciones(g);
        MapearConfeccion(g);
        return rutas;
    }

    private static void MapearMaestros(RouteGroupBuilder g)
    {
        g.MapPut("/campanas/{id:guid}", async (Guid id, DatosCampana datos, MaestrosAgro caso, CancellationToken ct) =>
                (await caso.ActualizarCampanaAsync(id, datos, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Modifica nombre y fechas de una campaña (con movimientos, solo se amplía).").RequierePermiso(Permisos.AgroGestionar);
        g.MapDelete("/campanas/{id:guid}", async (Guid id, MaestrosAgro caso, CancellationToken ct) =>
                (await caso.EliminarCampanaAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Elimina una campaña sin movimientos (con su configuración de artículos y precios).").RequierePermiso(Permisos.AgroGestionar);
        g.MapPut("/categorias/{id:guid}", async (Guid id, DatosCategoria datos, MaestrosAgro caso, CancellationToken ct) =>
                (await caso.ActualizarCategoriaAsync(id, datos, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Modifica nombre y orden de una categoría.").RequierePermiso(Permisos.AgroGestionar);
        g.MapDelete("/categorias/{id:guid}", async (Guid id, MaestrosAgro caso, CancellationToken ct) =>
                (await caso.EliminarCategoriaAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Elimina una categoría sin clasificaciones, partes ni liquidaciones.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPut("/tarifas/{id:guid}", async (Guid id, DatosTarifa datos, MaestrosAgro caso, CancellationToken ct) =>
                (await caso.ActualizarTarifaAsync(id, datos, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Modifica una tarifa de coste (si ya valoró partes, solo se cierra su vigencia).").RequierePermiso(Permisos.AgroGestionar);
        g.MapDelete("/tarifas/{id:guid}", async (Guid id, MaestrosAgro caso, CancellationToken ct) =>
                (await caso.EliminarTarifaAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Elimina una tarifa de coste que no ha valorado partes.").RequierePermiso(Permisos.AgroGestionar);
        g.MapDelete("/parcelas/{id:guid}", async (Guid id, MaestrosAgro caso, CancellationToken ct) =>
                (await caso.EliminarParcelaAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Elimina una parcela sin recepciones.").RequierePermiso(Permisos.AgroGestionar);
        g.MapDelete("/agricultores/{id:guid}", async (Guid id, MaestrosAgro caso, CancellationToken ct) =>
                (await caso.EliminarAgricultorAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Elimina la ficha agrícola de un agricultor sin movimientos (si no, bloquéalo).").RequierePermiso(Permisos.AgroGestionar);

        g.MapGet("/campanas", (IContextoEmpresa c, MaestrosAgro m, CancellationToken ct) => ConEmpresa(c, async e => Results.Ok(await m.CampanasAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Campañas.").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/campanas", (DatosCampana d, IContextoEmpresa c, MaestrosAgro m, CancellationToken ct) => ConEmpresa(c, async e => Creado(await m.CrearCampanaAsync(e, d, ct).ConfigureAwait(false), "campanas")))
            .WithSummary("Crea una campaña (sin solapes con otras).").RequierePermiso(Permisos.AgroGestionar);
        g.MapGet("/campanas/{id:guid}/articulos", async (Guid id, MaestrosAgro m, CancellationToken ct) => Results.Ok(await m.ArticulosCampanaAsync(id, ct).ConfigureAwait(false)))
            .WithSummary("Cómo se liquida cada artículo en la campaña.").RequierePermiso(Permisos.AgroLeer);
        g.MapPut("/campanas/{id:guid}/articulos", (Guid id, DatosArticuloCampana d, IContextoEmpresa c, MaestrosAgro m, CancellationToken ct) =>
                ConEmpresa(c, async e => (await m.FijarArticuloCampanaAsync(e, id, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Fija el método de liquidación de un artículo (por clasificación o por periodo).").RequierePermiso(Permisos.AgroLiquidar);
        g.MapGet("/campanas/{id:guid}/precios", async (Guid id, MaestrosAgro m, CancellationToken ct) => Results.Ok(await m.PreciosAsync(id, ct).ConfigureAwait(false)))
            .WithSummary("Precios de liquidación de la campaña.").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/campanas/{id:guid}/precios", (Guid id, DatosPrecio d, IContextoEmpresa c, MaestrosAgro m, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await m.CrearPrecioAsync(e, id, d, ct).ConfigureAwait(false), "precios")))
            .WithSummary("Añade un precio (€/kg) por artículo, categoría y periodo, sin solapes.").RequierePermiso(Permisos.AgroLiquidar);
        g.MapGet("/rendimientos", (IContextoEmpresa c, MaestrosAgro m, CancellationToken ct) => ConEmpresa(c, async e => Results.Ok(await m.RendimientosAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Rendimientos teóricos de confección (cajas por hora por producto y envase).").RequierePermiso(Permisos.AgroLeer);
        g.MapPut("/rendimientos", (DatosRendimiento d, IContextoEmpresa c, MaestrosAgro m, CancellationToken ct) =>
                ConEmpresa(c, async e => (await m.GuardarRendimientoAsync(e, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Da de alta o cambia el rendimiento de un producto y envase.").RequierePermiso(Permisos.AgroGestionar);
        g.MapDelete("/rendimientos/{id:guid}", async (Guid id, MaestrosAgro m, CancellationToken ct) => (await m.EliminarRendimientoAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Elimina un rendimiento (los partes ya validados no cambian).").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/campanas/{id:guid}/precios/masivo", (Guid id, DatosPreciosMasivos d, IContextoEmpresa c, MaestrosAgro m, CancellationToken ct) =>
                ConEmpresa(c, async e => (await m.FijarPreciosAsync(e, id, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Fijación masiva: da de alta o actualiza muchos precios de la campaña (día, periodo o general; por envase).").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/campanas/{id:guid}/precios/propuesta-ventas", async (Guid id, DatosPropuestaVentas d, MaestrosAgro m, CancellationToken ct) =>
                (await m.ProponerDesdeVentasAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Liquidación a resultas: precio medio de venta de cada artículo en las fechas, menos la deducción (no guarda).").RequierePermiso(Permisos.AgroLeer);
        g.MapPut("/precios/{id:guid}", async (Guid id, DatosPrecio d, MaestrosAgro m, CancellationToken ct) => (await m.ActualizarPrecioAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Cambia el periodo o el importe de un precio no aplicado en una liquidación emitida.").RequierePermiso(Permisos.AgroLiquidar);
        g.MapDelete("/precios/{id:guid}", async (Guid id, MaestrosAgro m, CancellationToken ct) => (await m.EliminarPrecioAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Elimina un precio que no se haya aplicado en una liquidación emitida.").RequierePermiso(Permisos.AgroLiquidar);

        g.MapGet("/agricultores", (IContextoEmpresa c, MaestrosAgro m, CancellationToken ct) => ConEmpresa(c, async e => Results.Ok(await m.AgricultoresAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Agricultores (proveedores con ficha agrícola).").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/agricultores", (DatosAgricultor d, IContextoEmpresa c, MaestrosAgro m, CancellationToken ct) => ConEmpresa(c, async e => Creado(await m.CrearAgricultorAsync(e, d, ct).ConfigureAwait(false), "agricultores")))
            .WithSummary("Da de alta a un proveedor como agricultor: régimen (REAGP o general), retención y autorización de autofacturación.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPut("/agricultores/{id:guid}", async (Guid id, DatosAgricultor d, MaestrosAgro m, CancellationToken ct) => (await m.ActualizarAgricultorAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Modifica la ficha agrícola (régimen, impuesto, retención, autofacturación, bloqueo).").RequierePermiso(Permisos.AgroGestionar);
        g.MapGet("/agricultores/{id:guid}/envases", async (Guid id, RecepcionesAgro r, CancellationToken ct) =>
                Results.Ok(new { Saldos = await r.SaldoEnvasesAsync(id, ct).ConfigureAwait(false), Movimientos = await r.MovimientosEnvaseAsync(id, ct).ConfigureAwait(false) }))
            .WithSummary("Envases de la empresa que tiene el agricultor y sus movimientos.").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/agricultores/{id:guid}/envases", (Guid id, DatosMovimientoEnvase d, IContextoEmpresa c, RecepcionesAgro r, CancellationToken ct) =>
                ConEmpresa(c, async e => (await r.MoverEnvasesAsync(e, id, d, ct).ConfigureAwait(false)).ASinContenido()))
            .WithSummary("Entrega (+) o devolución (−) de envases vacíos.").RequierePermiso(Permisos.AgroGestionar);
        // Reservas de palés a líneas de pedidos de venta.
        g.MapGet("/reservas", (Guid pedidoVentaId, IContextoEmpresa c, ReservasPales r, CancellationToken ct) =>
                ConEmpresa(c, async e => (await r.DePedidoAsync(e, pedidoVentaId, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Reservas de un pedido: por línea, lo pedido, servido y reservado; sus reservas y los palés disponibles.").RequierePermiso(Permisos.AgroLeer);
        g.MapGet("/reservas/activas", (IContextoEmpresa c, ReservasPales r, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await r.ActivasAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Reservas activas de la empresa (palés apartados para un pedido).").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/reservas", (ReservarPalesComando d, IContextoEmpresa c, ReservasPales r, CancellationToken ct) =>
                ConEmpresa(c, async e => (await r.ReservarAsync(e, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Reserva palés cerrados para una línea de un pedido de venta.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/reservas/{id:guid}/anular", async (Guid id, AnularPeticionEnvases? p, ReservasPales r, CancellationToken ct) =>
                (await r.AnularAsync(id, p?.Motivo, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Anula una reserva activa (el palé queda libre).").RequierePermiso(Permisos.AgroGestionar);

        // Envases retornables por tercero (clientes, proveedores, transportistas y pools).
        g.MapGet("/envases/cuentas", (DateOnly? hasta, IContextoEmpresa c, EnvasesTerceros v, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await v.CuentasAsync(e, hasta, ct).ConfigureAwait(false))))
            .WithSummary("Cuentas de envases con su saldo por envase (a una fecha, si se indica).").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/envases/cuentas", (CrearCuentaEnvasesComando d, IContextoEmpresa c, EnvasesTerceros v, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await v.CrearCuentaAsync(e, d, ct).ConfigureAwait(false), "envases/cuentas")))
            .WithSummary("Abre la cuenta de envases de un cliente, proveedor, transportista o pool.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPut("/envases/cuentas/{id:guid}", async (Guid id, ConfigurarCuentaEnvasesComando d, EnvasesTerceros v, CancellationToken ct) =>
                (await v.ConfigurarCuentaAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Agrupadora, llevar al transportista, bloqueo o aviso, límite y baja de la cuenta.").RequierePermiso(Permisos.AgroGestionar);
        g.MapGet("/envases/cuentas/{id:guid}/extracto", (Guid id, DateOnly? desde, DateOnly? hasta, IContextoEmpresa c, EnvasesTerceros v, CancellationToken ct) =>
                ConEmpresa(c, async e => (await v.ExtractoAsync(e, id, desde, hasta, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Extracto de la cuenta: saldo inicial, movimientos con el acumulado y saldo final.").RequierePermiso(Permisos.AgroLeer);
        g.MapGet("/envases/movimientos", (Guid? cuentaId, DateOnly? desde, DateOnly? hasta, IContextoEmpresa c, EnvasesTerceros v, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await v.MovimientosAsync(e, cuentaId, desde, hasta, ct).ConfigureAwait(false))))
            .WithSummary("Movimientos de envases (de una cuenta o de todas).").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/envases/movimientos", (MovimientoEnvasesComando d, IContextoEmpresa c, EnvasesTerceros v, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await v.RegistrarAsync(e, d, ct).ConfigureAwait(false), "envases/movimientos")))
            .WithSummary("Entrega (+) o recogida (−) de envases a un tercero, o una regularización.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/envases/movimientos/{id:guid}/anular", async (Guid id, AnularPeticionEnvases? p, EnvasesTerceros v, CancellationToken ct) =>
                (await v.AnularAsync(id, p?.Motivo, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Anula un movimiento con su contrario (el libro es de solo inserción).").RequierePermiso(Permisos.AgroGestionar);
        g.MapGet("/envases/cuentas/{id:guid}/limites", (Guid id, IContextoEmpresa c, EnvasesTerceros v, CancellationToken ct) =>
                ConEmpresa(c, async e => (await v.LimitesAsync(e, id, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Límite y mínimo de cada envase de la cuenta, con su saldo.").RequierePermiso(Permisos.AgroLeer);
        g.MapPut("/envases/cuentas/{id:guid}/limites", (Guid id, FijarLimitesEnvasesComando d, IContextoEmpresa c, EnvasesTerceros v, CancellationToken ct) =>
                ConEmpresa(c, async e => (await v.FijarLimitesAsync(e, id, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Fija los límites y mínimos por envase y si se avisa o se bloquea al superarlos.").RequierePermiso(Permisos.AgroGestionar);
        g.MapGet("/envases/configuracion", (IContextoEmpresa c, EnvasesTerceros v, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await v.ConfiguracionAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Fecha de cierre de los movimientos de envases.").RequierePermiso(Permisos.AgroLeer);
        g.MapPut("/envases/configuracion", (ConfiguracionEnvasesDto d, IContextoEmpresa c, EnvasesTerceros v, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await v.CerrarAsync(e, d.FechaCierre, ct).ConfigureAwait(false))))
            .WithSummary("Cierra los movimientos de envases hasta una fecha (null: reabre).").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/envases/cuentas/{id:guid}/facturar", (Guid id, FacturarEnvasesComando? d, IContextoEmpresa c, EnvasesTerceros v, CancellationToken ct) =>
                ConEmpresa(c, async e => (await v.FacturarAsync(e, id, d ?? new FacturarEnvasesComando(), ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Factura envases a un cliente: albarán de venta directo y salida de su saldo (sin líneas, según su gestión).").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/envases/facturar", (FacturarEnvasesComando? d, IContextoEmpresa c, EnvasesTerceros v, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await v.FacturarMasivoAsync(e, d?.Fecha, ct).ConfigureAwait(false))))
            .WithSummary("Factura los envases de todos los clientes con envases a facturar (todo o el exceso sobre el límite).").RequierePermiso(Permisos.AgroGestionar);
        g.MapGet("/envases/cuentas/{id:guid}/fichero-pool", (Guid id, DateOnly desde, DateOnly hasta, IContextoEmpresa c, EnvasesTerceros v, CancellationToken ct) =>
                ConEmpresa(c, async e =>
                {
                    var r = await v.FicheroPoolAsync(e, id, desde, hasta, ct).ConfigureAwait(false);
                    return r.EsCorrecto ? Results.File(System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(r.Valor.Csv)).ToArray(), "text/csv", r.Valor.Nombre)
                        : r.AOk();
                }))
            .WithSummary("Fichero CSV de declaración al pool: movimientos de sus envases con todos los terceros en el periodo.").RequierePermiso(Permisos.AgroLeer);
        g.MapGet("/envases/stock-terceros", (DateOnly? hasta, IContextoEmpresa c, EnvasesTerceros v, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await v.StockEnTercerosAsync(e, hasta, ct).ConfigureAwait(false))))
            .WithSummary("Envases en poder de clientes, proveedores y agricultores, transportistas y pools.").RequierePermiso(Permisos.AgroLeer);
        g.MapGet("/tratamientos", (Guid? agricultorId, Guid? parcelaId, DateOnly? desde, DateOnly? hasta, IContextoEmpresa c, CuadernoCampoAgro q, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await q.ListarAsync(e, agricultorId, parcelaId, desde, hasta, ct).ConfigureAwait(false))))
            .WithSummary("Tratamientos fitosanitarios de las parcelas (cuaderno de campo).").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/tratamientos", (DatosTratamiento d, IContextoEmpresa c, CuadernoCampoAgro q, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await q.RegistrarAsync(e, d, ct).ConfigureAwait(false), "tratamientos")))
            .WithSummary("Registra un tratamiento de una parcela con su plazo de seguridad.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/tratamientos/{id:guid}/anular", async (Guid id, AnularPeticionEnvases? p, CuadernoCampoAgro q, CancellationToken ct) =>
                (await q.AnularAsync(id, p?.Motivo, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Anula un tratamiento (no se borra).").RequierePermiso(Permisos.AgroGestionar);
        g.MapGet("/fitosanitarios", (string? buscar, IContextoEmpresa c, RegistroFitosanitarios q, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await q.ListarAsync(e, buscar, ct).ConfigureAwait(false))))
            .WithSummary("Productos del Registro Oficial de Productos Fitosanitarios (busca por nombre, número o materia activa).").RequierePermiso(Permisos.AgroLeer);
        g.MapGet("/fitosanitarios/{id:guid}", async (Guid id, RegistroFitosanitarios q, CancellationToken ct) =>
                Encontrado(await q.ObtenerAsync(id, ct).ConfigureAwait(false), "fitosanitario"))
            .WithSummary("Un producto del registro con sus materias activas y usos autorizados.").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/fitosanitarios", (DatosFitosanitario d, IContextoEmpresa c, RegistroFitosanitarios q, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await q.CrearAsync(e, d, ct).ConfigureAwait(false), "fitosanitarios")))
            .WithSummary("Da de alta a mano un producto del registro.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPut("/fitosanitarios/{id:guid}", async (Guid id, DatosFitosanitario d, RegistroFitosanitarios q, CancellationToken ct) =>
                (await q.ActualizarAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Cambia un producto del registro (los cambios que importan quedan como aviso) o su artículo del almacén.").RequierePermiso(Permisos.AgroGestionar);
        g.MapDelete("/fitosanitarios/{id:guid}", async (Guid id, RegistroFitosanitarios q, CancellationToken ct) =>
                (await q.EliminarAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Elimina un producto del registro que no está en ningún tratamiento.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/fitosanitarios/cargar", (CargaRegistroFito d, IContextoEmpresa c, RegistroFitosanitarios q, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await q.CargarAsync(e, d, ct).ConfigureAwait(false))))
            .WithSummary("Carga del registro del ministerio: altas, cambios (quedan como avisos) y, si es completa, retirada de lo que no viene.")
            .RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/fitosanitarios/importar-mapa", (HttpRequest peticion, bool? completa, IContextoEmpresa c, RegistroFitosanitarios q, CancellationToken ct) =>
                ConEmpresa(c, async e =>
                {
                    LecturaRegistroMapa lectura;
                    try
                    {
                        lectura = await ImportadorRegistroMapa.LeerAsync(peticion.Body, completa ?? true, ct).ConfigureAwait(false);
                    }
                    catch (Exception ex) when (ex is System.Text.Json.JsonException or FormatException)
                    {
                        return ResultadosHttp.AProblema(Error.Validacion("fitosanitario.fichero", $"El fichero no es el JSON del registro del ministerio: {ex.Message}"));
                    }

                    var r = await q.CargarAsync(e, lectura.Carga, ct).ConfigureAwait(false);
                    return Results.Ok(r with { Detalle = [.. lectura.Avisos, .. r.Detalle] });
                }))
            .WithMetadata(new Microsoft.AspNetCore.Mvc.RequestSizeLimitAttribute(1_073_741_824))
            .WithSummary("Carga el fichero JSON del Registro Oficial de Productos Fitosanitarios del MAPA tal cual (cuerpo de la petición). Por defecto, como registro completo.")
            .RequierePermiso(Permisos.AgroGestionar);
        g.MapGet("/fitosanitarios/avisos", (DateOnly? desde, bool? pendientes, IContextoEmpresa c, RegistroFitosanitarios q, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await q.CambiosAsync(e, desde, pendientes ?? true, ct).ConfigureAwait(false))))
            .WithSummary("Cambios del registro detectados en las cargas, con quién tiene el producto en el almacén o lo ha aplicado.").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/fitosanitarios/avisos/{id:guid}/revisado", async (Guid id, RegistroFitosanitarios q, CancellationToken ct) =>
                (await q.MarcarRevisadoAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Marca un aviso del registro como revisado.").RequierePermiso(Permisos.AgroGestionar);
        g.MapGet("/fitosanitarios/trazabilidad", (Guid articuloId, string lote, IContextoEmpresa c, TrazabilidadFitosanitarios q, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await q.DeLoteAsync(e, articuloId, lote, ct).ConfigureAwait(false))))
            .WithSummary("Traza de un lote de fitosanitario: tratamientos, partidas recolectadas después en esas parcelas y palés y clientes.").RequierePermiso(Permisos.AgroLeer);
        g.MapGet("/fitosanitarios/de-partida", (Guid? partidaId, string? sscc, IContextoEmpresa c, TrazabilidadFitosanitarios q, CancellationToken ct) =>
                ConEmpresa(c, async e => (await q.DePartidaAsync(e, partidaId, sscc, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Tratamientos (con su lote) que recibieron las parcelas de origen de una partida o un palé.").RequierePermiso(Permisos.AgroLeer);
        g.MapGet("/listas-control", (IContextoEmpresa c, AutoevaluacionesAgro q, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await q.ListasAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Listas de puntos de control (GlobalG.A.P. u otras normas).").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/listas-control", (DatosListaControl d, IContextoEmpresa c, AutoevaluacionesAgro q, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await q.CrearListaAsync(e, d, ct).ConfigureAwait(false), "listas-control")))
            .WithSummary("Crea una lista de puntos de control con su nivel (Mayor, Menor, Recomendacion).").RequierePermiso(Permisos.AgroGestionar);
        g.MapPut("/listas-control/{id:guid}", async (Guid id, DatosListaControl d, AutoevaluacionesAgro q, CancellationToken ct) =>
                (await q.ActualizarListaAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Modifica la lista (las evaluaciones ya abiertas conservan sus puntos).").RequierePermiso(Permisos.AgroGestionar);
        g.MapDelete("/listas-control/{id:guid}", async (Guid id, AutoevaluacionesAgro q, CancellationToken ct) =>
            {
                var r = await q.EliminarListaAsync(id, ct).ConfigureAwait(false);
                return r.EsCorrecto ? Results.Ok(new { eliminado = r.Valor }) : ResultadosHttp.AProblema(r.Error);
            })
            .WithSummary("Elimina la lista si no tiene evaluaciones; si las tiene, la da de baja.").RequierePermiso(Permisos.AgroGestionar);
        g.MapGet("/autoevaluaciones", (Guid? agricultorId, int? anio, IContextoEmpresa c, AutoevaluacionesAgro q, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await q.ListarAsync(e, agricultorId, anio, ct).ConfigureAwait(false))))
            .WithSummary("Autoevaluaciones y auditorías internas de GlobalG.A.P.").RequierePermiso(Permisos.AgroLeer);
        g.MapGet("/autoevaluaciones/{id:guid}", async (Guid id, AutoevaluacionesAgro q, CancellationToken ct) =>
                Encontrado(await q.ObtenerAsync(id, ct).ConfigureAwait(false), "autoevaluacion"))
            .WithSummary("Una evaluación con sus respuestas y su resultado.").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/autoevaluaciones", (DatosAutoevaluacion d, IContextoEmpresa c, AutoevaluacionesAgro q, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await q.AbrirAsync(e, d, ct).ConfigureAwait(false), "autoevaluaciones")))
            .WithSummary("Abre una autoevaluación o auditoría interna con los puntos de la lista.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPut("/autoevaluaciones/{id:guid}", async (Guid id, DatosRespuestas d, AutoevaluacionesAgro q, CancellationToken ct) =>
                (await q.ResponderAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Responde puntos de una evaluación abierta (cumple, no cumple con su acción correctiva, no aplica con su justificación).")
            .RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/autoevaluaciones/{id:guid}/cerrar", async (Guid id, AutoevaluacionesAgro q, CancellationToken ct) =>
                (await q.CerrarAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Cierra la evaluación: todo respondido; después no cambia.").RequierePermiso(Permisos.AgroGestionar);
        g.MapDelete("/autoevaluaciones/{id:guid}", async (Guid id, AutoevaluacionesAgro q, CancellationToken ct) =>
                (await q.EliminarAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Elimina una evaluación abierta (una cerrada no se borra).").RequierePermiso(Permisos.AgroGestionar);
        g.MapGet("/agricultores/{id:guid}/cuaderno", (Guid id, DateOnly? desde, DateOnly? hasta, IContextoEmpresa c, CuadernoCampoAgro q, CancellationToken ct) =>
                ConEmpresa(c, async e => (await q.CuadernoAsync(e, id, desde, hasta, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Cuaderno de campo del agricultor: tratamientos, recolecciones e incidencias de plazo de seguridad.").RequierePermiso(Permisos.AgroLeer);
        g.MapGet("/envases/informe-limites", (DateOnly? sinMovimientosDesde, IContextoEmpresa c, EnvasesTerceros v, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await v.InformeLimitesAsync(e, sinMovimientosDesde, ct).ConfigureAwait(false))))
            .WithSummary("Cuentas sobre su límite, bajo un mínimo o sin movimientos desde una fecha.").RequierePermiso(Permisos.AgroLeer);

        g.MapPost("/agricultores/{id:guid}/parcelas", (Guid id, DatosParcela d, IContextoEmpresa c, MaestrosAgro m, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await m.CrearParcelaAsync(e, id, d, ct).ConfigureAwait(false), "parcelas")))
            .WithSummary("Añade una parcela al agricultor (SIGPAC, superficie, cultivo, centro analítico).").RequierePermiso(Permisos.AgroGestionar);
        g.MapGet("/parcelas", (Guid? agricultorId, IContextoEmpresa c, MaestrosAgro m, CancellationToken ct) => ConEmpresa(c, async e => Results.Ok(await m.ParcelasAsync(e, agricultorId, ct).ConfigureAwait(false))))
            .WithSummary("Parcelas (de todos o de un agricultor).").RequierePermiso(Permisos.AgroLeer);
        g.MapPut("/parcelas/{id:guid}", async (Guid id, DatosParcela d, MaestrosAgro m, CancellationToken ct) => (await m.ActualizarParcelaAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Modifica una parcela.").RequierePermiso(Permisos.AgroGestionar);

        // Taras de envases, versionadas por fecha.
        g.MapGet("/taras", (Guid? envaseProductoId, IContextoEmpresa c, TarasAgro t, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await t.ListarAsync(e, envaseProductoId, ct).ConfigureAwait(false))))
            .WithSummary("Taras de los envases (palot, box, caja, palé…) con su vigencia.").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/taras", (DatosTara d, IContextoEmpresa c, TarasAgro t, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await t.CrearAsync(e, d, ct).ConfigureAwait(false), "taras")))
            .WithSummary("Nueva versión de la tara de un envase desde una fecha (cierra la anterior el día antes).").RequierePermiso(Permisos.AgroGestionar);
        g.MapPut("/taras/{id:guid}", async (Guid id, DatosTara d, TarasAgro t, CancellationToken ct) => (await t.ActualizarAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Corrige una tara que aún no se ha aplicado en ninguna pesada.").RequierePermiso(Permisos.AgroGestionar);
        g.MapDelete("/taras/{id:guid}", async (Guid id, TarasAgro t, CancellationToken ct) => (await t.EliminarAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Elimina una tara que aún no se ha aplicado.").RequierePermiso(Permisos.AgroGestionar);

        // Certificaciones: certificados de agricultores y parcelas, cómo se vende cada artículo y descalificaciones.
        g.MapGet("/certificados", (Guid? agricultorId, IContextoEmpresa c, CertificacionesAgro m, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await m.CertificadosAsync(e, agricultorId, ct).ConfigureAwait(false))))
            .WithSummary("Certificados (ecológico, GlobalG.A.P., GRASP) de los agricultores o de uno.").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/certificados", (DatosCertificado d, IContextoEmpresa c, CertificacionesAgro m, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await m.CrearCertificadoAsync(e, d, ct).ConfigureAwait(false), "certificados")))
            .WithSummary("Da de alta un certificado del agricultor (o de una parcela) con su número y vigencia.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPut("/certificados/{id:guid}", async (Guid id, DatosCertificado d, CertificacionesAgro m, CancellationToken ct) =>
                (await m.ActualizarCertificadoAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Cambia el número, el organismo o la vigencia del certificado.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/certificados/{id:guid}/baja", async (Guid id, CertificacionesAgro m, CancellationToken ct) => (await m.BajaCertificadoAsync(id, true, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Da de baja el certificado (retirado o suspendido): deja de certificar lo que se reciba.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/certificados/{id:guid}/alta", async (Guid id, CertificacionesAgro m, CancellationToken ct) => (await m.BajaCertificadoAsync(id, false, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Vuelve a dar de alta el certificado.").RequierePermiso(Permisos.AgroGestionar);
        g.MapGet("/declaraciones", (IContextoEmpresa c, CertificacionesAgro m, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await m.DeclaracionesAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Cómo se vende cada artículo: las certificaciones que exige a su fruta.").RequierePermiso(Permisos.AgroLeer);
        g.MapPut("/declaraciones/{productoId:guid}", (Guid productoId, DatosDeclaracion d, IContextoEmpresa c, CertificacionesAgro m, CancellationToken ct) =>
                ConEmpresa(c, async e => (await m.DeclararAsync(e, productoId, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Declara el artículo como ecológico, GlobalG.A.P., GRASP o convencional (Ninguna).").RequierePermiso(Permisos.AgroGestionar);
        g.MapDelete("/declaraciones/{productoId:guid}", (Guid productoId, IContextoEmpresa c, CertificacionesAgro m, CancellationToken ct) =>
                ConEmpresa(c, async e => (await m.QuitarDeclaracionAsync(e, productoId, ct).ConfigureAwait(false)).ASinContenido()))
            .WithSummary("Quita la declaración del artículo (deja de comprobarse).").RequierePermiso(Permisos.AgroGestionar);
        g.MapGet("/partidas/{id:guid}/descalificaciones", async (Guid id, CertificacionesAgro m, CancellationToken ct) =>
                Results.Ok(await m.DescalificacionesAsync(id, ct).ConfigureAwait(false)))
            .WithSummary("Descalificaciones de la partida: qué perdió, por qué, quién y en qué documento.").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/partidas/{id:guid}/descalificar", async (Guid id, DatosDescalificacion d, HttpContext http, CertificacionesAgro m, CancellationToken ct) =>
                (await m.DescalificarAsync(id, d, http.User.ObtenerUsuarioId(), ct).ConfigureAwait(false)).AOk())
            .WithSummary("Quita a la partida una certificación (p. ej. el ecológico), con motivo; queda registrado.").RequierePermiso(Permisos.AgroGestionar);

        g.MapGet("/categorias", (IContextoEmpresa c, MaestrosAgro m, CancellationToken ct) => ConEmpresa(c, async e => Results.Ok(await m.CategoriasAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Categorías de clasificación.").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/categorias", (DatosCategoria d, IContextoEmpresa c, MaestrosAgro m, CancellationToken ct) => ConEmpresa(c, async e => Creado(await m.CrearCategoriaAsync(e, d, ct).ConfigureAwait(false), "categorias")))
            .WithSummary("Crea una categoría (Extra, 1ª, 2ª, destrío…).").RequierePermiso(Permisos.AgroGestionar);
        g.MapGet("/conceptos", (IContextoEmpresa c, MaestrosAgro m, CancellationToken ct) => ConEmpresa(c, async e => Results.Ok(await m.ConceptosAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Conceptos de descuento de las liquidaciones.").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/conceptos", (DatosConcepto d, IContextoEmpresa c, MaestrosAgro m, CancellationToken ct) => ConEmpresa(c, async e => Creado(await m.CrearConceptoAsync(e, d, ct).ConfigureAwait(false), "conceptos")))
            .WithSummary("Crea un descuento: por kilo, % del bruto o fijo.").RequierePermiso(Permisos.AgroLiquidar);
        g.MapPut("/conceptos/{id:guid}/activo", async (Guid id, PeticionActivo p, MaestrosAgro m, CancellationToken ct) => (await m.ActivarConceptoAsync(id, p.Activo, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Activa o desactiva un descuento.").RequierePermiso(Permisos.AgroLiquidar);
        g.MapGet("/tarifas", (IContextoEmpresa c, MaestrosAgro m, CancellationToken ct) => ConEmpresa(c, async e => Results.Ok(await m.TarifasAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Tarifas de coste de mano de obra y maquinaria.").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/tarifas", (DatosTarifa d, IContextoEmpresa c, MaestrosAgro m, CancellationToken ct) => ConEmpresa(c, async e => Creado(await m.CrearTarifaAsync(e, d, ct).ConfigureAwait(false), "tarifas")))
            .WithSummary("Crea una tarifa (€/hora o €/pieza a destajo) con vigencia, sin solapes.").RequierePermiso(Permisos.AgroGestionar);
        g.MapGet("/configuracion", (IContextoEmpresa c, MaestrosAgro m, CancellationToken ct) => ConEmpresa(c, async e => Results.Ok(await m.ConfiguracionAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Ajustes del módulo (prefijo GS1 de los SSCC).").RequierePermiso(Permisos.AgroLeer);
        g.MapPut("/configuracion", (ConfiguracionAgroDto d, IContextoEmpresa c, MaestrosAgro m, CancellationToken ct) => ConEmpresa(c, async e => (await m.ActualizarConfiguracionAsync(e, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Cambia el prefijo de empresa GS1 y el dígito de extensión de los SSCC.").RequierePermiso(Permisos.EmpresaAjustes);
    }

    private static void MapearRecepciones(RouteGroupBuilder g)
    {
        g.MapGet("/recepciones", (DateOnly? desde, DateOnly? hasta, Guid? agricultorId, IContextoEmpresa c, RecepcionesAgro r, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await r.ListarAsync(e, desde, hasta, agricultorId, ct).ConfigureAwait(false))))
            .WithSummary("Recepciones de fruta.").RequierePermiso(Permisos.AgroLeer);
        g.MapGet("/recepciones/{id:guid}", async (Guid id, RecepcionesAgro r, CancellationToken ct) => Encontrado(await r.ObtenerAsync(id, ct).ConfigureAwait(false), "recepcion"))
            .WithSummary("Recepción con sus líneas y pesadas.").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/recepciones", (DatosRecepcion d, IContextoEmpresa c, RecepcionesAgro r, CancellationToken ct) => ConEmpresa(c, async e => Creado(await r.CrearAsync(e, d, ct).ConfigureAwait(false), "recepciones")))
            .WithSummary("Abre una recepción en borrador (agricultor, fecha, campaña, matrícula).").RequierePermiso(Permisos.AgroGestionar);
        g.MapDelete("/recepciones/{id:guid}", async (Guid id, RecepcionesAgro r, CancellationToken ct) => (await r.EliminarAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Elimina un borrador.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/recepciones/{id:guid}/lineas", async (Guid id, DatosLinea d, RecepcionesAgro r, CancellationToken ct) => (await r.AgregarLineaAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Añade una línea (producto en kg, parcela, recolección, envase, precio estimado).").RequierePermiso(Permisos.AgroGestionar);
        g.MapDelete("/recepciones/{id:guid}/lineas/{lineaId:guid}", async (Guid id, Guid lineaId, RecepcionesAgro r, CancellationToken ct) => (await r.QuitarLineaAsync(id, lineaId, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Quita una línea del borrador.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/recepciones/{id:guid}/lineas/{lineaId:guid}/pesadas", async (Guid id, Guid lineaId, DatosPesada d, RecepcionesAgro r, CancellationToken ct) =>
                (await r.AgregarPesadaAsync(id, lineaId, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Añade una pesada de báscula (bruto, tara, envases).").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/recepciones/{id:guid}/lineas/{lineaId:guid}/rectificar", async (Guid id, Guid lineaId, DatosRectificacion d, HttpContext http, RecepcionesAgro r,
                CancellationToken ct) => (await r.RectificarAsync(id, lineaId, d, http.User.ObtenerUsuarioId(), ct).ConfigureAwait(false)).AOk())
            .WithSummary("Rectifica una línea confirmada (neto real y/o kilos de liquidación) sobre la misma partida, con motivo; no se borra ni se rehace nada.")
            .RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/recepciones/{id:guid}/lineas/{lineaId:guid}/pales", async (Guid id, Guid lineaId, DatosPaleEntrada d, RecepcionesAgro r, CancellationToken ct) =>
                (await r.AgregarPaleEntradaAsync(id, lineaId, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Registra un palé o palot que llega en la línea (serie de su etiqueta, envases reales, kilos si se pesó solo).").RequierePermiso(Permisos.AgroGestionar);
        g.MapDelete("/recepciones/{id:guid}/pales/{paleEntradaId:guid}", async (Guid id, Guid paleEntradaId, RecepcionesAgro r, CancellationToken ct) =>
                (await r.QuitarPaleEntradaAsync(id, paleEntradaId, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Quita un palé de entrada (en borrador).").RequierePermiso(Permisos.AgroGestionar);
        g.MapPut("/recepciones/{id:guid}/lineas/{lineaId:guid}/kilos-liquidacion", async (Guid id, Guid lineaId, DatosKilosLiquidacion d, RecepcionesAgro r, CancellationToken ct) =>
                (await r.FijarKilosLiquidacionAsync(id, lineaId, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Kilos por los que se liquida la línea si no son los netos (con motivo); null los quita. Nunca cambian el neto.")
            .RequierePermiso(Permisos.AgroGestionar);
        g.MapDelete("/recepciones/{id:guid}/pesadas/{pesadaId:guid}", async (Guid id, Guid pesadaId, RecepcionesAgro r, CancellationToken ct) =>
                (await r.QuitarPesadaAsync(id, pesadaId, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Quita una pesada del borrador.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/recepciones/{id:guid}/confirmar", (Guid id, IContextoEmpresa c, RecepcionesAgro r, CancellationToken ct) =>
                ConEmpresa(c, async e => (await r.ConfirmarAsync(e, id, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Confirma: número sin huecos, una partida por línea y devolución de envases.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/recepciones/{id:guid}/anular", async (Guid id, PeticionMotivo p, RecepcionesAgro r, CancellationToken ct) => (await r.AnularAsync(id, p.Motivo, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Anula una recepción cuyas partidas no se han usado ni liquidado.").RequierePermiso(Permisos.AgroGestionar);

        g.MapGet("/partidas", (IContextoEmpresa c, RecepcionesAgro r, CancellationToken ct) => ConEmpresa(c, async e => Results.Ok(await r.ExistenciasAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Partidas con kilos disponibles.").RequierePermiso(Permisos.AgroLeer);
        g.MapGet("/partidas/{id:guid}", async (Guid id, RecepcionesAgro r, CancellationToken ct) =>
            {
                var p = await r.PartidaAsync(id, ct).ConfigureAwait(false);
                return p is { } x ? Results.Ok(new { x.Partida, x.Saldos, x.Movimientos }) : NoEncontrado("partida");
            })
            .WithSummary("Partida con su saldo por palé y su libro de movimientos.").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/partidas/{id:guid}/ajustes", async (Guid id, DatosAjustePartida d, RecepcionesAgro r, CancellationToken ct) => (await r.AjustarPartidaAsync(id, d, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Merma, destrío retirado o regularización (kilos con signo).").RequierePermiso(Permisos.AgroGestionar);
        g.MapGet("/partidas/{id:guid}/clasificaciones", (Guid id, IContextoEmpresa c, LiquidacionesAgro l, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await l.ClasificacionesAsync(e, id, ct).ConfigureAwait(false))))
            .WithSummary("Clasificaciones (muestreos) de la partida.").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/partidas/{id:guid}/clasificaciones", (Guid id, DatosClasificacion d, IContextoEmpresa c, LiquidacionesAgro l, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await l.ClasificarAsync(e, id, d, ct).ConfigureAwait(false), "clasificaciones")))
            .WithSummary("Registra un muestreo por categorías (provisional o definitivo).").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/clasificaciones/{id:guid}/definitiva", (Guid id, IContextoEmpresa c, LiquidacionesAgro l, CancellationToken ct) =>
                ConEmpresa(c, async e => (await l.DefinitivaAsync(e, id, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Hace definitiva una clasificación (sustituye a la anterior si la partida no está liquidada).").RequierePermiso(Permisos.AgroGestionar);
    }

    private static void MapearLiquidaciones(RouteGroupBuilder g)
    {
        g.MapGet("/liquidaciones", (Guid? agricultorId, IContextoEmpresa c, LiquidacionesAgro l, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await l.ListarAsync(e, agricultorId, ct).ConfigureAwait(false))))
            .WithSummary("Liquidaciones al agricultor.").RequierePermiso(Permisos.AgroLeer);
        g.MapGet("/liquidaciones/{id:guid}", async (Guid id, LiquidacionesAgro l, CancellationToken ct) => Encontrado(await l.ObtenerAsync(id, ct).ConfigureAwait(false), "liquidacion"))
            .WithSummary("Liquidación con el detalle de kilos, categorías, precios y descuentos.").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/liquidaciones/previsualizar", (DatosLiquidacion d, IContextoEmpresa c, LiquidacionesAgro l, CancellationToken ct) =>
                ConEmpresa(c, async e => (await l.PrevisualizarAsync(e, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Valora sin guardar: la liquidación que saldría, o todo lo que falta (precios, clasificaciones…).").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/liquidaciones", (DatosLiquidacion d, IContextoEmpresa c, LiquidacionesAgro l, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await l.CrearAsync(e, d, ct).ConfigureAwait(false), "liquidaciones")))
            .WithSummary("Crea la liquidación en borrador con las entregas pendientes del periodo.").RequierePermiso(Permisos.AgroLiquidar);
        g.MapPost("/liquidaciones/{id:guid}/recalcular", (Guid id, IContextoEmpresa c, LiquidacionesAgro l, CancellationToken ct) =>
                ConEmpresa(c, async e => (await l.RecalcularAsync(e, id, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Vuelve a valorar un borrador con los precios y clasificaciones actuales.").RequierePermiso(Permisos.AgroLiquidar);
        g.MapDelete("/liquidaciones/{id:guid}", async (Guid id, LiquidacionesAgro l, CancellationToken ct) => (await l.EliminarAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Elimina un borrador (sus entregas quedan libres).").RequierePermiso(Permisos.AgroLiquidar);
        g.MapPost("/liquidaciones/{id:guid}/emitir", (Guid id, IContextoEmpresa c, LiquidacionesAgro l, CancellationToken ct) =>
                ConEmpresa(c, async e => (await l.EmitirAsync(e, id, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Emite la liquidación: número sin huecos y autofactura (gasto con compensación REAGP o IVA y retención).").RequierePermiso(Permisos.AgroLiquidar);
        g.MapPost("/liquidaciones/{id:guid}/anular", async (Guid id, PeticionMotivo p, LiquidacionesAgro l, CancellationToken ct) => (await l.AnularAsync(id, p.Motivo, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Anula una liquidación emitida y su autofactura (si no está pagada).").RequierePermiso(Permisos.AgroLiquidar);

        g.MapGet("/informes/campana/{id:guid}", (Guid id, IContextoEmpresa c, InformesAgro i, CancellationToken ct) =>
                ConEmpresa(c, async e => (await i.CampanaAsync(e, id, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Informe de campaña: entregas y liquidaciones por agricultor, coste por kilo por parcela y de la confección.").RequierePermiso(Permisos.AgroLeer);
    }

    private static void MapearConfeccion(RouteGroupBuilder g)
    {
        g.MapGet("/partes", (DateOnly? desde, DateOnly? hasta, IContextoEmpresa c, ConfeccionAgro p, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await p.ListarAsync(e, desde, hasta, ct).ConfigureAwait(false))))
            .WithSummary("Partes de confección.").RequierePermiso(Permisos.AgroLeer);
        g.MapGet("/partes/{id:guid}", async (Guid id, ConfeccionAgro p, CancellationToken ct) => Encontrado(await p.ObtenerAsync(id, ct).ConfigureAwait(false), "parte"))
            .WithSummary("Parte con consumos, mano de obra, maquinaria, materiales y salidas.").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/partes", (DatosParteConfeccion d, IContextoEmpresa c, ConfeccionAgro p, CancellationToken ct) => ConEmpresa(c, async e => Creado(await p.CrearAsync(e, d, ct).ConfigureAwait(false), "partes")))
            .WithSummary("Crea un parte en borrador.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPut("/partes/{id:guid}", async (Guid id, DatosParteConfeccion d, ConfeccionAgro p, CancellationToken ct) => (await p.ActualizarAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Sustituye el contenido del borrador.").RequierePermiso(Permisos.AgroGestionar);
        g.MapDelete("/partes/{id:guid}", async (Guid id, ConfeccionAgro p, CancellationToken ct) => (await p.EliminarAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Elimina un borrador.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/partes/{id:guid}/valorar", async (Guid id, ConfeccionAgro p, CancellationToken ct) => (await p.ValorarAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Valora sin validar: costes y reparto entre salidas, o todos los errores.").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/partes/{id:guid}/aprobar-merma", async (Guid id, EndpointsLogistica.PeticionMotivo m, HttpContext http, ConfeccionAgro p, CancellationToken ct) =>
            {
                var yo = http.User.ObtenerIdentidad();
                return (await p.AprobarMermaAsync(id, m?.Motivo, yo?.Id, string.IsNullOrWhiteSpace(yo?.Nombre) ? yo?.Email : yo.Nombre, ct).ConfigureAwait(false)).AOk();
            })
            .WithSummary("Aprueba (con motivo, a nombre del usuario) una merma por encima de la tolerancia en un parte en borrador.").RequierePermiso(Permisos.AgroGestionar);
        g.MapGet("/transformaciones", (IContextoEmpresa c, TransformacionesAgro t, CancellationToken ct) => ConEmpresa(c, async e => Results.Ok(await t.ReglasAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Qué producto puede salir de cuál en la confección, con su merma máxima.").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/transformaciones", (DatosReglaTransformacion d, IContextoEmpresa c, TransformacionesAgro t, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await t.CrearReglaAsync(e, d, ct).ConfigureAwait(false), "transformaciones")))
            .WithSummary("Permite una transformación (producto de origen → producto de destino) con su merma máxima.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPut("/transformaciones/{id:guid}", async (Guid id, DatosReglaTransformacion d, TransformacionesAgro t, CancellationToken ct) =>
                (await t.CambiarReglaAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Cambia la merma máxima de una transformación.").RequierePermiso(Permisos.AgroGestionar);
        g.MapDelete("/transformaciones/{id:guid}", async (Guid id, TransformacionesAgro t, CancellationToken ct) => (await t.EliminarReglaAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Quita una transformación permitida.").RequierePermiso(Permisos.AgroGestionar);
        g.MapGet("/repaletizados", (Guid? paleId, IContextoEmpresa c, TransformacionesAgro t, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await t.RepaletizadosAsync(e, paleId, ct).ConfigureAwait(false))))
            .WithSummary("Repaletizados (de todos o de un palé): qué pasó de qué palé a cuál.").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/repaletizados", (DatosRepaletizado d, HttpContext http, IContextoEmpresa c, TransformacionesAgro t, CancellationToken ct) =>
                ConEmpresa(c, async e => (await t.RepaletizarAsync(e, d, http.User.ObtenerUsuarioId(), ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Pasa kilos (o palés enteros) de unos palés a otro, sin cambiar de partida, en una operación registrada.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/partes/{id:guid}/validar", (Guid id, IContextoEmpresa c, ConfeccionAgro p, CancellationToken ct) => ConEmpresa(c, async e => (await p.ValidarAsync(e, id, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Valida: consume las partidas, crea las de salida con su coste y registra la genealogía.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/partes/{id:guid}/anular", async (Guid id, ConfeccionAgro p, CancellationToken ct) => (await p.AnularAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Anula un parte validado cuyas salidas no se han movido.").RequierePermiso(Permisos.AgroGestionar);

        g.MapGet("/pales", (EstadoPale? estado, IContextoEmpresa c, PalesAgro p, CancellationToken ct) => ConEmpresa(c, async e => Results.Ok(await p.ListarAsync(e, estado, ct).ConfigureAwait(false))))
            .WithSummary("Palés con su contenido.").RequierePermiso(Permisos.AgroLeer);
        g.MapGet("/pales/{idOSscc}", (string idOSscc, IContextoEmpresa c, PalesAgro p, CancellationToken ct) =>
                ConEmpresa(c, async e => Encontrado(await p.ObtenerAsync(e, idOSscc, ct).ConfigureAwait(false), "pale")))
            .WithSummary("Palé por id o por SSCC.").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/pales", (DatosPale d, IContextoEmpresa c, PalesAgro p, CancellationToken ct) => ConEmpresa(c, async e => Creado(await p.CrearAsync(e, d, ct).ConfigureAwait(false), "pales")))
            .WithSummary("Da de alta un palé con el siguiente SSCC GS1.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/pales/montar", (DatosMontaje d, IContextoEmpresa c, PalesAgro p, CancellationToken ct) => ConEmpresa(c, async e => (await p.MontarAsync(e, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Montaje rápido: con una plantilla y una partida, monta de una vez los palés (completos y cerrados; el último, abierto si no se llena).")
            .RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/pales/{id:guid}/lecturas", (Guid id, LecturaCaja d, IContextoEmpresa c, PalesAgro p, CancellationToken ct) =>
                ConEmpresa(c, async e => (await p.LeerCajaAsync(e, id, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Escáner del punto de paletizado: añade al palé la caja leída (lote GS1 AI 10 = código de la partida).").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/pales/{id:guid}/cajas", async (Guid id, DatosCajas d, PalesAgro p, CancellationToken ct) => (await p.CajasAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Pone cajas de una partida en un palé con plantilla (o las saca, en negativo); se cierra solo al completarse.").RequierePermiso(Permisos.AgroGestionar);
        g.MapGet("/pales/{idOSscc}/etiqueta", (string idOSscc, IContextoEmpresa c, PalesAgro p, IConsultaEmpresas empresas, IGeneradorEtiquetaLogistica generador, CancellationToken ct) =>
                ConEmpresa(c, async e =>
                {
                    var et = await p.EtiquetaAsync(e, idOSscc, ct).ConfigureAwait(false);
                    if (et.EsFallo)
                    {
                        return ResultadosHttp.AProblema(et.Error);
                    }

                    var empresa = await empresas.ObtenerAsync(e, ct).ConfigureAwait(false);
                    if (empresa is null)
                    {
                        return ResultadosHttp.AProblema(Error.NoEncontrado("empresa.no_encontrada", "La empresa no existe."));
                    }

                    var v = et.Valor;
                    var pdf = generador.Generar(new EtiquetaLogistica(v.Sscc, v.Producto, v.Marca, v.TipoPale, v.Cajas, v.Kilos, v.Lote, v.Fecha, v.Destinatario), empresa);
                    return Results.File(pdf, "application/pdf", $"etiqueta-{v.Sscc}.pdf");
                }))
            .WithSummary("Etiqueta logística GS1 del palé (PDF A6 con el SSCC y el contenido en GS1-128).").RequierePermiso(Permisos.AgroLeer);

        g.MapGet("/plantillas-pale", (IContextoEmpresa c, PalesAgro p, CancellationToken ct) => ConEmpresa(c, async e => Results.Ok(await p.PlantillasAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Plantillas de palé (tipo, producto, marca, cajas por palé, kilos por caja y mosaico).").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/plantillas-pale", (DatosPlantilla d, IContextoEmpresa c, PalesAgro p, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await p.CrearPlantillaAsync(e, d, ct).ConfigureAwait(false), "plantillas-pale")))
            .WithSummary("Crea una plantilla de palé.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPut("/plantillas-pale/{id:guid}", async (Guid id, DatosPlantilla d, PalesAgro p, CancellationToken ct) => (await p.ActualizarPlantillaAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Modifica una plantilla (si ya se usó, no cambian sus cajas, kilos ni producto) o la desactiva.").RequierePermiso(Permisos.AgroGestionar);
        g.MapDelete("/plantillas-pale/{id:guid}", async (Guid id, PalesAgro p, CancellationToken ct) => (await p.EliminarPlantillaAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Elimina una plantilla con la que no se ha montado ningún palé.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/pales/{id:guid}/paletizar", async (Guid id, DatosMoverKilos d, PalesAgro p, CancellationToken ct) => (await p.PaletizarAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Pone kilos de una partida en el palé (sueltos o desde otro palé).").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/pales/{id:guid}/despaletizar", async (Guid id, DatosMoverKilos d, PalesAgro p, CancellationToken ct) => (await p.DespaletizarAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Saca kilos de una partida del palé.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/pales/{id:guid}/cerrar", async (Guid id, PalesAgro p, CancellationToken ct) => (await p.CerrarAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Cierra el palé (listo para expedir).").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/pales/{id:guid}/reabrir", async (Guid id, PalesAgro p, CancellationToken ct) => (await p.ReabrirAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Reabre un palé cerrado que no ha salido.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/pales/{id:guid}/anular-expedicion", (Guid id, IContextoEmpresa c, PalesAgro p, CancellationToken ct) => ConEmpresa(c, async e => (await p.AnularExpedicionAsync(e, id, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Anula la expedición de un palé (salió por error o volvió): queda cerrado con su contenido.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/expediciones", (DatosExpedicion d, IContextoEmpresa c, PalesAgro p, CancellationToken ct) => ConEmpresa(c, async e => (await p.ExpedirAsync(e, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Expide palés cerrados a un cliente (con CartaPorte = true emite además la carta de porte).").RequierePermiso(Permisos.AgroGestionar);

        // Órdenes de carga ligeras sobre la expedición.
        g.MapGet("/ordenes-carga", (EstadoOrdenCarga? estado, IContextoEmpresa c, OrdenesCargaAgro o, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await o.ListarAsync(e, estado, ct).ConfigureAwait(false))))
            .WithSummary("Órdenes de carga (filtro por estado).").RequierePermiso(Permisos.AgroLeer);
        g.MapGet("/ordenes-carga/pendientes", (Guid? clienteId, IContextoEmpresa c, OrdenesCargaAgro o, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await o.PendientesAsync(e, clienteId, ct).ConfigureAwait(false))))
            .WithSummary("Líneas de pedidos en firme pendientes de servir: lo reservado y lo previsto en otras órdenes.").RequierePermiso(Permisos.AgroLeer);
        g.MapGet("/ordenes-carga/{id:guid}", async (Guid id, OrdenesCargaAgro o, CancellationToken ct) => Encontrado(await o.ObtenerAsync(id, ct).ConfigureAwait(false), "ordencarga"))
            .WithSummary("Orden de carga con sus líneas y palés cargados.").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("/ordenes-carga", (DatosOrdenCarga d, IContextoEmpresa c, OrdenesCargaAgro o, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await o.CrearAsync(e, d, ct).ConfigureAwait(false), "ordenes-carga")))
            .WithSummary("Crea una orden de carga (en propuesta o pendiente): muelle, transportista, vehículo, conductor, temperatura y camión.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPut("/ordenes-carga/{id:guid}", async (Guid id, DatosOrdenCarga d, OrdenesCargaAgro o, CancellationToken ct) => (await o.CambiarAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Cambia los datos de una orden abierta.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/ordenes-carga/{id:guid}/liberar", async (Guid id, OrdenesCargaAgro o, CancellationToken ct) => (await o.LiberarAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("La propuesta pasa a pendiente de cargar.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/ordenes-carga/{id:guid}/lineas", async (Guid id, LineaOrdenCargaComando d, OrdenesCargaAgro o, CancellationToken ct) =>
                (await o.AgregarLineaAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Añade una línea de pedido con los palés previstos y su posición.").RequierePermiso(Permisos.AgroGestionar);
        g.MapDelete("/ordenes-carga/{id:guid}/lineas/{lineaId:guid}", async (Guid id, Guid lineaId, OrdenesCargaAgro o, CancellationToken ct) =>
                (await o.QuitarLineaAsync(id, lineaId, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Quita una línea sin palés cargados.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/ordenes-carga/{id:guid}/cargar", (Guid id, CargarPaleComando d, IContextoEmpresa c, OrdenesCargaAgro o, CancellationToken ct) =>
                ConEmpresa(c, async e => (await o.CargarAsync(e, id, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Carga un palé (por su SSCC), validado contra la línea y su reserva.").RequierePermiso(Permisos.AgroGestionar);
        g.MapDelete("/ordenes-carga/{id:guid}/pales/{paleId:guid}", async (Guid id, Guid paleId, OrdenesCargaAgro o, CancellationToken ct) =>
                (await o.DescargarAsync(id, paleId, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Descarga un palé aún no expedido.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/ordenes-carga/{id:guid}/finalizar", (Guid id, IContextoEmpresa c, OrdenesCargaAgro o, CancellationToken ct) =>
                ConEmpresa(c, async e => (await o.FinalizarAsync(e, id, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Expide lo cargado: un albarán por pedido (y carta de porte si se pidió) y los envases.").RequierePermiso(Permisos.AgroGestionar);
        g.MapPost("/ordenes-carga/{id:guid}/anular", async (Guid id, AnularPeticionEnvases? p, OrdenesCargaAgro o, CancellationToken ct) =>
                (await o.AnularAsync(id, p?.Motivo, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Anula una orden sin nada expedido.").RequierePermiso(Permisos.AgroGestionar);

        g.MapGet("/trazabilidad/atras", (Guid? partidaId, string? sscc, IContextoEmpresa c, TrazabilidadAgro t, CancellationToken ct) =>
                ConEmpresa(c, async e => (await t.HaciaAtrasAsync(e, partidaId, sscc, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("De un palé (SSCC) o una partida hasta el agricultor, la parcela y la recepción.").RequierePermiso(Permisos.AgroLeer);
        g.MapGet("/trazabilidad/adelante", (Guid? partidaId, Guid? recepcionId, IContextoEmpresa c, TrazabilidadAgro t, CancellationToken ct) =>
                ConEmpresa(c, async e => (await t.HaciaAdelanteAsync(e, partidaId, recepcionId, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("De una recepción o partida hasta los palés y los clientes a los que salió.").RequierePermiso(Permisos.AgroLeer);
    }

    private static async Task<IResult> ConEmpresa(IContextoEmpresa contexto, Func<Guid, Task<IResult>> accion) =>
        contexto.EmpresaId is { } empresa
            ? await accion(empresa).ConfigureAwait(false)
            : ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));

    private static IResult Creado<T>(Resultado<T> r, string ruta) =>
        r.EsCorrecto ? Results.Created($"/agro/{ruta}", r.Valor) : ResultadosHttp.AProblema(r.Error);

    private static IResult Encontrado<T>(T? valor, string que) where T : class =>
        valor is null ? NoEncontrado(que) : Results.Ok(valor);

    private static IResult NoEncontrado(string que) =>
        ResultadosHttp.AProblema(Error.NoEncontrado($"{que}.no_encontrado", "No existe."));
}

/// <summary>Motivo de la anulación de un movimiento de envases.</summary>
public sealed record AnularPeticionEnvases(string? Motivo);
