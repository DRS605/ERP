using Microsoft.EntityFrameworkCore;

namespace AlxorCore.Persistencia;

/// <summary>
/// Baja de una empresa: la única operación que puede borrar documentos que la base de datos protege
/// (facturas emitidas, movimientos, auditoría…). Lo declara de forma explícita fijando
/// <see cref="GarantiasSql.ParametroBorradoEmpresa"/> <b>solo dentro de su transacción</b>, y solo
/// para esa empresa; cualquier otro borrado sigue rechazándose.
/// </summary>
public static class BorradoEmpresa
{
    public static async Task EjecutarAsync(DbContext contexto, Guid empresaId, Func<Task> borrar, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(contexto);
        ArgumentNullException.ThrowIfNull(borrar);

        await using var transaccion = await contexto.Database.BeginTransactionAsync(ct).ConfigureAwait(false);
        var empresa = empresaId.ToString("D");
        await contexto.Database
            .ExecuteSqlInterpolatedAsync($"SELECT set_config({GarantiasSql.ParametroBorradoEmpresa}, {empresa}, true)", ct)
            .ConfigureAwait(false);
        await borrar().ConfigureAwait(false);
        await transaccion.CommitAsync(ct).ConfigureAwait(false);
    }
}
