# Plan de producto: núcleo generalista + vertical agrícola

**Decisión (26/09/2026):** ALXOR Core es la base del producto. Se vende a empresas generalistas por
ediciones (ver [modulos/ediciones.md](modulos/ediciones.md)). El trabajo del repositorio `DRS605/Clon`
se incorpora aquí en dos partes: las **garantías de integridad en la base de datos** y el **vertical
hortofrutícola** como módulo contratable. Clon queda como referencia de diseño.

## 1. Comparación con ERP generalistas

Referencia: lo que un comprador de pyme española espera de Holded, Odoo, Business Central, Sage 200 o A3ERP.

| Área | ALXOR hoy | Hueco |
|---|---|---|
| Facturación y Verifactu | ✅ Facturas, tickets, rectificativas, recurrentes, Facturae, XML Verifactu | Envío real a la AEAT y QR (si falta) |
| Ventas | ✅ Presupuesto → pedido → albarán → factura, carta de porte | **Tarifas de precios** por cliente o grupo, **descuentos** por volumen, **comisiones** de comerciales, **rappels** |
| Compras | ✅ Solicitud → pedido → albarán → factura, buzón de facturas | Tarifas de proveedor, anticipos a proveedor |
| Inventario y producción | ✅ Multialmacén, ubicaciones, lotes y series, montaje, órdenes de fabricación | Inventario por recuento con ajuste, reservas, valoración FIFO (revisar) |
| Contabilidad | ✅ PGC, asientos, mayor, balances, cierre, cuentas anuales, modelo 200, inmovilizado | **Analítica** (centros de coste), **presupuestos contables**, periodificaciones, **prorrata** de IVA, consolidación de grupo |
| Tesorería | ✅ Cobros y pagos, Norma 43, SEPA (19/34, pain.001/008), confirming, previsión, riesgo | **Reclamación de impagados**, **anticipos** de clientes, cartera de efectos (pagarés), sincronización bancaria PSD2 |
| Impuestos | ✅ 303, 130, 390, 347, 111, 190, 349, SII (XML), REAGP, OSS, criterio de caja, ISP | **IGIC** (Canarias), **TicketBAI/Batuz** (País Vasco), Intrastat, envío SII |
| Plataforma | ✅ Multiempresa y grupos, roles y permisos, 2FA, auditoría, webhooks, API, importación | **Ediciones y módulos** (✅ hecho), portal de cliente, menú por plan |

## 2. Garantías de integridad traídas de Clon

Hoy la integridad de ALXOR depende de la aplicación (EF Core): no hay restricciones `CHECK`, triggers ni
libros inmutables en la base de datos, y la RLS no se ejerce en los tests (se usa superusuario). En Clon
todo eso se garantiza en PostgreSQL y lo vigilan tests guardianes. Plan:

1. Tests de integración con un **rol sin privilegios** para que la RLS se pruebe de verdad.
2. **Numeración** de facturas y asientos protegida en la base de datos (solo avanza, sin huecos, fechas correlativas por serie).
3. **Inmutabilidad** de facturas emitidas, asientos contabilizados, movimientos de stock y registros Verifactu (triggers).
4. `CHECK` en importes y estados (cuadre de asientos, importe = cantidad × precio, totales).
5. **Tests guardianes del esquema**: toda tabla con `empresa_id` con RLS forzada, toda FK con índice, sin FK duplicadas.

Ya aplicado: restricciones `CHECK` del plan de la empresa y guardián de rutas por módulo.

## 3. Vertical hortofrutícola (módulo `agro`)

Se porta desde Clon como módulo contratable aparte de las ediciones, con su propio contexto de datos:

| Pieza | Origen en Clon |
|---|---|
| Campañas, parcelas del agricultor | `0002`, `0007` |
| Recepción de fruta: pesadas, envases, bloqueos | `0007_receptions.sql` |
| Partidas (lotes) y palés (SSCC) con ubicación, composición derivada del libro y trazabilidad | `0003`, `0007` |
| Clasificación y liquidación al agricultor (REAGP, retención, autofactura) | `0008`, `packages/domain/src/growerSettlement.ts` |
| Partes de confección con destajo y genealogía | `0004`, `0010` |
| Trazabilidad de palé a parcela y de parcela a cliente | `0007`, `0010`, `0011` |

Encaja sobre lo que ya tiene ALXOR: terceros (agricultor = proveedor con marca), catálogo (lotes y
series), inventario (almacenes y ubicaciones), compras (la liquidación genera la factura recibida o
autofactura) y ventas (albarán con palés).

## 4. Hoja de ruta

| Fase | Contenido | Estado |
|---|---|---|
| 1 | Ediciones y módulos contratables | ✅ |
| 2 | Menú de la interfaz por plan ✅ · tarifas de precios y descuentos ✅ · anticipos y reclamación de impagados | 🔜 |
| 3 | Garantías de integridad en la BD (puntos 1-5 de §2) | 🔜 |
| 4 | Analítica y presupuestos contables · prorrata · IGIC | 🔜 |
| 5 | Módulo `agro` (§3) | 🔜 |
| 6 | Migración desde Hispatec (esquema ya analizado en Clon: `docs/hispatec/`) | 🔜 |
