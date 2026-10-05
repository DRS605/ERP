using System.Text.Json;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Bodega.Dominio;

public enum TipoDeposito
{
    Acero = 1,
    Hormigon = 2,
    Barrica = 3,
    Tinaja = 4,
    Otro = 5,
}

/// <summary>Producto vitivinícola que contiene un depósito (la categoría de la declaración de existencias).</summary>
public enum ProductoVinico
{
    Mosto = 1,
    VinoBlanco = 2,
    VinoRosado = 3,
    VinoTinto = 4,

    /// <summary>Otros productos (vino de licor, base de espumoso…).</summary>
    Otro = 5,
}

/// <summary>Litros de una variedad y una añada dentro de un vino.</summary>
public sealed record ComponenteVino(string Variedad, int Anada, decimal Litros);

/// <summary>Contenido de un depósito en un momento: lo que se guarda para deshacer una operación.</summary>
public sealed record EstadoDeposito(decimal Litros, ProductoVinico? Producto, string? Calificacion, IReadOnlyList<ComponenteVino> Composicion, Guid? UltimaOperacionId);

public static class ReglasBodega
{
    public const int LongitudCodigo = 30;
    public const int LongitudNombre = 120;
    public const int LongitudVariedad = 60;
    public const int LongitudCalificacion = 80;
    public const int LongitudObservaciones = 500;

    /// <summary>Litros con 2 decimales.</summary>
    public static decimal Litros(decimal valor) => Math.Round(valor, 2, MidpointRounding.AwayFromZero);

    public static bool LitrosValidos(decimal litros) => litros > 0m && Litros(litros) == litros;

    public static string? Texto(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

    /// <summary>Reparte <paramref name="total"/> litros en proporción a los pesos, a 2 decimales, sin perder un céntimo de litro (el resto, al mayor).</summary>
    public static decimal[] Repartir(decimal total, IReadOnlyList<decimal> pesos)
    {
        ArgumentNullException.ThrowIfNull(pesos);
        var suma = pesos.Sum();
        var partes = pesos.Select(p => suma == 0m ? 0m : Litros(total * p / suma)).ToArray();
        if (partes.Length > 0)
        {
            var mayor = Array.IndexOf(pesos.ToArray(), pesos.Max());
            partes[mayor] += total - partes.Sum();
        }

        return partes;
    }

    /// <summary>Agrupa los componentes por variedad y añada, sin los que se quedan a cero.</summary>
    public static List<ComponenteVino> Agrupar(IEnumerable<ComponenteVino> componentes) =>
        componentes.GroupBy(c => (c.Variedad, c.Anada)).Select(g => new ComponenteVino(g.Key.Variedad, g.Key.Anada, g.Sum(c => c.Litros)))
            .Where(c => c.Litros != 0m).OrderByDescending(c => c.Litros).ThenBy(c => c.Variedad, StringComparer.Ordinal).ThenBy(c => c.Anada).ToList();
}

/// <summary>Variedad y añada de un depósito, con sus litros.</summary>
public sealed class ComponenteDeposito
{
    private ComponenteDeposito()
    {
        Variedad = null!;
    }

    internal ComponenteDeposito(ComponenteVino c)
    {
        Id = Guid.NewGuid();
        Variedad = c.Variedad;
        Anada = c.Anada;
        Litros = c.Litros;
    }

    public Guid Id { get; private set; }

    public string Variedad { get; private set; }

    public int Anada { get; private set; }

    public decimal Litros { get; private set; }
}

/// <summary>
/// Depósito de la bodega (acero, hormigón, barrica…) con su capacidad y lo que contiene: los litros, el producto, la
/// calificación (DOP, IGP… o ninguna) y la composición por variedad y añada. Solo cambia con las operaciones de bodega,
/// que guardan su estado anterior para poder deshacerse (la última de cada depósito).
/// </summary>
public sealed class Deposito : RaizAgregadoEmpresa<Guid>
{
    private readonly List<ComponenteDeposito> _composicion = [];

    private Deposito(Guid id)
        : base(id, Guid.Empty)
    {
        Codigo = null!;
        Nombre = null!;
    }

    private Deposito(Guid id, Guid empresaId, string codigo, string nombre, TipoDeposito tipo, decimal capacidad)
        : base(id, empresaId)
    {
        Codigo = codigo;
        Nombre = nombre;
        Tipo = tipo;
        CapacidadLitros = capacidad;
        Activo = true;
    }

    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    public TipoDeposito Tipo { get; private set; }

    public decimal CapacidadLitros { get; private set; }

    public bool Activo { get; private set; }

    public decimal Litros { get; private set; }

    public ProductoVinico? Producto { get; private set; }

    public string? Calificacion { get; private set; }

    /// <summary>Última operación que lo cambió: solo esa se puede deshacer.</summary>
    public Guid? UltimaOperacionId { get; private set; }

    public IReadOnlyList<ComponenteDeposito> Composicion => _composicion;

