using AlxorCore.Integraciones.Aplicacion;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlxorCore.Integraciones.Infraestructura;

/// <summary>Composición del módulo Integraciones (API pública por clave y webhooks salientes).</summary>
public static class RegistroServicios
{
    public const string CadenaConexion = "AlxorCore";

    public static IServiceCollection AgregarModuloIntegraciones(this IServiceCollection servicios, IConfiguration configuracion)
    {
        ArgumentNullException.ThrowIfNull(servicios);
        ArgumentNullException.ThrowIfNull(configuracion);

        var conexion = configuracion.GetConnectionString(CadenaConexion)
            ?? throw new InvalidOperationException($"Falta la cadena de conexión «{CadenaConexion}».");

        servicios.AddScoped<InterceptorEmpresa>();
        servicios.AddDbContext<IntegracionesDbContext>((sp, opciones) =>
            opciones.UseNpgsql(conexion, npgsql =>
                    npgsql.MigrationsHistoryTable("__historial_migraciones", IntegracionesDbContext.Esquema))
                .AddInterceptors(sp.GetRequiredService<InterceptorEmpresa>()));

        servicios.AddScoped<IUnidadDeTrabajoIntegraciones>(sp => sp.GetRequiredService<IntegracionesDbContext>());
        servicios.AddScoped<IRepositorioClavesApi, RepositorioClavesApi>();
        servicios.AddScoped<IRepositorioSuscripciones, RepositorioSuscripciones>();
        servicios.AddScoped<IRepositorioEntregas, RepositorioEntregas>();

        servicios.AddScoped<CrearClaveApi>();
        servicios.AddScoped<ListarClavesApi>();
        servicios.AddScoped<RevocarClaveApi>();
        servicios.AddScoped<AutenticarClaveApi>();
        servicios.AddScoped<CrearSuscripcionWebhook>();
        servicios.AddScoped<ListarSuscripcionesWebhook>();
        servicios.AddScoped<EliminarSuscripcionWebhook>();
        servicios.AddScoped<ListarEntregasWebhook>();
        servicios.AddScoped<ProcesarEntregasWebhook>();
        servicios.AddScoped<IColaWebhooks, ColaWebhooks>();

        servicios.AddHttpClient("webhooks", cliente => cliente.Timeout = TimeSpan.FromSeconds(15));
        servicios.AddScoped<IClienteHttpWebhook, ClienteHttpWebhook>();

        return servicios;
    }
}
