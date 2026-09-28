# Módulo Gastos

Registro de **gastos** (facturas recibidas simplificadas). Cada gasto puede enlazarse a un **proveedor** (entidad del módulo Terceros): se guarda su id y una copia de su nombre. También admite proveedor como texto libre para altas rápidas.

## Facturas de proveedor

Un gasto es una **factura recibida completa**:

- **Proveedor**: de la ficha, o texto libre en altas rápidas.
- **Número y fecha de la factura del proveedor** (`NumeroFactura`, `FechaFactura`). La **fecha de registro** (`Fecha`)
  es la del asiento y la del periodo de IVA en que se deduce; la de la factura no puede ser posterior.
- **Líneas** (`gastos.linea_gasto`). Cada una lleva:
  - descripción y **cuenta de gasto** propia (si no, la de las reglas de contabilización);
  - **base** y **tipo de impuesto**, del catálogo de la empresa;
  - **% deducible**, por ejemplo el 50 % de un turismo;
  - **recargo de equivalencia**, si el proveedor lo cobra.
- Según la clase del tipo:
  - **Ordinario**: cuota = base × tipo.
  - **Exento, no sujeto e importación**: sin cuota. El IVA de una importación va en el DUA.
  - **Inversión del sujeto pasivo e intracomunitaria**: la cuota **se autoliquida** al tipo indicado (por defecto el
    general). Se deduce por la 472 y se devenga por la 477, y el proveedor no la cobra.
- **Retención de IRPF** sobre la base total.
- **Vencimientos** (`gastos.vencimiento_gasto`): los de la forma de pago o los indicados, que tienen que sumar el
  total.
- Totales de cabecera:
  - base = Σ bases;
  - cuota = Σ cuotas;
  - total = base + cuotas cobradas por el proveedor + recargo − retención;
  - `CodigoIva` es el de la línea de mayor base.
- **Duplicados**: una factura viva de un proveedor no se registra dos veces. Mismo número (sin distinguir
  mayúsculas) en el mismo año da 409 `gasto.factura_duplicada`. Lo garantiza también el índice único
  `ux_gasto_factura_proveedor`.
- **Corrección** (`PUT /gastos/{id}`): solo sin pagos y si el gasto no nace de otro documento. Se encola el
  contraasiento de la versión anterior y el asiento de la nueva. `Revision` numera las versiones, para que cada una
  tenga su documento contable.
- **Asiento**, con líneas cuando hay varias o alguna especial:
  - Debe: cada cuenta de gasto (base + parte no deducible + recargo) y la 472 por lo deducible tras la prorrata.
  - Haber: la 477 por lo autoliquidado, la 4751 por la retención y la 400/subcuenta por el total.
- **Informes** (`GastoDto.DesgloseIva`, un desglose por tipo):
  - libro de IVA soportado: una fila por tipo, con el nº del proveedor;
  - 303 y 390: deducible = lo deducible de cada línea antes de la prorrata; devengado incluye lo autoliquidado;
  - SII de recibidas: número real, un detalle por tipo y bloque `InversionSujetoPasivo`;
  - 420, prorrata y REAGP.
- Los gastos anteriores pasan con una línea (la de la cabecera) y un vencimiento por el total.

Pantalla: **Compras → Facturas de proveedor** (módulo de documentos):

- **Listado**: búsqueda por número, proveedor o concepto; filtros por estado, fechas, proveedor, importe y estado de
  pago; columnas de base, impuestos, total y pendiente; totales de todo el filtro y exportación a Excel/CSV.
- **Editor**:
  - cabecera con el proveedor, el nº y las fechas, la forma de pago, el IRPF y la prorrata especial;
  - líneas con la cuenta, la base, el impuesto, el % autoliquidado y el % deducible;
  - vencimientos en 2, 3 o 4 plazos o a mano;
  - totales por tipo calculados en el servidor (`POST /gastos/simular`).
- **Vista**: líneas, deducible, vencimientos, estado de pago, corregir, duplicar, pagar y anular.

El buzón de facturas y la facturación de pedidos de compra pasan el número y la fecha de la factura del proveedor.

### Rectificativas y abonos del proveedor

Un abono (devolución, descuento posterior, error de precio) se registra como **rectificativa**: `RectificaGastoId` (la
factura rectificada, del mismo proveedor y no anulada; se copian su número y fecha) o, si no está en el sistema,
`NumeroRectificado` + `FechaRectificada`, y siempre `MotivoRectificacion`. Solo una rectificativa admite bases negativas.
El asiento sale invertido (400 al debe; gasto y 472 al haber: los importes negativos se pasan al otro lado y la prorrata
se aplica con el signo), el libro de IVA y el 303 restan, y el SII la declara como `R1` por diferencias
(`TipoRectificativa` = `I`) con `FacturasRectificadas`. En la pantalla: botón «Rectificativa / abono» en la factura.

### Régimen de recargo de equivalencia

Si la empresa está en `RegimenIva.RecargoEquivalencia` (comerciante minorista), el IVA y el recargo soportados no se
deducen: cada línea se registra con `PorcentajeDeducible` = 0 (salvo las autoliquidadas) y todo va a la cuenta de gasto.
El editor marca por defecto «El proveedor me cobra recargo de equivalencia».

## API

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/gastos` | permiso `gasto.leer` | Lista de gastos. |
| `GET` | `/gastos/buscar` | permiso `gasto.leer` | Búsqueda paginada: `texto` (concepto, proveedor o nº de factura), `estado`, `desde`/`hasta`, `importeMin`/`importeMax`, `proveedorId`, `cobro` (`pendiente`/`vencida`/`pagada`), `orden` (`fecha`/`numero`/`proveedor`/`base`/`impuestos`/`total`) y `desc`, `pagina`, `tamanoPagina`. Devuelve también `totales` de todo el filtro (sin anulados; con pendiente y vencido) y `pendientes` de los gastos de la página ([rejillas](../rejillas-y-exportacion.md)). |
| `GET` | `/gastos/{id}` | permiso `gasto.leer` | Obtiene un gasto con sus líneas, vencimientos y desglose. |
| `POST` | `/gastos` | permiso `gasto.gestionar` | Registra una factura recibida (`numeroFactura`, `fechaFactura`, `fecha`, `lineas[]`, `vencimientos[]`, `recargoEquivalencia`, `porcentajeIrpf`, `formaPagoId`…). Sin `lineas`, una con `baseImponible` y `codigoIva`. **201** |
| `POST` | `/gastos/simular` | permiso `gasto.leer` | Calcula la factura sin guardarla. |
| `PUT` | `/gastos/{id}` | permiso `gasto.gestionar` | Corrige una factura sin pagos (contraasiento + asiento nuevo). 409 `gasto.con_pagos`. |
| `POST` | `/gastos/{id}/anular` | permiso `gasto.gestionar` | Anula (contraasiento y fuera de libros). |

## Persistencia

Esquema **`gastos`**, tabla `gasto` (RLS por empresa). Repositorio con escritura
(`IRepositorioGastos`) y consultas (`IConsultaGastos`), que usarán Tesorería e Informes.

## Tests

- **Unitarios**: cálculo de IVA soportado y retención, validaciones, anulación.
- **Integración**: registrar/listar/obtener y aislamiento por empresa.
