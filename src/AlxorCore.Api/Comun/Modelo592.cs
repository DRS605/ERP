using System.Globalization;
using System.Text;
using AlxorCore.Compras.Aplicacion;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Informes.Aplicacion;
using AlxorCore.Informes.Dominio;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Api.Comun;

/// <summary>Concepto de un apunte del libro del impuesto sobre los envases de plástico.</summary>
public enum ConceptoPlastico
{
    /// <summary>Adquisición intracomunitaria (llegada de un albarán de compra de un proveedor de otro Estado miembro).</summary>
    AdquisicionIntracomunitaria = 1,

    /// <summary>Fabricación (orden de producción terminada).</summary>
    Fabricacion = 2,

    /// <summary>Envío fuera de España (venta a un cliente de otro país): deducible.</summary>
    EnvioFueraDelTerritorio = 3,
}

public sealed record ApuntePlasticoDto(DateOnly Fecha, string Concepto, string Documento, string? Tercero, string? Pais, Guid ProductoId, string Producto, string Clave,
    decimal Unidades, decimal KgPlastico, decimal KgReciclado, decimal KgNoReciclado, bool Exento, string? MotivoExencion);

public sealed record ResumenClavePlasticoDto(string Clave, decimal KgAdquisiciones, decimal KgFabricacion, decimal KgDeducibles, decimal KgExentos);

/// <summary>Autoliquidación del modelo 592 del periodo: base en kilos de plástico no reciclado, cuota y libro registro.</summary>
public sealed record Modelo592Dto(int Anio, string Periodo, DateOnly Desde, DateOnly Hasta, decimal KgAdquisiciones, decimal KgFabricacion, decimal KgDeducibles,
    decimal KgExentos, bool AdquisicionesNoSujetas, decimal BaseKg, decimal Tipo, decimal Cuota, IReadOnlyList<ResumenClavePlasticoDto> PorClave,
    IReadOnlyList<ApuntePlasticoDto> Libro, IReadOnlyList<string> Avisos);

/// <summary>
/// Modelo 592 del impuesto especial sobre los envases de plástico no reutilizables (Ley 7/2022): a partir de las fichas
/// de plástico de los artículos, suma los kilos de plástico no reciclado de las adquisiciones intracomunitarias (los
/// albaranes de compra de proveedores de otro Estado miembro) y de la fabricación (órdenes de producción terminadas), y
/// deduce los envíos fuera de España (facturas a clientes de otro país). Cuota = base × 0,45 €/kg. Las adquisiciones
/// intracomunitarias que no pasan de 5 kg en el mes no están sujetas. Devuelve también el libro registro del periodo.
/// </summary>
public sealed class Modelo592
{
    /// <summary>Kilos al mes de adquisiciones intracomunitarias por debajo de los cuales no hay sujeción.</summary>
    public const decimal UmbralAdquisicionesMes = 5m;

    private readonly IRepositorioFichasPlastico _fichas;
    private readonly IRepositorioAlbaranes _albaranesCompra;
    private readonly IRepositorioPedidos _pedidosCompra;
    private readonly IConsultaProveedores _proveedores;
    private readonly IConsultaFacturas _facturas;
    private readonly AlxorCore.Catalogo.Aplicacion.IConsultaProductos _productos;
    private readonly AlxorCore.Produccion.Aplicacion.IRepositorioOrdenes? _ordenes;

    public Modelo592(IRepositorioFichasPlastico fichas, IRepositorioAlbaranes albaranesCompra, IRepositorioPedidos pedidosCompra, IConsultaProveedores proveedores,
        IConsultaFacturas facturas, AlxorCore.Catalogo.Aplicacion.IConsultaProductos productos, AlxorCore.Produccion.Aplicacion.IRepositorioOrdenes? ordenes = null)
    {
        _fichas = fichas; _albaranesCompra = albaranesCompra; _pedidosCompra = pedidosCompra; _proveedores = proveedores; _facturas = facturas; _productos = productos;
        _ordenes = ordenes;
    }

    /// <summary>Periodo «1T»…«4T» (trimestral) o «01»…«12» (mensual).</summary>
    public static Resultado<(DateOnly Desde, DateOnly Hasta)> Periodo(int anio, string? periodo)
    {
        var p = (periodo ?? string.Empty).Trim().ToUpperInvariant();
        if (anio is < 2023 or > 2100)
        {
            return Resultado.Fallo<(DateOnly, DateOnly)>(Error.Validacion("m592.anio", "El impuesto se aplica desde 2023."));
        }

        if (p.Length == 2 && p[1] == 'T' && p[0] is >= '1' and <= '4')
        {
            var desde = new DateOnly(anio, ((p[0] - '1') * 3) + 1, 1);
            return Resultado.Ok((desde, desde.AddMonths(3).AddDays(-1)));
        }

        if (int.TryParse(p, NumberStyles.None, CultureInfo.InvariantCulture, out var mes) && mes is >= 1 and <= 12)
        {
            var desde = new DateOnly(anio, mes, 1);
            return Resultado.Ok((desde, desde.AddMonths(1).AddDays(-1)));
        }

        return Resultado.Fallo<(DateOnly, DateOnly)>(Error.Validacion("m592.periodo", "El periodo es 1T a 4T (trimestral) o 01 a 12 (mensual)."));
    }

