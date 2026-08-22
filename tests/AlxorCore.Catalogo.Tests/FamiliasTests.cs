using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Catalogo.Dominio;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Catalogo.Tests;

/// <summary>Pruebas del dominio de familias (árbol de subfamilias) y de las utilidades del árbol.</summary>
public class FamiliaDominioTests
{
    private static readonly RelojFijo Reloj = new();

    [Fact]
    public void Crear_familia_raiz_valida()
    {
        var f = Familia.Crear(Guid.NewGuid(), "  Alimentación  ", padreId: null, codigo: " AL ", Reloj);
        f.EsCorrecto.Should().BeTrue();
        f.Valor.Nombre.Should().Be("Alimentación"); // recortado
        f.Valor.Codigo.Should().Be("AL");
        f.Valor.PadreId.Should().BeNull();
        f.Valor.Activo.Should().BeTrue();
    }

    [Fact]
    public void Crear_subfamilia_con_padre()
    {
        var padreId = Guid.NewGuid();
        var f = Familia.Crear(Guid.NewGuid(), "Bebidas", padreId, codigo: null, Reloj);
        f.EsCorrecto.Should().BeTrue();
        f.Valor.PadreId.Should().Be(padreId);
    }

    [Fact]
    public void Nombre_vacio_falla()
    {
        Familia.Crear(Guid.NewGuid(), "   ", null, null, Reloj).EsFallo.Should().BeTrue();
    }

    [Fact]
    public void No_puede_ser_su_propio_padre()
    {
        var f = Familia.Crear(Guid.NewGuid(), "X", null, null, Reloj).Valor;
        f.Reubicar(f.Id, Reloj).EsFallo.Should().BeTrue();
    }

    [Fact]
    public void Reubicar_bajo_otra_familia_cambia_el_padre()
    {
        var f = Familia.Crear(Guid.NewGuid(), "X", null, null, Reloj).Valor;
        var nuevoPadre = Guid.NewGuid();
        f.Reubicar(nuevoPadre, Reloj).EsCorrecto.Should().BeTrue();
        f.PadreId.Should().Be(nuevoPadre);

        f.Reubicar(null, Reloj); // volver a raíz
        f.PadreId.Should().BeNull();
    }

    [Fact]
    public void Desactivar_y_activar()
    {
        var f = Familia.Crear(Guid.NewGuid(), "X", null, null, Reloj).Valor;
        f.Desactivar(Reloj);
        f.Activo.Should().BeFalse();
        f.Activar(Reloj);
        f.Activo.Should().BeTrue();
    }
}

/// <summary>Pruebas de las utilidades de árbol (ruta completa, nivel y detección de ancestros).</summary>
public class ArbolFamiliasTests
{
    private static readonly RelojFijo Reloj = new();
    private static readonly Guid Empresa = Guid.NewGuid();

    private static Familia Nueva(string nombre, Guid? padre) =>
        Familia.Crear(Empresa, nombre, padre, null, Reloj).Valor;

    [Fact]
    public void Ruta_completa_y_nivel_de_una_familia_anidada()
    {
        var raiz = Nueva("Alimentación", null);
        var media = Nueva("Bebidas", raiz.Id);
        var hoja = Nueva("Refrescos", media.Id);
        var porId = new Dictionary<Guid, Familia> { [raiz.Id] = raiz, [media.Id] = media, [hoja.Id] = hoja };

        var (ruta, nivel) = ArbolFamilias.RutaYNivel(hoja, porId);
        ruta.Should().Be("Alimentación > Bebidas > Refrescos");
        nivel.Should().Be(2);

        ArbolFamilias.RutaYNivel(raiz, porId).Nivel.Should().Be(0);
    }

    [Fact]
    public void Detecta_ancestro_para_evitar_ciclos()
    {
        var raiz = Nueva("A", null);
        var media = Nueva("B", raiz.Id);
        var hoja = Nueva("C", media.Id);
        var porId = new Dictionary<Guid, Familia> { [raiz.Id] = raiz, [media.Id] = media, [hoja.Id] = hoja };

        // raiz es ancestro de hoja → mover raiz bajo hoja crearía un ciclo.
        ArbolFamilias.EsAncestro(raiz.Id, hoja.Id, porId).Should().BeTrue();
        // hoja NO es ancestro de raiz.
        ArbolFamilias.EsAncestro(hoja.Id, raiz.Id, porId).Should().BeFalse();
    }
}
