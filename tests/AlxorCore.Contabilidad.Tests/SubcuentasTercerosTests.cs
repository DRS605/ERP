using AlxorCore.Contabilidad.Aplicacion;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Contabilidad.Tests;

public sealed class SubcuentasTercerosTests
{
    [Fact]
    public void La_raiz_depende_del_tipo_de_tercero()
    {
        SubcuentasTerceros.Raiz(TipoTerceroContable.Cliente).Should().Be("430");
        SubcuentasTerceros.Raiz(TipoTerceroContable.Proveedor).Should().Be("400");
        SubcuentasTerceros.Raiz(TipoTerceroContable.Trabajador).Should().Be("465");
    }

    [Fact]
    public void El_primer_codigo_rellena_la_secuencia_hasta_la_longitud()
    {
        SubcuentasTerceros.SiguienteCodigo("430", 8, Array.Empty<string>()).Should().Be("43000001");
        SubcuentasTerceros.SiguienteCodigo("400", 7, Array.Empty<string>()).Should().Be("4000001");
        SubcuentasTerceros.SiguienteCodigo("465", 8, Array.Empty<string>()).Should().Be("46500001");
    }

    [Fact]
    public void El_siguiente_codigo_toma_el_maximo_existente_de_esa_raiz_y_longitud()
    {
        var existentes = new[] { "430", "43000001", "43000007", "400", "40000003", "472", "705" };
        SubcuentasTerceros.SiguienteCodigo("430", 8, existentes).Should().Be("43000008");
        SubcuentasTerceros.SiguienteCodigo("400", 8, existentes).Should().Be("40000004");
    }

    [Fact]
    public void Si_la_longitud_no_deja_sitio_para_secuencia_devuelve_la_raiz()
    {
        SubcuentasTerceros.SiguienteCodigo("430", 3, Array.Empty<string>()).Should().Be("430");
        SubcuentasTerceros.SiguienteCodigo("430", 2, Array.Empty<string>()).Should().Be("430");
    }

    [Fact]
    public void Ignora_codigos_de_otra_longitud_al_calcular_el_siguiente()
    {
        // "4300001" (7) no cuenta para longitud 8; el siguiente sigue siendo el primero.
        SubcuentasTerceros.SiguienteCodigo("430", 8, new[] { "4300001", "4300009" }).Should().Be("43000001");
    }
}
