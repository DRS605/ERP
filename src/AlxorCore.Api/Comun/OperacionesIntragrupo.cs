using AlxorCore.Compras.Aplicacion;
using AlxorCore.Documentos.Aplicacion;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Organizacion.Aplicacion.Modelos;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Recepcion.Aplicacion;
using AlxorCore.Tesoreria.Aplicacion;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Api.Comun;

/// <summary>Factura emitida a otra empresa del grupo y su espejo en la bandeja de esa empresa.</summary>
public sealed record EspejoIntragrupoDto(Guid EmpresaDestinoId, string EmpresaDestino, Guid FacturaRecibidaId, string Estado);

/// <summary>Una factura intragrupo en el cuadre: lo emitido por una empresa frente a lo contabilizado por la otra.</summary>
public sealed record LineaCuadreDto(Guid FacturaId, string Numero, DateOnly Fecha, decimal BaseEmitida, string EstadoFactura, string? EstadoEspejo,
    decimal BaseContabilizada, decimal Diferencia, string Situacion, Guid? GastoId = null);

/// <summary>Cuadre de una pareja emisora → receptora.</summary>
public sealed record ParejaCuadreDto(Guid EmisorId, string Emisor, Guid ReceptorId, string Receptor, decimal Emitido, decimal Contabilizado,
    decimal PendienteEnDestino, decimal Diferencia, IReadOnlyList<LineaCuadreDto> Facturas);

/// <summary>Cuadre recíproco del grupo en un ejercicio (solo entre las empresas a las que tiene acceso el usuario).</summary>
public sealed record CuadreIntragrupoDto(int Ejercicio, bool Cuadra, IReadOnlyList<ParejaCuadreDto> Parejas, IReadOnlyList<string> EmpresasSinAcceso);

/// <summary>Liquidación de una factura intragrupo: el cobro en la emisora y el pago en la receptora, por el mismo importe.</summary>
public sealed record LiquidacionIntragrupoDto(Guid FacturaId, Guid GastoId, decimal Importe, SaldoDto Cobro, SaldoDto Pago);

/// <summary>Cuerpo de la liquidación de una factura intragrupo.</summary>
public sealed record LiquidarIntragrupoPeticion(decimal? Importe = null, DateOnly? Fecha = null, string? Metodo = null);

/// <summary>
/// Operaciones entre empresas del grupo. Un cliente (o proveedor) enlazado a otra empresa del grupo hace que:
/// <list type="bullet">
/// <item>la factura que se le emite llegue a la bandeja de facturas recibidas de esa empresa con el PDF y sus datos
/// (el espejo), para validarla y contabilizarla allí; si se anula, su espejo se rechaza si aún no se contabilizó;</item>
/// <item>el cuadre recíproco compare lo emitido por una empresa con lo contabilizado por la otra;</item>
/// <item>la liquidación registre a la vez el cobro en la emisora y el pago en la receptora.</item>
/// </list>
/// Cada empresa se lee y se escribe en su propio ámbito (con su empresa activa), así la seguridad por empresa de la
/// base de datos se cumple también en estas operaciones.
/// </summary>
public sealed class OperacionesIntragrupo
{
    public const string MetodoLiquidacion = "Transferencia intragrupo";

    private readonly IServiceScopeFactory _ambitos;
    private readonly ILogger<OperacionesIntragrupo> _log;

    public OperacionesIntragrupo(IServiceScopeFactory ambitos, ILogger<OperacionesIntragrupo> log)
    {
        _ambitos = ambitos;
        _log = log;
    }

    /// <summary>Ámbito de servicios con la empresa (y su grupo) activa.</summary>
    internal async Task<AsyncServiceScope> AmbitoAsync(Guid empresaId, CancellationToken ct)
    {
        var ambito = _ambitos.CreateAsyncScope();
        var contexto = ambito.ServiceProvider.GetRequiredService<IContextoEmpresaMutable>();
        contexto.Fijar(empresaId);
        if (await ambito.ServiceProvider.GetRequiredService<IRepositorioEmpresas>().ObtenerGrupoIdAsync(empresaId, ct).ConfigureAwait(false) is { } grupo)
        {
            contexto.FijarGrupo(grupo);
        }

        return ambito;
    }

