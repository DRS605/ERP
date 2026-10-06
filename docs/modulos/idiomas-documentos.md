# Documentos en otros idiomas

Las facturas, los presupuestos, los albaranes de venta y los pedidos de venta y de compra se imprimen en el idioma
del cliente o del proveedor. Hay seis idiomas: castellano (el de siempre), inglés, francés, alemán, italiano y
portugués.

## Qué se configura

- **Ficha del cliente y del proveedor:** el campo `idioma` (`en`, `fr`, `de`, `it`, `pt`). Si se deja vacío o es
  `es`, el tercero queda en castellano. Un código desconocido da 400 `tercero.idioma`. En la SPA es el desplegable
  «Idioma de sus documentos».
- **Artículo:** el nombre en otros idiomas, una traducción por idioma.
  - `GET` y `PUT /productos/{id}/traducciones` con `[{ idioma, nombre }]`. El `PUT` sustituye la lista entera y
    quita las traducciones vacías.
  - El castellano no se traduce (400 `producto.idioma`).
  - En la ficha del artículo es la sección «Nombre en otros idiomas».
  - Las traducciones se guardan en `catalogo.traduccion_articulo`, que comparte la RLS del artículo.

## Qué cambia en el documento

Al imprimir, el idioma es el pedido con `?idioma=`, si se indica. Si no, el del cliente (factura, presupuesto,
albarán, pedido de venta) o el del proveedor (pedido de compra).

- **Textos fijos:**
  - el título del documento, la fecha y la validez;
  - las etiquetas del cliente o proveedor, del NIF (también el del emisor), de las columnas, de los totales y de las
    observaciones;
  - la numeración de las páginas y las leyendas (albarán valorado, confirmación de pedido, pedido de compra,
    presupuesto).

  Las siglas del IVA también se traducen: VAT, TVA, MwSt. El IGIC se queda como está.
- **Líneas:** si la descripción empieza por el nombre del artículo en castellano, tal como se puso al elegirlo, ese
  nombre se cambia por la traducción y se mantiene lo que venga detrás («Naranja Navel calibre 3» pasa a «Navel orange
  calibre 3»). Una descripción escrita a mano, o un artículo sin traducción a ese idioma, sale como está.
- **Mención fiscal de la factura:** se imprime siempre en castellano, que es la que vale ante la AEAT. Para las
  menciones de los tipos predeterminados (exenta, no sujeta, inversión del sujeto pasivo, entrega intracomunitaria y
  exportación) se añade debajo su traducción.
- **Números:** en inglés llevan punto decimal y coma de miles (1,510.00). En los demás idiomas se mantiene el
  formato español. Las fechas van siempre como dd/mm/aaaa.
- **Correo:** el asunto y el texto del envío de la factura o del presupuesto van en el mismo idioma que el PDF.
- **Estado del pedido** (borrador, confirmado…): es un dato interno y solo sale cuando el pedido se imprime en
  castellano.

Lo que se guarda no cambia. La factura, sus líneas, los libros, el SII y la contabilidad siguen en castellano. La
traducción solo se aplica al imprimir.

## Qué sigue en castellano

- El ticket (factura simplificada).
- La liquidación al agricultor, la hoja de carga y los justificantes de envases.
- La carta de porte nacional. El **CMR** ya es bilingüe (castellano e inglés) por diseño.

## API

- `GET /facturas/{id}/pdf?idioma=`
- `GET /presupuestos/{id}/pdf?idioma=`
- `GET /albaranes-venta/{id}/pdf?idioma=&valorado=`
- `GET /pedidos-venta/{id}/pdf?idioma=`
- `GET /compras/pedidos/{id}/pdf?idioma=`

Los textos están en `AlxorCore.Documentos.Aplicacion.TextosImpreso`. Las claves que no tienen traducción salen en
castellano.
