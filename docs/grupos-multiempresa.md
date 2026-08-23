# Grupos (holding), maestros compartidos y actividades de negocio

Refuerza el enfoque multiempresa: por encima de la empresa se introduce el **grupo (holding)**, y los
**datos maestros** pasan a compartirse dentro del grupo, de modo que **un cliente o proveedor creado
una vez vale para todas las empresas del mismo grupo**. Los documentos fiscales (facturas, cobros,
gastos, contabilidad) siguen siendo **por empresa**, como exige la fiscalidad.

- **Fase 1** (hecha): introduce el grupo y comparte los maestros de **Terceros** (clientes y
  proveedores).
- **Fase 2** (hecha): **actividades de negocio** (clasificación transversal) y **visibilidad por
  usuario y pantalla** (ver más abajo).
- **Fase 3** (hecha): **catálogo (artículos) compartido** por grupo, con **existencias por empresa**
  y actividad de negocio (área `Articulos`) — ver más abajo.

## Modelo

- **Grupo** (`organizacion.grupo`): tenant de los maestros compartidos. Cada **empresa** pertenece a
  un grupo (`empresa.grupo_id`).
- Los maestros compartidos heredan de `RaizAgregadoGrupo` (llevan `grupo_id` en vez de `empresa_id`).
  La persistencia aplica un **filtro global por grupo** y **RLS por grupo**
  (`app.grupo_actual`), del mismo modo que antes lo hacía por empresa. El grupo activo viaja en el
  **token** (claim `grupo_id`), derivado de la empresa seleccionada.
- **Clientes y proveedores** son ahora del grupo: se ven y se editan igual desde cualquier empresa del
  grupo.

## Retrocompatibilidad y migración

La migración es **no disruptiva**: cada empresa existente pasa a su **propio grupo** (un grupo por
empresa), de modo que el aislamiento actual se mantiene idéntico. Los clientes/proveedores existentes
se reasignan al grupo de su empresa. Para **empezar a compartir**, basta con crear (o mover) varias
empresas dentro del **mismo grupo**.

## API

- `POST /empresas` acepta un `grupoId` opcional: si se indica, la nueva empresa **se une a ese grupo**
  (y comparte sus maestros); si se omite, se crea un grupo nuevo para ella.
- `GET /grupos/actual` devuelve el grupo de la empresa activa.
- Los endpoints de clientes/proveedores (`/clientes`, `/proveedores`, y `/api/v1/clientes`) operan
  sobre el **grupo** de la empresa activa; el aislamiento lo garantiza el filtro global + RLS.

## Aislamiento y seguridad

- Filtro global de EF Core por `grupo_id` en todos los maestros del grupo.
- **RLS** en `terceros.cliente` y `terceros.proveedor` por `app.grupo_actual` (segunda barrera).
- La tabla `clave_api` sigue siendo por empresa; la API pública fija empresa **y** grupo al autenticar.
- Los procesos en segundo plano (facturación recurrente) fijan empresa **y** grupo por ámbito para
  acceder correctamente a los maestros compartidos.

## Actividades de negocio y visibilidad (Fase 2)

Una **actividad de negocio** (`organizacion.actividad_negocio`) es una dimensión transversal del
grupo —una línea o división de negocio— con la que se **clasifican los datos maestros**. Pertenece al
grupo (compartida entre sus empresas) y tiene nombre y estado (activa/inactiva; una inactiva no se
ofrece para clasificar nuevos maestros). Cada **cliente** y **proveedor** puede llevar una
`actividad_negocio_id` opcional (`null` = sin actividad).

### Visibilidad por usuario y por pantalla

La **visibilidad** (`organizacion.visibilidad_actividad`) concede a un usuario ver una actividad en
un **área** (pantalla): `Ventas`, `Compras`, `Articulos` o `General`. La regla es **abierta por
defecto**:

- Si un usuario **no** tiene ninguna regla en un área, ve **todas** las actividades en ella.
- En cuanto tiene **alguna** regla en el área, solo ve las actividades **concedidas**… **más** los
  maestros **sin actividad** (`actividad_negocio_id = null`), que son visibles para todos.

