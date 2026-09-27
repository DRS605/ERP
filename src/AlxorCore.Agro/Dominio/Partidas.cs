using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Agro.Dominio;

/// <summary>De dónde sale una partida.</summary>
public enum OrigenPartida
{
    /// <summary>Fruta recibida de un agricultor (una línea de recepción).</summary>
    Recepcion = 1,

    /// <summary>Producto confeccionado en un parte de confección, a partir de otras partidas.</summary>
    Confeccion = 2,
}

/// <summary>
/// Partida (lote) de fruta o de producto confeccionado. Es la unidad de trazabilidad: sabe de qué
/// agricultor, parcela y campaña viene, o de qué partidas se confeccionó. Sus kilos disponibles son la
/// suma de sus <see cref="MovimientoPartida"/>, que la base de datos impide dejar en negativo.
/// </summary>
public sealed class Partida : RaizAgregadoEmpresa<Guid>
{
    private Partida(Guid id)
        : base(id, Guid.Empty)
    {
        Codigo = null!;
    }

    private Partida(Guid id, Guid empresaId, string codigo, Guid productoId, OrigenPartida origen, DateOnly fecha, decimal kilos, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Codigo = codigo;
        ProductoId = productoId;
        Origen = origen;
        Fecha = fecha;
        KilosIniciales = kilos;
        CreadoEn = ahora;
    }

    public string Codigo { get; private set; }

    public Guid ProductoId { get; private set; }

    public OrigenPartida Origen { get; private set; }

    public DateOnly Fecha { get; private set; }

    public decimal KilosIniciales { get; private set; }

    public Guid? RecepcionId { get; private set; }

    public Guid? LineaRecepcionId { get; private set; }

    public Guid? AgricultorId { get; private set; }

    public Guid? ParcelaId { get; private set; }

    public Guid? CampanaId { get; private set; }

    public string? Calibre { get; private set; }

    /// <summary>Parte de confección que la produjo.</summary>
    public Guid? ParteConfeccionId { get; private set; }

    /// <summary>Coste por kilo (el de la confección para las confeccionadas; el precio liquidado para las recibidas, cuando se liquida).</summary>
    public decimal? CosteKg { get; private set; }

    /// <summary>Anulada con su recepción: ya no se puede usar.</summary>
    public bool Anulada { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public static Partida DeRecepcion(Guid empresaId, Recepcion recepcion, LineaRecepcion linea, decimal kilos, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(recepcion);
        ArgumentNullException.ThrowIfNull(linea);
        ArgumentNullException.ThrowIfNull(reloj);
        return new Partida(Guid.NewGuid(), empresaId, string.Empty, linea.ProductoId, OrigenPartida.Recepcion, recepcion.Fecha, kilos, reloj.AhoraUtc)
        {
            RecepcionId = recepcion.Id,
            LineaRecepcionId = linea.Id,
            AgricultorId = recepcion.AgricultorId,
            ParcelaId = linea.ParcelaId,
            CampanaId = recepcion.CampanaId,
            Calibre = linea.Calibre,
        };
    }

    public static Partida DeConfeccion(Guid empresaId, string codigo, Guid productoId, DateOnly fecha, decimal kilos, Guid parteId, Guid? campanaId, string? calibre, decimal costeKg, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        return new Partida(Guid.NewGuid(), empresaId, codigo, productoId, OrigenPartida.Confeccion, fecha, kilos, reloj.AhoraUtc)
        {
            ParteConfeccionId = parteId,
            CampanaId = campanaId,
            Calibre = calibre,
            CosteKg = costeKg,
        };
    }

    /// <summary>Código de la partida de una recepción: número de la recepción y línea (REC-2026-000012/2).</summary>
    public void AsignarCodigo(string codigo) => Codigo = codigo;

    public void Anular() => Anulada = true;

    public void FijarCoste(decimal? costeKg) => CosteKg = costeKg;
}

/// <summary>Tipo de movimiento de kilos de una partida.</summary>
public enum TipoMovimientoPartida
{
    /// <summary>Entrada de la recepción o de la confección.</summary>
    Entrada = 1,

