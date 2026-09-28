using AlxorCore.Contabilidad.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Contabilidad.Aplicacion;

// --------------------------------------------------------------------------------------------
//  DTOs
// --------------------------------------------------------------------------------------------

public sealed record CuentaDto(Guid Id, string Codigo, string Nombre, int Grupo)
{
    public static CuentaDto Desde(Cuenta c) => new(c.Id, c.Codigo, c.Nombre, c.Grupo);
}

public sealed record ApunteDto(string CuentaCodigo, string? Concepto, decimal Debe, decimal Haber, Guid Id = default);

public sealed record AsientoDto(Guid Id, int Ejercicio, int Numero, DateOnly Fecha, string Concepto,
    string Origen, decimal Total, IReadOnlyList<ApunteDto> Apuntes, Guid? AnulaAsientoId = null, Guid? AnuladoPorId = null, string? Diario = null, int NumeroDiario = 0)
{
    public static AsientoDto Desde(Asiento a) => Desde(a, null);

    public static AsientoDto Desde(Asiento a, Guid? anuladoPorId) => new(a.Id, a.Ejercicio, a.Numero, a.Fecha, a.Concepto,
        a.Origen, a.TotalDebe, a.Apuntes.Select(p => new ApunteDto(p.CuentaCodigo, p.Concepto, p.Debe, p.Haber, p.Id)).ToList(),
        a.AnulaAsientoId, anuladoPorId, a.Diario);
}

/// <summary>Línea del libro mayor de una cuenta.</summary>
public sealed record LineaMayorDto(DateOnly Fecha, int Numero, string Concepto, decimal Debe, decimal Haber, decimal SaldoAcumulado);

/// <summary>Fila del balance de sumas y saldos.</summary>
public sealed record SaldoCuentaDto(string CuentaCodigo, string CuentaNombre, decimal SumaDebe, decimal SumaHaber, decimal SaldoDeudor, decimal SaldoAcreedor);

/// <summary>Apunte de un asiento generado por un documento (factura, gasto…), identificado por su origen.</summary>
public sealed record ApunteOrigen(Guid OrigenId, string CuentaCodigo, decimal Debe, decimal Haber);

/// <summary>Saldo agregado de una cuenta (suma de debe/haber), calculado en la base de datos.</summary>
public sealed record SaldoCuentaAgregado(string CuentaCodigo, decimal Debe, decimal Haber);

// --------------------------------------------------------------------------------------------
//  Puertos
// --------------------------------------------------------------------------------------------

public interface IRepositorioCuentas
{
    Task<IReadOnlyList<CuentaDto>> ListarAsync(Guid empresaId, CancellationToken ct = default);

    void Agregar(Cuenta cuenta);

    Task<IReadOnlySet<string>> CodigosExistentesAsync(Guid empresaId, CancellationToken ct = default);

    /// <summary>Subcuenta individual enlazada a un tercero (con seguimiento para poder mutarla). Null si no tiene.</summary>
    Task<Cuenta?> ObtenerPorTerceroAsync(Guid empresaId, Guid terceroId, CancellationToken ct = default);

    /// <summary>Cuenta por su código (con seguimiento). Null si no existe.</summary>
    Task<Cuenta?> ObtenerPorCodigoAsync(Guid empresaId, string codigo, CancellationToken ct = default);

    /// <summary>Subcuentas individuales de un tipo de tercero: tercero_id → código de cuenta.</summary>
    Task<IReadOnlyDictionary<Guid, string>> SubcuentasPorTipoAsync(Guid empresaId, string tipoTercero, CancellationToken ct = default);
}

