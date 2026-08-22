using AlxorCore.Contabilidad.Dominio;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Contabilidad.Aplicacion;

// --------------------------------------------------------------------------------------------
//  Cuentas por defecto del inmovilizado (además del plan básico)
// --------------------------------------------------------------------------------------------

/// <summary>Cuentas relacionadas con el inmovilizado y el impuesto diferido.</summary>
public static class PlanInmovilizado
{
    public const string PerdidasInmovilizado = "671";  // Pérdidas procedentes del inmovilizado material
    public const string BeneficiosInmovilizado = "771"; // Beneficios procedentes del inmovilizado material
    public const string ActivoImpuestoDiferido = "4740"; // Activos por diferencias temporarias deducibles
    public const string PasivoImpuestoDiferido = "479";  // Pasivos por diferencias temporarias imponibles
    public const string ImpuestoDiferido = "6301";        // Impuesto diferido
}

// --------------------------------------------------------------------------------------------
//  DTOs
// --------------------------------------------------------------------------------------------

/// <summary>Vista de un inmovilizado con sus importes derivados.</summary>
public sealed record InmovilizadoDto(
    Guid Id, string Codigo, string Descripcion, string CuentaActivo, string CuentaAmortizacion, string CuentaDotacion,
    DateOnly FechaAdquisicion, DateOnly FechaAlta, decimal ValorAdquisicion, decimal ValorResidual,
    string Periodicidad, string MetodoContable, int VidaUtilContable, decimal PorcentajeDegresivoContable,
    string MetodoFiscal, int VidaUtilFiscal, decimal PorcentajeDegresivoFiscal,
    string Estado, decimal AmortizacionAcumulada, decimal ValorNetoContable, DateOnly? FechaBaja, decimal? ValorEnajenacion)
{
    public static InmovilizadoDto Desde(Inmovilizado i) => new(
        i.Id, i.Codigo, i.Descripcion, i.CuentaActivo, i.CuentaAmortizacion, i.CuentaDotacion,
        i.FechaAdquisicion, i.FechaAlta, i.ValorAdquisicion, i.ValorResidual,
        i.Periodicidad.ToString(), i.Contable.Metodo.ToString(), i.Contable.VidaUtilAnios, i.Contable.PorcentajeDegresivo,
        i.Fiscal.Metodo.ToString(), i.Fiscal.VidaUtilAnios, i.Fiscal.PorcentajeDegresivo,
        i.Estado.ToString(), i.AmortizacionAcumulada, i.ValorNetoContable, i.FechaBaja, i.ValorEnajenacion);
}

/// <summary>Datos para crear un inmovilizado.</summary>
public sealed record CrearInmovilizadoComando(
    string Codigo, string Descripcion, string CuentaActivo, string CuentaAmortizacion, string CuentaDotacion,
    DateOnly FechaAdquisicion, DateOnly FechaAlta, decimal ValorAdquisicion, decimal ValorResidual,
    PeriodicidadAmortizacion Periodicidad,
    MetodoAmortizacion MetodoContable, int VidaUtilContable, decimal PorcentajeDegresivoContable,
    MetodoAmortizacion MetodoFiscal, int VidaUtilFiscal, decimal PorcentajeDegresivoFiscal);

/// <summary>Fila del cuadro de amortización de un ejercicio (contable vs fiscal).</summary>
public sealed record FilaCuadroDto(int Ejercicio, decimal Contable, decimal Fiscal, decimal DiferenciaTemporaria, decimal Contabilizado);

/// <summary>Cuadro de amortización completo de un inmovilizado.</summary>
public sealed record CuadroAmortizacionDto(
    Guid InmovilizadoId, string Codigo, string Descripcion, decimal BaseAmortizable,
    decimal AmortizacionAcumulada, decimal ValorNetoContable, IReadOnlyList<FilaCuadroDto> Filas);

/// <summary>Resultado de generar la amortización de un ejercicio.</summary>
public sealed record ResultadoAmortizacionDto(int Ejercicio, int AsientosGenerados, decimal TotalDotado, decimal TotalImpuestoDiferido);

