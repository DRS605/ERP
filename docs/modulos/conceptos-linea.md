# Conceptos de línea

Recargos, bonificaciones y costes que se ponen en las líneas de los documentos. Son los portes, rappels, comisiones,
envases, pronto pago o aranceles de cada operación. Un concepto tiene uno de dos **efectos**:

- **Cambia el importe**: sale en el documento y forma parte de su base imponible. Por ejemplo, portes cobrados al
  cliente, rappel o recargo por envase.
- **Solo coste**: no toca el documento ni lo que se cobra o paga. Solo suma o resta al coste de la línea. Por
  ejemplo, la comisión del comercial, los portes pagados a un transportista o los aranceles.
  - En una **venta** baja el margen de la línea (factura, informe de beneficio y ventas por artículo).
  - En una **compra** sube el coste con que entra la mercancía en el almacén (PMP, última compra, FIFO).

Pantalla: **Artículos y almacén → Conceptos de línea** (maestro e informe del periodo).

## El concepto

El maestro se comparte en el grupo, como los artículos y las tarifas (tabla `catalogo.concepto_linea`).

| Campo | Qué es |
|---|---|
| Código · nombre · texto en el documento | El texto es lo que se imprime (si está vacío, el nombre) |
| Se usa en | Ventas, compras o ambos |
| Efecto | Cambia el importe o solo coste |
| Sentido | Suma o resta |
| Cálculo | % sobre la línea (tras el descuento), € por unidad, € por kilo neto, € por **bulto**, € por **palé** o importe fijo |
| Valor | El valor por defecto (nunca negativo: el signo lo pone el sentido; un porcentaje no pasa de 100) |
| Reparto | Cómo se reparte un importe fijo puesto al documento entero: por importe, por cantidad o por peso |
| Orden | Orden de aplicación en la línea: los de orden menor van antes |
| Base del porcentaje | La línea (tras el descuento) o **cascada**: la línea más los conceptos de importe que van antes en el orden (la «base importe calculado» de Hispatec) |
| Acreedor | Proveedor a quien se debe el concepto (transportista, comisionista…). Sus cargos se liquidan con su factura (ver abajo) |
| Cuenta contable | Cuenta propia del concepto en el asiento de la factura (por ejemplo, 7590 para los portes cobrados). Si está vacía, va a la cuenta de la línea |
| Reglas | Cuándo se pone solo (ver abajo) |

Los kilos netos de una línea son:

- la cantidad, si el artículo se vende por kilos;
- el peso del artículo por la cantidad, si no;
- nada, si la línea no tiene artículo.

Un concepto por kilo no se pone solo en una línea sin peso.

Los **bultos** de una línea son los que se indican (`Bultos` en la línea del albarán directo); si no, la cantidad entre el
factor de la unidad de venta del artículo (100 kg en cajas de 10 kg son 10 bultos) o, si no se vende por kilos, la
cantidad. Los **palés** son los que se indican (`Pales`). Un concepto por bulto o por palé no se pone solo si no se
conocen. El concepto guarda las unidades usadas (`unidades`) y no cambia al revalorar la línea.

- **Al entregar un pedido**, los conceptos por bulto y palé van por los bultos y palés que se indiquen en la entrega
  (`Bultos`, `Pales`), o en proporción a lo entregado.
- **Al expedir palés agro contra un pedido**, van por las cajas y los palés reales expedidos de cada artículo.

### Reglas: cuándo se pone solo

Cada regla (tabla `catalogo.asignacion_concepto`) dice:

- **para quién** vale: un **cliente o proveedor**, un **tipo de tercero** (el campo «Tipo» de su ficha, por ejemplo
  «Mayorista») o cualquiera;
- **para qué** vale: un **artículo**, una **familia** (con sus subfamilias) o todos;
- y, si se indica, **entre qué fechas** (vigencia desde y hasta, por la fecha del documento).

Opcionalmente, lleva un **valor propio** (otra comisión para un cliente) y un **acreedor propio** (otro transportista
para una ruta).

Si varias reglas encajan, manda la más específica con la jerarquía de Hispatec. **Primero cuenta el tercero:** el
cliente o proveedor, luego su tipo y luego cualquiera. Dentro de cada nivel, el artículo, luego la familia más cercana y
luego todos. A igualdad, gana la regla con vigencia acotada. Por ejemplo, «cliente X, todos los artículos» gana a
«cualquier cliente, artículo Y».

> Cambio respecto a la primera versión, que daba prioridad al artículo sobre el tercero. Así se pueden migrar tal
> cual los valores por defecto de Hispatec (`CAVentaLDefecto`, `CACompraLDefecto`).

Un concepto sin reglas solo se pone a mano.

### Reglas por envase

