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

        tc.MapDelete("/{id:guid}", async (Guid id, EliminarTipoCambio caso, CancellationToken ct) =>
            {
                var r = await caso.EjecutarAsync(id, ct).ConfigureAwait(false);
                return r.EsCorrecto ? Results.NoContent() : ResultadosHttp.AProblema(r.Error);
            })
            .WithSummary("Elimina un tipo de cambio (los documentos emitidos conservan su tasa). Para corregirlo, regístralo de nuevo en la misma fecha.")
            .RequierePermiso(Permisos.EmpresaAjustes);

        rutas.MapGet("/divisas/convertir", ConvertirAsync)
            .WithTags("Divisas")
            .WithSummary("Convierte un importe en divisa a euros a una fecha (tasa vigente).")
            .RequireAuthorization();

        rutas.MapGet("/divisas/revalorizaciones", async (IContextoEmpresa c, AlxorCore.Tesoreria.Aplicacion.GestionRevalorizacionDivisa caso, CancellationToken ct) =>
                c.EmpresaId is { } e ? Results.Ok(await caso.ListarAsync(e, ct).ConfigureAwait(false))
                    : ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero.")))
            .WithTags("Divisas").WithSummary("Revalorizaciones de saldos en divisa al cierre.")
            .RequierePermiso(Permisos.ContabilidadLeer);

        rutas.MapPost("/divisas/revalorizaciones", async (PeticionRevalorizacion p, IContextoEmpresa c, AlxorCore.Tesoreria.Aplicacion.GestionRevalorizacionDivisa caso,
                CancellationToken ct) =>
                c.EmpresaId is not { } e ? ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."))
                : p.Simular ? (await caso.CalcularAsync(e, p.Ejercicio, false, ct).ConfigureAwait(false)).AOk()
                : (await caso.CalcularAsync(e, p.Ejercicio, true, ct).ConfigureAwait(false)).ACreado("/divisas/revalorizaciones"))
            .WithTags("Divisas").WithSummary("Revaloriza a 31/12 lo pendiente de facturas y gastos en divisa (668/768 y reversión el 1/1); ?simular sin guardar.")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        rutas.MapPost("/divisas/revalorizaciones/{id:guid}/anular", async (Guid id, IContextoEmpresa c, AlxorCore.Tesoreria.Aplicacion.GestionRevalorizacionDivisa caso,
                CancellationToken ct) =>
                c.EmpresaId is { } e ? (await caso.AnularAsync(e, id, ct).ConfigureAwait(false)).AOk()
                    : ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero.")))
            .WithTags("Divisas").WithSummary("Anula una revalorización con los contraasientos de su ajuste y su reversión.")
            .RequierePermiso(Permisos.ContabilidadGestionar);

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

    /// <summary>Revalorización del ejercicio; <see cref="Simular"/> la calcula sin guardar.</summary>
    public sealed record PeticionRevalorizacion(int Ejercicio, bool Simular = false);
}
