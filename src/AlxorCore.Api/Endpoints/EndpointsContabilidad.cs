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

        grupo.MapPost("/cuentas", async (DatosCuenta datos, IContextoEmpresa contexto, GestionCuentas caso, CancellationToken ct) =>
            {
                if (contexto.EmpresaId is null)
                {
                    return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
                }

                var r = await caso.CrearAsync(contexto.EmpresaId.Value, datos, ct).ConfigureAwait(false);
                return r.EsCorrecto ? Results.Created($"/contabilidad/cuentas/{r.Valor.Codigo}", r.Valor) : ResultadosHttp.AProblema(r.Error);
            })
            .WithSummary("Da de alta una cuenta en el plan de la empresa.")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        grupo.MapPut("/cuentas/{codigo}", async (string codigo, DatosCuenta datos, IContextoEmpresa contexto, GestionCuentas caso, CancellationToken ct) =>
                contexto.EmpresaId is null
                    ? ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."))
                    : (await caso.RenombrarAsync(contexto.EmpresaId.Value, codigo, datos?.Nombre, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Renombra una cuenta del plan (el código no cambia).")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        grupo.MapGet("/diario", DiarioAsync)
            .WithSummary("Libro diario: asientos del ejercicio (con ?diario=VEN, solo los de ese diario, por su número en él).")
            .RequierePermiso(Permisos.ContabilidadLeer);

        grupo.MapGet("/plantillas", async (IContextoEmpresa contexto, GestionPlantillasAsiento caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? Results.Ok(await caso.ListarAsync(e, ct).ConfigureAwait(false)) : SinEmpresa())
            .WithSummary("Plantillas de asiento de la empresa (concepto, diario y cuentas por papel de cada sentido u origen).")
            .RequierePermiso(Permisos.ContabilidadLeer);
        grupo.MapGet("/plantillas/esquemas", () => Results.Ok(GestionPlantillasAsiento.Esquemas()))
            .WithSummary("Papeles de cada asiento con la cuenta y el concepto que se usan sin plantilla, y las variables del concepto.")
            .RequierePermiso(Permisos.ContabilidadLeer);
        grupo.MapPost("/plantillas", async (DatosPlantillaAsiento datos, IContextoEmpresa contexto, GestionPlantillasAsiento caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.GuardarAsync(e, null, datos, ct).ConfigureAwait(false)).ACreado("/contabilidad/plantillas") : SinEmpresa())
            .WithSummary("Crea la plantilla de un sentido (o de un origen concreto).")
            .RequierePermiso(Permisos.ContabilidadGestionar);
        grupo.MapPut("/plantillas/{id:guid}", async (Guid id, DatosPlantillaAsiento datos, IContextoEmpresa contexto, GestionPlantillasAsiento caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.GuardarAsync(e, id, datos, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Modifica una plantilla (vale para los asientos que se generen desde ahora).")
            .RequierePermiso(Permisos.ContabilidadGestionar);
        grupo.MapDelete("/plantillas/{id:guid}", async (Guid id, IContextoEmpresa contexto, GestionPlantillasAsiento caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.EliminarAsync(e, id, ct).ConfigureAwait(false)).ASinContenido() : SinEmpresa())
            .WithSummary("Borra una plantilla: sus documentos vuelven a contabilizarse como siempre.")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        grupo.MapGet("/diarios", async (IContextoEmpresa contexto, GestionDiarios caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? Results.Ok(await caso.ListarAsync(e, ct).ConfigureAwait(false)) : SinEmpresa())
            .WithSummary("Diarios de asientos: los de sistema y los propios, con los orígenes que recoge cada uno.")
            .RequierePermiso(Permisos.ContabilidadLeer);
        grupo.MapPost("/diarios", async (DatosDiario datos, IContextoEmpresa contexto, GestionDiarios caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.CrearAsync(e, datos, ct).ConfigureAwait(false)).ACreado("/contabilidad/diarios") : SinEmpresa())
            .WithSummary("Crea un diario propio (puede recoger los asientos de algunos orígenes).")
            .RequierePermiso(Permisos.ContabilidadGestionar);
        grupo.MapPut("/diarios/{id:guid}", async (Guid id, DatosDiario datos, IContextoEmpresa contexto, GestionDiarios caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.ModificarAsync(e, id, datos, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Modifica un diario propio: nombre, orígenes y alta o baja.")
            .RequierePermiso(Permisos.ContabilidadGestionar);
        grupo.MapDelete("/diarios/{id:guid}", async (Guid id, IContextoEmpresa contexto, GestionDiarios caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.EliminarAsync(e, id, ct).ConfigureAwait(false)).ASinContenido() : SinEmpresa())
            .WithSummary("Elimina un diario propio sin asientos (el que tiene asientos se da de baja).")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        grupo.MapGet("/periodificaciones", async (IContextoEmpresa contexto, GestionPeriodificaciones caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? Results.Ok(await caso.ListarAsync(e, ct).ConfigureAwait(false)) : SinEmpresa())
            .WithSummary("Periodificaciones de gastos e ingresos, con sus cuotas mensuales.")
            .RequierePermiso(Permisos.ContabilidadLeer);
        grupo.MapPost("/periodificaciones", async (DatosPeriodificacion datos, IContextoEmpresa contexto, GestionPeriodificaciones caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.CrearAsync(e, datos, ct).ConfigureAwait(false)).ACreado("/contabilidad/periodificaciones") : SinEmpresa())
            .WithSummary("Da de alta una periodificación (con el asiento de reclasificación a 480/485, si se pide).")
            .RequierePermiso(Permisos.ContabilidadGestionar);
        grupo.MapPost("/periodificaciones/generar", async (PeticionGenerarPeriodificaciones peticion, IContextoEmpresa contexto, GestionPeriodificaciones caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.GenerarAsync(e, peticion.Hasta, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Contabiliza las cuotas pendientes hasta una fecha (las de meses cerrados, no).")
            .RequierePermiso(Permisos.ContabilidadGestionar);
        grupo.MapPost("/periodificaciones/{id:guid}/cancelar", async (Guid id, PeticionFechaPeriodificacion peticion, IContextoEmpresa contexto, GestionPeriodificaciones caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.CancelarAsync(e, id, peticion.Fecha, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Termina antes una periodificación: lo pendiente va a resultados de una vez.")
            .RequierePermiso(Permisos.ContabilidadGestionar);
        grupo.MapPost("/periodificaciones/{id:guid}/anular", async (Guid id, PeticionFechaPeriodificacion peticion, IContextoEmpresa contexto, GestionPeriodificaciones caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.AnularAsync(e, id, peticion.Fecha, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Anula todos los asientos de una periodificación con contraasientos en la fecha indicada.")
            .RequierePermiso(Permisos.ContabilidadGestionar);
        grupo.MapDelete("/periodificaciones/{id:guid}", async (Guid id, IContextoEmpresa contexto, GestionPeriodificaciones caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.EliminarAsync(e, id, ct).ConfigureAwait(false)).ASinContenido() : SinEmpresa())
            .WithSummary("Elimina una periodificación sin asientos.")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        grupo.MapGet("/existencias/cuentas", async (IContextoEmpresa contexto, RegularizacionExistencias caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? Results.Ok(await caso.CuentasAsync(e, ct).ConfigureAwait(false)) : SinEmpresa())
            .WithSummary("Cuenta de existencias (grupo 3) y de variación (61x/71x) de cada familia de artículos; sin familia, la de por defecto.")
            .RequierePermiso(Permisos.ContabilidadLeer);
        grupo.MapPut("/existencias/cuentas", async (IReadOnlyList<CuentaExistenciasDto> cuentas, IContextoEmpresa contexto, RegularizacionExistencias caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.FijarCuentasAsync(e, cuentas, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Sustituye las cuentas de existencias por familia.")
            .RequierePermiso(Permisos.ContabilidadGestionar);
        grupo.MapGet("/existencias/{ejercicio:int}", async (int ejercicio, IContextoEmpresa contexto, RegularizacionExistencias caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? Results.Ok(await caso.ConsultarAsync(e, ejercicio, null, ct).ConfigureAwait(false)) : SinEmpresa())
            .WithSummary("Regularización de existencias del ejercicio: la contabilizada o la previsión (saldo inicial del grupo 3 y stock valorado a 31/12).")
            .RequierePermiso(Permisos.ContabilidadLeer);
        grupo.MapPost("/existencias/{ejercicio:int}", async (int ejercicio, PeticionRegularizarExistencias? peticion, IContextoEmpresa contexto, RegularizacionExistencias caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.ContabilizarAsync(e, ejercicio, peticion?.Ajustes, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Contabiliza la regularización de existencias a 31/12 (con valores finales corregidos por cuenta, si se indican).")
            .RequierePermiso(Permisos.ContabilidadGestionar);
        grupo.MapPost("/existencias/{ejercicio:int}/anular", async (int ejercicio, IContextoEmpresa contexto, RegularizacionExistencias caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.AnularAsync(e, ejercicio, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Anula la regularización de existencias con un contraasiento.")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        grupo.MapGet("/periodos", async (int? ejercicio, IContextoEmpresa contexto, CierreMensual caso, IReloj reloj, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? Results.Ok(await caso.EstadoAsync(e, Ejercicio(ejercicio, reloj), ct).ConfigureAwait(false)) : SinEmpresa())
            .WithSummary("Meses del ejercicio: cerrados o abiertos, con sus asientos y documentos pendientes de contabilizar.")
            .RequierePermiso(Permisos.ContabilidadLeer);
        grupo.MapPost("/periodos/cerrar", async (PeticionPeriodo peticion, IContextoEmpresa contexto, CierreMensual caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.CerrarAsync(e, peticion.Anio, peticion.Mes, peticion.Forzar, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Cierra hasta el final de un mes: no se registran asientos con fecha de ese mes o anterior.")
            .RequierePermiso(Permisos.ContabilidadGestionar);
        grupo.MapPost("/periodos/reabrir", async (PeticionPeriodo peticion, IContextoEmpresa contexto, CierreMensual caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.ReabrirAsync(e, peticion.Anio, peticion.Mes, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Reabre desde un mes (y los posteriores).")
            .RequierePermiso(Permisos.ContabilidadGestionar);

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

        grupo.MapGet("/cuentas-anuales/deposito", async (int? ejercicio, IContextoEmpresa contexto, GenerarModeloDeposito caso, IReloj reloj, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? Results.Ok(await caso.EjecutarAsync(e, Ejercicio(ejercicio, reloj), ct).ConfigureAwait(false)) : SinEmpresa())
            .WithSummary("Balance y pérdidas y ganancias abreviados con las claves del modelo de depósito en el Registro Mercantil (ejercicio y anterior).")
            .RequierePermiso(Permisos.ContabilidadLeer);
        grupo.MapGet("/cuentas-anuales/deposito/csv", async (int? ejercicio, IContextoEmpresa contexto, GenerarModeloDeposito caso, IReloj reloj, CancellationToken ct) =>
            {
                if (contexto.EmpresaId is not { } e)
                {
                    return SinEmpresa();
                }

                var m = await caso.EjecutarAsync(e, Ejercicio(ejercicio, reloj), ct).ConfigureAwait(false);
                return Results.File(System.Text.Encoding.UTF8.GetBytes(LegalizacionLibros.DepositoCsv(m)), "text/csv", $"cuentas-anuales-{m.Ejercicio}.csv");
            })
            .WithSummary("Las partidas del modelo de depósito en CSV, para copiarlas en el programa del Registro.")
            .RequierePermiso(Permisos.ContabilidadLeer);
        grupo.MapGet("/legalizacion", async (int? ejercicio, IContextoEmpresa contexto, LibrosLegalizacion caso, AlxorCore.Organizacion.Aplicacion.Puertos.IConsultaEmpresas empresas,
                AlxorCore.Documentos.Aplicacion.IGeneradorPdfLibro pdf, IReloj reloj, CancellationToken ct) =>
            {
                if (contexto.EmpresaId is not { } e)
                {
                    return SinEmpresa();
                }

                var anio = ejercicio ?? reloj.AhoraUtc.Year - 1;
                var libros = await caso.EjecutarAsync(e, anio, ct).ConfigureAwait(false);
                if (libros.Diario.Count == 0)
                {
                    return ResultadosHttp.AProblema(Error.Validacion("legalizacion.sin_asientos", $"El ejercicio {anio} no tiene asientos que legalizar."));
                }

                var emp = await empresas.ObtenerAsync(e, ct).ConfigureAwait(false);
                return Results.File(LegalizacionLibros.Zip(libros, emp?.RazonSocial ?? "Empresa", emp?.Nif ?? string.Empty, pdf), "application/zip", $"legalizacion-libros-{anio}.zip");
            })
            .WithSummary("Paquete para legalizar el Diario y el de Inventarios y Cuentas Anuales en el Registro Mercantil (PDF para Legalia).")
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

        grupo.MapPost("/asientos/{id:guid}/anular", async (Guid id, PeticionAnularAsiento? peticion, IContextoEmpresa contexto, AnularAsiento caso, CancellationToken ct) =>
                contexto.EmpresaId is null
                    ? ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."))
                    : (await caso.EjecutarAsync(contexto.EmpresaId.Value, id, peticion?.Fecha, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Anula un asiento manual con su contraasiento (los de documentos se anulan desde el documento).")
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

    private static async Task<IResult> DiarioAsync(int? ejercicio, string? diario, IContextoEmpresa contexto, ListarDiario caso, IReloj reloj, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, Ejercicio(ejercicio, reloj), diario, ct).ConfigureAwait(false));
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

        var resultado = await caso.EjecutarManualAsync(contexto.EmpresaId.Value, comando, ct).ConfigureAwait(false);
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

    /// <summary>Fecha del contraasiento (por defecto, la del asiento que anula).</summary>
    public sealed record PeticionAnularAsiento(DateOnly? Fecha);

    /// <summary>Mes que se cierra (hasta él) o se reabre (desde él).</summary>
    public sealed record PeticionPeriodo(int Anio, int Mes, bool Forzar = false);

    public sealed record PeticionGenerarPeriodificaciones(DateOnly Hasta);

    public sealed record PeticionFechaPeriodificacion(DateOnly Fecha);

    public sealed record PeticionRegularizarExistencias(IReadOnlyList<AjusteExistencias>? Ajustes);

    private static IResult SinEmpresa() => ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
}
