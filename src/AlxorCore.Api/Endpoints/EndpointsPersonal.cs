using AlxorCore.Api.Comun;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Personal.Aplicacion;

namespace AlxorCore.Api.Endpoints;

/// <summary>Endpoints REST del módulo Personal (personas con tarifa).</summary>
public static class EndpointsPersonal
{
    public static IEndpointRouteBuilder MapearPersonal(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        var g = rutas.MapGroup("/personal").WithTags("Personal");
        g.MapGet("", ListarAsync).WithSummary("Lista las personas con su tarifa.").RequierePermiso(Permisos.PersonalLeer);
        g.MapPost("", CrearAsync).WithSummary("Crea una persona.").RequierePermiso(Permisos.PersonalGestionar);
        g.MapPut("/{id:guid}", ActualizarAsync).WithSummary("Actualiza una persona (nombre, puesto, tarifa, activa).").RequierePermiso(Permisos.PersonalGestionar);

        return rutas;
    }

    private static IResult SinEmpresa() => ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));

    private static async Task<IResult> ListarAsync(IContextoEmpresa c, ListarPersonas caso, CancellationToken ct)
        => c.EmpresaId is null ? SinEmpresa() : Results.Ok(await caso.EjecutarAsync(c.EmpresaId.Value, ct).ConfigureAwait(false));

    private static async Task<IResult> CrearAsync(DatosPersona datos, IContextoEmpresa c, CrearPersona caso, CancellationToken ct)
    {
        if (c.EmpresaId is null) return SinEmpresa();
        var r = await caso.EjecutarAsync(c.EmpresaId.Value, datos, ct).ConfigureAwait(false);
        return r.EsCorrecto ? r.ACreado("/personal") : ResultadosHttp.AProblema(r.Error);
    }

    private static async Task<IResult> ActualizarAsync(Guid id, DatosPersona datos, ActualizarPersona caso, CancellationToken ct) =>
        (await caso.EjecutarAsync(id, datos, ct).ConfigureAwait(false)).AOk();
}
