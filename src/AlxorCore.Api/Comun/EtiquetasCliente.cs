using AlxorCore.Documentos.Aplicacion;
using AlxorCore.Extensiones.Aplicacion;

namespace AlxorCore.Api.Comun;

/// <summary>Aplica a la etiqueta de un palé la plantilla de su cliente (o la general) y la referencia del artículo en el cliente.</summary>
public static class EtiquetasCliente
{
    public static async Task<EtiquetaLogistica> AplicarAsync(DisenoEtiquetas disenos, Guid empresaId, Guid? clienteId, Guid? productoId, EtiquetaLogistica etiqueta,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(disenos);
        ArgumentNullException.ThrowIfNull(etiqueta);
        if (await disenos.ResolverAsync(empresaId, clienteId, productoId, ct).ConfigureAwait(false) is not { } d)
        {
            return etiqueta;
        }

        var referencia = d.ReferenciaCodigo is null ? null : d.ReferenciaDescripcion is null ? d.ReferenciaCodigo : $"{d.ReferenciaCodigo} · {d.ReferenciaDescripcion}";
        return etiqueta with
        {
            Marca = d.Marca ?? etiqueta.Marca,
            Gtin = d.ReferenciaGtin ?? etiqueta.Gtin,
            ReferenciaCliente = referencia,
            Diseno = new DisenoEtiqueta(d.Campos, d.TextoLibre, d.Formato == nameof(AlxorCore.Extensiones.Dominio.FormatoEtiqueta.Rollo100x150)),
        };
    }

    /// <summary>La etiqueta en PDF o, con <c>formato=zpl</c>, en ZPL para impresoras Zebra.</summary>
    public static IResult Respuesta(IGeneradorEtiquetaLogistica generador, EtiquetaLogistica etiqueta, AlxorCore.Organizacion.Aplicacion.Modelos.EmpresaDto empresa, string? formato)
    {
        ArgumentNullException.ThrowIfNull(generador);
        ArgumentNullException.ThrowIfNull(etiqueta);
        return string.Equals(formato, "zpl", StringComparison.OrdinalIgnoreCase)
            ? Results.File(System.Text.Encoding.UTF8.GetBytes(generador.GenerarZpl(etiqueta, empresa)), "text/plain", $"etiqueta-{etiqueta.Sscc}.zpl")
            : Results.File(generador.Generar(etiqueta, empresa), "application/pdf", $"etiqueta-{etiqueta.Sscc}.pdf");
    }
}
