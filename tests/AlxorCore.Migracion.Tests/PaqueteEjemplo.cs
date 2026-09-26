using System.IO.Compression;
using System.Text;

namespace AlxorCore.Migracion.Tests;

/// <summary>Paquete sintético con la forma que genera exportar.ps1 y los problemas típicos de Hispatec.</summary>
public static class PaqueteEjemplo
{
    public static Dictionary<string, string> Archivos(int anio = 2026) => new()
    {
        ["manifiesto.csv"] = $"clave;valor\nformato;alxor-hispatec\nversion;1\nempresa_codigo;01\nempresa_nombre;Cítricos del Turia SAT\nfecha_corte;{anio}-12-31\n",
        ["terceros.csv"] = "id_sujeto;nif;nombre;calle;codigo_postal;poblacion;provincia;pais;email;iban\n"
            + "1;B12345674;Frutas del Norte SA;Calle Mayor 1;46001;Valencia;Valencia;ES;compras@norte.es;\n"
            + "2;B12345674;\"Frutas del Norte, SA (duplicado)\";;;;;ES;;\n"
            + "3;12345678Z;Juan Labrador Ferrer;Camí Vell 3;46200;Paiporta;Valencia;ES;;ES9121000418450200051332\n"
            + "4;A58818501;Transportes Rápidos SL;;;;;ES;;\n"
            + "5;X1234;Cliente con NIF raro;;;;;ES;;\n",
        ["clientes.csv"] = "id;codigo;id_sujeto;subcuenta\nC1;CLI001;1;4300001\nC2;CLI002;2;\nC5;CLI005;5;\nC9;CLI009;99;\n",
        ["proveedores.csv"] = "id;codigo;id_sujeto;subcuenta;agricultor;autoriza_autofactura;retencion\nP1;SOC001;3;4000001;1;1;2\nA1;ACR001;4;4100001;0;0;\n",
        ["familias.csv"] = "id;codigo;nombre;id_superior\n10;FRU;Fruta;\n11;CIT;Cítricos;10\n12;RAR;Huérfana;999\n",
        ["articulos.csv"] = "id;codigo;nombre;id_familia;unidad;iva;tipo;controla_stock;precio_venta;precio_compra\n"
            + "100;NAR;Naranja Navel;11;kg;4;bien;1;0.85;0\n"
            + "200;TRANS;Transporte;;ud;21;servicio;0;25;0\n"
            + "101;NAR;Naranja repetida;11;kg;4;bien;1;0;0\n",
        ["cuentas.csv"] = "codigo;nombre\n1000000;Capital social\n4300001;Frutas del Norte SA\n4000001;Juan Labrador Ferrer\n5720001;Banco\n",
        ["saldos.csv"] = "cuenta;debe;haber\n5720001;10000.00;0\n4300001;1500.00;0\n4000001;0;800.00\n1000000;0;10700.00\n",
        ["acumuladores.csv"] = "cuenta;debe;haber\n5720001;10000.00;0\n4300001;1400.00;0\n4000001;0;800.00\n1000000;0;10700.00\n",
        ["cartera.csv"] = "id;sentido;id_tercero;documento;fecha_documento;vencimiento;importe\n"
            + $"DC1;cobro;C1;F-{anio}-120 / R-1 (plazo 1);{anio}-11-15;{anio + 1}-01-15;1000.00\n"
            + $"DC2;cobro;C1;F-{anio}-121 / R-2 (plazo 1);{anio}-12-01;{anio + 1}-02-01;500.00\n"
            + $"DC3;cobro;C9;F-{anio}-099 / R-3 (plazo 1);{anio}-10-01;{anio}-11-01;50.00\n"
            + $"DP1;pago;P1;LIQ-44 / P-1 (plazo 1);{anio}-12-20;{anio + 1}-01-20;800.00\n",
        ["campanas.csv"] = $"id;codigo;nombre;desde;hasta\n1;{anio}/{anio - 1999:00};Campaña {anio};{anio}-09-01;{anio + 1}-08-31\n",
        ["parcelas.csv"] = "id;codigo;nombre;id_proveedor;sigpac;superficie_ha;id_articulo;variedad\n"
            + "500;HR-500;Huerto del Río · Navel;P1;46:190:0:0:12:45:1;2.5;100;Navelina\n"
            + "501;TR-501;Nave de transportes;A1;;1;;\n",
    };

    public static byte[] Zip(Dictionary<string, string> archivos)
    {
        ArgumentNullException.ThrowIfNull(archivos);
        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (nombre, contenido) in archivos)
            {
                using var w = new StreamWriter(zip.CreateEntry(nombre).Open(), new UTF8Encoding(false));
                w.Write(contenido);
            }
        }

        return ms.ToArray();
    }
}
