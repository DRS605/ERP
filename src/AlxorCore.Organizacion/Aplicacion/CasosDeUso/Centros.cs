using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Organizacion.Dominio;

namespace AlxorCore.Organizacion.Aplicacion.CasosDeUso;

public sealed record DatosCentro(string? Codigo, string? Nombre, string? Direccion = null, Guid? AlmacenId = null, bool Activo = true);

public sealed record DatosCaja(string? Codigo, string? Nombre, bool Activa = true);

public sealed record CajaDto(Guid Id, string Codigo, string Nombre, bool Activa);

public sealed record CentroDto(Guid Id, string Codigo, string Nombre, string? Direccion, Guid? AlmacenId, bool Activo, IReadOnlyList<CajaDto> Cajas, int Usuarios)
{
    public static CentroDto De(Centro c, int usuarios) => new(c.Id, c.Codigo, c.Nombre, c.Direccion, c.AlmacenId, c.Activo,
        c.Cajas.OrderBy(x => x.Codigo, StringComparer.Ordinal).Select(x => new CajaDto(x.Id, x.Codigo, x.Nombre, x.Activa)).ToList(), usuarios);
}

/// <summary>Centros con que trabaja un usuario (vacío: todos).</summary>
public sealed record AccesosUsuarioDto(Guid UsuarioId, IReadOnlyList<Guid> Centros);

/// <summary>Centros de trabajo de la empresa, sus cajas y qué usuarios trabajan con cada uno.</summary>
public sealed class GestionCentros
{
    private readonly IRepositorioCentros _repo;
    private readonly IUnidadDeTrabajoOrganizacion _uow;
    private readonly IRepositorioMembresias _membresias;
    private readonly IReloj _reloj;
    private readonly IRepositorioAsignacionesSerie? _series;

    public GestionCentros(IRepositorioCentros repo, IUnidadDeTrabajoOrganizacion uow, IRepositorioMembresias membresias, IReloj reloj,
        IRepositorioAsignacionesSerie? series = null)
    {
        _series = series;
        _repo = repo;
        _uow = uow;
        _membresias = membresias;
        _reloj = reloj;
    }