    /// <summary>Consumo en un parte de confección.</summary>
    Consumo = 2,

    /// <summary>Paso de kilos sueltos a un palé (o de un palé a otro).</summary>
    Paletizado = 3,

    /// <summary>Salida en una expedición a un cliente.</summary>
    Expedicion = 4,

    /// <summary>Merma, destrío retirado o regularización de inventario.</summary>
    Ajuste = 5,

    /// <summary>Inversión de otro movimiento al anular la recepción o el parte de confección que lo creó.</summary>
    Anulacion = 6,
}

/// <summary>
/// Movimiento del libro de partidas: kilos con signo, opcionalmente en un palé. Solo se insertan, nunca
/// se modifican ni se borran (la base de datos lo impide); un error se corrige con otro movimiento.
/// </summary>
public sealed class MovimientoPartida : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudConcepto = 200;

    private MovimientoPartida(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private MovimientoPartida(Guid id, Guid empresaId, Guid partidaId, DateOnly fecha, TipoMovimientoPartida tipo, decimal kilos, Guid? paleId,
        string? documentoTipo, Guid? documentoId, string? concepto, int cajas, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Cajas = cajas;
        PartidaId = partidaId;
        Fecha = fecha;
        Tipo = tipo;
        Kilos = kilos;
        PaleId = paleId;
        DocumentoTipo = documentoTipo;
        DocumentoId = documentoId;
        Concepto = concepto;
        CreadoEn = ahora;
    }

    public Guid PartidaId { get; private set; }

    public DateOnly Fecha { get; private set; }

    public TipoMovimientoPartida Tipo { get; private set; }

    /// <summary>Kilos con signo: positivos entran, negativos salen.</summary>
    public decimal Kilos { get; private set; }

    /// <summary>Cajas (bultos) que representan esos kilos, con el mismo signo; 0 si se movieron a granel.</summary>
    public int Cajas { get; private set; }

    /// <summary>Palé en el que están (o del que salen) los kilos; null si están sueltos.</summary>
    public Guid? PaleId { get; private set; }

    public string? DocumentoTipo { get; private set; }

    public Guid? DocumentoId { get; private set; }

    public string? Concepto { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public static Resultado<MovimientoPartida> Crear(Guid empresaId, Guid partidaId, DateOnly fecha, TipoMovimientoPartida tipo, decimal kilos, Guid? paleId,
        string? documentoTipo, Guid? documentoId, string? concepto, IReloj reloj, int cajas = 0)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (cajas != 0 && Math.Sign(cajas) != Math.Sign(kilos))
        {
            return Resultado.Fallo<MovimientoPartida>(Error.Validacion("partida.cajas", "Las cajas llevan el mismo signo que los kilos."));
        }

        if (kilos == 0m || decimal.Round(kilos, 3) != kilos)
        {
            return Resultado.Fallo<MovimientoPartida>(Error.Validacion("partida.kilos", "Los kilos no pueden ser cero (hasta 3 decimales)."));
        }

        // Las anulaciones invierten el movimiento que anulan (una entrada o un consumo): llevan cualquier signo.
        if ((tipo == TipoMovimientoPartida.Entrada && kilos < 0m) || (tipo is TipoMovimientoPartida.Consumo or TipoMovimientoPartida.Expedicion && kilos > 0m))
        {
            return Resultado.Fallo<MovimientoPartida>(Error.Validacion("partida.signo", "El signo de los kilos no corresponde al tipo de movimiento."));
        }

        var texto = string.IsNullOrWhiteSpace(concepto) ? null : concepto.Trim();
        if (texto?.Length > LongitudConcepto)
        {
            texto = texto[..LongitudConcepto];
        }

        return Resultado.Ok(new MovimientoPartida(Guid.NewGuid(), empresaId, partidaId, fecha, tipo, kilos, paleId, documentoTipo, documentoId, texto, cajas, reloj.AhoraUtc));
    }
}

/// <summary>Estado de un palé.</summary>
public enum EstadoPale
{
    /// <summary>Se está montando: admite y suelta kilos.</summary>
    Abierto = 1,

