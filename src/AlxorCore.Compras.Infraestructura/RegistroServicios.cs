using AlxorCore.Compras.Aplicacion;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlxorCore.Compras.Infraestructura;

/// <summary>Registro de servicios del módulo Compras (solicitud → pedido → albarán → factura).</summary>
public static class RegistroServicios
{
    public const string CadenaConexion = "AlxorCore";

    public static IServiceCollection AgregarModuloCompras(this IServiceCollection servicios, IConfiguration configuracion)
    {
        ArgumentNullException.ThrowIfNull(servicios);
        ArgumentNullException.ThrowIfNull(configuracion);

        var conexion = configuracion.GetConnectionString(CadenaConexion)
            ?? throw new InvalidOperationException($"Falta la cadena de conexión «{CadenaConexion}».");

        servicios.AddScoped<InterceptorEmpresa>();
        servicios.AddDbContext<ComprasDbContext>((sp, opciones) =>
            opciones.UseNpgsql(conexion, npgsql =>
                    npgsql.MigrationsHistoryTable("__historial_migraciones", ComprasDbContext.Esquema))
                .AddInterceptors(sp.GetRequiredService<InterceptorEmpresa>()));

        servicios.AddScoped<IUnidadDeTrabajoCompras>(sp => sp.GetRequiredService<ComprasDbContext>());
        servicios.AddScoped<IRepositorioSolicitudes, RepositorioSolicitudes>();
        servicios.AddScoped<IRepositorioPedidos, RepositorioPedidos>();
        servicios.AddScoped<IRepositorioAlbaranes, RepositorioAlbaranes>();

        servicios.AddScoped<CrearSolicitud>();
        servicios.AddScoped<DecidirSolicitud>();
        servicios.AddScoped<ListarSolicitudes>();
        servicios.AddScoped<CrearPedido>();
        servicios.AddScoped<DecidirPedido>();
        servicios.AddScoped<ListarPedidos>();
        servicios.AddScoped<ObtenerPedido>();
        servicios.AddScoped<RecibirMercancia>();
        servicios.AddScoped<ListarAlbaranesPedido>();
        servicios.AddScoped<FacturarPedido>();

        return servicios;
    }
}
