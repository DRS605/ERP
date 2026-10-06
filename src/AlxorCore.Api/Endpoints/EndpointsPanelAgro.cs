using AlxorCore.Agro.Aplicacion;
using AlxorCore.Agro.Dominio;
using AlxorCore.Api.Comun;
using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Api.Endpoints;

/// <summary>
/// Panel de campaña en vivo: lo que ha entrado hoy, lo que está en el almacén, lo que ha salido, la campaña acumulada
/// y los avisos de anomalías (reglas fijas sobre los datos: documentos olvidados en borrador, fruta parada, palés sin
/// expedir, rendimientos de confección anómalos).
/// </summary>
public static class EndpointsPanelAgro
{
    public sealed record KilosProductoDto(Guid ProductoId, string Producto, decimal Kilos);

    public sealed record DiaPanelDto(DateOnly Fecha, decimal KilosRecibidos, decimal KilosExpedidos);

    public sealed record AgricultorPanelDto(Guid AgricultorId, string Agricultor, int Entregas, decimal Kilos, decimal KilosPendientes);

    public sealed record AnomaliaDto(string Codigo, string Gravedad, string Mensaje, string? Entidad, Guid? Id, string? Referencia);

    public sealed record PanelCampanaDto(
        DateOnly Hoy, Guid? CampanaId, string? Campana, DateTimeOffset GeneradoEn,
        decimal KilosHoy, int RecepcionesHoy, int RecepcionesBorrador, IReadOnlyList<KilosProductoDto> HoyPorProducto,
        decimal KilosEnAlmacen, int PartidasConSaldo, IReadOnlyList<KilosProductoDto> AlmacenPorProducto,
        int PalesAbiertos, int PalesCerrados, int PalesExpedidosHoy, decimal KilosExpedidosHoy,
        decimal KilosConfeccionadosHoy, IReadOnlyList<DiaPanelDto> UltimosDias,
        decimal KilosCampana, decimal KilosLiquidados, decimal ImporteLiquidado, IReadOnlyList<AgricultorPanelDto> TopAgricultores,
        IReadOnlyList<AnomaliaDto> Anomalias);

    /// <summary>Umbrales de las reglas de anomalías.</summary>
    public const int DiasBorradorRecepcion = 1;
    public const int DiasFrutaParada = 7;
    public const int DiasPaleSinExpedir = 3;
    public const int DiasBorradorLiquidacion = 7;
    public const decimal RendimientoMinimo = 0.6m;

