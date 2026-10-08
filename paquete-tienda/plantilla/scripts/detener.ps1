# detener.ps1 — Detiene ALXOR Core (el ERP y su PostgreSQL) de forma limpia.
$ErrorActionPreference = "SilentlyContinue"

$Base    = Split-Path -Parent $PSScriptRoot
$PgBin   = Join-Path $Base "postgres\pgsql\bin"
$DataDir = Join-Path $Base "datos"

Write-Host "Deteniendo ALXOR Core..." -ForegroundColor Yellow

# Parar el ERP
Get-Process -Name "AlxorCore.Api" -ErrorAction SilentlyContinue | Stop-Process -Force

# Parar PostgreSQL
if (Test-Path (Join-Path $PgBin "pg_ctl.exe")) {
    & (Join-Path $PgBin "pg_ctl.exe") -D $DataDir -m fast stop *> $null
}

Write-Host "Listo. ALXOR Core esta detenido." -ForegroundColor Green
