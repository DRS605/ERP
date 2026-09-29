using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Agro.Dominio;

/// <summary>Situación de un producto en el Registro Oficial de Productos Fitosanitarios.</summary>
public enum EstadoFitosanitario
{
    Autorizado,
    Suspendido,
    Caducado,
    Cancelado,
}

/// <summary>Materia activa de un producto, con su riqueza (p. ej. «10 % [EC] p/v»).</summary>
public sealed class MateriaActivaFito
{
    private MateriaActivaFito()
    {
        Nombre = null!;
    }

    internal MateriaActivaFito(string nombre, string? riqueza)
    {
        Id = Guid.NewGuid();
        Nombre = nombre;
        Riqueza = riqueza;
    }

    public Guid Id { get; private set; }

    public string Nombre { get; private set; }

    public string? Riqueza { get; private set; }
}

/// <summary>Uso autorizado: en qué cultivo, contra qué plaga, a qué dosis, con qué plazo de seguridad y cuántas aplicaciones.</summary>
public sealed class UsoFito
{
    private UsoFito()
    {
        Cultivo = null!;
        Plaga = null!;
    }

    internal UsoFito(string cultivo, string plaga, decimal? dosisMinima, decimal? dosisMaxima, string? unidadDosis, int? plazoSeguridadDias,
        int? aplicaciones, string? observaciones)
    {
        Id = Guid.NewGuid();
        Cultivo = cultivo;
        Plaga = plaga;
        DosisMinima = dosisMinima;
        DosisMaxima = dosisMaxima;
        UnidadDosis = unidadDosis;
        PlazoSeguridadDias = plazoSeguridadDias;
        Aplicaciones = aplicaciones;
        Observaciones = observaciones;
    }

    public Guid Id { get; private set; }

    public string Cultivo { get; private set; }

    /// <summary>Plaga, enfermedad o efecto (el «agente» del registro).</summary>
    public string Plaga { get; private set; }

    public decimal? DosisMinima { get; private set; }

    public decimal? DosisMaxima { get; private set; }

    public string? UnidadDosis { get; private set; }

    /// <summary>Días desde la última aplicación hasta la recolección; null si no procede.</summary>
    public int? PlazoSeguridadDias { get; private set; }

    /// <summary>Número máximo de aplicaciones por campaña.</summary>
    public int? Aplicaciones { get; private set; }

    public string? Observaciones { get; private set; }

    /// <summary>Clave del uso para comparar cargas del registro.</summary>
    internal string Clave => $"{Cultivo.ToUpperInvariant()}|{Plaga.ToUpperInvariant()}";

    internal string Resumen => $"dosis {DosisMinima?.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) ?? "—"}–{DosisMaxima?.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) ?? "—"} {UnidadDosis}, plazo {PlazoSeguridadDias?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "NP"} d, {Aplicaciones?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "—"} aplic.";
}

/// <summary>Datos de un producto del registro, tal como vienen en cada carga (o se dan de alta a mano).</summary>
public sealed record DatosProductoFito(string? NumeroRegistro, string? Nombre, string? Titular, EstadoFitosanitario Estado, DateOnly? FechaCaducidad,
    DateOnly? FechaLimiteVenta, DateOnly? FechaLimiteUso, IReadOnlyList<(string? Nombre, string? Riqueza)>? MateriasActivas,
    IReadOnlyList<(string? Cultivo, string? Plaga, decimal? DosisMinima, decimal? DosisMaxima, string? UnidadDosis, int? PlazoSeguridadDias, int? Aplicaciones, string? Observaciones)>? Usos,
    DateOnly? FechaCancelacion = null, string? Formulado = null);

