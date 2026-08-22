# Ficheros bancarios (remesas de cobro y pago)

Generación de ficheros para el banco desde Tesorería. Estado actual:

## Cobros a clientes — Adeudos SEPA (pain.008 / Norma 19.14)

`GenerarRemesaSepa` produce el fichero `pain.008.001.02` de adeudos directos con **esquema** y
**secuencia** elegibles:

- **Esquema**: `CORE` (deudores particulares) o `B2B` (deudores empresa).
- **Secuencia**: `OOFF` (único), `FRST` (primero), `RCUR` (recurrente), `FNAL` (último).

Requiere IBAN + identificador de acreedor de la empresa y, por cliente, IBAN + mandato + fecha de
mandato. Cobra el pendiente de cada factura; omite (informando) las no domiciliables.
`POST /tesoreria/remesa` con `{ facturaIds, fechaCobro?, esquema?, secuencia? }`.

## Pagos a proveedores — Transferencias SEPA (pain.001 / Cuaderno 34.14)

`GenerarTransferenciasSepa` produce el fichero `pain.001.001.03` de transferencias para pagar el
pendiente de los gastos indicados. Requiere IBAN de la empresa (ordenante) y de cada proveedor
(beneficiario); omite los gastos ya pagados o de proveedores sin IBAN.
`POST /tesoreria/transferencias` con `{ gastoIds, fechaPago? }`.

El proveedor incorpora un campo **IBAN** (`terceros.proveedor.iban`, migración `ProveedorIban`).

## UI

En **Ventas → Remesas SEPA**: un panel de adeudos (cobros, con selector de esquema/secuencia) y un
panel de transferencias (pagos a proveedores). El fichero se descarga para subirlo al banco.

## Tests

- **Integración**: la remesa de transferencias genera un `pain.001` con los proveedores con IBAN y
  omite los que no lo tienen; la remesa de adeudos admite esquema/secuencia (cubierto por las pruebas
  de remesa existentes).

## Pendiente (mismo bloque de trabajo)

- **Cuaderno 19 clásico (CSB, texto plano)** y **Confirming (Cuaderno 68)**: formatos heredados de
  ancho fijo que varían por banco. Se implementarán como *mejor esfuerzo* con la estructura estándar
  AEB y **deben validarse contra el servicio de pruebas del banco** antes de usarse en producción
  (mismo criterio que los ficheros oficiales AEAT).
