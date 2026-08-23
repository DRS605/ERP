using AlxorCore.Api.Comun;
using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Catalogo.Dominio;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;

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
    public sealed record PeticionTipoIva(string? Codigo, string? Nombre, decimal Porcentaje, decimal RecargoEquivalencia, ClaseIva Clase, string? MencionFactura, bool Activo = true);

    private static async Task<IResult> ListarAsync(IContextoEmpresa contexto, ListarTiposIva caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> CrearAsync(PeticionTipoIva peticion, IContextoEmpresa contexto, CrearTipoIva caso, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(peticion);
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var datos = new DatosTipoIva(peticion.Codigo, peticion.Nombre, peticion.Porcentaje, peticion.RecargoEquivalencia, peticion.Clase, peticion.MencionFactura);
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