    internal static T Servicio<T>(AsyncServiceScope ambito) where T : notnull => ambito.ServiceProvider.GetRequiredService<T>();

    /// <summary>
    /// Refleja una factura emitida a un cliente enlazado con otra empresa del grupo en la bandeja de esa empresa.
    /// Devuelve null si el cliente no es del grupo. Es idempotente.
    /// </summary>
    public async Task<Resultado<EspejoIntragrupoDto?>> ReflejarFacturaAsync(Guid empresaOrigenId, Guid facturaId, CancellationToken ct = default)
    {
        FacturaIntragrupo datos;
        EmpresaGrupoDto destino;
        await using (var origen = await AmbitoAsync(empresaOrigenId, ct).ConfigureAwait(false))
        {
            var factura = await Servicio<IConsultaFacturas>(origen).ObtenerAsync(facturaId, ct).ConfigureAwait(false);
            if (factura is null)
            {
                return Resultado.Fallo<EspejoIntragrupoDto?>(Error.NoEncontrado("factura.no_encontrada", "La factura no existe."));
            }

            var vinculada = await EmpresaDelClienteAsync(origen, empresaOrigenId, factura.ClienteId, ct).ConfigureAwait(false);
            if (vinculada is null)
            {
                return Resultado.Ok<EspejoIntragrupoDto?>(null);
            }

            if (factura.Estado != "Emitida")
            {
                return Resultado.Fallo<EspejoIntragrupoDto?>(Error.Conflicto("intragrupo.factura_no_emitida", "Solo se refleja una factura emitida (no anulada)."));
            }

            destino = vinculada.Value.Destino;
            var pdf = await Servicio<GenerarPdfFactura>(origen).EjecutarAsync(empresaOrigenId, facturaId, ct).ConfigureAwait(false);
            if (pdf.EsFallo)
            {
                return Resultado.Fallo<EspejoIntragrupoDto?>(pdf.Error);
            }

            // El proveedor que es la empresa emisora (si está dado de alta en el grupo).
            var grupo = Servicio<IContextoEmpresa>(origen).GrupoId!.Value;
            var proveedor = (await Servicio<IConsultaProveedores>(origen).ListarAsync(grupo, true, null, ct).ConfigureAwait(false))
                .FirstOrDefault(p => p.EmpresaVinculadaId == empresaOrigenId);
            var codigos = factura.Lineas.Select(l => l.CodigoIva).Distinct().ToList();
            var rectificativa = factura.Tipo == "Rectificativa";
            var nota = rectificativa
                ? $"Factura rectificativa {factura.NumeroCompleto} de {vinculada.Value.Origen.RazonSocial}: registra el abono sobre el gasto de la factura rectificada."
                : codigos.Count > 1
                    ? $"Factura {factura.NumeroCompleto} de {vinculada.Value.Origen.RazonSocial} con varios tipos de impuesto: revisa el desglose al validarla."
                    : $"Factura {factura.NumeroCompleto} de {vinculada.Value.Origen.RazonSocial} (empresa del grupo).";
            datos = new FacturaIntragrupo(empresaOrigenId, vinculada.Value.Origen.RazonSocial, facturaId, factura.NumeroCompleto, factura.FechaEmision,
                rectificativa ? null : factura.BaseImponible, codigos.Count == 1 ? codigos[0] : null, factura.PorcentajeIrpf, proveedor?.Id,
                pdf.Valor.NombreArchivo, pdf.Valor.Contenido, nota);
        }

        await using var receptora = await AmbitoAsync(destino.Id, ct).ConfigureAwait(false);
        var espejo = await Servicio<RecibirFacturaIntragrupo>(receptora).EjecutarAsync(destino.Id, datos, ct).ConfigureAwait(false);
        return espejo.EsFallo
            ? Resultado.Fallo<EspejoIntragrupoDto?>(espejo.Error)
            : Resultado.Ok<EspejoIntragrupoDto?>(new EspejoIntragrupoDto(destino.Id, destino.RazonSocial, espejo.Valor.Id, espejo.Valor.Estado));
    }

