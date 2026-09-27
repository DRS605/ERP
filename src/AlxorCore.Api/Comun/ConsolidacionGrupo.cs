using AlxorCore.Contabilidad.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Tesoreria.Aplicacion;

namespace AlxorCore.Api.Comun;

/// <summary>Empresa incluida en la consolidación.</summary>
public sealed record EmpresaConsolidadaDto(Guid Id, string Nombre, bool ConContabilidad);

/// <summary>
/// Cuenta (a 3 dígitos, nivel del PGC común a todas las empresas) en la consolidación: saldo de cada empresa
/// (debe − haber, en el orden de <see cref="ConsolidadoDto.Empresas"/>), su suma, las eliminaciones y el consolidado.
/// </summary>
public sealed record LineaConsolidadoDto(string Cuenta, string Nombre, IReadOnlyList<decimal> PorEmpresa, decimal Agregado, decimal Eliminado, decimal Consolidado);

/// <summary>Un apunte de eliminación intragrupo.</summary>
public sealed record EliminacionDto(string Empresa, string Documento, string Concepto, string Cuenta, decimal Debe, decimal Haber);

/// <summary>Factura intragrupo que no se elimina (no cuadra entre las dos empresas) y por qué.</summary>
public sealed record NoEliminadaDto(string Emisor, string Receptor, string Numero, decimal BaseEmitida, decimal BaseContabilizada, string Situacion);

/// <summary>
/// Consolidación del grupo en un ejercicio: balance de sumas y saldos de todas las empresas, con las operaciones
/// intragrupo eliminadas (ventas contra compras, y los saldos pendientes de cobro contra los de pago).
/// </summary>
public sealed record ConsolidadoDto(int Ejercicio, IReadOnlyList<EmpresaConsolidadaDto> Empresas, IReadOnlyList<LineaConsolidadoDto> Lineas,
    IReadOnlyList<EliminacionDto> Eliminaciones, IReadOnlyList<NoEliminadaDto> NoEliminadas, decimal ResultadoAgregado, decimal ResultadoConsolidado,
    decimal VentasEliminadas, decimal ComprasEliminadas, decimal SaldosEliminados, bool EliminacionesCuadran, IReadOnlyList<string> EmpresasSinAcceso);

/// <summary>
/// Consolidación contable del grupo (agregación y eliminaciones), sobre la contabilidad de cada empresa leída en su
/// propio ámbito:
/// <list type="number">
/// <item>se suman los saldos de todas las empresas por cuenta de 3 dígitos;</item>
/// <item>de cada factura intragrupo que cuadra (emitida por una y contabilizada por la otra por la misma base) se
/// eliminan los apuntes reales de sus asientos: la venta (7xx) en la emisora y la compra o el gasto (6xx) en la
/// receptora;</item>
/// <item>se eliminan también lo que queda pendiente de cobro (430) y de pago (400) entre ellas.</item>
/// </list>
/// Las facturas que no cuadran no se eliminan: se listan para resolverlas (el cuadre intragrupo dice qué falta).
/// </summary>
public sealed class ConsolidacionGrupo
{
    private readonly OperacionesIntragrupo _intragrupo;

    public ConsolidacionGrupo(OperacionesIntragrupo intragrupo) => _intragrupo = intragrupo;

    private static string Tres(string cuenta) => cuenta.Length > 3 ? cuenta[..3] : cuenta;

