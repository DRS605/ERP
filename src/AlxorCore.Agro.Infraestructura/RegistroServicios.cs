using AlxorCore.Agro.Aplicacion;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlxorCore.Agro.Infraestructura;

/// <summary>Registro de servicios del módulo agro (recepción de fruta, liquidación al agricultor, confección y trazabilidad).</summary>
public static class RegistroServicios
{
    public const string CadenaConexion = "AlxorCore";

    /// <summary>
    /// Registra el módulo. Los puertos <see cref="IAutofacturas"/> y <see cref="ICosteAnalitico"/> los registra la
    /// API, que conoce los módulos de Gastos, Tesorería y Contabilidad.
    /// </summary>
    public static IServiceCollection AgregarModuloAgro(this IServiceCollection servicios, IConfiguration configuracion)
    {
        ArgumentNullException.ThrowIfNull(servicios);
        ArgumentNullException.ThrowIfNull(configuracion);

        var conexion = configuracion.GetConnectionString(CadenaConexion)
            ?? throw new InvalidOperationException($"Falta la cadena de conexión «{CadenaConexion}».");

        servicios.AddScoped<InterceptorEmpresa>();
        servicios.AddDbContext<AgroDbContext>((sp, opciones) =>
            opciones.UseNpgsql(conexion, npgsql =>
                    npgsql.MigrationsHistoryTable("__historial_migraciones", AgroDbContext.Esquema))
                .AddInterceptors(sp.GetRequiredService<InterceptorEmpresa>()));

        servicios.AddScoped<IUnidadDeTrabajoAgro>(sp => new UnidadAgroConInventario(sp.GetRequiredService<AgroDbContext>(), sp));
        servicios.AddScoped<IRepositorioAgro, RepositorioAgro>();
        servicios.AddScoped<IRepositorioEnvases, RepositorioEnvases>();
        servicios.AddScoped<EnvasesTerceros>();
        servicios.AddScoped<IRepositorioReservas, RepositorioReservas>();
        servicios.AddScoped<ReservasPales>();

        servicios.AddScoped<MaestrosAgro>();
        servicios.AddScoped<RecepcionesAgro>();
        servicios.AddScoped<LiquidacionesAgro>();
        servicios.AddScoped<ConfeccionAgro>();
        servicios.AddScoped<PalesAgro>();
        servicios.AddScoped<IRepositorioOrdenesCarga, RepositorioOrdenesCarga>();
        servicios.AddScoped<OrdenesCargaAgro>();
        servicios.AddScoped<IRepositorioCuaderno, RepositorioCuaderno>();
        servicios.AddScoped<CuadernoCampoAgro>();
        servicios.AddScoped<IRepositorioAutoevaluaciones, RepositorioAutoevaluaciones>();
        servicios.AddScoped<AutoevaluacionesAgro>();
        servicios.AddScoped<TrazabilidadAgro>();
        servicios.AddScoped<InformesAgro>();

        return servicios;
    }
}
