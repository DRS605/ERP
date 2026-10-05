using AlxorCore.Api.Comun;
using AlxorCore.Informes.Aplicacion;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Api.Endpoints;

/// <summary>Impuesto especial sobre los envases de plástico no reutilizables: fichas de plástico de los artículos y modelo 592.</summary>
public static class EndpointsPlastico
{
    public static IEndpointRouteBuilder MapearPlastico(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var g = rutas.MapGroup("/informes").WithTags("Informes");

        g.MapGet("/plastico/fichas", (IContextoEmpresa c, FichasPlastico s, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await s.ListarAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Fichas de plástico de los artículos (clave, kilos de plástico y de reciclado por unidad, exención).").RequierePermiso(Permisos.InventarioLeer);
        g.MapPut("/plastico/fichas/{productoId:guid}", (Guid productoId, DatosFichaPlastico d, IContextoEmpresa c, FichasPlastico s, CancellationToken ct) =>
                ConEmpresa(c, async e => (await s.FijarAsync(e, productoId, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Crea o cambia la ficha de plástico de un artículo.").RequierePermiso(Permisos.ProductoGestionar);
        g.MapDelete("/plastico/fichas/{productoId:guid}", (Guid productoId, IContextoEmpresa c, FichasPlastico s, CancellationToken ct) =>
                ConEmpresa(c, async e => (await s.QuitarAsync(e, productoId, ct).ConfigureAwait(false)).ASinContenido()))
            .WithSummary("Quita la ficha de plástico de un artículo.").RequierePermiso(Permisos.ProductoGestionar);

        g.MapGet("/modelo-592", (int anio, string periodo, string? formato, IContextoEmpresa c, Modelo592 m, CancellationToken ct) =>
                ConEmpresa(c, async e =>
                {
                    var r = await m.CalcularAsync(e, anio, periodo, ct).ConfigureAwait(false);
                    if (r.EsFallo)
                    {
                        return ResultadosHttp.AProblema(r.Error);
                    }

                    return string.Equals(formato, "csv", StringComparison.OrdinalIgnoreCase)
                        ? Results.File(System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(Modelo592.LibroCsv(r.Valor))).ToArray(), "text/csv",
                            $"libro-plastico-{r.Valor.Anio}-{r.Valor.Periodo}.csv")
                        : Results.Ok(r.Valor);
                }))
            .WithSummary("Modelo 592 (envases de plástico no reutilizables) de un periodo (1T-4T o 01-12): kilos, cuota y libro registro (csv con formato=csv).")
            .RequierePermiso(Permisos.InformeLeer);
        return rutas;
    }

    private static async Task<IResult> ConEmpresa(IContextoEmpresa contexto, Func<Guid, Task<IResult>> accion) =>
        contexto.EmpresaId is { } empresa
            ? await accion(empresa).ConfigureAwait(false)
            : ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
}