/// <summary>
/// Producto del Registro Oficial de Productos Fitosanitarios (MAPA): número de registro, nombre, titular, situación y
/// fechas, materias activas y usos autorizados. Se actualiza con cada carga del registro; cada cambio que importa
/// (situación, fechas, materias activas, usos retirados o modificados) queda anotado en un <see cref="CambioFitosanitario"/>.
/// Opcionalmente se enlaza con el artículo del almacén con que se compra y se aplica.
/// </summary>
public sealed class ProductoFitosanitario : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudNumero = 30;
    public const int LongitudTexto = 200;

    private readonly List<MateriaActivaFito> _materiasActivas = [];
    private readonly List<UsoFito> _usos = [];

    private ProductoFitosanitario(Guid id)
        : base(id, Guid.Empty)
    {
        NumeroRegistro = null!;
        Nombre = null!;
    }

    private ProductoFitosanitario(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
        NumeroRegistro = null!;
        Nombre = null!;
    }

    public string NumeroRegistro { get; private set; }

    public string Nombre { get; private set; }

    public string? Titular { get; private set; }

    public EstadoFitosanitario Estado { get; private set; }

    /// <summary>Fin de la autorización.</summary>
    public DateOnly? FechaCaducidad { get; private set; }

    /// <summary>Fecha de cancelación de la autorización.</summary>
    public DateOnly? FechaCancelacion { get; private set; }

    /// <summary>Formulado (p. ej. «AZUFRE 80% [WG] P/P»).</summary>
    public string? Formulado { get; private set; }

    /// <summary>Tras cancelarse o caducar: hasta cuándo se puede vender.</summary>
    public DateOnly? FechaLimiteVenta { get; private set; }

    /// <summary>Tras cancelarse o caducar: hasta cuándo se puede aplicar lo que quede.</summary>
    public DateOnly? FechaLimiteUso { get; private set; }

    /// <summary>Artículo del almacén (el envase que se compra y se aplica).</summary>
    public Guid? ProductoId { get; private set; }

    public DateTimeOffset ActualizadoEn { get; private set; }

    public IReadOnlyList<MateriaActivaFito> MateriasActivas => _materiasActivas;

    public IReadOnlyList<UsoFito> Usos => _usos;

    public static string Normalizar(string? numero) => (numero ?? string.Empty).Trim().ToUpperInvariant();

    public static Resultado<ProductoFitosanitario> Crear(Guid empresaId, DatosProductoFito datos, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var numero = Normalizar(datos.NumeroRegistro);
        if (numero.Length is 0 or > LongitudNumero)
        {
            return Resultado.Fallo<ProductoFitosanitario>(Error.Validacion("fitosanitario.numero", $"Indica el número de registro (hasta {LongitudNumero} caracteres)."));
        }

        var p = new ProductoFitosanitario(Guid.NewGuid(), empresaId) { NumeroRegistro = numero };
        var r = p.Aplicar(datos, reloj, out _);
        return r.EsFallo ? Resultado.Fallo<ProductoFitosanitario>(r.Error) : Resultado.Ok(p);
    }

    /// <summary>Aplica una carga del registro: devuelve los cambios respecto a lo que había (vacío si es igual).</summary>
    public Resultado Aplicar(DatosProductoFito datos, IReloj reloj, out IReadOnlyList<(TipoCambioFito Tipo, string Detalle)> cambios)
    {
        ArgumentNullException.ThrowIfNull(datos);
        ArgumentNullException.ThrowIfNull(reloj);
        cambios = [];
        if (string.IsNullOrWhiteSpace(datos.Nombre))
        {
            return Resultado.Fallo(Error.Validacion("fitosanitario.nombre", "Indica el nombre comercial del producto."));
        }

        if (!Enum.IsDefined(datos.Estado))
        {
            return Resultado.Fallo(Error.Validacion("fitosanitario.estado", "Situación no válida: Autorizado, Suspendido, Caducado o Cancelado."));
        }

        static string? T(string? t, int max = LongitudTexto) => string.IsNullOrWhiteSpace(t) ? null : t.Trim()[..Math.Min(t.Trim().Length, max)];
        var materias = new List<MateriaActivaFito>();
        foreach (var (n, r) in datos.MateriasActivas ?? [])
        {
            if (T(n) is { } nombre && !materias.Any(m => string.Equals(m.Nombre, nombre, StringComparison.OrdinalIgnoreCase)))
            {
                materias.Add(new MateriaActivaFito(nombre, T(r, 60)));
            }
        }

        var usos = new List<UsoFito>();
        foreach (var u in datos.Usos ?? [])
        {
            if (T(u.Cultivo) is not { } cultivo || T(u.Plaga) is not { } plaga)
            {
                return Resultado.Fallo(Error.Validacion("fitosanitario.uso", "Cada uso necesita el cultivo y la plaga."));
            }

            if (u.DosisMinima is < 0m || u.DosisMaxima is < 0m || (u.DosisMinima is { } mn && u.DosisMaxima is { } mx && mn > mx)
                || u.PlazoSeguridadDias is < 0 or > 365 || u.Aplicaciones is < 0)
            {
                return Resultado.Fallo(Error.Validacion("fitosanitario.uso", $"Uso {cultivo} / {plaga}: dosis, plazo de seguridad (0 a 365 días) o aplicaciones no válidos."));
            }

            var nuevo = new UsoFito(cultivo, plaga, u.DosisMinima, u.DosisMaxima, T(u.UnidadDosis, 20), u.PlazoSeguridadDias, u.Aplicaciones, T(u.Observaciones, 500));
            if (usos.All(x => x.Clave != nuevo.Clave))
            {
                usos.Add(nuevo);
            }
        }

        var lista = new List<(TipoCambioFito, string)>();
        var primera = ActualizadoEn == default;
        if (!primera)
        {
            if (Estado != datos.Estado)
            {
                lista.Add((datos.Estado == EstadoFitosanitario.Autorizado ? TipoCambioFito.Autorizado : TipoCambioFito.Retirado,
                    $"Situación: {Estado} → {datos.Estado}."));
            }

            if (FechaCaducidad != datos.FechaCaducidad || FechaLimiteVenta != datos.FechaLimiteVenta || FechaLimiteUso != datos.FechaLimiteUso
                || FechaCancelacion != datos.FechaCancelacion)
            {
                lista.Add((TipoCambioFito.Fechas, $"Caducidad {F(datos.FechaCaducidad)}, cancelación {F(datos.FechaCancelacion)}, límite de venta {F(datos.FechaLimiteVenta)}, límite de uso {F(datos.FechaLimiteUso)} (antes {F(FechaCaducidad)}, {F(FechaCancelacion)}, {F(FechaLimiteVenta)}, {F(FechaLimiteUso)})."));
            }

            foreach (var m in _materiasActivas.Where(m => materias.All(x => !string.Equals(x.Nombre, m.Nombre, StringComparison.OrdinalIgnoreCase))))
            {
                lista.Add((TipoCambioFito.MateriaActiva, $"Ya no lleva la materia activa {m.Nombre}."));
            }

            foreach (var m in materias.Where(m => _materiasActivas.All(x => !string.Equals(x.Nombre, m.Nombre, StringComparison.OrdinalIgnoreCase))))
            {
                lista.Add((TipoCambioFito.MateriaActiva, $"Nueva materia activa {m.Nombre}."));
            }

            foreach (var u in _usos)
            {
                var ahora = usos.FirstOrDefault(x => x.Clave == u.Clave);
                if (ahora is null)
                {
                    lista.Add((TipoCambioFito.UsoRetirado, $"Uso retirado: {u.Cultivo} / {u.Plaga}."));
                }
                else if (ahora.Resumen != u.Resumen)
                {
                    lista.Add((TipoCambioFito.UsoModificado, $"Uso {u.Cultivo} / {u.Plaga}: {u.Resumen} → {ahora.Resumen}"));
                }
            }

            foreach (var u in usos.Where(u => _usos.All(x => x.Clave != u.Clave)))
            {
                lista.Add((TipoCambioFito.UsoNuevo, $"Nuevo uso: {u.Cultivo} / {u.Plaga} ({u.Resumen})"));
            }

            if (!string.Equals(Nombre, datos.Nombre.Trim(), StringComparison.Ordinal) || !string.Equals(Titular, T(datos.Titular), StringComparison.Ordinal))
            {
                lista.Add((TipoCambioFito.Datos, $"Nombre o titular: {Nombre} ({Titular}) → {datos.Nombre.Trim()} ({T(datos.Titular)})."));
            }
        }

        Nombre = T(datos.Nombre)!;
        Titular = T(datos.Titular);
        Estado = datos.Estado;
        FechaCaducidad = datos.FechaCaducidad;
        FechaLimiteVenta = datos.FechaLimiteVenta;
        FechaLimiteUso = datos.FechaLimiteUso;
        FechaCancelacion = datos.FechaCancelacion;
        Formulado = T(datos.Formulado);
        if (primera || lista.Any(c => c.Item1 is TipoCambioFito.MateriaActiva))
        {
            _materiasActivas.Clear();
            _materiasActivas.AddRange(materias);
        }

        if (primera || lista.Any(c => c.Item1 is TipoCambioFito.UsoNuevo or TipoCambioFito.UsoRetirado or TipoCambioFito.UsoModificado))
        {
            _usos.Clear();
            _usos.AddRange(usos);
        }

        ActualizadoEn = reloj.AhoraUtc;
        cambios = lista;
        return Resultado.Ok();
    }

    public void EnlazarArticulo(Guid? productoId) => ProductoId = productoId;

    /// <summary>Si se puede aplicar ese día: autorizado y sin caducar, o dentro del plazo de uso tras la retirada.</summary>
    public bool AplicableEl(DateOnly fecha) =>
        (Estado == EstadoFitosanitario.Autorizado && (FechaCaducidad is null || fecha <= FechaCaducidad) && (FechaCancelacion is null || fecha <= FechaCancelacion))
        || (FechaLimiteUso is { } uso && fecha <= uso && Estado != EstadoFitosanitario.Suspendido);

    private static string F(DateOnly? d) => d?.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture) ?? "—";
}

