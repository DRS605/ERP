using System.Security.Claims;
using AlxorCore.Agro.Aplicacion;
using AlxorCore.Api.Comun;
using AlxorCore.Extensiones.Aplicacion;
using AlxorCore.Extensiones.Dominio;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Seguridad;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Api.Endpoints;

/// <summary>
/// Portal del agricultor y del cliente. La empresa crea un acceso (un enlace con una clave secreta) para un agricultor o
/// un cliente; el enlace se canjea en <c>POST /portal/entrar</c> por una sesión que solo vale para las rutas /portal y
/// solo ve lo suyo: el agricultor, sus entregas y liquidaciones; el cliente, sus albaranes, facturas y su saldo.
/// </summary>
public static class EndpointsPortal
{
    public sealed record PeticionEntrar(string? Clave);

    public sealed record SesionPortalDto(string Token, DateTimeOffset ExpiraEn, string Tipo, string Nombre, string Empresa);

    public sealed record YoPortalDto(string Tipo, string Nombre, string Empresa);

    public sealed record EntregaPortalDto(Guid Id, string? Numero, DateOnly Fecha, decimal NetoKg, IReadOnlyList<LineaEntregaPortalDto> Lineas);

    public sealed record LineaEntregaPortalDto(string Producto, decimal NetoKg, int Envases, string? Calibre, string? Partida);

    public sealed record LiquidacionPortalDto(Guid Id, string? Numero, DateOnly Fecha, DateOnly Desde, DateOnly Hasta, decimal Kilos, decimal BaseImponible, decimal APagar);

    public sealed record ResumenAgricultorDto(decimal KilosEntregados, int Entregas, decimal KilosLiquidados, decimal KilosPendientesLiquidar, decimal ImporteLiquidado,
        DateOnly? UltimaEntrega);

    public sealed record FacturaPortalDto(Guid Id, string Numero, DateOnly Fecha, DateOnly Vencimiento, decimal Total, decimal Pendiente, bool Vencida, string Estado);

    public sealed record AlbaranPortalDto(Guid Id, string Numero, DateOnly Fecha, string? Referencia, decimal Base, string Estado);

    public sealed record ResumenClientePortalDto(decimal Pendiente, decimal Vencido, int FacturasPendientes, DateOnly? ProximoVencimiento);

    /// <summary>La sesión del portal de la petición (del claim).</summary>
    internal sealed record Sesion(TipoPortal Tipo, Guid AccesoId, Guid TerceroId);

