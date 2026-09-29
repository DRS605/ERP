using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AlxorCore.Persistencia;

/// <summary>
/// Base común de los <see cref="DbContext"/> de los módulos. Actúa como Unidad de Trabajo:
/// al guardar, confirma los cambios y publica los eventos de dominio acumulados por los
/// agregados dentro del mismo flujo, dejando negocio y auditoría consistentes.
/// </summary>
public abstract class DbContextBase : DbContext, IUnidadDeTrabajo
{
    private readonly IPublicadorEventos _publicadorEventos;
    private IDbContextTransaction? _transaccionPropia;

    protected DbContextBase(DbContextOptions opciones, IPublicadorEventos publicadorEventos)
        : base(opciones)
    {
        _publicadorEventos = publicadorEventos;
    }

    public async Task<int> GuardarCambiosAsync(CancellationToken ct = default)
    {
        var agregados = ChangeTracker
            .Entries<RaizAgregado<Guid>>()
            .Where(e => e.Entity.EventosDominio.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var eventos = agregados.SelectMany(a => a.EventosDominio).ToList();

        int filas;
        try
        {
            filas = await SaveChangesAsync(ct).ConfigureAwait(false);
            if (_transaccionPropia is not null)
            {
                await _transaccionPropia.CommitAsync(ct).ConfigureAwait(false);
            }
        }
        catch
        {
            if (_transaccionPropia is not null)
            {
                try
                {
                    await _transaccionPropia.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
                }
                catch (InvalidOperationException)
                {
                    // El commit falló (p. ej. una garantía diferida de la base de datos) y la transacción ya terminó:
                    // no hay nada que deshacer, y lo que importa es la excepción original.
                }
            }

            throw;
        }
        finally
        {
            if (_transaccionPropia is not null)
            {
                await _transaccionPropia.DisposeAsync().ConfigureAwait(false);
                _transaccionPropia = null;
            }
        }

        if (eventos.Count > 0)
        {
            await _publicadorEventos.PublicarAsync(eventos, ct).ConfigureAwait(false);
            foreach (var agregado in agregados)
            {
                agregado.LimpiarEventos();
            }
        }

        return filas;
    }

    /// <summary>
    /// Toma un bloqueo consultivo de PostgreSQL (<c>pg_advisory_xact_lock</c>) identificado por
    /// <paramref name="clave"/>, abriendo antes una transacción propia si no hay ninguna. El bloqueo
    /// dura hasta que <see cref="GuardarCambiosAsync"/> confirma (o hasta que se descarta el contexto,
    /// que deshace): así, «leer el último número y guardar el siguiente» ocurre sin que otra petición
    /// se cuele entre medias. Es reentrante dentro de la misma transacción.
    /// </summary>
    public async Task BloquearAsync(string clave, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clave);
        if (Database.CurrentTransaction is null)
        {
            _transaccionPropia = await Database.BeginTransactionAsync(ct).ConfigureAwait(false);
        }

        await Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({clave}, 0))", ct)
            .ConfigureAwait(false);
    }
}
