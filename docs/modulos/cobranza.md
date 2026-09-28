# Cobranza: anticipos e impagados (base, en todas las ediciones)

## Anticipos de clientes

Un **anticipo** (entrega a cuenta) es dinero cobrado antes de facturar. Después se aplica a una o
varias facturas del **mismo cliente**, y cada aplicación es un **cobro** de esa factura (con las
reglas de Tesorería: sin sobrepago y con el estado derivado del saldo). El cobro y la anotación en
el anticipo se guardan en la **misma transacción**.

- Estado derivado: `Disponible` → `Parcial` → `Aplicado`.
- No se aplica más de lo disponible (*"El anticipo solo tiene 50.00 € disponibles."*) ni a facturas de otro cliente.
- Sin importe, se aplica lo máximo posible: lo menor entre el disponible y el pendiente de la factura.

> **IVA:** si el anticipo corresponde a una entrega o servicio futuro, el IVA se devenga al cobrarlo
> (art. 75.2 LIVA) y hay que emitir una factura de anticipo. Hoy el anticipo es un registro de
> tesorería. La factura de anticipo automática está pendiente.

## Impagados y reclamaciones

`GET /impagados` lista las facturas **emitidas, vencidas y con pendiente**, de más a menos retraso,
con el **nivel de reclamación que toca** y el último enviado. Niveles por defecto:

| Nivel | Días tras el vencimiento | Tono |
|---|---|---|
| 1 | 7 | Recordatorio amable |
| 2 | 30 | Segundo aviso |
| 3 | 60 | Aviso final |

Cada empresa puede cambiarlos (`PUT /impagados/niveles`): numerados desde 1, días crecientes, asunto
y texto con las variables `{cliente}`, `{factura}`, `{vencimiento}`, `{pendiente}` y `{dias}`.

`POST /impagados/{facturaId}/reclamar` registra la reclamación (nivel, canal, pendiente y días de
retraso en ese momento; es un histórico que no se edita). Si el canal es **email** y el cliente
tiene correo, la envía con la **factura en PDF** adjunta. La respuesta dice si se envió y, si no, por qué.

## API

| Método | Ruta | Permiso |
|---|---|---|
| `POST` | `/anticipos` `{ clienteId, importe, fecha?, concepto?, metodo? }` | `cobro.registrar` |
| `GET` | `/anticipos?clienteId=` | `factura.leer` |
| `POST` | `/anticipos/{id}/aplicar` `{ facturaId, importe?, fecha? }` | `cobro.registrar` |
| `GET` | `/impagados` | `factura.leer` |
| `POST` | `/impagados/{facturaId}/reclamar` `{ canal, nivel?, nota? }` | `cobro.registrar` |
| `GET` / `PUT` | `/impagados/niveles` | `factura.leer` / `empresa.ajustes` |

## Interfaz

Ventas → **Impagados** (lista con indicadores y botón *Reclamar · nivel N*) y Ventas → **Anticipos**
(alta y aplicación a facturas).

## Base de datos

Tablas `tesoreria.anticipo`, `aplicacion_anticipo`, `reclamacion` y `configuracion_reclamaciones`, con
RLS por empresa y `CHECK` de importes y niveles.

### Anticipos en las facturas

- **Al hacer la factura**: si el cliente tiene anticipos con algo por aplicar, la ficha del cliente lo avisa
  («Tiene X € en N anticipos pendientes de aplicar») con la casilla **Aplicarlo al emitir** (marcada por defecto). Al
  emitir se aplican los anticipos del más antiguo al más reciente hasta cubrir el total de la factura.
- **En la factura emitida** con importe pendiente: «Anticipos del cliente sin aplicar» y **Aplicar a esta factura**,
  para elegir cuánto de cada anticipo.
- **Asiento de cancelación**: cada aplicación registra el cobro de la factura y su asiento, 438 Anticipos de clientes
  al debe y 430 Clientes al haber (no mueve dinero: el banco ya se movió al recibir el anticipo, 572 a 438).
- **Listado de clientes**: columna «Anticipos» con lo pendiente de aplicar de cada cliente y su total.

### Anticipos con factura (IVA al cobrar, art. 75.2 LIVA)

El IVA de un anticipo se devenga cuando se cobra, así que lo normal es **emitir la factura del anticipo**. En
*Anticipos → Nuevo anticipo* la casilla «Emitir factura de anticipo» viene marcada: se indica el importe cobrado (IVA
incluido) y el impuesto, y el ERP:

1. Emite la factura del anticipo (serie del cliente, VeriFactu, SII como `F1`): una línea «Anticipo a cuenta: …» cuya
   base va a la **438 Anticipos de clientes** (no es venta) y su IVA a la 477. La base se ajusta para que el total sea
   exactamente lo cobrado. Asiento: 430 al debe; 438 y 477 al haber.
2. Registra su cobro (57x a 430) en el banco o caja elegido.
3. Guarda el anticipo enlazado a su factura, con la base pendiente de descontar.

En la **factura final** el editor avisa de los anticipos facturados del cliente («Descontar en esta factura», marcado):
se añade una línea negativa por anticipo con su base e IVA («Anticipo a cuenta, fra. X de fecha»), como mucho hasta la
base de la factura (lo que sobre queda para la siguiente). La factura final solo declara la diferencia. Asiento:
430 por el total neto al debe, 438 al debe por la base descontada, 70x por la venta completa y 477 por el IVA neto.
Los informes de ventas y el análisis cuentan la venta completa (las líneas de la 438 no son venta).

- Un anticipo facturado no se aplica como cobro (`anticipo.facturado`) ni se anula desde Anticipos: se anula su factura
  (si no está descontado; primero hay que devolver el cobro) y el anticipo queda anulado.
- Si se anula la factura final, sus descuentos se deshacen (apunte contrario: la tabla es de solo inserción) y el
  anticipo vuelve a quedar disponible.
- `POST /anticipos` con `facturar: true`, `codigoIva` y `cuentaBancariaId`; `POST /facturas` y `/facturas/simular`
  con `descontarAnticipos: [{ anticipoId, base? }]`.
- Los anticipos sin factura (casilla desmarcada) siguen como antes: 57x a 438 y se aplican como cobro de la factura
  final (438 a 430).

### Orden de las líneas de los documentos

Las líneas de facturas, presupuestos, pedidos de venta y de compra, albaranes de venta y de compra, solicitudes de
compra, facturas periódicas y cartas de porte guardan su número (`orden`): pantalla, PDF, Facturae y SII las muestran
en el orden en que se escribieron (la base de datos no garantiza el orden en que devuelve las filas). Los documentos
anteriores a este cambio se numeraron en el orden en que estaban guardados.
