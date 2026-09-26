/* =====================================================================================================
   Diagnóstico previo a la migración Hispatec → ALXOR. Solo lee.
   Sustituir {{EMPRESA}} y {{FECHA_CORTE}} (lo hace exportar.ps1 con -Diagnostico).

   Sirve para dos cosas:
   1. Confirmar las interpretaciones marcadas «CONFIRMAR» en extraer_paquete.sql.
   2. Medir los problemas de integridad que Hispatec no impide (ver docs/hispatec/MAPA.md) y que ALXOR
      rechazará o avisará al validar el paquete.
   ===================================================================================================== */
DECLARE @Empresa varchar(10) = '{{EMPRESA}}';
DECLARE @FechaCorte date = '{{FECHA_CORTE}}';
DECLARE @IdEmpresa int = (SELECT Id FROM dbo.Empresas WHERE Codigo = @Empresa);
DECLARE @IdGrupo int = (SELECT Id_GrupoEmpresarial FROM dbo.Empresas WHERE Id = @IdEmpresa);
DECLARE @InicioEjercicio date = (SELECT TOP 1 CAST(FechaInicio AS date) FROM dbo.EjerciciosContables
                                  WHERE Id_Empresa = @IdEmpresa AND @FechaCorte BETWEEN CAST(FechaInicio AS date) AND CAST(FechaFin AS date));

-- 1. TipoApunte: ¿0 = debe y 1 = haber? Con esa lectura, los asientos deben cuadrar (0 descuadrados).
SELECT 'D1 valores de TipoApunte' AS comprobacion, ap.TipoApunte, COUNT(*) AS apuntes, SUM(ap.ImporteFunc) AS importe
FROM dbo.Apuntes ap JOIN dbo.Asientos a ON a.Id = ap.Id_Asiento WHERE a.Id_Empresa = @IdEmpresa GROUP BY ap.TipoApunte;

SELECT 'D2 asientos descuadrados (leyendo 0 = debe, 1 = haber)' AS comprobacion, COUNT(*) AS asientos
FROM (SELECT ap.Id_Asiento FROM dbo.Apuntes ap JOIN dbo.Asientos a ON a.Id = ap.Id_Asiento
      WHERE a.Id_Empresa = @IdEmpresa AND CAST(a.Fecha AS date) BETWEEN @InicioEjercicio AND @FechaCorte
      GROUP BY ap.Id_Asiento
      HAVING SUM(CASE WHEN ap.TipoApunte = 0 THEN ap.ImporteFunc ELSE 0 END) <> SUM(CASE WHEN ap.TipoApunte = 1 THEN ap.ImporteFunc ELSE 0 END)) x;

-- 2. Acumuladores (mantenidos por triggers) frente a los apuntes: cuentas con diferencia.
SELECT 'D3 valores de TipoSaldo en Acumuladores' AS comprobacion, TipoSaldo, COUNT(*) AS filas FROM dbo.Acumuladores GROUP BY TipoSaldo;

WITH apu AS (
    SELECT ap.Id_CuentaContable, SUM(CASE WHEN ap.TipoApunte = 0 THEN ap.ImporteFunc ELSE -ap.ImporteFunc END) AS saldo
    FROM dbo.Apuntes ap JOIN dbo.Asientos a ON a.Id = ap.Id_Asiento
    WHERE a.Id_Empresa = @IdEmpresa AND CAST(a.Fecha AS date) BETWEEN @InicioEjercicio AND @FechaCorte GROUP BY ap.Id_CuentaContable),
acu AS (
    SELECT ac.Id_CuentaContable, SUM(ac.TotalDebe - ac.TotalHaber) AS saldo
    FROM dbo.Acumuladores ac JOIN dbo.Centros ce ON ce.Id = ac.Id_Centro AND ce.Id_Empresa = @IdEmpresa
    WHERE CAST(ac.Fecha AS date) BETWEEN @InicioEjercicio AND @FechaCorte AND ac.TipoSaldo = 0 GROUP BY ac.Id_CuentaContable)
SELECT TOP 100 'D4 cuentas con acumulado distinto de los apuntes' AS comprobacion, cc.Codigo, cc.Nombre,
       ISNULL(apu.saldo, 0) AS saldo_apuntes, ISNULL(acu.saldo, 0) AS saldo_acumuladores, ISNULL(apu.saldo, 0) - ISNULL(acu.saldo, 0) AS diferencia
