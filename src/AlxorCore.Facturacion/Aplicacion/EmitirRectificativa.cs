using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Facturacion.Dominio;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Organizacion.Dominio;

namespace AlxorCore.Facturacion.Aplicacion;

/// <summary>Datos para emitir una factura rectificativa que corrige a otra.</summary>
public sealed record EmitirRectificativaComando(
    string Motivo,
    IReadOnlyList<LineaComando> Lineas,
    DateOnly? FechaEmision = null,
    decimal? PorcentajeIrpf = null,
    string? Serie = null);

/// <summary>
/// Caso de uso: emitir una <b>factura rectificativa</b> (por sustitución). Referencia a la factura
/// original (que debe estar emitida), congela sus datos de cliente, numera en su serie (por defecto
/// <c>R</c>), genera su registro VeriFactu (tipo R1, encadenado) y marca la original como rectificada.
/// </summary>
public sealed class EmitirRectificativa
{
    /// <summary>Serie por defecto de las rectificativas.</summary>
    public const string SeriePorDefecto = "R";

    private readonly IConsultaProductos _productos;
    private readonly IResolverSerie _resolverSerie;
    private readonly IRepositorioFacturas _facturas;
    private readonly IConsultaEmpresas _empresas;
    private readonly IUnidadDeTrabajoFacturacion _unidadDeTrabajo;
    private readonly IResolverIvaEmpresa _resolverIva;
    private readonly IReloj _reloj;
    private readonly IResolverConceptos? _conceptos;

    public EmitirRectificativa(
        IConsultaProductos productos,
        IResolverSerie resolverSerie,
        IRepositorioFacturas facturas,
        IConsultaEmpresas empresas,
        IUnidadDeTrabajoFacturacion unidadDeTrabajo,
        IResolverIvaEmpresa resolverIva,
        IReloj reloj,
        IResolverConceptos? conceptos = null)
    {
        _conceptos = conceptos;
        _productos = productos;
        _resolverSerie = resolverSerie;
        _facturas = facturas;
        _empresas = empresas;
        _unidadDeTrabajo = unidadDeTrabajo;
        _resolverIva = resolverIva;
        _reloj = reloj;
    }

    public async Task<Resultado<FacturaDto>> EjecutarAsync(Guid empresaId, Guid facturaOriginalId, EmitirRectificativaComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);

        if (comando.Lineas is null || comando.Lineas.Count == 0)
        {
            return Resultado.Fallo<FacturaDto>(Error.Validacion("factura.sin_lineas", "La rectificativa debe tener al menos una línea."));
        }

        var original = await _facturas.ObtenerPorIdAsync(facturaOriginalId, ct).ConfigureAwait(false);
        if (original is null)
        {
            return Resultado.Fallo<FacturaDto>(Error.NoEncontrado("factura.no_encontrada", "La factura original no existe."));
        }

        // F6: solo se rectifica una factura emitida (no una ya anulada/rectificada).
        var marcado = original.MarcarRectificada();
        if (marcado.EsFallo)
        {
            return Resultado.Fallo<FacturaDto>(marcado.Error);
        }

        // En divisa, los precios van en la divisa de la original (el del artículo está en euros).
        if (original.Moneda is { } enDivisa && comando.Lineas.Any(l => l.PrecioUnitario is null))
        {
            return Resultado.Fallo<FacturaDto>(Error.Validacion("documento.divisa_precio", $"En una rectificativa en {enDivisa} cada línea lleva su precio en esa divisa."));
        }

        // La rectificativa corrige la original: lleva su mismo impuesto (IVA o IGIC).
        var resolucion = await ResolucionLineasFactura.ResolverAsync(comando.Lineas, _productos, ct, false, empresaId, _resolverIva, impuestoEmpresa: original.Impuesto).ConfigureAwait(false);
        if (resolucion.EsFallo)
        {
            return Resultado.Fallo<FacturaDto>(resolucion.Error);
        }

        // La rectificativa de una factura en divisa va en la misma divisa y al tipo de cambio de la original.
        var lineas = resolucion.Valor;
        if (original.Moneda is { } moneda && original.TasaCambio is { } tasa)
        {
            lineas = lineas.Select(l => l with { PrecioDivisa = l.PrecioUnitario, TasaCambio = tasa }).ToList();
        }

        var conConceptos = await ConceptosRectificativaAsync(original, comando.Lineas, lineas, ct).ConfigureAwait(false);
        if (conConceptos.EsFallo)
        {
            return Resultado.Fallo<FacturaDto>(conConceptos.Error);
        }

