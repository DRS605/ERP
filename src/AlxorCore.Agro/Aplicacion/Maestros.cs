using AlxorCore.Agro.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Agro.Aplicacion;

public sealed record CampanaDto(Guid Id, string Codigo, string Nombre, DateOnly Desde, DateOnly Hasta)
{
    public static CampanaDto De(Campana c) => new(c.Id, c.Codigo, c.Nombre, c.Desde, c.Hasta);
}

public sealed record AgricultorDto(
    Guid Id, Guid ProveedorId, string Nombre, string Regimen, string CodigoImpuesto, decimal PorcentajeRetencion,
    DateOnly? AutofacturacionDesde, string? MotivoBloqueo, bool Bloqueado)
{
    public static AgricultorDto De(Agricultor a) => new(a.Id, a.ProveedorId, a.Nombre, a.Regimen.ToString(), a.CodigoImpuesto, a.PorcentajeRetencion,
        a.AutofacturacionDesde, a.MotivoBloqueo, a.Bloqueado);
}

public sealed record ParcelaDto(
    Guid Id, Guid AgricultorId, string Codigo, string Nombre, string? ReferenciaSigpac, decimal? SuperficieHa, Guid? ProductoId,
    string? Variedad, Guid? CentroAnaliticoId, bool Activa)
{
    public static ParcelaDto De(Parcela p) => new(p.Id, p.AgricultorId, p.Codigo, p.Nombre, p.ReferenciaSigpac, p.SuperficieHa, p.ProductoId,
        p.Variedad, p.CentroAnaliticoId, p.Activa);
}

public sealed record CategoriaDto(Guid Id, string Codigo, string Nombre, bool EsDestrio, int Orden)
{
    public static CategoriaDto De(Categoria c) => new(c.Id, c.Codigo, c.Nombre, c.EsDestrio, c.Orden);
}

public sealed record ArticuloCampanaDto(Guid Id, Guid CampanaId, Guid ProductoId, string Metodo)
{
    public static ArticuloCampanaDto De(ArticuloCampana a) => new(a.Id, a.CampanaId, a.ProductoId, a.Metodo.ToString());
}

public sealed record PrecioDto(Guid Id, Guid CampanaId, Guid ProductoId, Guid? CategoriaId, DateOnly Desde, DateOnly Hasta, decimal PrecioKg)
{
    public static PrecioDto De(PrecioLiquidacion p) => new(p.Id, p.CampanaId, p.ProductoId, p.CategoriaId, p.Desde, p.Hasta, p.PrecioKg);
}

public sealed record ConceptoDto(Guid Id, string Codigo, string Nombre, string Tipo, decimal Valor, bool Activo)
{
    public static ConceptoDto De(ConceptoLiquidacion c) => new(c.Id, c.Codigo, c.Nombre, c.Tipo.ToString(), c.Valor, c.Activo);
}

public sealed record TarifaDto(Guid Id, string Recurso, string Categoria, string TipoHora, DateOnly Desde, DateOnly? Hasta, decimal CosteUnitario)
{
    public static TarifaDto De(TarifaCoste t) => new(t.Id, t.Recurso.ToString(), t.Categoria, t.TipoHora.ToString(), t.Desde, t.Hasta, t.CosteUnitario);
}

public sealed record ConfiguracionAgroDto(string PrefijoGs1, int DigitoExtension);

public sealed record DatosCampana(string? Codigo, string? Nombre, DateOnly Desde, DateOnly Hasta);

public sealed record DatosAgricultor(
    Guid ProveedorId, RegimenAgricultor Regimen = RegimenAgricultor.Reagp, decimal? PorcentajeRetencion = null, DateOnly? AutofacturacionDesde = null,
    string? MotivoBloqueo = null, string? CodigoImpuesto = null);

public sealed record DatosCategoria(string? Codigo, string? Nombre, bool EsDestrio = false, int Orden = 0);

public sealed record DatosArticuloCampana(Guid ProductoId, MetodoLiquidacion Metodo);

public sealed record DatosPrecio(Guid ProductoId, Guid? CategoriaId, DateOnly Desde, DateOnly Hasta, decimal PrecioKg);

public sealed record DatosConcepto(string? Codigo, string? Nombre, TipoConceptoLiquidacion Tipo, decimal Valor, bool Activo = true);

public sealed record DatosTarifa(RecursoCoste Recurso, string? Categoria, TipoHora TipoHora, DateOnly Desde, DateOnly? Hasta, decimal CosteUnitario);

