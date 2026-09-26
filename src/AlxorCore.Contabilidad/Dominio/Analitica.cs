using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Contabilidad.Dominio;

// ================================================================================================
//  Contabilidad analítica en dos dimensiones (como Hispatec): el CENTRO dice dónde se produce el
//  coste o el ingreso (centro de coste, proyecto, finca, campaña, departamento…) y la PARTIDA qué es
//  (su naturaleza analítica). La partida es opcional: sin ella, el análisis se hace por cuenta del PGC.
//  Los apuntes de los grupos 6 y 7 se imputan a centros y partidas sin tocar el libro diario (los
//  apuntes son inalterables); la imputación es información de gestión y puede corregirse.
// ================================================================================================

/// <summary>Naturaleza analítica de un importe: ingreso (grupo 7) o gasto (grupo 6).</summary>
public enum NaturalezaAnalitica
{
    Ingreso = 1,
    Gasto = 2,
}

/// <summary>Qué representa un centro analítico (solo informativo y para filtrar).</summary>
public enum TipoCentroAnalitico
{
    CentroCoste = 1,
    Proyecto = 2,
    Departamento = 3,
    Campana = 4,
    Finca = 5,
    Otro = 9,
}

/// <summary>De dónde sale una imputación analítica.</summary>
public enum OrigenImputacion
{
    /// <summary>Asignada a mano (en el asiento manual o corrigiendo un apunte).</summary>
    Manual = 1,

    /// <summary>Asignada por una regla al contabilizar el documento.</summary>
    Regla = 2,

    /// <summary>Asignada por el proceso «imputar pendientes» (reglas aplicadas después).</summary>
    Proceso = 3,

    /// <summary>Reparto secundario: traspaso de costes de un centro a otros con una clave.</summary>
    Reparto = 4,
}

/// <summary>Validaciones comunes de los maestros analíticos.</summary>
internal static class MaestroAnalitico
{
    public const int LongitudMaximaCodigo = 20;
    public const int LongitudMaximaNombre = 100;

    public static Error? Validar(ref string? codigo, ref string? nombre, string ambito)
    {
        codigo = codigo?.Trim().ToUpperInvariant();
        nombre = nombre?.Trim();
        if (string.IsNullOrWhiteSpace(codigo) || codigo.Length > LongitudMaximaCodigo)
        {
            return Error.Validacion($"{ambito}.codigo", $"El código es obligatorio (máximo {LongitudMaximaCodigo} caracteres).");
        }

        return string.IsNullOrWhiteSpace(nombre) || nombre.Length > LongitudMaximaNombre
            ? Error.Validacion($"{ambito}.nombre", $"El nombre es obligatorio (máximo {LongitudMaximaNombre} caracteres).")
            : null;
    }
}

/// <summary>
/// Centro analítico (dónde): maestro del grupo, compartido por sus empresas, en árbol de cualquier
/// profundidad (el importe de un centro incluye el de sus hijos en el informe).
/// </summary>
public sealed class CentroAnalitico : RaizAgregadoGrupo<Guid>
{
    private CentroAnalitico(Guid id)
        : base(id, Guid.Empty)
    {
        Codigo = null!;
        Nombre = null!;
    }

    private CentroAnalitico(Guid id, Guid grupoId, string codigo, string nombre, TipoCentroAnalitico tipo, Guid? padreId)
        : base(id, grupoId)
    {
        Codigo = codigo;
        Nombre = nombre;
        Tipo = tipo;
        PadreId = padreId;
        Activo = true;
    }

    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    public TipoCentroAnalitico Tipo { get; private set; }

    public Guid? PadreId { get; private set; }

    /// <summary>Un centro inactivo no admite imputaciones nuevas (sí conserva las que tiene).</summary>
    public bool Activo { get; private set; }

