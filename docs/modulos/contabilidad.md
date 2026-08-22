# Módulo Contabilidad (partida doble)

Contabilidad por **partida doble**: plan de cuentas (PGC), **asientos** que cuadran, **libro diario**,
**libro mayor** y **balance de sumas y saldos**. Es la "capa 2" de la contabilización: se enchufa en
el puerto `IContabilizador` del módulo Recepción, de modo que contabilizar una factura de proveedor
puede además generar su asiento, según el **modo de contabilidad** de la empresa. Multiempresa (RLS).

## Modo de contabilidad (por empresa)

Cada empresa elige su modo (`GET`/`PUT /contabilidad/modo`):

- **Simple** (por defecto): contabilizar = registrar un **gasto** con IVA soportado (Libro de IVA,
  303/130). Es lo que necesita un autónomo. No genera asientos ni usa el panel de pendientes.
- **Completo**: además del gasto, genera el **asiento** de partida doble (libro diario y mayor).

El adaptador `ContabilizadorSegunModo` implementa el puerto `IContabilizador` de Recepción y registra
el gasto; el **asiento** ya no se genera aquí en línea, sino a través de la **cola de
contabilización** (ver abajo). Así, activar la partida doble **no cambia** el flujo de recepción ni el
de gastos.

## Contabilización diferida y panel del contable

Cumpliendo la petición de que **las facturas no contabilicen por defecto**, en modo Completo cada
documento (factura emitida, gasto, factura recibida) se **encola** como *documento pendiente de
contabilizar* en vez de asentarse al instante. El contable dispone de un **panel único**
(`GET /contabilidad/pendientes`) donde:

- revisa cada documento (venta/compra, tercero, base, IVA, total);
- ajusta la **fecha de registro** —editable, imprescindible para las **facturas recibidas** que
  llegan fuera de plazo y deben registrarse en otro periodo— con
  `PUT /contabilidad/pendientes/{id}/fecha-registro`;
- **contabiliza** los que elija de una vez (`POST /contabilidad/pendientes/contabilizar`), generando
  cada asiento con su fecha de registro (el ejercicio se deriva de esa fecha).

El puerto compartido `IColaContabilizacion` (en `AlxorCore.Nucleo`) recibe los documentos desde
Facturación, Gastos, Recepción y Compras sin que esos módulos conozcan Contabilidad. Su
implementación (`EncolarDocumento`) crea el pendiente y —solo si la empresa activó la
**contabilización automática** (`PUT /contabilidad/contabilizacion-automatica`)— genera el asiento en
el acto. En modo Simple la cola es un no-op (esas empresas solo llevan Libro de IVA).

## Asiento de una factura de proveedor (modo Completo)

Para una factura de base `B`, IVA `i %` y retención `r %`:

| Cuenta | Debe | Haber |
|---|---|---|
| `629` Otros servicios (gasto) | `B` | |
| `472` H.P. IVA soportado | `B · i%` | |
| `4751` H.P. acreedora por retenciones | | `B · r%` |
| `400` Proveedores | | `B + B·i% − B·r%` |

## Asiento de una factura de venta (modo Completo)

| Cuenta | Debe | Haber |
|---|---|---|
| `430` Clientes | `B + B·i% − B·r%` | |
| `473` H.P. retenciones y pagos a cuenta | `B · r%` | |
| `705` Prestaciones de servicios (ingreso) | | `B` |
| `477` H.P. IVA repercutido | | `B · i%` |

Ambos asientos **cuadran** por construcción (Σ debe = Σ haber). El número es correlativo por empresa y
ejercicio (el ejercicio se deriva del año de la **fecha de registro**).

## Reglas de contabilización (cuenta por familia / tipo)

La cuenta de resultado (ingreso 7xx en ventas, gasto 6xx en compras) se elige con **reglas
configurables** en lugar de una única cuenta genérica. Cada regla fija una cuenta para una
combinación de:

- **Familia del artículo** (campo `familia` del producto; se toma la del primer artículo de la venta), y/o
- **Tipo del tercero** (campo `tipo` del cliente/proveedor).

La regla **más específica gana**: familia + tipo (3) &gt; familia (2) &gt; tipo (1) &gt; genérica (0).
Si ninguna regla encaja se usa la cuenta genérica (`705` ventas, `629` compras). El resolutor
`ResolverCuentasReglas` implementa el puerto `IResolverCuentas` que usa `PosterDocumento` al construir
el asiento, de modo que la elección de cuenta es transparente para el resto del flujo. La cuenta de una
regla debe existir en el plan de la empresa.

Ejemplos: ventas de familia «Mercaderías» → `700`; compras de proveedores tipo «Profesional» → `623`;
ventas de familia «Formación» a clientes tipo «Intracomunitario» → una cuenta específica.

## Invariantes

- **Cuadre**: un asiento no se crea si la suma del debe ≠ la suma del haber, o si su importe es cero.
- **Apunte válido**: cada apunte carga en el debe **o** abona en el haber (no ambos ni ninguno), con
  importes no negativos. Mínimo dos apuntes.
- **Inmutable**: un asiento no se edita una vez creado (las correcciones se hacen con otro asiento).

