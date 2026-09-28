using AlxorCore.Agro.Dominio;
using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Agro.Aplicacion;

public sealed record SaldoEnvaseCuentaDto(Guid EnvaseProductoId, string Envase, int Saldo);

public sealed record CuentaEnvasesDto(Guid Id, string Tipo, Guid TerceroId, string Nombre, Guid? AgrupadoraId, bool ImputarATransportista, string Bloqueo,
    string? MotivoBloqueo, int? Limite, bool Activa, int SaldoTotal, IReadOnlyList<SaldoEnvaseCuentaDto> Saldos, bool SuperaLimite);

public sealed record CrearCuentaEnvasesComando(TipoCuentaEnvases Tipo, Guid TerceroId, string? Nombre = null);

public sealed record ConfigurarCuentaEnvasesComando(Guid? AgrupadoraId = null, bool ImputarATransportista = false, BloqueoEnvases Bloqueo = BloqueoEnvases.Ninguno,
    string? MotivoBloqueo = null, int? Limite = null, bool Activa = true);

/// <summary>Línea de un movimiento: + lo que se entrega al tercero, − lo que se recoge.</summary>
public sealed record LineaEnvasesComando(Guid EnvaseProductoId, int Cantidad);

public sealed record MovimientoEnvasesComando(Guid CuentaId, IReadOnlyList<LineaEnvasesComando> Lineas, DateOnly? Fecha = null, bool Regularizacion = false,
    Guid? TransportistaId = null, string? Matricula = null, string? Observaciones = null);

public sealed record LineaMovimientoEnvasesDto(Guid EnvaseProductoId, string Envase, int Cantidad);

public sealed record MovimientoEnvasesDto(Guid Id, string Numero, DateOnly Fecha, Guid CuentaId, string Cuenta, Guid CuentaSolicitadaId, string Origen,
    Guid? DocumentoId, string? Matricula, string? Observaciones, bool Anulado, Guid? AnulaMovimientoId, IReadOnlyList<LineaMovimientoEnvasesDto> Lineas,
    string? Aviso = null);

public sealed record LineaExtractoEnvasesDto(MovimientoEnvasesDto Movimiento, int Neto, int Acumulado);

public sealed record ExtractoEnvasesDto(CuentaEnvasesDto Cuenta, DateOnly? Desde, DateOnly? Hasta, int SaldoInicial, IReadOnlyList<SaldoEnvaseCuentaDto> SaldoInicialPorEnvase,
    IReadOnlyList<LineaExtractoEnvasesDto> Movimientos, int SaldoFinal, IReadOnlyList<SaldoEnvaseCuentaDto> SaldoFinalPorEnvase);

/// <summary>
/// Envases retornables por tercero, como en Hispatec: cuentas de clientes, proveedores, transportistas y pools, con un
/// libro de movimientos numerado y de solo inserción. El saldo (+ lo que tiene el tercero) sale del libro. El movimiento
/// va a la cuenta agrupadora si la hay, y el de un cliente que lleva los envases al transportista, a la del transportista.
/// Una cuenta bloqueada no admite movimientos; con aviso, o si supera su límite, se avisa.
/// </summary>
public sealed class EnvasesTerceros
{
    private const int ProfundidadAgrupadoras = 5;

    private readonly IRepositorioEnvases _repo;
    private readonly IUnidadDeTrabajoAgro _unidad;
    private readonly IConsultaClientes _clientes;
    private readonly IConsultaProveedores _proveedores;
    private readonly IConsultaProductos _productos;
    private readonly IReloj _reloj;

    /// <summary>Último número dado en esta unidad de trabajo (una expedición puede generar varios movimientos antes de guardar).</summary>
    private readonly Dictionary<(Guid, int), int> _ultimos = [];

    public EnvasesTerceros(IRepositorioEnvases repo, IUnidadDeTrabajoAgro unidad, IConsultaClientes clientes, IConsultaProveedores proveedores, IConsultaProductos productos,
        IReloj reloj)
    {
        _repo = repo;
        _unidad = unidad;
        _clientes = clientes;
        _proveedores = proveedores;
        _productos = productos;
        _reloj = reloj;
    }

