using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using AlxorCore.Api.Comun;
using AlxorCore.Api.Endpoints;
using AlxorCore.Identidad.Infraestructura;
using AlxorCore.Identidad.Infraestructura.Persistencia;
using AlxorCore.Identidad.Infraestructura.Seguridad;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Organizacion.Infraestructura;
using AlxorCore.Terceros.Infraestructura;
using AlxorCore.Catalogo.Infraestructura;
using AlxorCore.Facturacion.Infraestructura;
using AlxorCore.Gastos.Infraestructura;
using AlxorCore.Recepcion.Infraestructura;
using AlxorCore.Contabilidad.Infraestructura;
using AlxorCore.Compras.Infraestructura;
using AlxorCore.Inventario.Infraestructura;
using AlxorCore.Produccion.Infraestructura;
using AlxorCore.Personal.Infraestructura;
using AlxorCore.Proyectos.Infraestructura;
using AlxorCore.Tesoreria.Infraestructura;
using AlxorCore.Documentos.Infraestructura;
using AlxorCore.Informes.Infraestructura;
using AlxorCore.Auditoria.Infraestructura;
using AlxorCore.Divisas.Infraestructura;
using AlxorCore.Aprobaciones.Infraestructura;
using AlxorCore.Integraciones.Infraestructura;
using AlxorCore.Organizacion.Infraestructura.Persistencia;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- Contexto de empresa (multiempresa) ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ContextoEmpresaHttp>();
builder.Services.AddScoped<IContextoEmpresa>(sp => sp.GetRequiredService<ContextoEmpresaHttp>());
builder.Services.AddScoped<IContextoEmpresaMutable>(sp => sp.GetRequiredService<ContextoEmpresaHttp>());

// --- Módulos de ALXOR Core ---
builder.Services.AgregarModuloIdentidad(builder.Configuration);
builder.Services.AgregarModuloOrganizacion(builder.Configuration);
builder.Services.AgregarModuloTerceros(builder.Configuration);
builder.Services.AgregarModuloCatalogo(builder.Configuration);
builder.Services.AgregarModuloFacturacion(builder.Configuration);
builder.Services.AgregarModuloGastos(builder.Configuration);
builder.Services.AgregarModuloRecepcion(builder.Configuration);
// Contabilidad va DESPUÉS de Recepción: sustituye su IContabilizador por el que decide según modo.
builder.Services.AgregarModuloContabilidad(builder.Configuration);
builder.Services.AgregarModuloCompras(builder.Configuration);
builder.Services.AgregarModuloInventario(builder.Configuration);
builder.Services.AgregarModuloProduccion(builder.Configuration);
builder.Services.AgregarModuloPersonal(builder.Configuration);
builder.Services.AgregarModuloProyectos(builder.Configuration);
builder.Services.AgregarModuloTesoreria(builder.Configuration);
builder.Services.AgregarModuloDocumentos();
builder.Services.AgregarModuloInformes();
builder.Services.AgregarModuloAuditoria(builder.Configuration);
builder.Services.AgregarModuloDivisas(builder.Configuration);
builder.Services.AgregarModuloAprobaciones(builder.Configuration);
builder.Services.AgregarModuloIntegraciones(builder.Configuration);

// El publicador de eventos que alimenta las integraciones (API/webhooks) reemplaza al provisional
// que solo registraba en el log; debe registrarse tras los módulos para ser el resuelto.
builder.Services.AddScoped<AlxorCore.Nucleo.Aplicacion.IPublicadorEventos, AlxorCore.Api.Comun.PublicadorEventosIntegraciones>();

// --- Entrega de webhooks (proceso en segundo plano) ---
builder.Services.Configure<AlxorCore.Api.Servicios.OpcionesWebhooks>(
    builder.Configuration.GetSection(AlxorCore.Api.Servicios.OpcionesWebhooks.Seccion));
builder.Services.AddHostedService<AlxorCore.Api.Servicios.ServicioWebhooks>();

// --- Facturación automática periódica (proceso en segundo plano) ---
builder.Services.Configure<AlxorCore.Api.Servicios.OpcionesFacturacionRecurrente>(
    builder.Configuration.GetSection(AlxorCore.Api.Servicios.OpcionesFacturacionRecurrente.Seccion));
builder.Services.AddHostedService<AlxorCore.Api.Servicios.ServicioFacturacionRecurrente>();

