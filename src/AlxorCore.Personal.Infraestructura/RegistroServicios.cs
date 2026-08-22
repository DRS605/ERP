using AlxorCore.Persistencia;
using AlxorCore.Personal.Aplicacion;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlxorCore.Personal.Infraestructura;

/// <summary>Registro de servicios del módulo Personal (personas con tarifa).</summary>
public static class RegistroServicios
{
    public const string CadenaConexion = "AlxorCore";

    public static IServiceCollection AgregarModuloPersonal(this IServiceCollection servicios, IConfiguration configuracion)
    {
        ArgumentNullException.ThrowIfNull(servicios);
        ArgumentNullException.ThrowIfNull(configuracion);

        var conexion = configuracion.GetConnectionString(CadenaConexion)
            ?? throw new InvalidOperationException($"Falta la cadena de conexión «{CadenaConexion}».");

        servicios.AddScoped<InterceptorEmpresa>();
        servicios.AddDbContext<PersonalDbContext>((sp, opciones) =>
            opciones.UseNpgsql(conexion, npgsql =>
                    npgsql.MigrationsHistoryTable("__historial_migraciones", PersonalDbContext.Esquema))
                .AddInterceptors(sp.GetRequiredService<InterceptorEmpresa>()));

        servicios.AddScoped<IUnidadDeTrabajoPersonal>(sp => sp.GetRequiredService<PersonalDbContext>());
        servicios.AddScoped<RepositorioPersonas>();
        servicios.AddScoped<IRepositorioPersonas>(sp => sp.GetRequiredService<RepositorioPersonas>());
        servicios.AddScoped<IConsultaPersonas>(sp => sp.GetRequiredService<RepositorioPersonas>());

        servicios.AddScoped<CrearPersona>();
        servicios.AddScoped<ActualizarPersona>();
        servicios.AddScoped<ListarPersonas>();

        return servicios;
    }
}
