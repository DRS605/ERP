# Módulo Organización

Segundo módulo de ALXOR Core. Introduce el **tenant** (empresa), las **membresías** que ligan a los
usuarios con sus empresas y roles, y las **series de numeración**. Aquí vive la **infraestructura
multiempresa** que usarán todos los módulos posteriores.

## Responsabilidades

- Crear y consultar **empresas** (el usuario que la crea es su **Propietario**).
- **Seleccionar la empresa activa**: emite un nuevo JWT con el alcance de la empresa (empresa, rol y
  permisos), que el resto de módulos usan para autorizar y aislar datos.
- Gestionar **series de numeración** y ofrecer la **numeración correlativa** a otros módulos.
- Guardar los **datos de cobro** de la empresa —IBAN e **identificador de acreedor SEPA**— que usa
  Tesorería para generar las remesas de adeudos domiciliados (`PUT /empresas/actual/cobro`,
  caso de uso `ActualizarDatosCobro`, permiso `empresa.ajustes`).

## Multiempresa (cómo funciona)

1. Tras `login`, el usuario tiene un token **sin empresa**. Con `GET /empresas` ve sus empresas y con
   `POST /empresas/{id}/seleccionar` obtiene un token **con empresa activa** (claims `empresa_id`,
   `rol` y varios `permiso`).
2. `ContextoEmpresaHttp` lee `empresa_id` del token en cada petición.
3. `DbContextEmpresaBase` (proyecto `AlxorCore.Persistencia`) aplica un **filtro global** por
   `empresa_id` a toda entidad `IEntidadEmpresa`: es imposible olvidar el filtrado.
4. `InterceptorEmpresa` fija `app.empresa_actual` en la conexión para que la **Row-Level Security**
   de PostgreSQL actúe como segunda barrera.

> `empresa` y `membresia` **no** se filtran por empresa (son las tablas que definen el tenant y su
> acceso). `serie_numeracion` sí es dato multiempresa, con RLS activada.

### Nota sobre RLS y el rol de base de datos

La RLS solo surte efecto si la aplicación se conecta con un rol **sin** superusuario ni BYPASSRLS.
En desarrollo/tests se usa `postgres` (superusuario), que la ignora; ahí el aislamiento lo garantiza
el filtro global de EF Core (probado en los tests de integración). En **producción** debe usarse un
rol de aplicación restringido para que la RLS sea efectiva.

## Numeración correlativa

`IServicioNumeracion.SiguienteAsync(empresa, tipo, ejercicio, prefijo?)` asigna el siguiente número
con un `UPDATE ... RETURNING` **atómico** (bloqueo de fila), evitando duplicados y carreras. Si no se
indica `prefijo` usa la serie por defecto (`FA`); en cualquier caso crea la serie de forma
**perezosa** si no existe, de modo que cada nuevo ejercicio (y cada serie: `FA`, `R`, `T`…) obtiene
su propio contador. Lo consume Facturación: al emitir una factura se puede elegir la **serie**
(`EmitirFacturaComando.Serie`), y cada serie numera de forma correlativa e independiente.

> Compromiso conocido: el número se confirma de inmediato. Si la creación del documento fallara
> después, podría quedar un hueco. Se asigna como último paso antes de guardar para minimizar la
> ventana; una numeración 100 % sin huecos ante fallos (misma transacción documento+serie) es una
> mejora futura documentada.

## API

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `POST` | `/empresas` | JWT | Crea empresa (el usuario pasa a Propietario). **201** |
| `GET` | `/empresas` | JWT | Lista las empresas del usuario, con su rol. |
| `POST` | `/empresas/{id}/seleccionar` | JWT | Devuelve un token con el alcance de la empresa. |
| `GET` | `/empresas/actual` | JWT + empresa | Empresa activa. |
| `GET` | `/series` | JWT + empresa | Series de la empresa activa. |
| `POST` | `/series` | permiso `empresa.ajustes` | Crea una serie. |
| `GET` | `/usuarios` | permiso `usuario.gestionar` | Miembros de la empresa (usuario + rol). |
| `POST` | `/usuarios/invitar` | permiso `usuario.gestionar` | Invita a un usuario con un rol. |
| `POST` | `/usuarios/{id}/rol` | permiso `usuario.gestionar` | Cambia el rol de un miembro. |
| `POST` | `/usuarios/{id}/revocar` | permiso `usuario.gestionar` | Revoca el acceso de un miembro. |