    public bool Vacio => Litros == 0m;

    public static Resultado<Deposito> Crear(Guid empresaId, string? codigo, string? nombre, TipoDeposito tipo, decimal capacidad)
    {
        var c = ReglasBodega.Texto(codigo)?.ToUpperInvariant();
        if (c is null || c.Length > ReglasBodega.LongitudCodigo)
        {
            return Resultado.Fallo<Deposito>(Error.Validacion("deposito.codigo", $"El código es obligatorio (hasta {ReglasBodega.LongitudCodigo} caracteres)."));
        }

        var d = new Deposito(Guid.NewGuid(), empresaId, c, c, tipo, 0m);
        var r = d.Cambiar(nombre, tipo, capacidad, true);
        return r.EsFallo ? Resultado.Fallo<Deposito>(r.Error) : Resultado.Ok(d);
    }

    public Resultado Cambiar(string? nombre, TipoDeposito tipo, decimal capacidad, bool activo)
    {
        var n = ReglasBodega.Texto(nombre) ?? Codigo;
        if (n.Length > ReglasBodega.LongitudNombre)
        {
            return Resultado.Fallo(Error.Validacion("deposito.nombre", $"El nombre admite hasta {ReglasBodega.LongitudNombre} caracteres."));
        }

        if (!Enum.IsDefined(tipo))
        {
            return Resultado.Fallo(Error.Validacion("deposito.tipo", "Tipo de depósito no válido."));
        }

        if (!ReglasBodega.LitrosValidos(capacidad))
        {
            return Resultado.Fallo(Error.Validacion("deposito.capacidad", "La capacidad en litros debe ser positiva (hasta 2 decimales)."));
        }

        if (capacidad < Litros)
        {
            return Resultado.Fallo(Error.Conflicto("deposito.capacidad", $"El depósito contiene {Redondeo.Formatear(Litros)} l: la capacidad no puede ser menor."));
        }

        if (!activo && !Vacio)
        {
            return Resultado.Fallo(Error.Conflicto("deposito.con_vino", "Un depósito con vino no se da de baja: vacíalo antes."));
        }

        Nombre = n;
        Tipo = tipo;
        CapacidadLitros = capacidad;
        Activo = activo;
        return Resultado.Ok();
    }

    public EstadoDeposito Estado() =>
        new(Litros, Producto, Calificacion, _composicion.Select(c => new ComponenteVino(c.Variedad, c.Anada, c.Litros)).ToList(), UltimaOperacionId);

    /// <summary>
    /// Entran litros con su composición. Un depósito con vino solo admite el mismo producto; si la calificación no
    /// coincide, el conjunto se queda sin ella. Devuelve el estado anterior si los litros que ya había cambian de
    /// categoría (para reclasificarlos en el libro).
    /// </summary>
    internal Resultado<(decimal Litros, ProductoVinico Producto, string? Calificacion)?> Entrar(decimal litros, ProductoVinico producto, string? calificacion,
        IReadOnlyList<ComponenteVino> composicion, Guid operacionId)
    {
        if (!Activo)
        {
            return Resultado.Fallo<(decimal, ProductoVinico, string?)?>(Error.Conflicto("deposito.inactivo", $"El depósito {Codigo} está dado de baja."));
        }

        if (!ReglasBodega.LitrosValidos(litros))
        {
            return Resultado.Fallo<(decimal, ProductoVinico, string?)?>(Error.Validacion("bodega.litros", "Los litros deben ser positivos (hasta 2 decimales)."));
        }

        if (Litros + litros > CapacidadLitros)
        {
            return Resultado.Fallo<(decimal, ProductoVinico, string?)?>(Error.Conflicto("deposito.capacidad",
                $"En el depósito {Codigo} caben {Redondeo.Formatear(CapacidadLitros - Litros)} l más y entran {Redondeo.Formatear(litros)} l."));
        }

        if (!Vacio && Producto != producto)
        {
            return Resultado.Fallo<(decimal, ProductoVinico, string?)?>(Error.Conflicto("deposito.mezcla_producto",
                $"El depósito {Codigo} tiene {Producto}: no se le mezcla {producto}."));
        }

        (decimal, ProductoVinico, string?)? reclasificar = null;
        var nueva = Vacio ? calificacion : string.Equals(Calificacion, calificacion, StringComparison.OrdinalIgnoreCase) ? Calificacion : null;
        if (!Vacio && !string.Equals(nueva, Calificacion, StringComparison.OrdinalIgnoreCase))
        {
            reclasificar = (Litros, Producto!.Value, Calificacion);
        }

        var componentes = ReglasBodega.Agrupar(Estado().Composicion.Concat(composicion));
        _composicion.Clear();
        _composicion.AddRange(componentes.Select(c => new ComponenteDeposito(c)));
        Litros += litros;
        Producto = producto;
        Calificacion = nueva;
        UltimaOperacionId = operacionId;
        return Resultado.Ok(reclasificar);
    }

