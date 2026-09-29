using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Agro.Dominio;

/// <summary>
/// Tara de un tipo de envase (palot, box, caja, palé de madera…) desde una fecha. Es versionada: cuando cambia se da de
/// alta otra versión y la anterior se cierra el día antes, así las pesadas antiguas conservan la tara con que se hicieron.
/// Una versión que ya se ha aplicado en una pesada no cambia de valor ni de fechas.
/// </summary>
public sealed class TaraEnvase : RaizAgregadoEmpresa<Guid>
{
    private TaraEnvase(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private TaraEnvase(Guid id, Guid empresaId, Guid envaseProductoId)
        : base(id, empresaId)
    {
        EnvaseProductoId = envaseProductoId;
    }

    public Guid EnvaseProductoId { get; private set; }

    public decimal TaraKg { get; private set; }

    public DateOnly Desde { get; private set; }

    public DateOnly? Hasta { get; private set; }

    /// <summary>De dónde sale el valor (tara pesada, ficha del fabricante…).</summary>
    public string? Observaciones { get; private set; }

    public bool VigenteEl(DateOnly fecha) => Desde <= fecha && (Hasta is null || fecha <= Hasta);

    public static Resultado<TaraEnvase> Crear(Guid empresaId, Guid envaseProductoId, decimal taraKg, DateOnly desde, DateOnly? hasta, string? observaciones)
    {
        var t = new TaraEnvase(Guid.NewGuid(), empresaId, envaseProductoId);
        var r = t.Actualizar(taraKg, desde, hasta, observaciones);
        return r.EsFallo ? Resultado.Fallo<TaraEnvase>(r.Error) : Resultado.Ok(t);
    }

    public Resultado Actualizar(decimal taraKg, DateOnly desde, DateOnly? hasta, string? observaciones)
    {
        if (taraKg < 0m || decimal.Round(taraKg, 3) != taraKg)
        {
            return Resultado.Fallo(Error.Validacion("tara.kilos", "La tara no puede ser negativa y admite hasta 3 decimales."));
        }

        if (hasta is { } h && h < desde)
        {
            return Resultado.Fallo(Error.Validacion("tara.vigencia", "La vigencia acaba antes de empezar."));
        }

        TaraKg = taraKg;
        Desde = desde;
        Hasta = hasta;
        Observaciones = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim().Length > 200 ? observaciones.Trim()[..200] : observaciones.Trim();
        return Resultado.Ok();
    }

    /// <summary>Cierra la versión el día antes de <paramref name="nuevaDesde"/> (al entrar una versión nueva).</summary>
    public Resultado CerrarAntesDe(DateOnly nuevaDesde)
    {
        if (nuevaDesde <= Desde)
        {
            return Resultado.Fallo(Error.Conflicto("tara.solapada", $"La tara vigente desde el {Desde:dd/MM/yyyy} empieza el mismo día o después: cambia esa versión."));
        }

        Hasta = nuevaDesde.AddDays(-1);
        return Resultado.Ok();
    }
}
