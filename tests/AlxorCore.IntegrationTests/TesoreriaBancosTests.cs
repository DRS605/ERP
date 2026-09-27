using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Tesorería con cuentas bancarias: bancos y cajas con su subcuenta, cobros y pagos por banco, remesas SEPA
/// registradas (no se remesa dos veces), devoluciones de recibos, conciliación bancaria persistente y saldos para la
/// previsión.
/// </summary>
public sealed class TesoreriaBancosTests : IClassFixture<FabricaApiPruebas>
{
    private const string IbanBanco = "ES9121000418450200051332";
    private const string IbanBanco2 = "ES7921000813610123456789";
    private const string IbanCliente = "ES7620770024003102575766";

    private readonly FabricaApiPruebas _fabrica;

    public TesoreriaBancosTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record FacturaResp(Guid Id, decimal Total, string NumeroCompleto);
    private sealed record BancoResp(Guid Id, string Tipo, string Nombre, string? Iban, string Subcuenta, bool Activa, bool Predeterminada);
    private sealed record CuentaResp(string Codigo, string Nombre);
    private sealed record ProblemaResp(string Codigo);
    private sealed record ApunteResp(string CuentaCodigo, decimal Debe, decimal Haber);
    private sealed record AsientoResp(Guid Id, string Origen, string Concepto, List<ApunteResp> Apuntes);
    private sealed record MovimientoResp(Guid Id, string Sentido, decimal Importe, string? Metodo, Guid? CuentaBancariaId, Guid? AnulaMovimientoId, bool Anulado);
    private sealed record SaldoResp(decimal Total, decimal Liquidado, decimal Pendiente, string Estado, List<MovimientoResp> Movimientos);
    private sealed record LineaRemesaResp(Guid Id, string Documento, decimal Importe, Guid? MovimientoId, bool Devuelta, string? MotivoDevolucion);
    private sealed record RemesaResp(Guid Id, string Tipo, string Codigo, string Estado, string EstadoTexto, decimal Total, int NumeroLineas, Guid? CuentaBancariaId,
        List<LineaRemesaResp> Lineas);
    private sealed record RemesaCreadaResp(RemesaResp Remesa, List<string> Omitidos);
    private sealed record FicheroResp(string Fichero, string NombreArchivo);
    private sealed record ImpagadoResp(Guid FacturaId, decimal Pendiente, string? MotivoDevolucion, string? DescripcionDevolucion, decimal GastosDevolucion);
    private sealed record EfectoResp(Guid Id, string Documento, decimal Importe, decimal Pendiente);
    private sealed record CasacionResp(Guid MovimientoId, string TipoDocumento, Guid DocumentoId, decimal Importe, bool MovimientoCreado);
    private sealed record ApunteBancoResp(Guid Id, int Orden, decimal Importe, string Concepto, string Estado, string? CuentaAsiento, List<CasacionResp> Casaciones);
    private sealed record ExtractoResp(Guid Id, Guid CuentaBancariaId, int NumeroApuntes, int Pendientes);
    private sealed record ExtractoDetalleResp(ExtractoResp Extracto, List<ApunteBancoResp> Apuntes);
    private sealed record AutoResp(int Conciliados, int Pendientes, List<string> Detalle);
    private sealed record CandidatoResp(string Tipo, Guid Id, string Documento, decimal Importe);
    private sealed record SaldoCuentaResp(Guid? Id, string Nombre, decimal? SaldoContable, decimal SaldoMovimientos, decimal Saldo);
    private sealed record SaldosResp(List<SaldoCuentaResp> Cuentas, decimal Total, string Fuente);

    private static readonly int Anio = DateTime.UtcNow.Year;
    private static string Hoy => DateTime.UtcNow.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

