using AlxorCore.Facturacion.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Facturacion.Aplicacion;

/// <summary>Línea de mercancía al crear una carta de porte.</summary>
public sealed record LineaCartaPorteComando(string? Descripcion, int Bultos, decimal PesoKg, string? Marcas = null, string? Embalaje = null,
    decimal? PesoNetoKg = null, decimal? VolumenM3 = null, string? CodigoArancelario = null, Guid? ProductoId = null);

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
    Guid? AlbaranId = null,
    TransporteCarta? Transporte = null);

/// <summary>Vista de una línea de mercancía de la carta de porte.</summary>
public sealed record LineaCartaPorteDto(string Descripcion, int Bultos, decimal PesoKg, string? Marcas = null, string? Embalaje = null,
    decimal? PesoNetoKg = null, decimal? VolumenM3 = null, string? CodigoArancelario = null)
{
    public static LineaCartaPorteDto Desde(LineaCartaPorte l) => new(l.Descripcion, l.Bultos, l.PesoKg, l.Marcas, l.Embalaje, l.PesoNetoKg, l.VolumenM3, l.CodigoArancelario);
}

/// <summary>Vista completa de una carta de porte.</summary>
public sealed record CartaPorteDto(
    Guid Id, string NumeroCompleto, DateOnly FechaExpedicion,
    string RemitenteNombre, string? RemitenteNif,
    Guid? DestinatarioClienteId, string DestinatarioNombre, string? DestinatarioNif,
    string? TransportistaNombre, string? TransportistaNif, string? Matricula,
    string LugarOrigen, string LugarDestino, DateOnly? FechaCarga, string? Observaciones,
    Guid? AlbaranId, int TotalBultos, decimal TotalPesoKg, IReadOnlyList<LineaCartaPorteDto> Lineas, bool Anulada = false, string? MotivoAnulacion = null,
    decimal? TotalPesoNetoKg = null, decimal? TotalVolumenM3 = null, TransporteCarta? Transporte = null,
    IReadOnlyList<string>? Certificados = null)
{
    public static CartaPorteDto Desde(CartaPorte c) => new(
        c.Id, c.NumeroCompleto, c.FechaExpedicion, c.RemitenteNombre, c.RemitenteNif,
        c.DestinatarioClienteId, c.DestinatarioNombre, c.DestinatarioNif,
        c.TransportistaNombre, c.TransportistaNif, c.Matricula,
        c.LugarOrigen, c.LugarDestino, c.FechaCarga, c.Observaciones,
        c.AlbaranId, c.TotalBultos, c.TotalPesoKg, c.Lineas.Select(LineaCartaPorteDto.Desde).ToList(), c.AnuladaEn is not null, c.MotivoAnulacion,
        c.TotalPesoNetoKg, c.TotalVolumenM3, new TransporteCarta
        {
            Modo = c.Modo, TransportistaId = c.TransportistaId, VehiculoId = c.VehiculoId, MatriculaRemolque = c.MatriculaRemolque,
            Conductor = c.Conductor, Conductor2 = c.Conductor2, TemperaturaConsigna = c.TemperaturaConsigna, Termografo = c.Termografo,
            Incoterm = c.Incoterm, LugarIncoterm = c.LugarIncoterm, Portes = c.Portes, DocumentosAnexos = c.DocumentosAnexos, Instrucciones = c.Instrucciones,
            PaisOrigen = c.PaisOrigen, PaisDestino = c.PaisDestino, Naviera = c.Naviera, Buque = c.Buque, Contenedor = c.Contenedor, Precinto = c.Precinto,
            PuertoCarga = c.PuertoCarga, PuertoDestino = c.PuertoDestino, CompaniaAerea = c.CompaniaAerea, Vuelo = c.Vuelo, Awb = c.Awb, Reserva = c.Reserva,
        });
}

/// <summary>Resumen de una carta de porte para listados.</summary>
public sealed record CartaPorteResumen(Guid Id, string NumeroCompleto, DateOnly FechaExpedicion, string DestinatarioNombre, string LugarDestino, int TotalBultos, decimal TotalPesoKg,
    string LugarOrigen = "", string? TransportistaNombre = null, string? Matricula = null, bool Anulada = false, string? MotivoAnulacion = null,
    string? PaisDestino = null, string? Incoterm = null);

/// <summary>Repositorio de cartas de porte (escritura y numeración).</summary>
public interface IRepositorioCartasPorte
{
    void Agregar(CartaPorte cartaPorte);

    Task<CartaPorte?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Siguiente número correlativo para la empresa/serie/ejercicio (máximo actual + 1).</summary>
    Task<int> SiguienteNumeroAsync(Guid empresaId, string? serie, int ejercicio, CancellationToken ct = default);
}

/// <summary>Consultas de lectura de cartas de porte.</summary>
public interface IConsultaCartasPorte
{
    Task<CartaPorteDto?> ObtenerAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<CartaPorteResumen>> ListarAsync(Guid empresaId, CancellationToken ct = default);

