using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Logistica.Dominio;

/// <summary>Configuración logística de la empresa: el prefijo GS1 con que numera sus SSCC y el último número usado.</summary>
public sealed class ConfiguracionLogistica : RaizAgregadoEmpresa<Guid>
{
    /// <summary>Prefijo de pruebas (el de España sin empresa): se cambia por el que asigna GS1 a la empresa.</summary>
    public const string PrefijoPruebas = "8400000";

    private ConfiguracionLogistica(Guid id)
        : base(id, Guid.Empty)
    {
        PrefijoGs1 = null!;
    }

    private ConfiguracionLogistica(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
        PrefijoGs1 = PrefijoPruebas;
    }

    public string PrefijoGs1 { get; private set; }

    public int DigitoExtension { get; private set; }

    public long UltimaSerie { get; private set; }

    /// <summary>Un palé puede llevar varios lotes del mismo artículo (si no, cada lote empieza palé).</summary>
    public bool MezclarLotes { get; private set; }

    public static ConfiguracionLogistica Crear(Guid empresaId) => new(Guid.NewGuid(), empresaId);

    public Resultado Actualizar(string? prefijo, int extension, bool mezclarLotes)
    {
        var p = prefijo?.Trim();
        if (p is null || p.Length is < 7 or > 10 || !p.All(char.IsAsciiDigit))
        {
            return Resultado.Fallo(Error.Validacion("logistica.prefijo_gs1", "El prefijo de empresa GS1 tiene de 7 a 10 dígitos."));
        }

        if (extension is < 0 or > 9)
        {
            return Resultado.Fallo(Error.Validacion("logistica.extension_sscc", "El dígito de extensión va de 0 a 9."));
        }

        if (p != PrefijoGs1 || extension != DigitoExtension)
        {
            UltimaSerie = 0;
        }

        PrefijoGs1 = p;
        DigitoExtension = extension;
        MezclarLotes = mezclarLotes;
        return Resultado.Ok();
    }

    /// <summary>Siguiente SSCC (hay que haber bloqueado la numeración antes: dos peticiones no se cruzan).</summary>
    public Resultado<string> SiguienteSscc()
    {
        var sscc = Gs1.Sscc(DigitoExtension, PrefijoGs1, UltimaSerie + 1);
        if (sscc is null)
        {
            return Resultado.Fallo<string>(Error.Conflicto("logistica.sscc_agotado", "Se ha agotado la numeración de SSCC del prefijo GS1."));
        }

        UltimaSerie++;
        return Resultado.Ok(sscc);
    }
}

/// <summary>Soporte de la unidad logística: europalé 1200×800, palé americano 1200×1000, medio palé, contenedor…</summary>
public sealed class TipoSoporte : RaizAgregadoEmpresa<Guid>
{
    private TipoSoporte(Guid id)
        : base(id, Guid.Empty)
    {
        Codigo = null!;
        Nombre = null!;
    }

    private TipoSoporte(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
        Codigo = null!;
        Nombre = null!;
    }

    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    public int LargoMm { get; private set; }

    public int AnchoMm { get; private set; }

    /// <summary>Altura del propio soporte (un europalé, 144 mm).</summary>
    public int AltoMm { get; private set; }

    public decimal TaraKg { get; private set; }

    /// <summary>Carga máxima que admite (peso de la mercancía).</summary>
    public decimal? CargaMaxKg { get; private set; }

    /// <summary>Artículo del envase retornable (para la cuenta de envases con el cliente o el pool).</summary>
    public Guid? EnvaseProductoId { get; private set; }

    public bool Activo { get; private set; }

    public static Resultado<TipoSoporte> Crear(Guid empresaId, string? codigo, string? nombre, int largo, int ancho, int alto, decimal tara, decimal? cargaMax,
        Guid? envase)
    {
        if (string.IsNullOrWhiteSpace(codigo) || codigo.Trim().Length > 20)
        {
            return Resultado.Fallo<TipoSoporte>(Error.Validacion("soporte.codigo", "Indica un código de hasta 20 caracteres."));
        }

        var s = new TipoSoporte(Guid.NewGuid(), empresaId) { Codigo = codigo.Trim().ToUpperInvariant(), Activo = true };
        var r = s.Actualizar(nombre, largo, ancho, alto, tara, cargaMax, envase, true);
        return r.EsFallo ? Resultado.Fallo<TipoSoporte>(r.Error) : Resultado.Ok(s);
    }

