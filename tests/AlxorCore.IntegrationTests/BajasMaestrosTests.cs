using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AlxorCore.Api.Comun;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Eliminar, dar de baja y reactivar maestros: solo se elimina lo que no se ha usado en ninguna empresa del grupo;
/// lo usado se da de baja (409 «…en_uso» al intentar eliminarlo) y se puede reactivar.
/// </summary>
public sealed class BajasMaestrosTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public BajasMaestrosTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record GrupoResp(Guid Id);
    private sealed record SeleccionResp(string Token);
    private sealed record TerceroResp(Guid Id, string Nombre, bool Activo);
    private sealed record BajaResp(Guid Id, bool Eliminado, bool Activo);
    private sealed record ProblemaResp(string Title, string Codigo);
    private sealed record MiembroResp(Guid UsuarioId, bool EsYo);

    private static async Task<Guid> CrearAsync(HttpClient cli, string ruta, object cuerpo)
    {
        var r = await cli.PostAsJsonAsync(ruta, cuerpo);
        r.IsSuccessStatusCode.Should().BeTrue(await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<IdResp>())!.Id;
    }

    private static async Task<ProblemaResp> ConflictoAsync(HttpResponseMessage r)
    {
        r.StatusCode.Should().Be(HttpStatusCode.Conflict, await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<ProblemaResp>())!;
    }

    private static async Task SeleccionarAsync(HttpClient cli, Guid empresaId)
    {
        var sel = await (await cli.PostAsync(new Uri($"/empresas/{empresaId}/seleccionar", UriKind.Relative), null))
            .Content.ReadFromJsonAsync<SeleccionResp>();
        cli.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sel!.Token);
    }

    [Fact]
    public async Task Un_cliente_sin_uso_se_elimina_y_uno_con_facturas_solo_se_da_de_baja()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var libre = await CrearAsync(cli, "/clientes", new { Nombre = "Por error", NifFiscal = Ayudas.GenerarNif() });
        var r = await cli.DeleteAsync(new Uri($"/clientes/{libre}", UriKind.Relative));
        r.StatusCode.Should().Be(HttpStatusCode.OK, await r.Content.ReadAsStringAsync());
        (await r.Content.ReadFromJsonAsync<BajaResp>())!.Eliminado.Should().BeTrue();
        (await cli.GetAsync(new Uri($"/clientes/{libre}", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.NotFound);

        var usado = await CrearAsync(cli, "/clientes", new { Nombre = "Con facturas", NifFiscal = Ayudas.GenerarNif() });
        await CrearAsync(cli, "/facturas", new { ClienteId = usado, Lineas = new[] { new { Cantidad = 1m, Descripcion = "X", PrecioUnitario = 100m, CodigoIva = "IVA21" } } });
        var p = await ConflictoAsync(await cli.DeleteAsync(new Uri($"/clientes/{usado}", UriKind.Relative)));
        p.Codigo.Should().Be("cliente.en_uso");
        p.Title.Should().Contain("facturas").And.Contain("dar");

        // Baja: sale del listado normal, aparece con ?bajas=true y se reactiva.
        (await cli.PostAsync(new Uri($"/clientes/{usado}/baja", UriKind.Relative), null)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await cli.GetFromJsonAsync<List<TerceroResp>>("/clientes"))!.Should().NotContain(c => c.Id == usado);
        (await cli.GetFromJsonAsync<List<TerceroResp>>("/clientes?bajas=true"))!.Single(c => c.Id == usado).Activo.Should().BeFalse();
        (await cli.PostAsync(new Uri($"/clientes/{usado}/alta", UriKind.Relative), null)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await cli.GetFromJsonAsync<List<TerceroResp>>("/clientes"))!.Single(c => c.Id == usado).Activo.Should().BeTrue();
    }

    [Fact]
    public async Task El_uso_en_otra_empresa_del_grupo_tambien_impide_eliminar()
    {
        // El cliente es del grupo; se factura en B y se intenta borrar desde A (que no ve las facturas de B).
        var (cli, empresaA) = await Ayudas.ConEmpresaAsync(_fabrica);
        var grupo = (await cli.GetFromJsonAsync<GrupoResp>("/grupos/actual"))!.Id;
        var clienteId = await CrearAsync(cli, "/clientes", new { Nombre = "Compartido", NifFiscal = Ayudas.GenerarNif() });
        var productoId = await CrearAsync(cli, "/productos", new { Nombre = "Compartido", PrecioUnitario = 5m, Tipo = "Bien", CodigoIva = "IVA21" });

        var empresaB = await CrearAsync(cli, "/empresas", new { Nif = Ayudas.GenerarNif(), RazonSocial = "Empresa B SL", GrupoId = grupo });
        await SeleccionarAsync(cli, empresaB);
        await CrearAsync(cli, "/facturas", new { ClienteId = clienteId, Lineas = new[] { new { ProductoId = productoId, Cantidad = 1m, Descripcion = "X", PrecioUnitario = 5m, CodigoIva = "IVA21" } } });

        await SeleccionarAsync(cli, empresaA);
        (await ConflictoAsync(await cli.DeleteAsync(new Uri($"/clientes/{clienteId}", UriKind.Relative)))).Codigo.Should().Be("cliente.en_uso");
        (await ConflictoAsync(await cli.DeleteAsync(new Uri($"/productos/{productoId}", UriKind.Relative)))).Codigo.Should().Be("producto.en_uso");
    }

    [Fact]
    public async Task Proveedor_articulo_y_tarifa()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);

        var proveedor = await CrearAsync(cli, "/proveedores", new { Nombre = "Prov", NifFiscal = Ayudas.GenerarNif() });
        (await cli.DeleteAsync(new Uri($"/proveedores/{proveedor}", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.OK);

        // Artículo con stock inicial: tiene movimientos, no se borra.
        var conStock = await CrearAsync(cli, "/productos", new { Nombre = "Con stock", PrecioUnitario = 10m, Tipo = "Bien", CodigoIva = "IVA21", ControlarStock = true, StockInicial = 3m });
        (await ConflictoAsync(await cli.DeleteAsync(new Uri($"/productos/{conStock}", UriKind.Relative)))).Codigo.Should().Be("producto.en_uso");
        (await cli.PostAsync(new Uri($"/productos/{conStock}/baja", UriKind.Relative), null)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await cli.GetFromJsonAsync<List<TerceroResp>>("/productos"))!.Should().NotContain(p => p.Id == conStock);
        (await cli.GetFromJsonAsync<List<TerceroResp>>("/productos?bajas=true"))!.Should().Contain(p => p.Id == conStock);

        var libre = await CrearAsync(cli, "/productos", new { Nombre = "Libre", PrecioUnitario = 10m, Tipo = "Bien", CodigoIva = "IVA21" });
        (await cli.DeleteAsync(new Uri($"/productos/{libre}", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.OK);

        // Tarifa asignada a un cliente: no se borra hasta quitársela.
        var tarifa = await CrearAsync(cli, "/tarifas", new { Codigo = "T" + Guid.NewGuid().ToString("N")[..6], Nombre = "Mayoristas" });
        var cliente = await CrearAsync(cli, "/clientes", new { Nombre = "Mayorista", NifFiscal = Ayudas.GenerarNif() });
        (await cli.PutAsJsonAsync($"/clientes/{cliente}/tarifa", new { TarifaId = tarifa })).IsSuccessStatusCode.Should().BeTrue();
        (await ConflictoAsync(await cli.DeleteAsync(new Uri($"/tarifas/{tarifa}", UriKind.Relative)))).Codigo.Should().Be("tarifa.en_uso");
        (await cli.PutAsJsonAsync($"/clientes/{cliente}/tarifa", new { TarifaId = (Guid?)null })).IsSuccessStatusCode.Should().BeTrue();
        (await cli.DeleteAsync(new Uri($"/tarifas/{tarifa}", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Almacenes_y_ubicaciones_se_modifican_y_se_eliminan_si_no_tienen_movimientos()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var almacen = await CrearAsync(cli, "/inventario/almacenes", new { Codigo = "A1", Nombre = "Central" });
        var ubicacion = await CrearAsync(cli, "/inventario/ubicaciones", new { AlmacenId = almacen, Codigo = "P1", Nombre = "Pasillo 1" });

        (await cli.PutAsJsonAsync($"/inventario/almacenes/{almacen}", new { Codigo = "A1", Nombre = "Central (nave 2)" })).IsSuccessStatusCode.Should().BeTrue();
        (await cli.PutAsJsonAsync($"/inventario/ubicaciones/{ubicacion}", new { AlmacenId = almacen, Codigo = "P1", Nombre = "Pasillo uno" })).IsSuccessStatusCode.Should().BeTrue();

        // Con un movimiento ya no se puede eliminar: se da de baja.
        var producto = await CrearAsync(cli, "/productos", new { Nombre = "Tornillo", PrecioUnitario = 1m, Tipo = "Bien", CodigoIva = "IVA21", ControlarStock = true });
        var entrada = await cli.PostAsJsonAsync("/inventario/entrada", new { ProductoId = producto, AlmacenId = almacen, UbicacionId = ubicacion, Cantidad = 5m });
        entrada.IsSuccessStatusCode.Should().BeTrue(await entrada.Content.ReadAsStringAsync());
        (await ConflictoAsync(await cli.DeleteAsync(new Uri($"/inventario/ubicaciones/{ubicacion}", UriKind.Relative)))).Codigo.Should().Be("ubicacion.en_uso");
        (await ConflictoAsync(await cli.DeleteAsync(new Uri($"/inventario/almacenes/{almacen}", UriKind.Relative)))).Codigo.Should().Be("almacen.en_uso");
        (await cli.PostAsync(new Uri($"/inventario/almacenes/{almacen}/baja", UriKind.Relative), null)).StatusCode.Should().Be(HttpStatusCode.OK);

        // Uno vacío (con una ubicación sin uso) se elimina entero.
        var vacio = await CrearAsync(cli, "/inventario/almacenes", new { Codigo = "A2", Nombre = "Vacío" });
        await CrearAsync(cli, "/inventario/ubicaciones", new { AlmacenId = vacio, Codigo = "X", Nombre = "X" });
        (await cli.DeleteAsync(new Uri($"/inventario/almacenes/{vacio}", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Una_actividad_sin_uso_se_elimina_con_su_visibilidad_y_una_usada_no()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var libre = await CrearAsync(cli, "/actividades", new { Nombre = "Por error" });
        var usada = await CrearAsync(cli, "/actividades", new { Nombre = "Cítricos" });
        var yo = (await cli.GetFromJsonAsync<List<MiembroResp>>("/usuarios"))!.First(u => u.EsYo).UsuarioId;
        (await cli.PutAsJsonAsync($"/actividades/visibilidad/{yo}", new { Area = "Ventas", Actividades = new[] { libre, usada } })).IsSuccessStatusCode.Should().BeTrue();
        await CrearAsync(cli, "/clientes", new { Nombre = "Frutas", NifFiscal = Ayudas.GenerarNif(), ActividadNegocioId = usada });

        var r = await cli.DeleteAsync(new Uri($"/actividades/{libre}", UriKind.Relative));
        r.StatusCode.Should().Be(HttpStatusCode.OK, await r.Content.ReadAsStringAsync());
        var enUso = await ConflictoAsync(await cli.DeleteAsync(new Uri($"/actividades/{usada}", UriKind.Relative)));
        enUso.Codigo.Should().Be("actividad.en_uso");
        enUso.Title.Should().Contain("clientes");

        (await cli.GetFromJsonAsync<List<IdResp>>("/actividades"))!.Select(a => a.Id).Should().BeEquivalentTo([usada]);
    }

    /// <summary>
    /// Guardia: toda columna de la base de datos que referencia a un maestro está en el mapa de referencias
    /// (o declarada como propia del maestro). Si un módulo nuevo añade una, esta prueba obliga a mapearla; si no,
    /// se podría borrar un maestro que ese módulo usa.
    /// </summary>
    [Fact]
    public async Task Toda_columna_que_referencia_a_un_maestro_esta_en_el_mapa()
    {
        using var ambito = _fabrica.Services.CreateScope();
        var db = ambito.ServiceProvider.GetRequiredService<AlxorCore.Organizacion.Infraestructura.Persistencia.OrganizacionDbContext>();
        var patrones = new Dictionary<string, string>
        {
            [@"(^|_)cliente_id$"] = "cliente",
            [@"(^|_)proveedor(_habitual)?_id$"] = "proveedor",
            [@"^(producto|componente|envase_producto|producto_padre)_id$"] = "producto",
            [@"^almacen_id$"] = "almacen",
            [@"^ubicacion_id$"] = "ubicacion",
            [@"^tercero_id$"] = "tercero",
            [@"^centro(_origen|_analitico)?_id$"] = "centro_coste",
            [@"^clave_reparto_id$"] = "clave_reparto",
            [@"^actividad_negocio_id$"] = "actividad_negocio",
        };
        var columnas = await db.Database.SqlQueryRaw<string>("""
            SELECT table_schema || '.' || table_name || '.' || column_name AS "Value"
            FROM information_schema.columns
            WHERE data_type = 'uuid' AND table_schema NOT IN ('pg_catalog', 'information_schema', 'public')
            """).ToListAsync();

        var mapeadas = ReferenciasRegistros.Mapa.SelectMany(m => m.Value.Select(r => r.Referencia)).Concat(ReferenciasRegistros.Propias).ToHashSet();
        var sinMapear = columnas
            .Where(c => patrones.Keys.Any(p => System.Text.RegularExpressions.Regex.IsMatch(c.Split('.')[2], p)))
            .Where(c => !mapeadas.Contains(c))
            .OrderBy(c => c, StringComparer.Ordinal).ToList();
        sinMapear.Should().BeEmpty("cada referencia a un maestro debe estar en ReferenciasRegistros.Mapa (impide borrar) o en Propias");

        // Y a la inversa: todo lo mapeado existe (evita erratas en el mapa, que harían fallar la función SQL).
        var existentes = columnas.ToHashSet();
        mapeadas.Where(m => !existentes.Contains(m)).Should().BeEmpty("el mapa no puede citar columnas que no existen");
    }
}
