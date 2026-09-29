using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Logistica.Dominio;

public enum TipoUnidadLogistica
{
    Pale,
    Caja,
    Contenedor,
}

/// <summary>Abierta (se monta) ⇄ cerrada (lista) → expedida; o anulada (desmontada).</summary>
public enum EstadoUnidadLogistica
{
    Abierta,
    Cerrada,
    Expedida,
    Anulada,
}

public enum OrigenUnidadLogistica
{
    Almacen,
    Fabricacion,
    Manual,
}

/// <summary>Contenido de una unidad: un artículo de un lote, en cajas y unidades, con su peso.</summary>
public sealed class LineaUnidadLogistica
{
    private LineaUnidadLogistica()
    {
    }

    internal LineaUnidadLogistica(Guid productoId, string? lote, DateOnly? caducidad, int cajas, decimal unidades, decimal pesoNeto, decimal pesoBruto)
    {
        Id = Guid.NewGuid();
        ProductoId = productoId;
        Lote = lote;
        FechaCaducidad = caducidad;
        Cajas = cajas;
        Unidades = unidades;
        PesoNetoKg = pesoNeto;
        PesoBrutoKg = pesoBruto;
    }

    public Guid Id { get; private set; }

    public Guid ProductoId { get; private set; }

    public string? Lote { get; private set; }

    public DateOnly? FechaCaducidad { get; private set; }

    public int Cajas { get; private set; }

    /// <summary>Unidades del artículo (en su unidad de inventario).</summary>
    public decimal Unidades { get; private set; }

    public decimal PesoNetoKg { get; private set; }

    /// <summary>Mercancía con su embalaje (cajas), sin el soporte.</summary>
    public decimal PesoBrutoKg { get; private set; }

    internal void Sumar(int cajas, decimal unidades, decimal neto, decimal bruto)
    {
        Cajas += cajas;
        Unidades = Math.Round(Unidades + unidades, 4);
        PesoNetoKg = Math.Round(PesoNetoKg + neto, 3);
        PesoBrutoKg = Math.Round(PesoBrutoKg + bruto, 3);
    }
}

/// <summary>Lo que se pone en una unidad logística.</summary>
public sealed record ContenidoUnidad(Guid ProductoId, string? Lote, DateOnly? FechaCaducidad, int Cajas, decimal Unidades, decimal PesoNetoKg, decimal PesoBrutoKg);

/// <summary>
/// Unidad logística GS1: un palé, una caja o un contenedor con su SSCC. Lleva su contenido (artículo, lote, caducidad,
/// cajas, unidades y pesos) y, si es multinivel, otras unidades dentro (las cajas con SSCC de un palé mixto: se enlazan
/// con <see cref="PadreId"/>). Su peso bruto es el de su contenido más la tara del soporte; su altura, la del mosaico.
/// Montarla no mueve existencias: agrupa las que hay en su almacén. Sale del almacén al expedirse.
/// </summary>
public sealed class UnidadLogistica : RaizAgregadoEmpresa<Guid>
{
    private readonly List<LineaUnidadLogistica> _contenido = [];

    private UnidadLogistica(Guid id)
        : base(id, Guid.Empty)
    {
        Sscc = null!;
    }

    private UnidadLogistica(Guid id, Guid empresaId, string sscc)
        : base(id, empresaId)
    {
        Sscc = sscc;
    }

    public string Sscc { get; private set; }

    public TipoUnidadLogistica Tipo { get; private set; }

    public EstadoUnidadLogistica Estado { get; private set; }

    public OrigenUnidadLogistica Origen { get; private set; }

    public Guid? SoporteId { get; private set; }

    public decimal TaraKg { get; private set; }

    /// <summary>Unidad que la contiene (una caja dentro de un palé).</summary>
    public Guid? PadreId { get; private set; }

    public Guid AlmacenId { get; private set; }

    public Guid? UbicacionId { get; private set; }

    public Guid? ClienteId { get; private set; }

    public Guid? PedidoVentaId { get; private set; }

    public Guid? OrdenFabricacionId { get; private set; }

    public Guid? PlantillaId { get; private set; }

    /// <summary>Cajas que caben (las de la plantilla o la ficha): al llenarse, la unidad se cierra sola.</summary>
    public int? CajasCompleta { get; private set; }

    public int? AlturaMm { get; private set; }

    public decimal PesoBrutoKg { get; private set; }

    public string? Observaciones { get; private set; }

    public string? MotivoAnulacion { get; private set; }

    public DateTimeOffset CreadaEn { get; private set; }

    public DateTimeOffset? CerradaEn { get; private set; }

    public DateOnly? FechaExpedicion { get; private set; }

    public string? ReferenciaExpedicion { get; private set; }

