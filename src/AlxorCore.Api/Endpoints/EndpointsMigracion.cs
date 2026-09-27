using AlxorCore.Api.Comun;
using AlxorCore.Migracion.Hispatec;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Tesoreria.Aplicacion;
using AlxorCore.Tesoreria.Dominio;

namespace AlxorCore.Api.Endpoints;

/// <summary>
/// Migración desde Hispatec: se sube el paquete (ZIP de CSV generado por <c>herramientas/hispatec/exportar.ps1</c>),
/// se valida (informe de errores, avisos y cuadres, sin tocar nada) y, si se quiere, se carga. La carga es
/// idempotente: se puede repetir sin duplicar.
/// </summary>
public static class EndpointsMigracion
{
    public sealed record PeticionPaquete(string ContenidoBase64);

    public static IEndpointRouteBuilder MapearMigracion(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var g = rutas.MapGroup("/migracion/hispatec").WithTags("Migración");

        g.MapPost("/validar", (PeticionPaquete p, IContextoEmpresa c, CargaHispatec carga, CancellationToken ct) => EjecutarAsync(p, c, carga, false, ct))
            .WithSummary("Valida el paquete de Hispatec sin cargar nada: filas válidas, errores, avisos y cuadres.")
            .RequierePermiso(Permisos.EmpresaAjustes);
        g.MapPost("/cargar", (PeticionPaquete p, IContextoEmpresa c, CargaHispatec carga, CancellationToken ct) => EjecutarAsync(p, c, carga, true, ct))
            .WithSummary("Carga lo válido del paquete (terceros, artículos, plan de cuentas, apertura, cartera y agro). Se puede repetir.")
            .RequierePermiso(Permisos.EmpresaAjustes);
        g.MapGet("/ejecuciones", async (IContextoEmpresa c, IRepositorioCorrespondencias repo, CancellationToken ct) =>
                c.EmpresaId is { } e ? Results.Ok(await repo.EjecucionesAsync(e, ct).ConfigureAwait(false)) : SinEmpresa())
            .WithSummary("Cargas realizadas en la empresa.")
            .RequierePermiso(Permisos.EmpresaAjustes);

        var cartera = rutas.MapGroup("/cartera").WithTags("Cartera");
        cartera.MapGet("", async (SentidoCartera? sentido, bool? pendientes, IContextoEmpresa c, GestionCartera gestion, CancellationToken ct) =>
                c.EmpresaId is { } e ? Results.Ok(await gestion.ListarAsync(e, sentido, pendientes ?? true, ct).ConfigureAwait(false)) : SinEmpresa())
            .WithSummary("Efectos de cartera sin factura ni gasto en ALXOR (p. ej. la cartera migrada), con su pendiente.")
            .RequierePermiso(Permisos.FacturaLeer);
        cartera.MapPost("", async (CrearEfectoCarteraComando comando, IContextoEmpresa c, GestionCartera gestion, CancellationToken ct) =>
            {
                if (c.EmpresaId is not { } e)
                {
                    return SinEmpresa();
                }

                var r = await gestion.CrearAsync(e, comando, ct).ConfigureAwait(false);
                return r.EsCorrecto ? Results.Created($"/cartera/{r.Valor.Id}", r.Valor) : ResultadosHttp.AProblema(r.Error);
            })
            .WithSummary("Da de alta un efecto de cobro o de pago.")
            .RequierePermiso(Permisos.CobroRegistrar);
        cartera.MapPost("/{id:guid}/anular", async (Guid id, PeticionAnularEfecto? peticion, GestionCartera gestion, CancellationToken ct) =>
                (await gestion.AnularAsync(id, peticion?.Motivo, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Anula un efecto dado de alta por error o saldado fuera de ALXOR (sin cobros ni pagos vivos).").RequierePermiso(Permisos.CobroRegistrar);

        cartera.MapGet("/{id:guid}/saldo", async (Guid id, GestionCartera gestion, CancellationToken ct) =>
                (await gestion.SaldoAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Saldo de un efecto con sus cobros o pagos.").RequierePermiso(Permisos.FacturaLeer);

        cartera.MapPost("/{id:guid}/movimientos", async (Guid id, MovimientoCarteraComando comando, IContextoEmpresa c, GestionCartera gestion, CancellationToken ct) =>
                c.EmpresaId is { } e ? (await gestion.LiquidarAsync(e, id, comando, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Cobra o paga (total o parcialmente) un efecto de cartera.")
            .RequierePermiso(Permisos.CobroRegistrar);
        return rutas;
    }

    private static async Task<IResult> EjecutarAsync(PeticionPaquete p, IContextoEmpresa c, CargaHispatec carga, bool aplicar, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(p);
        if (c.EmpresaId is not { } empresa || c.GrupoId is not { } grupo)
        {
            return SinEmpresa();
        }

        var paquete = Paquete.Leer(p.ContenidoBase64);
        return Results.Ok(await carga.EjecutarAsync(grupo, empresa, paquete, aplicar, ct).ConfigureAwait(false));
    }

    private static IResult SinEmpresa() => ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));

    /// <summary>Motivo de la anulación de un efecto.</summary>
    public sealed record PeticionAnularEfecto(string? Motivo);
}
