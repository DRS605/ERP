# Subasta hortofrutícola (alhóndiga)

Módulo sectorial `subasta`, que se contrata aparte y **necesita agro**: la fruta que se subasta es la de las partidas
recibidas en agro, y el precio de la subasta es el que se paga al agricultor en su liquidación. Tablas en el esquema
`agro` (`sesion_subasta`, `lote_subasta`, `puja_subasta`), con RLS por empresa; rutas bajo `/subasta`.

## Sesiones

Una **sesión** es un día de subasta, numerada por año (`SU2026/0001`), de uno de estos tipos:

- **a la baja** (reloj holandés): el precio baja desde el de salida y el lote se adjudica al primero que para el reloj,
  como mucho al precio de salida;
- **al alza**: los compradores pujan. Cada puja tiene que superar la mejor y llegar al precio de salida, y el lote se
  adjudica a la mejor puja (o a mano, no por debajo de ella).

Mientras está **abierta** se le añaden y quitan lotes, se puja, se adjudica, se deja un lote **desierto** o se deshace
una adjudicación. Se cambian la fecha (dentro del año), el tipo (si no hay pujas) y las observaciones.

## Lotes

Un lote son **kilos sueltos de una partida** (`POST /subasta/sesiones/{id}/lotes`): sin kilos, todo lo disponible. Lo
disponible es lo suelto de la partida (lo paletizado se despaletiza antes) menos lo que ya está en lotes vivos de otras
sesiones abiertas (`subasta.sin_kilos`). `GET /subasta/partidas` da las partidas con algo disponible. El lote lleva el
agricultor de la partida, los bultos (informativo) y el precio de salida opcional. Los compradores son **clientes**.

## Cierre

`POST /subasta/sesiones/{id}/cerrar`:

- lo pendiente queda desierto, y hace falta algún lote adjudicado;
- cada comprador recibe un **albarán de venta directo** con sus lotes, una línea por lote, al precio adjudicado. Si un
  albarán falla, se anulan los ya emitidos y la sesión no se cierra;
- los kilos salen de sus partidas (movimiento de expedición con documento `Subasta`). Con «reflejar las partidas en el
  inventario», el stock lo mueve el albarán (como en la expedición con albarán), no el movimiento de la partida.

## Precio al agricultor

En la liquidación al agricultor, una partida vendida en subasta se valora con la **media ponderada de sus lotes
adjudicados** en sesiones cerradas (importe entre kilos, a 6 decimales), sobre los kilos netos de la entrega. Ese precio
manda sobre los de la campaña, y la línea de la liquidación apunta a la sesión (`sesion_subasta_id`, la última de la
partida) en lugar de a un precio de la campaña. Una partida con lotes en una sesión **abierta** no se liquida
(`liquidacion.subasta_pendiente`). La comisión de la alhóndiga y los demás descuentos se ponen como **conceptos de
liquidación** (por ejemplo, un porcentaje del bruto).

## Anulación

`POST /subasta/sesiones/{id}/anular` con el motivo. Una sesión cerrada solo se anula si ninguna de sus partidas está en
una liquidación viva (`subasta.liquidada`): se anulan sus albaranes (si alguno está facturado, no se puede) y los kilos
vuelven a las partidas.

## Consultas

- `GET /subasta/sesiones/{id}`: lotes, pujas y totales por comprador y por agricultor.
- `GET /subasta/ventas?desde=&hasta=&compradorId=&agricultorId=`: los lotes vendidos en sesiones cerradas.

## Garantías en la base de datos

- Tipo y estados válidos, kilos positivos y precio de salida no negativo.
- Un lote adjudicado lleva comprador y precio positivo, y su importe es kilos × precio al céntimo. Uno sin adjudicar no
  lleva comprador, ni precio, ni importe, ni albarán.
- Cada línea de liquidación tiene un precio de la campaña o una sesión de subasta, nunca los dos ni ninguno.

## Pantallas

Menú **Subasta**: *Sesiones de subasta* (lotes, pujas, adjudicación, cierre y anulación) y *Ventas por comprador y
agricultor*. Plantilla de rol **Subastador**. Permisos `subasta.leer` y `subasta.gestionar`.
