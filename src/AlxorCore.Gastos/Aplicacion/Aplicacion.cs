using AlxorCore.Gastos.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Consultas;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Aplicacion.Modelos;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Gastos.Aplicacion;

public sealed record LineaGastoDto(string? Descripcion, string? CuentaGasto, decimal Base, string CodigoIva, decimal PorcentajeIva, decimal Cuota, bool Autoliquidada,
    decimal PorcentajeRecargo, decimal CuotaRecargo, decimal PorcentajeDeducible, decimal CuotaDeducible);

/// <summary>Bases y cuotas de la factura por tipo de impuesto (lo que usan los libros, el 303, el 390 y el SII).</summary>
public sealed record DesgloseIvaDto(string CodigoIva, decimal PorcentajeIva, decimal Base, decimal Cuota, decimal CuotaDeducible, decimal CuotaRecargo, bool Autoliquidada);

/// <summary>Vista de un gasto.</summary>
public sealed record GastoDto(
    Guid Id, Guid? ProveedorId, string? ProveedorTexto, string Concepto, DateOnly Fecha,
    decimal BaseImponible, string CodigoIva, decimal PorcentajeIva, decimal CuotaIva,
    decimal PorcentajeIrpf, decimal RetencionIrpf, decimal Total, string Estado, string? AvisoRiesgo = null,
    Guid? ActividadNegocioId = null, AfectacionIva Afectacion = AfectacionIva.Comun,
    string? NumeroFactura = null, DateOnly? FechaFactura = null, decimal RecargoTotal = 0m,
    IReadOnlyList<LineaGastoDto>? Lineas = null, IReadOnlyList<VencimientoGasto>? Vencimientos = null, IReadOnlyList<DesgloseIvaDto>? Desglose = null,
    bool EsRectificativa = false, Guid? RectificaGastoId = null, string? NumeroRectificado = null, DateOnly? FechaRectificada = null, string? MotivoRectificacion = null)
{
    public static GastoDto Desde(Gasto g) => new(
        g.Id, g.ProveedorId, g.ProveedorTexto, g.Concepto, g.Fecha, g.BaseImponible, g.CodigoIva, g.PorcentajeIva, g.CuotaIva,
        g.PorcentajeIrpf, g.RetencionIrpf, g.Total, g.Estado.ToString(), ActividadNegocioId: g.ActividadNegocioId, Afectacion: g.Afectacion,
        NumeroFactura: g.NumeroFactura, FechaFactura: g.FechaFactura, RecargoTotal: g.RecargoTotal,
        Lineas: g.Lineas.OrderBy(l => l.Orden).Select(l => new LineaGastoDto(l.Descripcion, l.CuentaGasto, l.Base, l.CodigoIva, l.PorcentajeIva, l.Cuota, l.Autoliquidada,
            l.PorcentajeRecargo, l.CuotaRecargo, l.PorcentajeDeducible, l.CuotaDeducible)).ToList(),
        Vencimientos: g.Vencimientos.OrderBy(v => v.Fecha).ToList(),
        Desglose: DesgloseDe(g),
        EsRectificativa: g.EsRectificativa, RectificaGastoId: g.RectificaGastoId, NumeroRectificado: g.NumeroRectificado, FechaRectificada: g.FechaRectificada,
        MotivoRectificacion: g.MotivoRectificacion);

    /// <summary>Desglose por tipo. Un gasto antiguo sin líneas sale con una sola, la de su cabecera.</summary>
    public static IReadOnlyList<DesgloseIvaDto> DesgloseDe(Gasto g) =>
        g.Lineas.Count == 0
            ? [new DesgloseIvaDto(g.CodigoIva, g.PorcentajeIva, g.BaseImponible, g.CuotaIva, g.CuotaIva, 0m, false)]
            : g.Lineas.GroupBy(l => (l.CodigoIva, l.PorcentajeIva, l.Autoliquidada))
                .Select(x => new DesgloseIvaDto(x.Key.CodigoIva, x.Key.PorcentajeIva, Redondeo.Dos(x.Sum(l => l.Base)), Redondeo.Dos(x.Sum(l => l.Cuota)),
                    Redondeo.Dos(x.Sum(l => l.CuotaDeducible)), Redondeo.Dos(x.Sum(l => l.CuotaRecargo)), x.Key.Autoliquidada))
                .OrderByDescending(d => d.Base).ToList();

    /// <summary>Desglose, también para los DTO construidos a mano (sin líneas).</summary>
    public IReadOnlyList<DesgloseIvaDto> DesgloseIva => Desglose is { Count: > 0 } ? Desglose : [new DesgloseIvaDto(CodigoIva, PorcentajeIva, BaseImponible, CuotaIva, CuotaIva, 0m, false)];
}

