using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Facturacion.Dominio;

/// <summary>Línea de mercancía de una carta de porte: qué se transporta, en cuántos bultos y su peso.</summary>
public sealed class LineaCartaPorte
{
    private LineaCartaPorte() => Descripcion = null!;

    internal LineaCartaPorte(Guid id, string descripcion, int bultos, decimal pesoKg)
    {
        Id = id;
        Descripcion = descripcion;
        Bultos = bultos;
        PesoKg = pesoKg;
    }

    public Guid Id { get; private set; }

    public string Descripcion { get; private set; }

    /// <summary>Número de bultos (unidades de carga).</summary>
    public int Bultos { get; private set; }

    /// <summary>Peso de la línea en kilogramos.</summary>
    public decimal PesoKg { get; private set; }
}

/// <summary>
/// <b>Carta de porte</b>: documento de control del transporte de mercancías por carretera (remitente,
/// destinatario, transportista, vehículo, origen/destino y relación de mercancías). En España acompaña
/// al transporte de mercancías; aquí se emite como documento (con su PDF), opcionalmente ligado a un
/// albarán de entrega. Es por empresa (documento operativo, no maestro compartido).
/// </summary>
public sealed class CartaPorte : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMaximaTexto = 200;

    private readonly List<LineaCartaPorte> _lineas = new();

    private CartaPorte(Guid id)
        : base(id, Guid.Empty)
    {
        RemitenteNombre = null!;
        DestinatarioNombre = null!;
        LugarOrigen = null!;
        LugarDestino = null!;
    }

    private CartaPorte(
        Guid id, Guid empresaId, string? serie, int ejercicio, int numero, DateOnly fechaExpedicion,
        string remitenteNombre, string? remitenteNif, Guid? destinatarioClienteId, string destinatarioNombre, string? destinatarioNif,
        string? transportistaNombre, string? transportistaNif, string? matricula, string lugarOrigen, string lugarDestino,
        DateOnly? fechaCarga, string? observaciones, Guid? albaranId, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Serie = string.IsNullOrWhiteSpace(serie) ? null : serie.Trim().ToUpperInvariant();
        Ejercicio = ejercicio;
        Numero = numero;
        FechaExpedicion = fechaExpedicion;
        RemitenteNombre = remitenteNombre;
        RemitenteNif = remitenteNif;
        DestinatarioClienteId = destinatarioClienteId;
        DestinatarioNombre = destinatarioNombre;
        DestinatarioNif = destinatarioNif;
        TransportistaNombre = transportistaNombre;
        TransportistaNif = transportistaNif;
        Matricula = matricula;
        LugarOrigen = lugarOrigen;
        LugarDestino = lugarDestino;
        FechaCarga = fechaCarga;
        Observaciones = observaciones;
        AlbaranId = albaranId;
        CreadoEn = ahora;
    }

    public string? Serie { get; private set; }

    public int Ejercicio { get; private set; }

    public int Numero { get; private set; }

    public string NumeroCompleto => Serie is { Length: > 0 } ? $"{Serie}{Ejercicio}/{Numero:D5}" : $"{Ejercicio}/{Numero:D5}";

    public DateOnly FechaExpedicion { get; private set; }

    // --- Remitente (cargador): normalmente la empresa emisora ---
    public string RemitenteNombre { get; private set; }
    public string? RemitenteNif { get; private set; }

    // --- Destinatario: el cliente que recibe la mercancía ---
    public Guid? DestinatarioClienteId { get; private set; }
    public string DestinatarioNombre { get; private set; }
    public string? DestinatarioNif { get; private set; }

    // --- Transportista y vehículo ---
    public string? TransportistaNombre { get; private set; }
    public string? TransportistaNif { get; private set; }
    public string? Matricula { get; private set; }

    // --- Ruta ---
    public string LugarOrigen { get; private set; }
    public string LugarDestino { get; private set; }
    public DateOnly? FechaCarga { get; private set; }

    public string? Observaciones { get; private set; }

    /// <summary>Albarán de entrega del que procede la carta de porte (opcional).</summary>
    public Guid? AlbaranId { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public IReadOnlyList<LineaCartaPorte> Lineas => _lineas;

    /// <summary>Total de bultos de la carta de porte.</summary>
    public int TotalBultos => _lineas.Sum(l => l.Bultos);

    /// <summary>Peso total transportado (kg), a 3 decimales.</summary>
    public decimal TotalPesoKg => Math.Round(_lineas.Sum(l => l.PesoKg), 3, MidpointRounding.AwayFromZero);

    public static Resultado<CartaPorte> Crear(
        Guid empresaId, string? serie, int ejercicio, int numero, DateOnly fechaExpedicion,
        string? remitenteNombre, string? remitenteNif, Guid? destinatarioClienteId, string? destinatarioNombre, string? destinatarioNif,
        string? transportistaNombre, string? transportistaNif, string? matricula, string? lugarOrigen, string? lugarDestino,
        DateOnly? fechaCarga, string? observaciones, Guid? albaranId,
        IReadOnlyList<(string? Descripcion, int Bultos, decimal PesoKg)> lineas, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        ArgumentNullException.ThrowIfNull(lineas);

        if (string.IsNullOrWhiteSpace(destinatarioNombre))
        {
            return Resultado.Fallo<CartaPorte>(Error.Validacion("cartaporte.destinatario_vacio", "El destinatario es obligatorio."));
        }

        var limpias = lineas
            .Where(l => !string.IsNullOrWhiteSpace(l.Descripcion))
            .Select(l => (Descripcion: l.Descripcion!.Trim(), l.Bultos, l.PesoKg))
            .ToList();
        if (limpias.Count == 0)
        {
            return Resultado.Fallo<CartaPorte>(Error.Validacion("cartaporte.sin_lineas", "La carta de porte necesita al menos una mercancía."));
        }

        if (limpias.Any(l => l.Bultos < 0 || l.PesoKg < 0))
        {
            return Resultado.Fallo<CartaPorte>(Error.Validacion("cartaporte.cantidades", "Los bultos y el peso no pueden ser negativos."));
        }

        var carta = new CartaPorte(
            Guid.NewGuid(), empresaId, serie, ejercicio, numero, fechaExpedicion,
            Recortar(remitenteNombre) ?? string.Empty, Recortar(remitenteNif), destinatarioClienteId, destinatarioNombre.Trim(), Recortar(destinatarioNif),
            Recortar(transportistaNombre), Recortar(transportistaNif), Recortar(matricula), Recortar(lugarOrigen) ?? string.Empty, Recortar(lugarDestino) ?? string.Empty,
            fechaCarga, Recortar(observaciones), albaranId, reloj.AhoraUtc);
        foreach (var l in limpias)
        {
            carta._lineas.Add(new LineaCartaPorte(Guid.NewGuid(), l.Descripcion, l.Bultos, l.PesoKg));
        }

        return Resultado.Ok(carta);
    }

    private static string? Recortar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        var limpio = valor.Trim();
        return limpio.Length > LongitudMaximaTexto ? limpio[..LongitudMaximaTexto] : limpio;
    }
}
