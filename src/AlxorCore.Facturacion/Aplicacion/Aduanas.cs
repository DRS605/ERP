using System.Text;
using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Catalogo.Dominio;
using AlxorCore.Facturacion.Dominio;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Facturacion.Aplicacion;

public sealed record DespachoDto(Guid Id, Guid FacturaId, string Mrn, DateOnly FechaDespacho, DateOnly? FechaSalida, string? Aduana, string? Observaciones)
{
    public static DespachoDto Desde(DespachoAduanero d) => new(d.Id, d.FacturaId, d.Mrn, d.FechaDespacho, d.FechaSalida, d.Aduana, d.Observaciones);
}

public sealed record DatosDespacho(string? Mrn, DateOnly FechaDespacho, DateOnly? FechaSalida = null, string? Aduana = null, string? Observaciones = null);

/// <summary>
/// Factura de exportación y su situación aduanera: <c>Sin DUA</c> (falta la prueba de la exención), <c>Despachada</c>
/// (con DUA, sin salida confirmada) o <c>Salida confirmada</c>.
/// </summary>
public sealed record ExportacionDto(Guid FacturaId, string Numero, DateOnly Fecha, string Cliente, string? PaisDestino, decimal BaseExportacion, string Situacion,
    IReadOnlyList<DespachoDto> Despachos);

/// <summary>Partida de la declaración: la mercancía de un código arancelario y país de origen.</summary>
public sealed record PartidaAduanaDto(string? CodigoArancelario, string? PaisOrigen, string Descripcion, decimal Cantidad, decimal? PesoNetoKg, decimal Valor);

/// <summary>
/// Lo que el agente de aduanas necesita para el DUA de exportación de una factura: exportador y destinatario (con sus
/// EORI), Incoterm, transporte, bultos y pesos, y las partidas por código arancelario. Los avisos dicen lo que falta.
/// </summary>
public sealed record DatosAduanaDto(Guid FacturaId, string Factura, DateOnly Fecha, string Moneda,
    string Exportador, string ExportadorNif, string ExportadorEori, string Destinatario, string? DestinatarioNif, string? DestinatarioEori, string DestinatarioDireccion,
    string? PaisDestino, string? Incoterm, string? LugarIncoterm, string? ModoTransporte, string? Matricula, string? Contenedor, string? Precinto, string? Buque,
    string? Vuelo, string? Awb, int? Bultos, decimal? PesoBrutoKg, decimal? PesoNetoKg, decimal ValorTotal, IReadOnlyList<PartidaAduanaDto> Partidas,
    IReadOnlyList<string> CartasPorte, IReadOnlyList<DespachoDto> Despachos, IReadOnlyList<string> Avisos);

public interface IRepositorioDespachos
{
    Task<IReadOnlyList<DespachoAduanero>> DeFacturaAsync(Guid facturaId, CancellationToken ct = default);

    Task<IReadOnlyList<DespachoAduanero>> ListarAsync(Guid empresaId, CancellationToken ct = default);

    Task<DespachoAduanero?> ObtenerAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExisteMrnAsync(Guid empresaId, string mrn, Guid? salvo, CancellationToken ct = default);

    void Agregar(DespachoAduanero despacho);

    void Eliminar(DespachoAduanero despacho);
}

/// <summary>Facturas con líneas de ciertos códigos de IVA (las de exportación).</summary>
public interface IConsultaFacturasPorIva
{
    Task<IReadOnlyList<(Guid Id, string Numero, DateOnly Fecha, string Cliente, Guid? ClienteId, string Pais, decimal Base)>> ConCodigosAsync(
        Guid empresaId, IReadOnlyCollection<string> codigosIva, CancellationToken ct = default);
}

/// <summary>
/// Aduanas de exportación: el DUA/MRN de cada factura exenta por exportación (la prueba del art. 21), las facturas que
/// aún no lo tienen, y los datos para que el agente de aduanas presente la declaración.
/// </summary>
public sealed class Aduanas
{
    private readonly IRepositorioDespachos _despachos;
    private readonly IConsultaFacturas _facturas;
    private readonly IConsultaFacturasPorIva _porIva;
    private readonly IRepositorioTiposIva _tiposIva;
    private readonly IConsultaClientes _clientes;
    private readonly IConsultaEmpresas _empresas;
    private readonly IConsultaProductos _productos;
    private readonly IConsultaCartasPorte _cartas;
    private readonly IUnidadDeTrabajoFacturacion _unidad;

