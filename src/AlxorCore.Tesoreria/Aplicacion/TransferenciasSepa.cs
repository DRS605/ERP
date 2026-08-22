using System.Globalization;
using System.Xml.Linq;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Terceros.Aplicacion;
using AlxorCore.Tesoreria.Dominio;

namespace AlxorCore.Tesoreria.Aplicacion;

/// <summary>Resultado de generar una remesa de transferencias/pagos (SEPA o confirming).</summary>
public sealed record RemesaPagoDto(string Fichero, string NombreArchivo, int NumeroPagos, decimal Total, IReadOnlyList<string> Omitidos);

/// <summary>Datos para generar una remesa de pagos a proveedores desde los gastos indicados.</summary>
public sealed record GenerarPagosComando(IReadOnlyList<Guid> GastoIds, DateOnly? FechaPago = null);

/// <summary>
/// Genera una <b>remesa de transferencias SEPA</b> (<c>pain.001.001.03</c>, equivalente al Cuaderno
/// 34.14) para pagar a los proveedores el importe pendiente de los gastos indicados. Necesita el IBAN
/// de la empresa (ordenante) y el de cada proveedor (beneficiario); omite (informando) los que no lo
/// tengan o ya estén pagados.
/// </summary>
public sealed class GenerarTransferenciasSepa
{
    private static readonly XNamespace Ns = "urn:iso:std:iso:20022:tech:xsd:pain.001.001.03";

    private readonly IConsultaGastos _gastos;
    private readonly IConsultaProveedores _proveedores;
    private readonly IConsultaEmpresas _empresas;
    private readonly IRepositorioMovimientos _movimientos;
    private readonly IReloj _reloj;

    public GenerarTransferenciasSepa(IConsultaGastos gastos, IConsultaProveedores proveedores, IConsultaEmpresas empresas, IRepositorioMovimientos movimientos, IReloj reloj)
    {
        _gastos = gastos;
        _proveedores = proveedores;
        _empresas = empresas;
        _movimientos = movimientos;
        _reloj = reloj;
    }

    public async Task<Resultado<RemesaPagoDto>> EjecutarAsync(Guid empresaId, GenerarPagosComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var recogida = await Pagos.RecopilarAsync(empresaId, comando, _gastos, _proveedores, _empresas, _movimientos, exigeIban: true, ct).ConfigureAwait(false);
        if (recogida.EsFallo)
        {
            return Resultado.Fallo<RemesaPagoDto>(recogida.Error);
        }

        var (empresa, pagos, omitidos) = recogida.Valor;
        var fechaPago = comando.FechaPago ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime).AddDays(1);
        var xml = Construir(empresa.RazonSocial, empresa.Iban!, fechaPago, pagos);
        var total = Redondeo.Dos(pagos.Sum(p => p.Importe));
        return Resultado.Ok(new RemesaPagoDto(xml, $"transferencias-{fechaPago:yyyyMMdd}.xml", pagos.Count, total, omitidos));
    }

    private string Construir(string ordenanteNombre, string ordenanteIban, DateOnly fechaPago, IReadOnlyList<PagoProveedor> pagos)
    {
        var total = Redondeo.Dos(pagos.Sum(p => p.Importe)).ToString("F2", CultureInfo.InvariantCulture);
        var numero = pagos.Count.ToString(CultureInfo.InvariantCulture);
        var ahora = _reloj.AhoraUtc.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
        var mensajeId = $"ALX{_reloj.AhoraUtc:yyyyMMddHHmmss}{Guid.NewGuid():N}"[..35];

        var transacciones = pagos.Select(p => new XElement(
            Ns + "CdtTrfTxInf",
            new XElement(Ns + "PmtId", new XElement(Ns + "EndToEndId", Limitar(p.Referencia, 35))),
            new XElement(Ns + "Amt", new XElement(Ns + "InstdAmt", new XAttribute("Ccy", "EUR"), p.Importe.ToString("F2", CultureInfo.InvariantCulture))),
            new XElement(Ns + "CdtrAgt", new XElement(Ns + "FinInstnId", new XElement(Ns + "Othr", new XElement(Ns + "Id", "NOTPROVIDED")))),
            new XElement(Ns + "Cdtr", new XElement(Ns + "Nm", Limitar(p.ProveedorNombre, 70))),
            new XElement(Ns + "CdtrAcct", new XElement(Ns + "Id", new XElement(Ns + "IBAN", p.Iban!))),
            new XElement(Ns + "RmtInf", new XElement(Ns + "Ustrd", Limitar(p.Concepto, 140)))));

        var documento = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(
                Ns + "Document",
                new XElement(
                    Ns + "CstmrCdtTrfInitn",
                    new XElement(
                        Ns + "GrpHdr",
                        new XElement(Ns + "MsgId", mensajeId),
                        new XElement(Ns + "CreDtTm", ahora),
                        new XElement(Ns + "NbOfTxs", numero),
                        new XElement(Ns + "CtrlSum", total),
                        new XElement(Ns + "InitgPty", new XElement(Ns + "Nm", Limitar(ordenanteNombre, 70)))),
                    new XElement(
                        Ns + "PmtInf",
                        new XElement(Ns + "PmtInfId", mensajeId),
                        new XElement(Ns + "PmtMtd", "TRF"),
                        new XElement(Ns + "NbOfTxs", numero),
                        new XElement(Ns + "CtrlSum", total),
                        new XElement(Ns + "PmtTpInf", new XElement(Ns + "SvcLvl", new XElement(Ns + "Cd", "SEPA"))),
                        new XElement(Ns + "ReqdExctnDt", fechaPago.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                        new XElement(Ns + "Dbtr", new XElement(Ns + "Nm", Limitar(ordenanteNombre, 70))),
                        new XElement(Ns + "DbtrAcct", new XElement(Ns + "Id", new XElement(Ns + "IBAN", ordenanteIban))),
                        new XElement(Ns + "DbtrAgt", new XElement(Ns + "FinInstnId", new XElement(Ns + "Othr", new XElement(Ns + "Id", "NOTPROVIDED")))),
                        transacciones))));

        return documento.Declaration + Environment.NewLine + documento.ToString();
    }

    private static string Limitar(string valor, int max) => valor.Length <= max ? valor : valor[..max];
}