    private static async Task<HttpClient> EmpresaAsync(FabricaApiPruebas fabrica, bool completa)
    {
        var (c, _) = await Ayudas.ConEmpresaAsync(fabrica);
        if (completa)
        {
            (await c.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();
            (await c.PutAsJsonAsync("/contabilidad/contabilizacion-automatica", new { Automatica = true })).EnsureSuccessStatusCode();
        }

        (await c.PutAsJsonAsync("/empresas/actual/cobro", new { Iban = IbanBanco, IdentificadorAcreedor = "ES12345M1234567890" })).EnsureSuccessStatusCode();
        return c;
    }

    private static async Task<BancoResp> BancoAsync(HttpClient c, string nombre, string? iban = IbanBanco, string tipo = "Banco", bool predeterminada = false, decimal saldoInicial = 0m)
    {
        var r = await c.PostAsJsonAsync("/cuentas-bancarias", new { Tipo = tipo, Nombre = nombre, Iban = iban, Predeterminada = predeterminada, SaldoInicial = saldoInicial });
        r.StatusCode.Should().Be(HttpStatusCode.Created, await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<BancoResp>())!;
    }

    private static async Task<Guid> ClienteDomiciliadoAsync(HttpClient c, string nombre) =>
        (await (await c.PostAsJsonAsync("/clientes", new { Nombre = nombre, NifFiscal = Ayudas.GenerarNif(), Iban = IbanCliente, MandatoReferencia = "MND-" + nombre[..3], MandatoFecha = "2025-01-10" }))
            .Content.ReadFromJsonAsync<IdResp>())!.Id;

    private static async Task<FacturaResp> FacturaAsync(HttpClient c, Guid cliente, decimal precio) =>
        (await (await c.PostAsJsonAsync("/facturas", new { ClienteId = cliente, Lineas = new[] { new { Cantidad = 1m, Descripcion = "Cuota", PrecioUnitario = precio, CodigoIva = "IVA0" } } }))
            .Content.ReadFromJsonAsync<FacturaResp>())!;

    private static async Task<List<AsientoResp>> DiarioAsync(HttpClient c) =>
        (await c.GetFromJsonAsync<List<AsientoResp>>($"/contabilidad/diario?ejercicio={Anio}"))!;

    private static decimal Debe(IEnumerable<AsientoResp> a, string cuenta) => a.SelectMany(x => x.Apuntes).Where(p => p.CuentaCodigo == cuenta).Sum(p => p.Debe);

    private static decimal Haber(IEnumerable<AsientoResp> a, string cuenta) => a.SelectMany(x => x.Apuntes).Where(p => p.CuentaCodigo == cuenta).Sum(p => p.Haber);

    // ---------------------------------------------------------------- cuentas bancarias

    [Fact]
    public async Task Alta_de_bancos_valida_el_iban_crea_la_subcuenta_y_elige_la_predeterminada()
    {
        var c = await EmpresaAsync(_fabrica, completa: true);

        var mal = await c.PostAsJsonAsync("/cuentas-bancarias", new { Nombre = "Malo", Iban = "ES1234567890123456789012" });
        mal.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await mal.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("banco.iban_invalido");

        var b1 = await BancoAsync(c, "BBVA principal", "ES91 2100 0418 4502 0005 1332");
        b1.Iban.Should().Be(IbanBanco);
        b1.Subcuenta.Should().StartWith("572").And.HaveLength(8);
        b1.Predeterminada.Should().BeTrue("el primer banco es el predeterminado");

        var b2 = await BancoAsync(c, "Santander", IbanBanco2, predeterminada: true);
        b2.Subcuenta.Should().NotBe(b1.Subcuenta);
        var caja = await BancoAsync(c, "Caja tienda", iban: null, tipo: "Caja");
        caja.Subcuenta.Should().StartWith("570");

        var lista = (await c.GetFromJsonAsync<List<BancoResp>>("/cuentas-bancarias"))!;
        lista.Should().HaveCount(3);
        lista.Single(b => b.Predeterminada).Id.Should().Be(b2.Id);

        // Las subcuentas quedan en el plan de cuentas.
        var plan = (await c.GetFromJsonAsync<List<CuentaResp>>("/contabilidad/cuentas"))!;
        plan.Select(x => x.Codigo).Should().Contain([b1.Subcuenta, b2.Subcuenta, caja.Subcuenta, "626"]);

        // Mismo IBAN otra vez → conflicto; la subcuenta no se cambia.
        (await c.PostAsJsonAsync("/cuentas-bancarias", new { Nombre = "Duplicada", Iban = IbanBanco })).StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await c.PutAsJsonAsync($"/cuentas-bancarias/{b1.Id}", new { Nombre = "BBVA", Iban = IbanBanco, Subcuenta = "5729999", Activa = true }))
            .StatusCode.Should().Be(HttpStatusCode.Conflict);

        // Una cuenta sin uso se borra.
        (await c.DeleteAsync($"/cuentas-bancarias/{caja.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task El_cobro_va_a_la_subcuenta_del_banco_elegido_o_del_predeterminado_y_la_caja_en_efectivo()
    {
        var c = await EmpresaAsync(_fabrica, completa: true);
        var principal = await BancoAsync(c, "Principal");
        var otro = await BancoAsync(c, "Secundario", IbanBanco2);
        var caja = await BancoAsync(c, "Caja", iban: null, tipo: "Caja");
        var cliente = await ClienteDomiciliadoAsync(c, "Bar Uno");
        var f = await FacturaAsync(c, cliente, 300m);

        (await c.PostAsJsonAsync("/cobros", new { FacturaId = f.Id, Importe = 100m, Metodo = "Transferencia" })).EnsureSuccessStatusCode();
        (await c.PostAsJsonAsync("/cobros", new { FacturaId = f.Id, Importe = 50m, Metodo = "Transferencia", CuentaBancariaId = otro.Id })).EnsureSuccessStatusCode();
        (await c.PostAsJsonAsync("/cobros", new { FacturaId = f.Id, Importe = 20m, Metodo = "Efectivo" })).EnsureSuccessStatusCode();

        var cobros = (await DiarioAsync(c)).Where(a => a.Origen == "Cobro").ToList();
        Debe(cobros, principal.Subcuenta).Should().Be(100m);
        Debe(cobros, otro.Subcuenta).Should().Be(50m);
        Debe(cobros, caja.Subcuenta).Should().Be(20m);
        Debe(cobros, "572").Should().Be(0m);

        var saldo = (await c.GetFromJsonAsync<SaldoResp>($"/facturas/{f.Id}/saldo"))!;
        saldo.Movimientos.Select(m => m.CuentaBancariaId).Should().BeEquivalentTo(new Guid?[] { principal.Id, otro.Id, caja.Id });

        // Anular el cobro del banco secundario revierte contra esa misma subcuenta.
        var mov = saldo.Movimientos.Single(m => m.CuentaBancariaId == otro.Id);
        (await c.PostAsJsonAsync($"/tesoreria/movimientos/{mov.Id}/anular", new { })).EnsureSuccessStatusCode();
        Haber((await DiarioAsync(c)).Where(a => a.Origen == "Cobro"), otro.Subcuenta).Should().Be(50m);

        // Un banco en uso no se borra (se desactiva) y uno inactivo no admite cobros.
        (await c.DeleteAsync($"/cuentas-bancarias/{otro.Id}")).StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await c.PutAsJsonAsync($"/cuentas-bancarias/{otro.Id}", new { Nombre = "Secundario", Iban = IbanBanco2, Activa = false })).EnsureSuccessStatusCode();
        (await c.PostAsJsonAsync("/cobros", new { FacturaId = f.Id, Importe = 1m, CuentaBancariaId = otro.Id })).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Sin_cuentas_bancarias_el_pago_sigue_yendo_a_572()
    {
        var c = await EmpresaAsync(_fabrica, completa: true);
        var gasto = (await (await c.PostAsJsonAsync("/gastos", new { Concepto = "Material", BaseImponible = 100m, CodigoIva = "IVA0" })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        (await c.PostAsJsonAsync("/pagos", new { GastoId = gasto, Importe = 100m, Metodo = "Transferencia" })).EnsureSuccessStatusCode();
        Haber((await DiarioAsync(c)).Where(a => a.Origen == "Pago"), "572").Should().Be(100m);
    }

    // ---------------------------------------------------------------- remesas y devoluciones

    [Fact]
    public async Task La_remesa_queda_registrada_no_se_remesa_dos_veces_y_al_cobrarla_registra_los_cobros_contra_el_banco()
    {
        var c = await EmpresaAsync(_fabrica, completa: true);
        var banco = await BancoAsync(c, "Banco de remesas");
        var cliente = await ClienteDomiciliadoAsync(c, "Gimnasio Norte");
        var f1 = await FacturaAsync(c, cliente, 40m);
        var f2 = await FacturaAsync(c, cliente, 60m);

        var crear = await c.PostAsJsonAsync("/tesoreria/remesas", new { Tipo = "Cobro", FacturaIds = new[] { f1.Id, f2.Id }, FechaCargo = Hoy });
        crear.StatusCode.Should().Be(HttpStatusCode.Created, await crear.Content.ReadAsStringAsync());
        var remesa = (await crear.Content.ReadFromJsonAsync<RemesaCreadaResp>())!.Remesa;
        remesa.Estado.Should().Be("Generada");
        remesa.Total.Should().Be(100m);
        remesa.CuentaBancariaId.Should().Be(banco.Id);

        // La misma factura no entra en otra remesa mientras esta está viva (tampoco por la ruta antigua).
        var otra = await c.PostAsJsonAsync("/tesoreria/remesa", new { FacturaIds = new[] { f1.Id } });
        otra.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await otra.Content.ReadAsStringAsync()).Should().Contain("ya está en la remesa");

        // El fichero se vuelve a descargar.
        var fichero = (await c.GetFromJsonAsync<FicheroResp>($"/tesoreria/remesas/{remesa.Id}/fichero"))!;
        fichero.Fichero.Should().Contain("pain.008.001.02").And.Contain(IbanBanco).And.Contain("100.00");

        (await c.PostAsJsonAsync($"/tesoreria/remesas/{remesa.Id}/presentar", new { })).EnsureSuccessStatusCode();
        var liquidada = await c.PostAsJsonAsync($"/tesoreria/remesas/{remesa.Id}/liquidar", new { Fecha = Hoy });
        liquidada.StatusCode.Should().Be(HttpStatusCode.OK, await liquidada.Content.ReadAsStringAsync());
        var r = (await liquidada.Content.ReadFromJsonAsync<RemesaResp>())!;
        r.EstadoTexto.Should().Be("Cobrada");
        r.Lineas.Should().OnlyContain(l => l.MovimientoId != null);

        (await c.GetFromJsonAsync<SaldoResp>($"/facturas/{f1.Id}/saldo"))!.Pendiente.Should().Be(0m);
        var cobros = (await DiarioAsync(c)).Where(a => a.Origen == "Cobro").ToList();
        Debe(cobros, banco.Subcuenta).Should().Be(100m);

        // Una remesa cobrada ya no se anula (se registran devoluciones) ni se liquida otra vez.
        (await c.PostAsJsonAsync($"/tesoreria/remesas/{remesa.Id}/anular", new { })).StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await c.PostAsJsonAsync($"/tesoreria/remesas/{remesa.Id}/liquidar", new { })).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Anular_una_remesa_viva_libera_sus_facturas()
    {
        var c = await EmpresaAsync(_fabrica, completa: false);
        var cliente = await ClienteDomiciliadoAsync(c, "Academia");
        var f = await FacturaAsync(c, cliente, 75m);
        var r1 = (await (await c.PostAsJsonAsync("/tesoreria/remesas", new { Tipo = "Cobro", FacturaIds = new[] { f.Id } })).Content.ReadFromJsonAsync<RemesaCreadaResp>())!.Remesa;
        (await c.PostAsJsonAsync($"/tesoreria/remesas/{r1.Id}/anular", new { })).EnsureSuccessStatusCode();

        var r2 = await c.PostAsJsonAsync("/tesoreria/remesas", new { Tipo = "Cobro", FacturaIds = new[] { f.Id } });
        r2.StatusCode.Should().Be(HttpStatusCode.Created);
        var lista = (await c.GetFromJsonAsync<List<RemesaResp>>("/tesoreria/remesas?tipo=Cobro"))!;
        lista.Select(x => x.Estado).Should().BeEquivalentTo(["Anulada", "Generada"]);
        lista.Select(x => x.Codigo).Distinct().Should().HaveCount(2);
    }

    [Fact]
    public async Task La_remesa_de_transferencias_se_registra_y_al_pagarla_salen_los_pagos_del_banco()
    {
        var c = await EmpresaAsync(_fabrica, completa: true);
        var banco = await BancoAsync(c, "Pagos");
        var prov = (await (await c.PostAsJsonAsync("/proveedores", new { Nombre = "Suministros Ebro SL", NifFiscal = Ayudas.GenerarNif(), Iban = IbanCliente })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        var gasto = (await (await c.PostAsJsonAsync("/gastos", new { Concepto = "Material", ProveedorId = prov, BaseImponible = 100m, CodigoIva = "IVA21" })).Content.ReadFromJsonAsync<IdResp>())!.Id;

        var remesa = (await (await c.PostAsJsonAsync("/tesoreria/remesas", new { Tipo = "Pago", GastoIds = new[] { gasto }, FechaCargo = Hoy })).Content.ReadFromJsonAsync<RemesaCreadaResp>())!.Remesa;
        remesa.Total.Should().Be(121m);
        (await c.PostAsJsonAsync("/tesoreria/transferencias", new { GastoIds = new[] { gasto } })).StatusCode.Should().Be(HttpStatusCode.BadRequest, "ya está en una remesa viva");

        (await c.PostAsJsonAsync($"/tesoreria/remesas/{remesa.Id}/liquidar", new { })).EnsureSuccessStatusCode();
        (await c.GetFromJsonAsync<SaldoResp>($"/gastos/{gasto}/saldo"))!.Pendiente.Should().Be(0m);
        Haber((await DiarioAsync(c)).Where(a => a.Origen == "Pago"), banco.Subcuenta).Should().Be(121m);
    }

    [Fact]
    public async Task Una_devolucion_reabre_el_pendiente_contabiliza_el_contraasiento_y_los_gastos_y_sale_en_impagados()
    {
        var c = await EmpresaAsync(_fabrica, completa: true);
        var banco = await BancoAsync(c, "Banco devoluciones");
        var cliente = await ClienteDomiciliadoAsync(c, "Cliente Moroso");
        var f1 = await FacturaAsync(c, cliente, 80m);
        var f2 = await FacturaAsync(c, cliente, 20m);
        var remesa = (await (await c.PostAsJsonAsync("/tesoreria/remesas", new { Tipo = "Cobro", FacturaIds = new[] { f1.Id, f2.Id }, FechaCargo = Hoy }))
            .Content.ReadFromJsonAsync<RemesaCreadaResp>())!.Remesa;
        var liquidada = (await (await c.PostAsJsonAsync($"/tesoreria/remesas/{remesa.Id}/liquidar", new { Fecha = Hoy })).Content.ReadFromJsonAsync<RemesaResp>())!;
        var linea1 = liquidada.Lineas.Single(l => l.Importe == 80m);
        var linea2 = liquidada.Lineas.Single(l => l.Importe == 20m);

        // Motivo desconocido → 400.
        (await c.PostAsJsonAsync("/tesoreria/devoluciones", new { MovimientoId = linea1.MovimientoId, Motivo = "XX99" })).StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var dev = await c.PostAsJsonAsync("/tesoreria/devoluciones", new { MovimientoId = linea1.MovimientoId, Motivo = "AM04", Fecha = Hoy, Gastos = 3.5m });
        dev.StatusCode.Should().Be(HttpStatusCode.Created, await dev.Content.ReadAsStringAsync());

        // Pendiente reabierto, de forma trazable (anulación del cobro con el motivo).
        var saldo = (await c.GetFromJsonAsync<SaldoResp>($"/facturas/{f1.Id}/saldo"))!;
        saldo.Pendiente.Should().Be(80m);
        saldo.Movimientos.Should().Contain(m => m.AnulaMovimientoId == linea1.MovimientoId && m.Metodo == "Devolución AM04");

        // Dos veces no.
        (await c.PostAsJsonAsync("/tesoreria/devoluciones", new { MovimientoId = linea1.MovimientoId, Motivo = "AM04" })).StatusCode.Should().Be(HttpStatusCode.Conflict);

        // Asientos: 430 contra el banco por el recibo, y 626 contra el banco por los gastos.
        var diario = await DiarioAsync(c);
        var contra = diario.Single(a => a.Origen == "Cobro" && a.Concepto.StartsWith("Devolución AM04", StringComparison.Ordinal));
        Haber([contra], banco.Subcuenta).Should().Be(80m);
        contra.Apuntes.Single(p => p.Debe > 0).CuentaCodigo.Should().StartWith("430");
        var gastos = diario.Single(a => a.Concepto.StartsWith("Gastos devolución", StringComparison.Ordinal));
        Debe([gastos], "626").Should().Be(3.5m);
        Haber([gastos], banco.Subcuenta).Should().Be(3.5m);

        // Aparece en impagados con el motivo, aunque la factura no esté vencida.
        var impagados = (await c.GetFromJsonAsync<List<ImpagadoResp>>("/impagados"))!;
        var imp = impagados.Single(i => i.FacturaId == f1.Id);
        imp.MotivoDevolucion.Should().Be("AM04");
        imp.DescripcionDevolucion.Should().Be("Fondos insuficientes");
        imp.GastosDevolucion.Should().Be(3.5m);

        // La línea de la remesa figura devuelta.
        var detalle = (await c.GetFromJsonAsync<RemesaResp>($"/tesoreria/remesas/{remesa.Id}"))!;
        detalle.Lineas.Single(l => l.Id == linea1.Id).Devuelta.Should().BeTrue();

        // Gastos repercutidos al cliente: efecto a cobrar y cargo en su cuenta.
        (await c.PostAsJsonAsync("/tesoreria/devoluciones", new { MovimientoId = linea2.MovimientoId, Motivo = "MD06", Fecha = Hoy, Gastos = 2m, RepercutirGastos = true }))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        var efectos = (await c.GetFromJsonAsync<List<EfectoResp>>("/cartera?pendientes=true"))!;
        efectos.Should().ContainSingle(e => e.Documento.StartsWith("Gastos devolución") && e.Importe == 2m);
        var repercutidos = (await DiarioAsync(c)).Single(a => a.Concepto.Contains("a cargo del cliente", StringComparison.Ordinal));
        repercutidos.Apuntes.Single(p => p.Debe == 2m).CuentaCodigo.Should().StartWith("430");
        Haber([repercutidos], banco.Subcuenta).Should().Be(2m);

        // Un cobro que no es por domiciliación no se "devuelve": se anula.
        var f3 = await FacturaAsync(c, cliente, 10m);
        (await c.PostAsJsonAsync("/cobros", new { FacturaId = f3.Id, Importe = 10m, Metodo = "Transferencia" })).EnsureSuccessStatusCode();
        var mov = (await c.GetFromJsonAsync<SaldoResp>($"/facturas/{f3.Id}/saldo"))!.Movimientos.Single();
        (await c.PostAsJsonAsync("/tesoreria/devoluciones", new { MovimientoId = mov.Id, Motivo = "AM04" })).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // ---------------------------------------------------------------- conciliación bancaria

    private static string RegistroN43(string codigo, params (int Inicio, string Valor)[] campos)
    {
        var buf = new char[80];
        Array.Fill(buf, ' ');
        for (var i = 0; i < codigo.Length; i++)
        {
            buf[i] = codigo[i];
        }

        foreach (var (inicio, valor) in campos)
        {
            for (var i = 0; i < valor.Length && inicio + i < 80; i++)
            {
                buf[inicio + i] = valor[i];
            }
        }

        return new string(buf);
    }

    private static string ImporteN43(decimal valor) => ((long)(Math.Abs(valor) * 100)).ToString("D14", System.Globalization.CultureInfo.InvariantCulture);

    private static string FechaN43(DateTime d) => d.ToString("yyMMdd", System.Globalization.CultureInfo.InvariantCulture);

    private static string Apunte(DateTime fecha, decimal importe, string concepto) =>
        RegistroN43("22", (10, FechaN43(fecha)), (16, FechaN43(fecha)), (22, "04"), (27, importe < 0 ? "1" : "2"), (28, ImporteN43(importe))) + "\r\n"
        + RegistroN43("23", (2, "01"), (4, concepto));

    private static string ExtractoN43(params (decimal Importe, string Concepto)[] apuntes)
    {
        var hoy = DateTime.UtcNow;
        var cab = RegistroN43("11", (2, "210004180200051332"), (20, FechaN43(hoy.AddDays(-10))), (26, FechaN43(hoy)), (32, "2"), (33, ImporteN43(1000m)));
        return string.Join("\r\n", new[] { cab }.Concat(apuntes.Select(a => Apunte(hoy, a.Importe, a.Concepto))).Append(RegistroN43("88")));
    }

    [Fact]
    public async Task La_conciliacion_se_guarda_casa_sola_lo_claro_y_permite_casar_a_mano_asentar_y_deshacer()
    {
        var c = await EmpresaAsync(_fabrica, completa: true);
        var banco = await BancoAsync(c, "Cuenta conciliada");
        var cliente = await ClienteDomiciliadoAsync(c, "Frutas Levante");
        var remesadas1 = await FacturaAsync(c, cliente, 30m);
        var remesadas2 = await FacturaAsync(c, cliente, 45m);
        var pendiente = await FacturaAsync(c, cliente, 123.45m);
        var n1 = await FacturaAsync(c, cliente, 10m);
        var n2 = await FacturaAsync(c, cliente, 15m);

        // Remesa cobrada (75 €) que el banco abona en un solo apunte.
        var remesa = (await (await c.PostAsJsonAsync("/tesoreria/remesas", new { Tipo = "Cobro", FacturaIds = new[] { remesadas1.Id, remesadas2.Id }, FechaCargo = Hoy }))
            .Content.ReadFromJsonAsync<RemesaCreadaResp>())!.Remesa;
        (await c.PostAsJsonAsync($"/tesoreria/remesas/{remesa.Id}/liquidar", new { Fecha = Hoy })).EnsureSuccessStatusCode();

        var contenido = ExtractoN43((75m, "ABONO REMESA ADEUDOS"), (123.45m, "TRANSF " + pendiente.NumeroCompleto), (25m, "TRANSFERENCIA FRUTAS LEVANTE"), (-4.5m, "COMISION MANTENIMIENTO"));
        var imp = await c.PostAsJsonAsync("/tesoreria/extractos", new { Contenido = contenido, NombreArchivo = "extracto.n43" });
        imp.StatusCode.Should().Be(HttpStatusCode.Created, await imp.Content.ReadAsStringAsync());
        var extracto = (await imp.Content.ReadFromJsonAsync<ExtractoDetalleResp>())!;
        extracto.Extracto.CuentaBancariaId.Should().Be(banco.Id, "se detecta por la cuenta del fichero");
        extracto.Apuntes.Should().HaveCount(4).And.OnlyContain(a => a.Estado == "Pendiente");

        // El mismo fichero no se importa dos veces.
        (await c.PostAsJsonAsync("/tesoreria/extractos", new { Contenido = contenido })).StatusCode.Should().Be(HttpStatusCode.Conflict);

        // Automática: la remesa por su total y la factura por su importe con su número en el concepto.
        var auto = (await (await c.PostAsJsonAsync($"/tesoreria/extractos/{extracto.Extracto.Id}/conciliar-automatico", new { DiasMargen = 5 })).Content.ReadFromJsonAsync<AutoResp>())!;
        auto.Conciliados.Should().Be(2);
        (await c.GetFromJsonAsync<SaldoResp>($"/facturas/{pendiente.Id}/saldo"))!.Pendiente.Should().Be(0m);

        // Sobrevive a recargar: se lee de nuevo desde la base de datos.
        var det = (await c.GetFromJsonAsync<ExtractoDetalleResp>($"/tesoreria/extractos/{extracto.Extracto.Id}"))!;
        var aRemesa = det.Apuntes.Single(a => a.Orden == 1);
        aRemesa.Estado.Should().Be("ConciliadoAutomatico");
        aRemesa.Casaciones.Should().HaveCount(2).And.OnlyContain(x => !x.MovimientoCreado);
        det.Apuntes.Single(a => a.Orden == 2).Casaciones.Single().MovimientoCreado.Should().BeTrue();

        // Manual: un apunte de 25 € contra dos facturas (10 + 15). Primero, un descuadre se rechaza.
        var a3 = det.Apuntes.Single(a => a.Orden == 3);
        (await c.GetFromJsonAsync<List<CandidatoResp>>($"/tesoreria/apuntes/{a3.Id}/candidatos"))!.Select(x => x.Id).Should().Contain([n1.Id, n2.Id]);
        (await c.PostAsJsonAsync($"/tesoreria/apuntes/{a3.Id}/conciliar", new { Elementos = new[] { new { Tipo = "Factura", Id = n1.Id } } }))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var manual = await c.PostAsJsonAsync($"/tesoreria/apuntes/{a3.Id}/conciliar", new { Elementos = new[] { new { Tipo = "Factura", Id = n1.Id }, new { Tipo = "Factura", Id = n2.Id } } });
        manual.StatusCode.Should().Be(HttpStatusCode.OK, await manual.Content.ReadAsStringAsync());
        (await manual.Content.ReadFromJsonAsync<ApunteBancoResp>())!.Estado.Should().Be("ConciliadoManual");
        (await c.GetFromJsonAsync<SaldoResp>($"/facturas/{n2.Id}/saldo"))!.Pendiente.Should().Be(0m);

        // La comisión, con un asiento directo a 626 contra el banco.
        var a4 = det.Apuntes.Single(a => a.Orden == 4);
        (await c.PostAsJsonAsync($"/tesoreria/apuntes/{a4.Id}/asiento", new { Cuenta = "626", Concepto = "Comisión de mantenimiento" })).EnsureSuccessStatusCode();
        var comision = (await DiarioAsync(c)).Single(a => a.Concepto.StartsWith("Comisión de mantenimiento", StringComparison.Ordinal));
        Debe([comision], "626").Should().Be(4.5m);
        Haber([comision], banco.Subcuenta).Should().Be(4.5m);

        det = (await c.GetFromJsonAsync<ExtractoDetalleResp>($"/tesoreria/extractos/{extracto.Extracto.Id}"))!;
        det.Extracto.Pendientes.Should().Be(0);
        det.Apuntes.Single(a => a.Orden == 4).CuentaAsiento.Should().Be("626");

        // Deshacer: lo registrado por la conciliación se anula y el apunte vuelve a pendiente.
        (await c.PostAsJsonAsync($"/tesoreria/apuntes/{a3.Id}/deshacer", new { })).EnsureSuccessStatusCode();
        (await c.GetFromJsonAsync<SaldoResp>($"/facturas/{n1.Id}/saldo"))!.Pendiente.Should().Be(10m);
        (await c.PostAsJsonAsync($"/tesoreria/apuntes/{a4.Id}/deshacer", new { })).EnsureSuccessStatusCode();
        var anulacion = (await DiarioAsync(c)).Single(a => a.Concepto.StartsWith("Anulación: Comisión", StringComparison.Ordinal));
        Haber([anulacion], "626").Should().Be(4.5m);

        // Un extracto con apuntes conciliados no se borra.
        (await c.DeleteAsync($"/tesoreria/extractos/{extracto.Extracto.Id}")).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // ---------------------------------------------------------------- saldos para la previsión

    [Fact]
    public async Task Los_saldos_de_tesoreria_salen_de_la_contabilidad_o_de_los_movimientos()
    {
        // Modo Simple: saldo inicial + cobros − pagos de cada cuenta.
        var simple = await EmpresaAsync(_fabrica, completa: false);
        var banco = await BancoAsync(simple, "Banco", saldoInicial: 1000m);
        var cliente = await ClienteDomiciliadoAsync(simple, "Cliente saldo");
        var f = await FacturaAsync(simple, cliente, 200m);
        (await simple.PostAsJsonAsync("/cobros", new { FacturaId = f.Id, Importe = 200m, Metodo = "Transferencia" })).EnsureSuccessStatusCode();
        var gasto = (await (await simple.PostAsJsonAsync("/gastos", new { Concepto = "Alquiler", BaseImponible = 50m, CodigoIva = "IVA0" })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        (await simple.PostAsJsonAsync("/pagos", new { GastoId = gasto, Importe = 50m, Metodo = "Transferencia" })).EnsureSuccessStatusCode();

        var s1 = (await simple.GetFromJsonAsync<SaldosResp>("/cuentas-bancarias/saldos"))!;
        s1.Fuente.Should().Be("Movimientos");
        s1.Cuentas.Single(x => x.Id == banco.Id).Saldo.Should().Be(1150m);
        s1.Total.Should().Be(1150m);

        // Modo Completo: el saldo contable de la subcuenta.
        var completa = await EmpresaAsync(_fabrica, completa: true);
        var b2 = await BancoAsync(completa, "Banco contable");
        var cliente2 = await ClienteDomiciliadoAsync(completa, "Cliente contable");
        var f2 = await FacturaAsync(completa, cliente2, 300m);
        (await completa.PostAsJsonAsync("/cobros", new { FacturaId = f2.Id, Importe = 300m, Metodo = "Transferencia" })).EnsureSuccessStatusCode();
        var s2 = (await completa.GetFromJsonAsync<SaldosResp>("/cuentas-bancarias/saldos"))!;
        s2.Fuente.Should().Be("Contabilidad");
        s2.Cuentas.Single(x => x.Id == b2.Id).SaldoContable.Should().Be(300m);
        s2.Total.Should().Be(300m);
    }
}
