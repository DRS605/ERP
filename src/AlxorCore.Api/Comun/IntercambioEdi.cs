using System.Text;
using AlxorCore.Agro.Aplicacion;
using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Integraciones.Aplicacion;
using AlxorCore.Integraciones.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Api.Comun;

public sealed record ConfiguracionEdiDto(string? GlnEmpresa);

public sealed record SocioEdiDto(Guid Id, Guid ClienteId, string? Cliente, string GlnComprador, string? GlnFacturacion, string? GlnEntrega);

public sealed record GuardarSocioEdi(Guid ClienteId, string GlnComprador, string? GlnFacturacion, string? GlnEntrega);

public sealed record PedidoEdiDto(Guid Id, string NumeroCliente, string GlnComprador, Guid PedidoVentaId, DateTimeOffset RecibidoEn);

public sealed record MensajeEdi(string NombreArchivo, string Contenido);

public sealed record PedidoImportadoEdi(Guid PedidoVentaId, string NumeroPedido, string NumeroCliente, string Cliente, int Lineas);

public sealed record DiferenciaRecadv(string? Gtin, string Descripcion, decimal Expedido, decimal Recibido, decimal? Aceptado, decimal Diferencia);

public sealed record ResultadoRecadv(string Numero, string? Albaran, Guid? AlbaranId, bool Conforme, IReadOnlyList<DiferenciaRecadv> Lineas);

/// <summary>
/// Intercambio EDI con la gran distribución (EANCOM D96A), como el módulo EDI de Hispatec: el ORDERS del cliente entra
/// como pedido de venta; el albarán sale como DESADV con los SSCC de sus palés y el número de pedido del cliente; la
/// factura sale como INVOIC; el RECADV se contrasta con lo expedido. Los artículos se identifican por su referencia
/// (el GTIN/EAN) y los clientes por su GLN (socios EDI).
/// </summary>
public sealed class IntercambioEdi
{
    private readonly IRepositorioEdi _edi;
    private readonly IUnidadDeTrabajoIntegraciones _unidad;
    private readonly IConsultaClientes _clientes;
    private readonly IConsultaProductos _productos;
    private readonly ObtenerFactura _facturas;
    private readonly ConsultarAlbaranesVenta _albaranes;
    private readonly CrearPedidoVenta _crearPedido;
    private readonly IServiceProvider _servicios;
    private readonly IReloj _reloj;

    public IntercambioEdi(IRepositorioEdi edi, IUnidadDeTrabajoIntegraciones unidad, IConsultaClientes clientes, IConsultaProductos productos,
        ObtenerFactura facturas, ConsultarAlbaranesVenta albaranes, CrearPedidoVenta crearPedido, IServiceProvider servicios, IReloj reloj)
    {
        _edi = edi;
        _unidad = unidad;
        _clientes = clientes;
        _productos = productos;
        _facturas = facturas;
        _albaranes = albaranes;
        _crearPedido = crearPedido;
        _servicios = servicios;
        _reloj = reloj;
    }

    // ------------------------------------------------------------------ configuración y socios

    public async Task<ConfiguracionEdiDto> ConfiguracionAsync(CancellationToken ct = default) =>
        new((await _edi.ConfiguracionAsync(ct).ConfigureAwait(false))?.GlnEmpresa);

