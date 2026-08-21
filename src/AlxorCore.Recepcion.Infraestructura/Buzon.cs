using AlxorCore.Recepcion.Aplicacion;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AlxorCore.Recepcion.Infraestructura;

/// <summary>
/// Configuración del buzón de correo del que se leen las facturas de proveedor. La contraseña
/// NO se guarda aquí: solo su referencia (<see cref="SecretoRef"/>) al almacén de secretos.
/// </summary>
public sealed class OpcionesBuzon
{
    public const string Seccion = "BuzonProveedores";

    /// <summary>Si el buzón está activo (por defecto no, para no arrancar procesos sin configurar).</summary>
    public bool Activo { get; set; }

    /// <summary>Empresa a la que se asignan las facturas del buzón.</summary>
    public Guid? EmpresaId { get; set; }

    public string? Host { get; set; }

    public int Puerto { get; set; } = 993;

    public bool UsarSsl { get; set; } = true;

    public string? Usuario { get; set; }

    /// <summary>Clave (referencia) del secreto con la contraseña, en el almacén de secretos.</summary>
    public string? SecretoRef { get; set; }

    /// <summary>Cada cuántos segundos se revisa el buzón.</summary>
    public int IntervaloSegundos { get; set; } = 300;

    public bool Configurado => Activo && EmpresaId is not null && !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(Usuario);
}

/// <summary>
/// Almacén de secretos respaldado por la configuración/entorno. Las credenciales viven en la
/// sección <c>Secretos</c> de la configuración o en una variable de entorno, nunca en la BD.
/// </summary>
internal sealed class AlmacenSecretosConfiguracion : IAlmacenSecretos
{
    private readonly IConfiguration _configuracion;

    public AlmacenSecretosConfiguracion(IConfiguration configuracion) => _configuracion = configuracion;

    public string? Obtener(string referencia)
    {
        if (string.IsNullOrWhiteSpace(referencia))
        {
            return null;
        }

        return _configuracion[$"Secretos:{referencia}"] ?? Environment.GetEnvironmentVariable(referencia);
    }
}

/// <summary>Buzón inactivo: cuando no hay correo configurado. No lee nada.</summary>
internal sealed class BuzonInactivo : IBuzonFacturas
{
    public bool Configurado => false;

    public Task<IReadOnlyList<CorreoEntrante>> LeerNuevosAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<CorreoEntrante>>(Array.Empty<CorreoEntrante>());
}

/// <summary>Adaptador IMAP (MailKit): lee los correos no leídos y extrae los adjuntos PDF.</summary>
internal sealed class BuzonImapMailKit : IBuzonFacturas
{
    private readonly OpcionesBuzon _opciones;
    private readonly IAlmacenSecretos _secretos;
    private readonly ILogger<BuzonImapMailKit> _log;

    public BuzonImapMailKit(IOptions<OpcionesBuzon> opciones, IAlmacenSecretos secretos, ILogger<BuzonImapMailKit> log)
    {
        ArgumentNullException.ThrowIfNull(opciones);
        _opciones = opciones.Value;
        _secretos = secretos;
        _log = log;
    }

    public bool Configurado => _opciones.Configurado;

    public async Task<IReadOnlyList<CorreoEntrante>> LeerNuevosAsync(CancellationToken ct = default)
    {
        if (!_opciones.Configurado)
        {
            return Array.Empty<CorreoEntrante>();
        }

        var clave = string.IsNullOrWhiteSpace(_opciones.SecretoRef) ? null : _secretos.Obtener(_opciones.SecretoRef!);
        if (string.IsNullOrWhiteSpace(clave))
        {
            _log.LogWarning("Buzón de facturas configurado pero sin contraseña en el almacén de secretos (referencia «{Ref}»).", _opciones.SecretoRef);
            return Array.Empty<CorreoEntrante>();
        }

        var resultado = new List<CorreoEntrante>();
        try
        {
            using var cliente = new ImapClient();
            var seguridad = _opciones.UsarSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable;
            await cliente.ConnectAsync(_opciones.Host, _opciones.Puerto, seguridad, ct).ConfigureAwait(false);
            await cliente.AuthenticateAsync(_opciones.Usuario, clave, ct).ConfigureAwait(false);

            var bandeja = cliente.Inbox;
            await bandeja.OpenAsync(FolderAccess.ReadWrite, ct).ConfigureAwait(false);
            var noLeidos = await bandeja.SearchAsync(SearchQuery.NotSeen, ct).ConfigureAwait(false);

            foreach (var uid in noLeidos)
            {
                var mensaje = await bandeja.GetMessageAsync(uid, ct).ConfigureAwait(false);
                var adjuntos = new List<AdjuntoCorreo>();
                foreach (var parte in mensaje.Attachments.OfType<MimePart>())
                {
                    var nombre = parte.FileName ?? parte.ContentDisposition?.FileName ?? "adjunto";
                    if (!nombre.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    using var memoria = new MemoryStream();
                    await parte.Content.DecodeToAsync(memoria, ct).ConfigureAwait(false);
                    adjuntos.Add(new AdjuntoCorreo(nombre, parte.ContentType?.MimeType ?? "application/pdf", memoria.ToArray()));
                }

                if (adjuntos.Count > 0)
                {
                    var remitente = mensaje.From.Mailboxes.FirstOrDefault()?.Address ?? mensaje.From.ToString();
                    resultado.Add(new CorreoEntrante(remitente, mensaje.Subject ?? string.Empty, mensaje.Date, adjuntos));
                }

                await bandeja.AddFlagsAsync(uid, MessageFlags.Seen, silent: true, ct).ConfigureAwait(false);
            }

            await cliente.DisconnectAsync(quit: true, ct).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _log.LogError(ex, "Error leyendo el buzón de facturas de proveedor.");
            return Array.Empty<CorreoEntrante>();
        }

        return resultado;
    }
}
