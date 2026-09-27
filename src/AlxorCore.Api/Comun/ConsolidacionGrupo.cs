using AlxorCore.Contabilidad.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Tesoreria.Aplicacion;

namespace AlxorCore.Api.Comun;

/// <summary>Empresa incluida en la consolidación.</summary>
public sealed record EmpresaConsolidadaDto(Guid Id, string Nombre, bool ConContabilidad, decimal Porcentaje = 100m, string Metodo = "Global");

/// <summary>Saldos de una pareja de cuentas recíprocas en la consolidación y su diferencia (debería ser 0).</summary>
public sealed record CorrespondenciaConsolidadaDto(string Descripcion, string EmpresaA, string CuentaA, decimal SaldoA, string EmpresaB, string CuentaB, decimal SaldoB, decimal Diferencia);

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
/// Eliminación inversión-patrimonio neto de una empresa participada: el coste de la participación en la titular contra
/// el patrimonio neto de la participada. La diferencia con su parte del patrimonio al adquirirla es el fondo de comercio
/// (o, si es negativa, reservas); su parte de lo generado después, reservas en sociedades consolidadas; y la parte de
/// otros socios, patrimonio de socios externos.
/// </summary>
public sealed record InversionEliminadaDto(string Participada, string Titular, decimal Porcentaje, string CuentaInversion, decimal Coste,
    decimal PatrimonioAdquisicion, decimal PatrimonioActual, decimal FondoComercio, decimal Reservas, decimal SociosExternos);

/// <summary>
/// Consolidación del grupo en un ejercicio: balance de sumas y saldos de todas las empresas, con las operaciones
/// intragrupo eliminadas (ventas contra compras, y los saldos pendientes de cobro contra los de pago).
/// </summary>
public sealed record ConsolidadoDto(int Ejercicio, IReadOnlyList<EmpresaConsolidadaDto> Empresas, IReadOnlyList<LineaConsolidadoDto> Lineas,
    IReadOnlyList<EliminacionDto> Eliminaciones, IReadOnlyList<NoEliminadaDto> NoEliminadas, decimal ResultadoAgregado, decimal ResultadoConsolidado,
    decimal VentasEliminadas, decimal ComprasEliminadas, decimal SaldosEliminados, bool EliminacionesCuadran, IReadOnlyList<string> EmpresasSinAcceso,
    IReadOnlyList<CorrespondenciaConsolidadaDto>? Correspondencias = null, decimal ResultadoSociosExternos = 0m, decimal ResultadoDominante = 0m,
    IReadOnlyList<string>? EmpresasExcluidas = null, IReadOnlyList<InversionEliminadaDto>? Inversiones = null, decimal FondoComercio = 0m,
    decimal ReservasConsolidadas = 0m, decimal PatrimonioSociosExternos = 0m, IReadOnlyList<string>? Avisos = null);

/// <summary>
/// Consolidación contable del grupo (agregación y eliminaciones), sobre la contabilidad de cada empresa leída en su
/// propio ámbito:
/// <list type="number">
/// <item>se suman los saldos de todas las empresas por cuenta de 3 dígitos;</item>
/// <item>de cada factura intragrupo que cuadra (emitida por una y contabilizada por la otra por la misma base) se
/// eliminan los apuntes reales de sus asientos: la venta (7xx) en la emisora y la compra o el gasto (6xx) en la
/// receptora;</item>
/// <item>se eliminan también lo que queda pendiente de cobro (430) y de pago (400) entre ellas;</item>
/// <item>y la inversión de la titular en cada participada contra el patrimonio neto de esta (grupos 10 a 13), con el
/// fondo de comercio, las reservas en sociedades consolidadas y el patrimonio de socios externos.</item>
/// </list>
/// Las facturas que no cuadran no se eliminan: se listan para resolverlas (el cuadre intragrupo dice qué falta).
/// </summary>
public sealed class ConsolidacionGrupo
{
    private readonly OperacionesIntragrupo _intragrupo;

    public ConsolidacionGrupo(OperacionesIntragrupo intragrupo) => _intragrupo = intragrupo;

    private static string Tres(string cuenta) => cuenta.Length > 3 ? cuenta[..3] : cuenta;

    /// <summary>Línea de reservas en sociedades consolidadas (no es una cuenta del PGC de las empresas).</summary>
    public const string CuentaReservasConsolidadas = "RSC";

    /// <summary>Línea de patrimonio neto de socios externos.</summary>
    public const string CuentaSociosExternos = "SOE";

    /// <summary>Fondo de comercio de consolidación.</summary>
    public const string CuentaFondoComercio = "204";

    private static bool DePatrimonioNeto(string cuenta) => cuenta.Length >= 2 && cuenta[0] == '1' && cuenta[1] is '0' or '1' or '2' or '3';

