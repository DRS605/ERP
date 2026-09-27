using AlxorCore.Catalogo.Dominio;
using AlxorCore.Nucleo.Comun;

namespace AlxorCore.Catalogo.Aplicacion;

/// <summary>
/// Recorre las listas de materiales de varios niveles (un compuesto puede llevar otros compuestos): el árbol con su
/// coste, precio y peso calculados de abajo arriba, la explosión en materiales básicos, el control de ciclos y la
/// búsqueda inversa (dónde se usa un artículo, y qué compuesto tiene ya una misma lista). Guarda en memoria los
/// artículos que lee, así cada uno se consulta una vez por operación.
/// </summary>
public sealed class ArbolComposiciones
{
    /// <summary>Profundidad máxima de un árbol de composición.</summary>
    public const int MaximoNiveles = 10;

    private readonly IRepositorioProductos _productos;
    private readonly IConsultaProductos _consulta;
    private readonly Dictionary<Guid, Producto?> _entidades = [];
    private readonly Dictionary<Guid, ProductoDto?> _vistas = [];

    public ArbolComposiciones(IRepositorioProductos productos, IConsultaProductos consulta)
    {
        _productos = productos;
        _consulta = consulta;
    }

    private async Task<Producto?> ProductoAsync(Guid id, CancellationToken ct)
    {
        if (!_entidades.TryGetValue(id, out var p))
        {
            p = await _productos.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
            _entidades[id] = p;
        }

        return p;
    }

    private async Task<ProductoDto?> VistaAsync(Guid id, CancellationToken ct)
    {
        if (!_vistas.TryGetValue(id, out var v))
        {
            v = await _consulta.ObtenerAsync(id, ct).ConfigureAwait(false);
            _vistas[id] = v;
        }

        return v;
    }

    /// <summary>¿Alguno de los componentes contiene ya (en cualquier nivel) al propio artículo?</summary>
    public async Task<bool> CreariaCicloAsync(Guid productoId, IEnumerable<Guid> componentes, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(componentes);
        var pendientes = new Queue<(Guid Id, int Nivel)>(componentes.Select(c => (c, 1)));
        var vistos = new HashSet<Guid>();
        while (pendientes.Count > 0)
        {
            var (id, nivel) = pendientes.Dequeue();
            if (id == productoId)
            {
                return true;
            }

            if (!vistos.Add(id) || nivel > MaximoNiveles)
            {
                continue;
            }

            if (await ProductoAsync(id, ct).ConfigureAwait(false) is { EsCompuesto: true } p)
            {
                foreach (var c in p.Componentes)
                {
                    pendientes.Enqueue((c.ComponenteId, nivel + 1));
                }
            }
        }

        return false;
    }

    /// <summary>Número de niveles que tendría el árbol con estos componentes.</summary>
    public async Task<int> NivelesAsync(IEnumerable<Guid> componentes, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(componentes);
        var maximo = 1;
        foreach (var id in componentes.Distinct())
        {
            if (await ProductoAsync(id, ct).ConfigureAwait(false) is { EsCompuesto: true } p)
            {
                maximo = Math.Max(maximo, 1 + (await NodoAsync(p, [p.Id], ct).ConfigureAwait(false)).Niveles);
            }
        }

        return maximo;
    }

    private sealed record Nodo(IReadOnlyList<ComponenteDto> Componentes, decimal Coste, decimal Precio, decimal? Peso, int Niveles);