    public static IEndpointRouteBuilder MapearPortal(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        // Gestión de los accesos (usuarios de la empresa).
        var a = rutas.MapGroup("/accesos-portal").WithTags("Portal");
        a.MapGet("", (IContextoEmpresa c, AccesosPortal s, CancellationToken ct) => ConEmpresa(c, async e => Results.Ok(await s.ListarAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Accesos al portal de agricultores y clientes.").RequierePermiso(Permisos.UsuarioGestionar);
        a.MapPost("", (DatosAccesoPortal d, IContextoEmpresa c, AccesosPortal s, IConsultaClientes clientes, MaestrosAgro maestros, CancellationToken ct) =>
                ConEmpresa(c, async e =>
                {
                    string? nombre = d.Tipo switch
                    {
                        TipoPortal.TerminalPlanta => string.IsNullOrWhiteSpace(d.Nombre) ? null : d.Nombre.Trim(),
                        TipoPortal.Cliente => (await clientes.ObtenerAsync(d.TerceroId, ct).ConfigureAwait(false))?.Nombre,
                        TipoPortal.Agricultor =>
                            (await maestros.AgricultoresAsync(e, ct).ConfigureAwait(false)).FirstOrDefault(x => x.Id == d.TerceroId)?.Nombre,
                        _ => null,
                    };
                    if (nombre is null)
                    {
                        return ResultadosHttp.AProblema(Error.NoEncontrado("portal.tercero", d.Tipo == TipoPortal.TerminalPlanta ? "Ponle un nombre al terminal (por ejemplo, «Volcador línea 1»)." : "No existe ese agricultor o cliente."));
                    }

                    var r = await s.CrearAsync(e, d with { Nombre = nombre }, ct).ConfigureAwait(false);
                    return r.EsCorrecto ? Results.Created($"/accesos-portal/{r.Valor.Acceso.Id}", r.Valor) : ResultadosHttp.AProblema(r.Error);
                }))
            .WithSummary("Crea el acceso de un agricultor o un cliente y devuelve su clave (solo esta vez) para el enlace del portal.")
            .RequierePermiso(Permisos.UsuarioGestionar);
        a.MapPost("/{id:guid}/regenerar", async (Guid id, AccesosPortal s, CancellationToken ct) => (await s.RegenerarAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Nueva clave del acceso (la anterior deja de valer).").RequierePermiso(Permisos.UsuarioGestionar);
        a.MapPost("/{id:guid}/revocar", async (Guid id, AccesosPortal s, CancellationToken ct) => (await s.RevocarAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Revoca el acceso: el enlace y las sesiones abiertas dejan de valer.").RequierePermiso(Permisos.UsuarioGestionar);

        var g = rutas.MapGroup("/portal").WithTags("Portal");
        g.MapPost("/entrar", async (PeticionEntrar p, IContextoEmpresaMutable contexto, IRepositorioEmpresas empresas, AccesosPortal accesos, IProveedorTokens tokens,
                IConsultaEmpresas consultaEmpresas, CancellationToken ct) =>
            {
                if (AccesosPortal.Partes(p.Clave) is not { } partes)
                {
                    return ResultadosHttp.AProblema(Error.NoAutenticado("portal.clave", "El enlace no es válido, ha caducado o se ha revocado."));
                }

                contexto.Fijar(partes.Empresa);
                if (await empresas.ObtenerGrupoIdAsync(partes.Empresa, ct).ConfigureAwait(false) is not { } grupo)
                {
                    return ResultadosHttp.AProblema(Error.NoAutenticado("portal.clave", "El enlace no es válido, ha caducado o se ha revocado."));
                }

                contexto.FijarGrupo(grupo);
                var acceso = await accesos.EntrarAsync(partes.Acceso, partes.Secreto, ct).ConfigureAwait(false);
                if (acceso.EsFallo)
                {
                    return ResultadosHttp.AProblema(acceso.Error);
                }

                var x = acceso.Valor;
                var token = tokens.GenerarToken(new IdentidadUsuario(x.Id, string.Empty, x.Nombre, false),
                    new AlcanceEmpresa(partes.Empresa, grupo, "portal", [], Extras: new Dictionary<string, string> { [ClaimsAlxor.Portal] = $"{x.Tipo}:{x.Id:N}:{x.TerceroId:N}" }));
                var empresa = await consultaEmpresas.ObtenerAsync(partes.Empresa, ct).ConfigureAwait(false);
                return Results.Ok(new SesionPortalDto(token.Token, token.ExpiraEn, x.Tipo.ToString(), x.Nombre, empresa?.RazonSocial ?? string.Empty));
            })
            .WithSummary("Canjea la clave del enlace del portal por una sesión (solo para las rutas /portal).").AllowAnonymous()
            .RequireRateLimiting(OpcionesSeguridad.PoliticaAuth);

        g.MapGet("/yo", (ClaimsPrincipal u, IContextoEmpresa c, AccesosPortal accesos, IConsultaEmpresas empresas, CancellationToken ct) =>
                ConSesion(u, accesos, null, ct: ct, accion: async (s, acceso) =>
                    Results.Ok(new YoPortalDto(s.Tipo.ToString(), acceso.Nombre, (await empresas.ObtenerAsync(c.EmpresaId!.Value, ct).ConfigureAwait(false))?.RazonSocial ?? string.Empty))))
            .WithSummary("Quién es la sesión del portal.").RequireAuthorization();

        // Agricultor.
        g.MapGet("/agricultor/resumen", (Guid? campanaId, ClaimsPrincipal u, IContextoEmpresa c, AccesosPortal accesos, RecepcionesAgro recepciones, LiquidacionesAgro liquidaciones,
                CancellationToken ct) =>
                ConSesion(u, accesos, TipoPortal.Agricultor, ct: ct, accion: async (s, _) =>
                {
                    var entregas = (await recepciones.ListarAsync(c.EmpresaId!.Value, null, null, s.TerceroId, ct).ConfigureAwait(false)).Where(r => r.Estado == "Confirmada").ToList();
                    var liqs = (await liquidaciones.ListarAsync(c.EmpresaId!.Value, s.TerceroId, ct).ConfigureAwait(false)).Where(l => l.Estado == "Emitida").ToList();
                    var kilos = entregas.Sum(r => r.NetoKg);
                    var liquidados = liqs.Sum(l => l.Kilos);
                    return Results.Ok(new ResumenAgricultorDto(kilos, entregas.Count, liquidados, Math.Max(0m, kilos - liquidados), Redondeo.Dos(liqs.Sum(l => l.APagar)),
                        entregas.Count == 0 ? null : entregas.Max(r => r.Fecha)));
                }))
            .WithSummary("Kilos entregados, liquidados y pendientes, e importe liquidado.").RequireAuthorization();
        g.MapGet("/agricultor/entregas", (DateOnly? desde, DateOnly? hasta, ClaimsPrincipal u, IContextoEmpresa c, AccesosPortal accesos, RecepcionesAgro recepciones,
                CancellationToken ct) =>
                ConSesion(u, accesos, TipoPortal.Agricultor, ct: ct, accion: async (s, _) =>
                {
                    var lista = new List<EntregaPortalDto>();
                    foreach (var r in (await recepciones.ListarAsync(c.EmpresaId!.Value, desde, hasta, s.TerceroId, ct).ConfigureAwait(false))
                                 .Where(r => r.Estado == "Confirmada").OrderByDescending(r => r.Fecha).Take(200))
                    {
                        var d = await recepciones.ObtenerAsync(r.Id, ct).ConfigureAwait(false);
                        lista.Add(new EntregaPortalDto(r.Id, r.Numero, r.Fecha, r.NetoKg,
                            (d?.Lineas ?? []).Select(l => new LineaEntregaPortalDto(l.ProductoNombre, l.NetoKg, l.Envases, l.Calibre, null)).ToList()));
                    }

                    return Results.Ok(lista);
                }))
            .WithSummary("Entregas (recepciones confirmadas) del agricultor con sus kilos por producto.").RequireAuthorization();
        g.MapGet("/agricultor/liquidaciones", (ClaimsPrincipal u, IContextoEmpresa c, AccesosPortal accesos, LiquidacionesAgro liquidaciones, CancellationToken ct) =>
                ConSesion(u, accesos, TipoPortal.Agricultor, ct: ct, accion: async (s, _) =>
                    Results.Ok((await liquidaciones.ListarAsync(c.EmpresaId!.Value, s.TerceroId, ct).ConfigureAwait(false)).Where(l => l.Estado == "Emitida")
                        .OrderByDescending(l => l.Fecha).Select(l => new LiquidacionPortalDto(l.Id, l.Numero, l.Fecha, l.Desde, l.Hasta, l.Kilos, l.BaseImponible, l.APagar)).ToList())))
            .WithSummary("Liquidaciones emitidas al agricultor.").RequireAuthorization();
        g.MapGet("/agricultor/liquidaciones/{id:guid}/pdf", (Guid id, ClaimsPrincipal u, IContextoEmpresa c, AccesosPortal accesos, LiquidacionesAgro liquidaciones,
                ImpresosComerciales impresos, CancellationToken ct) =>
                ConSesion(u, accesos, TipoPortal.Agricultor, ct: ct, accion: async (s, _) =>
                {
                    var l = await liquidaciones.ObtenerAsync(id, ct).ConfigureAwait(false);
                    if (l is null || l.AgricultorId != s.TerceroId || l.Estado != "Emitida")
                    {
                        return NoEsSuyo();
                    }

                    var pdf = await impresos.LiquidacionAsync(c.EmpresaId!.Value, id, ct).ConfigureAwait(false);
                    return pdf.EsCorrecto ? Results.File(pdf.Valor.Contenido, "application/pdf", pdf.Valor.NombreArchivo) : ResultadosHttp.AProblema(pdf.Error);
                }))
            .WithSummary("PDF de una liquidación del agricultor.").RequireAuthorization();

        // Cliente.
        g.MapGet("/cliente/facturas", (ClaimsPrincipal u, IContextoEmpresa c, AccesosPortal accesos, IConsultaFacturas facturas,
                AlxorCore.Tesoreria.Aplicacion.IConsultaTesoreria tesoreria, AlxorCore.Nucleo.Tiempo.IReloj reloj, CancellationToken ct) =>
                ConSesion(u, accesos, TipoPortal.Cliente, ct: ct, accion: async (s, _) => Results.Ok(await FacturasClienteAsync(c.EmpresaId!.Value, s.TerceroId, facturas, tesoreria, reloj, ct)
                    .ConfigureAwait(false))))
            .WithSummary("Facturas del cliente con lo pendiente de cada una.").RequireAuthorization();
        g.MapGet("/cliente/resumen", (ClaimsPrincipal u, IContextoEmpresa c, AccesosPortal accesos, IConsultaFacturas facturas,
                AlxorCore.Tesoreria.Aplicacion.IConsultaTesoreria tesoreria, AlxorCore.Nucleo.Tiempo.IReloj reloj, CancellationToken ct) =>
                ConSesion(u, accesos, TipoPortal.Cliente, ct: ct, accion: async (s, _) =>
                {
                    var lista = (await FacturasClienteAsync(c.EmpresaId!.Value, s.TerceroId, facturas, tesoreria, reloj, ct).ConfigureAwait(false)).Where(f => f.Pendiente > 0m).ToList();
                    return Results.Ok(new ResumenClientePortalDto(Redondeo.Dos(lista.Sum(f => f.Pendiente)), Redondeo.Dos(lista.Where(f => f.Vencida).Sum(f => f.Pendiente)), lista.Count,
                        lista.Where(f => !f.Vencida).Select(f => (DateOnly?)f.Vencimiento).Min()));
                }))
            .WithSummary("Saldo del cliente: pendiente, vencido y próximo vencimiento.").RequireAuthorization();
        g.MapGet("/cliente/facturas/{id:guid}/pdf", (Guid id, ClaimsPrincipal u, IContextoEmpresa c, AccesosPortal accesos, IConsultaFacturas facturas,
                AlxorCore.Documentos.Aplicacion.GenerarPdfFactura pdfs, CancellationToken ct) =>
                ConSesion(u, accesos, TipoPortal.Cliente, ct: ct, accion: async (s, _) =>
                {
                    var f = await facturas.ObtenerAsync(id, ct).ConfigureAwait(false);
                    if (f is null || f.ClienteId != s.TerceroId)
                    {
                        return NoEsSuyo();
                    }

                    var pdf = await pdfs.EjecutarAsync(c.EmpresaId!.Value, id, ct).ConfigureAwait(false);
                    return pdf.EsCorrecto ? Results.File(pdf.Valor.Contenido, "application/pdf", pdf.Valor.NombreArchivo) : ResultadosHttp.AProblema(pdf.Error);
                }))
            .WithSummary("PDF de una factura del cliente.").RequireAuthorization();
        g.MapGet("/cliente/albaranes", (ClaimsPrincipal u, IContextoEmpresa c, AccesosPortal accesos, ConsultarAlbaranesVenta albaranes, CancellationToken ct) =>
                ConSesion(u, accesos, TipoPortal.Cliente, ct: ct, accion: async (s, _) =>
                    Results.Ok((await albaranes.ListarAsync(c.EmpresaId!.Value, new FiltroAlbaranesVenta(s.TerceroId), ct).ConfigureAwait(false)).Where(x => !x.Anulado).Take(500)
                        .Select(x => new AlbaranPortalDto(x.Id, x.NumeroCompleto, x.Fecha, x.Referencia, x.Base, x.Estado)).ToList())))
            .WithSummary("Albaranes del cliente.").RequireAuthorization();
        g.MapGet("/cliente/albaranes/{id:guid}/pdf", (Guid id, ClaimsPrincipal u, IContextoEmpresa c, AccesosPortal accesos, ConsultarAlbaranesVenta albaranes,
                ImpresosComerciales impresos, CancellationToken ct) =>
                ConSesion(u, accesos, TipoPortal.Cliente, ct: ct, accion: async (s, _) =>
                {
                    var x = await albaranes.ObtenerAsync(id, ct).ConfigureAwait(false);
                    if (x is null || x.ClienteId != s.TerceroId || x.Anulado)
                    {
                        return NoEsSuyo();
                    }

                    var pdf = await impresos.AlbaranAsync(c.EmpresaId!.Value, id, true, ct).ConfigureAwait(false);
                    return pdf.EsCorrecto ? Results.File(pdf.Valor.Contenido, "application/pdf", pdf.Valor.NombreArchivo) : ResultadosHttp.AProblema(pdf.Error);
                }))
            .WithSummary("PDF de un albarán del cliente.").RequireAuthorization();
        return rutas;
    }

    private static async Task<IReadOnlyList<FacturaPortalDto>> FacturasClienteAsync(Guid empresaId, Guid clienteId, IConsultaFacturas facturas,
        AlxorCore.Tesoreria.Aplicacion.IConsultaTesoreria tesoreria, AlxorCore.Nucleo.Tiempo.IReloj reloj, CancellationToken ct)
    {
        var hoy = DateOnly.FromDateTime(reloj.AhoraUtc.UtcDateTime);
        var lista = (await facturas.ListarAsync(empresaId, ct).ConfigureAwait(false)).Where(f => f.ClienteId == clienteId && f.Estado != "Anulada")
            .OrderByDescending(f => f.FechaEmision).Take(500).ToList();
        var cobrado = lista.Count == 0 ? new Dictionary<Guid, decimal>()
            : await tesoreria.LiquidadoPorDocumentosAsync(AlxorCore.Tesoreria.Dominio.TipoDocumentoTesoreria.Factura, lista.Select(f => f.Id).ToList(), ct).ConfigureAwait(false);
        return lista.Select(f =>
        {
            var pendiente = Math.Max(0m, Redondeo.Dos(f.Total - cobrado.GetValueOrDefault(f.Id)));
            return new FacturaPortalDto(f.Id, f.NumeroCompleto, f.FechaEmision, f.FechaVencimiento, f.Total, pendiente, pendiente > 0m && f.FechaVencimiento < hoy, f.Estado);
        }).ToList();
    }

    private static IResult NoEsSuyo() => ResultadosHttp.AProblema(Error.NoEncontrado("portal.no_encontrado", "El documento no existe."));

    /// <summary>Comprueba que la sesión es del portal, del tipo pedido y que su acceso sigue vigente (revocado o caducado, se acaba).</summary>
    internal static async Task<IResult> ConSesion(ClaimsPrincipal u, AccesosPortal accesos, TipoPortal? tipo,
        Func<Sesion, AccesoPortal, Task<IResult>> accion, CancellationToken ct)
    {
        var partes = (u.FindFirstValue(ClaimsAlxor.Portal) ?? string.Empty).Split(':');
        if (partes.Length != 3 || !Enum.TryParse<TipoPortal>(partes[0], out var t) || !Guid.TryParseExact(partes[1], "N", out var acceso)
            || !Guid.TryParseExact(partes[2], "N", out var tercero))
        {
            return ResultadosHttp.AProblema(Error.Prohibido("portal.sin_sesion", "Esto es solo para el portal."));
        }

        if (tipo is { } esperado && esperado != t)
        {
            return ResultadosHttp.AProblema(Error.Prohibido("portal.tipo", "Esta parte del portal no es para ti."));
        }

        var vigente = await accesos.VigenteAsync(acceso, ct).ConfigureAwait(false);
        return vigente is null || vigente.TerceroId != tercero
            ? ResultadosHttp.AProblema(Error.NoAutenticado("portal.clave", "El acceso se ha revocado o ha caducado."))
            : await accion(new Sesion(t, acceso, tercero), vigente).ConfigureAwait(false);
    }

    private static async Task<IResult> ConEmpresa(IContextoEmpresa contexto, Func<Guid, Task<IResult>> accion) =>
        contexto.EmpresaId is { } empresa
            ? await accion(empresa).ConfigureAwait(false)
            : ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
}

/// <summary>Una sesión del portal (claim <c>portal</c>) no puede usar ninguna ruta fuera de /portal.</summary>
public sealed class MiddlewarePortal
{
    private readonly RequestDelegate _siguiente;

    public MiddlewarePortal(RequestDelegate siguiente) => _siguiente = siguiente;

    public async Task InvokeAsync(HttpContext contexto)
    {
        ArgumentNullException.ThrowIfNull(contexto);
        var ruta = contexto.Request.Path.Value ?? string.Empty;
        if (contexto.User.HasClaim(c => c.Type == ClaimsAlxor.Portal) && !ruta.StartsWith("/portal/", StringComparison.OrdinalIgnoreCase)
            && !System.IO.Path.HasExtension(ruta))
        {
            await ResultadosHttp.AProblema(Error.Prohibido("portal.solo_portal", "La sesión del portal solo vale para el portal.")).ExecuteAsync(contexto).ConfigureAwait(false);
            return;
        }

        await _siguiente(contexto).ConfigureAwait(false);
    }
}
