using AlxorCore.Cooperativa.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Cooperativa.Aplicacion;

public interface IUnidadDeTrabajoCooperativa : IUnidadDeTrabajo
{
    /// <summary>Bloqueo transaccional (hasta guardar) para numerar socios y actas sin huecos ni cruces.</summary>
    Task BloquearAsync(string clave, CancellationToken ct = default);
}

public interface IRepositorioCooperativa
{
    Task<ConfiguracionCooperativa?> ConfiguracionAsync(Guid empresaId, CancellationToken ct = default);

    void Agregar(ConfiguracionCooperativa configuracion);

    Task<IReadOnlyList<Socio>> SociosAsync(Guid empresaId, CancellationToken ct = default);

    Task<Socio?> SocioAsync(Guid id, CancellationToken ct = default);

    Task<int> UltimoNumeroSocioAsync(Guid empresaId, CancellationToken ct = default);

    void Agregar(Socio socio);

    void Eliminar(Socio socio);

    Task<IReadOnlyList<MovimientoCapital>> MovimientosAsync(Guid empresaId, Guid? socioId, CancellationToken ct = default);

    Task<MovimientoCapital?> MovimientoAsync(Guid id, CancellationToken ct = default);

    void Agregar(MovimientoCapital movimiento);

    Task<IReadOnlyList<Reparto>> RepartosAsync(Guid empresaId, CancellationToken ct = default);

    Task<Reparto?> RepartoAsync(Guid id, CancellationToken ct = default);

    void Agregar(Reparto reparto);

    void Eliminar(Reparto reparto);

    Task<IReadOnlyList<Acta>> ActasAsync(Guid empresaId, CancellationToken ct = default);

    Task<Acta?> ActaAsync(Guid id, CancellationToken ct = default);

    Task<int> UltimoNumeroActaAsync(Guid empresaId, OrganoSocial organo, CancellationToken ct = default);

    void Agregar(Acta acta);

    void Eliminar(Acta acta);
}

/// <summary>Actividad de un socio (su tercero) en unas fechas: kilos entregados e importe liquidado.</summary>
public sealed record ActividadTercero(decimal Kilos, decimal Importe);

/// <summary>
/// Lo que cada tercero entregó a la entidad en unas fechas. Lo implementa la API sobre las liquidaciones emitidas de
/// agro; sin agro no hay actividad y el reparto se hace con la actividad que se indique a mano.
/// </summary>
public interface IActividadSocios
{
    Task<IReadOnlyDictionary<Guid, ActividadTercero>> PorTerceroAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default);
}

/// <summary>Un apunte de un asiento de la cooperativa.</summary>
public sealed record ApunteCooperativa(string Cuenta, decimal Debe, decimal Haber, string? Concepto = null);

/// <summary>Asientos de las operaciones de capital y de los repartos. Lo implementa la API sobre el módulo de contabilidad.</summary>
public interface IContabilidadCooperativa
{
    Task<Resultado<Guid>> RegistrarAsync(Guid empresaId, DateOnly fecha, string concepto, IReadOnlyList<ApunteCooperativa> apuntes, CancellationToken ct = default);

    /// <summary>Contraasiento del asiento (en la fecha indicada).</summary>
    Task<Resultado> AnularAsync(Guid empresaId, Guid asientoId, DateOnly fecha, CancellationToken ct = default);
}
