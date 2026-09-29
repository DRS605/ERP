using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Agro.Dominio;

public enum EstadoParte
{
    /// <summary>Se está rellenando: no mueve partidas.</summary>
    Borrador = 1,

    /// <summary>Validado: consumió sus partidas, creó las de salida con su coste y quedó numerado.</summary>
    Validado = 2,

    /// <summary>Anulado: sus movimientos se invirtieron (solo si sus salidas no se han usado).</summary>
    Anulado = 3,
}

/// <summary>Cómo se reparte el coste del parte entre sus salidas.</summary>
public enum RepartoCoste
{
    /// <summary>En proporción a los kilos.</summary>
    PorKilos = 1,

    /// <summary>En proporción a kilos × factor (por ejemplo, la 1ª vale más que la 2ª y el destrío, factor 0).</summary>
    PorFactor = 2,

    /// <summary>
    /// Como Hispatec: la mano de obra <b>de confección</b> se reparte por el tiempo teórico de cada salida
    /// (cajas × 3600 / rendimiento en cajas por hora); todo lo demás (fruta, materiales, apoyo, maquinaria, indirectos), por kilos.
    /// </summary>
    PorTiempoTeorico = 3,
}

/// <summary>
/// Parte de confección: transforma partidas de fruta (consumos) en partidas de producto confeccionado
/// (salidas) con materiales, mano de obra y maquinaria. Valida y valora como el Clon: tarifas vigentes
/// obligatorias (nunca coste 0 por olvido), destajo por piezas, coste de materiales conocido e
/// indirectos sobre mano de obra y maquinaria. El coste se reparte entre las salidas al céntimo.
/// </summary>
public sealed class ParteConfeccion : RaizAgregadoEmpresa<Guid>
{
    public const string Serie = "PC";

    private readonly List<ConsumoParte> _consumos = [];
    private readonly List<ManoObraParte> _manoObra = [];
    private readonly List<MaquinaParte> _maquinas = [];
    private readonly List<MaterialParte> _materiales = [];
    private readonly List<SalidaParte> _salidas = [];

