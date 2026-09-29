# Logística: paletización y unidades SSCC

Módulo genérico para cualquier empresa que fabrique o almacene producto envasado: alimentación,
bebidas, droguería, industria… No depende de Agro. Agro tiene sus palés de confección, que se
pasarán a este núcleo más adelante.

- **Código de módulo:** `logistica`. Necesita Inventario y entra en la edición **Completa**.
- **Esquema:** `logistica`.
- **Rutas:** `/logistica/*`.
- **Permisos:** leer con `inventario.leer`; montar y gestionar con `inventario.gestionar`. La
  numeración SSCC se cambia con `empresa.ajustes`.

## Conceptos

| Concepto | Qué es |
|---|---|
| **Ficha logística** | Por artículo. Incluye:<br>- GTIN de la unidad y de la caja (ITF-14), normalizados a 14 dígitos con su dígito de control.<br>- Unidades por caja, peso neto de la unidad y peso bruto de la caja.<br>- Medidas de la caja.<br>- Mosaico por defecto (cajas por capa × capas) sobre un soporte.<br>- Altura y peso máximos del palé, y si es remontable.<br>- Temperaturas de conservación.<br>- Vida útil, y vida mínima que debe quedar al entregar.<br>- Si se gestiona por lotes. |
| **Soporte** | Europalé, palé americano, medio palé, contenedor… Tiene medidas, tara y carga máxima. Puede tener un artículo de envase asociado para el stock de palés. |
| **Plantilla de paletizado** | Mosaico y límites que pide un cliente para un artículo, o uno general del artículo. |
| **Unidad logística** | Palé, caja o contenedor con su **SSCC**. Es multinivel: una caja con SSCC puede ir dentro de un palé mixto (`padre_id`). |
| **Configuración** | Prefijo de empresa GS1 (7 a 10 dígitos, lo asigna AECOC en España), dígito de extensión, contador de series y si al paletizar se mezclan lotes. |

El SSCC se compone así: extensión + prefijo + serie + dígito de control. Cada empresa lleva su
propio contador, bloqueado en la transacción (`logistica.sscc.{empresa}`), así que dos terminales
nunca dan el mismo número.

**Resolución del mosaico.** Se aplica la primera que exista, en este orden:

1. La plantilla indicada.
2. La plantilla del cliente para el artículo.
3. La plantilla general del artículo.
4. La ficha logística.

## Existencias

Montar un palé **no mueve existencias**. Lo que está dentro de unidades abiertas o cerradas de un
almacén se considera paletizado y ya no se puede volver a paletizar.

- **Disponible** = existencias del lote en el almacén − lo que llevan las unidades vivas de ese
  almacén.
- **Anular** una unidad (desmontarla) libera su contenido. Las cajas con SSCC que llevaba dentro
  quedan sueltas.
- **Expedir** marca la unidad y las que lleva dentro. La salida de existencias la hace el albarán;
  enlazarlos es la fase de expedición.

## Montaje

- **Desde el almacén (`POST /logistica/paletizar`).** Cajas o unidades de un artículo:
  - Del lote indicado o, sin lote, por caducidad (**FEFO**; los lotes sin caducidad van al final).
  - Solo cajas enteras.
  - Sin mezclar lotes, cada lote empieza palé.
  - Cada palé completo se cierra. El último queda abierto con el pico, o no se monta si se pide
    «solo completos».
  - Avisa si el palé supera la altura o el peso máximos, o si algún lote tiene menos días de vida
    que los que exige la ficha.
- **Desde fabricación (`POST /logistica/paletizar-fabricacion`).** Solo para órdenes terminadas.
  - Al terminar la orden se indican el **lote** y la **caducidad** de lo fabricado
    (`POST /produccion/ordenes/{id}/terminar` con `{ lote, fechaCaducidad }`), que entran así en el
    inventario.
  - Por defecto paletiza las cajas que aún no están en palés de esa orden.
  - Los palés quedan enlazados a la orden (`orden_fabricacion_id`), lo que da trazabilidad
    fabricación → palé → cliente.
- **A mano o con la pistola.** Se crea una unidad vacía y se le pone contenido
  (`/unidades/{id}/contenido`; en negativo, se quita). La unidad se cierra sola al completar el
  mosaico.
  - Si el artículo se gestiona por lotes, el lote es obligatorio.
  - No se pone más de lo disponible.

## Lector (pistola y cámara)

`GET /logistica/lectura?codigo=` interpreta lo que envía el escáner. Admite estos formatos:

- **Texto legible con paréntesis**, por ejemplo `(00)384…(10)L1`.
- **Salida de pistola o cámara** con el separador FNC1 (GS, carácter 29). También reconoce las
  variantes `<GS>`, `{GS}` y `~1`.
- **Identificador de simbología** al principio: `]C1`, `]d2`, `]Q3`.
- **Códigos sueltos:** un SSCC de 18 dígitos, un EAN-13 o un ITF-14.

Identificadores de aplicación reconocidos:

