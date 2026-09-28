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
    string? Motivo = null, decimal? Dosis = null, string? UnidadDosis = null, decimal? SuperficieTratadaHa = null, string? Aplicador = null, string? Observaciones = null);

public sealed record TratamientoDto(Guid Id, Guid ParcelaId, string Parcela, Guid AgricultorId, DateOnly Fecha, string Producto, string? NumeroRegistro, string? MateriaActiva,
    string? Motivo, decimal? Dosis, string? UnidadDosis, decimal? SuperficieTratadaHa, int PlazoSeguridadDias, DateOnly RecolectableDesde, string? Aplicador,
    string? Observaciones, bool Anulado, string? MotivoAnulacion);

/// <summary>Recolección (entrega) de una parcela en el cuaderno, con el aviso si fue dentro del plazo de seguridad de un tratamiento.</summary>
public sealed record RecoleccionCuadernoDto(Guid RecepcionId, string Recepcion, DateOnly Fecha, Guid ParcelaId, string Parcela, string Producto, decimal NetoKg,
    string? Incidencia);

public sealed record CuadernoCampoDto(Guid AgricultorId, string Agricultor, DateOnly? Desde, DateOnly? Hasta, IReadOnlyList<TratamientoDto> Tratamientos,
    IReadOnlyList<RecoleccionCuadernoDto> Recolecciones, int Incidencias);

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

    public CuadernoCampoAgro(IRepositorioCuaderno cuaderno, IRepositorioAgro agro, IUnidadDeTrabajoAgro unidad, IReloj reloj)
    {
        _cuaderno = cuaderno;
        _agro = agro;
        _unidad = unidad;
        _reloj = reloj;
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

        var t = TratamientoParcela.Crear(empresaId, parcela.Id, datos.Fecha, datos.Producto, datos.PlazoSeguridadDias, _reloj, datos.NumeroRegistro, datos.MateriaActiva,
            datos.Motivo, datos.Dosis, datos.UnidadDosis, datos.SuperficieTratadaHa, datos.Aplicador, datos.Observaciones);
        if (t.EsFallo)
        {
            return Resultado.Fallo<TratamientoDto>(t.Error);
        }

        _cuaderno.Agregar(t.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(t.Valor, parcela));
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

        var tratamientos = todos.Where(t => desde is null || t.Fecha >= desde).Select(t => Dto(t, parcelas.GetValueOrDefault(t.ParcelaId))).ToList();
        return Resultado.Ok(new CuadernoCampoDto(agricultor.Id, agricultor.Nombre, desde, hasta, tratamientos,
            recolecciones.OrderBy(x => x.Fecha).ToList(), recolecciones.Count(x => x.Incidencia is not null)));
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
        t.Observaciones, t.Anulado, t.MotivoAnulacion);
}
