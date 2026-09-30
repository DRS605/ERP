using AlxorCore.Contabilidad.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Contabilidad.Aplicacion;

/// <summary>Vista de un documento pendiente de contabilizar (panel del contable).</summary>
public sealed record DocumentoPendienteDto(
    Guid Id, string Sentido, string OrigenTipo, Guid OrigenId, string Referencia,
    Guid? TerceroId, string TerceroNombre, DateOnly FechaDocumento, DateOnly FechaRegistro,
    decimal BaseImponible, decimal CuotaIva, decimal RetencionIrpf, decimal Total,
    string? Familia, string? TipoTercero, string Estado, Guid? AsientoId)
{
    public static DocumentoPendienteDto Desde(DocumentoPendiente d) => new(
        d.Id, d.Sentido.ToString(), d.OrigenTipo, d.OrigenId, d.Referencia, d.TerceroId, d.TerceroNombre,
        d.FechaDocumento, d.FechaRegistro, d.BaseImponible, d.CuotaIva, d.RetencionIrpf, d.Total,
        d.Familia, d.TipoTercero, d.Estado.ToString(), d.AsientoId);
}

/// <summary>Repositorio de la cola de documentos pendientes de contabilizar.</summary>
public interface IRepositorioDocumentosPendientes
{
    void Agregar(DocumentoPendiente documento);
    Task<DocumentoPendiente?> ObtenerAsync(Guid id, CancellationToken ct = default);

    /// <summary>¿Ya existe un pendiente para este documento de origen? (idempotencia de la cola/outbox).</summary>
    Task<bool> ExistePorOrigenAsync(Guid empresaId, string origenTipo, Guid origenId, CancellationToken ct = default);
    Task<IReadOnlyList<DocumentoPendiente>> ListarPendientesAsync(Guid empresaId, CancellationToken ct = default);
    Task<IReadOnlyList<DocumentoPendiente>> ListarAsync(Guid empresaId, CancellationToken ct = default);
}

/// <summary>
/// Resuelve la cuenta de resultado (ingreso 7xx en ventas, gasto 6xx en compras) según la familia
/// del artículo y el tipo de tercero. Implementación básica devuelve la cuenta genérica; el módulo
/// de reglas la sustituye por una que aplica las reglas configuradas.
/// </summary>
public interface IResolverCuentas
{
    Task<string> CuentaResultadoAsync(Guid empresaId, SentidoContable sentido, string? familia, string? tipoTercero, CancellationToken ct = default);

    /// <summary>La cuenta de la regla que encaja, o null si ninguna (para elegir entonces la genérica de la línea).</summary>
    Task<string?> CuentaDeReglaAsync(Guid empresaId, SentidoContable sentido, string? familia, string? tipoTercero, CancellationToken ct = default) =>
        Task.FromResult<string?>(null);
}

/// <summary>Resolutor básico: cuenta genérica de ingresos/gastos, sin reglas.</summary>
public sealed class ResolverCuentasBasico : IResolverCuentas
{
    public Task<string> CuentaResultadoAsync(Guid empresaId, SentidoContable sentido, string? familia, string? tipoTercero, CancellationToken ct = default)
        => Task.FromResult(sentido == SentidoContable.Venta ? PlanBasico.CuentaVentas : PlanBasico.CuentaCompras);
}

/// <summary>Construye y guarda el asiento de partida doble de un documento pendiente.</summary>
/// <summary>
/// Parte deducible de la cuota soportada de una compra. Sin prorrata es la cuota entera; con prorrata,
/// el porcentaje del ejercicio (o la cuota entera / nada en la especial, según la afectación).
/// </summary>
public interface IDeduccionImpuesto
{
    Task<decimal> CuotaDeducibleAsync(Guid empresaId, int ejercicio, decimal cuota, string? afectacion, CancellationToken ct = default);
}

/// <summary>Deducción íntegra (empresa sin prorrata).</summary>
public sealed class DeduccionTotal : IDeduccionImpuesto
{
    public static DeduccionTotal Instancia { get; } = new();

    public Task<decimal> CuotaDeducibleAsync(Guid empresaId, int ejercicio, decimal cuota, string? afectacion, CancellationToken ct = default) =>
        Task.FromResult(cuota);
}

public sealed class PosterDocumento
{
    private readonly IRepositorioAsientos _asientos;
    private readonly IRepositorioCuentas _cuentas;
    private readonly IResolverCuentas _resolver;
    private readonly IUnidadDeTrabajoContabilidad _unidad;
    private readonly IReloj _reloj;

