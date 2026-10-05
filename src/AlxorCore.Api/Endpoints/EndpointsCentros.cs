using AlxorCore.Api.Comun;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Organizacion.Aplicacion.CasosDeUso;
using Microsoft.EntityFrameworkCore;

namespace AlxorCore.Api.Endpoints;

/// <summary>
/// Centros de trabajo de la empresa (tiendas, delegaciones, almacenes, plantas): sus cajas, su almacén habitual, los
/// usuarios que trabajan con cada uno y el cierre de cada caja. Sus series son asignaciones de ámbito Centro o Caja en
/// <c>/series/asignaciones</c>.
/// </summary>
public static class EndpointsCentros
{
    public sealed record PeticionAccesos(IReadOnlyList<Guid>? Centros);

    public sealed record CentroUsuarioDto(Guid Id, string Codigo, string Nombre, Guid? AlmacenId, IReadOnlyList<CajaDto> Cajas);

    public sealed record MisCentrosDto(bool Limitado, IReadOnlyList<CentroUsuarioDto> Centros);

    public sealed record CierreCajaCentroDto(DateOnly Dia, Guid CentroId, Guid CajaId, string Caja, int Tickets, int Anulados, decimal Base, decimal Impuestos, decimal Total,
        decimal Cobrado, decimal Pendiente, string? PrimerTicket, string? UltimoTicket);

