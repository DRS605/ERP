using AlxorCore.Facturacion.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Facturacion.Aplicacion;

/// <summary>Línea de mercancía al crear una carta de porte.</summary>
public sealed record LineaCartaPorteComando(string? Descripcion, int Bultos, decimal PesoKg);

/// <summary>Datos para crear una carta de porte.</summary>
public sealed record CrearCartaPorteComando(
    IReadOnlyList<LineaCartaPorteComando> Lineas,
    string? Serie = null,
    DateOnly? FechaExpedicion = null,
    Guid? DestinatarioClienteId = null,
    string? DestinatarioNombre = null,
    string? DestinatarioNif = null,
    string? TransportistaNombre = null,
    string? TransportistaNif = null,
    string? Matricula = null,
    string? LugarOrigen = null,
    string? LugarDestino = null,
    DateOnly? FechaCarga = null,
    string? Observaciones = null,
    Guid? AlbaranId = null);

/// <summary>Vista de una línea de mercancía de la carta de porte.</summary>
public sealed record LineaCartaPorteDto(string Descripcion, int Bultos, decimal PesoKg)
{
    public static LineaCartaPorteDto Desde(LineaCartaPorte l) => new(l.Descripcion, l.Bultos, l.PesoKg);
}

/// <summary>Vista completa de una carta de porte.</summary>
public sealed record CartaPorteDto(
    Guid Id, string NumeroCompleto, DateOnly FechaExpedicion,
    string RemitenteNombre, string? RemitenteNif,
    Guid? DestinatarioClienteId, string DestinatarioNombre, string? DestinatarioNif,
    string? TransportistaNombre, string? TransportistaNif, string? Matricula,
    string LugarOrigen, string LugarDestino, DateOnly? FechaCarga, string? Observaciones,
    Guid? AlbaranId, int TotalBultos, decimal TotalPesoKg, IReadOnlyList<LineaCartaPorteDto> Lineas)
{
    public static CartaPorteDto Desde(CartaPorte c) => new(
        c.Id, c.NumeroCompleto, c.FechaExpedicion, c.RemitenteNombre, c.RemitenteNif,
        c.DestinatarioClienteId, c.DestinatarioNombre, c.DestinatarioNif,
        c.TransportistaNombre, c.TransportistaNif, c.Matricula,
        c.LugarOrigen, c.LugarDestino, c.FechaCarga, c.Observaciones,
        c.AlbaranId, c.TotalBultos, c.TotalPesoKg, c.Lineas.Select(LineaCartaPorteDto.Desde).ToList());
}

/// <summary>Resumen de una carta de porte para listados.</summary>
public sealed record CartaPorteResumen(Guid Id, string NumeroCompleto, DateOnly FechaExpedicion, string DestinatarioNombre, string LugarDestino, int TotalBultos, decimal TotalPesoKg);

/// <summary>Repositorio de cartas de porte (escritura y numeración).</summary>
public interface IRepositorioCartasPorte
{
    void Agregar(CartaPorte cartaPorte);

    /// <summary>Siguiente número correlativo para la empresa/serie/ejercicio (máximo actual + 1).</summary>
    Task<int> SiguienteNumeroAsync(Guid empresaId, string? serie, int ejercicio, CancellationToken ct = default);
}

/// <summary>Consultas de lectura de cartas de porte.</summary>
public interface IConsultaCartasPorte
{
    Task<CartaPorteDto?> ObtenerAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<CartaPorteResumen>> ListarAsync(Guid empresaId, CancellationToken ct = default);
}

