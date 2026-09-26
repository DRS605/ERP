/* =====================================================================================================
   Extracción del paquete de migración Hispatec → ALXOR (formato «alxor-hispatec», versión 1).

   Se ejecuta con exportar.ps1, que sustituye {{EMPRESA}} (código de la empresa en Hispatec, tabla
   Empresas) y {{FECHA_CORTE}} (aaaa-mm-dd), lanza cada consulta y escribe su resultado en el CSV que
   indica la marca «-- @archivo». Solo lee: no modifica nada en Hispatec.

   Reglas del paquete (las comprueba ALXOR al validar):
   - Identificadores con prefijo por rol (C = cliente, P = proveedor, A = acreedor), porque en Hispatec
     cada rol tiene su propia tabla y sus Id se repiten.
   - Fechas como date (Hispatec guarda todo en datetime) y decimales con punto.
   - Los saldos se calculan con los APUNTES, no con la tabla Acumuladores (mantenida por triggers);
     los acumuladores se exportan aparte solo para compararlos.

   PUNTOS A CONFIRMAR con la base real (marcados «CONFIRMAR»): el significado de algunos enumerados
   (TipoApunte, TipoSaldo, EstadoDocumentoCobro/Pago, TipoArticulo) y las unidades de superficie. El script
   diagnostico.sql ayuda a comprobarlos antes de migrar.
   ===================================================================================================== */

-- @parametros
DECLARE @Empresa varchar(10) = '{{EMPRESA}}';
DECLARE @FechaCorte date = '{{FECHA_CORTE}}';
DECLARE @IdEmpresa int = (SELECT Id FROM dbo.Empresas WHERE Codigo = @Empresa);
DECLARE @IdGrupo int = (SELECT Id_GrupoEmpresarial FROM dbo.Empresas WHERE Id = @IdEmpresa);
DECLARE @InicioEjercicio date = (SELECT TOP 1 CAST(FechaInicio AS date) FROM dbo.EjerciciosContables
                                  WHERE Id_Empresa = @IdEmpresa AND @FechaCorte BETWEEN CAST(FechaInicio AS date) AND CAST(FechaFin AS date));

-- @archivo manifiesto.csv
SELECT v.clave, v.valor FROM (VALUES
    ('formato', 'alxor-hispatec'),
    ('version', '1'),
    ('grupo_codigo', (SELECT Codigo FROM dbo.GruposEmpresariales WHERE Id = @IdGrupo)),
    ('empresa_codigo', @Empresa),
    ('empresa_nombre', (SELECT s.Nombre FROM dbo.Empresas e JOIN dbo.Sujetos s ON s.Id = e.Id_Sujeto WHERE e.Id = @IdEmpresa)),
    ('empresa_nif', (SELECT s.Identificador FROM dbo.Empresas e JOIN dbo.Sujetos s ON s.Id = e.Id_Sujeto WHERE e.Id = @IdEmpresa)),
    ('fecha_corte', CONVERT(varchar(10), @FechaCorte, 23)),
    ('inicio_ejercicio', CONVERT(varchar(10), @InicioEjercicio, 23)),
    ('generado_en', CONVERT(varchar(19), SYSDATETIME(), 126))
) v (clave, valor);

-- @archivo terceros.csv
-- El sujeto único de Hispatec, solo los que son clientes, proveedores o acreedores de la empresa.
WITH sujetos AS (
    SELECT c.Id_Sujeto FROM dbo.Clientes c JOIN dbo.ClientesEmpresas ce ON ce.Id_Cliente = c.Id AND ce.Id_Empresa = @IdEmpresa
    UNION SELECT p.Id_Sujeto FROM dbo.Proveedores p JOIN dbo.ProveedoresEmpresa pe ON pe.Id_Proveedor = p.Id AND pe.Id_Empresa = @IdEmpresa
    UNION SELECT a.Id_Sujeto FROM dbo.Acreedores a JOIN dbo.AcreedoresEmpresa ae ON ae.Id_Acreedor = a.Id AND ae.Id_Empresa = @IdEmpresa
)
SELECT s.Id AS id_sujeto,
       NULLIF(LTRIM(RTRIM(s.Identificador)), '') AS nif,
       LTRIM(RTRIM(CONCAT(s.Nombre, ' ' + NULLIF(s.PrimerApellido, ''), ' ' + NULLIF(s.SegundoApellido, '')))) AS nombre,
       LTRIM(RTRIM(CONCAT(d.NombreVia, ' ' + NULLIF(d.Numero, ''), ' ' + NULLIF(d.Bloque, ''), ' ' + NULLIF(d.Escalera, ''),
                          ' ' + NULLIF(d.Piso, ''), ' ' + NULLIF(d.Puerta, '')))) AS calle,
       d.CodigoPostal AS codigo_postal, d.Poblacion AS poblacion, d.Provincia AS provincia,
       pa.Codigo AS pais,                                             -- CONFIRMAR: que Paises.Codigo sea el ISO de 2 letras
       em.CuentaCorreoElectronico AS email,
       cb.IBAN AS iban