Una regla puede llevar un **envase** (un artículo, como la caja IFCO o el palé): entonces solo encaja en las líneas de
venta con ese envase (recargo por caja retornable, fianza del palé…). A igualdad de tercero y artículo, la regla del
envase gana a la general, y después cuenta la vigencia.

- Las líneas de presupuesto, pedido de venta, albarán de venta y factura llevan `envaseProductoId`. Al pasar de un
  documento a otro (presupuesto → pedido → albarán → factura) el envase va con la línea.
- `GET /conceptos-linea/sugeridos?envaseProductoId=` da los que se pondrían en una línea con ese envase.
- Un envase que no existe da 400 `concepto.envase_no_encontrado`.
- En el editor de ventas, el envase se elige bajo la línea (±), entre los envases que usan las reglas.

## Después de la base: suplidos y fianzas

Con el efecto **Suplido** (Hispatec: aplicación después de la base imponible) el concepto no forma parte de la base ni
paga impuesto: es un gasto que se repercute al cliente tal cual (un suplido, la fianza de los envases).

- Suma y lleva su **cuenta contable** (por ejemplo, 4709 o 5550 en ventas; 631 o 4709 en compras); si no, falla con
  `concepto.suplido`.
- En la factura: `suplidos` = suma de los suplidos de las líneas, y **total = base + impuesto + recargo − retención +
  suplidos**. La base de datos lo comprueba (`ck_factura_total`, `ck_linea_factura_suplidos` y el cuadre con las líneas).
- En el asiento: el cliente al debe por el total; el suplido, al haber de su cuenta, sin IVA.
- En el presupuesto, `suplidos` y el total los suman igual que en la factura. En el pedido de venta, cuyo total es la
  base, van aparte en `suplidos`.
- En el PDF salen después de los impuestos; en Facturae, como `TotalReimbursableExpenses`.
- El libro de IVA, el SII y los modelos no los incluyen en la base.

### Suplidos en compras

Un suplido de compras (tasas portuarias o aranceles que paga el transitario por la empresa) va en el pedido de compra
aparte de su total (`suplidos`). Al facturar el pedido, la factura del proveedor lleva una **línea de suplido** por
cuenta: sin impuesto, fuera de la base y a su cuenta.

- **Gasto:** `total = base + impuesto + recargo − retención + suplidos`. La línea lleva `suplido: true` y su cuenta es
  obligatoria (400 `gasto.suplido`). La base de datos lo comprueba (`ck_linea_gasto_suplido`).
- **Asiento:** el suplido va al debe de su cuenta y el proveedor al haber por el total.
- El libro de IVA, el SII (`ImporteTotal`) y el 347 no lo cuentan.
- Una factura recibida registrada a mano también admite líneas de suplido (en el editor, la casilla «Suplido»).

## En los documentos

Se aplican en:

- presupuestos, pedidos de venta, **albaranes de venta**, facturas y **tickets** (los automáticos del cliente o, en uno
  de contado, los de cualquiera);
- **rectificativas** (ver abajo), **facturas periódicas** (con sus propias reglas) y **facturas del buzón**;
- pedidos de compra.

**Rectificativa.** Es por sustitución: lleva las líneas corregidas. Una línea con conceptos pedidos lleva exactamente
esos. Si no, la del mismo artículo (o, sin artículo, la misma descripción) en la factura original le pasa los suyos,
recalculados sobre la línea corregida como al pasar un documento a otro. Los cargos con acreedor pierden el acreedor:
el servicio ya se prestó y su cargo es el de la original.

**Factura periódica.** Cada línea de la plantilla puede llevar sus conceptos (`conceptos`) y la plantilla los del
documento (`conceptosDocumento`). Sin indicarlos, se ponen los automáticos al emitir; con una lista (también vacía),
exactamente esos. Se comprueban al guardar la plantilla.

**Factura del buzón.** Al validarla (`POST /recepcion/facturas/{id}/validar` con `conceptos`) se eligen los cargos y
abonos de compras que trae; no se ponen automáticos, porque el importe de la factura ya es el que cobra el proveedor.
Al contabilizarla, los de importe con cuenta van a su propia línea del gasto (los demás suman a la base) y los suplidos
van fuera de la base. Los de solo coste no aplican.

**Divisa.** En un documento en divisa los conceptos van en la divisa: ver *Cargos y abonos en divisa* en
[divisas.md](divisas.md).

En los albaranes de venta:

- **Albarán de un pedido:** copia los conceptos de la línea del pedido para la parte entregada. Los porcentajes y los
  importes por unidad se recalculan; el resto se prorratea.
- **Albarán directo:** lleva los automáticos o los pedidos, igual que la factura.
- **Valoración:** al valorar el albarán, los porcentajes se recalculan con el precio definitivo.
- **Facturación:** la factura que recoge el albarán copia sus conceptos.

