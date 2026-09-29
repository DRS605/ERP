using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Cooperativa.Dominio;

public enum EstadoReparto
{
    /// <summary>Propuesta: se recalcula tantas veces como haga falta hasta que la asamblea la aprueba.</summary>
    Borrador = 1,

    /// <summary>Aprobado y contabilizado: el retorno capitalizado ya está en el capital de cada socio. No cambia.</summary>
    Contabilizado = 2,

    /// <summary>Anulado (con su asiento y sus capitalizaciones).</summary>
    Anulado = 3,
}

/// <summary>Lo que entra en el cálculo de un socio: su actividad, su capital y si deja el retorno como capital.</summary>
public sealed record SocioEnReparto(Guid SocioId, string Nombre, decimal Actividad, decimal CapitalDesembolsado, bool Capitalizar);

/// <summary>
/// Distribución del excedente de un ejercicio: fondo de reserva obligatorio (FRO), fondo de educación y promoción (FEP),
/// intereses al capital, reservas voluntarias y retorno cooperativo a los socios en proporción a su actividad (o a su
/// capital, en una SAT). El retorno se paga (con retención) o se deja como capital.
/// </summary>
public sealed class Reparto : RaizAgregadoEmpresa<Guid>
{
    private readonly List<LineaReparto> _lineas = [];

    private Reparto(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private Reparto(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
    }

    public int Ejercicio { get; private set; }

    /// <summary>Fecha del acuerdo (la del asiento y de las capitalizaciones).</summary>
    public DateOnly Fecha { get; private set; }

    public DateOnly? FechaAsamblea { get; private set; }

    public EstadoReparto Estado { get; private set; }

    public BaseRetorno Base { get; private set; }

    public decimal Excedente { get; private set; }

    public decimal PorcentajeFro { get; private set; }

    public decimal PorcentajeFep { get; private set; }

    public decimal ImporteFro { get; private set; }

    public decimal ImporteFep { get; private set; }

    public decimal ReservasVoluntarias { get; private set; }

    public decimal PorcentajeIntereses { get; private set; }

    public decimal ImporteIntereses { get; private set; }

    public decimal ImporteRetorno { get; private set; }

    public decimal PorcentajeRetencion { get; private set; }

    public string? Observaciones { get; private set; }

    public Guid? AsientoId { get; private set; }

    public string? MotivoAnulacion { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public IReadOnlyList<LineaReparto> Lineas => _lineas;

    public decimal TotalRetencion => _lineas.Sum(l => l.Retencion);

    public decimal TotalCapitalizado => _lineas.Sum(l => l.Capitalizado);

    public decimal TotalNeto => _lineas.Sum(l => l.Neto);

    public static Reparto Nuevo(Guid empresaId, int ejercicio, DateTimeOffset ahora) =>
        new(Guid.NewGuid(), empresaId) { Ejercicio = ejercicio, Estado = EstadoReparto.Borrador, CreadoEn = ahora };

    /// <summary>
    /// Calcula el reparto. Los fondos salen del excedente con sus porcentajes (que no pueden bajar de los mínimos); los
    /// intereses, del capital desembolsado; lo que queda tras las reservas voluntarias es el retorno, que se reparte por
    /// actividad al céntimo (el redondeo se lo lleva el de más actividad). La retención se aplica a lo que se paga, no a
    /// lo que se capitaliza.
    /// </summary>
    public Resultado Calcular(ConfiguracionCooperativa config, DateOnly fecha, DateOnly? fechaAsamblea, decimal excedente, decimal? porcentajeFro, decimal? porcentajeFep,
        decimal reservasVoluntarias, decimal porcentajeIntereses, IReadOnlyList<SocioEnReparto> socios, string? observaciones)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(socios);
        if (Estado != EstadoReparto.Borrador)
        {
            return Resultado.Fallo(Error.Conflicto("reparto.no_borrador", "El reparto ya está contabilizado: anúlalo para rehacerlo."));
        }

        if (fecha.Year < Ejercicio)
        {
            return Resultado.Fallo(Error.Validacion("reparto.fecha", "El reparto se acuerda después de cerrar el ejercicio que reparte."));
        }

        var fro = porcentajeFro ?? config.PorcentajeFroMinimo;
        var fep = porcentajeFep ?? config.PorcentajeFepMinimo;
        if (!Dos(excedente) || excedente <= 0m)
        {
            return Resultado.Fallo(Error.Validacion("reparto.excedente", "Solo se reparte un excedente positivo (con dos decimales); las pérdidas se imputan aparte."));
        }

        if (fro < config.PorcentajeFroMinimo || fep < config.PorcentajeFepMinimo || fro + fep > 100m)
        {
            return Resultado.Fallo(Error.Validacion("reparto.fondos",
                $"El fondo de reserva lleva al menos el {Redondeo.Formatear(config.PorcentajeFroMinimo, 2)} % y el de educación el {Redondeo.Formatear(config.PorcentajeFepMinimo, 2)} %."));
        }

        if (porcentajeIntereses < 0m || porcentajeIntereses > config.InteresMaximoCapital)
        {
            return Resultado.Fallo(Error.Validacion("reparto.intereses",
                $"El interés al capital va de 0 al {Redondeo.Formatear(config.InteresMaximoCapital, 2)} % (máximo de los ajustes)."));
        }

        if (!Dos(reservasVoluntarias) || reservasVoluntarias < 0m)
        {
            return Resultado.Fallo(Error.Validacion("reparto.reservas", "Las reservas voluntarias no pueden ser negativas."));
        }

        if (socios.Any(s => s.Actividad < 0m || s.CapitalDesembolsado < 0m) || socios.Select(s => s.SocioId).Distinct().Count() != socios.Count)
        {
            return Resultado.Fallo(Error.Validacion("reparto.socios", "Cada socio va una vez, con actividad y capital no negativos."));
        }

        var importeFro = Redondeo.Dos(excedente * fro / 100m);
        var importeFep = Redondeo.Dos(excedente * fep / 100m);
        var intereses = socios.ToDictionary(s => s.SocioId, s => Redondeo.Dos(s.CapitalDesembolsado * porcentajeIntereses / 100m));
        var totalIntereses = intereses.Values.Sum();
        var retorno = excedente - importeFro - importeFep - reservasVoluntarias - totalIntereses;
        if (retorno < 0m)
        {
            return Resultado.Fallo(Error.Validacion("reparto.insuficiente",
                $"El excedente no llega para los fondos, las reservas y los intereses (faltan {Redondeo.Formatear(-retorno, 2)} €)."));
        }

        var actividad = socios.Sum(s => s.Actividad);
        if (retorno > 0m && actividad == 0m)
        {
            return Resultado.Fallo(Error.Validacion("reparto.sin_actividad",
                "Ningún socio con derecho a retorno tiene actividad en el ejercicio: pasa lo que queda a reservas voluntarias."));
        }

        // Retorno por actividad, al céntimo; la diferencia de redondeo va al de más actividad.
        var retornos = socios.ToDictionary(s => s.SocioId, s => actividad == 0m ? 0m : Math.Round(retorno * s.Actividad / actividad, 2, MidpointRounding.ToZero));
        if (retorno > 0m)
        {
            var mayor = socios.OrderByDescending(s => s.Actividad).ThenBy(s => s.SocioId).First().SocioId;
            retornos[mayor] += retorno - retornos.Values.Sum();
        }

        _lineas.Clear();
        foreach (var s in socios)
        {
            var r = retornos[s.SocioId];
            var i = intereses[s.SocioId];
            if (r == 0m && i == 0m)
            {
                continue;
            }

            var capitalizado = s.Capitalizar ? r : 0m;
            var retencion = Redondeo.Dos((r - capitalizado + i) * config.PorcentajeRetencion / 100m);
            _lineas.Add(new LineaReparto(Guid.NewGuid(), _lineas.Count + 1, s.SocioId, s.Nombre, s.Actividad, s.CapitalDesembolsado, i, r, retencion, capitalizado,
                r + i - retencion - capitalizado));
        }

        Fecha = fecha;
        FechaAsamblea = fechaAsamblea;
        Base = config.Base;
        Excedente = excedente;
        PorcentajeFro = fro;
        PorcentajeFep = fep;
        ImporteFro = importeFro;
        ImporteFep = importeFep;
        ReservasVoluntarias = reservasVoluntarias;
        PorcentajeIntereses = porcentajeIntereses;
        ImporteIntereses = totalIntereses;
        ImporteRetorno = retorno;
        PorcentajeRetencion = config.PorcentajeRetencion;
        Observaciones = Socio.Recortar(observaciones, 500);
        return Resultado.Ok();
    }

    public Resultado Contabilizar(Guid? asientoId)
    {
        if (Estado != EstadoReparto.Borrador)
        {
            return Resultado.Fallo(Error.Conflicto("reparto.no_borrador", "El reparto ya está contabilizado o anulado."));
        }

        if (Excedente <= 0m)
        {
            return Resultado.Fallo(Error.Validacion("reparto.sin_calcular", "Calcula el reparto antes de contabilizarlo."));
        }

        Estado = EstadoReparto.Contabilizado;
        AsientoId = asientoId;
        return Resultado.Ok();
    }

    public Resultado Anular(string? motivo)
    {
        if (Estado != EstadoReparto.Contabilizado)
        {
            return Resultado.Fallo(Error.Conflicto("reparto.no_contabilizado", "Solo se anula un reparto contabilizado (el borrador se elimina)."));
        }

        if (string.IsNullOrWhiteSpace(motivo))
        {
            return Resultado.Fallo(Error.Validacion("reparto.motivo", "Indica el motivo de la anulación."));
        }

        Estado = EstadoReparto.Anulado;
        MotivoAnulacion = Socio.Recortar(motivo, 200);
        return Resultado.Ok();
    }

    private static bool Dos(decimal v) => Redondeo.Dos(v) == v;
}

/// <summary>Lo que le toca a un socio en el reparto.</summary>
public sealed class LineaReparto : EntidadBase<Guid>
{
    private LineaReparto(Guid id)
        : base(id)
    {
        Nombre = null!;
    }

