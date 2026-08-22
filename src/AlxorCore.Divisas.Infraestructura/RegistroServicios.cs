using AlxorCore.Divisas.Aplicacion;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlxorCore.Divisas.Infraestructura;

/// <summary>Composición del módulo Divisas (tipos de cambio, conversión y diferencias de cambio).</summary>
public static class RegistroServicios
{
    public const string CadenaConexion = "AlxorCore";

    public static IServiceCollection AgregarModuloDivisas(this IServiceCollection servicios, IConfiguration configuracion)
    {
        ArgumentNullException.ThrowIfNull(servicios);
        ArgumentNullException.ThrowIfNull(configuracion);

        var conexion = configuracion.GetConnectionString(CadenaConexion)
            ?? throw new InvalidOperationException($"Falta la cadena de conexión «{CadenaConexion}».");

        servicios.AddScoped<InterceptorEmpresa>();
        servicios.AddDbContext<DivisasDbContext>((sp, opciones) =>
            opciones.UseNpgsql(conexion, npgsql =>
                    npgsql.MigrationsHistoryTable("__historial_migraciones", DivisasDbContext.Esquema))
                .AddInterceptors(sp.GetRequiredService<InterceptorEmpresa>()));

        servicios.AddScoped<IUnidadDeTrabajoDivisas>(sp => sp.GetRequiredService<DivisasDbContext>());
        servicios.AddScoped<RepositorioTiposCambio>();
        servicios.AddScoped<IRepositorioTiposCambio>(sp => sp.GetRequiredService<RepositorioTiposCambio>());
        servicios.AddScoped<IConsultaTiposCambio>(sp => sp.GetRequiredService<RepositorioTiposCambio>());
        servicios.AddScoped<IConversorDivisa, ConversorDivisa>();

        servicios.AddScoped<RegistrarTipoCambio>();
        servicios.AddScoped<ListarTiposCambio>();
        servicios.AddScoped<ConvertirImporte>();
        servicios.AddScoped<CalcularDiferenciasCambio>();

        return servicios;
    }
}
