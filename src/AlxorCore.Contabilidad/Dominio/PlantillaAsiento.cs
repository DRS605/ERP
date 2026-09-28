using System.Globalization;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Contabilidad.Dominio;

/// <summary>Papel de un apunte en el asiento de un documento (el <c>OrigenApunte</c> de Hispatec).</summary>
public enum PapelApunte
{
    /// <summary>Cliente o proveedor (430/400, o su subcuenta).</summary>
    Tercero = 1,

    /// <summary>Ingreso (7xx) en ventas o gasto (6xx) en compras.</summary>
    Resultado = 2,

    IvaRepercutido = 3,
    IvaSoportado = 4,

    /// <summary>Retención que nos practican en una venta (473).</summary>
    RetencionVenta = 5,

    /// <summary>Retención que practicamos en una compra (4751).</summary>
    RetencionCompra = 6,

    /// <summary>Banco o caja de un cobro o pago.</summary>
    Tesoreria = 7,
}

/// <summary>Cuenta y concepto de un papel en la plantilla (null: lo de siempre).</summary>
public sealed record LineaPlantillaAsiento(PapelApunte Papel, string? Cuenta = null, string? Concepto = null);

/// <summary>
/// Plantilla de asiento por datos, como las de Hispatec: para un sentido (venta, compra, cobro, pago) y, si se quiere,
/// un origen concreto (Factura, Gasto, Movimiento, Anticipo…), el <b>concepto</b> del asiento con variables, el
/// <b>diario</b> y, por papel, la <b>cuenta</b> y el concepto del apunte. Lo que no fija la plantilla sale como siempre.
/// Variables: <c>{Referencia}</c>, <c>{Tercero}</c>, <c>{Fecha}</c>, <c>{Total}</c>, <c>{Origen}</c>.
/// </summary>
public sealed class PlantillaAsiento : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudConcepto = 200;
    public const int LongitudOrigen = 40;
    public const int LongitudCuenta = 12;

    public static readonly IReadOnlyList<string> Variables = ["{Referencia}", "{Tercero}", "{Fecha}", "{Total}", "{Origen}"];

    private PlantillaAsiento(Guid id)
        : base(id, Guid.Empty)
    {
        Nombre = null!;
    }

    private PlantillaAsiento(Guid id, Guid empresaId, SentidoContable sentido, string? origenTipo)
        : base(id, empresaId)
    {
        Sentido = sentido;
        OrigenTipo = origenTipo;
        Nombre = string.Empty;
    }

    public SentidoContable Sentido { get; private set; }

    /// <summary>Origen del documento al que se limita (null: todos los de su sentido).</summary>
    public string? OrigenTipo { get; private set; }

    public string Nombre { get; private set; }

    /// <summary>Concepto del asiento con variables (null: «{Referencia} · {Tercero}»).</summary>
    public string? Concepto { get; private set; }

    /// <summary>Diario del asiento (null: el de su origen).</summary>
    public string? Diario { get; private set; }

    public IReadOnlyList<LineaPlantillaAsiento> Lineas { get; private set; } = [];

    public bool Activa { get; private set; }

    public static Resultado<PlantillaAsiento> Crear(Guid empresaId, SentidoContable sentido, string? origenTipo, string? nombre, string? concepto, string? diario,
        IReadOnlyList<LineaPlantillaAsiento>? lineas, bool activa = true)
    {
        if (!Enum.IsDefined(sentido))
        {
            return Resultado.Fallo<PlantillaAsiento>(Error.Validacion("plantilla.sentido", "Sentido no válido: Venta, Compra, Cobro o Pago."));
        }

        var origen = string.IsNullOrWhiteSpace(origenTipo) ? null : origenTipo.Trim();
        if (origen is { Length: > LongitudOrigen })
        {
            return Resultado.Fallo<PlantillaAsiento>(Error.Validacion("plantilla.origen", "El origen es demasiado largo."));
        }

        var p = new PlantillaAsiento(Guid.NewGuid(), empresaId, sentido, origen);
        var r = p.Actualizar(nombre, concepto, diario, lineas, activa);
        return r.EsFallo ? Resultado.Fallo<PlantillaAsiento>(r.Error) : Resultado.Ok(p);
    }

    public Resultado Actualizar(string? nombre, string? concepto, string? diario, IReadOnlyList<LineaPlantillaAsiento>? lineas, bool activa)
    {
        var texto = string.IsNullOrWhiteSpace(concepto) ? null : concepto.Trim();
        if (texto is { Length: > LongitudConcepto })
        {
            return Resultado.Fallo(Error.Validacion("plantilla.concepto", $"El concepto no puede pasar de {LongitudConcepto} caracteres."));
        }

        var limpias = new List<LineaPlantillaAsiento>();
        foreach (var l in lineas ?? [])
        {
            if (!Enum.IsDefined(l.Papel))
            {
                return Resultado.Fallo(Error.Validacion("plantilla.papel", "Papel de apunte no válido."));
            }

            if (!Admite(Sentido, l.Papel))
            {
                return Resultado.Fallo(Error.Validacion("plantilla.papel", $"El papel {l.Papel} no aparece en los asientos de {Sentido.ToString().ToLowerInvariant()}."));
            }

            if (limpias.Any(x => x.Papel == l.Papel))
            {
                return Resultado.Fallo(Error.Validacion("plantilla.papel_repetido", $"El papel {l.Papel} está repetido."));
            }

            var cuenta = string.IsNullOrWhiteSpace(l.Cuenta) ? null : l.Cuenta.Trim();
            if (cuenta is not null && (cuenta.Length > LongitudCuenta || !cuenta.All(char.IsDigit)))
            {
                return Resultado.Fallo(Error.Validacion("plantilla.cuenta", $"La cuenta «{cuenta}» no es válida (solo dígitos, hasta {LongitudCuenta})."));
            }

            var c = string.IsNullOrWhiteSpace(l.Concepto) ? null : l.Concepto.Trim();
            if (c is { Length: > LongitudConcepto })
            {
                return Resultado.Fallo(Error.Validacion("plantilla.concepto", "El concepto de un apunte es demasiado largo."));
            }

            if (cuenta is not null || c is not null)
            {
                limpias.Add(new LineaPlantillaAsiento(l.Papel, cuenta, c));
            }
        }

        Nombre = string.IsNullOrWhiteSpace(nombre) ? $"{Sentido}{(OrigenTipo is null ? "" : " · " + OrigenTipo)}" : nombre.Trim()[..Math.Min(nombre.Trim().Length, 100)];
        Concepto = texto;
        Diario = string.IsNullOrWhiteSpace(diario) ? null : diario.Trim().ToUpperInvariant();
        Lineas = limpias;
        Activa = activa;
        return Resultado.Ok();
    }

    /// <summary>Papeles que tiene el asiento de cada sentido.</summary>
    public static bool Admite(SentidoContable sentido, PapelApunte papel) => sentido switch
    {
        SentidoContable.Venta => papel is PapelApunte.Tercero or PapelApunte.Resultado or PapelApunte.IvaRepercutido or PapelApunte.RetencionVenta,
        SentidoContable.Compra => papel is PapelApunte.Tercero or PapelApunte.Resultado or PapelApunte.IvaSoportado or PapelApunte.IvaRepercutido or PapelApunte.RetencionCompra,
        _ => papel is PapelApunte.Tercero or PapelApunte.Tesoreria,
    };

    public string? CuentaDe(PapelApunte papel) => Lineas.FirstOrDefault(l => l.Papel == papel)?.Cuenta;

    public string? ConceptoDe(PapelApunte papel) => Lineas.FirstOrDefault(l => l.Papel == papel)?.Concepto;

    /// <summary>Sustituye las variables del texto con los datos del documento.</summary>
    public static string Rellenar(string plantilla, string referencia, string tercero, DateOnly fecha, decimal total, string origen) =>
        plantilla.Replace("{Referencia}", referencia, StringComparison.OrdinalIgnoreCase)
            .Replace("{Tercero}", tercero, StringComparison.OrdinalIgnoreCase)
            .Replace("{Fecha}", fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase)
            .Replace("{Total}", total.ToString("#,##0.00", CultureInfo.InvariantCulture).Replace(",", "#", StringComparison.Ordinal)
                .Replace(".", ",", StringComparison.Ordinal).Replace("#", ".", StringComparison.Ordinal), StringComparison.OrdinalIgnoreCase)
            .Replace("{Origen}", origen, StringComparison.OrdinalIgnoreCase)
            .Trim();
}
