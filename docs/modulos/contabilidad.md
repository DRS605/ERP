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

## Cuenta de Pérdidas y Ganancias

`GET /contabilidad/pyg?ejercicio=` calcula la **cuenta de resultados** a partir del libro diario:

- **Ingresos** = cuentas del **grupo 7** por su saldo acreedor (`haber − debe`).
- **Gastos** = cuentas del **grupo 6** por su saldo deudor (`debe − haber`).
- **Resultado del ejercicio** = total ingresos − total gastos (positivo = beneficio, negativo = pérdida).

Es una vista analítica por cuenta (no el modelo oficial con todos los epígrafes normalizados); las
líneas con importe cero se omiten.

## Balance de situación

`GET /contabilidad/balance-situacion?ejercicio=` clasifica las cuentas patrimoniales por **masas** del
PGC (simplificación por grupo/subgrupo, no el modelo oficial):

- **Activo no corriente** (grupo 2, inmovilizado) y **activo corriente** (grupo 3 existencias, grupo 5
  tesorería con saldo deudor, deudores del grupo 4).
- **Patrimonio neto** (subgrupos 10–13 y la cuenta de resultado 129) y **pasivo** (resto del grupo 1,
  acreedores del grupo 4, grupo 5 con saldo acreedor).
- Las cuentas de gestión (grupos 6 y 7) **no** van al balance.

Para que **cuadre también antes del cierre**, si la cuenta `129` aún no tiene saldo se añade una línea
provisional *«Resultado del ejercicio»* en el patrimonio neto por el resultado de la PyG (como hace
cualquier software contable). El campo `cuadra` indica si el total activo iguala al total de patrimonio
neto + pasivo.

## Cierre de ejercicio

`POST /contabilidad/cierre?ejercicio=` cierra un ejercicio en **tres asientos** (solo modo Completo, y de
forma transaccional):

1. **Regularización** (fecha 31/12, origen `Regularizacion`): salda los grupos 6 y 7 contra la cuenta de
   resultado `129`. El saldo de la 129 pasa a ser el beneficio (acreedor) o la pérdida (deudor).
2. **Cierre** (fecha 31/12, origen `Cierre`): salda **todas** las cuentas patrimoniales (grupos 1–5,
   incluida la 129) dejándolas a cero.
3. **Apertura** (fecha 1/1 del ejercicio siguiente, origen `Apertura`): reabre esos mismos saldos con el
   asiento inverso, de modo que el nuevo ejercicio arranca con el balance de cierre.

La respuesta (`CierreEjercicioDto`) devuelve el `resultado` y los ids de los tres asientos. **No se puede
cerrar dos veces** (409 `cierre.ya_cerrado`) ni cerrar un ejercicio **sin movimientos**
(400 `cierre.sin_movimientos`).

## Invariantes

- **Cuadre**: un asiento no se crea si la suma del debe ≠ la suma del haber, o si su importe es cero.
- **Apunte válido**: cada apunte carga en el debe **o** abona en el haber (no ambos ni ninguno), con
  importes no negativos. Mínimo dos apuntes.
- **Inmutable**: un asiento no se edita una vez creado (las correcciones se hacen con otro asiento).
- **Ejercicio cerrado**: una vez generado el asiento de cierre, el ejercicio **no admite nuevos
  asientos** (409 `asiento.ejercicio_cerrado`), ni manuales ni de contabilización. La contabilidad de
  ese año queda congelada.

