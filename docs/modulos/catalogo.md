# Módulo Catálogo

Gestión de **productos y servicios** y catálogo de **impuestos (IVA)**.

## Impuestos

Los tipos de IVA españoles (21 %, 10 %, 4 %, 0 %) se modelan como **catálogo de código**
(`AlxorCore.Nucleo.Comun.Impuesto`), no como datos editables: son tipos nacionales y estables. Las
facturas guardarán una **copia del porcentaje** aplicado, de modo que un cambio futuro de tipos no
altere las facturas ya emitidas. `GET /impuestos` los expone.

## Productos

`Producto` { Referencia (opcional), Nombre, Tipo (Bien/Servicio), **PrecioUnitario** (venta),
**PrecioCompra** (coste, para el margen; 0 si no aplica), CodigoIva por defecto (validado contra el
catálogo), Unidad, **ProveedorHabitualId** (proveedor habitual del artículo; referencia opcional a
Terceros), **FamiliaId** (familia/categoría opcional del artículo, ver más abajo; usada por las reglas
de contabilización para elegir la cuenta de ingreso/gasto), Activo }. Multiempresa (RLS por empresa).

Al añadir un producto a una factura se prerrellenan su precio de venta, su IVA y también su
**precio de compra**, que la factura **congela por línea** (`coste_unitario`) para que el margen del
informe de beneficio sea fiel aunque el coste cambie después.

## Familias (árbol de subfamilias)

Las familias clasifican los artículos y se organizan en **árbol**: cada `Familia` puede colgar de una
familia padre (`PadreId`), lo que permite crear **subfamilias** con la profundidad que haga falta
(p. ej. «Alimentación › Bebidas › Refrescos»). Las familias raíz no tienen padre. Es un **catálogo
normalizado** que sustituye al antiguo texto libre del artículo: evita erratas («Servicios» vs
«servicios») y da un vocabulario reutilizable.

`Familia` { Nombre, **Codigo** (corto, opcional), **PadreId** (null = raíz), Activo }. Multiempresa
(RLS por empresa). Invariantes: nombre obligatorio; una familia **no puede ser su propio padre** ni
colgar de una de sus **descendientes** (se detecta el ciclo comparando ascendencias); no se puede
**eliminar** una familia con subfamilias o con artículos (se protege el borrado; en su lugar se puede
**desactivar**). El árbol se recorre en memoria para calcular la **ruta completa** («A › B › C») y el
**nivel** de cada familia, y para servir el árbol anidado.

El artículo se enlaza a una familia por `FamiliaId`. Para no romper las **reglas de contabilización**
(que casan por *nombre* de familia), el texto `Producto.Familia` se mantiene **sincronizado** con el
nombre de la familia enlazada; si el artículo no tiene familia enlazada, ese texto queda como valor
libre (compatibilidad con importaciones antiguas). Al asignar una familia se valida que exista y esté
activa.

Endpoints: `GET /familias` (lista plana con ruta y nivel), `GET /familias/arbol` (árbol anidado),
`POST /familias`, `PUT /familias/{id}` (renombrar, recolocar bajo otro padre, activar/desactivar) y
`DELETE /familias/{id}` (solo hojas sin artículos). La gestión se hace desde **Artículos → Familias**;
el formulario de artículo ofrece la familia en un desplegable jerárquico.

### Unidades y envases

El artículo tiene una **unidad base** (`Unidad`) —la unidad canónica en la que se guardan las
existencias y se expresan los precios— y, opcionalmente, una **unidad de compra** y una **unidad de
venta** distintas, cada una con su **factor de conversión** (cuántas unidades base contiene):

- `UnidadCompra` + `FactorCompra` (p. ej. *caja* = 12 ud): se compra en cajas pero el stock se lleva
  en unidades.
- `UnidadVenta` + `FactorVenta` (p. ej. *garrafa* = 5 l): se compra a granel y se vende en envases, o
  al revés (envases que se venden partidos).

Los factores deben ser **> 0** (por defecto 1 = misma unidad que la base). El dominio ofrece las
conversiones puras `CompraABase(q)` y `VentaABase(q)` y los precios derivados
`PrecioCompraPorUnidadCompra` y `PrecioVentaPorUnidadVenta`. **Todo lo demás del sistema opera en
unidad base** (inventario, facturación y —a futuro— producción), y la conversión se hace solo en los
bordes: p. ej. al **recibir un pedido de compra**, la cantidad recibida (en unidad de compra) se
convierte a unidades base antes de dar entrada al almacén. Así el stock y el consumo de producción
son siempre inequívocos.

### Trazabilidad: lote o número de serie

El artículo declara su **modo de seguimiento** (`Seguimiento`): `Ninguno` (por defecto), `Lote`
(varias unidades comparten un código de lote; útil para caducidades o control sanitario) o `Serie`
(cada unidad es única). `RequiereLoteOSerie` indica si sus movimientos de stock deben llevar lote.
El seguimiento vive en el artículo; la trazabilidad efectiva (existencias y movimientos por lote) la
lleva el módulo **Inventario**. Al **recibir un pedido**, la UI exige el lote/nº de serie de las
líneas trazadas y lo propaga a la entrada de almacén.

### Artículos compuestos (lista de materiales)

