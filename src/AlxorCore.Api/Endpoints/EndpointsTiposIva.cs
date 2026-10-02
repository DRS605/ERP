using AlxorCore.Api.Comun;
using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Catalogo.Dominio;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Organizacion.Aplicacion.Puertos;

namespace AlxorCore.Api.Endpoints;

/// <summary>Endpoints REST del catálogo de tipos de IVA configurable por empresa.</summary>
public static class EndpointsTiposIva
{
    public static IEndpointRouteBuilder MapearTiposIva(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        var g = rutas.MapGroup("/tipos-iva").WithTags("Tipos de IVA");

        g.MapGet("", ListarAsync)
            .WithSummary("Lista los tipos de IVA de la empresa (siembra el estándar la primera vez).")
            .RequireAuthorization();

        g.MapPost("", CrearAsync)
            .WithSummary("Crea un tipo de IVA (con su clase: ordinario, exento, no sujeto, ISP, importación, intracomunitario).")
            .RequierePermiso(Permisos.EmpresaAjustes);

        g.MapPut("/{id:guid}", ActualizarAsync)
            .WithSummary("Actualiza un tipo de IVA (nombre, porcentaje, recargo, clase, mención y estado).")
            .RequierePermiso(Permisos.EmpresaAjustes);

        return rutas;
    }

    /// <summary>Cuerpo para crear o actualizar un tipo de IVA.</summary>
    public sealed record PeticionTipoIva(
        string? Codigo, string? Nombre, decimal Porcentaje, decimal RecargoEquivalencia, ClaseIva Clase, string? MencionFactura, bool Activo = true,
        TipoImpuesto? Impuesto = null);

    private static async Task<IResult> ListarAsync(IContextoEmpresa contexto, ListarTiposIva caso, IConsultaEmpresas empresas, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        // Se siembran los tipos del impuesto de la empresa: IVA, o IGIC si está en Canarias.
        var empresa = await empresas.ObtenerAsync(contexto.EmpresaId.Value, ct).ConfigureAwait(false);
        var impuesto = empresa?.ImpuestoIndirecto ?? TipoImpuesto.Iva;
        // Con actividad en los dos territorios se siembran también los tipos del otro impuesto.
        if (empresa is { OperaEnAmbosTerritorios: true })
        {
            await caso.EjecutarAsync(contexto.EmpresaId.Value, impuesto == TipoImpuesto.Igic ? TipoImpuesto.Iva : TipoImpuesto.Igic, ct).ConfigureAwait(false);
        }

        return Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, impuesto, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> CrearAsync(PeticionTipoIva peticion, IContextoEmpresa contexto, CrearTipoIva caso, IConsultaEmpresas empresas, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(peticion);
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        // Sin impuesto indicado, el tipo es del impuesto de la empresa (IVA, o IGIC en Canarias).
        var impuesto = peticion.Impuesto
            ?? (await empresas.ObtenerAsync(contexto.EmpresaId.Value, ct).ConfigureAwait(false))?.ImpuestoIndirecto
            ?? TipoImpuesto.Iva;
        var datos = new DatosTipoIva(peticion.Codigo, peticion.Nombre, peticion.Porcentaje, peticion.RecargoEquivalencia, peticion.Clase, peticion.MencionFactura, impuesto);
        var r = await caso.EjecutarAsync(contexto.EmpresaId.Value, datos, ct).ConfigureAwait(false);
        return r.EsCorrecto ? r.ACreado($"/tipos-iva/{r.Valor.Id}") : ResultadosHttp.AProblema(r.Error);
    }

    private static async Task<IResult> ActualizarAsync(Guid id, PeticionTipoIva peticion, ActualizarTipoIva caso, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(peticion);
        var datos = new DatosTipoIva(peticion.Codigo, peticion.Nombre, peticion.Porcentaje, peticion.RecargoEquivalencia, peticion.Clase, peticion.MencionFactura);
        return (await caso.EjecutarAsync(id, datos, peticion.Activo, ct).ConfigureAwait(false)).AOk();
    }
}
