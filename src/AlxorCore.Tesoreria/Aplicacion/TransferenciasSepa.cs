namespace AlxorCore.Tesoreria.Aplicacion;

/// <summary>Resultado de generar una remesa de transferencias/pagos (SEPA o confirming). Si queda registrada: su id y código.</summary>
public sealed record RemesaPagoDto(string Fichero, string NombreArchivo, int NumeroPagos, decimal Total, IReadOnlyList<string> Omitidos,
    Guid? RemesaId = null, string? Codigo = null);

/// <summary>Datos para generar una remesa de pagos a proveedores desde los gastos indicados.</summary>
public sealed record GenerarPagosComando(IReadOnlyList<Guid> GastoIds, DateOnly? FechaPago = null, Guid? CuentaBancariaId = null);
