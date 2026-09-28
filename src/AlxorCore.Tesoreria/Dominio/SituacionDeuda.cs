using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Tesoreria.Dominio;

/// <summary>Cuentas del PGC con que se lleva la situación de la deuda de clientes.</summary>
public static class CuentasDeuda
{
    /// <summary>Efectos comerciales en cartera (los efectos nuevos de una renovación).</summary>
    public const string EnCartera = "4310";

    /// <summary>Efectos comerciales impagados (recibos devueltos, si la empresa lo elige).</summary>
    public const string Impagados = "4315";

    /// <summary>Clientes de dudoso cobro.</summary>
    public const string Dudoso = "436";

    /// <summary>Deterioro de valor de créditos por operaciones comerciales (la provisión).</summary>
    public const string Deterioro = "490";

    /// <summary>Pérdidas por deterioro de créditos por operaciones comerciales (la dotación).</summary>
    public const string Dotacion = "694";

    /// <summary>Reversión del deterioro de créditos por operaciones comerciales.</summary>
    public const string Reversion = "794";

    /// <summary>Pérdidas de créditos comerciales incobrables.</summary>
    public const string Incobrables = "650";

    /// <summary>Ingresos de la renovación de efectos (intereses y gastos repercutidos al cliente).</summary>
    public const string IngresosRenovacion = "769";

    public const string MetodoRenovacion = "Renovación";

    public const string MetodoIncobrable = "Incobrable";
}

/// <summary>Calidad de la deuda, como <c>ClasificacionDocumentoCobro</c> de Hispatec.</summary>
public enum ClasificacionDeuda
{
    SinClasificar,
    Dudoso,
    Precontencioso,
    Contencioso,
    Moroso,
}

/// <summary>Configuración de la cartera de cobros de la empresa.</summary>
public sealed class ConfiguracionCartera : RaizAgregadoEmpresa<Guid>
{
    private ConfiguracionCartera(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private ConfiguracionCartera(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
    }

    /// <summary>Un recibo devuelto pasa de la cuenta del cliente a efectos impagados (4315).</summary>
    public bool ImpagadosA4315 { get; private set; }

    /// <summary>Porcentaje que se carga al cliente al renovar un efecto si no se indican los gastos (<c>PorcRenovEfectos</c>).</summary>
    public decimal PorcentajeRenovacion { get; private set; }

    public static ConfiguracionCartera Crear(Guid empresaId) => new(Guid.NewGuid(), empresaId);

    public Resultado Cambiar(bool impagadosA4315, decimal porcentajeRenovacion)
    {
        if (porcentajeRenovacion is < 0m or > 100m)
        {
            return Resultado.Fallo(Error.Validacion("cartera.porcentaje", "El porcentaje de renovación va de 0 a 100."));
        }

        ImpagadosA4315 = impagadosA4315;
        PorcentajeRenovacion = Redondeo.Dos(porcentajeRenovacion);
        return Resultado.Ok();
    }
}

/// <summary>
/// Dónde está la deuda de una factura o un efecto cuando ha salido de su cuenta de origen (la del cliente o la del
/// efecto): en efectos impagados (4315) o en clientes de dudoso cobro (436), por qué importe y con qué dotación (490).
/// Al cobrarse, lo cobrado vuelve primero a la cuenta de origen (y la dotación se revierte en proporción), de modo que el
/// cobro de siempre la cancela.
/// </summary>
public sealed class SituacionDeuda : RaizAgregadoEmpresa<Guid>
{
    private SituacionDeuda(Guid id)
        : base(id, Guid.Empty)
    {
        Documento = null!;
        TerceroNombre = null!;
    }

    private SituacionDeuda(Guid id, Guid empresaId, TipoDocumentoTesoreria tipo, Guid documentoId, string documento, Guid? terceroId, string terceroNombre,
        string? cuentaOrigen)
        : base(id, empresaId)
    {
        TipoDocumento = tipo;
        DocumentoId = documentoId;
        Documento = documento;
        TerceroId = terceroId;
        TerceroNombre = terceroNombre;
        CuentaOrigen = cuentaOrigen;
    }

    public TipoDocumentoTesoreria TipoDocumento { get; private set; }

    public Guid DocumentoId { get; private set; }

    public string Documento { get; private set; }

    public Guid? TerceroId { get; private set; }

    public string TerceroNombre { get; private set; }

