# Carta de porte

La **carta de porte** es el documento de control del **transporte de mercancías por carretera**:
identifica al remitente (cargador), al destinatario, al transportista y al vehículo, el origen y el
destino, y la relación de mercancías (bultos y peso). En España acompaña al transporte de mercancías;
aquí se emite como documento con su PDF, opcionalmente ligado a un albarán de entrega.

## Modelo

- `CartaPorte` (`facturacion.carta_porte`) es un **documento por empresa** (no un maestro
  compartido): filtro y **RLS por empresa**, como el resto de documentos operativos.
- Numeración correlativa por **empresa + serie + ejercicio** (`NumeroCompleto` = `SERIE·AÑO/00001`).
- **Remitente** (cargador): se toma de la empresa emisora (razón social + NIF); el origen por defecto
  es su dirección fiscal.
- **Destinatario**: si se indica un cliente, se copian su nombre/NIF y el destino por defecto es su
  dirección; si no, se aceptan datos libres.
- **Transportista** (nombre + NIF), **matrícula** del vehículo, **fecha de expedición** y **de
  carga**, **observaciones** y **líneas de mercancía** (descripción, bultos, peso en kg). Totales de
  bultos y peso calculados.

## Datos de transporte

La carta de porte es siempre el documento **nacional** (no se emite CMR). Cuando se indican, el PDF lleva además el
remolque, el conductor, la temperatura de consigna y el termógrafo, el Incoterm y su lugar, los países de origen y
destino, los portes, los documentos que acompañan a la mercancía (los indicados en la carta y los certificados
fitosanitarios registrados) y las instrucciones del remitente.

## Transporte y comercio exterior

**Transportistas y vehículos** (`/transporte/transportistas`, `/transporte/vehiculos`), por empresa:

- el transportista, con su nombre, NIF, dirección, país y teléfono;
- el vehículo, con su matrícula normalizada (sin espacios ni guiones), remolque, tara, si es frigorífico y su
  transportista.

Al elegir un vehículo en la carta de porte se toman su matrícula, su remolque y su transportista, y de este el
nombre y el NIF. Lo que ya usan las cartas de porte no se elimina: se da de baja.

**Datos de la carta de porte** (`Transporte` en `POST /cartas-porte`):

- tipo (automático si se deja vacío) y modo (carretera, marítimo, aéreo, ferrocarril, multimodal);
- transportista, vehículo, remolque y dos conductores;
- temperatura de consigna (−60 a 60 °C) y termógrafo;
- Incoterm 2020 y su lugar (por defecto, los habituales del cliente) y portes pagados o debidos;
- documentos anexos e instrucciones del remitente;
- países de origen y destino (por defecto, los de la empresa y el cliente);
- envío marítimo: naviera, buque, contenedor, precinto, puertos de carga y destino y reserva; envío aéreo:
  compañía, vuelo y AWB.

Cada línea admite marcas, embalaje, peso neto, volumen y código arancelario. Si indica el artículo, el código
arancelario sale de su ficha.

**Expedición de palés**: al expedir con carta de porte se pueden elegir el vehículo, el transportista, la
temperatura y el termógrafo. Cada línea lleva el artículo (su código arancelario), su embalaje («Cajas en N palés»)
y los kilos como peso neto. Sin la tara de cajas y palés, el peso bruto es el mismo que el neto.

**Fichas**:

- el artículo tiene su código arancelario (NC de 8 dígitos o TARIC de 10) y su país de origen;
- el cliente tiene su EORI y su Incoterm habitual con el lugar;
- los clientes y proveedores tienen su dirección en el formulario. Antes no salía, y guardar la ficha vaciaba la
  dirección.
- El país se guarda como código ISO de dos letras. Se admite el nombre («Francia» → FR); uno desconocido se rechaza
  (400 `tercero.pais`) en lugar de fallar al guardar.

Guardas en la base de datos: códigos arancelarios de 8 o 10 dígitos, países de dos letras, Incoterms válidos,
temperatura en rango y FK de la carta de porte a sus transportista y vehículo.

## Certificados fitosanitarios

Cada carta de porte puede llevar sus certificados fitosanitarios (tabla `facturacion.certificado_fitosanitario`).
Cada certificado guarda:

- el tipo: `Exportacion`, `Reexportacion` o `PasaporteFitosanitario`;
- el número y la fecha de emisión;
- el país de destino (por defecto, el de la carta);
- el organismo emisor, la mercancía y unas observaciones;
- el documento escaneado, opcional: PDF, JPG o PNG de hasta 5 MB (400 `certificado.documento` si no cumple).

Salen en `certificados` del detalle de la carta («Certificado fitosanitario nº …»), en los documentos que acompañan del PDF de la carta y en los
datos de aduana de la factura. A una carta anulada no se le añaden certificados (409 `certificado.carta_anulada`).

API:
- `GET/POST /cartas-porte/{id}/certificados`
- `PUT/DELETE /certificados-fitosanitarios/{id}`
- `GET /certificados-fitosanitarios/{id}/documento`

En la pantalla de cartas de porte, el botón **«Certificados»** los lista, añade (con el fichero) y elimina.

## API

- `POST /cartas-porte` — crea la carta (permiso `factura.crear`). Cuerpo: destinatario (cliente id o
  datos libres), transportista, matrícula, origen/destino, fechas, observaciones, serie, albarán
  opcional, `transporte` (ver arriba) y las líneas de mercancía.
- `GET /transporte/incoterms`; CRUD de `/transporte/transportistas` y `/transporte/vehiculos`.
- `GET /cartas-porte` y `GET /cartas-porte/{id}` — listado y detalle (permiso `factura.leer`).
- `GET /cartas-porte/{id}/pdf` — genera el PDF (QuestPDF): la carta de porte nacional con remitente, destinatario, ruta, transporte, mercancías y firmas.

## SPA

La pantalla **«Cartas de porte»** lista las emitidas y permite crear una nueva (destinatario,
transportista, matrícula, origen/destino y mercancías) y descargar su PDF.

## Tests

- **Unitarios**: `CartaPorte` (totales de bultos/peso, número completo con y sin serie, validación de
  destinatario y de al menos una mercancía).
- **Integración**: alta para un cliente (hereda el destino de su dirección, calcula totales, numera),
  numeración correlativa por empresa, generación del PDF y rechazo sin mercancías.