    public static Resultado<CentroAnalitico> Crear(Guid grupoId, string? codigo, string? nombre, TipoCentroAnalitico tipo, Guid? padreId)
    {
        var error = MaestroAnalitico.Validar(ref codigo, ref nombre, "centro") ?? ValidarTipo(tipo);
        return error is not null
            ? Resultado.Fallo<CentroAnalitico>(error)
            : Resultado.Ok(new CentroAnalitico(Guid.NewGuid(), grupoId, codigo!, nombre!, tipo, padreId));
    }

    public Resultado Actualizar(string? nombre, TipoCentroAnalitico tipo, Guid? padreId, bool activo)
    {
        var codigo = Codigo;
        var error = MaestroAnalitico.Validar(ref codigo, ref nombre, "centro") ?? ValidarTipo(tipo);
        if (error is not null)
        {
            return Resultado.Fallo(error);
        }

        if (padreId == Id)
        {
            return Resultado.Fallo(Error.Validacion("centro.padre", "Un centro no puede depender de sí mismo."));
        }

        Nombre = nombre!;
        Tipo = tipo;
        PadreId = padreId;
        Activo = activo;
        return Resultado.Ok();
    }

    private static Error? ValidarTipo(TipoCentroAnalitico tipo) =>
        Enum.IsDefined(tipo) ? null : Error.Validacion("centro.tipo", "El tipo de centro no es válido.");
}

/// <summary>Partida analítica (qué): maestro del grupo en árbol libre; su naturaleza es ingreso o gasto.</summary>
public sealed class PartidaAnalitica : RaizAgregadoGrupo<Guid>
{
    private PartidaAnalitica(Guid id)
        : base(id, Guid.Empty)
    {
        Codigo = null!;
        Nombre = null!;
    }

    private PartidaAnalitica(Guid id, Guid grupoId, string codigo, string nombre, NaturalezaAnalitica naturaleza, Guid? padreId)
        : base(id, grupoId)
    {
        Codigo = codigo;
        Nombre = nombre;
        Naturaleza = naturaleza;
        PadreId = padreId;
        Activo = true;
    }

    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    public NaturalezaAnalitica Naturaleza { get; private set; }

    public Guid? PadreId { get; private set; }

    public bool Activo { get; private set; }

    public static Resultado<PartidaAnalitica> Crear(Guid grupoId, string? codigo, string? nombre, NaturalezaAnalitica naturaleza, Guid? padreId)
    {
        var error = MaestroAnalitico.Validar(ref codigo, ref nombre, "partida") ?? ValidarNaturaleza(naturaleza);
        return error is not null
            ? Resultado.Fallo<PartidaAnalitica>(error)
            : Resultado.Ok(new PartidaAnalitica(Guid.NewGuid(), grupoId, codigo!, nombre!, naturaleza, padreId));
    }

    public Resultado Actualizar(string? nombre, Guid? padreId, bool activo)
    {
        var codigo = Codigo;
        var error = MaestroAnalitico.Validar(ref codigo, ref nombre, "partida");
        if (error is not null)
        {
            return Resultado.Fallo(error);
        }

        if (padreId == Id)
        {
            return Resultado.Fallo(Error.Validacion("partida.padre", "Una partida no puede depender de sí misma."));
        }

        Nombre = nombre!;
        PadreId = padreId;
        Activo = activo;
        return Resultado.Ok();
    }

    private static Error? ValidarNaturaleza(NaturalezaAnalitica naturaleza) =>
        Enum.IsDefined(naturaleza) ? null : Error.Validacion("partida.naturaleza", "La naturaleza debe ser Ingreso o Gasto.");
}

/// <summary>Porcentaje de una clave de reparto que va a un centro.</summary>
public sealed record PorcentajeReparto(Guid CentroId, decimal Porcentaje);

/// <summary>Línea de una clave de reparto.</summary>
public sealed class LineaClaveReparto
{
    private LineaClaveReparto()
    {
    }