    /// <summary>Cuenta de origen de la deuda: null, la del cliente; si no, la del efecto (4310…).</summary>
    public string? CuentaOrigen { get; private set; }

    /// <summary>Cuenta donde está ahora (null: en la de origen).</summary>
    public string? Cuenta { get; private set; }

    /// <summary>Importe que está en <see cref="Cuenta"/>.</summary>
    public decimal Importe { get; private set; }

    public ClasificacionDeuda Clasificacion { get; private set; }

    public DateOnly? FechaClasificacion { get; private set; }

    /// <summary>Deterioro dotado vivo (490).</summary>
    public decimal Dotado { get; private set; }

    public DateOnly? IncobrableEl { get; private set; }

    public static SituacionDeuda Crear(Guid empresaId, TipoDocumentoTesoreria tipo, Guid documentoId, string documento, Guid? terceroId, string terceroNombre,
        string? cuentaOrigen) =>
        new(Guid.NewGuid(), empresaId, tipo, documentoId, documento, terceroId, terceroNombre, cuentaOrigen);

    /// <summary>
    /// En la misma cuenta, suma <paramref name="importe"/>; en otra, la deuda pasa entera a ella con ese importe total.
    /// Devuelve la cuenta en que estaba (null: la de origen).
    /// </summary>
    public string? Mover(string cuenta, decimal importe)
    {
        var desde = Cuenta;
        Importe = Redondeo.Dos(Cuenta == cuenta ? Importe + importe : importe);
        Cuenta = cuenta;
        return desde;
    }

    /// <summary>Lo cobrado sale de la cuenta en que estaba; devuelve el importe y la dotación que se revierte.</summary>
    public (decimal Importe, decimal Dotacion) Regularizar(decimal cobrado)
    {
        if (Cuenta is null || Importe <= 0m)
        {
            return (0m, 0m);
        }

        var importe = Math.Min(Importe, cobrado);
        var dotacion = Importe == 0m ? 0m : Redondeo.Dos(Dotado * importe / Importe);
        Importe = Redondeo.Dos(Importe - importe);
        Dotado = Redondeo.Dos(Dotado - dotacion);
        return (importe, dotacion);
    }

    /// <summary>Se anuló un cobro que había regularizado: la deuda vuelve a su cuenta con su dotación.</summary>
    public void Deshacer(string cuenta, decimal importe, decimal dotacion)
    {
        Cuenta = cuenta;
        Importe = Redondeo.Dos(Importe + importe);
        Dotado = Redondeo.Dos(Dotado + dotacion);
    }

    public Resultado Clasificar(ClasificacionDeuda clasificacion, DateOnly fecha)
    {
        if (!Enum.IsDefined(clasificacion) || clasificacion == ClasificacionDeuda.SinClasificar)
        {
            return Resultado.Fallo(Error.Validacion("deuda.clasificacion", "Clasificación no válida: Dudoso, Precontencioso, Contencioso o Moroso."));
        }

        Clasificacion = clasificacion;
        FechaClasificacion = fecha;
        return Resultado.Ok();
    }

    /// <summary>Fija la dotación: devuelve el ajuste (+ dotar, − revertir).</summary>
    public Resultado<decimal> Dotar(decimal objetivo)
    {
        objetivo = Redondeo.Dos(objetivo);
        if (Cuenta != CuentasDeuda.Dudoso)
        {
            return Resultado.Fallo<decimal>(Error.Conflicto("deuda.no_dudosa", "Solo se dota el deterioro de una deuda clasificada como dudosa."));
        }

        if (objetivo < 0m || objetivo > Importe)
        {
            return Resultado.Fallo<decimal>(Error.Validacion("deuda.dotacion", $"La dotación va de 0 a lo pendiente ({Redondeo.Formatear(Importe)} €)."));
        }

        var ajuste = Redondeo.Dos(objetivo - Dotado);
        Dotado = objetivo;
        return Resultado.Ok(ajuste);
    }

    /// <summary>Sale de dudoso: vuelve a la cuenta de origen; devuelve el importe y la dotación que se revierte.</summary>
    public (decimal Importe, decimal Dotacion) Desclasificar()
    {
        var r = (Importe, Dotado);
        Cuenta = null;
        Importe = 0m;
        Dotado = 0m;
        Clasificacion = ClasificacionDeuda.SinClasificar;
        FechaClasificacion = null;
        return r;
    }

