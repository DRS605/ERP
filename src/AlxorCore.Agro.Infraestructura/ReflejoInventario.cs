using AlxorCore.Agro.Aplicacion;
using AlxorCore.Agro.Dominio;
using AlxorCore.Catalogo.Aplicacion;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AlxorCore.Agro.Infraestructura;

/// <summary>
/// Unidad de trabajo agro que, al guardar, refleja en el inventario del almacén lo que acaban de mover las partidas y los
/// envases (si la empresa lo tiene activado en la configuración agro):
/// <list type="bullet">
/// <item>partidas de artículos por kilos: la suma de sus movimientos nuevos por artículo (recepción, confección, ajustes,
/// anulaciones y expediciones). La expedición de un palé con albarán, y su vuelta, no cuentan: el stock lo mueve el
/// albarán (y su anulación o su devolución), y lo mismo lo vendido en subasta;</item>
/// <item>envases: lo entregado a un tercero sale del almacén y lo recogido entra (una facturación de envases entra y su
/// albarán sale: no cambia nada, porque los envases ya salieron al entregarse).</item>
/// </list>
/// Se hace después de guardar (el inventario es otro contexto); los avisos no deshacen lo guardado.
/// </summary>
internal sealed class UnidadAgroConInventario : IUnidadDeTrabajoAgro
{
    private readonly AgroDbContext _ctx;
    private readonly IServiceProvider _servicios;

    public UnidadAgroConInventario(AgroDbContext ctx, IServiceProvider servicios)
    {
        _ctx = ctx;
        _servicios = servicios;
    }

    /// <summary>Avisos del último reflejo en el inventario (vacío si todo entró).</summary>
    public IReadOnlyList<string> UltimosAvisos { get; private set; } = [];

    public Task BloquearAsync(string clave, CancellationToken ct = default) => _ctx.BloquearAsync(clave, ct);

    public async Task<int> GuardarCambiosAsync(CancellationToken ct = default)
    {
        var partidas = new List<(Guid Empresa, Guid Partida, decimal Kilos)>();
        var envases = new List<(Guid Empresa, Guid Envase, int Cantidad, string Motivo)>();
        foreach (var e in _ctx.ChangeTracker.Entries<MovimientoPartida>().Where(e => e.State == EntityState.Added))
        {
            var m = e.Entity;
            if (m.DocumentoTipo == PalesAgro.DocumentoExpedicion && m.PaleId is { } paleId && ConAlbaran(paleId, m.Tipo == TipoMovimientoPartida.Anulacion))
            {
                continue;
            }

            // Lo vendido en subasta sale con el albarán de cada comprador (y vuelve al anularlo).
            if (m.DocumentoTipo == SubastasAgro.DocumentoSubasta)
            {
                continue;
            }

            partidas.Add((m.EmpresaId, m.PartidaId, m.Kilos));
        }

        foreach (var e in _ctx.ChangeTracker.Entries<MovimientoEnvases>().Where(e => e.State == EntityState.Added))
        {
            envases.AddRange(e.Entity.Lineas.Select(l => (e.Entity.EmpresaId, l.EnvaseProductoId, l.Cantidad, $"Envases {e.Entity.NumeroCompleto}")));
        }

        var filas = await _ctx.GuardarCambiosAsync(ct).ConfigureAwait(false);
        UltimosAvisos = [];
        if (partidas.Count > 0 || envases.Count > 0)
        {
            UltimosAvisos = await ReflejarAsync(partidas, envases, ct).ConfigureAwait(false);
        }

        return filas;
    }

    /// <summary>Si el palé salió con albarán (en su vuelta, si lo tenía antes de anular la expedición).</summary>
    private bool ConAlbaran(Guid paleId, bool vuelta)
    {
        var entrada = _ctx.ChangeTracker.Entries<Pale>().FirstOrDefault(p => p.Entity.Id == paleId);
        if (entrada is null)
        {
            return false;
        }

        return (vuelta ? entrada.Property(p => p.AlbaranId).OriginalValue : entrada.Entity.AlbaranId) is not null;
    }

    private async Task<IReadOnlyList<string>> ReflejarAsync(List<(Guid Empresa, Guid Partida, decimal Kilos)> partidas,
        List<(Guid Empresa, Guid Envase, int Cantidad, string Motivo)> envases, CancellationToken ct)
    {
        var inventario = _servicios.GetService<IInventarioAgro>();
        if (inventario is null)
        {
            return [];
        }

        var productos = _servicios.GetRequiredService<IConsultaProductos>();
        var avisos = new List<string>();
        foreach (var empresa in partidas.Select(p => p.Empresa).Concat(envases.Select(e => e.Empresa)).Distinct())
        {
            var config = await _ctx.Set<ConfiguracionAgro>().AsNoTracking().FirstOrDefaultAsync(c => c.EmpresaId == empresa, ct).ConfigureAwait(false);
            if (config is null || (!config.ReflejarPartidasEnInventario && !config.ReflejarEnvasesEnInventario))
            {
                continue;
            }

            var movimientos = new List<MovimientoInventarioAgro>();
            if (config.ReflejarPartidasEnInventario)
            {
                var suyas = partidas.Where(p => p.Empresa == empresa).ToList();
                var ids = suyas.Select(p => p.Partida).Distinct().ToList();
                var producto = await _ctx.Set<Partida>().AsNoTracking().Where(p => ids.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.ProductoId, ct).ConfigureAwait(false);
                foreach (var g in suyas.GroupBy(p => producto.GetValueOrDefault(p.Partida)))
                {
                    var kilos = g.Sum(p => p.Kilos);
                    if (g.Key == Guid.Empty || kilos == 0m || !EsPorKilos((await productos.ObtenerAsync(g.Key, ct).ConfigureAwait(false))?.Unidad))
                    {
                        continue;
                    }

                    movimientos.Add(new MovimientoInventarioAgro(g.Key, kilos, "Partidas agro"));
                }
            }

            if (config.ReflejarEnvasesEnInventario)
            {
                movimientos.AddRange(envases.Where(e => e.Empresa == empresa).GroupBy(e => (e.Envase, e.Motivo))
                    .Select(g => new MovimientoInventarioAgro(g.Key.Envase, -g.Sum(e => e.Cantidad), g.Key.Motivo)).Where(m => m.Cantidad != 0m));
            }

            if (movimientos.Count > 0)
            {
                avisos.AddRange(await inventario.MoverAsync(empresa, movimientos, ct).ConfigureAwait(false));
            }
        }

        return avisos;
    }

    internal static bool EsPorKilos(string? unidad) =>
        (unidad ?? string.Empty).Trim().ToLowerInvariant() is "kg" or "kilo" or "kilos" or "kilogramo" or "kilogramos";
}
