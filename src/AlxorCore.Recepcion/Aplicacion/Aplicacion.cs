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
    decimal? BaseImponible, string? CodigoIva, decimal? PorcentajeIrpf, Guid? GastoId, string? MotivoRechazo,
    Guid? EmpresaOrigenId = null, Guid? FacturaOrigenId = null, IReadOnlyList<AlxorCore.Nucleo.Comun.ConceptoAplicado>? Conceptos = null)
{
    public static FacturaRecibidaDto Desde(FacturaRecibida f) => new(
        f.Id, f.Origen.ToString(), f.FechaRecepcion, f.RemitenteCorreo, f.AsuntoCorreo,
        f.NombreArchivo, f.TipoContenido, f.Contenido.LongLength, f.Estado.ToString(),
        f.ProveedorId, f.ProveedorTexto, f.NumeroFactura, f.FechaFactura,
        f.BaseImponible, f.CodigoIva, f.PorcentajeIrpf, f.GastoId, f.MotivoRechazo, f.EmpresaOrigenId, f.FacturaOrigenId, f.Conceptos);
}

/// <summary>Contenido descargable de un adjunto.</summary>
public sealed record ContenidoAdjunto(string NombreArchivo, string TipoContenido, byte[] Contenido);

/// <summary>Repositorio de facturas recibidas (escritura).</summary>
public interface IRepositorioFacturasRecibidas
{
    Task<FacturaRecibida?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>El espejo de una factura emitida en otra empresa del grupo (null si no se ha reflejado).</summary>
    Task<FacturaRecibida?> ObtenerPorFacturaOrigenAsync(Guid empresaId, Guid facturaOrigenId, CancellationToken ct = default);

    void Agregar(FacturaRecibida factura);
}

/// <summary>Consultas de lectura de facturas recibidas.</summary>
public interface IConsultaFacturasRecibidas
{
    Task<FacturaRecibidaDto?> ObtenerAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<FacturaRecibidaDto>> ListarAsync(Guid empresaId, string? estado = null, CancellationToken ct = default);

    Task<ContenidoAdjunto?> ObtenerContenidoAsync(Guid id, CancellationToken ct = default);

    /// <summary>Facturas recibidas de otras empresas del grupo.</summary>
    Task<IReadOnlyList<FacturaRecibidaDto>> IntragrupoAsync(Guid empresaId, CancellationToken ct = default);
}

/// <summary>Unidad de trabajo del módulo Recepción.</summary>
public interface IUnidadDeTrabajoRecepcion : IUnidadDeTrabajo;

// --------------------------------------------------------------------------------------------
//  Puerto de contabilización (la costura que permite crecer a partida doble sin rehacer nada)
// --------------------------------------------------------------------------------------------

/// <summary>Datos con los que se contabiliza una factura de proveedor.</summary>
public sealed record DatosContabilizacion(
    Guid? ProveedorId, string? ProveedorTexto, string Concepto, DateOnly Fecha,
    decimal BaseImponible, string CodigoIva, decimal PorcentajeIrpf, string? NumeroFactura = null, DateOnly? FechaFactura = null,
    IReadOnlyList<(decimal Base, string? Cuenta, string? Descripcion)>? Lineas = null, DatosRectificacionRecibida? Rectificacion = null,
    IReadOnlyList<(decimal Importe, string Cuenta, string? Descripcion)>? Suplidos = null);

