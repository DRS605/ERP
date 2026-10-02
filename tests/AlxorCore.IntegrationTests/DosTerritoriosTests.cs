using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Un mismo NIF con actividad en la Península y en Canarias: cada factura va con IVA o con IGIC según sus tipos, el 303 suma
/// solo las del IVA y el 420 solo las del IGIC, y una factura nunca mezcla los dos. Sin marcarlo, la empresa solo factura con
/// el impuesto de su territorio.
/// </summary>
public sealed class DosTerritoriosTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public DosTerritoriosTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record FacturaResp(Guid Id, string Impuesto, decimal BaseImponible);
    private sealed record M303(decimal IvaDevengadoBase);
    private sealed record ResumenResp(M303 Modelo303);
    private sealed record M420(decimal DevengadoBase);
    private sealed record TipoResp(string Codigo, string Impuesto);
    private sealed record FichaResp(List<string> Sugeridos);
    private sealed record LineaResp(string CodigoIva);
    private sealed record FacturaLineasResp(string Impuesto, List<LineaResp> Lineas);
    private sealed record ProrrataResp(string? Regimen, int PorcentajeProvisional);
    private sealed record M303P(int PorcentajeProrrata);
    private sealed record ResumenPResp(M303P Modelo303);
    private sealed record M420P(int PorcentajeProrrata);

    private static Task<HttpResponseMessage> FacturarAsync(HttpClient api, Guid cliente, string codigo, decimal precio) =>
        api.PostAsJsonAsync("/facturas", new { ClienteId = cliente, Lineas = new[] { new { Cantidad = 1m, Descripcion = "Fruta", PrecioUnitario = precio, CodigoIva = codigo } } });

    [Fact]
    public async Task Factura_con_iva_o_con_igic_y_cada_modelo_suma_lo_suyo()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var cliente = (await (await api.PostAsJsonAsync("/clientes", new { Nombre = "Mercado SL", NifFiscal = Ayudas.GenerarNif() })).Content.ReadFromJsonAsync<FacturaResp>())!.Id;

        // Sin marcar los dos territorios, una línea con IGIC no se acepta en una empresa de la Península.
        (await FacturarAsync(api, cliente, "IGIC7", 100m)).StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await api.PutAsJsonAsync("/empresas/actual/perfil-fiscal", new { Perfil = new { OperaEnAmbosTerritorios = true, Modelos = new[] { "303", "420" } } }))
            .IsSuccessStatusCode.Should().BeTrue();
        var tipos = (await api.GetFromJsonAsync<List<TipoResp>>("/tipos-iva"))!;
        tipos.Should().Contain(t => t.Impuesto == "Igic").And.Contain(t => t.Impuesto == "Iva");
        (await api.GetFromJsonAsync<FichaResp>("/empresas/actual/perfil-fiscal"))!.Sugeridos.Should().Contain(["303", "420", "425"]);

        var peninsula = await FacturarAsync(api, cliente, "IVA21", 100m);
        peninsula.IsSuccessStatusCode.Should().BeTrue(await peninsula.Content.ReadAsStringAsync());
        (await peninsula.Content.ReadFromJsonAsync<FacturaResp>())!.Impuesto.Should().Be("Iva");
        var canarias = await FacturarAsync(api, cliente, "IGIC7", 300m);
        canarias.IsSuccessStatusCode.Should().BeTrue(await canarias.Content.ReadAsStringAsync());
        (await canarias.Content.ReadFromJsonAsync<FacturaResp>())!.Impuesto.Should().Be("Igic");

        // Una factura no mezcla los dos impuestos.
        var mixta = await api.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente,
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "A", PrecioUnitario = 10m, CodigoIva = "IGIC7" }, new { Cantidad = 1m, Descripcion = "B", PrecioUnitario = 10m, CodigoIva = "IVA21" } },
        });
        mixta.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var anio = DateTime.Today.Year;
        var trimestre = (DateTime.Today.Month - 1) / 3 + 1;
        (await api.GetFromJsonAsync<ResumenResp>($"/informes/resumen-trimestral?anio={anio}&trimestre={trimestre}"))!.Modelo303.IvaDevengadoBase.Should().Be(100m);
        (await api.GetFromJsonAsync<M420>($"/impuestos/modelo-420?anio={anio}&trimestre={trimestre}"))!.DevengadoBase.Should().Be(300m);
    }

    [Fact]
    public async Task El_territorio_se_elige_en_la_factura_los_articulos_pasan_a_igic_y_cada_impuesto_tiene_su_prorrata()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var cliente = (await (await api.PostAsJsonAsync("/clientes", new { Nombre = "Cliente SL", NifFiscal = Ayudas.GenerarNif() })).Content.ReadFromJsonAsync<FacturaResp>())!.Id;
        var fruta = (await (await api.PostAsJsonAsync("/productos", new { Nombre = "Naranja", PrecioUnitario = 1m, Tipo = "Bien", Unidad = "kg", CodigoIva = "IVA4" })).Content.ReadFromJsonAsync<FacturaResp>())!.Id;
        var vino = await api.PostAsJsonAsync("/productos", new { Nombre = "Vino", PrecioUnitario = 5m, Tipo = "Bien", Unidad = "ud", CodigoIva = "IVA21", CodigoIgic = "IGIC3" });
        vino.IsSuccessStatusCode.Should().BeTrue(await vino.Content.ReadAsStringAsync());
        var vinoId = (await vino.Content.ReadFromJsonAsync<FacturaResp>())!.Id;
        (await api.PostAsJsonAsync("/productos", new { Nombre = "Malo", PrecioUnitario = 1m, CodigoIva = "IVA21", CodigoIgic = "IVA10" })).StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Elegir IGIC en una empresa que no opera en Canarias se rechaza.
        (await api.PostAsJsonAsync("/facturas", new { ClienteId = cliente, Impuesto = "Igic", Lineas = new[] { new { Cantidad = 1m, ProductoId = fruta } } }))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await api.PutAsJsonAsync("/empresas/actual/perfil-fiscal", new { Perfil = new { OperaEnAmbosTerritorios = true } })).IsSuccessStatusCode.Should().BeTrue();

        // Factura de Canarias: la fruta (IVA 4 %) va al IGIC 0 % y el vino al IGIC 3 % que tiene puesto.
        var r = await api.PostAsJsonAsync("/facturas", new { ClienteId = cliente, Impuesto = "Igic", Lineas = new[] { new { Cantidad = 10m, ProductoId = fruta }, new { Cantidad = 2m, ProductoId = vinoId } } });
        r.IsSuccessStatusCode.Should().BeTrue(await r.Content.ReadAsStringAsync());
        var f = (await r.Content.ReadFromJsonAsync<FacturaLineasResp>())!;
        f.Impuesto.Should().Be("Igic");
        f.Lineas.Select(l => l.CodigoIva).Should().Equal("IGIC0", "IGIC3");

        // Y de la Península: los mismos artículos con su IVA.
        var p = (await (await api.PostAsJsonAsync("/facturas", new { ClienteId = cliente, Impuesto = "Iva", Lineas = new[] { new { Cantidad = 10m, ProductoId = fruta } } }))
            .Content.ReadFromJsonAsync<FacturaLineasResp>())!;
        (p.Impuesto, p.Lineas[0].CodigoIva).Should().Be(("Iva", "IVA4"));

        // Prorrata solo en el IGIC: el 420 la aplica y el 303 no.
        var anio = DateTime.Today.Year;
        (await api.PutAsJsonAsync($"/impuestos/prorrata/{anio}", new { Regimen = "General", PorcentajeProvisional = 60, Impuesto = "Igic" })).IsSuccessStatusCode.Should().BeTrue();
        (await api.GetFromJsonAsync<ProrrataResp>($"/impuestos/prorrata?ejercicio={anio}&impuesto=Igic"))!.Regimen.Should().Be("General");
        (await api.GetFromJsonAsync<ProrrataResp>($"/impuestos/prorrata?ejercicio={anio}&impuesto=Iva"))!.Regimen.Should().BeNull();
        var trimestre = (DateTime.Today.Month - 1) / 3 + 1;
        (await api.GetFromJsonAsync<M420P>($"/impuestos/modelo-420?anio={anio}&trimestre={trimestre}"))!.PorcentajeProrrata.Should().Be(60);
        (await api.GetFromJsonAsync<ResumenPResp>($"/informes/resumen-trimestral?anio={anio}&trimestre={trimestre}"))!.Modelo303.PorcentajeProrrata.Should().Be(100);
    }

    private sealed record DocResp(Guid Id, List<LineaResp> Lineas);

    [Fact]
    public async Task Presupuestos_pedidos_y_albaranes_van_con_el_impuesto_del_territorio_y_pasan_a_la_factura()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var cliente = (await (await api.PostAsJsonAsync("/clientes", new { Nombre = "Cliente SL", NifFiscal = Ayudas.GenerarNif() })).Content.ReadFromJsonAsync<FacturaResp>())!.Id;
        var fruta = (await (await api.PostAsJsonAsync("/productos", new { Nombre = "Naranja", PrecioUnitario = 1m, Tipo = "Bien", Unidad = "kg", CodigoIva = "IVA4" })).Content.ReadFromJsonAsync<FacturaResp>())!.Id;
        var lineas = new[] { new { Cantidad = 10m, ProductoId = fruta, Descripcion = "Naranja", PrecioUnitario = 1m } };

        // Sin operar en Canarias, ningún documento se hace con IGIC.
        (await api.PostAsJsonAsync("/presupuestos", new { ClienteId = cliente, Impuesto = "Igic", Lineas = lineas })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await api.PostAsJsonAsync("/pedidos-venta", new { ClienteId = cliente, Impuesto = "Igic", Lineas = lineas })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await api.PostAsJsonAsync("/albaranes-venta", new { ClienteId = cliente, Impuesto = "Igic", Lineas = lineas })).StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await api.PutAsJsonAsync("/empresas/actual/perfil-fiscal", new { Perfil = new { OperaEnAmbosTerritorios = true } })).IsSuccessStatusCode.Should().BeTrue();

        // Presupuesto de Canarias: la fruta va al IGIC 0 % y al aceptarlo sale una factura con IGIC.
        var pr = await api.PostAsJsonAsync("/presupuestos", new { ClienteId = cliente, Impuesto = "Igic", Lineas = lineas });
        pr.IsSuccessStatusCode.Should().BeTrue(await pr.Content.ReadAsStringAsync());
        var presupuesto = (await pr.Content.ReadFromJsonAsync<DocResp>())!;
        presupuesto.Lineas.Select(l => l.CodigoIva).Should().Equal("IGIC0");
        var aceptado = await api.PostAsJsonAsync($"/presupuestos/{presupuesto.Id}/aceptar", new { });
        aceptado.IsSuccessStatusCode.Should().BeTrue(await aceptado.Content.ReadAsStringAsync());
        (await aceptado.Content.ReadFromJsonAsync<FacturaLineasResp>())!.Impuesto.Should().Be("Igic");

        // Un tipo del IVA en un documento con IGIC se rechaza.
        (await api.PostAsJsonAsync("/pedidos-venta", new { ClienteId = cliente, Impuesto = "Igic", Lineas = new[] { new { Cantidad = 1m, Descripcion = "X", PrecioUnitario = 1m, CodigoIva = "IVA21" } } }))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Pedido de Canarias: IGIC 0 %; y de la Península sin elegir: el IVA del artículo.
        var pe = await api.PostAsJsonAsync("/pedidos-venta", new { ClienteId = cliente, Impuesto = "Igic", Lineas = lineas });
        pe.IsSuccessStatusCode.Should().BeTrue(await pe.Content.ReadAsStringAsync());
        (await pe.Content.ReadFromJsonAsync<DocResp>())!.Lineas.Select(l => l.CodigoIva).Should().Equal("IGIC0");
        var peIva = await api.PostAsJsonAsync("/pedidos-venta", new { ClienteId = cliente, Lineas = lineas });
        (await peIva.Content.ReadFromJsonAsync<DocResp>())!.Lineas.Select(l => l.CodigoIva).Should().Equal("IVA4");

        // Albarán de Canarias facturado: factura con IGIC.
        var al = await api.PostAsJsonAsync("/albaranes-venta", new { ClienteId = cliente, Impuesto = "Igic", Lineas = lineas });
        al.IsSuccessStatusCode.Should().BeTrue(await al.Content.ReadAsStringAsync());
        var albaran = (await al.Content.ReadFromJsonAsync<DocResp>())!;
        albaran.Lineas.Select(l => l.CodigoIva).Should().Equal("IGIC0");
        var fa = await api.PostAsJsonAsync("/albaranes-venta/facturar", new { AlbaranIds = new[] { albaran.Id } });
        fa.IsSuccessStatusCode.Should().BeTrue(await fa.Content.ReadAsStringAsync());
        (await fa.Content.ReadFromJsonAsync<FacturaLineasResp>())!.Impuesto.Should().Be("Igic");
    }

    private sealed record EmpresaResp(string Nif);
    private sealed record SituacionSiiResp(string Numero, string Situacion);
    private sealed record EnvioSiiResp(string TipoComunicacion, int Registros, int Correctos);
    private sealed record ResultadoSiiResp(List<EnvioSiiResp> Envios);
    private sealed record ProblemaResp(string Codigo);

    private static string Pfx()
    {
        using var rsa = System.Security.Cryptography.RSA.Create(2048);
        var peticion = new System.Security.Cryptography.X509Certificates.CertificateRequest("CN=EMPRESA DOS TERRITORIOS SL", rsa,
            System.Security.Cryptography.HashAlgorithmName.SHA256, System.Security.Cryptography.RSASignaturePadding.Pkcs1);
        using var cert = peticion.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));
        return Convert.ToBase64String(cert.Export(System.Security.Cryptography.X509Certificates.X509ContentType.Pfx, "clave"));
    }

    [Fact]
    public async Task Lo_del_igic_va_al_sii_de_la_agencia_tributaria_canaria_y_lo_del_iva_al_de_la_aeat()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var (anio, mes) = (hoy.Year, hoy.Month);
        var nif = (await api.GetFromJsonAsync<EmpresaResp>("/empresas/actual"))!.Nif;
        var cliente = (await (await api.PostAsJsonAsync("/clientes", new { Nombre = "Cliente SII SL", NifFiscal = Ayudas.GenerarNif() })).Content.ReadFromJsonAsync<FacturaResp>())!.Id;

        // Una empresa solo de la Península no lleva el SII de la ATC.
        var r = await api.GetAsync(new Uri($"/informes/sii/situacion?libro=Emitidas&ejercicio={anio}&periodo={mes}&administracion=Atc", UriKind.Relative));
        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await r.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("sii.administracion");

        (await api.PutAsJsonAsync("/empresas/actual/perfil-fiscal", new { Perfil = new { OperaEnAmbosTerritorios = true } })).IsSuccessStatusCode.Should().BeTrue();
        (await api.PutAsJsonAsync("/informes/sii/certificado", new { PfxBase64 = Pfx(), Clave = "clave", Entorno = "Pruebas" })).IsSuccessStatusCode.Should().BeTrue();
        (await FacturarAsync(api, cliente, "IVA21", 100m)).IsSuccessStatusCode.Should().BeTrue();
        (await FacturarAsync(api, cliente, "IGIC7", 300m)).IsSuccessStatusCode.Should().BeTrue();
        var proveedor = (await (await api.PostAsJsonAsync("/proveedores", new { Nombre = "Envases Canarios SL", NifFiscal = Ayudas.GenerarNif() })).Content.ReadFromJsonAsync<FacturaResp>())!.Id;
        var gasto = await api.PostAsJsonAsync("/gastos", new { ProveedorId = proveedor, Concepto = "Cajas", NumeroFactura = "EC-1", Lineas = new[] { new { Base = 500m, CodigoIva = "IGIC7" } } });
        gasto.IsSuccessStatusCode.Should().BeTrue(await gasto.Content.ReadAsStringAsync());

        // Cada libro lleva lo de su impuesto.
        var aeat = (await api.GetFromJsonAsync<List<SituacionSiiResp>>($"/informes/sii/situacion?libro=Emitidas&ejercicio={anio}&periodo={mes}"))!;
        var atc = (await api.GetFromJsonAsync<List<SituacionSiiResp>>($"/informes/sii/situacion?libro=Emitidas&ejercicio={anio}&periodo={mes}&administracion=Atc"))!;
        aeat.Should().ContainSingle();
        atc.Should().ContainSingle().Which.Numero.Should().NotBe(aeat[0].Numero);
        (await api.GetFromJsonAsync<List<SituacionSiiResp>>($"/informes/sii/situacion?libro=Recibidas&ejercicio={anio}&periodo={mes}"))!.Should().BeEmpty();
        (await api.GetFromJsonAsync<List<SituacionSiiResp>>($"/informes/sii/situacion?libro=Recibidas&ejercicio={anio}&periodo={mes}&administracion=Atc"))!
            .Should().ContainSingle().Which.Numero.Should().Be("EC-1");

        // El XML del SII-IGIC lleva los esquemas de la ATC, no los de la AEAT.
        var xml = await api.GetStringAsync($"/informes/sii?tipo=Emitidas&ejercicio={anio}&periodo={mes}&administracion=Atc");
        xml.Should().Contain("https://atc.prueba.invalid/sii-igic/ws/SuministroLR.xsd").And.NotContain("agenciatributaria").And.Contain(atc[0].Numero);

        // Envío a la ATC: va a su servicio y queda registrado aparte del de la AEAT.
        var envio = await api.PostAsJsonAsync("/informes/sii/enviar", new { Libro = "Emitidas", Ejercicio = anio, Periodo = mes, Administracion = "Atc" });
        envio.IsSuccessStatusCode.Should().BeTrue(await envio.Content.ReadAsStringAsync());
        (await envio.Content.ReadFromJsonAsync<ResultadoSiiResp>())!.Envios.Should().ContainSingle().Which.Should().Match<EnvioSiiResp>(e => e.TipoComunicacion == "A0" && e.Correctos == 1);
        var destino = TransporteSiiFalso.Destinos.Last(d => d.Sobre.Contains(nif, StringComparison.Ordinal)).Destino;
        destino.Should().Be(new Uri("https://pruebas.atc.prueba.invalid/sii-igic/fe"));
        (await api.GetFromJsonAsync<List<SituacionSiiResp>>($"/informes/sii/situacion?libro=Emitidas&ejercicio={anio}&periodo={mes}"))!
            .Should().ContainSingle().Which.Situacion.Should().Be("Pendiente");

        var envioAeat = await api.PostAsJsonAsync("/informes/sii/enviar", new { Libro = "Emitidas", Ejercicio = anio, Periodo = mes });
        envioAeat.IsSuccessStatusCode.Should().BeTrue(await envioAeat.Content.ReadAsStringAsync());
        TransporteSiiFalso.Destinos.Last(d => d.Sobre.Contains(nif, StringComparison.Ordinal)).Destino.Host.Should().Be("prewww1.aeat.es");

        // Sin configurar el servicio de la ATC no hay dirección a la que enviar.
        new AlxorCore.Informes.Aplicacion.OpcionesSiiAtc().Url(AlxorCore.Informes.Aplicacion.TipoLibroSii.Emitidas, AlxorCore.Informes.Dominio.EntornoSii.Pruebas)
            .Should().BeNull();
    }
}
