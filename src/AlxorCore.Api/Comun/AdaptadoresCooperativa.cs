using AlxorCore.Agro.Aplicacion;
using AlxorCore.Agro.Dominio;
using AlxorCore.Contabilidad.Aplicacion;
using AlxorCore.Cooperativa.Aplicacion;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Api.Comun;

/// <summary>
/// Adaptador de <see cref="IActividadSocios"/> sobre agro: la actividad de cada tercero son sus liquidaciones emitidas con
/// fecha en el periodo (kilos y base). Sin agricultores, no hay actividad.
/// </summary>
public sealed class ActividadSociosAgro : IActividadSocios
{
    private readonly IRepositorioAgro _agro;

    public ActividadSociosAgro(IRepositorioAgro agro) => _agro = agro;

    public async Task<IReadOnlyDictionary<Guid, ActividadTercero>> PorTerceroAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default)
    {
        var proveedores = (await _agro.AgricultoresAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(a => a.Id, a => a.ProveedorId);
        return (await _agro.LiquidacionesAsync(empresaId, null, ct).ConfigureAwait(false))
            .Where(l => l.Estado == EstadoLiquidacion.Emitida && l.Fecha >= desde && l.Fecha <= hasta && proveedores.ContainsKey(l.AgricultorId))
            .GroupBy(l => proveedores[l.AgricultorId])
            .ToDictionary(g => g.Key, g => new ActividadTercero(g.Sum(l => l.Kilos), g.Sum(l => l.BaseImponible)));
    }
}

/// <summary>Adaptador de <see cref="IContabilidadCooperativa"/>: asientos de capital y de reparto, y su contraasiento al anular.</summary>
public sealed class ContabilidadCooperativa : IContabilidadCooperativa
{
    private readonly CrearAsiento _crear;
    private readonly AnularAsiento _anular;

    public ContabilidadCooperativa(CrearAsiento crear, AnularAsiento anular)
    {
        _crear = crear;
        _anular = anular;
    }

    public async Task<Resultado<Guid>> RegistrarAsync(Guid empresaId, DateOnly fecha, string concepto, IReadOnlyList<ApunteCooperativa> apuntes, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(apuntes);
        var r = await _crear.EjecutarAsync(empresaId, new CrearAsientoComando(fecha, concepto,
            apuntes.Select(a => new LineaAsientoComando(a.Cuenta, a.Debe, a.Haber, a.Concepto)).ToList()), ct).ConfigureAwait(false);
        return r.EsFallo ? Resultado.Fallo<Guid>(r.Error) : Resultado.Ok(r.Valor.Id);
    }

    public async Task<Resultado> AnularAsync(Guid empresaId, Guid asientoId, DateOnly fecha, CancellationToken ct = default)
    {
        var r = await _anular.EjecutarAsync(empresaId, asientoId, fecha, ct).ConfigureAwait(false);
        return r.EsFallo ? Resultado.Fallo(r.Error) : Resultado.Ok();
    }
}
