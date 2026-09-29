using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Cooperativa.Dominio;

/// <summary>Forma jurídica de la entidad: cambia los fondos obligatorios y cómo se reparte el resultado.</summary>
public enum FormaJuridica
{
    /// <summary>
    /// Sociedad cooperativa: fondos obligatorios (reserva y educación) y retorno a los socios según su actividad con la
    /// cooperativa (lo que entregaron), no según su capital.
    /// </summary>
    Cooperativa = 1,

    /// <summary>Sociedad agraria de transformación: sin fondos obligatorios; el resultado se reparte según el capital.</summary>
    Sat = 2,
}

/// <summary>Con qué se mide la parte de cada socio en el retorno.</summary>
public enum BaseRetorno
{
    /// <summary>Kilos entregados en el ejercicio (liquidaciones emitidas).</summary>
    Kilos = 1,

    /// <summary>Importe liquidado en el ejercicio (base de las liquidaciones emitidas).</summary>
    Importe = 2,

    /// <summary>Capital desembolsado al cierre del ejercicio (lo propio de una SAT).</summary>
    Capital = 3,
}

/// <summary>
/// Ajustes de la cooperativa o SAT: forma jurídica, aportación obligatoria, porcentajes mínimos de los fondos, límites de
/// las deducciones en las bajas, retención y cuentas contables. Las cuentas por defecto son orientativas: se ajustan al
/// plan de la entidad (adaptación del plan contable a cooperativas).
/// </summary>
public sealed class ConfiguracionCooperativa : RaizAgregadoEmpresa<Guid>
{
    private ConfiguracionCooperativa(Guid id)
        : base(id, Guid.Empty)
    {
        Cuentas = CuentasCooperativa.PorDefecto;
    }

    private ConfiguracionCooperativa(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
        Cuentas = CuentasCooperativa.PorDefecto;
    }

    public FormaJuridica Forma { get; private set; } = FormaJuridica.Cooperativa;

    /// <summary>Aportación obligatoria mínima de cada socio (0: los estatutos no fijan mínimo).</summary>
    public decimal AportacionObligatoria { get; private set; }

    public decimal PorcentajeFroMinimo { get; private set; } = 20m;

    public decimal PorcentajeFepMinimo { get; private set; } = 5m;

    /// <summary>Interés máximo que se puede pagar al capital (el legal del dinero más seis puntos, según la ley aplicable).</summary>
    public decimal InteresMaximoCapital { get; private set; } = 9.25m;

    /// <summary>Retención sobre retornos e intereses (rendimientos del capital mobiliario).</summary>
    public decimal PorcentajeRetencion { get; private set; } = 19m;

    public BaseRetorno Base { get; private set; } = BaseRetorno.Kilos;

    /// <summary>Deducción máxima sobre las aportaciones obligatorias en una expulsión (%).</summary>
    public decimal DeduccionMaximaExpulsion { get; private set; } = 30m;

    /// <summary>Deducción máxima sobre las aportaciones obligatorias en una baja voluntaria no justificada (%).</summary>
    public decimal DeduccionMaximaNoJustificada { get; private set; } = 20m;

    /// <summary>Si las operaciones de capital y los repartos generan su asiento.</summary>
    public bool Contabilizar { get; private set; } = true;

    public CuentasCooperativa Cuentas { get; private set; }

    public static ConfiguracionCooperativa PorDefecto(Guid empresaId) => new(Guid.NewGuid(), empresaId);