## Gestión de usuarios (membresías)

Los endpoints `/usuarios` (en `EndpointsUsuarios`) **orquestan** Identidad y Organización: listar los
miembros de la empresa activa (membresía + datos del usuario), **invitar** (si el correo no existe se
crea el usuario con una contraseña aleatoria y un token de restablecimiento para que fije la suya),
**cambiar el rol** y **revocar** el acceso. No puedes revocarte a ti mismo.

## Roles

Hay tres roles fijos, Propietario, Usuario y Solo lectura (`AlxorCore.Nucleo.Autorizacion.Rol`). Además, cada
empresa puede crear **roles propios por puesto de trabajo** (`/roles`, permiso `usuario.gestionar`; *Ajustes →
Roles*).

- **Plantillas** (`GET /roles/plantillas`): Báscula, Confección, Expedición, Jefe de almacén, Calidad, Técnico de
  campo, Administración, Comercial y Dirección. Se crea el rol desde una plantilla (`{ plantilla: "bascula" }`) y
  luego se ajustan sus permisos.
- **Permisos** (`GET /roles/permisos`): el catálogo completo con su área y lo que deja hacer
  (`CatalogoPermisos`). Una prueba exige que cada permiso tenga descripción.
- **Código del rol:** `rol_` + su identificador. Se asigna a los miembros como un rol fijo. Al elegir la empresa,
  el token lleva los permisos del rol, y un cambio de permisos rige cuando la persona vuelve a entrar.
- **Reglas:**
  - No se repite el nombre del rol en una empresa.
  - Solo se admiten permisos del catálogo.
  - Un rol con miembros activos no se borra (`rol.en_uso`).
- **Base de datos:**
  - Un miembro solo puede tener un rol fijo o uno propio de **su** empresa (`rol.desconocido`).
  - Un rol propio con miembros activos no se borra.
- **Permisos de agro por puesto:** `agro.recepcionar`, `agro.confeccionar`, `agro.expedir`, `agro.calidad` y
  `agro.campo`. Cada operación acepta el suyo o `agro.gestionar`, que sigue dándolo todo, así que los roles de
  antes no cambian. `agro.corregir` es aparte: rectificar, anular o corregir salidas y aprobar mermas.
- La tabla `rol_empresa` no tiene RLS: se lee al elegir empresa, como la membresía, y siempre se filtra por empresa.

## Modelo y persistencia

- Esquema **`organizacion`**: `empresa`, `membresia`, `rol_empresa`, `serie_numeracion`.
- `Nif` (value object) valida DNI, NIE y CIF con su dígito/letra de control.
- Índices únicos: `empresa.nif`, `(membresia.usuario_id, empresa_id)`,
  `(serie.empresa_id, tipo_documento, ejercicio, prefijo)`.
- Migración: `MigracionInicialOrganizacion` (incluye la activación de RLS sobre `serie_numeracion`).

## Autorización compartida

Los roles y permisos viven en el Núcleo (`AlxorCore.Nucleo.Autorizacion`) porque los comparten
Identidad (emisión del token), Organización (resolución de permisos al seleccionar empresa) y la API
(policies `RequierePermiso` y `RequiereAlgunPermiso`).

## Tests

- **Unitarios**: `Nif` (DNI/NIE/CIF válidos e inválidos), `SerieNumeracion` (correlatividad y
  formato), `Empresa` y `Membresia`.
- **Integración**: crear → listar → seleccionar → consultar empresa; crear/listar series; y
  **aislamiento multiempresa** (un usuario no puede seleccionar ni ver la empresa de otro).

## Alta completa de una empresa

`POST /empresas` acepta, además de NIF, razón social, dirección, régimen de IVA y grupo, todos los datos que luego se
cambian en Ajustes: `territorioFiscal` (Comun / Canarias), `telefono`, `email`, `web`, `iban`, `identificadorAcreedor`,
`edicion` y `modulosAdicionales`, `metodoValoracion` y `controlRiesgo`. El alta y esos datos van en **una sola
transacción**: si alguno no vale (un IBAN con el control mal, una edición que no existe…) no se crea la empresa.

La pantalla (**+ Nueva empresa**) lo pide por secciones —datos fiscales, domicilio, contacto, banco y SEPA, plan,
almacén y riesgo, grupo— y rellena lo que se deduce:

