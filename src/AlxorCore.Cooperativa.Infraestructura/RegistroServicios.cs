using AlxorCore.Cooperativa.Aplicacion;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlxorCore.Cooperativa.Infraestructura;

/// <summary>
/// Registro del módulo de cooperativas y SAT. Los puertos <see cref="IActividadSocios"/> (lo entregado, desde agro) y
/// <see cref="IContabilidadCooperativa"/> (los asientos) los registra la API, que conoce esos módulos.
/// </summary>
public static class RegistroServicios
{
    public const string CadenaConexion = "AlxorCore";

    public static IServiceCollection AgregarModuloCooperativa(this IServiceCollection servicios, IConfiguration configuracion)
    {
        ArgumentNullException.ThrowIfNull(servicios);
        ArgumentNullException.ThrowIfNull(configuracion);

        var conexion = configuracion.GetConnectionString(CadenaConexion)
            ?? throw new InvalidOperationException($"Falta la cadena de conexión «{CadenaConexion}».");

        servicios.AddScoped<InterceptorEmpresa>();
        servicios.AddDbContext<CooperativaDbContext>((sp, opciones) =>
            opciones.UseNpgsql(conexion, npgsql =>
                    npgsql.MigrationsHistoryTable("__historial_migraciones", CooperativaDbContext.Esquema))
                .AddInterceptors(sp.GetRequiredService<InterceptorEmpresa>()));

        servicios.AddScoped<IUnidadDeTrabajoCooperativa>(sp => sp.GetRequiredService<CooperativaDbContext>());
        servicios.AddScoped<IRepositorioCooperativa, RepositorioCooperativa>();
        servicios.AddScoped<SociosCooperativa>();
        servicios.AddScoped<RepartosCooperativa>();
        servicios.AddScoped<ActasCooperativa>();
        return servicios;
    }
}
