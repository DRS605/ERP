using System.IO.Ports;
using System.Net.Sockets;
using System.Text;
using AlxorCore.AgenteBascula;

// Agente local de báscula: lee el indicador (por TCP, habitual con conversores serie-Ethernet, o por el puerto serie)
// y expone el último peso en http://localhost:5199/peso para la pantalla de pesadas de ALXOR (botón «Leer báscula»).
//
// Configuración (appsettings.json o variables de entorno Bascula__*):
//   Bascula:Nombre        nombre que se guarda en la pesada (B1, Báscula puente…)
//   Bascula:Tcp           host:puerto del indicador (p. ej. 192.168.1.50:4001), o
//   Bascula:Puerto        puerto serie (COM3, /dev/ttyUSB0) y Bascula:Baudios (9600), Bascula:Paridad (None)
//   Bascula:Peticion      texto que se envía para pedir el peso en indicadores bajo petición (p. ej. "SI\r\n"); vacío = continuo
//   Bascula:Origenes      orígenes web permitidos (CORS), separados por comas; por defecto, cualquiera
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(builder.Configuration["Urls"] ?? "http://localhost:5199");
var cfg = builder.Configuration.GetSection("Bascula");
var origenes = (cfg["Origenes"] ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
{
    if (origenes.Length > 0)
    {
        p.WithOrigins(origenes);
    }
    else
    {
        p.AllowAnyOrigin();
    }

    p.AllowAnyHeader().AllowAnyMethod();
}));
var estado = new EstadoBascula(cfg["Nombre"] ?? "B1");
builder.Services.AddSingleton(estado);
builder.Services.AddHostedService(sp => new LectorBascula(estado, cfg, sp.GetRequiredService<ILogger<LectorBascula>>()));

var app = builder.Build();
// El ERP (en https) llama a este agente en localhost: Chrome pide permiso de red privada en la petición previa.
app.Use(async (ctx, siguiente) =>
{
    if (ctx.Request.Headers.ContainsKey("Access-Control-Request-Private-Network"))
    {
        ctx.Response.Headers["Access-Control-Allow-Private-Network"] = "true";
    }

    await siguiente(ctx).ConfigureAwait(false);
});
app.UseCors();
app.MapGet("/peso", (EstadoBascula e) => e.Ultima is { } p
    ? Results.Ok(new { bascula = e.Nombre, kilos = p.Kilos, estable = p.Estable, leidaEn = p.LeidaEn, trama = p.Trama, antiguedadSegundos = (DateTimeOffset.UtcNow - p.LeidaEn).TotalSeconds })
    : Results.Problem(e.Error ?? "Aún no se ha leído ningún peso.", statusCode: 503));
app.MapGet("/estado", (EstadoBascula e) => Results.Ok(new { bascula = e.Nombre, conectada = e.Conectada, error = e.Error, ultima = e.Ultima }));
app.Run();

/// <summary>Último peso leído y estado de la conexión.</summary>
internal sealed class EstadoBascula(string nombre)
{
    public string Nombre { get; } = nombre;

    public volatile bool Conectada;

    public string? Error { get; set; }

    public Pesada? Ultima { get; set; }
}

/// <summary>Lee tramas del indicador (líneas terminadas en CR o LF) y guarda la última pesada válida; reconecta si se corta.</summary>
internal sealed partial class LectorBascula(EstadoBascula estado, IConfiguration cfg, ILogger<LectorBascula> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await using var flujo = await AbrirAsync(ct).ConfigureAwait(false);
                estado.Conectada = true;
                estado.Error = null;
                await LeerAsync(flujo, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return;
            }
#pragma warning disable CA1031 // El agente no se para por un fallo del indicador: lo anota y reconecta.
            catch (Exception ex)
#pragma warning restore CA1031
            {
                estado.Conectada = false;
                estado.Error = ex.Message;
                Registro.Fallo(log, ex.Message);
            }

            await Task.Delay(TimeSpan.FromSeconds(3), ct).ConfigureAwait(false);
        }
    }

    private async Task<Stream> AbrirAsync(CancellationToken ct)
    {
        if (cfg["Tcp"] is { Length: > 0 } tcp)
        {
            var partes = tcp.Split(':');
            var cliente = new TcpClient();
            await cliente.ConnectAsync(partes[0], int.Parse(partes[1], System.Globalization.CultureInfo.InvariantCulture), ct).ConfigureAwait(false);
            return cliente.GetStream();
        }

        var puerto = new SerialPort(cfg["Puerto"] ?? "COM1", int.Parse(cfg["Baudios"] ?? "9600", System.Globalization.CultureInfo.InvariantCulture),
            Enum.Parse<Parity>(cfg["Paridad"] ?? "None", true), int.Parse(cfg["BitsDatos"] ?? "8", System.Globalization.CultureInfo.InvariantCulture), StopBits.One);
        puerto.Open();
        return puerto.BaseStream;
    }

    private async Task LeerAsync(Stream flujo, CancellationToken ct)
    {
        var peticion = cfg["Peticion"];
        var buffer = new byte[256];
        var linea = new StringBuilder();
        using var temporizador = string.IsNullOrEmpty(peticion) ? null : new PeriodicTimer(TimeSpan.FromMilliseconds(500));
        while (!ct.IsCancellationRequested)
        {
            if (temporizador is not null)
            {
                await flujo.WriteAsync(Encoding.ASCII.GetBytes(peticion!.Replace("\\r", "\r", StringComparison.Ordinal).Replace("\\n", "\n", StringComparison.Ordinal)), ct)
                    .ConfigureAwait(false);
            }

            var n = await flujo.ReadAsync(buffer, ct).ConfigureAwait(false);
            if (n == 0)
            {
                throw new IOException("El indicador ha cerrado la conexión.");
            }

            foreach (var c in Encoding.ASCII.GetString(buffer, 0, n))
            {
                if (c is '\r' or '\n')
                {
                    if (linea.Length > 0 && LecturaBascula.Interpretar(linea.ToString(), DateTimeOffset.UtcNow) is { } pesada)
                    {
                        estado.Ultima = pesada;
                    }

                    linea.Clear();
                }
                else
                {
                    linea.Append(c);
                }
            }

            if (temporizador is not null)
            {
                await temporizador.WaitForNextTickAsync(ct).ConfigureAwait(false);
            }
        }
    }

    private static partial class Registro
    {
        [LoggerMessage(Level = LogLevel.Warning, Message = "Báscula: {Mensaje}. Se reintenta en 3 s.")]
        public static partial void Fallo(ILogger logger, string mensaje);
    }
}
