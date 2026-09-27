using System.Security.Cryptography;
using System.Text;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Tesoreria.Dominio;

namespace AlxorCore.Tesoreria.Aplicacion;

/// <summary>Datos para importar un extracto Norma 43. Sin cuenta bancaria, se busca la que tenga la cuenta del fichero.</summary>
public sealed record ImportarExtractoComando(string Contenido, Guid? CuentaBancariaId = null, string? NombreArchivo = null);

public sealed record ExtractoDto(
    Guid Id, Guid CuentaBancariaId, string? CuentaBancaria, string CuentaFichero, DateOnly? Desde, DateOnly? Hasta, decimal? SaldoInicial, decimal? SaldoFinal,
    string NombreArchivo, int NumeroApuntes, int Pendientes, DateTimeOffset CreadoEn);

public sealed record CasacionDto(Guid MovimientoId, string TipoDocumento, Guid DocumentoId, string Documento, decimal Importe, bool MovimientoCreado);

public sealed record ApunteBancarioDto(
    Guid Id, int Orden, DateOnly Fecha, DateOnly? FechaValor, decimal Importe, string Concepto, string? ConceptoComun, string? Referencia1, string? Referencia2,
    string Estado, string? CuentaAsiento, string? ConceptoAsiento, IReadOnlyList<CasacionDto> Casaciones);

public sealed record ExtractoDetalleDto(ExtractoDto Extracto, IReadOnlyList<ApunteBancarioDto> Apuntes);

/// <summary>
/// Posible casación de un apunte. <paramref name="Tipo"/>: «Movimiento» (cobro o pago ya registrado y sin conciliar),
/// «Remesa» (remesa liquidada: todos sus movimientos), o «Factura», «Gasto», «Cartera» (documento pendiente: al casar se
/// registra su cobro o pago). <paramref name="Importe"/> lleva el signo del extracto (cobro +, pago −).
/// </summary>
public sealed record CandidatoConciliacionDto(string Tipo, Guid Id, string Documento, string Tercero, DateOnly? Fecha, decimal Importe, decimal? Pendiente, int Puntuacion);

/// <summary>Un elemento de una casación manual. <paramref name="Importe"/> (positivo) solo para documentos: por defecto su pendiente.</summary>
public sealed record ElementoConciliacion(string Tipo, Guid Id, decimal? Importe = null);

public sealed record ConciliarApunteComando(IReadOnlyList<ElementoConciliacion> Elementos);

/// <summary>Asiento directo desde un apunte: cuenta de contrapartida (626, 669, 769…) y concepto.</summary>
public sealed record AsientoApunteComando(string Cuenta, string? Concepto = null);

public sealed record ConciliacionAutomaticaComando(int DiasMargen = 5);

public sealed record ResultadoConciliacionAutomaticaDto(int Conciliados, int Pendientes, IReadOnlyList<string> Detalle);

public interface IRepositorioConciliacion
{
    void Agregar(ExtractoImportado extracto);

    void Agregar(ApunteBancario apunte);

    Task<ExtractoImportado?> ObtenerExtractoAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<ExtractoImportado>> ListarExtractosAsync(Guid? cuentaBancariaId, CancellationToken ct = default);

    Task<IReadOnlyDictionary<Guid, int>> PendientesPorExtractoAsync(IReadOnlyCollection<Guid> extractoIds, CancellationToken ct = default);

    Task<bool> ExisteHuellaAsync(Guid cuentaBancariaId, string huella, CancellationToken ct = default);

    Task<IReadOnlyList<ApunteBancario>> ApuntesAsync(Guid extractoId, CancellationToken ct = default);

    Task<ApunteBancario?> ObtenerApunteAsync(Guid id, CancellationToken ct = default);

    void Eliminar(ExtractoImportado extracto, IReadOnlyList<ApunteBancario> apuntes);

    /// <summary>Cobros y pagos vivos (ni anulaciones ni anulados) sin conciliar de una cuenta en un rango de fechas.</summary>
    Task<IReadOnlyList<Movimiento>> MovimientosLibresAsync(Guid cuentaBancariaId, bool incluirSinCuenta, DateOnly desde, DateOnly hasta, CancellationToken ct = default);

    /// <summary>¿Alguno de estos movimientos está ya casado con un apunte?</summary>
    Task<IReadOnlySet<Guid>> ConciliadosAsync(IReadOnlyCollection<Guid> movimientoIds, CancellationToken ct = default);
}

/// <summary>
/// Conciliación bancaria persistente. Se importa el extracto Norma 43 de una cuenta bancaria (queda guardado con sus
/// apuntes) y cada apunte se concilia:
/// <list type="bullet">
/// <item>automáticamente: con un cobro o pago ya registrado por el mismo importe y fecha cercana, con una remesa
/// liquidada por su total, o con la factura, gasto o efecto pendiente de ese importe (usando como pista el número del
/// documento o el nombre del tercero en el concepto);</item>
/// <item>a mano: con uno o varios movimientos o documentos cuya suma es la del apunte (N documentos contra 1 apunte);</item>
/// <item>con un asiento directo (comisiones, gastos, intereses) contra la cuenta que se elija.</item>
/// </list>
/// Toda conciliación se puede deshacer: lo que registró se anula (con su contraasiento).
/// </summary>
public sealed class ConciliacionBancaria
{
    public const string MetodoConciliacion = "Transferencia (extracto)";

