using AlxorCore.Api.Comun;
using AlxorCore.Documentos.Aplicacion;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Api.Endpoints;

/// <summary>Endpoints REST de las cartas de porte (documento de control del transporte de mercancías).</summary>
public static class EndpointsCartasPorte
{
    public static IEndpointRouteBuilder MapearCartasPorte(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        var g = rutas.MapGroup("/cartas-porte").WithTags("Cartas de porte");

        g.MapGet("", ListarAsync)
            .WithSummary("Lista las cartas de porte de la empresa activa.")
            .RequierePermiso(Permisos.FacturaLeer);

        g.MapGet("/{id:guid}", ObtenerAsync)
            .WithSummary("Obtiene una carta de porte.")
            .RequierePermiso(Permisos.FacturaLeer);

        g.MapGet("/{id:guid}/pdf", PdfAsync)
            .WithSummary("Genera el PDF de la carta de porte.")
            .RequierePermiso(Permisos.FacturaLeer);

        g.MapPost("", CrearAsync)
            .WithSummary("Crea una carta de porte (remitente = empresa; destinatario = cliente o datos libres).")
            .RequierePermiso(Permisos.FacturaCrear);

        return rutas;
    }

    private static async Task<IResult> ListarAsync(IContextoEmpresa contexto, ListarCartasPorte caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> ObtenerAsync(Guid id, ObtenerCartaPorte caso, CancellationToken ct) =>
        (await caso.EjecutarAsync(id, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> CrearAsync(CrearCartaPorteComando comando, IContextoEmpresa contexto, CrearCartaPorte caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var r = await caso.EjecutarAsync(contexto.EmpresaId.Value, comando, ct).ConfigureAwait(false);
        return r.EsCorrecto ? r.ACreado($"/cartas-porte/{r.Valor.Id}") : ResultadosHttp.AProblema(r.Error);
    }

    private static async Task<IResult> PdfAsync(Guid id, IContextoEmpresa contexto, GenerarPdfCartaPorte caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var r = await caso.EjecutarAsync(contexto.EmpresaId.Value, id, ct).ConfigureAwait(false);
        return r.EsCorrecto
            ? Results.File(r.Valor.Contenido, "application/pdf", r.Valor.NombreArchivo)
            : ResultadosHttp.AProblema(r.Error);
    }
}
