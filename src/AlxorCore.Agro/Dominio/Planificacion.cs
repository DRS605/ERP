using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Agro.Dominio;

/// <summary>Qué se planifica.</summary>
public enum TipoPlan
{
    /// <summary>Ventas por periodo, producto y (opcional) cliente, con su precio previsto. Real: lo expedido.</summary>
    Comercial = 1,

    /// <summary>Producción (confección) por día o periodo, producto y (opcional) línea. Real: las salidas de los partes validados.</summary>
    Produccion = 2,

    /// <summary>Previsión de entradas (aforo de cosecha) por periodo, producto, agricultor y parcela. Real: lo recibido.</summary>
    Entradas = 3,
}

public enum EstadoPlan
{
    Borrador = 1,

    /// <summary>Aprobado: es el que se sigue (el cuadro de mando usa el último aprobado de cada tipo).</summary>
    Aprobado = 2,

    /// <summary>Sustituido por otra versión.</summary>
    Cerrado = 3,
}

/// <summary>Una línea del plan: un periodo, un producto y, según el tipo, un cliente, un agricultor, una parcela o una línea de confección.</summary>
public sealed record DatosLineaPlan(DateOnly Desde, DateOnly Hasta, Guid ProductoId, decimal Kilos, Guid? ClienteId = null, Guid? AgricultorId = null, Guid? ParcelaId = null,
    string? LineaConfeccion = null, int? Cajas = null, decimal? PrecioKg = null, string? Notas = null);

/// <summary>
/// Plan de la campaña: comercial, de producción o de entradas. Tiene versiones (se aprueba una y la anterior queda
/// cerrada) y líneas por periodo. El seguimiento compara cada línea con lo real, que se calcula en cada consulta.
/// </summary>
public sealed class Plan : RaizAgregadoEmpresa<Guid>
{
    private readonly List<LineaPlan> _lineas = [];

    private Plan(Guid id)
        : base(id, Guid.Empty)
    {
        Nombre = null!;
    }

    private Plan(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
        Nombre = null!;
    }

    public TipoPlan Tipo { get; private set; }

    public Guid CampanaId { get; private set; }

    public string Nombre { get; private set; }

    public int Version { get; private set; }

    public EstadoPlan Estado { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset? AprobadoEn { get; private set; }

    public IReadOnlyList<LineaPlan> Lineas => _lineas;

    public decimal Kilos => _lineas.Sum(l => l.Kilos);

    public static Resultado<Plan> Crear(Guid empresaId, TipoPlan tipo, Guid campanaId, string? nombre, int version, DateTimeOffset ahora)
    {
        if (!Enum.IsDefined(tipo))
        {
            return Resultado.Fallo<Plan>(Error.Validacion("plan.tipo", "El plan es comercial, de producción o de entradas."));
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            return Resultado.Fallo<Plan>(Error.Validacion("plan.nombre", "Pon un nombre al plan."));
        }

        return Resultado.Ok(new Plan(Guid.NewGuid(), empresaId)
        {
            Tipo = tipo, CampanaId = campanaId, Nombre = nombre.Trim().Length > 120 ? nombre.Trim()[..120] : nombre.Trim(), Version = version, Estado = EstadoPlan.Borrador,
            CreadoEn = ahora,
        });
    }

    /// <summary>Copia de este plan como versión nueva en borrador (para replanificar sin perder lo aprobado).</summary>
    public Plan NuevaVersion(int version, DateTimeOffset ahora)
    {
        var copia = new Plan(Guid.NewGuid(), EmpresaId)
        {
            Tipo = Tipo, CampanaId = CampanaId, Nombre = Nombre, Version = version, Estado = EstadoPlan.Borrador, CreadoEn = ahora,
        };
        copia._lineas.AddRange(_lineas.Select(l => l.Copia()));
        return copia;
    }

    public Resultado Renombrar(string? nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return Resultado.Fallo(Error.Validacion("plan.nombre", "Pon un nombre al plan."));
        }

        Nombre = nombre.Trim().Length > 120 ? nombre.Trim()[..120] : nombre.Trim();
        return Resultado.Ok();
    }

    /// <summary>Sustituye las líneas (el plan se edita como una hoja). Solo en borrador.</summary>
    public Resultado FijarLineas(IReadOnlyList<DatosLineaPlan> lineas, DateOnly campanaDesde, DateOnly campanaHasta)
    {
        ArgumentNullException.ThrowIfNull(lineas);
        if (Estado != EstadoPlan.Borrador)
        {
            return Resultado.Fallo(Error.Conflicto("plan.no_borrador", "El plan ya está aprobado: haz una versión nueva para cambiarlo."));
        }

        var nuevas = new List<LineaPlan>();
        foreach (var (d, i) in lineas.Select((d, i) => (d, i + 1)))
        {
            if (d.Hasta < d.Desde || d.Desde < campanaDesde || d.Hasta > campanaHasta)
            {
                return Resultado.Fallo(Error.Validacion("plan.periodo", $"Línea {i}: el periodo tiene que estar dentro de la campaña y acabar después de empezar."));
            }

            if (d.Kilos < 0m || decimal.Round(d.Kilos, 3) != d.Kilos || d.Cajas is < 0 || d.PrecioKg is < 0m)
            {
                return Resultado.Fallo(Error.Validacion("plan.kilos", $"Línea {i}: kilos, cajas y precio no pueden ser negativos (kilos con hasta 3 decimales)."));
            }

            var ajena = Tipo switch
            {
                TipoPlan.Comercial => d.AgricultorId is not null || d.ParcelaId is not null,
                TipoPlan.Produccion => d.AgricultorId is not null || d.ParcelaId is not null || d.ClienteId is not null,
                _ => d.ClienteId is not null || d.LineaConfeccion is not null,
            };
            if (ajena)
            {
                return Resultado.Fallo(Error.Validacion("plan.dimension", $"Línea {i}: ese dato no es de un plan {Texto(Tipo)}."));
            }

            nuevas.Add(new LineaPlan(Guid.NewGuid(), i, d));
        }

        _lineas.Clear();
        _lineas.AddRange(nuevas);
        return Resultado.Ok();
    }