## API

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/contabilidad/cuentas` | `contabilidad.leer` | Plan de cuentas (se siembra si falta). |
| `GET` | `/contabilidad/diario?ejercicio=` | `contabilidad.leer` | Libro diario del ejercicio. |
| `GET` | `/contabilidad/mayor/{codigo}?ejercicio=` | `contabilidad.leer` | Libro mayor de una cuenta (con saldo acumulado). |
| `GET` | `/contabilidad/balance?ejercicio=` | `contabilidad.leer` | Balance de sumas y saldos. |
| `GET` | `/contabilidad/pyg?ejercicio=` | `contabilidad.leer` | Cuenta de Pérdidas y Ganancias (grupos 6 y 7). |
| `GET` | `/contabilidad/balance-situacion?ejercicio=` | `contabilidad.leer` | Balance de situación por masas patrimoniales. |
| `POST` | `/contabilidad/cierre?ejercicio=` | `contabilidad.gestionar` | Cierra el ejercicio (regularización + cierre + apertura). |
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

Se siembra por empresa un subconjunto común del PGC (129 Resultado del ejercicio, 400, 430, 472, 477,
475, 4751, 570, 572, 600, 621–629, 700, 705…) la primera vez que se consulta el plan o se genera un
asiento. Es ampliable (los asientos referencian cuentas por su código; el plan da el nombre para los
informes).

## Persistencia

- Esquema **`contabilidad`**: `cuenta`, `asiento` (con `apunte` como colección propia),
  `config_contabilidad`, `documento_pendiente` y `regla_contabilizacion`.
- RLS por empresa en `cuenta`, `asiento`, `config_contabilidad`, `documento_pendiente` y
  `regla_contabilizacion`; el `apunte` se protege a través de su `asiento` (filtro global de EF Core).
- Índices: únicos `(empresa_id, codigo)` en cuenta y `(empresa_id, ejercicio, numero)` en asiento;
  `(empresa_id, estado)` en `documento_pendiente`; `(empresa_id, sentido)` en `regla_contabilizacion`.
- `config_contabilidad` incorpora las columnas `contabilizacion_automatica` (por defecto `false`) y
  `longitud_subcuenta` (por defecto 8).
- `cuenta` incorpora `tercero_id`/`tipo_tercero` (subcuentas de tercero) con índice `(empresa_id, tercero_id)`.
- Además existen las tablas del inmovilizado (`inmovilizado`, `dotacion_amortizacion`,
  `ajuste_fiscal_amortizacion`); ver [`inmovilizado.md`](inmovilizado.md).
- Migraciones: `MigracionInicialContabilidad`, `ContabilizacionDiferida` (tabla `documento_pendiente`
  + columna `contabilizacion_automatica` + RLS), `ReglasContabilizacion` (tabla
  `regla_contabilizacion` + RLS), `Inmovilizado` (tablas del inmovilizado + RLS) y
  `SubcuentasTerceros` (columnas `tercero_id`/`tipo_tercero` en `cuenta` y `longitud_subcuenta`).

## Composición

- Recepción define el puerto `IContabilizador`; Contabilidad lo implementa con `ContabilizadorSegunModo`
  (registrado **después** de Recepción para sustituir el adaptador por defecto). Hoy solo registra el
  gasto; el asiento llega por la cola.
- `AlxorCore.Nucleo` define el puerto `IColaContabilizacion`; Contabilidad lo implementa con
  `EncolarDocumento`. Facturación, Gastos, Recepción y Compras **encolan** sus documentos sin conocer
  este módulo. El asiento se genera al contabilizar desde el panel (o al encolar si la contabilización
  automática está activa). `EncolarDocumento` es **idempotente** por documento de origen (no duplica el
  pendiente aunque el mensaje se reintente).
- **Bandeja de salida (outbox transaccional)**: al emitir una factura, el documento a contabilizar se
  guarda como `mensaje_salida` en la **misma transacción** que la factura (módulo Facturación), de modo
  que emitir la factura y encolar su contabilización son **atómicos**. Un despachador (`DespacharSalida`)
  procesa los mensajes pendientes tras confirmar la factura y en cada emisión posterior (reintento «al
  menos una vez»), por lo que ninguna factura queda sin su contabilización aunque el destino falle en ese
  instante. Es la primera ruta migrada al outbox; el resto de productores (ticket, gastos, ciclo de
  venta) siguen usando la cola directa (mejora futura: migrarlos también y añadir un despachador en
  segundo plano).

## Tests

- **Unitarios**: cuadre del asiento (cuadrado/descuadrado, apunte debe-y-haber, mínimo de apuntes),
  validación del código de cuenta, el `documento_pendiente` (fecha de registro editable solo mientras
  está pendiente; no se recontabiliza) y la resolución de reglas (regla válida, cuenta genérica sin
  reglas, gana la más específica, ventas y compras no se mezclan).
- **Integración**: modo por defecto Simple; alta de asiento manual y su aparición en el diario;
  asiento descuadrado → 400; una compra queda **pendiente** sin asiento por defecto; el contable ajusta
  la fecha de registro y contabiliza desde el panel (asiento 629/472 al debe, 400 al haber); con
  contabilización automática se asienta al instante; una **venta** genera el asiento de ingreso
  (430 al debe; 705/477 al haber); y en modo Simple no se crean pendientes. Cierre: la **PyG** resta
  gastos a ingresos; el **balance de situación** cuadra incluyendo el resultado en el patrimonio neto;
  el **cierre** genera regularización + cierre en el ejercicio y apertura en el siguiente; y no se puede
  cerrar dos veces ni asentar en un ejercicio ya cerrado.

## Subcuentas de tercero (código contable siguiente)

Clientes, proveedores y trabajadores pueden tener su **subcuenta contable individual**, autonumerada
(el «código siguiente») a partir de su cuenta raíz:

- Clientes → raíz **430**, proveedores → **400**, trabajadores → **465**.
- La **longitud** de la subcuenta es configurable por empresa (`PUT /contabilidad/longitud-subcuenta`,
  por defecto **8** dígitos, estándar ContaPlus/a3). Con longitud 8 y raíz 430, el primer cliente es
  `43000001`, el siguiente `43000002`, etc.

El comportamiento depende del **modo de contabilidad**:

- **Simple**: los terceros **comparten la cuenta raíz** (430/400/465); no hay subcuentas individuales.
- **Completo**: cada tercero tiene su **subcuenta propia**. Se sugiere el código siguiente, pero es
  **editable** y no tiene por qué empezar por la raíz (puedes teclear cualquier cuenta).

La subcuenta se registra como una **`Cuenta` del plan** etiquetada con el `tercero_id` (columnas
`tercero_id`/`tipo_tercero`), de modo que aparece en el libro mayor y el balance con el saldo de ese
tercero. Al **contabilizar** una venta o una compra, el asiento usa la subcuenta del tercero si la
tiene asignada; si no, la cuenta raíz genérica.

### API

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/contabilidad/subcuenta-siguiente?tipo=Cliente\|Proveedor\|Trabajador` | `contabilidad.leer` | Sugiere el código siguiente (o la raíz en modo Simple). |
| `GET` | `/contabilidad/subcuenta?tipo=&terceroId=` | `contabilidad.leer` | Subcuenta asignada a un tercero. |
| `GET` | `/contabilidad/subcuentas?tipo=` | `contabilidad.leer` | Subcuentas asignadas de un tipo (para los listados). |
| `PUT` | `/contabilidad/subcuenta` | `contabilidad.gestionar` | Asigna/edita la subcuenta de un tercero (autonumerada o manual). |
| `PUT` | `/contabilidad/longitud-subcuenta` | `contabilidad.gestionar` | Cambia la longitud de subcuenta de la empresa. |

