using AlxorCore.Api.Comun;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Api.Endpoints;

/// <summary>EDI EANCOM con la gran distribución: GLN, socios, ORDERS y RECADV entrantes, DESADV e INVOIC salientes.</summary>
public static class EndpointsEdi
{
    public sealed record PeticionGlnEmpresa(string? GlnEmpresa);

    public sealed record PeticionMensajeEdi(string Contenido);

    public static IEndpointRouteBuilder MapearEdi(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var g = rutas.MapGroup("/integraciones/edi").WithTags("Integraciones");

        g.MapGet("/configuracion", async (IntercambioEdi edi, CancellationToken ct) => Results.Ok(await edi.ConfiguracionAsync(ct).ConfigureAwait(false)))
            .WithSummary("GLN de la empresa como emisora de mensajes EDI.").RequierePermiso(Permisos.IntegracionGestionar);
        g.MapPut("/configuracion", (PeticionGlnEmpresa p, IContextoEmpresa c, IntercambioEdi edi, CancellationToken ct) =>
                ConEmpresa(c, async e => (await edi.GuardarConfiguracionAsync(e, p.GlnEmpresa, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Guarda el GLN de la empresa.").RequierePermiso(Permisos.IntegracionGestionar);

        g.MapGet("/socios", async (IntercambioEdi edi, CancellationToken ct) => Results.Ok(await edi.SociosAsync(ct).ConfigureAwait(false)))
            .WithSummary("Clientes con intercambio EDI y sus GLN.").RequierePermiso(Permisos.IntegracionGestionar);
        g.MapPost("/socios", (GuardarSocioEdi p, IContextoEmpresa c, IntercambioEdi edi, CancellationToken ct) =>
                ConEmpresa(c, async e =>
                {
                    var r = await edi.GuardarSocioAsync(e, null, p, ct).ConfigureAwait(false);
                    return r.EsCorrecto ? Results.Created($"/integraciones/edi/socios/{r.Valor.Id}", r.Valor) : ResultadosHttp.AProblema(r.Error);
                }))
            .WithSummary("Da de alta un cliente como socio EDI (GLN del comprador, de facturación y de entrega).").RequierePermiso(Permisos.IntegracionGestionar);
        g.MapPut("/socios/{id:guid}", (Guid id, GuardarSocioEdi p, IContextoEmpresa c, IntercambioEdi edi, CancellationToken ct) =>
                ConEmpresa(c, async e => await edi.SocioExisteAsync(id, ct).ConfigureAwait(false)
                    ? (await edi.GuardarSocioAsync(e, id, p, ct).ConfigureAwait(false)).AOk()
                    : ResultadosHttp.AProblema(Error.NoEncontrado("edi.socio_no_encontrado", "El socio EDI no existe."))))
            .WithSummary("Cambia los GLN de un socio EDI.").RequierePermiso(Permisos.IntegracionGestionar);
        g.MapDelete("/socios/{id:guid}", async (Guid id, IntercambioEdi edi, CancellationToken ct) =>
                (await edi.EliminarSocioAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Quita un socio EDI.").RequierePermiso(Permisos.IntegracionGestionar);

        g.MapGet("/pedidos", async (IntercambioEdi edi, CancellationToken ct) => Results.Ok(await edi.PedidosAsync(ct).ConfigureAwait(false)))
            .WithSummary("Pedidos recibidos por EDI (ORDERS) y el pedido de venta creado.").RequierePermiso(Permisos.IntegracionGestionar);
        g.MapPost("/orders", (PeticionMensajeEdi p, IContextoEmpresa c, IntercambioEdi edi, CancellationToken ct) =>
                ConEmpresa(c, async e =>
                {
                    var r = await edi.ImportarOrdersAsync(e, c.GrupoRequerido, p.Contenido, ct).ConfigureAwait(false);
                    return r.EsCorrecto ? Results.Created($"/pedidos-venta/{r.Valor.PedidoVentaId}", r.Valor) : ResultadosHttp.AProblema(r.Error);
                }))
            .WithSummary("Importa un ORDERS EANCOM como pedido de venta (cliente por GLN, artículos por GTIN).").RequierePermiso(Permisos.IntegracionGestionar);
        g.MapPost("/recadv", (PeticionMensajeEdi p, IContextoEmpresa c, IntercambioEdi edi, CancellationToken ct) =>
                ConEmpresa(c, async e => (await edi.ContrastarRecadvAsync(e, p.Contenido, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Contrasta un RECADV (aviso de recepción) con lo expedido en el albarán.").RequierePermiso(Permisos.IntegracionGestionar);

        g.MapGet("/facturas/{id:guid}/invoic", async (Guid id, IntercambioEdi edi, CancellationToken ct) =>
                Archivo(await edi.InvoicAsync(id, ct).ConfigureAwait(false)))
            .WithSummary("Factura en EDIFACT INVOIC (EANCOM D96A).").RequierePermiso(Permisos.FacturaLeer);
        g.MapGet("/albaranes/{id:guid}/desadv", (Guid id, IContextoEmpresa c, IntercambioEdi edi, CancellationToken ct) =>
                ConEmpresa(c, async e => Archivo(await edi.DesadvAsync(e, id, ct).ConfigureAwait(false))))
            .WithSummary("Albarán en EDIFACT DESADV con los SSCC de sus palés (EANCOM D96A).").RequierePermiso(Permisos.FacturaLeer);
        return rutas;
    }

    private static IResult Archivo(Resultado<MensajeEdi> r) =>
        r.EsCorrecto ? Results.File(System.Text.Encoding.UTF8.GetBytes(r.Valor.Contenido), "application/edifact", r.Valor.NombreArchivo) : ResultadosHttp.AProblema(r.Error);

    private static async Task<IResult> ConEmpresa(IContextoEmpresa contexto, Func<Guid, Task<IResult>> accion) =>
        contexto.EmpresaId is { } e
            ? await accion(e).ConfigureAwait(false)
            : ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
}
