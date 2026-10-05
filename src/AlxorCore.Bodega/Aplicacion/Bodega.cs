using AlxorCore.Bodega.Dominio;
using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Bodega.Aplicacion;

// ----------------------------------------------------------------------------- Contratos
public sealed record DatosDeposito(string? Codigo, string? Nombre, TipoDeposito Tipo, decimal CapacidadLitros, bool Activo = true);

public sealed record ComponenteDto(string Variedad, int Anada, decimal Litros, decimal Porcentaje);

public sealed record DepositoDto(Guid Id, string Codigo, string Nombre, string Tipo, decimal CapacidadLitros, bool Activo, decimal Litros, decimal Ocupacion, string? Producto,
    string? Calificacion, IReadOnlyList<ComponenteDto> Composicion, Guid? UltimaOperacionId)
{
    public static DepositoDto De(Deposito d) => new(d.Id, d.Codigo, d.Nombre, d.Tipo.ToString(), d.CapacidadLitros, d.Activo, d.Litros,
        d.CapacidadLitros == 0m ? 0m : decimal.Round(d.Litros * 100m / d.CapacidadLitros, 1), d.Producto?.ToString(), d.Calificacion,
        d.Composicion.OrderByDescending(c => c.Litros).Select(c => new ComponenteDto(c.Variedad, c.Anada, c.Litros,
            d.Litros == 0m ? 0m : decimal.Round(c.Litros * 100m / d.Litros, 2))).ToList(), d.UltimaOperacionId);
}

public sealed record DatosEntradaUva(Guid ViticultorId, string? Variedad, decimal Kilos, decimal GradoBaume, DateOnly? Fecha = null, string? Parcela = null,
    string? Calificacion = null, string? Observaciones = null);

public sealed record EntradaUvaDto(Guid Id, string Numero, DateOnly Fecha, int Anada, Guid ViticultorId, string Viticultor, string Variedad, decimal Kilos, decimal GradoBaume,
    string? Parcela, string? Calificacion, string? Observaciones, Guid? OperacionId, Guid? LiquidacionId, bool Anulada, string? MotivoAnulacion)
{
    public static EntradaUvaDto De(EntradaUva e) => new(e.Id, e.NumeroCompleto, e.Fecha, e.Anada, e.ViticultorId, e.ViticultorNombre, e.Variedad, e.Kilos, e.GradoBaume,
        e.Parcela, e.Calificacion, e.Observaciones, e.OperacionId, e.LiquidacionId, e.Anulada, e.MotivoAnulacion);
}

public sealed record DatosPrecioUva(int Anada, string? Variedad, decimal PrecioKg, decimal GradoReferencia, decimal PorcentajePorGrado = 0m);

public sealed record PrecioUvaDto(Guid Id, int Anada, string Variedad, decimal PrecioKg, decimal GradoReferencia, decimal PorcentajePorGrado)
{
    public static PrecioUvaDto De(PrecioUva p) => new(p.Id, p.Anada, p.Variedad, p.PrecioKg, p.GradoReferencia, p.PorcentajePorGrado);
}

public sealed record DatosLiquidacionUva(Guid ViticultorId, DateOnly Desde, DateOnly Hasta, DateOnly? Fecha = null, string? CodigoImpuesto = null, decimal PorcentajeRetencion = 0m);

public sealed record LineaLiquidacionUvaDto(Guid EntradaUvaId, string Entrada, string Variedad, decimal Kilos, decimal GradoBaume, decimal PrecioKg, decimal Importe);

public sealed record LiquidacionUvaDto(Guid Id, string Numero, DateOnly Fecha, Guid ViticultorId, string Viticultor, DateOnly Desde, DateOnly Hasta, string CodigoImpuesto,
    decimal PorcentajeRetencion, decimal Kilos, decimal BaseImponible, decimal TotalFactura, Guid? GastoId, string Estado, string? MotivoAnulacion,
    IReadOnlyList<LineaLiquidacionUvaDto> Lineas)
{
    public static LiquidacionUvaDto De(LiquidacionUva l) => new(l.Id, l.NumeroCompleto, l.Fecha, l.ViticultorId, l.ViticultorNombre, l.Desde, l.Hasta, l.CodigoImpuesto,
        l.PorcentajeRetencion, l.Kilos, l.BaseImponible, l.TotalFactura, l.GastoId, l.Estado.ToString(), l.MotivoAnulacion,
        l.Lineas.Select(x => new LineaLiquidacionUvaDto(x.EntradaUvaId, x.Entrada, x.Variedad, x.Kilos, x.GradoBaume, x.PrecioKg, x.Importe)).ToList());
}

public sealed record DatosElaboracion(IReadOnlyList<Guid> EntradaIds, Guid DepositoId, decimal Litros, ProductoVinico Producto, string? Calificacion = null,
    DateOnly? Fecha = null, string? Observaciones = null);

public sealed record DatosTrasiego(Guid OrigenId, Guid DestinoId, decimal? Litros = null, decimal MermaLitros = 0m, DateOnly? Fecha = null, string? Observaciones = null);

public sealed record OrigenCoupage(Guid DepositoId, decimal Litros);

public sealed record DatosCoupage(IReadOnlyList<OrigenCoupage> Origenes, Guid DestinoId, DateOnly? Fecha = null, string? Observaciones = null);

public sealed record DatosMerma(Guid DepositoId, decimal Litros, string? Observaciones, DateOnly? Fecha = null);

public sealed record DatosEmbotellado(Guid DepositoId, Guid ProductoId, int Botellas, decimal FormatoLitros = 0.75m, decimal MermaLitros = 0m, string? Lote = null,
    DateOnly? Fecha = null, string? Observaciones = null);