    internal LineaClaveReparto(Guid centroId, decimal porcentaje)
    {
        Id = Guid.NewGuid();
        CentroId = centroId;
        Porcentaje = porcentaje;
    }

    public Guid Id { get; private set; }

    public Guid CentroId { get; private set; }

    public decimal Porcentaje { get; private set; }
}

/// <summary>Clave de reparto: cómo se reparte un importe entre centros (los porcentajes suman 100).</summary>
public sealed class ClaveReparto : RaizAgregadoGrupo<Guid>
{
    private readonly List<LineaClaveReparto> _lineas = [];

    private ClaveReparto(Guid id)
        : base(id, Guid.Empty)
    {
        Codigo = null!;
        Nombre = null!;
    }

    private ClaveReparto(Guid id, Guid grupoId, string codigo, string nombre)
        : base(id, grupoId)
    {
        Codigo = codigo;
        Nombre = nombre;
        Activa = true;
    }

    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    public bool Activa { get; private set; }

    public IReadOnlyList<LineaClaveReparto> Lineas => _lineas.AsReadOnly();

    public IReadOnlyList<PorcentajeReparto> Reparto => _lineas.Select(l => new PorcentajeReparto(l.CentroId, l.Porcentaje)).ToList();

    public static Resultado<ClaveReparto> Crear(Guid grupoId, string? codigo, string? nombre, IReadOnlyList<PorcentajeReparto> reparto)
    {
        var error = MaestroAnalitico.Validar(ref codigo, ref nombre, "clave") ?? ValidarReparto(reparto);
        if (error is not null)
        {
            return Resultado.Fallo<ClaveReparto>(error);
        }

        var clave = new ClaveReparto(Guid.NewGuid(), grupoId, codigo!, nombre!);
        clave._lineas.AddRange(reparto.Select(r => new LineaClaveReparto(r.CentroId, r.Porcentaje)));
        return Resultado.Ok(clave);
    }

    public Resultado Actualizar(string? nombre, IReadOnlyList<PorcentajeReparto> reparto, bool activa)
    {
        var codigo = Codigo;
        var error = MaestroAnalitico.Validar(ref codigo, ref nombre, "clave") ?? ValidarReparto(reparto);
        if (error is not null)
        {
            return Resultado.Fallo(error);
        }

        Nombre = nombre!;
        Activa = activa;
        _lineas.Clear();
        _lineas.AddRange(reparto.Select(r => new LineaClaveReparto(r.CentroId, r.Porcentaje)));
        return Resultado.Ok();
    }

    /// <summary>Al menos un centro, sin repetir, porcentajes positivos con dos decimales y suma exacta de 100.</summary>
    public static Error? ValidarReparto(IReadOnlyList<PorcentajeReparto>? reparto)
    {
        if (reparto is null || reparto.Count == 0)
        {
            return Error.Validacion("clave.reparto", "La clave necesita al menos un centro.");
        }

        if (reparto.Select(r => r.CentroId).Distinct().Count() != reparto.Count)
        {
            return Error.Validacion("clave.reparto", "Un centro aparece dos veces en la clave.");
        }

        if (reparto.Any(r => r.Porcentaje <= 0m || r.Porcentaje > 100m || decimal.Round(r.Porcentaje, 2) != r.Porcentaje))
        {
            return Error.Validacion("clave.reparto", "Cada porcentaje debe ser mayor que 0, como mucho 100 y con dos decimales.");
        }

        var suma = reparto.Sum(r => r.Porcentaje);
        return suma != 100m
            ? Error.Validacion("clave.reparto", $"Los porcentajes deben sumar 100 (suman {suma.ToString(System.Globalization.CultureInfo.InvariantCulture)}).")
            : null;
    }
}

/// <summary>Datos de un apunte (o documento) con los que se decide qué regla analítica aplica.</summary>
public sealed record ContextoImputacion(string CuentaCodigo, DateOnly Fecha, Guid? TerceroId = null, Guid? ActividadNegocioId = null, string? Familia = null);

