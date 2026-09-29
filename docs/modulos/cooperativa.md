# Cooperativas y SAT

Módulo sectorial para sociedades cooperativas (agrarias o de cualquier otro tipo) y sociedades agrarias de
transformación (SAT). Recoge lo que la ley les exige llevar: socios, capital social, distribución del excedente
con su retorno cooperativo, retenciones y libros registro.

- **Código de módulo:** `cooperativa`. Es sectorial: no entra en ninguna edición y se contrata aparte. Con agro
  contratado, la actividad de cada socio sale de sus liquidaciones.
- **Esquema:** `cooperativa`, proyectos `AlxorCore.Cooperativa` y `AlxorCore.Cooperativa.Infraestructura`.
- **Rutas:** `/cooperativa/*`.
- **Permisos:**
  - `cooperativa.leer`: ver socios, capital, repartos, retenciones y libros.
  - `cooperativa.gestionar`: altas y bajas, operaciones de capital, repartos y actas.
  - Los ajustes se cambian con `empresa.ajustes`.
  - La plantilla de rol *Administración* lleva los dos permisos.
- **Puertos** (los implementa la API):
  - `IActividadSocios`: los kilos y el importe de las liquidaciones de agro **emitidas**, por tercero.
  - `IContabilidadCooperativa`: los asientos y sus contraasientos en contabilidad.

## Ajustes

| Ajuste | Por defecto | Para qué |
|---|---|---|
| Forma jurídica | Cooperativa | Una **SAT** no tiene fondos obligatorios (0 % y 0 %) y reparte por capital. |
| Aportación obligatoria mínima | 0 € | Lo que exigen los estatutos para ser socio. El alta se niega por debajo del mínimo, y la lista de socios muestra lo que falta a cada uno. |
| Fondo de reserva (FRO) y de educación (FEP) mínimos | 20 % y 5 % | Porcentajes mínimos del excedente. Dependen de la ley que aplique (estatal o autonómica) y de los estatutos. |
| Interés máximo al capital | 9,25 % | Tope de los intereses que acuerde la asamblea: el interés legal del dinero más seis puntos, o lo que diga la ley aplicable. |
| Retención | 19 % | Sobre retornos e intereses que se **pagan**, que son rendimientos del capital mobiliario. |
| Retorno según | Kilos | Kilos entregados, importe liquidado o capital desembolsado. |
| Deducción máxima | 30 % expulsión, 20 % baja no justificada | Tope de la deducción sobre las aportaciones obligatorias al reembolsar. |
| Contabilizar | Sí | Si las operaciones generan su asiento. |
| Cuentas | 100, 103, 572, 552, 129, 112, 144, 113, 526, 4751 | Capital, desembolsos pendientes, tesorería, reembolsos a socios, resultado, FRO, FEP, reservas voluntarias, retornos a pagar y retenciones. Son orientativas: se ajustan al plan de la entidad (adaptación contable a cooperativas). |

## Socios

Un socio es un tercero, con su ficha de proveedor, que tiene el NIF y el domicilio.

- **Número:** correlativo; no se reutiliza ni cambia.
- **Clases:** común, de trabajo, colaborador e inactivo. En una cooperativa, los colaboradores y los inactivos no
  tienen retorno, pero sí intereses por su capital.
- **Alta:** puede llevar la aportación obligatoria y lo que se desembolsa en el acto.
- **Baja:** es definitiva. Lleva un motivo: voluntaria justificada, voluntaria no justificada, obligatoria,
  expulsión o fallecimiento. La baja puede reembolsar todo el capital a la vez, con la deducción que permita el
  motivo sobre lo obligatorio.
- **Si vuelve:** es un socio nuevo, con otro número.
- **Eliminar:** solo se elimina un socio dado de alta por error, que no tenga capital ni repartos.

## Capital social

El libro de aportaciones es la lista de movimientos de cada socio en cada clase (obligatorio o voluntario). Cada
movimiento cambia lo **suscrito** y lo **desembolsado**.

