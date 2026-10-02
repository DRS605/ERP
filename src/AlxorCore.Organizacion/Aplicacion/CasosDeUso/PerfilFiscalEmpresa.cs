using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Organizacion.Dominio;

namespace AlxorCore.Organizacion.Aplicacion.CasosDeUso;

/// <summary>Caso de uso: leer y guardar la ficha fiscal de la empresa, y su calendario de vencimientos.</summary>
public sealed class PerfilFiscalEmpresa
{
    private readonly IRepositorioEmpresas _empresas;
    private readonly IUnidadDeTrabajoOrganizacion _unidad;
    private readonly IReloj _reloj;

    public PerfilFiscalEmpresa(IRepositorioEmpresas empresas, IUnidadDeTrabajoOrganizacion unidad, IReloj reloj)
    {
        _empresas = empresas;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<Resultado<PerfilFiscal>> ObtenerAsync(Guid empresaId, CancellationToken ct = default) =>
        await _empresas.ObtenerPorIdAsync(empresaId, ct).ConfigureAwait(false) is { } e
            ? Resultado.Ok(e.PerfilFiscal)
            : Resultado.Fallo<PerfilFiscal>(Error.NoEncontrado("empresa.no_encontrada", "La empresa no existe."));

    public async Task<Resultado<PerfilFiscal>> GuardarAsync(Guid empresaId, PerfilFiscal perfil, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(perfil);
        var empresa = await _empresas.ObtenerPorIdAsync(empresaId, ct).ConfigureAwait(false);
        if (empresa is null)
        {
            return Resultado.Fallo<PerfilFiscal>(Error.NoEncontrado("empresa.no_encontrada", "La empresa no existe."));
        }

        var r = empresa.EstablecerPerfilFiscal(perfil, _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<PerfilFiscal>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(empresa.PerfilFiscal);
    }

    public async Task<Resultado<IReadOnlyList<VencimientoFiscal>>> CalendarioAsync(Guid empresaId, int anio, CancellationToken ct = default)
    {
        var perfil = await ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        return perfil.EsFallo ? Resultado.Fallo<IReadOnlyList<VencimientoFiscal>>(perfil.Error) : Resultado.Ok(perfil.Valor.Calendario(anio));
    }
}
