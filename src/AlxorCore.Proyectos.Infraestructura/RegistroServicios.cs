using AlxorCore.Persistencia;
using AlxorCore.Proyectos.Aplicacion;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlxorCore.Proyectos.Infraestructura;

/// <summary>Registro de servicios del módulo Proyectos (imputación de costes y presupuesto vs. real).</summary>
public static class RegistroServicios
{
    public const string CadenaConexion = "AlxorCore";

    public static IServiceCollection AgregarModuloProyectos(this IServiceCollection servicios, IConfiguration configuracion)
    {
        ArgumentNullException.ThrowIfNull(servicios);
        ArgumentNullException.ThrowIfNull(configuracion);

        var conexion = configuracion.GetConnectionString(CadenaConexion)
            ?? throw new InvalidOperationException($"Falta la cadena de conexión «{CadenaConexion}».");

        servicios.AddScoped<InterceptorEmpresa>();
        servicios.AddDbContext<ProyectosDbContext>((sp, opciones) =>
            opciones.UseNpgsql(conexion, npgsql =>
                    npgsql.MigrationsHistoryTable("__historial_migraciones", ProyectosDbContext.Esquema))
                .AddInterceptors(sp.GetRequiredService<InterceptorEmpresa>()));

        servicios.AddScoped<IUnidadDeTrabajoProyectos>(sp => sp.GetRequiredService<ProyectosDbContext>());
        servicios.AddScoped<IRepositorioProyectos, RepositorioProyectos>();
        servicios.AddScoped<ITarifaPersona, TarifaPersonaPersonal>();
        servicios.AddScoped<IConsultaArticuloProyectos, ConsultaArticuloProyectosCatalogo>();

        servicios.AddScoped<CrearProyecto>();
        servicios.AddScoped<ActualizarProyecto>();
        servicios.AddScoped<CambiarEstadoProyecto>();
        servicios.AddScoped<ImputarCostes>();
        servicios.AddScoped<ListarProyectos>();
        servicios.AddScoped<ObtenerProyecto>();

        return servicios;
    }
}
