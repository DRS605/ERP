# Módulo Contabilidad (partida doble)

Contabilidad por **partida doble**: plan de cuentas (PGC), **asientos** que cuadran, **libro diario**,
**libro mayor** y **balance de sumas y saldos**. Es la "capa 2" de la contabilización: se enchufa en
el puerto `IContabilizador` del módulo Recepción, de modo que contabilizar una factura de proveedor
puede además generar su asiento, según el **modo de contabilidad** de la empresa. Multiempresa (RLS).

## Modo de contabilidad (por empresa)

Cada empresa elige su modo (`GET`/`PUT /contabilidad/modo`):

- **Simple** (por defecto): contabilizar = registrar un **gasto** con IVA soportado (Libro de IVA,
  303/130). Es lo que necesita un autónomo. No genera asientos.
- **Completo**: además del gasto, genera el **asiento** de partida doble (libro diario y mayor).

El adaptador `ContabilizadorSegunModo` implementa el puerto `IContabilizador` de Recepción y decide
según el modo. Así, activar la partida doble **no cambia** el flujo de recepción ni el de gastos.

## Asiento de una factura de proveedor (modo Completo)

Para una factura de base `B`, IVA `i %` y retención `r %`:

| Cuenta | Debe | Haber |
|---|---|---|
| `629` Otros servicios (gasto) | `B` | |
| `472` H.P. IVA soportado | `B · i%` | |
| `4751` H.P. acreedora por retenciones | | `B · r%` |
| `400` Proveedores | | `B + B·i% − B·r%` |

El asiento **cuadra** por construcción (Σ debe = Σ haber). El número es correlativo por empresa y
ejercicio (el ejercicio se deriva del año de la fecha).

## Invariantes

- **Cuadre**: un asiento no se crea si la suma del debe ≠ la suma del haber, o si su importe es cero.
- **Apunte válido**: cada apunte carga en el debe **o** abona en el haber (no ambos ni ninguno), con
  importes no negativos. Mínimo dos apuntes.
- **Inmutable**: un asiento no se edita una vez creado (las correcciones se hacen con otro asiento).

## API

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/contabilidad/cuentas` | `contabilidad.leer` | Plan de cuentas (se siembra si falta). |
| `GET` | `/contabilidad/diario?ejercicio=` | `contabilidad.leer` | Libro diario del ejercicio. |
| `GET` | `/contabilidad/mayor/{codigo}?ejercicio=` | `contabilidad.leer` | Libro mayor de una cuenta (con saldo acumulado). |
| `GET` | `/contabilidad/balance?ejercicio=` | `contabilidad.leer` | Balance de sumas y saldos. |
| `POST` | `/contabilidad/asientos` | `contabilidad.gestionar` | Crea un asiento manual. **201** |
| `GET` | `/contabilidad/modo` | `contabilidad.leer` | Modo de contabilidad actual. |
| `PUT` | `/contabilidad/modo` | `contabilidad.gestionar` | Cambia el modo (Simple / Completo). |

## Plan de cuentas

Se siembra por empresa un subconjunto común del PGC (400, 430, 472, 477, 475, 4751, 570, 572, 600,
621–629, 700, 705…) la primera vez que se consulta el plan o se genera un asiento. Es ampliable (los
asientos referencian cuentas por su código; el plan da el nombre para los informes).

## Persistencia

- Esquema **`contabilidad`**: `cuenta`, `asiento` (con `apunte` como colección propia), `config_contabilidad`.
- RLS por empresa en `cuenta`, `asiento` y `config_contabilidad`; el `apunte` se protege a través de
  su `asiento` (filtro global de EF Core).
- Índices únicos: `(empresa_id, codigo)` en cuenta y `(empresa_id, ejercicio, numero)` en asiento.
- Migración: `MigracionInicialContabilidad` (incluye la activación de RLS).

## Composición

- Recepción define el puerto `IContabilizador`; Contabilidad lo implementa con `ContabilizadorSegunModo`
  (registrado **después** de Recepción para sustituir el adaptador por defecto).
- En modo Completo, contabilizar crea el **gasto** (módulo Gastos) **y** el **asiento** (este módulo).
  No es una transacción única entre módulos: si el asiento fallara, se devuelve el error (mejora futura:
  outbox/transacción distribuida).

## Tests

- **Unitarios**: cuadre del asiento (cuadrado/descuadrado, apunte debe-y-haber, mínimo de apuntes) y
  validación del código de cuenta.
- **Integración**: modo por defecto Simple; alta de asiento manual y su aparición en el diario;
  asiento descuadrado → 400; y el flujo en modo Completo (contabilizar una factura genera el asiento
  de partida doble que cuadra: 629/472 al debe, 400 al haber).

## Futuro (documentado)

Ejercicios con cierre/apertura, cuentas de resultados (PyG) y balance de situación clasificado,
amortizaciones, y numeración de asientos 100 % sin huecos ante fallos.
