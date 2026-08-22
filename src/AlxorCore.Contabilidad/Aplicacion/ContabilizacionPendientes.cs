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
}

/// <summary>Resolutor básico: cuenta genérica de ingresos/gastos, sin reglas.</summary>
public sealed class ResolverCuentasBasico : IResolverCuentas
{
    public Task<string> CuentaResultadoAsync(Guid empresaId, SentidoContable sentido, string? familia, string? tipoTercero, CancellationToken ct = default)
        => Task.FromResult(sentido == SentidoContable.Venta ? PlanBasico.CuentaVentas : PlanBasico.CuentaCompras);
}

/// <summary>Construye y guarda el asiento de partida doble de un documento pendiente.</summary>
public sealed class PosterDocumento
{
    private readonly IRepositorioAsientos _asientos;
    private readonly IRepositorioCuentas _cuentas;
    private readonly IResolverCuentas _resolver;
    private readonly IUnidadDeTrabajoContabilidad _unidad;
    private readonly IReloj _reloj;

    public PosterDocumento(IRepositorioAsientos asientos, IRepositorioCuentas cuentas, IResolverCuentas resolver, IUnidadDeTrabajoContabilidad unidad, IReloj reloj)
    {
        _asientos = asientos; _cuentas = cuentas; _resolver = resolver; _unidad = unidad; _reloj = reloj;
    }

    /// <summary>Genera el asiento del documento con su fecha de registro. No guarda (lo hace el llamador).</summary>
    public async Task<Resultado<Asiento>> ConstruirAsync(DocumentoPendiente doc, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(doc);
        var cuentaResultado = await _resolver.CuentaResultadoAsync(doc.EmpresaId, doc.Sentido, doc.Familia, doc.TipoTercero, ct).ConfigureAwait(false);
        var concepto = doc.Referencia + (string.IsNullOrWhiteSpace(doc.TerceroNombre) ? "" : " · " + doc.TerceroNombre);

        // Cuenta del tercero: su subcuenta individual si la tiene asignada; si no, la raíz genérica
        // (430 clientes / 400 proveedores). Así el mayor y el balance muestran el saldo por tercero.
        var cuentaGenerica = doc.Sentido == SentidoContable.Venta ? PlanBasico.CuentaClientes : PlanBasico.CuentaProveedores;
        var cuentaTercero = cuentaGenerica;
        if (doc.TerceroId is Guid terceroId)
        {
            var sub = await _cuentas.ObtenerPorTerceroAsync(doc.EmpresaId, terceroId, ct).ConfigureAwait(false);
            if (sub is not null)
            {
                cuentaTercero = sub.Codigo;
            }
        }

        var lineas = doc.Sentido == SentidoContable.Venta
            ? LineasVenta(doc, cuentaResultado, cuentaTercero, concepto)
            : LineasCompra(doc, cuentaResultado, cuentaTercero, concepto);

        await SembradorPlan.AsegurarAsync(doc.EmpresaId, _cuentas, ct).ConfigureAwait(false);
        var ejercicio = doc.FechaRegistro.Year;
        var numero = await _asientos.SiguienteNumeroAsync(doc.EmpresaId, ejercicio, ct).ConfigureAwait(false);
        var origen = doc.Sentido == SentidoContable.Venta ? "Venta" : "Compra";
        return Asiento.Crear(doc.EmpresaId, ejercicio, numero, doc.FechaRegistro, concepto, origen, lineas, _reloj);
    }

    public void Agregar(Asiento asiento) => _asientos.Agregar(asiento);

    public Task GuardarAsync(CancellationToken ct) => _unidad.GuardarCambiosAsync(ct);

    private static List<LineaAsiento> LineasVenta(DocumentoPendiente d, string cuentaIngreso, string cuentaCliente, string concepto)
    {
        // Debe: cliente (total a cobrar) + retención soportada. Haber: ingreso (base) + IVA repercutido.
        var lineas = new List<LineaAsiento> { new(cuentaCliente, d.Total, 0m, concepto) };
        if (d.RetencionIrpf > 0m)
        {
            lineas.Add(new LineaAsiento(PlanBasico.CuentaRetencionVenta, d.RetencionIrpf, 0m, "Retención IRPF"));
        }

        lineas.Add(new LineaAsiento(cuentaIngreso, 0m, d.BaseImponible, concepto));
        if (d.CuotaIva > 0m)
        {
            lineas.Add(new LineaAsiento(PlanBasico.CuentaIvaRepercutido, 0m, d.CuotaIva, "IVA repercutido"));
        }

        return lineas;
    }

    private static List<LineaAsiento> LineasCompra(DocumentoPendiente d, string cuentaGasto, string cuentaProveedor, string concepto)
    {
        // Debe: gasto (base) + IVA soportado. Haber: retención + proveedores (total).
        var lineas = new List<LineaAsiento> { new(cuentaGasto, d.BaseImponible, 0m, concepto) };
        if (d.CuotaIva > 0m)
        {
            lineas.Add(new LineaAsiento(PlanBasico.CuentaIvaSoportado, d.CuotaIva, 0m, "IVA soportado"));
        }

        if (d.RetencionIrpf > 0m)
        {
            lineas.Add(new LineaAsiento(PlanBasico.CuentaRetencion, 0m, d.RetencionIrpf, "Retención IRPF"));
        }

        lineas.Add(new LineaAsiento(cuentaProveedor, 0m, d.Total, concepto));
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
                _poster.Agregar(asiento.Valor);
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

            _poster.Agregar(asiento.Valor);
            doc.MarcarContabilizado(asiento.Valor.Id);
            contabilizados++;
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(contabilizados);
    }
}
