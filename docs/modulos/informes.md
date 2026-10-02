# Módulo Informes

Lecturas agregadas para el **panel principal**, los **libros de IVA** y la **exportación para la
gestoría**. No tiene persistencia: compone las consultas de Facturación, Gastos y Tesorería.

## Dashboard

`GET /informes/dashboard` devuelve: facturado y gastado del **mes en curso**, número de facturas del
mes, y **pendiente de cobro** / **pendiente de pago** (total de documentos menos lo liquidado en
Tesorería).

## Libros de IVA

`GET /informes/libro-iva?tipo=Repercutido|Soportado&desde=&hasta=` devuelve los asientos del periodo
con sus totales:

- **Repercutido**: a partir de las facturas emitidas (fecha, número, cliente, NIF, base, cuota IVA).
- **Soportado**: a partir de los gastos (fecha, concepto, proveedor, base, cuota IVA).

> Simplificación del MVP: un asiento por documento con su base y cuota totales. El desglose por tipo
> de IVA dentro de un mismo documento es una mejora futura.

## Exportación para la gestoría

`GET /informes/libro-iva/csv?tipo=&desde=&hasta=` descarga el libro en **CSV** (separador `;`,
decimales con coma, formato español), listo para la gestoría. Requiere el permiso `datos.exportar`.

## Resúmenes fiscales trimestrales (303 y 130)

`GET /informes/resumen-trimestral?anio=&trimestre=1..4` calcula, a partir de las facturas emitidas y
los gastos, los dos modelos que un autónomo en estimación directa presenta cada trimestre. Es una
**ayuda informativa** para prepararlos con la gestoría, **no** un envío oficial a la AEAT.

- **Modelo 303 (IVA)** — por **trimestre**: `IVA repercutido` (cuota de las facturas del trimestre)
  menos `IVA soportado` (cuota de los gastos). El **resultado** positivo es *a ingresar*, negativo
  *a compensar/devolver*.
- **Modelo 130 (IRPF)** — **acumulado** desde el 1 de enero: sobre el rendimiento neto acumulado
  (`ingresos − gastos`, en base imponible) se aplica el **20 %**, del que se descuentan las
  **retenciones soportadas** (el IRPF que los clientes retuvieron en tus facturas) y los **pagos
  fraccionados de los trimestres anteriores**. Nunca resulta negativo (mínimo 0).

Solo cuentan las facturas en estado **Emitida**: se excluyen las **anuladas** y las ya
**rectificadas** (sustituidas por su rectificativa, que aporta los importes corregidos). Requiere el
permiso `informe.leer`.

## Declaraciones anuales (390 y 347)

`GET /informes/declaracion-anual?anio=` calcula, para el ejercicio indicado, los dos resúmenes
anuales. Como los trimestrales, es **ayuda informativa** para la gestoría, no un envío a la AEAT.

- **Modelo 390 (resumen anual de IVA)** — es la suma de los cuatro `303` del año: IVA repercutido
  (facturas emitidas del ejercicio) menos IVA soportado (gastos del ejercicio).
- **Modelo 347 (operaciones con terceros)** — relación de **clientes** y **proveedores** con los que
  el volumen de operaciones del año (**IVA incluido**) supera el umbral legal de **3.005,06 €**. Los
  clientes se agrupan por NIF (o por nombre si la factura no lo llevaba) sumando el total de sus
  facturas; los proveedores se agrupan por el maestro de proveedores (resolviendo nombre y NIF) o,
  en su defecto, por el texto libre del gasto. Requiere el permiso `informe.leer`.

## SII (Suministro Inmediato de Información)

`GET /informes/sii?tipo=Emitidas|Recibidas&ejercicio=&periodo=1..12` genera el **XML del libro
registro** de facturas expedidas (`Emitidas`) o recibidas (`Recibidas`) de un mes, con la estructura
y espacios de nombres del SII de la AEAT (`SuministroInformacion.xsd` / `SuministroLR.xsd`,
`IDVersionSii` 1.1, comunicación `A0` de alta). Pensado para grandes empresas obligadas al SII
(&gt;6 M€ de facturación) que deben remitir sus libros en un plazo de 4 días.

La descarga sirve para revisar el libro. **El envío a la AEAT** se hace desde ALXOR (ver «Envío del SII»
más abajo). Descarga el fichero `application/xml`; requiere el permiso `datos.exportar`.