    /// <summary>Salen litros: la composición sale en proporción. Vacío, se queda sin producto ni calificación.</summary>
    internal Resultado<List<ComponenteVino>> Sacar(decimal litros, Guid operacionId)
    {
        if (!ReglasBodega.LitrosValidos(litros))
        {
            return Resultado.Fallo<List<ComponenteVino>>(Error.Validacion("bodega.litros", "Los litros deben ser positivos (hasta 2 decimales)."));
        }

        if (litros > Litros)
        {
            return Resultado.Fallo<List<ComponenteVino>>(Error.Conflicto("deposito.sin_litros",
                $"El depósito {Codigo} tiene {Redondeo.Formatear(Litros)} l y salen {Redondeo.Formatear(litros)} l."));
        }

        var actual = Estado().Composicion;
        var salen = litros == Litros
            ? actual.ToList()
            : actual.Zip(ReglasBodega.Repartir(litros, actual.Select(c => c.Litros).ToList()), (c, l) => c with { Litros = l }).ToList();
        var quedan = ReglasBodega.Agrupar(actual.Concat(salen.Select(c => c with { Litros = -c.Litros })));
        _composicion.Clear();
        _composicion.AddRange(quedan.Select(c => new ComponenteDeposito(c)));
        Litros -= litros;
        if (Litros == 0m)
        {
            _composicion.Clear();
            Producto = null;
            Calificacion = null;
        }

        UltimaOperacionId = operacionId;
        return Resultado.Ok(ReglasBodega.Agrupar(salen));
    }

    /// <summary>Vuelve al estado anterior a una operación (al deshacerla).</summary>
    internal void Restaurar(EstadoDeposito estado)
    {
        Litros = estado.Litros;
        Producto = estado.Producto;
        Calificacion = estado.Calificacion;
        UltimaOperacionId = estado.UltimaOperacionId;
        _composicion.Clear();
        _composicion.AddRange(estado.Composicion.Select(c => new ComponenteDeposito(c)));
    }
}

// ============================================================================================== Uva

/// <summary>
/// Entrada de uva en la báscula de la bodega: viticultor (proveedor), variedad, kilos, grado (ºBaumé), parcela y
/// calificación, de la añada de su fecha. Se elabora en un depósito y se liquida al viticultor; anulada no cuenta.
/// </summary>
public sealed class EntradaUva : RaizAgregadoEmpresa<Guid>
{
    private EntradaUva(Guid id)
        : base(id, Guid.Empty)
    {
        ViticultorNombre = null!;
        Variedad = null!;
    }

    private EntradaUva(Guid id, Guid empresaId, int numero, DateOnly fecha, Guid viticultorId, string viticultor, string variedad, decimal kilos, decimal grado,
        string? parcela, string? calificacion, string? observaciones, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Ejercicio = fecha.Year;
        Numero = numero;
        Fecha = fecha;
        Anada = fecha.Year;
        ViticultorId = viticultorId;
        ViticultorNombre = viticultor;
        Variedad = variedad;
        Kilos = kilos;
        GradoBaume = grado;
        Parcela = parcela;
        Calificacion = calificacion;
        Observaciones = observaciones;
        CreadaEn = ahora;
    }

    public int Ejercicio { get; private set; }

    public int Numero { get; private set; }

    public string NumeroCompleto => $"U{Ejercicio}/{Numero:00000}";

    public DateOnly Fecha { get; private set; }

    /// <summary>Añada (la del año de la vendimia).</summary>
    public int Anada { get; private set; }

    public Guid ViticultorId { get; private set; }

    public string ViticultorNombre { get; private set; }

    public string Variedad { get; private set; }

    public decimal Kilos { get; private set; }

    /// <summary>Grado del mosto en ºBaumé (el que se paga).</summary>
    public decimal GradoBaume { get; private set; }

    public string? Parcela { get; private set; }

    /// <summary>DOP, IGP… a la que puede ir la uva (null: sin indicación).</summary>
    public string? Calificacion { get; private set; }

    public string? Observaciones { get; private set; }

    /// <summary>Elaboración en que se usó (null: pendiente de elaborar).</summary>
    public Guid? OperacionId { get; private set; }

    /// <summary>Liquidación al viticultor (null: pendiente de pagar).</summary>
    public Guid? LiquidacionId { get; private set; }

    public bool Anulada { get; private set; }

    public string? MotivoAnulacion { get; private set; }

    public DateTimeOffset CreadaEn { get; private set; }

    public static Resultado<EntradaUva> Crear(Guid empresaId, int numero, DateOnly fecha, Guid viticultorId, string viticultor, string? variedad, decimal kilos, decimal grado,
        string? parcela, string? calificacion, string? observaciones, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        var e = new EntradaUva(Guid.NewGuid(), empresaId, numero, fecha, viticultorId, viticultor, "?", 0m, 0m, null, null, null, reloj.AhoraUtc);
        var r = e.Corregir(variedad, kilos, grado, parcela, calificacion, observaciones);
        return r.EsFallo ? Resultado.Fallo<EntradaUva>(r.Error) : Resultado.Ok(e);
    }

