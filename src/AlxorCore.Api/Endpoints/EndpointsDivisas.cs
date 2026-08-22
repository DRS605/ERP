using AlxorCore.Api.Comun;
using AlxorCore.Divisas.Aplicacion;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Api.Endpoints;

/// <summary>Endpoints REST del módulo Divisas (tipos de cambio, conversión y diferencias de cambio).</summary>
public static class EndpointsDivisas
{
    public static IEndpointRouteBuilder MapearDivisas(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        rutas.MapGet("/divisas", () => Results.Ok(ListarDivisas.Ejecutar()))
            .WithTags("Divisas")
            .WithSummary("Catálogo de divisas admitidas.")
            .RequireAuthorization();

        var tc = rutas.MapGroup("/tipos-cambio").WithTags("Divisas");

        tc.MapGet("", ListarAsync)
            .WithSummary("Lista los tipos de cambio de la empresa.")
            .RequireAuthorization();

        tc.MapPost("", RegistrarAsync)
            .WithSummary("Registra o actualiza el tipo de cambio de una divisa en una fecha.")
            .RequierePermiso(Permisos.EmpresaAjustes);

        rutas.MapGet("/divisas/convertir", ConvertirAsync)
            .WithTags("Divisas")
            .WithSummary("Convierte un importe en divisa a euros a una fecha (tasa vigente).")
            .RequireAuthorization();

        rutas.MapPost("/divisas/diferencias-cambio", DiferenciasAsync)
            .WithTags("Divisas")
            .WithSummary("Calcula las diferencias de cambio de posiciones abiertas en divisa a una fecha.")
            .RequierePermiso(Permisos.InformeLeer);

        return rutas;
    }

    /// <summary>Cuerpo para registrar un tipo de cambio.</summary>
    public sealed record PeticionTipoCambio(string Divisa, DateOnly Fecha, decimal TasaEur);

    private static async Task<IResult> ListarAsync(IContextoEmpresa contexto, ListarTiposCambio caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> RegistrarAsync(PeticionTipoCambio peticion, IContextoEmpresa contexto, RegistrarTipoCambio caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var r = await caso.EjecutarAsync(contexto.EmpresaId.Value, peticion.Divisa, peticion.Fecha, peticion.TasaEur, ct).ConfigureAwait(false);
        return r.EsCorrecto ? r.ACreado("/tipos-cambio") : ResultadosHttp.AProblema(r.Error);
    }

    private static async Task<IResult> ConvertirAsync(IContextoEmpresa contexto, ConvertirImporte caso, string divisa, decimal importe, DateOnly? fecha, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var f = fecha ?? DateOnly.FromDateTime(DateTime.UtcNow);
        return (await caso.EjecutarAsync(contexto.EmpresaId.Value, divisa, importe, f, ct).ConfigureAwait(false)).AOk();
    }

    private static async Task<IResult> DiferenciasAsync(DiferenciasCambioComando comando, IContextoEmpresa contexto, CalcularDiferenciasCambio caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return (await caso.EjecutarAsync(contexto.EmpresaId.Value, comando, ct).ConfigureAwait(false)).AOk();
    }
}
