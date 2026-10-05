# Bodegas

Módulo sectorial `bodega`, que se contrata aparte (necesita Inventario). Esquema propio `bodega`, con RLS por empresa;
rutas bajo `/bodega`. Las botellas entran en las existencias del artículo, la venta a granel sale con un albarán de venta
y la uva se paga al viticultor con una autofactura (un gasto), todo por puertos que implementa la API.

## Depósitos

Un **depósito** (`/bodega/depositos`) tiene código, nombre, tipo (acero, hormigón, barrica, tinaja u otro), capacidad en
litros y lo que contiene:

- los **litros**;
- el **producto**: mosto, vino blanco, rosado, tinto u otro;
- la **calificación**: DOP, IGP… o ninguna;
- la **composición** por variedad y añada.

Lo que contiene solo cambia con las operaciones. Un depósito con vino no admite otro producto (`deposito.mezcla_producto`)
ni pasa de su capacidad (`deposito.capacidad`). Se da de baja vacío y se borra si no ha tenido operaciones.

## Uva

- **Entradas de uva** (`/bodega/uva`): viticultor (un proveedor), variedad, kilos, grado (ºBaumé), parcela y
  calificación, de la añada de su fecha. Numeradas por año (`U2026/00001`). Se corrigen o se anulan mientras no estén
  elaboradas ni liquidadas.
- **Precios de la uva** (`/bodega/precios-uva`): uno por variedad y añada. Es el precio por kilo al grado de referencia,
  que sube o baja un porcentaje por cada grado por encima o por debajo (y nunca es negativo).
- **Liquidación al viticultor** (`/bodega/liquidaciones`, con `/simular` para calcularla sin guardar): sus entradas
  pendientes de unas fechas, cada una a su precio corregido por su grado. Se registra como **autofactura**, con la
  compensación REAGP (por defecto `REAGP12`), el IVA o el IGIC que se indique y la retención. Sin precio para alguna
  variedad y añada es un error, nunca un cero. Al anularla se anula su autofactura (no si está pagada) y las entradas
  vuelven a estar pendientes.

## Operaciones

Cada operación (`/bodega/operaciones/...`) se numera por año (`OB2026/00001`) y deja una línea por movimiento. La línea
lleva el depósito, los litros con signo y el producto y la calificación de lo que mueve:

- **Elaboración**: entradas de uva → litros obtenidos en un depósito. La composición se reparte por los kilos de cada
  variedad y añada, y la calificación es la común de la uva (o la que se indique, si toda la uva puede ir a ella). Se
  guarda el rendimiento (l/100 kg), que no puede pasar de 100.
- **Trasiego**: de un depósito a otro, todo o una parte, con su merma.
- **Coupage**: varios depósitos a uno. Si se mezclan calificaciones distintas, el conjunto se queda sin calificación y
  lo que ya había en el destino se **reclasifica** (sale de una categoría y entra en la otra).
- **Merma**: evaporación, lías…, con el motivo.
- **Embotellado**: salen las botellas × su formato (más la merma) y las botellas entran en las existencias del artículo,
  con su lote.
- **Venta a granel**: salen los litros y se emite un albarán de venta al cliente con el artículo (en litros).

La composición sale de cada depósito en proporción y entra en el destino, al céntimo de litro, sin perder nada.

**Anulación** (`POST /bodega/operaciones/{id}/anular`): una operación se deshace si sigue siendo la **última** de
todos sus depósitos (`operacion_bodega.posterior`). Cada depósito vuelve exactamente a como estaba (se guardó su estado
antes de la operación). La uva vuelve a estar pendiente de elaborar, las botellas salen de las existencias y el albarán
de granel se anula (si está facturado, no se puede).

## Declaración de existencias y movimientos

`GET /bodega/declaracion?anio=&mes=`: por producto y calificación, en litros:

- existencia inicial;
- elaboración;
- entradas y salidas internas (trasiegos, coupages y reclasificaciones);
- embotellado y granel;
- mermas;
- existencia final, que cuadra con lo que hay en los depósitos.

Es la base de la declaración mensual de existencias (INFOVI). **El fichero oficial no se genera**.

## Trazabilidad

`GET /bodega/depositos/{id}/origen`: recorre hacia atrás las operaciones vivas que metieron vino en el depósito, y las
de los depósitos de donde vino, hasta las elaboraciones y sus entradas de uva (viticultor, variedad, añada, parcela y
calificación). Son los **posibles** orígenes: no se descuenta lo que ya salió de un depósito intermedio.
`GET /bodega/depositos/{id}/operaciones` da el historial del depósito.

## Garantías en la base de datos

- Tipos y estados válidos.
- Un depósito no pasa de su capacidad ni baja de cero. Vacío, no tiene producto ni calificación. Con vino, tiene
  producto. De baja, está vacío.
- Al confirmar, la composición de cada depósito suma exactamente sus litros.
- Uva con kilos positivos y grado de 0 a 30; importes de la liquidación al céntimo.
- Las líneas de las operaciones tienen el signo de su clase y no se cambian ni se borran: la operación se anula.

## Pantallas

Menú **Bodega**:

- *Depósitos*: ocupación, composición, historial y origen;
- *Operaciones de bodega*;
- *Vendimia*: entradas de uva, precios y liquidaciones;
- *Existencias y movimientos*.

Plantilla de rol **Bodeguero**; permisos `bodega.leer` y `bodega.gestionar`.
