using AlxorCore.Agro.Dominio;
using AlxorCore.Nucleo.Aplicacion;
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

public sealed record PrecioDto(Guid Id, Guid CampanaId, Guid ProductoId, Guid? CategoriaId, DateOnly Desde, DateOnly Hasta, decimal PrecioKg,
    string Tipo = "Periodo", Guid? EnvaseProductoId = null)
{
    public static PrecioDto De(PrecioLiquidacion p) => new(p.Id, p.CampanaId, p.ProductoId, p.CategoriaId, p.Desde, p.Hasta, p.PrecioKg, p.Tipo.ToString(), p.EnvaseProductoId);
}

public sealed record ConceptoDto(Guid Id, string Codigo, string Nombre, string Tipo, decimal Valor, bool Activo, Guid? AgricultorId = null, Guid? ProductoId = null,
    Guid? EnvaseProductoId = null, bool Abono = false)
{
    public static ConceptoDto De(ConceptoLiquidacion c) => new(c.Id, c.Codigo, c.Nombre, c.Tipo.ToString(), c.Valor, c.Activo, c.AgricultorId, c.ProductoId,
        c.EnvaseProductoId, c.Abono);
}

public sealed record TarifaDto(Guid Id, string Recurso, string Categoria, string TipoHora, DateOnly Desde, DateOnly? Hasta, decimal CosteUnitario)
{
    public static TarifaDto De(TarifaCoste t) => new(t.Id, t.Recurso.ToString(), t.Categoria, t.TipoHora.ToString(), t.Desde, t.Hasta, t.CosteUnitario);
}

public sealed record ConfiguracionAgroDto(string PrefijoGs1, int DigitoExtension, bool? ReflejarPartidasEnInventario = null, bool? ReflejarEnvasesEnInventario = null,
    decimal? ToleranciaMermaPct = null);

public sealed record DatosCampana(string? Codigo, string? Nombre, DateOnly Desde, DateOnly Hasta);

public sealed record DatosAgricultor(
    Guid ProveedorId, RegimenAgricultor Regimen = RegimenAgricultor.Reagp, decimal? PorcentajeRetencion = null, DateOnly? AutofacturacionDesde = null,
    string? MotivoBloqueo = null, string? CodigoImpuesto = null);

public sealed record DatosCategoria(string? Codigo, string? Nombre, bool EsDestrio = false, int Orden = 0);

public sealed record DatosArticuloCampana(Guid ProductoId, MetodoLiquidacion Metodo);

public sealed record DatosPrecio(Guid ProductoId, Guid? CategoriaId, DateOnly Desde, DateOnly Hasta, decimal PrecioKg,
    TipoPrecioLiquidacion Tipo = TipoPrecioLiquidacion.Periodo, Guid? EnvaseProductoId = null);

/// <summary>Varios precios de una vez. Con <paramref name="Sustituir"/>, el que tenga exactamente la misma clave y fechas se actualiza.</summary>
public sealed record DatosPreciosMasivos(IReadOnlyList<DatosPrecio> Precios, bool Sustituir = true);

public sealed record ResultadoPreciosMasivosDto(int Creados, int Actualizados, IReadOnlyList<ErrorDto> Errores);

/// <summary>Propuesta de precios del periodo a partir de lo vendido: precio medio de venta menos una deducción por kilo y un porcentaje.</summary>
public sealed record DatosPropuestaVentas(DateOnly Desde, DateOnly Hasta, decimal DeduccionKg = 0m, decimal DeduccionPorcentaje = 0m, IReadOnlyList<Guid>? ProductoIds = null);

public sealed record PropuestaPrecioVentaDto(Guid ProductoId, string? Producto, string Metodo, decimal KilosVendidos, decimal ImporteVentas, decimal? PrecioMedioVenta,
    decimal? PrecioPropuesto, string? Aviso);

public sealed record DatosRendimiento(Guid ProductoId, Guid? EnvaseProductoId, decimal CajasHora);

public sealed record RendimientoDto(Guid Id, Guid ProductoId, Guid? EnvaseProductoId, decimal CajasHora, decimal SegundosCaja)
{
    public static RendimientoDto De(RendimientoConfeccion r) => new(r.Id, r.ProductoId, r.EnvaseProductoId, r.CajasHora, decimal.Round(3600m / r.CajasHora, 2));
}

