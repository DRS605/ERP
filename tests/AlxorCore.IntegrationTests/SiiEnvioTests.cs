using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Envío del SII con una AEAT simulada (<see cref="TransporteSiiFalso"/>): certificado de la empresa guardado cifrado;
/// alta (A0) de lo pendiente; lo aceptado no se reenvía; un rechazo se reenvía como alta; un gasto corregido después de
/// aceptarse va como modificación (A1); una factura anulada ya enviada se da de baja; y las autofacturas REAGP llevan la
/// clave 02 con la compensación.
/// </summary>
public sealed class SiiEnvioTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public SiiEnvioTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record CertificadoResp(string Titular, string? Nif, string Entorno, bool Caducado);
    private sealed record EnvioResp(Guid Id, string TipoComunicacion, int Registros, int Correctos, int ConErrores, int Incorrectos, string Estado, string? Csv);
    private sealed record SituacionResp(Guid DocumentoId, string Numero, string Situacion, string? Estado, string? CodigoError);
    private sealed record ResultadoResp(List<EnvioResp> Envios, List<SituacionResp> Situacion);

    private static string Pfx(string clave)
    {
        using var rsa = RSA.Create(2048);
        var peticion = new CertificateRequest("CN=EMPRESA PRUEBAS SL, SERIALNUMBER=B12345674, C=ES", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var cert = peticion.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));
        return Convert.ToBase64String(cert.Export(X509ContentType.Pfx, clave));
    }

    private static async Task<T> OkAsync<T>(HttpResponseMessage r)
    {
        r.IsSuccessStatusCode.Should().BeTrue(await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<T>())!;
    }

    private static async Task<string> CodigoAsync(HttpResponseMessage r) => (await r.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo;

    [Fact]
    public async Task Envia_lo_pendiente_reenvia_rechazos_modifica_y_da_de_baja()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.GetAsync(new Uri("/tipos-iva", UriKind.Relative))).EnsureSuccessStatusCode();
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var (anio, mes) = (hoy.Year, hoy.Month);

        // Sin certificado no se envía; con una contraseña errónea no se guarda.
        (await CodigoAsync(await api.PostAsJsonAsync("/informes/sii/enviar", new { Libro = "Emitidas", Ejercicio = anio, Periodo = mes }))).Should().Be("sii.sin_certificado");
        (await CodigoAsync(await api.PutAsJsonAsync("/informes/sii/certificado", new { PfxBase64 = Pfx("buena"), Clave = "mala" }))).Should().Be("sii.certificado_clave");
        var cert = await OkAsync<CertificadoResp>(await api.PutAsJsonAsync("/informes/sii/certificado", new { PfxBase64 = Pfx("buena"), Clave = "buena", Entorno = "Pruebas" }));
        cert.Should().Match<CertificadoResp>(c => c.Titular == "EMPRESA PRUEBAS SL" && c.Nif == "B12345674" && c.Entorno == "Pruebas" && !c.Caducado);

        // Emitidas: dos facturas del mes.
        var cliente = (await (await api.PostAsJsonAsync("/clientes", new { Nombre = "Cliente SII SL", NifFiscal = "B12345674" })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        var facturas = new List<Guid>();
        foreach (var precio in new[] { 100m, 250m })
        {
            facturas.Add((await OkAsync<IdResp>(await api.PostAsJsonAsync("/facturas", new { ClienteId = cliente, Lineas = new[] { new { Descripcion = "Servicio", Cantidad = 1m, PrecioUnitario = precio, CodigoIva = "IVA21" } } }))).Id);
        }

        var situacion = await OkAsync<List<SituacionResp>>(await api.GetAsync(new Uri($"/informes/sii/situacion?libro=Emitidas&ejercicio={anio}&periodo={mes}", UriKind.Relative)));
        situacion.Should().HaveCount(2).And.OnlyContain(s => s.Situacion == "Pendiente");

        var primero = await OkAsync<ResultadoResp>(await api.PostAsJsonAsync("/informes/sii/enviar", new { Libro = "Emitidas", Ejercicio = anio, Periodo = mes }));
        primero.Envios.Should().ContainSingle().Which.Should().Match<EnvioResp>(e => e.TipoComunicacion == "A0" && e.Registros == 2 && e.Correctos == 2 && e.Estado == "Correcto" && e.Csv != null);
        primero.Situacion.Should().OnlyContain(s => s.Situacion == "Enviada" && s.Estado == "Correcto");
        TransporteSiiFalso.Recibidos.Last().Sobre.Should().Contain("<siiLR:SuministroLRFacturasEmitidas").And.Contain(">A0<");

        // Lo aceptado y sin cambios no se reenvía.
        (await CodigoAsync(await api.PostAsJsonAsync("/informes/sii/enviar", new { Libro = "Emitidas", Ejercicio = anio, Periodo = mes }))).Should().Be("sii.nada_que_enviar");

        // Anulada una factura ya enviada, se da de baja.
        (await api.PostAsJsonAsync($"/facturas/{facturas[0]}/anular", new { Motivo = "Error" })).EnsureSuccessStatusCode();
        var baja = await OkAsync<ResultadoResp>(await api.PostAsJsonAsync("/informes/sii/enviar", new { Libro = "Emitidas", Ejercicio = anio, Periodo = mes }));
        baja.Envios.Should().ContainSingle().Which.Should().Match<EnvioResp>(e => e.TipoComunicacion == "B" && e.Registros == 1 && e.Correctos == 1);
        baja.Situacion.Single(s => s.DocumentoId == facturas[0]).Situacion.Should().Be("Dada de baja");
        TransporteSiiFalso.Recibidos.Last().Sobre.Should().Contain("BajaLRFacturasEmitidas").And.NotContain("TipoComunicacion");

        // Recibidas: una aceptada, una rechazada (NIF no identificado) y una autofactura REAGP.
        var proveedor = (await (await api.PostAsJsonAsync("/proveedores", new { Nombre = "Suministros SL", NifFiscal = "B87654321" })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        var agricultor = (await (await api.PostAsJsonAsync("/proveedores", new { Nombre = "José Agricultor", NifFiscal = "22222222J" })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        var bueno = await OkAsync<IdResp>(await api.PostAsJsonAsync("/gastos", new { ProveedorId = proveedor, Concepto = "Cajas", NumeroFactura = "F-100", Lineas = new[] { new { Base = 1000m, CodigoIva = "IVA21" } } }));
        await OkAsync<IdResp>(await api.PostAsJsonAsync("/gastos", new { ProveedorId = proveedor, Concepto = "Film", NumeroFactura = "RECHAZO-7", Lineas = new[] { new { Base = 200m, CodigoIva = "IVA21" } } }));
        await OkAsync<IdResp>(await api.PostAsJsonAsync("/gastos", new { ProveedorId = agricultor, Concepto = "Tomate", NumeroFactura = "LIQ-1", Lineas = new[] { new { Base = 5000m, CodigoIva = "REAGP12" } } }));

        var recibidas = await OkAsync<ResultadoResp>(await api.PostAsJsonAsync("/informes/sii/enviar", new { Libro = "Recibidas", Ejercicio = anio, Periodo = mes }));
        recibidas.Envios.Should().ContainSingle().Which.Should().Match<EnvioResp>(e => e.Registros == 3 && e.Correctos == 2 && e.Incorrectos == 1 && e.Estado == "ParcialmenteCorrecto");
        recibidas.Situacion.Single(s => s.Numero == "RECHAZO-7").Should().Match<SituacionResp>(s => s.Situacion == "Rechazada: corregir y reenviar" && s.CodigoError == "1117");
        var sobreRecibidas = TransporteSiiFalso.Recibidos.Last(r => r.Sobre.Contains("LIQ-1", StringComparison.Ordinal)).Sobre;
        sobreRecibidas.Should().Contain("<siiLR:ClaveRegimenEspecialOTrascendencia>02</siiLR:ClaveRegimenEspecialOTrascendencia>")
            .And.Contain("<siiLR:PorcentCompensacionREAGYP>12</siiLR:PorcentCompensacionREAGYP>")
            .And.Contain("<siiLR:ImporteCompensacionREAGYP>600.00</siiLR:ImporteCompensacionREAGYP>");

        // Corregido el gasto aceptado, va como modificación (A1); el rechazado vuelve a ir como alta (A0).
        (await api.PutAsJsonAsync($"/gastos/{bueno.Id}", new { ProveedorId = proveedor, Concepto = "Cajas", NumeroFactura = "F-100", Lineas = new[] { new { Base = 1100m, CodigoIva = "IVA21" } } }))
            .EnsureSuccessStatusCode();
        var segundo = await OkAsync<ResultadoResp>(await api.PostAsJsonAsync("/informes/sii/enviar", new { Libro = "Recibidas", Ejercicio = anio, Periodo = mes }));
        segundo.Envios.Select(e => (e.TipoComunicacion, e.Registros)).Should().BeEquivalentTo([("A0", 1), ("A1", 1)]);
        segundo.Situacion.Single(s => s.DocumentoId == bueno.Id).Situacion.Should().Be("Enviada");

        // Histórico con petición y respuesta descargables.
        var envios = await OkAsync<List<EnvioResp>>(await api.GetAsync(new Uri("/informes/sii/envios", UriKind.Relative)));
        envios.Should().HaveCount(5);
        var xml = await api.GetStringAsync($"/informes/sii/envios/{envios[0].Id}/xml?parte=respuesta");
        xml.Should().Contain("EstadoEnvio");
    }
}
