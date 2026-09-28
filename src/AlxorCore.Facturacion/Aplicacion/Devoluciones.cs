using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Facturacion.Dominio;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Facturacion.Aplicacion;

// ----------------------------------------------------------------------------- Puertos
public interface IRepositorioDevolucionesVenta
{
    void Agregar(DevolucionVenta devolucion);

    Task<DevolucionVenta?> ObtenerAsync(Guid id, CancellationToken ct = default);

    /// <summary>Devoluciones (con seguimiento) de los albaranes indicados, en cualquier estado.</summary>
    Task<IReadOnlyList<DevolucionVenta>> DeAlbaranesAsync(IReadOnlyCollection<Guid> albaranIds, CancellationToken ct = default);

    Task<IReadOnlyList<DevolucionVenta>> DeFacturaAsync(Guid facturaId, CancellationToken ct = default);

    Task<IReadOnlyList<DevolucionVenta>> ListarAsync(Guid empresaId, FiltroDevoluciones filtro, CancellationToken ct = default);

    Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, CancellationToken ct = default);

    /// <summary>Rectificativa vigente (no anulada) que corrige a la factura, si la hay.</summary>
    Task<Guid?> RectificativaDeAsync(Guid facturaId, CancellationToken ct = default);
}

public interface IRepositorioReclamaciones
{
    void Agregar(ReclamacionVenta reclamacion);

    Task<ReclamacionVenta?> ObtenerAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<ReclamacionVenta>> ListarAsync(Guid empresaId, FiltroReclamaciones filtro, CancellationToken ct = default);

    Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, CancellationToken ct = default);

    void Agregar(ConceptoReclamacion concepto);

    Task<ConceptoReclamacion?> ConceptoAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<ConceptoReclamacion>> ConceptosAsync(CancellationToken ct = default);

    Task<bool> CodigoConceptoUsadoAsync(string codigo, Guid? salvo, CancellationToken ct = default);
}

// ----------------------------------------------------------------------------- Contratos
public sealed record FiltroDevoluciones(Guid? ClienteId = null, Guid? AlbaranId = null, EstadoDevolucionVenta? Estado = null, DateOnly? Desde = null, DateOnly? Hasta = null);

public sealed record LineaDevolucionDto(int OrdenAlbaran, Guid? ProductoId, string Descripcion, decimal Cantidad, decimal PrecioUnitario, decimal PorcentajeDescuento,
    string CodigoIva, bool Reingresa, decimal Base);

public sealed record DevolucionVentaDto(Guid Id, string Numero, DateOnly Fecha, Guid AlbaranId, string Albaran, Guid ClienteId, string Cliente, string Motivo,
    Guid? ReclamacionId, string Estado, string? FormaAbono, Guid? FacturaAbonoId, decimal Base, string? MotivoAnulacion, IReadOnlyList<LineaDevolucionDto> Lineas)
{
    public static DevolucionVentaDto Desde(DevolucionVenta d) => new(d.Id, d.NumeroCompleto, d.Fecha, d.AlbaranId, d.AlbaranNumero, d.ClienteId, d.ClienteNombre, d.Motivo,
        d.ReclamacionId, d.Estado.ToString(), d.FormaAbono?.ToString(), d.FacturaAbonoId, d.Base, d.MotivoAnulacion,
        d.Lineas.Select(l => new LineaDevolucionDto(l.OrdenAlbaran, l.ProductoId, l.Descripcion, l.Cantidad, l.PrecioUnitario, l.PorcentajeDescuento, l.CodigoIva,
            l.Reingresa, l.Base)).ToList());
}

public sealed record LineaDevolucionComando(int OrdenAlbaran, decimal Cantidad, bool Reingresa = true);

public sealed record CrearDevolucionComando(Guid AlbaranId, string Motivo, IReadOnlyList<LineaDevolucionComando> Lineas, DateOnly? Fecha = null, Guid? ReclamacionId = null);

/// <summary>Lo devuelto de un albarán, por línea: para proponer la devolución y mostrar lo que aún se puede devolver.</summary>
public sealed record DevolublesDto(Guid AlbaranId, string Albaran, string Estado, IReadOnlyList<LineaDevolubleDto> Lineas);

