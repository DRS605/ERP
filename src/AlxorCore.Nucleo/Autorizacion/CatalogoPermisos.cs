namespace AlxorCore.Nucleo.Autorizacion;

/// <summary>Un permiso con su área y lo que deja hacer, para montar roles propios.</summary>
public sealed record DescripcionPermiso(string Codigo, string Area, string Descripcion);

/// <summary>Una plantilla de rol por puesto de trabajo: el punto de partida de un rol propio.</summary>
public sealed record PlantillaRol(string Codigo, string Nombre, string Descripcion, IReadOnlyList<string> Permisos);

/// <summary>
/// Descripción de todos los permisos (para la pantalla de roles) y plantillas de roles por puesto: báscula, confección,
/// expedición, jefe de almacén, calidad, técnico de campo, administración, comercial y dirección. Cada empresa crea sus
/// roles a partir de ellas y los ajusta.
/// </summary>
public static class CatalogoPermisos
{
    public static IReadOnlyList<DescripcionPermiso> Todos { get; } =
    [
        new(Permisos.FacturaLeer, "Ventas y facturación", "Ver facturas, presupuestos, pedidos y albaranes de venta"),
        new(Permisos.FacturaCrear, "Ventas y facturación", "Crear y editar documentos de venta"),
        new(Permisos.FacturaEmitir, "Ventas y facturación", "Emitir facturas (numerarlas y enviarlas)"),
        new(Permisos.ClienteGestionar, "Ventas y facturación", "Dar de alta y editar clientes"),
        new(Permisos.RiesgoForzar, "Ventas y facturación", "Vender por encima del riesgo del cliente"),
        new(Permisos.GastoLeer, "Compras y gastos", "Ver facturas de proveedor y gastos"),
        new(Permisos.GastoGestionar, "Compras y gastos", "Registrar facturas de proveedor y gastos"),
        new(Permisos.RecepcionLeer, "Compras y gastos", "Ver el buzón de facturas recibidas"),
        new(Permisos.RecepcionGestionar, "Compras y gastos", "Tramitar las facturas recibidas"),
        new(Permisos.RecepcionContabilizar, "Compras y gastos", "Contabilizar las facturas recibidas"),
        new(Permisos.CompraLeer, "Compras y gastos", "Ver solicitudes, pedidos y albaranes de compra"),
        new(Permisos.CompraGestionar, "Compras y gastos", "Gestionar las compras"),
        new(Permisos.CobroRegistrar, "Tesorería", "Registrar cobros"),
        new(Permisos.PagoRegistrar, "Tesorería", "Registrar pagos"),
        new(Permisos.ContabilidadLeer, "Contabilidad", "Ver la contabilidad"),
        new(Permisos.ContabilidadGestionar, "Contabilidad", "Hacer asientos, cierres y modelos"),
        new(Permisos.InventarioLeer, "Almacén", "Ver existencias y movimientos"),
        new(Permisos.InventarioGestionar, "Almacén", "Mover existencias, palés y ubicaciones"),
        new(Permisos.ProductoGestionar, "Almacén", "Dar de alta y editar artículos"),
        new(Permisos.ProduccionLeer, "Producción", "Ver órdenes de fabricación"),
        new(Permisos.ProduccionGestionar, "Producción", "Gestionar órdenes de fabricación"),
        new(Permisos.PersonalLeer, "Personal y proyectos", "Ver personas y tarifas"),
        new(Permisos.PersonalGestionar, "Personal y proyectos", "Gestionar personas y tarifas"),
        new(Permisos.ProyectoLeer, "Personal y proyectos", "Ver proyectos"),
        new(Permisos.ProyectoGestionar, "Personal y proyectos", "Gestionar proyectos"),
        new(Permisos.AgroLeer, "Agro", "Ver recepciones, partidas, palés, partes y trazabilidad"),
        new(Permisos.AgroRecepcionar, "Agro", "Báscula: recepciones, pesadas, palés de entrada y etiquetas de campo"),
        new(Permisos.AgroConfeccionar, "Agro", "Línea: partes de confección, palés, cajas, clasificación y repaletizado"),
        new(Permisos.AgroExpedir, "Agro", "Muelle: expediciones, órdenes de carga y reservas de palés"),
        new(Permisos.AgroCalidad, "Agro", "Calidad: certificados, autoevaluaciones y descalificación de partidas"),
        new(Permisos.AgroCampo, "Agro", "Campo: tratamientos y cuaderno de campo"),
        new(Permisos.AgroPlanificar, "Agro", "Planificación: plan comercial, de producción y de entradas, y su seguimiento"),
        new(Permisos.AgroGestionar, "Agro", "Todo lo operativo de agro y sus maestros (campañas, agricultores, precios, taras…)"),
        new(Permisos.AgroCorregir, "Agro", "Corregir lo ya hecho: rectificar entradas, anular o corregir salidas, aprobar mermas"),
        new(Permisos.AgroLiquidar, "Agro", "Liquidar al agricultor (genera sus facturas)"),
        new(Permisos.InformeLeer, "Informes y datos", "Ver informes y análisis"),
        new(Permisos.DatosExportar, "Informes y datos", "Exportar datos"),
        new(Permisos.AprobacionConfigurar, "Control", "Configurar los circuitos de aprobación"),
        new(Permisos.AprobacionAprobar, "Control", "Aprobar documentos"),
        new(Permisos.IntegracionGestionar, "Control", "API, webhooks y EDI"),
        new(Permisos.ActividadGestionar, "Control", "Actividades de negocio"),
        new(Permisos.UsuarioGestionar, "Administración", "Usuarios y roles"),
        new(Permisos.EmpresaAjustes, "Administración", "Ajustes de la empresa, plan y numeración"),
    ];

