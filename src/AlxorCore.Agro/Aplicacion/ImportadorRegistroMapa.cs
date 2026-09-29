using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using AlxorCore.Agro.Dominio;

namespace AlxorCore.Agro.Aplicacion;

/// <summary>Resultado de leer el fichero del ministerio: la carga, más los avisos de lo que no se pudo interpretar.</summary>
public sealed record LecturaRegistroMapa(CargaRegistroFito Carga, IReadOnlyList<string> Avisos);

/// <summary>
/// Lector del fichero JSON del Registro Oficial de Productos Fitosanitarios del MAPA:
/// <c>{"Productos":[{"DATOSPRODUCTO":{...},"COMPOSICION":[...],"USOS":[...],"OTRASDENOMINACIONES":[...]}]}</c>.
/// Traduce cada producto a <see cref="DatosFitosanitario"/>:
/// <list type="bullet">
/// <item>el estado («Vigente» es autorizado; «Cancelado», «Caducado», «Suspendido»…);</item>
/// <item>las fechas, en «aaaa/mm/dd» o ISO;</item>
/// <item>la composición, con su concentración;</item>
/// <item>los usos: cultivo, agente, dosis, unidad, plazo de seguridad («NO PROCEDE» es sin plazo), el máximo de
/// aplicaciones («1-8» son 8), y el intervalo y el condicionamiento específico como observaciones.</item>
/// </list>
/// </summary>
public static partial class ImportadorRegistroMapa
{
    private static readonly JsonDocumentOptions Opciones = new() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };

    public static async Task<LecturaRegistroMapa> LeerAsync(Stream fichero, bool completa, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(fichero);
        using var doc = await JsonDocument.ParseAsync(fichero, Opciones, ct).ConfigureAwait(false);
        var avisos = new List<string>();
        if (!Propiedad(doc.RootElement, "Productos", out var productos) || productos.ValueKind != JsonValueKind.Array)
        {
            throw new FormatException("El fichero no tiene la lista «Productos» del registro del ministerio.");
        }

        var lista = new List<DatosFitosanitario>();
        foreach (var p in productos.EnumerateArray())
        {
            if (!Propiedad(p, "DATOSPRODUCTO", out var datos))
            {
                avisos.Add("Un producto sin «DATOSPRODUCTO»: se ignora.");
                continue;
            }

            var numero = Texto(datos, "Num_Registro");
            var nombre = Texto(datos, "Nombre");
            if (numero is null || nombre is null)
            {
                avisos.Add($"Producto sin número de registro o nombre ({numero ?? nombre ?? "?"}): se ignora.");
                continue;
            }

            var estadoTexto = Texto(datos, "Estado");
            var estado = Estado(estadoTexto);
            if (estado is null)
            {
                avisos.Add($"{numero}: estado «{estadoTexto}» desconocido, se toma como suspendido.");
            }

            var materias = new List<DatosMateriaActiva>();
            if (Propiedad(p, "COMPOSICION", out var composicion) && composicion.ValueKind == JsonValueKind.Array)
            {
                foreach (var c in composicion.EnumerateArray())
                {
                    var sustancia = Texto(c, "Nombre Sustancia") ?? Texto(c, "NombreUE");
                    if (sustancia is null)
                    {
                        continue;
                    }

                    var concentracion = Numero(c, "Concentracion");
                    var nota = Texto(c, "DescripcionNota");
                    materias.Add(new DatosMateriaActiva(sustancia,
                        concentracion is { } v ? $"{v.ToString("0.###", CultureInfo.InvariantCulture).Replace('.', ',')} {nota}".Trim() : nota));
                }
            }

            var usos = new List<DatosUsoFito>();
            if (Propiedad(p, "USOS", out var listaUsos) && listaUsos.ValueKind == JsonValueKind.Array)
            {
                foreach (var u in listaUsos.EnumerateArray())
                {
                    var cultivo = Texto(u, "Cultivo");
                    var agente = Texto(u, "Agente");
                    if (cultivo is null || agente is null)
                    {
                        continue;
                    }

                    var intervalo = Texto(u, "IntervaloAplicaciones");
                    var condicionamiento = Texto(u, "CondicionamientoEspecifico");
                    var observaciones = string.Join(" ", new[] { intervalo is null ? null : $"Intervalo: {intervalo}.", condicionamiento }.Where(x => x is not null));
                    var dosisMin = Numero(u, "Dosis_Min");
                    var dosisMax = Numero(u, "Dosis_Max");
                    usos.Add(new DatosUsoFito(cultivo, agente, dosisMin is 0m && dosisMax is 0m ? null : dosisMin, dosisMax is 0m ? null : dosisMax,
                        Texto(u, "Unidad Medida dosis"), Plazo(Texto(u, "Plazo Seguridad")), Aplicaciones(Texto(u, "Aplicaciones")),
                        observaciones.Length == 0 ? null : Limpiar(observaciones)));
                }
            }

            lista.Add(new DatosFitosanitario(numero, nombre, Texto(datos, "Titular"), estado ?? EstadoFitosanitario.Suspendido, Fecha(Texto(datos, "Fecha_Caducidad")),
                Fecha(Texto(datos, "Fecha_LimiteVenta")), null, materias, usos, FechaCancelacion: Fecha(Texto(datos, "Fecha_Cancelacion")),
                Formulado: Texto(datos, "Formulado")));
        }

        return new LecturaRegistroMapa(new CargaRegistroFito(lista, completa, "MAPA"), avisos);
    }

    /// <summary>Estado del registro: «Vigente» es autorizado; null si no se reconoce.</summary>
    public static EstadoFitosanitario? Estado(string? texto)
    {
        var t = (texto ?? string.Empty).Trim().ToUpperInvariant();
        return t switch
        {
            "" or "VIGENTE" or "AUTORIZADO" => EstadoFitosanitario.Autorizado,
            _ when t.StartsWith("CANCEL", StringComparison.Ordinal) || t.StartsWith("REVOC", StringComparison.Ordinal) || t.StartsWith("RETIRAD", StringComparison.Ordinal)
                => EstadoFitosanitario.Cancelado,
            _ when t.StartsWith("CADUC", StringComparison.Ordinal) || t.StartsWith("NO RENOV", StringComparison.Ordinal) => EstadoFitosanitario.Caducado,
            _ when t.StartsWith("SUSPEN", StringComparison.Ordinal) => EstadoFitosanitario.Suspendido,
            _ => null,
        };
    }

    /// <summary>Fecha en «aaaa/mm/dd», «aaaa-mm-dd[Thh:mm:ss]» o «dd/mm/aaaa»; null si está vacía o no se entiende.</summary>
    public static DateOnly? Fecha(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return null;
        }

        var t = texto.Trim();
        if (t.Length >= 10 && DateOnly.TryParseExact(t[..10].Replace('-', '/'), "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var iso))
        {
            return iso;
        }

        return DateOnly.TryParseExact(t, ["dd/MM/yyyy", "d/M/yyyy"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var es) ? es : null;
    }

    /// <summary>Plazo de seguridad en días: «NO PROCEDE» (o vacío) es sin plazo; «21», «21 días»… es el número.</summary>
    public static int? Plazo(string? texto)
    {
        var m = Entero().Match(texto ?? string.Empty);
        return m.Success && int.TryParse(m.Value, CultureInfo.InvariantCulture, out var d) ? Math.Clamp(d, 0, 365) : null;
    }

    /// <summary>Máximo de aplicaciones: «1-8» son 8, «3» son 3.</summary>
    public static int? Aplicaciones(string? texto)
    {
        var numeros = Entero().Matches(texto ?? string.Empty);
        return numeros.Count > 0 && int.TryParse(numeros[^1].Value, CultureInfo.InvariantCulture, out var n) ? n : null;
    }

    private static string Limpiar(string t) => Espacios().Replace(t.Replace('\r', ' ').Replace('\n', ' '), " ").Trim();

    private static bool Propiedad(JsonElement e, string nombre, out JsonElement valor)
    {
        if (e.ValueKind == JsonValueKind.Object)
        {
            foreach (var p in e.EnumerateObject())
            {
                if (string.Equals(p.Name, nombre, StringComparison.OrdinalIgnoreCase))
                {
                    valor = p.Value;
                    return true;
                }
            }
        }

        valor = default;
        return false;
    }

    private static string? Texto(JsonElement e, string nombre)
    {
        if (!Propiedad(e, nombre, out var v))
        {
            return null;
        }

        var t = v.ValueKind switch
        {
            JsonValueKind.String => v.GetString(),
            JsonValueKind.Number => v.GetRawText(),
            _ => null,
        };
        return string.IsNullOrWhiteSpace(t) ? null : Espacios().Replace(t.Trim(), " ");
    }

    private static decimal? Numero(JsonElement e, string nombre)
    {
        if (!Propiedad(e, nombre, out var v))
        {
            return null;
        }

        if (v.ValueKind == JsonValueKind.Number)
        {
            return v.TryGetDecimal(out var d) ? d : v.TryGetDouble(out var f) ? (decimal)f : null;
        }

        return v.ValueKind == JsonValueKind.String && decimal.TryParse(v.GetString()?.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var s) ? s : null;
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex Entero();

    [GeneratedRegex(@"[ \t]{2,}")]
    private static partial Regex Espacios();
}
