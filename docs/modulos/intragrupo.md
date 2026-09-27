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

### Perímetro y participación

`GET/PUT /intragrupo/perimetro/{empresaId}` fija la participación del grupo en cada empresa y su método. Sin fijar, la
empresa entra por integración global al 100 %.

| Método | Saldos | Resultado | Eliminaciones |
|---|---|---|---|
| Global | Al 100 % | La parte de los otros socios (100 % − participación) se atribuye a **socios externos**; el resto, a la sociedad dominante | Completas |
| Proporcional | En el porcentaje de participación | Todo del grupo | En el menor de los dos porcentajes de la pareja |
| Excluida | No entra | — | Ninguna: sus operaciones con el grupo son con terceros |

### Correspondencias de cuentas

`/intragrupo/correspondencias` empareja una cuenta de una empresa con la recíproca de otra (la
`CuentaContableCorrespondenciaEmpresasGrupo` de Hispatec). Sirve, por ejemplo, para un préstamo: 5523 en la
prestamista y 5133 en la prestataria.

- La cuenta puede ser un prefijo de 3 a 12 dígitos: suma todas las subcuentas que empiezan así.
- En la consolidación se eliminan los dos saldos.
- Su suma es el **descuadre**: debería ser 0, y si no lo es, las eliminaciones no cuadran y se señala.
- Las ventas, compras y saldos de clientes y proveedores entre empresas ya se eliminan solos: no hace falta
  emparejarlos.

## 6. Traspaso de existencias

Cuando una empresa entrega mercancía a otra del grupo (albarán de venta a un cliente enlazado), la receptora recibe
la mercancía sola:

1. Se crea en la receptora un **pedido de compra confirmado**:
   - proveedor: el enlazado con la empresa de origen;
   - líneas: las del albarán, al precio del pedido de venta con su descuento aplicado (precio neto).
2. Se registra su **albarán de recepción**, con la referencia «Albarán N de …».
3. Hay **entrada en el almacén** activo de la receptora con el código más bajo. Si no tiene almacén, se registra el
   albarán sin movimiento de existencias.
4. El pedido lleva la marca **Traspaso intragrupo**. No se factura desde compras (409 `pedido.intragrupo`): la factura
   llega a la bandeja de facturas recibidas cuando la emisora factura (§ 2).
5. Es **idempotente**: un albarán de venta solo genera un pedido (índice único `ux_pedido_compra_albaran_origen`).

Al **anular el albarán de venta** en la empresa de origen:

- en la receptora se anula el albarán de compra;
- la mercancía sale del almacén;
- el pedido se cancela.

Si la receptora ya ha consumido esas existencias, la anulación se rechaza (`albaran.existencias_usadas`) y no se
deshace nada. El albarán de compra del traspaso no se anula desde la receptora (409 `albaran.intragrupo`).

## 7. Pendiente

- Patrimonio de socios externos y eliminación inversión-patrimonio neto (la consolidación da el resultado atribuido,
  no la eliminación de la participación en el capital).
- Traspaso a un almacén elegido por la receptora (hoy entra en el de código más bajo) y traspasos parciales por
  líneas.