// --- Buzón de correo de facturas de proveedor (apagado salvo que esté configurado) ---
builder.Services.AddHostedService<AlxorCore.Api.Servicios.ServicioBuzonProveedores>();

// Los enumerados se serializan por nombre en la API.
builder.Services.ConfigureHttpJsonOptions(opciones =>
    opciones.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// --- Autenticación JWT ---
// Conservamos los nombres originales de los claims (sub, email) sin remapearlos.
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

// La validación se configura desde las MISMAS opciones (IOptions<OpcionesJwt>) que usa la
// emisión de tokens, garantizando una única fuente de verdad para la clave, el emisor y la
// audiencia (evita desajustes de clave entre firma y validación).
builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<OpcionesJwt>>((jwt, opcionesJwt) =>
    {
        jwt.MapInboundClaims = false;
        jwt.TokenValidationParameters = ConfiguracionJwt.ConstruirParametrosValidacion(opcionesJwt.Value);
    });

builder.Services.AddAuthorization();

// --- Seguridad: limitación de peticiones (rate limiting) en autenticación ---
builder.Services.Configure<AlxorCore.Api.Comun.OpcionesSeguridad>(
    builder.Configuration.GetSection(AlxorCore.Api.Comun.OpcionesSeguridad.Seccion));
builder.Services.AddRateLimiter(opciones =>
{
    opciones.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    opciones.AddPolicy(AlxorCore.Api.Comun.OpcionesSeguridad.PoliticaAuth, contexto =>
    {
        // El cupo se lee de la configuración (IOptions) en cada petición, no al arrancar.
        var seg = contexto.RequestServices
            .GetRequiredService<Microsoft.Extensions.Options.IOptions<AlxorCore.Api.Comun.OpcionesSeguridad>>().Value;
        // Partición por IP del cliente: cada origen tiene su propio cupo (frena la fuerza bruta).
        var clave = contexto.Connection.RemoteIpAddress?.ToString() ?? "desconocida";
        return System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(clave, _ =>
            new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit = seg.RateLimitPeticiones,
                Window = TimeSpan.FromSeconds(seg.RateLimitVentanaSegundos),
                QueueLimit = 0,
            });
    });
});

// --- Operación: health checks (liveness/readiness) ---
builder.Services.AddHealthChecks()
    .AddCheck<AlxorCore.Api.Comun.ComprobacionBaseDatos>("base_datos", tags: ["listo"]);

// --- OpenAPI (API First) ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ALXOR Core API",
        Version = "v1",
        Description = "API del núcleo ALXOR Core. Módulo Identidad.",
    });

    var esquemaJwt = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduce el token JWT (sin el prefijo 'Bearer').",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
    };

    opciones.AddSecurityDefinition("Bearer", esquemaJwt);
    opciones.AddSecurityRequirement(new OpenApiSecurityRequirement { [esquemaJwt] = Array.Empty<string>() });
});

builder.Services.AddProblemDetails();

var app = builder.Build();

