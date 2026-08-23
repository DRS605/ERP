using AlxorCore.Nucleo.Comun;
using AlxorCore.Organizacion.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Organizacion.Infraestructura.Persistencia.Configuraciones;

internal sealed class ConfiguracionEmpresa : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("empresa");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.GrupoId).HasColumnName("grupo_id").IsRequired();
        builder.HasIndex(e => e.GrupoId).HasDatabaseName("ix_empresa_grupo");

        builder.Property(e => e.Nif)
            .HasColumnName("nif")
            .HasMaxLength(20)
            .HasConversion(n => n.Valor, v => Nif.Rehidratar(v))
            .IsRequired();
        builder.HasIndex(e => e.Nif).IsUnique().HasDatabaseName("ux_empresa_nif");

        builder.Property(e => e.RazonSocial)
            .HasColumnName("razon_social")
            .HasMaxLength(Empresa.LongitudMaximaRazonSocial)
            .IsRequired();

        builder.OwnsOne(e => e.Direccion, d =>
        {
            d.Property(p => p.Calle).HasColumnName("direccion_calle").HasMaxLength(200);
            d.Property(p => p.CodigoPostal).HasColumnName("direccion_cp").HasMaxLength(10);
            d.Property(p => p.Poblacion).HasColumnName("direccion_poblacion").HasMaxLength(120);
            d.Property(p => p.Provincia).HasColumnName("direccion_provincia").HasMaxLength(120);
            d.Property(p => p.Pais).HasColumnName("direccion_pais").HasMaxLength(2);
        });

        builder.Property(e => e.RegimenIva).HasColumnName("regimen_iva").HasMaxLength(30).HasConversion<string>().IsRequired();
        builder.Property(e => e.Moneda).HasColumnName("moneda").HasMaxLength(3).IsRequired();
        builder.Property(e => e.Pais).HasColumnName("pais").HasMaxLength(2).IsRequired();
        builder.Property(e => e.Iban).HasColumnName("iban").HasMaxLength(34);
        builder.Property(e => e.IdentificadorAcreedor).HasColumnName("identificador_acreedor").HasMaxLength(35);
        builder.Property(e => e.MetodoValoracion).HasColumnName("metodo_valoracion").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(e => e.ControlRiesgo).HasColumnName("control_riesgo").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(e => e.Telefono).HasColumnName("telefono").HasMaxLength(40);
        builder.Property(e => e.Web).HasColumnName("web").HasMaxLength(120);
        builder.Property(e => e.EmailContacto).HasColumnName("email_contacto").HasMaxLength(254);
        builder.Property(e => e.ColorPrincipal).HasColumnName("color_principal").HasMaxLength(7);
        builder.Property(e => e.TextoPie).HasColumnName("texto_pie").HasMaxLength(Empresa.LongitudMaximaTexto);
        builder.Property(e => e.LogoPng).HasColumnName("logo_png").HasColumnType("bytea");
        builder.Property(e => e.CreadoEn).HasColumnName("creado_en").IsRequired();
        builder.Property(e => e.ActualizadoEn).HasColumnName("actualizado_en").IsRequired();

        builder.Ignore(e => e.EventosDominio);
    }
}

internal sealed class ConfiguracionGrupo : IEntityTypeConfiguration<Grupo>
{
    public void Configure(EntityTypeBuilder<Grupo> builder)
    {
        builder.ToTable("grupo");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).HasColumnName("id");
        builder.Property(g => g.Nombre).HasColumnName("nombre").HasMaxLength(Grupo.LongitudMaximaNombre).IsRequired();
        builder.Property(g => g.CreadoEn).HasColumnName("creado_en").IsRequired();
        builder.Ignore(g => g.EventosDominio);
    }
}