/// <summary>Datos para dar de baja un inmovilizado (sin contraprestación).</summary>
public sealed record BajaInmovilizadoComando(DateOnly Fecha);

/// <summary>Datos para enajenar (vender) un inmovilizado.</summary>
public sealed record EnajenarInmovilizadoComando(DateOnly Fecha, decimal ValorEnajenacion, decimal PorcentajeIva = 0m, string? CuentaCobro = null);

// --------------------------------------------------------------------------------------------
//  Puerto
// --------------------------------------------------------------------------------------------

public interface IRepositorioInmovilizado
{
    void Agregar(Inmovilizado inmovilizado);

    Task<Inmovilizado?> ObtenerAsync(Guid empresaId, Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Inmovilizado>> ListarAsync(Guid empresaId, CancellationToken ct = default);

    /// <summary>Inmovilizados en estado Activo, con seguimiento para poder mutarlos.</summary>
    Task<IReadOnlyList<Inmovilizado>> ListarActivosAsync(Guid empresaId, CancellationToken ct = default);
}

// --------------------------------------------------------------------------------------------
//  Cuenta cerrada / helper de ejercicio
// --------------------------------------------------------------------------------------------

internal static class GuardaEjercicio
{
    public static async Task<bool> EstaCerradoAsync(IRepositorioAsientos asientos, Guid empresaId, int ejercicio, CancellationToken ct)
    {
        var existentes = await asientos.TodosAsync(empresaId, ejercicio, ct).ConfigureAwait(false);
        return existentes.Any(a => a.Origen == "Cierre");
    }
}

// --------------------------------------------------------------------------------------------
//  Casos de uso
// --------------------------------------------------------------------------------------------

/// <summary>Crea un inmovilizado y siembra las cuentas necesarias si faltan.</summary>
public sealed class CrearInmovilizado
{
    private readonly IRepositorioInmovilizado _inmovilizados;
    private readonly IRepositorioCuentas _cuentas;
    private readonly IUnidadDeTrabajoContabilidad _unidad;

    public CrearInmovilizado(IRepositorioInmovilizado inmovilizados, IRepositorioCuentas cuentas, IUnidadDeTrabajoContabilidad unidad)
    {
        _inmovilizados = inmovilizados;
        _cuentas = cuentas;
        _unidad = unidad;
    }

    public async Task<Resultado<InmovilizadoDto>> EjecutarAsync(Guid empresaId, CrearInmovilizadoComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var inmovilizado = Inmovilizado.Crear(empresaId, comando.Codigo, comando.Descripcion,
            comando.CuentaActivo, comando.CuentaAmortizacion, comando.CuentaDotacion,
            comando.FechaAdquisicion, comando.FechaAlta, comando.ValorAdquisicion, comando.ValorResidual,
            comando.Periodicidad,
            new PlanAmortizacion(comando.MetodoContable, comando.VidaUtilContable, comando.PorcentajeDegresivoContable),
            new PlanAmortizacion(comando.MetodoFiscal, comando.VidaUtilFiscal, comando.PorcentajeDegresivoFiscal));
        if (inmovilizado.EsFallo)
        {
            return Resultado.Fallo<InmovilizadoDto>(inmovilizado.Error);
        }

        await SembradorPlan.AsegurarAsync(empresaId, _cuentas, ct).ConfigureAwait(false);
        _inmovilizados.Agregar(inmovilizado.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(InmovilizadoDto.Desde(inmovilizado.Valor));
    }
}

/// <summary>Lista los inmovilizados de la empresa.</summary>
public sealed class ListarInmovilizados
{
    private readonly IRepositorioInmovilizado _inmovilizados;

    public ListarInmovilizados(IRepositorioInmovilizado inmovilizados) => _inmovilizados = inmovilizados;

    public async Task<IReadOnlyList<InmovilizadoDto>> EjecutarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var lista = await _inmovilizados.ListarAsync(empresaId, ct).ConfigureAwait(false);
        return lista.Select(InmovilizadoDto.Desde).ToList();
    }
}

/// <summary>Cuadro de amortización (contable, fiscal y diferencia) por ejercicio de un inmovilizado.</summary>
public sealed class ObtenerCuadroAmortizacion
{
    private readonly IRepositorioInmovilizado _inmovilizados;

