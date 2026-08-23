using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Al emitir una factura o registrar un gasto se puede <b>elegir</b> la actividad de negocio
/// (además de heredarla del tercero), pero solo si el usuario tiene acceso a ella según su
/// visibilidad en el área correspondiente (Ventas/Compras).
/// </summary>
public sealed class ActividadEnDocumentosTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public ActividadEnDocumentosTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record ActividadResp(Guid Id, string Nombre, bool Activa);
    private sealed record IdResp(Guid Id);
    private sealed record PerfilResp(Guid Id);
    private sealed record ActividadResultadoDto(Guid? ActividadNegocioId, string Actividad, decimal Ventas, decimal Compras, decimal Resultado);
    private sealed record InformePorActividadDto(DateOnly Desde, DateOnly Hasta, decimal Ventas, decimal Compras, decimal Resultado, List<ActividadResultadoDto> Actividades);

    [Fact]
    public async Task Sin_reglas_se_puede_elegir_cualquier_actividad_al_emitir()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var actividad = (await (await cli.PostAsJsonAsync("/actividades", new { Nombre = "Libre" })).Content.ReadFromJsonAsync<ActividadResp>())!.Id;
        var clienteId = (await (await cli.PostAsJsonAsync("/clientes", new { Nombre = "Cli", NifFiscal = "B12345674" })).Content.ReadFromJsonAsync<IdResp>())!.Id;

        var emitir = await cli.PostAsJsonAsync("/facturas", new
        {
            ClienteId = clienteId,
            ActividadNegocioId = actividad,
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = 100m, CodigoIva = "IVA21" } },
        });
        emitir.StatusCode.Should().Be(HttpStatusCode.Created);

        var informe = await cli.GetFromJsonAsync<InformePorActividadDto>("/informes/por-actividad");
        informe!.Actividades.Single(a => a.ActividadNegocioId == actividad).Ventas.Should().Be(100m);
    }

    [Fact]
    public async Task Con_reglas_solo_se_puede_elegir_una_actividad_concedida()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var usuario = (await cli.GetFromJsonAsync<PerfilResp>("/auth/perfil"))!.Id;

        var permitida = (await (await cli.PostAsJsonAsync("/actividades", new { Nombre = "Permitida" })).Content.ReadFromJsonAsync<ActividadResp>())!.Id;
        var vetada = (await (await cli.PostAsJsonAsync("/actividades", new { Nombre = "Vetada" })).Content.ReadFromJsonAsync<ActividadResp>())!.Id;

        // El usuario solo tiene acceso a «Permitida» en Ventas.
        await cli.PutAsJsonAsync($"/actividades/visibilidad/{usuario}", new { Area = "Ventas", Actividades = new[] { permitida } });

        var clienteId = (await (await cli.PostAsJsonAsync("/clientes", new { Nombre = "Cli", NifFiscal = "B12345683" })).Content.ReadFromJsonAsync<IdResp>())!.Id;

        object Comando(Guid actividad) => new
        {
            ClienteId = clienteId,
            ActividadNegocioId = actividad,
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = 100m, CodigoIva = "IVA21" } },
        };

        // Elegir la vetada -> 403; elegir la permitida -> 201.
        (await cli.PostAsJsonAsync("/facturas", Comando(vetada))).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await cli.PostAsJsonAsync("/facturas", Comando(permitida))).StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task El_control_de_acceso_aplica_tambien_a_los_gastos()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var usuario = (await cli.GetFromJsonAsync<PerfilResp>("/auth/perfil"))!.Id;

        var permitida = (await (await cli.PostAsJsonAsync("/actividades", new { Nombre = "Compras OK" })).Content.ReadFromJsonAsync<ActividadResp>())!.Id;
        var vetada = (await (await cli.PostAsJsonAsync("/actividades", new { Nombre = "Compras NO" })).Content.ReadFromJsonAsync<ActividadResp>())!.Id;

        await cli.PutAsJsonAsync($"/actividades/visibilidad/{usuario}", new { Area = "Compras", Actividades = new[] { permitida } });

        (await cli.PostAsJsonAsync("/gastos", new { Concepto = "X", BaseImponible = 50m, CodigoIva = "IVA21", ActividadNegocioId = vetada }))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await cli.PostAsJsonAsync("/gastos", new { Concepto = "X", BaseImponible = 50m, CodigoIva = "IVA21", ActividadNegocioId = permitida }))
            .StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
