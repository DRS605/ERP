using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Organizacion.Aplicacion.Modelos;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Terceros.Aplicacion;
using AlxorCore.Tesoreria.Dominio;

namespace AlxorCore.Tesoreria.Aplicacion;

/// <summary>Un pago a un proveedor (línea de una remesa de transferencias o de confirming).</summary>
public sealed record PagoProveedor(string Referencia, string ProveedorNombre, string? Iban, decimal Importe, string Concepto, Guid GastoId = default);

/// <summary>Recopila los pagos pendientes a proveedores para las remesas de pago. Compartido.</summary>
internal static class Pagos
{
    public static async Task<Resultado<(EmpresaDto Empresa, List<PagoProveedor> Lista, List<string> Omitidos)>> RecopilarAsync(
        Guid empresaId, GenerarPagosComando comando, IConsultaGastos gastos, IConsultaProveedores proveedores,
        IConsultaEmpresas empresas, IRepositorioMovimientos movimientos, bool exigeIban, CancellationToken ct,
        bool ibanOrdenanteAparte = false, IReadOnlyDictionary<Guid, string>? bloqueados = null)
    {
        if (comando.GastoIds is null || comando.GastoIds.Count == 0)
        {
            return Resultado.Fallo<(EmpresaDto, List<PagoProveedor>, List<string>)>(Error.Validacion("pagos.sin_gastos", "Selecciona al menos un gasto para pagar."));
        }

        var empresa = await empresas.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        if (empresa is null)
        {
            return Resultado.Fallo<(EmpresaDto, List<PagoProveedor>, List<string>)>(Error.NoEncontrado("empresa.no_encontrada", "La empresa no existe."));
        }

        if (!ibanOrdenanteAparte && string.IsNullOrWhiteSpace(empresa.Iban))
        {
            return Resultado.Fallo<(EmpresaDto, List<PagoProveedor>, List<string>)>(Error.Validacion("pagos.empresa_sin_iban", "Configura el IBAN de la empresa (Ajustes → Datos de cobro)."));
        }

        var lista = new List<PagoProveedor>();
        var omitidos = new List<string>();
        foreach (var gastoId in comando.GastoIds)
        {
            var gasto = await gastos.ObtenerAsync(gastoId, ct).ConfigureAwait(false);
            if (gasto is null)
            {
                omitidos.Add($"{gastoId}: gasto no encontrado.");
                continue;
            }

            if (gasto.Estado == "Anulado")
            {
                omitidos.Add($"{gasto.Concepto}: está anulado.");
                continue;
            }

            if (bloqueados is not null && bloqueados.TryGetValue(gastoId, out var remesa))
            {
                omitidos.Add($"{gasto.Concepto}: ya está en la remesa {remesa}.");
                continue;
            }

            var liquidado = await movimientos.SumaAsync(TipoDocumentoTesoreria.Gasto, gastoId, ct).ConfigureAwait(false);
            var pendiente = Redondeo.Dos(gasto.Total - liquidado);
            if (pendiente <= 0m)
            {
                omitidos.Add($"{gasto.Concepto}: ya está pagado.");
                continue;
            }

            var proveedor = gasto.ProveedorId is { } pid ? await proveedores.ObtenerAsync(pid, ct).ConfigureAwait(false) : null;
            if (exigeIban && (proveedor is null || string.IsNullOrWhiteSpace(proveedor.Iban)))
            {
                omitidos.Add($"{gasto.Concepto}: el proveedor no tiene IBAN.");
                continue;
            }

            var nombre = proveedor?.Nombre ?? gasto.ProveedorTexto ?? gasto.Concepto;
            lista.Add(new PagoProveedor(gastoId.ToString("N")[..12], nombre, proveedor?.Iban, pendiente, gasto.Concepto, gastoId));
        }

        if (lista.Count == 0)
        {
            return Resultado.Fallo<(EmpresaDto, List<PagoProveedor>, List<string>)>(Error.Validacion("pagos.sin_pagos", "Ninguno de los gastos seleccionados se puede pagar. " + string.Join(" ", omitidos)));
        }

        return Resultado.Ok((empresa, lista, omitidos));
    }
}
