using AlxorCore.Api.Comun;
using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Catalogo.Dominio;
using AlxorCore.Contabilidad.Aplicacion;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Tesoreria.Aplicacion;
using AlxorCore.Tesoreria.Dominio;

namespace AlxorCore.Api.Endpoints;

/// <summary>Petición de importación por Excel (.xlsx en base64). Previsualizar no aplica cambios.</summary>
public sealed record ImportarExcelPeticion(string ContenidoBase64, bool Previsualizar = true);

/// <summary>Resultado de una importación por Excel.</summary>
public sealed record ResultadoImportacionExcel(int Total, int Correctas, int Errores, bool Aplicado, IReadOnlyList<string> Mensajes);

/// <summary>
/// Importaciones de arranque por Excel (.xlsx): saldos contables (asiento de apertura), cartera
/// (previsiones de cobro/pago) y stock (existencias iniciales). Cada endpoint admite
/// <c>previsualizar</c> para validar sin aplicar.
/// </summary>
public static class EndpointsImportacion
{
    public static IEndpointRouteBuilder MapearImportacion(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var grupo = rutas.MapGroup("/importar").WithTags("Importación");

        grupo.MapPost("/saldos", SaldosAsync)
            .WithSummary("Importa saldos contables desde Excel (columnas: cuenta, debe, haber) como asiento de apertura.")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        grupo.MapPost("/cartera", CarteraAsync)
            .WithSummary("Importa la cartera pendiente desde Excel (columnas: sentido, concepto, importe, fecha) como previsiones.")
            .RequierePermiso(Permisos.CobroRegistrar);

        grupo.MapPost("/stock", StockAsync)
            .WithSummary("Importa existencias iniciales desde Excel (columnas: referencia, cantidad) ajustando el stock.")
            .RequierePermiso(Permisos.ProductoGestionar);

        return rutas;
    }

    private static bool TryLeer(ImportarExcelPeticion peticion, out IReadOnlyList<LectorCsv.FilaCsv> filas, out IResult? error)
    {
        filas = [];
        error = null;
        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(peticion.ContenidoBase64 ?? string.Empty);
        }
        catch (FormatException)
        {
            error = ResultadosHttp.AProblema(Error.Validacion("importar.base64", "El archivo no es un base64 válido."));
            return false;
        }

        try
        {
            filas = LectorExcel.Parsear(bytes);
        }
        catch (Exception ex) when (ex is InvalidOperationException or InvalidDataException)
        {
            error = ResultadosHttp.AProblema(Error.Validacion("importar.xlsx_invalido", "No se pudo leer el Excel: " + ex.Message));
            return false;
        }

