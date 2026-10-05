using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Gastos.Dominio;

/// <summary>Estado de un gasto.</summary>
public enum EstadoGasto
{
    Registrado = 1,
    Anulado = 2,
}

/// <summary>Se ha registrado un gasto.</summary>
public sealed record GastoRegistrado(Guid GastoId, Guid EmpresaId, decimal Total, DateTimeOffset OcurridoEn) : IEventoDominio;

/// <summary>Línea nueva de una factura recibida, con su impuesto ya resuelto del catálogo de la empresa.</summary>
public sealed record NuevaLineaGasto(
    string? Descripcion,
    decimal Base,
    string CodigoIva,
    decimal PorcentajeIva,
    TipoImpuesto Impuesto = TipoImpuesto.Iva,
    bool Autoliquidada = false,
    bool SinCuota = false,
    decimal PorcentajeRecargo = 0m,
    decimal PorcentajeDeducible = 100m,
    string? CuentaGasto = null);

/// <summary>
/// Factura rectificativa recibida (abono o cargo del proveedor por diferencias): a qué factura rectifica y por qué. Sus
/// importes son la diferencia, así que un abono va en negativo.
/// </summary>
public sealed record DatosRectificacion(Guid? RectificaGastoId, string? NumeroRectificado, DateOnly? FechaRectificada, string? Motivo);

/// <summary>Vencimiento (plazo de pago) de una factura recibida.</summary>
public sealed record VencimientoGasto(DateOnly Fecha, decimal Importe);

/// <summary>
/// Línea de una factura recibida: base, tipo e impuesto (cuota soportada o, en inversión del sujeto pasivo y
/// adquisiciones intracomunitarias, autoliquidada), recargo de equivalencia, parte deducible (antes de la prorrata) y
/// cuenta de gasto propia.
/// </summary>
public sealed class LineaGasto
{
    private LineaGasto()
    {
        CodigoIva = null!;
    }

    internal LineaGasto(int orden, NuevaLineaGasto d)
    {
        Id = Guid.NewGuid();
        Orden = orden;
        Descripcion = string.IsNullOrWhiteSpace(d.Descripcion) ? null : d.Descripcion.Trim();
        CuentaGasto = string.IsNullOrWhiteSpace(d.CuentaGasto) ? null : d.CuentaGasto.Trim();
        Base = Redondeo.Dos(d.Base);
        CodigoIva = d.CodigoIva;
        PorcentajeIva = Redondeo.Dos(d.PorcentajeIva);
        Autoliquidada = d.Autoliquidada;
        Cuota = d.SinCuota ? 0m : Redondeo.Dos(Base * PorcentajeIva / 100m);
        PorcentajeRecargo = Redondeo.Dos(d.PorcentajeRecargo);
        CuotaRecargo = Redondeo.Dos(Base * PorcentajeRecargo / 100m);
        PorcentajeDeducible = Redondeo.Dos(d.PorcentajeDeducible);
        CuotaDeducible = Redondeo.Dos(Cuota * PorcentajeDeducible / 100m);
    }

    public Guid Id { get; private set; }

    public int Orden { get; private set; }

    public string? Descripcion { get; private set; }

    /// <summary>Cuenta de gasto de la línea (null: la de las reglas de contabilización).</summary>
    public string? CuentaGasto { get; private set; }

    public decimal Base { get; private set; }

    public string CodigoIva { get; private set; }

    public decimal PorcentajeIva { get; private set; }

    /// <summary>Cuota de la línea: la que cobra el proveedor o, si <see cref="Autoliquidada"/>, la que autoliquida la empresa.</summary>
    public decimal Cuota { get; private set; }

    /// <summary>Inversión del sujeto pasivo o adquisición intracomunitaria: la cuota no la cobra el proveedor.</summary>
    public bool Autoliquidada { get; private set; }

    public decimal PorcentajeRecargo { get; private set; }

    public decimal CuotaRecargo { get; private set; }

    /// <summary>Porcentaje deducible de la cuota (p. ej. 50 % de un turismo), antes de la prorrata.</summary>
    public decimal PorcentajeDeducible { get; private set; }

    public decimal CuotaDeducible { get; private set; }
}

