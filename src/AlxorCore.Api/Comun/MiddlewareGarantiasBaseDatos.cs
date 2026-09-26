using AlxorCore.Persistencia;
using Npgsql;

namespace AlxorCore.Api.Comun;

/// <summary>
/// Convierte las violaciones de las garantías de la base de datos (SQLSTATE
/// <see cref="GarantiasSql.CodigoError"/>: factura emitida inalterable, hueco en la numeración, asiento
/// descuadrado…) en un 409 con el mismo formato que el resto de errores de la API. La aplicación ya
/// valida estas reglas antes; si una llega hasta aquí, la base de datos ha evitado un dato incorrecto.
/// El resto de excepciones sigue su curso.
/// </summary>
public sealed class MiddlewareGarantiasBaseDatos
{
    private readonly RequestDelegate _siguiente;
    private readonly ILogger<MiddlewareGarantiasBaseDatos> _log;

    public MiddlewareGarantiasBaseDatos(RequestDelegate siguiente, ILogger<MiddlewareGarantiasBaseDatos> log)
    {
        _siguiente = siguiente;
        _log = log;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        ArgumentNullException.ThrowIfNull(contexto);
        try
        {
            await _siguiente(contexto).ConfigureAwait(false);
        }
        catch (Exception ex) when (Garantia(ex) is { } pg && !contexto.Response.HasStarted)
        {
            _log.LogWarning("La base de datos rechazó la operación ({Codigo}): {Mensaje}", pg.Hint, pg.MessageText);
            contexto.Response.Clear();
            await Results.Problem(
                    title: pg.MessageText,
                    statusCode: StatusCodes.Status409Conflict,
                    extensions: new Dictionary<string, object?> { ["codigo"] = pg.Hint })
                .ExecuteAsync(contexto).ConfigureAwait(false);
        }
    }

    /// <summary>La violación de una garantía, si la excepción (o alguna interna) lo es.</summary>
    public static PostgresException? Garantia(Exception? ex)
    {
        for (var actual = ex; actual is not null; actual = actual.InnerException)
        {
            if (actual is PostgresException { SqlState: GarantiasSql.CodigoError } pg)
            {
                return pg;
            }
        }

        return null;
    }
}
