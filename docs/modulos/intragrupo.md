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

### Inversión – patrimonio neto

En el perímetro de la participada se indica, en «Participación que tiene»:

- la **titular**: la empresa del grupo que tiene la participación;
- la **cuenta de la inversión**: por defecto 2403, participaciones a largo plazo en empresas del grupo; admite prefijo;
- el **coste**: sin él, se toma el saldo de esa cuenta en la titular;
- el **patrimonio neto al adquirirla**: sin él, el actual, y entonces no hay reservas posteriores a la compra.

La consolidación elimina el coste de la inversión, en la titular, contra el patrimonio neto de la participada (grupos
10 a 13). Con *p* = participación:

| Línea | Importe |
|---|---|
| Fondo de comercio de consolidación (204) | coste − *p* × patrimonio al adquirirla, si es positivo |
| Reservas en sociedades consolidadas (`RSC`) | *p* × (patrimonio actual − al adquirirla), más la diferencia si es negativa |
| Socios externos (`SOE`) | (100 % − *p*) × patrimonio actual en integración global; 0 en proporcional |

Ejemplo: A compra el 80 % de B por 1.000, cuando el patrimonio neto de B es 900; hoy es 1.000. Salen 280 de fondo de
comercio, 80 de reservas y 200 de socios externos.

Los apuntes cuadran siempre. El resultado del ejercicio de los socios externos sigue aparte («Atribuido a socios
externos»). Si la titular no está en la consolidación (excluida o sin acceso), no se elimina y se avisa.

## 6. Traspaso de existencias

Cuando una empresa entrega mercancía a otra del grupo (albarán de venta a un cliente enlazado), la receptora recibe
la mercancía sola:

1. Con la **primera entrega** se crea en la receptora el **pedido de compra espejo** del pedido de venta, confirmado:
   - proveedor: el enlazado con la empresa de origen;
   - líneas: todas las del pedido de venta, al precio con su descuento aplicado (precio neto); cada una queda
     enlazada con su línea de venta.
2. Cada albarán de venta es un **albarán de recepción** de ese pedido, con la referencia «Albarán N de …». Las
   **entregas parciales** se acumulan en el mismo pedido hasta completarlo.
3. Hay **entrada en el almacén** que la receptora haya elegido (ver abajo).
4. El pedido lleva la marca **Traspaso intragrupo**. No se recibe, edita, cancela ni factura a mano (409
   `pedido.intragrupo`): la factura llega a la bandeja de facturas recibidas cuando la emisora factura (§ 2).
5. Es **idempotente**: cada albarán de venta genera un solo albarán de recepción
   (`ux_albaran_compra_albaran_venta_origen`), y hay un solo pedido vivo por pedido de venta
   (`ux_pedido_compra_pedido_venta_origen`).

### Almacén de entrada

En «Entre empresas» → «Traspasos de existencias: almacén de entrada» (`/intragrupo/traspasos/almacenes`), la receptora
elige dónde entra lo que le llega:

- un almacén **por empresa de origen**, o uno **general** para el resto;
- **Sin entrada en inventario**: se registra el albarán sin movimiento de existencias;
- sin elegir, o si el almacén elegido se ha desactivado: el almacén activo con el código más bajo. Si no hay ninguno,
  no hay entrada.

### Anulación

Al **anular un albarán de venta** en la empresa de origen, en la receptora:

- se anula **solo su albarán de recepción**;
- la mercancía sale del almacén y la cantidad vuelve a quedar pendiente en el pedido;
- si el pedido se queda sin recepciones vivas, se cancela, y la siguiente entrega abre otro.

El albarán de compra del traspaso no se anula desde la receptora (409 `albaran.intragrupo`).

Antes de anular, se comprueba que la receptora aún tiene en ese almacén lo que entró con el albarán. Si ya lo ha
gastado, **no se anula nada** (409 `albaran.existencias_usadas`, en las dos rutas de anulación): hay que regularizar su
almacén y volver a anular.

Si aun así la anulación en destino falla (por ejemplo, las existencias salen entre la comprobación y el traspaso), el
albarán de venta queda anulado en origen y la recepción sigue viva en destino. El fallo queda en el registro de la API.
Se repite con `POST /intragrupo/albaranes/{id}/deshacer-traspaso`, que solo funciona con el albarán de venta ya anulado.

## 7. Pendiente

- Consolidación en cadena (participaciones indirectas) y por fecha de adquisición a mitad de ejercicio.
