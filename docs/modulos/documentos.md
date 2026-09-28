# Módulo Documentos

Generación del **PDF** de facturas y **envío por correo**. No tiene persistencia propia: compone la
factura (Facturación) y los datos del emisor (Organización).

## PDF

`GET /facturas/{id}/pdf` devuelve el PDF de la factura (cabecera con la empresa emisora, datos del
cliente, líneas, base/IVA/IRPF y total). Se genera con **QuestPDF** (licencia Community). El importe
se formatea en español (coma decimal) sin depender de la cultura instalada.

### Albaranes, pedidos y liquidaciones

El resto de documentos comerciales se imprime con un generador común (`IGeneradorPdfDocumento`), con la
misma plantilla de marca. Cada módulo traduce su documento a un `DocumentoImpreso`: título, número,
fecha, tercero, líneas, totales, datos de cabecera y leyenda. Lo hace `Api/Comun/ImpresosComerciales`.

| Documento | Ruta | Contenido |
|---|---|---|
| Albarán de venta | `GET /albaranes-venta/{id}/pdf[?valorado=false]` | Valorado, con precio, descuento, importe y base; o sin precios, el que viaja con la mercancía. Si queda algún precio por fijar, la línea lo avisa. |
| Pedido de venta | `GET /pedidos-venta/{id}/pdf` | Confirmación de pedido, con lo ya servido por línea. |
| Pedido de compra | `GET /compras/pedidos/{id}/pdf` | Para enviar al proveedor. |
| Liquidación al agricultor | `GET /agro/liquidaciones/{id}/pdf` | Entregas con kilos, precio e importe, descuentos, impuesto o compensación REAGP, retención y líquido. Lleva la leyenda de autofactura o del recibo de compensación REAGP. |

La columna de descuento solo aparece si alguna línea lo tiene. Las cantidades se muestran con los
decimales que tienen, hasta un máximo de tres.

## Plantilla de documentos (marca configurable por el usuario)

El aspecto de facturas, tickets y presupuestos es **configurable por el propio usuario** desde
*Ajustes → Plantilla de documentos*, sin tocar código. La empresa guarda: **logotipo** (PNG, máx.
512 KB), **color corporativo** (hex, aplicado a títulos, línea de cabecera de la tabla y total),
**dirección** y **datos de contacto** (teléfono, web, correo) que encabezan el documento, y un
**texto de pie** libre (condiciones, forma de pago, agradecimiento). Todo es opcional: cuando falta un
dato, el generador cae al comportamiento por defecto (razón social + NIF, cian de ALXOR, pie «ALXOR
Core»). La lógica común de pintado vive en `PlantillaImpreso` y la comparten los tres generadores.

Se configura con `PUT /empresas/actual/plantilla` (permiso `empresa.ajustes`); el logotipo viaja en
Base64 (cadena vacía = quitarlo, ausente = no tocarlo) y se valida que sea PNG y no exceda el tamaño.
El validador del dominio (`Empresa.EstablecerPlantillaDocumento`) rechaza colores mal formados y logos
que no sean PNG. Los datos se exponen en `EmpresaDto` (que ahora incluye dirección, contacto, color,
pie y logo) y `GET /empresas/actual` los devuelve para previsualizar.

## Correo

`POST /facturas/{id}/enviar` con `{ "email": "..." }` envía la factura con el PDF adjunto. En el MVP
el envío es un **stub** (registra en el log); el proveedor real (SMTP/servicio) se añadirá sin
cambiar el puerto `IServicioCorreo`.

## API

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/facturas/{id}/pdf` | permiso `factura.leer` | Descarga el PDF. |
| `GET` | `/albaranes-venta/{id}/pdf`, `/pedidos-venta/{id}/pdf` | permiso `factura.leer` | PDF del albarán o del pedido de venta. |
| `GET` | `/compras/pedidos/{id}/pdf` | permiso `compra.leer` | PDF del pedido de compra. |
| `GET` | `/agro/liquidaciones/{id}/pdf` | permiso `agro.leer` | PDF de la liquidación al agricultor. |
| `POST` | `/facturas/{id}/enviar` | permiso `factura.leer` | Envía la factura por correo. |
| `PUT` | `/empresas/actual/plantilla` | permiso `empresa.ajustes` | Configura la plantilla (logo, color, contacto, pie). |

## Puertos

- `IGeneradorPdfFactura` → implementado con QuestPDF.
- `IServicioCorreo` → stub que se sustituirá por el proveedor real.

## Tests

- **Integración**: descarga del PDF (200, `application/pdf`, cabecera `%PDF`) y envío por correo (204);
  guardar y releer la plantilla con logo, color inválido rechazado, y generación del PDF con la
  plantilla aplicada.
- **Unitarios** (Organización): validación de la plantilla en el dominio (color hex, logo PNG, tamaño
  máximo, quitar logo con array vacío, null no toca el logo existente).
