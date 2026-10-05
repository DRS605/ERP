using AlxorCore.Extensiones.Aplicacion;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlxorCore.Extensiones.Infraestructura;

/// <summary>
/// Registro de las extensiones (campos personalizados, adjuntos y alertas). Las comprobaciones de otros módulos
/// (<see cref="IFuentesAlertas"/>) las registra la API.
/// </summary>
public static class RegistroServicios
{
    public const string CadenaConexion = "AlxorCore";

    public static IServiceCollection AgregarModuloExtensiones(this IServiceCollection servicios, IConfiguration configuracion)
    {
        ArgumentNullException.ThrowIfNull(servicios);
        ArgumentNullException.ThrowIfNull(configuracion);

        var conexion = configuracion.GetConnectionString(CadenaConexion)
            ?? throw new InvalidOperationException($"Falta la cadena de conexión «{CadenaConexion}».");

        servicios.AddScoped<InterceptorEmpresa>();
        servicios.AddDbContext<ExtensionesDbContext>((sp, opciones) =>
            opciones.UseNpgsql(conexion, npgsql =>
                    npgsql.MigrationsHistoryTable("__historial_migraciones", ExtensionesDbContext.Esquema))
                .AddInterceptors(sp.GetRequiredService<InterceptorEmpresa>()));

        servicios.AddScoped<IUnidadDeTrabajoExtensiones>(sp => sp.GetRequiredService<ExtensionesDbContext>());
        servicios.AddScoped<IRepositorioExtensiones, RepositorioExtensiones>();
        servicios.AddScoped<CamposPersonalizados>();
        servicios.AddScoped<AdjuntosRegistros>();
        servicios.AddScoped<IRepositorioEtiquetas, RepositorioEtiquetas>();
        servicios.AddScoped<DisenoEtiquetas>();
        servicios.AddScoped(sp => new AlertasEmpresa(sp.GetRequiredService<IRepositorioExtensiones>(), sp.GetRequiredService<IUnidadDeTrabajoExtensiones>(),
            sp.GetRequiredService<AlxorCore.Nucleo.Tiempo.IReloj>(), sp.GetService<IFuentesAlertas>()));
        return servicios;
    }
}
