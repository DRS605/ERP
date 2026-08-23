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

    // Compras (solicitud → pedido → albarán → factura)
    public const string CompraLeer = "compra.leer";
    public const string CompraGestionar = "compra.gestionar";

    // Inventario (multi-almacén, ubicaciones, movimientos)
    public const string InventarioLeer = "inventario.leer";
    public const string InventarioGestionar = "inventario.gestionar";

    // Producción (órdenes de fabricación sobre la lista de materiales)
    public const string ProduccionLeer = "produccion.leer";
    public const string ProduccionGestionar = "produccion.gestionar";

    // Personal (personas con tarifa)
    public const string PersonalLeer = "personal.leer";
    public const string PersonalGestionar = "personal.gestionar";

    // Proyectos (imputación de costes: mano de obra, materiales y gastos)
    public const string ProyectoLeer = "proyecto.leer";
    public const string ProyectoGestionar = "proyecto.gestionar";

    // Terceros y catálogo
    public const string ClienteGestionar = "cliente.gestionar";
    public const string ProductoGestionar = "producto.gestionar";

    // Informes y datos
    public const string InformeLeer = "informe.leer";
    public const string DatosExportar = "datos.exportar";

    // Aprobaciones (flujo de autorización con segregación de funciones)
    public const string AprobacionConfigurar = "aprobacion.configurar";
    public const string AprobacionAprobar = "aprobacion.aprobar";

    // Integraciones (API pública y webhooks)
    public const string IntegracionGestionar = "integracion.gestionar";

    // Actividades de negocio (clasificación transversal + visibilidad por usuario/pantalla)
    public const string ActividadGestionar = "actividad.gestionar";

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
        CompraLeer, CompraGestionar,
        InventarioLeer, InventarioGestionar,
        ProduccionLeer, ProduccionGestionar,
        PersonalLeer, PersonalGestionar,
        ProyectoLeer, ProyectoGestionar,
        ClienteGestionar, ProductoGestionar,
        InformeLeer, DatosExportar,
        AprobacionConfigurar, AprobacionAprobar,
        IntegracionGestionar,
        ActividadGestionar,
        EmpresaAjustes, UsuarioGestionar,
    };
}
