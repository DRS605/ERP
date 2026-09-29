using AlxorCore.Agro.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Agro.Aplicacion;

public interface IRepositorioSiex
{
    Task<ExplotacionSiex?> ExplotacionAsync(Guid agricultorId, CancellationToken ct = default);

    void Agregar(ExplotacionSiex explotacion);

    Task<IReadOnlyList<AnalisisAgro>> AnalisisAsync(Guid empresaId, IReadOnlyCollection<Guid> parcelas, CancellationToken ct = default);

    Task<AnalisisAgro?> AnalisisPorIdAsync(Guid id, CancellationToken ct = default);

    void Agregar(AnalisisAgro analisis);

    void Eliminar(AnalisisAgro analisis);

    Task<IReadOnlyList<PlanAbonado>> PlanesAbonadoAsync(Guid empresaId, IReadOnlyCollection<Guid> parcelas, int? anio, CancellationToken ct = default);

    Task<PlanAbonado?> PlanAbonadoAsync(Guid id, CancellationToken ct = default);

    void Agregar(PlanAbonado plan);

    void Eliminar(PlanAbonado plan);
}

public sealed record ExplotacionSiexDto(Guid AgricultorId, string Agricultor, string? Nif, string? CodigoRegepa, string? AsesorNombre, string? AsesorRopo, string? CarneAplicador,
    string? EquipoRoma, bool PlanAbonadoObligatorio);

public sealed record AnalisisDto(Guid Id, Guid ParcelaId, string Parcela, DateOnly Fecha, TipoAnalisis Tipo, string Laboratorio, string? Boletin, decimal? NitrogenoKgHa,
    decimal? MateriaOrganicaPct, decimal? Ph, bool? SuperaLimites, string? Conclusion);

public sealed record DatosNuevoAnalisis(Guid ParcelaId, DatosAnalisis Analisis);

/// <summary>Plan de abonado de una parcela en un año: se crea o se cambia (hay uno por parcela y año).</summary>
public sealed record DatosPlanAbonadoParcela(Guid ParcelaId, int Anio, decimal NitrogenoKgHa, decimal FosforoKgHa, decimal PotasioKgHa, decimal? ProduccionEsperadaKgHa = null,
    string? Observaciones = null);

/// <summary>Plan de abonado de la parcela frente a lo aportado en sus abonados del año (kg/ha de N, P₂O₅ y K₂O).</summary>
public sealed record BalanceAbonadoDto(Guid ParcelaId, string Parcela, int Anio, Guid? PlanId, decimal? ProduccionEsperadaKgHa, decimal? PlanN, decimal? PlanP, decimal? PlanK,
    decimal AportadoN, decimal AportadoP, decimal AportadoK, int Abonados)
{
    public decimal? DiferenciaN => PlanN is { } n ? AportadoN - n : null;

    public decimal? DiferenciaP => PlanP is { } p ? AportadoP - p : null;

    public decimal? DiferenciaK => PlanK is { } k ? AportadoK - k : null;
}

/// <summary>Algo que falta o no cuadra para el cuaderno digital. Un error impide darlo por completo; un aviso, no.</summary>
public sealed record IncidenciaSiexDto(string Nivel, string Ambito, Guid? Id, string Referencia, string Codigo, string Mensaje);

public sealed record ValidacionSiexDto(Guid AgricultorId, int Anio, int Errores, int Avisos, IReadOnlyList<IncidenciaSiexDto> Incidencias)
{
    public bool Completo => Errores == 0;
}

public sealed record SigpacDto(int Provincia, int Municipio, int Agregado, int Zona, int Poligono, int Parcela, int Recinto);

public sealed record ParcelaSiexDto(string Codigo, string Nombre, string? ReferenciaSigpac, SigpacDto? Sigpac, decimal? SuperficieHa, string? Cultivo, string? Variedad,
    SistemaCultivo? Sistema, ModoCultivo? Modo, TipoProduccion Produccion);

public sealed record LaborSiexDto(Guid Id, string Parcela, string? ReferenciaSigpac, DateOnly Fecha, string Tipo, string Producto, string? NumeroRegistro, string? MateriaActiva,
    string? Cultivo, string? Plaga, decimal? Dosis, string? UnidadDosis, decimal? SuperficieHa, int PlazoSeguridadDias, string? Aplicador, string? CarneAplicador,
    string? EquipoRoma, string? Asesor, EficaciaTratamiento? Eficacia, TipoFertilizante? TipoFertilizante, string? MetodoAplicacion, decimal? NitrogenoKgHa,
    decimal? FosforoKgHa, decimal? PotasioKgHa, decimal? VolumenM3, string? Observaciones);