    public ObtenerCuadroAmortizacion(IRepositorioInmovilizado inmovilizados) => _inmovilizados = inmovilizados;

    public async Task<Resultado<CuadroAmortizacionDto>> EjecutarAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        var inmovilizado = await _inmovilizados.ObtenerAsync(empresaId, id, ct).ConfigureAwait(false);
        if (inmovilizado is null)
        {
            return Resultado.Fallo<CuadroAmortizacionDto>(Error.NoEncontrado("inmovilizado.no_encontrado", "Inmovilizado no encontrado."));
        }

        var contable = CalculadoraAmortizacion.PlanMensual(inmovilizado.FechaAlta, inmovilizado.CuotasAnualesContable());
        var fiscal = CalculadoraAmortizacion.PlanMensual(inmovilizado.FechaAlta, inmovilizado.CuotasAnualesFiscal());

        var porEjercicioContable = contable.GroupBy(c => c.Ejercicio).ToDictionary(g => g.Key, g => Redondeo.Dos(g.Sum(x => x.Importe)));
        var porEjercicioFiscal = fiscal.GroupBy(c => c.Ejercicio).ToDictionary(g => g.Key, g => Redondeo.Dos(g.Sum(x => x.Importe)));
        var contabilizado = inmovilizado.Dotaciones.GroupBy(d => d.Ejercicio).ToDictionary(g => g.Key, g => Redondeo.Dos(g.Sum(x => x.Importe)));

        var ejercicios = porEjercicioContable.Keys.Union(porEjercicioFiscal.Keys).OrderBy(e => e).ToList();
        var filas = ejercicios.Select(e =>
        {
            var c = porEjercicioContable.GetValueOrDefault(e, 0m);
            var f = porEjercicioFiscal.GetValueOrDefault(e, 0m);
            return new FilaCuadroDto(e, c, f, Redondeo.Dos(f - c), contabilizado.GetValueOrDefault(e, 0m));
        }).ToList();

        return Resultado.Ok(new CuadroAmortizacionDto(inmovilizado.Id, inmovilizado.Codigo, inmovilizado.Descripcion,
            inmovilizado.BaseAmortizable, inmovilizado.AmortizacionAcumulada, inmovilizado.ValorNetoContable, filas));
    }
}

/// <summary>
/// Genera la amortización contable de un ejercicio (dotaciones) para todos los inmovilizados activos y,
/// opcionalmente, los asientos de impuesto diferido por la diferencia con la amortización fiscal.
/// Es idempotente: no vuelve a dotar un periodo ya contabilizado.
/// </summary>
public sealed class GenerarAmortizacion
{
    private readonly IRepositorioInmovilizado _inmovilizados;
    private readonly IRepositorioAsientos _asientos;
    private readonly IRepositorioCuentas _cuentas;
    private readonly IUnidadDeTrabajoContabilidad _unidad;
    private readonly IReloj _reloj;