public sealed record DatosSalidaGranel(Guid DepositoId, Guid ClienteId, Guid ProductoId, decimal Litros, decimal? PrecioLitro = null, DateOnly? Fecha = null,
    string? Observaciones = null);

public sealed record LineaOperacionDto(int Orden, Guid DepositoId, string Deposito, string Clase, decimal Litros, string Producto, string? Calificacion);

public sealed record OperacionBodegaDto(Guid Id, string Numero, DateOnly Fecha, string Tipo, string? Observaciones, decimal MermaLitros, decimal? KilosUva, decimal? Rendimiento,
    Guid? ProductoId, int? Botellas, decimal? FormatoLitros, string? Lote, Guid? ClienteId, Guid? AlbaranId, string? Albaran, bool Anulada, string? MotivoAnulacion,
    IReadOnlyList<LineaOperacionDto> Lineas)
{
    public static OperacionBodegaDto De(OperacionBodega o)
    {
        var entran = o.Lineas.Where(l => l.Clase == ClaseLineaBodega.Entra).Sum(l => l.Litros);
        return new(o.Id, o.NumeroCompleto, o.Fecha, o.Tipo.ToString(), o.Observaciones, o.MermaLitros, o.KilosUva,
            o.KilosUva is > 0m ? decimal.Round(entran * 100m / o.KilosUva.Value, 2) : null, o.ProductoId, o.Botellas, o.FormatoLitros, o.Lote, o.ClienteId, o.AlbaranId,
            o.AlbaranNumero, o.Anulada, o.MotivoAnulacion,
            o.Lineas.OrderBy(l => l.Orden).Select(l => new LineaOperacionDto(l.Orden, l.DepositoId, l.DepositoCodigo, l.Clase.ToString(), l.Litros, l.Producto.ToString(),
                l.Calificacion)).ToList());
    }
}

/// <summary>Una fila de la declaración de existencias y movimientos: un producto y una calificación, en litros.</summary>
public sealed record FilaDeclaracionDto(string Producto, string? Calificacion, decimal Inicial, decimal Elaboracion, decimal EntradasInternas, decimal SalidasInternas,
    decimal Embotellado, decimal Granel, decimal Mermas, decimal Final);

public sealed record DeclaracionBodegaDto(int Anio, int Mes, DateOnly Desde, DateOnly Hasta, IReadOnlyList<FilaDeclaracionDto> Filas, decimal TotalInicial, decimal TotalFinal);

public sealed record OrigenUvaDto(Guid EntradaId, string Entrada, DateOnly Fecha, Guid ViticultorId, string Viticultor, string Variedad, int Anada, decimal Kilos,
    string? Parcela, string? Calificacion);

/// <summary>De qué entradas de uva (y por qué depósitos) puede venir lo que hay en un depósito.</summary>
public sealed record OrigenDepositoDto(DepositoDto Deposito, IReadOnlyList<string> DepositosRecorridos, IReadOnlyList<string> Operaciones, IReadOnlyList<OrigenUvaDto> Uva);

// ----------------------------------------------------------------------------- Maestros y uva
/// <summary>Depósitos, entradas de uva, precios de la uva y liquidaciones al viticultor.</summary>
public sealed class UvaYDepositosBodega
{
    private readonly IRepositorioBodega _repo;
    private readonly IUnidadDeTrabajoBodega _unidad;
    private readonly IConsultaProveedores _proveedores;
    private readonly IReloj _reloj;
    private readonly IAutofacturasBodega? _autofacturas;

    public UvaYDepositosBodega(IRepositorioBodega repo, IUnidadDeTrabajoBodega unidad, IConsultaProveedores proveedores, IReloj reloj, IAutofacturasBodega? autofacturas = null)
    {
        _repo = repo;
        _unidad = unidad;
        _proveedores = proveedores;
        _reloj = reloj;
        _autofacturas = autofacturas;
    }

