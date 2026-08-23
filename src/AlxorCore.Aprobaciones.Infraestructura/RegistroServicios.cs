using AlxorCore.Aprobaciones.Aplicacion;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlxorCore.Aprobaciones.Infraestructura;

/// <summary>Composición del módulo Aprobaciones (reglas, solicitudes y segregación de funciones).</summary>
public static class RegistroServicios
{
    public const string CadenaConexion = "AlxorCore";

    public static IServiceCollection AgregarModuloAprobaciones(this IServiceCollection servicios, IConfiguration configuracion)
    {
        ArgumentNullException.ThrowIfNull(servicios);
        ArgumentNullException.ThrowIfNull(configuracion);

        var conexion = configuracion.GetConnectionString(CadenaConexion)
            ?? throw new InvalidOperationException($"Falta la cadena de conexión «{CadenaConexion}».");

        servicios.AddScoped<InterceptorEmpresa>();
        servicios.AddDbContext<AprobacionesDbContext>((sp, opciones) =>
            opciones.UseNpgsql(conexion, npgsql =>
                    npgsql.MigrationsHistoryTable("__historial_migraciones", AprobacionesDbContext.Esquema))
                .AddInterceptors(sp.GetRequiredService<InterceptorEmpresa>()));

        servicios.AddScoped<IUnidadDeTrabajoAprobaciones>(sp => sp.GetRequiredService<AprobacionesDbContext>());
        servicios.AddScoped<IRepositorioReglas, RepositorioReglas>();
        servicios.AddScoped<IRepositorioSolicitudes, RepositorioSolicitudes>();

        servicios.AddScoped<ConfigurarRegla>();
        servicios.AddScoped<ListarReglas>();
        servicios.AddScoped<CrearSolicitud>();
        servicios.AddScoped<ResolverSolicitud>();
        servicios.AddScoped<ListarSolicitudes>();
        servicios.AddScoped<ConsultarRequiere>();
        servicios.AddScoped<IFlujoAprobaciones, FlujoAprobaciones>();

        return servicios;
    }
}
