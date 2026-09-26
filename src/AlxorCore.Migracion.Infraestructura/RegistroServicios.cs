using AlxorCore.Migracion.Hispatec;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlxorCore.Migracion.Infraestructura;

/// <summary>Registro de servicios de la migración desde otros ERP (Hispatec).</summary>
public static class RegistroServicios
{
    public const string CadenaConexion = "AlxorCore";

    /// <summary>Registra el contexto de correspondencias y la carga desde Hispatec.</summary>
    public static IServiceCollection AgregarModuloMigracion(this IServiceCollection servicios, IConfiguration configuracion)
    {
        ArgumentNullException.ThrowIfNull(servicios);
        ArgumentNullException.ThrowIfNull(configuracion);

        var conexion = configuracion.GetConnectionString(CadenaConexion)
            ?? throw new InvalidOperationException($"Falta la cadena de conexión «{CadenaConexion}».");

        servicios.AddScoped<InterceptorEmpresa>();
        servicios.AddDbContext<MigracionDbContext>((sp, opciones) =>
            opciones.UseNpgsql(conexion, npgsql =>
                    npgsql.MigrationsHistoryTable("__historial_migraciones", MigracionDbContext.Esquema))
                .AddInterceptors(sp.GetRequiredService<InterceptorEmpresa>()));

        servicios.AddScoped<IUnidadDeTrabajoMigracion>(sp => sp.GetRequiredService<MigracionDbContext>());
        servicios.AddScoped<IRepositorioCorrespondencias, RepositorioCorrespondencias>();
        servicios.AddScoped<CargaHispatec>();

        return servicios;
    }
}
