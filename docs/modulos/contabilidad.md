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

## Asientos de cobros, pagos y anticipos (modo Completo)

Tesorería deja cada movimiento en su bandeja de salida (en la misma transacción que el cobro o el pago) y el despachador
lo entrega a la cola de contabilización, con las mismas reglas que las facturas: en modo Simple no hay asiento; en
modo Completo queda pendiente o, con la contabilización automática, se contabiliza en el acto.

| Operación | Debe | Haber |
|---|---|---|
| Cobro de una factura o de un efecto a cobrar | 572 Bancos (570 Caja si el método es efectivo) | Cliente (su subcuenta 430…) |
| Pago de un gasto o de un efecto a pagar | Proveedor (su subcuenta 400…/410…) | 572 Bancos (570 Caja en efectivo) |
| Anticipo recibido de un cliente | 572 / 570 | 438 Anticipos de clientes |
| Aplicación del anticipo a una factura | 438 Anticipos de clientes | Cliente |
| Anulación de cualquiera de ellos | el contraasiento (debe y haber cambiados) | |

Los asientos llevan el origen `Cobro` o `Pago` y, como los de las facturas, se anulan anulando el movimiento (no a mano).
La cartera migrada cobra contra la subcuenta del cliente que abrió el asiento de apertura, así que ambas cuadran.

## Reglas de contabilización (cuenta por familia / tipo)

La cuenta de resultado (ingreso 7xx en ventas, gasto 6xx en compras) se elige con **reglas
configurables** en lugar de una única cuenta genérica. Cada regla fija una cuenta para una
combinación de:

- **Familia del artículo** (campo `familia` del producto; en las ventas, la de cada línea), y/o
- **Tipo del tercero** (campo `tipo` del cliente/proveedor).

La regla **más específica gana**: familia + tipo (3) &gt; familia (2) &gt; tipo (1) &gt; genérica (0).
**Ventas línea a línea.** Cada línea de la factura (o del ticket) va a su propia cuenta de ingreso, por este orden:

1. la cuenta propia de la línea o de su concepto;
2. la regla que encaje con la familia de su artículo;
3. la cuenta de la plantilla;
4. si no hay nada de lo anterior, la **700** si el artículo es un bien y la **705** si es un servicio.

Así, una factura con fruta, envases y portes lleva cada importe a su cuenta. Si al redondear por cuenta el asiento no
cuadra por céntimos, la diferencia va a la cuenta de más importe.

Sin ninguna regla, las compras van a la `629`, salvo las que traen su cuenta en la línea. Las autofacturas de fruta
de agro van a la cuenta de compras de los ajustes de agro (`600` por defecto). El resolutor
`ResolverCuentasReglas` implementa el puerto `IResolverCuentas` que usa `PosterDocumento` al construir
el asiento, de modo que la elección de cuenta es transparente para el resto del flujo. La cuenta de una
regla debe existir en el plan de la empresa.

Ejemplos: ventas de familia «Mercaderías» → `700`; compras de proveedores tipo «Profesional» → `623`;
ventas de familia «Formación» a clientes tipo «Intracomunitario» → una cuenta específica.

## Plantillas de asiento

Como las plantillas de Hispatec, los asientos de los documentos se configuran por datos (`contabilidad.plantilla_asiento`).
Hay una plantilla por sentido (venta, compra, cobro, pago) y, si se quiere, otra por origen concreto (`Factura`,
`Gasto`, `Movimiento`, `Anticipo`, `EntregaCuenta`…). Gana la del origen; si no la hay, la del sentido.

- **Concepto del asiento**, con variables: `{Referencia}`, `{Tercero}`, `{Fecha}` (dd/mm/aaaa), `{Total}` (1.234,56) y
  `{Origen}`. Sin concepto en la plantilla, sale «Referencia · Tercero».
- **Diario** del asiento. Sin diario, el de su origen.
- **Por papel del apunte**, la cuenta y el concepto:
  - venta: cliente, ingreso, IVA repercutido y retención soportada;
  - compra: proveedor, gasto, IVA soportado, IVA autoliquidado y retención practicada;
  - cobro y pago: tercero y banco o caja.
- **Qué manda más que la plantilla:**
  - la subcuenta propia del tercero y la contrapartida fija del documento (438, 407…);
  - la regla por familia o tipo de tercero y la cuenta propia de la línea;
  - la subcuenta del banco elegido. La plantilla solo cambia la tesorería genérica (570/572).
