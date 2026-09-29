using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Agro.Dominio;

/// <summary>
/// Plantilla de control de calidad en la recepción: qué defectos se buscan en la muestra (podrido, golpe, calibre fuera,
/// materia extraña…), cuáles descuentan peso al agricultor y a partir de qué porcentaje. Puede ser de un producto, de
/// una familia o general.
/// </summary>
public sealed class PlantillaCalidad : RaizAgregadoEmpresa<Guid>
{
    private readonly List<DefectoCalidad> _defectos = [];

    private PlantillaCalidad(Guid id)
        : base(id, Guid.Empty)
    {
        Codigo = null!;
        Nombre = null!;
    }

    private PlantillaCalidad(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
        Codigo = null!;
        Nombre = null!;
    }

    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    /// <summary>Solo para este producto; sin producto ni familia, para todos.</summary>
    public Guid? ProductoId { get; private set; }

    public Guid? FamiliaId { get; private set; }

    public bool Activa { get; private set; } = true;

    public IReadOnlyList<DefectoCalidad> Defectos => _defectos;

    public static Resultado<PlantillaCalidad> Crear(Guid empresaId, string? codigo, string? nombre, Guid? productoId, Guid? familiaId,
        IReadOnlyList<(string Nombre, bool DescuentaPeso, decimal? ToleranciaPct, decimal? MaximoPct)> defectos)
    {
        var p = new PlantillaCalidad(Guid.NewGuid(), empresaId);
        var r = p.Cambiar(codigo, nombre, productoId, familiaId, defectos, enUso: false);
        return r.EsFallo ? Resultado.Fallo<PlantillaCalidad>(r.Error) : Resultado.Ok(p);
    }

    /// <summary>
    /// Cambia la plantilla. Si ya se usó en muestreos, los defectos existentes se conservan (por su identificador) y solo
    /// se pueden añadir nuevos o cambiar sus límites: un muestreo hecho no pierde sus defectos.
    /// </summary>
    public Resultado Cambiar(string? codigo, string? nombre, Guid? productoId, Guid? familiaId,
        IReadOnlyList<(string Nombre, bool DescuentaPeso, decimal? ToleranciaPct, decimal? MaximoPct)> defectos, bool enUso)
    {
        ArgumentNullException.ThrowIfNull(defectos);
        if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(nombre))
        {
            return Resultado.Fallo(Error.Validacion("calidad.plantilla", "La plantilla necesita código y nombre."));
        }

        if (productoId is not null && familiaId is not null)
        {
            return Resultado.Fallo(Error.Validacion("calidad.ambito", "La plantilla es de un producto, de una familia o general, no de ambos."));
        }

        if (defectos.Count == 0 || defectos.Any(d => string.IsNullOrWhiteSpace(d.Nombre) || d.ToleranciaPct is < 0m or > 100m || d.MaximoPct is < 0m or > 100m
                || (d.ToleranciaPct is { } t && d.MaximoPct is { } m && m < t))
            || defectos.Select(d => d.Nombre.Trim().ToUpperInvariant()).Distinct().Count() != defectos.Count)
        {
            return Resultado.Fallo(Error.Validacion("calidad.defectos",
                "Indica los defectos, cada uno una vez, con tolerancia y máximo entre 0 y 100 % (el máximo no por debajo de la tolerancia)."));
        }

        if (enUso && _defectos.Any(d => defectos.All(n => !string.Equals(n.Nombre.Trim(), d.Nombre, StringComparison.OrdinalIgnoreCase))))
        {
            return Resultado.Fallo(Error.Conflicto("calidad.defecto_en_uso", "La plantilla ya se usó: no se quitan defectos (desactívala y crea otra)."));
        }

        Codigo = codigo.Trim().ToUpperInvariant();
        Nombre = nombre.Trim();
        ProductoId = productoId;
        FamiliaId = familiaId;
        var orden = 0;
        var nuevos = new List<DefectoCalidad>();
        foreach (var d in defectos)
        {
            orden++;
            var existente = _defectos.FirstOrDefault(x => string.Equals(x.Nombre, d.Nombre.Trim(), StringComparison.OrdinalIgnoreCase));
            var defecto = existente ?? new DefectoCalidad(Guid.NewGuid(), d.Nombre.Trim());
            defecto.Fijar(orden, d.DescuentaPeso, d.ToleranciaPct, d.MaximoPct);
            nuevos.Add(defecto);
        }

