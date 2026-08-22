# Importación de arranque por Excel (.xlsx)

Carga rápida de datos iniciales al implantar ALXOR Core (o migrar desde otro programa), leyendo
ficheros **.xlsx** sin dependencias externas (lector propio basado en ZIP + XML, `LectorExcel`). Cada
importación admite **previsualizar** (valida y cuenta sin aplicar) antes de guardar.

## Qué se importa

| Tipo | Endpoint | Columnas | Efecto |
|---|---|---|---|
| **Saldos** | `POST /importar/saldos` | `cuenta`, `debe`, `haber` | Crea el **asiento de apertura** (1 de enero del ejercicio). Debe cuadrar (Σ debe = Σ haber). |
| **Cartera** | `POST /importar/cartera` | `sentido` (cobro/pago), `concepto`, `importe`, `fecha` | Crea **previsiones** de tesorería (ingreso/gasto) para poblar la previsión y el aging. |
| **Stock** | `POST /importar/stock` | `referencia`, `cantidad` | **Ajusta** las existencias del artículo (por referencia) al valor contado. |

Cuerpo: `{ contenidoBase64, previsualizar }`. Respuesta: `{ total, correctas, errores, aplicado,
mensajes[] }`.

## Decisiones de diseño

- La **cartera** no re-crea facturas fiscales (eso rompería la numeración/VeriFactu): se modela como
  previsiones de tesorería, suficiente para arrancar la previsión y el seguimiento de vencimientos.
- El **stock** se fija con un movimiento de tipo *Ajuste* (recuento), no una entrada, para reflejar
  las existencias iniciales exactas. Solo aplica a artículos con control de stock.
- Fechas: se aceptan como número de serie de Excel o como texto (`yyyy-MM-dd`, `dd/MM/yyyy`).
- La cabecera se normaliza (minúsculas, sin acentos) y admite alias por columna.

## UI

**Ajustes → Importación de arranque (Excel)**: un botón por tipo; se elige el `.xlsx`, se
previsualiza (resumen + avisos) y se confirma antes de aplicar.

## Tests

- **Integración** (`ImportacionExcelEndpointsTests`): saldos como asiento de apertura (y rechazo si
  descuadra); cartera como previsiones de ingreso/gasto; existencias iniciales ajustando el stock.
  Se construye un `.xlsx` mínimo (celdas inline) en la propia prueba.
