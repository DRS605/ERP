using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Facturacion.Dominio;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Tesoreria.Aplicacion;
using AlxorCore.Tesoreria.Dominio;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Api.Comun;

public sealed record DatosAgente(string? Nombre, decimal Porcentaje, DevengoComision Devengo = DevengoComision.Facturado, Guid? ProveedorId = null,
    decimal PorcentajeIrpf = 0m, bool Activo = true);

public sealed record AgenteDto(Guid Id, string Nombre, decimal Porcentaje, string Devengo, Guid? ProveedorId, decimal PorcentajeIrpf, bool Activo, int Clientes);

public sealed record DatosAsignacionAgente(Guid ClienteId, Guid? AgenteId, DateOnly? Desde = null);

public sealed record AsignacionDto(Guid Id, Guid ClienteId, Guid? AgenteId, DateOnly Desde);

public sealed record DatosReglaComision(Guid AgenteId, decimal Porcentaje, Guid? FamiliaId = null, Guid? ClienteId = null);

public sealed record ReglaComisionDto(Guid Id, Guid AgenteId, Guid? FamiliaId, Guid? ClienteId, decimal Porcentaje);

public sealed record LineaComisionDto(Guid FacturaId, string Factura, DateOnly Fecha, string? Cliente, decimal BaseVenta, decimal Comision, decimal PorcentajeDevengado,
    decimal YaLiquidado, decimal Pendiente);

public sealed record CalculoComisionesDto(Guid AgenteId, string Agente, DateOnly Desde, DateOnly Hasta, string Devengo, decimal BaseVenta, decimal Pendiente,
    IReadOnlyList<LineaComisionDto> Lineas);

public sealed record DatosLiquidacionAgente(Guid AgenteId, DateOnly Desde, DateOnly Hasta, DateOnly? Fecha = null, bool GenerarFactura = false,
    string? NumeroFacturaAgente = null, string CodigoIva = "IVA21");

public sealed record LiquidacionAgenteDto(Guid Id, string Numero, Guid AgenteId, string? Agente, DateOnly Fecha, DateOnly Desde, DateOnly Hasta, string Estado, decimal Importe,
    Guid? GastoId, IReadOnlyList<LineaComisionDto> Lineas);

/// <summary>
/// Comisiones de agentes y vendedores: cada factura emitida a un cliente es del agente que lo llevaba en su fecha, con
/// la comisión de la regla más concreta (cliente y familia, cliente, familia o la general del agente). Se devenga al
/// facturar o, si el agente cobra al cobro, en proporción a lo cobrado. La liquidación congela lo devengado y aún no
/// liquidado: una factura no se liquida dos veces, y si después se rectifica o se anula, la siguiente liquidación lo
/// descuenta. Si el agente es externo, genera su factura de comisiones (con su retención). Los vendedores en nómina
/// solo tienen el informe: la nómina queda fuera del ERP.
/// </summary>
public sealed class ComisionesAgentes
{
    private readonly IRepositorioComisiones _repo;
    private readonly IUnidadDeTrabajoFacturacion _unidad;
    private readonly IConsultaFacturas _facturas;
    private readonly IConsultaTesoreria _tesoreria;
    private readonly IConsultaProductos _productos;
    private readonly IConsultaProveedores _proveedores;
    private readonly RegistrarGasto _registrarGasto;
    private readonly AnularGasto _anularGasto;
    private readonly IReloj _reloj;

    public ComisionesAgentes(IRepositorioComisiones repo, IUnidadDeTrabajoFacturacion unidad, IConsultaFacturas facturas, IConsultaTesoreria tesoreria,
        IConsultaProductos productos, IConsultaProveedores proveedores, RegistrarGasto registrarGasto, AnularGasto anularGasto, IReloj reloj)
    {
        _repo = repo; _unidad = unidad; _facturas = facturas; _tesoreria = tesoreria; _productos = productos; _proveedores = proveedores;
        _registrarGasto = registrarGasto; _anularGasto = anularGasto; _reloj = reloj;
    }

