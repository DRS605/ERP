using AlxorCore.Identidad.Infraestructura.Persistencia;
using AlxorCore.Organizacion.Infraestructura.Persistencia;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Fábrica de la API para pruebas de integración: arranca el host real contra una base de datos
/// PostgreSQL real (por defecto <c>alxor_test</c> en localhost), aplicando las migraciones y
/// dejando la tabla de usuarios vacía antes de la batería de pruebas.
/// La cadena de conexión puede sobrescribirse con la variable <c>ALXOR_TEST_CONEXION</c>.
/// </summary>
public sealed class FabricaApiPruebas : WebApplicationFactory<Program>, IAsyncLifetime
{
    private static readonly string CadenaConexion =
        Environment.GetEnvironmentVariable("ALXOR_TEST_CONEXION")
        ?? "Host=localhost;Port=5432;Database=alxor_test;Username=postgres;Password=postgres";

    static FabricaApiPruebas()
    {
        // El host lee la cadena de conexión de forma anticipada al registrar los módulos
        // (RegistroServicios.GetConnectionString), antes de que se aplique la configuración
        // en memoria de la fábrica. La fuente de variables de entorno de WebApplication.CreateBuilder
        // sí tiene prioridad sobre appsettings.json, así que fijamos aquí la base de pruebas para
        // que ningún test toque la base de datos de desarrollo.
        Environment.SetEnvironmentVariable("ConnectionStrings__AlxorCore", CadenaConexion);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuracion) =>
        {
            configuracion.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:AlxorCore"] = CadenaConexion,
                ["Jwt:Emisor"] = "alxor-core",
                ["Jwt:Audiencia"] = "alxor-core",
                ["Jwt:ClaveSecreta"] = "clave-de-pruebas-de-integracion-con-mas-de-32-caracteres",
                ["Jwt:MinutosExpiracion"] = "60",
                // El proceso automático se prueba de forma determinista con /procesar; se apaga aquí.
                ["FacturacionRecurrente:Activo"] = "false",
            });
        });
    }

    public async Task InitializeAsync()
    {
        await AsegurarBaseDatosAsync().ConfigureAwait(false);

        using var ambito = Services.CreateScope();
        var identidad = ambito.ServiceProvider.GetRequiredService<IdentidadDbContext>();
        var organizacion = ambito.ServiceProvider.GetRequiredService<OrganizacionDbContext>();
        var terceros = ambito.ServiceProvider.GetRequiredService<AlxorCore.Terceros.Infraestructura.TercerosDbContext>();
        var catalogo = ambito.ServiceProvider.GetRequiredService<AlxorCore.Catalogo.Infraestructura.CatalogoDbContext>();
        var facturacion = ambito.ServiceProvider.GetRequiredService<AlxorCore.Facturacion.Infraestructura.FacturacionDbContext>();
        var gastos = ambito.ServiceProvider.GetRequiredService<AlxorCore.Gastos.Infraestructura.GastosDbContext>();
        var recepcion = ambito.ServiceProvider.GetRequiredService<AlxorCore.Recepcion.Infraestructura.RecepcionDbContext>();
        var contabilidad = ambito.ServiceProvider.GetRequiredService<AlxorCore.Contabilidad.Infraestructura.ContabilidadDbContext>();
        var compras = ambito.ServiceProvider.GetRequiredService<AlxorCore.Compras.Infraestructura.ComprasDbContext>();
        var inventario = ambito.ServiceProvider.GetRequiredService<AlxorCore.Inventario.Infraestructura.InventarioDbContext>();
        var produccion = ambito.ServiceProvider.GetRequiredService<AlxorCore.Produccion.Infraestructura.ProduccionDbContext>();
        var personal = ambito.ServiceProvider.GetRequiredService<AlxorCore.Personal.Infraestructura.PersonalDbContext>();
        var proyectos = ambito.ServiceProvider.GetRequiredService<AlxorCore.Proyectos.Infraestructura.ProyectosDbContext>();
        var tesoreria = ambito.ServiceProvider.GetRequiredService<AlxorCore.Tesoreria.Infraestructura.TesoreriaDbContext>();
        var auditoria = ambito.ServiceProvider.GetRequiredService<AlxorCore.Auditoria.Infraestructura.AuditoriaDbContext>();
        var divisas = ambito.ServiceProvider.GetRequiredService<AlxorCore.Divisas.Infraestructura.DivisasDbContext>();
        var aprobaciones = ambito.ServiceProvider.GetRequiredService<AlxorCore.Aprobaciones.Infraestructura.AprobacionesDbContext>();

        await identidad.Database.MigrateAsync().ConfigureAwait(false);
        await organizacion.Database.MigrateAsync().ConfigureAwait(false);
        await terceros.Database.MigrateAsync().ConfigureAwait(false);
        await catalogo.Database.MigrateAsync().ConfigureAwait(false);
        await facturacion.Database.MigrateAsync().ConfigureAwait(false);
        await gastos.Database.MigrateAsync().ConfigureAwait(false);
        await recepcion.Database.MigrateAsync().ConfigureAwait(false);
        await contabilidad.Database.MigrateAsync().ConfigureAwait(false);
        await compras.Database.MigrateAsync().ConfigureAwait(false);
        await inventario.Database.MigrateAsync().ConfigureAwait(false);
        await produccion.Database.MigrateAsync().ConfigureAwait(false);
        await personal.Database.MigrateAsync().ConfigureAwait(false);
        await proyectos.Database.MigrateAsync().ConfigureAwait(false);
        await tesoreria.Database.MigrateAsync().ConfigureAwait(false);
        await auditoria.Database.MigrateAsync().ConfigureAwait(false);
        await divisas.Database.MigrateAsync().ConfigureAwait(false);
        await aprobaciones.Database.MigrateAsync().ConfigureAwait(false);

        await LimpiarBaseDatosAsync(identidad).ConfigureAwait(false);
    }

    /// <summary>
    /// Vacía todas las tablas de negocio de todos los esquemas de la aplicación de forma <b>dinámica</b>
    /// (descubriéndolas de <c>pg_tables</c>), excluyendo las tablas de historial de migraciones
    /// (<c>__…</c>). Así no hay que mantener una lista a mano: cualquier tabla nueva se limpia sola.
    /// <c>CASCADE</c> resuelve el orden por claves foráneas y <c>RESTART IDENTITY</c> reinicia las secuencias.
    /// </summary>
    public static async Task LimpiarBaseDatosAsync(DbContext contexto)
    {
        ArgumentNullException.ThrowIfNull(contexto);
        await contexto.Database.ExecuteSqlRawAsync("""
            DO $$
            DECLARE sentencia text;
            BEGIN
                SELECT 'TRUNCATE ' || string_agg(format('%I.%I', schemaname, tablename), ', ') || ' RESTART IDENTITY CASCADE'
                INTO sentencia
                FROM pg_tables
                WHERE schemaname NOT IN ('pg_catalog', 'information_schema', 'public')
                  AND tablename NOT LIKE '\_\_%';
                IF sentencia IS NOT NULL THEN
                    EXECUTE sentencia;
                END IF;
            END $$;
            """).ConfigureAwait(false);
    }

    public new async Task DisposeAsync() => await base.DisposeAsync().ConfigureAwait(false);

    private static async Task AsegurarBaseDatosAsync()
    {
        var constructor = new NpgsqlConnectionStringBuilder(CadenaConexion);
        var nombreBd = constructor.Database;
        constructor.Database = "postgres";

        await using var conexion = new NpgsqlConnection(constructor.ConnectionString);
        await conexion.OpenAsync().ConfigureAwait(false);

        await using var comprobar = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @n", conexion);
        comprobar.Parameters.AddWithValue("n", nombreBd!);
        var existe = await comprobar.ExecuteScalarAsync().ConfigureAwait(false);

        if (existe is null)
        {
            await using var crear = new NpgsqlCommand($"CREATE DATABASE \"{nombreBd}\"", conexion);
            await crear.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}
