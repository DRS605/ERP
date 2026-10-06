# App de planta: volcado de palots sin conexión

App web instalable (PWA) en `/planta/index.html`, pensada para la tableta o el lector de cada línea. Al volcar un palot
en la línea, el operario lee su etiqueta SSCC. Con cada lectura queda anotado qué palé, en qué línea y orden, a qué hora
y desde qué terminal. Con esos volcados, el ERP genera el parte de confección.

## Terminal

Un terminal es un acceso de tipo **Terminal de planta**. Se crea en *Configuración → Portal y terminales de planta*
con un nombre, por ejemplo «Volcador línea 1». Su enlace `…/planta/index.html#clave` se abre una vez en el
dispositivo. La app guarda la clave, quita el fragmento de la dirección y entra sola cada vez que caduca su sesión.

Funciona como el portal (ver [portal.md](portal.md)):

- la sesión solo vale para las rutas `/portal/…`;
- revocar el acceso corta el terminal en la siguiente petición. Las lecturas que tenga sin enviar se conservan y se
  mandan si en ese dispositivo se abre un enlace nuevo.

## Sin conexión

- El *service worker* (`/planta/sw.js`) guarda la app. Se abre aunque no haya red: red primero y, si falla, caché.
- `GET /portal/planta/datos` descarga las líneas activas, las órdenes de ayer a mañana sin terminar y los palés que
  se pueden volcar (abiertos o cerrados con kilos), con su producto y agricultor. Se refresca cada 10 minutos.
- Cada lectura se guarda en el dispositivo (`localStorage`) con una **clave única** y la hora del terminal. Antes de
  guardarla, la app comprueba el código SSCC (dígito de control GS1; acepta `(00)`, `00…` y el GS1-128 completo) y que
  ese palot no se haya volcado ya hoy en ese terminal. Si el palé está en los datos descargados, muestra su producto,
  su agricultor y sus kilos.
- La cola se manda con `POST /portal/planta/volcados` al momento, cada 15 s y al recuperar la red. Reenviar una
  lectura con la misma clave devuelve `ya_registrado` y no la duplica. Si se corta la red justo después de enviar, no
  pasa nada.
- Si el servidor rechaza una lectura, el terminal pita y vibra. La lectura queda en rojo con el motivo hasta que el
  operario la descarta. Motivos posibles: palé desconocido, ya volcado, expedido, vacío, hora futura o línea de baja.

## Reglas del volcado

- Un palé solo tiene un volcado vivo: pendiente o en un parte que no se ha borrado ni anulado. Lo garantiza un índice
  único parcial en la base de datos.
- La hora es la del terminal. Se admite hasta 10 minutos por delante del servidor (`volcado.futuro`). El día del
  volcado es el día local del terminal.
- El volcado no mueve existencias.

## En el ERP

*Agro → Volcados de palots* (`/agro/planta/volcados`):

- volcados del día con su terminal. Los que llegaron más de 5 minutos tarde se marcan «sin conexión»;
- **Anular** un volcado pendiente (lectura errónea);
- **Generar parte de confección** por línea (`POST /agro/planta/volcados/parte {lineaId, fecha, ordenId?}`): crea un
  parte en borrador que consume lo que lleve cada palé volcado, por partida y palé, con el centro analítico de la línea.
  Los volcados pasan a «En parte». Si el borrador se elimina o el parte se anula, vuelven a quedar pendientes;
- `POST /agro/planta/volcados` registra lecturas desde el propio ERP (lector en el puesto), con el nombre del usuario
  como terminal.