/// <summary>Repositorio de gastos (escritura).</summary>
public interface IRepositorioGastos
{
    Task<Gasto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    void Agregar(Gasto gasto);

    /// <summary>¿Hay otra factura viva del proveedor con ese número en ese año?</summary>
    Task<bool> ExisteFacturaAsync(Guid empresaId, Guid proveedorId, string numero, int anio, Guid? excluirId, CancellationToken ct = default) => Task.FromResult(false);
}

/// <summary>
/// Filtros de búsqueda de gastos en servidor (todos opcionales). <paramref name="Texto"/> busca en el
/// concepto y en el texto libre del proveedor.
/// </summary>
public sealed record FiltroGastos(
    string? Texto = null,
    string? Estado = null,
    DateOnly? Desde = null,
    DateOnly? Hasta = null,
    decimal? ImporteMin = null,
    decimal? ImporteMax = null,
    Guid? ProveedorId = null);

/// <summary>Consultas de lectura de gastos (las usan la API, Tesorería e Informes).</summary>
public interface IConsultaGastos
{
    Task<GastoDto?> ObtenerAsync(Guid gastoId, CancellationToken ct = default);

    Task<IReadOnlyList<GastoDto>> ListarAsync(Guid empresaId, CancellationToken ct = default);

    /// <summary>Búsqueda paginada y filtrada de gastos (el filtrado ocurre en la base de datos).</summary>
    Task<PaginaResultado<GastoDto>> BuscarAsync(Guid empresaId, FiltroGastos filtro, Paginacion paginacion, CancellationToken ct = default);
}

/// <summary>Unidad de trabajo del módulo Gastos.</summary>
public interface IUnidadDeTrabajoGastos : IUnidadDeTrabajo;

/// <summary>Datos para registrar un gasto.</summary>
public sealed record RegistrarGastoComando(
    string Concepto,
    decimal BaseImponible,
    Guid? ProveedorId = null,
    string? ProveedorTexto = null,
    string? CodigoIva = null,
    decimal PorcentajeIrpf = 0m,
    DateOnly? Fecha = null,
    Guid? FormaPagoId = null,
    Guid? ActividadNegocioId = null,
    AfectacionIva? Afectacion = null,
    string? NumeroFactura = null,
    DateOnly? FechaFactura = null,
    IReadOnlyList<LineaGastoComando>? Lineas = null,
    IReadOnlyList<VencimientoGasto>? Vencimientos = null,
    bool RecargoEquivalencia = false,
    Guid? RectificaGastoId = null,
    string? NumeroRectificado = null,
    DateOnly? FechaRectificada = null,
    string? MotivoRectificacion = null);

/// <summary>
/// Línea de una factura recibida. <see cref="PorcentajeIva"/> solo hace falta en inversión del sujeto pasivo e
/// intracomunitarias (el tipo que se autoliquida); en el resto sale del tipo de impuesto.
/// </summary>
public sealed record LineaGastoComando(
    decimal Base,
    string? CodigoIva = null,
    string? Descripcion = null,
    decimal? PorcentajeIva = null,
    decimal PorcentajeDeducible = 100m,
    string? CuentaGasto = null);

