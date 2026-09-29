using AlxorCore.Api.Comun;
using AlxorCore.Cooperativa.Aplicacion;
using AlxorCore.Cooperativa.Dominio;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Api.Endpoints;

/// <summary>
/// Cooperativas y SAT: ajustes, socios (alta, baja con reembolso), capital social (suscripciones, desembolsos, reembolsos,
/// transmisiones y anulaciones), reparto del excedente con retorno cooperativo, retenciones, libros registro y actas.
/// </summary>
public static class EndpointsCooperativa
{
    public sealed record PeticionAnulacion(string? Motivo, DateOnly? Fecha = null);

    public static IEndpointRouteBuilder MapearCooperativa(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var g = rutas.MapGroup("/cooperativa").WithTags("Cooperativa");
        const string leer = Permisos.CooperativaLeer;
        const string gestionar = Permisos.CooperativaGestionar;

        g.MapGet("/configuracion", (IContextoEmpresa c, SociosCooperativa s, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await s.ConfiguracionAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Forma jurídica, aportación obligatoria, fondos mínimos, interés máximo, retención, base del retorno, deducciones y cuentas.").RequierePermiso(leer);
        g.MapPut("/configuracion", (DatosConfiguracionCooperativa d, IContextoEmpresa c, SociosCooperativa s, CancellationToken ct) =>
                ConEmpresa(c, async e => (await s.FijarConfiguracionAsync(e, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Fija los ajustes (sin fondos ni base, los de la forma jurídica: cooperativa 20 % + 5 % por actividad; SAT sin fondos, por capital).")
            .RequierePermiso(Permisos.EmpresaAjustes);

        // ---------------------------------------------------------------- Socios
        g.MapGet("/socios", (bool? incluirBajas, IContextoEmpresa c, SociosCooperativa s, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await s.ListarAsync(e, incluirBajas ?? false, ct).ConfigureAwait(false))))
            .WithSummary("Socios (de alta; con incluirBajas, también los de baja) con su capital y lo que les falta para la aportación obligatoria.").RequierePermiso(leer);
        g.MapPost("/socios", (DatosSocio d, IContextoEmpresa c, SociosCooperativa s, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await s.CrearAsync(e, d, ct).ConfigureAwait(false), "socios")))
            .WithSummary("Alta de un socio (un proveedor) con el siguiente número y, si se indica, su aportación obligatoria.").RequierePermiso(gestionar);
        g.MapGet("/socios/{id:guid}", async (Guid id, SociosCooperativa s, CancellationToken ct) =>
                await s.CapitalSocioAsync(id, ct).ConfigureAwait(false) is { } r ? Results.Ok(r)
                    : ResultadosHttp.AProblema(Error.NoEncontrado("socio.no_encontrado", "El socio no existe.")))
            .WithSummary("Ficha del socio con su capital y sus movimientos (certificado de aportaciones).").RequierePermiso(leer);
        g.MapPut("/socios/{id:guid}", async (Guid id, DatosCambioSocio d, SociosCooperativa s, CancellationToken ct) =>
                (await s.ActualizarAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Cambia la clase de socio y las observaciones.").RequierePermiso(gestionar);
        g.MapPost("/socios/{id:guid}/baja", async (Guid id, DatosBaja d, SociosCooperativa s, CancellationToken ct) =>
                (await s.DarDeBajaAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Baja del socio (definitiva) y reembolso de su capital con la deducción que corresponda al motivo.").RequierePermiso(gestionar);
        g.MapDelete("/socios/{id:guid}", async (Guid id, SociosCooperativa s, CancellationToken ct) =>
                (await s.EliminarAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Elimina un socio dado de alta por error (sin capital ni repartos); si no, se le da de baja.").RequierePermiso(gestionar);

        // ---------------------------------------------------------------- Capital
        g.MapGet("/capital", (IContextoEmpresa c, SociosCooperativa s, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await s.ResumenAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Capital social: suscrito, desembolsado y pendiente (obligatorio y voluntario), en total y por socio.").RequierePermiso(leer);
        g.MapPost("/capital/suscripciones", (DatosSuscripcion d, IContextoEmpresa c, SociosCooperativa s, CancellationToken ct) =>
                ConEmpresa(c, async e => (await s.SuscribirAsync(e, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("El socio suscribe capital (y desembolsa en el acto lo que se indique).").RequierePermiso(gestionar);
        g.MapPost("/capital/desembolsos", (DatosDesembolso d, IContextoEmpresa c, SociosCooperativa s, CancellationToken ct) =>
                ConEmpresa(c, async e => (await s.DesembolsarAsync(e, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Desembolso de capital suscrito.").RequierePermiso(gestionar);
        g.MapPost("/capital/reembolsos", (DatosReembolso d, IContextoEmpresa c, SociosCooperativa s, CancellationToken ct) =>
                ConEmpresa(c, async e => (await s.ReembolsarAsync(e, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Reembolso de capital voluntario (o del obligatorio de un socio de baja), con deducción.").RequierePermiso(gestionar);
        g.MapPost("/capital/transmisiones", (DatosTransmision d, IContextoEmpresa c, SociosCooperativa s, CancellationToken ct) =>
                ConEmpresa(c, async e => (await s.TransmitirAsync(e, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Transmisión de capital desembolsado de un socio a otro.").RequierePermiso(gestionar);
        g.MapPost("/capital/movimientos/{id:guid}/anular", async (Guid id, DatosAnulacionCapital d, SociosCooperativa s, CancellationToken ct) =>
                (await s.AnularAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Anula un movimiento de capital (una transmisión, entera) con su asiento.").RequierePermiso(gestionar);

        // ---------------------------------------------------------------- Libros
        g.MapGet("/libros/socios", (IContextoEmpresa c, SociosCooperativa s, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await s.LibroSociosAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Libro registro de socios: todos, con sus datos, clase, alta, baja y capital.").RequierePermiso(leer);
        g.MapGet("/libros/aportaciones", (Guid? socioId, DateOnly? desde, DateOnly? hasta, IContextoEmpresa c, SociosCooperativa s, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await s.LibroAportacionesAsync(e, socioId, desde, hasta, ct).ConfigureAwait(false))))
            .WithSummary("Libro registro de aportaciones al capital: cada movimiento con el capital acumulado del socio.").RequierePermiso(leer);

        // ---------------------------------------------------------------- Repartos
        g.MapGet("/repartos", (IContextoEmpresa c, RepartosCooperativa r, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await r.ListarAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Repartos del excedente por ejercicio.").RequierePermiso(leer);
        g.MapPost("/repartos/simular", (DatosReparto d, IContextoEmpresa c, RepartosCooperativa r, CancellationToken ct) =>
                ConEmpresa(c, async e => (await r.SimularAsync(e, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Calcula la propuesta de reparto sin guardarla.").RequierePermiso(leer);
        g.MapPost("/repartos", (DatosReparto d, IContextoEmpresa c, RepartosCooperativa r, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await r.CrearAsync(e, d, ct).ConfigureAwait(false), "repartos")))
            .WithSummary("Guarda la propuesta de reparto (borrador): fondos, intereses, reservas y retorno por socio.").RequierePermiso(gestionar);
        g.MapGet("/repartos/{id:guid}", async (Guid id, RepartosCooperativa r, CancellationToken ct) =>
                await r.ObtenerAsync(id, ct).ConfigureAwait(false) is { } x ? Results.Ok(x)
                    : ResultadosHttp.AProblema(Error.NoEncontrado("reparto.no_encontrado", "El reparto no existe.")))
            .WithSummary("Un reparto con lo que le toca a cada socio.").RequierePermiso(leer);
        g.MapPut("/repartos/{id:guid}", async (Guid id, DatosReparto d, RepartosCooperativa r, CancellationToken ct) =>
                (await r.RecalcularAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Recalcula la propuesta (solo en borrador).").RequierePermiso(gestionar);
        g.MapPost("/repartos/{id:guid}/contabilizar", async (Guid id, RepartosCooperativa r, CancellationToken ct) =>
                (await r.ContabilizarAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Contabiliza el reparto aprobado por la asamblea y capitaliza el retorno de quien lo deja como capital.").RequierePermiso(gestionar);
        g.MapPost("/repartos/{id:guid}/anular", async (Guid id, PeticionAnulacion p, RepartosCooperativa r, CancellationToken ct) =>
                (await r.AnularAsync(id, p.Motivo, p.Fecha, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Anula un reparto contabilizado (contraasiento y anulación de lo capitalizado).").RequierePermiso(gestionar);
        g.MapDelete("/repartos/{id:guid}", async (Guid id, RepartosCooperativa r, CancellationToken ct) =>
                (await r.EliminarAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Elimina un reparto en borrador.").RequierePermiso(gestionar);
        g.MapGet("/retenciones", (int ejercicio, IContextoEmpresa c, RepartosCooperativa r, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await r.RetencionesAsync(e, ejercicio, ct).ConfigureAwait(false))))
            .WithSummary("Retenciones del año por socio sobre retornos e intereses (base de los modelos 123 y 193).").RequierePermiso(leer);

        // ---------------------------------------------------------------- Actas
        g.MapGet("/actas", (OrganoSocial? organo, IContextoEmpresa c, ActasCooperativa a, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await a.ListarAsync(e, organo, ct).ConfigureAwait(false))))
            .WithSummary("Libros de actas (asamblea general, consejo rector, junta rectora).").RequierePermiso(leer);
        g.MapPost("/actas", (DatosNuevaActa d, IContextoEmpresa c, ActasCooperativa a, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await a.CrearAsync(e, d, ct).ConfigureAwait(false), "actas")))
            .WithSummary("Redacta un acta (borrador).").RequierePermiso(gestionar);
        g.MapGet("/actas/{id:guid}", async (Guid id, ActasCooperativa a, CancellationToken ct) =>
                await a.ObtenerAsync(id, ct).ConfigureAwait(false) is { } x ? Results.Ok(x)
                    : ResultadosHttp.AProblema(Error.NoEncontrado("acta.no_encontrada", "El acta no existe.")))
            .WithSummary("Un acta.").RequierePermiso(leer);
        g.MapPut("/actas/{id:guid}", async (Guid id, DatosActa d, ActasCooperativa a, CancellationToken ct) =>
                (await a.CambiarAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Cambia un acta en borrador.").RequierePermiso(gestionar);
        g.MapPost("/actas/{id:guid}/aprobar", async (Guid id, ActasCooperativa a, CancellationToken ct) =>
                (await a.AprobarAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Aprueba el acta: recibe su número en el libro y ya no cambia.").RequierePermiso(gestionar);
        g.MapDelete("/actas/{id:guid}", async (Guid id, ActasCooperativa a, CancellationToken ct) =>
                (await a.EliminarAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Elimina un acta en borrador.").RequierePermiso(gestionar);

        return rutas;
    }

    private static async Task<IResult> ConEmpresa(IContextoEmpresa contexto, Func<Guid, Task<IResult>> accion) =>
        contexto.EmpresaId is { } empresa
            ? await accion(empresa).ConfigureAwait(false)
            : ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));

    private static IResult Creado<T>(Resultado<T> r, string ruta) =>
        r.EsCorrecto ? Results.Created($"/cooperativa/{ruta}", r.Valor) : ResultadosHttp.AProblema(r.Error);
}