        _defectos.Clear();
        _defectos.AddRange(nuevos);
        return Resultado.Ok();
    }

    public void Activar(bool activa) => Activa = activa;

    /// <summary>¿Aplica a un producto de esa familia? (la de producto manda sobre la de familia, y esta sobre la general).</summary>
    public int Prioridad(Guid productoId, Guid? familiaId) =>
        ProductoId == productoId ? 3 : ProductoId is null && FamiliaId is not null && FamiliaId == familiaId ? 2 : ProductoId is null && FamiliaId is null ? 1 : 0;
}

/// <summary>Un defecto de la plantilla, con si descuenta peso y sus límites (en % del peso de la muestra).</summary>
public sealed class DefectoCalidad : EntidadBase<Guid>
{
    private DefectoCalidad(Guid id)
        : base(id)
    {
        Nombre = null!;
    }

    internal DefectoCalidad(Guid id, string nombre)
        : base(id)
    {
        Nombre = nombre.Length > 80 ? nombre[..80] : nombre;
    }

    public string Nombre { get; private set; }

    public int Orden { get; private set; }

    /// <summary>Descuenta su porcentaje del peso que se liquida al agricultor (el podrido, la tierra…).</summary>
    public bool DescuentaPeso { get; private set; }

    /// <summary>Por encima de este %, aviso.</summary>
    public decimal? ToleranciaPct { get; private set; }

    /// <summary>Por encima de este %, la partida no es aceptable tal cual (se rechaza, se reclasifica o se descalifica).</summary>
    public decimal? MaximoPct { get; private set; }

    internal void Fijar(int orden, bool descuenta, decimal? tolerancia, decimal? maximo)
    {
        Orden = orden;
        DescuentaPeso = descuenta;
        ToleranciaPct = tolerancia;
        MaximoPct = maximo;
    }
}

/// <summary>
/// Muestreo de calidad de una línea de recepción: el peso de la muestra y los kilos de cada defecto encontrados. El
/// <b>definitivo</b> fija el descuento de peso (la suma de los defectos que descuentan) y es el que se aplica a la
/// liquidación; los provisionales solo informan. Un definitivo no se cambia: se anula con motivo (si la entrega no está
/// liquidada) y se hace otro.
/// </summary>
public sealed class MuestreoCalidad : RaizAgregadoEmpresa<Guid>
{
    private readonly List<ResultadoMuestreo> _resultados = [];

