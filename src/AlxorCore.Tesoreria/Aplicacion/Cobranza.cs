using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Terceros.Aplicacion;
using AlxorCore.Tesoreria.Dominio;

namespace AlxorCore.Tesoreria.Aplicacion;

// ---------------------------------------------------------------------------- Anticipos

/// <summary>Vista de una aplicación de anticipo.</summary>
public sealed record AplicacionAnticipoDto(Guid FacturaId, decimal Importe, DateOnly Fecha);

/// <summary>Vista de un anticipo con su saldo.</summary>
public sealed record AnticipoDto(
    Guid Id, Guid ClienteId, DateOnly Fecha, decimal Importe, decimal Aplicado, decimal Disponible, string Estado, string Concepto,
    IReadOnlyList<AplicacionAnticipoDto> Aplicaciones, Guid? FacturaId = null, string? FacturaNumero = null, decimal? BaseFacturada = null,
    decimal DisponibleBase = 0m, string? CodigoIva = null)
{
    public static AnticipoDto Desde(Anticipo a) => new(a.Id, a.ClienteId, a.Fecha, a.Importe, a.Aplicado, a.Disponible, a.Estado.ToString(),
        a.Concepto, a.Aplicaciones.Select(x => new AplicacionAnticipoDto(x.FacturaId, x.Importe, x.Fecha)).ToList(),
        a.FacturaId, a.FacturaNumero, a.BaseFacturada, a.DisponibleBase, a.CodigoIva);
}

/// <summary>Datos para registrar un anticipo de cliente.</summary>
/// <remarks>
/// Con <c>Facturar</c> (lo normal: el IVA se devenga al cobrar el anticipo) se emite la factura del anticipo con el
/// impuesto <c>CodigoIva</c> y el cobro se registra contra ella; <c>Factura</c> la rellena quien la emite.
/// </remarks>
public sealed record RegistrarAnticipoComando(Guid ClienteId, decimal Importe, DateOnly? Fecha = null, string? Concepto = null, string? Metodo = null,
    bool Facturar = false, string? CodigoIva = null, Guid? CuentaBancariaId = null, DatosFacturaAnticipo? Factura = null);

/// <summary>Datos para aplicar un anticipo a una factura. Sin importe, se aplica lo máximo posible.</summary>
public sealed record AplicarAnticipoComando(Guid FacturaId, decimal? Importe = null, DateOnly? Fecha = null);

/// <summary>Repositorio de anticipos.</summary>
public interface IRepositorioAnticipos
{
    void Agregar(Anticipo anticipo);

    /// <summary>El anticipo cuya factura es esta (si la hay).</summary>
    Task<Anticipo?> ObtenerPorFacturaAsync(Guid facturaId, CancellationToken ct = default);

    Task<Anticipo?> ObtenerAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Anticipo>> ListarAsync(Guid? clienteId, CancellationToken ct = default);
}

/// <summary>Caso de uso: registrar un anticipo (entrega a cuenta) de un cliente.</summary>
public sealed class RegistrarAnticipo
{
    private readonly IConsultaClientes _clientes;
    private readonly IRepositorioAnticipos _anticipos;
    private readonly IUnidadDeTrabajoTesoreria _unidadDeTrabajo;
    private readonly IReloj _reloj;

    private readonly ContabilizacionTesoreria? _contabilizacion;

