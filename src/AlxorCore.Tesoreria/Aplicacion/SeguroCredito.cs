using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Terceros.Aplicacion;
using AlxorCore.Tesoreria.Dominio;

namespace AlxorCore.Tesoreria.Aplicacion;

public interface IRepositorioSeguroCredito
{
    void Agregar(object entidad);

    Task<IReadOnlyList<PolizaSeguroCredito>> PolizasAsync(Guid empresaId, CancellationToken ct = default);

    Task<PolizaSeguroCredito?> PolizaAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<ClasificacionSeguro>> ClasificacionesAsync(Guid empresaId, Guid? clienteId = null, CancellationToken ct = default);

    Task<ClasificacionSeguro?> ClasificacionAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<AvisoImpago>> AvisosAsync(Guid empresaId, CancellationToken ct = default);

    Task<AvisoImpago?> AvisoAsync(Guid id, CancellationToken ct = default);
}

public sealed record DatosPoliza(string? Aseguradora, string? NumeroPoliza, decimal PorcentajeCobertura, int PlazoAvisoDias, DateOnly? Desde = null, DateOnly? Hasta = null,
    bool BloquearSinCobertura = false, bool Activa = true);

public sealed record PolizaDto(Guid Id, string Aseguradora, string NumeroPoliza, decimal PorcentajeCobertura, int PlazoAvisoDias, DateOnly Desde, DateOnly? Hasta,
    bool BloquearSinCobertura, bool Activa, bool Vigente)
{
    public static PolizaDto De(PolizaSeguroCredito p, DateOnly hoy) =>
        new(p.Id, p.Aseguradora, p.NumeroPoliza, p.PorcentajeCobertura, p.PlazoAvisoDias, p.Desde, p.Hasta, p.BloquearSinCobertura, p.Activa, p.VigenteEl(hoy));
}

public sealed record DatosSolicitud(Guid PolizaId, Guid ClienteId, decimal Solicitado, string? Referencia = null, DateOnly? Fecha = null);

public sealed record DatosComunicacion(EstadoClasificacion Estado, decimal Concedido = 0m, DateOnly? Efecto = null, DateOnly? Vence = null, string? Nota = null,
    decimal? Solicitado = null, string? Referencia = null);

public sealed record CambioClasificacionDto(DateOnly Fecha, string Estado, decimal Concedido, string? Nota);

/// <summary>Un cliente clasificado: lo concedido hoy frente a su riesgo (pendiente de cobro y de facturar), y lo que cubre el seguro.</summary>
public sealed record ClasificacionDto(Guid Id, Guid PolizaId, string Aseguradora, Guid ClienteId, string Cliente, string? Referencia, string Estado, decimal Solicitado,
    decimal ConcedidoHoy, DateOnly Efecto, DateOnly? Vence, decimal Riesgo, decimal SinCobertura, decimal Indemnizable, int AvisosAbiertos,
    IReadOnlyList<CambioClasificacionDto> Historial);

public sealed record SituacionSeguroDto(decimal Riesgo, decimal Concedido, decimal SinCobertura, decimal Indemnizable, int ClientesSinCobertura,
    IReadOnlyList<ClasificacionDto> Clasificaciones);

/// <summary>Factura impagada de un cliente asegurado: hasta cuándo se puede avisar a la aseguradora.</summary>
public sealed record ImpagoAseguradoDto(Guid FacturaId, string Factura, Guid ClienteId, string Cliente, DateOnly Vencimiento, int DiasRetraso, decimal Pendiente,
    DateOnly LimiteAviso, int DiasParaAvisar, string Situacion, Guid PolizaId);

public sealed record DatosAviso(Guid FacturaId, DateOnly? Fecha = null, string? Referencia = null);

public sealed record DatosCierreAviso(EstadoAvisoImpago Estado, DateOnly? Fecha = null, decimal? Indemnizacion = null, string? Nota = null);