    private readonly IDeduccionImpuesto _deduccion;
    private readonly ImputadorAnalitico? _imputador;
    private readonly IRepositorioPlantillasAsiento? _plantillas;

    public PosterDocumento(IRepositorioAsientos asientos, IRepositorioCuentas cuentas, IResolverCuentas resolver, IUnidadDeTrabajoContabilidad unidad, IReloj reloj,
        IDeduccionImpuesto? deduccion = null, ImputadorAnalitico? imputador = null, IRepositorioPlantillasAsiento? plantillas = null)
    {
        _plantillas = plantillas;
        _asientos = asientos; _cuentas = cuentas; _resolver = resolver; _unidad = unidad; _reloj = reloj;
        _deduccion = deduccion ?? DeduccionTotal.Instancia;
        _imputador = imputador;
    }

    /// <summary>Genera el asiento del documento con su fecha de registro. No guarda (lo hace el llamador).</summary>
    public async Task<Resultado<Asiento>> ConstruirAsync(DocumentoPendiente doc, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(doc);
        var esTesoreria = doc.Sentido is SentidoContable.Cobro or SentidoContable.Pago;
        var cuentaResultado = esTesoreria ? string.Empty
            : await _resolver.CuentaResultadoAsync(doc.EmpresaId, doc.Sentido, doc.Familia, doc.TipoTercero, ct).ConfigureAwait(false);
        // Plantilla del documento: la de su origen o, si no hay, la de su sentido. Sin plantilla, todo como siempre.
        var plantilla = _plantillas is null ? null : await _plantillas.AplicableAsync(doc.EmpresaId, doc.Sentido, doc.OrigenTipo, ct).ConfigureAwait(false);
        string Texto(string t) => PlantillaAsiento.Rellenar(t, doc.Referencia, doc.TerceroNombre, doc.FechaRegistro, doc.Total, doc.OrigenTipo);
        var concepto = plantilla?.Concepto is { } c0 ? Texto(c0) : doc.Referencia + (string.IsNullOrWhiteSpace(doc.TerceroNombre) ? "" : " · " + doc.TerceroNombre);
        if (concepto.Length > PlantillaAsiento.LongitudConcepto)
        {
            concepto = concepto[..PlantillaAsiento.LongitudConcepto];
        }

        if (plantilla?.CuentaDe(PapelApunte.Resultado) is { } resultadoPlantilla
            && cuentaResultado == (doc.Sentido == SentidoContable.Venta ? PlanBasico.CuentaVentas : PlanBasico.CuentaCompras))
        {
            cuentaResultado = resultadoPlantilla;   // solo si ninguna regla por familia o tipo de tercero eligió otra
        }

        // Cuenta del tercero: su subcuenta individual si la tiene asignada; si no, la raíz genérica
        // (430 clientes / 400 proveedores). Así el mayor y el balance muestran el saldo por tercero.
        // Un cobro o un pago puede ir contra otra cuenta (438 anticipos de clientes).
        var cuentaGenerica = doc.Sentido is SentidoContable.Venta or SentidoContable.Cobro ? PlanBasico.CuentaClientes : PlanBasico.CuentaProveedores;
        var cuentaTercero = plantilla?.CuentaDe(PapelApunte.Tercero) ?? cuentaGenerica;
        if (!string.IsNullOrWhiteSpace(doc.CuentaTercero))
        {
            cuentaTercero = doc.CuentaTercero;
        }
        else if (doc.TerceroId is Guid terceroId)
        {
            var sub = await _cuentas.ObtenerPorTerceroAsync(doc.EmpresaId, terceroId, ct).ConfigureAwait(false);
            if (sub is not null)
            {
                cuentaTercero = sub.Codigo;
            }
        }

        // Compras: con prorrata solo es deducible parte de la cuota; la no deducible es más gasto. Con líneas, la
        // prorrata se aplica a la parte que la línea ya deja deducible (p. ej. el 50 % de un turismo).
        var baseProrrata = doc.Sentido == SentidoContable.Compra && doc.Lineas.Count > 0 ? doc.Lineas.Sum(l => l.CuotaDeducible) : doc.CuotaIva;
        // Un abono (rectificativa en negativo) aplica la misma prorrata con el signo cambiado.
        var cuotaDeducible = doc.Sentido != SentidoContable.Compra || baseProrrata == 0m
            ? baseProrrata
            : Math.Sign(baseProrrata) * await _deduccion.CuotaDeducibleAsync(doc.EmpresaId, doc.FechaRegistro.Year, Math.Abs(baseProrrata), doc.Afectacion, ct).ConfigureAwait(false);

        // La plantilla fija la tesorería cuando el documento solo trae la genérica (570/572), no la subcuenta de un banco.
        var tesoreria = string.IsNullOrWhiteSpace(doc.CuentaTesoreria) || doc.CuentaTesoreria is PlanBasico.CuentaBancos or PlanBasico.CuentaCaja
            ? plantilla?.CuentaDe(PapelApunte.Tesoreria) ?? (string.IsNullOrWhiteSpace(doc.CuentaTesoreria) ? PlanBasico.CuentaBancos : doc.CuentaTesoreria)
            : doc.CuentaTesoreria;
        var p = new Papeles(
            papel => plantilla?.CuentaDe(papel),
            (papel, defecto) => plantilla?.ConceptoDe(papel) is { } t ? Texto(t) : defecto,
            concepto);
        // Ventas con líneas: cada una a su cuenta. La propia de la línea; si no, la regla de su familia; si no, la de la
        // plantilla; si no, 700 si es un bien y 705 si es un servicio (sin saberlo, la del documento).
        var ingresos = new List<(string Cuenta, decimal Base)>();
        if (doc.Sentido == SentidoContable.Venta)
        {
            foreach (var l in doc.Lineas)
            {
                var cuenta = !string.IsNullOrWhiteSpace(l.CuentaGasto) ? l.CuentaGasto!
                    : await _resolver.CuentaDeReglaAsync(doc.EmpresaId, doc.Sentido, l.Familia ?? doc.Familia, doc.TipoTercero, ct).ConfigureAwait(false)
                      ?? plantilla?.CuentaDe(PapelApunte.Resultado)
                      ?? (l.EsBien is { } bien ? bien ? PlanBasico.CuentaVentasMercaderias : PlanBasico.CuentaVentas : cuentaResultado);
                ingresos.Add((cuenta, l.Base));
            }
        }

        var lineas = doc.Sentido switch
        {
            SentidoContable.Venta => LineasVenta(doc, cuentaResultado, cuentaTercero, p, ingresos),
            SentidoContable.Compra when doc.Lineas.Count > 0 => LineasCompraDesglosada(doc, cuentaResultado, cuentaTercero, p, cuotaDeducible),
            SentidoContable.Compra => LineasCompra(doc, cuentaResultado, cuentaTercero, p, cuotaDeducible),
            SentidoContable.Cobro => [new LineaAsiento(tesoreria, doc.Total, 0m, p.Concepto(PapelApunte.Tesoreria)), new LineaAsiento(cuentaTercero, 0m, doc.Total, p.Concepto(PapelApunte.Tercero))],
            _ => [new LineaAsiento(cuentaTercero, doc.Total, 0m, p.Concepto(PapelApunte.Tercero)), new LineaAsiento(tesoreria, 0m, doc.Total, p.Concepto(PapelApunte.Tesoreria))],
        };

        // Anulación: el contraasiento, con el debe y el haber cambiados.
        if (doc.Anulacion)
        {
            lineas = lineas.Select(l => l with { Debe = l.Haber, Haber = l.Debe }).ToList();
        }

        // Rectificativas (abonos): un importe negativo en un lado es positivo en el otro; los apuntes a cero sobran.
        lineas = lineas
            .Select(l => l.Debe < 0m || l.Haber < 0m ? l with { Debe = Math.Max(0m, l.Debe) + Math.Max(0m, -l.Haber), Haber = Math.Max(0m, l.Haber) + Math.Max(0m, -l.Debe) } : l)
            .Select(l => l.Debe > 0m && l.Haber > 0m ? l with { Debe = Math.Max(0m, l.Debe - l.Haber), Haber = Math.Max(0m, l.Haber - l.Debe) } : l)
            .Where(l => l.Debe != 0m || l.Haber != 0m)
            .ToList();

        // Mes cerrado: el documento sigue pendiente (se contabiliza con otra fecha de registro o al reabrir el mes).
        if (await PeriodosContables.ComprobarAsync(_asientos, doc.EmpresaId, doc.FechaRegistro, ct).ConfigureAwait(false) is { } cerrado)
        {
            return Resultado.Fallo<Asiento>(cerrado);
        }

        await SembradorPlan.AsegurarAsync(doc.EmpresaId, _cuentas, ct).ConfigureAwait(false);
        var ejercicio = doc.FechaRegistro.Year;
        var numero = await _asientos.SiguienteNumeroAsync(doc.EmpresaId, ejercicio, ct).ConfigureAwait(false);
        var origen = doc.Sentido.ToString();
        return Asiento.Crear(doc.EmpresaId, ejercicio, numero, doc.FechaRegistro, concepto, origen, lineas, _reloj, plantilla?.Diario);
    }

