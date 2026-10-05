using AlxorCore.Bodega.Aplicacion;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlxorCore.Bodega.Infraestructura;

/// <summary>
/// Registro del módulo de bodegas. Los puertos de existencias (<see cref="IExistenciasBodega"/>), albaranes
/// (<see cref="IVentasBodega"/>) y autofacturas (<see cref="IAutofacturasBodega"/>) los registra la API.
/// </summary>
public static class RegistroServicios
{
    public const string CadenaConexion = "AlxorCore";

    public static IServiceCollection AgregarModuloBodega(this IServiceCollection servicios, IConfiguration configuracion)
    {
        ArgumentNullException.ThrowIfNull(servicios);
        ArgumentNullException.ThrowIfNull(configuracion);

        var conexion = configuracion.GetConnectionString(CadenaConexion)
            ?? throw new InvalidOperationException($"Falta la cadena de conexión «{CadenaConexion}».");

        servicios.AddScoped<InterceptorEmpresa>();
        servicios.AddDbContext<BodegaDbContext>((sp, opciones) =>
            opciones.UseNpgsql(conexion, npgsql =>
                    npgsql.MigrationsHistoryTable("__historial_migraciones", BodegaDbContext.Esquema))
                .AddInterceptors(sp.GetRequiredService<InterceptorEmpresa>()));

        servicios.AddScoped<IUnidadDeTrabajoBodega>(sp => sp.GetRequiredService<BodegaDbContext>());
        servicios.AddScoped<IRepositorioBodega, RepositorioBodega>();
        servicios.AddScoped<UvaYDepositosBodega>();
        servicios.AddScoped<OperacionesBodega>();
        return servicios;
    }
}
