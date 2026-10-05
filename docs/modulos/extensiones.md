# Extensiones: campos personalizados, adjuntos y alertas

Parte de la base (todas las ediciones). Proyectos `AlxorCore.Extensiones` y `AlxorCore.Extensiones.Infraestructura`,
esquema `extensiones`, rutas `/extensiones`. No conoce los demás módulos: lo que vigilan las alertas llega por el
puerto `IFuentesAlertas`, que implementa la API (`FuentesAlertasErp`).

## Registros extensibles

`EntidadesExtensibles` es el catálogo de registros que admiten campos y adjuntos, cada uno con su código estable
(`cliente`, `proveedor`, `producto`, `factura`, `albaran_venta`, `gasto`, `agricultor`, `partida`, `pale`, `socio`,
`deposito`, `lote_planta`…) y los permisos con que se **ven** y se **cambian** sus valores y adjuntos (los mismos del
registro: un usuario que no ve los clientes tampoco ve sus campos ni sus ficheros). `GET /extensiones/entidades`.

## Campos personalizados

- **Definición** (`definicion_campo`, permiso `empresa.ajustes`): registro, código (`^[a-z][a-z0-9_]*$`, único por
  registro), etiqueta, tipo (`Texto`, `Numero`, `Fecha`, `SiNo`, `Lista` con sus opciones), obligatorio, valor por
  defecto, ayuda, orden y activo. El registro, el código y el tipo no cambian (los valores dependen de ellos).
- **Valores** (`valor_campo`, uno por campo y registro) normalizados: número con punto, fecha `AAAA-MM-DD`, sí/no
  como `true`/`false`, la opción tal como está en la lista. Se aceptan la coma decimal y `DD/MM/AAAA`.
- `PUT /extensiones/valores/{entidad}/{id}` guarda por código, todo o nada: vacío quita el valor (salvo en un
  obligatorio); un obligatorio sin valor toma el defecto o da error.
- `GET /extensiones/buscar/{entidad}?codigo=&valor=&desde=&hasta=`: ids de los registros con ese valor (texto que
  contiene; número y fecha también por rango).
- Un campo con valores no se borra (`campo.con_valores`): se desactiva. Una lista no puede perder una opción en uso
  (`campo.opciones_en_uso`).

## Adjuntos

`adjunto` (metadatos) y `adjunto_contenido` (los bytes, aparte para que los listados no los carguen). Nombre del
fichero sin ruta, tipos admitidos (PDF, imágenes, texto, CSV, XML, Excel, Word, correos), hasta 10 MB, huella
SHA-256. Subida en JSON con el contenido en base64; descarga en `GET /extensiones/adjuntos/{id}/contenido`;
`POST /extensiones/adjuntos/{entidad}/contar` da cuántos tiene cada registro de una lista.

## Alertas

- **Reglas** (`regla_alerta`, permiso `empresa.ajustes`):
  - `FacturasVencidas`: facturas emitidas con pendiente y más de N días de retraso (de los impagados de tesorería).
  - `RiesgoSuperado`: clientes con límite cuyo riesgo (pendiente de cobro + pendiente de facturar) llega al
    porcentaje indicado del límite.
  - `FechaCampo`: un campo personalizado de tipo fecha que vence dentro de N días o ya ha vencido (ITV, certificado
    IFS/GlobalG.A.P., revisión de báscula…).
  - `CertificadoCaducidad`: el certificado del SII caduca dentro de N días.
  - `Evento`: ocurre algo (`FacturaEmitida`, `AlbaranVentaAnulado`, `GastoRegistrado`, `ClienteCreado`…). Lo
    registra `PublicadorEventosIntegraciones` tras guardar; un fallo ahí se registra en el log y no tumba la operación.
  - **Permiso de destino**: ven la alerta los usuarios de la empresa con ese permiso.
- **Evaluación** sin procesos que recorran empresas (con RLS forzado cada empresa se evalúa en su propio contexto):
  al consultar las alertas (`GET /extensiones/alertas`) si la última evaluación tiene más de 15 minutos, o al pedirlo
  (`POST /extensiones/alertas/evaluar`). Bajo un candado por empresa: cada hallazgo tiene una **clave** (la factura, el
  cliente, el valor de la fecha); mientras haya una alerta viva con esa clave no se repite (índice único parcial
  `ux_alerta_viva_clave`), y la que ya no se encuentra **se resuelve sola**.
- Cada usuario marca las suyas como leídas (`lectura_alerta`) y cualquiera que las vea puede resolverlas a mano.
- Una regla que ya ha dado alertas no se borra (`alerta.regla_con_alertas`): se desactiva.

## Garantías en la base de datos

RLS por empresa en todas las tablas (las hijas `opcion_campo` y `adjunto_contenido`, por su padre); CHECK del código
del campo, del tamaño y la huella del adjunto, de los umbrales y los datos de cada tipo de regla. La baja de la
empresa borra sus extensiones.

## Etiquetas por cliente o plataforma

- **Plantillas** (`extensiones.plantilla_etiqueta`, `/extensiones/etiquetas/plantillas`): una general y una por cliente o
  plataforma (una sola activa de cada): marca comercial, qué datos salen y en qué orden (producto, referencia del
  cliente, marca, cajas, pesos, lote, fecha, consumo preferente, GTIN, tipo de palé, origen, destinatario, texto fijo),
  el texto fijo y el formato (A6 o rollo de 100 × 150 mm). El SSCC y los códigos GS1-128 salen siempre.
- **Referencias** (`extensiones.referencia_cliente`, `PUT /extensiones/etiquetas/referencias`): el código, la descripción
  y, si lo pide distinto, el GTIN del artículo en el cliente (con el dígito de control comprobado).
- Las etiquetas de palé de logística (`/logistica/unidades/{sscc}/etiqueta`) y de agro (`/agro/pales/{sscc}/etiqueta`)
  aplican la plantilla del cliente del palé (o la general) y su referencia, en **PDF** o, con `formato=zpl`, en **ZPL**
  para impresoras Zebra (203 ppp, 100 × 150 mm, códigos GS1-128 en modo UCC/EAN).
