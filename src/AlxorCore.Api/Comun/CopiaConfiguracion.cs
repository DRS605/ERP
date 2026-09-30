using System.Security.Claims;
using AlxorCore.Agro.Aplicacion;
using AlxorCore.Agro.Dominio;
using AlxorCore.Contabilidad.Aplicacion;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Inventario.Aplicacion;
using AlxorCore.Logistica.Aplicacion;
using AlxorCore.Nucleo.Modulos;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Seguridad;
using AlxorCore.Organizacion.Aplicacion;
using AlxorCore.Organizacion.Aplicacion.CasosDeUso;
using AlxorCore.Organizacion.Aplicacion.Puertos;

namespace AlxorCore.Api.Comun;

/// <summary>Empresa de la que se puede copiar la configuración.</summary>
public sealed record OrigenCopiaDto(Guid Id, string Nif, string RazonSocial, bool MismoGrupo);

/// <summary>Un bloque de configuración que se puede copiar (plan de cuentas, formas de pago…).</summary>
public sealed record ElementoCopiaDto(string Clave, string Nombre, string Descripcion, string? Modulo, bool SoloMismoGrupo);

/// <summary>Qué copiar y de qué empresa.</summary>
public sealed record PeticionCopia(Guid OrigenEmpresaId, IReadOnlyList<string>? Elementos);

/// <summary>Resultado de un bloque: lo que se crea (o se crearía), lo que ya estaba y lo que no se puede copiar.</summary>
public sealed record ResultadoElementoCopia(string Clave, string Nombre, IReadOnlyList<string> Nuevos, int YaExistian, IReadOnlyList<string> Omitidos,
    IReadOnlyList<string> Errores);

/// <summary>Resultado de la copia (o de su vista previa).</summary>
public sealed record ResultadoCopiaDto(Guid OrigenEmpresaId, string Origen, bool Ejecutada, int Creados, IReadOnlyList<ResultadoElementoCopia> Elementos);

/// <summary>
/// Copia la configuración de una empresa a la empresa activa: plan de cuentas, diarios, formas de pago, series,
/// almacenes, transportistas, soportes y maestros de agro. Solo añade lo que falta (se compara por código, nombre o
/// matrícula) y nunca cambia lo que la empresa de destino ya tiene, así que se puede repetir sin duplicar nada.
/// Lo que apunta a artículos o agricultores (maestros del grupo) solo se copia entre empresas del mismo grupo.
/// Cada empresa se lee y se escribe en su propio ámbito, con su empresa activa, para que la seguridad por empresa de
/// la base de datos se cumpla. El usuario tiene que tener acceso a la empresa de origen.
/// </summary>
public sealed class CopiaConfiguracion
{
    public static readonly IReadOnlyList<ElementoCopiaDto> Catalogo =
    [
        new("cuentas", "Plan de cuentas", "Las cuentas y subcuentas que la empresa de destino no tiene.", CatalogoModulos.Contabilidad, false),
        new("diarios", "Diarios contables", "Los diarios propios con los orígenes que recogen.", CatalogoModulos.Contabilidad, false),
        new("formas_pago", "Formas de pago", "Contado, aplazadas, con sus días de vencimiento.", null, false),
        new("series", "Series de numeración", "Las series del ejercicio en curso y siguientes, empezando en el 1.", null, false),
        new("almacenes", "Almacenes y ubicaciones", "Los almacenes con sus ubicaciones (sin existencias).", CatalogoModulos.Inventario, false),
        new("transporte", "Transportistas y vehículos", "Transportistas y sus vehículos para cartas de porte y órdenes de carga.", null, false),
        new("soportes", "Soportes logísticos", "Europalé, medio palé, contenedor… con medidas y tara.", CatalogoModulos.Logistica, false),
        new("agro_campanas", "Campañas agrícolas", "Código, nombre y fechas de cada campaña.", CatalogoModulos.Agro, false),
        new("agro_categorias", "Categorías de fruta", "Extra, primera, destrío… con su orden.", CatalogoModulos.Agro, false),
        new("agro_conceptos", "Conceptos de liquidación", "Cargos y abonos generales de la liquidación al agricultor.", CatalogoModulos.Agro, false),
        new("agro_taras", "Taras de envases", "La tara vigente de cada envase (palot, caja, box…).", CatalogoModulos.Agro, true),
    ];

