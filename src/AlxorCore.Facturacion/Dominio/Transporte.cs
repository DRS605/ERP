using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Facturacion.Dominio;

public enum ModoTransporte
{
    Carretera = 1,
    Maritimo = 2,
    Aereo = 3,
    Ferrocarril = 4,
    Multimodal = 5,
}

/// <summary>Quién paga el porte.</summary>
public enum Portes
{
    Pagados = 1,
    Debidos = 2,
}

/// <summary>Empresa de transporte habitual (se copia su nombre y NIF en la carta de porte).</summary>
public sealed class Transportista : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudTexto = 120;

    private Transportista(Guid id) : base(id, Guid.Empty) { Nombre = null!; }

    private Transportista(Guid id, Guid empresaId) : base(id, empresaId) { Nombre = null!; Activo = true; }

    public string Nombre { get; private set; }

    public string? Nif { get; private set; }

    public string? Direccion { get; private set; }

    /// <summary>País (ISO alfa-2).</summary>
    public string? Pais { get; private set; }

    public string? Telefono { get; private set; }

    public bool Activo { get; private set; }

    public static Resultado<Transportista> Crear(Guid empresaId, string? nombre, string? nif, string? direccion, string? pais, string? telefono)
    {
        var t = new Transportista(Guid.NewGuid(), empresaId);
        var r = t.Modificar(nombre, nif, direccion, pais, telefono, true);
        return r.EsFallo ? Resultado.Fallo<Transportista>(r.Error) : Resultado.Ok(t);
    }

    public Resultado Modificar(string? nombre, string? nif, string? direccion, string? pais, string? telefono, bool activo)
    {
        var n = (nombre ?? string.Empty).Trim();
        if (n.Length is 0 or > LongitudTexto)
        {
            return Resultado.Fallo(Error.Validacion("transportista.nombre", $"El nombre del transportista es obligatorio (hasta {LongitudTexto} caracteres)."));
        }

        var codigoPais = Paises.Codigo(pais);
        if (!string.IsNullOrWhiteSpace(pais) && codigoPais is null)
        {
            return Resultado.Fallo(Error.Validacion("transportista.pais", "Indica el país con su código de dos letras (ES, FR…)."));
        }

        Nombre = n;
        Nif = Texto(nif, 20);
        Direccion = Texto(direccion, LongitudTexto);
        Pais = codigoPais;
        Telefono = Texto(telefono, 30);
        Activo = activo;
        return Resultado.Ok();
    }

    internal static string? Texto(string? s, int max) => string.IsNullOrWhiteSpace(s) ? null : s.Trim() is var t && t.Length > max ? t[..max] : s.Trim();
}

/// <summary>Vehículo (tractora o camión, con su remolque) con su tara y si es frigorífico.</summary>
public sealed class Vehiculo : RaizAgregadoEmpresa<Guid>
{
    private Vehiculo(Guid id) : base(id, Guid.Empty) { Matricula = null!; }

    private Vehiculo(Guid id, Guid empresaId) : base(id, empresaId) { Matricula = null!; Activo = true; }

    public string Matricula { get; private set; }

    public string? MatriculaRemolque { get; private set; }

    public string? Descripcion { get; private set; }

    public decimal? TaraKg { get; private set; }

    public bool Frigorifico { get; private set; }

    /// <summary>Transportista al que pertenece (se propone en la carta de porte).</summary>
    public Guid? TransportistaId { get; private set; }

    public bool Activo { get; private set; }

    public static Resultado<Vehiculo> Crear(Guid empresaId, string? matricula, string? remolque, string? descripcion, decimal? taraKg, bool frigorifico, Guid? transportistaId)
    {
        var v = new Vehiculo(Guid.NewGuid(), empresaId);
        var r = v.Modificar(matricula, remolque, descripcion, taraKg, frigorifico, transportistaId, true);
        return r.EsFallo ? Resultado.Fallo<Vehiculo>(r.Error) : Resultado.Ok(v);
    }

    public Resultado Modificar(string? matricula, string? remolque, string? descripcion, decimal? taraKg, bool frigorifico, Guid? transportistaId, bool activo)
    {
        var m = NormalizarMatricula(matricula);
        if (m is null)
        {
            return Resultado.Fallo(Error.Validacion("vehiculo.matricula", "La matrícula es obligatoria (hasta 15 letras o números)."));
        }

        if (taraKg is < 0m)
        {
            return Resultado.Fallo(Error.Validacion("vehiculo.tara", "La tara no puede ser negativa."));
        }

        Matricula = m;
        MatriculaRemolque = NormalizarMatricula(remolque);
        Descripcion = Transportista.Texto(descripcion, Transportista.LongitudTexto);
        TaraKg = taraKg is 0m ? null : taraKg;
        Frigorifico = frigorifico;
        TransportistaId = transportistaId is { } t && t != Guid.Empty ? t : null;
        Activo = activo;
        return Resultado.Ok();
    }

