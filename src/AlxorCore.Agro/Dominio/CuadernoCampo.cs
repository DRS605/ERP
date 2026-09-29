using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Agro.Dominio;

/// <summary>Tipo de labor del cuaderno de campo.</summary>
public enum TipoLabor
{
    Fitosanitario,

    /// <summary>Abonado: fertilizante con sus unidades fertilizantes (N, P₂O₅, K₂O en kg/ha).</summary>
    Abonado,

    /// <summary>Riego: volumen de agua aplicado.</summary>
    Riego,

    /// <summary>Otra labor (poda, laboreo, siembra…).</summary>
    Otra,
}

/// <summary>
/// Labor del cuaderno de campo de una parcela; la principal, el tratamiento fitosanitario: el registro del cuaderno de campo (RD 1311/2012) que piden GlobalG.A.P. y
/// los clientes. El plazo de seguridad son los días que deben pasar desde el tratamiento hasta la recolección: una
/// entrega recolectada antes no se confirma. No se borra: se anula con el motivo.
/// </summary>
public sealed class TratamientoParcela : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudTexto = 150;

    private TratamientoParcela(Guid id)
        : base(id, Guid.Empty)
    {
        Producto = null!;
    }

    private TratamientoParcela(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
        Producto = null!;
    }

    public Guid ParcelaId { get; private set; }

    public TipoLabor Tipo { get; private set; }

    /// <summary>Abonado: unidades fertilizantes de nitrógeno (kg/ha).</summary>
    public decimal? NitrogenoKgHa { get; private set; }

    /// <summary>Abonado: fósforo (P₂O₅, kg/ha).</summary>
    public decimal? FosforoKgHa { get; private set; }

    /// <summary>Abonado: potasio (K₂O, kg/ha).</summary>
    public decimal? PotasioKgHa { get; private set; }

    /// <summary>Riego: metros cúbicos aplicados.</summary>
    public decimal? VolumenM3 { get; private set; }

    public DateOnly Fecha { get; private set; }

    /// <summary>Producto fitosanitario (nombre comercial).</summary>
    public string Producto { get; private set; }

    /// <summary>Número del Registro Oficial de Productos Fitosanitarios.</summary>
    public string? NumeroRegistro { get; private set; }

    public string? MateriaActiva { get; private set; }

    /// <summary>Plaga, enfermedad o motivo del tratamiento.</summary>
    public string? Motivo { get; private set; }

    public decimal? Dosis { get; private set; }

    /// <summary>Unidad de la dosis (l/ha, kg/ha, cc/hl…).</summary>
    public string? UnidadDosis { get; private set; }

    public decimal? SuperficieTratadaHa { get; private set; }

    /// <summary>Días desde el tratamiento hasta que se puede recolectar.</summary>
    public int PlazoSeguridadDias { get; private set; }

    /// <summary>Aplicador (con su carné de aplicador) o empresa de servicios.</summary>
    public string? Aplicador { get; private set; }

    public string? Observaciones { get; private set; }

    public bool Anulado { get; private set; }

    public string? MotivoAnulacion { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    /// <summary>Primer día en que se puede recolectar.</summary>
    public DateOnly RecolectableDesde => Fecha.AddDays(PlazoSeguridadDias);

    /// <summary>Si una recolección de ese día queda dentro del plazo de seguridad (no se puede recolectar aún).</summary>
    public bool DentroDePlazo(DateOnly recoleccion) => !Anulado && recoleccion >= Fecha && recoleccion < RecolectableDesde;

    public static Resultado<TratamientoParcela> Crear(Guid empresaId, Guid parcelaId, DateOnly fecha, string? producto, int plazoSeguridadDias, IReloj reloj,
        string? numeroRegistro = null, string? materiaActiva = null, string? motivo = null, decimal? dosis = null, string? unidadDosis = null,
        decimal? superficieTratadaHa = null, string? aplicador = null, string? observaciones = null, TipoLabor tipo = TipoLabor.Fitosanitario,
        decimal? nitrogenoKgHa = null, decimal? fosforoKgHa = null, decimal? potasioKgHa = null, decimal? volumenM3 = null)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (!Enum.IsDefined(tipo))
        {
            return Resultado.Fallo<TratamientoParcela>(Error.Validacion("tratamiento.tipo", "Tipo de labor no válido: Fitosanitario, Abonado, Riego u Otra."));
        }

        if (string.IsNullOrWhiteSpace(producto))
        {
            return Resultado.Fallo<TratamientoParcela>(Error.Validacion("tratamiento.producto",
                tipo == TipoLabor.Riego ? "Indica el origen del agua o el sistema de riego." : "Indica el producto o la labor."));
        }

        if (nitrogenoKgHa is < 0m || fosforoKgHa is < 0m || potasioKgHa is < 0m || volumenM3 is < 0m)
        {
            return Resultado.Fallo<TratamientoParcela>(Error.Validacion("tratamiento.valores", "Las unidades fertilizantes y el volumen no pueden ser negativos."));
        }

        // Solo un fitosanitario tiene plazo de seguridad; el abonado lleva sus unidades y el riego su volumen.
        if (tipo != TipoLabor.Fitosanitario)
        {
            plazoSeguridadDias = 0;
        }

        if (tipo != TipoLabor.Abonado)
        {
            nitrogenoKgHa = fosforoKgHa = potasioKgHa = null;
        }

        if (tipo != TipoLabor.Riego)
        {
            volumenM3 = null;
        }

        if (plazoSeguridadDias is < 0 or > 365 || dosis is < 0m || superficieTratadaHa is < 0m)
        {
            return Resultado.Fallo<TratamientoParcela>(Error.Validacion("tratamiento.valores", "El plazo de seguridad (0 a 365 días), la dosis y la superficie no pueden ser negativos."));
        }

        static string? T(string? t, int max = LongitudTexto) => string.IsNullOrWhiteSpace(t) ? null : t.Trim()[..Math.Min(t.Trim().Length, max)];
        return Resultado.Ok(new TratamientoParcela(Guid.NewGuid(), empresaId)
        {
            ParcelaId = parcelaId, Tipo = tipo, NitrogenoKgHa = nitrogenoKgHa, FosforoKgHa = fosforoKgHa, PotasioKgHa = potasioKgHa, VolumenM3 = volumenM3, Fecha = fecha, Producto = T(producto)!, NumeroRegistro = T(numeroRegistro, 30), MateriaActiva = T(materiaActiva), Motivo = T(motivo),
            Dosis = dosis, UnidadDosis = T(unidadDosis, 20), SuperficieTratadaHa = superficieTratadaHa, PlazoSeguridadDias = plazoSeguridadDias, Aplicador = T(aplicador),
            Observaciones = T(observaciones, 300), CreadoEn = reloj.AhoraUtc,
        });
    }

    /// <summary>Producto del Registro Oficial aplicado (null si se escribió a mano).</summary>
    public Guid? FitosanitarioId { get; private set; }

    /// <summary>Cultivo del uso autorizado con que se aplicó (la plaga va en <see cref="Motivo"/>).</summary>
    public string? Cultivo { get; private set; }

    /// <summary>Artículo del almacén consumido (el envase del producto).</summary>
    public Guid? ArticuloId { get; private set; }

    public Guid? AlmacenId { get; private set; }

    public string? Lote { get; private set; }

    /// <summary>Cantidad del artículo que salió del almacén.</summary>
    public decimal? CantidadConsumida { get; private set; }

    /// <summary>Enlaza el tratamiento con el producto del registro y el uso autorizado.</summary>
    public void EnlazarRegistro(ProductoFitosanitario producto, string? cultivo)
    {
        ArgumentNullException.ThrowIfNull(producto);
        FitosanitarioId = producto.Id;
        NumeroRegistro = producto.NumeroRegistro;
        MateriaActiva ??= string.Join(" + ", producto.MateriasActivas.Select(m => m.Nombre)) is { Length: > 0 } ma ? ma[..Math.Min(ma.Length, LongitudTexto)] : null;
        Cultivo = string.IsNullOrWhiteSpace(cultivo) ? null : cultivo.Trim()[..Math.Min(cultivo.Trim().Length, LongitudTexto)];
    }

    /// <summary>Anota lo que salió del almacén con el tratamiento.</summary>
    public void AnotarConsumo(Guid articuloId, Guid almacenId, string? lote, decimal cantidad)
    {
        ArticuloId = articuloId;
        AlmacenId = almacenId;
        Lote = string.IsNullOrWhiteSpace(lote) ? null : lote.Trim()[..Math.Min(lote.Trim().Length, 60)];
        CantidadConsumida = cantidad;
    }

    public Resultado Anular(string? motivo)
    {
        if (Anulado)
        {
            return Resultado.Fallo(Error.Conflicto("tratamiento.anulado", "El tratamiento ya está anulado."));
        }

        Anulado = true;
        MotivoAnulacion = string.IsNullOrWhiteSpace(motivo) ? "Anulado" : motivo.Trim()[..Math.Min(motivo.Trim().Length, 200)];
        return Resultado.Ok();
    }
}
