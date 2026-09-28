using System.Globalization;
using System.Xml.Linq;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Terceros.Aplicacion;
using AlxorCore.Tesoreria.Dominio;

namespace AlxorCore.Tesoreria.Aplicacion;

/// <summary>Resultado de generar una remesa de adeudos SEPA (Norma 19). Con la remesa ya registrada: su id y código.</summary>
public sealed record RemesaSepaDto(string FicheroXml, string NombreArchivo, int NumeroAdeudos, decimal Total, IReadOnlyList<string> Omitidas,
    Guid? RemesaId = null, string? Codigo = null);

/// <summary>
/// Datos para generar una remesa de adeudos: facturas a domiciliar, fecha de cobro y esquema.
/// <paramref name="Esquema"/> = CORE (particulares) o B2B (empresas). <paramref name="Secuencia"/> =
/// OOFF (único), FRST (primero), RCUR (recurrente) o FNAL (último).
/// </summary>
public sealed record GenerarRemesaComando(IReadOnlyList<Guid> FacturaIds, DateOnly? FechaCobro = null, string? Esquema = null, string? Secuencia = null,
    Guid? CuentaBancariaId = null);

/// <summary>
/// Datos para crear una remesa registrada. Cobro: facturas (y efectos de cartera a cobrar) a domiciliar; Pago: gastos a
/// pagar por transferencia. Sin <paramref name="CuentaBancariaId"/> se usa el banco predeterminado (o el IBAN de la ficha
/// de la empresa si no hay cuentas bancarias).
/// </summary>
public sealed record CrearRemesaComando(
    TipoRemesa Tipo, IReadOnlyList<Guid>? FacturaIds = null, IReadOnlyList<Guid>? EfectoIds = null, IReadOnlyList<Guid>? GastoIds = null,
    DateOnly? FechaCargo = null, string? Esquema = null, string? Secuencia = null, Guid? CuentaBancariaId = null,
    ModalidadRemesa Modalidad = ModalidadRemesa.Vencimiento, CondicionesRemesa? Condiciones = null);

/// <summary>Modalidad y condiciones del banco de una remesa de cobro viva.</summary>
public sealed record CondicionesRemesaComando(ModalidadRemesa Modalidad, CondicionesRemesa? Condiciones = null);

public sealed record LineaRemesaDto(
    Guid Id, string TipoDocumento, Guid DocumentoId, string Documento, string TerceroNombre, string? Iban, string? Mandato, decimal Importe,
    Guid? MovimientoId, bool Devuelta, string? MotivoDevolucion);

public sealed record RemesaDto(
    Guid Id, string Tipo, string Codigo, DateOnly Fecha, DateOnly FechaCargo, Guid? CuentaBancariaId, string? CuentaBancaria, string? Esquema, string? Secuencia,
    string Estado, string EstadoTexto, decimal Total, int NumeroLineas, string NombreArchivo, DateOnly? FechaLiquidacion, IReadOnlyList<LineaRemesaDto> Lineas,
    string Modalidad = "Vencimiento", CondicionesRemesa? Condiciones = null, decimal Intereses = 0m, decimal Comision = 0m, decimal IvaComision = 0m,
    decimal Gastos = 0m, decimal? Liquido = null, DateOnly? RiesgoCanceladoEn = null);

/// <summary>Remesa creada, con los documentos que no se pudieron incluir y por qué.</summary>
public sealed record RemesaCreadaDto(RemesaDto Remesa, IReadOnlyList<string> Omitidos);

/// <summary>Fichero de una remesa registrada (para volver a descargarlo).</summary>
public sealed record FicheroRemesaDto(string Fichero, string NombreArchivo);

/// <summary>Fecha de liquidación de una remesa (por defecto, la de cargo pedida al banco).</summary>
public sealed record LiquidarRemesaComando(DateOnly? Fecha = null);

public interface IRepositorioRemesas
{
    void Agregar(Remesa remesa);

    Task<Remesa?> ObtenerAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Remesa>> ListarAsync(TipoRemesa? tipo, CancellationToken ct = default);

    Task<int> SiguienteNumeroAsync(TipoRemesa tipo, int ejercicio, CancellationToken ct = default);

    /// <summary>Documentos que ya están en una remesa viva (generada o presentada): documento → código de la remesa.</summary>
    Task<IReadOnlyDictionary<Guid, string>> DocumentosVivosAsync(TipoDocumentoTesoreria tipo, IReadOnlyCollection<Guid> documentoIds, CancellationToken ct = default);

    /// <summary>Remesa en la que se registró un movimiento al liquidarla (null si no viene de una).</summary>
    Task<Remesa?> DeMovimientoAsync(Guid movimientoId, CancellationToken ct = default);
}

/// <summary>Construcción de los ficheros XML SEPA (ISO 20022) de adeudos y transferencias.</summary>
public static class XmlSepa
{
    private static readonly XNamespace NsAdeudos = "urn:iso:std:iso:20022:tech:xsd:pain.008.001.02";
    private static readonly XNamespace NsTransferencias = "urn:iso:std:iso:20022:tech:xsd:pain.001.001.03";