    internal LineaReparto(Guid id, int numero, Guid socioId, string nombre, decimal actividad, decimal capital, decimal intereses, decimal retorno, decimal retencion,
        decimal capitalizado, decimal neto)
        : base(id)
    {
        Numero = numero;
        SocioId = socioId;
        Nombre = Socio.Recortar(nombre, 200) ?? "-";
        Actividad = actividad;
        Capital = capital;
        Intereses = intereses;
        Retorno = retorno;
        Retencion = retencion;
        Capitalizado = capitalizado;
        Neto = neto;
    }

    public int Numero { get; private set; }

    public Guid SocioId { get; private set; }

    public string Nombre { get; private set; }

    /// <summary>Kilos, euros o capital, según la base del reparto.</summary>
    public decimal Actividad { get; private set; }

    /// <summary>Capital desembolsado sobre el que se calculan los intereses.</summary>
    public decimal Capital { get; private set; }

    public decimal Intereses { get; private set; }

    public decimal Retorno { get; private set; }

    public decimal Retencion { get; private set; }

    /// <summary>Parte del retorno que queda como capital del socio.</summary>
    public decimal Capitalizado { get; private set; }

    /// <summary>Lo que se le paga: retorno e intereses, menos la retención y lo capitalizado.</summary>
    public decimal Neto { get; private set; }
}
