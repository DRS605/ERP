using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Analisis.Dominio;

/// <summary>
/// Informe de análisis guardado: la definición (conjunto de datos, dimensiones, medidas, filtros, periodo relativo,
/// gráfico…) tal cual la guarda el diseñador, en JSON. Es de quien lo crea; si está <see cref="Compartido"/>, lo ve
/// toda la empresa (pero solo su autor lo cambia o lo borra).
/// </summary>
public sealed class InformeAnalisis : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMaximaNombre = 120;
    public const int LongitudMaximaDefinicion = 32_000;

    private InformeAnalisis(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
        Nombre = null!;
        Dataset = null!;
        Definicion = null!;
    }

    public Guid UsuarioId { get; private set; }

    public string Nombre { get; private set; }

    public string Dataset { get; private set; }

    public string Definicion { get; private set; }

    public bool Compartido { get; private set; }

    public bool Favorito { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset ActualizadoEn { get; private set; }

    public static Resultado<InformeAnalisis> Crear(Guid empresaId, Guid usuarioId, string? nombre, string? dataset, string? definicion, bool compartido, bool favorito, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        var informe = new InformeAnalisis(Guid.NewGuid(), empresaId) { UsuarioId = usuarioId, CreadoEn = reloj.AhoraUtc };
        var r = informe.Actualizar(nombre, dataset, definicion, compartido, favorito, reloj);
        return r.EsFallo ? Resultado.Fallo<InformeAnalisis>(r.Error) : Resultado.Ok(informe);
    }

    public Resultado Actualizar(string? nombre, string? dataset, string? definicion, bool compartido, bool favorito, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        var n = (nombre ?? string.Empty).Trim();
        if (n.Length == 0 || n.Length > LongitudMaximaNombre)
        {
            return Resultado.Fallo(Error.Validacion("informe.nombre", $"El nombre es obligatorio y de hasta {LongitudMaximaNombre} caracteres."));
        }

        if (string.IsNullOrWhiteSpace(dataset) || dataset.Length > 40)
        {
            return Resultado.Fallo(Error.Validacion("informe.dataset", "Falta el conjunto de datos."));
        }

        if (string.IsNullOrWhiteSpace(definicion) || definicion.Length > LongitudMaximaDefinicion)
        {
            return Resultado.Fallo(Error.Validacion("informe.definicion", "La definición del informe falta o es demasiado grande."));
        }

        Nombre = n;
        Dataset = dataset.Trim();
        Definicion = definicion;
        Compartido = compartido;
        Favorito = favorito;
        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }
}
