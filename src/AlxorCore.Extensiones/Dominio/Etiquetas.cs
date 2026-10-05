using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Extensiones.Dominio;

/// <summary>Dato que puede salir en una etiqueta logística (el SSCC y sus códigos GS1-128 salen siempre).</summary>
public enum CampoEtiqueta
{
    Producto = 1,
    Marca = 2,
    Cajas = 3,
    PesoNeto = 4,
    PesoBruto = 5,
    Lote = 6,
    Fecha = 7,
    Caducidad = 8,
    Gtin = 9,
    TipoPale = 10,
    Origen = 11,
    Destinatario = 12,

    /// <summary>Código y descripción del artículo en el cliente (su referencia).</summary>
    ReferenciaCliente = 13,

    /// <summary>El texto fijo de la plantilla (categoría, conservación, «Producto de España»…).</summary>
    TextoLibre = 14,
}

/// <summary>Tamaño de la etiqueta.</summary>
public enum FormatoEtiqueta
{
    /// <summary>A6 (105 × 148 mm), la de siempre.</summary>
    A6 = 1,

    /// <summary>Etiqueta de rollo de 100 × 150 mm (impresoras térmicas).</summary>
    Rollo100x150 = 2,
}

/// <summary>
/// Plantilla de etiqueta de palé de un cliente o plataforma (Mercadona, Lidl, Carrefour…) o la general de la empresa:
/// marca comercial, qué datos salen y en qué orden, un texto fijo y el tamaño.
/// </summary>
public sealed class PlantillaEtiqueta : RaizAgregadoEmpresa<Guid>
{
    public static IReadOnlyList<CampoEtiqueta> CamposPorDefecto { get; } =
    [
        CampoEtiqueta.Producto, CampoEtiqueta.Marca, CampoEtiqueta.Cajas, CampoEtiqueta.PesoNeto, CampoEtiqueta.Lote, CampoEtiqueta.Caducidad, CampoEtiqueta.Gtin,
        CampoEtiqueta.PesoBruto, CampoEtiqueta.TipoPale, CampoEtiqueta.Origen, CampoEtiqueta.Destinatario,
    ];

    private PlantillaEtiqueta(Guid id)
        : base(id, Guid.Empty)
    {
        Nombre = null!;
        Campos = null!;
    }

    private PlantillaEtiqueta(Guid id, Guid empresaId, Guid? clienteId)
        : base(id, empresaId)
    {
        ClienteId = clienteId;
        Nombre = string.Empty;
        Campos = string.Empty;
        Activa = true;
    }

    public string Nombre { get; private set; }

    /// <summary>Cliente o plataforma a la que se aplica; null: la general de la empresa.</summary>
    public Guid? ClienteId { get; private set; }

    public string? Marca { get; private set; }

    /// <summary>Campos que salen, en orden (separados por comas, por nombre).</summary>
    public string Campos { get; private set; }

    public string? TextoLibre { get; private set; }

    public FormatoEtiqueta Formato { get; private set; }

    public bool Activa { get; private set; }

    public IReadOnlyList<CampoEtiqueta> ListaCampos =>
        Campos.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(c => Enum.Parse<CampoEtiqueta>(c)).ToList();

    public static Resultado<PlantillaEtiqueta> Crear(Guid empresaId, Guid? clienteId, string? nombre, string? marca, IReadOnlyList<CampoEtiqueta>? campos, string? texto,
        FormatoEtiqueta formato)
    {
        var p = new PlantillaEtiqueta(Guid.NewGuid(), empresaId, clienteId == Guid.Empty ? null : clienteId);
        var r = p.Cambiar(nombre, marca, campos, texto, formato, true);
        return r.EsFallo ? Resultado.Fallo<PlantillaEtiqueta>(r.Error) : Resultado.Ok(p);
    }