/// <summary>Caso de uso: registrar un gasto. Si se indica un proveedor, se copia su nombre.</summary>
public sealed class RegistrarGasto
{
    private readonly IRepositorioGastos _gastos;
    private readonly IConsultaProveedores _proveedores;
    private readonly IUnidadDeTrabajoGastos _unidadDeTrabajo;
    private readonly EncolarSalidaGastos _encolarSalida;
    private readonly DespacharSalidaGastos _despacharSalida;
    private readonly IConsultaFormasPago _formasPago;
    private readonly IPagosAutomaticos _pagos;
    private readonly IConsultaRiesgo _riesgo;
    private readonly IConsultaEmpresas _empresas;
    private readonly IReloj _reloj;
    private readonly AlxorCore.Catalogo.Aplicacion.IResolverIvaEmpresa? _resolverIva;

    public RegistrarGasto(
        IRepositorioGastos gastos,
        IConsultaProveedores proveedores,
        IUnidadDeTrabajoGastos unidadDeTrabajo,
        EncolarSalidaGastos encolarSalida,
        DespacharSalidaGastos despacharSalida,
        IConsultaFormasPago formasPago,
        IPagosAutomaticos pagos,
        IConsultaRiesgo riesgo,
        IConsultaEmpresas empresas,
        IReloj reloj,
        AlxorCore.Catalogo.Aplicacion.IResolverIvaEmpresa? resolverIva = null)
    {
        _resolverIva = resolverIva;
        _gastos = gastos;
        _proveedores = proveedores;
        _unidadDeTrabajo = unidadDeTrabajo;
        _encolarSalida = encolarSalida;
        _despacharSalida = despacharSalida;
        _formasPago = formasPago;
        _pagos = pagos;
        _riesgo = riesgo;
        _empresas = empresas;
        _reloj = reloj;
    }

    public Task<Resultado<GastoDto>> EjecutarAsync(Guid empresaId, RegistrarGastoComando comando, CancellationToken ct = default) =>
        EjecutarInternoAsync(empresaId, comando, false, ct);

    /// <summary>Calcula la factura tal como se registraría (líneas, impuestos, retención, total y vencimientos) sin guardarla.</summary>
    public Task<Resultado<GastoDto>> SimularAsync(Guid empresaId, RegistrarGastoComando comando, CancellationToken ct = default) =>
        EjecutarInternoAsync(empresaId, comando, true, ct);

    /// <summary>Resuelve las líneas del comando con el catálogo de impuestos de la empresa.</summary>
    internal static async Task<Resultado<List<NuevaLineaGasto>>> ResolverLineasAsync(
        Guid empresaId, RegistrarGastoComando comando, AlxorCore.Catalogo.Aplicacion.IResolverIvaEmpresa? resolverIva, IConsultaEmpresas empresas, CancellationToken ct)
    {
        var empresa = await empresas.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        var impuestoEmpresa = empresa?.ImpuestoIndirecto ?? TipoImpuesto.Iva;
        // Comerciante minorista en recargo de equivalencia: no deduce el IVA soportado (ni el recargo), todo es coste.
        var enRecargo = empresa?.RegimenIva == AlxorCore.Organizacion.Dominio.RegimenIva.RecargoEquivalencia;
        var general = impuestoEmpresa == TipoImpuesto.Igic ? Impuesto.IgicGeneral : Impuesto.IvaGeneral;
        var lineas = comando.Lineas is { Count: > 0 } ? comando.Lineas : [new LineaGastoComando(comando.BaseImponible, comando.CodigoIva)];
        var resultado = new List<NuevaLineaGasto>();
        foreach (var (l, i) in lineas.Select((l, i) => (l, i + 1)))
        {
            var codigo = string.IsNullOrWhiteSpace(l.CodigoIva) ? general.Codigo : l.CodigoIva.Trim().ToUpperInvariant();
            AlxorCore.Catalogo.Aplicacion.IvaResuelto? iva = resolverIva is null ? null : await resolverIva.ResolverAsync(empresaId, codigo, ct).ConfigureAwait(false);
            if (iva is null)
            {
                var estatal = Impuesto.PorCodigoImpuesto(codigo);
                if (estatal.EsFallo)
                {
                    return Resultado.Fallo<List<NuevaLineaGasto>>(Error.Validacion(estatal.Error.Codigo, $"Línea {i}: {estatal.Error.Mensaje}"));
                }

                iva = new AlxorCore.Catalogo.Aplicacion.IvaResuelto(estatal.Valor.Codigo, estatal.Valor.Porcentaje, 0m, AlxorCore.Catalogo.Dominio.ClaseIva.Ordinario,
                    estatal.Valor.Porcentaje, null, estatal.Valor.Tipo);
            }

            var autoliquidada = iva.Clase is AlxorCore.Catalogo.Dominio.ClaseIva.InversionSujetoPasivo or AlxorCore.Catalogo.Dominio.ClaseIva.Intracomunitario;
            var sinCuota = !autoliquidada && iva.Clase != AlxorCore.Catalogo.Dominio.ClaseIva.Ordinario;
            var porcentaje = autoliquidada
                ? l.PorcentajeIva ?? (iva.Porcentaje > 0m ? iva.Porcentaje : (iva.Impuesto == TipoImpuesto.Igic ? Impuesto.IgicGeneral : Impuesto.IvaGeneral).Porcentaje)
                : iva.Porcentaje;
            var recargo = comando.RecargoEquivalencia && iva.Clase == AlxorCore.Catalogo.Dominio.ClaseIva.Ordinario ? iva.RecargoEquivalencia : 0m;
            resultado.Add(new NuevaLineaGasto(l.Descripcion, l.Base, iva.Codigo, porcentaje, iva.Impuesto, autoliquidada, sinCuota, recargo,
                enRecargo && !autoliquidada ? 0m : l.PorcentajeDeducible, l.CuentaGasto));
        }

        return Resultado.Ok(resultado);
    }

