using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using Microsoft.EntityFrameworkCore;

namespace AlxorCore.Api.Comun;

/// <summary>
/// Centros con que trabaja el usuario de la petición. Sin centros asignados trabaja con todos (y con los documentos sin
/// centro); con alguno, solo ve los documentos de sus centros y crea documentos en ellos.
/// </summary>
public sealed class CentrosUsuario
{
    private readonly IHttpContextAccessor _http;
    private readonly IContextoEmpresa _contexto;
    private readonly IConsultaCentros _centros;
    private IReadOnlyCollection<Guid>? _permitidos;
    private bool _cargados;

    public CentrosUsuario(IHttpContextAccessor http, IContextoEmpresa contexto, IConsultaCentros centros)
    {
        _http = http;
        _contexto = contexto;
        _centros = centros;
    }

    /// <summary>Los centros del usuario, o <c>null</c> si trabaja con todos.</summary>
    public async Task<IReadOnlyCollection<Guid>?> PermitidosAsync(CancellationToken ct = default)
    {
        if (!_cargados)
        {
            var usuario = _http.HttpContext?.User.ObtenerUsuarioId();
            _permitidos = usuario is { } u && _contexto.EmpresaId is { } e ? await _centros.PermitidosAsync(e, u, ct).ConfigureAwait(false) : null;
            _cargados = true;
        }

        return _permitidos;
    }

    /// <summary>¿Puede ver un documento de ese centro?</summary>
    public async Task<bool> PuedeVerAsync(Guid? centroId, CancellationToken ct = default) =>
        await PermitidosAsync(ct).ConfigureAwait(false) is not { } p || (centroId is { } c && p.Contains(c));

    /// <summary>
    /// El centro (y la caja) de un documento nuevo: el indicado, si existe, está activo y el usuario trabaja con él; sin
    /// indicar, ninguno o, si el usuario solo trabaja con uno, ese. Una caja tiene que ser de su centro y estar activa.
    /// </summary>
    public async Task<Resultado<(Guid? Centro, Guid? Caja)>> ResolverAsync(Guid? centroId, Guid? cajaId = null, bool admiteCaja = false, CancellationToken ct = default)
    {
        var permitidos = await PermitidosAsync(ct).ConfigureAwait(false);
        var centro = centroId is { } c && c != Guid.Empty ? c : (Guid?)null;

        // Sin centro en el documento, el centro de trabajo elegido en la aplicación (cabeceras X-Centro y X-Caja).
        if (centro is null && _http.HttpContext?.Request.Headers is { } cabeceras && Guid.TryParse(cabeceras["X-Centro"].ToString(), out var deCabecera))
        {
            centro = deCabecera;
            if (admiteCaja && (cajaId is null || cajaId == Guid.Empty) && Guid.TryParse(cabeceras["X-Caja"].ToString(), out var cajaCabecera))
            {
                cajaId = cajaCabecera;
            }
        }
        if (centro is null)
        {
            if (permitidos is null)
            {
                return cajaId is { } k && k != Guid.Empty
                    ? Resultado.Fallo<(Guid?, Guid?)>(Error.Validacion("centro.caja_sin_centro", "Indica el centro de la caja."))
                    : Resultado.Ok<(Guid?, Guid?)>((null, null));
            }

            if (permitidos.Count != 1)
            {
                return Resultado.Fallo<(Guid?, Guid?)>(Error.Validacion("centro.obligatorio", "Indica el centro del documento."));
            }

            centro = permitidos.First();
        }

        if (permitidos is not null && !permitidos.Contains(centro.Value))
        {
            return Resultado.Fallo<(Guid?, Guid?)>(Error.Prohibido("centro.sin_acceso", "No trabajas con ese centro."));
        }

        var info = await _centros.ObtenerAsync(centro.Value, ct).ConfigureAwait(false);
        if (info is null)
        {
            return Resultado.Fallo<(Guid?, Guid?)>(Error.Validacion("centro.no_encontrado", "El centro no existe."));
        }

        if (!info.Activo)
        {
            return Resultado.Fallo<(Guid?, Guid?)>(Error.Validacion("centro.inactivo", $"El centro {info.Codigo} está dado de baja."));
        }

        if (cajaId is not { } caja || caja == Guid.Empty)
        {
            return Resultado.Ok<(Guid?, Guid?)>((centro, null));
        }

        var k2 = info.Cajas.FirstOrDefault(x => x.Id == caja);
        if (k2 is null && centroId is null)
        {
            // La caja de la cabecera es de otro centro: el documento va sin caja.
            return Resultado.Ok<(Guid?, Guid?)>((centro, null));
        }

        return k2 is null
            ? Resultado.Fallo<(Guid?, Guid?)>(Error.Validacion("centro.caja_no_encontrada", "La caja no es de ese centro."))
            : !k2.Activa
                ? Resultado.Fallo<(Guid?, Guid?)>(Error.Validacion("centro.caja_inactiva", $"La caja {k2.Codigo} está dada de baja."))
                : Resultado.Ok<(Guid?, Guid?)>((centro, caja));
    }
}

