using AlxorCore.Api.Comun;
using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Terceros.Aplicacion;
using AlxorCore.Auditoria.Infraestructura;
using AlxorCore.Catalogo.Infraestructura;
using AlxorCore.Facturacion.Infraestructura;
using AlxorCore.Gastos.Infraestructura;
using AlxorCore.Organizacion.Infraestructura.Persistencia;
using AlxorCore.Persistencia;
using AlxorCore.Terceros.Infraestructura;
using AlxorCore.Tesoreria.Infraestructura;
using Microsoft.EntityFrameworkCore;

namespace AlxorCore.Api.Endpoints;

/// <summary>Endpoints de la cuenta/empresa: derechos RGPD (portabilidad y supresión).</summary>
public static class EndpointsCuenta
{
    private static readonly System.Text.Json.JsonSerializerOptions OpcionesExport = CrearOpciones();

    private static System.Text.Json.JsonSerializerOptions CrearOpciones()
    {
        var opciones = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
        opciones.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        return opciones;
    }

    public static IEndpointRouteBuilder MapearCuenta(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        var cuenta = rutas.MapGroup("/cuenta").WithTags("Cuenta / RGPD");

        cuenta.MapGet("/exportar", ExportarAsync)
            .WithSummary("Exporta todos los datos de la empresa activa (RGPD: acceso y portabilidad).")
            .RequierePermiso(Permisos.DatosExportar);

        cuenta.MapDelete("", EliminarAsync)
            .WithSummary("Elimina la empresa activa y todos sus datos (RGPD: derecho de supresión).")
            .RequierePermiso(Permisos.UsuarioGestionar);

        return rutas;
    }

    private static async Task<IResult> ExportarAsync(
        IContextoEmpresa contexto,
        IConsultaEmpresas empresas,
        IConsultaClientes clientes,
        IConsultaProveedores proveedores,
        IConsultaProductos productos,
        IConsultaFacturas facturas,
        IConsultaGastos gastos,
        CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var id = contexto.EmpresaId.Value;
        var datos = new
        {
            generadoEn = DateTimeOffset.UtcNow,
            empresa = await empresas.ObtenerAsync(id, ct).ConfigureAwait(false),
            clientes = await clientes.ListarAsync(id, incluirInactivos: true, ct: ct).ConfigureAwait(false),
            proveedores = await proveedores.ListarAsync(id, incluirInactivos: true, ct: ct).ConfigureAwait(false),
            productos = await productos.ListarAsync(id, incluirInactivos: true, ct: ct).ConfigureAwait(false),
            facturas = await facturas.ListarAsync(id, ct).ConfigureAwait(false),
            gastos = await gastos.ListarAsync(id, ct).ConfigureAwait(false),
        };

        var bytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(datos, OpcionesExport);
        var nombre = $"alxor-export-{DateTime.UtcNow:yyyyMMdd}.json";
        return Results.File(bytes, "application/json", nombre);
    }

