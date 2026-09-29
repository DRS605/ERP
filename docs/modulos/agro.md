# Agro (hortofrutícola)

Módulo contratable **`agro`**. Es **sectorial**: no entra en ninguna edición, ni siquiera en la
Completa. Se contrata aparte en *Ajustes → Plan*. Rutas: `/agro/*`. Permisos:

- `agro.leer`: consultar.
- `agro.gestionar`: recepciones, partes y palés.
- `agro.liquidar`: precios, descuentos y liquidaciones. Va aparte porque genera facturas.

Cubre el circuito de un almacén de fruta, portado del Clon (`DRS605/Clon`, migraciones `0007`, `0008`
y `0010`, y los módulos de dominio `growerSettlement.ts` y `workOrder.ts`) y mejorado (ver
[§9](#9-respecto-al-clon)):

```
agricultor ─► recepción (pesadas, envases) ─► partida ─┬─► clasificación ─► liquidación ─► autofactura (gasto)
                                                         └─► confección ─► partidas de producto ─► palé SSCC ─► expedición
```

Por decisión de producto **no hay recursos humanos**: la mano de obra aparece solo como coste en los
partes de confección, con categoría y tarifa. No hay nóminas, contratos ni control de presencia.

## 1. Maestros

| Maestro | Qué es |
|---|---|
| **Campaña** | Periodo agrícola (por ejemplo, de septiembre a agosto). Las campañas de una empresa no se solapan. |
| **Agricultor** | Un **proveedor** de Terceros con su ficha agrícola, detallada debajo de esta tabla. |
| **Parcela** | Del agricultor, con referencia **SIGPAC** (7 campos), superficie, cultivo, variedad y, opcionalmente, un **centro analítico** para el coste por kilo. |
| **Categoría** | Extra, 1ª, 2ª, destrío… |
| **Artículo de campaña** | Cómo se liquida cada producto en la campaña: **por clasificación** o **por periodo**. |
| **Precio de liquidación** | €/kg por producto (y categoría) y fechas, sin solapes. |
| **Descuento** | Por kilo, porcentaje del bruto o importe fijo. |
| **Tarifa de coste** | €/hora por categoría y tipo de hora (normal, extra, nocturna, festiva), o €/pieza a destajo. También para maquinaria. Con vigencia y sin solapes. |

La ficha agrícola del agricultor recoge:

- el **régimen**: REAGP o general;
- el **impuesto de la autofactura**: `REAGP12` por defecto en el REAGP, `IVA4` en el general, o el IGIC que corresponda en Canarias;
- la **retención de IRPF**: 2 % por defecto;
- la **fecha en que autorizó la autofacturación**;
- un **motivo de bloqueo**, si lo tiene.

## 2. Recepción de fruta

1. **Borrador.** Se indican el agricultor, la fecha (la campaña se deduce), la matrícula y el conductor.
2. **Líneas.** Cada una es un producto, que debe medirse en **kg**, con:
   - la parcela, que debe ser del agricultor y estar activa;
   - la fecha de recolección, que no puede ser posterior a la recepción;
   - el envase (palot, caja…), el precio estimado y el calibre.
3. **Pesadas de báscula** por línea: bruto, tara (neto = bruto − tara) y envases.
4. **Confirmar.** Si algo falla, se devuelven **todos los problemas a la vez**: líneas sin pesadas, envases
   sin artículo, fechas, agricultor bloqueado, fecha fuera de la campaña. Si todo está bien:
   - se asigna el número **sin huecos** (`REC-2026-000001`), también con confirmaciones simultáneas;
   - cada línea crea su **partida** (`REC-2026-000001/1`) con los kilos netos;
   - se registra la **devolución de los envases** en que llegó la fruta.
5. **Anular.** Solo se puede si sus partidas no se han confeccionado, paletizado ni liquidado. Invierte los
   movimientos de kilos y de envases.

**Envases del agricultor.** Hay un libro de movimientos: la entrega de vacíos suma, la devolución resta.
Su saldo son los envases de la empresa que tiene el agricultor.

## Envases por tercero (clientes, proveedores, transportistas y pools)

Como en Hispatec, cada tercero puede tener una **cuenta de envases** (`agro.cuenta_envases`, única por tipo y tercero).
Los tipos son cliente, proveedor, transportista o **pool** (CHEP, IFCO, Euro Pool… dados de alta como proveedor). El
saldo, los envases de la empresa que tiene el tercero, **se calcula del libro**; no hay contadores.

- **Libro** (`agro.movimiento_envases` y sus líneas):
  - cada movimiento se numera sin huecos por ejercicio (`ENV-2026-000001`) y es de **solo inserción**;
  - las líneas llevan el envase (un artículo del catálogo) y la cantidad con signo: **+ entregado** al tercero, **−
    recogido**;
  - se anula con su **contrario**: un movimiento de origen `Anulacion` que apunta al anulado. Un movimiento solo se
    anula una vez (índice único).
- **Orígenes:** manual (entrega o recogida), regularización, expedición, recepción (el libro del agricultor), facturación y anulación.
- **Al expedir palés a un cliente**, se entregan solos la **caja** y el **palé retornables** de su plantilla (campos
  nuevos de la plantilla). Se entregan tantas cajas como lleve el palé y un palé. Si el cliente no tiene cuenta, se abre
  sola. Todo se guarda en la misma transacción que la expedición.
- **Al anular la expedición**, sus envases vuelven con el contramovimiento.
- **Opciones de la cuenta:**
  - **Agrupadora** (la «cuenta familiar»): los movimientos se acumulan en ella, por ejemplo los de las tiendas en su
    central. No se admiten ciclos.
  - **Llevar al transportista:** en un cliente, lo que se le entrega va a la cuenta del transportista de la expedición.
  - **Bloqueo:** con `Aviso` se deja mover y se avisa; con `Bloqueo` no se admite ningún movimiento.
  - **Límite:** por encima, se avisa y la cuenta sale marcada.
- **Límites por envase:**
  - como el límite y el mínimo por cuenta y artículo de Hispatec: cada envase de la cuenta puede tener un **límite** (lo máximo que tiene el tercero) y un **mínimo** (lo que debe conservar);
  - `PUT /agro/envases/cuentas/{id}/limites` los fija junto con el **control**: con `Aviso` el movimiento se registra y avisa; con `Bloqueo`, una entrega que deja el envase por encima del límite, o el total por encima del límite general, o una recogida que lo deja por debajo del mínimo, no se registra (`envases.limite`) salvo que se **fuerce** (`Forzar: true`). El movimiento forzado queda anotado en sus observaciones;
  - la entrega automática al expedir palés solo avisa, para no parar la expedición;
  - el mínimo no puede superar el límite. La tabla es `agro.limite_envase`, con RLS por su cuenta.
- **Cierre de periodo** (`FechaBloqueoMovimientoArticRetor` de Hispatec): `PUT /agro/envases/configuracion` con `FechaCierre`. Hasta esa fecha, inclusive, no se registra ni se anula ningún movimiento (`envases.periodo_cerrado`); con `null` se reabre.
- **Informe de límites** (`GET /agro/envases/informe-limites?sinMovimientosDesde=`): cuentas que superan su límite general o el de un envase, cuentas por debajo de un mínimo y, con la fecha, cuentas con saldo y sin movimientos desde entonces.
- **Extracto:** saldo inicial, movimientos con el acumulado y saldo final, en total y por envase.
- **Pantalla:** Agro → **Envases**.
- **API:**
  - `GET/POST /agro/envases/cuentas`, `PUT /agro/envases/cuentas/{id}`, `GET /agro/envases/cuentas/{id}/extracto`;
  - `GET/POST /agro/envases/movimientos`, `POST /agro/envases/movimientos/{id}/anular`.
- **Errores:**
  - de la cuenta: `envases.cuenta_existe`, `envases.cuenta_bloqueada`, `envases.cuenta_inactiva` y
    `envases.agrupadora`;
  - del movimiento: `envases.sin_lineas` y `envases.ya_anulado`.

- **Envases a facturar** (Hispatec: envases a retornar o a facturar). La **gestión** de la cuenta de un cliente
  (`cuenta_envases.gestion`) dice qué se hace con los envases que tiene:
  - `Retornar` (por defecto): se devuelven. Se le pueden facturar a mano indicando qué envases;
  - `Facturar`: se le factura todo su saldo;
  - `FacturarExceso`: solo lo que pasa del **límite por envase** (sin límite de ese envase, nada).
  - `POST /agro/envases/cuentas/{id}/facturar` (sin líneas, según la gestión; con líneas, envase, cantidad y precio)
    emite un **albarán de venta directo** con los envases, a su precio de tarifa o del artículo si no se indica, y un
    movimiento de origen `Facturacion` que los saca de su saldo. No se factura más de lo que tiene (`envases.supera_saldo`),
    ni en una cuenta que acumula en otra (`envases.facturar_agrupada`) ni a quien no es cliente (`envases.gestion_cliente`).
  - `POST /agro/envases/facturar` lo hace con todos los clientes a facturar (un albarán por cliente); los que fallan
    salen en `omitidas`.
  - **Anular** el movimiento anula el albarán (si aún no está facturado) y los envases vuelven a su saldo.
  - El albarán se factura como cualquier otro.
- **Libro del agricultor unido.** Las recepciones (envases llenos que trae), las entregas y devoluciones de vacíos al
  agricultor se llevan también al libro por tercero, en la **cuenta de su proveedor** (se abre sola), con origen
  `Recepcion`. Así el agricultor aparece en el extracto, los límites, el informe y el stock en terceros.
  - Al anular la recepción se anula su movimiento. Un movimiento de recepción no se anula suelto (`envases.de_recepcion`).
  - La recepción no se para por el bloqueo o los límites de la cuenta: solo avisa.
  - La ficha del agricultor sigue mostrando su libro propio (`agro.movimiento_envase`). Los movimientos anteriores a
    este cambio se pasaron al libro por tercero como una regularización por agricultor con su saldo.
- **Pools.** En la cuenta de un pool se indican **sus envases** (`agro.envase_pool`). El **fichero de declaración**
  (`GET /agro/envases/cuentas/{id}/fichero-pool?desde=&hasta=`) es un CSV (UTF-8, `;`) con los movimientos de esos
  envases con todos los terceros en el periodo: fecha, movimiento, tipo y nombre del tercero, NIF, código y nombre del
  envase, entregado, recogido, documento y matrícula.
- **Stock en terceros** (`GET /agro/envases/stock-terceros?hasta=`): por envase, lo que tienen clientes, proveedores y
  agricultores, transportistas y pools (en negativo lo que se debe, normalmente al pool). Es el stock del envase que
  está fuera del almacén.
- **Impresos:** justificante de un movimiento para firmar el tercero (`GET /agro/envases/movimientos/{id}/pdf`) y
  extracto de la cuenta (`GET /agro/envases/cuentas/{id}/extracto/pdf?desde=&hasta=`).

- **Stock del envase vacío en el almacén:** con «Mover los envases retornables» en la configuración agro, cada
  movimiento de envases mueve el inventario del artículo envase: lo entregado a un tercero sale del almacén y lo
  recogido entra. Una facturación de envases entra y su albarán sale, así que no cambia nada (los envases ya salieron
  al entregarse).

## Reservas de palés a pedidos de venta

Como en Hispatec, un palé **cerrado** puede apartarse para una línea de un pedido de venta confirmado
(`agro.reserva_pale`). Un palé solo tiene **una reserva activa** (índice único parcial).

- **Reservar:**
  - el palé debe estar cerrado, sin expedir y ser del artículo de la línea;
  - no se reserva más de lo pendiente de la línea (pedido − servido − ya reservado), salvo con **Forzar**;
  - la reserva guarda los kilos y las cajas del palé en ese momento.
- **Expedir:** un palé reservado **solo sale con su pedido**. Si se expide sin pedido o con otro, el error es
  `pale.reservado`. Al expedirlo con su pedido, la reserva queda **consumida**.
- **Anular la expedición:** la reserva vuelve a estar **activa**.
- **Anular la reserva:** con motivo; el palé queda libre para otro pedido.
- **Pantalla:** en el pedido de venta, el botón **Reservar palés** (con Agro contratado). Muestra lo reservado y lo
  pendiente por línea, las reservas y los palés disponibles del artículo.
- **API:**
  - `GET /agro/reservas?pedidoVentaId=`;
  - `GET /agro/reservas/activas`;
  - `POST /agro/reservas`;
  - `POST /agro/reservas/{id}/anular`.
- **Errores:** `reserva.pedido_cerrado`, `reserva.linea`, `reserva.sin_pales`, `reserva.pale_no_cerrado`,
  `reserva.pale_reservado`, `reserva.producto`, `reserva.supera_pendiente` y `reserva.no_encontrada`.

## 3. Partidas y palés

La **partida** (lote) es la unidad de trazabilidad. Sus kilos disponibles son la suma de su **libro de
movimientos**, de solo inserción:

| Movimiento | Qué hace |
|---|---|
| Entrada | Llegan kilos a la partida. |
| Consumo | Sale en confección. |
| Paletizado | Pasa de suelto a palé, o de un palé a otro. |
| Expedición | Sale en un palé hacia el cliente. |
| Ajuste | Merma o regularización. |
| Anulación | Invierte otro movimiento. |

El saldo, tanto suelto como en cada palé, **nunca es negativo**.

El **palé** se identifica con un **SSCC** GS1 de 18 dígitos: extensión + prefijo de empresa + serie + dígito
de control, calculado y comprobado. Sus estados son: abierto ⇄ cerrado → expedido. La **expedición** saca
todo el contenido de los palés cerrados hacia un cliente, con referencia de albarán o carta de porte.

### Palés rápidos (plantillas, cajas y montaje de una vez)

Lo que en Hispatec es la *confección* de palé:

- **Plantilla de palé** (`/agro/plantillas-pale`):
  - datos: tipo de palé, producto (opcional), marca, **cajas por palé**, **kilos por caja**, mosaico (filas ×
    columnas por capa; las cajas deben ser capas enteras) y cliente habitual;
  - si ya se han montado palés con ella, no cambian sus cajas, kilos ni producto: se crea otra y se desactiva;
  - se elimina solo si no se ha usado.
- **Montaje de una vez** (`POST /agro/pales/montar`):
  - con una plantilla y una partida se crean los palés con SSCC correlativos;
  - se monta una de tres cantidades:
    - todos los palés completos que dan los kilos sueltos de la partida;
    - un número de palés;
    - un número de cajas (el último palé queda abierto si no se llena);
  - los palés completos quedan **cerrados**.
- **Por cajas** (`POST /agro/pales/{id}/cajas`):
  - en un palé con plantilla se ponen o se sacan cajas (en negativo);
  - los kilos son las cajas por los kilos por caja;
  - el palé **se cierra solo** al completarse;
  - no se paletiza por kilos.
- **Libro de movimientos:** cada movimiento guarda sus cajas (con el mismo signo que los kilos), así que el
  contenido del palé se ve en cajas y kilos por partida.
- **Etiqueta logística** (`GET /agro/pales/{id o SSCC}/etiqueta`):
  - PDF A6 con dos códigos GS1-128:
    - el del SSCC (IA 00);
    - el del contenido: peso neto (IA 310n), cajas (IA 37) y lote (IA 10);
  - el codificador está contrastado con otro independiente (bwip-js) en las pruebas.
- **Carta de porte al expedir** (`CartaPorte = true` en `/agro/expediciones`):
  - se emite en facturación, con una línea por producto (bultos = cajas, peso = kilos), y queda enlazada a los
    palés;
  - si la expedición no llega a guardarse, la carta se anula;
  - al anular la expedición de todos sus palés, también se anula.
- **Albarán del pedido al expedir** (`PedidoVentaId` en `/agro/expediciones`):
  - se emite el albarán de venta del pedido con lo expedido, repartido en sus líneas pendientes del mismo artículo;
  - va en kilos si el artículo se vende por kilos, y en cajas si no;
  - el cliente es el del pedido;
  - si lo expedido no cabe en lo pendiente, no sale nada;
  - al anular la expedición de todos sus palés, el albarán se anula y lo servido vuelve a quedar pendiente.

## 4. Clasificación

Un **muestreo** dice cuántos kilos de la muestra salieron de cada categoría. Puede ser:

- **Provisional.**
- **Definitiva.** Solo puede haber una por partida, y es la que cuenta para liquidar. Se puede sustituir
  por otra, salvo que la partida ya esté en una liquidación.

## 5. Liquidación al agricultor

La liquidación reúne las entregas confirmadas del agricultor en la campaña y el periodo, siempre que no
estén ya en otra liquidación. Así se valora (portado de `growerSettlement.ts`):

| Concepto | Cálculo |
|---|---|
| Bruto | Por clasificación: los kilos netos de cada partida se reparten por categorías **sin perder un gramo**. Por periodo: el precio vigente en la fecha de entrega. Cada línea guarda el precio aplicado. |
| Cargos y abonos | Por kilo, porcentaje del bruto, **por envase** recibido (bulto) o fijo. Un abono (bonificación) suma en vez de descontar. |
| Base | Bruto − descuentos (nunca negativa). |
| Impuesto | Compensación REAGP (12 %) o IVA (4 %) sobre la base. |
| Retención | Porcentaje de IRPF sobre la base. |
| Total factura | Base + impuesto. |
| A pagar | Total − retención. |

**Cargos y abonos de las recepciones** (como los cargos por agricultor, artículo y envase de Hispatec). Cada concepto
de liquidación (`agro.concepto_liquidacion`) puede valer solo para un **agricultor**, un **artículo** o un **envase**:

- con artículo o envase, se calcula solo sobre las entregas que encajan: sus kilos, su importe o sus envases (por
  ejemplo, 0,50 € por palot recibido);
- si no encaja ninguna entrega, no sale;
- con `Abono`, es una bonificación (un premio de calidad): el importe va en negativo y aumenta la base;
- la base sigue sin poder ser negativa. En el borrador y en la autofactura cada concepto sale con su base y su importe.

Sin precio o sin clasificación definitiva es un **error**, nunca un cero, y la **previsualización**
devuelve todos los problemas de una vez.

**Ciclo.** Previsualizar → borrador (reserva las entregas) → recalcular → **emitir**. Al emitir:

- se exige que el agricultor haya **autorizado la autofacturación** antes de la fecha de la liquidación;
- se exige que la valoración siga vigente: si han cambiado precios, clasificaciones o descuentos, hay que
  recalcularla y revisarla;
- se numera sin huecos (`LIQ-2026-000001`);
- se registra la **autofactura como gasto** del proveedor-agricultor, con la compensación o el IVA y la
  retención. Con eso entra en el libro de IVA soportado, en el **303**, en el **111** y en la contabilidad
  (asiento en la 472 y la 4751) y queda **pendiente de pago** en tesorería;
- la partida recibida queda valorada al precio liquidado (€/kg), que después usa la confección.

En el **303**, las compensaciones REAGP se muestran aparte (casillas 42-43) dentro de lo deducible.

**REAGP del IGIC (Canarias).** En una empresa de Canarias el agricultor del REAGP liquida con `REAGPIGIC`: la
autofactura **no lleva compensación a cargo del adquirente** (cuota 0) y solo la retención. Es el agricultor quien pide a
la Hacienda Canaria el reintegro de la compensación por sus envíos y exportaciones fuera de Canarias (modelo 422).

- Sin código de impuesto, el REAGP toma `REAGPIGIC` en Canarias y `REAGP12` en el resto; en régimen general, en
  Canarias hay que indicar el tipo de IGIC.
- El impuesto del agricultor tiene que ser el del territorio de la empresa (`agricultor.impuesto`); si la empresa cambia
  de territorio, la liquidación avisa hasta que se corrija la ficha (`agricultor.impuesto_territorio`).
- **A confirmar con el asesor:** esta regla sale de la documentación pública del modelo 422; conviene revisarla con el
  texto refundido del IGIC (Decreto Legislativo 1/2025, arts. 56 a 67) antes de usarla.

**Anular** una liquidación emitida anula su autofactura con un **contraasiento**, siempre que la autofactura
no esté pagada. Sus entregas quedan libres para otra liquidación.

### Tipos de precio, fijación masiva y precios a resultas

Como la valoración de compras de Hispatec (`PreciosValoracionAlbaranCompra`):

- **Tipos de precio:** cada precio de liquidación es **general**, **de periodo** o **del día** (desde = hasta), y puede
  limitarse a un **envase** de la entrega.
- **Valoración:** entre los precios vigentes gana el del día, luego el del periodo y luego el general. En cada tipo, el
  del envase de la entrega va antes que el que no tiene envase; a igualdad, el tramo más corto.
- **Solapes:** solo se impiden entre precios del mismo tipo, artículo, categoría y envase. La comprobación está en la
  aplicación y en la base de datos.
- **Fijación masiva** (`POST /agro/campanas/{id}/precios/masivo`):
  - muchos precios de una vez;
  - con `sustituir`, el de la misma clave y fechas se actualiza, salvo si ya lo usa una liquidación;
  - los que no se pueden guardar vuelven con su motivo y el resto se guarda.
- **A resultas** (`POST /agro/campanas/{id}/precios/propuesta-ventas`), como la valoración según ventas de Hispatec:
  - calcula, por artículo de la campaña, el precio medio de venta en las fechas: albaranes valorados y facturas vivas sin
    albarán, con su descuento y sin conceptos;
  - le resta un porcentaje y un importe por kilo;
  - no guarda nada: la propuesta se fija como precio del periodo.
- **Pantalla:** Agro → Maestros → **Fijación masiva** y **Desde ventas**. El alta de un precio pide el tipo y el envase.
- **Errores:** `precio.dia`, `precio.tipo`, `precio.solapado`, `precio.en_uso`, `precio.sin_precios` y
  `precio.propuesta`.

## 6. Confección

El parte de confección tiene:

- **Consumos:** partidas y kilos, sueltos o tomados de un palé.
- **Mano de obra:** descripción, categoría, tipo de hora, horas y piezas si es a destajo.
- **Maquinaria:** categoría y horas.
- **Materiales:** artículos del catálogo, valorados a su precio de compra.
- **Salidas:** producto en kg, kilos, **factor** de reparto, calibre, categoría y palé de destino.

**Valoración** (portada de `workOrder.ts`):

- **Tarifa vigente obligatoria.** Sin ella, error: nunca se valora a cero por olvido.
- **Destajo:** se paga por piezas; las horas se registran pero no se valoran.
- **Coste de la fruta:** el liquidado si ya se liquidó; si no, el precio estimado de la recepción; si no hay
  ninguno, error.
- **Materiales sin coste:** error.
- **Indirectos:** porcentaje sobre la mano de obra y la maquinaria.
- **Reparto del coste total** entre las salidas, **al céntimo**, por kilos o por kilos × factor (el destrío
  suele llevar factor 0).
- **Límite de kilos:** no se puede obtener más de lo consumido; la diferencia es la merma.

**Validar.** Consume las partidas y crea una partida por salida, con su coste por kilo y, si se indicó, en
su palé. También registra la **genealogía** y numera el parte (`PC-2026-000001`).

**Anular.** Solo si las salidas no se han movido. Los kilos vuelven a su origen.

**Reparto por tiempo teórico** (como Hispatec):

- **Rendimientos** (`agro.rendimiento_confeccion`): cajas por hora de cada producto confeccionado y envase, o de
  cualquier envase si no se indica.
- **Datos del parte:** cada salida puede llevar sus **cajas** y su **envase**. Cada línea de mano de obra es de
  **confección** o de **apoyo**.
- **Reparto:** con `Reparto = PorTiempoTeorico`, la mano de obra de confección se reparte entre las salidas por
  cajas × 3600 / rendimiento. Todo lo demás (fruta, materiales, apoyo, maquinaria e indirectos) va por kilos. Cada
  parte se reparte al céntimo.
- **Errores:** si hay confección que repartir, una salida sin cajas o sin rendimiento es un error
  (`parte.sin_rendimiento`), nunca un cero.
- **Qué se guarda por salida:** los segundos teóricos y la parte de confección de su coste.
- **API:** `GET/PUT /agro/rendimientos` y `DELETE /agro/rendimientos/{id}`.

**Coste del palé en la venta:**

- **Al expedir con pedido:** el coste por kilo de cada producto es la media de sus partidas cargadas (confeccionadas o
  liquidadas), si todas lo tienen. Pasa a la línea del albarán (`linea_albaran_venta.coste_unitario`), por kilo o por
  caja según la unidad de venta.
- **Al facturar:** la factura usa ese coste, no el precio de compra del artículo. Así el margen de la factura es el
  real.

## 7. Trazabilidad

Cumple el Reglamento (CE) 178/2002 («un paso atrás, un paso adelante») y lo supera: recorre la genealogía
entera, a cualquier profundidad.

- **Hacia atrás**, desde un SSCC o una partida: el árbol de partidas y partes, y el **origen en el campo**
  (recepción, agricultor, parcela, SIGPAC, fecha de recolección y kilos).
- **Hacia delante**, desde una recepción o una partida: las partidas confeccionadas y los **palés**,
  expedidos (cliente, fecha, albarán) o todavía en almacén.

## 8. Informe de campaña

`GET /agro/informes/campana/{id}`:

- **Por agricultor:** entregas, kilos entregados, liquidados y pendientes, importe de la fruta, €/kg medio y
  total a pagar.
- **Por parcela:** kilos, **kg/ha**, precio medio liquidado y **coste de cultivo por kilo**. Este último es el
  gasto imputado en [analítica](analitica.md) al centro de la parcela durante la campaña, dividido entre sus
  kilos. Así se resuelve el pendiente de la fase 4: la analítica da el coste y el módulo agro, los kilos.
- **Por producto confeccionado:** kilos, coste y **€/kg**.

## 9. Respecto al Clon

| Clon | ALXOR |
|---|---|
| Stock de fruta en el inventario general, por unidad logística | Libro de partidas propio, suelto o en palé, con saldo no negativo garantizado en la base de datos |
| Liquidación con el gasto a mano | La emisión registra la autofactura (gasto con compensación y retención) y la anulación, su contraasiento |
| Numeración al confirmar | Igual, y además sin huecos con confirmaciones simultáneas (bloqueo + comprobación en la base de datos) |
| Precios con `daterange` y exclusión GiST | Trigger sin extensiones; los precios aplicados en una emitida no cambian |
| Coste por kilo fuera del sistema | Informe de campaña con kg/ha, precio medio y coste de cultivo por kilo desde la analítica |
| Partes de trabajo con empleados | Mano de obra por categoría y tarifa, sin RRHH (decisión de producto) |

## 10. Garantías en la base de datos

Ver [garantías de la base de datos](../garantias-base-datos.md). Todas se cumplen aunque se escriba
saltándose la aplicación:

- **Aislamiento:** RLS forzada por empresa en las raíces y por cabecera en las líneas.
- **Documentos:**
  - recepciones, liquidaciones, partes y clasificaciones solo cambian en borrador y, después, solo en su
    transición (confirmada → anulada, emitida → anulada…);
  - sus líneas solo cambian en borrador o en la misma transacción del cambio de estado;
  - la numeración no tiene huecos.
- **Libros de solo inserción:** los movimientos de partidas y de envases y la genealogía.
- **Saldos:** el de cada partida, suelto y en cada palé, nunca es negativo. Una partida anulada no se mueve.
  Un palé solo admite movimientos compatibles con su estado, y uno expedido ya no cambia. El SSCC y la plantilla
  de un palé no cambian, y solo un palé expedido lleva carta de porte. Las cajas de un movimiento llevan el signo
  de sus kilos, y la plantilla tiene cajas y kilos positivos y un mosaico coherente.
- **Cuadres:**
  - **recepción:** cada línea, con el neto y los envases de sus pesadas y con su partida;
  - **liquidación:**
    - cada línea cumple importe = round(kilos × precio, 2);
    - se liquidan todos los kilos netos de cada entrega, y cada entrega en una sola liquidación viva;
    - cada línea lleva el precio vigente;
    - los totales cuadran: base, cuota, retención, total y a pagar;
    - emitir exige la autofacturación autorizada;
  - **parte:** el coste de cada línea cuadra y el total se reparte entero entre las salidas;
  - **genealogía:** coherente con el parte.
- **Valores:** SSCC con dígito de control GS1, SIGPAC, pesadas (bruto > tara ≥ 0), régimen coherente con el
  impuesto, y precios y tarifas sin solapes.

## Órdenes de carga

Es una versión ligera de `OrdenesCarga` de Hispatec, montada sobre la expedición de palés que ya existe. Tablas:
`agro.orden_carga`, `linea_orden_carga` y `pale_orden_carga`, con RLS; migración `OrdenesCarga`.

- **Cabecera:**
  - número sin huecos por ejercicio (`OC-2026-000001`) y fecha de carga;
  - muelle, transportista, vehículo, matrícula, conductor y temperatura de consigna;
  - filas × columnas del camión, de 1 a 20 × 1 a 6, para colocar los palés;
  - si al finalizar se emite la carta de porte;
  - observaciones.
- **Estados:** *propuesta* (se prepara, no se carga) → *pendiente* → *en carga* → *finalizada*, o *anulada*. Con `Propuesta: true` nace en propuesta (`OrdenCargaEstadoPorDefectoPropuesta`) y se libera después.
- **Montaje:**
  - `GET /agro/ordenes-carga/pendientes` da las líneas de pedidos confirmados con algo pendiente de servir: lo pedido, lo servido, los palés reservados y los ya previstos en otras órdenes abiertas;
  - cada línea de la orden es una línea de pedido con los **palés previstos** y, si el camión tiene distribución, su posición;
  - una línea con palés cargados no se quita (`NoPermitirModificarConLineasCargadasOC`).
- **Carga** (`POST …/cargar` con el SSCC). El palé:
  - tiene que estar cerrado y no cargado en otra orden abierta;
  - si está reservado, va a la línea de su reserva, que tiene que estar en la orden; un palé reservado a otro pedido no entra (`CargaPDAPermitirULOtrosPedidos`);
  - si no está reservado, va a la línea indicada o a la única del artículo del palé con hueco; si vale para varias, se pregunta cuál;
  - tiene que llevar el artículo de la línea, y la línea no admite más palés que los previstos;
  - se puede descargar mientras no haya salido.
- **Finalizar** (`POST …/finalizar`):
  - agrupa los palés cargados por pedido y los expide con la expedición de siempre: **un albarán por pedido**, la carta de porte si se pidió (una por pedido, porque es un destinatario) y el movimiento de envases;
  - la orden queda enlazada a sus albaranes;
  - si un pedido falla, los demás ya han salido; la orden sigue en carga con lo pendiente y se puede volver a finalizar.
- **Anular:** solo si no ha salido nada; los palés cargados quedan libres.
- **Hoja de carga:** `GET /agro/ordenes-carga/{id}/pdf`, con las líneas, los SSCC cargados y su posición, y el esquema del camión por filas.
- **Pantalla:** *Agro → Órdenes de carga*. Tiene el lector de SSCC (se escanea y se pulsa Intro), las líneas, los palés cargados y el camión dibujado por posiciones.
- **Fuera de alcance** (P3): la PDA con lector, la situación de los muelles, las pilas de palés, el traspaso entre centros con recepción de la orden y las integraciones de temperatura o de control de accesos.

## Partidas en el inventario del almacén

Con «Reflejar las partidas» en la configuración agro (`ReflejarPartidasEnInventario`), lo que mueven las partidas se
refleja en el inventario general del artículo (su existencia o, si la empresa trabaja con almacenes, su almacén
principal), para una valoración única de existencias:

- la **recepción** entra los kilos netos y su anulación los saca;
- el **parte de confección** saca lo consumido y entra lo obtenido (otro artículo);
- los **ajustes** (merma, recuento) entran o salen;
- la **expedición sin albarán** sale y su vuelta entra. La expedición **con albarán** no cuenta: el stock lo saca el
  albarán, y su anulación o la devolución de una vuelta parcial lo devuelven;
- el paletizado no cambia nada (pasa kilos de un sitio a otro).

Se hace al guardar, sumando por artículo los movimientos nuevos de partidas. Solo cuentan los artículos por kilos con
control de stock. Si una salida no cabe en el almacén, queda como aviso y no deshace lo agro.

## Cuaderno de campo (tratamientos fitosanitarios)

Registro de los tratamientos de cada parcela, como pide el RD 1311/2012 y GlobalG.A.P. (`agro.tratamiento_parcela`):
fecha, producto y su **número de registro**, materia activa, plaga o motivo, dosis y unidad, superficie tratada (no más
que la de la parcela), **plazo de seguridad** en días, aplicador y observaciones.

- **Plazo de seguridad:** una recepción no se confirma si alguna línea se recolectó (fecha de recolección, o la de la
  recepción) desde el día del tratamiento hasta antes de que pase el plazo (`recepcion.plazo_seguridad`, con el producto
  y el primer día en que se podía recolectar).
- Un tratamiento no se borra: se anula con el motivo (`POST /agro/tratamientos/{id}/anular`) y deja de contar.
- **Cuaderno del agricultor** (`GET /agro/agricultores/{id}/cuaderno?desde=&hasta=`): sus tratamientos y sus
  recolecciones (las entregas con parcela), con las que quedaron dentro de un plazo de seguridad.
- **Otras labores** (`tipo`): además del **fitosanitario**, el **abonado** con sus unidades fertilizantes (N, P₂O₅ y
  K₂O en kg/ha), el **riego** con su volumen en m³ y **otras** labores (poda, laboreo…). Solo el fitosanitario tiene
  plazo de seguridad. El cuaderno resume por parcela los tratamientos, las unidades fertilizantes aportadas y el agua.
- **Pantalla:** Agro → **Cuaderno de campo**. **API:** `GET/POST /agro/tratamientos` (filtros por agricultor,
  parcela y fechas).

## Registro Oficial de Productos Fitosanitarios

`agro.fitosanitario`, con sus tablas `fitosanitario_materia_activa` y `fitosanitario_uso`, guarda cada producto:

- número de registro, nombre, titular y formulado;
- situación (autorizado, suspendido, caducado o cancelado) y fechas de caducidad, cancelación, límite de venta y límite
  de uso;
- materias activas con su concentración;
- **usos autorizados**: cultivo, plaga o agente, dosis mínima y máxima con su unidad, plazo de seguridad y número
  máximo de aplicaciones. El intervalo entre aplicaciones y el condicionamiento van en las observaciones.

Se puede enlazar con el **artículo del almacén** con que se compra y se aplica.

### Carga semanal del fichero del ministerio

`POST /agro/fitosanitarios/importar-mapa?completa=true` recibe **tal cual** el JSON del MAPA
(`{"Productos":[{"DATOSPRODUCTO":…,"COMPOSICION":…,"USOS":…}]}`). También se puede subir en Agro → Fitosanitarios →
«Cargar registro».

Cómo se lee cada campo:

- **Estado:** «Vigente» es autorizado; «Cancelado», «Caducado» y «Suspendido», su situación. Un estado desconocido se
  toma como suspendido y se avisa en el detalle.
- **Fechas:** en «aaaa/mm/dd» o ISO.
- **Plazo de seguridad:** «NO PROCEDE» es sin plazo; si trae un número, esos días.
- **Aplicaciones:** «1-8» son 8 como máximo.
- **Dosis:** mínima y máxima en su unidad (%, l/ha, kg/ha…).

Cada carga compara producto a producto con la anterior y anota cada cambio en `agro.cambio_fitosanitario`:

- situación;
- fechas;
- materias activas que entran o salen;
- usos nuevos, retirados o modificados (dosis, plazo, aplicaciones);
- nombre o titular.

En una **carga completa** (por defecto), el producto autorizado que ya no viene pasa a cancelado. Una carga igual a la
anterior no cambia nada.

Con 3.000 productos y 75.000 usos (22 MB), la primera carga tarda unos 20 segundos y las semanales, unos 5.

Para altas o cambios a mano: `GET/POST /agro/fitosanitarios`, `GET/PUT/DELETE /agro/fitosanitarios/{id}`. Un producto
usado en tratamientos no se elimina.

### Avisos

`GET /agro/fitosanitarios/avisos` (pantalla Agro → Fitosanitarios) lista los cambios pendientes de revisar. Los que
**restringen** (retirada, fechas, materias activas, usos retirados o modificados) dicen a quién afectan:

- existencias del artículo enlazado en el almacén;
- tratamientos del último año con ese producto (parcela, agricultor, cultivo y plaga).

`POST /agro/fitosanitarios/avisos/{id}/revisado` los quita de pendientes.

### Validación del tratamiento

Un tratamiento con `fitosanitarioId` (el producto del registro) se comprueba contra el registro:

- **Autorizado ese día:** autorizado y sin caducar ni cancelar, o dentro de su límite de uso. Si no,
  `tratamiento.no_autorizado`.
- **Uso autorizado:** cultivo (`cultivo`) y plaga (`motivo`) de uno de sus usos. Si no, `tratamiento.uso_no_autorizado`,
  con la lista de usos.
- **Dosis:** dentro del rango del uso cuando la unidad coincide. Si no, `tratamiento.dosis`.
- **Aplicaciones:** que no pasen del máximo del uso en el año y la parcela. Si no, `tratamiento.aplicaciones`.
- **Plazo de seguridad:** el del uso, si es mayor que el indicado. Con él se bloquean las entregas recolectadas antes.

El número de registro y la materia activa se toman del registro.

### Consumo del almacén por lote

Con `almacenId`, `lote` y `cantidadConsumida`, el tratamiento **descuenta** del almacén ese lote del artículo. El
artículo es el indicado en `articuloId` o, si no, el del registro.

- Si el lote está caducado el día del tratamiento, no se registra (`tratamiento.lote_caducado`).
- Si no hay existencias suficientes, tampoco.
- Al anular el tratamiento, lo consumido vuelve a su lote.

### Trazabilidad del lote

- `GET /agro/fitosanitarios/trazabilidad?articuloId=&lote=` va del lote a los tratamientos (parcela y agricultor), de
  ahí a las **partidas recolectadas en esas parcelas** desde el tratamiento hasta un año después, y de ahí a sus
  **palés expedidos y clientes**.
- Al revés, `GET /agro/fitosanitarios/de-partida?partidaId=` (o `sscc=`) da los tratamientos, con su lote, que
  recibieron las parcelas de origen de una partida o un palé en el año anterior a su recolección.

### Pendiente

- La fecha límite de uso no viene en el fichero. Un producto cancelado solo se puede seguir aplicando si se le pone
  esa fecha a mano.
- Las otras denominaciones (`OTRASDENOMINACIONES`, `OTROSNOMBRES`) no se guardan.
- Los usos de un mismo cultivo y agente que se distinguen solo por el tipo de usuario (profesional o no profesional)
  se guardan una vez.
- Cruzar el cultivo del uso con el artículo de la parcela: hoy el uso se elige en el tratamiento.

## Autoevaluaciones y auditorías internas (GlobalG.A.P.)

- **Listas de control** (`agro.lista_control` y `agro.punto_control`): la de la norma en su versión (por ejemplo, IFA
  v6) o una propia. Cada punto lleva su código, su texto y su nivel: **obligación mayor**, **menor** o
  **recomendación**. En la pantalla se pegan los puntos, uno por línea, con el formato `código;nivel;texto`. Una lista
  con evaluaciones no se borra: queda de baja y ya no abre otras.
- **Evaluación** (`agro.autoevaluacion` y `agro.respuesta_autoevaluacion`): es una **autoevaluación** o una
  **auditoría interna**, del productor (un agricultor) o de la empresa, con su fecha y su auditor. Al abrirse copia los
  puntos de la lista, así que cambiar la lista no altera las evaluaciones ya abiertas.
- **Respuestas:** cada punto se marca como «cumple», «no cumple» o «no aplica». Para cerrar la evaluación:
  - todos los puntos tienen que estar respondidos;
  - un «no aplica» necesita su justificación;
  - un «no cumple» de una obligación necesita su acción correctiva (y, opcionalmente, una fecha límite).
- **Cierre:** cerrada, la evaluación no se modifica ni se borra. Un trigger lo impide también en la base de datos.
- **Resultado:** la evaluación se supera con el 100 % de las obligaciones mayores y al menos el 95 % de las menores,
  contando solo los puntos que aplican.
- **Borrado del agricultor:** un agricultor con evaluaciones no se borra.
- **Pantalla:** Agro → **GlobalG.A.P.**
- **API:**
  - `GET/POST /agro/listas-control`, `PUT/DELETE /agro/listas-control/{id}`
  - `GET/POST /agro/autoevaluaciones?agricultorId=&anio=`
  - `GET/PUT/DELETE /agro/autoevaluaciones/{id}`
  - `POST /agro/autoevaluaciones/{id}/cerrar`

## Básculas

El **agente de báscula** (`src/AlxorCore.AgenteBascula`, ver su README) es un servicio local del puesto de la báscula:
lee el indicador por TCP o por el puerto serie (Dini Argeo, A&D, Gram, Mettler Toledo SICS y tramas genéricas) y da el
último peso en `http://localhost:5199/peso`. En la pesada de una recepción, **Leer báscula → bruto / tara** lo toma: no
acepta un peso inestable ni una lectura de más de 10 s, y pone el nombre de la báscula. La dirección del agente se
guarda por navegador («Agente de báscula…»).

## Escáner de cajas en el punto de paletizado

`POST /agro/pales/{id}/lecturas` con el `Codigo` leído añade la caja al palé, como si se indicara a mano (con la
plantilla del palé, sus kilos por caja). El lote de la etiqueta es el código de la partida:

- GS1-128 legible, `(01)GTIN(10)LOTE…`, o en bruto (`01` + GTIN, fechas `11`/`13`/`15`/`17` opcionales, `10` + lote
  hasta el separador FNC1), con o sin el prefijo de simbología `]C1`;
- o, si no es GS1, el propio código de la partida.

Si no hay ninguna partida con existencias con ese lote, falla con `lectura.partida`. En pantalla, la ventana de cajas
del palé tiene un campo para el lector: cada lectura añade una caja y, al completar el palé, se cierra.

## Vuelta de palés expedidos

Al anular la expedición de un palé que salió con un albarán del pedido:

- si ya han vuelto **todos** los palés del albarán, se anula el albarán (lo servido vuelve a quedar pendiente en el
  pedido) y las devoluciones que dejaron las vueltas parciales;
- si vuelven **solo algunos**, se registra una **devolución de venta** del albarán con lo que trae el palé (kilos o cajas
  según la unidad del artículo, repartidos entre sus líneas), con reingreso en el almacén. Así el albarán queda
  corregido: si no estaba facturado, su factura sale sin lo devuelto; si lo estaba, la devolución se abona con una
  rectificativa desde Ventas → Devoluciones.

## Impresos y EDI

- **PDF de la liquidación al agricultor:** `GET /agro/liquidaciones/{id}/pdf`. Es la hoja que recibe el agricultor, con las entregas, los descuentos, el impuesto o la compensación REAGP, la retención y el líquido.
- **DESADV de un albarán expedido desde palés:** lleva el SSCC de cada palé y su contenido por GTIN. Ver [integraciones](integraciones.md#edi-con-la-gran-distribución-eancom-d96a).

## 11. Pendiente

No queda nada pendiente de lo analizado en Hispatec para este módulo.