FROM sujetos x
JOIN dbo.Sujetos s ON s.Id = x.Id_Sujeto
OUTER APPLY (SELECT TOP 1 dd.* FROM dbo.DireccionesSujeto ds JOIN dbo.Direcciones dd ON dd.Id = ds.Id_Direccion
             WHERE ds.Id_Sujeto = s.Id ORDER BY ds.UsoDefecto DESC, dd.Id) d
LEFT JOIN dbo.Paises pa ON pa.Id = d.Id_Pais
OUTER APPLY (SELECT TOP 1 e.CuentaCorreoElectronico FROM dbo.CorreosElectronicosSujeto e WHERE e.Id_Sujeto = s.Id ORDER BY e.Id) em
OUTER APPLY (SELECT TOP 1 c.IBAN FROM dbo.CuentasBancariasSujeto cs JOIN dbo.CuentasBancarias c ON c.Id = cs.Id_CuentaBancaria
             WHERE cs.Id_Sujeto = s.Id ORDER BY cs.UsoDefecto DESC, c.Id) cb;

-- @archivo clientes.csv
SELECT 'C' + CAST(c.Id AS varchar(12)) AS id, c.Codigo AS codigo, c.Id_Sujeto AS id_sujeto, cc.Codigo AS subcuenta
FROM dbo.Clientes c
JOIN dbo.ClientesEmpresas ce ON ce.Id_Cliente = c.Id AND ce.Id_Empresa = @IdEmpresa
LEFT JOIN dbo.CuentasContables cc ON cc.Id = ce.Id_CuentaContable
WHERE c.EsClientePotencial = 0;

-- @archivo proveedores.csv
-- Proveedores y acreedores (servicios, transportistas…) van juntos: en ALXOR ambos son proveedores.
SELECT 'P' + CAST(p.Id AS varchar(12)) AS id, p.Codigo AS codigo, p.Id_Sujeto AS id_sujeto,
       (SELECT TOP 1 cc.Codigo FROM dbo.CuentasContables cc
         WHERE cc.Id_Empresa = @IdEmpresa AND cc.Id_Sujeto = p.Id_Sujeto AND (cc.Codigo LIKE '40%' OR cc.Codigo LIKE '41%') ORDER BY cc.Codigo) AS subcuenta,
       CASE WHEN p.Id_ProveedorAgro IS NULL THEN 0 ELSE 1 END AS agricultor,
       CASE WHEN pa.AutorizaEmitirFacturas = 1 THEN 1 ELSE 0 END AS autoriza_autofactura,
       NULL AS retencion                                              -- CONFIRMAR: porcentaje de ClavesRetencion (ProveedoresEmpresa.Id_Clave)
FROM dbo.Proveedores p
JOIN dbo.ProveedoresEmpresa pe ON pe.Id_Proveedor = p.Id AND pe.Id_Empresa = @IdEmpresa
LEFT JOIN dbo.ProveedoresAgro pa ON pa.Id = p.Id_ProveedorAgro
UNION ALL
SELECT 'A' + CAST(a.Id AS varchar(12)), a.Codigo, a.Id_Sujeto, cc.Codigo, 0, 0, NULL
FROM dbo.Acreedores a
JOIN dbo.AcreedoresEmpresa ae ON ae.Id_Acreedor = a.Id AND ae.Id_Empresa = @IdEmpresa
LEFT JOIN dbo.CuentasContables cc ON cc.Id = ae.Id_CuentaContable;

-- @archivo familias.csv
SELECT f.Id AS id, f.Codigo AS codigo, f.Nombre AS nombre, f.Id_FamiliaSuperior AS id_superior
FROM dbo.FamiliasArticulo f WHERE f.Id_GrupoEmpresarial = @IdGrupo;