- **Validación:** las cuentas deben existir en el plan (se siembra si hace falta) y el diario debe estar dado de alta.
  No puede haber dos plantillas para el mismo sentido y origen.
- **Efecto:** vale para los asientos que se generen desde ese momento. Al borrarla, los documentos vuelven a salir como
  siempre.
- **API:** `GET/POST /contabilidad/plantillas`, `PUT/DELETE /contabilidad/plantillas/{id}` y
  `GET /contabilidad/plantillas/esquemas`, que devuelve los papeles de cada asiento con su cuenta y concepto por defecto.
- **Pantalla:** Contabilidad → panel **Plantillas de asiento**, junto a las reglas.
- **Errores:** `plantilla.cuenta_inexistente`, `plantilla.papel`, `plantilla.papel_repetido`, `plantilla.repetida`,
  `plantilla.diario`, `plantilla.cuenta` y `plantilla.concepto`.

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

**Solo se cierran ejercicios terminados**: mientras no haya pasado el 31 de diciembre del ejercicio el cierre
responde 409 `cierre.ejercicio_abierto` (cerrar a mitad de año regularizaría resultados incompletos y abriría el
siguiente con saldos parciales). La interfaz solo ofrece los ejercicios anteriores al actual.

## Asientos manuales y plan de cuentas

`POST /contabilidad/asientos` (asiento manual) exige que **cada cuenta exista en el plan** de la empresa (el plan
básico se siembra si falta): si no, 400 `asiento.cuenta_inexistente` con los códigos desconocidos. La
comprobación es solo de este camino manual; las contabilizaciones automáticas, las importaciones de saldos y la
migración no la aplican. Cada línea admite su propio `concepto`.

El plan se amplía con `POST /contabilidad/cuentas` `{ codigo, nombre }` (201; 409 `cuenta.duplicada` si ya
existe) y se renombra con `PUT /contabilidad/cuentas/{codigo}` (el código no cambia: los apuntes lo
referencian). El plan básico incluye además `100` Capital social, `113` Reservas voluntarias y `752` Ingresos por
arrendamientos.

En la interfaz clásica, Contabilidad tiene selector de **ejercicio** (diario, balance, PyG, cuentas anuales…),
el modal de asiento autocompleta la cuenta con el plan, muestra el cuadre debe/haber en vivo y permite dar de alta
una cuenta que falte; las filas del balance abren el **libro mayor** de la cuenta
(`GET /contabilidad/mayor/{codigo}?ejercicio=`), filtrable por fechas con saldo anterior.

## Diarios de asientos

El libro diario es **uno** por empresa y ejercicio, con numeración correlativa sin huecos. Además, cada asiento va a
un **diario** (serie) y lleva su **número dentro del diario**, correlativo por ejercicio. Sirven para listar y
revisar por tipo de operación (el `AsientosSerieNumero` de Hispatec). No son libros paralelos.

Diarios de sistema, que existen siempre:

| Código | Diario | Recoge |
|---|---|---|
| `GEN` | General | Asientos manuales (y lo que no tenga otro) |
| `VEN` | Ventas | Facturas y tickets de venta |
| `COM` | Compras | Facturas de compra y gastos |
| `TES` | Cobros y pagos | Cobros, pagos y anticipos |
| `INM` | Inmovilizado | Amortizaciones, impuesto diferido, bajas y ventas de inmovilizado |
| `PER` | Periodificaciones | Reclasificaciones, cuotas y cancelaciones de periodificaciones |
| `CIE` | Regularización y cierre | Regularización y cierre del ejercicio |
| `APE` | Apertura | Apertura del ejercicio |

**Diarios propios** (`/contabilidad/diarios`):

- La empresa crea los suyos, por ejemplo `BAN` para bancos o `REG` para regularizaciones.
- Cada diario propio puede **recoger los asientos de unos orígenes** en lugar de su diario de sistema: manuales,
  ventas, compras, cobros, pagos o inmovilizado. Cada origen va a un solo diario propio.
- Un asiento manual puede indicar su diario; si no lo indica, va al de los manuales.
- Un **contraasiento** va siempre al diario del asiento que anula.
- Un diario con asientos no se elimina: se da de baja, y sus orígenes vuelven a su diario de sistema.

La base de datos asigna el diario al dar de alta el asiento (trigger `contabilidad.asiento_diario`), así que lo
cumplen todos los orígenes. Al migrar, los asientos existentes recibieron su diario por su origen.

El **número dentro del diario no se guarda**: es el orden del asiento, por su número correlativo, en su diario y
ejercicio, y se calcula al leer el libro diario. Es estable porque los asientos no se borran y la numeración solo
crece.

