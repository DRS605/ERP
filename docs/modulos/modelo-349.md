# Modelo 349 — Operaciones intracomunitarias

Dentro del módulo **Informes**. Declaración recapitulativa de las operaciones intracomunitarias del
periodo, con **fichero telemático** en el diseño de registro de la AEAT.

## Marca de operación intracomunitaria

Un cliente o proveedor es **operador intracomunitario** cuando tiene informado su **NIF-IVA (VIES)**
—código de país (2 letras) + número, p. ej. `DE123456789`— en su ficha (campo `NifIva` añadido a
Cliente y Proveedor). No se han tocado los esquemas de facturas/gastos: las operaciones se derivan
del maestro de terceros.

- **Entregas (clave E)**: facturas emitidas cuyo cliente (casado por NIF fiscal) tiene NIF-IVA.
- **Adquisiciones (clave A)**: gastos cuyo proveedor (por id) tiene NIF-IVA.
- Se agrupa por operador y se suma la **base imponible** del trimestre.

## API

- `GET /informes/modelo-349?anio={a}&trimestre={1-4}` · `informe.leer` → resumen JSON.
- `GET /informes/modelo-349/fichero?anio={a}&trimestre={1-4}` · `datos.exportar` → **fichero
  telemático** (`text/plain`).

## Fichero telemático (diseño de registro AEAT)

`FicheroModelo349.cs` sobre `RegistroAeat`. Registro tipo 1 (declarante, con el **período** `1T`–`4T`)
+ un registro tipo 2 por operador con NIF-IVA (país+número, 17), nombre (40), clave (E/A) y base
imponible (13 = 11+2). Convenciones comunes en `docs/modulos/modelos-retenciones-irpf.md`.

> **Validación:** las longitudes de los campos del tipo 2 están verificadas; los offsets del bloque
> de totales del tipo 1 siguen el diseño estándar. Como en los demás modelos, **valida el fichero con
> el servicio de predeclaración de la AEAT** antes de presentarlo. Posiciones centralizadas como
> constantes en `FicheroModelo349.cs`.

## Alcance (honesto)

Se cubren las claves **E** (entregas de bienes) y **A** (adquisiciones), que son el caso habitual de
una pyme. Las claves de servicios (S/I), triangulares (T) y ventas en consigna (R/D/C) no se derivan
automáticamente porque el ERP no distingue bienes de servicios ni marca esas operaciones especiales;
quedan como ampliación futura (una clave por operación en factura/gasto).

## Tests

- **Unitario** (`Modelo349Tests`): separación de entregas/adquisiciones, filtro por trimestre y
  fichero (registros de 500, `1349`/`2349`, período, NIF-IVA y base en sus posiciones).
- **Integración** (`InformesEndpointsTests`): cliente con NIF-IVA + factura → 349 con una entrega y
  descarga del fichero.

## Interfaz

En Terceros, campo **NIF-IVA intracomunitario** en las fichas de cliente y proveedor. En Informes,
panel «Modelo 349» con los operadores del trimestre y el botón **↓ Fichero oficial 349**.
