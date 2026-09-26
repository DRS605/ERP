using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Agro.Dominio;

/// <summary>Estado de una clasificación de partida.</summary>
public enum EstadoClasificacion
{
    /// <summary>Muestreo orientativo: se puede cambiar o descartar.</summary>
    Provisional = 1,

    /// <summary>La que cuenta para liquidar. Una por partida; no se modifica.</summary>
    Definitiva = 2,

    /// <summary>Era definitiva y la sustituyó otra (solo si la partida no está liquidada).</summary>
    Sustituida = 3,
}

/// <summary>
/// Clasificación (muestreo) de una partida: cuántos kilos de la muestra salieron de cada categoría. Los
/// kilos netos de la partida se reparten en esa proporción al liquidar.
/// </summary>
public sealed class ClasificacionPartida : RaizAgregadoEmpresa<Guid>
{
    private readonly List<LineaClasificacion> _lineas = [];

    private ClasificacionPartida(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private ClasificacionPartida(Guid id, Guid empresaId, Guid partidaId, DateOnly fecha, string? observaciones, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        PartidaId = partidaId;
        Fecha = fecha;
        Observaciones = observaciones;
        Estado = EstadoClasificacion.Provisional;
        CreadoEn = ahora;
    }

    public Guid PartidaId { get; private set; }

    public DateOnly Fecha { get; private set; }

    public EstadoClasificacion Estado { get; private set; }

    public string? Observaciones { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public IReadOnlyList<LineaClasificacion> Lineas => _lineas;

    public decimal KgMuestra => _lineas.Sum(l => l.KgMuestra);

    public static Resultado<ClasificacionPartida> Crear(Guid empresaId, Guid partidaId, DateOnly fecha, IReadOnlyList<(Guid CategoriaId, decimal KgMuestra)> lineas, string? observaciones, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(lineas);
        ArgumentNullException.ThrowIfNull(reloj);
        if (lineas.Count == 0)
        {
            return Resultado.Fallo<ClasificacionPartida>(Error.Validacion("clasificacion.sin_lineas", "Indica los kilos de la muestra de al menos una categoría."));
        }

        if (lineas.Any(l => l.KgMuestra < 0m || decimal.Round(l.KgMuestra, 3) != l.KgMuestra) || lineas.Sum(l => l.KgMuestra) <= 0m)
        {
            return Resultado.Fallo<ClasificacionPartida>(Error.Validacion("clasificacion.kilos", "Los kilos de la muestra no pueden ser negativos (hasta 3 decimales) y deben sumar más de cero."));
        }

        if (lineas.Select(l => l.CategoriaId).Distinct().Count() != lineas.Count)
        {
            return Resultado.Fallo<ClasificacionPartida>(Error.Validacion("clasificacion.categoria_repetida", "Cada categoría va una sola vez."));
        }

        var texto = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim();
        var c = new ClasificacionPartida(Guid.NewGuid(), empresaId, partidaId, fecha, texto?.Length > 200 ? texto[..200] : texto, reloj.AhoraUtc);
        c._lineas.AddRange(lineas.Where(l => l.KgMuestra > 0m).Select(l => new LineaClasificacion(Guid.NewGuid(), l.CategoriaId, l.KgMuestra)));
        return Resultado.Ok(c);
    }

    public Resultado HacerDefinitiva()
    {
        if (Estado != EstadoClasificacion.Provisional)
        {
            return Resultado.Fallo(Error.Conflicto("clasificacion.no_provisional", "Solo una clasificación provisional pasa a definitiva."));
        }

        Estado = EstadoClasificacion.Definitiva;
        return Resultado.Ok();
    }

    public void Sustituir() => Estado = EstadoClasificacion.Sustituida;
}

public sealed class LineaClasificacion : EntidadBase<Guid>
{
    private LineaClasificacion(Guid id)
        : base(id)
    {
    }

    internal LineaClasificacion(Guid id, Guid categoriaId, decimal kgMuestra)
        : base(id)
    {
        CategoriaId = categoriaId;
        KgMuestra = kgMuestra;
    }

    public Guid CategoriaId { get; private set; }

    public decimal KgMuestra { get; private set; }
}

// ---------------------------------------------------------------------------------------------- Valoración

/// <summary>Línea de recepción a liquidar.</summary>
public sealed record LineaALiquidar(Guid LineaRecepcionId, Guid RecepcionId, Guid PartidaId, string Etiqueta, Guid ProductoId, DateOnly Fecha, decimal NetoKg);

/// <summary>Muestra de una categoría en la clasificación definitiva.</summary>
public sealed record MuestraCategoria(Guid CategoriaId, string Categoria, decimal KgMuestra);

/// <summary>Precio aplicado.</summary>
public sealed record PrecioAplicable(Guid Id, decimal PrecioKg);

/// <summary>Datos de precios de la campaña que necesita la valoración.</summary>
public interface IPreciosLiquidacion
{
    MetodoLiquidacion? Metodo(Guid productoId);

    IReadOnlyList<MuestraCategoria>? Clasificacion(Guid partidaId);

    PrecioAplicable? Precio(Guid productoId, Guid? categoriaId, DateOnly fecha);
}

public sealed record LineaValorada(Guid LineaRecepcionId, Guid RecepcionId, Guid PartidaId, Guid? CategoriaId, Guid PrecioId, DateOnly Fecha, decimal Kilos, decimal PrecioKg, decimal Importe);

public sealed record DescuentoValorado(Guid ConceptoId, string Nombre, TipoConceptoLiquidacion Tipo, decimal Valor, decimal Base, decimal Importe);

public sealed record ValoracionLiquidacion(
    IReadOnlyList<LineaValorada> Lineas, IReadOnlyList<DescuentoValorado> Descuentos,
    decimal Kilos, decimal Bruto, decimal TotalDescuentos, decimal Base, decimal PorcentajeImpuesto, decimal CuotaImpuesto,
    decimal PorcentajeRetencion, decimal Retencion, decimal TotalFactura, decimal APagar);

/// <summary>
/// Valoración de una liquidación al agricultor (portada del Clon y ampliada):
/// <list type="bullet">
/// <item>bruto = Σ kilos × precio (cada línea al céntimo); por clasificación, los kilos netos de cada partida
/// se reparten por categorías según su clasificación definitiva, sin perder ni un gramo;</item>
/// <item>descuentos: por kilo, porcentaje del bruto o fijo; base = bruto − descuentos;</item>
/// <item>impuesto (IVA en régimen general o compensación en REAGP) y retención sobre la base;</item>
/// <item>total factura = base + impuesto; a pagar = total − retención.</item>
/// </list>
/// Sin precio o sin clasificación definitiva es un error, nunca un cero, y se devuelven todos a la vez.
/// </summary>
public static class Valoracion
{
    public static Resultado<ValoracionLiquidacion> Calcular(
        IReadOnlyList<LineaALiquidar> lineas, IReadOnlyList<ConceptoLiquidacion> conceptos, decimal porcentajeImpuesto, decimal porcentajeRetencion,
        IPreciosLiquidacion precios, out IReadOnlyList<Error> errores)
    {
        ArgumentNullException.ThrowIfNull(lineas);
        ArgumentNullException.ThrowIfNull(conceptos);
        ArgumentNullException.ThrowIfNull(precios);
        var lista = new List<Error>();
        var valoradas = new List<LineaValorada>();
        if (lineas.Count == 0)
        {
            lista.Add(Error.Validacion("liquidacion.sin_lineas", "No hay entregas confirmadas pendientes de liquidar en el periodo."));
        }

        foreach (var l in lineas)
        {
            if (l.NetoKg <= 0m)
            {
                lista.Add(Error.Validacion("liquidacion.kilos", $"{l.Etiqueta}: el peso neto debe ser positivo."));
                continue;
            }

            var metodo = precios.Metodo(l.ProductoId);
            if (metodo is null)
            {
                lista.Add(Error.Validacion("liquidacion.metodo", $"{l.Etiqueta}: la campaña no dice cómo se liquida este artículo (por clasificación o por periodo)."));
                continue;
            }

            if (metodo == MetodoLiquidacion.PorPeriodo)
            {
                var precio = precios.Precio(l.ProductoId, null, l.Fecha);
                if (precio is null)
                {
                    lista.Add(Error.Validacion("liquidacion.sin_precio", $"{l.Etiqueta}: no hay precio de liquidación vigente el {l.Fecha:dd/MM/yyyy}."));
                    continue;
                }

                valoradas.Add(Linea(l, null, precio, l.NetoKg));
                continue;
            }

            var muestra = precios.Clasificacion(l.PartidaId);
            if (muestra is null || muestra.Count == 0)
            {
                lista.Add(Error.Validacion("liquidacion.sin_clasificacion", $"{l.Etiqueta}: la partida no tiene clasificación definitiva."));
                continue;
            }

            var kilos = ReglasAgro.Repartir(l.NetoKg, muestra.Select(m => m.KgMuestra).ToList(), 3);
            for (var i = 0; i < muestra.Count; i++)
            {
                var precio = precios.Precio(l.ProductoId, muestra[i].CategoriaId, l.Fecha);
                if (precio is null)
                {
                    lista.Add(Error.Validacion("liquidacion.sin_precio", $"{l.Etiqueta}: no hay precio de la categoría {muestra[i].Categoria} vigente el {l.Fecha:dd/MM/yyyy}."));
                    continue;
                }

                if (kilos[i] != 0m)
                {
                    valoradas.Add(Linea(l, muestra[i].CategoriaId, precio, kilos[i]));
                }
            }
        }

        if (porcentajeImpuesto is < 0m or > 100m || porcentajeRetencion is < 0m or > 100m)
        {
            lista.Add(Error.Validacion("liquidacion.porcentajes", "El porcentaje de impuesto o de retención no es válido."));
        }

        errores = lista;
        if (lista.Count > 0)
        {
            return Resultado.Fallo<ValoracionLiquidacion>(Resumen(lista));
        }

        var totalKg = valoradas.Sum(v => v.Kilos);
        var bruto = valoradas.Sum(v => v.Importe);
        var descuentos = conceptos.Where(c => c.Activo).Select(c => c.Tipo switch
        {
            TipoConceptoLiquidacion.PorKilo => new DescuentoValorado(c.Id, c.Nombre, c.Tipo, c.Valor, totalKg, Redondeo.Dos(totalKg * c.Valor)),
            TipoConceptoLiquidacion.PorcentajeBruto => new DescuentoValorado(c.Id, c.Nombre, c.Tipo, c.Valor, bruto, Redondeo.Dos(bruto * c.Valor / 100m)),
            _ => new DescuentoValorado(c.Id, c.Nombre, c.Tipo, c.Valor, 0m, Redondeo.Dos(c.Valor)),
        }).ToList();
        var totalDescuentos = descuentos.Sum(d => d.Importe);
        var baseImponible = bruto - totalDescuentos;
        if (baseImponible < 0m)
        {
            var error = Error.Validacion("liquidacion.base_negativa", $"Los descuentos ({Redondeo.Formatear(totalDescuentos)} €) superan el importe de la fruta ({Redondeo.Formatear(bruto)} €).");
            errores = [error];
            return Resultado.Fallo<ValoracionLiquidacion>(error);
        }

        // Mismas fórmulas que el gasto (autofactura) que se registra al emitir: los importes cuadran.
        var cuota = Redondeo.Dos(baseImponible * porcentajeImpuesto / 100m);
        var retencion = Redondeo.Dos(baseImponible * porcentajeRetencion / 100m);
        var total = baseImponible + cuota;
        return Resultado.Ok(new ValoracionLiquidacion(valoradas, descuentos, totalKg, bruto, totalDescuentos, baseImponible,
            porcentajeImpuesto, cuota, porcentajeRetencion, retencion, total, total - retencion));
    }

    /// <summary>Un error que resume todos (el código del primero y todos los mensajes).</summary>
    public static Error Resumen(IReadOnlyList<Error> errores)
    {
        ArgumentNullException.ThrowIfNull(errores);
        return errores.Count == 1 ? errores[0] : Error.Validacion(errores[0].Codigo, string.Join(" ", errores.Select(e => e.Mensaje)));
    }

    private static LineaValorada Linea(LineaALiquidar l, Guid? categoriaId, PrecioAplicable precio, decimal kilos) =>
        new(l.LineaRecepcionId, l.RecepcionId, l.PartidaId, categoriaId, precio.Id, l.Fecha, kilos, precio.PrecioKg, Redondeo.Dos(kilos * precio.PrecioKg));
}

// ---------------------------------------------------------------------------------------------- Liquidación

public enum EstadoLiquidacion
{
    /// <summary>Calculada, sin número: se puede recalcular o eliminar. Reserva sus entregas.</summary>
    Borrador = 1,

    /// <summary>Emitida: numerada sin huecos y registrada como autofactura (gasto). No cambia.</summary>
    Emitida = 2,

    /// <summary>Anulada (su autofactura también): las entregas quedan libres para otra liquidación.</summary>
    Anulada = 3,
}

/// <summary>
/// Liquidación al agricultor de la fruta entregada en un periodo de la campaña: la factura que la empresa
/// emite en nombre del agricultor (autofacturación), con el detalle de kilos, categorías y precios.
/// </summary>
public sealed class Liquidacion : RaizAgregadoEmpresa<Guid>
{
    public const string Serie = "LIQ";

    private readonly List<LineaLiquidacion> _lineas = [];
    private readonly List<DescuentoLiquidacion> _descuentos = [];

    private Liquidacion(Guid id)
        : base(id, Guid.Empty)
    {
        CodigoImpuesto = null!;
    }

    private Liquidacion(Guid id, Guid empresaId, Guid agricultorId, Guid campanaId, DateOnly desde, DateOnly hasta, DateOnly fecha,
        RegimenAgricultor regimen, string codigoImpuesto, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        AgricultorId = agricultorId;
        CampanaId = campanaId;
        Desde = desde;
        Hasta = hasta;
        Fecha = fecha;
        Ejercicio = fecha.Year;
        Regimen = regimen;
        CodigoImpuesto = codigoImpuesto;
        Estado = EstadoLiquidacion.Borrador;
        CreadoEn = ahora;
    }

    public Guid AgricultorId { get; private set; }

    public Guid CampanaId { get; private set; }

    public DateOnly Desde { get; private set; }

    public DateOnly Hasta { get; private set; }

    /// <summary>Fecha de la liquidación (de la autofactura).</summary>
    public DateOnly Fecha { get; private set; }

    public int Ejercicio { get; private set; }

    public int? Numero { get; private set; }

    public string? NumeroCompleto => Numero is { } n ? $"{Serie}-{Ejercicio}-{n:D6}" : null;

    public RegimenAgricultor Regimen { get; private set; }

    /// <summary>Código del impuesto de la autofactura: REAGP12 (compensación) o el IVA del régimen general.</summary>
    public string CodigoImpuesto { get; private set; }

    public decimal PorcentajeImpuesto { get; private set; }

    public decimal PorcentajeRetencion { get; private set; }

    public decimal Kilos { get; private set; }

    public decimal Bruto { get; private set; }

    public decimal TotalDescuentos { get; private set; }

    public decimal BaseImponible { get; private set; }

    public decimal CuotaImpuesto { get; private set; }

    public decimal Retencion { get; private set; }

    public decimal TotalFactura { get; private set; }

    public decimal APagar { get; private set; }

    public EstadoLiquidacion Estado { get; private set; }

    /// <summary>Gasto (autofactura) registrado al emitir.</summary>
    public Guid? GastoId { get; private set; }

    public string? MotivoAnulacion { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset? EmitidaEn { get; private set; }

    public IReadOnlyList<LineaLiquidacion> Lineas => _lineas;

    public IReadOnlyList<DescuentoLiquidacion> Descuentos => _descuentos;

    public static Resultado<Liquidacion> Crear(Guid empresaId, Agricultor agricultor, Guid campanaId, DateOnly desde, DateOnly hasta, DateOnly fecha,
        string codigoImpuesto, ValoracionLiquidacion valoracion, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(agricultor);
        ArgumentNullException.ThrowIfNull(reloj);
        if (hasta < desde || fecha < hasta)
        {
            return Resultado.Fallo<Liquidacion>(Error.Validacion("liquidacion.fechas", "El periodo no es válido o la fecha de la liquidación es anterior a su final."));
        }

        var l = new Liquidacion(Guid.NewGuid(), empresaId, agricultor.Id, campanaId, desde, hasta, fecha, agricultor.Regimen, codigoImpuesto, reloj.AhoraUtc);
        l.Fijar(valoracion);
        return Resultado.Ok(l);
    }

    /// <summary>Vuelve a valorar un borrador (han cambiado precios, clasificaciones o descuentos).</summary>
    public Resultado Recalcular(ValoracionLiquidacion valoracion)
    {
        if (Estado != EstadoLiquidacion.Borrador)
        {
            return Resultado.Fallo(Error.Conflicto("liquidacion.no_borrador", "Solo se recalcula una liquidación en borrador."));
        }

        Fijar(valoracion);
        return Resultado.Ok();
    }

    public Resultado Emitir(int numero, Guid gastoId, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (Estado != EstadoLiquidacion.Borrador)
        {
            return Resultado.Fallo(Error.Conflicto("liquidacion.no_borrador", "La liquidación ya está emitida o anulada."));
        }

        Numero = numero;
        GastoId = gastoId;
        Estado = EstadoLiquidacion.Emitida;
        EmitidaEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    public Resultado Anular(string? motivo)
    {
        if (Estado != EstadoLiquidacion.Emitida)
        {
            return Resultado.Fallo(Error.Conflicto("liquidacion.no_emitida", "Solo se anula una liquidación emitida (un borrador se elimina)."));
        }

        if (string.IsNullOrWhiteSpace(motivo) || motivo.Trim().Length > Recepcion.LongitudTexto)
        {
            return Resultado.Fallo(Error.Validacion("liquidacion.motivo", "Indica el motivo de la anulación."));
        }

        Estado = EstadoLiquidacion.Anulada;
        MotivoAnulacion = motivo.Trim();
        return Resultado.Ok();
    }

    private void Fijar(ValoracionLiquidacion v)
    {
        ArgumentNullException.ThrowIfNull(v);
        _lineas.Clear();
        _descuentos.Clear();
        _lineas.AddRange(v.Lineas.Select(x => new LineaLiquidacion(Guid.NewGuid(), x)));
        _descuentos.AddRange(v.Descuentos.Select(x => new DescuentoLiquidacion(Guid.NewGuid(), x)));
        PorcentajeImpuesto = v.PorcentajeImpuesto;
        PorcentajeRetencion = v.PorcentajeRetencion;
        Kilos = v.Kilos;
        Bruto = v.Bruto;
        TotalDescuentos = v.TotalDescuentos;
        BaseImponible = v.Base;
        CuotaImpuesto = v.CuotaImpuesto;
        Retencion = v.Retencion;
        TotalFactura = v.TotalFactura;
        APagar = v.APagar;
    }
}

public sealed class LineaLiquidacion : EntidadBase<Guid>
{
    private LineaLiquidacion(Guid id)
        : base(id)
    {
    }

    internal LineaLiquidacion(Guid id, LineaValorada v)
        : base(id)
    {
        LineaRecepcionId = v.LineaRecepcionId;
        RecepcionId = v.RecepcionId;
        PartidaId = v.PartidaId;
        CategoriaId = v.CategoriaId;
        PrecioId = v.PrecioId;
        FechaRecepcion = v.Fecha;
        Kilos = v.Kilos;
        PrecioKg = v.PrecioKg;
        Importe = v.Importe;
    }

    public Guid LineaRecepcionId { get; private set; }

    public Guid RecepcionId { get; private set; }

    public Guid PartidaId { get; private set; }

    public Guid? CategoriaId { get; private set; }

    /// <summary>Precio aplicado (para explicar el importe).</summary>
    public Guid PrecioId { get; private set; }

    public DateOnly FechaRecepcion { get; private set; }

    public decimal Kilos { get; private set; }

    public decimal PrecioKg { get; private set; }

    public decimal Importe { get; private set; }
}

public sealed class DescuentoLiquidacion : EntidadBase<Guid>
{
    private DescuentoLiquidacion(Guid id)
        : base(id)
    {
        Nombre = null!;
    }

    internal DescuentoLiquidacion(Guid id, DescuentoValorado d)
        : base(id)
    {
        ConceptoId = d.ConceptoId;
        Nombre = d.Nombre;
        Tipo = d.Tipo;
        Valor = d.Valor;
        Base = d.Base;
        Importe = d.Importe;
    }

    public Guid ConceptoId { get; private set; }

    public string Nombre { get; private set; }

    public TipoConceptoLiquidacion Tipo { get; private set; }

    public decimal Valor { get; private set; }

    public decimal Base { get; private set; }

    public decimal Importe { get; private set; }
}
