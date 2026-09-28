using AlxorCore.Contabilidad.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Contabilidad.Aplicacion;

public interface IRepositorioPlantillasAsiento
{
    void Agregar(PlantillaAsiento plantilla);

    void Eliminar(PlantillaAsiento plantilla);

    Task<PlantillaAsiento?> ObtenerAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<PlantillaAsiento>> ListarAsync(Guid empresaId, CancellationToken ct = default);

    /// <summary>La plantilla activa del origen del documento o, si no hay, la de su sentido (sin origen).</summary>
    async Task<PlantillaAsiento?> AplicableAsync(Guid empresaId, SentidoContable sentido, string? origenTipo, CancellationToken ct = default)
    {
        var activas = (await ListarAsync(empresaId, ct).ConfigureAwait(false)).Where(p => p.Activa && p.Sentido == sentido).ToList();
        return activas.FirstOrDefault(p => p.OrigenTipo is not null && string.Equals(p.OrigenTipo, origenTipo, StringComparison.OrdinalIgnoreCase))
               ?? activas.FirstOrDefault(p => p.OrigenTipo is null);
    }
}

public sealed record DatosPlantillaAsiento(SentidoContable Sentido, string? OrigenTipo = null, string? Nombre = null, string? Concepto = null, string? Diario = null,
    IReadOnlyList<LineaPlantillaAsiento>? Lineas = null, bool Activa = true);

public sealed record PlantillaAsientoDto(Guid Id, string Sentido, string? OrigenTipo, string Nombre, string? Concepto, string? Diario, bool Activa,
    IReadOnlyList<LineaPlantillaAsiento> Lineas)
{
    public static PlantillaAsientoDto Desde(PlantillaAsiento p) => new(p.Id, p.Sentido.ToString(), p.OrigenTipo, p.Nombre, p.Concepto, p.Diario, p.Activa, p.Lineas);
}

/// <summary>Papel de un apunte con la cuenta y el concepto que se usan si la plantilla no dice otra cosa.</summary>
public sealed record PapelPorDefectoDto(string Papel, string Nombre, string? Cuenta, string Concepto);

/// <summary>Lo que hace cada sentido sin plantilla: sus papeles con la cuenta y el concepto de siempre.</summary>
public sealed record EsquemaAsientoDto(string Sentido, string Concepto, IReadOnlyList<PapelPorDefectoDto> Papeles);

/// <summary>
/// Plantillas de asiento de la empresa: una por sentido y, si se quiere, por origen. Las cuentas deben existir en el
/// plan y el diario, en los diarios de la empresa.
/// </summary>
public sealed class GestionPlantillasAsiento
{
    private readonly IRepositorioPlantillasAsiento _repo;
    private readonly IRepositorioCuentas _cuentas;
    private readonly IUnidadDeTrabajoContabilidad _unidad;
    private readonly IRepositorioDiarios? _diarios;

    public GestionPlantillasAsiento(IRepositorioPlantillasAsiento repo, IRepositorioCuentas cuentas, IUnidadDeTrabajoContabilidad unidad, IRepositorioDiarios? diarios = null)
    {
        _repo = repo;
        _cuentas = cuentas;
        _unidad = unidad;
        _diarios = diarios;
    }

    public static IReadOnlyList<EsquemaAsientoDto> Esquemas() =>
    [
        new("Venta", "{Referencia} · {Tercero}",
        [
            new("Tercero", "Cliente", PlanBasico.CuentaClientes + " (o su subcuenta)", "Concepto del asiento"),
            new("Resultado", "Ingreso", PlanBasico.CuentaVentas + " (o la de la regla por familia o tipo)", "Concepto del asiento"),
            new("IvaRepercutido", "IVA repercutido", PlanBasico.CuentaIvaRepercutido, "IVA repercutido"),
            new("RetencionVenta", "Retención soportada", PlanBasico.CuentaRetencionVenta, "Retención IRPF"),
        ]),
        new("Compra", "{Referencia} · {Tercero}",
        [
            new("Tercero", "Proveedor", PlanBasico.CuentaProveedores + " (o su subcuenta)", "Concepto del asiento"),
            new("Resultado", "Gasto", PlanBasico.CuentaCompras + " (o la de la regla o la línea)", "Concepto del asiento"),
            new("IvaSoportado", "IVA soportado", PlanBasico.CuentaIvaSoportado, "IVA soportado"),
            new("IvaRepercutido", "IVA autoliquidado", PlanBasico.CuentaIvaRepercutido, "IVA autoliquidado (inversión del sujeto pasivo / intracomunitaria)"),
            new("RetencionCompra", "Retención practicada", PlanBasico.CuentaRetencion, "Retención IRPF"),
        ]),
        new("Cobro", "{Referencia} · {Tercero}",
        [
            new("Tesoreria", "Banco o caja", PlanBasico.CuentaBancos + " (o la del banco elegido)", "Concepto del asiento"),
            new("Tercero", "Cliente", PlanBasico.CuentaClientes + " (o su subcuenta)", "Concepto del asiento"),
        ]),
        new("Pago", "{Referencia} · {Tercero}",
        [
            new("Tercero", "Proveedor", PlanBasico.CuentaProveedores + " (o su subcuenta)", "Concepto del asiento"),
            new("Tesoreria", "Banco o caja", PlanBasico.CuentaBancos + " (o la del banco elegido)", "Concepto del asiento"),
        ]),
    ];