    public Resultado Aprobar(DateTimeOffset ahora)
    {
        if (Estado != EstadoPlan.Borrador)
        {
            return Resultado.Fallo(Error.Conflicto("plan.no_borrador", "El plan no está en borrador."));
        }

        if (_lineas.Count == 0)
        {
            return Resultado.Fallo(Error.Validacion("plan.vacio", "El plan no tiene líneas."));
        }

        Estado = EstadoPlan.Aprobado;
        AprobadoEn = ahora;
        return Resultado.Ok();
    }

    public void Cerrar() => Estado = EstadoPlan.Cerrado;

    public static string Texto(TipoPlan tipo) => tipo switch
    {
        TipoPlan.Comercial => "comercial",
        TipoPlan.Produccion => "de producción",
        _ => "de entradas",
    };
}

public sealed class LineaPlan : EntidadBase<Guid>
{
    private LineaPlan(Guid id)
        : base(id)
    {
    }

    internal LineaPlan(Guid id, int numero, DatosLineaPlan d)
        : base(id)
    {
        Numero = numero;
        Desde = d.Desde;
        Hasta = d.Hasta;
        ProductoId = d.ProductoId;
        Kilos = d.Kilos;
        ClienteId = d.ClienteId;
        AgricultorId = d.AgricultorId;
        ParcelaId = d.ParcelaId;
        LineaConfeccion = string.IsNullOrWhiteSpace(d.LineaConfeccion) ? null : d.LineaConfeccion.Trim().Length > 40 ? d.LineaConfeccion.Trim()[..40] : d.LineaConfeccion.Trim();
        Cajas = d.Cajas;
        PrecioKg = d.PrecioKg;
        Notas = string.IsNullOrWhiteSpace(d.Notas) ? null : d.Notas.Trim().Length > 200 ? d.Notas.Trim()[..200] : d.Notas.Trim();
    }

    public int Numero { get; private set; }

    public DateOnly Desde { get; private set; }

    public DateOnly Hasta { get; private set; }

    public Guid ProductoId { get; private set; }

    public decimal Kilos { get; private set; }

    public Guid? ClienteId { get; private set; }

    public Guid? AgricultorId { get; private set; }

    public Guid? ParcelaId { get; private set; }

    public string? LineaConfeccion { get; private set; }

    public int? Cajas { get; private set; }

    public decimal? PrecioKg { get; private set; }

    public string? Notas { get; private set; }

    internal LineaPlan Copia() =>
        new(Guid.NewGuid(), Numero, new DatosLineaPlan(Desde, Hasta, ProductoId, Kilos, ClienteId, AgricultorId, ParcelaId, LineaConfeccion, Cajas, PrecioKg, Notas));

    /// <summary>¿Le toca este hecho real? Y cuánto de concreta es (para dárselo a la más concreta).</summary>
    public int Encaje(HechoReal h)
    {
        ArgumentNullException.ThrowIfNull(h);
        if (h.ProductoId != ProductoId || h.Fecha < Desde || h.Fecha > Hasta)
        {
            return 0;
        }

        if ((ClienteId is not null && ClienteId != h.ClienteId) || (AgricultorId is not null && AgricultorId != h.AgricultorId) || (ParcelaId is not null && ParcelaId != h.ParcelaId))
        {
            return 0;
        }

        // Más concreta: con parcela, agricultor o cliente; a igualdad, la de periodo más corto.
        return 1_000_000 + (ParcelaId is not null ? 300_000 : 0) + (AgricultorId is not null ? 200_000 : 0) + (ClienteId is not null ? 200_000 : 0)
            - Math.Min(99_999, Hasta.DayNumber - Desde.DayNumber);
    }
}

/// <summary>Un hecho real (una expedición, una salida de confección o una entrada) para comparar con el plan.</summary>
public sealed record HechoReal(DateOnly Fecha, Guid ProductoId, decimal Kilos, int Cajas, Guid? ClienteId = null, Guid? AgricultorId = null, Guid? ParcelaId = null);

/// <summary>Reparte los hechos reales entre las líneas del plan: cada hecho va a la línea más concreta que le toca; el resto queda fuera de plan.</summary>
public static class SeguimientoPlan
{
    public static (IReadOnlyDictionary<Guid, (decimal Kilos, int Cajas)> PorLinea, IReadOnlyList<HechoReal> FueraDePlan) Repartir(IReadOnlyList<LineaPlan> lineas,
        IEnumerable<HechoReal> hechos)
    {
        ArgumentNullException.ThrowIfNull(lineas);
        ArgumentNullException.ThrowIfNull(hechos);
        var por = lineas.ToDictionary(l => l.Id, _ => (0m, 0));
        var fuera = new List<HechoReal>();
        foreach (var h in hechos)
        {
            var mejor = lineas.Select(l => (l, e: l.Encaje(h))).Where(x => x.e > 0).OrderByDescending(x => x.e).ThenBy(x => x.l.Numero).Select(x => x.l).FirstOrDefault();
            if (mejor is null)
            {
                fuera.Add(h);
                continue;
            }

            var (k, c) = por[mejor.Id];
            por[mejor.Id] = (k + h.Kilos, c + h.Cajas);
        }

        return (por, fuera);
    }
}
