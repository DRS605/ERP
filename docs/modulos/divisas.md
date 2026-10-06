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
  divisa. En las facturas en euros sigue comprobando la de siempre.
- **Totales.** La factura guarda `baseDivisa`, `cuotaDivisa` y `totalDivisa`. Se calculan con el mismo redondeo línea a
  línea que en euros (impuesto, recargo y retención). `ck_factura_divisa` exige que vayan todos o ninguno.
- **Qué va en euros.** Los importes en euros son el contravalor. La contabilidad, el SII, los libros de IVA y
  VeriFactu siguen en euros, y la cuota que vale es la de euros.
- **Qué no admite.** Una factura en divisa no admite conceptos de línea, suplidos ni descuento de anticipos, porque se
  definen en euros (400 `factura.divisa_conceptos`).
- **Rectificativas y facturas desde albaranes o pedidos.** Se emiten en euros.
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

## Tests

- **Unitarios**: conversión a/desde euros; diferencias de cambio (activo que se aprecia → 768, que se
  deprecia → 668, y pasivo como inverso); validación de `TipoCambio` (código normalizado, EUR
  rechazado, divisa desconocida y tasa no positiva); catálogo.
- **Integración**: catálogo; alta/actualización de tipos de cambio y uso de la **tasa vigente** en la
  conversión según la fecha; el euro se convierte a sí mismo; conversión sin tasa da 404; reparto de
  diferencias en 768/668 (activo y pasivo).
