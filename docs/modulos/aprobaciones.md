# Módulo Aprobaciones (control de firmas y segregación de funciones)

Añade **control interno** al ERP: exigir la aprobación de una **segunda persona** antes de dar por
buena una operación relevante (una factura, un pago, un gasto…) y garantizar la **segregación de
funciones (SoD, *segregation of duties*)** — quien **solicita** una operación **no puede aprobarla**
él mismo. Es un requisito habitual en empresas medianas y grandes para prevenir fraude y errores.

El módulo es **transversal y ligero**: no obliga a ningún otro módulo a integrarse con él. Otros
módulos pueden consultarlo mediante el puerto `IFlujoAprobaciones` (en el núcleo) para preguntar si
una operación requiere aprobación y abrir la solicitud correspondiente, sin acoplarse a la
persistencia de Aprobaciones.

## Reglas por umbral

`ReglaAprobacion` { TipoDocumento (Factura, Gasto, Pago…), **UmbralImporte**, Activa } por empresa
(RLS). La regla `Requiere(importe)` devuelve verdadero cuando está **activa** y el importe es **mayor
o igual** al umbral. Configurar el mismo tipo de documento **actualiza** la regla (no duplica; hay
índice único `empresa+tipo_documento`). El tipo de documento se **normaliza** (recorta espacios) y no
puede quedar vacío; el umbral no puede ser negativo.

- `GET /aprobaciones/reglas` — lista las reglas de la empresa.
- `PUT /aprobaciones/reglas` — crea o actualiza la regla de un tipo (permiso `aprobacion.configurar`).
- `GET /aprobaciones/requiere?tipoDocumento=&importe=` — indica si esa operación exige aprobación.

## Solicitudes y segregación de funciones

`SolicitudAprobacion` { TipoDocumento, DocumentoId, Referencia, Importe, **SolicitanteUsuarioId**,
Estado, AprobadorUsuarioId?, Motivo?, CreadoEn, ResueltaEn? } por empresa (RLS). El estado sigue la
máquina `Pendiente → Aprobada | Rechazada`; una vez resuelta **no** vuelve a cambiar.

El **solicitante** es siempre el **usuario actual** (se toma del token, no del cuerpo de la petición).
Al **aprobar** o **rechazar** se valida en el **dominio**:

- La solicitud debe estar **pendiente** (no se resuelve dos veces).
- El aprobador debe estar identificado.
- **SoD**: `aprobadorUsuarioId != solicitanteUsuarioId`. Si coinciden, la operación falla con
  `Error.Conflicto("aprobacion.segregacion")` → **HTTP 409**. El solicitante no puede aprobar **ni
  rechazar** su propia solicitud.

Al resolverse, el agregado registra el evento de dominio `SolicitudResuelta` (aprobada o rechazada),
que la infraestructura publica tras el `SaveChanges` (queda en la auditoría como el resto de eventos).

- `GET /aprobaciones/solicitudes[?estado=Pendiente|Aprobada|Rechazada]` — lista, opcionalmente por estado.
- `POST /aprobaciones/solicitudes` — abre una solicitud (el solicitante es el usuario actual).
- `POST /aprobaciones/solicitudes/{id}/aprobar` — aprueba (permiso `aprobacion.aprobar`; SoD aplicada).
- `POST /aprobaciones/solicitudes/{id}/rechazar` — rechaza con motivo (permiso `aprobacion.aprobar`).

## Permisos

Dos permisos nuevos en el núcleo, incluidos en `Permisos.Todos` (el rol **Propietario** los recibe
automáticamente):

- `aprobacion.configurar` — definir las reglas por umbral.
- `aprobacion.aprobar` — resolver (aprobar/rechazar) solicitudes.

Consultar reglas, la comprobación de si una operación requiere aprobación, listar y **abrir**
solicitudes solo exige estar autenticado (cualquier usuario de la empresa puede pedir una aprobación;
únicamente **resolverla** requiere el permiso).

## Interfaz de usuario

Pantalla **Aprobaciones** en el menú lateral: gestión de las **reglas por umbral** (alta/edición con
activación) y bandeja de **solicitudes** con su estado. En una solicitud propia la UI muestra
«Requiere otra firma» en vez de los botones de aprobar/rechazar, reflejando la SoD antes incluso de
llamar al servidor.

## Persistencia

Esquema **`aprobaciones`** con dos tablas (RLS por empresa):

- `regla_aprobacion` — índice único `empresa+tipo_documento`.
- `solicitud_aprobacion` — índice `empresa+estado+creado_en`; `estado` persistido como texto.

## Alcance y siguiente paso

Este incremento entrega el **motor de aprobaciones** independiente. La integración automática desde
otros módulos (p. ej. bloquear el pago de una factura hasta que su solicitud esté aprobada) se apoya
en el puerto `IFlujoAprobaciones` ya disponible: `RequiereAprobacionAsync` y `AbrirSolicitudAsync`.

## Tests

- **Unitarios** (dominio): la regla activa exige a partir del umbral y la inactiva nunca; rechazo de
  tipo vacío y umbral negativo; un aprobador **distinto** puede aprobar (con evento
  `SolicitudResuelta`); la **SoD** impide que el solicitante apruebe **y** rechace; no se resuelve dos
  veces; el motivo se guarda al rechazar; referencia y solicitante vacíos rechazados.
- **Integración**: configurar una regla y consultar `requiere` por debajo/encima del umbral (y que
  reconfigurar el mismo tipo no duplica); abrir una solicitud y listar las pendientes; la **SoD**
  extremo a extremo (el propietario, único usuario, crea la solicitud y al intentar aprobarla y
  rechazarla obtiene **409** y sigue pendiente); referencia vacía da **400**.