    /// <summary>Corrige los datos mientras no se haya elaborado ni liquidado.</summary>
    public Resultado Corregir(string? variedad, decimal kilos, decimal grado, string? parcela, string? calificacion, string? observaciones)
    {
        if (Anulada || OperacionId is not null || LiquidacionId is not null)
        {
            return Resultado.Fallo(Error.Conflicto("uva.usada", "La entrada ya está elaborada, liquidada o anulada: no se corrige."));
        }

        var v = ReglasBodega.Texto(variedad);
        if (v is null || v.Length > ReglasBodega.LongitudVariedad)
        {
            return Resultado.Fallo(Error.Validacion("uva.variedad", "Indica la variedad."));
        }

        if (kilos <= 0m || decimal.Round(kilos, 2) != kilos)
        {
            return Resultado.Fallo(Error.Validacion("uva.kilos", "Los kilos deben ser positivos (hasta 2 decimales)."));
        }

        if (grado is < 0m or > 30m || decimal.Round(grado, 2) != grado)
        {
            return Resultado.Fallo(Error.Validacion("uva.grado", "El grado Baumé va de 0 a 30 (hasta 2 decimales)."));
        }

        var c = ReglasBodega.Texto(calificacion);
        var p = ReglasBodega.Texto(parcela);
        var o = ReglasBodega.Texto(observaciones);
        if (c?.Length > ReglasBodega.LongitudCalificacion || p?.Length > ReglasBodega.LongitudNombre || o?.Length > ReglasBodega.LongitudObservaciones)
        {
            return Resultado.Fallo(Error.Validacion("uva.texto", "La parcela, la calificación o las observaciones son demasiado largas."));
        }

        Variedad = v;
        Kilos = kilos;
        GradoBaume = grado;
        Parcela = p;
        Calificacion = c;
        Observaciones = o;
        return Resultado.Ok();
    }

    internal Resultado Elaborar(Guid operacionId)
    {
        if (Anulada || OperacionId is not null)
        {
            return Resultado.Fallo(Error.Conflicto("uva.elaborada", $"La entrada {NumeroCompleto} ya está elaborada o anulada."));
        }

        OperacionId = operacionId;
        return Resultado.Ok();
    }

    internal void DeshacerElaboracion() => OperacionId = null;

    internal Resultado Liquidar(Guid liquidacionId)
    {
        if (Anulada || LiquidacionId is not null)
        {
            return Resultado.Fallo(Error.Conflicto("uva.liquidada", $"La entrada {NumeroCompleto} ya está liquidada o anulada."));
        }

        LiquidacionId = liquidacionId;
        return Resultado.Ok();
    }

    internal void DeshacerLiquidacion() => LiquidacionId = null;

    public Resultado Anular(string? motivo)
    {
        if (Anulada || OperacionId is not null || LiquidacionId is not null)
        {
            return Resultado.Fallo(Error.Conflicto("uva.usada", "La entrada ya está elaborada, liquidada o anulada: deshaz antes la elaboración o la liquidación."));
        }

        var m = ReglasBodega.Texto(motivo);
        if (m is null)
        {
            return Resultado.Fallo(Error.Validacion("uva.motivo", "Indica el motivo de la anulación."));
        }

        Anulada = true;
        MotivoAnulacion = m.Length > 200 ? m[..200] : m;
        return Resultado.Ok();
    }
}

/// <summary>
/// Precio de la uva de una variedad en una añada: el precio por kilo al grado de referencia, que sube o baja un
/// porcentaje por cada grado Baumé por encima o por debajo.
/// </summary>
public sealed class PrecioUva : RaizAgregadoEmpresa<Guid>
{
    private PrecioUva(Guid id)
        : base(id, Guid.Empty)
    {
        Variedad = null!;
    }

    private PrecioUva(Guid id, Guid empresaId, int anada, string variedad)
        : base(id, empresaId)
    {
        Anada = anada;
        Variedad = variedad;
    }

    public int Anada { get; private set; }

    public string Variedad { get; private set; }

    public decimal PrecioKg { get; private set; }

    public decimal GradoReferencia { get; private set; }

    /// <summary>Porcentaje del precio que se suma (o resta) por cada grado por encima (o por debajo) del de referencia.</summary>
    public decimal PorcentajePorGrado { get; private set; }

