using AlxorCore.Contabilidad.Aplicacion;
using AlxorCore.Persistencia;
using AlxorCore.Recepcion.Aplicacion;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlxorCore.Contabilidad.Infraestructura;

/// <summary>Registro de servicios del módulo Contabilidad (partida doble).</summary>
public static class RegistroServicios
{
    public const string CadenaConexion = "AlxorCore";

    public static IServiceCollection AgregarModuloContabilidad(this IServiceCollection servicios, IConfiguration configuracion)
    {
        ArgumentNullException.ThrowIfNull(servicios);
        ArgumentNullException.ThrowIfNull(configuracion);

        var conexion = configuracion.GetConnectionString(CadenaConexion)
            ?? throw new InvalidOperationException($"Falta la cadena de conexión «{CadenaConexion}».");

        servicios.AddScoped<InterceptorEmpresa>();
        servicios.AddDbContext<ContabilidadDbContext>((sp, opciones) =>
            opciones.UseNpgsql(conexion, npgsql =>
                    npgsql.MigrationsHistoryTable("__historial_migraciones", ContabilidadDbContext.Esquema))
                .AddInterceptors(sp.GetRequiredService<InterceptorEmpresa>()));

        servicios.AddScoped<IUnidadDeTrabajoContabilidad>(sp => sp.GetRequiredService<ContabilidadDbContext>());
        servicios.AddScoped<IRepositorioCuentas, RepositorioCuentas>();
        servicios.AddScoped<IRepositorioAsientos, RepositorioAsientos>();
        servicios.AddScoped<IRepositorioConfigContabilidad, RepositorioConfigContabilidad>();
        servicios.AddScoped<IRepositorioDocumentosPendientes, RepositorioDocumentosPendientes>();

        servicios.AddScoped<ListarCuentas>();
        servicios.AddScoped<CrearAsiento>();
        servicios.AddScoped<ListarDiario>();
        servicios.AddScoped<MayorCuenta>();
        servicios.AddScoped<BalanceSumasYSaldos>();
        servicios.AddScoped<ObtenerModoContabilidad>();
        servicios.AddScoped<CambiarModoContabilidad>();
        servicios.AddScoped<GenerarAsientoCompra>();

        // Cola de contabilización: documentos pendientes + panel del contable.
        servicios.AddScoped<IResolverCuentas, ResolverCuentasBasico>();
        servicios.AddScoped<PosterDocumento>();
        servicios.AddScoped<EncolarDocumento>();
        servicios.AddScoped<AlxorCore.Nucleo.Aplicacion.IColaContabilizacion>(sp => sp.GetRequiredService<EncolarDocumento>());
        servicios.AddScoped<ObtenerConfigContabilidad>();
        servicios.AddScoped<CambiarContabilizacionAutomatica>();
        servicios.AddScoped<ListarPendientesContabilizar>();
        servicios.AddScoped<CambiarFechaRegistro>();
        servicios.AddScoped<ContabilizarPendientes>();

        // Sustituye al contabilizador por defecto (Recepción): ahora decide según el modo de la
        // empresa (Simple = solo gasto; Completo = gasto + asiento de partida doble).
        servicios.AddScoped<IContabilizador, ContabilizadorSegunModo>();

        return servicios;
    }
}
