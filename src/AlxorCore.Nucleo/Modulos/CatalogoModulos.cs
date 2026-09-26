using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Nucleo.Modulos;

/// <summary>Módulo contratable de ALXOR Core. <see cref="Requiere"/> son los módulos sin los que no funciona.</summary>
public sealed record ModuloAlxor(string Codigo, string Nombre, string Descripcion, IReadOnlyList<string> Requiere);

/// <summary>Edición comercial: un conjunto de módulos que se vende junto.</summary>
public sealed record EdicionAlxor(string Codigo, string Nombre, string Descripcion, IReadOnlyList<string> Modulos);

/// <summary>Plan resuelto de una empresa: su edición, los módulos añadidos aparte y el total activo.</summary>
public sealed record PlanEmpresa(string Edicion, IReadOnlyList<string> ModulosAdicionales, IReadOnlyList<string> ModulosActivos)
{
    public bool Incluye(string modulo) => ModulosActivos.Contains(modulo, StringComparer.Ordinal);
}

/// <summary>
/// Catálogo de módulos y ediciones con los que se vende ALXOR Core. La <b>base</b> (identidad,
/// empresa, usuarios, clientes, proveedores, productos, facturación, gastos, cobros y pagos,
/// documentos, informes y libros de IVA) va siempre incluida y no figura aquí. Lo demás se contrata
/// por edición o como módulo suelto. Así se puede vender solo la gestión a una empresa generalista,
/// solo las finanzas a una gestoría, o todo junto.
/// </summary>
public static class CatalogoModulos
{
    public const string Ventas = "ventas";
    public const string Compras = "compras";
    public const string Inventario = "inventario";
    public const string Produccion = "produccion";
    public const string Proyectos = "proyectos";
    public const string Personal = "personal";
    public const string Contabilidad = "contabilidad";
    public const string Inmovilizado = "inmovilizado";
    public const string TesoreriaAvanzada = "tesoreria_avanzada";
    public const string Divisas = "divisas";
    public const string Aprobaciones = "aprobaciones";
    public const string Integraciones = "integraciones";

    public const string EdicionStart = "start";
    public const string EdicionGestion = "gestion";
    public const string EdicionFinanzas = "finanzas";
    public const string EdicionGestionFinanzas = "gestion_finanzas";
    public const string EdicionCompleta = "completa";

    /// <summary>Edición de las empresas existentes y de las nuevas si no se indica otra (no quita nada a nadie).</summary>
    public const string EdicionPorDefecto = EdicionCompleta;

    public static IReadOnlyList<ModuloAlxor> Modulos { get; } =
    [
        new(Ventas, "Ventas", "Presupuestos, pedidos de venta, albaranes, cartas de porte y tarifas de precios con descuentos.", []),
        new(Compras, "Compras", "Solicitudes, pedidos y albaranes de compra.", []),
        new(Inventario, "Inventario", "Almacenes, ubicaciones, movimientos, lotes y números de serie.", []),
        new(Produccion, "Producción", "Órdenes de fabricación sobre la lista de materiales.", [Inventario]),
        new(Personal, "Personal", "Personas con tarifa por hora para imputar mano de obra.", []),
        new(Proyectos, "Proyectos", "Imputación de mano de obra, materiales y gastos; presupuesto frente a real.", [Personal]),
        new(Contabilidad, "Contabilidad", "Partida doble: plan contable, asientos, mayor, balances, cierre y cuentas anuales.", []),
        new(Inmovilizado, "Inmovilizado", "Amortización contable y fiscal, bajas y enajenaciones.", [Contabilidad]),
        new(TesoreriaAvanzada, "Tesorería avanzada", "Remesas SEPA, transferencias, confirming, conciliación Norma 43 y previsión.", []),
        new(Divisas, "Divisas", "Tipos de cambio y documentos en otras monedas.", []),
        new(Aprobaciones, "Aprobaciones", "Circuitos de aprobación de documentos.", []),
        new(Integraciones, "Integraciones", "API pública y webhooks para conectar otras aplicaciones.", []),
    ];