/// <summary>Maestros del módulo agro: campañas, agricultores, parcelas, categorías, precios, descuentos y tarifas.</summary>
public sealed class MaestrosAgro
{
    /// <summary>Retención de IRPF por defecto de las actividades agrícolas en estimación objetiva.</summary>
    public const decimal RetencionAgricola = 2m;

    private readonly IRepositorioAgro _repo;
    private readonly IUnidadDeTrabajoAgro _unidad;
    private readonly IConsultaProveedores _proveedores;

    public MaestrosAgro(IRepositorioAgro repo, IUnidadDeTrabajoAgro unidad, IConsultaProveedores proveedores)
    {
        _repo = repo;
        _unidad = unidad;
        _proveedores = proveedores;
    }

    // ------------------------------------------------------------------ Campañas
    public async Task<IReadOnlyList<CampanaDto>> CampanasAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.CampanasAsync(empresaId, ct).ConfigureAwait(false)).OrderByDescending(c => c.Desde).Select(CampanaDto.De).ToList();

    public async Task<Resultado<CampanaDto>> CrearCampanaAsync(Guid empresaId, DatosCampana datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var c = Campana.Crear(empresaId, datos.Codigo, datos.Nombre, datos.Desde, datos.Hasta);
        if (c.EsFallo)
        {
            return Resultado.Fallo<CampanaDto>(c.Error);
        }

        var existentes = await _repo.CampanasAsync(empresaId, ct).ConfigureAwait(false);
        if (existentes.Any(e => e.Codigo == c.Valor.Codigo))
        {
            return Resultado.Fallo<CampanaDto>(Error.Conflicto("campana.duplicada", $"Ya existe la campaña {c.Valor.Codigo}."));
        }

        if (existentes.Any(e => e.Desde <= c.Valor.Hasta && c.Valor.Desde <= e.Hasta))
        {
            return Resultado.Fallo<CampanaDto>(Error.Conflicto("campana.solapada", "Las fechas se solapan con otra campaña."));
        }

        _repo.Agregar(c.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(CampanaDto.De(c.Valor));
    }

    // ------------------------------------------------------------------ Agricultores
    public async Task<IReadOnlyList<AgricultorDto>> AgricultoresAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.AgricultoresAsync(empresaId, ct).ConfigureAwait(false)).OrderBy(a => a.Nombre, StringComparer.CurrentCulture).Select(AgricultorDto.De).ToList();

    public async Task<Resultado<AgricultorDto>> CrearAgricultorAsync(Guid empresaId, DatosAgricultor datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var proveedor = await _proveedores.ObtenerAsync(datos.ProveedorId, ct).ConfigureAwait(false);
        if (proveedor is null)
        {
            return Resultado.Fallo<AgricultorDto>(Error.NoEncontrado("proveedor.no_encontrado", "El proveedor no existe."));
        }

        if (await _repo.ExisteAgricultorAsync(empresaId, datos.ProveedorId, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<AgricultorDto>(Error.Conflicto("agricultor.duplicado", "Ese proveedor ya es agricultor de la empresa."));
        }

        var retencion = datos.PorcentajeRetencion ?? (proveedor.PorcentajeIrpfDefecto > 0m ? proveedor.PorcentajeIrpfDefecto : RetencionAgricola);
        var a = Agricultor.Crear(empresaId, proveedor.Id, proveedor.Nombre, datos.Regimen, retencion, datos.AutofacturacionDesde, datos.CodigoImpuesto);
        if (a.EsFallo)
        {
            return Resultado.Fallo<AgricultorDto>(a.Error);
        }

        _repo.Agregar(a.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(AgricultorDto.De(a.Valor));
    }

    public async Task<Resultado<AgricultorDto>> ActualizarAgricultorAsync(Guid id, DatosAgricultor datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var a = await _repo.AgricultorAsync(id, ct).ConfigureAwait(false);
        if (a is null)
        {
            return Resultado.Fallo<AgricultorDto>(Error.NoEncontrado("agricultor.no_encontrado", "El agricultor no existe."));
        }

        var r = a.Actualizar(datos.Regimen, datos.PorcentajeRetencion ?? a.PorcentajeRetencion, datos.AutofacturacionDesde, datos.MotivoBloqueo, datos.CodigoImpuesto);
        if (r.EsFallo)
        {
            return Resultado.Fallo<AgricultorDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(AgricultorDto.De(a));
    }

    // ------------------------------------------------------------------ Parcelas
    public async Task<IReadOnlyList<ParcelaDto>> ParcelasAsync(Guid empresaId, Guid? agricultorId, CancellationToken ct = default) =>
        (await _repo.ParcelasAsync(empresaId, agricultorId, ct).ConfigureAwait(false)).OrderBy(p => p.Codigo, StringComparer.Ordinal).Select(ParcelaDto.De).ToList();

    public async Task<Resultado<ParcelaDto>> CrearParcelaAsync(Guid empresaId, Guid agricultorId, DatosParcela datos, CancellationToken ct = default)
    {
        if (await _repo.AgricultorAsync(agricultorId, ct).ConfigureAwait(false) is null)
        {
            return Resultado.Fallo<ParcelaDto>(Error.NoEncontrado("agricultor.no_encontrado", "El agricultor no existe."));
        }

        var p = Parcela.Crear(empresaId, agricultorId, datos);
        if (p.EsFallo)
        {
            return Resultado.Fallo<ParcelaDto>(p.Error);
        }

        if ((await _repo.ParcelasAsync(empresaId, null, ct).ConfigureAwait(false)).Any(x => x.Codigo == p.Valor.Codigo))
        {
            return Resultado.Fallo<ParcelaDto>(Error.Conflicto("parcela.duplicada", $"Ya existe la parcela {p.Valor.Codigo}."));
        }

        _repo.Agregar(p.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ParcelaDto.De(p.Valor));
    }

    public async Task<Resultado<ParcelaDto>> ActualizarParcelaAsync(Guid id, DatosParcela datos, CancellationToken ct = default)
    {
        var p = await _repo.ParcelaAsync(id, ct).ConfigureAwait(false);
        if (p is null)
        {
            return Resultado.Fallo<ParcelaDto>(Error.NoEncontrado("parcela.no_encontrada", "La parcela no existe."));
        }

        var r = p.Actualizar(datos);
        if (r.EsFallo)
        {
            return Resultado.Fallo<ParcelaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ParcelaDto.De(p));
    }

    // ------------------------------------------------------------------ Categorías
    public async Task<IReadOnlyList<CategoriaDto>> CategoriasAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.CategoriasAsync(empresaId, ct).ConfigureAwait(false)).OrderBy(c => c.Orden).ThenBy(c => c.Codigo, StringComparer.Ordinal).Select(CategoriaDto.De).ToList();

    public async Task<Resultado<CategoriaDto>> CrearCategoriaAsync(Guid empresaId, DatosCategoria datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var c = Categoria.Crear(empresaId, datos.Codigo, datos.Nombre, datos.EsDestrio, datos.Orden);
        if (c.EsFallo)
        {
            return Resultado.Fallo<CategoriaDto>(c.Error);
        }

        if ((await _repo.CategoriasAsync(empresaId, ct).ConfigureAwait(false)).Any(x => x.Codigo == c.Valor.Codigo))
        {
            return Resultado.Fallo<CategoriaDto>(Error.Conflicto("categoria.duplicada", $"Ya existe la categoría {c.Valor.Codigo}."));
        }

        _repo.Agregar(c.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(CategoriaDto.De(c.Valor));
    }

    // ------------------------------------------------------------------ Artículos y precios de la campaña
    public async Task<IReadOnlyList<ArticuloCampanaDto>> ArticulosCampanaAsync(Guid campanaId, CancellationToken ct = default) =>
        (await _repo.ArticulosCampanaAsync(campanaId, ct).ConfigureAwait(false)).Select(ArticuloCampanaDto.De).ToList();

    /// <summary>Fija cómo se liquida un artículo en la campaña (lo crea o lo cambia).</summary>
    public async Task<Resultado<ArticuloCampanaDto>> FijarArticuloCampanaAsync(Guid empresaId, Guid campanaId, DatosArticuloCampana datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        if (await _repo.CampanaAsync(campanaId, ct).ConfigureAwait(false) is null)
        {
            return Resultado.Fallo<ArticuloCampanaDto>(Error.NoEncontrado("campana.no_encontrada", "La campaña no existe."));
        }

        if (!Enum.IsDefined(datos.Metodo))
        {
            return Resultado.Fallo<ArticuloCampanaDto>(Error.Validacion("campana.metodo", "El método debe ser PorClasificacion o PorPeriodo."));
        }

        var actual = (await _repo.ArticulosCampanaAsync(campanaId, ct).ConfigureAwait(false)).FirstOrDefault(a => a.ProductoId == datos.ProductoId);
        if (actual is null)
        {
            actual = ArticuloCampana.Crear(empresaId, campanaId, datos.ProductoId, datos.Metodo);
            _repo.Agregar(actual);
        }
        else
        {
            actual.CambiarMetodo(datos.Metodo);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ArticuloCampanaDto.De(actual));
    }

    public async Task<IReadOnlyList<PrecioDto>> PreciosAsync(Guid campanaId, CancellationToken ct = default) =>
        (await _repo.PreciosAsync(campanaId, ct).ConfigureAwait(false)).OrderBy(p => p.ProductoId).ThenBy(p => p.Desde).Select(PrecioDto.De).ToList();

    public async Task<Resultado<PrecioDto>> CrearPrecioAsync(Guid empresaId, Guid campanaId, DatosPrecio datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var campana = await _repo.CampanaAsync(campanaId, ct).ConfigureAwait(false);
        if (campana is null)
        {
            return Resultado.Fallo<PrecioDto>(Error.NoEncontrado("campana.no_encontrada", "La campaña no existe."));
        }

        var p = PrecioLiquidacion.Crear(empresaId, campanaId, datos.ProductoId, datos.CategoriaId, datos.Desde, datos.Hasta, datos.PrecioKg);
        if (p.EsFallo)
        {
            return Resultado.Fallo<PrecioDto>(p.Error);
        }

        if (!campana.Contiene(datos.Desde) || !campana.Contiene(datos.Hasta))
        {
            return Resultado.Fallo<PrecioDto>(Error.Validacion("precio.fuera_campana", "El periodo del precio debe estar dentro de la campaña."));
        }

        var solapa = (await _repo.PreciosAsync(campanaId, ct).ConfigureAwait(false))
            .Any(x => x.ProductoId == datos.ProductoId && x.CategoriaId == datos.CategoriaId && x.Desde <= datos.Hasta && datos.Desde <= x.Hasta);
        if (solapa)
        {
            return Resultado.Fallo<PrecioDto>(Error.Conflicto("precio.solapado", "Ya hay un precio de ese artículo y categoría en esas fechas."));
        }

        _repo.Agregar(p.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PrecioDto.De(p.Valor));
    }

    /// <summary>
    /// Cambia un precio. Las liquidaciones en borrador que lo usan quedan desactualizadas (hay que recalcularlas
    /// antes de emitir); si ya se aplicó en una emitida, la base de datos lo impide.
    /// </summary>
    public async Task<Resultado<PrecioDto>> ActualizarPrecioAsync(Guid id, DatosPrecio datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var p = await _repo.PrecioAsync(id, ct).ConfigureAwait(false);
        if (p is null)
        {
            return Resultado.Fallo<PrecioDto>(Error.NoEncontrado("precio.no_encontrado", "El precio no existe."));
        }

        var campana = await _repo.CampanaAsync(p.CampanaId, ct).ConfigureAwait(false);
        if (campana is not null && (!campana.Contiene(datos.Desde) || !campana.Contiene(datos.Hasta)))
        {
            return Resultado.Fallo<PrecioDto>(Error.Validacion("precio.fuera_campana", "El periodo del precio debe estar dentro de la campaña."));
        }

        var solapa = (await _repo.PreciosAsync(p.CampanaId, ct).ConfigureAwait(false))
            .Any(x => x.Id != p.Id && x.ProductoId == p.ProductoId && x.CategoriaId == p.CategoriaId && x.Desde <= datos.Hasta && datos.Desde <= x.Hasta);
        if (solapa)
        {
            return Resultado.Fallo<PrecioDto>(Error.Conflicto("precio.solapado", "Ya hay un precio de ese artículo y categoría en esas fechas."));
        }

        var r = p.Actualizar(datos.Desde, datos.Hasta, datos.PrecioKg);
        if (r.EsFallo)
        {
            return Resultado.Fallo<PrecioDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PrecioDto.De(p));
    }

    /// <summary>Elimina un precio que no use ninguna liquidación (en borrador o emitida).</summary>
    public async Task<Resultado> EliminarPrecioAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _repo.PrecioAsync(id, ct).ConfigureAwait(false);
        if (p is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("precio.no_encontrado", "El precio no existe."));
        }

        if (await _repo.PrecioEnUsoAsync(id, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo(Error.Conflicto("precio.en_uso", "Una liquidación usa este precio: cámbialo en lugar de borrarlo, o elimina antes el borrador."));
        }

        _repo.Eliminar(p);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    // ------------------------------------------------------------------ Conceptos de descuento
    public async Task<IReadOnlyList<ConceptoDto>> ConceptosAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.ConceptosAsync(empresaId, ct).ConfigureAwait(false)).OrderBy(c => c.Codigo, StringComparer.Ordinal).Select(ConceptoDto.De).ToList();

    public async Task<Resultado<ConceptoDto>> CrearConceptoAsync(Guid empresaId, DatosConcepto datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var c = ConceptoLiquidacion.Crear(empresaId, datos.Codigo, datos.Nombre, datos.Tipo, datos.Valor);
        if (c.EsFallo)
        {
            return Resultado.Fallo<ConceptoDto>(c.Error);
        }

        if ((await _repo.ConceptosAsync(empresaId, ct).ConfigureAwait(false)).Any(x => x.Codigo == c.Valor.Codigo))
        {
            return Resultado.Fallo<ConceptoDto>(Error.Conflicto("concepto.duplicado", $"Ya existe el concepto {c.Valor.Codigo}."));
        }

        c.Valor.FijarActivo(datos.Activo);
        _repo.Agregar(c.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ConceptoDto.De(c.Valor));
    }

    public async Task<Resultado<ConceptoDto>> ActivarConceptoAsync(Guid id, bool activo, CancellationToken ct = default)
    {
        var c = await _repo.ConceptoAsync(id, ct).ConfigureAwait(false);
        if (c is null)
        {
            return Resultado.Fallo<ConceptoDto>(Error.NoEncontrado("concepto.no_encontrado", "El concepto no existe."));
        }

        c.FijarActivo(activo);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ConceptoDto.De(c));
    }

    // ------------------------------------------------------------------ Tarifas de coste
    public async Task<IReadOnlyList<TarifaDto>> TarifasAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.TarifasAsync(empresaId, ct).ConfigureAwait(false)).OrderBy(t => t.Recurso).ThenBy(t => t.Categoria, StringComparer.Ordinal).ThenBy(t => t.TipoHora).ThenBy(t => t.Desde)
            .Select(TarifaDto.De).ToList();

    public async Task<Resultado<TarifaDto>> CrearTarifaAsync(Guid empresaId, DatosTarifa datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var t = TarifaCoste.Crear(empresaId, datos.Recurso, datos.Categoria, datos.TipoHora, datos.Desde, datos.Hasta, datos.CosteUnitario);
        if (t.EsFallo)
        {
            return Resultado.Fallo<TarifaDto>(t.Error);
        }

        var hasta = datos.Hasta ?? DateOnly.MaxValue;
        var solapa = (await _repo.TarifasAsync(empresaId, ct).ConfigureAwait(false)).Any(x =>
            x.Recurso == t.Valor.Recurso && x.Categoria == t.Valor.Categoria && x.TipoHora == t.Valor.TipoHora &&
            x.Desde <= hasta && datos.Desde <= (x.Hasta ?? DateOnly.MaxValue));
        if (solapa)
        {
            return Resultado.Fallo<TarifaDto>(Error.Conflicto("tarifa.solapada", "Ya hay una tarifa de esa categoría y tipo de hora en esas fechas: cierra la anterior."));
        }

        _repo.Agregar(t.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(TarifaDto.De(t.Valor));
    }

    // ------------------------------------------------------------------ Ajustes
    public async Task<ConfiguracionAgroDto> ConfiguracionAsync(Guid empresaId, CancellationToken ct = default)
    {
        var c = await _repo.ConfiguracionAsync(empresaId, ct).ConfigureAwait(false);
        return c is null ? new ConfiguracionAgroDto(ConfiguracionAgro.PrefijoPruebas, 0) : new ConfiguracionAgroDto(c.PrefijoGs1, c.DigitoExtension);
    }

    public async Task<Resultado<ConfiguracionAgroDto>> ActualizarConfiguracionAsync(Guid empresaId, ConfiguracionAgroDto datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var c = await _repo.ConfiguracionAsync(empresaId, ct).ConfigureAwait(false);
        if (c is null)
        {
            c = ConfiguracionAgro.Crear(empresaId);
            _repo.Agregar(c);
        }

        var r = c.Actualizar(datos.PrefijoGs1, datos.DigitoExtension);
        if (r.EsFallo)
        {
            return Resultado.Fallo<ConfiguracionAgroDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new ConfiguracionAgroDto(c.PrefijoGs1, c.DigitoExtension));
    }
}