public sealed record CosechaSiexDto(string Parcela, string? ReferenciaSigpac, DateOnly Fecha, string Producto, decimal Kilos, string Destino, string Documento);

public sealed record ExplotacionCuadernoDto(string Titular, string? Nif, string? Domicilio, string? CodigoRegepa, string? Asesor, string? AsesorRopo);

/// <summary>
/// Cuaderno digital de explotación de un agricultor en un año, con los bloques del RD 1054/2022: explotación, recintos,
/// tratamientos fitosanitarios, fertilización, riego, otras labores, cosecha, análisis y plan de abonado. Es el fichero
/// que se entrega (o se sube) al SIEX por la vía de la comunidad autónoma, con las incidencias que quedan pendientes.
/// </summary>
public sealed record CuadernoSiexDto(string Formato, int Version, DateTimeOffset Generado, int Anio, ExplotacionCuadernoDto Explotacion, IReadOnlyList<ParcelaSiexDto> Parcelas,
    IReadOnlyList<LaborSiexDto> TratamientosFitosanitarios, IReadOnlyList<LaborSiexDto> Fertilizaciones, IReadOnlyList<LaborSiexDto> Riegos, IReadOnlyList<LaborSiexDto> OtrasLabores,
    IReadOnlyList<CosechaSiexDto> Cosechas, IReadOnlyList<AnalisisDto> Analisis, IReadOnlyList<BalanceAbonadoDto> PlanesAbonado, IReadOnlyList<IncidenciaSiexDto> Incidencias);

/// <summary>
/// Cuaderno digital de explotación (SIEX): datos de la explotación, análisis, planes de abonado con su balance, la
/// comprobación de que el cuaderno del año está completo y su exportación. Los tratamientos, abonados, riegos y
/// labores son los del cuaderno de campo; la cosecha, las entregas recibidas de sus parcelas.
/// </summary>
public sealed class SiexAgro
{
    public const string Formato = "ALXOR-CUE";
    public const int VersionFormato = 1;

    /// <summary>Días tras el tratamiento a partir de los que se echa en falta su eficacia.</summary>
    public const int DiasParaEficacia = 15;

    private readonly IRepositorioSiex _siex;
    private readonly IRepositorioAgro _agro;
    private readonly IRepositorioCuaderno _cuaderno;
    private readonly IUnidadDeTrabajoAgro _unidad;
    private readonly IConsultaProveedores _proveedores;
    private readonly CuadernoCampoAgro _campo;
    private readonly IReloj _reloj;
    private readonly AlxorCore.Catalogo.Aplicacion.IConsultaProductos? _productos;

    public SiexAgro(IRepositorioSiex siex, IRepositorioAgro agro, IRepositorioCuaderno cuaderno, IUnidadDeTrabajoAgro unidad, IConsultaProveedores proveedores,
        CuadernoCampoAgro campo, IReloj reloj, AlxorCore.Catalogo.Aplicacion.IConsultaProductos? productos = null)
    {
        _siex = siex;
        _agro = agro;
        _cuaderno = cuaderno;
        _unidad = unidad;
        _proveedores = proveedores;
        _campo = campo;
        _reloj = reloj;
        _productos = productos;
    }

