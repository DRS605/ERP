using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Agro.Dominio;

/// <summary>Estado de una reserva de palé.</summary>
public enum EstadoReservaPale
{
    Activa,

    /// <summary>El palé salió con su pedido (si se anula la expedición, vuelve a estar activa).</summary>
    Consumida,

    Anulada,
}

/// <summary>
/// Reserva de un palé (cerrado, sin expedir) para una línea de un pedido de venta, como las reservas de Hispatec: el palé
/// queda apartado para ese pedido, no sale con otro, y la expedición del pedido la consume. Un palé solo tiene una reserva
/// activa (lo garantiza un índice único en la base de datos).
/// </summary>
public sealed class ReservaPale : RaizAgregadoEmpresa<Guid>
{
    private ReservaPale(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private ReservaPale(Guid id, Guid empresaId, Guid paleId, Guid pedidoVentaId, Guid lineaPedidoId, decimal kilos, int cajas, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Cajas = cajas;
        PaleId = paleId;
        PedidoVentaId = pedidoVentaId;
        LineaPedidoId = lineaPedidoId;
        Kilos = kilos;
        Estado = EstadoReservaPale.Activa;
        CreadaEn = ahora;
    }

    public Guid PaleId { get; private set; }

    public Guid PedidoVentaId { get; private set; }

    public Guid LineaPedidoId { get; private set; }

    /// <summary>Kilos del palé al reservarlo.</summary>
    public decimal Kilos { get; private set; }

    /// <summary>Cajas del palé al reservarlo.</summary>
    public int Cajas { get; private set; }

    public EstadoReservaPale Estado { get; private set; }

    public DateTimeOffset CreadaEn { get; private set; }

    public DateOnly? ConsumidaEl { get; private set; }

    public string? MotivoAnulacion { get; private set; }

    public static ReservaPale Crear(Guid empresaId, Guid paleId, Guid pedidoVentaId, Guid lineaPedidoId, decimal kilos, int cajas, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        return new ReservaPale(Guid.NewGuid(), empresaId, paleId, pedidoVentaId, lineaPedidoId, kilos, cajas, reloj.AhoraUtc);
    }

    public void Consumir(DateOnly fecha)
    {
        if (Estado == EstadoReservaPale.Activa)
        {
            Estado = EstadoReservaPale.Consumida;
            ConsumidaEl = fecha;
        }
    }

    /// <summary>Se anuló la expedición del palé: la reserva vuelve a estar activa.</summary>
    public void Reactivar()
    {
        if (Estado == EstadoReservaPale.Consumida)
        {
            Estado = EstadoReservaPale.Activa;
            ConsumidaEl = null;
        }
    }

    public Resultado Anular(string? motivo)
    {
        if (Estado != EstadoReservaPale.Activa)
        {
            return Resultado.Fallo(Error.Conflicto("reserva.no_activa", "Solo se anula una reserva activa."));
        }

        Estado = EstadoReservaPale.Anulada;
        MotivoAnulacion = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim()[..Math.Min(motivo.Trim().Length, 200)];
        return Resultado.Ok();
    }
}
