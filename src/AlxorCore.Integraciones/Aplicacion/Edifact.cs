using System.Globalization;
using System.Text;

namespace AlxorCore.Integraciones.Aplicacion;

public sealed record LineaEdi(string? Gtin, string Descripcion, decimal Cantidad, decimal? Precio = null, decimal? Importe = null, decimal? PorcentajeIva = null,
    string? CodigoInterno = null);

/// <summary>Datos de una factura para el INVOIC (EANCOM D96A, EAN008).</summary>
public sealed record DatosInvoic(string Numero, DateOnly Fecha, string GlnEmisor, string GlnComprador, string? GlnFacturacion, string? PedidoCliente,
    string? Albaran, IReadOnlyList<LineaEdi> Lineas, IReadOnlyList<(decimal Porcentaje, decimal Base, decimal Cuota)> Impuestos, decimal Base, decimal Total,
    bool Rectificativa = false);

/// <summary>Palé del DESADV: su SSCC y lo que lleva.</summary>
public sealed record PaleEdi(string Sscc, IReadOnlyList<LineaEdi> Lineas);

/// <summary>Datos de un albarán para el DESADV (EANCOM D96A, EAN007): palés con SSCC o, sin palés, las líneas.</summary>
public sealed record DatosDesadv(string Numero, DateOnly Fecha, string GlnEmisor, string GlnComprador, string? GlnEntrega, string? PedidoCliente,
    IReadOnlyList<PaleEdi> Pales, IReadOnlyList<LineaEdi> Lineas);

/// <summary>Pedido leído de un ORDERS.</summary>
public sealed record PedidoLeidoEdi(string Numero, DateOnly? Fecha, DateOnly? FechaEntrega, string? GlnComprador, string? GlnEntrega, IReadOnlyList<LineaEdi> Lineas);

/// <summary>Recepción leída de un RECADV: el albarán (DESADV) al que responde y lo recibido por artículo.</summary>
public sealed record RecepcionLeidaEdi(string Numero, string? Albaran, string? PedidoCliente, IReadOnlyList<(string? Gtin, decimal Recibido, decimal? Aceptado)> Lineas);

