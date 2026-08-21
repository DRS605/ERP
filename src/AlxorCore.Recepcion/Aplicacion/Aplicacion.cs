using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Recepcion.Dominio;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Recepcion.Aplicacion;

/// <summary>Vista de una factura recibida (no incluye el binario del adjunto).</summary>
public sealed record FacturaRecibidaDto(
    Guid Id, string Origen, DateTimeOffset FechaRecepcion, string? RemitenteCorreo, string? AsuntoCorreo,
    string NombreArchivo, string TipoContenido, long TamanoBytes, string Estado,
    Guid? ProveedorId, string? ProveedorTexto, string? NumeroFactura, DateOnly? FechaFactura,
    decimal? BaseImponible, string? CodigoIva, decimal? PorcentajeIrpf, Guid? GastoId, string? MotivoRechazo)
{
    public static FacturaRecibidaDto Desde(FacturaRecibida f) => new(
        f.Id, f.Origen.ToString(), f.FechaRecepcion, f.RemitenteCorreo, f.AsuntoCorreo,
        f.NombreArchivo, f.TipoContenido, f.Contenido.LongLength, f.Estado.ToString(),
        f.ProveedorId, f.ProveedorTexto, f.NumeroFactura, f.FechaFactura,
        f.BaseImponible, f.CodigoIva, f.PorcentajeIrpf, f.GastoId, f.MotivoRechazo);
}

/// <summary>Contenido descargable de un adjunto.</summary>
public sealed record ContenidoAdjunto(string NombreArchivo, string TipoContenido, byte[] Contenido);

/// <summary>Repositorio de facturas recibidas (escritura).</summary>
public interface IRepositorioFacturasRecibidas
{
    Task<FacturaRecibida?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    void Agregar(FacturaRecibida factura);
}

/// <summary>Consultas de lectura de facturas recibidas.</summary>
public interface IConsultaFacturasRecibidas
{
    Task<FacturaRecibidaDto?> ObtenerAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<FacturaRecibidaDto>> ListarAsync(Guid empresaId, string? estado = null, CancellationToken ct = default);

    Task<ContenidoAdjunto?> ObtenerContenidoAsync(Guid id, CancellationToken ct = default);
}

/// <summary>Unidad de trabajo del módulo Recepción.</summary>
public interface IUnidadDeTrabajoRecepcion : IUnidadDeTrabajo;

// --------------------------------------------------------------------------------------------
//  Puerto de contabilización (la costura que permite crecer a partida doble sin rehacer nada)
// --------------------------------------------------------------------------------------------

/// <summary>Datos con los que se contabiliza una factura de proveedor.</summary>
public sealed record DatosContabilizacion(
    Guid? ProveedorId, string? ProveedorTexto, string Concepto, DateOnly Fecha,
    decimal BaseImponible, string CodigoIva, decimal PorcentajeIrpf);

/// <summary>Resultado de contabilizar: el gasto generado (y, en el futuro, el asiento).</summary>
public sealed record ResultadoContabilizacion(Guid GastoId, Guid? AsientoId = null);

/// <summary>
/// Contabiliza una factura de proveedor. Hoy el único adaptador crea un <c>Gasto</c> con IVA
/// soportado (modo "un libro"); una futura implementación de partida doble generará además el
/// asiento contable. El resto del módulo no cambia: solo se sustituye este adaptador.
/// </summary>
public interface IContabilizador
{
    Task<Resultado<ResultadoContabilizacion>> ContabilizarAsync(Guid empresaId, DatosContabilizacion datos, CancellationToken ct = default);
}

// --------------------------------------------------------------------------------------------
//  Captura por buzón de correo (puerto + secreto). El adaptador IMAP vive en Infraestructura.
// --------------------------------------------------------------------------------------------

/// <summary>Un adjunto de un correo entrante.</summary>
public sealed record AdjuntoCorreo(string Nombre, string TipoContenido, byte[] Contenido);

