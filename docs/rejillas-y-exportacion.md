# Rejillas: filtros, totales, agrupación y exportación

Todas las pantallas con listados tienen las mismas herramientas de consulta, sin programar nada por pantalla. El
objetivo es que cualquier lista sirva para contestar preguntas («¿cuánto me debe este cliente de lo vencido?»,
«¿qué cobros de marzo superan 1.000 €?») y para sacar el dato a Excel o a papel tal como se ve.

## Interfaz clásica (`wwwroot/index.html`)

La función `mejorarTablas()` actúa sobre cada tabla de la vista con cabecera y al menos dos filas. Se ejecuta al
pintar una pantalla y, además, un `MutationObserver` sobre `#view` la vuelve a lanzar cuando una pantalla pinta
sus tablas más tarde (listas paginadas, pestañas, consultas). No actúa dentro del módulo React de documentos (tiene
las suyas) ni en tablas anidadas.

| Herramienta | Qué hace |
|---|---|
| **Ordenar** | Clic en la cabecera: fechas, números y textos (sin acentos). Las filas de totales de la pantalla se quedan abajo. |
| **Buscar** | Filtra las filas que contienen el texto (sin acentos ni mayúsculas). |
| **Filtros** | Fila de filtros bajo la cabecera: texto → «contiene» (o lista de valores si hay ≤ 15 distintos y se repiten); números e importes → mínimo/máximo; fechas → desde/hasta. Se combinan con la búsqueda. El botón muestra cuántos hay activos y **Limpiar** los quita. |
| **Totales** | Pie con la suma de las filas visibles de cada columna de importes y el número de filas («Total · 23»); se recalcula con cada filtro. Precios, €/kg, porcentajes, factores y días se **promedian** (⌀). |
| **Selección** | Casillas (solo si hay importes): «3 seleccionadas · Total 1.234,56 € · Pendiente …». No abre la fila al marcarla. |
| **Agrupar por** | Columnas de texto con 2 a 50 valores distintos: cabecera de grupo con el valor, el número de filas y los subtotales; clic para plegar. Se recuerda por tabla. El pie sigue siendo el total general. |
| **Columnas** | Mostrar u ocultar columnas; se recuerda por tabla. |
| **Exportar** | Excel (.xlsx) o CSV con las columnas visibles y las filas filtradas, en el orden (y agrupación) de la pantalla. |
| **Imprimir** | Abre una ventana solo con la tabla: título, empresa, fecha, filtros aplicados, grupos con subtotales y totales. |

En tablas cortas (menos de 5 filas: rankings, resúmenes) solo se ponen el buscador, los totales, las columnas, la
exportación y la impresión.

### Cómo se detectan importes, fechas y códigos

- Una columna es numérica si al menos el 80 % de sus celdas no vacías tiene formato español (`1.234,56 €`,
  `-12,00`, `12,5 %`, `3 kg`, `0,85 €/kg`); es de fechas si empiezan por `dd/mm/aaaa`.
- **No se suman** (se tratan como texto): las columnas cuyo título es un identificador o una clasificación — Nº,
  Número, Código, NIF, Año, Ejercicio, Tipo, Cuenta, Subcuenta, Teléfono, IBAN, CP, Serie, Mes, Periodo, Asiento,
  Lote, SSCC… —, las de enteros sin alinear a la derecha (códigos) y los tipos impositivos (IVA %, IRPF %).
- El **saldo acumulado** de un extracto (columna «Saldo» junto a «Debe» y «Haber», como el libro mayor) no se suma.
- Si la pantalla ya pinta su fila de totales (libro mayor, balance, libro de IVA…), se respeta y queda fija abajo; el
  pie propio solo aparece al filtrar («Total filtrado»).
- Las tablas con secciones y subtotales intercalados (pérdidas y ganancias, balance de situación) no reciben
  totales, selección ni agrupación.

### Ajustes por tabla

Una pantalla puede afinar el comportamiento con atributos:

