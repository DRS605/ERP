using AlxorCore.Api.Comun;
using AlxorCore.Api.Contratos;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Organizacion.Aplicacion.CasosDeUso;
using System.Security.Claims;

namespace AlxorCore.Api.Endpoints;

/// <summary>Endpoints REST del módulo Organización (empresas y series).</summary>
public static class EndpointsOrganizacion
{
    public static IEndpointRouteBuilder MapearOrganizacion(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        var empresas = rutas.MapGroup("/empresas").WithTags("Empresas");

        empresas.MapPost("", CrearAsync)
            .WithSummary("Crea una empresa; el usuario pasa a ser su propietario.")
            .RequireAuthorization();

        empresas.MapGet("", ListarMiasAsync)
            .WithSummary("Lista las empresas del usuario autenticado.")
            .RequireAuthorization();

        empresas.MapPost("/{empresaId:guid}/seleccionar", SeleccionarAsync)
            .WithSummary("Selecciona la empresa activa y devuelve un token con su alcance.")
            .RequireAuthorization();

        empresas.MapGet("/actual", ActualAsync)
            .WithSummary("Devuelve la empresa activa.")
            .RequireAuthorization();

        empresas.MapPut("/actual/cobro", DatosCobroAsync)
            .WithSummary("Fija los datos de cobro por domiciliación (IBAN e identificador del acreedor SEPA).")
            .RequierePermiso(Permisos.EmpresaAjustes);

        empresas.MapPut("/actual/metodo-valoracion", MetodoValoracionAsync)
            .WithSummary("Fija el método de valoración de existencias/consumos de la empresa (parámetro de implantación).")
            .RequierePermiso(Permisos.EmpresaAjustes);

        var series = rutas.MapGroup("/series").WithTags("Series");

        series.MapGet("", ListarSeriesAsync)
            .WithSummary("Lista las series de la empresa activa.")
            .RequireAuthorization();

        series.MapPost("", CrearSerieAsync)
            .WithSummary("Crea una serie de numeración.")
            .RequierePermiso(Permisos.EmpresaAjustes);

        series.MapGet("/asignaciones", ListarAsignacionesAsync)
            .WithSummary("Lista las asignaciones de serie (empresa/cliente/proveedor por documento).")
            .RequireAuthorization();

        series.MapPost("/asignaciones", AsignarSerieAsync)
            .WithSummary("Asigna una serie a un tipo de documento (empresa, cliente o proveedor).")
            .RequierePermiso(Permisos.EmpresaAjustes);

        series.MapDelete("/asignaciones/{id:guid}", EliminarAsignacionAsync)
            .WithSummary("Elimina una asignación de serie.")
            .RequierePermiso(Permisos.EmpresaAjustes);

        return rutas;
    }

    private static async Task<IResult> ListarAsignacionesAsync(IContextoEmpresa contexto, ListarAsignacionesSerie caso, CancellationToken ct)
        => contexto.EmpresaId is null
            ? ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."))
            : Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, ct).ConfigureAwait(false));

    private static async Task<IResult> AsignarSerieAsync(AsignarSerieComando comando, IContextoEmpresa contexto, AsignarSerie caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var r = await caso.EjecutarAsync(contexto.EmpresaId.Value, comando, ct).ConfigureAwait(false);
        return r.EsCorrecto ? r.ACreado("/series/asignaciones") : ResultadosHttp.AProblema(r.Error);
    }

    private static async Task<IResult> EliminarAsignacionAsync(Guid id, EliminarAsignacionSerie caso, CancellationToken ct)
    {
        var r = await caso.EjecutarAsync(id, ct).ConfigureAwait(false);
        return r.EsCorrecto ? Results.NoContent() : ResultadosHttp.AProblema(r.Error);
    }

    private static async Task<IResult> CrearAsync(CrearEmpresaPeticion peticion, ClaimsPrincipal usuario, CrearEmpresa caso, CancellationToken ct)
    {
        var usuarioId = usuario.ObtenerUsuarioId();
        if (usuarioId is null)
        {
            return ResultadosHttp.AProblema(Error.NoAutenticado("auth.token_invalido", "El token no identifica al usuario."));
        }

        var comando = new CrearEmpresaComando(
            usuarioId.Value, peticion.Nif, peticion.RazonSocial,
            peticion.Calle, peticion.CodigoPostal, peticion.Poblacion, peticion.Provincia, peticion.RegimenIva);

        var resultado = await caso.EjecutarAsync(comando, ct).ConfigureAwait(false);
        return resultado.EsCorrecto ? resultado.ACreado("/empresas/actual") : ResultadosHttp.AProblema(resultado.Error);
    }

    private static async Task<IResult> ListarMiasAsync(ClaimsPrincipal usuario, ListarMisEmpresas caso, CancellationToken ct)
    {
        var usuarioId = usuario.ObtenerUsuarioId();
        if (usuarioId is null)
        {
            return ResultadosHttp.AProblema(Error.NoAutenticado("auth.token_invalido", "El token no identifica al usuario."));
        }

        var empresas = await caso.EjecutarAsync(usuarioId.Value, ct).ConfigureAwait(false);
        return Results.Ok(empresas);
    }

    private static async Task<IResult> SeleccionarAsync(Guid empresaId, ClaimsPrincipal usuario, SeleccionarEmpresa caso, CancellationToken ct)
    {
        var identidad = usuario.ObtenerIdentidad();
        if (identidad is null)
        {
            return ResultadosHttp.AProblema(Error.NoAutenticado("auth.token_invalido", "El token no identifica al usuario."));
        }

        var resultado = await caso.EjecutarAsync(identidad, empresaId, ct).ConfigureAwait(false);
        return resultado.AOk();
    }

    private static async Task<IResult> ActualAsync(IContextoEmpresa contexto, ObtenerEmpresa caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var resultado = await caso.EjecutarAsync(contexto.EmpresaId.Value, ct).ConfigureAwait(false);
        return resultado.AOk();
    }

    private static async Task<IResult> DatosCobroAsync(DatosCobroComando comando, IContextoEmpresa contexto, ActualizarDatosCobro caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var resultado = await caso.EjecutarAsync(contexto.EmpresaId.Value, comando, ct).ConfigureAwait(false);
        return resultado.AOk();
    }

    private static async Task<IResult> MetodoValoracionAsync(MetodoValoracionComando comando, IContextoEmpresa contexto, ActualizarMetodoValoracion caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return (await caso.EjecutarAsync(contexto.EmpresaId.Value, comando, ct).ConfigureAwait(false)).AOk();
    }

    private static async Task<IResult> ListarSeriesAsync(IContextoEmpresa contexto, ListarSeries caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var series = await caso.EjecutarAsync(contexto.EmpresaId.Value, ct).ConfigureAwait(false);
        return Results.Ok(series);
    }

    private static async Task<IResult> CrearSerieAsync(CrearSeriePeticion peticion, IContextoEmpresa contexto, CrearSerie caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var comando = new CrearSerieComando(peticion.TipoDocumento, peticion.Ejercicio, peticion.Prefijo);
        var resultado = await caso.EjecutarAsync(contexto.EmpresaId.Value, comando, ct).ConfigureAwait(false);
        return resultado.AOk();
    }
}
