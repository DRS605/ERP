# Control de riesgo por cliente y proveedor

Límite de riesgo (crédito) **editable** por cliente y por proveedor, con control **configurable por
empresa**: al emitir un documento que haría superar el límite, el programa puede solo **avisar** o
**bloquear** la operación.

## Modelo

- `Cliente.LimiteRiesgo` y `Proveedor.LimiteRiesgo` (`decimal?`, editable; null o ≤ 0 = sin límite).
- `Empresa.ControlRiesgo` (`Aviso` por defecto / `Bloqueo`), enum transversal en `AlxorCore.Nucleo`.

## Riesgo vivo

El **riesgo vivo** de un tercero es el importe **pendiente** (total − liquidado) de sus documentos:

- Cliente: facturas emitidas pendientes de cobro.
- Proveedor: gastos pendientes de pago.

Lo calcula Tesorería (que conoce los saldos) e implementa el puerto compartido `IConsultaRiesgo` en
`AlxorCore.Nucleo`, consumido por Facturación y Gastos.

## Comprobación al emitir/registrar

Si el tercero tiene límite y `riesgo vivo + total del documento > límite`:

- **Aviso**: la operación continúa y la respuesta incluye `AvisoRiesgo` (texto), que la UI muestra
  como aviso no bloqueante.
- **Bloqueo**: la operación se rechaza con `409 Conflicto` (`riesgo.superado`).

En facturas la comprobación se hace **antes de numerar**, calculando el total con una factura
provisional (número 0) que se descarta, de modo que un bloqueo **no consume número** (no crea huecos).
En gastos se comprueba antes de guardar. Los tickets de TPV (contado) no aplican control de riesgo.

## API

| Método | Ruta | Descripción |
|---|---|---|
| `PUT` | `/empresas/actual/control-riesgo` | Fija el modo (`Aviso`/`Bloqueo`). |

El límite se fija con `limiteRiesgo` en `/clientes` y `/proveedores`. Las facturas y gastos devuelven
`avisoRiesgo` cuando procede.

## Persistencia

- Columnas `limite_riesgo` en `terceros.cliente` y `terceros.proveedor`; `control_riesgo` en
  `organizacion.empresa` (por defecto `Aviso`).
- Migraciones `TerceroLimiteRiesgo` y `ControlRiesgoEmpresa`.

## UI

- Campo **Límite de riesgo (€)** en las fichas de cliente y proveedor.
- **Ajustes → Control de riesgo**: elegir avisar o bloquear.
- Al facturar o registrar un gasto se muestra el aviso si se supera el límite (y se impide en modo
  bloqueo).

## Tests

- **Integración** (`RiesgoEndpointsTests`): por defecto avisa sin bloquear; en modo bloqueo impide
  emitir al superar; dentro del límite emite sin aviso; el riesgo vivo acumula facturas pendientes;
  el límite del proveedor avisa al registrar un gasto.

## Futuro (documentado)

Incluir en el riesgo vivo los pedidos/presupuestos aún no facturados; permiso especial para forzar
por encima del límite en modo bloqueo.
