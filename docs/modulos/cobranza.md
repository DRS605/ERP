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