    public Aduanas(IRepositorioDespachos despachos, IConsultaFacturas facturas, IConsultaFacturasPorIva porIva, IRepositorioTiposIva tiposIva, IConsultaClientes clientes,
        IConsultaEmpresas empresas, IConsultaProductos productos, IConsultaCartasPorte cartas, IUnidadDeTrabajoFacturacion unidad)
    {
        _despachos = despachos; _facturas = facturas; _porIva = porIva; _tiposIva = tiposIva; _clientes = clientes; _empresas = empresas;
        _productos = productos; _cartas = cartas; _unidad = unidad;
    }

    /// <summary>Códigos de IVA de exportación de la empresa (clase Exportación), con los predeterminados siempre.</summary>
    private async Task<HashSet<string>> CodigosExportacionAsync(Guid empresaId, CancellationToken ct) =>
        (await _tiposIva.ListarAsync(empresaId, ct).ConfigureAwait(false)).Where(t => t.Clase == ClaseIva.Exportacion).Select(t => t.Codigo)
            .Concat(["EXPORT", "IGICEXPORT"]).ToHashSet(StringComparer.OrdinalIgnoreCase);

    public async Task<IReadOnlyList<ExportacionDto>> ExportacionesAsync(Guid empresaId, CancellationToken ct = default)
    {
        var codigos = await CodigosExportacionAsync(empresaId, ct).ConfigureAwait(false);
        var despachos = (await _despachos.ListarAsync(empresaId, ct).ConfigureAwait(false)).GroupBy(d => d.FacturaId).ToDictionary(g => g.Key, g => g.ToList());
        return (await _porIva.ConCodigosAsync(empresaId, codigos, ct).ConfigureAwait(false))
            .Select(f =>
            {
                var suyos = despachos.GetValueOrDefault(f.Id) ?? [];
                var situacion = suyos.Count == 0 ? "Sin DUA" : suyos.All(d => d.FechaSalida is not null) ? "Salida confirmada" : "Despachada";
                return new ExportacionDto(f.Id, f.Numero, f.Fecha, f.Cliente, Paises.Codigo(f.Pais), f.Base, situacion,
                    suyos.OrderBy(d => d.FechaDespacho).Select(DespachoDto.Desde).ToList());
            })
            .OrderBy(e => e.Situacion == "Sin DUA" ? 0 : e.Situacion == "Despachada" ? 1 : 2).ThenByDescending(e => e.Fecha).ToList();
    }

    public async Task<IReadOnlyList<DespachoDto>> DespachosAsync(Guid facturaId, CancellationToken ct = default) =>
        (await _despachos.DeFacturaAsync(facturaId, ct).ConfigureAwait(false)).OrderBy(d => d.FechaDespacho).Select(DespachoDto.Desde).ToList();