    /// <summary>Un adeudo (línea de la remesa de cobros).</summary>
    public sealed record Adeudo(string Referencia, decimal Importe, string Deudor, string Iban, string Mandato, DateOnly MandatoFecha, string Concepto);

    public static string NuevoMensajeId(IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        return $"ALX{reloj.AhoraUtc:yyyyMMddHHmmss}{Guid.NewGuid():N}"[..35];
    }

    /// <summary>pain.008.001.02 (adeudos directos SEPA, equivalente a la Norma 19.14).</summary>
    public static string Adeudos(string mensajeId, DateTimeOffset ahora, string acreedorNombre, string acreedorIban, string? acreedorBic, string acreedorId,
        DateOnly fechaCobro, IReadOnlyList<Adeudo> adeudos, string esquema, string secuencia)
    {
        ArgumentNullException.ThrowIfNull(adeudos);
        var ns = NsAdeudos;
        var total = Redondeo.Dos(adeudos.Sum(a => a.Importe)).ToString("F2", CultureInfo.InvariantCulture);
        var numero = adeudos.Count.ToString(CultureInfo.InvariantCulture);

        var transacciones = adeudos.Select(a => new XElement(
            ns + "DrctDbtTxInf",
            new XElement(ns + "PmtId", new XElement(ns + "EndToEndId", Limitar(a.Referencia, 35))),
            new XElement(ns + "InstdAmt", new XAttribute("Ccy", "EUR"), a.Importe.ToString("F2", CultureInfo.InvariantCulture)),
            new XElement(ns + "DrctDbtTx", new XElement(ns + "MndtRltdInf",
                new XElement(ns + "MndtId", Limitar(a.Mandato, 35)),
                new XElement(ns + "DtOfSgntr", a.MandatoFecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)))),
            Agente(ns, "DbtrAgt", null),
            new XElement(ns + "Dbtr", new XElement(ns + "Nm", Limitar(a.Deudor, 70))),
            new XElement(ns + "DbtrAcct", new XElement(ns + "Id", new XElement(ns + "IBAN", a.Iban))),
            new XElement(ns + "RmtInf", new XElement(ns + "Ustrd", Limitar(a.Concepto, 140)))));

        var documento = new XDocument(new XDeclaration("1.0", "UTF-8", null), new XElement(ns + "Document", new XElement(ns + "CstmrDrctDbtInitn",
            new XElement(ns + "GrpHdr",
                new XElement(ns + "MsgId", mensajeId),
                new XElement(ns + "CreDtTm", ahora.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)),
                new XElement(ns + "NbOfTxs", numero),
                new XElement(ns + "CtrlSum", total),
                new XElement(ns + "InitgPty", new XElement(ns + "Nm", Limitar(acreedorNombre, 70)))),
            new XElement(ns + "PmtInf",
                new XElement(ns + "PmtInfId", mensajeId),
                new XElement(ns + "PmtMtd", "DD"),
                new XElement(ns + "NbOfTxs", numero),
                new XElement(ns + "CtrlSum", total),
                new XElement(ns + "PmtTpInf",
                    new XElement(ns + "SvcLvl", new XElement(ns + "Cd", "SEPA")),
                    new XElement(ns + "LclInstrm", new XElement(ns + "Cd", esquema)),
                    new XElement(ns + "SeqTp", secuencia)),
                new XElement(ns + "ReqdColltnDt", fechaCobro.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                new XElement(ns + "Cdtr", new XElement(ns + "Nm", Limitar(acreedorNombre, 70))),
                new XElement(ns + "CdtrAcct", new XElement(ns + "Id", new XElement(ns + "IBAN", acreedorIban))),
                Agente(ns, "CdtrAgt", acreedorBic),
                new XElement(ns + "CdtrSchmeId", new XElement(ns + "Id", new XElement(ns + "PrvtId", new XElement(ns + "Othr",
                    new XElement(ns + "Id", acreedorId),
                    new XElement(ns + "SchmeNm", new XElement(ns + "Prtry", "SEPA")))))),
                transacciones))));