    public void Agregar(Asiento asiento) => _asientos.Agregar(asiento);

    /// <summary>
    /// Añade el asiento del documento y lo imputa en analítica con las reglas (cuenta, tercero,
    /// actividad y familia del documento). Los apuntes sin regla quedan pendientes de imputar.
    /// </summary>
    public async Task AgregarAsync(Asiento asiento, DocumentoPendiente doc, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(asiento);
        ArgumentNullException.ThrowIfNull(doc);
        _asientos.Agregar(asiento);
        if (_imputador is not null)
        {
            await _imputador.ImputarAsync(doc.EmpresaId, ImputadorAnalitico.ApuntesDe(asiento, doc.TerceroId, doc.ActividadNegocioId, doc.Familia),
                OrigenImputacion.Regla, null, ct).ConfigureAwait(false);
        }
    }

    public Task GuardarAsync(CancellationToken ct) => _unidad.GuardarCambiosAsync(ct);

    /// <summary>Cuenta y concepto de cada papel según la plantilla (con los de siempre por defecto).</summary>
    private sealed record Papeles(Func<PapelApunte, string?> CuentaPlantilla, Func<PapelApunte, string, string> ConceptoPlantilla, string ConceptoAsiento)
    {
        public string Cuenta(PapelApunte papel, string defecto) => CuentaPlantilla(papel) ?? defecto;

