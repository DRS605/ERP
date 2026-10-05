namespace AlxorCore.Nucleo.Autorizacion;

/// <summary>
/// Catálogo de permisos granulares de ALXOR Core. Son códigos estables: cada empresa los combina en
/// sus roles (los fijos de <see cref="Rol"/> o los suyos propios, por puesto de trabajo).
/// Cada módulo futuro añadirá aquí sus permisos, con su descripción en <see cref="CatalogoPermisos"/>.
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

    // Agro (recepción de fruta, confección y trazabilidad; la liquidación al agricultor va aparte porque genera facturas)
    public const string AgroLeer = "agro.leer";
    public const string AgroGestionar = "agro.gestionar";
    public const string AgroLiquidar = "agro.liquidar";

    /// <summary>Corregir lo ya hecho: rectificar entradas, anular o corregir expediciones, aprobar mermas y descalificar partidas.</summary>
    public const string AgroCorregir = "agro.corregir";

    // Agro por puesto: la báscula recepciona, la línea confecciona, el muelle expide; calidad y campo aparte.
    public const string AgroRecepcionar = "agro.recepcionar";
    public const string AgroConfeccionar = "agro.confeccionar";
    public const string AgroExpedir = "agro.expedir";
    public const string AgroCalidad = "agro.calidad";
    public const string AgroCampo = "agro.campo";
    public const string AgroPlanificar = "agro.planificar";

    // Cooperativas y SAT (socios, capital, repartos y libros)
    public const string CooperativaLeer = "cooperativa.leer";
    public const string CooperativaGestionar = "cooperativa.gestionar";

    // Sectores: subasta, bodegas y viveros
    public const string SubastaLeer = "subasta.leer";
    public const string SubastaGestionar = "subasta.gestionar";
    public const string BodegaLeer = "bodega.leer";
    public const string BodegaGestionar = "bodega.gestionar";
    public const string ViveroLeer = "vivero.leer";
    public const string ViveroGestionar = "vivero.gestionar";

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

    /// <summary>Emitir o confirmar por encima del límite de riesgo del tercero aunque la empresa lo bloquee.</summary>
    public const string RiesgoForzar = "riesgo.forzar";

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
        AgroLeer, AgroGestionar, AgroLiquidar, AgroCorregir, AgroRecepcionar, AgroConfeccionar, AgroExpedir, AgroCalidad, AgroCampo, AgroPlanificar,
        CooperativaLeer, CooperativaGestionar,
        SubastaLeer, SubastaGestionar, BodegaLeer, BodegaGestionar, ViveroLeer, ViveroGestionar,
        ClienteGestionar, ProductoGestionar,
        InformeLeer, DatosExportar,
        AprobacionConfigurar, AprobacionAprobar,
        IntegracionGestionar,
        ActividadGestionar,
        EmpresaAjustes, UsuarioGestionar,
        RiesgoForzar,
    };
}
