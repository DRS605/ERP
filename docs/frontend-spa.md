# Interfaz web (SPA React + Vite + TypeScript)

La interfaz de ALXOR Core está migrando de un único `index.html` de JavaScript a mano a una
**SPA real** con framework, build y tests. Esta primera entrega establece **toda la fundación** y un
**slice vertical probado** (login → inicio → clientes → facturas); el resto de pantallas se migran
después siguiendo el mismo patrón. La interfaz clásica sigue funcionando en la raíz durante la
transición.

## Dónde vive

- **Código fuente**: `frontend/` (proyecto Vite). No se toca desde el backend.
- **Build**: `npm run build` compila y emite a `src/AlxorCore.Api/wwwroot/app/`, que el host .NET
  sirve como estáticos. Se **versiona el resultado del build** para que el despliegue de .NET no
  dependa de Node (el build de Node solo hace falta al desarrollar/compilar la SPA).
- **Rutas**: la SPA se sirve bajo **`/app`** (con enrutado en el cliente y *fallback* a
  `app/index.html`); la interfaz clásica permanece en `/`. Un enlace en cada una lleva a la otra.

## Puesta en marcha (desarrollo)

```bash
cd frontend
npm install
npm run dev        # servidor de desarrollo con proxy a la API en http://localhost:5080
npm run test       # pruebas (Vitest)
npm run typecheck  # comprobación de tipos (tsc)
npm run build      # build de producción → wwwroot/app
```

En producción se sirve el build ya compilado; basta con `dotnet run` en la API.

## Arquitectura de la SPA

- **React 18 + TypeScript en modo estricto** (`noUnusedLocals`, `noUnusedParameters`, etc.).
- **Enrutado**: `react-router-dom` con `basename="/app"`; rutas protegidas que redirigen a `/login`
  si no hay sesión.
- **Cliente API tipado** (`src/lib/api.ts`): añade el token Bearer, serializa el cuerpo y normaliza
  los errores como `ApiError` (con `status` y el `title` del *problem details*). Se construye con
  `fetch` y el proveedor de token **inyectables**, para poder probarlo sin red.
- **Sesión** (`src/lib/sesion.ts` + `auth.tsx`): login → selección de empresa → token con empresa
  activa, persistido en `localStorage`. El cambio de empresa reemite el token (aislamiento
  multiempresa, igual que en la interfaz clásica).
- **Componentes reutilizables**: `Shell` (barra lateral + cabecera + tema + salir), `Modal`,
  `DataTable`, y un sistema de avisos (`toast`). Todo con los **tokens de diseño** de la marca
  (paleta cian/azul marino) en `src/styles/tokens.css`, incluido el tema oscuro.
- **Formato** (`src/lib/format.ts`): euros y fechas en español, deterministas (sin depender de los
  datos de ICU del entorno).

## Pantallas migradas en esta entrega

- **Login** con selección de empresa.
- **Inicio**: KPIs de facturación.
- **Clientes**: listado + alta/edición.
- **Facturas**: listado + detalle (con enlaces a PDF y Facturae).

## Pruebas

`Vitest` + `@testing-library/react` (entorno `jsdom`):

- **`format`**: euros con separador de miles y decimal, fechas ISO→DD/MM/AAAA, cantidades.
- **`api`**: añade el Bearer, serializa el cuerpo, no añade cabecera sin token, lanza `ApiError` con
  `status`, y no parsea en `204`.
- **`DataTable`**: estado vacío, render de filas y clic de fila.

## Plan de migración del resto

Cada pantalla clásica se reimplementa como una página React que reutiliza el cliente API, el
`DataTable`, el `Modal` y los tokens ya existentes; no hace falta tocar el backend. Orden sugerido
por tráfico: productos, TPV/tickets, gastos y proveedores, tesorería (cobros/pagos), informes y
análisis de gestión, contabilidad e inmovilizado, y las pantallas de administración (series,
usuarios, divisas, aprobaciones, integraciones, ajustes). Cuando la SPA alcance paridad, pasará a
servirse en la raíz y la interfaz clásica se retirará.

## Módulo de documentos (dentro de la interfaz clásica)

Los documentos de venta y compra (presupuestos, pedidos de venta, facturas y pedidos de compra) son un módulo React
(`frontend/src/documentos/`). Se compila como librería con `npm run build:docs`, que genera
`wwwroot/app-docs/documentos.js`. La interfaz clásica lo carga al abrir esas pantallas y lo monta en su vista con
`montar(elemento, { token, aviso, irA }, ruta)`, así que comparte el menú, la sesión, los avisos y los estilos.

- **Listados**:
  - búsqueda por número, cliente o NIF, y filtros por estado, fechas, cliente o proveedor, serie, importe y estado de
    cobro/pago;
  - columnas de base, impuestos, total y pendiente, orden por columna y exportación a Excel/CSV;
  - totales sin anulados; en facturas y gastos, paginados en el servidor y con los totales de todo el filtro
    (ver [`rejillas-y-exportacion.md`](rejillas-y-exportacion.md)).
- **Editor**:
  - Cabecera con el cliente o proveedor (buscador por nombre o NIF) y su ficha: tarifa, recargo, límite de riesgo y
    aviso de riesgo.
  - Rejilla de líneas:
    - buscador de artículos por referencia o nombre, con precio, unidad y stock;
    - cantidad, precio, descuento e impuesto;
    - importe, margen (ventas) o coste de entrada en almacén (compras);
    - conceptos de cada línea, y aparte los del documento.
  - Teclado: Intro pasa a la siguiente casilla (y en la última, a una línea nueva), ↑↓ cambian de línea y F2 abre el
    buscador.
  - Mientras se edita, el documento se calcula en el servidor sin guardarlo (`POST /facturas/simular`,
    `POST /compras/pedidos/simular`): precio de tarifa, conceptos, impuestos por tipo, recargo, IRPF, margen y riesgo.
    Lo que se ve es lo que se emite; al guardar se fijan esos precios.