-- @archivo articulos.csv
SELECT a.Id AS id, a.Codigo AS codigo, a.Nombre AS nombre, a.Id_Familia AS id_familia,
       LOWER(COALESCE(NULLIF(u.Abreviatura, ''), u.Codigo, 'ud')) AS unidad,
       a.TipoImpositivo AS iva,                                       -- CONFIRMAR: que TipoImpositivo sea el porcentaje
       CASE WHEN a.AfectaInventario = 0 AND a.TipoVarios = 1 THEN 'servicio' ELSE 'bien' END AS tipo,  -- CONFIRMAR con TipoArticulo
       CASE WHEN a.AfectaInventario = 1 THEN 1 ELSE 0 END AS controla_stock,
       0 AS precio_venta, 0 AS precio_compra                          -- Los precios están en tarifas: se cargan aparte
FROM dbo.Articulos a
LEFT JOIN dbo.UnidadesMedida u ON u.Id = a.Id_UnidadMedidaAlmacen
WHERE a.Id_GrupoEmpresarial = @IdGrupo;

-- @archivo cuentas.csv
SELECT c.Codigo AS codigo, c.Nombre AS nombre FROM dbo.CuentasContables c WHERE c.Id_Empresa = @IdEmpresa;

-- @archivo saldos.csv
-- Saldo de cada cuenta desde el inicio del ejercicio hasta la fecha de corte, sumando los apuntes.
-- CONFIRMAR: TipoApunte 0 = debe y 1 = haber (diagnostico.sql comprueba que así cuadran los asientos).
WITH mov AS (
    SELECT cc.Codigo AS cuenta,
           SUM(CASE WHEN ap.TipoApunte = 0 THEN ap.ImporteFunc ELSE 0 END) - SUM(CASE WHEN ap.TipoApunte = 1 THEN ap.ImporteFunc ELSE 0 END) AS saldo
    FROM dbo.Apuntes ap
    JOIN dbo.Asientos asi ON asi.Id = ap.Id_Asiento
    JOIN dbo.CuentasContables cc ON cc.Id = ap.Id_CuentaContable
    WHERE asi.Id_Empresa = @IdEmpresa AND CAST(asi.Fecha AS date) BETWEEN @InicioEjercicio AND @FechaCorte
    GROUP BY cc.Codigo
)
SELECT cuenta, CASE WHEN saldo > 0 THEN saldo ELSE 0 END AS debe, CASE WHEN saldo < 0 THEN -saldo ELSE 0 END AS haber
FROM mov WHERE saldo <> 0;

-- @archivo acumuladores.csv
-- Solo para comparar: lo que dice la tabla Acumuladores en el mismo periodo. CONFIRMAR el valor de TipoSaldo.
WITH acu AS (
    SELECT cc.Codigo AS cuenta, SUM(a.TotalDebe) - SUM(a.TotalHaber) AS saldo
    FROM dbo.Acumuladores a
    JOIN dbo.Centros ce ON ce.Id = a.Id_Centro AND ce.Id_Empresa = @IdEmpresa
    JOIN dbo.CuentasContables cc ON cc.Id = a.Id_CuentaContable
    WHERE CAST(a.Fecha AS date) BETWEEN @InicioEjercicio AND @FechaCorte AND a.TipoSaldo = 0
    GROUP BY cc.Codigo
)
SELECT cuenta, CASE WHEN saldo > 0 THEN saldo ELSE 0 END AS debe, CASE WHEN saldo < 0 THEN -saldo ELSE 0 END AS haber
FROM acu WHERE saldo <> 0;

-- @archivo cartera.csv
-- Efectos vivos a la fecha de corte. DocumentosCobro no tiene claves foráneas en Hispatec: se exporta el
-- cliente tal cual para que ALXOR detecte los huérfanos. CONFIRMAR los estados que cuentan como vivos.
SELECT 'DC' + CAST(dc.Id AS varchar(12)) AS id, 'cobro' AS sentido, 'C' + CAST(dc.Id_Cliente AS varchar(12)) AS id_tercero,
       CONCAT(dc.SerieFactura, '-', CAST(dc.NumeroFactura AS varchar(12)), ' / ', dc.Serie, '-', CAST(dc.Numero AS varchar(12)), ' (plazo ', CAST(dc.NumeroPlazo AS varchar(3)), ')') AS documento,
       CONVERT(varchar(10), CAST(dc.FechaFactura AS date), 23) AS fecha_documento,
       CONVERT(varchar(10), CAST(COALESCE(dc.FechaVencimiento, dc.FechaEmision) AS date), 23) AS vencimiento,
       dc.ImporteACobrarFunc AS importe
