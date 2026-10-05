using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Terceros.Aplicacion;
using AlxorCore.Vivero.Dominio;

namespace AlxorCore.Vivero.Aplicacion;

public interface IUnidadDeTrabajoVivero : IUnidadDeTrabajo
{
    Task BloquearAsync(string clave, CancellationToken ct = default);
}

public interface IRepositorioVivero
{
    void Agregar(object entidad);

    Task<ConfiguracionVivero?> ConfiguracionAsync(Guid empresaId, CancellationToken ct = default);

    Task<IReadOnlyList<LotePlanta>> LotesAsync(Guid empresaId, bool incluirTerminados, CancellationToken ct = default);

    Task<LotePlanta?> LoteAsync(Guid id, CancellationToken ct = default);

    Task<int> SiguienteNumeroLoteAsync(Guid empresaId, int ejercicio, CancellationToken ct = default);

    Task<IReadOnlyList<EncargoPlanta>> EncargosAsync(Guid empresaId, CancellationToken ct = default);

    Task<IReadOnlyList<EncargoPlanta>> EncargosDeLoteAsync(Guid loteId, CancellationToken ct = default);

    Task<EncargoPlanta?> EncargoAsync(Guid id, CancellationToken ct = default);

    Task<int> SiguienteNumeroEncargoAsync(Guid empresaId, int ejercicio, CancellationToken ct = default);
}

/// <summary>Existencias del artículo de la planta (Catálogo e Inventario). Lo implementa la API.</summary>
public interface IExistenciasVivero
{
    Task<Resultado> EntrarAsync(Guid empresaId, Guid productoId, decimal cantidad, string motivo, CancellationToken ct = default);

    Task<Resultado> SacarAsync(Guid empresaId, Guid productoId, decimal cantidad, string motivo, CancellationToken ct = default);
}

/// <summary>Albarán de la entrega de un encargo (Facturación). Lo implementa la API.</summary>
public interface IVentasVivero
{
    Task<Resultado<(Guid Id, string Numero)>> AlbaranAsync(Guid empresaId, Guid clienteId, DateOnly fecha, string referencia, Guid productoId, decimal cantidad, decimal? precio,
        CancellationToken ct = default);

    Task<Resultado> AnularAlbaranAsync(Guid albaranId, string motivo, CancellationToken ct = default);
}

// ----------------------------------------------------------------------------- Contratos
public sealed record DatosConfiguracionVivero(string? CodigoRegistro, string? PaisOrigen = "ES");

public sealed record ConfiguracionViveroDto(string CodigoRegistro, string PaisOrigen);

public sealed record DatosLotePlanta(string? Especie, Guid ProductoId, int Plantas, DateOnly? FechaSiembra = null, string? Variedad = null, string? Portainjerto = null,
    FasePlanta Fase = FasePlanta.Semillero, string? Ubicacion = null, string? OrigenMaterial = null, DateOnly? FechaPrevistaLista = null, string? Observaciones = null);

public sealed record DatosCambioLote(string? Variedad, string? Portainjerto, string? OrigenMaterial, DateOnly? FechaPrevistaLista, string? Observaciones);

public sealed record DatosAvanceLote(FasePlanta? Fase = null, string? Ubicacion = null, DateOnly? Fecha = null);

public sealed record DatosSalidaLote(int Plantas, string? Motivo = null, DateOnly? Fecha = null);

public sealed record DatosAnulacionMovimiento(string? Motivo, DateOnly? Fecha = null);

public sealed record MovimientoLoteDto(Guid Id, int Orden, DateOnly Fecha, string Tipo, int Plantas, string Fase, string? Ubicacion, string? Concepto, Guid? DocumentoId,
    Guid? AnulaId, bool Anulado);

public sealed record LotePlantaDto(Guid Id, string Codigo, string Especie, string? Variedad, string? Portainjerto, Guid ProductoId, string? Producto, string? OrigenMaterial,
    DateOnly FechaSiembra, DateOnly? FechaPrevistaLista, string Fase, string? Ubicacion, int PlantasIniciales, int PlantasVivas, int Reservadas, int Disponibles,
    int Bajas, decimal PorcentajeBajas, bool Terminado, bool Anulado, string? Observaciones, IReadOnlyList<MovimientoLoteDto> Movimientos);

