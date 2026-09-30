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

        return rutas;
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
