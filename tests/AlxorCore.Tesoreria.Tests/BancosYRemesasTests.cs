using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Tesoreria.Aplicacion;
using AlxorCore.Tesoreria.Dominio;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Tesoreria.Tests;

public class BancosYRemesasTests
{
    private static readonly IReloj Reloj = new RelojFijo();
    private static readonly DateOnly Hoy = new(2026, 1, 1);

    [Theory]
    [InlineData("ES9121000418450200051332", true)]
    [InlineData("es91 2100 0418 4502 0005 1332", true)]
    [InlineData("ES9121000418450200051333", false)]
    [InlineData("ES91210004184502000513", false)]
    [InlineData("DE89370400440532013000", true)]
    [InlineData("", false)]
    public void Valida_el_iban_con_sus_digitos_de_control(string iban, bool valido) => ValidadorIban.EsValido(iban).Should().Be(valido);

    [Fact]
    public void Un_banco_necesita_iban_y_subcuenta_572_y_una_caja_570()
    {
        CuentaBancaria.Crear(Guid.NewGuid(), TipoCuentaTesoreria.Banco, "BBVA", null, null, "5720001", 0m, null, Reloj).EsFallo.Should().BeTrue();
        CuentaBancaria.Crear(Guid.NewGuid(), TipoCuentaTesoreria.Banco, "BBVA", "ES9121000418450200051332", null, "5700001", 0m, null, Reloj).EsFallo.Should().BeTrue();
        CuentaBancaria.Crear(Guid.NewGuid(), TipoCuentaTesoreria.Caja, "Caja", null, null, "5700001", 0m, null, Reloj).EsCorrecto.Should().BeTrue();
        var b = CuentaBancaria.Crear(Guid.NewGuid(), TipoCuentaTesoreria.Banco, "BBVA", "ES9121000418450200051332", "bbvaesmm", "5720001", 0m, null, Reloj);
        b.Valor.Bic.Should().Be("BBVAESMM");
        b.Valor.CambiarPredeterminada(true).EsCorrecto.Should().BeTrue();
        b.Valor.Actualizar("BBVA", b.Valor.Iban, null, activa: false, 0m, null);
        b.Valor.Predeterminada.Should().BeFalse("una cuenta inactiva deja de ser la predeterminada");
    }

    [Fact]
    public void La_remesa_avanza_de_generada_a_presentada_y_liquidada_y_libera_sus_lineas()
    {
        var r = Remesa.Crear(Guid.NewGuid(), TipoRemesa.Cobro, 1, Hoy, Hoy.AddDays(3), null, "CORE", "OOFF", Reloj).Valor;
        var factura = Guid.NewGuid();
        r.AgregarLinea(TipoDocumentoTesoreria.Factura, factura, "F-1", "Cliente", "ES7620770024003102575766", "M1", Hoy, 50m).EsCorrecto.Should().BeTrue();
        r.AgregarLinea(TipoDocumentoTesoreria.Factura, factura, "F-1", "Cliente", null, null, null, 50m).EsFallo.Should().BeTrue("no se repite un documento");
        r.Lineas.Single().Viva.Should().BeTrue();

        r.Presentar(Reloj).EsCorrecto.Should().BeTrue();
        r.Presentar(Reloj).EsFallo.Should().BeTrue();
        r.PuedeLiquidarse(Hoy.AddDays(-1)).EsFallo.Should().BeTrue();
        r.Liquidar(Hoy, new Dictionary<Guid, Guid> { [r.Lineas[0].Id] = Guid.NewGuid() });
        r.Estado.Should().Be(EstadoRemesa.Liquidada);
        Remesa.Descripcion(r.Tipo, r.Estado).Should().Be("Cobrada");
        r.Lineas.Single().Viva.Should().BeFalse();
        r.Anular(Reloj).EsFallo.Should().BeTrue("una remesa cobrada no se anula: se registran devoluciones");
    }

    [Fact]
    public void El_apunte_solo_se_concilia_si_la_suma_cuadra_y_se_puede_deshacer()
    {
        var extracto = ExtractoImportado.Crear(Guid.NewGuid(), Guid.NewGuid(), "210004180200051332", Hoy, Hoy, 0m, 25m, "x.n43", "h", 1, Reloj);
        var a = ApunteBancario.Crear(extracto, 1, Hoy, null, 25m, "TRANSFERENCIA", "04", null, null, null);
        a.Conciliar([(Guid.NewGuid(), TipoDocumentoTesoreria.Factura, Guid.NewGuid(), 10m, true)], automatico: false, Reloj).EsFallo.Should().BeTrue();
        a.Conciliar([(Guid.NewGuid(), TipoDocumentoTesoreria.Factura, Guid.NewGuid(), 10m, true), (Guid.NewGuid(), TipoDocumentoTesoreria.Factura, Guid.NewGuid(), 15m, true)],
            automatico: false, Reloj).EsCorrecto.Should().BeTrue();
        a.Estado.Should().Be(EstadoApunte.ConciliadoManual);
        a.ContabilizarDirecto("626", null, Guid.NewGuid(), Reloj).EsFallo.Should().BeTrue("ya está conciliado");
        a.Deshacer().EsCorrecto.Should().BeTrue();
        a.Casaciones.Should().BeEmpty();
        a.ContabilizarDirecto("5720001", null, Guid.NewGuid(), Reloj).EsFallo.Should().BeTrue("la contrapartida no es otra cuenta de tesorería");
        a.ContabilizarDirecto("769", "Intereses", Guid.NewGuid(), Reloj).EsCorrecto.Should().BeTrue();
        a.Estado.Should().Be(EstadoApunte.ConAsiento);
    }

    [Fact]
    public void La_cuenta_del_fichero_norma43_casa_con_el_iban()
    {
        ConciliacionBancaria.CasaCuenta("ES9121000418450200051332", "210004180200051332").Should().BeTrue();
        ConciliacionBancaria.CasaCuenta("ES9121000418450200051332", "210004189999999999").Should().BeFalse();
    }

    [Fact]
    public void Las_devoluciones_exigen_un_motivo_sepa()
    {
        var cobro = Movimiento.Crear(Guid.NewGuid(), TipoDocumentoTesoreria.Factura, Guid.NewGuid(), SentidoMovimiento.Cobro, 80m, Hoy, "Domiciliación remesa 2026/1", Reloj).Valor;
        var anulacion = Movimiento.CrearAnulacion(cobro, Hoy, Reloj, "Devolución AM04").Valor;
        anulacion.Importe.Should().Be(-80m);
        anulacion.Metodo.Should().Be("Devolución AM04");
        DevolucionRecibo.Crear(cobro, anulacion, null, Hoy, "ZZ01", 0m, false, null, null, Reloj).EsFallo.Should().BeTrue();
        var d = DevolucionRecibo.Crear(cobro, anulacion, null, Hoy, "am04", 3.5m, true, null, null, Reloj).Valor;
        d.Motivo.Should().Be("AM04");
        MotivosDevolucionSepa.Describir(d.Motivo).Should().Be("Fondos insuficientes");
        GestionDevoluciones.EsDomiciliado(cobro, enRemesa: false).Should().BeTrue();
    }
}
