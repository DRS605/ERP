using AlxorCore.Organizacion.Dominio;
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

    /// <summary>Empresas del grupo (holding), por razón social.</summary>
    Task<IReadOnlyList<EmpresaGrupoDto>> EmpresasDelGrupoAsync(Guid grupoId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<EmpresaGrupoDto>>([]);

    /// <summary>Grupo al que pertenece la empresa.</summary>
    Task<Guid?> GrupoDeEmpresaAsync(Guid empresaId, CancellationToken ct = default) => Task.FromResult<Guid?>(null);
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

/// <summary>Prorrata configurada para un ejercicio (null = la empresa no aplica prorrata: deduce el 100 %).</summary>
public sealed record ProrrataDto(int Ejercicio, RegimenProrrata Regimen, int PorcentajeProvisional, AlxorCore.Nucleo.Comun.TipoImpuesto Impuesto = AlxorCore.Nucleo.Comun.TipoImpuesto.Iva);

/// <summary>Consulta y configuración de la prorrata de IVA/IGIC por ejercicio.</summary>
public interface IConsultaProrrata
{
    /// <summary>Prorrata del ejercicio para un impuesto (IVA o IGIC).</summary>
    Task<ProrrataDto?> ObtenerAsync(Guid empresaId, int ejercicio, AlxorCore.Nucleo.Comun.TipoImpuesto impuesto, CancellationToken ct = default);
}