public sealed record LineaDevolubleDto(int Orden, Guid? ProductoId, string Descripcion, decimal Entregado, decimal Devuelto, decimal Devolvible, decimal PrecioUnitario);

// ----------------------------------------------------------------------------- Apoyo
/// <summary>Cantidades devueltas por línea de albarán, para descontarlas al facturarlo.</summary>
internal static class DevolucionesAlbaran
{
    public static Dictionary<int, decimal> Devuelto(IEnumerable<DevolucionVenta> devoluciones) =>
        devoluciones.Where(d => d.Estado != EstadoDevolucionVenta.Anulada).SelectMany(d => d.Lineas)
            .GroupBy(l => l.OrdenAlbaran).ToDictionary(g => g.Key, g => g.Sum(l => l.Cantidad));
}

// ----------------------------------------------------------------------------- Casos de uso
/// <summary>
/// Devoluciones de venta: registrar lo que el cliente devuelve de un albarán (con entrada en el almacén si vuelve en
/// buen estado), abonarlo con una rectificativa si el albarán ya estaba facturado —si no, la factura del albarán ya
/// sale con lo devuelto descontado—, cerrarla sin abono o anularla.
/// </summary>
public sealed class GestionDevolucionesVenta
{
    private readonly IRepositorioDevolucionesVenta _devoluciones;
    private readonly IRepositorioAlbaranesVenta _albaranes;
    private readonly IRepositorioFacturas _facturas;
    private readonly EmitirRectificativa _rectificativa;
    private readonly IUnidadDeTrabajoFacturacion _unidad;
    private readonly IReloj _reloj;
    private readonly IStockVentas? _stock;

    public GestionDevolucionesVenta(IRepositorioDevolucionesVenta devoluciones, IRepositorioAlbaranesVenta albaranes, IRepositorioFacturas facturas,
        EmitirRectificativa rectificativa, IUnidadDeTrabajoFacturacion unidad, IReloj reloj, IStockVentas? stock = null)
    {
        _devoluciones = devoluciones;
        _albaranes = albaranes;
        _facturas = facturas;
        _rectificativa = rectificativa;
        _unidad = unidad;
        _reloj = reloj;
        _stock = stock;
    }

    public async Task<IReadOnlyList<DevolucionVentaDto>> ListarAsync(Guid empresaId, FiltroDevoluciones filtro, CancellationToken ct = default) =>
        (await _devoluciones.ListarAsync(empresaId, filtro, ct).ConfigureAwait(false)).Select(DevolucionVentaDto.Desde).ToList();

    public async Task<DevolucionVentaDto?> ObtenerAsync(Guid id, CancellationToken ct = default) =>
        await _devoluciones.ObtenerAsync(id, ct).ConfigureAwait(false) is { } d ? DevolucionVentaDto.Desde(d) : null;

    public async Task<Resultado<DevolublesDto>> DevolublesAsync(Guid albaranId, CancellationToken ct = default)
    {
        var albaran = await _albaranes.ObtenerPorIdAsync(albaranId, ct).ConfigureAwait(false);
        if (albaran is null)
        {
            return Resultado.Fallo<DevolublesDto>(Error.NoEncontrado("albaranventa.no_encontrado", "El albarán no existe."));
        }

        var devuelto = DevolucionesAlbaran.Devuelto(await _devoluciones.DeAlbaranesAsync([albaranId], ct).ConfigureAwait(false));
        return Resultado.Ok(new DevolublesDto(albaran.Id, albaran.NumeroCompleto, albaran.Estado.ToString(), albaran.Lineas.Select(l =>
        {
            var d = devuelto.TryGetValue(l.Orden, out var v) ? v : 0m;
            return new LineaDevolubleDto(l.Orden, l.ProductoId, l.Descripcion, l.Cantidad, d, l.Cantidad - d, l.PrecioUnitario);
        }).ToList()));
    }

