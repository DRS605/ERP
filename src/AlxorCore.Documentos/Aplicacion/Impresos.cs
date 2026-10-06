using AlxorCore.Organizacion.Aplicacion.Modelos;

namespace AlxorCore.Documentos.Aplicacion;

/// <summary>Tercero de un impreso: el cliente, el proveedor o el agricultor.</summary>
public sealed record TerceroImpreso(string Etiqueta, string Nombre, string? Nif = null, string? Direccion = null);

/// <summary>
/// Línea de un impreso. <paramref name="Precio"/> e <paramref name="Importe"/> son null en los documentos sin valorar
/// (un albarán sin precios). <paramref name="Detalle"/> va debajo, en pequeño (lote, palé, cargos…).
/// </summary>
public sealed record LineaImpresa(string Descripcion, decimal Cantidad, string? Unidad = null, decimal? Precio = null, decimal? Descuento = null,
    decimal? Importe = null, string? Detalle = null);

/// <summary>Fila del cuadro de totales. La destacada va en grande (el total o el líquido a pagar).</summary>
public sealed record TotalImpreso(string Etiqueta, decimal Importe, bool Destacado = false);

/// <summary>
/// Documento comercial genérico para imprimir: albarán, pedido de venta o de compra, liquidación al agricultor…
/// El módulo de cada documento lo rellena; aquí solo se maqueta, con la plantilla de la empresa.
/// </summary>
public sealed record DocumentoImpreso(
    string Titulo, string Numero, DateOnly Fecha, TerceroImpreso Tercero, IReadOnlyList<LineaImpresa> Lineas,
    IReadOnlyList<TotalImpreso> Totales, IReadOnlyList<(string Etiqueta, string Valor)>? Datos = null, string? Observaciones = null,
    string? Leyenda = null, string? TituloCantidad = null, bool Valorado = true, string? Idioma = null, string? Moneda = null);

/// <summary>Puerto de generación del PDF de un documento comercial genérico.</summary>
public interface IGeneradorPdfDocumento
{
    byte[] Generar(DocumentoImpreso documento, EmpresaDto emisor);
}
