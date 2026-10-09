# actualizar.ps1 — Aplica una actualización de Core Evolution sin reinstalar nada.
# Para el servicio, copia las novedades sobre la carpeta 'app' y lo vuelve a arrancar.
# EJECUTAR COMO ADMINISTRADOR.
#Requires -RunAsAdministrator
$ErrorActionPreference = "Stop"

$Here = Split-Path -Parent $PSScriptRoot        # carpeta AlxorCore-Update
$Src  = Join-Path $Here "app"

# Localizar la instalación existente.
$cands = @(
    (Join-Path $env:USERPROFILE "AlxorCore-Tienda"),
    (Join-Path $env:USERPROFILE "Downloads\AlxorCore-Tienda"),
    (Join-Path ([Environment]::GetFolderPath('Desktop')) "AlxorCore-Tienda")
)
$Base = $cands | Where-Object { Test-Path (Join-Path $_ "app\AlxorCore.Api.exe") } | Select-Object -First 1
if (-not $Base) { throw "No encuentro la carpeta AlxorCore-Tienda. Dime dónde está." }
$Dest = Join-Path $Base "app"
Write-Host "Actualizando: $Dest" -ForegroundColor Cyan

# Parar el servicio (o el proceso manual) para poder sustituir los archivos.
$svc = Get-Service "ALXOR-Core" -ErrorAction SilentlyContinue
if ($svc) { Write-Host "Deteniendo el servicio..." -ForegroundColor Cyan; Stop-Service "ALXOR-Core" -Force; Start-Sleep -Seconds 2 }
Get-Process AlxorCore.Api -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 1

# Copiar las novedades (DLLs de código, appsettings y la web) sobre la instalación.
robocopy $Src $Dest /E /IS /IT /NFL /NDL /NJH /NJS | Out-Null
Write-Host "Archivos actualizados." -ForegroundColor Green

# Rearrancar.
if ($svc) {
    Start-Service "ALXOR-Core"
    Write-Host "Servicio rearrancado." -ForegroundColor Green
} else {
    Write-Host "No hay servicio instalado: arranca con ArrancarTienda.bat cuando quieras." -ForegroundColor Yellow
}
Write-Host ""
Write-Host "Listo. Core Evolution actualizado. Abre http://localhost:8080 (Ctrl+F5 para refrescar)." -ForegroundColor Green