    public Resultado Fijar(FormaJuridica forma, decimal aportacionObligatoria, decimal froMinimo, decimal fepMinimo, decimal interesMaximo, decimal retencion,
        BaseRetorno baseRetorno, decimal deduccionExpulsion, decimal deduccionNoJustificada, bool contabilizar, CuentasCooperativa cuentas)
    {
        ArgumentNullException.ThrowIfNull(cuentas);
        if (!Enum.IsDefined(forma) || !Enum.IsDefined(baseRetorno))
        {
            return Resultado.Fallo(Error.Validacion("cooperativa.forma", "Indica la forma jurídica (cooperativa o SAT) y la base del retorno (kilos, importe o capital)."));
        }

        if (aportacionObligatoria < 0m || decimal.Round(aportacionObligatoria, 2) != aportacionObligatoria)
        {
            return Resultado.Fallo(Error.Validacion("cooperativa.aportacion", "La aportación obligatoria no puede ser negativa (y lleva dos decimales como mucho)."));
        }

        if (new[] { froMinimo, fepMinimo, interesMaximo, retencion, deduccionExpulsion, deduccionNoJustificada }.Any(p => p is < 0m or > 100m))
        {
            return Resultado.Fallo(Error.Validacion("cooperativa.porcentaje", "Los porcentajes van de 0 a 100."));
        }

        if (froMinimo + fepMinimo > 100m)
        {
            return Resultado.Fallo(Error.Validacion("cooperativa.fondos", "Los fondos obligatorios no pueden pasar del 100 % del excedente."));
        }

        if (cuentas.Validar() is { } mal)
        {
            return Resultado.Fallo(mal);
        }

        Forma = forma;
        AportacionObligatoria = aportacionObligatoria;
        PorcentajeFroMinimo = froMinimo;
        PorcentajeFepMinimo = fepMinimo;
        InteresMaximoCapital = interesMaximo;
        PorcentajeRetencion = retencion;
        Base = baseRetorno;
        DeduccionMaximaExpulsion = deduccionExpulsion;
        DeduccionMaximaNoJustificada = deduccionNoJustificada;
        Contabilizar = contabilizar;
        Cuentas = cuentas;
        return Resultado.Ok();
    }

    /// <summary>Deducción máxima (%) sobre las aportaciones obligatorias según el motivo de la baja.</summary>
    public decimal DeduccionMaxima(MotivoBaja motivo) => motivo switch
    {
        MotivoBaja.Expulsion => DeduccionMaximaExpulsion,
        MotivoBaja.VoluntariaNoJustificada => DeduccionMaximaNoJustificada,
        _ => 0m,
    };
}

/// <summary>Cuentas de las operaciones de la cooperativa.</summary>
public sealed record CuentasCooperativa(
    string Capital,
    string DesembolsosPendientes,
    string Tesoreria,
    string Reembolsos,
    string Resultado,
    string Fro,
    string Fep,
    string ReservasVoluntarias,
    string Retornos,
    string Retenciones)
{
    public static CuentasCooperativa PorDefecto { get; } = new("100", "103", "572", "552", "129", "112", "144", "113", "526", "4751");

    public IEnumerable<string> Todas => [Capital, DesembolsosPendientes, Tesoreria, Reembolsos, Resultado, Fro, Fep, ReservasVoluntarias, Retornos, Retenciones];

    public Error? Validar() =>
        Todas.Any(c => string.IsNullOrWhiteSpace(c) || c.Trim().Length > 20 || !c.Trim().All(char.IsAsciiDigit))
            ? Error.Validacion("cooperativa.cuentas", "Cada cuenta es un código numérico de hasta 20 dígitos.")
            : null;

    public CuentasCooperativa Limpias() => new(Capital.Trim(), DesembolsosPendientes.Trim(), Tesoreria.Trim(), Reembolsos.Trim(), Resultado.Trim(), Fro.Trim(), Fep.Trim(),
        ReservasVoluntarias.Trim(), Retornos.Trim(), Retenciones.Trim());
}

/// <summary>Clase de socio.</summary>
public enum TipoSocio
{
    /// <summary>Socio común: entrega su producción y participa en el retorno.</summary>
    Comun = 1,

    /// <summary>Socio de trabajo: aporta su trabajo; participa en el retorno.</summary>
    DeTrabajo = 2,

    /// <summary>Colaborador: aporta capital pero no actividad; no tiene retorno (sí intereses).</summary>
    Colaborador = 3,

    /// <summary>Inactivo (excedente): dejó la actividad pero sigue siendo socio; no tiene retorno.</summary>
    Inactivo = 4,
}

/// <summary>Motivo de la baja del socio: fija la deducción máxima sobre sus aportaciones obligatorias.</summary>
public enum MotivoBaja
{
    VoluntariaJustificada = 1,
    VoluntariaNoJustificada = 2,

    /// <summary>Obligatoria: deja de cumplir los requisitos para ser socio.</summary>
    Obligatoria = 3,

    Expulsion = 4,
    Fallecimiento = 5,
}