Cada factura emitida lleva **un `DetalleIVA` por tipo impositivo** (con su recargo de equivalencia si lo
hay), no un tipo medio. En recibidas, el `IDEmisorFactura` y la `Contraparte` se identifican por el **NIF del
proveedor** de la ficha (si falta, un `IDOtro` tipo 07 con el nombre y un comentario «completar antes de
enviar»), el tipo es el del gasto y los **gastos anulados no se declaran**. Sin número de factura del proveedor en el
gasto, se genera uno estable (`G{ejercicio}-` y 8 caracteres del gasto) a completar antes de enviar. Las facturas
emitidas **anuladas** no se dan de alta. Las autofacturas y recibos **REAGP** (tipo `REAGP12` o `REAGP105`) van con
la clave de régimen **02** y su `PorcentCompensacionREAGYP` e `ImporteCompensacionREAGYP`.

### Envío del SII

Como en Hispatec, se lleva un **registro por factura** (esquema `fiscal`: `certificado`, `envio_sii` y
`registro_sii`, con RLS por empresa):

- **Certificado**: `PUT /informes/sii/certificado` recibe el `.pfx` o `.p12` en base64 y su contraseña. Comprueba que
  se abre, que tiene clave privada y que no ha caducado, y lo guarda **cifrado** con la protección de datos de
  ASP.NET Core. Las claves de cifrado se guardan en `ProteccionDatos:Ruta`, que en producción debe ser un volumen
  persistente. La API nunca devuelve el certificado; solo el titular, el NIF, la caducidad y el entorno. El entorno
  empieza en **Pruebas** (`prewww1.aeat.es`) y se pasa a **Produccion** con `PUT /informes/sii/certificado/entorno`.
- **Envío**: `POST /informes/sii/enviar` (libro, ejercicio, mes) compara cada documento con lo que consta enviado,
  mediante la huella SHA-256 de su registro:
  - los nuevos o rechazados van como alta **A0**;
  - los aceptados que han cambiado, o que se aceptaron con errores, van como modificación **A1**;
  - los anulados que se habían aceptado se dan de **baja**.

  Cada envío se hace por SOAP con autenticación TLS de cliente, en bloques de hasta 10.000 facturas. Lo aceptado sin
  cambios no se reenvía (`sii.nada_que_enviar`).
- **Respuesta**: se guarda la petición, la respuesta, el CSV y el estado global (`Correcto`, `ParcialmenteCorrecto`,
  `Incorrecto` o `ErrorComunicacion`). Cada factura queda `Correcto`, `AceptadoConErrores`, `Incorrecto` (con el
  código y la descripción de la AEAT) o `DadoDeBaja`. El código 3000 (factura ya registrada) se trata como aceptada y
  se corrige con A1.
- **Consultas**:
  - `GET /informes/sii/situacion` da la situación de cada factura del mes: pendiente, enviada, rechazada, modificada,
    pendiente de baja o dada de baja.
  - `GET /informes/sii/envios` da el histórico.
  - `GET /informes/sii/envios/{id}/xml?parte=peticion|respuesta` descarga la petición o la respuesta de un envío.
- **Interfaz**: en Informes, el panel SII muestra el certificado, la situación del mes, el botón «Enviar a la AEAT» y
  el histórico.

### SII-IGIC de la Agencia Tributaria Canaria

Las facturas y los gastos con **IGIC** se llevan al SII de la **Agencia Tributaria Canaria (ATC)**, no al de la AEAT.
Todos los puntos del SII (`/informes/sii`, `/situacion`, `/enviar`) aceptan `administracion` = `Aeat` o `Atc`:

- sin indicarlo, el del territorio de la empresa: la AEAT en la Península y la ATC en Canarias;
- una empresa que **opera en los dos territorios** lleva los dos SII: al de la AEAT van solo los documentos con IVA y al
  de la ATC solo los del IGIC. Pedir el de un impuesto por el que no tributa es un 400 `sii.administracion`;
- son **libros distintos** en el registro (`IgicEmitidas` / `IgicRecibidas`): enviar uno no marca nada en el otro. El
  certificado es el mismo (mismo NIF);
- el suministro tiene la **misma estructura** que el de la AEAT (cabecera, `RegistroLRFacturasEmitidas`/`Recibidas`,
  A0/A1/baja, desgloses) y cambia la raíz de los espacios de nombres de los esquemas y las direcciones del servicio.

**Configuración** (`Sii:Atc`): `EspacioNombres` (raíz de los esquemas, sin el nombre del `.xsd`), `ServidorPruebas`,
`ServidorProduccion`, `RutaEmitidas` y `RutaRecibidas`, tomados de la documentación técnica vigente de la ATC. No van
puestos de fábrica porque ALXOR no los ha podido contrastar con la ATC: **hay que confirmarlos antes de usarlo**. Sin
configurar, el XML se puede descargar para revisarlo (con un espacio de nombres provisional `urn:alxor:…`), pero el
envío se rechaza con `sii.atc_sin_configurar`.