public enum TipoCambioFito
{
    Alta,
    Autorizado,
    Retirado,
    Fechas,
    MateriaActiva,
    UsoNuevo,
    UsoRetirado,
    UsoModificado,
    Datos,
}

/// <summary>Cambio en el registro detectado en una carga: es la base de los avisos a quien tenga el producto o lo use.</summary>
public sealed class CambioFitosanitario : RaizAgregadoEmpresa<Guid>
{
    private CambioFitosanitario(Guid id)
        : base(id, Guid.Empty)
    {
        NumeroRegistro = null!;
        Producto = null!;
        Detalle = null!;
    }

    private CambioFitosanitario(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
        NumeroRegistro = null!;
        Producto = null!;
        Detalle = null!;
    }

    public Guid FitosanitarioId { get; private set; }

    public string NumeroRegistro { get; private set; }

    public string Producto { get; private set; }

    public TipoCambioFito Tipo { get; private set; }

    public string Detalle { get; private set; }

    public DateTimeOffset DetectadoEn { get; private set; }

    /// <summary>Si alguien lo ha revisado (el aviso deja de salir como pendiente).</summary>
    public bool Revisado { get; private set; }

    public static CambioFitosanitario Nuevo(ProductoFitosanitario p, TipoCambioFito tipo, string detalle, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(p);
        ArgumentNullException.ThrowIfNull(reloj);
        return new CambioFitosanitario(Guid.NewGuid(), p.EmpresaId)
        {
            FitosanitarioId = p.Id, NumeroRegistro = p.NumeroRegistro, Producto = p.Nombre, Tipo = tipo,
            Detalle = detalle[..Math.Min(detalle.Length, 1000)], DetectadoEn = reloj.AhoraUtc,
        };
    }

    public void MarcarRevisado() => Revisado = true;

    /// <summary>Si el cambio puede impedir aplicar el producto o su uso (merece un aviso a quien lo tenga o lo use).</summary>
    public bool Restrictivo => Tipo is TipoCambioFito.Retirado or TipoCambioFito.Fechas or TipoCambioFito.MateriaActiva or TipoCambioFito.UsoRetirado or TipoCambioFito.UsoModificado;
}
