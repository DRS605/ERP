using AlxorCore.Organizacion.Aplicacion.Modelos;

namespace AlxorCore.Organizacion.Aplicacion.Puertos;

/// <summary>Consultas de lectura optimizadas del módulo Organización.</summary>
public interface IConsultasOrganizacion
{
    /// <summary>Lista las empresas en las que el usuario tiene una membresía activa, con su rol.</summary>
    Task<IReadOnlyList<EmpresaResumen>> ListarEmpresasDeUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
}

/// <summary>Consulta de una empresa por id (la usan otros módulos, p. ej. Documentos para el PDF).</summary>
public interface IConsultaEmpresas
{
    Task<EmpresaDto?> ObtenerAsync(Guid empresaId, CancellationToken ct = default);
}

/// <summary>
/// Consulta de formas de pago (la usan Facturación y Gastos para resolver vencimiento y pago
/// automático al emitir/registrar un documento).
/// </summary>
public interface IConsultaFormasPago
{
    Task<FormaPagoDto?> ObtenerAsync(Guid formaPagoId, CancellationToken ct = default);

    Task<IReadOnlyList<FormaPagoDto>> ListarAsync(Guid empresaId, bool incluirInactivas = false, CancellationToken ct = default);
}