- **Vista de cada documento con sus acciones**:
  - factura: PDF, Facturae, duplicar, rectificar (en el mismo editor), anular con motivo y estado de cobro;
  - presupuesto: PDF, editar, pasar a pedido, aceptar y facturar, rechazar;
  - pedido de venta: confirmar, entrega parcial con albarán, facturar, cancelar, anular albarán;
  - pedido de compra: confirmar, recepción por almacén con lote, facturar (gasto), cancelar, anular albarán;
  - enlaces entre documentos: presupuesto → pedido → factura.

## Orden de las pantallas: secciones en el menú y pestañas

Para no amontonar funciones en una misma pantalla:

- **Menú por secciones.** En `NAV`, un texto suelto entre las vistas de un grupo es un rótulo de sección. Solo se pinta
  si debajo hay alguna vista contratada. Así están ahora los grupos:
  - Ventas: Comercial / Venta en comisión / Facturación / Posventa / Informes.
  - Compras: Compras / Facturas y cargos / Informes. Artículos y almacén: Artículos / Almacén / Informes.
  - Logística: Operativa / Transporte y exportación / Maestros logísticos.
  - Agro: Entrada de fruta / Planta y confección / Expedición / Liquidación / Campo y calidad / Campaña / Maestros agro.
  - Cooperativa (módulo aparte): Socios y capital / Libros / Configuración.
  - Subasta (módulo aparte, sobre agro): Subasta (sesiones y ventas por comprador y agricultor).
  - Tesorería: Cobros / Impagados / Pagos / Bancos / Divisas / Informes.
  - Contabilidad: Contabilidad / Impuestos / Analítica. Informes: Centro de informes / Análisis. Configuración: Empresa /
    Usuarios y datos / Integraciones.
- **Entradas que abren una pestaña.** Una entrada del menú puede ser `"vista~Título de la pestaña"`: `ir()` abre la
  vista y deja la pestaña pedida en `window._pestanaPedida`, que `pestanas()` activa (y recuerda) la primera vez que
  encuentra ese título. Así cada área de una pantalla con pestañas tiene su propio punto de menú (p. ej.
  «Contabilidad → Estados financieros», «Agro → Calibrados», «Tesorería → Entregas a cuenta»). La pestaña pedida se
  muestra aunque esté vacía, con un aviso. `MODULO_VISTA` admite la clave completa (`"centroinformes~Cooperativa"`)
  para ocultar una entrada concreta sin el módulo; si no, manda el de la vista.
- **Pestañas dentro de la pantalla.** `pestanas(clave, [[título, html], …])` reparte en pestañas las áreas de una
  pantalla. Las áreas vacías no se muestran, y si solo queda una se pinta sin pestañas. La pestaña elegida se recuerda
  por pantalla en `localStorage`, con la clave `alxor.pestana.<clave>`.

Pantallas con pestañas:

| Pantalla | Pestañas |
|---|---|
| Ajustes | empresa y plan · numeración · cobros y riesgo · inventario · usuarios y roles · datos e importación · actividad |
| Maestros agro | campañas y agricultores · precios y liquidación · confección · calidad y certificación · envases y taras · palés e inventario |
| Partidas y palés | partidas · palés · plantillas de palé |
| Contabilidad | pendientes · diario y saldos · periodos y diarios · estados financieros · cierre y cuentas anuales |
| Analítica: maestros | centros y partidas · reparto y reglas · periodos |
| Consolidación | eliminaciones · inversión y patrimonio · perímetro y cuentas |
| Análisis de gestión | evolución · ventas · compras · cobros pendientes · existencias · extracto de tercero |
| Logística: maestros | fichas · soportes · plantillas de paletizado · numeración SSCC |
| Integraciones | API · webhooks · EDI |
| Reclamaciones | reclamaciones · informe · conceptos |
| Liquidaciones de pagos | pendiente de pagar · entregas a cuenta · liquidaciones emitidas |
| Divisas | tipos de cambio · conversor · diferencias de cambio |
| Impagados | facturas vencidas · impagados y dudosos · renovaciones |
| Centro de informes | favoritos · ventas · compras · almacén · agro · entradas · agro · producción · envases · tesorería · contabilidad e impuestos · cooperativa |

## Centro de informes

**Informes → Centro de informes** (y un acceso «Informes de …» al final de cada grupo del menú) reúne los informes del
ERP por carpetas. `CI_INFORMES` en `index.html` es el catálogo: `[carpeta, acción, nombre, descripción]`, donde la acción
es `ir:vista~Pestaña` (una pantalla o una pestaña suya) o `an:clave` (una plantilla de `PlantillasAnalisis`, que se abre
en el análisis de datos con `irAnalisis({plantilla})`). Solo se ven los informes de las pantallas contratadas y las
plantillas que `/analisis/catalogo` ofrece a la empresa y al usuario; las carpetas vacías no se pintan. Hay un buscador
que mira en todas las carpetas (nombre, descripción y carpeta, sin acentos) y **favoritos** (☆), que se guardan en el
navegador (`alxor.informes.favoritos`). Para añadir un informe: una línea en `CI_INFORMES` (y, si es una plantilla
nueva, su definición en `PlantillasAnalisis`, que `AnalisisTests` ejecuta).

