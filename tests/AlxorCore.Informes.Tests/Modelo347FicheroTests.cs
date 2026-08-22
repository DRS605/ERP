using System.Text;
using AlxorCore.Informes.Aplicacion;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Informes.Tests;

public class Modelo347FicheroTests
{
    private static Modelo347LineaDto Linea(string nombre, string nif, string sentido, string clave, decimal anual,
        decimal t1 = 0m, decimal t2 = 0m, decimal t3 = 0m, decimal t4 = 0m) =>
        new("k:" + nif, nombre, nif, sentido, clave, anual, t1, t2, t3, t4);

    [Fact]
    public void El_fichero_347_respeta_el_diseno_de_registro()
    {
        var declarante = new DeclaranteAeat("B12345674", "Mi Empresa SL");
        var lineas = new List<Modelo347LineaDto>
        {
            Linea("Cliente Uno SL", "A11111111", "Cliente", "B", 5000m, t1: 5000m),
            Linea("Proveedor Dos SL", "B22222222", "Proveedor", "A", 4000m, t2: 4000m),
        };

        var bytes = FicheroModelo347.Generar(declarante, 2026, lineas);
        var lineasTxt = Encoding.Latin1.GetString(bytes).Split("\r\n");

        lineasTxt.Should().HaveCount(3); // declarante + 2 declarados
        lineasTxt.Should().OnlyContain(l => l.Length == 500);

        // Cabecera.
        var cab = lineasTxt[0];
        cab[..8].Should().Be("13472026");                 // tipo 1 + modelo 347 + ejercicio
        cab.Substring(8, 9).Should().Be("B12345674");     // NIF declarante
        cab[57].Should().Be('T');                          // tipo de soporte
        cab.Substring(135, 9).Should().Be("000000002");   // nº declarados (136-144)
        cab.Substring(145, 15).Should().Be("000000000900000"); // total 9000,00 (146-160)

        // Declarado 1 (cliente, clave B).
        var d1 = lineasTxt[1];
        d1[..8].Should().Be("23472026");
        d1.Substring(17, 9).Should().Be("A11111111");     // NIF declarado (18-26)
        d1.Substring(35, 40).Trim().Should().Be("CLIENTE UNO SL"); // nombre (36-75)
        d1.Substring(83, 15).Should().Be("000000000500000"); // importe anual 5000,00 (84-98)
        d1[98].Should().Be('B');                           // clave operación (99)
        d1.Substring(100, 15).Should().Be("000000000500000"); // 1T (101-115)

        // Declarado 2 (proveedor, clave A) con importe en 2T.
        var d2 = lineasTxt[2];
        d2[98].Should().Be('A');
        d2.Substring(116, 15).Should().Be("000000000400000"); // 2T (117-131)
    }
}
