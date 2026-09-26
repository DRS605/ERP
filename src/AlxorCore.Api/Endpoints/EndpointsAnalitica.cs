using AlxorCore.Api.Comun;
using AlxorCore.Contabilidad.Aplicacion;
using AlxorCore.Contabilidad.Dominio;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Api.Endpoints;

/// <summary>
/// Contabilidad analítica (módulo «analitica»): centros y partidas, claves de reparto, reglas,
/// periodos, imputación de apuntes, procesos (imputar pendientes, reparto secundario, deshacer) e
/// informe de resultados por centro o partida.
/// </summary>
public static class EndpointsAnalitica
{
    public sealed record PeticionImputacion(IReadOnlyList<LineaImputacionComando> Lineas);

    public sealed record PeticionPeriodoCerrado(bool Cerrado);

    public sealed record PeticionImputarPendientes(DateOnly Desde, DateOnly Hasta);

    public static IEndpointRouteBuilder MapearAnalitica(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var g = rutas.MapGroup("/contabilidad/analitica").WithTags("Analítica");

        g.MapGet("/centros", async (MaestrosAnaliticos caso, CancellationToken ct) => Results.Ok(await caso.CentrosAsync(ct).ConfigureAwait(false)))
            .WithSummary("Centros analíticos (dónde: centro de coste, proyecto, departamento, campaña, finca…), en árbol.")
            .RequierePermiso(Permisos.ContabilidadLeer);
        g.MapPost("/centros", async (DatosCentroAnalitico datos, IContextoEmpresa contexto, MaestrosAnaliticos caso, CancellationToken ct) =>
                contexto.GrupoId is not { } grupo ? SinEmpresa() : Creado(await caso.CrearCentroAsync(grupo, datos, ct).ConfigureAwait(false), "centros"))
            .WithSummary("Crea un centro analítico (compartido por las empresas del grupo).")
            .RequierePermiso(Permisos.ContabilidadGestionar);
        g.MapPut("/centros/{id:guid}", async (Guid id, DatosCentroAnalitico datos, MaestrosAnaliticos caso, CancellationToken ct) =>
                (await caso.ActualizarCentroAsync(id, datos, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Modifica un centro (nombre, tipo, padre, activo). El código no cambia.")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        g.MapGet("/partidas", async (MaestrosAnaliticos caso, CancellationToken ct) => Results.Ok(await caso.PartidasAsync(ct).ConfigureAwait(false)))
            .WithSummary("Partidas analíticas (qué: naturaleza del coste o del ingreso), en árbol.")
            .RequierePermiso(Permisos.ContabilidadLeer);
        g.MapPost("/partidas", async (DatosPartidaAnalitica datos, IContextoEmpresa contexto, MaestrosAnaliticos caso, CancellationToken ct) =>
                contexto.GrupoId is not { } grupo ? SinEmpresa() : Creado(await caso.CrearPartidaAsync(grupo, datos, ct).ConfigureAwait(false), "partidas"))
            .WithSummary("Crea una partida analítica.")
            .RequierePermiso(Permisos.ContabilidadGestionar);
        g.MapPut("/partidas/{id:guid}", async (Guid id, DatosPartidaAnalitica datos, MaestrosAnaliticos caso, CancellationToken ct) =>
                (await caso.ActualizarPartidaAsync(id, datos, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Modifica una partida (nombre, padre, activa).")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        g.MapGet("/claves", async (MaestrosAnaliticos caso, CancellationToken ct) => Results.Ok(await caso.ClavesAsync(ct).ConfigureAwait(false)))
            .WithSummary("Claves de reparto entre centros.")
            .RequierePermiso(Permisos.ContabilidadLeer);
        g.MapPost("/claves", async (DatosClaveReparto datos, IContextoEmpresa contexto, MaestrosAnaliticos caso, CancellationToken ct) =>
                contexto.GrupoId is not { } grupo ? SinEmpresa() : Creado(await caso.CrearClaveAsync(grupo, datos, ct).ConfigureAwait(false), "claves"))
            .WithSummary("Crea una clave de reparto (los porcentajes deben sumar 100).")
            .RequierePermiso(Permisos.ContabilidadGestionar);
        g.MapPut("/claves/{id:guid}", async (Guid id, DatosClaveReparto datos, MaestrosAnaliticos caso, CancellationToken ct) =>
                (await caso.ActualizarClaveAsync(id, datos, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Modifica una clave de reparto.")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        g.MapGet("/reglas", async (IContextoEmpresa contexto, MaestrosAnaliticos caso, CancellationToken ct) =>
                contexto.EmpresaId is not { } e ? SinEmpresa() : Results.Ok(await caso.ReglasAsync(e, ct).ConfigureAwait(false)))
            .WithSummary("Reglas de asignación, de la más a la menos específica (el orden en que se aplican).")
            .RequierePermiso(Permisos.ContabilidadLeer);
        g.MapPost("/reglas", async (DatosReglaAnalitica datos, IContextoEmpresa contexto, MaestrosAnaliticos caso, CancellationToken ct) =>
                contexto.EmpresaId is not { } e ? SinEmpresa() : Creado(await caso.GuardarReglaAsync(e, null, datos, ct).ConfigureAwait(false), "reglas"))
            .WithSummary("Crea una regla: cuenta (prefijo), tercero, actividad y/o familia, con vigencia, → centro o clave y partida.")
            .RequierePermiso(Permisos.ContabilidadGestionar);
        g.MapPut("/reglas/{id:guid}", async (Guid id, DatosReglaAnalitica datos, IContextoEmpresa contexto, MaestrosAnaliticos caso, CancellationToken ct) =>
                contexto.EmpresaId is not { } e ? SinEmpresa() : (await caso.GuardarReglaAsync(e, id, datos, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Modifica una regla (no cambia lo ya imputado).")
            .RequierePermiso(Permisos.ContabilidadGestionar);
        g.MapDelete("/reglas/{id:guid}", async (Guid id, MaestrosAnaliticos caso, CancellationToken ct) =>
                (await caso.EliminarReglaAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Borra una regla (no cambia lo ya imputado).")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        g.MapGet("/periodos", async (IContextoEmpresa contexto, MaestrosAnaliticos caso, CancellationToken ct) =>
                contexto.EmpresaId is not { } e ? SinEmpresa() : Results.Ok(await caso.PeriodosAsync(e, ct).ConfigureAwait(false)))
            .WithSummary("Periodos analíticos (campañas u otros ejercicios de gestión).")
            .RequierePermiso(Permisos.ContabilidadLeer);
        g.MapPost("/periodos", async (DatosPeriodoAnalitico datos, IContextoEmpresa contexto, MaestrosAnaliticos caso, CancellationToken ct) =>
                contexto.EmpresaId is not { } e ? SinEmpresa() : Creado(await caso.CrearPeriodoAsync(e, datos, ct).ConfigureAwait(false), "periodos"))
            .WithSummary("Crea un periodo analítico (no puede solaparse con otro).")
            .RequierePermiso(Permisos.ContabilidadGestionar);
        g.MapPut("/periodos/{id:guid}/cierre", async (Guid id, PeticionPeriodoCerrado peticion, MaestrosAnaliticos caso, CancellationToken ct) =>
                (await caso.CerrarPeriodoAsync(id, peticion.Cerrado, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Cierra (o reabre) un periodo: cerrado, sus imputaciones no se pueden cambiar.")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        g.MapGet("/pendientes", async (DateOnly desde, DateOnly hasta, IContextoEmpresa contexto, ImputacionesAnaliticas caso, CancellationToken ct) =>
                contexto.EmpresaId is not { } e ? SinEmpresa() : Results.Ok(await caso.PendientesAsync(e, desde, hasta, ct).ConfigureAwait(false)))
            .WithSummary("Apuntes de gastos e ingresos con importe sin imputar, y la regla que se les aplicaría.")
            .RequierePermiso(Permisos.ContabilidadLeer);
        g.MapPut("/apuntes/{apunteId:guid}", async (Guid apunteId, PeticionImputacion peticion, IContextoEmpresa contexto, ImputacionesAnaliticas caso, CancellationToken ct) =>
                contexto.EmpresaId is not { } e ? SinEmpresa() : (await caso.ImputarApunteAsync(e, apunteId, peticion.Lineas ?? [], ct).ConfigureAwait(false)).AOk())
            .WithSummary("Imputa un apunte a mano por porcentajes o importes (sustituye lo que tuviera; puede quedar parte pendiente).")
            .RequierePermiso(Permisos.ContabilidadGestionar);
        g.MapDelete("/apuntes/{apunteId:guid}", async (Guid apunteId, IContextoEmpresa contexto, ImputacionesAnaliticas caso, CancellationToken ct) =>
                contexto.EmpresaId is not { } e ? SinEmpresa() : (await caso.QuitarAsync(e, apunteId, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Quita la imputación de un apunte (vuelve a pendientes).")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        g.MapPost("/imputar-pendientes", async (PeticionImputarPendientes peticion, IContextoEmpresa contexto, ImputacionesAnaliticas caso, CancellationToken ct) =>
                contexto.EmpresaId is not { } e ? SinEmpresa() : (await caso.ImputarPendientesAsync(e, peticion.Desde, peticion.Hasta, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Aplica las reglas a los apuntes del periodo que no tienen imputación (queda registrado y se puede deshacer).")
            .RequierePermiso(Permisos.ContabilidadGestionar);
        g.MapPost("/repartos", async (RepartoSecundarioComando comando, IContextoEmpresa contexto, ImputacionesAnaliticas caso, CancellationToken ct) =>
                contexto.EmpresaId is not { } e ? SinEmpresa() : (await caso.RepartirAsync(e, comando, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Reparto secundario: traspasa lo imputado a un centro en el periodo a otros centros con una clave.")
            .RequierePermiso(Permisos.ContabilidadGestionar);
        g.MapGet("/ejecuciones", async (IContextoEmpresa contexto, ImputacionesAnaliticas caso, CancellationToken ct) =>
                contexto.EmpresaId is not { } e ? SinEmpresa() : Results.Ok(await caso.EjecucionesAsync(e, ct).ConfigureAwait(false)))
            .WithSummary("Procesos analíticos ejecutados (imputaciones de pendientes y repartos secundarios).")
            .RequierePermiso(Permisos.ContabilidadLeer);
        g.MapDelete("/ejecuciones/{id:guid}", async (Guid id, IContextoEmpresa contexto, ImputacionesAnaliticas caso, CancellationToken ct) =>
                contexto.EmpresaId is not { } e ? SinEmpresa() : (await caso.DeshacerAsync(e, id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Deshace un proceso: borra todas las imputaciones que generó.")
            .RequierePermiso(Permisos.ContabilidadGestionar);

        g.MapGet("/informe", InformeAsync)
            .WithSummary("Cuenta de resultados analítica por centro o por partida (con árbol y lo que queda sin asignar). Periodo por fechas o por periodo analítico.")
            .RequierePermiso(Permisos.ContabilidadLeer);

        return rutas;
    }

    private static IResult SinEmpresa() =>
        ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));

    private static IResult Creado<T>(Resultado<T> r, string ruta) =>
        r.EsCorrecto ? Results.Created($"/contabilidad/analitica/{ruta}", r.Valor) : ResultadosHttp.AProblema(r.Error);

    private static async Task<IResult> InformeAsync(
        IContextoEmpresa contexto, InformeAnalitico caso, MaestrosAnaliticos maestros, CancellationToken ct,
        DateOnly? desde = null, DateOnly? hasta = null, Guid? periodoId = null, DimensionAnalitica dimension = DimensionAnalitica.Centro)
    {
        if (contexto.EmpresaId is not { } empresaId)
        {
            return SinEmpresa();
        }

        if (periodoId is { } pid)
        {
            var periodo = (await maestros.PeriodosAsync(empresaId, ct).ConfigureAwait(false)).FirstOrDefault(p => p.Id == pid);
            if (periodo is null)
            {
                return ResultadosHttp.AProblema(Error.NoEncontrado("periodo.no_encontrado", "El periodo no existe."));
            }

            desde = periodo.Desde;
            hasta = periodo.Hasta;
        }

        var anio = DateTime.UtcNow.Year;
        return Results.Ok(await caso.EjecutarAsync(empresaId, desde ?? new DateOnly(anio, 1, 1), hasta ?? new DateOnly(anio, 12, 31), dimension, ct)
            .ConfigureAwait(false));
    }
}