    private readonly IRepositorioConciliacion _repo;
    private readonly IRepositorioCuentasBancarias _bancos;
    private readonly IRepositorioMovimientos _movimientos;
    private readonly IRepositorioRemesas _remesas;
    private readonly IRepositorioCartera _cartera;
    private readonly IConsultaFacturas _facturas;
    private readonly IConsultaGastos _gastos;
    private readonly IConsultaTesoreria _consulta;
    private readonly IUnidadDeTrabajoTesoreria _unidad;
    private readonly IReloj _reloj;
    private readonly ContabilizacionTesoreria? _contabilizacion;

    public ConciliacionBancaria(IRepositorioConciliacion repo, IRepositorioCuentasBancarias bancos, IRepositorioMovimientos movimientos, IRepositorioRemesas remesas,
        IRepositorioCartera cartera, IConsultaFacturas facturas, IConsultaGastos gastos, IConsultaTesoreria consulta, IUnidadDeTrabajoTesoreria unidad, IReloj reloj,
        ContabilizacionTesoreria? contabilizacion = null)
    {
        _repo = repo;
        _bancos = bancos;
        _movimientos = movimientos;
        _remesas = remesas;
        _cartera = cartera;
        _facturas = facturas;
        _gastos = gastos;
        _consulta = consulta;
        _unidad = unidad;
        _reloj = reloj;
        _contabilizacion = contabilizacion;
    }

    // ------------------------------------------------------------------ extractos

