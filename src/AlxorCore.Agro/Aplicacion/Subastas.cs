using AlxorCore.Agro.Dominio;
using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Agro.Aplicacion;

public interface IRepositorioSubastas
{
    void Agregar(SesionSubasta sesion);

    Task<SesionSubasta?> ObtenerAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<SesionSubasta>> ListarAsync(Guid empresaId, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);

    Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, CancellationToken ct = default);

    /// <summary>Sesiones abiertas o cerradas con algún lote de estas partidas.</summary>
    Task<IReadOnlyList<SesionSubasta>> ConPartidasAsync(IReadOnlyCollection<Guid> partidaIds, CancellationToken ct = default);
}

// ----------------------------------------------------------------------------- Contratos
public sealed record DatosSesionSubasta(DateOnly? Fecha = null, TipoSubasta Tipo = TipoSubasta.Baja, string? Observaciones = null);

/// <summary>Lote desde una partida: sin kilos, todo lo suelto disponible (lo que no está ya en otro lote vivo).</summary>
public sealed record DatosLoteSubasta(Guid PartidaId, decimal? Kilos = null, int Envases = 0, decimal? PrecioSalida = null, string? Descripcion = null);

public sealed record DatosPujaSubasta(Guid CompradorId, decimal PrecioKg);

/// <summary>Adjudicación: al alza, sin datos, a la mejor puja; si no, al comprador y precio indicados.</summary>
public sealed record DatosAdjudicacionSubasta(Guid? CompradorId = null, decimal? PrecioKg = null);

public sealed record PujaSubastaDto(Guid Id, Guid LoteId, Guid CompradorId, string Comprador, decimal PrecioKg, DateTimeOffset CreadaEn);

public sealed record LoteSubastaDto(Guid Id, int Orden, Guid PartidaId, string? Partida, Guid? AgricultorId, string? Agricultor, Guid ProductoId, string Descripcion,
    decimal Kilos, int Envases, decimal? PrecioSalida, string Estado, Guid? CompradorId, string? Comprador, decimal? PrecioKg, decimal Importe, decimal? MejorPuja,
    Guid? AlbaranId, string? Albaran);

public sealed record TotalSubastaDto(Guid Id, string Nombre, int Lotes, decimal Kilos, decimal Importe, decimal PrecioMedio);

public sealed record SesionSubastaDto(Guid Id, string Numero, DateOnly Fecha, string Tipo, string Estado, string? Observaciones, int Lotes, int Adjudicados, int Desiertos,
    decimal KilosAdjudicados, decimal Importe, decimal PrecioMedio, IReadOnlyList<LoteSubastaDto> DetalleLotes, IReadOnlyList<PujaSubastaDto> Pujas,
    IReadOnlyList<TotalSubastaDto> PorComprador, IReadOnlyList<TotalSubastaDto> PorAgricultor, string? MotivoAnulacion);

public sealed record SesionSubastaResumenDto(Guid Id, string Numero, DateOnly Fecha, string Tipo, string Estado, int Lotes, int Adjudicados, decimal KilosAdjudicados,
    decimal Importe);

/// <summary>Partida con kilos sueltos que aún se pueden subastar.</summary>
public sealed record PartidaSubastableDto(Guid PartidaId, string Codigo, DateOnly Fecha, Guid ProductoId, string? Producto, Guid? AgricultorId, string? Agricultor,
    decimal Suelto, decimal EnLotes, decimal Disponible);

/// <summary>Lote adjudicado de una sesión cerrada (para el resumen de ventas por comprador o por agricultor).</summary>
public sealed record VentaSubastaDto(Guid SesionId, string Sesion, DateOnly Fecha, int Lote, string Partida, Guid? AgricultorId, string? Agricultor, Guid CompradorId,
    string Comprador, string Descripcion, decimal Kilos, decimal PrecioKg, decimal Importe, string? Albaran);

/// <summary>
/// Subasta hortofrutícola (alhóndiga): sesiones con lotes sacados de los kilos sueltos de las partidas, adjudicación a la
/// baja o al alza, cierre con un albarán de venta por comprador (la fruta sale de su partida) y el precio de cada partida
/// para la liquidación al agricultor. Una sesión cerrada se anula entera si nada de lo suyo se ha facturado ni liquidado.
/// </summary>
public sealed class SubastasAgro
{
    /// <summary>Tipo de documento de los movimientos de partida de una subasta (el stock lo mueve su albarán).</summary>
    public const string DocumentoSubasta = "Subasta";