    public async Task<Resultado<ConfiguracionEdiDto>> GuardarConfiguracionAsync(Guid empresaId, string? gln, CancellationToken ct = default)
    {
        var actual = await _edi.ConfiguracionAsync(ct).ConfigureAwait(false);
        if (actual is null)
        {
            var nueva = ConfiguracionEdi.Crear(empresaId, gln);
            if (nueva.EsFallo)
            {
                return Resultado.Fallo<ConfiguracionEdiDto>(nueva.Error);
            }

            _edi.Agregar(nueva.Valor);
        }
        else
        {
            var r = actual.Cambiar(gln);
            if (r.EsFallo)
            {
                return Resultado.Fallo<ConfiguracionEdiDto>(r.Error);
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new ConfiguracionEdiDto(gln!.Trim()));
    }

    public async Task<IReadOnlyList<SocioEdiDto>> SociosAsync(CancellationToken ct = default)
    {
        var lista = new List<SocioEdiDto>();
        foreach (var s in await _edi.SociosAsync(ct).ConfigureAwait(false))
        {
            var c = await _clientes.ObtenerAsync(s.ClienteId, ct).ConfigureAwait(false);
            lista.Add(Dto(s, c?.Nombre));
        }

        return lista;
    }

    public async Task<Resultado<SocioEdiDto>> GuardarSocioAsync(Guid empresaId, Guid? id, GuardarSocioEdi datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var cliente = await _clientes.ObtenerAsync(datos.ClienteId, ct).ConfigureAwait(false);
        if (cliente is null)
        {
            return Resultado.Fallo<SocioEdiDto>(Error.NoEncontrado("cliente.no_encontrado", "El cliente no existe."));
        }

        var delCliente = await _edi.SocioPorClienteAsync(datos.ClienteId, ct).ConfigureAwait(false);
        var delGln = await _edi.SocioPorGlnAsync(datos.GlnComprador?.Trim() ?? "", ct).ConfigureAwait(false);
        if ((delCliente is not null && delCliente.Id != id) || (delGln is not null && delGln.Id != id))
        {
            return Resultado.Fallo<SocioEdiDto>(Error.Conflicto("edi.socio_duplicado", "Ese cliente o ese GLN ya tienen un socio EDI."));
        }

        SocioEdi socio;
        if (id is { } existente)
        {
            socio = await _edi.SocioAsync(existente, ct).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Socio EDI inexistente.");
            if (socio.ClienteId != datos.ClienteId)
            {
                return Resultado.Fallo<SocioEdiDto>(Error.Validacion("edi.socio_cliente", "El cliente de un socio EDI no se cambia: da de alta otro."));
            }

            var r = socio.Cambiar(datos.GlnComprador, datos.GlnFacturacion, datos.GlnEntrega);
            if (r.EsFallo)
            {
                return Resultado.Fallo<SocioEdiDto>(r.Error);
            }
        }
        else
        {
            var r = SocioEdi.Crear(empresaId, datos.ClienteId, datos.GlnComprador, datos.GlnFacturacion, datos.GlnEntrega);
            if (r.EsFallo)
            {
                return Resultado.Fallo<SocioEdiDto>(r.Error);
            }

            socio = r.Valor;
            _edi.Agregar(socio);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(socio, cliente.Nombre));
    }

    public async Task<bool> SocioExisteAsync(Guid id, CancellationToken ct = default) => await _edi.SocioAsync(id, ct).ConfigureAwait(false) is not null;

    public async Task<Resultado> EliminarSocioAsync(Guid id, CancellationToken ct = default)
    {
        var socio = await _edi.SocioAsync(id, ct).ConfigureAwait(false);
        if (socio is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("edi.socio_no_encontrado", "El socio EDI no existe."));
        }

        _edi.Quitar(socio);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    public async Task<IReadOnlyList<PedidoEdiDto>> PedidosAsync(CancellationToken ct = default) =>
        (await _edi.PedidosAsync(ct).ConfigureAwait(false)).Select(p => new PedidoEdiDto(p.Id, p.NumeroCliente, p.GlnComprador, p.PedidoVentaId, p.RecibidoEn)).ToList();

    // ------------------------------------------------------------------ mensajes salientes

    public async Task<Resultado<MensajeEdi>> InvoicAsync(Guid facturaId, CancellationToken ct = default)
    {
        var rf = await _facturas.EjecutarAsync(facturaId, ct).ConfigureAwait(false);
        if (rf.EsFallo)
        {
            return Resultado.Fallo<MensajeEdi>(rf.Error);
        }

        var f = rf.Valor;
        if (f.Estado is "Borrador")
        {
            return Resultado.Fallo<MensajeEdi>(Error.Validacion("edi.factura_borrador", "Emite la factura antes de enviarla por EDI."));
        }

        var (emisor, socio, fallo) = await CodigosAsync(f.ClienteId, ct).ConfigureAwait(false);
        if (fallo is not null)
        {
            return Resultado.Fallo<MensajeEdi>(fallo);
        }

        // El albarán y el pedido del cliente: los de la primera línea que viene de un albarán.
        string? albaran = null, pedidoCliente = null;
        if (f.Lineas.Select(l => l.AlbaranVentaId).FirstOrDefault(a => a is not null) is { } albaranId)
        {
            var a = await _albaranes.ObtenerAsync(albaranId, ct).ConfigureAwait(false);
            albaran = a?.NumeroCompleto;
            if (a?.PedidoId is { } pedidoId)
            {
                pedidoCliente = (await _edi.PedidoPorVentaAsync(pedidoId, ct).ConfigureAwait(false))?.NumeroCliente;
            }
        }

        var lineas = new List<LineaEdi>();
        foreach (var l in f.Lineas)
        {
            lineas.Add(new LineaEdi(await GtinAsync(l.ProductoId, ct).ConfigureAwait(false), l.Descripcion, l.Cantidad,
                l.Cantidad == 0 ? l.PrecioUnitario : Math.Round(l.Base / l.Cantidad, 4), l.Base, l.PorcentajeIva));
        }

        var impuestos = f.Lineas.GroupBy(l => l.PorcentajeIva).OrderBy(g => g.Key)
            .Select(g => (g.Key, g.Sum(l => l.Base), g.Sum(l => l.CuotaIva))).ToList();
        var datos = new DatosInvoic(f.NumeroCompleto, f.FechaEmision, emisor!, socio!.GlnComprador, socio.GlnFacturacion, pedidoCliente, albaran,
            lineas, impuestos, f.BaseImponible, f.Total, f.RectificaFacturaId is not null);
        return Resultado.Ok(new MensajeEdi($"INVOIC-{Archivo(f.NumeroCompleto)}.edi", Edifact.Invoic(datos, Referencia(f.Id), _reloj.AhoraUtc)));
    }

    public async Task<Resultado<MensajeEdi>> DesadvAsync(Guid empresaId, Guid albaranId, CancellationToken ct = default)
    {
        var a = await _albaranes.ObtenerAsync(albaranId, ct).ConfigureAwait(false);
        if (a is null)
        {
            return Resultado.Fallo<MensajeEdi>(Error.NoEncontrado("albaranventa.no_encontrado", "El albarán no existe."));
        }

        if (a.Anulado)
        {
            return Resultado.Fallo<MensajeEdi>(Error.Validacion("edi.albaran_anulado", "El albarán está anulado."));
        }

        var (emisor, socio, fallo) = await CodigosAsync(a.ClienteId, ct).ConfigureAwait(false);
        if (fallo is not null)
        {
            return Resultado.Fallo<MensajeEdi>(fallo);
        }

        var pedidoCliente = a.PedidoId is { } pid ? (await _edi.PedidoPorVentaAsync(pid, ct).ConfigureAwait(false))?.NumeroCliente : null;

        // Los palés agro expedidos con el albarán, con su SSCC y su contenido.
        var pales = new List<PaleEdi>();
        if (_servicios.GetService<IRepositorioAgro>() is { } agro && _servicios.GetService<PalesAgro>() is { } servicioPales)
        {
            foreach (var p in await agro.PalesDeAlbaranAsync(albaranId, ct).ConfigureAwait(false))
            {
                var dto = await servicioPales.ObtenerAsync(empresaId, p.Id.ToString(), ct).ConfigureAwait(false);
                if (dto is null)
                {
                    continue;
                }

                var contenido = new List<LineaEdi>();
                foreach (var g in dto.Contenido.GroupBy(c => c.ProductoId))
                {
                    contenido.Add(new LineaEdi(await GtinAsync(g.Key, ct).ConfigureAwait(false), "", g.Sum(c => c.Kilos)));
                }

                pales.Add(new PaleEdi(dto.Sscc, contenido));
            }
        }

        var lineas = new List<LineaEdi>();
        foreach (var l in a.Lineas.OrderBy(l => l.Orden))
        {
            lineas.Add(new LineaEdi(await GtinAsync(l.ProductoId, ct).ConfigureAwait(false), l.Descripcion, l.Cantidad));
        }

        var datos = new DatosDesadv(a.NumeroCompleto, a.Fecha, emisor!, socio!.GlnComprador, socio.GlnEntrega, pedidoCliente, pales, lineas);
        return Resultado.Ok(new MensajeEdi($"DESADV-{Archivo(a.NumeroCompleto)}.edi", Edifact.Desadv(datos, Referencia(a.Id), _reloj.AhoraUtc)));
    }

    // ------------------------------------------------------------------ mensajes entrantes

    /// <summary>Importa un ORDERS: el cliente por el GLN del comprador; cada línea, por el GTIN en la referencia del artículo.</summary>
    public async Task<Resultado<PedidoImportadoEdi>> ImportarOrdersAsync(Guid empresaId, Guid grupoId, string texto, CancellationToken ct = default)
    {
        PedidoLeidoEdi leido;
        try
        {
            leido = Edifact.LeerOrders(texto);
        }
        catch (FormatException ex)
        {
            return Resultado.Fallo<PedidoImportadoEdi>(Error.Validacion("edi.formato", ex.Message));
        }

        var gln = leido.GlnComprador ?? leido.GlnEntrega;
        var socio = gln is null ? null : await _edi.SocioPorGlnAsync(gln, ct).ConfigureAwait(false);
        if (socio is null)
        {
            return Resultado.Fallo<PedidoImportadoEdi>(Error.Validacion("edi.socio_desconocido",
                $"Ningún cliente tiene el GLN {gln ?? "(vacío)"}: dalo de alta como socio EDI."));
        }

        if (await _edi.PedidoAsync(leido.Numero, socio.GlnComprador, ct).ConfigureAwait(false) is { } previo)
        {
            return Resultado.Fallo<PedidoImportadoEdi>(Error.Conflicto("edi.pedido_duplicado",
                $"El pedido {leido.Numero} de este cliente ya se importó."));
        }

        if (leido.Lineas.Count == 0)
        {
            return Resultado.Fallo<PedidoImportadoEdi>(Error.Validacion("edi.sin_lineas", "El ORDERS no trae líneas."));
        }

        var productos = (await _productos.ListarAsync(grupoId, false, null, ct).ConfigureAwait(false))
            .Where(p => !string.IsNullOrWhiteSpace(p.Referencia)).GroupBy(p => p.Referencia!.Trim()).ToDictionary(g => g.Key, g => g.First());
        var lineas = new List<LineaPedidoVentaComando>();
        var desconocidos = new List<string>();
        foreach (var l in leido.Lineas)
        {
            var clave = l.Gtin ?? l.CodigoInterno;
            if (clave is null || !productos.TryGetValue(clave, out var p))
            {
                desconocidos.Add(clave ?? "(sin código)");
                continue;
            }

            lineas.Add(new LineaPedidoVentaComando(string.IsNullOrWhiteSpace(l.Descripcion) ? p.Nombre : l.Descripcion, l.Cantidad, l.Precio ?? p.PrecioUnitario,
                p.CodigoIva, 0m, p.Id));
        }

        if (desconocidos.Count > 0)
        {
            return Resultado.Fallo<PedidoImportadoEdi>(Error.Validacion("edi.articulo_desconocido",
                $"Ningún artículo tiene como referencia: {string.Join(", ", desconocidos)}."));
        }

        var pedido = await _crearPedido.EjecutarAsync(empresaId, new CrearPedidoVentaComando(socio.ClienteId, lineas, leido.Fecha), ct).ConfigureAwait(false);
        if (pedido.EsFallo)
        {
            return Resultado.Fallo<PedidoImportadoEdi>(pedido.Error);
        }

        _edi.Agregar(PedidoEdi.Crear(empresaId, leido.Numero, socio.GlnComprador, pedido.Valor.Id, _reloj));
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new PedidoImportadoEdi(pedido.Valor.Id, pedido.Valor.NumeroCompleto, leido.Numero, pedido.Valor.ClienteNombre, lineas.Count));
    }