public sealed record DatosEncargo(Guid ClienteId, Guid ProductoId, int Plantas, DateOnly FechaEntrega, decimal? PrecioPlanta = null, DateOnly? Fecha = null,
    string? Observaciones = null, Guid? LoteId = null);

public sealed record EncargoDto(Guid Id, string Numero, DateOnly Fecha, Guid ClienteId, string Cliente, Guid ProductoId, string? Producto, int Plantas, DateOnly FechaEntrega,
    decimal? PrecioPlanta, string Estado, Guid? LoteId, string? Lote, Guid? AlbaranId, string? Albaran, string? Observaciones);

/// <summary>Datos del pasaporte fitosanitario UE de un lote (Reglamento (UE) 2016/2031): A especie, B registro, C trazabilidad, D origen.</summary>
public sealed record PasaporteDto(string Titulo, string Especie, string? Variedad, string CodigoRegistro, string CodigoTrazabilidad, string PaisOrigen, string Lote,
    string? Producto, int? Plantas, string? Cliente, string? Albaran);

/// <summary>Movimiento del libro del vivero (todos los lotes).</summary>
public sealed record LibroViveroDto(DateOnly Fecha, string Lote, string Especie, string? Variedad, string Tipo, int Plantas, string Fase, string? Ubicacion, string? Concepto);

/// <summary>
/// Vivero: lotes de planta con sus fases, ubicaciones, bajas y paso a existencias; encargos de clientes con reserva de
/// plantas y entrega con albarán; pasaporte fitosanitario y libro de movimientos.
/// </summary>
public sealed class GestionVivero
{
    private readonly IRepositorioVivero _repo;
    private readonly IUnidadDeTrabajoVivero _unidad;
    private readonly IConsultaProductos _productos;
    private readonly IConsultaClientes _clientes;
    private readonly IReloj _reloj;
    private readonly IExistenciasVivero? _existencias;
    private readonly IVentasVivero? _ventas;

    public GestionVivero(IRepositorioVivero repo, IUnidadDeTrabajoVivero unidad, IConsultaProductos productos, IConsultaClientes clientes, IReloj reloj,
        IExistenciasVivero? existencias = null, IVentasVivero? ventas = null)
    {
        _repo = repo;
        _unidad = unidad;
        _productos = productos;
        _clientes = clientes;
        _reloj = reloj;
        _existencias = existencias;
        _ventas = ventas;
    }