    /// <summary>Cerrado y etiquetado: listo para expedir.</summary>
    Cerrado = 2,

    /// <summary>Salió en una expedición.</summary>
    Expedido = 3,
}

/// <summary>
/// Palé identificado con un SSCC (GS1, 18 dígitos): la unidad logística que se etiqueta, se expide y se
/// traza. Su contenido son los kilos de las partidas que se le han paletizado.
/// </summary>
public sealed class Pale : RaizAgregadoEmpresa<Guid>
{
    private Pale(Guid id)
        : base(id, Guid.Empty)
    {
        Sscc = null!;
    }

    private Pale(Guid id, Guid empresaId, string sscc, string? tipo, Guid? plantillaId, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Sscc = sscc;
        Tipo = tipo;
        PlantillaId = plantillaId;
        Estado = EstadoPale.Abierto;
        CreadoEn = ahora;
    }

    public string Sscc { get; private set; }

    /// <summary>Tipo de palé (europeo, americano, medio palé…).</summary>
    public string? Tipo { get; private set; }

    public EstadoPale Estado { get; private set; }

    /// <summary>Plantilla con que se monta (cajas por palé, kilos por caja, mosaico…); null en un palé a granel.</summary>
    public Guid? PlantillaId { get; private set; }

    /// <summary>Carta de porte que se emitió al expedirlo.</summary>
    public Guid? CartaPorteId { get; private set; }

    /// <summary>Albarán de venta (del pedido) con que salió.</summary>
    public Guid? AlbaranId { get; private set; }

    public Guid? ClienteId { get; private set; }

    public DateOnly? FechaExpedicion { get; private set; }

    /// <summary>Referencia del albarán o carta de porte con que salió.</summary>
    public string? ReferenciaExpedicion { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public static Resultado<Pale> Crear(Guid empresaId, string sscc, string? tipo, IReloj reloj, Guid? plantillaId = null)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (sscc?.Length != 18 || !ReglasAgro.Gs1Valido(sscc))
        {
            return Resultado.Fallo<Pale>(Error.Validacion("pale.sscc", "El SSCC debe tener 18 dígitos con el dígito de control GS1 correcto."));
        }

        return Resultado.Ok(new Pale(Guid.NewGuid(), empresaId, sscc, string.IsNullOrWhiteSpace(tipo) ? null : tipo.Trim(), plantillaId, reloj.AhoraUtc));
    }

    public Resultado Cerrar()
    {
        if (Estado != EstadoPale.Abierto)
        {
            return Resultado.Fallo(Error.Conflicto("pale.no_abierto", "El palé no está abierto."));
        }

        Estado = EstadoPale.Cerrado;
        return Resultado.Ok();
    }

    public Resultado Reabrir()
    {
        if (Estado != EstadoPale.Cerrado)
        {
            return Resultado.Fallo(Error.Conflicto("pale.no_cerrado", "Solo se reabre un palé cerrado que no ha salido."));
        }

        Estado = EstadoPale.Abierto;
        return Resultado.Ok();
    }

    public Resultado Expedir(Guid? clienteId, DateOnly fecha, string? referencia)
    {
        if (Estado != EstadoPale.Cerrado)
        {
            return Resultado.Fallo(Error.Conflicto("pale.no_cerrado", "Solo se expide un palé cerrado."));
        }

        Estado = EstadoPale.Expedido;
        ClienteId = clienteId;
        FechaExpedicion = fecha;
        ReferenciaExpedicion = string.IsNullOrWhiteSpace(referencia) ? null : referencia.Trim();
        return Resultado.Ok();
    }

