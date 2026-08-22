using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Informes.Aplicacion;

/// <summary>
/// Un operador intracomunitario con el importe (base imponible) de las operaciones del periodo y su
/// clave: <b>E</b> entregas intracomunitarias de bienes (ventas del declarante) · <b>A</b>
/// adquisiciones intracomunitarias (compras del declarante).
/// </summary>
public sealed record Modelo349OperadorDto(string Clave, string NifIva, string Nombre, decimal BaseImponible);

/// <summary>
/// Resumen del <b>modelo 349</b> (declaración recapitulativa de operaciones intracomunitarias) de un
/// periodo. Los operadores se detectan porque el cliente/proveedor tiene <b>NIF-IVA (VIES)</b>.
/// </summary>
public sealed record Modelo349Dto(
    int Anio, int Trimestre, string Periodo, decimal BaseTotal, IReadOnlyList<Modelo349OperadorDto> Operadores);

/// <summary>
/// Caso de uso: calcula el modelo 349 a partir de las facturas emitidas a clientes con NIF-IVA
/// (entregas) y de los gastos de proveedores con NIF-IVA (adquisiciones), y genera su fichero
/// telemático. Ayuda para preparar la declaración con la gestoría.
/// </summary>
public sealed class GenerarModelo349
{
    public const string ClaveEntregas = "E";
    public const string ClaveAdquisiciones = "A";

    private readonly IConsultaFacturas _facturas;
    private readonly IConsultaGastos _gastos;
    private readonly IConsultaClientes _clientes;
    private readonly IConsultaProveedores _proveedores;
    private readonly IConsultaEmpresas _empresas;

    public GenerarModelo349(IConsultaFacturas facturas, IConsultaGastos gastos, IConsultaClientes clientes,
        IConsultaProveedores proveedores, IConsultaEmpresas empresas)
    {
        _facturas = facturas;
        _gastos = gastos;
        _clientes = clientes;
        _proveedores = proveedores;
        _empresas = empresas;
    }

    public async Task<Modelo349Dto> EjecutarAsync(Guid empresaId, int anio, int trimestre, CancellationToken ct = default)
    {
        var (desde, hasta) = RangoTrimestre(anio, trimestre);
        var operadores = await OperadoresAsync(empresaId, desde, hasta, ct).ConfigureAwait(false);
        var total = Redondeo.Dos(operadores.Sum(o => o.BaseImponible));
        return new Modelo349Dto(anio, trimestre, $"{trimestre}T", total, operadores);
    }

    /// <summary>Genera el fichero telemático del 349; null si no hay empresa ni operadores.</summary>
    public async Task<byte[]?> FicheroAsync(Guid empresaId, int anio, int trimestre, CancellationToken ct = default)
    {
        var empresa = await _empresas.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        if (empresa is null)
        {
            return null;
        }

        var modelo = await EjecutarAsync(empresaId, anio, trimestre, ct).ConfigureAwait(false);
        if (modelo.Operadores.Count == 0)
        {
            return null;
        }

        return FicheroModelo349.Generar(new DeclaranteAeat(empresa.Nif, empresa.RazonSocial), modelo);
    }

    private async Task<List<Modelo349OperadorDto>> OperadoresAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct)
    {
        var facturas = await _facturas.ListarAsync(empresaId, ct).ConfigureAwait(false);
        var gastos = await _gastos.ListarAsync(empresaId, ct).ConfigureAwait(false);
        var clientes = await _clientes.ListarAsync(empresaId, true, ct).ConfigureAwait(false);
        var proveedores = await _proveedores.ListarAsync(empresaId, true, ct).ConfigureAwait(false);

        // Entregas: clientes con NIF-IVA. Se casan las facturas por su NIF fiscal (congelado).
        var clientesIntra = clientes
            .Where(c => c.NifIva is { Length: > 0 } && c.NifFiscal is { Length: > 0 })
            .ToDictionary(c => c.NifFiscal!.Trim().ToUpperInvariant());

        var entregas = facturas
            .Where(f => f.Estado == "Emitida" && f.FechaEmision >= desde && f.FechaEmision <= hasta
                        && f.ClienteNif is { Length: > 0 } && clientesIntra.ContainsKey(f.ClienteNif.Trim().ToUpperInvariant()))
            .GroupBy(f => clientesIntra[f.ClienteNif!.Trim().ToUpperInvariant()])
            .Select(g => new Modelo349OperadorDto(ClaveEntregas, g.Key.NifIva!, g.Key.Nombre, Redondeo.Dos(g.Sum(f => f.BaseImponible))));

        // Adquisiciones: proveedores con NIF-IVA. Se casan los gastos por su id.
        var proveedoresIntra = proveedores.Where(p => p.NifIva is { Length: > 0 }).ToDictionary(p => p.Id);

        var adquisiciones = gastos
            .Where(g => g.Estado != "Anulado" && g.Fecha >= desde && g.Fecha <= hasta
                        && g.ProveedorId is { } id && proveedoresIntra.ContainsKey(id))
            .GroupBy(g => proveedoresIntra[g.ProveedorId!.Value])
            .Select(g => new Modelo349OperadorDto(ClaveAdquisiciones, g.Key.NifIva!, g.Key.Nombre, Redondeo.Dos(g.Sum(x => x.BaseImponible))));

        return entregas.Concat(adquisiciones)
            .Where(o => o.BaseImponible != 0m)
            .OrderBy(o => o.Clave).ThenByDescending(o => o.BaseImponible)
            .ToList();
    }

    private static (DateOnly Desde, DateOnly Hasta) RangoTrimestre(int anio, int trimestre)
    {
        var mesInicio = ((trimestre - 1) * 3) + 1;
        var desde = new DateOnly(anio, mesInicio, 1);
        return (desde, desde.AddMonths(3).AddDays(-1));
    }
}