    /// <summary>Contrasta un RECADV con el albarán al que responde (RFF+AAK = número del DESADV/albarán).</summary>
    public async Task<Resultado<ResultadoRecadv>> ContrastarRecadvAsync(Guid empresaId, string texto, CancellationToken ct = default)
    {
        RecepcionLeidaEdi leido;
        try
        {
            leido = Edifact.LeerRecadv(texto);
        }
        catch (FormatException ex)
        {
            return Resultado.Fallo<ResultadoRecadv>(Error.Validacion("edi.formato", ex.Message));
        }

        var albaran = leido.Albaran is null ? null
            : (await _albaranes.ListarAsync(empresaId, new FiltroAlbaranesVenta(), ct).ConfigureAwait(false))
                .FirstOrDefault(a => string.Equals(a.NumeroCompleto, leido.Albaran, StringComparison.OrdinalIgnoreCase));
        if (albaran is null)
        {
            return Resultado.Fallo<ResultadoRecadv>(Error.NoEncontrado("edi.albaran_no_encontrado",
                $"No hay ningún albarán {leido.Albaran ?? "(el RECADV no trae RFF+AAK)"}."));
        }

        var expedido = new Dictionary<string, (string Descripcion, decimal Cantidad)>();
        foreach (var l in albaran.Lineas)
        {
            var clave = await GtinAsync(l.ProductoId, ct).ConfigureAwait(false) ?? l.Descripcion;
            expedido[clave] = expedido.TryGetValue(clave, out var v) ? (v.Descripcion, v.Cantidad + l.Cantidad) : (l.Descripcion, l.Cantidad);
        }

        var recibido = leido.Lineas.GroupBy(l => l.Gtin ?? "").ToDictionary(g => g.Key, g => (g.Sum(x => x.Recibido), g.Any(x => x.Aceptado is not null) ? g.Sum(x => x.Aceptado ?? x.Recibido) : (decimal?)null));
        var lineas = expedido.Keys.Union(recibido.Keys.Where(k => k.Length > 0)).Select(k =>
        {
            var (desc, exp) = expedido.TryGetValue(k, out var e) ? e : ("(no expedido)", 0m);
            var (rec, acep) = recibido.TryGetValue(k, out var r) ? r : (0m, null);
            return new DiferenciaRecadv(k, desc, exp, rec, acep, (acep ?? rec) - exp);
        }).ToList();
        return Resultado.Ok(new ResultadoRecadv(leido.Numero, leido.Albaran, albaran.Id, lineas.All(l => l.Diferencia == 0), lineas));
    }

