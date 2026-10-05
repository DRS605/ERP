# Ediciones y módulos contratables

ALXOR Core se vende por **ediciones** y **módulos sueltos**. Así se puede ofrecer solo la gestión a
una empresa generalista, solo las finanzas a una asesoría, o todo junto, con el mismo producto.

## Base (incluida siempre)

Identidad y usuarios, empresa y series, clientes y proveedores, productos e impuestos, facturas y
tickets (incluidas las recurrentes), gastos y buzón de facturas de proveedor, cobros y pagos,
documentos (PDF y correo), informes (panel, libros de IVA, modelos AEAT, exportación a la gestoría),
auditoría e importación de datos.

## Ediciones

| Edición | Código | Para quién | Módulos |
|---|---|---|---|
| Start | `start` | Autónomos y microempresas | Solo la base |
| Gestión | `gestion` | Pyme comercial o de servicios | Ventas, Compras, Inventario, Divisas, Aprobaciones, Integraciones |
| Finanzas | `finanzas` | Empresas con contabilidad propia, asesorías | Contabilidad, Inmovilizado, Analítica y presupuestos, Tesorería avanzada |
| Gestión y finanzas | `gestion_finanzas` | Pyme completa | Gestión + Finanzas |
| Completa | `completa` | Industria y proyectos | Todos los generalistas, incluidos Producción, Logística, Personal y Proyectos |

Cualquier módulo se puede añadir suelto sobre una edición (por ejemplo, Start + Contabilidad).

Los módulos **sectoriales** no entran en ninguna edición, ni siquiera en la Completa: una empresa
generalista no debe ver menús de otro sector. Se contratan aparte (por ejemplo, Gestión y finanzas + Agro).

## Módulos y dependencias

| Módulo | Código | Necesita |
|---|---|---|
| Ventas (presupuestos, pedidos, albaranes, cartas de porte) | `ventas` | — |
| Compras (solicitudes, pedidos, albaranes) | `compras` | — |
| Inventario | `inventario` | — |
| Producción | `produccion` | Inventario |
| Logística: fichas GTIN, paletizado, palés SSCC, terminal de pistola/móvil — [logistica.md](logistica.md) | `logistica` | Inventario |
| Personal (tarifa por hora para imputar mano de obra) | `personal` | — |
| Proyectos | `proyectos` | Personal |
| Contabilidad | `contabilidad` | — |
| Inmovilizado | `inmovilizado` | Contabilidad |
| Analítica y presupuestos (centros, partidas, reglas, repartos, campañas, presupuestos) | `analitica` | Contabilidad |
| Tesorería avanzada (remesas SEPA, confirming, Norma 43, previsión) | `tesoreria_avanzada` | — |
| Divisas | `divisas` | — |
| Aprobaciones | `aprobaciones` | — |
| Integraciones (API pública y webhooks) | `integraciones` | — |
| **Agro** (sectorial): recepción de fruta, liquidación al agricultor, confección, palés SSCC, trazabilidad — [agro.md](agro.md) | `agro` | — |
| **Cooperativas y SAT** (sectorial): socios, capital social, reparto del excedente con retorno cooperativo, retenciones, libros registro y actas — [cooperativa.md](cooperativa.md) | `cooperativa` | — |
| **Bodegas** (sectorial): depósitos, entrada y liquidación de la uva, elaboración, trasiegos, coupages, embotellado, granel y declaración de existencias — [bodega.md](bodega.md) | `bodega` | Inventario |
| **Subasta hortofrutícola** (sectorial): sesiones, lotes de las partidas, reloj a la baja o pujas al alza, albarán por comprador y precio al agricultor — [subasta.md](subasta.md) | `subasta` | Agro |

Un plan al que le falta una dependencia se **rechaza** (no se añade sola, porque es de pago):
*"Producción necesita Inventario."*

## Cómo se aplica

- El plan se guarda en la empresa (`organizacion.empresa.edicion` y `modulos_adicionales`). La base
  de datos solo admite ediciones y módulos del catálogo (restricciones `CHECK`).
- Al **seleccionar la empresa**, el token incluye `edicion` y un claim `modulo` por módulo activo.
  La respuesta de selección los devuelve también, para que la interfaz muestre solo lo contratado.
- `MiddlewareModulos` rechaza con **403 `modulo.no_contratado`** las rutas de módulos no contratados:
  *"Tu plan (Gestión) no incluye el módulo Contabilidad. Puedes contratarlo en Ajustes → Plan."*
  El mapa de rutas está en `RutasModulos`.
- **Guardián:** el test `ModulosEndpointsTests.Toda_ruta_esta_clasificada` falla si una ruta nueva no
  está asignada a la base o a un módulo.
- Cambiar de plan **no borra datos**. Lo de un módulo que se deja de contratar vuelve a verse si se
  contrata de nuevo.
- Las empresas existentes y las nuevas tienen la edición **Completa** por defecto, para que nadie
  pierda nada.

## API

| Método | Ruta | Permiso |
|---|---|---|
| `GET` | `/planes` | Público (catálogo de ediciones y módulos, sirve para la página de precios) |
| `GET` | `/empresas/actual/plan` | Autenticado con empresa |
| `PUT` | `/empresas/actual/plan` `{ edicion, modulosAdicionales }` | `empresa.ajustes` |

El cambio de plan se aplica a los tokens nuevos: la interfaz vuelve a seleccionar la empresa.

## Interfaz

Las dos interfaces (clásica y React) filtran el menú con el plan de la respuesta de selección, y
la clásica tiene la sección **Ajustes → Plan contratado** para cambiar edición y módulos.

## Pendiente

- Enlazar el cambio de plan con la facturación de la suscripción (hoy lo cambia quien tenga `empresa.ajustes`).

La fiscalidad indirecta (IVA o IGIC según el territorio, prorrata, modelos 303 y 420) es parte de la base: está en todas las ediciones. Ver [impuestos-indirectos.md](impuestos-indirectos.md).
