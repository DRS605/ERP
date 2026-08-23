using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas del flujo de aprobaciones y de la segregación de funciones (SoD).</summary>
public sealed class AprobacionesEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public AprobacionesEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record ReglaResp(Guid Id, string TipoDocumento, decimal UmbralImporte, bool Activa);
    private sealed record RequiereResp(bool Requiere);
    private sealed record SolicitudResp(Guid Id, string TipoDocumento, string Referencia, decimal Importe, string Estado, Guid SolicitanteUsuarioId);

    [Fact]
    public async Task Configurar_regla_y_consultar_si_requiere()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);

        var put = await cli.PutAsJsonAsync("/aprobaciones/reglas", new { TipoDocumento = "Factura", UmbralImporte = 1000m, Activa = true });
        put.StatusCode.Should().Be(HttpStatusCode.OK);

        var reglas = await cli.GetFromJsonAsync<List<ReglaResp>>("/aprobaciones/reglas");
        reglas!.Should().ContainSingle(r => r.TipoDocumento == "Factura" && r.UmbralImporte == 1000m);

        (await cli.GetFromJsonAsync<RequiereResp>("/aprobaciones/requiere?tipoDocumento=Factura&importe=999"))!.Requiere.Should().BeFalse();
        (await cli.GetFromJsonAsync<RequiereResp>("/aprobaciones/requiere?tipoDocumento=Factura&importe=1500"))!.Requiere.Should().BeTrue();

        // Reconfigurar el mismo tipo actualiza (no duplica).
        await cli.PutAsJsonAsync("/aprobaciones/reglas", new { TipoDocumento = "Factura", UmbralImporte = 2000m, Activa = true });
        (await cli.GetFromJsonAsync<List<ReglaResp>>("/aprobaciones/reglas"))!.Should().ContainSingle();
    }

    [Fact]
    public async Task Crear_solicitud_y_listar_pendientes()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);

        var crear = await cli.PostAsJsonAsync("/aprobaciones/solicitudes", new { TipoDocumento = "Gasto", DocumentoId = Guid.NewGuid(), Referencia = "Gasto grande", Importe = 5000m });
        crear.StatusCode.Should().Be(HttpStatusCode.Created);
        var s = (await crear.Content.ReadFromJsonAsync<SolicitudResp>())!;
        s.Estado.Should().Be("Pendiente");

        var pendientes = await cli.GetFromJsonAsync<List<SolicitudResp>>("/aprobaciones/solicitudes?estado=Pendiente");
        pendientes!.Should().ContainSingle(x => x.Id == s.Id);
    }

    [Fact]
    public async Task Segregacion_de_funciones_el_solicitante_no_puede_aprobar_ni_rechazar()
    {
        // El propietario (único usuario) crea la solicitud y luego intenta resolverla → SoD lo impide.
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);

        var crear = await cli.PostAsJsonAsync("/aprobaciones/solicitudes", new { TipoDocumento = "Pago", DocumentoId = Guid.NewGuid(), Referencia = "Pago proveedor", Importe = 8000m });
        var s = (await crear.Content.ReadFromJsonAsync<SolicitudResp>())!;

        var aprobar = await cli.PostAsync(new Uri($"/aprobaciones/solicitudes/{s.Id}/aprobar", UriKind.Relative), content: null);
        aprobar.StatusCode.Should().Be(HttpStatusCode.Conflict); // Error.Conflicto → 409

        var rechazar = await cli.PostAsJsonAsync($"/aprobaciones/solicitudes/{s.Id}/rechazar", new { Motivo = "no" });
        rechazar.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // Sigue pendiente.
        var pendientes = await cli.GetFromJsonAsync<List<SolicitudResp>>("/aprobaciones/solicitudes?estado=Pendiente");
        pendientes!.Should().ContainSingle(x => x.Id == s.Id);
    }

    [Fact]
    public async Task Referencia_vacia_da_error()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var r = await cli.PostAsJsonAsync("/aprobaciones/solicitudes", new { TipoDocumento = "Factura", DocumentoId = Guid.NewGuid(), Referencia = "", Importe = 100m });
        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
