using AlxorCore.Api.Comun;
using AlxorCore.Contabilidad.Aplicacion;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Api.Endpoints;

/// <summary>Préstamos, leasing y pólizas de crédito (submódulo de Contabilidad).</summary>
public static class EndpointsFinanciacion
{
    public sealed record PeticionFecha(DateOnly Fecha);

    public sealed record PeticionCuotas(DateOnly Hasta, Guid? OperacionId = null);

    public sealed record PeticionEjercicio(int Ejercicio);

    public sealed record PeticionRevision(DateOnly Fecha, decimal TipoInteres);

    public sealed record PeticionAnticipo(DateOnly Fecha, decimal Importe, decimal Comision = 0m);

    public sealed record PeticionMovimiento(DateOnly Fecha, decimal Importe);

    public sealed record PeticionDeshacer(Guid EventoId, DateOnly? Fecha = null);

    public sealed record PeticionDatos(string? Descripcion, string? Entidad);

    public sealed record PeticionCierre(bool Cerrar = true);

    public static IEndpointRouteBuilder MapearFinanciacion(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var g = rutas.MapGroup("/contabilidad/financiacion").WithTags("Financiación");

        g.MapGet("/", async (IContextoEmpresa contexto, GestionFinanciacion caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? Results.Ok(await caso.ListarAsync(e, ct).ConfigureAwait(false)) : SinEmpresa())
            .WithSummary("Préstamos, leasing y pólizas de crédito con su capital pendiente a corto y largo plazo.")
            .RequierePermiso(Permisos.ContabilidadLeer);

        g.MapGet("/vencimientos", async (DateOnly? hasta, IContextoEmpresa contexto, GestionFinanciacion caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? Results.Ok(await caso.VencimientosAsync(e, hasta, ct).ConfigureAwait(false)) : SinEmpresa())
            .WithSummary("Cuotas pendientes y vencimientos de pólizas (para la previsión de tesorería).")
            .RequierePermiso(Permisos.ContabilidadLeer);

        g.MapGet("/{id:guid}", async (Guid id, IContextoEmpresa contexto, GestionFinanciacion caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.DetalleAsync(e, id, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Detalle: cuadro de amortización, hechos y, en la póliza, el dispuesto por tramos.")
            .RequierePermiso(Permisos.ContabilidadLeer);

        g.MapPost("/", async (DatosOperacionFinanciacion datos, IContextoEmpresa contexto, GestionFinanciacion caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.CrearAsync(e, datos, ct).ConfigureAwait(false)).ACreado("/contabilidad/financiacion") : SinEmpresa())
            .WithSummary("Da de alta un préstamo, leasing o póliza (con el asiento de formalización, si se pide).")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        g.MapPut("/{id:guid}", async (Guid id, PeticionDatos p, IContextoEmpresa contexto, GestionFinanciacion caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.RenombrarAsync(e, id, p.Descripcion, p.Entidad, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Cambia la descripción y la entidad.")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        g.MapDelete("/{id:guid}", async (Guid id, IContextoEmpresa contexto, GestionFinanciacion caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.EliminarAsync(e, id, ct).ConfigureAwait(false)).ASinContenido() : SinEmpresa())
            .WithSummary("Elimina una operación sin más hechos que la formalización (anula su asiento).")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        g.MapPost("/cuotas", async (PeticionCuotas p, IContextoEmpresa contexto, GestionFinanciacion caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.ContabilizarCuotasAsync(e, p.Hasta, p.OperacionId, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Contabiliza las cuotas vencidas hasta una fecha (capital, intereses e IVA del leasing contra el banco).")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        g.MapPost("/reclasificar", async (PeticionEjercicio p, IContextoEmpresa contexto, GestionFinanciacion caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.ReclasificarAsync(e, p.Ejercicio, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Cierre: pasa a corto plazo el capital que vence el año siguiente (asiento a 31/12).")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        g.MapPost("/{id:guid}/revision", async (Guid id, PeticionRevision p, IContextoEmpresa contexto, GestionFinanciacion caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.RevisarTipoAsync(e, id, p.Fecha, p.TipoInteres, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Revisión del tipo de interés: recalcula la cuota de lo que queda.")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        g.MapPost("/{id:guid}/anticipo", async (Guid id, PeticionAnticipo p, IContextoEmpresa contexto, GestionFinanciacion caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.AmortizarAnticipadamenteAsync(e, id, p.Fecha, p.Importe, p.Comision, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Amortización o cancelación anticipada (mantiene el plazo y reduce la cuota).")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        g.MapPost("/{id:guid}/disposicion", async (Guid id, PeticionMovimiento p, IContextoEmpresa contexto, GestionFinanciacion caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.MoverPolizaAsync(e, id, true, p.Fecha, p.Importe, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Póliza: disposición (hasta el límite).")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        g.MapPost("/{id:guid}/reintegro", async (Guid id, PeticionMovimiento p, IContextoEmpresa contexto, GestionFinanciacion caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.MoverPolizaAsync(e, id, false, p.Fecha, p.Importe, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Póliza: reintegro de lo dispuesto.")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        g.MapPost("/{id:guid}/liquidacion", async (Guid id, PeticionFecha p, IContextoEmpresa contexto, GestionFinanciacion caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.LiquidarPolizaAsync(e, id, p.Fecha, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Póliza: liquida intereses y comisión de no disponibilidad hasta la fecha.")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        g.MapPost("/{id:guid}/cierre", async (Guid id, PeticionCierre p, IContextoEmpresa contexto, GestionFinanciacion caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.CerrarPolizaAsync(e, id, p.Cerrar, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Póliza: la cierra (sin nada dispuesto) o la vuelve a abrir.")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        g.MapPost("/{id:guid}/deshacer", async (Guid id, PeticionDeshacer p, IContextoEmpresa contexto, GestionFinanciacion caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.DeshacerAsync(e, id, p.EventoId, p.Fecha, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Deshace el último hecho (cuota, revisión, anticipo, traspaso, movimiento o liquidación) con su contraasiento.")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        return rutas;
    }

    private static IResult SinEmpresa() =>
        ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
}