En la pantalla, una empresa de los dos territorios elige la administración en el panel SII; una de Canarias trabaja
directamente con la ATC («Enviar a la ATC»).

**Emitidas según la clase de sus tipos de IVA:**

- **Clave de régimen:** 02 exportación (o régimen de viajeros), 03 bienes usados (REBU), 05 agencias de viajes,
  07 criterio de caja; si no, 01 régimen general.
- **Desglose:** lo exento va en `Sujeta/Exenta` con su causa (E1 art. 20, E2 exportación, E5 entrega intracomunitaria,
  E6 otras); lo no sujeto, en `NoSujeta`; la inversión del sujeto pasivo, como `NoExenta` S2 (S3 si se mezcla con S1).
- **Contraparte extranjera** (país del cliente distinto de ES): `IDOtro` con el país y el tipo de documento (02 NIF-IVA
  en la UE, 04 fuera), y el desglose va en `DesgloseTipoOperacion/Entrega`, como pide la AEAT.

Pendiente: los certificados de sello (servidores `www10`), la consulta de lo registrado en la AEAT
(`ConsultaLRFacturas…`), otros libros (bienes de inversión, cobros en metálico) y las prestaciones de servicios a
extranjeros (hoy todo va como entrega de bienes).

En la interfaz clásica, Informes tiene selector de **ejercicio** y trimestre; el libro de IVA muestra
repercutido o **soportado** (con NIF) por año o trimestre y su CSV respeta esa selección, y el panel SII
descarga el XML del mes indicando que **no se envía a la AEAT**.

## Beneficio (margen bruto y neto)

`GET /informes/beneficio?desde=&hasta=` calcula el beneficio del periodo a partir del **margen por
línea** de las facturas emitidas (venta − coste congelado) y de los gastos:

- **Margen bruto** = `Σ ingresos de venta − Σ coste (precio de compra)`.
- **Beneficio neto** = `margen bruto − gastos genéricos del periodo`.
- **Desglose por artículo/concepto**: unidades, ingresos, coste y margen, ordenado por margen.

El coste sale del **precio de compra congelado** en cada línea al emitir (Catálogo → Facturación),
de modo que el margen no cambia aunque después varíe el coste del producto. Requiere `informe.leer`.

A partir de ese desglose por artículo, la interfaz muestra un **ranking de artículos**: los **más
rentables** (mayor margen), el **rey de las ventas** (mayores ingresos) y **dónde se gana menos**
(menor margen), con el margen en % sobre ingresos. Es una vista derivada, sin endpoint propio.

## Informes de gestión (análisis de negocio)

Además de los informes fiscales, hay un bloque de **análisis de gestión** (pantalla *Análisis de
gestión*) que agrega los datos que ya exponen los demás módulos, sin persistencia propia:

- **Ventas por cliente** (`GET /informes/ventas-cliente?desde=&hasta=`): ranking de clientes por total
  facturado del periodo (nº de facturas, base y total). Excluye facturas anuladas y rectificadas.
- **Ventas por artículo** (`GET /informes/ventas-articulo?desde=&hasta=`): unidades, ingresos, coste y
  **margen** por artículo (a partir del margen por línea de las facturas), ordenado por ingresos.
- **Compras por proveedor** (`GET /informes/compras-proveedor?desde=&hasta=`): gasto agregado por
  proveedor del periodo (resuelve el nombre por el maestro de proveedores o el texto libre del gasto).
- **Comparativa mensual** (`GET /informes/comparativa-mensual?anio=`): ventas, gastos y resultado mes a
  mes del ejercicio (bases imponibles), para ver evolución y estacionalidad.
- **Rotación de existencias** (`GET /informes/rotacion-stock?desde=&hasta=`): cruza las unidades
  vendidas por artículo con el stock actual; calcula la **rotación** (vendidas ÷ stock) y los **días de
  cobertura** (stock ÷ ventas diarias) de los artículos con control de stock.
- **Extracto de tercero** (`GET /informes/extracto-tercero?tipo=Cliente|Proveedor&terceroId=&desde=&hasta=`):
  documentos de un cliente (facturas/cobros) o proveedor (gastos/pagos) con total, liquidado y
  pendiente.
