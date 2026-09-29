using AlxorCore.Agro.Dominio;
using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Agro.Aplicacion;

public sealed record DatosDefecto(string Nombre, bool DescuentaPeso = false, decimal? ToleranciaPct = null, decimal? MaximoPct = null);

public sealed record DatosPlantillaCalidad(string? Codigo, string? Nombre, IReadOnlyList<DatosDefecto> Defectos, Guid? ProductoId = null, Guid? FamiliaId = null, bool Activa = true);

public sealed record DefectoDto(Guid Id, string Nombre, int Orden, bool DescuentaPeso, decimal? ToleranciaPct, decimal? MaximoPct);

public sealed record PlantillaCalidadDto(Guid Id, string Codigo, string Nombre, Guid? ProductoId, Guid? FamiliaId, bool Activa, IReadOnlyList<DefectoDto> Defectos);

public sealed record DatosResultadoMuestreo(Guid DefectoId, decimal Kilos);

public sealed record DatosMuestreo(Guid PlantillaId, decimal PesoMuestraKg, IReadOnlyList<DatosResultadoMuestreo> Resultados, bool Definitivo = false, DateOnly? Fecha = null,
    string? Observaciones = null);

public sealed record ResultadoMuestreoDto(Guid DefectoId, string Defecto, decimal Kilos, decimal Porcentaje, bool DescuentaPeso, bool SuperaTolerancia, bool SuperaMaximo);

public sealed record MuestreoDto(Guid Id, Guid RecepcionId, Guid LineaRecepcionId, Guid PlantillaId, string? Plantilla, DateOnly Fecha, decimal PesoMuestraKg, bool Definitivo,
    decimal DescuentoPct, decimal? KilosALiquidar, bool Anulado, string? MotivoAnulacion, string? Observaciones, Guid? UsuarioId, IReadOnlyList<ResultadoMuestreoDto> Resultados);

/// <summary>
/// Control de calidad en la recepción: plantillas de defectos (por producto, familia o generales) y muestreos de cada
/// línea. El muestreo definitivo descuenta del peso a liquidar al agricultor los defectos que descuentan (podrido,
/// tierra…); el neto real de la partida y de la traza no cambia.
/// </summary>
public sealed class CalidadAgro
{
    private readonly IRepositorioAgro _repo;
    private readonly IUnidadDeTrabajoAgro _unidad;
    private readonly IConsultaProductos _productos;
    private readonly IReloj _reloj;

    public CalidadAgro(IRepositorioAgro repo, IUnidadDeTrabajoAgro unidad, IConsultaProductos productos, IReloj reloj)
    {
        _repo = repo;
        _unidad = unidad;
        _productos = productos;
        _reloj = reloj;
    }

    public async Task<IReadOnlyList<PlantillaCalidadDto>> PlantillasAsync(Guid empresaId, Guid? productoId, CancellationToken ct = default)
    {
        var lista = await _repo.PlantillasCalidadAsync(empresaId, ct).ConfigureAwait(false);
        if (productoId is { } pid)
        {
            // Las que aplican al producto, la más específica primero (la del producto, la de su familia, la general).
            var familia = (await _productos.ObtenerAsync(pid, ct).ConfigureAwait(false))?.FamiliaId;
            lista = lista.Where(p => p.Activa && p.Prioridad(pid, familia) > 0).OrderByDescending(p => p.Prioridad(pid, familia)).ToList();
        }

        return lista.Select(Dto).ToList();
    }

