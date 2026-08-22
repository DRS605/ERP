# Cumplimiento antifraude (Ley 11/2021 · RD 1007/2023 · VeriFactu)

Cómo ALXOR Core cubre los requisitos del software de facturación antifraude, y qué queda fuera de
alcance a día de hoy.

## Requisitos y cómo se cubren

| Requisito | Estado | Dónde |
|---|---|---|
| **Integridad e inalterabilidad** de las facturas emitidas | ✅ | La factura emitida es **inmutable** (invariante F2): sin métodos de edición/borrado en el dominio y **sin endpoints PUT/DELETE**. Los datos fiscales se congelan al emitir (F4). |
| **Conservación** (no se borran) | ✅ | No hay borrado físico de facturas. Corregir = **rectificativa**; dejar sin efecto = **anulación** (cambia de estado, no se elimina). |
| **Trazabilidad / registro de eventos** | ✅ | Cada emisión, rectificación y anulación queda en el **registro de auditoría** (append-only, por empresa) y, además, genera un **registro VeriFactu con huella encadenada**. |
| **Huella (hash) y encadenamiento** | ✅ | SHA-256 sobre la cadena canónica AEAT; cada registro encadena la huella del anterior de la empresa. Alta en `Factura.RegistrarVerifactu`, anulación en `Factura.Anular` (`Verifactu.CalcularHuellaAnulacion`). La cadena incluye altas **y** anulaciones (`UltimaHuellaAsync`). |
| **Registro de facturación de alta** | ✅ | Se genera al emitir; XML de `RegistroAlta` descargable en `GET /facturas/{id}/verifactu.xml`. |
| **Registro de anulación** | ✅ | `POST /facturas/{id}/anular` con motivo obligatorio: estado → `Anulada`, huella de anulación encadenada. **No borra** la factura. |
| **Facturas rectificativas** | ✅ | `POST /facturas/{id}/rectificar` (tipo **R1** por sustitución): referencia a la original, motivo obligatorio, la original queda `Rectificada`. |
| **QR de cotejo** | ✅ | URL de cotejo AEAT en el QR del PDF de factura/ticket. |
| **Numeración correlativa sin huecos** | ✅ | Asignación atómica por serie/ejercicio (bloqueo de fila). |

## Anular vs. rectificar (guía rápida)

- **Rectificar**: cuando hay que **corregir importes/datos** de una factura correcta pero equivocada.
  Emite una rectificativa R1 encadenada; la original pasa a `Rectificada`.
- **Anular**: cuando la factura **no debió existir** (duplicada, emitida por error). No se borra:
  pasa a `Anulada`, con motivo y registro de anulación encadenado. Solo desde `Emitida` (una
  rectificada o ya anulada, no).

Ambas acciones están en la lista de facturas (botones **Rectificar** y **Anular**), solo sobre
facturas ordinarias emitidas.

## Fuera de alcance (documentado)

- **Envío en vivo a la AEAT** (servicio web VeriFactu/SII con certificado): el registro se genera y
  almacena localmente (`EstadoEnvioAeat = "Registrado"`); el envío telemático real es un paso
  posterior. Documentado en `Verifactu.cs` y `GeneradorXmlVerifactu.cs`.
- Algunos campos del XML (`ClaveRegimen`, `CalificacionOperacion`) van con valores por defecto
  (`01`/`S1`); afinarlos por operación es una mejora futura.

> Nota: este documento describe el soporte del software; la conformidad formal (declaración
> responsable / certificación) corresponde al titular conforme a la normativa vigente.
