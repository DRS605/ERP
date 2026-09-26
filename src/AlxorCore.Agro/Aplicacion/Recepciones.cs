using AlxorCore.Nucleo.Comun;
using AlxorCore.Agro.Dominio;
using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Agro.Aplicacion;

public sealed record PesadaDto(Guid Id, Guid LineaId, int Secuencia, decimal BrutoKg, decimal TaraKg, decimal NetoKg, int Envases, string? Bascula);

public sealed record LineaRecepcionDto(
    Guid Id, int NumeroLinea, Guid ProductoId, string ProductoNombre, Guid? ParcelaId, DateOnly? FechaRecoleccion, Guid? EnvaseProductoId,
    decimal? PrecioEstimadoKg, string? Calibre, decimal NetoKg, int Envases, Guid? PartidaId);

public sealed record RecepcionDto(
    Guid Id, string? Numero, DateOnly Fecha, Guid AgricultorId, string? AgricultorNombre, Guid CampanaId, string? Matricula, string? Conductor,
    string? Observaciones, string Estado, string? MotivoAnulacion, decimal NetoKg, IReadOnlyList<LineaRecepcionDto> Lineas, IReadOnlyList<PesadaDto> Pesadas)
{
    public static RecepcionDto De(Recepcion r, string? agricultor) => new(
        r.Id, r.NumeroCompleto, r.Fecha, r.AgricultorId, agricultor, r.CampanaId, r.Matricula, r.Conductor, r.Observaciones, r.Estado.ToString(),
        r.MotivoAnulacion, r.NetoKg,
        r.Lineas.OrderBy(l => l.NumeroLinea).Select(l => new LineaRecepcionDto(l.Id, l.NumeroLinea, l.ProductoId, l.ProductoNombre, l.ParcelaId,
            l.FechaRecoleccion, l.EnvaseProductoId, l.PrecioEstimadoKg, l.Calibre, l.NetoKg ?? r.NetoDe(l.Id), l.Envases ?? r.EnvasesDe(l.Id), l.PartidaId)).ToList(),
        r.Pesadas.OrderBy(p => p.Secuencia).Select(p => new PesadaDto(p.Id, p.LineaId, p.Secuencia, p.BrutoKg, p.TaraKg, p.NetoKg, p.Envases, p.Bascula)).ToList());
}

public sealed record RecepcionResumenDto(Guid Id, string? Numero, DateOnly Fecha, Guid AgricultorId, string? AgricultorNombre, string Estado, int Lineas, decimal NetoKg);

public sealed record PartidaDto(
    Guid Id, string Codigo, Guid ProductoId, string Origen, DateOnly Fecha, decimal KilosIniciales, decimal Saldo, Guid? AgricultorId, Guid? ParcelaId,
    Guid? CampanaId, string? Calibre, decimal? CosteKg, bool Anulada, Guid? RecepcionId, Guid? ParteConfeccionId);

public sealed record MovimientoPartidaDto(Guid Id, Guid PartidaId, DateOnly Fecha, string Tipo, decimal Kilos, Guid? PaleId, string? DocumentoTipo, Guid? DocumentoId, string? Concepto);

public sealed record SaldoEnvaseDto(Guid EnvaseProductoId, int Saldo);

public sealed record MovimientoEnvaseDto(Guid Id, Guid EnvaseProductoId, DateOnly Fecha, int Cantidad, Guid? RecepcionId, string? Concepto);

public sealed record DatosRecepcion(Guid AgricultorId, DateOnly Fecha, Guid? CampanaId = null, string? Matricula = null, string? Conductor = null, string? Observaciones = null);

public sealed record DatosLinea(Guid ProductoId, Guid? ParcelaId = null, DateOnly? FechaRecoleccion = null, Guid? EnvaseProductoId = null, decimal? PrecioEstimadoKg = null, string? Calibre = null);

public sealed record DatosPesada(decimal BrutoKg, decimal TaraKg, int Envases = 0, string? Bascula = null);