    public async Task<IReadOnlyList<CentroDto>> ListarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var usuarios = (await _repo.AccesosAsync(empresaId, null, ct).ConfigureAwait(false)).GroupBy(a => a.CentroId).ToDictionary(g => g.Key, g => g.Count());
        return (await _repo.ListarAsync(empresaId, ct).ConfigureAwait(false)).OrderBy(c => c.Codigo, StringComparer.Ordinal)
            .Select(c => CentroDto.De(c, usuarios.GetValueOrDefault(c.Id))).ToList();
    }

    public async Task<Resultado<CentroDto>> CrearAsync(Guid empresaId, DatosCentro datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var c = Centro.Crear(empresaId, datos.Codigo, datos.Nombre, datos.Direccion, datos.AlmacenId, _reloj);
        if (c.EsFallo)
        {
            return Resultado.Fallo<CentroDto>(c.Error);
        }

        if ((await _repo.ListarAsync(empresaId, ct).ConfigureAwait(false)).Any(x => x.Codigo == c.Valor.Codigo))
        {
            return Resultado.Fallo<CentroDto>(Error.Conflicto("centro.duplicado", $"Ya hay un centro {c.Valor.Codigo}."));
        }

        _repo.Agregar(c.Valor);
        await _uow.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(CentroDto.De(c.Valor, 0));
    }

    public async Task<Resultado<CentroDto>> CambiarAsync(Guid empresaId, Guid id, DatosCentro datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var c = await _repo.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (c is null)
        {
            return Resultado.Fallo<CentroDto>(Error.NoEncontrado("centro.no_encontrado", "El centro no existe."));
        }

        var r = c.Cambiar(datos.Nombre, datos.Direccion, datos.AlmacenId, datos.Activo);
        if (r.EsFallo)
        {
            return Resultado.Fallo<CentroDto>(r.Error);
        }

        await _uow.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(CentroDto.De(c, (await _repo.AccesosAsync(empresaId, null, ct).ConfigureAwait(false)).Count(a => a.CentroId == id)));
    }

    public async Task<Resultado<CentroDto>> AgregarCajaAsync(Guid empresaId, Guid id, DatosCaja datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var c = await _repo.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (c is null)
        {
            return Resultado.Fallo<CentroDto>(Error.NoEncontrado("centro.no_encontrado", "El centro no existe."));
        }

        var r = c.AgregarCaja(datos.Codigo, datos.Nombre);
        if (r.EsFallo)
        {
            return Resultado.Fallo<CentroDto>(r.Error);
        }

        await _uow.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(CentroDto.De(c, (await _repo.AccesosAsync(empresaId, null, ct).ConfigureAwait(false)).Count(a => a.CentroId == id)));
    }

    public async Task<Resultado<CentroDto>> CambiarCajaAsync(Guid empresaId, Guid id, Guid cajaId, DatosCaja datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var c = await _repo.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (c is null)
        {
            return Resultado.Fallo<CentroDto>(Error.NoEncontrado("centro.no_encontrado", "El centro no existe."));
        }

        var r = c.CambiarCaja(cajaId, datos.Nombre, datos.Activa);
        if (r.EsFallo)
        {
            return Resultado.Fallo<CentroDto>(r.Error);
        }

        await _uow.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(CentroDto.De(c, (await _repo.AccesosAsync(empresaId, null, ct).ConfigureAwait(false)).Count(a => a.CentroId == id)));
    }

    /// <summary>Borra un centro sin documentos (con sus cajas, sus accesos y sus series); con documentos, se da de baja.</summary>
    public async Task<Resultado> EliminarAsync(Guid id, AlxorCore.Nucleo.Aplicacion.IComprobadorUso? uso, CancellationToken ct = default)
    {
        var c = await _repo.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (c is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("centro.no_encontrado", "El centro no existe."));
        }

        if (uso is not null && await uso.BuscarUsoAsync(AlxorCore.Nucleo.Aplicacion.TiposRegistro.Centro, id, ct).ConfigureAwait(false) is { } en)
        {
            return Resultado.Fallo(AlxorCore.Nucleo.Aplicacion.Bajas.EnUso("centro", $"el centro {c.Codigo}", en));
        }

        if (_series is not null)
        {
            var propios = c.Cajas.Select(k => k.Id).Append(c.Id).ToHashSet();
            foreach (var a in (await _series.ListarAsync(c.EmpresaId, ct).ConfigureAwait(false))
                         .Where(a => a.Ambito is AmbitoSerie.Centro or AmbitoSerie.Caja && propios.Contains(a.TerceroId)))
            {
                _series.Eliminar(a);
            }
        }

        foreach (var a in await _repo.AccesosAsync(c.EmpresaId, null, ct).ConfigureAwait(false))
        {
            if (a.CentroId == id)
            {
                _repo.Eliminar(a);
            }
        }

        _repo.Eliminar(c);
        await _uow.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    public async Task<IReadOnlyList<AccesosUsuarioDto>> AccesosAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.AccesosAsync(empresaId, null, ct).ConfigureAwait(false)).GroupBy(a => a.UsuarioId)
            .Select(g => new AccesosUsuarioDto(g.Key, g.Select(a => a.CentroId).ToList())).ToList();

    /// <summary>Fija los centros con que trabaja un usuario de la empresa (lista vacía: todos).</summary>
    public async Task<Resultado<AccesosUsuarioDto>> FijarAccesosAsync(Guid empresaId, Guid usuarioId, IReadOnlyList<Guid>? centros, CancellationToken ct = default)
    {
        if (await _membresias.ObtenerAsync(usuarioId, empresaId, ct).ConfigureAwait(false) is null)
        {
            return Resultado.Fallo<AccesosUsuarioDto>(Error.NoEncontrado("centro.usuario_no_encontrado", "El usuario no es de la empresa."));
        }

        var existentes = (await _repo.ListarAsync(empresaId, ct).ConfigureAwait(false)).Select(c => c.Id).ToHashSet();
        var pedidos = (centros ?? []).Distinct().ToList();
        if (pedidos.Any(c => !existentes.Contains(c)))
        {
            return Resultado.Fallo<AccesosUsuarioDto>(Error.Validacion("centro.no_encontrado", "Alguno de los centros no existe."));
        }

        var actuales = await _repo.AccesosAsync(empresaId, usuarioId, ct).ConfigureAwait(false);
        foreach (var a in actuales.Where(a => !pedidos.Contains(a.CentroId)))
        {
            _repo.Eliminar(a);
        }

        foreach (var c in pedidos.Where(c => actuales.All(a => a.CentroId != c)))
        {
            _repo.Agregar(AccesoCentro.Crear(empresaId, usuarioId, c, _reloj));
        }

        await _uow.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new AccesosUsuarioDto(usuarioId, pedidos));
    }
}
