# Factura electrónica (Facturae 3.2.2 + FACe)

Genera la **factura electrónica española (Facturae) versión 3.2.2** en XML a partir de cualquier
factura ya emitida. Es el formato estructurado que exige la Administración a través de **FACe** (el
Punto General de Entrada de Facturas Electrónicas) y que aceptan gestorías y clientes que trabajan
con factura-e. No sustituye a VeriFactu ni al PDF: es un canal adicional del **mismo** documento
fiscal ya emitido e inmutable.

## Qué se genera

`GET /facturas/{id}/facturae.xml` devuelve el XML Facturae 3.2.2 de la factura (permiso
`factura.leer`). Contiene:

- **FileHeader** — versión de esquema 3.2.2, modalidad individual, tipo emisor, y el *batch* con los
  totales (importe de facturas, pendiente y ejecutable) en EUR.
- **Parties** — vendedor (`SellerParty`, la empresa) y comprador (`BuyerParty`, el cliente), cada uno
  con su identificación fiscal (tipo de persona física/jurídica deducido del NIF, residencia) y su
  dirección (nacional o de ultramar). El comprador toma su **dirección congelada** de la factura.
- **Invoice** — cabecera (número, tipo de documento FC/FA, clase OO/OR), fecha e idioma, el
  **desglose de IVA** repercutido por tipo (con recargo de equivalencia si aplica), la **retención de
  IRPF** como impuesto retenido, los **totales** y las **líneas** (`Items`) con su descripción,
  cantidad, precio, descuento e impuestos por línea.

Los importes cuadran con el documento fiscal: `TotalExecutableAmount` coincide con el total de la
factura (base + IVA + recargo − retención).

## Centros administrativos DIR3 (FACe)

Cuando el destinatario es una **Administración Pública**, FACe necesita sus **códigos DIR3** para
enrutar la factura. Se guardan en el **cliente**:

- `EsAdministracionPublica` — marca al cliente como AAPP.
- `Dir3OficinaContable` (rol fiscal **01**), `Dir3OrganoGestor` (**02**), `Dir3UnidadTramitadora`
  (**03**) — los tres códigos DIR3 que facilita el propio organismo.

Si el cliente es AAPP y tiene los tres códigos (`CentrosDir3Completos`), el XML incluye el bloque
`AdministrativeCentres` con los tres centros y su rol. Un cliente privado no lleva centros. Los
códigos son **metadatos de enrutado**, no datos fiscales congelados: se leen del cliente en el
momento de generar el XML (no se copian en la factura), por lo que corregir un DIR3 mal tecleado no
exige reemitir la factura.

Una factura **simplificada** (ticket) sin destinatario identificado **no** puede convertirse a
Facturae: el endpoint responde `400` (`facturae.sin_destinatario`).

## Interfaz de usuario

- **Ficha del cliente**: casilla «Administración Pública (factura electrónica por FACe)» que
  despliega los tres campos DIR3.
- **Detalle de la factura**: botón **«Facturae (XML)»** (junto a «Registro XML» de VeriFactu),
  visible en facturas con destinatario identificado.

## Alcance y paso siguiente (firma + envío)

Se genera el **documento de negocio sin firmar**. Para presentar en FACe faltan dos pasos que
dependen del **certificado electrónico de la empresa** —hoy no disponible en el entorno, igual que el
envío en vivo de VeriFactu a la AEAT—:

1. **Firma electrónica XAdES** envuelta (`XAdES-EPES`) sobre este XML.
2. **Envío al servicio web de FACe** (SSPP) y seguimiento del estado (registrada, contabilizada,
   pagada, rechazada).

Ambos son **aditivos**: se conectan sobre este generador sin rehacerlo, del mismo modo que VeriFactu
deja el envío SOAP a expensas del certificado. El XML producido ya es el que se firmaría y remitiría.

## Persistencia

Los cuatro campos DIR3 viven en la tabla `terceros.cliente`
(`es_administracion_publica`, `dir3_oficina_contable`, `dir3_organo_gestor`,
`dir3_unidad_tramitadora`); migración `CentrosDir3Cliente`. El generador Facturae no añade tablas: es
una proyección XML de datos ya existentes (factura + empresa + cliente).

## Tests

- **Unitarios** (dominio Terceros): fijar y normalizar los centros DIR3; desmarcar AAPP limpia los
  códigos; completitud de los tres centros.
- **Integración**: una factura a cliente privado genera Facturae 3.2.2 con partes y totales correctos
  y **sin** centros; una factura a una AAPP con DIR3 completo incluye los tres `AdministrativeCentres`
  con sus roles 01/02/03; un ticket sin destinatario da `400`.
