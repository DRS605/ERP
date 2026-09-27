# Módulo Tesorería

Registro de **cobros** (contra facturas y efectos) y **pagos** (contra gastos y efectos), totales o parciales,
por **cuentas bancarias y cajas** con su propia subcuenta contable; **remesas SEPA** registradas (adeudos y
transferencias), **devoluciones** de recibos, **conciliación bancaria** persistente con extractos Norma 43 y
**previsión** de tesorería partiendo del saldo real de los bancos.

## Reglas (invariantes)

- **P1 — Sin sobrepago**: la suma de movimientos de un documento no puede superar su total. El caso
  de uso lo comprueba antes de registrar (devuelve 409).
- **P2 — Estado derivado**: el estado (`Pendiente` / `Parcial` / `Liquidado`) se **calcula** a partir
  del total y de lo liquidado, nunca se fija a mano.
- **P3 — Movimientos inalterables**: un cobro o pago no se modifica ni se borra; se anula con un movimiento en
  negativo (`anula_movimiento_id`), también en las devoluciones y al deshacer una conciliación.
- **P4 — Un documento, una remesa viva**: una factura, gasto o efecto que está en una remesa generada o
  presentada no entra en otra (índice único parcial `ux_linea_remesa_documento_viva` sobre las líneas vivas).
- **P5 — Un movimiento, un apunte**: un cobro o pago solo se concilia con un apunte del extracto
  (`ux_casacion_apunte_movimiento`).

## Cuentas bancarias y cajas

`CuentaBancaria` (`tesoreria.cuenta_bancaria`, RLS por empresa): tipo `Banco` o `Caja`, nombre, **IBAN**
(obligatorio en un banco; se normaliza y se valida con la longitud del país y los dígitos de control módulo 97),
BIC (8 u 11), **subcuenta** contable, activa, **predeterminada** (una por empresa, solo un banco activo) y un
saldo inicial opcional (con su fecha) para cuando la empresa no lleva contabilidad completa.

- **Subcuenta**: al dar de alta la cuenta, Contabilidad (puerto `IPlanCuentasTesoreria`, en Núcleo) calcula la
  siguiente libre bajo `572` (bancos) o `570` (cajas) con la longitud de subcuenta de la empresa —igual que las
  subcuentas de tercero— y la crea en el plan (también en modo Simple, para que exista si la empresa pasa a
  Completo). Se puede indicar a mano (debe empezar por 572/570). **No se cambia** una vez creada: los asientos ya
  la usan; una cuenta que deja de usarse se **desactiva**. Solo se **borra** si no tiene movimientos, remesas ni
  extractos.
- **Elección en cobros y pagos**: `POST /cobros`, `/pagos` y `/cartera/{id}/movimientos` aceptan
  `cuentaBancariaId`. Sin ella, `ResolutorCuentaTesoreria` elige la **caja** activa si el método contiene
  «efectivo» y el **banco predeterminado** en lo demás. El movimiento guarda la cuenta
  (`movimiento.cuenta_bancaria_id`) y su asiento va a la subcuenta de esa cuenta (su anulación también).
- **Compatibilidad**: sin cuentas dadas de alta todo funciona como antes: el asiento va a `572`, o a `570` en
  efectivo. Los movimientos antiguos no tienen cuenta.
- Los anticipos siguen yendo a 572/570 genéricas (no eligen banco).

## Remesas SEPA registradas

`Remesa` (`tesoreria.remesa` + `linea_remesa`): tipo `Cobro` (adeudos directos, **pain.008.001.02** / Norma
19.14) o `Pago` (transferencias, **pain.001.001.03** / Cuaderno 34), numerada por tipo y ejercicio
(`2026/3`), fecha de cargo, **cuenta bancaria** (la elegida o la predeterminada; su IBAN y BIC van en el
fichero; sin cuentas, el IBAN de la ficha de la empresa), esquema/secuencia (adeudos), total, el **fichero XML
guardado** (se vuelve a descargar) y las líneas (documento, tercero, IBAN, mandato, importe y el movimiento que
la liquidó).

Estados: **Generada → Presentada → Liquidada** («Cobrada» o «Pagada»), o **Anulada** (solo si no está
liquidada: libera sus documentos). Al **liquidar** (`fecha` = la del abono o cargo del banco, por defecto la de
cargo) se registra **un movimiento por línea** contra la cuenta bancaria de la remesa (método
«Domiciliación remesa 2026/3» o «Transferencia remesa 2026/3»), con su asiento como cualquier cobro o pago
(572… contra la cuenta del cliente o del proveedor). Si algún documento ya no tiene pendiente el importe
remesado (se cobró a mano entretanto), no se registra nada y se devuelve 409 con la lista.

