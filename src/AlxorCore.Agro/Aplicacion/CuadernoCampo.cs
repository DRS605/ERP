using AlxorCore.Agro.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Agro.Aplicacion;

public interface IRepositorioCuaderno
{
    void Agregar(TratamientoParcela tratamiento);

    Task<TratamientoParcela?> ObtenerAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<TratamientoParcela>> ListarAsync(Guid empresaId, IReadOnlyCollection<Guid>? parcelas, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);
}

public sealed record DatosTratamiento(Guid ParcelaId, DateOnly Fecha, string? Producto, int PlazoSeguridadDias, string? NumeroRegistro = null, string? MateriaActiva = null,
    string? Motivo = null, decimal? Dosis = null, string? UnidadDosis = null, decimal? SuperficieTratadaHa = null, string? Aplicador = null, string? Observaciones = null,
    TipoLabor Tipo = TipoLabor.Fitosanitario, decimal? NitrogenoKgHa = null, decimal? FosforoKgHa = null, decimal? PotasioKgHa = null, decimal? VolumenM3 = null,
    Guid? FitosanitarioId = null, string? Cultivo = null, Guid? ArticuloId = null, Guid? AlmacenId = null, string? Lote = null, decimal? CantidadConsumida = null);

public sealed record TratamientoDto(Guid Id, Guid ParcelaId, string Parcela, Guid AgricultorId, DateOnly Fecha, string Producto, string? NumeroRegistro, string? MateriaActiva,
    string? Motivo, decimal? Dosis, string? UnidadDosis, decimal? SuperficieTratadaHa, int PlazoSeguridadDias, DateOnly RecolectableDesde, string? Aplicador,
    string? Observaciones, bool Anulado, string? MotivoAnulacion, string Tipo = "Fitosanitario", decimal? NitrogenoKgHa = null, decimal? FosforoKgHa = null,
    decimal? PotasioKgHa = null, decimal? VolumenM3 = null, Guid? FitosanitarioId = null, string? Cultivo = null, Guid? ArticuloId = null, Guid? AlmacenId = null,
    string? Lote = null, decimal? CantidadConsumida = null);

/// <summary>Resumen de una parcela en el cuaderno: unidades fertilizantes aportadas (kg/ha) y agua de riego.</summary>
public sealed record ResumenParcelaCuadernoDto(Guid ParcelaId, string Parcela, int Tratamientos, decimal NitrogenoKgHa, decimal FosforoKgHa, decimal PotasioKgHa,
    decimal RiegoM3);

/// <summary>Recolección (entrega) de una parcela en el cuaderno, con el aviso si fue dentro del plazo de seguridad de un tratamiento.</summary>
public sealed record RecoleccionCuadernoDto(Guid RecepcionId, string Recepcion, DateOnly Fecha, Guid ParcelaId, string Parcela, string Producto, decimal NetoKg,
    string? Incidencia);

public sealed record CuadernoCampoDto(Guid AgricultorId, string Agricultor, DateOnly? Desde, DateOnly? Hasta, IReadOnlyList<TratamientoDto> Tratamientos,
    IReadOnlyList<RecoleccionCuadernoDto> Recolecciones, int Incidencias, IReadOnlyList<ResumenParcelaCuadernoDto>? Parcelas = null);

/// <summary>
/// Cuaderno de campo del agricultor: los tratamientos fitosanitarios de sus parcelas y sus recolecciones (las entregas
/// recibidas), con las que se hicieron dentro de un plazo de seguridad. La recepción no se confirma si alguna línea se
/// recolectó dentro del plazo (<see cref="ComprobarPlazosAsync"/>).
/// </summary>
public sealed class CuadernoCampoAgro
{
    private readonly IRepositorioCuaderno _cuaderno;
    private readonly IRepositorioAgro _agro;
    private readonly IUnidadDeTrabajoAgro _unidad;
    private readonly IReloj _reloj;

    private readonly IRepositorioFitosanitarios? _registro;
    private readonly IInventarioAgro? _inventario;

