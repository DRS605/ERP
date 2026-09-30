using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Agro.Dominio;

/// <summary>Qué hace una línea de la planta.</summary>
public enum TipoLineaPlanta
{
    /// <summary>Calibradora: separa la fruta por calibre y categoría (peso, color, defectos).</summary>
    Calibradora = 1,

    /// <summary>Línea de confección: llena cajas y monta palés.</summary>
    Confeccion = 2,

    /// <summary>Envasado: mallas, bolsas o tarrinas.</summary>
    Envasado = 3,
}

/// <summary>
/// Línea de la planta de confección: su capacidad en kilos por hora y los turnos de trabajo, que dan la capacidad del día;
/// y, en una calibradora, sus salidas (canales), cada una con el calibre y la categoría que recoge.
/// </summary>
public sealed class LineaPlanta : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudCodigo = 40;
    public const int LongitudNombre = 120;

    private readonly List<SalidaCalibradora> _salidas = [];

    private LineaPlanta(Guid id) : base(id, Guid.Empty) { Codigo = null!; Nombre = null!; }

    private LineaPlanta(Guid id, Guid empresaId) : base(id, empresaId) { Codigo = null!; Nombre = null!; }

    /// <summary>Código de la línea: el mismo que se usa en el plan de producción.</summary>
    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    public TipoLineaPlanta Tipo { get; private set; }

    public decimal CapacidadKgHora { get; private set; }

    public decimal HorasTurno { get; private set; }

    public int Turnos { get; private set; }

    public Guid? CentroAnaliticoId { get; private set; }

    public bool Activa { get; private set; }

    public IReadOnlyList<SalidaCalibradora> Salidas => _salidas;

    /// <summary>Kilos que puede trabajar en un día con todos sus turnos.</summary>
    public decimal CapacidadDia => Redondeo.Dos(CapacidadKgHora * HorasTurno * Turnos);

    public static Resultado<LineaPlanta> Crear(Guid empresaId, DatosLineaPlanta d)
    {
        var l = new LineaPlanta(Guid.NewGuid(), empresaId) { Activa = true };
        var r = l.Cambiar(d);
        return r.EsFallo ? Resultado.Fallo<LineaPlanta>(r.Error) : Resultado.Ok(l);
    }

    public Resultado Cambiar(DatosLineaPlanta d)
    {
        ArgumentNullException.ThrowIfNull(d);
        var codigo = d.Codigo?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(codigo) || codigo.Length > LongitudCodigo)
        {
            return Resultado.Fallo(Error.Validacion("linea_planta.codigo", $"Indica el código de la línea (hasta {LongitudCodigo} caracteres)."));
        }

        if (string.IsNullOrWhiteSpace(d.Nombre) || d.Nombre.Trim().Length > LongitudNombre)
        {
            return Resultado.Fallo(Error.Validacion("linea_planta.nombre", "Indica el nombre de la línea."));
        }

        if (!Enum.IsDefined(d.Tipo))
        {
            return Resultado.Fallo(Error.Validacion("linea_planta.tipo", "La línea es calibradora, de confección o de envasado."));
        }

        if (d.CapacidadKgHora <= 0m || d.HorasTurno is <= 0m or > 24m || d.Turnos is < 1 or > 3 || d.HorasTurno * d.Turnos > 24m)
        {
            return Resultado.Fallo(Error.Validacion("linea_planta.capacidad",
                "La capacidad es positiva, cada turno dura entre 0 y 24 horas y hay de 1 a 3 turnos que no pasan de 24 horas en total."));
        }

        var salidas = d.Salidas ?? [];
        if (salidas.Count > 0 && d.Tipo != TipoLineaPlanta.Calibradora)
        {
            return Resultado.Fallo(Error.Validacion("linea_planta.salidas", "Solo una calibradora tiene salidas."));
        }

        if (salidas.Any(s => s.Numero is < 1 or > 999) || salidas.Select(s => s.Numero).Distinct().Count() != salidas.Count)
        {
            return Resultado.Fallo(Error.Validacion("linea_planta.salidas", "Cada salida tiene un número del 1 al 999, sin repetir."));
        }

        if (salidas.Any(s => string.IsNullOrWhiteSpace(s.Calibre) && s.CategoriaId is null && !s.Destrio))
        {
            return Resultado.Fallo(Error.Validacion("linea_planta.salidas", "Cada salida recoge un calibre, una categoría o el destrío."));
        }

        Codigo = codigo;
        Nombre = d.Nombre.Trim();
        Tipo = d.Tipo;
        CapacidadKgHora = d.CapacidadKgHora;
        HorasTurno = d.HorasTurno;
        Turnos = d.Turnos;
        CentroAnaliticoId = d.CentroAnaliticoId;
        _salidas.Clear();
        _salidas.AddRange(salidas.OrderBy(s => s.Numero).Select(s => new SalidaCalibradora(Guid.NewGuid(), s.Numero, Texto(s.Calibre, 20), s.CategoriaId, s.Destrio)));
        return Resultado.Ok();
    }

    public void FijarActiva(bool activa) => Activa = activa;

    public SalidaCalibradora? Salida(int numero) => _salidas.FirstOrDefault(s => s.Numero == numero);

    internal static string? Texto(string? t, int max) => string.IsNullOrWhiteSpace(t) ? null : t.Trim()[..Math.Min(t.Trim().Length, max)];
}

