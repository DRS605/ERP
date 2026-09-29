using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Agro.Dominio;

/// <summary>Estado de una recepción de fruta.</summary>
public enum EstadoRecepcion
{
    /// <summary>En curso: se añaden líneas y pesadas. No tiene número ni mueve existencias.</summary>
    Borrador = 1,

    /// <summary>Confirmada: numerada sin huecos, con una partida por línea y el movimiento de envases.</summary>
    Confirmada = 2,

    /// <summary>Anulada: sus partidas se dan de baja (solo si no se han usado ni liquidado).</summary>
    Anulada = 3,
}

/// <summary>
/// Entrada de fruta de un agricultor en el almacén: el albarán de entrada. Cada línea es un producto de
/// una parcela, con sus pesadas en báscula (bruto − tara = neto) y sus envases. Al confirmarla, cada
/// línea crea una <see cref="Partida"/> con los kilos netos, que es lo que se clasifica, se confecciona
/// y se liquida al agricultor.
/// </summary>
public sealed class Recepcion : RaizAgregadoEmpresa<Guid>
{
    public const string Serie = "REC";
    public const int LongitudTexto = 200;

    private readonly List<LineaRecepcion> _lineas = [];
    private readonly List<Pesada> _pesadas = [];

    private Recepcion(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private Recepcion(Guid id, Guid empresaId, Guid agricultorId, Guid campanaId, DateOnly fecha, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        AgricultorId = agricultorId;
        CampanaId = campanaId;
        Fecha = fecha;
        Ejercicio = fecha.Year;
        Estado = EstadoRecepcion.Borrador;
        CreadoEn = ahora;
    }

    public int Ejercicio { get; private set; }

    /// <summary>Número correlativo sin huecos de la serie y ejercicio; se asigna al confirmar.</summary>
    public int? Numero { get; private set; }

    public string? NumeroCompleto => Numero is { } n ? $"{Serie}-{Ejercicio}-{n:D6}" : null;

    public DateOnly Fecha { get; private set; }

    public Guid AgricultorId { get; private set; }

    public Guid CampanaId { get; private set; }

    public string? Matricula { get; private set; }

    public string? Conductor { get; private set; }

    public string? Observaciones { get; private set; }

    public EstadoRecepcion Estado { get; private set; }

    public string? MotivoAnulacion { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset? ConfirmadaEn { get; private set; }

    public DateTimeOffset? AnuladaEn { get; private set; }

    public IReadOnlyList<LineaRecepcion> Lineas => _lineas;

    public IReadOnlyList<Pesada> Pesadas => _pesadas;

    public decimal NetoKg => _lineas.Sum(l => NetoDe(l.Id));

    public decimal NetoDe(Guid lineaId) => _pesadas.Where(p => p.LineaId == lineaId).Sum(p => p.NetoKg);

    public int EnvasesDe(Guid lineaId) => _pesadas.Where(p => p.LineaId == lineaId).Sum(p => p.Envases);

    public static Resultado<Recepcion> Crear(Guid empresaId, Guid agricultorId, Guid campanaId, DateOnly fecha, string? matricula, string? conductor, string? observaciones, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        var r = new Recepcion(Guid.NewGuid(), empresaId, agricultorId, campanaId, fecha, reloj.AhoraUtc);
        var datos = r.ActualizarCabecera(fecha, matricula, conductor, observaciones);
        return datos.EsFallo ? Resultado.Fallo<Recepcion>(datos.Error) : Resultado.Ok(r);
    }

    public Resultado ActualizarCabecera(DateOnly fecha, string? matricula, string? conductor, string? observaciones)
    {
        var borrador = SoloBorrador();
        if (borrador.EsFallo)
        {
            return borrador;
        }

        if (new[] { matricula, conductor, observaciones }.Any(t => t?.Trim().Length > LongitudTexto))
        {
            return Resultado.Fallo(Error.Validacion("recepcion.texto_largo", $"Los textos admiten hasta {LongitudTexto} caracteres."));
        }

        Fecha = fecha;
        Ejercicio = fecha.Year;
        Matricula = Limpio(matricula)?.ToUpperInvariant();
        Conductor = Limpio(conductor);
        Observaciones = Limpio(observaciones);
        return Resultado.Ok();
    }

    public Resultado<LineaRecepcion> AgregarLinea(DatosLineaRecepcion datos)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var borrador = SoloBorrador();
        if (borrador.EsFallo)
        {
            return Resultado.Fallo<LineaRecepcion>(borrador.Error);
        }

        if (datos.PrecioEstimadoKg is < 0m)
        {
            return Resultado.Fallo<LineaRecepcion>(Error.Validacion("recepcion.precio", "El precio estimado no puede ser negativo."));
        }

        if (datos.FechaRecoleccion is { } fr && fr > Fecha)
        {
            return Resultado.Fallo<LineaRecepcion>(Error.Validacion("recepcion.fecha_recoleccion", "La fruta no puede recolectarse después de recibirla."));
        }

        var linea = new LineaRecepcion(Guid.NewGuid(), _lineas.Count == 0 ? 1 : _lineas.Max(l => l.NumeroLinea) + 1, datos);
        _lineas.Add(linea);
        return Resultado.Ok(linea);
    }

    public Resultado QuitarLinea(Guid lineaId)
    {
        var borrador = SoloBorrador();
        if (borrador.EsFallo)
        {
            return borrador;
        }

        var linea = _lineas.FirstOrDefault(l => l.Id == lineaId);
        if (linea is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("recepcion.linea_no_encontrada", "La línea no existe."));
        }

        _pesadas.RemoveAll(p => p.LineaId == lineaId);
        _lineas.Remove(linea);
        return Resultado.Ok();
    }