    public static Resultado<PrecioUva> Crear(Guid empresaId, int anada, string? variedad, decimal precioKg, decimal gradoReferencia, decimal porcentajePorGrado)
    {
        var v = ReglasBodega.Texto(variedad);
        if (v is null || v.Length > ReglasBodega.LongitudVariedad)
        {
            return Resultado.Fallo<PrecioUva>(Error.Validacion("uva.variedad", "Indica la variedad."));
        }

        if (anada is < 1900 or > 2200)
        {
            return Resultado.Fallo<PrecioUva>(Error.Validacion("precio_uva.anada", "Añada no válida."));
        }

        var p = new PrecioUva(Guid.NewGuid(), empresaId, anada, v);
        var r = p.Fijar(precioKg, gradoReferencia, porcentajePorGrado);
        return r.EsFallo ? Resultado.Fallo<PrecioUva>(r.Error) : Resultado.Ok(p);
    }

    public Resultado Fijar(decimal precioKg, decimal gradoReferencia, decimal porcentajePorGrado)
    {
        if (precioKg < 0m || decimal.Round(precioKg, 6) != precioKg)
        {
            return Resultado.Fallo(Error.Validacion("precio_uva.precio", "El precio por kilo no puede ser negativo (hasta 6 decimales)."));
        }

        if (gradoReferencia is <= 0m or > 30m || porcentajePorGrado is < 0m or > 100m)
        {
            return Resultado.Fallo(Error.Validacion("precio_uva.grado", "El grado de referencia va de 0 a 30 y el porcentaje por grado de 0 a 100."));
        }

        PrecioKg = precioKg;
        GradoReferencia = gradoReferencia;
        PorcentajePorGrado = porcentajePorGrado;
        return Resultado.Ok();
    }

    /// <summary>Precio por kilo de una uva de ese grado (nunca negativo).</summary>
    public decimal PrecioDe(decimal grado) =>
        Math.Max(0m, decimal.Round(PrecioKg * (1m + ((grado - GradoReferencia) * PorcentajePorGrado / 100m)), 6));
}

public enum EstadoLiquidacionUva
{
    Emitida = 1,
    Anulada = 2,
}

public sealed class LineaLiquidacionUva
{
    private LineaLiquidacionUva()
    {
        Entrada = null!;
        Variedad = null!;
    }

    internal LineaLiquidacionUva(Guid entradaUvaId, string entrada, string variedad, decimal kilos, decimal grado, decimal precioKg)
    {
        Id = Guid.NewGuid();
        EntradaUvaId = entradaUvaId;
        Entrada = entrada;
        Variedad = variedad;
        Kilos = kilos;
        GradoBaume = grado;
        PrecioKg = precioKg;
        Importe = Redondeo.Dos(kilos * precioKg);
    }

    public Guid Id { get; private set; }

    public Guid EntradaUvaId { get; private set; }

    public string Entrada { get; private set; }

    public string Variedad { get; private set; }

    public decimal Kilos { get; private set; }

    public decimal GradoBaume { get; private set; }

    public decimal PrecioKg { get; private set; }

    public decimal Importe { get; private set; }
}

/// <summary>
/// Liquidación de la uva a un viticultor: sus entradas pendientes de unas fechas, cada una al precio de su variedad y
/// añada corregido por su grado; se registra como autofactura (gasto) con la compensación REAGP o el IVA y la retención.
/// </summary>
public sealed class LiquidacionUva : RaizAgregadoEmpresa<Guid>
{
    private readonly List<LineaLiquidacionUva> _lineas = [];

    private LiquidacionUva(Guid id)
        : base(id, Guid.Empty)
    {
        ViticultorNombre = null!;
        CodigoImpuesto = null!;
    }

    private LiquidacionUva(Guid id, Guid empresaId, int numero, DateOnly fecha, Guid viticultorId, string viticultor, DateOnly desde, DateOnly hasta, string codigoImpuesto,
        decimal porcentajeRetencion, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Ejercicio = fecha.Year;
        Numero = numero;
        Fecha = fecha;
        ViticultorId = viticultorId;
        ViticultorNombre = viticultor;
        Desde = desde;
        Hasta = hasta;
        CodigoImpuesto = codigoImpuesto;
        PorcentajeRetencion = porcentajeRetencion;
        Estado = EstadoLiquidacionUva.Emitida;
        CreadaEn = ahora;
    }

    public int Ejercicio { get; private set; }

    public int Numero { get; private set; }

    public string NumeroCompleto => $"LU{Ejercicio}/{Numero:00000}";

    public DateOnly Fecha { get; private set; }

    public Guid ViticultorId { get; private set; }

    public string ViticultorNombre { get; private set; }

    public DateOnly Desde { get; private set; }

    public DateOnly Hasta { get; private set; }

    public string CodigoImpuesto { get; private set; }

    public decimal PorcentajeRetencion { get; private set; }

    public decimal Kilos { get; private set; }

    public decimal BaseImponible { get; private set; }

    public Guid? GastoId { get; private set; }

    public decimal TotalFactura { get; private set; }

    public EstadoLiquidacionUva Estado { get; private set; }

    public string? MotivoAnulacion { get; private set; }

    public DateTimeOffset CreadaEn { get; private set; }