En cada línea los conceptos se aplican **en su orden**. Un porcentaje en cascada se calcula sobre la línea más los
conceptos de importe que ya se han puesto.

En cada línea:

- **Sin indicar nada** se ponen los automáticos del cliente o proveedor y del artículo.
- **Con una lista** se ponen exactamente esos. Si un concepto no lleva valor, se usa el de su regla o el del concepto.
  Una lista vacía quita los automáticos.
- **Conceptos del documento**:
  - Uno de importe fijo se reparte entre las líneas, al céntimo y sin descuadre.
  - Los de porcentaje, por unidad o por kilo se aplican a cada línea.
  - Todos quedan marcados como repartidos.

Cada línea guarda una **copia** de sus conceptos: código, nombre, efecto, sentido, cálculo, valor e importe con signo.
Se guardan en la columna jsonb `conceptos`, junto con sus totales `importe_conceptos` y `coste_conceptos`. Cambiar o
dar de baja el concepto no altera los documentos hechos. Al pasar un documento a otro (presupuesto → pedido →
factura) se copian los conceptos y se recalculan sobre la nueva línea.

Importes:

- Base de la línea de venta = cantidad × precio − descuento + conceptos que cambian el importe. Ningún concepto puede
  dejar una línea en negativo (400 `concepto.linea_negativa`).
- Coste de la línea de venta = coste unitario × cantidad + conceptos de coste; margen = base − coste.
- Importe de la línea de compra = cantidad × precio + conceptos que cambian el importe. Es el total del pedido y lo que
  se contabiliza al facturarlo.
- Coste de entrada en almacén = (importe + conceptos de coste) / cantidad.

El PDF de la factura y del presupuesto imprime bajo cada línea los conceptos que cambian el importe. En **Facturae**
salen como cargos y descuentos de la línea, así que `GrossAmount = TotalCost − descuentos + cargos`. El Intrastat de
introducciones valora con el importe de la línea con sus conceptos.

### Garantías en la base de datos

- `ck_linea_factura_base`: la base de la línea de factura es cantidad × precio − descuento + `importe_conceptos`.
- `ck_<tabla>_conceptos` en `linea_factura`, `linea_presupuesto`, `linea_pedido_venta` y `compras.linea_pedido`: la
  columna es una lista, y `importe_conceptos` y `coste_conceptos` son la suma de sus conceptos por efecto (función
  `public.alxor_suma_conceptos`).
- RLS por grupo en el maestro. Otros checks validan el ámbito, el efecto, el sentido, el cálculo, el reparto y el
  valor (≥ 0 y ≤ 100 si es porcentaje).
- `catalogo.concepto_linea_en_uso(id)`: busca el concepto en las líneas de todas las empresas del grupo para decidir
  entre borrarlo y darlo de baja.

## Cargos con acreedor y su liquidación

Un concepto con acreedor, en el propio concepto o en su regla, es una **deuda con ese acreedor**, como los cargos con
«acreedor asociado» de Hispatec. Por ejemplo, 0,05 €/kg de portes al transportista o el 3 % de comisión al
comisionista. Suelen ser de efecto «solo coste», pero también puede llevarlo uno que se cobra al cliente.

- **Pantalla:** Compras → **Cargos de acreedores**. Muestra los pendientes por acreedor y fechas, sacados de:
  - las facturas de venta no anuladas que no vienen de albaranes (las que vienen de albaranes se cuentan en el
    albarán, para no contar el cargo dos veces);
  - los albaranes de venta no anulados;
  - los pedidos de compra no cancelados.
- **Liquidar:** se eligen los cargos (o todos los del acreedor en las fechas) y se registra su factura (número, fecha,
  IVA y retención; por ejemplo, el 1 % a un transportista en módulos). Queda como **factura de proveedor**, con una
  línea por concepto. Cada cargo queda anotado en `gastos.cargo_acreedor_liquidado`, que es de solo inserción y lleva
  RLS.
- **Anulación:** si se anula la factura del acreedor, sus cargos vuelven a estar pendientes.
- **API:**
  - `GET /gastos/cargos-acreedores?acreedorId=&desde=&hasta=&todos=`
  - `POST /gastos/cargos-acreedores/liquidar` con `{ acreedorId, numeroFactura, fechaFactura, desde, hasta, claves[],
    codigoIva, porcentajeIrpf }`
  - Errores: `acreedor.sin_cargos`, `acreedor.cargo_no_pendiente` y `acreedor.numero_factura`.

Lo que se paga al acreedor es el **importe del cargo** (a diferencia de Hispatec, no hay un segundo importe distinto
para el acreedor).

### Provisión del coste con acreedor

Un concepto de **solo coste con acreedor** (los portes al transportista, la comisión del comisionista) se contabiliza
al emitir la factura de venta, sin esperar a la factura del acreedor:

