using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Agro.Dominio;

/// <summary>Estado de una recepción de fruta.</summary>
public enum EstadoRecepcion
{
    /// <summary>En curso: se añaden líneas y pesadas. No tiene número ni mueve existencias.</summary>
    Borrador = 1,

    /// <summary>Confirmada: numerada sin huecos, con una partida por línea y el movimiento de envases.</summary>
    Confirmada = 2,

    /// <summary>Anulada: sus partidas se dan de baja (solo si no se han usado ni liquidado).</summary>
    Anulada = 3,
}

/// <summary>
/// Entrada de fruta de un agricultor en el almacén: el albarán de entrada. Cada línea es un producto de
/// una parcela, con sus pesadas en báscula (bruto − tara = neto) y sus envases. Al confirmarla, cada
/// línea crea una <see cref="Partida"/> con los kilos netos, que es lo que se clasifica, se confecciona
/// y se liquida al agricultor.
/// </summary>
public sealed class Recepcion : RaizAgregadoEmpresa<Guid>
{
    public const string Serie = "REC";
    public const int LongitudTexto = 200;

    private readonly List<LineaRecepcion> _lineas = [];
    private readonly List<Pesada> _pesadas = [];
    private readonly List<EnvasePesada> _envasesPesadas = [];
    private readonly List<PaleEntrada> _palesEntrada = [];

    private Recepcion(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private Recepcion(Guid id, Guid empresaId, Guid agricultorId, Guid campanaId, DateOnly fecha, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        AgricultorId = agricultorId;
        CampanaId = campanaId;
        Fecha = fecha;
        Ejercicio = fecha.Year;
        Estado = EstadoRecepcion.Borrador;
        CreadoEn = ahora;
    }

    public int Ejercicio { get; private set; }

    /// <summary>Número correlativo sin huecos de la serie y ejercicio; se asigna al confirmar.</summary>
    public int? Numero { get; private set; }

    public string? NumeroCompleto => Numero is { } n ? $"{Serie}-{Ejercicio}-{n:D6}" : null;

    public DateOnly Fecha { get; private set; }

    public Guid AgricultorId { get; private set; }

    public Guid CampanaId { get; private set; }

    public string? Matricula { get; private set; }

    public string? Conductor { get; private set; }

    public string? Observaciones { get; private set; }

    public EstadoRecepcion Estado { get; private set; }

    public string? MotivoAnulacion { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset? ConfirmadaEn { get; private set; }

    public DateTimeOffset? AnuladaEn { get; private set; }

    public IReadOnlyList<LineaRecepcion> Lineas => _lineas;

    public IReadOnlyList<Pesada> Pesadas => _pesadas;

    /// <summary>Envases contados en cada pesada, por tipo, con la tara unitaria y la versión de tara aplicada.</summary>
    public IReadOnlyList<EnvasePesada> EnvasesPesadas => _envasesPesadas;

    /// <summary>Palés (o palots) que llegan, contados uno a uno: cada uno será un palé con su SSCC y su serie de origen.</summary>
    public IReadOnlyList<PaleEntrada> PalesEntrada => _palesEntrada;

    public decimal NetoKg => _lineas.Sum(l => NetoDe(l.Id));

    public decimal NetoDe(Guid lineaId) => _pesadas.Where(p => p.LineaId == lineaId).Sum(p => p.NetoKg);

    public int EnvasesDe(Guid lineaId) => _pesadas.Where(p => p.LineaId == lineaId).Sum(p => p.Envases);

    public static Resultado<Recepcion> Crear(Guid empresaId, Guid agricultorId, Guid campanaId, DateOnly fecha, string? matricula, string? conductor, string? observaciones, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        var r = new Recepcion(Guid.NewGuid(), empresaId, agricultorId, campanaId, fecha, reloj.AhoraUtc);
        var datos = r.ActualizarCabecera(fecha, matricula, conductor, observaciones);
        return datos.EsFallo ? Resultado.Fallo<Recepcion>(datos.Error) : Resultado.Ok(r);
    }

    public Resultado ActualizarCabecera(DateOnly fecha, string? matricula, string? conductor, string? observaciones)
    {
        var borrador = SoloBorrador();
        if (borrador.EsFallo)
        {
            return borrador;
        }

        if (new[] { matricula, conductor, observaciones }.Any(t => t?.Trim().Length > LongitudTexto))
        {
            return Resultado.Fallo(Error.Validacion("recepcion.texto_largo", $"Los textos admiten hasta {LongitudTexto} caracteres."));
        }

        Fecha = fecha;
        Ejercicio = fecha.Year;
        Matricula = Limpio(matricula)?.ToUpperInvariant();
        Conductor = Limpio(conductor);
        Observaciones = Limpio(observaciones);
        return Resultado.Ok();
    }

    public Resultado<LineaRecepcion> AgregarLinea(DatosLineaRecepcion datos)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var borrador = SoloBorrador();
        if (borrador.EsFallo)
        {
            return Resultado.Fallo<LineaRecepcion>(borrador.Error);
        }

        if (datos.PrecioEstimadoKg is < 0m)
        {
            return Resultado.Fallo<LineaRecepcion>(Error.Validacion("recepcion.precio", "El precio estimado no puede ser negativo."));
        }

        if (datos.FechaRecoleccion is { } fr && fr > Fecha)
        {
            return Resultado.Fallo<LineaRecepcion>(Error.Validacion("recepcion.fecha_recoleccion", "La fruta no puede recolectarse después de recibirla."));
        }

        if (datos.KilosLiquidacion is { } kl && (kl <= 0m || decimal.Round(kl, 3) != kl || string.IsNullOrWhiteSpace(datos.MotivoKilosLiquidacion)))
        {
            return Resultado.Fallo<LineaRecepcion>(Error.Validacion("recepcion.kilos_liquidacion",
                "Los kilos de liquidación son positivos y llevan el motivo (contrato, destrío…): nunca sustituyen al neto pesado."));
        }

        var linea = new LineaRecepcion(Guid.NewGuid(), _lineas.Count == 0 ? 1 : _lineas.Max(l => l.NumeroLinea) + 1, datos);
        _lineas.Add(linea);
        return Resultado.Ok(linea);
    }

    /// <summary>Fija (o quita, con null) los kilos de liquidación de una línea, con su motivo. Solo en borrador.</summary>
    public Resultado FijarKilosLiquidacion(Guid lineaId, decimal? kilos, string? motivo)
    {
        var borrador = SoloBorrador();
        if (borrador.EsFallo)
        {
            return borrador;
        }

        var linea = _lineas.FirstOrDefault(l => l.Id == lineaId);
        if (linea is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("recepcion.linea_no_encontrada", "La línea no existe."));
        }

        if (kilos is { } k && (k <= 0m || decimal.Round(k, 3) != k))
        {
            return Resultado.Fallo(Error.Validacion("recepcion.kilos_liquidacion", "Los kilos de liquidación son positivos (hasta 3 decimales)."));
        }

        if (kilos is not null && string.IsNullOrWhiteSpace(motivo))
        {
            return Resultado.Fallo(Error.Validacion("recepcion.kilos_liquidacion_motivo", "Indica por qué se liquidan kilos distintos del neto (contrato, destrío…)."));
        }

        linea.FijarKilosLiquidacion(kilos, motivo);
        return Resultado.Ok();
    }

