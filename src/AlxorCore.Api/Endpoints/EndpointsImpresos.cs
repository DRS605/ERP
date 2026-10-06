using AlxorCore.Api.Comun;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Api.Endpoints;

/// <summary>PDF de albaranes, pedidos y liquidaciones al agricultor.</summary>
public static class EndpointsImpresos
{
    public static IEndpointRouteBuilder MapearImpresos(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        rutas.MapGet("/albaranes-venta/{id:guid}/pdf", (Guid id, bool? valorado, string? idioma, IContextoEmpresa c, ImpresosComerciales i, CancellationToken ct) =>
                Pdf(c, e => i.AlbaranAsync(e, id, valorado != false, idioma, ct)))
            .WithTags("Documentos").WithSummary("PDF del albarán de venta (?valorado=false: sin precios, el que viaja con la mercancía; ?idioma=en: en otro idioma, por defecto el del cliente).")
            .RequierePermiso(Permisos.FacturaLeer);
        rutas.MapGet("/pedidos-venta/{id:guid}/pdf", (Guid id, string? idioma, IContextoEmpresa c, ImpresosComerciales i, CancellationToken ct) => Pdf(c, e => i.PedidoVentaAsync(e, id, idioma, ct)))
            .WithTags("Documentos").WithSummary("PDF del pedido de venta (confirmación de pedido), en el idioma del cliente o el de ?idioma=.")
            .RequierePermiso(Permisos.FacturaLeer);
        rutas.MapGet("/compras/pedidos/{id:guid}/pdf", (Guid id, string? idioma, IContextoEmpresa c, ImpresosComerciales i, CancellationToken ct) => Pdf(c, e => i.PedidoCompraAsync(e, id, idioma, ct)))
            .WithTags("Documentos").WithSummary("PDF del pedido de compra para enviar al proveedor, en su idioma o el de ?idioma=.")
            .RequierePermiso(Permisos.CompraLeer);
        rutas.MapGet("/agro/liquidaciones/{id:guid}/pdf", (Guid id, IContextoEmpresa c, ImpresosComerciales i, CancellationToken ct) => Pdf(c, e => i.LiquidacionAsync(e, id, ct)))
            .WithTags("Documentos").WithSummary("PDF de la liquidación al agricultor (autofactura o recibo de compensación REAGP).")
            .RequierePermiso(Permisos.AgroLeer);
        rutas.MapGet("/pagos/liquidaciones/{id:guid}/pdf", (Guid id, IContextoEmpresa c, ImpresosComerciales i, CancellationToken ct) =>
                Pdf(c, e => i.LiquidacionPagosAsync(e, id, ct)))
            .WithTags("Documentos").WithSummary("Impreso de la liquidación de pagos al proveedor o agricultor.")
            .RequierePermiso(Permisos.FacturaLeer);
        rutas.MapGet("/agro/envases/movimientos/{id:guid}/pdf", (Guid id, IContextoEmpresa c, ImpresosComerciales i, CancellationToken ct) =>
                Pdf(c, e => i.JustificanteEnvasesAsync(e, id, ct)))
            .WithTags("Documentos").WithSummary("Justificante del movimiento de envases, para firmar el tercero.")
            .RequierePermiso(Permisos.AgroLeer);
        rutas.MapGet("/agro/envases/cuentas/{id:guid}/extracto/pdf", (Guid id, DateOnly? desde, DateOnly? hasta, IContextoEmpresa c, ImpresosComerciales i, CancellationToken ct) =>
                Pdf(c, e => i.ExtractoEnvasesAsync(e, id, desde, hasta, ct)))
            .WithTags("Documentos").WithSummary("Extracto de envases de la cuenta en PDF.")
            .RequierePermiso(Permisos.AgroLeer);
        rutas.MapGet("/agro/ordenes-carga/{id:guid}/pdf", (Guid id, IContextoEmpresa c, ImpresosComerciales i, CancellationToken ct) => Pdf(c, e => i.HojaCargaAsync(e, id, ct)))
            .WithTags("Documentos").WithSummary("Hoja de carga de la orden: palés por línea con su SSCC y el esquema del camión.")
            .RequierePermiso(Permisos.AgroLeer);
        return rutas;
    }

    private static async Task<IResult> Pdf(IContextoEmpresa contexto, Func<Guid, Task<Resultado<AlxorCore.Documentos.Aplicacion.DocumentoPdf>>> generar)
    {
        if (contexto.EmpresaId is not { } empresa)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var r = await generar(empresa).ConfigureAwait(false);
        return r.EsCorrecto ? Results.File(r.Valor.Contenido, "application/pdf", r.Valor.NombreArchivo) : ResultadosHttp.AProblema(r.Error);
    }
}
