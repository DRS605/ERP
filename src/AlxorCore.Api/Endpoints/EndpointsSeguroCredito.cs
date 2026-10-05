using AlxorCore.Api.Comun;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Tesoreria.Aplicacion;

namespace AlxorCore.Api.Endpoints;

/// <summary>
/// Seguro de crédito: pólizas, clasificaciones de los clientes, cartera asegurada frente al riesgo y avisos de impago
/// a la aseguradora en plazo. Al facturar o confirmar un pedido a crédito se avisa (o se impide, si la póliza lo pide)
/// de la venta sin cobertura.
/// </summary>
public static class EndpointsSeguroCredito
{
    public static IEndpointRouteBuilder MapearSeguroCredito(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var g = rutas.MapGroup("/seguro-credito").WithTags("Seguro de crédito");

        g.MapGet("/polizas", (IContextoEmpresa c, GestionSeguroCredito s, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await s.PolizasAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Pólizas de seguro de crédito.").RequierePermiso(Permisos.FacturaLeer);
        g.MapPost("/polizas", (DatosPoliza d, IContextoEmpresa c, GestionSeguroCredito s, CancellationToken ct) =>
                ConEmpresa(c, async e =>
                {
                    var r = await s.CrearPolizaAsync(e, d, ct).ConfigureAwait(false);
                    return r.EsCorrecto ? Results.Created($"/seguro-credito/polizas/{r.Valor.Id}", r.Valor) : ResultadosHttp.AProblema(r.Error);
                }))
            .WithSummary("Da de alta una póliza: aseguradora, número, cobertura, plazo de aviso de impago, vigencia y si se bloquea la venta sin cobertura.")
            .RequierePermiso(Permisos.CobroRegistrar);
        g.MapPut("/polizas/{id:guid}", async (Guid id, DatosPoliza d, GestionSeguroCredito s, CancellationToken ct) => (await s.CambiarPolizaAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Cambia la póliza (o la da de baja con Activa = false).").RequierePermiso(Permisos.CobroRegistrar);

        g.MapGet("/clasificaciones", (IContextoEmpresa c, GestionSeguroCredito s, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await s.SituacionAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Cartera asegurada: cada cliente clasificado con lo concedido hoy, su riesgo, lo que queda sin cobertura y lo indemnizable.")
            .RequierePermiso(Permisos.FacturaLeer);
        g.MapPost("/clasificaciones", (DatosSolicitud d, IContextoEmpresa c, GestionSeguroCredito s, CancellationToken ct) =>
                ConEmpresa(c, async e =>
                {
                    var r = await s.SolicitarAsync(e, d, ct).ConfigureAwait(false);
                    return r.EsCorrecto ? Results.Created($"/seguro-credito/clasificaciones/{r.Valor.Id}", r.Valor) : ResultadosHttp.AProblema(r.Error);
                }))
            .WithSummary("Solicita la clasificación de un cliente en una póliza.").RequierePermiso(Permisos.CobroRegistrar);
        g.MapPost("/clasificaciones/{id:guid}/comunicacion", (Guid id, DatosComunicacion d, IContextoEmpresa c, GestionSeguroCredito s, CancellationToken ct) =>
                ConEmpresa(c, async e => (await s.ComunicarAsync(e, id, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Registra lo que comunica la aseguradora: concesión, reducción, denegación o anulación, con su fecha de efecto.")
            .RequierePermiso(Permisos.CobroRegistrar);

        g.MapGet("/impagos", (IContextoEmpresa c, GestionSeguroCredito s, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await s.ImpagosAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Impagados de clientes asegurados sin aviso, con el límite para avisar (en plazo, avisar ya o fuera de plazo).")
            .RequierePermiso(Permisos.FacturaLeer);
        g.MapGet("/avisos", (IContextoEmpresa c, GestionSeguroCredito s, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await s.AvisosAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Avisos de impago comunicados a la aseguradora.").RequierePermiso(Permisos.FacturaLeer);
        g.MapPost("/avisos", (DatosAviso d, IContextoEmpresa c, GestionSeguroCredito s, CancellationToken ct) =>
                ConEmpresa(c, async e =>
                {
                    var r = await s.AvisarAsync(e, d, ct).ConfigureAwait(false);
                    return r.EsCorrecto ? Results.Created($"/seguro-credito/avisos/{r.Valor.Id}", r.Valor) : ResultadosHttp.AProblema(r.Error);
                }))
            .WithSummary("Anota el aviso de impago de una factura a la aseguradora (con el número de siniestro).").RequierePermiso(Permisos.CobroRegistrar);
        g.MapPost("/avisos/{id:guid}/cierre", async (Guid id, DatosCierreAviso d, GestionSeguroCredito s, CancellationToken ct) =>
                (await s.CerrarAvisoAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Cierra el aviso: cobrado, indemnizado (con el importe) o retirado.").RequierePermiso(Permisos.CobroRegistrar);
        return rutas;
    }

    private static async Task<IResult> ConEmpresa(IContextoEmpresa contexto, Func<Guid, Task<IResult>> accion) =>
        contexto.EmpresaId is { } empresa
            ? await accion(empresa).ConfigureAwait(false)
            : ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
}