    /// <summary>Añade una pesada de báscula a una línea. El neto es bruto − tara (camión, palots…).</summary>
    public Resultado<Pesada> AgregarPesada(Guid lineaId, decimal brutoKg, decimal taraKg, int envases, string? bascula)
    {
        var borrador = SoloBorrador();
        if (borrador.EsFallo)
        {
            return Resultado.Fallo<Pesada>(borrador.Error);
        }

        if (_lineas.All(l => l.Id != lineaId))
        {
            return Resultado.Fallo<Pesada>(Error.NoEncontrado("recepcion.linea_no_encontrada", "La línea no existe."));
        }

        if (taraKg < 0m || brutoKg <= taraKg)
        {
            return Resultado.Fallo<Pesada>(Error.Validacion("pesada.kilos", "El bruto debe ser mayor que la tara, y la tara no puede ser negativa."));
        }

        if (decimal.Round(brutoKg, 3) != brutoKg || decimal.Round(taraKg, 3) != taraKg)
        {
            return Resultado.Fallo<Pesada>(Error.Validacion("pesada.decimales", "Los kilos admiten hasta 3 decimales."));
        }

        if (envases < 0)
        {
            return Resultado.Fallo<Pesada>(Error.Validacion("pesada.envases", "Los envases no pueden ser negativos."));
        }

        var secuencia = _pesadas.Where(p => p.LineaId == lineaId).Select(p => p.Secuencia).DefaultIfEmpty(0).Max() + 1;
        var pesada = new Pesada(Guid.NewGuid(), lineaId, secuencia, brutoKg, taraKg, envases, Limpio(bascula));
        _pesadas.Add(pesada);
        return Resultado.Ok(pesada);
    }

    public Resultado QuitarPesada(Guid pesadaId)
    {
        var borrador = SoloBorrador();
        if (borrador.EsFallo)
        {
            return borrador;
        }

        return _pesadas.RemoveAll(p => p.Id == pesadaId) == 0
            ? Resultado.Fallo(Error.NoEncontrado("pesada.no_encontrada", "La pesada no existe."))
            : Resultado.Ok();
    }

    /// <summary>
    /// Errores que impiden confirmar, todos a la vez (para corregirlos de una pasada): sin líneas, líneas
    /// sin pesadas, envases sin artículo de envase, recolección posterior a la entrega.
    /// </summary>
    public IReadOnlyList<Error> ErroresConfirmacion()
    {
        var errores = new List<Error>();
        if (Estado != EstadoRecepcion.Borrador)
        {
            errores.Add(Error.Conflicto("recepcion.no_borrador", "La recepción ya está confirmada o anulada."));
            return errores;
        }

        if (_lineas.Count == 0)
        {
            errores.Add(Error.Validacion("recepcion.sin_lineas", "La recepción no tiene líneas."));
        }

        foreach (var l in _lineas.OrderBy(l => l.NumeroLinea))
        {
            if (_pesadas.All(p => p.LineaId != l.Id))
            {
                errores.Add(Error.Validacion("recepcion.sin_pesadas", $"La línea {l.NumeroLinea} no tiene pesadas."));
            }

            if (EnvasesDe(l.Id) > 0 && l.EnvaseProductoId is null)
            {
                errores.Add(Error.Validacion("recepcion.envase", $"La línea {l.NumeroLinea} trae envases pero no indica cuál es el envase."));
            }

            if (l.FechaRecoleccion is { } fr && fr > Fecha)
            {
                errores.Add(Error.Validacion("recepcion.fecha_recoleccion", $"La línea {l.NumeroLinea} se recolectó después de la fecha de la recepción."));
            }
        }

        return errores;
    }

