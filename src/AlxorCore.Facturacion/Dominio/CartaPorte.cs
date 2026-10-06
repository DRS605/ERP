using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Facturacion.Dominio;

/// <summary>
/// Mercancía de una línea: descripción, bultos y peso bruto, y las casillas del CMR (marcas, embalaje, número
/// estadístico o código arancelario, volumen) y el peso neto.
/// </summary>
public sealed record DatosLineaCarta(string? Descripcion, int Bultos, decimal PesoKg, string? Marcas = null, string? Embalaje = null,
    decimal? PesoNetoKg = null, decimal? VolumenM3 = null, string? CodigoArancelario = null);

/// <summary>Línea de mercancía de una carta de porte: qué se transporta, en cuántos bultos y su peso.</summary>
public sealed class LineaCartaPorte
{
    /// <summary>Número de la línea en el documento (1, 2, 3…).</summary>
    public int Orden { get; internal set; }

    private LineaCartaPorte() => Descripcion = null!;

    internal LineaCartaPorte(Guid id, string descripcion, int bultos, decimal pesoKg)
    {
        Id = id;
        Descripcion = descripcion;
        Bultos = bultos;
        PesoKg = pesoKg;
    }

    /// <summary>Marcas y números de los bultos (casilla 6 del CMR).</summary>
    public string? Marcas { get; private set; }

    /// <summary>Clase de embalaje: palés, cajas, a granel… (casilla 8).</summary>
    public string? Embalaje { get; private set; }

    public decimal? PesoNetoKg { get; private set; }

    /// <summary>Volumen en m³ (casilla 12).</summary>
    public decimal? VolumenM3 { get; private set; }

    /// <summary>Código arancelario (NC de 8 dígitos o TARIC de 10): número estadístico, casilla 10.</summary>
    public string? CodigoArancelario { get; private set; }

    internal void Completar(DatosLineaCarta d)
    {
        Marcas = CartaPorte.Recortar(d.Marcas);
        Embalaje = CartaPorte.Recortar(d.Embalaje);
        PesoNetoKg = d.PesoNetoKg;
        VolumenM3 = d.VolumenM3;
        CodigoArancelario = CodigosArancelarios.Normalizar(d.CodigoArancelario);
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

    /// <summary>Cuándo se anuló (el transporte no se hizo o se emitió por error); el número no se reutiliza.</summary>
    public DateTimeOffset? AnuladaEn { get; private set; }

    public string? MotivoAnulacion { get; private set; }

    // --- Tipo y comercio exterior (CMR, Incoterm, portes, países) ---
    public TipoCartaPorte Tipo { get; private set; } = TipoCartaPorte.Nacional;
    public ModoTransporte Modo { get; private set; } = ModoTransporte.Carretera;
    public string? Incoterm { get; private set; }
    public string? LugarIncoterm { get; private set; }
    public Portes? Portes { get; private set; }
    public string? DocumentosAnexos { get; private set; }
    public string? Instrucciones { get; private set; }
    public string? PaisOrigen { get; private set; }
    public string? PaisDestino { get; private set; }

    // --- Vehículo, conductores y frío ---
    public Guid? TransportistaId { get; private set; }
    public Guid? VehiculoId { get; private set; }
    public string? MatriculaRemolque { get; private set; }
    public string? Conductor { get; private set; }
    public string? Conductor2 { get; private set; }
    public decimal? TemperaturaConsigna { get; private set; }
    public string? Termografo { get; private set; }

    // --- Marítimo y aéreo ---
    public string? Naviera { get; private set; }
    public string? Buque { get; private set; }
    public string? Contenedor { get; private set; }
    public string? Precinto { get; private set; }
    public string? PuertoCarga { get; private set; }
    public string? PuertoDestino { get; private set; }
    public string? CompaniaAerea { get; private set; }
    public string? Vuelo { get; private set; }
    public string? Awb { get; private set; }
    public string? Reserva { get; private set; }

    /// <summary>
    /// Aplica los datos de transporte (ya normalizados). Sin tipo indicado, es internacional (CMR) si los países de
    /// origen y destino son distintos.
    /// </summary>
    public void AplicarTransporte(TransporteCarta t)
    {
        ArgumentNullException.ThrowIfNull(t);
        Modo = t.Modo;
        Incoterm = t.Incoterm;
        LugarIncoterm = t.LugarIncoterm;
        Portes = t.Portes;
        DocumentosAnexos = t.DocumentosAnexos;
        Instrucciones = t.Instrucciones;
        PaisOrigen = t.PaisOrigen;
        PaisDestino = t.PaisDestino;
        TransportistaId = t.TransportistaId;
        VehiculoId = t.VehiculoId;
        MatriculaRemolque = t.MatriculaRemolque;
        Conductor = t.Conductor;
        Conductor2 = t.Conductor2;
        TemperaturaConsigna = t.TemperaturaConsigna;
        Termografo = t.Termografo;
        Naviera = t.Naviera;
        Buque = t.Buque;
        Contenedor = t.Contenedor;
        Precinto = t.Precinto;
        PuertoCarga = t.PuertoCarga;
        PuertoDestino = t.PuertoDestino;
        CompaniaAerea = t.CompaniaAerea;
        Vuelo = t.Vuelo;
        Awb = t.Awb;
        Reserva = t.Reserva;
        Tipo = t.Tipo ?? (PaisOrigen is { } o && PaisDestino is { } d && o != d ? TipoCartaPorte.Internacional : TipoCartaPorte.Nacional);
    }

    /// <summary>Peso neto total, si todas las líneas lo tienen.</summary>
    public decimal? TotalPesoNetoKg => _lineas.Count > 0 && _lineas.All(l => l.PesoNetoKg is not null) ? _lineas.Sum(l => l.PesoNetoKg!.Value) : null;

    public decimal? TotalVolumenM3 => _lineas.Any(l => l.VolumenM3 is not null) ? _lineas.Sum(l => l.VolumenM3 ?? 0m) : null;

    public Resultado Anular(string? motivo, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (AnuladaEn is not null)
        {
            return Resultado.Fallo(Error.Conflicto("cartaporte.ya_anulada", "La carta de porte ya está anulada."));
        }

        AnuladaEn = reloj.AhoraUtc;
        MotivoAnulacion = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim()[..Math.Min(motivo.Trim().Length, LongitudMaximaTexto)];
        return Resultado.Ok();
    }

    /// <summary>Líneas en su orden (la base de datos no garantiza el orden en que las devuelve).</summary>
    public IReadOnlyList<LineaCartaPorte> Lineas => _lineas.OrderBy(l => l.Orden).ToList().AsReadOnly();

    /// <summary>Añade una línea con el número siguiente.</summary>
    private void AgregarLinea(LineaCartaPorte linea)
    {
        linea.Orden = _lineas.Count + 1;
        _lineas.Add(linea);
    }

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
        ArgumentNullException.ThrowIfNull(lineas);
        return Crear(empresaId, serie, ejercicio, numero, fechaExpedicion, remitenteNombre, remitenteNif, destinatarioClienteId, destinatarioNombre, destinatarioNif,
            transportistaNombre, transportistaNif, matricula, lugarOrigen, lugarDestino, fechaCarga, observaciones, albaranId,
            lineas.Select(l => new DatosLineaCarta(l.Descripcion, l.Bultos, l.PesoKg)).ToList(), reloj);
    }