public sealed record DatosSalidaCalibradora(int Numero, string? Calibre = null, Guid? CategoriaId = null, bool Destrio = false);

public sealed record DatosLineaPlanta(string? Codigo, string? Nombre, TipoLineaPlanta Tipo, decimal CapacidadKgHora, decimal HorasTurno = 8m, int Turnos = 1,
    Guid? CentroAnaliticoId = null, IReadOnlyList<DatosSalidaCalibradora>? Salidas = null);

/// <summary>Salida (canal) de una calibradora: el calibre y la categoría que recoge, o el destrío.</summary>
public sealed class SalidaCalibradora : EntidadBase<Guid>
{
    private SalidaCalibradora(Guid id) : base(id) { }

    internal SalidaCalibradora(Guid id, int numero, string? calibre, Guid? categoriaId, bool destrio) : base(id)
    {
        Numero = numero;
        Calibre = calibre;
        CategoriaId = categoriaId;
        Destrio = destrio;
    }

    public int Numero { get; private set; }

    public string? Calibre { get; private set; }

    public Guid? CategoriaId { get; private set; }

    public bool Destrio { get; private set; }
}

public enum EstadoCalibrado
{
    Borrador = 1,
    Confirmado = 2,
    Anulado = 3,
}

/// <summary>
/// Paso de una partida por la calibradora: los kilos que entraron y cómo salieron por calibre y categoría (y piezas, que dan
/// el peso medio del fruto). Es una medida: no mueve kilos. Al confirmarlo puede pasar a la clasificación de la partida, que
/// es la que se liquida al agricultor.
/// </summary>
public sealed class Calibrado : RaizAgregadoEmpresa<Guid>
{
    private readonly List<LineaCalibrado> _lineas = [];

    private Calibrado(Guid id) : base(id, Guid.Empty) { }

    private Calibrado(Guid id, Guid empresaId) : base(id, empresaId) { }

    public Guid LineaId { get; private set; }

    public Guid PartidaId { get; private set; }

    public DateOnly Fecha { get; private set; }

    public decimal KilosEntrada { get; private set; }

    public EstadoCalibrado Estado { get; private set; }

    /// <summary>Referencia del lote en la calibradora (el fichero o el número de vaciado).</summary>
    public string? Referencia { get; private set; }

    public Guid? ClasificacionId { get; private set; }

