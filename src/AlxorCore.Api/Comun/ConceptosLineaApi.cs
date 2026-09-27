using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Compras.Aplicacion;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Organizacion.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace AlxorCore.Api.Comun;

/// <summary>Uso de un concepto en los documentos del grupo (con la función SQL <c>catalogo.concepto_linea_en_uso</c>).</summary>
public sealed class UsoConceptosLinea : IUsoConceptosLinea
{
    private readonly OrganizacionDbContext _db;

    public UsoConceptosLinea(OrganizacionDbContext db) => _db = db;

    public async Task<bool> EnUsoAsync(Guid conceptoId, CancellationToken ct = default) =>
        await _db.Database.SqlQuery<bool>($"SELECT catalogo.concepto_linea_en_uso({conceptoId}) AS \"Value\"").SingleAsync(ct).ConfigureAwait(false);
}

public sealed record ResumenConceptoDto(Guid ConceptoId, string Codigo, string Nombre, string Efecto, decimal Ventas, int DocumentosVenta, decimal Compras, int DocumentosCompra);

public sealed record DetalleConceptoDto(string Circuito, Guid DocumentoId, string Documento, DateOnly Fecha, string Tercero, string Linea, string Codigo, string Nombre,
    string Efecto, decimal Valor, string Calculo, decimal Importe, bool Repartido);

public sealed record InformeConceptosDto(DateOnly Desde, DateOnly Hasta, IReadOnlyList<ResumenConceptoDto> Conceptos, IReadOnlyList<DetalleConceptoDto> Detalle,
    decimal PrecioVentas, decimal CosteVentas, decimal PrecioCompras, decimal CosteCompras);

/// <summary>
/// Informe de conceptos de línea de un periodo: cuánto ha sumado o restado cada concepto en las facturas de venta y en
/// los pedidos de compra (no cancelados), separando lo que cambia el importe de los documentos de lo que solo es coste.
/// </summary>
public sealed class InformeConceptosLinea
{
    private readonly IConsultaFacturas _facturas;
    private readonly IRepositorioPedidos _pedidos;

    public InformeConceptosLinea(IConsultaFacturas facturas, IRepositorioPedidos pedidos)
    {
        _facturas = facturas;
        _pedidos = pedidos;
    }

    public async Task<InformeConceptosDto> GenerarAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default)
    {
        var detalle = new List<DetalleConceptoDto>();
        foreach (var c in await _facturas.ListarConceptosAsync(empresaId, desde, hasta, ct).ConfigureAwait(false))
        {
            detalle.Add(Detalle("Ventas", c.DocumentoId, c.Documento, c.Fecha, c.Tercero, c.Linea, c.Concepto));
        }

        foreach (var p in (await _pedidos.ListarAsync(empresaId, ct).ConfigureAwait(false)).Where(p => p.Estado != "Cancelado" && p.Fecha >= desde && p.Fecha <= hasta))
        {
            foreach (var l in p.Lineas)
            {
                detalle.AddRange((l.Conceptos ?? []).Select(c => Detalle("Compras", p.Id, p.NumeroCompleto, p.Fecha, p.ProveedorTexto, l.Descripcion, c)));
            }
        }

        var resumen = detalle.GroupBy(d => (d.Codigo, d.Efecto))
            .Select(g => new ResumenConceptoDto(Guid.Empty, g.Key.Codigo, g.First().Nombre, g.Key.Efecto,
                Redondeo.Dos(g.Where(d => d.Circuito == "Ventas").Sum(d => d.Importe)), g.Where(d => d.Circuito == "Ventas").Select(d => d.DocumentoId).Distinct().Count(),
                Redondeo.Dos(g.Where(d => d.Circuito == "Compras").Sum(d => d.Importe)), g.Where(d => d.Circuito == "Compras").Select(d => d.DocumentoId).Distinct().Count()))
            .OrderBy(r => r.Codigo, StringComparer.Ordinal).ToList();
        decimal Suma(string circuito, string efecto) => Redondeo.Dos(detalle.Where(d => d.Circuito == circuito && d.Efecto == efecto).Sum(d => d.Importe));
        return new InformeConceptosDto(desde, hasta, resumen, detalle.OrderBy(d => d.Fecha).ThenBy(d => d.Documento, StringComparer.Ordinal).ToList(),
            Suma("Ventas", "Precio"), Suma("Ventas", "Coste"), Suma("Compras", "Precio"), Suma("Compras", "Coste"));
    }

    private static DetalleConceptoDto Detalle(string circuito, Guid id, string documento, DateOnly fecha, string tercero, string linea, ConceptoAplicado c) =>
        new(circuito, id, documento, fecha, tercero, linea, c.Codigo, c.Nombre, c.Efecto.ToString(), c.Valor, c.Calculo.ToString(), c.Importe, c.Repartido);
}
