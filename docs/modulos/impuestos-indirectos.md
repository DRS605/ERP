# Impuestos indirectos: IGIC (Canarias) y prorrata

Parte de la base: está disponible en todas las ediciones, porque una empresa canaria o con ventas
exentas lo necesita para facturar y declarar. Rutas: `/impuestos/*` y `/empresas/actual/territorio-fiscal`.

## 1. Territorio fiscal e IGIC

Cada empresa tiene un **territorio fiscal**: **Comun** (Península y Baleares, IVA) o **Canarias**
(IGIC, Ley 20/1991). Se cambia en *Ajustes → Fiscalidad* o con
`PUT /empresas/actual/territorio-fiscal`. El territorio decide:

- **El impuesto de cada factura.** Se guarda en la propia factura (`impuesto`: `Iva` o `Igic`) y no cambia
  aunque luego cambie el territorio.
  - Una factura no mezcla IVA e IGIC: una empresa canaria no puede facturar con `IVA21` ni una
    peninsular con `IGIC7` (error `factura.impuesto_territorio`).
  - Una línea sin tipo lleva el general del territorio: `IVA21` o `IGIC7`.
  - El IGIC **no tiene recargo de equivalencia**. Se ignora aunque el cliente lo tenga marcado, y la base
    de datos lo impide.
  - La rectificativa lleva el impuesto de la factura que corrige.
- **El catálogo de tipos.** Al consultar el catálogo, a una empresa canaria se le siembran los tipos de IGIC:

  | Código | Tipo |
  |---|---|
  | `IGIC0` | Tipo cero |
  | `IGIC3` | Reducido |
  | `IGIC7` | General |
  | `IGIC95` | Incrementado (9,5 %) |
  | `IGIC15` | Incrementado (15 %) |
  | `IGIC20` | Especial incrementado (20 %) |
  | `IGICEXENTO` | Exento |
  | `IGICNOSUJ` | No sujeto |
  | `IGICISP` | Inversión del sujeto pasivo |
  | `IGICEXPORT` | Exportación |
  | `IGICIMP7` | Importación |

  La empresa puede ajustar los porcentajes. Los gastos, las facturas recibidas y los artículos aceptan
  también estos códigos.
- **Los documentos.** El PDF de la factura y del presupuesto rotula «IGIC», y el XML VeriFactu lleva
  `<Impuesto>03</Impuesto>` en cada desglose.
- **La autoliquidación.** El **303** cuenta solo el IVA, y el **modelo 420** (borrador, ante la Agencia
  Tributaria Canaria) el IGIC: devengado por tipo, soportado, deducible (con prorrata) y resultado.
  `GET /impuestos/modelo-420?anio&trimestre`.

**Pendiente:** el modelo 425 (resumen anual), el SII del IGIC y el IPSI de Ceuta y Melilla.

## 2. Prorrata

La aplica quien tiene a la vez ventas con derecho a deducir (sujetas, exportaciones, entregas
intracomunitarias, ISP…) y ventas **exentas sin derecho** (art. 20 LIVA). Se configura por ejercicio
en *Ajustes → Fiscalidad* o con `PUT /impuestos/prorrata/{ejercicio}`:

| Régimen | Qué se deduce |
|---|---|
| **General** | El porcentaje de todo el impuesto soportado. |
| **Especial** | Entero lo usado solo en operaciones con derecho; nada de lo usado solo en exentas; el porcentaje de lo común. Cada gasto indica su **afectación** (`Comun`, `ConDerecho`, `SinDerecho`). |

**Durante el año** se deduce con el **porcentaje provisional**, que suele ser el definitivo del año anterior.

**En el cuarto trimestre**, `GET /impuestos/prorrata?ejercicio` calcula:
- **El porcentaje definitivo:** ventas con derecho entre ventas con y sin derecho, **redondeado a la unidad
  superior** (art. 104.Dos). Las operaciones no sujetas no cuentan.
- **La regularización:** deducible con el definitivo menos lo deducido con el provisional. El 303 (o el 420)
  del cuarto trimestre la incluye.

**Avisos:**
- Si hay ventas exentas y no se ha configurado prorrata.
- Si con la general se deduce un **10 % o más** que con la especial. En ese caso la especial es
  obligatoria (art. 103.Dos.2º).

**En la contabilidad** (modo Completo), al contabilizar una compra solo va a la **472** la parte
deducible. La no deducible se suma a la cuenta de gasto, como manda el PGC.

**Pendiente:**
- El asiento de la regularización anual (634/639); hoy el importe se calcula pero el asiento se hace a mano.
- La regularización de bienes de inversión en 5 o 10 años (art. 107).

## 3. Compensaciones del REAGP

El catálogo incluye las compensaciones del régimen especial de la agricultura, ganadería y pesca:
`REAGP12` (agrícola y forestal, 12 %) y `REAGP105` (ganadera y pesquera, 10,5 %). Las usa la autofactura de
las liquidaciones al agricultor del [módulo agro](agro.md). Son IVA soportado deducible, y el 303 las
muestra aparte (casillas 42-43).

## 4. Correcciones de paso

- El 303 y el 130 contaban los **gastos anulados**, y ya no.
- El PDF redondeaba los porcentajes a enteros: 10,5 % salía como «11 %».
- El catálogo admitía códigos de tipo de hasta 15 caracteres, pero las facturas, los gastos y los
  artículos guardan 10. Un código largo rompía al facturar; ahora se rechaza al crearlo.
- Para encadenar VeriFactu se cargaban en memoria **todas** las facturas de la empresa en cada emisión;
  ahora se lee solo la última.
