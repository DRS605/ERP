namespace AlxorCore.Nucleo.Aplicacion;

/// <summary>Caja (punto de venta) de un centro.</summary>
public sealed record CajaInfo(Guid Id, string Codigo, string Nombre, bool Activa);

/// <summary>Centro de trabajo de la empresa, con su almacén habitual y sus cajas.</summary>
public sealed record CentroInfo(Guid Id, string Codigo, string Nombre, bool Activo, Guid? AlmacenId, IReadOnlyList<CajaInfo> Cajas);

/// <summary>Consulta de los centros de la empresa y de los que puede usar cada usuario (la implementa Organización).</summary>
public interface IConsultaCentros
{
    Task<CentroInfo?> ObtenerAsync(Guid centroId, CancellationToken ct = default);

    Task<IReadOnlyList<CentroInfo>> ListarAsync(Guid empresaId, CancellationToken ct = default);

    /// <summary>Centros con que puede trabajar el usuario; <c>null</c> si no tiene ninguno asignado (trabaja con todos).</summary>
    Task<IReadOnlyCollection<Guid>?> PermitidosAsync(Guid empresaId, Guid usuarioId, CancellationToken ct = default);
}