    private ParteConfeccion(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private ParteConfeccion(Guid id, Guid empresaId, DateOnly fecha, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Fecha = fecha;
        Ejercicio = fecha.Year;
        Estado = EstadoParte.Borrador;
        Reparto = RepartoCoste.PorKilos;
        CreadoEn = ahora;
    }

    public DateOnly Fecha { get; private set; }

    public int Ejercicio { get; private set; }

    public int? Numero { get; private set; }

    public string? NumeroCompleto => Numero is { } n ? $"{Serie}-{Ejercicio}-{n:D6}" : null;

    public Guid? CampanaId { get; private set; }

    public string? Descripcion { get; private set; }

    /// <summary>Centro analítico del almacén o la línea de confección (para el informe de coste por kilo).</summary>
    public Guid? CentroAnaliticoId { get; private set; }

    public decimal PorcentajeIndirectos { get; private set; }

    public RepartoCoste Reparto { get; private set; }

    public EstadoParte Estado { get; private set; }

    public decimal CosteFruta { get; private set; }

    public decimal CosteMateriales { get; private set; }

    public decimal CosteManoObra { get; private set; }

    public decimal CosteMaquinaria { get; private set; }

    public decimal CosteIndirectos { get; private set; }

    public decimal CosteTotal { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset? ValidadoEn { get; private set; }

    public IReadOnlyList<ConsumoParte> Consumos => _consumos;

    public IReadOnlyList<ManoObraParte> ManoObra => _manoObra;

    public IReadOnlyList<MaquinaParte> Maquinas => _maquinas;

    public IReadOnlyList<MaterialParte> Materiales => _materiales;

    public IReadOnlyList<SalidaParte> Salidas => _salidas;

    public decimal KilosConsumidos => _consumos.Sum(c => c.Kilos);

    public decimal KilosObtenidos => _salidas.Sum(s => s.Kilos);

    /// <summary>Merma: kilos consumidos que no salen como producto (ni como destrío).</summary>
    public decimal Merma => KilosConsumidos - KilosObtenidos;

    /// <summary>Merma en % de lo consumido.</summary>
    public decimal PorcentajeMerma => KilosConsumidos == 0m ? 0m : decimal.Round(Merma * 100m / KilosConsumidos, 2);

    /// <summary>Quién aprobó una merma por encima de la tolerancia, y por qué.</summary>
    public Guid? MermaAprobadaPorId { get; private set; }

    public string? MermaAprobadaPor { get; private set; }

    public string? MotivoMerma { get; private set; }

    public DateTimeOffset? MermaAprobadaEn { get; private set; }

    /// <summary>Merma (en %) que se aprobó: si el parte cambia y la merma sube, la aprobación ya no vale.</summary>
    public decimal? MermaAprobadaPct { get; private set; }

    public bool MermaAprobada => MermaAprobadaPct is { } a && PorcentajeMerma <= a;

    /// <summary>Tolerancia de merma aplicada al valorar el parte (la de su transformación, su familia o la general).</summary>
    public decimal? ToleranciaMermaPct { get; private set; }

    public void FijarToleranciaMerma(decimal? tolerancia)
    {
        if (Estado == EstadoParte.Borrador)
        {
            ToleranciaMermaPct = tolerancia;
        }
    }

    /// <summary>Aprueba la merma actual del borrador (por encima de la tolerancia), con motivo.</summary>
    public Resultado AprobarMerma(Guid? usuarioId, string? usuario, string? motivo, DateTimeOffset ahora)
    {
        if (Estado != EstadoParte.Borrador)
        {
            return Resultado.Fallo(Error.Conflicto("parte.no_borrador", "El parte ya está validado o anulado."));
        }

        if (string.IsNullOrWhiteSpace(motivo))
        {
            return Resultado.Fallo(Error.Validacion("parte.motivo_merma", "Indica el motivo de la merma (destrío en campo, podrido, calibre…)."));
        }

        MermaAprobadaPorId = usuarioId;
        MermaAprobadaPor = string.IsNullOrWhiteSpace(usuario) ? null : usuario.Trim().Length > 150 ? usuario.Trim()[..150] : usuario.Trim();
        MotivoMerma = motivo.Trim().Length > 300 ? motivo.Trim()[..300] : motivo.Trim();
        MermaAprobadaEn = ahora;
        MermaAprobadaPct = PorcentajeMerma;
        return Resultado.Ok();
    }

    public static ParteConfeccion Crear(Guid empresaId, DateOnly fecha, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        return new ParteConfeccion(Guid.NewGuid(), empresaId, fecha, reloj.AhoraUtc);
    }

    /// <summary>Sustituye el contenido del borrador.</summary>
    public Resultado Fijar(DatosParte datos)
    {
        ArgumentNullException.ThrowIfNull(datos);
        if (Estado != EstadoParte.Borrador)
        {
            return Resultado.Fallo(Error.Conflicto("parte.no_borrador", "El parte ya está validado o anulado: no se puede modificar."));
        }

        if (datos.PorcentajeIndirectos is < 0m or > 1000m || !Enum.IsDefined(datos.Reparto))
        {
            return Resultado.Fallo(Error.Validacion("parte.indirectos", "El porcentaje de indirectos o el reparto no son válidos."));
        }

        if (datos.Consumos.Any(c => c.Kilos <= 0m || decimal.Round(c.Kilos, 3) != c.Kilos) ||
            datos.Salidas.Any(s => s.Kilos <= 0m || decimal.Round(s.Kilos, 3) != s.Kilos || s.Factor < 0m) ||
            datos.ManoObra.Any(m => m.Horas < 0m || m.Piezas < 0m) || datos.Maquinas.Any(m => m.Horas <= 0m) || datos.Salidas.Any(s => s.Cajas < 0) ||
            datos.Materiales.Any(m => m.Cantidad <= 0m))
        {
            return Resultado.Fallo(Error.Validacion("parte.cantidades", "Las cantidades deben ser positivas (los kilos, hasta 3 decimales) y los factores no negativos."));
        }

        Fecha = datos.Fecha;
        Ejercicio = datos.Fecha.Year;
        CampanaId = datos.CampanaId;
        Descripcion = string.IsNullOrWhiteSpace(datos.Descripcion) ? null : datos.Descripcion.Trim();
        CentroAnaliticoId = datos.CentroAnaliticoId;
        PorcentajeIndirectos = datos.PorcentajeIndirectos;
        Reparto = datos.Reparto;
        _consumos.Clear();
        _consumos.AddRange(datos.Consumos.Select(c => new ConsumoParte(Guid.NewGuid(), c.PartidaId, c.PaleId, c.Kilos)));
        _manoObra.Clear();
        _manoObra.AddRange(datos.ManoObra.Select(m => new ManoObraParte(Guid.NewGuid(), m.Descripcion, m.Categoria.Trim().ToUpperInvariant(), m.TipoHora, m.Horas, m.Piezas, m.Confeccion)));
        _maquinas.Clear();
        _maquinas.AddRange(datos.Maquinas.Select(m => new MaquinaParte(Guid.NewGuid(), m.Descripcion, m.Categoria.Trim().ToUpperInvariant(), m.Horas)));
        _materiales.Clear();
        _materiales.AddRange(datos.Materiales.Select(m => new MaterialParte(Guid.NewGuid(), m.ProductoId, m.Nombre, m.Cantidad)));
        _salidas.Clear();
        _salidas.AddRange(datos.Salidas.Select((s, i) => new SalidaParte(Guid.NewGuid(), i + 1, s.ProductoId, s.Nombre, s.Kilos, s.Factor, s.Calibre, s.CategoriaId, s.PaleId, s.Cajas, s.EnvaseProductoId, s.MotivoDescalificacion)));
        return Resultado.Ok();
    }

    /// <summary>
    /// Valora el parte y devuelve todos los errores a la vez. <paramref name="costeKgPartida"/> da el
    /// coste por kilo de cada partida consumida (null si no se conoce).
    /// </summary>
    public IReadOnlyList<Error> Valorar(IReadOnlyList<TarifaCoste> tarifas, Func<Guid, decimal?> costeKgPartida, Func<Guid, decimal?> costeMaterial,
        Func<Guid, Guid?, decimal?>? rendimiento = null)
    {
        ArgumentNullException.ThrowIfNull(tarifas);
        ArgumentNullException.ThrowIfNull(costeKgPartida);
        ArgumentNullException.ThrowIfNull(costeMaterial);
        var errores = new List<Error>();
        if (Estado != EstadoParte.Borrador)
        {
            return [Error.Conflicto("parte.no_borrador", "El parte ya está validado o anulado.")];
        }

        if (_consumos.Count == 0)
        {
            errores.Add(Error.Validacion("parte.sin_consumos", "El parte no consume ninguna partida."));
        }

        if (_salidas.Count == 0)
        {
            errores.Add(Error.Validacion("parte.sin_salidas", "El parte no obtiene ningún producto."));
        }

        if (KilosObtenidos > KilosConsumidos)
        {
            errores.Add(Error.Validacion("parte.kilos_salida", $"Se obtienen más kilos ({Redondeo.Formatear(KilosObtenidos, 3)}) de los que se consumen ({Redondeo.Formatear(KilosConsumidos, 3)})."));
        }

        if (Reparto == RepartoCoste.PorFactor && _salidas.All(s => s.Factor == 0m))
        {
            errores.Add(Error.Validacion("parte.factores", "Con reparto por factor, alguna salida debe tener factor mayor que cero."));
        }

        TarifaCoste? Tarifa(RecursoCoste recurso, string categoria, TipoHora tipo, string etiqueta)
        {
            var t = tarifas.Where(x => x.Recurso == recurso && x.Categoria == categoria && x.TipoHora == tipo && x.Vigente(Fecha))
                .OrderByDescending(x => x.Desde).FirstOrDefault();
            if (t is null)
            {
                var clase = recurso == RecursoCoste.ManoObra ? "mano de obra" : "maquinaria";
                errores.Add(Error.Validacion("parte.sin_tarifa", $"No hay tarifa de {clase} para {etiqueta} (categoría {categoria}, {tipo}) vigente el {Fecha:dd/MM/yyyy}."));
            }

            return t;
        }

        foreach (var c in _consumos)
        {
            var coste = costeKgPartida(c.PartidaId);
            if (coste is null)
            {
                errores.Add(Error.Validacion("parte.coste_fruta", "Una partida consumida no tiene coste: ni está liquidada ni su recepción indica precio estimado."));
            }

            c.Valorar(coste ?? 0m);
        }

        foreach (var m in _manoObra)
        {
            var destajo = m.TipoHora == TipoHora.Destajo;
            if (destajo && m.Piezas is not > 0m)
            {
                errores.Add(Error.Validacion("parte.piezas", $"{m.Descripcion}: el destajo necesita las piezas producidas."));
            }

            if (!destajo && m.Piezas is not null)
            {
                errores.Add(Error.Validacion("parte.piezas_sin_destajo", $"{m.Descripcion}: las piezas solo se indican a destajo."));
            }

            var t = Tarifa(RecursoCoste.ManoObra, m.Categoria, m.TipoHora, m.Descripcion);
            m.Valorar(t?.Id, t?.CosteUnitario ?? 0m);
        }

        foreach (var m in _maquinas)
        {
            var t = Tarifa(RecursoCoste.Maquina, m.Categoria, TipoHora.Normal, m.Descripcion);
            m.Valorar(t?.Id, t?.CosteUnitario ?? 0m);
        }

        foreach (var m in _materiales)
        {
            var coste = costeMaterial(m.ProductoId);
            if (coste is null)
            {
                errores.Add(Error.Validacion("parte.coste_material", $"No se conoce el coste de {m.Nombre}: indica su precio de compra en el catálogo."));
            }

            m.Valorar(coste ?? 0m);
        }

        if (errores.Count > 0)
        {
            return errores;
        }

        CosteFruta = _consumos.Sum(c => c.Coste);
        CosteMateriales = _materiales.Sum(m => m.Coste);
        CosteManoObra = _manoObra.Sum(m => m.Coste);
        CosteMaquinaria = _maquinas.Sum(m => m.Coste);
        CosteIndirectos = Redondeo.Dos((CosteManoObra + CosteMaquinaria) * PorcentajeIndirectos / 100m);
        CosteTotal = CosteFruta + CosteMateriales + CosteManoObra + CosteMaquinaria + CosteIndirectos;

        if (Reparto == RepartoCoste.PorTiempoTeorico)
        {
            return RepartirPorTiempo(rendimiento);
        }

        var pesos = _salidas.Select(s => Reparto == RepartoCoste.PorFactor ? s.Kilos * s.Factor : s.Kilos).ToList();
        var partes = ReglasAgro.Repartir(CosteTotal, pesos, 2);
        for (var i = 0; i < _salidas.Count; i++)
        {
            _salidas[i].Valorar(partes[i]);
        }

        return errores;
    }

    /// <summary>
    /// Reparto por tiempo teórico: la mano de obra de confección, por cajas × 3600 / rendimiento de cada salida; lo demás,
    /// por kilos. Una salida con kilos que no tiene cajas o rendimiento es un error, nunca un cero (si hay confección que repartir).
    /// </summary>
    private List<Error> RepartirPorTiempo(Func<Guid, Guid?, decimal?>? rendimiento)
    {
        var errores = new List<Error>();
        var confeccion = _manoObra.Where(m => m.Confeccion).Sum(m => m.Coste);
        var segundos = new List<decimal>();
        foreach (var s in _salidas)
        {
            var r = rendimiento?.Invoke(s.ProductoId, s.EnvaseProductoId);
            if (confeccion > 0m && (s.Cajas is not > 0 || r is not > 0m))
            {
                errores.Add(Error.Validacion("parte.sin_rendimiento", s.Cajas is not > 0
                    ? $"{s.Nombre}: indica las cajas confeccionadas para repartir la mano de obra por tiempo."
                    : $"{s.Nombre}: no hay rendimiento de confección (cajas por hora) para su producto y envase."));
            }

            segundos.Add(s.Cajas is > 0 && r is > 0m ? decimal.Round(s.Cajas.Value * 3600m / r.Value, 2) : 0m);
        }

        if (errores.Count > 0)
        {
            return errores;
        }

        var porKilos = ReglasAgro.Repartir(CosteTotal - confeccion, _salidas.Select(s => s.Kilos).ToList(), 2);
        IReadOnlyList<decimal> porTiempo = confeccion > 0m ? ReglasAgro.Repartir(confeccion, segundos, 2) : _salidas.Select(_ => 0m).ToList();
        for (var i = 0; i < _salidas.Count; i++)
        {
            _salidas[i].Valorar(porKilos[i] + porTiempo[i], porTiempo[i], segundos[i] > 0m ? segundos[i] : null);
        }

        return errores;
    }

    public Resultado Validar(int numero, IReadOnlyDictionary<Guid, Guid> partidaPorSalida, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(partidaPorSalida);
        ArgumentNullException.ThrowIfNull(reloj);
        if (Estado != EstadoParte.Borrador)
        {
            return Resultado.Fallo(Error.Conflicto("parte.no_borrador", "El parte ya está validado o anulado."));
        }

        foreach (var s in _salidas)
        {
            s.AsignarPartida(partidaPorSalida[s.Id]);
        }

        Numero = numero;
        Estado = EstadoParte.Validado;
        ValidadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    public Resultado Anular()
    {
        if (Estado != EstadoParte.Validado)
        {
            return Resultado.Fallo(Error.Conflicto("parte.no_validado", "Solo se anula un parte validado (un borrador se elimina)."));
        }

        Estado = EstadoParte.Anulado;
        return Resultado.Ok();
    }
}

public sealed record DatosParte(
    DateOnly Fecha, Guid? CampanaId, string? Descripcion, Guid? CentroAnaliticoId, decimal PorcentajeIndirectos, RepartoCoste Reparto,
    IReadOnlyList<DatosConsumo> Consumos, IReadOnlyList<DatosManoObra> ManoObra, IReadOnlyList<DatosMaquina> Maquinas,
    IReadOnlyList<DatosMaterial> Materiales, IReadOnlyList<DatosSalida> Salidas);

public sealed record DatosConsumo(Guid PartidaId, decimal Kilos, Guid? PaleId = null);

public sealed record DatosManoObra(string Descripcion, string Categoria, TipoHora TipoHora, decimal Horas, decimal? Piezas = null, bool Confeccion = true);

public sealed record DatosMaquina(string Descripcion, string Categoria, decimal Horas);

public sealed record DatosMaterial(Guid ProductoId, string Nombre, decimal Cantidad);

public sealed record DatosSalida(Guid ProductoId, string Nombre, decimal Kilos, decimal Factor = 1m, string? Calibre = null, Guid? CategoriaId = null, Guid? PaleId = null,
    int? Cajas = null, Guid? EnvaseProductoId = null, string? MotivoDescalificacion = null);

public sealed class ConsumoParte : EntidadBase<Guid>
{
    private ConsumoParte(Guid id)
        : base(id)
    {
    }

    internal ConsumoParte(Guid id, Guid partidaId, Guid? paleId, decimal kilos)
        : base(id)
    {
        PartidaId = partidaId;
        PaleId = paleId;
        Kilos = kilos;
    }

    public Guid PartidaId { get; private set; }

    /// <summary>Palé del que se toman los kilos (null: kilos sueltos de la partida).</summary>
    public Guid? PaleId { get; private set; }

    public decimal Kilos { get; private set; }

    public decimal CosteKg { get; private set; }

    public decimal Coste { get; private set; }

    internal void Valorar(decimal costeKg)
    {
        CosteKg = costeKg;
        Coste = Redondeo.Dos(Kilos * costeKg);
    }
}

public sealed class ManoObraParte : EntidadBase<Guid>
{
    private ManoObraParte(Guid id)
        : base(id)
    {
        Descripcion = null!;
        Categoria = null!;
    }

    internal ManoObraParte(Guid id, string descripcion, string categoria, TipoHora tipoHora, decimal horas, decimal? piezas, bool confeccion = true)
        : base(id)
    {
        Confeccion = confeccion;
        Descripcion = string.IsNullOrWhiteSpace(descripcion) ? categoria : descripcion.Trim();
        Categoria = categoria;
        TipoHora = tipoHora;
        Horas = horas;
        Piezas = piezas;
    }

    /// <summary>Quién (cuadrilla, turno…): el parte recoge el coste de la mano de obra, no gestiona personal.</summary>
    public string Descripcion { get; private set; }

    public string Categoria { get; private set; }

    public TipoHora TipoHora { get; private set; }

    /// <summary>Horas trabajadas (a destajo se registran, pero se paga por piezas).</summary>
    public decimal Horas { get; private set; }

    public decimal? Piezas { get; private set; }

    /// <summary>Confecciona (se reparte por tiempo teórico) o es de apoyo (por kilos), como el <c>IntervieneTiempoConfeccion</c> de Hispatec.</summary>
    public bool Confeccion { get; private set; } = true;

    public Guid? TarifaId { get; private set; }

    public decimal CosteUnitario { get; private set; }

    public decimal Coste { get; private set; }

    internal void Valorar(Guid? tarifaId, decimal costeUnitario)
    {
        TarifaId = tarifaId;
        CosteUnitario = costeUnitario;
        Coste = Redondeo.Dos((TipoHora == TipoHora.Destajo ? Piezas ?? 0m : Horas) * costeUnitario);
    }
}

public sealed class MaquinaParte : EntidadBase<Guid>
{
    private MaquinaParte(Guid id)
        : base(id)
    {
        Descripcion = null!;
        Categoria = null!;
    }

    internal MaquinaParte(Guid id, string descripcion, string categoria, decimal horas)
        : base(id)
    {
        Descripcion = string.IsNullOrWhiteSpace(descripcion) ? categoria : descripcion.Trim();
        Categoria = categoria;
        Horas = horas;
    }

    public string Descripcion { get; private set; }

    public string Categoria { get; private set; }

    public decimal Horas { get; private set; }

    public Guid? TarifaId { get; private set; }

    public decimal CosteUnitario { get; private set; }

    public decimal Coste { get; private set; }

    internal void Valorar(Guid? tarifaId, decimal costeUnitario)
    {
        TarifaId = tarifaId;
        CosteUnitario = costeUnitario;
        Coste = Redondeo.Dos(Horas * costeUnitario);
    }
}

public sealed class MaterialParte : EntidadBase<Guid>
{
    private MaterialParte(Guid id)
        : base(id)
    {
        Nombre = null!;
    }

    internal MaterialParte(Guid id, Guid productoId, string nombre, decimal cantidad)
        : base(id)
    {
        ProductoId = productoId;
        Nombre = nombre;
        Cantidad = cantidad;
    }

    public Guid ProductoId { get; private set; }

    public string Nombre { get; private set; }

    public decimal Cantidad { get; private set; }

    public decimal CosteUnitario { get; private set; }

    public decimal Coste { get; private set; }

    internal void Valorar(decimal costeUnitario)
    {
        CosteUnitario = costeUnitario;
        Coste = Redondeo.Dos(Cantidad * costeUnitario);
    }
}

public sealed class SalidaParte : EntidadBase<Guid>
{
    private SalidaParte(Guid id)
        : base(id)
    {
        Nombre = null!;
    }

    internal SalidaParte(Guid id, int numeroLinea, Guid productoId, string nombre, decimal kilos, decimal factor, string? calibre, Guid? categoriaId, Guid? paleId,
        int? cajas = null, Guid? envaseProductoId = null, string? motivoDescalificacion = null)
        : base(id)
    {
        MotivoDescalificacion = string.IsNullOrWhiteSpace(motivoDescalificacion) ? null : motivoDescalificacion.Trim();
        Cajas = cajas;
        EnvaseProductoId = envaseProductoId;
        NumeroLinea = numeroLinea;
        ProductoId = productoId;
        Nombre = nombre;
        Kilos = kilos;
        Factor = factor;
        Calibre = string.IsNullOrWhiteSpace(calibre) ? null : calibre.Trim();
        CategoriaId = categoriaId;
        PaleId = paleId;
    }

    public int NumeroLinea { get; private set; }

    public Guid ProductoId { get; private set; }

    public string Nombre { get; private set; }

    public decimal Kilos { get; private set; }

    /// <summary>Peso relativo en el reparto por factor (el destrío suele ser 0).</summary>
    public decimal Factor { get; private set; }

    public string? Calibre { get; private set; }

    public Guid? CategoriaId { get; private set; }

    /// <summary>Palé en el que se deja directamente el producto obtenido.</summary>
    public Guid? PaleId { get; private set; }

    /// <summary>Motivo para confeccionar un artículo convencional con fruta ecológica (descalificación explícita).</summary>
    public string? MotivoDescalificacion { get; private set; }

    public Guid? PartidaId { get; private set; }

    public decimal Coste { get; private set; }

    /// <summary>Cajas confeccionadas (para el tiempo teórico).</summary>
    public int? Cajas { get; private set; }

    /// <summary>Envase de la confección (el rendimiento es por producto y envase).</summary>
    public Guid? EnvaseProductoId { get; private set; }

    /// <summary>Segundos teóricos de confección (cajas × 3600 / rendimiento), si se repartió por tiempo.</summary>
    public decimal? SegundosTeoricos { get; private set; }

    /// <summary>Parte del coste que es mano de obra de confección (por tiempo teórico).</summary>
    public decimal CosteConfeccion { get; private set; }

    public decimal CosteKg => Kilos == 0m ? 0m : decimal.Round(Coste / Kilos, 6);

    internal void Valorar(decimal coste, decimal costeConfeccion = 0m, decimal? segundos = null)
    {
        Coste = coste;
        CosteConfeccion = costeConfeccion;
        SegundosTeoricos = segundos;
    }

    internal void AsignarPartida(Guid partidaId) => PartidaId = partidaId;
}

/// <summary>Genealogía: la partida <see cref="DestinoId"/> se confeccionó (en parte) con la <see cref="OrigenId"/>.</summary>
public sealed class Genealogia : RaizAgregadoEmpresa<Guid>
{
    private Genealogia(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private Genealogia(Guid id, Guid empresaId, Guid parteId, Guid origenId, Guid destinoId, decimal kilosOrigen)
        : base(id, empresaId)
    {
        ParteId = parteId;
        OrigenId = origenId;
        DestinoId = destinoId;
        KilosOrigen = kilosOrigen;
    }

    public Guid ParteId { get; private set; }

    public Guid OrigenId { get; private set; }

    public Guid DestinoId { get; private set; }

    /// <summary>
    /// Kilos de la partida de origen que fueron a la de destino. Lo consumido de cada origen se reparte entre las
    /// salidas en proporción a sus kilos, así que las aristas de un origen suman exactamente lo que se consumió de él.
    /// </summary>
    public decimal KilosOrigen { get; private set; }

    public static Genealogia Crear(Guid empresaId, Guid parteId, Guid origenId, Guid destinoId, decimal kilosOrigen) =>
        new(Guid.NewGuid(), empresaId, parteId, origenId, destinoId, kilosOrigen);

    /// <summary>
    /// Reparte lo consumido de cada partida de origen entre las salidas, en proporción a los kilos de cada salida
    /// (a gramos; la última salida se lleva el redondeo). Cada origen suma lo que se consumió de él y cada salida
    /// recibe de todos los orígenes: la merma queda repartida igual.
    /// </summary>
    public static IReadOnlyList<(Guid Origen, Guid Destino, decimal Kilos)> Repartir(IReadOnlyList<(Guid Origen, decimal Kilos)> consumidas,
        IReadOnlyList<(Guid Destino, decimal Kilos)> salidas)
    {
        ArgumentNullException.ThrowIfNull(consumidas);
        ArgumentNullException.ThrowIfNull(salidas);
        var total = salidas.Sum(s => s.Kilos);
        var aristas = new List<(Guid, Guid, decimal)>();
        if (total <= 0m)
        {
            return aristas;
        }

        foreach (var (origen, kilos) in consumidas)
        {
            var resto = kilos;
            for (var i = 0; i < salidas.Count; i++)
            {
                var parte = i == salidas.Count - 1 ? resto : Math.Round(kilos * salidas[i].Kilos / total, 3, MidpointRounding.AwayFromZero);
                resto -= parte;
                if (parte > 0m)
                {
                    aristas.Add((origen, salidas[i].Destino, parte));
                }
            }
        }

        return aristas;
    }
}
