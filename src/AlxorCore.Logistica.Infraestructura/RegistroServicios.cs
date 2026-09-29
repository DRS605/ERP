using AlxorCore.Logistica.Aplicacion;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlxorCore.Logistica.Infraestructura;

/// <summary>
/// Registro del módulo de logística. Los puertos hacia el inventario, el catálogo, los pedidos y la fabricación
/// (<see cref="IExistenciasLogistica"/>, <see cref="IArticulosLogistica"/>, <see cref="IPedidosLogistica"/> y
/// <see cref="IFabricacionLogistica"/>) los registra la API, que conoce esos módulos.
/// </summary>
public static class RegistroServicios
{
    public const string CadenaConexion = "AlxorCore";

    public static IServiceCollection AgregarModuloLogistica(this IServiceCollection servicios, IConfiguration configuracion)
    {
        ArgumentNullException.ThrowIfNull(servicios);
        ArgumentNullException.ThrowIfNull(configuracion);

        var conexion = configuracion.GetConnectionString(CadenaConexion)
            ?? throw new InvalidOperationException($"Falta la cadena de conexión «{CadenaConexion}».");

        servicios.AddScoped<InterceptorEmpresa>();
        servicios.AddDbContext<LogisticaDbContext>((sp, opciones) =>
            opciones.UseNpgsql(conexion, npgsql =>
                    npgsql.MigrationsHistoryTable("__historial_migraciones", LogisticaDbContext.Esquema))
                .AddInterceptors(sp.GetRequiredService<InterceptorEmpresa>()));

        servicios.AddScoped<IUnidadDeTrabajoLogistica>(sp => sp.GetRequiredService<LogisticaDbContext>());
        servicios.AddScoped<IRepositorioLogistica, RepositorioLogistica>();
        servicios.AddScoped<MaestrosLogistica>();
        servicios.AddScoped<PaletizacionLogistica>();
        return servicios;
    }
}