public sealed record DatosMovimientoEnvase(Guid EnvaseProductoId, int Cantidad, DateOnly? Fecha = null, string? Concepto = null);

public sealed record DatosAjustePartida(decimal Kilos, string? Concepto, Guid? PaleId = null, DateOnly? Fecha = null);

/// <summary>Recepciones de fruta, partidas y envases de los agricultores.</summary>
public sealed class RecepcionesAgro
{
    private readonly IRepositorioAgro _repo;
    private readonly IUnidadDeTrabajoAgro _unidad;
    private readonly IConsultaProductos _productos;
    private readonly IReloj _reloj;

    public RecepcionesAgro(IRepositorioAgro repo, IUnidadDeTrabajoAgro unidad, IConsultaProductos productos, IReloj reloj)
    {
        _repo = repo;
        _unidad = unidad;
        _productos = productos;
        _reloj = reloj;
    }

    public static string ClaveNumeracion(Guid empresaId, string serie) => $"alxor.agro.{serie}:{empresaId}";

    public async Task<IReadOnlyList<RecepcionResumenDto>> ListarAsync(Guid empresaId, DateOnly? desde, DateOnly? hasta, Guid? agricultorId, CancellationToken ct = default)
    {
        var lista = await _repo.RecepcionesAsync(empresaId, desde, hasta, agricultorId, ct).ConfigureAwait(false);
        var nombres = (await _repo.AgricultoresAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(a => a.Id, a => a.Nombre);
        return lista.OrderByDescending(r => r.Fecha).ThenByDescending(r => r.Numero ?? int.MaxValue)
            .Select(r => new RecepcionResumenDto(r.Id, r.NumeroCompleto, r.Fecha, r.AgricultorId, nombres.GetValueOrDefault(r.AgricultorId), r.Estado.ToString(), r.Lineas.Count, r.NetoKg))
            .ToList();
    }

    public async Task<RecepcionDto?> ObtenerAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _repo.RecepcionAsync(id, ct).ConfigureAwait(false);
        return r is null ? null : RecepcionDto.De(r, (await _repo.AgricultorAsync(r.AgricultorId, ct).ConfigureAwait(false))?.Nombre);
    }

