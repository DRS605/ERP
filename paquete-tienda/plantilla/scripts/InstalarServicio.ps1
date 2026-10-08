# InstalarServicio.ps1 — Deja ALXOR Core como SERVICIO de Windows (siempre arrancado).
# Registra PostgreSQL como servicio nativo y el ERP como servicio (via NSSM), ambos con
# arranque automatico al encender el PC. EJECUTAR COMO ADMINISTRADOR.
#Requires -RunAsAdministrator
$ErrorActionPreference = "Stop"

$Base    = Split-Path -Parent $PSScriptRoot
$PgBin   = Join-Path $Base "postgres\pgsql\bin"
$DataDir = Join-Path $Base "datos"
$AppDir  = Join-Path $Base "app"
$AppExe  = Join-Path $AppDir "AlxorCore.Api.exe"
$LogDir  = Join-Path $Base "logs"

$PuertoApi = 8080
$PuertoPg  = 5433
$ClaveJwt  = "alxor-core-tienda-local-clave-de-firma-32-min-cambia-esto"
$SvcPg     = "ALXOR-Postgres"
$SvcApi    = "ALXOR-Core"

New-Item -ItemType Directory -Force -Path $LogDir | Out-Null
function Log($m, $c = "Gray") { Write-Host $m -ForegroundColor $c }

if (-not (Test-Path $AppExe)) { throw "Falta $AppExe. Ejecuta antes ArrancarTienda.bat una vez." }
if (-not (Test-Path (Join-Path $PgBin "pg_ctl.exe"))) { throw "Falta PostgreSQL. Ejecuta antes ArrancarTienda.bat una vez." }

# 0) Cerrar cualquier instancia manual (ventanas abiertas).
Log "Cerrando instancias manuales si las hay..." "Cyan"
Get-Process AlxorCore.Api -ErrorAction SilentlyContinue | Stop-Process -Force
& (Join-Path $PgBin "pg_ctl.exe") -D $DataDir -m fast stop *> $null
Start-Sleep -Seconds 2

# 1) PostgreSQL como servicio.
if (-not (Get-Service $SvcPg -ErrorAction SilentlyContinue)) {
    Log "Registrando el servicio $SvcPg..." "Cyan"
    & (Join-Path $PgBin "pg_ctl.exe") register -N $SvcPg -D $DataDir -S auto -o "-p $PuertoPg -c listen_addresses=localhost"
}
Set-Service $SvcPg -StartupType Automatic
Start-Service $SvcPg
for ($i = 0; $i -lt 30; $i++) {
    & (Join-Path $PgBin "pg_isready.exe") -h localhost -p $PuertoPg *> $null
    if ($LASTEXITCODE -eq 0) { break }
    Start-Sleep -Seconds 1
}
Log "  PostgreSQL en marcha." "Green"

# 2) Base 'alxor' si no existe.
$hay = (& (Join-Path $PgBin "psql.exe") -h localhost -p $PuertoPg -U postgres -tAc "SELECT 1 FROM pg_database WHERE datname='alxor'") 2>$null
if (("$hay").Trim() -ne "1") {
    & (Join-Path $PgBin "createdb.exe") -h localhost -p $PuertoPg -U postgres alxor
    Log "  Base 'alxor' creada." "Green"
}

# 3) NSSM: gestor que convierte el ERP (consola) en servicio de Windows.
$nssm = Join-Path $Base "nssm\nssm.exe"
if (-not (Test-Path $nssm)) {
    Log "Descargando NSSM (gestor de servicios)..." "Cyan"
    $zip = Join-Path $Base "nssm.zip"; $tmp = Join-Path $Base "nssm_tmp"
    Invoke-WebRequest "https://nssm.cc/release/nssm-2.24.zip" -OutFile $zip -UseBasicParsing
    Expand-Archive $zip $tmp -Force
    New-Item -ItemType Directory -Force -Path (Split-Path $nssm) | Out-Null
    Copy-Item (Join-Path $tmp "nssm-2.24\win64\nssm.exe") $nssm -Force
    Remove-Item $zip -Force; Remove-Item $tmp -Recurse -Force
}

# 4) Servicio del ERP.
if (Get-Service $SvcApi -ErrorAction SilentlyContinue) {
    Log "Reinstalando el servicio $SvcApi..." "Cyan"
    & $nssm stop $SvcApi 2>$null | Out-Null
    & $nssm remove $SvcApi confirm 2>$null | Out-Null
    Start-Sleep -Seconds 2
}
Log "Instalando el servicio $SvcApi..." "Cyan"
& $nssm install $SvcApi $AppExe | Out-Null
& $nssm set $SvcApi AppDirectory $AppDir | Out-Null
& $nssm set $SvcApi AppEnvironmentExtra `
    "ASPNETCORE_ENVIRONMENT=Development" `
    "ASPNETCORE_URLS=http://localhost:$PuertoApi" `
    "ConnectionStrings__AlxorCore=Host=localhost;Port=$PuertoPg;Database=alxor;Username=postgres" `
    "Jwt__ClaveSecreta=$ClaveJwt" | Out-Null
& $nssm set $SvcApi Start SERVICE_AUTO_START | Out-Null
& $nssm set $SvcApi DependOnService $SvcPg | Out-Null
& $nssm set $SvcApi AppStdout (Join-Path $LogDir "api.log") | Out-Null
& $nssm set $SvcApi AppStderr (Join-Path $LogDir "api.log") | Out-Null
& $nssm start $SvcApi | Out-Null

# 5) Esperar a que responda.
Log "Esperando a que el ERP responda..." "Cyan"
$ok = $false
for ($i = 0; $i -lt 90; $i++) {
    try { if ((Invoke-WebRequest "http://localhost:$PuertoApi/salud" -UseBasicParsing -TimeoutSec 2).StatusCode -eq 200) { $ok = $true; break } } catch { }
    Start-Sleep -Seconds 1
}

Log ""
if ($ok) {
    Log "=============================================================" "Green"
    Log "  Listo. ALXOR Core instalado como SERVICIO." "Green"
    Log "  Arranca solo con Windows, en segundo plano:" "Green"
    Log "      http://localhost:$PuertoApi" "Green"
    Log "  Servicios creados: $SvcPg y $SvcApi" "DarkGray"
    Log "=============================================================" "Green"
    Start-Process "http://localhost:$PuertoApi"
} else {
    Log "El servicio quedo instalado pero el ERP aun no responde." "Yellow"
    Log "Revisa el detalle en: $LogDir\api.log" "Yellow"
}