    public static IReadOnlyList<PlantillaRol> Plantillas { get; } =
    [
        new("bascula", "Báscula", "Recibe la fruta: recepciones, pesadas y palés de entrada.",
            [Permisos.AgroLeer, Permisos.AgroRecepcionar]),
        new("confeccion", "Confección", "Partes de confección, palés y cajas en la línea.",
            [Permisos.AgroLeer, Permisos.AgroConfeccionar]),
        new("expedicion", "Expedición", "Carga y salida de palés: órdenes de carga y expediciones.",
            [Permisos.AgroLeer, Permisos.AgroExpedir, Permisos.FacturaLeer]),
        new("jefe_almacen", "Jefe de almacén", "Todo el almacén, incluidas las correcciones y la aprobación de mermas.",
            [Permisos.AgroLeer, Permisos.AgroRecepcionar, Permisos.AgroConfeccionar, Permisos.AgroExpedir, Permisos.AgroGestionar, Permisos.AgroCorregir,
                Permisos.InventarioLeer, Permisos.InventarioGestionar, Permisos.InformeLeer, Permisos.AgroPlanificar]),
        new("calidad", "Calidad", "Certificaciones, autoevaluaciones y descalificación de partidas.",
            [Permisos.AgroLeer, Permisos.AgroCalidad, Permisos.InformeLeer]),
        new("tecnico_campo", "Técnico de campo", "Tratamientos y cuaderno de campo de las parcelas.",
            [Permisos.AgroLeer, Permisos.AgroCampo]),
        new("administracion", "Administración", "Facturación, compras, tesorería, contabilidad y liquidaciones al agricultor.",
            [Permisos.FacturaLeer, Permisos.FacturaCrear, Permisos.FacturaEmitir, Permisos.ClienteGestionar, Permisos.GastoLeer, Permisos.GastoGestionar,
                Permisos.RecepcionLeer, Permisos.RecepcionGestionar, Permisos.RecepcionContabilizar, Permisos.CompraLeer, Permisos.CompraGestionar,
                Permisos.CobroRegistrar, Permisos.PagoRegistrar, Permisos.ContabilidadLeer, Permisos.ContabilidadGestionar, Permisos.ProductoGestionar,
                Permisos.AgroLeer, Permisos.AgroLiquidar, Permisos.InformeLeer, Permisos.DatosExportar]),
        new("comercial", "Comercial", "Clientes, ofertas, pedidos y seguimiento de lo expedido.",
            [Permisos.FacturaLeer, Permisos.FacturaCrear, Permisos.ClienteGestionar, Permisos.AgroLeer, Permisos.AgroPlanificar, Permisos.InventarioLeer, Permisos.InformeLeer]),
        new("direccion", "Dirección", "Ve y aprueba todo; no gestiona usuarios ni ajustes.",
            Permisos.Todos.Where(p => p is not Permisos.UsuarioGestionar and not Permisos.EmpresaAjustes).ToList()),
    ];
}
