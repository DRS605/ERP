using AlxorCore.Inventario.Aplicacion;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlxorCore.Inventario.Infraestructura;

/// <summary>Registro de servicios del módulo Inventario (multi-almacén, ubicaciones, movimientos).</summary>
public static class RegistroServicios
{
    public const string CadenaConexion = "AlxorCore";

    public static IServiceCollection AgregarModuloInventario(this IServiceCollection servicios, IConfiguration configuracion)
    {
        ArgumentNullException.ThrowIfNull(servicios);
        ArgumentNullException.ThrowIfNull(configuracion);

        var conexion = configuracion.GetConnectionString(CadenaConexion)
            ?? throw new InvalidOperationException($"Falta la cadena de conexión «{CadenaConexion}».");

        servicios.AddScoped<InterceptorEmpresa>();
        servicios.AddDbContext<InventarioDbContext>((sp, opciones) =>
            opciones.UseNpgsql(conexion, npgsql =>
                    npgsql.MigrationsHistoryTable("__historial_migraciones", InventarioDbContext.Esquema))
                .AddInterceptors(sp.GetRequiredService<InterceptorEmpresa>()));

        servicios.AddScoped<IUnidadDeTrabajoInventario>(sp => sp.GetRequiredService<InventarioDbContext>());
        servicios.AddScoped<IRepositorioAlmacenes, RepositorioAlmacenes>();
        servicios.AddScoped<IRepositorioExistencias, RepositorioExistencias>();
        servicios.AddScoped<IRepositorioMovimientos, RepositorioMovimientos>();
        servicios.AddScoped<IRepositorioUbicacionesDefecto, RepositorioUbicacionesDefecto>();

        servicios.AddScoped<GestionAlmacenes>();
        servicios.AddScoped<MovimientosInventario>();
        servicios.AddScoped<ConsultasInventario>();
        servicios.AddScoped<UbicacionesPorDefecto>();
        servicios.AddScoped<TrazabilidadLote>();
        servicios.AddScoped<IConsultaComposicion, ConsultaComposicionCatalogo>();
        servicios.AddScoped<MontajeArticulo>();

        return servicios;
    }
}
