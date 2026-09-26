# Extracción de Hispatec

Scripts para generar el paquete de migración a ALXOR desde la base de datos de Hispatec (SQL Server). Solo leen.

- `diagnostico.sql`: comprobaciones previas. Confirma cómo interpretar los enumerados y mide los problemas de integridad.
- `extraer_paquete.sql`: una consulta por archivo del paquete (marcas `-- @archivo`).
- `exportar.ps1`: ejecuta lo anterior y genera el ZIP.

Uso y formato: [docs/migracion-hispatec.md](../../docs/migracion-hispatec.md).