    public CuadernoCampoAgro(IRepositorioCuaderno cuaderno, IRepositorioAgro agro, IUnidadDeTrabajoAgro unidad, IReloj reloj, IRepositorioFitosanitarios? registro = null,
        IInventarioAgro? inventario = null)
    {
        _cuaderno = cuaderno;
        _agro = agro;
        _unidad = unidad;
        _reloj = reloj;
        _registro = registro;
        _inventario = inventario;
    }

    public async Task<Resultado<TratamientoDto>> RegistrarAsync(Guid empresaId, DatosTratamiento datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var parcela = await _agro.ParcelaAsync(datos.ParcelaId, ct).ConfigureAwait(false);
        if (parcela is null)
        {
            return Resultado.Fallo<TratamientoDto>(Error.NoEncontrado("parcela.no_encontrada", "La parcela no existe."));
        }

        if (datos.SuperficieTratadaHa is { } sup && parcela.SuperficieHa is { } total && sup > total)
        {
            return Resultado.Fallo<TratamientoDto>(Error.Validacion("tratamiento.superficie", $"La superficie tratada supera la de la parcela ({total} ha)."));
        }

        // Producto del Registro Oficial: autorizado ese día, en un uso autorizado, a su dosis y con su plazo de seguridad.
        ProductoFitosanitario? fito = null;
        var plazo = datos.PlazoSeguridadDias;
        if (datos.FitosanitarioId is { } fitoId)
        {
            fito = _registro is null ? null : await _registro.ObtenerAsync(fitoId, ct).ConfigureAwait(false);
            if (fito is null)
            {
                return Resultado.Fallo<TratamientoDto>(Error.NoEncontrado("fitosanitario.no_encontrado", "El producto no está en el registro."));
            }

            var comprobado = await ComprobarRegistroAsync(empresaId, parcela, fito, datos, ct).ConfigureAwait(false);
            if (comprobado.EsFallo)
            {
                return Resultado.Fallo<TratamientoDto>(comprobado.Error);
            }

            plazo = Math.Max(plazo, comprobado.Valor);
        }

        var t = TratamientoParcela.Crear(empresaId, parcela.Id, datos.Fecha, string.IsNullOrWhiteSpace(datos.Producto) ? fito?.Nombre : datos.Producto, plazo, _reloj,
            datos.NumeroRegistro, datos.MateriaActiva, datos.Motivo, datos.Dosis, datos.UnidadDosis, datos.SuperficieTratadaHa, datos.Aplicador, datos.Observaciones, datos.Tipo,
            datos.NitrogenoKgHa, datos.FosforoKgHa, datos.PotasioKgHa, datos.VolumenM3);
        if (t.EsFallo)
        {
            return Resultado.Fallo<TratamientoDto>(t.Error);
        }

        if (fito is not null)
        {
            t.Valor.EnlazarRegistro(fito, datos.Cultivo);
        }

        // Consumo del almacén: el artículo (el del registro si no se indica) sale del almacén, del lote aplicado, si no está caducado.
        if (datos.CantidadConsumida is { } cantidad && cantidad != 0m)
        {
            var articulo = datos.ArticuloId ?? fito?.ProductoId;
            if (cantidad < 0m || articulo is null || datos.AlmacenId is null || _inventario is null)
            {
                return Resultado.Fallo<TratamientoDto>(Error.Validacion("tratamiento.consumo",
                    "Para descontar del almacén indica el artículo (o enlázalo al producto del registro), el almacén y una cantidad positiva."));
            }

            if (!string.IsNullOrWhiteSpace(datos.Lote)
                && await _inventario.CaducidadLoteAsync(empresaId, articulo.Value, datos.Lote.Trim(), ct).ConfigureAwait(false) is { } caducidad && datos.Fecha > caducidad)
            {
                return Resultado.Fallo<TratamientoDto>(Error.Conflicto("tratamiento.lote_caducado", $"El lote {datos.Lote.Trim()} caducó el {caducidad:dd/MM/yyyy}."));
            }

            var salida = await _inventario.ConsumirAsync(empresaId, articulo.Value, datos.AlmacenId.Value, datos.Lote, cantidad, datos.Fecha,
                $"Tratamiento {t.Valor.Producto} en {parcela.Codigo}", ct).ConfigureAwait(false);
            if (salida.EsFallo)
            {
                return Resultado.Fallo<TratamientoDto>(salida.Error);
            }

            t.Valor.AnotarConsumo(articulo.Value, datos.AlmacenId.Value, datos.Lote, cantidad);
        }

        _cuaderno.Agregar(t.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(t.Valor, parcela));
    }