| Atributo | Efecto |
|---|---|
| `<table data-rejilla="no">` | Sin herramientas (también en todo lo que haya dentro de un elemento con ese atributo). |
| `<table data-totales="no">` | Sin pie de totales (p. ej. el libro diario, donde el asiento y sus apuntes se sumarían dos veces). |
| `<table data-seleccion="no">` / `data-agrupar="no"` | Sin casillas / sin agrupación. |
| `<table data-agrupar-por="Sentido">` | Agrupación propuesta mientras el usuario no elija otra (efectos de cartera: cobros y pagos por separado). |
| `<table data-pagina="1">` | El pie dice «Total página» (lo pone solo `listaServidor` cuando hay más de una página). |
| `<th data-suma>` / `<th data-media>` / `<th data-sin-total>` | Fuerza el cálculo de esa columna (artículos: precios de venta y compra en media, IVA sin total). |

## Exportación a Excel: `POST /exportar/xlsx`

Genera un `.xlsx` con lo que envía la interfaz. Es de la base (todas las ediciones) y solo exige sesión; no lee
datos de la empresa. Queda en la auditoría como «Exportación de datos a Excel».

```json
{
  "titulo": "Cobros · Cartera de cobro",
  "columnas": [
    { "titulo": "Factura", "tipo": "texto" },
    { "titulo": "Vencimiento", "tipo": "fecha" },
    { "titulo": "Pendiente", "tipo": "moneda" },
    { "titulo": "Dto.", "tipo": "porcentaje", "total": "media" }
  ],
  "filas": [["FA2026/000014", "31/12/2026", 1601.26, 5]],
  "totales": ["Total · 1"]
}
```

- `tipo`: `texto`, `numero`, `moneda` (`#,##0.00 €`), `fecha` (`dd/mm/aaaa`) o `porcentaje` (`0,00 %`; el valor va
  en tanto por ciento: 21 = 21 %). En columnas numéricas y de fecha también se aceptan textos con formato español
  («1.234,56 €», «31/12/2026») o ISO.
- `total` (opcional): `suma` (por defecto en números e importes), `media` o `no`.
- `totales` (opcional): si se envía, añade una fila «Total» con fórmulas `SUM`/`AVERAGE` (con su valor ya calculado);
  sus valores no nulos rotulan o fijan las columnas sin fórmula.
- El libro lleva cabecera en negrita con fondo, fila de cabecera inmovilizada, autofiltro, anchos según el contenido,
  título en las propiedades y la hoja con el título (≤ 31 caracteres, sin `[]:*?/\`). Los textos nunca se
  interpretan como fórmulas.
- Límites: 100 000 filas y 200 columnas por exportación (400 si se superan).

El escritor es la clase pública `AlxorCore.Api.Comun.ExcelXlsx`
(`ExcelXlsx.Generar(titulo, columnas, filas, filaTotales)`), sin dependencias externas, para cualquier otra
exportación del servidor.

## Listados de documentos (módulo React)

Presupuestos, pedidos de venta, facturas, pedidos de compra y facturas de proveedor:

- Columnas **Base, Impuestos, Total** y, en facturas y gastos, **Pendiente** (en rojo si está vencido).
- Filtros: texto, estado, fechas, cliente o proveedor, **serie** (facturas), importe mínimo y máximo y **estado de
  cobro/pago** (pendientes, vencidas, cobradas/pagadas).
- Orden por cualquier columna (en facturas y gastos lo hace el servidor sobre todo el resultado).
- Fila de totales sin anulados: en facturas y gastos son los de **todo el resultado filtrado**, no los de la página,
  con el pendiente y lo vencido.
- Exportación a Excel (con los totales) y a CSV de todo lo filtrado (en facturas y gastos se piden todas las
  páginas).

`GET /facturas/buscar` y `GET /gastos/buscar` admiten, además de los filtros de siempre, `serie` (facturas),
`cobro` (`pendiente`, `vencida`, `cobrada`/`pagada`), `orden` (`fecha`, `numero`, `cliente`/`proveedor`, `base`,
`impuestos`, `total`) y `desc`. La respuesta conserva los campos de la paginación y añade `totales`
(`documentos`, `baseImponible`, `impuestos`, `retenciones`, `total`, `pendiente`, `vencido`, `documentosVencidos`)
y `pendientes` (pendiente de cada documento de la página). El pendiente sale de Tesorería (total menos lo cobrado o
pagado) de los documentos vivos: sin anulados ni facturas rectificadas.