/// <summary>
/// Regla de asignación analítica: si el apunte cumple sus criterios (cuenta que empieza por un prefijo,
/// tercero, actividad de negocio, familia del artículo, vigencia), se imputa al centro indicado —o se
/// reparte con una clave— y, opcionalmente, a una partida. Sustituye a las dos tablas de Hispatec
/// (proyecto por entidad y partida por entidad) con una precedencia explícita (<see cref="Especificidad"/>).
/// </summary>
public sealed class ReglaAnalitica : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMaximaDescripcion = 150;

    private ReglaAnalitica(Guid id)
        : base(id, Guid.Empty)
    {
        Descripcion = null!;
    }

    private ReglaAnalitica(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
        Descripcion = null!;
    }

    public string Descripcion { get; private set; }

    public string? PrefijoCuenta { get; private set; }

    public Guid? TerceroId { get; private set; }

    public Guid? ActividadNegocioId { get; private set; }

    public string? Familia { get; private set; }

    public DateOnly? VigenteDesde { get; private set; }

    public DateOnly? VigenteHasta { get; private set; }

    public Guid? CentroId { get; private set; }

    public Guid? ClaveRepartoId { get; private set; }

    public Guid? PartidaId { get; private set; }

    public int Prioridad { get; private set; }

    public static Resultado<ReglaAnalitica> Crear(Guid empresaId, DatosReglaAnalitica datos)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var regla = new ReglaAnalitica(Guid.NewGuid(), empresaId);
        var r = regla.Aplicar(datos);
        return r.EsFallo ? Resultado.Fallo<ReglaAnalitica>(r.Error) : Resultado.Ok(regla);
    }

    public Resultado Aplicar(DatosReglaAnalitica datos)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var prefijo = string.IsNullOrWhiteSpace(datos.PrefijoCuenta) ? null : datos.PrefijoCuenta.Trim();
        if (prefijo is not null && (prefijo.Length > 12 || !prefijo.All(char.IsAsciiDigit) || prefijo[0] is not ('6' or '7')))
        {
            return Resultado.Fallo(Error.Validacion("regla.cuenta", "El prefijo de cuenta debe ser numérico y de los grupos 6 (gastos) o 7 (ingresos)."));
        }

        var familia = string.IsNullOrWhiteSpace(datos.Familia) ? null : datos.Familia.Trim();
        if (prefijo is null && datos.TerceroId is null && datos.ActividadNegocioId is null && familia is null)
        {
            return Resultado.Fallo(Error.Validacion("regla.sin_criterio", "La regla necesita al menos un criterio: cuenta, tercero, actividad o familia."));
        }

        if ((datos.CentroId is null) == (datos.ClaveRepartoId is null))
        {
            return Resultado.Fallo(Error.Validacion("regla.destino", "Indica un centro o una clave de reparto (uno de los dos)."));
        }

        if (datos.VigenteDesde is { } d && datos.VigenteHasta is { } h && h < d)
        {
            return Resultado.Fallo(Error.Validacion("regla.vigencia", "La vigencia termina antes de empezar."));
        }

        var descripcion = string.IsNullOrWhiteSpace(datos.Descripcion) ? "Regla analítica" : datos.Descripcion.Trim();
        if (descripcion.Length > LongitudMaximaDescripcion)
        {
            return Resultado.Fallo(Error.Validacion("regla.descripcion", "La descripción es demasiado larga."));
        }

        Descripcion = descripcion;
        PrefijoCuenta = prefijo;
        TerceroId = datos.TerceroId;
        ActividadNegocioId = datos.ActividadNegocioId;
        Familia = familia;
        VigenteDesde = datos.VigenteDesde;
        VigenteHasta = datos.VigenteHasta;
        CentroId = datos.CentroId;
        ClaveRepartoId = datos.ClaveRepartoId;
        PartidaId = datos.PartidaId;
        Prioridad = datos.Prioridad;
        return Resultado.Ok();
    }

    /// <summary>Si el apunte cumple todos los criterios de la regla.</summary>
    public bool Cumple(ContextoImputacion c)
    {
        ArgumentNullException.ThrowIfNull(c);
        return (PrefijoCuenta is null || c.CuentaCodigo.StartsWith(PrefijoCuenta, StringComparison.Ordinal))
            && (TerceroId is null || TerceroId == c.TerceroId)
            && (ActividadNegocioId is null || ActividadNegocioId == c.ActividadNegocioId)
            && (Familia is null || string.Equals(Familia, c.Familia, StringComparison.OrdinalIgnoreCase))
            && (VigenteDesde is null || c.Fecha >= VigenteDesde)
            && (VigenteHasta is null || c.Fecha <= VigenteHasta);
    }

    /// <summary>
    /// Orden de precedencia: primero la prioridad explícita; a igualdad, la regla con más criterios; y
    /// entre ellas, la que fija el tercero, luego la actividad, luego la familia y luego el prefijo de
    /// cuenta más largo (lo más concreto gana a lo más general).
    /// </summary>
    public (int Prioridad, int Criterios, int Tercero, int Actividad, int Familia, int Prefijo) Especificidad =>
        (Prioridad,
         (PrefijoCuenta is null ? 0 : 1) + (TerceroId is null ? 0 : 1) + (ActividadNegocioId is null ? 0 : 1) + (Familia is null ? 0 : 1),
         TerceroId is null ? 0 : 1, ActividadNegocioId is null ? 0 : 1, Familia is null ? 0 : 1, PrefijoCuenta?.Length ?? 0);
}