public interface IRepositorioAsientos
{
    void Agregar(Asiento asiento);

    Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, CancellationToken ct = default);

    Task<IReadOnlyList<AsientoDto>> DiarioAsync(Guid empresaId, int ejercicio, CancellationToken ct = default);

    Task<Asiento?> ObtenerAsync(Guid id, CancellationToken ct = default);

    /// <summary>¿Hay ya un contraasiento que lo anule?</summary>
    Task<bool> EstaAnuladoAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<AsientoDto>> AsientosDeCuentaAsync(Guid empresaId, int ejercicio, string cuentaCodigo, CancellationToken ct = default);

    Task<IReadOnlyList<AsientoDto>> TodosAsync(Guid empresaId, int ejercicio, CancellationToken ct = default);

    /// <summary>
    /// Saldo (debe/haber) agregado por cuenta del ejercicio, calculado con <c>GROUP BY</c> en la base de
    /// datos: la memoria depende del número de cuentas, no del número de apuntes (escala a grandes volúmenes).
    /// </summary>
    Task<IReadOnlyList<SaldoCuentaAgregado>> SaldosAgregadosAsync(Guid empresaId, int ejercicio, CancellationToken ct = default);

    /// <summary>Apuntes de los asientos contabilizados de unos documentos de origen (incluidas sus anulaciones).</summary>
    Task<IReadOnlyList<ApunteOrigen>> ApuntesDeOrigenesAsync(Guid empresaId, IReadOnlyCollection<Guid> origenIds, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<ApunteOrigen>>([]);

    /// <summary>¿El ejercicio tiene algún asiento de cierre? (para saber si está cerrado sin cargarlos todos).</summary>
    Task<bool> TieneCierreAsync(Guid empresaId, int ejercicio, CancellationToken ct = default);

    /// <summary>Último día de los meses cerrados de la empresa (null: ninguno).</summary>
    Task<DateOnly?> CerradoHastaAsync(Guid empresaId, CancellationToken ct = default) => Task.FromResult<DateOnly?>(null);

    /// <summary>
    /// Suma de debe y haber de unas cuentas en todos los asientos con fecha hasta la indicada (todos los ejercicios:
    /// el cierre y la apertura se compensan). La usa Tesorería para el saldo de cada banco.
    /// </summary>
    Task<IReadOnlyList<SaldoCuentaAgregado>> SaldosHastaAsync(Guid empresaId, IReadOnlyCollection<string> codigos, DateOnly hasta, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<SaldoCuentaAgregado>>([]);
}

public interface IRepositorioConfigContabilidad
{
    Task<ConfiguracionContabilidad?> ObtenerAsync(Guid empresaId, CancellationToken ct = default);

    void Agregar(ConfiguracionContabilidad config);
}

public interface IUnidadDeTrabajoContabilidad : IUnidadDeTrabajo;

// --------------------------------------------------------------------------------------------
//  Plan contable básico (subconjunto común del PGC)
// --------------------------------------------------------------------------------------------

/// <summary>Cuentas por defecto y subconjunto del PGC que se siembra por empresa.</summary>
public static class PlanBasico
{
    public const string CuentaCompras = "629";      // Otros servicios (gasto genérico de proveedor)
    public const string CuentaIvaSoportado = "472"; // H.P. IVA soportado
    public const string CuentaRetencion = "4751";   // H.P. acreedora por retenciones (compras)
    public const string CuentaProveedores = "400";  // Proveedores
    public const string CuentaVentas = "705";        // Prestaciones de servicios (ingreso genérico)
    public const string CuentaClientes = "430";      // Clientes
    public const string CuentaAnticiposClientes = "438"; // Anticipos de clientes
    public const string CuentaAnticiposProveedores = "407"; // Anticipos a proveedores (entregas a cuenta)
    public const string CuentaPartidasPendientes = "555"; // Partidas pendientes de aplicación (puente de compensaciones)
    public const string CuentaBancos = "572";        // Bancos
    public const string CuentaCaja = "570";          // Caja
    public const string CuentaServiciosBancarios = "626"; // Servicios bancarios y similares (comisiones, gastos de devolución)
    public const string CuentaIvaRepercutido = "477"; // H.P. IVA repercutido
    public const string CuentaRetencionVenta = "473"; // H.P. retenciones y pagos a cuenta (ventas)
    public const string CuentaTrabajadores = "465";   // Remuneraciones pendientes de pago (raíz de trabajadores)

    /// <summary>Cuenta de resultado del ejercicio (regularización de gastos e ingresos en el cierre).</summary>
    public const string CuentaResultado = "129";

    public static readonly IReadOnlyList<(string Codigo, string Nombre)> Cuentas = new[]
    {
        ("100", "Capital social"),
        ("113", "Reservas voluntarias"),
        ("129", "Resultado del ejercicio"),
        ("430", "Clientes"),
        ("438", "Anticipos de clientes"),
        ("407", "Anticipos a proveedores"),
        ("555", "Partidas pendientes de aplicación"),
        ("4310", "Efectos comerciales en cartera"),
        ("4311", "Efectos comerciales descontados"),
        ("4315", "Efectos comerciales impagados"),
        ("436", "Clientes de dudoso cobro"),
        ("490", "Deterioro de valor de créditos por operaciones comerciales"),
        ("694", "Pérdidas por deterioro de créditos por operaciones comerciales"),
        ("794", "Reversión del deterioro de créditos por operaciones comerciales"),
        ("650", "Pérdidas de créditos comerciales incobrables"),
        ("5208", "Deudas por efectos descontados"),
        ("665", "Intereses por descuento de efectos"),
        ("400", "Proveedores"),
        ("401", "Proveedores, efectos comerciales a pagar"),
        ("410", "Acreedores por prestaciones de servicios"),
        ("472", "H.P. IVA soportado"),
        ("473", "H.P. retenciones y pagos a cuenta"),
        ("477", "H.P. IVA repercutido"),
        ("475", "H.P. acreedora por conceptos fiscales"),
        ("4751", "H.P. acreedora por retenciones practicadas"),
        ("465", "Remuneraciones pendientes de pago"),
        ("570", "Caja"),
        ("572", "Bancos"),
        ("626", "Servicios bancarios y similares"),
        ("662", "Intereses de deudas"),
        ("669", "Otros gastos financieros"),
        ("769", "Otros ingresos financieros"),
        ("600", "Compras de mercaderías"),
        ("621", "Arrendamientos y cánones"),
        ("622", "Reparaciones y conservación"),
        ("623", "Servicios de profesionales independientes"),
        ("628", "Suministros"),
        ("629", "Otros servicios"),
        ("700", "Ventas de mercaderías"),
        ("705", "Prestaciones de servicios"),
        ("752", "Ingresos por arrendamientos"),

        // Inmovilizado material (activo no corriente) y sus cuentas asociadas.
        ("211", "Construcciones"),
        ("213", "Maquinaria"),
        ("216", "Mobiliario"),
        ("217", "Equipos para procesos de información"),
        ("218", "Elementos de transporte"),
        ("280", "Amortización acumulada del inmovilizado intangible"),
        ("281", "Amortización acumulada del inmovilizado material"),
        ("680", "Amortización del inmovilizado intangible"),
        ("681", "Amortización del inmovilizado material"),
        ("671", "Pérdidas procedentes del inmovilizado material"),
        ("771", "Beneficios procedentes del inmovilizado material"),

        // Impuesto sobre beneficios diferido (diferencias temporarias).
        ("4740", "Activos por diferencias temporarias deducibles"),
        ("479", "Pasivos por diferencias temporarias imponibles"),
        ("6301", "Impuesto diferido"),

        // Periodificaciones.
        ("480", "Gastos anticipados"),
        ("485", "Ingresos anticipados"),
    };
}

// --------------------------------------------------------------------------------------------
//  Casos de uso: plan de cuentas y modo
// --------------------------------------------------------------------------------------------

/// <summary>Lista el plan de cuentas de la empresa; lo siembra si aún no existe.</summary>
public sealed class ListarCuentas
{
    private readonly IRepositorioCuentas _cuentas;
    private readonly IUnidadDeTrabajoContabilidad _unidad;

    public ListarCuentas(IRepositorioCuentas cuentas, IUnidadDeTrabajoContabilidad unidad)
    {
        _cuentas = cuentas;
        _unidad = unidad;
    }

    public async Task<IReadOnlyList<CuentaDto>> EjecutarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var creadas = await SembradorPlan.AsegurarAsync(empresaId, _cuentas, ct).ConfigureAwait(false);
        if (creadas > 0)
        {
            await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        }

        return await _cuentas.ListarAsync(empresaId, ct).ConfigureAwait(false);
    }
}