FROM apu FULL JOIN acu ON acu.Id_CuentaContable = apu.Id_CuentaContable
JOIN dbo.CuentasContables cc ON cc.Id = COALESCE(apu.Id_CuentaContable, acu.Id_CuentaContable)
WHERE ISNULL(apu.saldo, 0) <> ISNULL(acu.saldo, 0)
ORDER BY ABS(ISNULL(apu.saldo, 0) - ISNULL(acu.saldo, 0)) DESC;

-- 3. Cartera: estados y huérfanos (DocumentosCobro no tiene ninguna clave foránea).
SELECT 'D5 estados de DocumentosCobro' AS comprobacion, EstadoDocumentoCobro, Liquidado, COUNT(*) AS efectos, SUM(ImporteACobrarFunc) AS importe
FROM dbo.DocumentosCobro dc JOIN dbo.Centros ce ON ce.Id = dc.Id_Centro AND ce.Id_Empresa = @IdEmpresa GROUP BY EstadoDocumentoCobro, Liquidado;
SELECT 'D6 estados de DocumentosPago' AS comprobacion, EstadoDocumentoPago, Liquidado, COUNT(*) AS efectos, SUM(ImporteAPagarFunc) AS importe
FROM dbo.DocumentosPago dp JOIN dbo.Centros ce ON ce.Id = dp.Id_Centro AND ce.Id_Empresa = @IdEmpresa GROUP BY EstadoDocumentoPago, Liquidado;
SELECT 'D7 efectos de cobro con cliente inexistente' AS comprobacion, COUNT(*) AS efectos, SUM(dc.ImporteACobrarFunc) AS importe
FROM dbo.DocumentosCobro dc WHERE NOT EXISTS (SELECT 1 FROM dbo.Clientes c WHERE c.Id = dc.Id_Cliente);
SELECT 'D8 efectos de cobro con centro inexistente' AS comprobacion, COUNT(*) AS efectos
FROM dbo.DocumentosCobro dc WHERE NOT EXISTS (SELECT 1 FROM dbo.Centros c WHERE c.Id = dc.Id_Centro);

-- 4. Terceros: NIF repetidos en varios sujetos.
SELECT TOP 100 'D9 NIF en varios sujetos' AS comprobacion, s.Identificador, COUNT(*) AS sujetos
FROM dbo.Sujetos s WHERE LTRIM(RTRIM(s.Identificador)) <> '' GROUP BY s.Identificador HAVING COUNT(*) > 1 ORDER BY COUNT(*) DESC;

-- 5. Agro: ¿el representante de la finca es un proveedor agrícola? ¿Id_Subrecinto apunta a CatastroParcelas?
SELECT 'D10 fincas cuyo representante es proveedor agrícola' AS comprobacion,
       SUM(CASE WHEN EXISTS (SELECT 1 FROM dbo.Proveedores p WHERE p.Id_Sujeto = f.Id_Representante AND p.Id_ProveedorAgro IS NOT NULL) THEN 1 ELSE 0 END) AS si,
       COUNT(*) AS fincas
FROM dbo.Fincas f WHERE f.Id_GrupoEmpresarial = @IdGrupo;
SELECT 'D11 subrecintos que casan con CatastroParcelas' AS comprobacion,
       SUM(CASE WHEN EXISTS (SELECT 1 FROM dbo.CatastroParcelas c WHERE c.Id = fs.Id_Subrecinto) THEN 1 ELSE 0 END) AS casan, COUNT(*) AS subrecintos
FROM dbo.FincaSubrecinto fs;
SELECT 'D12 superficies (¿ha o m²?)' AS comprobacion, MIN(SuperficieCultivada) AS minima, AVG(SuperficieCultivada) AS media, MAX(SuperficieCultivada) AS maxima FROM dbo.Cultivos;

-- 6. Artículos: tipos de IVA y tipos de artículo.
SELECT 'D13 TipoImpositivo de los artículos' AS comprobacion, TipoImpositivo, COUNT(*) AS articulos FROM dbo.Articulos WHERE Id_GrupoEmpresarial = @IdGrupo GROUP BY TipoImpositivo;
SELECT 'D14 TipoArticulo / AfectaInventario / TipoVarios' AS comprobacion, TipoArticulo, AfectaInventario, TipoVarios, COUNT(*) AS articulos
FROM dbo.Articulos WHERE Id_GrupoEmpresarial = @IdGrupo GROUP BY TipoArticulo, AfectaInventario, TipoVarios;