        lineas = conConceptos.Valor;
        var mencionFiscal = await ResolucionLineasFactura.MencionFiscalAsync(empresaId, lineas, _resolverIva, ct).ConfigureAwait(false);
        var cliente = new ClienteFacturado(
            original.ClienteId, original.ClienteNombre, original.ClienteNif,
            original.ClienteCalle, original.ClienteCodigoPostal, original.ClientePoblacion, original.ClienteProvincia, original.Pais, original.ActividadNegocioId);

        var hoy = DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var fecha = comando.FechaEmision ?? hoy;
        var porcentajeIrpf = comando.PorcentajeIrpf ?? original.PorcentajeIrpf;
        var serie = comando.Serie;
        if (string.IsNullOrWhiteSpace(serie))
        {
            serie = await _resolverSerie.ResolverPrefijoAsync(empresaId, TipoDocumento.Rectificativa, original.ClienteId, original.CentroId, null, ct).ConfigureAwait(false);
        }

        serie = string.IsNullOrWhiteSpace(serie) ? SeriePorDefecto : serie;

        // Número correlativo SIN huecos: se calcula (último + 1) dentro de la misma transacción que
        // guarda la factura y bajo un bloqueo por empresa, de modo que si la emisión falla no se pierde
        // ningún número y dos emisiones simultáneas no se pisan (ni el número ni la cadena VeriFactu).
        var numero = await _facturas.ReservarNumeroAsync(empresaId, serie, fecha, ct).ConfigureAwait(false);
        if (numero.EsFallo)
        {
            return Resultado.Fallo<FacturaDto>(numero.Error);
        }

        var numeroFactura = numero.Valor;
        var rectificativa = Factura.EmitirRectificativa(
            empresaId, numeroFactura, fecha, cliente, lineas, porcentajeIrpf, facturaOriginalId, comando.Motivo, _reloj);
        if (rectificativa.EsFallo)
        {
            return Resultado.Fallo<FacturaDto>(rectificativa.Error);
        }

        rectificativa.Valor.EstablecerMencionFiscal(mencionFiscal);
        rectificativa.Valor.EstablecerImpuesto(original.Impuesto);
        if (original.Moneda is { } m && original.TasaCambio is { } t)
        {
            rectificativa.Valor.EstablecerDivisa(m, t);
        }

        rectificativa.Valor.AsignarCentro(original.CentroId, original.CajaId);
        await RegistroVerifactu.AplicarAsync(empresaId, rectificativa.Valor, _empresas, _facturas, _reloj, ct).ConfigureAwait(false);
        _facturas.Agregar(rectificativa.Valor);
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(FacturaDto.Desde(rectificativa.Valor));
    }

    /// <summary>
    /// Cargos y abonos de la rectificativa (por sustitución: lleva las líneas corregidas). Una línea con conceptos pedidos
    /// lleva exactamente esos. Si no, la del mismo artículo (o, sin artículo, la misma descripción) en la factura original le
    /// pasa los suyos, recalculados sobre la línea corregida (como al pasar un documento a otro). Los cargos con acreedor
    /// pierden el acreedor: el servicio ya se prestó y su cargo es el de la original.
    /// </summary>
    private async Task<Resultado<List<NuevaLinea>>> ConceptosRectificativaAsync(Factura original, IReadOnlyList<LineaComando> comandos, List<NuevaLinea> lineas,
        CancellationToken ct)
    {
        if (_conceptos is null || comandos.Count != lineas.Count)
        {
            return Resultado.Ok(lineas);
        }

        var conCopias = comandos.Select((c, i) =>
        {
            if (c.Conceptos is not null)
            {
                return c;
            }

            var l = lineas[i];
            var origen = original.Lineas.FirstOrDefault(o => l.ProductoId is { } p ? o.ProductoId == p
                : o.ProductoId is null && string.Equals(o.Descripcion, l.Descripcion, StringComparison.OrdinalIgnoreCase));
            return origen is null || origen.Conceptos.Count == 0
                ? c with { Conceptos = [] }
                : c with
                {
                    // En la moneda del documento (en divisa, el importe en la divisa) y sin acreedor.
                    ConceptosCopiados = origen.Conceptos.Select(x => x with { Importe = x.ImporteDivisa ?? x.Importe, ImporteDivisa = null, AcreedorId = null, Provisionado = false })
                        .ToList(),
                };
        }).ToList();
        return await ResolucionLineasFactura.AplicarConceptosAsync(_conceptos, original.ClienteId, conCopias, lineas, null, false,
            new ContextoConceptos(null, original.FechaEmision, original.Moneda, original.TasaCambio), ct).ConfigureAwait(false);
    }
}