        return documento.Declaration + Environment.NewLine + documento;
    }

    /// <summary>pain.001.001.03 (transferencias SEPA, equivalente al Cuaderno 34.14).</summary>
    public static string Transferencias(string mensajeId, DateTimeOffset ahora, string ordenanteNombre, string ordenanteIban, string? ordenanteBic, DateOnly fechaPago,
        IReadOnlyList<PagoProveedor> pagos)
    {
        ArgumentNullException.ThrowIfNull(pagos);
        var ns = NsTransferencias;
        var total = Redondeo.Dos(pagos.Sum(p => p.Importe)).ToString("F2", CultureInfo.InvariantCulture);
        var numero = pagos.Count.ToString(CultureInfo.InvariantCulture);

        var transacciones = pagos.Select(p => new XElement(
            ns + "CdtTrfTxInf",
            new XElement(ns + "PmtId", new XElement(ns + "EndToEndId", Limitar(p.Referencia, 35))),
            new XElement(ns + "Amt", new XElement(ns + "InstdAmt", new XAttribute("Ccy", "EUR"), p.Importe.ToString("F2", CultureInfo.InvariantCulture))),
            Agente(ns, "CdtrAgt", null),
            new XElement(ns + "Cdtr", new XElement(ns + "Nm", Limitar(p.ProveedorNombre, 70))),
            new XElement(ns + "CdtrAcct", new XElement(ns + "Id", new XElement(ns + "IBAN", p.Iban!))),
            new XElement(ns + "RmtInf", new XElement(ns + "Ustrd", Limitar(p.Concepto, 140)))));

        var documento = new XDocument(new XDeclaration("1.0", "UTF-8", null), new XElement(ns + "Document", new XElement(ns + "CstmrCdtTrfInitn",
            new XElement(ns + "GrpHdr",
                new XElement(ns + "MsgId", mensajeId),
                new XElement(ns + "CreDtTm", ahora.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)),
                new XElement(ns + "NbOfTxs", numero),
                new XElement(ns + "CtrlSum", total),
                new XElement(ns + "InitgPty", new XElement(ns + "Nm", Limitar(ordenanteNombre, 70)))),
            new XElement(ns + "PmtInf",
                new XElement(ns + "PmtInfId", mensajeId),
                new XElement(ns + "PmtMtd", "TRF"),
                new XElement(ns + "NbOfTxs", numero),
                new XElement(ns + "CtrlSum", total),
                new XElement(ns + "PmtTpInf", new XElement(ns + "SvcLvl", new XElement(ns + "Cd", "SEPA"))),
                new XElement(ns + "ReqdExctnDt", fechaPago.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                new XElement(ns + "Dbtr", new XElement(ns + "Nm", Limitar(ordenanteNombre, 70))),
                new XElement(ns + "DbtrAcct", new XElement(ns + "Id", new XElement(ns + "IBAN", ordenanteIban))),
                Agente(ns, "DbtrAgt", ordenanteBic),
                transacciones))));

        return documento.Declaration + Environment.NewLine + documento;
    }

    private static XElement Agente(XNamespace ns, string nombre, string? bic) =>
        new(ns + nombre, new XElement(ns + "FinInstnId", string.IsNullOrWhiteSpace(bic)
            ? new XElement(ns + "Othr", new XElement(ns + "Id", "NOTPROVIDED"))
            : new XElement(ns + "BIC", bic)));

    private static string Limitar(string valor, int max) => valor.Length <= max ? valor : valor[..max];
}

/// <summary>
/// Remesas SEPA registradas: se generan (con su fichero), se presentan al banco, se liquidan (cobradas o pagadas:
/// un movimiento por línea contra la cuenta bancaria de la remesa, con su asiento) o se anulan. Un documento que está en
/// una remesa viva no entra en otra. El fichero se puede volver a descargar.
/// </summary>
public sealed class GestionRemesas
{
    private readonly IRepositorioRemesas _remesas;
    private readonly IRepositorioCuentasBancarias _bancos;
    private readonly IRepositorioMovimientos _movimientos;
    private readonly IRepositorioCartera _cartera;
    private readonly IConsultaFacturas _facturas;
    private readonly IConsultaGastos _gastos;
    private readonly IConsultaClientes _clientes;
    private readonly IConsultaProveedores _proveedores;
    private readonly IConsultaEmpresas _empresas;
    private readonly IUnidadDeTrabajoTesoreria _unidad;
    private readonly IReloj _reloj;
    private readonly ContabilizacionTesoreria? _contabilizacion;
    private readonly IRepositorioDevoluciones? _devoluciones;

    public GestionRemesas(IRepositorioRemesas remesas, IRepositorioCuentasBancarias bancos, IRepositorioMovimientos movimientos, IRepositorioCartera cartera,
        IConsultaFacturas facturas, IConsultaGastos gastos, IConsultaClientes clientes, IConsultaProveedores proveedores, IConsultaEmpresas empresas,
        IUnidadDeTrabajoTesoreria unidad, IReloj reloj, ContabilizacionTesoreria? contabilizacion = null, IRepositorioDevoluciones? devoluciones = null)
    {
        _remesas = remesas;
        _bancos = bancos;
        _movimientos = movimientos;
        _cartera = cartera;
        _facturas = facturas;
        _gastos = gastos;
        _clientes = clientes;
        _proveedores = proveedores;
        _empresas = empresas;
        _unidad = unidad;
        _reloj = reloj;
        _contabilizacion = contabilizacion;
        _devoluciones = devoluciones;
    }