    public IReadOnlyList<LineaLiquidacionUva> Lineas => _lineas;

    /// <summary>Valora las entradas con sus precios; sin precio para alguna variedad y añada es un error (nunca un cero).</summary>
    public static Resultado<LiquidacionUva> Crear(Guid empresaId, int numero, DateOnly fecha, Guid viticultorId, string viticultor, DateOnly desde, DateOnly hasta,
        string codigoImpuesto, decimal porcentajeRetencion, IReadOnlyList<EntradaUva> entradas, IReadOnlyList<PrecioUva> precios, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(entradas);
        ArgumentNullException.ThrowIfNull(precios);
        ArgumentNullException.ThrowIfNull(reloj);
        if (entradas.Count == 0)
        {
            return Resultado.Fallo<LiquidacionUva>(Error.Validacion("liquidacion_uva.sin_entradas", "El viticultor no tiene entradas de uva pendientes de liquidar en esas fechas."));
        }

        if (porcentajeRetencion is < 0m or > 100m)
        {
            return Resultado.Fallo<LiquidacionUva>(Error.Validacion("liquidacion_uva.retencion", "La retención va de 0 a 100 %."));
        }

        var faltan = entradas.Where(e => !precios.Any(p => p.Anada == e.Anada && string.Equals(p.Variedad, e.Variedad, StringComparison.OrdinalIgnoreCase)))
            .Select(e => $"{e.Variedad} {e.Anada}").Distinct().ToList();
        if (faltan.Count > 0)
        {
            return Resultado.Fallo<LiquidacionUva>(Error.Validacion("liquidacion_uva.sin_precio", $"No hay precio de la uva para: {string.Join(", ", faltan)}."));
        }

        var l = new LiquidacionUva(Guid.NewGuid(), empresaId, numero, fecha, viticultorId, viticultor, desde, hasta, codigoImpuesto, porcentajeRetencion, reloj.AhoraUtc);
        foreach (var e in entradas.OrderBy(e => e.Fecha).ThenBy(e => e.Numero))
        {
            var precio = precios.First(p => p.Anada == e.Anada && string.Equals(p.Variedad, e.Variedad, StringComparison.OrdinalIgnoreCase));
            l._lineas.Add(new LineaLiquidacionUva(e.Id, e.NumeroCompleto, e.Variedad, e.Kilos, e.GradoBaume, precio.PrecioDe(e.GradoBaume)));
            if (e.Liquidar(l.Id) is { EsFallo: true } r)
            {
                return Resultado.Fallo<LiquidacionUva>(r.Error);
            }
        }

        l.Kilos = l._lineas.Sum(x => x.Kilos);
        l.BaseImponible = l._lineas.Sum(x => x.Importe);
        return Resultado.Ok(l);
    }

    public void Registrada(Guid gastoId, decimal total)
    {
        GastoId = gastoId;
        TotalFactura = total;
    }

    public Resultado Anular(string? motivo, IReadOnlyList<EntradaUva> entradas)
    {
        ArgumentNullException.ThrowIfNull(entradas);
        if (Estado == EstadoLiquidacionUva.Anulada)
        {
            return Resultado.Fallo(Error.Conflicto("liquidacion_uva.anulada", "La liquidación ya está anulada."));
        }

        var m = ReglasBodega.Texto(motivo);
        if (m is null)
        {
            return Resultado.Fallo(Error.Validacion("liquidacion_uva.motivo", "Indica el motivo de la anulación."));
        }

        foreach (var e in entradas.Where(e => e.LiquidacionId == Id))
        {
            e.DeshacerLiquidacion();
        }

        Estado = EstadoLiquidacionUva.Anulada;
        MotivoAnulacion = m.Length > 200 ? m[..200] : m;
        return Resultado.Ok();
    }
}

// ============================================================================================== Operaciones

public enum TipoOperacionBodega
{
    /// <summary>Uva a mosto o vino en un depósito.</summary>
    Elaboracion = 1,

    /// <summary>De un depósito a otro.</summary>
    Trasiego = 2,

    /// <summary>Varios depósitos a uno (mezcla).</summary>
    Coupage = 3,

    /// <summary>Pérdida (evaporación, lías, merma).</summary>
    Merma = 4,

    /// <summary>Del depósito a botellas, que entran en las existencias del artículo.</summary>
    Embotellado = 5,

    /// <summary>Venta a granel a un cliente (con albarán).</summary>
    SalidaGranel = 6,
}

public enum ClaseLineaBodega
{
    /// <summary>Salen litros del depósito.</summary>
    Sale = 1,

    /// <summary>Entran litros en el depósito.</summary>
    Entra = 2,

    /// <summary>Los litros que ya había cambian de categoría al mezclarse (sale una categoría y entra otra).</summary>
    Reclasifica = 3,