    /// <summary>Deshace la expedición (salió por error o volvió): el palé queda cerrado con su contenido, listo para expedir.</summary>
    public Resultado AnularExpedicion()
    {
        if (Estado != EstadoPale.Expedido)
        {
            return Resultado.Fallo(Error.Conflicto("pale.no_expedido", "El palé no está expedido."));
        }

        Estado = EstadoPale.Cerrado;
        ClienteId = null;
        FechaExpedicion = null;
        ReferenciaExpedicion = null;
        CartaPorteId = null;
        AlbaranId = null;
        return Resultado.Ok();
    }

    /// <summary>Enlaza el albarán de venta emitido con la expedición (solo en un palé expedido).</summary>
    public Resultado AsignarAlbaran(Guid albaranId, string numero)
    {
        if (Estado != EstadoPale.Expedido)
        {
            return Resultado.Fallo(Error.Conflicto("pale.no_expedido", "El palé no está expedido."));
        }

        AlbaranId = albaranId;
        ReferenciaExpedicion ??= numero;
        return Resultado.Ok();
    }

    /// <summary>Enlaza la carta de porte emitida con la expedición (solo en un palé expedido).</summary>
    public Resultado AsignarCartaPorte(Guid cartaPorteId, string numero)
    {
        if (Estado != EstadoPale.Expedido)
        {
            return Resultado.Fallo(Error.Conflicto("pale.no_expedido", "El palé no está expedido."));
        }

        CartaPorteId = cartaPorteId;
        ReferenciaExpedicion ??= numero;
        return Resultado.Ok();
    }
}

/// <summary>
/// Plantilla de palé (la «confección» de Hispatec): qué tipo de palé, qué producto y marca, cuántas cajas lleva y de
/// cuántos kilos, y su mosaico (cajas por capa = filas × columnas). Con ella el palé se monta por cajas y se cierra solo
/// al completarse, y se pueden montar de una vez todos los palés completos que da una partida.
/// </summary>
public sealed class PlantillaPale : RaizAgregadoEmpresa<Guid>
{
    public const int MaximoCajas = 10_000;

    private PlantillaPale(Guid id)
        : base(id, Guid.Empty)
    {
        Codigo = null!;
        Nombre = null!;
    }

    private PlantillaPale(Guid id, Guid empresaId, string codigo, string nombre)
        : base(id, empresaId)
    {
        Codigo = codigo;
        Nombre = nombre;
        Activa = true;
    }

    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    /// <summary>Tipo de palé (europeo, americano, medio palé…).</summary>
    public string? TipoPale { get; private set; }

    /// <summary>Producto que admite; null si admite cualquiera.</summary>
    public Guid? ProductoId { get; private set; }

    public string? Marca { get; private set; }

    public int CajasPorPale { get; private set; }

    public decimal KilosPorCaja { get; private set; }

    /// <summary>Mosaico de cada capa (opcional): filas × columnas cajas.</summary>
    public int? Filas { get; private set; }

    public int? Columnas { get; private set; }

    /// <summary>Cliente para el que se monta habitualmente (opcional; se propone al expedir).</summary>
    public Guid? ClienteId { get; private set; }

    public bool Activa { get; private set; }

    public int? CajasPorCapa => Filas is { } f && Columnas is { } c ? f * c : null;

    public int? Capas => CajasPorCapa is { } porCapa ? CajasPorPale / porCapa : null;

    public decimal KilosPorPale => CajasPorPale * KilosPorCaja;

    public static Resultado<PlantillaPale> Crear(Guid empresaId, string? codigo, string? nombre, DatosPlantillaPale datos)
    {
        var error = ReglasAgro.CodigoNombre(ref codigo, ref nombre, "plantilla");
        if (error is not null)
        {
            return Resultado.Fallo<PlantillaPale>(error);
        }

        var p = new PlantillaPale(Guid.NewGuid(), empresaId, codigo!, nombre!);
        var r = p.Fijar(nombre, datos, true, false);
        return r.EsFallo ? Resultado.Fallo<PlantillaPale>(r.Error) : Resultado.Ok(p);
    }