    private DateOnly Hoy => DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);

    // ------------------------------------------------------------------ Depósitos
    public async Task<IReadOnlyList<DepositoDto>> DepositosAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.DepositosAsync(empresaId, ct).ConfigureAwait(false)).OrderBy(d => d.Codigo, StringComparer.Ordinal).Select(DepositoDto.De).ToList();

    public async Task<DepositoDto?> DepositoAsync(Guid id, CancellationToken ct = default) =>
        await _repo.DepositoAsync(id, ct).ConfigureAwait(false) is { } d ? DepositoDto.De(d) : null;

    public async Task<Resultado<DepositoDto>> CrearDepositoAsync(Guid empresaId, DatosDeposito datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var d = Deposito.Crear(empresaId, datos.Codigo, datos.Nombre, datos.Tipo, datos.CapacidadLitros);
        if (d.EsFallo)
        {
            return Resultado.Fallo<DepositoDto>(d.Error);
        }

        if (await _repo.ExisteDepositoAsync(empresaId, d.Valor.Codigo, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<DepositoDto>(Error.Conflicto("deposito.duplicado", $"Ya existe el depósito {d.Valor.Codigo}."));
        }

        _repo.Agregar(d.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(DepositoDto.De(d.Valor));
    }

    public async Task<Resultado<DepositoDto>> CambiarDepositoAsync(Guid id, DatosDeposito datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var d = await _repo.DepositoAsync(id, ct).ConfigureAwait(false);
        if (d is null)
        {
            return Resultado.Fallo<DepositoDto>(Error.NoEncontrado("deposito.no_encontrado", "El depósito no existe."));
        }

        var r = d.Cambiar(datos.Nombre, datos.Tipo, datos.CapacidadLitros, datos.Activo);
        if (r.EsFallo)
        {
            return Resultado.Fallo<DepositoDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(DepositoDto.De(d));
    }

    public async Task<Resultado> EliminarDepositoAsync(Guid id, CancellationToken ct = default)
    {
        var d = await _repo.DepositoAsync(id, ct).ConfigureAwait(false);
        if (d is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("deposito.no_encontrado", "El depósito no existe."));
        }

        if (!d.Vacio || await _repo.DepositoConOperacionesAsync(id, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo(Error.Conflicto("deposito.en_uso", "El depósito tiene vino u operaciones: dalo de baja en lugar de borrarlo."));
        }

        _repo.Eliminar(d);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    // ------------------------------------------------------------------ Entradas de uva
    public async Task<IReadOnlyList<EntradaUvaDto>> EntradasAsync(Guid empresaId, DateOnly? desde, DateOnly? hasta, Guid? viticultorId, CancellationToken ct = default) =>
        (await _repo.EntradasAsync(empresaId, desde, hasta, viticultorId, ct).ConfigureAwait(false)).Select(EntradaUvaDto.De).ToList();

    public async Task<Resultado<EntradaUvaDto>> CrearEntradaAsync(Guid empresaId, DatosEntradaUva datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var viticultor = await _proveedores.ObtenerAsync(datos.ViticultorId, ct).ConfigureAwait(false);
        if (viticultor is null)
        {
            return Resultado.Fallo<EntradaUvaDto>(Error.NoEncontrado("proveedor.no_encontrado", "El viticultor no existe (es un proveedor)."));
        }

        var fecha = datos.Fecha ?? Hoy;
        await _unidad.BloquearAsync($"bodega:uva:{empresaId}:{fecha.Year}", ct).ConfigureAwait(false);
        var numero = await _repo.SiguienteNumeroEntradaAsync(empresaId, fecha.Year, ct).ConfigureAwait(false);
        var e = EntradaUva.Crear(empresaId, numero, fecha, viticultor.Id, viticultor.Nombre, datos.Variedad, datos.Kilos, datos.GradoBaume, datos.Parcela, datos.Calificacion,
            datos.Observaciones, _reloj);
        if (e.EsFallo)
        {
            return Resultado.Fallo<EntradaUvaDto>(e.Error);
        }

        _repo.Agregar(e.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(EntradaUvaDto.De(e.Valor));
    }

    public async Task<Resultado<EntradaUvaDto>> CorregirEntradaAsync(Guid id, DatosEntradaUva datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var e = await _repo.EntradaAsync(id, ct).ConfigureAwait(false);
        if (e is null)
        {
            return Resultado.Fallo<EntradaUvaDto>(Error.NoEncontrado("uva.no_encontrada", "La entrada de uva no existe."));
        }

        var r = e.Corregir(datos.Variedad, datos.Kilos, datos.GradoBaume, datos.Parcela, datos.Calificacion, datos.Observaciones);
        if (r.EsFallo)
        {
            return Resultado.Fallo<EntradaUvaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(EntradaUvaDto.De(e));
    }

    public async Task<Resultado<EntradaUvaDto>> AnularEntradaAsync(Guid id, string? motivo, CancellationToken ct = default)
    {
        var e = await _repo.EntradaAsync(id, ct).ConfigureAwait(false);
        if (e is null)
        {
            return Resultado.Fallo<EntradaUvaDto>(Error.NoEncontrado("uva.no_encontrada", "La entrada de uva no existe."));
        }

        var r = e.Anular(motivo);
        if (r.EsFallo)
        {
            return Resultado.Fallo<EntradaUvaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(EntradaUvaDto.De(e));
    }

    // ------------------------------------------------------------------ Precios de la uva
    public async Task<IReadOnlyList<PrecioUvaDto>> PreciosAsync(Guid empresaId, int? anada, CancellationToken ct = default) =>
        (await _repo.PreciosAsync(empresaId, anada, ct).ConfigureAwait(false)).OrderByDescending(p => p.Anada).ThenBy(p => p.Variedad, StringComparer.Ordinal)
            .Select(PrecioUvaDto.De).ToList();

    /// <summary>Crea o cambia el precio de una variedad en una añada (uno por variedad y añada).</summary>
    public async Task<Resultado<PrecioUvaDto>> FijarPrecioAsync(Guid empresaId, DatosPrecioUva datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var existente = (await _repo.PreciosAsync(empresaId, datos.Anada, ct).ConfigureAwait(false))
            .FirstOrDefault(p => string.Equals(p.Variedad, datos.Variedad?.Trim(), StringComparison.OrdinalIgnoreCase));
        if (existente is not null)
        {
            var r = existente.Fijar(datos.PrecioKg, datos.GradoReferencia, datos.PorcentajePorGrado);
            if (r.EsFallo)
            {
                return Resultado.Fallo<PrecioUvaDto>(r.Error);
            }

            await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
            return Resultado.Ok(PrecioUvaDto.De(existente));
        }

        var p = PrecioUva.Crear(empresaId, datos.Anada, datos.Variedad, datos.PrecioKg, datos.GradoReferencia, datos.PorcentajePorGrado);
        if (p.EsFallo)
        {
            return Resultado.Fallo<PrecioUvaDto>(p.Error);
        }

        _repo.Agregar(p.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PrecioUvaDto.De(p.Valor));
    }

    public async Task<Resultado> EliminarPrecioAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _repo.PrecioAsync(id, ct).ConfigureAwait(false);
        if (p is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("precio_uva.no_encontrado", "El precio no existe."));
        }

        _repo.Eliminar(p);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    // ------------------------------------------------------------------ Liquidación al viticultor
    public async Task<IReadOnlyList<LiquidacionUvaDto>> LiquidacionesAsync(Guid empresaId, Guid? viticultorId, CancellationToken ct = default) =>
        (await _repo.LiquidacionesAsync(empresaId, viticultorId, ct).ConfigureAwait(false)).Select(LiquidacionUvaDto.De).ToList();

    /// <summary>Calcula la liquidación sin guardar nada.</summary>
    public Task<Resultado<LiquidacionUvaDto>> SimularLiquidacionAsync(Guid empresaId, DatosLiquidacionUva datos, CancellationToken ct = default) =>
        LiquidarAsync(empresaId, datos, false, ct);

    /// <summary>Liquida las entradas pendientes del viticultor y registra la autofactura (gasto).</summary>
    public Task<Resultado<LiquidacionUvaDto>> CrearLiquidacionAsync(Guid empresaId, DatosLiquidacionUva datos, CancellationToken ct = default) =>
        LiquidarAsync(empresaId, datos, true, ct);

    private async Task<Resultado<LiquidacionUvaDto>> LiquidarAsync(Guid empresaId, DatosLiquidacionUva datos, bool guardar, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(datos);
        if (datos.Hasta < datos.Desde)
        {
            return Resultado.Fallo<LiquidacionUvaDto>(Error.Validacion("liquidacion_uva.fechas", "La fecha final es anterior a la inicial."));
        }

        var viticultor = await _proveedores.ObtenerAsync(datos.ViticultorId, ct).ConfigureAwait(false);
        if (viticultor is null)
        {
            return Resultado.Fallo<LiquidacionUvaDto>(Error.NoEncontrado("proveedor.no_encontrado", "El viticultor no existe (es un proveedor)."));
        }

        var codigo = (datos.CodigoImpuesto ?? Impuesto.CompensacionAgricola.Codigo).Trim().ToUpperInvariant();
        if (Impuesto.PorCodigoImpuesto(codigo).EsFallo)
        {
            return Resultado.Fallo<LiquidacionUvaDto>(Error.Validacion("liquidacion_uva.impuesto", $"El tipo {codigo} no existe."));
        }

        var fecha = datos.Fecha ?? Hoy;
        if (guardar)
        {
            await _unidad.BloquearAsync($"bodega:liquidacion:{empresaId}:{fecha.Year}", ct).ConfigureAwait(false);
        }

        var entradas = (await _repo.EntradasAsync(empresaId, datos.Desde, datos.Hasta, viticultor.Id, ct).ConfigureAwait(false))
            .Where(e => !e.Anulada && e.LiquidacionId is null).ToList();
        var precios = await _repo.PreciosAsync(empresaId, null, ct).ConfigureAwait(false);
        var numero = guardar ? await _repo.SiguienteNumeroLiquidacionAsync(empresaId, fecha.Year, ct).ConfigureAwait(false) : 0;
        var l = LiquidacionUva.Crear(empresaId, numero, fecha, viticultor.Id, viticultor.Nombre, datos.Desde, datos.Hasta, codigo, datos.PorcentajeRetencion, entradas,
            precios, _reloj);
        if (l.EsFallo)
        {
            return Resultado.Fallo<LiquidacionUvaDto>(l.Error);
        }

        if (!guardar)
        {
            return Resultado.Ok(LiquidacionUvaDto.De(l.Valor));
        }

        if (_autofacturas is null)
        {
            return Resultado.Fallo<LiquidacionUvaDto>(Error.Validacion("liquidacion_uva.sin_gastos", "No se pueden registrar autofacturas."));
        }

        var gasto = await _autofacturas.RegistrarAsync(empresaId, viticultor.Id, $"Liquidación de uva {l.Valor.NumeroCompleto} (autofactura)", fecha, l.Valor.BaseImponible,
            codigo, datos.PorcentajeRetencion, ct).ConfigureAwait(false);
        if (gasto.EsFallo)
        {
            return Resultado.Fallo<LiquidacionUvaDto>(gasto.Error);
        }

        l.Valor.Registrada(gasto.Valor.GastoId, gasto.Valor.Total);
        _repo.Agregar(l.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(LiquidacionUvaDto.De(l.Valor));
    }

    /// <summary>Anula la liquidación y su autofactura (si no está pagada); sus entradas vuelven a estar pendientes.</summary>
    public async Task<Resultado<LiquidacionUvaDto>> AnularLiquidacionAsync(Guid id, string? motivo, CancellationToken ct = default)
    {
        var l = await _repo.LiquidacionAsync(id, ct).ConfigureAwait(false);
        if (l is null)
        {
            return Resultado.Fallo<LiquidacionUvaDto>(Error.NoEncontrado("liquidacion_uva.no_encontrada", "La liquidación no existe."));
        }

        var entradas = await _repo.EntradasAsync(l.Lineas.Select(x => x.EntradaUvaId).ToList(), ct).ConfigureAwait(false);
        var r = l.Anular(motivo, entradas);
        if (r.EsFallo)
        {
            return Resultado.Fallo<LiquidacionUvaDto>(r.Error);
        }

        if (l.GastoId is { } gasto && _autofacturas is not null)
        {
            var anulado = await _autofacturas.AnularAsync(gasto, ct).ConfigureAwait(false);
            if (anulado.EsFallo)
            {
                return Resultado.Fallo<LiquidacionUvaDto>(anulado.Error);
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(LiquidacionUvaDto.De(l));
    }
}

// ----------------------------------------------------------------------------- Operaciones
/// <summary>
/// Operaciones de bodega: elaboración, trasiego, coupage, merma, embotellado (a existencias) y salida a granel (con
/// albarán); su anulación (la última de cada depósito), la declaración de existencias y movimientos y la trazabilidad
/// de un depósito hasta la uva.
/// </summary>
public sealed class OperacionesBodega
{
    private readonly IRepositorioBodega _repo;
    private readonly IUnidadDeTrabajoBodega _unidad;
    private readonly IConsultaProductos _productos;
    private readonly IReloj _reloj;
    private readonly IExistenciasBodega? _existencias;
    private readonly IVentasBodega? _ventas;
    private readonly AlxorCore.Terceros.Aplicacion.IConsultaClientes? _clientes;

    public OperacionesBodega(IRepositorioBodega repo, IUnidadDeTrabajoBodega unidad, IConsultaProductos productos, IReloj reloj, IExistenciasBodega? existencias = null,
        IVentasBodega? ventas = null, AlxorCore.Terceros.Aplicacion.IConsultaClientes? clientes = null)
    {
        _repo = repo;
        _unidad = unidad;
        _productos = productos;
        _reloj = reloj;
        _existencias = existencias;
        _ventas = ventas;
        _clientes = clientes;
    }

    private DateOnly Hoy => DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);

    public async Task<IReadOnlyList<OperacionBodegaDto>> ListarAsync(Guid empresaId, DateOnly? desde, DateOnly? hasta, Guid? depositoId, CancellationToken ct = default)
    {
        var lista = depositoId is { } d
            ? await _repo.OperacionesDeDepositoAsync(d, ct).ConfigureAwait(false)
            : await _repo.OperacionesAsync(empresaId, desde, hasta, ct).ConfigureAwait(false);
        return lista.Where(o => (desde is null || o.Fecha >= desde) && (hasta is null || o.Fecha <= hasta))
            .OrderByDescending(o => o.Fecha).ThenByDescending(o => o.Numero).Select(OperacionBodegaDto.De).ToList();
    }

    public async Task<OperacionBodegaDto?> ObtenerAsync(Guid id, CancellationToken ct = default) =>
        await _repo.OperacionAsync(id, ct).ConfigureAwait(false) is { } o ? OperacionBodegaDto.De(o) : null;

    /// <summary>Uva a mosto o vino: los litros obtenidos entran en el depósito con la composición de las entradas (por kilos).</summary>
    public Task<Resultado<OperacionBodegaDto>> ElaborarAsync(Guid empresaId, DatosElaboracion datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        return OperarAsync(empresaId, TipoOperacionBodega.Elaboracion, datos.Fecha, datos.Observaciones, [datos.DepositoId], async (op, depositos) =>
        {
            var ids = (datos.EntradaIds ?? []).Distinct().ToList();
            if (ids.Count == 0)
            {
                return Resultado.Fallo(Error.Validacion("elaboracion.sin_uva", "Elige las entradas de uva que se elaboran."));
            }

            var entradas = await _repo.EntradasAsync(ids, ct).ConfigureAwait(false);
            if (entradas.Count != ids.Count || entradas.Any(e => e.EmpresaId != empresaId))
            {
                return Resultado.Fallo(Error.NoEncontrado("uva.no_encontrada", "Alguna entrada de uva no existe."));
            }

            var kilos = entradas.Sum(e => e.Kilos);
            if (!ReglasBodega.LitrosValidos(datos.Litros))
            {
                return Resultado.Fallo(Error.Validacion("bodega.litros", "Los litros obtenidos deben ser positivos (hasta 2 decimales)."));
            }

            // Más de 100 litros por cada 100 kg de uva no es posible.
            if (datos.Litros > kilos)
            {
                return Resultado.Fallo(Error.Validacion("elaboracion.rendimiento",
                    $"De {Redondeo.Formatear(kilos)} kg de uva no salen {Redondeo.Formatear(datos.Litros)} l (más de 100 l por 100 kg)."));
            }

            foreach (var e in entradas)
            {
                if (e.Elaborar(op.Id) is { EsFallo: true } r)
                {
                    return r;
                }
            }

            var grupos = entradas.GroupBy(e => (e.Variedad, e.Anada)).ToList();
            var litros = ReglasBodega.Repartir(datos.Litros, grupos.Select(g => g.Sum(e => e.Kilos)).ToList());
            var composicion = grupos.Select((g, i) => new ComponenteVino(g.Key.Variedad, g.Key.Anada, litros[i])).ToList();
            var calificaciones = entradas.Select(e => e.Calificacion).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var calificacion = ReglasBodega.Texto(datos.Calificacion) ?? (calificaciones.Count == 1 ? calificaciones[0] : null);
            if (datos.Calificacion is not null && entradas.Any(e => !string.Equals(e.Calificacion, calificacion, StringComparison.OrdinalIgnoreCase)))
            {
                return Resultado.Fallo(Error.Validacion("elaboracion.calificacion", $"No toda la uva puede ir a «{calificacion}»."));
            }

            op.Elaboracion(kilos);
            return op.Meter(depositos[datos.DepositoId], datos.Litros, datos.Producto, calificacion, composicion);
        }, ct);
    }

    /// <summary>De un depósito a otro (todo o una parte), con la merma de la operación.</summary>
    public Task<Resultado<OperacionBodegaDto>> TrasegarAsync(Guid empresaId, DatosTrasiego datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        if (datos.OrigenId == datos.DestinoId)
        {
            return Task.FromResult(Resultado.Fallo<OperacionBodegaDto>(Error.Validacion("trasiego.mismo", "El origen y el destino son el mismo depósito.")));
        }

        return OperarAsync(empresaId, TipoOperacionBodega.Trasiego, datos.Fecha, datos.Observaciones, [datos.OrigenId, datos.DestinoId], (op, depositos) =>
        {
            var origen = depositos[datos.OrigenId];
            var merma = datos.MermaLitros;
            if (merma < 0m || ReglasBodega.Litros(merma) != merma)
            {
                return Task.FromResult(Resultado.Fallo(Error.Validacion("bodega.merma", "La merma no puede ser negativa (hasta 2 decimales).")));
            }

            var litros = datos.Litros ?? origen.Litros - merma;
            return Task.FromResult(Mover(op, origen, depositos[datos.DestinoId], litros, merma));
        }, ct);
    }

    /// <summary>Mezcla: los litros de varios depósitos entran en uno.</summary>
    public Task<Resultado<OperacionBodegaDto>> CoupageAsync(Guid empresaId, DatosCoupage datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var origenes = datos.Origenes ?? [];
        if (origenes.Count < 2 || origenes.Select(o => o.DepositoId).Distinct().Count() != origenes.Count)
        {
            return Task.FromResult(Resultado.Fallo<OperacionBodegaDto>(Error.Validacion("coupage.origenes", "Un coupage mezcla al menos dos depósitos distintos.")));
        }

        if (origenes.Any(o => o.DepositoId == datos.DestinoId))
        {
            return Task.FromResult(Resultado.Fallo<OperacionBodegaDto>(Error.Validacion("coupage.destino", "El destino no puede ser uno de los depósitos de origen.")));
        }

        return OperarAsync(empresaId, TipoOperacionBodega.Coupage, datos.Fecha, datos.Observaciones, [.. origenes.Select(o => o.DepositoId), datos.DestinoId], (op, depositos) =>
        {
            var destino = depositos[datos.DestinoId];
            foreach (var o in origenes)
            {
                var r = Mover(op, depositos[o.DepositoId], destino, o.Litros, 0m);
                if (r.EsFallo)
                {
                    return Task.FromResult(r);
                }
            }

            return Task.FromResult(Resultado.Ok());
        }, ct);
    }

    public Task<Resultado<OperacionBodegaDto>> MermaAsync(Guid empresaId, DatosMerma datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        if (ReglasBodega.Texto(datos.Observaciones) is null)
        {
            return Task.FromResult(Resultado.Fallo<OperacionBodegaDto>(Error.Validacion("bodega.motivo", "Indica el motivo de la merma (evaporación, lías…).")));
        }

        return OperarAsync(empresaId, TipoOperacionBodega.Merma, datos.Fecha, datos.Observaciones, [datos.DepositoId], (op, depositos) =>
        {
            var r = op.Sacar(depositos[datos.DepositoId], datos.Litros, ClaseLineaBodega.Merma);
            op.Merma(datos.Litros);
            return Task.FromResult(r.EsFallo ? Resultado.Fallo(r.Error) : Resultado.Ok());
        }, ct);
    }

    /// <summary>Del depósito salen las botellas × su formato (más la merma) y las botellas entran en las existencias del artículo.</summary>
    public Task<Resultado<OperacionBodegaDto>> EmbotellarAsync(Guid empresaId, DatosEmbotellado datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        return OperarAsync(empresaId, TipoOperacionBodega.Embotellado, datos.Fecha, datos.Observaciones, [datos.DepositoId], async (op, depositos) =>
        {
            var producto = await _productos.ObtenerAsync(datos.ProductoId, ct).ConfigureAwait(false);
            if (producto is null)
            {
                return Resultado.Fallo(Error.NoEncontrado("producto.no_encontrado", "El artículo embotellado no existe."));
            }

            if (datos.Botellas <= 0 || datos.FormatoLitros is <= 0m or > 50m || datos.MermaLitros < 0m)
            {
                return Resultado.Fallo(Error.Validacion("embotellado.datos", "Indica las botellas, su formato en litros (0,75…) y la merma (o cero)."));
            }

            var litros = ReglasBodega.Litros(datos.Botellas * datos.FormatoLitros);
            var sale = op.Sacar(depositos[datos.DepositoId], litros, ClaseLineaBodega.Sale);
            if (sale.EsFallo)
            {
                return Resultado.Fallo(sale.Error);
            }

            if (datos.MermaLitros > 0m && op.Sacar(depositos[datos.DepositoId], ReglasBodega.Litros(datos.MermaLitros), ClaseLineaBodega.Merma) is { EsFallo: true } m)
            {
                return Resultado.Fallo(m.Error);
            }

            var lote = ReglasBodega.Texto(datos.Lote) ?? op.NumeroCompleto;
            op.Embotellado(producto.Id, datos.Botellas, datos.FormatoLitros, lote.Length > 40 ? lote[..40] : lote);
            op.Merma(ReglasBodega.Litros(datos.MermaLitros));
            if (_existencias is not null)
            {
                var entrada = await _existencias.EntrarAsync(empresaId, producto.Id, datos.Botellas, $"Embotellado {op.NumeroCompleto} lote {op.Lote}", ct).ConfigureAwait(false);
                if (entrada.EsFallo)
                {
                    return entrada;
                }
            }

            return Resultado.Ok();
        }, ct);
    }

    /// <summary>Venta a granel: salen los litros y se emite el albarán al cliente (en la unidad del artículo, litros).</summary>
    public Task<Resultado<OperacionBodegaDto>> SalidaGranelAsync(Guid empresaId, DatosSalidaGranel datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        return OperarAsync(empresaId, TipoOperacionBodega.SalidaGranel, datos.Fecha, datos.Observaciones, [datos.DepositoId], async (op, depositos) =>
        {
            if (_ventas is null)
            {
                return Resultado.Fallo(Error.Validacion("granel.sin_albaran", "No se pueden emitir albaranes."));
            }

            if (_clientes is not null && await _clientes.ObtenerAsync(datos.ClienteId, ct).ConfigureAwait(false) is null)
            {
                return Resultado.Fallo(Error.NoEncontrado("cliente.no_encontrado", "El cliente no existe."));
            }

            if (await _productos.ObtenerAsync(datos.ProductoId, ct).ConfigureAwait(false) is null)
            {
                return Resultado.Fallo(Error.NoEncontrado("producto.no_encontrado", "El artículo (vino a granel) no existe."));
            }

            var sale = op.Sacar(depositos[datos.DepositoId], datos.Litros, ClaseLineaBodega.Sale);
            if (sale.EsFallo)
            {
                return Resultado.Fallo(sale.Error);
            }

            op.Granel(datos.ClienteId, datos.ProductoId);
            var albaran = await _ventas.AlbaranAsync(empresaId, datos.ClienteId, op.Fecha, $"Granel {op.NumeroCompleto} · depósito {depositos[datos.DepositoId].Codigo}",
                datos.ProductoId, datos.Litros, datos.PrecioLitro, ct).ConfigureAwait(false);
            if (albaran.EsFallo)
            {
                return Resultado.Fallo(albaran.Error);
            }

            op.Albaran(albaran.Valor.Id, albaran.Valor.Numero);
            return Resultado.Ok();
        }, ct);
    }

    /// <summary>
    /// Deshace la operación (si es la última de todos sus depósitos): los depósitos vuelven a como estaban, la uva vuelve
    /// a estar pendiente de elaborar, las botellas salen de las existencias y el albarán de granel se anula.
    /// </summary>
    public async Task<Resultado<OperacionBodegaDto>> AnularAsync(Guid empresaId, Guid id, string? motivo, CancellationToken ct = default)
    {
        await _unidad.BloquearAsync($"bodega:operaciones:{empresaId}", ct).ConfigureAwait(false);
        var op = await _repo.OperacionAsync(id, ct).ConfigureAwait(false);
        if (op is null)
        {
            return Resultado.Fallo<OperacionBodegaDto>(Error.NoEncontrado("operacion_bodega.no_encontrada", "La operación no existe."));
        }

        var depositos = (await _repo.DepositosAsync(op.Lineas.Select(l => l.DepositoId).Distinct().ToList(), ct).ConfigureAwait(false)).ToDictionary(d => d.Id);
        var r = op.Anular(motivo, depositos);
        if (r.EsFallo)
        {
            return Resultado.Fallo<OperacionBodegaDto>(r.Error);
        }

        if (op.Tipo == TipoOperacionBodega.Elaboracion)
        {
            foreach (var e in await _repo.EntradasDeOperacionesAsync([op.Id], ct).ConfigureAwait(false))
            {
                e.DeshacerElaboracion();
            }
        }

        if (op.Tipo == TipoOperacionBodega.Embotellado && op.ProductoId is { } producto && op.Botellas is { } botellas && _existencias is not null)
        {
            var salida = await _existencias.SacarAsync(empresaId, producto, botellas, $"Anulación del embotellado {op.NumeroCompleto}", ct).ConfigureAwait(false);
            if (salida.EsFallo)
            {
                return Resultado.Fallo<OperacionBodegaDto>(Error.Conflicto(salida.Error.Codigo, $"Las botellas no se pueden sacar de las existencias: {salida.Error.Mensaje}"));
            }
        }

        if (op.AlbaranId is { } albaran && _ventas is not null)
        {
            var anulado = await _ventas.AnularAlbaranAsync(albaran, $"Anulación de la salida a granel {op.NumeroCompleto}", ct).ConfigureAwait(false);
            if (anulado.EsFallo)
            {
                return Resultado.Fallo<OperacionBodegaDto>(Error.Conflicto(anulado.Error.Codigo, $"El albarán {op.AlbaranNumero} no se puede anular: {anulado.Error.Mensaje}"));
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(OperacionBodegaDto.De(op));
    }

    /// <summary>
    /// Declaración mensual de existencias y movimientos por producto y calificación, en litros: existencia inicial,
    /// elaboración, entradas y salidas internas (trasiegos, coupages y reclasificaciones), embotellado, granel, mermas y
    /// existencia final. Es el resumen que se traslada a la declaración oficial (INFOVI); no genera su fichero.
    /// </summary>
    public async Task<DeclaracionBodegaDto> DeclaracionAsync(Guid empresaId, int anio, int mes, CancellationToken ct = default)
    {
        var desde = new DateOnly(anio, Math.Clamp(mes, 1, 12), 1);
        var hasta = desde.AddMonths(1).AddDays(-1);
        var libro = await _repo.LibroAsync(empresaId, hasta, ct).ConfigureAwait(false);
        var filas = libro.GroupBy(m => (m.Producto, Calificacion: m.Calificacion?.ToUpperInvariant()))
            .Select(g =>
            {
                var antes = g.Where(m => m.Fecha < desde).Sum(m => m.Litros);
                var mesL = g.Where(m => m.Fecha >= desde).ToList();
                decimal Suma(Func<MovimientoLibroBodega, bool> f) => mesL.Where(f).Sum(m => m.Litros);
                var interno = (Func<MovimientoLibroBodega, bool>)(m => m.Clase == ClaseLineaBodega.Reclasifica
                    || (m.Tipo is TipoOperacionBodega.Trasiego or TipoOperacionBodega.Coupage && m.Clase is ClaseLineaBodega.Entra or ClaseLineaBodega.Sale));
                return new FilaDeclaracionDto(g.Key.Producto.ToString(), g.First().Calificacion, antes,
                    Suma(m => m.Tipo == TipoOperacionBodega.Elaboracion && m.Clase == ClaseLineaBodega.Entra),
                    Suma(m => interno(m) && m.Litros > 0m), -Suma(m => interno(m) && m.Litros < 0m),
                    -Suma(m => m.Tipo == TipoOperacionBodega.Embotellado && m.Clase == ClaseLineaBodega.Sale),
                    -Suma(m => m.Tipo == TipoOperacionBodega.SalidaGranel && m.Clase == ClaseLineaBodega.Sale),
                    -Suma(m => m.Clase == ClaseLineaBodega.Merma),
                    antes + mesL.Sum(m => m.Litros));
            })
            .Where(f => f.Inicial != 0m || f.Final != 0m || f.Elaboracion != 0m || f.EntradasInternas != 0m || f.SalidasInternas != 0m || f.Embotellado != 0m
                        || f.Granel != 0m || f.Mermas != 0m)
            .OrderBy(f => f.Producto, StringComparer.Ordinal).ThenBy(f => f.Calificacion, StringComparer.Ordinal).ToList();
        return new DeclaracionBodegaDto(anio, desde.Month, desde, hasta, filas, filas.Sum(f => f.Inicial), filas.Sum(f => f.Final));
    }

    /// <summary>
    /// De qué uva puede venir lo que hay en el depósito: recorre hacia atrás las operaciones vivas que le metieron vino
    /// (y las de los depósitos de donde vino) hasta las elaboraciones y sus entradas de uva.
    /// </summary>
    public async Task<Resultado<OrigenDepositoDto>> OrigenAsync(Guid depositoId, CancellationToken ct = default)
    {
        var deposito = await _repo.DepositoAsync(depositoId, ct).ConfigureAwait(false);
        if (deposito is null)
        {
            return Resultado.Fallo<OrigenDepositoDto>(Error.NoEncontrado("deposito.no_encontrado", "El depósito no existe."));
        }

        var visitados = new HashSet<Guid> { depositoId };
        var pendientes = new Queue<Guid>([depositoId]);
        var operaciones = new Dictionary<Guid, OperacionBodega>();
        while (pendientes.Count > 0)
        {
            var d = pendientes.Dequeue();
            foreach (var op in (await _repo.OperacionesDeDepositoAsync(d, ct).ConfigureAwait(false))
                     .Where(o => !o.Anulada && o.Lineas.Any(l => l.DepositoId == d && l.Clase == ClaseLineaBodega.Entra)))
            {
                operaciones.TryAdd(op.Id, op);
                foreach (var origen in op.Lineas.Where(l => l.Clase == ClaseLineaBodega.Sale).Select(l => l.DepositoId).Where(visitados.Add))
                {
                    pendientes.Enqueue(origen);
                }
            }
        }

        var elaboraciones = operaciones.Values.Where(o => o.Tipo == TipoOperacionBodega.Elaboracion).Select(o => o.Id).ToHashSet();
        var uva = (await _repo.EntradasDeOperacionesAsync(elaboraciones, ct).ConfigureAwait(false))
            .OrderBy(e => e.Fecha).ThenBy(e => e.Numero)
            .Select(e => new OrigenUvaDto(e.Id, e.NumeroCompleto, e.Fecha, e.ViticultorId, e.ViticultorNombre, e.Variedad, e.Anada, e.Kilos, e.Parcela, e.Calificacion)).ToList();
        var codigos = (await _repo.DepositosAsync(visitados.ToList(), ct).ConfigureAwait(false)).Select(x => x.Codigo).OrderBy(c => c, StringComparer.Ordinal).ToList();
        return Resultado.Ok(new OrigenDepositoDto(DepositoDto.De(deposito), codigos,
            operaciones.Values.OrderBy(o => o.Fecha).ThenBy(o => o.Numero).Select(o => o.NumeroCompleto).ToList(), uva));
    }

    // ------------------------------------------------------------------ Apoyo
    /// <summary>Saca de un depósito litros + merma y mete los litros en otro con su composición.</summary>
    private static Resultado Mover(OperacionBodega op, Deposito origen, Deposito destino, decimal litros, decimal merma)
    {
        if (origen.Vacio)
        {
            return Resultado.Fallo(Error.Conflicto("deposito.sin_litros", $"El depósito {origen.Codigo} está vacío."));
        }

        var producto = origen.Producto!.Value;
        var calificacion = origen.Calificacion;
        var sale = op.Sacar(origen, litros, ClaseLineaBodega.Sale);
        if (sale.EsFallo)
        {
            return Resultado.Fallo(sale.Error);
        }

        if (merma > 0m)
        {
            var perdido = op.Sacar(origen, merma, ClaseLineaBodega.Merma);
            if (perdido.EsFallo)
            {
                return Resultado.Fallo(perdido.Error);
            }

            op.Merma(op.MermaLitros + merma);
        }

        return op.Meter(destino, litros, producto, calificacion, sale.Valor);
    }

    /// <summary>Numera la operación, bloquea sus depósitos, la ejecuta y la guarda (o nada si falla).</summary>
    private async Task<Resultado<OperacionBodegaDto>> OperarAsync(Guid empresaId, TipoOperacionBodega tipo, DateOnly? fecha, string? observaciones,
        IReadOnlyList<Guid> depositoIds, Func<OperacionBodega, IReadOnlyDictionary<Guid, Deposito>, Task<Resultado>> accion, CancellationToken ct)
    {
        var dia = fecha ?? Hoy;
        await _unidad.BloquearAsync($"bodega:operaciones:{empresaId}", ct).ConfigureAwait(false);
        var depositos = (await _repo.DepositosAsync(depositoIds.Distinct().ToList(), ct).ConfigureAwait(false)).Where(d => d.EmpresaId == empresaId).ToDictionary(d => d.Id);
        if (depositos.Count != depositoIds.Distinct().Count())
        {
            return Resultado.Fallo<OperacionBodegaDto>(Error.NoEncontrado("deposito.no_encontrado", "Algún depósito no existe."));
        }

        var numero = await _repo.SiguienteNumeroOperacionAsync(empresaId, dia.Year, ct).ConfigureAwait(false);
        var op = OperacionBodega.Crear(empresaId, numero, dia, tipo, observaciones, _reloj);
        if (op.EsFallo)
        {
            return Resultado.Fallo<OperacionBodegaDto>(op.Error);
        }

        var r = await accion(op.Valor, depositos).ConfigureAwait(false);
        if (r.EsFallo)
        {
            return Resultado.Fallo<OperacionBodegaDto>(r.Error);
        }

        _repo.Agregar(op.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(OperacionBodegaDto.De(op.Valor));
    }
}
