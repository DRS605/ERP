# Seguridad y operación

Endurecimiento de la plataforma en cinco frentes: verificación en dos pasos, limitación de
peticiones en autenticación, comprobaciones de salud, cabeceras de seguridad e identificador de
correlación en las trazas.

## Verificación en dos pasos (2FA / TOTP)

Segundo factor basado en tiempo (**TOTP**, RFC 6238, HMAC-SHA1, 6 dígitos, ventana de 30 s),
compatible con Google Authenticator, Authy, 1Password, etc. El secreto se guarda en el usuario
(Base32) y **nunca** viaja tras la activación; los **códigos de recuperación** se guardan solo como
hash SHA-256.

Flujo:

1. `POST /auth/2fa/preparar` (autenticado) → genera el secreto y el URI `otpauth://` (aún **no** activo).
2. El usuario lo añade a su app y `POST /auth/2fa/activar` con un código válido → se activa y devuelve
   **8 códigos de recuperación** (una sola vez).
3. `GET /auth/2fa` consulta el estado; `POST /auth/2fa/desactivar` lo apaga.

En el **login en dos pasos**: si el usuario tiene 2FA activo, `POST /auth/login` sin código responde
`{ requiere2fa: true }` sin emitir token; repitiendo el login con `codigo` (un TOTP **o** un código de
recuperación, que se consume) se completa y se emite el token. La contraseña se revalida en cada
intento, por lo que el flujo no necesita estado de servidor.

La lógica TOTP vive en el dominio (`Totp`) y las invariantes del 2FA en el agregado `Usuario`
(`PrepararDobleFactor`, `ActivarDobleFactor`, `VerificarSegundoFactor`, `DesactivarDobleFactor`), sin
dependencias de framework.

**Interfaz**: la clásica y la SPA piden el código en el login cuando hace falta; Ajustes → «Seguridad»
permite activar (con secreto + códigos de recuperación) y desactivar el 2FA.

## Limitación de peticiones (rate limiting)

Los endpoints sensibles a fuerza bruta — `POST /auth/login` y `POST /auth/recuperar` — usan una
política de ventana fija **particionada por IP**. Superado el cupo se responde **429**. Configurable:

```
"Seguridad": { "RateLimitPeticiones": 10, "RateLimitVentanaSegundos": 60 }
```

## Health checks (operación)

- `GET /salud/vivo` — *liveness*: responde 200 si el proceso está vivo (sin comprobaciones).
- `GET /salud/listo` — *readiness*: comprueba que **PostgreSQL** responde (`CanConnect`); 200 si está
  listo para recibir tráfico. Ambos anónimos, para sondas de Kubernetes/orquestador. Se mantiene el
  `GET /salud` original.

## Cabeceras de seguridad

Un middleware añade a toda respuesta: `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`
(anti-clickjacking), `Referrer-Policy: strict-origin-when-cross-origin`,
`Cross-Origin-Opener-Policy: same-origin` y `Permissions-Policy` (solo `camera=(self)` para el TPV; el
resto deshabilitado). En producción se activa además **HSTS**. No se fija una **CSP** estricta todavía
porque la interfaz clásica usa código en línea; la SPA (sin *inline*) podrá adoptarla más adelante.

## Identificador de correlación

Un middleware asigna a cada petición un **`X-Correlation-Id`** (el entrante si llega, o uno nuevo), lo
incorpora al ámbito de logging —así aparece en todas las trazas de la petición— y lo devuelve en la
respuesta, facilitando el diagnóstico en producción.

## Tests

- **Unitarios** (dominio): `Totp` (cálculo/verificación de 6 dígitos, ventana de tolerancia, rechazo de
  códigos mal formados, URI otpauth) y `Usuario` (preparar/activar/desactivar 2FA, TOTP y consumo de
  código de recuperación); y el caso de uso `IniciarSesion` con 2FA (reto sin código, éxito con código,
  fallo con código incorrecto).
- **Integración**: `/salud/vivo` y `/salud/listo`; cabeceras de seguridad presentes; el
  `X-Correlation-Id` se devuelve y se respeta el entrante; flujo 2FA extremo a extremo (preparar →
  activar → login pide código → login con TOTP → login con código de recuperación); y el **429** al
  superar el cupo de login (fábrica aislada con límite bajo).
