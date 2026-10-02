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
  (y comparte sus maestros); si se omite, se crea un grupo nuevo para ella. Solo puede indicarlo quien tenga el
  permiso `empresa.ajustes` en alguna empresa de ese grupo (si no, **403** `grupo.sin_acceso`): entrar en un grupo da
  acceso a sus clientes, proveedores y artículos. En la pantalla es la casilla «En el mismo grupo que…» de
  **+ Nueva empresa**; al crearla se ofrece copiarle la configuración (ver más abajo).
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
  (un movimiento en una empresa no afecta a otra) y visibilidad de artículos por actividad; informe
  por actividad; y elección de actividad al emitir/registrar restringida al acceso del usuario (403 si
  no tiene acceso). Toda la batería de integración (248) sigue verde.

## Actividad en documentos e informes

Las **facturas** (emitidas, tickets y rectificativas) y los **gastos** guardan la actividad de
negocio del tercero en el momento de emitir/registrar (snapshot `actividad_negocio_id`, derivado del
cliente/proveedor). Al emitir una factura o registrar un gasto se puede **elegir** explícitamente la
actividad (en vez de heredarla): el comando acepta `ActividadNegocioId` y la API solo la admite si el
usuario **tiene acceso** a esa actividad según su visibilidad en el área (Ventas para facturas,
Compras para gastos); en caso contrario responde **403**. «Sin actividad» (null) siempre se admite. Con ello, el informe **`GET /informes/por-actividad`** (permiso `informe.leer`)
agrega **ventas y compras en base imponible por actividad** en un periodo, con una fila «Sin
actividad» para los documentos sin clasificar y el resultado (ventas − compras) por actividad y
total. Los nombres de actividad se resuelven en el grupo.

## Copiar la configuración de otra empresa

Lo que es **de cada empresa** (no del grupo) se puede copiar de otra empresa del usuario a la activa desde
**Configuración → Copiar de otra empresa** (también en **Grupo de empresas → Copiar configuración**). Sirve para montar
una empresa hermana sin volver a dar de alta su configuración.

| Bloque | Qué copia | Se compara por |
|---|---|---|
| Plan de cuentas | las cuentas y subcuentas que faltan | código |
| Diarios contables | los diarios propios con sus orígenes (los de sistema ya los tiene cada empresa) | código |
| Formas de pago | las activas, con sus días de vencimiento | nombre |
| Series de numeración | las del ejercicio en curso y siguientes, empezando en el 1 | tipo, ejercicio y prefijo |
| Almacenes y ubicaciones | los almacenes activos y las ubicaciones que falten (sin existencias) | código |
| Transportistas y vehículos | los activos; el vehículo va con el transportista copiado | nombre / matrícula |
| Soportes logísticos | medidas, tara y carga máxima (el envase, solo dentro del grupo) | código |
| Campañas, categorías y conceptos de liquidación (agro) | los generales; los de un agricultor concreto no | código |
| Taras de envases (agro) | la tara vigente de cada envase; **solo entre empresas del mismo grupo** | envase |

- **Primero la vista previa** (`POST /empresas/actual/copia/vista-previa`): qué se crearía, qué ya estaba y qué no se
  copia y por qué. No cambia nada. Luego la copia (`POST /empresas/actual/copia`), con el mismo cuerpo
  `{ origenEmpresaId, elementos: [...] }`. `GET /empresas/actual/copia` da los bloques y las empresas de origen.
- **Solo añade**: nunca cambia ni borra lo que la empresa de destino ya tiene, así que se puede repetir sin duplicar.
  Cada alta pasa por su caso de uso normal (con sus validaciones); un error en una no para las demás y se informa.
- **Acceso**: el usuario tiene que pertenecer a la empresa de origen y tener el permiso `empresa.ajustes` en la activa.
  Los bloques de un módulo que la empresa activa no tiene contratado no se copian.
- **Aislamiento**: cada empresa se lee y se escribe en su propio ámbito, con su empresa activa (`CopiaConfiguracion`,
  igual que las operaciones intragrupo), así que la RLS de la base de datos se cumple.
- **Fuera del grupo**: lo que apunta a artículos (maestros del grupo) no existe en la otra empresa: las taras no se
  copian, los soportes pierden el envase y los conceptos ligados a un artículo se saltan.

## Unir al grupo una empresa que ya existe

**Grupo de empresas → Unir una empresa al grupo** pasa al grupo de la empresa activa otra empresa del usuario que hoy
va por su cuenta (`GET /grupos/actual/union` da las candidatas; `POST /grupos/actual/union/vista-previa` y
`POST /grupos/actual/union` con `{ empresaId }`).

- **Qué pasa**: la empresa cambia de grupo y todos sus maestros compartidos (clientes, proveedores, artículos y su
  histórico de precios, familias, tarifas, conceptos de línea, centros, partidas y claves de reparto, actividades y su
  visibilidad, perímetro y correspondencias de consolidación) pasan al grupo **con el mismo identificador**, así que
  facturas, albaranes, cartera, asientos y stock siguen apuntando a ellos sin tocarlos. Su grupo antiguo desaparece.
- **Repetidos**: los que coinciden con uno del grupo (clientes y proveedores por NIF, artículos por referencia, familias
  por código o nombre, tarifas, conceptos y analítica por código, actividades por nombre) se **dan de baja** en la
  empresa que se une: sus documentos antiguos los conservan y los nuevos usan la ficha del grupo. Los que tienen un
  código único en el grupo (tarifas, conceptos, centros, partidas, claves) cambian de código con un sufijo («MAYOR-2»).
  No se reescriben documentos: las facturas emitidas y la contabilidad no se pueden cambiar.
- **Condiciones**: la empresa tiene que estar **sola en su grupo** (si comparte grupo con otras, se uniría sin ellas y
  se quedarían sin sus maestros) y el usuario tiene que tener `empresa.ajustes` en las dos. **No se puede deshacer**;
  por eso hay vista previa. Quien tenga abierta esa empresa debe volver a seleccionarla (el grupo viaja en el token).
- **Base de datos**: la RLS por grupo no deja que una fila salga de su grupo. La unión, en una sola transacción, fija
  `app.grupo_fusion` (`RlsSql.ParametroFusionGrupo`) con el grupo de destino, y las políticas `pol_grupo_*` admiten ese
  grupo además del activo (migración `FusionGrupos`). Ninguna otra operación fija ese parámetro.

## Baja de una empresa del grupo

Al eliminar una empresa (`DELETE /cuenta`) los maestros del grupo **solo se borran si es la última empresa del grupo**;
si quedan otras, siguen siendo suyos.
