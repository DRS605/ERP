#!/usr/bin/env bash
# Genera un paquete de ACTUALIZACION ligero (solo DLLs de código + web + appsettings),
# para actualizar una instalación de la tienda sin reinstalar ni re-descargar PostgreSQL.
#   bash paquete-tienda/crear-actualizacion.sh
set -euo pipefail
cd "$(dirname "$0")/.."
RAIZ="$(pwd)"; PT="$RAIZ/paquete-tienda"
PUB="$PT/_update/publish"; OUT="$PT/_update/AlxorCore-Update"

# SPA primero (vuelca a wwwroot/app), luego publish.
( cd frontend && npm run build )
rm -rf "$PT/_update"
dotnet publish src/AlxorCore.Api/AlxorCore.Api.csproj -c Release -o "$PUB" -p:TreatWarningsAsErrors=false

mkdir -p "$OUT/app" "$OUT/scripts"
cp "$PUB"/AlxorCore.*.dll "$OUT/app/"
cp "$PUB"/appsettings*.json "$OUT/app/" 2>/dev/null || true
cp -r "$PUB"/wwwroot "$OUT/app/wwwroot"
cp -r "$PT/plantilla-update/." "$OUT/"

( cd "$PT/_update" && zip -r -q AlxorCore-Update.zip AlxorCore-Update )
echo "Actualización lista: $PT/_update/AlxorCore-Update.zip"
du -sh "$PT/_update/AlxorCore-Update.zip"
