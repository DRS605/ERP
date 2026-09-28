using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Integraciones.Dominio;

/// <summary>Reglas de los códigos GS1 (GLN de 13 dígitos con su dígito de control).</summary>
public static class CodigosGs1
{
    public static bool EsGlnValido(string? gln) => gln is { Length: 13 } && gln.All(char.IsDigit) && DigitoControl(gln[..12]) == gln[12] - '0';

    /// <summary>GTIN de 8, 12, 13 o 14 dígitos con su dígito de control.</summary>
    public static bool EsGtinValido(string? gtin) =>
        gtin is { Length: 8 or 12 or 13 or 14 } && gtin.All(char.IsDigit) && DigitoControl(gtin[..^1]) == gtin[^1] - '0';

    public static int DigitoControl(string cuerpo)
    {
        ArgumentNullException.ThrowIfNull(cuerpo);
        var suma = 0;
        for (var i = 0; i < cuerpo.Length; i++)
        {
            var d = cuerpo[cuerpo.Length - 1 - i] - '0';
            suma += i % 2 == 0 ? d * 3 : d;
        }

        return (10 - suma % 10) % 10;
    }
}

/// <summary>Configuración EDI de la empresa: su GLN (emisor de los mensajes EANCOM).</summary>
public sealed class ConfiguracionEdi : RaizAgregadoEmpresa<Guid>
{
    private ConfiguracionEdi(Guid id)
        : base(id, Guid.Empty)
    {
        GlnEmpresa = null!;
    }

    private ConfiguracionEdi(Guid id, Guid empresaId, string gln)
        : base(id, empresaId)
    {
        GlnEmpresa = gln;
    }

    public string GlnEmpresa { get; private set; }

    public static Resultado<ConfiguracionEdi> Crear(Guid empresaId, string? gln) =>
        CodigosGs1.EsGlnValido(gln?.Trim()) ? Resultado.Ok(new ConfiguracionEdi(Guid.NewGuid(), empresaId, gln!.Trim()))
            : Resultado.Fallo<ConfiguracionEdi>(Error.Validacion("edi.gln", "El GLN de la empresa debe tener 13 dígitos con su dígito de control."));

    public Resultado Cambiar(string? gln)
    {
        if (!CodigosGs1.EsGlnValido(gln?.Trim()))
        {
            return Resultado.Fallo(Error.Validacion("edi.gln", "El GLN de la empresa debe tener 13 dígitos con su dígito de control."));
        }

        GlnEmpresa = gln!.Trim();
        return Resultado.Ok();
    }
}

/// <summary>
/// Socio EDI: un cliente con sus GLN (comprador, a quien se factura y punto de entrega). Con él, un ORDERS entrante se
/// asigna al cliente y los INVOIC y DESADV salientes llevan sus códigos.
/// </summary>
public sealed class SocioEdi : RaizAgregadoEmpresa<Guid>
{
    private SocioEdi(Guid id)
        : base(id, Guid.Empty)
    {
        GlnComprador = null!;
    }

    private SocioEdi(Guid id, Guid empresaId, Guid clienteId)
        : base(id, empresaId)
    {
        ClienteId = clienteId;
        GlnComprador = string.Empty;
    }

    public Guid ClienteId { get; private set; }

    public string GlnComprador { get; private set; }

    public string? GlnFacturacion { get; private set; }

    public string? GlnEntrega { get; private set; }

    public static Resultado<SocioEdi> Crear(Guid empresaId, Guid clienteId, string? comprador, string? facturacion, string? entrega)
    {
        var s = new SocioEdi(Guid.NewGuid(), empresaId, clienteId);
        var r = s.Cambiar(comprador, facturacion, entrega);
        return r.EsFallo ? Resultado.Fallo<SocioEdi>(r.Error) : Resultado.Ok(s);
    }

    public Resultado Cambiar(string? comprador, string? facturacion, string? entrega)
    {
        static string? Limpio(string? g) => string.IsNullOrWhiteSpace(g) ? null : g.Trim();
        var (c, f, e) = (Limpio(comprador), Limpio(facturacion), Limpio(entrega));
        if (!CodigosGs1.EsGlnValido(c) || (f is not null && !CodigosGs1.EsGlnValido(f)) || (e is not null && !CodigosGs1.EsGlnValido(e)))
        {
            return Resultado.Fallo(Error.Validacion("edi.gln", "Cada GLN debe tener 13 dígitos con su dígito de control (el del comprador es obligatorio)."));
        }

        GlnComprador = c!;
        GlnFacturacion = f;
        GlnEntrega = e;
        return Resultado.Ok();
    }
}

/// <summary>Pedido recibido por EDI (ORDERS): enlaza el número del cliente con el pedido de venta creado y evita importarlo dos veces.</summary>
public sealed class PedidoEdi : RaizAgregadoEmpresa<Guid>
{
    private PedidoEdi(Guid id)
        : base(id, Guid.Empty)
    {
        NumeroCliente = null!;
        GlnComprador = null!;
    }

    private PedidoEdi(Guid id, Guid empresaId, string numeroCliente, string glnComprador, Guid pedidoVentaId, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        NumeroCliente = numeroCliente;
        GlnComprador = glnComprador;
        PedidoVentaId = pedidoVentaId;
        RecibidoEn = ahora;
    }

    public string NumeroCliente { get; private set; }

    public string GlnComprador { get; private set; }

    public Guid PedidoVentaId { get; private set; }

    public DateTimeOffset RecibidoEn { get; private set; }

    public static PedidoEdi Crear(Guid empresaId, string numeroCliente, string glnComprador, Guid pedidoVentaId, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        return new PedidoEdi(Guid.NewGuid(), empresaId, numeroCliente.Length > 35 ? numeroCliente[..35] : numeroCliente, glnComprador, pedidoVentaId, reloj.AhoraUtc);
    }
}