Un artículo puede ser **compuesto** (`EsCompuesto`): se fabrica a partir de otros mediante una
**lista de materiales** (`Componentes`), donde cada componente es otro artículo con una cantidad (en
la unidad base del componente) por unidad del compuesto. Reglas: al menos un componente, cantidades
`> 0`, sin autorreferencia ni componentes repetidos. El dominio ofrece `Explosionar(cantidad)` (qué y
cuánto hace falta para fabricar N unidades) y las consultas calculan el **escandallo** (coste
agregado = Σ coste_componente × cantidad). Esta lista de materiales es la base del futuro módulo de
**producción**: el módulo **Inventario** ya la usa en el **montaje** (`/inventario/montaje`), que
consume los componentes del almacén y da entrada del artículo compuesto de forma atómica.

### Variantes de artículo

Un artículo puede tener **variantes** (talla, color, sabor…). Cada variante es **un artículo real**
(su propio SKU): hereda del padre el tipo, IVA, unidad, factores y seguimiento, y tiene su propia
**referencia**, **precio** y **stock**. El padre queda marcado como **plantilla** (`EsPlantilla`) y
la variante guarda `ProductoPadreId` + sus **atributos** (`Atributos`: eje → valor, p. ej.
Talla=M, Color=Rojo); `ResumenVariante` los muestra («M · Rojo»). No se permiten variantes de una
variante. `POST /productos/{id}/variantes` crea una variante y `GET /productos/{id}/variantes` las
lista. Como cada variante es un artículo normal, funcionan sobre ella —sin nada extra— el stock, los
lotes/series, la composición y las unidades; producción podrá fabricar variantes igual que cualquier
artículo.

## Histórico de precios

Cada alta de producto y cada **cambio de precio** (de venta o de compra) añade una fila a
`historico_precio` { ProductoId, PrecioVenta, PrecioCompra, RegistradoEn } (RLS por empresa,
solo-inserción). Permite ver la **evolución de precios** de un artículo en el tiempo.
`GET /productos/{id}/precios` la expone (más reciente primero).

## Stock (existencias)

Un producto puede llevar **control de stock** (`ControlarStock`). Los servicios normalmente no; una
tienda sí. Cuando está activo, el artículo tiene existencias (`Stock`) y cada variación queda
registrada en `movimiento_stock` { ProductoId, Tipo, Cantidad (con signo), StockResultante, Motivo,
CreadoEn } (RLS por empresa, histórico inmutable).

- **Movimientos manuales**: `Entrada` (compra/reposición), `Salida` (merma, rotura) y `Ajuste`
  (fija el stock al valor contado en un recuento).
- **Descuento automático por venta**: al emitir una **factura** o un **ticket**, Facturación llama al
  puerto `IStockVentas` (implementado por Catálogo), que registra un movimiento de tipo `Venta` por
  cada línea con producto que lleve control de stock. Es **mejor esfuerzo**: los servicios y los
  artículos sin control se ignoran, y la factura —verdad fiscal— ya está emitida.

## API

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/impuestos` | JWT | Tipos de IVA disponibles. |
| `GET` | `/productos` | JWT + empresa | Lista de productos activos. |
| `GET` | `/productos/{id}` | JWT + empresa | Obtiene un producto. |
| `GET` | `/productos/{id}/precios` | JWT + empresa | Histórico de precios del producto. |
| `POST` | `/productos` | permiso `producto.gestionar` | Crea un producto. **201** |
| `PUT` | `/productos/{id}` | permiso `producto.gestionar` | Actualiza un producto. |
| `GET` | `/productos/{id}/stock` | JWT + empresa | Histórico de movimientos de stock. |
| `POST` | `/productos/{id}/stock` | permiso `producto.gestionar` | Registra un movimiento de stock. |
| `GET` | `/familias` | JWT + empresa | Familias en plano (con ruta completa y nivel). |
| `GET` | `/familias/arbol` | JWT + empresa | Árbol de familias (subfamilias anidadas). |
| `POST` | `/familias` | permiso `producto.gestionar` | Crea una familia o subfamilia. **201** |
| `PUT` | `/familias/{id}` | permiso `producto.gestionar` | Renombra, recoloca o (des)activa una familia. |
| `DELETE` | `/familias/{id}` | permiso `producto.gestionar` | Elimina una familia hoja sin artículos. **204** |

La importación CSV admite una columna opcional de **precio de compra** (`precio compra`, `coste`,
`compra`).

## Persistencia

Esquema **`catalogo`**, tablas `producto`, `familia`, `historico_precio` y `movimiento_stock` (RLS por
empresa). El repositorio ofrece escritura (`IRepositorioProductos`, `IRepositorioFamilias`,
`IRepositorioHistoricoPrecios`, `IRepositorioMovimientosStock`) y consultas (`IConsultaProductos`,
`IConsultaFamilias`, `IConsultaHistoricoPrecios`, `IConsultaMovimientosStock`), que consumirán
**Facturación** e **Informes**.

## Tests

- **Unitarios**: catálogo de IVA y validaciones de `Producto` (nombre, precio, precio de compra, IVA);
  dominio de `Familia` (nombre, no ser su propio padre, recolocar, activar/desactivar) y utilidades del
  árbol (ruta completa, nivel, detección de ancestros para evitar ciclos).
- **Integración**: listar impuestos, CRUD de productos, histórico de precios (alta + cambios),
  aislamiento por empresa; familias (subfamilias con ruta, árbol anidado, veto de ciclos, asignación a
  artículo con sincronización del texto, y protección de borrado con subfamilias/artículos).
