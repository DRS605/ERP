# Devoluciones a proveedor y abonos

*Compras → Devoluciones y abonos* (`/compras/devoluciones`).

## Devolución

Se devuelve mercancía recibida de un pedido de compra. De cada línea se puede devolver como mucho lo recibido menos lo
ya devuelto (`GET /compras/devoluciones/devolubles/{pedidoId}`). Si se indica almacén, la mercancía sale de él con su
lote o número de serie. Si no quedan existencias, no se registra (`devolucion_compra.sin_existencias`).

| Estado | Qué pasa |
|---|---|
| Registrada | Devuelta, pendiente del abono del proveedor. |
| EnFactura | El pedido aún no estaba facturado: al facturarlo, su factura sale sin lo devuelto. |
| Abonada | El proveedor la abonó con su factura rectificativa. |
| SinAbono | El proveedor repone la mercancía: no hay abono. |
| Anulada | No se llegó a devolver: la mercancía vuelve a entrar en el almacén. |

El precio de lo devuelto es el de la línea del pedido, con sus conceptos.

## Abono del proveedor (factura rectificativa recibida)

`POST /compras/devoluciones/{id}/abonar` registra el abono como **gasto en negativo** y rectificativo. Rectifica el
gasto con que se facturó el pedido; el pedido guarda ahora ese gasto, su IVA, su retención y el número y la fecha de la
factura del proveedor. El abono lleva el mismo IVA y la misma retención, salvo que se indique otro.

En el SII sale como R1 «por diferencias» y en el 303 resta del IVA soportado.

Un abono que no viene de una devolución (rappel, error de precio…) se registra desde *Nuevo gasto → Es un abono del
proveedor*, con la base en negativo y la factura que rectifica.

## Aplicar el abono

Un abono no se paga: se aplica a lo que se le debe al proveedor (`POST /pagos/abonos/{abonoId}/aplicar`). Por defecto
se aplica a la factura que rectifica, y si no a la que se indique, que debe ser del mismo proveedor. El importe es,
como mucho, lo pendiente del abono y de la factura.

En tesorería se crean dos movimientos contra la cuenta puente **555**:

- un pago de la factura;
- un movimiento negativo del abono, marcado como aplicación de abono. Es el único movimiento normal con importe
  negativo que admite la base de datos.

En contabilidad la 555 queda a cero, la 400 baja lo que el abono descuenta y el IVA soportado queda neto.