Los adeudos incluyen facturas y **efectos de cartera** a cobrar (de clientes con IBAN y mandato). Se remesa el
pendiente; se omiten informando los documentos anulados, cobrados, sin mandato o **ya en otra remesa viva**.

Las rutas antiguas `POST /tesoreria/remesa` y `POST /tesoreria/transferencias` siguen existiendo (misma
respuesta que antes más `remesaId` y `codigo`), pero ahora **registran** la remesa: no permiten remesar dos veces.

## Devoluciones de recibos

`DevolucionRecibo` (`tesoreria.devolucion_recibo`, solo inserción): se registra desde una línea de una remesa
cobrada o desde cualquier cobro por domiciliación (método «Domiciliación…»), con **motivo SEPA** (códigos R:
AC01, AC04, AC06, AC13, AG01, AG02, AM04, AM05, BE05, FF01, MD01, MD02, MD06, MD07, MS02, MS03, RC01, RR01–RR04,
SL01, con su descripción en `GET /tesoreria/devoluciones/motivos`), fecha, **gastos de devolución** y nota.
Efectos, en una transacción:

1. **Anulación del cobro** (movimiento en negativo, método «Devolución AM04», enlazado al cobro y a la
   devolución): el documento **vuelve a tener pendiente** y aparece en **Impagados** con el motivo, la fecha y los
   gastos (aunque aún no haya vencido).
2. **Contraasiento** del cobro: cliente (430…) al debe, banco (572…) al haber.
3. **Gastos**: `626` (servicios bancarios y similares) al debe, banco al haber. Con `repercutirGastos`, en
   cambio, se cargan al **cliente** (430… al debe) y se crea un **efecto de cartera** a cobrar por ese importe.

Un cobro solo se devuelve una vez; un cobro que no es por domiciliación se anula (409 `devolucion.no_domiciliado`).

## Conciliación bancaria (persistente)

`POST /tesoreria/extractos` importa un fichero **Norma 43** (`ParserNorma43`: registros 11, 22, 23, 33; lee
también fecha valor, concepto común, número de documento y las referencias 1 y 2) y lo **guarda**
(`extracto_bancario` + `apunte_bancario`) contra una cuenta bancaria: la indicada o la que tenga la cuenta del
fichero (entidad + oficina + número dentro del IBAN). El mismo fichero no se importa dos veces en la misma cuenta
(huella SHA-256). Cada apunte tiene estado **Pendiente**, **ConciliadoAutomatico**, **ConciliadoManual** o
**ConAsiento**, y sus casaciones (`casacion_apunte`: movimiento, documento, importe con signo y si el movimiento
lo registró la conciliación).

- **Automática** (`POST /tesoreria/extractos/{id}/conciliar-automatico`, margen de fechas por defecto 5 días),
  por orden: (1) un **cobro o pago ya registrado** en esa cuenta (o sin cuenta, si es la predeterminada) sin
  conciliar, por el mismo importe y fecha dentro del margen; (2) una **remesa liquidada** cuyo total es el apunte
  (se casan todos sus movimientos: N movimientos contra 1 apunte); (3) una **factura, gasto o efecto pendiente**
  de ese importe exacto, registrando su cobro o pago en la cuenta y fecha del apunte. Las **pistas** del
  concepto y las referencias (el número del documento, +3; palabras del nombre del tercero, +1) deshacen empates;
  si sigue habiendo duda, el apunte queda pendiente.
- **Manual** (`POST /tesoreria/apuntes/{id}/conciliar` con `elementos: [{tipo, id, importe?}]`): uno o varios
  movimientos (`Movimiento`), remesas (`Remesa`) o documentos (`Factura`, `Gasto`, `Cartera`, con importe parcial
  opcional) cuya suma con signo (cobros +, pagos −) sea **exactamente** la del apunte. `GET …/candidatos` propone
  los posibles, ordenados por pistas, importe y fecha.
- **Asiento directo** (`POST /tesoreria/apuntes/{id}/asiento` con `cuenta` y `concepto`): comisiones, gastos o
  intereses. Un cargo: la cuenta elegida (626, 669, 662…) al debe y el banco al haber; un abono: el banco al debe y
  la cuenta (769…) al haber. No admite otra cuenta 57x (sería un traspaso).