/// <summary>Datos para dar de alta (o renombrar) una cuenta del plan.</summary>
public sealed record DatosCuenta(string Codigo, string Nombre);

/// <summary>
/// Alta y renombrado de cuentas del plan (el plan básico se amplía con las que necesite la empresa).
/// El código no se cambia: los apuntes lo referencian.
/// </summary>
public sealed class GestionCuentas
{
    private readonly IRepositorioCuentas _cuentas;
    private readonly IUnidadDeTrabajoContabilidad _unidad;

    public GestionCuentas(IRepositorioCuentas cuentas, IUnidadDeTrabajoContabilidad unidad)
    {
        _cuentas = cuentas;
        _unidad = unidad;
    }

    public async Task<Resultado<CuentaDto>> CrearAsync(Guid empresaId, DatosCuenta datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var codigo = (datos.Codigo ?? string.Empty).Trim();
        if ((datos.Nombre ?? string.Empty).Trim().Length > Cuenta.LongitudMaximaNombre)
        {
            return Resultado.Fallo<CuentaDto>(Error.Validacion("cuenta.nombre_largo", $"El nombre supera {Cuenta.LongitudMaximaNombre} caracteres."));
        }

        if (await _cuentas.ObtenerPorCodigoAsync(empresaId, codigo, ct).ConfigureAwait(false) is not null)
        {
            return Resultado.Fallo<CuentaDto>(Error.Conflicto("cuenta.duplicada", $"La cuenta {codigo} ya existe en el plan."));
        }

        var cuenta = Cuenta.Crear(empresaId, codigo, datos.Nombre);
        if (cuenta.EsFallo)
        {
            return Resultado.Fallo<CuentaDto>(cuenta.Error);
        }

        _cuentas.Agregar(cuenta.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(CuentaDto.Desde(cuenta.Valor));
    }

    public async Task<Resultado<CuentaDto>> RenombrarAsync(Guid empresaId, string codigo, string? nombre, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return Resultado.Fallo<CuentaDto>(Error.Validacion("cuenta.nombre_obligatorio", "El nombre de la cuenta es obligatorio."));
        }

        if (nombre.Trim().Length > Cuenta.LongitudMaximaNombre)
        {
            return Resultado.Fallo<CuentaDto>(Error.Validacion("cuenta.nombre_largo", $"El nombre supera {Cuenta.LongitudMaximaNombre} caracteres."));
        }

        var cuenta = await _cuentas.ObtenerPorCodigoAsync(empresaId, (codigo ?? string.Empty).Trim(), ct).ConfigureAwait(false);
        if (cuenta is null)
        {
            return Resultado.Fallo<CuentaDto>(Error.NoEncontrado("cuenta.no_encontrada", $"La cuenta {codigo} no existe."));
        }

        cuenta.Renombrar(nombre);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(CuentaDto.Desde(cuenta));
    }
}