/// <summary>Un correo entrante con sus adjuntos.</summary>
public sealed record CorreoEntrante(string Remitente, string Asunto, DateTimeOffset Fecha, IReadOnlyList<AdjuntoCorreo> Adjuntos);

/// <summary>Buzón del que se leen las facturas que llegan por correo.</summary>
public interface IBuzonFacturas
{
    /// <summary>Indica si hay un buzón configurado y operativo.</summary>
    bool Configurado { get; }

    /// <summary>Lee los correos nuevos (no procesados) del buzón.</summary>
    Task<IReadOnlyList<CorreoEntrante>> LeerNuevosAsync(CancellationToken ct = default);
}

/// <summary>Almacén de secretos: las credenciales NUNCA se guardan en la base de datos.</summary>
public interface IAlmacenSecretos
{
    string? Obtener(string referencia);
}

// --------------------------------------------------------------------------------------------
//  Comandos
// --------------------------------------------------------------------------------------------

/// <summary>Alta manual de una factura en la bandeja (subiendo el PDF en base64).</summary>
public sealed record RecibirFacturaComando(
    string NombreArchivo, string ContenidoBase64, string? TipoContenido = null,
    string? RemitenteCorreo = null, string? AsuntoCorreo = null);

/// <summary>Validación (revisión humana) de una factura recibida.</summary>
public sealed record ValidarFacturaComando(
    decimal BaseImponible, DateOnly? FechaFactura, Guid? ProveedorId = null, string? ProveedorTexto = null,
    string? NumeroFactura = null, string? CodigoIva = null, decimal PorcentajeIrpf = 0m);

/// <summary>Rechazo de una factura recibida.</summary>
public sealed record RechazarFacturaComando(string? Motivo = null);

// --------------------------------------------------------------------------------------------
//  Casos de uso
// --------------------------------------------------------------------------------------------

/// <summary>Da de alta manualmente una factura en la bandeja de entrada.</summary>
public sealed class RecibirFactura
{
    private readonly IRepositorioFacturasRecibidas _facturas;
    private readonly IUnidadDeTrabajoRecepcion _unidad;
    private readonly IReloj _reloj;

