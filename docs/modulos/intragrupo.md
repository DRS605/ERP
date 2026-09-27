# Operaciones entre empresas del grupo

Las empresas de un mismo grupo (holding) comparten los maestros: clientes, proveedores y artículos. Cuando una
empresa factura a otra del grupo, ALXOR lo trata como una operación intragrupo.

## 1. Terceros enlazados

- El cliente o el proveedor que *es* otra empresa del grupo lleva `EmpresaVinculadaId`. Se elige en su ficha:
  «Es una empresa del grupo».
- Solo se puede enlazar con una empresa del mismo grupo. Lo comprueba la API y, además, un trigger de la base de
  datos. Si la empresa se da de baja, el tercero queda como externo.
- La lista de empresas del grupo sale en `GET /grupos/actual/empresas`.

## 2. Documento espejo

1. Una empresa A emite una factura a un cliente enlazado con B.
2. Al confirmarse la factura, la factura (PDF) llega a la **bandeja de facturas recibidas de B** con origen
   «Intragrupo» y sus datos ya rellenos:
   - el proveedor que es A, si está dado de alta y enlazado;
   - el número, la fecha, la base, el tipo de impuesto y la retención.
3. B la revisa, la valida y la contabiliza como cualquier otra factura recibida, y así se genera su gasto.

Detalles del espejo:

- **Se lee y se escribe en el ámbito de cada empresa:** la seguridad por empresa (RLS) se cumple también aquí.
- **Es idempotente:** hay un índice único por empresa y factura de origen, y `POST /intragrupo/facturas/{id}/reflejar`
  reenvía la factura sin duplicarla si no llegó.
- **Si A anula la factura,** su espejo se rechaza con el motivo, siempre que B aún no lo haya contabilizado. Si ya lo
  contabilizó, el cuadre lo señala: B debe anular su gasto.
- **Con varios tipos de impuesto,** o si es una **rectificativa**, el espejo llega con una nota para revisarlo. El gasto
  lleva un solo tipo, y el abono se registra a mano.

## 3. Cuadre recíproco

`GET /intragrupo/cuadre?ejercicio=` (pantalla «Entre empresas») compara, por cada pareja emisora → receptora, lo
facturado por una empresa con lo contabilizado por la otra, factura a factura.

Cada factura queda en una de estas situaciones:

- cuadrada;
- pendiente en destino;
- importes distintos;
- rechazada en destino;
- sin reflejar;
- anulada (con su gasto aún vivo, si lo tiene).

Solo entran las empresas a las que tiene acceso el usuario; las demás se listan como «sin acceso».

## 4. Liquidación

`POST /intragrupo/facturas/{id}/liquidar` registra a la vez:

- el **cobro** de la factura en la empresa emisora;
- el **pago** del gasto en la receptora.

Las dos operaciones van por el mismo importe; por defecto, lo pendiente en las dos. Condiciones:

- el usuario tiene que tener acceso a las dos empresas;
- la receptora tiene que haber contabilizado ya la factura.

Si el pago no se puede registrar, el cobro se anula (contraasiento en modo Completo).

## 5. Consolidación

`GET /intragrupo/consolidado?ejercicio=` (pantalla «Entre empresas» → «Ver consolidado»):

1. Suma los saldos (debe − haber) de todas las empresas del grupo a las que tiene acceso el usuario, por cuenta de
   3 dígitos (el nivel del PGC común a todas).
2. De cada factura intragrupo que **cuadra** (emitida por una empresa y contabilizada por la otra por la misma base)
   elimina los **apuntes reales de sus asientos**:
   - la venta (7xx) en la emisora;
   - la compra o el gasto (6xx) en la receptora, sea cual sea la cuenta con que se contabilizó.
3. Elimina lo que queda **pendiente de cobro (430) y de pago (400)** entre ellas.
4. Da el resultado sumado y el consolidado. La venta intragrupo no crea resultado para el grupo.

Además:

- Las facturas que no cuadran (pendientes en destino, rechazadas, importes distintos…) **no se eliminan** y se
  listan aparte.
- La consolidación comprueba que los apuntes de eliminación cuadran (debe = haber).
- Las empresas con contabilidad simple no tienen asientos: se marcan y no aportan saldos.

## 6. Pendiente

- Emparejar las cuentas contables de las dos empresas (la correspondencia de asientos de Hispatec) y el porcentaje de
  participación (consolidación proporcional, socios externos).
- Albarán de compra nacido del albarán de venta intragrupo, con traspaso de existencias entre empresas.
