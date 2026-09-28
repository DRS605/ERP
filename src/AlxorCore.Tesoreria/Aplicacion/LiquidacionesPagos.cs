using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Terceros.Aplicacion;
using AlxorCore.Tesoreria.Dominio;

namespace AlxorCore.Tesoreria.Aplicacion;

/// <summary>Factura pendiente (del proveedor o, para compensar, del mismo NIF como cliente).</summary>
public sealed record DocumentoPendienteDto(Guid Id, string Documento, DateOnly Fecha, decimal Total, decimal Pendiente);

/// <summary>Proveedor con facturas pendientes hasta la fecha de corte, para la liquidación masiva.</summary>
public sealed record ProveedorPendienteDto(Guid ProveedorId, string Nombre, string? Nif, bool Agricultor, int Facturas, decimal Pendiente,
    Guid? ClienteId, string? Cliente, decimal PendienteCliente, decimal Entregas, bool TieneIban);

/// <summary>
/// Lo pendiente que entra en una liquidación de pagos: facturas del proveedor sin pagar (fuera de remesas vivas),
/// facturas del mismo NIF como cliente y proveedores con algo que liquidar. Lo implementa la API con SQL de solo lectura.
/// </summary>
public interface IPendientesLiquidacionPagos
{
    Task<IReadOnlyList<DocumentoPendienteDto>> GastosAsync(Guid proveedorId, DateOnly hasta, CancellationToken ct = default);

    Task<IReadOnlyList<DocumentoPendienteDto>> FacturasAsync(Guid clienteId, DateOnly hasta, CancellationToken ct = default);

    /// <summary>La ficha de cliente del grupo con el mismo NIF que el proveedor (null si no hay una sola).</summary>
    Task<Guid?> ClienteMismoNifAsync(Guid proveedorId, CancellationToken ct = default);

    Task<IReadOnlyList<ProveedorPendienteDto>> ProveedoresAsync(DateOnly hasta, bool soloAgricultores, CancellationToken ct = default);
}

public interface IRepositorioLiquidacionesPagos
{
    void Agregar(EntregaCuentaProveedor entrega);

    void Agregar(LiquidacionPagos liquidacion);

    Task<EntregaCuentaProveedor?> EntregaAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<EntregaCuentaProveedor>> EntregasAsync(Guid? proveedorId, bool soloPendientes, CancellationToken ct = default);

    /// <summary>La entrega a cuenta que se canceló con este pago (null si no es una cancelación).</summary>
    Task<EntregaCuentaProveedor?> EntregaDeMovimientoAsync(Guid movimientoId, CancellationToken ct = default);

    Task<LiquidacionPagos?> LiquidacionAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<LiquidacionPagos>> LiquidacionesAsync(Guid? proveedorId, Guid? loteId, CancellationToken ct = default);

    /// <summary>La liquidación de pagos en la que se registró este movimiento (null si no es de una).</summary>
    Task<LiquidacionPagos?> LiquidacionDeMovimientoAsync(Guid movimientoId, CancellationToken ct = default);

    Task<int> UltimoNumeroAsync(int ejercicio, CancellationToken ct = default);

    /// <summary>Bloqueo consultivo hasta confirmar (numeración sin huecos y sin liquidar dos veces lo mismo).</summary>
    Task BloquearAsync(string clave, CancellationToken ct = default);
}

public sealed record DatosEntregaCuenta(Guid ProveedorId, decimal Importe, DateOnly? Fecha = null, string? Concepto = null, string? Metodo = null, Guid? CuentaBancariaId = null);

public sealed record AplicarEntregaComando(Guid GastoId, decimal? Importe = null, DateOnly? Fecha = null);

public sealed record CancelacionEntregaDto(Guid GastoId, decimal Importe, DateOnly Fecha, Guid MovimientoId, Guid? LiquidacionId);

public sealed record EntregaCuentaDto(Guid Id, Guid ProveedorId, string? Proveedor, decimal Importe, DateOnly Fecha, string Concepto, string? Metodo, Guid? CuentaBancariaId,
    decimal Cancelado, decimal Pendiente, string Estado, string? MotivoAnulacion, IReadOnlyList<CancelacionEntregaDto> Cancelaciones);

/// <summary>
/// Liquidación de pagos a un proveedor. <paramref name="Hasta"/>: fecha de corte de las facturas (por defecto, la de la
/// liquidación). <paramref name="FormaPago"/>: dejar el líquido pendiente, pagarlo ya por el banco o incluirlo en una remesa.
/// </summary>
public sealed record DatosLiquidacionPagos(Guid ProveedorId, DateOnly? Hasta = null, DateOnly? Fecha = null, bool CancelarEntregas = true, bool Compensar = true,
    FormaPagoLiquidacion FormaPago = FormaPagoLiquidacion.Pendiente, Guid? CuentaBancariaId = null, string? Metodo = null);

/// <summary>Liquidación de todos los proveedores (o solo agricultores, o los indicados) con facturas pendientes hasta la fecha.</summary>
public sealed record DatosLiquidacionMasiva(DateOnly? Hasta = null, DateOnly? Fecha = null, bool SoloAgricultores = false, IReadOnlyList<Guid>? ProveedorIds = null,
    bool CancelarEntregas = true, bool Compensar = true, FormaPagoLiquidacion FormaPago = FormaPagoLiquidacion.Remesa, Guid? CuentaBancariaId = null, string? Metodo = null);