/// <summary>
/// Gasto o factura recibida de una empresa: proveedor (de la ficha o texto libre), número y fecha de la factura del
/// proveedor, fecha de registro (contable), líneas con sus bases e impuestos, retención de IRPF y vencimientos. Los
/// totales de cabecera son la suma de las líneas; <see cref="CodigoIva"/> es el de la línea de mayor base.
/// </summary>
public sealed class Gasto : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMaximaConcepto = 200;
    public const int LongitudMaximaNumeroFactura = 60;
    public const decimal IrpfMaximo = 60m;

    private readonly List<LineaGasto> _lineas = [];
    private readonly List<VencimientoGasto> _vencimientos = [];

    private Gasto(Guid id)
        : base(id, Guid.Empty)
    {
        Concepto = null!;
        CodigoIva = null!;
    }

    private Gasto(Guid id, Guid empresaId, Guid? proveedorId, string? proveedorTexto, string concepto, DateOnly fecha, decimal baseImponible, string codigoIva, decimal porcentajeIva, decimal porcentajeIrpf, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        ProveedorId = proveedorId;
        ProveedorTexto = proveedorTexto;
        Concepto = concepto;
        Fecha = fecha;
        BaseImponible = baseImponible;
        CodigoIva = codigoIva;
        PorcentajeIva = porcentajeIva;
        CuotaIva = Redondeo.Dos(baseImponible * porcentajeIva / 100m);
        PorcentajeIrpf = porcentajeIrpf;
        RetencionIrpf = Redondeo.Dos(baseImponible * porcentajeIrpf / 100m);
        Total = Redondeo.Dos(BaseImponible + CuotaIva - RetencionIrpf);
        Estado = EstadoGasto.Registrado;
        CreadoEn = ahora;
        ActualizadoEn = ahora;
    }

    /// <summary>Proveedor asociado (opcional; permite gastos rápidos sin proveedor fijo).</summary>
    public Guid? ProveedorId { get; private set; }

    /// <summary>Nombre del proveedor (copia del proveedor asociado, o texto libre).</summary>
    public string? ProveedorTexto { get; private set; }

    /// <summary>
    /// Actividad de negocio del proveedor en el momento de registrar (snapshot). Permite segmentar
    /// las compras/gastos por línea/división de negocio en los informes. Null = sin actividad.
    /// </summary>
    public Guid? ActividadNegocioId { get; private set; }

    /// <summary>Centro de la empresa al que corresponde la factura recibida (null: sin centro).</summary>
    public Guid? CentroId { get; private set; }

    public void AsignarCentro(Guid? centroId) => CentroId = centroId == Guid.Empty ? null : centroId;

    public string Concepto { get; private set; }

    /// <summary>Fecha de registro (la del asiento y la del periodo de IVA en que se deduce).</summary>
    public DateOnly Fecha { get; private set; }

    /// <summary>Número de la factura del proveedor (null en gastos sin factura, como un ticket).</summary>
    public string? NumeroFactura { get; private set; }

    /// <summary>Fecha de expedición de la factura del proveedor (null: la de registro).</summary>
    public DateOnly? FechaFactura { get; private set; }

    /// <summary>Recargo de equivalencia soportado (suma de las líneas).</summary>
    public decimal RecargoTotal { get; private set; }

    public IReadOnlyList<LineaGasto> Lineas => _lineas;

    /// <summary>Si es una rectificativa del proveedor (abono o cargo por diferencias).</summary>
    public bool EsRectificativa { get; private set; }

    /// <summary>Gasto que rectifica (si está registrado aquí).</summary>
    public Guid? RectificaGastoId { get; private set; }

    /// <summary>Número y fecha de la factura rectificada (copia, para los libros y el SII).</summary>
    public string? NumeroRectificado { get; private set; }

    public DateOnly? FechaRectificada { get; private set; }

    public string? MotivoRectificacion { get; private set; }

    /// <summary>Número de correcciones: cada versión de la factura tiene su asiento (y su contraasiento al cambiarla).</summary>
    public int Revision { get; private set; }

    public IReadOnlyList<VencimientoGasto> Vencimientos => _vencimientos;

    public decimal BaseImponible { get; private set; }

    public string CodigoIva { get; private set; }

    public decimal PorcentajeIva { get; private set; }

    public decimal CuotaIva { get; private set; }

    public decimal PorcentajeIrpf { get; private set; }

    public decimal RetencionIrpf { get; private set; }

    public decimal Total { get; private set; }

    public EstadoGasto Estado { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset ActualizadoEn { get; private set; }

    public static Resultado<Gasto> Registrar(
        Guid empresaId, Guid? proveedorId, string? proveedorTexto, string? concepto, DateOnly fecha, decimal baseImponible, string? codigoIva, decimal porcentajeIrpf, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        if (string.IsNullOrWhiteSpace(concepto))
        {
            return Resultado.Fallo<Gasto>(Error.Validacion("gasto.concepto_vacio", "El concepto es obligatorio."));
        }

        if (concepto.Trim().Length > LongitudMaximaConcepto)
        {
            return Resultado.Fallo<Gasto>(Error.Validacion("gasto.concepto_largo", "El concepto es demasiado largo."));
        }

        if (baseImponible < 0)
        {
            return Resultado.Fallo<Gasto>(Error.Validacion("gasto.base_negativa", "La base no puede ser negativa."));
        }

        if (porcentajeIrpf is < 0 or > IrpfMaximo)
        {
            return Resultado.Fallo<Gasto>(Error.Validacion("gasto.irpf_invalido", "El porcentaje de IRPF no es válido."));
        }

        var impuesto = Impuesto.PorCodigoImpuesto(string.IsNullOrWhiteSpace(codigoIva) ? Impuesto.IvaGeneral.Codigo : codigoIva);
        if (impuesto.EsFallo)
        {
            return Resultado.Fallo<Gasto>(impuesto.Error);
        }

        var gasto = new Gasto(
            Guid.NewGuid(), empresaId, proveedorId, Normalizar(proveedorTexto), concepto.Trim(), fecha, Redondeo.Dos(baseImponible),
            impuesto.Valor.Codigo, impuesto.Valor.Porcentaje, porcentajeIrpf, reloj.AhoraUtc);
        gasto._lineas.Add(new LineaGasto(1, new NuevaLineaGasto(null, gasto.BaseImponible, impuesto.Valor.Codigo, impuesto.Valor.Porcentaje, impuesto.Valor.Tipo)));
        gasto._vencimientos.Add(new VencimientoGasto(fecha, gasto.Total));
        gasto.RegistrarEvento(new GastoRegistrado(gasto.Id, empresaId, gasto.Total, reloj.AhoraUtc));
        return Resultado.Ok(gasto);
    }

    /// <summary>
    /// Registra una factura recibida con sus líneas. Sin vencimientos, uno por el total en <paramref name="vencimientoPorDefecto"/>.
    /// </summary>
    public static Resultado<Gasto> RegistrarFactura(
        Guid empresaId, Guid? proveedorId, string? proveedorTexto, string? concepto, string? numeroFactura, DateOnly? fechaFactura, DateOnly fecha,
        IReadOnlyList<NuevaLineaGasto> lineas, decimal porcentajeIrpf, IReadOnlyList<VencimientoGasto>? vencimientos, DateOnly vencimientoPorDefecto, IReloj reloj,
        DatosRectificacion? rectificacion = null)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        var gasto = new Gasto(Guid.NewGuid(), empresaId, proveedorId, Normalizar(proveedorTexto), string.Empty, fecha, 0m, "IVA0", 0m, 0m, reloj.AhoraUtc);
        var r = gasto.EstablecerRectificacion(rectificacion);
        if (r.EsCorrecto)
        {
            r = gasto.Establecer(concepto, numeroFactura, fechaFactura, fecha, lineas, porcentajeIrpf, vencimientos, vencimientoPorDefecto);
        }

        if (r.EsFallo)
        {
            return Resultado.Fallo<Gasto>(r.Error);
        }

        gasto.RegistrarEvento(new GastoRegistrado(gasto.Id, empresaId, gasto.Total, reloj.AhoraUtc));
        return Resultado.Ok(gasto);
    }

    /// <summary>Cambia los datos de una factura registrada (quien lo llama comprueba que no tenga pagos).</summary>
    public Resultado Modificar(
        Guid? proveedorId, string? proveedorTexto, string? concepto, string? numeroFactura, DateOnly? fechaFactura, DateOnly fecha,
        IReadOnlyList<NuevaLineaGasto> lineas, decimal porcentajeIrpf, IReadOnlyList<VencimientoGasto>? vencimientos, DateOnly vencimientoPorDefecto, IReloj reloj,
        DatosRectificacion? rectificacion = null)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (Estado != EstadoGasto.Registrado)
        {
            return Resultado.Fallo(Error.Conflicto("gasto.anulado", "Un gasto anulado no se modifica."));
        }

        var r = EstablecerRectificacion(rectificacion);
        if (r.EsCorrecto)
        {
            r = Establecer(concepto, numeroFactura, fechaFactura, fecha, lineas, porcentajeIrpf, vencimientos, vencimientoPorDefecto);
        }

        if (r.EsFallo)
        {
            return r;
        }

        ProveedorId = proveedorId;
        ProveedorTexto = Normalizar(proveedorTexto);
        Revision++;
        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    private Resultado EstablecerRectificacion(DatosRectificacion? r)
    {
        if (r is null)
        {
            EsRectificativa = false;
            RectificaGastoId = null;
            NumeroRectificado = null;
            FechaRectificada = null;
            MotivoRectificacion = null;
            return Resultado.Ok();
        }

        var numero = Normalizar(r.NumeroRectificado);
        var motivo = Normalizar(r.Motivo);
        if (numero is null || motivo is null)
        {
            return Resultado.Fallo(Error.Validacion("gasto.rectificativa", "Una rectificativa necesita el número de la factura que rectifica y el motivo."));
        }

        if (numero.Length > LongitudMaximaNumeroFactura || motivo.Length > LongitudMaximaConcepto)
        {
            return Resultado.Fallo(Error.Validacion("gasto.rectificativa", "El número rectificado o el motivo son demasiado largos."));
        }

        EsRectificativa = true;
        RectificaGastoId = r.RectificaGastoId;
        NumeroRectificado = numero;
        FechaRectificada = r.FechaRectificada;
        MotivoRectificacion = motivo;
        return Resultado.Ok();
    }

    private Resultado Establecer(
        string? concepto, string? numeroFactura, DateOnly? fechaFactura, DateOnly fecha, IReadOnlyList<NuevaLineaGasto> lineas, decimal porcentajeIrpf,
        IReadOnlyList<VencimientoGasto>? vencimientos, DateOnly vencimientoPorDefecto)
    {
        ArgumentNullException.ThrowIfNull(lineas);
        var numero = Normalizar(numeroFactura);
        var textoConcepto = Normalizar(concepto) ?? (numero is null ? null : $"Factura {numero}");
        if (textoConcepto is null)
        {
            return Resultado.Fallo(Error.Validacion("gasto.concepto_vacio", "Indica el número de factura o un concepto."));
        }

        if (textoConcepto.Length > LongitudMaximaConcepto || numero is { Length: > LongitudMaximaNumeroFactura })
        {
            return Resultado.Fallo(Error.Validacion("gasto.concepto_largo", "El concepto o el número de factura es demasiado largo."));
        }

        if (lineas.Count == 0)
        {
            return Resultado.Fallo(Error.Validacion("gasto.sin_lineas", "La factura necesita al menos una línea."));
        }

        if (lineas.Select(l => l.Impuesto).Distinct().Count() > 1)
        {
            return Resultado.Fallo(Error.Validacion("gasto.impuestos_mezclados", "Una factura no mezcla IVA e IGIC."));
        }

        foreach (var (l, i) in lineas.Select((l, i) => (l, i + 1)))
        {
            if (l.PorcentajeIva < 0m || l.PorcentajeRecargo < 0m || l.PorcentajeDeducible is < 0m or > 100m)
            {
                return Resultado.Fallo(Error.Validacion("gasto.linea", $"Línea {i}: los porcentajes no son válidos (deducible entre 0 y 100)."));
            }
        }

        if (porcentajeIrpf is < 0 or > IrpfMaximo)
        {
            return Resultado.Fallo(Error.Validacion("gasto.irpf_invalido", "El porcentaje de IRPF no es válido."));
        }

        if (fechaFactura is { } ff && ff > fecha)
        {
            return Resultado.Fallo(Error.Validacion("gasto.fecha_factura", "La factura no puede ser posterior a la fecha de registro."));
        }

        var nuevas = lineas.Select((l, i) => new LineaGasto(i + 1, l)).ToList();
        var baseTotal = Redondeo.Dos(nuevas.Sum(l => l.Base));
        if (baseTotal < 0m && !EsRectificativa)
        {
            return Resultado.Fallo(Error.Validacion("gasto.base_negativa", "La base no puede ser negativa: un abono del proveedor se registra como rectificativa."));
        }

        var cuota = Redondeo.Dos(nuevas.Sum(l => l.Cuota));
        var cobrada = Redondeo.Dos(nuevas.Where(l => !l.Autoliquidada).Sum(l => l.Cuota));
        var recargo = Redondeo.Dos(nuevas.Sum(l => l.CuotaRecargo));
        var retencion = Redondeo.Dos(baseTotal * porcentajeIrpf / 100m);
        var total = Redondeo.Dos(baseTotal + cobrada + recargo - retencion);

        var plazos = (vencimientos is { Count: > 0 } ? vencimientos : [new VencimientoGasto(vencimientoPorDefecto, total)])
            .Select(v => new VencimientoGasto(v.Fecha, Redondeo.Dos(v.Importe))).OrderBy(v => v.Fecha).ToList();
        if (plazos.Sum(v => v.Importe) != total)
        {
            return Resultado.Fallo(Error.Validacion("gasto.vencimientos", $"Los vencimientos suman {plazos.Sum(v => v.Importe):F2} y la factura {total:F2}."));
        }

        var principal = nuevas.OrderByDescending(l => Math.Abs(l.Base)).First();
        Concepto = textoConcepto;
        NumeroFactura = numero;
        FechaFactura = fechaFactura;
        Fecha = fecha;
        BaseImponible = baseTotal;
        CuotaIva = cuota;
        RecargoTotal = recargo;
        CodigoIva = principal.CodigoIva;
        PorcentajeIva = principal.PorcentajeIva;
        PorcentajeIrpf = Redondeo.Dos(porcentajeIrpf);
        RetencionIrpf = retencion;
        Total = total;
        _lineas.Clear();
        _lineas.AddRange(nuevas);
        _vencimientos.Clear();
        _vencimientos.AddRange(plazos);
        return Resultado.Ok();
    }

    /// <summary>
    /// A qué operaciones se destina lo comprado, a efectos de la prorrata especial: de uso común
    /// (por defecto), solo a operaciones con derecho a deducir o solo a operaciones exentas sin derecho.
    /// </summary>
    public AfectacionIva Afectacion { get; private set; } = AfectacionIva.Comun;

    /// <summary>Fija la afectación del gasto (solo cuenta si la empresa aplica la prorrata especial).</summary>
    public void EstablecerAfectacion(AfectacionIva afectacion, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        Afectacion = Enum.IsDefined(afectacion) ? afectacion : AfectacionIva.Comun;
        ActualizadoEn = reloj.AhoraUtc;
    }

    /// <summary>Clasifica el gasto en una actividad de negocio (snapshot del proveedor). Null/vacío = sin actividad.</summary>
    public void EstablecerActividad(Guid? actividadNegocioId) =>
        ActividadNegocioId = actividadNegocioId is { } a && a != Guid.Empty ? a : null;

    /// <summary>Anula el gasto: deja de contar en los libros de IVA y en las autoliquidaciones.</summary>
    public Resultado Anular(IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (Estado == EstadoGasto.Anulado)
        {
            return Resultado.Fallo(Error.Conflicto("gasto.anulado", "El gasto ya está anulado."));
        }

        Estado = EstadoGasto.Anulado;
        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    private static string? Normalizar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}

/// <summary>Destino de una compra a efectos de la prorrata especial del IVA/IGIC.</summary>
public enum AfectacionIva
{
    /// <summary>Se usa a la vez en operaciones con y sin derecho a deducción: se aplica el porcentaje de prorrata.</summary>
    Comun = 1,

    /// <summary>Se usa solo en operaciones con derecho a deducción: su cuota se deduce entera.</summary>
    ConDerecho = 2,

    /// <summary>Se usa solo en operaciones exentas sin derecho a deducción: su cuota no se deduce.</summary>
    SinDerecho = 3,
}
