# Grupos (holding) y maestros compartidos — Fase 1 (Terceros)

Refuerza el enfoque multiempresa: por encima de la empresa se introduce el **grupo (holding)**, y los
**datos maestros** pasan a compartirse dentro del grupo, de modo que **un cliente o proveedor creado
una vez vale para todas las empresas del mismo grupo**. Los documentos fiscales (facturas, cobros,
gastos, contabilidad) siguen siendo **por empresa**, como exige la fiscalidad.

Esta es la **Fase 1**: introduce el grupo y comparte los maestros de **Terceros** (clientes y
proveedores). Las fases siguientes: **actividades de negocio + visibilidad por usuario/pantalla**
(Fase 2) y **artículos compartidos** (Fase 3).

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

## Tests

- **Unitarios**: `Empresa` y `Grupo` (creación); `Cliente`/`Proveedor` conservan sus invariantes con
  `GrupoId`.
- **Integración**: dos empresas del **mismo grupo comparten** los clientes; empresas de **grupos
  distintos no**; y toda la batería previa (238) sigue verde con los maestros ya a nivel de grupo.

## Siguientes fases

1. **Actividades de negocio**: clasificación transversal (líneas/divisiones) asignable a los maestros
   (y opcionalmente a documentos), con **visibilidad por usuario y por pantalla**.
2. **Artículos (Catálogo) compartidos por grupo**, con el mismo patrón que Terceros.
3. Actividad de negocio en documentos e informes segmentados por actividad.