    private DateOnly Hoy => DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);

    // ----------------------------------------------------------------------------- Cuentas
    public async Task<IReadOnlyList<CuentaEnvasesDto>> CuentasAsync(Guid empresaId, DateOnly? hasta = null, CancellationToken ct = default)
    {
        var cuentas = await _repo.CuentasAsync(empresaId, ct).ConfigureAwait(false);
        var saldos = await _repo.SaldosAsync(empresaId, hasta, ct).ConfigureAwait(false);
        var nombres = await NombresEnvasesAsync(saldos.Select(s => s.EnvaseProductoId), ct).ConfigureAwait(false);
        return cuentas.Select(c => Dto(c, saldos.Where(s => s.CuentaId == c.Id).Select(s => (s.EnvaseProductoId, s.Saldo)), nombres)).ToList();
    }

    public async Task<Resultado<CuentaEnvasesDto>> CrearCuentaAsync(Guid empresaId, CrearCuentaEnvasesComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        if (await _repo.CuentaDeAsync(empresaId, comando.Tipo, comando.TerceroId, ct).ConfigureAwait(false) is not null)
        {
            return Resultado.Fallo<CuentaEnvasesDto>(Error.Conflicto("envases.cuenta_existe", "Ese tercero ya tiene cuenta de envases."));
        }

        var nombre = await NombreTerceroAsync(comando.Tipo, comando.TerceroId, ct).ConfigureAwait(false) ?? comando.Nombre;
        if (comando.Tipo != TipoCuentaEnvases.Transportista && nombre is null)
        {
            return Resultado.Fallo<CuentaEnvasesDto>(Error.NoEncontrado("envases.tercero", "El cliente o proveedor no existe."));
        }

        var cuenta = CuentaEnvases.Crear(empresaId, comando.Tipo, comando.TerceroId, nombre, _reloj);
        if (cuenta.EsFallo)
        {
            return Resultado.Fallo<CuentaEnvasesDto>(cuenta.Error);
        }

        _repo.Agregar(cuenta.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(cuenta.Valor, [], new Dictionary<Guid, string>()));
    }

    public async Task<Resultado<CuentaEnvasesDto>> ConfigurarCuentaAsync(Guid cuentaId, ConfigurarCuentaEnvasesComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var cuenta = await _repo.CuentaAsync(cuentaId, ct).ConfigureAwait(false);
        if (cuenta is null)
        {
            return Resultado.Fallo<CuentaEnvasesDto>(NoEncontrada());
        }

        // La agrupadora existe y no forma un ciclo.
        for (Guid? a = comando.AgrupadoraId; a is { } actual;)
        {
            if (actual == cuenta.Id)
            {
                return Resultado.Fallo<CuentaEnvasesDto>(Error.Validacion("envases.agrupadora", "Esa agrupación forma un ciclo."));
            }

            var agrupadora = await _repo.CuentaAsync(actual, ct).ConfigureAwait(false);
            if (agrupadora is null)
            {
                return Resultado.Fallo<CuentaEnvasesDto>(Error.NoEncontrado("envases.agrupadora", "La cuenta agrupadora no existe."));
            }

            a = agrupadora.AgrupadoraId;
        }

        var r = cuenta.Configurar(comando.AgrupadoraId, comando.ImputarATransportista, comando.Bloqueo, comando.MotivoBloqueo, comando.Limite, comando.Activa);
        if (r.EsFallo)
        {
            return Resultado.Fallo<CuentaEnvasesDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok((await CuentasAsync(cuenta.EmpresaId, null, ct).ConfigureAwait(false)).Single(c => c.Id == cuenta.Id));
    }

    // ----------------------------------------------------------------------------- Movimientos
    public async Task<Resultado<MovimientoEnvasesDto>> RegistrarAsync(Guid empresaId, MovimientoEnvasesComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var cuenta = await _repo.CuentaAsync(comando.CuentaId, ct).ConfigureAwait(false);
        if (cuenta is null)
        {
            return Resultado.Fallo<MovimientoEnvasesDto>(NoEncontrada());
        }

        foreach (var l in comando.Lineas ?? [])
        {
            if (await _productos.ObtenerAsync(l.EnvaseProductoId, ct).ConfigureAwait(false) is null)
            {
                return Resultado.Fallo<MovimientoEnvasesDto>(Error.NoEncontrado("envases.envase", "Un envase no existe en el catálogo."));
            }
        }

        var r = await PrepararAsync(empresaId, cuenta, comando.Regularizacion ? OrigenMovimientoEnvases.Regularizacion : OrigenMovimientoEnvases.Manual,
            (comando.Lineas ?? []).Select(l => (l.EnvaseProductoId, l.Cantidad)).ToList(), comando.Fecha ?? Hoy, null, comando.TransportistaId, comando.Matricula,
            comando.Observaciones, ct).ConfigureAwait(false);
        if (r.EsFallo)
        {
            return Resultado.Fallo<MovimientoEnvasesDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(r.Valor.Movimiento, false, ct).ConfigureAwait(false) with { Aviso = r.Valor.Aviso });
    }

    /// <summary>Anula un movimiento con su contrario (misma cuenta y fecha de hoy). Un movimiento se anula una sola vez.</summary>
    public async Task<Resultado<MovimientoEnvasesDto>> AnularAsync(Guid movimientoId, string? motivo, CancellationToken ct = default)
    {
        var m = await _repo.MovimientoAsync(movimientoId, ct).ConfigureAwait(false);
        if (m is null)
        {
            return Resultado.Fallo<MovimientoEnvasesDto>(Error.NoEncontrado("envases.movimiento", "El movimiento de envases no existe."));
        }

        var contra = await AnularInternoAsync(m, motivo, ct).ConfigureAwait(false);
        if (contra.EsFallo)
        {
            return Resultado.Fallo<MovimientoEnvasesDto>(contra.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(contra.Valor, false, ct).ConfigureAwait(false));
    }

    public async Task<IReadOnlyList<MovimientoEnvasesDto>> MovimientosAsync(Guid empresaId, Guid? cuentaId, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
    {
        var lista = new List<MovimientoEnvasesDto>();
        foreach (var m in await _repo.MovimientosAsync(empresaId, cuentaId, desde, hasta, ct).ConfigureAwait(false))
        {
            lista.Add(await DtoAsync(m, await _repo.AnuladoAsync(m.Id, ct).ConfigureAwait(false), ct).ConfigureAwait(false));
        }

        return lista;
    }

    /// <summary>Extracto de una cuenta: saldo inicial, movimientos con el acumulado y saldo final (en total y por envase).</summary>
    public async Task<Resultado<ExtractoEnvasesDto>> ExtractoAsync(Guid empresaId, Guid cuentaId, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
    {
        var cuenta = (await CuentasAsync(empresaId, hasta, ct).ConfigureAwait(false)).SingleOrDefault(c => c.Id == cuentaId);
        if (cuenta is null)
        {
            return Resultado.Fallo<ExtractoEnvasesDto>(NoEncontrada());
        }

        var inicial = desde is { } d
            ? (await _repo.SaldosAsync(empresaId, d.AddDays(-1), ct).ConfigureAwait(false)).Where(s => s.CuentaId == cuentaId).ToList()
            : [];
        var nombres = await NombresEnvasesAsync(inicial.Select(s => s.EnvaseProductoId), ct).ConfigureAwait(false);
        var saldoInicial = inicial.Sum(s => s.Saldo);
        var acumulado = saldoInicial;
        var lineas = new List<LineaExtractoEnvasesDto>();
        foreach (var m in (await MovimientosAsync(empresaId, cuentaId, desde, hasta, ct).ConfigureAwait(false)).Where(m => m.CuentaId == cuentaId))
        {
            var neto = m.Lineas.Sum(l => l.Cantidad);
            acumulado += neto;
            lineas.Add(new LineaExtractoEnvasesDto(m, neto, acumulado));
        }

        return Resultado.Ok(new ExtractoEnvasesDto(cuenta, desde, hasta, saldoInicial,
            inicial.Select(s => new SaldoEnvaseCuentaDto(s.EnvaseProductoId, nombres.GetValueOrDefault(s.EnvaseProductoId, "?"), s.Saldo)).ToList(),
            lineas, cuenta.SaldoTotal, cuenta.Saldos));
    }

    // ----------------------------------------------------------------------------- Expedición
    /// <summary>
    /// Entrega automática al expedir palés a un cliente: las cajas y el palé retornables de su plantilla. Se prepara en la
    /// misma unidad de trabajo que la expedición (se guarda con ella). Sin cuenta, se abre la del cliente. Devuelve un aviso,
    /// si lo hay, o null.
    /// </summary>
    public async Task<Resultado<string?>> EntregarEnExpedicionAsync(Guid empresaId, Guid clienteId, IReadOnlyList<(Guid PaleId, Guid? Envase, int Cajas, Guid? PaleEnvase)> pales,
        DateOnly fecha, Guid? transportistaId, string? matricula, string? referencia, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(pales);
        var lineasPorPale = pales.Select(p => (p.PaleId, Lineas: new List<(Guid, int)>(
            (p.Envase is { } e && p.Cajas > 0 ? [(e, p.Cajas)] : Array.Empty<(Guid, int)>()).Concat(p.PaleEnvase is { } pe ? [(pe, 1)] : [])))).Where(x => x.Lineas.Count > 0).ToList();
        if (lineasPorPale.Count == 0)
        {
            return Resultado.Ok<string?>(null);
        }

        var cuenta = await _repo.CuentaDeAsync(empresaId, TipoCuentaEnvases.Cliente, clienteId, ct).ConfigureAwait(false);
        if (cuenta is null)
        {
            var nombre = await NombreTerceroAsync(TipoCuentaEnvases.Cliente, clienteId, ct).ConfigureAwait(false);
            var nueva = CuentaEnvases.Crear(empresaId, TipoCuentaEnvases.Cliente, clienteId, nombre ?? "Cliente", _reloj);
            if (nueva.EsFallo)
            {
                return Resultado.Fallo<string?>(nueva.Error);
            }

            cuenta = nueva.Valor;
            _repo.Agregar(cuenta);
        }

        string? aviso = null;
        foreach (var (paleId, lineas) in lineasPorPale)
        {
            var r = await PrepararAsync(empresaId, cuenta, OrigenMovimientoEnvases.Expedicion, lineas, fecha, paleId, transportistaId, matricula,
                string.IsNullOrWhiteSpace(referencia) ? "Expedición" : $"Expedición {referencia}", ct).ConfigureAwait(false);
            if (r.EsFallo)
            {
                return Resultado.Fallo<string?>(r.Error);
            }

            aviso ??= r.Valor.Aviso;
        }

        return Resultado.Ok(aviso);
    }

    /// <summary>Anula los movimientos de envases de un documento (la expedición de un palé), en la misma unidad de trabajo.</summary>
    public async Task<Resultado> AnularDeDocumentoAsync(Guid documentoId, string motivo, CancellationToken ct = default)
    {
        foreach (var m in (await _repo.DeDocumentoAsync(documentoId, ct).ConfigureAwait(false)).Where(m => m.Origen != OrigenMovimientoEnvases.Anulacion))
        {
            if (await _repo.AnuladoAsync(m.Id, ct).ConfigureAwait(false))
            {
                continue;
            }

            var r = await AnularInternoAsync(m, motivo, ct).ConfigureAwait(false);
            if (r.EsFallo)
            {
                return Resultado.Fallo(r.Error);
            }
        }

        return Resultado.Ok();
    }

    // ----------------------------------------------------------------------------- Apoyo
    private async Task<Resultado<(MovimientoEnvases Movimiento, string? Aviso)>> PrepararAsync(Guid empresaId, CuentaEnvases solicitada, OrigenMovimientoEnvases origen,
        IReadOnlyList<(Guid EnvaseProductoId, int Cantidad)> lineas, DateOnly fecha, Guid? documentoId, Guid? transportistaId, string? matricula, string? observaciones,
        CancellationToken ct, Guid? anula = null)
    {
        // Cuenta efectiva: la del transportista (si el cliente lleva ahí los envases) y, después, la agrupadora.
        var cuenta = solicitada;
        if (origen != OrigenMovimientoEnvases.Anulacion && solicitada.ImputarATransportista && transportistaId is { } tr)
        {
            cuenta = await _repo.CuentaDeAsync(empresaId, TipoCuentaEnvases.Transportista, tr, ct).ConfigureAwait(false) ?? solicitada;
        }

        for (var i = 0; origen != OrigenMovimientoEnvases.Anulacion && cuenta.AgrupadoraId is { } a && i < ProfundidadAgrupadoras; i++)
        {
            cuenta = await _repo.CuentaAsync(a, ct).ConfigureAwait(false) ?? cuenta;
        }

        string? aviso = null;
        if (origen != OrigenMovimientoEnvases.Anulacion)
        {
            if (!cuenta.Activa)
            {
                return Resultado.Fallo<(MovimientoEnvases, string?)>(Error.Conflicto("envases.cuenta_inactiva", $"La cuenta de envases de {cuenta.Nombre} está dada de baja."));
            }

            if (cuenta.Bloqueo == BloqueoEnvases.Bloqueo)
            {
                return Resultado.Fallo<(MovimientoEnvases, string?)>(Error.Conflicto("envases.cuenta_bloqueada",
                    $"La cuenta de envases de {cuenta.Nombre} está bloqueada{(cuenta.MotivoBloqueo is { } mb ? ": " + mb : ".")}"));
            }

            if (cuenta.Bloqueo == BloqueoEnvases.Aviso)
            {
                aviso = $"Aviso en la cuenta de envases de {cuenta.Nombre}{(cuenta.MotivoBloqueo is { } mb ? ": " + mb : ".")}";
            }
        }

        await _unidad.BloquearAsync($"agro:envases:{empresaId}:{fecha.Year}", ct).ConfigureAwait(false);
        var numero = Math.Max(await _repo.SiguienteNumeroAsync(empresaId, fecha.Year, ct).ConfigureAwait(false),
            _ultimos.GetValueOrDefault((empresaId, fecha.Year)) + 1);
        _ultimos[(empresaId, fecha.Year)] = numero;
        var m = MovimientoEnvases.Crear(empresaId, numero, fecha, cuenta.Id, solicitada.Id, origen, lineas, _reloj, documentoId, transportistaId, matricula,
            observaciones, anula);
        if (m.EsFallo)
        {
            return Resultado.Fallo<(MovimientoEnvases, string?)>(m.Error);
        }

        _repo.Agregar(m.Valor);
        if (cuenta.Limite is { } limite && origen != OrigenMovimientoEnvases.Anulacion)
        {
            var saldo = (await _repo.SaldosAsync(empresaId, null, ct).ConfigureAwait(false)).Where(s => s.CuentaId == cuenta.Id).Sum(s => s.Saldo) + m.Valor.Lineas.Sum(l => l.Cantidad);
            if (saldo > limite)
            {
                aviso ??= $"{cuenta.Nombre} supera su límite de envases: {saldo} de {limite}.";
            }
        }

        return Resultado.Ok<(MovimientoEnvases, string?)>((m.Valor, aviso));
    }

    private async Task<Resultado<MovimientoEnvases>> AnularInternoAsync(MovimientoEnvases m, string? motivo, CancellationToken ct)
    {
        if (m.Origen == OrigenMovimientoEnvases.Anulacion)
        {
            return Resultado.Fallo<MovimientoEnvases>(Error.Conflicto("envases.anulacion", "Un movimiento de anulación no se anula."));
        }

        if (await _repo.AnuladoAsync(m.Id, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<MovimientoEnvases>(Error.Conflicto("envases.ya_anulado", "El movimiento ya está anulado."));
        }

        var cuenta = await _repo.CuentaAsync(m.CuentaId, ct).ConfigureAwait(false);
        if (cuenta is null)
        {
            return Resultado.Fallo<MovimientoEnvases>(NoEncontrada());
        }

        var r = await PrepararAsync(m.EmpresaId, cuenta, OrigenMovimientoEnvases.Anulacion, m.Lineas.Select(l => (l.EnvaseProductoId, -l.Cantidad)).ToList(), Hoy,
            m.DocumentoId, m.TransportistaId, m.Matricula, $"Anulación de {m.NumeroCompleto}{(string.IsNullOrWhiteSpace(motivo) ? string.Empty : ": " + motivo.Trim())}", ct, m.Id)
            .ConfigureAwait(false);
        return r.EsFallo ? Resultado.Fallo<MovimientoEnvases>(r.Error) : Resultado.Ok(r.Valor.Movimiento);
    }

    private async Task<string?> NombreTerceroAsync(TipoCuentaEnvases tipo, Guid terceroId, CancellationToken ct) => tipo switch
    {
        TipoCuentaEnvases.Cliente => (await _clientes.ObtenerAsync(terceroId, ct).ConfigureAwait(false))?.Nombre,
        TipoCuentaEnvases.Proveedor or TipoCuentaEnvases.Pool => (await _proveedores.ObtenerAsync(terceroId, ct).ConfigureAwait(false))?.Nombre,
        _ => null,
    };

    private async Task<Dictionary<Guid, string>> NombresEnvasesAsync(IEnumerable<Guid> ids, CancellationToken ct)
    {
        var nombres = new Dictionary<Guid, string>();
        foreach (var id in ids.Distinct())
        {
            nombres[id] = (await _productos.ObtenerAsync(id, ct).ConfigureAwait(false))?.Nombre ?? "(envase)";
        }

        return nombres;
    }

    private async Task<MovimientoEnvasesDto> DtoAsync(MovimientoEnvases m, bool anulado, CancellationToken ct)
    {
        var nombres = await NombresEnvasesAsync(m.Lineas.Select(l => l.EnvaseProductoId), ct).ConfigureAwait(false);
        var cuenta = await _repo.CuentaAsync(m.CuentaId, ct).ConfigureAwait(false);
        return new MovimientoEnvasesDto(m.Id, m.NumeroCompleto, m.Fecha, m.CuentaId, cuenta?.Nombre ?? "?", m.CuentaSolicitadaId, m.Origen.ToString(), m.DocumentoId,
            m.Matricula, m.Observaciones, anulado, m.AnulaMovimientoId,
            m.Lineas.Select(l => new LineaMovimientoEnvasesDto(l.EnvaseProductoId, nombres.GetValueOrDefault(l.EnvaseProductoId, "?"), l.Cantidad)).ToList());
    }

    private static CuentaEnvasesDto Dto(CuentaEnvases c, IEnumerable<(Guid Envase, int Saldo)> saldos, IReadOnlyDictionary<Guid, string> nombres)
    {
        var lista = saldos.Where(s => s.Saldo != 0).Select(s => new SaldoEnvaseCuentaDto(s.Envase, nombres.GetValueOrDefault(s.Envase, "?"), s.Saldo))
            .OrderBy(s => s.Envase, StringComparer.CurrentCulture).ToList();
        var total = lista.Sum(s => s.Saldo);
        return new CuentaEnvasesDto(c.Id, c.Tipo.ToString(), c.TerceroId, c.Nombre, c.AgrupadoraId, c.ImputarATransportista, c.Bloqueo.ToString(), c.MotivoBloqueo,
            c.Limite, c.Activa, total, lista, c.Limite is { } l && total > l);
    }

    private static Error NoEncontrada() => Error.NoEncontrado("envases.cuenta", "La cuenta de envases no existe.");
}