    public static Resultado<CartaPorte> Crear(
        Guid empresaId, string? serie, int ejercicio, int numero, DateOnly fechaExpedicion,
        string? remitenteNombre, string? remitenteNif, Guid? destinatarioClienteId, string? destinatarioNombre, string? destinatarioNif,
        string? transportistaNombre, string? transportistaNif, string? matricula, string? lugarOrigen, string? lugarDestino,
        DateOnly? fechaCarga, string? observaciones, Guid? albaranId,
        IReadOnlyList<DatosLineaCarta> lineas, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        ArgumentNullException.ThrowIfNull(lineas);

        if (string.IsNullOrWhiteSpace(destinatarioNombre))
        {
            return Resultado.Fallo<CartaPorte>(Error.Validacion("cartaporte.destinatario_vacio", "El destinatario es obligatorio."));
        }

        var limpias = lineas.Where(l => !string.IsNullOrWhiteSpace(l.Descripcion)).ToList();
        if (limpias.Count == 0)
        {
            return Resultado.Fallo<CartaPorte>(Error.Validacion("cartaporte.sin_lineas", "La carta de porte necesita al menos una mercancía."));
        }

        if (limpias.Any(l => l.Bultos < 0 || l.PesoKg < 0 || l.PesoNetoKg < 0 || l.VolumenM3 < 0))
        {
            return Resultado.Fallo<CartaPorte>(Error.Validacion("cartaporte.cantidades", "Los bultos, los pesos y el volumen no pueden ser negativos."));
        }

        if (limpias.FirstOrDefault(l => !string.IsNullOrWhiteSpace(l.CodigoArancelario) && CodigosArancelarios.Normalizar(l.CodigoArancelario) is null) is { } mal)
        {
            return Resultado.Fallo<CartaPorte>(Error.Validacion("cartaporte.codigo_arancelario", $"«{mal.CodigoArancelario}» no es un código arancelario (8 dígitos NC o 10 TARIC)."));
        }

        var carta = new CartaPorte(
            Guid.NewGuid(), empresaId, serie, ejercicio, numero, fechaExpedicion,
            Recortar(remitenteNombre) ?? string.Empty, Recortar(remitenteNif), destinatarioClienteId, destinatarioNombre.Trim(), Recortar(destinatarioNif),
            Recortar(transportistaNombre), Recortar(transportistaNif), Recortar(matricula), Recortar(lugarOrigen) ?? string.Empty, Recortar(lugarDestino) ?? string.Empty,
            fechaCarga, Recortar(observaciones), albaranId, reloj.AhoraUtc);
        foreach (var l in limpias)
        {
            var linea = new LineaCartaPorte(Guid.NewGuid(), l.Descripcion!.Trim(), l.Bultos, l.PesoKg);
            linea.Completar(l);
            carta.AgregarLinea(linea);
        }

        return Resultado.Ok(carta);
    }

    internal static string? Recortar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        var limpio = valor.Trim();
        return limpio.Length > LongitudMaximaTexto ? limpio[..LongitudMaximaTexto] : limpio;
    }
}
