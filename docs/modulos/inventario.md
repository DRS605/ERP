# Módulo Inventario

Gestión de existencias **multi-almacén** con **ubicaciones**, movimientos (entrada, salida, ajuste
por recuento y traspaso), trazabilidad y **ubicación por defecto** del artículo. Multiempresa (RLS).

## Conceptos

- **Almacén** (`Almacen`): almacén físico o lógico de la empresa (código + nombre).
- **Ubicación** (`Ubicacion`): posición dentro de un almacén (p. ej. `A-1`, `PASILLO-3`). Opcional:
  un almacén puede llevar stock sin ubicaciones.
- **Existencia** (`Existencia`): stock de un artículo en un almacén y, opcionalmente, una ubicación.
  Nunca queda negativa.
- **Movimiento** (`MovimientoInventario`): registro histórico (trazabilidad) de cada entrada,
  salida, ajuste o traspaso, con cantidad **con signo**.
- **Ubicación por defecto** (`UbicacionDefecto`): dónde colocar un artículo por defecto. Puede ser
  **solo por almacén** o **por proveedor y almacén**.

## Movimientos

- **Entrada** (+): suma stock (crea la existencia si no existía).
- **Salida** (−): resta stock; falla si no hay suficiente.
- **Ajuste** (recuento): fija la cantidad contada y registra el movimiento por la **diferencia**.
- **Traspaso**: salida en origen + entrada en destino (entre almacenes y/o ubicaciones), atómico.

## Ubicación por defecto (resolución)

Al dar entrada a mercancía de un proveedor se resuelve la ubicación así:

1. Regla **específica del proveedor** para ese artículo y almacén, si existe.
2. Si no, la regla **general del almacén** para ese artículo.

Esto permite, por ejemplo, que un mismo artículo se ubique en `RECEPCIÓN-A` cuando llega del
proveedor X y en la ubicación estándar del almacén para el resto.

## API

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/inventario/almacenes` | `inventario.leer` | Lista almacenes. |
| `POST` | `/inventario/almacenes` | `inventario.gestionar` | Crea un almacén. **201** |
| `GET` | `/inventario/ubicaciones?almacenId=` | `inventario.leer` | Lista ubicaciones. |
| `POST` | `/inventario/ubicaciones` | `inventario.gestionar` | Crea una ubicación. **201** |
| `GET` | `/inventario/stock/producto/{id}` | `inventario.leer` | Existencias de un artículo por almacén/ubicación. |
| `GET` | `/inventario/stock/almacen/{id}` | `inventario.leer` | Existencias de un almacén. |
| `GET` | `/inventario/movimientos/producto/{id}` | `inventario.leer` | Trazabilidad de un artículo. |
| `POST` | `/inventario/entrada` | `inventario.gestionar` | Entrada de stock. |
| `POST` | `/inventario/salida` | `inventario.gestionar` | Salida de stock. |
| `POST` | `/inventario/ajuste` | `inventario.gestionar` | Ajuste por recuento. |
| `POST` | `/inventario/traspaso` | `inventario.gestionar` | Traspaso entre almacenes/ubicaciones. |
| `GET` | `/inventario/ubicacion-defecto/producto/{id}` | `inventario.leer` | Reglas de ubicación por defecto. |
| `POST` | `/inventario/ubicacion-defecto` | `inventario.gestionar` | Fija la regla (por almacén o proveedor+almacén). |

## Persistencia

- Esquema **`inventario`**: `almacen`, `ubicacion`, `existencia`, `movimiento_inventario`,
  `ubicacion_defecto`. RLS por empresa en las cinco tablas.
- Índices únicos: `(empresa, código)` de almacén, `(empresa, almacén, código)` de ubicación.
- Cantidades con 3 decimales (`numeric(14,3)`).

## Tests

- **Unitarios**: aumentar/disminuir existencia (sin negativo), ajuste por recuento y validación de
  almacén/ubicación.
- **Integración**: entradas/salidas/ajuste y **traspaso entre almacenes** (los saldos cuadran), y
  las **reglas de ubicación por defecto** (general por almacén y específica por proveedor).

## Relación con Compras (futuro)

La recepción de un albarán de compra podrá dar **entrada automática** al almacén, resolviendo la
ubicación por defecto por proveedor+almacén. Requiere que las líneas de compra referencien el
artículo del catálogo (hoy son texto); es la evolución natural de esta base.