    /// <summary>Matrícula sin espacios ni guiones, en mayúsculas (1234ABC, R1234BCD…).</summary>
    public static string? NormalizarMatricula(string? matricula)
    {
        var m = new string((matricula ?? string.Empty).Where(char.IsAsciiLetterOrDigit).ToArray()).ToUpperInvariant();
        return m.Length is 0 or > 15 ? null : m;
    }
}

/// <summary>
/// Datos de transporte y de comercio exterior de una carta de porte: los del transporte (portes, documentos anexos,
/// instrucciones del remitente, Incoterm), el vehículo y los conductores, la temperatura de consigna y el termógrafo
/// (mercancía perecedera), y los de un envío marítimo o aéreo.
/// </summary>
public sealed record TransporteCarta
{

    public ModoTransporte Modo { get; init; } = ModoTransporte.Carretera;

    public Guid? TransportistaId { get; init; }

    public Guid? VehiculoId { get; init; }

    public string? MatriculaRemolque { get; init; }

    public string? Conductor { get; init; }

    public string? Conductor2 { get; init; }

    /// <summary>Temperatura de consigna (°C) de la mercancía perecedera.</summary>
    public decimal? TemperaturaConsigna { get; init; }

    /// <summary>Número del termógrafo o registrador de temperatura.</summary>
    public string? Termografo { get; init; }

    public string? Incoterm { get; init; }

    /// <summary>Lugar del Incoterm (p. ej. «Rotterdam» en un DAP Rotterdam).</summary>
    public string? LugarIncoterm { get; init; }

    public Portes? Portes { get; init; }

    /// <summary>Documentos que acompañan (factura, certificado fitosanitario…).</summary>
    public string? DocumentosAnexos { get; init; }

    /// <summary>Instrucciones del remitente (aduanas, temperatura…).</summary>
    public string? Instrucciones { get; init; }

    public string? PaisOrigen { get; init; }

    public string? PaisDestino { get; init; }

    public string? Naviera { get; init; }

    public string? Buque { get; init; }

    public string? Contenedor { get; init; }

    public string? Precinto { get; init; }

    public string? PuertoCarga { get; init; }

    public string? PuertoDestino { get; init; }

    public string? CompaniaAerea { get; init; }

    public string? Vuelo { get; init; }

    /// <summary>Conocimiento aéreo (Air Waybill).</summary>
    public string? Awb { get; init; }

    /// <summary>Número de reserva (booking) del transporte.</summary>
    public string? Reserva { get; init; }

    /// <summary>Valida y normaliza (textos recortados, códigos en mayúsculas).</summary>
    public Resultado<TransporteCarta> Normalizar()
    {
        if (!Enum.IsDefined(Modo))
        {
            return Fallo("cartaporte.modo", "El modo de transporte no es válido.");
        }

        if (Portes is { } p && !Enum.IsDefined(p))
        {
            return Fallo("cartaporte.portes", "Los portes son pagados o debidos.");
        }

        var incoterm = Incoterms.Normalizar(Incoterm);
        if (!Incoterms.EsValido(incoterm))
        {
            return Fallo("cartaporte.incoterm", $"«{Incoterm}» no es un Incoterm 2020 ({string.Join(", ", Incoterms.Todos.Keys)}).");
        }

        var origen = Paises.Codigo(PaisOrigen);
        var destino = Paises.Codigo(PaisDestino);
        if ((!string.IsNullOrWhiteSpace(PaisOrigen) && origen is null) || (!string.IsNullOrWhiteSpace(PaisDestino) && destino is null))
        {
            return Fallo("cartaporte.pais", "Indica los países con su código de dos letras (ES, FR…).");
        }

        if (TemperaturaConsigna is < -60m or > 60m)
        {
            return Fallo("cartaporte.temperatura", "La temperatura de consigna va de −60 °C a 60 °C.");
        }

        static string? T(string? s, int max = 100) => Transportista.Texto(s, max);
        return Resultado.Ok(this with
        {
            TransportistaId = TransportistaId is { } ti && ti != Guid.Empty ? ti : null,
            VehiculoId = VehiculoId is { } vi && vi != Guid.Empty ? vi : null,
            MatriculaRemolque = Vehiculo.NormalizarMatricula(MatriculaRemolque),
            Conductor = T(Conductor), Conductor2 = T(Conductor2), Termografo = T(Termografo, 40), Incoterm = incoterm, LugarIncoterm = T(LugarIncoterm),
            DocumentosAnexos = T(DocumentosAnexos, 300), Instrucciones = T(Instrucciones, 500), PaisOrigen = origen, PaisDestino = destino,
            Naviera = T(Naviera), Buque = T(Buque), Contenedor = T(Contenedor, 20)?.ToUpperInvariant(), Precinto = T(Precinto, 40),
            PuertoCarga = T(PuertoCarga), PuertoDestino = T(PuertoDestino), CompaniaAerea = T(CompaniaAerea), Vuelo = T(Vuelo, 20), Awb = T(Awb, 20), Reserva = T(Reserva, 40),
        });
    }

    private static Resultado<TransporteCarta> Fallo(string codigo, string mensaje) => Resultado.Fallo<TransporteCarta>(Error.Validacion(codigo, mensaje));
}
