using AlxorCore.Catalogo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Catalogo.Aplicacion;

/// <summary>Vista de un tipo de IVA configurable de la empresa.</summary>
public sealed record TipoIvaDto(
    Guid Id, string Codigo, string Nombre, decimal Porcentaje, decimal RecargoEquivalencia,
    ClaseIva Clase, string? MencionFactura, bool Activo, bool Repercute)
{
    public static TipoIvaDto Desde(TipoIva t) =>
        new(t.Id, t.Codigo, t.Nombre, t.Porcentaje, t.RecargoEquivalencia, t.Clase, t.MencionFactura, t.Activo, t.Clase.Repercute());
}

/// <summary>Datos para crear o actualizar un tipo de IVA.</summary>
public sealed record DatosTipoIva(string? Codigo, string? Nombre, decimal Porcentaje, decimal RecargoEquivalencia, ClaseIva Clase, string? MencionFactura);

/// <summary>Resultado de resolver un código de IVA en el catálogo de la empresa (para facturar).</summary>
public sealed record IvaResuelto(string Codigo, decimal Porcentaje, decimal RecargoEquivalencia, ClaseIva Clase, decimal PorcentajeRepercutido, string? MencionFactura);

/// <summary>Repositorio de tipos de IVA por empresa.</summary>
public interface IRepositorioTiposIva
{
    void Agregar(TipoIva tipo);

    Task<TipoIva?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    Task<TipoIva?> ObtenerPorCodigoAsync(Guid empresaId, string codigo, CancellationToken ct = default);

    Task<IReadOnlyList<TipoIva>> ListarAsync(Guid empresaId, CancellationToken ct = default);

    Task<bool> ExisteCodigoAsync(Guid empresaId, string codigo, CancellationToken ct = default);
}

/// <summary>
/// Resuelve un código de IVA en el catálogo de la empresa (lo usa Facturación al emitir). Devuelve
/// <c>null</c> si la empresa no tiene ese código configurado, para que el emisor use el respaldo
/// estatal fijo (compatibilidad con empresas aún sin catálogo propio sembrado).
/// </summary>
public interface IResolverIvaEmpresa
{
    Task<IvaResuelto?> ResolverAsync(Guid empresaId, string codigo, CancellationToken ct = default);
}

/// <summary>
/// Caso de uso: listar los tipos de IVA de la empresa. Si la empresa aún no tiene catálogo propio, lo
/// <b>siembra</b> con el conjunto estándar español (editable después).
/// </summary>
public sealed class ListarTiposIva
{
    private readonly IRepositorioTiposIva _tipos;
    private readonly IUnidadDeTrabajoCatalogo _uow;
    private readonly IReloj _reloj;

    public ListarTiposIva(IRepositorioTiposIva tipos, IUnidadDeTrabajoCatalogo uow, IReloj reloj)
    {
        _tipos = tipos;
        _uow = uow;
        _reloj = reloj;
    }

    public async Task<IReadOnlyList<TipoIvaDto>> EjecutarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var existentes = await _tipos.ListarAsync(empresaId, ct).ConfigureAwait(false);
        if (existentes.Count == 0)
        {
            foreach (var (codigo, nombre, porcentaje, recargo, clase, mencion) in TipoIva.Predeterminados)
            {
                var creado = TipoIva.Crear(empresaId, codigo, nombre, porcentaje, recargo, clase, mencion, _reloj);
                if (creado.EsCorrecto)
                {
                    _tipos.Agregar(creado.Valor);
                }
            }

            await _uow.GuardarCambiosAsync(ct).ConfigureAwait(false);
            existentes = await _tipos.ListarAsync(empresaId, ct).ConfigureAwait(false);
        }

        return existentes.Select(TipoIvaDto.Desde).ToList();
    }
}

/// <summary>Caso de uso: crear un tipo de IVA en la empresa.</summary>
public sealed class CrearTipoIva
{
    private readonly IRepositorioTiposIva _tipos;
    private readonly IUnidadDeTrabajoCatalogo _uow;
    private readonly IReloj _reloj;

    public CrearTipoIva(IRepositorioTiposIva tipos, IUnidadDeTrabajoCatalogo uow, IReloj reloj)
    {
        _tipos = tipos;
        _uow = uow;
        _reloj = reloj;
    }

    public async Task<Resultado<TipoIvaDto>> EjecutarAsync(Guid empresaId, DatosTipoIva datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);

        var tipo = TipoIva.Crear(empresaId, datos.Codigo, datos.Nombre, datos.Porcentaje, datos.RecargoEquivalencia, datos.Clase, datos.MencionFactura, _reloj);
        if (tipo.EsFallo)
        {
            return Resultado.Fallo<TipoIvaDto>(tipo.Error);
        }

        if (await _tipos.ExisteCodigoAsync(empresaId, tipo.Valor.Codigo, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<TipoIvaDto>(Error.Conflicto("tipoiva.codigo_duplicado", $"Ya existe un tipo de IVA con el código «{tipo.Valor.Codigo}»."));
        }

        _tipos.Agregar(tipo.Valor);
        await _uow.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(TipoIvaDto.Desde(tipo.Valor));
    }
}

/// <summary>Caso de uso: actualizar un tipo de IVA (nombre, porcentaje, clase, mención, activo).</summary>
public sealed class ActualizarTipoIva
{
    private readonly IRepositorioTiposIva _tipos;
    private readonly IUnidadDeTrabajoCatalogo _uow;
    private readonly IReloj _reloj;

    public ActualizarTipoIva(IRepositorioTiposIva tipos, IUnidadDeTrabajoCatalogo uow, IReloj reloj)
    {
        _tipos = tipos;
        _uow = uow;
        _reloj = reloj;
    }

    public async Task<Resultado<TipoIvaDto>> EjecutarAsync(Guid id, DatosTipoIva datos, bool activo, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);

        var tipo = await _tipos.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (tipo is null)
        {
            return Resultado.Fallo<TipoIvaDto>(Error.NoEncontrado("tipoiva.no_encontrado", "El tipo de IVA no existe."));
        }

        var r = tipo.Actualizar(datos.Nombre, datos.Porcentaje, datos.RecargoEquivalencia, datos.Clase, datos.MencionFactura, _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<TipoIvaDto>(r.Error);
        }

        tipo.FijarActivo(activo, _reloj);
        await _uow.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(TipoIvaDto.Desde(tipo));
    }
}