    public Resultado Cambiar(string? nombre, string? marca, IReadOnlyList<CampoEtiqueta>? campos, string? texto, FormatoEtiqueta formato, bool activa)
    {
        var n = nombre?.Trim();
        if (string.IsNullOrEmpty(n) || n.Length > 120)
        {
            return Resultado.Fallo(Error.Validacion("etiqueta.nombre", "Ponle un nombre a la plantilla (hasta 120 caracteres)."));
        }

        var lista = (campos is { Count: > 0 } ? campos : CamposPorDefecto).Distinct().ToList();
        if (lista.Any(c => !Enum.IsDefined(c)))
        {
            return Resultado.Fallo(Error.Validacion("etiqueta.campo", "Hay un campo de etiqueta que no existe."));
        }

        if (!Enum.IsDefined(formato))
        {
            return Resultado.Fallo(Error.Validacion("etiqueta.formato", "El formato es A6 o Rollo100x150."));
        }

        var t = string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
        if (t?.Length > 300)
        {
            return Resultado.Fallo(Error.Validacion("etiqueta.texto", "El texto fijo admite hasta 300 caracteres."));
        }

        var m = string.IsNullOrWhiteSpace(marca) ? null : marca.Trim();
        if (m?.Length > 60)
        {
            return Resultado.Fallo(Error.Validacion("etiqueta.marca", "La marca admite hasta 60 caracteres."));
        }

        if (t is not null && !lista.Contains(CampoEtiqueta.TextoLibre))
        {
            lista.Add(CampoEtiqueta.TextoLibre);
        }

        Nombre = n;
        Marca = m;
        Campos = string.Join(',', lista);
        TextoLibre = t;
        Formato = formato;
        Activa = activa;
        return Resultado.Ok();
    }
}

/// <summary>Cómo conoce el cliente un artículo nuestro: su código, su descripción y, si tiene, su GTIN.</summary>
public sealed class ReferenciaCliente : RaizAgregadoEmpresa<Guid>
{
    private ReferenciaCliente(Guid id)
        : base(id, Guid.Empty)
    {
        Codigo = null!;
    }

    private ReferenciaCliente(Guid id, Guid empresaId, Guid clienteId, Guid productoId)
        : base(id, empresaId)
    {
        ClienteId = clienteId;
        ProductoId = productoId;
        Codigo = string.Empty;
    }

    public Guid ClienteId { get; private set; }

    public Guid ProductoId { get; private set; }

    public string Codigo { get; private set; }

    public string? Descripcion { get; private set; }

    /// <summary>GTIN-14 de la unidad de expedición en el cliente (si el cliente lo pide distinto del nuestro).</summary>
    public string? Gtin { get; private set; }

    public static Resultado<ReferenciaCliente> Crear(Guid empresaId, Guid clienteId, Guid productoId, string? codigo, string? descripcion, string? gtin)
    {
        var r = new ReferenciaCliente(Guid.NewGuid(), empresaId, clienteId, productoId);
        var c = r.Cambiar(codigo, descripcion, gtin);
        return c.EsFallo ? Resultado.Fallo<ReferenciaCliente>(c.Error) : Resultado.Ok(r);
    }

    public Resultado Cambiar(string? codigo, string? descripcion, string? gtin)
    {
        var c = codigo?.Trim();
        if (string.IsNullOrEmpty(c) || c.Length > 40)
        {
            return Resultado.Fallo(Error.Validacion("referencia.codigo", "Indica el código del artículo en el cliente (hasta 40 caracteres)."));
        }

        var g = string.IsNullOrWhiteSpace(gtin) ? null : gtin.Trim();
        if (g is not null)
        {
            if (g.Length is 8 or 12 or 13)
            {
                g = g.PadLeft(14, '0');
            }

            if (g.Length != 14 || !g.All(char.IsAsciiDigit) || !DigitoControlCorrecto(g))
            {
                return Resultado.Fallo(Error.Validacion("referencia.gtin", "El GTIN no es válido (8, 12, 13 o 14 dígitos con su dígito de control)."));
            }
        }

        var d = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();
        Codigo = c;
        Descripcion = d?.Length > 120 ? d[..120] : d;
        Gtin = g;
        return Resultado.Ok();
    }

    private static bool DigitoControlCorrecto(string gtin14)
    {
        var suma = 0;
        for (var i = 0; i < 13; i++)
        {
            suma += (gtin14[i] - '0') * (i % 2 == 0 ? 3 : 1);
        }

        return (10 - (suma % 10)) % 10 == gtin14[13] - '0';
    }
}
