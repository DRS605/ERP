# CrearAccesoKiosko.ps1 — Crea en el Escritorio un acceso directo que abre Core Evolution
# en una ventana de aplicación con IMPRESIÓN SILENCIOSA (sin diálogo): al pulsar
# "Imprimir ticket" sale directo a la impresora predeterminada, sin ningún clic más.
$ErrorActionPreference = "Stop"

$Url = "http://localhost:8080"

# Buscar Chrome; si no está, usar Edge (siempre presente en Windows 10/11).
$chrome = @(
    "$env:ProgramFiles\Google\Chrome\Application\chrome.exe",
    "${env:ProgramFiles(x86)}\Google\Chrome\Application\chrome.exe",
    "$env:LOCALAPPDATA\Google\Chrome\Application\chrome.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1

$edge = @(
    "${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe",
    "$env:ProgramFiles\Microsoft\Edge\Application\msedge.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1

$browser = if ($chrome) { $chrome } elseif ($edge) { $edge } else { $null }
if (-not $browser) { throw "No encuentro Chrome ni Edge. Instala Google Chrome (recomendado) y repite." }

# Perfil propio aislado para el modo tienda (no molesta a la navegación normal).
$perfil = Join-Path $env:LOCALAPPDATA "CoreEvolutionKiosko"
New-Item -ItemType Directory -Force -Path $perfil | Out-Null

$args = "--app=$Url --kiosk-printing --no-first-run --no-default-browser-check --disable-features=Translate --user-data-dir=`"$perfil`""

$desktop = [Environment]::GetFolderPath('Desktop')
$lnkPath = Join-Path $desktop "Core Evolution (Tienda).lnk"
$ws = New-Object -ComObject WScript.Shell
$lnk = $ws.CreateShortcut($lnkPath)
$lnk.TargetPath = $browser
$lnk.Arguments  = $args
$lnk.WorkingDirectory = Split-Path $browser
$lnk.IconLocation = "$browser,0"
$lnk.Description = "Core Evolution - Punto de venta (impresion directa)"
$lnk.Save()

Write-Host ""
Write-Host "Acceso directo creado en el Escritorio: 'Core Evolution (Tienda)'" -ForegroundColor Green
Write-Host "Navegador usado: $browser" -ForegroundColor DarkGray
Write-Host ""
Write-Host "IMPORTANTE para la impresion silenciosa:" -ForegroundColor Yellow
Write-Host " - Pon tu impresora de tickets como PREDETERMINADA en Windows" -ForegroundColor Yellow
Write-Host "   (Configuracion > Bluetooth y dispositivos > Impresoras y escaneres," -ForegroundColor Yellow
Write-Host "    y desactiva 'Permitir que Windows administre mi impresora predeterminada')." -ForegroundColor Yellow
Write-Host " - A partir de ahora abre la tienda SIEMPRE con ese acceso directo." -ForegroundColor Yellow
Write-Host "   Al pulsar 'Imprimir ticket' saldra directo, sin ningun clic mas." -ForegroundColor Green