        public string Concepto(PapelApunte papel, string? defecto = null) => ConceptoPlantilla(papel, defecto ?? ConceptoAsiento);
    }

    private static List<LineaAsiento> LineasVenta(DocumentoPendiente d, string cuentaIngreso, string cuentaCliente, Papeles p, IReadOnlyList<(string Cuenta, decimal Base)> ingresos)
    {
        var concepto = p.Concepto(PapelApunte.Resultado);
        // Debe: cliente (total a cobrar) + retención soportada. Haber: ingreso (base) + IVA repercutido.
        var lineas = new List<LineaAsiento> { new(cuentaCliente, d.Total, 0m, p.Concepto(PapelApunte.Tercero)) };
        if (d.RetencionIrpf != 0m)
        {
            lineas.Add(new LineaAsiento(p.Cuenta(PapelApunte.RetencionVenta, PlanBasico.CuentaRetencionVenta), d.RetencionIrpf, 0m, p.Concepto(PapelApunte.RetencionVenta, "Retención IRPF")));
        }

        // Con líneas, cada base a su cuenta (la de un anticipo, 438, con base negativa queda al debe: cancela el anticipo
        // facturado; un suplido, fuera de la base, va al haber de su cuenta). Si al redondear por cuenta el asiento no
        // cuadra por céntimos, la diferencia va a la cuenta de más importe.
        if (ingresos.Count > 0)
        {
            var grupos = ingresos.GroupBy(x => x.Cuenta).Select(g => (Cuenta: g.Key, Base: Math.Round(g.Sum(x => x.Base), 2))).Where(g => g.Base != 0m).ToList();
            var diferencia = d.Total + d.RetencionIrpf - d.CuotaIva - grupos.Sum(g => g.Base);
            if (diferencia != 0m && grupos.Count > 0)
            {
                var i = grupos.FindIndex(g => Math.Abs(g.Base) == grupos.Max(x => Math.Abs(x.Base)));
                grupos[i] = (grupos[i].Cuenta, grupos[i].Base + diferencia);
            }

            foreach (var (cuenta, importe) in grupos)
            {
                lineas.Add(importe >= 0m ? new LineaAsiento(cuenta, 0m, importe, concepto) : new LineaAsiento(cuenta, -importe, 0m, concepto));
            }
        }
        else
        {
            lineas.Add(new LineaAsiento(cuentaIngreso, 0m, d.BaseImponible, concepto));
        }

        if (d.CuotaIva != 0m)
        {
            lineas.Add(new LineaAsiento(p.Cuenta(PapelApunte.IvaRepercutido, PlanBasico.CuentaIvaRepercutido), 0m, d.CuotaIva, p.Concepto(PapelApunte.IvaRepercutido, "IVA repercutido")));
        }

        return lineas;
    }

