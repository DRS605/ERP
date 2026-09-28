using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Motor de análisis: todos los conjuntos de datos y dimensiones ejecutan; subtotales, tabla dinámica, comparación
/// con el año anterior, N primeros con «resto», filtros de dimensión y de medida, detalle, valores e informes guardados.
/// </summary>
public sealed class AnalisisTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public AnalisisTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record CampoResp(string Clave, string Nombre, string Grupo);
    private sealed record DatasetResp(string Clave, List<CampoResp> Dimensiones, List<CampoResp> Medidas);
    private sealed record PlantillaResp(string Clave, JsonElement Consulta);
    private sealed record CatalogoResp(List<DatasetResp> Datasets, List<PlantillaResp> Plantillas);
    private sealed record FilaResp(int Nivel, List<string?> Claves, List<decimal?> Valores, List<decimal?>? Anteriores, List<List<decimal?>>? Celdas, bool Resto);
    private sealed record ResultadoResp(List<string?> ValoresColumna, List<FilaResp> Filas, DateOnly? DesdeAnterior);
    private sealed record DetalleResp(List<JsonElement> Columnas, List<List<JsonElement>> Filas, List<string?> Ids, string? VistaDocumento);
    private sealed record ValorResp(string? Valor, decimal Registros);
    private sealed record InformeResp(Guid Id, string Nombre, bool Compartido, bool Propio);

    private static async Task<Guid> IdAsync(HttpClient api, string ruta, object cuerpo)
    {
        var r = await api.PostAsJsonAsync(ruta, cuerpo);
        r.IsSuccessStatusCode.Should().BeTrue($"{ruta}: {await r.Content.ReadAsStringAsync()}");
        return (await r.Content.ReadFromJsonAsync<IdResp>())!.Id;
    }

    private static async Task<ResultadoResp> ConsultarAsync(HttpClient api, object consulta)
    {
        var r = await api.PostAsJsonAsync("/analisis/consulta", consulta);
        r.StatusCode.Should().Be(HttpStatusCode.OK, await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<ResultadoResp>())!;
    }

    private async Task<HttpClient> EmpresaConDatosAsync()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();
        (await api.PutAsJsonAsync("/contabilidad/contabilizacion-automatica", new { Automatica = true })).EnsureSuccessStatusCode();
        (await api.GetAsync(new Uri("/tipos-iva", UriKind.Relative))).EnsureSuccessStatusCode();
        var fruta = await IdAsync(api, "/familias", new { Nombre = "Fruta", Codigo = "FRU" });
        var naranja = await IdAsync(api, "/productos", new { Nombre = "Naranja", PrecioUnitario = 1m, Tipo = "Bien", Unidad = "kg", FamiliaId = fruta, PrecioCompra = 0.6m });
        var limon = await IdAsync(api, "/productos", new { Nombre = "Limón", PrecioUnitario = 2m, Tipo = "Bien", Unidad = "kg", FamiliaId = fruta });
        var grande = await IdAsync(api, "/clientes", new { Nombre = "Grande SA", NifFiscal = "A46103834" });
        var pequeno = await IdAsync(api, "/clientes", new { Nombre = "Pequeño SL", NifFiscal = "B12345674" });
        var tercero = await IdAsync(api, "/clientes", new { Nombre = "Tercero SL" });

        async Task<Guid> Factura(Guid cliente, string fecha, Guid producto, decimal cantidad, decimal precio) =>
            await IdAsync(api, "/facturas", new
            {
                ClienteId = cliente, FechaEmision = fecha,
                Lineas = new[] { new { ProductoId = producto, Descripcion = "x", Cantidad = cantidad, PrecioUnitario = precio, CodigoIva = "IVA4" } },
            });

        await Factura(grande, "2025-03-10", naranja, 500m, 1m);        // año anterior: 500
        await Factura(grande, "2026-03-10", naranja, 1000m, 1m);       // 1.000
        var cobrable = await Factura(grande, "2026-04-10", limon, 100m, 2m); // 200
        await Factura(pequeno, "2026-04-15", naranja, 300m, 1m);       // 300
        await Factura(tercero, "2026-05-02", limon, 25m, 2m);          // 50
        (await api.PostAsJsonAsync("/cobros", new { FacturaId = cobrable, Importe = 208m })).EnsureSuccessStatusCode();

        var proveedor = await IdAsync(api, "/proveedores", new { Nombre = "Envases SL", NifFiscal = "B12345674" });
        (await api.PostAsJsonAsync("/gastos", new
        {
            ProveedorId = proveedor, NumeroFactura = "E-1", FechaFactura = "2026-04-01", Fecha = "2026-04-01",
            Lineas = new object[] { new { Base = 100m, CodigoIva = "IVA21", CuentaGasto = "602" }, new { Base = 50m, CodigoIva = "IVA21" } },
        })).EnsureSuccessStatusCode();
        return api;
    }

    [Fact]
    public async Task Todos_los_conjuntos_y_dimensiones_se_pueden_consultar()
    {
        var api = await EmpresaConDatosAsync();
        var catalogo = (await api.GetFromJsonAsync<CatalogoResp>("/analisis/catalogo"))!;
        catalogo.Datasets.Select(d => d.Clave).Should().Contain(["ventas", "compras", "deuda", "tesoreria", "contabilidad"]);
        catalogo.Plantillas.Should().NotBeEmpty();

        foreach (var ds in catalogo.Datasets)
        {
            var medidas = ds.Medidas.Select(m => m.Clave).ToList();
            foreach (var dim in ds.Dimensiones)
            {
                var r = await api.PostAsJsonAsync("/analisis/consulta", new { Dataset = ds.Clave, Filas = new[] { dim.Clave }, Medidas = medidas.Take(12) });
                r.StatusCode.Should().Be(HttpStatusCode.OK, $"{ds.Clave}/{dim.Clave}: {await r.Content.ReadAsStringAsync()}");
            }

            var detalle = await api.PostAsJsonAsync("/analisis/detalle", new { Dataset = ds.Clave });
            detalle.StatusCode.Should().Be(HttpStatusCode.OK, $"{ds.Clave} detalle: {await detalle.Content.ReadAsStringAsync()}");
        }

        // Las plantillas de la galería también ejecutan.
        foreach (var p in catalogo.Plantillas)
        {
            var consulta = System.Text.Json.Nodes.JsonNode.Parse(p.Consulta.GetRawText())!.AsObject();
            consulta["desde"] = "2026-01-01";
            consulta["hasta"] = "2026-12-31";
            var r = await api.PostAsync(new Uri("/analisis/consulta", UriKind.Relative), JsonContent.Create(consulta));
            r.StatusCode.Should().Be(HttpStatusCode.OK, $"plantilla {p.Clave}: {await r.Content.ReadAsStringAsync()}");
        }
    }

    [Fact]
    public async Task Subtotales_tabla_dinamica_comparacion_y_filtros()
    {
        var api = await EmpresaConDatosAsync();

        // Cliente > artículo: total, subtotales por cliente (ordenados por base) y detalle.
        var r = await ConsultarAsync(api, new { Dataset = "ventas", Filas = new[] { "cliente", "articulo" }, Medidas = new[] { "base", "facturas", "margen_pct" }, Desde = "2026-01-01", Hasta = "2026-12-31" });
        r.Filas[0].Should().Match<FilaResp>(f => f.Nivel == 0 && f.Valores[0] == 1550m && f.Valores[1] == 4m);
        r.Filas.Where(f => f.Nivel == 1).Select(f => (f.Claves[0], f.Valores[0])).Should().Equal(("Grande SA", 1200m), ("Pequeño SL", 300m), ("Tercero SL", 50m));
        r.Filas.Where(f => f.Nivel == 2 && f.Claves[0] == "Grande SA").Select(f => f.Claves[1]).Should().Equal("Naranja", "Limón");
        r.Filas.Single(f => f.Nivel == 2 && f.Claves[1] == "Naranja" && f.Claves[0] == "Grande SA").Valores[2].Should().Be(40m, "coste 0,60 sobre precio 1");

        // Tabla dinámica: artículo × mes.
        var pivote = await ConsultarAsync(api, new { Dataset = "ventas", Filas = new[] { "articulo" }, Columna = "mes", Medidas = new[] { "base" }, Desde = "2026-01-01", Hasta = "2026-12-31" });
        pivote.ValoresColumna.Should().Equal("2026-03", "2026-04", "2026-05");
        var naranja = pivote.Filas.Single(f => f.Nivel == 1 && f.Claves[0] == "Naranja");
        naranja.Celdas!.Select(c => c[0]).Should().Equal(1000m, 300m, null);
        pivote.Filas[0].Celdas!.Select(c => c[0]).Should().Equal(1000m, 500m, 50m);

        // Comparación con el año anterior.
        var comp = await ConsultarAsync(api, new { Dataset = "ventas", Filas = new[] { "cliente" }, Medidas = new[] { "base" }, Desde = "2026-01-01", Hasta = "2026-12-31", Comparar = "anio_anterior" });
        comp.DesdeAnterior.Should().Be(new DateOnly(2025, 1, 1));
        comp.Filas[0].Anteriores![0].Should().Be(500m);
        comp.Filas.Single(f => f.Claves.FirstOrDefault() == "Pequeño SL").Anteriores![0].Should().BeNull();

        // Por mes, el año anterior se alinea con el mismo mes: marzo 2026 frente a marzo 2025.
        var meses = await ConsultarAsync(api, new { Dataset = "ventas", Filas = new[] { "mes" }, Medidas = new[] { "base" }, Desde = "2026-01-01", Hasta = "2026-12-31", Comparar = "anio_anterior" });
        meses.Filas.Single(f => f.Claves.FirstOrDefault() == "2026-03").Anteriores![0].Should().Be(500m);

        // Los 2 primeros y el resto.
        var top = await ConsultarAsync(api, new { Dataset = "ventas", Filas = new[] { "cliente" }, Medidas = new[] { "base", "margen_pct" }, Desde = "2026-01-01", Hasta = "2026-12-31", Limite = 2 });
        top.Filas.Where(f => f.Nivel == 1).Should().HaveCount(3);
        top.Filas.Last().Should().Match<FilaResp>(f => f.Resto && f.Valores[0] == 50m && f.Valores[1] == null);

        // Filtros: por dimensión (varios valores) y por medida (clientes de más de 250 €).
        var filtrado = await ConsultarAsync(api, new
        {
            Dataset = "ventas", Filas = new[] { "cliente" }, Medidas = new[] { "base" }, Desde = "2026-01-01", Hasta = "2026-12-31",
            Filtros = new[] { new { Dimension = "articulo", Operador = "en", Valores = new[] { "Naranja" } } },
            FiltrosMedida = new[] { new { Medida = "base", Operador = "mayor", Valor = 250m } },
        });
        filtrado.Filas.Select(f => (f.Nivel, f.Valores[0])).Should().Equal((0, 1300m), (1, 1000m), (1, 300m));

        // Deuda: lo cobrado de la factura de 208 € no está pendiente.
        var deuda = await ConsultarAsync(api, new { Dataset = "deuda", Filas = new[] { "cliente" }, Medidas = new[] { "total", "cobrado", "pendiente" } });
        deuda.Filas[0].Valores.Should().Equal(1612m + 520m, 208m, 1612m + 520m - 208m);

        // Contabilidad: los asientos cuadran.
        var conta = await ConsultarAsync(api, new { Dataset = "contabilidad", Filas = new[] { "grupo" }, Medidas = new[] { "debe", "haber", "saldo" } });
        conta.Filas[0].Valores[2].Should().Be(0m);

        // Errores: dimensión inexistente.
        var mal = await api.PostAsJsonAsync("/analisis/consulta", new { Dataset = "ventas", Filas = new[] { "inventada" } });
        (await mal.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("analisis.dimension");
    }

    [Fact]
    public async Task Detalle_valores_e_informes_guardados()
    {
        var api = await EmpresaConDatosAsync();

        var detalle = (await (await api.PostAsJsonAsync("/analisis/detalle", new
        {
            Dataset = "ventas", Desde = "2026-01-01", Hasta = "2026-12-31",
            Filtros = new[] { new { Dimension = "cliente", Operador = "en", Valores = new[] { "Grande SA" } } },
        })).Content.ReadFromJsonAsync<DetalleResp>())!;
        detalle.Filas.Should().HaveCount(2);
        detalle.VistaDocumento.Should().Be("facturas");
        detalle.Ids.Should().OnlyContain(i => i != null && i.Length == 36);

        var valores = (await api.GetFromJsonAsync<List<ValorResp>>("/analisis/ventas/valores?dimension=cliente&texto=sa"))!;
        valores.Should().ContainSingle().Which.Valor.Should().Be("Grande SA");

        var creado = await api.PostAsJsonAsync("/analisis/informes", new { Nombre = "Ventas por cliente", Dataset = "ventas", Definicion = """{"filas":["cliente"]}""", Compartido = true });
        creado.StatusCode.Should().Be(HttpStatusCode.Created, await creado.Content.ReadAsStringAsync());
        var informe = (await creado.Content.ReadFromJsonAsync<InformeResp>())!;
        (await api.PutAsJsonAsync($"/analisis/informes/{informe.Id}", new { Nombre = "Ventas (cliente)", Dataset = "ventas", Definicion = "{}", Compartido = false })).EnsureSuccessStatusCode();
        var lista = (await api.GetFromJsonAsync<List<InformeResp>>("/analisis/informes"))!;
        lista.Should().ContainSingle().Which.Should().Match<InformeResp>(i => i.Nombre == "Ventas (cliente)" && i.Propio && !i.Compartido);
        (await api.DeleteAsync(new Uri($"/analisis/informes/{informe.Id}", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Otra empresa no ve los informes ni los datos de esta.
        var (otra, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var ajena = await ConsultarAsync(otra, new { Dataset = "ventas", Filas = new[] { "cliente" }, Medidas = new[] { "base" } });
        ajena.Filas.Should().ContainSingle().Which.Valores[0].Should().BeNull();
    }
}
