<#
.SYNOPSIS
  Genera el paquete de migración Hispatec → ALXOR (ZIP de CSV) o ejecuta el diagnóstico previo.

.EXAMPLE
  .\exportar.ps1 -Servidor SQL01 -BaseDatos ERPHispatec -Empresa 01 -FechaCorte 2026-12-31
  .\exportar.ps1 -Servidor SQL01 -BaseDatos ERPHispatec -Empresa 01 -FechaCorte 2026-12-31 -Diagnostico

.NOTES
  Solo lee de Hispatec. Usa la autenticación de Windows salvo que se indique -Usuario.
  El ZIP resultante se sube en ALXOR en Ajustes → Migración desde Hispatec, primero para validar.
#>
param(
    [Parameter(Mandatory)] [string] $Servidor,
    [Parameter(Mandatory)] [string] $BaseDatos,
    [Parameter(Mandatory)] [string] $Empresa,
    [Parameter(Mandatory)] [string] $FechaCorte,
    [string] $Usuario,
    [string] $Contrasena,
    [string] $Salida = ".\paquete-hispatec-$Empresa-$FechaCorte.zip",
    [switch] $Diagnostico
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem

$cadena = "Server=$Servidor;Database=$BaseDatos;TrustServerCertificate=True;Application Name=alxor-migracion;" +
    ($(if ($Usuario) { "User Id=$Usuario;Password=$Contrasena;" } else { "Integrated Security=True;" }))
$conexion = New-Object System.Data.SqlClient.SqlConnection $cadena
$conexion.Open()

function Sustituir([string] $sql) { $sql.Replace('{{EMPRESA}}', $Empresa.Replace("'", "''")).Replace('{{FECHA_CORTE}}', $FechaCorte) }

function Ejecutar([string] $sql) {
    $cmd = $conexion.CreateCommand(); $cmd.CommandText = $sql; $cmd.CommandTimeout = 0
    $tabla = New-Object System.Data.DataSet
    (New-Object System.Data.SqlClient.SqlDataAdapter $cmd).Fill($tabla) | Out-Null
    return $tabla.Tables
}

function Csv($valor) {
    if ($valor -is [System.DBNull] -or $null -eq $valor) { return '' }
    if ($valor -is [decimal] -or $valor -is [double]) { return ([decimal]$valor).ToString([Globalization.CultureInfo]::InvariantCulture) }
    if ($valor -is [datetime]) { return $valor.ToString('yyyy-MM-dd') }
    if ($valor -is [bool]) { return $(if ($valor) { '1' } else { '0' }) }
    $t = [string]$valor
    if ($t -match '[;"\r\n]') { return '"' + $t.Replace('"', '""') + '"' }
    return $t
}

$dir = Split-Path -Parent $MyInvocation.MyCommand.Path
if ($Diagnostico) {
    foreach ($t in Ejecutar (Sustituir (Get-Content "$dir\diagnostico.sql" -Raw -Encoding UTF8))) { $t | Format-Table -AutoSize | Out-String -Width 250 }
    $conexion.Close(); return
}

$script = Get-Content "$dir\extraer_paquete.sql" -Raw -Encoding UTF8
$bloques = [regex]::Split($script, '(?m)^-- @')
$parametros = ($bloques | Where-Object { $_.StartsWith('parametros') }) -replace '^parametros', ''
$temporal = Join-Path ([IO.Path]::GetTempPath()) ("alxor-" + [guid]::NewGuid())
New-Item -ItemType Directory $temporal | Out-Null
foreach ($b in $bloques | Where-Object { $_.StartsWith('archivo ') }) {
    $nombre = ($b -split "`n")[0].Substring(8).Trim()
    $consulta = Sustituir ($parametros + "`n" + ($b -replace '^archivo [^\n]*\n', ''))
    Write-Host "Exportando $nombre…"
    $datos = (Ejecutar $consulta)[0]
    $lineas = New-Object System.Collections.Generic.List[string]
    $lineas.Add((($datos.Columns | ForEach-Object { $_.ColumnName }) -join ';'))
    foreach ($fila in $datos.Rows) { $lineas.Add((($fila.ItemArray | ForEach-Object { Csv $_ }) -join ';')) }
    [IO.File]::WriteAllLines((Join-Path $temporal $nombre), $lineas, (New-Object System.Text.UTF8Encoding $false))
    Write-Host "  $($datos.Rows.Count) filas"
}
$conexion.Close()
$destino = [IO.Path]::GetFullPath((Join-Path (Get-Location) $Salida))
if (Test-Path $destino) { Remove-Item $destino }
[IO.Compression.ZipFile]::CreateFromDirectory($temporal, $destino)
Remove-Item -Recurse $temporal
Write-Host "Paquete generado: $destino"
