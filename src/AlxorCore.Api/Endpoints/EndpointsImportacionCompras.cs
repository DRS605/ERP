using AlxorCore.Api.Comun;
using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Catalogo.Dominio;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Api.Endpoints;

/// <summary>Petición de importación de factura de proveedor (Excel/CSV en base64).</summary>
public sealed record ImportarFacturaPeticion(string ContenidoBase64, bool Previsualizar = true, string? ProveedorTexto = null, string? Fecha = null, bool SoloArticulos = false);

/// <summary>Petición de importación de precios de venta por EAN.</summary>
public sealed record ImportarPreciosPeticion(string ContenidoBase64, bool Previsualizar = true, bool PreciosConIva = true);

/// <summary>
/// Importaciones de compras: (A) factura de proveedor por EAN que crea los artículos que no existan
/// (con su IVA y coste), da entrada de stock y registra el gasto; y (B) precios de venta por EAN.
/// </summary>
public static class EndpointsImportacionCompras
{
    public static IEndpointRouteBuilder MapearImportacionCompras(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var grupo = rutas.MapGroup("/importar").WithTags("Importación");

        grupo.MapPost("/factura-proveedor", FacturaProveedorAsync)
            .WithSummary("Importa una factura de proveedor (EAN, descripción, cantidad, coste, IVA): crea los artículos nuevos, da entrada de stock y registra el gasto.")
            .RequierePermiso(Permisos.ProductoGestionar);

        grupo.MapPost("/precios-venta", PreciosVentaAsync)
            .WithSummary("Fija el precio de venta de los artículos por EAN desde Excel/CSV (columnas: ean, precio).")
            .RequierePermiso(Permisos.ProductoGestionar);

        return rutas;
    }

    /// <summary>Lee el fichero (sniff xlsx vs CSV) a filas normalizadas.</summary>
    private static bool LeerFilas(string? base64, out IReadOnlyList<LectorCsv.FilaCsv> filas, out IResult? error)
    {
        filas = [];
        error = null;
        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(base64 ?? string.Empty);
        }
        catch (FormatException)
        {
            error = ResultadosHttp.AProblema(Error.Validacion("importar.base64", "El archivo no es un base64 válido."));
            return false;
        }

        try
        {
            // Un .xlsx es un ZIP y empieza por "PK"; si no, lo tratamos como CSV de texto.
            filas = bytes.Length >= 2 && bytes[0] == 0x50 && bytes[1] == 0x4B
                ? LectorExcel.Parsear(bytes)
                : LectorCsv.Parsear(System.Text.Encoding.UTF8.GetString(bytes));
        }
        catch (Exception ex) when (ex is InvalidOperationException or InvalidDataException)
        {
            error = ResultadosHttp.AProblema(Error.Validacion("importar.archivo_invalido", "No se pudo leer el archivo: " + ex.Message));
            return false;
        }

