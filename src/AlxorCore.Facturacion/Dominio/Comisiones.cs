using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Facturacion.Dominio;

/// <summary>Cuándo se gana la comisión.</summary>
public enum DevengoComision
{
    /// <summary>Al facturar.</summary>
    Facturado = 1,

    /// <summary>Al cobrar: en proporción a lo cobrado de cada factura.</summary>
    Cobrado = 2,
}

/// <summary>
/// Agente comercial o vendedor que cobra comisión por las ventas a sus clientes. Si es externo y factura sus comisiones,
/// se enlaza con su ficha de proveedor y la liquidación genera su factura (gasto con su retención).
/// </summary>
public sealed class AgenteComercial : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudNombre = 120;

    private AgenteComercial(Guid id) : base(id, Guid.Empty) => Nombre = null!;

    private AgenteComercial(Guid id, Guid empresaId) : base(id, empresaId) => Nombre = null!;

    public string Nombre { get; private set; }

    public Guid? ProveedorId { get; private set; }

    /// <summary>Comisión general (% sobre la base de la venta).</summary>
    public decimal Porcentaje { get; private set; }

    public DevengoComision Devengo { get; private set; }

    /// <summary>Retención de IRPF de su factura de comisiones (agentes profesionales).</summary>
    public decimal PorcentajeIrpf { get; private set; }

    public bool Activo { get; private set; }

    public static Resultado<AgenteComercial> Crear(Guid empresaId, string? nombre, decimal porcentaje, DevengoComision devengo, Guid? proveedorId, decimal porcentajeIrpf)
    {
        var a = new AgenteComercial(Guid.NewGuid(), empresaId) { Activo = true };
        var r = a.Cambiar(nombre, porcentaje, devengo, proveedorId, porcentajeIrpf, true);
        return r.EsFallo ? Resultado.Fallo<AgenteComercial>(r.Error) : Resultado.Ok(a);
    }

    public Resultado Cambiar(string? nombre, decimal porcentaje, DevengoComision devengo, Guid? proveedorId, decimal porcentajeIrpf, bool activo)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return Resultado.Fallo(Error.Validacion("agente.nombre", "Pon el nombre del agente."));
        }

        if (porcentaje is < 0m or > 100m || porcentajeIrpf is < 0m or > 50m)
        {
            return Resultado.Fallo(Error.Validacion("agente.porcentaje", "La comisión va de 0 a 100 % y la retención de 0 a 50 %."));
        }

        if (!Enum.IsDefined(devengo))
        {
            return Resultado.Fallo(Error.Validacion("agente.devengo", "La comisión se devenga al facturar o al cobrar."));
        }

        Nombre = nombre.Trim()[..Math.Min(nombre.Trim().Length, LongitudNombre)];
        Porcentaje = porcentaje;
        Devengo = devengo;
        ProveedorId = proveedorId;
        PorcentajeIrpf = porcentajeIrpf;
        Activo = activo;
        return Resultado.Ok();
    }
}

/// <summary>El agente que lleva un cliente desde una fecha (el de una factura es el asignado en su fecha).</summary>
public sealed class AsignacionAgente : RaizAgregadoEmpresa<Guid>
{
    private AsignacionAgente(Guid id) : base(id, Guid.Empty) { }

    private AsignacionAgente(Guid id, Guid empresaId) : base(id, empresaId) { }

    public Guid ClienteId { get; private set; }

    /// <summary>Agente (null: desde esa fecha el cliente no tiene agente).</summary>
    public Guid? AgenteId { get; private set; }

    public DateOnly Desde { get; private set; }

    public static AsignacionAgente Crear(Guid empresaId, Guid clienteId, Guid? agenteId, DateOnly desde) =>
        new(Guid.NewGuid(), empresaId) { ClienteId = clienteId, AgenteId = agenteId, Desde = desde };

    public void Cambiar(Guid? agenteId) => AgenteId = agenteId;
}

/// <summary>Comisión particular de un agente para una familia de artículos, un cliente o los dos (gana la más concreta).</summary>
public sealed class ReglaComision : RaizAgregadoEmpresa<Guid>
{
    private ReglaComision(Guid id) : base(id, Guid.Empty) { }

    private ReglaComision(Guid id, Guid empresaId) : base(id, empresaId) { }

    public Guid AgenteId { get; private set; }

    public Guid? FamiliaId { get; private set; }

    public Guid? ClienteId { get; private set; }

    public decimal Porcentaje { get; private set; }

    public static Resultado<ReglaComision> Crear(Guid empresaId, Guid agenteId, Guid? familiaId, Guid? clienteId, decimal porcentaje)
    {
        if (familiaId is null && clienteId is null)
        {
            return Resultado.Fallo<ReglaComision>(Error.Validacion("comision.regla", "La regla es para una familia, un cliente o los dos (la general va en el agente)."));
        }

        if (porcentaje is < 0m or > 100m)
        {
            return Resultado.Fallo<ReglaComision>(Error.Validacion("agente.porcentaje", "La comisión va de 0 a 100 %."));
        }

        return Resultado.Ok(new ReglaComision(Guid.NewGuid(), empresaId) { AgenteId = agenteId, FamiliaId = familiaId, ClienteId = clienteId, Porcentaje = porcentaje });
    }

