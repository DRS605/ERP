using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using AlxorCore.Informes.Aplicacion;
using AlxorCore.Informes.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Informes.Infraestructura;

/// <summary>Contexto fiscal: certificado de la empresa, envíos al SII y situación de cada factura.</summary>
public sealed class FiscalDbContext : DbContextEmpresaBase
{
    public const string Esquema = "fiscal";

    public FiscalDbContext(DbContextOptions<FiscalDbContext> opciones, IPublicadorEventos publicador, IContextoEmpresa contexto)
        : base(opciones, publicador, contexto)
    {
    }

    public DbSet<CertificadoSii> Certificados => Set<CertificadoSii>();

    public DbSet<EnvioSii> Envios => Set<EnvioSii>();

    public DbSet<RegistroSii> Registros => Set<RegistroSii>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Esquema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FiscalDbContext).Assembly);
        AplicarFiltroMultiempresa(modelBuilder);
    }
}

internal sealed class ConfiguracionCertificadoSii : IEntityTypeConfiguration<CertificadoSii>
{
    public void Configure(EntityTypeBuilder<CertificadoSii> b)
    {
        b.ToTable("certificado");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(c => c.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(c => c.PfxCifrado).HasColumnName("pfx_cifrado").IsRequired();
        b.Property(c => c.ClaveCifrada).HasColumnName("clave_cifrada").IsRequired();
        b.Property(c => c.Titular).HasColumnName("titular").HasMaxLength(300).IsRequired();
        b.Property(c => c.Nif).HasColumnName("nif").HasMaxLength(20);
        b.Property(c => c.CaducaEn).HasColumnName("caduca_en").IsRequired();
        b.Property(c => c.Entorno).HasColumnName("entorno").HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(c => c.ActualizadoEn).HasColumnName("actualizado_en").IsRequired();
        b.HasIndex(c => c.EmpresaId).IsUnique().HasDatabaseName("ux_certificado_empresa");
        b.Ignore(c => c.EventosDominio);
    }
}

internal sealed class ConfiguracionEnvioSii : IEntityTypeConfiguration<EnvioSii>
{
    public void Configure(EntityTypeBuilder<EnvioSii> b)
    {
        b.ToTable("envio_sii");
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(e => e.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(e => e.Libro).HasColumnName("libro").HasMaxLength(20).IsRequired();
        b.Property(e => e.Ejercicio).HasColumnName("ejercicio").IsRequired();
        b.Property(e => e.Periodo).HasColumnName("periodo").IsRequired();
        b.Property(e => e.TipoComunicacion).HasColumnName("tipo_comunicacion").HasMaxLength(4).IsRequired();
        b.Property(e => e.Entorno).HasColumnName("entorno").HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(e => e.Registros).HasColumnName("registros").IsRequired();
        b.Property(e => e.Correctos).HasColumnName("correctos").IsRequired();
        b.Property(e => e.ConErrores).HasColumnName("con_errores").IsRequired();
        b.Property(e => e.Incorrectos).HasColumnName("incorrectos").IsRequired();
        b.Property(e => e.Estado).HasColumnName("estado").HasConversion<string>().HasMaxLength(30).IsRequired();
        b.Property(e => e.Csv).HasColumnName("csv").HasMaxLength(40);
        b.Property(e => e.Error).HasColumnName("error").HasMaxLength(2000);
        b.Property(e => e.Peticion).HasColumnName("peticion").IsRequired();
        b.Property(e => e.Respuesta).HasColumnName("respuesta");
        b.Property(e => e.EnviadoEn).HasColumnName("enviado_en").IsRequired();
        b.HasIndex(e => new { e.EmpresaId, e.EnviadoEn }).HasDatabaseName("ix_envio_sii_empresa_fecha");
        b.Ignore(e => e.EventosDominio);
    }
}

internal sealed class ConfiguracionRegistroSii : IEntityTypeConfiguration<RegistroSii>
{
    public void Configure(EntityTypeBuilder<RegistroSii> b)
    {
        b.ToTable("registro_sii");
        b.HasKey(r => r.Id);
        b.Property(r => r.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(r => r.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(r => r.Libro).HasColumnName("libro").HasMaxLength(20).IsRequired();
        b.Property(r => r.DocumentoId).HasColumnName("documento_id").IsRequired();
        b.Property(r => r.Numero).HasColumnName("numero").HasMaxLength(60).IsRequired();
        b.Property(r => r.FechaExpedicion).HasColumnName("fecha_expedicion").IsRequired();
        b.Property(r => r.Ejercicio).HasColumnName("ejercicio").IsRequired();
        b.Property(r => r.Periodo).HasColumnName("periodo").IsRequired();
        b.Property(r => r.Estado).HasColumnName("estado").HasConversion<string>().HasMaxLength(30).IsRequired();
        b.Property(r => r.Aceptado).HasColumnName("aceptado").IsRequired();
        b.Property(r => r.CodigoError).HasColumnName("codigo_error").HasMaxLength(10);
        b.Property(r => r.DescripcionError).HasColumnName("descripcion_error").HasMaxLength(500);
        b.Property(r => r.Csv).HasColumnName("csv").HasMaxLength(40);
        b.Property(r => r.Huella).HasColumnName("huella").HasMaxLength(64).IsRequired();
        b.Property(r => r.UltimoEnvioId).HasColumnName("ultimo_envio_id");
        b.Property(r => r.EnviadoEn).HasColumnName("enviado_en");
        b.HasIndex(r => new { r.EmpresaId, r.Libro, r.DocumentoId }).IsUnique().HasDatabaseName("ux_registro_sii_documento");
        b.HasIndex(r => new { r.EmpresaId, r.Libro, r.Ejercicio, r.Periodo }).HasDatabaseName("ix_registro_sii_periodo");
        b.Ignore(r => r.EventosDominio);
    }
}

internal sealed class RepositorioSii : IRepositorioSii
{
    private readonly FiscalDbContext _db;

    public RepositorioSii(FiscalDbContext db) => _db = db;

    public Task<CertificadoSii?> CertificadoAsync(Guid empresaId, CancellationToken ct = default) =>
        _db.Certificados.FirstOrDefaultAsync(c => c.EmpresaId == empresaId, ct);

    public void Agregar(CertificadoSii certificado) => _db.Certificados.Add(certificado);

    public void Eliminar(CertificadoSii certificado) => _db.Certificados.Remove(certificado);

    public async Task<IReadOnlyList<RegistroSii>> RegistrosAsync(Guid empresaId, string libro, int ejercicio, int periodo, CancellationToken ct = default) =>
        await _db.Registros.Where(r => r.EmpresaId == empresaId && r.Libro == libro && r.Ejercicio == ejercicio && r.Periodo == periodo)
            .ToListAsync(ct).ConfigureAwait(false);

    public void Agregar(RegistroSii registro) => _db.Registros.Add(registro);

    public void Agregar(EnvioSii envio) => _db.Envios.Add(envio);

    public async Task<IReadOnlyList<EnvioSii>> EnviosAsync(Guid empresaId, int limite, CancellationToken ct = default) =>
        await _db.Envios.AsNoTracking().Where(e => e.EmpresaId == empresaId).OrderByDescending(e => e.EnviadoEn).Take(limite)
            .ToListAsync(ct).ConfigureAwait(false);

    public Task<EnvioSii?> EnvioAsync(Guid id, CancellationToken ct = default) => _db.Envios.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task GuardarAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}

/// <summary>
/// Llamada SOAP 1.1 al servicio web del SII con autenticación TLS de cliente (el certificado de la empresa). Un error
/// de red, de TLS o un HTTP distinto de 200 sin cuerpo SOAP se devuelven como error de comunicación.
/// </summary>
internal sealed class TransporteSiiHttp : ITransporteSii
{
    private static readonly TimeSpan Tiempo = TimeSpan.FromSeconds(120);

    public async Task<RespuestaTransporteSii> EnviarAsync(Uri destino, TipoLibroSii libro, EntornoSii entorno, X509Certificate2 certificado, string sobreSoap,
        CancellationToken ct = default)
    {
        using var manejador = new HttpClientHandler { ClientCertificateOptions = ClientCertificateOption.Manual };
        manejador.ClientCertificates.Add(certificado);
        using var cliente = new HttpClient(manejador) { Timeout = Tiempo };
        using var peticion = new HttpRequestMessage(HttpMethod.Post, destino)
        {
            Content = new StringContent(sobreSoap, Encoding.UTF8, "text/xml"),
        };
        peticion.Headers.Add("SOAPAction", "\"\"");
        peticion.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
        try
        {
            using var respuesta = await cliente.SendAsync(peticion, ct).ConfigureAwait(false);
            var cuerpo = await respuesta.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            // Un Fault SOAP llega con HTTP 500 y cuerpo: se interpreta arriba. Sin cuerpo XML es un error de comunicación.
            if (respuesta.StatusCode != HttpStatusCode.OK && !cuerpo.TrimStart().StartsWith('<'))
            {
                return new RespuestaTransporteSii((int)respuesta.StatusCode, cuerpo, $"{destino.Host} respondió HTTP {(int)respuesta.StatusCode}.");
            }

            return new RespuestaTransporteSii((int)respuesta.StatusCode, cuerpo, null);
        }
        catch (HttpRequestException ex)
        {
            return new RespuestaTransporteSii(null, null, $"No se pudo conectar con {destino.Host}: " + ex.Message);
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            return new RespuestaTransporteSii(null, null, $"{destino.Host} no respondió a tiempo.");
        }
    }
}

/// <summary>Fábrica en tiempo de diseño (dotnet ef migrations).</summary>
public sealed class FiscalDbContextFactory : Microsoft.EntityFrameworkCore.Design.IDesignTimeDbContextFactory<FiscalDbContext>
{
    public FiscalDbContext CreateDbContext(string[] args)
    {
        var conexion = Environment.GetEnvironmentVariable("ALXOR_MIGRACIONES_CONEXION")
            ?? "Host=localhost;Port=5432;Database=alxor;Username=postgres;Password=postgres";
        var opciones = new DbContextOptionsBuilder<FiscalDbContext>().UseNpgsql(conexion).Options;
        return new FiscalDbContext(opciones, new PublicadorInactivo(), new ContextoVacio());
    }

    private sealed class PublicadorInactivo : IPublicadorEventos
    {
        public Task PublicarAsync(IReadOnlyCollection<AlxorCore.Nucleo.Dominio.IEventoDominio> eventos, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class ContextoVacio : IContextoEmpresa
    {
        public Guid? EmpresaId => null;
    }
}

internal sealed class ConfiguracionFichaPlastico : IEntityTypeConfiguration<FichaPlastico>
{
    public void Configure(EntityTypeBuilder<FichaPlastico> b)
    {
        b.ToTable("ficha_plastico");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(x => x.ProductoId).HasColumnName("producto_id").IsRequired();
        b.Property(x => x.Clave).HasColumnName("clave").HasMaxLength(20).HasConversion<string>().IsRequired();
        b.Property(x => x.KgPorUnidad).HasColumnName("kg_por_unidad").HasColumnType("numeric(14,6)").IsRequired();
        b.Property(x => x.KgRecicladoPorUnidad).HasColumnName("kg_reciclado_por_unidad").HasColumnType("numeric(14,6)").IsRequired();
        b.Property(x => x.Exento).HasColumnName("exento").IsRequired();
        b.Property(x => x.MotivoExencion).HasColumnName("motivo_exencion").HasMaxLength(200);
        b.Ignore(x => x.KgNoRecicladoPorUnidad);
        b.HasIndex(x => new { x.EmpresaId, x.ProductoId }).IsUnique().HasDatabaseName("ux_ficha_plastico_producto");
        b.Ignore(x => x.EventosDominio);
    }
}

internal sealed class RepositorioFichasPlastico : IRepositorioFichasPlastico
{
    private readonly FiscalDbContext _ctx;

    public RepositorioFichasPlastico(FiscalDbContext ctx) => _ctx = ctx;

    public void Agregar(FichaPlastico ficha) => _ctx.Add(ficha);

    public void Eliminar(FichaPlastico ficha) => _ctx.Remove(ficha);

    public async Task<IReadOnlyList<FichaPlastico>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<FichaPlastico>().Where(f => f.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<FichaPlastico?> DeProductoAsync(Guid empresaId, Guid productoId, CancellationToken ct = default) =>
        _ctx.Set<FichaPlastico>().FirstOrDefaultAsync(f => f.EmpresaId == empresaId && f.ProductoId == productoId, ct);

    public Task GuardarAsync(CancellationToken ct = default) => _ctx.GuardarCambiosAsync(ct);
}
