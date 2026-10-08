#!/usr/bin/env bash
# Ensambla el paquete instalable para Windows (sin Docker): publica la API
# self-contained (lleva su propio .NET), le anade PostgreSQL portatil bajo demanda
# mediante los lanzadores, y empaqueta todo en AlxorCore-Tienda.zip.
#
#   bash paquete-tienda/crear-paquete.sh
#
# Requiere el SDK de .NET 8 (o el wrapper 'dotnet' por Docker de este repo) y 'zip'.
set -euo pipefail

cd "$(dirname "$0")/.."
RAIZ="$(pwd)"
PT="$RAIZ/paquete-tienda"
BUILD="$PT/_build/app"
DIST="$PT/_dist/AlxorCore-Tienda"

# 1) Publicar la API self-contained para Windows si no esta ya hecha.
if [ ! -f "$BUILD/AlxorCore.Api.exe" ]; then
  echo ">> Publicando la API (self-contained win-x64)..."
  dotnet publish src/AlxorCore.Api/AlxorCore.Api.csproj \
    -c Release -r win-x64 --self-contained true \
    -p:TreatWarningsAsErrors=false -o "$BUILD"
fi

# 2) Ensamblar la carpeta distribuible.
echo ">> Ensamblando la carpeta..."
rm -rf "$PT/_dist"
mkdir -p "$DIST/scripts"
cp -r "$PT/plantilla/." "$DIST/"
cp -r "$BUILD" "$DIST/app"
cp "$RAIZ/scripts/datos-demo.ps1" "$DIST/scripts/datos-demo.ps1"

# 3) Comprimir en un .zip que Windows abre con doble clic.
echo ">> Comprimiendo..."
( cd "$PT/_dist" && zip -r -q "AlxorCore-Tienda.zip" "AlxorCore-Tienda" )

echo ""
echo "Paquete listo:"
echo "   $PT/_dist/AlxorCore-Tienda.zip"
du -sh "$PT/_dist/AlxorCore-Tienda.zip" | awk '{print "   tamano: "$1}'