    private readonly IServiceScopeFactory _ambitos;

    public CopiaConfiguracion(IServiceScopeFactory ambitos) => _ambitos = ambitos;

    private async Task<AsyncServiceScope> AmbitoAsync(Guid empresaId, CancellationToken ct)
    {
        var ambito = _ambitos.CreateAsyncScope();
        var contexto = ambito.ServiceProvider.GetRequiredService<IContextoEmpresaMutable>();
        contexto.Fijar(empresaId);
        if (await ambito.ServiceProvider.GetRequiredService<IRepositorioEmpresas>().ObtenerGrupoIdAsync(empresaId, ct).ConfigureAwait(false) is { } grupo)
        {
            contexto.FijarGrupo(grupo);
        }

        return ambito;
    }

    private static T S<T>(AsyncServiceScope a) where T : notnull => a.ServiceProvider.GetRequiredService<T>();

    /// <summary>Empresas del usuario (salvo la activa) de las que puede copiar.</summary>
    public async Task<IReadOnlyList<OrigenCopiaDto>> OrigenesAsync(Guid destinoId, Guid usuarioId, CancellationToken ct = default)
    {
        await using var ambito = await AmbitoAsync(destinoId, ct).ConfigureAwait(false);
        var grupo = S<IContextoEmpresa>(ambito).GrupoId;
        var repo = S<IRepositorioEmpresas>(ambito);
        var lista = new List<OrigenCopiaDto>();
        foreach (var e in await S<IConsultasOrganizacion>(ambito).ListarEmpresasDeUsuarioAsync(usuarioId, ct).ConfigureAwait(false))
        {
            if (e.Id == destinoId)
            {
                continue;
            }

            var suyo = await repo.ObtenerGrupoIdAsync(e.Id, ct).ConfigureAwait(false);
            lista.Add(new OrigenCopiaDto(e.Id, e.Nif, e.RazonSocial, grupo is not null && suyo == grupo));
        }

        return lista.OrderBy(o => o.RazonSocial, StringComparer.CurrentCulture).ToList();
    }

    /// <summary>Vista previa (<paramref name="ejecutar"/> = false) o copia de la configuración elegida.</summary>
    public async Task<Resultado<ResultadoCopiaDto>> CopiarAsync(Guid destinoId, Guid usuarioId, ClaimsPrincipal usuario, PeticionCopia peticion, bool ejecutar,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(peticion);
        ArgumentNullException.ThrowIfNull(usuario);
        if (peticion.OrigenEmpresaId == destinoId)
        {
            return Resultado.Fallo<ResultadoCopiaDto>(Error.Validacion("copia.misma_empresa", "Elige otra empresa: no se puede copiar una empresa sobre sí misma."));
        }

        var origen = (await OrigenesAsync(destinoId, usuarioId, ct).ConfigureAwait(false)).FirstOrDefault(o => o.Id == peticion.OrigenEmpresaId);
        if (origen is null)
        {
            return Resultado.Fallo<ResultadoCopiaDto>(Error.NoEncontrado("copia.origen", "No tienes acceso a esa empresa."));
        }

        var pedidos = (peticion.Elementos ?? []).Select(e => e.Trim()).ToHashSet(StringComparer.Ordinal);
        if (pedidos.Count == 0)
        {
            return Resultado.Fallo<ResultadoCopiaDto>(Error.Validacion("copia.sin_elementos", "Marca al menos una cosa que copiar."));
        }

        if (pedidos.FirstOrDefault(p => Catalogo.All(c => c.Clave != p)) is { } desconocido)
        {
            return Resultado.Fallo<ResultadoCopiaDto>(Error.Validacion("copia.elemento", $"No se puede copiar «{desconocido}»."));
        }

        // Los módulos contratados de la empresa activa viajan en el token (sin edición en el token, todo está incluido).
        var conEdicion = usuario.FindFirst(ClaimsAlxor.Edicion) is not null;
        var resultados = new List<ResultadoElementoCopia>();
        await using var o = await AmbitoAsync(origen.Id, ct).ConfigureAwait(false);
        await using var d = await AmbitoAsync(destinoId, ct).ConfigureAwait(false);
        foreach (var el in Catalogo.Where(c => pedidos.Contains(c.Clave)))
        {
            var r = new Acumulado(el);
            if (el.Modulo is { } m && conEdicion && !usuario.HasClaim(ClaimsAlxor.Modulo, m))
            {
                r.Omitidos.Add("Tu plan no incluye este módulo en esta empresa.");
            }
            else if (el.SoloMismoGrupo && !origen.MismoGrupo)
            {
                r.Omitidos.Add("Solo entre empresas del mismo grupo: hace referencia a artículos, que son del grupo.");
            }
            else
            {
                await CopiarElementoAsync(el.Clave, o, d, origen, destinoId, ejecutar, r, ct).ConfigureAwait(false);
            }

            resultados.Add(r.Cerrar());
        }

        return Resultado.Ok(new ResultadoCopiaDto(origen.Id, origen.RazonSocial, ejecutar,
            ejecutar ? resultados.Sum(x => x.Nuevos.Count) : 0, resultados));
    }