    private DateOnly Hoy => DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);

    // ------------------------------------------------------------------ Agentes, clientes y reglas

    public async Task<IReadOnlyList<AgenteDto>> AgentesAsync(Guid empresaId, CancellationToken ct = default)
    {
        var vigentes = Vigentes(await _repo.AsignacionesAsync(empresaId, ct).ConfigureAwait(false), Hoy);
        return (await _repo.AgentesAsync(empresaId, ct).ConfigureAwait(false)).OrderBy(a => a.Nombre, StringComparer.CurrentCulture)
            .Select(a => new AgenteDto(a.Id, a.Nombre, a.Porcentaje, a.Devengo.ToString(), a.ProveedorId, a.PorcentajeIrpf, a.Activo, vigentes.Count(v => v.Value == a.Id))).ToList();
    }

    public async Task<Resultado<AgenteDto>> GuardarAgenteAsync(Guid empresaId, Guid? id, DatosAgente d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        if (d.ProveedorId is { } p && await _proveedores.ObtenerAsync(p, ct).ConfigureAwait(false) is null)
        {
            return Resultado.Fallo<AgenteDto>(Error.NoEncontrado("agente.proveedor", "El proveedor no existe."));
        }

        AgenteComercial agente;
        if (id is { } existente)
        {
            agente = await _repo.AgenteAsync(existente, ct).ConfigureAwait(false) ?? null!;
            if (agente is null)
            {
                return Resultado.Fallo<AgenteDto>(AgenteNoEncontrado());
            }

            var c = agente.Cambiar(d.Nombre, d.Porcentaje, d.Devengo, d.ProveedorId, d.PorcentajeIrpf, d.Activo);
            if (c.EsFallo)
            {
                return Resultado.Fallo<AgenteDto>(c.Error);
            }
        }
        else
        {
            var n = AgenteComercial.Crear(empresaId, d.Nombre, d.Porcentaje, d.Devengo, d.ProveedorId, d.PorcentajeIrpf);
            if (n.EsFallo)
            {
                return Resultado.Fallo<AgenteDto>(n.Error);
            }

            agente = n.Valor;
            _repo.Agregar(agente);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new AgenteDto(agente.Id, agente.Nombre, agente.Porcentaje, agente.Devengo.ToString(), agente.ProveedorId, agente.PorcentajeIrpf, agente.Activo, 0));
    }

    public async Task<IReadOnlyList<AsignacionDto>> AsignacionesAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.AsignacionesAsync(empresaId, ct).ConfigureAwait(false)).OrderBy(a => a.ClienteId).ThenByDescending(a => a.Desde)
            .Select(a => new AsignacionDto(a.Id, a.ClienteId, a.AgenteId, a.Desde)).ToList();

    /// <summary>Asigna (o quita, con agente null) el agente de un cliente desde una fecha; las facturas anteriores siguen con el de entonces.</summary>
    public async Task<Resultado<AsignacionDto>> AsignarAsync(Guid empresaId, DatosAsignacionAgente d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        if (d.AgenteId is { } a && (await _repo.AgenteAsync(a, ct).ConfigureAwait(false))?.EmpresaId != empresaId)
        {
            return Resultado.Fallo<AsignacionDto>(AgenteNoEncontrado());
        }

        var desde = d.Desde ?? Hoy;
        var asignacion = (await _repo.AsignacionesAsync(empresaId, ct).ConfigureAwait(false)).FirstOrDefault(x => x.ClienteId == d.ClienteId && x.Desde == desde);
        if (asignacion is null)
        {
            asignacion = AsignacionAgente.Crear(empresaId, d.ClienteId, d.AgenteId, desde);
            _repo.Agregar(asignacion);
        }
        else
        {
            asignacion.Cambiar(d.AgenteId);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new AsignacionDto(asignacion.Id, asignacion.ClienteId, asignacion.AgenteId, asignacion.Desde));
    }

    public async Task<IReadOnlyList<ReglaComisionDto>> ReglasAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.ReglasAsync(empresaId, ct).ConfigureAwait(false)).Select(r => new ReglaComisionDto(r.Id, r.AgenteId, r.FamiliaId, r.ClienteId, r.Porcentaje)).ToList();

    public async Task<Resultado<ReglaComisionDto>> CrearReglaAsync(Guid empresaId, DatosReglaComision d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        if ((await _repo.AgenteAsync(d.AgenteId, ct).ConfigureAwait(false))?.EmpresaId != empresaId)
        {
            return Resultado.Fallo<ReglaComisionDto>(AgenteNoEncontrado());
        }

        if ((await _repo.ReglasAsync(empresaId, ct).ConfigureAwait(false)).Any(r => r.AgenteId == d.AgenteId && r.FamiliaId == d.FamiliaId && r.ClienteId == d.ClienteId))
        {
            return Resultado.Fallo<ReglaComisionDto>(Error.Conflicto("comision.regla_repetida", "Ya hay una regla del agente para esa familia y cliente: quítala antes."));
        }

        var r = ReglaComision.Crear(empresaId, d.AgenteId, d.FamiliaId, d.ClienteId, d.Porcentaje);
        if (r.EsFallo)
        {
            return Resultado.Fallo<ReglaComisionDto>(r.Error);
        }

        _repo.Agregar(r.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new ReglaComisionDto(r.Valor.Id, r.Valor.AgenteId, r.Valor.FamiliaId, r.Valor.ClienteId, r.Valor.Porcentaje));
    }

    public async Task<Resultado> EliminarReglaAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _repo.ReglaAsync(id, ct).ConfigureAwait(false);
        if (r is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("comision.regla", "La regla no existe."));
        }

        _repo.Eliminar(r);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    // ------------------------------------------------------------------ Cálculo y liquidación

    /// <summary>Comisiones pendientes de liquidar de un agente: facturas del periodo (o, al cobro, cobradas hasta la fecha).</summary>
    public async Task<Resultado<CalculoComisionesDto>> CalcularAsync(Guid empresaId, Guid agenteId, DateOnly desde, DateOnly hasta, CancellationToken ct = default)
    {
        var agente = await _repo.AgenteAsync(agenteId, ct).ConfigureAwait(false);
        if (agente is null || agente.EmpresaId != empresaId)
        {
            return Resultado.Fallo<CalculoComisionesDto>(AgenteNoEncontrado());
        }

        if (hasta < desde)
        {
            return Resultado.Fallo<CalculoComisionesDto>(Error.Validacion("liquidacion_agente.periodo", "El periodo está al revés."));
        }

        var asignaciones = await _repo.AsignacionesAsync(empresaId, ct).ConfigureAwait(false);
        var reglas = (await _repo.ReglasAsync(empresaId, ct).ConfigureAwait(false)).Where(r => r.AgenteId == agenteId).ToList();
        var liquidado = (await _repo.LiquidacionesAsync(empresaId, agenteId, ct).ConfigureAwait(false)).Where(l => l.Estado == EstadoLiquidacionAgente.Emitida)
            .SelectMany(l => l.Lineas).GroupBy(l => l.FacturaId).ToDictionary(g => g.Key, g => g.Sum(l => l.Importe));

        var todas = await _facturas.ListarAsync(empresaId, ct).ConfigureAwait(false);
        var candidatas = todas.Where(f => f.ClienteId is { } c && AgenteEn(asignaciones, c, f.FechaEmision) == agenteId
                && (agente.Devengo == DevengoComision.Cobrado ? f.FechaEmision <= hasta : f.FechaEmision >= desde && f.FechaEmision <= hasta))
            .ToList();
        // Las ya liquidadas que se rectificaron o anularon después: su comisión se descuenta.
        candidatas.AddRange(todas.Where(f => liquidado.ContainsKey(f.Id) && candidatas.All(c => c.Id != f.Id)));

        var cobrado = agente.Devengo == DevengoComision.Cobrado && candidatas.Count > 0
            ? await _tesoreria.LiquidadoPorDocumentosAsync(TipoDocumentoTesoreria.Factura, candidatas.Select(f => f.Id).ToList(), ct).ConfigureAwait(false)
            : new Dictionary<Guid, decimal>();
        var familias = new Dictionary<Guid, Guid?>();
        var lineas = new List<LineaComisionDto>();
        foreach (var f in candidatas.OrderBy(f => f.FechaEmision).ThenBy(f => f.NumeroCompleto, StringComparer.Ordinal))
        {
            var vigente = f.Estado == nameof(EstadoFactura.Emitida);
            var comision = vigente ? await ComisionAsync(agente, reglas, f, familias, ct).ConfigureAwait(false) : 0m;
            var devengado = !vigente || agente.Devengo == DevengoComision.Facturado || f.Total <= 0m ? 1m : Math.Min(1m, cobrado.GetValueOrDefault(f.Id) / f.Total);
            var ya = liquidado.GetValueOrDefault(f.Id);
            var pendiente = Redondeo.Dos(comision * devengado - ya);
            if (pendiente != 0m)
            {
                lineas.Add(new LineaComisionDto(f.Id, f.NumeroCompleto, f.FechaEmision, f.ClienteNombre, vigente ? f.BaseImponible : 0m, comision,
                    Math.Round(devengado * 100m, 2, MidpointRounding.AwayFromZero), ya, pendiente));
            }
        }

        return Resultado.Ok(new CalculoComisionesDto(agente.Id, agente.Nombre, desde, hasta, agente.Devengo.ToString(), Redondeo.Dos(lineas.Sum(l => l.BaseVenta)),
            Redondeo.Dos(lineas.Sum(l => l.Pendiente)), lineas));
    }

    public async Task<Resultado<LiquidacionAgenteDto>> EmitirAsync(Guid empresaId, DatosLiquidacionAgente d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var calculo = await CalcularAsync(empresaId, d.AgenteId, d.Desde, d.Hasta, ct).ConfigureAwait(false);
        if (calculo.EsFallo)
        {
            return Resultado.Fallo<LiquidacionAgenteDto>(calculo.Error);
        }

        var agente = (await _repo.AgenteAsync(d.AgenteId, ct).ConfigureAwait(false))!;
        var fecha = d.Fecha ?? Hoy;
        var numero = await _repo.SiguienteNumeroLiquidacionAsync(empresaId, fecha.Year, ct).ConfigureAwait(false);
        var l = LiquidacionAgente.Emitir(empresaId, agente.Id, numero, fecha, d.Desde, d.Hasta,
            calculo.Valor.Lineas.Select(x => (x.FacturaId, x.Factura, x.Fecha, x.Cliente, x.BaseVenta, x.PorcentajeDevengado, x.Pendiente)).ToList(), _reloj);
        if (l.EsFallo)
        {
            return Resultado.Fallo<LiquidacionAgenteDto>(l.Error);
        }

        if (d.GenerarFactura)
        {
            if (agente.ProveedorId is null)
            {
                return Resultado.Fallo<LiquidacionAgenteDto>(Error.Validacion("liquidacion_agente.sin_proveedor",
                    "El agente no está enlazado con una ficha de proveedor: no factura sus comisiones (vendedor en nómina)."));
            }

            if (l.Valor.Importe <= 0m)
            {
                return Resultado.Fallo<LiquidacionAgenteDto>(Error.Validacion("liquidacion_agente.negativa", "La liquidación sale a favor de la empresa: no se genera factura del agente."));
            }

            var gasto = await _registrarGasto.EjecutarAsync(empresaId, new RegistrarGastoComando($"Comisiones {l.Valor.NumeroCompleto} ({d.Desde:dd/MM/yyyy}-{d.Hasta:dd/MM/yyyy})",
                l.Valor.Importe, agente.ProveedorId, agente.Nombre, d.CodigoIva, agente.PorcentajeIrpf, fecha,
                NumeroFactura: string.IsNullOrWhiteSpace(d.NumeroFacturaAgente) ? null : d.NumeroFacturaAgente.Trim(), FechaFactura: fecha), ct).ConfigureAwait(false);
            if (gasto.EsFallo)
            {
                return Resultado.Fallo<LiquidacionAgenteDto>(gasto.Error);
            }

            l.Valor.AnotarGasto(gasto.Valor.Id);
        }

        _repo.Agregar(l.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(l.Valor, agente.Nombre));
    }

    public async Task<IReadOnlyList<LiquidacionAgenteDto>> LiquidacionesAsync(Guid empresaId, Guid? agenteId, CancellationToken ct = default)
    {
        var agentes = (await _repo.AgentesAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(a => a.Id, a => a.Nombre);
        return (await _repo.LiquidacionesAsync(empresaId, agenteId, ct).ConfigureAwait(false)).OrderByDescending(l => l.Fecha).ThenByDescending(l => l.Numero)
            .Select(l => Dto(l, agentes.GetValueOrDefault(l.AgenteId))).ToList();
    }

    /// <summary>Anula la liquidación (y la factura del agente, si se generó): sus facturas vuelven a quedar pendientes.</summary>
    public async Task<Resultado> AnularAsync(Guid id, CancellationToken ct = default)
    {
        var l = await _repo.LiquidacionAsync(id, ct).ConfigureAwait(false);
        if (l is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("liquidacion_agente.no_encontrada", "La liquidación no existe."));
        }

        var a = l.Anular();
        if (a.EsFallo)
        {
            return a;
        }

        if (l.GastoId is { } gasto)
        {
            var g = await _anularGasto.EjecutarAsync(gasto, ct).ConfigureAwait(false);
            if (g.EsFallo)
            {
                return Resultado.Fallo(Error.Conflicto("liquidacion_agente.factura", $"No se puede anular la factura del agente: {g.Error.Mensaje}"));
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    /// <summary>Comisión de una factura: por línea si el agente tiene reglas por familia; si no, sobre su base.</summary>
    private async Task<decimal> ComisionAsync(AgenteComercial agente, IReadOnlyList<ReglaComision> reglas, FacturaResumen f, Dictionary<Guid, Guid?> familias, CancellationToken ct)
    {
        var cliente = f.ClienteId!.Value;
        decimal Porcentaje(Guid? familia) =>
            reglas.Select(r => (Regla: r, Encaje: r.Encaje(cliente, familia))).Where(x => x.Encaje >= 0).OrderByDescending(x => x.Encaje).Select(x => (decimal?)x.Regla.Porcentaje)
                .FirstOrDefault() ?? agente.Porcentaje;

        if (!reglas.Any(r => r.FamiliaId is not null))
        {
            return Redondeo.Dos(f.BaseImponible * Porcentaje(null) / 100m);
        }

        var factura = await _facturas.ObtenerAsync(f.Id, ct).ConfigureAwait(false);
        var total = 0m;
        foreach (var l in factura?.Lineas ?? [])
        {
            Guid? familia = null;
            if (l.ProductoId is { } p && !familias.TryGetValue(p, out familia))
            {
                familia = (await _productos.ObtenerAsync(p, ct).ConfigureAwait(false))?.FamiliaId;
                familias[p] = familia;
            }

            total += l.Base * Porcentaje(familia) / 100m;
        }

        return Redondeo.Dos(total);
    }

    /// <summary>Agente que llevaba al cliente en una fecha (la asignación más reciente no posterior).</summary>
    private static Guid? AgenteEn(IReadOnlyList<AsignacionAgente> asignaciones, Guid clienteId, DateOnly fecha) =>
        asignaciones.Where(a => a.ClienteId == clienteId && a.Desde <= fecha).OrderByDescending(a => a.Desde).FirstOrDefault()?.AgenteId;

    private static Dictionary<Guid, Guid?> Vigentes(IReadOnlyList<AsignacionAgente> asignaciones, DateOnly hoy) =>
        asignaciones.GroupBy(a => a.ClienteId).ToDictionary(g => g.Key, g => g.Where(a => a.Desde <= hoy).OrderByDescending(a => a.Desde).FirstOrDefault()?.AgenteId);

    private static LiquidacionAgenteDto Dto(LiquidacionAgente l, string? agente) =>
        new(l.Id, l.NumeroCompleto, l.AgenteId, agente, l.Fecha, l.Desde, l.Hasta, l.Estado.ToString(), l.Importe, l.GastoId,
            l.Lineas.Select(x => new LineaComisionDto(x.FacturaId, x.Factura, x.Fecha, x.Cliente, x.BaseVenta, x.Importe, x.PorcentajeDevengado, 0m, x.Importe)).ToList());

    private static Error AgenteNoEncontrado() => Error.NoEncontrado("agente.no_encontrado", "El agente no existe.");
}