    public async Task<Resultado<DevolucionVentaDto>> CrearAsync(Guid empresaId, CrearDevolucionComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var albaran = await _albaranes.ObtenerPorIdAsync(comando.AlbaranId, ct).ConfigureAwait(false);
        if (albaran is null)
        {
            return Resultado.Fallo<DevolucionVentaDto>(Error.NoEncontrado("albaranventa.no_encontrado", "El albarán no existe."));
        }

        var devuelto = DevolucionesAlbaran.Devuelto(await _devoluciones.DeAlbaranesAsync([albaran.Id], ct).ConfigureAwait(false));
        var fecha = comando.Fecha ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var numero = await _devoluciones.SiguienteNumeroAsync(empresaId, fecha.Year, ct).ConfigureAwait(false);
        var d = DevolucionVenta.Crear(empresaId, albaran, numero, fecha, comando.Motivo,
            (comando.Lineas ?? []).Select(l => (l.OrdenAlbaran, l.Cantidad, l.Reingresa)).ToList(), devuelto, _reloj, comando.ReclamacionId);
        if (d.EsFallo)
        {
            return Resultado.Fallo<DevolucionVentaDto>(d.Error);
        }

        _devoluciones.Agregar(d.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        var reingreso = Reingreso(d.Valor);
        if (_stock is not null && reingreso.Count > 0 && albaran.StockDescontado)
        {
            await _stock.DevolverVentaAsync(empresaId, reingreso, $"Devolución {d.Valor.NumeroCompleto} del albarán {albaran.NumeroCompleto}", ct).ConfigureAwait(false);
        }

        return Resultado.Ok(DevolucionVentaDto.Desde(d.Valor));
    }

    /// <summary>
    /// Abona una devolución de un albarán ya facturado: rectificativa (por sustitución) de la factura vigente con las
    /// cantidades devueltas descontadas de sus líneas.
    /// </summary>
    public async Task<Resultado<DevolucionVentaDto>> AbonarAsync(Guid empresaId, Guid id, DateOnly? fecha, CancellationToken ct = default)
    {
        var d = await _devoluciones.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (d is null)
        {
            return Resultado.Fallo<DevolucionVentaDto>(Error.NoEncontrado("devolucion.no_encontrada", "La devolución no existe."));
        }

        if (d.Estado != EstadoDevolucionVenta.Registrada)
        {
            return Resultado.Fallo<DevolucionVentaDto>(Error.Conflicto("devolucion.no_pendiente", "La devolución no está pendiente de abono."));
        }

        var albaran = await _albaranes.ObtenerPorIdAsync(d.AlbaranId, ct).ConfigureAwait(false);
        if (albaran?.FacturaId is not { } facturaAlbaran)
        {
            return Resultado.Fallo<DevolucionVentaDto>(Error.Conflicto("devolucion.albaran_sin_factura",
                "El albarán aún no está facturado: lo devuelto se descontará en su factura."));
        }

        // La factura vigente: la del albarán o la última rectificativa que la sustituye.
        var vigente = facturaAlbaran;
        while (await _devoluciones.RectificativaDeAsync(vigente, ct).ConfigureAwait(false) is { } siguiente)
        {
            vigente = siguiente;
        }

        var factura = await _facturas.ObtenerPorIdAsync(vigente, ct).ConfigureAwait(false);
        if (factura is null || factura.Estado != EstadoFactura.Emitida)
        {
            return Resultado.Fallo<DevolucionVentaDto>(Error.Conflicto("devolucion.factura_no_vigente", "La factura del albarán no está vigente (anulada): no se puede rectificar."));
        }

        if (factura.Lineas.Any(l => l.AnticipoId is not null))
        {
            return Resultado.Fallo<DevolucionVentaDto>(Error.Conflicto("devolucion.factura_con_anticipo",
                "La factura descuenta anticipos: rectifícala a mano desde la factura."));
        }

        // Las líneas del albarán aparecen en la factura en su orden: la k-ésima línea de ese albarán es su línea k.
        var devuelto = d.Lineas.ToDictionary(l => l.OrdenAlbaran, l => l.Cantidad);
        var lineas = new List<LineaComando>();
        var posicion = 0;
        foreach (var l in factura.Lineas.OrderBy(l => l.Orden))
        {
            var cantidad = l.Cantidad;
            if (l.AlbaranVentaId == d.AlbaranId)
            {
                posicion++;
                cantidad -= devuelto.TryGetValue(posicion, out var menos) ? menos : 0m;
            }

            if (cantidad > 0m)
            {
                lineas.Add(new LineaComando(cantidad, l.Descripcion, l.PrecioUnitario, l.CodigoIva, l.PorcentajeDescuento, l.ProductoId, l.CosteUnitario,
                    ConceptosCopiados: l.Conceptos.Count == 0 ? null : l.Conceptos, CuentaContable: l.CuentaContable, AlbaranVentaId: l.AlbaranVentaId, SinSalidaStock: true));
            }
        }

        if (lineas.Count == 0)
        {
            return Resultado.Fallo<DevolucionVentaDto>(Error.Conflicto("devolucion.factura_entera",
                "Se devuelve toda la factura: anúlala en vez de rectificarla."));
        }

        var r = await _rectificativa.EjecutarAsync(empresaId, vigente,
            new EmitirRectificativaComando($"Devolución {d.NumeroCompleto} del albarán {d.AlbaranNumero}: {d.Motivo}", lineas, fecha), ct).ConfigureAwait(false);
        if (r.EsFallo)
        {
            return Resultado.Fallo<DevolucionVentaDto>(r.Error);
        }

        d.Abonar(FormaAbonoDevolucion.Rectificativa, r.Valor.Id, _reloj);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(DevolucionVentaDto.Desde(d));
    }

    public async Task<Resultado<DevolucionVentaDto>> CerrarSinAbonoAsync(Guid id, CancellationToken ct = default)
    {
        var d = await _devoluciones.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (d is null)
        {
            return Resultado.Fallo<DevolucionVentaDto>(Error.NoEncontrado("devolucion.no_encontrada", "La devolución no existe."));
        }

        var r = d.Abonar(FormaAbonoDevolucion.SinAbono, null, _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<DevolucionVentaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(DevolucionVentaDto.Desde(d));
    }

    public async Task<Resultado<DevolucionVentaDto>> AnularAsync(Guid empresaId, Guid id, string? motivo, CancellationToken ct = default)
    {
        var d = await _devoluciones.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (d is null)
        {
            return Resultado.Fallo<DevolucionVentaDto>(Error.NoEncontrado("devolucion.no_encontrada", "La devolución no existe."));
        }

        var r = d.Anular(motivo, _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<DevolucionVentaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        var albaran = await _albaranes.ObtenerPorIdAsync(d.AlbaranId, ct).ConfigureAwait(false);
        var reingreso = Reingreso(d);
        if (_stock is not null && reingreso.Count > 0 && albaran?.StockDescontado == true)
        {
            await _stock.DescontarVentaAsync(empresaId, reingreso, ct).ConfigureAwait(false);
        }

        return Resultado.Ok(DevolucionVentaDto.Desde(d));
    }

    private static List<LineaVenta> Reingreso(DevolucionVenta d) =>
        d.Lineas.Where(l => l.Reingresa && l.ProductoId is not null).Select(l => new LineaVenta(l.ProductoId!.Value, l.Cantidad)).ToList();
}

// ----------------------------------------------------------------------------- Reclamaciones
public sealed record FiltroReclamaciones(Guid? ClienteId = null, EstadoReclamacion? Estado = null, Guid? ConceptoId = null, DateOnly? Desde = null, DateOnly? Hasta = null);

public sealed record ConceptoReclamacionDto(Guid Id, string Codigo, string Nombre, bool Activo);

public sealed record GuardarConceptoReclamacion(string Codigo, string Nombre);

public sealed record ReclamacionDto(Guid Id, string Numero, DateOnly Fecha, Guid ClienteId, string Cliente, Guid? AlbaranId, Guid? FacturaId, Guid ConceptoId,
    string? Concepto, string Descripcion, decimal? ImporteReclamado, string? Responsable, string Estado, string? Resolucion, decimal? ImporteReconocido,
    string? TextoResolucion, DateTimeOffset? ResueltaEn, Guid? DevolucionId, Guid? FacturaAbonoId, int? DiasResolucion);

public sealed record GuardarReclamacion(Guid ClienteId, Guid ConceptoId, string Descripcion, Guid? AlbaranId = null, Guid? FacturaId = null,
    decimal? ImporteReclamado = null, DateOnly? Fecha = null);

public sealed record ResolverReclamacion(ResolucionReclamacion Resolucion, string Texto, decimal? ImporteReconocido = null, Guid? DevolucionId = null,
    Guid? FacturaAbonoId = null);

/// <summary>Informe de reclamaciones: por concepto y por cliente, con lo reclamado, lo reconocido y los días medios de resolución.</summary>
public sealed record InformeReclamacionesDto(int Total, int Abiertas, int Resueltas, decimal Reclamado, decimal Reconocido, decimal? DiasMedios,
    IReadOnlyList<FilaInformeReclamaciones> PorConcepto, IReadOnlyList<FilaInformeReclamaciones> PorCliente);

public sealed record FilaInformeReclamaciones(string Clave, int Numero, int Aceptadas, int Rechazadas, decimal Reclamado, decimal Reconocido, decimal? DiasMedios);

/// <summary>Reclamaciones sobre ventas y su maestro de conceptos.</summary>
public sealed class GestionReclamaciones
{
    private readonly IRepositorioReclamaciones _repo;
    private readonly IRepositorioAlbaranesVenta _albaranes;
    private readonly IRepositorioDevolucionesVenta _devoluciones;
    private readonly IConsultaFacturas _facturas;
    private readonly Terceros.Aplicacion.IConsultaClientes _clientes;
    private readonly IUnidadDeTrabajoFacturacion _unidad;
    private readonly IReloj _reloj;

    public GestionReclamaciones(IRepositorioReclamaciones repo, IRepositorioAlbaranesVenta albaranes, IRepositorioDevolucionesVenta devoluciones,
        IConsultaFacturas facturas, Terceros.Aplicacion.IConsultaClientes clientes, IUnidadDeTrabajoFacturacion unidad, IReloj reloj)
    {
        _repo = repo;
        _albaranes = albaranes;
        _devoluciones = devoluciones;
        _facturas = facturas;
        _clientes = clientes;
        _unidad = unidad;
        _reloj = reloj;
    }

    // ---------------------------------------------------------- conceptos
    public async Task<IReadOnlyList<ConceptoReclamacionDto>> ConceptosAsync(CancellationToken ct = default) =>
        (await _repo.ConceptosAsync(ct).ConfigureAwait(false)).Select(c => new ConceptoReclamacionDto(c.Id, c.Codigo, c.Nombre, c.Activo)).ToList();

    public async Task<Resultado<ConceptoReclamacionDto>> GuardarConceptoAsync(Guid empresaId, Guid? id, GuardarConceptoReclamacion datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        if (await _repo.CodigoConceptoUsadoAsync((datos.Codigo ?? "").Trim().ToUpperInvariant(), id, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<ConceptoReclamacionDto>(Error.Conflicto("reclamacion.concepto_duplicado", "Ya hay un concepto con ese código."));
        }

        ConceptoReclamacion concepto;
        if (id is { } existente)
        {
            concepto = await _repo.ConceptoAsync(existente, ct).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Concepto inexistente.");
            var r = concepto.Cambiar(datos.Codigo, datos.Nombre);
            if (r.EsFallo)
            {
                return Resultado.Fallo<ConceptoReclamacionDto>(r.Error);
            }
        }
        else
        {
            var r = ConceptoReclamacion.Crear(empresaId, datos.Codigo, datos.Nombre);
            if (r.EsFallo)
            {
                return Resultado.Fallo<ConceptoReclamacionDto>(r.Error);
            }

            concepto = r.Valor;
            _repo.Agregar(concepto);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new ConceptoReclamacionDto(concepto.Id, concepto.Codigo, concepto.Nombre, concepto.Activo));
    }

    public async Task<bool> ConceptoExisteAsync(Guid id, CancellationToken ct = default) => await _repo.ConceptoAsync(id, ct).ConfigureAwait(false) is not null;

    public async Task<Resultado> ActivarConceptoAsync(Guid id, bool activo, CancellationToken ct = default)
    {
        var c = await _repo.ConceptoAsync(id, ct).ConfigureAwait(false);
        if (c is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("reclamacion.concepto_no_encontrado", "El concepto no existe."));
        }

        if (activo) c.Reactivar(); else c.DarDeBaja();
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    // ---------------------------------------------------------- reclamaciones
    public async Task<IReadOnlyList<ReclamacionDto>> ListarAsync(Guid empresaId, FiltroReclamaciones filtro, CancellationToken ct = default)
    {
        var conceptos = (await _repo.ConceptosAsync(ct).ConfigureAwait(false)).ToDictionary(c => c.Id, c => c.Nombre);
        return (await _repo.ListarAsync(empresaId, filtro, ct).ConfigureAwait(false)).Select(r => Dto(r, conceptos)).ToList();
    }

    public async Task<ReclamacionDto?> ObtenerAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _repo.ObtenerAsync(id, ct).ConfigureAwait(false);
        return r is null ? null : Dto(r, (await _repo.ConceptosAsync(ct).ConfigureAwait(false)).ToDictionary(c => c.Id, c => c.Nombre));
    }

    public async Task<Resultado<ReclamacionDto>> CrearAsync(Guid empresaId, GuardarReclamacion datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var cliente = await _clientes.ObtenerAsync(datos.ClienteId, ct).ConfigureAwait(false);
        if (cliente is null)
        {
            return Resultado.Fallo<ReclamacionDto>(Error.NoEncontrado("cliente.no_encontrado", "El cliente no existe."));
        }

        var error = await ValidarReferenciasAsync(datos, true, ct).ConfigureAwait(false);
        if (error is not null)
        {
            return Resultado.Fallo<ReclamacionDto>(error);
        }

        var fecha = datos.Fecha ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var numero = await _repo.SiguienteNumeroAsync(empresaId, fecha.Year, ct).ConfigureAwait(false);
        var r = ReclamacionVenta.Crear(empresaId, numero, fecha, cliente.Id, cliente.Nombre, datos.AlbaranId, datos.FacturaId, datos.ConceptoId, datos.Descripcion,
            datos.ImporteReclamado, _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<ReclamacionDto>(r.Error);
        }

        _repo.Agregar(r.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok((await ObtenerAsync(r.Valor.Id, ct).ConfigureAwait(false))!);
    }

    public async Task<Resultado<ReclamacionDto>> CambiarAsync(Guid id, GuardarReclamacion datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        return await ConAsync(id, async r =>
        {
            if (r.ClienteId != datos.ClienteId)
            {
                return Resultado.Fallo(Error.Validacion("reclamacion.cliente", "El cliente de una reclamación no se cambia."));
            }

            var error = await ValidarReferenciasAsync(datos, r.ConceptoId != datos.ConceptoId, ct).ConfigureAwait(false);
            return error is not null ? Resultado.Fallo(error) : r.Cambiar(datos.AlbaranId, datos.FacturaId, datos.ConceptoId, datos.Descripcion, datos.ImporteReclamado);
        }, ct).ConfigureAwait(false);
    }

    public Task<Resultado<ReclamacionDto>> TramitarAsync(Guid id, string? responsable, CancellationToken ct = default) =>
        ConAsync(id, r => Task.FromResult(r.Tramitar(responsable)), ct);

    public Task<Resultado<ReclamacionDto>> ResolverAsync(Guid id, ResolverReclamacion datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        return ConAsync(id, async r =>
        {
            if (datos.DevolucionId is { } dev && (await _devoluciones.ObtenerAsync(dev, ct).ConfigureAwait(false) is not { } devolucion || devolucion.ClienteId != r.ClienteId))
            {
                return Resultado.Fallo(Error.Validacion("reclamacion.devolucion", "La devolución no existe o es de otro cliente."));
            }

            if (datos.FacturaAbonoId is { } fac && (await _facturas.ObtenerAsync(fac, ct).ConfigureAwait(false) is not { } factura || factura.ClienteId != r.ClienteId))
            {
                return Resultado.Fallo(Error.Validacion("reclamacion.factura", "La factura de abono no existe o es de otro cliente."));
            }

            return r.Resolver(datos.Resolucion, datos.ImporteReconocido, datos.Texto, datos.DevolucionId, datos.FacturaAbonoId, _reloj);
        }, ct);
    }

    public Task<Resultado<ReclamacionDto>> ReabrirAsync(Guid id, CancellationToken ct = default) => ConAsync(id, r => Task.FromResult(r.Reabrir()), ct);

    public Task<Resultado<ReclamacionDto>> AnularAsync(Guid id, CancellationToken ct = default) => ConAsync(id, r => Task.FromResult(r.Anular(_reloj)), ct);

    public async Task<InformeReclamacionesDto> InformeAsync(Guid empresaId, FiltroReclamaciones filtro, CancellationToken ct = default)
    {
        var lista = (await ListarAsync(empresaId, filtro, ct).ConfigureAwait(false)).Where(r => r.Estado != nameof(EstadoReclamacion.Anulada)).ToList();
        static decimal? Media(IEnumerable<ReclamacionDto> rs)
        {
            var dias = rs.Where(r => r.DiasResolucion is not null).Select(r => (decimal)r.DiasResolucion!.Value).ToList();
            return dias.Count == 0 ? null : Redondeo.Dos(dias.Average());
        }

        static FilaInformeReclamaciones Fila(string clave, IReadOnlyCollection<ReclamacionDto> rs) => new(clave, rs.Count,
            rs.Count(r => r.Resolucion is nameof(ResolucionReclamacion.Aceptada) or nameof(ResolucionReclamacion.AceptadaParcial)),
            rs.Count(r => r.Resolucion == nameof(ResolucionReclamacion.Rechazada)),
            rs.Sum(r => r.ImporteReclamado ?? 0m), rs.Sum(r => r.ImporteReconocido ?? 0m), Media(rs));

        return new InformeReclamacionesDto(lista.Count, lista.Count(r => r.Estado is nameof(EstadoReclamacion.Abierta) or nameof(EstadoReclamacion.EnTramite)),
            lista.Count(r => r.Estado == nameof(EstadoReclamacion.Resuelta)), lista.Sum(r => r.ImporteReclamado ?? 0m), lista.Sum(r => r.ImporteReconocido ?? 0m), Media(lista),
            lista.GroupBy(r => r.Concepto ?? "—").Select(g => Fila(g.Key, g.ToList())).OrderByDescending(f => f.Numero).ToList(),
            lista.GroupBy(r => r.Cliente).Select(g => Fila(g.Key, g.ToList())).OrderByDescending(f => f.Numero).ToList());
    }

    private async Task<Error?> ValidarReferenciasAsync(GuardarReclamacion datos, bool conceptoNuevo, CancellationToken ct)
    {
        var concepto = await _repo.ConceptoAsync(datos.ConceptoId, ct).ConfigureAwait(false);
        if (concepto is null || (conceptoNuevo && !concepto.Activo))
        {
            return Error.Validacion("reclamacion.concepto", "Elige un concepto de reclamación activo.");
        }

        if (datos.AlbaranId is { } a && (await _albaranes.ObtenerPorIdAsync(a, ct).ConfigureAwait(false) is not { } albaran || albaran.ClienteId != datos.ClienteId))
        {
            return Error.Validacion("reclamacion.albaran", "El albarán no existe o es de otro cliente.");
        }

        if (datos.FacturaId is { } f && (await _facturas.ObtenerAsync(f, ct).ConfigureAwait(false) is not { } factura || factura.ClienteId != datos.ClienteId))
        {
            return Error.Validacion("reclamacion.factura", "La factura no existe o es de otro cliente.");
        }

        return null;
    }

    private async Task<Resultado<ReclamacionDto>> ConAsync(Guid id, Func<ReclamacionVenta, Task<Resultado>> accion, CancellationToken ct)
    {
        var r = await _repo.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (r is null)
        {
            return Resultado.Fallo<ReclamacionDto>(Error.NoEncontrado("reclamacion.no_encontrada", "La reclamación no existe."));
        }

        var hecho = await accion(r).ConfigureAwait(false);
        if (hecho.EsFallo)
        {
            return Resultado.Fallo<ReclamacionDto>(hecho.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok((await ObtenerAsync(id, ct).ConfigureAwait(false))!);
    }

    private static ReclamacionDto Dto(ReclamacionVenta r, Dictionary<Guid, string> conceptos) => new(r.Id, r.NumeroCompleto, r.Fecha, r.ClienteId, r.ClienteNombre,
        r.AlbaranId, r.FacturaId, r.ConceptoId, conceptos.TryGetValue(r.ConceptoId, out var c) ? c : null, r.Descripcion, r.ImporteReclamado, r.Responsable,
        r.Estado.ToString(), r.Resolucion?.ToString(), r.ImporteReconocido, r.TextoResolucion, r.ResueltaEn, r.DevolucionId, r.FacturaAbonoId,
        r.ResueltaEn is { } fin ? DateOnly.FromDateTime(fin.UtcDateTime).DayNumber - r.Fecha.DayNumber : null);
}
