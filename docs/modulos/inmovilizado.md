# Inmovilizado y amortizaciones

Submódulo de **Contabilidad** para la gestión del inmovilizado (activos fijos): alta de activos con
**amortización contable y fiscal independientes**, generación de las dotaciones (asientos), el
**impuesto diferido** por la diferencia temporaria, y la **baja** y **enajenación** del activo. Todo
genera asientos de partida doble reutilizando el libro diario del módulo Contabilidad. Multiempresa
(RLS sobre `inmovilizado`; las dotaciones y ajustes se protegen a través de su activo).

## Métodos de amortización

Cada activo lleva **dos planes** —contable y fiscal— con su propio método y vida útil, porque la
norma fiscal suele permitir amortizar más rápido que la imagen fiel contable. Métodos soportados
(clase pura `CalculadoraAmortizacion`, con tests unitarios):

| Método | Cuota del año | Uso típico |
|---|---|---|
| **Lineal** | `base / vida útil` (constante) | La inmensa mayoría de activos. |
| **Degresivo** | `saldo pendiente × %` (decreciente); el último año amortiza el resto | Activos que pierden valor rápido. El `%` es un parámetro; si es 0 se usa `2/n`. |
| **Dígitos** (suma de dígitos) | `base × (n−k+1) / Σdígitos` (decreciente) | Alternativa acelerada clásica. |

La base amortizable es `valor de adquisición − valor residual`. Sea cual sea el método, **Σcuotas =
base** (el último año absorbe el redondeo).

### Prorrateo y periodicidad

Las cuotas anuales se reparten **mes a mes** desde la *puesta en funcionamiento* (`fecha_alta`): cada
año de vida = 12 meses, y cada mes recibe `cuota_anual / 12`. Así, un activo que entra en
funcionamiento a mitad de año amortiza solo los meses en servicio. La **periodicidad es elegible por
activo**:

- **Anual**: un único asiento de dotación al 31/12 con la suma del año natural.
- **Mensual**: un asiento por mes (a fin de mes) con su cuota.

El asiento de dotación es: **(Debe)** cuenta de dotación `68x` · **(Haber)** amortización acumulada
`28x` (cuenta correctora del activo).

## Impuesto diferido (contable vs fiscal)

La diferencia entre la amortización fiscal y la contable de cada ejercicio es una **diferencia
temporaria** que revierte a lo largo de la vida del activo (Σcontable = Σfiscal = base). Por ella se
genera —una vez al año, si está activado— el asiento de **impuesto diferido** al tipo del Impuesto de
Sociedades (25 % por defecto, configurable):

- **Fiscal > contable** (se amortiza más rápido fiscalmente → menor base imponible ahora): nace un
  **pasivo por diferencia temporaria imponible** → `(Debe) 6301 Impuesto diferido / (Haber) 479`.
- **Contable > fiscal** (reversión, o amortización contable mayor): nace/aumenta un **activo por
  diferencia temporaria deducible** → `(Debe) 4740 / (Haber) 6301`.

El cálculo se basa en la **diferencia temporaria acumulada** del activo: rutea el efecto a la 479
mientras la acumulada es imponible y a la 4740 mientras es deducible, partiendo el asiento si el año
cruza el cero. Así los asientos **se revierten solos** y netean a cero al terminar la vida del activo.

## Baja y enajenación

- **Baja** (sin contraprestación): `(Debe) 28x` amortización acumulada + `(Debe) 671` valor neto
  pendiente (pérdida) / `(Haber) 2xx` valor de adquisición. El activo queda `DadoDeBaja`.
- **Enajenación** (venta): `(Debe) 28x` + `(Debe)` cuenta de cobro (572) por precio + IVA /
  `(Haber) 2xx` valor de adquisición + `(Haber) 477` IVA repercutido, y el **resultado** (precio −
  valor neto) como `(Haber) 771` beneficio o `(Debe) 671` pérdida. El activo queda `Enajenado`.

Ambas operaciones usan la amortización acumulada **ya contabilizada**; conviene ejecutar la
amortización del periodo antes de la baja/venta si procede. Un activo dado de baja o enajenado ya no
amortiza.

## Idempotencia y ejercicios cerrados

`Generar amortización` es **idempotente**: no vuelve a dotar un periodo (año o mes) ya contabilizado,
ni repite el ajuste fiscal de un ejercicio. Ninguna operación (dotación, baja, enajenación) puede
asentarse en un **ejercicio cerrado** (409 `asiento.ejercicio_cerrado`).

## Balance de situación

Al incorporar activos y su amortización acumulada, el **balance de situación** del módulo Contabilidad
trata las cuentas `28x` como **correctoras del activo** (saldo con signo): minoran el activo no
corriente en lugar de sumarse, de modo que el balance sigue cuadrando.

## API

Todos los endpoints cuelgan de `/contabilidad/inmovilizado`.

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/contabilidad/inmovilizado` | `contabilidad.leer` | Lista los inmovilizados (con acumulada y valor neto). |
| `POST` | `/contabilidad/inmovilizado` | `contabilidad.gestionar` | Alta de un inmovilizado. **201** |
| `GET` | `/contabilidad/inmovilizado/{id}/cuadro` | `contabilidad.leer` | Cuadro de amortización (contable, fiscal, diferencia) por ejercicio. |
| `POST` | `/contabilidad/inmovilizado/amortizar?ejercicio=&tipoImpuesto=&impuestoDiferido=` | `contabilidad.gestionar` | Genera la dotación del ejercicio y el impuesto diferido. |
| `POST` | `/contabilidad/inmovilizado/{id}/baja` | `contabilidad.gestionar` | Baja del inmovilizado (asiento con pérdida). |
| `POST` | `/contabilidad/inmovilizado/{id}/enajenar` | `contabilidad.gestionar` | Enajenación (asiento con beneficio/pérdida). |

## Plan de cuentas

Se siembran, junto al plan básico, las cuentas necesarias: inmovilizado material (211, 213, 216, 217,
218), amortización acumulada (280, 281), dotaciones (680, 681), resultados por el inmovilizado (671,
771) e impuesto diferido (4740, 479, 6301).

## Persistencia

- Esquema **`contabilidad`**: tabla `inmovilizado` (con los planes contable y fiscal como columnas
  propias y `periodicidad`/`estado`), y las colecciones propias `dotacion_amortizacion` (periodos
  contabilizados, idempotencia por año/mes) y `ajuste_fiscal_amortizacion` (impuesto diferido por
  ejercicio).
- RLS por empresa en `inmovilizado`; índice único `(empresa_id, codigo)`.
- Migración: `Inmovilizado`.

## Tests

- **Unitarios** (`InmovilizadoTests`): las cuotas de los tres métodos suman la base; el reparto mensual
  prorratea el primer y último año; el impuesto diferido crea el pasivo, lo revierte y no genera asiento
  sin diferencia; validaciones de la entidad (residual, baja/enajenación únicas).
- **Integración** (`InmovilizadoEndpointsTests`): la amortización anual dota y genera el impuesto
  diferido (479/6301); es idempotente; el cuadro muestra contable/fiscal/diferencia; el balance de
  situación cuadra con la amortización acumulada como correctora; la enajenación con beneficio registra
  el 771 y bloquea futuras amortizaciones; la baja lleva el valor neto a pérdidas (671).