Guardarlo no funcionaba cuando varios asientos se graban juntos (las cuotas de periodificación, el cierre del
ejercicio, la amortización mensual). La base de datos los recibe en el orden de su clave, no en el de su número, y
el número del diario salía desordenado.

## Cierre mensual

Al presentar el IVA de un mes, se **cierra** (`POST /contabilidad/periodos/cerrar` con `{anio, mes}`). Desde entonces
no se registra **ningún asiento con fecha de ese mes ni anterior**: ni manual, ni de documentos, ni amortizaciones,
ni anulaciones (409 `asiento.periodo_cerrado`). La regularización y el cierre del ejercicio sí se registran a
31/12.

- Se cierra **hasta** un mes, sin huecos: cerrar marzo cierra también enero y febrero. No se cierra hacia atrás.
- Si hay **documentos pendientes de contabilizar** con fecha hasta ese mes, el cierre pide confirmación (409
  `periodo.pendientes`; con `forzar: true` cierra igualmente). Esos documentos se contabilizan después con otra fecha
  de registro, o reabriendo el mes.
- También pide confirmación si quedan **cuotas de periodificación** sin generar hasta ese mes.
- Con **contabilización automática**, un documento con fecha de un mes cerrado se registra igual, pero su asiento se
  queda **pendiente**.
- **Reabrir** (`POST /contabilidad/periodos/reabrir`) reabre desde un mes y todos los posteriores. Los meses de un
  ejercicio ya cerrado no se reabren.
- `GET /contabilidad/periodos?ejercicio=` da los doce meses con su estado, sus asientos y sus pendientes (pantalla
  «Contabilidad» → «Cierre mensual»).

La fecha de cierre es `config_contabilidad.cerrado_hasta`. El mismo trigger de alta la comprueba, de modo que ningún
camino puede saltársela.

## Periodificaciones

Un gasto o un ingreso que corresponde a varios meses se lleva a resultados **mes a mes** (el
`AsientosPeriodificacion` de Hispatec). Ejemplos: un seguro anual pagado en marzo, una suscripción, un alquiler
cobrado por adelantado. Pantalla «Contabilidad» → «Periodificaciones»; API `/contabilidad/periodificaciones`.

**Alta**: descripción, tipo (gasto o ingreso), cuenta de resultados (grupo 6 o 7), importe, fecha, meses (2 a 120) y
primer mes (por defecto, el de la fecha). La cuenta de periodificación es por defecto la **480** (gastos anticipados)
o la **485** (ingresos anticipados).

- Con **reclasificar** (por defecto), un asiento en la fecha saca el importe de la cuenta de resultados, donde lo
  dejó la factura, y lo lleva a la 480/485. Gasto: 480 al debe, 6xx al haber. Ingreso: 7xx al debe, 485 al haber.
- Sin reclasificar, se entiende que la factura ya se contabilizó en la 480/485.

**Cuotas**: el importe entre los meses; la última recoge el redondeo (1.000 en 3 meses: 333,33 + 333,33 + 333,34).

- «Generar cuotas» (`POST …/generar` con `{hasta}`) contabiliza las pendientes hasta esa fecha, con un asiento al
  **último día de cada mes**. Gasto: 6xx al debe, 480 al haber. Ingreso: 485 al debe, 7xx al haber.
- Cada cuota se contabiliza una sola vez. Las de meses o ejercicios cerrados no se generan: se cuentan aparte en la
  respuesta.
- Con la última cuota, la periodificación queda **Terminada**.

**Cancelar** (`POST …/{id}/cancelar` con `{fecha}`): termina antes. Lo pendiente va a resultados de una vez.

**Anular** (`POST …/{id}/anular` con `{fecha}`): contraasientos de todos sus asientos en esa fecha. Todo vuelve a
donde estaba, y la fecha no puede ser anterior a sus asientos. Sus asientos no se anulan uno a uno desde el diario.

**Eliminar**: solo si no tiene asientos.

Todos sus asientos van al diario `PER`, con origen `Periodificacion`, y respetan el cierre mensual.

## Invariantes

- **Cuadre**: un asiento no se crea si la suma del debe ≠ la suma del haber, o si su importe es cero.
- **Apunte válido**: cada apunte carga en el debe **o** abona en el haber (no ambos ni ninguno), con
  importes no negativos. Mínimo dos apuntes.