- la **forma jurídica** por la letra del NIF, y con ella el modo de contabilidad por defecto (completa para sociedades,
  sencilla para personas físicas);
- la **provincia** y el **territorio fiscal** por el código postal (35 y 38: Canarias, IGIC);
- el **identificador de acreedor SEPA** a partir del NIF (`ES` + control + `000` + NIF), que se puede cambiar;
- la comprobación del **IBAN** mientras se escribe.

Con una empresa abierta se puede crear la nueva en su mismo grupo y copiarle su configuración en el mismo paso.

## Ficha fiscal y calendario de vencimientos

Cada empresa tiene una **ficha fiscal** (`PerfilFiscal`, columna `perfil_fiscal` jsonb de `organizacion.empresa`), en
**Ajustes → Datos fiscales** y en el alta:

- **Identificación**: nombre comercial, CNAE (4 cifras), epígrafe del IAE, fecha de constitución e inicio de actividad,
  mes en que empieza el ejercicio, datos registrales (Registro Mercantil, tomo, folio, hoja) y administradores o
  representantes (nombre, NIF, cargo).
- **Cómo tributa**: periodicidad (trimestral o mensual), gran empresa, REDEME, criterio de caja y SII con su fecha de
  alta. Las grandes empresas y las del REDEME tienen que declarar cada mes y estar en el SII (si no, 400).
- **Modelos que presenta**, del catálogo `PerfilFiscal.Catalogo`: 303, 390, 420, 425, 349, 347, 111, 190, 115, 180,
  123, 193, 130, 131, 200, 202, 184 y 232. `GET /modelos-fiscales?nif=&territorio=&sii=` propone los habituales: el
  impuesto de su territorio (303/390 o 420/425), 347 y 390 si no está en el SII, 111/190, y 200/202 si es sociedad,
  184 si es una entidad en atribución de rentas o 130 si es persona física.
- La **prorrata** sigue siendo por ejercicio (`/impuestos/prorrata/{ejercicio}`); el alta y `PUT /empresas/actual/perfil-fiscal`
  aceptan la del año en curso (`prorrata: { regimen, porcentajeProvisional }`; régimen nulo la quita).

`GET /empresas/actual/calendario-fiscal?anio=` da los **vencimientos del año** de los modelos marcados con los plazos
generales de la AEAT: trimestrales del 1 al 20 de abril, julio y octubre y hasta el 30 de enero el 4T (el 20 las
retenciones); mensuales hasta el 20 del mes siguiente (el 303 y el 420, hasta el último día), julio en septiembre;
390 y 425 el 30 de enero, 190/180/193/184 el 31, 347 en febrero, 200 en los 25 días siguientes a los seis meses del
cierre, 202 en abril, octubre y diciembre y 232 el mes siguiente a los diez meses del cierre. Un plazo que acaba en fin
de semana pasa al lunes; los festivos no se tienen en cuenta. La pantalla **Contabilidad → Calendario fiscal** lo
enseña con su estado (pasado, en plazo, próximo), y **Libros de IVA y modelos** solo muestra los modelos que presenta la
empresa, con los próximos vencimientos arriba.

### Mismo NIF en la Península y en Canarias

Una empresa con establecimientos en los dos territorios marca en su ficha **«Opera en la Península y en Canarias con el
mismo NIF»** (`operaEnAmbosTerritorios`). Entonces:

- cada factura (y ticket) va con **IVA o con IGIC según los tipos de sus líneas**: las ventas desde Canarias con IGIC 7,
  3, 0…, las de la Península con IVA 21, 10, 4… (sin tipos, con el de su territorio principal). Una factura **nunca mezcla**
  los dos impuestos: se hace una por territorio, y conviene una serie para cada uno;
- `/tipos-iva` ofrece los tipos de los dos impuestos y la pantalla los propone en las líneas;
- el **303** suma solo lo facturado con IVA y el **420** solo lo facturado con IGIC; los gastos ya iban por el código de
  cada línea. Impuestos enseña los dos modelos, y la propuesta de modelos incluye 303/390 y 420/425;
- al **SII de la AEAT** solo van las facturas y los gastos con IVA; los del IGIC son de la Agencia Tributaria Canaria.

El territorio de la empresa sigue siendo el principal (el de su domicilio fiscal y el impuesto por defecto).
