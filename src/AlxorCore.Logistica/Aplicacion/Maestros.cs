using AlxorCore.Logistica.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Logistica.Aplicacion;

public sealed record ConfiguracionLogisticaDto(string PrefijoGs1, int DigitoExtension, bool MezclarLotes, long UltimaSerie);

public sealed record DatosConfiguracionLogistica(string? PrefijoGs1, int DigitoExtension, bool MezclarLotes = false);

public sealed record DatosSoporte(string? Codigo, string? Nombre, int LargoMm, int AnchoMm, int AltoMm, decimal TaraKg, decimal? CargaMaxKg = null, Guid? EnvaseProductoId = null,
    bool Activo = true);

public sealed record SoporteDto(Guid Id, string Codigo, string Nombre, int LargoMm, int AnchoMm, int AltoMm, decimal TaraKg, decimal? CargaMaxKg, Guid? EnvaseProductoId, bool Activo);

public sealed record FichaLogisticaDto(Guid Id, Guid ProductoId, string? Producto, string? Gtin, string? GtinCaja, int UnidadesPorCaja, decimal? PesoNetoUnidadKg,
    decimal? PesoNetoCajaKg, decimal? PesoBrutoCajaKg, int? LargoCajaMm, int? AnchoCajaMm, int? AltoCajaMm, int? CajasPorCapa, int? Capas, Guid? SoporteId,
    int? AlturaMaxPaleMm, decimal? PesoMaxPaleKg, bool Remontable, int? TemperaturaMinC, int? TemperaturaMaxC, int? VidaUtilDias, int? VidaMinimaEntregaDias,
    bool GestionLotes);

public sealed record DatosPlantillaPaletizado(Guid ProductoId, Guid? ClienteId, Guid SoporteId, int CajasPorCapa, int Capas, int? AlturaMaxMm = null, decimal? PesoMaxKg = null,
    string? Instrucciones = null, bool Activa = true);

public sealed record PlantillaPaletizadoDto(Guid Id, Guid ProductoId, string? Producto, Guid? ClienteId, Guid SoporteId, string? Soporte, int CajasPorCapa, int Capas,
    int CajasPorPale, int? AlturaMaxMm, decimal? PesoMaxKg, string? Instrucciones, bool Activa);

/// <summary>Maestros logísticos: configuración GS1, soportes, fichas logísticas de los artículos y plantillas de paletizado.</summary>
public sealed class MaestrosLogistica
{
    private readonly IRepositorioLogistica _repo;
    private readonly IUnidadDeTrabajoLogistica _unidad;
    private readonly IArticulosLogistica _articulos;

    public MaestrosLogistica(IRepositorioLogistica repo, IUnidadDeTrabajoLogistica unidad, IArticulosLogistica articulos)
    {
        _repo = repo;
        _unidad = unidad;
        _articulos = articulos;
    }

    public async Task<ConfiguracionLogisticaDto> ConfiguracionAsync(Guid empresaId, CancellationToken ct = default)
    {
        var c = await _repo.ConfiguracionAsync(empresaId, ct).ConfigureAwait(false);
        return c is null
            ? new ConfiguracionLogisticaDto(ConfiguracionLogistica.PrefijoPruebas, 0, false, 0)
            : new ConfiguracionLogisticaDto(c.PrefijoGs1, c.DigitoExtension, c.MezclarLotes, c.UltimaSerie);
    }

    public async Task<Resultado<ConfiguracionLogisticaDto>> FijarConfiguracionAsync(Guid empresaId, DatosConfiguracionLogistica d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var c = await _repo.ConfiguracionAsync(empresaId, ct).ConfigureAwait(false);
        if (c is null)
        {
            c = ConfiguracionLogistica.Crear(empresaId);
            _repo.Agregar(c);
        }

        var r = c.Actualizar(d.PrefijoGs1, d.DigitoExtension, d.MezclarLotes);
        if (r.EsFallo)
        {
            return Resultado.Fallo<ConfiguracionLogisticaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new ConfiguracionLogisticaDto(c.PrefijoGs1, c.DigitoExtension, c.MezclarLotes, c.UltimaSerie));
    }

