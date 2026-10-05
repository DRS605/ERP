# Viveros

Módulo sectorial `vivero`, que se contrata aparte (necesita Inventario). Esquema propio `vivero`, con RLS por empresa;
rutas bajo `/vivero`. Las plantas que pasan a la venta entran en las existencias del artículo y los encargos se entregan
con un albarán de venta, por puertos que implementa la API.

## Lotes de planta

Un **lote** (`/vivero/lotes`) tiene:

- la especie con su **nombre botánico**, la variedad y el portainjerto;
- el **artículo** con que se vende;
- el origen del material (semilla, proveedor, lote);
- la fecha de siembra y la prevista para estar lista;
- la **fase**: semillero, injerto, crecimiento o lista;
- la **ubicación** (invernadero, mesa…);
- las plantas iniciales y las **vivas**.

Su código (`LP2026-0001`) es el **código de trazabilidad** del pasaporte fitosanitario.

- **Fase y ubicación** (`/avanzar`): la fase solo avanza, y se puede saltar una (sin injerto, por ejemplo). Cada cambio
  queda en el libro.
- **Bajas** (`/bajas`): plantas que se pierden (mortandad, fallo del injerto, descarte), con el motivo. Se ven las bajas
  y su porcentaje.
- **Paso a existencias** (`/existencias`): plantas **listas y sin reservar** que entran en las existencias del artículo,
  para venderlas con los documentos de venta normales.
- **Anulaciones** (`/movimientos/{id}/anular`): una baja o un paso a existencias se anula y las plantas vuelven al lote.
  En el paso a existencias, además, las plantas salen del stock. Una entrega se deshace anulando su encargo. El lote
  entero solo se anula si no ha tenido bajas, entregas ni ventas.

Cada lote lleva un **libro de movimientos**: siembra, cambios, bajas, entregas, pasos a existencias y anulaciones. Las
plantas vivas son siempre su suma. `GET /vivero/libro?desde=&hasta=` da el libro de todo el vivero.

## Encargos

Un **encargo** (`/vivero/encargos`) es lo que pide un cliente: el artículo, las plantas, la fecha de entrega y el precio
por planta (sin precio, el de su tarifa).

- **Reserva** (`/reservar`): se le asigna un lote del mismo artículo con plantas sin reservar suficientes. Esas plantas no
  se entregan a otros encargos ni pasan a existencias (`encargo.sin_plantas`, `lote_planta.sin_plantas`). `/liberar` la
  quita.
- **Entrega** (`/servir`): con el lote **listo**, las plantas salen del lote y se emite un **albarán de venta** al cliente.
  La referencia del albarán lleva el encargo y el lote. Para que el stock cuadre, las plantas entran en las existencias
  del artículo y el albarán las saca.
- **Anulación** (`/anular`): servido, se anula el albarán (si está facturado, no se puede) y las plantas vuelven al lote.

## Pasaporte fitosanitario

`GET /vivero/lotes/{id}/pasaporte` (con `encargoId`, el de una entrega) da los datos del pasaporte fitosanitario UE
(Reglamento (UE) 2016/2031):

- **A**: la especie (y la variedad);
- **B**: el código de registro del operador (ROPVEG), que se pone en los ajustes del vivero (`/vivero/configuracion`);
- **C**: el código de trazabilidad (el del lote);
- **D**: el país de origen.

Sin código de registro no se emite (`vivero.registro`). La pantalla lo imprime como etiqueta. Las zonas protegidas
(ZP) y los pasaportes de reemplazo no están incluidos.

## Garantías en la base de datos

- Fases, tipos y estados válidos; plantas vivas entre cero y las iniciales; país de dos letras.
- Cada movimiento tiene el signo de su tipo; una anulación apunta al movimiento que anula, y cada movimiento se anula
  una sola vez.
- Al confirmar, las plantas vivas de cada lote son la suma de sus movimientos. El libro no se cambia ni se borra.
- Un encargo pendiente no tiene lote, uno reservado o servido sí, y solo el servido tiene albarán y entrega.

## Pantallas

Menú **Vivero**:

- *Lotes de planta*: fase, ubicación, bajas, paso a existencias, libro y pasaporte;
- *Encargos de planta*;
- *Libro del vivero*;
- *Ajustes del vivero*.

Plantilla de rol **Viverista**; permisos `vivero.leer` y `vivero.gestionar`.
