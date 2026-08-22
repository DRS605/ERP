using AlxorCore.Api.Comun;
using AlxorCore.Contabilidad.Aplicacion;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Api.Endpoints;

/// <summary>Endpoints REST del inmovilizado y las amortizaciones (submódulo de Contabilidad).</summary>
public static class EndpointsInmovilizado
{
    public static IEndpointRouteBuilder MapearInmovilizado(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        var grupo = rutas.MapGroup("/contabilidad/inmovilizado").WithTags("Inmovilizado");

        grupo.MapGet("/", ListarAsync)
            .WithSummary("Lista los inmovilizados de la empresa.")
            .RequierePermiso(Permisos.ContabilidadLeer);

        grupo.MapPost("/", CrearAsync)
            .WithSummary("Da de alta un inmovilizado (plan de amortización contable y fiscal).")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        grupo.MapGet("/{id:guid}/cuadro", CuadroAsync)
            .WithSummary("Cuadro de amortización (contable, fiscal y diferencia) por ejercicio.")
            .RequierePermiso(Permisos.ContabilidadLeer);

        grupo.MapPost("/amortizar", AmortizarAsync)
            .WithSummary("Genera la dotación de amortización del ejercicio y el impuesto diferido.")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        grupo.MapPost("/{id:guid}/baja", BajaAsync)
            .WithSummary("Da de baja un inmovilizado (sin contraprestación) y genera su asiento.")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        grupo.MapPost("/{id:guid}/enajenar", EnajenarAsync)
            .WithSummary("Enajena (vende) un inmovilizado y genera su asiento con el resultado.")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        return rutas;
    }

    private static async Task<IResult> ListarAsync(IContextoEmpresa contexto, ListarInmovilizados caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> CrearAsync(CrearInmovilizadoComando comando, IContextoEmpresa contexto, CrearInmovilizado caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var resultado = await caso.EjecutarAsync(contexto.EmpresaId.Value, comando, ct).ConfigureAwait(false);
        return resultado.EsCorrecto ? resultado.ACreado("/contabilidad/inmovilizado") : ResultadosHttp.AProblema(resultado.Error);
    }

    private static async Task<IResult> CuadroAsync(Guid id, IContextoEmpresa contexto, ObtenerCuadroAmortizacion caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var resultado = await caso.EjecutarAsync(contexto.EmpresaId.Value, id, ct).ConfigureAwait(false);
        return resultado.EsCorrecto ? Results.Ok(resultado.Valor) : ResultadosHttp.AProblema(resultado.Error);
    }

    private static async Task<IResult> AmortizarAsync(int? ejercicio, decimal? tipoImpuesto, bool? impuestoDiferido,
        IContextoEmpresa contexto, GenerarAmortizacion caso, IReloj reloj, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var anio = ejercicio ?? reloj.AhoraUtc.Year;
        var resultado = await caso.EjecutarAsync(contexto.EmpresaId.Value, anio, tipoImpuesto ?? 0.25m, impuestoDiferido ?? true, ct).ConfigureAwait(false);
        return resultado.EsCorrecto ? Results.Ok(resultado.Valor) : ResultadosHttp.AProblema(resultado.Error);
    }

    private static async Task<IResult> BajaAsync(Guid id, BajaInmovilizadoComando comando, IContextoEmpresa contexto, DarDeBajaInmovilizado caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var resultado = await caso.EjecutarAsync(contexto.EmpresaId.Value, id, comando, ct).ConfigureAwait(false);
        return resultado.EsCorrecto ? Results.Ok(resultado.Valor) : ResultadosHttp.AProblema(resultado.Error);
    }

    private static async Task<IResult> EnajenarAsync(Guid id, EnajenarInmovilizadoComando comando, IContextoEmpresa contexto, EnajenarInmovilizado caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var resultado = await caso.EjecutarAsync(contexto.EmpresaId.Value, id, comando, ct).ConfigureAwait(false);
        return resultado.EsCorrecto ? Results.Ok(resultado.Valor) : ResultadosHttp.AProblema(resultado.Error);
    }
}