    /// <summary>La factura de origen se anuló: su espejo se rechaza si aún no se contabilizó.</summary>
    public async Task<Resultado<EspejoIntragrupoDto?>> AnularReflejoAsync(Guid empresaOrigenId, Guid facturaId, string motivo, CancellationToken ct = default)
    {
        EmpresaGrupoDto destino;
        string numero;
        await using (var origen = await AmbitoAsync(empresaOrigenId, ct).ConfigureAwait(false))
        {
            var factura = await Servicio<IConsultaFacturas>(origen).ObtenerAsync(facturaId, ct).ConfigureAwait(false);
            var vinculada = factura is null ? null : await EmpresaDelClienteAsync(origen, empresaOrigenId, factura.ClienteId, ct).ConfigureAwait(false);
            if (vinculada is null)
            {
                return Resultado.Ok<EspejoIntragrupoDto?>(null);
            }

            destino = vinculada.Value.Destino;
            numero = factura!.NumeroCompleto;
        }

        await using var receptora = await AmbitoAsync(destino.Id, ct).ConfigureAwait(false);
        var r = await Servicio<RecibirFacturaIntragrupo>(receptora)
            .AnularOrigenAsync(destino.Id, facturaId, $"El emisor anuló la factura {numero}: {motivo}", ct).ConfigureAwait(false);
        return r.EsFallo
            ? Resultado.Fallo<EspejoIntragrupoDto?>(r.Error)
            : Resultado.Ok(r.Valor is { } f ? new EspejoIntragrupoDto(destino.Id, destino.RazonSocial, f.Id, f.Estado) : null);
    }