public sealed record LineaLiquidacionPagosDto(string Tipo, Guid DocumentoId, string Documento, decimal Importe, Guid? MovimientoId, Guid? EntregaId);

public sealed record LiquidacionPagosDto(Guid Id, string Numero, DateOnly Fecha, DateOnly Hasta, Guid ProveedorId, string? Proveedor, Guid? ClienteId,
    decimal APagar, decimal EntregasCanceladas, decimal Compensado, decimal Liquido, string FormaPago, Guid? CuentaBancariaId, Guid? RemesaId, Guid? LoteId,
    string Estado, string? MotivoAnulacion, IReadOnlyList<LineaLiquidacionPagosDto> Lineas, IReadOnlyList<string> Avisos);

public sealed record PropuestaLiquidacionPagosDto(Guid ProveedorId, string? Proveedor, Guid? ClienteId, DateOnly Hasta, IReadOnlyList<DocumentoPendienteDto> Gastos,
    IReadOnlyList<DocumentoPendienteDto> Facturas, IReadOnlyList<EntregaCuentaDto> Entregas, decimal APagar, decimal EntregasCanceladas, decimal Compensado,
    decimal Liquido, IReadOnlyList<LineaLiquidacionPagosDto> Lineas);

public sealed record ResultadoLiquidacionMasivaDto(Guid LoteId, int Liquidaciones, decimal APagar, decimal EntregasCanceladas, decimal Compensado, decimal Liquido,
    Guid? RemesaId, string? Remesa, IReadOnlyList<LiquidacionPagosDto> Detalle, IReadOnlyList<string> Omitidos);

/// <summary>
/// Entregas a cuenta a proveedores y agricultores y liquidación de pagos, como en Hispatec:
/// <list type="bullet">
/// <item><b>Entrega a cuenta</b> (sin IVA): 407 a tesorería. Se cancela contra sus facturas con un pago de cuenta puente 407.</item>
/// <item><b>Liquidación de pagos</b>: facturas pendientes del proveedor hasta la fecha − entregas a cuenta − lo que debe él
/// como cliente (mismo NIF, compensado a través de 555) = líquido, que se paga directo o por remesa SEPA. Nunca negativa.</item>
/// <item><b>Masiva</b>: una liquidación por proveedor (o por agricultor) y una sola remesa de transferencias con todos los líquidos.</item>
/// </list>
/// Todo lo de una liquidación (movimientos, cancelaciones, asientos en la bandeja) se guarda en una transacción.
/// </summary>
public sealed class LiquidacionesPagos
{
    private const string MetodoEntrega = "Entrega a cuenta";

    private readonly IRepositorioLiquidacionesPagos _repo;
    private readonly IPendientesLiquidacionPagos _pendientes;
    private readonly IRepositorioMovimientos _movimientos;
    private readonly IConsultaGastos _gastos;
    private readonly IConsultaFacturas _facturas;
    private readonly IConsultaProveedores _proveedores;
    private readonly IUnidadDeTrabajoTesoreria _unidad;
    private readonly IReloj _reloj;
    private readonly ContabilizacionTesoreria? _contabilizacion;
    private readonly ResolutorCuentaTesoreria? _resolutor;
    private readonly GestionRemesas? _remesas;
    private readonly IRepositorioRemesas? _repoRemesas;
    private readonly Dictionary<int, int> _ultimos = [];

    public LiquidacionesPagos(IRepositorioLiquidacionesPagos repo, IPendientesLiquidacionPagos pendientes, IRepositorioMovimientos movimientos, IConsultaGastos gastos,
        IConsultaFacturas facturas, IConsultaProveedores proveedores, IUnidadDeTrabajoTesoreria unidad, IReloj reloj, ContabilizacionTesoreria? contabilizacion = null,
        ResolutorCuentaTesoreria? resolutor = null, GestionRemesas? remesas = null, IRepositorioRemesas? repoRemesas = null)
    {
        _repo = repo;
        _pendientes = pendientes;
        _movimientos = movimientos;
        _gastos = gastos;
        _facturas = facturas;
        _proveedores = proveedores;
        _unidad = unidad;
        _reloj = reloj;
        _contabilizacion = contabilizacion;
        _resolutor = resolutor;
        _remesas = remesas;
        _repoRemesas = repoRemesas;
    }

