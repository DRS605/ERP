using AlxorCore.Analisis.Aplicacion;
using AlxorCore.Api.Comun;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Modulos;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Seguridad;

namespace AlxorCore.Api.Endpoints;

/// <summary>Petición del detalle (profundizar en una cifra del análisis).</summary>
public sealed record DetalleAnalisisPeticion(string Dataset, IReadOnlyList<FiltroAnalisis>? Filtros, DateOnly? Desde, DateOnly? Hasta, int? Limite);

/// <summary>
/// Conjuntos del análisis que ve el usuario: los de los módulos contratados y con el permiso de lectura de lo que
/// contienen (la contabilidad, con permiso de contabilidad; las compras, con el de gastos…).
/// </summary>
public sealed class PermisosAnalisisHttp : IPermisosAnalisis
{
    private static readonly Dictionary<string, string> PermisoDataset = new(StringComparer.Ordinal)
    {
        ["ventas"] = Permisos.FacturaLeer,
        ["pedidos"] = Permisos.FacturaLeer,
        ["deuda"] = Permisos.FacturaLeer,
        ["tesoreria"] = Permisos.FacturaLeer,
        ["compras"] = Permisos.GastoLeer,
        ["contabilidad"] = Permisos.ContabilidadLeer,
        ["almacen"] = Permisos.InventarioLeer,
        ["agro"] = Permisos.AgroLeer,
    };

    private readonly IHttpContextAccessor _http;

    public PermisosAnalisisHttp(IHttpContextAccessor http) => _http = http;

    public bool Puede(DatasetAnalisis dataset)
    {
        ArgumentNullException.ThrowIfNull(dataset);
        var usuario = _http.HttpContext?.User;
        if (usuario is null)
        {
            return false;
        }

        var edicion = usuario.FindFirst(ClaimsAlxor.Edicion)?.Value;
        if (dataset.Modulo is { } modulo && edicion is not null && !usuario.HasClaim(ClaimsAlxor.Modulo, modulo))
        {
            return false;
        }

        return !PermisoDataset.TryGetValue(dataset.Clave, out var permiso) || usuario.HasClaim(ClaimsAlxor.Permiso, permiso);
    }
}

/// <summary>
/// Análisis de datos: consultas libres (dimensiones × medidas, tabla dinámica, comparación con el periodo anterior,
/// los N primeros, filtros), el detalle de cada cifra, los valores para filtrar, la galería de informes y los
/// informes guardados.
/// </summary>
public static class EndpointsAnalisis
{
    public static IEndpointRouteBuilder MapearAnalisis(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var g = rutas.MapGroup("/analisis").WithTags("Análisis");

        g.MapGet("/catalogo", (MotorAnalisis motor) =>
            {
                var datasets = motor.Catalogo();
                var visibles = datasets.Select(d => d.Clave).ToHashSet(StringComparer.Ordinal);
                return Results.Ok(new { datasets, plantillas = PlantillasAnalisis.Todas.Where(p => visibles.Contains(p.Consulta.Dataset)) });
            })
            .WithSummary("Conjuntos de datos (con sus dimensiones y medidas) y la galería de informes predefinidos.")
            .RequierePermiso(Permisos.InformeLeer);

        g.MapPost("/consulta", async (ConsultaAnalisis consulta, MotorAnalisis motor, CancellationToken ct) =>
                (await motor.ConsultarAsync(consulta, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Ejecuta un análisis: filas con subtotales por nivel, columnas (tabla dinámica), comparación y N primeros.")
            .RequierePermiso(Permisos.InformeLeer);

        g.MapGet("/{dataset}/valores", async (string dataset, string dimension, string? texto, DateOnly? desde, DateOnly? hasta, MotorAnalisis motor, CancellationToken ct) =>
                (await motor.ValoresAsync(dataset, dimension, texto, desde, hasta, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Valores de una dimensión (para los filtros), los más frecuentes primero.")
            .RequierePermiso(Permisos.InformeLeer);

        g.MapPost("/detalle", async (DetalleAnalisisPeticion p, MotorAnalisis motor, CancellationToken ct) =>
                (await motor.DetalleAsync(p.Dataset, p.Filtros, p.Desde, p.Hasta, p.Limite ?? 500, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Registros que forman una cifra (profundizar), con el enlace al documento.")
            .RequierePermiso(Permisos.InformeLeer);

        var informes = g.MapGroup("/informes");
        informes.MapGet("", async (HttpContext http, IContextoEmpresa contexto, GestionInformesAnalisis caso, CancellationToken ct) =>
                contexto.EmpresaId is { } empresaId && http.User.ObtenerUsuarioId() is { } usuarioId
                    ? Results.Ok(await caso.ListarAsync(empresaId, usuarioId, ct).ConfigureAwait(false))
                    : ResultadosHttp.AProblema(SinEmpresa()))
            .WithSummary("Informes guardados: los propios y los compartidos en la empresa.")
            .RequierePermiso(Permisos.InformeLeer);
        informes.MapPost("", async (GuardarInformeComando comando, HttpContext http, IContextoEmpresa contexto, GestionInformesAnalisis caso, CancellationToken ct) =>
            {
                if (contexto.EmpresaId is not { } empresaId || http.User.ObtenerUsuarioId() is not { } usuarioId)
                {
                    return ResultadosHttp.AProblema(SinEmpresa());
                }

                var r = await caso.CrearAsync(empresaId, usuarioId, comando, ct).ConfigureAwait(false);
                return r.EsCorrecto ? r.ACreado($"/analisis/informes/{r.Valor.Id}") : ResultadosHttp.AProblema(r.Error);
            })
            .WithSummary("Guarda un informe (definición del diseñador); compartido lo ve toda la empresa.")
            .RequierePermiso(Permisos.InformeLeer);
        informes.MapPut("/{id:guid}", async (Guid id, GuardarInformeComando comando, HttpContext http, GestionInformesAnalisis caso, CancellationToken ct) =>
                http.User.ObtenerUsuarioId() is { } usuarioId
                    ? (await caso.ActualizarAsync(id, usuarioId, comando, ct).ConfigureAwait(false)).AOk()
                    : ResultadosHttp.AProblema(SinEmpresa()))
            .WithSummary("Cambia un informe propio.")
            .RequierePermiso(Permisos.InformeLeer);
        informes.MapDelete("/{id:guid}", async (Guid id, HttpContext http, GestionInformesAnalisis caso, CancellationToken ct) =>
            {
                if (http.User.ObtenerUsuarioId() is not { } usuarioId)
                {
                    return ResultadosHttp.AProblema(SinEmpresa());
                }

                var r = await caso.EliminarAsync(id, usuarioId, ct).ConfigureAwait(false);
                return r.EsCorrecto ? Results.NoContent() : ResultadosHttp.AProblema(r.Error);
            })
            .WithSummary("Borra un informe propio.")
            .RequierePermiso(Permisos.InformeLeer);

        return rutas;
    }

    private static Error SinEmpresa() => Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero.");
}
