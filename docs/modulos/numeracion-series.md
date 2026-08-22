# Numeración: series asignables por empresa / cliente / proveedor

Dentro del módulo **Organización**. Amplía las series de numeración para que, además de existir por
(empresa · tipo de documento · ejercicio · prefijo), se puedan **asignar** a un tipo de documento a
nivel de **empresa** (por defecto) o para un **cliente** o **proveedor** concreto. Al emitir un
documento, la serie se resuelve automáticamente.

## Concepto

- `SerieNumeracion` (ya existente): contador correlativo sin huecos por prefijo/ejercicio.
- `AsignacionSerie` (nuevo): dice **qué prefijo** usar para un `TipoDocumento` y un **ámbito**
  (`Empresa`, `Cliente`, `Proveedor`). Para el ámbito Empresa el tercero es `Guid.Empty`; para
  cliente/proveedor guarda su id. Índice único por (empresa, tipo, ámbito, tercero).
- `TipoDocumento` se amplía: `Factura, Ticket, Rectificativa, Presupuesto, PedidoCompra, AlbaranCompra`.

## Resolución al emitir (`IResolverSerie`)

`ResolverPrefijoAsync(empresa, tipoDocumento, terceroId?)` devuelve:
1. el prefijo asignado a ese **tercero** (si tiene uno), si no
2. el prefijo por defecto de la **empresa** para ese documento, si no
3. `null` (se usa el comportamiento por defecto del módulo, p. ej. `FA`).

**Cableado actual**: `EmitirFactura` resuelve la serie por el **cliente** cuando no se indica una
serie explícita en el comando. Así, «este cliente factura siempre en la serie X» funciona solo. El
resto de documentos (tickets, rectificativas, presupuestos, pedidos/albaranes de compra) pueden
asignarse ya en la pantalla y quedan listos para cablearse en su emisión (ampliación acotada).

## API

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/series/asignaciones` | (autenticado) | Lista las asignaciones de la empresa. |
| `POST` | `/series/asignaciones` | `empresa.ajustes` | Crea una asignación (empresa/cliente/proveedor). **201** |
| `DELETE` | `/series/asignaciones/{id}` | `empresa.ajustes` | Elimina una asignación. **204** |

## Persistencia

Esquema `organizacion`, tabla `asignacion_serie` (RLS por empresa), índice único `ux_asignacion_serie`.
Migración `AsignacionSerie`.

## Tests

- **Unitarios** (`AsignacionSerieTests`): ámbito empresa sin tercero, ámbito cliente exige tercero,
  validación del prefijo.
- **Integración** (`AsignacionesSerieEndpointsTests`): la serie del cliente gana a la de la empresa
  al emitir (el número sale con el prefijo del cliente; otro cliente usa el de la empresa); no se
  asigna dos veces el mismo ámbito; se puede eliminar.

## Interfaz

En **Ajustes**, panel «Asignación de series»: tabla de asignaciones y modal para asignar una serie a
un documento eligiendo ámbito (empresa / cliente / proveedor) y prefijo. La creación de series
sueltas sigue en el panel «Series de facturación».