    private sealed class Acumulado(ElementoCopiaDto el)
    {
        public List<string> Nuevos { get; } = [];
        public int YaExistian { get; set; }
        public List<string> Omitidos { get; } = [];
        public List<string> Errores { get; } = [];

        /// <summary>Registra el resultado de crear un elemento: si falla, no cuenta como nuevo.</summary>
        public void Anotar(string nombre, Resultado r)
        {
            if (r.EsCorrecto)
            {
                Nuevos.Add(nombre);
            }
            else
            {
                Errores.Add($"{nombre}: {r.Error.Mensaje}");
            }
        }

        public ResultadoElementoCopia Cerrar() => new(el.Clave, el.Nombre, Nuevos, YaExistian, Omitidos, Errores);
    }

    private static string Clave(string? s) => (s ?? string.Empty).Trim().ToUpperInvariant();

    private static async Task CopiarElementoAsync(string clave, AsyncServiceScope o, AsyncServiceScope d, OrigenCopiaDto origen, Guid destino, bool ejecutar,
        Acumulado r, CancellationToken ct)
    {
        var eo = origen.Id;
        switch (clave)
        {
            case "cuentas":
            {
                var desde = await S<ListarCuentas>(o).EjecutarAsync(eo, ct).ConfigureAwait(false);
                var hay = (await S<ListarCuentas>(d).EjecutarAsync(destino, ct).ConfigureAwait(false)).Select(c => c.Codigo).ToHashSet(StringComparer.Ordinal);
                foreach (var c in desde.OrderBy(c => c.Codigo.Length).ThenBy(c => c.Codigo, StringComparer.Ordinal))
                {
                    if (hay.Contains(c.Codigo)) { r.YaExistian++; continue; }
                    var nombre = $"{c.Codigo} {c.Nombre}";
                    if (!ejecutar) { r.Nuevos.Add(nombre); continue; }
                    r.Anotar(nombre, await S<GestionCuentas>(d).CrearAsync(destino, new DatosCuenta(c.Codigo, c.Nombre), ct).ConfigureAwait(false));
                }

                break;
            }

            case "diarios":
            {
                var hay = (await S<GestionDiarios>(d).ListarAsync(destino, ct).ConfigureAwait(false)).Select(x => Clave(x.Codigo)).ToHashSet(StringComparer.Ordinal);
                foreach (var x in (await S<GestionDiarios>(o).ListarAsync(eo, ct).ConfigureAwait(false)).Where(x => !x.DeSistema))
                {
                    if (hay.Contains(Clave(x.Codigo))) { r.YaExistian++; continue; }
                    var nombre = $"{x.Codigo} · {x.Nombre}";
                    if (!ejecutar) { r.Nuevos.Add(nombre); continue; }
                    r.Anotar(nombre, await S<GestionDiarios>(d).CrearAsync(destino, new DatosDiario(x.Codigo, x.Nombre, x.Origenes, x.Activo), ct).ConfigureAwait(false));
                }

                break;
            }

            case "formas_pago":
            {
                var hay = (await S<ListarFormasPago>(d).EjecutarAsync(destino, false, ct).ConfigureAwait(false)).Select(x => Clave(x.Nombre)).ToHashSet(StringComparer.Ordinal);
                foreach (var x in (await S<ListarFormasPago>(o).EjecutarAsync(eo, false, ct).ConfigureAwait(false)).Where(x => x.Activo))
                {
                    if (hay.Contains(Clave(x.Nombre))) { r.YaExistian++; continue; }
                    if (!ejecutar) { r.Nuevos.Add(x.Nombre); continue; }
                    r.Anotar(x.Nombre, await S<GuardarFormaPago>(d).EjecutarAsync(destino, null,
                        new DatosFormaPago(x.Nombre, x.GeneraVencimiento, x.DiasVencimiento, x.RegistrarPagoAutomatico), ct).ConfigureAwait(false));
                }

                break;
            }

            case "series":
            {
                var anio = DateTime.Today.Year;
                var hay = (await S<ListarSeries>(d).EjecutarAsync(destino, ct).ConfigureAwait(false))
                    .Select(x => (x.TipoDocumento, x.Ejercicio, P: Clave(x.Prefijo))).ToHashSet();
                foreach (var x in (await S<ListarSeries>(o).EjecutarAsync(eo, ct).ConfigureAwait(false)).Where(x => x.Ejercicio >= anio))
                {
                    if (hay.Contains((x.TipoDocumento, x.Ejercicio, Clave(x.Prefijo)))) { r.YaExistian++; continue; }
                    var nombre = $"{x.Prefijo} · {x.TipoDocumento} {x.Ejercicio}";
                    if (!ejecutar) { r.Nuevos.Add(nombre); continue; }
                    r.Anotar(nombre, await S<CrearSerie>(d).EjecutarAsync(destino, new CrearSerieComando(x.TipoDocumento, x.Ejercicio, x.Prefijo), ct).ConfigureAwait(false));
                }

                break;
            }

            case "almacenes":
            {
                var go = S<GestionAlmacenes>(o);
                var gd = S<GestionAlmacenes>(d);
                var destinoAlm = (await gd.ListarAlmacenesAsync(destino, ct).ConfigureAwait(false)).ToDictionary(a => Clave(a.Codigo), a => a.Id, StringComparer.Ordinal);
                foreach (var a in (await go.ListarAlmacenesAsync(eo, ct).ConfigureAwait(false)).Where(a => a.Activo))
                {
                    var ubicaciones = await go.ListarUbicacionesAsync(eo, a.Id, ct).ConfigureAwait(false);
                    Guid? idDestino = destinoAlm.TryGetValue(Clave(a.Codigo), out var existente) ? existente : null;
                    if (idDestino is null)
                    {
                        var nombre = $"{a.Codigo} · {a.Nombre}";
                        if (!ejecutar)
                        {
                            r.Nuevos.Add(nombre);
                            r.Nuevos.AddRange(ubicaciones.Select(u => $"{a.Codigo} / {u.Codigo}"));
                            continue;
                        }

                        var creado = await gd.CrearAlmacenAsync(destino, new CrearAlmacenComando(a.Codigo, a.Nombre), ct).ConfigureAwait(false);
                        r.Anotar(nombre, creado);
                        if (!creado.EsCorrecto) continue;
                        idDestino = creado.Valor.Id;
                    }
                    else
                    {
                        r.YaExistian++;
                    }

                    // Las ubicaciones que falten en el almacén (nuevo o que ya existía).
                    var hayUb = (await gd.ListarUbicacionesAsync(destino, idDestino, ct).ConfigureAwait(false)).Select(u => Clave(u.Codigo)).ToHashSet(StringComparer.Ordinal);
                    foreach (var u in ubicaciones.Where(u => !hayUb.Contains(Clave(u.Codigo))))
                    {
                        var nu = $"{a.Codigo} / {u.Codigo}";
                        if (!ejecutar) { r.Nuevos.Add(nu); continue; }
                        r.Anotar(nu, await gd.CrearUbicacionAsync(destino, new CrearUbicacionComando(idDestino.Value, u.Codigo, u.Nombre), ct).ConfigureAwait(false));
                    }
                }

                break;
            }

            case "transporte":
            {
                var to = S<GestionTransporte>(o);
                var td = S<GestionTransporte>(d);
                var destinoTr = (await td.TransportistasAsync(destino, ct).ConfigureAwait(false)).GroupBy(t => Clave(t.Nombre)).ToDictionary(g => g.Key, g => g.First().Id, StringComparer.Ordinal);
                var origenTr = (await to.TransportistasAsync(eo, ct).ConfigureAwait(false)).Where(t => t.Activo).ToList();
                var mapa = new Dictionary<Guid, Guid>();
                foreach (var t in origenTr)
                {
                    if (destinoTr.TryGetValue(Clave(t.Nombre), out var ya)) { r.YaExistian++; mapa[t.Id] = ya; continue; }
                    if (!ejecutar) { r.Nuevos.Add(t.Nombre); continue; }
                    var creado = await td.CrearTransportistaAsync(destino, new DatosTransportista(t.Nombre, t.Nif, t.Direccion, t.Pais, t.Telefono), ct).ConfigureAwait(false);
                    r.Anotar(t.Nombre, creado);
                    if (creado.EsCorrecto) mapa[t.Id] = creado.Valor.Id;
                }

                var hayV = (await td.VehiculosAsync(destino, ct).ConfigureAwait(false)).Select(v => Clave(v.Matricula)).ToHashSet(StringComparer.Ordinal);
                foreach (var v in (await to.VehiculosAsync(eo, ct).ConfigureAwait(false)).Where(v => v.Activo))
                {
                    if (hayV.Contains(Clave(v.Matricula))) { r.YaExistian++; continue; }
                    var nombre = $"Vehículo {v.Matricula}";
                    if (!ejecutar) { r.Nuevos.Add(nombre); continue; }
                    Guid? tr = v.TransportistaId is { } x && mapa.TryGetValue(x, out var nuevo) ? nuevo : null;
                    r.Anotar(nombre, await td.CrearVehiculoAsync(destino, new DatosVehiculo(v.Matricula, v.MatriculaRemolque, v.Descripcion, v.TaraKg, v.Frigorifico, tr), ct).ConfigureAwait(false));
                }

                break;
            }

            case "soportes":
            {
                var hay = (await S<MaestrosLogistica>(d).SoportesAsync(destino, ct).ConfigureAwait(false)).Select(x => Clave(x.Codigo)).ToHashSet(StringComparer.Ordinal);
                foreach (var x in (await S<MaestrosLogistica>(o).SoportesAsync(eo, ct).ConfigureAwait(false)).Where(x => x.Activo))
                {
                    if (hay.Contains(Clave(x.Codigo))) { r.YaExistian++; continue; }
                    var nombre = $"{x.Codigo} · {x.Nombre}";
                    if (!ejecutar) { r.Nuevos.Add(nombre); continue; }
                    // El envase es un artículo: fuera del grupo no existe en el destino.
                    var envase = origen.MismoGrupo ? x.EnvaseProductoId : null;
                    r.Anotar(nombre, await S<MaestrosLogistica>(d).CrearSoporteAsync(destino,
                        new DatosSoporte(x.Codigo, x.Nombre, x.LargoMm, x.AnchoMm, x.AltoMm, x.TaraKg, x.CargaMaxKg, envase), ct).ConfigureAwait(false));
                }

                break;
            }

            case "agro_campanas":
            {
                var hay = (await S<MaestrosAgro>(d).CampanasAsync(destino, ct).ConfigureAwait(false)).Select(x => Clave(x.Codigo)).ToHashSet(StringComparer.Ordinal);
                foreach (var x in await S<MaestrosAgro>(o).CampanasAsync(eo, ct).ConfigureAwait(false))
                {
                    if (hay.Contains(Clave(x.Codigo))) { r.YaExistian++; continue; }
                    var nombre = $"{x.Codigo} · {x.Nombre}";
                    if (!ejecutar) { r.Nuevos.Add(nombre); continue; }
                    r.Anotar(nombre, await S<MaestrosAgro>(d).CrearCampanaAsync(destino, new DatosCampana(x.Codigo, x.Nombre, x.Desde, x.Hasta), ct).ConfigureAwait(false));
                }

                break;
            }

            case "agro_categorias":
            {
                var hay = (await S<MaestrosAgro>(d).CategoriasAsync(destino, ct).ConfigureAwait(false)).Select(x => Clave(x.Codigo)).ToHashSet(StringComparer.Ordinal);
                foreach (var x in await S<MaestrosAgro>(o).CategoriasAsync(eo, ct).ConfigureAwait(false))
                {
                    if (hay.Contains(Clave(x.Codigo))) { r.YaExistian++; continue; }
                    var nombre = $"{x.Codigo} · {x.Nombre}";
                    if (!ejecutar) { r.Nuevos.Add(nombre); continue; }
                    r.Anotar(nombre, await S<MaestrosAgro>(d).CrearCategoriaAsync(destino, new DatosCategoria(x.Codigo, x.Nombre, x.EsDestrio, x.Orden), ct).ConfigureAwait(false));
                }

                break;
            }

            case "agro_conceptos":
            {
                var hay = (await S<MaestrosAgro>(d).ConceptosAsync(destino, ct).ConfigureAwait(false)).Select(x => Clave(x.Codigo)).ToHashSet(StringComparer.Ordinal);
                foreach (var x in (await S<MaestrosAgro>(o).ConceptosAsync(eo, ct).ConfigureAwait(false)).Where(x => x.Activo))
                {
                    var nombre = $"{x.Codigo} · {x.Nombre}";
                    if (hay.Contains(Clave(x.Codigo))) { r.YaExistian++; continue; }
                    // Un concepto de un agricultor concreto es de esa empresa; uno por artículo o envase, del grupo.
                    if (x.AgricultorId is not null) { r.Omitidos.Add($"{nombre}: es de un agricultor concreto."); continue; }
                    if (!origen.MismoGrupo && (x.ProductoId is not null || x.EnvaseProductoId is not null))
                    {
                        r.Omitidos.Add($"{nombre}: va ligado a un artículo de otro grupo.");
                        continue;
                    }

                    if (!Enum.TryParse<TipoConceptoLiquidacion>(x.Tipo, out var tipo)) { r.Omitidos.Add($"{nombre}: tipo {x.Tipo} desconocido."); continue; }
                    if (!ejecutar) { r.Nuevos.Add(nombre); continue; }
                    r.Anotar(nombre, await S<MaestrosAgro>(d).CrearConceptoAsync(destino,
                        new DatosConcepto(x.Codigo, x.Nombre, tipo, x.Valor, true, null, x.ProductoId, x.EnvaseProductoId, x.Abono), ct).ConfigureAwait(false));
                }

                break;
            }

            case "agro_taras":
            {
                var hoy = DateOnly.FromDateTime(DateTime.Today);
                static bool Vigente(TaraDto t, DateOnly dia) => t.Desde <= dia && (t.Hasta is null || t.Hasta >= dia);
                var hay = (await S<TarasAgro>(d).ListarAsync(destino, null, ct).ConfigureAwait(false)).Where(t => Vigente(t, hoy)).Select(t => t.EnvaseProductoId).ToHashSet();
                var productos = S<AlxorCore.Catalogo.Aplicacion.IConsultaProductos>(d);
                foreach (var t in (await S<TarasAgro>(o).ListarAsync(eo, null, ct).ConfigureAwait(false)).Where(t => Vigente(t, hoy)))
                {
                    var envase = (await productos.ObtenerAsync(t.EnvaseProductoId, ct).ConfigureAwait(false))?.Nombre ?? "Envase";
                    var nombre = $"{envase} · {AlxorCore.Nucleo.Comun.Redondeo.Formatear(t.TaraKg)} kg";
                    if (hay.Contains(t.EnvaseProductoId)) { r.YaExistian++; continue; }
                    if (!ejecutar) { r.Nuevos.Add(nombre); continue; }
                    r.Anotar(nombre, await S<TarasAgro>(d).CrearAsync(destino, new DatosTara(t.EnvaseProductoId, t.TaraKg, t.Desde, t.Hasta, t.Observaciones), ct).ConfigureAwait(false));
                }

                break;
            }
        }
    }
}