| Movimiento | Qué hace | Asiento |
|---|---|---|
| Suscripción | El socio suscribe capital y, si quiere, lo desembolsa en el acto. | 103 contra 100 por lo suscrito, y 572 contra 103 por lo desembolsado (neto por cuenta). |
| Desembolso | Paga capital ya suscrito. No se puede pasar de lo pendiente. | 572 contra 103. |
| Reembolso | Devuelve capital: primero se cancela lo pendiente y luego lo desembolsado. El obligatorio solo se reembolsa con el socio de baja. La deducción solo cabe en el obligatorio. | 100 al debe; al haber 103 por lo pendiente, 552 por lo que se le devuelve y FRO por la deducción. |
| Transmisión | Cede capital desembolsado a otro socio de alta. Son dos movimientos unidos. | Ninguno: el capital total no cambia. |
| Retorno capitalizado | El retorno que el socio deja como capital (voluntario). | Lo lleva el asiento del reparto. |
| Anulación | Deshace otro movimiento con el signo cambiado. Una transmisión se anula entera, y su asiento, con contraasiento. | Contraasiento. |

El pago de lo que se devuelve (552) o de los retornos (526) se hace en tesorería, como cualquier pago.

## Reparto del excedente

1. **Propuesta.** Se calcula (`/repartos/simular`) y se guarda en borrador, que se recalcula las veces que haga
   falta. Solo hay un reparto vivo, borrador o contabilizado, por ejercicio.
2. **Cálculo:**
   - FRO y FEP salen del excedente con sus porcentajes, que no pueden bajar de los mínimos.
   - Los intereses al capital salen del capital desembolsado al cierre del ejercicio.
   - Lo que queda, descontadas las reservas voluntarias, es el **retorno**.
3. **Actividad.** El retorno se reparte según la actividad de cada socio en el ejercicio, o en las fechas que se
   indiquen, por ejemplo la campaña:
   - kilos o importe de sus liquidaciones emitidas;
   - su capital, en una SAT;
   - o la actividad que se indique a mano, sin agro.

   Se reparte al céntimo: la diferencia de redondeo va al socio de más actividad. El socio que se dio de baja durante
   el ejercicio cobra por lo que entregó.
4. **Capitalizar.** Cada socio de alta puede dejar su retorno como capital. Lo capitalizado no lleva retención; lo
   que se paga, sí (retorno más intereses).
5. **Contabilizar.** Se hace cuando la asamblea lo aprueba:
   - 129 al debe por el excedente;
   - al haber FRO, FEP, reservas voluntarias, 526 por lo que se paga, 4751 por las retenciones y 100 por lo capitalizado;
   - lo capitalizado entra en el capital de cada socio.
6. **Anular.** Hace el contraasiento y quita lo capitalizado del capital. Si ese capital ya se había devuelto, la
   base de datos no lo deja.

**Retenciones** (`/retenciones?ejercicio=`): lo retenido a cada socio en el año, con su NIF. Sirve de base de los
modelos 123 (trimestral) y 193 (resumen anual).

## Libros

- **Registro de socios:** todos, también los de baja, con número, nombre, NIF, domicilio de su ficha, clase, alta,
  baja con motivo y capital.
- **Registro de aportaciones al capital:** cada movimiento con el capital acumulado del socio.
- **Actas:** un libro por órgano (asamblea general, consejo rector o, en una SAT, junta rectora). El acta se redacta
  en borrador y al **aprobarla** recibe su número. Así, eliminar un borrador no deja hueco en el libro. Aprobada ya
  no cambia ni se borra.

## Garantías de la base de datos

- RLS por empresa en todas las tablas; las líneas del reparto, por su cabecera.
- **Socio:**
  - su número, su tercero y su alta no cambian;
  - la baja no se deshace;
  - un tercero solo es socio de alta una vez.
- **Movimientos de capital:**
  - solo se insertan;
  - cada tipo tiene sus signos (`CHECK`).
- **Al confirmar la transacción, un movimiento de capital tiene que cumplir esto:**
  - lo desembolsado del socio en cada clase no queda en negativo ni pasa de lo suscrito;
  - la anulación es exactamente el movimiento anulado con el signo cambiado;
  - la transmisión sale de un socio y entra en otro por el mismo importe;
  - a un socio de baja solo se le reembolsa o se le cede el capital;
  - el obligatorio solo se reembolsa con la baja;
  - el retorno capitalizado es de un reparto contabilizado, y su anulación de uno anulado.
- **Reparto:**
  - el excedente es la suma de fondos, reservas, intereses y retorno;
  - cada línea cuadra (retorno + intereses = neto + retención + capitalizado);
  - al confirmar, las líneas suman el retorno y los intereses;
  - contabilizado no cambia y solo se anula;
  - sus líneas no cambian.
- **Actas:**
  - aprobada, ni cambia ni se borra;
  - al confirmar, el libro de cada órgano va numerado sin huecos.
- La baja de la empresa borra todo lo del módulo.