    public async Task<ConsolidadoDto> ConsolidarAsync(Guid empresaActual, Guid usuarioId, int ejercicio, CancellationToken ct = default)
    {
        IReadOnlyList<AlxorCore.Organizacion.Aplicacion.Modelos.EmpresaGrupoDto> delGrupo;
        HashSet<Guid> accesibles;
        Dictionary<Guid, AlxorCore.Organizacion.Aplicacion.CasosDeUso.PerimetroDto> perimetro;
        IReadOnlyList<AlxorCore.Organizacion.Aplicacion.CasosDeUso.CorrespondenciaDto> parejas;
        await using (var actual = await _intragrupo.AmbitoAsync(empresaActual, ct).ConfigureAwait(false))
        {
            var grupo = OperacionesIntragrupo.Servicio<IContextoEmpresa>(actual).GrupoId!.Value;
            delGrupo = await OperacionesIntragrupo.Servicio<IConsultaEmpresas>(actual).EmpresasDelGrupoAsync(grupo, ct).ConfigureAwait(false);
            accesibles = (await OperacionesIntragrupo.Servicio<IConsultasOrganizacion>(actual).ListarEmpresasDeUsuarioAsync(usuarioId, ct).ConfigureAwait(false))
                .Select(e => e.Id).ToHashSet();
            var config = OperacionesIntragrupo.Servicio<AlxorCore.Organizacion.Aplicacion.CasosDeUso.ConfiguracionConsolidacion>(actual);
            perimetro = (await config.PerimetroAsync(grupo, ct).ConfigureAwait(false)).ToDictionary(p => p.EmpresaId);
            parejas = await config.CorrespondenciasAsync(grupo, ct).ConfigureAwait(false);
        }

        // Perímetro: las excluidas no entran; las proporcionales, en su porcentaje.
        static bool Excluida(AlxorCore.Organizacion.Aplicacion.CasosDeUso.PerimetroDto? p) => p?.Metodo == AlxorCore.Organizacion.Dominio.MetodoConsolidacion.Excluida;
        var visibles = delGrupo.Where(e => accesibles.Contains(e.Id)).ToList();
        var empresas = visibles.Where(e => !Excluida(perimetro.GetValueOrDefault(e.Id))).ToList();
        var dentro = empresas.Select(e => e.Id).ToHashSet();
        var factor = empresas.ToDictionary(e => e.Id, e => perimetro.GetValueOrDefault(e.Id) is { Metodo: AlxorCore.Organizacion.Dominio.MetodoConsolidacion.Proporcional } p ? p.Porcentaje / 100m : 1m);
        var nombreEmpresa = delGrupo.ToDictionary(e => e.Id, e => e.RazonSocial);
        var nombres = new Dictionary<string, string>(StringComparer.Ordinal);
        var saldos = new Dictionary<Guid, Dictionary<string, decimal>>();
        var hojas = new Dictionary<Guid, IReadOnlyList<SaldoCuentaDto>>();
        var conContabilidad = new Dictionary<Guid, bool>();
        foreach (var e in empresas)
        {
            await using var ambito = await _intragrupo.AmbitoAsync(e.Id, ct).ConfigureAwait(false);
            var balance = await OperacionesIntragrupo.Servicio<BalanceSumasYSaldos>(ambito).EjecutarAsync(e.Id, ejercicio, ct).ConfigureAwait(false);
            hojas[e.Id] = balance;
            saldos[e.Id] = balance.GroupBy(b => Tres(b.CuentaCodigo), StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.Sum(b => b.SumaDebe - b.SumaHaber) * factor[e.Id], StringComparer.Ordinal);
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

                if (!dentro.Contains(pareja.EmisorId) || !dentro.Contains(pareja.ReceptorId))
                {
                    continue;
                }

                // En integración proporcional, la operación se elimina en el menor de los dos porcentajes.
                var f = Math.Min(factor[pareja.EmisorId], factor[pareja.ReceptorId]);
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
                        var importe = Redondeo.Dos(g.Sum(a => a.Haber - a.Debe) * f);
                        if (importe != 0m)
                        {
                            eliminaciones.Add(new EliminacionDto(pareja.Emisor, l.Numero, $"Venta a {pareja.Receptor}", g.Key, importe, 0m));
                            ventas += importe;
                        }
                    }

                    var saldo = await OperacionesIntragrupo.Servicio<ConsultarSaldo>(emisora).DeFacturaAsync(l.FacturaId, ct).ConfigureAwait(false);
                    if (saldo.EsCorrecto && saldo.Valor.Pendiente != 0m)
                    {
                        var pendiente = Redondeo.Dos(saldo.Valor.Pendiente * f);
                        eliminaciones.Add(new EliminacionDto(pareja.Emisor, l.Numero, $"Pendiente de cobro a {pareja.Receptor}", "430", 0m, pendiente));
                        pendientes += pendiente;
                    }
                }