/// <summary>
/// Socio de la cooperativa o SAT. Es un tercero (su ficha de proveedor tiene el NIF y el domicilio); el número de socio
/// es correlativo y no se reutiliza. La baja no se deshace: si vuelve, es un socio nuevo con otro número.
/// </summary>
public sealed class Socio : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudObservaciones = 500;

    private Socio(Guid id)
        : base(id, Guid.Empty)
    {
        Nombre = null!;
    }

    private Socio(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
        Nombre = null!;
    }

    public int Numero { get; private set; }

    public Guid ProveedorId { get; private set; }

    /// <summary>Nombre del tercero al darlo de alta (para listados; el libro toma los datos al día de la ficha).</summary>
    public string Nombre { get; private set; }

    public string? Nif { get; private set; }

    public TipoSocio Tipo { get; private set; }

    public DateOnly FechaAlta { get; private set; }

    public DateOnly? FechaBaja { get; private set; }

    public MotivoBaja? MotivoBaja { get; private set; }

    public string? Observaciones { get; private set; }

    public bool DeBaja => FechaBaja is not null;

    /// <summary>¿Le toca retorno por su actividad? Los colaboradores e inactivos no (salvo en una SAT, que reparte por capital).</summary>
    public bool ConRetorno(FormaJuridica forma) => forma == FormaJuridica.Sat || Tipo is TipoSocio.Comun or TipoSocio.DeTrabajo;

    public static Resultado<Socio> Crear(Guid empresaId, int numero, Guid proveedorId, string nombre, string? nif, TipoSocio tipo, DateOnly fechaAlta, string? observaciones)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        if (numero <= 0)
        {
            return Resultado.Fallo<Socio>(Error.Validacion("socio.numero", "El número de socio es un entero positivo."));
        }

        var s = new Socio(Guid.NewGuid(), empresaId)
        {
            Numero = numero, ProveedorId = proveedorId, Nombre = Recortar(nombre, 200)!, Nif = Recortar(nif, 20), FechaAlta = fechaAlta,
        };
        var r = s.Actualizar(tipo, observaciones);
        return r.EsFallo ? Resultado.Fallo<Socio>(r.Error) : Resultado.Ok(s);
    }

    public Resultado Actualizar(TipoSocio tipo, string? observaciones)
    {
        if (!Enum.IsDefined(tipo))
        {
            return Resultado.Fallo(Error.Validacion("socio.tipo", "El socio es común, de trabajo, colaborador o inactivo."));
        }

        Tipo = tipo;
        Observaciones = Recortar(observaciones, LongitudObservaciones);
        return Resultado.Ok();
    }

    public Resultado DarDeBaja(DateOnly fecha, MotivoBaja motivo)
    {
        if (DeBaja)
        {
            return Resultado.Fallo(Error.Conflicto("socio.ya_de_baja", "El socio ya está de baja."));
        }

        if (!Enum.IsDefined(motivo))
        {
            return Resultado.Fallo(Error.Validacion("socio.motivo_baja", "Indica el motivo de la baja."));
        }

        if (fecha < FechaAlta)
        {
            return Resultado.Fallo(Error.Validacion("socio.fecha_baja", "La baja no puede ser anterior al alta."));
        }

        FechaBaja = fecha;
        MotivoBaja = motivo;
        return Resultado.Ok();
    }

    internal static string? Recortar(string? texto, int maximo) =>
        string.IsNullOrWhiteSpace(texto) ? null : texto.Trim().Length > maximo ? texto.Trim()[..maximo] : texto.Trim();
}

/// <summary>Aportación obligatoria (la que exigen los estatutos para ser socio) o voluntaria.</summary>
public enum ClaseAportacion
{
    Obligatoria = 1,
    Voluntaria = 2,
}

public enum TipoMovimientoCapital
{
    /// <summary>El socio suscribe capital (y puede desembolsar parte o todo en el acto).</summary>
    Suscripcion = 1,

    /// <summary>Desembolso de capital ya suscrito.</summary>
    Desembolso = 2,

    /// <summary>Devolución del capital (en la baja, o de aportaciones voluntarias), con la deducción que corresponda.</summary>
    Reembolso = 3,

    /// <summary>Cesión de capital a otro socio: sale de este…</summary>
    TransmisionSalida = 4,

    /// <summary>…y entra en el otro.</summary>
    TransmisionEntrada = 5,

    /// <summary>Retorno cooperativo que el socio deja como capital (acuerdo de reparto).</summary>
    RetornoCapitalizado = 6,

    /// <summary>Deja sin efecto otro movimiento (el mismo importe con el signo cambiado).</summary>
    Anulacion = 7,
}