    private MuestreoCalidad(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private MuestreoCalidad(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
    }

    public Guid RecepcionId { get; private set; }

    public Guid LineaRecepcionId { get; private set; }

    public Guid PlantillaId { get; private set; }

    public DateOnly Fecha { get; private set; }

    public decimal PesoMuestraKg { get; private set; }

    public bool Definitivo { get; private set; }

    /// <summary>% del peso que se descuenta al liquidar (solo en el definitivo).</summary>
    public decimal DescuentoPct { get; private set; }

    public string? Observaciones { get; private set; }

    public Guid? UsuarioId { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public bool Anulado { get; private set; }

    public string? MotivoAnulacion { get; private set; }

    public IReadOnlyList<ResultadoMuestreo> Resultados => _resultados;

    public static Resultado<MuestreoCalidad> Crear(Guid empresaId, Guid recepcionId, Guid lineaId, PlantillaCalidad plantilla, DateOnly fecha, decimal pesoMuestra,
        IReadOnlyList<(Guid DefectoId, decimal Kilos)> resultados, bool definitivo, string? observaciones, Guid? usuarioId, DateTimeOffset ahora)
    {
        ArgumentNullException.ThrowIfNull(plantilla);
        ArgumentNullException.ThrowIfNull(resultados);
        if (pesoMuestra <= 0m || decimal.Round(pesoMuestra, 3) != pesoMuestra)
        {
            return Resultado.Fallo<MuestreoCalidad>(Error.Validacion("muestreo.peso", "El peso de la muestra es positivo (hasta 3 decimales)."));
        }

        if (resultados.Any(r => r.Kilos < 0m || decimal.Round(r.Kilos, 3) != r.Kilos || plantilla.Defectos.All(d => d.Id != r.DefectoId))
            || resultados.GroupBy(r => r.DefectoId).Any(g => g.Count() > 1))
        {
            return Resultado.Fallo<MuestreoCalidad>(Error.Validacion("muestreo.resultados", "Cada defecto de la plantilla una vez, con sus kilos (no negativos)."));
        }

        if (resultados.Sum(r => r.Kilos) > pesoMuestra)
        {
            return Resultado.Fallo<MuestreoCalidad>(Error.Validacion("muestreo.supera_muestra", "Los defectos pesan más que la muestra."));
        }

        var m = new MuestreoCalidad(Guid.NewGuid(), empresaId)
        {
            RecepcionId = recepcionId, LineaRecepcionId = lineaId, PlantillaId = plantilla.Id, Fecha = fecha, PesoMuestraKg = pesoMuestra, Definitivo = definitivo,
            Observaciones = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim().Length > 300 ? observaciones.Trim()[..300] : observaciones.Trim(),
            UsuarioId = usuarioId, CreadoEn = ahora,
        };
        foreach (var (defectoId, kilos) in resultados.Where(r => r.Kilos > 0m))
        {
            var d = plantilla.Defectos.Single(x => x.Id == defectoId);
            m._resultados.Add(new ResultadoMuestreo(Guid.NewGuid(), d.Id, d.Nombre, kilos, Math.Round(kilos * 100m / pesoMuestra, 2, MidpointRounding.AwayFromZero), d.DescuentaPeso));
        }

        m.DescuentoPct = Math.Min(100m, m._resultados.Where(r => r.DescuentaPeso).Sum(r => r.Porcentaje));
        return Resultado.Ok(m);
    }

    public Resultado Anular(string? motivo)
    {
        if (Anulado)
        {
            return Resultado.Fallo(Error.Conflicto("muestreo.anulado", "El muestreo ya está anulado."));
        }

        if (string.IsNullOrWhiteSpace(motivo))
        {
            return Resultado.Fallo(Error.Validacion("muestreo.motivo", "Indica el motivo de la anulación."));
        }

        Anulado = true;
        MotivoAnulacion = motivo.Trim().Length > 300 ? motivo.Trim()[..300] : motivo.Trim();
        return Resultado.Ok();
    }

    /// <summary>Kilos que se liquidan con el descuento de este muestreo sobre el neto real.</summary>
    public decimal Aplicar(decimal neto) => Math.Round(neto * (100m - DescuentoPct) / 100m, 3, MidpointRounding.AwayFromZero);

    /// <summary>Defectos por encima de su tolerancia o de su máximo.</summary>
    public IReadOnlyList<(ResultadoMuestreo Resultado, bool SuperaMaximo)> Avisos(PlantillaCalidad plantilla)
    {
        ArgumentNullException.ThrowIfNull(plantilla);
        return _resultados.Select(r => (r, plantilla.Defectos.FirstOrDefault(d => d.Id == r.DefectoId)))
            .Where(x => x.Item2 is not null && ((x.Item2.ToleranciaPct is { } t && x.r.Porcentaje > t) || (x.Item2.MaximoPct is { } mx && x.r.Porcentaje > mx)))
            .Select(x => (x.r, x.Item2!.MaximoPct is { } mx && x.r.Porcentaje > mx)).ToList();
    }
}

/// <summary>Kilos de un defecto en la muestra y su % (copiados del defecto al hacer el muestreo).</summary>
public sealed class ResultadoMuestreo : EntidadBase<Guid>
{
    private ResultadoMuestreo(Guid id)
        : base(id)
    {
        Defecto = null!;
    }

    internal ResultadoMuestreo(Guid id, Guid defectoId, string defecto, decimal kilos, decimal porcentaje, bool descuentaPeso)
        : base(id)
    {
        DefectoId = defectoId;
        Defecto = defecto;
        Kilos = kilos;
        Porcentaje = porcentaje;
        DescuentaPeso = descuentaPeso;
    }

    public Guid DefectoId { get; private set; }

    public string Defecto { get; private set; }

    public decimal Kilos { get; private set; }

    public decimal Porcentaje { get; private set; }

    public bool DescuentaPeso { get; private set; }
}
