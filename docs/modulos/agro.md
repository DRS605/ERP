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
| Descuentos | Por kilo, porcentaje del bruto o fijo. |
| Base | Bruto − descuentos (nunca negativa). |
| Impuesto | Compensación REAGP (12 %) o IVA (4 %) sobre la base. |
| Retención | Porcentaje de IRPF sobre la base. |
| Total factura | Base + impuesto. |
| A pagar | Total − retención. |

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

**Anular** una liquidación emitida anula su autofactura con un **contraasiento**, siempre que la autofactura
no esté pagada. Sus entregas quedan libres para otra liquidación.

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

## 11. Pendiente

- **Otros módulos:**
  - reflejar las partidas en Inventario, para una valoración única de existencias;
  - albarán de venta (del pedido) con los palés expedidos, y su factura. Hoy la expedición emite la carta de
    porte; el albarán de venta necesita el pedido y todavía no se puede anular, por eso no se genera solo.
- **Liquidaciones:**
  - anticipos a cuenta;
  - liquidaciones masivas de todos los agricultores del periodo;
  - REAGP del IGIC en Canarias (hoy se indica a mano el impuesto de la autofactura).
- **Operativa:**
  - lectura de cajas con escáner en el punto de paletizado (hoy se indican las cajas en pantalla);
  - lectura directa de básculas;
  - certificaciones (GlobalG.A.P.) y cuaderno de campo.
