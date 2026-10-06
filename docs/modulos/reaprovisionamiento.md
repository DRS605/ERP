# Stock mínimo, propuesta de compra y necesidades de fabricación

## Reglas

*Artículos y almacén → Stock mínimo* (`/inventario/reaprovisionamiento`). Cada regla es de un artículo en un almacén o
en todos, y tiene:

- **mínimo**;
- **máximo**: hasta dónde se repone;
- **múltiplo de compra**, opcional, en unidades base;
- **proveedor**, opcional; si no se pone, se usa el habitual del artículo.

Volver a fijar la regla de un artículo y almacén la cambia; no la duplica.

## Propuesta de compra

*Compras → Propuesta de compra* (`GET /compras/propuesta`).

**Disponible** = existencias + pendiente de recibir − necesidades de fabricación, donde:

- **pendiente de recibir** incluye los pedidos de compra en borrador, confirmados o recibidos a medias. Así, generada
  la propuesta, no se vuelve a proponer;
- **necesidades de fabricación** son los componentes de las órdenes de fabricación planificadas o en curso, por almacén.

Si lo disponible baja del mínimo, se propone llegar al máximo, redondeando al múltiplo. Los componentes que faltan para
fabricar se proponen aunque no tengan regla: lo que falta exactamente. La cantidad se pasa a la unidad de compra (factor
de compra) y se valora al precio de compra.

`POST /compras/propuesta/pedidos` crea un **pedido en borrador por proveedor** con las líneas elegidas. Las líneas sin
proveedor se omiten y se avisa de ellas.