    public async Task<Resultado<ExtractoDetalleDto>> ImportarAsync(Guid empresaId, ImportarExtractoComando c, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(c);
        var leido = ParserNorma43.Parsear(c.Contenido);
        if (leido.EsFallo)
        {
            return Resultado.Fallo<ExtractoDetalleDto>(leido.Error);
        }

        var e = leido.Valor;
        var bancos = await _bancos.ListarAsync(ct).ConfigureAwait(false);
        CuentaBancaria? banco;
        if (c.CuentaBancariaId is { } id)
        {
            banco = bancos.FirstOrDefault(b => b.Id == id);
            if (banco is null)
            {
                return Resultado.Fallo<ExtractoDetalleDto>(Error.NoEncontrado("banco.no_encontrado", "La cuenta bancaria no existe."));
            }
        }
        else
        {
            banco = bancos.FirstOrDefault(b => b.Activa && CasaCuenta(b.Iban, e.Cuenta));
            if (banco is null)
            {
                return Resultado.Fallo<ExtractoDetalleDto>(Error.Validacion("extracto.sin_cuenta",
                    $"Ninguna cuenta bancaria tiene la cuenta {e.Cuenta} del fichero: elige la cuenta bancaria del extracto."));
            }
        }

        if (banco.Tipo != TipoCuentaTesoreria.Banco)
        {
            return Resultado.Fallo<ExtractoDetalleDto>(Error.Validacion("extracto.no_banco", "Los extractos Norma 43 son de cuentas bancarias, no de caja."));
        }

        var huella = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(c.Contenido.Replace("\r", string.Empty, StringComparison.Ordinal).Trim())));
        if (await _repo.ExisteHuellaAsync(banco.Id, huella, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<ExtractoDetalleDto>(Error.Conflicto("extracto.duplicado", "Este extracto ya está importado en la cuenta."));
        }

        var extracto = ExtractoImportado.Crear(empresaId, banco.Id, e.Cuenta, e.Desde, e.Hasta, e.SaldoInicial, e.SaldoFinal, c.NombreArchivo, huella, e.Apuntes.Count, _reloj);
        _repo.Agregar(extracto);
        var orden = 0;
        foreach (var a in e.Apuntes)
        {
            _repo.Agregar(ApunteBancario.Crear(extracto, ++orden, a.Fecha, a.FechaValor, a.Importe, a.Concepto, a.ConceptoComun, a.Documento, a.Referencia1, a.Referencia2));
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return await ObtenerAsync(empresaId, extracto.Id, ct).ConfigureAwait(false);
    }

    /// <summary>¿El IBAN (ES) corresponde a la cuenta del fichero (entidad + oficina + número)?</summary>
    public static bool CasaCuenta(string? iban, string cuentaFichero)
    {
        var digitos = new string((cuentaFichero ?? string.Empty).Where(char.IsAsciiDigit).ToArray());
        if (iban is null || iban.Length != 24 || digitos.Length != 18)
        {
            return false;
        }

        return iban[4..12] == digitos[..8] && iban[14..24] == digitos[8..];
    }

    public async Task<IReadOnlyList<ExtractoDto>> ListarAsync(Guid? cuentaBancariaId, CancellationToken ct = default)
    {
        var lista = await _repo.ListarExtractosAsync(cuentaBancariaId, ct).ConfigureAwait(false);
        var pendientes = await _repo.PendientesPorExtractoAsync(lista.Select(x => x.Id).ToList(), ct).ConfigureAwait(false);
        var bancos = (await _bancos.ListarAsync(ct).ConfigureAwait(false)).ToDictionary(b => b.Id, b => b.Nombre);
        return lista.Select(x => Dto(x, bancos, pendientes.GetValueOrDefault(x.Id))).ToList();
    }

    public async Task<Resultado<ExtractoDetalleDto>> ObtenerAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        var extracto = await _repo.ObtenerExtractoAsync(id, ct).ConfigureAwait(false);
        if (extracto is null)
        {
            return Resultado.Fallo<ExtractoDetalleDto>(Error.NoEncontrado("extracto.no_encontrado", "El extracto no existe."));
        }

        var apuntes = await _repo.ApuntesAsync(id, ct).ConfigureAwait(false);
        var docs = await DocumentosAsync(empresaId, ct).ConfigureAwait(false);
        var bancos = (await _bancos.ListarAsync(ct).ConfigureAwait(false)).ToDictionary(b => b.Id, b => b.Nombre);
        return Resultado.Ok(new ExtractoDetalleDto(Dto(extracto, bancos, apuntes.Count(a => a.Estado == EstadoApunte.Pendiente)),
            apuntes.OrderBy(a => a.Orden).Select(a => Dto(a, docs)).ToList()));
    }

    /// <summary>Borra un extracto importado por error. Solo si ninguno de sus apuntes está conciliado.</summary>
    public async Task<Resultado> EliminarAsync(Guid id, CancellationToken ct = default)
    {
        var extracto = await _repo.ObtenerExtractoAsync(id, ct).ConfigureAwait(false);
        if (extracto is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("extracto.no_encontrado", "El extracto no existe."));
        }

        var apuntes = await _repo.ApuntesAsync(id, ct).ConfigureAwait(false);
        if (apuntes.Any(a => a.Estado != EstadoApunte.Pendiente))
        {
            return Resultado.Fallo(Error.Conflicto("extracto.conciliado", "El extracto tiene apuntes conciliados: deshaz antes su conciliación."));
        }

        _repo.Eliminar(extracto, apuntes);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    // ------------------------------------------------------------------ candidatos y conciliación automática

    /// <summary>Movimientos, remesas y documentos que pueden casar con el apunte, de más a menos probables.</summary>
    public async Task<Resultado<IReadOnlyList<CandidatoConciliacionDto>>> CandidatosAsync(Guid empresaId, Guid apunteId, CancellationToken ct = default)
    {
        var apunte = await _repo.ObtenerApunteAsync(apunteId, ct).ConfigureAwait(false);
        if (apunte is null)
        {
            return Resultado.Fallo<IReadOnlyList<CandidatoConciliacionDto>>(Error.NoEncontrado("apunte.no_encontrado", "El apunte no existe."));
        }

        var contexto = await ContextoAsync(empresaId, apunte.CuentaBancariaId, apunte.Fecha.AddDays(-60), apunte.Fecha.AddDays(30), ct).ConfigureAwait(false);
        var pajar = Pajar(apunte);
        var lista = new List<CandidatoConciliacionDto>();
        foreach (var m in contexto.Movimientos.Where(m => Signo(m) * m.Importe * apunte.Importe > 0))
        {
            var d = contexto.Documento(m.TipoDocumento, m.DocumentoId);
            lista.Add(new CandidatoConciliacionDto("Movimiento", m.Id, $"{(m.Sentido == SentidoMovimiento.Cobro ? "Cobro" : "Pago")} {d.Numero}".Trim(), d.Tercero, m.Fecha,
                Signo(m) * m.Importe, null, Puntuar(pajar, d.Numero, d.Tercero) + (Signo(m) * m.Importe == apunte.Importe ? 5 : 0)));
        }

        foreach (var r in contexto.Remesas.Where(r => (r.Tipo == TipoRemesa.Cobro ? 1 : -1) * apunte.Importe > 0))
        {
            var importe = r.Tipo == TipoRemesa.Cobro ? r.Total : -r.Total;
            lista.Add(new CandidatoConciliacionDto("Remesa", r.Id, $"Remesa {r.Codigo} ({r.Lineas.Count} recibos)", string.Empty, r.FechaLiquidacion, importe, null,
                (importe == apunte.Importe ? 5 : 0) + (pajar.Contains("REMESA", StringComparison.Ordinal) ? 1 : 0)));
        }

        foreach (var d in contexto.Pendientes.Where(d => d.Signo * apunte.Importe > 0))
        {
            lista.Add(new CandidatoConciliacionDto(d.Tipo.ToString(), d.Id, d.Numero, d.Tercero, d.Fecha, d.Signo * d.Pendiente, d.Pendiente,
                Puntuar(pajar, d.Numero, d.Tercero) + (d.Signo * d.Pendiente == apunte.Importe ? 5 : 0)));
        }

        return Resultado.Ok<IReadOnlyList<CandidatoConciliacionDto>>(lista
            .OrderByDescending(x => x.Puntuacion)
            .ThenBy(x => Math.Abs(x.Importe - apunte.Importe))
            .ThenBy(x => x.Fecha is { } f ? Math.Abs(f.DayNumber - apunte.Fecha.DayNumber) : int.MaxValue)
            .Take(50).ToList());
    }

    /// <summary>
    /// Concilia los apuntes pendientes de un extracto: primero con movimientos ya registrados (mismo importe, fecha a
    /// menos de <see cref="ConciliacionAutomaticaComando.DiasMargen"/> días), después con remesas liquidadas por su total y,
    /// por último, con documentos pendientes de ese importe exacto (solo si es el único o la pista del concepto lo
    /// señala sin duda). Lo que no está claro queda pendiente para casarlo a mano.
    /// </summary>
    public async Task<Resultado<ResultadoConciliacionAutomaticaDto>> ConciliarAutomaticoAsync(Guid empresaId, Guid extractoId, ConciliacionAutomaticaComando c,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(c);
        var extracto = await _repo.ObtenerExtractoAsync(extractoId, ct).ConfigureAwait(false);
        if (extracto is null)
        {
            return Resultado.Fallo<ResultadoConciliacionAutomaticaDto>(Error.NoEncontrado("extracto.no_encontrado", "El extracto no existe."));
        }

        var margen = Math.Clamp(c.DiasMargen, 0, 60);
        var apuntes = (await _repo.ApuntesAsync(extractoId, ct).ConfigureAwait(false)).Where(a => a.Estado == EstadoApunte.Pendiente).OrderBy(a => a.Orden).ToList();
        if (apuntes.Count == 0)
        {
            return Resultado.Ok(new ResultadoConciliacionAutomaticaDto(0, 0, []));
        }

        var desde = apuntes.Min(a => a.Fecha).AddDays(-margen);
        var hasta = apuntes.Max(a => a.Fecha).AddDays(margen);
        var contexto = await ContextoAsync(empresaId, extracto.CuentaBancariaId, desde, hasta, ct).ConfigureAwait(false);
        var movimientos = contexto.Movimientos.ToList();
        var remesas = contexto.Remesas.ToList();
        var pendientes = contexto.Pendientes.ToList();
        var detalle = new List<string>();
        var conciliados = 0;

        foreach (var apunte in apuntes)
        {
            var pajar = Pajar(apunte);

            // 1) Un cobro o pago ya registrado.
            var candidatosMov = movimientos
                .Where(m => Signo(m) * m.Importe == apunte.Importe && Math.Abs(m.Fecha.DayNumber - apunte.Fecha.DayNumber) <= margen)
                .Select(m => (M: m, P: Puntuar(pajar, contexto.Documento(m.TipoDocumento, m.DocumentoId).Numero, contexto.Documento(m.TipoDocumento, m.DocumentoId).Tercero),
                    D: Math.Abs(m.Fecha.DayNumber - apunte.Fecha.DayNumber)))
                .OrderByDescending(x => x.P).ThenBy(x => x.D).ToList();
            if (candidatosMov.Count > 0 && (candidatosMov.Count == 1 || candidatosMov[0].P > candidatosMov[1].P || candidatosMov[0].D < candidatosMov[1].D))
            {
                var m = candidatosMov[0].M;
                if (apunte.Conciliar([(m.Id, m.TipoDocumento, m.DocumentoId, Signo(m) * m.Importe, false)], automatico: true, _reloj).EsCorrecto)
                {
                    movimientos.Remove(m);
                    conciliados++;
                    detalle.Add($"{apunte.Fecha:dd/MM} {Redondeo.Formatear(apunte.Importe)} €: {contexto.Documento(m.TipoDocumento, m.DocumentoId).Numero}");
                    continue;
                }
            }

            // 2) Una remesa liquidada por su total (el banco abona o carga la remesa en un solo apunte).
            var candidatasRem = remesas
                .Where(r => (r.Tipo == TipoRemesa.Cobro ? r.Total : -r.Total) == apunte.Importe && r.FechaLiquidacion is { } f
                            && Math.Abs(f.DayNumber - apunte.Fecha.DayNumber) <= margen)
                .ToList();
            if (candidatasRem.Count == 1)
            {
                var r = candidatasRem[0];
                var lineas = r.Lineas.Where(l => l.MovimientoId is not null).ToList();
                var movsRemesa = movimientos.Where(m => lineas.Any(l => l.MovimientoId == m.Id)).ToList();
                if (movsRemesa.Count == lineas.Count && lineas.Count > 0
                    && apunte.Conciliar(movsRemesa.Select(m => (m.Id, m.TipoDocumento, m.DocumentoId, Signo(m) * m.Importe, false)).ToList(), automatico: true, _reloj).EsCorrecto)
                {
                    movimientos.RemoveAll(movsRemesa.Contains);
                    remesas.Remove(r);
                    conciliados++;
                    detalle.Add($"{apunte.Fecha:dd/MM} {Redondeo.Formatear(apunte.Importe)} €: remesa {r.Codigo}");
                    continue;
                }
            }

            // 3) Un documento pendiente de ese importe: se registra su cobro o pago.
            var candidatosDoc = pendientes
                .Where(d => d.Signo * d.Pendiente == apunte.Importe)
                .Select(d => (Doc: d, P: Puntuar(pajar, d.Numero, d.Tercero)))
                .OrderByDescending(x => x.P).ToList();
            if (candidatosDoc.Count > 0 && (candidatosDoc.Count == 1 || candidatosDoc[0].P > candidatosDoc[1].P))
            {
                var d = candidatosDoc[0].Doc;
                var registrado = await RegistrarMovimientoAsync(empresaId, apunte, d.Tipo, d.Id, d.Signo > 0 ? SentidoMovimiento.Cobro : SentidoMovimiento.Pago, d.Pendiente, ct)
                    .ConfigureAwait(false);
                if (registrado.EsCorrecto
                    && apunte.Conciliar([(registrado.Valor.Id, d.Tipo, d.Id, apunte.Importe, true)], automatico: true, _reloj).EsCorrecto)
                {
                    pendientes.Remove(d);
                    conciliados++;
                    detalle.Add($"{apunte.Fecha:dd/MM} {Redondeo.Formatear(apunte.Importe)} €: {d.Numero} (registrado el {(d.Signo > 0 ? "cobro" : "pago")})");
                }
            }
        }

        await GuardarAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new ResultadoConciliacionAutomaticaDto(conciliados, apuntes.Count - conciliados, detalle));
    }

    // ------------------------------------------------------------------ conciliación manual, asiento directo y deshacer

    /// <summary>Casa el apunte con uno o varios movimientos, remesas o documentos cuya suma es su importe.</summary>
    public async Task<Resultado<ApunteBancarioDto>> ConciliarManualAsync(Guid empresaId, Guid apunteId, ConciliarApunteComando c, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(c);
        var apunte = await _repo.ObtenerApunteAsync(apunteId, ct).ConfigureAwait(false);
        if (apunte is null)
        {
            return Resultado.Fallo<ApunteBancarioDto>(Error.NoEncontrado("apunte.no_encontrado", "El apunte no existe."));
        }

        if (apunte.Estado != EstadoApunte.Pendiente)
        {
            return Resultado.Fallo<ApunteBancarioDto>(Error.Conflicto("apunte.conciliado", "El apunte ya está conciliado: deshaz antes su conciliación."));
        }

        var elementos = c.Elementos ?? [];
        if (elementos.Count == 0)
        {
            return Resultado.Fallo<ApunteBancarioDto>(Error.Validacion("apunte.sin_casacion", "Elige al menos un cobro, pago o documento."));
        }

        var casaciones = new List<(Guid, TipoDocumentoTesoreria, Guid, decimal, bool)>();
        var existentes = new List<Movimiento>();
        var nuevos = new List<(TipoDocumentoTesoreria Tipo, Guid Id, SentidoMovimiento Sentido, decimal Importe)>();
        foreach (var el in elementos)
        {
            switch (el.Tipo)
            {
                case "Movimiento":
                    var m = await _movimientos.ObtenerAsync(el.Id, ct).ConfigureAwait(false);
                    if (m is null || m.EmpresaId != empresaId || m.AnulaMovimientoId is not null || await _movimientos.EstaAnuladoAsync(m.Id, ct).ConfigureAwait(false))
                    {
                        return Resultado.Fallo<ApunteBancarioDto>(Error.Validacion("apunte.movimiento", "Uno de los cobros o pagos no existe o está anulado."));
                    }

                    if (m.CuentaBancariaId is { } cb && cb != apunte.CuentaBancariaId)
                    {
                        return Resultado.Fallo<ApunteBancarioDto>(Error.Validacion("apunte.otra_cuenta", "Uno de los cobros o pagos es de otra cuenta bancaria."));
                    }

                    existentes.Add(m);
                    break;

                case "Remesa":
                    var r = await _remesas.ObtenerAsync(el.Id, ct).ConfigureAwait(false);
                    if (r is null || r.Estado != EstadoRemesa.Liquidada)
                    {
                        return Resultado.Fallo<ApunteBancarioDto>(Error.Validacion("apunte.remesa", "La remesa no existe o no está liquidada."));
                    }

                    foreach (var l in r.Lineas.Where(l => l.MovimientoId is not null))
                    {
                        if (await _movimientos.ObtenerAsync(l.MovimientoId!.Value, ct).ConfigureAwait(false) is { } ml)
                        {
                            existentes.Add(ml);
                        }
                    }

                    break;

                case "Factura" or "Gasto" or "Cartera":
                    var tipo = Enum.Parse<TipoDocumentoTesoreria>(el.Tipo);
                    var total = await TotalDocumentoAsync(tipo, el.Id, ct).ConfigureAwait(false);
                    if (total is null)
                    {
                        return Resultado.Fallo<ApunteBancarioDto>(Error.Validacion("apunte.documento", "Uno de los documentos no existe o está anulado."));
                    }

                    var pendiente = Redondeo.Dos(total.Value.Total - await _movimientos.SumaAsync(tipo, el.Id, ct).ConfigureAwait(false));
                    var importe = Redondeo.Dos(el.Importe ?? pendiente);
                    if (importe <= 0m || importe > pendiente)
                    {
                        return Resultado.Fallo<ApunteBancarioDto>(Error.Conflicto("movimiento.sobrepago", $"El importe supera el pendiente del documento ({Redondeo.Formatear(pendiente)} €)."));
                    }

                    nuevos.Add((tipo, el.Id, total.Value.Sentido, importe));
                    break;

                default:
                    return Resultado.Fallo<ApunteBancarioDto>(Error.Validacion("apunte.tipo", $"Tipo de elemento desconocido: {el.Tipo}."));
            }
        }

        if (existentes.Select(m => m.Id).Distinct().Count() != existentes.Count)
        {
            return Resultado.Fallo<ApunteBancarioDto>(Error.Validacion("apunte.repetido", "Un movimiento aparece dos veces."));
        }

        if ((await _repo.ConciliadosAsync(existentes.Select(m => m.Id).ToList(), ct).ConfigureAwait(false)).Count > 0)
        {
            return Resultado.Fallo<ApunteBancarioDto>(Error.Conflicto("apunte.movimiento_conciliado", "Alguno de los cobros o pagos ya está conciliado con otro apunte."));
        }

        casaciones.AddRange(existentes.Select(m => (m.Id, m.TipoDocumento, m.DocumentoId, Signo(m) * m.Importe, false)));
        var sumaPrevista = Redondeo.Dos(casaciones.Sum(x => x.Item4) + nuevos.Sum(n => n.Sentido == SentidoMovimiento.Cobro ? n.Importe : -n.Importe));
        if (sumaPrevista != apunte.Importe)
        {
            return Resultado.Fallo<ApunteBancarioDto>(Error.Validacion("apunte.descuadre",
                $"La suma de lo elegido ({Redondeo.Formatear(sumaPrevista)} €) no coincide con el apunte ({Redondeo.Formatear(apunte.Importe)} €)."));
        }

        foreach (var n in nuevos)
        {
            var registrado = await RegistrarMovimientoAsync(empresaId, apunte, n.Tipo, n.Id, n.Sentido, n.Importe, ct).ConfigureAwait(false);
            if (registrado.EsFallo)
            {
                return Resultado.Fallo<ApunteBancarioDto>(registrado.Error);
            }

            casaciones.Add((registrado.Valor.Id, n.Tipo, n.Id, n.Sentido == SentidoMovimiento.Cobro ? n.Importe : -n.Importe, true));
        }

        var r2 = apunte.Conciliar(casaciones, automatico: false, _reloj);
        if (r2.EsFallo)
        {
            return Resultado.Fallo<ApunteBancarioDto>(r2.Error);
        }

        await GuardarAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(apunte, await DocumentosAsync(empresaId, ct).ConfigureAwait(false)));
    }

    /// <summary>
    /// Contabiliza el apunte con un asiento directo: un cargo va de la cuenta elegida (626 comisiones, 669 otros gastos
    /// financieros…) al banco; un abono, del banco a la cuenta elegida (769 intereses…).
    /// </summary>
    public async Task<Resultado<ApunteBancarioDto>> AsientoAsync(Guid empresaId, Guid apunteId, AsientoApunteComando c, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(c);
        var apunte = await _repo.ObtenerApunteAsync(apunteId, ct).ConfigureAwait(false);
        if (apunte is null)
        {
            return Resultado.Fallo<ApunteBancarioDto>(Error.NoEncontrado("apunte.no_encontrado", "El apunte no existe."));
        }

        var origen = Guid.NewGuid();
        var r = apunte.ContabilizarDirecto(c.Cuenta, c.Concepto, origen, _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<ApunteBancarioDto>(r.Error);
        }

        await EncolarAsientoApunteAsync(empresaId, apunte, origen, anulacion: false, ct).ConfigureAwait(false);
        await GuardarAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(apunte, await DocumentosAsync(empresaId, ct).ConfigureAwait(false)));
    }

    /// <summary>Deshace la conciliación del apunte: anula los cobros o pagos que registró y el asiento directo.</summary>
    public async Task<Resultado<ApunteBancarioDto>> DeshacerAsync(Guid empresaId, Guid apunteId, CancellationToken ct = default)
    {
        var apunte = await _repo.ObtenerApunteAsync(apunteId, ct).ConfigureAwait(false);
        if (apunte is null)
        {
            return Resultado.Fallo<ApunteBancarioDto>(Error.NoEncontrado("apunte.no_encontrado", "El apunte no existe."));
        }

        if (apunte.Estado == EstadoApunte.Pendiente)
        {
            return Resultado.Fallo<ApunteBancarioDto>(Error.Conflicto("apunte.pendiente", "El apunte no está conciliado."));
        }

        var hoy = DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        foreach (var cas in apunte.Casaciones.Where(x => x.MovimientoCreado))
        {
            var m = await _movimientos.ObtenerAsync(cas.MovimientoId, ct).ConfigureAwait(false);
            if (m is null || await _movimientos.EstaAnuladoAsync(m.Id, ct).ConfigureAwait(false))
            {
                continue;
            }

            var anulacion = Movimiento.CrearAnulacion(m, hoy < m.Fecha ? m.Fecha : hoy, _reloj, "Anulación (conciliación deshecha)");
            if (anulacion.EsFallo)
            {
                return Resultado.Fallo<ApunteBancarioDto>(anulacion.Error);
            }

            _movimientos.Agregar(anulacion.Valor);
            if (_contabilizacion is not null)
            {
                await _contabilizacion.EncolarMovimientoAsync(anulacion.Valor, aplicacionAnticipo: false, m, ct: ct).ConfigureAwait(false);
            }
        }

        if (apunte.Estado == EstadoApunte.ConAsiento)
        {
            await EncolarAsientoApunteAsync(empresaId, apunte, Guid.NewGuid(), anulacion: true, ct).ConfigureAwait(false);
        }

        apunte.Deshacer();
        await GuardarAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(apunte, await DocumentosAsync(empresaId, ct).ConfigureAwait(false)));
    }

    // ------------------------------------------------------------------ apoyo

    private async Task EncolarAsientoApunteAsync(Guid empresaId, ApunteBancario apunte, Guid origen, bool anulacion, CancellationToken ct)
    {
        if (_contabilizacion is null)
        {
            return;
        }

        var tesoreria = await _contabilizacion.CuentaTesoreriaAsync(apunte.CuentaBancariaId, null, ct).ConfigureAwait(false);
        _contabilizacion.EncolarAsientoDirecto(empresaId, "ApunteBancario", origen, apunte.Importe > 0 ? SentidoMovimiento.Cobro : SentidoMovimiento.Pago,
            (anulacion ? "Anulación: " : string.Empty) + (apunte.ConceptoAsiento ?? apunte.Concepto), anulacion ? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime) : apunte.Fecha,
            apunte.Importe, tesoreria, apunte.CuentaAsiento ?? "626", anulacion);
    }

    private async Task GuardarAsync(CancellationToken ct)
    {
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        if (_contabilizacion is not null)
        {
            await _contabilizacion.DespacharAsync(ct: ct).ConfigureAwait(false);
        }
    }

    /// <summary>Registra (sin guardar) el cobro o pago de un documento por la cuenta y en la fecha del apunte, con su asiento.</summary>
    private async Task<Resultado<Movimiento>> RegistrarMovimientoAsync(Guid empresaId, ApunteBancario apunte, TipoDocumentoTesoreria tipo, Guid documentoId,
        SentidoMovimiento sentido, decimal importe, CancellationToken ct)
    {
        var m = Movimiento.Crear(empresaId, tipo, documentoId, sentido, importe, apunte.Fecha, MetodoConciliacion, _reloj, apunte.CuentaBancariaId);
        if (m.EsFallo)
        {
            return m;
        }

        _movimientos.Agregar(m.Valor);
        if (_contabilizacion is not null)
        {
            await _contabilizacion.EncolarMovimientoAsync(m.Valor, aplicacionAnticipo: false, ct: ct).ConfigureAwait(false);
        }

        return m;
    }

    private async Task<(decimal Total, SentidoMovimiento Sentido)?> TotalDocumentoAsync(TipoDocumentoTesoreria tipo, Guid id, CancellationToken ct)
    {
        switch (tipo)
        {
            case TipoDocumentoTesoreria.Factura:
                return await _facturas.ObtenerAsync(id, ct).ConfigureAwait(false) is { Estado: not "Anulada" } f ? (f.Total, SentidoMovimiento.Cobro) : null;
            case TipoDocumentoTesoreria.Gasto:
                return await _gastos.ObtenerAsync(id, ct).ConfigureAwait(false) is { Estado: not "Anulado" } g ? (g.Total, SentidoMovimiento.Pago) : null;
            default:
                var e = await _cartera.ObtenerAsync(id, ct).ConfigureAwait(false);
                if (e is null || (await _cartera.AnuladosAsync([id], ct).ConfigureAwait(false)).Count > 0)
                {
                    return null;
                }

                return (e.Importe, e.Sentido == SentidoCartera.Cobro ? SentidoMovimiento.Cobro : SentidoMovimiento.Pago);
        }
    }

    private static int Signo(Movimiento m) => m.Sentido == SentidoMovimiento.Cobro ? 1 : -1;

    /// <summary>Texto del apunte normalizado (mayúsculas sin acentos, solo letras y números) para buscar pistas.</summary>
    private static string Pajar(ApunteBancario a) =>
        Normalizar(string.Join(" ", new[] { a.Concepto, a.Referencia1, a.Referencia2, a.Documento }.Where(x => !string.IsNullOrWhiteSpace(x))));

    private static string Normalizar(string texto)
    {
        var mayus = TextoAeat.Mayusculas(texto);
        var sb = new StringBuilder(mayus.Length + 2).Append(' ');
        foreach (var ch in mayus)
        {
            sb.Append(char.IsAsciiLetterOrDigit(ch) || ch == 'Ñ' ? ch : ' ');
        }

        return sb.Append(' ').ToString();
    }

    /// <summary>
    /// Pistas del concepto: el número del documento (+3) y las palabras del nombre del tercero (+1 cada una, hasta 2).
    /// </summary>
    internal static int Puntuar(string pajar, string numero, string tercero)
    {
        var puntos = 0;
        var compacto = pajar.Replace(" ", string.Empty, StringComparison.Ordinal);
        var num = Normalizar(numero).Replace(" ", string.Empty, StringComparison.Ordinal);
        if (num.Length >= 3 && compacto.Contains(num, StringComparison.Ordinal))
        {
            puntos += 3;
        }

        var palabras = Normalizar(tercero).Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(p => p.Length >= 4).Distinct();
        puntos += Math.Min(2, palabras.Count(p => pajar.Contains(" " + p + " ", StringComparison.Ordinal)));
        return puntos;
    }

    private sealed record DocumentoPendiente(TipoDocumentoTesoreria Tipo, Guid Id, string Numero, string Tercero, DateOnly? Fecha, decimal Pendiente, int Signo);

    private sealed class ContextoConciliacion
    {
        public required IReadOnlyList<Movimiento> Movimientos { get; init; }

        public required IReadOnlyList<Remesa> Remesas { get; init; }

        public required IReadOnlyList<DocumentoPendiente> Pendientes { get; init; }

        public required IReadOnlyDictionary<(TipoDocumentoTesoreria, Guid), (string Numero, string Tercero)> Nombres { get; init; }

        public (string Numero, string Tercero) Documento(TipoDocumentoTesoreria tipo, Guid id) =>
            Nombres.TryGetValue((tipo, id), out var n) ? n : (string.Empty, string.Empty);
    }

    private async Task<ContextoConciliacion> ContextoAsync(Guid empresaId, Guid cuentaBancariaId, DateOnly desde, DateOnly hasta, CancellationToken ct)
    {
        var banco = await _bancos.ObtenerAsync(cuentaBancariaId, ct).ConfigureAwait(false);
        var movimientos = await _repo.MovimientosLibresAsync(cuentaBancariaId, banco?.Predeterminada == true, desde, hasta, ct).ConfigureAwait(false);
        var remesas = (await _remesas.ListarAsync(null, ct).ConfigureAwait(false))
            .Where(r => r.Estado == EstadoRemesa.Liquidada && (r.CuentaBancariaId == cuentaBancariaId || (r.CuentaBancariaId is null && banco?.Predeterminada == true))
                        && r.FechaLiquidacion >= desde && r.FechaLiquidacion <= hasta)
            .ToList();
        var docs = await DocumentosAsync(empresaId, ct).ConfigureAwait(false);
        return new ContextoConciliacion
        {
            Movimientos = movimientos,
            Remesas = remesas,
            Pendientes = docs.Pendientes,
            Nombres = docs.Nombres,
        };
    }

    private sealed record Documentos(List<DocumentoPendiente> Pendientes, IReadOnlyDictionary<(TipoDocumentoTesoreria, Guid), (string Numero, string Tercero)> Nombres);

    private async Task<Documentos> DocumentosAsync(Guid empresaId, CancellationToken ct)
    {
        var nombres = new Dictionary<(TipoDocumentoTesoreria, Guid), (string, string)>();
        var pendientes = new List<DocumentoPendiente>();

        var facturas = await _facturas.ListarAsync(empresaId, ct).ConfigureAwait(false);
        var cobrado = await _consulta.LiquidadoPorDocumentosAsync(TipoDocumentoTesoreria.Factura, facturas.Select(f => f.Id).ToList(), ct).ConfigureAwait(false);
        foreach (var f in facturas)
        {
            nombres[(TipoDocumentoTesoreria.Factura, f.Id)] = (f.NumeroCompleto, f.ClienteNombre);
            var pendiente = Redondeo.Dos(f.Total - cobrado.GetValueOrDefault(f.Id));
            if (f.Estado == "Emitida" && pendiente > 0m)
            {
                pendientes.Add(new DocumentoPendiente(TipoDocumentoTesoreria.Factura, f.Id, f.NumeroCompleto, f.ClienteNombre, f.FechaVencimiento, pendiente, 1));
            }
        }

        var gastos = await _gastos.ListarAsync(empresaId, ct).ConfigureAwait(false);
        var pagado = await _consulta.LiquidadoPorDocumentosAsync(TipoDocumentoTesoreria.Gasto, gastos.Select(g => g.Id).ToList(), ct).ConfigureAwait(false);
        foreach (var g in gastos)
        {
            nombres[(TipoDocumentoTesoreria.Gasto, g.Id)] = (g.Concepto, g.ProveedorTexto ?? string.Empty);
            var pendiente = Redondeo.Dos(g.Total - pagado.GetValueOrDefault(g.Id));
            if (g.Estado != "Anulado" && pendiente > 0m)
            {
                pendientes.Add(new DocumentoPendiente(TipoDocumentoTesoreria.Gasto, g.Id, g.Concepto, g.ProveedorTexto ?? string.Empty, g.Fecha, pendiente, -1));
            }
        }

        var efectos = await _cartera.ListarAsync(empresaId, null, ct).ConfigureAwait(false);
        var ids = efectos.Select(e => e.Id).ToList();
        var liquidado = await _consulta.LiquidadoPorDocumentosAsync(TipoDocumentoTesoreria.Cartera, ids, ct).ConfigureAwait(false);
        var anulados = await _cartera.AnuladosAsync(ids, ct).ConfigureAwait(false);
        foreach (var e in efectos)
        {
            nombres[(TipoDocumentoTesoreria.Cartera, e.Id)] = (e.Documento, e.TerceroNombre);
            var pendiente = Redondeo.Dos(e.Importe - liquidado.GetValueOrDefault(e.Id));
            if (!anulados.Contains(e.Id) && pendiente > 0m)
            {
                pendientes.Add(new DocumentoPendiente(TipoDocumentoTesoreria.Cartera, e.Id, e.Documento, e.TerceroNombre, e.Vencimiento, pendiente,
                    e.Sentido == SentidoCartera.Cobro ? 1 : -1));
            }
        }

        return new Documentos(pendientes, nombres);
    }

    private static ExtractoDto Dto(ExtractoImportado e, IReadOnlyDictionary<Guid, string> bancos, int pendientes) => new(
        e.Id, e.CuentaBancariaId, bancos.GetValueOrDefault(e.CuentaBancariaId), e.CuentaFichero, e.Desde, e.Hasta, e.SaldoInicial, e.SaldoFinal, e.NombreArchivo,
        e.NumeroApuntes, pendientes, e.CreadoEn);

    private static ApunteBancarioDto Dto(ApunteBancario a, Documentos docs) => new(
        a.Id, a.Orden, a.Fecha, a.FechaValor, a.Importe, a.Concepto, a.ConceptoComun, a.Referencia1, a.Referencia2, a.Estado.ToString(), a.CuentaAsiento,
        a.ConceptoAsiento,
        a.Casaciones.Select(c => new CasacionDto(c.MovimientoId, c.TipoDocumento.ToString(), c.DocumentoId,
            docs.Nombres.TryGetValue((c.TipoDocumento, c.DocumentoId), out var n) ? n.Numero : string.Empty, c.Importe, c.MovimientoCreado)).ToList());
}