/// <summary>Datos de una regla analítica.</summary>
public sealed record DatosReglaAnalitica(
    string? Descripcion, string? PrefijoCuenta, Guid? TerceroId, Guid? ActividadNegocioId, string? Familia,
    DateOnly? VigenteDesde, DateOnly? VigenteHasta, Guid? CentroId, Guid? ClaveRepartoId, Guid? PartidaId, int Prioridad = 0);

/// <summary>
/// Periodo analítico: ejercicio de gestión independiente del fiscal (una campaña de septiembre a
/// agosto, un trimestre…). Los periodos de una empresa no se solapan. Cerrado, sus imputaciones no
/// se pueden cambiar (lo impide también la base de datos).
/// </summary>
public sealed class PeriodoAnalitico : RaizAgregadoEmpresa<Guid>
{
    private PeriodoAnalitico(Guid id)
        : base(id, Guid.Empty)
    {
        Codigo = null!;
        Nombre = null!;
    }

    private PeriodoAnalitico(Guid id, Guid empresaId, string codigo, string nombre, DateOnly desde, DateOnly hasta)
        : base(id, empresaId)
    {
        Codigo = codigo;
        Nombre = nombre;
        Desde = desde;
        Hasta = hasta;
    }

    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    public DateOnly Desde { get; private set; }

    public DateOnly Hasta { get; private set; }

    public bool Cerrado { get; private set; }

    public static Resultado<PeriodoAnalitico> Crear(Guid empresaId, string? codigo, string? nombre, DateOnly desde, DateOnly hasta)
    {
        var error = MaestroAnalitico.Validar(ref codigo, ref nombre, "periodo");
        if (error is null && hasta < desde)
        {
            error = Error.Validacion("periodo.fechas", "El periodo termina antes de empezar.");
        }

        return error is not null
            ? Resultado.Fallo<PeriodoAnalitico>(error)
            : Resultado.Ok(new PeriodoAnalitico(Guid.NewGuid(), empresaId, codigo!, nombre!, desde, hasta));
    }

    public void FijarCerrado(bool cerrado) => Cerrado = cerrado;

