using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Agro.Dominio;

/// <summary>Estado de la orden de carga (<c>TipoEstadoOrdenCarga</c> de Hispatec).</summary>
public enum EstadoOrdenCarga
{
    /// <summary>Propuesta: se está preparando, no se carga todavía.</summary>
    Propuesta,

    /// <summary>Lista para cargar.</summary>
    Pendiente,

    /// <summary>Con algún palé cargado.</summary>
    EnCarga,

    /// <summary>Expedida: sus palés salieron con sus albaranes.</summary>
    Finalizada,

    Anulada,
}

/// <summary>Línea prevista de la orden: una línea de pedido de venta con los palés que se piensan cargar.</summary>
public sealed class LineaOrdenCarga
{
    private LineaOrdenCarga()
    {
        Descripcion = null!;
    }

    internal LineaOrdenCarga(int orden, Guid pedidoVentaId, Guid lineaPedidoId, Guid? productoId, string descripcion, int palesPrevistos, int? fila, int? columna)
    {
        Id = Guid.NewGuid();
        Orden = orden;
        PedidoVentaId = pedidoVentaId;
        LineaPedidoId = lineaPedidoId;
        ProductoId = productoId;
        Descripcion = descripcion;
        PalesPrevistos = palesPrevistos;
        Fila = fila;
        Columna = columna;
    }

    public Guid Id { get; private set; }

    /// <summary>Orden de carga de la línea (1, 2…).</summary>
    public int Orden { get; private set; }

    public Guid PedidoVentaId { get; private set; }

    public Guid LineaPedidoId { get; private set; }

    public Guid? ProductoId { get; private set; }

    public string Descripcion { get; private set; }

    public int PalesPrevistos { get; private set; }

    /// <summary>Posición en el camión (opcional).</summary>
    public int? Fila { get; private set; }

    public int? Columna { get; private set; }
}

/// <summary>Palé cargado en la orden, sobre una de sus líneas; al finalizar, con el albarán con que salió.</summary>
public sealed class PaleCargado
{
    private PaleCargado()
    {
        Sscc = null!;
    }

    internal PaleCargado(Guid lineaId, Guid paleId, string sscc, decimal kilos, int? fila, int? columna, DateTimeOffset ahora)
    {
        Id = Guid.NewGuid();
        LineaId = lineaId;
        PaleId = paleId;
        Sscc = sscc;
        Kilos = kilos;
        Fila = fila;
        Columna = columna;
        CargadoEn = ahora;
    }

    public Guid Id { get; private set; }

    public Guid LineaId { get; private set; }

    public Guid PaleId { get; private set; }

    public string Sscc { get; private set; }

    public decimal Kilos { get; private set; }

    public int? Fila { get; private set; }

    public int? Columna { get; private set; }

    public DateTimeOffset CargadoEn { get; private set; }

    /// <summary>Albarán con que salió (null hasta finalizar la orden).</summary>
    public Guid? AlbaranId { get; private set; }

    internal void Expedido(Guid? albaranId) => AlbaranId = albaranId ?? Guid.Empty;

    public bool EstaExpedido => AlbaranId is not null;
}

/// <summary>
/// Orden de carga ligera sobre la expedición de palés, como <c>OrdenesCarga</c> de Hispatec: fecha de carga, muelle,
/// transportista, vehículo, conductor y temperatura; líneas de pedidos de venta con los palés previstos y su posición; se
/// cargan palés (validados contra la línea y su reserva) y al finalizar se expiden por pedido —un albarán por pedido—,
/// quedando enlazada a sus albaranes. Una línea con palés cargados no se quita.
/// </summary>
public sealed class OrdenCarga : RaizAgregadoEmpresa<Guid>
{
    private readonly List<LineaOrdenCarga> _lineas = [];
    private readonly List<PaleCargado> _cargados = [];