/// <summary>Siembra el plan básico si faltan cuentas.</summary>
internal static class SembradorPlan
{
    public static async Task<int> AsegurarAsync(Guid empresaId, IRepositorioCuentas cuentas, CancellationToken ct)
    {
        var existentes = await cuentas.CodigosExistentesAsync(empresaId, ct).ConfigureAwait(false);
        var creadas = 0;
        foreach (var (codigo, nombre) in PlanBasico.Cuentas)
        {
            if (existentes.Contains(codigo))
            {
                continue;
            }

            var cuenta = Cuenta.Crear(empresaId, codigo, nombre);
            if (cuenta.EsCorrecto)
            {
                cuentas.Agregar(cuenta.Valor);
                creadas++;
            }
        }

        return creadas;
    }
}

/// <summary>Obtiene el modo de contabilidad de la empresa (Simple por defecto).</summary>
public sealed class ObtenerModoContabilidad
{
    private readonly IRepositorioConfigContabilidad _config;

    public ObtenerModoContabilidad(IRepositorioConfigContabilidad config) => _config = config;

    public async Task<ModoContabilidad> EjecutarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var config = await _config.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        return config?.Modo ?? ModoContabilidad.Simple;
    }
}

/// <summary>Cambia el modo de contabilidad de la empresa.</summary>
public sealed class CambiarModoContabilidad
{
    private readonly IRepositorioConfigContabilidad _config;
    private readonly IUnidadDeTrabajoContabilidad _unidad;

    public CambiarModoContabilidad(IRepositorioConfigContabilidad config, IUnidadDeTrabajoContabilidad unidad)
    {
        _config = config;
        _unidad = unidad;
    }

    public async Task<ModoContabilidad> EjecutarAsync(Guid empresaId, ModoContabilidad modo, CancellationToken ct = default)
    {
        var config = await _config.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        if (config is null)
        {
            _config.Agregar(new ConfiguracionContabilidad(empresaId, modo));
        }
        else
        {
            config.CambiarModo(modo);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return modo;
    }
}

// --------------------------------------------------------------------------------------------
//  Casos de uso: asientos y libros
// --------------------------------------------------------------------------------------------

/// <summary>Datos de una línea al crear un asiento manual.</summary>
/// <remarks>
/// En los gastos (6) e ingresos (7) se puede indicar el centro y la partida analíticos; si no, se
/// aplican las reglas analíticas.
/// </remarks>
public sealed record LineaAsientoComando(string CuentaCodigo, decimal Debe, decimal Haber, string? Concepto = null, Guid? CentroId = null, Guid? PartidaId = null);

/// <summary>Crea un asiento manual.</summary>
public sealed record CrearAsientoComando(DateOnly Fecha, string Concepto, IReadOnlyList<LineaAsientoComando> Lineas, string? Diario = null);

/// <summary>
/// Anula un asiento <b>manual</b> con su contraasiento (los asientos no se borran ni se modifican). Los asientos que
/// genera un documento (factura, gasto, cobro…) se anulan anulando el documento, para no descuadrar el documento y
/// su contabilidad.
/// </summary>
public sealed class AnularAsiento
{
    private readonly IRepositorioAsientos _asientos;
    private readonly IUnidadDeTrabajoContabilidad _unidad;
    private readonly IReloj _reloj;

