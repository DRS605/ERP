using AlxorCore.Persistencia;
using AlxorCore.Produccion.Aplicacion;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlxorCore.Produccion.Infraestructura;

/// <summary>Registro de servicios del módulo Producción (órdenes de fabricación).</summary>
public static class RegistroServicios
{
    public const string CadenaConexion = "AlxorCore";

    public static IServiceCollection AgregarModuloProduccion(this IServiceCollection servicios, IConfiguration configuracion)
    {
        ArgumentNullException.ThrowIfNull(servicios);
        ArgumentNullException.ThrowIfNull(configuracion);

        var conexion = configuracion.GetConnectionString(CadenaConexion)
            ?? throw new InvalidOperationException($"Falta la cadena de conexión «{CadenaConexion}».");

        servicios.AddScoped<InterceptorEmpresa>();
        servicios.AddDbContext<ProduccionDbContext>((sp, opciones) =>
            opciones.UseNpgsql(conexion, npgsql =>
                    npgsql.MigrationsHistoryTable("__historial_migraciones", ProduccionDbContext.Esquema))
                .AddInterceptors(sp.GetRequiredService<InterceptorEmpresa>()));

        servicios.AddScoped<IUnidadDeTrabajoProduccion>(sp => sp.GetRequiredService<ProduccionDbContext>());
        servicios.AddScoped<IRepositorioOrdenes, RepositorioOrdenes>();
        servicios.AddScoped<IConsultaListaMateriales, ConsultaListaMaterialesCatalogo>();
        servicios.AddScoped<IMontajeProduccion, MontajeProduccionInventario>();

        servicios.AddScoped<CrearOrden>();
        servicios.AddScoped<DecidirOrden>();
        servicios.AddScoped<ListarOrdenes>();
        servicios.AddScoped<ObtenerOrden>();

        return servicios;
    }
}