- **Aging de cartera** (`GET /informes/aging-cartera`): antigüedad del saldo **pendiente de cobro** a
  día de hoy, clasificado por tramos de vencimiento (por vencer, 1-30, 31-60, 61-90, más de 90 días).

Los dos últimos cruzan los documentos con lo liquidado en Tesorería mediante una consulta agregada
(`IConsultaTesoreria.LiquidadoPorDocumentosAsync`, un solo `GROUP BY`) para calcular el pendiente sin
una consulta por documento. Todos requieren el permiso `informe.leer`.

## Cierre de caja (arqueo diario)

`GET /informes/cierre-caja?dia=` devuelve el **cierre de caja** de un día a partir de los movimientos
de Tesorería: **total cobrado** desglosado por **método de pago** (efectivo, tarjeta, Bizum…), total
pagado (salidas) y **neto**. Pensado para cuadrar la caja de una tienda al cerrar; accesible desde el
botón *Cierre de caja* del TPV.

## API

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/informes/dashboard` | permiso `informe.leer` | Panel principal. |
| `GET` | `/informes/libro-iva` | permiso `informe.leer` | Libro de IVA del periodo. |
| `GET` | `/informes/libro-iva/csv` | permiso `datos.exportar` | Exportación CSV. |
| `GET` | `/informes/resumen-trimestral` | permiso `informe.leer` | Resúmenes 303 (IVA) y 130 (IRPF) del trimestre. |
| `GET` | `/informes/declaracion-anual` | permiso `informe.leer` | Declaraciones anuales 390 (IVA) y 347 (terceros). |
| `GET` | `/informes/sii` | permiso `datos.exportar` | XML del SII (libro de facturas emitidas o recibidas de un mes). |
| `GET`/`PUT`/`DELETE` | `/informes/sii/certificado` | `informe.leer` / `empresa.ajustes` | Certificado de la empresa (se guarda cifrado). |
| `PUT` | `/informes/sii/certificado/entorno` | `empresa.ajustes` | Entorno Pruebas o Produccion. |
| `GET` | `/informes/sii/situacion` | `informe.leer` | Situación de cada factura del mes en el SII. |
| `POST` | `/informes/sii/enviar` | `contabilidad.gestionar` | Envía altas (A0), modificaciones (A1) y bajas del mes. |
| `GET` | `/informes/sii/envios` | `informe.leer` | Histórico de envíos (estado, CSV, recuento). |
| `GET` | `/informes/sii/envios/{id}/xml` | `datos.exportar` | Petición o respuesta de un envío. |
| `GET` | `/informes/ventas-cliente` | permiso `informe.leer` | Ventas por cliente del periodo. |
| `GET` | `/informes/ventas-articulo` | permiso `informe.leer` | Ventas por artículo (con margen). |
| `GET` | `/informes/compras-proveedor` | permiso `informe.leer` | Compras/gastos por proveedor. |
| `GET` | `/informes/comparativa-mensual` | permiso `informe.leer` | Ventas/gastos/resultado mes a mes. |
| `GET` | `/informes/rotacion-stock` | permiso `informe.leer` | Rotación y cobertura de existencias. |
| `GET` | `/informes/extracto-tercero` | permiso `informe.leer` | Extracto de cliente o proveedor. |
| `GET` | `/informes/aging-cartera` | permiso `informe.leer` | Antigüedad de la cartera de cobro. |
| `GET` | `/informes/beneficio` | permiso `informe.leer` | Beneficio del periodo (margen bruto y neto). |
| `GET` | `/informes/cierre-caja?dia=` | permiso `informe.leer` | Cierre de caja de un día (cobrado por método, pagado, neto). |

## Tests

- **Unitarios**: exportador CSV (cabecera, asientos, totales, escapado); resúmenes fiscales (303
  repercutido − soportado por trimestre; 130 acumulado con el 20 %, retenciones, pagos anteriores y
  suelo en 0; exclusión de facturas anuladas/rectificadas; trimestre fuera de rango).
- **Integración**: dashboard (facturado/gastado/pendientes y su actualización tras un cobro), libro
  de IVA repercutido, exportación CSV y resumen trimestral (303 y 130); generación del XML del SII
  (facturas emitidas y recibidas del periodo, periodo fuera de rango); informes de gestión (ventas por
  cliente ordenadas, ventas por artículo con margen, compras por proveedor, comparativa mensual,
  rotación de stock, y aging + extracto con actualización del pendiente tras un cobro parcial).

> Para análisis libres (dimensiones × medidas, tabla dinámica, comparación, filtros y informes guardados) ver [Análisis de datos](analisis.md).