    public RecibirFactura(IRepositorioFacturasRecibidas facturas, IUnidadDeTrabajoRecepcion unidad, IReloj reloj)
    {
        _facturas = facturas;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<Resultado<FacturaRecibidaDto>> EjecutarAsync(Guid empresaId, RecibirFacturaComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        byte[] contenido;
        try
        {
            contenido = Convert.FromBase64String(comando.ContenidoBase64 ?? string.Empty);
        }
        catch (FormatException)
        {
            return Resultado.Fallo<FacturaRecibidaDto>(Error.Validacion("recepcion.base64_invalido", "El contenido del archivo no es un base64 válido."));
        }

        var creada = FacturaRecibida.Recibir(empresaId, OrigenRecepcion.Manual, comando.RemitenteCorreo,
            comando.AsuntoCorreo, comando.NombreArchivo, comando.TipoContenido, contenido, _reloj);
        if (creada.EsFallo)
        {
            return Resultado.Fallo<FacturaRecibidaDto>(creada.Error);
        }

        _facturas.Agregar(creada.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(FacturaRecibidaDto.Desde(creada.Valor));
    }
}

/// <summary>Valida (confirma o corrige) los datos de una factura recibida.</summary>
public sealed class ValidarFactura
{
    private readonly IRepositorioFacturasRecibidas _facturas;
    private readonly IConsultaProveedores _proveedores;
    private readonly IUnidadDeTrabajoRecepcion _unidad;

    public ValidarFactura(IRepositorioFacturasRecibidas facturas, IConsultaProveedores proveedores, IUnidadDeTrabajoRecepcion unidad)
    {
        _facturas = facturas;
        _proveedores = proveedores;
        _unidad = unidad;
    }

    public async Task<Resultado<FacturaRecibidaDto>> EjecutarAsync(Guid empresaId, Guid id, ValidarFacturaComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var factura = await _facturas.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (factura is null)
        {
            return Resultado.Fallo<FacturaRecibidaDto>(Error.NoEncontrado("recepcion.no_encontrada", "No se encontró la factura recibida."));
        }

        var proveedorTexto = comando.ProveedorTexto;
        if (comando.ProveedorId is { } provId)
        {
            var proveedor = await _proveedores.ObtenerAsync(provId, ct).ConfigureAwait(false);
            if (proveedor is null)
            {
                return Resultado.Fallo<FacturaRecibidaDto>(Error.Validacion("recepcion.proveedor_desconocido", "El proveedor indicado no existe."));
            }

            proveedorTexto = proveedor.Nombre;
        }

        var validado = factura.Validar(comando.ProveedorId, proveedorTexto, comando.NumeroFactura,
            comando.FechaFactura, comando.BaseImponible, comando.CodigoIva, comando.PorcentajeIrpf);
        if (validado.EsFallo)
        {
            return Resultado.Fallo<FacturaRecibidaDto>(validado.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(FacturaRecibidaDto.Desde(factura));
    }
}

/// <summary>Contabiliza una factura validada delegando en el <see cref="IContabilizador"/>.</summary>
public sealed class ContabilizarFactura
{
    private readonly IRepositorioFacturasRecibidas _facturas;
    private readonly IContabilizador _contabilizador;
    private readonly IUnidadDeTrabajoRecepcion _unidad;
    private readonly IReloj _reloj;

    public ContabilizarFactura(IRepositorioFacturasRecibidas facturas, IContabilizador contabilizador, IUnidadDeTrabajoRecepcion unidad, IReloj reloj)
    {
        _facturas = facturas;
        _contabilizador = contabilizador;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<Resultado<FacturaRecibidaDto>> EjecutarAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        var factura = await _facturas.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (factura is null)
        {
            return Resultado.Fallo<FacturaRecibidaDto>(Error.NoEncontrado("recepcion.no_encontrada", "No se encontró la factura recibida."));
        }

        if (factura.Estado is not EstadoRecepcion.Validada)
        {
            return Resultado.Fallo<FacturaRecibidaDto>(Error.Conflicto("recepcion.no_validada", "Valida la factura antes de contabilizarla."));
        }

        var concepto = ComponerConcepto(factura);
        var datos = new DatosContabilizacion(factura.ProveedorId, factura.ProveedorTexto, concepto,
            factura.FechaFactura!.Value, factura.BaseImponible!.Value, factura.CodigoIva!, factura.PorcentajeIrpf ?? 0m);

        var contabilizado = await _contabilizador.ContabilizarAsync(empresaId, datos, ct).ConfigureAwait(false);
        if (contabilizado.EsFallo)
        {
            return Resultado.Fallo<FacturaRecibidaDto>(contabilizado.Error);
        }

        var marcada = factura.Contabilizar(contabilizado.Valor.GastoId, _reloj.AhoraUtc);
        if (marcada.EsFallo)
        {
            return Resultado.Fallo<FacturaRecibidaDto>(marcada.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(FacturaRecibidaDto.Desde(factura));
    }

    private static string ComponerConcepto(FacturaRecibida f)
    {
        var proveedor = string.IsNullOrWhiteSpace(f.ProveedorTexto) ? "proveedor" : f.ProveedorTexto;
        return string.IsNullOrWhiteSpace(f.NumeroFactura)
            ? $"Factura de {proveedor}"
            : $"Factura {f.NumeroFactura} · {proveedor}";
    }
}

/// <summary>Rechaza (descarta) una factura recibida.</summary>
public sealed class RechazarFactura
{
    private readonly IRepositorioFacturasRecibidas _facturas;
    private readonly IUnidadDeTrabajoRecepcion _unidad;

    public RechazarFactura(IRepositorioFacturasRecibidas facturas, IUnidadDeTrabajoRecepcion unidad)
    {
        _facturas = facturas;
        _unidad = unidad;
    }

    public async Task<Resultado<FacturaRecibidaDto>> EjecutarAsync(Guid empresaId, Guid id, RechazarFacturaComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var factura = await _facturas.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (factura is null)
        {
            return Resultado.Fallo<FacturaRecibidaDto>(Error.NoEncontrado("recepcion.no_encontrada", "No se encontró la factura recibida."));
        }

        var rechazada = factura.Rechazar(comando.Motivo);
        if (rechazada.EsFallo)
        {
            return Resultado.Fallo<FacturaRecibidaDto>(rechazada.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(FacturaRecibidaDto.Desde(factura));
    }
}

/// <summary>Lista las facturas recibidas de la empresa (opcionalmente por estado).</summary>
public sealed class ListarFacturasRecibidas
{
    private readonly IConsultaFacturasRecibidas _consulta;

    public ListarFacturasRecibidas(IConsultaFacturasRecibidas consulta) => _consulta = consulta;

    public Task<IReadOnlyList<FacturaRecibidaDto>> EjecutarAsync(Guid empresaId, string? estado = null, CancellationToken ct = default)
        => _consulta.ListarAsync(empresaId, estado, ct);
}

/// <summary>Obtiene una factura recibida por id.</summary>
public sealed class ObtenerFacturaRecibida
{
    private readonly IConsultaFacturasRecibidas _consulta;

    public ObtenerFacturaRecibida(IConsultaFacturasRecibidas consulta) => _consulta = consulta;

    public Task<FacturaRecibidaDto?> EjecutarAsync(Guid id, CancellationToken ct = default) => _consulta.ObtenerAsync(id, ct);
}

/// <summary>
/// Procesa el buzón de correo de una empresa: lee los correos nuevos y da de alta en la bandeja
/// una factura por cada adjunto PDF. No contabiliza nada (eso exige validación humana).
/// </summary>
public sealed class ProcesarBuzon
{
    private static readonly string[] ExtensionesFactura = { ".pdf" };
    private readonly IBuzonFacturas _buzon;
    private readonly IRepositorioFacturasRecibidas _facturas;
    private readonly IUnidadDeTrabajoRecepcion _unidad;
    private readonly IReloj _reloj;

    public ProcesarBuzon(IBuzonFacturas buzon, IRepositorioFacturasRecibidas facturas, IUnidadDeTrabajoRecepcion unidad, IReloj reloj)
    {
        _buzon = buzon;
        _facturas = facturas;
        _unidad = unidad;
        _reloj = reloj;
    }

    /// <summary>Devuelve cuántas facturas se dieron de alta.</summary>
    public async Task<int> EjecutarAsync(Guid empresaId, CancellationToken ct = default)
    {
        if (!_buzon.Configurado)
        {
            return 0;
        }

        var correos = await _buzon.LeerNuevosAsync(ct).ConfigureAwait(false);
        var alta = 0;
        foreach (var correo in correos)
        {
            foreach (var adjunto in correo.Adjuntos)
            {
                if (!EsFactura(adjunto.Nombre))
                {
                    continue;
                }

                var creada = FacturaRecibida.Recibir(empresaId, OrigenRecepcion.Correo, correo.Remitente,
                    correo.Asunto, adjunto.Nombre, adjunto.TipoContenido, adjunto.Contenido, _reloj);
                if (creada.EsCorrecto)
                {
                    _facturas.Agregar(creada.Valor);
                    alta++;
                }
            }
        }

        if (alta > 0)
        {
            await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        }

        return alta;
    }

    private static bool EsFactura(string nombre)
    {
        foreach (var ext in ExtensionesFactura)
        {
            if (nombre.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
