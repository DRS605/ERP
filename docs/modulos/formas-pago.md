# Formas de pago (vencimiento y pago automático)

Catálogo de **modalidades de pago** por empresa que determina, al emitir/registrar un documento, si
genera un **vencimiento** abierto y si el **cobro/pago** se registra en el acto. Resuelve el caso de
«una factura que no lleva vencimiento porque ya está pagada», sin obligar a que todos los documentos
funcionen igual: cada cliente/proveedor puede tener su forma habitual y se puede cambiar en cada
documento.

El catálogo vive en el módulo **Organización** (como las series). El pago automático se registra a
través del puerto `IPagosAutomaticos` (en `AlxorCore.Nucleo`), implementado por **Tesorería**, y lo
consumen **Facturación** (ventas y tickets) y **Gastos** (compras).

## Modelo

`FormaPago` { Nombre, **GeneraVencimiento**, **DiasVencimiento**, **RegistrarPagoAutomatico**, Activo }.
Multiempresa (RLS por empresa). Se desactiva en vez de borrarse, para preservar el histórico.

- **GeneraVencimiento = false** → el documento no crea un vencimiento aplazado (vence el mismo día de
  emisión, «al contado»).
- **GeneraVencimiento = true** → el vencimiento se fija a `fecha + DiasVencimiento`.
- **RegistrarPagoAutomatico = true** → al emitir/registrar se registra el cobro/pago por el **total**
  en la fecha del documento (queda saldado, fuera de la cartera y de la previsión). Se deja en
  `false` cuando el pago se registra aparte, para **no duplicarlo**.

Ejemplos: «Contado (pagado)» = no genera vencimiento + registra el pago; «Contado (registro manual)»
= no genera vencimiento + no registra el pago (lo registras tú); «Transferencia 30 días» = genera
vencimiento a 30 días + no registra el pago.

## Cómo se resuelve al emitir/registrar

1. Se toma la forma de pago **indicada** en el documento; si no, la **habitual del cliente/proveedor**
   (`FormaPagoDefectoId`); si no hay ninguna, se mantiene el comportamiento anterior (vencimiento por
   los días indicados en la factura, sin pago automático).
2. **Ventas** (`EmitirFactura`, `EmitirTicket`): la forma fija la fecha de vencimiento y, si procede,
   registra el **cobro** total.
3. **Compras** (`RegistrarGasto`, y por tanto la contabilización de facturas recibidas de Recepción):
   si la forma marca el pago automático, registra el **pago** total del gasto.

## API

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/formas-pago` | autenticado | Lista las formas de pago activas. |
| `POST` | `/formas-pago` | `empresa.ajustes` | Crea una forma de pago. **201** |
| `PUT` | `/formas-pago/{id}` | `empresa.ajustes` | Actualiza una forma de pago. |
| `DELETE` | `/formas-pago/{id}` | `empresa.ajustes` | Desactiva una forma de pago. **204** |

Además, `/facturas`, `/tickets` y `/gastos` aceptan `formaPagoId`; y `/clientes` y `/proveedores`
aceptan `formaPagoDefectoId`.

## Persistencia

- Tabla **`organizacion.forma_pago`** (RLS por empresa; índice por `empresa_id`).
- Columna `forma_pago_defecto_id` en `terceros.cliente` y `terceros.proveedor`.
- Migraciones: `FormasPago` (tabla + RLS) y `TerceroFormaPagoDefecto` (columnas).

## UI

- **Ajustes → Formas de pago**: alta/edición/baja del catálogo, con el comportamiento resumido.
- Selector **Forma de pago** al emitir factura, en el TPV y al registrar un gasto.
- Selector **Forma de pago habitual** en la ficha de cliente y de proveedor.

## Tests

- **Unitarios** (`FormaPagoTests`): contado sin vencimiento fuerza días a 0, nombre obligatorio, días
  negativos inválidos, actualizar y desactivar.
- **Integración** (`FormasPagoEndpointsTests`): contado con pago automático deja la factura liquidada
  sin vencimiento abierto; contado sin pago automático la deja pendiente; forma aplazada fija el
  vencimiento a N días; la forma por defecto del cliente se aplica sin indicarla; un gasto al contado
  queda liquidado al registrarlo.

## Futuro (documentado)

Vencimientos en varios plazos (p. ej. 30/60/90) generando una cartera multiplazo (hoy es un único
vencimiento por documento).