## Cuentas Anuales y modelo 200 (Impuesto de Sociedades)

A partir de los mismos saldos contables se generan (aproximación PGC-Pymes abreviado, mejor esfuerzo a
validar con la gestoría):

- **Cuentas Anuales** (`GET /contabilidad/cuentas-anuales?ejercicio=`): balance de situación
  normalizado por masas y epígrafes (activo no corriente/corriente; patrimonio neto, pasivo no
  corriente/corriente) y cuenta de PyG normalizada con subtotales (resultado de explotación,
  financiero, antes de impuestos y del ejercicio). El balance cuadra por construcción.
- **Modelo 200 / liquidación del IS** (`GET /contabilidad/modelo-200?ejercicio=&tipo=&ajustesAumentos=&ajustesDisminuciones=&deducciones=&retenciones=`):
  resultado contable antes de impuestos (ingresos del grupo 7 − gastos del grupo 6 salvo el 630),
  base imponible (± ajustes extracontables), cuota íntegra al tipo indicado (25 % por defecto), cuota
  líquida (− deducciones) y cuota diferencial (− retenciones y pagos a cuenta; las retenciones se
  toman del saldo deudor de la 473 si no se indican). No es el modelo 200 oficial completo con todas
  sus casillas: es la liquidación calculada desde la contabilidad.

## Inmovilizado y amortizaciones

La amortización del inmovilizado (contable y fiscal, impuesto diferido, baja y enajenación) es un
submódulo de Contabilidad con su propia documentación: ver [`inmovilizado.md`](inmovilizado.md).

## Futuro (documentado)

Modelo oficial de PyG y balance con todos los epígrafes normalizados del PGC (activo/pasivo/PN
abreviado y normal), y numeración de asientos 100 % sin huecos ante fallos.
