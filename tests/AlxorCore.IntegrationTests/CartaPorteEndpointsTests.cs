using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de la carta de porte (documento de control del transporte): alta, detalle y PDF.</summary>
public sealed class CartaPorteEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public CartaPorteEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record LineaResp(string Descripcion, int Bultos, decimal PesoKg);
    private sealed record CartaResp(
        Guid Id, string NumeroCompleto, DateOnly FechaExpedicion, string RemitenteNombre, string DestinatarioNombre,
        string? Matricula, string LugarOrigen, string LugarDestino, int TotalBultos, decimal TotalPesoKg, List<LineaResp> Lineas);
    private sealed record ResumenResp(Guid Id, string NumeroCompleto, string DestinatarioNombre, int TotalBultos, decimal TotalPesoKg);

    [Fact]
    public async Task Crear_carta_de_porte_para_un_cliente_calcula_totales_y_numera()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var clienteId = (await (await cli.PostAsJsonAsync("/clientes",
            new { Nombre = "Distribuciones Norte SL", NifFiscal = "B12345674", Calle = "Pol. Ind. 4", CodigoPostal = "33211", Poblacion = "Gijón", Provincia = "Asturias" }))
            .Content.ReadFromJsonAsync<IdResp>())!.Id;

        var comando = new
        {
            DestinatarioClienteId = clienteId,
            TransportistaNombre = "Transportes Rápidos SA",
            TransportistaNif = "A11111111",
            Matricula = "1234 ABC",
            LugarOrigen = "Almacén central, Oviedo",
            Lineas = new[]
            {
                new { Descripcion = "Palés de bebida", Bultos = 10, PesoKg = 250.5m },
                new { Descripcion = "Cajas de vaso", Bultos = 5, PesoKg = 30m },
            },
        };

        var crear = await cli.PostAsJsonAsync("/cartas-porte", comando);
        crear.StatusCode.Should().Be(HttpStatusCode.Created);
        var carta = await crear.Content.ReadFromJsonAsync<CartaResp>();

        carta!.DestinatarioNombre.Should().Be("Distribuciones Norte SL");
        carta.RemitenteNombre.Should().Be("Empresa de Pruebas SL");
        carta.TotalBultos.Should().Be(15);
        carta.TotalPesoKg.Should().Be(280.5m);
        carta.LugarDestino.Should().Contain("Gijón");           // heredado de la dirección del cliente
        carta.NumeroCompleto.Should().EndWith("/00001");
        carta.Lineas.Should().HaveCount(2);

        // Aparece en el listado.
        var lista = await cli.GetFromJsonAsync<List<ResumenResp>>("/cartas-porte");
        lista!.Should().ContainSingle(c => c.Id == carta.Id);

        // El PDF se genera.
        var pdf = await cli.GetAsync(new Uri($"/cartas-porte/{carta.Id}/pdf", UriKind.Relative));
        pdf.StatusCode.Should().Be(HttpStatusCode.OK);
        pdf.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        (await pdf.Content.ReadAsByteArrayAsync()).Length.Should().BeGreaterThan(500);
    }

    [Fact]
    public async Task La_numeracion_es_correlativa_por_empresa()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        object Comando() => new
        {
            DestinatarioNombre = "Cliente libre",
            LugarOrigen = "A",
            LugarDestino = "B",
            Lineas = new[] { new { Descripcion = "Mercancía", Bultos = 1, PesoKg = 1m } },
        };

        var a = await (await cli.PostAsJsonAsync("/cartas-porte", Comando())).Content.ReadFromJsonAsync<CartaResp>();
        var b = await (await cli.PostAsJsonAsync("/cartas-porte", Comando())).Content.ReadFromJsonAsync<CartaResp>();

        a!.NumeroCompleto.Should().EndWith("/00001");
        b!.NumeroCompleto.Should().EndWith("/00002");
    }

    [Fact]
    public async Task Sin_mercancias_no_se_admite()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var r = await cli.PostAsJsonAsync("/cartas-porte", new { DestinatarioNombre = "X", Lineas = Array.Empty<object>() });
        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
