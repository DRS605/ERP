using System.Globalization;
using AlxorCore.Agro.Aplicacion;
using AlxorCore.Agro.Dominio;
using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Catalogo.Dominio;
using AlxorCore.Contabilidad.Aplicacion;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Terceros.Aplicacion;
using AlxorCore.Tesoreria.Aplicacion;
using AlxorCore.Tesoreria.Dominio;

namespace AlxorCore.Migracion.Hispatec;

/// <summary>Correspondencia entre un registro de Hispatec y el que se creó (o reutilizó) en ALXOR.</summary>
public interface IRepositorioCorrespondencias
{
    Task<IReadOnlyDictionary<string, Guid>> TodasAsync(Guid empresaId, string entidad, CancellationToken ct = default);

    void Registrar(Guid empresaId, string entidad, string origenId, Guid destinoId);

    void RegistrarEjecucion(Guid empresaId, DateOnly fechaCorte, string resumen);

    Task<IReadOnlyList<EjecucionMigracionDto>> EjecucionesAsync(Guid empresaId, CancellationToken ct = default);
}

public interface IUnidadDeTrabajoMigracion : IUnidadDeTrabajo;

public sealed record EjecucionMigracionDto(Guid Id, DateOnly FechaCorte, string Resumen, DateTimeOffset CreadoEn);

/// <summary>Resultado de un paso de la carga.</summary>
public sealed record PasoCarga(string Paso, int Creados, int Reutilizados, int YaMigrados, IReadOnlyList<string> Errores);

public sealed record ResultadoCarga(InformeMigracion Informe, bool Cargado, IReadOnlyList<PasoCarga> Pasos);

/// <summary>
/// Carga en ALXOR lo válido de un paquete de Hispatec, siempre a través de los casos de uso de cada módulo
/// (con sus validaciones y garantías). Es <b>idempotente</b>: lo ya migrado se salta (tabla de
/// correspondencias) y lo que ya existía se reutiliza por su clave natural (NIF, código, referencia), así que
/// si se interrumpe se vuelve a lanzar y continúa. No es una transacción única: cada módulo guarda lo suyo.
/// </summary>
public sealed class CargaHispatec
{
    public const string Origen = "Hispatec";

    private readonly IRepositorioCorrespondencias _correspondencias;
    private readonly IUnidadDeTrabajoMigracion _unidad;
    private readonly IConsultaClientes _clientes;
    private readonly IConsultaProveedores _proveedores;
    private readonly CrearCliente _crearCliente;
    private readonly CrearProveedor _crearProveedor;
    private readonly IRepositorioFamilias _familias;
    private readonly CrearFamilia _crearFamilia;
    private readonly IConsultaProductos _productos;
    private readonly CrearProducto _crearProducto;
    private readonly ImportarPlanCuentas _planCuentas;
    private readonly AsignarSubcuentaTercero _subcuentas;
    private readonly CrearAsiento _crearAsiento;
    private readonly GestionCartera _cartera;
    private readonly MaestrosAgro _agro;
    private readonly IReloj _reloj;

    public CargaHispatec(
        IRepositorioCorrespondencias correspondencias, IUnidadDeTrabajoMigracion unidad, IConsultaClientes clientes, IConsultaProveedores proveedores,
        CrearCliente crearCliente, CrearProveedor crearProveedor, IRepositorioFamilias familias, CrearFamilia crearFamilia, IConsultaProductos productos,
        CrearProducto crearProducto, ImportarPlanCuentas planCuentas, AsignarSubcuentaTercero subcuentas, CrearAsiento crearAsiento, GestionCartera cartera,
        MaestrosAgro agro, IReloj reloj)
    {
        _correspondencias = correspondencias;
        _unidad = unidad;
        _clientes = clientes;
        _proveedores = proveedores;
        _crearCliente = crearCliente;
        _crearProveedor = crearProveedor;
        _familias = familias;
        _crearFamilia = crearFamilia;
        _productos = productos;
        _crearProducto = crearProducto;
        _planCuentas = planCuentas;
        _subcuentas = subcuentas;
        _crearAsiento = crearAsiento;
        _cartera = cartera;
        _agro = agro;
        _reloj = reloj;
    }

