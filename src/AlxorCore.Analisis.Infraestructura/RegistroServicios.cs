using AlxorCore.Persistencia;
using AlxorCore.Analisis.Aplicacion;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlxorCore.Analisis.Infraestructura;

/// <summary>Registro de servicios del módulo Analisis (análisis de datos e informes guardados).</summary>
public static class RegistroServicios
{
    public const string CadenaConexion = "AlxorCore";

    public static IServiceCollection AgregarModuloAnalisis(this IServiceCollection servicios, IConfiguration configuracion)
    {
        ArgumentNullException.ThrowIfNull(servicios);
        ArgumentNullException.ThrowIfNull(configuracion);

        var conexion = configuracion.GetConnectionString(CadenaConexion)
            ?? throw new InvalidOperationException($"Falta la cadena de conexión «{CadenaConexion}».");

        servicios.AddScoped<InterceptorEmpresa>();
        servicios.AddDbContext<AnalisisDbContext>((sp, opciones) =>
            opciones.UseNpgsql(conexion, npgsql =>
                    npgsql.MigrationsHistoryTable("__historial_migraciones", AnalisisDbContext.Esquema))
                .AddInterceptors(sp.GetRequiredService<InterceptorEmpresa>()));

        servicios.AddScoped<IUnidadDeTrabajoAnalisis>(sp => sp.GetRequiredService<AnalisisDbContext>());
        servicios.AddScoped<IRepositorioInformesAnalisis, RepositorioInformesAnalisis>();
        servicios.AddScoped<IEjecutorAnalisis, EjecutorAnalisisPostgres>();
        servicios.AddScoped<MotorAnalisis>();
        servicios.AddScoped<GestionInformesAnalisis>();

        return servicios;
    }
}

