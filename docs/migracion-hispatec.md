# Migración desde Hispatec

Fase 6 del [plan de producto](plan-producto.md). Trae a ALXOR los datos vivos de una empresa de Hispatec
(ERPHispatec, SQL Server) para arrancar con ellos. Se basa en el análisis del esquema de Hispatec que hay en
`DRS605/Clon/docs/hispatec`:
- `MAPA.md`, con las áreas y los hallazgos de integridad;
- `esquema_compacto.txt`, tabla a tabla.

> **Sin validar con datos reales.** Los scripts de extracción usan solo tablas y columnas que existen en el
> esquema. Se han comprobado contra el listado de columnas extraído (`esquema/2_columnas.tsv`). Pero no se han ejecutado sobre la base
> real, porque no hay acceso a ella. Hay que confirmar los puntos marcados «CONFIRMAR»
> (ver [§5](#5-puntos-a-confirmar)) con `diagnostico.sql` antes de la primera migración.

## 1. Qué se migra y qué no

**Se migran** los datos vivos a una **fecha de corte**, normalmente el cierre de un ejercicio:

| Hispatec | ALXOR | Cómo |
|---|---|---|
| `Sujetos` + `Clientes` / `ClientesEmpresas` | Clientes | El tercero único de Hispatec se desdobla por rol. Los sujetos con el mismo NIF se **fusionan**. |
| `Proveedores` / `ProveedoresEmpresa` y `Acreedores` / `AcreedoresEmpresa` | Proveedores | Acreedores (transporte, servicios) y proveedores son lo mismo en ALXOR. |
| Dirección fiscal, correo, IBAN (`DireccionesSujeto`, `CorreosElectronicosSujeto`, `CuentasBancariasSujeto`) | Datos del tercero | La dirección por defecto y la primera cuenta. |
| `FamiliasArticulo`, `Articulos`, `UnidadesMedida` | Familias (con su árbol) y artículos | Por código. Los precios están en tarifas y se cargan aparte. |
| `CuentasContables` | Plan de cuentas | Las subcuentas de cliente y proveedor (430…, 400…, 410…) se **enlazan con su tercero**, con el mismo código. |
| `Apuntes` del ejercicio hasta el corte | **Asiento de apertura** el día siguiente al corte | Con los saldos calculados desde los apuntes. |
| `DocumentosCobro` / `DocumentosPago` no liquidados | **Efectos de cartera** en Tesorería | Se cobran y se pagan en ALXOR como cualquier documento. |
| `Campanyas` | Campañas (agro) | |
| `ProveedoresAgro` + `AutorizaEmitirFacturas` | Agricultores (agro) | En el REAGP. Si autorizaba la autofacturación, desde la fecha de corte. |
| `Cultivos` vivos de `Fincas`, con `ReferenciaSIGPAC` | Parcelas del agricultor (agro) | Una parcela por cultivo vivo, con su SIGPAC, superficie, cultivo y variedad. |

**No se migran:**
- **El histórico de documentos** (facturas, albaranes, asientos anteriores). Hispatec se queda como
  consulta: rehacer facturas emitidas en ALXOR rompería su numeración y su cadena VeriFactu.
- **Recursos humanos.** No se migran trabajadores ni tablas laborales (decisión de producto).
- **Tampoco, por ahora:**
  - existencias y partidas abiertas;
  - tarifas de precios;
  - campos definidos por el usuario (`DatosComplementarios`);
  - cuaderno de campo.

  Ver [§6](#6-pendiente).

## 2. Cómo se hace

1. **En el servidor de Hispatec**, primero el diagnóstico (solo lee) y luego el paquete:

   ```powershell
   .\herramientas\hispatec\exportar.ps1 -Servidor SQL01 -BaseDatos ERPHispatec -Empresa 01 -FechaCorte 2026-12-31 -Diagnostico
   .\herramientas\hispatec\exportar.ps1 -Servidor SQL01 -BaseDatos ERPHispatec -Empresa 01 -FechaCorte 2026-12-31
   ```

   Genera `paquete-hispatec-01-2026-12-31.zip`.
2. **En ALXOR**, con la empresa de destino seleccionada (y el módulo agro contratado, si procede), ir a
   *Ajustes → Migración desde Hispatec*:
   - **Validar.** Informa sin tocar nada: filas válidas por archivo, errores (filas que no se cargan), avisos y
     cuadres.
   - **Cargar.** Carga todo lo válido. Se puede **repetir**: lo ya migrado se salta y lo que ya existía se
     reutiliza, así que si algo falla a medias se corrige y se vuelve a lanzar.

API: `POST /migracion/hispatec/validar`, `POST /migracion/hispatec/cargar` (cuerpo `{ contenidoBase64 }`) y
`GET /migracion/hispatec/ejecuciones`. Permiso `empresa.ajustes`.

## 3. El paquete

Es un ZIP de CSV en UTF-8, con separador `;`, decimales con punto, fechas `aaaa-mm-dd` y booleanos 0/1.
Formato `alxor-hispatec`, versión 1. Los identificadores llevan un prefijo por rol (`C`, `P`, `A`, `DC`, `DP`),
porque en Hispatec cada rol tiene su tabla y sus `Id` se repiten.

| Archivo | Columnas |
|---|---|
| `manifiesto.csv` (obligatorio) | `clave;valor`: formato, versión, empresa, **fecha_corte** |
| `terceros.csv` (obligatorio) | id_sujeto, nif, nombre, calle, codigo_postal, poblacion, provincia, pais, email, iban |
| `clientes.csv` (obligatorio) | id, codigo, id_sujeto, subcuenta |
| `proveedores.csv` (obligatorio) | id, codigo, id_sujeto, subcuenta, agricultor, autoriza_autofactura, retencion |
| `familias.csv` | id, codigo, nombre, id_superior |
| `articulos.csv` | id, codigo, nombre, id_familia, unidad, iva, tipo, controla_stock, precio_venta, precio_compra |
| `cuentas.csv` | codigo, nombre |
| `saldos.csv` | cuenta, debe, haber (saldo neto a la fecha de corte) |
| `acumuladores.csv` | cuenta, debe, haber (lo que dice `Acumuladores`, solo para comparar) |
| `cartera.csv` | id, sentido (cobro/pago), id_tercero, documento, fecha_documento, vencimiento, importe |
| `campanas.csv` | id, codigo, nombre, desde, hasta |
| `parcelas.csv` | id, codigo, nombre, id_proveedor, sigpac, superficie_ha, id_articulo, variedad |

Se puede generar con otra herramienta, siempre que respete este formato.

## 4. Lo que la validación detecta

Son las reglas que la base de datos de Hispatec no garantiza (ver los hallazgos de `MAPA.md`):

| Comprobación | Nivel | Por qué |
|---|---|---|
| Cliente, proveedor o efecto que apunta a un sujeto o tercero que no existe | Error (la fila no se carga) | `DocumentosCobro` no tiene ninguna clave foránea; 396 columnas `Id_*` sin FK. |
| Saldos cuyo debe no iguala al haber | Error que **bloquea la carga** | Un asiento de apertura descuadrado no se puede contabilizar. |
| Cuenta cuyo saldo en `Acumuladores` difiere del de sus apuntes | Aviso (se usan los apuntes) | Los acumuladores se mantienen con triggers de incremento y decremento, y se desincronizan. |
| Cartera de cobro distinta del saldo de las 43, o la de pago distinta de las 40 y 41 | Aviso | Anticipos, efectos en gestión o apuntes sin efecto: conviene conciliarlo antes de arrancar. |
| NIF repetido en varios sujetos | Aviso (se fusionan) | Hispatec permite el mismo NIF con distinto sufijo o tipo de sujeto. |
| NIF español inválido, tercero sin NIF | Aviso | Se carga tal cual para corregirlo. |
| Familia que cuelga de otra inexistente, artículo con familia inexistente | Aviso | Quedan en la raíz o sin familia. |
| Códigos de artículo o de parcela repetidos | Aviso | Se carga el primero. |
| Campañas que se solapan | Error | En ALXOR no pueden solaparse. |
| Parcela de un proveedor que no es agricultor | Error | |
| Estructura: archivo o columna que falta, versión o formato desconocidos, sin fecha de corte | Error que bloquea | |

## 5. Puntos a confirmar

`diagnostico.sql` saca lo necesario para confirmar cada punto antes de migrar:

| Punto | Supuesto del script | Comprobación |
|---|---|---|
| `Apuntes.TipoApunte` | 0 = debe, 1 = haber | D1-D2: con esa lectura los asientos deben cuadrar |
| `Acumuladores.TipoSaldo` | 0 = saldo normal | D3-D4 |
| Efectos vivos | `Liquidado = 0` | D5-D6: estados de cobros y pagos |
| `Articulos.TipoImpositivo` | Porcentaje de IVA | D13 |
| Artículo o servicio | Servicio si no afecta a inventario y es «varios» | D14 |
| `Paises.Codigo` | ISO de 2 letras | Si no, el país queda en España |
| `Fincas.Id_Representante` | Es el agricultor | D10 |
| `FincaSubrecinto.Id_Subrecinto` | Apunta a `CatastroParcelas` (sin FK declarada) | D11 |
| Superficies de los cultivos | En hectáreas | D12 |
| Retención del agricultor | No se extrae (`ClavesRetencion`); se usa el 2 % | Revisar en la ficha |
| Régimen del agricultor | REAGP | Revisar los de régimen general |

## 6. Pendiente

- **Existencias:** partidas y palés abiertos a la fecha de corte (`ArticulosPartida`, `NumerosSeriePartida`),
  como partidas de entrada en el módulo agro, y stock del inventario general.
- **Datos:** tarifas de precios (`TarifasVenta`, `PreciosVenta`) y campos definidos por el usuario.
- **Contabilidad analítica:** imputaciones y presupuestos (`AnaliticaImputaciones`).
- **Contabilidad multiempresa:** varias empresas del mismo grupo en una sola carga. Hoy es una carga por empresa;
  los terceros y artículos, que son del grupo, se reutilizan en la segunda.

## 7. Garantías

- **Módulos:** todo se carga con los **casos de uso** de cada módulo, así que se aplican las mismas validaciones
  y garantías que al darlo de alta a mano (RLS, numeración, cuadre del asiento…).
- **Correspondencia con Hispatec:** `migracion.correspondencia` guarda qué registro de Hispatec corresponde a
  cuál de ALXOR, y `migracion.ejecucion` registra cada carga. Ambas son de **solo inserción**.
- **Cartera:** los efectos de cartera (`tesoreria.efecto_cartera`) tampoco se modifican. Se cobran, se pagan o
  se compensan con movimientos, sin superar el pendiente.
- **Baja de la empresa:** borra también todo lo migrado.
