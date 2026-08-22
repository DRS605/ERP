namespace AlxorCore.Organizacion.Dominio;

/// <summary>Régimen de IVA de la empresa (simplificado para el MVP).</summary>
public enum RegimenIva
{
    /// <summary>Régimen general del IVA.</summary>
    General = 1,

    /// <summary>Recargo de equivalencia.</summary>
    RecargoEquivalencia = 2,
}

/// <summary>Tipo de documento numerado por una serie.</summary>
public enum TipoDocumento
{
    /// <summary>Factura emitida.</summary>
    Factura = 1,

    /// <summary>Ticket / factura simplificada (TPV).</summary>
    Ticket = 2,

    /// <summary>Factura rectificativa.</summary>
    Rectificativa = 3,

    /// <summary>Presupuesto.</summary>
    Presupuesto = 4,

    /// <summary>Pedido de compra.</summary>
    PedidoCompra = 5,

    /// <summary>Albarán de compra.</summary>
    AlbaranCompra = 6,
}

/// <summary>Ámbito al que se asigna una serie: la empresa (por defecto) o un tercero concreto.</summary>
public enum AmbitoSerie
{
    /// <summary>Serie por defecto de la empresa para el tipo de documento.</summary>
    Empresa = 1,

    /// <summary>Serie específica para un cliente.</summary>
    Cliente = 2,

    /// <summary>Serie específica para un proveedor.</summary>
    Proveedor = 3,
}

/// <summary>Estado de una membresía (usuario dentro de una empresa).</summary>
public enum EstadoMembresia
{
    /// <summary>Membresía activa.</summary>
    Activa = 1,

    /// <summary>Membresía revocada.</summary>
    Revocada = 2,
}
