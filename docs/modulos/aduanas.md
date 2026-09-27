# Aduanas de exportación

Una factura de exportación (IVA `EXPORT`, exenta por el art. 21 LIVA, o `IGICEXPORT`) necesita la prueba de que la
mercancía salió de la UE: el **DUA de exportación**. Lo presenta el agente de aduanas con su programa. El ERP le da
los datos y guarda lo que devuelve la aduana.

## Pantalla «Ventas → Aduanas · exportación»

`GET /aduanas/exportaciones` lista las facturas con líneas de exportación (emitidas y no anuladas), con la base
exenta y su situación:

| Situación | Significa |
|---|---|
| **Sin DUA** | Falta la prueba de la exención |
| **Despachada** | Con DUA (MRN), sin salida confirmada |
| **Salida confirmada** | La aduana de salida confirmó que la mercancía salió |

Los códigos de exportación son los de la clase *Exportación* del catálogo de IVA de la empresa (y siempre `EXPORT` e
`IGICEXPORT`).

## DUA (despacho)

`POST /aduanas/facturas/{id}/despachos` registra:

- el **MRN**: 18 caracteres, dos dígitos del año, dos letras del país y 14 letras o números; se guarda en
  mayúsculas y sin espacios, y es único por empresa;
- la fecha del despacho;
- la **fecha de salida**, que no puede ser anterior al despacho;
- la aduana y unas observaciones.

Una factura puede tener varios despachos (envíos parciales). `PUT /aduanas/despachos/{id}` corrige uno (típicamente
para anotar la salida) y `DELETE` elimina uno registrado por error. No se registra un DUA en una factura anulada.

## Datos para el agente de aduanas

`GET /aduanas/facturas/{id}` (y `?formato=csv` para el fichero) reúne:

- **exportador**: razón social, NIF y EORI (en España, `ES` + NIF);
- **destinatario**: nombre, NIF, EORI (de su ficha) y dirección;
- país de destino, Incoterm y su lugar (de la carta de porte o, si no hay, del cliente);
- transporte, bultos y peso bruto, de las cartas de porte de los albaranes de los pedidos facturados en esa factura;
- **partidas** por código arancelario y país de origen (de la ficha del artículo): descripción, cantidad, peso neto y
  valor;
- los DUA ya registrados.

El peso neto es la cantidad si el artículo se vende por kilos, o la cantidad por su peso por unidad.

Los **avisos** dicen lo que falta: artículos sin código arancelario, destinatario fuera de la UE sin EORI, falta de
Incoterm o de carta de porte.

El CSV va separado por punto y coma, con coma decimal y BOM, para que Excel en español lo abra bien.

## Pendiente

- Intrastat (declaración estadística de las operaciones dentro de la UE).
- Certificados fitosanitarios de exportación por expedición.
- DUA de importación ligado a la factura del proveedor (el IVA a la importación ya tiene su tipo, `IMPORT21`).
