using AlxorCore.Api.Comun;
using AlxorCore.Contabilidad.Aplicacion;
using AlxorCore.Contabilidad.Dominio;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Api.Endpoints;

/// <summary>Petición para cambiar el modo de contabilidad de la empresa.</summary>
public sealed record CambiarModoContabilidadPeticion(ModoContabilidad Modo);

/// <summary>Petición para activar/desactivar la contabilización automática.</summary>
public sealed record ContabilizacionAutomaticaPeticion(bool Automatica);

/// <summary>Petición para cambiar la fecha de registro de un documento pendiente.</summary>
public sealed record CambiarFechaRegistroPeticion(DateOnly Fecha);

/// <summary>Petición para contabilizar varios documentos pendientes de una vez.</summary>
public sealed record ContabilizarPendientesPeticion(IReadOnlyList<Guid> Ids);

/// <summary>Petición para asignar/editar la subcuenta de un tercero.</summary>
public sealed record AsignarSubcuentaPeticion(TipoTerceroContable Tipo, Guid TerceroId, string Nombre, string? CuentaPreferida);

/// <summary>Petición para cambiar la longitud de las subcuentas de tercero.</summary>
public sealed record LongitudSubcuentaPeticion(int Longitud);

/// <summary>Endpoints REST del módulo Contabilidad (partida doble).</summary>
public static class EndpointsContabilidad
{
    public static IEndpointRouteBuilder MapearContabilidad(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        var grupo = rutas.MapGroup("/contabilidad").WithTags("Contabilidad");

        grupo.MapGet("/cuentas", ListarCuentasAsync)
            .WithSummary("Lista el plan de cuentas de la empresa.")
            .RequierePermiso(Permisos.ContabilidadLeer);

        grupo.MapGet("/diario", DiarioAsync)
            .WithSummary("Libro diario: asientos del ejercicio.")
            .RequierePermiso(Permisos.ContabilidadLeer);

        grupo.MapGet("/mayor/{codigo}", MayorAsync)
            .WithSummary("Libro mayor de una cuenta.")
            .RequierePermiso(Permisos.ContabilidadLeer);

        grupo.MapGet("/balance", BalanceAsync)
            .WithSummary("Balance de sumas y saldos del ejercicio.")
            .RequierePermiso(Permisos.ContabilidadLeer);

        grupo.MapGet("/pyg", PyGAsync)
            .WithSummary("Cuenta de Pérdidas y Ganancias del ejercicio.")
            .RequierePermiso(Permisos.ContabilidadLeer);

        grupo.MapGet("/balance-situacion", BalanceSituacionAsync)
            .WithSummary("Balance de situación clasificado por masas del ejercicio.")
            .RequierePermiso(Permisos.ContabilidadLeer);

        grupo.MapGet("/cuentas-anuales", CuentasAnualesAsync)
            .WithSummary("Cuentas Anuales normalizadas (balance y PyG, PGC-Pymes).")
            .RequierePermiso(Permisos.ContabilidadLeer);

        grupo.MapGet("/modelo-200", Modelo200Async)
            .WithSummary("Liquidación del Impuesto de Sociedades (modelo 200) desde la contabilidad.")
            .RequierePermiso(Permisos.ContabilidadLeer);

        grupo.MapPost("/cierre", CierreAsync)
            .WithSummary("Cierra el ejercicio (regularización + cierre + apertura del siguiente).")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        grupo.MapPost("/asientos", CrearAsientoAsync)
            .WithSummary("Crea un asiento manual (debe la suma del debe = suma del haber).")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        grupo.MapGet("/modo", ModoAsync)
            .WithSummary("Modo de contabilidad de la empresa (Simple / Completo).")
            .RequierePermiso(Permisos.ContabilidadLeer);

        grupo.MapPut("/modo", CambiarModoAsync)
            .WithSummary("Cambia el modo de contabilidad de la empresa.")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        grupo.MapGet("/config", ConfigAsync)
            .WithSummary("Configuración contable: modo y contabilización automática.")
            .RequierePermiso(Permisos.ContabilidadLeer);

        grupo.MapPut("/contabilizacion-automatica", ContabilizacionAutomaticaAsync)
            .WithSummary("Activa o desactiva la contabilización automática (por defecto: diferida).")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        grupo.MapGet("/pendientes", PendientesAsync)
            .WithSummary("Documentos pendientes de contabilizar (panel del contable).")
            .RequierePermiso(Permisos.ContabilidadLeer);

        grupo.MapPut("/pendientes/{id:guid}/fecha-registro", FechaRegistroAsync)
            .WithSummary("Cambia la fecha de registro de un documento pendiente (típico en recibidas).")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        grupo.MapPost("/pendientes/contabilizar", ContabilizarPendientesAsync)
            .WithSummary("Contabiliza (genera el asiento de) los documentos pendientes indicados.")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        grupo.MapGet("/subcuenta-siguiente", SubcuentaSiguienteAsync)
            .WithSummary("Sugiere la siguiente subcuenta contable para un tipo de tercero (código siguiente).")
            .RequierePermiso(Permisos.ContabilidadLeer);

        grupo.MapGet("/subcuenta", SubcuentaTerceroAsync)
            .WithSummary("Subcuenta contable asignada a un tercero (o la raíz común en modo Simple).")
            .RequierePermiso(Permisos.ContabilidadLeer);

        grupo.MapGet("/subcuentas", SubcuentasAsync)
            .WithSummary("Subcuentas individuales asignadas a los terceros de un tipo (para los listados).")
            .RequierePermiso(Permisos.ContabilidadLeer);

        grupo.MapPut("/subcuenta", AsignarSubcuentaAsync)
            .WithSummary("Asigna o edita la subcuenta contable de un tercero (autonumerada o manual).")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        grupo.MapPut("/longitud-subcuenta", LongitudSubcuentaAsync)
            .WithSummary("Cambia la longitud de las subcuentas de tercero de la empresa.")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        grupo.MapGet("/reglas", ReglasAsync)
            .WithSummary("Reglas de contabilización (cuenta por familia / tipo de tercero / combinación).")
            .RequierePermiso(Permisos.ContabilidadLeer);

        grupo.MapPost("/reglas", CrearReglaAsync)
            .WithSummary("Crea una regla de contabilización.")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        grupo.MapPut("/reglas/{id:guid}", ActualizarReglaAsync)
            .WithSummary("Actualiza una regla de contabilización.")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        grupo.MapDelete("/reglas/{id:guid}", EliminarReglaAsync)
            .WithSummary("Elimina una regla de contabilización.")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        return rutas;
    }