| IA | Dato |
|---|---|
| 00 | SSCC |
| 01 / 02 | GTIN |
| 10 | Lote |
| 21 | Número de serie |
| 11 / 13 | Fecha de fabricación o envasado |
| 15 / 17 | Fecha de consumo preferente o caducidad |
| 30 / 37 | Cantidad |
| 310n | Peso neto |
| 330n | Peso bruto |
| 400 | Pedido del cliente |

La lectura devuelve uno de estos resultados:

- **Unidad:** la unidad con ese SSCC.
- **Artículo:** el artículo con ese GTIN (de la unidad o de la caja), con el lote, la caducidad y la
  cantidad leídos.
- **Desconocido:** lo que se haya podido leer, con un aviso.

## Etiqueta

`GET /logistica/unidades/{id o SSCC}/etiqueta` genera un PDF A6 con dos códigos:

- **SSCC** en GS1-128: `(00)`.
- **Contenido:** `(02)` GTIN + `(17)` caducidad + `(37)` cajas + `(10)` lote.

También imprime el peso neto y el bruto, el soporte y el destinatario.

## Interfaz

Menú **Logística y expedición**:

- **Terminal (móvil / pistola).** Pantalla grande para el muelle o la línea de envasado:
  - Funciona con una pistola que actúa como teclado. Lo que llega rápido y termina en Intro se toma
    como lectura aunque el campo no tenga el foco.
  - Funciona con la cámara del móvil (`BarcodeDetector`: Code 128, EAN, ITF, DataMatrix, QR) o
    tecleando.
  - Leer un SSCC abre el palé. Con un palé abierto, cada caja leída suma una caja de su lote y una
    caja con SSCC se mete dentro.
  - Pita y vibra en cada lectura. Tiene modo de pantalla completa y opción de ocultar el teclado del
    móvil.
- **Palés y unidades SSCC.** Lista con filtros y búsqueda por SSCC. Desde aquí se paletiza desde el
  almacén o desde fabricación. La ficha de cada unidad permite cerrar, abrir, mover, asignar, meter
  en otra unidad, expedir, desmontar y sacar la etiqueta.
- **Cálculo de palés.** Para una cantidad o para un pedido de venta pendiente de servir.
- **Fichas, soportes y plantillas.** Maestros del módulo y numeración SSCC.

## Rutas

| Método | Ruta | Qué hace |
|---|---|---|
| GET/PUT | `/logistica/configuracion` | Prefijo GS1, extensión y mezcla de lotes. |
| GET/POST, PUT/DELETE | `/logistica/soportes`, `/logistica/soportes/{id}` | Soportes. Uno que ya se ha usado se da de baja en vez de borrarse. |
| GET, GET/PUT/DELETE | `/logistica/fichas`, `/logistica/fichas/{productoId}` | Ficha logística. |
| GET/POST, PUT/DELETE | `/logistica/plantillas`, `/logistica/plantillas/{id}` | Plantillas de paletizado. |
| GET | `/logistica/calculo?productoId&cajas\|unidades&clienteId&plantillaId` | Palés, pico, peso, altura y avisos. |
| GET | `/logistica/pedidos/{id}/pales` | Palés de lo pendiente de un pedido de venta. |
| GET/POST | `/logistica/unidades` | Lista con filtros (estado, almacén, artículo, lote, pedido, orden, solo raíz) y alta de una unidad vacía. |
| GET | `/logistica/unidades/{id o SSCC}` | Una unidad. |
| POST | `/logistica/unidades/{id}/contenido\|cerrar\|abrir\|anular\|mover\|asignar\|meter\|expedir` | Ciclo de la unidad. |
| GET | `/logistica/unidades/{id o SSCC}/etiqueta` | Etiqueta GS1 en PDF. |
| POST | `/logistica/paletizar`, `/logistica/paletizar-fabricacion` | Montaje. |
| GET | `/logistica/lectura?codigo=` | Lector. |

## Garantías de la base de datos

- **RLS** por empresa en las raíces y por cabecera en `linea_unidad_logistica`.
- **CHECK** de:
  - Prefijo GS1, GTIN y SSCC.
  - Estados, tipos y orígenes.
  - Cantidades y pesos no negativos.
  - Mosaicos positivos.
  - Temperatura mínima ≤ máxima.
  - Que una unidad no sea su propio padre.
- **Claves foráneas** diferidas:
  - Ficha, plantilla y unidad → soporte.
  - Unidad → plantilla.
  - Unidad → unidad padre.
- **Referencias a otros módulos.** Las columnas que referencian artículos, clientes, almacenes y
  ubicaciones están en `ReferenciasRegistros`, así que impiden borrar esos maestros.
- **Borrado de empresa.** La baja de la empresa borra su esquema logístico.

## Siguientes fases

1. **Picking** guiado desde el pedido, en el móvil o con la pistola.
2. **Planificación de la carga:** vehículos, peso por eje, paradas y muelles.
3. **Expedición:** albarán desde los palés cargados, DESADV, POD y tarifas de transporte.
4. **Migración** de los palés de Agro a este núcleo.
