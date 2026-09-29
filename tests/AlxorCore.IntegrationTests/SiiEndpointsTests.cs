using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de la generación del XML del SII (facturas emitidas y recibidas).</summary>
public sealed class SiiEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public SiiEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);

    [Fact]
    public async Task El_sii_de_emitidas_incluye_la_factura_del_periodo()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var clienteId = (await (await cliente.PostAsJsonAsync("/clientes", new { Nombre = "Cliente SII SL", NifFiscal = "B12345674" })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        await cliente.PostAsJsonAsync("/facturas", new
        {
            ClienteId = clienteId,
            FechaEmision = "2026-03-15",
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = 1000m, CodigoIva = "IVA21" } },
        });

        var r = await cliente.GetAsync("/informes/sii?tipo=Emitidas&ejercicio=2026&periodo=3");
        r.StatusCode.Should().Be(HttpStatusCode.OK);
        r.Content.Headers.ContentType!.MediaType.Should().Be("application/xml");
        var xml = await r.Content.ReadAsStringAsync();

        xml.Should().Contain("SuministroLRFacturasEmitidas");
        xml.Should().Contain("RegistroLRFacturasEmitidas");
        xml.Should().Contain("B12345674");                 // NIF de la contraparte
        xml.Should().Contain("<siiLR:ImporteTotal");       // total de la factura presente
        xml.Should().Contain("1210.00");                   // 1000 + 21%
        xml.Should().Contain("21");                        // tipo impositivo
    }

    [Fact]
    public async Task El_sii_de_recibidas_incluye_el_gasto_del_periodo()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await cliente.PostAsJsonAsync("/gastos", new { Concepto = "Suministros marzo", BaseImponible = 200m, CodigoIva = "IVA21", Fecha = "2026-03-10" });

        var r = await cliente.GetAsync("/informes/sii?tipo=Recibidas&ejercicio=2026&periodo=3");
        r.StatusCode.Should().Be(HttpStatusCode.OK);
        var xml = await r.Content.ReadAsStringAsync();
        xml.Should().Contain("SuministroLRFacturasRecibidas");
        xml.Should().Contain("Suministros marzo");
        xml.Should().Contain("CuotaDeducible");
    }

    [Fact]
    public async Task Un_periodo_invalido_da_error()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await cliente.GetAsync("/informes/sii?tipo=Emitidas&ejercicio=2026&periodo=13")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task El_sii_de_recibidas_identifica_al_proveedor_por_su_nif_y_omite_los_gastos_anulados()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var nif = Ayudas.GenerarNif();
        var proveedor = (await (await cliente.PostAsJsonAsync("/proveedores", new { Nombre = "Suministros del Sur SL", NifFiscal = nif })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        (await cliente.PostAsJsonAsync("/gastos", new { Concepto = "Material abril", BaseImponible = 100m, CodigoIva = "IVA10", ProveedorId = proveedor, Fecha = "2026-04-10" }))
            .IsSuccessStatusCode.Should().BeTrue();
        var anulado = (await (await cliente.PostAsJsonAsync("/gastos", new { Concepto = "Gasto erróneo", BaseImponible = 50m, CodigoIva = "IVA21", Fecha = "2026-04-11" })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        (await cliente.PostAsync(new Uri($"/gastos/{anulado}/anular", UriKind.Relative), null)).IsSuccessStatusCode.Should().BeTrue();

        var xml = await (await cliente.GetAsync("/informes/sii?tipo=Recibidas&ejercicio=2026&periodo=4")).Content.ReadAsStringAsync();
        var doc = System.Xml.Linq.XDocument.Parse(xml);
        System.Xml.Linq.XNamespace lr = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/ssii/fact/ws/SuministroLR.xsd";

        var registros = doc.Descendants(lr + "RegistroLRFacturasRecibidas").ToList();
        registros.Should().ContainSingle("el gasto anulado no se declara");
        var emisor = registros[0].Descendants(lr + "IDEmisorFactura").Single();
        emisor.Element(lr + "NIF")!.Value.Should().Be(nif);
        emisor.Element(lr + "NombreRazon").Should().BeNull("el emisor se identifica por NIF, no por nombre");
        var detalle = registros[0].Descendants(lr + "DetalleIVA").Single();
        detalle.Element(lr + "TipoImpositivo")!.Value.Should().Be("10");
        detalle.Element(lr + "CuotaSoportada")!.Value.Should().Be("10.00");
    }

    [Fact]
    public async Task El_sii_de_emitidas_desglosa_un_detalle_iva_por_tipo()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var clienteId = (await (await cliente.PostAsJsonAsync("/clientes", new { Nombre = "Cliente Mixto SL", NifFiscal = Ayudas.GenerarNif() })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        (await cliente.PostAsJsonAsync("/facturas", new
        {
            ClienteId = clienteId,
            FechaEmision = "2026-05-15",
            Lineas = new[]
            {
                new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = 1000m, CodigoIva = "IVA21" },
                new { Cantidad = 1m, Descripcion = "Alimentación", PrecioUnitario = 200m, CodigoIva = "IVA10" },
            },
        })).IsSuccessStatusCode.Should().BeTrue();

        var xml = await (await cliente.GetAsync("/informes/sii?tipo=Emitidas&ejercicio=2026&periodo=5")).Content.ReadAsStringAsync();
        var doc = System.Xml.Linq.XDocument.Parse(xml);
        System.Xml.Linq.XNamespace lr = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/ssii/fact/ws/SuministroLR.xsd";

        var detalles = doc.Descendants(lr + "DetalleIVA").ToList();
        detalles.Should().HaveCount(2, "una factura con dos tipos lleva dos DetalleIVA, no un tipo medio");
        detalles.Select(d => d.Element(lr + "TipoImpositivo")!.Value).Should().BeEquivalentTo("21", "10");
        detalles.Single(d => d.Element(lr + "TipoImpositivo")!.Value == "21").Element(lr + "CuotaRepercutida")!.Value.Should().Be("210.00");
        detalles.Single(d => d.Element(lr + "TipoImpositivo")!.Value == "10").Element(lr + "BaseImponible")!.Value.Should().Be("200.00");
    }

    [Fact]
    public async Task Las_exportaciones_y_las_exentas_llevan_su_clave_su_causa_y_la_contraparte_extranjera()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await cliente.GetAsync(new Uri("/tipos-iva", UriKind.Relative))).EnsureSuccessStatusCode();
        var extranjero = (await (await cliente.PostAsJsonAsync("/clientes", new { Nombre = "Fresh Imports Inc", NifFiscal = "US987654321", Pais = "US" }))
            .Content.ReadFromJsonAsync<IdResp>())!.Id;
        var nacional = (await (await cliente.PostAsJsonAsync("/clientes", new { Nombre = "Academia SL", NifFiscal = Ayudas.GenerarNif() })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        (await cliente.PostAsJsonAsync("/facturas", new
        {
            ClienteId = extranjero, FechaEmision = "2026-06-10",
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Naranjas", PrecioUnitario = 5000m, CodigoIva = "EXPORT" } },
        })).IsSuccessStatusCode.Should().BeTrue();
        (await cliente.PostAsJsonAsync("/facturas", new
        {
            ClienteId = nacional, FechaEmision = "2026-06-11",
            Lineas = new[]
            {
                new { Cantidad = 1m, Descripcion = "Curso", PrecioUnitario = 300m, CodigoIva = "IVA0" },
                new { Cantidad = 1m, Descripcion = "Material", PrecioUnitario = 100m, CodigoIva = "IVA21" },
            },
        })).IsSuccessStatusCode.Should().BeTrue();

        var xml = await (await cliente.GetAsync("/informes/sii?tipo=Emitidas&ejercicio=2026&periodo=6")).Content.ReadAsStringAsync();
        var doc = System.Xml.Linq.XDocument.Parse(xml);
        System.Xml.Linq.XNamespace lr = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/ssii/fact/ws/SuministroLR.xsd";
        var registros = doc.Descendants(lr + "RegistroLRFacturasEmitidas").ToList();
        registros.Should().HaveCount(2);

        var export = registros.Single(r => r.Descendants(lr + "NombreRazon").Any(n => n.Value == "Fresh Imports Inc"));
        export.Descendants(lr + "ClaveRegimenEspecialOTrascendencia").Single().Value.Should().Be("02");
        export.Descendants(lr + "IDOtro").Single().Element(lr + "CodigoPais")!.Value.Should().Be("US");
        export.Descendants(lr + "IDOtro").Single().Element(lr + "IDType")!.Value.Should().Be("04");
        export.Descendants(lr + "DesgloseTipoOperacion").Should().ContainSingle();
        export.Descendants(lr + "CausaExencion").Single().Value.Should().Be("E2");

        var mixta = registros.Single(r => r.Descendants(lr + "NombreRazon").Any(n => n.Value == "Academia SL"));
        mixta.Descendants(lr + "ClaveRegimenEspecialOTrascendencia").Single().Value.Should().Be("01");
        mixta.Descendants(lr + "DesgloseFactura").Should().ContainSingle();
        var exenta = mixta.Descendants(lr + "DetalleExenta").Single();
        exenta.Element(lr + "CausaExencion")!.Value.Should().Be("E1");
        exenta.Element(lr + "BaseImponible")!.Value.Should().Be("300.00");
        mixta.Descendants(lr + "DetalleIVA").Single().Element(lr + "CuotaRepercutida")!.Value.Should().Be("21.00");
    }
}
