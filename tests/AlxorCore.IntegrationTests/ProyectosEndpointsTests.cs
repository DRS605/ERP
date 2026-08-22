using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de integración del módulo Proyectos (imputación de costes y presupuesto vs. real).</summary>
public sealed class ProyectosEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;
    public ProyectosEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record PersonaResp(Guid Id, string Nombre, string? Puesto, decimal TarifaHora, bool Activo);
    private sealed record ProyectoResp(Guid Id, int Ejercicio, int Numero, string Nombre, string Estado, decimal Presupuesto, decimal CosteReal, decimal Desviacion);
    private sealed record ImputacionResp(Guid Id, string Tipo, decimal Cantidad, decimal CosteUnitario, decimal Importe);
    private sealed record DetalleResp(Guid Id, string Estado, decimal Presupuesto, decimal CosteManoObra, decimal CosteMateriales,
        decimal CosteGastos, decimal CosteReal, decimal Desviacion, List<ImputacionResp> Imputaciones);

    [Fact]
    public async Task Imputa_mano_de_obra_material_y_gasto_y_calcula_desviacion()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);

        // Persona con tarifa 30 €/h.
        var ana = (await (await cliente.PostAsJsonAsync("/personal", new { Nombre = "Ana Pérez", TarifaHora = 30m })).Content.ReadFromJsonAsync<PersonaResp>())!;
        // Artículo con precio de compra 10 (método por defecto de la empresa: estándar → precio de compra).
        var cemento = (await (await cliente.PostAsJsonAsync("/productos", new { Nombre = "Cemento", PrecioUnitario = 20m, PrecioCompra = 10m, CodigoIva = "IVA21" })).Content.ReadFromJsonAsync<IdResp>())!;

        // Proyecto con presupuesto 1000.
        var crear = await cliente.PostAsJsonAsync("/proyectos", new { Nombre = "Reforma local", Presupuesto = 1000m });
        crear.StatusCode.Should().Be(HttpStatusCode.Created);
        var proyecto = (await crear.Content.ReadFromJsonAsync<ProyectoResp>())!;
        proyecto.Numero.Should().Be(1);
        proyecto.Estado.Should().Be("Abierto");

        // Mano de obra: 4 h × 30 €/h = 120.
        (await cliente.PostAsJsonAsync($"/proyectos/{proyecto.Id}/mano-obra", new { PersonaId = ana.Id, Horas = 4m })).StatusCode.Should().Be(HttpStatusCode.OK);
        // Material: 3 × 10 = 30 (valorado según el método de la empresa).
        (await cliente.PostAsJsonAsync($"/proyectos/{proyecto.Id}/material", new { ProductoId = cemento.Id, Cantidad = 3m })).StatusCode.Should().Be(HttpStatusCode.OK);
        // Gasto directo: 50.
        (await cliente.PostAsJsonAsync($"/proyectos/{proyecto.Id}/gasto", new { Concepto = "Dietas", Importe = 50m })).StatusCode.Should().Be(HttpStatusCode.OK);

        var det = await cliente.GetFromJsonAsync<DetalleResp>($"/proyectos/{proyecto.Id}");
        det!.CosteManoObra.Should().Be(120m);
        det.CosteMateriales.Should().Be(30m);
        det.CosteGastos.Should().Be(50m);
        det.CosteReal.Should().Be(200m);
        det.Desviacion.Should().Be(800m);
        det.Imputaciones.Should().HaveCount(3);

        // Eliminar el gasto recalcula el coste real.
        var gasto = det.Imputaciones.Single(i => i.Tipo == "Gasto");
        (await cliente.DeleteAsync($"/proyectos/{proyecto.Id}/imputaciones/{gasto.Id}")).StatusCode.Should().Be(HttpStatusCode.OK);
        var det2 = await cliente.GetFromJsonAsync<DetalleResp>($"/proyectos/{proyecto.Id}");
        det2!.CosteReal.Should().Be(150m);

        // El listado refleja el coste y la desviación.
        var lista = await cliente.GetFromJsonAsync<List<ProyectoResp>>("/proyectos");
        lista!.Single().CosteReal.Should().Be(150m);
    }

    [Fact]
    public async Task No_se_imputa_a_una_persona_inexistente_ni_a_un_proyecto_cerrado()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var proyecto = (await (await cliente.PostAsJsonAsync("/proyectos", new { Nombre = "P2", Presupuesto = 100m })).Content.ReadFromJsonAsync<ProyectoResp>())!;

        (await cliente.PostAsJsonAsync($"/proyectos/{proyecto.Id}/mano-obra", new { PersonaId = Guid.NewGuid(), Horas = 1m })).StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await cliente.PostAsync($"/proyectos/{proyecto.Id}/cerrar", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await cliente.PostAsJsonAsync($"/proyectos/{proyecto.Id}/gasto", new { Concepto = "Tarde", Importe = 10m })).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
