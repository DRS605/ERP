using System.Data;
using System.Data.Common;
using AlxorCore.Analisis.Aplicacion;
using AlxorCore.Analisis.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Analisis.Infraestructura;

/// <summary>Contexto del módulo de análisis: los informes guardados y la conexión para las consultas de solo lectura.</summary>
public sealed class AnalisisDbContext : DbContextEmpresaBase, IUnidadDeTrabajoAnalisis
{
    public const string Esquema = "analisis";

    public AnalisisDbContext(DbContextOptions<AnalisisDbContext> opciones, IPublicadorEventos publicador, IContextoEmpresa contexto)
        : base(opciones, publicador, contexto)
    {
    }

    public DbSet<InformeAnalisis> Informes => Set<InformeAnalisis>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Esquema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnalisisDbContext).Assembly);
        AplicarFiltroMultiempresa(modelBuilder);
    }
}

internal sealed class ConfiguracionInformeAnalisis : IEntityTypeConfiguration<InformeAnalisis>
{
    public void Configure(EntityTypeBuilder<InformeAnalisis> b)
    {
        b.ToTable("informe");
        b.HasKey(i => i.Id);
        b.Property(i => i.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(i => i.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(i => i.UsuarioId).HasColumnName("usuario_id").IsRequired();
        b.Property(i => i.Nombre).HasColumnName("nombre").HasMaxLength(InformeAnalisis.LongitudMaximaNombre).IsRequired();
        b.Property(i => i.Dataset).HasColumnName("dataset").HasMaxLength(40).IsRequired();
        b.Property(i => i.Definicion).HasColumnName("definicion").HasColumnType("jsonb").IsRequired();
        b.Property(i => i.Compartido).HasColumnName("compartido").IsRequired();
        b.Property(i => i.Favorito).HasColumnName("favorito").IsRequired();
        b.Property(i => i.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.Property(i => i.ActualizadoEn).HasColumnName("actualizado_en").IsRequired();
        b.HasIndex(i => new { i.EmpresaId, i.UsuarioId }).HasDatabaseName("ix_informe_empresa_usuario");
        b.Ignore(i => i.EventosDominio);
    }
}

internal sealed class RepositorioInformesAnalisis : IRepositorioInformesAnalisis
{
    private readonly AnalisisDbContext _db;

    public RepositorioInformesAnalisis(AnalisisDbContext db) => _db = db;

    public async Task<IReadOnlyList<InformeAnalisis>> ListarAsync(Guid empresaId, Guid usuarioId, CancellationToken ct = default) =>
        await _db.Informes.Where(i => i.EmpresaId == empresaId && (i.UsuarioId == usuarioId || i.Compartido)).ToListAsync(ct).ConfigureAwait(false);

    public Task<InformeAnalisis?> ObtenerAsync(Guid id, CancellationToken ct = default) =>
        _db.Informes.FirstOrDefaultAsync(i => i.Id == id, ct);

    public void Agregar(InformeAnalisis informe) => _db.Informes.Add(informe);

    public void Eliminar(InformeAnalisis informe) => _db.Informes.Remove(informe);
}

/// <summary>
/// Ejecuta las consultas del análisis por la conexión del contexto (el interceptor fija la empresa y el grupo, y la
/// RLS de cada tabla hace el resto), dentro de una transacción de solo lectura y con tiempo máximo.
/// </summary>
internal sealed class EjecutorAnalisisPostgres : IEjecutorAnalisis
{
    private const int SegundosMaximos = 60;
    private readonly AnalisisDbContext _db;

    public EjecutorAnalisisPostgres(AnalisisDbContext db) => _db = db;

    public async Task<IReadOnlyList<object?[]>> EjecutarAsync(SentenciaSql sentencia, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(sentencia);
        await _db.Database.OpenConnectionAsync(ct).ConfigureAwait(false);
        try
        {
            var conexion = _db.Database.GetDbConnection();
            await using var tx = await conexion.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct).ConfigureAwait(false);
            await using (var soloLectura = conexion.CreateCommand())
            {
                soloLectura.Transaction = tx;
                soloLectura.CommandText = $"SET TRANSACTION READ ONLY; SET LOCAL statement_timeout = '{SegundosMaximos}s'";
                await soloLectura.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }

            await using var cmd = conexion.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sentencia.Texto;
            cmd.CommandTimeout = SegundosMaximos + 5;
            foreach (var (nombre, valor) in sentencia.Parametros)
            {
                var p = cmd.CreateParameter();
                p.ParameterName = nombre;
                p.Value = valor;
                cmd.Parameters.Add(p);
            }

            var filas = new List<object?[]>();
            await using (var lector = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await lector.ReadAsync(ct).ConfigureAwait(false))
                {
                    var fila = new object?[lector.FieldCount];
                    for (var i = 0; i < fila.Length; i++)
                    {
                        fila[i] = await lector.IsDBNullAsync(i, ct).ConfigureAwait(false) ? null : lector.GetValue(i);
                    }

                    filas.Add(fila);
                }
            }

            await tx.CommitAsync(ct).ConfigureAwait(false);
            return filas;
        }
        finally
        {
            await _db.Database.CloseConnectionAsync().ConfigureAwait(false);
        }
    }
}

public sealed class AnalisisDbContextFactory : IDesignTimeDbContextFactory<AnalisisDbContext>
{
    public AnalisisDbContext CreateDbContext(string[] args)
    {
        var conexion = Environment.GetEnvironmentVariable("ALXOR_MIGRACIONES_CONEXION")
            ?? "Host=localhost;Port=5432;Database=alxor;Username=postgres;Password=postgres";
        var opciones = new DbContextOptionsBuilder<AnalisisDbContext>().UseNpgsql(conexion).Options;
        return new AnalisisDbContext(opciones, new PublicadorInactivo(), new ContextoVacio());
    }

    private sealed class PublicadorInactivo : IPublicadorEventos
    {
        public Task PublicarAsync(IReadOnlyCollection<IEventoDominio> eventos, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class ContextoVacio : IContextoEmpresa
    {
        public Guid? EmpresaId => null;
    }
}
