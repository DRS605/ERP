using AlxorCore.Catalogo.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Catalogo.Aplicacion;

public sealed record AsignacionConceptoDto(Guid? TerceroId, Guid? FamiliaId, Guid? ProductoId, decimal? Valor);

public sealed record ConceptoLineaDto(
    Guid Id, string Codigo, string Nombre, string? TextoDocumento, string Ambito, string Efecto, string Sentido, string Calculo, decimal Valor, string Reparto,
    bool Activo, IReadOnlyList<AsignacionConceptoDto> Asignaciones)
{
    public static ConceptoLineaDto Desde(ConceptoLinea c) => new(c.Id, c.Codigo, c.Nombre, c.TextoDocumento, c.Ambito.ToString(), c.Efecto.ToString(),
        c.Sentido.ToString(), c.Calculo.ToString(), c.Valor, c.Reparto.ToString(), c.Activo,
        c.Asignaciones.Select(a => new AsignacionConceptoDto(a.TerceroId, a.FamiliaId, a.ProductoId, a.Valor)).ToList());
}

/// <summary>Alta de un concepto de línea.</summary>
public sealed record CrearConceptoComando(string Codigo, DatosConcepto Datos);

/// <summary>Modificación de un concepto de línea (sustituye sus asignaciones).</summary>
public sealed record ActualizarConceptoComando(DatosConcepto Datos, bool Activo = true);

/// <summary>Concepto pedido para una línea o para todo el documento. Sin valor, se usa el de su asignación o el del concepto.</summary>
public sealed record ConceptoSolicitado(Guid ConceptoId, decimal? Valor = null);

/// <summary>
/// Línea a la que poner conceptos. <see cref="BaseBruta"/> es su importe tras el descuento y antes de conceptos.
/// Con <see cref="Conceptos"/> se ponen exactamente esos; con <see cref="Copiados"/> (al pasar un documento a otro)
/// se recalculan los de la línea de origen; si no hay ninguno de los dos, se ponen los automáticos.
/// </summary>
public sealed record LineaConceptos(
    Guid? ProductoId, decimal Cantidad, decimal BaseBruta, IReadOnlyList<ConceptoSolicitado>? Conceptos = null, IReadOnlyList<ConceptoAplicado>? Copiados = null);

/// <summary>Concepto que se pondría solo en una línea (para que la interfaz lo muestre antes de guardar).</summary>
public sealed record ConceptoSugeridoDto(Guid ConceptoId, string Codigo, string Nombre, string Efecto, string Sentido, string Calculo, decimal Valor);

public interface IRepositorioConceptosLinea
{
    Task<ConceptoLinea?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct = default);

    Task<IReadOnlyList<ConceptoLinea>> ListarAsync(CancellationToken ct = default);

    void Agregar(ConceptoLinea concepto);

    void Eliminar(ConceptoLinea concepto);
}

/// <summary>Dice si un concepto se ha usado en algún documento del grupo (los documentos guardan copia del concepto).</summary>
public interface IUsoConceptosLinea
{
    Task<bool> EnUsoAsync(Guid conceptoId, CancellationToken ct = default);
}

/// <summary>
/// Pone los conceptos a las líneas de un documento de venta o de compra y calcula su importe. Lo usan Facturación
/// (presupuestos, pedidos y facturas) y Compras (pedidos).
/// </summary>
public interface IResolverConceptos
{
    Task<Resultado<IReadOnlyList<IReadOnlyList<ConceptoAplicado>>>> ResolverAsync(
        AmbitoConcepto ambito, Guid? terceroId, IReadOnlyList<LineaConceptos> lineas, IReadOnlyList<ConceptoSolicitado>? documento = null,
        bool automaticos = true, CancellationToken ct = default);

    Task<IReadOnlyList<ConceptoSugeridoDto>> SugeridosAsync(AmbitoConcepto ambito, Guid? terceroId, Guid? productoId, CancellationToken ct = default);
}

/// <summary>Alta, modificación, consulta y baja de los conceptos de línea.</summary>
public sealed class GestionConceptosLinea
{
    private readonly IRepositorioConceptosLinea _conceptos;
    private readonly IConsultaProductos _productos;
    private readonly IRepositorioFamilias _familias;
    private readonly IUsoConceptosLinea _uso;
    private readonly IUnidadDeTrabajoCatalogo _unidad;
    private readonly IReloj _reloj;