    public RegistrarAnticipo(IConsultaClientes clientes, IRepositorioAnticipos anticipos, IUnidadDeTrabajoTesoreria unidadDeTrabajo, IReloj reloj,
        ContabilizacionTesoreria? contabilizacion = null)
    {
        _contabilizacion = contabilizacion;
        _clientes = clientes;
        _anticipos = anticipos;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<Resultado<AnticipoDto>> EjecutarAsync(Guid empresaId, RegistrarAnticipoComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var cliente = await _clientes.ObtenerAsync(comando.ClienteId, ct).ConfigureAwait(false);
        if (cliente is null)
        {
            return Resultado.Fallo<AnticipoDto>(Error.NoEncontrado("cliente.no_encontrado", "El cliente no existe."));
        }

        var fecha = comando.Fecha ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var anticipo = Anticipo.Registrar(empresaId, comando.ClienteId, comando.Importe, fecha, comando.Concepto, comando.Metodo, _reloj, comando.Factura);
        if (anticipo.EsFallo)
        {
            return Resultado.Fallo<AnticipoDto>(anticipo.Error);
        }

        _anticipos.Agregar(anticipo.Valor);

        // Con factura, el asiento es el de la factura (430 a 438 y 477) y el de su cobro (57x a 430): aquí no hay otro.
        if (comando.Factura is null)
        {
            _contabilizacion?.EncolarAnticipo(anticipo.Valor, cliente.Nombre, anulacion: false);
        }
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        if (_contabilizacion is not null)
        {
            await _contabilizacion.DespacharAsync(ct: ct).ConfigureAwait(false);
        }

        return Resultado.Ok(AnticipoDto.Desde(anticipo.Valor));
    }
}

/// <summary>
/// Caso de uso: aplicar un anticipo a una factura del mismo cliente. Registra un cobro de la factura
/// (con las mismas reglas: sin sobrepago) y anota la aplicación en el anticipo, en la misma transacción.
/// </summary>
/// <summary>
/// Descuentos de anticipos facturados en las facturas finales: comprobar cuánto se puede descontar (antes de emitir),
/// anotarlo (después) y deshacerlo si la factura final se anula.
/// </summary>
public sealed class DescuentosAnticipo
{
    private readonly IRepositorioAnticipos _anticipos;
    private readonly IUnidadDeTrabajoTesoreria _unidad;

    public DescuentosAnticipo(IRepositorioAnticipos anticipos, IUnidadDeTrabajoTesoreria unidad)
    {
        _anticipos = anticipos;
        _unidad = unidad;
    }

    /// <summary>Anticipo y base a descontar (lo pedido o lo que queda).</summary>
    public async Task<Resultado<(AnticipoDto Anticipo, decimal Base)>> PrepararAsync(Guid anticipoId, Guid clienteId, decimal? baseSolicitada, CancellationToken ct = default)
    {
        var anticipo = await _anticipos.ObtenerAsync(anticipoId, ct).ConfigureAwait(false);
        if (anticipo is null)
        {
            return Resultado.Fallo<(AnticipoDto, decimal)>(Error.NoEncontrado("anticipo.no_encontrado", "El anticipo no existe."));
        }

        var b = anticipo.BaseDescontable(clienteId, baseSolicitada);
        return b.EsFallo ? Resultado.Fallo<(AnticipoDto, decimal)>(b.Error) : Resultado.Ok((AnticipoDto.Desde(anticipo), b.Valor));
    }

