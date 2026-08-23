# Tipos de IVA configurables por empresa

Cada empresa gestiona su propio **catálogo de tipos de IVA** desde una pantalla propia
(**Ajustes → Tipos de IVA**). El catálogo cubre todas las casuísticas habituales de la
facturación española: tipos ordinarios (21/10/4 %), operaciones **exentas**, **no sujetas**,
con **inversión del sujeto pasivo (ISP)**, de **importación** y **intracomunitarias**.

## Modelo

- `TipoIva` (`catalogo.tipo_iva`) es un **maestro por empresa**: filtro y **RLS por empresa**
  (`app.empresa_actual`). El código es único dentro de la empresa (índice único
  `(empresa_id, codigo)`).
- Campos: `Codigo` (estable, en mayúsculas — p. ej. `IVA21`, `ISP`, `EXENTO`), `Nombre`,
  `Porcentaje`, `RecargoEquivalencia`, `Clase`, `MencionFactura` (mención legal a imprimir) y
  `Activo`.
- La **clase** (`ClaseIva`) determina el comportamiento fiscal:

  | Clase | Repercute IVA | Uso típico |
  |---|---|---|
  | `Ordinario` | Sí | 21 %, 10 %, 4 % |
  | `Exento` | No | Operación exenta (art. 20 LIVA, etc.) |
  | `NoSujeto` | No | Operación no sujeta a IVA (art. 7) |
  | `InversionSujetoPasivo` | No | ISP (art. 84.Uno.2º LIVA) |
  | `Importacion` | Sí | IVA de la importación |
  | `Intracomunitario` | No | Entregas/adquisiciones intracomunitarias (art. 25) |
  | `Viajeros` | No | Régimen especial de viajeros (art. 21.2º) |
  | `BienesUsados` | No | REBU — bienes usados/arte/antigüedades (art. 135) |
  | `AgenciasViajes` | No | Régimen especial de agencias de viajes (art. 141) |
  | `OroInversion` | No | Oro de inversión exento (art. 140 bis) |
  | `CriterioCaja` | Sí | RECC — devengo al cobro (art. 163 decies) |

### Regímenes especiales (alcance actual)

Los regímenes especiales están modelados como **clases con su mención legal**, de forma que la
factura ya sale correcta a efectos de repercusión y menciones. Su **mecánica completa** llega con
el bloque fiscal:

- **Viajeros**: se modela la exención y la mención; el documento **DIVA**, la validación de viajero
  **no residente en la UE** y la **devolución** del IVA se implementarán después.
- **Bienes usados (REBU)** y **agencias de viajes**: se modelan como operación sin cuota desglosada
  con su mención; el **cálculo sobre el margen** llega con el bloque fiscal.
- **Criterio de caja (RECC)**: repercute IVA al tipo ordinario con su mención; el **diferimiento del
  devengo al cobro** (libros y modelo 303) se tratará en el bloque fiscal.

- **Invariante**: una clase que no repercute (`Exento`, `NoSujeto`, `InversionSujetoPasivo`,
  `Intracomunitario`) **no admite porcentaje > 0**; su `PorcentajeRepercutido` es siempre 0.
  `Ordinario` e `Importacion` sí repercuten el porcentaje configurado.

## Siembra automática

La primera consulta del catálogo de una empresa (`GET /tipos-iva`) **siembra** el conjunto
predeterminado si está vacío: `IVA21`, `IVA10`, `IVA4`, `IVA0` (Exento), `NOSUJETO`, `ISP`,
`INTRA`, `IMPORT21` y los regímenes especiales `VIAJEROS`, `REBU`, `AGENCIAS`, `ORO` y `CAJA21`.
A partir de ahí la empresa puede editar, desactivar o añadir tipos propios. No se duplica en
consultas posteriores.

## API

- `GET /tipos-iva` — lista (y siembra si procede). Requiere autenticación.
- `POST /tipos-iva` — alta de un tipo. Requiere permiso `empresa.ajustes`.
- `PUT /tipos-iva/{id}` — edición. Requiere permiso `empresa.ajustes`.

El `Clase` se acepta como cadena (`"Exento"`, `"InversionSujetoPasivo"`, …). Un código
duplicado devuelve `409`; una clase sin repercusión con porcentaje devuelve `400`.

## Efecto en la emisión

Al emitir una **factura**, un **ticket** (factura simplificada) o una **rectificativa**, la
resolución de líneas consulta primero el catálogo de la empresa por el código de IVA de la
línea:

1. Si el código existe en el catálogo de la empresa, se usa su `PorcentajeRepercutido`
   (**0** en las clases sin repercusión → **cuota 0**, base íntegra) y, si procede, su recargo
   de equivalencia.
2. Si no está configurado, se recurre al **catálogo estatal** estático (compatibilidad hacia
   atrás: gastos, recepción, contabilidad e informes siguen funcionando sin cambios).

Además, las **menciones legales** de los tipos usados (exención, ISP, no sujeto,
intracomunitario…) se reúnen y se **estampan en la factura** (`MencionFiscal`), se muestran en
el detalle del SPA y se imprimen en el **PDF** (A4 y ticket 80 mm), cumpliendo la obligación de
indicar la mención correspondiente cuando la operación no repercute IVA.

## Nota sobre VeriFactu / modelos fiscales

La **propagación completa** de estas clases a las casillas de los modelos 303/390 y a las
categorías fiscales de Facturae/VeriFactu se activará junto con la puesta en marcha fiscal
(VeriFactu). El catálogo, la pantalla y el cálculo de la cuota (cuota 0 en exención/ISP/no
sujeto/intracomunitario) ya están operativos y son la base sobre la que se conectará esa
propagación sin rehacer el modelo.
