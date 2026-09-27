using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Comprobaciones estáticas de la interfaz clásica (<c>wwwroot/index.html</c>, JavaScript sin compilar):
/// lo que un compilador detectaría y aquí no hay quien lo detecte.
/// </summary>
public sealed partial class InterfazClasicaTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public InterfazClasicaTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private static readonly Lazy<string> Script = new(() =>
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "ALXORCore.sln")))
        {
            dir = dir.Parent;
        }

        dir.Should().NotBeNull("las pruebas se ejecutan dentro del repositorio");
        return File.ReadAllText(Path.Combine(dir!.FullName, "src", "AlxorCore.Api", "wwwroot", "index.html"));
    });

    [GeneratedRegex(@"(?m)^\s*(?:async\s+)?function\s+([A-Za-z_$][\w$]*)\s*\(")]
    private static partial Regex Declaracion();

    [GeneratedRegex(@"\bon(?:click|change|input|submit|keydown|keyup)\s*=\s*(?:""|')\s*(?:return\s+)?(?:await\s+)?([A-Za-z_$][\w$]*)\s*\(")]
    private static partial Regex Manejador();

    [GeneratedRegex(@"\bapi\(\s*([""'`])((?:(?!\1).)*)\1(\s*\+)?")]
    private static partial Regex LlamadaApi();

    [GeneratedRegex(@"method\s*:\s*[""'](\w+)[""']")]
    private static partial Regex Metodo();

    [Fact]
    public void Toda_entrada_del_menu_tiene_icono_titulo_y_vista()
    {
        // Sin icono, el menú pinta «undefined»; sin título o sin vista, la entrada no abre nada.
        var js = Script.Value;
        var iconos = Regex.Matches(js, @"(?m)^\s{6}(\w+):'<svg").Select(m => m.Groups[1].Value)
            .Concat(Regex.Matches(js, @"ICON\.(\w+)\s*=").Select(m => m.Groups[1].Value)).ToHashSet(StringComparer.Ordinal);
        var nav = js[js.IndexOf("const NAV = [", StringComparison.Ordinal)..];
        nav = nav[..nav.IndexOf("];", StringComparison.Ordinal)];
        var entradas = Regex.Matches(nav, @"t:""(item|grupo)"",\s*k:""(\w+)""").Select(m => (Tipo: m.Groups[1].Value, Clave: m.Groups[2].Value)).ToList();
        entradas.Should().NotBeEmpty();
        entradas.Where(e => !iconos.Contains(e.Clave)).Select(e => e.Clave).Should().BeEmpty("cada entrada del menú necesita su icono en ICON");

        var titulos = js[js.IndexOf("const TITULOS", StringComparison.Ordinal)..];
        titulos = titulos[..titulos.IndexOf('\n', StringComparison.Ordinal)];
        var vistas = Regex.Matches(nav, @"\[""(\w+)"",""[^""]+""\]").Select(m => m.Groups[1].Value)
            .Concat(entradas.Where(e => e.Tipo == "item").Select(e => e.Clave)).Distinct().ToList();
        vistas.Where(v => !Regex.IsMatch(titulos, $@"\b{v}:")).Should().BeEmpty("cada vista del menú necesita su título en TITULOS");
        var mapa = Regex.Match(js, @"\(\{cartera:vCartera[^}]+\}\[k\]\)").Value;
        mapa.Should().NotBeEmpty();
        vistas.Where(v => !Regex.IsMatch(mapa, $@"\b{v}:")).Should().BeEmpty("cada vista del menú necesita su función en ir()");
    }

    [Fact]
    public void Ninguna_funcion_se_declara_dos_veces()
    {
        // En un script, una segunda declaración con el mismo nombre sustituye a la primera sin avisar.
        var repetidas = Declaracion().Matches(Script.Value).Select(m => m.Groups[1].Value)
            .GroupBy(n => n, StringComparer.Ordinal).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        repetidas.Should().BeEmpty("cada función de la interfaz debe tener un nombre propio");
    }

    [Fact]
    public void Todo_manejador_llama_a_una_funcion_que_existe()
    {
        var declaradas = Declaracion().Matches(Script.Value).Select(m => m.Groups[1].Value).ToHashSet(StringComparer.Ordinal);
        // Funciones del navegador o asignadas como expresión.
        string[] ajenas = ["if", "event", "window", "document", "this", "alert", "confirm", "setTimeout", "location", "history", "navigator"];
        var inexistentes = Manejador().Matches(Script.Value).Select(m => m.Groups[1].Value)
            .Where(n => !declaradas.Contains(n) && !ajenas.Contains(n) && !Regex.IsMatch(Script.Value, $@"\b(?:const|let|var|window\.)\s*{Regex.Escape(n)}\s*="))
            .Distinct().OrderBy(n => n, StringComparer.Ordinal).ToList();
        inexistentes.Should().BeEmpty("un botón que llama a una función inexistente no hace nada");
    }

    [Fact]
    public void Toda_llamada_a_la_api_apunta_a_una_ruta_existente_con_su_metodo()
    {
        var rutas = _fabrica.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>()
            .Where(e => !(e.RoutePattern.RawText ?? string.Empty).Contains("{*", StringComparison.Ordinal))
            .Select(e => (Segmentos: e.RoutePattern.PathSegments, Metodos: e.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? []))
            .ToList();

        var fallos = new List<string>();
        var script = Script.Value;
        foreach (Match m in LlamadaApi().Matches(script))
        {
            var ruta = m.Groups[2].Value;
            var concatenada = m.Groups[3].Success;
            var q = ruta.IndexOf('?', StringComparison.Ordinal);
            if (q >= 0)
            {
                ruta = ruta[..q];
                concatenada = false;
            }

            if (!ruta.StartsWith('/') || ruta.StartsWith("/${", StringComparison.Ordinal))
            {
                continue; // ruta construida entera en tiempo de ejecución
            }

            // Método: el primer «method:» dentro de los argumentos de esta llamada.
            var fin = script.IndexOf("api(", m.Index + m.Length, StringComparison.Ordinal);
            var args = script[(m.Index + m.Length)..Math.Min(fin < 0 ? script.Length : fin, m.Index + m.Length + 400)];
            var cierre = CierreLlamada(args);
            var mm = Metodo().Match(args[..cierre]);
            var metodo = mm.Success ? mm.Groups[1].Value.ToUpperInvariant() : "GET";

            var segmentos = ruta.Trim('/').Split('/', StringSplitOptions.None).ToList();
            var abierta = concatenada && ruta.EndsWith('/');
            if (abierta)
            {
                segmentos.RemoveAt(segmentos.Count - 1);
            }
            else if (concatenada)
            {
                segmentos[^1] += "${}"; // último segmento completado en tiempo de ejecución
            }

            var existe = rutas.Any(r => Casa(r.Segmentos, segmentos, abierta) && (r.Metodos.Count == 0 || r.Metodos.Contains(metodo)));
            if (!existe)
            {
                fallos.Add($"{metodo} {m.Groups[2].Value}{(concatenada ? "+…" : string.Empty)}");
            }
        }

        fallos.Distinct().Should().BeEmpty("la interfaz solo puede llamar a rutas que la API expone");
    }

    /// <summary>Posición del paréntesis que cierra la llamada (o el final del texto).</summary>
    private static int CierreLlamada(string args)
    {
        var nivel = 1;
        for (var i = 0; i < args.Length; i++)
        {
            nivel += args[i] switch { '(' => 1, ')' => -1, _ => 0 };
            if (nivel == 0)
            {
                return i;
            }
        }

        return args.Length;
    }

    private static bool Casa(IReadOnlyList<RoutePatternPathSegment> patron, List<string> ui, bool abierta)
    {
        if (abierta ? patron.Count <= ui.Count : patron.Count != ui.Count)
        {
            return false;
        }

        for (var i = 0; i < ui.Count; i++)
        {
            var seg = ui[i];
            var p = patron[i];
            if (seg.Contains("${", StringComparison.Ordinal))
            {
                continue; // segmento dinámico: casa con un parámetro o con un literal
            }

            if (p.IsSimple && p.Parts[0] is RoutePatternLiteralPart literal)
            {
                if (!string.Equals(literal.Content, seg, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            else if (!p.Parts.Any(x => x.IsParameter))
            {
                return false;
            }
        }

        return true;
    }
}
