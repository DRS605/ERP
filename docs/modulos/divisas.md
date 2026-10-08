# Módulo Divisas (multidivisa)

Soporte de **multidivisa** para empresas que operan fuera de la zona euro. La **moneda funcional**
del ERP es el **euro (EUR)**: la contabilidad, el IVA y VeriFactu van siempre en euros. Este módulo
aporta la base para trabajar con divisas extranjeras: catálogo de divisas, **tipos de cambio**,
**conversión** a euros y cálculo de **diferencias de cambio**.

## Catálogo de divisas

`DivisasConocidas` es un catálogo estático de las divisas admitidas (ISO 4217: EUR, USD, GBP, CHF,
JPY, CNY, CAD, MXN, BRL, ARS, SEK, NOK, DKK, PLN, MAD…), cada una con su nombre y número de decimales
habituales. `GET /divisas` lo expone. El euro es la moneda funcional y no admite tipo de cambio.

## Tipos de cambio

`TipoCambio` { Divisa (ISO, nunca EUR), Fecha, **TasaEur** } por empresa (RLS). La tasa es **cuántos
euros vale una unidad** de la divisa (p. ej. USD con tasa `0,92` ⇒ 1 USD = 0,92 €). La **tasa vigente**
a una fecha es la del registro más reciente con fecha ≤ la buscada. Registrar la misma divisa+fecha
actualiza la tasa (no duplica; hay índice único `empresa+divisa+fecha`). Validaciones: divisa del
catálogo, distinta de EUR, tasa > 0.

## Conversión

`GET /divisas/convertir?divisa=&importe=&fecha=` convierte un importe en divisa a euros con la tasa
vigente a la fecha (`importe × tasa`, redondeo a 2 decimales). El euro se convierte a sí mismo (tasa
1). Si no hay tipo de cambio para esa divisa y fecha, devuelve **404**. El puerto transversal
`IConversorDivisa` (en el núcleo) permite que otros módulos (Facturación, Gastos…) adopten la divisa
en el futuro sin acoplarse a la persistencia de Divisas.

## Diferencias de cambio

`CalculadoraDivisa.Diferencia` calcula la diferencia de cambio en euros de una posición en divisa
entre dos tasas. Para un **activo** (saldo a cobrar) el resultado es `valorNuevo − valorOrigen`; para
un **pasivo** (a pagar) es el inverso (si la divisa se aprecia, se debe más y hay pérdida). Un
resultado **positivo** es un ingreso financiero (cuenta **768**) y uno **negativo**, un gasto (**668**).

`POST /divisas/diferencias-cambio` recibe la fecha de valoración y una lista de posiciones abiertas
(referencia, divisa, importe en divisa, tasa de origen, si es activo o pasivo), busca la tasa vigente
a la fecha de valoración y devuelve, por posición, la diferencia y su cuenta (668/768), con el total
de ingresos, gastos y neto. Es la base de la **revalorización de saldos en divisa** al cierre del
ejercicio.