    private DateOnly Hoy => DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);

    public async Task<IReadOnlyList<RemesaDto>> ListarAsync(TipoRemesa? tipo, CancellationToken ct = default)
    {
        var lista = await _remesas.ListarAsync(tipo, ct).ConfigureAwait(false);
        var bancos = (await _bancos.ListarAsync(ct).ConfigureAwait(false)).ToDictionary(b => b.Id, b => b.Nombre);
        return lista.Select(r => Dto(r, bancos, null, conLineas: false)).ToList();
    }

    public async Task<Resultado<RemesaDto>> ObtenerAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _remesas.ObtenerAsync(id, ct).ConfigureAwait(false);
        return r is null ? NoEncontrada<RemesaDto>() : Resultado.Ok(await DtoAsync(r, ct).ConfigureAwait(false));
    }

    public async Task<Resultado<FicheroRemesaDto>> FicheroAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _remesas.ObtenerAsync(id, ct).ConfigureAwait(false);
        return r is null ? NoEncontrada<FicheroRemesaDto>() : Resultado.Ok(new FicheroRemesaDto(r.Fichero, r.NombreArchivo));
    }

    public Task<Resultado<RemesaCreadaDto>> CrearAsync(Guid empresaId, CrearRemesaComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        return comando.Tipo == TipoRemesa.Pago ? CrearPagoAsync(empresaId, comando, ct) : CrearCobroAsync(empresaId, comando, ct);
    }

    private async Task<Resultado<RemesaCreadaDto>> CrearCobroAsync(Guid empresaId, CrearRemesaComando comando, CancellationToken ct)
    {
        var facturaIds = comando.FacturaIds ?? [];
        var efectoIds = comando.EfectoIds ?? [];
        if (facturaIds.Count == 0 && efectoIds.Count == 0)
        {
            return Resultado.Fallo<RemesaCreadaDto>(Error.Validacion("remesa.sin_facturas", "Selecciona al menos una factura para domiciliar."));
        }

        var empresa = await _empresas.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        if (empresa is null)
        {
            return Resultado.Fallo<RemesaCreadaDto>(Error.NoEncontrado("empresa.no_encontrada", "La empresa no existe."));
        }

        var banco = await BancoAsync(comando.CuentaBancariaId, ct).ConfigureAwait(false);
        if (banco.EsFallo)
        {
            return Resultado.Fallo<RemesaCreadaDto>(banco.Error);
        }

        var ibanAcreedor = banco.Valor?.Iban ?? empresa.Iban;
        if (string.IsNullOrWhiteSpace(ibanAcreedor) || string.IsNullOrWhiteSpace(empresa.IdentificadorAcreedor))
        {
            return Resultado.Fallo<RemesaCreadaDto>(Error.Validacion("remesa.empresa_sin_datos_cobro",
                "Configura el IBAN (o una cuenta bancaria) y el identificador de acreedor de la empresa (Ajustes → Datos de cobro)."));
        }

        var fechaCobro = comando.FechaCargo ?? Hoy.AddDays(3);
        var esquema = string.Equals(comando.Esquema, "B2B", StringComparison.OrdinalIgnoreCase) ? "B2B" : "CORE";
        var secuencia = (comando.Secuencia ?? "OOFF").ToUpperInvariant();
        if (secuencia is not ("OOFF" or "FRST" or "RCUR" or "FNAL"))
        {
            secuencia = "OOFF";
        }

        var remesa = Remesa.Crear(empresaId, TipoRemesa.Cobro, await _remesas.SiguienteNumeroAsync(TipoRemesa.Cobro, Hoy.Year, ct).ConfigureAwait(false),
            Hoy, fechaCobro, banco.Valor?.Id, esquema, secuencia, _reloj);
        if (remesa.EsFallo)
        {
            return Resultado.Fallo<RemesaCreadaDto>(remesa.Error);
        }

        var condiciones = remesa.Valor.Condiciones(comando.Modalidad, comando.Condiciones);
        if (condiciones.EsFallo)
        {
            return Resultado.Fallo<RemesaCreadaDto>(condiciones.Error);
        }

        var omitidas = new List<string>();
        var adeudos = new List<XmlSepa.Adeudo>();
        var vivasF = await _remesas.DocumentosVivosAsync(TipoDocumentoTesoreria.Factura, facturaIds, ct).ConfigureAwait(false);
        foreach (var facturaId in facturaIds.Distinct())
        {
            var factura = await _facturas.ObtenerAsync(facturaId, ct).ConfigureAwait(false);
            if (factura is null || factura.ClienteId is null)
            {
                omitidas.Add($"{facturaId}: factura no encontrada o sin cliente.");
                continue;
            }

            if (factura.Estado == "Anulada")
            {
                omitidas.Add($"{factura.NumeroCompleto}: está anulada.");
                continue;
            }

            if (vivasF.TryGetValue(facturaId, out var otra))
            {
                omitidas.Add($"{factura.NumeroCompleto}: ya está en la remesa {otra}.");
                continue;
            }

            var pendiente = Redondeo.Dos(factura.Total - await _movimientos.SumaAsync(TipoDocumentoTesoreria.Factura, facturaId, ct).ConfigureAwait(false));
            if (pendiente <= 0)
            {
                omitidas.Add($"{factura.NumeroCompleto}: ya está cobrada.");
                continue;
            }

            var cliente = await _clientes.ObtenerAsync(factura.ClienteId.Value, ct).ConfigureAwait(false);
            if (cliente is null || string.IsNullOrWhiteSpace(cliente.Iban) || string.IsNullOrWhiteSpace(cliente.MandatoReferencia) || cliente.MandatoFecha is null)
            {
                omitidas.Add($"{factura.NumeroCompleto}: el cliente no tiene IBAN y mandato de domiciliación.");
                continue;
            }

            remesa.Valor.AgregarLinea(TipoDocumentoTesoreria.Factura, facturaId, factura.NumeroCompleto, cliente.Nombre, cliente.Iban, cliente.MandatoReferencia,
                cliente.MandatoFecha, pendiente, factura.FechaVencimiento);
            adeudos.Add(new XmlSepa.Adeudo(factura.NumeroCompleto, pendiente, cliente.Nombre, cliente.Iban!, cliente.MandatoReferencia!, cliente.MandatoFecha.Value,
                $"Factura {factura.NumeroCompleto}"));
        }

        var vivasE = await _remesas.DocumentosVivosAsync(TipoDocumentoTesoreria.Cartera, efectoIds, ct).ConfigureAwait(false);
        var anulados = await _cartera.AnuladosAsync(efectoIds, ct).ConfigureAwait(false);
        foreach (var efectoId in efectoIds.Distinct())
        {
            var efecto = await _cartera.ObtenerAsync(efectoId, ct).ConfigureAwait(false);
            if (efecto is null || efecto.Sentido != SentidoCartera.Cobro || anulados.Contains(efectoId))
            {
                omitidas.Add($"{efecto?.Documento ?? efectoId.ToString()}: no es un efecto a cobrar vivo.");
                continue;
            }

            if (vivasE.TryGetValue(efectoId, out var otra))
            {
                omitidas.Add($"{efecto.Documento}: ya está en la remesa {otra}.");
                continue;
            }

            var pendiente = Redondeo.Dos(efecto.Importe - await _movimientos.SumaAsync(TipoDocumentoTesoreria.Cartera, efectoId, ct).ConfigureAwait(false));
            if (pendiente <= 0)
            {
                omitidas.Add($"{efecto.Documento}: ya está cobrado.");
                continue;
            }

            var cliente = efecto.TerceroId is { } tid ? await _clientes.ObtenerAsync(tid, ct).ConfigureAwait(false) : null;
            if (cliente is null || string.IsNullOrWhiteSpace(cliente.Iban) || string.IsNullOrWhiteSpace(cliente.MandatoReferencia) || cliente.MandatoFecha is null)
            {
                omitidas.Add($"{efecto.Documento}: el cliente no tiene IBAN y mandato de domiciliación.");
                continue;
            }

            remesa.Valor.AgregarLinea(TipoDocumentoTesoreria.Cartera, efectoId, efecto.Documento, cliente.Nombre, cliente.Iban, cliente.MandatoReferencia,
                cliente.MandatoFecha, pendiente, efecto.Vencimiento);
            adeudos.Add(new XmlSepa.Adeudo(efecto.Documento, pendiente, cliente.Nombre, cliente.Iban!, cliente.MandatoReferencia!, cliente.MandatoFecha.Value,
                $"Recibo {efecto.Documento}"));
        }

        if (adeudos.Count == 0)
        {
            return Resultado.Fallo<RemesaCreadaDto>(Error.Validacion("remesa.sin_adeudos", "Ninguna de las facturas seleccionadas se puede domiciliar. " + string.Join(" ", omitidas)));
        }

        var mensajeId = XmlSepa.NuevoMensajeId(_reloj);
        var xml = XmlSepa.Adeudos(mensajeId, _reloj.AhoraUtc, empresa.RazonSocial, ibanAcreedor, banco.Valor?.Bic, empresa.IdentificadorAcreedor!, fechaCobro, adeudos,
            esquema, secuencia);
        remesa.Valor.AdjuntarFichero(xml, $"remesa-{remesa.Valor.Ejercicio}-{remesa.Valor.Numero}-{fechaCobro:yyyyMMdd}.xml", mensajeId);
        _remesas.Agregar(remesa.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new RemesaCreadaDto(await DtoAsync(remesa.Valor, ct).ConfigureAwait(false), omitidas));
    }

    private async Task<Resultado<RemesaCreadaDto>> CrearPagoAsync(Guid empresaId, CrearRemesaComando comando, CancellationToken ct)
    {
        var gastoIds = comando.GastoIds ?? [];
        var banco = await BancoAsync(comando.CuentaBancariaId, ct).ConfigureAwait(false);
        if (banco.EsFallo)
        {
            return Resultado.Fallo<RemesaCreadaDto>(banco.Error);
        }

        var bloqueados = await _remesas.DocumentosVivosAsync(TipoDocumentoTesoreria.Gasto, gastoIds, ct).ConfigureAwait(false);
        var recogida = await Pagos.RecopilarAsync(empresaId, new GenerarPagosComando(gastoIds, comando.FechaCargo), _gastos, _proveedores, _empresas, _movimientos,
            exigeIban: true, ct, ibanOrdenanteAparte: banco.Valor?.Iban is not null, bloqueados: bloqueados).ConfigureAwait(false);
        if (recogida.EsFallo)
        {
            return Resultado.Fallo<RemesaCreadaDto>(recogida.Error);
        }

        var (empresa, pagos, omitidos) = recogida.Valor;
        var fechaPago = comando.FechaCargo ?? Hoy.AddDays(1);
        var remesa = Remesa.Crear(empresaId, TipoRemesa.Pago, await _remesas.SiguienteNumeroAsync(TipoRemesa.Pago, Hoy.Year, ct).ConfigureAwait(false),
            Hoy, fechaPago, banco.Valor?.Id, null, null, _reloj);
        if (remesa.EsFallo)
        {
            return Resultado.Fallo<RemesaCreadaDto>(remesa.Error);
        }

        foreach (var p in pagos)
        {
            remesa.Valor.AgregarLinea(TipoDocumentoTesoreria.Gasto, p.GastoId, p.Concepto, p.ProveedorNombre, p.Iban, null, null, p.Importe);
        }

        var mensajeId = XmlSepa.NuevoMensajeId(_reloj);
        var xml = XmlSepa.Transferencias(mensajeId, _reloj.AhoraUtc, empresa.RazonSocial, banco.Valor?.Iban ?? empresa.Iban!, banco.Valor?.Bic, fechaPago, pagos);
        remesa.Valor.AdjuntarFichero(xml, $"transferencias-{remesa.Valor.Ejercicio}-{remesa.Valor.Numero}-{fechaPago:yyyyMMdd}.xml", mensajeId);
        _remesas.Agregar(remesa.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new RemesaCreadaDto(await DtoAsync(remesa.Valor, ct).ConfigureAwait(false), omitidos));
    }

    public async Task<Resultado<RemesaDto>> PresentarAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _remesas.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (r is null)
        {
            return NoEncontrada<RemesaDto>();
        }

        var p = r.Presentar(_reloj);
        if (p.EsFallo)
        {
            return Resultado.Fallo<RemesaDto>(p.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(r, ct).ConfigureAwait(false));
    }

    /// <summary>Cambia la modalidad (al vencimiento, en gestión de cobro o al descuento) y las condiciones de una remesa viva.</summary>
    public async Task<Resultado<RemesaDto>> CondicionesAsync(Guid id, CondicionesRemesaComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var r = await _remesas.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (r is null)
        {
            return NoEncontrada<RemesaDto>();
        }

        var c = r.Condiciones(comando.Modalidad, comando.Condiciones);
        if (c.EsFallo)
        {
            return Resultado.Fallo<RemesaDto>(c.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(r, ct).ConfigureAwait(false));
    }

    /// <summary>Lo que liquidaría el banco en esa fecha (intereses, comisión, IVA, gastos y líquido), sin registrar nada.</summary>
    public async Task<Resultado<CalculoLiquidacionRemesa>> CalcularAsync(Guid id, DateOnly? fecha, CancellationToken ct = default)
    {
        var r = await _remesas.ObtenerAsync(id, ct).ConfigureAwait(false);
        return r is null ? NoEncontrada<CalculoLiquidacionRemesa>() : Resultado.Ok(r.Calcular(fecha ?? (r.Modalidad == ModalidadRemesa.Descuento ? Hoy : r.FechaCargo)));
    }

    /// <summary>
    /// Al vencimiento de una remesa al descuento: el riesgo con el banco se cancela (5208 contra 4311) por lo que no se
    /// devolvió. Los recibos devueltos antes ya pagaron su riesgo al banco.
    /// </summary>
    public async Task<Resultado<RemesaDto>> CancelarRiesgoAsync(Guid empresaId, Guid id, DateOnly? fecha, CancellationToken ct = default)
    {
        var r = await _remesas.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (r is null)
        {
            return NoEncontrada<RemesaDto>();
        }

        var dia = fecha ?? (Hoy < r.FechaCargo ? r.FechaCargo : Hoy);
        var c = r.CancelarRiesgo(dia);
        if (c.EsFallo)
        {
            return Resultado.Fallo<RemesaDto>(c.Error);
        }

        var movimientos = r.Lineas.Where(l => l.MovimientoId is not null).Select(l => l.MovimientoId!.Value).ToList();
        var devueltas = new HashSet<Guid>();
        if (_devoluciones is not null && movimientos.Count > 0)
        {
            devueltas.UnionWith((await _devoluciones.MotivosPorMovimientoAsync(movimientos, ct).ConfigureAwait(false)).Keys);
        }

        var vivo = Redondeo.Dos(r.Lineas.Where(l => l.MovimientoId is { } m && !devueltas.Contains(m)).Sum(l => l.Importe));
        if (_contabilizacion is not null && vivo > 0)
        {
            _contabilizacion.EncolarAsientoDirecto(empresaId, OrigenRemesaDescuento, ContabilizacionTesoreria.Derivado(r.Id, "riesgo"), SentidoMovimiento.Cobro,
                $"Vencimiento remesa al descuento {r.Codigo}", dia, vivo, CuentasPuente.Deudas, CuentasPuente.Descontados);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        if (_contabilizacion is not null)
        {
            await _contabilizacion.DespacharAsync(ct: ct).ConfigureAwait(false);
        }

        return Resultado.Ok(await DtoAsync(r, ct).ConfigureAwait(false));
    }

    public const string OrigenRemesaDescuento = "RemesaDescuento";

    public async Task<Resultado<RemesaDto>> AnularAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _remesas.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (r is null)
        {
            return NoEncontrada<RemesaDto>();
        }

        var a = r.Anular(_reloj);
        if (a.EsFallo)
        {
            return Resultado.Fallo<RemesaDto>(a.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(r, ct).ConfigureAwait(false));
    }

    /// <summary>
    /// Marca la remesa como cobrada (adeudos) o pagada (transferencias): registra un movimiento por línea, con la cuenta
    /// bancaria de la remesa y su asiento (572… contra el cliente o el proveedor). Falla, sin registrar nada, si algún
    /// documento ya no tiene pendiente el importe remesado (p. ej. se cobró a mano entretanto).
    /// </summary>
    public async Task<Resultado<RemesaDto>> LiquidarAsync(Guid empresaId, Guid id, LiquidarRemesaComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var r = await _remesas.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (r is null)
        {
            return NoEncontrada<RemesaDto>();
        }

        var fecha = comando.Fecha ?? r.FechaCargo;
        var puede = r.PuedeLiquidarse(fecha);
        if (puede.EsFallo)
        {
            return Resultado.Fallo<RemesaDto>(puede.Error);
        }

        var problemas = new List<string>();
        foreach (var linea in r.Lineas)
        {
            var total = await TotalDocumentoAsync(linea.TipoDocumento, linea.DocumentoId, ct).ConfigureAwait(false);
            var pendiente = total is null ? 0m : Redondeo.Dos(total.Value - await _movimientos.SumaAsync(linea.TipoDocumento, linea.DocumentoId, ct).ConfigureAwait(false));
            if (pendiente < linea.Importe)
            {
                problemas.Add($"{linea.Documento} (pendiente {Redondeo.Formatear(pendiente)} €)");
            }
        }

        if (problemas.Count > 0)
        {
            return Resultado.Fallo<RemesaDto>(Error.Conflicto("remesa.pendiente_cambiado",
                "Estos documentos ya no tienen pendiente el importe remesado: " + string.Join(", ", problemas) + ". Anula sus cobros o pagos, o anula la remesa."));
        }

        var sentido = r.Tipo == TipoRemesa.Cobro ? SentidoMovimiento.Cobro : SentidoMovimiento.Pago;
        var descuento = r.Modalidad == ModalidadRemesa.Descuento;
        var metodo = (r.Tipo == TipoRemesa.Cobro ? (descuento ? "Descuento remesa " : "Domiciliación remesa ") : "Transferencia remesa ") + r.Codigo;
        var calculo = r.Calcular(fecha);
        if (r.Modalidad != ModalidadRemesa.Vencimiento && calculo.Liquido <= 0)
        {
            return Resultado.Fallo<RemesaDto>(Error.Validacion("remesa.liquido", "Los intereses y gastos se comen todo el nominal: revisa las condiciones de la remesa."));
        }

        var mapa = new Dictionary<Guid, Guid>();
        foreach (var linea in r.Lineas)
        {
            // Al descuento, cada factura pasa a efectos descontados (4311 contra el cliente): el dinero llega en un solo abono.
            var m = Movimiento.Crear(empresaId, linea.TipoDocumento, linea.DocumentoId, sentido, linea.Importe, fecha, metodo, _reloj, r.CuentaBancariaId,
                descuento ? CuentasPuente.Descontados : null);
            if (m.EsFallo)
            {
                return Resultado.Fallo<RemesaDto>(m.Error);
            }

            _movimientos.Agregar(m.Valor);
            mapa[linea.Id] = m.Valor.Id;
            if (_contabilizacion is not null)
            {
                await _contabilizacion.EncolarMovimientoAsync(m.Valor, aplicacionAnticipo: false, ct: ct).ConfigureAwait(false);
            }
        }

        r.Liquidar(fecha, mapa);
        if (_contabilizacion is not null && r.Modalidad != ModalidadRemesa.Vencimiento)
        {
            var banco = await _contabilizacion.CuentaTesoreriaAsync(r.CuentaBancariaId, null, ct).ConfigureAwait(false);
            void Cargo(string etiqueta, string texto, decimal importe, string cuenta)
            {
                if (importe > 0)
                {
                    _contabilizacion.EncolarAsientoDirecto(empresaId, OrigenRemesaDescuento, ContabilizacionTesoreria.Derivado(r.Id, etiqueta), SentidoMovimiento.Pago,
                        $"{texto} remesa {r.Codigo}", fecha, importe, banco, cuenta);
                }
            }

            if (descuento)
            {
                // El banco abona el nominal y queda la deuda por efectos descontados hasta el vencimiento.
                _contabilizacion.EncolarAsientoDirecto(empresaId, OrigenRemesaDescuento, ContabilizacionTesoreria.Derivado(r.Id, "abono"), SentidoMovimiento.Cobro,
                    $"Descuento remesa {r.Codigo}", fecha, r.Total, banco, CuentasPuente.Deudas);
            }

            Cargo("intereses", "Intereses del descuento", calculo.Intereses, "665");
            Cargo("comision", "Comisión y gastos", Redondeo.Dos(calculo.Comision + calculo.Gastos), "626");
            Cargo("iva", "IVA de la comisión", calculo.IvaComision, "472");
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        if (_contabilizacion is not null)
        {
            await _contabilizacion.DespacharAsync(ct: ct).ConfigureAwait(false);
        }

        return Resultado.Ok(await DtoAsync(r, ct).ConfigureAwait(false));
    }

    private async Task<decimal?> TotalDocumentoAsync(TipoDocumentoTesoreria tipo, Guid id, CancellationToken ct) => tipo switch
    {
        TipoDocumentoTesoreria.Factura => (await _facturas.ObtenerAsync(id, ct).ConfigureAwait(false)) is { Estado: not "Anulada" } f ? f.Total : null,
        TipoDocumentoTesoreria.Gasto => (await _gastos.ObtenerAsync(id, ct).ConfigureAwait(false)) is { Estado: not "Anulado" } g ? g.Total : null,
        _ => (await _cartera.ObtenerAsync(id, ct).ConfigureAwait(false))?.Importe,
    };

    /// <summary>Cuenta bancaria de la remesa: la indicada (activa) o la predeterminada; null si no hay ninguna.</summary>
    private async Task<Resultado<CuentaBancaria?>> BancoAsync(Guid? id, CancellationToken ct)
    {
        if (id is { } cid)
        {
            var b = await _bancos.ObtenerAsync(cid, ct).ConfigureAwait(false);
            if (b is null)
            {
                return Resultado.Fallo<CuentaBancaria?>(Error.NoEncontrado("banco.no_encontrado", "La cuenta bancaria no existe."));
            }

            return b.Activa && b.Tipo == TipoCuentaTesoreria.Banco
                ? Resultado.Ok<CuentaBancaria?>(b)
                : Resultado.Fallo<CuentaBancaria?>(Error.Conflicto("banco.no_remesable", $"«{b.Nombre}» no es una cuenta bancaria activa."));
        }

        return Resultado.Ok((await _bancos.ListarAsync(ct).ConfigureAwait(false)).FirstOrDefault(b => b.Activa && b.Predeterminada));
    }

    private async Task<RemesaDto> DtoAsync(Remesa r, CancellationToken ct)
    {
        var bancos = (await _bancos.ListarAsync(ct).ConfigureAwait(false)).ToDictionary(b => b.Id, b => b.Nombre);
        var movimientos = r.Lineas.Where(l => l.MovimientoId is not null).Select(l => l.MovimientoId!.Value).ToList();
        var devueltas = _devoluciones is null || movimientos.Count == 0
            ? new Dictionary<Guid, string>()
            : await _devoluciones.MotivosPorMovimientoAsync(movimientos, ct).ConfigureAwait(false);
        return Dto(r, bancos, devueltas, conLineas: true);
    }

    private static RemesaDto Dto(Remesa r, Dictionary<Guid, string> bancos, IReadOnlyDictionary<Guid, string>? devueltas, bool conLineas) => new(
        r.Id, r.Tipo.ToString(), r.Codigo, r.Fecha, r.FechaCargo, r.CuentaBancariaId,
        r.CuentaBancariaId is { } b && bancos.TryGetValue(b, out var nb) ? nb : null, r.Esquema, r.Secuencia, r.Estado.ToString(),
        r.Modalidad == ModalidadRemesa.Descuento && r.Estado == EstadoRemesa.Liquidada ? (r.RiesgoCanceladoEn is null ? "Descontada" : "Vencida")
            : Remesa.Descripcion(r.Tipo, r.Estado), r.Total, r.Lineas.Count, r.NombreArchivo, r.FechaLiquidacion,
        conLineas
            ? r.Lineas.Select(l =>
            {
                string? motivo = null;
                var devuelta = l.MovimientoId is { } m && devueltas is not null && devueltas.TryGetValue(m, out motivo);
                return new LineaRemesaDto(l.Id, l.TipoDocumento.ToString(), l.DocumentoId, l.Documento, l.TerceroNombre, l.Iban, l.Mandato, l.Importe,
                    l.MovimientoId, devuelta, motivo);
            }).ToList()
            : [],
        r.Modalidad.ToString(),
        r.Modalidad == ModalidadRemesa.Vencimiento ? null : new CondicionesRemesa(r.PorcentajeInteres, r.DiasMinimos, r.GastosFijos, r.GastosPorEfecto, r.Timbres,
            r.OtrosGastos, r.PorcentajeComision, r.PorcentajeIvaComision),
        r.Intereses, r.Comision, r.IvaComision, r.Gastos, r.Liquido, r.RiesgoCanceladoEn);

    private static Resultado<T> NoEncontrada<T>() => Resultado.Fallo<T>(Error.NoEncontrado("remesa.no_encontrada", "La remesa no existe."));
}