    /// <summary>
    /// Datos de rectificación del comando: si indica el gasto rectificado, tiene que ser del mismo proveedor y estar vivo, y
    /// se copian su número y su fecha. Null si no es rectificativa.
    /// </summary>
    internal static async Task<Resultado<DatosRectificacion?>> RectificacionAsync(RegistrarGastoComando comando, IRepositorioGastos gastos, Guid? excluirId, CancellationToken ct)
    {
        if (comando.RectificaGastoId is null && string.IsNullOrWhiteSpace(comando.NumeroRectificado))
        {
            return Resultado.Ok<DatosRectificacion?>(null);
        }

        if (comando.RectificaGastoId is not { } id)
        {
            return Resultado.Ok<DatosRectificacion?>(new DatosRectificacion(null, comando.NumeroRectificado, comando.FechaRectificada, comando.MotivoRectificacion));
        }

        var original = await gastos.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (original is null || original.Id == excluirId)
        {
            return Resultado.Fallo<DatosRectificacion?>(Error.Validacion("gasto.rectificada_no_encontrada", "La factura que se rectifica no existe."));
        }

        if (original.Estado != EstadoGasto.Registrado)
        {
            return Resultado.Fallo<DatosRectificacion?>(Error.Conflicto("gasto.rectificada_anulada", "No se rectifica una factura anulada."));
        }

        if (original.ProveedorId != comando.ProveedorId)
        {
            return Resultado.Fallo<DatosRectificacion?>(Error.Validacion("gasto.rectificada_proveedor", "La rectificativa tiene que ser del mismo proveedor que la factura rectificada."));
        }

        return Resultado.Ok<DatosRectificacion?>(new DatosRectificacion(id, original.NumeroFactura ?? original.Concepto, original.FechaFactura ?? original.Fecha, comando.MotivoRectificacion));
    }

    /// <summary>Documento a contabilizar de un gasto (con sus líneas si tiene más de una o alguna especial).</summary>
    internal static DocumentoContabilizable Documento(Gasto g, string? tipoTercero, bool anulacion)
    {
        var especial = g.Lineas.Count > 1 || g.Lineas.Any(l => l.Autoliquidada || l.CuotaRecargo != 0m || l.PorcentajeDeducible != 100m || l.CuentaGasto is not null);
        var referencia = anulacion ? $"Anulación: {g.NumeroFactura ?? g.Concepto}" : g.NumeroFactura is null ? g.Concepto : $"Fra. {g.NumeroFactura}";
        return new DocumentoContabilizable(
            SentidoContable.Compra, (anulacion ? "AnulacionGasto" : "Gasto") + (g.Revision == 0 ? string.Empty : $"#{g.Revision}"), g.Id, referencia.Length > 80 ? referencia[..80] : referencia, g.ProveedorId, g.ProveedorTexto ?? g.Concepto,
            g.Fecha, g.BaseImponible, g.CodigoIva, g.CuotaIva, g.PorcentajeIrpf, g.RetencionIrpf, g.Total, TipoTercero: tipoTercero,
            Afectacion: g.Afectacion.ToString(), ActividadNegocioId: g.ActividadNegocioId, Anulacion: anulacion,
            Lineas: especial ? g.Lineas.Select(l => new LineaContable(l.Base, l.CodigoIva, l.Cuota, l.CuotaDeducible, l.CuotaRecargo, l.Autoliquidada, l.CuentaGasto)).ToList() : null);
    }