public sealed record AvisoImpagoDto(Guid Id, Guid FacturaId, string Factura, Guid ClienteId, string Cliente, DateOnly Vencimiento, decimal Importe, DateOnly Fecha,
    string? Referencia, string Estado, DateOnly? FechaCierre, decimal? Indemnizacion, string? Nota)
{
    public static AvisoImpagoDto De(AvisoImpago a) => new(a.Id, a.FacturaId, a.Factura, a.ClienteId, a.ClienteNombre, a.Vencimiento, a.Importe, a.Fecha, a.Referencia,
        a.Estado.ToString(), a.FechaCierre, a.Indemnizacion, a.Nota);
}

/// <summary>
/// Seguro de crédito: pólizas, clasificaciones de los clientes (lo que la aseguradora concede, con su historial), la
/// cartera asegurada frente al riesgo de cada cliente y los avisos de impago dentro del plazo de la póliza.
/// </summary>
public sealed class GestionSeguroCredito
{
    /// <summary>Días antes del límite en que un impago pasa a «avisar ya».</summary>
    public const int MargenAviso = 15;

    private readonly IRepositorioSeguroCredito _repo;
    private readonly IUnidadDeTrabajoTesoreria _unidad;
    private readonly IConsultaRiesgo _riesgo;
    private readonly IConsultaClientes _clientes;
    private readonly GestionImpagados _impagados;
    private readonly IReloj _reloj;

    public GestionSeguroCredito(IRepositorioSeguroCredito repo, IUnidadDeTrabajoTesoreria unidad, IConsultaRiesgo riesgo, IConsultaClientes clientes,
        GestionImpagados impagados, IReloj reloj)
    {
        _repo = repo;
        _unidad = unidad;
        _riesgo = riesgo;
        _clientes = clientes;
        _impagados = impagados;
        _reloj = reloj;
    }

