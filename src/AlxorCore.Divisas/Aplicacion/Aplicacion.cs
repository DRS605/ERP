using AlxorCore.Divisas.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Divisas.Aplicacion;

/// <summary>Vista de una divisa del catálogo.</summary>
public sealed record DivisaDto(string Codigo, string Nombre, int Decimales)
{
    public static DivisaDto Desde(DivisaInfo d) => new(d.Codigo, d.Nombre, d.Decimales);
}

/// <summary>Vista de un tipo de cambio.</summary>
public sealed record TipoCambioDto(Guid Id, string Divisa, DateOnly Fecha, decimal TasaEur)
{
    public static TipoCambioDto Desde(TipoCambio t) => new(t.Id, t.Divisa, t.Fecha, t.TasaEur);
}

/// <summary>Unidad de trabajo del módulo Divisas.</summary>
public interface IUnidadDeTrabajoDivisas : IUnidadDeTrabajo;

/// <summary>Repositorio de tipos de cambio (escritura y listado).</summary>
public interface IRepositorioTiposCambio
{
    Task<TipoCambio?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Tipo de cambio exacto de una divisa en una fecha concreta (para crear o actualizar).</summary>
    Task<TipoCambio?> ObtenerPorDivisaFechaAsync(Guid empresaId, string divisa, DateOnly fecha, CancellationToken ct = default);

    void Agregar(TipoCambio tipoCambio);

    Task<IReadOnlyList<TipoCambio>> ListarAsync(Guid empresaId, CancellationToken ct = default);
}

/// <summary>Consulta de la tasa vigente (para conversiones).</summary>
public interface IConsultaTiposCambio
{
    /// <summary>Tasa (EUR por unidad) más reciente de una divisa con fecha ≤ la indicada, o null si no hay.</summary>
    Task<decimal?> TasaVigenteAsync(Guid empresaId, string divisa, DateOnly fecha, CancellationToken ct = default);
}

/// <summary>Caso de uso: listar el catálogo de divisas admitidas.</summary>
public static class ListarDivisas
{
    public static IReadOnlyList<DivisaDto> Ejecutar() => DivisasConocidas.Todas.Select(DivisaDto.Desde).ToList();
}

/// <summary>Caso de uso: registrar (o actualizar) el tipo de cambio de una divisa en una fecha.</summary>
public sealed class RegistrarTipoCambio
{
    private readonly IRepositorioTiposCambio _repositorio;
    private readonly IUnidadDeTrabajoDivisas _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public RegistrarTipoCambio(IRepositorioTiposCambio repositorio, IUnidadDeTrabajoDivisas unidadDeTrabajo, IReloj reloj)
    {
        _repositorio = repositorio;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<Resultado<TipoCambioDto>> EjecutarAsync(Guid empresaId, string? divisa, DateOnly fecha, decimal tasaEur, CancellationToken ct = default)
    {
        var codigo = DivisasConocidas.Normalizar(divisa);
        if (codigo is not null)
        {
            var existente = await _repositorio.ObtenerPorDivisaFechaAsync(empresaId, codigo, fecha, ct).ConfigureAwait(false);
            if (existente is not null)
            {
                var act = existente.Actualizar(tasaEur, _reloj);
                if (act.EsFallo)
                {
                    return Resultado.Fallo<TipoCambioDto>(act.Error);
                }

                await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
                return Resultado.Ok(TipoCambioDto.Desde(existente));
            }
        }

        var tipo = TipoCambio.Crear(empresaId, divisa, fecha, tasaEur, _reloj);
        if (tipo.EsFallo)
        {
            return Resultado.Fallo<TipoCambioDto>(tipo.Error);
        }

        _repositorio.Agregar(tipo.Valor);
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(TipoCambioDto.Desde(tipo.Valor));
    }
}

/// <summary>Caso de uso: listar los tipos de cambio de la empresa (más reciente primero).</summary>
public sealed class ListarTiposCambio
{
    private readonly IRepositorioTiposCambio _repositorio;

    public ListarTiposCambio(IRepositorioTiposCambio repositorio) => _repositorio = repositorio;

    public async Task<IReadOnlyList<TipoCambioDto>> EjecutarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var lista = await _repositorio.ListarAsync(empresaId, ct).ConfigureAwait(false);
        return lista.OrderByDescending(t => t.Fecha).ThenBy(t => t.Divisa).Select(TipoCambioDto.Desde).ToList();
    }
}

/// <summary>Resultado de una conversión a euros.</summary>
public sealed record ConversionDto(string Divisa, decimal Importe, DateOnly Fecha, decimal TasaEur, decimal ImporteEur);

/// <summary>Caso de uso: convertir un importe en divisa a euros a una fecha, con la tasa vigente.</summary>
public sealed class ConvertirImporte
{
    private readonly IConsultaTiposCambio _consulta;

    public ConvertirImporte(IConsultaTiposCambio consulta) => _consulta = consulta;