    private async Task<Resultado<GastoDto>> EjecutarInternoAsync(Guid empresaId, RegistrarGastoComando comando, bool simular, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var proveedorTexto = comando.ProveedorTexto;
        string? tipoTercero = null;
        Guid? formaPagoDefectoId = null;
        decimal? limiteRiesgo = null;
        Guid? proveedorRiesgoId = null;
        Guid? actividadNegocioId = null;
        if (comando.ProveedorId is { } provId)
        {
            var proveedor = await _proveedores.ObtenerAsync(provId, ct).ConfigureAwait(false);
            if (proveedor is null)
            {
                return Resultado.Fallo<GastoDto>(Error.NoEncontrado("proveedor.no_encontrado", "El proveedor no existe."));
            }

            proveedorTexto = proveedor.Nombre;
            tipoTercero = proveedor.Tipo;
            formaPagoDefectoId = proveedor.FormaPagoDefectoId;
            limiteRiesgo = proveedor.LimiteRiesgo;
            proveedorRiesgoId = provId;
            actividadNegocioId = proveedor.ActividadNegocioId;
        }

        var formaPagoId = comando.FormaPagoId ?? formaPagoDefectoId;
        FormaPagoDto? formaPago = formaPagoId is { } fpid
            ? await _formasPago.ObtenerAsync(fpid, ct).ConfigureAwait(false)
            : null;

        var fecha = comando.Fecha ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);

        // Líneas con el impuesto del catálogo de la empresa (sin tipo, el general: IVA 21 % o IGIC 7 % en Canarias).
        var lineas = await ResolverLineasAsync(empresaId, comando, _resolverIva, _empresas, ct).ConfigureAwait(false);
        if (lineas.EsFallo)
        {
            return Resultado.Fallo<GastoDto>(lineas.Error);
        }

        var fechaFactura = comando.FechaFactura;
        var vencimiento = (fechaFactura ?? fecha).AddDays(formaPago is { GeneraVencimiento: true } ? formaPago.DiasVencimiento : 0);
        var rectificacion = await RectificacionAsync(comando, _gastos, null, ct).ConfigureAwait(false);
        if (rectificacion.EsFallo)
        {
            return Resultado.Fallo<GastoDto>(rectificacion.Error);
        }

        var gasto = Gasto.RegistrarFactura(empresaId, comando.ProveedorId, proveedorTexto, comando.Concepto, comando.NumeroFactura, fechaFactura, fecha,
            lineas.Valor, comando.PorcentajeIrpf, comando.Vencimientos, vencimiento, _reloj, rectificacion.Valor);
        if (gasto.EsFallo)
        {
            return Resultado.Fallo<GastoDto>(gasto.Error);
        }

