# Módulo Integraciones (API pública + webhooks)

Abre ALXOR Core al exterior de dos maneras complementarias: una **API pública** para que otras
aplicaciones consulten datos, y **webhooks** salientes para avisar a esas aplicaciones cuando ocurre
algo. Es la base para conectar el resto de la plataforma (Questioner, Tuday, CostControl…) y
cualquier integración de terceros, sin acoplar los módulos de negocio a los consumidores.

## Claves de API

`ClaveApi` { Nombre, Prefijo, HashSecreto, Activa, CreadoEn, UltimoUsoEn, RevocadaEn } por empresa.
Autentican la API pública, **al margen del JWT** de la interfaz. Al crear una clave se genera un
secreto aleatorio (`ak_…`) que se muestra **una sola vez**; en la base solo se guarda su **hash
SHA-256**, de modo que un volcado de la tabla no revela ninguna clave utilizable.

- `GET /integraciones/claves` — lista las claves (sin secreto).
- `POST /integraciones/claves` — crea una y devuelve el secreto (una única vez).
- `DELETE /integraciones/claves/{id}` — revoca una clave.

Todo bajo el permiso `integracion.gestionar`.

## API pública versionada

Bajo `/api/v1`, autenticada por la cabecera **`X-Api-Key`** (no JWT). Un filtro valida la clave,
resuelve su empresa y la fija en el contexto multiempresa de la petición (filtro global + RLS), de
modo que la API pública queda **aislada por empresa** igual que el resto del sistema. Sin clave
válida → `401`.

- `GET /api/v1/ping` — comprueba la clave y devuelve la empresa asociada.
- `GET /api/v1/facturas` — facturas de la empresa.
- `GET /api/v1/clientes` — clientes de la empresa.
- `GET /api/v1/productos` — productos de la empresa.

La tabla `clave_api` es la única del sistema **sin RLS a propósito**: la autenticación busca la clave
por su hash *antes* de resolver el tenant. El aislamiento posterior lo garantiza el filtro por
empresa una vez fijada.

## Webhooks

`SuscripcionWebhook` { Url, Secreto, Eventos, Activa } por empresa (RLS). Una URL http(s) que recibe,
por **POST firmado**, los eventos a los que se suscribe. Al crearla se genera un **secreto de firma**
(`whsec_…`), que se muestra una sola vez, para que el receptor verifique la autenticidad.

Eventos disponibles (`GET /integraciones/eventos`): `factura.emitida`, `cobro.registrado`,
`gasto.registrado`, `cliente.creado`, `proveedor.creado`, `producto.creado`.

- `GET /integraciones/webhooks`, `POST /integraciones/webhooks`, `DELETE /integraciones/webhooks/{id}`.
- `GET /integraciones/webhooks/entregas` — buzón de salida (estado de cada entrega).
- `POST /integraciones/webhooks/procesar` — fuerza el envío inmediato de las pendientes.

### Cómo se entrega

1. Cada agregado emite sus **eventos de dominio**; el publicador central
   (`PublicadorEventosIntegraciones`) traduce los de nombre público (p. ej. `FacturaEmitida` →
   `factura.emitida`) y, por cada suscripción activa, encola una `EntregaWebhook` con el cuerpo JSON.
   Así los módulos de negocio **no conocen** las integraciones.
2. Un proceso en segundo plano (`ServicioWebhooks`) recorre las empresas con entregas pendientes y,
   para cada una en su propio ámbito aislado, envía por HTTP POST con las cabeceras `X-Alxor-Firma`
   (HMAC-SHA256 hex del cuerpo con el secreto de la suscripción), `X-Alxor-Evento` y `X-Alxor-Entrega`.
3. Un `2xx` marca la entrega como **entregada**; cualquier otro resultado la reprograma con
   **backoff exponencial** (1, 2, 4, 8 min) hasta un máximo de 5 intentos, tras el cual queda
   **fallida**.

El cuerpo (envoltorio): `{ evento, empresaId, ocurridoEn, datos }`, donde `datos` es el evento de
dominio serializado.

## Interfaz de usuario

Pantalla **API y webhooks**: gestión de claves (crear muestra el secreto una vez, revocar), de
webhooks (crear con selección de eventos y secreto de firma, eliminar) y el listado de **entregas
recientes** con su estado, además de un botón para forzar el envío de las pendientes.

## Persistencia

Esquema **`integraciones`**: `clave_api` (índice único por hash; **sin RLS**, tabla de
autenticación), `suscripcion_webhook` y `entrega_webhook` (ambas con RLS por empresa). Migración
`MigracionInicialIntegraciones`.

## Alcance y siguiente paso

Cubre lo esencial: autenticación por clave, lectura por API y notificación por webhook con firma y
reintentos. Ampliaciones naturales: *scopes* por clave (limitar a ciertos recursos), límites de
frecuencia (*rate limiting*), paginación/filtrado en los endpoints públicos, escritura por API
(crear facturas/clientes) y reenvío manual de una entrega concreta.

## Tests

- **Unitarios** (dominio): la clave devuelve el secreto y guarda solo su hash; revocación; el hash es
  estable y distingue secretos; validación de la suscripción (URL http, eventos normalizados y
  conocidos); firma HMAC estable; backoff y fallo definitivo al agotar los intentos.
- **Integración**: la clave autentica `/api/v1` y su revocación devuelve `401`; la API sin clave o con
  clave inventada da `401`; emitir una factura **encola** la entrega del webhook y `procesar` la marca
  **entregada** (con cliente HTTP falso, sin red); se rechaza suscribir un evento desconocido.
