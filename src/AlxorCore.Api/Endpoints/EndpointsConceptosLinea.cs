using AlxorCore.Api.Comun;
using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Api.Endpoints;

/// <summary>
/// Conceptos de línea: recargos, bonificaciones y costes que se ponen en las líneas de los documentos de venta y
/// compra (maestro compartido por el grupo), los que se pondrían solos en una línea y el informe del periodo.
/// </summary>
public static class EndpointsConceptosLinea
{
    public static IEndpointRouteBuilder MapearConceptosLinea(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var g = rutas.MapGroup("/conceptos-linea").WithTags("Conceptos de línea");

        g.MapGet("", async (AmbitoConcepto? ambito, bool? activos, GestionConceptosLinea caso, CancellationToken ct) =>
                Results.Ok(await caso.ListarAsync(ambito, activos ?? false, ct).ConfigureAwait(false)))
            .WithSummary("Conceptos de línea del grupo (con ?ambito=Ventas|Compras, los que valen ahí; con ?activos=true, solo los activos).")
            .RequireAuthorization();

        g.MapGet("/{id:guid}", async (Guid id, GestionConceptosLinea caso, CancellationToken ct) => (await caso.ObtenerAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Detalle de un concepto de línea con sus asignaciones automáticas.")
            .RequireAuthorization();

        g.MapPost("", async (CrearConceptoComando comando, IContextoEmpresa contexto, GestionConceptosLinea caso, CancellationToken ct) =>
            {
                if (contexto.GrupoId is not { } grupo)
                {
                    return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
                }

                var r = await caso.CrearAsync(grupo, comando, ct).ConfigureAwait(false);
                return r.EsCorrecto ? r.ACreado($"/conceptos-linea/{r.Valor.Id}") : ResultadosHttp.AProblema(r.Error);
            })
            .WithSummary("Crea un concepto de línea.")
            .RequierePermiso(Permisos.ProductoGestionar);

        g.MapPut("/{id:guid}", async (Guid id, ActualizarConceptoComando comando, GestionConceptosLinea caso, CancellationToken ct) =>
                (await caso.ActualizarAsync(id, comando, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Modifica un concepto de línea (sustituye sus asignaciones). Los documentos ya hechos guardan su copia y no cambian.")
            .RequierePermiso(Permisos.ProductoGestionar);

        g.MapDelete("/{id:guid}", async (Guid id, GestionConceptosLinea caso, CancellationToken ct) =>
                (await caso.EliminarAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Elimina un concepto que ningún documento usa; si ya se usó, lo da de baja.")
            .RequierePermiso(Permisos.ProductoGestionar);

        g.MapGet("/sugeridos", async (AmbitoConcepto? ambito, Guid? terceroId, Guid? productoId, DateOnly? fecha, IResolverConceptos caso,
                AlxorCore.Terceros.Aplicacion.IConsultaClientes clientes, AlxorCore.Terceros.Aplicacion.IConsultaProveedores proveedores, CancellationToken ct) =>
            {
                // El tipo del cliente o proveedor decide las reglas por tipo de tercero.
                var a = ambito ?? AmbitoConcepto.Ventas;
                var tipo = terceroId is not { } t ? null
                    : a == AmbitoConcepto.Compras ? (await proveedores.ObtenerAsync(t, ct).ConfigureAwait(false))?.Tipo
                    : (await clientes.ObtenerAsync(t, ct).ConfigureAwait(false))?.Tipo;
                return Results.Ok(await caso.SugeridosAsync(a, terceroId, productoId, new ContextoConceptos(tipo, fecha ?? DateOnly.FromDateTime(DateTime.Today)), ct)
                    .ConfigureAwait(false));
            })
            .WithSummary("Conceptos que se pondrían solos en una línea para ese cliente o proveedor y ese artículo.")
            .RequireAuthorization();

        g.MapGet("/informe", async (DateOnly? desde, DateOnly? hasta, IContextoEmpresa contexto, InformeConceptosLinea informe, IReloj reloj, CancellationToken ct) =>
            {
                if (contexto.EmpresaId is not { } empresa)
                {
                    return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
                }

                var hoy = DateOnly.FromDateTime(reloj.AhoraUtc.UtcDateTime);
                return Results.Ok(await informe.GenerarAsync(empresa, desde ?? new DateOnly(hoy.Year, 1, 1), hasta ?? hoy, ct).ConfigureAwait(false));
            })
            .WithSummary("Importe de cada concepto en las facturas de venta y los pedidos de compra del periodo, separando precio y coste.")
            .RequireAuthorization();

        return rutas;
    }
}