    /// <summary>Valida el paquete y, si se puede, lo carga. Sin <paramref name="aplicar"/> solo valida.</summary>
    public async Task<ResultadoCarga> EjecutarAsync(Guid grupoId, Guid empresaId, Paquete paquete, bool aplicar, CancellationToken ct = default)
    {
        var (informe, datos) = ValidadorHispatec.Validar(paquete);
        if (!aplicar || !informe.PuedeCargar)
        {
            return new ResultadoCarga(informe, false, []);
        }

        var pasos = new List<PasoCarga>();
        var clientes = await ClientesAsync(grupoId, empresaId, datos, pasos, ct).ConfigureAwait(false);
        var proveedores = await ProveedoresAsync(grupoId, empresaId, datos, pasos, ct).ConfigureAwait(false);
        var familias = await FamiliasAsync(grupoId, empresaId, datos, pasos, ct).ConfigureAwait(false);
        var articulos = await ArticulosAsync(grupoId, empresaId, datos, familias, pasos, ct).ConfigureAwait(false);
        await CuentasYSubcuentasAsync(empresaId, datos, clientes, proveedores, pasos, ct).ConfigureAwait(false);
        await AperturaAsync(empresaId, datos, pasos, ct).ConfigureAwait(false);
        await CarteraAsync(empresaId, datos, clientes, proveedores, pasos, ct).ConfigureAwait(false);
        await AgroAsync(empresaId, datos, proveedores, articulos, pasos, ct).ConfigureAwait(false);

        var resumen = string.Join(" · ", pasos.Select(p => string.Create(CultureInfo.InvariantCulture,
            $"{p.Paso}: {p.Creados} nuevos, {p.Reutilizados} reutilizados, {p.YaMigrados} ya migrados, {p.Errores.Count} errores")));
        _correspondencias.RegistrarEjecucion(empresaId, datos.FechaCorte, resumen.Length > 2000 ? resumen[..2000] : resumen);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return new ResultadoCarga(informe, true, pasos);
    }

    // ------------------------------------------------------------------ Terceros
    private static string? ClaveNif(string? nif) => string.IsNullOrWhiteSpace(nif) ? null : nif.Replace(" ", string.Empty, StringComparison.Ordinal).Replace("-", string.Empty, StringComparison.Ordinal).ToUpperInvariant();