    private readonly IRepositorioSubastas _sesiones;
    private readonly IRepositorioAgro _repo;
    private readonly IUnidadDeTrabajoAgro _unidad;
    private readonly IConsultaClientes _clientes;
    private readonly IConsultaProductos _productos;
    private readonly IReloj _reloj;
    private readonly IDocumentosExpedicion? _documentos;

    public SubastasAgro(IRepositorioSubastas sesiones, IRepositorioAgro repo, IUnidadDeTrabajoAgro unidad, IConsultaClientes clientes, IConsultaProductos productos,
        IReloj reloj, IDocumentosExpedicion? documentos = null)
    {
        _sesiones = sesiones;
        _repo = repo;
        _unidad = unidad;
        _clientes = clientes;
        _productos = productos;
        _reloj = reloj;
        _documentos = documentos;
    }

    private DateOnly Hoy => DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);

    public async Task<IReadOnlyList<SesionSubastaResumenDto>> ListarAsync(Guid empresaId, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default) =>
        (await _sesiones.ListarAsync(empresaId, desde, hasta, ct).ConfigureAwait(false))
            .Select(s =>
            {
                var adjudicados = s.Lotes.Where(l => l.Estado == EstadoLoteSubasta.Adjudicado).ToList();
                return new SesionSubastaResumenDto(s.Id, s.NumeroCompleto, s.Fecha, s.Tipo.ToString(), s.Estado.ToString(), s.Lotes.Count, adjudicados.Count,
                    adjudicados.Sum(l => l.Kilos), adjudicados.Sum(l => l.Importe));
            }).ToList();

    public async Task<SesionSubastaDto?> ObtenerAsync(Guid id, CancellationToken ct = default) =>
        await _sesiones.ObtenerAsync(id, ct).ConfigureAwait(false) is { } s ? await DtoAsync(s, ct).ConfigureAwait(false) : null;

    public async Task<Resultado<SesionSubastaDto>> CrearAsync(Guid empresaId, DatosSesionSubasta datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var fecha = datos.Fecha ?? Hoy;
        await _unidad.BloquearAsync($"agro:subasta:{empresaId}:{fecha.Year}", ct).ConfigureAwait(false);
        var numero = await _sesiones.SiguienteNumeroAsync(empresaId, fecha.Year, ct).ConfigureAwait(false);
        var s = SesionSubasta.Crear(empresaId, numero, fecha, datos.Tipo, datos.Observaciones, _reloj);
        if (s.EsFallo)
        {
            return Resultado.Fallo<SesionSubastaDto>(s.Error);
        }

        _sesiones.Agregar(s.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(s.Valor, ct).ConfigureAwait(false));
    }

    public Task<Resultado<SesionSubastaDto>> CambiarAsync(Guid id, DatosSesionSubasta datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        return ConAsync(id, s => Task.FromResult(s.Cambiar(datos.Fecha ?? s.Fecha, datos.Tipo, datos.Observaciones)), ct);
    }

    /// <summary>Partidas con kilos sueltos sin comprometer en otros lotes vivos.</summary>
    public async Task<IReadOnlyList<PartidaSubastableDto>> SubastablesAsync(Guid empresaId, Guid? productoId, CancellationToken ct = default)
    {
        var partidas = (await _repo.PartidasConSaldoAsync(empresaId, ct).ConfigureAwait(false))
            .Where(p => !p.Anulada && (productoId is null || p.ProductoId == productoId)).ToList();
        var saldos = await SueltoAsync(partidas.Select(p => p.Id).ToList(), ct).ConfigureAwait(false);
        var conSaldo = partidas.Where(p => saldos.GetValueOrDefault(p.Id) > 0m).ToList();
        var comprometido = await ComprometidoAsync(conSaldo.Select(p => p.Id).ToList(), null, ct).ConfigureAwait(false);
        var agricultores = (await _repo.AgricultoresAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(a => a.Id, a => a.Nombre);
        var lista = new List<PartidaSubastableDto>();
        foreach (var p in conSaldo.OrderBy(p => p.Fecha).ThenBy(p => p.Codigo, StringComparer.Ordinal))
        {
            var suelto = saldos[p.Id];
            var enLotes = comprometido.GetValueOrDefault(p.Id);
            if (suelto - enLotes <= 0m)
            {
                continue;
            }

            var producto = await _productos.ObtenerAsync(p.ProductoId, ct).ConfigureAwait(false);
            lista.Add(new PartidaSubastableDto(p.Id, p.Codigo, p.Fecha, p.ProductoId, producto?.Nombre, p.AgricultorId,
                p.AgricultorId is { } a ? agricultores.GetValueOrDefault(a) : null, suelto, enLotes, suelto - enLotes));
        }

        return lista;
    }

    public Task<Resultado<SesionSubastaDto>> AgregarLoteAsync(Guid id, DatosLoteSubasta datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        return ConAsync(id, async s =>
        {
            var partida = await _repo.PartidaAsync(datos.PartidaId, ct).ConfigureAwait(false);
            if (partida is null || partida.Anulada || partida.EmpresaId != s.EmpresaId)
            {
                return Resultado.Fallo(Error.NoEncontrado("partida.no_encontrada", "La partida no existe o está anulada."));
            }

            await _unidad.BloquearAsync($"agro:subasta:partida:{partida.Id}", ct).ConfigureAwait(false);
            var suelto = (await SueltoAsync([partida.Id], ct).ConfigureAwait(false)).GetValueOrDefault(partida.Id);
            var comprometido = (await ComprometidoAsync([partida.Id], s, ct).ConfigureAwait(false)).GetValueOrDefault(partida.Id);
            var disponible = suelto - comprometido;
            var kilos = datos.Kilos ?? disponible;
            if (disponible <= 0m || kilos > disponible)
            {
                return Resultado.Fallo(Error.Conflicto("subasta.sin_kilos",
                    $"La partida {partida.Codigo} tiene {Formato(disponible)} kg sueltos sin subastar (lo paletizado se despaletiza antes)."));
            }

            var producto = await _productos.ObtenerAsync(partida.ProductoId, ct).ConfigureAwait(false);
            var r = s.AgregarLote(partida.Id, partida.Codigo, partida.AgricultorId, partida.ProductoId,
                datos.Descripcion ?? $"{producto?.Nombre ?? "Fruta"} · {partida.Codigo}", kilos, datos.Envases, datos.PrecioSalida);
            return r.EsFallo ? Resultado.Fallo(r.Error) : Resultado.Ok();
        }, ct);
    }

    public Task<Resultado<SesionSubastaDto>> QuitarLoteAsync(Guid id, Guid loteId, CancellationToken ct = default) =>
        ConAsync(id, s => Task.FromResult(s.QuitarLote(loteId)), ct);

    public Task<Resultado<SesionSubastaDto>> PujarAsync(Guid id, Guid loteId, DatosPujaSubasta datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        return ConAsync(id, async s =>
        {
            var comprador = await _clientes.ObtenerAsync(datos.CompradorId, ct).ConfigureAwait(false);
            if (comprador is null)
            {
                return Resultado.Fallo(Error.NoEncontrado("cliente.no_encontrado", "El comprador no existe (es un cliente)."));
            }

            var r = s.Pujar(loteId, comprador.Id, comprador.Nombre, datos.PrecioKg, _reloj);
            return r.EsFallo ? Resultado.Fallo(r.Error) : Resultado.Ok();
        }, ct);
    }

    public Task<Resultado<SesionSubastaDto>> AdjudicarAsync(Guid id, Guid loteId, DatosAdjudicacionSubasta datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        return ConAsync(id, async s =>
        {
            string? nombre = null;
            if (datos.CompradorId is { } compradorId)
            {
                var comprador = await _clientes.ObtenerAsync(compradorId, ct).ConfigureAwait(false);
                if (comprador is null)
                {
                    return Resultado.Fallo(Error.NoEncontrado("cliente.no_encontrado", "El comprador no existe (es un cliente)."));
                }

                nombre = comprador.Nombre;
            }

            var r = s.Adjudicar(loteId, datos.CompradorId, nombre, datos.PrecioKg, _reloj);
            return r.EsFallo ? Resultado.Fallo(r.Error) : Resultado.Ok();
        }, ct);
    }

    public Task<Resultado<SesionSubastaDto>> DesiertoAsync(Guid id, Guid loteId, CancellationToken ct = default) =>
        ConAsync(id, s => Task.FromResult(s.DejarDesierto(loteId)), ct);

    public Task<Resultado<SesionSubastaDto>> DeshacerAsync(Guid id, Guid loteId, CancellationToken ct = default) =>
        ConAsync(id, s => Task.FromResult(s.Deshacer(loteId)), ct);

    /// <summary>
    /// Cierra la sesión: lo pendiente queda desierto, cada comprador recibe un albarán con sus lotes (una línea por lote, al
    /// precio adjudicado) y los kilos salen de sus partidas. Si un albarán falla, se anulan los ya emitidos y no se cierra.
    /// </summary>
    public async Task<Resultado<SesionSubastaDto>> CerrarAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        if (_documentos is null)
        {
            return Resultado.Fallo<SesionSubastaDto>(Error.Validacion("expedicion.sin_albaran", "No se pueden emitir albaranes."));
        }

        await _unidad.BloquearAsync($"agro:subasta:{empresaId}:cierre", ct).ConfigureAwait(false);
        var s = await _sesiones.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (s is null)
        {
            return NoEncontrada();
        }

        var cierre = s.Cerrar(_reloj);
        if (cierre.EsFallo)
        {
            return Resultado.Fallo<SesionSubastaDto>(cierre.Error);
        }

        // La fruta tiene que seguir suelta en sus partidas.
        var adjudicados = s.Lotes.Where(l => l.Estado == EstadoLoteSubasta.Adjudicado).ToList();
        var ids = adjudicados.Select(l => l.PartidaId).Distinct().ToList();
        var suelto = await SueltoAsync(ids, ct).ConfigureAwait(false);
        foreach (var g in adjudicados.GroupBy(l => l.PartidaId))
        {
            if (g.Sum(l => l.Kilos) > suelto.GetValueOrDefault(g.Key))
            {
                return Resultado.Fallo<SesionSubastaDto>(Error.Conflicto("subasta.sin_kilos",
                    $"La partida {g.First().PartidaCodigo} ya no tiene sueltos los {Formato(g.Sum(l => l.Kilos))} kg de sus lotes."));
            }
        }

        var emitidos = new List<Guid>();
        foreach (var g in adjudicados.GroupBy(l => l.CompradorId!.Value))
        {
            var lineas = g.OrderBy(l => l.Orden).Select(l => (l.ProductoId, l.Kilos, (decimal?)l.PrecioKg)).ToList();
            var albaran = await _documentos.EmitirAlbaranDirectoAsync(empresaId, g.Key, s.Fecha, $"Subasta {s.NumeroCompleto}", lineas, ct).ConfigureAwait(false);
            if (albaran.EsFallo)
            {
                foreach (var emitido in emitidos)
                {
                    await _documentos.AnularAlbaranAsync(emitido, $"Cierre de la subasta {s.NumeroCompleto} fallido", ct).ConfigureAwait(false);
                }

                return Resultado.Fallo<SesionSubastaDto>(Error.Validacion(albaran.Error.Codigo, $"{g.First().CompradorNombre}: {albaran.Error.Mensaje}"));
            }

            emitidos.Add(albaran.Valor.Id);
            s.AsignarAlbaran(g.Key, albaran.Valor.Id, albaran.Valor.Numero);
        }

        foreach (var l in adjudicados)
        {
            var m = MovimientoPartida.Crear(empresaId, l.PartidaId, s.Fecha, TipoMovimientoPartida.Expedicion, -l.Kilos, null, DocumentoSubasta, s.Id,
                $"Subasta {s.NumeroCompleto} lote {l.Orden}", _reloj);
            if (m.EsFallo)
            {
                return Resultado.Fallo<SesionSubastaDto>(m.Error);
            }

            _repo.Agregar(m.Valor);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(s, ct).ConfigureAwait(false));
    }

    /// <summary>
    /// Anula la sesión. Cerrada, solo si ninguna de sus partidas está ya liquidada al agricultor: anula sus albaranes (si
    /// alguno está facturado, no se puede) y devuelve los kilos a las partidas.
    /// </summary>
    public async Task<Resultado<SesionSubastaDto>> AnularAsync(Guid empresaId, Guid id, string? motivo, CancellationToken ct = default)
    {
        var s = await _sesiones.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (s is null)
        {
            return NoEncontrada();
        }

        var cerrada = s.Estado == EstadoSesionSubasta.Cerrada;
        var adjudicados = s.Lotes.Where(l => l.Estado == EstadoLoteSubasta.Adjudicado).ToList();
        if (cerrada && await _repo.PartidasEnLiquidacionAsync(adjudicados.Select(l => l.PartidaId).Distinct().ToList(), ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<SesionSubastaDto>(Error.Conflicto("subasta.liquidada",
                "Alguna partida de la sesión ya está en una liquidación al agricultor: anula antes la liquidación."));
        }

        var r = s.Anular(motivo);
        if (r.EsFallo)
        {
            return Resultado.Fallo<SesionSubastaDto>(r.Error);
        }

        if (cerrada)
        {
            foreach (var albaran in adjudicados.Where(l => l.AlbaranId is not null).GroupBy(l => (l.AlbaranId!.Value, l.CompradorId!.Value)))
            {
                var anulado = _documentos is null ? Resultado.Ok()
                    : await _documentos.AnularAlbaranAsync(albaran.Key.Item1, $"Anulación de la subasta {s.NumeroCompleto}: {s.MotivoAnulacion}", ct).ConfigureAwait(false);
                if (anulado.EsFallo)
                {
                    return Resultado.Fallo<SesionSubastaDto>(Error.Conflicto(anulado.Error.Codigo,
                        $"El albarán {albaran.First().AlbaranNumero} no se puede anular: {anulado.Error.Mensaje}"));
                }
            }

            foreach (var l in adjudicados)
            {
                var m = MovimientoPartida.Crear(empresaId, l.PartidaId, Hoy, TipoMovimientoPartida.Anulacion, l.Kilos, null, DocumentoSubasta, s.Id,
                    $"Anulación de la subasta {s.NumeroCompleto} lote {l.Orden}", _reloj);
                if (m.EsFallo)
                {
                    return Resultado.Fallo<SesionSubastaDto>(m.Error);
                }

                _repo.Agregar(m.Valor);
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(s, ct).ConfigureAwait(false));
    }

    /// <summary>Lotes adjudicados de las sesiones cerradas en unas fechas, para el resumen por comprador o por agricultor.</summary>
    public async Task<IReadOnlyList<VentaSubastaDto>> VentasAsync(Guid empresaId, DateOnly? desde, DateOnly? hasta, Guid? compradorId, Guid? agricultorId,
        CancellationToken ct = default)
    {
        var agricultores = (await _repo.AgricultoresAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(a => a.Id, a => a.Nombre);
        return (await _sesiones.ListarAsync(empresaId, desde, hasta, ct).ConfigureAwait(false))
            .Where(s => s.Estado == EstadoSesionSubasta.Cerrada)
            .SelectMany(s => s.Lotes.Where(l => l.Estado == EstadoLoteSubasta.Adjudicado
                    && (compradorId is null || l.CompradorId == compradorId) && (agricultorId is null || l.AgricultorId == agricultorId))
                .Select(l => new VentaSubastaDto(s.Id, s.NumeroCompleto, s.Fecha, l.Orden, l.PartidaCodigo ?? string.Empty, l.AgricultorId,
                    l.AgricultorId is { } a ? agricultores.GetValueOrDefault(a) : null, l.CompradorId!.Value, l.CompradorNombre ?? string.Empty, l.Descripcion,
                    l.Kilos, l.PrecioKg ?? 0m, l.Importe, l.AlbaranNumero)))
            .OrderBy(v => v.Fecha).ThenBy(v => v.Sesion, StringComparer.Ordinal).ThenBy(v => v.Lote).ToList();
    }

    // ------------------------------------------------------------------ Apoyo
    private async Task<Resultado<SesionSubastaDto>> ConAsync(Guid id, Func<SesionSubasta, Task<Resultado>> accion, CancellationToken ct)
    {
        var s = await _sesiones.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (s is null)
        {
            return NoEncontrada();
        }

        var r = await accion(s).ConfigureAwait(false);
        if (r.EsFallo)
        {
            return Resultado.Fallo<SesionSubastaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(s, ct).ConfigureAwait(false));
    }

    private static Resultado<SesionSubastaDto> NoEncontrada() =>
        Resultado.Fallo<SesionSubastaDto>(Error.NoEncontrado("subasta.no_encontrada", "La sesión de subasta no existe."));

    /// <summary>Kilos sueltos (fuera de palés) de cada partida.</summary>
    private async Task<Dictionary<Guid, decimal>> SueltoAsync(IReadOnlyCollection<Guid> partidaIds, CancellationToken ct) =>
        partidaIds.Count == 0 ? []
            : (await _repo.SaldosAsync(partidaIds, ct).ConfigureAwait(false)).Where(x => x.PaleId is null)
                .GroupBy(x => x.PartidaId).ToDictionary(g => g.Key, g => g.Sum(x => x.Kilos));

    /// <summary>Kilos de cada partida en lotes vivos (pendientes o adjudicados) de sesiones abiertas, contando la sesión dada aunque no esté guardada.</summary>
    private async Task<Dictionary<Guid, decimal>> ComprometidoAsync(IReadOnlyCollection<Guid> partidaIds, SesionSubasta? actual, CancellationToken ct)
    {
        var sesiones = (await _sesiones.ConPartidasAsync(partidaIds, ct).ConfigureAwait(false)).Where(s => actual is null || s.Id != actual.Id).ToList();
        if (actual is not null)
        {
            sesiones.Add(actual);
        }

        return sesiones.Where(s => s.Estado == EstadoSesionSubasta.Abierta)
            .SelectMany(s => s.Lotes.Where(l => l.Estado != EstadoLoteSubasta.Desierto && partidaIds.Contains(l.PartidaId)))
            .GroupBy(l => l.PartidaId).ToDictionary(g => g.Key, g => g.Sum(l => l.Kilos));
    }

    private static string Formato(decimal kilos) => AlxorCore.Nucleo.Comun.Redondeo.Formatear(kilos, 3);

    private async Task<SesionSubastaDto> DtoAsync(SesionSubasta s, CancellationToken ct)
    {
        var agricultores = (await _repo.AgricultoresAsync(s.EmpresaId, ct).ConfigureAwait(false)).ToDictionary(a => a.Id, a => a.Nombre);
        var lotes = s.Lotes.OrderBy(l => l.Orden).Select(l => new LoteSubastaDto(l.Id, l.Orden, l.PartidaId, l.PartidaCodigo, l.AgricultorId,
            l.AgricultorId is { } a ? agricultores.GetValueOrDefault(a) : null, l.ProductoId, l.Descripcion, l.Kilos, l.Envases, l.PrecioSalida, l.Estado.ToString(),
            l.CompradorId, l.CompradorNombre, l.PrecioKg, l.Importe, s.MejorPuja(l.Id)?.PrecioKg, l.AlbaranId, l.AlbaranNumero)).ToList();
        var adjudicados = lotes.Where(l => l.Estado == nameof(EstadoLoteSubasta.Adjudicado)).ToList();
        static TotalSubastaDto Total(Guid id, string nombre, IReadOnlyCollection<LoteSubastaDto> l)
        {
            var kilos = l.Sum(x => x.Kilos);
            var importe = l.Sum(x => x.Importe);
            return new TotalSubastaDto(id, nombre, l.Count, kilos, importe, kilos == 0m ? 0m : decimal.Round(importe / kilos, 4));
        }

        var kilos = adjudicados.Sum(l => l.Kilos);
        var importe = adjudicados.Sum(l => l.Importe);
        return new SesionSubastaDto(s.Id, s.NumeroCompleto, s.Fecha, s.Tipo.ToString(), s.Estado.ToString(), s.Observaciones, lotes.Count, adjudicados.Count,
            lotes.Count(l => l.Estado == nameof(EstadoLoteSubasta.Desierto)), kilos, importe, kilos == 0m ? 0m : decimal.Round(importe / kilos, 4), lotes,
            s.Pujas.OrderBy(p => p.CreadaEn).Select(p => new PujaSubastaDto(p.Id, p.LoteId, p.CompradorId, p.CompradorNombre, p.PrecioKg, p.CreadaEn)).ToList(),
            adjudicados.GroupBy(l => l.CompradorId!.Value).Select(g => Total(g.Key, g.First().Comprador ?? string.Empty, g.ToList()))
                .OrderByDescending(t => t.Importe).ToList(),
            adjudicados.GroupBy(l => l.AgricultorId ?? Guid.Empty).Select(g => Total(g.Key, g.First().Agricultor ?? "Sin agricultor", g.ToList()))
                .OrderByDescending(t => t.Importe).ToList(),
            s.MotivoAnulacion);
    }
}