- **Inmutable**: un asiento no se edita una vez creado (las correcciones se hacen con otro asiento).
- **Mes cerrado**: no admite asientos con su fecha (409 `asiento.periodo_cerrado`, ver «Cierre mensual»).
- **Ejercicio cerrado**: una vez generado el asiento de cierre, el ejercicio **no admite nuevos
  asientos** (409 `asiento.ejercicio_cerrado`), ni manuales ni de contabilización. La contabilidad de
  ese año queda congelada.

## API

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/contabilidad/cuentas` | `contabilidad.leer` | Plan de cuentas (se siembra si falta). |
| `GET` | `/contabilidad/diario?ejercicio=&diario=` | `contabilidad.leer` | Libro diario del ejercicio; con `diario`, solo ese diario, por su número. |
| `GET` | `/contabilidad/diarios` | `contabilidad.leer` | Diarios de sistema y propios, con lo que recoge cada uno. |
| `POST` | `/contabilidad/diarios` | `contabilidad.gestionar` | Crea un diario propio. **201** |
| `PUT` | `/contabilidad/diarios/{id}` | `contabilidad.gestionar` | Nombre, orígenes y alta o baja de un diario propio. |
| `DELETE` | `/contabilidad/diarios/{id}` | `contabilidad.gestionar` | Elimina un diario propio sin asientos. **204** |
| `GET` | `/contabilidad/periodificaciones` | `contabilidad.leer` | Periodificaciones con sus cuotas. |
| `POST` | `/contabilidad/periodificaciones` | `contabilidad.gestionar` | Alta (con reclasificación a 480/485 si se pide). **201** |
| `POST` | `/contabilidad/periodificaciones/generar` | `contabilidad.gestionar` | Contabiliza las cuotas pendientes hasta `{hasta}`. |
| `POST` | `/contabilidad/periodificaciones/{id}/cancelar` | `contabilidad.gestionar` | Lo pendiente, a resultados en `{fecha}`. |
| `POST` | `/contabilidad/periodificaciones/{id}/anular` | `contabilidad.gestionar` | Contraasientos de todos sus asientos en `{fecha}`. |
| `DELETE` | `/contabilidad/periodificaciones/{id}` | `contabilidad.gestionar` | Elimina una sin asientos. **204** |
| `GET` | `/contabilidad/periodos?ejercicio=` | `contabilidad.leer` | Meses del ejercicio: cerrados o abiertos, asientos y pendientes. |
| `POST` | `/contabilidad/periodos/cerrar` | `contabilidad.gestionar` | Cierra hasta un mes (`{anio, mes, forzar}`). |
| `POST` | `/contabilidad/periodos/reabrir` | `contabilidad.gestionar` | Reabre desde un mes (`{anio, mes}`). |
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
- **Bandeja de salida (outbox transaccional)**: al emitir una factura o un ticket, o al registrar un
  gasto, el documento a contabilizar se guarda como `mensaje_salida` en la **misma transacción** que el
  documento de origen (cada módulo tiene su propia tabla `mensaje_salida` en su esquema), de modo que
  emitir/registrar y encolar su contabilización son **atómicos**. Un despachador procesa los mensajes
  pendientes tras confirmar la operación y en cada operación posterior (reintento «al menos una vez»),
  por lo que ningún documento queda sin su contabilización aunque el destino falle en ese instante; el
  consumo es idempotente. El ciclo de venta (pedido → factura) hereda el outbox al facturar vía
  `EmitirFactura`. La serialización de la carga se comparte en `AlxorCore.Nucleo` (`SalidaJson`). Mejora
  futura: un despachador en segundo plano para recuperar mensajes atascados sin esperar a la siguiente
  operación.

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

## Rendimiento

Los informes contables (balance de sumas y saldos, libro mayor agregado, PyG, balance de situación,
cuentas anuales y modelo 200) calculan los saldos **agregando por cuenta con `GROUP BY` en la base
de datos** (`SaldosAgregadosAsync`), en lugar de cargar todos los apuntes del ejercicio en memoria.
El consumo de memoria queda acotado por el **número de cuentas**, no por el volumen de asientos, de
modo que el coste no crece con el tamaño de la contabilidad. Del mismo modo, el control de ejercicio
cerrado usa un `EXISTS` (`TieneCierreAsync`) en lugar de traer los asientos de cierre.

## Futuro (documentado)

Modelo oficial de PyG y balance con todos los epígrafes normalizados del PGC (activo/pasivo/PN
abreviado y normal), y numeración de asientos 100 % sin huecos ante fallos.
