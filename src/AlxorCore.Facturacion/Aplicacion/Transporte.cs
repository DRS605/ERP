using AlxorCore.Facturacion.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Facturacion.Aplicacion;

public sealed record TransportistaDto(Guid Id, string Nombre, string? Nif, string? Direccion, string? Pais, string? Telefono, bool Activo)
{
    public static TransportistaDto Desde(Transportista t) => new(t.Id, t.Nombre, t.Nif, t.Direccion, t.Pais, t.Telefono, t.Activo);
}

public sealed record VehiculoDto(Guid Id, string Matricula, string? MatriculaRemolque, string? Descripcion, decimal? TaraKg, bool Frigorifico, Guid? TransportistaId, bool Activo)
{
    public static VehiculoDto Desde(Vehiculo v) => new(v.Id, v.Matricula, v.MatriculaRemolque, v.Descripcion, v.TaraKg, v.Frigorifico, v.TransportistaId, v.Activo);
}

public sealed record DatosTransportista(string? Nombre, string? Nif = null, string? Direccion = null, string? Pais = null, string? Telefono = null, bool Activo = true);

public sealed record DatosVehiculo(string? Matricula, string? MatriculaRemolque = null, string? Descripcion = null, decimal? TaraKg = null, bool Frigorifico = false,
    Guid? TransportistaId = null, bool Activo = true);

public interface IRepositorioTransporte
{
    Task<IReadOnlyList<Transportista>> TransportistasAsync(Guid empresaId, CancellationToken ct = default);

    Task<Transportista?> TransportistaAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Vehiculo>> VehiculosAsync(Guid empresaId, CancellationToken ct = default);

    Task<Vehiculo?> VehiculoAsync(Guid id, CancellationToken ct = default);

    void Agregar(object entidad);

    void Eliminar(object entidad);

    /// <summary>Cartas de porte que lo usan (para no eliminar lo usado).</summary>
    Task<int> CartasConAsync(Guid? transportistaId, Guid? vehiculoId, CancellationToken ct = default);
}

/// <summary>Transportistas y vehículos habituales: se proponen en la carta de porte y en la expedición.</summary>
public sealed class GestionTransporte
{
    private readonly IRepositorioTransporte _repo;
    private readonly IUnidadDeTrabajoFacturacion _unidad;

    public GestionTransporte(IRepositorioTransporte repo, IUnidadDeTrabajoFacturacion unidad) { _repo = repo; _unidad = unidad; }

