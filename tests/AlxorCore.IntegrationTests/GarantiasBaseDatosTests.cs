using System.Net;
using System.Net.Http.Json;
using AlxorCore.Persistencia;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Garantías que impone la propia base de datos, comprobadas <b>saltándose la aplicación</b>: se
/// conecta directamente con el rol de la aplicación (sin privilegios, como en producción) y se intenta
/// lo que la aplicación nunca debería hacer. Además, controles sobre todo el esquema para que las
/// tablas nuevas no se olviden de ellas.
/// </summary>
public sealed class GarantiasBaseDatosTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public GarantiasBaseDatosTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record FacturaResp(Guid Id, string NumeroCompleto, decimal Total)
    {
        public long Numero => long.Parse(NumeroCompleto[(NumeroCompleto.IndexOf('/', StringComparison.Ordinal) + 1)..], System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Tablas sin RLS a propósito: se consultan antes de que exista una empresa activa (inicio de
    /// sesión, elección de empresa, autenticación por clave de API). Cualquier otra tabla debe tenerla.
    /// </summary>
    private static readonly Dictionary<string, string> SinRlsJustificadas = new()
    {
        ["identidad.usuario"] = "los usuarios son globales: un usuario puede trabajar en varias empresas",
        ["organizacion.grupo"] = "se lee al elegir empresa, antes de fijar el grupo activo",
        ["organizacion.empresa"] = "se lee al iniciar sesión, filtrada por las membresías del usuario",
        ["organizacion.membresia"] = "es la que dice a qué empresas puede entrar el usuario",
        ["organizacion.rol_empresa"] = "da los permisos del rol propio al elegir empresa, junto con la membresía, antes de fijar la empresa activa",
        ["integraciones.clave_api"] = "se busca por la clave para autenticar la petición, antes de conocer la empresa",
    };

    // ---------------------------------------------------------------- controles sobre el esquema

    [Fact]
    public async Task El_rol_de_la_aplicacion_no_puede_saltarse_la_RLS()
    {
        await using var c = await ConexionAppAsync(null);
        await using var cmd = new NpgsqlCommand("SELECT rolsuper, rolbypassrls FROM pg_roles WHERE rolname = current_user", c);
        await using var r = await cmd.ExecuteReaderAsync();
        (await r.ReadAsync()).Should().BeTrue();
        r.GetBoolean(0).Should().BeFalse("la aplicación no debe conectarse como superusuario");
        r.GetBoolean(1).Should().BeFalse("la aplicación no debe tener BYPASSRLS");
    }

    [Fact]
    public async Task Toda_tabla_de_negocio_tiene_la_RLS_forzada_y_con_politica()
    {
        var sinRls = await ListaAdminAsync("""
            SELECT n.nspname || '.' || c.relname
              FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
             WHERE c.relkind = 'r' AND n.nspname NOT IN ('pg_catalog', 'information_schema', 'public')
               AND c.relname NOT LIKE '\_\_%'
               AND (NOT c.relrowsecurity OR NOT c.relforcerowsecurity
                    OR NOT EXISTS (SELECT 1 FROM pg_policy p WHERE p.polrelid = c.oid))
             ORDER BY 1
            """);

        sinRls.Except(SinRlsJustificadas.Keys).Should().BeEmpty(
            "toda tabla con datos de una empresa o grupo necesita RLS (RlsSql.Activar/ActivarPorGrupo o GarantiasSql.RlsPorPadre en su migración)");
        SinRlsJustificadas.Keys.Except(sinRls).Should().BeEmpty("si una tabla justificada ya tiene RLS, quítala de la lista");
    }

    [Fact]
    public async Task Toda_clave_foranea_tiene_indice_y_ninguna_esta_duplicada()
    {
        var sinIndice = await ListaAdminAsync("""
            SELECT c.conrelid::regclass || ' (' || c.conname || ')'
              FROM pg_constraint c
             WHERE c.contype = 'f'
               AND NOT EXISTS (
                   SELECT 1 FROM pg_index i
                    WHERE i.indrelid = c.conrelid
                      AND (i.indkey::int2[])[0:array_length(c.conkey, 1) - 1] @> c.conkey
                      AND (i.indkey::int2[])[0:array_length(c.conkey, 1) - 1] <@ c.conkey)
             ORDER BY 1
            """);
        sinIndice.Should().BeEmpty("sin índice, borrar o actualizar la tabla referenciada recorre entera la que referencia");

        var duplicadas = await ListaAdminAsync("""
            SELECT conrelid::regclass || ' → ' || confrelid::regclass
              FROM pg_constraint WHERE contype = 'f'
             GROUP BY conrelid, conkey, confrelid, confkey HAVING count(*) > 1
            """);
        duplicadas.Should().BeEmpty();
    }

    // ---------------------------------------------------------------- aislamiento entre empresas

    [Fact]
    public async Task Una_empresa_no_ve_ni_escribe_datos_de_otra_aunque_consulte_la_base_directamente()
    {
        var (a, empresaA) = await Ayudas.ConEmpresaAsync(_fabrica);
        var (b, empresaB) = await Ayudas.ConEmpresaAsync(_fabrica);
        var facturaA = await FacturaAsync(a);
        await FacturaAsync(b);
        await CobrarAsync(a, facturaA);
        await CobrarAsync(b, await FacturaAsync(b));
        await AsientoAsync(a);
        await AsientoAsync(b);

        await using var c = await ConexionAppAsync(empresaA);
        (await EscalarAsync<long>(c, "SELECT count(*) FROM facturacion.factura WHERE empresa_id <> @a", empresaA)).Should().Be(0);
        (await EscalarAsync<long>(c, "SELECT count(*) FROM facturacion.factura")).Should().Be(1);
        (await EscalarAsync<long>(c, "SELECT count(*) FROM facturacion.linea_factura")).Should().Be(1);
        await using (var admin = new NpgsqlConnection(FabricaApiPruebas.CadenaAdmin))
        {
            await admin.OpenAsync();
            var apuntesDeA = await EscalarAsync<long>(admin,
                "SELECT count(*) FROM contabilidad.apunte p JOIN contabilidad.asiento a ON a.id = p.asiento_id WHERE a.empresa_id = @a", empresaA);
            apuntesDeA.Should().BeGreaterThan(0);
            (await EscalarAsync<long>(c, "SELECT count(*) FROM contabilidad.apunte")).Should().Be(apuntesDeA, "los apuntes heredan el aislamiento de su asiento");
            (await EscalarAsync<long>(admin, "SELECT count(*) FROM contabilidad.apunte")).Should().BeGreaterThan(apuntesDeA);
        }

        // Lo que no ve, tampoco lo toca…
        (await EjecutarAsync(c, "UPDATE facturacion.factura SET estado_envio_aeat = 'x' WHERE empresa_id = @a", empresaB)).Should().Be(0);

        // …ni puede escribir a nombre de otra empresa.
        var e = await FallaAsync(c, """
            INSERT INTO tesoreria.movimiento
            SELECT (jsonb_populate_record(NULL::tesoreria.movimiento,
                    to_jsonb(m) || jsonb_build_object('id', gen_random_uuid(), 'empresa_id', @a::text))).*
              FROM tesoreria.movimiento m
            """, empresaB);
        e.SqlState.Should().Be(PostgresErrorCodes.InsufficientPrivilege, "la política de RLS rechaza la fila (WITH CHECK)");

        await using var sinEmpresa = await ConexionAppAsync(null);
        (await EscalarAsync<long>(sinEmpresa, "SELECT count(*) FROM facturacion.factura")).Should().Be(0, "sin empresa activa no se ve nada");
    }

    // ---------------------------------------------------------------- documentos inalterables

    [Fact]
    public async Task Una_factura_emitida_es_inalterable_en_la_base_de_datos()
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        var factura = await FacturaAsync(api);
        await using var c = await ConexionAppAsync(empresa);

        (await FallaAsync(c, "UPDATE facturacion.factura SET total = 1 WHERE id = @a", factura.Id)).DebeSerGarantia("factura.inalterable");
        (await FallaAsync(c, "UPDATE facturacion.factura SET cliente_nombre = 'Otro' WHERE id = @a", factura.Id)).DebeSerGarantia("factura.inalterable");
        (await FallaAsync(c, "DELETE FROM facturacion.factura WHERE id = @a", factura.Id)).DebeSerGarantia("factura.inalterable");
        (await FallaAsync(c, "UPDATE facturacion.linea_factura SET descripcion = 'x' WHERE factura_id = @a", factura.Id)).DebeSerGarantia("factura.inalterable");
        (await FallaAsync(c, "DELETE FROM facturacion.linea_factura WHERE factura_id = @a", factura.Id)).DebeSerGarantia("factura.inalterable");
        (await FallaAsync(c, """
            INSERT INTO facturacion.linea_factura
            SELECT (jsonb_populate_record(NULL::facturacion.linea_factura, to_jsonb(l) || jsonb_build_object('id', gen_random_uuid()))).*
              FROM facturacion.linea_factura l WHERE factura_id = @a
            """, factura.Id)).DebeSerGarantia("factura.inalterable");

        // La anulación (el único cambio permitido) funciona, y no tiene vuelta atrás.
        (await api.PostAsJsonAsync($"/facturas/{factura.Id}/anular", new { Motivo = "Error en el cliente" })).StatusCode.Should().Be(HttpStatusCode.OK);
        (await FallaAsync(c, "UPDATE facturacion.factura SET estado = 'Emitida' WHERE id = @a", factura.Id)).DebeSerGarantia("factura.estado");
    }

    [Fact]
    public async Task La_numeracion_de_facturas_no_admite_huecos_ni_fechas_desordenadas()
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        var factura = await FacturaAsync(api, new DateOnly(DateTime.UtcNow.Year, 6, 15));
        await using var c = await ConexionAppAsync(empresa);

        // Copia de la factura con otro número (y otra fecha): el control se hace al confirmar.
        const string copia = """
            BEGIN;
            INSERT INTO facturacion.factura
            SELECT (jsonb_populate_record(NULL::facturacion.factura, to_jsonb(f) || jsonb_build_object(
                    'id', gen_random_uuid(), 'numero', f.numero + {0}, 'fecha_emision', f.fecha_emision + {1},
                    'numero_completo', f.prefijo || f.ejercicio || '/' || lpad((f.numero + {0})::text, 6, '0')))).*
              FROM facturacion.factura f WHERE id = @a;
            COMMIT;
            """;
        (await FallaAsync(c, string.Format(System.Globalization.CultureInfo.InvariantCulture, copia, 2, 0), factura.Id))
            .DebeSerGarantia("factura.numeracion");
        (await FallaAsync(c, string.Format(System.Globalization.CultureInfo.InvariantCulture, copia, 1, -1), factura.Id))
            .DebeSerGarantia("factura.fecha_no_correlativa");

        // La aplicación avisa antes con un error claro.
        var anterior = await api.PostAsJsonAsync("/facturas", PeticionFactura(await ClienteAsync(api), new DateOnly(DateTime.UtcNow.Year, 6, 14)));
        anterior.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await anterior.Content.ReadAsStringAsync()).Should().Contain("factura.fecha_no_correlativa");
    }

    [Fact]
    public async Task Las_emisiones_simultaneas_numeran_sin_huecos_y_encadenan_verifactu()
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        var cliente = await ClienteAsync(api);
        var respuestas = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => api.PostAsJsonAsync("/facturas", PeticionFactura(cliente))));
        respuestas.Select(r => r.StatusCode).Should().AllBeEquivalentTo(HttpStatusCode.Created);

        var numeros = new List<long>();
        foreach (var r in respuestas)
        {
            numeros.Add((await r.Content.ReadFromJsonAsync<FacturaResp>())!.Numero);
        }

        numeros.Order().Should().Equal(Enumerable.Range(1, 8).Select(n => (long)n));

        // La cadena VeriFactu es una sola, sin bifurcaciones: cada huella anterior es la del registro previo.
        await using var c = await ConexionAppAsync(empresa);
        var registros = new Dictionary<string, string?>();
        await using (var cmd = new NpgsqlCommand("SELECT huella, huella_anterior FROM facturacion.factura", c))
        await using (var lector = await cmd.ExecuteReaderAsync())
        {
            while (await lector.ReadAsync())
            {
                registros[lector.GetString(0)] = lector.IsDBNull(1) ? null : lector.GetString(1);
            }
        }

        var siguiente = registros.ToDictionary(x => x.Value ?? string.Empty, x => x.Key);
        siguiente.Should().HaveCount(8, "dos facturas con la misma huella anterior romperían la cadena");
        var eslabones = 0;
        for (var huella = string.Empty; siguiente.TryGetValue(huella, out var hija); huella = hija)
        {
            eslabones++;
        }

        eslabones.Should().Be(8);
    }

    [Fact]
    public async Task Un_asiento_registrado_es_inalterable_y_debe_cuadrar()
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        var asiento = await AsientoAsync(api);
        await using var c = await ConexionAppAsync(empresa);

        (await FallaAsync(c, "UPDATE contabilidad.apunte SET debe = debe + 1 WHERE asiento_id = @a", asiento)).DebeSerGarantia("asiento.inalterable");
        (await FallaAsync(c, "UPDATE contabilidad.asiento SET concepto = 'x' WHERE id = @a", asiento)).DebeSerGarantia("asiento.inalterable");
        (await FallaAsync(c, "DELETE FROM contabilidad.asiento WHERE id = @a", asiento)).DebeSerGarantia("asiento.inalterable");

        var nuevo = Guid.NewGuid();
        (await FallaAsync(c, $"""
            BEGIN;
            INSERT INTO contabilidad.asiento
            SELECT (jsonb_populate_record(NULL::contabilidad.asiento, to_jsonb(a) || jsonb_build_object('id', '{nuevo}', 'numero', a.numero + 1))).*
              FROM contabilidad.asiento a WHERE id = @a;
            INSERT INTO contabilidad.apunte (id, cuenta_codigo, concepto, debe, haber, asiento_id)
            VALUES (gen_random_uuid(), '572', NULL, 100, 0, '{nuevo}');
            COMMIT;
            """, asiento)).DebeSerGarantia("asiento.descuadrado");
    }

    [Fact]
    public async Task Los_cobros_y_la_auditoria_no_se_modifican()
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        await CobrarAsync(api, await FacturaAsync(api));
        await using var c = await ConexionAppAsync(empresa);

        (await FallaAsync(c, "UPDATE tesoreria.movimiento SET importe = 1 WHERE empresa_id = @a", empresa)).DebeSerGarantia("tesoreria.movimiento_inalterable");
        (await FallaAsync(c, "DELETE FROM tesoreria.movimiento WHERE empresa_id = @a", empresa)).DebeSerGarantia("tesoreria.movimiento_inalterable");
        (await FallaAsync(c, "DELETE FROM auditoria.registro_auditoria WHERE empresa_id = @a", empresa)).DebeSerGarantia("auditoria.inalterable");
    }

    [Fact]
    public async Task La_baja_de_la_empresa_es_la_unica_que_borra_sus_facturas_y_cobros()
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        await CobrarAsync(api, await FacturaAsync(api));

        (await api.DeleteAsync(new Uri("/cuenta", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.OK);

        await using var admin = new NpgsqlConnection(FabricaApiPruebas.CadenaAdmin);
        await admin.OpenAsync();
        (await EscalarAsync<long>(admin, "SELECT count(*) FROM facturacion.factura WHERE empresa_id = @a", empresa)).Should().Be(0);
        (await EscalarAsync<long>(admin, "SELECT count(*) FROM tesoreria.movimiento WHERE empresa_id = @a", empresa)).Should().Be(0);

        // El permiso de borrado era local a esa transacción: no queda en la conexión.
        await using var c = await ConexionAppAsync(empresa);
        (await EscalarAsync<string>(c, $"SELECT coalesce(current_setting('{GarantiasSql.ParametroBorradoEmpresa}', true), '')")).Should().BeEmpty();
    }

    // ---------------------------------------------------------------- ayudas

    private static object PeticionFactura(Guid cliente, DateOnly? fecha = null) => new
    {
        ClienteId = cliente,
        FechaEmision = fecha ?? DateOnly.FromDateTime(DateTime.UtcNow),
        DiasVencimiento = 0,
        Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = 100m, CodigoIva = "IVA21" } },
    };

    private static async Task<Guid> ClienteAsync(HttpClient api) =>
        (await (await api.PostAsJsonAsync("/clientes", new { Nombre = "Cliente SL", NifFiscal = Ayudas.GenerarNif() })).Content.ReadFromJsonAsync<IdResp>())!.Id;

    private static async Task<FacturaResp> FacturaAsync(HttpClient api, DateOnly? fecha = null)
    {
        var resp = await api.PostAsJsonAsync("/facturas", PeticionFactura(await ClienteAsync(api), fecha));
        resp.StatusCode.Should().Be(HttpStatusCode.Created, await resp.Content.ReadAsStringAsync());
        return (await resp.Content.ReadFromJsonAsync<FacturaResp>())!;
    }

    private static async Task CobrarAsync(HttpClient api, FacturaResp factura) =>
        (await api.PostAsJsonAsync("/cobros", new { FacturaId = factura.Id, Importe = factura.Total })).IsSuccessStatusCode.Should().BeTrue();

    private static async Task<Guid> AsientoAsync(HttpClient api)
    {
        var resp = await api.PostAsJsonAsync("/contabilidad/asientos", new
        {
            Fecha = $"{DateTime.UtcNow.Year}-03-15",
            Concepto = "Aportación de socio",
            Lineas = new[]
            {
                new { CuentaCodigo = "572", Debe = 1000m, Haber = 0m },
                new { CuentaCodigo = "100", Debe = 0m, Haber = 1000m },
            },
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Created, await resp.Content.ReadAsStringAsync());
        return (await resp.Content.ReadFromJsonAsync<IdResp>())!.Id;
    }

    /// <summary>Conexión directa con el rol de la aplicación, con (o sin) empresa activa: la RLS se aplica.</summary>
    private static async Task<NpgsqlConnection> ConexionAppAsync(Guid? empresa)
    {
        var c = new NpgsqlConnection(FabricaApiPruebas.CadenaConexion);
        await c.OpenAsync();
        await using var cmd = new NpgsqlCommand("SELECT set_config('app.empresa_actual', @e, false)", c);
        cmd.Parameters.AddWithValue("e", empresa?.ToString() ?? string.Empty);
        await cmd.ExecuteNonQueryAsync();
        return c;
    }

    private static async Task<List<string>> ListaAdminAsync(string sql)
    {
        await using var c = new NpgsqlConnection(FabricaApiPruebas.CadenaAdmin);
        await c.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, c);
        await using var r = await cmd.ExecuteReaderAsync();
        var filas = new List<string>();
        while (await r.ReadAsync())
        {
            filas.Add(r.GetString(0));
        }

        return filas;
    }

    private static NpgsqlCommand Comando(NpgsqlConnection c, string sql, Guid? a)
    {
        var cmd = new NpgsqlCommand(sql, c);
        if (a is { } valor)
        {
            cmd.Parameters.AddWithValue("a", valor);
        }

        return cmd;
    }

    private static async Task<T> EscalarAsync<T>(NpgsqlConnection c, string sql, Guid? a = null)
    {
        await using var cmd = Comando(c, sql, a);
        return (T)(await cmd.ExecuteScalarAsync())!;
    }

    private static async Task<int> EjecutarAsync(NpgsqlConnection c, string sql, Guid? a = null)
    {
        await using var cmd = Comando(c, sql, a);
        return await cmd.ExecuteNonQueryAsync();
    }

    private static async Task<PostgresException> FallaAsync(NpgsqlConnection c, string sql, Guid? a = null)
    {
        try
        {
            await EjecutarAsync(c, sql, a);
        }
        catch (PostgresException e)
        {
            // Si la sentencia iba dentro de BEGIN…COMMIT y falló antes del COMMIT, se cierra la transacción.
            await EjecutarAsync(c, "ROLLBACK");
            return e;
        }

        throw new Xunit.Sdk.XunitException($"Se esperaba que la base de datos rechazara:\n{sql}");
    }
}

/// <summary>Aserción de que una excepción es la violación de una garantía concreta.</summary>
internal static class AsercionesGarantia
{
    public static void DebeSerGarantia(this PostgresException e, string codigo)
    {
        e.SqlState.Should().Be(GarantiasSql.CodigoError, e.MessageText);
        e.Hint.Should().Be(codigo, e.MessageText);
    }
}
