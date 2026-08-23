using AlxorCore.Identidad.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AlxorCore.Api.Comun;

/// <summary>
/// Comprobación de <i>readiness</i>: verifica que la aplicación puede hablar con PostgreSQL. Se usa en
/// <c>/salud/listo</c> para que el orquestador no envíe tráfico hasta que la base de datos responda.
/// </summary>
public sealed class ComprobacionBaseDatos : IHealthCheck
{
    private readonly IServiceScopeFactory _ambitos;

    public ComprobacionBaseDatos(IServiceScopeFactory ambitos) => _ambitos = ambitos;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var ambito = _ambitos.CreateScope();
            var contexto = ambito.ServiceProvider.GetRequiredService<IdentidadDbContext>();
            var conecta = await contexto.Database.CanConnectAsync(cancellationToken).ConfigureAwait(false);
            return conecta
                ? HealthCheckResult.Healthy("La base de datos responde.")
                : HealthCheckResult.Unhealthy("No se puede conectar con la base de datos.");
        }
#pragma warning disable CA1031 // Cualquier fallo de conexión debe traducirse en «no listo», no propagarse.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            return HealthCheckResult.Unhealthy("Error al comprobar la base de datos.", ex);
        }
    }
}
