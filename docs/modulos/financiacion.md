# Préstamos, leasing y pólizas de crédito

Submódulo de **Contabilidad** (`/contabilidad/financiacion`) para la deuda con entidades financieras. Cada operación
guarda sus condiciones y sus **hechos** (formalización, cuotas pagadas, revisiones del tipo, amortizaciones anticipadas,
traspasos a corto plazo, disposiciones, reintegros y liquidaciones de intereses). El cuadro de amortización se calcula
a partir de las condiciones, las revisiones y los anticipos. Todo lo que mueve dinero o deuda genera su asiento, con
origen `Financiacion`. Multiempresa: RLS sobre `operacion_financiacion`, y sus hechos se protegen a través de ella.

## Tipos

| Tipo | Qué es | Cuentas por defecto |
|---|---|---|
| **Préstamo** | Se recibe el capital en el banco y se devuelve en cuotas | 170 largo plazo · 520 corto plazo |
| **Leasing** | Se recibe un bien y se paga en cuotas con IVA; al final queda la opción de compra | 174 · 524 · cuenta del bien (2xx, obligatoria) |
| **Póliza de crédito** | Un límite del que se dispone y al que se reintegra; se pagan intereses por lo dispuesto | 5201 |

En los tres tipos, las cuentas por defecto son 6623 para los intereses, 572 para el banco y 626 para las comisiones.
Cualquier cuenta se puede cambiar en el alta.

## Cuadro de amortización

- **Sistema francés:** cuota constante. Los intereses bajan y el capital amortizado sube.
- **Capital constante:** se amortiza el mismo capital en cada cuota, así que la cuota baja.
- **Periodicidad:** mensual, trimestral, semestral o anual. La primera cuota cae, por defecto, un periodo después de
  la formalización.
- **Carencia:** las primeras N cuotas solo pagan intereses. Esas N cuotas cuentan dentro del número total de cuotas.
- **Leasing:** la opción de compra (valor residual) es una fila más, sin intereses, un periodo después de la última
  cuota. Cada cuota lleva el IVA del porcentaje indicado.
- **Redondeo:** la última cuota recoge el redondeo, así que la suma del capital de las cuotas es siempre el capital
  inicial menos lo anticipado.
- **Interés por periodo:** tipo nominal anual × meses del periodo / 12.

## Corto y largo plazo

El capital pendiente se reparte por el año en que vence cada cuota. Está a **corto plazo** el capital de las cuotas
pendientes que vencen hasta el 31/12 del *horizonte*:

- al formalizar, el horizonte es el año de la formalización;
- con cada traspaso de cierre, el horizonte avanza un año.

Por eso el saldo de la 520/524 coincide siempre con el corto plazo que muestra la operación.

## Asientos

| Hecho | Asiento |
|---|---|
| Formalización (préstamo) | 572 al debe / 520 (cuotas de este año) y 170 (el resto) al haber |
| Formalización (leasing) | Cuenta del bien al debe / 524 y 174 al haber |
| Cuota | Capital desde la 520/524 (o desde la 170/174 si su año aún no se ha traspasado) + 6623 intereses + 472 IVA del leasing / 572 |
| Traspaso de cierre (ejercicio N) | 170/174 al debe / 520/524 al haber, por el capital de las cuotas de N+1, a 31/12/N |
| Revisión del tipo | Sin asiento, salvo que cambie el capital a corto plazo: en ese caso, un ajuste entre largo y corto plazo |
| Amortización anticipada | Lo que baja el corto y el largo plazo + 626 comisión / 572 |
| Disposición de la póliza | 572 / 5201 |
| Reintegro a la póliza | 5201 / 572 |
| Liquidación de la póliza | 6623 intereses + 626 comisión de no disponibilidad / 572 |

Reglas comunes a todos los asientos:

- Ningún asiento se hace en un **ejercicio cerrado** ni en un **mes cerrado**.
- En los lotes (contabilizar cuotas, traspaso de cierre), las operaciones con fechas cerradas se quedan pendientes y se
  cuentan aparte.