public sealed record DatosConcepto(string? Codigo, string? Nombre, TipoConceptoLiquidacion Tipo, decimal Valor, bool Activo = true, Guid? AgricultorId = null,
    Guid? ProductoId = null, Guid? EnvaseProductoId = null, bool Abono = false);

public sealed record DatosTarifa(RecursoCoste Recurso, string? Categoria, TipoHora TipoHora, DateOnly Desde, DateOnly? Hasta, decimal CosteUnitario);

/// <summary>Maestros del módulo agro: campañas, agricultores, parcelas, categorías, precios, descuentos y tarifas.</summary>
public sealed class MaestrosAgro
{
    /// <summary>Retención de IRPF por defecto de las actividades agrícolas en estimación objetiva.</summary>
    public const decimal RetencionAgricola = 2m;

    private readonly IRepositorioAgro _repo;
    private readonly IUnidadDeTrabajoAgro _unidad;
    private readonly IConsultaProveedores _proveedores;

    private readonly IComprobadorUso? _uso;
    private readonly IVentasAgro? _ventas;
    private readonly AlxorCore.Catalogo.Aplicacion.IConsultaProductos? _productos;

    public MaestrosAgro(IRepositorioAgro repo, IUnidadDeTrabajoAgro unidad, IConsultaProveedores proveedores, IComprobadorUso? uso = null, IVentasAgro? ventas = null,
        AlxorCore.Catalogo.Aplicacion.IConsultaProductos? productos = null, IImpuestoEmpresaAgro? impuesto = null)
    {
        _impuesto = impuesto;
        _ventas = ventas;
        _productos = productos;
        _repo = repo;
        _unidad = unidad;
        _proveedores = proveedores;
        _uso = uso;
    }

    private readonly IImpuestoEmpresaAgro? _impuesto;

    private async Task<AlxorCore.Nucleo.Comun.TipoImpuesto> ImpuestoAsync(Guid empresaId, CancellationToken ct) =>
        _impuesto is null ? AlxorCore.Nucleo.Comun.TipoImpuesto.Iva : await _impuesto.ImpuestoAsync(empresaId, ct).ConfigureAwait(false);

    private async Task<string?> UsoAsync(string tipo, Guid id, CancellationToken ct) =>
        _uso is null ? null : await _uso.BuscarUsoAsync(tipo, id, ct).ConfigureAwait(false);

