using AlxorCore.Api.Comun;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Recepcion.Aplicacion;

namespace AlxorCore.Api.Endpoints;

/// <summary>Endpoints REST del módulo Recepción de facturas de proveedor.</summary>
public static class EndpointsRecepcion
{
    public static IEndpointRouteBuilder MapearRecepcion(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        var grupo = rutas.MapGroup("/recepcion/facturas").WithTags("Recepción de facturas");

        grupo.MapGet("", ListarAsync)
            .WithSummary("Lista las facturas recibidas de la empresa (opcionalmente por estado).")
            .RequierePermiso(Permisos.RecepcionLeer);

        grupo.MapGet("/{id:guid}", ObtenerAsync)
            .WithSummary("Obtiene una factura recibida.")
            .RequierePermiso(Permisos.RecepcionLeer);

        grupo.MapGet("/{id:guid}/documento", DescargarAsync)
            .WithSummary("Descarga el documento adjunto (PDF) de una factura recibida.")
            .RequierePermiso(Permisos.RecepcionLeer);

        grupo.MapPost("", RecibirAsync)
            .WithSummary("Da de alta una factura en la bandeja subiendo el documento (base64).")
            .RequierePermiso(Permisos.RecepcionGestionar);

        grupo.MapPost("/{id:guid}/validar", ValidarAsync)
            .WithSummary("Valida (confirma o corrige) los datos de una factura recibida.")
            .RequierePermiso(Permisos.RecepcionGestionar);

        grupo.MapPost("/{id:guid}/contabilizar", ContabilizarAsync)
            .WithSummary("Contabiliza una factura validada (genera el gasto con IVA soportado).")
            .RequierePermiso(Permisos.RecepcionContabilizar);

        grupo.MapPost("/{id:guid}/rechazar", RechazarAsync)
            .WithSummary("Rechaza (descarta) una factura recibida.")
            .RequierePermiso(Permisos.RecepcionGestionar);

        rutas.MapPost("/recepcion/buzon/procesar", ProcesarBuzonAsync)
            .WithTags("Recepción de facturas")
            .WithSummary("Revisa el buzón de correo y da de alta las facturas nuevas.")
            .RequierePermiso(Permisos.RecepcionGestionar);

        return rutas;
    }

    private static async Task<IResult> ListarAsync(string? estado, IContextoEmpresa contexto, ListarFacturasRecibidas caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, estado, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> ObtenerAsync(Guid id, ObtenerFacturaRecibida caso, CancellationToken ct)
    {
        var dto = await caso.EjecutarAsync(id, ct).ConfigureAwait(false);
        return dto is null ? ResultadosHttp.AProblema(Error.NoEncontrado("recepcion.no_encontrada", "No se encontró la factura recibida.")) : Results.Ok(dto);
    }

    private static async Task<IResult> DescargarAsync(Guid id, IConsultaFacturasRecibidas consulta, CancellationToken ct)
    {
        var adjunto = await consulta.ObtenerContenidoAsync(id, ct).ConfigureAwait(false);
        return adjunto is null
            ? ResultadosHttp.AProblema(Error.NoEncontrado("recepcion.no_encontrada", "No se encontró la factura recibida."))
            : Results.File(adjunto.Contenido, adjunto.TipoContenido, adjunto.NombreArchivo);
    }

    private static async Task<IResult> RecibirAsync(RecibirFacturaComando comando, IContextoEmpresa contexto, RecibirFactura caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var resultado = await caso.EjecutarAsync(contexto.EmpresaId.Value, comando, ct).ConfigureAwait(false);
        return resultado.EsCorrecto ? resultado.ACreado($"/recepcion/facturas/{resultado.Valor.Id}") : ResultadosHttp.AProblema(resultado.Error);
    }

    private static async Task<IResult> ValidarAsync(Guid id, ValidarFacturaComando comando, IContextoEmpresa contexto, ValidarFactura caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return (await caso.EjecutarAsync(contexto.EmpresaId.Value, id, comando, ct).ConfigureAwait(false)).AOk();
    }

    private static async Task<IResult> ContabilizarAsync(Guid id, IContextoEmpresa contexto, ContabilizarFactura caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return (await caso.EjecutarAsync(contexto.EmpresaId.Value, id, ct).ConfigureAwait(false)).AOk();
    }

    private static async Task<IResult> RechazarAsync(Guid id, RechazarFacturaComando comando, IContextoEmpresa contexto, RechazarFactura caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return (await caso.EjecutarAsync(contexto.EmpresaId.Value, id, comando, ct).ConfigureAwait(false)).AOk();
    }

    private static async Task<IResult> ProcesarBuzonAsync(IContextoEmpresa contexto, ProcesarBuzon caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var altas = await caso.EjecutarAsync(contexto.EmpresaId.Value, ct).ConfigureAwait(false);
        return Results.Ok(new { altas });
    }
}