/// <summary>
/// Un movimiento del capital de un socio: lo que cambia su capital suscrito y el desembolsado. El libro de aportaciones
/// es la lista de movimientos, así que no se modifican ni se borran: un error se corrige con una anulación. La base de
/// datos impide que el desembolsado supere lo suscrito o quede en negativo.
/// </summary>
public sealed class MovimientoCapital : RaizAgregadoEmpresa<Guid>
{
    private MovimientoCapital(Guid id)
        : base(id, Guid.Empty)
    {
        Concepto = null!;
    }

    private MovimientoCapital(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
        Concepto = null!;
    }

    public Guid SocioId { get; private set; }

    public DateOnly Fecha { get; private set; }

    public TipoMovimientoCapital Tipo { get; private set; }

    public ClaseAportacion Clase { get; private set; }

    /// <summary>Cambio del capital suscrito (+ sube, − baja).</summary>
    public decimal Suscrito { get; private set; }

    /// <summary>Cambio del capital desembolsado (+ sube, − baja).</summary>
    public decimal Desembolsado { get; private set; }

    /// <summary>En un reembolso: lo que la cooperativa retiene (va al fondo de reserva); el socio cobra el resto.</summary>
    public decimal Deduccion { get; private set; }

    public string Concepto { get; private set; }

    /// <summary>Une los dos movimientos de una transmisión.</summary>
    public Guid? GrupoId { get; private set; }

    /// <summary>Reparto que capitalizó el retorno.</summary>
    public Guid? RepartoId { get; private set; }

    public Guid? AnulaId { get; private set; }

