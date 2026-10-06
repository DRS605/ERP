using System.Security.Cryptography;
using System.Text;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Extensiones.Dominio;

/// <summary>A quién da acceso un enlace del portal.</summary>
public enum TipoPortal
{
    /// <summary>Un agricultor: sus entregas y sus liquidaciones.</summary>
    Agricultor = 1,

    /// <summary>Un cliente: sus albaranes, sus facturas y lo que tiene pendiente.</summary>
    Cliente = 2,

    /// <summary>
    /// Un terminal de la planta (tableta o lector en una línea): registra volcados de palots. No es de ningún tercero: su
    /// «tercero» es el propio acceso.
    /// </summary>
    TerminalPlanta = 3,
}

/// <summary>
/// Acceso al portal de un agricultor o un cliente por un enlace con una clave secreta (solo se guarda su huella
/// SHA-256). Se puede revocar, regenerar (la clave anterior deja de valer) y poner caducidad; se anotan los accesos.
/// </summary>
public sealed class AccesoPortal : RaizAgregadoEmpresa<Guid>
{
    private AccesoPortal(Guid id)
        : base(id, Guid.Empty)
    {
        Nombre = null!;
        Huella = null!;
    }

    private AccesoPortal(Guid id, Guid empresaId, TipoPortal tipo, Guid terceroId, string nombre, DateOnly? caduca, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Tipo = tipo;
        TerceroId = terceroId;
        Nombre = nombre;
        Caduca = caduca;
        CreadoEn = ahora;
        Huella = string.Empty;
    }

    public TipoPortal Tipo { get; private set; }

    /// <summary>El agricultor o el cliente.</summary>
    public Guid TerceroId { get; private set; }

    public string Nombre { get; private set; }

    public string Huella { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public DateOnly? Caduca { get; private set; }

    public DateTimeOffset? RevocadoEn { get; private set; }

    public DateTimeOffset? UltimoAcceso { get; private set; }

    public int Accesos { get; private set; }

    public bool VigenteEl(DateOnly dia) => RevocadoEn is null && (Caduca is null || Caduca >= dia);

    /// <summary>Crea el acceso y devuelve la clave secreta (solo se ve ahora).</summary>
    public static Resultado<(AccesoPortal Acceso, string Secreto)> Crear(Guid empresaId, TipoPortal tipo, Guid terceroId, string? nombre, DateOnly? caduca, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (!Enum.IsDefined(tipo))
        {
            return Resultado.Fallo<(AccesoPortal, string)>(Error.Validacion("portal.tipo", "El acceso es de un agricultor, de un cliente o de un terminal de la planta."));
        }

        if (terceroId == Guid.Empty || string.IsNullOrWhiteSpace(nombre))
        {
            return Resultado.Fallo<(AccesoPortal, string)>(Error.Validacion("portal.tercero", "Indica el agricultor o el cliente."));
        }

        if (caduca is { } c && c < DateOnly.FromDateTime(reloj.AhoraUtc.UtcDateTime))
        {
            return Resultado.Fallo<(AccesoPortal, string)>(Error.Validacion("portal.caducidad", "La caducidad no puede ser pasada."));
        }

        var a = new AccesoPortal(Guid.NewGuid(), empresaId, tipo, terceroId, nombre.Trim().Length > 200 ? nombre.Trim()[..200] : nombre.Trim(), caduca, reloj.AhoraUtc);
        return Resultado.Ok((a, a.NuevoSecreto()));
    }

    /// <summary>Nueva clave: la anterior deja de valer.</summary>
    public Resultado<string> Regenerar()
    {
        if (RevocadoEn is not null)
        {
            return Resultado.Fallo<string>(Error.Conflicto("portal.revocado", "El acceso está revocado: crea otro."));
        }

        return Resultado.Ok(NuevoSecreto());
    }

    public Resultado Revocar(IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (RevocadoEn is not null)
        {
            return Resultado.Fallo(Error.Conflicto("portal.revocado", "El acceso ya está revocado."));
        }

        RevocadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    /// <summary>¿La clave es la de este acceso y sigue vigente? (comparación en tiempo constante).</summary>
    public bool Valida(string secreto, DateOnly hoy) =>
        VigenteEl(hoy) && CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(HuellaDe(secreto)), Encoding.ASCII.GetBytes(Huella));

    public void RegistrarAcceso(IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        UltimoAcceso = reloj.AhoraUtc;
        Accesos++;
    }

    private string NuevoSecreto()
    {
        var secreto = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        Huella = HuellaDe(secreto);
        return secreto;
    }

    private static string HuellaDe(string secreto) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(secreto ?? string.Empty)));
}
