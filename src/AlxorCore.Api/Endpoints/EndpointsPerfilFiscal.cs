using AlxorCore.Api.Comun;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Organizacion.Aplicacion.CasosDeUso;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Organizacion.Dominio;

namespace AlxorCore.Api.Endpoints;

/// <summary>Prorrata del ejercicio en curso, tal como la pide la ficha fiscal (régimen nulo: sin prorrata).</summary>
public sealed record ProrrataFicha(RegimenProrrata? Regimen, int PorcentajeProvisional, TipoImpuesto? Impuesto = null);

/// <summary>Ficha fiscal y prorrata del ejercicio en curso.</summary>
public sealed record PeticionPerfilFiscal(PerfilFiscal Perfil, ProrrataFicha? Prorrata);

/// <summary>
/// Ficha fiscal de la empresa (identificación, cómo tributa, qué modelos presenta), su calendario de vencimientos y el
/// catálogo de modelos con los que se le proponen.
/// </summary>
public static class EndpointsPerfilFiscal
{
    public static IEndpointRouteBuilder MapearPerfilFiscal(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        rutas.MapGet("/modelos-fiscales", (string? nif, TerritorioFiscal? territorio, bool? sii, bool? ambos) => Results.Ok(new
            {
                Modelos = PerfilFiscal.Catalogo,
                Sugeridos = PerfilFiscal.Sugeridos(nif, territorio ?? TerritorioFiscal.Comun, sii ?? false, ambos ?? false),
            }))
            .WithTags("Empresas")
            .WithSummary("Modelos tributarios que se pueden marcar y los que se proponen para un NIF, territorio y SII.")
            .RequireAuthorization();

        var g = rutas.MapGroup("/empresas/actual").WithTags("Empresas");
        g.MapGet("/perfil-fiscal", async (IContextoEmpresa contexto, IConsultaEmpresas empresas, PerfilFiscalEmpresa caso, IConsultaProrrata prorratas,
                CancellationToken ct) =>
            {
                if (contexto.EmpresaId is not { } id)
                {
                    return SinEmpresa();
                }

                var perfil = await caso.ObtenerAsync(id, ct).ConfigureAwait(false);
                if (perfil.EsFallo)
                {
                    return ResultadosHttp.AProblema(perfil.Error);
                }

                var empresa = await empresas.ObtenerAsync(id, ct).ConfigureAwait(false);
                var anio = DateTime.Today.Year;
                var principal = empresa?.ImpuestoIndirecto ?? TipoImpuesto.Iva;
                var otro = principal == TipoImpuesto.Igic ? TipoImpuesto.Iva : TipoImpuesto.Igic;
                return Results.Ok(new
                {
                    Perfil = perfil.Valor,
                    Prorrata = await prorratas.ObtenerAsync(id, anio, principal, ct).ConfigureAwait(false),
                    // Con actividad en los dos territorios, la prorrata del otro impuesto.
                    ProrrataOtroImpuesto = perfil.Valor.OperaEnAmbosTerritorios ? await prorratas.ObtenerAsync(id, anio, otro, ct).ConfigureAwait(false) : null,
                    Ejercicio = anio,
                    Modelos = PerfilFiscal.Catalogo,
                    Sugeridos = PerfilFiscal.Sugeridos(empresa?.Nif, empresa?.TerritorioFiscal ?? TerritorioFiscal.Comun, perfil.Valor.Sii, perfil.Valor.OperaEnAmbosTerritorios),
                });
            })
            .WithSummary("Ficha fiscal: nombre comercial, CNAE, IAE, datos registrales, administradores, periodicidad, SII, modelos y prorrata del año.")
            .RequierePermiso(Permisos.EmpresaAjustes);

        g.MapPut("/perfil-fiscal", async (PeticionPerfilFiscal peticion, IContextoEmpresa contexto, PerfilFiscalEmpresa caso, ConfigurarProrrata prorrata,
                CancellationToken ct) =>
            {
                if (contexto.EmpresaId is not { } id)
                {
                    return SinEmpresa();
                }

                var r = await caso.GuardarAsync(id, peticion.Perfil ?? PerfilFiscal.Vacio, ct).ConfigureAwait(false);
                if (r.EsFallo)
                {
                    return ResultadosHttp.AProblema(r.Error);
                }

                if (peticion.Prorrata is { } p)
                {
                    var rp = await prorrata.EjecutarAsync(id, DateTime.Today.Year, new ConfigurarProrrataComando(p.Regimen, p.PorcentajeProvisional, p.Impuesto), ct).ConfigureAwait(false);
                    if (rp.EsFallo)
                    {
                        return ResultadosHttp.AProblema(rp.Error);
                    }
                }

                return Results.Ok(r.Valor);
            })
            .WithSummary("Guarda la ficha fiscal y, si viene, la prorrata del ejercicio en curso.")
            .RequierePermiso(Permisos.EmpresaAjustes);

        g.MapGet("/calendario-fiscal", async (int? anio, IContextoEmpresa contexto, PerfilFiscalEmpresa caso, CancellationToken ct) =>
                contexto.EmpresaId is not { } id
                    ? SinEmpresa()
                    : (await caso.CalendarioAsync(id, anio ?? DateTime.Today.Year, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Vencimientos del año de los modelos que presenta la empresa, con los plazos generales de la AEAT.")
            .RequierePermiso(Permisos.InformeLeer);

        return rutas;
    }

    private static IResult SinEmpresa() => ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
}
