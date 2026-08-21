using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Contabilidad.Dominio;

/// <summary>
/// Cuenta contable del Plan General Contable (PGC). El código es el número de cuenta (p. ej.
/// «600», «472», «400»); el grupo es su primer dígito. Es dato multiempresa: cada empresa tiene
/// su propio plan (sembrado con un subconjunto común y ampliable).
/// </summary>
public sealed class Cuenta : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMaximaCodigo = 12;
    public const int LongitudMaximaNombre = 150;

    private Cuenta(Guid id)
        : base(id, Guid.Empty)
    {
        Codigo = null!;
        Nombre = null!;
    }

    private Cuenta(Guid id, Guid empresaId, string codigo, string nombre)
        : base(id, empresaId)
    {
        Codigo = codigo;
        Nombre = nombre;
        Grupo = codigo.Length > 0 && char.IsDigit(codigo[0]) ? codigo[0] - '0' : 0;
    }

    /// <summary>Número de cuenta (PGC).</summary>
    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    /// <summary>Grupo contable (primer dígito del código): 1–9.</summary>
    public int Grupo { get; private set; }

    public static Resultado<Cuenta> Crear(Guid empresaId, string? codigo, string? nombre)
    {
        if (string.IsNullOrWhiteSpace(codigo) || !codigo.All(char.IsDigit))
        {
            return Resultado.Fallo<Cuenta>(Error.Validacion("cuenta.codigo_invalido", "El código de cuenta debe ser numérico."));
        }

        if (codigo.Length > LongitudMaximaCodigo)
        {
            return Resultado.Fallo<Cuenta>(Error.Validacion("cuenta.codigo_largo", $"El código supera {LongitudMaximaCodigo} dígitos."));
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            return Resultado.Fallo<Cuenta>(Error.Validacion("cuenta.nombre_vacio", "El nombre de la cuenta es obligatorio."));
        }

        return Resultado.Ok(new Cuenta(Guid.NewGuid(), empresaId, codigo.Trim(), nombre.Trim()));
    }
}