    /// <summary>
    /// Factura recibida con varias bases. Debe: cada cuenta de gasto (base + cuota no deducible + recargo) e IVA soportado
    /// deducible (tras la prorrata). Haber: IVA autoliquidado (inversión del sujeto pasivo, intracomunitarias), retención y
    /// proveedor (total a pagar). La parte no deducible se reparte entre las líneas en proporción a su cuota.
    /// </summary>
    private static List<LineaAsiento> LineasCompraDesglosada(DocumentoPendiente d, string cuentaGasto, string cuentaProveedor, Papeles p, decimal cuotaDeducible)
    {
        var concepto = p.Concepto(PapelApunte.Resultado);
        // Cada línea carga su parte no deducible propia (p. ej. el 50 % del IVA de un turismo); lo que además quita la
        // prorrata se reparte en proporción a lo que cada línea dejaba deducir.
        var candidata = d.Lineas.Sum(l => l.CuotaDeducible);
        var porProrrata = candidata - cuotaDeducible;
        var cargos = new Dictionary<string, decimal>(StringComparer.Ordinal);
        var repartida = 0m;
        var conDeducible = d.Lineas.Select((l, i) => (l, i)).Where(x => x.l.CuotaDeducible != 0m).Select(x => x.i).LastOrDefault(-1);
        for (var i = 0; i < d.Lineas.Count; i++)
        {
            var l = d.Lineas[i];
            var parte = candidata == 0m ? 0m
                : i == conDeducible ? porProrrata - repartida
                : Math.Round(porProrrata * l.CuotaDeducible / candidata, 2, MidpointRounding.AwayFromZero);
            repartida += parte;
            var cuenta = string.IsNullOrWhiteSpace(l.CuentaGasto) ? cuentaGasto : l.CuentaGasto!;
            cargos[cuenta] = cargos.GetValueOrDefault(cuenta) + l.Base + (l.Cuota - l.CuotaDeducible) + parte + l.Recargo;
        }

        var lineas = cargos.Where(c => c.Value != 0m).Select(c => new LineaAsiento(c.Key, c.Value, 0m, concepto)).ToList();
        if (cuotaDeducible != 0m)
        {
            lineas.Add(new LineaAsiento(p.Cuenta(PapelApunte.IvaSoportado, PlanBasico.CuentaIvaSoportado), cuotaDeducible, 0m, p.Concepto(PapelApunte.IvaSoportado, "IVA soportado")));
        }

        var autoliquidada = d.Lineas.Where(l => l.Autoliquidada).Sum(l => l.Cuota);
        if (autoliquidada != 0m)
        {
            lineas.Add(new LineaAsiento(p.Cuenta(PapelApunte.IvaRepercutido, PlanBasico.CuentaIvaRepercutido), 0m, autoliquidada,
                p.Concepto(PapelApunte.IvaRepercutido, "IVA autoliquidado (inversión del sujeto pasivo / intracomunitaria)")));
        }

        if (d.RetencionIrpf != 0m)
        {
            lineas.Add(new LineaAsiento(p.Cuenta(PapelApunte.RetencionCompra, PlanBasico.CuentaRetencion), 0m, d.RetencionIrpf, p.Concepto(PapelApunte.RetencionCompra, "Retención IRPF")));
        }

        lineas.Add(new LineaAsiento(cuentaProveedor, 0m, d.Total, p.Concepto(PapelApunte.Tercero)));
        return lineas;
    }