    public async Task<Resultado> AnotarAsync(Guid facturaId, DateOnly fecha, IReadOnlyList<(Guid AnticipoId, decimal Base, decimal Importe)> descuentos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(descuentos);
        foreach (var (anticipoId, baseDescontada, importe) in descuentos)
        {
            var anticipo = await _anticipos.ObtenerAsync(anticipoId, ct).ConfigureAwait(false);
            if (anticipo is null)
            {
                return Resultado.Fallo(Error.NoEncontrado("anticipo.no_encontrado", "El anticipo no existe."));
            }

            var r = anticipo.AnotarDescuento(facturaId, baseDescontada, importe, fecha);
            if (r.EsFallo)
            {
                return r;
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    /// <summary>Error si la factura es la de un anticipo que ya se ha descontado.</summary>
    public async Task<Error?> ComprobarAnulacionAsync(Guid facturaId, CancellationToken ct = default)
    {
        var anticipo = await _anticipos.ObtenerPorFacturaAsync(facturaId, ct).ConfigureAwait(false);
        return anticipo is not null && anticipo.Aplicado != 0m
            ? Error.Conflicto("anticipo.descontado", $"Es la factura del anticipo y ya está descontado en otra factura: anula antes esa factura.")
            : null;
    }

    /// <summary>La factura de un anticipo se ha anulado: el anticipo queda anulado (su cobro se devuelve aparte).</summary>
    public async Task FacturaAnticipoAnuladaAsync(Guid facturaId, IReloj reloj, CancellationToken ct = default)
    {
        var anticipo = await _anticipos.ObtenerPorFacturaAsync(facturaId, ct).ConfigureAwait(false);
        if (anticipo is not null && anticipo.AnularPorFactura(reloj).EsCorrecto)
        {
            await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        }
    }

    /// <summary>La factura final se ha anulado: sus anticipos vuelven a quedar disponibles.</summary>
    public async Task RevertirAsync(IEnumerable<Guid> anticipos, Guid facturaId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(anticipos);
        var cambios = false;
        foreach (var id in anticipos.Distinct())
        {
            var anticipo = await _anticipos.ObtenerAsync(id, ct).ConfigureAwait(false);
            cambios |= anticipo?.RevertirDescuentos(facturaId, DateOnly.FromDateTime(DateTime.Today)) == true;
        }

        if (cambios)
        {
            await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        }
    }
}

/// <summary>Caso de uso: anular un anticipo registrado por error o devuelto (sin nada aplicado).</summary>
public sealed class AnularAnticipo
{
    private readonly IRepositorioAnticipos _anticipos;
    private readonly IUnidadDeTrabajoTesoreria _unidad;
    private readonly IReloj _reloj;

    private readonly ContabilizacionTesoreria? _contabilizacion;

    public AnularAnticipo(IRepositorioAnticipos anticipos, IUnidadDeTrabajoTesoreria unidad, IReloj reloj, ContabilizacionTesoreria? contabilizacion = null)
    {
        _contabilizacion = contabilizacion;
        _anticipos = anticipos;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<Resultado<AnticipoDto>> EjecutarAsync(Guid id, CancellationToken ct = default)
    {
        var a = await _anticipos.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (a is null)
        {
            return Resultado.Fallo<AnticipoDto>(Error.NoEncontrado("anticipo.no_encontrado", "El anticipo no existe."));
        }

        var r = a.Anular(_reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<AnticipoDto>(r.Error);
        }

        _contabilizacion?.EncolarAnticipo(a, null, anulacion: true);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        if (_contabilizacion is not null)
        {
            await _contabilizacion.DespacharAsync(ct: ct).ConfigureAwait(false);
        }
        return Resultado.Ok(AnticipoDto.Desde(a));
    }
}

public sealed class AplicarAnticipo
{
    private readonly IRepositorioAnticipos _anticipos;
    private readonly IConsultaFacturas _facturas;
    private readonly IRepositorioMovimientos _movimientos;
    private readonly IUnidadDeTrabajoTesoreria _unidadDeTrabajo;
    private readonly IReloj _reloj;

    private readonly ContabilizacionTesoreria? _contabilizacion;

    public AplicarAnticipo(
        IRepositorioAnticipos anticipos, IConsultaFacturas facturas, IRepositorioMovimientos movimientos, IUnidadDeTrabajoTesoreria unidadDeTrabajo, IReloj reloj,
        ContabilizacionTesoreria? contabilizacion = null)
    {
        _contabilizacion = contabilizacion;
        _anticipos = anticipos;
        _facturas = facturas;
        _movimientos = movimientos;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<Resultado<AnticipoDto>> EjecutarAsync(Guid empresaId, Guid anticipoId, AplicarAnticipoComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var anticipo = await _anticipos.ObtenerAsync(anticipoId, ct).ConfigureAwait(false);
        if (anticipo is null)
        {
            return Resultado.Fallo<AnticipoDto>(Error.NoEncontrado("anticipo.no_encontrado", "El anticipo no existe."));
        }

        var factura = await _facturas.ObtenerAsync(comando.FacturaId, ct).ConfigureAwait(false);
        if (factura is null)
        {
            return Resultado.Fallo<AnticipoDto>(Error.NoEncontrado("factura.no_encontrada", "La factura no existe."));
        }

        if (factura.Estado == "Anulada")
        {
            return Resultado.Fallo<AnticipoDto>(Error.Conflicto("factura.anulada", $"La factura {factura.NumeroCompleto} está anulada: no se le puede aplicar el anticipo."));
        }

        var pendiente = Redondeo.Dos(factura.Total - await _movimientos.SumaAsync(TipoDocumentoTesoreria.Factura, factura.Id, ct).ConfigureAwait(false));
        var importe = anticipo.ValidarAplicacion(factura.ClienteId, comando.Importe ?? Math.Min(anticipo.Disponible, pendiente));
        if (importe.EsFallo)
        {
            return Resultado.Fallo<AnticipoDto>(importe.Error);
        }

        var fecha = comando.Fecha ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var cobro = await RegistrarCobro.RegistrarAsync(
            empresaId, TipoDocumentoTesoreria.Factura, factura.Id, SentidoMovimiento.Cobro, importe.Valor, factura.Total, fecha,
            $"Anticipo del {anticipo.Fecha:dd/MM/yyyy}", _movimientos, _unidadDeTrabajo, _reloj, ct,
            antesDeGuardar: m => anticipo.AnotarAplicacion(factura.Id, importe.Valor, fecha, m.Id), contabilizacion: _contabilizacion, aplicacionAnticipo: true).ConfigureAwait(false);
        return cobro.EsFallo ? Resultado.Fallo<AnticipoDto>(cobro.Error) : Resultado.Ok(AnticipoDto.Desde(anticipo));
    }
}

/// <summary>Caso de uso: listar anticipos (de un cliente o todos).</summary>
public sealed class ListarAnticipos
{
    private readonly IRepositorioAnticipos _anticipos;

    public ListarAnticipos(IRepositorioAnticipos anticipos) => _anticipos = anticipos;

    public async Task<IReadOnlyList<AnticipoDto>> EjecutarAsync(Guid? clienteId, CancellationToken ct = default) =>
        (await _anticipos.ListarAsync(clienteId, ct).ConfigureAwait(false)).Select(AnticipoDto.Desde).ToList();
}

// ---------------------------------------------------------------------------- Impagados y reclamaciones

/// <summary>
/// Factura vencida (o con un recibo devuelto) con importe pendiente y su situación de reclamación. Si se devolvió un
/// recibo domiciliado, lleva el motivo SEPA, su fecha y los gastos de la última devolución.
/// </summary>
public sealed record ImpagadoDto(
    Guid FacturaId, string Numero, Guid? ClienteId, string ClienteNombre, DateOnly Vencimiento, int DiasRetraso,
    decimal Total, decimal Pendiente, int? NivelQueToca, int? UltimoNivelReclamado, DateTimeOffset? UltimaReclamacion,
    string? MotivoDevolucion = null, string? DescripcionDevolucion = null, DateOnly? FechaDevolucion = null, decimal GastosDevolucion = 0m)
{
    /// <summary>¿Toca enviar una reclamación de un nivel superior al último enviado?</summary>
    public bool PendienteDeReclamar => NivelQueToca is { } n && (UltimoNivelReclamado ?? 0) < n;
}

/// <summary>Vista de una reclamación registrada, con el texto compuesto para enviar.</summary>
public sealed record ReclamacionDto(
    Guid Id, Guid FacturaId, int Nivel, string Canal, decimal Pendiente, int DiasRetraso, string? Nota, DateTimeOffset RealizadaEn,
    string Asunto, string Texto, string? EmailCliente);

/// <summary>Datos para registrar una reclamación. Sin nivel, se usa el que toca por los días de retraso.</summary>
public sealed record RegistrarReclamacionComando(CanalReclamacion Canal = CanalReclamacion.Email, int? Nivel = null, string? Nota = null);

/// <summary>Repositorio de reclamaciones y de la configuración de niveles.</summary>
public interface IRepositorioReclamaciones
{
    void Agregar(Reclamacion reclamacion);

    Task<IReadOnlyList<Reclamacion>> ListarAsync(IReadOnlyCollection<Guid> facturaIds, CancellationToken ct = default);

    Task<ConfiguracionReclamaciones?> ConfiguracionAsync(CancellationToken ct = default);

    void AgregarConfiguracion(ConfiguracionReclamaciones configuracion);
}

/// <summary>Caso de uso: consultar y cambiar los niveles de reclamación de la empresa.</summary>
public sealed class NivelesReclamacion
{
    private readonly IRepositorioReclamaciones _repo;
    private readonly IUnidadDeTrabajoTesoreria _unidadDeTrabajo;

    public NivelesReclamacion(IRepositorioReclamaciones repo, IUnidadDeTrabajoTesoreria unidadDeTrabajo)
    {
        _repo = repo;
        _unidadDeTrabajo = unidadDeTrabajo;
    }

    public async Task<IReadOnlyList<NivelReclamacion>> ObtenerAsync(CancellationToken ct = default) =>
        (await _repo.ConfiguracionAsync(ct).ConfigureAwait(false))?.Niveles ?? NivelReclamacion.PorDefecto;

    public async Task<Resultado<IReadOnlyList<NivelReclamacion>>> CambiarAsync(Guid empresaId, IReadOnlyList<NivelReclamacion> niveles, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(niveles);
        var actual = await _repo.ConfiguracionAsync(ct).ConfigureAwait(false);
        if (actual is null)
        {
            var nueva = ConfiguracionReclamaciones.Crear(empresaId, niveles);
            if (nueva.EsFallo)
            {
                return Resultado.Fallo<IReadOnlyList<NivelReclamacion>>(nueva.Error);
            }

            _repo.AgregarConfiguracion(nueva.Valor);
        }
        else if (actual.Cambiar(niveles) is { EsFallo: true } r)
        {
            return Resultado.Fallo<IReadOnlyList<NivelReclamacion>>(r.Error);
        }

        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok<IReadOnlyList<NivelReclamacion>>([.. niveles]);
    }
}

/// <summary>Caso de uso: impagados (facturas vencidas con pendiente) y registro de reclamaciones.</summary>
public sealed class GestionImpagados
{
    private readonly IConsultaFacturas _facturas;
    private readonly IConsultaTesoreria _tesoreria;
    private readonly IConsultaClientes _clientes;
    private readonly IRepositorioReclamaciones _reclamaciones;
    private readonly NivelesReclamacion _niveles;
    private readonly IUnidadDeTrabajoTesoreria _unidadDeTrabajo;
    private readonly IReloj _reloj;
    private readonly IRepositorioDevoluciones? _devoluciones;

    public GestionImpagados(
        IConsultaFacturas facturas, IConsultaTesoreria tesoreria, IConsultaClientes clientes, IRepositorioReclamaciones reclamaciones,
        NivelesReclamacion niveles, IUnidadDeTrabajoTesoreria unidadDeTrabajo, IReloj reloj, IRepositorioDevoluciones? devoluciones = null)
    {
        _devoluciones = devoluciones;
        _facturas = facturas;
        _tesoreria = tesoreria;
        _clientes = clientes;
        _reclamaciones = reclamaciones;
        _niveles = niveles;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    private DateOnly Hoy => DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);

    /// <summary>Facturas emitidas, vencidas (o con un recibo devuelto) y con pendiente, de más a menos retraso.</summary>
    public async Task<IReadOnlyList<ImpagadoDto>> ListarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var hoy = Hoy;
        var emitidas = (await _facturas.ListarAsync(empresaId, ct).ConfigureAwait(false))
            .Where(f => f.Estado == "Emitida" && f.Tipo != "Rectificativa" && f.Total > 0)
            .ToList();
        var devoluciones = _devoluciones is null || emitidas.Count == 0
            ? []
            : await _devoluciones.DeDocumentosAsync(TipoDocumentoTesoreria.Factura, emitidas.Select(f => f.Id).ToList(), ct).ConfigureAwait(false);
        var ultimaDevolucion = devoluciones.GroupBy(d => d.DocumentoId).ToDictionary(g => g.Key, g => g.MaxBy(d => d.CreadoEn)!);
        var vencidas = emitidas.Where(f => f.FechaVencimiento < hoy || ultimaDevolucion.ContainsKey(f.Id)).ToList();
        if (vencidas.Count == 0)
        {
            return [];
        }

        var ids = vencidas.Select(f => f.Id).ToList();
        var cobrado = await _tesoreria.LiquidadoPorDocumentosAsync(TipoDocumentoTesoreria.Factura, ids, ct).ConfigureAwait(false);
        var reclamaciones = (await _reclamaciones.ListarAsync(ids, ct).ConfigureAwait(false)).ToLookup(r => r.FacturaId);
        var niveles = await _niveles.ObtenerAsync(ct).ConfigureAwait(false);

        return vencidas
            .Select(f =>
            {
                var pendiente = Redondeo.Dos(f.Total - cobrado.GetValueOrDefault(f.Id));
                var dias = Math.Max(0, hoy.DayNumber - f.FechaVencimiento.DayNumber);
                var ultima = reclamaciones[f.Id].MaxBy(r => r.RealizadaEn);
                var dev = ultimaDevolucion.GetValueOrDefault(f.Id);
                return new ImpagadoDto(f.Id, f.NumeroCompleto, f.ClienteId, f.ClienteNombre, f.FechaVencimiento, dias, f.Total, pendiente,
                    NivelReclamacion.QueToca(niveles, dias)?.Nivel, reclamaciones[f.Id].Select(r => (int?)r.Nivel).Max(), ultima?.RealizadaEn,
                    dev?.Motivo, dev is null ? null : MotivosDevolucionSepa.Describir(dev.Motivo), dev?.Fecha, dev?.Gastos ?? 0m);
            })
            .Where(i => i.Pendiente > 0)
            .OrderByDescending(i => i.DiasRetraso)
            .ToList();
    }

    /// <summary>Registra una reclamación de una factura vencida y devuelve el texto del nivel, listo para enviar.</summary>
    public async Task<Resultado<ReclamacionDto>> ReclamarAsync(Guid empresaId, Guid facturaId, RegistrarReclamacionComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var impagado = (await ListarAsync(empresaId, ct).ConfigureAwait(false)).FirstOrDefault(i => i.FacturaId == facturaId);
        if (impagado is null)
        {
            return Resultado.Fallo<ReclamacionDto>(Error.Conflicto("reclamacion.no_vencida",
                "Solo se reclaman facturas emitidas, vencidas y con importe pendiente."));
        }

        var niveles = await _niveles.ObtenerAsync(ct).ConfigureAwait(false);
        var numeroNivel = comando.Nivel ?? impagado.NivelQueToca ?? 1;
        var nivel = niveles.FirstOrDefault(n => n.Nivel == numeroNivel);
        if (nivel is null)
        {
            return Resultado.Fallo<ReclamacionDto>(Error.Validacion("reclamacion.nivel", $"No existe el nivel de reclamación {numeroNivel}."));
        }

        var reclamacion = Reclamacion.Registrar(empresaId, facturaId, nivel.Nivel, comando.Canal, impagado.Pendiente, impagado.DiasRetraso, comando.Nota, _reloj);
        if (reclamacion.EsFallo)
        {
            return Resultado.Fallo<ReclamacionDto>(reclamacion.Error);
        }

        _reclamaciones.Agregar(reclamacion.Valor);
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);

        var cliente = impagado.ClienteId is { } cid ? await _clientes.ObtenerAsync(cid, ct).ConfigureAwait(false) : null;
        string Componer(string plantilla) =>
            NivelReclamacion.Componer(plantilla, impagado.ClienteNombre, impagado.Numero, impagado.Vencimiento, impagado.Pendiente, impagado.DiasRetraso);
        var r = reclamacion.Valor;
        return Resultado.Ok(new ReclamacionDto(r.Id, r.FacturaId, r.Nivel, r.Canal.ToString(), r.Pendiente, r.DiasRetraso, r.Nota, r.RealizadaEn,
            Componer(nivel.Asunto), Componer(nivel.Texto), cliente?.Email));
    }
}
