# Módulo Producción

Fabricación de artículos compuestos mediante **órdenes de fabricación (OF)**, apoyadas en la **lista
de materiales** del Catálogo y en el **montaje** del Inventario. Multiempresa (RLS).

## Concepto

Una **orden de fabricación** (`OrdenFabricacion`) fabrica una cantidad de un **artículo compuesto**.
Al crearse se toma una **instantánea de su lista de materiales** (los componentes y las cantidades
necesarias, ya explosionadas por la cantidad a fabricar) para poder planificar. Estados:

`Planificada → EnCurso → Terminada` · o `Cancelada` (antes de terminar).

Al **terminar** la orden se ejecuta el **montaje** en el Inventario: se **consumen del almacén** los
componentes y se **da entrada** del artículo fabricado, todo en una única transacción. Si falta stock
de algún componente, el montaje falla y la orden no se termina.

Numeración correlativa **por empresa y ejercicio** (índice único `ux_orden_serie`).

## Reglas (invariantes)

- La cantidad a fabricar debe ser **> 0**.
- Solo se pueden crear órdenes de artículos **compuestos** (con lista de materiales); si no, 400.
- No se termina dos veces ni se termina/cancela una orden ya terminada.
- El consumo de componentes respeta el stock disponible (lo valida el montaje de Inventario).

## API

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/produccion/ordenes` | `produccion.leer` | Lista las órdenes de fabricación. |
| `GET` | `/produccion/ordenes/{id}` | `produccion.leer` | Obtiene una orden. |
| `POST` | `/produccion/ordenes` | `produccion.gestionar` | Crea una orden (planifica la lista de materiales). **201** |
| `POST` | `/produccion/ordenes/{id}/iniciar` | `produccion.gestionar` | Pasa a EnCurso. |
| `POST` | `/produccion/ordenes/{id}/terminar` | `produccion.gestionar` | Consume componentes y produce el artículo. |
| `POST` | `/produccion/ordenes/{id}/cancelar` | `produccion.gestionar` | Cancela la orden. |

## Composición (fronteras entre módulos)

- La lista de materiales se lee del **Catálogo** por el puerto `IConsultaListaMateriales` (sobre
  `ObtenerComposicion`); Producción no accede a su base de datos.
- El consumo/producción de stock se hace por el puerto `IMontajeProduccion`, implementado sobre el
  **montaje** del Inventario (`MontajeArticulo`). No es una transacción única entre módulos: cada
  módulo guarda su parte (mejora futura, *outbox*).

## Persistencia

- Esquema **`produccion`**: `orden_fabricacion` (RLS por empresa) y `componente_plan` (colección
  propia con la instantánea de la lista de materiales). Migración `MigracionInicialProduccion`.

## Tests

- **Unitarios**: explosión de la lista de materiales al crear, validaciones (cantidad, sin
  componentes) y ciclo de estados (iniciar/terminar/cancelar).
- **Integración**: fabricar 10 «mesas» (1 tabla + 4 patas) consume 10 tablas y 40 patas y produce 10
  mesas en el almacén; y que no se puede crear una orden de un artículo no compuesto (400).

## Futuro (documentado)

Partes de trabajo y consumo real vs. plan, escalado de tiempos/costes por operación, e imputación a
**proyectos** (siguiente incremento).
