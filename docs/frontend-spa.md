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