    /// <summary>Tras emitir o anular una factura, sin romper la operación ya confirmada si el espejo falla.</summary>
    public async Task ProcesarEventoAsync(Guid empresaId, object evento, CancellationToken ct)
    {
        try
        {
            var r = evento switch
            {
                AlxorCore.Facturacion.Dominio.FacturaEmitida e => await ReflejarFacturaAsync(empresaId, e.FacturaId, ct).ConfigureAwait(false),
                AlxorCore.Facturacion.Dominio.FacturaAnulada e => await AnularReflejoAsync(empresaId, e.FacturaId, e.Motivo, ct).ConfigureAwait(false),
                AlxorCore.Facturacion.Dominio.AlbaranVentaEmitido e => Sin(await TraspasarAlbaranAsync(empresaId, e.AlbaranId, ct).ConfigureAwait(false)),
                AlxorCore.Facturacion.Dominio.AlbaranVentaAnulado e => Sin(await AnularTraspasoAsync(empresaId, e.AlbaranId, e.Motivo ?? "Albarán de venta anulado.", ct).ConfigureAwait(false)),
                _ => Resultado.Ok<EspejoIntragrupoDto?>(null),
            };
            if (r.EsFallo)
            {
                _log.LogWarning("No se pudo reflejar la factura en la empresa del grupo: {Error}", r.Error.Mensaje);
            }
        }
#pragma warning disable CA1031 // La factura ya está confirmada: el espejo se puede reintentar con /intragrupo/facturas/{id}/reflejar.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            _log.LogWarning(ex, "No se pudo reflejar la factura en la empresa del grupo.");
        }
    }

    private static Resultado<EspejoIntragrupoDto?> Sin<T>(Resultado<T> r) => r.EsFallo ? Resultado.Fallo<EspejoIntragrupoDto?>(r.Error) : Resultado.Ok<EspejoIntragrupoDto?>(null);

    /// <summary>
    /// Traspaso de existencias: el albarán de venta a un cliente enlazado con otra empresa del grupo es, en esa empresa,
    /// un albarán de recepción del pedido de compra espejo de su pedido de venta (que nace con la primera entrega), con la
    /// entrada en el almacén que la receptora tenga configurado para los traspasos (por defecto, el activo de código más
    /// bajo). Devuelve el pedido de compra, o null si el cliente no es del grupo. Es idempotente.
    /// </summary>
    public async Task<Resultado<Guid?>> TraspasarAlbaranAsync(Guid empresaOrigenId, Guid albaranId, CancellationToken ct = default)
    {
        DatosTraspasoParcial datos;
        EmpresaGrupoDto destino;
        await using (var origen = await AmbitoAsync(empresaOrigenId, ct).ConfigureAwait(false))
        {
            var albaran = await Servicio<IRepositorioAlbaranesVenta>(origen).ObtenerPorIdAsync(albaranId, ct).ConfigureAwait(false);
            if (albaran is null)
            {
                return Resultado.Fallo<Guid?>(Error.NoEncontrado("albaranventa.no_encontrado", "El albarán no existe."));
            }

            var vinculada = await EmpresaDelClienteAsync(origen, empresaOrigenId, albaran.ClienteId, ct).ConfigureAwait(false);
            if (vinculada is null || albaran.AnuladoEn is not null)
            {
                return Resultado.Ok<Guid?>(null);
            }

            destino = vinculada.Value.Destino;
            var pedido = await Servicio<ObtenerPedidoVenta>(origen).EjecutarAsync(albaran.PedidoId, ct).ConfigureAwait(false);
            if (pedido is null)
            {
                return Resultado.Fallo<Guid?>(Error.NoEncontrado("pedidoventa.no_encontrado", "El pedido de venta del albarán no existe."));
            }

            datos = new DatosTraspasoParcial(vinculada.Value.Origen.RazonSocial, pedido.Id, albaran.NumeroCompleto, albaran.Fecha,
                pedido.Lineas.Select(l => new LineaTraspaso(l.Id, l.ProductoId, l.Descripcion, l.Cantidad,
                    Redondeo.Dos(l.PrecioUnitario * (1m - (l.PorcentajeDescuento / 100m))))).ToList(),
                albaran.Lineas.Select(l => new EntregaTraspaso(l.LineaPedidoId, l.Cantidad)).ToList());
        }

        await using var receptora = await AmbitoAsync(destino.Id, ct).ConfigureAwait(false);
        var grupo = Servicio<IContextoEmpresa>(receptora).GrupoId!.Value;
        var proveedor = (await Servicio<IConsultaProveedores>(receptora).ListarAsync(grupo, true, null, ct).ConfigureAwait(false))
            .FirstOrDefault(p => p.EmpresaVinculadaId == empresaOrigenId);
        var activos = (await Servicio<AlxorCore.Inventario.Aplicacion.GestionAlmacenes>(receptora).ListarAlmacenesAsync(destino.Id, ct).ConfigureAwait(false))
            .Where(a => a.Activo).OrderBy(a => a.Codigo, StringComparer.Ordinal).ToList();
        var (configurado, elegido) = await Servicio<AlmacenesTraspaso>(receptora).ResolverAsync(destino.Id, empresaOrigenId, ct).ConfigureAwait(false);
        // El almacén elegido, si sigue activo; sin configuración (o dado de baja), el primero activo.
        var almacen = configurado && (elegido is null || activos.Any(a => a.Id == elegido)) ? elegido : activos.FirstOrDefault()?.Id;
        var r = await Servicio<TraspasoIntragrupoCompras>(receptora).RecibirAsync(destino.Id, new DatosTraspaso(empresaOrigenId, datos.Origen, datos.PedidoVentaId,
            albaranId, datos.Numero, proveedor?.Id, datos.Fecha, almacen, datos.LineasPedido, datos.Entregas), ct).ConfigureAwait(false);
        return r.EsFallo ? Resultado.Fallo<Guid?>(r.Error) : Resultado.Ok<Guid?>(r.Valor.Id);
    }

    private sealed record DatosTraspasoParcial(string Origen, Guid PedidoVentaId, string Numero, DateOnly Fecha, IReadOnlyList<LineaTraspaso> LineasPedido,
        IReadOnlyList<EntregaTraspaso> Entregas);

    /// <summary>El albarán de venta de origen se anuló: en la receptora se anula su recepción (y sale del almacén); sin recepciones vivas, el pedido se cancela.</summary>
    public async Task<Resultado<Guid?>> AnularTraspasoAsync(Guid empresaOrigenId, Guid albaranId, string motivo, CancellationToken ct = default)
    {
        EmpresaGrupoDto destino;
        await using (var origen = await AmbitoAsync(empresaOrigenId, ct).ConfigureAwait(false))
        {
            var albaran = await Servicio<IRepositorioAlbaranesVenta>(origen).ObtenerPorIdAsync(albaranId, ct).ConfigureAwait(false);
            var vinculada = albaran is null ? null : await EmpresaDelClienteAsync(origen, empresaOrigenId, albaran.ClienteId, ct).ConfigureAwait(false);
            if (vinculada is null)
            {
                return Resultado.Ok<Guid?>(null);
            }

            if (albaran!.AnuladoEn is null)
            {
                return Resultado.Fallo<Guid?>(Error.Conflicto("albaranventa.vivo", "El albarán de venta no está anulado: su traspaso no se deshace."));
            }

            destino = vinculada.Value.Destino;
        }

        await using var receptora = await AmbitoAsync(destino.Id, ct).ConfigureAwait(false);
        var r = await Servicio<TraspasoIntragrupoCompras>(receptora).AnularAsync(destino.Id, albaranId, motivo, ct).ConfigureAwait(false);
        return r.EsFallo ? Resultado.Fallo<Guid?>(r.Error) : Resultado.Ok(r.Valor?.Id);
    }

    /// <summary>Cuadre recíproco del ejercicio entre las empresas del grupo a las que tiene acceso el usuario.</summary>
    public async Task<CuadreIntragrupoDto> CuadreAsync(Guid empresaActual, Guid usuarioId, int ejercicio, CancellationToken ct = default)
    {
        IReadOnlyList<EmpresaGrupoDto> empresas;
        HashSet<Guid> accesibles;
        Dictionary<Guid, Guid?> clientes;
        await using (var actual = await AmbitoAsync(empresaActual, ct).ConfigureAwait(false))
        {
            var grupo = Servicio<IContextoEmpresa>(actual).GrupoId!.Value;
            empresas = await Servicio<IConsultaEmpresas>(actual).EmpresasDelGrupoAsync(grupo, ct).ConfigureAwait(false);
            accesibles = (await Servicio<IConsultasOrganizacion>(actual).ListarEmpresasDeUsuarioAsync(usuarioId, ct).ConfigureAwait(false))
                .Select(e => e.Id).ToHashSet();
            clientes = (await Servicio<IConsultaClientes>(actual).ListarAsync(grupo, true, null, ct).ConfigureAwait(false))
                .ToDictionary(c => c.Id, c => c.EmpresaVinculadaId);
        }

        var visibles = empresas.Where(e => accesibles.Contains(e.Id)).ToList();
        var emitidas = new Dictionary<Guid, List<FacturaResumen>>();
        var espejos = new Dictionary<Guid, List<FacturaRecibidaDto>>();
        foreach (var e in visibles)
        {
            await using var ambito = await AmbitoAsync(e.Id, ct).ConfigureAwait(false);
            emitidas[e.Id] = (await Servicio<IConsultaFacturas>(ambito).ListarAsync(e.Id, ct).ConfigureAwait(false))
                .Where(f => f.FechaEmision.Year == ejercicio && f.ClienteId is { } c && clientes.GetValueOrDefault(c) is { } v && v != e.Id).ToList();
            espejos[e.Id] = (await Servicio<IConsultaFacturasRecibidas>(ambito).IntragrupoAsync(e.Id, ct).ConfigureAwait(false)).ToList();
        }

        var parejas = new List<ParejaCuadreDto>();
        foreach (var emisor in visibles)
        {
            foreach (var receptor in visibles.Where(r => r.Id != emisor.Id))
            {
                var suyas = emitidas[emisor.Id].Where(f => clientes[f.ClienteId!.Value] == receptor.Id).ToList();
                var reflejos = espejos[receptor.Id].Where(x => x.EmpresaOrigenId == emisor.Id).ToDictionary(x => x.FacturaOrigenId!.Value);
                if (suyas.Count == 0 && reflejos.Count == 0)
                {
                    continue;
                }

                var lineas = suyas.OrderBy(f => f.FechaEmision).ThenBy(f => f.NumeroCompleto, StringComparer.Ordinal).Select(f =>
                {
                    reflejos.TryGetValue(f.Id, out var espejo);
                    var viva = f.Estado != "Anulada";
                    var emitido = viva ? f.BaseImponible : 0m;
                    var contabilizado = espejo is { Estado: "Contabilizada" } ? espejo.BaseImponible ?? 0m : 0m;
                    var situacion = (viva, espejo?.Estado) switch
                    {
                        (_, null) => "Sin reflejar",
                        (true, "Contabilizada") when contabilizado == emitido => "Cuadrada",
                        (true, "Contabilizada") => "Importes distintos",
                        (true, "Rechazada") => "Rechazada en destino",
                        (true, _) => "Pendiente en destino",
                        (false, "Contabilizada") => "Anulada en origen: anula el gasto en destino",
                        (false, _) => "Anulada",
                    };
                    return new LineaCuadreDto(f.Id, f.NumeroCompleto, f.FechaEmision, emitido, f.Estado, espejo?.Estado, contabilizado,
                        Redondeo.Dos(emitido - contabilizado), situacion, espejo?.GastoId);
                }).ToList();
                var pendiente = lineas.Where(l => l.Situacion == "Pendiente en destino").Sum(l => l.BaseEmitida);
                parejas.Add(new ParejaCuadreDto(emisor.Id, emisor.RazonSocial, receptor.Id, receptor.RazonSocial, lineas.Sum(l => l.BaseEmitida),
                    lineas.Sum(l => l.BaseContabilizada), pendiente, Redondeo.Dos(lineas.Sum(l => l.Diferencia)), lineas));
            }
        }

        return new CuadreIntragrupoDto(ejercicio, parejas.All(p => p.Diferencia == 0m), parejas,
            empresas.Where(e => !accesibles.Contains(e.Id)).Select(e => e.RazonSocial).ToList());
    }

    /// <summary>
    /// Liquida una factura intragrupo: registra el cobro en la empresa emisora y el pago del gasto contabilizado en la
    /// receptora, por el mismo importe (por defecto, lo pendiente). Si el pago no se puede registrar, el cobro se anula.
    /// </summary>
    public async Task<Resultado<LiquidacionIntragrupoDto>> LiquidarAsync(Guid empresaOrigenId, Guid usuarioId, Guid facturaId, LiquidarIntragrupoPeticion peticion,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(peticion);
        EmpresaGrupoDto destino;
        SaldoDto saldoFactura;
        await using (var origen = await AmbitoAsync(empresaOrigenId, ct).ConfigureAwait(false))
        {
            var factura = await Servicio<IConsultaFacturas>(origen).ObtenerAsync(facturaId, ct).ConfigureAwait(false);
            var vinculada = factura is null ? null : await EmpresaDelClienteAsync(origen, empresaOrigenId, factura.ClienteId, ct).ConfigureAwait(false);
            if (vinculada is null)
            {
                return Resultado.Fallo<LiquidacionIntragrupoDto>(Error.Validacion("intragrupo.no_intragrupo", "La factura no es de un cliente enlazado con otra empresa del grupo."));
            }

            destino = vinculada.Value.Destino;
            var usuario = await Servicio<IConsultasOrganizacion>(origen).ListarEmpresasDeUsuarioAsync(usuarioId, ct).ConfigureAwait(false);
            if (usuario.All(e => e.Id != destino.Id))
            {
                return Resultado.Fallo<LiquidacionIntragrupoDto>(Error.Prohibido("intragrupo.sin_acceso", $"No tienes acceso a {destino.RazonSocial} para registrar allí el pago."));
            }

            var saldo = await Servicio<ConsultarSaldo>(origen).DeFacturaAsync(facturaId, ct).ConfigureAwait(false);
            if (saldo.EsFallo)
            {
                return Resultado.Fallo<LiquidacionIntragrupoDto>(saldo.Error);
            }

            saldoFactura = saldo.Valor;
        }

        Guid gastoId;
        SaldoDto saldoGasto;
        await using (var receptora = await AmbitoAsync(destino.Id, ct).ConfigureAwait(false))
        {
            var espejo = (await Servicio<IConsultaFacturasRecibidas>(receptora).IntragrupoAsync(destino.Id, ct).ConfigureAwait(false))
                .FirstOrDefault(f => f.FacturaOrigenId == facturaId);
            if (espejo?.GastoId is not { } g)
            {
                return Resultado.Fallo<LiquidacionIntragrupoDto>(Error.Conflicto("intragrupo.sin_contabilizar",
                    $"{destino.RazonSocial} aún no ha contabilizado esta factura: valídala y contabilízala en su bandeja de facturas recibidas."));
            }

            gastoId = g;
            var saldo = await Servicio<ConsultarSaldo>(receptora).DeGastoAsync(gastoId, ct).ConfigureAwait(false);
            if (saldo.EsFallo)
            {
                return Resultado.Fallo<LiquidacionIntragrupoDto>(saldo.Error);
            }

            saldoGasto = saldo.Valor;
        }

        var importe = peticion.Importe ?? Math.Min(saldoFactura.Pendiente, saldoGasto.Pendiente);
        if (importe <= 0m || importe > saldoFactura.Pendiente || importe > saldoGasto.Pendiente)
        {
            return Resultado.Fallo<LiquidacionIntragrupoDto>(Error.Validacion("intragrupo.importe",
                $"El importe debe ser positivo y no superar lo pendiente: {Redondeo.Formatear(saldoFactura.Pendiente, 2)} € por cobrar y {Redondeo.Formatear(saldoGasto.Pendiente, 2)} € por pagar."));
        }

        var metodo = string.IsNullOrWhiteSpace(peticion.Metodo) ? MetodoLiquidacion : peticion.Metodo;
        SaldoDto cobro;
        await using (var origen = await AmbitoAsync(empresaOrigenId, ct).ConfigureAwait(false))
        {
            var r = await Servicio<RegistrarCobro>(origen).EjecutarAsync(empresaOrigenId, new RegistrarCobroComando(facturaId, importe, peticion.Fecha, metodo), ct).ConfigureAwait(false);
            if (r.EsFallo)
            {
                return Resultado.Fallo<LiquidacionIntragrupoDto>(r.Error);
            }

            cobro = r.Valor;
        }

        Resultado<SaldoDto> pago;
        try
        {
            await using var receptora = await AmbitoAsync(destino.Id, ct).ConfigureAwait(false);
            pago = await Servicio<RegistrarPago>(receptora).EjecutarAsync(destino.Id, new RegistrarPagoComando(gastoId, importe, peticion.Fecha, metodo), ct).ConfigureAwait(false);
        }
        catch
        {
            await DeshacerCobroAsync(empresaOrigenId, saldoFactura, cobro).ConfigureAwait(false);
            throw;
        }

        if (pago.EsFallo)
        {
            await DeshacerCobroAsync(empresaOrigenId, saldoFactura, cobro).ConfigureAwait(false);
            return Resultado.Fallo<LiquidacionIntragrupoDto>(pago.Error);
        }

        return Resultado.Ok(new LiquidacionIntragrupoDto(facturaId, gastoId, importe, cobro, pago.Valor));
    }

    /// <summary>Anula el cobro recién registrado (el que no estaba antes) si el pago en la otra empresa falla.</summary>
    private async Task DeshacerCobroAsync(Guid empresaId, SaldoDto antes, SaldoDto despues)
    {
        var previos = antes.Movimientos.Select(m => m.Id).ToHashSet();
        foreach (var nuevo in despues.Movimientos.Where(m => !previos.Contains(m.Id)))
        {
            await using var ambito = await AmbitoAsync(empresaId, CancellationToken.None).ConfigureAwait(false);
            await Servicio<AnularMovimiento>(ambito).EjecutarAsync(empresaId, nuevo.Id, null, CancellationToken.None).ConfigureAwait(false);
        }
    }

    /// <summary>Empresa del grupo enlazada al cliente (si no es la propia), con la empresa de origen.</summary>
    private static async Task<(EmpresaGrupoDto Origen, EmpresaGrupoDto Destino)?> EmpresaDelClienteAsync(AsyncServiceScope ambito, Guid empresaOrigenId, Guid? clienteId,
        CancellationToken ct)
    {
        if (clienteId is null)
        {
            return null;
        }

        var cliente = await Servicio<IConsultaClientes>(ambito).ObtenerAsync(clienteId.Value, ct).ConfigureAwait(false);
        if (cliente?.EmpresaVinculadaId is not { } destinoId || destinoId == empresaOrigenId)
        {
            return null;
        }

        var grupo = Servicio<IContextoEmpresa>(ambito).GrupoId;
        var empresas = grupo is { } g ? await Servicio<IConsultaEmpresas>(ambito).EmpresasDelGrupoAsync(g, ct).ConfigureAwait(false) : [];
        var origen = empresas.FirstOrDefault(e => e.Id == empresaOrigenId);
        var destino = empresas.FirstOrDefault(e => e.Id == destinoId);
        return origen is null || destino is null ? null : (origen, destino);
    }
}