    private async Task<IReadOnlyDictionary<string, Guid>> ClientesAsync(Guid grupoId, Guid empresaId, DatosHispatec datos, List<PasoCarga> pasos, CancellationToken ct)
    {
        var mapa = (await _correspondencias.TodasAsync(empresaId, "cliente", ct).ConfigureAwait(false)).ToDictionary(k => k.Key, v => v.Value);
        var porNif = (await _clientes.ListarAsync(grupoId, incluirInactivos: true, ct: ct).ConfigureAwait(false))
            .Where(c => ClaveNif(c.NifFiscal) is not null).GroupBy(c => ClaveNif(c.NifFiscal)!).ToDictionary(g => g.Key, g => g.First().Id);
        int creados = 0, reutilizados = 0, ya = 0;
        var errores = new List<string>();
        foreach (var c in datos.Clientes)
        {
            if (mapa.ContainsKey(c.Id))
            {
                ya++;
                continue;
            }

            var t = c.Tercero;
            if (ClaveNif(t.Nif) is { } nif && porNif.TryGetValue(nif, out var existente))
            {
                Registrar(empresaId, "cliente", c.Id, existente, mapa);
                reutilizados++;
                continue;
            }

            var r = await _crearCliente.EjecutarAsync(grupoId, new DatosCliente(t.Nombre, t.Nif, t.Email, t.Calle, t.CodigoPostal, t.Poblacion, t.Provincia,
                Pais(t.Pais), Iban: t.Iban), ct).ConfigureAwait(false);
            if (r.EsFallo)
            {
                errores.Add($"Cliente {c.Codigo} ({t.Nombre}): {r.Error.Mensaje}");
                continue;
            }

            Registrar(empresaId, "cliente", c.Id, r.Valor.Id, mapa);
            if (ClaveNif(t.Nif) is { } n)
            {
                porNif[n] = r.Valor.Id;
            }

            creados++;
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        pasos.Add(new PasoCarga("Clientes", creados, reutilizados, ya, errores));
        return mapa;
    }

    private async Task<IReadOnlyDictionary<string, Guid>> ProveedoresAsync(Guid grupoId, Guid empresaId, DatosHispatec datos, List<PasoCarga> pasos, CancellationToken ct)
    {
        var mapa = (await _correspondencias.TodasAsync(empresaId, "proveedor", ct).ConfigureAwait(false)).ToDictionary(k => k.Key, v => v.Value);
        var porNif = (await _proveedores.ListarAsync(grupoId, incluirInactivos: true, ct: ct).ConfigureAwait(false))
            .Where(p => ClaveNif(p.NifFiscal) is not null).GroupBy(p => ClaveNif(p.NifFiscal)!).ToDictionary(g => g.Key, g => g.First().Id);
        int creados = 0, reutilizados = 0, ya = 0;
        var errores = new List<string>();
        foreach (var p in datos.Proveedores)
        {
            if (mapa.ContainsKey(p.Id))
            {
                ya++;
                continue;
            }

            var t = p.Tercero;
            if (ClaveNif(t.Nif) is { } nif && porNif.TryGetValue(nif, out var existente))
            {
                Registrar(empresaId, "proveedor", p.Id, existente, mapa);
                reutilizados++;
                continue;
            }

            var r = await _crearProveedor.EjecutarAsync(grupoId, new DatosProveedor(t.Nombre, t.Nif, t.Email, t.Calle, t.CodigoPostal, t.Poblacion, t.Provincia,
                Pais(t.Pais), p.Retencion ?? 0m, Iban: t.Iban), ct).ConfigureAwait(false);
            if (r.EsFallo)
            {
                errores.Add($"Proveedor {p.Codigo} ({t.Nombre}): {r.Error.Mensaje}");
                continue;
            }

            Registrar(empresaId, "proveedor", p.Id, r.Valor.Id, mapa);
            if (ClaveNif(t.Nif) is { } n)
            {
                porNif[n] = r.Valor.Id;
            }

            creados++;
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        pasos.Add(new PasoCarga("Proveedores", creados, reutilizados, ya, errores));
        return mapa;
    }

    // ------------------------------------------------------------------ Catálogo
    private async Task<IReadOnlyDictionary<string, Guid>> FamiliasAsync(Guid grupoId, Guid empresaId, DatosHispatec datos, List<PasoCarga> pasos, CancellationToken ct)
    {
        var mapa = (await _correspondencias.TodasAsync(empresaId, "familia", ct).ConfigureAwait(false)).ToDictionary(k => k.Key, v => v.Value);
        if (datos.Familias.Count == 0)
        {
            return mapa;
        }

        var existentes = await _familias.ListarTodasAsync(grupoId, ct).ConfigureAwait(false);
        int creados = 0, reutilizados = 0, ya = 0;
        var errores = new List<string>();

        // Primero las familias raíz y después las hijas, nivel a nivel.
        var pendientes = datos.Familias.ToList();
        while (pendientes.Count > 0)
        {
            var listas = pendientes.Where(f => f.IdSuperior is null || mapa.ContainsKey(f.IdSuperior) || !pendientes.Any(p => p.Id == f.IdSuperior)).ToList();
            if (listas.Count == 0)
            {
                errores.AddRange(pendientes.Select(f => $"Familia {f.Nombre}: su jerarquía tiene un ciclo."));
                break;
            }

            foreach (var f in listas)
            {
                pendientes.Remove(f);
                if (mapa.ContainsKey(f.Id))
                {
                    ya++;
                    continue;
                }

                var padre = f.IdSuperior is not null && mapa.TryGetValue(f.IdSuperior, out var idPadre) ? idPadre : (Guid?)null;
                var igual = existentes.FirstOrDefault(e => (f.Codigo is not null && string.Equals(e.Codigo, f.Codigo, StringComparison.OrdinalIgnoreCase))
                                                           || (f.Codigo is null && string.Equals(e.Nombre, f.Nombre, StringComparison.OrdinalIgnoreCase)));
                if (igual is not null)
                {
                    Registrar(empresaId, "familia", f.Id, igual.Id, mapa);
                    reutilizados++;
                    continue;
                }

                var r = await _crearFamilia.EjecutarAsync(grupoId, new DatosFamilia(f.Nombre, f.Codigo, padre), ct).ConfigureAwait(false);
                if (r.EsFallo)
                {
                    errores.Add($"Familia {f.Nombre}: {r.Error.Mensaje}");
                    continue;
                }

                Registrar(empresaId, "familia", f.Id, r.Valor.Id, mapa);
                creados++;
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        pasos.Add(new PasoCarga("Familias", creados, reutilizados, ya, errores));
        return mapa;
    }

    private async Task<IReadOnlyDictionary<string, Guid>> ArticulosAsync(Guid grupoId, Guid empresaId, DatosHispatec datos, IReadOnlyDictionary<string, Guid> familias,
        List<PasoCarga> pasos, CancellationToken ct)
    {
        var mapa = (await _correspondencias.TodasAsync(empresaId, "articulo", ct).ConfigureAwait(false)).ToDictionary(k => k.Key, v => v.Value);
        if (datos.Articulos.Count == 0)
        {
            return mapa;
        }

        var porReferencia = (await _productos.ListarAsync(grupoId, incluirInactivos: true, ct: ct).ConfigureAwait(false))
            .Where(p => !string.IsNullOrWhiteSpace(p.Referencia)).GroupBy(p => p.Referencia!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Id, StringComparer.OrdinalIgnoreCase);
        int creados = 0, reutilizados = 0, ya = 0;
        var errores = new List<string>();
        foreach (var a in datos.Articulos)
        {
            if (mapa.ContainsKey(a.Id))
            {
                ya++;
                continue;
            }

            if (porReferencia.TryGetValue(a.Codigo, out var existente))
            {
                Registrar(empresaId, "articulo", a.Id, existente, mapa);
                reutilizados++;
                continue;
            }

            var r = await _crearProducto.EjecutarAsync(grupoId, empresaId, new DatosProducto(
                a.Nombre, a.PrecioVenta, a.Codigo, a.EsServicio ? TipoProducto.Servicio : TipoProducto.Bien, CodigoImpuesto(a.Iva), a.Unidad, a.PrecioCompra,
                ControlarStock: a.ControlaStock && !a.EsServicio, FamiliaId: a.IdFamilia is not null && familias.TryGetValue(a.IdFamilia, out var fam) ? fam : null), ct).ConfigureAwait(false);
            if (r.EsFallo)
            {
                errores.Add($"Artículo {a.Codigo} ({a.Nombre}): {r.Error.Mensaje}");
                continue;
            }

            Registrar(empresaId, "articulo", a.Id, r.Valor.Id, mapa);
            porReferencia[a.Codigo] = r.Valor.Id;
            creados++;
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        pasos.Add(new PasoCarga("Artículos", creados, reutilizados, ya, errores));
        return mapa;
    }

    // ------------------------------------------------------------------ Contabilidad
    private async Task CuentasYSubcuentasAsync(Guid empresaId, DatosHispatec datos, IReadOnlyDictionary<string, Guid> clientes, IReadOnlyDictionary<string, Guid> proveedores,
        List<PasoCarga> pasos, CancellationToken ct)
    {
        var conocidas = datos.Cuentas.Select(c => c.Codigo).ToHashSet(StringComparer.Ordinal);
        var cuentas = datos.Cuentas.Concat(datos.Saldos.Where(s => !conocidas.Contains(s.Cuenta)).Select(s => (s.Cuenta, $"Cuenta {s.Cuenta} (Hispatec)"))).ToList();
        if (cuentas.Count > 0)
        {
            var (creadas, rechazadas) = await _planCuentas.EjecutarAsync(empresaId, cuentas, ct).ConfigureAwait(false);
            pasos.Add(new PasoCarga("Plan de cuentas", creadas, cuentas.Count - creadas - rechazadas.Count, 0, rechazadas));
        }

        // Las subcuentas de Hispatec (430…, 400…) se enlazan con su cliente o proveedor en ALXOR, con el mismo código.
        var errores = new List<string>();
        var enlazadas = 0;
        foreach (var (tipo, id, subcuenta, nombre) in datos.Clientes.Where(c => c.Subcuenta is not null).Select(c => (TipoTerceroContable.Cliente, clientes.GetValueOrDefault(c.Id), c.Subcuenta!, c.Tercero.Nombre))
                     .Concat(datos.Proveedores.Where(p => p.Subcuenta is not null).Select(p => (TipoTerceroContable.Proveedor, proveedores.GetValueOrDefault(p.Id), p.Subcuenta!, p.Tercero.Nombre))))
        {
            if (id == Guid.Empty)
            {
                continue;
            }

            var r = await _subcuentas.EjecutarAsync(empresaId, new AsignarSubcuentaComando(tipo, id, nombre, subcuenta), ct).ConfigureAwait(false);
            if (r.EsFallo)
            {
                errores.Add($"Subcuenta {subcuenta} de {nombre}: {r.Error.Mensaje}");
            }
            else
            {
                enlazadas++;
            }
        }

        if (enlazadas > 0 || errores.Count > 0)
        {
            pasos.Add(new PasoCarga("Subcuentas de terceros", enlazadas, 0, 0, errores));
        }
    }

    /// <summary>Asiento de apertura con los saldos a la fecha de corte, el día siguiente al corte. Solo una vez por fecha de corte.</summary>
    private async Task AperturaAsync(Guid empresaId, DatosHispatec datos, List<PasoCarga> pasos, CancellationToken ct)
    {
        if (datos.Saldos.Count == 0)
        {
            return;
        }

        var clave = datos.FechaCorte.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var hechas = await _correspondencias.TodasAsync(empresaId, "apertura", ct).ConfigureAwait(false);
        if (hechas.ContainsKey(clave))
        {
            pasos.Add(new PasoCarga("Asiento de apertura", 0, 0, 1, []));
            return;
        }

        var lineas = datos.Saldos.OrderBy(s => s.Cuenta, StringComparer.Ordinal)
            .Select(s => new LineaAsientoComando(s.Cuenta, s.Debe, s.Haber, "Saldo a " + datos.FechaCorte.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture))).ToList();
        var r = await _crearAsiento.EjecutarAsync(empresaId, new CrearAsientoComando(datos.FechaCorte.AddDays(1),
            $"Apertura · migración desde Hispatec (saldos a {datos.FechaCorte:dd/MM/yyyy})", lineas), ct).ConfigureAwait(false);
        if (r.EsFallo)
        {
            pasos.Add(new PasoCarga("Asiento de apertura", 0, 0, 0, [r.Error.Mensaje]));
            return;
        }

        _correspondencias.Registrar(empresaId, "apertura", clave, r.Valor.Id);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        pasos.Add(new PasoCarga("Asiento de apertura", 1, 0, 0, []));
    }

    // ------------------------------------------------------------------ Tesorería
    private async Task CarteraAsync(Guid empresaId, DatosHispatec datos, IReadOnlyDictionary<string, Guid> clientes, IReadOnlyDictionary<string, Guid> proveedores,
        List<PasoCarga> pasos, CancellationToken ct)
    {
        if (datos.Cartera.Count == 0)
        {
            return;
        }

        var existentes = (await _cartera.ListarAsync(empresaId, null, soloPendientes: false, ct).ConfigureAwait(false))
            .Where(e => e.Origen == Origen && e.OrigenReferencia is not null).Select(e => e.OrigenReferencia!).ToHashSet(StringComparer.Ordinal);
        var nombres = datos.Clientes.ToDictionary(c => c.Id, c => c.Tercero.Nombre).Concat(datos.Proveedores.ToDictionary(p => p.Id, p => p.Tercero.Nombre))
            .ToDictionary(k => k.Key, v => v.Value, StringComparer.Ordinal);
        int creados = 0, ya = 0;
        var errores = new List<string>();
        foreach (var e in datos.Cartera)
        {
            if (existentes.Contains(e.Id))
            {
                ya++;
                continue;
            }

            var terceroId = (e.EsCobro ? clientes : proveedores).TryGetValue(e.IdTercero, out var t) ? t : (Guid?)null;
            var r = await _cartera.CrearAsync(empresaId, new CrearEfectoCarteraComando(e.EsCobro ? SentidoCartera.Cobro : SentidoCartera.Pago,
                nombres.GetValueOrDefault(e.IdTercero) ?? e.IdTercero, e.Documento, e.Vencimiento, e.Importe, terceroId, e.FechaDocumento, Origen, e.Id), ct).ConfigureAwait(false);
            if (r.EsFallo)
            {
                errores.Add($"Efecto {e.Documento}: {r.Error.Mensaje}");
                continue;
            }

            creados++;
        }

        pasos.Add(new PasoCarga("Cartera pendiente", creados, 0, ya, errores));
    }

    // ------------------------------------------------------------------ Agro
    private async Task AgroAsync(Guid empresaId, DatosHispatec datos, IReadOnlyDictionary<string, Guid> proveedores, IReadOnlyDictionary<string, Guid> articulos,
        List<PasoCarga> pasos, CancellationToken ct)
    {
        var agricultoresH = datos.Proveedores.Where(p => p.Agricultor).ToList();
        if (agricultoresH.Count == 0 && datos.Campanas.Count == 0 && datos.Parcelas.Count == 0)
        {
            return;
        }

        // Agricultores: el proveedor con su ficha agrícola. Hispatec no distingue el régimen (REAGP o general):
        // se da de alta en el REAGP y la autorización de autofacturación, si la tenía, desde la fecha de corte.
        var mapa = (await _correspondencias.TodasAsync(empresaId, "agricultor", ct).ConfigureAwait(false)).ToDictionary(k => k.Key, v => v.Value);
        var existentes = (await _agro.AgricultoresAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(a => a.ProveedorId, a => a.Id);
        int creados = 0, reutilizados = 0, ya = 0;
        var errores = new List<string>();
        foreach (var p in agricultoresH)
        {
            if (mapa.ContainsKey(p.Id))
            {
                ya++;
                continue;
            }

            if (!proveedores.TryGetValue(p.Id, out var proveedorId))
            {
                continue;
            }

            if (existentes.TryGetValue(proveedorId, out var agricultorId))
            {
                Registrar(empresaId, "agricultor", p.Id, agricultorId, mapa);
                reutilizados++;
                continue;
            }

            var r = await _agro.CrearAgricultorAsync(empresaId, new DatosAgricultor(proveedorId, RegimenAgricultor.Reagp, p.Retencion,
                p.AutorizaAutofactura ? datos.FechaCorte : null), ct).ConfigureAwait(false);
            if (r.EsFallo)
            {
                errores.Add($"Agricultor {p.Codigo}: {r.Error.Mensaje}");
                continue;
            }

            Registrar(empresaId, "agricultor", p.Id, r.Valor.Id, mapa);
            existentes[proveedorId] = r.Valor.Id;
            creados++;
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        pasos.Add(new PasoCarga("Agricultores", creados, reutilizados, ya, errores));

        // Campañas.
        var mapaCampanas = (await _correspondencias.TodasAsync(empresaId, "campana", ct).ConfigureAwait(false)).ToDictionary(k => k.Key, v => v.Value);
        var campanas = (await _agro.CampanasAsync(empresaId, ct).ConfigureAwait(false)).ToList();
        (creados, reutilizados, ya) = (0, 0, 0);
        var erroresCampanas = new List<string>();
        foreach (var c in datos.Campanas)
        {
            if (mapaCampanas.ContainsKey(c.Id))
            {
                ya++;
                continue;
            }

            var igual = campanas.FirstOrDefault(x => string.Equals(x.Codigo, c.Codigo, StringComparison.OrdinalIgnoreCase));
            if (igual is not null)
            {
                Registrar(empresaId, "campana", c.Id, igual.Id, mapaCampanas);
                reutilizados++;
                continue;
            }

            var r = await _agro.CrearCampanaAsync(empresaId, new DatosCampana(c.Codigo, c.Nombre, c.Desde, c.Hasta), ct).ConfigureAwait(false);
            if (r.EsFallo)
            {
                erroresCampanas.Add($"Campaña {c.Codigo}: {r.Error.Mensaje}");
                continue;
            }

            Registrar(empresaId, "campana", c.Id, r.Valor.Id, mapaCampanas);
            campanas.Add(r.Valor);
            creados++;
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        if (datos.Campanas.Count > 0)
        {
            pasos.Add(new PasoCarga("Campañas", creados, reutilizados, ya, erroresCampanas));
        }

        // Parcelas.
        var mapaParcelas = (await _correspondencias.TodasAsync(empresaId, "parcela", ct).ConfigureAwait(false)).ToDictionary(k => k.Key, v => v.Value);
        var parcelas = (await _agro.ParcelasAsync(empresaId, null, ct).ConfigureAwait(false)).ToDictionary(p => p.Codigo, p => p.Id, StringComparer.OrdinalIgnoreCase);
        (creados, reutilizados, ya) = (0, 0, 0);
        var erroresParcelas = new List<string>();
        foreach (var p in datos.Parcelas)
        {
            if (mapaParcelas.ContainsKey(p.Id))
            {
                ya++;
                continue;
            }

            var codigo = p.Codigo.ToUpperInvariant();
            if (parcelas.TryGetValue(codigo, out var existente))
            {
                Registrar(empresaId, "parcela", p.Id, existente, mapaParcelas);
                reutilizados++;
                continue;
            }

            if (!mapa.TryGetValue(p.IdProveedor, out var agricultorId))
            {
                erroresParcelas.Add($"Parcela {p.Codigo}: su agricultor no se pudo dar de alta.");
                continue;
            }

            var r = await _agro.CrearParcelaAsync(empresaId, agricultorId, new DatosParcela(p.Codigo, p.Nombre, p.Sigpac, p.SuperficieHa,
                p.IdArticulo is not null && articulos.TryGetValue(p.IdArticulo, out var art) ? art : null, p.Variedad), ct).ConfigureAwait(false);
            if (r.EsFallo)
            {
                erroresParcelas.Add($"Parcela {p.Codigo}: {r.Error.Mensaje}");
                continue;
            }

            Registrar(empresaId, "parcela", p.Id, r.Valor.Id, mapaParcelas);
            parcelas[codigo] = r.Valor.Id;
            creados++;
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        if (datos.Parcelas.Count > 0)
        {
            pasos.Add(new PasoCarga("Parcelas", creados, reutilizados, ya, erroresParcelas));
        }
    }

    // ------------------------------------------------------------------ Utilidades
    private void Registrar(Guid empresaId, string entidad, string origen, Guid destino, Dictionary<string, Guid> mapa)
    {
        _correspondencias.Registrar(empresaId, entidad, origen, destino);
        mapa[origen] = destino;
    }

    /// <summary>País ISO de dos letras (España si no se indica o si viene con otro formato).</summary>
    private static string Pais(string? pais) => pais is { Length: 2 } p && p.All(char.IsAsciiLetter) ? p : "ES";

    /// <summary>Código del catálogo para un porcentaje: IVA si existe en IVA, IGIC para los tipos propios del IGIC.</summary>
    private static string CodigoImpuesto(decimal porcentaje) => porcentaje switch
    {
        21m => "IVA21",
        10m => "IVA10",
        4m => "IVA4",
        0m => "IVA0",
        3m => "IGIC3",
        7m => "IGIC7",
        9.5m => "IGIC95",
        15m => "IGIC15",
        20m => "IGIC20",
        _ => "IVA21",
    };
}