    /// <summary>Comprueba el tratamiento con el registro y devuelve el plazo de seguridad del uso (0 si no tiene).</summary>
    private async Task<Resultado<int>> ComprobarRegistroAsync(Guid empresaId, Parcela parcela, ProductoFitosanitario fito, DatosTratamiento datos, CancellationToken ct)
    {
        if (datos.Tipo != TipoLabor.Fitosanitario)
        {
            return Resultado.Fallo<int>(Error.Validacion("tratamiento.tipo", "Un producto del registro de fitosanitarios va en un tratamiento fitosanitario."));
        }

        if (!fito.AplicableEl(datos.Fecha))
        {
            return Resultado.Fallo<int>(Error.Conflicto("tratamiento.no_autorizado",
                $"{fito.Nombre} ({fito.NumeroRegistro}) no se puede aplicar el {datos.Fecha:dd/MM/yyyy}: {fito.Estado}"
                + (fito.FechaCaducidad is { } c ? $", caducidad {c:dd/MM/yyyy}" : "") + (fito.FechaLimiteUso is { } u ? $", límite de uso {u:dd/MM/yyyy}" : "") + "."));
        }

        if (fito.Usos.Count == 0)
        {
            return Resultado.Ok(0);
        }

        static string N(string? t) => (t ?? string.Empty).Trim().ToUpperInvariant();
        var uso = fito.Usos.FirstOrDefault(u => N(u.Cultivo) == N(datos.Cultivo) && N(u.Plaga) == N(datos.Motivo));
        if (uso is null)
        {
            return Resultado.Fallo<int>(Error.Conflicto("tratamiento.uso_no_autorizado",
                $"{fito.Nombre} no tiene autorizado el uso «{datos.Cultivo} / {datos.Motivo}». Usos: "
                + string.Join("; ", fito.Usos.Select(u => $"{u.Cultivo} / {u.Plaga}").Take(10)) + (fito.Usos.Count > 10 ? "…" : "") + "."));
        }

        var mismaUnidad = uso.UnidadDosis is null || datos.UnidadDosis is null || string.Equals(uso.UnidadDosis.Trim(), datos.UnidadDosis.Trim(), StringComparison.OrdinalIgnoreCase);
        if (datos.Dosis is { } dosis && mismaUnidad && ((uso.DosisMinima is { } min && dosis < min) || (uso.DosisMaxima is { } max && dosis > max)))
        {
            return Resultado.Fallo<int>(Error.Validacion("tratamiento.dosis",
                $"La dosis {dosis} {datos.UnidadDosis ?? uso.UnidadDosis} está fuera de la autorizada para {uso.Cultivo} / {uso.Plaga}: {uso.DosisMinima?.ToString() ?? "—"} a {uso.DosisMaxima?.ToString() ?? "—"} {uso.UnidadDosis}."));
        }

        if (uso.Aplicaciones is { } maximo && maximo > 0)
        {
            var hechas = (await _cuaderno.ListarAsync(empresaId, [parcela.Id], new DateOnly(datos.Fecha.Year, 1, 1), new DateOnly(datos.Fecha.Year, 12, 31), ct).ConfigureAwait(false))
                .Count(t => !t.Anulado && t.FitosanitarioId == fito.Id && N(t.Cultivo) == N(uso.Cultivo));
            if (hechas >= maximo)
            {
                return Resultado.Fallo<int>(Error.Conflicto("tratamiento.aplicaciones",
                    $"{fito.Nombre} ya se ha aplicado {hechas} vez/veces este año en la parcela {parcela.Codigo}: el uso {uso.Cultivo} / {uso.Plaga} admite {maximo}."));
            }
        }

        return Resultado.Ok(uso.PlazoSeguridadDias ?? 0);
    }