    private static int Ejercicio(int? ejercicio, IReloj reloj) => ejercicio ?? reloj.AhoraUtc.Year;

    private static async Task<IResult> ListarCuentasAsync(IContextoEmpresa contexto, ListarCuentas caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> DiarioAsync(int? ejercicio, IContextoEmpresa contexto, ListarDiario caso, IReloj reloj, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, Ejercicio(ejercicio, reloj), ct).ConfigureAwait(false));
    }

    private static async Task<IResult> MayorAsync(string codigo, int? ejercicio, IContextoEmpresa contexto, MayorCuenta caso, IReloj reloj, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, Ejercicio(ejercicio, reloj), codigo, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> BalanceAsync(int? ejercicio, IContextoEmpresa contexto, BalanceSumasYSaldos caso, IReloj reloj, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, Ejercicio(ejercicio, reloj), ct).ConfigureAwait(false));
    }

    private static async Task<IResult> PyGAsync(int? ejercicio, IContextoEmpresa contexto, GenerarPerdidasGanancias caso, IReloj reloj, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, Ejercicio(ejercicio, reloj), ct).ConfigureAwait(false));
    }

    private static async Task<IResult> BalanceSituacionAsync(int? ejercicio, IContextoEmpresa contexto, GenerarBalanceSituacion caso, IReloj reloj, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, Ejercicio(ejercicio, reloj), ct).ConfigureAwait(false));
    }

    private static async Task<IResult> CuentasAnualesAsync(int? ejercicio, IContextoEmpresa contexto, GenerarCuentasAnuales caso, IReloj reloj, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, Ejercicio(ejercicio, reloj), ct).ConfigureAwait(false));
    }

    private static async Task<IResult> Modelo200Async(int? ejercicio, decimal? tipo, decimal? ajustesAumentos, decimal? ajustesDisminuciones,
        decimal? deducciones, decimal? retenciones, IContextoEmpresa contexto, GenerarCuentasAnuales caso, IReloj reloj, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var resultado = await caso.LiquidacionAsync(contexto.EmpresaId.Value, Ejercicio(ejercicio, reloj),
            tipo ?? 0.25m, ajustesAumentos ?? 0m, ajustesDisminuciones ?? 0m, deducciones ?? 0m, retenciones, ct).ConfigureAwait(false);
        return Results.Ok(resultado);
    }

    private static async Task<IResult> CierreAsync(int? ejercicio, IContextoEmpresa contexto, CerrarEjercicio caso, IReloj reloj, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var resultado = await caso.EjecutarAsync(contexto.EmpresaId.Value, Ejercicio(ejercicio, reloj), ct).ConfigureAwait(false);
        return resultado.EsCorrecto ? Results.Ok(resultado.Valor) : ResultadosHttp.AProblema(resultado.Error);
    }

    private static async Task<IResult> CrearAsientoAsync(CrearAsientoComando comando, IContextoEmpresa contexto, CrearAsiento caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var resultado = await caso.EjecutarAsync(contexto.EmpresaId.Value, comando, ct).ConfigureAwait(false);
        return resultado.EsCorrecto ? resultado.ACreado($"/contabilidad/diario?ejercicio={resultado.Valor.Ejercicio}") : ResultadosHttp.AProblema(resultado.Error);
    }

    private static async Task<IResult> ModoAsync(IContextoEmpresa contexto, ObtenerModoContabilidad caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var modo = await caso.EjecutarAsync(contexto.EmpresaId.Value, ct).ConfigureAwait(false);
        return Results.Ok(new { modo = modo.ToString() });
    }

    private static async Task<IResult> CambiarModoAsync(CambiarModoContabilidadPeticion peticion, IContextoEmpresa contexto, CambiarModoContabilidad caso, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(peticion);
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var modo = await caso.EjecutarAsync(contexto.EmpresaId.Value, peticion.Modo, ct).ConfigureAwait(false);
        return Results.Ok(new { modo = modo.ToString() });
    }

    private static async Task<IResult> ConfigAsync(IContextoEmpresa contexto, ObtenerConfigContabilidad caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> ContabilizacionAutomaticaAsync(ContabilizacionAutomaticaPeticion peticion, IContextoEmpresa contexto, CambiarContabilizacionAutomatica caso, ObtenerConfigContabilidad config, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(peticion);
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        await caso.EjecutarAsync(contexto.EmpresaId.Value, peticion.Automatica, ct).ConfigureAwait(false);
        return Results.Ok(await config.EjecutarAsync(contexto.EmpresaId.Value, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> PendientesAsync(IContextoEmpresa contexto, ListarPendientesContabilizar caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> FechaRegistroAsync(Guid id, CambiarFechaRegistroPeticion peticion, IContextoEmpresa contexto, CambiarFechaRegistro caso, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(peticion);
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var resultado = await caso.EjecutarAsync(id, peticion.Fecha, ct).ConfigureAwait(false);
        return resultado.EsCorrecto ? Results.NoContent() : ResultadosHttp.AProblema(resultado.Error);
    }

    private static async Task<IResult> ContabilizarPendientesAsync(ContabilizarPendientesPeticion peticion, IContextoEmpresa contexto, ContabilizarPendientes caso, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(peticion);
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var resultado = await caso.EjecutarAsync(contexto.EmpresaId.Value, peticion.Ids ?? Array.Empty<Guid>(), ct).ConfigureAwait(false);
        return resultado.EsCorrecto ? Results.Ok(new { contabilizados = resultado.Valor }) : ResultadosHttp.AProblema(resultado.Error);
    }

    private static async Task<IResult> SubcuentaSiguienteAsync(TipoTerceroContable tipo, IContextoEmpresa contexto, ObtenerSiguienteSubcuenta caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, tipo, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> SubcuentaTerceroAsync(TipoTerceroContable tipo, Guid terceroId, IContextoEmpresa contexto, ObtenerSubcuentaTercero caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, tipo, terceroId, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> SubcuentasAsync(TipoTerceroContable tipo, IContextoEmpresa contexto, ListarSubcuentasTercero caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, tipo, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> AsignarSubcuentaAsync(AsignarSubcuentaPeticion peticion, IContextoEmpresa contexto, AsignarSubcuentaTercero caso, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(peticion);
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var resultado = await caso.EjecutarAsync(contexto.EmpresaId.Value,
            new AsignarSubcuentaComando(peticion.Tipo, peticion.TerceroId, peticion.Nombre, peticion.CuentaPreferida), ct).ConfigureAwait(false);
        return resultado.EsCorrecto ? Results.Ok(resultado.Valor) : ResultadosHttp.AProblema(resultado.Error);
    }

    private static async Task<IResult> LongitudSubcuentaAsync(LongitudSubcuentaPeticion peticion, IContextoEmpresa contexto, CambiarLongitudSubcuenta caso, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(peticion);
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var longitud = await caso.EjecutarAsync(contexto.EmpresaId.Value, peticion.Longitud, ct).ConfigureAwait(false);
        return Results.Ok(new { longitudSubcuenta = longitud });
    }

    private static async Task<IResult> ReglasAsync(IContextoEmpresa contexto, ListarReglasContabilizacion caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> CrearReglaAsync(DatosReglaContabilizacion datos, IContextoEmpresa contexto, GuardarReglaContabilizacion caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var resultado = await caso.EjecutarAsync(contexto.EmpresaId.Value, null, datos, ct).ConfigureAwait(false);
        return resultado.EsCorrecto ? resultado.ACreado("/contabilidad/reglas") : ResultadosHttp.AProblema(resultado.Error);
    }

    private static async Task<IResult> ActualizarReglaAsync(Guid id, DatosReglaContabilizacion datos, IContextoEmpresa contexto, GuardarReglaContabilizacion caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var resultado = await caso.EjecutarAsync(contexto.EmpresaId.Value, id, datos, ct).ConfigureAwait(false);
        return resultado.EsCorrecto ? Results.Ok(resultado.Valor) : ResultadosHttp.AProblema(resultado.Error);
    }

    private static async Task<IResult> EliminarReglaAsync(Guid id, IContextoEmpresa contexto, EliminarReglaContabilizacion caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var resultado = await caso.EjecutarAsync(contexto.EmpresaId.Value, id, ct).ConfigureAwait(false);
        return resultado.EsCorrecto ? Results.NoContent() : ResultadosHttp.AProblema(resultado.Error);
    }
}