    public async Task<ConsolidadoDto> ConsolidarAsync(Guid empresaActual, Guid usuarioId, int ejercicio, CancellationToken ct = default)
    {
        IReadOnlyList<AlxorCore.Organizacion.Aplicacion.Modelos.EmpresaGrupoDto> delGrupo;
        HashSet<Guid> accesibles;
        await using (var actual = await _intragrupo.AmbitoAsync(empresaActual, ct).ConfigureAwait(false))
        {
            var grupo = OperacionesIntragrupo.Servicio<IContextoEmpresa>(actual).GrupoId!.Value;
            delGrupo = await OperacionesIntragrupo.Servicio<IConsultaEmpresas>(actual).EmpresasDelGrupoAsync(grupo, ct).ConfigureAwait(false);
            accesibles = (await OperacionesIntragrupo.Servicio<IConsultasOrganizacion>(actual).ListarEmpresasDeUsuarioAsync(usuarioId, ct).ConfigureAwait(false))
                .Select(e => e.Id).ToHashSet();
        }

        var empresas = delGrupo.Where(e => accesibles.Contains(e.Id)).ToList();
        var nombres = new Dictionary<string, string>(StringComparer.Ordinal);
        var saldos = new Dictionary<Guid, Dictionary<string, decimal>>();
        var conContabilidad = new Dictionary<Guid, bool>();
        foreach (var e in empresas)
        {
            await using var ambito = await _intragrupo.AmbitoAsync(e.Id, ct).ConfigureAwait(false);
            var balance = await OperacionesIntragrupo.Servicio<BalanceSumasYSaldos>(ambito).EjecutarAsync(e.Id, ejercicio, ct).ConfigureAwait(false);
            saldos[e.Id] = balance.GroupBy(b => Tres(b.CuentaCodigo), StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.Sum(b => b.SumaDebe - b.SumaHaber), StringComparer.Ordinal);
            conContabilidad[e.Id] = balance.Count > 0;
            foreach (var c in await OperacionesIntragrupo.Servicio<IRepositorioCuentas>(ambito).ListarAsync(e.Id, ct).ConfigureAwait(false))
            {
                if (c.Codigo.Length == 3)
                {
                    nombres.TryAdd(c.Codigo, c.Nombre);
                }
            }
        }

        // Eliminaciones de las facturas intragrupo que cuadran.
        var cuadre = await _intragrupo.CuadreAsync(empresaActual, usuarioId, ejercicio, ct).ConfigureAwait(false);
        var eliminaciones = new List<EliminacionDto>();
        var noEliminadas = new List<NoEliminadaDto>();
        decimal ventas = 0m, compras = 0m, pendientes = 0m;
        foreach (var pareja in cuadre.Parejas)
        {
            foreach (var l in pareja.Facturas)
            {
                if (l.Situacion is "Anulada")
                {
                    continue;
                }

                if (l.Situacion != "Cuadrada" || l.GastoId is not { } gastoId)
                {
                    noEliminadas.Add(new NoEliminadaDto(pareja.Emisor, pareja.Receptor, l.Numero, l.BaseEmitida, l.BaseContabilizada, l.Situacion));
                    continue;
                }

                await using (var emisora = await _intragrupo.AmbitoAsync(pareja.EmisorId, ct).ConfigureAwait(false))
                {
                    var apuntes = await OperacionesIntragrupo.Servicio<IRepositorioAsientos>(emisora).ApuntesDeOrigenesAsync(pareja.EmisorId, [l.FacturaId], ct).ConfigureAwait(false);
                    foreach (var g in apuntes.Where(a => a.CuentaCodigo.StartsWith('7')).GroupBy(a => Tres(a.CuentaCodigo), StringComparer.Ordinal))
                    {
                        var importe = Redondeo.Dos(g.Sum(a => a.Haber - a.Debe));
                        if (importe != 0m)
                        {
                            eliminaciones.Add(new EliminacionDto(pareja.Emisor, l.Numero, $"Venta a {pareja.Receptor}", g.Key, importe, 0m));
                            ventas += importe;
                        }
                    }

                    var saldo = await OperacionesIntragrupo.Servicio<ConsultarSaldo>(emisora).DeFacturaAsync(l.FacturaId, ct).ConfigureAwait(false);
                    if (saldo.EsCorrecto && saldo.Valor.Pendiente != 0m)
                    {
                        eliminaciones.Add(new EliminacionDto(pareja.Emisor, l.Numero, $"Pendiente de cobro a {pareja.Receptor}", "430", 0m, saldo.Valor.Pendiente));
                        pendientes += saldo.Valor.Pendiente;
                    }
                }

                await using (var receptora = await _intragrupo.AmbitoAsync(pareja.ReceptorId, ct).ConfigureAwait(false))
                {
                    var apuntes = await OperacionesIntragrupo.Servicio<IRepositorioAsientos>(receptora).ApuntesDeOrigenesAsync(pareja.ReceptorId, [gastoId], ct).ConfigureAwait(false);
                    foreach (var g in apuntes.Where(a => a.CuentaCodigo.StartsWith('6')).GroupBy(a => Tres(a.CuentaCodigo), StringComparer.Ordinal))
                    {
                        var importe = Redondeo.Dos(g.Sum(a => a.Debe - a.Haber));
                        if (importe != 0m)
                        {
                            eliminaciones.Add(new EliminacionDto(pareja.Receptor, l.Numero, $"Compra a {pareja.Emisor}", g.Key, 0m, importe));
                            compras += importe;
                        }
                    }

                    var saldo = await OperacionesIntragrupo.Servicio<ConsultarSaldo>(receptora).DeGastoAsync(gastoId, ct).ConfigureAwait(false);
                    if (saldo.EsCorrecto && saldo.Valor.Pendiente != 0m)
                    {
                        eliminaciones.Add(new EliminacionDto(pareja.Receptor, l.Numero, $"Pendiente de pago a {pareja.Emisor}", "400", saldo.Valor.Pendiente, 0m));
                    }
                }
            }
        }

        var eliminado = eliminaciones.GroupBy(x => x.Cuenta, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Sum(x => x.Debe - x.Haber), StringComparer.Ordinal);
        var cuentas = saldos.Values.SelectMany(s => s.Keys).Concat(eliminado.Keys).Distinct(StringComparer.Ordinal).OrderBy(c => c, StringComparer.Ordinal).ToList();
        var lineas = cuentas.Select(c =>
        {
            var por = empresas.Select(e => Redondeo.Dos(saldos[e.Id].GetValueOrDefault(c))).ToList();
            var agregado = por.Sum();
            var elim = Redondeo.Dos(eliminado.GetValueOrDefault(c));
            return new LineaConsolidadoDto(c, nombres.GetValueOrDefault(c, "—"), por, agregado, elim, Redondeo.Dos(agregado + elim));
        }).Where(x => x.Agregado != 0m || x.Eliminado != 0m).ToList();

        // Resultado = ingresos (7) − gastos (6): el saldo acreedor neto de los grupos 6 y 7.
        static bool DeResultados(string cuenta) => cuenta[0] is '6' or '7';
        var agregadoResultado = -lineas.Where(x => DeResultados(x.Cuenta)).Sum(x => x.Agregado);
        var consolidadoResultado = -lineas.Where(x => DeResultados(x.Cuenta)).Sum(x => x.Consolidado);
        return new ConsolidadoDto(ejercicio, empresas.Select(e => new EmpresaConsolidadaDto(e.Id, e.RazonSocial, conContabilidad[e.Id])).ToList(), lineas,
            eliminaciones, noEliminadas, Redondeo.Dos(agregadoResultado), Redondeo.Dos(consolidadoResultado), Redondeo.Dos(ventas), Redondeo.Dos(compras),
            Redondeo.Dos(pendientes), eliminaciones.Sum(x => x.Debe) == eliminaciones.Sum(x => x.Haber), cuadre.EmpresasSinAcceso);
    }
}
