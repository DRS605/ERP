using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas del catálogo de tipos de IVA configurable por empresa (siembra, alta y edición).</summary>
public sealed class TiposIvaEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public TiposIvaEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record TipoIvaResp(Guid Id, string Codigo, string Nombre, decimal Porcentaje, string Clase, string? MencionFactura, bool Activo, bool Repercute);

    [Fact]
    public async Task La_primera_consulta_siembra_el_estandar()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);

        var tipos = await cli.GetFromJsonAsync<List<TipoIvaResp>>("/tipos-iva");
        tipos!.Select(t => t.Codigo).Should().Contain(new[] { "IVA21", "IVA10", "IVA4", "IVA0", "NOSUJETO", "ISP", "INTRA", "IMPORT21" });

        // La segunda consulta no duplica.
        var otra = await cli.GetFromJsonAsync<List<TipoIvaResp>>("/tipos-iva");
        otra!.Count.Should().Be(tipos.Count);

        // El ISP no repercute; el IVA21 sí.
        tipos.Single(t => t.Codigo == "ISP").Repercute.Should().BeFalse();
        tipos.Single(t => t.Codigo == "IVA21").Repercute.Should().BeTrue();
    }

    [Fact]
    public async Task Crear_y_editar_un_tipo_de_iva()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await cli.GetAsync(new Uri("/tipos-iva", UriKind.Relative)); // siembra

        var crear = await cli.PostAsJsonAsync("/tipos-iva", new
        {
            Codigo = "REDES",
            Nombre = "Régimen especial",
            Porcentaje = 0m,
            RecargoEquivalencia = 0m,
            Clase = "Exento",
            MencionFactura = "Operación exenta (art. 20).",
        });
        crear.StatusCode.Should().Be(HttpStatusCode.Created);
        var creado = await crear.Content.ReadFromJsonAsync<TipoIvaResp>();
        creado!.Repercute.Should().BeFalse();

        // Código duplicado -> 409.
        var dup = await cli.PostAsJsonAsync("/tipos-iva", new { Codigo = "REDES", Nombre = "x", Porcentaje = 0m, RecargoEquivalencia = 0m, Clase = "Exento", MencionFactura = (string?)null });
        dup.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // Editar nombre.
        var editar = await cli.PutAsJsonAsync($"/tipos-iva/{creado.Id}", new { Codigo = "REDES", Nombre = "Exención cambiada", Porcentaje = 0m, RecargoEquivalencia = 0m, Clase = "Exento", MencionFactura = "otra", Activo = true });
        editar.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Una_clase_sin_repercusion_con_porcentaje_se_rechaza()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var r = await cli.PostAsJsonAsync("/tipos-iva", new { Codigo = "MALO", Nombre = "malo", Porcentaje = 21m, RecargoEquivalencia = 0m, Clase = "NoSujeto", MencionFactura = (string?)null });
        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