    private DateOnly Hoy => DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);

    private static string Clave(Guid empresaId) => $"tesoreria:liquidacion-pagos:{empresaId}";

    // ------------------------------------------------------------------ Entregas a cuenta

    public async Task<IReadOnlyList<EntregaCuentaDto>> EntregasAsync(Guid? proveedorId, bool soloPendientes, CancellationToken ct = default)
    {
        var lista = await _repo.EntregasAsync(proveedorId, soloPendientes, ct).ConfigureAwait(false);
        var nombres = await NombresAsync(lista.Select(e => e.ProveedorId), ct).ConfigureAwait(false);
        return lista.Select(e => Dto(e, nombres.GetValueOrDefault(e.ProveedorId))).ToList();
    }

    public async Task<Resultado<EntregaCuentaDto>> RegistrarEntregaAsync(Guid empresaId, DatosEntregaCuenta datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var proveedor = await _proveedores.ObtenerAsync(datos.ProveedorId, ct).ConfigureAwait(false);
        if (proveedor is null)
        {
            return Resultado.Fallo<EntregaCuentaDto>(Error.NoEncontrado("proveedor.no_encontrado", "El proveedor no existe."));
        }

        var cuenta = await RegistrarCobro.ResolverCuentaAsync(_resolutor, datos.CuentaBancariaId, datos.Metodo, ct).ConfigureAwait(false);
        if (cuenta.EsFallo)
        {
            return Resultado.Fallo<EntregaCuentaDto>(cuenta.Error);
        }

        var e = EntregaCuentaProveedor.Registrar(empresaId, datos.ProveedorId, datos.Importe, datos.Fecha ?? Hoy, datos.Concepto, datos.Metodo, cuenta.Valor, _reloj);
        if (e.EsFallo)
        {
            return Resultado.Fallo<EntregaCuentaDto>(e.Error);
        }

        _repo.Agregar(e.Valor);
        await EncolarEntregaAsync(e.Valor, proveedor.Nombre, anulacion: false, ct).ConfigureAwait(false);
        await GuardarYDespacharAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(e.Valor, proveedor.Nombre));
    }

    public async Task<Resultado<EntregaCuentaDto>> AnularEntregaAsync(Guid id, string? motivo, CancellationToken ct = default)
    {
        var e = await _repo.EntregaAsync(id, ct).ConfigureAwait(false);
        if (e is null)
        {
            return Resultado.Fallo<EntregaCuentaDto>(Error.NoEncontrado("entregacuenta.no_encontrada", "La entrega a cuenta no existe."));
        }

        var r = e.Anular(motivo, _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<EntregaCuentaDto>(r.Error);
        }

        var nombre = (await _proveedores.ObtenerAsync(e.ProveedorId, ct).ConfigureAwait(false))?.Nombre;
        await EncolarEntregaAsync(e, nombre, anulacion: true, ct).ConfigureAwait(false);
        await GuardarYDespacharAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(e, nombre));
    }

    /// <summary>Cancela (parte de) una entrega a cuenta contra una factura del mismo proveedor, fuera de una liquidación.</summary>
    public async Task<Resultado<EntregaCuentaDto>> AplicarEntregaAsync(Guid empresaId, Guid id, AplicarEntregaComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var e = await _repo.EntregaAsync(id, ct).ConfigureAwait(false);
        if (e is null)
        {
            return Resultado.Fallo<EntregaCuentaDto>(Error.NoEncontrado("entregacuenta.no_encontrada", "La entrega a cuenta no existe."));
        }

        var gasto = await _gastos.ObtenerAsync(comando.GastoId, ct).ConfigureAwait(false);
        if (gasto is null || gasto.Estado == "Anulado")
        {
            return Resultado.Fallo<EntregaCuentaDto>(Error.NoEncontrado("gasto.no_encontrado", "La factura del proveedor no existe o está anulada."));
        }

        if (gasto.ProveedorId != e.ProveedorId)
        {
            return Resultado.Fallo<EntregaCuentaDto>(Error.Conflicto("entregacuenta.otro_proveedor", "La factura es de otro proveedor."));
        }

        var pendiente = Redondeo.Dos(gasto.Total - await _movimientos.SumaAsync(TipoDocumentoTesoreria.Gasto, gasto.Id, ct).ConfigureAwait(false));
        var importe = Redondeo.Dos(comando.Importe ?? Math.Min(e.Pendiente, pendiente));
        if (importe > pendiente)
        {
            return Resultado.Fallo<EntregaCuentaDto>(Error.Conflicto("movimiento.sobrepago", "El importe supera el pendiente de la factura."));
        }

        var v = e.ValidarCancelacion(importe);
        if (v.EsFallo)
        {
            return Resultado.Fallo<EntregaCuentaDto>(v.Error);
        }

        var fecha = comando.Fecha ?? Hoy;
        var m = Movimiento.Crear(empresaId, TipoDocumentoTesoreria.Gasto, gasto.Id, SentidoMovimiento.Pago, importe, fecha, MetodoEntrega, _reloj,
            cuentaPuente: CuentasPuente.Entregas);
        if (m.EsFallo)
        {
            return Resultado.Fallo<EntregaCuentaDto>(m.Error);
        }

        _movimientos.Agregar(m.Valor);
        e.Cancelar(gasto.Id, importe, fecha, m.Valor.Id, null);
        await EncolarAsync(m.Valor, ct).ConfigureAwait(false);
        await GuardarYDespacharAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(e, (await _proveedores.ObtenerAsync(e.ProveedorId, ct).ConfigureAwait(false))?.Nombre));
    }

    // ------------------------------------------------------------------ Liquidaciones de pagos

    public async Task<IReadOnlyList<LiquidacionPagosDto>> ListarAsync(Guid? proveedorId, Guid? loteId, CancellationToken ct = default)
    {
        var lista = await _repo.LiquidacionesAsync(proveedorId, loteId, ct).ConfigureAwait(false);
        var nombres = await NombresAsync(lista.Select(l => l.ProveedorId), ct).ConfigureAwait(false);
        return lista.Select(l => Dto(l, nombres.GetValueOrDefault(l.ProveedorId), [])).ToList();
    }

    public async Task<Resultado<LiquidacionPagosDto>> ObtenerAsync(Guid id, CancellationToken ct = default)
    {
        var l = await _repo.LiquidacionAsync(id, ct).ConfigureAwait(false);
        return l is null ? NoEncontrada<LiquidacionPagosDto>()
            : Resultado.Ok(Dto(l, (await _proveedores.ObtenerAsync(l.ProveedorId, ct).ConfigureAwait(false))?.Nombre, []));
    }

    /// <summary>Lo que saldría de liquidar ahora al proveedor, sin registrar nada.</summary>
    public async Task<Resultado<PropuestaLiquidacionPagosDto>> PrevisualizarAsync(DatosLiquidacionPagos datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var plan = await PlanificarAsync(datos, ct).ConfigureAwait(false);
        if (plan.EsFallo)
        {
            return Resultado.Fallo<PropuestaLiquidacionPagosDto>(plan.Error);
        }

        var p = plan.Valor;
        return Resultado.Ok(new PropuestaLiquidacionPagosDto(datos.ProveedorId, p.Proveedor, p.ClienteId, p.Hasta, p.Gastos, p.Facturas,
            p.Entregas.Select(e => Dto(e, p.Proveedor)).ToList(), p.APagar, p.EntregasCanceladas, p.Compensado, p.Liquido,
            p.Lineas.Select(x => new LineaLiquidacionPagosDto(x.Tipo.ToString(), x.DocumentoId, x.Documento, x.Importe, null, x.Entrega?.Id)).ToList()));
    }

    public async Task<Resultado<LiquidacionPagosDto>> EmitirAsync(Guid empresaId, DatosLiquidacionPagos datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var r = await EmitirInternoAsync(empresaId, datos, loteId: null, ct).ConfigureAwait(false);
        if (r.EsFallo)
        {
            return Resultado.Fallo<LiquidacionPagosDto>(r.Error);
        }

        var (l, proveedor, gastosPendientes) = r.Valor;
        var avisos = new List<string>();
        if (datos.FormaPago == FormaPagoLiquidacion.Remesa)
        {
            await RemesarAsync(empresaId, [(l, gastosPendientes)], datos.Fecha ?? Hoy, datos.CuentaBancariaId, avisos, ct).ConfigureAwait(false);
        }

        return Resultado.Ok(Dto(l, proveedor, avisos));
    }

    public async Task<Resultado<ResultadoLiquidacionMasivaDto>> MasivaAsync(Guid empresaId, DatosLiquidacionMasiva datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var fecha = datos.Fecha ?? Hoy;
        var hasta = datos.Hasta ?? fecha;
        var candidatos = await _pendientes.ProveedoresAsync(hasta, datos.SoloAgricultores, ct).ConfigureAwait(false);
        if (datos.ProveedorIds is { Count: > 0 } ids)
        {
            candidatos = candidatos.Where(c => ids.Contains(c.ProveedorId)).ToList();
        }

        if (candidatos.Count == 0)
        {
            return Resultado.Fallo<ResultadoLiquidacionMasivaDto>(Error.Validacion("liquidacionpagos.nada", "No hay facturas de proveedores pendientes de pago hasta esa fecha."));
        }

        var lote = Guid.NewGuid();
        var forma = datos.FormaPago == FormaPagoLiquidacion.Remesa ? FormaPagoLiquidacion.Pendiente : datos.FormaPago;
        var hechas = new List<(LiquidacionPagos L, string? Proveedor, IReadOnlyList<Guid> Gastos)>();
        var omitidos = new List<string>();
        foreach (var c in candidatos)
        {
            var r = await EmitirInternoAsync(empresaId, new DatosLiquidacionPagos(c.ProveedorId, hasta, fecha, datos.CancelarEntregas, datos.Compensar, forma,
                datos.CuentaBancariaId, datos.Metodo), lote, ct).ConfigureAwait(false);
            if (r.EsFallo)
            {
                omitidos.Add($"{c.Nombre}: {r.Error.Mensaje}");
                continue;
            }

            hechas.Add(r.Valor);
        }

        var avisos = new List<string>();
        Guid? remesaId = null;
        string? codigo = null;
        if (datos.FormaPago == FormaPagoLiquidacion.Remesa && hechas.Count > 0)
        {
            (remesaId, codigo) = await RemesarAsync(empresaId, hechas.Select(h => (h.L, h.Gastos)).ToList(), fecha, datos.CuentaBancariaId, avisos, ct).ConfigureAwait(false);
        }

        omitidos.AddRange(avisos);
        var detalle = hechas.Select(h => Dto(h.L, h.Proveedor, [])).ToList();
        return Resultado.Ok(new ResultadoLiquidacionMasivaDto(lote, hechas.Count, Redondeo.Dos(detalle.Sum(d => d.APagar)), Redondeo.Dos(detalle.Sum(d => d.EntregasCanceladas)),
            Redondeo.Dos(detalle.Sum(d => d.Compensado)), Redondeo.Dos(detalle.Sum(d => d.Liquido)), remesaId, codigo, detalle, omitidos));
    }

    /// <summary>
    /// Anula una liquidación: deshace sus pagos, cobros compensados y cancelaciones de entregas (con sus contraasientos).
    /// Si su líquido va en una remesa, hay que anular antes la remesa.
    /// </summary>
    public async Task<Resultado<LiquidacionPagosDto>> AnularAsync(Guid id, string? motivo, CancellationToken ct = default)
    {
        var l = await _repo.LiquidacionAsync(id, ct).ConfigureAwait(false);
        if (l is null)
        {
            return NoEncontrada<LiquidacionPagosDto>();
        }

        if (l.Estado == EstadoLiquidacionPagos.Anulada)
        {
            return Resultado.Fallo<LiquidacionPagosDto>(Error.Conflicto("liquidacionpagos.anulada", "La liquidación ya está anulada."));
        }

        if (l.RemesaId is { } remesaId && _repoRemesas is not null && await _repoRemesas.ObtenerAsync(remesaId, ct).ConfigureAwait(false) is { } remesa
            && remesa.Estado != EstadoRemesa.Anulada)
        {
            return Resultado.Fallo<LiquidacionPagosDto>(Error.Conflicto("liquidacionpagos.en_remesa",
                $"El líquido va en la remesa {remesa.Codigo}: anula antes la remesa (o sus pagos) y después la liquidación."));
        }

        var hoy = Hoy;
        var entregas = new Dictionary<Guid, EntregaCuentaProveedor>();
        foreach (var linea in l.Lineas.Where(x => x.MovimientoId is not null))
        {
            var original = await _movimientos.ObtenerAsync(linea.MovimientoId!.Value, ct).ConfigureAwait(false);
            if (original is null || await _movimientos.EstaAnuladoAsync(original.Id, ct).ConfigureAwait(false))
            {
                continue;
            }

            var anulacion = Movimiento.CrearAnulacion(original, hoy < original.Fecha ? original.Fecha : hoy, _reloj, $"Anulación {l.NumeroCompleto}");
            if (anulacion.EsFallo)
            {
                return Resultado.Fallo<LiquidacionPagosDto>(anulacion.Error);
            }

            _movimientos.Agregar(anulacion.Valor);
            if (linea.EntregaId is { } entregaId)
            {
                if (!entregas.TryGetValue(entregaId, out var e))
                {
                    e = (await _repo.EntregaAsync(entregaId, ct).ConfigureAwait(false))!;
                    entregas[entregaId] = e;
                }

                e.RevertirCancelacion(original.Id, anulacion.Valor.Id, anulacion.Valor.Fecha);
            }

            if (_contabilizacion is not null)
            {
                await _contabilizacion.EncolarMovimientoAsync(anulacion.Valor, aplicacionAnticipo: false, original, ct: ct).ConfigureAwait(false);
            }
        }

        l.Anular(motivo, _reloj);
        await GuardarYDespacharAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(l, (await _proveedores.ObtenerAsync(l.ProveedorId, ct).ConfigureAwait(false))?.Nombre, []));
    }

    // ------------------------------------------------------------------ Cálculo

    private sealed record LineaPlan(TipoLineaLiquidacionPagos Tipo, Guid DocumentoId, string Documento, decimal Importe, EntregaCuentaProveedor? Entrega);

    private sealed record Plan(string? Proveedor, Guid? ClienteId, DateOnly Hasta, IReadOnlyList<DocumentoPendienteDto> Gastos, IReadOnlyList<DocumentoPendienteDto> Facturas,
        IReadOnlyList<EntregaCuentaProveedor> Entregas, IReadOnlyList<LineaPlan> Lineas, IReadOnlyDictionary<Guid, decimal> Restante)
    {
        public decimal APagar => Redondeo.Dos(Gastos.Sum(g => g.Pendiente));

        public decimal EntregasCanceladas => Redondeo.Dos(Lineas.Where(l => l.Tipo == TipoLineaLiquidacionPagos.EntregaCuenta).Sum(l => l.Importe));

        public decimal Compensado => Redondeo.Dos(Lineas.Where(l => l.Tipo == TipoLineaLiquidacionPagos.Compensacion).Sum(l => l.Importe));

        public decimal Liquido => Redondeo.Dos(APagar - EntregasCanceladas - Compensado);
    }

    private async Task<Resultado<Plan>> PlanificarAsync(DatosLiquidacionPagos datos, CancellationToken ct)
    {
        var proveedor = await _proveedores.ObtenerAsync(datos.ProveedorId, ct).ConfigureAwait(false);
        if (proveedor is null)
        {
            return Resultado.Fallo<Plan>(Error.NoEncontrado("proveedor.no_encontrado", "El proveedor no existe."));
        }

        if (!Enum.IsDefined(datos.FormaPago))
        {
            return Resultado.Fallo<Plan>(Error.Validacion("liquidacionpagos.forma_pago", "Forma de pago no válida: Pendiente, Directo o Remesa."));
        }

        var hasta = datos.Hasta ?? datos.Fecha ?? Hoy;
        var gastos = (await _pendientes.GastosAsync(datos.ProveedorId, hasta, ct).ConfigureAwait(false)).OrderBy(g => g.Fecha).ThenBy(g => g.Documento, StringComparer.Ordinal).ToList();
        if (gastos.Count == 0)
        {
            return Resultado.Fallo<Plan>(Error.Validacion("liquidacionpagos.sin_facturas",
                "El proveedor no tiene facturas pendientes de pago hasta esa fecha (o ya están en una remesa)."));
        }

        var clienteId = datos.Compensar ? await _pendientes.ClienteMismoNifAsync(datos.ProveedorId, ct).ConfigureAwait(false) : null;
        var facturas = clienteId is { } cid
            ? (await _pendientes.FacturasAsync(cid, hasta, ct).ConfigureAwait(false)).OrderBy(f => f.Fecha).ThenBy(f => f.Documento, StringComparer.Ordinal).ToList()
            : [];
        var entregas = datos.CancelarEntregas
            ? (await _repo.EntregasAsync(datos.ProveedorId, soloPendientes: true, ct).ConfigureAwait(false)).Where(e => e.Fecha <= hasta).OrderBy(e => e.Fecha).ToList()
            : [];

        var restante = gastos.ToDictionary(g => g.Id, g => g.Pendiente);
        var lineas = new List<LineaPlan>();

        // 1. Entregas a cuenta, de la más antigua a la más reciente, contra las facturas más antiguas.
        foreach (var e in entregas)
        {
            var queda = e.Pendiente;
            foreach (var g in gastos)
            {
                var toma = Math.Min(queda, restante[g.Id]);
                if (toma <= 0)
                {
                    continue;
                }

                lineas.Add(new LineaPlan(TipoLineaLiquidacionPagos.EntregaCuenta, g.Id, g.Documento, toma, e));
                restante[g.Id] = Redondeo.Dos(restante[g.Id] - toma);
                queda = Redondeo.Dos(queda - toma);
            }
        }

        // 2. Compensación con lo que debe como cliente, solo hasta cubrir lo que queda por pagarle (nunca en negativo).
        var compensar = Math.Min(Redondeo.Dos(facturas.Sum(f => f.Pendiente)), Redondeo.Dos(restante.Values.Sum()));
        if (compensar > 0)
        {
            var quedaG = compensar;
            foreach (var g in gastos)
            {
                var toma = Math.Min(quedaG, restante[g.Id]);
                if (toma <= 0)
                {
                    continue;
                }

                lineas.Add(new LineaPlan(TipoLineaLiquidacionPagos.Compensacion, g.Id, g.Documento, toma, null));
                restante[g.Id] = Redondeo.Dos(restante[g.Id] - toma);
                quedaG = Redondeo.Dos(quedaG - toma);
            }

            var quedaF = compensar;
            foreach (var f in facturas)
            {
                var toma = Math.Min(quedaF, f.Pendiente);
                if (toma <= 0)
                {
                    continue;
                }

                lineas.Add(new LineaPlan(TipoLineaLiquidacionPagos.CobroCompensado, f.Id, f.Documento, toma, null));
                quedaF = Redondeo.Dos(quedaF - toma);
            }
        }

        // 3. El líquido, por banco o caja si se paga ya.
        if (datos.FormaPago == FormaPagoLiquidacion.Directo)
        {
            lineas.AddRange(gastos.Where(g => restante[g.Id] > 0).Select(g => new LineaPlan(TipoLineaLiquidacionPagos.Pago, g.Id, g.Documento, restante[g.Id], null)));
        }

        return Resultado.Ok(new Plan(proveedor.Nombre, clienteId, hasta, gastos, facturas, entregas, lineas, restante));
    }

    private async Task<Resultado<(LiquidacionPagos L, string? Proveedor, IReadOnlyList<Guid> Gastos)>> EmitirInternoAsync(Guid empresaId, DatosLiquidacionPagos datos,
        Guid? loteId, CancellationToken ct)
    {
        await _repo.BloquearAsync(Clave(empresaId), ct).ConfigureAwait(false);
        var plan = await PlanificarAsync(datos, ct).ConfigureAwait(false);
        if (plan.EsFallo)
        {
            return Resultado.Fallo<(LiquidacionPagos, string?, IReadOnlyList<Guid>)>(plan.Error);
        }

        var p = plan.Valor;
        Guid? cuentaBancaria = null;
        if (datos.FormaPago == FormaPagoLiquidacion.Directo)
        {
            var cuenta = await RegistrarCobro.ResolverCuentaAsync(_resolutor, datos.CuentaBancariaId, datos.Metodo, ct).ConfigureAwait(false);
            if (cuenta.EsFallo)
            {
                return Resultado.Fallo<(LiquidacionPagos, string?, IReadOnlyList<Guid>)>(cuenta.Error);
            }

            cuentaBancaria = cuenta.Valor;
        }

        // Lo pendiente sale de una lectura aparte: se comprueba contra la tesorería de esta transacción antes de tocar nada.
        var totales = new Dictionary<Guid, decimal>();
        foreach (var g in p.Gastos)
        {
            totales[g.Id] = g.Total;
        }

        foreach (var f in p.Facturas)
        {
            totales[f.Id] = f.Total;
        }

        foreach (var grupo in p.Lineas.GroupBy(x => (x.DocumentoId, Factura: x.Tipo == TipoLineaLiquidacionPagos.CobroCompensado)))
        {
            var tipo = grupo.Key.Factura ? TipoDocumentoTesoreria.Factura : TipoDocumentoTesoreria.Gasto;
            var liquidado = await _movimientos.SumaAsync(tipo, grupo.Key.DocumentoId, ct).ConfigureAwait(false);
            if (Redondeo.Dos(liquidado + grupo.Sum(x => x.Importe)) > totales[grupo.Key.DocumentoId])
            {
                return Resultado.Fallo<(LiquidacionPagos, string?, IReadOnlyList<Guid>)>(Error.Conflicto("liquidacionpagos.desactualizada",
                    $"El pendiente de {grupo.First().Documento} ha cambiado mientras se calculaba: vuelve a calcular la liquidación."));
            }
        }

        var fecha = datos.Fecha ?? Hoy;
        var numero = await SiguienteNumeroAsync(fecha.Year, ct).ConfigureAwait(false);
        var l = LiquidacionPagos.Crear(empresaId, numero, fecha, p.Hasta, datos.ProveedorId, p.ClienteId, datos.FormaPago, cuentaBancaria ?? datos.CuentaBancariaId,
            p.APagar, _reloj, loteId);
        foreach (var x in p.Lineas)
        {
            var (tipoDoc, sentido, metodo, puente) = x.Tipo switch
            {
                TipoLineaLiquidacionPagos.EntregaCuenta => (TipoDocumentoTesoreria.Gasto, SentidoMovimiento.Pago, $"Entrega a cuenta {l.NumeroCompleto}", CuentasPuente.Entregas),
                TipoLineaLiquidacionPagos.Compensacion => (TipoDocumentoTesoreria.Gasto, SentidoMovimiento.Pago, $"Compensación {l.NumeroCompleto}", CuentasPuente.Compensaciones),
                TipoLineaLiquidacionPagos.CobroCompensado => (TipoDocumentoTesoreria.Factura, SentidoMovimiento.Cobro, $"Compensación {l.NumeroCompleto}", CuentasPuente.Compensaciones),
                _ => (TipoDocumentoTesoreria.Gasto, SentidoMovimiento.Pago, string.IsNullOrWhiteSpace(datos.Metodo) ? $"Transferencia {l.NumeroCompleto}" : datos.Metodo, (string?)null),
            };
            var m = Movimiento.Crear(empresaId, tipoDoc, x.DocumentoId, sentido, x.Importe, fecha, metodo, _reloj, puente is null ? cuentaBancaria : null, puente);
            if (m.EsFallo)
            {
                return Resultado.Fallo<(LiquidacionPagos, string?, IReadOnlyList<Guid>)>(m.Error);
            }

            _movimientos.Agregar(m.Valor);
            if (x.Entrega is { } e)
            {
                var c = e.Cancelar(x.DocumentoId, x.Importe, fecha, m.Valor.Id, l.Id);
                if (c.EsFallo)
                {
                    return Resultado.Fallo<(LiquidacionPagos, string?, IReadOnlyList<Guid>)>(c.Error);
                }
            }

            l.AgregarLinea(x.Tipo, x.DocumentoId, x.Documento, x.Importe, m.Valor.Id, x.Entrega?.Id);
            await EncolarAsync(m.Valor, ct).ConfigureAwait(false);
        }

        if (p.Lineas.Count == 0)
        {
            // Nada que cancelar ni compensar: la liquidación solo recoge el líquido (se pagará después o por remesa).
            l.AgregarLinea(TipoLineaLiquidacionPagos.Pago, p.Gastos[0].Id, p.Gastos[0].Documento, 0m, null);
        }

        _repo.Agregar(l);
        await GuardarYDespacharAsync(ct).ConfigureAwait(false);
        IReadOnlyList<Guid> pendientes = datos.FormaPago == FormaPagoLiquidacion.Directo ? [] : p.Restante.Where(kv => kv.Value > 0).Select(kv => kv.Key).ToList();
        return Resultado.Ok<(LiquidacionPagos, string?, IReadOnlyList<Guid>)>((l, p.Proveedor, pendientes));
    }

    /// <summary>Genera una remesa de transferencias con los líquidos y la anota en cada liquidación (o las deja pendientes si no se pudo).</summary>
    private async Task<(Guid? Id, string? Codigo)> RemesarAsync(Guid empresaId, IReadOnlyList<(LiquidacionPagos L, IReadOnlyList<Guid> Gastos)> liquidaciones, DateOnly fecha,
        Guid? cuentaBancariaId, List<string> avisos, CancellationToken ct)
    {
        var gastos = liquidaciones.SelectMany(x => x.Gastos).Distinct().ToList();
        Resultado<RemesaCreadaDto>? r = null;
        if (gastos.Count > 0 && _remesas is not null)
        {
            r = await _remesas.CrearAsync(empresaId, new CrearRemesaComando(TipoRemesa.Pago, GastoIds: gastos, FechaCargo: fecha, CuentaBancariaId: cuentaBancariaId), ct)
                .ConfigureAwait(false);
        }

        if (r is { EsFallo: false })
        {
            avisos.AddRange(r.Valor.Omitidos);
            var enRemesa = r.Valor.Remesa.Lineas.Select(x => x.DocumentoId).ToHashSet();
            foreach (var (l, g) in liquidaciones)
            {
                if (g.Count > 0 && g.All(enRemesa.Contains))
                {
                    l.IncluirEnRemesa(r.Valor.Remesa.Id);
                }
                else
                {
                    l.DejarPendiente();
                }
            }
        }
        else
        {
            if (gastos.Count > 0)
            {
                avisos.Add(r is null ? "No se generó la remesa." : r.Error.Mensaje);
            }

            foreach (var (l, _) in liquidaciones)
            {
                l.DejarPendiente();
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return r is { EsFallo: false } ? (r.Valor.Remesa.Id, r.Valor.Remesa.Codigo) : (null, null);
    }

    private async Task<int> SiguienteNumeroAsync(int ejercicio, CancellationToken ct)
    {
        var ultimo = await _repo.UltimoNumeroAsync(ejercicio, ct).ConfigureAwait(false);
        if (_ultimos.TryGetValue(ejercicio, out var previo) && previo > ultimo)
        {
            ultimo = previo;
        }

        _ultimos[ejercicio] = ultimo + 1;
        return ultimo + 1;
    }

    private async Task EncolarAsync(Movimiento m, CancellationToken ct)
    {
        if (_contabilizacion is not null)
        {
            await _contabilizacion.EncolarMovimientoAsync(m, aplicacionAnticipo: false, ct: ct).ConfigureAwait(false);
        }
    }

    private async Task EncolarEntregaAsync(EntregaCuentaProveedor e, string? proveedor, bool anulacion, CancellationToken ct)
    {
        if (_contabilizacion is null)
        {
            return;
        }

        var tesoreria = await _contabilizacion.CuentaTesoreriaAsync(e.CuentaBancariaId, e.Metodo, ct).ConfigureAwait(false);
        _contabilizacion.EncolarAsientoDirecto(e.EmpresaId, ContabilizacionTesoreria.OrigenEntregaCuenta, anulacion ? ContabilizacionTesoreria.DeterministaAnulacion(e.Id) : e.Id,
            SentidoMovimiento.Pago, (anulacion ? "Anulación: " : "") + "Entrega a cuenta · " + e.Concepto, anulacion ? Hoy : e.Fecha, e.Importe, tesoreria,
            CuentasPuente.Entregas, anulacion, e.ProveedorId, proveedor);
    }

    private async Task GuardarYDespacharAsync(CancellationToken ct)
    {
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        if (_contabilizacion is not null)
        {
            await _contabilizacion.DespacharAsync(ct: ct).ConfigureAwait(false);
        }
    }

    private async Task<Dictionary<Guid, string>> NombresAsync(IEnumerable<Guid> ids, CancellationToken ct)
    {
        var d = new Dictionary<Guid, string>();
        foreach (var id in ids.Distinct())
        {
            if (await _proveedores.ObtenerAsync(id, ct).ConfigureAwait(false) is { } p)
            {
                d[id] = p.Nombre;
            }
        }

        return d;
    }

    private static Resultado<T> NoEncontrada<T>() =>
        Resultado.Fallo<T>(Error.NoEncontrado("liquidacionpagos.no_encontrada", "La liquidación de pagos no existe."));

    private static EntregaCuentaDto Dto(EntregaCuentaProveedor e, string? proveedor) => new(e.Id, e.ProveedorId, proveedor, e.Importe, e.Fecha, e.Concepto, e.Metodo,
        e.CuentaBancariaId, e.Cancelado, e.Pendiente, e.Estado.ToString(), e.MotivoAnulacion,
        e.Cancelaciones.Select(c => new CancelacionEntregaDto(c.GastoId, c.Importe, c.Fecha, c.MovimientoId, c.LiquidacionId)).ToList());

    private static LiquidacionPagosDto Dto(LiquidacionPagos l, string? proveedor, IReadOnlyList<string> avisos) => new(l.Id, l.NumeroCompleto, l.Fecha, l.Hasta,
        l.ProveedorId, proveedor, l.ClienteId, l.APagar, l.EntregasCanceladas, l.Compensado, l.Liquido, l.FormaPago.ToString(), l.CuentaBancariaId, l.RemesaId, l.LoteId,
        l.Estado.ToString(), l.MotivoAnulacion,
        l.Lineas.Where(x => x.Importe != 0).Select(x => new LineaLiquidacionPagosDto(x.Tipo.ToString(), x.DocumentoId, x.Documento, x.Importe, x.MovimientoId, x.EntregaId)).ToList(),
        avisos);
}

/// <summary>Cuentas puente de los movimientos que no mueven dinero.</summary>
public static class CuentasPuente
{
    /// <summary>Anticipos a proveedores: cancelar una entrega a cuenta contra su factura (400 a 407).</summary>
    public const string Entregas = "407";

    /// <summary>Partidas pendientes de aplicación: compensar lo que se debe al proveedor con lo que debe él como cliente.</summary>
    public const string Compensaciones = "555";

    /// <summary>Efectos comerciales descontados: la factura cobrada por una remesa al descuento, hasta su vencimiento.</summary>
    public const string Descontados = "4311";

    /// <summary>Deudas por efectos descontados: el riesgo con el banco hasta el vencimiento.</summary>
    public const string Deudas = "5208";
}