## API

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/divisas` | JWT + empresa | Catálogo de divisas admitidas. |
| `GET` | `/tipos-cambio` | JWT + empresa | Lista de tipos de cambio de la empresa. |
| `POST` | `/tipos-cambio` | permiso `empresa.ajustes` | Registra o actualiza un tipo de cambio. **201** |
| `GET` | `/divisas/convertir` | JWT + empresa | Convierte un importe en divisa a euros a una fecha. |
| `POST` | `/divisas/diferencias-cambio` | permiso `informe.leer` | Diferencias de cambio de posiciones abiertas. |

## Persistencia

Esquema **`divisas`**, tabla `tipo_cambio` (RLS por empresa; índice único `empresa+divisa+fecha`).

## Facturas y gastos en divisa

Las facturas de venta y las facturas de proveedor (gastos) pueden ir en una divisa (`moneda`, ISO 4217, distinta de
EUR). El tipo de cambio se congela en el documento: es el indicado en `tasaCambio` (euros por 1 unidad) o, si no se
indica, el vigente a la fecha de emisión o de la factura del proveedor. Sin tipo de cambio, el alta da 400
`factura.sin_tipo_cambio` o `gasto.sin_tipo_cambio`.

### Factura de venta en divisa

- **Precios.** Los precios de las líneas se escriben en la divisa. Si una línea no lleva precio, se toma el de la
  tarifa o el del artículo (que está en euros) y se convierte a la divisa al tipo de cambio.
- **Línea.** La línea guarda su precio y su base en la divisa (`precioDivisa`, `baseDivisa`). La base en euros es
  `round(baseDivisa × tasa, 2)`. El precio en euros es orientativo.
- **Restricción en la base de datos.** `ck_linea_factura_base` comprueba la base en divisa a partir del precio en
  divisa y de los conceptos en la divisa (`public.alxor_suma_conceptos_divisa`). En las facturas en euros sigue
  comprobando la de siempre.
- **Totales.** La factura guarda `baseDivisa`, `cuotaDivisa` y `totalDivisa`. Se calculan con el mismo redondeo línea a
  línea que en euros (impuesto, recargo y retención). `ck_factura_divisa` exige que vayan todos o ninguno.
- **Qué va en euros.** Los importes en euros son el contravalor. La contabilidad, el SII, los libros de IVA y
  VeriFactu siguen en euros, y la cuota que vale es la de euros.
- **Qué no admite.** Una factura en divisa no descuenta anticipos, porque se facturaron en euros (400
  `factura.divisa_anticipos`).
- **Rectificativas y facturas desde albaranes o pedidos.** Conservan la divisa (ver más abajo).
- **PDF.** Las líneas y los totales salen en la divisa. Debajo va el contravalor en euros: base, impuesto y total, con
  el tipo de cambio.

### Gasto en divisa

Las bases de las líneas se escriben en la divisa y se pasan a euros al tipo de cambio. El `totalDivisa` se calcula con
las reglas del gasto: sin cuota en las autoliquidadas ni en las exentas, más el recargo y menos la retención. La
restricción es `ck_gasto_divisa`.

### Cobros y pagos en divisa

`POST /cobros` y `POST /pagos` admiten `importeDivisa`, que es lo cobrado o pagado en la divisa. El campo `importe`
son entonces los euros que entran o salen del banco.

- **Lo que se liquida.** Del documento se liquida el importe en divisa al tipo de cambio de la factura. Si salda todo
  lo pendiente en divisa, se liquida exactamente lo pendiente en euros, para que no queden céntimos.
- **Diferencia de cambio.** La diferencia entre los euros del banco y lo liquidado se guarda en el movimiento
  (`diferenciaCambio`) y tiene su propio asiento:
  - cobro con más euros, o pago con menos euros: 572 / 768, diferencia positiva;
  - cobro con menos euros, o pago con más euros: 668 / 572, diferencia negativa.
- **Saldo del banco.** Recoge los euros reales: lo liquidado más la diferencia.
- **Sin `importeDivisa`.** Los euros liquidan lo mismo, sin diferencia, y su equivalente en divisa se calcula al tipo
  de la factura.
- **Errores.** Un importe en divisa sobre un documento en euros da 400 `movimiento.no_divisa`. Pagar más de lo
  pendiente en divisa da 409 `movimiento.sobrepago`.
- **Anulación.** Anular el cobro o el pago anula también su diferencia de cambio.

Las cuentas 668 (Diferencias negativas de cambio) y 768 (Diferencias positivas de cambio) están en el plan básico.

**En la interfaz:**
- El editor de facturas y el de facturas de proveedor tienen el desplegable «Moneda» y el campo del tipo de cambio.
  Muestran el total en la divisa y el contravalor.
- Los diálogos de cobro y pago de un documento en divisa piden el importe en la divisa y los euros del banco.
- El editor de presupuestos y pedidos tiene el desplegable «Moneda». La rectificativa muestra la divisa de la original,
  sin poder cambiarla. El alta de albarán de la SPA también tiene el desplegable, y el listado muestra la divisa.

### Presupuestos, pedidos y albaranes en divisa

El presupuesto, el pedido de venta y el albarán de venta llevan `moneda` (vacío o `EUR`: euros). Sus precios e
importes están en esa divisa. El tipo de cambio no se guarda en ellos: se fija **al facturar**.

- **Precios.** Cada línea lleva su precio en la divisa, porque la tarifa y el precio del artículo están en euros. Una
  línea sin precio da 400 `documento.divisa_precio`. En el albarán se admite la línea «por fijar»: se valora después,
  también en la divisa.
- **Cargos y abonos.** Van en la divisa del documento (ver «Cargos y abonos en divisa»).
- **Código.** Un código que no sea de tres letras da 400 `documento.moneda`.
- **De un documento a otro.**
  - El pedido creado desde un presupuesto hereda su divisa, y los albaranes de entrega, la del pedido.
  - Aceptar el presupuesto, facturar el pedido o facturar los albaranes emite la factura en la misma divisa, al tipo
    del día de la emisión (módulo Divisas).
  - `POST /pedidos-venta/{id}/facturar` y `POST /albaranes-venta/facturar` admiten `tasaCambio` para indicarlo a mano.
- **Varios albaranes.** No se facturan juntos albaranes en divisas distintas (409 `albaranventa.monedas_distintas`).
  La facturación masiva los agrupa por cliente, centro y divisa.
- **PDF.** Los importes llevan el código de la divisa en lugar de «€».

### Cargos y abonos en divisa

En un documento en divisa (presupuesto, pedido, albarán o factura), los conceptos de línea van en la divisa, como el
resto de sus importes.

- **Valores.** Los porcentajes no cambian. Los valores en euros del maestro y de las reglas (por unidad, por kilo, por
  bulto, por palé o importe fijo) se pasan a la divisa al tipo de cambio: en la factura, el suyo; en el presupuesto, el
  pedido y el albarán, el vigente en su fecha. Sin tipo de cambio da 400 `concepto.sin_tipo_cambio`, salvo que el valor
  se escriba en el documento, que ya va en la divisa. `GET /conceptos-linea/sugeridos?moneda=&tasaCambio=` devuelve
  los valores ya en la divisa.
- **Factura.** Cada concepto guarda su importe en la divisa (`importeDivisa`) y su contravalor en euros (`importe`, al
  tipo de la factura). La base de la línea en divisa suma los conceptos en la divisa; la base en euros, el contravalor
  de la mercancía más los conceptos en euros. Los suplidos en divisa suman al total en divisa.
- **Lo que va en euros.** La contabilidad (cuentas propias y suplidos), el informe de conceptos y los cargos de
  acreedores usan el importe en euros. En un albarán en divisa sin facturar, el informe y los cargos lo pasan a euros
  al tipo del día del albarán.
- **PDF y pantallas.** Los conceptos salen en la divisa.

### Rectificativa de una factura en divisa

La rectificativa va en la misma divisa y al **tipo de cambio de la original**. Sus precios se escriben en la divisa y
cada línea lleva el suyo (400 `documento.divisa_precio`).

## Revalorización al cierre

Lo que queda pendiente de cobro o de pago en divisa al cierre del ejercicio se valora al tipo de cambio del 31/12 (NRV
11.ª del PGC: partidas monetarias al tipo de cierre). Entran:

- las facturas de venta en divisa emitidas hasta el 31/12 que no estén anuladas;
- los gastos (facturas de proveedor) en divisa que no estén anulados.

Para cada documento se calcula, con los cobros o pagos hasta el 31/12:

- **Pendiente en divisa:** total en divisa − lo cobrado o pagado en divisa.
- **Valor en libros:** total en euros − lo liquidado en euros.
- **Valor al cierre:** pendiente en divisa × tipo vigente a 31/12.
- **Diferencia:** valor al cierre − valor en libros. Un cliente que vale más, o un proveedor que vale menos, es
  ganancia (768); al revés, pérdida (668).

Asientos, con origen `RevalorizacionDivisa`, uno por documento:

| Caso | 31/12 | 1/1 del año siguiente |
|---|---|---|
| Cliente, ganancia | 430 / 768 | 768 / 430 |
| Cliente, pérdida | 668 / 430 | 430 / 668 |
| Proveedor, ganancia | 400 / 768 | 768 / 400 |
| Proveedor, pérdida | 668 / 400 | 400 / 668 |

La reversión del 1/1 devuelve el saldo al valor en libros. Así, el cobro o el pago del año siguiente calcula su
diferencia de cambio sobre el valor original, como siempre.

- **Simular** (`simular: true`) calcula sin guardar ni contabilizar nada.
- **Una por ejercicio.** Repetirla da 409 `revalorizacion.hecha`. Para rehacerla se anula antes.
- **Anular** deshace los asientos del cierre y de la reversión. Anular dos veces da 409 `revalorizacion.anulada`.
- **Sin tipo de cambio** de una divisa a 31/12 (ni anterior) da 400 `revalorizacion.sin_tipo_cambio`.

**API:**
- `GET /divisas/revalorizaciones` (permiso `contabilidad.leer`).
- `POST /divisas/revalorizaciones` `{ ejercicio, simular }`: 200 al simular, 201 al registrar (permiso
  `contabilidad.gestionar`).
- `POST /divisas/revalorizaciones/{id}/anular`.

**Persistencia:** `tesoreria.revalorizacion_divisa` (RLS por empresa, una vigente por ejercicio) y sus líneas en
`tesoreria.linea_revalorizacion_divisa`, que comparten su RLS.

**En la interfaz:** la pantalla **Divisas** tiene la pestaña «Revalorización al cierre», con Simular, Registrar
asientos, la lista de las hechas y Anular.

## Tests

- **Unitarios**: conversión a/desde euros; diferencias de cambio (activo que se aprecia → 768, que se
  deprecia → 668, y pasivo como inverso); validación de `TipoCambio` (código normalizado, EUR
  rechazado, divisa desconocida y tasa no positiva); catálogo.
- **Integración**: catálogo; alta/actualización de tipos de cambio y uso de la **tasa vigente** en la
  conversión según la fecha; el euro se convierte a sí mismo; conversión sin tasa da 404; reparto de
  diferencias en 768/668 (activo y pasivo).