    public string? MotivoAnulacion { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public IReadOnlyList<LineaCalibrado> Lineas => _lineas;

    public decimal KilosSalida => _lineas.Sum(l => l.Kilos);

    /// <summary>Lo que entró y no salió por ninguna salida (agua, hojas, fruta perdida).</summary>
    public decimal Merma => KilosEntrada - KilosSalida;

    public static Resultado<Calibrado> Crear(Guid empresaId, LineaPlanta linea, Guid partidaId, decimal kilosPartida, DatosCalibrado d, DateTimeOffset ahora)
    {
        ArgumentNullException.ThrowIfNull(linea);
        if (linea.Tipo != TipoLineaPlanta.Calibradora)
        {
            return Resultado.Fallo<Calibrado>(Error.Validacion("calibrado.linea", $"La línea {linea.Codigo} no es una calibradora."));
        }

        var c = new Calibrado(Guid.NewGuid(), empresaId) { LineaId = linea.Id, PartidaId = partidaId, Estado = EstadoCalibrado.Borrador, CreadoEn = ahora };
        var r = c.Cambiar(linea, kilosPartida, d);
        return r.EsFallo ? Resultado.Fallo<Calibrado>(r.Error) : Resultado.Ok(c);
    }

    public Resultado Cambiar(LineaPlanta linea, decimal kilosPartida, DatosCalibrado d)
    {
        ArgumentNullException.ThrowIfNull(linea);
        ArgumentNullException.ThrowIfNull(d);
        if (Estado != EstadoCalibrado.Borrador)
        {
            return Resultado.Fallo(Error.Conflicto("calibrado.no_borrador", "Solo se cambia un calibrado en borrador."));
        }

        if (d.Lineas is not { Count: > 0 })
        {
            return Resultado.Fallo(Error.Validacion("calibrado.lineas", "El calibrado necesita al menos una salida con kilos."));
        }

        var lineas = new List<LineaCalibrado>();
        foreach (var l in d.Lineas)
        {
            var calibre = LineaPlanta.Texto(l.Calibre, 20);
            var categoria = l.CategoriaId;
            var destrio = l.Destrio;
            if (l.Salida is { } n)
            {
                if (linea.Salida(n) is not { } s)
                {
                    return Resultado.Fallo(Error.Validacion("calibrado.salida", $"La calibradora {linea.Codigo} no tiene la salida {n}."));
                }

                calibre ??= s.Calibre;
                categoria ??= s.CategoriaId;
                destrio |= s.Destrio;
            }

            if (l.Kilos < 0m || l.Piezas is < 0)
            {
                return Resultado.Fallo(Error.Validacion("calibrado.kilos", "Los kilos y las piezas no pueden ser negativos."));
            }

            if (calibre is null && categoria is null && !destrio)
            {
                return Resultado.Fallo(Error.Validacion("calibrado.linea_sin_destino", "Cada línea es de un calibre, una categoría, una salida o el destrío."));
            }

            // Varias líneas del mismo calibre y categoría (dos salidas iguales) se suman.
            if (lineas.FirstOrDefault(x => x.Calibre == calibre && x.CategoriaId == categoria && x.Destrio == destrio) is { } igual)
            {
                igual.Sumar(l.Kilos, l.Piezas);
            }
            else
            {
                lineas.Add(new LineaCalibrado(Guid.NewGuid(), calibre, categoria, destrio, l.Kilos, l.Piezas));
            }
        }

        var salida = lineas.Sum(l => l.Kilos);
        var entrada = d.KilosEntrada ?? salida;
        if (salida <= 0m)
        {
            return Resultado.Fallo(Error.Validacion("calibrado.kilos", "El calibrado no tiene kilos."));
        }

        if (salida > entrada)
        {
            return Resultado.Fallo(Error.Validacion("calibrado.mas_de_lo_entrado", $"Salen {salida:0.###} kg y solo entraron {entrada:0.###}."));
        }

        if (entrada > kilosPartida)
        {
            return Resultado.Fallo(Error.Validacion("calibrado.mas_que_la_partida", $"Entran {entrada:0.###} kg y la partida tiene {kilosPartida:0.###}."));
        }

        Fecha = d.Fecha;
        KilosEntrada = entrada;
        Referencia = LineaPlanta.Texto(d.Referencia, 60);
        _lineas.Clear();
        _lineas.AddRange(lineas);
        return Resultado.Ok();
    }

    public Resultado Confirmar(Guid? clasificacionId)
    {
        if (Estado != EstadoCalibrado.Borrador)
        {
            return Resultado.Fallo(Error.Conflicto("calibrado.no_borrador", "El calibrado ya está confirmado o anulado."));
        }

        Estado = EstadoCalibrado.Confirmado;
        ClasificacionId = clasificacionId;
        return Resultado.Ok();
    }

    public Resultado Anular(string? motivo)
    {
        if (Estado == EstadoCalibrado.Anulado)
        {
            return Resultado.Fallo(Error.Conflicto("calibrado.anulado", "El calibrado ya está anulado."));
        }

        if (string.IsNullOrWhiteSpace(motivo))
        {
            return Resultado.Fallo(Error.Validacion("calibrado.motivo", "Indica el motivo de la anulación."));
        }

        Estado = EstadoCalibrado.Anulado;
        MotivoAnulacion = LineaPlanta.Texto(motivo, 200);
        return Resultado.Ok();
    }

    /// <summary>Kilos por categoría (lo que no tiene categoría no cuenta): la muestra de la clasificación.</summary>
    public IReadOnlyList<(Guid CategoriaId, decimal Kilos)> PorCategoria() =>
        _lineas.Where(l => l.CategoriaId is not null && l.Kilos > 0m).GroupBy(l => l.CategoriaId!.Value)
            .Select(g => (g.Key, g.Sum(l => l.Kilos))).ToList();
}

public sealed record DatosLineaCalibrado(decimal Kilos, int? Salida = null, string? Calibre = null, Guid? CategoriaId = null, bool Destrio = false, int? Piezas = null);

public sealed record DatosCalibrado(DateOnly Fecha, IReadOnlyList<DatosLineaCalibrado> Lineas, decimal? KilosEntrada = null, string? Referencia = null);

public sealed class LineaCalibrado : EntidadBase<Guid>
{
    private LineaCalibrado(Guid id) : base(id) { }

