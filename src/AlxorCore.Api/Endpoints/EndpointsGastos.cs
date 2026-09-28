using AlxorCore.Api.Comun;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Consultas;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Api.Endpoints;

/// <summary>Endpoints REST del módulo Gastos.</summary>
public static class EndpointsGastos
{
    public static IEndpointRouteBuilder MapearGastos(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        var gastos = rutas.MapGroup("/gastos").WithTags("Gastos");

        gastos.MapGet("", ListarAsync)
            .WithSummary("Lista los gastos de la empresa activa.")
            .RequierePermiso(Permisos.GastoLeer);

        gastos.MapGet("/buscar", BuscarAsync)
            .WithSummary("Busca gastos con filtros (texto, estado, fechas, importe, proveedor, estado de pago), orden y paginación; incluye los totales de todo el resultado filtrado y el pendiente de cada gasto de la página.")
            .RequierePermiso(Permisos.GastoLeer);

        gastos.MapPost("/{id:guid}/anular", AnularGastoAsync)
            .WithSummary("Anula un gasto (contraasiento y fuera de los libros de IVA). Antes hay que anular sus pagos.")
            .RequierePermiso(Permisos.GastoGestionar);

        gastos.MapGet("/{id:guid}", ObtenerAsync)
            .WithSummary("Obtiene un gasto.")
            .RequierePermiso(Permisos.GastoLeer);

        gastos.MapPost("", RegistrarAsync)
            .WithSummary("Registra un gasto o factura recibida (líneas con sus impuestos, nº y fecha de la factura del proveedor, vencimientos).")
            .RequierePermiso(Permisos.GastoGestionar);

        gastos.MapPost("/simular", async (RegistrarGastoComando comando, IContextoEmpresa contexto, RegistrarGasto caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e ? (await caso.SimularAsync(e, comando, ct).ConfigureAwait(false)).AOk()
                    : ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero.")))
            .WithSummary("Calcula una factura recibida sin guardarla (impuestos por línea, recargo, retención, total y vencimientos).")
            .RequierePermiso(Permisos.GastoLeer);

        gastos.MapPut("/{id:guid}", ModificarAsync)
            .WithSummary("Corrige una factura recibida sin pagos: contraasiento de lo registrado y asiento de lo nuevo.")
            .RequierePermiso(Permisos.GastoGestionar);

        gastos.MapPut("/{id:guid}/afectacion", async (Guid id, PeticionAfectacion peticion, CambiarAfectacionGasto caso, CancellationToken ct) =>
                (await caso.EjecutarAsync(id, peticion.Afectacion, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Afectación del gasto para la prorrata especial: Comun, ConDerecho (operaciones con derecho a deducir) o SinDerecho (exentas).")
            .RequierePermiso(Permisos.GastoGestionar);

        return rutas;
    }

    /// <summary>Cuerpo para cambiar la afectación de un gasto.</summary>
    public sealed record PeticionAfectacion(AlxorCore.Gastos.Dominio.AfectacionIva Afectacion);

    private static async Task<IResult> ListarAsync(IContextoEmpresa contexto, ListarGastos caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> BuscarAsync(IContextoEmpresa contexto, BuscarGastos caso, IConsultaGastos consulta,
        AlxorCore.Tesoreria.Aplicacion.IConsultaTesoreria tesoreria, AlxorCore.Nucleo.Tiempo.IReloj reloj,
        string? texto, string? estado, DateOnly? desde, DateOnly? hasta, decimal? importeMin, decimal? importeMax, Guid? proveedorId,
        string? cobro, string? orden, bool? desc, int? pagina, int? tamanoPagina, CancellationToken ct)
    {
        if (contexto.EmpresaId is not { } empresaId)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        if (TotalesListados.ValidarCobro(cobro) is { } errorCobro)
        {
            return ResultadosHttp.AProblema(errorCobro);
        }

        // Totales y estado de pago sobre todo el resultado filtrado (no solo sobre la página).
        var filtro = new FiltroGastos(texto, estado, desde, hasta, importeMin, importeMax, proveedorId, Orden: orden, Descendente: desc ?? true);
        var hoy = DateOnly.FromDateTime(reloj.AhoraUtc.UtcDateTime);
        var documentos = (await consulta.FiltradosAsync(empresaId, filtro, ct).ConfigureAwait(false))
            .Select(g => new DocumentoListado(g.Id, g.Estado, g.Vencimiento, g.BaseImponible, g.CuotaIva, g.RetencionIrpf, g.Total)).ToList();
        var (pendientes, filtrados) = await TotalesListados.SaldosAsync(documentos, AlxorCore.Tesoreria.Dominio.TipoDocumentoTesoreria.Gasto, cobro, hoy, tesoreria, ct).ConfigureAwait(false);
        if (filtrados is not null)
        {
            filtro = filtro with { Ids = filtrados };
            var set = filtrados.ToHashSet();
            documentos = documentos.Where(d => set.Contains(d.Id)).ToList();
        }

        var paginaResultado = await caso.EjecutarAsync(empresaId, filtro, Paginacion.Normalizar(pagina, tamanoPagina), ct).ConfigureAwait(false);
        var pendientesPagina = paginaResultado.Elementos.ToDictionary(g => g.Id, g => pendientes.GetValueOrDefault(g.Id));
        return Results.Ok(PaginaConTotales<GastoDto>.Desde(paginaResultado, TotalesListados.Calcular(documentos, pendientes, hoy), pendientesPagina));
    }

    private static async Task<IResult> ObtenerAsync(Guid id, ObtenerGasto caso, CancellationToken ct) =>
        (await caso.EjecutarAsync(id, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> RegistrarAsync(RegistrarGastoComando comando, IContextoEmpresa contexto, System.Security.Claims.ClaimsPrincipal usuario,
        AlxorCore.Organizacion.Aplicacion.CasosDeUso.IConsultaVisibilidad visibilidad, RegistrarGasto caso, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(comando);
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        // Si se elige explícitamente una actividad, el usuario debe tener acceso a ella (área Compras).
        var acceso = await AccesoActividad.ValidarAsync(usuario, visibilidad, AlxorCore.Organizacion.Dominio.AreaVisibilidad.Compras, comando.ActividadNegocioId, ct).ConfigureAwait(false);
        if (acceso is not null)
        {
            return ResultadosHttp.AProblema(acceso);
        }

        var resultado = await caso.EjecutarAsync(contexto.EmpresaId.Value, comando, ct).ConfigureAwait(false);
        return resultado.EsCorrecto ? resultado.ACreado($"/gastos/{resultado.Valor.Id}") : ResultadosHttp.AProblema(resultado.Error);
    }

    private static async Task<IResult> ModificarAsync(Guid id, RegistrarGastoComando comando, ModificarGasto caso, AlxorCore.Tesoreria.Aplicacion.ConsultarSaldo saldo,
        IComprobadorUso uso, CancellationToken ct)
    {
        var s = await saldo.DeGastoAsync(id, ct).ConfigureAwait(false);
        if (s.EsFallo)
        {
            return ResultadosHttp.AProblema(s.Error);
        }

        if (s.Valor.Liquidado != 0m)
        {
            return ResultadosHttp.AProblema(Error.Conflicto("gasto.con_pagos",
                $"La factura tiene pagos por {Redondeo.Formatear(s.Valor.Liquidado)} €: anúlalos antes de corregirla."));
        }

        if (await uso.BuscarUsoAsync(TiposRegistro.Gasto, id, ct).ConfigureAwait(false) is { } origen)
        {
            return ResultadosHttp.AProblema(Error.Conflicto("gasto.de_documento", $"Este gasto es {origen}: se corrige desde allí."));
        }

        return (await caso.EjecutarAsync(id, comando, ct).ConfigureAwait(false)).AOk();
    }

    private static async Task<IResult> AnularGastoAsync(Guid id, AnularGasto caso, AlxorCore.Tesoreria.Aplicacion.ConsultarSaldo saldo, IComprobadorUso uso, CancellationToken ct)
    {
        var s = await saldo.DeGastoAsync(id, ct).ConfigureAwait(false);
        if (s.EsFallo)
        {
            return ResultadosHttp.AProblema(s.Error);
        }

        if (s.Valor.Liquidado > 0m)
        {
            return ResultadosHttp.AProblema(Error.Conflicto("gasto.con_pagos",
                $"El gasto tiene pagos por {Redondeo.Formatear(s.Valor.Liquidado)} €: anúlalos primero (Pagos → Pagos del gasto)."));
        }

        if (await uso.BuscarUsoAsync(TiposRegistro.Gasto, id, ct).ConfigureAwait(false) is { } origen)
        {
            return ResultadosHttp.AProblema(Error.Conflicto("gasto.de_documento", $"Este gasto es {origen}: anúlalo desde allí."));
        }

        var r = await caso.EjecutarAsync(id, ct).ConfigureAwait(false);
        return r.EsCorrecto ? Results.Ok(new { id, anulado = true }) : ResultadosHttp.AProblema(r.Error);
    }
}