    /// <summary>
    /// Cambia la plantilla. Si ya hay palés montados con ella, las cajas por palé, los kilos por caja y el producto
    /// no cambian (los palés abiertos se cerrarían con otra medida): se crea otra plantilla.
    /// </summary>
    public Resultado Fijar(string? nombre, DatosPlantillaPale datos, bool activa, bool enUso)
    {
        ArgumentNullException.ThrowIfNull(datos);
        nombre = nombre?.Trim();
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Length > ReglasAgro.LongitudNombre)
        {
            return Resultado.Fallo(Error.Validacion("plantilla.nombre", $"El nombre es obligatorio (máximo {ReglasAgro.LongitudNombre} caracteres)."));
        }

        if (datos.CajasPorPale is < 1 or > MaximoCajas)
        {
            return Resultado.Fallo(Error.Validacion("plantilla.cajas", $"Las cajas por palé van de 1 a {MaximoCajas}."));
        }

        if (datos.KilosPorCaja <= 0m || decimal.Round(datos.KilosPorCaja, 3) != datos.KilosPorCaja || datos.KilosPorCaja * datos.CajasPorPale > 100_000m)
        {
            return Resultado.Fallo(Error.Validacion("plantilla.kilos", "Los kilos por caja son positivos (hasta 3 decimales)."));
        }

        if ((datos.Filas is null) != (datos.Columnas is null) || datos.Filas is < 1 || datos.Columnas is < 1
            || (datos.Filas is { } f && datos.Columnas is { } c && datos.CajasPorPale % (f * c) != 0))
        {
            return Resultado.Fallo(Error.Validacion("plantilla.mosaico",
                "El mosaico lleva filas y columnas (las dos) y las cajas por palé deben ser un número entero de capas (filas × columnas)."));
        }

        if (enUso && (datos.CajasPorPale != CajasPorPale || datos.KilosPorCaja != KilosPorCaja || datos.ProductoId != ProductoId))
        {
            return Resultado.Fallo(Error.Conflicto("plantilla.en_uso",
                "Ya hay palés montados con esta plantilla: no cambian sus cajas, kilos ni producto. Crea otra plantilla (y desactiva esta)."));
        }

        Nombre = nombre;
        TipoPale = Texto(datos.TipoPale, 40);
        ProductoId = datos.ProductoId;
        Marca = Texto(datos.Marca, 60);
        CajasPorPale = datos.CajasPorPale;
        KilosPorCaja = datos.KilosPorCaja;
        Filas = datos.Filas;
        Columnas = datos.Columnas;
        ClienteId = datos.ClienteId;
        Activa = activa;
        return Resultado.Ok();
    }

    private static string? Texto(string? t, int maximo)
    {
        var v = string.IsNullOrWhiteSpace(t) ? null : t.Trim();
        return v?.Length > maximo ? v[..maximo] : v;
    }
}

/// <summary>Datos de una plantilla de palé.</summary>
public sealed record DatosPlantillaPale(int CajasPorPale, decimal KilosPorCaja, string? TipoPale = null, Guid? ProductoId = null, string? Marca = null,
    int? Filas = null, int? Columnas = null, Guid? ClienteId = null);

