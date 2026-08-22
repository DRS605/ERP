using AlxorCore.Organizacion.Dominio;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Organizacion.Tests.Dominio;

public sealed class FormaPagoTests
{
    private static readonly Guid Empresa = Guid.NewGuid();

    [Fact]
    public void Crea_una_forma_al_contado_pagada()
    {
        var forma = FormaPago.Crear(Empresa, "Contado (pagado)", generaVencimiento: false, diasVencimiento: 0, registrarPagoAutomatico: true);

        forma.EsCorrecto.Should().BeTrue();
        forma.Valor.GeneraVencimiento.Should().BeFalse();
        forma.Valor.RegistrarPagoAutomatico.Should().BeTrue();
        forma.Valor.Activo.Should().BeTrue();
    }

    [Fact]
    public void Sin_vencimiento_fuerza_dias_a_cero()
    {
        var forma = FormaPago.Crear(Empresa, "Contado", generaVencimiento: false, diasVencimiento: 30, registrarPagoAutomatico: false);

        forma.EsCorrecto.Should().BeTrue();
        forma.Valor.DiasVencimiento.Should().Be(0);
    }

    [Fact]
    public void El_nombre_es_obligatorio()
    {
        FormaPago.Crear(Empresa, "  ", generaVencimiento: true, diasVencimiento: 30, registrarPagoAutomatico: false)
            .EsFallo.Should().BeTrue();
    }

    [Fact]
    public void Los_dias_negativos_no_son_validos()
    {
        FormaPago.Crear(Empresa, "Aplazada", generaVencimiento: true, diasVencimiento: -5, registrarPagoAutomatico: false)
            .EsFallo.Should().BeTrue();
    }

    [Fact]
    public void Actualiza_una_forma_existente()
    {
        var forma = FormaPago.Crear(Empresa, "Transferencia", generaVencimiento: true, diasVencimiento: 30, registrarPagoAutomatico: false).Valor;

        var r = forma.Actualizar("Transferencia 60 días", generaVencimiento: true, diasVencimiento: 60, registrarPagoAutomatico: false);

        r.EsCorrecto.Should().BeTrue();
        forma.Nombre.Should().Be("Transferencia 60 días");
        forma.DiasVencimiento.Should().Be(60);
    }

    [Fact]
    public void Desactivar_marca_inactiva()
    {
        var forma = FormaPago.Crear(Empresa, "Contado", generaVencimiento: false, diasVencimiento: 0, registrarPagoAutomatico: true).Valor;
        forma.Desactivar();
        forma.Activo.Should().BeFalse();
    }
}