    private DateOnly Hoy => DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);

    // ------------------------------------------------------------------ Configuración
    public async Task<ConfiguracionViveroDto> ConfiguracionAsync(Guid empresaId, CancellationToken ct = default) =>
        await _repo.ConfiguracionAsync(empresaId, ct).ConfigureAwait(false) is { } c ? new(c.CodigoRegistro, c.PaisOrigen) : new(string.Empty, "ES");

    public async Task<Resultado<ConfiguracionViveroDto>> FijarConfiguracionAsync(Guid empresaId, DatosConfiguracionVivero datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var c = await _repo.ConfiguracionAsync(empresaId, ct).ConfigureAwait(false);
        var nueva = c is null;
        c ??= ConfiguracionVivero.Nueva(empresaId);
        var r = c.Fijar(datos.CodigoRegistro, datos.PaisOrigen);
        if (r.EsFallo)
        {
            return Resultado.Fallo<ConfiguracionViveroDto>(r.Error);
        }

        if (nueva)
        {
            _repo.Agregar(c);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new ConfiguracionViveroDto(c.CodigoRegistro, c.PaisOrigen));
    }

    // ------------------------------------------------------------------ Lotes
    public async Task<IReadOnlyList<LotePlantaDto>> LotesAsync(Guid empresaId, bool incluirTerminados, CancellationToken ct = default)
    {
        var lotes = await _repo.LotesAsync(empresaId, incluirTerminados, ct).ConfigureAwait(false);
        var encargos = await _repo.EncargosAsync(empresaId, ct).ConfigureAwait(false);
        var lista = new List<LotePlantaDto>();
        foreach (var l in lotes.OrderByDescending(l => l.FechaSiembra).ThenByDescending(l => l.Numero))
        {
            lista.Add(await DtoAsync(l, encargos, ct).ConfigureAwait(false));
        }

        return lista;
    }

    public async Task<LotePlantaDto?> LoteAsync(Guid id, CancellationToken ct = default)
    {
        var l = await _repo.LoteAsync(id, ct).ConfigureAwait(false);
        return l is null ? null : await DtoAsync(l, await _repo.EncargosDeLoteAsync(id, ct).ConfigureAwait(false), ct).ConfigureAwait(false);
    }

    public async Task<Resultado<LotePlantaDto>> CrearLoteAsync(Guid empresaId, DatosLotePlanta datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        if (await _productos.ObtenerAsync(datos.ProductoId, ct).ConfigureAwait(false) is null)
        {
            return Resultado.Fallo<LotePlantaDto>(Error.NoEncontrado("producto.no_encontrado", "El artículo de la planta no existe."));
        }

        var fecha = datos.FechaSiembra ?? Hoy;
        await _unidad.BloquearAsync($"vivero:lote:{empresaId}:{fecha.Year}", ct).ConfigureAwait(false);
        var numero = await _repo.SiguienteNumeroLoteAsync(empresaId, fecha.Year, ct).ConfigureAwait(false);
        var l = LotePlanta.Crear(empresaId, numero, fecha, datos.Especie, datos.Variedad, datos.Portainjerto, datos.ProductoId, datos.Plantas, datos.Fase, datos.Ubicacion,
            datos.OrigenMaterial, datos.FechaPrevistaLista, datos.Observaciones, _reloj);
        if (l.EsFallo)
        {
            return Resultado.Fallo<LotePlantaDto>(l.Error);
        }

        _repo.Agregar(l.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(l.Valor, [], ct).ConfigureAwait(false));
    }

    public Task<Resultado<LotePlantaDto>> CambiarLoteAsync(Guid id, DatosCambioLote datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        return ConLoteAsync(id, l => Task.FromResult(l.Cambiar(datos.Variedad, datos.Portainjerto, datos.OrigenMaterial, datos.FechaPrevistaLista, datos.Observaciones)), ct);
    }

    public Task<Resultado<LotePlantaDto>> AvanzarAsync(Guid id, DatosAvanceLote datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        return ConLoteAsync(id, l => Task.FromResult(l.Avanzar(datos.Fecha ?? Hoy, datos.Fase, datos.Ubicacion, _reloj)), ct);
    }

    public Task<Resultado<LotePlantaDto>> BajaAsync(Guid id, DatosSalidaLote datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        return ConLoteAsync(id, l =>
        {
            var r = l.Sacar(datos.Fecha ?? Hoy, TipoMovimientoLote.Baja, datos.Plantas, 0, datos.Motivo, null, _reloj);
            return Task.FromResult(r.EsFallo ? Resultado.Fallo(r.Error) : Resultado.Ok());
        }, ct);
    }

    /// <summary>Las plantas listas (sin reservar) pasan a las existencias del artículo, para venderlas con los documentos de venta.</summary>
    public Task<Resultado<LotePlantaDto>> PasarAExistenciasAsync(Guid empresaId, Guid id, DatosSalidaLote datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        return ConLoteAsync(id, async l =>
        {
            var reservadas = Reservadas(await _repo.EncargosDeLoteAsync(id, ct).ConfigureAwait(false), null);
            var r = l.Sacar(datos.Fecha ?? Hoy, TipoMovimientoLote.PasoExistencias, datos.Plantas, reservadas, datos.Motivo ?? "Paso a existencias", l.Id, _reloj);
            if (r.EsFallo)
            {
                return Resultado.Fallo(r.Error);
            }

            return _existencias is null ? Resultado.Ok()
                : await _existencias.EntrarAsync(empresaId, l.ProductoId, datos.Plantas, $"Vivero: lote {l.Codigo}", ct).ConfigureAwait(false);
        }, ct);
    }

    /// <summary>Anula una baja o un paso a existencias (las entregas se deshacen anulando el encargo).</summary>
    public Task<Resultado<LotePlantaDto>> AnularMovimientoAsync(Guid empresaId, Guid id, Guid movimientoId, DatosAnulacionMovimiento datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        return ConLoteAsync(id, async l =>
        {
            var original = l.Movimientos.SingleOrDefault(m => m.Id == movimientoId);
            if (original?.Tipo == TipoMovimientoLote.Entrega)
            {
                return Resultado.Fallo(Error.Conflicto("lote_planta.entrega", "Una entrega se deshace anulando su encargo (anula también el albarán)."));
            }

            var r = l.AnularMovimiento(movimientoId, datos.Fecha ?? Hoy, datos.Motivo, _reloj);
            if (r.EsFallo)
            {
                return Resultado.Fallo(r.Error);
            }

            if (original!.Tipo == TipoMovimientoLote.PasoExistencias && _existencias is not null)
            {
                var salida = await _existencias.SacarAsync(empresaId, l.ProductoId, -original.Plantas, $"Vivero: anulación del paso a existencias del lote {l.Codigo}", ct)
                    .ConfigureAwait(false);
                if (salida.EsFallo)
                {
                    return Resultado.Fallo(Error.Conflicto(salida.Error.Codigo, $"Las plantas no se pueden sacar de las existencias: {salida.Error.Mensaje}"));
                }
            }

            return Resultado.Ok();
        }, ct);
    }

    public Task<Resultado<LotePlantaDto>> AnularLoteAsync(Guid id, CancellationToken ct = default) =>
        ConLoteAsync(id, async l =>
        {
            if ((await _repo.EncargosDeLoteAsync(id, ct).ConfigureAwait(false)).Any(e => e.Estado == EstadoEncargo.Reservado))
            {
                return Resultado.Fallo(Error.Conflicto("lote_planta.reservado", "El lote tiene encargos reservados: libéralos antes."));
            }

            return l.Anular();
        }, ct);

    /// <summary>Pasaporte fitosanitario del lote (o de la entrega de un encargo, con su cliente, plantas y albarán).</summary>
    public async Task<Resultado<PasaporteDto>> PasaporteAsync(Guid empresaId, Guid loteId, Guid? encargoId, CancellationToken ct = default)
    {
        var config = await _repo.ConfiguracionAsync(empresaId, ct).ConfigureAwait(false);
        if (config is null || string.IsNullOrWhiteSpace(config.CodigoRegistro))
        {
            return Resultado.Fallo<PasaporteDto>(Error.Validacion("vivero.registro", "Indica antes el código de registro del vivero (ROPVEG) en los ajustes del vivero."));
        }

        var lote = await _repo.LoteAsync(loteId, ct).ConfigureAwait(false);
        if (lote is null || lote.EmpresaId != empresaId)
        {
            return Resultado.Fallo<PasaporteDto>(Error.NoEncontrado("lote_planta.no_encontrado", "El lote no existe."));
        }

        EncargoPlanta? encargo = null;
        if (encargoId is { } eid)
        {
            encargo = await _repo.EncargoAsync(eid, ct).ConfigureAwait(false);
            if (encargo is null || encargo.LoteId != loteId)
            {
                return Resultado.Fallo<PasaporteDto>(Error.NoEncontrado("encargo.no_encontrado", "El encargo no es de este lote."));
            }
        }

        var producto = await _productos.ObtenerAsync(lote.ProductoId, ct).ConfigureAwait(false);
        return Resultado.Ok(new PasaporteDto("Pasaporte fitosanitario / Plant Passport", lote.Especie, lote.Variedad, config.CodigoRegistro, lote.Codigo, config.PaisOrigen,
            lote.Codigo, producto?.Nombre, encargo?.Plantas, encargo?.ClienteNombre, encargo?.AlbaranNumero));
    }

    /// <summary>Libro del vivero: los movimientos de todos los lotes en unas fechas.</summary>
    public async Task<IReadOnlyList<LibroViveroDto>> LibroAsync(Guid empresaId, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default) =>
        (await _repo.LotesAsync(empresaId, true, ct).ConfigureAwait(false))
            .SelectMany(l => l.Movimientos.Where(m => (desde is null || m.Fecha >= desde) && (hasta is null || m.Fecha <= hasta))
                .Select(m => (l, m)))
            .OrderBy(x => x.m.Fecha).ThenBy(x => x.m.CreadoEn)
            .Select(x => new LibroViveroDto(x.m.Fecha, x.l.Codigo, x.l.Especie, x.l.Variedad, x.m.Tipo.ToString(), x.m.Plantas, x.m.Fase.ToString(), x.m.Ubicacion,
                x.m.Concepto))
            .ToList();

    // ------------------------------------------------------------------ Encargos
    public async Task<IReadOnlyList<EncargoDto>> EncargosAsync(Guid empresaId, CancellationToken ct = default)
    {
        var lotes = (await _repo.LotesAsync(empresaId, true, ct).ConfigureAwait(false)).ToDictionary(l => l.Id, l => l.Codigo);
        var lista = new List<EncargoDto>();
        foreach (var e in (await _repo.EncargosAsync(empresaId, ct).ConfigureAwait(false)).OrderBy(e => e.FechaEntrega).ThenBy(e => e.Numero))
        {
            lista.Add(await EncargoDtoAsync(e, e.LoteId is { } l ? lotes.GetValueOrDefault(l) : null, ct).ConfigureAwait(false));
        }

        return lista;
    }

    public async Task<Resultado<EncargoDto>> CrearEncargoAsync(Guid empresaId, DatosEncargo datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var cliente = await _clientes.ObtenerAsync(datos.ClienteId, ct).ConfigureAwait(false);
        if (cliente is null)
        {
            return Resultado.Fallo<EncargoDto>(Error.NoEncontrado("cliente.no_encontrado", "El cliente no existe."));
        }

        if (await _productos.ObtenerAsync(datos.ProductoId, ct).ConfigureAwait(false) is null)
        {
            return Resultado.Fallo<EncargoDto>(Error.NoEncontrado("producto.no_encontrado", "El artículo no existe."));
        }

        var fecha = datos.Fecha ?? Hoy;
        await _unidad.BloquearAsync($"vivero:encargo:{empresaId}:{fecha.Year}", ct).ConfigureAwait(false);
        var numero = await _repo.SiguienteNumeroEncargoAsync(empresaId, fecha.Year, ct).ConfigureAwait(false);
        var e = EncargoPlanta.Crear(empresaId, numero, fecha, cliente.Id, cliente.Nombre, datos.ProductoId, datos.Plantas, datos.FechaEntrega, datos.PrecioPlanta, datos.Observaciones);
        if (e.EsFallo)
        {
            return Resultado.Fallo<EncargoDto>(e.Error);
        }

        if (datos.LoteId is { } loteId)
        {
            var r = await ReservarEnAsync(e.Valor, loteId, ct).ConfigureAwait(false);
            if (r.EsFallo)
            {
                return Resultado.Fallo<EncargoDto>(r.Error);
            }
        }

        _repo.Agregar(e.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await EncargoDtoAsync(e.Valor, null, ct).ConfigureAwait(false));
    }

    public Task<Resultado<EncargoDto>> ReservarAsync(Guid id, Guid loteId, CancellationToken ct = default) =>
        ConEncargoAsync(id, e => ReservarEnAsync(e, loteId, ct), ct);

    public Task<Resultado<EncargoDto>> LiberarAsync(Guid id, CancellationToken ct = default) => ConEncargoAsync(id, e => Task.FromResult(e.Liberar()), ct);

    /// <summary>Entrega el encargo: las plantas salen de su lote (listo) y se emite el albarán al cliente.</summary>
    public Task<Resultado<EncargoDto>> ServirAsync(Guid empresaId, Guid id, DateOnly? fecha, CancellationToken ct = default) =>
        ConEncargoAsync(id, async e =>
        {
            if (e.Estado != EstadoEncargo.Reservado || e.LoteId is not { } loteId)
            {
                return Resultado.Fallo(Error.Conflicto("encargo.sin_reserva", "Reserva antes un lote para el encargo."));
            }

            if (_ventas is null)
            {
                return Resultado.Fallo(Error.Validacion("encargo.sin_albaran", "No se pueden emitir albaranes."));
            }

            await _unidad.BloquearAsync($"vivero:lote:{loteId}", ct).ConfigureAwait(false);
            var lote = await _repo.LoteAsync(loteId, ct).ConfigureAwait(false);
            var dia = fecha ?? Hoy;
            var reservadas = Reservadas(await _repo.EncargosDeLoteAsync(loteId, ct).ConfigureAwait(false), e.Id);
            var m = lote!.Sacar(dia, TipoMovimientoLote.Entrega, e.Plantas, reservadas, $"Encargo {e.NumeroCompleto} · {e.ClienteNombre}", e.Id, _reloj);
            if (m.EsFallo)
            {
                return Resultado.Fallo(m.Error);
            }

            // Las plantas entran en las existencias del artículo y el albarán las saca: el stock queda como estaba y cuadra.
            if (_existencias is not null)
            {
                var entrada = await _existencias.EntrarAsync(empresaId, e.ProductoId, e.Plantas, $"Vivero: lote {lote.Codigo}, encargo {e.NumeroCompleto}", ct).ConfigureAwait(false);
                if (entrada.EsFallo)
                {
                    return entrada;
                }
            }

            var albaran = await _ventas.AlbaranAsync(empresaId, e.ClienteId, dia, $"Encargo {e.NumeroCompleto} · lote {lote.Codigo} · pasaporte fitosanitario",
                e.ProductoId, e.Plantas, e.PrecioPlanta, ct).ConfigureAwait(false);
            if (albaran.EsFallo)
            {
                return Resultado.Fallo(albaran.Error);
            }

            e.Servido(m.Valor.Id, albaran.Valor.Id, albaran.Valor.Numero);
            return Resultado.Ok();
        }, ct);

    /// <summary>Anula el encargo; servido, anula su albarán y las plantas vuelven al lote.</summary>
    public Task<Resultado<EncargoDto>> AnularEncargoAsync(Guid id, string? motivo, CancellationToken ct = default) =>
        ConEncargoAsync(id, async e =>
        {
            if (e.Estado == EstadoEncargo.Servido)
            {
                if (ReglasVivero.Texto(motivo) is null)
                {
                    return Resultado.Fallo(Error.Validacion("encargo.motivo", "Indica el motivo: la entrega se deshace y su albarán se anula."));
                }

                if (e.AlbaranId is { } albaran && _ventas is not null)
                {
                    var anulado = await _ventas.AnularAlbaranAsync(albaran, $"Anulación del encargo {e.NumeroCompleto}: {motivo}", ct).ConfigureAwait(false);
                    if (anulado.EsFallo)
                    {
                        return Resultado.Fallo(Error.Conflicto(anulado.Error.Codigo, $"El albarán {e.AlbaranNumero} no se puede anular: {anulado.Error.Mensaje}"));
                    }
                }

                var lote = await _repo.LoteAsync(e.LoteId!.Value, ct).ConfigureAwait(false);
                var vuelta = lote!.AnularMovimiento(e.MovimientoId!.Value, Hoy, motivo, _reloj);
                if (vuelta.EsFallo)
                {
                    return Resultado.Fallo(vuelta.Error);
                }

                // El albarán anulado devuelve las plantas al stock; vuelven al lote, así que salen de las existencias.
                if (_existencias is not null)
                {
                    var salida = await _existencias.SacarAsync(e.EmpresaId, e.ProductoId, e.Plantas, $"Vivero: anulación del encargo {e.NumeroCompleto}", ct).ConfigureAwait(false);
                    if (salida.EsFallo)
                    {
                        return salida;
                    }
                }
            }

            return e.Anular();
        }, ct);

    // ------------------------------------------------------------------ Apoyo
    private async Task<Resultado> ReservarEnAsync(EncargoPlanta e, Guid loteId, CancellationToken ct)
    {
        await _unidad.BloquearAsync($"vivero:lote:{loteId}", ct).ConfigureAwait(false);
        var lote = await _repo.LoteAsync(loteId, ct).ConfigureAwait(false);
        if (lote is null || lote.EmpresaId != e.EmpresaId)
        {
            return Resultado.Fallo(Error.NoEncontrado("lote_planta.no_encontrado", "El lote no existe."));
        }

        return e.Reservar(lote, Reservadas(await _repo.EncargosDeLoteAsync(loteId, ct).ConfigureAwait(false), e.Id));
    }

    private static int Reservadas(IEnumerable<EncargoPlanta> encargos, Guid? salvo) =>
        encargos.Where(x => x.Estado == EstadoEncargo.Reservado && x.Id != salvo).Sum(x => x.Plantas);

    private async Task<Resultado<LotePlantaDto>> ConLoteAsync(Guid id, Func<LotePlanta, Task<Resultado>> accion, CancellationToken ct)
    {
        await _unidad.BloquearAsync($"vivero:lote:{id}", ct).ConfigureAwait(false);
        var l = await _repo.LoteAsync(id, ct).ConfigureAwait(false);
        if (l is null)
        {
            return Resultado.Fallo<LotePlantaDto>(Error.NoEncontrado("lote_planta.no_encontrado", "El lote no existe."));
        }

        var r = await accion(l).ConfigureAwait(false);
        if (r.EsFallo)
        {
            return Resultado.Fallo<LotePlantaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(l, await _repo.EncargosDeLoteAsync(id, ct).ConfigureAwait(false), ct).ConfigureAwait(false));
    }

    private async Task<Resultado<EncargoDto>> ConEncargoAsync(Guid id, Func<EncargoPlanta, Task<Resultado>> accion, CancellationToken ct)
    {
        var e = await _repo.EncargoAsync(id, ct).ConfigureAwait(false);
        if (e is null)
        {
            return Resultado.Fallo<EncargoDto>(Error.NoEncontrado("encargo.no_encontrado", "El encargo no existe."));
        }

        var r = await accion(e).ConfigureAwait(false);
        if (r.EsFallo)
        {
            return Resultado.Fallo<EncargoDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        var lote = e.LoteId is { } l ? (await _repo.LoteAsync(l, ct).ConfigureAwait(false))?.Codigo : null;
        return Resultado.Ok(await EncargoDtoAsync(e, lote, ct).ConfigureAwait(false));
    }

    private async Task<LotePlantaDto> DtoAsync(LotePlanta l, IEnumerable<EncargoPlanta> encargos, CancellationToken ct)
    {
        var reservadas = encargos.Where(e => e.LoteId == l.Id && e.Estado == EstadoEncargo.Reservado).Sum(e => e.Plantas);
        var bajas = -l.Movimientos.Where(m => m.Tipo == TipoMovimientoLote.Baja && !l.MovimientoAnulado(m.Id)).Sum(m => m.Plantas);
        var producto = await _productos.ObtenerAsync(l.ProductoId, ct).ConfigureAwait(false);
        return new LotePlantaDto(l.Id, l.Codigo, l.Especie, l.Variedad, l.Portainjerto, l.ProductoId, producto?.Nombre, l.OrigenMaterial, l.FechaSiembra, l.FechaPrevistaLista,
            l.Fase.ToString(), l.Ubicacion, l.PlantasIniciales, l.PlantasVivas, reservadas, Math.Max(0, l.PlantasVivas - reservadas), bajas,
            l.PlantasIniciales == 0 ? 0m : decimal.Round(bajas * 100m / l.PlantasIniciales, 2), l.Terminado, l.Anulado, l.Observaciones,
            l.Movimientos.OrderBy(m => m.Orden).Select(m => new MovimientoLoteDto(m.Id, m.Orden, m.Fecha, m.Tipo.ToString(), m.Plantas, m.Fase.ToString(), m.Ubicacion,
                m.Concepto, m.DocumentoId, m.AnulaId, l.MovimientoAnulado(m.Id))).ToList());
    }

    private async Task<EncargoDto> EncargoDtoAsync(EncargoPlanta e, string? lote, CancellationToken ct)
    {
        var producto = await _productos.ObtenerAsync(e.ProductoId, ct).ConfigureAwait(false);
        if (lote is null && e.LoteId is { } id)
        {
            lote = (await _repo.LoteAsync(id, ct).ConfigureAwait(false))?.Codigo;
        }

        return new EncargoDto(e.Id, e.NumeroCompleto, e.Fecha, e.ClienteId, e.ClienteNombre, e.ProductoId, producto?.Nombre, e.Plantas, e.FechaEntrega, e.PrecioPlanta,
            e.Estado.ToString(), e.LoteId, lote, e.AlbaranId, e.AlbaranNumero, e.Observaciones);
    }
}
