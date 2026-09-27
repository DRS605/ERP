using AlxorCore.Api.Comun;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Tesoreria.Aplicacion;
using AlxorCore.Tesoreria.Dominio;

namespace AlxorCore.Api.Endpoints;

/// <summary>
/// Endpoints de cuentas bancarias y cajas (base: los cobros y pagos eligen su banco), y de la tesorería avanzada:
/// remesas SEPA registradas, devoluciones de recibos y conciliación bancaria persistente.
/// </summary>
public static class EndpointsBancos
{
    public static IEndpointRouteBuilder MapearBancos(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        // ---------------------------------------------------------------- cuentas bancarias y cajas (base)
        var bancos = rutas.MapGroup("/cuentas-bancarias").WithTags("Tesorería · bancos");
        bancos.MapGet("", async (bool? activas, GestionCuentasBancarias caso, CancellationToken ct) =>
                Results.Ok(await caso.ListarAsync(activas ?? false, ct).ConfigureAwait(false)))
            .WithSummary("Cuentas bancarias y cajas de la empresa (?activas=true para las activas).")
            .RequierePermiso(Permisos.FacturaLeer);
        bancos.MapPost("", async (GuardarCuentaBancariaComando comando, IContextoEmpresa contexto, GestionCuentasBancarias caso, CancellationToken ct) =>
            {
                if (contexto.EmpresaId is not { } empresaId)
                {
                    return ResultadosHttp.AProblema(SinEmpresa());
                }

                var r = await caso.CrearAsync(empresaId, comando, ct).ConfigureAwait(false);
                return r.EsCorrecto ? r.ACreado($"/cuentas-bancarias/{r.Valor.Id}") : ResultadosHttp.AProblema(r.Error);
            })
            .WithSummary("Da de alta una cuenta bancaria (IBAN validado) o una caja; su subcuenta 572…/570… se crea en el plan de cuentas.")
            .RequierePermiso(Permisos.EmpresaAjustes);
        bancos.MapPut("/{id:guid}", async (Guid id, GuardarCuentaBancariaComando comando, GestionCuentasBancarias caso, CancellationToken ct) =>
                (await caso.ActualizarAsync(id, comando, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Modifica una cuenta (nombre, IBAN, BIC, activa, predeterminada, saldo inicial). La subcuenta no cambia.")
            .RequierePermiso(Permisos.EmpresaAjustes);
        bancos.MapDelete("/{id:guid}", async (Guid id, GestionCuentasBancarias caso, CancellationToken ct) =>
            {
                var r = await caso.EliminarAsync(id, ct).ConfigureAwait(false);
                return r.EsCorrecto ? Results.NoContent() : ResultadosHttp.AProblema(r.Error);
            })
            .WithSummary("Borra una cuenta sin movimientos, remesas ni extractos (si los tiene, se desactiva).")
            .RequierePermiso(Permisos.EmpresaAjustes);
        bancos.MapGet("/saldos", async (DateOnly? fecha, IContextoEmpresa contexto, GestionCuentasBancarias caso, CancellationToken ct) =>
                contexto.EmpresaId is { } empresaId
                    ? Results.Ok(await caso.SaldosAsync(empresaId, fecha, ct).ConfigureAwait(false))
                    : ResultadosHttp.AProblema(SinEmpresa()))
            .WithSummary("Saldo de cada banco y caja (contable si hay contabilidad completa; si no, por movimientos) y el total.")
            .RequierePermiso(Permisos.FacturaLeer);

        // ---------------------------------------------------------------- remesas registradas
        var remesas = rutas.MapGroup("/tesoreria/remesas").WithTags("Tesorería · remesas");
        remesas.MapGet("", async (TipoRemesa? tipo, GestionRemesas caso, CancellationToken ct) =>
                Results.Ok(await caso.ListarAsync(tipo, ct).ConfigureAwait(false)))
            .WithSummary("Remesas registradas (adeudos o transferencias), de la más reciente a la más antigua.")
            .RequierePermiso(Permisos.FacturaLeer);
        remesas.MapGet("/{id:guid}", async (Guid id, GestionRemesas caso, CancellationToken ct) =>
                (await caso.ObtenerAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Una remesa con sus líneas (y las devueltas).")
            .RequierePermiso(Permisos.FacturaLeer);
        remesas.MapGet("/{id:guid}/fichero", async (Guid id, GestionRemesas caso, CancellationToken ct) =>
                (await caso.FicheroAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("El fichero XML SEPA de la remesa, para volver a descargarlo.")
            .RequierePermiso(Permisos.FacturaLeer);
        remesas.MapPost("", async (CrearRemesaComando comando, IContextoEmpresa contexto, GestionRemesas caso, CancellationToken ct) =>
            {
                if (contexto.EmpresaId is not { } empresaId)
                {
                    return ResultadosHttp.AProblema(SinEmpresa());
                }

                var r = await caso.CrearAsync(empresaId, comando, ct).ConfigureAwait(false);
                return r.EsCorrecto ? r.ACreado($"/tesoreria/remesas/{r.Valor.Remesa.Id}") : ResultadosHttp.AProblema(r.Error);
            })
            .WithSummary("Crea y registra una remesa SEPA: Tipo=Cobro (facturas y efectos a domiciliar) o Pago (gastos a transferir), con su fichero.")
            .RequierePermiso(Permisos.CobroRegistrar);
        remesas.MapPost("/{id:guid}/presentar", async (Guid id, GestionRemesas caso, CancellationToken ct) =>
                (await caso.PresentarAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Marca la remesa como presentada (enviada al banco).")
            .RequierePermiso(Permisos.CobroRegistrar);
        remesas.MapPost("/{id:guid}/liquidar", async (Guid id, LiquidarRemesaComando? comando, IContextoEmpresa contexto, GestionRemesas caso, CancellationToken ct) =>
                contexto.EmpresaId is { } empresaId
                    ? (await caso.LiquidarAsync(empresaId, id, comando ?? new LiquidarRemesaComando(), ct).ConfigureAwait(false)).AOk()
                    : ResultadosHttp.AProblema(SinEmpresa()))
            .WithSummary("Marca la remesa como cobrada o pagada: registra un cobro o pago por línea contra su cuenta bancaria, con su asiento.")
            .RequierePermiso(Permisos.CobroRegistrar);
        remesas.MapPost("/{id:guid}/anular", async (Guid id, GestionRemesas caso, CancellationToken ct) =>
                (await caso.AnularAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Anula una remesa no liquidada (rechazada o por error): sus documentos quedan libres para otra.")
            .RequierePermiso(Permisos.CobroRegistrar);

        // ---------------------------------------------------------------- devoluciones de recibos
        var devoluciones = rutas.MapGroup("/tesoreria/devoluciones").WithTags("Tesorería · devoluciones");
        devoluciones.MapGet("", async (GestionDevoluciones caso, CancellationToken ct) => Results.Ok(await caso.ListarAsync(ct).ConfigureAwait(false)))
            .WithSummary("Recibos devueltos, con su motivo SEPA y sus gastos.")
            .RequierePermiso(Permisos.FacturaLeer);
        devoluciones.MapGet("/motivos", () => Results.Ok(GestionDevoluciones.Motivos()))
            .WithSummary("Motivos de devolución SEPA (códigos R) con su descripción.")
            .RequierePermiso(Permisos.FacturaLeer);
        devoluciones.MapPost("", async (RegistrarDevolucionComando comando, IContextoEmpresa contexto, GestionDevoluciones caso, CancellationToken ct) =>
            {
                if (contexto.EmpresaId is not { } empresaId)
                {
                    return ResultadosHttp.AProblema(SinEmpresa());
                }

                var r = await caso.RegistrarAsync(empresaId, comando, ct).ConfigureAwait(false);
                return r.EsCorrecto ? r.ACreado("/tesoreria/devoluciones") : ResultadosHttp.AProblema(r.Error);
            })
            .WithSummary("Registra la devolución de un recibo domiciliado: anula el cobro (vuelve a quedar pendiente), contraasiento y gastos de devolución.")
            .RequierePermiso(Permisos.CobroRegistrar);

        // ---------------------------------------------------------------- conciliación bancaria persistente
        var extractos = rutas.MapGroup("/tesoreria/extractos").WithTags("Tesorería · conciliación");
        extractos.MapGet("", async (Guid? cuentaBancariaId, ConciliacionBancaria caso, CancellationToken ct) =>
                Results.Ok(await caso.ListarAsync(cuentaBancariaId, ct).ConfigureAwait(false)))
            .WithSummary("Extractos importados (de una cuenta o de todas) con sus apuntes pendientes de conciliar.")
            .RequierePermiso(Permisos.FacturaLeer);
        extractos.MapPost("", async (ImportarExtractoComando comando, IContextoEmpresa contexto, ConciliacionBancaria caso, CancellationToken ct) =>
            {
                if (contexto.EmpresaId is not { } empresaId)
                {
                    return ResultadosHttp.AProblema(SinEmpresa());
                }

                var r = await caso.ImportarAsync(empresaId, comando, ct).ConfigureAwait(false);
                return r.EsCorrecto ? r.ACreado($"/tesoreria/extractos/{r.Valor.Extracto.Id}") : ResultadosHttp.AProblema(r.Error);
            })
            .WithSummary("Importa y guarda un extracto Norma 43 de una cuenta bancaria (no admite el mismo fichero dos veces).")
            .RequierePermiso(Permisos.CobroRegistrar);
        extractos.MapGet("/{id:guid}", async (Guid id, IContextoEmpresa contexto, ConciliacionBancaria caso, CancellationToken ct) =>
                contexto.EmpresaId is { } empresaId
                    ? (await caso.ObtenerAsync(empresaId, id, ct).ConfigureAwait(false)).AOk()
                    : ResultadosHttp.AProblema(SinEmpresa()))
            .WithSummary("Un extracto con sus apuntes, su estado de conciliación y lo casado con cada uno.")
            .RequierePermiso(Permisos.FacturaLeer);
        extractos.MapDelete("/{id:guid}", async (Guid id, ConciliacionBancaria caso, CancellationToken ct) =>
            {
                var r = await caso.EliminarAsync(id, ct).ConfigureAwait(false);
                return r.EsCorrecto ? Results.NoContent() : ResultadosHttp.AProblema(r.Error);
            })
            .WithSummary("Borra un extracto importado por error (sin apuntes conciliados).")
            .RequierePermiso(Permisos.CobroRegistrar);
        extractos.MapPost("/{id:guid}/conciliar-automatico", async (Guid id, ConciliacionAutomaticaComando? comando, IContextoEmpresa contexto, ConciliacionBancaria caso,
                CancellationToken ct) =>
                contexto.EmpresaId is { } empresaId
                    ? (await caso.ConciliarAutomaticoAsync(empresaId, id, comando ?? new ConciliacionAutomaticaComando(), ct).ConfigureAwait(false)).AOk()
                    : ResultadosHttp.AProblema(SinEmpresa()))
            .WithSummary("Concilia automáticamente los apuntes pendientes (importe exacto, margen de fechas y pistas del concepto).")
            .RequierePermiso(Permisos.CobroRegistrar);

        var apuntes = rutas.MapGroup("/tesoreria/apuntes").WithTags("Tesorería · conciliación");
        apuntes.MapGet("/{id:guid}/candidatos", async (Guid id, IContextoEmpresa contexto, ConciliacionBancaria caso, CancellationToken ct) =>
                contexto.EmpresaId is { } empresaId
                    ? (await caso.CandidatosAsync(empresaId, id, ct).ConfigureAwait(false)).AOk()
                    : ResultadosHttp.AProblema(SinEmpresa()))
            .WithSummary("Cobros, pagos, remesas y documentos pendientes que pueden casar con el apunte.")
            .RequierePermiso(Permisos.FacturaLeer);
        apuntes.MapPost("/{id:guid}/conciliar", async (Guid id, ConciliarApunteComando comando, IContextoEmpresa contexto, ConciliacionBancaria caso, CancellationToken ct) =>
                contexto.EmpresaId is { } empresaId
                    ? (await caso.ConciliarManualAsync(empresaId, id, comando, ct).ConfigureAwait(false)).AOk()
                    : ResultadosHttp.AProblema(SinEmpresa()))
            .WithSummary("Concilia a mano el apunte con uno o varios movimientos o documentos cuya suma es la del apunte.")
            .RequierePermiso(Permisos.CobroRegistrar);
        apuntes.MapPost("/{id:guid}/asiento", async (Guid id, AsientoApunteComando comando, IContextoEmpresa contexto, ConciliacionBancaria caso, CancellationToken ct) =>
                contexto.EmpresaId is { } empresaId
                    ? (await caso.AsientoAsync(empresaId, id, comando, ct).ConfigureAwait(false)).AOk()
                    : ResultadosHttp.AProblema(SinEmpresa()))
            .WithSummary("Contabiliza el apunte con un asiento directo contra la cuenta elegida (626 comisiones, 669, 769 intereses…).")
            .RequierePermiso(Permisos.CobroRegistrar);
        apuntes.MapPost("/{id:guid}/deshacer", async (Guid id, IContextoEmpresa contexto, ConciliacionBancaria caso, CancellationToken ct) =>
                contexto.EmpresaId is { } empresaId
                    ? (await caso.DeshacerAsync(empresaId, id, ct).ConfigureAwait(false)).AOk()
                    : ResultadosHttp.AProblema(SinEmpresa()))
            .WithSummary("Deshace la conciliación del apunte (anula lo que registró).")
            .RequierePermiso(Permisos.CobroRegistrar);

        return rutas;
    }

    private static Error SinEmpresa() => Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero.");
}
