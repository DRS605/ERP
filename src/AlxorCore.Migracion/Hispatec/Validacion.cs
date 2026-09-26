using System.Globalization;
using AlxorCore.Nucleo.Comun;

namespace AlxorCore.Migracion.Hispatec;

public enum NivelIncidencia
{
    /// <summary>La fila no se carga (o, si es del paquete entero, no se puede cargar nada).</summary>
    Error = 1,

    /// <summary>Se carga, pero conviene revisarlo.</summary>
    Aviso = 2,
}

public sealed record Incidencia(string Archivo, int? Fila, string Codigo, NivelIncidencia Nivel, string Mensaje);

public sealed record ResumenArchivo(string Archivo, int Filas, int Validas);

/// <summary>Cuadres del paquete: el balance de apertura y la cartera frente a sus cuentas.</summary>
public sealed record CuadresMigracion(
    decimal SaldosDebe, decimal SaldosHaber, decimal CarteraCobros, decimal SaldoClientes, decimal CarteraPagos, decimal SaldoProveedores,
    int CuentasDescuadradasConAcumuladores);

public sealed record InformeMigracion(
    string? Empresa, DateOnly FechaCorte, IReadOnlyList<ResumenArchivo> Archivos, IReadOnlyList<Incidencia> Incidencias, int Errores, int Avisos,
    bool PuedeCargar, CuadresMigracion Cuadres);

// ------------------------------------------------------------------ Datos del paquete, ya validados
public sealed record TerceroH(string IdSujeto, string? Nif, string Nombre, string? Calle, string? CodigoPostal, string? Poblacion, string? Provincia,
    string? Pais, string? Email, string? Iban);

public sealed record ClienteH(string Id, string Codigo, TerceroH Tercero, string? Subcuenta);

public sealed record ProveedorH(string Id, string Codigo, TerceroH Tercero, string? Subcuenta, bool Agricultor, bool AutorizaAutofactura, decimal? Retencion);

public sealed record FamiliaH(string Id, string? Codigo, string Nombre, string? IdSuperior);

public sealed record ArticuloH(string Id, string Codigo, string Nombre, string? IdFamilia, string Unidad, decimal Iva, bool EsServicio, bool ControlaStock,
    decimal PrecioVenta, decimal PrecioCompra);

public sealed record SaldoH(string Cuenta, decimal Debe, decimal Haber);

public sealed record EfectoH(string Id, bool EsCobro, string IdTercero, string Documento, DateOnly? FechaDocumento, DateOnly Vencimiento, decimal Importe);

public sealed record CampanaH(string Id, string Codigo, string Nombre, DateOnly Desde, DateOnly Hasta);

public sealed record ParcelaH(string Id, string Codigo, string Nombre, string IdProveedor, string? Sigpac, decimal? SuperficieHa, string? IdArticulo, string? Variedad);

/// <summary>Lo que se puede cargar del paquete: solo las filas válidas.</summary>
public sealed record DatosHispatec(
    DateOnly FechaCorte, IReadOnlyList<ClienteH> Clientes, IReadOnlyList<ProveedorH> Proveedores, IReadOnlyList<FamiliaH> Familias,
    IReadOnlyList<ArticuloH> Articulos, IReadOnlyList<(string Codigo, string Nombre)> Cuentas, IReadOnlyList<SaldoH> Saldos,
    IReadOnlyList<EfectoH> Cartera, IReadOnlyList<CampanaH> Campanas, IReadOnlyList<ParcelaH> Parcelas);

/// <summary>
/// Valida el paquete antes de cargar nada. Aplica las reglas que en Hispatec no garantiza la base de datos
/// (ver <c>docs/hispatec/MAPA.md</c>): referencias huérfanas (la cartera de <c>DocumentosCobro</c> no tiene
/// ninguna clave foránea), saldos mantenidos por triggers que no cuadran con los apuntes, terceros
/// duplicados, NIF inválidos, y cartera que no cuadra con la contabilidad.
/// </summary>
public static class ValidadorHispatec
{
    private const int MaximoDetalle = 50;

