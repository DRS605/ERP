using AlxorCore.Api.Comun;
using AlxorCore.Api.Contratos;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Organizacion.Aplicacion;
using AlxorCore.Organizacion.Aplicacion.CasosDeUso;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

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

        rutas.MapGet("/grupos/actual", (AlxorCore.Nucleo.Multiempresa.IContextoEmpresa contexto) =>
                Results.Ok(new { id = contexto.GrupoId }))
            .WithTags("Grupos")
            .WithSummary("Devuelve el grupo (holding) de la empresa activa; sus maestros son compartidos.")
            .RequireAuthorization();

        rutas.MapGet("/grupos/actual/empresas", async (AlxorCore.Nucleo.Multiempresa.IContextoEmpresa contexto, IConsultaEmpresas empresas, CancellationToken ct) =>
                contexto.GrupoId is { } grupo
                    ? Results.Ok(await empresas.EmpresasDelGrupoAsync(grupo, ct).ConfigureAwait(false))
                    : ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero.")))
            .WithTags("Grupos")
            .WithSummary("Empresas del grupo de la empresa activa (para enlazar clientes y proveedores del grupo).")
            .RequireAuthorization();

        empresas.MapPut("/actual/cobro", DatosCobroAsync)
            .WithSummary("Fija los datos de cobro por domiciliación (IBAN e identificador del acreedor SEPA).")
            .RequierePermiso(Permisos.EmpresaAjustes);

        empresas.MapPut("/actual/metodo-valoracion", MetodoValoracionAsync)
            .WithSummary("Fija el método de valoración de existencias/consumos de la empresa (parámetro de implantación).")
            .RequierePermiso(Permisos.EmpresaAjustes);

        empresas.MapPut("/actual/control-riesgo", ControlRiesgoAsync)
            .WithSummary("Fija el control de riesgo de la empresa (avisar o bloquear al superar el límite).")
            .RequierePermiso(Permisos.EmpresaAjustes);

        empresas.MapPut("/actual/territorio-fiscal", TerritorioFiscalAsync)
            .WithSummary("Fija el territorio fiscal: Comun (IVA, Península y Baleares) o Canarias (IGIC). Siembra los tipos de IGIC si faltan.")
            .RequierePermiso(Permisos.EmpresaAjustes);

        empresas.MapGet("/actual/plan", PlanAsync)
            .WithSummary("Plan contratado de la empresa: edición, módulos adicionales y módulos activos.")
            .RequireAuthorization();

        empresas.MapPut("/actual/plan", CambiarPlanAsync)
            .WithSummary("Cambia la edición y los módulos adicionales. Se aplica al volver a seleccionar la empresa.")
            .RequierePermiso(Permisos.EmpresaAjustes);

        rutas.MapGet("/planes", () => Results.Ok(new
            {
                ediciones = AlxorCore.Nucleo.Modulos.CatalogoModulos.Ediciones,
                modulos = AlxorCore.Nucleo.Modulos.CatalogoModulos.Modulos,
            }))
            .WithTags("Organización")
            .WithSummary("Catálogo de ediciones y módulos contratables.")
            .AllowAnonymous();

        empresas.MapPut("/actual/plantilla", PlantillaAsync)
            .WithSummary("Configura la plantilla de documentos (datos de cabecera, contacto, color, pie y logotipo).")
            .RequierePermiso(Permisos.EmpresaAjustes);

        var series = rutas.MapGroup("/series").WithTags("Series");

        series.MapGet("", ListarSeriesAsync)
            .WithSummary("Lista las series de la empresa activa.")
            .RequireAuthorization();

        series.MapPost("", CrearSerieAsync)
            .WithSummary("Crea una serie de numeración.")
            .RequierePermiso(Permisos.EmpresaAjustes);

        series.MapDelete("/{id:guid}", async (Guid id, EliminarSerie caso, CancellationToken ct) =>
                (await caso.EjecutarAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Elimina una serie que aún no ha numerado ningún documento.")
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

        var formasPago = rutas.MapGroup("/formas-pago").WithTags("Formas de pago");

        formasPago.MapGet("", ListarFormasPagoAsync)
            .WithSummary("Lista las formas de pago (modalidades: contado, aplazada…).")
            .RequireAuthorization();

        formasPago.MapPost("", CrearFormaPagoAsync)
            .WithSummary("Crea una forma de pago.")
            .RequierePermiso(Permisos.EmpresaAjustes);

        formasPago.MapPut("/{id:guid}", ActualizarFormaPagoAsync)
            .WithSummary("Actualiza una forma de pago.")
            .RequierePermiso(Permisos.EmpresaAjustes);

        formasPago.MapDelete("/{id:guid}", EliminarFormaPagoAsync)
            .WithSummary("Desactiva una forma de pago.")
            .RequierePermiso(Permisos.EmpresaAjustes);

        return rutas;
    }

    private static async Task<IResult> ListarFormasPagoAsync(IContextoEmpresa contexto, ListarFormasPago caso, CancellationToken ct)
        => contexto.EmpresaId is null
            ? ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."))
            : Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, false, ct).ConfigureAwait(false));

    private static async Task<IResult> CrearFormaPagoAsync(DatosFormaPago datos, IContextoEmpresa contexto, GuardarFormaPago caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var r = await caso.EjecutarAsync(contexto.EmpresaId.Value, null, datos, ct).ConfigureAwait(false);
        return r.EsCorrecto ? r.ACreado("/formas-pago") : ResultadosHttp.AProblema(r.Error);
    }

    private static async Task<IResult> ActualizarFormaPagoAsync(Guid id, DatosFormaPago datos, IContextoEmpresa contexto, GuardarFormaPago caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var r = await caso.EjecutarAsync(contexto.EmpresaId.Value, id, datos, ct).ConfigureAwait(false);
        return r.EsCorrecto ? Results.Ok(r.Valor) : ResultadosHttp.AProblema(r.Error);
    }

    private static async Task<IResult> EliminarFormaPagoAsync(Guid id, IContextoEmpresa contexto, EliminarFormaPago caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var r = await caso.EjecutarAsync(contexto.EmpresaId.Value, id, ct).ConfigureAwait(false);
        return r.EsCorrecto ? Results.NoContent() : ResultadosHttp.AProblema(r.Error);
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

    private static async Task<IResult> CrearAsync(CrearEmpresaPeticion peticion, ClaimsPrincipal usuario, CrearEmpresa caso, AlxorCore.Api.Comun.UnionGrupo union,
        AlxorCore.Organizacion.Infraestructura.Persistencia.OrganizacionDbContext db, ActualizarTerritorioFiscal territorio,
        ActualizarPlantillaDocumento plantilla, ActualizarDatosCobro cobro, CambiarPlan plan, ActualizarMetodoValoracion valoracion,
        ActualizarControlRiesgo riesgo, PerfilFiscalEmpresa perfil, ConfigurarProrrata prorrata, CancellationToken ct)
    {
        var usuarioId = usuario.ObtenerUsuarioId();
        if (usuarioId is null)
        {
            return ResultadosHttp.AProblema(Error.NoAutenticado("auth.token_invalido", "El token no identifica al usuario."));
        }

        // Entrar en un grupo da acceso a sus clientes, proveedores y artículos: solo quien gestiona una de sus empresas.
        if (peticion.GrupoId is { } grupo && !await union.PuedeGestionarGrupoAsync(usuarioId.Value, grupo, ct).ConfigureAwait(false))
        {
            return ResultadosHttp.AProblema(Error.Prohibido("grupo.sin_acceso", "Solo puedes crear la empresa en un grupo en el que gestionas alguna empresa."));
        }

        var iban = string.IsNullOrWhiteSpace(peticion.Iban) ? null : new string(peticion.Iban.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        if (iban is not null && !IbanValido(iban))
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.iban_invalido", "El IBAN no es válido: revisa los dígitos."));
        }

        var comando = new CrearEmpresaComando(
            usuarioId.Value, peticion.Nif, peticion.RazonSocial,
            peticion.Calle, peticion.CodigoPostal, peticion.Poblacion, peticion.Provincia, peticion.RegimenIva, peticion.GrupoId);

        // El alta y el resto de datos de la empresa van juntos: si alguno no vale, no se crea nada.
        await using var tx = await db.Database.BeginTransactionAsync(ct).ConfigureAwait(false);
        var resultado = await caso.EjecutarAsync(comando, ct).ConfigureAwait(false);
        if (resultado.EsFallo)
        {
            return ResultadosHttp.AProblema(resultado.Error);
        }

        var id = resultado.Valor.Id;
        var pasos = new List<Func<Task<Resultado>>>();
        if (peticion.TerritorioFiscal is { } t)
        {
            pasos.Add(async () => await territorio.EjecutarAsync(id, new TerritorioFiscalComando(t), ct).ConfigureAwait(false));
        }

        if (peticion.Telefono is not null || peticion.Email is not null || peticion.Web is not null)
        {
            pasos.Add(async () => await plantilla.EjecutarAsync(id, new PlantillaDocumentoComando(peticion.RazonSocial, peticion.Calle, peticion.CodigoPostal,
                peticion.Poblacion, peticion.Provincia, Vacio(peticion.Telefono), Vacio(peticion.Web), Vacio(peticion.Email), null, null, null), ct).ConfigureAwait(false));
        }

        if (iban is not null || !string.IsNullOrWhiteSpace(peticion.IdentificadorAcreedor))
        {
            pasos.Add(async () => await cobro.EjecutarAsync(id, new DatosCobroComando(iban, Vacio(peticion.IdentificadorAcreedor)?.ToUpperInvariant()), ct).ConfigureAwait(false));
        }

        if (!string.IsNullOrWhiteSpace(peticion.Edicion))
        {
            pasos.Add(async () => await plan.EjecutarAsync(id, new CambiarPlanComando(peticion.Edicion, peticion.ModulosAdicionales), ct).ConfigureAwait(false));
        }

        if (peticion.MetodoValoracion is { } mv)
        {
            pasos.Add(async () => await valoracion.EjecutarAsync(id, new MetodoValoracionComando(mv), ct).ConfigureAwait(false));
        }

        if (peticion.ControlRiesgo is { } cr)
        {
            pasos.Add(async () => await riesgo.EjecutarAsync(id, new ControlRiesgoComando(cr), ct).ConfigureAwait(false));
        }

        if (peticion.PerfilFiscal is { } pf)
        {
            pasos.Add(async () => await perfil.GuardarAsync(id, pf, ct).ConfigureAwait(false));
        }

        if (peticion.Prorrata is { Regimen: not null } pr)
        {
            pasos.Add(async () =>
            {
                // La prorrata es de la empresa nueva: la seguridad por empresa de la base de datos tiene que verla ya como activa.
                await db.Database.ExecuteSqlInterpolatedAsync($"SELECT set_config('app.empresa_actual', {id.ToString("D")}, true)", ct).ConfigureAwait(false);
                return await prorrata.EjecutarAsync(id, DateTime.Today.Year, new ConfigurarProrrataComando(pr.Regimen, pr.PorcentajeProvisional, pr.Impuesto), ct).ConfigureAwait(false);
            });
        }

        foreach (var paso in pasos)
        {
            var r = await paso().ConfigureAwait(false);
            if (r.EsFallo)
            {
                return ResultadosHttp.AProblema(r.Error);
            }
        }

        await tx.CommitAsync(ct).ConfigureAwait(false);
        return resultado.ACreado("/empresas/actual");
    }

    private static string? Vacio(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    /// <summary>IBAN con su control ISO 13616 (mod 97 = 1).</summary>
    private static bool IbanValido(string iban)
    {
        if (iban.Length is < 15 or > 34 || !char.IsLetter(iban[0]) || !char.IsLetter(iban[1]) || !char.IsDigit(iban[2]) || !char.IsDigit(iban[3]))
        {
            return false;
        }

        var resto = 0;
        foreach (var c in iban[4..] + iban[..4])
        {
            var v = char.IsDigit(c) ? c - '0' : char.IsLetter(c) ? c - 'A' + 10 : -1;
            if (v < 0)
            {
                return false;
            }

            resto = v >= 10 ? (resto * 100 + v) % 97 : (resto * 10 + v) % 97;
        }

        return resto == 1;
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

    private static async Task<IResult> PlanAsync(IContextoEmpresa contexto, ConsultarPlan caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return (await caso.EjecutarAsync(contexto.EmpresaId.Value, ct).ConfigureAwait(false)).AOk();
    }

    private static async Task<IResult> CambiarPlanAsync(CambiarPlanComando comando, IContextoEmpresa contexto, CambiarPlan caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return (await caso.EjecutarAsync(contexto.EmpresaId.Value, comando, ct).ConfigureAwait(false)).AOk();
    }

    private static async Task<IResult> TerritorioFiscalAsync(TerritorioFiscalComando comando, IContextoEmpresa contexto, ActualizarTerritorioFiscal caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return (await caso.EjecutarAsync(contexto.EmpresaId.Value, comando, ct).ConfigureAwait(false)).AOk();
    }

    private static async Task<IResult> ControlRiesgoAsync(ControlRiesgoComando comando, IContextoEmpresa contexto, ActualizarControlRiesgo caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return (await caso.EjecutarAsync(contexto.EmpresaId.Value, comando, ct).ConfigureAwait(false)).AOk();
    }

    private static async Task<IResult> PlantillaAsync(PlantillaDocumentoComando comando, IContextoEmpresa contexto, ActualizarPlantillaDocumento caso, CancellationToken ct)
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
