# Tarifas de precios de venta (módulo Ventas)

Una **tarifa** agrupa precios especiales y descuentos. Se asigna a los clientes y la usan las
facturas y los presupuestos cuando una línea de producto se crea **sin precio**. Si el usuario
escribe el precio, se respeta tal cual.

## Líneas

Cada línea tiene un **ámbito**:

| Ámbito | Ejemplo |
|---|---|
| Producto | "Refresco 33 cl a 8,50 € desde 100 unidades" |
| Familia (incluye sus subfamilias) | "−10 % en Bebidas" |
| General (sin producto ni familia) | "−5 % en todo" |

Y fija un **precio especial**, un **descuento**, o ambos, con **cantidad mínima** (escalado por volumen)
y **vigencia** opcional (`desde` y `hasta`, ambos incluidos).

## Qué línea se aplica

Se aplica **una sola** línea, entre las vigentes en la fecha y cuya cantidad mínima se alcanza:

1. La más específica: producto, luego la familia más cercana al producto (subfamilia antes que familia), luego general.
2. Dentro del mismo ámbito, la de mayor cantidad mínima (el tramo de volumen más alto alcanzado).

Si la línea no fija precio, se usa el del producto con el descuento de la línea. La respuesta dice
de dónde sale el precio: *"Tarifa MAYOR (precio del producto, desde 100 uds)"*.

## Reglas

- Una línea es de un producto **o** de una familia, nunca de ambos.
- Precio ≥ 0, descuento entre 0 y 100 %, cantidad mínima ≥ 0, `hasta` ≥ `desde`.
- Una línea tiene que cambiar algo (precio o descuento).
- No puede haber dos líneas del mismo ámbito y cantidad mínima con fechas que se solapen. Para
  programar un cambio de precio, se cierra una y se abre otra a continuación.
- Productos y familias de las líneas tienen que existir. El código de tarifa es único en el grupo.
- En la base de datos: RLS por grupo y restricciones `CHECK` con las mismas reglas.

## API

| Método | Ruta | Permiso |
|---|---|---|
| `GET` | `/tarifas`, `/tarifas/{id}` | Autenticado |
| `POST` | `/tarifas` `{ codigo, nombre, lineas }` | `producto.gestionar` |
| `PUT` | `/tarifas/{id}` `{ nombre, activa, lineas }` (sustituye las líneas) | `producto.gestionar` |
| `PUT` | `/clientes/{id}/tarifa` `{ tarifaId }` (null la quita) | `cliente.gestionar` |
| `GET` | `/precios?clienteId&productoId&cantidad&fecha` | Autenticado |

`/tarifas` y `/precios` pertenecen al módulo **Ventas** (ediciones Gestión, Gestión y finanzas y Completa).