    private static List<LineaAsiento> LineasCompra(DocumentoPendiente d, string cuentaGasto, string cuentaProveedor, Papeles p, decimal cuotaDeducible)
    {
        var concepto = p.Concepto(PapelApunte.Resultado);
        // Debe: gasto (base + cuota no deducible por prorrata) + IVA soportado deducible.
        // Haber: retención + proveedores (total).
        var noDeducible = d.CuotaIva - cuotaDeducible;
        var lineas = new List<LineaAsiento> { new(cuentaGasto, d.BaseImponible + noDeducible, 0m, concepto) };
        if (cuotaDeducible != 0m)
        {
            lineas.Add(new LineaAsiento(p.Cuenta(PapelApunte.IvaSoportado, PlanBasico.CuentaIvaSoportado), cuotaDeducible, 0m, p.Concepto(PapelApunte.IvaSoportado, "IVA soportado")));
        }

        if (d.RetencionIrpf != 0m)
        {
            lineas.Add(new LineaAsiento(p.Cuenta(PapelApunte.RetencionCompra, PlanBasico.CuentaRetencion), 0m, d.RetencionIrpf, p.Concepto(PapelApunte.RetencionCompra, "Retención IRPF")));
        }

        lineas.Add(new LineaAsiento(cuentaProveedor, 0m, d.Total, p.Concepto(PapelApunte.Tercero)));
        return lineas;
    }
}

/// <summary>
/// Encola un documento contabilizable. Lo deja <b>pendiente</b>; si la empresa tiene activada la
/// contabilización automática, genera su asiento en el acto. Implementa el puerto compartido.
/// </summary>
public sealed class EncolarDocumento : IColaContabilizacion
{
    private readonly IRepositorioDocumentosPendientes _pendientes;
    private readonly IRepositorioConfigContabilidad _config;
    private readonly PosterDocumento _poster;
    private readonly IUnidadDeTrabajoContabilidad _unidad;
    private readonly IReloj _reloj;

    public EncolarDocumento(IRepositorioDocumentosPendientes pendientes, IRepositorioConfigContabilidad config, PosterDocumento poster, IUnidadDeTrabajoContabilidad unidad, IReloj reloj)
    {
        _pendientes = pendientes; _config = config; _poster = poster; _unidad = unidad; _reloj = reloj;
    }

    public async Task EncolarAsync(Guid empresaId, DocumentoContabilizable documento, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(documento);

        // La cola de contabilización (partida doble) solo aplica en modo Completo. En modo Simple la
        // empresa solo lleva Libro de IVA (el gasto/factura ya lo alimentan): no hay asientos ni panel.
        var config = await _config.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        if ((config?.Modo ?? ModoContabilidad.Simple) != ModoContabilidad.Completo)
        {
            return;
        }

        // Idempotencia: si este documento de origen ya se encoló, no lo dupliques (la bandeja de salida
        // garantiza entrega «al menos una vez», así que el mismo mensaje puede reintentarse).
        if (await _pendientes.ExistePorOrigenAsync(empresaId, documento.OrigenTipo, documento.OrigenId, ct).ConfigureAwait(false))
        {
            return;
        }

        var pendiente = DocumentoPendiente.Crear(empresaId, documento, _reloj.AhoraUtc);
        _pendientes.Agregar(pendiente);

        if (config?.ContabilizacionAutomatica == true)
        {
            var asiento = await _poster.ConstruirAsync(pendiente, ct).ConfigureAwait(false);
            if (asiento.EsCorrecto)
            {
                await _poster.AgregarAsync(asiento.Valor, pendiente, ct).ConfigureAwait(false);
                pendiente.MarcarContabilizado(asiento.Valor.Id);
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
    }
}

/// <summary>Configuración contable de la empresa: modo, contabilización automática y longitud de subcuenta.</summary>
public sealed record ConfigContabilidadDto(string Modo, bool ContabilizacionAutomatica, int LongitudSubcuenta);

/// <summary>Lee la configuración contable de la empresa (por defecto: Simple, sin automática).</summary>
public sealed class ObtenerConfigContabilidad
{
    private readonly IRepositorioConfigContabilidad _config;
    public ObtenerConfigContabilidad(IRepositorioConfigContabilidad config) => _config = config;

    public async Task<ConfigContabilidadDto> EjecutarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var c = await _config.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        return new ConfigContabilidadDto(
            (c?.Modo ?? ModoContabilidad.Simple).ToString(),
            c?.ContabilizacionAutomatica ?? false,
            c?.LongitudSubcuenta ?? ConfiguracionContabilidad.LongitudSubcuentaDefecto);
    }
}

/// <summary>Cambia la longitud de las subcuentas de tercero de la empresa.</summary>
public sealed class CambiarLongitudSubcuenta
{
    private readonly IRepositorioConfigContabilidad _config;
    private readonly IUnidadDeTrabajoContabilidad _unidad;

    public CambiarLongitudSubcuenta(IRepositorioConfigContabilidad config, IUnidadDeTrabajoContabilidad unidad)
    {
        _config = config;
        _unidad = unidad;
    }

