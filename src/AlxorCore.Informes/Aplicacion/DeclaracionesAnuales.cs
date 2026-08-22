using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Informes.Aplicacion;

/// <summary>
/// Resumen del <b>modelo 390</b> (declaración-resumen anual del IVA): es la suma de los cuatro
/// trimestres del modelo 303 del ejercicio. IVA devengado (repercutido en las facturas emitidas del
/// año) menos IVA deducible (soportado en los gastos del año).
/// </summary>
public sealed record Modelo390Dto(
    int Anio,
    decimal IvaDevengadoBase, decimal IvaDevengadoCuota,
    decimal IvaDeducibleBase, decimal IvaDeducibleCuota,
    decimal Resultado);

/// <summary>
/// Un tercero (cliente o proveedor) con su volumen de operaciones del año para el modelo 347.
/// <paramref name="ClaveOperacion"/> es la clave AEAT (B = ventas/entregas del declarante;
/// A = compras/adquisiciones) y T1–T4 el desglose por trimestre (IVA incluido).
/// </summary>
public sealed record Modelo347LineaDto(
    string Clave, string Nombre, string? Nif, string Sentido, string ClaveOperacion,
    decimal ImporteAnual, decimal T1, decimal T2, decimal T3, decimal T4);

/// <summary>
/// Resumen del <b>modelo 347</b> (declaración anual de operaciones con terceros): relación de
/// clientes y proveedores con los que el volumen de operaciones del año (IVA incluido) ha superado
/// el umbral legal de <b>3.005,06 €</b>.
/// </summary>
public sealed record Modelo347Dto(
    int Anio, decimal Umbral,
    IReadOnlyList<Modelo347LineaDto> Clientes,
    IReadOnlyList<Modelo347LineaDto> Proveedores);

/// <summary>Declaraciones anuales de una empresa: modelo 390 (IVA) y modelo 347 (operaciones con terceros).</summary>
public sealed record DeclaracionAnualDto(Modelo390Dto Modelo390, Modelo347Dto Modelo347);

/// <summary>
/// Caso de uso: calcula las declaraciones anuales (390 y 347) a partir de las facturas emitidas, los
/// gastos y los datos de los proveedores. Es una <b>ayuda informativa</b> para preparar la
/// declaración con la gestoría, no un envío oficial a la AEAT.
/// </summary>
public sealed class GenerarDeclaracionAnual
{
    /// <summary>Umbral legal del modelo 347: 3.005,06 € (IVA incluido) de volumen anual con un tercero.</summary>
    public const decimal Umbral347 = 3005.06m;

    private readonly IConsultaFacturas _facturas;
    private readonly IConsultaGastos _gastos;
    private readonly IConsultaProveedores _proveedores;
    private readonly IConsultaEmpresas _empresas;

    public GenerarDeclaracionAnual(IConsultaFacturas facturas, IConsultaGastos gastos, IConsultaProveedores proveedores, IConsultaEmpresas empresas)
    {
        _facturas = facturas;
        _gastos = gastos;
        _proveedores = proveedores;
        _empresas = empresas;
    }

    /// <summary>
    /// Genera el fichero telemático oficial del modelo 347 del ejercicio (bytes ISO-8859-1). Solo
    /// incluye los terceros con NIF que superan el umbral. Devuelve null si no hay empresa ni nada
    /// que declarar.
    /// </summary>
    public async Task<byte[]?> FicheroModelo347Async(Guid empresaId, int anio, CancellationToken ct = default)
    {
        var empresa = await _empresas.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        if (empresa is null)
        {
            return null;
        }

        var decl = await EjecutarAsync(empresaId, anio, ct).ConfigureAwait(false);
        var lineas = decl.Modelo347.Clientes.Concat(decl.Modelo347.Proveedores)
            .Where(l => l.Nif is { Length: > 0 })
            .ToList();
        if (lineas.Count == 0)
        {
            return null;
        }

        return FicheroModelo347.Generar(new DeclaranteAeat(empresa.Nif, empresa.RazonSocial), anio, lineas);
    }