    /// <summary>Cuánto de concreta es para esta venta (−1: no aplica): cliente y familia 3, cliente 2, familia 1.</summary>
    public int Encaje(Guid clienteId, Guid? familiaId) =>
        (ClienteId is null || ClienteId == clienteId) && (FamiliaId is null || FamiliaId == familiaId)
            ? (ClienteId is not null ? 2 : 0) + (FamiliaId is not null ? 1 : 0)
            : -1;
}

public enum EstadoLiquidacionAgente
{
    Emitida = 1,
    Anulada = 2,
}

public sealed class LineaLiquidacionAgente
{
    private LineaLiquidacionAgente() => Factura = null!;

    internal LineaLiquidacionAgente(Guid id, Guid facturaId, string factura, DateOnly fecha, string? cliente, decimal baseVenta, decimal porcentajeDevengado, decimal importe)
    {
        Id = id;
        FacturaId = facturaId;
        Factura = factura;
        Fecha = fecha;
        Cliente = cliente;
        BaseVenta = baseVenta;
        PorcentajeDevengado = porcentajeDevengado;
        Importe = importe;
    }

    public Guid Id { get; private set; }

    public Guid FacturaId { get; private set; }

    public string Factura { get; private set; }

    public DateOnly Fecha { get; private set; }

    public string? Cliente { get; private set; }

    /// <summary>Base de la venta (sin IVA) sobre la que se calcula la comisión.</summary>
    public decimal BaseVenta { get; private set; }

    /// <summary>Parte de la factura devengada (100 al facturar; al cobrar, lo cobrado).</summary>
    public decimal PorcentajeDevengado { get; private set; }

    /// <summary>Comisión de esta liquidación por la factura (lo devengado menos lo ya liquidado).</summary>
    public decimal Importe { get; private set; }
}

/// <summary>
/// Liquidación de comisiones de un agente en un periodo: congela, factura a factura, la comisión devengada y aún no
/// liquidada. Una factura no se liquida dos veces; las rectificativas restan.
/// </summary>
public sealed class LiquidacionAgente : RaizAgregadoEmpresa<Guid>
{
    private readonly List<LineaLiquidacionAgente> _lineas = [];

    private LiquidacionAgente(Guid id) : base(id, Guid.Empty) { }

    private LiquidacionAgente(Guid id, Guid empresaId) : base(id, empresaId) { }

    public Guid AgenteId { get; private set; }

    public int Ejercicio { get; private set; }

    public int Numero { get; private set; }

    public string NumeroCompleto => $"LAG-{Ejercicio}-{Numero:D5}";

    public DateOnly Fecha { get; private set; }

    public DateOnly Desde { get; private set; }

    public DateOnly Hasta { get; private set; }

    public EstadoLiquidacionAgente Estado { get; private set; }

    /// <summary>Factura del agente (gasto) si es externo y factura sus comisiones.</summary>
    public Guid? GastoId { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public IReadOnlyList<LineaLiquidacionAgente> Lineas => _lineas;

    public decimal Importe => Redondeo.Dos(_lineas.Sum(l => l.Importe));

    public static Resultado<LiquidacionAgente> Emitir(Guid empresaId, Guid agenteId, int numero, DateOnly fecha, DateOnly desde, DateOnly hasta,
        IReadOnlyList<(Guid FacturaId, string Factura, DateOnly Fecha, string? Cliente, decimal BaseVenta, decimal PorcentajeDevengado, decimal Importe)> lineas, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(lineas);
        ArgumentNullException.ThrowIfNull(reloj);
        if (hasta < desde)
        {
            return Resultado.Fallo<LiquidacionAgente>(Error.Validacion("liquidacion_agente.periodo", "El periodo está al revés."));
        }

        var con = lineas.Where(l => l.Importe != 0m).ToList();
        if (con.Count == 0)
        {
            return Resultado.Fallo<LiquidacionAgente>(Error.Validacion("liquidacion_agente.vacia", "No hay comisiones pendientes de liquidar en el periodo."));
        }

        var l = new LiquidacionAgente(Guid.NewGuid(), empresaId)
        {
            AgenteId = agenteId, Ejercicio = fecha.Year, Numero = numero, Fecha = fecha, Desde = desde, Hasta = hasta, Estado = EstadoLiquidacionAgente.Emitida,
            CreadoEn = reloj.AhoraUtc,
        };
        l._lineas.AddRange(con.Select(x => new LineaLiquidacionAgente(Guid.NewGuid(), x.FacturaId, x.Factura, x.Fecha, x.Cliente, x.BaseVenta, x.PorcentajeDevengado,
            Redondeo.Dos(x.Importe))));
        return Resultado.Ok(l);
    }

    public void AnotarGasto(Guid gastoId) => GastoId = gastoId;

    public Resultado Anular()
    {
        if (Estado == EstadoLiquidacionAgente.Anulada)
        {
            return Resultado.Fallo(Error.Conflicto("liquidacion_agente.anulada", "La liquidación ya está anulada."));
        }

        Estado = EstadoLiquidacionAgente.Anulada;
        return Resultado.Ok();
    }
}