    /// <summary>Litros que se pierden (evaporación, lías, merma del trasiego o del embotellado).</summary>
    Merma = 4,
}

/// <summary>Movimiento de litros de una operación en un depósito, con la categoría de lo que mueve (el libro de la bodega).</summary>
public sealed class LineaOperacionBodega
{
    private LineaOperacionBodega()
    {
        DepositoCodigo = null!;
    }

    internal LineaOperacionBodega(int orden, Deposito deposito, ClaseLineaBodega clase, decimal litros, ProductoVinico producto, string? calificacion, string? estadoAnterior)
    {
        Id = Guid.NewGuid();
        Orden = orden;
        DepositoId = deposito.Id;
        DepositoCodigo = deposito.Codigo;
        Clase = clase;
        Litros = litros;
        Producto = producto;
        Calificacion = calificacion;
        EstadoAnterior = estadoAnterior;
    }

    public Guid Id { get; private set; }

    public int Orden { get; private set; }

    public Guid DepositoId { get; private set; }

    public string DepositoCodigo { get; private set; }

    public ClaseLineaBodega Clase { get; private set; }

    /// <summary>Litros con signo: positivos los que entran, negativos los que salen.</summary>
    public decimal Litros { get; private set; }

    public ProductoVinico Producto { get; private set; }

    public string? Calificacion { get; private set; }

    /// <summary>Estado del depósito antes de la operación (en la primera línea de cada depósito), en JSON.</summary>
    public string? EstadoAnterior { get; private set; }
}

/// <summary>
/// Operación de bodega: elaboración, trasiego, coupage, merma, embotellado o salida a granel. Mueve los litros (y la
/// composición) de sus depósitos y deja una línea por movimiento con la categoría de lo movido; con eso sale la
/// declaración de existencias. Se deshace (anula) mientras sea la última operación de todos sus depósitos.
/// </summary>
public sealed class OperacionBodega : RaizAgregadoEmpresa<Guid>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly List<LineaOperacionBodega> _lineas = [];
    private readonly HashSet<Guid> _guardados = [];