FROM dbo.DocumentosCobro dc
JOIN dbo.Centros ce ON ce.Id = dc.Id_Centro AND ce.Id_Empresa = @IdEmpresa
WHERE dc.Liquidado = 0 AND CAST(dc.FechaEmision AS date) <= @FechaCorte
UNION ALL
SELECT 'DP' + CAST(dp.Id AS varchar(12)), 'pago',
       CASE WHEN dp.Id_Proveedor IS NOT NULL THEN 'P' + CAST(dp.Id_Proveedor AS varchar(12)) ELSE 'A' + CAST(dp.Id_Acreedor AS varchar(12)) END,
       CONCAT(dp.Factura, ' / ', dp.Serie, '-', CAST(dp.Numero AS varchar(12)), ' (plazo ', CAST(dp.NumeroPlazo AS varchar(4)), ')'),
       CONVERT(varchar(10), CAST(dp.FechaFactura AS date), 23),
       CONVERT(varchar(10), CAST(COALESCE(dp.FechaVencimiento, dp.FechaEmision) AS date), 23),
       dp.ImporteAPagarFunc
FROM dbo.DocumentosPago dp
JOIN dbo.Centros ce ON ce.Id = dp.Id_Centro AND ce.Id_Empresa = @IdEmpresa
WHERE dp.Liquidado = 0 AND CAST(dp.FechaEmision AS date) <= @FechaCorte;

-- @archivo campanas.csv
SELECT c.Id AS id, c.Codigo AS codigo, c.Nombre AS nombre,
       CONVERT(varchar(10), CAST(c.FechaInicial AS date), 23) AS desde, CONVERT(varchar(10), CAST(c.FechaFinal AS date), 23) AS hasta
FROM dbo.Campanyas c WHERE c.Id_GrupoEmpresarial = @IdGrupo;

-- @archivo parcelas.csv
-- En ALXOR la parcela es del agricultor: cada cultivo vivo de Hispatec (finca + cultivo) pasa a ser una parcela
-- del proveedor agrícola que representa la finca. CONFIRMAR: que Fincas.Id_Representante sea el agricultor,
-- que FincaSubrecinto.Id_Subrecinto apunte a CatastroParcelas (no tiene FK declarada) y que las superficies estén en ha.
SELECT c.Id AS id,
       LEFT(CONCAT(f.Codigo, '-', CAST(c.Id AS varchar(12))), 30) AS codigo,
       LEFT(CONCAT(f.Nombre, ' · ', c.Descripcion), 150) AS nombre,
       (SELECT TOP 1 'P' + CAST(p.Id AS varchar(12)) FROM dbo.Proveedores p
         WHERE p.Id_Sujeto = f.Id_Representante AND p.Id_GrupoEmpresarial = @IdGrupo AND p.Id_ProveedorAgro IS NOT NULL ORDER BY p.Id) AS id_proveedor,
       (SELECT TOP 1 CONCAT(CAST(CAST(r.Provincia AS int) AS varchar(5)), ':', CAST(CAST(r.Municipio AS int) AS varchar(5)), ':', CAST(r.Agregado AS varchar(3)), ':',
                            CAST(r.Zona AS varchar(2)), ':', CAST(CAST(r.Poligono AS int) AS varchar(5)), ':', CAST(CAST(r.Parcela AS bigint) AS varchar(19)), ':',
                            CAST(r.Recinto AS varchar(15)))
          FROM dbo.CultivoSuperficiesCultivadas cs
          JOIN dbo.FincaSubrecinto fs ON fs.Id = cs.Id_FincaSubrecinto
          JOIN dbo.CatastroParcelas cp ON cp.Id = fs.Id_Subrecinto
          JOIN dbo.ReferenciaSIGPAC r ON r.Id = cp.Id_ReferenciaSIGPAC
         WHERE cs.Id_Cultivo = c.Id ORDER BY cs.SuperficieCultivada DESC) AS sigpac,
       c.SuperficieCultivada AS superficie_ha,
       c.Id_Articulo AS id_articulo,
       v.Denominacion AS variedad
FROM dbo.Cultivos c
JOIN dbo.Fincas f ON f.Id = c.Id_Finca AND f.Id_GrupoEmpresarial = @IdGrupo
LEFT JOIN dbo.EspecieBotanicaVariedad v ON v.Id = c.Id_Variedad
WHERE c.FechaFinCultivo IS NULL OR CAST(c.FechaFinCultivo AS date) >= @FechaCorte;
