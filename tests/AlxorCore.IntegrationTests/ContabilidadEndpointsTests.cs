using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de integración del módulo Contabilidad (partida doble) y su enganche con Recepción.</summary>
public sealed class ContabilidadEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public ContabilidadEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record ApunteResp(string CuentaCodigo, decimal Debe, decimal Haber);
    private sealed record AsientoResp(Guid Id, int Ejercicio, int Numero, string Concepto, string Origen, decimal Total, List<ApunteResp> Apuntes);
    private sealed record ModoResp(string Modo);
    private sealed record FacturaRecibidaResp(Guid Id, string Estado, Guid? GastoId);
    private sealed record ConfigResp(string Modo, bool ContabilizacionAutomatica);
    private sealed record PendienteResp(Guid Id, string Sentido, string OrigenTipo, string Referencia, string FechaDocumento, string FechaRegistro, decimal Total, string Estado, Guid? AsientoId);
    private sealed record FacturaResp(Guid Id, decimal Total);

    private static async Task PonerModoCompletoAsync(HttpClient cliente)
    {
        var modo = await cliente.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" });
        modo.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task<PendienteResp> RecibirYValidarCompraAsync(HttpClient cliente)
    {
        var pdf = Convert.ToBase64String(new byte[] { 0x25, 0x50, 0x44, 0x46 });
        var recibida = (await (await cliente.PostAsJsonAsync("/recepcion/facturas", new { NombreArchivo = "f.pdf", ContenidoBase64 = pdf, TipoContenido = "application/pdf" })).Content.ReadFromJsonAsync<FacturaRecibidaResp>())!;
        await cliente.PostAsJsonAsync($"/recepcion/facturas/{recibida.Id}/validar", new { BaseImponible = 300m, FechaFactura = "2026-08-01", ProveedorTexto = "Suministros Ebro SL", NumeroFactura = "P-1", CodigoIva = "IVA21", PorcentajeIrpf = 0m });
        var contab = await cliente.PostAsync($"/recepcion/facturas/{recibida.Id}/contabilizar", null);
        contab.StatusCode.Should().Be(HttpStatusCode.OK);

        var pendientes = await cliente.GetFromJsonAsync<List<PendienteResp>>("/contabilidad/pendientes");
        return pendientes!.Single(p => p.Sentido == "Compra");
    }

    [Fact]
    public async Task Modo_por_defecto_es_simple()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var modo = await cliente.GetFromJsonAsync<ModoResp>("/contabilidad/modo");
        modo!.Modo.Should().Be("Simple");
    }

    [Fact]
    public async Task Crear_asiento_manual_cuadrado_aparece_en_el_diario()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var crear = await cliente.PostAsJsonAsync("/contabilidad/asientos", new
        {
            Fecha = "2026-03-15",
            Concepto = "Aportación de socio",
            Lineas = new[]
            {
                new { CuentaCodigo = "572", Debe = 1000m, Haber = 0m, Concepto = (string?)null },
                new { CuentaCodigo = "100", Debe = 0m, Haber = 1000m, Concepto = (string?)null },
            },
        });
        crear.StatusCode.Should().Be(HttpStatusCode.Created);

        var diario = await cliente.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2026");
        diario!.Should().ContainSingle(a => a.Concepto == "Aportación de socio" && a.Total == 1000m);
    }

    [Fact]
    public async Task Asiento_descuadrado_devuelve_400()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var crear = await cliente.PostAsJsonAsync("/contabilidad/asientos", new
        {
            Fecha = "2026-03-15",
            Concepto = "Descuadrado",
            Lineas = new[]
            {
                new { CuentaCodigo = "572", Debe = 100m, Haber = 0m },
                new { CuentaCodigo = "700", Debe = 0m, Haber = 90m },
            },
        });
        crear.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Por_defecto_una_compra_queda_pendiente_de_contabilizar_sin_generar_asiento()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await PonerModoCompletoAsync(cliente);

        var pendiente = await RecibirYValidarCompraAsync(cliente);

        // Queda pendiente: aún no hay asiento en el diario.
        pendiente.Estado.Should().Be("Pendiente");
        pendiente.AsientoId.Should().BeNull();
        var diario = await cliente.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2026");
        diario!.Should().NotContain(a => a.Origen == "Compra");
    }

    [Fact]
    public async Task El_contable_ajusta_la_fecha_de_registro_y_contabiliza_desde_el_panel()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await PonerModoCompletoAsync(cliente);
        var pendiente = await RecibirYValidarCompraAsync(cliente);

        // La factura es del 01/08 pero el contable la registra en septiembre.
        var cambio = await cliente.PutAsJsonAsync($"/contabilidad/pendientes/{pendiente.Id}/fecha-registro", new { Fecha = "2026-09-30" });
        cambio.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var contab = await cliente.PostAsJsonAsync("/contabilidad/pendientes/contabilizar", new { Ids = new[] { pendiente.Id } });
        contab.StatusCode.Should().Be(HttpStatusCode.OK);

        // El asiento de compra (base 300, IVA 21 %) queda cuadrado y con la fecha de registro.
        var diario = await cliente.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2026");
        var asiento = diario!.Single(a => a.Origen == "Compra");
        asiento.Total.Should().Be(363m);
        asiento.Apuntes.Sum(x => x.Debe).Should().Be(asiento.Apuntes.Sum(x => x.Haber));
        asiento.Apuntes.Should().Contain(x => x.CuentaCodigo == "629" && x.Debe == 300m);
        asiento.Apuntes.Should().Contain(x => x.CuentaCodigo == "472" && x.Debe == 63m);
        asiento.Apuntes.Should().Contain(x => x.CuentaCodigo == "400" && x.Haber == 363m);

        // Ya no aparece entre los pendientes.
        var pendientes = await cliente.GetFromJsonAsync<List<PendienteResp>>("/contabilidad/pendientes");
        pendientes!.Should().NotContain(p => p.Id == pendiente.Id);
    }

    [Fact]
    public async Task Con_contabilizacion_automatica_la_compra_se_asienta_al_instante()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await PonerModoCompletoAsync(cliente);

        var config = await (await cliente.PutAsJsonAsync("/contabilidad/contabilizacion-automatica", new { Automatica = true })).Content.ReadFromJsonAsync<ConfigResp>();
        config!.ContabilizacionAutomatica.Should().BeTrue();

        var pdf = Convert.ToBase64String(new byte[] { 0x25, 0x50, 0x44, 0x46 });
        var recibida = (await (await cliente.PostAsJsonAsync("/recepcion/facturas", new { NombreArchivo = "f.pdf", ContenidoBase64 = pdf, TipoContenido = "application/pdf" })).Content.ReadFromJsonAsync<FacturaRecibidaResp>())!;
        await cliente.PostAsJsonAsync($"/recepcion/facturas/{recibida.Id}/validar", new { BaseImponible = 300m, FechaFactura = "2026-08-01", ProveedorTexto = "Suministros Ebro SL", NumeroFactura = "P-1", CodigoIva = "IVA21", PorcentajeIrpf = 0m });
        await cliente.PostAsync($"/recepcion/facturas/{recibida.Id}/contabilizar", null);

        // No queda nada pendiente y el asiento ya está en el diario.
        var pendientes = await cliente.GetFromJsonAsync<List<PendienteResp>>("/contabilidad/pendientes");
        pendientes!.Should().BeEmpty();
        var diario = await cliente.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2026");
        diario!.Should().ContainSingle(a => a.Origen == "Compra" && a.Total == 363m);
    }

    [Fact]
    public async Task Una_factura_de_venta_en_modo_completo_genera_asiento_de_ingreso_al_contabilizar()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await PonerModoCompletoAsync(cliente);

        var clienteId = (await (await cliente.PostAsJsonAsync("/clientes", new { Nombre = "Cliente Venta SL", NifFiscal = "B12345674" })).Content.ReadFromJsonAsync<FacturaResp>())!.Id;
        var factura = await (await cliente.PostAsJsonAsync("/facturas", new
        {
            ClienteId = clienteId,
            FechaEmision = "2026-08-10",
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = 1000m, CodigoIva = "IVA21" } },
        })).Content.ReadFromJsonAsync<FacturaResp>();

        var pendientes = await cliente.GetFromJsonAsync<List<PendienteResp>>("/contabilidad/pendientes");
        var venta = pendientes!.Single(p => p.Sentido == "Venta");
        venta.Total.Should().Be(1210m);

        var contab = await cliente.PostAsJsonAsync("/contabilidad/pendientes/contabilizar", new { Ids = new[] { venta.Id } });
        contab.StatusCode.Should().Be(HttpStatusCode.OK);

        var diario = await cliente.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2026");
        var asiento = diario!.Single(a => a.Origen == "Venta");
        asiento.Apuntes.Sum(x => x.Debe).Should().Be(asiento.Apuntes.Sum(x => x.Haber));
        asiento.Apuntes.Should().Contain(x => x.CuentaCodigo == "430" && x.Debe == 1210m);   // cliente
        asiento.Apuntes.Should().Contain(x => x.CuentaCodigo == "705" && x.Haber == 1000m);  // ingreso
        asiento.Apuntes.Should().Contain(x => x.CuentaCodigo == "477" && x.Haber == 210m);   // IVA repercutido
    }

    [Fact]
    public async Task Una_regla_por_familia_elige_la_cuenta_de_ingreso_al_contabilizar_una_venta()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await PonerModoCompletoAsync(cliente);
        await cliente.PutAsJsonAsync("/contabilidad/contabilizacion-automatica", new { Automatica = true });

        // Regla: las ventas de la familia «Mercaderías» van a la 700 (no a la genérica 705).
        var regla = await cliente.PostAsJsonAsync("/contabilidad/reglas", new { Sentido = "Venta", Familia = "Mercaderías", TipoTercero = (string?)null, CuentaCodigo = "700" });
        regla.StatusCode.Should().Be(HttpStatusCode.Created);

        // Artículo con esa familia + cliente + factura.
        var prod = (await (await cliente.PostAsJsonAsync("/productos", new { Nombre = "Camisa", PrecioUnitario = 100m, CodigoIva = "IVA21", Tipo = "Bien", Familia = "Mercaderías" })).Content.ReadFromJsonAsync<FacturaResp>())!;
        var clienteId = (await (await cliente.PostAsJsonAsync("/clientes", new { Nombre = "Tienda SL", NifFiscal = "B12345674" })).Content.ReadFromJsonAsync<FacturaResp>())!.Id;
        await cliente.PostAsJsonAsync("/facturas", new
        {
            ClienteId = clienteId,
            FechaEmision = "2026-08-12",
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Camisa", PrecioUnitario = 100m, CodigoIva = "IVA21", ProductoId = prod.Id } },
        });

        var diario = await cliente.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2026");
        var asiento = diario!.Single(a => a.Origen == "Venta");
        asiento.Apuntes.Should().Contain(x => x.CuentaCodigo == "700" && x.Haber == 100m);
        asiento.Apuntes.Should().NotContain(x => x.CuentaCodigo == "705");
    }

    [Fact]
    public async Task Un_ticket_del_TPV_en_modo_completo_queda_pendiente_de_contabilizar()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await PonerModoCompletoAsync(cliente);

        var ticket = await cliente.PostAsJsonAsync("/tickets", new
        {
            ClienteId = (Guid?)null,
            Serie = (string?)null,
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Café", PrecioUnitario = 2m, CodigoIva = "IVA21" } },
        });
        ticket.StatusCode.Should().Be(HttpStatusCode.Created);

        var pendientes = await cliente.GetFromJsonAsync<List<PendienteResp>>("/contabilidad/pendientes");
        pendientes!.Should().Contain(p => p.Sentido == "Venta" && p.OrigenTipo == "Ticket");
    }

    [Fact]
    public async Task En_modo_simple_no_se_crean_documentos_pendientes()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        // Modo por defecto = Simple; recibir + validar + contabilizar una compra.
        var pdf = Convert.ToBase64String(new byte[] { 0x25, 0x50, 0x44, 0x46 });
        var recibida = (await (await cliente.PostAsJsonAsync("/recepcion/facturas", new { NombreArchivo = "f.pdf", ContenidoBase64 = pdf, TipoContenido = "application/pdf" })).Content.ReadFromJsonAsync<FacturaRecibidaResp>())!;
        await cliente.PostAsJsonAsync($"/recepcion/facturas/{recibida.Id}/validar", new { BaseImponible = 100m, FechaFactura = "2026-08-01", ProveedorTexto = "Prov", NumeroFactura = "P-9", CodigoIva = "IVA21", PorcentajeIrpf = 0m });
        await cliente.PostAsync($"/recepcion/facturas/{recibida.Id}/contabilizar", null);

        var pendientes = await cliente.GetFromJsonAsync<List<PendienteResp>>("/contabilidad/pendientes");
        pendientes!.Should().BeEmpty();
    }
}
