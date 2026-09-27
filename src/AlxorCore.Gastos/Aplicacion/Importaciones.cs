using AlxorCore.Gastos.Dominio;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Gastos.Aplicacion;

public sealed record DuaImportacionDto(Guid Id, Guid GastoId, string Mrn, DateOnly FechaAdmision, string? Aduana, decimal BaseIva, decimal Aranceles, decimal CuotaIva,
    string? Observaciones)
{
    public static DuaImportacionDto Desde(DuaImportacion d) => new(d.Id, d.GastoId, d.Mrn, d.FechaAdmision, d.Aduana, d.BaseIva, d.Aranceles, d.CuotaIva, d.Observaciones);
}

public sealed record DatosDuaImportacion(string? Mrn, DateOnly FechaAdmision, decimal BaseIva, decimal CuotaIva, decimal Aranceles = 0m, string? Aduana = null,
    string? Observaciones = null);

/// <summary>Compra a un proveedor de fuera de la UE (o con IVA de importación) y sus DUA de importación.</summary>
public sealed record ImportacionDto(Guid GastoId, string Proveedor, string? Pais, string Concepto, DateOnly Fecha, decimal BaseImponible, string CodigoIva, string Situacion,
    decimal CuotaIvaImportacion, decimal Aranceles, IReadOnlyList<DuaImportacionDto> Duas);

public interface IRepositorioDuasImportacion
{
    Task<IReadOnlyList<DuaImportacion>> ListarAsync(Guid empresaId, CancellationToken ct = default);

    Task<DuaImportacion?> ObtenerAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExisteMrnAsync(Guid empresaId, string mrn, Guid? salvo, CancellationToken ct = default);

    void Agregar(DuaImportacion dua);

    void Eliminar(DuaImportacion dua);
}

/// <summary>
/// Importaciones: los gastos (facturas de proveedor) de proveedores de fuera de la UE o con IVA de importación, y sus
/// DUA de importación con el IVA liquidado en la aduana.
/// </summary>
public sealed class Importaciones
{
    private readonly IRepositorioDuasImportacion _duas;
    private readonly IConsultaGastos _gastos;
    private readonly IConsultaProveedores _proveedores;
    private readonly IUnidadDeTrabajoGastos _unidad;

    public Importaciones(IRepositorioDuasImportacion duas, IConsultaGastos gastos, IConsultaProveedores proveedores, IUnidadDeTrabajoGastos unidad)
    {
        _duas = duas; _gastos = gastos; _proveedores = proveedores; _unidad = unidad;
    }

    private static bool EsIvaImportacion(string codigo) =>
        codigo.StartsWith("IMPORT", StringComparison.OrdinalIgnoreCase) || codigo.StartsWith("IGICIMP", StringComparison.OrdinalIgnoreCase);

    public async Task<IReadOnlyList<ImportacionDto>> ListarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var duas = (await _duas.ListarAsync(empresaId, ct).ConfigureAwait(false)).GroupBy(d => d.GastoId).ToDictionary(g => g.Key, g => g.ToList());
        var paises = new Dictionary<Guid, string?>();
        var resultado = new List<ImportacionDto>();
        foreach (var g in (await _gastos.ListarAsync(empresaId, ct).ConfigureAwait(false)).Where(g => g.Estado != nameof(EstadoGasto.Anulado)))
        {
            string? pais = null;
            if (g.ProveedorId is { } p)
            {
                if (!paises.TryGetValue(p, out pais))
                {
                    pais = Paises.Codigo((await _proveedores.ObtenerAsync(p, ct).ConfigureAwait(false))?.Pais);
                    paises[p] = pais;
                }
            }

            var fueraUe = pais is { } c && c != "ES" && !Paises.UnionEuropea.Contains(c);
            var suyos = duas.GetValueOrDefault(g.Id) ?? [];
            if (!fueraUe && !EsIvaImportacion(g.CodigoIva) && suyos.Count == 0)
            {
                continue;
            }

            resultado.Add(new ImportacionDto(g.Id, g.ProveedorTexto ?? "—", pais, g.Concepto, g.Fecha, g.BaseImponible, g.CodigoIva, suyos.Count == 0 ? "Sin DUA" : "Con DUA",
                suyos.Sum(d => d.CuotaIva), suyos.Sum(d => d.Aranceles), suyos.OrderBy(d => d.FechaAdmision).Select(DuaImportacionDto.Desde).ToList()));
        }

        return resultado.OrderBy(i => i.Situacion == "Sin DUA" ? 0 : 1).ThenByDescending(i => i.Fecha).ToList();
    }

    public async Task<Resultado<DuaImportacionDto>> RegistrarAsync(Guid empresaId, Guid gastoId, DatosDuaImportacion datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var gasto = await _gastos.ObtenerAsync(gastoId, ct).ConfigureAwait(false);
        if (gasto is null)
        {
            return Resultado.Fallo<DuaImportacionDto>(Error.NoEncontrado("gasto.no_encontrado", "El gasto no existe."));
        }

        if (gasto.Estado == nameof(EstadoGasto.Anulado))
        {
            return Resultado.Fallo<DuaImportacionDto>(Error.Conflicto("dua.gasto_anulado", "El gasto está anulado: no lleva DUA."));
        }

        var d = DuaImportacion.Crear(empresaId, gastoId, datos.Mrn, datos.FechaAdmision, datos.Aduana, datos.BaseIva, datos.Aranceles, datos.CuotaIva, datos.Observaciones);
        if (d.EsFallo)
        {
            return Resultado.Fallo<DuaImportacionDto>(d.Error);
        }

        if (await _duas.ExisteMrnAsync(empresaId, d.Valor.Mrn, null, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<DuaImportacionDto>(Error.Conflicto("dua.mrn_duplicado", $"El MRN {d.Valor.Mrn} ya está registrado."));
        }

        _duas.Agregar(d.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(DuaImportacionDto.Desde(d.Valor));
    }

    public async Task<Resultado<DuaImportacionDto>> ModificarAsync(Guid empresaId, Guid id, DatosDuaImportacion datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        if (await _duas.ObtenerAsync(id, ct).ConfigureAwait(false) is not { } d || d.EmpresaId != empresaId)
        {
            return Resultado.Fallo<DuaImportacionDto>(Error.NoEncontrado("dua.no_encontrado", "El DUA no existe."));
        }

        var r = d.Modificar(datos.Mrn, datos.FechaAdmision, datos.Aduana, datos.BaseIva, datos.Aranceles, datos.CuotaIva, datos.Observaciones);
        if (r.EsFallo)
        {
            return Resultado.Fallo<DuaImportacionDto>(r.Error);
        }

        if (await _duas.ExisteMrnAsync(empresaId, d.Mrn, d.Id, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<DuaImportacionDto>(Error.Conflicto("dua.mrn_duplicado", $"El MRN {d.Mrn} ya está registrado."));
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(DuaImportacionDto.Desde(d));
    }

    public async Task<Resultado> EliminarAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        if (await _duas.ObtenerAsync(id, ct).ConfigureAwait(false) is not { } d || d.EmpresaId != empresaId)
        {
            return Resultado.Fallo(Error.NoEncontrado("dua.no_encontrado", "El DUA no existe."));
        }

        _duas.Eliminar(d);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }
}