- Los asientos de financiación no se anulan desde el diario (409 `asiento.de_documento`). Se deshacen desde la
  operación.

### Detalles

- **Cuotas.** «Contabilizar cuotas» se puede repetir sin duplicar nada: cada cuota se contabiliza una sola vez, en
  orden. Si una cuota no tiene importe (sin intereses en carencia al 0 %), se anota sin asiento.
- **Revisión del tipo.** Se aplica desde la primera cuota posterior a la fecha, que no puede estar ya pagada. La cuota
  de lo que queda se recalcula con el mismo plazo.
- **Amortización anticipada.** Antes hay que contabilizar las cuotas vencidas hasta esa fecha. Se mantiene el plazo y
  baja la cuota. Si se devuelve todo lo pendiente, la operación queda **cancelada**. En el leasing solo se admite la
  cancelación total.
- **Póliza.**
  - Los movimientos van en orden de fecha, nunca en un periodo ya liquidado, y sin pasar del límite.
  - Los intereses se calculan día a día: lo dispuesto × tipo / 365. La comisión, también día a día: lo no dispuesto ×
    % / 365.
  - La póliza se cierra cuando no queda nada dispuesto.

## Corregir

- **Deshacer** (`POST /{id}/deshacer`) quita el **último** hecho de la operación y anula su asiento con un
  contraasiento, por defecto en la fecha del asiento. Se repite para ir hacia atrás.
- **Eliminar** solo se admite si la operación no tiene más hechos que la formalización. El asiento de formalización
  se anula.
- **Datos** (`PUT /{id}`) cambia la descripción y la entidad. Las condiciones no se modifican: si estaban mal, se
  deshacen los hechos y se da de alta de nuevo.

## Operaciones que ya estaban en curso

- **Sin asiento de formalización** (`ContabilizarFormalizacion = false`): la deuda ya está en el asiento de apertura.
- **Cuotas ya pagadas** (`ContabilizarDesde`): las cuotas anteriores a esa fecha se dan por pagadas fuera (estado
  «Previa»), sin asiento. El horizonte de corto plazo empieza en el año de esa fecha.

## Previsión de tesorería

`GET /contabilidad/financiacion/vencimientos?hasta=` devuelve las cuotas pendientes (con el IVA del leasing) y, en las
pólizas, lo dispuesto a su vencimiento. La pantalla de previsión de tesorería los muestra como pagos con origen
«Financiación».

## API

- `GET /contabilidad/financiacion` y `GET /contabilidad/financiacion/{id}`: el detalle incluye el cuadro con el
  estado de cada cuota (Pagada, Previa, Vencida, Pendiente), los hechos y el dispuesto por tramos.
- `POST /contabilidad/financiacion`: alta (permiso `contabilidad.gestionar`). `PUT` y `DELETE /{id}`.
- `POST /cuotas` `{ hasta, operacionId? }` y `POST /reclasificar` `{ ejercicio }`.
- `POST /{id}/revision` `{ fecha, tipoInteres }` y `POST /{id}/anticipo` `{ fecha, importe, comision }`.
- `POST /{id}/disposicion`, `POST /{id}/reintegro` `{ fecha, importe }`, `POST /{id}/liquidacion` `{ fecha }` y
  `POST /{id}/cierre` `{ cerrar }`.
- `POST /{id}/deshacer` `{ eventoId, fecha? }`.

## SPA

El menú **Contabilidad → Préstamos, leasing y pólizas** abre una pantalla con:

- la deuda a largo y a corto plazo y lo disponible en pólizas;
- el lote de cuotas vencidas y el traspaso de cierre;
- el detalle de cada operación: cuadro, hechos con «Deshacer» en el último, revisión del tipo, amortización
  anticipada y, en la póliza, disposiciones, reintegros y liquidación de intereses.
