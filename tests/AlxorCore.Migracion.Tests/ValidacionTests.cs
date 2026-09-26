using AlxorCore.Migracion.Hispatec;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Migracion.Tests;

public class ValidacionTests
{
    private static (InformeMigracion Informe, DatosHispatec Datos) Validar(Dictionary<string, string> archivos) =>
        ValidadorHispatec.Validar(Paquete.Leer(PaqueteEjemplo.Zip(archivos)));

    [Fact]
    public void Lee_csv_con_comillas_y_punto_y_coma()
    {
        var t = TablaCsv.Parsear("x.csv", "a;b\n\"uno; dos\";\"con \"\"comillas\"\"\"\r\n;\n3;4");
        t.Filas.Should().HaveCount(2, "la fila vacía se ignora");
        t.Filas[0].Texto("a").Should().Be("uno; dos");
        t.Filas[0].Texto("b").Should().Be("con \"comillas\"");
        t.Filas[1].Numero.Should().Be(4);
    }

    [Fact]
    public void Detecta_los_problemas_de_integridad_de_hispatec()
    {
        var (informe, datos) = Validar(PaqueteEjemplo.Archivos());
        var codigos = informe.Incidencias.Select(i => i.Codigo).ToList();

        codigos.Should().Contain(["cliente.huerfano", "cartera.huerfana", "parcela.sin_agricultor"], "son errores: esas filas no se cargan");
        codigos.Should().Contain(["tercero.nif_repetido", "tercero.nif_invalido", "saldo.acumulador_descuadrado", "familia.superior_huerfana", "articulo.codigo_repetido"]);
        informe.Incidencias.Single(i => i.Codigo == "saldo.acumulador_descuadrado").Mensaje.Should().Contain("4300001").And.Contain("100,00");
        informe.PuedeCargar.Should().BeTrue("los errores de fila no impiden cargar lo válido");
        informe.Cuadres.CuentasDescuadradasConAcumuladores.Should().Be(1);
        informe.Cuadres.CarteraCobros.Should().Be(1500m).And.Be(informe.Cuadres.SaldoClientes);
        informe.Cuadres.CarteraPagos.Should().Be(800m).And.Be(informe.Cuadres.SaldoProveedores);

        datos.Clientes.Should().HaveCount(3);
        datos.Articulos.Should().HaveCount(2);
        datos.Cartera.Should().HaveCount(3);
        datos.Parcelas.Should().ContainSingle().Which.Sigpac.Should().Be("46:190:0:0:12:45:1");
        datos.Familias.Single(f => f.Id == "12").IdSuperior.Should().BeNull();
    }

    [Fact]
    public void Unos_saldos_descuadrados_o_un_paquete_incompleto_no_se_cargan()
    {
        var archivos = PaqueteEjemplo.Archivos();
        archivos["saldos.csv"] = "cuenta;debe;haber\n5720001;100;0\n";
        Validar(archivos).Informe.PuedeCargar.Should().BeFalse();

        var incompleto = PaqueteEjemplo.Archivos();
        incompleto.Remove("clientes.csv");
        incompleto["manifiesto.csv"] = "clave;valor\nformato;otro\nversion;9\n";
        var informe = Validar(incompleto).Informe;
        informe.PuedeCargar.Should().BeFalse();
        informe.Incidencias.Select(i => i.Mensaje).Should().Contain(m => m.Contains("clientes.csv")).And.Contain(m => m.Contains("fecha_corte"));
    }

    [Fact]
    public void Si_la_cartera_no_cuadra_con_la_contabilidad_avisa()
    {
        var archivos = PaqueteEjemplo.Archivos();
        archivos["cartera.csv"] = "id;sentido;id_tercero;documento;fecha_documento;vencimiento;importe\nDC1;cobro;C1;F1;2026-11-15;2027-01-15;900.00\n";
        Validar(archivos).Informe.Incidencias.Should().Contain(i => i.Codigo == "cartera.descuadre_clientes");
    }
}
