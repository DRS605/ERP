# Módulo Compras

Cadena completa de aprovisionamiento: **solicitud de compra → pedido → albarán de recepción →
factura**. Cada eslabón se apoya en el anterior (casación) y la factura se contabiliza por el
puerto `IContabilizador` (respeta el modo de contabilidad de la empresa). Multiempresa (RLS).

## La cadena

1. **Solicitud de compra** (`SolicitudCompra`) — petición interna de aprovisionamiento (qué se
   necesita y cuánto). Estados: `Borrador → Aprobada → Convertida` / `Rechazada`.
2. **Pedido de compra** (`PedidoCompra`) — pedido a un proveedor (con precios). Se puede crear
   desde una solicitud aprobada (que queda `Convertida`). Estados: `Borrador → Confirmado →
   Recibido → Facturado` / `Cancelado`. Cada línea lleva **cantidad**, **cantidad recibida** y
   **cantidad facturada**.
3. **Albarán de recepción** (`AlbaranCompra`) — registra qué y cuánto se recibe contra el pedido;
   al crearse **actualiza** las cantidades recibidas del pedido (casación por línea). Admite
   recepciones parciales; `RecibidoCompleto` indica si ya llegó todo.
4. **Factura** — facturar el pedido lo marca `Facturado` y lo **contabiliza**: crea el gasto con
   IVA soportado (modo Simple) y, en modo Completo, además el asiento de partida doble.

## Reglas (invariantes)

- No se recibe mercancía de un pedido sin **confirmar** (409).
- No se puede recibir **más de lo pedido** en una línea (400).
- No se factura dos veces ni se cancela un pedido ya facturado.
- El total del pedido = Σ(cantidad × precio) por línea.

## API

**Solicitudes** (`/compras/solicitudes`)

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/compras/solicitudes` | `compra.leer` | Lista solicitudes. |
| `POST` | `/compras/solicitudes` | `compra.gestionar` | Crea una solicitud. **201** |
| `POST` | `/compras/solicitudes/{id}/aprobar` | `compra.gestionar` | Aprueba. |
| `POST` | `/compras/solicitudes/{id}/rechazar` | `compra.gestionar` | Rechaza. |

**Pedidos** (`/compras/pedidos`)

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/compras/pedidos` | `compra.leer` | Lista pedidos. |
| `GET` | `/compras/pedidos/{id}` | `compra.leer` | Obtiene un pedido. |
| `GET` | `/compras/pedidos/{id}/albaranes` | `compra.leer` | Albaranes del pedido. |
| `POST` | `/compras/pedidos` | `compra.gestionar` | Crea un pedido (opcional `SolicitudOrigenId`). **201** |
| `POST` | `/compras/pedidos/{id}/confirmar` | `compra.gestionar` | Confirma. |
| `POST` | `/compras/pedidos/{id}/recibir` | `compra.gestionar` | Registra un albarán de recepción. **201** |
| `POST` | `/compras/pedidos/{id}/facturar` | `compra.gestionar` | Factura (genera gasto / asiento). |
| `POST` | `/compras/pedidos/{id}/cancelar` | `compra.gestionar` | Cancela. |

## Composición

- Resuelve el proveedor en **Terceros** (`IConsultaProveedores`) al crear un pedido con `ProveedorId`.
- La factura del pedido se contabiliza por el puerto **`IContabilizador`** de Recepción/Contabilidad,
  así que respeta el **modo de contabilidad** (Simple = gasto; Completo = gasto + asiento). No es
  una transacción única entre módulos (mejora futura).

## Persistencia

- Esquema **`compras`**: `solicitud_compra`, `pedido_compra`, `albaran_compra` (con sus líneas como
  colecciones propias). RLS por empresa en las tres tablas padre.
- Migración: `MigracionInicialCompras` (incluye la activación de RLS).

## Tests

- **Unitarios**: totales del pedido, recepción (no sin confirmar, parcial/total, no sobre-recepción),
  facturación (no dos veces) y estados de la solicitud (aprobar → convertir).
- **Integración**: la **cadena completa** solicitud → aprobar → pedido (desde la solicitud, que queda
  Convertida) → confirmar → recibir (albarán total) → facturar, comprobando que genera el gasto que
  cuadra (40 base + 21 % = 48,40); y que no se recibe un pedido sin confirmar (409).

## Futuro (documentado)

Casación factura↔albarán (fase 2 del pliego), facturación parcial por líneas recibidas, y precios
sugeridos desde el catálogo o el histórico del proveedor.