internal sealed class ConfiguracionMembresia : IEntityTypeConfiguration<Membresia>
{
    public void Configure(EntityTypeBuilder<Membresia> builder)
    {
        builder.ToTable("membresia");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.UsuarioId).HasColumnName("usuario_id").IsRequired();
        builder.Property(m => m.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(m => m.RolCodigo).HasColumnName("rol_codigo").HasMaxLength(30).IsRequired();
        builder.Property(m => m.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(m => m.CreadoEn).HasColumnName("creado_en").IsRequired();

        builder.HasIndex(m => new { m.UsuarioId, m.EmpresaId }).IsUnique().HasDatabaseName("ux_membresia_usuario_empresa");
        builder.Ignore(m => m.EventosDominio);
    }
}

internal sealed class ConfiguracionSerie : IEntityTypeConfiguration<SerieNumeracion>
{
    public void Configure(EntityTypeBuilder<SerieNumeracion> builder)
    {
        builder.ToTable("serie_numeracion");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(s => s.TipoDocumento).HasColumnName("tipo_documento").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(s => s.Ejercicio).HasColumnName("ejercicio").IsRequired();
        builder.Property(s => s.Prefijo).HasColumnName("prefijo").HasMaxLength(SerieNumeracion.LongitudMaximaPrefijo).IsRequired();
        builder.Property(s => s.SiguienteNumero).HasColumnName("siguiente_numero").IsRequired();
        builder.Property(s => s.CreadoEn).HasColumnName("creado_en").IsRequired();

        builder.HasIndex(s => new { s.EmpresaId, s.TipoDocumento, s.Ejercicio, s.Prefijo })
            .IsUnique()
            .HasDatabaseName("ux_serie_empresa_tipo_ejercicio_prefijo");
        builder.Ignore(s => s.EventosDominio);
    }
}

internal sealed class ConfiguracionAsignacionSerie : IEntityTypeConfiguration<AsignacionSerie>
{
    public void Configure(EntityTypeBuilder<AsignacionSerie> builder)
    {
        builder.ToTable("asignacion_serie");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(a => a.TipoDocumento).HasColumnName("tipo_documento").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(a => a.Ambito).HasColumnName("ambito").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(a => a.TerceroId).HasColumnName("tercero_id").IsRequired();
        builder.Property(a => a.Prefijo).HasColumnName("prefijo").HasMaxLength(SerieNumeracion.LongitudMaximaPrefijo).IsRequired();
        builder.Property(a => a.CreadoEn).HasColumnName("creado_en").IsRequired();

        builder.HasIndex(a => new { a.EmpresaId, a.TipoDocumento, a.Ambito, a.TerceroId })
            .IsUnique()
            .HasDatabaseName("ux_asignacion_serie");
        builder.Ignore(a => a.EventosDominio);
    }
}

internal sealed class ConfiguracionActividadNegocio : IEntityTypeConfiguration<ActividadNegocio>
{
    public void Configure(EntityTypeBuilder<ActividadNegocio> builder)
    {
        builder.ToTable("actividad_negocio");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.GrupoId).HasColumnName("grupo_id").IsRequired();
        builder.HasIndex(a => a.GrupoId).HasDatabaseName("ix_actividad_negocio_grupo");
        builder.Property(a => a.Nombre).HasColumnName("nombre").HasMaxLength(ActividadNegocio.LongitudMaximaNombre).IsRequired();
        builder.Property(a => a.Activa).HasColumnName("activa").IsRequired();
        builder.Property(a => a.CreadoEn).HasColumnName("creado_en").IsRequired();
        builder.Property(a => a.ActualizadoEn).HasColumnName("actualizado_en").IsRequired();
        builder.Ignore(a => a.EventosDominio);
    }
}

internal sealed class ConfiguracionVisibilidadActividad : IEntityTypeConfiguration<VisibilidadActividad>
{
    public void Configure(EntityTypeBuilder<VisibilidadActividad> builder)
    {
        builder.ToTable("visibilidad_actividad");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnName("id");
        builder.Property(v => v.GrupoId).HasColumnName("grupo_id").IsRequired();
        builder.Property(v => v.UsuarioId).HasColumnName("usuario_id").IsRequired();
        builder.Property(v => v.Area).HasColumnName("area").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(v => v.ActividadNegocioId).HasColumnName("actividad_negocio_id").IsRequired();
        builder.Property(v => v.CreadoEn).HasColumnName("creado_en").IsRequired();

        builder.HasIndex(v => new { v.GrupoId, v.UsuarioId, v.Area, v.ActividadNegocioId })
            .IsUnique()
            .HasDatabaseName("ux_visibilidad_grupo_usuario_area_actividad");
        builder.HasIndex(v => new { v.UsuarioId, v.Area }).HasDatabaseName("ix_visibilidad_usuario_area");
        builder.Ignore(v => v.EventosDominio);
    }
}

internal sealed class ConfiguracionFormaPago : IEntityTypeConfiguration<FormaPago>
{
    public void Configure(EntityTypeBuilder<FormaPago> builder)
    {
        builder.ToTable("forma_pago");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasColumnName("id");
        builder.Property(f => f.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(f => f.Nombre).HasColumnName("nombre").HasMaxLength(FormaPago.LongitudMaximaNombre).IsRequired();
        builder.Property(f => f.GeneraVencimiento).HasColumnName("genera_vencimiento").IsRequired();
        builder.Property(f => f.DiasVencimiento).HasColumnName("dias_vencimiento").IsRequired();
        builder.Property(f => f.RegistrarPagoAutomatico).HasColumnName("registrar_pago_automatico").IsRequired();
        builder.Property(f => f.Activo).HasColumnName("activo").IsRequired();
        builder.HasIndex(f => f.EmpresaId).HasDatabaseName("ix_forma_pago_empresa");
        builder.Ignore(f => f.EventosDominio);
    }
}