    public static IReadOnlyList<EdicionAlxor> Ediciones { get; } =
    [
        new(EdicionStart, "Start", "Facturación, gastos, cobros y pagos, libros de IVA y exportación para la gestoría.", []),
        new(EdicionGestion, "Gestión", "Start más el ciclo comercial completo: ventas, compras e inventario.",
            [Ventas, Compras, Inventario, Divisas, Aprobaciones, Integraciones]),
        new(EdicionFinanzas, "Finanzas", "Start más contabilidad completa, inmovilizado y tesorería avanzada.",
            [Contabilidad, Inmovilizado, TesoreriaAvanzada]),
        new(EdicionGestionFinanzas, "Gestión y finanzas", "Gestión y Finanzas juntas.",
            [Ventas, Compras, Inventario, Divisas, Aprobaciones, Integraciones, Contabilidad, Inmovilizado, TesoreriaAvanzada]),
        new(EdicionCompleta, "Completa", "Todos los módulos, incluidos producción y proyectos.",
            [Ventas, Compras, Inventario, Divisas, Aprobaciones, Integraciones, Contabilidad, Inmovilizado, TesoreriaAvanzada,
             Produccion, Personal, Proyectos]),
    ];

    public static ModuloAlxor? BuscarModulo(string? codigo) =>
        Modulos.FirstOrDefault(m => string.Equals(m.Codigo, codigo, StringComparison.Ordinal));

    public static EdicionAlxor? BuscarEdicion(string? codigo) =>
        Ediciones.FirstOrDefault(e => string.Equals(e.Codigo, codigo, StringComparison.Ordinal));

    /// <summary>
    /// Resuelve el plan de una edición con módulos adicionales. Rechaza códigos desconocidos y
    /// módulos a los que les falta otro del que dependen (no se añaden solos: son de pago).
    /// Los adicionales que ya incluye la edición se descartan.
    /// </summary>
    public static Resultado<PlanEmpresa> Resolver(string? edicion, IEnumerable<string>? adicionales)
    {
        var ed = BuscarEdicion(edicion);
        if (ed is null)
        {
            return Resultado.Fallo<PlanEmpresa>(Error.Validacion("plan.edicion_desconocida",
                $"La edición «{edicion}» no existe. Ediciones: {string.Join(", ", Ediciones.Select(e => e.Codigo))}."));
        }

        var pedidos = (adicionales ?? []).Select(a => (a ?? string.Empty).Trim()).Where(a => a.Length > 0).Distinct(StringComparer.Ordinal).ToList();
        var desconocidos = pedidos.Where(a => BuscarModulo(a) is null).ToList();
        if (desconocidos.Count > 0)
        {
            return Resultado.Fallo<PlanEmpresa>(Error.Validacion("plan.modulo_desconocido",
                $"Módulos desconocidos: {string.Join(", ", desconocidos)}."));
        }

        var extra = pedidos.Where(a => !ed.Modulos.Contains(a, StringComparer.Ordinal)).OrderBy(a => a, StringComparer.Ordinal).ToList();
        var activos = ed.Modulos.Concat(extra).Distinct(StringComparer.Ordinal).ToList();

        var faltan = activos
            .SelectMany(a => BuscarModulo(a)!.Requiere.Where(r => !activos.Contains(r, StringComparer.Ordinal))
                .Select(r => $"{BuscarModulo(a)!.Nombre} necesita {BuscarModulo(r)!.Nombre}"))
            .Distinct(StringComparer.Ordinal)
            .ToList();
        if (faltan.Count > 0)
        {
            return Resultado.Fallo<PlanEmpresa>(Error.Validacion("plan.dependencia", $"{string.Join("; ", faltan)}."));
        }

        // Orden estable (el del catálogo) para comparar planes y emitir claims siempre igual.
        var ordenados = Modulos.Select(m => m.Codigo).Where(c => activos.Contains(c, StringComparer.Ordinal)).ToList();
        return Resultado.Ok(new PlanEmpresa(ed.Codigo, extra, ordenados));
    }
}