        // Una factura del proveedor no se registra dos veces (mismo número en el mismo año).
        if (comando.ProveedorId is { } provDup && gasto.Valor.NumeroFactura is { } numero
            && await _gastos.ExisteFacturaAsync(empresaId, provDup, numero, (fechaFactura ?? fecha).Year, null, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<GastoDto>(Error.Conflicto("gasto.factura_duplicada", $"La factura {numero} de {proveedorTexto} ya está registrada."));
        }

        if (comando.Afectacion is { } afectacion)
        {
            gasto.Valor.EstablecerAfectacion(afectacion, _reloj);
        }

        // La actividad se hereda del proveedor, salvo que se indique una en el comando (el acceso del
        // usuario a esa actividad lo valida la capa de API antes de llegar aquí).
        gasto.Valor.EstablecerActividad(comando.ActividadNegocioId ?? actividadNegocioId);

        // Control de riesgo del proveedor (antes de guardar). Configurable por empresa: avisar o bloquear.
        string? avisoRiesgo = null;
        if (limiteRiesgo is { } limite && proveedorRiesgoId is { } provRiesgoId)
        {
            var riesgoVivo = await _riesgo.RiesgoVivoProveedorAsync(empresaId, provRiesgoId, ct).ConfigureAwait(false);
            var total = gasto.Valor.Total;
            if (riesgoVivo + total > limite)
            {
                var emp = await _empresas.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
                if ((emp?.ControlRiesgo ?? ControlRiesgo.Aviso) == ControlRiesgo.Bloqueo && !simular)
                {
                    return Resultado.Fallo<GastoDto>(Error.Conflicto("riesgo.superado",
                        $"El proveedor supera su límite de riesgo ({limite:F2} €): riesgo vivo {riesgoVivo:F2} € + este gasto {total:F2} €."));
                }

                avisoRiesgo = $"El proveedor supera su límite de riesgo ({limite:F2} €). Riesgo tras este gasto: {riesgoVivo + total:F2} €.";
            }
        }

        if (simular)
        {
            return Resultado.Ok(GastoDto.Desde(gasto.Valor) with { AvisoRiesgo = avisoRiesgo });
        }

        // Bandeja de salida (outbox): la contabilización del gasto se encola en la MISMA transacción que
        // el gasto, garantizando atomicidad (ni gasto sin contabilizar, ni al revés).
        var g = gasto.Valor;
        _encolarSalida.Contabilizacion(empresaId, Documento(g, tipoTercero, false));

        _gastos.Agregar(g);
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        await _despacharSalida.EjecutarAsync(ct: ct).ConfigureAwait(false);

        // Forma de pago «ya pagada»: registra el pago total en el acto (no genera vencimiento abierto).
        if (formaPago?.RegistrarPagoAutomatico == true)
        {
            await _pagos.RegistrarPagoTotalAsync(empresaId, g.Id, g.Total, g.Fecha, ct).ConfigureAwait(false);
        }

        return Resultado.Ok(GastoDto.Desde(g) with { AvisoRiesgo = avisoRiesgo });
    }
}

/// <summary>
/// Caso de uso: anular un gasto. Deja de contar en los libros y autoliquidaciones y, si se había
/// encolado su contabilización, se encola el contraasiento (en la misma transacción, por la bandeja de
/// salida). No comprueba los pagos: lo hace quien lo invoca (los gastos que nacen de otro documento, como
/// una liquidación agrícola, se anulan desde ese documento).
/// </summary>
public sealed class AnularGasto
{
    private readonly IRepositorioGastos _gastos;
    private readonly IConsultaProveedores _proveedores;
    private readonly IUnidadDeTrabajoGastos _unidadDeTrabajo;
    private readonly EncolarSalidaGastos _encolarSalida;
    private readonly DespacharSalidaGastos _despacharSalida;
    private readonly IReloj _reloj;

    public AnularGasto(IRepositorioGastos gastos, IConsultaProveedores proveedores, IUnidadDeTrabajoGastos unidadDeTrabajo,
        EncolarSalidaGastos encolarSalida, DespacharSalidaGastos despacharSalida, IReloj reloj)
    {
        _gastos = gastos;
        _proveedores = proveedores;
        _unidadDeTrabajo = unidadDeTrabajo;
        _encolarSalida = encolarSalida;
        _despacharSalida = despacharSalida;
        _reloj = reloj;
    }

    public async Task<Resultado> EjecutarAsync(Guid gastoId, CancellationToken ct = default)
    {
        var g = await _gastos.ObtenerPorIdAsync(gastoId, ct).ConfigureAwait(false);
        if (g is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("gasto.no_encontrado", "El gasto no existe."));
        }

        var r = g.Anular(_reloj);
        if (r.EsFallo)
        {
            return r;
        }