    public async Task<Resultado<ConversionDto>> EjecutarAsync(Guid empresaId, string? divisa, decimal importe, DateOnly fecha, CancellationToken ct = default)
    {
        var codigo = DivisasConocidas.Normalizar(divisa);
        if (codigo is null)
        {
            return Resultado.Fallo<ConversionDto>(Error.Validacion("divisa.desconocida", "La divisa no está en el catálogo admitido."));
        }

        if (DivisasConocidas.EsEuro(codigo))
        {
            return Resultado.Ok(new ConversionDto(codigo, importe, fecha, 1m, Redondeo.Dos(importe)));
        }

        var tasa = await _consulta.TasaVigenteAsync(empresaId, codigo, fecha, ct).ConfigureAwait(false);
        if (tasa is null)
        {
            return Resultado.Fallo<ConversionDto>(Error.NoEncontrado("divisa.sin_tasa", $"No hay tipo de cambio para {codigo} a esa fecha."));
        }

        return Resultado.Ok(new ConversionDto(codigo, importe, fecha, tasa.Value, CalculadoraDivisa.AEuros(importe, tasa.Value)));
    }
}

/// <summary>Una posición abierta en divisa a revalorizar (p. ej. una factura pendiente de cobro).</summary>
public sealed record PosicionDivisa(string Referencia, string Divisa, decimal ImporteDivisa, decimal TasaOrigen, bool EsActivo);

/// <summary>Comando para calcular diferencias de cambio de un conjunto de posiciones a una fecha.</summary>
public sealed record DiferenciasCambioComando(DateOnly FechaValoracion, IReadOnlyList<PosicionDivisa> Posiciones);

/// <summary>Diferencia de cambio calculada de una posición.</summary>
public sealed record LineaDiferenciaDto(
    string Referencia, string Divisa, decimal ImporteDivisa, decimal TasaOrigen, decimal TasaValoracion,
    decimal DiferenciaEur, bool EsIngreso, string Cuenta);

/// <summary>Resultado del cálculo de diferencias de cambio, con el asiento propuesto (668/768).</summary>
public sealed record DiferenciasCambioDto(
    DateOnly FechaValoracion, decimal TotalDiferencia, decimal TotalIngresos, decimal TotalGastos, IReadOnlyList<LineaDiferenciaDto> Lineas);

/// <summary>
/// Caso de uso: calcula las <b>diferencias de cambio</b> de una lista de posiciones abiertas en divisa
/// a una fecha de valoración (típicamente el cierre del ejercicio). Para cada posición usa la tasa de
/// origen indicada y la tasa vigente a la fecha de valoración; resume ingresos (768) y gastos (668).
/// </summary>
public sealed class CalcularDiferenciasCambio
{
    private readonly IConsultaTiposCambio _consulta;

    public CalcularDiferenciasCambio(IConsultaTiposCambio consulta) => _consulta = consulta;

    public async Task<Resultado<DiferenciasCambioDto>> EjecutarAsync(Guid empresaId, DiferenciasCambioComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var lineas = new List<LineaDiferenciaDto>();
        foreach (var p in comando.Posiciones ?? Array.Empty<PosicionDivisa>())
        {
            var codigo = DivisasConocidas.Normalizar(p.Divisa);
            if (codigo is null || DivisasConocidas.EsEuro(codigo))
            {
                continue; // el euro no genera diferencias de cambio
            }

            var tasa = await _consulta.TasaVigenteAsync(empresaId, codigo, comando.FechaValoracion, ct).ConfigureAwait(false);
            if (tasa is null)
            {
                return Resultado.Fallo<DiferenciasCambioDto>(Error.NoEncontrado("divisa.sin_tasa", $"No hay tipo de cambio para {codigo} a la fecha de valoración."));
            }

            var dif = CalculadoraDivisa.Diferencia(p.ImporteDivisa, p.TasaOrigen, tasa.Value, p.EsActivo);
            lineas.Add(new LineaDiferenciaDto(p.Referencia, codigo, p.ImporteDivisa, p.TasaOrigen, tasa.Value, dif.DiferenciaEur, dif.EsIngreso, dif.Cuenta));
        }

        var ingresos = Redondeo.Dos(lineas.Where(l => l.DiferenciaEur > 0m).Sum(l => l.DiferenciaEur));
        var gastos = Redondeo.Dos(lineas.Where(l => l.DiferenciaEur < 0m).Sum(l => -l.DiferenciaEur));
        var total = Redondeo.Dos(lineas.Sum(l => l.DiferenciaEur));
        return Resultado.Ok(new DiferenciasCambioDto(comando.FechaValoracion, total, ingresos, gastos, lineas));
    }
}

/// <summary>Implementación del puerto transversal <see cref="IConversorDivisa"/> sobre los tipos de cambio.</summary>
public sealed class ConversorDivisa : IConversorDivisa
{
    private readonly IConsultaTiposCambio _consulta;

    public ConversorDivisa(IConsultaTiposCambio consulta) => _consulta = consulta;

    public Task<decimal?> TasaVigenteAsync(Guid empresaId, string divisa, DateOnly fecha, CancellationToken ct = default)
    {
        var codigo = DivisasConocidas.Normalizar(divisa);
        if (codigo is null)
        {
            return Task.FromResult<decimal?>(null);
        }

        return DivisasConocidas.EsEuro(codigo) ? Task.FromResult<decimal?>(1m) : _consulta.TasaVigenteAsync(empresaId, codigo, fecha, ct);
    }

    public async Task<decimal?> AEurosAsync(Guid empresaId, string divisa, decimal importe, DateOnly fecha, CancellationToken ct = default)
    {
        var tasa = await TasaVigenteAsync(empresaId, divisa, fecha, ct).ConfigureAwait(false);
        return tasa is null ? null : CalculadoraDivisa.AEuros(importe, tasa.Value);
    }
}