    internal LineaCalibrado(Guid id, string? calibre, Guid? categoriaId, bool destrio, decimal kilos, int? piezas) : base(id)
    {
        Calibre = calibre;
        CategoriaId = categoriaId;
        Destrio = destrio;
        Kilos = kilos;
        Piezas = piezas;
    }

    public string? Calibre { get; private set; }

    public Guid? CategoriaId { get; private set; }

    public bool Destrio { get; private set; }

    public decimal Kilos { get; private set; }

    public int? Piezas { get; private set; }

    /// <summary>Peso medio del fruto en gramos.</summary>
    public decimal? GramosPieza => Piezas is > 0 ? Math.Round(Kilos * 1000m / Piezas.Value, 1, MidpointRounding.AwayFromZero) : null;

    internal void Sumar(decimal kilos, int? piezas)
    {
        Kilos += kilos;
        Piezas = Piezas is null && piezas is null ? null : (Piezas ?? 0) + (piezas ?? 0);
    }
}

public enum EstadoOrdenLinea
{
    Planificada = 1,
    EnCurso = 2,
    Terminada = 3,
    Cancelada = 4,
}

/// <summary>
/// Orden de trabajo de una línea: qué producto confeccionar, cuántos kilos, qué día y en qué turno, en qué orden dentro del
/// turno y, si es para un pedido, para cuál. Al terminarla se enlaza con el parte de confección, que da lo real.
/// </summary>
public sealed class OrdenLinea : RaizAgregadoEmpresa<Guid>
{
    private OrdenLinea(Guid id) : base(id, Guid.Empty) { }

    private OrdenLinea(Guid id, Guid empresaId) : base(id, empresaId) { }

    public Guid LineaId { get; private set; }

    public DateOnly Fecha { get; private set; }

    public int Turno { get; private set; }

    public int Secuencia { get; private set; }

    public Guid ProductoId { get; private set; }

    public decimal Kilos { get; private set; }

    public int? Cajas { get; private set; }

    public Guid? ClienteId { get; private set; }

    public Guid? PedidoVentaId { get; private set; }

    /// <summary>Línea del plan de producción de la que sale, si se generó desde él.</summary>
    public Guid? LineaPlanId { get; private set; }

    public string? Notas { get; private set; }

    public EstadoOrdenLinea Estado { get; private set; }

