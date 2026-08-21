namespace AlxorCore.Nucleo.Autorizacion;

/// <summary>
/// Catálogo de permisos granulares de ALXOR Core. Son códigos estables (no datos editables):
/// modelarlos como constantes mantiene la autorización simple, versionada y sin una pantalla
/// de administración que el usuario objetivo no necesita.
/// Cada módulo futuro añadirá aquí sus permisos.
/// </summary>
public static class Permisos
{
    // Facturación
    public const string FacturaLeer = "factura.leer";
    public const string FacturaCrear = "factura.crear";
    public const string FacturaEmitir = "factura.emitir";

    // Gastos
    public const string GastoLeer = "gasto.leer";
    public const string GastoGestionar = "gasto.gestionar";

    // Recepción de facturas de proveedor (bandeja + contabilización)
    public const string RecepcionLeer = "recepcion.leer";
    public const string RecepcionGestionar = "recepcion.gestionar";
    public const string RecepcionContabilizar = "recepcion.contabilizar";

    // Tesorería (cobros y pagos)
    public const string CobroRegistrar = "cobro.registrar";
    public const string PagoRegistrar = "pago.registrar";

    // Contabilidad (partida doble)
    public const string ContabilidadLeer = "contabilidad.leer";
    public const string ContabilidadGestionar = "contabilidad.gestionar";

    // Terceros y catálogo
    public const string ClienteGestionar = "cliente.gestionar";
    public const string ProductoGestionar = "producto.gestionar";

    // Informes y datos
    public const string InformeLeer = "informe.leer";
    public const string DatosExportar = "datos.exportar";

    // Administración de la empresa
    public const string EmpresaAjustes = "empresa.ajustes";
    public const string UsuarioGestionar = "usuario.gestionar";

    /// <summary>Todos los permisos definidos, para validación y semillas.</summary>
    public static readonly IReadOnlySet<string> Todos = new HashSet<string>(StringComparer.Ordinal)
    {
        FacturaLeer, FacturaCrear, FacturaEmitir,
        GastoLeer, GastoGestionar,
        RecepcionLeer, RecepcionGestionar, RecepcionContabilizar,
        CobroRegistrar, PagoRegistrar,
        ContabilidadLeer, ContabilidadGestionar,
        ClienteGestionar, ProductoGestionar,
        InformeLeer, DatosExportar,
        EmpresaAjustes, UsuarioGestionar,
    };
}
