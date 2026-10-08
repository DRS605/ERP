# Paquete "Tienda" — instalación local en Windows (sin Docker)

Genera un **paquete autónomo para Windows 10/11** que instala ALXOR Core en el PC de
la tienda sin Docker y sin instalar .NET: la API se publica *self-contained* (lleva su
propio runtime) y PostgreSQL se obtiene **portátil** (sin instalador ni servicio, dentro
de la propia carpeta).

## Cómo generar el paquete

```bash
bash paquete-tienda/crear-paquete.sh
```

Produce `paquete-tienda/_dist/AlxorCore-Tienda.zip`. Ese ZIP es lo que se copia al PC
de la tienda. (El build self-contained y el ZIP no se versionan: ver `.gitignore`.)

## Qué lleva dentro el ZIP

```
AlxorCore-Tienda/
  LEEME-PRIMERO.txt          # instrucciones para la usuaria (lenguaje llano)
  ArrancarTienda.bat         # doble clic: arranca BD + ERP y abre el navegador
  DetenerTienda.bat          # doble clic: detiene todo
  CargarDatosDeEjemplo.bat   # opcional: siembra datos de demo
  app/                       # API self-contained (AlxorCore.Api.exe + wwwroot + .NET)
  scripts/                   # arrancar.ps1, detener.ps1, datos-demo.ps1
```

En el primer arranque descarga PostgreSQL portátil (~300 MB, una sola vez; necesita
internet solo entonces), hace `initdb`, crea la base `alxor` y arranca la API en
`ASPNETCORE_ENVIRONMENT=Development` (así aplica el esquema automáticamente). El ERP
queda en `http://localhost:8080`.

## Ajustes

Todo es editable en `scripts/arrancar.ps1`:

- `$PuertoApi` (8080) y `$PuertoPg` (5433) — cámbialos si algún puerto está ocupado.
- `$ClaveJwt` — clave de firma de sesión. Para una instalación real, pon una propia.
- `$UrlPostgres` — si la URL de EDB cambiara, se puede dejar un `postgres-binaries.zip`
  en la carpeta y el lanzador lo usa sin descargar.

## Datos y copias de seguridad

Los datos viven en la subcarpeta `datos/` del propio paquete. Copiar toda la carpeta
= llevarse los datos. Conviene copiar `datos/` periódicamente como respaldo.

## Nota

Es una **versión de prueba** local. La parte fiscal (VeriFactu) se activa más adelante;
no es un despliegue de producción (sin HTTPS, sin backups automáticos, auth `trust` en
PostgreSQL local). Para producción multiempresa se recomienda PostgreSQL gestionado.