    private DateOnly Hoy => DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);

    // ------------------------------------------------------------------ Explotación
    public async Task<Resultado<ExplotacionSiexDto>> ExplotacionAsync(Guid empresaId, Guid agricultorId, CancellationToken ct = default)
    {
        var agricultor = await _agro.AgricultorAsync(agricultorId, ct).ConfigureAwait(false);
        if (agricultor is null || agricultor.EmpresaId != empresaId)
        {
            return Resultado.Fallo<ExplotacionSiexDto>(NoAgricultor());
        }

        var e = await _siex.ExplotacionAsync(agricultorId, ct).ConfigureAwait(false);
        var nif = (await _proveedores.ObtenerAsync(agricultor.ProveedorId, ct).ConfigureAwait(false))?.NifFiscal;
        return Resultado.Ok(new ExplotacionSiexDto(agricultor.Id, agricultor.Nombre, nif, e?.CodigoRegepa, e?.AsesorNombre, e?.AsesorRopo, e?.CarneAplicador, e?.EquipoRoma,
            e?.PlanAbonadoObligatorio ?? false));
    }

    public async Task<Resultado<ExplotacionSiexDto>> FijarExplotacionAsync(Guid empresaId, Guid agricultorId, DatosExplotacionSiex d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var agricultor = await _agro.AgricultorAsync(agricultorId, ct).ConfigureAwait(false);
        if (agricultor is null || agricultor.EmpresaId != empresaId)
        {
            return Resultado.Fallo<ExplotacionSiexDto>(NoAgricultor());
        }

        var e = await _siex.ExplotacionAsync(agricultorId, ct).ConfigureAwait(false);
        var nueva = e is null;
        e ??= ExplotacionSiex.Nueva(empresaId, agricultorId);
        var r = e.Fijar(d);
        if (r.EsFallo)
        {
            return Resultado.Fallo<ExplotacionSiexDto>(r.Error);
        }

        if (nueva)
        {
            _siex.Agregar(e);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return await ExplotacionAsync(empresaId, agricultorId, ct).ConfigureAwait(false);
    }

    // ------------------------------------------------------------------ Análisis
    public async Task<IReadOnlyList<AnalisisDto>> AnalisisAsync(Guid empresaId, Guid? agricultorId, Guid? parcelaId, CancellationToken ct = default)
    {
        var parcelas = (await _agro.ParcelasAsync(empresaId, agricultorId, ct).ConfigureAwait(false)).Where(p => parcelaId is null || p.Id == parcelaId).ToDictionary(p => p.Id);
        return (await _siex.AnalisisAsync(empresaId, parcelas.Keys, ct).ConfigureAwait(false)).OrderByDescending(a => a.Fecha)
            .Select(a => Dto(a, parcelas.GetValueOrDefault(a.ParcelaId))).ToList();
    }

    public async Task<Resultado<AnalisisDto>> CrearAnalisisAsync(Guid empresaId, DatosNuevoAnalisis d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var parcela = await _agro.ParcelaAsync(d.ParcelaId, ct).ConfigureAwait(false);
        if (parcela is null || parcela.EmpresaId != empresaId)
        {
            return Resultado.Fallo<AnalisisDto>(NoParcela());
        }

        var a = AnalisisAgro.Crear(empresaId, parcela.Id, d.Analisis);
        if (a.EsFallo)
        {
            return Resultado.Fallo<AnalisisDto>(a.Error);
        }

        _siex.Agregar(a.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(a.Valor, parcela));
    }

    public async Task<Resultado<AnalisisDto>> CambiarAnalisisAsync(Guid id, DatosAnalisis d, CancellationToken ct = default)
    {
        var a = await _siex.AnalisisPorIdAsync(id, ct).ConfigureAwait(false);
        if (a is null)
        {
            return Resultado.Fallo<AnalisisDto>(NoAnalisis());
        }

        var r = a.Cambiar(d);
        if (r.EsFallo)
        {
            return Resultado.Fallo<AnalisisDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(a, await _agro.ParcelaAsync(a.ParcelaId, ct).ConfigureAwait(false)));
    }

    public async Task<Resultado> EliminarAnalisisAsync(Guid id, CancellationToken ct = default)
    {
        var a = await _siex.AnalisisPorIdAsync(id, ct).ConfigureAwait(false);
        if (a is null)
        {
            return Resultado.Fallo(NoAnalisis());
        }

        _siex.Eliminar(a);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    // ------------------------------------------------------------------ Plan de abonado
    /// <summary>Crea o cambia el plan de abonado de la parcela en el año.</summary>
    public async Task<Resultado<BalanceAbonadoDto>> FijarPlanAbonadoAsync(Guid empresaId, DatosPlanAbonadoParcela d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var parcela = await _agro.ParcelaAsync(d.ParcelaId, ct).ConfigureAwait(false);
        if (parcela is null || parcela.EmpresaId != empresaId)
        {
            return Resultado.Fallo<BalanceAbonadoDto>(NoParcela());
        }

        var datos = new DatosPlanAbonado(d.NitrogenoKgHa, d.FosforoKgHa, d.PotasioKgHa, d.ProduccionEsperadaKgHa, d.Observaciones);
        var existentes = await _siex.PlanesAbonadoAsync(empresaId, [parcela.Id], d.Anio, ct).ConfigureAwait(false);
        var plan = existentes.Count > 0 ? existentes[0] : null;
        if (plan is null)
        {
            var nuevo = PlanAbonado.Crear(empresaId, parcela.Id, d.Anio, datos);
            if (nuevo.EsFallo)
            {
                return Resultado.Fallo<BalanceAbonadoDto>(nuevo.Error);
            }

            _siex.Agregar(nuevo.Valor);
        }
        else
        {
            plan = await _siex.PlanAbonadoAsync(plan.Id, ct).ConfigureAwait(false);
            var r = plan!.Cambiar(datos);
            if (r.EsFallo)
            {
                return Resultado.Fallo<BalanceAbonadoDto>(r.Error);
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok((await BalanceAsync(empresaId, parcela.AgricultorId, d.Anio, ct).ConfigureAwait(false)).First(b => b.ParcelaId == parcela.Id));
    }

    public async Task<Resultado> EliminarPlanAbonadoAsync(Guid id, CancellationToken ct = default)
    {
        var plan = await _siex.PlanAbonadoAsync(id, ct).ConfigureAwait(false);
        if (plan is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("plan_abonado.no_encontrado", "El plan de abonado no existe."));
        }

        _siex.Eliminar(plan);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    /// <summary>Por parcela del agricultor: el plan de abonado del año y lo aportado en sus abonados (no anulados).</summary>
    public async Task<IReadOnlyList<BalanceAbonadoDto>> BalanceAsync(Guid empresaId, Guid agricultorId, int anio, CancellationToken ct = default)
    {
        var parcelas = await _agro.ParcelasAsync(empresaId, agricultorId, ct).ConfigureAwait(false);
        var ids = parcelas.Select(p => p.Id).ToList();
        var planes = (await _siex.PlanesAbonadoAsync(empresaId, ids, anio, ct).ConfigureAwait(false)).ToDictionary(p => p.ParcelaId);
        var abonados = (await _cuaderno.ListarAsync(empresaId, ids, new DateOnly(anio, 1, 1), new DateOnly(anio, 12, 31), ct).ConfigureAwait(false))
            .Where(t => !t.Anulado && t.Tipo == TipoLabor.Abonado).ToLookup(t => t.ParcelaId);
        return parcelas.OrderBy(p => p.Codigo, StringComparer.Ordinal).Select(p =>
        {
            var plan = planes.GetValueOrDefault(p.Id);
            var suyos = abonados[p.Id].ToList();
            return new BalanceAbonadoDto(p.Id, p.Codigo, anio, plan?.Id, plan?.ProduccionEsperadaKgHa, plan?.NitrogenoKgHa, plan?.FosforoKgHa, plan?.PotasioKgHa,
                suyos.Sum(t => t.NitrogenoKgHa ?? 0m), suyos.Sum(t => t.FosforoKgHa ?? 0m), suyos.Sum(t => t.PotasioKgHa ?? 0m), suyos.Count);
        }).ToList();
    }

    // ------------------------------------------------------------------ Validación y exportación
    /// <summary>
    /// Comprueba que el cuaderno del año esté completo para el SIEX: la explotación (REGEPA y NIF del titular), los
    /// recintos (SIGPAC, superficie, cultivo y sistema), cada labor con lo que pide (en un fitosanitario: número de
    /// registro, plaga, dosis, superficie, carné del aplicador, equipo ROMA, asesor y eficacia; en un abonado: tipo,
    /// cantidad y unidades fertilizantes; en un riego: el volumen), las recolecciones fuera de plazo y el plan de abonado.
    /// </summary>
    public async Task<Resultado<ValidacionSiexDto>> ValidarAsync(Guid empresaId, Guid agricultorId, int anio, CancellationToken ct = default)
    {
        var datos = await DatosAsync(empresaId, agricultorId, anio, ct).ConfigureAwait(false);
        if (datos.EsFallo)
        {
            return Resultado.Fallo<ValidacionSiexDto>(datos.Error);
        }

        var l = datos.Valor.Incidencias;
        return Resultado.Ok(new ValidacionSiexDto(agricultorId, anio, l.Count(i => i.Nivel == "Error"), l.Count(i => i.Nivel == "Aviso"), l));
    }

    /// <summary>El cuaderno digital del año en el formato de intercambio de ALXOR, con sus incidencias.</summary>
    public async Task<Resultado<CuadernoSiexDto>> CuadernoAsync(Guid empresaId, Guid agricultorId, int anio, CancellationToken ct = default) =>
        await DatosAsync(empresaId, agricultorId, anio, ct).ConfigureAwait(false);

    private async Task<Resultado<CuadernoSiexDto>> DatosAsync(Guid empresaId, Guid agricultorId, int anio, CancellationToken ct)
    {
        var agricultor = await _agro.AgricultorAsync(agricultorId, ct).ConfigureAwait(false);
        if (agricultor is null || agricultor.EmpresaId != empresaId)
        {
            return Resultado.Fallo<CuadernoSiexDto>(NoAgricultor());
        }

        if (anio is < 2000 or > 2100)
        {
            return Resultado.Fallo<CuadernoSiexDto>(Error.Validacion("siex.anio", "Año no válido."));
        }

        var desde = new DateOnly(anio, 1, 1);
        var hasta = new DateOnly(anio, 12, 31);
        var incidencias = new List<IncidenciaSiexDto>();
        void Falta(string ambito, Guid? id, string referencia, string codigo, string mensaje) => incidencias.Add(new("Error", ambito, id, referencia, codigo, mensaje));
        void Avisa(string ambito, Guid? id, string referencia, string codigo, string mensaje) => incidencias.Add(new("Aviso", ambito, id, referencia, codigo, mensaje));

        // Explotación
        var explotacion = await _siex.ExplotacionAsync(agricultorId, ct).ConfigureAwait(false);
        var proveedor = await _proveedores.ObtenerAsync(agricultor.ProveedorId, ct).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(explotacion?.CodigoRegepa))
        {
            Falta("Explotacion", agricultor.Id, agricultor.Nombre, "siex.regepa", "Falta el código de la explotación en el REGEPA.");
        }

        if (string.IsNullOrWhiteSpace(proveedor?.NifFiscal))
        {
            Falta("Explotacion", agricultor.Id, agricultor.Nombre, "siex.nif", "El titular no tiene NIF en su ficha.");
        }

        // Recintos: los activos y los que tienen algo en el año.
        var todas = await _agro.ParcelasAsync(empresaId, agricultorId, ct).ConfigureAwait(false);
        var ids = todas.Select(p => p.Id).ToList();
        var labores = (await _cuaderno.ListarAsync(empresaId, ids, desde, hasta, ct).ConfigureAwait(false)).Where(t => !t.Anulado).OrderBy(t => t.Fecha).ToList();
        var cuaderno = await _campo.CuadernoAsync(empresaId, agricultorId, desde, hasta, ct).ConfigureAwait(false);
        var recolecciones = cuaderno.EsCorrecto ? cuaderno.Valor.Recolecciones : [];
        var conActividad = labores.Select(t => t.ParcelaId).Concat(recolecciones.Select(r => r.ParcelaId)).ToHashSet();
        var parcelas = todas.Where(p => p.Activa || conActividad.Contains(p.Id)).OrderBy(p => p.Codigo, StringComparer.Ordinal).ToList();
        var porId = todas.ToDictionary(p => p.Id);
        var cultivos = new Dictionary<Guid, string?>();
        foreach (var productoId in parcelas.Where(p => p.ProductoId is not null).Select(p => p.ProductoId!.Value).Distinct())
        {
            cultivos[productoId] = _productos is null ? null : (await _productos.ObtenerAsync(productoId, ct).ConfigureAwait(false))?.Nombre;
        }

        foreach (var p in parcelas)
        {
            if (Sigpac(p.ReferenciaSigpac) is null)
            {
                Falta("Parcela", p.Id, p.Codigo, "siex.sigpac", "Falta la referencia SIGPAC del recinto.");
            }

            if (p.SuperficieHa is null)
            {
                Falta("Parcela", p.Id, p.Codigo, "siex.superficie", "Falta la superficie.");
            }

            if (p.ProductoId is null)
            {
                Falta("Parcela", p.Id, p.Codigo, "siex.cultivo", "Falta el cultivo.");
            }

            if (p.Sistema is null)
            {
                Avisa("Parcela", p.Id, p.Codigo, "siex.sistema", "Indica si es secano o regadío.");
            }
        }

        // Labores
        foreach (var t in labores)
        {
            var refe = $"{porId.GetValueOrDefault(t.ParcelaId)?.Codigo ?? "?"} · {t.Fecha:dd/MM/yyyy} · {t.Producto}";
            switch (t.Tipo)
            {
                case TipoLabor.Fitosanitario:
                    if (string.IsNullOrWhiteSpace(t.NumeroRegistro))
                    {
                        Falta("Labor", t.Id, refe, "siex.registro", "Falta el número de registro del producto.");
                    }

                    if (string.IsNullOrWhiteSpace(t.Motivo))
                    {
                        Falta("Labor", t.Id, refe, "siex.plaga", "Falta la plaga o el motivo del tratamiento.");
                    }

                    if (t.Dosis is null || string.IsNullOrWhiteSpace(t.UnidadDosis))
                    {
                        Falta("Labor", t.Id, refe, "siex.dosis", "Falta la dosis con su unidad.");
                    }

                    if (t.SuperficieTratadaHa is null)
                    {
                        Falta("Labor", t.Id, refe, "siex.superficie_tratada", "Falta la superficie tratada.");
                    }

                    if (string.IsNullOrWhiteSpace(t.CarneAplicador))
                    {
                        Falta("Labor", t.Id, refe, "siex.aplicador", "Falta el carné de aplicador (ROPO).");
                    }

                    if (string.IsNullOrWhiteSpace(t.EquipoRoma))
                    {
                        Avisa("Labor", t.Id, refe, "siex.roma", "Falta el equipo de aplicación (ROMA).");
                    }

                    if (string.IsNullOrWhiteSpace(t.Asesor))
                    {
                        Avisa("Labor", t.Id, refe, "siex.asesor", "Falta el asesor que lo recomendó.");
                    }

                    if (t.Eficacia is null && t.Fecha.AddDays(DiasParaEficacia) <= Hoy)
                    {
                        Avisa("Labor", t.Id, refe, "siex.eficacia", "Anota la eficacia del tratamiento.");
                    }

                    break;
                case TipoLabor.Abonado:
                    if (t.TipoFertilizante is null)
                    {
                        Falta("Labor", t.Id, refe, "siex.fertilizante", "Falta el tipo de fertilizante.");
                    }

                    if (t.Dosis is null || string.IsNullOrWhiteSpace(t.UnidadDosis))
                    {
                        Falta("Labor", t.Id, refe, "siex.cantidad", "Falta la cantidad de fertilizante con su unidad.");
                    }

                    if (t.NitrogenoKgHa is null && t.FosforoKgHa is null && t.PotasioKgHa is null)
                    {
                        Avisa("Labor", t.Id, refe, "siex.unidades", "Faltan las unidades fertilizantes (N, P₂O₅, K₂O).");
                    }

                    if (string.IsNullOrWhiteSpace(t.MetodoAplicacion))
                    {
                        Avisa("Labor", t.Id, refe, "siex.metodo", "Falta el método de aplicación.");
                    }

                    break;
                case TipoLabor.Riego:
                    if (t.VolumenM3 is null)
                    {
                        Falta("Labor", t.Id, refe, "siex.volumen", "Falta el volumen de agua.");
                    }

                    break;
                default:
                    break;
            }
        }

        foreach (var r in recolecciones.Where(r => r.Incidencia is not null))
        {
            Falta("Cosecha", r.RecepcionId, $"{r.Parcela} · {r.Fecha:dd/MM/yyyy}", "siex.plazo_seguridad", r.Incidencia!);
        }

        // Plan de abonado
        var balance = (await BalanceAsync(empresaId, agricultorId, anio, ct).ConfigureAwait(false)).Where(b => parcelas.Any(p => p.Id == b.ParcelaId)).ToList();
        foreach (var b in balance)
        {
            if (b.PlanId is null && (explotacion?.PlanAbonadoObligatorio ?? false) && (b.Abonados > 0 || porId[b.ParcelaId].Activa))
            {
                Falta("PlanAbonado", b.ParcelaId, b.Parcela, "siex.plan_abonado", $"La explotación está obligada al plan de abonado y la parcela no tiene el de {anio}.");
            }

            if (b.PlanN is { } n && b.AportadoN > n * 1.1m)
            {
                Avisa("PlanAbonado", b.ParcelaId, b.Parcela, "siex.exceso_nitrogeno",
                    $"Se han aportado {b.AportadoN:0.##} kg N/ha y el plan prevé {n:0.##} (más de un 10 % por encima).");
            }
        }

        var analisis = (await _siex.AnalisisAsync(empresaId, ids, ct).ConfigureAwait(false)).Where(a => a.Fecha >= desde && a.Fecha <= hasta).OrderBy(a => a.Fecha)
            .Select(a => Dto(a, porId.GetValueOrDefault(a.ParcelaId))).ToList();
        foreach (var a in analisis.Where(a => a.SuperaLimites == true))
        {
            Avisa("Analisis", a.Id, $"{a.Parcela} · {a.Fecha:dd/MM/yyyy}", "siex.residuos", "El análisis de residuos supera los límites.");
        }

        var domicilio = proveedor is null ? null
            : string.Join(", ", new[] { proveedor.Calle, $"{proveedor.CodigoPostal} {proveedor.Poblacion}".Trim(), proveedor.Provincia }.Where(x => !string.IsNullOrWhiteSpace(x)));
        LaborSiexDto L(TratamientoParcela t)
        {
            var p = porId.GetValueOrDefault(t.ParcelaId);
            return new LaborSiexDto(t.Id, p?.Codigo ?? "?", p?.ReferenciaSigpac, t.Fecha, t.Tipo.ToString(), t.Producto, t.NumeroRegistro, t.MateriaActiva, t.Cultivo, t.Motivo, t.Dosis,
                t.UnidadDosis, t.SuperficieTratadaHa, t.PlazoSeguridadDias, t.Aplicador, t.CarneAplicador, t.EquipoRoma, t.Asesor, t.Eficacia, t.TipoFertilizante,
                t.MetodoAplicacion, t.NitrogenoKgHa, t.FosforoKgHa, t.PotasioKgHa, t.VolumenM3, t.Observaciones);
        }

        return Resultado.Ok(new CuadernoSiexDto(Formato, VersionFormato, _reloj.AhoraUtc, anio,
            new ExplotacionCuadernoDto(proveedor?.Nombre ?? agricultor.Nombre, proveedor?.NifFiscal, string.IsNullOrWhiteSpace(domicilio) ? null : domicilio, explotacion?.CodigoRegepa,
                explotacion?.AsesorNombre, explotacion?.AsesorRopo),
            parcelas.Select(p => new ParcelaSiexDto(p.Codigo, p.Nombre, p.ReferenciaSigpac, Sigpac(p.ReferenciaSigpac), p.SuperficieHa,
                p.ProductoId is { } c ? cultivos.GetValueOrDefault(c) : null, p.Variedad, p.Sistema, p.Modo, p.Produccion)).ToList(),
            labores.Where(t => t.Tipo == TipoLabor.Fitosanitario).Select(L).ToList(), labores.Where(t => t.Tipo == TipoLabor.Abonado).Select(L).ToList(),
            labores.Where(t => t.Tipo == TipoLabor.Riego).Select(L).ToList(), labores.Where(t => t.Tipo == TipoLabor.Otra).Select(L).ToList(),
            recolecciones.Select(r => new CosechaSiexDto(r.Parcela, porId.GetValueOrDefault(r.ParcelaId)?.ReferenciaSigpac, r.Fecha, r.Producto, r.NetoKg, "Entrega al almacén",
                r.Recepcion)).ToList(),
            analisis, balance, incidencias));
    }

    /// <summary>Los siete números de la referencia SIGPAC (provincia:municipio:agregado:zona:polígono:parcela:recinto), o null.</summary>
    public static SigpacDto? Sigpac(string? referencia)
    {
        var partes = (referencia ?? string.Empty).Split(':');
        if (partes.Length != 7 || !partes.All(x => int.TryParse(x, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out _)))
        {
            return null;
        }

        var n = partes.Select(x => int.Parse(x, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
        return new SigpacDto(n[0], n[1], n[2], n[3], n[4], n[5], n[6]);
    }

    private static AnalisisDto Dto(AnalisisAgro a, Parcela? p) => new(a.Id, a.ParcelaId, p?.Codigo ?? "?", a.Fecha, a.Tipo, a.Laboratorio, a.Boletin, a.NitrogenoKgHa,
        a.MateriaOrganicaPct, a.Ph, a.SuperaLimites, a.Conclusion);

    private static Error NoAgricultor() => Error.NoEncontrado("agricultor.no_encontrado", "El agricultor no existe.");

    private static Error NoParcela() => Error.NoEncontrado("parcela.no_encontrada", "La parcela no existe.");

    private static Error NoAnalisis() => Error.NoEncontrado("analisis.no_encontrado", "El análisis no existe.");
}