- **Deshacer** (`POST /tesoreria/apuntes/{id}/deshacer`): anula los cobros o pagos que registró la conciliación
  (con su contraasiento) y el asiento directo, y deja el apunte pendiente. Los movimientos que ya existían solo se
  desenlazan.

Un extracto sin apuntes conciliados se puede borrar. `POST /tesoreria/conciliacion` (sugerencias sin guardar,
por importe exacto) se mantiene por compatibilidad.

## Previsión de tesorería

`GET /cuentas-bancarias/saldos` da el saldo de cada banco y caja a una fecha: el **contable** de su subcuenta
(debe − haber de todos los asientos hasta esa fecha; cierre y apertura se compensan) si la empresa lleva
contabilidad completa, y si no el **calculado con los movimientos** (saldo inicial + cobros − pagos + apuntes
contabilizados directamente, desde la fecha del saldo inicial). Incluye una fila para lo registrado sin cuenta
(570/572 genéricas) cuando no es cero. La pantalla de previsión **parte de ese total** y acumula los vencimientos
reales y las previsiones manuales (`PrevisionTesoreria`: `{ sentido, concepto, importe, fecha }`).

## API

| Método | Ruta | Permiso | Descripción |
|---|---|---|---|
| `POST` | `/cobros` | `cobro.registrar` | Registra un cobro de una factura (`cuentaBancariaId` opcional). |
| `POST` | `/pagos` | `pago.registrar` | Registra un pago de un gasto (`cuentaBancariaId` opcional). |
| `POST` | `/tesoreria/movimientos/{id}/anular` | `cobro.registrar` | Anula un cobro o pago. |
| `GET` | `/facturas/{id}/saldo`, `/gastos/{id}/saldo` | `factura.leer` / `gasto.leer` | Total, liquidado, pendiente, estado y movimientos. |
| `GET` | `/facturas/saldos?ids=`, `/gastos/saldos?ids=` | `factura.leer` / `gasto.leer` | Saldos en lote. |
| `GET` | `/cuentas-bancarias?activas=` | `factura.leer` | Bancos y cajas. |
| `POST` | `/cuentas-bancarias` | `empresa.ajustes` | Alta (crea la subcuenta). **201** |
| `PUT` | `/cuentas-bancarias/{id}` | `empresa.ajustes` | Modifica (la subcuenta no cambia). |
| `DELETE` | `/cuentas-bancarias/{id}` | `empresa.ajustes` | Borra una cuenta sin uso. **204** |
| `GET` | `/cuentas-bancarias/saldos?fecha=` | `factura.leer` | Saldos por cuenta y total (fuente: Contabilidad o Movimientos). |
| `GET` | `/tesoreria/remesas?tipo=` | `factura.leer` | Remesas registradas. |
| `POST` | `/tesoreria/remesas` | `cobro.registrar` | Crea una remesa (`tipo`, `facturaIds`/`efectoIds` o `gastoIds`, `fechaCargo`, `esquema`, `secuencia`, `cuentaBancariaId`). **201** |
| `GET` | `/tesoreria/remesas/{id}` · `/{id}/fichero` | `factura.leer` | Detalle con líneas · fichero XML. |
| `POST` | `/tesoreria/remesas/{id}/presentar` · `/liquidar` · `/anular` | `cobro.registrar` | Cambios de estado. |
| `POST` | `/tesoreria/remesa`, `/tesoreria/transferencias` | `cobro.registrar` / `pago.registrar` | Compatibilidad (registran la remesa). |
| `GET` | `/tesoreria/devoluciones` · `/motivos` | `factura.leer` | Devoluciones · motivos SEPA. |
| `POST` | `/tesoreria/devoluciones` | `cobro.registrar` | Registra una devolución (`movimientoId`, `motivo`, `fecha`, `gastos`, `repercutirGastos`). **201** |
| `GET` | `/tesoreria/extractos?cuentaBancariaId=` · `/{id}` | `factura.leer` | Extractos · detalle con apuntes. |
| `POST` | `/tesoreria/extractos` | `cobro.registrar` | Importa un Norma 43. **201** |
| `DELETE` | `/tesoreria/extractos/{id}` | `cobro.registrar` | Borra un extracto sin conciliar. |
| `POST` | `/tesoreria/extractos/{id}/conciliar-automatico` | `cobro.registrar` | Conciliación automática. |
| `GET` | `/tesoreria/apuntes/{id}/candidatos` | `factura.leer` | Candidatos de casación. |
| `POST` | `/tesoreria/apuntes/{id}/conciliar` · `/asiento` · `/deshacer` | `cobro.registrar` | Casación manual · asiento directo · deshacer. |
| `POST` | `/tesoreria/conciliacion` | `cobro.registrar` | Sugerencias sin guardar (compatibilidad). |
| `GET`/`POST`/`PUT`/`DELETE` | `/tesoreria/previsiones` | `factura.leer` / `cobro.registrar` | Previsiones manuales. |