        var tipoTercero = g.ProveedorId is { } p ? (await _proveedores.ObtenerAsync(p, ct).ConfigureAwait(false))?.Tipo : null;
        _encolarSalida.Contabilizacion(g.EmpresaId, RegistrarGasto.Documento(g, tipoTercero, true));

        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        await _despacharSalida.EjecutarAsync(ct: ct).ConfigureAwait(false);
        return Resultado.Ok();
    }
}

/// <summary>
/// Caso de uso: corregir una factura recibida ya registrada (quien lo llama comprueba que no tenga pagos). Se encola el
/// contraasiento de lo registrado y el asiento de lo nuevo, en la misma transacción.
/// </summary>
public sealed class ModificarGasto
{
    private readonly IRepositorioGastos _gastos;
    private readonly IConsultaProveedores _proveedores;
    private readonly IUnidadDeTrabajoGastos _unidad;
    private readonly EncolarSalidaGastos _encolar;
    private readonly DespacharSalidaGastos _despachar;
    private readonly IConsultaEmpresas _empresas;
    private readonly IConsultaFormasPago _formasPago;
    private readonly IReloj _reloj;
    private readonly AlxorCore.Catalogo.Aplicacion.IResolverIvaEmpresa? _resolverIva;

    public ModificarGasto(IRepositorioGastos gastos, IConsultaProveedores proveedores, IUnidadDeTrabajoGastos unidad, EncolarSalidaGastos encolar, DespacharSalidaGastos despachar,
        IConsultaEmpresas empresas, IConsultaFormasPago formasPago, IReloj reloj, AlxorCore.Catalogo.Aplicacion.IResolverIvaEmpresa? resolverIva = null)
    {
        _gastos = gastos;
        _proveedores = proveedores;
        _unidad = unidad;
        _encolar = encolar;
        _despachar = despachar;
        _empresas = empresas;
        _formasPago = formasPago;
        _reloj = reloj;
        _resolverIva = resolverIva;
    }

