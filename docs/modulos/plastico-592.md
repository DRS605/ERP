# Impuesto sobre los envases de plástico no reutilizables (modelo 592)

Base (Informes). Ley 7/2022: 0,45 € por kilo de plástico no reciclado.

- **Ficha de plástico** de cada artículo (`fiscal.ficha_plastico`, `PUT /informes/plastico/fichas/{productoId}`): clave
  (A envase, B semielaborado, C cierre y presentación), kilos de plástico por unidad y de ellos los reciclados, y si está
  exento (con el motivo).
- **Modelo 592** (`GET /informes/modelo-592?anio=&periodo=`, trimestral `1T`–`4T` o mensual `01`–`12`):
  - adquisiciones intracomunitarias: los albaranes de compra de proveedores de otro Estado miembro (las de no más de 5 kg
    en un mes no están sujetas);
  - fabricación: las órdenes de producción terminadas;
  - deducibles: lo facturado a clientes de fuera de España;
  - exentos aparte. Base = adquisiciones + fabricación − deducibles; cuota = base × 0,45.
- **Libro registro** del periodo (cada apunte con documento, tercero, país, artículo, clave, unidades y kilos) en la
  respuesta y en CSV (`formato=csv`).
- Las compras nacionales ya llevan el impuesto repercutido por el fabricante y no entran. El modelo se presenta en la
  sede de la AEAT con estos importes; no se envía desde el ERP.
