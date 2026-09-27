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

        a.MapGet("/intrastat", async (int? anio, int? mes, FlujoIntrastat? flujo, string? formato, IContextoEmpresa c, Intrastat caso, CancellationToken ct) =>
            {
                if (c.EmpresaId is not { } e)
                {
                    return SinEmpresa();
                }

                var hoy = DateTime.UtcNow.AddMonths(-1);
                var m = mes ?? hoy.Month;
                if (m is < 1 or > 12)
                {
                    return ResultadosHttp.AProblema(Error.Validacion("intrastat.mes", "El mes va de 1 a 12."));
                }

                var d = await caso.DeclaracionAsync(e, anio ?? hoy.Year, m, flujo ?? FlujoIntrastat.Expedicion, ct).ConfigureAwait(false);
                return string.Equals(formato, "csv", StringComparison.OrdinalIgnoreCase)
                    ? Results.File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(Intrastat.Csv(d))).ToArray(), "text/csv; charset=utf-8",
                        $"intrastat-{d.Flujo.ToLowerInvariant()}-{d.Anio}-{d.Mes:D2}.csv")
                    : Results.Ok(d);
            })
            .WithSummary("Intrastat del mes: expediciones (facturas intracomunitarias) o introducciones (albaranes de compra de la UE); con ?formato=csv, el fichero.")
            .RequierePermiso(Permisos.FacturaLeer);

        a.MapGet("/importaciones", async (IContextoEmpresa c, AlxorCore.Gastos.Aplicacion.Importaciones caso, CancellationToken ct) =>
                c.EmpresaId is { } e ? Results.Ok(await caso.ListarAsync(e, ct).ConfigureAwait(false)) : SinEmpresa())
            .WithSummary("Compras de fuera de la UE (o con IVA de importación) y sus DUA de importación.").RequierePermiso(Permisos.FacturaLeer);
        a.MapPost("/gastos/{id:guid}/duas", async (Guid id, AlxorCore.Gastos.Aplicacion.DatosDuaImportacion d, IContextoEmpresa c, AlxorCore.Gastos.Aplicacion.Importaciones caso, CancellationToken ct) =>
                c.EmpresaId is { } e ? (await caso.RegistrarAsync(e, id, d, ct).ConfigureAwait(false)).ACreado("/aduanas/importaciones") : SinEmpresa())
            .WithSummary("Registra el DUA de importación de una factura de proveedor (MRN, admisión, base, aranceles e IVA a la importación).")
            .RequierePermiso(Permisos.GastoGestionar);
        a.MapPut("/duas-importacion/{id:guid}", async (Guid id, AlxorCore.Gastos.Aplicacion.DatosDuaImportacion d, IContextoEmpresa c, AlxorCore.Gastos.Aplicacion.Importaciones caso, CancellationToken ct) =>
                c.EmpresaId is { } e ? (await caso.ModificarAsync(e, id, d, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Corrige un DUA de importación.").RequierePermiso(Permisos.GastoGestionar);
        a.MapDelete("/duas-importacion/{id:guid}", async (Guid id, IContextoEmpresa c, AlxorCore.Gastos.Aplicacion.Importaciones caso, CancellationToken ct) =>
                c.EmpresaId is { } e ? (await caso.EliminarAsync(e, id, ct).ConfigureAwait(false)).ASinContenido() : SinEmpresa())
            .WithSummary("Elimina un DUA de importación registrado por error.").RequierePermiso(Permisos.GastoGestionar);

        rutas.MapGet("/cartas-porte/{id:guid}/certificados", async (Guid id, CertificadosFitosanitarios caso, CancellationToken ct) =>
                Results.Ok(await caso.DeCartaAsync(id, ct).ConfigureAwait(false)))
            .WithTags("Cartas de porte").WithSummary("Certificados fitosanitarios de la expedición.").RequierePermiso(Permisos.FacturaLeer);
        rutas.MapPost("/cartas-porte/{id:guid}/certificados", async (Guid id, DatosCertificadoFitosanitario d, IContextoEmpresa c, CertificadosFitosanitarios caso, CancellationToken ct) =>
                c.EmpresaId is { } e ? (await caso.RegistrarAsync(e, id, d, ct).ConfigureAwait(false)).ACreado($"/cartas-porte/{id}/certificados") : SinEmpresa())
            .WithTags("Cartas de porte").WithSummary("Registra un certificado fitosanitario (con su documento escaneado, en base64).").RequierePermiso(Permisos.FacturaCrear);
        var cf = rutas.MapGroup("/certificados-fitosanitarios").WithTags("Cartas de porte");
        cf.MapPut("/{id:guid}", async (Guid id, DatosCertificadoFitosanitario d, IContextoEmpresa c, CertificadosFitosanitarios caso, CancellationToken ct) =>
                c.EmpresaId is { } e ? (await caso.ModificarAsync(e, id, d, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Corrige un certificado fitosanitario.").RequierePermiso(Permisos.FacturaCrear);
        cf.MapDelete("/{id:guid}", async (Guid id, IContextoEmpresa c, CertificadosFitosanitarios caso, CancellationToken ct) =>
                c.EmpresaId is { } e ? (await caso.EliminarAsync(e, id, ct).ConfigureAwait(false)).ASinContenido() : SinEmpresa())
            .WithSummary("Elimina un certificado fitosanitario.").RequierePermiso(Permisos.FacturaCrear);
        cf.MapGet("/{id:guid}/documento", async (Guid id, IContextoEmpresa c, CertificadosFitosanitarios caso, CancellationToken ct) =>
            {
                if (c.EmpresaId is not { } e)
                {
                    return SinEmpresa();
                }

                var r = await caso.DocumentoAsync(e, id, ct).ConfigureAwait(false);
                return r.EsFallo ? ResultadosHttp.AProblema(r.Error) : Results.File(r.Valor.Contenido, r.Valor.Tipo, r.Valor.Nombre);
            })
            .WithSummary("Descarga el documento escaneado del certificado.").RequierePermiso(Permisos.FacturaLeer);

        return rutas;
    }

    private static IResult SinEmpresa() => ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
}