    private DateOnly Hoy => DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);

    public async Task<IReadOnlyList<PolizaDto>> PolizasAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.PolizasAsync(empresaId, ct).ConfigureAwait(false)).OrderByDescending(p => p.Desde).Select(p => PolizaDto.De(p, Hoy)).ToList();

    public async Task<Resultado<PolizaDto>> CrearPolizaAsync(Guid empresaId, DatosPoliza d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var p = PolizaSeguroCredito.Crear(empresaId, d.Aseguradora, d.NumeroPoliza, d.PorcentajeCobertura, d.PlazoAvisoDias, d.Desde ?? Hoy, d.Hasta, d.BloquearSinCobertura);
        if (p.EsFallo)
        {
            return Resultado.Fallo<PolizaDto>(p.Error);
        }

        _repo.Agregar(p.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PolizaDto.De(p.Valor, Hoy));
    }

    public async Task<Resultado<PolizaDto>> CambiarPolizaAsync(Guid id, DatosPoliza d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var p = await _repo.PolizaAsync(id, ct).ConfigureAwait(false);
        if (p is null)
        {
            return Resultado.Fallo<PolizaDto>(Error.NoEncontrado("seguro.poliza_no_encontrada", "La póliza no existe."));
        }

        var r = p.Cambiar(d.Aseguradora, d.NumeroPoliza, d.PorcentajeCobertura, d.PlazoAvisoDias, d.Desde ?? p.Desde, d.Hasta, d.BloquearSinCobertura, d.Activa);
        if (r.EsFallo)
        {
            return Resultado.Fallo<PolizaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PolizaDto.De(p, Hoy));
    }

    public async Task<Resultado<ClasificacionDto>> SolicitarAsync(Guid empresaId, DatosSolicitud d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var poliza = await _repo.PolizaAsync(d.PolizaId, ct).ConfigureAwait(false);
        if (poliza is null)
        {
            return Resultado.Fallo<ClasificacionDto>(Error.NoEncontrado("seguro.poliza_no_encontrada", "La póliza no existe."));
        }

        var cliente = await _clientes.ObtenerAsync(d.ClienteId, ct).ConfigureAwait(false);
        if (cliente is null)
        {
            return Resultado.Fallo<ClasificacionDto>(Error.NoEncontrado("cliente.no_encontrado", "El cliente no existe."));
        }

        if ((await _repo.ClasificacionesAsync(empresaId, d.ClienteId, ct).ConfigureAwait(false)).Any(c => c.PolizaId == d.PolizaId))
        {
            return Resultado.Fallo<ClasificacionDto>(Error.Conflicto("seguro.clasificacion_duplicada", $"{cliente.Nombre} ya tiene clasificación en esa póliza: comunica el cambio en ella."));
        }

        var c = ClasificacionSeguro.Solicitar(empresaId, poliza.Id, cliente.Id, cliente.Nombre, d.Solicitado, d.Referencia, d.Fecha ?? Hoy, _reloj);
        if (c.EsFallo)
        {
            return Resultado.Fallo<ClasificacionDto>(c.Error);
        }

        _repo.Agregar(c.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(empresaId, c.Valor, poliza, [], ct).ConfigureAwait(false));
    }

    public async Task<Resultado<ClasificacionDto>> ComunicarAsync(Guid empresaId, Guid id, DatosComunicacion d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var c = await _repo.ClasificacionAsync(id, ct).ConfigureAwait(false);
        if (c is null)
        {
            return Resultado.Fallo<ClasificacionDto>(Error.NoEncontrado("seguro.clasificacion_no_encontrada", "La clasificación no existe."));
        }

        var r = c.Comunicar(d.Estado, d.Concedido, d.Efecto ?? Hoy, d.Vence, d.Nota, d.Solicitado, d.Referencia, _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<ClasificacionDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        var poliza = await _repo.PolizaAsync(c.PolizaId, ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(empresaId, c, poliza!, await _repo.AvisosAsync(empresaId, ct).ConfigureAwait(false), ct).ConfigureAwait(false));
    }

    /// <summary>La cartera asegurada: cada cliente clasificado con su riesgo, lo que queda fuera de cobertura y lo indemnizable.</summary>
    public async Task<SituacionSeguroDto> SituacionAsync(Guid empresaId, CancellationToken ct = default)
    {
        var polizas = (await _repo.PolizasAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(p => p.Id);
        var avisos = await _repo.AvisosAsync(empresaId, ct).ConfigureAwait(false);
        var lista = new List<ClasificacionDto>();
        foreach (var c in (await _repo.ClasificacionesAsync(empresaId, null, ct).ConfigureAwait(false)).OrderBy(c => c.ClienteNombre, StringComparer.CurrentCulture))
        {
            lista.Add(await DtoAsync(empresaId, c, polizas[c.PolizaId], avisos, ct).ConfigureAwait(false));
        }

        return new SituacionSeguroDto(Redondeo.Dos(lista.Sum(c => c.Riesgo)), Redondeo.Dos(lista.Sum(c => c.ConcedidoHoy)), Redondeo.Dos(lista.Sum(c => c.SinCobertura)),
            Redondeo.Dos(lista.Sum(c => c.Indemnizable)), lista.Count(c => c.SinCobertura > 0m), lista);
    }

    /// <summary>Facturas impagadas de clientes asegurados sin aviso abierto, con el límite para avisar a la aseguradora.</summary>
    public async Task<IReadOnlyList<ImpagoAseguradoDto>> ImpagosAsync(Guid empresaId, CancellationToken ct = default)
    {
        var hoy = Hoy;
        var polizas = (await _repo.PolizasAsync(empresaId, ct).ConfigureAwait(false)).Where(p => p.Activa).ToDictionary(p => p.Id);
        var clasificados = (await _repo.ClasificacionesAsync(empresaId, null, ct).ConfigureAwait(false)).Where(c => polizas.ContainsKey(c.PolizaId))
            .GroupBy(c => c.ClienteId).ToDictionary(g => g.Key, g => g.First());
        var conAviso = (await _repo.AvisosAsync(empresaId, ct).ConfigureAwait(false)).Where(a => a.Estado == EstadoAvisoImpago.Comunicado).Select(a => a.FacturaId).ToHashSet();
        return (await _impagados.ListarAsync(empresaId, ct).ConfigureAwait(false))
            .Where(i => i.ClienteId is { } c && clasificados.ContainsKey(c) && !conAviso.Contains(i.FacturaId) && i.Pendiente > 0m && i.Vencimiento < hoy)
            .Select(i =>
            {
                var clasificacion = clasificados[i.ClienteId!.Value];
                var limite = i.Vencimiento.AddDays(polizas[clasificacion.PolizaId].PlazoAvisoDias);
                var dias = limite.DayNumber - hoy.DayNumber;
                return new ImpagoAseguradoDto(i.FacturaId, i.Numero, i.ClienteId.Value, i.ClienteNombre, i.Vencimiento, i.DiasRetraso, i.Pendiente, limite, dias,
                    dias < 0 ? "FueraDePlazo" : dias <= MargenAviso ? "AvisarYa" : "EnPlazo", clasificacion.PolizaId);
            })
            .OrderBy(i => i.DiasParaAvisar).ToList();
    }

    public async Task<IReadOnlyList<AvisoImpagoDto>> AvisosAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.AvisosAsync(empresaId, ct).ConfigureAwait(false)).OrderByDescending(a => a.Fecha).Select(AvisoImpagoDto.De).ToList();

    public async Task<Resultado<AvisoImpagoDto>> AvisarAsync(Guid empresaId, DatosAviso d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var impago = (await ImpagosAsync(empresaId, ct).ConfigureAwait(false)).FirstOrDefault(i => i.FacturaId == d.FacturaId);
        if (impago is null)
        {
            return Resultado.Fallo<AvisoImpagoDto>(Error.Validacion("seguro.aviso_sin_impago",
                "La factura no es un impago de un cliente asegurado (o ya tiene un aviso abierto)."));
        }

        var a = AvisoImpago.Comunicar(empresaId, impago.PolizaId, impago.FacturaId, impago.Factura, impago.ClienteId, impago.Cliente, impago.Vencimiento, impago.Pendiente,
            d.Fecha ?? Hoy, d.Referencia);
        if (a.EsFallo)
        {
            return Resultado.Fallo<AvisoImpagoDto>(a.Error);
        }

        _repo.Agregar(a.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(AvisoImpagoDto.De(a.Valor));
    }

    public async Task<Resultado<AvisoImpagoDto>> CerrarAvisoAsync(Guid id, DatosCierreAviso d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var a = await _repo.AvisoAsync(id, ct).ConfigureAwait(false);
        if (a is null)
        {
            return Resultado.Fallo<AvisoImpagoDto>(Error.NoEncontrado("seguro.aviso_no_encontrado", "El aviso no existe."));
        }

        var r = a.Cerrar(d.Estado, d.Fecha ?? Hoy, d.Indemnizacion, d.Nota);
        if (r.EsFallo)
        {
            return Resultado.Fallo<AvisoImpagoDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(AvisoImpagoDto.De(a));
    }

    private async Task<ClasificacionDto> DtoAsync(Guid empresaId, ClasificacionSeguro c, PolizaSeguroCredito p, IReadOnlyList<AvisoImpago> avisos, CancellationToken ct)
    {
        var riesgo = Redondeo.Dos(await _riesgo.RiesgoVivoClienteAsync(empresaId, c.ClienteId, ct).ConfigureAwait(false)
                                  + await _riesgo.PendienteFacturarClienteAsync(empresaId, c.ClienteId, null, ct).ConfigureAwait(false));
        var concedido = p.VigenteEl(Hoy) ? c.ConcedidoEl(Hoy) : 0m;
        var cubierto = Math.Min(riesgo, concedido);
        return new ClasificacionDto(c.Id, c.PolizaId, p.Aseguradora, c.ClienteId, c.ClienteNombre, c.Referencia, c.Estado.ToString(), c.Solicitado, concedido, c.Efecto, c.Vence,
            riesgo, Redondeo.Dos(riesgo - cubierto), Redondeo.Dos(cubierto * p.PorcentajeCobertura / 100m),
            avisos.Count(a => a.ClienteId == c.ClienteId && a.Estado == EstadoAvisoImpago.Comunicado),
            c.Historial.OrderBy(h => h.RegistradoEn).Select(h => new CambioClasificacionDto(h.Fecha, h.Estado.ToString(), h.Concedido, h.Nota)).ToList());
    }
}