    public async Task<IReadOnlyList<TransportistaDto>> TransportistasAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.TransportistasAsync(empresaId, ct).ConfigureAwait(false)).OrderBy(t => t.Nombre, StringComparer.CurrentCulture).Select(TransportistaDto.Desde).ToList();

    public async Task<IReadOnlyList<VehiculoDto>> VehiculosAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.VehiculosAsync(empresaId, ct).ConfigureAwait(false)).OrderBy(v => v.Matricula, StringComparer.Ordinal).Select(VehiculoDto.Desde).ToList();

    public async Task<Resultado<TransportistaDto>> CrearTransportistaAsync(Guid empresaId, DatosTransportista d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var t = Transportista.Crear(empresaId, d.Nombre, d.Nif, d.Direccion, d.Pais, d.Telefono);
        if (t.EsFallo)
        {
            return Resultado.Fallo<TransportistaDto>(t.Error);
        }

        _repo.Agregar(t.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(TransportistaDto.Desde(t.Valor));
    }

    public async Task<Resultado<TransportistaDto>> ModificarTransportistaAsync(Guid empresaId, Guid id, DatosTransportista d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        if (await _repo.TransportistaAsync(id, ct).ConfigureAwait(false) is not { } t || t.EmpresaId != empresaId)
        {
            return Resultado.Fallo<TransportistaDto>(Error.NoEncontrado("transportista.no_encontrado", "El transportista no existe."));
        }

        var r = t.Modificar(d.Nombre, d.Nif, d.Direccion, d.Pais, d.Telefono, d.Activo);
        if (r.EsFallo)
        {
            return Resultado.Fallo<TransportistaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(TransportistaDto.Desde(t));
    }

    public async Task<Resultado<VehiculoDto>> CrearVehiculoAsync(Guid empresaId, DatosVehiculo d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        if (await ValidarAsync(empresaId, d, null, ct).ConfigureAwait(false) is { } error)
        {
            return Resultado.Fallo<VehiculoDto>(error);
        }

        var v = Vehiculo.Crear(empresaId, d.Matricula, d.MatriculaRemolque, d.Descripcion, d.TaraKg, d.Frigorifico, d.TransportistaId);
        if (v.EsFallo)
        {
            return Resultado.Fallo<VehiculoDto>(v.Error);
        }

        _repo.Agregar(v.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(VehiculoDto.Desde(v.Valor));
    }

    public async Task<Resultado<VehiculoDto>> ModificarVehiculoAsync(Guid empresaId, Guid id, DatosVehiculo d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        if (await _repo.VehiculoAsync(id, ct).ConfigureAwait(false) is not { } v || v.EmpresaId != empresaId)
        {
            return Resultado.Fallo<VehiculoDto>(Error.NoEncontrado("vehiculo.no_encontrado", "El vehículo no existe."));
        }

        if (await ValidarAsync(empresaId, d, id, ct).ConfigureAwait(false) is { } error)
        {
            return Resultado.Fallo<VehiculoDto>(error);
        }

        var r = v.Modificar(d.Matricula, d.MatriculaRemolque, d.Descripcion, d.TaraKg, d.Frigorifico, d.TransportistaId, d.Activo);
        if (r.EsFallo)
        {
            return Resultado.Fallo<VehiculoDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(VehiculoDto.Desde(v));
    }

    /// <summary>Elimina un transportista o un vehículo que ninguna carta de porte usa (si no, se da de baja).</summary>
    public async Task<Resultado> EliminarAsync(Guid empresaId, Guid? transportistaId, Guid? vehiculoId, CancellationToken ct = default)
    {
        object? entidad = transportistaId is { } t ? await _repo.TransportistaAsync(t, ct).ConfigureAwait(false)
            : vehiculoId is { } v ? await _repo.VehiculoAsync(v, ct).ConfigureAwait(false) : null;
        if (entidad is not AlxorCore.Nucleo.Multiempresa.IEntidadEmpresa e || e.EmpresaId != empresaId)
        {
            return Resultado.Fallo(Error.NoEncontrado("transporte.no_encontrado", "No existe."));
        }

        if (await _repo.CartasConAsync(transportistaId, vehiculoId, ct).ConfigureAwait(false) > 0)
        {
            return Resultado.Fallo(Error.Conflicto("transporte.en_uso", "Lo usan cartas de porte: dalo de baja en lugar de eliminarlo."));
        }

        if (transportistaId is { } tid && (await _repo.VehiculosAsync(empresaId, ct).ConfigureAwait(false)).Any(x => x.TransportistaId == tid))
        {
            return Resultado.Fallo(Error.Conflicto("transporte.en_uso", "Tiene vehículos: quítaselos o dalo de baja."));
        }

        _repo.Eliminar(entidad);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    private async Task<Error?> ValidarAsync(Guid empresaId, DatosVehiculo d, Guid? id, CancellationToken ct)
    {
        if (d.TransportistaId is { } t && t != Guid.Empty)
        {
            var tr = await _repo.TransportistaAsync(t, ct).ConfigureAwait(false);
            if (tr is null || tr.EmpresaId != empresaId)
            {
                return Error.Validacion("vehiculo.transportista", "El transportista no existe.");
            }
        }

        var m = Vehiculo.NormalizarMatricula(d.Matricula);
        return (await _repo.VehiculosAsync(empresaId, ct).ConfigureAwait(false)).Any(v => v.Id != id && v.Matricula == m)
            ? Error.Conflicto("vehiculo.duplicado", $"Ya hay un vehículo con la matrícula {m}.")
            : null;
    }
}
