using AlxorCore.Persistencia;
using AlxorCore.Recepcion.Aplicacion;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlxorCore.Recepcion.Infraestructura;

/// <summary>Registro de servicios del módulo Recepción de facturas de proveedor.</summary>
public static class RegistroServicios
{
    /// <summary>Nombre de la cadena de conexión compartida.</summary>
    public const string CadenaConexion = "AlxorCore";

    public static IServiceCollection AgregarModuloRecepcion(this IServiceCollection servicios, IConfiguration configuracion)
    {
        ArgumentNullException.ThrowIfNull(servicios);
        ArgumentNullException.ThrowIfNull(configuracion);

        var conexion = configuracion.GetConnectionString(CadenaConexion)
            ?? throw new InvalidOperationException($"Falta la cadena de conexión «{CadenaConexion}».");

        servicios.AddScoped<InterceptorEmpresa>();
        servicios.AddDbContext<RecepcionDbContext>((sp, opciones) =>
            opciones.UseNpgsql(conexion, npgsql =>
                    npgsql.MigrationsHistoryTable("__historial_migraciones", RecepcionDbContext.Esquema))
                .AddInterceptors(sp.GetRequiredService<InterceptorEmpresa>()));

        servicios.AddScoped<IUnidadDeTrabajoRecepcion>(sp => sp.GetRequiredService<RecepcionDbContext>());
        servicios.AddScoped<RepositorioFacturasRecibidas>();
        servicios.AddScoped<IRepositorioFacturasRecibidas>(sp => sp.GetRequiredService<RepositorioFacturasRecibidas>());
        servicios.AddScoped<IConsultaFacturasRecibidas>(sp => sp.GetRequiredService<RepositorioFacturasRecibidas>());

        // Puerto de contabilización: hoy, modo "un libro" (crea un gasto con IVA soportado).
        servicios.AddScoped<IContabilizador, ContabilizadorGastos>();

        // Almacén de secretos + buzón de correo (adaptador real si está configurado; si no, inactivo).
        servicios.AddSingleton<IAlmacenSecretos, AlmacenSecretosConfiguracion>();
        servicios.AddOptions<OpcionesBuzon>().Bind(configuracion.GetSection(OpcionesBuzon.Seccion));
        var opcionesBuzon = new OpcionesBuzon();
        configuracion.GetSection(OpcionesBuzon.Seccion).Bind(opcionesBuzon);
        if (opcionesBuzon.Configurado)
        {
            servicios.AddScoped<IBuzonFacturas, BuzonImapMailKit>();
        }
        else
        {
            servicios.AddScoped<IBuzonFacturas, BuzonInactivo>();
        }

        // Casos de uso.
        servicios.AddScoped<RecibirFactura>();
        servicios.AddScoped<ValidarFactura>();
        servicios.AddScoped<ContabilizarFactura>();
        servicios.AddScoped<RechazarFactura>();
        servicios.AddScoped<ListarFacturasRecibidas>();
        servicios.AddScoped<ObtenerFacturaRecibida>();
        servicios.AddScoped<ProcesarBuzon>();

        return servicios;
    }
}
