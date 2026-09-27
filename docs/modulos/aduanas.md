# Aduanas: exportación, importación e Intrastat

Una factura de exportación (IVA `EXPORT`, exenta por el art. 21 LIVA, o `IGICEXPORT`) necesita la prueba de que la
mercancía salió de la UE: el **DUA de exportación**. Lo presenta el agente de aduanas con su programa. El ERP le da
los datos y guarda lo que devuelve la aduana.

## Pantalla «Ventas → Aduanas · Intrastat»: exportación

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

## DUA de importación

Una compra de fuera de la UE llega con una factura del proveedor **sin IVA español**. El IVA de la importación lo
liquida la aduana en el **DUA de importación**, y ese DUA es el justificante para deducirlo (art. 97 LIVA). El panel
**«Aduanas · importación»** lista los gastos no anulados que:

- son de un proveedor de fuera de la UE (según el país de su dirección);
- o llevan un IVA de importación (`IMPORT*`, `IGICIMP*`);
- o ya tienen algún DUA.

Cada gasto sale «Sin DUA» o «Con DUA», con la cuota de IVA y los aranceles de sus DUA. Un DUA (tabla
`gastos.dua_importacion`) guarda:

- el MRN (18 caracteres, único por empresa: 409 `dua.mrn_duplicado`);
- la fecha de admisión y la aduana;
- la base del IVA (valor en aduana + aranceles + gastos hasta el destino);
- los aranceles, la cuota y unas observaciones.

Un gasto puede tener varios DUA (envíos parciales). No se admiten DUA en un gasto anulado (409 `dua.gasto_anulado`).

API (permiso `gasto.gestionar`):
- `GET /aduanas/importaciones`
- `POST /aduanas/gastos/{id}/duas`
- `PUT/DELETE /aduanas/duas-importacion/{id}`

El DUA **no genera asiento**. El IVA de la importación se contabiliza con la factura del agente o del transitario, o
con un asiento manual a la 472, como hasta ahora. Aquí queda el justificante enlazado a la compra.

## Intrastat

La declaración estadística mensual del comercio de bienes con otros Estados miembros. Se ve en el panel
**«Intrastat»** (año, mes y flujo) y se descarga en CSV:

`GET /aduanas/intrastat?anio=&mes=&flujo=Expedicion|Introduccion[&formato=csv]`

**Expediciones.** Salen de las facturas ordinarias del mes con IVA intracomunitario (entrega exenta del art. 25). El
cliente debe estar en otro Estado miembro. Las líneas se agrupan por Estado miembro, provincia, Incoterm, naturaleza,
modo de transporte, código NC, país de origen y NIF-IVA del cliente. Cada campo sale de:

| Campo | De dónde sale |
|---|---|
| Código NC | Los 8 primeros dígitos del código TARIC del artículo |
| Masa neta | La cantidad si se vende por kg; si no, el peso por unidad × la cantidad |
| Incoterm | El de la carta de porte de la factura o, si no la tiene, el del cliente |
| Modo de transporte | El de la carta de porte (1 marítimo, 2 ferrocarril, 3 carretera, 4 aéreo); por defecto, 3 |
| Naturaleza de la transacción | 11 (compraventa en firme) |
| Régimen estadístico | 1 |
| Provincia | Los dos primeros dígitos del código postal de la empresa |

Las facturas rectificativas no se restan: sale un aviso para regularizarlas a mano.

**Introducciones.** Salen de los albaranes de compra del mes (no anulados) de proveedores de otro Estado miembro. Se
valoran al precio del pedido. El modo de transporte es la carretera y no llevan NIF de la contraparte.

**Umbral.** La respuesta trae el acumulado del flujo en el año y avisa si supera los 400.000 €, el umbral de
obligación.

**Avisos.** Dicen lo que falta: artículos sin código arancelario o sin peso, clientes sin NIF-IVA, empresa sin
código postal y rectificativas.

**Limitaciones:**
- El CSV no sigue aún el formato de fichero de la AEAT. Lleva las columnas de la declaración en ese orden, para
  cargarlas o copiarlas en la presentación.
- El valor estadístico es el importe facturado.
- Las unidades suplementarias van en blanco.

## Certificados fitosanitarios

Cada carta de porte (la expedición) puede llevar certificados fitosanitarios: de exportación, de reexportación o
pasaporte fitosanitario UE. Se imprimen en la casilla 5 del CMR y salen en los datos para el agente de aduanas. Ver
[carta de porte](../carta-de-porte.md#certificados-fitosanitarios).