/// <summary>
/// Caso de uso: crear una carta de porte. El remitente (cargador) se toma de la empresa emisora; el
/// destinatario, del cliente indicado (o de los datos libres). El número es correlativo por
/// empresa/serie/ejercicio.
/// </summary>
public sealed class CrearCartaPorte
{
    private readonly IRepositorioCartasPorte _cartas;
    private readonly IConsultaClientes _clientes;
    private readonly IConsultaEmpresas _empresas;
    private readonly IUnidadDeTrabajoFacturacion _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public CrearCartaPorte(IRepositorioCartasPorte cartas, IConsultaClientes clientes, IConsultaEmpresas empresas, IUnidadDeTrabajoFacturacion unidadDeTrabajo, IReloj reloj)
    {
        _cartas = cartas;
        _clientes = clientes;
        _empresas = empresas;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<Resultado<CartaPorteDto>> EjecutarAsync(Guid empresaId, CrearCartaPorteComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var empresa = await _empresas.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        if (empresa is null)
        {
            return Resultado.Fallo<CartaPorteDto>(Error.NoEncontrado("empresa.no_encontrada", "La empresa no existe."));
        }

        // Destinatario: del cliente (snapshot) si se indica; si no, de los datos libres del comando.
        var destinatarioNombre = comando.DestinatarioNombre;
        var destinatarioNif = comando.DestinatarioNif;
        var destino = comando.LugarDestino;
        if (comando.DestinatarioClienteId is { } clienteId)
        {
            var cliente = await _clientes.ObtenerAsync(clienteId, ct).ConfigureAwait(false);
            if (cliente is null)
            {
                return Resultado.Fallo<CartaPorteDto>(Error.NoEncontrado("cliente.no_encontrado", "El cliente no existe."));
            }

            destinatarioNombre = cliente.Nombre;
            destinatarioNif = cliente.NifFiscal;
            destino ??= DireccionTexto(cliente.Calle, cliente.CodigoPostal, cliente.Poblacion, cliente.Provincia);
        }

        // Remitente: la empresa emisora; origen por defecto, su dirección fiscal.
        var origen = comando.LugarOrigen ?? DireccionTexto(empresa.Calle, empresa.CodigoPostal, empresa.Poblacion, empresa.Provincia);
        var fechaExpedicion = comando.FechaExpedicion ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var ejercicio = fechaExpedicion.Year;
        var numero = await _cartas.SiguienteNumeroAsync(empresaId, comando.Serie, ejercicio, ct).ConfigureAwait(false);

        var lineas = comando.Lineas is null
            ? new List<(string?, int, decimal)>()
            : comando.Lineas.Select(l => (l.Descripcion, l.Bultos, l.PesoKg)).ToList();

        var carta = CartaPorte.Crear(
            empresaId, comando.Serie, ejercicio, numero, fechaExpedicion,
            empresa.RazonSocial, empresa.Nif, comando.DestinatarioClienteId, destinatarioNombre, destinatarioNif,
            comando.TransportistaNombre, comando.TransportistaNif, comando.Matricula, origen, destino,
            comando.FechaCarga, comando.Observaciones, comando.AlbaranId, lineas, _reloj);
        if (carta.EsFallo)
        {
            return Resultado.Fallo<CartaPorteDto>(carta.Error);
        }

        _cartas.Agregar(carta.Valor);
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(CartaPorteDto.Desde(carta.Valor));
    }

    private static string DireccionTexto(string? calle, string? cp, string? poblacion, string? provincia)
    {
        var partes = new[] { calle, string.Join(' ', new[] { cp, poblacion }.Where(s => !string.IsNullOrWhiteSpace(s))), provincia }
            .Where(s => !string.IsNullOrWhiteSpace(s));
        return string.Join(", ", partes);
    }
}

/// <summary>Caso de uso: listar las cartas de porte de la empresa activa (más recientes primero).</summary>
public sealed class ListarCartasPorte
{
    private readonly IConsultaCartasPorte _consulta;

    public ListarCartasPorte(IConsultaCartasPorte consulta) => _consulta = consulta;

    public Task<IReadOnlyList<CartaPorteResumen>> EjecutarAsync(Guid empresaId, CancellationToken ct = default) =>
        _consulta.ListarAsync(empresaId, ct);
}

/// <summary>Caso de uso: obtener una carta de porte por su identificador.</summary>
public sealed class ObtenerCartaPorte
{
    private readonly IConsultaCartasPorte _consulta;

    public ObtenerCartaPorte(IConsultaCartasPorte consulta) => _consulta = consulta;

    public async Task<Resultado<CartaPorteDto>> EjecutarAsync(Guid id, CancellationToken ct = default)
    {
        var carta = await _consulta.ObtenerAsync(id, ct).ConfigureAwait(false);
        return carta is null
            ? Resultado.Fallo<CartaPorteDto>(Error.NoEncontrado("cartaporte.no_encontrada", "La carta de porte no existe."))
            : Resultado.Ok(carta);
    }
}
