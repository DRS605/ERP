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

## Alcance y siguiente paso

Este módulo es la **base multidivisa**: no altera la emisión de facturas ni el IVA/VeriFactu (que
permanecen en euros ante la AEAT). El paso siguiente —emitir facturas y registrar gastos **en divisa**
guardando el importe en divisa y su contravalor en euros (tipo de cambio congelado al emitir)— se
apoya en el puerto `IConversorDivisa` y en el motor de diferencias de cambio ya disponibles aquí.

## Tests

- **Unitarios**: conversión a/desde euros; diferencias de cambio (activo que se aprecia → 768, que se
  deprecia → 668, y pasivo como inverso); validación de `TipoCambio` (código normalizado, EUR
  rechazado, divisa desconocida y tasa no positiva); catálogo.
- **Integración**: catálogo; alta/actualización de tipos de cambio y uso de la **tasa vigente** en la
  conversión según la fecha; el euro se convierte a sí mismo; conversión sin tasa da 404; reparto de
  diferencias en 768/668 (activo y pasivo).