    private OrdenCarga(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private OrdenCarga(Guid id, Guid empresaId, int ejercicio, int numero, DateOnly fechaCarga, bool propuesta, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Ejercicio = ejercicio;
        Numero = numero;
        FechaCarga = fechaCarga;
        Estado = propuesta ? EstadoOrdenCarga.Propuesta : EstadoOrdenCarga.Pendiente;
        CreadaEn = ahora;
    }

    public int Ejercicio { get; private set; }

    public int Numero { get; private set; }

    public string NumeroCompleto => $"OC-{Ejercicio}-{Numero:D6}";

    public DateOnly FechaCarga { get; private set; }

    public EstadoOrdenCarga Estado { get; private set; }

    public string? Muelle { get; private set; }

    public Guid? TransportistaId { get; private set; }

    public Guid? VehiculoId { get; private set; }

    public string? Matricula { get; private set; }

    public string? Conductor { get; private set; }

    public decimal? TemperaturaConsigna { get; private set; }

    /// <summary>Filas × columnas del camión, para colocar los palés (opcional).</summary>
    public int? Filas { get; private set; }

    public int? Columnas { get; private set; }

    /// <summary>Al finalizar, además del albarán, carta de porte (una por pedido: un destinatario).</summary>
    public bool CartaPorte { get; private set; }

    public string? Observaciones { get; private set; }

    public DateTimeOffset CreadaEn { get; private set; }

    public DateTimeOffset? FinalizadaEn { get; private set; }

    public string? MotivoAnulacion { get; private set; }

    public IReadOnlyList<LineaOrdenCarga> Lineas => _lineas.OrderBy(l => l.Orden).ToList();

    public IReadOnlyList<PaleCargado> Cargados => _cargados.OrderBy(c => c.CargadoEn).ToList();

    public static OrdenCarga Crear(Guid empresaId, int numero, DateOnly fechaCarga, bool propuesta, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        return new OrdenCarga(Guid.NewGuid(), empresaId, fechaCarga.Year, numero, fechaCarga, propuesta, reloj.AhoraUtc);
    }

    private bool Abierta => Estado is EstadoOrdenCarga.Propuesta or EstadoOrdenCarga.Pendiente or EstadoOrdenCarga.EnCarga;

    public Resultado Datos(DateOnly fechaCarga, string? muelle, Guid? transportistaId, Guid? vehiculoId, string? matricula, string? conductor, decimal? temperatura,
        int? filas, int? columnas, bool cartaPorte, string? observaciones)
    {
        if (!Abierta)
        {
            return Resultado.Fallo(Error.Conflicto("ordencarga.cerrada", "La orden de carga ya está finalizada o anulada."));
        }

        if (filas is < 1 or > 20 || columnas is < 1 or > 6 || (filas is null) != (columnas is null))
        {
            return Resultado.Fallo(Error.Validacion("ordencarga.distribucion", "El camión va de 1 a 20 filas y de 1 a 6 columnas (indica las dos o ninguna)."));
        }

        if (filas is { } f && columnas is { } c && _lineas.Any(l => l.Fila > f || l.Columna > c))
        {
            return Resultado.Fallo(Error.Validacion("ordencarga.posicion", "Hay líneas colocadas fuera de esa distribución."));
        }

        static string? T(string? t, int max) => string.IsNullOrWhiteSpace(t) ? null : t.Trim()[..Math.Min(t.Trim().Length, max)];
        FechaCarga = fechaCarga;
        Muelle = T(muelle, 50);
        TransportistaId = transportistaId;
        VehiculoId = vehiculoId;
        Matricula = T(matricula, 20);
        Conductor = T(conductor, 100);
        TemperaturaConsigna = temperatura;
        Filas = filas;
        Columnas = columnas;
        CartaPorte = cartaPorte;
        Observaciones = T(observaciones, 500);
        return Resultado.Ok();
    }

    /// <summary>La propuesta pasa a pendiente de cargar.</summary>
    public Resultado Liberar()
    {
        if (Estado != EstadoOrdenCarga.Propuesta)
        {
            return Resultado.Fallo(Error.Conflicto("ordencarga.no_propuesta", "Solo se libera una orden en propuesta."));
        }

        Estado = EstadoOrdenCarga.Pendiente;
        return Resultado.Ok();
    }

    public Resultado<LineaOrdenCarga> AgregarLinea(Guid pedidoVentaId, Guid lineaPedidoId, Guid? productoId, string descripcion, int palesPrevistos, int? fila, int? columna)
    {
        if (!Abierta)
        {
            return Resultado.Fallo<LineaOrdenCarga>(Error.Conflicto("ordencarga.cerrada", "La orden de carga ya está finalizada o anulada."));
        }

        if (_lineas.Any(l => l.LineaPedidoId == lineaPedidoId))
        {
            return Resultado.Fallo<LineaOrdenCarga>(Error.Conflicto("ordencarga.linea_repetida", "Esa línea de pedido ya está en la orden."));
        }

        if (palesPrevistos < 1)
        {
            return Resultado.Fallo<LineaOrdenCarga>(Error.Validacion("ordencarga.pales", "Indica cuántos palés se cargan de la línea."));
        }

        var posicion = ValidarPosicion(fila, columna);
        if (posicion.EsFallo)
        {
            return Resultado.Fallo<LineaOrdenCarga>(posicion.Error);
        }

        var linea = new LineaOrdenCarga(_lineas.Count == 0 ? 1 : _lineas.Max(l => l.Orden) + 1, pedidoVentaId, lineaPedidoId, productoId, descripcion, palesPrevistos, fila, columna);
        _lineas.Add(linea);
        return Resultado.Ok(linea);
    }

    /// <summary>Quita una línea sin palés cargados (<c>NoPermitirModificarConLineasCargadasOC</c>).</summary>
    public Resultado QuitarLinea(Guid lineaId)
    {
        var linea = _lineas.SingleOrDefault(l => l.Id == lineaId);
        if (linea is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("ordencarga.linea", "La línea no está en la orden."));
        }

        if (!Abierta || _cargados.Any(c => c.LineaId == lineaId))
        {
            return Resultado.Fallo(Error.Conflicto("ordencarga.linea_cargada", "La línea ya tiene palés cargados: descárgalos antes."));
        }

        _lineas.Remove(linea);
        return Resultado.Ok();
    }

