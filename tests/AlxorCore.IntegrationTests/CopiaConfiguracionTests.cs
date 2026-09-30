using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Copia de la configuración de una empresa a otra: vista previa sin cambios, copia de lo que falta, repetir sin duplicar,
/// lo que solo se copia dentro del grupo y el acceso a la empresa de origen.
/// </summary>
public sealed class CopiaConfiguracionTests : IClassFixture<FabricaApiPruebas>
{
    private static readonly int Anio = DateTime.Today.Year;
    private static readonly string[] Todo =
        ["cuentas", "diarios", "formas_pago", "series", "almacenes", "transporte", "soportes", "agro_campanas", "agro_categorias", "agro_conceptos", "agro_taras"];

    private readonly FabricaApiPruebas _fabrica;

    public CopiaConfiguracionTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record GrupoResp(Guid Id);
    private sealed record SeleccionResp(string Token);
    private sealed record OrigenResp(Guid Id, string RazonSocial, bool MismoGrupo);
    private sealed record ElementoResp(string Clave, string Nombre);
    private sealed record CatalogoResp(List<ElementoResp> Elementos, List<OrigenResp> Origenes);
    private sealed record BloqueResp(string Clave, List<string> Nuevos, int YaExistian, List<string> Omitidos, List<string> Errores);
    private sealed record CopiaResp(bool Ejecutada, int Creados, List<BloqueResp> Elementos);
    private sealed record NombreResp(Guid Id, string Nombre);
    private sealed record CodigoResp(Guid? Id, string Codigo);
    private sealed record SerieResp(string Prefijo, int Ejercicio, long SiguienteNumero);
    private sealed record VehiculoResp(string Matricula, Guid? TransportistaId);
    private sealed record UbicacionResp(Guid AlmacenId, string Codigo);

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