    public Resultado Actualizar(string? nombre, int largo, int ancho, int alto, decimal tara, decimal? cargaMax, Guid? envase, bool activo)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return Resultado.Fallo(Error.Validacion("soporte.nombre", "Indica el nombre del soporte."));
        }

        if (largo is < 1 or > 20_000 || ancho is < 1 or > 5_000 || alto is < 0 or > 3_000 || tara < 0m || cargaMax is <= 0m)
        {
            return Resultado.Fallo(Error.Validacion("soporte.medidas", "Medidas en mm (largo y ancho mayores que 0), tara no negativa y carga máxima positiva."));
        }

        Nombre = nombre.Trim()[..Math.Min(nombre.Trim().Length, 100)];
        LargoMm = largo;
        AnchoMm = ancho;
        AltoMm = alto;
        TaraKg = Redondeo.Dos(tara);
        CargaMaxKg = cargaMax;
        EnvaseProductoId = envase;
        Activo = activo;
        return Resultado.Ok();
    }

    public void DarDeBaja() => Activo = false;
}

/// <summary>Datos de la ficha logística de un artículo (unidad de consumo, caja y paletizado por defecto).</summary>
public sealed record DatosFichaLogistica(
    string? Gtin, string? GtinCaja, int UnidadesPorCaja, decimal? PesoNetoUnidadKg, decimal? PesoBrutoCajaKg, int? LargoCajaMm, int? AnchoCajaMm, int? AltoCajaMm,
    int? CajasPorCapa, int? Capas, Guid? SoporteId, int? AlturaMaxPaleMm, decimal? PesoMaxPaleKg, bool Remontable = false, int? TemperaturaMinC = null,
    int? TemperaturaMaxC = null, int? VidaUtilDias = null, int? VidaMinimaEntregaDias = null, bool GestionLotes = true);

