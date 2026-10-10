using AlxorCore.Organizacion.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Organizacion.Infraestructura.Persistencia.Configuraciones;

/// <summary>Mapeo de <see cref="Instalacion"/> (tabla global del panel de la plataforma, sin RLS).</summary>
internal sealed class ConfiguracionInstalacion : IEntityTypeConfiguration<Instalacion>
{
    public void Configure(EntityTypeBuilder<Instalacion> builder)
    {
        builder.ToTable("instalacion");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");

        builder.Property(i => i.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
        builder.Property(i => i.Contacto).HasColumnName("contacto").HasMaxLength(200);
        builder.Property(i => i.Email).HasColumnName("email").HasMaxLength(200);
        builder.Property(i => i.Telefono).HasColumnName("telefono").HasMaxLength(40);
        builder.Property(i => i.Poblacion).HasColumnName("poblacion").HasMaxLength(200);

        builder.Property(i => i.Plan).HasColumnName("plan").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(i => i.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(i => i.CuotaMensual).HasColumnName("cuota_mensual").HasPrecision(12, 2).IsRequired();
        builder.Property(i => i.FechaAlta).HasColumnName("fecha_alta").IsRequired();
        builder.Property(i => i.ProximoCobro).HasColumnName("proximo_cobro");

        builder.Property(i => i.Clave).HasColumnName("clave").HasMaxLength(40).IsRequired();
        builder.HasIndex(i => i.Clave).IsUnique().HasDatabaseName("ux_instalacion_clave");

        builder.Property(i => i.Notas).HasColumnName("notas").HasMaxLength(2000);
        builder.Property(i => i.UltimaConexion).HasColumnName("ultima_conexion");
        builder.Property(i => i.CreadoEn).HasColumnName("creado_en").IsRequired();
        builder.Property(i => i.ActualizadoEn).HasColumnName("actualizado_en").IsRequired();
    }
}
