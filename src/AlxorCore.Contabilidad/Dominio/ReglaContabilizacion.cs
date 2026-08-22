using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Contabilidad.Dominio;

/// <summary>
/// Regla que elige la cuenta contable de resultado (ingreso 7xx en ventas, gasto 6xx en compras) de un
/// documento según su <b>familia de artículo</b> y/o el <b>tipo de tercero</b>. La regla más específica
/// gana: familia + tipo (3) &gt; familia (2) &gt; tipo (1) &gt; genérica por defecto (0, sin regla).
/// </summary>
public sealed class ReglaContabilizacion : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMaximaClave = 80;
    public const int LongitudMaximaCuenta = 10;

    private ReglaContabilizacion(Guid id)
        : base(id, Guid.Empty)
    {
        CuentaCodigo = null!;
    }

    private ReglaContabilizacion(Guid id, Guid empresaId, SentidoContable sentido, string? familia, string? tipoTercero, string cuentaCodigo)
        : base(id, empresaId)
    {
        Sentido = sentido;
        Familia = familia;
        TipoTercero = tipoTercero;
        CuentaCodigo = cuentaCodigo;
    }

    public SentidoContable Sentido { get; private set; }

    /// <summary>Familia de artículo a la que aplica (null = cualquiera).</summary>
    public string? Familia { get; private set; }

    /// <summary>Tipo de tercero (cliente/proveedor) al que aplica (null = cualquiera).</summary>
    public string? TipoTercero { get; private set; }

    /// <summary>Cuenta contable de resultado a usar cuando la regla encaja.</summary>
    public string CuentaCodigo { get; private set; }

    /// <summary>Especificidad: familia (2) + tipo (1). A mayor valor, más prioritaria.</summary>
    public int Especificidad => (Familia is not null ? 2 : 0) + (TipoTercero is not null ? 1 : 0);

    public static Resultado<ReglaContabilizacion> Crear(Guid empresaId, SentidoContable sentido, string? familia, string? tipoTercero, string? cuentaCodigo)
    {
        var error = Validar(familia, tipoTercero, ref cuentaCodigo);
        if (error is not null)
        {
            return Resultado.Fallo<ReglaContabilizacion>(error);
        }

        return Resultado.Ok(new ReglaContabilizacion(Guid.NewGuid(), empresaId, sentido, Limpiar(familia), Limpiar(tipoTercero), cuentaCodigo!));
    }

    public Resultado Actualizar(SentidoContable sentido, string? familia, string? tipoTercero, string? cuentaCodigo)
    {
        var error = Validar(familia, tipoTercero, ref cuentaCodigo);
        if (error is not null)
        {
            return Resultado.Fallo(error);
        }

        Sentido = sentido;
        Familia = Limpiar(familia);
        TipoTercero = Limpiar(tipoTercero);
        CuentaCodigo = cuentaCodigo!;
        return Resultado.Ok();
    }

    private static Error? Validar(string? familia, string? tipoTercero, ref string? cuentaCodigo)
    {
        cuentaCodigo = string.IsNullOrWhiteSpace(cuentaCodigo) ? null : cuentaCodigo.Trim();
        if (cuentaCodigo is null)
        {
            return Error.Validacion("regla.cuenta_vacia", "La regla necesita una cuenta contable.");
        }

        if (cuentaCodigo.Length > LongitudMaximaCuenta)
        {
            return Error.Validacion("regla.cuenta_larga", "El código de cuenta es demasiado largo.");
        }

        if (Limpiar(familia) is null && Limpiar(tipoTercero) is null)
        {
            return Error.Validacion("regla.sin_criterio", "La regla debe indicar familia, tipo de tercero, o ambos.");
        }

        return null;
    }

    private static string? Limpiar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        var limpio = valor.Trim();
        return limpio.Length > LongitudMaximaClave ? limpio[..LongitudMaximaClave] : limpio;
    }
}
