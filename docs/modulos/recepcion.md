# Módulo Recepción de facturas de proveedor

Automatiza la **entrada de facturas de proveedor** y su **contabilización**. Las facturas llegan a
una **bandeja de entrada** (por correo o subidas a mano); una persona las **valida** y, con un clic,
se **contabilizan**. Multiempresa (todo lleva `empresa_id`, con filtro global y RLS).

> Principio del pliego: **nunca se contabiliza algo que no se ha podido validar por completo**. El
> paso a *Contabilizada* exige pasar antes por *Validada* (invariante del dominio).

## Ciclo de vida (`FacturaRecibida`)

```
Recibida → Validada → Contabilizada
   └──────────────→ Rechazada
```

- **Recibida**: está en la bandeja, pendiente de revisión. Guarda el adjunto (PDF) y, si vino por
  correo, el remitente y el asunto.
- **Validada**: una persona confirmó/corrigió proveedor, fecha, base imponible, IVA e IRPF. Se puede
  re-validar (editar) mientras no esté contabilizada.
- **Contabilizada**: generó un **gasto** (con IVA soportado). Queda enlazada por `gasto_id`. Ya no se
  edita ni se rechaza.
- **Rechazada**: descartada (duplicada, ilegible, no es una factura…).

## Contabilización: el puerto `IContabilizador` (preparado para partida doble)

"Contabilizar" delega en el puerto de aplicación **`IContabilizador`**. Hoy el único adaptador,
`ContabilizadorGastos`, trabaja en modo **"un libro"**: crea un `Gasto` con IVA soportado + retención,
que ya alimenta el **Libro de IVA** y los modelos **303/130**. El resto del módulo no conoce los
detalles contables.

Cuando se añada la **contabilidad de partida doble** (libro diario/mayor, plan PGC), bastará un
segundo adaptador del mismo puerto que, además del gasto, genere el **asiento** (cuentas 6xx/472/400/
4751). Está pensado como un **ajuste por empresa** ("Modo de contabilidad": *Simple* / *Completo*)
que elige qué `IContabilizador` corre, sin tocar la recepción.

## Captura por buzón de correo

El puerto **`IBuzonFacturas`** abstrae el buzón; el adaptador real es **`BuzonImapMailKit`** (IMAP con
MailKit): lee los correos no leídos, extrae los adjuntos **PDF** y los da de alta en la bandeja
(estado *Recibida*). El servicio en segundo plano **`ServicioBuzonProveedores`** lo sondea cada
`IntervaloSegundos`. Está **apagado** salvo que el buzón esté configurado.

- **Credenciales fuera de la base de datos**: la configuración (`BuzonProveedores`) guarda host,
  puerto, usuario y una **referencia** al secreto (`SecretoRef`), nunca la contraseña. Esta se
  resuelve por el puerto **`IAlmacenSecretos`** (adaptador que lee de la sección `Secretos` de la
  configuración o de una variable de entorno).
- Configuración de un único buzón por despliegue (se asigna a `EmpresaId`). El multi-buzón por
  empresa es una mejora futura.

## API

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/recepcion/facturas?estado=` | permiso `recepcion.leer` | Lista la bandeja (opcional por estado). |
| `GET` | `/recepcion/facturas/{id}` | permiso `recepcion.leer` | Obtiene una factura recibida. |
| `GET` | `/recepcion/facturas/{id}/documento` | permiso `recepcion.leer` | Descarga el PDF adjunto. |
| `POST` | `/recepcion/facturas` | permiso `recepcion.gestionar` | Alta manual subiendo el PDF (base64). **201** |
| `POST` | `/recepcion/facturas/{id}/validar` | permiso `recepcion.gestionar` | Valida/corrige los datos. |
| `POST` | `/recepcion/facturas/{id}/contabilizar` | permiso `recepcion.contabilizar` | Contabiliza (genera el gasto). |
| `POST` | `/recepcion/facturas/{id}/rechazar` | permiso `recepcion.gestionar` | Descarta la factura. |
| `POST` | `/recepcion/buzon/procesar` | permiso `recepcion.gestionar` | Revisa el buzón y da de alta las nuevas. |

## Composición

- Contabiliza a través de **Gastos** (`RegistrarGasto`), que a su vez resuelve el proveedor en
  **Terceros** (`IConsultaProveedores`) y calcula el IVA soportado.
- Al validar con `ProveedorId`, se copia el nombre del proveedor en `ProveedorTexto`.

## Persistencia

- Esquema **`recepcion`**, tabla `factura_recibida` (RLS por empresa). El adjunto se guarda en
  `contenido` (`bytea`). Índice `(empresa_id, estado)`.
- Migración: `MigracionInicialRecepcion` (incluye la activación de RLS).

## Tests

- **Unitarios**: ciclo de vida de `FacturaRecibida` (recibir, no contabilizar sin validar, validar,
  contabilizar, rechazar) y el pipeline `ProcesarBuzon` (un alta por cada PDF, ignora otros adjuntos).
- **Integración**: flujo completo **recibir → validar → contabilizar** y comprobación de que genera un
  gasto que cuadra (300 + 21 % = 363); validación incompleta → 400; contabilizar sin validar → 409;
  rechazo; descarga del PDF.