## API

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/contabilidad/cuentas` | `contabilidad.leer` | Plan de cuentas (se siembra si falta). |
| `GET` | `/contabilidad/diario?ejercicio=` | `contabilidad.leer` | Libro diario del ejercicio. |
| `GET` | `/contabilidad/mayor/{codigo}?ejercicio=` | `contabilidad.leer` | Libro mayor de una cuenta (con saldo acumulado). |
| `GET` | `/contabilidad/balance?ejercicio=` | `contabilidad.leer` | Balance de sumas y saldos. |
| `POST` | `/contabilidad/asientos` | `contabilidad.gestionar` | Crea un asiento manual. **201** |
| `GET` | `/contabilidad/modo` | `contabilidad.leer` | Modo de contabilidad actual. |
| `PUT` | `/contabilidad/modo` | `contabilidad.gestionar` | Cambia el modo (Simple / Completo). |
| `GET` | `/contabilidad/config` | `contabilidad.leer` | Modo + si contabiliza automáticamente. |
| `PUT` | `/contabilidad/contabilizacion-automatica` | `contabilidad.gestionar` | Activa/desactiva la contabilización automática. |
| `GET` | `/contabilidad/pendientes` | `contabilidad.leer` | Documentos pendientes de contabilizar (panel). |
| `PUT` | `/contabilidad/pendientes/{id}/fecha-registro` | `contabilidad.gestionar` | Cambia la fecha de registro de un pendiente. **204** |
| `POST` | `/contabilidad/pendientes/contabilizar` | `contabilidad.gestionar` | Contabiliza (asienta) los pendientes indicados. |
| `GET` | `/contabilidad/reglas` | `contabilidad.leer` | Reglas de contabilización (cuenta por familia/tipo). |
| `POST` | `/contabilidad/reglas` | `contabilidad.gestionar` | Crea una regla. **201** |
| `PUT` | `/contabilidad/reglas/{id}` | `contabilidad.gestionar` | Actualiza una regla. |
| `DELETE` | `/contabilidad/reglas/{id}` | `contabilidad.gestionar` | Elimina una regla. **204** |

## Plan de cuentas

Se siembra por empresa un subconjunto común del PGC (400, 430, 472, 477, 475, 4751, 570, 572, 600,
621–629, 700, 705…) la primera vez que se consulta el plan o se genera un asiento. Es ampliable (los
asientos referencian cuentas por su código; el plan da el nombre para los informes).

## Persistencia

- Esquema **`contabilidad`**: `cuenta`, `asiento` (con `apunte` como colección propia),
  `config_contabilidad`, `documento_pendiente` y `regla_contabilizacion`.
- RLS por empresa en `cuenta`, `asiento`, `config_contabilidad`, `documento_pendiente` y
  `regla_contabilizacion`; el `apunte` se protege a través de su `asiento` (filtro global de EF Core).
- Índices: únicos `(empresa_id, codigo)` en cuenta y `(empresa_id, ejercicio, numero)` en asiento;
  `(empresa_id, estado)` en `documento_pendiente`; `(empresa_id, sentido)` en `regla_contabilizacion`.
- `config_contabilidad` incorpora la columna `contabilizacion_automatica` (por defecto `false`).
- Migraciones: `MigracionInicialContabilidad`, `ContabilizacionDiferida` (tabla `documento_pendiente`
  + columna `contabilizacion_automatica` + RLS) y `ReglasContabilizacion` (tabla
  `regla_contabilizacion` + RLS).

## Composición

- Recepción define el puerto `IContabilizador`; Contabilidad lo implementa con `ContabilizadorSegunModo`
  (registrado **después** de Recepción para sustituir el adaptador por defecto). Hoy solo registra el
  gasto; el asiento llega por la cola.
- `AlxorCore.Nucleo` define el puerto `IColaContabilizacion`; Contabilidad lo implementa con
  `EncolarDocumento`. Facturación, Gastos, Recepción y Compras **encolan** sus documentos sin conocer
  este módulo. El asiento se genera al contabilizar desde el panel (o al encolar si la contabilización
  automática está activa). No es una transacción única entre módulos (mejora futura: outbox).

## Tests

- **Unitarios**: cuadre del asiento (cuadrado/descuadrado, apunte debe-y-haber, mínimo de apuntes),
  validación del código de cuenta, el `documento_pendiente` (fecha de registro editable solo mientras
  está pendiente; no se recontabiliza) y la resolución de reglas (regla válida, cuenta genérica sin
  reglas, gana la más específica, ventas y compras no se mezclan).
- **Integración**: modo por defecto Simple; alta de asiento manual y su aparición en el diario;
  asiento descuadrado → 400; una compra queda **pendiente** sin asiento por defecto; el contable ajusta
  la fecha de registro y contabiliza desde el panel (asiento 629/472 al debe, 400 al haber); con
  contabilización automática se asienta al instante; una **venta** genera el asiento de ingreso
  (430 al debe; 705/477 al haber); y en modo Simple no se crean pendientes.

## Futuro (documentado)

Ejercicios con cierre/apertura, cuentas de resultados (PyG) y balance de situación clasificado,
amortizaciones, y numeración de asientos 100 % sin huecos ante fallos.
