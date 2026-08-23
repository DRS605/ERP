using AlxorCore.Identidad.Dominio;
using AlxorCore.Identidad.Dominio.Eventos;
using AlxorCore.Identidad.Tests.Dobles;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Identidad.Tests.Dominio;

public class UsuarioDobleFactorTests
{
    private static readonly RelojFijo Reloj = RelojFijo.Predeterminado();

    private static Usuario NuevoUsuario()
    {
        var hash = HashContrasena.DesdeHash("hash-x");
        return Usuario.Registrar(Email.Crear("ana@ejemplo.com").Valor, "Ana", hash, Reloj).Valor;
    }

    [Fact]
    public void Preparar_guarda_el_secreto_sin_activar()
    {
        var u = NuevoUsuario();
        var secreto = u.PrepararDobleFactor(Reloj);

        secreto.Should().NotBeNullOrEmpty();
        u.DobleFactorActivo.Should().BeFalse();
        u.DobleFactorSecreto.Should().Be(secreto);
    }

    [Fact]
    public void Activar_con_codigo_valido_activa_y_entrega_codigos_de_recuperacion()
    {
        var u = NuevoUsuario();
        var secreto = u.PrepararDobleFactor(Reloj);
        var codigo = Totp.Calcular(secreto, Reloj.AhoraUtc);

        var r = u.ActivarDobleFactor(codigo, Reloj);

        r.EsCorrecto.Should().BeTrue();
        r.Valor.Should().HaveCount(Usuario.NumeroCodigosRecuperacion);
        u.DobleFactorActivo.Should().BeTrue();
        u.CodigosRecuperacionPendientes.Should().Be(Usuario.NumeroCodigosRecuperacion);
        u.EventosDominio.OfType<DobleFactorActivado>().Should().ContainSingle();
    }

    [Fact]
    public void Activar_con_codigo_invalido_falla_y_no_activa()
    {
        var u = NuevoUsuario();
        u.PrepararDobleFactor(Reloj);
        u.ActivarDobleFactor("000000", Reloj).EsFallo.Should().BeTrue();
        u.DobleFactorActivo.Should().BeFalse();
    }

    [Fact]
    public void El_segundo_factor_acepta_totp_y_consume_un_codigo_de_recuperacion()
    {
        var u = NuevoUsuario();
        var secreto = u.PrepararDobleFactor(Reloj);
        var codigos = u.ActivarDobleFactor(Totp.Calcular(secreto, Reloj.AhoraUtc), Reloj).Valor;

        // TOTP válido.
        u.VerificarSegundoFactor(Totp.Calcular(secreto, Reloj.AhoraUtc), Reloj).Should().BeTrue();

        // Código de recuperación: válido una vez y luego consumido.
        var recuperacion = codigos[0];
        u.VerificarSegundoFactor(recuperacion, Reloj).Should().BeTrue();
        u.CodigosRecuperacionPendientes.Should().Be(Usuario.NumeroCodigosRecuperacion - 1);
        u.VerificarSegundoFactor(recuperacion, Reloj).Should().BeFalse(); // ya usado
    }

    [Fact]
    public void Desactivar_borra_secreto_y_codigos_y_emite_evento()
    {
        var u = NuevoUsuario();
        var secreto = u.PrepararDobleFactor(Reloj);
        u.ActivarDobleFactor(Totp.Calcular(secreto, Reloj.AhoraUtc), Reloj);

        u.DesactivarDobleFactor(Reloj);

        u.DobleFactorActivo.Should().BeFalse();
        u.DobleFactorSecreto.Should().BeNull();
        u.CodigosRecuperacionPendientes.Should().Be(0);
        u.EventosDominio.OfType<DobleFactorDesactivado>().Should().ContainSingle();
    }
}