    private static async Task<IResult> EliminarAsync(
        IContextoEmpresa contexto,
        FacturacionDbContext facturacion,
        GastosDbContext gastos,
        TesoreriaDbContext tesoreria,
        TercerosDbContext terceros,
        CatalogoDbContext catalogo,
        AuditoriaDbContext auditoria,
        OrganizacionDbContext organizacion,
        AlxorCore.Agro.Infraestructura.AgroDbContext agro,
        AlxorCore.Logistica.Infraestructura.LogisticaDbContext logistica,
        AlxorCore.Cooperativa.Infraestructura.CooperativaDbContext cooperativa,
        AlxorCore.Bodega.Infraestructura.BodegaDbContext bodega,
        AlxorCore.Migracion.Infraestructura.MigracionDbContext migracion,
        AlxorCore.Analisis.Infraestructura.AnalisisDbContext analisis,
        CancellationToken ct)
    {
        if (contexto.EmpresaId is not { } id)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        // Borramos los datos de la empresa en cada módulo. El filtro por empresa (EF + RLS) garantiza
        // que solo se eliminan los de la empresa activa; las líneas (owned) caen en cascada.
        // Las facturas emitidas, los movimientos y la auditoría están protegidos en la base de datos:
        // la baja de la empresa es la única que los puede borrar, y lo declara en su transacción.
        await BorradoEmpresa.EjecutarAsync(facturacion, id, async () =>
        {
            await facturacion.Facturas.Where(f => f.EmpresaId == id).ExecuteDeleteAsync(ct).ConfigureAwait(false);
            await facturacion.FacturasRecurrentes.Where(r => r.EmpresaId == id).ExecuteDeleteAsync(ct).ConfigureAwait(false);
        }, ct).ConfigureAwait(false);
        await gastos.Gastos.Where(g => g.EmpresaId == id).ExecuteDeleteAsync(ct).ConfigureAwait(false);
        await BorradoEmpresa.EjecutarAsync(tesoreria, id, async () =>
        {
            await tesoreria.LiquidacionesPagos.Where(l => l.EmpresaId == id).ExecuteDeleteAsync(ct).ConfigureAwait(false);
            await tesoreria.EntregasCuenta.Where(e => e.EmpresaId == id).ExecuteDeleteAsync(ct).ConfigureAwait(false);
            await tesoreria.Movimientos.Where(m => m.EmpresaId == id).ExecuteDeleteAsync(ct).ConfigureAwait(false);
            await tesoreria.MensajesSalida.Where(m => m.EmpresaId == id).ExecuteDeleteAsync(ct).ConfigureAwait(false);
            await tesoreria.AnulacionesCartera.Where(e => e.EmpresaId == id).ExecuteDeleteAsync(ct).ConfigureAwait(false);
            await tesoreria.Cartera.Where(e => e.EmpresaId == id).ExecuteDeleteAsync(ct).ConfigureAwait(false);
        }, ct).ConfigureAwait(false);
        await BorradoEmpresa.EjecutarAsync(migracion, id, async () =>
        {
            await migracion.Correspondencias.Where(c => c.EmpresaId == id).ExecuteDeleteAsync(ct).ConfigureAwait(false);
            await migracion.Ejecuciones.Where(e => e.EmpresaId == id).ExecuteDeleteAsync(ct).ConfigureAwait(false);
        }, ct).ConfigureAwait(false);
        await BorradoEmpresa.EjecutarAsync(agro, id, () => agro.BorrarEmpresaAsync(id, ct), ct).ConfigureAwait(false);
        await BorradoEmpresa.EjecutarAsync(logistica, id, () => logistica.BorrarEmpresaAsync(id, ct), ct).ConfigureAwait(false);
        await BorradoEmpresa.EjecutarAsync(cooperativa, id, () => cooperativa.BorrarEmpresaAsync(id, ct), ct).ConfigureAwait(false);
        await BorradoEmpresa.EjecutarAsync(bodega, id, () => bodega.BorrarEmpresaAsync(id, ct), ct).ConfigureAwait(false);

        // Los maestros (clientes, proveedores, artículos) son del grupo: solo se borran si es la última empresa del grupo;
        // si quedan otras, siguen siendo suyos.
        var grupo = contexto.GrupoId ?? Guid.Empty;
        var ultima = !await organizacion.Empresas.AnyAsync(e => e.GrupoId == grupo && e.Id != id, ct).ConfigureAwait(false);
        if (ultima)
        {
            await terceros.Clientes.Where(c => c.GrupoId == grupo).ExecuteDeleteAsync(ct).ConfigureAwait(false);
            await terceros.Proveedores.Where(p => p.GrupoId == grupo).ExecuteDeleteAsync(ct).ConfigureAwait(false);
        }

        // El catálogo (artículos, familias, histórico de precios) es del grupo; las existencias y sus
        // movimientos son por empresa.
        await catalogo.Existencias.Where(e => e.EmpresaId == id).ExecuteDeleteAsync(ct).ConfigureAwait(false);
        await BorradoEmpresa.EjecutarAsync(catalogo, id, () => catalogo.MovimientosStock.Where(m => m.EmpresaId == id).ExecuteDeleteAsync(ct), ct).ConfigureAwait(false);
        if (ultima)
        {
            await catalogo.HistoricoPrecios.Where(h => h.GrupoId == grupo).ExecuteDeleteAsync(ct).ConfigureAwait(false);
            await catalogo.Productos.Where(p => p.GrupoId == grupo).ExecuteDeleteAsync(ct).ConfigureAwait(false);
            await catalogo.Familias.Where(f => f.GrupoId == grupo).ExecuteDeleteAsync(ct).ConfigureAwait(false);
        }
        await analisis.Informes.Where(i => i.EmpresaId == id).ExecuteDeleteAsync(ct).ConfigureAwait(false);
        await BorradoEmpresa.EjecutarAsync(auditoria, id, () => auditoria.Registros.Where(a => a.EmpresaId == id).ExecuteDeleteAsync(ct), ct).ConfigureAwait(false);
        await organizacion.Series.Where(s => s.EmpresaId == id).ExecuteDeleteAsync(ct).ConfigureAwait(false);
        await organizacion.Membresias.Where(m => m.EmpresaId == id).ExecuteDeleteAsync(ct).ConfigureAwait(false);
        await organizacion.Empresas.Where(e => e.Id == id).ExecuteDeleteAsync(ct).ConfigureAwait(false);

        return Results.Ok(new { eliminada = id });
    }
}
