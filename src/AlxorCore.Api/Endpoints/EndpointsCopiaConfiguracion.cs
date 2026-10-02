using System.Security.Claims;
using AlxorCore.Api.Comun;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Api.Endpoints;

/// <summary>
/// Copia de la configuración de otra empresa del usuario a la empresa activa (plan de cuentas, formas de pago, series,
/// almacenes, transportistas, maestros de agro…): primero la vista previa y luego la copia. Solo añade lo que falta.
/// </summary>
public static class EndpointsCopiaConfiguracion
{
    public static IEndpointRouteBuilder MapearCopiaConfiguracion(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var g = rutas.MapGroup("/empresas/actual/copia").WithTags("Empresas");

        g.MapGet("", async (IContextoEmpresa contexto, ClaimsPrincipal usuario, CopiaConfiguracion copia, CancellationToken ct) =>
                contexto.EmpresaId is not { } empresa || usuario.ObtenerUsuarioId() is not { } u
                    ? SinEmpresa()
                    : Results.Ok(new { Elementos = CopiaConfiguracion.Catalogo, Origenes = await copia.OrigenesAsync(empresa, u, ct).ConfigureAwait(false) }))
            .WithSummary("Qué se puede copiar y de qué empresas (las del usuario, salvo la activa; se indica si son del mismo grupo).")
            .RequierePermiso(Permisos.EmpresaAjustes);

        g.MapPost("/vista-previa", (PeticionCopia peticion, IContextoEmpresa contexto, ClaimsPrincipal usuario, CopiaConfiguracion copia, CancellationToken ct) =>
                EjecutarAsync(peticion, contexto, usuario, copia, false, ct))
            .WithSummary("Qué se crearía en la empresa activa, qué ya existe y qué no se puede copiar. No cambia nada.")
            .RequierePermiso(Permisos.EmpresaAjustes);

        g.MapPost("", (PeticionCopia peticion, IContextoEmpresa contexto, ClaimsPrincipal usuario, CopiaConfiguracion copia, CancellationToken ct) =>
                EjecutarAsync(peticion, contexto, usuario, copia, true, ct))
            .WithSummary("Copia a la empresa activa lo que le falta de la configuración elegida de la otra empresa. Se puede repetir: no duplica.")
            .RequierePermiso(Permisos.EmpresaAjustes);

        // Unir al grupo de la empresa activa otra empresa del usuario que está sola en su grupo (fusiona sus maestros).
        var u = rutas.MapGroup("/grupos/actual/union").WithTags("Empresas");
        u.MapGet("", async (IContextoEmpresa contexto, ClaimsPrincipal usuario, UnionGrupo union, CancellationToken ct) =>
                contexto.EmpresaId is not { } empresa || usuario.ObtenerUsuarioId() is not { } id
                    ? SinEmpresa()
                    : Results.Ok(await union.CandidatasAsync(empresa, id, ct).ConfigureAwait(false)))
            .WithSummary("Empresas del usuario de otros grupos y si se pueden unir a este (solo las que están solas en su grupo).")
            .RequierePermiso(Permisos.EmpresaAjustes);
        u.MapPost("/vista-previa", (PeticionUnion peticion, IContextoEmpresa contexto, ClaimsPrincipal usuario, UnionGrupo union, CancellationToken ct) =>
                UnirAsync(peticion, contexto, usuario, union, false, ct))
            .WithSummary("Cuántos maestros pasarían al grupo y cuáles coinciden con uno que ya tiene (se darían de baja). No cambia nada.")
            .RequierePermiso(Permisos.EmpresaAjustes);
        u.MapPost("", (PeticionUnion peticion, IContextoEmpresa contexto, ClaimsPrincipal usuario, UnionGrupo union, CancellationToken ct) =>
                UnirAsync(peticion, contexto, usuario, union, true, ct))
            .WithSummary("Une la empresa al grupo: sus clientes, proveedores, artículos… pasan a ser del grupo; los repetidos quedan de baja.")
            .RequierePermiso(Permisos.EmpresaAjustes);

        return rutas;
    }

    private static async Task<IResult> UnirAsync(PeticionUnion peticion, IContextoEmpresa contexto, ClaimsPrincipal usuario, UnionGrupo union, bool ejecutar,
        CancellationToken ct)
    {
        if (contexto.EmpresaId is not { } empresa || usuario.ObtenerUsuarioId() is not { } u)
        {
            return SinEmpresa();
        }

        return (await union.UnirAsync(empresa, u, peticion, ejecutar, ct).ConfigureAwait(false)).AOk();
    }

    private static async Task<IResult> EjecutarAsync(PeticionCopia peticion, IContextoEmpresa contexto, ClaimsPrincipal usuario, CopiaConfiguracion copia, bool ejecutar,
        CancellationToken ct)
    {
        if (contexto.EmpresaId is not { } empresa || usuario.ObtenerUsuarioId() is not { } u)
        {
            return SinEmpresa();
        }

        return (await copia.CopiarAsync(empresa, u, usuario, peticion, ejecutar, ct).ConfigureAwait(false)).AOk();
    }

    private static IResult SinEmpresa() => ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
}