    public async Task<Resultado<PlantillaCalidadDto>> CrearPlantillaAsync(Guid empresaId, DatosPlantillaCalidad d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        if ((await _repo.PlantillasCalidadAsync(empresaId, ct).ConfigureAwait(false)).Any(p => string.Equals(p.Codigo, d.Codigo?.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            return Resultado.Fallo<PlantillaCalidadDto>(Error.Conflicto("calidad.codigo_repetido", "Ya hay una plantilla con ese código."));
        }

        var p = PlantillaCalidad.Crear(empresaId, d.Codigo, d.Nombre, d.ProductoId, d.FamiliaId, Defectos(d));
        if (p.EsFallo)
        {
            return Resultado.Fallo<PlantillaCalidadDto>(p.Error);
        }

        p.Valor.Activar(d.Activa);
        _repo.Agregar(p.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(p.Valor));
    }

    public async Task<Resultado<PlantillaCalidadDto>> CambiarPlantillaAsync(Guid id, DatosPlantillaCalidad d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var p = await _repo.PlantillaCalidadAsync(id, ct).ConfigureAwait(false);
        if (p is null)
        {
            return Resultado.Fallo<PlantillaCalidadDto>(NoExistePlantilla());
        }

        var enUso = await _repo.PlantillaCalidadEnUsoAsync(id, ct).ConfigureAwait(false);
        var r = p.Cambiar(d.Codigo, d.Nombre, d.ProductoId, d.FamiliaId, Defectos(d), enUso);
        if (r.EsFallo)
        {
            return Resultado.Fallo<PlantillaCalidadDto>(r.Error);
        }

        p.Activar(d.Activa);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(p));
    }

    /// <summary>Borra una plantilla sin muestreos; con muestreos, la desactiva (se conserva para los muestreos hechos).</summary>
    public async Task<Resultado<BajaDto>> EliminarPlantillaAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _repo.PlantillaCalidadAsync(id, ct).ConfigureAwait(false);
        if (p is null)
        {
            return Resultado.Fallo<BajaDto>(NoExistePlantilla());
        }

        if (await _repo.PlantillaCalidadEnUsoAsync(id, ct).ConfigureAwait(false))
        {
            p.Activar(false);
            await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
            return Resultado.Ok(new BajaDto(p.Id, false, false));
        }

        _repo.Eliminar(p);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new BajaDto(p.Id, true, false));
    }

    public async Task<IReadOnlyList<MuestreoDto>> MuestreosAsync(Guid recepcionId, CancellationToken ct = default)
    {
        var r = await _repo.RecepcionAsync(recepcionId, ct).ConfigureAwait(false);
        return r is null ? [] : await DtosAsync(r, await _repo.MuestreosAsync(r.Lineas.Select(l => l.Id).ToList(), ct).ConfigureAwait(false), ct).ConfigureAwait(false);
    }

    /// <summary>Registra un muestreo de la línea. El definitivo fija el descuento de peso: uno por línea y fuera de liquidaciones vivas.</summary>
    public async Task<Resultado<MuestreoDto>> MuestrearAsync(Guid recepcionId, Guid lineaId, DatosMuestreo d, Guid? usuarioId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var r = await _repo.RecepcionAsync(recepcionId, ct).ConfigureAwait(false);
        var linea = r?.Lineas.FirstOrDefault(l => l.Id == lineaId);
        if (r is null || linea is null)
        {
            return Resultado.Fallo<MuestreoDto>(Error.NoEncontrado("recepcion.linea_no_encontrada", "La línea no existe."));
        }

        if (r.Estado == EstadoRecepcion.Anulada)
        {
            return Resultado.Fallo<MuestreoDto>(Error.Conflicto("recepcion.anulada", "La recepción está anulada."));
        }

        var plantilla = await _repo.PlantillaCalidadAsync(d.PlantillaId, ct).ConfigureAwait(false);
        if (plantilla is null || plantilla.EmpresaId != r.EmpresaId || !plantilla.Activa)
        {
            return Resultado.Fallo<MuestreoDto>(Error.NoEncontrado("calidad.plantilla_no_encontrada", "La plantilla de calidad no existe o está desactivada."));
        }

        if (d.Definitivo)
        {
            if ((await _repo.MuestreosAsync([lineaId], ct).ConfigureAwait(false)).Any(m => m.Definitivo && !m.Anulado))
            {
                return Resultado.Fallo<MuestreoDto>(Error.Conflicto("muestreo.ya_definitivo", "La línea ya tiene su muestreo definitivo: anúlalo antes para hacer otro."));
            }

            if ((await _repo.LineasEnLiquidacionAsync([lineaId], ct).ConfigureAwait(false)).Count > 0)
            {
                return Resultado.Fallo<MuestreoDto>(Error.Conflicto("muestreo.liquidada", "La entrega ya está en una liquidación: anula antes la liquidación."));
            }
        }

        var m = MuestreoCalidad.Crear(r.EmpresaId, r.Id, lineaId, plantilla, d.Fecha ?? r.Fecha, d.PesoMuestraKg,
            (d.Resultados ?? []).Select(x => (x.DefectoId, x.Kilos)).ToList(), d.Definitivo, d.Observaciones, usuarioId, _reloj.AhoraUtc);
        if (m.EsFallo)
        {
            return Resultado.Fallo<MuestreoDto>(m.Error);
        }

        _repo.Agregar(m.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok((await DtosAsync(r, [m.Valor], ct).ConfigureAwait(false))[0]);
    }

    /// <summary>Anula un muestreo (con motivo). El definitivo, solo si la entrega no está en una liquidación viva.</summary>
    public async Task<Resultado<MuestreoDto>> AnularMuestreoAsync(Guid id, string? motivo, CancellationToken ct = default)
    {
        var m = await _repo.MuestreoAsync(id, ct).ConfigureAwait(false);
        if (m is null)
        {
            return Resultado.Fallo<MuestreoDto>(Error.NoEncontrado("muestreo.no_encontrado", "El muestreo no existe."));
        }

        if (m.Definitivo && (await _repo.LineasEnLiquidacionAsync([m.LineaRecepcionId], ct).ConfigureAwait(false)).Count > 0)
        {
            return Resultado.Fallo<MuestreoDto>(Error.Conflicto("muestreo.liquidada", "La entrega ya está en una liquidación: anula antes la liquidación."));
        }

        var a = m.Anular(motivo);
        if (a.EsFallo)
        {
            return Resultado.Fallo<MuestreoDto>(a.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        var r = await _repo.RecepcionAsync(m.RecepcionId, ct).ConfigureAwait(false);
        return Resultado.Ok((await DtosAsync(r!, [m], ct).ConfigureAwait(false))[0]);
    }

    private async Task<IReadOnlyList<MuestreoDto>> DtosAsync(Recepcion r, IReadOnlyList<MuestreoCalidad> muestreos, CancellationToken ct)
    {
        var plantillas = new Dictionary<Guid, PlantillaCalidad?>();
        foreach (var id in muestreos.Select(m => m.PlantillaId).Distinct())
        {
            plantillas[id] = await _repo.PlantillaCalidadAsync(id, ct).ConfigureAwait(false);
        }

        var rectificaciones = await _repo.RectificacionesAsync(r.Lineas.Select(l => l.Id).ToList(), ct).ConfigureAwait(false);
        return muestreos.OrderBy(m => m.CreadoEn).Select(m =>
        {
            var p = plantillas.GetValueOrDefault(m.PlantillaId);
            var avisos = p is null ? [] : m.Avisos(p);
            var linea = r.Lineas.First(l => l.Id == m.LineaRecepcionId);
            decimal? aLiquidar = m.Definitivo && !m.Anulado && linea.NetoKg is not null ? RectificacionRecepcion.KilosALiquidar(linea, rectificaciones, [m]) : null;
            return new MuestreoDto(m.Id, m.RecepcionId, m.LineaRecepcionId, m.PlantillaId, p?.Nombre, m.Fecha, m.PesoMuestraKg, m.Definitivo, m.DescuentoPct, aLiquidar, m.Anulado,
                m.MotivoAnulacion, m.Observaciones, m.UsuarioId,
                m.Resultados.Select(x => new ResultadoMuestreoDto(x.DefectoId, x.Defecto, x.Kilos, x.Porcentaje, x.DescuentaPeso, avisos.Any(a => a.Resultado.Id == x.Id),
                    avisos.Any(a => a.Resultado.Id == x.Id && a.SuperaMaximo))).ToList());
        }).ToList();
    }

    private static List<(string, bool, decimal?, decimal?)> Defectos(DatosPlantillaCalidad d) =>
        (d.Defectos ?? []).Select(x => (x.Nombre ?? "", x.DescuentaPeso, x.ToleranciaPct, x.MaximoPct)).ToList();

    private static PlantillaCalidadDto Dto(PlantillaCalidad p) => new(p.Id, p.Codigo, p.Nombre, p.ProductoId, p.FamiliaId, p.Activa,
        p.Defectos.OrderBy(d => d.Orden).Select(d => new DefectoDto(d.Id, d.Nombre, d.Orden, d.DescuentaPeso, d.ToleranciaPct, d.MaximoPct)).ToList());

    private static Error NoExistePlantilla() => Error.NoEncontrado("calidad.plantilla_no_encontrada", "La plantilla de calidad no existe.");
}