    // ------------------------------------------------------------------ utilidades

    private async Task<(string? Emisor, SocioEdi? Socio, Error? Fallo)> CodigosAsync(Guid? clienteId, CancellationToken ct)
    {
        var config = await _edi.ConfiguracionAsync(ct).ConfigureAwait(false);
        if (config is null)
        {
            return (null, null, Error.Validacion("edi.sin_configurar", "Configura primero el GLN de la empresa (Integraciones → EDI)."));
        }

        var socio = clienteId is { } c ? await _edi.SocioPorClienteAsync(c, ct).ConfigureAwait(false) : null;
        return socio is null
            ? (null, null, Error.Validacion("edi.cliente_sin_gln", "El cliente no es socio EDI: dale de alta su GLN."))
            : (config.GlnEmpresa, socio, null);
    }

    private async Task<string?> GtinAsync(Guid? productoId, CancellationToken ct)
    {
        if (productoId is not { } id)
        {
            return null;
        }

        var p = await _productos.ObtenerAsync(id, ct).ConfigureAwait(false);
        var referencia = p?.Referencia?.Trim();
        return CodigosGs1.EsGtinValido(referencia) ? referencia : null;
    }

    private static SocioEdiDto Dto(SocioEdi s, string? cliente) => new(s.Id, s.ClienteId, cliente, s.GlnComprador, s.GlnFacturacion, s.GlnEntrega);

    private static string Referencia(Guid id) => id.ToString("N")[..14].ToUpperInvariant();

    private static string Archivo(string numero)
    {
        var sb = new StringBuilder();
        foreach (var c in numero)
        {
            sb.Append(char.IsLetterOrDigit(c) ? c : '-');
        }

        return sb.ToString();
    }
}