    public Resultado QuitarLinea(Guid lineaId)
    {
        var borrador = SoloBorrador();
        if (borrador.EsFallo)
        {
            return borrador;
        }

        var linea = _lineas.FirstOrDefault(l => l.Id == lineaId);
        if (linea is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("recepcion.linea_no_encontrada", "La línea no existe."));
        }

        var pesadas = _pesadas.Where(p => p.LineaId == lineaId).Select(p => p.Id).ToHashSet();
        _envasesPesadas.RemoveAll(e => pesadas.Contains(e.PesadaId));
        _pesadas.RemoveAll(p => p.LineaId == lineaId);
        _palesEntrada.RemoveAll(p => p.LineaId == lineaId);
        _lineas.Remove(linea);
        return Resultado.Ok();
    }

    /// <summary>Añade una pesada de báscula a una línea. El neto es bruto − tara (camión, palots…).</summary>
    public Resultado<Pesada> AgregarPesada(Guid lineaId, decimal brutoKg, decimal taraKg, int envases, string? bascula)
    {
        var borrador = SoloBorrador();
        if (borrador.EsFallo)
        {
            return Resultado.Fallo<Pesada>(borrador.Error);
        }

        if (_lineas.All(l => l.Id != lineaId))
        {
            return Resultado.Fallo<Pesada>(Error.NoEncontrado("recepcion.linea_no_encontrada", "La línea no existe."));
        }

        if (taraKg < 0m || brutoKg <= taraKg)
        {
            return Resultado.Fallo<Pesada>(Error.Validacion("pesada.kilos", "El bruto debe ser mayor que la tara, y la tara no puede ser negativa."));
        }

        if (decimal.Round(brutoKg, 3) != brutoKg || decimal.Round(taraKg, 3) != taraKg)
        {
            return Resultado.Fallo<Pesada>(Error.Validacion("pesada.decimales", "Los kilos admiten hasta 3 decimales."));
        }

        if (envases < 0)
        {
            return Resultado.Fallo<Pesada>(Error.Validacion("pesada.envases", "Los envases no pueden ser negativos."));
        }

        var secuencia = _pesadas.Where(p => p.LineaId == lineaId).Select(p => p.Secuencia).DefaultIfEmpty(0).Max() + 1;
        var pesada = new Pesada(Guid.NewGuid(), lineaId, secuencia, brutoKg, taraKg, envases, Limpio(bascula));
        _pesadas.Add(pesada);
        return Resultado.Ok(pesada);
    }

    public Resultado QuitarPesada(Guid pesadaId)
    {
        var borrador = SoloBorrador();
        if (borrador.EsFallo)
        {
            return borrador;
        }

        // La pesada de un camión repartida entre líneas se quita entera: por separado ya no sumaría el bruto de la báscula.
        var grupo = _pesadas.FirstOrDefault(p => p.Id == pesadaId)?.GrupoCamion;
        var quitar = _pesadas.Where(p => p.Id == pesadaId || (grupo is not null && p.GrupoCamion == grupo)).Select(p => p.Id).ToHashSet();
        _envasesPesadas.RemoveAll(e => quitar.Contains(e.PesadaId));
        return _pesadas.RemoveAll(p => quitar.Contains(p.Id)) == 0
            ? Resultado.Fallo(Error.NoEncontrado("pesada.no_encontrada", "La pesada no existe."))
            : Resultado.Ok();
    }

    /// <summary>
    /// Pesada completa: el bruto de báscula, la tara del camión (pesado vacío) y los envases contados por tipo, con la
    /// tara de cada uno. La tara total es la del camión más la de los envases, y el neto, bruto − tara: nada se teclea a
    /// ojo. Los envases de la pesada (para la cuenta de envases) son los del envase de la línea.
    /// </summary>
    public Resultado<Pesada> AgregarPesadaCompleta(Guid lineaId, decimal brutoKg, decimal taraCamionKg, IReadOnlyList<EnvaseContado> envases, string? bascula)
    {
        ArgumentNullException.ThrowIfNull(envases);
        var linea = _lineas.FirstOrDefault(l => l.Id == lineaId);
        if (linea is null)
        {
            return Resultado.Fallo<Pesada>(Error.NoEncontrado("recepcion.linea_no_encontrada", "La línea no existe."));
        }

        if (taraCamionKg < 0m || envases.Any(e => e.Cantidad <= 0 || e.TaraUnitariaKg < 0m) || envases.GroupBy(e => e.EnvaseProductoId).Any(g => g.Count() > 1))
        {
            return Resultado.Fallo<Pesada>(Error.Validacion("pesada.envases", "Cada tipo de envase una vez, con cantidad positiva; la tara del camión no puede ser negativa."));
        }

        var taraEnvases = envases.Sum(e => e.Cantidad * e.TaraUnitariaKg);
        var cuentan = linea.EnvaseProductoId is { } envaseLinea ? envases.Where(e => e.EnvaseProductoId == envaseLinea).Sum(e => e.Cantidad) : 0;
        var pesada = AgregarPesada(lineaId, brutoKg, taraCamionKg + taraEnvases, cuentan, bascula);
        if (pesada.EsFallo)
        {
            return pesada;
        }

        pesada.Valor.Desglosar(taraCamionKg, taraEnvases);
        _envasesPesadas.AddRange(envases.Select(e => new EnvasePesada(Guid.NewGuid(), pesada.Valor.Id, e.EnvaseProductoId, e.Cantidad, e.TaraUnitariaKg, e.TaraEnvaseId)));
        return pesada;
    }

    /// <summary>
    /// Pesada del camión entero con varias líneas (productos o parcelas): un solo bruto y una tara de camión, y los envases
    /// contados de cada línea. El neto total (bruto − camión − envases) se reparte entre las líneas en proporción a sus
    /// envases de fruta (los de su envase; si la línea no tiene envase, todos los suyos), y la tara del camión igual. Cada
    /// línea queda con su pesada, todas con el mismo grupo, y sus brutos suman el bruto de la báscula.
    /// </summary>
    public Resultado<IReadOnlyList<Pesada>> AgregarPesadaCamion(decimal brutoKg, decimal taraCamionKg, IReadOnlyList<(Guid LineaId, IReadOnlyList<EnvaseContado> Envases)> lineas,
        string? bascula)
    {
        ArgumentNullException.ThrowIfNull(lineas);
        var borrador = SoloBorrador();
        if (borrador.EsFallo)
        {
            return Resultado.Fallo<IReadOnlyList<Pesada>>(borrador.Error);
        }

        if (lineas.Count < 2 || lineas.Select(l => l.LineaId).Distinct().Count() != lineas.Count || lineas.Any(l => _lineas.All(x => x.Id != l.LineaId)))
        {
            return Resultado.Fallo<IReadOnlyList<Pesada>>(Error.Validacion("pesada_camion.lineas", "La pesada del camión reparte entre dos o más líneas distintas de la recepción."));
        }

        if (taraCamionKg < 0m || decimal.Round(brutoKg, 3) != brutoKg || decimal.Round(taraCamionKg, 3) != taraCamionKg)
        {
            return Resultado.Fallo<IReadOnlyList<Pesada>>(Error.Validacion("pesada.kilos", "Los kilos admiten hasta 3 decimales y la tara del camión no puede ser negativa."));
        }

        var fruta = lineas.Select(l =>
        {
            var envaseLinea = _lineas.First(x => x.Id == l.LineaId).EnvaseProductoId;
            return envaseLinea is { } e ? l.Envases.Where(x => x.EnvaseProductoId == e).Sum(x => x.Cantidad) : l.Envases.Sum(x => x.Cantidad);
        }).ToList();
        if (fruta.Any(f => f <= 0))
        {
            return Resultado.Fallo<IReadOnlyList<Pesada>>(Error.Validacion("pesada_camion.envases", "Cuenta los envases de fruta de cada línea: el neto se reparte en proporción a ellos."));
        }

        var taraEnvases = lineas.Sum(l => l.Envases.Sum(e => e.Cantidad * e.TaraUnitariaKg));
        var neto = brutoKg - taraCamionKg - taraEnvases;
        if (neto <= 0m)
        {
            return Resultado.Fallo<IReadOnlyList<Pesada>>(Error.Validacion("pesada.kilos", "El bruto no llega a la tara del camión más la de los envases."));
        }

        var total = fruta.Sum();
        var grupo = Guid.NewGuid();
        var pesadas = new List<Pesada>();
        decimal restoNeto = neto, restoCamion = taraCamionKg;
        for (var i = 0; i < lineas.Count; i++)
        {
            var ultima = i == lineas.Count - 1;
            var netoLinea = ultima ? restoNeto : Math.Round(neto * fruta[i] / total, 3, MidpointRounding.AwayFromZero);
            var camionLinea = ultima ? restoCamion : Math.Round(taraCamionKg * fruta[i] / total, 3, MidpointRounding.AwayFromZero);
            restoNeto -= netoLinea;
            restoCamion -= camionLinea;
            var taraLinea = lineas[i].Envases.Sum(e => e.Cantidad * e.TaraUnitariaKg);
            var p = AgregarPesadaCompleta(lineas[i].LineaId, netoLinea + camionLinea + taraLinea, camionLinea, lineas[i].Envases, bascula);
            if (p.EsFallo)
            {
                _pesadas.RemoveAll(x => pesadas.Contains(x));
                _envasesPesadas.RemoveAll(x => pesadas.Any(y => y.Id == x.PesadaId));
                return Resultado.Fallo<IReadOnlyList<Pesada>>(p.Error);
            }

            p.Valor.AgruparEn(grupo, brutoKg);
            pesadas.Add(p.Valor);
        }

        return Resultado.Ok<IReadOnlyList<Pesada>>(pesadas);
    }

    /// <summary>
    /// Vuelve a aplicar las taras de los envases con las vigentes en la fecha de la recepción (si la fecha cambió en el
    /// borrador). <paramref name="taraDe"/> da la tara vigente de un envase, o null si no hay.
    /// </summary>
    public Resultado AplicarTaras(Func<Guid, (decimal TaraKg, Guid TaraId)?> taraDe)
    {
        ArgumentNullException.ThrowIfNull(taraDe);
        var borrador = SoloBorrador();
        if (borrador.EsFallo)
        {
            return borrador;
        }

        foreach (var e in _envasesPesadas.Where(e => e.TaraEnvaseId is not null))
        {
            if (taraDe(e.EnvaseProductoId) is not { } t)
            {
                return Resultado.Fallo(Error.Validacion("tara.falta", $"No hay tara vigente el {Fecha:dd/MM/yyyy} para un envase de la recepción."));
            }

            e.Aplicar(t.TaraKg, t.TaraId);
        }

        foreach (var p in _pesadas.Where(p => p.TaraCamionKg is not null))
        {
            var taraEnvases = _envasesPesadas.Where(e => e.PesadaId == p.Id).Sum(e => e.Cantidad * e.TaraUnitariaKg);
            if (p.BrutoKg <= p.TaraCamionKg!.Value + taraEnvases)
            {
                return Resultado.Fallo(Error.Validacion("pesada.kilos", $"Con las taras del {Fecha:dd/MM/yyyy}, la pesada {p.Secuencia} no tiene neto."));
            }

            p.Desglosar(p.TaraCamionKg!.Value, taraEnvases);
        }

        return Resultado.Ok();
    }

    /// <summary>
    /// Registra un palé (o palot) que llega en la línea: su número de serie de origen (la etiqueta del proveedor o la
    /// finca), el envase y cuántos trae, y sus kilos netos si se pesó solo. Son los palés reales: nunca se deducen de un
    /// factor fijo del artículo.
    /// </summary>
    public Resultado<PaleEntrada> AgregarPaleEntrada(Guid lineaId, string? serieOrigen, Guid? envaseProductoId, int envases, decimal? kilosNetos)
    {
        var borrador = SoloBorrador();
        if (borrador.EsFallo)
        {
            return Resultado.Fallo<PaleEntrada>(borrador.Error);
        }

        if (_lineas.All(l => l.Id != lineaId))
        {
            return Resultado.Fallo<PaleEntrada>(Error.NoEncontrado("recepcion.linea_no_encontrada", "La línea no existe."));
        }

        if (envases < 0 || kilosNetos is <= 0m || (kilosNetos is { } k && decimal.Round(k, 3) != k))
        {
            return Resultado.Fallo<PaleEntrada>(Error.Validacion("pale_entrada.valores", "Los envases no pueden ser negativos y los kilos, si se indican, son positivos (hasta 3 decimales)."));
        }

        var serie = Limpio(serieOrigen);
        if (serie is not null && _palesEntrada.Any(p => string.Equals(p.SerieOrigen, serie, StringComparison.OrdinalIgnoreCase)))
        {
            return Resultado.Fallo<PaleEntrada>(Error.Conflicto("pale_entrada.repetido", $"El palé {serie} ya está en la recepción."));
        }

        var numero = _palesEntrada.Where(p => p.LineaId == lineaId).Select(p => p.Numero).DefaultIfEmpty(0).Max() + 1;
        var pale = new PaleEntrada(Guid.NewGuid(), lineaId, numero, serie is { Length: > 60 } ? serie[..60] : serie, envaseProductoId, envases, kilosNetos);
        _palesEntrada.Add(pale);
        return Resultado.Ok(pale);
    }

    public Resultado QuitarPaleEntrada(Guid paleEntradaId)
    {
        var borrador = SoloBorrador();
        if (borrador.EsFallo)
        {
            return borrador;
        }

        return _palesEntrada.RemoveAll(p => p.Id == paleEntradaId) == 0
            ? Resultado.Fallo(Error.NoEncontrado("pale_entrada.no_encontrado", "El palé no está en la recepción."))
            : Resultado.Ok();
    }

    /// <summary>
    /// Kilos de cada palé de entrada de la línea: los pesados uno a uno si todos se pesaron; si no, el neto de la línea
    /// repartido en proporción a sus envases (a partes iguales si no traen envases contados). Suman exactamente el neto.
    /// </summary>
    public IReadOnlyList<(PaleEntrada Pale, decimal Kilos)> KilosPalesEntrada(Guid lineaId)
    {
        var pales = _palesEntrada.Where(p => p.LineaId == lineaId).OrderBy(p => p.Numero).ToList();
        if (pales.Count == 0)
        {
            return [];
        }

        if (pales.All(p => p.KilosNetos is not null))
        {
            return pales.Select(p => (p, p.KilosNetos!.Value)).ToList();
        }

        var neto = NetoDe(lineaId);
        var envases = pales.Sum(p => p.Envases);
        var resto = neto;
        var resultado = new List<(PaleEntrada, decimal)>();
        for (var i = 0; i < pales.Count; i++)
        {
            var kilos = i == pales.Count - 1
                ? resto
                : Math.Round(envases > 0 ? neto * pales[i].Envases / envases : neto / pales.Count, 3, MidpointRounding.AwayFromZero);
            resto -= kilos;
            resultado.Add((pales[i], kilos));
        }

        return resultado;
    }

    /// <summary>
    /// Errores que impiden confirmar, todos a la vez (para corregirlos de una pasada): sin líneas, líneas
    /// sin pesadas, envases sin artículo de envase, recolección posterior a la entrega.
    /// </summary>
    public IReadOnlyList<Error> ErroresConfirmacion()
    {
        var errores = new List<Error>();
        if (Estado != EstadoRecepcion.Borrador)
        {
            errores.Add(Error.Conflicto("recepcion.no_borrador", "La recepción ya está confirmada o anulada."));
            return errores;
        }

        if (_lineas.Count == 0)
        {
            errores.Add(Error.Validacion("recepcion.sin_lineas", "La recepción no tiene líneas."));
        }

        foreach (var l in _lineas.OrderBy(l => l.NumeroLinea))
        {
            if (_pesadas.All(p => p.LineaId != l.Id))
            {
                errores.Add(Error.Validacion("recepcion.sin_pesadas", $"La línea {l.NumeroLinea} no tiene pesadas."));
            }

            if (EnvasesDe(l.Id) > 0 && l.EnvaseProductoId is null)
            {
                errores.Add(Error.Validacion("recepcion.envase", $"La línea {l.NumeroLinea} trae envases pero no indica cuál es el envase."));
            }

            if (l.FechaRecoleccion is { } fr && fr > Fecha)
            {
                errores.Add(Error.Validacion("recepcion.fecha_recoleccion", $"La línea {l.NumeroLinea} se recolectó después de la fecha de la recepción."));
            }

            // Los palés contados cuadran con lo pesado: los envases de la línea y, si se pesaron uno a uno, los kilos.
            var pales = _palesEntrada.Where(p => p.LineaId == l.Id).ToList();
            if (pales.Count > 0)
            {
                var envasesPales = pales.Where(p => p.EnvaseProductoId is null || p.EnvaseProductoId == l.EnvaseProductoId).Sum(p => p.Envases);
                if (EnvasesDe(l.Id) > 0 && envasesPales != EnvasesDe(l.Id))
                {
                    errores.Add(Error.Validacion("recepcion.pales_envases",
                        $"La línea {l.NumeroLinea}: los palés traen {envasesPales} envases y en las pesadas se contaron {EnvasesDe(l.Id)}."));
                }

                if (pales.Any(p => p.KilosNetos is not null) && (pales.Any(p => p.KilosNetos is null) || pales.Sum(p => p.KilosNetos!.Value) != NetoDe(l.Id)))
                {
                    errores.Add(Error.Validacion("recepcion.pales_kilos",
                        $"La línea {l.NumeroLinea}: si los palés se pesan uno a uno, se pesan todos y suman el neto ({NetoDe(l.Id):0.###} kg)."));
                }
            }

            if (l.KilosLiquidacion is not null && string.IsNullOrWhiteSpace(l.MotivoKilosLiquidacion))
            {
                errores.Add(Error.Validacion("recepcion.kilos_liquidacion_motivo", $"La línea {l.NumeroLinea} liquida kilos distintos del neto: indica el motivo."));
            }
        }

        return errores;
    }

    /// <summary>Confirma con el número reservado y fija en cada línea su partida, sus kilos netos y sus envases.</summary>
    public Resultado Confirmar(int numero, IReadOnlyDictionary<Guid, Guid> partidaPorLinea, IReloj reloj, IReadOnlyDictionary<Guid, Guid>? palePorEntrada = null)
    {
        ArgumentNullException.ThrowIfNull(partidaPorLinea);
        ArgumentNullException.ThrowIfNull(reloj);
        var errores = ErroresConfirmacion();
        if (errores.Count > 0)
        {
            return Resultado.Fallo(errores[0]);
        }

        foreach (var l in _lineas)
        {
            l.Fijar(partidaPorLinea[l.Id], NetoDe(l.Id), EnvasesDe(l.Id));
        }

        foreach (var l in _lineas)
        {
            foreach (var (pale, kilos) in KilosPalesEntrada(l.Id))
            {
                pale.Fijar(palePorEntrada is not null && palePorEntrada.TryGetValue(pale.Id, out var paleId) ? paleId : null, kilos);
            }
        }

        Numero = numero;
        Estado = EstadoRecepcion.Confirmada;
        ConfirmadaEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    public Resultado Anular(string? motivo, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (Estado != EstadoRecepcion.Confirmada)
        {
            return Resultado.Fallo(Error.Conflicto("recepcion.no_confirmada", "Solo se anula una recepción confirmada (un borrador se elimina)."));
        }

        if (string.IsNullOrWhiteSpace(motivo) || motivo.Trim().Length > LongitudTexto)
        {
            return Resultado.Fallo(Error.Validacion("recepcion.motivo", "Indica el motivo de la anulación."));
        }

        Estado = EstadoRecepcion.Anulada;
        MotivoAnulacion = motivo.Trim();
        AnuladaEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    private Resultado SoloBorrador() =>
        Estado == EstadoRecepcion.Borrador
            ? Resultado.Ok()
            : Resultado.Fallo(Error.Conflicto("recepcion.no_borrador", "La recepción ya está confirmada o anulada: no se puede modificar."));

    private static string? Limpio(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
}

public sealed record DatosLineaRecepcion(
    Guid ProductoId, string ProductoNombre, Guid? ParcelaId = null, DateOnly? FechaRecoleccion = null, Guid? EnvaseProductoId = null,
    decimal? PrecioEstimadoKg = null, string? Calibre = null, string? MotivoDescalificacion = null, decimal? KilosLiquidacion = null,
    string? MotivoKilosLiquidacion = null);

/// <summary>Envases de un tipo contados en una pesada, con la tara unitaria (y la versión de tara) que se les aplica.</summary>
public sealed record EnvaseContado(Guid EnvaseProductoId, int Cantidad, decimal TaraUnitariaKg, Guid? TaraEnvaseId);

/// <summary>Línea de una recepción: un producto de una parcela.</summary>
public sealed class LineaRecepcion : EntidadBase<Guid>
{
    private LineaRecepcion(Guid id)
        : base(id)
    {
        ProductoNombre = null!;
    }

    internal LineaRecepcion(Guid id, int numeroLinea, DatosLineaRecepcion d)
        : base(id)
    {
        NumeroLinea = numeroLinea;
        ProductoId = d.ProductoId;
        ProductoNombre = d.ProductoNombre;
        ParcelaId = d.ParcelaId;
        FechaRecoleccion = d.FechaRecoleccion;
        EnvaseProductoId = d.EnvaseProductoId;
        PrecioEstimadoKg = d.PrecioEstimadoKg;
        Calibre = string.IsNullOrWhiteSpace(d.Calibre) ? null : d.Calibre.Trim();
        MotivoDescalificacion = string.IsNullOrWhiteSpace(d.MotivoDescalificacion) ? null : d.MotivoDescalificacion.Trim();
        KilosLiquidacion = d.KilosLiquidacion;
        MotivoKilosLiquidacion = string.IsNullOrWhiteSpace(d.MotivoKilosLiquidacion) ? null : d.MotivoKilosLiquidacion.Trim();
    }

    /// <summary>
    /// Kilos por los que se liquida al agricultor si no son los netos reales (una cantidad teórica pactada, por
    /// ejemplo). Es un dato aparte y con motivo: nunca sustituye al neto, que es lo que entra en la partida.
    /// </summary>
    public decimal? KilosLiquidacion { get; private set; }

    public string? MotivoKilosLiquidacion { get; private set; }

    /// <summary>Lo que se liquida: los kilos de liquidación si se fijaron; si no, el neto real.</summary>
    public decimal? KilosALiquidar => KilosLiquidacion ?? NetoKg;

    internal void FijarKilosLiquidacion(decimal? kilos, string? motivo)
    {
        KilosLiquidacion = kilos;
        MotivoKilosLiquidacion = kilos is null ? null : motivo?.Trim();
    }

    /// <summary>Motivo para recibir fruta ecológica con un artículo convencional (descalificación explícita).</summary>
    public string? MotivoDescalificacion { get; private set; }

    public int NumeroLinea { get; private set; }

    public Guid ProductoId { get; private set; }

    public string ProductoNombre { get; private set; }

    public Guid? ParcelaId { get; private set; }

    public DateOnly? FechaRecoleccion { get; private set; }

    /// <summary>Artículo del envase en que llega la fruta (palot, caja…), para el control de envases del agricultor.</summary>
    public Guid? EnvaseProductoId { get; private set; }

    /// <summary>Precio orientativo (anticipo); el definitivo sale de los precios de liquidación.</summary>
    public decimal? PrecioEstimadoKg { get; private set; }

    public string? Calibre { get; private set; }

    /// <summary>Partida creada al confirmar.</summary>
    public Guid? PartidaId { get; private set; }

    /// <summary>Kilos netos (suma de las pesadas), fijados al confirmar.</summary>
    public decimal? NetoKg { get; private set; }

    public int? Envases { get; private set; }

    internal void Fijar(Guid partidaId, decimal netoKg, int envases)
    {
        PartidaId = partidaId;
        NetoKg = netoKg;
        Envases = envases;
    }
}

/// <summary>Pesada de báscula de una línea.</summary>
public sealed class Pesada : EntidadBase<Guid>
{
    private Pesada(Guid id)
        : base(id)
    {
    }

    internal Pesada(Guid id, Guid lineaId, int secuencia, decimal bruto, decimal tara, int envases, string? bascula)
        : base(id)
    {
        LineaId = lineaId;
        Secuencia = secuencia;
        BrutoKg = bruto;
        TaraKg = tara;
        Envases = envases;
        Bascula = bascula;
    }

    public Guid LineaId { get; private set; }

    public int Secuencia { get; private set; }

    public decimal BrutoKg { get; private set; }

    public decimal TaraKg { get; private set; }

    public decimal NetoKg => BrutoKg - TaraKg;

    public int Envases { get; private set; }

    /// <summary>Identificador de la báscula o del ticket de pesada.</summary>
    public string? Bascula { get; private set; }

    /// <summary>Tara del camión (pesado vacío) en una pesada completa; null si la tara se tecleó entera.</summary>
    public decimal? TaraCamionKg { get; private set; }

    /// <summary>Tara de los envases contados (cantidad × tara vigente de cada tipo).</summary>
    public decimal TaraEnvasesKg { get; private set; }

    /// <summary>Pesada del camión entero repartida entre varias líneas: todas las del reparto llevan el mismo grupo.</summary>
    public Guid? GrupoCamion { get; private set; }

    /// <summary>Bruto de la báscula del camión entero (el de esta línea es su parte).</summary>
    public decimal? BrutoCamionKg { get; private set; }

    internal void AgruparEn(Guid grupo, decimal brutoCamion)
    {
        GrupoCamion = grupo;
        BrutoCamionKg = brutoCamion;
    }

    internal void Desglosar(decimal taraCamion, decimal taraEnvases)
    {
        TaraCamionKg = taraCamion;
        TaraEnvasesKg = taraEnvases;
        TaraKg = taraCamion + taraEnvases;
    }
}

/// <summary>Envases de un tipo contados en una pesada, con la tara unitaria y la versión de tara aplicada.</summary>
public sealed class EnvasePesada : EntidadBase<Guid>
{
    private EnvasePesada(Guid id)
        : base(id)
    {
    }

    internal EnvasePesada(Guid id, Guid pesadaId, Guid envaseProductoId, int cantidad, decimal taraUnitariaKg, Guid? taraEnvaseId)
        : base(id)
    {
        PesadaId = pesadaId;
        EnvaseProductoId = envaseProductoId;
        Cantidad = cantidad;
        TaraUnitariaKg = taraUnitariaKg;
        TaraEnvaseId = taraEnvaseId;
    }

    public Guid PesadaId { get; private set; }

    public Guid EnvaseProductoId { get; private set; }

    public int Cantidad { get; private set; }

    public decimal TaraUnitariaKg { get; private set; }

    /// <summary>Versión de la tara del envase aplicada (null si la tara unitaria se indicó a mano).</summary>
    public Guid? TaraEnvaseId { get; private set; }

    public decimal TaraKg => Cantidad * TaraUnitariaKg;

    internal void Aplicar(decimal taraUnitaria, Guid taraId)
    {
        TaraUnitariaKg = taraUnitaria;
        TaraEnvaseId = taraId;
    }
}

/// <summary>
/// Palé (o palot) que llega en una línea de recepción, contado uno a uno: su serie de origen, el envase y los envases que
/// trae y, si se pesó solo, sus kilos. Al confirmar pasa a ser un palé con SSCC que lleva sus kilos de la partida.
/// </summary>
public sealed class PaleEntrada : EntidadBase<Guid>
{
    private PaleEntrada(Guid id)
        : base(id)
    {
    }

    internal PaleEntrada(Guid id, Guid lineaId, int numero, string? serieOrigen, Guid? envaseProductoId, int envases, decimal? kilosNetos)
        : base(id)
    {
        LineaId = lineaId;
        Numero = numero;
        SerieOrigen = serieOrigen;
        EnvaseProductoId = envaseProductoId;
        Envases = envases;
        KilosNetos = kilosNetos;
    }

    public Guid LineaId { get; private set; }

    public int Numero { get; private set; }

    /// <summary>Número de serie de la etiqueta con que llega (del proveedor o de la finca).</summary>
    public string? SerieOrigen { get; private set; }

    public Guid? EnvaseProductoId { get; private set; }

    /// <summary>Envases reales del palé (contados, no un factor del artículo).</summary>
    public int Envases { get; private set; }

    /// <summary>Kilos netos si el palé se pesó solo.</summary>
    public decimal? KilosNetos { get; private set; }

    /// <summary>Palé (SSCC) creado al confirmar.</summary>
    public Guid? PaleId { get; private set; }

    /// <summary>Kilos de la partida que lleva (los pesados o su parte del neto de la línea).</summary>
    public decimal? KilosAsignados { get; private set; }

    internal void Fijar(Guid? paleId, decimal kilos)
    {
        PaleId = paleId;
        KilosAsignados = kilos;
    }
}