    public async Task<IReadOnlyList<PlantillaAsientoDto>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.ListarAsync(empresaId, ct).ConfigureAwait(false)).OrderBy(p => p.Sentido).ThenBy(p => p.OrigenTipo ?? "").Select(PlantillaAsientoDto.Desde).ToList();

    public async Task<Resultado<PlantillaAsientoDto>> GuardarAsync(Guid empresaId, Guid? id, DatosPlantillaAsiento datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var todas = await _repo.ListarAsync(empresaId, ct).ConfigureAwait(false);
        PlantillaAsiento? p;
        if (id is { } existente)
        {
            p = await _repo.ObtenerAsync(existente, ct).ConfigureAwait(false);
            if (p is null || p.EmpresaId != empresaId)
            {
                return Resultado.Fallo<PlantillaAsientoDto>(Error.NoEncontrado("plantilla.no_encontrada", "La plantilla no existe."));
            }

            var r = p.Actualizar(datos.Nombre, datos.Concepto, datos.Diario, datos.Lineas, datos.Activa);
            if (r.EsFallo)
            {
                return Resultado.Fallo<PlantillaAsientoDto>(r.Error);
            }
        }
        else
        {
            var nueva = PlantillaAsiento.Crear(empresaId, datos.Sentido, datos.OrigenTipo, datos.Nombre, datos.Concepto, datos.Diario, datos.Lineas, datos.Activa);
            if (nueva.EsFallo)
            {
                return Resultado.Fallo<PlantillaAsientoDto>(nueva.Error);
            }

            p = nueva.Valor;
            if (todas.Any(x => x.Sentido == p.Sentido && string.Equals(x.OrigenTipo, p.OrigenTipo, StringComparison.OrdinalIgnoreCase)))
            {
                return Resultado.Fallo<PlantillaAsientoDto>(Error.Conflicto("plantilla.repetida",
                    $"Ya hay una plantilla de {p.Sentido.ToString().ToLowerInvariant()}{(p.OrigenTipo is null ? "" : " para " + p.OrigenTipo)}: modifícala."));
            }
        }

        await SembradorPlan.AsegurarAsync(empresaId, _cuentas, ct).ConfigureAwait(false);
        var codigos = await _cuentas.CodigosExistentesAsync(empresaId, ct).ConfigureAwait(false);
        var faltan = p.Lineas.Where(l => l.Cuenta is not null && !codigos.Contains(l.Cuenta)).Select(l => l.Cuenta!).ToList();
        if (faltan.Count > 0)
        {
            return Resultado.Fallo<PlantillaAsientoDto>(Error.Validacion("plantilla.cuenta_inexistente",
                $"Estas cuentas no están en el plan contable: {string.Join(", ", faltan)}. Créalas antes."));
        }

        if (p.Diario is { } diario && _diarios is not null && !await _diarios.ExisteActivoAsync(empresaId, diario, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<PlantillaAsientoDto>(Error.Validacion("plantilla.diario", $"El diario «{diario}» no existe o está de baja."));
        }

        if (id is null)
        {
            _repo.Agregar(p);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PlantillaAsientoDto.Desde(p));
    }

    /// <summary>Borra la plantilla: sus documentos vuelven a contabilizarse como siempre. Los asientos ya hechos no cambian.</summary>
    public async Task<Resultado> EliminarAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        var p = await _repo.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (p is null || p.EmpresaId != empresaId)
        {
            return Resultado.Fallo(Error.NoEncontrado("plantilla.no_encontrada", "La plantilla no existe."));
        }

        _repo.Eliminar(p);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }
}
