using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Valoración de las compras al agricultor como en Hispatec: el precio del día gana al del periodo y este al general, y
/// el del envase de la entrega al que no lo tiene; se fijan en masa y se pueden proponer desde lo vendido (a resultas).
/// </summary>
public sealed class PreciosValoracionAgroTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public PreciosValoracionAgroTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record SeleccionResp(string Token);
    private sealed record LineaResp(Guid Id, Guid? PartidaId);
    private sealed record RecepcionResp(Guid Id, List<LineaResp> Lineas);
    private sealed record ErrorResp(string Codigo, string Mensaje);
    private sealed record MasivoResp(int Creados, int Actualizados, List<ErrorResp> Errores);
    private sealed record LineaLiqResp(Guid LineaRecepcionId, decimal Kilos, decimal PrecioKg);
    private sealed record LiqResp(List<LineaLiqResp> Lineas);
    private sealed record PrevResp(bool Valida, List<ErrorResp> Errores, LiqResp? Liquidacion);
    private sealed record PropuestaResp(Guid ProductoId, decimal KilosVendidos, decimal? PrecioMedioVenta, decimal? PrecioPropuesto, string? Aviso);

    private static async Task<Guid> IdAsync(HttpClient api, string ruta, object cuerpo)
    {
        var r = await api.PostAsJsonAsync(ruta, cuerpo);
        r.StatusCode.Should().Be(HttpStatusCode.Created, $"{ruta}: {await r.Content.ReadAsStringAsync()}");
        return (await r.Content.ReadFromJsonAsync<IdResp>())!.Id;
    }

    private static async Task<T> OkAsync<T>(HttpResponseMessage r)
    {
        r.StatusCode.Should().BeOneOf([HttpStatusCode.OK, HttpStatusCode.Created], await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<T>())!;
    }

    [Fact]
    public async Task Gana_el_precio_del_dia_con_envase_se_fijan_en_masa_y_se_proponen_desde_las_ventas()
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "completa", ModulosAdicionales = new[] { "agro" } })).EnsureSuccessStatusCode();
        var token = (await (await api.PostAsync(new Uri($"/empresas/{empresa}/seleccionar", UriKind.Relative), null)).Content.ReadFromJsonAsync<SeleccionResp>())!.Token;
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        (await api.GetAsync(new Uri("/tipos-iva", UriKind.Relative))).EnsureSuccessStatusCode();
        var anio = DateTime.UtcNow.Year;
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var mes = new DateOnly(hoy.Year, hoy.Month, 1);

        var campana = await IdAsync(api, "/agro/campanas", new { Codigo = $"{anio}", Nombre = $"Campaña {anio}", Desde = new DateOnly(anio, 1, 1), Hasta = new DateOnly(anio, 12, 31) });
        var proveedor = await IdAsync(api, "/proveedores", new { Nombre = "Juan Labrador", NifFiscal = Ayudas.GenerarNif() });
        var agricultor = await IdAsync(api, "/agro/agricultores", new { ProveedorId = proveedor, Regimen = "Reagp" });
        var tomate = await IdAsync(api, "/productos", new { Nombre = "Tomate", PrecioUnitario = 1m, Tipo = "Bien", Unidad = "kg" });
        var caja = await IdAsync(api, "/productos", new { Nombre = "Caja IFCO", PrecioUnitario = 1m, Tipo = "Bien", Unidad = "ud" });
        (await api.PutAsJsonAsync($"/agro/campanas/{campana}/articulos", new { ProductoId = tomate, Metodo = "PorPeriodo" })).EnsureSuccessStatusCode();

        var rec = await IdAsync(api, "/agro/recepciones", new { AgricultorId = agricultor, Fecha = hoy });
        var conCaja = (await OkAsync<RecepcionResp>(await api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas", new { ProductoId = tomate, FechaRecoleccion = hoy, EnvaseProductoId = caja }))).Lineas[0].Id;
        var r2 = await OkAsync<RecepcionResp>(await api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas", new { ProductoId = tomate, FechaRecoleccion = hoy }));
        var sinCaja = r2.Lineas.Single(l => l.Id != conCaja).Id;
        (await api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{conCaja}/pesadas", new { BrutoKg = 1_100m, TaraKg = 100m })).EnsureSuccessStatusCode();
        (await api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{sinCaja}/pesadas", new { BrutoKg = 600m, TaraKg = 100m })).EnsureSuccessStatusCode();
        (await api.PostAsync(new Uri($"/agro/recepciones/{rec}/confirmar", UriKind.Relative), null)).EnsureSuccessStatusCode();

        // Fijación masiva: general, del mes, del día y del día con caja; uno repetido sale con su motivo.
        var masivo = await OkAsync<MasivoResp>(await api.PostAsJsonAsync($"/agro/campanas/{campana}/precios/masivo", new
        {
            Precios = new object[]
            {
                new { ProductoId = tomate, Desde = new DateOnly(anio, 1, 1), Hasta = new DateOnly(anio, 12, 31), PrecioKg = 0.30m, Tipo = "General" },
                new { ProductoId = tomate, Desde = mes, Hasta = mes.AddMonths(1).AddDays(-1), PrecioKg = 0.40m, Tipo = "Periodo" },
                new { ProductoId = tomate, Desde = hoy, Hasta = hoy, PrecioKg = 0.50m, Tipo = "Dia" },
                new { ProductoId = tomate, Desde = hoy, Hasta = hoy, PrecioKg = 0.55m, Tipo = "Dia", EnvaseProductoId = caja },
                new { ProductoId = tomate, Desde = mes, Hasta = mes.AddDays(1), PrecioKg = 0.45m, Tipo = "Periodo" },
            },
        }));
        masivo.Should().Match<MasivoResp>(m => m.Creados == 4 && m.Actualizados == 0);
        masivo.Errores.Should().ContainSingle(e => e.Codigo == "precio.solapado");

        var datos = new { AgricultorId = agricultor, CampanaId = campana, Desde = new DateOnly(anio, 1, 1), Hasta = hoy };
        var prev = await OkAsync<PrevResp>(await api.PostAsJsonAsync("/agro/liquidaciones/previsualizar", datos));
        prev.Valida.Should().BeTrue(string.Join(" ", prev.Errores.Select(e => e.Mensaje)));
        prev.Liquidacion!.Lineas.Single(l => l.LineaRecepcionId == conCaja).PrecioKg.Should().Be(0.55m, "el del día con su envase");
        prev.Liquidacion.Lineas.Single(l => l.LineaRecepcionId == sinCaja).PrecioKg.Should().Be(0.50m, "el del día sin envase");

        // Sustituir el precio del día: se actualiza y la valoración lo toma.
        (await OkAsync<MasivoResp>(await api.PostAsJsonAsync($"/agro/campanas/{campana}/precios/masivo", new
        {
            Precios = new[] { new { ProductoId = tomate, Desde = hoy, Hasta = hoy, PrecioKg = 0.48m, Tipo = "Dia" } },
        }))).Actualizados.Should().Be(1);
        (await OkAsync<PrevResp>(await api.PostAsJsonAsync("/agro/liquidaciones/previsualizar", datos))).Liquidacion!.Lineas
            .Single(l => l.LineaRecepcionId == sinCaja).PrecioKg.Should().Be(0.48m);
        (await api.PostAsJsonAsync($"/agro/campanas/{campana}/precios", new { ProductoId = tomate, Desde = hoy, Hasta = hoy.AddDays(1), PrecioKg = 1m, Tipo = "Dia" }))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest, "un precio del día vale para un día");

        // A resultas: 1.000 kg vendidos a 1,10 − 10 % − 0,20 €/kg = 0,79.
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Frutas del Norte SA", NifFiscal = Ayudas.GenerarNif() });
        (await api.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente, Lineas = new[] { new { ProductoId = tomate, Descripcion = "Tomate", Cantidad = 1_000m, PrecioUnitario = 1.10m, CodigoIva = "IVA4" } },
        })).StatusCode.Should().Be(HttpStatusCode.Created);
        var propuesta = await OkAsync<List<PropuestaResp>>(await api.PostAsJsonAsync($"/agro/campanas/{campana}/precios/propuesta-ventas", new
        {
            Desde = mes, Hasta = mes.AddMonths(1).AddDays(-1), DeduccionKg = 0.20m, DeduccionPorcentaje = 10m,
        }));
        propuesta.Single().Should().Match<PropuestaResp>(p => p.ProductoId == tomate && p.KilosVendidos == 1_000m && p.PrecioMedioVenta == 1.10m && p.PrecioPropuesto == 0.79m && p.Aviso == null);
    }
}
