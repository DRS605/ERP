using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Segunda tanda de la auditoría de altas: corregir lo que ya estaba dado de alta en series, divisas, previsiones,
/// presupuestos, facturación periódica, anticipos, cartera, producción, inmovilizado y cartas de porte.
/// </summary>
public sealed class CorreccionesRegistrosTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public CorreccionesRegistrosTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Title, string Codigo);
    private sealed record PrevisionResp(Guid Id, string Concepto, decimal Importe);
    private sealed record AnticipoResp(Guid Id, string Estado, decimal Disponible);
    private sealed record EfectoResp(Guid Id, decimal Pendiente, string Estado, bool Anulado);
    private sealed record OrdenComponenteResp(Guid ComponenteId, decimal CantidadTotal);
    private sealed record OrdenResp(Guid Id, decimal Cantidad, List<OrdenComponenteResp> Componentes);
    private sealed record CartaResp(Guid Id, bool Anulada, string? MotivoAnulacion);
    private sealed record TipoCambioResp(Guid Id, string Divisa);
    private sealed record SerieResp(Guid Id, string Prefijo);

    private static readonly int Anio = DateTime.UtcNow.Year;

    private static async Task<Guid> IdAsync(HttpClient c, string ruta, object cuerpo)
    {
        var r = await c.PostAsJsonAsync(ruta, cuerpo);
        r.IsSuccessStatusCode.Should().BeTrue($"{ruta}: {await r.Content.ReadAsStringAsync()}");
        return (await r.Content.ReadFromJsonAsync<IdResp>())!.Id;
    }

    private static async Task<string> CodigoConflictoAsync(HttpResponseMessage r)
    {
        r.StatusCode.Should().Be(HttpStatusCode.Conflict, await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo;
    }

    private static Task<HttpResponseMessage> PostAsync(HttpClient c, string ruta, object? cuerpo = null) =>
        c.PostAsJsonAsync(ruta, cuerpo ?? new { });

    [Fact]
    public async Task Series_tipos_de_cambio_y_previsiones()
    {
        var (c, _) = await Ayudas.ConEmpresaAsync(_fabrica);

        // Una serie sin usar se elimina; la FA que numera facturas, no.
        var serie = await IdAsync(c, "/series", new { TipoDocumento = "Factura", Ejercicio = Anio, Prefijo = "ERR" });
        (await c.DeleteAsync(new Uri($"/series/{serie}", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.OK);
        var cliente = await IdAsync(c, "/clientes", new { Nombre = "C", NifFiscal = Ayudas.GenerarNif() });
        await IdAsync(c, "/facturas", new { ClienteId = cliente, Lineas = new[] { new { Cantidad = 1m, Descripcion = "X", PrecioUnitario = 10m, CodigoIva = "IVA21" } } });
        var fa = (await c.GetFromJsonAsync<List<SerieResp>>("/series"))!.First();
        (await CodigoConflictoAsync(await c.DeleteAsync(new Uri($"/series/{fa.Id}", UriKind.Relative)))).Should().Be("serie.en_uso");

        // Tipo de cambio: se corrige registrándolo de nuevo y se elimina.
        (await c.PostAsJsonAsync("/tipos-cambio", new { Divisa = "USD", Fecha = new DateOnly(Anio, 1, 2), TasaEur = 0.9m })).IsSuccessStatusCode.Should().BeTrue();
        var tc = (await c.GetFromJsonAsync<List<TipoCambioResp>>("/tipos-cambio"))!.Single(t => t.Divisa == "USD");
        (await c.DeleteAsync(new Uri($"/tipos-cambio/{tc.Id}", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await c.GetFromJsonAsync<List<TipoCambioResp>>("/tipos-cambio"))!.Should().BeEmpty();

        // Previsión: se modifica.
        var prev = await IdAsync(c, "/tesoreria/previsiones", new { Sentido = "Gasto", Concepto = "Alquiler", Importe = 800m, Fecha = new DateOnly(Anio, 6, 1) });
        (await c.PutAsJsonAsync($"/tesoreria/previsiones/{prev}", new { Sentido = "Gasto", Concepto = "Alquiler nave", Importe = 950m, Fecha = new DateOnly(Anio, 6, 5) }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        (await c.GetFromJsonAsync<List<PrevisionResp>>("/tesoreria/previsiones"))!.Single(p => p.Id == prev).Importe.Should().Be(950m);
    }

    [Fact]
    public async Task Presupuestos_contables_facturas_periodicas_y_anticipos()
    {
        var (c, _) = await Ayudas.ConEmpresaAsync(_fabrica);

        var borrador = await IdAsync(c, "/contabilidad/presupuestos", new { Codigo = "B", Nombre = "Borrador", Desde = new DateOnly(Anio, 1, 1), Meses = 12, Lineas = new[] { new { CuentaCodigo = "62", Total = 1200m } } });
        (await c.DeleteAsync(new Uri($"/contabilidad/presupuestos/{borrador}", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var aprobado = await IdAsync(c, "/contabilidad/presupuestos", new { Codigo = "A", Nombre = "Aprobado", Desde = new DateOnly(Anio, 1, 1), Meses = 12, Lineas = new[] { new { CuentaCodigo = "62", Total = 1200m } } });
        (await PostAsync(c, $"/contabilidad/presupuestos/{aprobado}/aprobar")).IsSuccessStatusCode.Should().BeTrue();
        (await CodigoConflictoAsync(await c.DeleteAsync(new Uri($"/contabilidad/presupuestos/{aprobado}", UriKind.Relative)))).Should().Be("presupuesto.aprobado");

        var cliente = await IdAsync(c, "/clientes", new { Nombre = "Suscriptor", NifFiscal = Ayudas.GenerarNif() });
        var recurrente = await IdAsync(c, "/facturas-recurrentes", new
        {
            Nombre = "Cuota",
            ClienteId = cliente,
            Periodicidad = "Mensual",
            PrimeraEmision = new DateOnly(Anio + 1, 1, 1),
            Lineas = new[] { new { Descripcion = "Cuota", Cantidad = 1m, PrecioUnitario = 30m, CodigoIva = "IVA21" } },
        });
        (await c.DeleteAsync(new Uri($"/facturas-recurrentes/{recurrente}", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.NoContent);

        var anticipo = await IdAsync(c, "/anticipos", new { ClienteId = cliente, Importe = 100m });
        var anulado = await PostAsync(c, $"/anticipos/{anticipo}/anular");
        anulado.StatusCode.Should().Be(HttpStatusCode.OK, await anulado.Content.ReadAsStringAsync());
        (await anulado.Content.ReadFromJsonAsync<AnticipoResp>())!.Estado.Should().Be("Anulado");
        var factura = await IdAsync(c, "/facturas", new { ClienteId = cliente, Lineas = new[] { new { Cantidad = 1m, Descripcion = "X", PrecioUnitario = 10m, CodigoIva = "IVA21" } } });
        (await CodigoConflictoAsync(await c.PostAsJsonAsync($"/anticipos/{anticipo}/aplicar", new { FacturaId = factura }))).Should().Be("anticipo.anulado");
    }

    [Fact]
    public async Task Un_efecto_de_cartera_se_anula_si_no_tiene_movimientos_vivos()
    {
        var (c, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var efecto = await IdAsync(c, "/cartera", new { Sentido = "Cobro", TerceroNombre = "Antiguo cliente", Documento = "F-2020/1", Vencimiento = new DateOnly(Anio, 3, 1), Importe = 500m });

        (await c.PostAsJsonAsync($"/cartera/{efecto}/movimientos", new { Importe = 100m })).IsSuccessStatusCode.Should().BeTrue();
        (await CodigoConflictoAsync(await c.PostAsJsonAsync($"/cartera/{efecto}/anular", new { Motivo = "Duplicado" }))).Should().Be("cartera.con_movimientos");

        var saldo = await c.GetFromJsonAsync<SaldoResp>($"/cartera/{efecto}/saldo");
        (await PostAsync(c, $"/tesoreria/movimientos/{saldo!.Movimientos.Single().Id}/anular")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await c.PostAsJsonAsync($"/cartera/{efecto}/anular", new { Motivo = "Duplicado" })).StatusCode.Should().Be(HttpStatusCode.OK);

        (await c.GetFromJsonAsync<List<EfectoResp>>("/cartera?pendientes=true"))!.Should().NotContain(e => e.Id == efecto);
        (await c.GetFromJsonAsync<List<EfectoResp>>("/cartera?pendientes=false"))!.Single(e => e.Id == efecto).Anulado.Should().BeTrue();
        (await CodigoConflictoAsync(await c.PostAsJsonAsync($"/cartera/{efecto}/movimientos", new { Importe = 10m }))).Should().Be("cartera.anulado");
    }

    private sealed record MovResp(Guid Id);
    private sealed record SaldoResp(List<MovResp> Movimientos);

    [Fact]
    public async Task Ordenes_de_fabricacion_inmovilizado_y_cartas_de_porte()
    {
        var (c, _) = await Ayudas.ConEmpresaAsync(_fabrica);

        var tabla = await IdAsync(c, "/productos", new { Nombre = "Tabla", PrecioUnitario = 1m, CodigoIva = "IVA21" });
        var mesa = await IdAsync(c, "/productos", new { Nombre = "Mesa", PrecioUnitario = 60m, CodigoIva = "IVA21" });
        (await c.PutAsJsonAsync($"/productos/{mesa}/composicion", new { Componentes = new[] { new { ComponenteId = tabla, Cantidad = 2m } } })).IsSuccessStatusCode.Should().BeTrue();
        var alm = await IdAsync(c, "/inventario/almacenes", new { Codigo = "F", Nombre = "Fábrica" });
        var orden = await IdAsync(c, "/produccion/ordenes", new { ProductoId = mesa, Cantidad = 5m, AlmacenId = alm });
        var mod = await c.PutAsJsonAsync($"/produccion/ordenes/{orden}", new { ProductoId = mesa, Cantidad = 8m, AlmacenId = alm });
        mod.StatusCode.Should().Be(HttpStatusCode.OK, await mod.Content.ReadAsStringAsync());
        var o = (await mod.Content.ReadFromJsonAsync<OrdenResp>())!;
        o.Cantidad.Should().Be(8m);
        o.Componentes.Single().CantidadTotal.Should().Be(16m);

        var bien = await IdAsync(c, "/contabilidad/inmovilizado", new
        {
            Codigo = "ERR-1",
            Descripcion = "Alta por error",
            CuentaActivo = "218",
            CuentaAmortizacion = "281",
            CuentaDotacion = "681",
            FechaAdquisicion = new DateOnly(Anio, 1, 1),
            FechaAlta = new DateOnly(Anio, 1, 1),
            ValorAdquisicion = 1000m,
            ValorResidual = 0m,
            Periodicidad = "Anual",
            MetodoContable = "Lineal",
            VidaUtilContable = 4,
            PorcentajeDegresivoContable = 0m,
            MetodoFiscal = "Lineal",
            VidaUtilFiscal = 4,
            PorcentajeDegresivoFiscal = 0m,
        });
        (await c.DeleteAsync(new Uri($"/contabilidad/inmovilizado/{bien}", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.NoContent);

        var carta = await IdAsync(c, "/cartas-porte", new
        {
            DestinatarioNombre = "Cliente sin ficha",
            LugarOrigen = "Almacén",
            LugarDestino = "Destino",
            Lineas = new[] { new { Descripcion = "Palés", Bultos = 2, PesoKg = 500m } },
        });
        var anulada = await c.PostAsJsonAsync($"/cartas-porte/{carta}/anular", new { Motivo = "No se hizo el transporte" });
        anulada.StatusCode.Should().Be(HttpStatusCode.OK, await anulada.Content.ReadAsStringAsync());
        (await anulada.Content.ReadFromJsonAsync<CartaResp>())!.Anulada.Should().BeTrue();
        (await CodigoConflictoAsync(await c.PostAsJsonAsync($"/cartas-porte/{carta}/anular", new { }))).Should().Be("cartaporte.ya_anulada");
    }
}
