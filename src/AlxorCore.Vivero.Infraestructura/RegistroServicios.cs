using AlxorCore.Vivero.Aplicacion;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlxorCore.Vivero.Infraestructura;

/// <summary>
/// Registro del módulo de viveros. Los puertos de existencias (<see cref="IExistenciasVivero"/>) y albaranes
/// (<see cref="IVentasVivero"/>) los registra la API.
/// </summary>
public static class RegistroServicios
{
    public const string CadenaConexion = "AlxorCore";

    public static IServiceCollection AgregarModuloVivero(this IServiceCollection servicios, IConfiguration configuracion)
    {
        ArgumentNullException.ThrowIfNull(servicios);
        ArgumentNullException.ThrowIfNull(configuracion);

        var conexion = configuracion.GetConnectionString(CadenaConexion)
            ?? throw new InvalidOperationException($"Falta la cadena de conexión «{CadenaConexion}».");

        servicios.AddScoped<InterceptorEmpresa>();
        servicios.AddDbContext<ViveroDbContext>((sp, opciones) =>
            opciones.UseNpgsql(conexion, npgsql =>
                    npgsql.MigrationsHistoryTable("__historial_migraciones", ViveroDbContext.Esquema))
                .AddInterceptors(sp.GetRequiredService<InterceptorEmpresa>()));

        servicios.AddScoped<IUnidadDeTrabajoVivero>(sp => sp.GetRequiredService<ViveroDbContext>());
        servicios.AddScoped<IRepositorioVivero, RepositorioVivero>();
        servicios.AddScoped<GestionVivero>();
        return servicios;
    }
}