/// <summary>
/// Ficha logística del artículo: GTIN de la unidad y de la caja (ITF-14), unidades por caja, pesos y medidas de la caja,
/// mosaico por defecto (cajas por capa y capas) sobre su soporte, límites de altura y peso del palé, si se puede
/// remontar, temperatura de conservación, vida útil (para fijar la caducidad del lote al fabricar) y la vida mínima que
/// tiene que quedarle al entregarlo (para no mandar producto a punto de caducar).
/// </summary>
public sealed class FichaLogistica : RaizAgregadoEmpresa<Guid>
{
    private FichaLogistica(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private FichaLogistica(Guid id, Guid empresaId, Guid productoId)
        : base(id, empresaId)
    {
        ProductoId = productoId;
    }

    public Guid ProductoId { get; private set; }

    public string? Gtin { get; private set; }

    public string? GtinCaja { get; private set; }

    public int UnidadesPorCaja { get; private set; }

    public decimal? PesoNetoUnidadKg { get; private set; }

    public decimal? PesoBrutoCajaKg { get; private set; }

    public int? LargoCajaMm { get; private set; }

    public int? AnchoCajaMm { get; private set; }

    public int? AltoCajaMm { get; private set; }

    public int? CajasPorCapa { get; private set; }

    public int? Capas { get; private set; }

    public Guid? SoporteId { get; private set; }

    public int? AlturaMaxPaleMm { get; private set; }

    public decimal? PesoMaxPaleKg { get; private set; }

    public bool Remontable { get; private set; }

    public int? TemperaturaMinC { get; private set; }

    public int? TemperaturaMaxC { get; private set; }

    public int? VidaUtilDias { get; private set; }

    public int? VidaMinimaEntregaDias { get; private set; }

    public bool GestionLotes { get; private set; }

    /// <summary>Peso neto de una caja (unidades por caja × peso de la unidad).</summary>
    public decimal? PesoNetoCajaKg => PesoNetoUnidadKg is { } u ? Math.Round(u * UnidadesPorCaja, 3, MidpointRounding.AwayFromZero) : null;

    public static Resultado<FichaLogistica> Crear(Guid empresaId, Guid productoId, DatosFichaLogistica d)
    {
        var f = new FichaLogistica(Guid.NewGuid(), empresaId, productoId);
        var r = f.Actualizar(d);
        return r.EsFallo ? Resultado.Fallo<FichaLogistica>(r.Error) : Resultado.Ok(f);
    }

    public Resultado Actualizar(DatosFichaLogistica d)
    {
        ArgumentNullException.ThrowIfNull(d);
        var gtin = string.IsNullOrWhiteSpace(d.Gtin) ? null : d.Gtin.Trim();
        var gtinCaja = string.IsNullOrWhiteSpace(d.GtinCaja) ? null : d.GtinCaja.Trim();
        if ((gtin is not null && !Gs1.EsGtinValido(gtin)) || (gtinCaja is not null && !Gs1.EsGtinValido(gtinCaja)))
        {
            return Resultado.Fallo(Error.Validacion("ficha_logistica.gtin", "GTIN no válido: 8, 12, 13 o 14 dígitos con su dígito de control."));
        }

        if (d.UnidadesPorCaja < 1 || d.PesoNetoUnidadKg is < 0m || d.PesoBrutoCajaKg is < 0m || d.LargoCajaMm is < 1 || d.AnchoCajaMm is < 1 || d.AltoCajaMm is < 1
            || d.CajasPorCapa is < 1 || d.Capas is < 1 || d.AlturaMaxPaleMm is < 1 || d.PesoMaxPaleKg is <= 0m || d.VidaUtilDias is < 1 || d.VidaMinimaEntregaDias is < 0
            || (d.TemperaturaMinC is { } tmin && d.TemperaturaMaxC is { } tmax && tmin > tmax))
        {
            return Resultado.Fallo(Error.Validacion("ficha_logistica.valores",
                "Unidades por caja de 1 en adelante, pesos y medidas positivos, temperatura mínima no mayor que la máxima."));
        }

        if (d.PesoNetoUnidadKg is { } u && d.PesoBrutoCajaKg is { } b && b < u * d.UnidadesPorCaja)
        {
            return Resultado.Fallo(Error.Validacion("ficha_logistica.peso", "El peso bruto de la caja no puede ser menor que el neto de sus unidades."));
        }

        Gtin = gtin is null ? null : Gs1.Gtin14(gtin);
        GtinCaja = gtinCaja is null ? null : Gs1.Gtin14(gtinCaja);
        UnidadesPorCaja = d.UnidadesPorCaja;
        PesoNetoUnidadKg = d.PesoNetoUnidadKg;
        PesoBrutoCajaKg = d.PesoBrutoCajaKg;
        LargoCajaMm = d.LargoCajaMm;
        AnchoCajaMm = d.AnchoCajaMm;
        AltoCajaMm = d.AltoCajaMm;
        CajasPorCapa = d.CajasPorCapa;
        Capas = d.Capas;
        SoporteId = d.SoporteId;
        AlturaMaxPaleMm = d.AlturaMaxPaleMm;
        PesoMaxPaleKg = d.PesoMaxPaleKg;
        Remontable = d.Remontable;
        TemperaturaMinC = d.TemperaturaMinC;
        TemperaturaMaxC = d.TemperaturaMaxC;
        VidaUtilDias = d.VidaUtilDias;
        VidaMinimaEntregaDias = d.VidaMinimaEntregaDias;
        GestionLotes = d.GestionLotes;
        return Resultado.Ok();
    }
}

/// <summary>
/// Plantilla de paletizado de un artículo, en general o para un cliente: el soporte y el mosaico (cajas por capa y
/// capas), con los límites de altura y peso que pida el cliente. Manda la del cliente; si no, la general; si no, la ficha.
/// </summary>
public sealed class PlantillaPaletizado : RaizAgregadoEmpresa<Guid>
{
    private PlantillaPaletizado(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private PlantillaPaletizado(Guid id, Guid empresaId, Guid productoId)
        : base(id, empresaId)
    {
        ProductoId = productoId;
    }

    public Guid ProductoId { get; private set; }

    public Guid? ClienteId { get; private set; }

    public Guid SoporteId { get; private set; }

    public int CajasPorCapa { get; private set; }

    public int Capas { get; private set; }

    public int? AlturaMaxMm { get; private set; }

    public decimal? PesoMaxKg { get; private set; }

    /// <summary>Etiqueta, film, esquineros, hojas entre capas…</summary>
    public string? Instrucciones { get; private set; }

    public bool Activa { get; private set; }

    public int CajasPorPale => CajasPorCapa * Capas;

    public static Resultado<PlantillaPaletizado> Crear(Guid empresaId, Guid productoId, Guid? clienteId, Guid soporteId, int cajasPorCapa, int capas, int? alturaMax,
        decimal? pesoMax, string? instrucciones)
    {
        var p = new PlantillaPaletizado(Guid.NewGuid(), empresaId, productoId) { ClienteId = clienteId };
        var r = p.Actualizar(soporteId, cajasPorCapa, capas, alturaMax, pesoMax, instrucciones, true);
        return r.EsFallo ? Resultado.Fallo<PlantillaPaletizado>(r.Error) : Resultado.Ok(p);
    }

    public Resultado Actualizar(Guid soporteId, int cajasPorCapa, int capas, int? alturaMax, decimal? pesoMax, string? instrucciones, bool activa)
    {
        if (cajasPorCapa < 1 || capas < 1 || alturaMax is < 1 || pesoMax is <= 0m)
        {
            return Resultado.Fallo(Error.Validacion("plantilla_paletizado.mosaico", "Cajas por capa y capas de 1 en adelante; altura y peso máximos positivos."));
        }

        SoporteId = soporteId;
        CajasPorCapa = cajasPorCapa;
        Capas = capas;
        AlturaMaxMm = alturaMax;
        PesoMaxKg = pesoMax;
        Instrucciones = string.IsNullOrWhiteSpace(instrucciones) ? null : instrucciones.Trim()[..Math.Min(instrucciones.Trim().Length, 500)];
        Activa = activa;
        return Resultado.Ok();
    }
}