    private async Task<Nodo> NodoAsync(Producto p, HashSet<Guid> camino, CancellationToken ct)
    {
        var lineas = new List<ComponenteDto>();
        decimal coste = 0m, precio = 0m, peso = 0m;
        var hayPeso = false;
        var niveles = p.Componentes.Count == 0 ? 0 : 1;
        foreach (var c in p.Componentes)
        {
            var vista = await VistaAsync(c.ComponenteId, ct).ConfigureAwait(false);
            var entidad = await ProductoAsync(c.ComponenteId, ct).ConfigureAwait(false);
            var costeUnitario = vista?.PrecioCompra ?? 0m;
            var pesoUnitario = vista?.PesoKg;
            IReadOnlyList<ComponenteDto>? hijos = null;
            if (entidad is { EsCompuesto: true } && camino.Count < MaximoNiveles && camino.Add(entidad.Id))
            {
                var sub = await NodoAsync(entidad, camino, ct).ConfigureAwait(false);
                camino.Remove(entidad.Id);
                hijos = sub.Componentes;
                costeUnitario = sub.Coste > 0m ? sub.Coste : costeUnitario;
                pesoUnitario = sub.Peso ?? pesoUnitario;
                niveles = Math.Max(niveles, sub.Niveles + 1);
            }

            var costeLinea = Redondeo.Dos(costeUnitario * c.Cantidad);
            coste += costeLinea;
            precio += (vista?.PrecioUnitario ?? 0m) * c.Cantidad;
            if (pesoUnitario is { } pu)
            {
                peso += pu * c.Cantidad;
                hayPeso = true;
            }

            lineas.Add(new ComponenteDto(c.ComponenteId, vista?.Nombre ?? "(desconocido)", c.Cantidad, vista?.Unidad ?? "ud", costeUnitario, costeLinea,
                entidad?.EsCompuesto ?? false, entidad is { EsCompuesto: true } ? entidad.Composicion.ToString() : null, vista?.PrecioUnitario ?? 0m,
                pesoUnitario, hijos));
        }

        return new Nodo(lineas, Redondeo.Dos(coste), Redondeo.Dos(precio), hayPeso ? decimal.Round(peso, 3, MidpointRounding.AwayFromZero) : null, niveles);
    }

    /// <summary>Precio de venta calculado desde los componentes, con el ajuste del compuesto.</summary>
    public async Task<decimal> PrecioSegunComponentesAsync(Producto p, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(p);
        var nodo = await NodoAsync(p, [p.Id], ct).ConfigureAwait(false);
        return Redondeo.Dos(nodo.Precio * (1m + (p.AjustePrecioComponentes / 100m)));
    }

    /// <summary>
    /// Cantidades de cada artículo necesarias para <paramref name="cantidad"/> unidades del compuesto. Con
    /// <paramref name="soloKits"/> baja solo por los kits (lo que se descuenta al vender: un compuesto de fabricación
    /// tiene sus propias existencias); sin él, baja hasta los materiales básicos.
    /// </summary>
    public async Task<IReadOnlyList<(Guid ProductoId, decimal Cantidad)>> ExplosionAsync(Producto p, decimal cantidad, bool soloKits, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(p);
        var acumulado = new Dictionary<Guid, decimal>();
        await ExplotarAsync(p, cantidad, soloKits, [p.Id], acumulado, ct).ConfigureAwait(false);
        return acumulado.Select(x => (x.Key, decimal.Round(x.Value, 3, MidpointRounding.AwayFromZero))).ToList();
    }

    private async Task ExplotarAsync(Producto p, decimal cantidad, bool soloKits, HashSet<Guid> camino, Dictionary<Guid, decimal> acumulado, CancellationToken ct)
    {
        foreach (var c in p.Componentes)
        {
            var hijo = await ProductoAsync(c.ComponenteId, ct).ConfigureAwait(false);
            var necesaria = c.Cantidad * cantidad;
            var bajar = hijo is { EsCompuesto: true } && (!soloKits || hijo.Composicion == TipoComposicion.Kit)
                        && camino.Count < MaximoNiveles && !camino.Contains(hijo.Id);
            if (bajar)
            {
                camino.Add(hijo!.Id);
                await ExplotarAsync(hijo, necesaria, soloKits, camino, acumulado, ct).ConfigureAwait(false);
                camino.Remove(hijo.Id);
            }
            else
            {
                acumulado[c.ComponenteId] = acumulado.GetValueOrDefault(c.ComponenteId) + necesaria;
            }
        }
    }