    public IReadOnlyList<LineaUnidadLogistica> Contenido => _contenido;

    public int Cajas => _contenido.Sum(l => l.Cajas);

    public decimal PesoNetoKg => _contenido.Sum(l => l.PesoNetoKg);

    public bool Completa => CajasCompleta is { } c && Cajas >= c;

    public bool Viva => Estado is EstadoUnidadLogistica.Abierta or EstadoUnidadLogistica.Cerrada;

    public static UnidadLogistica Crear(Guid empresaId, string sscc, TipoUnidadLogistica tipo, OrigenUnidadLogistica origen, Guid almacenId, Guid? ubicacionId,
        TipoSoporte? soporte, IReloj reloj, Guid? plantillaId = null, int? cajasCompleta = null, Guid? clienteId = null, Guid? pedidoVentaId = null,
        Guid? ordenFabricacionId = null, string? observaciones = null)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        var u = new UnidadLogistica(Guid.NewGuid(), empresaId, sscc)
        {
            Tipo = tipo, Origen = origen, AlmacenId = almacenId, UbicacionId = ubicacionId, SoporteId = soporte?.Id, TaraKg = soporte?.TaraKg ?? 0m,
            PlantillaId = plantillaId, CajasCompleta = cajasCompleta, ClienteId = clienteId, PedidoVentaId = pedidoVentaId, OrdenFabricacionId = ordenFabricacionId,
            Observaciones = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim()[..Math.Min(observaciones.Trim().Length, 500)],
            Estado = EstadoUnidadLogistica.Abierta, CreadaEn = reloj.AhoraUtc,
        };
        u.PesoBrutoKg = u.TaraKg;
        return u;
    }

    /// <summary>Añade contenido (en negativo, lo quita). Solo en una unidad abierta; al completarse se cierra sola.</summary>
    public Resultado Poner(ContenidoUnidad c, int? alturaMm, IReloj reloj, bool cerrarAlCompletar = true)
    {
        ArgumentNullException.ThrowIfNull(c);
        if (Estado != EstadoUnidadLogistica.Abierta)
        {
            return Resultado.Fallo(Error.Conflicto("unidad.no_abierta", $"La unidad {Sscc} está {Estado.ToString().ToLowerInvariant()}: ábrela para cambiar su contenido."));
        }

        if (c.Cajas == 0 && c.Unidades == 0m)
        {
            return Resultado.Fallo(Error.Validacion("unidad.cantidad", "Indica las cajas o las unidades."));
        }

        var lote = string.IsNullOrWhiteSpace(c.Lote) ? null : c.Lote.Trim();
        var linea = _contenido.FirstOrDefault(l => l.ProductoId == c.ProductoId && l.Lote == lote);
        if (c.Cajas < 0 || c.Unidades < 0m)
        {
            if (linea is null || linea.Cajas + c.Cajas < 0 || linea.Unidades + c.Unidades < 0m)
            {
                return Resultado.Fallo(Error.Conflicto("unidad.sin_contenido", "No hay tanto de ese artículo y lote en la unidad."));
            }
        }

        if (CajasCompleta is { } maximo && c.Cajas > 0 && Cajas + c.Cajas > maximo)
        {
            return Resultado.Fallo(Error.Conflicto("unidad.llena", $"No caben: la unidad lleva {Cajas} cajas de {maximo}."));
        }

        if (linea is null)
        {
            _contenido.Add(new LineaUnidadLogistica(c.ProductoId, lote, c.FechaCaducidad, c.Cajas, c.Unidades, c.PesoNetoKg, c.PesoBrutoKg));
        }
        else
        {
            linea.Sumar(c.Cajas, c.Unidades, c.PesoNetoKg, c.PesoBrutoKg);
            if (linea.Cajas == 0 && linea.Unidades == 0m)
            {
                _contenido.Remove(linea);
            }
        }

        Recalcular(alturaMm);
        if (cerrarAlCompletar && Completa)
        {
            Estado = EstadoUnidadLogistica.Cerrada;
            CerradaEn = reloj.AhoraUtc;
        }

        return Resultado.Ok();
    }

    /// <summary>Peso bruto de las unidades hijas (cajas con SSCC dentro), que suma al de esta.</summary>
    public void FijarPesoHijas(decimal pesoHijas) => PesoBrutoKg = Math.Round(TaraKg + _contenido.Sum(l => l.PesoBrutoKg) + pesoHijas, 3);

    private void Recalcular(int? alturaMm)
    {
        PesoBrutoKg = Math.Round(TaraKg + _contenido.Sum(l => l.PesoBrutoKg), 3);
        AlturaMm = alturaMm ?? AlturaMm;
    }

    public Resultado Cerrar(IReloj reloj, bool tieneHijas = false)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (Estado != EstadoUnidadLogistica.Abierta)
        {
            return Resultado.Fallo(Error.Conflicto("unidad.no_abierta", $"La unidad {Sscc} no está abierta."));
        }

        if (_contenido.Count == 0 && !tieneHijas)
        {
            return Resultado.Fallo(Error.Validacion("unidad.vacia", "Una unidad vacía no se cierra: anúlala."));
        }

        Estado = EstadoUnidadLogistica.Cerrada;
        CerradaEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    public Resultado Abrir()
    {
        if (Estado != EstadoUnidadLogistica.Cerrada)
        {
            return Resultado.Fallo(Error.Conflicto("unidad.no_cerrada", $"La unidad {Sscc} no está cerrada."));
        }

        Estado = EstadoUnidadLogistica.Abierta;
        CerradaEn = null;
        return Resultado.Ok();
    }

    public Resultado Anular(string? motivo)
    {
        if (!Viva)
        {
            return Resultado.Fallo(Error.Conflicto("unidad.no_viva", $"La unidad {Sscc} ya está {Estado.ToString().ToLowerInvariant()}."));
        }

        Estado = EstadoUnidadLogistica.Anulada;
        MotivoAnulacion = string.IsNullOrWhiteSpace(motivo) ? "Desmontada" : motivo.Trim()[..Math.Min(motivo.Trim().Length, 200)];
        PadreId = null;
        return Resultado.Ok();
    }

    public Resultado Mover(Guid almacenId, Guid? ubicacionId)
    {
        if (!Viva)
        {
            return Resultado.Fallo(Error.Conflicto("unidad.no_viva", $"La unidad {Sscc} no está en el almacén."));
        }

        AlmacenId = almacenId;
        UbicacionId = ubicacionId;
        return Resultado.Ok();
    }

    public Resultado AsignarPedido(Guid? clienteId, Guid? pedidoVentaId)
    {
        if (!Viva)
        {
            return Resultado.Fallo(Error.Conflicto("unidad.no_viva", $"La unidad {Sscc} no se puede asignar."));
        }

        ClienteId = clienteId;
        PedidoVentaId = pedidoVentaId;
        return Resultado.Ok();
    }

    /// <summary>Mete esta unidad dentro de otra (una caja con SSCC en un palé) o la saca (null).</summary>
    public Resultado MeterEn(UnidadLogistica? padre)
    {
        if (!Viva)
        {
            return Resultado.Fallo(Error.Conflicto("unidad.no_viva", $"La unidad {Sscc} no está viva."));
        }

        if (padre is not null)
        {
            if (padre.Id == Id || padre.PadreId == Id)
            {
                return Resultado.Fallo(Error.Validacion("unidad.ciclo", "Una unidad no puede ir dentro de sí misma."));
            }

            if (padre.Estado != EstadoUnidadLogistica.Abierta)
            {
                return Resultado.Fallo(Error.Conflicto("unidad.no_abierta", $"La unidad {padre.Sscc} no está abierta."));
            }

            if (padre.AlmacenId != AlmacenId)
            {
                return Resultado.Fallo(Error.Conflicto("unidad.otro_almacen", "Las dos unidades tienen que estar en el mismo almacén."));
            }
        }

        PadreId = padre?.Id;
        return Resultado.Ok();
    }

    /// <summary>La unidad es de lo fabricado en esa orden.</summary>
    public void MarcarFabricacion(Guid ordenFabricacionId)
    {
        OrdenFabricacionId = ordenFabricacionId;
        Origen = OrigenUnidadLogistica.Fabricacion;
    }

    public Resultado Expedir(DateOnly fecha, string? referencia)
    {
        if (Estado != EstadoUnidadLogistica.Cerrada)
        {
            return Resultado.Fallo(Error.Conflicto("unidad.no_cerrada", $"Solo se expide una unidad cerrada ({Sscc} está {Estado.ToString().ToLowerInvariant()})."));
        }

        // Nada sale antes de existir: ni antes de montar la unidad ni antes de que se cerrara.
        var desde = DateOnly.FromDateTime((CerradaEn ?? CreadaEn).UtcDateTime);
        if (fecha < desde)
        {
            return Resultado.Fallo(Error.Conflicto("unidad.fecha_anterior", $"La unidad {Sscc} se montó el {desde:dd/MM/yyyy}: no puede salir el {fecha:dd/MM/yyyy}."));
        }

        Estado = EstadoUnidadLogistica.Expedida;
        FechaExpedicion = fecha;
        ReferenciaExpedicion = string.IsNullOrWhiteSpace(referencia) ? null : referencia.Trim()[..Math.Min(referencia.Trim().Length, 60)];
        return Resultado.Ok();
    }
}