    public void MarcarIncobrable(DateOnly fecha) => IncobrableEl = fecha;
}

/// <summary>Lo que un cobro sacó de la cuenta de la deuda (para deshacerlo si se anula el cobro).</summary>
public sealed class RegularizacionDeuda : RaizAgregadoEmpresa<Guid>
{
    private RegularizacionDeuda(Guid id)
        : base(id, Guid.Empty)
    {
        Cuenta = null!;
    }

    private RegularizacionDeuda(Guid id, Guid empresaId, Guid situacionId, Guid movimientoId, string cuenta, decimal importe, decimal dotacion)
        : base(id, empresaId)
    {
        SituacionId = situacionId;
        MovimientoId = movimientoId;
        Cuenta = cuenta;
        Importe = importe;
        Dotacion = dotacion;
    }

    public Guid SituacionId { get; private set; }

    public Guid MovimientoId { get; private set; }

    public string Cuenta { get; private set; }

    public decimal Importe { get; private set; }

    public decimal Dotacion { get; private set; }

    public bool Deshecha { get; private set; }

    public static RegularizacionDeuda Crear(SituacionDeuda s, Guid movimientoId, string cuenta, decimal importe, decimal dotacion)
    {
        ArgumentNullException.ThrowIfNull(s);
        return new RegularizacionDeuda(Guid.NewGuid(), s.EmpresaId, s.Id, movimientoId, cuenta, importe, dotacion);
    }

    public void Deshacer() => Deshecha = true;
}

/// <summary>
/// Renovación de un efecto o factura pendiente (Hispatec: renovaciones): el documento se da por cobrado contra efectos
/// en cartera (4310) y se crean uno o varios efectos nuevos con sus vencimientos, que suman lo pendiente más los gastos
/// e intereses que se cargan al cliente (a 769).
/// </summary>
public sealed class RenovacionEfecto : RaizAgregadoEmpresa<Guid>
{
    private RenovacionEfecto(Guid id)
        : base(id, Guid.Empty)
    {
        Documento = null!;
        TerceroNombre = null!;
    }

    private RenovacionEfecto(Guid id, Guid empresaId, TipoDocumentoTesoreria tipo, Guid documentoId, string documento, Guid? terceroId, string terceroNombre,
        DateOnly fecha, decimal importe, decimal gastos, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        TipoDocumento = tipo;
        DocumentoId = documentoId;
        Documento = documento;
        TerceroId = terceroId;
        TerceroNombre = terceroNombre;
        Fecha = fecha;
        Importe = importe;
        Gastos = gastos;
        CreadaEn = ahora;
    }

    public TipoDocumentoTesoreria TipoDocumento { get; private set; }

    public Guid DocumentoId { get; private set; }

    public string Documento { get; private set; }

    public Guid? TerceroId { get; private set; }

    public string TerceroNombre { get; private set; }

    public DateOnly Fecha { get; private set; }

    /// <summary>Pendiente que se renueva.</summary>
    public decimal Importe { get; private set; }

    /// <summary>Gastos e intereses cargados al cliente.</summary>
    public decimal Gastos { get; private set; }

    /// <summary>Cobro que cancela el documento renovado.</summary>
    public Guid MovimientoId { get; private set; }

    public DateTimeOffset CreadaEn { get; private set; }

    public DateTimeOffset? AnuladaEn { get; private set; }

    public static Resultado<RenovacionEfecto> Crear(Guid empresaId, TipoDocumentoTesoreria tipo, Guid documentoId, string documento, Guid? terceroId, string terceroNombre,
        DateOnly fecha, decimal importe, decimal gastos, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (tipo == TipoDocumentoTesoreria.Gasto)
        {
            return Resultado.Fallo<RenovacionEfecto>(Error.Validacion("renovacion.tipo", "Solo se renuevan facturas o efectos a cobrar."));
        }

        if (importe <= 0m || gastos < 0m)
        {
            return Resultado.Fallo<RenovacionEfecto>(Error.Validacion("renovacion.importe", "No hay nada pendiente que renovar."));
        }

        return Resultado.Ok(new RenovacionEfecto(Guid.NewGuid(), empresaId, tipo, documentoId, documento, terceroId, terceroNombre, fecha, Redondeo.Dos(importe),
            Redondeo.Dos(gastos), reloj.AhoraUtc));
    }

    public void AsignarMovimiento(Guid movimientoId) => MovimientoId = movimientoId;

    public void Anular(IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        AnuladaEn = reloj.AhoraUtc;
    }
}