                await using (var receptora = await _intragrupo.AmbitoAsync(pareja.ReceptorId, ct).ConfigureAwait(false))
                {
                    var apuntes = await OperacionesIntragrupo.Servicio<IRepositorioAsientos>(receptora).ApuntesDeOrigenesAsync(pareja.ReceptorId, [gastoId], ct).ConfigureAwait(false);
                    foreach (var g in apuntes.Where(a => a.CuentaCodigo.StartsWith('6')).GroupBy(a => Tres(a.CuentaCodigo), StringComparer.Ordinal))
                    {
                        var importe = Redondeo.Dos(g.Sum(a => a.Debe - a.Haber) * f);
                        if (importe != 0m)
                        {
                            eliminaciones.Add(new EliminacionDto(pareja.Receptor, l.Numero, $"Compra a {pareja.Emisor}", g.Key, 0m, importe));
                            compras += importe;
                        }
                    }

                    var saldo = await OperacionesIntragrupo.Servicio<ConsultarSaldo>(receptora).DeGastoAsync(gastoId, ct).ConfigureAwait(false);
                    if (saldo.EsCorrecto && saldo.Valor.Pendiente != 0m)
                    {
                        eliminaciones.Add(new EliminacionDto(pareja.Receptor, l.Numero, $"Pendiente de pago a {pareja.Emisor}", "400", Redondeo.Dos(saldo.Valor.Pendiente * f), 0m));
                    }
                }
            }
        }

        // Saldos recíprocos de las correspondencias de cuentas: se eliminan los dos; su suma es el descuadre.
        var correspondencias = new List<CorrespondenciaConsolidadaDto>();
        foreach (var c in parejas.Where(c => dentro.Contains(c.EmpresaAId) && dentro.Contains(c.EmpresaBId)))
        {
            var f = Math.Min(factor[c.EmpresaAId], factor[c.EmpresaBId]);
            var sa = Redondeo.Dos(hojas[c.EmpresaAId].Where(h => h.CuentaCodigo.StartsWith(c.CuentaA, StringComparison.Ordinal)).Sum(h => h.SumaDebe - h.SumaHaber) * f);
            var sb = Redondeo.Dos(hojas[c.EmpresaBId].Where(h => h.CuentaCodigo.StartsWith(c.CuentaB, StringComparison.Ordinal)).Sum(h => h.SumaDebe - h.SumaHaber) * f);
            foreach (var (empresa, cuenta, saldo) in new[] { (c.EmpresaAId, c.CuentaA, sa), (c.EmpresaBId, c.CuentaB, sb) }.Where(x => x.Item3 != 0m))
            {
                eliminaciones.Add(new EliminacionDto(nombreEmpresa[empresa], c.Descripcion, "Saldo recíproco", Tres(cuenta), saldo < 0m ? -saldo : 0m, saldo > 0m ? saldo : 0m));
            }

            correspondencias.Add(new CorrespondenciaConsolidadaDto(c.Descripcion, nombreEmpresa[c.EmpresaAId], c.CuentaA, sa, nombreEmpresa[c.EmpresaBId], c.CuentaB, sb, Redondeo.Dos(sa + sb)));
        }

        // Inversión-patrimonio neto de cada participada con titular en el perímetro.
        var inversiones = new List<InversionEliminadaDto>();
        var avisos = new List<string>();
        foreach (var e in empresas)
        {
            if (perimetro.GetValueOrDefault(e.Id) is not { TitularId: { } titular } p)
            {
                continue;
            }

            if (!dentro.Contains(titular))
            {
                avisos.Add($"{e.RazonSocial}: su titular ({nombreEmpresa.GetValueOrDefault(titular, "—")}) no está en la consolidación; no se elimina la inversión.");
                continue;
            }

            var pct = p.Porcentaje / 100m;
            var cuentaInv = p.CuentaInversion ?? AlxorCore.Organizacion.Dominio.PerimetroConsolidacion.CuentaInversionPorDefecto;
            var coste = p.CosteInversion
                ?? hojas[titular].Where(h => h.CuentaCodigo.StartsWith(cuentaInv, StringComparison.Ordinal)).Sum(h => h.SumaDebe - h.SumaHaber);
            var costeEliminado = Redondeo.Dos(coste * factor[titular]);
            var pnCuentas = hojas[e.Id].Where(h => DePatrimonioNeto(h.CuentaCodigo)).GroupBy(h => Tres(h.CuentaCodigo), StringComparer.Ordinal)
                .Select(g => (Cuenta: g.Key, Acreedor: g.Sum(h => h.SumaHaber - h.SumaDebe))).Where(x => x.Acreedor != 0m).ToList();
            var pnActual = pnCuentas.Sum(x => x.Acreedor);
            var pnAdquisicion = p.PatrimonioAdquisicion ?? pnActual;
            if (costeEliminado == 0m && pnActual == 0m)
            {
                avisos.Add($"{e.RazonSocial}: ni la inversión ({cuentaInv}) ni su patrimonio neto tienen saldo; no hay nada que eliminar.");
                continue;
            }

            var pnEliminado = 0m;
            foreach (var (cuenta, acreedor) in pnCuentas)
            {
                var importe = Redondeo.Dos(acreedor * factor[e.Id]);
                pnEliminado += importe;
                eliminaciones.Add(new EliminacionDto(e.RazonSocial, "Inversión-patrimonio neto", "Patrimonio neto de la participada", cuenta,
                    importe > 0m ? importe : 0m, importe < 0m ? -importe : 0m));
            }

            eliminaciones.Add(new EliminacionDto(nombreEmpresa[titular], "Inversión-patrimonio neto", $"Participación en {e.RazonSocial}", Tres(cuentaInv),
                costeEliminado < 0m ? -costeEliminado : 0m, costeEliminado > 0m ? costeEliminado : 0m));
            var diferencia = Redondeo.Dos(costeEliminado - (pct * pnAdquisicion));
            var fondo = Math.Max(diferencia, 0m);
            var reservas = Redondeo.Dos((pct * (pnActual - pnAdquisicion)) + Math.Max(-diferencia, 0m));
            // Lo que falta para cuadrar es la parte de otros socios: (factor − participación) × patrimonio neto.
            var externos = Redondeo.Dos(pnEliminado + fondo - costeEliminado - reservas);
            if (fondo != 0m)
            {
                eliminaciones.Add(new EliminacionDto(nombreEmpresa[titular], "Inversión-patrimonio neto", $"Fondo de comercio de {e.RazonSocial}", CuentaFondoComercio, fondo, 0m));
            }

            foreach (var (cuenta, concepto, importe) in new[] { (CuentaReservasConsolidadas, "Reservas en sociedades consolidadas", reservas), (CuentaSociosExternos, "Socios externos", externos) })
            {
                if (importe != 0m)
                {
                    eliminaciones.Add(new EliminacionDto(e.RazonSocial, "Inversión-patrimonio neto", concepto, cuenta, importe < 0m ? -importe : 0m, importe > 0m ? importe : 0m));
                }
            }

            inversiones.Add(new InversionEliminadaDto(e.RazonSocial, nombreEmpresa[titular], p.Porcentaje, cuentaInv, Redondeo.Dos(coste), Redondeo.Dos(pnAdquisicion),
                Redondeo.Dos(pnActual), fondo, reservas, externos));
        }

        nombres.TryAdd(CuentaFondoComercio, "Fondo de comercio de consolidación");
        nombres[CuentaReservasConsolidadas] = "Reservas en sociedades consolidadas";
        nombres[CuentaSociosExternos] = "Socios externos (patrimonio neto)";

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

        // Socios externos: en integración global con participación menor del 100 %, su parte del resultado de la empresa.
        var socios = empresas.Where(e => perimetro.GetValueOrDefault(e.Id) is { Metodo: AlxorCore.Organizacion.Dominio.MetodoConsolidacion.Global, Porcentaje: < 100m })
            .Sum(e => -saldos[e.Id].Where(x => DeResultados(x.Key)).Sum(x => x.Value) * (1m - (perimetro[e.Id].Porcentaje / 100m)));
        var empresasDto = empresas.Select(e => new EmpresaConsolidadaDto(e.Id, e.RazonSocial, conContabilidad[e.Id],
            perimetro.GetValueOrDefault(e.Id)?.Porcentaje ?? 100m, (perimetro.GetValueOrDefault(e.Id)?.Metodo ?? AlxorCore.Organizacion.Dominio.MetodoConsolidacion.Global).ToString())).ToList();
        return new ConsolidadoDto(ejercicio, empresasDto, lineas,
            eliminaciones, noEliminadas, Redondeo.Dos(agregadoResultado), Redondeo.Dos(consolidadoResultado), Redondeo.Dos(ventas), Redondeo.Dos(compras),
            Redondeo.Dos(pendientes), eliminaciones.Sum(x => x.Debe) == eliminaciones.Sum(x => x.Haber), cuadre.EmpresasSinAcceso,
            correspondencias, Redondeo.Dos(socios), Redondeo.Dos(consolidadoResultado - socios),
            visibles.Where(e => !dentro.Contains(e.Id)).Select(e => e.RazonSocial).ToList(), inversiones, inversiones.Sum(i => i.FondoComercio),
            inversiones.Sum(i => i.Reservas), inversiones.Sum(i => i.SociosExternos), avisos);
    }
}