    public AnularAsiento(IRepositorioAsientos asientos, IUnidadDeTrabajoContabilidad unidad, IReloj reloj)
    {
        _asientos = asientos;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<Resultado<AsientoDto>> EjecutarAsync(Guid empresaId, Guid asientoId, DateOnly? fecha, CancellationToken ct = default)
    {
        var original = await _asientos.ObtenerAsync(asientoId, ct).ConfigureAwait(false);
        if (original is null || original.EmpresaId != empresaId)
        {
            return Resultado.Fallo<AsientoDto>(Error.NoEncontrado("asiento.no_encontrado", "El asiento no existe."));
        }

        if (original.Origen == Asiento.OrigenAnulacion)
        {
            return Resultado.Fallo<AsientoDto>(Error.Conflicto("asiento.es_anulacion", "Este asiento ya es una anulación; para deshacerla, registra un asiento nuevo."));
        }

        if (original.Origen != Asiento.OrigenManual)
        {
            return Resultado.Fallo<AsientoDto>(Error.Conflicto("asiento.de_documento",
                $"Este asiento lo generó un documento ({original.Origen}): anúlalo desde el documento para que ambos queden coherentes."));
        }

        if (await _asientos.EstaAnuladoAsync(asientoId, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<AsientoDto>(Error.Conflicto("asiento.ya_anulado", "El asiento ya está anulado."));
        }

        var dia = fecha ?? original.Fecha;
        if (dia < original.Fecha)
        {
            return Resultado.Fallo<AsientoDto>(Error.Validacion("asiento.fecha_anulacion", "La anulación no puede ser anterior al asiento que anula."));
        }

        if (await _asientos.TieneCierreAsync(empresaId, dia.Year, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<AsientoDto>(Error.Conflicto("asiento.ejercicio_cerrado", $"El ejercicio {dia.Year} está cerrado; indica una fecha de un ejercicio abierto."));
        }

        if (await PeriodosContables.ComprobarAsync(_asientos, empresaId, dia, ct).ConfigureAwait(false) is { } cerrado)
        {
            return Resultado.Fallo<AsientoDto>(cerrado);
        }

        var numero = await _asientos.SiguienteNumeroAsync(empresaId, dia.Year, ct).ConfigureAwait(false);
        var contra = Asiento.CrearAnulacion(original, numero, dia, _reloj);
        if (contra.EsFallo)
        {
            return Resultado.Fallo<AsientoDto>(contra.Error);
        }

        _asientos.Agregar(contra.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(AsientoDto.Desde(contra.Valor));
    }
}

public sealed class CrearAsiento
{
    private readonly IRepositorioAsientos _asientos;
    private readonly IUnidadDeTrabajoContabilidad _unidad;
    private readonly IReloj _reloj;

    private readonly IRepositorioAnalitica? _analitica;
    private readonly ImputadorAnalitico? _imputador;
    private readonly IRepositorioDiarios? _diarios;
    private readonly IRepositorioCuentas? _cuentas;

    public CrearAsiento(IRepositorioAsientos asientos, IUnidadDeTrabajoContabilidad unidad, IReloj reloj,
        IRepositorioAnalitica? analitica = null, ImputadorAnalitico? imputador = null, IRepositorioDiarios? diarios = null,
        IRepositorioCuentas? cuentas = null)
    {
        _cuentas = cuentas;
        _diarios = diarios;
        _asientos = asientos;
        _unidad = unidad;
        _reloj = reloj;
        _analitica = analitica;
        _imputador = imputador;
    }

    /// <summary>
    /// Asiento manual tecleado por el usuario: además de las reglas generales, cada cuenta tiene que
    /// existir en el plan de la empresa (evita saldos en cuentas mal tecleadas). Las contabilizaciones
    /// automáticas y las importaciones usan <see cref="EjecutarAsync"/> sin esta comprobación.
    /// </summary>
    public async Task<Resultado<AsientoDto>> EjecutarManualAsync(Guid empresaId, CrearAsientoComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        if (_cuentas is not null)
        {
            await SembradorPlan.AsegurarAsync(empresaId, _cuentas, ct).ConfigureAwait(false);
            var existentes = await _cuentas.CodigosExistentesAsync(empresaId, ct).ConfigureAwait(false);
            var plan = existentes.Concat(PlanBasico.Cuentas.Select(c => c.Codigo)).ToHashSet(StringComparer.Ordinal);
            var desconocidas = (comando.Lineas ?? Array.Empty<LineaAsientoComando>())
                .Select(l => (l.CuentaCodigo ?? string.Empty).Trim())
                .Where(c => !plan.Contains(c)).Distinct(StringComparer.Ordinal).ToList();
            if (desconocidas.Count > 0)
            {
                return Resultado.Fallo<AsientoDto>(Error.Validacion("asiento.cuenta_inexistente",
                    $"La cuenta {string.Join(", ", desconocidas.Select(c => c.Length == 0 ? "(vacía)" : c))} no existe en el plan de cuentas."));
            }
        }

        return await EjecutarAsync(empresaId, comando, ct).ConfigureAwait(false);
    }

    public async Task<Resultado<AsientoDto>> EjecutarAsync(Guid empresaId, CrearAsientoComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var ejercicio = comando.Fecha.Year;

        // No se pueden añadir asientos manuales a un ejercicio ya cerrado (comprobación por EXISTS).
        if (await _asientos.TieneCierreAsync(empresaId, ejercicio, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<AsientoDto>(Error.Conflicto("asiento.ejercicio_cerrado", $"El ejercicio {ejercicio} está cerrado; no admite nuevos asientos."));
        }

        if (await PeriodosContables.ComprobarAsync(_asientos, empresaId, comando.Fecha, ct).ConfigureAwait(false) is { } cerrado)
        {
            return Resultado.Fallo<AsientoDto>(cerrado);
        }

        if (!string.IsNullOrWhiteSpace(comando.Diario) && _diarios is not null
            && !await _diarios.ExisteActivoAsync(empresaId, comando.Diario.Trim().ToUpperInvariant(), ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<AsientoDto>(Error.Validacion("asiento.diario", $"El diario «{comando.Diario}» no existe o está de baja."));
        }

        var numero = await _asientos.SiguienteNumeroAsync(empresaId, ejercicio, ct).ConfigureAwait(false);
        var lineas = (comando.Lineas ?? Array.Empty<LineaAsientoComando>())
            .Select(l => new LineaAsiento(l.CuentaCodigo, l.Debe, l.Haber, l.Concepto)).ToList();

        var asiento = Asiento.Crear(empresaId, ejercicio, numero, comando.Fecha, comando.Concepto, "Manual", lineas, _reloj, comando.Diario);
        if (asiento.EsFallo)
        {
            return Resultado.Fallo<AsientoDto>(asiento.Error);
        }

        _asientos.Agregar(asiento.Valor);
        var analitica = await ImputarAsync(empresaId, asiento.Valor, comando.Lineas ?? [], ct).ConfigureAwait(false);
        if (analitica is not null)
        {
            return Resultado.Fallo<AsientoDto>(analitica);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(AsientoDto.Desde(asiento.Valor));
    }

    /// <summary>Imputa en analítica: al centro/partida de la línea si se indican; si no, con las reglas.</summary>
    private async Task<Error?> ImputarAsync(Guid empresaId, Asiento asiento, IReadOnlyList<LineaAsientoComando> lineas, CancellationToken ct)
    {
        if (_analitica is null || _imputador is null)
        {
            return null;
        }

        var apuntes = ImputadorAnalitico.ApuntesDe(asiento, null, null, null).ToDictionary(a => a.ApunteId);
        var porReglas = new List<ApunteAnalitico>();
        for (var i = 0; i < lineas.Count && i < asiento.Apuntes.Count; i++)
        {
            var linea = lineas[i];
            if (!apuntes.TryGetValue(asiento.Apuntes[i].Id, out var apunte))
            {
                if (linea.CentroId is not null)
                {
                    return Error.Validacion("asiento.analitica_cuenta", $"Solo los gastos (6) y los ingresos (7) llevan centro analítico (la cuenta {linea.CuentaCodigo} no).");
                }

                continue;
            }

            if (linea.CentroId is not { } centroId)
            {
                porReglas.Add(apunte);
                continue;
            }

            var centro = await _analitica.CentroAsync(centroId, ct).ConfigureAwait(false);
            if (centro is not { Activo: true })
            {
                return Error.Validacion("asiento.analitica_centro", "El centro analítico no existe o está inactivo.");
            }

            if (linea.PartidaId is { } p && await _analitica.PartidaAsync(p, ct).ConfigureAwait(false) is not { } partida)
            {
                return Error.Validacion("asiento.analitica_partida", "La partida analítica no existe.");
            }

            _analitica.Agregar(ImputacionAnalitica.DeApunte(empresaId, apunte.ApunteId, asiento.Id, asiento.Fecha, apunte.CuentaCodigo,
                centroId, linea.PartidaId, apunte.Importe, OrigenImputacion.Manual));
        }

        await _imputador.ImputarAsync(empresaId, porReglas, OrigenImputacion.Regla, null, ct).ConfigureAwait(false);
        return null;
    }
}

/// <summary>Libro diario: todos los asientos de un ejercicio, en orden.</summary>
public sealed class ListarDiario
{
    private readonly IRepositorioAsientos _asientos;

    public ListarDiario(IRepositorioAsientos asientos) => _asientos = asientos;

    public Task<IReadOnlyList<AsientoDto>> EjecutarAsync(Guid empresaId, int ejercicio, CancellationToken ct = default) =>
        _asientos.DiarioAsync(empresaId, ejercicio, ct);

    /// <summary>Los asientos de un diario (serie), en su orden.</summary>
    public async Task<IReadOnlyList<AsientoDto>> EjecutarAsync(Guid empresaId, int ejercicio, string? diario, CancellationToken ct = default)
    {
        var todos = await _asientos.DiarioAsync(empresaId, ejercicio, ct).ConfigureAwait(false);
        return string.IsNullOrWhiteSpace(diario) ? todos
            : todos.Where(a => string.Equals(a.Diario, diario.Trim(), StringComparison.OrdinalIgnoreCase)).OrderBy(a => a.NumeroDiario).ToList();
    }
}

/// <summary>Libro mayor de una cuenta: sus movimientos con saldo acumulado.</summary>
public sealed class MayorCuenta
{
    private readonly IRepositorioAsientos _asientos;

    public MayorCuenta(IRepositorioAsientos asientos) => _asientos = asientos;

    public async Task<IReadOnlyList<LineaMayorDto>> EjecutarAsync(Guid empresaId, int ejercicio, string cuentaCodigo, CancellationToken ct = default)
    {
        var asientos = await _asientos.AsientosDeCuentaAsync(empresaId, ejercicio, cuentaCodigo, ct).ConfigureAwait(false);
        var lineas = new List<LineaMayorDto>();
        var saldo = 0m;
        foreach (var a in asientos.OrderBy(x => x.Fecha).ThenBy(x => x.Numero))
        {
            foreach (var ap in a.Apuntes.Where(p => p.CuentaCodigo == cuentaCodigo))
            {
                saldo = Redondeo.Dos(saldo + ap.Debe - ap.Haber);
                lineas.Add(new LineaMayorDto(a.Fecha, a.Numero, a.Concepto, ap.Debe, ap.Haber, saldo));
            }
        }

        return lineas;
    }
}

/// <summary>Balance de sumas y saldos del ejercicio.</summary>
public sealed class BalanceSumasYSaldos
{
    private readonly IRepositorioAsientos _asientos;
    private readonly IRepositorioCuentas _cuentas;

    public BalanceSumasYSaldos(IRepositorioAsientos asientos, IRepositorioCuentas cuentas)
    {
        _asientos = asientos;
        _cuentas = cuentas;
    }

    public async Task<IReadOnlyList<SaldoCuentaDto>> EjecutarAsync(Guid empresaId, int ejercicio, CancellationToken ct = default)
    {
        // La suma por cuenta la calcula la base de datos (GROUP BY); no se cargan los apuntes en memoria.
        var agregados = await _asientos.SaldosAgregadosAsync(empresaId, ejercicio, ct).ConfigureAwait(false);
        var cuentas = await _cuentas.ListarAsync(empresaId, ct).ConfigureAwait(false);
        var nombres = cuentas.ToDictionary(c => c.Codigo, c => c.Nombre, StringComparer.Ordinal);

        return agregados.OrderBy(s => s.CuentaCodigo, StringComparer.Ordinal).Select(s =>
        {
            var debe = Redondeo.Dos(s.Debe);
            var haber = Redondeo.Dos(s.Haber);
            var saldo = Redondeo.Dos(debe - haber);
            return new SaldoCuentaDto(s.CuentaCodigo, nombres.GetValueOrDefault(s.CuentaCodigo, "—"), debe, haber,
                saldo > 0m ? saldo : 0m, saldo < 0m ? -saldo : 0m);
        }).ToList();
    }
}

/// <summary>
/// Genera el asiento de una factura de proveedor por partida doble:
/// (Debe) gasto 6xx + IVA soportado 472 / (Haber) retención 4751 + proveedores 400.
/// </summary>
public sealed class GenerarAsientoCompra
{
    private readonly IRepositorioAsientos _asientos;
    private readonly IRepositorioCuentas _cuentas;
    private readonly IUnidadDeTrabajoContabilidad _unidad;
    private readonly IReloj _reloj;

    public GenerarAsientoCompra(IRepositorioAsientos asientos, IRepositorioCuentas cuentas, IUnidadDeTrabajoContabilidad unidad, IReloj reloj)
    {
        _asientos = asientos;
        _cuentas = cuentas;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<Resultado<AsientoDto>> EjecutarAsync(Guid empresaId, string concepto, DateOnly fecha,
        decimal baseImponible, string codigoIva, decimal porcentajeIrpf, CancellationToken ct = default)
    {
        var impuesto = Impuesto.PorCodigoImpuesto(codigoIva);
        if (impuesto.EsFallo)
        {
            return Resultado.Fallo<AsientoDto>(impuesto.Error);
        }

        var cuotaIva = Redondeo.Dos(baseImponible * impuesto.Valor.Porcentaje / 100m);
        var retencion = Redondeo.Dos(baseImponible * porcentajeIrpf / 100m);
        var totalProveedor = Redondeo.Dos(baseImponible + cuotaIva - retencion);

        var lineas = new List<LineaAsiento>
        {
            new(PlanBasico.CuentaCompras, baseImponible, 0m, concepto),
        };
        if (cuotaIva > 0m)
        {
            lineas.Add(new LineaAsiento(PlanBasico.CuentaIvaSoportado, cuotaIva, 0m, "IVA soportado"));
        }

        if (retencion > 0m)
        {
            lineas.Add(new LineaAsiento(PlanBasico.CuentaRetencion, 0m, retencion, "Retención IRPF"));
        }

        lineas.Add(new LineaAsiento(PlanBasico.CuentaProveedores, 0m, totalProveedor, concepto));

        if (await PeriodosContables.ComprobarAsync(_asientos, empresaId, fecha, ct).ConfigureAwait(false) is { } cerrado)
        {
            return Resultado.Fallo<AsientoDto>(cerrado);
        }

        await SembradorPlan.AsegurarAsync(empresaId, _cuentas, ct).ConfigureAwait(false);
        var ejercicio = fecha.Year;
        var numero = await _asientos.SiguienteNumeroAsync(empresaId, ejercicio, ct).ConfigureAwait(false);
        var asiento = Asiento.Crear(empresaId, ejercicio, numero, fecha, concepto, "Compra", lineas, _reloj);
        if (asiento.EsFallo)
        {
            return Resultado.Fallo<AsientoDto>(asiento.Error);
        }

        _asientos.Agregar(asiento.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(AsientoDto.Desde(asiento.Valor));
    }
}

/// <summary>
/// Da de alta un plan de cuentas (por ejemplo, el de otro ERP al migrar): crea las cuentas que falten y deja
/// intactas las que ya existen. Devuelve cuántas creó y los códigos rechazados con su motivo.
/// </summary>
public sealed class ImportarPlanCuentas
{
    private readonly IRepositorioCuentas _cuentas;
    private readonly IUnidadDeTrabajoContabilidad _unidad;

    public ImportarPlanCuentas(IRepositorioCuentas cuentas, IUnidadDeTrabajoContabilidad unidad)
    {
        _cuentas = cuentas;
        _unidad = unidad;
    }

    public async Task<(int Creadas, IReadOnlyList<string> Rechazadas)> EjecutarAsync(Guid empresaId, IReadOnlyList<(string Codigo, string Nombre)> cuentas, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(cuentas);
        await SembradorPlan.AsegurarAsync(empresaId, _cuentas, ct).ConfigureAwait(false);
        var existentes = (await _cuentas.CodigosExistentesAsync(empresaId, ct).ConfigureAwait(false)).ToHashSet(StringComparer.Ordinal);
        var creadas = 0;
        var rechazadas = new List<string>();
        foreach (var (codigo, nombre) in cuentas)
        {
            var c = codigo?.Trim() ?? string.Empty;
            if (existentes.Contains(c))
            {
                continue;
            }

            var cuenta = Cuenta.Crear(empresaId, c, nombre);
            if (cuenta.EsFallo)
            {
                rechazadas.Add($"{c}: {cuenta.Error.Mensaje}");
                continue;
            }

            _cuentas.Agregar(cuenta.Valor);
            existentes.Add(c);
            creadas++;
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return (creadas, rechazadas);
    }
}
