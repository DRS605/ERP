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

## Albarán de venta (documento central)

Tercer eslabón y, como en el sector hortofrutícola (Hispatec), **el documento central de la venta**:
documenta qué, cuánto y a qué precio se entrega. Numera por empresa · ejercicio
(`TipoDocumento.AlbaranVenta`).

- **De pedido** (`POST /pedidos-venta/{id}/entregar`): toma el precio, el descuento y el IVA de la línea
  del pedido y actualiza lo servido (no se puede entregar más de lo pedido).
- **Directo** (`POST /albaranes-venta`), sin pedido: con artículo y sin precio toma el de la tarifa del
  cliente (o el del artículo); sin artículo, hay que indicar el precio.
- **Salida de existencias al emitirse** (almacén principal o existencia simple, kits por componentes).
  La factura que lo recoge ya no la mueve. Al anular un albarán sin facturar, la mercancía vuelve al
  almacén con el motivo «Anulación del albarán …».
- **Precio por fijar** (venta a resultas): una línea marcada `PrecioPorFijar` se entrega con un precio
  estimado y el albarán queda `PendienteValorar`; `PUT /albaranes-venta/{id}/valorar` fija los precios
  definitivos. No se factura hasta estar valorado.
- **Estados**: `PendienteValorar`, `PendienteFacturar`, `Facturado` y `Anulado`. Un albarán facturado
  no se anula: se anula (o rectifica) antes la factura.

## Facturación de albaranes

- `POST /albaranes-venta/facturar` recoge **uno o varios albaranes del mismo cliente en una factura**:
  una línea por línea de albarán («Alb. N · descripción», con `albaranVentaId`). La fecha de operación es
  la de la última entrega. Si el albarán viene de un pedido, se anota lo facturado en el pedido, que pasa
  a `Facturado` cuando está todo facturado.
- `POST /albaranes-venta/facturacion-masiva` factura los albaranes valorados y pendientes hasta una
  fecha (de un cliente o de todos): una factura por cliente o, con `UnaFacturaPorAlbaran`, una por
  albarán. Devuelve las facturas emitidas, los errores por cliente y los albaranes que quedaron fuera por
  estar sin valorar.
- **Anular la factura** devuelve sus albaranes a `PendienteFacturar` (y su pedido, a servido).
- Los albaranes anteriores a este cambio no sacaron la mercancía (`stock_descontado = false`): si se
  facturan ahora, la factura sí la saca.

## Devoluciones de venta

Siguen `DevolucionesVenta` de Hispatec. Una **devolución** (serie `DV`, numerada por ejercicio) se registra sobre un
albarán y recoge qué líneas y cuánto vuelve.

- **Límite:** nunca se devuelve más de lo entregado. Se suman las devoluciones anteriores que no estén anuladas (`devolucion.excede`). `GET /devoluciones-venta/devolubles/{albaranId}` da, por línea, lo entregado, lo devuelto y lo que aún se puede devolver.
- **Almacén:** la mercancía marcada «reingresa» vuelve al almacén al registrar la devolución. La que no, se considera merma o destruida en destino.
- **Abono:**
  - si el albarán **aún no está facturado**, su factura sale con lo devuelto descontado; una línea devuelta entera no sale. La devolución queda *abonada en la factura del albarán*. Si se anula esa factura, la devolución vuelve a quedar pendiente;
  - si el albarán **ya está facturado**, «Abonar» (`POST /devoluciones-venta/{id}/abonar`) emite una **rectificativa por sustitución** de la factura vigente con las cantidades devueltas descontadas. La factura vigente es la del albarán o la última rectificativa que la sustituye, de modo que varias devoluciones se encadenan;
  - no se rectifica una factura que descuenta anticipos (se hace a mano), ni se deja una rectificativa sin líneas: si se devuelve la factura entera, se anula;
  - también se puede cerrar **sin abono**, por ejemplo si la mercancía se repone.
- **Anular:** solo una devolución no abonada; lo que reingresó vuelve a salir del almacén.
- **Límites en otros documentos:** un albarán con devoluciones no se anula (`albaranventa.con_devoluciones`). Uno devuelto entero no se factura (`albaranventa.devuelto_entero`).
- **Conceptos de línea:** los cargos y abonos de la línea del albarán se copian tal como se aplicaron; no se prorratean por lo devuelto.

## Reclamaciones sobre ventas

Siguen «Reclamaciones sobre ventas» y «Conceptos para reclamaciones» de Hispatec.

