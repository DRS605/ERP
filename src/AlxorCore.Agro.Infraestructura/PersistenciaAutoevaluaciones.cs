using AlxorCore.Agro.Aplicacion;
using AlxorCore.Agro.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Agro.Infraestructura;

internal sealed class ConfiguracionListaControl : IEntityTypeConfiguration<ListaControl>
{
    public void Configure(EntityTypeBuilder<ListaControl> b)
    {
        Columnas.Base(b, "lista_control");
        b.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(ListaControl.LongitudCodigo).IsRequired();
        b.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(ListaControl.LongitudNombre).IsRequired();
        b.Property(x => x.Version).HasColumnName("version").HasMaxLength(50);
        b.Property(x => x.Activa).HasColumnName("activa").IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.Codigo }).IsUnique().HasDatabaseName("ux_lista_control_codigo");
        b.OwnsMany(x => x.Puntos, p =>
        {
            p.ToTable("punto_control");
            p.WithOwner().HasForeignKey("lista_control_id");
            p.HasKey(x => x.Id);
            p.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            p.Property(x => x.Orden).HasColumnName("orden").IsRequired();
            p.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(ListaControl.LongitudCodigo).IsRequired();
            p.Property(x => x.Texto).HasColumnName("texto").HasMaxLength(ListaControl.LongitudTexto).IsRequired();
            Columnas.Enum(p.Property(x => x.Nivel), "nivel");
        });
        b.Navigation(x => x.Puntos).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ConfiguracionAutoevaluacion : IEntityTypeConfiguration<Autoevaluacion>
{
    public void Configure(EntityTypeBuilder<Autoevaluacion> b)
    {
        Columnas.Base(b, "autoevaluacion");
        b.Property(x => x.ListaControlId).HasColumnName("lista_control_id").IsRequired();
        b.Property(x => x.ListaCodigo).HasColumnName("lista_codigo").HasMaxLength(ListaControl.LongitudCodigo).IsRequired();
        b.Property(x => x.AgricultorId).HasColumnName("agricultor_id");
        Columnas.Enum(b.Property(x => x.Tipo), "tipo");
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.Auditor).HasColumnName("auditor").HasMaxLength(150).IsRequired();
        b.Property(x => x.Observaciones).HasColumnName("observaciones").HasMaxLength(1000);
        b.Property(x => x.Cerrada).HasColumnName("cerrada").IsRequired();
        b.Property(x => x.CerradaEn).HasColumnName("cerrada_en");
        b.Property(x => x.CreadaEn).HasColumnName("creada_en").IsRequired();
        b.Ignore(x => x.Supera);
        b.HasIndex(x => new { x.EmpresaId, x.Fecha }).HasDatabaseName("ix_autoevaluacion_fecha");
        b.HasIndex(x => x.ListaControlId).HasDatabaseName("ix_autoevaluacion_lista");
        b.HasIndex(x => x.AgricultorId).HasDatabaseName("ix_autoevaluacion_agricultor");
        b.OwnsMany(x => x.Respuestas, r =>
        {
            r.ToTable("respuesta_autoevaluacion");
            r.WithOwner().HasForeignKey("autoevaluacion_id");
            r.HasKey(x => x.Id);
            r.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            r.Property(x => x.Orden).HasColumnName("orden").IsRequired();
            r.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(ListaControl.LongitudCodigo).IsRequired();
            r.Property(x => x.Texto).HasColumnName("texto").HasMaxLength(ListaControl.LongitudTexto).IsRequired();
            Columnas.Enum(r.Property(x => x.Nivel), "nivel");
            r.Property(x => x.Resultado).HasColumnName("resultado").HasMaxLength(30).HasConversion<string>();
            r.Property(x => x.Comentario).HasColumnName("comentario").HasMaxLength(500);
            r.Property(x => x.AccionCorrectiva).HasColumnName("accion_correctiva").HasMaxLength(500);
            r.Property(x => x.FechaLimite).HasColumnName("fecha_limite");
        });
        b.Navigation(x => x.Respuestas).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class RepositorioAutoevaluaciones : IRepositorioAutoevaluaciones
{
    private readonly AgroDbContext _ctx;

    public RepositorioAutoevaluaciones(AgroDbContext ctx) => _ctx = ctx;

    public void Agregar(ListaControl lista) => _ctx.Add(lista);

    public void Agregar(Autoevaluacion evaluacion) => _ctx.Add(evaluacion);

    public void Eliminar(ListaControl lista) => _ctx.Remove(lista);

    public void Eliminar(Autoevaluacion evaluacion) => _ctx.Remove(evaluacion);

    public Task<ListaControl?> ListaAsync(Guid id, CancellationToken ct = default) => _ctx.Set<ListaControl>().FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task<IReadOnlyList<ListaControl>> ListasAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<ListaControl>().AsNoTracking().Where(l => l.EmpresaId == empresaId).OrderBy(l => l.Codigo).ToListAsync(ct).ConfigureAwait(false);

    public Task<bool> ExisteCodigoAsync(Guid empresaId, string codigo, CancellationToken ct = default) =>
        _ctx.Set<ListaControl>().AnyAsync(l => l.EmpresaId == empresaId && l.Codigo == codigo, ct);

    public Task<bool> ListaUsadaAsync(Guid listaId, CancellationToken ct = default) => _ctx.Set<Autoevaluacion>().AnyAsync(a => a.ListaControlId == listaId, ct);

    public Task<Autoevaluacion?> EvaluacionAsync(Guid id, CancellationToken ct = default) => _ctx.Set<Autoevaluacion>().FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<Autoevaluacion>> EvaluacionesAsync(Guid empresaId, Guid? agricultorId, int? anio, CancellationToken ct = default)
    {
        var q = _ctx.Set<Autoevaluacion>().AsNoTracking().Where(a => a.EmpresaId == empresaId);
        if (agricultorId is { } g)
        {
            q = q.Where(a => a.AgricultorId == g);
        }

        if (anio is { } y)
        {
            q = q.Where(a => a.Fecha >= new DateOnly(y, 1, 1) && a.Fecha <= new DateOnly(y, 12, 31));
        }

        return await q.OrderByDescending(a => a.Fecha).ThenByDescending(a => a.CreadaEn).ToListAsync(ct).ConfigureAwait(false);
    }
}