    /// <summary>Confirma con el número reservado y fija en cada línea su partida, sus kilos netos y sus envases.</summary>
    public Resultado Confirmar(int numero, IReadOnlyDictionary<Guid, Guid> partidaPorLinea, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(partidaPorLinea);
        ArgumentNullException.ThrowIfNull(reloj);
        var errores = ErroresConfirmacion();
        if (errores.Count > 0)
        {
            return Resultado.Fallo(errores[0]);
        }

        foreach (var l in _lineas)
        {
            l.Fijar(partidaPorLinea[l.Id], NetoDe(l.Id), EnvasesDe(l.Id));
        }

        Numero = numero;
        Estado = EstadoRecepcion.Confirmada;
        ConfirmadaEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    public Resultado Anular(string? motivo, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (Estado != EstadoRecepcion.Confirmada)
        {
            return Resultado.Fallo(Error.Conflicto("recepcion.no_confirmada", "Solo se anula una recepción confirmada (un borrador se elimina)."));
        }

        if (string.IsNullOrWhiteSpace(motivo) || motivo.Trim().Length > LongitudTexto)
        {
            return Resultado.Fallo(Error.Validacion("recepcion.motivo", "Indica el motivo de la anulación."));
        }

        Estado = EstadoRecepcion.Anulada;
        MotivoAnulacion = motivo.Trim();
        AnuladaEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    private Resultado SoloBorrador() =>
        Estado == EstadoRecepcion.Borrador
            ? Resultado.Ok()
            : Resultado.Fallo(Error.Conflicto("recepcion.no_borrador", "La recepción ya está confirmada o anulada: no se puede modificar."));

    private static string? Limpio(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
}

public sealed record DatosLineaRecepcion(
    Guid ProductoId, string ProductoNombre, Guid? ParcelaId = null, DateOnly? FechaRecoleccion = null, Guid? EnvaseProductoId = null,
    decimal? PrecioEstimadoKg = null, string? Calibre = null, string? MotivoDescalificacion = null);

/// <summary>Línea de una recepción: un producto de una parcela.</summary>
public sealed class LineaRecepcion : EntidadBase<Guid>
{
    private LineaRecepcion(Guid id)
        : base(id)
    {
        ProductoNombre = null!;
    }

    internal LineaRecepcion(Guid id, int numeroLinea, DatosLineaRecepcion d)
        : base(id)
    {
        NumeroLinea = numeroLinea;
        ProductoId = d.ProductoId;
        ProductoNombre = d.ProductoNombre;
        ParcelaId = d.ParcelaId;
        FechaRecoleccion = d.FechaRecoleccion;
        EnvaseProductoId = d.EnvaseProductoId;
        PrecioEstimadoKg = d.PrecioEstimadoKg;
        Calibre = string.IsNullOrWhiteSpace(d.Calibre) ? null : d.Calibre.Trim();
        MotivoDescalificacion = string.IsNullOrWhiteSpace(d.MotivoDescalificacion) ? null : d.MotivoDescalificacion.Trim();
    }

    /// <summary>Motivo para recibir fruta ecológica con un artículo convencional (descalificación explícita).</summary>
    public string? MotivoDescalificacion { get; private set; }

    public int NumeroLinea { get; private set; }

    public Guid ProductoId { get; private set; }

    public string ProductoNombre { get; private set; }

    public Guid? ParcelaId { get; private set; }

    public DateOnly? FechaRecoleccion { get; private set; }

    /// <summary>Artículo del envase en que llega la fruta (palot, caja…), para el control de envases del agricultor.</summary>
    public Guid? EnvaseProductoId { get; private set; }

    /// <summary>Precio orientativo (anticipo); el definitivo sale de los precios de liquidación.</summary>
    public decimal? PrecioEstimadoKg { get; private set; }

    public string? Calibre { get; private set; }

    /// <summary>Partida creada al confirmar.</summary>
    public Guid? PartidaId { get; private set; }

    /// <summary>Kilos netos (suma de las pesadas), fijados al confirmar.</summary>
    public decimal? NetoKg { get; private set; }

    public int? Envases { get; private set; }

    internal void Fijar(Guid partidaId, decimal netoKg, int envases)
    {
        PartidaId = partidaId;
        NetoKg = netoKg;
        Envases = envases;
    }
}

/// <summary>Pesada de báscula de una línea.</summary>
public sealed class Pesada : EntidadBase<Guid>
{
    private Pesada(Guid id)
        : base(id)
    {
    }

    internal Pesada(Guid id, Guid lineaId, int secuencia, decimal bruto, decimal tara, int envases, string? bascula)
        : base(id)
    {
        LineaId = lineaId;
        Secuencia = secuencia;
        BrutoKg = bruto;
        TaraKg = tara;
        Envases = envases;
        Bascula = bascula;
    }

    public Guid LineaId { get; private set; }

    public int Secuencia { get; private set; }

    public decimal BrutoKg { get; private set; }

    public decimal TaraKg { get; private set; }

    public decimal NetoKg => BrutoKg - TaraKg;

    public int Envases { get; private set; }

    /// <summary>Identificador de la báscula o del ticket de pesada.</summary>
    public string? Bascula { get; private set; }
}
