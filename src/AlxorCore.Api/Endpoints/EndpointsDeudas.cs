using AlxorCore.Api.Comun;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Tesoreria.Aplicacion;
using AlxorCore.Tesoreria.Dominio;

namespace AlxorCore.Api.Endpoints;

/// <summary>Situación de la deuda de clientes: impagados (4315), dudosos con deterioro, incobrables y renovación de efectos.</summary>
public static class EndpointsDeudas
{
    public static IEndpointRouteBuilder MapearDeudas(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var g = rutas.MapGroup("/tesoreria").WithTags("Tesorería · deuda de clientes");

        g.MapGet("/cartera/configuracion", async (GestionDeudas d, CancellationToken ct) => Results.Ok(await d.ConfiguracionAsync(ct).ConfigureAwait(false)))
            .WithSummary("Si los recibos devueltos pasan a impagados (4315) y el porcentaje de renovación.").RequierePermiso(Permisos.FacturaLeer);
        g.MapPut("/cartera/configuracion", (ConfiguracionCarteraDto p, IContextoEmpresa c, GestionDeudas d, CancellationToken ct) =>
                ConEmpresa(c, async e => (await d.GuardarConfiguracionAsync(e, p, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Guarda la configuración de la cartera.").RequierePermiso(Permisos.CobroRegistrar);

        g.MapGet("/deudas", async (GestionDeudas d, CancellationToken ct) => Results.Ok(await d.SituacionesAsync(ct).ConfigureAwait(false)))
            .WithSummary("Documentos con la deuda en impagados o en dudoso cobro, con su deterioro, e incobrables.").RequierePermiso(Permisos.FacturaLeer);
        g.MapPost("/deudas/{tipo}/{documentoId:guid}/clasificar", (TipoDocumentoTesoreria tipo, Guid documentoId, ClasificarDeudaComando p, IContextoEmpresa c, GestionDeudas d,
                CancellationToken ct) => ConEmpresa(c, async e => (await d.ClasificarAsync(e, tipo, documentoId, p, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Clasifica la deuda (dudoso, precontencioso, contencioso, moroso): pasa a 436 y, si se indica, se dota el deterioro.").RequierePermiso(Permisos.CobroRegistrar);
        g.MapPost("/deudas/{id:guid}/dotar", async (Guid id, DotarDeudaComando p, GestionDeudas d, CancellationToken ct) => (await d.DotarAsync(id, p, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Fija el deterioro dotado (694/490 al subir, 490/794 al bajar).").RequierePermiso(Permisos.CobroRegistrar);
        g.MapPost("/deudas/{id:guid}/desclasificar", async (Guid id, GestionDeudas d, CancellationToken ct) => (await d.DesclasificarAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Deja de ser dudosa: vuelve a la cuenta del cliente y se revierte el deterioro.").RequierePermiso(Permisos.CobroRegistrar);
        g.MapPost("/deudas/{tipo}/{documentoId:guid}/incobrable", (TipoDocumentoTesoreria tipo, Guid documentoId, IncobrableComando p, IContextoEmpresa c, GestionDeudas d,
                CancellationToken ct) => ConEmpresa(c, async e => (await d.IncobrableAsync(e, tipo, documentoId, p, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Declara incobrable lo pendiente: a pérdidas (650), aplicando el deterioro. Se deshace anulando el cobro.").RequierePermiso(Permisos.CobroRegistrar);

        g.MapGet("/renovaciones", async (GestionDeudas d, CancellationToken ct) => Results.Ok(await d.RenovacionesAsync(ct).ConfigureAwait(false)))
            .WithSummary("Renovaciones de efectos con sus efectos nuevos.").RequierePermiso(Permisos.FacturaLeer);
        g.MapPost("/deudas/{tipo}/{documentoId:guid}/renovar", (TipoDocumentoTesoreria tipo, Guid documentoId, RenovarComando p, IContextoEmpresa c, GestionDeudas d,
                CancellationToken ct) => ConEmpresa(c, async e =>
                {
                    var r = await d.RenovarAsync(e, tipo, documentoId, p, ct).ConfigureAwait(false);
                    return r.EsCorrecto ? Results.Created($"/tesoreria/renovaciones/{r.Valor.Id}", r.Valor) : ResultadosHttp.AProblema(r.Error);
                }))
            .WithSummary("Renueva lo pendiente en efectos nuevos (cartera 4310), con gastos e intereses para el cliente (769).").RequierePermiso(Permisos.CobroRegistrar);
        g.MapPost("/renovaciones/{id:guid}/anular", (Guid id, IContextoEmpresa c, GestionDeudas d, CancellationToken ct) =>
                ConEmpresa(c, async e => (await d.AnularRenovacionAsync(e, id, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Deshace una renovación cuyos efectos no tienen cobros.").RequierePermiso(Permisos.CobroRegistrar);
        return rutas;
    }

    private static async Task<IResult> ConEmpresa(IContextoEmpresa contexto, Func<Guid, Task<IResult>> accion) =>
        contexto.EmpresaId is { } e
            ? await accion(e).ConfigureAwait(false)
            : ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
}