- **Conceptos:** un maestro por empresa, con código único (calidad, calibre, retraso, rotura…). Se da de baja sin borrarse; uno de baja no sirve para reclamaciones nuevas.
- **Reclamación** (serie `RC`): del cliente, opcionalmente sobre un albarán o una factura suyos, con concepto, descripción e importe reclamado.
- **Ciclo:**
  - *abierta* → *en trámite* (con responsable) → *resuelta* (aceptada, aceptada en parte o rechazada);
  - al resolverla se indican lo reconocido, la explicación y, si la hay, la devolución o la rectificativa que la compensa;
  - una reclamación rechazada no reconoce importe;
  - una resuelta se puede reabrir, y una registrada por error se anula.
- **Informe** (`GET /reclamaciones/informe`): totales, abiertas, lo reclamado y lo reconocido, y los días medios de resolución, por concepto y por cliente.

Pantallas: *Ventas → Devoluciones*, *Ventas → Reclamaciones* (con el informe y los conceptos) y el botón «Devolver» en la
lista de albaranes. Tablas: `facturacion.devolucion_venta`, `linea_devolucion_venta`, `reclamacion_venta` y
`concepto_reclamacion`, todas con RLS. Tienen comprobaciones de estado y de cantidades positivas, y numeración única por
empresa y ejercicio (migración `DevolucionesReclamaciones`). Pruebas: `DevolucionesReclamacionesTests`.

## Facturación del pedido

`POST /pedidos-venta/{id}/facturar` genera una **factura real** reutilizando `EmitirFactura` (con su
numeración fiscal, huella VeriFactu, vencimientos y contabilización), y enlaza la factura al pedido
(`FacturaId`), dejándolo en estado `Facturado`. Sin albaranes, factura el pedido entero y saca la
mercancía; con albaranes, recoge los pendientes (su mercancía ya salió) más lo que quede sin servir. No
se puede facturar un pedido en borrador ni facturarlo dos veces.

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
| `GET` | `/albaranes-venta` | `factura.leer` | Albaranes (filtro `clienteId`, `estado`, `desde`, `hasta`). |
| `GET` | `/albaranes-venta/{id}` | `factura.leer` | Albarán con sus líneas y precios. |
| `POST` | `/albaranes-venta` | `factura.emitir` | Albarán directo (saca la mercancía). **201** |
| `PUT` | `/albaranes-venta/{id}/valorar` | `factura.emitir` | Fija los precios de las líneas por fijar. |
| `POST` | `/albaranes-venta/{id}/anular` | `factura.emitir` | Anula un albarán sin facturar (la mercancía vuelve). |
| `POST` | `/albaranes-venta/facturar` | `factura.emitir` | Una factura con varios albaranes del cliente. **201** |
| `POST` | `/albaranes-venta/facturacion-masiva` | `factura.emitir` | Facturas por cliente (o por albarán) hasta una fecha. |

## Persistencia

- Esquema **`facturacion`**: tablas `pedido_venta` (+ `linea_pedido_venta`) y `albaran_venta`
  (+ `linea_albaran_venta`), con RLS por empresa en los cabeceros; las líneas se protegen a través de
  su padre (filtro global de EF Core).
- Índice único `(empresa_id, ejercicio, numero)` en `pedido_venta`.
- Migraciones: `CicloVenta`; `AlbaranVentaCentral` (precios de línea, precio fijado, factura, stock
  descontado y albarán sin pedido; rellena los existentes con el precio de su pedido y su factura).

## Tests

- **Integración** (`VentasCicloEndpointsTests`): el pedido calcula el total y arranca en borrador; no
  se factura sin confirmar; el albarán actualiza lo servido (y no permite entregar de más); facturar
  un pedido confirmado genera una factura real, la enlaza y no permite refacturar; y se puede crear un
  pedido a partir de un presupuesto.

- **Integración** (`AlbaranesVentaTests`): el albarán directo saca stock; uno a precio por fijar no se
  factura hasta valorarlo; dos albaranes del cliente en una factura que no vuelve a sacar la mercancía;
  un albarán facturado no se anula; anular la factura los deja pendientes y anular el albarán devuelve el
  stock; la facturación masiva hace una factura por cliente y deja fuera los no valorados; el pedido se
  factura con sus albaranes y lo no servido, y la mercancía sale una sola vez.

## Futuro (documentado)

PDF del albarán, conceptos de línea (cargos y abonos) en el albarán, un albarán directo a un cliente del
grupo como traspaso (hoy el traspaso sigue al pedido), precios que informa el cliente (venta en
comisión) y criterio de facturación por cliente (agrupada o por albarán).
