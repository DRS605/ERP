# CrearCuentaYEmpresa.ps1 — Crea la cuenta de acceso y la primera empresa en ALXOR Core.
# Ejecutar con el ERP ya arrancado (servicio o ArrancarTienda.bat), en http://localhost:8080.
$ErrorActionPreference = "Stop"
$BaseUrl = "http://localhost:8080"

# ---- Datos de acceso (puedes cambiarlos antes de ejecutar) ----
$Email  = "maria@laalacena.es"
$Nombre = "Maria Encarnacion Simon Alcazar"
$Pass   = "Alacena2026!"

# ---- Datos de la empresa ----
# NOTA: para una autonoma, el NIF de la empresa es su DNI, y la persona declarante
# queda identificada por ese DNI (el modelo tiene un unico nombre: razon social).
$Empresa = @{
    nif          = "75222983B"
    razonSocial  = "La Alacena de Maria Alcazar"
    calle        = "Cervantes 1"
    codigoPostal = "04280"
    poblacion    = "Los Gallardos"
    provincia    = "Almeria"
}

function Api($Method, $Path, $Body, $Token) {
    $h = @{}
    if ($Token) { $h["Authorization"] = "Bearer $Token" }
    $p = @{ Method = $Method; Uri = "$BaseUrl$Path"; Headers = $h }
    if ($Body) { $p.Body = ($Body | ConvertTo-Json -Depth 6); $p.ContentType = "application/json; charset=utf-8" }
    return Invoke-RestMethod @p
}

# Comprobar que el ERP responde.
try { Invoke-WebRequest "$BaseUrl/salud" -UseBasicParsing -TimeoutSec 4 | Out-Null }
catch { throw "El ERP no responde en $BaseUrl. Arrancalo primero (servicio o ArrancarTienda.bat)." }

Write-Host "Registrando la cuenta..." -ForegroundColor Cyan
try {
    Api POST "/auth/registro" @{ email = $Email; nombre = $Nombre; contrasena = $Pass } $null | Out-Null
    Write-Host "  Cuenta creada." -ForegroundColor Green
} catch {
    Write-Host "  La cuenta ya existia; se reutiliza." -ForegroundColor Yellow
}

$login = Api POST "/auth/login" @{ email = $Email; contrasena = $Pass } $null
$token = $login.token

$empresas = Api GET "/empresas" $null $token
if ($empresas -and @($empresas).Count -gt 0) {
    Write-Host "Ya habia una empresa; no se crea otra." -ForegroundColor Yellow
} else {
    Write-Host "Creando la empresa..." -ForegroundColor Cyan
    Api POST "/empresas" $Empresa $token | Out-Null
    Write-Host "  Empresa creada: $($Empresa.razonSocial)" -ForegroundColor Green
}

Write-Host ""
Write-Host "=====================================================" -ForegroundColor Green
Write-Host "  Listo. Entra en $BaseUrl con:" -ForegroundColor Green
Write-Host "     Usuario:    $Email"
Write-Host "     Contrasena: $Pass"
Write-Host "=====================================================" -ForegroundColor Green
