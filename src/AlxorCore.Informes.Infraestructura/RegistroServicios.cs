using AlxorCore.Informes.Aplicacion;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlxorCore.Informes.Infraestructura;

/// <summary>Composición del módulo Informes (solo casos de uso de lectura).</summary>
public static class RegistroServicios
{
    public static IServiceCollection AgregarModuloInformes(this IServiceCollection servicios)
    {
        ArgumentNullException.ThrowIfNull(servicios);

        servicios.AddScoped<ObtenerDashboard>();
        servicios.AddScoped<GenerarLibroIva>();
        servicios.AddScoped<GenerarSii>();
        servicios.AddScoped<CalcularProrrata>();
        servicios.AddScoped<GenerarResumenesFiscales>();
        servicios.AddScoped<GenerarModelo420>();
        servicios.AddScoped<GenerarModelo425>();
        servicios.AddScoped<GenerarDeclaracionAnual>();
        servicios.AddScoped<GenerarRetencionesIrpf>();
        servicios.AddScoped<GenerarModelo349>();
        servicios.AddScoped<GenerarBeneficio>();
        servicios.AddScoped<GenerarCierreCaja>();
        servicios.AddScoped<GenerarVentasPorCliente>();
        servicios.AddScoped<GenerarVentasPorArticulo>();
        servicios.AddScoped<GenerarComprasPorProveedor>();
        servicios.AddScoped<GenerarRotacionStock>();
        servicios.AddScoped<GenerarComparativaMensual>();
        servicios.AddScoped<GenerarInformePorActividad>();
        servicios.AddScoped<GenerarExtractoTercero>();
        servicios.AddScoped<GenerarAgingCartera>();

        return servicios;
    }

    /// <summary>
    /// Parte fiscal con estado propio (esquema <c>fiscal</c>): certificado de la empresa y envío del SII a la AEAT. El
    /// cifrado del certificado (<see cref="IProtectorSecretos"/>) lo aporta el anfitrión.
    /// </summary>
    public static IServiceCollection AgregarFiscalSii(this IServiceCollection servicios, IConfiguration configuracion)
    {
        ArgumentNullException.ThrowIfNull(servicios);
        ArgumentNullException.ThrowIfNull(configuracion);
        var conexion = configuracion.GetConnectionString("AlxorCore")
            ?? throw new InvalidOperationException("Falta la cadena de conexión «AlxorCore».");

        servicios.AddScoped<InterceptorEmpresa>();
        servicios.AddDbContext<FiscalDbContext>((sp, opciones) =>
            opciones.UseNpgsql(conexion, npgsql => npgsql.MigrationsHistoryTable("__historial_migraciones", FiscalDbContext.Esquema))
                .AddInterceptors(sp.GetRequiredService<InterceptorEmpresa>()));
        servicios.AddScoped<IRepositorioSii, RepositorioSii>();
        servicios.AddSingleton<ITransporteSii, TransporteSiiHttp>();
        servicios.AddScoped<GestionCertificadoSii>();
        servicios.AddScoped<EnviarSii>();
        return servicios;
    }
}