    /// <summary>Carga un palé en una línea: no más palés que los previstos, y un palé una sola vez.</summary>
    public Resultado<PaleCargado> Cargar(Guid lineaId, Guid paleId, string sscc, decimal kilos, int? fila, int? columna, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (Estado is not (EstadoOrdenCarga.Pendiente or EstadoOrdenCarga.EnCarga))
        {
            return Resultado.Fallo<PaleCargado>(Error.Conflicto("ordencarga.no_cargable",
                Estado == EstadoOrdenCarga.Propuesta ? "La orden está en propuesta: libérala antes de cargar." : "La orden ya está finalizada o anulada."));
        }

        var linea = _lineas.SingleOrDefault(l => l.Id == lineaId);
        if (linea is null)
        {
            return Resultado.Fallo<PaleCargado>(Error.NoEncontrado("ordencarga.linea", "La línea no está en la orden."));
        }

        if (_cargados.Any(c => c.PaleId == paleId))
        {
            return Resultado.Fallo<PaleCargado>(Error.Conflicto("ordencarga.pale_cargado", $"{sscc} ya está cargado en esta orden."));
        }

        if (_cargados.Count(c => c.LineaId == lineaId) >= linea.PalesPrevistos)
        {
            return Resultado.Fallo<PaleCargado>(Error.Conflicto("ordencarga.linea_completa", $"La línea {linea.Orden} ya tiene sus {linea.PalesPrevistos} palé(s)."));
        }

        var posicion = ValidarPosicion(fila ?? linea.Fila, columna ?? linea.Columna);
        if (posicion.EsFallo)
        {
            return Resultado.Fallo<PaleCargado>(posicion.Error);
        }

        var cargado = new PaleCargado(lineaId, paleId, sscc, kilos, fila ?? linea.Fila, columna ?? linea.Columna, reloj.AhoraUtc);
        _cargados.Add(cargado);
        Estado = EstadoOrdenCarga.EnCarga;
        return Resultado.Ok(cargado);
    }

    public Resultado Descargar(Guid paleId)
    {
        var cargado = _cargados.SingleOrDefault(c => c.PaleId == paleId);
        if (cargado is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("ordencarga.pale", "Ese palé no está cargado en la orden."));
        }

        if (cargado.EstaExpedido || !Abierta)
        {
            return Resultado.Fallo(Error.Conflicto("ordencarga.pale_expedido", "El palé ya salió con su albarán."));
        }

        _cargados.Remove(cargado);
        if (_cargados.Count == 0)
        {
            Estado = EstadoOrdenCarga.Pendiente;
        }

        return Resultado.Ok();
    }

    /// <summary>Anota con qué albarán salieron los palés de un pedido; con todos expedidos, la orden queda finalizada.</summary>
    public void Expedidos(Guid pedidoVentaId, Guid? albaranId, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        var lineas = _lineas.Where(l => l.PedidoVentaId == pedidoVentaId).Select(l => l.Id).ToHashSet();
        foreach (var c in _cargados.Where(c => lineas.Contains(c.LineaId) && !c.EstaExpedido))
        {
            c.Expedido(albaranId);
        }

        if (_cargados.Count > 0 && _cargados.All(c => c.EstaExpedido))
        {
            Estado = EstadoOrdenCarga.Finalizada;
            FinalizadaEn = reloj.AhoraUtc;
        }
    }

    public Resultado PuedeFinalizarse()
    {
        if (Estado != EstadoOrdenCarga.EnCarga)
        {
            return Resultado.Fallo(Error.Conflicto("ordencarga.sin_carga", Estado == EstadoOrdenCarga.Finalizada ? "La orden ya está finalizada." : "No hay palés cargados."));
        }

        return Resultado.Ok();
    }

    /// <summary>Anula una orden sin nada expedido (los palés cargados quedan libres).</summary>
    public Resultado Anular(string? motivo)
    {
        if (!Abierta)
        {
            return Resultado.Fallo(Error.Conflicto("ordencarga.cerrada", "La orden ya está finalizada o anulada."));
        }

        if (_cargados.Any(c => c.EstaExpedido))
        {
            return Resultado.Fallo(Error.Conflicto("ordencarga.con_expedidos", "Parte de la orden ya salió: termina de expedirla o anula sus albaranes."));
        }

        _cargados.Clear();
        Estado = EstadoOrdenCarga.Anulada;
        MotivoAnulacion = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim()[..Math.Min(motivo.Trim().Length, 200)];
        return Resultado.Ok();
    }

    private Resultado ValidarPosicion(int? fila, int? columna)
    {
        if (fila is null && columna is null)
        {
            return Resultado.Ok();
        }

        if (Filas is null || Columnas is null || fila is not { } f || columna is not { } c || f < 1 || f > Filas || c < 1 || c > Columnas)
        {
            return Resultado.Fallo(Error.Validacion("ordencarga.posicion",
                Filas is null ? "Indica antes las filas y columnas del camión." : $"La posición va de 1 a {Filas} (fila) y de 1 a {Columnas} (columna)."));
        }

        return Resultado.Ok();
    }
}
