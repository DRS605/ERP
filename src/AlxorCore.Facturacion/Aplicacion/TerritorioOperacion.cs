using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Organizacion.Aplicacion.Modelos;

namespace AlxorCore.Facturacion.Aplicacion;

/// <summary>
/// Impuesto (IVA o IGIC) de un documento de venta y tipo de cada línea. Una empresa de un solo territorio usa siempre el
/// suyo; una con actividad en la Península y en Canarias, el elegido en el documento o el de los tipos de sus líneas. Las
/// líneas sin tipo toman el del artículo (en Canarias, su tipo del IGIC o el equivalente de su IVA) o el general del
/// impuesto. Presupuestos, pedidos y albaranes lo resuelven igual que la factura, así que al convertirlos no cambia nada.
/// </summary>
public static class TerritorioOperacion
{
    /// <summary>Impuesto del documento; error si se pide el del otro territorio en una empresa que no opera en él.</summary>
    public static Resultado<TipoImpuesto> Resolver(EmpresaDto? empresa, TipoImpuesto? elegido, IEnumerable<string?> codigosLineas)
    {
        var principal = empresa?.ImpuestoIndirecto ?? TipoImpuesto.Iva;
        if (elegido is { } pedido && empresa is { OperaEnAmbosTerritorios: false } && pedido != principal)
        {
            return Resultado.Fallo<TipoImpuesto>(Error.Validacion("documento.territorio",
                $"La empresa solo tributa por {principal.Siglas()}. Si también opera en el otro territorio, márcalo en Ajustes → Datos fiscales."));
        }

        return Resultado.Ok(EmitirFactura.ImpuestoDeLaOperacion(empresa, codigosLineas, elegido));
    }

    /// <summary>Tipo de una línea con el impuesto del documento. Un tipo del catálogo estatal del otro impuesto se rechaza.</summary>
    public static Resultado<string> CodigoLinea(TipoImpuesto impuesto, string? codigo, ProductoDto? producto)
    {
        string resuelto;
        if (!string.IsNullOrWhiteSpace(codigo))
        {
            resuelto = codigo.Trim().ToUpperInvariant();
        }
        else if (producto is not null)
        {
            resuelto = impuesto == TipoImpuesto.Igic && Impuesto.TipoDeCodigo(producto.CodigoIva) == TipoImpuesto.Iva
                ? producto.CodigoIgic ?? Impuesto.EquivalenteIgic(producto.CodigoIva)
                : producto.CodigoIva;
        }
        else
        {
            resuelto = impuesto == TipoImpuesto.Igic ? Impuesto.IgicGeneral.Codigo : Impuesto.IvaGeneral.Codigo;
        }

        if (Impuesto.PorCodigoImpuesto(resuelto) is { EsCorrecto: true } i && i.Valor.Tipo != impuesto)
        {
            return Resultado.Fallo<string>(Error.Validacion("documento.impuesto_territorio",
                $"El tipo {resuelto} es de {i.Valor.Tipo.Siglas()}, pero este documento va con {impuesto.Siglas()}. Un documento no mezcla IVA e IGIC."));
        }

        return Resultado.Ok(resuelto);
    }
}
