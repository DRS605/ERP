using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Empresas y grupos: crear una empresa dentro del grupo (solo quien gestiona una de sus empresas), unir al grupo una
/// empresa que ya existe con sus maestros (los repetidos quedan de baja y sus documentos intactos) y dar de baja una
/// empresa sin llevarse los maestros que comparte con las demás.
/// </summary>
public sealed class UnionGrupoTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public UnionGrupoTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record GrupoResp(Guid Id);
    private sealed record SeleccionResp(string Token);
    private sealed record ClienteResp(Guid Id, string Nombre, string? NifFiscal, bool Activo);
    private sealed record TarifaResp(Guid Id, string Codigo, bool Activa);
    private sealed record CandidataResp(Guid Id, string RazonSocial, bool Unible, string? Motivo);
    private sealed record MaestroResp(string Clave, int Incorporados, List<string> Duplicados);
    private sealed record UnionResp(bool Ejecutada, List<MaestroResp> Maestros);
    private sealed record FacturaResp(Guid Id, Guid? ClienteId);

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

    private static async Task<List<ClienteResp>> ClientesAsync(HttpClient api, bool bajas = false) =>
        (await api.GetFromJsonAsync<List<ClienteResp>>(bajas ? "/clientes?bajas=true" : "/clientes"))!;

    [Fact]
    public async Task Solo_se_crea_una_empresa_en_un_grupo_propio_y_la_baja_no_se_lleva_los_maestros_compartidos()
    {
        var (api, a) = await Ayudas.ConEmpresaAsync(_fabrica);
        var grupo = (await api.GetFromJsonAsync<GrupoResp>("/grupos/actual"))!.Id;
        await IdAsync(api, "/clientes", new { Nombre = "Cliente del grupo SL", NifFiscal = Ayudas.GenerarNif() });

        // Otro usuario no puede meterse en el grupo, aunque conozca su identificador.
        var (ajeno, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var intruso = await ajeno.PostAsJsonAsync("/empresas", new { Nif = Ayudas.GenerarNif(), RazonSocial = "Intrusa SL", GrupoId = grupo });
        intruso.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Quien gestiona una empresa del grupo, sí; y la nueva ve sus clientes.
        var b = await IdAsync(api, "/empresas", new { Nif = Ayudas.GenerarNif(), RazonSocial = "Hermana SL", GrupoId = grupo });
        await SeleccionarAsync(api, b);
        (await api.GetFromJsonAsync<GrupoResp>("/grupos/actual"))!.Id.Should().Be(grupo);
        (await ClientesAsync(api)).Should().Contain(c => c.Nombre == "Cliente del grupo SL");

        // Dar de baja la hermana no borra los clientes del grupo, que siguen siendo de A.
        (await api.DeleteAsync(new Uri("/cuenta", UriKind.Relative))).IsSuccessStatusCode.Should().BeTrue();
        await SeleccionarAsync(api, a);
        (await ClientesAsync(api)).Should().Contain(c => c.Nombre == "Cliente del grupo SL");
    }

    [Fact]
    public async Task Une_una_empresa_existente_al_grupo_y_da_de_baja_los_repetidos()
    {
        var (api, a) = await Ayudas.ConEmpresaAsync(_fabrica);
        var grupo = (await api.GetFromJsonAsync<GrupoResp>("/grupos/actual"))!.Id;
        var nifComun = Ayudas.GenerarNif();
        var delGrupo = await IdAsync(api, "/clientes", new { Nombre = "Mercado Central SA", NifFiscal = nifComun });
        await IdAsync(api, "/tarifas", new { Codigo = "MAYOR", Nombre = "Mayoristas", Lineas = new[] { new { PorcentajeDescuento = 5m } } });

        // E: otra empresa del usuario, sola en su grupo, con un cliente repetido (mismo NIF), uno propio, una tarifa con el
        // mismo código y una factura al repetido.
        var e = await IdAsync(api, "/empresas", new { Nif = Ayudas.GenerarNif(), RazonSocial = "Comprada SL" });
        await SeleccionarAsync(api, e);
        var repetido = await IdAsync(api, "/clientes", new { Nombre = "Mercado Central (antiguo)", NifFiscal = nifComun });
        await IdAsync(api, "/clientes", new { Nombre = "Cliente propio de E SL", NifFiscal = Ayudas.GenerarNif() });
        await IdAsync(api, "/tarifas", new { Codigo = "MAYOR", Nombre = "Mayoristas de E", Lineas = new[] { new { PorcentajeDescuento = 7m } } });
        var factura = await api.PostAsJsonAsync("/facturas", new { ClienteId = repetido, Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = 100m, CodigoIva = "IVA21" } } });
        factura.IsSuccessStatusCode.Should().BeTrue(await factura.Content.ReadAsStringAsync());

        // F: comparte grupo con otra, así que no se puede unir sola.
        var f = await IdAsync(api, "/empresas", new { Nif = Ayudas.GenerarNif(), RazonSocial = "Con pareja SL" });
        await SeleccionarAsync(api, f);
        var grupoF = (await api.GetFromJsonAsync<GrupoResp>("/grupos/actual"))!.Id;
        await IdAsync(api, "/empresas", new { Nif = Ayudas.GenerarNif(), RazonSocial = "Pareja SL", GrupoId = grupoF });

        await SeleccionarAsync(api, a);
        var candidatas = (await api.GetFromJsonAsync<List<CandidataResp>>("/grupos/actual/union"))!;
        candidatas.Should().ContainSingle(c => c.Id == e).Which.Unible.Should().BeTrue();
        candidatas.Should().ContainSingle(c => c.Id == f).Which.Unible.Should().BeFalse();
        (await api.PostAsJsonAsync("/grupos/actual/union", new { EmpresaId = f })).StatusCode.Should().Be(HttpStatusCode.Conflict);

        // La vista previa no cambia nada.
        var previa = (await (await api.PostAsJsonAsync("/grupos/actual/union/vista-previa", new { EmpresaId = e })).Content.ReadFromJsonAsync<UnionResp>())!;
        previa.Ejecutada.Should().BeFalse();
        var cli = previa.Maestros.Single(m => m.Clave == "clientes");
        cli.Incorporados.Should().Be(2);
        cli.Duplicados.Should().Equal("Mercado Central (antiguo)");
        previa.Maestros.Single(m => m.Clave == "tarifas").Duplicados.Should().ContainSingle();
        (await ClientesAsync(api)).Should().NotContain(c => c.Nombre == "Cliente propio de E SL");

        var union = await api.PostAsJsonAsync("/grupos/actual/union", new { EmpresaId = e });
        union.IsSuccessStatusCode.Should().BeTrue(await union.Content.ReadAsStringAsync());

        // A ve los clientes de E; el repetido, de baja.
        var todos = await ClientesAsync(api, bajas: true);
        todos.Should().Contain(c => c.Nombre == "Cliente propio de E SL" && c.Activo);
        todos.Should().ContainSingle(c => c.Id == repetido).Which.Activo.Should().BeFalse();
        todos.Should().ContainSingle(c => c.Id == delGrupo).Which.Activo.Should().BeTrue();
        (await ClientesAsync(api)).Should().NotContain(c => c.Id == repetido);

        // La tarifa de E cambia de código para no chocar.
        var tarifas = (await api.GetFromJsonAsync<List<TarifaResp>>("/tarifas"))!;
        tarifas.Select(t => t.Codigo.ToUpperInvariant()).Should().Contain(["MAYOR", "MAYOR-2"]);

        // E es ahora del grupo, y su factura sigue con su cliente.
        await SeleccionarAsync(api, e);
        (await api.GetFromJsonAsync<GrupoResp>("/grupos/actual"))!.Id.Should().Be(grupo);
        (await api.GetFromJsonAsync<List<FacturaResp>>("/facturas"))!.Should().ContainSingle().Which.ClienteId.Should().Be(repetido);
        (await ClientesAsync(api)).Should().Contain(c => c.Id == delGrupo);

        // Ya no es candidata: es del grupo.
        await SeleccionarAsync(api, a);
        (await api.GetFromJsonAsync<List<CandidataResp>>("/grupos/actual/union"))!.Should().NotContain(c => c.Id == e);
    }
}