- **En la venta:** asiento aparte con el gasto al debe (la cuenta del concepto o, sin ella, la 629) y la **4009**
  «Proveedores, facturas pendientes de recibir» al haber; uno por acreedor y cuenta. El concepto queda marcado
  `provisionado`.
- **Al liquidar:** en la factura del acreedor, los cargos provisionados van a la 4009 (la cancelan); los demás, a la
  cuenta de gasto del concepto (si es del grupo 6) o a la de gastos. La pantalla de cargos los marca «provisionado».
- Un cargo de albarán liquidado antes de facturar el albarán ya cargó el gasto: la factura no lo provisiona otra vez.

## Cuenta propia en la contabilidad

Un concepto de importe con **cuenta contable** se contabiliza en ella en el asiento de la factura de venta. Por
ejemplo, con 100 € de mercancía y 20 € de portes a la 7590, el haber lleva 70x 100 € y 7590 20 €. El resto de la línea
va a su cuenta o a la de ventas.

En las **compras** pasa lo mismo: al facturar un pedido de compra, cada concepto de importe con cuenta propia sale en
su propia línea del gasto (con su cuenta de gasto; por ejemplo, 30 € de portes a la 624) y el resto va a la cuenta de
compras.

## Impuesto propio

Un concepto de importe (efecto precio, sentido suma) puede llevar un **código de IVA/IGIC propio**, distinto del de la
línea. Por ejemplo, fruta al 4 % con portes al 21 %. Al emitir la factura, el concepto sale de su línea y va a una línea
aparte con su tipo, su cuenta y el mismo albarán, sin mover existencias. Con 100 € de fruta al 4 % y 10 € de portes al
21 %, la base es 110 € y la cuota 6,10 €. Un concepto de otro efecto o de resta con impuesto propio se rechaza con
`concepto.impuesto_propio`.

## API

Todos los endpoints de escritura piden el permiso `producto.gestionar`.

- `GET /conceptos-linea?ambito=Ventas|Compras&activos=true`
- `GET /conceptos-linea/{id}`
- `POST /conceptos-linea` con `{ codigo, datos }`
- `PUT /conceptos-linea/{id}` con `{ datos, activo }`
- `DELETE /conceptos-linea/{id}`: borra el concepto o lo da de baja si ya se usó.
- `GET /conceptos-linea/sugeridos?ambito=&terceroId=&productoId=&fecha=`: los que se pondrían solos en una línea, con el
  tipo del tercero y la vigencia a esa fecha, y su acreedor.
- `GET /conceptos-linea/informe?desde=&hasta=`: importe de cada concepto en las facturas de venta (no anuladas) y en
  los pedidos de compra (no cancelados) del periodo, separando importe y coste, con el detalle por documento. También
  cuentan los albaranes de venta entregados y aún sin facturar (marcados `sinFacturar`, con su total aparte en
  `precioSinFacturar` y `costeSinFacturar`); los facturados cuentan en su factura.
- En las líneas de `POST/PUT /presupuestos`, `/pedidos-venta`, `/facturas` y `/compras/pedidos`:
  - `conceptos: [{ conceptoId, valor? }]` en cada línea;
  - `conceptosDocumento: [...]` en el documento.
- Las líneas de las respuestas traen `conceptos`, `importeConceptos` y `costeConceptos`. Las de compra traen además
  `costeUnitarioEntrada`.

Errores: `concepto.vigencia`, `concepto.cuenta`, `concepto.codigo`, `concepto.codigo_duplicado` (409), `concepto.valor`, `concepto.tipo`,
`concepto.asignacion_repetida`, `concepto.no_encontrado`, `concepto.ambito` (un concepto de compras en una venta o al
revés) y `concepto.linea_negativa`.

## Interfaz

- **Maestro**: lista, alta y edición con sus reglas, baja, e informe del periodo.
- **Editores de presupuesto, pedido de venta, factura y pedido de compra**:
  - Bajo cada línea, «Conceptos: los que se pongan solos · elegir». Al elegir se cargan los sugeridos para el
    cliente o proveedor y el artículo, que se pueden cambiar, quitar o completar.
  - Bajo las líneas, los conceptos del documento.
  - El total que se ve al editar es una estimación; el bueno lo calcula el servidor al guardar.
- **Detalle de la factura y del pedido de venta**: cada línea con sus conceptos (los de coste marcados) y su margen.

## Pendiente

- Rectificativas **por diferencias** (con importes negativos): hoy las rectificativas son por sustitución (ver
  facturación); cuando existan, sus conceptos irán en proporción a la diferencia.
- La provisión en las rectificativas y en la anulación de la factura de venta (hoy no generan contraasiento).
- Elegir los conceptos del documento de una factura periódica desde la pantalla (la API ya los admite).