    public GenerarAmortizacion(IRepositorioInmovilizado inmovilizados, IRepositorioAsientos asientos,
        IRepositorioCuentas cuentas, IUnidadDeTrabajoContabilidad unidad, IReloj reloj)
    {
        _inmovilizados = inmovilizados;
        _asientos = asientos;
        _cuentas = cuentas;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<Resultado<ResultadoAmortizacionDto>> EjecutarAsync(Guid empresaId, int ejercicio,
        decimal tipoImpositivo = 0.25m, bool impuestoDiferido = true, CancellationToken ct = default)
    {
        if (await GuardaEjercicio.EstaCerradoAsync(_asientos, empresaId, ejercicio, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<ResultadoAmortizacionDto>(Error.Conflicto("asiento.ejercicio_cerrado", $"El ejercicio {ejercicio} está cerrado; no admite nuevos asientos."));
        }

        await SembradorPlan.AsegurarAsync(empresaId, _cuentas, ct).ConfigureAwait(false);
        var activos = await _inmovilizados.ListarActivosAsync(empresaId, ct).ConfigureAwait(false);
        var numero = await _asientos.SiguienteNumeroAsync(empresaId, ejercicio, ct).ConfigureAwait(false);

        var generados = 0;
        decimal totalDotado = 0m, totalImpuesto = 0m;

        foreach (var inmo in activos)
        {
            var contableMensual = CalculadoraAmortizacion.PlanMensual(inmo.FechaAlta, inmo.CuotasAnualesContable())
                .Where(c => c.Ejercicio == ejercicio).ToList();
            if (contableMensual.Count == 0)
            {
                continue; // este inmovilizado no amortiza en este ejercicio
            }

            // 1) Dotación contable: un asiento por mes (mensual) o uno anual al 31/12.
            if (inmo.Periodicidad == PeriodicidadAmortizacion.Mensual)
            {
                foreach (var mes in contableMensual)
                {
                    if (inmo.DotacionExiste(mes.Ejercicio, mes.Mes))
                    {
                        continue;
                    }

                    var fecha = FinDeMes(mes.Ejercicio, mes.Mes);
                    var asiento = CrearDotacion(empresaId, ejercicio, ref numero, fecha, inmo, mes.Importe);
                    if (asiento.EsFallo)
                    {
                        return Resultado.Fallo<ResultadoAmortizacionDto>(asiento.Error);
                    }

                    inmo.RegistrarDotacion(mes.Ejercicio, mes.Mes, fecha, mes.Importe, asiento.Valor.Id);
                    generados++;
                    totalDotado = Redondeo.Dos(totalDotado + mes.Importe);
                }
            }
            else if (!inmo.DotacionExiste(ejercicio, 0))
            {
                var importe = Redondeo.Dos(contableMensual.Sum(c => c.Importe));
                var fecha = new DateOnly(ejercicio, 12, 31);
                var asiento = CrearDotacion(empresaId, ejercicio, ref numero, fecha, inmo, importe);
                if (asiento.EsFallo)
                {
                    return Resultado.Fallo<ResultadoAmortizacionDto>(asiento.Error);
                }

                inmo.RegistrarDotacion(ejercicio, 0, fecha, importe, asiento.Valor.Id);
                generados++;
                totalDotado = Redondeo.Dos(totalDotado + importe);
            }

            // 2) Impuesto diferido del ejercicio (una vez por año), sobre la diferencia contable-fiscal.
            if (impuestoDiferido && !inmo.AjusteFiscalExiste(ejercicio))
            {
                var contableAnual = Redondeo.Dos(contableMensual.Sum(c => c.Importe));
                var fiscalAnual = CalculadoraAmortizacion.ImporteDeEjercicio(inmo.FechaAlta, inmo.CuotasAnualesFiscal(), ejercicio);
                var diferencia = Redondeo.Dos(fiscalAnual - contableAnual);
                var patas = CalculadoraAmortizacion.AsientoImpuestoDiferido(
                    inmo.DiferenciaTemporariaAcumulada, diferencia, tipoImpositivo,
                    PlanInmovilizado.PasivoImpuestoDiferido, PlanInmovilizado.ActivoImpuestoDiferido, PlanInmovilizado.ImpuestoDiferido);
                if (patas.Count > 0)
                {
                    var lineas = patas.Select(p => new LineaAsiento(p.CuentaCodigo, p.Debe, p.Haber, "Impuesto diferido")).ToList();
                    var fecha = new DateOnly(ejercicio, 12, 31);
                    var asiento = Asiento.Crear(empresaId, ejercicio, numero, fecha,
                        $"Impuesto diferido amortización · {inmo.Codigo}", "ImpuestoDiferido", lineas, _reloj);
                    if (asiento.EsFallo)
                    {
                        return Resultado.Fallo<ResultadoAmortizacionDto>(asiento.Error);
                    }

                    _asientos.Agregar(asiento.Valor);
                    numero++;
                    inmo.RegistrarAjusteFiscal(ejercicio, diferencia, tipoImpositivo, asiento.Valor.Id);
                    generados++;
                    totalImpuesto = Redondeo.Dos(totalImpuesto + asiento.Valor.TotalDebe);
                }
                else
                {
                    // Sin efecto fiscal (contable == fiscal): se marca el ejercicio para no reevaluarlo.
                    inmo.RegistrarAjusteFiscal(ejercicio, diferencia, tipoImpositivo, Guid.Empty);
                }
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new ResultadoAmortizacionDto(ejercicio, generados, totalDotado, totalImpuesto));
    }

    private Resultado<Asiento> CrearDotacion(Guid empresaId, int ejercicio, ref int numero, DateOnly fecha, Inmovilizado inmo, decimal importe)
    {
        var lineas = new List<LineaAsiento>
        {
            new(inmo.CuentaDotacion, importe, 0m, $"Amortización {inmo.Codigo}"),
            new(inmo.CuentaAmortizacion, 0m, importe, $"Amortización {inmo.Codigo}"),
        };
        var asiento = Asiento.Crear(empresaId, ejercicio, numero, fecha, $"Dotación amortización · {inmo.Descripcion}", "Amortizacion", lineas, _reloj);
        if (asiento.EsCorrecto)
        {
            _asientos.Agregar(asiento.Valor);
            numero++;
        }

        return asiento;
    }

    private static DateOnly FinDeMes(int anio, int mes) => new DateOnly(anio, mes, 1).AddMonths(1).AddDays(-1);
}

/// <summary>Da de baja un inmovilizado (sin contraprestación) y genera su asiento de baja.</summary>
public sealed class DarDeBajaInmovilizado
{
    private readonly IRepositorioInmovilizado _inmovilizados;
    private readonly IRepositorioAsientos _asientos;
    private readonly IUnidadDeTrabajoContabilidad _unidad;
    private readonly IReloj _reloj;

    public DarDeBajaInmovilizado(IRepositorioInmovilizado inmovilizados, IRepositorioAsientos asientos, IUnidadDeTrabajoContabilidad unidad, IReloj reloj)
    {
        _inmovilizados = inmovilizados;
        _asientos = asientos;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<Resultado<AsientoDto>> EjecutarAsync(Guid empresaId, Guid id, BajaInmovilizadoComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var inmo = await _inmovilizados.ObtenerAsync(empresaId, id, ct).ConfigureAwait(false);
        if (inmo is null)
        {
            return Resultado.Fallo<AsientoDto>(Error.NoEncontrado("inmovilizado.no_encontrado", "Inmovilizado no encontrado."));
        }

        var ejercicio = comando.Fecha.Year;
        if (await GuardaEjercicio.EstaCerradoAsync(_asientos, empresaId, ejercicio, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<AsientoDto>(Error.Conflicto("asiento.ejercicio_cerrado", $"El ejercicio {ejercicio} está cerrado; no admite nuevos asientos."));
        }

        var baja = inmo.DarDeBaja(comando.Fecha);
        if (baja.EsFallo)
        {
            return Resultado.Fallo<AsientoDto>(baja.Error);
        }

        var amortAcum = inmo.AmortizacionAcumulada;
        var valorNeto = inmo.ValorNetoContable;
        var lineas = new List<LineaAsiento>();
        if (amortAcum > 0m)
        {
            lineas.Add(new LineaAsiento(inmo.CuentaAmortizacion, amortAcum, 0m, "Baja: amortización acumulada"));
        }

        if (valorNeto > 0m)
        {
            lineas.Add(new LineaAsiento(PlanInmovilizado.PerdidasInmovilizado, valorNeto, 0m, "Pérdida por baja"));
        }

        lineas.Add(new LineaAsiento(inmo.CuentaActivo, 0m, inmo.ValorAdquisicion, "Baja del inmovilizado"));

        var numero = await _asientos.SiguienteNumeroAsync(empresaId, ejercicio, ct).ConfigureAwait(false);
        var asiento = Asiento.Crear(empresaId, ejercicio, numero, comando.Fecha, $"Baja inmovilizado · {inmo.Descripcion}", "BajaInmovilizado", lineas, _reloj);
        if (asiento.EsFallo)
        {
            return Resultado.Fallo<AsientoDto>(asiento.Error);
        }

        _asientos.Agregar(asiento.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(AsientoDto.Desde(asiento.Valor));
    }
}

/// <summary>Enajena (vende) un inmovilizado y genera su asiento con el resultado (beneficio/pérdida).</summary>
public sealed class EnajenarInmovilizado
{
    private readonly IRepositorioInmovilizado _inmovilizados;
    private readonly IRepositorioAsientos _asientos;
    private readonly IUnidadDeTrabajoContabilidad _unidad;
    private readonly IReloj _reloj;

    public EnajenarInmovilizado(IRepositorioInmovilizado inmovilizados, IRepositorioAsientos asientos, IUnidadDeTrabajoContabilidad unidad, IReloj reloj)
    {
        _inmovilizados = inmovilizados;
        _asientos = asientos;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<Resultado<AsientoDto>> EjecutarAsync(Guid empresaId, Guid id, EnajenarInmovilizadoComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var inmo = await _inmovilizados.ObtenerAsync(empresaId, id, ct).ConfigureAwait(false);
        if (inmo is null)
        {
            return Resultado.Fallo<AsientoDto>(Error.NoEncontrado("inmovilizado.no_encontrado", "Inmovilizado no encontrado."));
        }

        var ejercicio = comando.Fecha.Year;
        if (await GuardaEjercicio.EstaCerradoAsync(_asientos, empresaId, ejercicio, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<AsientoDto>(Error.Conflicto("asiento.ejercicio_cerrado", $"El ejercicio {ejercicio} está cerrado; no admite nuevos asientos."));
        }

        var enajena = inmo.Enajenar(comando.Fecha, comando.ValorEnajenacion);
        if (enajena.EsFallo)
        {
            return Resultado.Fallo<AsientoDto>(enajena.Error);
        }

        var precio = Redondeo.Dos(comando.ValorEnajenacion);
        var cuotaIva = Redondeo.Dos(precio * comando.PorcentajeIva / 100m);
        var cobro = Redondeo.Dos(precio + cuotaIva);
        var amortAcum = inmo.AmortizacionAcumulada;
        var valorNeto = inmo.ValorNetoContable;
        var resultado = Redondeo.Dos(precio - valorNeto);
        var cuentaCobro = string.IsNullOrWhiteSpace(comando.CuentaCobro) ? "572" : comando.CuentaCobro.Trim();

        var lineas = new List<LineaAsiento>();
        if (amortAcum > 0m)
        {
            lineas.Add(new LineaAsiento(inmo.CuentaAmortizacion, amortAcum, 0m, "Enajenación: amortización acumulada"));
        }

        lineas.Add(new LineaAsiento(cuentaCobro, cobro, 0m, "Cobro de la venta"));
        lineas.Add(new LineaAsiento(inmo.CuentaActivo, 0m, inmo.ValorAdquisicion, "Baja del inmovilizado enajenado"));
        if (cuotaIva > 0m)
        {
            lineas.Add(new LineaAsiento(PlanBasico.CuentaIvaRepercutido, 0m, cuotaIva, "IVA repercutido"));
        }

        if (resultado > 0m)
        {
            lineas.Add(new LineaAsiento(PlanInmovilizado.BeneficiosInmovilizado, 0m, resultado, "Beneficio en la venta"));
        }
        else if (resultado < 0m)
        {
            lineas.Add(new LineaAsiento(PlanInmovilizado.PerdidasInmovilizado, -resultado, 0m, "Pérdida en la venta"));
        }

        var numero = await _asientos.SiguienteNumeroAsync(empresaId, ejercicio, ct).ConfigureAwait(false);
        var asiento = Asiento.Crear(empresaId, ejercicio, numero, comando.Fecha, $"Enajenación inmovilizado · {inmo.Descripcion}", "Enajenacion", lineas, _reloj);
        if (asiento.EsFallo)
        {
            return Resultado.Fallo<AsientoDto>(asiento.Error);
        }

        _asientos.Agregar(asiento.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(AsientoDto.Desde(asiento.Valor));
    }
}