// En desarrollo aplicamos las migraciones automáticamente para facilitar el arranque.
if (app.Environment.IsDevelopment())
{
    using var ambito = app.Services.CreateScope();
    await ambito.ServiceProvider.GetRequiredService<IdentidadDbContext>().Database.MigrateAsync().ConfigureAwait(false);
    await ambito.ServiceProvider.GetRequiredService<OrganizacionDbContext>().Database.MigrateAsync().ConfigureAwait(false);
    await ambito.ServiceProvider.GetRequiredService<AlxorCore.Terceros.Infraestructura.TercerosDbContext>().Database.MigrateAsync().ConfigureAwait(false);
    await ambito.ServiceProvider.GetRequiredService<AlxorCore.Catalogo.Infraestructura.CatalogoDbContext>().Database.MigrateAsync().ConfigureAwait(false);
    await ambito.ServiceProvider.GetRequiredService<AlxorCore.Facturacion.Infraestructura.FacturacionDbContext>().Database.MigrateAsync().ConfigureAwait(false);
    await ambito.ServiceProvider.GetRequiredService<AlxorCore.Gastos.Infraestructura.GastosDbContext>().Database.MigrateAsync().ConfigureAwait(false);
    await ambito.ServiceProvider.GetRequiredService<AlxorCore.Recepcion.Infraestructura.RecepcionDbContext>().Database.MigrateAsync().ConfigureAwait(false);
    await ambito.ServiceProvider.GetRequiredService<AlxorCore.Contabilidad.Infraestructura.ContabilidadDbContext>().Database.MigrateAsync().ConfigureAwait(false);
    await ambito.ServiceProvider.GetRequiredService<AlxorCore.Compras.Infraestructura.ComprasDbContext>().Database.MigrateAsync().ConfigureAwait(false);
    await ambito.ServiceProvider.GetRequiredService<AlxorCore.Inventario.Infraestructura.InventarioDbContext>().Database.MigrateAsync().ConfigureAwait(false);
    await ambito.ServiceProvider.GetRequiredService<ProduccionDbContext>().Database.MigrateAsync().ConfigureAwait(false);
    await ambito.ServiceProvider.GetRequiredService<PersonalDbContext>().Database.MigrateAsync().ConfigureAwait(false);
    await ambito.ServiceProvider.GetRequiredService<ProyectosDbContext>().Database.MigrateAsync().ConfigureAwait(false);
    await ambito.ServiceProvider.GetRequiredService<AlxorCore.Tesoreria.Infraestructura.TesoreriaDbContext>().Database.MigrateAsync().ConfigureAwait(false);
    await ambito.ServiceProvider.GetRequiredService<AlxorCore.Auditoria.Infraestructura.AuditoriaDbContext>().Database.MigrateAsync().ConfigureAwait(false);
    await ambito.ServiceProvider.GetRequiredService<AlxorCore.Divisas.Infraestructura.DivisasDbContext>().Database.MigrateAsync().ConfigureAwait(false);
    await ambito.ServiceProvider.GetRequiredService<AlxorCore.Aprobaciones.Infraestructura.AprobacionesDbContext>().Database.MigrateAsync().ConfigureAwait(false);
    await ambito.ServiceProvider.GetRequiredService<AlxorCore.Integraciones.Infraestructura.IntegracionesDbContext>().Database.MigrateAsync().ConfigureAwait(false);

    app.UseSwagger();
    app.UseSwaggerUI();
}

// Identificador de correlación y cabeceras de seguridad: lo antes posible en el pipeline.
app.UseMiddleware<AlxorCore.Api.Comun.MiddlewareCorrelacion>();
app.UseMiddleware<AlxorCore.Api.Comun.MiddlewareCabecerasSeguridad>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// Sirve la interfaz web (SPA) desde wwwroot, en el mismo origen que la API (sin CORS).
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Salud: liveness (sin comprobaciones) y readiness (comprueba la base de datos).
app.MapHealthChecks("/salud/vivo", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false,
}).AllowAnonymous();
app.MapHealthChecks("/salud/listo", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = comprobacion => comprobacion.Tags.Contains("listo"),
}).AllowAnonymous();

// Auditoría: registra las operaciones que modifican datos (tras autenticar, para conocer al autor).
app.UseMiddleware<AlxorCore.Api.Comun.MiddlewareAuditoria>();

app.MapGet("/salud", () => Results.Ok(new { estado = "ok" }))
    .WithTags("Salud")
    .WithName("Salud")
    .AllowAnonymous();

app.MapearIdentidad();
app.MapearOrganizacion();
app.MapearUsuarios();
app.MapearTerceros();
app.MapearActividades();
app.MapearCatalogo();
app.MapearTiposIva();
app.MapearFacturacion();
app.MapearVentas();
app.MapearCartasPorte();
app.MapearGastos();
app.MapearRecepcion();
app.MapearContabilidad();
app.MapearInmovilizado();
app.MapearCompras();
app.MapearInventario();
app.MapearProduccion();
app.MapearPersonal();
app.MapearProyectos();
app.MapearTesoreria();
app.MapearDocumentos();
app.MapearInformes();
app.MapearAuditoria();
app.MapearDivisas();
app.MapearAprobaciones();
app.MapearIntegraciones();
app.MapearCuenta();
app.MapearImportacion();

// La nueva interfaz (SPA React) se sirve bajo /app con enrutado en el cliente: cualquier ruta
// /app/... que no sea un fichero devuelve su index.html. Debe ir antes del fallback general.
app.MapFallbackToFile("/app/{*rest}", "app/index.html");

// Cualquier otra ruta no-API devuelve la interfaz clásica (enrutado en el cliente).
app.MapFallbackToFile("index.html");

await app.RunAsync().ConfigureAwait(false);

/// <summary>Punto de entrada expuesto para las pruebas de integración (WebApplicationFactory).</summary>
public partial class Program;
