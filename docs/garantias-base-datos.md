# Garantías de la base de datos

Las reglas que no pueden fallar las impone **PostgreSQL**, no solo la aplicación. Si mañana hay un fallo
en el código, una integración escribe directamente en la base o alguien lanza un `UPDATE` a mano, la
base de datos lo rechaza. Es la parte que se ha traído de Clon (fase 3 del [plan de producto](plan-producto.md)).

Todas las violaciones se lanzan con el SQLSTATE propio **`AX001`**, el mensaje para el usuario y el
código del error en el `HINT`. La API las devuelve como **409** con el mismo formato que el resto de
errores (`MiddlewareGarantiasBaseDatos`). La aplicación valida lo mismo antes, así que en uso normal
no deberían verse: son la última barrera.

## 1. Aislamiento entre empresas (RLS)

- Toda tabla con `empresa_id` o `grupo_id` tiene **Row-Level Security forzada** con su política
  (`RlsSql.Activar` / `RlsSql.ActivarPorGrupo`).
- Las tablas hijas sin esas columnas (líneas de pedidos, albaranes, cartas de porte y compras, apuntes,
  dotaciones de amortización, componentes, tarifas, imputaciones, aplicaciones de anticipos) **heredan
  el aislamiento de su cabecera**: una fila solo se ve y se escribe si se ve su cabecera
  (`GarantiasSql.RlsPorPadre`).
- Solo cinco tablas no tienen RLS, a propósito: se leen antes de que haya una empresa activa (usuarios,
  grupos, empresas, membresías y claves de API). Están listadas, con el motivo, en el test guardián.

**Los tests se conectan con un rol sin privilegios** (`alxor_app`: sin superusuario ni `BYPASSRLS`),
igual que debe hacerlo la aplicación en producción. Antes usaban `postgres`, que se salta la RLS, y por
eso no se había visto que **la numeración de facturas abría la conexión por fuera de EF** sin fijar la
empresa activa: con un rol correcto, no se habría podido emitir ni una factura.

### Producción

```sql
CREATE ROLE alxor_app LOGIN PASSWORD '…' NOSUPERUSER NOBYPASSRLS;
CREATE DATABASE alxor OWNER alxor_app;
```

El rol puede ser dueño de la base (aplica las migraciones): como las tablas tienen `FORCE ROW LEVEL
SECURITY`, la RLS le afecta igualmente. Lo que no puede es ser superusuario ni tener `BYPASSRLS`.

## 2. Numeración de facturas sin huecos

| Antes | Ahora |
|---|---|
| El número se reservaba en la serie (Organización) en **otra conexión y otra transacción** que la factura: si la emisión fallaba después, el número se perdía y quedaba un **hueco**. | El número es «último de la serie + 1», calculado **en la misma transacción que guarda la factura** y bajo un **bloqueo por empresa**. Si la emisión falla, no se consume número. |
| Dos emisiones simultáneas leían la misma huella anterior: la **cadena VeriFactu se bifurcaba**. | El mismo bloqueo serializa la lectura de la última huella y el guardado: una sola cadena (lo prueba un test con 8 emisiones en paralelo). |
| Una factura podía llevar fecha anterior a la de la factura previa de su serie. | Rechazado con `factura.fecha_no_correlativa` (la numeración debe ser correlativa también en fechas). |
| La última huella se buscaba cargando en memoria **todas** las facturas de la empresa. | Se leen solo el último alta y la última anulación. |

En la base de datos, al confirmar la transacción (trigger diferido):
- la factura *n* exige que exista la *n − 1* de su serie y ejercicio (como las facturas no se borran,
  eso basta para que no haya huecos) y que su fecha no sea anterior;
- base, cuota de IVA y recargo de la cabecera deben ser la suma de sus líneas, y debe tener líneas.

La serie de numeración (que muestra «próximo número» en Ajustes) se actualiza sola desde la factura
guardada. Los demás documentos (pedidos, albaranes, presupuestos) siguen con su numeración: un hueco
ahí no tiene consecuencias legales.

## 3. Documentos inalterables

| Tabla | Regla |
|---|---|
| `facturacion.factura` | Inalterable salvo el paso **Emitida → Anulada** (con su registro de anulación) o **Emitida → Rectificada**, y el estado de envío a la AEAT. Cualquier columna nueva queda protegida por defecto. No se borra. |
| `facturacion.linea_factura` | Solo se insertan en la transacción en que se emite la factura; después, ni altas, ni cambios, ni borrados. |
| `contabilidad.asiento`, `contabilidad.apunte` | Igual: un asiento registrado no se modifica ni se borra, y no admite apuntes nuevos. Al confirmar: numeración sin huecos por ejercicio y **debe = haber** con al menos dos apuntes. |
| `catalogo.movimiento_stock`, `inventario.movimiento_inventario` | Solo inserción: los errores se corrigen con un ajuste. |
| `tesoreria.movimiento` (cobros y pagos), `tesoreria.aplicacion_anticipo`, `tesoreria.reclamacion` | Solo inserción. |
| `auditoria.registro_auditoria` | Solo inserción. |

Las líneas se atan a su alta con la columna `tx_alta` de la cabecera (`xid8`, la transacción en que se
insertó), que rellena un trigger.

