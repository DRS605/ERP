# Analítica y presupuestos

Módulo contratable **`analitica`** (necesita Contabilidad). Incluido en las ediciones Finanzas,
Gestión y finanzas y Completa. Rutas: `/contabilidad/analitica/*` y `/contabilidad/presupuestos/*`.

Parte del modelo de Hispatec, que funciona bien en empresas agrícolas y de servicios, y corrige sus
puntos débiles (ver [§6](#6-comparación-con-hispatec)).

## 1. Dos dimensiones

| Dimensión | Pregunta | Ejemplos |
|---|---|---|
| **Centro** | ¿Dónde se produce el coste o el ingreso? | Centro de coste, departamento, proyecto, **finca**, **campaña** |
| **Partida** | ¿Qué es? | Mano de obra, fitosanitarios, energía, venta de fruta |

- Los dos son **maestros del grupo**: las empresas del holding comparten centros y partidas.
- Son **árboles libres**, de cualquier profundidad: un centro suma a sus hijos en el informe.
- La **partida es opcional**. Sin partidas, el análisis se hace por cuenta del PGC, que es lo que
  necesita una pyme generalista.
- Solo se imputan los **gastos (grupo 6)** y los **ingresos (grupo 7)**, con su signo de gestión: un
  abono en un gasto es menos coste y una devolución de ventas es menos ingreso.

## 2. Cómo llegan los importes a los centros

1. **Reglas, al contabilizar.** Cada regla tiene:
   - **Criterios:** cuenta (prefijo), tercero, actividad de negocio y familia del artículo, con fechas de vigencia.
   - **Destino:** un centro, o una **clave de reparto** (porcentajes que suman 100 %), y opcionalmente una partida.
   - **Precedencia:** gana la de mayor **prioridad**. A igualdad, la más concreta: la que tiene más criterios, y
     entre esas, tercero antes que actividad, actividad antes que familia, y familia antes que el prefijo de
     cuenta más largo.
2. **A mano:**
   - En el **asiento manual**, cada línea de gasto o ingreso puede llevar su centro y su partida.
   - Un apunte ya contabilizado se imputa por **porcentajes o importes**, y puede quedar una parte pendiente.
3. **Proceso «imputar pendientes».** Aplica las reglas vigentes a lo que no tiene imputación: lo que se
   contabilizó antes de crear la regla, las amortizaciones, etc.
4. **Reparto secundario** (costes indirectos). Traspasa lo imputado a un centro, por ejemplo
   *Administración*, a otros centros con una clave, cuenta a cuenta y partida a partida.
   - El centro de origen queda a cero y el total no cambia.

Los procesos quedan registrados (quién, cuándo, qué periodo) y se **deshacen enteros**.

Los repartos son **exactos al céntimo** (método del mayor resto): 100 € al 33,33/33,33/33,34 % dan
33,33 + 33,33 + 33,34, y nunca 99,99 ni 100,01.

## 3. Periodos analíticos

Ejercicios de gestión independientes del fiscal, como una **campaña** de septiembre a agosto.

- Los periodos de una empresa **no se solapan**.
- Un periodo **cerrado** congela sus imputaciones.
- El informe se puede pedir por periodo o por fechas.

## 4. Informe

`GET /contabilidad/analitica/informe?desde&hasta` (o `?periodoId`) con `&dimension=Centro|Partida`:

- **Por fila:** ingresos, gastos, resultado y margen, con el árbol sumado y el detalle por cuenta.
- **Sin asignar:** lo que queda sin imputar va a esa fila.
- **Porcentaje imputado:** qué parte de los gastos e ingresos del periodo tiene centro.
- **Totales:** coinciden con la cuenta de pérdidas y ganancias del periodo (sin los asientos de
  regularización, cierre y apertura).

`GET /pendientes` lista los apuntes con importe sin imputar y **explica qué regla se les aplicaría**.

## 5. Presupuestos contables

- **Periodo:** de 1 a 24 meses desde cualquier mes (año natural o campaña).
- **Líneas:** por **cuenta o grupo de cuentas** (un prefijo: `62` es todo el subgrupo 62) y, si se quiere,
  por centro y partida.
- **Importes:** los de cada mes, o un total que se reparte al céntimo.
- **Generar desde el real** de otro periodo, mes a mes, con un **incremento** (por cuenta, o por centro y
  partida con lo imputado).
- **Aprobar:** el presupuesto queda **congelado** (también en la base de datos). Para cambiarlo se **copia**
  a una versión nueva, con incremento si se quiere.
- **Seguimiento** hasta un mes:
  - presupuesto y real, mes a mes y acumulado;
  - desviación, porcentaje de ejecución y si es **favorable** (gastar menos, o ingresar más, de lo previsto);
  - sin centro, el real sale de la contabilidad; con centro, de la analítica, incluidos los centros hijos;
  - aviso si dos líneas cuentan el mismo real (por ejemplo, «62» y «629» sin centro).

## 6. Comparación con Hispatec

Esquema analizado en `DRS605/Clon/docs/hispatec`: `AnaliticaEstructuras/Grupos/Subgrupos/Partidas`,
`AnaliticaProyectos`, `AnaliticaImputaciones` (845.000 filas), `AnaliticaRepartos`,
`AnaliticaProyectoPorEntidad(+Temporal)`, `AnaliticaPartidaPorEntidadActividadProduccion`,
`AnaliticaProyectoRepartoRealizado` (4.154 ejecuciones y 237.000 imputaciones generadas),
`AnaliticaEjercicio`, `AnaliticaProyectoBloqueo` y `AnaliticaPresupuestos`.

| Hispatec | ALXOR |
|---|---|
| Proyecto × Partida | Centro × Partida (se mantiene; la partida es opcional) |
| Árbol fijo Estructura → Grupo → Subgrupo → Partida, con códigos de 3 caracteres | Árboles libres de cualquier profundidad, en centros y partidas |
| Origen de la imputación polimórfico (`TipoOrigen` + `Id_Origen`), sin FK, vigilado por unos 15 triggers de «existencia» | Clave foránea real al apunte y al asiento |
| FK duplicadas y contradictorias (Grupos→Estructura y Partidas→Subgrupo, una `CASCADE` y otra `NO ACTION`) | Una FK por relación, con su índice (lo vigila un test del esquema) |
| Nada impide imputar más que el apunte; `Porcentaje` e `Importe` guardados a la vez | La base rechaza lo que supera el apunte o cambia de signo; se guarda solo el importe, repartido al céntimo |
| Reglas en dos tablas (proyecto por entidad y partida por entidad) con muchas columnas opcionales y precedencia poco clara | Una regla con criterios combinables, vigencia y precedencia explícita; «pendientes» explica cuál aplica |
| `AcumuladorContableAnalitico` mantenido por triggers | Sin acumuladores: el informe se calcula siempre y no se desincroniza |
| Repartos realizados, reversibles | Igual, y además la base comprueba que cada reparto suma cero |
| Bloqueo de proyectos por fechas | Periodos cerrados que la base de datos hace inalterables; centros inactivos |
| Ejercicio analítico por empresa | Igual (periodos) y sin solapes, garantizado en la base |
| Presupuesto analítico (año × proyecto × partida × 12 meses) sin uso (0 filas) | Presupuesto por cuenta o grupo, centro y partida, de 1 a 24 meses, generado desde el real, con versiones y seguimiento |
| Repartos por fórmula y por pesos (tablas sin uso) | No se portan: claves de reparto y reparto secundario cubren lo que se usa |

**Coste por unidad (fase 5):** el [módulo agro](agro.md) divide el gasto imputado al centro de cada parcela
entre los kilos que produce en la campaña (coste de cultivo por kilo), y valora la confección por kilo.

**Pendiente:**
- Imputación desde documentos operativos (partes de confección, albaranes) directamente a centros, con
  cantidades, como `AnaliticaLinea` de Hispatec.
- Mano de obra entre empresas del grupo.

## 7. Garantías en la base de datos

Todas se comprueban aunque se escriba saltándose la aplicación. Ver
[garantías de la base de datos](../garantias-base-datos.md).

- **Aislamiento:** RLS por grupo en centros, partidas y claves; por empresa en reglas, periodos,
  imputaciones, ejecuciones y presupuestos; las líneas heredan la de su cabecera.
- **Maestros del propio grupo:** no se puede imputar a un centro ni a una partida de otro grupo.
- **Imputación coherente con su apunte** (cuenta, fecha, asiento y empresa), sin superarlo ni cambiar de signo.
- **Cuadres:** cada reparto secundario suma cero y cada clave suma exactamente 100.
- **Periodos:** los cerrados no admiten cambios y no se solapan.
- **Presupuestos:** cada línea tiene un importe por mes y ninguno es negativo; un presupuesto aprobado no cambia.