/// <summary>
/// Mensajes EDIFACT EANCOM D96A (el estándar de la distribución en España: Carrefour, Mercadona, El Corte Inglés…):
/// escribe INVOIC (factura) y DESADV (aviso de expedición con los SSCC) y lee ORDERS (pedido) y RECADV (aviso de
/// recepción). Sintaxis UNOC nivel 3 con los separadores por defecto (<c>UNA:+.? '</c>).
/// </summary>
public static class Edifact
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static string Invoic(DatosInvoic d, string referenciaIntercambio, DateTimeOffset ahora)
    {
        ArgumentNullException.ThrowIfNull(d);
        var s = new List<string>
        {
            $"UNH+1+INVOIC:D:96A:UN:EAN008",
            $"BGM+{(d.Rectificativa ? "381" : "380")}+{Esc(d.Numero)}+9",
            $"DTM+137:{d.Fecha:yyyyMMdd}:102",
        };
        if (!string.IsNullOrWhiteSpace(d.PedidoCliente)) s.Add($"RFF+ON:{Esc(d.PedidoCliente)}");
        if (!string.IsNullOrWhiteSpace(d.Albaran)) s.Add($"RFF+DQ:{Esc(d.Albaran)}");
        s.Add($"NAD+SU+{d.GlnEmisor}::9");
        s.Add($"NAD+BY+{d.GlnComprador}::9");
        s.Add($"NAD+IV+{d.GlnFacturacion ?? d.GlnComprador}::9");
        s.Add("CUX+2:EUR:4");
        var n = 0;
        foreach (var l in d.Lineas)
        {
            n++;
            s.Add(Lin(n, l));
            s.Add($"IMD+F++:::{Esc(Corta(l.Descripcion, 35))}");
            s.Add($"QTY+47:{Num(l.Cantidad)}");
            s.Add($"MOA+203:{Num(l.Importe ?? 0m)}");
            s.Add($"PRI+AAA:{Num(l.Precio ?? 0m)}");
            if (l.PorcentajeIva is { } iva) s.Add($"TAX+7+VAT+++:::{Num(iva)}");
        }

        s.Add("UNS+S");
        s.Add($"CNT+2:{n}");
        s.Add($"MOA+86:{Num(d.Total)}");
        s.Add($"MOA+79:{Num(d.Lineas.Sum(l => l.Importe ?? 0m))}");
        s.Add($"MOA+125:{Num(d.Base)}");
        foreach (var (pct, b, cuota) in d.Impuestos)
        {
            s.Add($"TAX+7+VAT+++:::{Num(pct)}");
            s.Add($"MOA+125:{Num(b)}");
            s.Add($"MOA+124:{Num(cuota)}");
        }

        return Envolver(s, d.GlnEmisor, d.GlnFacturacion ?? d.GlnComprador, referenciaIntercambio, ahora);
    }

    public static string Desadv(DatosDesadv d, string referenciaIntercambio, DateTimeOffset ahora)
    {
        ArgumentNullException.ThrowIfNull(d);
        var s = new List<string>
        {
            "UNH+1+DESADV:D:96A:UN:EAN007",
            $"BGM+351+{Esc(d.Numero)}+9",
            $"DTM+137:{d.Fecha:yyyyMMdd}:102",
            $"DTM+11:{d.Fecha:yyyyMMdd}:102",
        };
        if (!string.IsNullOrWhiteSpace(d.PedidoCliente)) s.Add($"RFF+ON:{Esc(d.PedidoCliente)}");
        s.Add($"NAD+SU+{d.GlnEmisor}::9");
        s.Add($"NAD+BY+{d.GlnComprador}::9");
        s.Add($"NAD+DP+{d.GlnEntrega ?? d.GlnComprador}::9");
        s.Add("CPS+1");
        var n = 0;
        var cps = 1;
        if (d.Pales.Count > 0)
        {
            s.Add($"PAC+{d.Pales.Count}++201");
            foreach (var p in d.Pales)
            {
                cps++;
                s.Add($"CPS+{cps}+1");
                s.Add("PAC+1++201");
                s.Add("PCI+33E");
                s.Add($"GIN+BJ+{p.Sscc}");
                foreach (var l in p.Lineas)
                {
                    n++;
                    s.Add(Lin(n, l));
                    s.Add($"QTY+12:{Num(l.Cantidad)}");
                }
            }
        }
        else
        {
            foreach (var l in d.Lineas)
            {
                n++;
                s.Add(Lin(n, l));
                s.Add($"IMD+F++:::{Esc(Corta(l.Descripcion, 35))}");
                s.Add($"QTY+12:{Num(l.Cantidad)}");
            }
        }

        s.Add($"CNT+2:{n}");
        return Envolver(s, d.GlnEmisor, d.GlnComprador, referenciaIntercambio, ahora);
    }

    public static PedidoLeidoEdi LeerOrders(string texto)
    {
        var segmentos = Segmentos(texto);
        if (!segmentos.Any(x => x[0] == "UNH" && x.Length > 2 && Comp(x[2], 0) == "ORDERS"))
        {
            throw new FormatException("El mensaje no es un ORDERS (pedido) EDIFACT.");
        }

        string? numero = null, gln = null, entrega = null;
        DateOnly? fecha = null, fechaEntrega = null;
        var lineas = new List<LineaEdi>();
        LineaEdi? actual = null;
        foreach (var x in segmentos)
        {
            switch (x[0])
            {
                case "BGM": numero = Comp(Elem(x, 2), 0); break;
                case "DTM" when Comp(Elem(x, 1), 0) == "137": fecha = Fecha(Elem(x, 1)); break;
                case "DTM" when Comp(Elem(x, 1), 0) is "2" or "64" or "17": fechaEntrega ??= Fecha(Elem(x, 1)); break;
                case "NAD" when Elem(x, 1) == "BY": gln = Comp(Elem(x, 2), 0); break;
                case "NAD" when Elem(x, 1) == "DP": entrega = Comp(Elem(x, 2), 0); break;
                case "LIN":
                    if (actual is not null) lineas.Add(actual);
                    actual = new LineaEdi(Comp(Elem(x, 3), 0), string.Empty, 0m);
                    break;
                case "PIA" when actual is not null && Comp(Elem(x, 2), 1) is "SA" or "IN": actual = actual with { CodigoInterno = Comp(Elem(x, 2), 0) }; break;
                case "IMD" when actual is not null: actual = actual with { Descripcion = Comp(Elem(x, 3), 3) ?? actual.Descripcion }; break;
                case "QTY" when actual is not null && Comp(Elem(x, 1), 0) == "21": actual = actual with { Cantidad = Dec(Comp(Elem(x, 1), 1)) }; break;
                case "PRI" when actual is not null && Comp(Elem(x, 1), 0) is "AAA" or "AAB": actual = actual with { Precio = actual.Precio ?? Dec(Comp(Elem(x, 1), 1)) }; break;
            }
        }

        if (actual is not null) lineas.Add(actual);
        return new PedidoLeidoEdi(numero ?? throw new FormatException("El ORDERS no trae número de pedido (BGM)."), fecha, fechaEntrega, gln, entrega, lineas);
    }

    public static RecepcionLeidaEdi LeerRecadv(string texto)
    {
        var segmentos = Segmentos(texto);
        if (!segmentos.Any(x => x[0] == "UNH" && x.Length > 2 && Comp(x[2], 0) == "RECADV"))
        {
            throw new FormatException("El mensaje no es un RECADV (aviso de recepción) EDIFACT.");
        }

        string? numero = null, albaran = null, pedido = null;
        var lineas = new List<(string? Gtin, decimal Recibido, decimal? Aceptado)>();
        string? gtin = null;
        decimal? recibido = null, aceptado = null;
        void Cerrar()
        {
            if (gtin is not null || recibido is not null) lineas.Add((gtin, recibido ?? 0m, aceptado));
            gtin = null;
            recibido = null;
            aceptado = null;
        }

        foreach (var x in segmentos)
        {
            switch (x[0])
            {
                case "BGM": numero = Comp(Elem(x, 2), 0); break;
                case "RFF" when Comp(Elem(x, 1), 0) == "AAK": albaran = Comp(Elem(x, 1), 1); break;
                case "RFF" when Comp(Elem(x, 1), 0) == "ON": pedido = Comp(Elem(x, 1), 1); break;
                case "LIN": Cerrar(); gtin = Comp(Elem(x, 3), 0); break;
                case "QTY" when Comp(Elem(x, 1), 0) == "194": recibido = Dec(Comp(Elem(x, 1), 1)); break;
                case "QTY" when Comp(Elem(x, 1), 0) == "12": recibido ??= Dec(Comp(Elem(x, 1), 1)); break;
                case "QTY" when Comp(Elem(x, 1), 0) == "46": aceptado = Dec(Comp(Elem(x, 1), 1)); break;
            }
        }

        Cerrar();
        return new RecepcionLeidaEdi(numero ?? "", albaran, pedido, lineas);
    }

    // ------------------------------------------------------------------ sintaxis

    private static string Lin(int n, LineaEdi l) =>
        string.IsNullOrWhiteSpace(l.Gtin) ? $"LIN+{n}" : $"LIN+{n}++{l.Gtin}:EN";

    private static string Envolver(List<string> cuerpo, string emisor, string receptor, string referencia, DateTimeOffset ahora)
    {
        var sb = new StringBuilder("UNA:+.? '");
        sb.Append(CultureInfo.InvariantCulture, $"UNB+UNOC:3+{emisor}:14+{receptor}:14+{ahora.UtcDateTime:yyMMdd}:{ahora.UtcDateTime:HHmm}+{Esc(referencia)}'");
        foreach (var s in cuerpo)
        {
            sb.Append(s).Append('\'');
        }

        sb.Append(CultureInfo.InvariantCulture, $"UNT+{cuerpo.Count + 1}+1'");
        sb.Append(CultureInfo.InvariantCulture, $"UNZ+1+{Esc(referencia)}'");
        return sb.ToString();
    }

    /// <summary>Protege los caracteres reservados (<c>+ : ' ?</c>) con el de escape.</summary>
    public static string Esc(string? t) => (t ?? string.Empty).Replace("?", "??", StringComparison.Ordinal).Replace("+", "?+", StringComparison.Ordinal)
        .Replace(":", "?:", StringComparison.Ordinal).Replace("'", "?'", StringComparison.Ordinal);

    private static string Num(decimal v) => v.ToString("0.####", Inv);

    private static string Corta(string t, int max) => t.Length > max ? t[..max] : t;

    /// <summary>Segmentos del mensaje: cada uno, sus elementos (separados por + y sin escapes resueltos en los componentes).</summary>
    private static List<string[]> Segmentos(string texto)
    {
        ArgumentNullException.ThrowIfNull(texto);
        var t = texto.Replace("\r", "", StringComparison.Ordinal).Replace("\n", "", StringComparison.Ordinal);
        if (t.StartsWith("UNA", StringComparison.Ordinal))
        {
            t = t[9..];
        }

        return Partir(t, '\'').Where(x => x.Length > 0).Select(x => Partir(x, '+').ToArray()).ToList();
    }

    /// <summary>Parte por el separador respetando el carácter de escape (?), que se conserva para los componentes.</summary>
    private static List<string> Partir(string t, char separador)
    {
        var partes = new List<string>();
        var actual = new StringBuilder();
        for (var i = 0; i < t.Length; i++)
        {
            if (t[i] == '?' && i + 1 < t.Length)
            {
                actual.Append(t[i]).Append(t[i + 1]);
                i++;
            }
            else if (t[i] == separador)
            {
                partes.Add(actual.ToString());
                actual.Clear();
            }
            else
            {
                actual.Append(t[i]);
            }
        }

        partes.Add(actual.ToString());
        return partes;
    }

    private static string? Elem(string[] seg, int i) => i < seg.Length ? seg[i] : null;

    /// <summary>Componente <paramref name="i"/> (separados por :) del elemento, ya sin escapes.</summary>
    private static string? Comp(string? elemento, int i)
    {
        if (elemento is null)
        {
            return null;
        }

        var c = Partir(elemento, ':');
        return i < c.Count && c[i].Length > 0 ? Des(c[i]) : null;
    }

    private static string Des(string t)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < t.Length; i++)
        {
            if (t[i] == '?' && i + 1 < t.Length)
            {
                i++;
            }

            sb.Append(t[i]);
        }

        return sb.ToString();
    }

    private static decimal Dec(string? v) => decimal.TryParse(v, NumberStyles.Number, Inv, out var d) ? d : 0m;

    private static DateOnly? Fecha(string? elemento)
    {
        var v = Comp(elemento, 1);
        return v is { Length: >= 8 } && DateOnly.TryParseExact(v[..8], "yyyyMMdd", Inv, DateTimeStyles.None, out var f) ? f : null;
    }
}