    public async Task<Resultado<Modelo592Dto>> CalcularAsync(Guid empresaId, int anio, string? periodo, CancellationToken ct = default)
    {
        var p = Periodo(anio, periodo);
        if (p.EsFallo)
        {
            return Resultado.Fallo<Modelo592Dto>(p.Error);
        }

        var (desde, hasta) = p.Valor;
        var fichas = (await _fichas.ListarAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(f => f.ProductoId);
        var avisos = new List<string>();
        if (fichas.Count == 0)
        {
            avisos.Add("Ningún artículo tiene ficha de plástico: indica en cada envase su clave y los kilos de plástico (y de reciclado) por unidad.");
        }

        var nombres = new Dictionary<Guid, string>();
        async Task<string> NombreAsync(Guid id)
        {
            if (!nombres.TryGetValue(id, out var n))
            {
                n = (await _productos.ObtenerAsync(id, ct).ConfigureAwait(false))?.Nombre ?? "?";
                nombres[id] = n;
            }

            return n;
        }

        var libro = new List<ApuntePlasticoDto>();
        async Task AnotarAsync(DateOnly fecha, ConceptoPlastico concepto, string documento, string? tercero, string? pais, Guid productoId, decimal unidades)
        {
            if (!fichas.TryGetValue(productoId, out var f) || unidades == 0m)
            {
                return;
            }

            libro.Add(new ApuntePlasticoDto(fecha, concepto.ToString(), documento, tercero, pais, productoId, await NombreAsync(productoId).ConfigureAwait(false), Letra(f.Clave),
                unidades, Kg(unidades * f.KgPorUnidad), Kg(unidades * f.KgRecicladoPorUnidad), Kg(unidades * f.KgNoRecicladoPorUnidad), f.Exento, f.MotivoExencion));
        }

        // Adquisiciones intracomunitarias: la llegada de la mercancía (albarán de compra) de un proveedor de otro Estado miembro.
        var proveedores = new Dictionary<Guid, ProveedorDto?>();
        foreach (var a in await _albaranesCompra.EnPeriodoAsync(empresaId, desde, hasta, ct).ConfigureAwait(false))
        {
            if (a.AnuladoEn is not null || await _pedidosCompra.ObtenerPorIdAsync(a.PedidoId, ct).ConfigureAwait(false) is not { ProveedorId: { } pid })
            {
                continue;
            }

            if (!proveedores.TryGetValue(pid, out var proveedor))
            {
                proveedor = await _proveedores.ObtenerAsync(pid, ct).ConfigureAwait(false);
                proveedores[pid] = proveedor;
            }

            var pais = Paises.Codigo(proveedor?.Pais);
            if (pais is null || pais == "ES" || !Paises.UnionEuropea.Contains(pais))
            {
                continue;
            }

            foreach (var l in a.Lineas.Where(l => l.ProductoId is not null))
            {
                await AnotarAsync(a.Fecha, ConceptoPlastico.AdquisicionIntracomunitaria, $"Albarán de compra {a.NumeroCompleto}", proveedor?.Nombre, pais, l.ProductoId!.Value,
                    l.Cantidad).ConfigureAwait(false);
            }
        }

        // Fabricación: órdenes de producción terminadas en el periodo.
        if (_ordenes is not null)
        {
            foreach (var o in (await _ordenes.ListarAsync(empresaId, ct).ConfigureAwait(false))
                     .Where(o => o.Estado == "Terminada" && o.TerminadaEn is { } t && DateOnly.FromDateTime(t.UtcDateTime) is var d && d >= desde && d <= hasta))
            {
                await AnotarAsync(DateOnly.FromDateTime(o.TerminadaEn!.Value.UtcDateTime), ConceptoPlastico.Fabricacion, $"Orden de fabricación {o.Ejercicio}/{o.Numero}", null, "ES",
                    o.ProductoId, o.Cantidad).ConfigureAwait(false);
            }
        }

        // Envíos fuera de España: facturas a clientes de otro país (deducibles). Las rectificativas restan lo devuelto.
        if (fichas.Count > 0)
        {
            foreach (var r in (await _facturas.ListarAsync(empresaId, ct).ConfigureAwait(false))
                     .Where(f => f.Estado != "Anulada" && f.FechaEmision >= desde && f.FechaEmision <= hasta))
            {
                var f = await _facturas.ObtenerAsync(r.Id, ct).ConfigureAwait(false);
                var pais = Paises.Codigo(f?.ClientePais);
                if (f is null || pais is null || pais == "ES")
                {
                    continue;
                }

                foreach (var l in f.Lineas.Where(l => l.ProductoId is not null))
                {
                    await AnotarAsync(f.FechaEmision, ConceptoPlastico.EnvioFueraDelTerritorio, $"Factura {f.NumeroCompleto}", f.ClienteNombre, pais, l.ProductoId!.Value, l.Cantidad)
                        .ConfigureAwait(false);
                }
            }
        }

        libro = libro.OrderBy(x => x.Fecha).ThenBy(x => x.Documento, StringComparer.Ordinal).ToList();
        var sujetos = libro.Where(x => !x.Exento).ToList();
        decimal Suma(IEnumerable<ApuntePlasticoDto> xs, ConceptoPlastico c) => Kg(xs.Where(x => x.Concepto == c.ToString()).Sum(x => x.KgNoReciclado));

        // Adquisiciones de no más de 5 kg en un mes: no sujetas (se mira mes a mes).
        var adquisiciones = 0m;
        var noSujetas = false;
        foreach (var mes in sujetos.Where(x => x.Concepto == nameof(ConceptoPlastico.AdquisicionIntracomunitaria)).GroupBy(x => (x.Fecha.Year, x.Fecha.Month)))
        {
            var kg = Kg(mes.Sum(x => x.KgNoReciclado));
            if (kg <= UmbralAdquisicionesMes)
            {
                noSujetas = true;
                avisos.Add($"Adquisiciones intracomunitarias de {mes.Key.Month:00}/{mes.Key.Year}: {Redondeo.Formatear(kg, 3)} kg, no pasan de {UmbralAdquisicionesMes} kg: no sujetas.");
            }
            else
            {
                adquisiciones += kg;
            }
        }

        var fabricacion = Suma(sujetos, ConceptoPlastico.Fabricacion);
        var deducibles = Suma(sujetos, ConceptoPlastico.EnvioFueraDelTerritorio);
        var exentos = Kg(libro.Where(x => x.Exento && x.Concepto != nameof(ConceptoPlastico.EnvioFueraDelTerritorio)).Sum(x => x.KgNoReciclado));
        var baseKg = Kg(adquisiciones + fabricacion - deducibles);
        if (baseKg < 0m)
        {
            avisos.Add("Los envíos fuera de España superan lo adquirido y fabricado en el periodo: la diferencia se compensa o se pide su devolución.");
        }

        var porClave = libro.GroupBy(x => x.Clave).OrderBy(g => g.Key, StringComparer.Ordinal).Select(g => new ResumenClavePlasticoDto(g.Key,
            Suma(g.Where(x => !x.Exento), ConceptoPlastico.AdquisicionIntracomunitaria), Suma(g.Where(x => !x.Exento), ConceptoPlastico.Fabricacion),
            Suma(g.Where(x => !x.Exento), ConceptoPlastico.EnvioFueraDelTerritorio),
            Kg(g.Where(x => x.Exento && x.Concepto != nameof(ConceptoPlastico.EnvioFueraDelTerritorio)).Sum(x => x.KgNoReciclado)))).ToList();
        return Resultado.Ok(new Modelo592Dto(anio, (periodo ?? string.Empty).Trim().ToUpperInvariant(), desde, hasta, Kg(adquisiciones), fabricacion, deducibles, exentos,
            noSujetas, baseKg, FichaPlastico.TipoPorKg, Redondeo.Dos(baseKg * FichaPlastico.TipoPorKg), porClave, libro, avisos));
    }

    /// <summary>Libro registro del periodo en CSV (separado por punto y coma, para Excel).</summary>
    public static string LibroCsv(Modelo592Dto m)
    {
        ArgumentNullException.ThrowIfNull(m);
        var sb = new StringBuilder("Fecha;Concepto;Documento;Tercero;País;Artículo;Clave;Unidades;Kg plástico;Kg reciclado;Kg no reciclado;Exento\r\n");
        foreach (var a in m.Libro)
        {
            sb.Append(CultureInfo.InvariantCulture, $"{a.Fecha:dd/MM/yyyy};{a.Concepto};{Csv(a.Documento)};{Csv(a.Tercero)};{a.Pais};{Csv(a.Producto)};{a.Clave};")
                .Append(CultureInfo.InvariantCulture, $"{Num(a.Unidades)};{Num(a.KgPlastico)};{Num(a.KgReciclado)};{Num(a.KgNoReciclado)};{(a.Exento ? "Sí" : "No")}\r\n");
        }

        return sb.ToString();
    }

    private static string Num(decimal v) => v.ToString("0.###", CultureInfo.InvariantCulture).Replace('.', ',');

    private static string Csv(string? t) => t is null ? string.Empty : t.Contains(';', StringComparison.Ordinal) || t.Contains('"', StringComparison.Ordinal)
        ? $"\"{t.Replace("\"", "\"\"", StringComparison.Ordinal)}\"" : t;

    private static decimal Kg(decimal v) => Math.Round(v, 3, MidpointRounding.AwayFromZero);

    private static string Letra(ClavePlastico c) => c switch
    {
        ClavePlastico.Envase => "A",
        ClavePlastico.Semielaborado => "B",
        _ => "C",
    };
}