    public async Task<Resultado<RecepcionDto>> CrearAsync(Guid empresaId, DatosRecepcion datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var agricultor = await _repo.AgricultorAsync(datos.AgricultorId, ct).ConfigureAwait(false);
        if (agricultor is null)
        {
            return Resultado.Fallo<RecepcionDto>(Error.NoEncontrado("agricultor.no_encontrado", "El agricultor no existe."));
        }

        if (agricultor.Bloqueado)
        {
            return Resultado.Fallo<RecepcionDto>(Error.Conflicto("agricultor.bloqueado", $"El agricultor está bloqueado: {agricultor.MotivoBloqueo}"));
        }

        var campana = await CampanaDeAsync(empresaId, datos.CampanaId, datos.Fecha, ct).ConfigureAwait(false);
        if (campana.EsFallo)
        {
            return Resultado.Fallo<RecepcionDto>(campana.Error);
        }

        var r = Recepcion.Crear(empresaId, agricultor.Id, campana.Valor.Id, datos.Fecha, datos.Matricula, datos.Conductor, datos.Observaciones, _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<RecepcionDto>(r.Error);
        }

        _repo.Agregar(r.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(RecepcionDto.De(r.Valor, agricultor.Nombre));
    }

    public async Task<Resultado<RecepcionDto>> AgregarLineaAsync(Guid recepcionId, DatosLinea datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var r = await _repo.RecepcionAsync(recepcionId, ct).ConfigureAwait(false);
        if (r is null)
        {
            return NoEncontrada<RecepcionDto>();
        }

        var producto = await _productos.ObtenerAsync(datos.ProductoId, ct).ConfigureAwait(false);
        if (producto is null)
        {
            return Resultado.Fallo<RecepcionDto>(Error.NoEncontrado("producto.no_encontrado", "El producto no existe."));
        }

        if (!EsKilo(producto.Unidad))
        {
            return Resultado.Fallo<RecepcionDto>(Error.Validacion("recepcion.unidad", $"{producto.Nombre} se mide en «{producto.Unidad}»: la fruta se recibe en kilos (unidad «kg»)."));
        }

        if (datos.ParcelaId is { } parcelaId)
        {
            var parcela = await _repo.ParcelaAsync(parcelaId, ct).ConfigureAwait(false);
            if (parcela is null || parcela.AgricultorId != r.AgricultorId || !parcela.Activa)
            {
                return Resultado.Fallo<RecepcionDto>(Error.Validacion("recepcion.parcela", "La parcela no es del agricultor o está inactiva."));
            }
        }

        if (datos.EnvaseProductoId is { } envaseId && await _productos.ObtenerAsync(envaseId, ct).ConfigureAwait(false) is null)
        {
            return Resultado.Fallo<RecepcionDto>(Error.NoEncontrado("recepcion.envase", "El artículo de envase no existe."));
        }

        var linea = r.AgregarLinea(new DatosLineaRecepcion(producto.Id, producto.Nombre, datos.ParcelaId, datos.FechaRecoleccion, datos.EnvaseProductoId,
            datos.PrecioEstimadoKg, datos.Calibre));
        return await GuardarAsync(r, linea.EsFallo ? Resultado.Fallo(linea.Error) : Resultado.Ok(), ct).ConfigureAwait(false);
    }

    public async Task<Resultado<RecepcionDto>> QuitarLineaAsync(Guid recepcionId, Guid lineaId, CancellationToken ct = default)
    {
        var r = await _repo.RecepcionAsync(recepcionId, ct).ConfigureAwait(false);
        return r is null ? NoEncontrada<RecepcionDto>() : await GuardarAsync(r, r.QuitarLinea(lineaId), ct).ConfigureAwait(false);
    }

    public async Task<Resultado<RecepcionDto>> AgregarPesadaAsync(Guid recepcionId, Guid lineaId, DatosPesada datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var r = await _repo.RecepcionAsync(recepcionId, ct).ConfigureAwait(false);
        if (r is null)
        {
            return NoEncontrada<RecepcionDto>();
        }

        var p = r.AgregarPesada(lineaId, datos.BrutoKg, datos.TaraKg, datos.Envases, datos.Bascula);
        return await GuardarAsync(r, p.EsFallo ? Resultado.Fallo(p.Error) : Resultado.Ok(), ct).ConfigureAwait(false);
    }

    public async Task<Resultado<RecepcionDto>> QuitarPesadaAsync(Guid recepcionId, Guid pesadaId, CancellationToken ct = default)
    {
        var r = await _repo.RecepcionAsync(recepcionId, ct).ConfigureAwait(false);
        return r is null ? NoEncontrada<RecepcionDto>() : await GuardarAsync(r, r.QuitarPesada(pesadaId), ct).ConfigureAwait(false);
    }

    public async Task<Resultado> EliminarAsync(Guid recepcionId, CancellationToken ct = default)
    {
        var r = await _repo.RecepcionAsync(recepcionId, ct).ConfigureAwait(false);
        if (r is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("recepcion.no_encontrada", "La recepción no existe."));
        }

        if (r.Estado != EstadoRecepcion.Borrador)
        {
            return Resultado.Fallo(Error.Conflicto("recepcion.no_borrador", "Solo se elimina un borrador; una recepción confirmada se anula."));
        }

        _repo.Eliminar(r);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    /// <summary>
    /// Confirma la recepción: la numera sin huecos, crea una partida por línea con sus kilos netos y registra
    /// la devolución de los envases en que llega la fruta. Devuelve todos los problemas a la vez.
    /// </summary>
    public async Task<Resultado<RecepcionDto>> ConfirmarAsync(Guid empresaId, Guid recepcionId, CancellationToken ct = default)
    {
        // Primero el bloqueo y luego la lectura: dos confirmaciones a la vez no numeran dos veces la misma recepción.
        await _unidad.BloquearAsync(ClaveNumeracion(empresaId, Recepcion.Serie), ct).ConfigureAwait(false);
        var r = await _repo.RecepcionAsync(recepcionId, ct).ConfigureAwait(false);
        if (r is null)
        {
            return NoEncontrada<RecepcionDto>();
        }

        var errores = r.ErroresConfirmacion().ToList();
        var agricultor = await _repo.AgricultorAsync(r.AgricultorId, ct).ConfigureAwait(false);
        if (agricultor?.Bloqueado == true)
        {
            errores.Add(Error.Conflicto("agricultor.bloqueado", $"El agricultor está bloqueado: {agricultor.MotivoBloqueo}"));
        }

        var campana = await _repo.CampanaAsync(r.CampanaId, ct).ConfigureAwait(false);
        if (campana is null || !campana.Contiene(r.Fecha))
        {
            errores.Add(Error.Validacion("recepcion.fuera_campana", "La fecha de la recepción no está dentro de su campaña."));
        }

        if (errores.Count > 0)
        {
            return Resultado.Fallo<RecepcionDto>(Valoracion.Resumen(errores));
        }

        var numero = await _repo.UltimoNumeroAsync(r.EmpresaId, Recepcion.Serie, r.Ejercicio, ct).ConfigureAwait(false) + 1;

        var partidas = new Dictionary<Guid, Guid>();
        var codigo = $"{Recepcion.Serie}-{r.Ejercicio}-{numero:D6}";
        foreach (var l in r.Lineas)
        {
            var kilos = r.NetoDe(l.Id);
            var partida = Partida.DeRecepcion(r.EmpresaId, r, l, kilos, _reloj);
            partida.AsignarCodigo($"{codigo}/{l.NumeroLinea}");
            _repo.Agregar(partida);
            partidas[l.Id] = partida.Id;
            _repo.Agregar(MovimientoPartida.Crear(r.EmpresaId, partida.Id, r.Fecha, TipoMovimientoPartida.Entrada, kilos, null, "Recepcion", r.Id,
                $"Recepción {codigo}", _reloj).Valor);

            var envases = r.EnvasesDe(l.Id);
            if (envases > 0)
            {
                _repo.Agregar(MovimientoEnvase.Crear(r.EmpresaId, r.AgricultorId, l.EnvaseProductoId!.Value, r.Fecha, -envases, r.Id, $"Recepción {codigo}", _reloj).Valor);
            }
        }

        var confirmada = r.Confirmar(numero, partidas, _reloj);
        return await GuardarAsync(r, confirmada, ct).ConfigureAwait(false);
    }

    /// <summary>Anula una recepción confirmada si sus partidas no se han usado ni liquidado.</summary>
    public async Task<Resultado<RecepcionDto>> AnularAsync(Guid recepcionId, string? motivo, CancellationToken ct = default)
    {
        var r = await _repo.RecepcionAsync(recepcionId, ct).ConfigureAwait(false);
        if (r is null)
        {
            return NoEncontrada<RecepcionDto>();
        }

        var partidas = await _repo.PartidasDeRecepcionAsync(r.Id, ct).ConfigureAwait(false);
        var ids = partidas.Select(p => p.Id).ToList();
        if (await _repo.TieneMovimientosPosterioresAsync(ids, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<RecepcionDto>(Error.Conflicto("recepcion.partidas_usadas", "Sus partidas ya se han confeccionado, paletizado o ajustado: anula antes esos movimientos."));
        }

        if (await _repo.PartidasEnLiquidacionAsync(ids, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<RecepcionDto>(Error.Conflicto("recepcion.liquidada", "La recepción está en una liquidación: elimina o anula antes la liquidación."));
        }

        var anulada = r.Anular(motivo, _reloj);
        if (anulada.EsFallo)
        {
            return Resultado.Fallo<RecepcionDto>(anulada.Error);
        }

        var hoy = DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        foreach (var p in partidas)
        {
            p.Anular();
            _repo.Agregar(MovimientoPartida.Crear(r.EmpresaId, p.Id, hoy, TipoMovimientoPartida.Anulacion, -p.KilosIniciales, null, "Recepcion", r.Id,
                $"Anulación de {r.NumeroCompleto}", _reloj).Valor);
        }

        foreach (var l in r.Lineas.Where(l => l.Envases > 0))
        {
            _repo.Agregar(MovimientoEnvase.Crear(r.EmpresaId, r.AgricultorId, l.EnvaseProductoId!.Value, hoy, l.Envases!.Value, r.Id, $"Anulación de {r.NumeroCompleto}", _reloj).Valor);
        }

        return await GuardarAsync(r, Resultado.Ok(), ct).ConfigureAwait(false);
    }

    // ------------------------------------------------------------------ Partidas
    public async Task<IReadOnlyList<PartidaDto>> ExistenciasAsync(Guid empresaId, CancellationToken ct = default)
    {
        var partidas = await _repo.PartidasConSaldoAsync(empresaId, ct).ConfigureAwait(false);
        var saldos = (await _repo.SaldosAsync(partidas.Select(p => p.Id).ToList(), ct).ConfigureAwait(false))
            .GroupBy(s => s.PartidaId).ToDictionary(g => g.Key, g => g.Sum(s => s.Kilos));
        return partidas.OrderBy(p => p.Fecha).ThenBy(p => p.Codigo, StringComparer.Ordinal).Select(p => PartidaDe(p, saldos.GetValueOrDefault(p.Id))).ToList();
    }

    public async Task<(PartidaDto Partida, IReadOnlyList<SaldoPartida> Saldos, IReadOnlyList<MovimientoPartidaDto> Movimientos)?> PartidaAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _repo.PartidaAsync(id, ct).ConfigureAwait(false);
        if (p is null)
        {
            return null;
        }

        var saldos = await _repo.SaldosAsync([id], ct).ConfigureAwait(false);
        var movimientos = await _repo.MovimientosAsync([id], ct).ConfigureAwait(false);
        return (PartidaDe(p, saldos.Sum(s => s.Kilos)), saldos,
            movimientos.OrderBy(m => m.CreadoEn).Select(m => new MovimientoPartidaDto(m.Id, m.PartidaId, m.Fecha, m.Tipo.ToString(), m.Kilos, m.PaleId, m.DocumentoTipo, m.DocumentoId, m.Concepto)).ToList());
    }

    /// <summary>Merma, destrío retirado o regularización de inventario de una partida (con signo).</summary>
    public async Task<Resultado> AjustarPartidaAsync(Guid partidaId, DatosAjustePartida datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var p = await _repo.PartidaAsync(partidaId, ct).ConfigureAwait(false);
        if (p is null || p.Anulada)
        {
            return Resultado.Fallo(Error.NoEncontrado("partida.no_encontrada", "La partida no existe o está anulada."));
        }

        if (string.IsNullOrWhiteSpace(datos.Concepto))
        {
            return Resultado.Fallo(Error.Validacion("partida.concepto", "Indica el motivo del ajuste."));
        }

        var m = MovimientoPartida.Crear(p.EmpresaId, p.Id, datos.Fecha ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime), TipoMovimientoPartida.Ajuste,
            datos.Kilos, datos.PaleId, "Ajuste", null, datos.Concepto, _reloj);
        if (m.EsFallo)
        {
            return Resultado.Fallo(m.Error);
        }

        var saldo = (await _repo.SaldosAsync([p.Id], ct).ConfigureAwait(false)).Where(s => s.PaleId == datos.PaleId).Sum(s => s.Kilos);
        if (saldo + datos.Kilos < 0m)
        {
            return Resultado.Fallo(Error.Conflicto("partida.saldo_insuficiente", $"La partida solo tiene {Redondeo.Formatear(saldo, 3)} kg ahí."));
        }

        _repo.Agregar(m.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    // ------------------------------------------------------------------ Envases
    public async Task<IReadOnlyList<SaldoEnvaseDto>> SaldoEnvasesAsync(Guid agricultorId, CancellationToken ct = default) =>
        (await _repo.SaldoEnvasesAsync(agricultorId, ct).ConfigureAwait(false)).Select(s => new SaldoEnvaseDto(s.EnvaseProductoId, s.Saldo)).ToList();

    public async Task<IReadOnlyList<MovimientoEnvaseDto>> MovimientosEnvaseAsync(Guid agricultorId, CancellationToken ct = default) =>
        (await _repo.MovimientosEnvaseAsync(agricultorId, ct).ConfigureAwait(false)).OrderByDescending(m => m.Fecha).ThenByDescending(m => m.CreadoEn)
            .Select(m => new MovimientoEnvaseDto(m.Id, m.EnvaseProductoId, m.Fecha, m.Cantidad, m.RecepcionId, m.Concepto)).ToList();

    /// <summary>Entrega (+) o devolución (−) de envases vacíos a un agricultor.</summary>
    public async Task<Resultado> MoverEnvasesAsync(Guid empresaId, Guid agricultorId, DatosMovimientoEnvase datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        if (await _repo.AgricultorAsync(agricultorId, ct).ConfigureAwait(false) is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("agricultor.no_encontrado", "El agricultor no existe."));
        }

        if (await _productos.ObtenerAsync(datos.EnvaseProductoId, ct).ConfigureAwait(false) is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("recepcion.envase", "El artículo de envase no existe."));
        }

        var m = MovimientoEnvase.Crear(empresaId, agricultorId, datos.EnvaseProductoId, datos.Fecha ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime),
            datos.Cantidad, null, datos.Concepto ?? (datos.Cantidad > 0 ? "Entrega de envases vacíos" : "Devolución de envases vacíos"), _reloj);
        if (m.EsFallo)
        {
            return Resultado.Fallo(m.Error);
        }

        _repo.Agregar(m.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    internal static PartidaDto PartidaDe(Partida p, decimal saldo) => new(
        p.Id, p.Codigo, p.ProductoId, p.Origen.ToString(), p.Fecha, p.KilosIniciales, saldo, p.AgricultorId, p.ParcelaId, p.CampanaId, p.Calibre,
        p.CosteKg, p.Anulada, p.RecepcionId, p.ParteConfeccionId);

    internal static bool EsKilo(string? unidad) =>
        string.Equals(unidad?.Trim(), "kg", StringComparison.OrdinalIgnoreCase) || string.Equals(unidad?.Trim(), "kilo", StringComparison.OrdinalIgnoreCase);

    private async Task<Resultado<Campana>> CampanaDeAsync(Guid empresaId, Guid? campanaId, DateOnly fecha, CancellationToken ct)
    {
        if (campanaId is { } id)
        {
            var c = await _repo.CampanaAsync(id, ct).ConfigureAwait(false);
            if (c is null)
            {
                return Resultado.Fallo<Campana>(Error.NoEncontrado("campana.no_encontrada", "La campaña no existe."));
            }

            return c.Contiene(fecha) ? Resultado.Ok(c) : Resultado.Fallo<Campana>(Error.Validacion("recepcion.fuera_campana", "La fecha no está dentro de la campaña."));
        }

        var abierta = (await _repo.CampanasAsync(empresaId, ct).ConfigureAwait(false)).FirstOrDefault(c => c.Contiene(fecha));
        return abierta is null
            ? Resultado.Fallo<Campana>(Error.Validacion("recepcion.sin_campana", $"No hay ninguna campaña que incluya el {fecha:dd/MM/yyyy}."))
            : Resultado.Ok(abierta);
    }

    private async Task<Resultado<RecepcionDto>> GuardarAsync(Recepcion r, Resultado resultado, CancellationToken ct)
    {
        if (resultado.EsFallo)
        {
            return Resultado.Fallo<RecepcionDto>(resultado.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(RecepcionDto.De(r, (await _repo.AgricultorAsync(r.AgricultorId, ct).ConfigureAwait(false))?.Nombre));
    }

    private static Resultado<T> NoEncontrada<T>() => Resultado.Fallo<T>(Error.NoEncontrado("recepcion.no_encontrada", "La recepción no existe."));
}