/// <summary>Mosaico con el que se calcula un palé: cajas por capa, capas y soporte, con sus límites.</summary>
public sealed record Mosaico(int CajasPorCapa, int Capas, TipoSoporte? Soporte, int? AlturaMaxMm, decimal? PesoMaxKg, Guid? PlantillaId, string Origen)
{
    public int CajasPorPale => CajasPorCapa * Capas;
}

/// <summary>Un palé calculado: sus cajas, peso bruto y altura.</summary>
public sealed record PaleCalculado(int Cajas, int CapasCompletas, int CajasCapaIncompleta, decimal PesoNetoKg, decimal PesoBrutoKg, int? AlturaMm);

/// <summary>Cálculo del paletizado de una cantidad: palés completos, el pico y los avisos de límites.</summary>
public sealed record CalculoPaletizado(int CajasTotales, decimal UnidadesTotales, int CajasPorPale, int PalesCompletos, PaleCalculado? PaleCompleto, PaleCalculado? Pico,
    int Pales, decimal PesoBrutoTotalKg, IReadOnlyList<string> Avisos);

/// <summary>Cálculos de paletizado a partir de la ficha logística y el mosaico (puros: sin base de datos).</summary>
public static class CalculadoraPaletizado
{
    /// <summary>Cajas que son esas unidades del artículo (redondeando hacia arriba: la última caja puede ir incompleta).</summary>
    public static int Cajas(FichaLogistica ficha, decimal unidades)
    {
        ArgumentNullException.ThrowIfNull(ficha);
        return (int)Math.Ceiling(unidades / ficha.UnidadesPorCaja);
    }

