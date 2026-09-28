# Análisis de datos

Diseñador de informes al estilo de las tablas dinámicas: se elige un **conjunto de datos**, las **dimensiones** por
las que agrupar (en filas, con subtotales por nivel, y opcionalmente una en columnas), las **medidas** que calcular,
el **periodo** (relativo: «este año», «últimos 12 meses»…), la **comparación** con el año o el periodo anterior,
**filtros** por dimensión y por cifra, el orden y los **N primeros** (el resto se agrupa en «Resto»). El resultado se
recalcula al cambiar cualquier cosa, con KPI del total (y su variación), gráfico y tabla.

Menú: **Informes → Análisis de datos**. Módulos: `src/AlxorCore.Analisis` (catálogo, motor, informes guardados) y
`src/AlxorCore.Analisis.Infraestructura` (ejecución en PostgreSQL y persistencia, esquema `analisis`). La pantalla es
un módulo React (`frontend/src/analisis`, compilado con `npx vite build -c vite.analisis.config.ts` en
`wwwroot/app-analisis/analisis.js`) que la interfaz clásica monta en su vista (`irAnalisis`).

## Conjuntos de datos

| Clave | Qué es | Módulo | Permiso |
|---|---|---|---|
| `ventas` | Líneas de facturas emitidas (sin anuladas ni sustituidas): cantidad, kilos, base, IVA, coste, margen, margen %, precio medio, €/kg | base | `factura.leer` |
| `compras` | Líneas de facturas de proveedor (las rectificativas restan): base, cuota, deducible, no deducible, recargo, coste | base | `gasto.leer` |
| `pedidos` | Cartera de pedidos de venta: pedido, servido, facturado, pendiente, nivel de servicio | ventas | `factura.leer` |
| `tesoreria` | Cobros y pagos (anulaciones y devoluciones restan) por banco, forma y tercero | base | `factura.leer` |
| `deuda` | Facturas con cobrado y pendiente a hoy, situación y antigüedad (aging), días de retraso | base | `factura.leer` |
| `contabilidad` | Apuntes: debe, haber, saldo por cuenta, cuenta de 3 dígitos, subgrupo, grupo del PGC, diario, origen | contabilidad | `contabilidad.leer` |
| `almacen` | Movimientos de almacén: entradas, salidas, variación de stock y de valor | inventario | `inventario.leer` |
| `agro` | Entradas de fruta confirmadas: kilos, envases, importe estimado por agricultor, producto, calibre, campaña | agro | `agro.leer` |

Todos tienen además las dimensiones de fecha: año, trimestre, mes, mes sin año, semana, día y día de la semana.

## Cómo funciona

- **Seguridad.** Todo el SQL está fijo en `CatalogoDatasets` (C#): el usuario solo elige claves y da valores, que van
  siempre como parámetros. Las consultas se ejecutan en una transacción de **solo lectura** con tiempo máximo (60 s)
  por la conexión del contexto, así que la **RLS** de cada tabla limita los datos a la empresa (y al grupo, en los
  maestros). Los conjuntos se ven solo con el módulo contratado y el permiso de lectura de lo que contienen.
- **Subtotales.** Un único `GROUP BY GROUPING SETS` devuelve cada nivel de las filas (y cada uno con y sin la
  columna, en la tabla dinámica); las medidas no sumables (porcentajes, medias, recuentos distintos) se **recalculan**
  en cada subtotal, nunca se suman.
- **Comparación.** Se repite la consulta con el periodo anterior; las dimensiones de fecha se calculan sobre la fecha
  desplazada, así «marzo 2026» se compara con «marzo 2025».
- **Filtros de cifra** (p. ej. clientes con más de 10.000 € de base): se quedan los registros cuyo grupo del último
  nivel cumple la condición, y los subtotales y el total son los de lo que se ve.
- **N primeros**: en el primer nivel; lo demás va a una fila «Resto (n)» (en las medidas sumables).
- **Límites**: 4 dimensiones en filas, 12 medidas, 60 valores en columnas, 20.000 grupos.

## En la pantalla

- **Galería**: mis informes (propios y compartidos), informes listos para usar (`PlantillasAnalisis`: ventas por
  cliente y mes, evolución y margen, margen por familia y artículo, top artículos por kilos, ventas por país, gastos
  por cuenta, compras por proveedor y mes, deuda por antigüedad, cobros y pagos por banco, cartera de pedidos, saldos
  por grupo contable, gastos e ingresos por mes, movimientos de almacén, entradas de fruta por agricultor y calibre) y
  «Nuevo análisis» por conjunto.
- **Fila**: al pulsarla, *Ver los registros* (el detalle, con enlace al documento), *Filtrar: solo esto*, *Excluir* y
  *Desglosar por…* otra dimensión. En la tabla dinámica, pulsar una celda abre sus registros. Los niveles se pliegan.
- **Gráfico**: líneas si el eje es el tiempo (el periodo anterior en gris discontinuo), barras horizontales si no
  (apiladas con la tabla dinámica; siete series y el resto en «Otros»). Se dibuja la medida del KPI elegido.
- **Exportar**: Excel (`POST /exportar/xlsx`, con las filas de detalle y la fila de totales del propio Excel), CSV
  (con los subtotales) e imprimir.
- **Guardar**: con nombre, compartido con la empresa y favorito. Se guarda el periodo relativo, así el informe está
  siempre al día. Solo el autor cambia o borra un informe; los demás pueden guardarlo como uno nuevo.

## API

| Método | Ruta | Qué hace |
|---|---|---|
| GET | `/analisis/catalogo` | Conjuntos visibles (dimensiones, medidas) y plantillas |
| POST | `/analisis/consulta` | Ejecuta un análisis (`ConsultaAnalisis`) |
| GET | `/analisis/{dataset}/valores?dimension=&texto=&desde=&hasta=` | Valores de una dimensión para filtrar |
| POST | `/analisis/detalle` | Registros de una cifra (máx. 5.000) con el id del documento |
| GET/POST | `/analisis/informes` | Informes guardados / guardar |
| PUT/DELETE | `/analisis/informes/{id}` | Cambiar / borrar (solo el autor) |

Ejemplo de consulta:

```json
{
  "dataset": "ventas", "filas": ["familia", "articulo"], "columna": "mes", "medidas": ["base", "margen_pct"],
  "desde": "2026-01-01", "hasta": "2026-12-31", "comparar": "anio_anterior",
  "filtros": [{ "dimension": "pais", "operador": "en", "valores": ["ES", "FR"] }],
  "filtrosMedida": [{ "medida": "base", "operador": "mayor", "valor": 1000 }],
  "ordenarPor": "base", "limite": 20
}
```

Operadores de filtro: `en`, `no_en` (un valor vacío es «sin valor»), `contiene`, `empieza`, `desde`, `hasta`
(comparación de texto, vale para fechas y periodos como `2026-03`), `vacio`, `no_vacio`.

## Añadir un conjunto o una medida

Se añade en `CatalogoDatasets` (SQL fijo: `Desde` con las uniones, `CondicionBase`, `ColumnaFecha`, dimensiones como
expresiones de texto y medidas como agregaciones) y, si hace falta, su permiso en `PermisosAnalisisHttp`. La prueba
`AnalisisTests.Todos_los_conjuntos_y_dimensiones_se_pueden_consultar` ejecuta cada dimensión con todas las medidas y
cada plantilla, así un error de SQL no llega a producción.