/// <summary>Una factura rectificativa recibida (abono del proveedor): base en negativo y la factura que rectifica.</summary>
public sealed record DatosRectificacionRecibida(Guid? RectificaGastoId, string? NumeroRectificado, DateOnly? FechaRectificada, string? Motivo);

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
    string? NumeroFactura = null, string? CodigoIva = null, decimal PorcentajeIrpf = 0m,
    IReadOnlyList<AlxorCore.Catalogo.Aplicacion.ConceptoSolicitado>? Conceptos = null);

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
    private readonly AlxorCore.Catalogo.Aplicacion.IResolverConceptos? _conceptos;

    public ValidarFactura(IRepositorioFacturasRecibidas facturas, IConsultaProveedores proveedores, IUnidadDeTrabajoRecepcion unidad,
        AlxorCore.Catalogo.Aplicacion.IResolverConceptos? conceptos = null)
    {
        _conceptos = conceptos;
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

        // Cargos y abonos elegidos (solo los pedidos: el importe de la factura ya es el que cobra el proveedor).
        var conceptos = new List<AlxorCore.Nucleo.Comun.ConceptoAplicado>();
        if (comando.Conceptos is { Count: > 0 } pedidos && _conceptos is not null)
        {
            var r = await _conceptos.ResolverAsync(AlxorCore.Nucleo.Comun.AmbitoConcepto.Compras, comando.ProveedorId,
                [new AlxorCore.Catalogo.Aplicacion.LineaConceptos(null, 1m, comando.BaseImponible, pedidos)], null, false,
                new AlxorCore.Catalogo.Aplicacion.ContextoConceptos(null, comando.FechaFactura), ct).ConfigureAwait(false);
            if (r.EsFallo)
            {
                return Resultado.Fallo<FacturaRecibidaDto>(r.Error);
            }

            conceptos.AddRange(r.Valor[0].Where(c => c.Efecto != AlxorCore.Nucleo.Comun.EfectoConcepto.Coste));
        }

        factura.FijarConceptos(conceptos);

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
        // Los cargos y abonos de importe con cuenta van en su propia línea (a su cuenta); los demás, en la base; los
        // suplidos, aparte (sin impuesto).
        var precio = factura.Conceptos.Where(c => c.Efecto == AlxorCore.Nucleo.Comun.EfectoConcepto.Precio).ToList();
        var propios = precio.Where(c => !string.IsNullOrWhiteSpace(c.CuentaContable))
            .Select(c => (c.Importe, (string?)c.CuentaContable, (string?)c.Nombre)).ToList();
        var baseResto = AlxorCore.Nucleo.Comun.Redondeo.Dos(factura.BaseImponible!.Value + precio.Where(c => string.IsNullOrWhiteSpace(c.CuentaContable)).Sum(c => c.Importe));
        var suplidos = factura.Conceptos.Where(c => c.Efecto == AlxorCore.Nucleo.Comun.EfectoConcepto.Suplido && c.Importe != 0m)
            .Select(c => (c.Importe, c.CuentaContable ?? "4709", (string?)c.Nombre)).ToList();
        IReadOnlyList<(decimal Base, string? Cuenta, string? Descripcion)>? lineas = propios.Count > 0 ? [(baseResto, null, concepto), .. propios] : null;
        var datos = new DatosContabilizacion(factura.ProveedorId, factura.ProveedorTexto, concepto,
            factura.FechaFactura!.Value, AlxorCore.Nucleo.Comun.Redondeo.Dos(baseResto + propios.Sum(p => p.Importe)), factura.CodigoIva!, factura.PorcentajeIrpf ?? 0m,
            factura.NumeroFactura, factura.FechaFactura, lineas, Suplidos: suplidos.Count > 0 ? suplidos : null);

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

/// <summary>Datos de la factura que otra empresa del grupo emite a esta.</summary>
public sealed record FacturaIntragrupo(Guid EmpresaOrigenId, string EmpresaOrigenNombre, Guid FacturaOrigenId, string NumeroFactura, DateOnly Fecha,
    decimal? BaseImponible, string? CodigoIva, decimal PorcentajeIrpf, Guid? ProveedorId, string NombreArchivo, byte[] Pdf, string? Nota);

/// <summary>
/// Caso de uso: dejar en la bandeja de entrada la factura que otra empresa del grupo ha emitido a esta (el espejo).
/// Es idempotente: si ya se reflejó, devuelve la que hay.
/// </summary>
public sealed class RecibirFacturaIntragrupo
{
    private readonly IRepositorioFacturasRecibidas _facturas;
    private readonly IUnidadDeTrabajoRecepcion _unidad;
    private readonly IReloj _reloj;

    public RecibirFacturaIntragrupo(IRepositorioFacturasRecibidas facturas, IUnidadDeTrabajoRecepcion unidad, IReloj reloj)
    {
        _facturas = facturas;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<Resultado<FacturaRecibidaDto>> EjecutarAsync(Guid empresaId, FacturaIntragrupo datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        if (await _facturas.ObtenerPorFacturaOrigenAsync(empresaId, datos.FacturaOrigenId, ct).ConfigureAwait(false) is { } existente)
        {
            return Resultado.Ok(FacturaRecibidaDto.Desde(existente));
        }

        var f = FacturaRecibida.RecibirIntragrupo(empresaId, datos.EmpresaOrigenId, datos.FacturaOrigenId, datos.EmpresaOrigenNombre, datos.NombreArchivo, datos.Pdf,
            datos.ProveedorId, datos.NumeroFactura, datos.Fecha, datos.BaseImponible, datos.CodigoIva, datos.PorcentajeIrpf, datos.Nota, _reloj);
        if (f.EsFallo)
        {
            return Resultado.Fallo<FacturaRecibidaDto>(f.Error);
        }

        _facturas.Agregar(f.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(FacturaRecibidaDto.Desde(f.Valor));
    }

    /// <summary>
    /// La factura de origen se anuló: su espejo se rechaza si aún no se ha contabilizado. Si ya lo está, se devuelve
    /// tal cual (la empresa receptora debe anular su gasto) y el cuadre intragrupo lo muestra.
    /// </summary>
    public async Task<Resultado<FacturaRecibidaDto?>> AnularOrigenAsync(Guid empresaId, Guid facturaOrigenId, string motivo, CancellationToken ct = default)
    {
        var f = await _facturas.ObtenerPorFacturaOrigenAsync(empresaId, facturaOrigenId, ct).ConfigureAwait(false);
        if (f is null)
        {
            return Resultado.Ok<FacturaRecibidaDto?>(null);
        }

        if (f.Estado is EstadoRecepcion.Recibida or EstadoRecepcion.Validada)
        {
            f.Rechazar(motivo);
            await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        }

        return Resultado.Ok<FacturaRecibidaDto?>(FacturaRecibidaDto.Desde(f));
    }
}