    public static IEndpointRouteBuilder MapearCentros(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var g = rutas.MapGroup("/centros").WithTags("Centros");

        g.MapGet("", (IContextoEmpresa c, GestionCentros s, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await s.ListarAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Centros de la empresa con sus cajas y cuántos usuarios tienen asignados.").RequireAuthorization();
        g.MapGet("/mios", (IContextoEmpresa c, GestionCentros s, CentrosUsuario centros, CancellationToken ct) =>
                ConEmpresa(c, async e =>
                {
                    var permitidos = await centros.PermitidosAsync(ct).ConfigureAwait(false);
                    var lista = (await s.ListarAsync(e, ct).ConfigureAwait(false)).Where(x => x.Activo && (permitidos is null || permitidos.Contains(x.Id)))
                        .Select(x => new CentroUsuarioDto(x.Id, x.Codigo, x.Nombre, x.AlmacenId, x.Cajas.Where(k => k.Activa).ToList())).ToList();
                    return Results.Ok(new MisCentrosDto(permitidos is not null, lista));
                }))
            .WithSummary("Centros activos con que trabaja el usuario (todos, si no está limitado) y sus cajas activas.").RequireAuthorization();
        g.MapPost("", (DatosCentro d, IContextoEmpresa c, GestionCentros s, CancellationToken ct) =>
                ConEmpresa(c, async e =>
                {
                    var r = await s.CrearAsync(e, d, ct).ConfigureAwait(false);
                    return r.EsCorrecto ? Results.Created($"/centros/{r.Valor.Id}", r.Valor) : ResultadosHttp.AProblema(r.Error);
                }))
            .WithSummary("Crea un centro: código, nombre, dirección y almacén habitual.").RequierePermiso(Permisos.EmpresaAjustes);
        g.MapPut("/{id:guid}", (Guid id, DatosCentro d, IContextoEmpresa c, GestionCentros s, CancellationToken ct) =>
                ConEmpresa(c, async e => (await s.CambiarAsync(e, id, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Cambia nombre, dirección, almacén habitual o si está activo (un centro dado de baja no admite documentos nuevos).")
            .RequierePermiso(Permisos.EmpresaAjustes);
        g.MapDelete("/{id:guid}", async (Guid id, GestionCentros s, AlxorCore.Nucleo.Aplicacion.IComprobadorUso uso, CancellationToken ct) =>
                (await s.EliminarAsync(id, uso, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Borra un centro sin documentos; con documentos, se da de baja (PUT con Activo = false).").RequierePermiso(Permisos.EmpresaAjustes);
        g.MapPost("/{id:guid}/cajas", (Guid id, DatosCaja d, IContextoEmpresa c, GestionCentros s, CancellationToken ct) =>
                ConEmpresa(c, async e => (await s.AgregarCajaAsync(e, id, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Añade una caja (punto de venta) al centro.").RequierePermiso(Permisos.EmpresaAjustes);
        g.MapPut("/{id:guid}/cajas/{cajaId:guid}", (Guid id, Guid cajaId, DatosCaja d, IContextoEmpresa c, GestionCentros s, CancellationToken ct) =>
                ConEmpresa(c, async e => (await s.CambiarCajaAsync(e, id, cajaId, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Cambia el nombre de una caja o la da de baja.").RequierePermiso(Permisos.EmpresaAjustes);

        g.MapGet("/accesos", (IContextoEmpresa c, GestionCentros s, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await s.AccesosAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Usuarios limitados a unos centros (los que no salen trabajan con todos).").RequierePermiso(Permisos.UsuarioGestionar);
        g.MapPut("/accesos/{usuarioId:guid}", (Guid usuarioId, PeticionAccesos p, IContextoEmpresa c, GestionCentros s, CancellationToken ct) =>
                ConEmpresa(c, async e => (await s.FijarAccesosAsync(e, usuarioId, p.Centros, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Fija los centros con que trabaja un usuario (lista vacía: todos).").RequierePermiso(Permisos.UsuarioGestionar);

        g.MapGet("/{id:guid}/cajas/{cajaId:guid}/cierre", async (Guid id, Guid cajaId, DateOnly? dia, IContextoEmpresa c, CentrosUsuario centros,
                AlxorCore.Nucleo.Aplicacion.IConsultaCentros consulta, AlxorCore.Facturacion.Infraestructura.FacturacionDbContext facturacion,
                AlxorCore.Tesoreria.Aplicacion.IConsultaTesoreria tesoreria, AlxorCore.Nucleo.Tiempo.IReloj reloj, CancellationToken ct) =>
            {
                if (!await centros.PuedeVerAsync(id, ct).ConfigureAwait(false) || await consulta.ObtenerAsync(id, ct).ConfigureAwait(false) is not { } centro
                    || centro.Cajas.FirstOrDefault(k => k.Id == cajaId) is not { } caja)
                {
                    return ResultadosHttp.AProblema(Error.NoEncontrado("centro.caja_no_encontrada", "La caja no existe."));
                }

                var d = dia ?? DateOnly.FromDateTime(reloj.AhoraUtc.UtcDateTime);
                var tickets = await facturacion.Facturas.AsNoTracking().Where(f => f.CajaId == cajaId && f.FechaEmision == d)
                    .Select(f => new { f.Id, f.NumeroCompleto, f.Numero, f.Estado, f.BaseImponible, f.CuotaIva, f.Total }).ToListAsync(ct).ConfigureAwait(false);
                var vivos = tickets.Where(t => t.Estado != AlxorCore.Facturacion.Dominio.EstadoFactura.Anulada).ToList();
                var cobrado = vivos.Count == 0 ? 0m
                    : (await tesoreria.LiquidadoPorDocumentosAsync(AlxorCore.Tesoreria.Dominio.TipoDocumentoTesoreria.Factura, vivos.Select(t => t.Id).ToList(), ct).ConfigureAwait(false))
                        .Values.Sum();
                var total = Redondeo.Dos(vivos.Sum(t => t.Total));
                var orden = tickets.OrderBy(t => t.Numero).ToList();
                return Results.Ok(new CierreCajaCentroDto(d, id, cajaId, $"{centro.Codigo} · {caja.Nombre}", vivos.Count, tickets.Count - vivos.Count,
                    Redondeo.Dos(vivos.Sum(t => t.BaseImponible)), Redondeo.Dos(vivos.Sum(t => t.CuotaIva)), total, Redondeo.Dos(cobrado), Redondeo.Dos(total - cobrado),
                    orden.FirstOrDefault()?.NumeroCompleto, orden.LastOrDefault()?.NumeroCompleto));
            })
            .WithSummary("Cierre de una caja en un día: tickets, base, impuestos, total, cobrado y pendiente, primer y último ticket.")
            .RequierePermiso(Permisos.InformeLeer);
        return rutas;
    }

    private static async Task<IResult> ConEmpresa(IContextoEmpresa contexto, Func<Guid, Task<IResult>> accion) =>
        contexto.EmpresaId is { } empresa
            ? await accion(empresa).ConfigureAwait(false)
            : ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
}
