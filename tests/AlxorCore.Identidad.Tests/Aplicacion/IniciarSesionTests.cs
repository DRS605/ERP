using AlxorCore.Identidad.Aplicacion.CasosDeUso;
using AlxorCore.Identidad.Tests.Dobles;
using AlxorCore.Nucleo.Resultados;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Identidad.Tests.Aplicacion;

public class IniciarSesionTests
{
    private readonly FakeRepositorioUsuarios _usuarios = new();
    private readonly FakeHasherContrasena _hasher = new();
    private readonly FakeProveedorTokens _tokens = new();
    private readonly RelojFijo _reloj = RelojFijo.Predeterminado();

    private async Task RegistrarAsync(string email, string contrasena)
    {
        var registro = new RegistrarUsuario(_usuarios, _hasher, new FakeServicioVerificacionEmail(), new FakeUnidadDeTrabajo(), _reloj);
        await registro.EjecutarAsync(new RegistrarUsuarioComando(email, "Ana", contrasena));
    }

    [Fact]
    public async Task Login_correcto_devuelve_token_y_perfil()
    {
        await RegistrarAsync("ana@ejemplo.com", "contrasena123");
        var caso = new IniciarSesion(_usuarios, _hasher, _tokens, new FakeUnidadDeTrabajo(), _reloj);

        var resultado = await caso.EjecutarAsync(new IniciarSesionComando("ana@ejemplo.com", "contrasena123"));

        resultado.EsCorrecto.Should().BeTrue();
        resultado.Valor.Token.Should().StartWith("token-");
        resultado.Valor.Usuario.Email.Should().Be("ana@ejemplo.com");
    }

    [Fact]
    public async Task Login_con_contrasena_incorrecta_devuelve_no_autenticado()
    {
        await RegistrarAsync("ana@ejemplo.com", "contrasena123");
        var caso = new IniciarSesion(_usuarios, _hasher, _tokens, new FakeUnidadDeTrabajo(), _reloj);

        var resultado = await caso.EjecutarAsync(new IniciarSesionComando("ana@ejemplo.com", "incorrecta"));

        resultado.EsFallo.Should().BeTrue();
        resultado.Error.Tipo.Should().Be(TipoError.NoAutenticado);
        resultado.Error.Codigo.Should().Be("auth.credenciales_invalidas");
    }

    [Fact]
    public async Task Login_con_email_inexistente_devuelve_mismo_error_generico()
    {
        var caso = new IniciarSesion(_usuarios, _hasher, _tokens, new FakeUnidadDeTrabajo(), _reloj);

        var resultado = await caso.EjecutarAsync(new IniciarSesionComando("noexiste@ejemplo.com", "contrasena123"));

        resultado.EsFallo.Should().BeTrue();
        resultado.Error.Codigo.Should().Be("auth.credenciales_invalidas");
    }

    [Fact]
    public async Task Login_con_2fa_activo_pide_codigo_y_lo_acepta()
    {
        await RegistrarAsync("ana@ejemplo.com", "contrasena123");
        var usuario = _usuarios.Usuarios[0];
        var secreto = usuario.PrepararDobleFactor(_reloj);
        usuario.ActivarDobleFactor(AlxorCore.Identidad.Dominio.Totp.Calcular(secreto, _reloj.AhoraUtc), _reloj);
        var caso = new IniciarSesion(_usuarios, _hasher, _tokens, new FakeUnidadDeTrabajo(), _reloj);

        // Sin código: se pide el segundo factor y no se emite token.
        var sinCodigo = await caso.EjecutarAsync(new IniciarSesionComando("ana@ejemplo.com", "contrasena123"));
        sinCodigo.EsCorrecto.Should().BeTrue();
        sinCodigo.Valor.Requiere2fa.Should().BeTrue();
        sinCodigo.Valor.Token.Should().BeEmpty();

        // Con código válido: token emitido.
        var codigo = AlxorCore.Identidad.Dominio.Totp.Calcular(secreto, _reloj.AhoraUtc);
        var conCodigo = await caso.EjecutarAsync(new IniciarSesionComando("ana@ejemplo.com", "contrasena123", codigo));
        conCodigo.EsCorrecto.Should().BeTrue();
        conCodigo.Valor.Requiere2fa.Should().BeFalse();
        conCodigo.Valor.Token.Should().StartWith("token-");

        // Con código incorrecto: no autenticado.
        var malo = await caso.EjecutarAsync(new IniciarSesionComando("ana@ejemplo.com", "contrasena123", "000000"));
        malo.EsFallo.Should().BeTrue();
        malo.Error.Codigo.Should().Be("auth.2fa_invalido");
    }

    [Fact]
    public async Task Login_de_cuenta_suspendida_esta_prohibido()
    {
        await RegistrarAsync("ana@ejemplo.com", "contrasena123");
        _usuarios.Usuarios[0].Suspender(_reloj);
        var caso = new IniciarSesion(_usuarios, _hasher, _tokens, new FakeUnidadDeTrabajo(), _reloj);

        var resultado = await caso.EjecutarAsync(new IniciarSesionComando("ana@ejemplo.com", "contrasena123"));

        resultado.EsFallo.Should().BeTrue();
        resultado.Error.Tipo.Should().Be(TipoError.Prohibido);
    }
}