        return true;
    }

    private sealed record LineaFactura(string Ean, string Descripcion, decimal Cantidad, decimal Coste, string CodigoIva, bool Nuevo, Guid? ProductoId);

    private static async Task<IResult> FacturaProveedorAsync(
        ImportarFacturaPeticion peticion, IContextoEmpresa contexto, IConsultaProductos productos,
        CrearProducto crear, RegistrarMovimientoStock stock, RegistrarGasto gasto, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(peticion);
        if (contexto.EmpresaId is null || contexto.GrupoId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        if (!LeerFilas(peticion.ContenidoBase64, out var filas, out var err))
        {
            return err!;
        }

        var catalogo = await productos.ListarAsync(contexto.EmpresaId.Value, incluirInactivos: true, ct: ct).ConfigureAwait(false);
        var porRef = catalogo.Where(p => !string.IsNullOrWhiteSpace(p.Referencia))
            .GroupBy(p => p.Referencia!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var lineas = new List<LineaFactura>();
        var mensajes = new List<string>();
        foreach (var f in filas)
        {
            var ean = f.Campo("ean", "codigo", "referencia", "sku", "código", "codigo de barras")?.Trim();
            var descripcion = f.Campo("descripcion", "nombre", "articulo", "artículo", "producto", "concepto");
            var cantidad = ImportacionCsv.Numero(f.Campo("cantidad", "uds", "unidades", "cant"));
            var coste = ImportacionCsv.Numero(f.Campo("coste", "precio", "precio compra", "precio de compra", "importe", "neto"));
            var codigoIva = ImportacionCsv.CodigoIva(f.Campo("iva", "tipo iva", "codigo iva", "% iva")) ?? "IVA21";

            if (string.IsNullOrWhiteSpace(ean))
            {
                mensajes.Add($"Fila {f.Numero}: sin EAN/código.");
                continue;
            }

            if (cantidad <= 0m)
            {
                cantidad = 1m;
            }

            var existe = porRef.TryGetValue(ean, out var prod);
            if (existe)
            {
                lineas.Add(new LineaFactura(ean, prod!.Nombre, cantidad, coste, prod.CodigoIva, false, prod.Id));
            }
            else
            {
                if (string.IsNullOrWhiteSpace(descripcion))
                {
                    descripcion = "Artículo " + ean;
                }

                lineas.Add(new LineaFactura(ean, descripcion!, cantidad, coste, codigoIva, true, null));
            }
        }

        var nuevos = lineas.Count(l => l.Nuevo);
        var existentes = lineas.Count(l => !l.Nuevo);
        var totalBase = lineas.Sum(l => l.Cantidad * l.Coste);
        mensajes.Insert(0, peticion.SoloArticulos
            ? $"{lineas.Count} línea(s): se crearán {nuevos} artículo(s) nuevo(s); {existentes} ya existen (se omiten). Sin stock ni gasto."
            : $"{lineas.Count} línea(s): {nuevos} artículo(s) nuevo(s), {existentes} ya existente(s). Base total {totalBase:F2} €.");

        if (peticion.Previsualizar)
        {
            return Results.Ok(new ResultadoImportacionExcel(filas.Count, lineas.Count, filas.Count - lineas.Count, false, mensajes));
        }

        var fecha = LectorExcel.FechaDeSerie(peticion.Fecha ?? string.Empty);
        var empresaId = contexto.EmpresaId.Value;
        var grupoId = contexto.GrupoId.Value;
        var baseporIva = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        var creados = 0;

        foreach (var l in lineas)
        {
            var productoId = l.ProductoId;
            var controlaStock = !l.Nuevo && porRef.TryGetValue(l.Ean, out var p) && p.ControlarStock;

            if (l.Nuevo)
            {
                var datos = new DatosProducto(
                    Nombre: l.Descripcion, PrecioUnitario: 0m, Referencia: l.Ean, Tipo: TipoProducto.Bien,
                    CodigoIva: l.CodigoIva, PrecioCompra: l.Coste, ControlarStock: true);
                var creado = await crear.EjecutarAsync(grupoId, empresaId, datos, ct).ConfigureAwait(false);
                if (creado.EsFallo)
                {
                    mensajes.Add($"EAN {l.Ean}: no se pudo crear ({creado.Error.Mensaje}).");
                    continue;
                }

                productoId = creado.Valor.Id;
                controlaStock = true;
                creados++;
            }

            if (!peticion.SoloArticulos && productoId is { } pid && controlaStock)
            {
                await stock.EjecutarAsync(empresaId, pid, new DatosMovimientoStock(TipoMovimientoStock.Entrada, l.Cantidad, "Importación factura proveedor"), ct).ConfigureAwait(false);
            }

            var iva = l.Nuevo ? l.CodigoIva : (porRef.TryGetValue(l.Ean, out var pe) ? pe.CodigoIva : l.CodigoIva);
            baseporIva[iva] = baseporIva.GetValueOrDefault(iva) + (l.Cantidad * l.Coste);
        }

        if (peticion.SoloArticulos)
        {
            mensajes.Add($"Creados {creados} artículo(s) nuevo(s). No se ha tocado el stock ni se ha registrado ningún gasto.");
            return Results.Ok(new ResultadoImportacionExcel(filas.Count, lineas.Count, filas.Count - lineas.Count, true, mensajes));
        }

        var concepto = string.IsNullOrWhiteSpace(peticion.ProveedorTexto)
            ? "Factura de proveedor (importada)"
            : $"Factura de {peticion.ProveedorTexto!.Trim()}";
        foreach (var (iva, baseImp) in baseporIva.Where(x => x.Value > 0m))
        {
            await gasto.EjecutarAsync(empresaId, new RegistrarGastoComando(
                Concepto: concepto, BaseImponible: baseImp, ProveedorTexto: peticion.ProveedorTexto, CodigoIva: iva, Fecha: fecha), ct).ConfigureAwait(false);
        }

        mensajes.Add($"Creados {creados} artículo(s). Gasto registrado en {baseporIva.Count(x => x.Value > 0m)} tipo(s) de IVA.");
        return Results.Ok(new ResultadoImportacionExcel(filas.Count, lineas.Count, filas.Count - lineas.Count, true, mensajes));
    }

    private static async Task<IResult> PreciosVentaAsync(
        ImportarPreciosPeticion peticion, IContextoEmpresa contexto, IConsultaProductos productos,
        ActualizarProducto actualizar, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(peticion);
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        if (!LeerFilas(peticion.ContenidoBase64, out var filas, out var err))
        {
            return err!;
        }

        var catalogo = await productos.ListarAsync(contexto.EmpresaId.Value, incluirInactivos: true, ct: ct).ConfigureAwait(false);
        var porRef = catalogo.Where(p => !string.IsNullOrWhiteSpace(p.Referencia))
            .GroupBy(p => p.Referencia!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var cambios = new List<(ProductoDto Prod, decimal Base)>();
        var mensajes = new List<string>();
        foreach (var f in filas)
        {
            var ean = f.Campo("ean", "codigo", "referencia", "sku", "código")?.Trim();
            var precio = ImportacionCsv.Numero(f.Campo("precio", "pvp", "precio venta", "precio de venta", "importe"));
            if (string.IsNullOrWhiteSpace(ean) || !porRef.TryGetValue(ean, out var prod))
            {
                mensajes.Add($"Fila {f.Numero}: EAN «{ean}» no encontrado.");
                continue;
            }

            if (precio <= 0m)
            {
                mensajes.Add($"Fila {f.Numero}: precio no válido para «{prod.Nombre}».");
                continue;
            }

            var baseUnit = peticion.PreciosConIva ? Math.Round(precio / (1 + (prod.PorcentajeIva / 100m)), 4) : precio;
            cambios.Add((prod, baseUnit));
        }

        mensajes.Insert(0, $"{cambios.Count} precio(s) a actualizar de {filas.Count} fila(s). ({(peticion.PreciosConIva ? "precios con IVA" : "precios sin IVA")})");
        if (peticion.Previsualizar)
        {
            return Results.Ok(new ResultadoImportacionExcel(filas.Count, cambios.Count, filas.Count - cambios.Count, false, mensajes));
        }

        var aplicados = 0;
        foreach (var (prod, baseUnit) in cambios)
        {
            var datos = DesdeDto(prod, baseUnit);
            var r = await actualizar.EjecutarAsync(prod.Id, datos, ct).ConfigureAwait(false);
            if (r.EsCorrecto)
            {
                aplicados++;
            }
        }

        return Results.Ok(new ResultadoImportacionExcel(filas.Count, aplicados, filas.Count - aplicados, true, mensajes));
    }

    /// <summary>Reconstruye los datos del producto cambiando sólo el precio de venta (no pisa el resto).</summary>
    private static DatosProducto DesdeDto(ProductoDto p, decimal nuevoPrecio) => new(
        Nombre: p.Nombre, PrecioUnitario: nuevoPrecio, Referencia: p.Referencia, Tipo: p.Tipo, CodigoIva: p.CodigoIva,
        Unidad: p.Unidad, PrecioCompra: p.PrecioCompra, ProveedorHabitualId: p.ProveedorHabitualId,
        ControlarStock: p.ControlarStock, StockInicial: 0m, UnidadCompra: p.UnidadCompra, FactorCompra: p.FactorCompra,
        UnidadVenta: p.UnidadVenta, FactorVenta: p.FactorVenta, Seguimiento: p.Seguimiento, Familia: p.Familia,
        FamiliaId: p.FamiliaId, ActividadNegocioId: p.ActividadNegocioId);
}