    private OperacionBodega(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private OperacionBodega(Guid id, Guid empresaId, int numero, DateOnly fecha, TipoOperacionBodega tipo, string? observaciones, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Ejercicio = fecha.Year;
        Numero = numero;
        Fecha = fecha;
        Tipo = tipo;
        Observaciones = observaciones;
        CreadaEn = ahora;
    }

    public int Ejercicio { get; private set; }

    public int Numero { get; private set; }

    public string NumeroCompleto => $"OB{Ejercicio}/{Numero:00000}";

    public DateOnly Fecha { get; private set; }

    public TipoOperacionBodega Tipo { get; private set; }

    public string? Observaciones { get; private set; }

    /// <summary>Litros perdidos en la operación (merma del trasiego, del embotellado…).</summary>
    public decimal MermaLitros { get; private set; }

    /// <summary>Kilos de uva elaborados (en la elaboración).</summary>
    public decimal? KilosUva { get; private set; }

    /// <summary>Artículo embotellado o vendido a granel.</summary>
    public Guid? ProductoId { get; private set; }

    public int? Botellas { get; private set; }

    public decimal? FormatoLitros { get; private set; }

    public string? Lote { get; private set; }

    public Guid? ClienteId { get; private set; }

    public Guid? AlbaranId { get; private set; }

    public string? AlbaranNumero { get; private set; }

    public bool Anulada { get; private set; }

    public string? MotivoAnulacion { get; private set; }

    public DateTimeOffset CreadaEn { get; private set; }

    public IReadOnlyList<LineaOperacionBodega> Lineas => _lineas;

    public static Resultado<OperacionBodega> Crear(Guid empresaId, int numero, DateOnly fecha, TipoOperacionBodega tipo, string? observaciones, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        var o = ReglasBodega.Texto(observaciones);
        if (o?.Length > ReglasBodega.LongitudObservaciones)
        {
            return Resultado.Fallo<OperacionBodega>(Error.Validacion("bodega.observaciones", $"Las observaciones admiten hasta {ReglasBodega.LongitudObservaciones} caracteres."));
        }

        return Resultado.Ok(new OperacionBodega(Guid.NewGuid(), empresaId, numero, fecha, tipo, o, reloj.AhoraUtc));
    }

    /// <summary>Saca litros de un depósito (o los pierde, con <see cref="ClaseLineaBodega.Merma"/>) y devuelve su composición.</summary>
    public Resultado<List<ComponenteVino>> Sacar(Deposito deposito, decimal litros, ClaseLineaBodega clase = ClaseLineaBodega.Sale)
    {
        ArgumentNullException.ThrowIfNull(deposito);
        if (deposito.Vacio)
        {
            return Resultado.Fallo<List<ComponenteVino>>(Error.Conflicto("deposito.sin_litros", $"El depósito {deposito.Codigo} está vacío."));
        }

        var antes = Guardar(deposito);
        var producto = deposito.Producto!.Value;
        var calificacion = deposito.Calificacion;
        var r = deposito.Sacar(litros, Id);
        if (r.EsCorrecto)
        {
            _lineas.Add(new LineaOperacionBodega(_lineas.Count + 1, deposito, clase == ClaseLineaBodega.Merma ? ClaseLineaBodega.Merma : ClaseLineaBodega.Sale, -litros,
                producto, calificacion, antes));
        }

        return r;
    }

    /// <summary>Mete litros en un depósito con su composición (y reclasifica lo que había si cambia su categoría).</summary>
    public Resultado Meter(Deposito deposito, decimal litros, ProductoVinico producto, string? calificacion, IReadOnlyList<ComponenteVino> composicion)
    {
        ArgumentNullException.ThrowIfNull(deposito);
        ArgumentNullException.ThrowIfNull(composicion);
        // La composición suma exactamente los litros que entran (el céntimo de litro que falte o sobre, al componente mayor).
        var diferencia = litros - composicion.Sum(c => c.Litros);
        if (composicion.Count == 0 || Math.Abs(diferencia) > 0.05m)
        {
            return Resultado.Fallo(Error.Validacion("bodega.composicion", "La composición no suma los litros que entran."));
        }

        if (diferencia != 0m)
        {
            var lista = composicion.ToList();
            var mayor = lista.IndexOf(lista.MaxBy(c => c.Litros)!);
            lista[mayor] = lista[mayor] with { Litros = lista[mayor].Litros + diferencia };
            composicion = lista;
        }

        var antes = Guardar(deposito);
        var r = deposito.Entrar(litros, producto, calificacion, composicion, Id);
        if (r.EsFallo)
        {
            return Resultado.Fallo(r.Error);
        }

        if (r.Valor is { } previo)
        {
            _lineas.Add(new LineaOperacionBodega(_lineas.Count + 1, deposito, ClaseLineaBodega.Reclasifica, -previo.Litros, previo.Producto, previo.Calificacion, antes));
            antes = null;
            _lineas.Add(new LineaOperacionBodega(_lineas.Count + 1, deposito, ClaseLineaBodega.Reclasifica, previo.Litros, deposito.Producto!.Value, deposito.Calificacion, null));
        }

        _lineas.Add(new LineaOperacionBodega(_lineas.Count + 1, deposito, ClaseLineaBodega.Entra, litros, deposito.Producto!.Value, deposito.Calificacion, antes));
        return Resultado.Ok();
    }

    public void Elaboracion(decimal kilos) => KilosUva = kilos;

    public void Merma(decimal litros) => MermaLitros = litros;

    public void Embotellado(Guid productoId, int botellas, decimal formato, string lote)
    {
        ProductoId = productoId;
        Botellas = botellas;
        FormatoLitros = formato;
        Lote = lote;
    }

    public void Granel(Guid clienteId, Guid productoId)
    {
        ClienteId = clienteId;
        ProductoId = productoId;
    }

    public void Albaran(Guid albaranId, string numero)
    {
        AlbaranId = albaranId;
        AlbaranNumero = numero;
    }

    /// <summary>Depósitos de la operación con su estado anterior.</summary>
    public IReadOnlyList<(Guid DepositoId, EstadoDeposito Estado)> EstadosAnteriores() =>
        _lineas.Where(l => l.EstadoAnterior is not null).Select(l => (l.DepositoId, JsonSerializer.Deserialize<EstadoDeposito>(l.EstadoAnterior!, Json)!)).ToList();

    /// <summary>Deshace la operación: cada depósito vuelve a su estado anterior, si esta sigue siendo su última operación.</summary>
    public Resultado Anular(string? motivo, IReadOnlyDictionary<Guid, Deposito> depositos)
    {
        ArgumentNullException.ThrowIfNull(depositos);
        if (Anulada)
        {
            return Resultado.Fallo(Error.Conflicto("operacion_bodega.anulada", "La operación ya está anulada."));
        }

        var m = ReglasBodega.Texto(motivo);
        if (m is null)
        {
            return Resultado.Fallo(Error.Validacion("operacion_bodega.motivo", "Indica el motivo de la anulación."));
        }

        var estados = EstadosAnteriores();
        foreach (var (id, _) in estados)
        {
            if (!depositos.TryGetValue(id, out var d) || d.UltimaOperacionId != Id)
            {
                var codigo = depositos.TryGetValue(id, out var x) ? x.Codigo : "?";
                return Resultado.Fallo(Error.Conflicto("operacion_bodega.posterior",
                    $"El depósito {codigo} ha tenido operaciones después: deshaz antes las posteriores."));
            }
        }

        foreach (var (id, estado) in estados)
        {
            depositos[id].Restaurar(estado);
        }

        Anulada = true;
        MotivoAnulacion = m.Length > 200 ? m[..200] : m;
        return Resultado.Ok();
    }

    private string? Guardar(Deposito d) => _guardados.Add(d.Id) ? JsonSerializer.Serialize(d.Estado(), Json) : null;
}