    private static async Task ConAgroAsync(HttpClient api, Guid empresa)
    {
        (await api.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "completa", ModulosAdicionales = new[] { "agro" } })).EnsureSuccessStatusCode();
        await SeleccionarAsync(api, empresa);
    }

    private static async Task<CopiaResp> CopiarAsync(HttpClient api, Guid origen, bool ejecutar, params string[] elementos)
    {
        var r = await api.PostAsJsonAsync(ejecutar ? "/empresas/actual/copia" : "/empresas/actual/copia/vista-previa",
            new { OrigenEmpresaId = origen, Elementos = elementos.Length > 0 ? elementos : Todo });
        r.IsSuccessStatusCode.Should().BeTrue(await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<CopiaResp>())!;
    }

    private static BloqueResp Bloque(CopiaResp c, string clave) => c.Elementos.Single(e => e.Clave == clave);

    [Fact]
    public async Task Copia_lo_que_falta_sin_duplicar_y_respeta_el_grupo()
    {
        // Empresa A con su configuración.
        var (api, a) = await Ayudas.ConEmpresaAsync(_fabrica);
        await ConAgroAsync(api, a);
        var grupo = (await api.GetFromJsonAsync<GrupoResp>("/grupos/actual"))!.Id;
        await IdAsync(api, "/contabilidad/cuentas", new { Codigo = "43000099", Nombre = "Clientes de exportación" });
        await IdAsync(api, "/contabilidad/diarios", new { Codigo = "REG", Nombre = "Regularizaciones", Origenes = new[] { "Manual" } });
        await IdAsync(api, "/formas-pago", new { Nombre = "Giro a 90 días", GeneraVencimiento = true, DiasVencimiento = 90, RegistrarPagoAutomatico = false });
        await IdAsync(api, "/series", new { TipoDocumento = "Factura", Ejercicio = Anio, Prefijo = "EXP" });
        var almacen = await IdAsync(api, "/inventario/almacenes", new { Codigo = "CAM", Nombre = "Cámara frigorífica" });
        await IdAsync(api, "/inventario/ubicaciones", new { AlmacenId = almacen, Codigo = "C1", Nombre = "Cámara 1" });
        var transportista = await IdAsync(api, "/transporte/transportistas", new { Nombre = "Transportes Levante SL", Nif = Ayudas.GenerarNif() });
        await IdAsync(api, "/transporte/vehiculos", new { Matricula = "1234BCD", TaraKg = 14000m, Frigorifico = true, TransportistaId = transportista });
        await IdAsync(api, "/logistica/soportes", new { Codigo = "EUR", Nombre = "Europalé", LargoMm = 1200, AnchoMm = 800, AltoMm = 144, TaraKg = 25m });
        await IdAsync(api, "/agro/campanas", new { Codigo = $"C{Anio}", Nombre = "Campaña cítricos", Desde = new DateOnly(Anio, 1, 1), Hasta = new DateOnly(Anio, 12, 31) });
        await IdAsync(api, "/agro/categorias", new { Codigo = "EXTRA", Nombre = "Extra", Orden = 1 });
        await IdAsync(api, "/agro/conceptos", new { Codigo = "TRANS", Nombre = "Transporte", Tipo = "PorKilo", Valor = 0.01m });
        var box = await IdAsync(api, "/productos", new { Nombre = "Box plástico", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "ud" });
        await IdAsync(api, "/agro/taras", new { EnvaseProductoId = box, TaraKg = 28m, Desde = new DateOnly(Anio, 1, 1) });

        // B del mismo grupo y C de otro grupo.
        var b = await IdAsync(api, "/empresas", new { Nif = Ayudas.GenerarNif(), RazonSocial = "Hermana SL", GrupoId = grupo });
        var c = await IdAsync(api, "/empresas", new { Nif = Ayudas.GenerarNif(), RazonSocial = "Otra SL" });
        await SeleccionarAsync(api, b);
        await ConAgroAsync(api, b);

        var catalogo = (await api.GetFromJsonAsync<CatalogoResp>("/empresas/actual/copia"))!;
        catalogo.Elementos.Select(e => e.Clave).Should().BeEquivalentTo(Todo);
        catalogo.Origenes.Should().ContainSingle(o => o.Id == a).Which.MismoGrupo.Should().BeTrue();
        catalogo.Origenes.Should().ContainSingle(o => o.Id == c).Which.MismoGrupo.Should().BeFalse();
        catalogo.Origenes.Should().NotContain(o => o.Id == b);

        // La vista previa dice lo que se crearía y no cambia nada.
        var previa = await CopiarAsync(api, a, false);
        previa.Ejecutada.Should().BeFalse();
        Bloque(previa, "cuentas").Nuevos.Should().Contain("43000099 Clientes de exportación");
        Bloque(previa, "cuentas").YaExistian.Should().BePositive("el plan básico ya lo tienen las dos");
        Bloque(previa, "formas_pago").Nuevos.Should().Equal("Giro a 90 días");
        Bloque(previa, "almacenes").Nuevos.Should().Contain(n => n.StartsWith("CAM", StringComparison.Ordinal));
        Bloque(previa, "agro_taras").Nuevos.Should().ContainSingle();
        (await api.GetFromJsonAsync<List<NombreResp>>("/formas-pago"))!.Should().NotContain(f => f.Nombre == "Giro a 90 días");

        // La copia crea lo que falta.
        var copia = await CopiarAsync(api, a, true);
        copia.Ejecutada.Should().BeTrue();
        copia.Elementos.SelectMany(e => e.Errores).Should().BeEmpty();
        copia.Creados.Should().Be(previa.Elementos.Sum(e => e.Nuevos.Count));
        (await api.GetFromJsonAsync<List<CodigoResp>>("/contabilidad/cuentas"))!.Should().Contain(x => x.Codigo == "43000099");
        (await api.GetFromJsonAsync<List<CodigoResp>>("/contabilidad/diarios"))!.Should().Contain(x => x.Codigo == "REG");
        (await api.GetFromJsonAsync<List<NombreResp>>("/formas-pago"))!.Should().Contain(f => f.Nombre == "Giro a 90 días");
        (await api.GetFromJsonAsync<List<SerieResp>>("/series"))!.Should().Contain(s => s.Prefijo == "EXP" && s.Ejercicio == Anio && s.SiguienteNumero == 1);
        var almacenB = (await api.GetFromJsonAsync<List<CodigoResp>>("/inventario/almacenes"))!.Single(x => x.Codigo == "CAM");
        (await api.GetFromJsonAsync<List<UbicacionResp>>($"/inventario/ubicaciones?almacenId={almacenB.Id}"))!.Should().ContainSingle(u => u.Codigo == "C1");
        var transB = (await api.GetFromJsonAsync<List<NombreResp>>("/transporte/transportistas"))!.Single(t => t.Nombre == "Transportes Levante SL");
        (await api.GetFromJsonAsync<List<VehiculoResp>>("/transporte/vehiculos"))!.Should().ContainSingle(v => v.Matricula == "1234BCD")
            .Which.TransportistaId.Should().Be(transB.Id, "el vehículo va con el transportista copiado, no con el de la otra empresa");
        (await api.GetFromJsonAsync<List<CodigoResp>>("/logistica/soportes"))!.Should().Contain(x => x.Codigo == "EUR");
        (await api.GetFromJsonAsync<List<CodigoResp>>("/agro/campanas"))!.Should().Contain(x => x.Codigo == $"C{Anio}");
        (await api.GetFromJsonAsync<List<CodigoResp>>("/agro/categorias"))!.Should().Contain(x => x.Codigo == "EXTRA");
        (await api.GetFromJsonAsync<List<CodigoResp>>("/agro/conceptos"))!.Should().Contain(x => x.Codigo == "TRANS");

        // Repetir no duplica nada.
        var otra = await CopiarAsync(api, a, true);
        otra.Creados.Should().Be(0);
        Bloque(otra, "formas_pago").YaExistian.Should().BePositive();
        (await api.GetFromJsonAsync<List<NombreResp>>("/formas-pago"))!.Count(f => f.Nombre == "Giro a 90 días").Should().Be(1);

        // En una empresa de otro grupo, las taras (van con artículos del grupo) no se copian.
        await SeleccionarAsync(api, c);
        await ConAgroAsync(api, c);
        var fuera = await CopiarAsync(api, a, false, "agro_taras", "formas_pago");
        Bloque(fuera, "agro_taras").Nuevos.Should().BeEmpty();
        Bloque(fuera, "agro_taras").Omitidos.Should().ContainSingle();
        Bloque(fuera, "formas_pago").Nuevos.Should().Equal("Giro a 90 días");
    }

    [Fact]
    public async Task Solo_se_copia_de_empresas_a_las_que_se_tiene_acceso()
    {
        var (ajeno, empresaAjena) = await Ayudas.ConEmpresaAsync(_fabrica);
        ajeno.Dispose();
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);

        var r = await api.PostAsJsonAsync("/empresas/actual/copia/vista-previa", new { OrigenEmpresaId = empresaAjena, Elementos = new[] { "formas_pago" } });
        r.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var vacia = await api.PostAsJsonAsync("/empresas/actual/copia", new { OrigenEmpresaId = empresaAjena, Elementos = Array.Empty<string>() });
        vacia.IsSuccessStatusCode.Should().BeFalse();
    }
}