        return true;
    }

    private static async Task<IResult> SaldosAsync(ImportarExcelPeticion peticion, IContextoEmpresa contexto, CrearAsiento caso, IReloj reloj, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(peticion);
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        if (!TryLeer(peticion, out var filas, out var err))
        {
            return err!;
        }

        var lineas = new List<LineaAsientoComando>();
        var mensajes = new List<string>();
        foreach (var f in filas)
        {
            var cuenta = f.Campo("cuenta", "codigo", "cuenta_codigo");
            if (string.IsNullOrWhiteSpace(cuenta))
            {
                mensajes.Add($"Fila {f.Numero}: sin cuenta.");
                continue;
            }

            var debe = ImportacionCsv.Numero(f.Campo("debe"));
            var haber = ImportacionCsv.Numero(f.Campo("haber"));
            lineas.Add(new LineaAsientoComando(cuenta, debe, haber, "Apertura"));
        }

        var sumaDebe = lineas.Sum(l => l.Debe);
        var sumaHaber = lineas.Sum(l => l.Haber);
        mensajes.Insert(0, $"{lineas.Count} apunte(s). Debe {sumaDebe:F2} € / Haber {sumaHaber:F2} €.");

        if (peticion.Previsualizar)
        {
            if (sumaDebe != sumaHaber)
            {
                mensajes.Add("El asiento no cuadra: la suma del debe debe igualar la del haber.");
            }

            return Results.Ok(new ResultadoImportacionExcel(lineas.Count, lineas.Count, sumaDebe == sumaHaber ? 0 : 1, false, mensajes));
        }

        var anio = reloj.AhoraUtc.Year;
        var comando = new CrearAsientoComando(new DateOnly(anio, 1, 1), "Apertura (importación)", lineas);
        var r = await caso.EjecutarAsync(contexto.EmpresaId.Value, comando, ct).ConfigureAwait(false);
        return r.EsCorrecto
            ? Results.Ok(new ResultadoImportacionExcel(lineas.Count, lineas.Count, 0, true, mensajes))
            : ResultadosHttp.AProblema(r.Error);
    }

    private static async Task<IResult> CarteraAsync(ImportarExcelPeticion peticion, IContextoEmpresa contexto, CrearPrevision caso, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(peticion);
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        if (!TryLeer(peticion, out var filas, out var err))
        {
            return err!;
        }

        var items = new List<(SentidoPrevision Sentido, string Concepto, decimal Importe, DateOnly Fecha)>();
        var mensajes = new List<string>();
        foreach (var f in filas)
        {
            var sentidoTxt = (f.Campo("sentido", "tipo") ?? string.Empty).ToLowerInvariant();
            var sentido = sentidoTxt is "cobro" or "ingreso" or "cliente" ? SentidoPrevision.Ingreso
                : sentidoTxt is "pago" or "gasto" or "proveedor" ? SentidoPrevision.Gasto
                : (SentidoPrevision?)null;
            var concepto = f.Campo("concepto", "descripcion", "tercero", "documento");
            var importe = ImportacionCsv.Numero(f.Campo("importe", "total", "pendiente"));
            var fecha = LectorExcel.FechaDeSerie(f.Campo("fecha", "vencimiento"));
            if (sentido is null || string.IsNullOrWhiteSpace(concepto) || importe <= 0m || fecha is null)
            {
                mensajes.Add($"Fila {f.Numero}: revisa sentido (cobro/pago), concepto, importe y fecha.");
                continue;
            }

            items.Add((sentido.Value, concepto!, importe, fecha.Value));
        }

        mensajes.Insert(0, $"{items.Count} apunte(s) de cartera válidos de {filas.Count} fila(s).");
        if (peticion.Previsualizar)
        {
            return Results.Ok(new ResultadoImportacionExcel(filas.Count, items.Count, filas.Count - items.Count, false, mensajes));
        }

        var creadas = 0;
        foreach (var it in items)
        {
            var r = await caso.EjecutarAsync(contexto.EmpresaId.Value, new CrearPrevisionComando(it.Sentido, it.Concepto, it.Importe, it.Fecha), ct).ConfigureAwait(false);
            if (r.EsCorrecto)
            {
                creadas++;
            }
        }

        return Results.Ok(new ResultadoImportacionExcel(filas.Count, creadas, filas.Count - creadas, true, mensajes));
    }

    private static async Task<IResult> StockAsync(ImportarExcelPeticion peticion, IContextoEmpresa contexto, IConsultaProductos productos, RegistrarMovimientoStock caso, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(peticion);
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        if (!TryLeer(peticion, out var filas, out var err))
        {
            return err!;
        }

        var catalogo = await productos.ListarAsync(contexto.EmpresaId.Value, incluirInactivos: true, ct: ct).ConfigureAwait(false);
        var porReferencia = catalogo.Where(p => !string.IsNullOrWhiteSpace(p.Referencia))
            .GroupBy(p => p.Referencia!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var ajustes = new List<(Guid Id, decimal Cantidad)>();
        var mensajes = new List<string>();
        foreach (var f in filas)
        {
            var referencia = f.Campo("referencia", "codigo", "ean", "sku");
            var cantidad = ImportacionCsv.Numero(f.Campo("cantidad", "stock", "existencias"));
            if (string.IsNullOrWhiteSpace(referencia) || !porReferencia.TryGetValue(referencia!, out var prod))
            {
                mensajes.Add($"Fila {f.Numero}: artículo con referencia «{referencia}» no encontrado.");
                continue;
            }

            if (!prod.ControlarStock)
            {
                mensajes.Add($"Fila {f.Numero}: «{prod.Nombre}» no lleva control de stock.");
                continue;
            }

            ajustes.Add((prod.Id, cantidad));
        }

        mensajes.Insert(0, $"{ajustes.Count} ajuste(s) de stock válidos de {filas.Count} fila(s).");
        if (peticion.Previsualizar)
        {
            return Results.Ok(new ResultadoImportacionExcel(filas.Count, ajustes.Count, filas.Count - ajustes.Count, false, mensajes));
        }

        var aplicados = 0;
        foreach (var a in ajustes)
        {
            var r = await caso.EjecutarAsync(contexto.EmpresaId.Value, a.Id, new DatosMovimientoStock(TipoMovimientoStock.Ajuste, a.Cantidad, "Importación de existencias iniciales"), ct).ConfigureAwait(false);
            if (r.EsCorrecto)
            {
                aplicados++;
            }
        }

        return Results.Ok(new ResultadoImportacionExcel(filas.Count, aplicados, filas.Count - aplicados, true, mensajes));
    }
}
