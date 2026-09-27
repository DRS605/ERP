using AlxorCore.Nucleo.Modulos;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Seguridad;

namespace AlxorCore.Api.Comun;

/// <summary>
/// Qué módulo contratable necesita cada ruta de la API. Las rutas que no aparecen son de la
/// <b>base</b> (incluida en todas las ediciones). El test de integración
/// <c>ModulosEndpointsTests.Toda_ruta_esta_clasificada</c> obliga a clasificar cada ruta nueva.
/// </summary>
public static class RutasModulos
{
    /// <summary>Prefijos de ruta y su módulo. Se comprueban de más largo a más corto.</summary>
    public static IReadOnlyList<(string Prefijo, string Modulo)> Mapa { get; } =
        new (string Prefijo, string Modulo)[]
        {
            ("/contabilidad/inmovilizado", CatalogoModulos.Inmovilizado),
            ("/contabilidad/analitica", CatalogoModulos.Analitica),
            ("/contabilidad/presupuestos", CatalogoModulos.Analitica),
            ("/contabilidad", CatalogoModulos.Contabilidad),
            ("/pedidos-venta", CatalogoModulos.Ventas),
            ("/presupuestos", CatalogoModulos.Ventas),
            ("/cartas-porte", CatalogoModulos.Ventas),
            ("/transporte", CatalogoModulos.Ventas),
            ("/aduanas", CatalogoModulos.Ventas),
            ("/certificados-fitosanitarios", CatalogoModulos.Ventas),
            ("/tarifas", CatalogoModulos.Ventas),
            ("/precios", CatalogoModulos.Ventas),
            ("/compras", CatalogoModulos.Compras),
            ("/inventario", CatalogoModulos.Inventario),
            ("/produccion", CatalogoModulos.Produccion),
            ("/proyectos", CatalogoModulos.Proyectos),
            ("/personal", CatalogoModulos.Personal),
            ("/tesoreria", CatalogoModulos.TesoreriaAvanzada),
            ("/tipos-cambio", CatalogoModulos.Divisas),
            ("/divisas", CatalogoModulos.Divisas),
            ("/aprobaciones", CatalogoModulos.Aprobaciones),
            ("/integraciones", CatalogoModulos.Integraciones),
            ("/api/v1", CatalogoModulos.Integraciones),
            ("/agro", CatalogoModulos.Agro),
        }.OrderByDescending(x => x.Prefijo.Length).ToList();

    /// <summary>Prefijos de la base (en todas las ediciones). Solo sirven para el test de clasificación.</summary>
    public static IReadOnlyList<string> Base { get; } =
    [
        "/auth", "/empresas", "/grupos", "/intragrupo", "/planes", "/usuarios", "/cuenta", "/series", "/formas-pago", "/actividades",
        "/clientes", "/proveedores", "/productos", "/familias", "/tipos-iva", "/impuestos", "/tickets",
        "/facturas", "/facturas-recurrentes", "/gastos", "/recepcion", "/cobros", "/pagos", "/anticipos", "/impagados",
        "/informes", "/auditoria", "/importar", "/migracion", "/cartera", "/salud", "/swagger",
    ];

    /// <summary>Módulo que exige la ruta, o <c>null</c> si es de la base.</summary>
    public static string? ModuloDe(PathString ruta)
    {
        var valor = ruta.Value ?? string.Empty;
        foreach (var (prefijo, modulo) in Mapa)
        {
            if (valor.Equals(prefijo, StringComparison.OrdinalIgnoreCase)
                || valor.StartsWith(prefijo + "/", StringComparison.OrdinalIgnoreCase))
            {
                return modulo;
            }
        }

        return null;
    }
}

/// <summary>
/// Rechaza (403, <c>modulo.no_contratado</c>) las peticiones a rutas de un módulo que el plan de la
/// empresa activa no incluye. Se basa en los claims del token (<see cref="ClaimsAlxor.Modulo"/>). Un
/// token sin edición (emitido antes de existir los planes, o sin empresa activa) no se restringe
/// aquí: la autorización por permisos sigue aplicándose igual.
/// </summary>
public sealed class MiddlewareModulos
{
    private readonly RequestDelegate _siguiente;

    public MiddlewareModulos(RequestDelegate siguiente) => _siguiente = siguiente;

    public async Task InvokeAsync(HttpContext contexto)
    {
        ArgumentNullException.ThrowIfNull(contexto);

        var modulo = RutasModulos.ModuloDe(contexto.Request.Path);
        var edicion = contexto.User.FindFirst(ClaimsAlxor.Edicion)?.Value;
        if (modulo is not null && edicion is not null && !contexto.User.HasClaim(ClaimsAlxor.Modulo, modulo))
        {
            var nombreModulo = CatalogoModulos.BuscarModulo(modulo)?.Nombre ?? modulo;
            var nombreEdicion = CatalogoModulos.BuscarEdicion(edicion)?.Nombre ?? edicion;
            var error = Error.Prohibido("modulo.no_contratado",
                $"Tu plan ({nombreEdicion}) no incluye el módulo {nombreModulo}. Puedes contratarlo en Ajustes → Plan.");
            await ResultadosHttp.AProblema(error).ExecuteAsync(contexto).ConfigureAwait(false);
            return;
        }

        await _siguiente(contexto).ConfigureAwait(false);
    }
}
