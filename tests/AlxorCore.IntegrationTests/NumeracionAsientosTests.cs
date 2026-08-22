using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Verifica que la numeración de asientos no colisiona al crear varios en un mismo lote (regresión del
/// bug en el que contabilizar dos pendientes del mismo ejercicio de una vez asignaba el mismo número).
/// </summary>
public sealed class NumeracionAsientosTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public NumeracionAsientosTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record PendienteResp(Guid Id, string Sentido, decimal Total);
    private sealed record AsientoResp(int Numero, string Origen);

    [Fact]
    public async Task Contabilizar_varios_pendientes_del_mismo_ejercicio_asigna_numeros_distintos()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await cliente.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).StatusCode.Should().Be(HttpStatusCode.OK);

        // Dos ventas del mismo ejercicio → dos documentos pendientes.
        var clienteId = (await (await cliente.PostAsJsonAsync("/clientes", new { Nombre = "Cliente A", NifFiscal = "B12345674" })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        foreach (var precio in new[] { 100m, 200m })
        {
            await cliente.PostAsJsonAsync("/facturas", new
            {
                ClienteId = clienteId,
                FechaEmision = "2026-05-10",
                Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = precio, CodigoIva = "IVA21" } },
            });
        }

        var pendientes = await cliente.GetFromJsonAsync<List<PendienteResp>>("/contabilidad/pendientes");
        var ids = pendientes!.Where(p => p.Sentido == "Venta").Select(p => p.Id).ToArray();
        ids.Should().HaveCount(2);

        // Contabilizar AMBOS de una vez (antes fallaba por número de asiento duplicado).
        var r = await cliente.PostAsJsonAsync("/contabilidad/pendientes/contabilizar", new { Ids = ids });
        r.StatusCode.Should().Be(HttpStatusCode.OK);

        var diario = await cliente.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2026");
        var ventas = diario!.Where(a => a.Origen == "Venta").ToList();
        ventas.Should().HaveCount(2);
        ventas.Select(a => a.Numero).Distinct().Should().HaveCount(2); // números distintos, sin colisión
    }
}
