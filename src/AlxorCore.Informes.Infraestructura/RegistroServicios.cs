using AlxorCore.Informes.Aplicacion;
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
        servicios.AddScoped<GenerarResumenesFiscales>();
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
        servicios.AddScoped<GenerarExtractoTercero>();
        servicios.AddScoped<GenerarAgingCartera>();

        return servicios;
    }
}
