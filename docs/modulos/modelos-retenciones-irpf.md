# Modelos 111 y 190 — Retenciones de IRPF

Dentro del módulo **Informes**. Calculan las retenciones de IRPF que la empresa ha **practicado** a
sus perceptores (profesionales/proveedores) y generan los modelos **111** (autoliquidación
trimestral) y **190** (resumen anual), este último con **fichero telemático** en el diseño de la AEAT.

## Origen de los datos

Las retenciones practicadas se toman de los **gastos con retención** (`Gasto.RetencionIrpf > 0`,
estado ≠ Anulado): cuando la empresa recibe una factura de un profesional con retención de IRPF, es
la empresa quien retiene ese importe y lo ingresa en Hacienda vía el 111/190.

- El **perceptor** se resuelve por su ficha de proveedor (`ProveedorId`), de donde se obtienen
  **NIF** y **provincia**. Los gastos cuyo proveedor está solo como **texto libre** (sin ficha) no
  tienen NIF y **no pueden entrar en el fichero oficial del 190**: se listan aparte como aviso.
- Clave del 190: **G** (rendimientos de actividades económicas — actividades profesionales), que es
  la habitual para las retenciones que un autónomo/pyme practica a otros profesionales. Las
  retenciones de rendimientos del trabajo (nóminas) quedan fuera al no gestionarse aquí.

## Modelo 111 (trimestral)

Resumen del trimestre: número de perceptores, base de las percepciones, retenciones e importe a
ingresar. Es una **ayuda**: el 111 se presenta por formulario en la Sede de la AEAT.

`GET /informes/modelo-111?anio={a}&trimestre={1-4}` · permiso `informe.leer`.

## Modelo 190 (anual) y fichero oficial

Detalle por perceptor de las percepciones y retenciones del ejercicio (la suma de los cuatro 111).

- `GET /informes/modelo-190?anio={a}` · permiso `informe.leer` → resumen JSON (perceptores + los que
  no tienen NIF).
- `GET /informes/modelo-190/fichero?anio={a}` · permiso `datos.exportar` → **fichero telemático**
  (`text/plain`).

### Fichero telemático (diseño de registro AEAT)

Implementado en `AeatFicheros.cs` (constructor de registros de ancho fijo) y `FicheroModelo190.cs`.
Convenciones **oficiales** aplicadas:

- Registros de **500 posiciones**, codificación **ISO-8859-1**, separados por CRLF.
- Un registro **tipo 1** (declarante) seguido de un registro **tipo 2** por perceptor.
- **Numéricos**: a la derecha, ceros a la izquierda, 2 decimales implícitos (sin coma); el **signo**
  va en un campo aparte («N» negativo, espacio positivo).
- **Alfanuméricos**: a la izquierda, blancos a la derecha, MAYÚSCULAS sin acentos (la **Ñ** se
  conserva). Provincia por **código INE** (mapeada desde el nombre; «99» si se desconoce).

> **Validación (importante):** en el entorno de construcción no fue posible descargar los PDF
> oficiales del diseño de registro de la AEAT para confirmar cada offset byte a byte, por lo que
> algunos rellenos del bloque de totales del tipo 1 siguen el diseño estándar publicado. Como hace
> cualquier software fiscal, **el fichero generado debe validarse con el servicio de predeclaración /
> validación de la AEAT** antes de su presentación oficial. Las posiciones están centralizadas como
> constantes en `FicheroModelo190.cs` para ajustarlas con facilidad.

## Tests

- **Unitarios** (`RetencionesIrpfTests`): cálculo del 111 (suma por trimestre, recuento de
  perceptores), del 190 (agregación por perceptor, separación de los que no tienen NIF), y del
  **fichero** (registros de 500, cabecera `1190`, perceptor `2190`, NIF/clave/provincia y campos de
  importe en sus posiciones, mayúsculas sin acentos con Ñ, codificación del signo, códigos de
  provincia).
- **Integración** (`RetencionesIrpfEndpointsTests`): alta de proveedor + gasto con retención →
  modelo 111 y 190 correctos y descarga del fichero (líneas de 500); aviso cuando no hay perceptores
  con NIF.

## Interfaz

En **Informes**, panel «Retenciones de IRPF» con el 111 del trimestre elegido, el detalle del 190 y
el botón **↓ Fichero oficial 190**, con el aviso de validar en la AEAT.