    public Guid? ParteConfeccionId { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public static Resultado<OrdenLinea> Crear(Guid empresaId, LineaPlanta linea, DatosOrdenLinea d, DateTimeOffset ahora, Guid? lineaPlanId = null)
    {
        var o = new OrdenLinea(Guid.NewGuid(), empresaId) { Estado = EstadoOrdenLinea.Planificada, CreadoEn = ahora, LineaPlanId = lineaPlanId };
        var r = o.Cambiar(linea, d);
        return r.EsFallo ? Resultado.Fallo<OrdenLinea>(r.Error) : Resultado.Ok(o);
    }

    public Resultado Cambiar(LineaPlanta linea, DatosOrdenLinea d)
    {
        ArgumentNullException.ThrowIfNull(linea);
        ArgumentNullException.ThrowIfNull(d);
        if (Estado != EstadoOrdenLinea.Planificada)
        {
            return Resultado.Fallo(Error.Conflicto("orden_linea.no_planificada", "Solo se cambia o se mueve una orden planificada."));
        }

        if (!linea.Activa)
        {
            return Resultado.Fallo(Error.Validacion("orden_linea.linea_baja", $"La línea {linea.Codigo} está de baja."));
        }

        if (d.Turno < 1 || d.Turno > linea.Turnos)
        {
            return Resultado.Fallo(Error.Validacion("orden_linea.turno", $"La línea {linea.Codigo} trabaja {linea.Turnos} turno(s)."));
        }

        if (d.Kilos <= 0m || d.Cajas is < 0 || d.Secuencia is < 0)
        {
            return Resultado.Fallo(Error.Validacion("orden_linea.kilos", "Los kilos son positivos y las cajas y la secuencia no negativas."));
        }

        LineaId = linea.Id;
        Fecha = d.Fecha;
        Turno = d.Turno;
        Secuencia = d.Secuencia ?? 0;
        ProductoId = d.ProductoId;
        Kilos = d.Kilos;
        Cajas = d.Cajas;
        ClienteId = d.ClienteId;
        PedidoVentaId = d.PedidoVentaId;
        Notas = LineaPlanta.Texto(d.Notas, 200);
        return Resultado.Ok();
    }

    public Resultado Iniciar()
    {
        if (Estado != EstadoOrdenLinea.Planificada)
        {
            return Resultado.Fallo(Error.Conflicto("orden_linea.estado", "Solo se inicia una orden planificada."));
        }

        Estado = EstadoOrdenLinea.EnCurso;
        return Resultado.Ok();
    }

    public Resultado Terminar(Guid? parteConfeccionId)
    {
        if (Estado is not (EstadoOrdenLinea.Planificada or EstadoOrdenLinea.EnCurso))
        {
            return Resultado.Fallo(Error.Conflicto("orden_linea.estado", "La orden ya está terminada o cancelada."));
        }

        Estado = EstadoOrdenLinea.Terminada;
        ParteConfeccionId = parteConfeccionId;
        return Resultado.Ok();
    }

    public Resultado Cancelar()
    {
        if (Estado is not (EstadoOrdenLinea.Planificada or EstadoOrdenLinea.EnCurso))
        {
            return Resultado.Fallo(Error.Conflicto("orden_linea.estado", "La orden ya está terminada o cancelada."));
        }

        Estado = EstadoOrdenLinea.Cancelada;
        return Resultado.Ok();
    }

    /// <summary>Vuelve a planificada una orden en curso o terminada por error (se desenlaza su parte).</summary>
    public Resultado Reabrir()
    {
        if (Estado is not (EstadoOrdenLinea.EnCurso or EstadoOrdenLinea.Terminada))
        {
            return Resultado.Fallo(Error.Conflicto("orden_linea.estado", "Solo se reabre una orden en curso o terminada."));
        }

        Estado = EstadoOrdenLinea.Planificada;
        ParteConfeccionId = null;
        return Resultado.Ok();
    }
}

public sealed record DatosOrdenLinea(DateOnly Fecha, Guid ProductoId, decimal Kilos, int Turno = 1, int? Secuencia = null, int? Cajas = null, Guid? ClienteId = null,
    Guid? PedidoVentaId = null, string? Notas = null);

public enum MotivoParada
{
    Averia = 1,
    Limpieza = 2,
    CambioFormato = 3,
    FaltaFruta = 4,
    FaltaPersonal = 5,
    Mantenimiento = 6,
    Otro = 9,
}

/// <summary>Parada de una línea: desde cuándo y hasta cuándo, y por qué. Resta de la capacidad del día.</summary>
public sealed class ParadaLinea : RaizAgregadoEmpresa<Guid>
{
    private ParadaLinea(Guid id) : base(id, Guid.Empty) { }

    private ParadaLinea(Guid id, Guid empresaId) : base(id, empresaId) { }

    public Guid LineaId { get; private set; }

    public DateOnly Fecha { get; private set; }

    public int Turno { get; private set; }

    public decimal Minutos { get; private set; }

    public MotivoParada Motivo { get; private set; }

    public string? Notas { get; private set; }

    public static Resultado<ParadaLinea> Crear(Guid empresaId, LineaPlanta linea, DatosParada d)
    {
        var p = new ParadaLinea(Guid.NewGuid(), empresaId);
        var r = p.Cambiar(linea, d);
        return r.EsFallo ? Resultado.Fallo<ParadaLinea>(r.Error) : Resultado.Ok(p);
    }

    public Resultado Cambiar(LineaPlanta linea, DatosParada d)
    {
        ArgumentNullException.ThrowIfNull(linea);
        ArgumentNullException.ThrowIfNull(d);
        if (d.Turno < 1 || d.Turno > linea.Turnos)
        {
            return Resultado.Fallo(Error.Validacion("parada.turno", $"La línea {linea.Codigo} trabaja {linea.Turnos} turno(s)."));
        }

        if (d.Minutos <= 0m || d.Minutos > linea.HorasTurno * 60m)
        {
            return Resultado.Fallo(Error.Validacion("parada.minutos", $"La parada dura entre 1 minuto y el turno entero ({linea.HorasTurno * 60m:0} minutos)."));
        }

        if (!Enum.IsDefined(d.Motivo))
        {
            return Resultado.Fallo(Error.Validacion("parada.motivo", "Motivo de parada no válido."));
        }

        LineaId = linea.Id;
        Fecha = d.Fecha;
        Turno = d.Turno;
        Minutos = d.Minutos;
        Motivo = d.Motivo;
        Notas = LineaPlanta.Texto(d.Notas, 200);
        return Resultado.Ok();
    }
}

public sealed record DatosParada(DateOnly Fecha, decimal Minutos, MotivoParada Motivo, int Turno = 1, string? Notas = null);
