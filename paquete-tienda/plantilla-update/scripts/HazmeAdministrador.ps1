# HazmeAdministrador.ps1 — Te marca como ADMINISTRADOR de la plataforma en ESTA instalación,
# para que veas la sección "Instalaciones". Escribe tu correo en app\plataforma.json (que las
# actualizaciones NO sobrescriben) y reinicia el servicio. EJECUTAR COMO ADMINISTRADOR.
#Requires -RunAsAdministrator
$ErrorActionPreference = "Stop"

$cands = @(
    (Join-Path $env:USERPROFILE "AlxorCore-Tienda"),
    (Join-Path $env:USERPROFILE "Downloads\AlxorCore-Tienda"),
    (Join-Path ([Environment]::GetFolderPath('Desktop')) "AlxorCore-Tienda")
)
$Base = $cands | Where-Object { Test-Path (Join-Path $_ "app\AlxorCore.Api.exe") } | Select-Object -First 1
if (-not $Base) { throw "No encuentro la carpeta AlxorCore-Tienda." }
$App = Join-Path $Base "app"

$email = Read-Host "Tu correo (el mismo con el que entras en Core Evolution)"
if ([string]::IsNullOrWhiteSpace($email)) { throw "Correo vacío." }

$json = @{ Plataforma = @{ Administradores = @($email.Trim()) } } | ConvertTo-Json -Depth 4
Set-Content -Path (Join-Path $App "plataforma.json") -Value $json -Encoding UTF8
Write-Host "Guardado administrador: $email" -ForegroundColor Green

$svc = Get-Service "ALXOR-Core" -ErrorAction SilentlyContinue
if ($svc) { Restart-Service "ALXOR-Core"; Write-Host "Servicio reiniciado." -ForegroundColor Green }
else { Write-Host "No hay servicio; reinicia con ArrancarTienda.bat." -ForegroundColor Yellow }

Write-Host "Listo. Entra y recarga: veras la seccion 'Instalaciones' en el menu." -ForegroundColor Green