    public static PaleCalculado Pale(FichaLogistica ficha, Mosaico mosaico, int cajas)
    {
        ArgumentNullException.ThrowIfNull(ficha);
        ArgumentNullException.ThrowIfNull(mosaico);
        var capas = cajas / mosaico.CajasPorCapa;
        var resto = cajas % mosaico.CajasPorCapa;
        var neto = ficha.PesoNetoCajaKg is { } n ? Math.Round(n * cajas, 3) : 0m;
        var bruto = Math.Round((ficha.PesoBrutoCajaKg ?? ficha.PesoNetoCajaKg ?? 0m) * cajas + (mosaico.Soporte?.TaraKg ?? 0m), 3);
        int? altura = ficha.AltoCajaMm is { } alto ? (mosaico.Soporte?.AltoMm ?? 0) + alto * (capas + (resto > 0 ? 1 : 0)) : null;
        return new PaleCalculado(cajas, capas, resto, neto, bruto, altura);
    }

    public static CalculoPaletizado Calcular(FichaLogistica ficha, Mosaico mosaico, int cajas)
    {
        ArgumentNullException.ThrowIfNull(ficha);
        ArgumentNullException.ThrowIfNull(mosaico);
        var porPale = mosaico.CajasPorPale;
        var completos = cajas / porPale;
        var resto = cajas % porPale;
        var completo = completos > 0 || cajas == 0 ? Pale(ficha, mosaico, porPale) : null;
        var pico = resto > 0 ? Pale(ficha, mosaico, resto) : null;
        var avisos = new List<string>();
        var referencia = Pale(ficha, mosaico, porPale);
        var alturaMax = mosaico.AlturaMaxMm ?? ficha.AlturaMaxPaleMm;
        var pesoMax = mosaico.PesoMaxKg ?? ficha.PesoMaxPaleKg ?? mosaico.Soporte?.CargaMaxKg + mosaico.Soporte?.TaraKg;
        if (alturaMax is { } am && referencia.AlturaMm is { } a && a > am)
        {
            avisos.Add($"El palé completo mide {a} mm y el máximo es {am} mm.");
        }

        if (pesoMax is { } pm && referencia.PesoBrutoKg > pm)
        {
            avisos.Add($"El palé completo pesa {referencia.PesoBrutoKg:0.#} kg y el máximo es {pm:0.#} kg.");
        }

        if (ficha.AltoCajaMm is null || ficha.PesoBrutoCajaKg is null)
        {
            avisos.Add("La ficha logística no tiene la altura o el peso de la caja: la altura o el peso del palé son aproximados.");
        }

        var palets = completos + (pico is null ? 0 : 1);
        var peso = (completo?.PesoBrutoKg ?? 0m) * completos + (pico?.PesoBrutoKg ?? 0m);
        return new CalculoPaletizado(cajas, cajas * (decimal)ficha.UnidadesPorCaja, porPale, completos, completos > 0 ? completo : null, pico, palets, Math.Round(peso, 3), avisos);
    }
}
