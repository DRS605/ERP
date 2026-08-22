# Módulo Proyectos

Imputación de costes a **proyectos** para compararlos con el presupuesto: **mano de obra** (personas
× horas × tarifa), **materiales** (valorados según el método de la empresa) y **gastos** directos.
Multiempresa (RLS). Numeración correlativa por empresa y ejercicio.

## Concepto

Un **proyecto** (`Proyecto`) tiene un **presupuesto** (coste objetivo) y acumula **imputaciones**.
Cada imputación (`Imputacion`) es de uno de tres tipos, unificados en una sola línea
(`Importe = Cantidad × CosteUnitario`):

| Tipo | `Cantidad` | `CosteUnitario` | Origen del coste |
|---|---|---|---|
| **ManoObra** | horas | tarifa/hora de la persona | tarifa de la **persona** (módulo Personal) |
| **Material** | unidades | valor unitario del artículo | **método de valoración de la empresa** (estándar / última compra / PMP / FIFO) |
| **Gasto** | 1 | importe | importe directo tecleado |

El **coste real** es la suma de las imputaciones; la **desviación** = `presupuesto − coste real`
(positiva = por debajo del presupuesto). Estados: `Abierto → Cerrado` (reversible) · o `Cancelado`.
Solo un proyecto **abierto** admite imputaciones o su eliminación.

El coste **se congela** en el momento de imputar: la tarifa de la persona y el valor del artículo se
copian en la línea, de modo que un cambio posterior de tarifa o de coste no altera lo ya imputado.

## Reglas (invariantes)

- Nombre obligatorio; presupuesto ≥ 0.
- Horas/cantidad de material **> 0**; coste unitario e importe de gasto **≥ 0**.
- Solo se imputa (o se borra una imputación) con el proyecto **abierto**.
- Un proyecto **cancelado** no se edita, ni se cierra ni se reabre.

## API

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/proyectos` | `proyecto.leer` | Lista los proyectos con coste real y desviación. |
| `GET` | `/proyectos/{id}` | `proyecto.leer` | Detalle: imputaciones y subtotales por tipo. |
| `POST` | `/proyectos` | `proyecto.gestionar` | Crea un proyecto. **201** |
| `PUT` | `/proyectos/{id}` | `proyecto.gestionar` | Actualiza nombre, cliente y presupuesto. |
| `POST` | `/proyectos/{id}/cerrar` · `/reabrir` · `/cancelar` | `proyecto.gestionar` | Cambia el estado. |
| `POST` | `/proyectos/{id}/mano-obra` | `proyecto.gestionar` | Imputa horas de una persona (tarifa del módulo Personal). |
| `POST` | `/proyectos/{id}/material` | `proyecto.gestionar` | Imputa material (valorado según el método de la empresa). |
| `POST` | `/proyectos/{id}/gasto` | `proyecto.gestionar` | Imputa un gasto directo. |
| `DELETE` | `/proyectos/{id}/imputaciones/{imputacionId}` | `proyecto.gestionar` | Elimina una imputación. |

## Puertos (fronteras entre módulos)

- `ITarifaPersona` → implementado sobre `IConsultaPersonas` del módulo **Personal** (tarifa/hora).
- `IValoracionArticulos` (puerto del **Núcleo**, implementado por **Inventario**) → coste unitario
  del material según el método de valoración de la empresa.
- `IConsultaArticuloProyectos` → nombre del artículo del **Catálogo** para describir la línea.

Proyectos no accede a la base de datos de otros módulos: todo pasa por estos puertos.

## Permisos y roles

- Permisos `proyecto.leer` y `proyecto.gestionar`.
- **Propietario** y **Usuario**: ambos. **Solo lectura**: `proyecto.leer`.

## Persistencia

- Esquema **`proyectos`**: `proyecto` (RLS por empresa; índice único `ux_proyecto_serie` por
  `empresa_id, ejercicio, numero`) y `imputacion` (colección propietaria del proyecto, protegida por
  su FK). Migración `MigracionInicialProyectos`.

## Tests

- **Unitarios**: arranque abierto sin coste, suma de coste real y subtotales por tipo, desviación
  (positiva/negativa), validaciones, bloqueo de imputación en cerrado/cancelado, reapertura y
  recálculo al eliminar.
- **Integración**: imputar mano de obra (4 h × 30 €/h = 120), material (3 × coste estándar 10 = 30) y
  gasto (50) → coste real 200 y desviación 800; eliminar recalcula; rechazo de persona inexistente
  (400) y de imputación a proyecto cerrado (409).

## Interfaz

Entrada de menú **Proyectos**: listado con presupuesto, coste real y desviación (con color según
signo); ficha del proyecto con sus imputaciones y botones para imputar mano de obra, material o
gasto, y para cerrar/reabrir/cancelar.

## Futuro (documentado)

Partes de horas por persona/día, tarifas por periodo o coste-empresa (SS), enlace de los gastos del
proyecto con el módulo Gastos/Compras, y márgenes cuando el proyecto se factura al cliente.
