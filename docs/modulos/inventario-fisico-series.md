# Números de serie e inventario físico

## Números de serie

Un artículo con **Seguimiento: número de serie** (ficha del artículo) cumple estas reglas en el almacén:

- todo movimiento lleva su número (`inventario.serie_requerida`) y es de una unidad (`inventario.serie_unidad`);
- un número no puede estar dos veces en existencias (`inventario.serie_duplicada`). Si se vende y vuelve, sí puede
  volver a entrar.

`POST /inventario/series/entrada` da de entrada varias unidades de una vez: entran todas o ninguna, sin repetidos en la
lista. *Artículos y almacén → Números de serie* lista las unidades en existencias con su almacén y ubicación, y da el
historial de cada número (trazabilidad).

## Inventario físico

*Artículos y almacén → Inventario físico* (`/inventario/recuentos`).

1. **Abrir**: se congela el stock teórico del almacén, de una ubicación o de unos artículos, por artículo, ubicación y
   lote. No puede haber dos recuentos abiertos que se solapen.
2. **Contar**: se anota lo contado. Lo que aparece y no estaba se añade con teórico 0. Con número de serie se cuenta
   cada número: 1 o 0.
3. **Cerrar**: cada diferencia (contado − teórico congelado) se suma al stock de ese momento con un ajuste (motivo
   «Recuento INV-…»). Así, lo que se vendió o entró mientras se contaba no se pierde ni se duplica. Lo no contado se deja
   como estaba o, si se elige, se da por cero.
4. **Anular**: un recuento abierto se anula sin tocar nada.
