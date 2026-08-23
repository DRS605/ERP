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

## API

- `POST /cartas-porte` — crea la carta (permiso `factura.crear`). Cuerpo: destinatario (cliente id o
  datos libres), transportista, matrícula, origen/destino, fechas, observaciones, serie, albarán
  opcional, y las líneas de mercancía.
- `GET /cartas-porte` y `GET /cartas-porte/{id}` — listado y detalle (permiso `factura.leer`).
- `GET /cartas-porte/{id}/pdf` — genera el PDF (QuestPDF) con remitente, destinatario, ruta,
  transportista/vehículo, relación de mercancías y casillas de firma.

## SPA

La pantalla **«Cartas de porte»** lista las emitidas y permite crear una nueva (destinatario,
transportista, matrícula, origen/destino y mercancías) y descargar su PDF.

## Tests

- **Unitarios**: `CartaPorte` (totales de bultos/peso, número completo con y sin serie, validación de
  destinatario y de al menos una mercancía).
- **Integración**: alta para un cliente (hereda el destino de su dirección, calcula totales, numera),
  numeración correlativa por empresa, generación del PDF y rechazo sin mercancías.
