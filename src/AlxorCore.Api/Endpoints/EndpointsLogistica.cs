using AlxorCore.Api.Comun;
using AlxorCore.Documentos.Aplicacion;
using AlxorCore.Logistica.Aplicacion;
using AlxorCore.Logistica.Dominio;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Api.Endpoints;

/// <summary>
/// Logística genérica: configuración GS1, soportes, fichas logísticas, plantillas de paletizado, cálculo de palés,
/// unidades logísticas SSCC (montaje desde el almacén o desde fabricación, ciclo, etiqueta) y lectura del escáner.
/// </summary>
public static class EndpointsLogistica
{
    public sealed record PeticionMotivo(string? Motivo);

    public static IEndpointRouteBuilder MapearLogistica(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var g = rutas.MapGroup("/logistica").WithTags("Logística");
        const string leer = Permisos.InventarioLeer;
        const string gestionar = Permisos.InventarioGestionar;

        g.MapGet("/configuracion", (IContextoEmpresa c, MaestrosLogistica m, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await m.ConfiguracionAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Prefijo GS1 y dígito de extensión de los SSCC, y si un palé puede mezclar lotes.").RequierePermiso(leer);
        g.MapPut("/configuracion", (DatosConfiguracionLogistica d, IContextoEmpresa c, MaestrosLogistica m, CancellationToken ct) =>
                ConEmpresa(c, async e => (await m.FijarConfiguracionAsync(e, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Fija el prefijo GS1 (7 a 10 dígitos), la extensión del SSCC y la mezcla de lotes.").RequierePermiso(Permisos.EmpresaAjustes);

        g.MapGet("/soportes", (IContextoEmpresa c, MaestrosLogistica m, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await m.SoportesAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Soportes: europalé, palé americano, medio palé, contenedor… con medidas, tara y carga máxima.").RequierePermiso(leer);
        g.MapPost("/soportes", (DatosSoporte d, IContextoEmpresa c, MaestrosLogistica m, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await m.CrearSoporteAsync(e, d, ct).ConfigureAwait(false), "soportes")))
            .WithSummary("Da de alta un soporte.").RequierePermiso(gestionar);
        g.MapPut("/soportes/{id:guid}", async (Guid id, DatosSoporte d, MaestrosLogistica m, CancellationToken ct) =>
                (await m.ActualizarSoporteAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Cambia un soporte.").RequierePermiso(gestionar);
        g.MapDelete("/soportes/{id:guid}", async (Guid id, MaestrosLogistica m, CancellationToken ct) =>
            {
                var r = await m.EliminarSoporteAsync(id, ct).ConfigureAwait(false);
                return r.EsCorrecto ? Results.Ok(new { eliminado = r.Valor }) : ResultadosHttp.AProblema(r.Error);
            })
            .WithSummary("Elimina un soporte sin usar; si se ha usado, lo da de baja.").RequierePermiso(gestionar);

        g.MapGet("/fichas", (IContextoEmpresa c, MaestrosLogistica m, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await m.FichasAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Fichas logísticas de los artículos.").RequierePermiso(leer);
        g.MapGet("/fichas/{productoId:guid}", (Guid productoId, IContextoEmpresa c, MaestrosLogistica m, CancellationToken ct) =>
                ConEmpresa(c, async e => await m.FichaAsync(e, productoId, ct).ConfigureAwait(false) is { } f ? Results.Ok(f)
                    : ResultadosHttp.AProblema(Error.NoEncontrado("ficha_logistica.no_encontrada", "El artículo no tiene ficha logística."))))
            .WithSummary("Ficha logística de un artículo.").RequierePermiso(leer);
        g.MapPut("/fichas/{productoId:guid}", (Guid productoId, DatosFichaLogistica d, IContextoEmpresa c, MaestrosLogistica m, CancellationToken ct) =>
                ConEmpresa(c, async e => (await m.FijarFichaAsync(e, productoId, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Da de alta o cambia la ficha logística: GTIN, unidades por caja, pesos, medidas, mosaico, límites, temperatura y vida útil.")
            .RequierePermiso(gestionar);
        g.MapDelete("/fichas/{productoId:guid}", (Guid productoId, IContextoEmpresa c, MaestrosLogistica m, CancellationToken ct) =>
                ConEmpresa(c, async e => (await m.EliminarFichaAsync(e, productoId, ct).ConfigureAwait(false)).ASinContenido()))
            .WithSummary("Quita la ficha logística del artículo.").RequierePermiso(gestionar);

        g.MapGet("/plantillas", (Guid? productoId, IContextoEmpresa c, MaestrosLogistica m, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await m.PlantillasAsync(e, productoId, ct).ConfigureAwait(false))))
            .WithSummary("Plantillas de paletizado (por artículo y, si se indica, cliente).").RequierePermiso(leer);
        g.MapPost("/plantillas", (DatosPlantillaPaletizado d, IContextoEmpresa c, MaestrosLogistica m, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await m.CrearPlantillaAsync(e, d, ct).ConfigureAwait(false), "plantillas")))
            .WithSummary("Da de alta una plantilla de paletizado: soporte, cajas por capa, capas y límites del cliente.").RequierePermiso(gestionar);
        g.MapPut("/plantillas/{id:guid}", async (Guid id, DatosPlantillaPaletizado d, MaestrosLogistica m, CancellationToken ct) =>
                (await m.ActualizarPlantillaAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Cambia una plantilla de paletizado.").RequierePermiso(gestionar);
        g.MapDelete("/plantillas/{id:guid}", async (Guid id, MaestrosLogistica m, CancellationToken ct) =>
            {
                var r = await m.EliminarPlantillaAsync(id, ct).ConfigureAwait(false);
                return r.EsCorrecto ? Results.Ok(new { eliminado = r.Valor }) : ResultadosHttp.AProblema(r.Error);
            })
            .WithSummary("Elimina una plantilla sin usar; si se ha usado, la desactiva.").RequierePermiso(gestionar);

        g.MapGet("/calculo", (Guid productoId, int? cajas, decimal? unidades, Guid? clienteId, Guid? plantillaId, IContextoEmpresa c, PaletizacionLogistica p,
                CancellationToken ct) =>
                ConEmpresa(c, async e => (await p.CalcularAsync(e, new DatosCalculoPaletizado(productoId, cajas, unidades, clienteId, plantillaId), ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Palés completos, pico, peso y altura de una cantidad del artículo, con la plantilla que corresponda.").RequierePermiso(leer);
        g.MapGet("/pedidos/{id:guid}/pales", (Guid id, IContextoEmpresa c, PaletizacionLogistica p, CancellationToken ct) =>
                ConEmpresa(c, async e => (await p.CalcularPedidoAsync(e, id, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Palés que necesita lo pendiente de servir de un pedido de venta, con las plantillas de su cliente.").RequierePermiso(leer);

        g.MapGet("/unidades", (EstadoUnidadLogistica? estado, Guid? almacenId, Guid? productoId, string? lote, Guid? pedidoVentaId, Guid? ordenFabricacionId, bool? soloRaiz,
                IContextoEmpresa c, PaletizacionLogistica p, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await p.ListarAsync(e, new FiltroUnidades(estado, almacenId, productoId, lote, pedidoVentaId, ordenFabricacionId, soloRaiz ?? false),
                    ct).ConfigureAwait(false))))
            .WithSummary("Unidades logísticas (palés, cajas, contenedores) con filtros.").RequierePermiso(leer);
        g.MapGet("/unidades/{idOSscc}", (string idOSscc, IContextoEmpresa c, PaletizacionLogistica p, CancellationToken ct) =>
                ConEmpresa(c, async e => await p.ObtenerAsync(e, idOSscc, ct).ConfigureAwait(false) is { } u ? Results.Ok(u)
                    : ResultadosHttp.AProblema(Error.NoEncontrado("unidad.no_encontrada", "La unidad logística no existe."))))
            .WithSummary("Una unidad por su id o su SSCC (también la lectura GS1-128 de su etiqueta).").RequierePermiso(leer);
        g.MapPost("/unidades", (DatosUnidad d, IContextoEmpresa c, PaletizacionLogistica p, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await p.CrearAsync(e, d, ct).ConfigureAwait(false), "unidades")))
            .WithSummary("Abre una unidad vacía (con su SSCC) para montarla a mano o con el escáner.").RequierePermiso(gestionar);
        g.MapPost("/unidades/{id:guid}/contenido", async (Guid id, DatosContenido d, PaletizacionLogistica p, CancellationToken ct) =>
                (await p.PonerAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Pone cajas o unidades de un artículo y lote en una unidad abierta (en negativo, las quita); se cierra sola al completarse.")
            .RequierePermiso(gestionar);
        g.MapPost("/unidades/{id:guid}/cerrar", async (Guid id, PaletizacionLogistica p, CancellationToken ct) => (await p.CerrarAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Cierra la unidad.").RequierePermiso(gestionar);
        g.MapPost("/unidades/{id:guid}/abrir", async (Guid id, PaletizacionLogistica p, CancellationToken ct) => (await p.AbrirAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Vuelve a abrir una unidad cerrada.").RequierePermiso(gestionar);
        g.MapPost("/unidades/{id:guid}/anular", async (Guid id, PeticionMotivo? m, PaletizacionLogistica p, CancellationToken ct) =>
                (await p.AnularAsync(id, m?.Motivo, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Desmonta la unidad: su contenido queda libre en el almacén.").RequierePermiso(gestionar);
        g.MapPost("/unidades/{id:guid}/mover", async (Guid id, DatosMover d, PaletizacionLogistica p, CancellationToken ct) =>
                (await p.MoverAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Cambia la ubicación de la unidad (y lo que lleva dentro).").RequierePermiso(gestionar);
        g.MapPost("/unidades/{id:guid}/asignar", async (Guid id, DatosAsignar d, PaletizacionLogistica p, CancellationToken ct) =>
                (await p.AsignarAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Asigna la unidad a un cliente y pedido (o la libera).").RequierePermiso(gestionar);
        g.MapPost("/unidades/{id:guid}/meter", async (Guid id, DatosMeter d, PaletizacionLogistica p, CancellationToken ct) =>
                (await p.MeterAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Mete la unidad (una caja con SSCC) en otra abierta (un palé mixto), o la saca.").RequierePermiso(gestionar);
        g.MapPost("/unidades/{id:guid}/expedir", async (Guid id, DatosExpedir d, PaletizacionLogistica p, CancellationToken ct) =>
                (await p.ExpedirAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Marca la unidad cerrada como expedida.").RequierePermiso(gestionar);
        g.MapGet("/unidades/{idOSscc}/etiqueta", (string idOSscc, IContextoEmpresa c, PaletizacionLogistica p, IConsultaEmpresas empresas, IConsultaClientes clientes,
                IGeneradorEtiquetaLogistica generador, CancellationToken ct) =>
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
                    var destinatario = v.ClienteId is { } cid ? (await clientes.ObtenerAsync(cid, ct).ConfigureAwait(false))?.Nombre : null;
                    var pdf = generador.Generar(new EtiquetaLogistica(v.Sscc, v.Producto, null, v.TipoSoporte, v.Cajas, v.PesoNetoKg, v.Lote, v.Fecha, destinatario, v.Gtin,
                        v.FechaCaducidad, v.PesoBrutoKg), empresa);
                    return Results.File(pdf, "application/pdf", $"etiqueta-{v.Sscc}.pdf");
                }))
            .WithSummary("Etiqueta logística GS1 (PDF A6): SSCC, GTIN, caducidad, cajas, peso y lote en GS1-128.").RequierePermiso(leer);

        g.MapPost("/paletizar", (DatosPaletizar d, IContextoEmpresa c, PaletizacionLogistica p, CancellationToken ct) =>
                ConEmpresa(c, async e => (await p.PaletizarAsync(e, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Monta palés del almacén: las cajas pedidas del lote indicado o por caducidad (FEFO), con la plantilla del cliente.").RequierePermiso(gestionar);
        g.MapPost("/paletizar-fabricacion", (DatosPaletizarFabricacion d, IContextoEmpresa c, PaletizacionLogistica p, CancellationToken ct) =>
                ConEmpresa(c, async e => (await p.PaletizarFabricacionAsync(e, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Monta palés con lo fabricado en una orden terminada (su lote), enlazados a la orden.").RequierePermiso(gestionar);
        g.MapGet("/lectura", (string codigo, IContextoEmpresa c, PaletizacionLogistica p, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await p.LeerAsync(e, codigo, ct).ConfigureAwait(false))))
            .WithSummary("Interpreta lo leído con la pistola o la cámara: un SSCC (la unidad) o un GTIN con lote, caducidad y cantidad (el artículo).")
            .RequierePermiso(leer);

        return rutas;
    }

    private static async Task<IResult> ConEmpresa(IContextoEmpresa contexto, Func<Guid, Task<IResult>> accion) =>
        contexto.EmpresaId is { } empresa
            ? await accion(empresa).ConfigureAwait(false)
            : ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));

    private static IResult Creado<T>(Resultado<T> r, string ruta) =>
        r.EsCorrecto ? Results.Created($"/logistica/{ruta}", r.Valor) : ResultadosHttp.AProblema(r.Error);
}
