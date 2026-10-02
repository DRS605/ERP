using AlxorCore.Api.Comun;
using AlxorCore.Informes.Aplicacion;
using AlxorCore.Informes.Dominio;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Api.Endpoints;

/// <summary>
/// Envío del SII a la AEAT: certificado electrónico de la empresa (cifrado), situación de cada factura del mes, envío
/// (alta, modificación y baja) y el histórico de envíos con su petición y respuesta.
/// </summary>
public static class EndpointsSii
{
    public sealed record EntornoPeticion(EntornoSii Entorno);

    public static IEndpointRouteBuilder MapearSii(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var sii = rutas.MapGroup("/informes/sii").WithTags("SII");

        sii.MapGet("/certificado", async (IContextoEmpresa contexto, GestionCertificadoSii caso, CancellationToken ct) =>
                contexto.EmpresaId is not { } empresa ? SinEmpresa()
                : await caso.ObtenerAsync(empresa, ct).ConfigureAwait(false) is { } c ? Results.Ok(c) : Results.NotFound())
            .WithSummary("Datos del certificado de la empresa para el SII (titular, NIF, caducidad y entorno).")
            .RequierePermiso(Permisos.InformeLeer);

        sii.MapPut("/certificado", async (SubirCertificadoSiiComando comando, IContextoEmpresa contexto, GestionCertificadoSii caso, CancellationToken ct) =>
                contexto.EmpresaId is not { } empresa ? SinEmpresa() : (await caso.GuardarAsync(empresa, comando, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Sube (o sustituye) el certificado .pfx de la empresa, en base64, con su contraseña. Se guarda cifrado.")
            .RequierePermiso(Permisos.EmpresaAjustes);

        sii.MapPut("/certificado/entorno", async (EntornoPeticion peticion, IContextoEmpresa contexto, GestionCertificadoSii caso, CancellationToken ct) =>
                contexto.EmpresaId is not { } empresa ? SinEmpresa() : (await caso.CambiarEntornoAsync(empresa, peticion.Entorno, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Cambia el entorno del SII: Pruebas o Produccion.")
            .RequierePermiso(Permisos.EmpresaAjustes);

        sii.MapDelete("/certificado", async (IContextoEmpresa contexto, GestionCertificadoSii caso, CancellationToken ct) =>
                contexto.EmpresaId is not { } empresa ? SinEmpresa() : (await caso.EliminarAsync(empresa, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Quita el certificado de la empresa.")
            .RequierePermiso(Permisos.EmpresaAjustes);

        sii.MapGet("/situacion", async (TipoLibroSii libro, int ejercicio, int periodo, AdministracionSii? administracion, IContextoEmpresa contexto, EnviarSii caso,
                CancellationToken ct) =>
                contexto.EmpresaId is not { } empresa ? SinEmpresa()
                : (await caso.SituacionAsync(empresa, libro, ejercicio, periodo, administracion, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Situación de cada factura del mes en el SII: pendiente, enviada, rechazada, modificada o de baja.")
            .RequierePermiso(Permisos.InformeLeer);

        sii.MapPost("/enviar", async (EnviarSiiComando comando, IContextoEmpresa contexto, EnviarSii caso, CancellationToken ct) =>
                contexto.EmpresaId is not { } empresa ? SinEmpresa() : (await caso.EjecutarAsync(empresa, comando, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Envía lo pendiente del libro y el mes (a la AEAT o, con Administracion=Atc, a la Agencia Tributaria Canaria): altas (A0), modificaciones (A1) y bajas de las anuladas.")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        sii.MapGet("/envios", async (IContextoEmpresa contexto, EnviarSii caso, CancellationToken ct) =>
                contexto.EmpresaId is not { } empresa ? SinEmpresa() : Results.Ok(await caso.EnviosAsync(empresa, ct).ConfigureAwait(false)))
            .WithSummary("Histórico de envíos al SII (estado, CSV y recuento).")
            .RequierePermiso(Permisos.InformeLeer);

        sii.MapGet("/envios/{id:guid}/xml", async (Guid id, string? parte, EnviarSii caso, CancellationToken ct) =>
            {
                if (await caso.DetalleEnvioAsync(id, ct).ConfigureAwait(false) is not { } e)
                {
                    return Results.NotFound();
                }

                var respuesta = string.Equals(parte, "respuesta", StringComparison.OrdinalIgnoreCase);
                var xml = respuesta ? e.Respuesta ?? string.Empty : e.Peticion;
                return Results.File(System.Text.Encoding.UTF8.GetBytes(xml), "application/xml", $"sii-{(respuesta ? "respuesta" : "peticion")}-{id:N}.xml");
            })
            .WithSummary("XML de la petición o (con parte=respuesta) de la respuesta de un envío.")
            .RequierePermiso(Permisos.DatosExportar);

        return rutas;
    }

    private static IResult SinEmpresa() => ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
}
