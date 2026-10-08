# DesinstalarServicio.ps1 — Quita los servicios de ALXOR Core (no borra los datos).
# EJECUTAR COMO ADMINISTRADOR.
#Requires -RunAsAdministrator
$ErrorActionPreference = "SilentlyContinue"

$Base  = Split-Path -Parent $PSScriptRoot
$nssm  = Join-Path $Base "nssm\nssm.exe"
$PgBin = Join-Path $Base "postgres\pgsql\bin"

Write-Host "Quitando el servicio ALXOR-Core..." -ForegroundColor Cyan
if (Test-Path $nssm) { & $nssm stop "ALXOR-Core"; & $nssm remove "ALXOR-Core" confirm }

Write-Host "Quitando el servicio ALXOR-Postgres..." -ForegroundColor Cyan
Stop-Service "ALXOR-Postgres"
if (Test-Path (Join-Path $PgBin "pg_ctl.exe")) { & (Join-Path $PgBin "pg_ctl.exe") unregister -N "ALXOR-Postgres" }

Write-Host "Hecho. Los datos siguen en la carpeta 'datos' (no se han borrado)." -ForegroundColor Green