    public bool Contiene(DateOnly fecha) => fecha >= Desde && fecha <= Hasta;
}

/// <summary>
/// Imputación analítica: parte del importe de un apunte (o de un reparto secundario) asignada a un
/// centro y, opcionalmente, a una partida. El importe va con el signo natural: un gasto (grupo 6)
/// positivo es más coste; un ingreso (grupo 7) positivo es más ingreso.
/// </summary>
public sealed class ImputacionAnalitica : RaizAgregadoEmpresa<Guid>
{
    private ImputacionAnalitica(Guid id)
        : base(id, Guid.Empty)
    {
        CuentaCodigo = null!;
    }

    private ImputacionAnalitica(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
        CuentaCodigo = null!;
    }

    public Guid? ApunteId { get; private set; }

    public Guid? AsientoId { get; private set; }

    public DateOnly Fecha { get; private set; }

    public string CuentaCodigo { get; private set; }

    public NaturalezaAnalitica Naturaleza { get; private set; }

    public Guid CentroId { get; private set; }

    public Guid? PartidaId { get; private set; }

    public decimal Importe { get; private set; }

    public OrigenImputacion Origen { get; private set; }

    /// <summary>Ejecución (proceso o reparto secundario) que la generó; al deshacerla, se borra.</summary>
    public Guid? EjecucionId { get; private set; }

    /// <summary>Imputación de (parte de) un apunte.</summary>
    public static ImputacionAnalitica DeApunte(
        Guid empresaId, Guid apunteId, Guid asientoId, DateOnly fecha, string cuenta, Guid centroId, Guid? partidaId, decimal importe,
        OrigenImputacion origen, Guid? ejecucionId = null) =>
        new(Guid.NewGuid(), empresaId)
        {
            ApunteId = apunteId,
            AsientoId = asientoId,
            Fecha = fecha,
            CuentaCodigo = cuenta,
            Naturaleza = MotorAnalitico.NaturalezaDe(cuenta) ?? throw new ArgumentException("La cuenta no es de gastos ni de ingresos.", nameof(cuenta)),
            CentroId = centroId,
            PartidaId = partidaId,
            Importe = importe,
            Origen = origen,
            EjecucionId = ejecucionId,
        };

    /// <summary>Movimiento de un reparto secundario (sin apunte): salida del centro origen o llegada a un destino.</summary>
    public static ImputacionAnalitica DeReparto(
        Guid empresaId, Guid ejecucionId, DateOnly fecha, string cuenta, NaturalezaAnalitica naturaleza, Guid centroId, Guid? partidaId, decimal importe) =>
        new(Guid.NewGuid(), empresaId)
        {
            Fecha = fecha,
            CuentaCodigo = cuenta,
            Naturaleza = naturaleza,
            CentroId = centroId,
            PartidaId = partidaId,
            Importe = importe,
            Origen = OrigenImputacion.Reparto,
            EjecucionId = ejecucionId,
        };
}

/// <summary>Tipo de ejecución analítica.</summary>
public enum TipoEjecucionAnalitica
{
    /// <summary>Aplicación de las reglas a los apuntes pendientes de un periodo.</summary>
    ImputacionPendientes = 1,

    /// <summary>Reparto de los costes de un centro entre otros con una clave.</summary>
    RepartoSecundario = 2,
}

/// <summary>
/// Ejecución de un proceso analítico. Deja rastro de qué se generó y cuándo, y permite deshacerlo
/// entero (como los «repartos realizados» de Hispatec).
/// </summary>
public sealed class EjecucionAnalitica : RaizAgregadoEmpresa<Guid>
{
    private EjecucionAnalitica(Guid id)
        : base(id, Guid.Empty)
    {
        Descripcion = null!;
    }

    private EjecucionAnalitica(Guid id, Guid empresaId, TipoEjecucionAnalitica tipo, DateOnly desde, DateOnly hasta, string descripcion, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Tipo = tipo;
        Desde = desde;
        Hasta = hasta;
        Descripcion = descripcion;
        CreadoEn = ahora;
    }