    public Guid? AsientoId { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    /// <summary>Lo que se paga (+) o cobra (−) al socio en dinero: en un reembolso, lo desembolsado menos la deducción.</summary>
    public decimal ADevolver => Tipo == TipoMovimientoCapital.Reembolso ? -Desembolsado - Deduccion : 0m;

    internal void Contabilizado(Guid asientoId) => AsientoId = asientoId;

    public static Resultado<MovimientoCapital> Suscripcion(Guid empresaId, Guid socioId, DateOnly fecha, ClaseAportacion clase, decimal suscrito, decimal desembolsado,
        string? concepto, DateTimeOffset ahora)
    {
        if (!Importe(suscrito) || suscrito <= 0m || !Importe(desembolsado) || desembolsado < 0m || desembolsado > suscrito)
        {
            return Fallo("capital.importe", "Lo suscrito tiene que ser positivo y lo desembolsado, entre cero y lo suscrito (con dos decimales).");
        }

        return Nuevo(empresaId, socioId, fecha, TipoMovimientoCapital.Suscripcion, clase, suscrito, desembolsado, 0m, concepto ?? "Suscripción de capital", ahora);
    }

    public static Resultado<MovimientoCapital> Desembolso(Guid empresaId, Guid socioId, DateOnly fecha, ClaseAportacion clase, decimal importe, string? concepto,
        DateTimeOffset ahora)
    {
        if (!Importe(importe) || importe <= 0m)
        {
            return Fallo("capital.importe", "El desembolso tiene que ser positivo (con dos decimales).");
        }

        return Nuevo(empresaId, socioId, fecha, TipoMovimientoCapital.Desembolso, clase, 0m, importe, 0m, concepto ?? "Desembolso de capital", ahora);
    }

    /// <summary>Reembolso: baja lo suscrito y lo desembolsado; la deducción se aplica sobre lo desembolsado.</summary>
    public static Resultado<MovimientoCapital> Reembolso(Guid empresaId, Guid socioId, DateOnly fecha, ClaseAportacion clase, decimal suscrito, decimal desembolsado,
        decimal deduccion, string? concepto, DateTimeOffset ahora)
    {
        if (!Importe(suscrito) || !Importe(desembolsado) || !Importe(deduccion) || suscrito <= 0m || desembolsado < 0m || desembolsado > suscrito || deduccion < 0m
            || deduccion > desembolsado)
        {
            return Fallo("capital.importe", "El reembolso baja lo suscrito (positivo) y lo desembolsado (hasta lo suscrito); la deducción no puede pasar de lo desembolsado.");
        }

        return Nuevo(empresaId, socioId, fecha, TipoMovimientoCapital.Reembolso, clase, -suscrito, -desembolsado, deduccion, concepto ?? "Reembolso de capital", ahora);
    }

    /// <summary>Transmisión de capital desembolsado de un socio a otro: dos movimientos unidos.</summary>
    public static Resultado<(MovimientoCapital Salida, MovimientoCapital Entrada)> Transmision(Guid empresaId, Guid deSocioId, Guid aSocioId, DateOnly fecha,
        ClaseAportacion clase, decimal importe, string? concepto, DateTimeOffset ahora)
    {
        if (deSocioId == aSocioId)
        {
            return Resultado.Fallo<(MovimientoCapital, MovimientoCapital)>(Error.Validacion("capital.transmision", "El capital se transmite a otro socio."));
        }

        if (!Importe(importe) || importe <= 0m)
        {
            return Resultado.Fallo<(MovimientoCapital, MovimientoCapital)>(Error.Validacion("capital.importe", "La transmisión tiene que ser positiva (con dos decimales)."));
        }

        var grupo = Guid.NewGuid();
        var texto = concepto ?? "Transmisión de capital";
        var salida = Nuevo(empresaId, deSocioId, fecha, TipoMovimientoCapital.TransmisionSalida, clase, -importe, -importe, 0m, texto, ahora).Valor;
        var entrada = Nuevo(empresaId, aSocioId, fecha, TipoMovimientoCapital.TransmisionEntrada, clase, importe, importe, 0m, texto, ahora).Valor;
        salida.GrupoId = grupo;
        entrada.GrupoId = grupo;
        return Resultado.Ok((salida, entrada));
    }

    public static MovimientoCapital RetornoCapitalizado(Guid empresaId, Guid socioId, DateOnly fecha, decimal importe, Guid repartoId, string concepto, DateTimeOffset ahora)
    {
        var m = Nuevo(empresaId, socioId, fecha, TipoMovimientoCapital.RetornoCapitalizado, ClaseAportacion.Voluntaria, importe, importe, 0m, concepto, ahora).Valor;
        m.RepartoId = repartoId;
        return m;
    }

    /// <summary>Anulación de este movimiento: el mismo importe con el signo cambiado.</summary>
    public MovimientoCapital Anulacion(DateOnly fecha, string motivo, DateTimeOffset ahora)
    {
        var m = Nuevo(EmpresaId, SocioId, fecha, TipoMovimientoCapital.Anulacion, Clase, -Suscrito, -Desembolsado, -Deduccion, $"Anulación: {motivo}", ahora).Valor;
        m.AnulaId = Id;
        m.GrupoId = GrupoId;
        m.RepartoId = RepartoId;
        return m;
    }

    private static Resultado<MovimientoCapital> Nuevo(Guid empresaId, Guid socioId, DateOnly fecha, TipoMovimientoCapital tipo, ClaseAportacion clase, decimal suscrito,
        decimal desembolsado, decimal deduccion, string concepto, DateTimeOffset ahora)
    {
        if (!Enum.IsDefined(clase))
        {
            return Fallo("capital.clase", "La aportación es obligatoria o voluntaria.");
        }

        return Resultado.Ok(new MovimientoCapital(Guid.NewGuid(), empresaId)
        {
            SocioId = socioId, Fecha = fecha, Tipo = tipo, Clase = clase, Suscrito = suscrito, Desembolsado = desembolsado, Deduccion = deduccion,
            Concepto = Socio.Recortar(concepto, 200) ?? tipo.ToString(), CreadoEn = ahora,
        });
    }

    private static bool Importe(decimal v) => Redondeo.Dos(v) == v;

    private static Resultado<MovimientoCapital> Fallo(string codigo, string mensaje) => Resultado.Fallo<MovimientoCapital>(Error.Validacion(codigo, mensaje));
}

/// <summary>Capital de un socio en una clase de aportación.</summary>
public sealed record SaldoCapital(ClaseAportacion Clase, decimal Suscrito, decimal Desembolsado)
{
    public decimal Pendiente => Suscrito - Desembolsado;

    public static IReadOnlyList<SaldoCapital> De(IEnumerable<MovimientoCapital> movimientos) =>
        Enum.GetValues<ClaseAportacion>()
            .Select(c => (c, l: movimientos.Where(m => m.Clase == c).ToList()))
            .Select(x => new SaldoCapital(x.c, x.l.Sum(m => m.Suscrito), x.l.Sum(m => m.Desembolsado)))
            .ToList();
}