    public async Task<DeclaracionAnualDto> EjecutarAsync(Guid empresaId, int anio, CancellationToken ct = default)
    {
        var desde = new DateOnly(anio, 1, 1);
        var hasta = new DateOnly(anio, 12, 31);

        var facturas = await _facturas.ListarAsync(empresaId, ct).ConfigureAwait(false);
        var gastos = await _gastos.ListarAsync(empresaId, ct).ConfigureAwait(false);
        var proveedores = await _proveedores.ListarAsync(empresaId, true, ct).ConfigureAwait(false);

        // Solo cuentan las facturas realmente emitidas del ejercicio (se excluyen anuladas y ya
        // rectificadas, cuya rectificativa aporta los importes corregidos).
        var emitidas = facturas
            .Where(f => f.Estado == "Emitida" && f.FechaEmision >= desde && f.FechaEmision <= hasta)
            .ToList();
        var gastosAnio = gastos.Where(g => g.Fecha >= desde && g.Fecha <= hasta).ToList();

        return new DeclaracionAnualDto(
            Calcular390(anio, emitidas, gastosAnio),
            Calcular347(anio, emitidas, gastosAnio, proveedores));
    }

    private static Modelo390Dto Calcular390(int anio, IReadOnlyList<FacturaResumen> facturas, IReadOnlyList<GastoDto> gastos)
    {
        var devBase = Redondeo.Dos(facturas.Sum(f => f.BaseImponible));
        var devCuota = Redondeo.Dos(facturas.Sum(f => f.CuotaIva));
        var dedBase = Redondeo.Dos(gastos.Sum(g => g.BaseImponible));
        var dedCuota = Redondeo.Dos(gastos.Sum(g => g.CuotaIva));
        return new Modelo390Dto(anio, devBase, devCuota, dedBase, dedCuota, Redondeo.Dos(devCuota - dedCuota));
    }

    private static Modelo347Dto Calcular347(
        int anio, IReadOnlyList<FacturaResumen> facturas, IReadOnlyList<GastoDto> gastos, IReadOnlyList<ProveedorDto> proveedores)
    {
        // Clientes: se agrupan por NIF si lo hay; si no, por nombre (las facturas congelan ambos).
        var clientes = facturas
            .GroupBy(f => f.ClienteNif is { Length: > 0 } nif ? "nif:" + nif : "nom:" + f.ClienteNombre)
            .Select(g =>
            {
                var primera = g.First();
                var (t1, t2, t3, t4) = PorTrimestre(g.Select(f => (f.FechaEmision, f.Total)));
                return new Modelo347LineaDto(g.Key, primera.ClienteNombre, primera.ClienteNif, "Cliente", "B",
                    Redondeo.Dos(g.Sum(f => f.Total)), t1, t2, t3, t4);
            })
            .Where(l => l.ImporteAnual > Umbral347)
            .OrderByDescending(l => l.ImporteAnual)
            .ToList();

        // Proveedores: se agrupan por su id (resolviendo nombre y NIF del maestro de proveedores);
        // los gastos sin proveedor asociado se agrupan por el texto libre.
        var mapa = proveedores.ToDictionary(p => p.Id);
        var proveedoresLinea = gastos
            .GroupBy(g => g.ProveedorId is { } id ? "id:" + id : "txt:" + (g.ProveedorTexto ?? "Sin proveedor"))
            .Select(g =>
            {
                var primero = g.First();
                string nombre;
                string? nif;
                if (primero.ProveedorId is { } id && mapa.TryGetValue(id, out var prov))
                {
                    nombre = prov.Nombre;
                    nif = prov.NifFiscal;
                }
                else
                {
                    nombre = primero.ProveedorTexto ?? "Sin proveedor";
                    nif = null;
                }

                var (t1, t2, t3, t4) = PorTrimestre(g.Select(x => (x.Fecha, x.Total)));
                return new Modelo347LineaDto(g.Key, nombre, nif, "Proveedor", "A", Redondeo.Dos(g.Sum(x => x.Total)), t1, t2, t3, t4);
            })
            .Where(l => l.ImporteAnual > Umbral347)
            .OrderByDescending(l => l.ImporteAnual)
            .ToList();

        return new Modelo347Dto(anio, Umbral347, clientes, proveedoresLinea);
    }

    /// <summary>Reparte importes por trimestre según su fecha.</summary>
    private static (decimal T1, decimal T2, decimal T3, decimal T4) PorTrimestre(IEnumerable<(DateOnly Fecha, decimal Importe)> ops)
    {
        decimal t1 = 0m, t2 = 0m, t3 = 0m, t4 = 0m;
        foreach (var (fecha, importe) in ops)
        {
            switch ((fecha.Month - 1) / 3)
            {
                case 0: t1 += importe; break;
                case 1: t2 += importe; break;
                case 2: t3 += importe; break;
                default: t4 += importe; break;
            }
        }

        return (Redondeo.Dos(t1), Redondeo.Dos(t2), Redondeo.Dos(t3), Redondeo.Dos(t4));
    }
}