El filtrado ocurre en la base de datos: los listados de clientes (área `Ventas`) y proveedores (área
`Compras`) resuelven el conjunto permitido del usuario autenticado (claim `sub` del token) y filtran
`actividad_negocio_id IN (permitidas) OR actividad_negocio_id IS NULL`.

### API

- `GET /actividades` · `POST /actividades` · `PUT /actividades/{id}` (renombrar / activar). Crear y
  editar requieren el permiso `actividad.gestionar`; listar solo autenticación.
- `GET /actividades/visibilidad/{usuarioId}` y `PUT /actividades/visibilidad/{usuarioId}` (reemplaza
  el conjunto de un usuario en un área), ambos con permiso `actividad.gestionar`.
- Los listados `GET /clientes` y `GET /proveedores` (y sus `/buscar`) aplican la visibilidad del
  usuario según el área.

### Aislamiento

Las tablas `actividad_negocio` y `visibilidad_actividad` son del **grupo**: filtro global de EF Core
por `grupo_id` y **RLS** por `app.grupo_actual` (segunda barrera), igual que el resto de maestros.

## Catálogo compartido por grupo, existencias por empresa (Fase 3)

El **catálogo** (artículo, familia y su histórico de precios) pasa a ser del **grupo**: un artículo
creado una vez —con su referencia, nombre, precio, IVA, familia, unidades, composición y variantes—
**vale para todas las empresas del grupo**. Filtro global de EF Core por `grupo_id` y **RLS** por
`app.grupo_actual`, igual que Terceros.

Las **existencias son por empresa** (cada empresa lleva su stock): se extraen a
`catalogo.existencia_simple` (una fila por empresa + artículo, aislada por empresa), y los
`movimiento_stock` siguen siendo por empresa. El control de stock por almacenes/ubicaciones vive,
como antes, en el módulo de **Inventario** (por empresa). Así, el mismo artículo puede tener stock 0
en una empresa y 50 en otra del mismo grupo.

Cada **artículo** admite una `actividad_negocio_id` opcional. Los listados/búsquedas de artículos
aplican la **visibilidad por usuario** en el área `Articulos` (misma regla abierta-por-defecto que
Ventas/Compras): sin reglas se ven todos; con reglas, solo las actividades concedidas más los
artículos sin actividad.

### Migración

No disruptiva: el stock simple del artículo se traslada a `existencia_simple` de su empresa antes de
compartir el artículo; luego `empresa_id` → `grupo_id` en `producto`, `familia` e `historico_precio`
(rellenando el grupo de cada empresa, uno por empresa por defecto). Las políticas RLS por empresa se
sustituyen por RLS por grupo en el catálogo; `existencia_simple` estrena RLS por empresa.

## Tests

- **Unitarios**: `Empresa`/`Grupo`; `Cliente`/`Proveedor` con `GrupoId`; `ActividadNegocio`/
  `VisibilidadActividad`; `ExistenciaSimple` (entradas/salidas/ajuste, cantidad no negativa).
- **Integración**: maestros de Terceros compartidos por grupo y aislados entre grupos; CRUD de
  actividades y visibilidad por área; **catálogo compartido** por grupo con **stock por empresa**
  (un movimiento en una empresa no afecta a otra) y visibilidad de artículos por actividad; e informe
  por actividad (ver abajo). Toda la batería de integración (245) sigue verde.

## Actividad en documentos e informes

Las **facturas** (emitidas, tickets y rectificativas) y los **gastos** guardan la actividad de
negocio del tercero en el momento de emitir/registrar (snapshot `actividad_negocio_id`, derivado del
cliente/proveedor). Con ello, el informe **`GET /informes/por-actividad`** (permiso `informe.leer`)
agrega **ventas y compras en base imponible por actividad** en un periodo, con una fila «Sin
actividad» para los documentos sin clasificar y el resultado (ventas − compras) por actividad y
total. Los nombres de actividad se resuelven en el grupo.
