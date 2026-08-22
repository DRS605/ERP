# Modelo 347 — Operaciones con terceros

Dentro del módulo **Informes** (junto al 390, en la «declaración anual»). Lista los clientes y
proveedores con los que el volumen de operaciones del año (IVA incluido) supera **3.005,06 €** y
genera el **fichero telemático** en el diseño de registro de la AEAT.

## Cálculo

- **Clientes**: se agrupan las facturas emitidas por NIF (o nombre si no hay NIF), clave de
  operación **B** (entregas/ventas del declarante).
- **Proveedores**: se agrupan los gastos por proveedor (NIF de su ficha), clave **A**
  (adquisiciones/compras).
- Para cada tercero se calcula el **importe anual** y su **desglose por trimestre** (T1–T4).
- Solo se listan los que superan el umbral. Los terceros **sin NIF** aparecen en el resumen pero
  **no entran en el fichero oficial**.

## API

- `GET /informes/declaracion-anual?anio={a}` · `informe.leer` → resumen JSON (390 + 347 con
  trimestres y clave).
- `GET /informes/modelo-347/fichero?anio={a}` · `datos.exportar` → **fichero telemático** (`text/plain`).

## Fichero telemático (diseño de registro AEAT)

`FicheroModelo347.cs` sobre `RegistroAeat` (ver `docs/modulos/modelos-retenciones-irpf.md` para las
convenciones comunes: 500 posiciones, ISO-8859-1, numéricos con signo aparte y 2 decimales
implícitos, alfanuméricos en mayúsculas sin acentos). Registro tipo 1 (declarante) + un registro
tipo 2 por tercero, con NIF, nombre, signo+importe anual (pos. 83–98), clave de operación (pos. 99)
y el desglose por trimestre.

> **Validación:** como en el resto de ficheros AEAT, no fue posible confirmar cada offset contra el
> PDF oficial en el entorno de construcción; el desglose por trimestres sigue el patrón documentado
> (signo + importe por trimestre). **Valida el fichero con el servicio de predeclaración de la AEAT**
> antes de presentarlo. Las posiciones están centralizadas como constantes en `FicheroModelo347.cs`.

## Tests

- **Unitario** (`Modelo347FicheroTests`): registros de 500, cabecera `1347`, declarado `2347`,
  NIF/nombre/importe anual/clave/desglose trimestral en sus posiciones.
- **Integración** (`InformesEndpointsTests`): factura por encima del umbral → descarga del fichero
  con registros de 500 y el NIF del declarado.

## Interfaz

En el panel «Declaración anual» de Informes, botón **↓ Fichero oficial 347**, con el aviso de validar
en la AEAT.