    /// <summary>Cartas de porte (vivas) de los albaranes de los pedidos que se facturaron en esta factura.</summary>
    Task<IReadOnlyList<CartaPorteDto>> DeFacturaAsync(Guid facturaId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<CartaPorteDto>>([]);
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
    private readonly IRepositorioTransporte? _transporte;
    private readonly AlxorCore.Catalogo.Aplicacion.IConsultaProductos? _productos;

    public CrearCartaPorte(IRepositorioCartasPorte cartas, IConsultaClientes clientes, IConsultaEmpresas empresas, IUnidadDeTrabajoFacturacion unidadDeTrabajo, IReloj reloj,
        IRepositorioTransporte? transporte = null, AlxorCore.Catalogo.Aplicacion.IConsultaProductos? productos = null)
    {
        _transporte = transporte;
        _productos = productos;
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
        AlxorCore.Terceros.Aplicacion.ClienteDto? cliente = null;
        if (comando.DestinatarioClienteId is { } clienteId)
        {
            cliente = await _clientes.ObtenerAsync(clienteId, ct).ConfigureAwait(false);
            if (cliente is null)
            {
                return Resultado.Fallo<CartaPorteDto>(Error.NoEncontrado("cliente.no_encontrado", "El cliente no existe."));
            }

            destinatarioNombre = cliente.Nombre;
            destinatarioNif = cliente.NifFiscal;
            destino ??= DireccionTexto(cliente.Calle, cliente.CodigoPostal, cliente.Poblacion, cliente.Provincia);
        }

        // Transporte: el transportista y el vehículo habituales dan nombre, NIF y matrículas; los países salen de la
        // empresa y del cliente, y el Incoterm, del habitual del cliente.
        var transporte = comando.Transporte ?? new TransporteCarta();
        var transportistaNombre = comando.TransportistaNombre;
        var transportistaNif = comando.TransportistaNif;
        var matricula = comando.Matricula;
        if (transporte.VehiculoId is { } vehiculoId && vehiculoId != Guid.Empty)
        {
            var v = _transporte is null ? null : await _transporte.VehiculoAsync(vehiculoId, ct).ConfigureAwait(false);
            if (v is null || v.EmpresaId != empresaId)
            {
                return Resultado.Fallo<CartaPorteDto>(Error.Validacion("cartaporte.vehiculo", "El vehículo no existe."));
            }

            matricula ??= v.Matricula;
            transporte = transporte with { MatriculaRemolque = transporte.MatriculaRemolque ?? v.MatriculaRemolque, TransportistaId = transporte.TransportistaId ?? v.TransportistaId };
        }

        if (transporte.TransportistaId is { } transportistaId && transportistaId != Guid.Empty)
        {
            var t = _transporte is null ? null : await _transporte.TransportistaAsync(transportistaId, ct).ConfigureAwait(false);
            if (t is null || t.EmpresaId != empresaId)
            {
                return Resultado.Fallo<CartaPorteDto>(Error.Validacion("cartaporte.transportista", "El transportista no existe."));
            }

            transportistaNombre ??= t.Nombre;
            transportistaNif ??= t.Nif;
        }

        transporte = transporte with
        {
            PaisOrigen = transporte.PaisOrigen ?? AlxorCore.Nucleo.Comun.Paises.Codigo(empresa.Pais),
            PaisDestino = transporte.PaisDestino ?? AlxorCore.Nucleo.Comun.Paises.Codigo(cliente?.Pais),
            Incoterm = transporte.Incoterm ?? cliente?.Incoterm,
            LugarIncoterm = transporte.LugarIncoterm ?? (transporte.Incoterm is null ? cliente?.LugarIncoterm : null),
        };
        var normalizado = transporte.Normalizar();
        if (normalizado.EsFallo)
        {
            return Resultado.Fallo<CartaPorteDto>(normalizado.Error);
        }

        // Remitente: la empresa emisora; origen por defecto, su dirección fiscal.
        var origen = comando.LugarOrigen ?? DireccionTexto(empresa.Calle, empresa.CodigoPostal, empresa.Poblacion, empresa.Provincia);
        var fechaExpedicion = comando.FechaExpedicion ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var ejercicio = fechaExpedicion.Year;
        var numero = await _cartas.SiguienteNumeroAsync(empresaId, comando.Serie, ejercicio, ct).ConfigureAwait(false);

        var lineas = new List<DatosLineaCarta>();
        foreach (var l in comando.Lineas ?? [])
        {
            // El código arancelario, si no se indica, es el del artículo.
            var codigo = l.CodigoArancelario;
            if (string.IsNullOrWhiteSpace(codigo) && l.ProductoId is { } productoId && _productos is not null)
            {
                codigo = (await _productos.ObtenerAsync(productoId, ct).ConfigureAwait(false))?.CodigoArancelario;
            }

            lineas.Add(new DatosLineaCarta(l.Descripcion, l.Bultos, l.PesoKg, l.Marcas, l.Embalaje, l.PesoNetoKg, l.VolumenM3, codigo));
        }

        var carta = CartaPorte.Crear(
            empresaId, comando.Serie, ejercicio, numero, fechaExpedicion,
            empresa.RazonSocial, empresa.Nif, comando.DestinatarioClienteId, destinatarioNombre, destinatarioNif,
            transportistaNombre, transportistaNif, matricula, origen, destino,
            comando.FechaCarga, comando.Observaciones, comando.AlbaranId, lineas, _reloj);
        if (carta.EsFallo)
        {
            return Resultado.Fallo<CartaPorteDto>(carta.Error);
        }

        carta.Valor.AplicarTransporte(normalizado.Valor);

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
/// <summary>Caso de uso: anular una carta de porte (no se borra: su número queda usado).</summary>
public sealed class AnularCartaPorte
{
    private readonly IRepositorioCartasPorte _repo;
    private readonly IUnidadDeTrabajoFacturacion _unidad;
    private readonly IReloj _reloj;

    public AnularCartaPorte(IRepositorioCartasPorte repo, IUnidadDeTrabajoFacturacion unidad, IReloj reloj)
    {
        _repo = repo;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<Resultado<CartaPorteDto>> EjecutarAsync(Guid id, string? motivo, CancellationToken ct = default)
    {
        var c = await _repo.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (c is null)
        {
            return Resultado.Fallo<CartaPorteDto>(Error.NoEncontrado("cartaporte.no_encontrada", "La carta de porte no existe."));
        }

        var r = c.Anular(motivo, _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<CartaPorteDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(CartaPorteDto.Desde(c));
    }
}

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