/// <summary>Persistencia de la configuración EDI, los socios y los pedidos recibidos.</summary>
public interface IRepositorioEdi
{
    Task<AlxorCore.Integraciones.Dominio.ConfiguracionEdi?> ConfiguracionAsync(CancellationToken ct = default);

    void Agregar(AlxorCore.Integraciones.Dominio.ConfiguracionEdi configuracion);

    Task<IReadOnlyList<AlxorCore.Integraciones.Dominio.SocioEdi>> SociosAsync(CancellationToken ct = default);

    Task<AlxorCore.Integraciones.Dominio.SocioEdi?> SocioAsync(Guid id, CancellationToken ct = default);

    Task<AlxorCore.Integraciones.Dominio.SocioEdi?> SocioPorClienteAsync(Guid clienteId, CancellationToken ct = default);

    Task<AlxorCore.Integraciones.Dominio.SocioEdi?> SocioPorGlnAsync(string gln, CancellationToken ct = default);

    void Agregar(AlxorCore.Integraciones.Dominio.SocioEdi socio);

    void Quitar(AlxorCore.Integraciones.Dominio.SocioEdi socio);

    Task<AlxorCore.Integraciones.Dominio.PedidoEdi?> PedidoAsync(string numeroCliente, string glnComprador, CancellationToken ct = default);

    Task<AlxorCore.Integraciones.Dominio.PedidoEdi?> PedidoPorVentaAsync(Guid pedidoVentaId, CancellationToken ct = default);

    Task<IReadOnlyList<AlxorCore.Integraciones.Dominio.PedidoEdi>> PedidosAsync(CancellationToken ct = default);

    void Agregar(AlxorCore.Integraciones.Dominio.PedidoEdi pedido);
}
