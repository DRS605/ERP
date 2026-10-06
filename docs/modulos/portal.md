# Portal del agricultor y del cliente

Página aparte (`/portal.html`), pensada para el móvil, donde un agricultor consulta sus entregas y liquidaciones y un
cliente sus facturas, albaranes y saldo. No necesita usuario ni contraseña: entra con un **enlace** que le envía la
empresa.

## Accesos

*Configuración → Portal y terminales de planta* (permiso de gestionar usuarios). Los terminales de la planta usan el
mismo mecanismo de enlace: ver [app-planta.md](app-planta.md).

- **Nuevo acceso**: se elige el agricultor o el cliente y, si se quiere, una fecha de caducidad. Se muestra el enlace
  `https://…/portal.html#<clave>` **una sola vez**. La base de datos guarda solo la huella SHA-256 del secreto.
- **Nuevo enlace**: genera otra clave. La anterior deja de valer.
- **Revocar**: el enlace deja de valer y las sesiones abiertas se cortan en la siguiente petición.
- Cada tercero tiene como mucho un acceso vigente. Se anotan el último acceso y el número de entradas.

La clave es `empresa.acceso.secreto`, con un secreto aleatorio de 24 bytes. La parte tras `#` no viaja en las
peticiones ni queda en los registros del servidor. La página no envía referer.

## Sesión del portal

`POST /portal/entrar {clave}` (anónimo, con el límite de intentos de la autenticación) devuelve un token con el claim
`portal = Tipo:acceso:tercero`. Ese token:

- solo sirve para las rutas `/portal/…`. Cualquier otra ruta de la API responde 403 `portal.solo_portal` (lo comprueba
  `MiddlewarePortal`);
- en cada petición comprueba de nuevo que el acceso sigue vigente (si se ha revocado o ha caducado, 401
  `portal.clave`);
- solo ve los documentos del tercero del acceso. Pedir un documento ajeno devuelve 404 `portal.no_encontrado`.

| Ruta | Quién | Qué |
|---|---|---|
| `GET /portal/yo` | los dos | nombre, tipo y empresa |
| `GET /portal/agricultor/resumen` | agricultor | kilos entregados, liquidados y pendientes; importe liquidado |
| `GET /portal/agricultor/entregas` | agricultor | recepciones confirmadas con los kilos por producto y calibre |
| `GET /portal/agricultor/liquidaciones` (+ `/{id}/pdf`) | agricultor | liquidaciones emitidas |
| `GET /portal/cliente/resumen` | cliente | pendiente, vencido y próximo vencimiento |
| `GET /portal/cliente/facturas` (+ `/{id}/pdf`) | cliente | facturas con lo pendiente de cada una |
| `GET /portal/cliente/albaranes` (+ `/{id}/pdf`) | cliente | albaranes no anulados (PDF valorado) |

El portal no muestra borradores: ni recepciones sin confirmar ni liquidaciones sin emitir.

# Panel de campaña

*Agro → Panel de campaña* (`GET /agro/panel?campanaId=`, permiso de leer agro). Se refresca solo cada 30 segundos. Sin
`campanaId` toma la campaña que contiene la fecha de hoy o, si no hay, la última.

- **Hoy**: kilos entrados (recepciones confirmadas) en total y por producto, kilos confeccionados (partes validados),
  palés y kilos expedidos.
- **Almacén**: kilos de las partidas con saldo, por producto.
- **Últimos 14 días**: entrada y salida por día.
- **Campaña**: kilos recibidos y liquidados, importe liquidado y los diez agricultores con más kilos.

## Avisos de anomalías

Son reglas fijas sobre los datos, sin aprendizaje automático:

| Código | Gravedad | Cuándo |
|---|---|---|
| `recepcion.borrador` | aviso | recepción de hace 1 día o más sin confirmar |
| `recepcion.sin_kilos` | grave | recepción confirmada con 0 kg netos |
| `partida.parada` | aviso | partida con saldo y de hace 7 días o más (fruta parada en cámara) |
| `pale.sin_expedir` | aviso | palé cerrado cuya fruta más reciente tiene 3 días o más |
| `confeccion.borrador` | aviso | parte de confección de días anteriores sin validar |
| `confeccion.rendimiento` | grave | parte validado que obtiene menos del 60 % de los kilos que consume |
| `liquidacion.borrador` | aviso | liquidación en borrador de hace 7 días o más |

Los umbrales son constantes de `EndpointsPanelAgro`. Las recepciones y los partes se miran en los últimos 60 días.
