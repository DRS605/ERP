# arrancar.ps1 — Arranca ALXOR Core (ERP) en este PC.
# Levanta un PostgreSQL portatil (dentro de esta misma carpeta), prepara la base de
# datos, arranca el ERP y abre el navegador. No instala nada en el sistema.
$ErrorActionPreference = "Stop"

# --- Rutas: todo vive dentro de la carpeta del paquete ---
$Base    = Split-Path -Parent $PSScriptRoot         # carpeta AlxorCore-Tienda
$PgRoot  = Join-Path $Base "postgres"
$PgBin   = Join-Path $PgRoot "pgsql\bin"
$DataDir = Join-Path $Base "datos"
$LogDir  = Join-Path $Base "logs"
$AppExe  = Join-Path $Base "app\AlxorCore.Api.exe"

# --- Ajustes (cambialos solo si algun puerto esta ocupado) ---
$PuertoApi   = 8080
$PuertoPg    = 5433
$UrlPostgres = "https://get.enterprisedb.com/postgresql/postgresql-16.4-1-windows-x64-binaries.zip"
$ClaveJwt    = "alxor-core-tienda-local-clave-de-firma-32-min-cambia-esto"

New-Item -ItemType Directory -Force -Path $LogDir | Out-Null
function Log($m, $c = "Gray") { Write-Host $m -ForegroundColor $c }

# --- 1) PostgreSQL portatil (solo la primera vez) ---
if (-not (Test-Path (Join-Path $PgBin "pg_ctl.exe"))) {
    $zipLocal = Join-Path $Base "postgres-binaries.zip"
    if (-not (Test-Path $zipLocal)) {
        Log "Primera vez: descargando PostgreSQL portatil (~300 MB, solo esta vez)..." "Cyan"
        try {
            Invoke-WebRequest -Uri $UrlPostgres -OutFile $zipLocal -UseBasicParsing
        } catch {
            Log "No se ha podido descargar PostgreSQL automaticamente." "Red"
            Log "Descarga manual: postgresql.org/download/windows -> 'Zip archives'." "Yellow"
            Log "Guarda el ZIP como 'postgres-binaries.zip' en esta carpeta y vuelve a ejecutar." "Yellow"
            throw
        }
    }
    Log "Extrayendo PostgreSQL..." "Cyan"
    Expand-Archive -Path $zipLocal -DestinationPath $PgRoot -Force
}

# --- 2) Crear la base de datos local la primera vez ---
if (-not (Test-Path (Join-Path $DataDir "PG_VERSION"))) {
    Log "Preparando la base de datos por primera vez..." "Cyan"
    & (Join-Path $PgBin "initdb.exe") -D $DataDir -U postgres -A trust -E UTF8 --locale=C | Out-Null
}

# --- 3) Arrancar PostgreSQL si no esta ya en marcha ---
$pgCtl = Join-Path $PgBin "pg_ctl.exe"
& $pgCtl -D $DataDir status *> $null
if ($LASTEXITCODE -ne 0) {
    Log "Arrancando PostgreSQL..." "Cyan"
    & $pgCtl -D $DataDir -l (Join-Path $LogDir "postgres.log") -o "-p $PuertoPg -c listen_addresses=localhost" -w start | Out-Null
} else {
    Log "PostgreSQL ya estaba en marcha." "DarkGray"
}

# --- 4) Crear la base 'alxor' si no existe ---
$psql   = Join-Path $PgBin "psql.exe"
$existe = (& $psql -h localhost -p $PuertoPg -U postgres -tAc "SELECT 1 FROM pg_database WHERE datname='alxor'") 2>$null
if (("$existe").Trim() -ne "1") {
    Log "Creando la base de datos 'alxor'..." "Cyan"
    & (Join-Path $PgBin "createdb.exe") -h localhost -p $PuertoPg -U postgres alxor
}

# --- 5) Configurar y arrancar el ERP ---
$env:ASPNETCORE_ENVIRONMENT       = "Development"   # aplica el esquema automaticamente
$env:ASPNETCORE_URLS              = "http://localhost:$PuertoApi"
$env:ConnectionStrings__AlxorCore = "Host=localhost;Port=$PuertoPg;Database=alxor;Username=postgres"
$env:Jwt__ClaveSecreta            = $ClaveJwt

# Abrir el navegador en cuanto el ERP responda (en segundo plano).
$abrir = Start-Job -ArgumentList $PuertoApi -ScriptBlock {
    param($p)
    $u = "http://localhost:$p"
    for ($i = 0; $i -lt 90; $i++) {
        try {
            if ((Invoke-WebRequest "$u/salud" -UseBasicParsing -TimeoutSec 2).StatusCode -eq 200) {
                Start-Process $u; break
            }
        } catch { }
        Start-Sleep -Seconds 1
    }
}

Log ""
Log "=============================================================" "Green"
Log "  ALXOR Core esta arrancando..." "Green"
Log "  Se abrira solo en el navegador en cuanto este listo:" "Green"
Log "      http://localhost:$PuertoApi" "Green"
Log "  Para apagarlo: cierra esta ventana o usa DetenerTienda.bat" "DarkGray"
Log "=============================================================" "Green"
Log ""

try {
    & $AppExe
} finally {
    Remove-Job $abrir -Force -ErrorAction SilentlyContinue
    Log "Deteniendo PostgreSQL..." "DarkGray"
    & $pgCtl -D $DataDir -m fast stop *> $null
}