    /// <summary>Escandallo completo del artículo (ver <see cref="ComposicionDto"/>).</summary>
    public async Task<ComposicionDto> ComposicionAsync(Producto p, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(p);
        if (!p.EsCompuesto)
        {
            return new ComposicionDto(p.Id, false, 0m, [], PesoKg: p.PesoKg);
        }

        var nodo = await NodoAsync(p, [p.Id], ct).ConfigureAwait(false);
        var explosion = new List<NecesidadDto>();
        foreach (var (id, cantidad) in await ExplosionAsync(p, 1m, false, ct).ConfigureAwait(false))
        {
            var v = await VistaAsync(id, ct).ConfigureAwait(false);
            explosion.Add(new NecesidadDto(id, v?.Nombre ?? "(desconocido)", v?.Unidad ?? "ud", cantidad, v?.ControlarStock ?? false, v?.Stock ?? 0m));
        }

        decimal? disponible = null;
        if (p.Composicion == TipoComposicion.Kit)
        {
            foreach (var (id, cantidad) in await ExplosionAsync(p, 1m, true, ct).ConfigureAwait(false))
            {
                if (await VistaAsync(id, ct).ConfigureAwait(false) is { ControlarStock: true } v && cantidad > 0m)
                {
                    var caben = Math.Floor(Math.Max(v.Stock, 0m) / cantidad);
                    disponible = disponible is null ? caben : Math.Min(disponible.Value, caben);
                }
            }
        }

        var iguales = (await IgualesAsync(p.GrupoId, p.Componentes.Select(c => (c.ComponenteId, c.Cantidad)).ToList(), ct).ConfigureAwait(false))
            .Where(x => x.Id != p.Id).Select(x => x.Nombre).ToList();
        return new ComposicionDto(p.Id, true, nodo.Coste, nodo.Componentes, p.Composicion.ToString(), p.PrecioSegunComponentes, p.AjustePrecioComponentes,
            nodo.Precio, p.PrecioSegunComponentes ? Redondeo.Dos(nodo.Precio * (1m + (p.AjustePrecioComponentes / 100m))) : null, nodo.Peso ?? p.PesoKg,
            nodo.Niveles, explosion.OrderBy(x => x.Nombre, StringComparer.CurrentCulture).ToList(), disponible, iguales);
    }

    /// <summary>Compuestos del grupo con exactamente esta lista de materiales (mismos componentes y cantidades).</summary>
    public async Task<IReadOnlyList<(Guid Id, string Nombre)>> IgualesAsync(Guid grupoId, IReadOnlyList<(Guid ComponenteId, decimal Cantidad)> componentes,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(componentes);
        if (componentes.Count == 0)
        {
            return [];
        }

        var buscada = componentes.OrderBy(c => c.ComponenteId).Select(c => (c.ComponenteId, decimal.Round(c.Cantidad, 3))).ToList();
        return (await _productos.CompuestosAsync(grupoId, ct).ConfigureAwait(false))
            .Where(x => x.Componentes.Count == buscada.Count
                        && x.Componentes.OrderBy(c => c.ComponenteId).Select(c => (c.ComponenteId, decimal.Round(c.Cantidad, 3))).SequenceEqual(buscada))
            .Select(x => (x.Id, x.Nombre)).ToList();
    }

    /// <summary>Dónde se usa un artículo: los compuestos que lo llevan, directamente o dentro de otro compuesto.</summary>
    public async Task<IReadOnlyList<UsoComponenteDto>> UsosAsync(Guid componenteId, CancellationToken ct = default)
    {
        var usos = new List<UsoComponenteDto>();
        var pendientes = new Queue<(Guid Id, int Nivel, decimal Factor)>([(componenteId, 0, 1m)]);
        var vistos = new HashSet<Guid> { componenteId };
        while (pendientes.Count > 0)
        {
            var (id, nivel, factor) = pendientes.Dequeue();
            if (nivel >= MaximoNiveles)
            {
                continue;
            }

            foreach (var padre in await _productos.CompuestosConComponenteAsync(id, ct).ConfigureAwait(false))
            {
                var cantidad = factor * padre.Componentes.Where(c => c.ComponenteId == id).Sum(c => c.Cantidad);
                usos.Add(new UsoComponenteDto(padre.Id, padre.Nombre, padre.Composicion.ToString(), nivel + 1, decimal.Round(cantidad, 3, MidpointRounding.AwayFromZero)));
                if (vistos.Add(padre.Id))
                {
                    pendientes.Enqueue((padre.Id, nivel + 1, cantidad));
                }
            }
        }

        return usos.OrderBy(u => u.Nivel).ThenBy(u => u.Nombre, StringComparer.CurrentCulture).ToList();
    }
}