    public static IEndpointRouteBuilder MapearPanelAgro(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        rutas.MapGet("/agro/panel", async (Guid? campanaId, IContextoEmpresa contexto, RecepcionesAgro recepciones, PalesAgro pales, ConfeccionAgro confeccion,
                LiquidacionesAgro liquidaciones, MaestrosAgro maestros, InformesAgro informes, IConsultaProductos productos, IReloj reloj, CancellationToken ct) =>
            {
                if (contexto.EmpresaId is not { } e)
                {
                    return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
                }

                var hoy = DateOnly.FromDateTime(reloj.AhoraUtc.UtcDateTime);
                var desde = hoy.AddDays(-13);
                var campanas = await maestros.CampanasAsync(e, ct).ConfigureAwait(false);
                var campana = campanaId is { } cid ? campanas.FirstOrDefault(c => c.Id == cid)
                    : campanas.FirstOrDefault(c => c.Desde <= hoy && hoy <= c.Hasta) ?? campanas.OrderByDescending(c => c.Desde).FirstOrDefault();

                var nombres = new Dictionary<Guid, string>();
                async Task<string> NombreAsync(Guid id)
                {
                    if (!nombres.TryGetValue(id, out var n))
                    {
                        n = (await productos.ObtenerAsync(id, ct).ConfigureAwait(false))?.Nombre ?? "?";
                        nombres[id] = n;
                    }

                    return n;
                }

                // Entradas.
                var recientes = await recepciones.ListarAsync(e, hoy.AddDays(-60), hoy, null, ct).ConfigureAwait(false);
                var hoyConfirmadas = recientes.Where(r => r.Fecha == hoy && r.Estado == "Confirmada").ToList();
                var porProductoHoy = new Dictionary<Guid, decimal>();
                foreach (var r in hoyConfirmadas)
                {
                    foreach (var l in (await recepciones.ObtenerAsync(r.Id, ct).ConfigureAwait(false))?.Lineas ?? [])
                    {
                        nombres.TryAdd(l.ProductoId, l.ProductoNombre);
                        porProductoHoy[l.ProductoId] = porProductoHoy.GetValueOrDefault(l.ProductoId) + l.NetoKg;
                    }
                }

                var hoyPorProducto = new List<KilosProductoDto>();
                foreach (var (p, k) in porProductoHoy.OrderByDescending(x => x.Value))
                {
                    hoyPorProducto.Add(new KilosProductoDto(p, await NombreAsync(p).ConfigureAwait(false), k));
                }

                // Almacén.
                var partidas = (await recepciones.ExistenciasAsync(e, ct).ConfigureAwait(false)).Where(p => !p.Anulada && p.Saldo > 0m).ToList();
                var almacen = new List<KilosProductoDto>();
                foreach (var grupo in partidas.GroupBy(p => p.ProductoId).OrderByDescending(x => x.Sum(p => p.Saldo)))
                {
                    almacen.Add(new KilosProductoDto(grupo.Key, await NombreAsync(grupo.Key).ConfigureAwait(false), grupo.Sum(p => p.Saldo)));
                }

                // Salidas.
                var listaPales = await pales.ListarAsync(e, null, ct).ConfigureAwait(false);
                var expedidos = listaPales.Where(p => p.Estado == nameof(EstadoPale.Expedido) && p.FechaExpedicion >= desde).ToList();
                var partes = await confeccion.ListarAsync(e, hoy.AddDays(-60), hoy, ct).ConfigureAwait(false);

                var dias = Enumerable.Range(0, 14).Select(i => desde.AddDays(i)).Select(d => new DiaPanelDto(d,
                    recientes.Where(r => r.Fecha == d && r.Estado == "Confirmada").Sum(r => r.NetoKg),
                    expedidos.Where(p => p.FechaExpedicion == d).Sum(p => p.Kilos))).ToList();

                // Campaña.
                InformeCampanaDto? informe = null;
                if (campana is not null)
                {
                    var r = await informes.CampanaAsync(e, campana.Id, ct).ConfigureAwait(false);
                    informe = r.EsCorrecto ? r.Valor : null;
                }

                var top = (informe?.Agricultores ?? []).OrderByDescending(a => a.KilosEntregados).Take(10)
                    .Select(a => new AgricultorPanelDto(a.AgricultorId, a.Agricultor, a.Entregas, a.KilosEntregados, a.KilosPendientes)).ToList();

                // Anomalías.
                var anomalias = new List<AnomaliaDto>();
                foreach (var r in recientes.Where(r => r.Estado == "Borrador" && r.Fecha <= hoy.AddDays(-DiasBorradorRecepcion)))
                {
                    anomalias.Add(new AnomaliaDto("recepcion.borrador", "aviso", $"Recepción del {Fecha(r.Fecha)} de {r.AgricultorNombre} sigue en borrador ({(hoy.DayNumber - r.Fecha.DayNumber)} días).",
                        "recepcion", r.Id, r.Numero));
                }

                foreach (var r in recientes.Where(r => r.Estado == "Confirmada" && r.NetoKg <= 0m))
                {
                    anomalias.Add(new AnomaliaDto("recepcion.sin_kilos", "grave", $"Recepción {r.Numero} del {Fecha(r.Fecha)} confirmada sin kilos netos.", "recepcion", r.Id, r.Numero));
                }

                foreach (var p in partidas.Where(p => p.Fecha <= hoy.AddDays(-DiasFrutaParada)).OrderBy(p => p.Fecha))
                {
                    anomalias.Add(new AnomaliaDto("partida.parada", "aviso",
                        $"Partida {p.Codigo} ({await NombreAsync(p.ProductoId).ConfigureAwait(false)}) con {Redondeo.Formatear(p.Saldo)} kg sin mover desde el {Fecha(p.Fecha)}.",
                        "partida", p.Id, p.Codigo));
                }

                foreach (var p in listaPales.Where(p => p.Estado == nameof(EstadoPale.Cerrado)))
                {
                    // El palé no guarda la fecha de cierre: se toma la de la partida más reciente que lleva.
                    var fecha = p.Contenido.Select(c => partidas.FirstOrDefault(x => x.Id == c.PartidaId)?.Fecha).Where(f => f is not null).Max();
                    if (fecha is { } f && f <= hoy.AddDays(-DiasPaleSinExpedir))
                    {
                        anomalias.Add(new AnomaliaDto("pale.sin_expedir", "aviso", $"Palé {p.Sscc} cerrado y sin expedir (fruta del {Fecha(f)}).", "pale", p.Id, p.Sscc));
                    }
                }

                foreach (var p in partes.Where(p => p.Estado == nameof(EstadoParte.Borrador) && p.Fecha <= hoy.AddDays(-1)))
                {
                    anomalias.Add(new AnomaliaDto("confeccion.borrador", "aviso", $"Parte de confección {p.Numero ?? p.Descripcion} del {Fecha(p.Fecha)} sin validar.", "parte", p.Id, p.Numero));
                }

                foreach (var p in partes.Where(p => p.Estado == nameof(EstadoParte.Validado) && p.KilosConsumidos > 0m && p.KilosObtenidos / p.KilosConsumidos < RendimientoMinimo))
                {
                    anomalias.Add(new AnomaliaDto("confeccion.rendimiento", "grave",
                        $"Parte {p.Numero} del {Fecha(p.Fecha)}: rendimiento del {Redondeo.Formatear(Math.Round(p.KilosObtenidos * 100m / p.KilosConsumidos, 1))} % ({Redondeo.Formatear(p.KilosObtenidos)} de {Redondeo.Formatear(p.KilosConsumidos)} kg).",
                        "parte", p.Id, p.Numero));
                }

                foreach (var l in (await liquidaciones.ListarAsync(e, null, ct).ConfigureAwait(false)).Where(l => l.Estado == "Borrador" && l.Fecha <= hoy.AddDays(-DiasBorradorLiquidacion)))
                {
                    anomalias.Add(new AnomaliaDto("liquidacion.borrador", "aviso", $"Liquidación de {l.Agricultor} del {Fecha(l.Fecha)} sigue en borrador.", "liquidacion", l.Id, l.Numero));
                }

                return Results.Ok(new PanelCampanaDto(hoy, campana?.Id, campana?.Nombre, reloj.AhoraUtc,
                    hoyConfirmadas.Sum(r => r.NetoKg), hoyConfirmadas.Count, recientes.Count(r => r.Estado == "Borrador"), hoyPorProducto,
                    partidas.Sum(p => p.Saldo), partidas.Count, almacen,
                    listaPales.Count(p => p.Estado == nameof(EstadoPale.Abierto)), listaPales.Count(p => p.Estado == nameof(EstadoPale.Cerrado)),
                    expedidos.Count(p => p.FechaExpedicion == hoy), expedidos.Where(p => p.FechaExpedicion == hoy).Sum(p => p.Kilos),
                    partes.Where(p => p.Fecha == hoy && p.Estado == nameof(EstadoParte.Validado)).Sum(p => p.KilosObtenidos), dias,
                    informe?.KilosRecibidos ?? 0m, informe?.KilosLiquidados ?? 0m, informe?.ImporteLiquidado ?? 0m, top,
                    anomalias.OrderBy(a => a.Gravedad == "grave" ? 0 : 1).ToList()));
            })
            .WithTags("Agro").WithSummary("Panel de campaña en vivo: entradas de hoy, almacén, salidas, campaña y avisos de anomalías.")
            .RequierePermiso(Permisos.AgroLeer);
        return rutas;
    }

    private static string Fecha(DateOnly d) => d.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
}
