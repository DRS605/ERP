using AlxorCore.Nucleo.Resultados;
using AlxorCore.Organizacion.Aplicacion.Modelos;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Organizacion.Dominio;

namespace AlxorCore.Organizacion.Aplicacion;

/// <summary>Datos de una forma de pago (crear/actualizar).</summary>
public sealed record DatosFormaPago(string Nombre, bool GeneraVencimiento, int DiasVencimiento, bool RegistrarPagoAutomatico);

/// <summary>Lista las formas de pago de la empresa activa.</summary>
public sealed class ListarFormasPago
{
    private readonly IConsultaFormasPago _consulta;

    public ListarFormasPago(IConsultaFormasPago consulta) => _consulta = consulta;

    public Task<IReadOnlyList<FormaPagoDto>> EjecutarAsync(Guid empresaId, bool incluirInactivas = false, CancellationToken ct = default) =>
        _consulta.ListarAsync(empresaId, incluirInactivas, ct);
}

/// <summary>Crea o actualiza una forma de pago.</summary>
public sealed class GuardarFormaPago
{
    private readonly IRepositorioFormasPago _formas;
    private readonly IUnidadDeTrabajoOrganizacion _unidad;

    public GuardarFormaPago(IRepositorioFormasPago formas, IUnidadDeTrabajoOrganizacion unidad)
    {
        _formas = formas;
        _unidad = unidad;
    }

    public async Task<Resultado<FormaPagoDto>> EjecutarAsync(Guid empresaId, Guid? id, DatosFormaPago datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);

        FormaPago forma;
        if (id is { } formaId)
        {
            var existente = await _formas.ObtenerAsync(formaId, ct).ConfigureAwait(false);
            if (existente is null || existente.EmpresaId != empresaId)
            {
                return Resultado.Fallo<FormaPagoDto>(Error.NoEncontrado("forma_pago.no_encontrada", "La forma de pago no existe."));
            }

            var r = existente.Actualizar(datos.Nombre, datos.GeneraVencimiento, datos.DiasVencimiento, datos.RegistrarPagoAutomatico);
            if (r.EsFallo)
            {
                return Resultado.Fallo<FormaPagoDto>(r.Error);
            }

            forma = existente;
        }
        else
        {
            var creada = FormaPago.Crear(empresaId, datos.Nombre, datos.GeneraVencimiento, datos.DiasVencimiento, datos.RegistrarPagoAutomatico);
            if (creada.EsFallo)
            {
                return Resultado.Fallo<FormaPagoDto>(creada.Error);
            }

            forma = creada.Valor;
            _formas.Agregar(forma);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(FormaPagoDto.Desde(forma));
    }
}

/// <summary>Desactiva una forma de pago (no se borra para preservar el histórico de documentos).</summary>
public sealed class EliminarFormaPago
{
    private readonly IRepositorioFormasPago _formas;
    private readonly IUnidadDeTrabajoOrganizacion _unidad;

    public EliminarFormaPago(IRepositorioFormasPago formas, IUnidadDeTrabajoOrganizacion unidad)
    {
        _formas = formas;
        _unidad = unidad;
    }

    public async Task<Resultado> EjecutarAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        var forma = await _formas.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (forma is null || forma.EmpresaId != empresaId)
        {
            return Resultado.Fallo(Error.NoEncontrado("forma_pago.no_encontrada", "La forma de pago no existe."));
        }

        forma.Desactivar();
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }
}