    public async Task<Resultado<TratamientoDto>> AnularAsync(Guid id, string? motivo, CancellationToken ct = default)
    {
        var t = await _cuaderno.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (t is null)
        {
            return Resultado.Fallo<TratamientoDto>(Error.NoEncontrado("tratamiento.no_encontrado", "El tratamiento no existe."));
        }

        var r = t.Anular(motivo);
        if (r.EsFallo)
        {
            return Resultado.Fallo<TratamientoDto>(r.Error);
        }

        // Lo que salió del almacén vuelve a su lote.
        if (t.CantidadConsumida is { } cantidad && cantidad > 0m && t.ArticuloId is { } articulo && t.AlmacenId is { } almacen && _inventario is not null)
        {
            var vuelta = await _inventario.DevolverAsync(t.EmpresaId, articulo, almacen, t.Lote, cantidad, DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime),
                $"Anulación del tratamiento {t.Producto}", ct).ConfigureAwait(false);
            if (vuelta.EsFallo)
            {
                return Resultado.Fallo<TratamientoDto>(vuelta.Error);
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(t, await _agro.ParcelaAsync(t.ParcelaId, ct).ConfigureAwait(false)));
    }

    public async Task<IReadOnlyList<TratamientoDto>> ListarAsync(Guid empresaId, Guid? agricultorId, Guid? parcelaId, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
    {
        var parcelas = (await _agro.ParcelasAsync(empresaId, agricultorId, ct).ConfigureAwait(false)).Where(p => parcelaId is null || p.Id == parcelaId).ToDictionary(p => p.Id);
        return (await _cuaderno.ListarAsync(empresaId, parcelas.Keys, desde, hasta, ct).ConfigureAwait(false))
            .Select(t => Dto(t, parcelas.GetValueOrDefault(t.ParcelaId))).ToList();
    }

    /// <summary>Cuaderno de un agricultor en un periodo: tratamientos y recolecciones, con las incidencias de plazo de seguridad.</summary>
    public async Task<Resultado<CuadernoCampoDto>> CuadernoAsync(Guid empresaId, Guid agricultorId, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
    {
        var agricultor = await _agro.AgricultorAsync(agricultorId, ct).ConfigureAwait(false);
        if (agricultor is null)
        {
            return Resultado.Fallo<CuadernoCampoDto>(Error.NoEncontrado("agricultor.no_encontrado", "El agricultor no existe."));
        }

        var parcelas = (await _agro.ParcelasAsync(empresaId, agricultorId, ct).ConfigureAwait(false)).ToDictionary(p => p.Id);
        // Para las incidencias cuentan también los tratamientos anteriores al periodo (su plazo puede llegar dentro).
        var todos = await _cuaderno.ListarAsync(empresaId, parcelas.Keys, null, hasta, ct).ConfigureAwait(false);
        var recolecciones = new List<RecoleccionCuadernoDto>();
        foreach (var r in (await _agro.RecepcionesAsync(empresaId, desde, hasta, agricultorId, ct).ConfigureAwait(false)).Where(r => r.Estado != EstadoRecepcion.Anulada))
        {
            foreach (var l in r.Lineas.Where(l => l.ParcelaId is not null))
            {
                var dia = l.FechaRecoleccion ?? r.Fecha;
                var t = todos.FirstOrDefault(t => t.ParcelaId == l.ParcelaId && t.DentroDePlazo(dia));
                recolecciones.Add(new RecoleccionCuadernoDto(r.Id, r.NumeroCompleto ?? "(borrador)", dia, l.ParcelaId!.Value, parcelas.GetValueOrDefault(l.ParcelaId.Value)?.Codigo ?? "?",
                    l.ProductoNombre, l.NetoKg ?? r.NetoDe(l.Id),
                    t is null ? null : $"Recolectada dentro del plazo de seguridad de {t.Producto} ({t.Fecha:dd/MM/yyyy}, {t.PlazoSeguridadDias} días: desde el {t.RecolectableDesde:dd/MM/yyyy})."));
            }
        }

        var delPeriodo = todos.Where(t => desde is null || t.Fecha >= desde).ToList();
        var tratamientos = delPeriodo.Select(t => Dto(t, parcelas.GetValueOrDefault(t.ParcelaId))).ToList();
        var resumen = parcelas.Values.OrderBy(p => p.Codigo, StringComparer.Ordinal).Select(p =>
        {
            var suyas = delPeriodo.Where(t => t.ParcelaId == p.Id && !t.Anulado).ToList();
            return new ResumenParcelaCuadernoDto(p.Id, p.Codigo, suyas.Count(t => t.Tipo == TipoLabor.Fitosanitario), suyas.Sum(t => t.NitrogenoKgHa ?? 0m),
                suyas.Sum(t => t.FosforoKgHa ?? 0m), suyas.Sum(t => t.PotasioKgHa ?? 0m), suyas.Sum(t => t.VolumenM3 ?? 0m));
        }).ToList();
        return Resultado.Ok(new CuadernoCampoDto(agricultor.Id, agricultor.Nombre, desde, hasta, tratamientos,
            recolecciones.OrderBy(x => x.Fecha).ToList(), recolecciones.Count(x => x.Incidencia is not null), resumen));
    }

    /// <summary>Errores de plazo de seguridad de las líneas de una recepción (parcela y fecha de recolección).</summary>
    public async Task<IReadOnlyList<Error>> ComprobarPlazosAsync(Recepcion recepcion, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(recepcion);
        var parcelas = recepcion.Lineas.Where(l => l.ParcelaId is not null).Select(l => l.ParcelaId!.Value).Distinct().ToList();
        if (parcelas.Count == 0)
        {
            return [];
        }

        var tratamientos = await _cuaderno.ListarAsync(recepcion.EmpresaId, parcelas, null, recepcion.Fecha, ct).ConfigureAwait(false);
        var errores = new List<Error>();
        foreach (var l in recepcion.Lineas.Where(l => l.ParcelaId is not null))
        {
            var dia = l.FechaRecoleccion ?? recepcion.Fecha;
            if (tratamientos.FirstOrDefault(t => t.ParcelaId == l.ParcelaId && t.DentroDePlazo(dia)) is { } t)
            {
                errores.Add(Error.Conflicto("recepcion.plazo_seguridad",
                    $"Línea {l.NumeroLinea}: la parcela se trató con {t.Producto} el {t.Fecha:dd/MM/yyyy} y no se puede recolectar hasta el {t.RecolectableDesde:dd/MM/yyyy}."));
            }
        }

        return errores;
    }

    private static TratamientoDto Dto(TratamientoParcela t, Parcela? p) => new(t.Id, t.ParcelaId, p?.Codigo ?? "?", p?.AgricultorId ?? Guid.Empty, t.Fecha, t.Producto,
        t.NumeroRegistro, t.MateriaActiva, t.Motivo, t.Dosis, t.UnidadDosis, t.SuperficieTratadaHa, t.PlazoSeguridadDias, t.RecolectableDesde, t.Aplicador,
        t.Observaciones, t.Anulado, t.MotivoAnulacion, t.Tipo.ToString(), t.NitrogenoKgHa, t.FosforoKgHa, t.PotasioKgHa, t.VolumenM3, t.FitosanitarioId, t.Cultivo,
        t.ArticuloId, t.AlmacenId, t.Lote, t.CantidadConsumida);
}