    private async Task<Resultado<BajaDto>> EliminarAsync(object? entidad, Guid id, string tipo, string prefijo, string nombre, string alternativa,
        Func<Task>? antes, CancellationToken ct)
    {
        if (entidad is null)
        {
            return Resultado.Fallo<BajaDto>(Error.NoEncontrado(prefijo + ".no_encontrado", $"No existe {nombre}."));
        }

        if (await UsoAsync(tipo, id, ct).ConfigureAwait(false) is { } uso)
        {
            return Resultado.Fallo<BajaDto>(Error.Conflicto(prefijo + Bajas.SufijoEnUso, $"No se puede eliminar {nombre} porque ya tiene {uso}. {alternativa}"));
        }

        if (antes is not null)
        {
            await antes().ConfigureAwait(false);
        }

        _repo.Eliminar(entidad);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new BajaDto(id, true, false));
    }

    // ------------------------------------------------------------------ Modificar y eliminar maestros
    public async Task<Resultado<CampanaDto>> ActualizarCampanaAsync(Guid id, DatosCampana datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var c = await _repo.CampanaAsync(id, ct).ConfigureAwait(false);
        if (c is null)
        {
            return Resultado.Fallo<CampanaDto>(Error.NoEncontrado("campana.no_encontrada", "La campaña no existe."));
        }

        var otras = (await _repo.CampanasAsync(c.EmpresaId, ct).ConfigureAwait(false)).Where(x => x.Id != id);
        if (otras.Any(e => e.Desde <= datos.Hasta && datos.Desde <= e.Hasta))
        {
            return Resultado.Fallo<CampanaDto>(Error.Conflicto("campana.solapada", "Las fechas se solapan con otra campaña."));
        }

        var r = c.Actualizar(datos.Nombre, datos.Desde, datos.Hasta, await UsoAsync(TiposRegistro.Campana, id, ct).ConfigureAwait(false) is not null);
        if (r.EsFallo)
        {
            return Resultado.Fallo<CampanaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(CampanaDto.De(c));
    }

    /// <summary>Elimina una campaña sin recepciones, partidas, partes ni liquidaciones (con sus artículos y precios).</summary>
    public async Task<Resultado<BajaDto>> EliminarCampanaAsync(Guid id, CancellationToken ct = default) =>
        await EliminarAsync(await _repo.CampanaAsync(id, ct).ConfigureAwait(false), id, TiposRegistro.Campana, "campana", "la campaña",
            "Una campaña con movimientos se conserva como histórico.", async () =>
            {
                foreach (var a in await _repo.ArticulosCampanaAsync(id, ct).ConfigureAwait(false))
                {
                    _repo.Eliminar(a);
                }

                foreach (var p in await _repo.PreciosAsync(id, ct).ConfigureAwait(false))
                {
                    _repo.Eliminar(p);
                }
            }, ct).ConfigureAwait(false);

    public async Task<Resultado<CategoriaDto>> ActualizarCategoriaAsync(Guid id, DatosCategoria datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var c = await _repo.CategoriaAsync(id, ct).ConfigureAwait(false);
        if (c is null)
        {
            return Resultado.Fallo<CategoriaDto>(Error.NoEncontrado("categoria.no_encontrada", "La categoría no existe."));
        }

        var r = c.Actualizar(datos.Nombre, datos.EsDestrio, datos.Orden, await UsoAsync(TiposRegistro.Categoria, id, ct).ConfigureAwait(false) is not null);
        if (r.EsFallo)
        {
            return Resultado.Fallo<CategoriaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(CategoriaDto.De(c));
    }

    public async Task<Resultado<BajaDto>> EliminarCategoriaAsync(Guid id, CancellationToken ct = default) =>
        await EliminarAsync(await _repo.CategoriaAsync(id, ct).ConfigureAwait(false), id, TiposRegistro.Categoria, "categoria", "la categoría",
            "Se conserva porque forma parte de clasificaciones o liquidaciones.", null, ct).ConfigureAwait(false);

    public async Task<Resultado<TarifaDto>> ActualizarTarifaAsync(Guid id, DatosTarifa datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var t = await _repo.TarifaAsync(id, ct).ConfigureAwait(false);
        if (t is null)
        {
            return Resultado.Fallo<TarifaDto>(Error.NoEncontrado("tarifa.no_encontrada", "La tarifa no existe."));
        }

        var hasta = datos.Hasta ?? DateOnly.MaxValue;
        var solapa = (await _repo.TarifasAsync(t.EmpresaId, ct).ConfigureAwait(false)).Any(x => x.Id != id &&
            x.Recurso == t.Recurso && x.Categoria == t.Categoria && x.TipoHora == t.TipoHora &&
            x.Desde <= hasta && datos.Desde <= (x.Hasta ?? DateOnly.MaxValue));
        if (solapa)
        {
            return Resultado.Fallo<TarifaDto>(Error.Conflicto("tarifa.solapada", "Se solaparía con otra tarifa de esa categoría y tipo de hora."));
        }

        var r = t.Actualizar(datos.Desde, datos.Hasta, datos.CosteUnitario, await UsoAsync(TiposRegistro.TarifaCoste, id, ct).ConfigureAwait(false) is not null);
        if (r.EsFallo)
        {
            return Resultado.Fallo<TarifaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(TarifaDto.De(t));
    }

    public async Task<Resultado<BajaDto>> EliminarTarifaAsync(Guid id, CancellationToken ct = default) =>
        await EliminarAsync(await _repo.TarifaAsync(id, ct).ConfigureAwait(false), id, TiposRegistro.TarifaCoste, "tarifa", "la tarifa",
            "Cierra su vigencia en lugar de eliminarla.", null, ct).ConfigureAwait(false);

    public async Task<Resultado<BajaDto>> EliminarParcelaAsync(Guid id, CancellationToken ct = default) =>
        await EliminarAsync(await _repo.ParcelaAsync(id, ct).ConfigureAwait(false), id, TiposRegistro.Parcela, "parcela", "la parcela",
            "Se conserva por la trazabilidad de lo recolectado.", null, ct).ConfigureAwait(false);

    /// <summary>Elimina la ficha agrícola (y sus parcelas sin uso) de un agricultor sin recepciones, envases ni liquidaciones.</summary>
    public async Task<Resultado<BajaDto>> EliminarAgricultorAsync(Guid id, CancellationToken ct = default)
    {
        var a = await _repo.AgricultorAsync(id, ct).ConfigureAwait(false);
        return await EliminarAsync(a, id, TiposRegistro.Agricultor, "agricultor", "el agricultor",
            "Bloquéalo en su ficha para que no se le reciba fruta.", async () =>
            {
                foreach (var p in await _repo.ParcelasAsync(a!.EmpresaId, id, ct).ConfigureAwait(false))
                {
                    if (await UsoAsync(TiposRegistro.Parcela, p.Id, ct).ConfigureAwait(false) is null)
                    {
                        _repo.Eliminar(p);
                    }
                }
            }, ct).ConfigureAwait(false);
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
        var a = Agricultor.Crear(empresaId, proveedor.Id, proveedor.Nombre, datos.Regimen, retencion, datos.AutofacturacionDesde, datos.CodigoImpuesto,
            await ImpuestoAsync(empresaId, ct).ConfigureAwait(false));
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

        var r = a.Actualizar(datos.Regimen, datos.PorcentajeRetencion ?? a.PorcentajeRetencion, datos.AutofacturacionDesde, datos.MotivoBloqueo, datos.CodigoImpuesto,
            await ImpuestoAsync(a.EmpresaId, ct).ConfigureAwait(false));
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

        var p = PrecioLiquidacion.Crear(empresaId, campanaId, datos.ProductoId, datos.CategoriaId, datos.Desde, datos.Hasta, datos.PrecioKg, datos.Tipo, datos.EnvaseProductoId);
        if (p.EsFallo)
        {
            return Resultado.Fallo<PrecioDto>(p.Error);
        }

        if (!campana.Contiene(datos.Desde) || !campana.Contiene(datos.Hasta))
        {
            return Resultado.Fallo<PrecioDto>(Error.Validacion("precio.fuera_campana", "El periodo del precio debe estar dentro de la campaña."));
        }

        var solapa = (await _repo.PreciosAsync(campanaId, ct).ConfigureAwait(false))
            .Any(x => x.Solapa(datos.ProductoId, datos.CategoriaId, datos.EnvaseProductoId, datos.Tipo, datos.Desde, datos.Hasta));
        if (solapa)
        {
            return Resultado.Fallo<PrecioDto>(Error.Conflicto("precio.solapado", "Ya hay un precio de ese tipo para el artículo, la categoría y el envase en esas fechas."));
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
            .Any(x => x.Id != p.Id && x.Solapa(p.ProductoId, p.CategoriaId, p.EnvaseProductoId, p.Tipo, datos.Desde, datos.Hasta));
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

    /// <summary>
    /// Fijación masiva: da de alta (o, con sustituir, actualiza el de la misma clave y fechas) muchos precios de la campaña
    /// de una vez. Los que no se pueden se devuelven con su motivo; los demás se guardan juntos.
    /// </summary>
    public async Task<Resultado<ResultadoPreciosMasivosDto>> FijarPreciosAsync(Guid empresaId, Guid campanaId, DatosPreciosMasivos datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var campana = await _repo.CampanaAsync(campanaId, ct).ConfigureAwait(false);
        if (campana is null)
        {
            return Resultado.Fallo<ResultadoPreciosMasivosDto>(Error.NoEncontrado("campana.no_encontrada", "La campaña no existe."));
        }

        if (datos.Precios is not { Count: > 0 })
        {
            return Resultado.Fallo<ResultadoPreciosMasivosDto>(Error.Validacion("precio.sin_precios", "Indica al menos un precio."));
        }

        var existentes = (await _repo.PreciosAsync(campanaId, ct).ConfigureAwait(false)).ToList();
        var nuevos = new List<PrecioLiquidacion>();
        var errores = new List<ErrorDto>();
        var actualizados = 0;
        foreach (var (d, i) in datos.Precios.Select((d, i) => (d, i + 1)))
        {
            if (!campana.Contiene(d.Desde) || !campana.Contiene(d.Hasta))
            {
                errores.Add(new ErrorDto("precio.fuera_campana", $"Fila {i}: el periodo debe estar dentro de la campaña."));
                continue;
            }

            var igual = existentes.FirstOrDefault(x => x.ProductoId == d.ProductoId && x.CategoriaId == d.CategoriaId && x.EnvaseProductoId == d.EnvaseProductoId
                                                        && x.Tipo == d.Tipo && x.Desde == d.Desde && x.Hasta == d.Hasta);
            if (igual is not null && datos.Sustituir)
            {
                if (igual.PrecioKg == d.PrecioKg)
                {
                    continue;
                }

                if (await _repo.PrecioEnUsoAsync(igual.Id, ct).ConfigureAwait(false))
                {
                    errores.Add(new ErrorDto("precio.en_uso", $"Fila {i}: el precio actual ya se usa en una liquidación: cámbialo a mano."));
                    continue;
                }

                var tracked = await _repo.PrecioAsync(igual.Id, ct).ConfigureAwait(false);
                var r = tracked!.Actualizar(d.Desde, d.Hasta, d.PrecioKg);
                if (r.EsFallo)
                {
                    errores.Add(new ErrorDto(r.Error.Codigo, $"Fila {i}: {r.Error.Mensaje}"));
                    continue;
                }

                actualizados++;
                continue;
            }

            if (existentes.Concat(nuevos).Any(x => x.Solapa(d.ProductoId, d.CategoriaId, d.EnvaseProductoId, d.Tipo, d.Desde, d.Hasta)))
            {
                errores.Add(new ErrorDto("precio.solapado", $"Fila {i}: ya hay un precio de ese tipo para el artículo, la categoría y el envase en esas fechas."));
                continue;
            }

            var p = PrecioLiquidacion.Crear(empresaId, campanaId, d.ProductoId, d.CategoriaId, d.Desde, d.Hasta, d.PrecioKg, d.Tipo, d.EnvaseProductoId);
            if (p.EsFallo)
            {
                errores.Add(new ErrorDto(p.Error.Codigo, $"Fila {i}: {p.Error.Mensaje}"));
                continue;
            }

            nuevos.Add(p.Valor);
            _repo.Agregar(p.Valor);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new ResultadoPreciosMasivosDto(nuevos.Count, actualizados, errores));
    }

    /// <summary>
    /// Liquidación a resultas, como la valoración de compras según ventas de Hispatec: por artículo de la campaña, el precio
    /// medio al que se vendió en las fechas (albaranes valorados y facturas sin albarán) menos la deducción. No guarda nada:
    /// la propuesta se fija después con la fijación masiva.
    /// </summary>
    public async Task<Resultado<IReadOnlyList<PropuestaPrecioVentaDto>>> ProponerDesdeVentasAsync(Guid campanaId, DatosPropuestaVentas datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        if (_ventas is null)
        {
            return Resultado.Fallo<IReadOnlyList<PropuestaPrecioVentaDto>>(Error.Validacion("precio.sin_ventas", "No hay acceso a las ventas."));
        }

        if (datos.Hasta < datos.Desde || datos.DeduccionKg < 0 || datos.DeduccionPorcentaje is < 0 or > 100)
        {
            return Resultado.Fallo<IReadOnlyList<PropuestaPrecioVentaDto>>(Error.Validacion("precio.propuesta", "Revisa las fechas y las deducciones (no negativas, porcentaje hasta 100)."));
        }

        var articulos = await _repo.ArticulosCampanaAsync(campanaId, ct).ConfigureAwait(false);
        var lista = new List<PropuestaPrecioVentaDto>();
        foreach (var a in articulos.Where(a => datos.ProductoIds is not { Count: > 0 } || datos.ProductoIds.Contains(a.ProductoId)))
        {
            var nombre = _productos is null ? null : (await _productos.ObtenerAsync(a.ProductoId, ct).ConfigureAwait(false))?.Nombre;
            var (kilos, importe) = await _ventas.VentasAsync(a.ProductoId, datos.Desde, datos.Hasta, ct).ConfigureAwait(false);
            decimal? medio = kilos > 0 ? decimal.Round(importe / kilos, 6) : null;
            decimal? propuesto = medio is { } m ? decimal.Round(Math.Max(0m, m * (1 - datos.DeduccionPorcentaje / 100m) - datos.DeduccionKg), 4) : null;
            var aviso = kilos <= 0 ? "Sin ventas valoradas en esas fechas."
                : a.Metodo == MetodoLiquidacion.PorClasificacion ? "Se liquida por clasificación: la propuesta es para todas las categorías (ajústala por categoría)." : null;
            lista.Add(new PropuestaPrecioVentaDto(a.ProductoId, nombre, a.Metodo.ToString(), decimal.Round(kilos, 3), decimal.Round(importe, 2), medio, propuesto, aviso));
        }

        return Resultado.Ok<IReadOnlyList<PropuestaPrecioVentaDto>>(lista);
    }

    // ------------------------------------------------------------------ Rendimientos de confección
    public async Task<IReadOnlyList<RendimientoDto>> RendimientosAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.RendimientosAsync(empresaId, ct).ConfigureAwait(false)).Select(RendimientoDto.De).ToList();

    /// <summary>Da de alta (o cambia, si ya existe para ese producto y envase) el rendimiento en cajas por hora.</summary>
    public async Task<Resultado<RendimientoDto>> GuardarRendimientoAsync(Guid empresaId, DatosRendimiento datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var existente = (await _repo.RendimientosAsync(empresaId, ct).ConfigureAwait(false))
            .FirstOrDefault(r => r.ProductoId == datos.ProductoId && r.EnvaseProductoId == datos.EnvaseProductoId);
        if (existente is not null)
        {
            var c = existente.Cambiar(datos.CajasHora);
            if (c.EsFallo)
            {
                return Resultado.Fallo<RendimientoDto>(c.Error);
            }

            await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
            return Resultado.Ok(RendimientoDto.De(existente));
        }

        var r = RendimientoConfeccion.Crear(empresaId, datos.ProductoId, datos.EnvaseProductoId, datos.CajasHora);
        if (r.EsFallo)
        {
            return Resultado.Fallo<RendimientoDto>(r.Error);
        }

        _repo.Agregar(r.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(RendimientoDto.De(r.Valor));
    }

    public async Task<Resultado> EliminarRendimientoAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _repo.RendimientoAsync(id, ct).ConfigureAwait(false);
        if (r is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("rendimiento.no_encontrado", "El rendimiento no existe."));
        }

        _repo.Eliminar(r);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
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
        if (datos.AgricultorId is { } ag && await _repo.AgricultorAsync(ag, ct).ConfigureAwait(false) is null)
        {
            return Resultado.Fallo<ConceptoDto>(Error.NoEncontrado("agricultor.no_encontrado", "El agricultor no existe."));
        }

        var c = ConceptoLiquidacion.Crear(empresaId, datos.Codigo, datos.Nombre, datos.Tipo, datos.Valor, datos.AgricultorId, datos.ProductoId, datos.EnvaseProductoId,
            datos.Abono);
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
        return c is null ? new ConfiguracionAgroDto(ConfiguracionAgro.PrefijoPruebas, 0, false, false)
            : new ConfiguracionAgroDto(c.PrefijoGs1, c.DigitoExtension, c.ReflejarPartidasEnInventario, c.ReflejarEnvasesEnInventario, c.ToleranciaMermaPct);
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

        c.FijarInventario(datos.ReflejarPartidasEnInventario ?? c.ReflejarPartidasEnInventario, datos.ReflejarEnvasesEnInventario ?? c.ReflejarEnvasesEnInventario);
        var tolerancia = c.FijarToleranciaMerma(datos.ToleranciaMermaPct);
        if (tolerancia.EsFallo)
        {
            return Resultado.Fallo<ConfiguracionAgroDto>(tolerancia.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new ConfiguracionAgroDto(c.PrefijoGs1, c.DigitoExtension, c.ReflejarPartidasEnInventario, c.ReflejarEnvasesEnInventario, c.ToleranciaMermaPct));
    }
}