**Única excepción: la baja de la empresa** (`DELETE /cuenta`). Declara el borrado en su transacción
(`BorradoEmpresa.EjecutarAsync`, parámetro local `app.borrado_empresa`) y solo para esa empresa.

### Analítica y presupuestos (fase 4)

| Tabla | Regla |
|---|---|
| `contabilidad.imputacion_analitica` | FK real al apunte (diferida). Coherente con su apunte (cuenta, fecha, asiento y empresa); lo imputado no lo supera ni cambia de signo. Cada reparto secundario suma cero. Solo a centros y partidas del propio grupo. No se toca si su periodo analítico está cerrado. |
| `contabilidad.linea_clave_reparto` | Los porcentajes de cada clave suman exactamente 100. |
| `contabilidad.periodo_analitico` | Los periodos de una empresa no se solapan. |
| `contabilidad.presupuesto_contable` y sus líneas | Un importe por mes y ninguno negativo; un presupuesto aprobado no se modifica ni se borra. |

Detalle en [modulos/analitica.md](modulos/analitica.md).

### Agro (fase 5)

| Tabla | Regla |
|---|---|
| `agro.recepcion`, `agro.liquidacion`, `agro.parte_confeccion`, `agro.clasificacion` | Solo cambian en borrador (provisional en la clasificación). Después, solo su transición (confirmada → anulada, emitida → anulada, validado → anulado, definitiva → sustituida), y solo las columnas de esa transición. Solo el borrador se borra. Numeración sin huecos por ejercicio. |
| Sus líneas (pesadas, líneas de recepción y de liquidación, consumos, salidas…) | Se modifican con la cabecera en borrador o en la misma transacción de su cambio de estado (columna `tx_estado`). |
| `agro.movimiento_partida`, `agro.movimiento_envase`, `agro.genealogia` | Solo inserción. El saldo de cada partida, suelto y en cada palé, nunca es negativo. Una partida anulada no se mueve. Cada palé solo admite los movimientos de su estado. |
| `agro.partida`, `agro.pale` | El origen y los kilos de una partida no cambian. El SSCC no cambia y lleva un dígito de control GS1 válido. Un palé expedido ya no se toca. |
| Recepción confirmada | Cada línea tiene el neto y los envases de sus pesadas, y su partida con esos kilos. |
| Liquidación | Cada entrega en una sola liquidación viva y con todos sus kilos netos; el precio vigente en su fecha. Importe = round(kilos × precio, 2) y totales coherentes. Emitir exige que el agricultor haya autorizado la autofacturación. |
| Precios y tarifas | Sin solapes; los precios aplicados en una liquidación emitida y las tarifas usadas en un parte no cambian. |

Detalle en [modulos/agro.md](modulos/agro.md).

**Migración y cartera (fase 6):**
- `migracion.correspondencia` y `migracion.ejecucion`: solo inserción, para que quede la traza de qué se trajo de dónde.
- `tesoreria.efecto_cartera`: solo inserción, con importe positivo; se cobra o se paga con movimientos, sin
  superar el pendiente.

Ver [migracion-hispatec.md](migracion-hispatec.md).

**Anulación de gastos:** un gasto anulado (hoy, la autofactura de una liquidación agrícola) encola su
**contraasiento** en la misma transacción que la anulación, y deja de contar en los libros de IVA (antes el
libro de IVA soportado incluía los gastos anulados) y en las autoliquidaciones.

## 4. Restricciones sobre importes y estados

- Factura: número ≥ 1, ejercicio = año de la fecha, número completo coherente con prefijo, ejercicio y
  número, estados y tipos válidos, rectificativa ⇔ factura rectificada, IRPF entre 0 y 60 %,
  retención = base × IRPF, **total = base + IVA + recargo − retención**, anulada ⇔ registro de
  anulación completo, vencimiento ≥ emisión.
- Línea de factura: cantidad > 0, precios y porcentajes válidos, **base = cantidad × precio × (1 −
  descuento)** y cuotas = base × tipo, todo redondeado a céntimos.
- Apunte: importes no negativos y en el debe **o** en el haber.
- Factura con IGIC: sin recargo de equivalencia. Tipos de IGIC del catálogo: sin recargo.

Para que la línea guardada reproduzca su base, el dominio redondea ahora cantidad, precio y porcentajes a
la precisión de su columna **antes** de calcular (antes, 2,0004 uds × 100 € daba 200,04 € de base pero se
guardaba 2,000 uds).

## 5. Tests guardianes

`tests/AlxorCore.IntegrationTests/GarantiasBaseDatosTests.cs`, contra la base real:

- el rol de la aplicación no es superusuario ni tiene `BYPASSRLS`;
- toda tabla de negocio tiene RLS forzada con política (salvo las cinco justificadas);
- toda clave foránea tiene índice y ninguna está duplicada;
- conectándose directamente, una empresa no ve ni escribe datos de otra;
- facturas, asientos, cobros y auditoría rechazan cambios y borrados;
- la numeración rechaza huecos y fechas desordenadas;
- 8 emisiones simultáneas dan los números 1..8 y una sola cadena VeriFactu;
- la baja de la empresa sí borra sus facturas y cobros.

Una tabla nueva sin RLS o una FK sin índice hacen fallar la batería.
