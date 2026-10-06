# Agentes y comisiones

*Ventas → Agentes y comisiones* (`/comisiones`).

## Agentes

- **Agente externo**: se enlaza con su ficha de proveedor. Su liquidación puede registrar su factura de comisiones:
  gasto con IVA y su retención de IRPF.
- **Vendedor de la casa**: sin proveedor. Solo tiene el informe de comisiones; la nómina queda fuera del ERP (no se
  hace RRHH).
- Cada agente tiene su **comisión general** y su **devengo**: al facturar, o al cobrar en proporción a lo cobrado de
  cada factura.

## A quién pertenece cada venta

Cada cliente tiene su agente **desde una fecha** (`/comisiones/asignaciones`). Una factura es del agente que llevaba al
cliente en su fecha. Cambiar de agente no cambia las facturas anteriores.

## Reglas

Una regla da una comisión particular para una familia de artículos, un cliente o los dos. Gana la más concreta:

1. cliente y familia;
2. cliente;
3. familia;
4. la general del agente.

Con reglas por familia, la comisión se calcula línea a línea.

## Liquidación

`GET /comisiones/calculo/{agenteId}?desde&hasta` muestra, factura a factura, la comisión, lo devengado, lo ya liquidado
y lo pendiente. `POST /comisiones/liquidaciones` congela lo pendiente.

- Una factura no se liquida dos veces.
- Si una factura ya liquidada se rectifica o se anula después, la siguiente liquidación descuenta su comisión.
- Solo cuentan las facturas en estado Emitida, como en los márgenes.
- Anular una liquidación anula también la factura del agente (si no se puede, no se anula) y deja sus comisiones
  pendientes.
