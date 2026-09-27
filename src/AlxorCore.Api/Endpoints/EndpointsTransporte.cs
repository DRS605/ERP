using System.Text;
using AlxorCore.Api.Comun;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Api.Endpoints;

/// <summary>Transportistas y vehículos, y aduanas de exportación (DUA/MRN de las facturas y datos para el agente).</summary>
public static class EndpointsTransporte
{
    public static IEndpointRouteBuilder MapearTransporteYAduanas(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var t = rutas.MapGroup("/transporte").WithTags("Transporte");

        t.MapGet("/incoterms", () => Results.Ok(Incoterms.Todos.Select(i => new { Codigo = i.Key, Nombre = i.Value, Maritimo = Incoterms.EsMaritimo(i.Key) })))
            .WithSummary("Incoterms 2020.").RequierePermiso(Permisos.FacturaLeer);

        t.MapGet("/transportistas", async (IContextoEmpresa c, GestionTransporte caso, CancellationToken ct) =>
                c.EmpresaId is { } e ? Results.Ok(await caso.TransportistasAsync(e, ct).ConfigureAwait(false)) : SinEmpresa())
            .WithSummary("Transportistas habituales.").RequierePermiso(Permisos.FacturaLeer);
        t.MapPost("/transportistas", async (DatosTransportista d, IContextoEmpresa c, GestionTransporte caso, CancellationToken ct) =>
                c.EmpresaId is { } e ? (await caso.CrearTransportistaAsync(e, d, ct).ConfigureAwait(false)).ACreado("/transporte/transportistas") : SinEmpresa())
            .WithSummary("Da de alta un transportista.").RequierePermiso(Permisos.FacturaCrear);
        t.MapPut("/transportistas/{id:guid}", async (Guid id, DatosTransportista d, IContextoEmpresa c, GestionTransporte caso, CancellationToken ct) =>
                c.EmpresaId is { } e ? (await caso.ModificarTransportistaAsync(e, id, d, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Modifica (o da de baja) un transportista.").RequierePermiso(Permisos.FacturaCrear);
        t.MapDelete("/transportistas/{id:guid}", async (Guid id, IContextoEmpresa c, GestionTransporte caso, CancellationToken ct) =>
                c.EmpresaId is { } e ? (await caso.EliminarAsync(e, id, null, ct).ConfigureAwait(false)).ASinContenido() : SinEmpresa())
            .WithSummary("Elimina un transportista sin cartas de porte ni vehículos.").RequierePermiso(Permisos.FacturaCrear);

        t.MapGet("/vehiculos", async (IContextoEmpresa c, GestionTransporte caso, CancellationToken ct) =>
                c.EmpresaId is { } e ? Results.Ok(await caso.VehiculosAsync(e, ct).ConfigureAwait(false)) : SinEmpresa())
            .WithSummary("Vehículos (matrícula, remolque, tara, frigorífico).").RequierePermiso(Permisos.FacturaLeer);
        t.MapPost("/vehiculos", async (DatosVehiculo d, IContextoEmpresa c, GestionTransporte caso, CancellationToken ct) =>
                c.EmpresaId is { } e ? (await caso.CrearVehiculoAsync(e, d, ct).ConfigureAwait(false)).ACreado("/transporte/vehiculos") : SinEmpresa())
            .WithSummary("Da de alta un vehículo.").RequierePermiso(Permisos.FacturaCrear);
        t.MapPut("/vehiculos/{id:guid}", async (Guid id, DatosVehiculo d, IContextoEmpresa c, GestionTransporte caso, CancellationToken ct) =>
                c.EmpresaId is { } e ? (await caso.ModificarVehiculoAsync(e, id, d, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Modifica (o da de baja) un vehículo.").RequierePermiso(Permisos.FacturaCrear);
        t.MapDelete("/vehiculos/{id:guid}", async (Guid id, IContextoEmpresa c, GestionTransporte caso, CancellationToken ct) =>
                c.EmpresaId is { } e ? (await caso.EliminarAsync(e, null, id, ct).ConfigureAwait(false)).ASinContenido() : SinEmpresa())
            .WithSummary("Elimina un vehículo sin cartas de porte.").RequierePermiso(Permisos.FacturaCrear);

        var a = rutas.MapGroup("/aduanas").WithTags("Aduanas");
        a.MapGet("/exportaciones", async (IContextoEmpresa c, Aduanas caso, CancellationToken ct) =>
                c.EmpresaId is { } e ? Results.Ok(await caso.ExportacionesAsync(e, ct).ConfigureAwait(false)) : SinEmpresa())
            .WithSummary("Facturas exentas por exportación y su situación aduanera (sin DUA, despachada, salida confirmada).").RequierePermiso(Permisos.FacturaLeer);
        a.MapGet("/facturas/{id:guid}", async (Guid id, string? formato, IContextoEmpresa c, Aduanas caso, CancellationToken ct) =>
            {
                if (c.EmpresaId is not { } e)
                {
                    return SinEmpresa();
                }

                var r = await caso.DatosAsync(e, id, ct).ConfigureAwait(false);
                if (r.EsFallo)
                {
                    return ResultadosHttp.AProblema(r.Error);
                }

                return string.Equals(formato, "csv", StringComparison.OrdinalIgnoreCase)
                    ? Results.File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(Aduanas.Csv(r.Valor))).ToArray(), "text/csv; charset=utf-8",
                        $"aduana-{r.Valor.Factura.Replace('/', '-')}.csv")
                    : Results.Ok(r.Valor);
            })
            .WithSummary("Datos de la factura para el DUA de exportación (con ?formato=csv, las partidas en CSV para el agente de aduanas).")
            .RequierePermiso(Permisos.FacturaLeer);
        a.MapGet("/facturas/{id:guid}/despachos", async (Guid id, Aduanas caso, CancellationToken ct) => Results.Ok(await caso.DespachosAsync(id, ct).ConfigureAwait(false)))
            .WithSummary("DUA/MRN registrados de una factura.").RequierePermiso(Permisos.FacturaLeer);
        a.MapPost("/facturas/{id:guid}/despachos", async (Guid id, DatosDespacho d, IContextoEmpresa c, Aduanas caso, CancellationToken ct) =>
                c.EmpresaId is { } e ? (await caso.RegistrarAsync(e, id, d, ct).ConfigureAwait(false)).ACreado($"/aduanas/facturas/{id}/despachos") : SinEmpresa())
            .WithSummary("Registra el DUA de exportación (MRN, fecha de despacho y, si se conoce, de salida).").RequierePermiso(Permisos.FacturaCrear);
        a.MapPut("/despachos/{id:guid}", async (Guid id, DatosDespacho d, IContextoEmpresa c, Aduanas caso, CancellationToken ct) =>
                c.EmpresaId is { } e ? (await caso.ModificarAsync(e, id, d, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Corrige un despacho (p. ej. anota la fecha de salida).").RequierePermiso(Permisos.FacturaCrear);
        a.MapDelete("/despachos/{id:guid}", async (Guid id, IContextoEmpresa c, Aduanas caso, CancellationToken ct) =>
                c.EmpresaId is { } e ? (await caso.EliminarAsync(e, id, ct).ConfigureAwait(false)).ASinContenido() : SinEmpresa())
            .WithSummary("Elimina un despacho registrado por error.").RequierePermiso(Permisos.FacturaCrear);

        return rutas;
    }

    private static IResult SinEmpresa() => ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
}