`/cuentas-bancarias` es de la **base** (los cobros de todas las ediciones eligen banco); el resto de rutas
`/tesoreria/…` son del módulo **Tesorería avanzada**.

## Contabilización

Todo pasa por la **bandeja de salida** del módulo (`mensaje_salida`, misma transacción) y la cola de
contabilización (en modo Simple no hay asientos). `DocumentoContabilizable.CuentaTesoreria` lleva la subcuenta del
banco o caja; `CuentaTercero` la contrapartida fija (438 anticipos, 626 gastos, la elegida en un apunte). Orígenes
nuevos: `DevolucionGastos` y `ApunteBancario`. El plan básico siembra además 626, 662, 669 y 769.

## UI clásica

- **Tesorería → Bancos y cajas** (`vBancos`): alta/edición/borrado, IBAN, BIC, subcuenta, predeterminada y saldo.
- Los modales de cobro, pago y cobro/pago de efectos piden el **banco o caja** (si hay cuentas dadas de alta).
- **Remesas SEPA** (`vRemesa`): remesas registradas con sus acciones (ver, descargar el XML, presentada,
  cobrada/pagada, anular), detalle con **devolución** por línea, y los paneles para crear remesas de adeudos y de
  transferencias eligiendo la cuenta.
- **Cobros/Pagos/Cartera → Movimientos**: botón **Devolución** en los cobros por domiciliación.
- **Impagados**: columna con el motivo de la devolución.
- **Conciliación bancaria** (`vConciliacion`): importar extractos por cuenta, lista de extractos guardados,
  detalle con el estado de cada apunte, conciliación automática, casación manual con suma en vivo, asiento
  directo y deshacer.
- **Previsión de tesorería**: parte del saldo actual de bancos y cajas.

## Persistencia

Esquema **`tesoreria`**: `movimiento` (+ `cuenta_bancaria_id`), `cuenta_bancaria`, `remesa`, `linea_remesa`,
`devolucion_recibo`, `extracto_bancario`, `apunte_bancario`, `casacion_apunte` (migración
`CuentasBancariasRemesasConciliacion`). RLS por empresa (`RlsSql.Activar`) y, en las tablas hijas, por su
cabecera (`GarantiasSql.RlsPorPadre`). Checks de tipo, estado e importes; `devolucion_recibo` es de solo
inserción. Las FK de `linea_remesa` y `casacion_apunte` a `movimiento` son diferidas (se crean en la misma
transacción que el movimiento).

## Composición

Tesorería consulta los totales de los documentos a **Facturación** (`IConsultaFacturas`) y **Gastos**
(`IConsultaGastos`, solo lectura); para las remesas lee **Terceros** (IBAN y mandato) y **Organización**
(identificador de acreedor). Con **Contabilidad** habla por la cola de contabilización y por
`IPlanCuentasTesoreria` (subcuentas y saldos de los bancos).

## Limitaciones

- Una remesa de adeudos se liquida entera; los recibos devueltos se registran después, uno a uno.
- La conciliación no concilia traspasos entre cuentas propias ni divisas; los extractos son en euros.
- Los anticipos no eligen banco; la anulación manual de un movimiento ya conciliado no desconcilia su apunte
  (hay que deshacer la conciliación).
- Gastos de devolución repercutidos: se crea el efecto a cobrar, no se suman al pendiente de la factura.

## Tests

- **Unitarios** (`AlxorCore.Tesoreria.Tests`): importe y estado de saldo, IBAN, cuentas, ciclo de la remesa,
  casación de apuntes, devoluciones.
- **Integración** (`TesoreriaBancosTests`, además de los existentes): alta de bancos y subcuentas, cobros por
  banco/caja con su asiento, compatibilidad sin bancos, remesa registrada que no se repite y se cobra contra el
  banco, remesa de transferencias, anular una remesa viva, devolución con contraasiento, 626 e impagados, gastos
  repercutidos, conciliación automática (remesa y factura), manual N a 1, asiento de comisión, deshacer,
  duplicados, y saldos para la previsión en modo Simple y Completo.
