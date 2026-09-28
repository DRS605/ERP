using System.Globalization;
using System.Text;
using System.Text.Json;
using AlxorCore.Api.Comun;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Api.Endpoints;

/// <summary>Columna de una exportación: título y tipo (<c>texto</c>, <c>numero</c>, <c>moneda</c>, <c>fecha</c>, <c>porcentaje</c>).</summary>
/// <param name="Titulo">Cabecera de la columna.</param>
/// <param name="Tipo">Tipo de dato; por defecto texto.</param>
/// <param name="Total">Cálculo de la fila de totales: <c>suma</c>, <c>media</c>, <c>no</c> o vacío (automático: suma en números e importes).</param>
public sealed record ColumnaExportacion(string Titulo, string? Tipo = null, string? Total = null);

/// <summary>Petición de exportación a Excel de una rejilla ya filtrada en la interfaz.</summary>
/// <param name="Titulo">Título (nombre de la hoja y del fichero).</param>
/// <param name="Columnas">Columnas en orden.</param>
/// <param name="Filas">Filas: una lista de valores por fila, en el orden de las columnas.</param>
/// <param name="Totales">Si se envía (aunque sea vacío), se añade la fila de totales; sus valores no nulos rotulan las columnas sin fórmula.</param>
public sealed record PeticionExportarXlsx(string? Titulo, IReadOnlyList<ColumnaExportacion>? Columnas, IReadOnlyList<IReadOnlyList<JsonElement>>? Filas, IReadOnlyList<JsonElement>? Totales = null);

/// <summary>
/// Exportación genérica a Excel (.xlsx) de cualquier rejilla de la aplicación: la interfaz envía lo que el
/// usuario está viendo (columnas visibles, filas filtradas) y recibe un .xlsx con formatos y totales.
/// Es de la base (todas las ediciones) y solo exige estar autenticado: no lee datos de la empresa.
/// </summary>
public static class EndpointsExportacion
{
    public static IEndpointRouteBuilder MapearExportacion(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var grupo = rutas.MapGroup("/exportar").WithTags("Exportación");

        grupo.MapPost("/xlsx", ExportarXlsx)
            .WithSummary("Genera un Excel (.xlsx) con las columnas, filas y totales enviados (hasta 100 000 filas).")
            .RequireAuthorization();

        return rutas;
    }

    private static IResult ExportarXlsx(PeticionExportarXlsx peticion)
    {
        ArgumentNullException.ThrowIfNull(peticion);
        var titulo = (peticion.Titulo ?? string.Empty).Trim();
        if (titulo.Length > 200)
        {
            return Problema("exportar.titulo", "El título admite como máximo 200 caracteres.");
        }

        if (peticion.Columnas is not { Count: > 0 } cols)
        {
            return Problema("exportar.columnas", "Indica al menos una columna.");
        }

        if (cols.Count > ExcelXlsx.MaximoColumnas)
        {
            return Problema("exportar.columnas", $"Como máximo {ExcelXlsx.MaximoColumnas} columnas.");
        }

        var filas = peticion.Filas ?? [];
        if (filas.Count > ExcelXlsx.MaximoFilas)
        {
            return Problema("exportar.filas", "Como máximo 100.000 filas por exportación; filtra antes de exportar.");
        }

        var columnas = new List<ColumnaExcel>(cols.Count);
        foreach (var c in cols)
        {
            if (c is null || (c.Titulo ?? string.Empty).Length > 200)
            {
                return Problema("exportar.columnas", "Cada columna necesita un título de hasta 200 caracteres.");
            }

            TipoColumnaExcel? tipo = (c.Tipo ?? "texto").Trim().ToUpperInvariant() switch
            {
                "" or "TEXTO" => TipoColumnaExcel.Texto,
                "NUMERO" => TipoColumnaExcel.Numero,
                "MONEDA" or "IMPORTE" => TipoColumnaExcel.Moneda,
                "FECHA" => TipoColumnaExcel.Fecha,
                "PORCENTAJE" => TipoColumnaExcel.Porcentaje,
                _ => null,
            };
            TotalColumnaExcel? total = (c.Total ?? string.Empty).Trim().ToUpperInvariant() switch
            {
                "" or "AUTO" => TotalColumnaExcel.Auto,
                "SUMA" => TotalColumnaExcel.Suma,
                "MEDIA" => TotalColumnaExcel.Media,
                "NO" or "NINGUNO" => TotalColumnaExcel.Ninguno,
                _ => null,
            };
            if (tipo is null || total is null)
            {
                return Problema("exportar.columnas", $"Tipo o total no válido en la columna «{c.Titulo}» (tipos: texto, numero, moneda, fecha, porcentaje; totales: suma, media, no).");
            }

            columnas.Add(new ColumnaExcel(c.Titulo ?? string.Empty, tipo.Value, total.Value));
        }

        var valores = filas.Select(f => (IReadOnlyList<object?>)(f ?? []).Select(Valor).ToList());
        var totales = peticion.Totales?.Select(Valor).ToList();
        var contenido = ExcelXlsx.Generar(titulo.Length == 0 ? "Exportación" : titulo, columnas, valores, totales);
        return Results.File(contenido, ExcelXlsx.TipoMime, NombreFichero(titulo));
    }

    private static object? Valor(JsonElement e) => e.ValueKind switch
    {
        JsonValueKind.String => e.GetString(),
        JsonValueKind.Number => e.TryGetDecimal(out var d) ? d : e.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Undefined or JsonValueKind.Null => null,
        _ => e.GetRawText(),
    };

    /// <summary>Nombre de fichero seguro: título sin acentos ni símbolos y la fecha del día.</summary>
    internal static string NombreFichero(string titulo)
    {
        var sb = new StringBuilder();
        foreach (var c in titulo.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            sb.Append(char.IsAsciiLetterOrDigit(c) ? char.ToLowerInvariant(c) : '-');
        }

        var baseNombre = string.Join('-', sb.ToString().Split('-', StringSplitOptions.RemoveEmptyEntries));
        if (baseNombre.Length > 60)
        {
            baseNombre = baseNombre[..60].TrimEnd('-');
        }

        return $"{(baseNombre.Length == 0 ? "exportacion" : baseNombre)}-{DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.xlsx";
    }

    private static IResult Problema(string codigo, string mensaje) => ResultadosHttp.AProblema(Error.Validacion(codigo, mensaje));
}