/// <summary>
/// Movimiento de envases con un agricultor (palots, cajas…): positivos los que se le entregan vacíos,
/// negativos los que devuelve (llenos de fruta en una recepción, o vacíos). El saldo son los envases de
/// la empresa que tiene el agricultor. Solo inserción.
/// </summary>
public sealed class MovimientoEnvase : RaizAgregadoEmpresa<Guid>
{
    private MovimientoEnvase(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private MovimientoEnvase(Guid id, Guid empresaId, Guid agricultorId, Guid envaseProductoId, DateOnly fecha, int cantidad, Guid? recepcionId, string? concepto, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        AgricultorId = agricultorId;
        EnvaseProductoId = envaseProductoId;
        Fecha = fecha;
        Cantidad = cantidad;
        RecepcionId = recepcionId;
        Concepto = concepto;
        CreadoEn = ahora;
    }

    public Guid AgricultorId { get; private set; }

    public Guid EnvaseProductoId { get; private set; }

    public DateOnly Fecha { get; private set; }

    public int Cantidad { get; private set; }

    public Guid? RecepcionId { get; private set; }

    public string? Concepto { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public static Resultado<MovimientoEnvase> Crear(Guid empresaId, Guid agricultorId, Guid envaseProductoId, DateOnly fecha, int cantidad, Guid? recepcionId, string? concepto, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (cantidad == 0)
        {
            return Resultado.Fallo<MovimientoEnvase>(Error.Validacion("envase.cantidad", "La cantidad de envases no puede ser cero."));
        }

        var texto = string.IsNullOrWhiteSpace(concepto) ? null : concepto.Trim();
        return Resultado.Ok(new MovimientoEnvase(Guid.NewGuid(), empresaId, agricultorId, envaseProductoId, fecha, cantidad, recepcionId,
            texto?.Length > MovimientoPartida.LongitudConcepto ? texto[..MovimientoPartida.LongitudConcepto] : texto, reloj.AhoraUtc));
    }
}

/// <summary>Ajustes del módulo agro de la empresa.</summary>
public sealed class ConfiguracionAgro : RaizAgregadoEmpresa<Guid>
{
    private ConfiguracionAgro(Guid id)
        : base(id, Guid.Empty)
    {
        PrefijoGs1 = null!;
    }

    private ConfiguracionAgro(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
        PrefijoGs1 = PrefijoPruebas;
    }

    /// <summary>Prefijo de pruebas (el de España sin empresa): se cambia por el que asigna GS1 a la empresa.</summary>
    public const string PrefijoPruebas = "8400000";

    /// <summary>Prefijo de empresa GS1 (de 7 a 10 dígitos) para los SSCC de los palés.</summary>
    public string PrefijoGs1 { get; private set; }

    /// <summary>Dígito de extensión del SSCC (0-9).</summary>
    public int DigitoExtension { get; private set; }

    public static ConfiguracionAgro Crear(Guid empresaId) => new(Guid.NewGuid(), empresaId);

    public Resultado Actualizar(string? prefijoGs1, int digitoExtension)
    {
        var prefijo = prefijoGs1?.Trim();
        if (prefijo is null || prefijo.Length is < 7 or > 10 || !prefijo.All(char.IsAsciiDigit))
        {
            return Resultado.Fallo(Error.Validacion("agro.prefijo_gs1", "El prefijo de empresa GS1 tiene de 7 a 10 dígitos."));
        }

        if (digitoExtension is < 0 or > 9)
        {
            return Resultado.Fallo(Error.Validacion("agro.extension_sscc", "El dígito de extensión va de 0 a 9."));
        }

        PrefijoGs1 = prefijo;
        DigitoExtension = digitoExtension;
        return Resultado.Ok();
    }

    /// <summary>SSCC del palé con número de serie <paramref name="serie"/>: extensión + prefijo + serie + control.</summary>
    public Resultado<string> Sscc(long serie)
    {
        var digitosSerie = 16 - PrefijoGs1.Length;
        var maximo = (long)Math.Pow(10, digitosSerie) - 1;
        if (serie < 1 || serie > maximo)
        {
            return Resultado.Fallo<string>(Error.Conflicto("agro.sscc_agotado", "Se ha agotado la numeración de SSCC del prefijo GS1."));
        }

        var cuerpo = DigitoExtension.ToString(System.Globalization.CultureInfo.InvariantCulture) + PrefijoGs1
            + serie.ToString(System.Globalization.CultureInfo.InvariantCulture).PadLeft(digitosSerie, '0');
        return Resultado.Ok(ReglasAgro.ConDigitoControl(cuerpo));
    }
}