    public async Task<int> EjecutarAsync(Guid empresaId, int longitud, CancellationToken ct = default)
    {
        var config = await _config.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        if (config is null)
        {
            config = new ConfiguracionContabilidad(empresaId, ModoContabilidad.Simple);
            config.CambiarLongitudSubcuenta(longitud);
            _config.Agregar(config);
        }
        else
        {
            config.CambiarLongitudSubcuenta(longitud);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return config.LongitudSubcuenta;
    }
}

/// <summary>Activa o desactiva la contabilización automática de la empresa.</summary>
public sealed class CambiarContabilizacionAutomatica
{
    private readonly IRepositorioConfigContabilidad _config;
    private readonly IUnidadDeTrabajoContabilidad _unidad;

    public CambiarContabilizacionAutomatica(IRepositorioConfigContabilidad config, IUnidadDeTrabajoContabilidad unidad)
    {
        _config = config; _unidad = unidad;
    }

    public async Task EjecutarAsync(Guid empresaId, bool automatica, CancellationToken ct = default)
    {
        var config = await _config.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        if (config is null)
        {
            config = new ConfiguracionContabilidad(empresaId, ModoContabilidad.Simple);
            config.CambiarContabilizacionAutomatica(automatica);
            _config.Agregar(config);
        }
        else
        {
            config.CambiarContabilizacionAutomatica(automatica);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
    }
}

/// <summary>Lista los documentos pendientes de contabilizar (panel del contable).</summary>
public sealed class ListarPendientesContabilizar
{
    private readonly IRepositorioDocumentosPendientes _pendientes;
    public ListarPendientesContabilizar(IRepositorioDocumentosPendientes pendientes) => _pendientes = pendientes;

    public async Task<IReadOnlyList<DocumentoPendienteDto>> EjecutarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var lista = await _pendientes.ListarPendientesAsync(empresaId, ct).ConfigureAwait(false);
        return lista.Select(DocumentoPendienteDto.Desde).ToList();
    }
}

/// <summary>Cambia la fecha de registro de un documento pendiente (típico en facturas recibidas).</summary>
public sealed class CambiarFechaRegistro
{
    private readonly IRepositorioDocumentosPendientes _pendientes;
    private readonly IUnidadDeTrabajoContabilidad _unidad;

    public CambiarFechaRegistro(IRepositorioDocumentosPendientes pendientes, IUnidadDeTrabajoContabilidad unidad)
    {
        _pendientes = pendientes; _unidad = unidad;
    }

    public async Task<Resultado> EjecutarAsync(Guid id, DateOnly fecha, CancellationToken ct = default)
    {
        var doc = await _pendientes.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (doc is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("pendiente.no_encontrado", "No se encontró el documento pendiente."));
        }

        var r = doc.CambiarFechaRegistro(fecha);
        if (r.EsFallo)
        {
            return r;
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }
}

/// <summary>Contabiliza uno o varios documentos pendientes (genera su asiento con la fecha de registro).</summary>
public sealed class ContabilizarPendientes
{
    private readonly IRepositorioDocumentosPendientes _pendientes;
    private readonly PosterDocumento _poster;
    private readonly IUnidadDeTrabajoContabilidad _unidad;

    public ContabilizarPendientes(IRepositorioDocumentosPendientes pendientes, PosterDocumento poster, IUnidadDeTrabajoContabilidad unidad)
    {
        _pendientes = pendientes; _poster = poster; _unidad = unidad;
    }

    public async Task<Resultado<int>> EjecutarAsync(Guid empresaId, IReadOnlyList<Guid> ids, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ids);
        var contabilizados = 0;
        foreach (var id in ids)
        {
            var doc = await _pendientes.ObtenerAsync(id, ct).ConfigureAwait(false);
            if (doc is null || doc.EmpresaId != empresaId || doc.Estado != EstadoContabilizacion.Pendiente)
            {
                continue;
            }

            var asiento = await _poster.ConstruirAsync(doc, ct).ConfigureAwait(false);
            if (asiento.EsFallo)
            {
                return Resultado.Fallo<int>(asiento.Error);
            }

            await _poster.AgregarAsync(asiento.Valor, doc, ct).ConfigureAwait(false);
            doc.MarcarContabilizado(asiento.Valor.Id);
            contabilizados++;
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(contabilizados);
    }
}
