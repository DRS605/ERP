# Altas, modificaciones, bajas y anulaciones

Regla de producto: **todo lo que se da de alta se puede corregir**. Qué significa corregir depende de si el registro ya ha
tenido consecuencias (contables, fiscales, de stock o de trazabilidad):

| Situación | Qué se ofrece |
|---|---|
| Maestro **sin uso** en ninguna empresa del grupo | **Eliminar** (borrado real) |
| Maestro **ya usado** | **Dar de baja** (deja de ofrecerse en las altas nuevas, conserva el histórico) y **reactivar** |
| Documento **sin consecuencias** (borrador, pedido sin entregas, orden sin iniciar…) | **Modificar** y, si procede, **eliminar** |
| Documento **con consecuencias** (factura emitida, asiento, cobro, gasto contabilizado…) | **Anular**: se registra la anulación (contraasiento, movimiento en negativo, registro de anulación) y el original no se toca |

La interfaz lo resuelve con un único botón 🗑: intenta eliminar y, si el registro ya se usó (409 `….en_uso`), explica
dónde («ya tiene facturas») y ofrece la baja.

## Cómo se sabe si un maestro está en uso

Los módulos se referencian por identificador, sin claves foráneas entre esquemas, y los maestros (clientes, artículos…)
son **del grupo**: los usan documentos de cualquiera de sus empresas, que la RLS no deja ver desde la empresa actual.

- `ReferenciasRegistros.Mapa` (API) lista, para cada tipo de maestro, las columnas que lo referencian
  (`facturacion.factura.cliente_id` → «facturas»…).
- La función SQL `alxor_en_uso(referencias, id)` recorre **las empresas del grupo actual** fijando cada una como
  empresa activa (la RLS sigue actuando: nunca mira fuera del grupo) y devuelve la primera referencia con uso.
- La subcuenta contable de un tercero solo cuenta como uso si tiene apuntes.
- **Prueba guardia** (`BajasMaestrosTests.Toda_columna_que_referencia_a_un_maestro_esta_en_el_mapa`): recorre la base de
  datos y falla si aparece una columna `cliente_id`, `producto_id`, `almacen_id`… que no esté en el mapa. Un módulo nuevo
  no puede olvidarse de declarar que usa un maestro.

## Qué se puede hacer con cada registro

| Registro | Modificar | Eliminar / baja | Anular |
|---|---|---|---|
| Clientes, proveedores, artículos | ✅ | Eliminar sin uso · baja / reactivar | — |
| Almacenes · ubicaciones | ✅ | Eliminar sin movimientos · baja (almacén) | — |
| Familias · formas de pago · tipos de IVA | ✅ | ✅ | — |
| Tarifas de precios (pantalla nueva) | ✅ | Eliminar si ningún cliente la tiene | — |
| Actividades de negocio (pantalla nueva, con visibilidad por usuario) | ✅ (renombrar, activar/desactivar) | Eliminar si nada la usa (sus reglas de visibilidad se borran con ella); si está en uso, se desactiva | — |
| Series de numeración | — (el prefijo no cambia) | Eliminar si no ha numerado nada | — |
| Tipos de cambio | ✅ (se vuelve a registrar) | ✅ (los documentos guardan su tasa) | — |
| Analítica: centros, partidas, claves, reglas | ✅ (activar/desactivar) | Eliminar sin imputaciones | — |
| Agro: campañas | Nombre; fechas solo se amplían si tiene movimientos | Eliminar sin movimientos | — |
| Agro: categorías, tarifas de coste, parcelas, precios | ✅ (una tarifa usada solo cierra su vigencia) | Eliminar sin uso | — |
| Agro: agricultores | ✅ (bloqueo) | Eliminar la ficha sin movimientos | — |
| Pedidos de venta y de compra | Mientras no haya entregas / recepciones ni factura | — | Cancelar |
| Solicitudes de compra | En borrador o rechazada | En borrador o rechazada | Rechazar |
| Órdenes de fabricación | Planificada (cantidad, almacén, fecha) | — | Cancelar |
| Presupuestos contables | Borrador (líneas) | Borrador | — (aprobado: se copia) |
| Facturas periódicas | ✅ | Si no ha emitido facturas | Pausar |
| Previsiones de tesorería | ✅ | ✅ | — |
| Inmovilizado | — | Si no tiene amortizaciones | Baja · enajenación |
| Asientos manuales | — | — | **Contraasiento** (una sola vez; los de documentos se anulan desde el documento) |
| Cobros y pagos | — | — | **Anulación en negativo** (el anticipo aplicado recupera su saldo) |
| Facturas emitidas | — | — | Anular / rectificar (antes hay que anular sus cobros) |
| Gastos | — | — | Anular (antes sus pagos; una autofactura agraria se anula desde su liquidación) |
| Anticipos | — | — | Anular si no hay nada aplicado |
| Efectos de cartera | — | — | Anular si no tiene cobros ni pagos vivos |
| Palés (agro) | Reabrir si está cerrado | — | **Anular la expedición**: vuelve a cerrado |
| Albaranes de venta | — | — | Anular si el pedido no está facturado: lo servido vuelve a quedar pendiente |
| Albaranes de compra | — | — | Anular si el pedido no está facturado: sale del almacén lo que entró (si sigue allí) y vuelve a quedar pendiente de recibir; el de un traspaso intragrupo se anula desde el albarán de venta de origen |
| Cartas de porte (pantalla nueva: alta, PDF, anular) | — | — | Anular con motivo (el número queda usado) |
| Movimientos de almacén, ajustes de partida, envases | — | — | Se corrigen con el movimiento contrario |

## Garantías en la base de datos

- `contabilidad.asiento.anula_asiento_id`: clave foránea al asiento anulado e **índice único** (un asiento solo se
  anula una vez).
- `tesoreria.movimiento.anula_movimiento_id`: igual, y un `CHECK` exige importe positivo en los movimientos normales y
  negativo en las anulaciones.
- `tesoreria.anulacion_efecto_cartera`: la anulación de un efecto es su propio registro de **solo inserción**, único por
  efecto (el efecto también es de solo inserción).
- `agro.pale`: el disparador de transiciones solo admite `Expedido → Cerrado` al anular la expedición (sin cliente ni
  fecha de salida); cualquier otro cambio de un palé expedido sigue prohibido.

## Pruebas guardia

- `OperacionesRegistrosTests`: recorre todas las rutas de la API y falla si un alta (`POST`) no tiene su modificación,
  eliminación, baja o anulación. Las excepciones son operaciones o cálculos (generar un fichero SEPA, una importación…)
  y se declaran con su motivo. Las altas de registros hijos declaran dónde se corrigen, y la prueba comprueba que esa
  ruta existe.
- `InterfazClasicaTests`: sobre `index.html`, sin funciones con el mismo nombre (una segunda declaración pisaba a la
  primera y rompía cuatro pantallas), todo botón llama a una función que existe y toda llamada a la API va a una ruta que
  existe con su método.
