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

## Cobros — Cuaderno 19 clásico (CSB, texto de 162 posiciones)

`GenerarCuaderno19` produce el fichero heredado de ancho fijo (registros 5180 presentador, 5170
ordenante, 5670 adeudos, 5870/5980 totales). `POST /tesoreria/cuaderno19` con `{ facturaIds }`.

## Pagos — Confirming (Cuaderno 68, texto de 100 posiciones)

`GenerarConfirming` produce el fichero de confirming (cabecera 01, detalle 02 por proveedor/factura,
totales 09) para que el banco gestione el pago a proveedores. `POST /tesoreria/confirming` con
`{ gastoIds }`. No exige IBAN del proveedor (el banco gestiona la adhesión), pero lo incluye si está.

> **Aviso**: el Cuaderno 19 clásico y el Confirming (C68) son formatos heredados de ancho fijo cuyos
> offsets pueden variar por entidad. Están implementados como **mejor esfuerzo** con la estructura
> estándar AEB y **deben validarse contra el servicio de pruebas del banco** antes de producción
> (mismo criterio que los ficheros oficiales AEAT). Para la mayoría de bancos el formato vigente es el
> **SEPA XML** (pain.008 / pain.001), que sí se genera con precisión.

El helper `RegistroCsb` construye registros de ancho fijo (alfa a la izquierda, numérico a la
derecha; importes en céntimos; mayúsculas sin acentos conservando la Ñ).

## Tests

- **Integración**: la remesa de transferencias genera un `pain.001` con los proveedores con IBAN y
  omite los que no lo tienen; el Cuaderno 19 clásico produce registros de 162 posiciones (5180…5980);
  el Confirming produce cabecera 01 + detalle + totales 09 de 100 posiciones.
