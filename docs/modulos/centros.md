# Centros de trabajo y cajas

Parte de la base (todas las ediciones). Entidades en Organización (`organizacion.centro`, `caja_centro`,
`acceso_centro`, con RLS), rutas `/centros`. Un **centro** es una tienda, delegación, almacén regional o planta de
la empresa; no confundir con el centro de coste de la analítica.

## Qué tiene un centro

- **Código** (`^[A-Z0-9_-]{1,10}$`, único en la empresa), nombre, dirección y si está **activo**: uno dado de baja no
  admite documentos nuevos (`centro.inactivo`). Se borra solo si no tiene documentos (`centro.en_uso`); al borrarlo
  se van sus cajas, sus accesos y sus series.
- **Almacén habitual**: las ventas del centro (factura, ticket, albarán) sacan primero de él, y las devoluciones y
  anulaciones vuelven a él (`IStockVentas` con el centro; `MovimientosInventario.SalidaVentaAsync` con almacén
  preferente). Sin almacén, el orden de siempre.
- **Cajas** (puntos de venta), con código único en el centro y alta/baja.
- **Series**: asignaciones de serie de ámbito `Centro` (y `Caja` para los tickets) en `/series/asignaciones`, con el
  centro o la caja en `TerceroId`. Orden al numerar: la de la caja (tickets) → la del cliente → la del centro → la de
  la empresa (`IResolverSerie.ResolverPrefijoAsync` con centro y caja).

## Documentos con centro

Facturas y tickets (con su caja), rectificativas (las del original), presupuestos, pedidos de venta, albaranes de venta
y facturas de proveedor llevan `centro_id`. Los que nacen de otro heredan el suyo: el pedido del presupuesto, el
albarán del pedido, la factura del presupuesto, del pedido o de los albaranes (no se facturan juntos albaranes de dos
centros: `albaranventa.centros_distintos`; la facturación masiva agrupa por cliente y centro).

El centro de un documento nuevo (`CentrosUsuario.ResolverAsync`):

1. El que trae el documento (`CentroId`; en el ticket también `CajaId`).
2. Si no trae, el **centro de trabajo** elegido en la aplicación (cabeceras `X-Centro` y `X-Caja`; la caja solo en tickets).
3. Si no, ninguno; o, si el usuario solo trabaja con un centro, ese.

Se comprueba que existe, que está activo, que el usuario trabaja con él (`centro.sin_acceso`, 403) y que la caja es de
ese centro y está activa.

## Usuarios limitados a unos centros

`PUT /centros/accesos/{usuarioId}` con la lista de centros (vacía: todos). Un usuario con centros asignados:

- solo ve en los listados (facturas, búsqueda de facturas, presupuestos, pedidos, albaranes, facturas de proveedor)
  los documentos de sus centros, no los de otros ni los que no tienen centro;
- no abre ni actúa sobre documentos de otros centros: `MiddlewareCentros` responde 404 (`centro.documento_ajeno`) en
  cualquier ruta `/facturas/{id}…`, `/presupuestos/{id}…`, `/pedidos-venta/{id}…`, `/albaranes-venta/{id}…` y
  `/gastos/{id}…`;
- tiene que hacer sus documentos en uno de sus centros (con uno solo, se pone solo).

Los informes y libros fiscales siguen siendo de toda la empresa (quien los consulta suele no estar limitado).

## Cierre de caja

`GET /centros/{id}/cajas/{cajaId}/cierre?dia=`: tickets del día en la caja (vivos y anulados), base, impuestos, total,
cobrado y pendiente, primer y último ticket.

## Interfaz

- **📍 Centro de trabajo** en la barra superior: se elige centro y caja (por usuario y empresa, en el navegador). El
  SPA lo manda en las cabeceras de cada petición, así que también vale para el editor de documentos.
- Configuración → Empresa → **Centros y cajas**: centros con almacén, series y cajas, y los usuarios con sus centros.
