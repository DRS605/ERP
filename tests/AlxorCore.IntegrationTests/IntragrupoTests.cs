using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Operaciones entre empresas del grupo: tercero enlazado con otra empresa, espejo de la factura en la bandeja de la
/// receptora, anulación, cuadre recíproco y liquidación (cobro en una, pago en la otra).
/// </summary>
public sealed class IntragrupoTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public IntragrupoTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record GrupoResp(Guid Id);
    private sealed record SeleccionResp(string Token);
    private sealed record ProblemaResp(string Title, string Codigo);
    private sealed record FacturaResp(Guid Id, string NumeroCompleto, decimal Total);
    private sealed record EmpresaGrupoResp(Guid Id, string RazonSocial);
    private sealed record RecibidaResp(Guid Id, string Origen, string Estado, Guid? ProveedorId, string? ProveedorTexto, string? NumeroFactura, DateOnly? FechaFactura,
        decimal? BaseImponible, string? CodigoIva, Guid? GastoId, Guid? EmpresaOrigenId, Guid? FacturaOrigenId, string? MotivoRechazo, string TipoContenido);
    private sealed record LineaCuadreResp(string Numero, decimal BaseEmitida, string? EstadoEspejo, decimal BaseContabilizada, decimal Diferencia, string Situacion);
    private sealed record ParejaResp(Guid EmisorId, Guid ReceptorId, decimal Emitido, decimal Contabilizado, decimal PendienteEnDestino, decimal Diferencia, List<LineaCuadreResp> Facturas);
    private sealed record CuadreResp(bool Cuadra, List<ParejaResp> Parejas);
    private sealed record SaldoResp(decimal Pendiente, string Estado);
    private sealed record LiquidacionResp(decimal Importe, SaldoResp Cobro, SaldoResp Pago);
    private sealed record EspejoResp(Guid FacturaRecibidaId, string Estado);
    private sealed record LineaConsResp(string Cuenta, List<decimal> PorEmpresa, decimal Agregado, decimal Eliminado, decimal Consolidado);
    private sealed record CorrespResp(decimal SaldoA, decimal SaldoB, decimal Diferencia);
    private sealed record EmpresaConsResp(Guid Id, decimal Porcentaje, string Metodo);
    private sealed record ConsolidadoResp(List<LineaConsResp> Lineas, decimal ResultadoAgregado, decimal ResultadoConsolidado, decimal VentasEliminadas,
        decimal ComprasEliminadas, decimal SaldosEliminados, bool EliminacionesCuadran, List<object> NoEliminadas, List<CorrespResp>? Correspondencias = null,
        decimal ResultadoSociosExternos = 0m, decimal ResultadoDominante = 0m, List<string>? EmpresasExcluidas = null, List<EmpresaConsResp>? Empresas = null);

    private sealed record Grupo(HttpClient Api, Guid A, Guid B, Guid ClienteB);

    private static async Task<Guid> IdAsync(HttpClient api, string ruta, object cuerpo)
    {
        var r = await api.PostAsJsonAsync(ruta, cuerpo);
        r.IsSuccessStatusCode.Should().BeTrue($"{ruta}: {await r.Content.ReadAsStringAsync()}");
        return (await r.Content.ReadFromJsonAsync<IdResp>())!.Id;
    }

    private static async Task SeleccionarAsync(HttpClient api, Guid empresa)
    {
        var sel = await (await api.PostAsync(new Uri($"/empresas/{empresa}/seleccionar", UriKind.Relative), null)).Content.ReadFromJsonAsync<SeleccionResp>();
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sel!.Token);
    }

    /// <summary>Empresa A y empresa B del mismo grupo; el cliente «Empresa B» enlazado con B y el proveedor «Empresa A» con A.</summary>
    private async Task<Grupo> GrupoAsync()
    {
        var (api, a) = await Ayudas.ConEmpresaAsync(_fabrica);
        var grupo = (await api.GetFromJsonAsync<GrupoResp>("/grupos/actual"))!.Id;
        var b = await IdAsync(api, "/empresas", new { Nif = Ayudas.GenerarNif(), RazonSocial = "Empresa B SL", GrupoId = grupo });
        await SeleccionarAsync(api, a);
        (await api.GetFromJsonAsync<List<EmpresaGrupoResp>>("/grupos/actual/empresas"))!.Select(e => e.Id).Should().Contain([a, b]);

        var clienteB = await IdAsync(api, "/clientes", new { Nombre = "Empresa B SL", NifFiscal = Ayudas.GenerarNif(), EmpresaVinculadaId = b });
        await IdAsync(api, "/proveedores", new { Nombre = "Empresa A SL", NifFiscal = Ayudas.GenerarNif(), EmpresaVinculadaId = a });
        return new Grupo(api, a, b, clienteB);
    }

    private static async Task<FacturaResp> FacturarAsync(Grupo g, decimal precio = 100m)
    {
        var r = await g.Api.PostAsJsonAsync("/facturas", new { ClienteId = g.ClienteB, Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicios de gestión", PrecioUnitario = precio, CodigoIva = "IVA21" } } });
        r.IsSuccessStatusCode.Should().BeTrue(await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<FacturaResp>())!;
    }

    private static async Task<List<RecibidaResp>> BandejaAsync(Grupo g) =>
        (await g.Api.GetFromJsonAsync<List<RecibidaResp>>("/recepcion/facturas"))!.Where(f => f.Origen == "Intragrupo").ToList();

    [Fact]
    public async Task Un_tercero_solo_se_enlaza_con_una_empresa_del_grupo()
    {
        var g = await GrupoAsync();
        var (otro, ajena) = await Ayudas.ConEmpresaAsync(_fabrica);
        var r = await g.Api.PostAsJsonAsync("/clientes", new { Nombre = "Ajena", NifFiscal = Ayudas.GenerarNif(), EmpresaVinculadaId = ajena });
        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await r.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("tercero.empresa_vinculada");
        otro.Dispose();
    }

    [Fact]
    public async Task La_factura_a_otra_empresa_del_grupo_llega_a_su_bandeja_se_cuadra_y_se_liquida()
    {
        var g = await GrupoAsync();
        var f = await FacturarAsync(g);

        // Cuadre desde A: emitido, pendiente de contabilizar en B.
        var cuadre = (await g.Api.GetFromJsonAsync<CuadreResp>("/intragrupo/cuadre"))!;
        cuadre.Cuadra.Should().BeFalse();
        var pareja = cuadre.Parejas.Single(p => p.EmisorId == g.A && p.ReceptorId == g.B);
        pareja.Emitido.Should().Be(100m);
        pareja.PendienteEnDestino.Should().Be(100m);
        pareja.Facturas.Single().Situacion.Should().Be("Pendiente en destino");

        // La liquidación exige que B la haya contabilizado.
        var pronto = await g.Api.PostAsJsonAsync($"/intragrupo/facturas/{f.Id}/liquidar", new { });
        pronto.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await pronto.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("intragrupo.sin_contabilizar");

        // En B: el espejo con el PDF y los datos ya rellenos, del proveedor que es A.
        await SeleccionarAsync(g.Api, g.B);
        var espejo = (await BandejaAsync(g)).Single();
        espejo.Estado.Should().Be("Recibida");
        espejo.NumeroFactura.Should().Be(f.NumeroCompleto);
        espejo.BaseImponible.Should().Be(100m);
        espejo.CodigoIva.Should().Be("IVA21");
        espejo.ProveedorId.Should().NotBeNull();
        espejo.EmpresaOrigenId.Should().Be(g.A);
        espejo.FacturaOrigenId.Should().Be(f.Id);
        espejo.TipoContenido.Should().Be("application/pdf");
        var pdf = await g.Api.GetByteArrayAsync(new Uri($"/recepcion/facturas/{espejo.Id}/documento", UriKind.Relative));
        pdf.Take(4).Should().Equal("%PDF"u8.ToArray());

        (await g.Api.PostAsJsonAsync($"/recepcion/facturas/{espejo.Id}/validar", new
        {
            BaseImponible = espejo.BaseImponible, FechaFactura = espejo.FechaFactura, ProveedorId = espejo.ProveedorId, NumeroFactura = espejo.NumeroFactura, CodigoIva = espejo.CodigoIva,
        })).IsSuccessStatusCode.Should().BeTrue();
        (await g.Api.PostAsync(new Uri($"/recepcion/facturas/{espejo.Id}/contabilizar", UriKind.Relative), null)).IsSuccessStatusCode.Should().BeTrue();

        // Vuelve a A: cuadra, y se liquida (cobro en A y pago en B por lo pendiente).
        await SeleccionarAsync(g.Api, g.A);
        cuadre = (await g.Api.GetFromJsonAsync<CuadreResp>("/intragrupo/cuadre"))!;
        cuadre.Cuadra.Should().BeTrue();
        cuadre.Parejas.Single().Facturas.Single().Situacion.Should().Be("Cuadrada");

        var liq = await g.Api.PostAsJsonAsync($"/intragrupo/facturas/{f.Id}/liquidar", new { });
        liq.StatusCode.Should().Be(HttpStatusCode.OK, await liq.Content.ReadAsStringAsync());
        var liquidacion = (await liq.Content.ReadFromJsonAsync<LiquidacionResp>())!;
        liquidacion.Importe.Should().Be(121m);
        liquidacion.Cobro.Pendiente.Should().Be(0m);
        liquidacion.Pago.Pendiente.Should().Be(0m);

        // Reflejar de nuevo no duplica.
        var otra = await g.Api.PostAsync(new Uri($"/intragrupo/facturas/{f.Id}/reflejar", UriKind.Relative), null);
        (await otra.Content.ReadFromJsonAsync<EspejoResp>())!.FacturaRecibidaId.Should().Be(espejo.Id);
    }

    [Fact]
    public async Task Si_el_emisor_anula_la_factura_su_espejo_se_rechaza()
    {
        var g = await GrupoAsync();
        var f = await FacturarAsync(g, 50m);
        (await g.Api.PostAsJsonAsync($"/facturas/{f.Id}/anular", new { Motivo = "Error de importe" })).IsSuccessStatusCode.Should().BeTrue();

        await SeleccionarAsync(g.Api, g.B);
        var espejo = (await BandejaAsync(g)).Single();
        espejo.Estado.Should().Be("Rechazada");
        espejo.MotivoRechazo.Should().Contain(f.NumeroCompleto).And.Contain("Error de importe");

        await SeleccionarAsync(g.Api, g.A);
        var linea = (await g.Api.GetFromJsonAsync<CuadreResp>("/intragrupo/cuadre"))!.Parejas.Single().Facturas.Single();
        linea.Situacion.Should().Be("Anulada");
        linea.Diferencia.Should().Be(0m);
    }

    private static async Task ContabilidadCompletaAsync(HttpClient api)
    {
        (await api.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();
        (await api.PutAsJsonAsync("/contabilidad/contabilizacion-automatica", new { Automatica = true })).EnsureSuccessStatusCode();
    }

    private static decimal Cuenta(ConsolidadoResp c, string cuenta, Func<LineaConsResp, decimal> valor) =>
        c.Lineas.Where(l => l.Cuenta == cuenta).Sum(valor);

    [Fact]
    public async Task La_consolidacion_elimina_ventas_compras_y_saldos_entre_empresas()
    {
        var g = await GrupoAsync();
        await ContabilidadCompletaAsync(g.Api);
        await SeleccionarAsync(g.Api, g.B);
        await ContabilidadCompletaAsync(g.Api);
        await SeleccionarAsync(g.Api, g.A);

        // A factura 1.000 a B (intragrupo) y 500 a un cliente de fuera.
        var f = await FacturarAsync(g, 1_000m);
        var externo = (await (await g.Api.PostAsJsonAsync("/clientes", new { Nombre = "Cliente externo", NifFiscal = Ayudas.GenerarNif() })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        (await g.Api.PostAsJsonAsync("/facturas", new { ClienteId = externo, Lineas = new[] { new { Cantidad = 1m, Descripcion = "Venta", PrecioUnitario = 500m, CodigoIva = "IVA21" } } }))
            .EnsureSuccessStatusCode();

        // Mientras B no la contabiliza, no se elimina (se lista como pendiente).
        var antes = (await g.Api.GetFromJsonAsync<ConsolidadoResp>("/intragrupo/consolidado"))!;
        antes.VentasEliminadas.Should().Be(0m);
        antes.NoEliminadas.Should().HaveCount(1);

        await SeleccionarAsync(g.Api, g.B);
        var espejo = (await BandejaAsync(g)).Single();
        (await g.Api.PostAsJsonAsync($"/recepcion/facturas/{espejo.Id}/validar", new
        {
            BaseImponible = espejo.BaseImponible, FechaFactura = espejo.FechaFactura, ProveedorId = espejo.ProveedorId, NumeroFactura = espejo.NumeroFactura, CodigoIva = espejo.CodigoIva,
        })).EnsureSuccessStatusCode();
        (await g.Api.PostAsync(new Uri($"/recepcion/facturas/{espejo.Id}/contabilizar", UriKind.Relative), null)).EnsureSuccessStatusCode();
        await SeleccionarAsync(g.Api, g.A);

        var c = (await g.Api.GetFromJsonAsync<ConsolidadoResp>("/intragrupo/consolidado"))!;
        c.NoEliminadas.Should().BeEmpty();
        c.EliminacionesCuadran.Should().BeTrue();
        c.VentasEliminadas.Should().Be(1_000m);
        c.ComprasEliminadas.Should().Be(1_000m);
        c.SaldosEliminados.Should().Be(1_210m);
        Cuenta(c, "705", l => l.Agregado).Should().Be(-1_500m);
        Cuenta(c, "705", l => l.Consolidado).Should().Be(-500m, "solo queda la venta a fuera del grupo");
        c.Lineas.Where(l => l.Cuenta.StartsWith('6')).Sum(l => l.Consolidado).Should().Be(0m);
        Cuenta(c, "430", l => l.Consolidado).Should().Be(605m, "queda el cliente externo");
        Cuenta(c, "400", l => l.Consolidado).Should().Be(0m);
        c.ResultadoAgregado.Should().Be(500m);
        c.ResultadoConsolidado.Should().Be(500m, "la venta intragrupo no crea resultado para el grupo");

        // Liquidada, ya no hay saldos pendientes que eliminar.
        (await g.Api.PostAsJsonAsync($"/intragrupo/facturas/{f.Id}/liquidar", new { })).EnsureSuccessStatusCode();
        var tras = (await g.Api.GetFromJsonAsync<ConsolidadoResp>("/intragrupo/consolidado"))!;
        tras.SaldosEliminados.Should().Be(0m);
        Cuenta(tras, "430", l => l.Consolidado).Should().Be(605m);
    }

    private static async Task AsientoAsync(HttpClient api, string concepto, params (string Cuenta, decimal Debe, decimal Haber)[] lineas)
    {
        var r = await api.PostAsJsonAsync("/contabilidad/asientos", new
        {
            Fecha = new DateOnly(DateTime.UtcNow.Year, 2, 1), Concepto = concepto,
            Lineas = lineas.Select(l => new { CuentaCodigo = l.Cuenta, l.Debe, l.Haber }),
        });
        r.IsSuccessStatusCode.Should().BeTrue(await r.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task La_consolidacion_elimina_saldos_reciprocos_emparejados_y_atribuye_socios_externos()
    {
        var g = await GrupoAsync();
        await ContabilidadCompletaAsync(g.Api);
        // A presta 1.000 a B (5523) y vende 500 fuera; B lo recibe (5133) y tiene 200 de gastos.
        await AsientoAsync(g.Api, "Préstamo a B", ("5523", 1_000m, 0m), ("572", 0m, 1_000m));
        await AsientoAsync(g.Api, "Venta", ("430", 500m, 0m), ("705", 0m, 500m));
        await SeleccionarAsync(g.Api, g.B);
        await ContabilidadCompletaAsync(g.Api);
        await AsientoAsync(g.Api, "Préstamo de A", ("572", 1_000m, 0m), ("5133", 0m, 1_000m));
        await AsientoAsync(g.Api, "Gastos", ("629", 200m, 0m), ("572", 0m, 200m));
        await SeleccionarAsync(g.Api, g.A);

        (await g.Api.PostAsJsonAsync("/intragrupo/correspondencias", new { EmpresaAId = g.A, CuentaA = "5523", EmpresaBId = g.B, CuentaB = "5133", Descripcion = "Préstamo A → B" }))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        (await g.Api.PostAsJsonAsync("/intragrupo/correspondencias", new { EmpresaAId = g.A, CuentaA = "55", EmpresaBId = g.B, CuentaB = "5133" }))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest, "una cuenta tiene al menos 3 dígitos");

        // B participada al 80 % por integración global: el 20 % de su resultado es de socios externos.
        (await g.Api.PutAsJsonAsync($"/intragrupo/perimetro/{g.B}", new { Porcentaje = 80m, Metodo = "Global" })).EnsureSuccessStatusCode();
        var c = (await g.Api.GetFromJsonAsync<ConsolidadoResp>("/intragrupo/consolidado"))!;
        c.Correspondencias!.Single().Should().Be(new CorrespResp(1_000m, -1_000m, 0m));
        Cuenta(c, "552", l => l.Consolidado).Should().Be(0m);
        Cuenta(c, "513", l => l.Consolidado).Should().Be(0m);
        c.EliminacionesCuadran.Should().BeTrue();
        c.ResultadoConsolidado.Should().Be(300m);
        c.ResultadoSociosExternos.Should().Be(-40m, "el 20 % de la pérdida de 200 de B");
        c.ResultadoDominante.Should().Be(340m);

        // Proporcional al 50 %: B entra por la mitad de sus saldos.
        (await g.Api.PutAsJsonAsync($"/intragrupo/perimetro/{g.B}", new { Porcentaje = 50m, Metodo = "Proporcional" })).EnsureSuccessStatusCode();
        c = (await g.Api.GetFromJsonAsync<ConsolidadoResp>("/intragrupo/consolidado"))!;
        Cuenta(c, "629", l => l.Consolidado).Should().Be(100m);
        c.Correspondencias!.Single().Should().Be(new CorrespResp(500m, -500m, 0m), "la pareja se elimina en el menor de los porcentajes");
        Cuenta(c, "552", l => l.Consolidado).Should().Be(500m, "la otra mitad del préstamo es con los otros socios de B");
        c.ResultadoSociosExternos.Should().Be(0m);

        // Excluida: fuera del perímetro.
        (await g.Api.PutAsJsonAsync($"/intragrupo/perimetro/{g.B}", new { Porcentaje = 10m, Metodo = "Excluida" })).EnsureSuccessStatusCode();
        c = (await g.Api.GetFromJsonAsync<ConsolidadoResp>("/intragrupo/consolidado"))!;
        c.Empresas!.Select(e => e.Id).Should().Equal(g.A);
        c.EmpresasExcluidas.Should().Equal("Empresa B SL");
        c.Correspondencias.Should().BeEmpty();
        Cuenta(c, "552", l => l.Consolidado).Should().Be(1_000m);
    }
}
