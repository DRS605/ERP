using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>IGIC (Canarias), modelo 420 y prorrata del IVA/IGIC soportado.</summary>
public sealed class ImpuestosIndirectosEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public ImpuestosIndirectosEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record TipoResp(string Codigo, decimal Porcentaje, string Clase, string Impuesto);
    private sealed record ImpuestoResp(string Codigo, decimal Porcentaje);
    private sealed record LineaResp(string CodigoIva, decimal PorcentajeIva, decimal CuotaIva, decimal CuotaRecargo);
    private sealed record FacturaResp(Guid Id, decimal CuotaIva, decimal RecargoTotal, decimal Total, string Impuesto, List<LineaResp> Lineas);
    private sealed record ProblemaResp(string Title, string Codigo);
    private sealed record DevengoResp(string Codigo, decimal Porcentaje, decimal Base, decimal Cuota);
    private sealed record Modelo420Resp(List<DevengoResp> Devengado, decimal DevengadoCuota, decimal SoportadoCuota, int PorcentajeProrrata,
        decimal DeducibleCuota, decimal RegularizacionProrrata, decimal Resultado);
    private sealed record TrimestreResp(int Trimestre, decimal Resultado);
    private sealed record Modelo425Resp(decimal DevengadoCuota, decimal Resultado, List<TrimestreResp> Trimestres);
    private sealed record Modelo303Resp(decimal IvaDevengadoCuota, decimal IvaDeducibleCuota, decimal Resultado, decimal IvaSoportadoCuota,
        int PorcentajeProrrata, decimal RegularizacionProrrata, decimal RegularizacionBienesInversion = 0m);
    private sealed record LineaBienResp(string Codigo, int AnioDelPeriodo, int Anios, int PorcentajeInicial, int PorcentajeDefinitivo, decimal Regularizacion);
    private sealed record BienesResp(int PorcentajeDefinitivo, List<LineaBienResp> Bienes, decimal Total);
    private sealed record ResumenResp(Modelo303Resp Modelo303);
    private sealed record SoportadoResp(decimal Total, decimal Comun, decimal ConDerecho, decimal SinDerecho);
    private sealed record ProrrataResp(string? Regimen, int PorcentajeProvisional, int PorcentajeDefinitivo, decimal BaseConDerecho, decimal BaseSinDerecho,
        SoportadoResp Soportado, decimal DeducibleProvisional, decimal DeducibleDefinitivo, decimal Regularizacion,
        decimal DeducibleGeneral, decimal DeducibleEspecial, bool EspecialObligatoria, string? Aviso);
    private sealed record ApunteResp(string CuentaCodigo, decimal Debe, decimal Haber);
    private sealed record AsientoResp(string Origen, List<ApunteResp> Apuntes);

    private static readonly int Anio = DateTime.UtcNow.Year;

    private static DateOnly Dia(int mes, int dia) => new(Anio, mes, dia);

    private static async Task<(HttpClient Api, Guid Cliente)> EmpresaAsync(FabricaApiPruebas fabrica, bool canarias, bool recargo = false)
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(fabrica);
        if (canarias)
        {
            (await api.PutAsJsonAsync("/empresas/actual/territorio-fiscal", new { TerritorioFiscal = "Canarias" })).StatusCode.Should().Be(HttpStatusCode.OK);
        }

        await api.GetAsync(new Uri("/tipos-iva", UriKind.Relative));   // siembra el catálogo del territorio
        var cliente = (await (await api.PostAsJsonAsync("/clientes", new { Nombre = "Cliente SL", NifFiscal = Ayudas.GenerarNif(), RecargoEquivalencia = recargo }))
            .Content.ReadFromJsonAsync<IdResp>())!.Id;
        return (api, cliente);
    }

    private static Task<HttpResponseMessage> FacturarAsync(HttpClient api, Guid cliente, DateOnly fecha, params (decimal Base, string? Codigo)[] lineas) =>
        api.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente,
            FechaEmision = fecha,
            Lineas = lineas.Select(l => new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = l.Base, CodigoIva = l.Codigo }).ToArray(),
        });

    private static async Task<FacturaResp> FacturaAsync(HttpClient api, Guid cliente, DateOnly fecha, params (decimal Base, string? Codigo)[] lineas)
    {
        var resp = await FacturarAsync(api, cliente, fecha, lineas);
        resp.StatusCode.Should().Be(HttpStatusCode.Created, await resp.Content.ReadAsStringAsync());
        return (await resp.Content.ReadFromJsonAsync<FacturaResp>())!;
    }

    private static async Task GastoAsync(HttpClient api, DateOnly fecha, decimal baseImponible, string? codigo, string? afectacion = null)
    {
        var resp = await api.PostAsJsonAsync("/gastos", new { Concepto = "Compra", BaseImponible = baseImponible, CodigoIva = codigo, Fecha = fecha, Afectacion = afectacion });
        resp.StatusCode.Should().Be(HttpStatusCode.Created, await resp.Content.ReadAsStringAsync());
    }

    // ------------------------------------------------------------------ IGIC

    [Fact]
    public async Task Una_empresa_canaria_recibe_los_tipos_de_IGIC()
    {
        var (api, _) = await EmpresaAsync(_fabrica, canarias: true);

        var tipos = (await api.GetFromJsonAsync<List<TipoResp>>("/tipos-iva"))!;
        tipos.Should().Contain(t => t.Codigo == "IGIC7" && t.Porcentaje == 7m && t.Impuesto == "Igic");
        tipos.Should().Contain(t => t.Codigo == "IGIC95" && t.Porcentaje == 9.5m);
        tipos.Should().Contain(t => t.Codigo == "IGICEXENTO" && t.Clase == "Exento");

        var estatales = (await api.GetFromJsonAsync<List<ImpuestoResp>>("/impuestos"))!;
        estatales.Select(i => i.Codigo).Should().Contain("IGIC7").And.NotContain("IVA21");
    }

    [Fact]
    public async Task En_Canarias_se_factura_con_IGIC_sin_recargo_y_no_se_admite_IVA()
    {
        var (api, cliente) = await EmpresaAsync(_fabrica, canarias: true, recargo: true);

        var f = await FacturaAsync(api, cliente, Dia(3, 10), (100m, "IGIC7"), (200m, null));   // sin código: IGIC general
        f.Impuesto.Should().Be("Igic");
        f.Lineas.Select(l => l.CodigoIva).Should().Equal("IGIC7", "IGIC7");
        f.CuotaIva.Should().Be(21m);
        f.RecargoTotal.Should().Be(0m, "el IGIC no tiene recargo de equivalencia");
        f.Total.Should().Be(321m);

        var iva = await FacturarAsync(api, cliente, Dia(3, 11), (100m, "IVA21"));
        iva.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await iva.Content.ReadFromJsonAsync<ProblemaResp>())!.Title.Should().Be("El tipo IVA21 es de IVA, pero la empresa tributa por IGIC (Canarias).");

        var xml = await api.GetStringAsync(new Uri($"/facturas/{f.Id}/verifactu.xml", UriKind.Relative));
        xml.Should().Contain("<Impuesto>03</Impuesto>", "en VeriFactu el IGIC es la clave 03");
    }

    [Fact]
    public async Task Una_empresa_peninsular_no_puede_facturar_con_IGIC()
    {
        var (api, cliente) = await EmpresaAsync(_fabrica, canarias: false);
        var resp = await FacturarAsync(api, cliente, Dia(3, 10), (100m, "IGIC7"));
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await resp.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("factura.impuesto_territorio");
    }

    [Fact]
    public async Task El_modelo_420_desglosa_el_IGIC_por_tipos_y_el_303_no_lo_incluye()
    {
        var (api, cliente) = await EmpresaAsync(_fabrica, canarias: true);
        await FacturaAsync(api, cliente, Dia(2, 1), (100m, "IGIC7"), (200m, "IGIC3"));
        await FacturaAsync(api, cliente, Dia(2, 2), (50m, "IGIC0"));
        await GastoAsync(api, Dia(2, 3), 50m, null);             // sin código en Canarias: IGIC 7 %

        var m420 = (await api.GetFromJsonAsync<Modelo420Resp>($"/impuestos/modelo-420?anio={Anio}&trimestre=1"))!;
        m420.Devengado.Should().BeEquivalentTo(new[]
        {
            new DevengoResp("IGIC7", 7m, 100m, 7m),
            new DevengoResp("IGIC3", 3m, 200m, 6m),
            new DevengoResp("IGIC0", 0m, 50m, 0m),
        }, o => o.WithStrictOrdering());
        m420.DevengadoCuota.Should().Be(13m);
        m420.SoportadoCuota.Should().Be(3.50m);
        m420.Resultado.Should().Be(9.50m);

        var m425 = (await api.GetFromJsonAsync<Modelo425Resp>($"/impuestos/modelo-425?anio={Anio}"))!;
        m425.DevengadoCuota.Should().Be(13m);
        m425.Resultado.Should().Be(9.50m);
        m425.Trimestres.Should().HaveCount(4);
        m425.Trimestres.Single(t => t.Trimestre == 1).Resultado.Should().Be(9.50m);

        var r303 = (await api.GetFromJsonAsync<ResumenResp>($"/informes/resumen-trimestral?anio={Anio}&trimestre=1"))!;
        r303.Modelo303.IvaDevengadoCuota.Should().Be(0m);
        r303.Modelo303.IvaDeducibleCuota.Should().Be(0m);
    }

    // ------------------------------------------------------------------ prorrata

    [Fact]
    public async Task Con_prorrata_general_se_deduce_el_provisional_y_se_regulariza_con_el_definitivo()
    {
        var (api, cliente) = await EmpresaAsync(_fabrica, canarias: false);
        (await api.PutAsJsonAsync($"/impuestos/prorrata/{Anio}", new { Regimen = "General", PorcentajeProvisional = 70 })).StatusCode.Should().Be(HttpStatusCode.OK);

        // Ventas del año: 801 € con derecho a deducir y 200 € exentas (IVA0, art. 20) → 801 / 1001 = 80,02 % → 81 %.
        await FacturaAsync(api, cliente, Dia(2, 10), (801m, "IVA21"));
        await FacturaAsync(api, cliente, Dia(2, 11), (200m, "IVA0"));
        await GastoAsync(api, Dia(2, 12), 1000m, "IVA21");           // 210 € de IVA soportado

        var t1 = (await api.GetFromJsonAsync<ResumenResp>($"/informes/resumen-trimestral?anio={Anio}&trimestre=1"))!.Modelo303;
        t1.IvaSoportadoCuota.Should().Be(210m);
        t1.PorcentajeProrrata.Should().Be(70);
        t1.IvaDeducibleCuota.Should().Be(147m);
        t1.RegularizacionProrrata.Should().Be(0m);

        var p = (await api.GetFromJsonAsync<ProrrataResp>($"/impuestos/prorrata?ejercicio={Anio}"))!;
        p.PorcentajeDefinitivo.Should().Be(81, "el porcentaje se redondea a la unidad superior");
        p.DeducibleProvisional.Should().Be(147m);
        p.DeducibleDefinitivo.Should().Be(170.10m);
        p.Regularizacion.Should().Be(23.10m);

        var t4 = (await api.GetFromJsonAsync<ResumenResp>($"/informes/resumen-trimestral?anio={Anio}&trimestre=4"))!.Modelo303;
        t4.RegularizacionProrrata.Should().Be(23.10m, "la última autoliquidación regulariza el año con el porcentaje definitivo");
        t4.Resultado.Should().Be(-23.10m);

        // El asiento de la regularización, a 31/12: 472 a 639 (se deduce más). Una sola vez.
        var reg = await api.PostAsync(new Uri($"/impuestos/prorrata/{Anio}/regularizar", UriKind.Relative), null);
        reg.StatusCode.Should().Be(HttpStatusCode.OK, await reg.Content.ReadAsStringAsync());
        var asiento = (await reg.Content.ReadFromJsonAsync<AsientoResp>())!;
        asiento.Apuntes.Should().ContainSingle(x => x.CuentaCodigo == "472" && x.Debe == 23.10m);
        asiento.Apuntes.Should().ContainSingle(x => x.CuentaCodigo == "639" && x.Haber == 23.10m);
        var otra = await api.PostAsync(new Uri($"/impuestos/prorrata/{Anio}/regularizar", UriKind.Relative), null);
        (await otra.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("prorrata.regularizada");
    }

    [Fact]
    public async Task Los_bienes_de_inversion_se_regularizan_si_el_porcentaje_cambia_mas_de_10_puntos()
    {
        var (api, cliente) = await EmpresaAsync(_fabrica, canarias: false);
        // Máquina comprada hace dos años con 10.000 € de IVA deducido entero (100 %); este año, la mitad de las ventas es exenta: 50 %.
        var maquina = await api.PostAsJsonAsync("/contabilidad/inmovilizado", new
        {
            Codigo = "MAQ-1", Descripcion = "Máquina", CuentaActivo = "213", CuentaAmortizacion = "2813", CuentaDotacion = "6813",
            FechaAdquisicion = new DateOnly(Anio - 2, 1, 10), FechaAlta = new DateOnly(Anio - 2, 1, 10), ValorAdquisicion = 50000m, ValorResidual = 0m,
            Periodicidad = "Anual", MetodoContable = "Lineal", VidaUtilContable = 10, PorcentajeDegresivoContable = 0m, MetodoFiscal = "Lineal", VidaUtilFiscal = 10,
            PorcentajeDegresivoFiscal = 0m, CuotaImpuestoSoportada = 10000m, PorcentajeDeduccionInicial = 100,
        });
        maquina.StatusCode.Should().Be(HttpStatusCode.Created, await maquina.Content.ReadAsStringAsync());
        var nave = (await (await api.PostAsJsonAsync("/contabilidad/inmovilizado", new
        {
            Codigo = "NAVE", Descripcion = "Nave", CuentaActivo = "211", CuentaAmortizacion = "2811", CuentaDotacion = "6811",
            FechaAdquisicion = new DateOnly(Anio - 1, 6, 1), FechaAlta = new DateOnly(Anio - 1, 6, 1), ValorAdquisicion = 100000m, ValorResidual = 0m,
            Periodicidad = "Anual", MetodoContable = "Lineal", VidaUtilContable = 30, PorcentajeDegresivoContable = 0m, MetodoFiscal = "Lineal", VidaUtilFiscal = 30,
            PorcentajeDegresivoFiscal = 0m,
        })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        (await api.PutAsJsonAsync($"/contabilidad/inmovilizado/{nave}/impuesto", new { CuotaImpuestoSoportada = 21000m, PorcentajeDeduccionInicial = 55, BienInmueble = true }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        await FacturaAsync(api, cliente, Dia(2, 10), (500m, "IVA21"));
        await FacturaAsync(api, cliente, Dia(2, 11), (500m, "IVA0"));

        // Máquina: 10.000 × (50 − 100) % / 5 = −1.000. Nave (inmueble, 10 años): 55 → 50 no pasa de 10 puntos, no se regulariza.
        var bienes = (await api.GetFromJsonAsync<BienesResp>($"/impuestos/bienes-inversion?ejercicio={Anio}"))!;
        bienes.PorcentajeDefinitivo.Should().Be(50);
        bienes.Bienes.Should().ContainSingle(b => b.Codigo == "MAQ-1" && b.AnioDelPeriodo == 3 && b.Anios == 5 && b.Regularizacion == -1000m);
        bienes.Bienes.Should().ContainSingle(b => b.Codigo == "NAVE" && b.Anios == 10 && b.Regularizacion == 0m);
        bienes.Total.Should().Be(-1000m);

        // El 303 del cuarto trimestre la lleva (casilla 43): se ingresan 1.000 € más.
        var t4 = (await api.GetFromJsonAsync<ResumenResp>($"/informes/resumen-trimestral?anio={Anio}&trimestre=4"))!.Modelo303;
        t4.RegularizacionBienesInversion.Should().Be(-1000m);
        t4.Resultado.Should().Be(1000m);

        // Asiento a 31/12: 634 a 472 (se deduce menos). Una sola vez.
        var reg = await api.PostAsync(new Uri($"/impuestos/bienes-inversion/{Anio}/regularizar", UriKind.Relative), null);
        reg.StatusCode.Should().Be(HttpStatusCode.OK, await reg.Content.ReadAsStringAsync());
        var asiento = (await reg.Content.ReadFromJsonAsync<AsientoResp>())!;
        asiento.Apuntes.Should().ContainSingle(x => x.CuentaCodigo == "634" && x.Debe == 1000m);
        asiento.Apuntes.Should().ContainSingle(x => x.CuentaCodigo == "472" && x.Haber == 1000m);
        (await (await api.PostAsync(new Uri($"/impuestos/bienes-inversion/{Anio}/regularizar", UriKind.Relative), null)).Content.ReadFromJsonAsync<ProblemaResp>())!
            .Codigo.Should().Be("bienes_inversion.regularizada");

        // Fuera del periodo (el año de la compra lo regulariza la prorrata general) no hay nada.
        (await api.GetFromJsonAsync<BienesResp>($"/impuestos/bienes-inversion?ejercicio={Anio - 2}"))!.Bienes.Should().NotContain(b => b.Codigo == "MAQ-1");
    }

    [Fact]
    public async Task La_prorrata_especial_deduce_por_afectacion_y_se_avisa_si_es_obligatoria()
    {
        var (api, cliente) = await EmpresaAsync(_fabrica, canarias: false);
        (await api.PutAsJsonAsync($"/impuestos/prorrata/{Anio}", new { Regimen = "General", PorcentajeProvisional = 80 })).StatusCode.Should().Be(HttpStatusCode.OK);
        await FacturaAsync(api, cliente, Dia(1, 10), (800m, "IVA21"));
        await FacturaAsync(api, cliente, Dia(1, 11), (200m, "IVA0"));
        await GastoAsync(api, Dia(1, 12), 1000m, "IVA21", "ConDerecho");
        await GastoAsync(api, Dia(1, 13), 1000m, "IVA21", "SinDerecho");

        var general = (await api.GetFromJsonAsync<ProrrataResp>($"/impuestos/prorrata?ejercicio={Anio}"))!;
        general.PorcentajeDefinitivo.Should().Be(80);
        general.Soportado.Should().Be(new SoportadoResp(420m, 0m, 210m, 210m));
        general.DeducibleGeneral.Should().Be(336m);
        general.DeducibleEspecial.Should().Be(210m);
        general.EspecialObligatoria.Should().BeTrue("la general deduce un 10 % o más que la especial");
        general.Aviso.Should().Contain("prorrata especial es obligatoria");

        await api.PutAsJsonAsync($"/impuestos/prorrata/{Anio}", new { Regimen = "Especial", PorcentajeProvisional = 80 });
        var t1 = (await api.GetFromJsonAsync<ResumenResp>($"/informes/resumen-trimestral?anio={Anio}&trimestre=1"))!.Modelo303;
        t1.IvaDeducibleCuota.Should().Be(210m, "entero lo afecto a operaciones con derecho, nada lo afecto a exentas");
    }

    [Fact]
    public async Task Sin_prorrata_configurada_se_avisa_si_hay_ventas_exentas()
    {
        var (api, cliente) = await EmpresaAsync(_fabrica, canarias: false);
        await FacturaAsync(api, cliente, Dia(1, 10), (500m, "IVA21"));
        await FacturaAsync(api, cliente, Dia(1, 11), (500m, "IVA0"));

        var p = (await api.GetFromJsonAsync<ProrrataResp>($"/impuestos/prorrata?ejercicio={Anio}"))!;
        p.Regimen.Should().BeNull();
        p.PorcentajeDefinitivo.Should().Be(50);
        p.Aviso.Should().Contain("debe aplicar prorrata");

        (await api.PutAsJsonAsync($"/impuestos/prorrata/{Anio}", new { Regimen = "General", PorcentajeProvisional = 101 }))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Al_contabilizar_una_compra_con_prorrata_el_IVA_no_deducible_es_mas_gasto()
    {
        var (api, _) = await EmpresaAsync(_fabrica, canarias: false);
        (await api.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).StatusCode.Should().Be(HttpStatusCode.OK);
        await api.PutAsJsonAsync("/contabilidad/contabilizacion-automatica", new { Automatica = true });
        await api.PutAsJsonAsync($"/impuestos/prorrata/{Anio}", new { Regimen = "General", PorcentajeProvisional = 70 });

        await GastoAsync(api, Dia(3, 1), 1000m, "IVA21");

        var diario = (await api.GetFromJsonAsync<List<AsientoResp>>($"/contabilidad/diario?ejercicio={Anio}"))!;
        var compra = diario.Single(a => a.Origen == "Compra");
        compra.Apuntes.Should().Contain(x => x.CuentaCodigo == "629" && x.Debe == 1063m, "base + 30 % del IVA, no deducible");
        compra.Apuntes.Should().Contain(x => x.CuentaCodigo == "472" && x.Debe == 147m, "solo el 70 % deducible");
        compra.Apuntes.Should().Contain(x => x.CuentaCodigo == "400" && x.Haber == 1210m);
    }
}
