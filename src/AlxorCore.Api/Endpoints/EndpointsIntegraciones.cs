using AlxorCore.Api.Comun;
using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Integraciones.Aplicacion;
using AlxorCore.Integraciones.Dominio;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Api.Endpoints;

/// <summary>Endpoints del módulo Integraciones: gestión de claves de API y webhooks, y la API pública versionada.</summary>
public static class EndpointsIntegraciones
{
    public static IEndpointRouteBuilder MapearIntegraciones(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        MapearGestion(rutas);
        MapearApiPublica(rutas);
        return rutas;
    }

    // ------------------------------------------------------------------
    // Gestión (interfaz, autenticada por JWT + permiso integracion.gestionar)
    // ------------------------------------------------------------------
    private static void MapearGestion(IEndpointRouteBuilder rutas)
    {
        var g = rutas.MapGroup("/integraciones").WithTags("Integraciones");

        g.MapGet("/eventos", () => Results.Ok(EventosIntegracion.Todos.OrderBy(e => e, StringComparer.Ordinal)))
            .WithSummary("Lista los eventos disponibles para suscribir webhooks.")
            .RequierePermiso(Permisos.IntegracionGestionar);

        g.MapGet("/claves", async (IContextoEmpresa contexto, ListarClavesApi caso, CancellationToken ct) =>
            contexto.EmpresaId is null
                ? ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."))
                : Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, ct).ConfigureAwait(false)))
            .WithSummary("Lista las claves de API de la empresa.")
            .RequierePermiso(Permisos.IntegracionGestionar);

        g.MapPost("/claves", async (PeticionClave peticion, IContextoEmpresa contexto, CrearClaveApi caso, CancellationToken ct) =>
        {
            if (contexto.EmpresaId is null)
            {
                return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
            }

            var r = await caso.EjecutarAsync(contexto.EmpresaId.Value, peticion.Nombre, ct).ConfigureAwait(false);
            return r.ACreado($"/integraciones/claves/{(r.EsCorrecto ? r.Valor.Id : Guid.Empty)}");
        })
            .WithSummary("Crea una clave de API (el secreto se devuelve una única vez).")
            .RequierePermiso(Permisos.IntegracionGestionar);

        g.MapDelete("/claves/{id:guid}", async (Guid id, RevocarClaveApi caso, CancellationToken ct) =>
            (await caso.EjecutarAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Revoca una clave de API.")
            .RequierePermiso(Permisos.IntegracionGestionar);

        g.MapGet("/webhooks", async (IContextoEmpresa contexto, ListarSuscripcionesWebhook caso, CancellationToken ct) =>
            contexto.EmpresaId is null
                ? ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."))
                : Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, ct).ConfigureAwait(false)))
            .WithSummary("Lista las suscripciones de webhook.")
            .RequierePermiso(Permisos.IntegracionGestionar);

        g.MapPost("/webhooks", async (PeticionWebhook peticion, IContextoEmpresa contexto, CrearSuscripcionWebhook caso, CancellationToken ct) =>
        {
            if (contexto.EmpresaId is null)
            {
                return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
            }

            var r = await caso.EjecutarAsync(contexto.EmpresaId.Value, peticion.Url, peticion.Eventos, ct).ConfigureAwait(false);
            return r.ACreado($"/integraciones/webhooks/{(r.EsCorrecto ? r.Valor.Id : Guid.Empty)}");
        })
            .WithSummary("Crea una suscripción de webhook (el secreto de firma se devuelve una única vez).")
            .RequierePermiso(Permisos.IntegracionGestionar);

        g.MapDelete("/webhooks/{id:guid}", async (Guid id, EliminarSuscripcionWebhook caso, CancellationToken ct) =>
            (await caso.EjecutarAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Elimina (desactiva) una suscripción de webhook.")
            .RequierePermiso(Permisos.IntegracionGestionar);

        g.MapGet("/webhooks/entregas", async (IContextoEmpresa contexto, ListarEntregasWebhook caso, CancellationToken ct) =>
            contexto.EmpresaId is null
                ? ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."))
                : Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, ct).ConfigureAwait(false)))
            .WithSummary("Lista las últimas entregas de webhook (buzón de salida) de la empresa.")
            .RequierePermiso(Permisos.IntegracionGestionar);

        g.MapPost("/webhooks/procesar", async (IContextoEmpresa contexto, ProcesarEntregasWebhook caso, CancellationToken ct) =>
        {
            if (contexto.EmpresaId is null)
            {
                return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
            }

            var entregadas = await caso.EjecutarAsync(contexto.EmpresaId.Value, ct).ConfigureAwait(false);
            return Results.Ok(new { entregadas });
        })
            .WithSummary("Fuerza el envío inmediato de las entregas de webhook pendientes de la empresa.")
            .RequierePermiso(Permisos.IntegracionGestionar);
    }

    // ------------------------------------------------------------------
    // API pública versionada (autenticada por clave de API en la cabecera X-Api-Key)
    // ------------------------------------------------------------------
    private static void MapearApiPublica(IEndpointRouteBuilder rutas)
    {
        var api = rutas.MapGroup("/api/v1").WithTags("API pública v1").AddEndpointFilter<FiltroClaveApi>();

        api.MapGet("/ping", (IContextoEmpresa contexto) =>
            Results.Ok(new { empresaId = contexto.EmpresaId, version = "v1" }))
            .WithSummary("Comprueba la clave de API y devuelve la empresa asociada.");

        api.MapGet("/facturas", async (IContextoEmpresa contexto, IConsultaFacturas facturas, CancellationToken ct) =>
            Results.Ok(await facturas.ListarAsync(contexto.EmpresaRequerida, ct).ConfigureAwait(false)))
            .WithSummary("Lista las facturas de la empresa (API pública).");

        api.MapGet("/clientes", async (IContextoEmpresa contexto, IConsultaClientes clientes, CancellationToken ct) =>
            Results.Ok(await clientes.ListarAsync(contexto.GrupoRequerido, false, null, ct).ConfigureAwait(false)))
            .WithSummary("Lista los clientes del grupo (API pública).");

        api.MapGet("/productos", async (IContextoEmpresa contexto, IConsultaProductos productos, CancellationToken ct) =>
            Results.Ok(await productos.ListarAsync(contexto.GrupoRequerido, false, null, ct).ConfigureAwait(false)))
            .WithSummary("Lista los productos de la empresa (API pública).");
    }

    /// <summary>Cuerpo para crear una clave de API.</summary>
    public sealed record PeticionClave(string? Nombre);

    /// <summary>Cuerpo para crear una suscripción de webhook.</summary>
    public sealed record PeticionWebhook(string? Url, string[]? Eventos);

    /// <summary>
    /// Filtro de la API pública: exige la cabecera <c>X-Api-Key</c>, valida la clave y fija la empresa
    /// activa del ámbito de la petición (para el filtro multiempresa y la RLS). Sin clave válida, 401.
    /// </summary>
    public sealed class FiltroClaveApi : IEndpointFilter
    {
        private const string Cabecera = "X-Api-Key";

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(next);

            var http = context.HttpContext;
            var clave = http.Request.Headers[Cabecera].ToString();
            if (string.IsNullOrWhiteSpace(clave))
            {
                return NoAutorizado("Falta la cabecera X-Api-Key.");
            }

            var autenticar = http.RequestServices.GetRequiredService<AutenticarClaveApi>();
            var empresaId = await autenticar.EjecutarAsync(clave, http.RequestAborted).ConfigureAwait(false);
            if (empresaId is null)
            {
                return NoAutorizado("Clave de API no válida o revocada.");
            }

            var contexto = http.RequestServices.GetRequiredService<IContextoEmpresaMutable>();
            contexto.Fijar(empresaId.Value);
            // Los maestros son del grupo: fijamos también el grupo de la empresa para la API pública.
            var grupoId = await http.RequestServices
                .GetRequiredService<AlxorCore.Organizacion.Aplicacion.Puertos.IRepositorioEmpresas>()
                .ObtenerGrupoIdAsync(empresaId.Value, http.RequestAborted).ConfigureAwait(false);
            if (grupoId is { } g)
            {
                contexto.FijarGrupo(g);
            }

            return await next(context).ConfigureAwait(false);
        }

        private static IResult NoAutorizado(string detalle) =>
            Results.Problem(title: detalle, statusCode: StatusCodes.Status401Unauthorized);
    }
}