    public async Task<Resultado<DespachoDto>> RegistrarAsync(Guid empresaId, Guid facturaId, DatosDespacho datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var factura = await _facturas.ObtenerAsync(facturaId, ct).ConfigureAwait(false);
        if (factura is null)
        {
            return Resultado.Fallo<DespachoDto>(Error.NoEncontrado("factura.no_encontrada", "La factura no existe."));
        }

        if (factura.Estado == nameof(EstadoFactura.Anulada))
        {
            return Resultado.Fallo<DespachoDto>(Error.Conflicto("despacho.factura_estado", "La factura está anulada: no lleva DUA."));
        }

        var d = DespachoAduanero.Crear(empresaId, facturaId, datos.Mrn, datos.FechaDespacho, datos.FechaSalida, datos.Aduana, datos.Observaciones);
        if (d.EsFallo)
        {
            return Resultado.Fallo<DespachoDto>(d.Error);
        }

        if (await _despachos.ExisteMrnAsync(empresaId, d.Valor.Mrn, null, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<DespachoDto>(Error.Conflicto("despacho.mrn_duplicado", $"El MRN {d.Valor.Mrn} ya está registrado."));
        }

        _despachos.Agregar(d.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(DespachoDto.Desde(d.Valor));
    }

    public async Task<Resultado<DespachoDto>> ModificarAsync(Guid empresaId, Guid id, DatosDespacho datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        if (await _despachos.ObtenerAsync(id, ct).ConfigureAwait(false) is not { } d || d.EmpresaId != empresaId)
        {
            return Resultado.Fallo<DespachoDto>(Error.NoEncontrado("despacho.no_encontrado", "El despacho no existe."));
        }

        var r = d.Modificar(datos.Mrn, datos.FechaDespacho, datos.FechaSalida, datos.Aduana, datos.Observaciones);
        if (r.EsFallo)
        {
            return Resultado.Fallo<DespachoDto>(r.Error);
        }

        if (await _despachos.ExisteMrnAsync(empresaId, d.Mrn, d.Id, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<DespachoDto>(Error.Conflicto("despacho.mrn_duplicado", $"El MRN {d.Mrn} ya está registrado."));
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(DespachoDto.Desde(d));
    }

    public async Task<Resultado> EliminarAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        if (await _despachos.ObtenerAsync(id, ct).ConfigureAwait(false) is not { } d || d.EmpresaId != empresaId)
        {
            return Resultado.Fallo(Error.NoEncontrado("despacho.no_encontrado", "El despacho no existe."));
        }

        _despachos.Eliminar(d);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    /// <summary>Datos para el agente de aduanas: partidas por código arancelario y país de origen, y lo que falta.</summary>
    public async Task<Resultado<DatosAduanaDto>> DatosAsync(Guid empresaId, Guid facturaId, CancellationToken ct = default)
    {
        var f = await _facturas.ObtenerAsync(facturaId, ct).ConfigureAwait(false);
        if (f is null)
        {
            return Resultado.Fallo<DatosAduanaDto>(Error.NoEncontrado("factura.no_encontrada", "La factura no existe."));
        }

        var empresa = await _empresas.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        var cliente = f.ClienteId is { } c ? await _clientes.ObtenerAsync(c, ct).ConfigureAwait(false) : null;
        var avisos = new List<string>();
        var codigosExport = await CodigosExportacionAsync(empresaId, ct).ConfigureAwait(false);
        var lineas = f.Lineas.Where(l => codigosExport.Contains(l.CodigoIva)).ToList();
        if (lineas.Count == 0)
        {
            avisos.Add("La factura no tiene líneas exentas por exportación: se toman todas.");
            lineas = f.Lineas.ToList();
        }

        var partidas = new List<(string? Codigo, string? Pais, string Descripcion, decimal Cantidad, decimal? Peso, decimal Valor)>();
        foreach (var l in lineas)
        {
            var p = l.ProductoId is { } pid ? await _productos.ObtenerAsync(pid, ct).ConfigureAwait(false) : null;
            if (p?.CodigoArancelario is null)
            {
                avisos.Add($"«{l.Descripcion}» no tiene código arancelario (ficha del artículo).");
            }

            decimal? peso = p is null ? null
                : string.Equals(p.Unidad, "kg", StringComparison.OrdinalIgnoreCase) ? l.Cantidad
                : p.PesoKg is { } pk ? Math.Round(pk * l.Cantidad, 3, MidpointRounding.AwayFromZero) : null;
            partidas.Add((p?.CodigoArancelario, p?.PaisOrigen, l.Descripcion, l.Cantidad, peso, l.Base));
        }

        var agrupadas = partidas.GroupBy(x => (x.Codigo, x.Pais))
            .Select(g => new PartidaAduanaDto(g.Key.Codigo, g.Key.Pais, string.Join(" · ", g.Select(x => x.Descripcion).Distinct(StringComparer.CurrentCulture)),
                g.Sum(x => x.Cantidad), g.All(x => x.Peso is not null) ? g.Sum(x => x.Peso!.Value) : null, Redondeo.Dos(g.Sum(x => x.Valor))))
            .OrderBy(x => x.CodigoArancelario ?? "~", StringComparer.Ordinal).ToList();

        var cartas = (await _cartas.DeFacturaAsync(facturaId, ct).ConfigureAwait(false)).Where(x => !x.Anulada).ToList();
        var t = cartas.Select(x => x.Transporte).FirstOrDefault(x => x is not null);
        if (cartas.Count == 0)
        {
            avisos.Add("No hay carta de porte de sus albaranes: faltan bultos, peso bruto y transporte.");
        }

        var eoriCliente = cliente?.Eori;
        var paisDestino = t?.PaisDestino ?? Paises.Codigo(f.ClientePais);
        if (eoriCliente is null && paisDestino is { } pd && !Paises.UnionEuropea.Contains(pd))
        {
            avisos.Add("El destinatario no tiene EORI (ficha del cliente).");
        }

        var incoterm = t?.Incoterm ?? cliente?.Incoterm;
        if (incoterm is null)
        {
            avisos.Add("Falta el Incoterm (en la carta de porte o en la ficha del cliente).");
        }

        var nif = empresa?.Nif ?? string.Empty;
        return Resultado.Ok(new DatosAduanaDto(f.Id, f.NumeroCompleto, f.FechaEmision, empresa?.Moneda ?? "EUR",
            empresa?.RazonSocial ?? string.Empty, nif, $"{Paises.Codigo(empresa?.Pais) ?? "ES"}{nif}",
            f.ClienteNombre, f.ClienteNif, eoriCliente, string.Join(", ", new[] { f.ClienteCalle, $"{f.ClienteCodigoPostal} {f.ClientePoblacion}".Trim(), f.ClienteProvincia, f.ClientePais }
                .Where(x => !string.IsNullOrWhiteSpace(x))),
            paisDestino, incoterm, t?.LugarIncoterm ?? (t?.Incoterm is null ? cliente?.LugarIncoterm : null), t?.Modo.ToString(),
            cartas.Select(x => x.Matricula).FirstOrDefault(x => x is not null), t?.Contenedor, t?.Precinto, t?.Buque, t?.Vuelo, t?.Awb,
            cartas.Count > 0 ? cartas.Sum(x => x.TotalBultos) : null, cartas.Count > 0 ? cartas.Sum(x => x.TotalPesoKg) : null,
            agrupadas.All(x => x.PesoNetoKg is not null) ? agrupadas.Sum(x => x.PesoNetoKg!.Value) : null,
            Redondeo.Dos(agrupadas.Sum(x => x.Valor)), agrupadas, cartas.Select(x => x.NumeroCompleto).ToList(),
            await DespachosAsync(facturaId, ct).ConfigureAwait(false), avisos.Distinct(StringComparer.Ordinal).ToList()));
    }

    /// <summary>Las partidas en CSV (separado por punto y coma, como lo abre Excel en español).</summary>
    public static string Csv(DatosAduanaDto d)
    {
        ArgumentNullException.ThrowIfNull(d);
        var ci = Redondeo.NumeroEspanol;
        static string C(string? s) => s is null ? string.Empty : s.Contains(';', StringComparison.Ordinal) || s.Contains('"', StringComparison.Ordinal) ? $"\"{s.Replace("\"", "\"\"", StringComparison.Ordinal)}\"" : s;
        var sb = new StringBuilder();
        sb.AppendLine("Factura;Exportador;EORI exportador;Destinatario;EORI destinatario;País destino;Incoterm;Código arancelario;País origen;Descripción;Cantidad;Peso neto kg;Valor;Moneda");
        foreach (var p in d.Partidas)
        {
            sb.AppendLine(string.Join(';', C(d.Factura), C(d.Exportador), C(d.ExportadorEori), C(d.Destinatario), C(d.DestinatarioEori), C(d.PaisDestino),
                C(d.Incoterm is null ? null : $"{d.Incoterm} {d.LugarIncoterm}".Trim()), C(p.CodigoArancelario), C(p.PaisOrigen), C(p.Descripcion),
                p.Cantidad.ToString(ci), p.PesoNetoKg?.ToString(ci) ?? string.Empty, p.Valor.ToString("0.00", ci), C(d.Moneda)));
        }

        return sb.ToString();
    }
}
