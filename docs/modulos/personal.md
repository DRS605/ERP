# Módulo Personal

Mantenimiento de **personas** (empleados y colaboradores) con su **tarifa por hora**, para poder
**imputar mano de obra** a proyectos y a los partes de producción. Multiempresa (RLS).

## Concepto

Una **persona** (`Persona`) representa a alguien del equipo con un coste horario (`TarifaHora`, €/h) y,
opcionalmente, un **puesto** («Oficial 1ª», «Ingeniero»…). Cada persona pertenece a una empresa
(`empresa_id`, con RLS) y puede marcarse como **inactiva** cuando causa baja, sin borrarla, para
conservar el histórico de imputaciones.

El módulo es deliberadamente pequeño: solo el mantenimiento (alta, edición, baja lógica y listado).
La **valoración de la mano de obra** (persona × horas × tarifa) la consumirá el módulo **Proyectos**
(y, en el futuro, los partes de producción) a través del puerto de aplicación `IConsultaPersonas`.

## Reglas (invariantes)

- El **nombre** es obligatorio (máx. 150 caracteres).
- La **tarifa por hora** no puede ser negativa; se **redondea a 2 decimales**.
- La baja es **lógica** (`Activo = false`): el listado por defecto solo muestra las personas activas.

## API

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| `GET` | `/personal` | `personal.leer` | Lista las personas activas de la empresa con su tarifa. |
| `POST` | `/personal` | `personal.gestionar` | Crea una persona. **201** |
| `PUT` | `/personal/{id}` | `personal.gestionar` | Actualiza nombre, puesto, tarifa y estado (activa/inactiva). |

Cuerpo (`DatosPersona`): `{ nombre, tarifaHora, puesto?, activo? }`.

## Puertos (fronteras entre módulos)

- `IConsultaPersonas` (en `AlxorCore.Personal.Aplicacion`): expone `ObtenerAsync(personaId)` y
  `ListarAsync(empresaId, incluirInactivas)`. Lo usarán la API y el módulo **Proyectos** para imputar
  mano de obra sin acceder a la base de datos de Personal.

## Permisos y roles

- Permisos `personal.leer` y `personal.gestionar`.
- **Propietario**: ambos. **Usuario**: ambos. **Solo lectura**: solo `personal.leer`.

## Persistencia

- Esquema **`personal`**: tabla `persona` (RLS por empresa; índice `ix_persona_empresa` por
  `empresa_id, activo`). Migración `MigracionInicialPersonal`.

## Tests

- **Unitarios**: validación y redondeo de la tarifa, nombre obligatorio, puesto en blanco → nulo,
  y actualización (incluida la baja lógica) sin mutar ante datos inválidos.
- **Integración**: alta, listado (solo activas) y edición con baja lógica; rechazo de tarifa
  negativa (400).

## Interfaz

Entrada de menú **Personal**: tabla con buscador/orden/columnas configurables (como el resto del ERP)
y alta/edición en un modal (nombre, puesto, tarifa y, al editar, el interruptor de persona activa).

## Futuro (documentado)

Imputación de horas a **proyectos** y a **partes de producción**, con el coste calculado según la
tarifa de la persona; posibles tarifas por periodo/coste empresa (SS) más adelante.