    public async Task<Resultado<GastoDto>> EjecutarAsync(Guid gastoId, RegistrarGastoComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var g = await _gastos.ObtenerPorIdAsync(gastoId, ct).ConfigureAwait(false);
        if (g is null)
        {
            return Resultado.Fallo<GastoDto>(Error.NoEncontrado("gasto.no_encontrado", "El gasto no existe."));
        }

        var proveedorTexto = comando.ProveedorTexto;
        string? tipoTercero = null;
        Guid? formaPagoDefecto = null;
        if (comando.ProveedorId is { } provId)
        {
            var proveedor = await _proveedores.ObtenerAsync(provId, ct).ConfigureAwait(false);
            if (proveedor is null)
            {
                return Resultado.Fallo<GastoDto>(Error.NoEncontrado("proveedor.no_encontrado", "El proveedor no existe."));
            }

            proveedorTexto = proveedor.Nombre;
            tipoTercero = proveedor.Tipo;
            formaPagoDefecto = proveedor.FormaPagoDefectoId;
        }

        var lineas = await RegistrarGasto.ResolverLineasAsync(g.EmpresaId, comando, _resolverIva, _empresas, ct).ConfigureAwait(false);
        if (lineas.EsFallo)
        {
            return Resultado.Fallo<GastoDto>(lineas.Error);
        }

        var anterior = RegistrarGasto.Documento(g, g.ProveedorId is { } pa ? (await _proveedores.ObtenerAsync(pa, ct).ConfigureAwait(false))?.Tipo : null, true);
        var fecha = comando.Fecha ?? g.Fecha;
        var formaPago = (comando.FormaPagoId ?? formaPagoDefecto) is { } fp ? await _formasPago.ObtenerAsync(fp, ct).ConfigureAwait(false) : null;
        var vencimiento = (comando.FechaFactura ?? fecha).AddDays(formaPago is { GeneraVencimiento: true } ? formaPago.DiasVencimiento : 0);
        var rectificacion = await RegistrarGasto.RectificacionAsync(comando, _gastos, g.Id, ct).ConfigureAwait(false);
        if (rectificacion.EsFallo)
        {
            return Resultado.Fallo<GastoDto>(rectificacion.Error);
        }

        var r = g.Modificar(comando.ProveedorId, proveedorTexto, comando.Concepto, comando.NumeroFactura, comando.FechaFactura, fecha, lineas.Valor,
            comando.PorcentajeIrpf, comando.Vencimientos, vencimiento, _reloj, rectificacion.Valor);
        if (r.EsFallo)
        {
            return Resultado.Fallo<GastoDto>(r.Error);
        }

        if (comando.ProveedorId is { } provDup && g.NumeroFactura is { } numero
            && await _gastos.ExisteFacturaAsync(g.EmpresaId, provDup, numero, (g.FechaFactura ?? g.Fecha).Year, g.Id, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<GastoDto>(Error.Conflicto("gasto.factura_duplicada", $"La factura {numero} de {proveedorTexto} ya está registrada."));
        }

        if (comando.Afectacion is { } afectacion)
        {
            g.EstablecerAfectacion(afectacion, _reloj);
        }

        _encolar.Contabilizacion(g.EmpresaId, anterior);
        _encolar.Contabilizacion(g.EmpresaId, RegistrarGasto.Documento(g, tipoTercero, false));
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        await _despachar.EjecutarAsync(ct: ct).ConfigureAwait(false);
        return Resultado.Ok(GastoDto.Desde(g));
    }
}

/// <summary>Caso de uso: cambiar la afectación de un gasto a efectos de la prorrata especial.</summary>
public sealed class CambiarAfectacionGasto
{
    private readonly IRepositorioGastos _gastos;
    private readonly IUnidadDeTrabajoGastos _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public CambiarAfectacionGasto(IRepositorioGastos gastos, IUnidadDeTrabajoGastos unidadDeTrabajo, IReloj reloj)
    {
        _gastos = gastos;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<Resultado<GastoDto>> EjecutarAsync(Guid gastoId, AfectacionIva afectacion, CancellationToken ct = default)
    {
        if (!Enum.IsDefined(afectacion))
        {
            return Resultado.Fallo<GastoDto>(Error.Validacion("gasto.afectacion", "La afectación debe ser Comun, ConDerecho o SinDerecho."));
        }

        var gasto = await _gastos.ObtenerPorIdAsync(gastoId, ct).ConfigureAwait(false);
        if (gasto is null)
        {
            return Resultado.Fallo<GastoDto>(Error.NoEncontrado("gasto.no_encontrado", "El gasto no existe."));
        }

        gasto.EstablecerAfectacion(afectacion, _reloj);
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(GastoDto.Desde(gasto));
    }
}

/// <summary>Caso de uso: listar los gastos de la empresa activa.</summary>
public sealed class ListarGastos
{
    private readonly IConsultaGastos _consulta;

    public ListarGastos(IConsultaGastos consulta) => _consulta = consulta;

    public Task<IReadOnlyList<GastoDto>> EjecutarAsync(Guid empresaId, CancellationToken ct = default) =>
        _consulta.ListarAsync(empresaId, ct);
}

/// <summary>Caso de uso: buscar gastos con filtros y paginación (en servidor).</summary>
public sealed class BuscarGastos
{
    private readonly IConsultaGastos _consulta;

    public BuscarGastos(IConsultaGastos consulta) => _consulta = consulta;

    public Task<PaginaResultado<GastoDto>> EjecutarAsync(Guid empresaId, FiltroGastos filtro, Paginacion paginacion, CancellationToken ct = default) =>
        _consulta.BuscarAsync(empresaId, filtro, paginacion, ct);
}

/// <summary>Caso de uso: obtener un gasto.</summary>
public sealed class ObtenerGasto
{
    private readonly IConsultaGastos _consulta;

    public ObtenerGasto(IConsultaGastos consulta) => _consulta = consulta;

    public async Task<Resultado<GastoDto>> EjecutarAsync(Guid gastoId, CancellationToken ct = default)
    {
        var gasto = await _consulta.ObtenerAsync(gastoId, ct).ConfigureAwait(false);
        return gasto is null
            ? Resultado.Fallo<GastoDto>(Error.NoEncontrado("gasto.no_encontrado", "El gasto no existe."))
            : Resultado.Ok(gasto);
    }
}
