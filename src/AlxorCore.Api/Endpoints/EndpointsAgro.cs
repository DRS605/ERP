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
        g.MapPost("/agricultores/{id:guid}/parcelas", (Guid id, DatosParcela d, IContextoEmpresa c, MaestrosAgro m, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await m.CrearParcelaAsync(e, id, d, ct).ConfigureAwait(false), "parcelas")))
            .WithSummary("Añade una parcela al agricultor (SIGPAC, superficie, cultivo, centro analítico).").RequierePermiso(Permisos.AgroGestionar);
        g.MapGet("/parcelas", (Guid? agricultorId, IContextoEmpresa c, MaestrosAgro m, CancellationToken ct) => ConEmpresa(c, async e => Results.Ok(await m.ParcelasAsync(e, agricultorId, ct).ConfigureAwait(false))))
            .WithSummary("Parcelas (de todos o de un agricultor).").RequierePermiso(Permisos.AgroLeer);
        g.MapPut("/parcelas/{id:guid}", async (Guid id, DatosParcela d, MaestrosAgro m, CancellationToken ct) => (await m.ActualizarParcelaAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Modifica una parcela.").RequierePermiso(Permisos.AgroGestionar);

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