    public static (InformeMigracion Informe, DatosHispatec Datos) Validar(Paquete paquete)
    {
        ArgumentNullException.ThrowIfNull(paquete);
        var inc = new List<Incidencia>();
        var resumen = new List<ResumenArchivo>();
        foreach (var e in paquete.ErroresLectura)
        {
            inc.Add(new Incidencia("paquete", null, "paquete.estructura", NivelIncidencia.Error, e));
        }

        void Error(string archivo, int? fila, string codigo, string mensaje) => inc.Add(new(archivo, fila, codigo, NivelIncidencia.Error, mensaje));
        void Aviso(string archivo, int? fila, string codigo, string mensaje) => inc.Add(new(archivo, fila, codigo, NivelIncidencia.Aviso, mensaje));

        // ---------------------------------------------------------- Terceros (el «sujeto único» de Hispatec)
        var terceros = new Dictionary<string, TerceroH>(StringComparer.Ordinal);
        var tTerceros = paquete.Tabla("terceros.csv");
        foreach (var f in tTerceros.Filas)
        {
            var id = f.Texto("id_sujeto");
            var nombre = f.Texto("nombre");
            if (id is null || nombre is null)
            {
                Error(tTerceros.Nombre, f.Numero, "tercero.incompleto", "Falta el id del sujeto o el nombre.");
                continue;
            }

            if (!terceros.TryAdd(id, new TerceroH(id, f.Texto("nif")?.ToUpperInvariant().Replace(" ", string.Empty, StringComparison.Ordinal), Recortar(nombre, 200)!,
                    f.Texto("calle"), f.Texto("codigo_postal"), f.Texto("poblacion"), f.Texto("provincia"), f.Texto("pais")?.ToUpperInvariant(), f.Texto("email"),
                    f.Texto("iban")?.Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant())))
            {
                Error(tTerceros.Nombre, f.Numero, "tercero.duplicado", $"El sujeto {id} aparece dos veces.");
                continue;
            }

            var t = terceros[id];
            if (t.Nif is null)
            {
                Aviso(tTerceros.Nombre, f.Numero, "tercero.sin_nif", $"{t.Nombre}: sin NIF (no podrá facturarse con factura completa hasta completarlo).");
            }
            else if ((t.Pais is null or "ES" or "ESP") && Nif.Crear(t.Nif).EsFallo)
            {
                Aviso(tTerceros.Nombre, f.Numero, "tercero.nif_invalido", $"{t.Nombre}: el NIF {t.Nif} no es válido; se carga tal cual para corregirlo después.");
            }
        }

        resumen.Add(new ResumenArchivo(tTerceros.Nombre, tTerceros.Filas.Count, terceros.Count));
        foreach (var g in terceros.Values.Where(t => t.Nif is not null).GroupBy(t => t.Nif!).Where(g => g.Count() > 1).Take(MaximoDetalle))
        {
            Aviso(tTerceros.Nombre, null, "tercero.nif_repetido",
                $"El NIF {g.Key} lo tienen {g.Count()} sujetos ({string.Join(", ", g.Select(t => t.IdSujeto))}): en ALXOR quedarán como un solo cliente o proveedor.");
        }

        // ---------------------------------------------------------- Clientes y proveedores
        var clientes = new List<ClienteH>();
        var tClientes = paquete.Tabla("clientes.csv");
        foreach (var f in tClientes.Filas)
        {
            var (id, codigo, sujeto) = (f.Texto("id"), f.Texto("codigo"), f.Texto("id_sujeto"));
            if (id is null || codigo is null)
            {
                Error(tClientes.Nombre, f.Numero, "cliente.incompleto", "Falta el id o el código del cliente.");
            }
            else if (sujeto is null || !terceros.TryGetValue(sujeto, out var t))
            {
                Error(tClientes.Nombre, f.Numero, "cliente.huerfano", $"El cliente {codigo} apunta al sujeto {sujeto}, que no existe.");
            }
            else
            {
                clientes.Add(new ClienteH(id, codigo, t, Subcuenta(f.Texto("subcuenta"), tClientes.Nombre, f.Numero, inc)));
            }
        }

        resumen.Add(new ResumenArchivo(tClientes.Nombre, tClientes.Filas.Count, clientes.Count));

        var proveedores = new List<ProveedorH>();
        var tProv = paquete.Tabla("proveedores.csv");
        foreach (var f in tProv.Filas)
        {
            var (id, codigo, sujeto) = (f.Texto("id"), f.Texto("codigo"), f.Texto("id_sujeto"));
            if (id is null || codigo is null)
            {
                Error(tProv.Nombre, f.Numero, "proveedor.incompleto", "Falta el id o el código del proveedor.");
                continue;
            }

            if (sujeto is null || !terceros.TryGetValue(sujeto, out var t))
            {
                Error(tProv.Nombre, f.Numero, "proveedor.huerfano", $"El proveedor {codigo} apunta al sujeto {sujeto}, que no existe.");
                continue;
            }

            var retencion = Valor.Numero(f.Texto("retencion"));
            if (retencion is < 0m or > 50m)
            {
                Aviso(tProv.Nombre, f.Numero, "proveedor.retencion", $"{codigo}: retención {retencion} fuera de rango; se usa la de por defecto.");
                retencion = null;
            }

            proveedores.Add(new ProveedorH(id, codigo, t, Subcuenta(f.Texto("subcuenta"), tProv.Nombre, f.Numero, inc), Valor.Booleano(f.Texto("agricultor")),
                Valor.Booleano(f.Texto("autoriza_autofactura")), retencion));
        }

        resumen.Add(new ResumenArchivo(tProv.Nombre, tProv.Filas.Count, proveedores.Count));

        // ---------------------------------------------------------- Familias y artículos
        var familias = new List<FamiliaH>();
        var tFam = paquete.Tabla("familias.csv");
        foreach (var f in tFam.Filas)
        {
            var (id, nombre) = (f.Texto("id"), f.Texto("nombre"));
            if (id is null || nombre is null)
            {
                Error(tFam.Nombre, f.Numero, "familia.incompleta", "Falta el id o el nombre de la familia.");
                continue;
            }

            familias.Add(new FamiliaH(id, f.Texto("codigo"), nombre, f.Texto("id_superior")));
        }

        var idsFamilia = familias.Select(x => x.Id).ToHashSet(StringComparer.Ordinal);
        foreach (var fam in familias.Where(x => x.IdSuperior is not null && !idsFamilia.Contains(x.IdSuperior)).ToList())
        {
            Aviso(tFam.Nombre, null, "familia.superior_huerfana", $"La familia {fam.Nombre} cuelga de {fam.IdSuperior}, que no existe: queda en la raíz.");
            familias[familias.IndexOf(fam)] = fam with { IdSuperior = null };
        }

        resumen.Add(new ResumenArchivo(tFam.Nombre, tFam.Filas.Count, familias.Count));

        var articulos = new List<ArticuloH>();
        var tArt = paquete.Tabla("articulos.csv");
        foreach (var f in tArt.Filas)
        {
            var (id, codigo, nombre) = (f.Texto("id"), f.Texto("codigo"), f.Texto("nombre"));
            if (id is null || codigo is null || nombre is null)
            {
                Error(tArt.Nombre, f.Numero, "articulo.incompleto", "Falta el id, el código o el nombre del artículo.");
                continue;
            }

            var familia = f.Texto("id_familia");
            if (familia is not null && !idsFamilia.Contains(familia))
            {
                Aviso(tArt.Nombre, f.Numero, "articulo.familia_huerfana", $"{codigo}: la familia {familia} no existe; queda sin familia.");
                familia = null;
            }

            var iva = Valor.Numero(f.Texto("iva")) ?? 21m;
            if (iva is not (0m or 4m or 10m or 21m or 3m or 7m or 9.5m or 15m or 20m))
            {
                Aviso(tArt.Nombre, f.Numero, "articulo.iva", $"{codigo}: tipo de IVA {iva.ToString(CultureInfo.InvariantCulture)} desconocido; se usa el general.");
                iva = 21m;
            }

            var precioVenta = Valor.Numero(f.Texto("precio_venta")) ?? 0m;
            var precioCompra = Valor.Numero(f.Texto("precio_compra")) ?? 0m;
            if (precioVenta < 0m || precioCompra < 0m)
            {
                Error(tArt.Nombre, f.Numero, "articulo.precio", $"{codigo}: precio negativo.");
                continue;
            }

            articulos.Add(new ArticuloH(id, codigo, Recortar(nombre, 200)!, familia, (f.Texto("unidad") ?? "ud").ToLowerInvariant(), iva,
                string.Equals(f.Texto("tipo"), "servicio", StringComparison.OrdinalIgnoreCase), Valor.Booleano(f.Texto("controla_stock")), precioVenta, precioCompra));
        }

        foreach (var g in articulos.GroupBy(a => a.Codigo, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1).Take(MaximoDetalle))
        {
            Aviso(tArt.Nombre, null, "articulo.codigo_repetido", $"El código {g.Key} se repite {g.Count()} veces: se carga el primero.");
        }

        articulos = articulos.GroupBy(a => a.Codigo, StringComparer.OrdinalIgnoreCase).Select(g => g.First()).ToList();
        resumen.Add(new ResumenArchivo(tArt.Nombre, tArt.Filas.Count, articulos.Count));

        // ---------------------------------------------------------- Plan de cuentas, saldos y acumuladores
        var cuentas = new List<(string, string)>();
        var tCuentas = paquete.Tabla("cuentas.csv");
        foreach (var f in tCuentas.Filas)
        {
            var (codigo, nombre) = (f.Texto("codigo"), f.Texto("nombre"));
            if (codigo is null || !codigo.All(char.IsAsciiDigit) || nombre is null)
            {
                Error(tCuentas.Nombre, f.Numero, "cuenta.invalida", $"Cuenta «{codigo}» sin nombre o con código no numérico.");
                continue;
            }

            cuentas.Add((codigo, Recortar(nombre, 150)!));
        }

        resumen.Add(new ResumenArchivo(tCuentas.Nombre, tCuentas.Filas.Count, cuentas.Count));
        var codigosCuenta = cuentas.Select(c => c.Item1).ToHashSet(StringComparer.Ordinal);

        List<SaldoH> LeerSaldos(TablaCsv t)
        {
            var lista = new List<SaldoH>();
            foreach (var f in t.Filas)
            {
                var (cuenta, debe, haber) = (f.Texto("cuenta"), Valor.Numero(f.Texto("debe")), Valor.Numero(f.Texto("haber")));
                if (cuenta is null || debe is null || haber is null || debe < 0m || haber < 0m)
                {
                    Error(t.Nombre, f.Numero, "saldo.invalido", $"Saldo de la cuenta «{cuenta}» sin importes válidos.");
                    continue;
                }

                lista.Add(new SaldoH(cuenta, Redondeo.Dos(debe.Value), Redondeo.Dos(haber.Value)));
            }

            return lista.GroupBy(s => s.Cuenta).Select(g => new SaldoH(g.Key, g.Sum(s => s.Debe), g.Sum(s => s.Haber))).ToList();
        }

        var tSaldos = paquete.Tabla("saldos.csv");
        var saldos = LeerSaldos(tSaldos);
        foreach (var s in saldos.Where(s => codigosCuenta.Count > 0 && !codigosCuenta.Contains(s.Cuenta)).Take(MaximoDetalle))
        {
            Aviso(tSaldos.Nombre, null, "saldo.cuenta_desconocida", $"La cuenta {s.Cuenta} tiene saldo pero no está en el plan de cuentas: se creará.");
        }

        var (debeTotal, haberTotal) = (saldos.Sum(s => s.Debe), saldos.Sum(s => s.Haber));
        if (debeTotal != haberTotal)
        {
            Error(tSaldos.Nombre, null, "saldo.descuadre", $"Los saldos no cuadran: debe {Redondeo.Formatear(debeTotal)} € y haber {Redondeo.Formatear(haberTotal)} €. Revisa la extracción: el asiento de apertura no se puede cargar.");
        }

        resumen.Add(new ResumenArchivo(tSaldos.Nombre, tSaldos.Filas.Count, saldos.Count));

        // Hispatec guarda los saldos en «Acumuladores», mantenidos por triggers. Si el paquete trae ambos, se
        // comparan: la apertura se hace siempre con los apuntes, y aquí se listan las cuentas descuadradas.
        var descuadradas = 0;
        if (paquete.Tiene("acumuladores.csv"))
        {
            var tAcum = paquete.Tabla("acumuladores.csv");
            var acum = LeerSaldos(tAcum).ToDictionary(s => s.Cuenta, s => s.Debe - s.Haber, StringComparer.Ordinal);
            var reales = saldos.ToDictionary(s => s.Cuenta, s => s.Debe - s.Haber, StringComparer.Ordinal);
            foreach (var cuenta in reales.Keys.Union(acum.Keys).Order(StringComparer.Ordinal))
            {
                var diferencia = reales.GetValueOrDefault(cuenta) - acum.GetValueOrDefault(cuenta);
                if (diferencia == 0m)
                {
                    continue;
                }

                descuadradas++;
                if (descuadradas <= MaximoDetalle)
                {
                    Aviso(tAcum.Nombre, null, "saldo.acumulador_descuadrado",
                        $"Cuenta {cuenta}: los apuntes dan {Redondeo.Formatear(reales.GetValueOrDefault(cuenta))} € y el acumulado de Hispatec {Redondeo.Formatear(acum.GetValueOrDefault(cuenta))} € (diferencia {Redondeo.Formatear(diferencia)} €). Se usan los apuntes.");
                }
            }

            resumen.Add(new ResumenArchivo(tAcum.Nombre, tAcum.Filas.Count, acum.Count));
        }

        // ---------------------------------------------------------- Cartera pendiente
        var idsCliente = clientes.Select(c => c.Id).ToHashSet(StringComparer.Ordinal);
        var idsProveedor = proveedores.Select(p => p.Id).ToHashSet(StringComparer.Ordinal);
        var cartera = new List<EfectoH>();
        var tCartera = paquete.Tabla("cartera.csv");
        foreach (var f in tCartera.Filas)
        {
            var (id, sentido, tercero, documento) = (f.Texto("id"), f.Texto("sentido")?.ToLowerInvariant(), f.Texto("id_tercero"), f.Texto("documento"));
            var (vencimiento, importe) = (Valor.Fecha(f.Texto("vencimiento")), Valor.Numero(f.Texto("importe")));
            if (id is null || sentido is not ("cobro" or "pago") || documento is null || vencimiento is null || importe is null)
            {
                Error(tCartera.Nombre, f.Numero, "cartera.incompleta", "Faltan datos del efecto (id, sentido cobro/pago, documento, vencimiento o importe).");
                continue;
            }

            var esCobro = sentido == "cobro";
            if (tercero is null || !(esCobro ? idsCliente : idsProveedor).Contains(tercero))
            {
                Error(tCartera.Nombre, f.Numero, "cartera.huerfana", $"El efecto {documento} apunta al {(esCobro ? "cliente" : "proveedor")} {tercero}, que no existe.");
                continue;
            }

            if (importe <= 0m)
            {
                Aviso(tCartera.Nombre, f.Numero, "cartera.importe", $"El efecto {documento} no tiene importe pendiente positivo ({Redondeo.Formatear(importe.Value)} €): se omite.");
                continue;
            }

            cartera.Add(new EfectoH(id, esCobro, tercero, Recortar(documento, 200)!, Valor.Fecha(f.Texto("fecha_documento")), vencimiento.Value, Redondeo.Dos(importe.Value)));
        }

        resumen.Add(new ResumenArchivo(tCartera.Nombre, tCartera.Filas.Count, cartera.Count));

        decimal Saldo(Func<string, bool> filtro, bool deudor) => saldos.Where(s => filtro(s.Cuenta)).Sum(s => deudor ? s.Debe - s.Haber : s.Haber - s.Debe);
        var saldoClientes = Saldo(c => c.StartsWith("43", StringComparison.Ordinal), deudor: true);
        var saldoProveedores = Saldo(c => c.StartsWith("40", StringComparison.Ordinal) || c.StartsWith("41", StringComparison.Ordinal), deudor: false);
        var (carteraCobros, carteraPagos) = (cartera.Where(e => e.EsCobro).Sum(e => e.Importe), cartera.Where(e => !e.EsCobro).Sum(e => e.Importe));
        if (saldos.Count > 0 && cartera.Count > 0)
        {
            if (carteraCobros != saldoClientes)
            {
                Aviso(tCartera.Nombre, null, "cartera.descuadre_clientes",
                    $"La cartera de cobro ({Redondeo.Formatear(carteraCobros)} €) no coincide con el saldo de clientes, cuentas 43 ({Redondeo.Formatear(saldoClientes)} €). Suele deberse a anticipos, efectos en gestión de cobro o apuntes sin efecto.");
            }

            if (carteraPagos != saldoProveedores)
            {
                Aviso(tCartera.Nombre, null, "cartera.descuadre_proveedores",
                    $"La cartera de pago ({Redondeo.Formatear(carteraPagos)} €) no coincide con el saldo de proveedores y acreedores, cuentas 40 y 41 ({Redondeo.Formatear(saldoProveedores)} €).");
            }
        }

        // ---------------------------------------------------------- Agro: campañas y parcelas
        var campanas = new List<CampanaH>();
        var tCamp = paquete.Tabla("campanas.csv");
        foreach (var f in tCamp.Filas)
        {
            var (id, codigo, desde, hasta) = (f.Texto("id"), f.Texto("codigo"), Valor.Fecha(f.Texto("desde")), Valor.Fecha(f.Texto("hasta")));
            if (id is null || codigo is null || desde is null || hasta is null || hasta < desde)
            {
                Error(tCamp.Nombre, f.Numero, "campana.invalida", $"Campaña «{codigo}» sin fechas válidas.");
                continue;
            }

            campanas.Add(new CampanaH(id, codigo, f.Texto("nombre") ?? codigo, desde.Value, hasta.Value));
        }

        foreach (var (a, b) in campanas.SelectMany((a, i) => campanas.Skip(i + 1).Select(b => (a, b))).Where(p => p.a.Desde <= p.b.Hasta && p.b.Desde <= p.a.Hasta).ToList())
        {
            Error(tCamp.Nombre, null, "campana.solapada", $"Las campañas {a.Codigo} y {b.Codigo} se solapan: en ALXOR no pueden solaparse. Se carga solo {a.Codigo}.");
            campanas.Remove(b);
        }

        resumen.Add(new ResumenArchivo(tCamp.Nombre, tCamp.Filas.Count, campanas.Count));

        var agricultores = proveedores.Where(p => p.Agricultor).Select(p => p.Id).ToHashSet(StringComparer.Ordinal);
        var idsArticulo = articulos.Select(a => a.Id).ToHashSet(StringComparer.Ordinal);
        var parcelas = new List<ParcelaH>();
        var tPar = paquete.Tabla("parcelas.csv");
        foreach (var f in tPar.Filas)
        {
            var (id, codigo, nombre, prov) = (f.Texto("id"), f.Texto("codigo"), f.Texto("nombre"), f.Texto("id_proveedor"));
            if (id is null || codigo is null || prov is null)
            {
                Error(tPar.Nombre, f.Numero, "parcela.incompleta", "Falta el id, el código o el agricultor de la parcela.");
                continue;
            }

            if (!agricultores.Contains(prov))
            {
                Error(tPar.Nombre, f.Numero, "parcela.sin_agricultor", $"La parcela {codigo} es del proveedor {prov}, que no existe o no está marcado como agricultor.");
                continue;
            }

            var sigpac = f.Texto("sigpac");
            if (sigpac is not null && (sigpac.Split(':').Length != 7 || sigpac.Split(':').Any(p => p.Length == 0 || !p.All(char.IsAsciiDigit))))
            {
                Aviso(tPar.Nombre, f.Numero, "parcela.sigpac", $"{codigo}: referencia SIGPAC «{sigpac}» mal formada; se carga sin ella.");
                sigpac = null;
            }

            var superficie = Valor.Numero(f.Texto("superficie_ha"));
            var articulo = f.Texto("id_articulo");
            if (articulo is not null && !idsArticulo.Contains(articulo))
            {
                Aviso(tPar.Nombre, f.Numero, "parcela.cultivo", $"{codigo}: el artículo de cultivo {articulo} no existe; queda sin cultivo.");
                articulo = null;
            }

            parcelas.Add(new ParcelaH(id, codigo, nombre ?? codigo, prov, sigpac, superficie is > 0m ? superficie : null, articulo, f.Texto("variedad")));
        }

        foreach (var g in parcelas.GroupBy(p => p.Codigo, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1).Take(MaximoDetalle))
        {
            Aviso(tPar.Nombre, null, "parcela.codigo_repetido", $"El código de parcela {g.Key} se repite: se carga el primero.");
        }

        parcelas = parcelas.GroupBy(p => p.Codigo, StringComparer.OrdinalIgnoreCase).Select(g => g.First()).ToList();
        resumen.Add(new ResumenArchivo(tPar.Nombre, tPar.Filas.Count, parcelas.Count));

        var bloqueantes = inc.Any(i => i.Nivel == NivelIncidencia.Error && i.Codigo is "paquete.estructura" or "saldo.descuadre");
        var m = paquete.Manifiesto;
        var informe = new InformeMigracion(
            m.GetValueOrDefault("empresa_nombre") ?? m.GetValueOrDefault("empresa_codigo"), paquete.FechaCorte, resumen, inc,
            inc.Count(i => i.Nivel == NivelIncidencia.Error), inc.Count(i => i.Nivel == NivelIncidencia.Aviso), !bloqueantes,
            new CuadresMigracion(debeTotal, haberTotal, carteraCobros, saldoClientes, carteraPagos, saldoProveedores, descuadradas));
        return (informe, new DatosHispatec(paquete.FechaCorte, clientes, proveedores, familias, articulos, cuentas, saldos, cartera, campanas, parcelas));
    }

    private static string? Subcuenta(string? valor, string archivo, int fila, List<Incidencia> inc)
    {
        if (valor is null)
        {
            return null;
        }

        if (!valor.All(char.IsAsciiDigit) || valor.Length is < 4 or > 12)
        {
            inc.Add(new Incidencia(archivo, fila, "subcuenta.invalida", NivelIncidencia.Aviso, $"La subcuenta «{valor}» no es numérica; se asignará una nueva."));
            return null;
        }

        return valor;
    }

    private static string? Recortar(string? texto, int maximo) => texto is null ? null : texto.Length <= maximo ? texto : texto[..maximo];
}
