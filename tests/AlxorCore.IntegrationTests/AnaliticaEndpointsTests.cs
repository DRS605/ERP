using System.Net;
using System.Net.Http.Json;
using AlxorCore.Persistencia;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Contabilidad analítica: reglas, imputación, repartos, periodos e informe.</summary>
public sealed class AnaliticaEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private const string Base = "/contabilidad/analitica";
    private readonly FabricaApiPruebas _fabrica;

    public AnaliticaEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record CuentaResp(string CuentaCodigo, decimal Ingresos, decimal Gastos);
    private sealed record FilaResp(Guid? Id, string Codigo, int Nivel, decimal Ingresos, decimal Gastos, decimal Resultado, List<CuentaResp> Cuentas);
    private sealed record InformeResp(List<FilaResp> Filas, decimal TotalIngresos, decimal TotalGastos, decimal Resultado,
        decimal SinAsignarIngresos, decimal SinAsignarGastos, decimal PorcentajeImputado);
    private sealed record PendienteResp(Guid ApunteId, string CuentaCodigo, decimal Importe, decimal Pendiente, string? ReglaAplicable);
    private sealed record EjecucionResp(Guid Id, string Tipo, int Imputaciones, int SinRegla);
    private sealed record ProblemaResp(string Title, string Codigo);

    private static readonly int Anio = DateTime.UtcNow.Year;
    private static readonly string Desde = $"{Anio}-01-01";
    private static readonly string Hasta = $"{Anio}-12-31";

    private sealed record Escenario(HttpClient Api, Guid EmpresaId, Guid Adm, Guid Fincas, Guid Fin1, Guid Fin2, Guid Clave, Guid Fito, Guid Proveedor);

    /// <summary>
    /// ADM (administración), FINCAS con FIN1 y FIN2 debajo, clave 60/40 entre las fincas, partida
    /// «Fitosanitarios». Reglas: cuenta 629 → ADM; proveedor Fitos → clave de fincas y partida.
    /// </summary>
    private async Task<Escenario> EscenarioAsync()
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();
        (await api.PutAsJsonAsync("/contabilidad/contabilizacion-automatica", new { Automatica = true })).EnsureSuccessStatusCode();

        var adm = await CrearAsync(api, "centros", new { Codigo = "ADM", Nombre = "Administración", Tipo = "Departamento" });
        var fincas = await CrearAsync(api, "centros", new { Codigo = "FINCAS", Nombre = "Fincas", Tipo = "Finca" });
        var fin1 = await CrearAsync(api, "centros", new { Codigo = "FIN1", Nombre = "Finca Los Llanos", Tipo = "Finca", PadreId = fincas });
        var fin2 = await CrearAsync(api, "centros", new { Codigo = "FIN2", Nombre = "Finca El Cerro", Tipo = "Finca", PadreId = fincas });
        var clave = await CrearAsync(api, "claves", new { Codigo = "SUP", Nombre = "Por superficie", Reparto = new[] { new { CentroId = fin1, Porcentaje = 60m }, new { CentroId = fin2, Porcentaje = 40m } } });
        var fito = await CrearAsync(api, "partidas", new { Codigo = "FITO", Nombre = "Fitosanitarios", Naturaleza = "Gasto" });
        var proveedor = (await (await api.PostAsJsonAsync("/proveedores", new { Nombre = "Fitos del Sur SL", NifFiscal = Ayudas.GenerarNif() })).Content.ReadFromJsonAsync<IdResp>())!.Id;

        await CrearAsync(api, "reglas", new { Descripcion = "Servicios a administración", PrefijoCuenta = "629", CentroId = adm });
        await CrearAsync(api, "reglas", new { Descripcion = "Fitos a las fincas", TerceroId = proveedor, ClaveRepartoId = clave, PartidaId = fito });
        return new Escenario(api, empresa, adm, fincas, fin1, fin2, clave, fito, proveedor);
    }

    private static async Task<Guid> CrearAsync(HttpClient api, string ruta, object cuerpo)
    {
        var resp = await api.PostAsJsonAsync($"{Base}/{ruta}", cuerpo);
        resp.StatusCode.Should().Be(HttpStatusCode.Created, await resp.Content.ReadAsStringAsync());
        return (await resp.Content.ReadFromJsonAsync<IdResp>())!.Id;
    }

    private static async Task GastoAsync(HttpClient api, decimal baseImponible, Guid? proveedor = null, string? fecha = null)
    {
        var resp = await api.PostAsJsonAsync("/gastos", new { Concepto = "Compra", BaseImponible = baseImponible, ProveedorId = proveedor, Fecha = fecha ?? $"{Anio}-03-10" });
        resp.StatusCode.Should().Be(HttpStatusCode.Created, await resp.Content.ReadAsStringAsync());
    }

    private static async Task VentaAsync(HttpClient api, decimal importe)
    {
        var cliente = (await (await api.PostAsJsonAsync("/clientes", new { Nombre = "Cliente SL", NifFiscal = Ayudas.GenerarNif() })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        (await api.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente,
            FechaEmision = $"{Anio}-03-15",
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Fruta", PrecioUnitario = importe, CodigoIva = "IVA10" } },
        })).StatusCode.Should().Be(HttpStatusCode.Created);
    }

    private static async Task<InformeResp> InformeAsync(HttpClient api, string dimension = "Centro") =>
        (await api.GetFromJsonAsync<InformeResp>($"{Base}/informe?desde={Desde}&hasta={Hasta}&dimension={dimension}"))!;

    private static FilaResp Fila(InformeResp i, string codigo) => i.Filas.Single(f => f.Codigo == codigo);

    [Fact]
    public async Task Al_contabilizar_las_reglas_imputan_y_el_informe_suma_el_arbol()
    {
        var e = await EscenarioAsync();
        await GastoAsync(e.Api, 1000m);                     // 629 → ADM
        await GastoAsync(e.Api, 500m, e.Proveedor);         // proveedor Fitos → 60/40 fincas, partida FITO
        await VentaAsync(e.Api, 2000m);                     // 700/705: sin regla → sin asignar

        var informe = await InformeAsync(e.Api);
        Fila(informe, "ADM").Gastos.Should().Be(1000m);
        Fila(informe, "FIN1").Gastos.Should().Be(300m);
        Fila(informe, "FIN2").Gastos.Should().Be(200m);
        Fila(informe, "FINCAS").Should().Match<FilaResp>(f => f.Gastos == 500m && f.Nivel == 0, "el padre suma a sus hijos");
        Fila(informe, "FIN1").Nivel.Should().Be(1);
        Fila(informe, "—").Ingresos.Should().Be(2000m);
        informe.SinAsignarIngresos.Should().Be(2000m);
        informe.TotalGastos.Should().Be(1500m);
        informe.Resultado.Should().Be(500m);

        var porPartida = await InformeAsync(e.Api, "Partida");
        Fila(porPartida, "FITO").Gastos.Should().Be(500m);
    }

    [Fact]
    public async Task Los_pendientes_se_imputan_a_mano_sin_pasar_del_importe()
    {
        var e = await EscenarioAsync();
        await VentaAsync(e.Api, 2000m);

        var pendiente = (await e.Api.GetFromJsonAsync<List<PendienteResp>>($"{Base}/pendientes?desde={Desde}&hasta={Hasta}"))!.Single();
        pendiente.Pendiente.Should().Be(2000m);
        pendiente.ReglaAplicable.Should().BeNull();

        var exceso = await e.Api.PutAsJsonAsync($"{Base}/apuntes/{pendiente.ApunteId}", new { Lineas = new[] { new { CentroId = e.Fin1, Importe = 2500m } } });
        exceso.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var ok = await e.Api.PutAsJsonAsync($"{Base}/apuntes/{pendiente.ApunteId}", new
        {
            Lineas = new object[] { new { CentroId = e.Fin1, Porcentaje = 50m }, new { CentroId = e.Fin2, Importe = 700m } },
        });
        ok.StatusCode.Should().Be(HttpStatusCode.OK, await ok.Content.ReadAsStringAsync());

        var informe = await InformeAsync(e.Api);
        Fila(informe, "FIN1").Ingresos.Should().Be(1000m);
        Fila(informe, "FIN2").Ingresos.Should().Be(700m);
        informe.SinAsignarIngresos.Should().Be(300m, "se puede dejar parte sin imputar");
        (await e.Api.GetFromJsonAsync<List<PendienteResp>>($"{Base}/pendientes?desde={Desde}&hasta={Hasta}"))!.Single().Pendiente.Should().Be(300m);
    }

    [Fact]
    public async Task Una_regla_nueva_se_aplica_a_lo_ya_contabilizado_con_el_proceso_y_se_puede_deshacer()
    {
        var e = await EscenarioAsync();
        await VentaAsync(e.Api, 2000m);
        await CrearAsync(e.Api, "reglas", new { Descripcion = "Ventas a la finca 1", PrefijoCuenta = "7", CentroId = e.Fin1 });

        (await e.Api.GetFromJsonAsync<List<PendienteResp>>($"{Base}/pendientes?desde={Desde}&hasta={Hasta}"))!.Single().ReglaAplicable
            .Should().Be("Ventas a la finca 1", "se explica qué regla se aplicaría");

        var proceso = await e.Api.PostAsJsonAsync($"{Base}/imputar-pendientes", new { Desde, Hasta });
        var ejecucion = (await proceso.Content.ReadFromJsonAsync<EjecucionResp>())!;
        ejecucion.Imputaciones.Should().Be(1);
        Fila(await InformeAsync(e.Api), "FIN1").Ingresos.Should().Be(2000m);

        (await e.Api.DeleteAsync(new Uri($"{Base}/ejecuciones/{ejecucion.Id}", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await InformeAsync(e.Api)).SinAsignarIngresos.Should().Be(2000m);
    }

    [Fact]
    public async Task El_reparto_secundario_traspasa_los_costes_indirectos_y_se_deshace_entero()
    {
        var e = await EscenarioAsync();
        await GastoAsync(e.Api, 1000m);   // ADM

        var resp = await e.Api.PostAsJsonAsync($"{Base}/repartos", new { CentroOrigenId = e.Adm, ClaveRepartoId = e.Clave, Desde, Hasta });
        resp.StatusCode.Should().Be(HttpStatusCode.OK, await resp.Content.ReadAsStringAsync());
        var reparto = (await resp.Content.ReadFromJsonAsync<EjecucionResp>())!;

        var informe = await InformeAsync(e.Api);
        Fila(informe, "ADM").Gastos.Should().Be(0m, "el centro de origen queda a cero");
        Fila(informe, "FIN1").Gastos.Should().Be(600m);
        Fila(informe, "FIN2").Gastos.Should().Be(400m);
        informe.TotalGastos.Should().Be(1000m, "repartir no cambia el total");
        Fila(informe, "FIN1").Cuentas.Should().ContainSingle(c => c.CuentaCodigo == "629", "se conserva la cuenta de origen");

        (await e.Api.PostAsJsonAsync($"{Base}/repartos", new { CentroOrigenId = e.Fin1, ClaveRepartoId = e.Clave, Desde, Hasta }))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest, "no se reparte a sí mismo");

        (await e.Api.DeleteAsync(new Uri($"{Base}/ejecuciones/{reparto.Id}", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.NoContent);
        Fila(await InformeAsync(e.Api), "ADM").Gastos.Should().Be(1000m);
    }

    [Fact]
    public async Task Un_periodo_cerrado_no_admite_cambios_y_los_periodos_no_se_solapan()
    {
        var e = await EscenarioAsync();
        await VentaAsync(e.Api, 2000m);
        var campana = await CrearAsync(e.Api, "periodos", new { Codigo = $"C{Anio}", Nombre = "Campaña", Desde = $"{Anio}-01-01", Hasta = $"{Anio}-12-31" });
        (await e.Api.PostAsJsonAsync($"{Base}/periodos", new { Codigo = "X", Nombre = "Solapado", Desde = $"{Anio}-06-01", Hasta = $"{Anio + 1}-05-31" }))
            .StatusCode.Should().Be(HttpStatusCode.Conflict);

        (await e.Api.PutAsJsonAsync($"{Base}/periodos/{campana}/cierre", new { Cerrado = true })).StatusCode.Should().Be(HttpStatusCode.OK);
        var pendiente = (await e.Api.GetFromJsonAsync<List<PendienteResp>>($"{Base}/pendientes?desde={Desde}&hasta={Hasta}"))!.Single();
        var resp = await e.Api.PutAsJsonAsync($"{Base}/apuntes/{pendiente.ApunteId}", new { Lineas = new[] { new { CentroId = e.Fin1, Porcentaje = 100m } } });
        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await resp.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("analitica.periodo_cerrado");

        var informe = (await e.Api.GetFromJsonAsync<InformeResp>($"{Base}/informe?periodoId={campana}"))!;
        informe.TotalIngresos.Should().Be(2000m, "el informe se puede pedir por periodo analítico");
    }

    [Fact]
    public async Task En_el_asiento_manual_se_indica_el_centro_de_cada_gasto()
    {
        var e = await EscenarioAsync();
        var ok = await e.Api.PostAsJsonAsync("/contabilidad/asientos", new
        {
            Fecha = $"{Anio}-04-01",
            Concepto = "Reparación de maquinaria",
            Lineas = new object[]
            {
                new { CuentaCodigo = "622", Debe = 300m, Haber = 0m, CentroId = e.Fin2 },
                new { CuentaCodigo = "572", Debe = 0m, Haber = 300m },
            },
        });
        ok.StatusCode.Should().Be(HttpStatusCode.Created, await ok.Content.ReadAsStringAsync());
        Fila(await InformeAsync(e.Api), "FIN2").Gastos.Should().Be(300m);

        var mal = await e.Api.PostAsJsonAsync("/contabilidad/asientos", new
        {
            Fecha = $"{Anio}-04-02",
            Concepto = "Traspaso",
            Lineas = new object[]
            {
                new { CuentaCodigo = "570", Debe = 100m, Haber = 0m, CentroId = e.Fin2 },
                new { CuentaCodigo = "572", Debe = 0m, Haber = 100m },
            },
        });
        mal.StatusCode.Should().Be(HttpStatusCode.BadRequest, "solo los gastos e ingresos llevan centro");
    }

    [Fact]
    public async Task La_base_de_datos_impide_imputar_de_mas_claves_que_no_suman_100_y_tocar_periodos_cerrados()
    {
        var e = await EscenarioAsync();
        await GastoAsync(e.Api, 1000m);   // imputado a ADM por la regla

        await using var c = new NpgsqlConnection(FabricaApiPruebas.CadenaConexion);
        await c.OpenAsync();
        var grupo = await EscalarAsync<Guid>(c, "SELECT grupo_id FROM organizacion.empresa WHERE id = @e", e.EmpresaId);
        await EjecutarAsync(c, $"SELECT set_config('app.empresa_actual', '{e.EmpresaId}', false), set_config('app.grupo_actual', '{grupo}', false)");

        (await FallaAsync(c, "UPDATE contabilidad.imputacion_analitica SET importe = importe * 2")).Hint.Should().Be("imputacion.exceso");
        (await FallaAsync(c, "UPDATE contabilidad.linea_clave_reparto SET porcentaje = porcentaje - 10")).Hint.Should().Be("clave.reparto");
        (await FallaAsync(c, "UPDATE contabilidad.imputacion_analitica SET cuenta_codigo = '628'")).Hint.Should().Be("imputacion.apunte");

        await EjecutarAsync(c, $"""
            INSERT INTO contabilidad.periodo_analitico (id, empresa_id, codigo, nombre, desde, hasta, cerrado)
            VALUES (gen_random_uuid(), '{e.EmpresaId}', 'CERRADO', 'Cerrado', '{Anio}-01-01', '{Anio}-12-31', true)
            """);
        (await FallaAsync(c, "DELETE FROM contabilidad.imputacion_analitica")).Hint.Should().Be("analitica.periodo_cerrado");
    }

    [Fact]
    public async Task La_analitica_es_un_modulo_contratable()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "gestion" })).EnsureSuccessStatusCode();
        var empresas = await api.GetFromJsonAsync<List<IdResp>>("/empresas");
        var sel = await api.PostAsync(new Uri($"/empresas/{empresas![0].Id}/seleccionar", UriKind.Relative), null);
        var token = (await sel.Content.ReadFromJsonAsync<TokenResp>())!.Token;
        api.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var resp = await api.GetAsync(new Uri($"{Base}/centros", UriKind.Relative));
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await resp.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("modulo.no_contratado");
    }

    private sealed record TokenResp(string Token);

    private static async Task<T> EscalarAsync<T>(NpgsqlConnection c, string sql, Guid a)
    {
        await using var cmd = new NpgsqlCommand(sql, c);
        cmd.Parameters.AddWithValue("e", a);
        return (T)(await cmd.ExecuteScalarAsync())!;
    }

    private static async Task EjecutarAsync(NpgsqlConnection c, string sql)
    {
        await using var cmd = new NpgsqlCommand(sql, c);
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task<PostgresException> FallaAsync(NpgsqlConnection c, string sql)
    {
        try
        {
            await EjecutarAsync(c, sql);
        }
        catch (PostgresException ex)
        {
            ex.SqlState.Should().Be(GarantiasSql.CodigoError, ex.MessageText);
            return ex;
        }

        throw new Xunit.Sdk.XunitException($"Se esperaba que la base de datos rechazara:\n{sql}");
    }
}