    public GestionConceptosLinea(IRepositorioConceptosLinea conceptos, IConsultaProductos productos, IRepositorioFamilias familias, IUsoConceptosLinea uso,
        IUnidadDeTrabajoCatalogo unidad, IReloj reloj)
    {
        _conceptos = conceptos;
        _productos = productos;
        _familias = familias;
        _uso = uso;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<IReadOnlyList<ConceptoLineaDto>> ListarAsync(AmbitoConcepto? ambito = null, bool soloActivos = false, CancellationToken ct = default) =>
        (await _conceptos.ListarAsync(ct).ConfigureAwait(false))
            .Where(c => (ambito is not { } a || c.ValeEn(a)) && (!soloActivos || c.Activo))
            .Select(ConceptoLineaDto.Desde).ToList();

    public async Task<Resultado<ConceptoLineaDto>> ObtenerAsync(Guid id, CancellationToken ct = default) =>
        await _conceptos.ObtenerPorIdAsync(id, ct).ConfigureAwait(false) is { } c
            ? Resultado.Ok(ConceptoLineaDto.Desde(c))
            : Resultado.Fallo<ConceptoLineaDto>(NoEncontrado());

    public async Task<Resultado<ConceptoLineaDto>> CrearAsync(Guid grupoId, CrearConceptoComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var concepto = ConceptoLinea.Crear(grupoId, comando.Codigo, comando.Datos, _reloj);
        if (concepto.EsFallo)
        {
            return Resultado.Fallo<ConceptoLineaDto>(concepto.Error);
        }

        if (await _conceptos.ExisteCodigoAsync(concepto.Valor.Codigo, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<ConceptoLineaDto>(Error.Conflicto("concepto.codigo_duplicado", $"Ya existe un concepto con el código {concepto.Valor.Codigo}."));
        }

        if (await ReferenciasAsync(comando.Datos, ct).ConfigureAwait(false) is { } error)
        {
            return Resultado.Fallo<ConceptoLineaDto>(error);
        }

        _conceptos.Agregar(concepto.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ConceptoLineaDto.Desde(concepto.Valor));
    }

    public async Task<Resultado<ConceptoLineaDto>> ActualizarAsync(Guid id, ActualizarConceptoComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var concepto = await _conceptos.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (concepto is null)
        {
            return Resultado.Fallo<ConceptoLineaDto>(NoEncontrado());
        }

        if (await ReferenciasAsync(comando.Datos, ct).ConfigureAwait(false) is { } error)
        {
            return Resultado.Fallo<ConceptoLineaDto>(error);
        }

        var r = concepto.Actualizar(comando.Datos, comando.Activo, _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<ConceptoLineaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ConceptoLineaDto.Desde(concepto));
    }

    /// <summary>Elimina el concepto si ningún documento lo usa; si ya se usó, lo da de baja (los documentos conservan su copia).</summary>
    public async Task<Resultado<BajaDto>> EliminarAsync(Guid id, CancellationToken ct = default)
    {
        var concepto = await _conceptos.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (concepto is null)
        {
            return Resultado.Fallo<BajaDto>(NoEncontrado());
        }

        var enUso = await _uso.EnUsoAsync(id, ct).ConfigureAwait(false);
        if (enUso)
        {
            concepto.Desactivar(_reloj);
        }
        else
        {
            _conceptos.Eliminar(concepto);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new BajaDto(id, !enUso, false));
    }

    private async Task<Error?> ReferenciasAsync(DatosConcepto datos, CancellationToken ct)
    {
        foreach (var (a, i) in (datos?.Asignaciones ?? []).Select((a, i) => (a, i + 1)))
        {
            if (a.ProductoId is { } p && await _productos.ObtenerAsync(p, ct).ConfigureAwait(false) is null)
            {
                return Error.Validacion("concepto.producto_no_encontrado", $"El artículo de la asignación {i} no existe.");
            }

            if (a.FamiliaId is { } f && await _familias.ObtenerPorIdAsync(f, ct).ConfigureAwait(false) is null)
            {
                return Error.Validacion("concepto.familia_no_encontrada", $"La familia de la asignación {i} no existe.");
            }
        }

        return null;
    }

    private static Error NoEncontrado() => Error.NoEncontrado("concepto.no_encontrado", "El concepto de línea no existe.");
}

/// <summary>Implementación de <see cref="IResolverConceptos"/>.</summary>
public sealed class ResolverConceptos : IResolverConceptos
{
    private readonly IRepositorioConceptosLinea _conceptos;
    private readonly IConsultaProductos _productos;
    private readonly IRepositorioFamilias _familias;

    public ResolverConceptos(IRepositorioConceptosLinea conceptos, IConsultaProductos productos, IRepositorioFamilias familias)
    {
        _conceptos = conceptos;
        _productos = productos;
        _familias = familias;
    }

    public async Task<Resultado<IReadOnlyList<IReadOnlyList<ConceptoAplicado>>>> ResolverAsync(
        AmbitoConcepto ambito, Guid? terceroId, IReadOnlyList<LineaConceptos> lineas, IReadOnlyList<ConceptoSolicitado>? documento = null,
        bool automaticos = true, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(lineas);
        var todos = (await _conceptos.ListarAsync(ct).ConfigureAwait(false)).ToDictionary(c => c.Id);
        var resultado = lineas.Select(_ => new List<ConceptoAplicado>()).ToList();
        var nadaQuePedir = lineas.All(l => l.Conceptos is not { Count: > 0 } && l.Copiados is not { Count: > 0 }) && documento is not { Count: > 0 };
        if (todos.Count == 0 && nadaQuePedir)
        {
            return Resultado.Ok<IReadOnlyList<IReadOnlyList<ConceptoAplicado>>>(resultado);
        }

        var familias = await CadenasAsync(todos.Values.FirstOrDefault()?.GrupoId, ct).ConfigureAwait(false);
        var datos = new List<(Guid? ProductoId, IReadOnlyList<Guid> Familias, decimal? Kilos)>();
        foreach (var l in lineas)
        {
            var producto = l.ProductoId is { } p ? await _productos.ObtenerAsync(p, ct).ConfigureAwait(false) : null;
            datos.Add((l.ProductoId, familias(producto?.FamiliaId), Kilos(producto, l.Cantidad)));
        }

        for (var i = 0; i < lineas.Count; i++)
        {
            var l = lineas[i];
            if (l.Copiados is not null)
            {
                resultado[i].AddRange(l.Copiados.Select(c => c.Calculo == CalculoConcepto.Importe && c.Repartido
                    ? c : ConceptosLinea.Recalcular(c, l.BaseBruta, l.Cantidad, datos[i].Kilos)));
            }
            else if (l.Conceptos is not null)
            {
                foreach (var s in l.Conceptos)
                {
                    var c = Buscar(todos, s.ConceptoId, ambito);
                    if (c.EsFallo)
                    {
                        return Resultado.Fallo<IReadOnlyList<IReadOnlyList<ConceptoAplicado>>>(c.Error);
                    }

                    var valor = s.Valor ?? c.Valor.AsignacionPara(terceroId, l.ProductoId, datos[i].Familias)?.Valor ?? c.Valor.Valor;
                    if (ConceptoLinea.ErrorValor(valor, c.Valor.Calculo) is { } e)
                    {
                        return Resultado.Fallo<IReadOnlyList<IReadOnlyList<ConceptoAplicado>>>(Error.Validacion(e.Codigo, $"Línea {i + 1}, {c.Valor.Codigo}: {e.Mensaje}"));
                    }

                    resultado[i].Add(Aplicar(c.Valor, valor, l.BaseBruta, l.Cantidad, datos[i].Kilos, false));
                }
            }
            else if (automaticos)
            {
                foreach (var c in todos.Values.Where(c => c.Activo && c.ValeEn(ambito)).OrderBy(c => c.Codigo, StringComparer.Ordinal))
                {
                    // Un concepto por kilo no se pone solo en una línea sin peso conocido (quedaría a cero).
                    if (c.AsignacionPara(terceroId, l.ProductoId, datos[i].Familias) is { } a && (c.Calculo != CalculoConcepto.PorKilo || datos[i].Kilos is not null))
                    {
                        resultado[i].Add(Aplicar(c, a.Valor ?? c.Valor, l.BaseBruta, l.Cantidad, datos[i].Kilos, false));
                    }
                }
            }
        }

        foreach (var s in documento ?? [])
        {
            var c = Buscar(todos, s.ConceptoId, ambito);
            if (c.EsFallo)
            {
                return Resultado.Fallo<IReadOnlyList<IReadOnlyList<ConceptoAplicado>>>(c.Error);
            }

            var concepto = c.Valor;
            var valor = s.Valor ?? concepto.Valor;
            if (ConceptoLinea.ErrorValor(valor, concepto.Calculo) is { } e)
            {
                return Resultado.Fallo<IReadOnlyList<IReadOnlyList<ConceptoAplicado>>>(Error.Validacion(e.Codigo, $"{concepto.Codigo}: {e.Mensaje}"));
            }

            if (concepto.Calculo == CalculoConcepto.Importe)
            {
                var pesos = lineas.Select((l, i) => concepto.Reparto switch
                {
                    RepartoConcepto.PorCantidad => l.Cantidad,
                    RepartoConcepto.PorPeso => datos[i].Kilos ?? 0m,
                    _ => l.BaseBruta,
                }).ToList();
                var partes = ConceptosLinea.Repartir(valor, pesos);
                for (var i = 0; i < lineas.Count; i++)
                {
                    resultado[i].Add(Aplicar(concepto, partes[i], lineas[i].BaseBruta, lineas[i].Cantidad, datos[i].Kilos, true));
                }
            }
            else
            {
                for (var i = 0; i < lineas.Count; i++)
                {
                    resultado[i].Add(Aplicar(concepto, valor, lineas[i].BaseBruta, lineas[i].Cantidad, datos[i].Kilos, true));
                }
            }
        }

        for (var i = 0; i < lineas.Count; i++)
        {
            if (lineas[i].BaseBruta + ConceptosLinea.SumaPrecio(resultado[i]) < 0m)
            {
                return Resultado.Fallo<IReadOnlyList<IReadOnlyList<ConceptoAplicado>>>(Error.Validacion("concepto.linea_negativa",
                    $"Los conceptos dejan la línea {i + 1} con importe negativo."));
            }
        }

        return Resultado.Ok<IReadOnlyList<IReadOnlyList<ConceptoAplicado>>>(resultado);
    }

    public async Task<IReadOnlyList<ConceptoSugeridoDto>> SugeridosAsync(AmbitoConcepto ambito, Guid? terceroId, Guid? productoId, CancellationToken ct = default)
    {
        var todos = (await _conceptos.ListarAsync(ct).ConfigureAwait(false)).Where(c => c.Activo && c.ValeEn(ambito)).ToList();
        if (todos.Count == 0)
        {
            return [];
        }

        var producto = productoId is { } p ? await _productos.ObtenerAsync(p, ct).ConfigureAwait(false) : null;
        var cadena = (await CadenasAsync(todos[0].GrupoId, ct).ConfigureAwait(false))(producto?.FamiliaId);
        return todos.OrderBy(c => c.Codigo, StringComparer.Ordinal)
            .Select(c => (c, a: c.AsignacionPara(terceroId, productoId, cadena)))
            .Where(x => x.a is not null)
            .Select(x => new ConceptoSugeridoDto(x.c.Id, x.c.Codigo, x.c.TextoDocumento ?? x.c.Nombre, x.c.Efecto.ToString(), x.c.Sentido.ToString(), x.c.Calculo.ToString(),
                x.a!.Valor ?? x.c.Valor))
            .ToList();
    }

    /// <summary>Kilos netos de la línea: la cantidad si se vende por kilos, o el peso del artículo por la cantidad.</summary>
    private static decimal? Kilos(ProductoDto? producto, decimal cantidad) =>
        producto is null ? null
        : string.Equals(producto.Unidad, "kg", StringComparison.OrdinalIgnoreCase) ? cantidad
        : producto.PesoKg is { } kg ? kg * cantidad : null;

    private static ConceptoAplicado Aplicar(ConceptoLinea c, decimal valor, decimal baseLinea, decimal cantidad, decimal? kilos, bool repartido) =>
        new(c.Id, c.Codigo, c.TextoDocumento ?? c.Nombre, c.Efecto, c.Sentido, c.Calculo, valor,
            ConceptosLinea.Calcular(c.Calculo, c.Sentido, valor, baseLinea, cantidad, kilos), repartido);

    private static Resultado<ConceptoLinea> Buscar(Dictionary<Guid, ConceptoLinea> todos, Guid id, AmbitoConcepto ambito)
    {
        if (!todos.TryGetValue(id, out var c))
        {
            return Resultado.Fallo<ConceptoLinea>(Error.Validacion("concepto.no_encontrado", "Uno de los conceptos de línea no existe."));
        }

        return c.ValeEn(ambito)
            ? Resultado.Ok(c)
            : Resultado.Fallo<ConceptoLinea>(Error.Validacion("concepto.ambito",
                $"El concepto {c.Codigo} es solo de {(c.Ambito == AmbitoConcepto.Ventas ? "ventas" : "compras")}."));
    }

    /// <summary>Función que da la cadena de familias de un artículo (de la más cercana a la raíz).</summary>
    private async Task<Func<Guid?, IReadOnlyList<Guid>>> CadenasAsync(Guid? grupoId, CancellationToken ct)
    {
        var porId = grupoId is { } g ? (await _familias.ListarTodasAsync(g, ct).ConfigureAwait(false)).ToDictionary(f => f.Id) : [];
        return familiaId =>
        {
            var cadena = new List<Guid>();
            for (Guid? f = familiaId; f is { } actual && porId.TryGetValue(actual, out var fam) && !cadena.Contains(actual); f = fam.PadreId)
            {
                cadena.Add(actual);
            }

            return cadena;
        };
    }
}