    // ------------------------------------------------------------------ soportes
    public async Task<IReadOnlyList<SoporteDto>> SoportesAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.SoportesAsync(empresaId, ct).ConfigureAwait(false)).Select(Dto).ToList();

    public async Task<Resultado<SoporteDto>> CrearSoporteAsync(Guid empresaId, DatosSoporte d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        if ((await _repo.SoportesAsync(empresaId, ct).ConfigureAwait(false)).Any(s => string.Equals(s.Codigo, d.Codigo?.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            return Resultado.Fallo<SoporteDto>(Error.Conflicto("soporte.repetido", "Ya hay un soporte con ese código."));
        }

        var s = TipoSoporte.Crear(empresaId, d.Codigo, d.Nombre, d.LargoMm, d.AnchoMm, d.AltoMm, d.TaraKg, d.CargaMaxKg, d.EnvaseProductoId);
        if (s.EsFallo)
        {
            return Resultado.Fallo<SoporteDto>(s.Error);
        }

        _repo.Agregar(s.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(s.Valor));
    }

    public async Task<Resultado<SoporteDto>> ActualizarSoporteAsync(Guid id, DatosSoporte d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var s = await _repo.SoporteAsync(id, ct).ConfigureAwait(false);
        if (s is null)
        {
            return Resultado.Fallo<SoporteDto>(Error.NoEncontrado("soporte.no_encontrado", "El soporte no existe."));
        }

        var r = s.Actualizar(d.Nombre, d.LargoMm, d.AnchoMm, d.AltoMm, d.TaraKg, d.CargaMaxKg, d.EnvaseProductoId, d.Activo);
        if (r.EsFallo)
        {
            return Resultado.Fallo<SoporteDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(s));
    }

    /// <summary>Elimina el soporte si no se ha usado; si no, lo da de baja. Devuelve si se eliminó.</summary>
    public async Task<Resultado<bool>> EliminarSoporteAsync(Guid id, CancellationToken ct = default)
    {
        var s = await _repo.SoporteAsync(id, ct).ConfigureAwait(false);
        if (s is null)
        {
            return Resultado.Fallo<bool>(Error.NoEncontrado("soporte.no_encontrado", "El soporte no existe."));
        }

        var usado = await _repo.SoporteUsadoAsync(id, ct).ConfigureAwait(false);
        if (usado)
        {
            s.DarDeBaja();
        }
        else
        {
            _repo.Eliminar(s);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(!usado);
    }

    // ------------------------------------------------------------------ fichas logísticas
    public async Task<IReadOnlyList<FichaLogisticaDto>> FichasAsync(Guid empresaId, CancellationToken ct = default)
    {
        var fichas = await _repo.FichasAsync(empresaId, ct).ConfigureAwait(false);
        var articulos = await _articulos.ObtenerAsync(fichas.Select(f => f.ProductoId).ToList(), ct).ConfigureAwait(false);
        return fichas.Select(f => Dto(f, articulos.GetValueOrDefault(f.ProductoId)?.Nombre)).OrderBy(f => f.Producto, StringComparer.CurrentCulture).ToList();
    }

    public async Task<FichaLogisticaDto?> FichaAsync(Guid empresaId, Guid productoId, CancellationToken ct = default)
    {
        var f = await _repo.FichaAsync(empresaId, productoId, ct).ConfigureAwait(false);
        return f is null ? null : Dto(f, (await _articulos.ObtenerAsync([productoId], ct).ConfigureAwait(false)).GetValueOrDefault(productoId)?.Nombre);
    }

    /// <summary>Da de alta o cambia la ficha logística del artículo.</summary>
    public async Task<Resultado<FichaLogisticaDto>> FijarFichaAsync(Guid empresaId, Guid productoId, DatosFichaLogistica d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var articulo = (await _articulos.ObtenerAsync([productoId], ct).ConfigureAwait(false)).GetValueOrDefault(productoId);
        if (articulo is null)
        {
            return Resultado.Fallo<FichaLogisticaDto>(Error.NoEncontrado("producto.no_encontrado", "El artículo no existe."));
        }

        if (d.SoporteId is { } sid && await _repo.SoporteAsync(sid, ct).ConfigureAwait(false) is null)
        {
            return Resultado.Fallo<FichaLogisticaDto>(Error.NoEncontrado("soporte.no_encontrado", "El soporte no existe."));
        }

        foreach (var gtin in new[] { d.Gtin, d.GtinCaja }.Where(g => !string.IsNullOrWhiteSpace(g)))
        {
            if (AlxorCore.Nucleo.Comun.Gs1.Gtin14(gtin!.Trim()) is { } g14 && await _repo.FichaPorGtinAsync(empresaId, g14, ct).ConfigureAwait(false) is { } otra && otra.ProductoId != productoId)
            {
                return Resultado.Fallo<FichaLogisticaDto>(Error.Conflicto("ficha_logistica.gtin_repetido", $"El GTIN {gtin} ya es de otro artículo."));
            }
        }

        var f = await _repo.FichaAsync(empresaId, productoId, ct).ConfigureAwait(false);
        if (f is null)
        {
            var nueva = FichaLogistica.Crear(empresaId, productoId, d);
            if (nueva.EsFallo)
            {
                return Resultado.Fallo<FichaLogisticaDto>(nueva.Error);
            }

            f = nueva.Valor;
            _repo.Agregar(f);
        }
        else if (f.Actualizar(d) is { EsFallo: true } r)
        {
            return Resultado.Fallo<FichaLogisticaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(f, articulo.Nombre));
    }

    public async Task<Resultado> EliminarFichaAsync(Guid empresaId, Guid productoId, CancellationToken ct = default)
    {
        var f = await _repo.FichaAsync(empresaId, productoId, ct).ConfigureAwait(false);
        if (f is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("ficha_logistica.no_encontrada", "El artículo no tiene ficha logística."));
        }

        _repo.Eliminar(f);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    // ------------------------------------------------------------------ plantillas
    public async Task<IReadOnlyList<PlantillaPaletizadoDto>> PlantillasAsync(Guid empresaId, Guid? productoId, CancellationToken ct = default)
    {
        var lista = await _repo.PlantillasAsync(empresaId, productoId, ct).ConfigureAwait(false);
        var soportes = (await _repo.SoportesAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(s => s.Id);
        var articulos = await _articulos.ObtenerAsync(lista.Select(p => p.ProductoId).Distinct().ToList(), ct).ConfigureAwait(false);
        return lista.Select(p => Dto(p, articulos.GetValueOrDefault(p.ProductoId)?.Nombre, soportes.GetValueOrDefault(p.SoporteId)?.Nombre)).ToList();
    }

    public async Task<Resultado<PlantillaPaletizadoDto>> CrearPlantillaAsync(Guid empresaId, DatosPlantillaPaletizado d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var soporte = await _repo.SoporteAsync(d.SoporteId, ct).ConfigureAwait(false);
        if (soporte is null)
        {
            return Resultado.Fallo<PlantillaPaletizadoDto>(Error.NoEncontrado("soporte.no_encontrado", "El soporte no existe."));
        }

        if ((await _repo.PlantillasAsync(empresaId, d.ProductoId, ct).ConfigureAwait(false)).Any(p => p.Activa && p.ClienteId == d.ClienteId && p.SoporteId == d.SoporteId))
        {
            return Resultado.Fallo<PlantillaPaletizadoDto>(Error.Conflicto("plantilla_paletizado.repetida",
                "Ya hay una plantilla activa de ese artículo para ese cliente y soporte: cámbiala o dala de baja."));
        }

        var p = PlantillaPaletizado.Crear(empresaId, d.ProductoId, d.ClienteId, d.SoporteId, d.CajasPorCapa, d.Capas, d.AlturaMaxMm, d.PesoMaxKg, d.Instrucciones);
        if (p.EsFallo)
        {
            return Resultado.Fallo<PlantillaPaletizadoDto>(p.Error);
        }

        _repo.Agregar(p.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        var nombre = (await _articulos.ObtenerAsync([d.ProductoId], ct).ConfigureAwait(false)).GetValueOrDefault(d.ProductoId)?.Nombre;
        return Resultado.Ok(Dto(p.Valor, nombre, soporte.Nombre));
    }

    public async Task<Resultado<PlantillaPaletizadoDto>> ActualizarPlantillaAsync(Guid id, DatosPlantillaPaletizado d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var p = await _repo.PlantillaAsync(id, ct).ConfigureAwait(false);
        var soporte = await _repo.SoporteAsync(d.SoporteId, ct).ConfigureAwait(false);
        if (p is null || soporte is null)
        {
            return Resultado.Fallo<PlantillaPaletizadoDto>(Error.NoEncontrado("plantilla_paletizado.no_encontrada", "La plantilla o el soporte no existen."));
        }

        var r = p.Actualizar(d.SoporteId, d.CajasPorCapa, d.Capas, d.AlturaMaxMm, d.PesoMaxKg, d.Instrucciones, d.Activa);
        if (r.EsFallo)
        {
            return Resultado.Fallo<PlantillaPaletizadoDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        var nombre = (await _articulos.ObtenerAsync([p.ProductoId], ct).ConfigureAwait(false)).GetValueOrDefault(p.ProductoId)?.Nombre;
        return Resultado.Ok(Dto(p, nombre, soporte.Nombre));
    }

    public async Task<Resultado<bool>> EliminarPlantillaAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _repo.PlantillaAsync(id, ct).ConfigureAwait(false);
        if (p is null)
        {
            return Resultado.Fallo<bool>(Error.NoEncontrado("plantilla_paletizado.no_encontrada", "La plantilla no existe."));
        }

        var usada = await _repo.PlantillaUsadaAsync(id, ct).ConfigureAwait(false);
        if (usada)
        {
            p.Actualizar(p.SoporteId, p.CajasPorCapa, p.Capas, p.AlturaMaxMm, p.PesoMaxKg, p.Instrucciones, false);
        }
        else
        {
            _repo.Eliminar(p);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(!usada);
    }

    /// <summary>
    /// Mosaico con que se paletiza el artículo: la plantilla indicada; si no, la activa del cliente; si no, la general;
    /// si no, el de la ficha logística (con su soporte). Null si no hay con qué.
    /// </summary>
    public async Task<Resultado<Mosaico>> MosaicoAsync(Guid empresaId, FichaLogistica ficha, Guid? clienteId, Guid? plantillaId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ficha);
        PlantillaPaletizado? p;
        if (plantillaId is { } pid)
        {
            p = await _repo.PlantillaAsync(pid, ct).ConfigureAwait(false);
            if (p is null || p.ProductoId != ficha.ProductoId)
            {
                return Resultado.Fallo<Mosaico>(Error.NoEncontrado("plantilla_paletizado.no_encontrada", "La plantilla no existe o es de otro artículo."));
            }
        }
        else
        {
            var activas = (await _repo.PlantillasAsync(empresaId, ficha.ProductoId, ct).ConfigureAwait(false)).Where(x => x.Activa).ToList();
            p = (clienteId is null ? null : activas.FirstOrDefault(x => x.ClienteId == clienteId)) ?? activas.FirstOrDefault(x => x.ClienteId is null);
        }

        if (p is not null)
        {
            var soporte = await _repo.SoporteAsync(p.SoporteId, ct).ConfigureAwait(false);
            return Resultado.Ok(new Mosaico(p.CajasPorCapa, p.Capas, soporte, p.AlturaMaxMm, p.PesoMaxKg, p.Id, p.ClienteId is null ? "Plantilla" : "Plantilla del cliente"));
        }

        if (ficha.CajasPorCapa is { } cpc && ficha.Capas is { } capas)
        {
            var soporte = ficha.SoporteId is { } sid ? await _repo.SoporteAsync(sid, ct).ConfigureAwait(false) : null;
            return Resultado.Ok(new Mosaico(cpc, capas, soporte, ficha.AlturaMaxPaleMm, ficha.PesoMaxPaleKg, null, "Ficha logística"));
        }

        return Resultado.Fallo<Mosaico>(Error.Validacion("paletizado.sin_mosaico",
            "El artículo no tiene plantilla de paletizado ni mosaico en su ficha logística (cajas por capa y capas)."));
    }

    private static SoporteDto Dto(TipoSoporte s) => new(s.Id, s.Codigo, s.Nombre, s.LargoMm, s.AnchoMm, s.AltoMm, s.TaraKg, s.CargaMaxKg, s.EnvaseProductoId, s.Activo);

    public static FichaLogisticaDto Dto(FichaLogistica f, string? producto) => new(f.Id, f.ProductoId, producto, f.Gtin, f.GtinCaja, f.UnidadesPorCaja, f.PesoNetoUnidadKg,
        f.PesoNetoCajaKg, f.PesoBrutoCajaKg, f.LargoCajaMm, f.AnchoCajaMm, f.AltoCajaMm, f.CajasPorCapa, f.Capas, f.SoporteId, f.AlturaMaxPaleMm, f.PesoMaxPaleKg, f.Remontable,
        f.TemperaturaMinC, f.TemperaturaMaxC, f.VidaUtilDias, f.VidaMinimaEntregaDias, f.GestionLotes);

    private static PlantillaPaletizadoDto Dto(PlantillaPaletizado p, string? producto, string? soporte) => new(p.Id, p.ProductoId, producto, p.ClienteId, p.SoporteId, soporte,
        p.CajasPorCapa, p.Capas, p.CajasPorPale, p.AlturaMaxMm, p.PesoMaxKg, p.Instrucciones, p.Activa);
}
