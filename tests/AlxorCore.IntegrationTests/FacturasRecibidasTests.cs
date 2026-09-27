using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Facturas de proveedor completas: número y fecha de la factura, varias bases y tipos, parte no deducible, inversión del
/// sujeto pasivo, vencimientos, duplicados, corrección, y su efecto en el asiento, el libro de IVA, el 303 y el SII.
/// </summary>
public sealed class FacturasRecibidasTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public FacturasRecibidasTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record DesgloseResp(string CodigoIva, decimal PorcentajeIva, decimal Base, decimal Cuota, decimal CuotaDeducible, bool Autoliquidada);
    private sealed record VencimientoResp(DateOnly Fecha, decimal Importe);
    private sealed record GastoResp(Guid Id, string? NumeroFactura, DateOnly? FechaFactura, DateOnly Fecha, decimal BaseImponible, decimal CuotaIva, decimal RetencionIrpf, decimal Total,
        List<DesgloseResp> Desglose, List<VencimientoResp> Vencimientos);
    private sealed record ApunteResp(string CuentaCodigo, decimal Debe, decimal Haber);
    private sealed record AsientoResp(string Origen, decimal Total, List<ApunteResp> Apuntes);
    private sealed record FilaLibroResp(string Documento, string? Nif, decimal Base, decimal Cuota);
    private sealed record LibroResp(List<FilaLibroResp> Asientos, decimal TotalBase, decimal TotalCuota);

    private static async Task<Guid> IdAsync(HttpClient api, string ruta, object cuerpo)
    {
        var r = await api.PostAsJsonAsync(ruta, cuerpo);
        r.IsSuccessStatusCode.Should().BeTrue($"{ruta}: {await r.Content.ReadAsStringAsync()}");
        return (await r.Content.ReadFromJsonAsync<IdResp>())!.Id;
    }

    [Fact]
    public async Task Una_factura_con_varios_tipos_se_registra_asienta_y_declara_por_tipo()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();
        (await api.PutAsJsonAsync("/contabilidad/contabilizacion-automatica", new { Automatica = true })).EnsureSuccessStatusCode();
        (await api.GetAsync(new Uri("/tipos-iva", UriKind.Relative))).EnsureSuccessStatusCode();
        var proveedor = await IdAsync(api, "/proveedores", new { Nombre = "Suministros Hosteleros SL", NifFiscal = "B12345674" });

        var cuerpo = new
        {
            ProveedorId = proveedor,
            NumeroFactura = "F-2026/117",
            FechaFactura = "2026-09-20",
            Fecha = "2026-09-25",
            Lineas = new object[]
            {
                new { Descripcion = "Menaje", Base = 1000m, CodigoIva = "IVA21", CuentaGasto = "602" },
                new { Descripcion = "Alimentación", Base = 500m, CodigoIva = "IVA10" },
                new { Descripcion = "Turismo (50 % afecto)", Base = 200m, CodigoIva = "IVA21", PorcentajeDeducible = 50m },
                new { Descripcion = "Seguro", Base = 100m, CodigoIva = "IVA0" },
            },
            Vencimientos = new[] { new { Fecha = "2026-10-20", Importe = 1000m }, new { Fecha = "2026-11-20", Importe = 1102m } },
        };
        var simulada = (await (await api.PostAsJsonAsync("/gastos/simular", cuerpo)).Content.ReadFromJsonAsync<GastoResp>())!;
        simulada.Total.Should().Be(2102m, "1.800 de base + 210 + 50 + 42 de IVA");

        var r = await api.PostAsJsonAsync("/gastos", cuerpo);
        r.StatusCode.Should().Be(HttpStatusCode.Created, await r.Content.ReadAsStringAsync());
        var g = (await r.Content.ReadFromJsonAsync<GastoResp>())!;
        g.Should().Match<GastoResp>(x => x.NumeroFactura == "F-2026/117" && x.FechaFactura == new DateOnly(2026, 9, 20) && x.BaseImponible == 1800m && x.CuotaIva == 302m && x.Total == 2102m);
        g.Desglose.Select(d => (d.CodigoIva, d.Base, d.Cuota, d.CuotaDeducible)).Should().BeEquivalentTo(new[]
        {
            ("IVA21", 1200m, 252m, 231m), ("IVA10", 500m, 50m, 50m), ("IVA0", 100m, 0m, 0m),
        });
        g.Vencimientos.Should().HaveCount(2);

        // La misma factura del mismo proveedor no se registra dos veces.
        var dup = await api.PostAsJsonAsync("/gastos", cuerpo);
        dup.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await dup.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("gasto.factura_duplicada");
        var malVenc = await api.PostAsJsonAsync("/gastos", new { cuerpo.ProveedorId, NumeroFactura = "F-2", cuerpo.Lineas, Vencimientos = new[] { new { Fecha = "2026-10-20", Importe = 10m } } });
        (await malVenc.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("gasto.vencimientos");

        // Asiento: 602 menaje, 629 el resto con la mitad del IVA del turismo, 472 deducible, 400 el total.
        var diario = await api.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2026");
        var asiento = diario!.Single(a => a.Origen == "Compra");
        asiento.Apuntes.Sum(a => a.Debe).Should().Be(asiento.Apuntes.Sum(a => a.Haber));
        asiento.Apuntes.Should().Contain(a => a.CuentaCodigo == "602" && a.Debe == 1000m);
        asiento.Apuntes.Should().Contain(a => a.CuentaCodigo == "472" && a.Debe == 281m);
        asiento.Apuntes.Should().Contain(a => a.CuentaCodigo == "400" && a.Haber == 2102m);
        asiento.Apuntes.Where(a => a.CuentaCodigo.StartsWith('6')).Sum(a => a.Debe).Should().Be(1821m, "bases 1.800 + 21 de IVA no deducible");

        // Libro de IVA soportado: una fila por tipo, con el número del proveedor y su NIF.
        var libro = (await api.GetFromJsonAsync<LibroResp>("/informes/libro-iva?tipo=Soportado&desde=2026-07-01&hasta=2026-09-30"))!;
        libro.Asientos.Should().HaveCount(3).And.OnlyContain(a => a.Documento == "F-2026/117" && a.Nif == "B12345674");
        libro.TotalCuota.Should().Be(302m);

        // 303 del 3T: deducible 281 (la mitad del IVA del turismo no se deduce).
        using var resumen = JsonDocument.Parse(await api.GetStringAsync("/informes/resumen-trimestral?anio=2026&trimestre=3"));
        resumen.RootElement.GetProperty("modelo303").GetProperty("ivaDeducibleCuota").GetDecimal().Should().Be(281m);

        // SII de recibidas: el número real y un detalle por tipo.
        var sii = await api.GetStringAsync("/informes/sii?tipo=Recibidas&ejercicio=2026&periodo=9");
        sii.Should().Contain("<siiLR:NumSerieFacturaEmisor>F-2026/117</siiLR:NumSerieFacturaEmisor>").And.Contain("20-09-2026");
        System.Text.RegularExpressions.Regex.Matches(sii, "<siiLR:DetalleIVA>").Count.Should().Be(3);
    }

    [Fact]
    public async Task La_inversion_del_sujeto_pasivo_se_autoliquida_y_la_factura_se_corrige_sin_pagos()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();
        (await api.PutAsJsonAsync("/contabilidad/contabilizacion-automatica", new { Automatica = true })).EnsureSuccessStatusCode();
        (await api.GetAsync(new Uri("/tipos-iva", UriKind.Relative))).EnsureSuccessStatusCode();
        var proveedor = await IdAsync(api, "/proveedores", new { Nombre = "Reformas Levante SL" });

        var r = await api.PostAsJsonAsync("/gastos", new
        {
            ProveedorId = proveedor, NumeroFactura = "R-55", FechaFactura = "2026-09-10", Fecha = "2026-09-10",
            Lineas = new[] { new { Descripcion = "Obra nave", Base = 10000m, CodigoIva = "ISP", PorcentajeIva = (decimal?)21m } },
        });
        r.StatusCode.Should().Be(HttpStatusCode.Created, await r.Content.ReadAsStringAsync());
        var g = (await r.Content.ReadFromJsonAsync<GastoResp>())!;
        g.Total.Should().Be(10000m, "el proveedor no cobra el IVA: lo autoliquida la empresa");
        g.Desglose.Should().ContainSingle().Which.Should().Match<DesgloseResp>(d => d.Autoliquidada && d.Cuota == 2100m);

        var asiento = (await api.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2026"))!.Single(a => a.Origen == "Compra");
        asiento.Apuntes.Should().Contain(a => a.CuentaCodigo == "472" && a.Debe == 2100m).And.Contain(a => a.CuentaCodigo == "477" && a.Haber == 2100m)
            .And.Contain(a => a.CuentaCodigo == "400" && a.Haber == 10000m);

        using (var resumen = JsonDocument.Parse(await api.GetStringAsync("/informes/resumen-trimestral?anio=2026&trimestre=3")))
        {
            var m303 = resumen.RootElement.GetProperty("modelo303");
            m303.GetProperty("ivaDevengadoCuota").GetDecimal().Should().Be(2100m);
            m303.GetProperty("ivaDeducibleCuota").GetDecimal().Should().Be(2100m);
            m303.GetProperty("resultado").GetDecimal().Should().Be(0m);
        }

        // Corrección (sin pagos): pasa a IVA 21 ordinario. Contraasiento de lo anterior y asiento nuevo.
        var mod = await api.PutAsJsonAsync($"/gastos/{g.Id}", new
        {
            ProveedorId = proveedor, NumeroFactura = "R-55", FechaFactura = "2026-09-10", Fecha = "2026-09-10",
            Lineas = new[] { new { Descripcion = "Obra nave", Base = 10000m, CodigoIva = "IVA21" } },
        });
        mod.StatusCode.Should().Be(HttpStatusCode.OK, await mod.Content.ReadAsStringAsync());
        (await mod.Content.ReadFromJsonAsync<GastoResp>())!.Total.Should().Be(12100m);
        var compras = (await api.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2026"))!.Where(a => a.Origen == "Compra").ToList();
        compras.Should().HaveCount(3, "el original, su contraasiento y el corregido");
        compras.SelectMany(a => a.Apuntes).Where(a => a.CuentaCodigo == "400").Sum(a => a.Haber - a.Debe).Should().Be(12100m);
    }
    [Fact]
    public async Task Un_abono_del_proveedor_se_registra_como_rectificativa_con_importes_negativos()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();
        (await api.PutAsJsonAsync("/contabilidad/contabilizacion-automatica", new { Automatica = true })).EnsureSuccessStatusCode();
        (await api.GetAsync(new Uri("/tipos-iva", UriKind.Relative))).EnsureSuccessStatusCode();
        var proveedor = await IdAsync(api, "/proveedores", new { Nombre = "Envases del Sur SL", NifFiscal = "B12345674" });
        var otro = await IdAsync(api, "/proveedores", new { Nombre = "Otro SL" });
        var original = await IdAsync(api, "/gastos", new
        {
            ProveedorId = proveedor, NumeroFactura = "E-900", FechaFactura = "2026-09-01", Fecha = "2026-09-01",
            Lineas = new[] { new { Descripcion = "Cajas", Base = 1000m, CodigoIva = "IVA21" } },
        });

        // Sin ser rectificativa, una base negativa no vale; y la rectificada tiene que ser del mismo proveedor.
        var negativa = await api.PostAsJsonAsync("/gastos", new { ProveedorId = proveedor, NumeroFactura = "A-1", Lineas = new[] { new { Base = -100m, CodigoIva = "IVA21" } } });
        (await negativa.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("gasto.base_negativa");
        var ajena = await api.PostAsJsonAsync("/gastos", new
        {
            ProveedorId = otro, NumeroFactura = "A-1", RectificaGastoId = original, MotivoRectificacion = "Devolución",
            Lineas = new[] { new { Base = -100m, CodigoIva = "IVA21" } },
        });
        (await ajena.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("gasto.rectificada_proveedor");

        var r = await api.PostAsJsonAsync("/gastos", new
        {
            ProveedorId = proveedor, NumeroFactura = "A-1", FechaFactura = "2026-09-15", Fecha = "2026-09-15", RectificaGastoId = original,
            MotivoRectificacion = "Devolución de 200 cajas rotas", Lineas = new[] { new { Descripcion = "Cajas devueltas", Base = -200m, CodigoIva = "IVA21" } },
        });
        r.StatusCode.Should().Be(HttpStatusCode.Created, await r.Content.ReadAsStringAsync());
        using (var abono = JsonDocument.Parse(await r.Content.ReadAsStringAsync()))
        {
            abono.RootElement.GetProperty("total").GetDecimal().Should().Be(-242m);
            abono.RootElement.GetProperty("esRectificativa").GetBoolean().Should().BeTrue();
            abono.RootElement.GetProperty("numeroRectificado").GetString().Should().Be("E-900");
        }

        // El asiento del abono sale al revés: 400 al debe, 600 y 472 al haber (sin importes negativos).
        var asientos = (await api.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2026"))!.Where(a => a.Origen == "Compra").ToList();
        asientos.Should().HaveCount(2);
        var inverso = asientos.Single(a => a.Total == 242m);
        inverso.Apuntes.Should().OnlyContain(a => a.Debe >= 0m && a.Haber >= 0m)
            .And.Contain(a => a.CuentaCodigo == "400" && a.Debe == 242m).And.Contain(a => a.CuentaCodigo == "472" && a.Haber == 42m);

        // El 303 deduce la diferencia y el SII la declara como R1 por diferencias, con la factura rectificada.
        using (var resumen = JsonDocument.Parse(await api.GetStringAsync("/informes/resumen-trimestral?anio=2026&trimestre=3")))
        {
            resumen.RootElement.GetProperty("modelo303").GetProperty("ivaDeducibleCuota").GetDecimal().Should().Be(168m);
        }

        var sii = await api.GetStringAsync("/informes/sii?tipo=Recibidas&ejercicio=2026&periodo=9");
        sii.Should().Contain("<siiLR:TipoFactura>R1</siiLR:TipoFactura>").And.Contain("<siiLR:TipoRectificativa>I</siiLR:TipoRectificativa>")
            .And.Contain("<siiLR:ImporteTotal>-242.00</siiLR:ImporteTotal>").And.Contain("FacturasRectificadas");
    }

    [Fact]
    public async Task En_recargo_de_equivalencia_el_iva_soportado_es_coste()
    {
        var api = await Ayudas.AutenticadoAsync(_fabrica);
        var crear = await api.PostAsJsonAsync("/empresas", new { Nif = Ayudas.GenerarNif(), RazonSocial = "Frutería Minorista SL", RegimenIva = "RecargoEquivalencia" });
        crear.IsSuccessStatusCode.Should().BeTrue(await crear.Content.ReadAsStringAsync());
        var empresaId = (await crear.Content.ReadFromJsonAsync<IdResp>())!.Id;
        using (var sel = JsonDocument.Parse(await (await api.PostAsync(new Uri($"/empresas/{empresaId}/seleccionar", UriKind.Relative), null)).Content.ReadAsStringAsync()))
        {
            api.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", sel.RootElement.GetProperty("token").GetString());
        }

        (await api.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();
        (await api.PutAsJsonAsync("/contabilidad/contabilizacion-automatica", new { Automatica = true })).EnsureSuccessStatusCode();
        (await api.GetAsync(new Uri("/tipos-iva", UriKind.Relative))).EnsureSuccessStatusCode();
        var proveedor = await IdAsync(api, "/proveedores", new { Nombre = "Mayorista Frutas SA", NifFiscal = "B12345674" });

        var r = await api.PostAsJsonAsync("/gastos", new
        {
            ProveedorId = proveedor, NumeroFactura = "M-1", FechaFactura = "2026-09-05", Fecha = "2026-09-05", RecargoEquivalencia = true,
            Lineas = new[] { new { Descripcion = "Fruta", Base = 1000m, CodigoIva = "IVA10" } },
        });
        r.StatusCode.Should().Be(HttpStatusCode.Created, await r.Content.ReadAsStringAsync());
        var g = (await r.Content.ReadFromJsonAsync<GastoResp>())!;
        g.Total.Should().Be(1114m, "1.000 + 100 de IVA + 14 de recargo (1,4 %)");
        g.Desglose.Should().ContainSingle().Which.CuotaDeducible.Should().Be(0m);

        var asiento = (await api.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2026"))!.Single(a => a.Origen == "Compra");
        asiento.Apuntes.Should().NotContain(a => a.CuentaCodigo.StartsWith("472"));
        asiento.Apuntes.Where(a => a.CuentaCodigo.StartsWith('6')).Sum(a => a.Debe).Should().Be(1114m);
    }
}