/// <summary>
/// Un usuario limitado a unos centros no abre ni toca (404) documentos de otros: facturas y tickets, presupuestos,
/// pedidos y albaranes de venta y facturas de proveedor, en cualquier ruta <c>/{tipo}/{id}…</c>.
/// </summary>
public sealed class MiddlewareCentros
{
    private static readonly string[] Prefijos = ["/facturas/", "/presupuestos/", "/pedidos-venta/", "/albaranes-venta/", "/gastos/"];

    private readonly RequestDelegate _siguiente;

    public MiddlewareCentros(RequestDelegate siguiente) => _siguiente = siguiente;

    public async Task InvokeAsync(HttpContext contexto)
    {
        ArgumentNullException.ThrowIfNull(contexto);
        var ruta = contexto.Request.Path.Value ?? string.Empty;
        var prefijo = Prefijos.FirstOrDefault(p => ruta.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        if (prefijo is not null && contexto.User.Identity?.IsAuthenticated == true
            && Guid.TryParse(ruta[prefijo.Length..].Split('/')[0], out var id))
        {
            var centros = contexto.RequestServices.GetRequiredService<CentrosUsuario>();
            if (await centros.PermitidosAsync(contexto.RequestAborted).ConfigureAwait(false) is not null)
            {
                var (existe, centro) = await CentroDeAsync(contexto.RequestServices, prefijo, id, contexto.RequestAborted).ConfigureAwait(false);
                if (existe && !await centros.PuedeVerAsync(centro, contexto.RequestAborted).ConfigureAwait(false))
                {
                    await ResultadosHttp.AProblema(Error.NoEncontrado("centro.documento_ajeno", "El documento no existe o es de un centro con que no trabajas."))
                        .ExecuteAsync(contexto).ConfigureAwait(false);
                    return;
                }
            }
        }

        await _siguiente(contexto).ConfigureAwait(false);
    }

    private static async Task<(bool Existe, Guid? Centro)> CentroDeAsync(IServiceProvider sp, string prefijo, Guid id, CancellationToken ct)
    {
        async Task<(bool, Guid?)> De<T>(DbContext ctx, Func<IQueryable<T>, IQueryable<Guid?>> centro)
            where T : class
        {
            var filas = await centro(ctx.Set<T>().AsNoTracking()).Take(1).ToListAsync(ct).ConfigureAwait(false);
            return filas.Count == 0 ? (false, null) : (true, filas[0]);
        }

        var fac = sp.GetRequiredService<AlxorCore.Facturacion.Infraestructura.FacturacionDbContext>();
        return prefijo switch
        {
            "/facturas/" => await De<AlxorCore.Facturacion.Dominio.Factura>(fac, q => q.Where(x => x.Id == id).Select(x => x.CentroId)).ConfigureAwait(false),
            "/presupuestos/" => await De<AlxorCore.Facturacion.Dominio.Presupuesto>(fac, q => q.Where(x => x.Id == id).Select(x => x.CentroId)).ConfigureAwait(false),
            "/pedidos-venta/" => await De<AlxorCore.Facturacion.Dominio.PedidoVenta>(fac, q => q.Where(x => x.Id == id).Select(x => x.CentroId)).ConfigureAwait(false),
            "/albaranes-venta/" => await De<AlxorCore.Facturacion.Dominio.AlbaranVenta>(fac, q => q.Where(x => x.Id == id).Select(x => x.CentroId)).ConfigureAwait(false),
            _ => await De<AlxorCore.Gastos.Dominio.Gasto>(sp.GetRequiredService<AlxorCore.Gastos.Infraestructura.GastosDbContext>(),
                q => q.Where(x => x.Id == id).Select(x => x.CentroId)).ConfigureAwait(false),
        };
    }
}

/// <summary>Filtro de centros de un listado: el pedido (si el usuario trabaja con él) y, si no, los del usuario.</summary>
public static class FiltroCentros
{
    public static async Task<IReadOnlyCollection<Guid>?> DeAsync(CentrosUsuario centros, Guid? centroId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(centros);
        var permitidos = await centros.PermitidosAsync(ct).ConfigureAwait(false);
        return centroId is not { } c ? permitidos : permitidos is null || permitidos.Contains(c) ? [c] : [];
    }
}