    public TipoEjecucionAnalitica Tipo { get; private set; }

    public DateOnly Desde { get; private set; }

    public DateOnly Hasta { get; private set; }

    public string Descripcion { get; private set; }

    public Guid? CentroOrigenId { get; private set; }

    public Guid? ClaveRepartoId { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public static EjecucionAnalitica Imputacion(Guid empresaId, DateOnly desde, DateOnly hasta, DateTimeOffset ahora) =>
        new(Guid.NewGuid(), empresaId, TipoEjecucionAnalitica.ImputacionPendientes, desde, hasta, "Imputación de apuntes pendientes con las reglas", ahora);

    public static EjecucionAnalitica Reparto(Guid empresaId, DateOnly desde, DateOnly hasta, string descripcion, Guid centroOrigenId, Guid claveId, DateTimeOffset ahora) =>
        new(Guid.NewGuid(), empresaId, TipoEjecucionAnalitica.RepartoSecundario, desde, hasta, descripcion, ahora)
        {
            CentroOrigenId = centroOrigenId,
            ClaveRepartoId = claveId,
        };
}

/// <summary>Reglas de cálculo de la analítica (sin estado, para poder probarlas aisladas).</summary>
public static class MotorAnalitico
{
    /// <summary>Solo se imputan los gastos (grupo 6) y los ingresos (grupo 7).</summary>
    public static NaturalezaAnalitica? NaturalezaDe(string? cuenta) => cuenta?.Length > 0 ? cuenta[0] switch
    {
        '6' => NaturalezaAnalitica.Gasto,
        '7' => NaturalezaAnalitica.Ingreso,
        _ => null,
    } : null;

    /// <summary>Importe con signo natural: gasto = debe − haber; ingreso = haber − debe.</summary>
    public static decimal ImporteNatural(string cuenta, decimal debe, decimal haber) =>
        NaturalezaDe(cuenta) == NaturalezaAnalitica.Ingreso ? haber - debe : debe - haber;

    /// <summary>La regla que aplica (la más específica que se cumple), o null.</summary>
    public static ReglaAnalitica? ReglaQueAplica(IEnumerable<ReglaAnalitica> reglas, ContextoImputacion contexto) =>
        reglas.Where(r => r.Cumple(contexto)).OrderByDescending(r => r.Especificidad).FirstOrDefault();

    /// <summary>
    /// Reparte un importe por porcentajes al céntimo, sin perder ni sobrar nada: cada parte se trunca a
    /// céntimos y los céntimos que faltan van a las partes con mayor resto (método del mayor resto).
    /// La suma de las partes es exactamente el importe × Σ porcentajes / 100.
    /// </summary>
    public static IReadOnlyList<decimal> Repartir(decimal importe, IReadOnlyList<decimal> porcentajes)
    {
        ArgumentNullException.ThrowIfNull(porcentajes);
        if (porcentajes.Count == 0)
        {
            return [];
        }

        var signo = importe < 0m ? -1m : 1m;
        var totalCent = decimal.Round(Math.Abs(importe) * porcentajes.Sum() / 100m * 100m, 0, MidpointRounding.AwayFromZero);
        var exactos = porcentajes.Select(p => Math.Abs(importe) * p / 100m * 100m).ToList();
        var partes = exactos.Select(e => decimal.Floor(e)).ToList();
        var faltan = (int)(totalCent - partes.Sum());
        foreach (var i in exactos.Select((e, i) => (Resto: e - decimal.Floor(e), i)).OrderByDescending(x => x.Resto).ThenBy(x => x.i).Take(Math.Max(0, faltan)).Select(x => x.i))
        {
            partes[i] += 1m;
        }

        return partes.Select(p => signo * p / 100m).ToList();
    }
}
