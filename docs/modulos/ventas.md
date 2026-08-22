# Ciclo de venta (pedido y albarán)

Completa el lado de ventas al nivel del de compras: **presupuesto → pedido de venta → albarán de
entrega → factura**. Vive en el módulo **Facturación** y reutiliza toda la maquinaria de emisión de
facturas (numeración correlativa, VeriFactu, salida de existencias, vencimientos y contabilización).
Multiempresa (RLS sobre `pedido_venta` y `albaran_venta`).

## Pedido de venta

Segundo eslabón. Se crea directamente o **desde un presupuesto** (copia sus líneas), congela el
nombre del cliente y numera por empresa · ejercicio (con serie opcional resuelta por la numeración
avanzada, `TipoDocumento.PedidoVenta`).

Estados: `Borrador → Confirmado → Servido → Facturado`, más `Cancelado`. Cada línea guarda la
cantidad, el precio, el descuento y el código de IVA, y lleva el seguimiento de lo **servido** y lo
**facturado**.

## Albarán de entrega

Tercer eslabón. Documenta qué y cuánto se entrega al cliente contra un pedido confirmado; al crearse
actualiza las cantidades servidas del pedido (no se puede entregar más de lo pedido). La **salida de
existencias del inventario la realiza la factura al emitirse** (para no duplicar el movimiento), por
lo que el albarán es un documento de entrega. Numera por empresa · ejercicio
(`TipoDocumento.AlbaranVenta`).

## Facturación del pedido

`POST /pedidos-venta/{id}/facturar` genera una **factura real** reutilizando `EmitirFactura` (con su
numeración fiscal, huella VeriFactu, salida de stock, vencimientos y contabilización), y enlaza la
factura al pedido (`FacturaId`), dejándolo en estado `Facturado`. No se puede facturar un pedido en
borrador ni facturarlo dos veces.

## API

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/pedidos-venta` | `factura.leer` | Lista los pedidos de venta. |
| `GET` | `/pedidos-venta/{id}` | `factura.leer` | Pedido con sus líneas. |
| `POST` | `/pedidos-venta` | `factura.emitir` | Crea un pedido de venta. **201** |
| `POST` | `/pedidos-venta/desde-presupuesto` | `factura.emitir` | Crea un pedido copiando un presupuesto. **201** |
| `POST` | `/pedidos-venta/{id}/confirmar` | `factura.emitir` | Confirma el pedido. |
| `POST` | `/pedidos-venta/{id}/cancelar` | `factura.emitir` | Cancela el pedido. |
| `POST` | `/pedidos-venta/{id}/entregar` | `factura.emitir` | Registra un albarán de entrega. |
| `GET` | `/pedidos-venta/{id}/albaranes` | `factura.leer` | Albaranes de entrega del pedido. |
| `POST` | `/pedidos-venta/{id}/facturar` | `factura.emitir` | Factura el pedido (genera la factura real). **201** |

## Persistencia

- Esquema **`facturacion`**: tablas `pedido_venta` (+ `linea_pedido_venta`) y `albaran_venta`
  (+ `linea_albaran_venta`), con RLS por empresa en los cabeceros; las líneas se protegen a través de
  su padre (filtro global de EF Core).
- Índice único `(empresa_id, ejercicio, numero)` en `pedido_venta`.
- Migración: `CicloVenta`.

## Tests

- **Integración** (`VentasCicloEndpointsTests`): el pedido calcula el total y arranca en borrador; no
  se factura sin confirmar; el albarán actualiza lo servido (y no permite entregar de más); facturar
  un pedido confirmado genera una factura real, la enlaza y no permite refacturar; y se puede crear un
  pedido a partir de un presupuesto.

## Futuro (documentado)

Facturación parcial (facturar solo lo servido), PDF del pedido y del albarán, y salida de existencias
en el momento de la entrega (hoy la realiza la factura).
