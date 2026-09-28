using AlxorCore.Api.Comun;
using AlxorCore.Documentos.Aplicacion;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Tesoreria.Aplicacion;
using AlxorCore.Tesoreria.Dominio;

namespace AlxorCore.Api.Endpoints;

/// <summary>Respuesta de una reclamación: lo registrado y si se ha enviado el correo.</summary>
public sealed record ReclamacionEnviadaResp(ReclamacionDto Reclamacion, bool CorreoEnviado, string? MotivoNoEnviado);

/// <summary>
/// Endpoints de cobranza (base, en todas las ediciones): anticipos de clientes y su aplicación a
/// facturas, lista de impagados con el nivel de reclamación que toca y registro/envío de reclamaciones.
/// </summary>
public static class EndpointsCobranza
{
    public static IEndpointRouteBuilder MapearCobranza(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        var anticipos = rutas.MapGroup("/anticipos").WithTags("Cobranza");
        anticipos.MapPost("", RegistrarAnticipoAsync)
            .WithSummary("Registra un anticipo de un cliente. Con Facturar (y CodigoIva) emite la factura del anticipo con su IVA (art. 75.2 LIVA) y registra su cobro.")
            .RequierePermiso(Permisos.CobroRegistrar);
        anticipos.MapGet("", async (Guid? clienteId, ListarAnticipos caso, CancellationToken ct) =>
                Results.Ok(await caso.EjecutarAsync(clienteId, ct).ConfigureAwait(false)))
            .WithSummary("Lista los anticipos (de un cliente o todos) con su disponible.")
            .RequierePermiso(Permisos.FacturaLeer);
        anticipos.MapPost("/{id:guid}/anular", async (Guid id, AnularAnticipo caso, CancellationToken ct) =>
                (await caso.EjecutarAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Anula un anticipo sin nada aplicado (registrado por error o devuelto).")
            .RequierePermiso(Permisos.CobroRegistrar);

        anticipos.MapPost("/{id:guid}/aplicar", AplicarAnticipoAsync)
            .WithSummary("Aplica un anticipo a una factura del mismo cliente (genera el cobro de la factura).")
            .RequierePermiso(Permisos.CobroRegistrar);

        var impagados = rutas.MapGroup("/impagados").WithTags("Cobranza");
        impagados.MapGet("", ListarImpagadosAsync)
            .WithSummary("Facturas vencidas con importe pendiente, con el nivel de reclamación que toca.")
            .RequierePermiso(Permisos.FacturaLeer);
        impagados.MapPost("/{facturaId:guid}/reclamar", ReclamarAsync)
            .WithSummary("Registra una reclamación y, si el canal es email y el cliente tiene correo, la envía con la factura en PDF.")
            .RequierePermiso(Permisos.CobroRegistrar);
        impagados.MapGet("/niveles", async (NivelesReclamacion caso, CancellationToken ct) => Results.Ok(await caso.ObtenerAsync(ct).ConfigureAwait(false)))
            .WithSummary("Niveles de reclamación de la empresa (días tras el vencimiento y textos).")
            .RequierePermiso(Permisos.FacturaLeer);
        impagados.MapPut("/niveles", CambiarNivelesAsync)
            .WithSummary("Cambia los niveles de reclamación. Variables: {cliente}, {factura}, {vencimiento}, {pendiente}, {dias}.")
            .RequierePermiso(Permisos.EmpresaAjustes);

        return rutas;
    }

    private static Error SinEmpresa() => Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero.");

    private static async Task<IResult> RegistrarAnticipoAsync(RegistrarAnticipoComando comando, IContextoEmpresa contexto, RegistrarAnticipo caso,
        RegistrarAnticipoFacturado conFactura, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(SinEmpresa());
        }

        // Con factura (lo normal): factura del anticipo con su IVA, su cobro y el anticipo. Sin ella, solo el anticipo (57x a 438).
        var r = comando.Facturar
            ? await conFactura.EjecutarAsync(contexto.EmpresaId.Value, comando with { Factura = null }, ct).ConfigureAwait(false)
            : await caso.EjecutarAsync(contexto.EmpresaId.Value, comando with { Factura = null }, ct).ConfigureAwait(false);
        return r.EsCorrecto ? r.ACreado($"/anticipos/{r.Valor.Id}") : ResultadosHttp.AProblema(r.Error);
    }

    private static async Task<IResult> AplicarAnticipoAsync(Guid id, AplicarAnticipoComando comando, IContextoEmpresa contexto, AplicarAnticipo caso, CancellationToken ct) =>
        contexto.EmpresaId is null
            ? ResultadosHttp.AProblema(SinEmpresa())
            : (await caso.EjecutarAsync(contexto.EmpresaId.Value, id, comando, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> ListarImpagadosAsync(IContextoEmpresa contexto, GestionImpagados caso, CancellationToken ct) =>
        contexto.EmpresaId is null
            ? ResultadosHttp.AProblema(SinEmpresa())
            : Results.Ok(await caso.ListarAsync(contexto.EmpresaId.Value, ct).ConfigureAwait(false));

    private static async Task<IResult> CambiarNivelesAsync(List<NivelReclamacion> niveles, IContextoEmpresa contexto, NivelesReclamacion caso, CancellationToken ct) =>
        contexto.EmpresaId is null
            ? ResultadosHttp.AProblema(SinEmpresa())
            : (await caso.CambiarAsync(contexto.EmpresaId.Value, niveles, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> ReclamarAsync(
        Guid facturaId, RegistrarReclamacionComando comando, IContextoEmpresa contexto, GestionImpagados caso,
        GenerarPdfFactura pdf, IServicioCorreo correo, CancellationToken ct)
    {
        if (contexto.EmpresaId is not { } empresaId)
        {
            return ResultadosHttp.AProblema(SinEmpresa());
        }

        var r = await caso.ReclamarAsync(empresaId, facturaId, comando, ct).ConfigureAwait(false);
        if (r.EsFallo)
        {
            return ResultadosHttp.AProblema(r.Error);
        }

        var rec = r.Valor;
        if (comando.Canal != CanalReclamacion.Email)
        {
            return Results.Ok(new ReclamacionEnviadaResp(rec, false, "Reclamación registrada (canal distinto de email)."));
        }

        if (string.IsNullOrWhiteSpace(rec.EmailCliente))
        {
            return Results.Ok(new ReclamacionEnviadaResp(rec, false, "El cliente no tiene correo electrónico: la reclamación queda registrada sin enviar."));
        }

        var documento = await pdf.EjecutarAsync(empresaId, facturaId, ct).ConfigureAwait(false);
        if (documento.EsFallo)
        {
            return Results.Ok(new ReclamacionEnviadaResp(rec, false, documento.Error.Mensaje));
        }

        await correo.EnviarAsync(new MensajeCorreo(rec.EmailCliente, rec.Asunto, rec.Texto, documento.Valor.Contenido, documento.Valor.NombreArchivo), ct)
            .ConfigureAwait(false);
        return Results.Ok(new ReclamacionEnviadaResp(rec, true, null));
    }
}
