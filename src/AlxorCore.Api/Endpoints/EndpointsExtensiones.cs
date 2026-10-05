using System.Security.Claims;
using AlxorCore.Api.Comun;
using AlxorCore.Extensiones.Aplicacion;
using AlxorCore.Extensiones.Dominio;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Seguridad;

namespace AlxorCore.Api.Endpoints;

/// <summary>
/// Extensiones de la base: campos personalizados en cualquier registro, adjuntos y alertas configurables. Los valores
/// y los adjuntos de un registro se ven y se cambian con los permisos de ese registro.
/// </summary>
public static class EndpointsExtensiones
{
    public sealed record PeticionContarAdjuntos(IReadOnlyList<Guid>? Ids);

    public static IEndpointRouteBuilder MapearExtensiones(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var g = rutas.MapGroup("/extensiones").WithTags("Extensiones");

        // Campos personalizados.
        g.MapGet("/entidades", () => Results.Ok(CamposPersonalizados.Entidades()))
            .WithSummary("Registros que admiten campos personalizados y adjuntos.").RequireAuthorization();
        g.MapGet("/campos", (string? entidad, IContextoEmpresa c, CamposPersonalizados s, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await s.CamposAsync(e, entidad, ct).ConfigureAwait(false))))
            .WithSummary("Campos personalizados definidos (de un tipo de registro o de todos).").RequireAuthorization();
        g.MapPost("/campos", (DatosCampo d, IContextoEmpresa c, CamposPersonalizados s, CancellationToken ct) =>
                ConEmpresa(c, async e =>
                {
                    var r = await s.CrearAsync(e, d, ct).ConfigureAwait(false);
                    return r.EsCorrecto ? Results.Created($"/extensiones/campos/{r.Valor.Id}", r.Valor) : ResultadosHttp.AProblema(r.Error);
                }))
            .WithSummary("Define un campo: registro, código, tipo (texto, número, fecha, sí/no, lista), obligatorio, defecto y orden.").RequierePermiso(Permisos.EmpresaAjustes);
        g.MapPut("/campos/{id:guid}", async (Guid id, DatosCampo d, CamposPersonalizados s, CancellationToken ct) => (await s.CambiarAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Cambia etiqueta, obligatorio, defecto, opciones, orden, ayuda o si está activo.").RequierePermiso(Permisos.EmpresaAjustes);
        g.MapDelete("/campos/{id:guid}", async (Guid id, CamposPersonalizados s, CancellationToken ct) => (await s.EliminarAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Borra un campo sin valores (con valores, se desactiva).").RequierePermiso(Permisos.EmpresaAjustes);
        g.MapGet("/valores/{entidad}/{id:guid}", (string entidad, Guid id, ClaimsPrincipal u, IContextoEmpresa c, CamposPersonalizados s, CancellationToken ct) =>
                ConPermiso(u, entidad, false, () => ConEmpresa(c, async e => (await s.ValoresAsync(e, entidad, id, ct).ConfigureAwait(false)).AOk())))
            .WithSummary("Campos personalizados de un registro con sus valores.").RequireAuthorization();
        g.MapPut("/valores/{entidad}/{id:guid}", (string entidad, Guid id, DatosValores d, ClaimsPrincipal u, IContextoEmpresa c, CamposPersonalizados s,
                CancellationToken ct) =>
                ConPermiso(u, entidad, true, () => ConEmpresa(c, async e => (await s.GuardarAsync(e, entidad, id, d, ct).ConfigureAwait(false)).AOk())))
            .WithSummary("Guarda los valores (por código de campo) de un registro; vacío lo quita.").RequireAuthorization();
        g.MapGet("/buscar/{entidad}", (string entidad, string codigo, string? valor, string? desde, string? hasta, ClaimsPrincipal u, IContextoEmpresa c,
                CamposPersonalizados s, CancellationToken ct) =>
                ConPermiso(u, entidad, false, () => ConEmpresa(c, async e => (await s.BuscarAsync(e, entidad, codigo, valor, desde, hasta, ct).ConfigureAwait(false)).AOk())))
            .WithSummary("Registros con un valor en un campo (texto que contiene; número y fecha también por rango).").RequireAuthorization();

        // Adjuntos.
        g.MapGet("/adjuntos/{entidad}/{id:guid}", (string entidad, Guid id, ClaimsPrincipal u, AdjuntosRegistros s, CancellationToken ct) =>
                ConPermiso(u, entidad, false, async () => Results.Ok(await s.ListarAsync(entidad, id, ct).ConfigureAwait(false))))
            .WithSummary("Ficheros adjuntos de un registro.").RequireAuthorization();
        g.MapPost("/adjuntos/{entidad}/contar", (string entidad, PeticionContarAdjuntos p, ClaimsPrincipal u, AdjuntosRegistros s, CancellationToken ct) =>
                ConPermiso(u, entidad, false, async () => Results.Ok(await s.ContarAsync(entidad, p.Ids ?? [], ct).ConfigureAwait(false))))
            .WithSummary("Cuántos adjuntos tiene cada registro de una lista.").RequireAuthorization();
        g.MapPost("/adjuntos/{entidad}/{id:guid}", (string entidad, Guid id, DatosAdjunto d, ClaimsPrincipal u, IContextoEmpresa c, AdjuntosRegistros s,
                CancellationToken ct) =>
                ConPermiso(u, entidad, true, () => ConEmpresa(c, async e =>
                {
                    var quien = u.ObtenerIdentidad();
                    var r = await s.SubirAsync(e, entidad, id, d, quien?.Id, string.IsNullOrWhiteSpace(quien?.Nombre) ? quien?.Email : quien.Nombre, ct).ConfigureAwait(false);
                    return r.EsCorrecto ? Results.Created($"/extensiones/adjuntos/{r.Valor.Id}/contenido", r.Valor) : ResultadosHttp.AProblema(r.Error);
                })))
            .WithSummary("Adjunta un fichero (nombre y contenido en base64, hasta 10 MB) a un registro.").RequireAuthorization();
        g.MapGet("/adjuntos/{id:guid}/contenido", async (Guid id, ClaimsPrincipal u, AdjuntosRegistros s, CancellationToken ct) =>
            {
                var a = await s.ObtenerAsync(id, ct).ConfigureAwait(false);
                if (a is null)
                {
                    return ResultadosHttp.AProblema(Error.NoEncontrado("adjunto.no_encontrado", "El adjunto no existe."));
                }

                return await ConPermiso(u, a.Entidad, false, () => Task.FromResult(Results.File(a.Contenido.Datos, a.TipoMime, a.Nombre))).ConfigureAwait(false);
            })
            .WithSummary("Descarga un adjunto.").RequireAuthorization();
        g.MapDelete("/adjuntos/{id:guid}", async (Guid id, ClaimsPrincipal u, AdjuntosRegistros s, CancellationToken ct) =>
            {
                var a = await s.ObtenerAsync(id, ct).ConfigureAwait(false);
                if (a is null)
                {
                    return ResultadosHttp.AProblema(Error.NoEncontrado("adjunto.no_encontrado", "El adjunto no existe."));
                }

                return await ConPermiso(u, a.Entidad, true, async () => (await s.EliminarAsync(id, ct).ConfigureAwait(false)).ASinContenido()).ConfigureAwait(false);
            })
            .WithSummary("Quita un adjunto.").RequireAuthorization();

        // Alertas.
        g.MapGet("/reglas/eventos", () => Results.Ok(ReglaAlerta.Eventos.Select(e => new { Evento = e.Key, Descripcion = e.Value })))
            .WithSummary("Eventos que pueden disparar una alerta.").RequierePermiso(Permisos.EmpresaAjustes);
        g.MapGet("/reglas", (IContextoEmpresa c, AlertasEmpresa s, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await s.ReglasAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Reglas de alerta con sus alertas vivas.").RequierePermiso(Permisos.EmpresaAjustes);
        g.MapPost("/reglas", (DatosRegla d, IContextoEmpresa c, AlertasEmpresa s, CancellationToken ct) =>
                ConEmpresa(c, async e =>
                {
                    var r = await s.CrearReglaAsync(e, d, ct).ConfigureAwait(false);
                    return r.EsCorrecto ? Results.Created($"/extensiones/reglas/{r.Valor.Id}", r.Valor) : ResultadosHttp.AProblema(r.Error);
                }))
            .WithSummary("Regla: facturas vencidas, riesgo superado, fecha de un campo, caducidad del certificado o un evento; y quién la ve.")
            .RequierePermiso(Permisos.EmpresaAjustes);
        g.MapPut("/reglas/{id:guid}", (Guid id, DatosRegla d, IContextoEmpresa c, AlertasEmpresa s, CancellationToken ct) =>
                ConEmpresa(c, async e => (await s.CambiarReglaAsync(e, id, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Cambia el nombre, el umbral, el destino o si está activa.").RequierePermiso(Permisos.EmpresaAjustes);
        g.MapDelete("/reglas/{id:guid}", async (Guid id, AlertasEmpresa s, CancellationToken ct) => (await s.EliminarReglaAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Borra una regla que no ha dado alertas (si las ha dado, se desactiva).").RequierePermiso(Permisos.EmpresaAjustes);
        g.MapPost("/alertas/evaluar", (IContextoEmpresa c, AlertasEmpresa s, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await s.EvaluarAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Evalúa ahora las reglas: crea las alertas nuevas y resuelve las que ya no se dan.").RequierePermiso(Permisos.EmpresaAjustes);
        g.MapGet("/alertas", (bool? incluirResueltas, ClaimsPrincipal u, IContextoEmpresa c, AlertasEmpresa s, CancellationToken ct) =>
                ConEmpresa(c, async e => u.ObtenerUsuarioId() is { } usuario
                    ? Results.Ok(await s.ParaUsuarioAsync(e, usuario, PermisosDe(u), incluirResueltas ?? false, ct).ConfigureAwait(false))
                    : Results.Unauthorized()))
            .WithSummary("Alertas que ve el usuario (las de sus permisos), con cuántas quedan sin leer.").RequireAuthorization();
        g.MapPost("/alertas/{id:guid}/leida", async (Guid id, ClaimsPrincipal u, AlertasEmpresa s, CancellationToken ct) =>
                u.ObtenerUsuarioId() is { } usuario ? (await s.MarcarLeidaAsync(id, usuario, PermisosDe(u), ct).ConfigureAwait(false)).ASinContenido() : Results.Unauthorized())
            .WithSummary("Marca la alerta como leída por el usuario.").RequireAuthorization();
        g.MapPost("/alertas/{id:guid}/resolver", async (Guid id, ClaimsPrincipal u, AlertasEmpresa s, CancellationToken ct) =>
            {
                var quien = u.ObtenerIdentidad();
                return (await s.ResolverAsync(id, string.IsNullOrWhiteSpace(quien?.Nombre) ? quien?.Email : quien.Nombre, PermisosDe(u), ct).ConfigureAwait(false)).ASinContenido();
            })
            .WithSummary("Da la alerta por resuelta.").RequireAuthorization();
        return rutas;
    }

    private static HashSet<string> PermisosDe(ClaimsPrincipal u) => u.FindAll(ClaimsAlxor.Permiso).Select(c => c.Value).ToHashSet(StringComparer.Ordinal);

    /// <summary>Exige el permiso de leer (o de cambiar) el tipo de registro.</summary>
    private static async Task<IResult> ConPermiso(ClaimsPrincipal u, string entidad, bool editar, Func<Task<IResult>> accion)
    {
        var e = EntidadesExtensibles.Buscar(entidad);
        if (e is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("extensiones.entidad", "Ese tipo de registro no admite campos personalizados ni adjuntos."));
        }

        return u.HasClaim(ClaimsAlxor.Permiso, editar ? e.PermisoEditar : e.PermisoLeer)
            ? await accion().ConfigureAwait(false)
            : ResultadosHttp.AProblema(Error.Prohibido("extensiones.sin_permiso", $"No tienes permiso para {(editar ? "cambiar" : "ver")} {e.Nombre.ToLowerInvariant()}."));
    }

    private static async Task<IResult> ConEmpresa(IContextoEmpresa contexto, Func<Guid, Task<IResult>> accion) =>
        contexto.EmpresaId is { } empresa
            ? await accion(empresa).ConfigureAwait(false)
            : ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
}
