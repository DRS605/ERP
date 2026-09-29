using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Agro.Dominio;

/// <summary>Validaciones y utilidades comunes del módulo agro.</summary>
public static class ReglasAgro
{
    public const int LongitudCodigo = 30;
    public const int LongitudNombre = 150;

    public static Error? CodigoNombre(ref string? codigo, ref string? nombre, string ambito)
    {
        codigo = codigo?.Trim().ToUpperInvariant();
        nombre = nombre?.Trim();
        if (string.IsNullOrWhiteSpace(codigo) || codigo.Length > LongitudCodigo)
        {
            return Error.Validacion($"{ambito}.codigo", $"El código es obligatorio (máximo {LongitudCodigo} caracteres).");
        }

        return string.IsNullOrWhiteSpace(nombre) || nombre.Length > LongitudNombre
            ? Error.Validacion($"{ambito}.nombre", $"El nombre es obligatorio (máximo {LongitudNombre} caracteres).")
            : null;
    }

    /// <summary>
    /// Reparte <paramref name="total"/> en proporción a <paramref name="pesos"/> con <paramref name="decimales"/>
    /// decimales, sin perder ni sobrar nada (método del mayor resto): la suma de las partes es el total.
    /// </summary>
    public static IReadOnlyList<decimal> Repartir(decimal total, IReadOnlyList<decimal> pesos, int decimales)
    {
        ArgumentNullException.ThrowIfNull(pesos);
        var sumaPesos = pesos.Sum();
        if (pesos.Count == 0 || sumaPesos <= 0m)
        {
            return pesos.Select(_ => 0m).ToList();
        }

        var escala = (decimal)Math.Pow(10, decimales);
        var unidades = decimal.Round(total * escala, 0, MidpointRounding.AwayFromZero);
        var exactos = pesos.Select(p => unidades * p / sumaPesos).ToList();
        var partes = exactos.Select(e => decimal.Floor(e)).ToList();
        var faltan = (int)(unidades - partes.Sum());
        foreach (var i in exactos.Select((e, i) => (Resto: e - decimal.Floor(e), i)).OrderByDescending(x => x.Resto).ThenBy(x => x.i).Take(Math.Max(0, faltan)).Select(x => x.i))
        {
            partes[i] += 1m;
        }

        return partes.Select(p => p / escala).ToList();
    }

    /// <summary>Dígito de control GS1 correcto (SSCC de 18 dígitos, GTIN…).</summary>
    public static bool Gs1Valido(string? codigo)
    {
        if (codigo is null || codigo.Length is < 8 or > 18 || !codigo.All(char.IsAsciiDigit))
        {
            return false;
        }

        var n = codigo.Length;
        var suma = 0;
        for (var i = 0; i < n - 1; i++)
        {
            suma += (codigo[i] - '0') * ((n - 1 - i) % 2 == 1 ? 3 : 1);
        }

        return (10 - (suma % 10)) % 10 == codigo[n - 1] - '0';
    }

    /// <summary>Completa un SSCC: 17 dígitos (extensión + prefijo de empresa + serie) más el de control.</summary>
    public static string ConDigitoControl(string diecisiete)
    {
        ArgumentNullException.ThrowIfNull(diecisiete);
        var suma = 0;
        for (var i = 0; i < diecisiete.Length; i++)
        {
            suma += (diecisiete[i] - '0') * ((diecisiete.Length - i) % 2 == 1 ? 3 : 1);
        }

        return diecisiete + ((10 - (suma % 10)) % 10).ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}

/// <summary>Campaña agrícola (por ejemplo, 2026/27 de septiembre a agosto).</summary>
public sealed class Campana : RaizAgregadoEmpresa<Guid>
{
    private Campana(Guid id)
        : base(id, Guid.Empty)
    {
        Codigo = null!;
        Nombre = null!;
    }

    private Campana(Guid id, Guid empresaId, string codigo, string nombre, DateOnly desde, DateOnly hasta)
        : base(id, empresaId)
    {
        Codigo = codigo;
        Nombre = nombre;
        Desde = desde;
        Hasta = hasta;
    }

    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    public DateOnly Desde { get; private set; }

    public DateOnly Hasta { get; private set; }

    public bool Contiene(DateOnly fecha) => fecha >= Desde && fecha <= Hasta;

    /// <summary>
    /// Cambia nombre y fechas. Si la campaña ya tiene movimientos (<paramref name="enUso"/>), solo se puede ampliar:
    /// estrecharla dejaría recepciones o liquidaciones fuera de su campaña.
    /// </summary>
    public Resultado Actualizar(string? nombre, DateOnly desde, DateOnly hasta, bool enUso)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return Resultado.Fallo(Error.Validacion("campana.nombre_vacio", "El nombre es obligatorio."));
        }

        if (hasta < desde)
        {
            return Resultado.Fallo(Error.Validacion("campana.fechas", "La campaña termina antes de empezar."));
        }

        if (enUso && (desde > Desde || hasta < Hasta))
        {
            return Resultado.Fallo(Error.Conflicto("campana.en_uso",
                "La campaña ya tiene movimientos: solo se puede ampliar (adelantar el inicio o retrasar el final)."));
        }

        Nombre = nombre.Trim();
        Desde = desde;
        Hasta = hasta;
        return Resultado.Ok();
    }

    public static Resultado<Campana> Crear(Guid empresaId, string? codigo, string? nombre, DateOnly desde, DateOnly hasta)
    {
        var error = ReglasAgro.CodigoNombre(ref codigo, ref nombre, "campana");
        if (error is null && hasta < desde)
        {
            error = Error.Validacion("campana.fechas", "La campaña termina antes de empezar.");
        }

        return error is not null ? Resultado.Fallo<Campana>(error) : Resultado.Ok(new Campana(Guid.NewGuid(), empresaId, codigo!, nombre!, desde, hasta));
    }
}

/// <summary>Régimen fiscal del agricultor en sus ventas a la empresa.</summary>
public enum RegimenAgricultor
{
    /// <summary>Régimen especial de la agricultura (REAGP): se le paga una compensación (12 % agrícola).</summary>
    Reagp = 1,

    /// <summary>Régimen general: se le repercute IVA (4 % frutas y hortalizas).</summary>
    General = 2,
}

/// <summary>
/// Agricultor: un proveedor (Terceros) con su ficha agrícola en la empresa: régimen fiscal, retención de
/// IRPF, desde cuándo autoriza que la empresa le emita la factura (autofacturación) y bloqueo.
/// </summary>
public sealed class Agricultor : RaizAgregadoEmpresa<Guid>
{
    private Agricultor(Guid id)
        : base(id, Guid.Empty)
    {
        Nombre = null!;
    }

    private Agricultor(Guid id, Guid empresaId, Guid proveedorId, string nombre)
        : base(id, empresaId)
    {
        ProveedorId = proveedorId;
        Nombre = nombre;
    }

    public Guid ProveedorId { get; private set; }

    /// <summary>Nombre del proveedor al darlo de alta (para listados).</summary>
    public string Nombre { get; private set; }

    public RegimenAgricultor Regimen { get; private set; }

    /// <summary>Retención de IRPF sobre la base (actividades agrícolas en módulos: 2 %).</summary>
    public decimal PorcentajeRetencion { get; private set; }

    /// <summary>Fecha desde la que el agricultor autoriza la autofacturación. Sin ella no se emiten liquidaciones.</summary>
    public DateOnly? AutofacturacionDesde { get; private set; }

    /// <summary>Si tiene valor, el agricultor está bloqueado: no se le reciben entregas ni se le liquida.</summary>
    public string? MotivoBloqueo { get; private set; }

    /// <summary>
    /// Impuesto de sus autofacturas: la compensación <c>REAGP12</c> (o <c>REAGP105</c> en ganadería) en el
    /// REAGP; en régimen general, el IVA de la fruta (<c>IVA4</c>) o el IGIC en Canarias.
    /// </summary>
    public string CodigoImpuesto { get; private set; } = Impuesto.CompensacionAgricola.Codigo;

    public bool Bloqueado => MotivoBloqueo is not null;

    public static Resultado<Agricultor> Crear(Guid empresaId, Guid proveedorId, string nombre, RegimenAgricultor regimen, decimal retencion, DateOnly? autofacturacionDesde,
        string? codigoImpuesto = null, TipoImpuesto impuestoEmpresa = TipoImpuesto.Iva)
    {
        var a = new Agricultor(Guid.NewGuid(), empresaId, proveedorId, nombre.Trim());
        var r = a.Actualizar(regimen, retencion, autofacturacionDesde, null, codigoImpuesto, impuestoEmpresa, nuevo: true);
        return r.EsFallo ? Resultado.Fallo<Agricultor>(r.Error) : Resultado.Ok(a);
    }

    /// <summary>
    /// Sin código de impuesto se conserva el que tiene si sigue valiendo para el régimen y el territorio, o se pone el de
    /// siempre: la compensación <c>REAGP12</c> (en Canarias, <c>REAGPIGIC</c>) o el IVA de la fruta. En Canarias y régimen
    /// general hay que indicar el tipo de IGIC. El impuesto tiene que ser el del territorio de la empresa (IVA o IGIC).
    /// </summary>
    public Resultado Actualizar(RegimenAgricultor regimen, decimal retencion, DateOnly? autofacturacionDesde, string? motivoBloqueo, string? codigoImpuesto = null,
        TipoImpuesto impuestoEmpresa = TipoImpuesto.Iva, bool nuevo = false)
    {
        if (!Enum.IsDefined(regimen))
        {
            return Resultado.Fallo(Error.Validacion("agricultor.regimen", "El régimen debe ser Reagp o General."));
        }

        var igic = impuestoEmpresa == TipoImpuesto.Igic;
        bool Vale(Impuesto i) => i.Tipo == impuestoEmpresa && i.EsCompensacionReagp == (regimen == RegimenAgricultor.Reagp);
        string? codigo;
        if (!string.IsNullOrWhiteSpace(codigoImpuesto))
        {
            codigo = codigoImpuesto.Trim().ToUpperInvariant();
        }
        else if (!nuevo && Impuesto.PorCodigoImpuesto(CodigoImpuesto) is { EsCorrecto: true } actual && Vale(actual.Valor))
        {
            codigo = actual.Valor.Codigo;
        }
        else
        {
            codigo = regimen == RegimenAgricultor.Reagp
                ? (igic ? Impuesto.ReagpIgic : Impuesto.CompensacionAgricola).Codigo
                : igic ? null : Impuesto.IvaSuperreducido.Codigo;
        }

        if (codigo is null)
        {
            return Resultado.Fallo(Error.Validacion("agricultor.impuesto", "En Canarias, en régimen general, indica el tipo de IGIC de la autofactura."));
        }

        var impuesto = Impuesto.PorCodigoImpuesto(codigo);
        if (impuesto.EsFallo || impuesto.Valor.Tipo == TipoImpuesto.Irpf || !Vale(impuesto.Valor))
        {
            return Resultado.Fallo(Error.Validacion("agricultor.impuesto", (regimen, igic) switch
            {
                (RegimenAgricultor.Reagp, false) => "En el REAGP la autofactura lleva la compensación (REAGP12 o REAGP105).",
                (RegimenAgricultor.Reagp, true) => "En Canarias el agricultor del REAGP va con REAGPIGIC: el adquirente no paga compensación.",
                (_, false) => "En régimen general la autofactura lleva un tipo de IVA.",
                _ => "En Canarias, en régimen general, la autofactura lleva un tipo de IGIC.",
            }));
        }

        CodigoImpuesto = impuesto.Valor.Codigo;

        if (retencion is < 0m or > 50m)
        {
            return Resultado.Fallo(Error.Validacion("agricultor.retencion", "La retención no es válida."));
        }

        Regimen = regimen;
        PorcentajeRetencion = retencion;
        AutofacturacionDesde = autofacturacionDesde;
        MotivoBloqueo = string.IsNullOrWhiteSpace(motivoBloqueo) ? null : motivoBloqueo.Trim();
        return Resultado.Ok();
    }
}

/// <summary>Secano o regadío (dato del recinto en el cuaderno digital, SIEX).</summary>
public enum SistemaCultivo
{
    Secano = 1,
    Regadio = 2,
}

/// <summary>Al aire libre o protegido.</summary>
public enum ModoCultivo
{
    AireLibre = 1,
    Invernadero = 2,
    Malla = 3,
}

/// <summary>Sistema de producción del recinto.</summary>
public enum TipoProduccion
{
    Convencional = 1,
    Integrada = 2,
    Ecologica = 3,
}

/// <summary>Parcela de un agricultor, identificada en SIGPAC. Puede apuntar a un centro analítico (coste por kilo).</summary>
public sealed class Parcela : RaizAgregadoEmpresa<Guid>
{
    private Parcela(Guid id)
        : base(id, Guid.Empty)
    {
        Codigo = null!;
        Nombre = null!;
    }

    private Parcela(Guid id, Guid empresaId, Guid agricultorId, string codigo, string nombre)
        : base(id, empresaId)
    {
        AgricultorId = agricultorId;
        Codigo = codigo;
        Nombre = nombre;
        Activa = true;
    }

    public Guid AgricultorId { get; private set; }

    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    /// <summary>Referencia SIGPAC: provincia:municipio:agregado:zona:polígono:parcela:recinto.</summary>
    public string? ReferenciaSigpac { get; private set; }

    public decimal? SuperficieHa { get; private set; }

    /// <summary>Cultivo (artículo del catálogo) y variedad.</summary>
    public Guid? ProductoId { get; private set; }

    public string? Variedad { get; private set; }

    /// <summary>Centro analítico de la parcela: su coste imputado se divide por los kilos que produce.</summary>
    public Guid? CentroAnaliticoId { get; private set; }

    public bool Activa { get; private set; }

    public SistemaCultivo? Sistema { get; private set; }

    public ModoCultivo? Modo { get; private set; }

    public TipoProduccion Produccion { get; private set; } = TipoProduccion.Convencional;

    public static Resultado<Parcela> Crear(Guid empresaId, Guid agricultorId, DatosParcela datos)
    {
        ArgumentNullException.ThrowIfNull(datos);
        string? codigo = datos.Codigo, nombre = datos.Nombre;
        var error = ReglasAgro.CodigoNombre(ref codigo, ref nombre, "parcela");
        if (error is not null)
        {
            return Resultado.Fallo<Parcela>(error);
        }

        var p = new Parcela(Guid.NewGuid(), empresaId, agricultorId, codigo!, nombre!);
        var r = p.Actualizar(datos with { Codigo = codigo, Nombre = nombre });
        return r.EsFallo ? Resultado.Fallo<Parcela>(r.Error) : Resultado.Ok(p);
    }

    public Resultado Actualizar(DatosParcela datos)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var sigpac = string.IsNullOrWhiteSpace(datos.ReferenciaSigpac) ? null : datos.ReferenciaSigpac.Trim();
        if (sigpac is not null && (sigpac.Split(':').Length != 7 || sigpac.Split(':').Any(p => p.Length == 0 || !p.All(char.IsAsciiDigit))))
        {
            return Resultado.Fallo(Error.Validacion("parcela.sigpac", "La referencia SIGPAC son 7 números separados por «:» (provincia:municipio:agregado:zona:polígono:parcela:recinto)."));
        }

        if (datos.SuperficieHa is <= 0m)
        {
            return Resultado.Fallo(Error.Validacion("parcela.superficie", "La superficie debe ser positiva."));
        }

        if ((datos.Sistema is { } si && !Enum.IsDefined(si)) || (datos.Modo is { } mo && !Enum.IsDefined(mo)) || (datos.Produccion is { } pr && !Enum.IsDefined(pr)))
        {
            return Resultado.Fallo(Error.Validacion("parcela.cultivo", "Sistema (secano o regadío), modo (aire libre, invernadero o malla) o producción (convencional, integrada o ecológica) no válidos."));
        }

        var nombre = datos.Nombre?.Trim();
        if (!string.IsNullOrWhiteSpace(nombre))
        {
            Nombre = nombre;
        }

        ReferenciaSigpac = sigpac;
        SuperficieHa = datos.SuperficieHa;
        ProductoId = datos.ProductoId;
        Variedad = string.IsNullOrWhiteSpace(datos.Variedad) ? null : datos.Variedad.Trim();
        CentroAnaliticoId = datos.CentroAnaliticoId;
        Activa = datos.Activa;
        Sistema = datos.Sistema;
        Modo = datos.Modo;
        Produccion = datos.Produccion ?? TipoProduccion.Convencional;
        return Resultado.Ok();
    }
}

public sealed record DatosParcela(
    string? Codigo, string? Nombre, string? ReferenciaSigpac = null, decimal? SuperficieHa = null, Guid? ProductoId = null,
    string? Variedad = null, Guid? CentroAnaliticoId = null, bool Activa = true, SistemaCultivo? Sistema = null, ModoCultivo? Modo = null,
    TipoProduccion? Produccion = null);

/// <summary>Categoría de clasificación de la fruta (Extra, 1ª, 2ª, destrío…).</summary>
public sealed class Categoria : RaizAgregadoEmpresa<Guid>
{
    private Categoria(Guid id)
        : base(id, Guid.Empty)
    {
        Codigo = null!;
        Nombre = null!;
    }

    private Categoria(Guid id, Guid empresaId, string codigo, string nombre, bool esDestrio, int orden)
        : base(id, empresaId)
    {
        Codigo = codigo;
        Nombre = nombre;
        EsDestrio = esDestrio;
        Orden = orden;
    }

    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    public bool EsDestrio { get; private set; }

    public int Orden { get; private set; }

    /// <summary>Cambia nombre y orden; que sea destrío o no solo mientras no se ha usado (cambiaría liquidaciones).</summary>
    public Resultado Actualizar(string? nombre, bool esDestrio, int orden, bool enUso)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return Resultado.Fallo(Error.Validacion("categoria.nombre_vacio", "El nombre es obligatorio."));
        }

        if (enUso && esDestrio != EsDestrio)
        {
            return Resultado.Fallo(Error.Conflicto("categoria.en_uso", "La categoría ya se ha usado: no se puede cambiar si es destrío."));
        }

        Nombre = nombre.Trim();
        EsDestrio = esDestrio;
        Orden = orden;
        return Resultado.Ok();
    }

    public static Resultado<Categoria> Crear(Guid empresaId, string? codigo, string? nombre, bool esDestrio, int orden)
    {
        var error = ReglasAgro.CodigoNombre(ref codigo, ref nombre, "categoria");
        return error is not null ? Resultado.Fallo<Categoria>(error) : Resultado.Ok(new Categoria(Guid.NewGuid(), empresaId, codigo!, nombre!, esDestrio, orden));
    }
}

/// <summary>Cómo se liquida un artículo en una campaña.</summary>
public enum MetodoLiquidacion
{
    /// <summary>Los kilos de cada partida se reparten por categorías según su clasificación definitiva; cada categoría tiene su precio.</summary>
    PorClasificacion = 1,

    /// <summary>Un precio por kilo vigente en la fecha de recepción (semana, quincena…).</summary>
    PorPeriodo = 2,
}

/// <summary>Método de liquidación de un artículo en una campaña.</summary>
public sealed class ArticuloCampana : RaizAgregadoEmpresa<Guid>
{
    private ArticuloCampana(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private ArticuloCampana(Guid id, Guid empresaId, Guid campanaId, Guid productoId, MetodoLiquidacion metodo)
        : base(id, empresaId)
    {
        CampanaId = campanaId;
        ProductoId = productoId;
        Metodo = metodo;
    }

    public Guid CampanaId { get; private set; }

    public Guid ProductoId { get; private set; }

    public MetodoLiquidacion Metodo { get; private set; }

    public static ArticuloCampana Crear(Guid empresaId, Guid campanaId, Guid productoId, MetodoLiquidacion metodo) =>
        new(Guid.NewGuid(), empresaId, campanaId, productoId, metodo);

    public void CambiarMetodo(MetodoLiquidacion metodo) => Metodo = metodo;
}

/// <summary>
/// Tipo de precio de liquidación, como los de valoración de compras de Hispatec. Al valorar gana el más concreto: el del
/// día, luego el del periodo y luego el general; y, en cada uno, el del envase de la entrega antes que el sin envase.
/// </summary>
public enum TipoPrecioLiquidacion
{
    /// <summary>Precio general del artículo para la campaña (o un tramo largo).</summary>
    General = 1,

    /// <summary>Precio de un periodo de gestión (semana, quincena…).</summary>
    Periodo = 2,

    /// <summary>Precio de un día concreto.</summary>
    Dia = 3,
}

/// <summary>Precio de liquidación (€/kg) de un artículo —y categoría, si se liquida por clasificación— en un periodo de la campaña.</summary>
public sealed class PrecioLiquidacion : RaizAgregadoEmpresa<Guid>
{
    private PrecioLiquidacion(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private PrecioLiquidacion(Guid id, Guid empresaId, Guid campanaId, Guid productoId, Guid? categoriaId, DateOnly desde, DateOnly hasta, decimal precioKg,
        TipoPrecioLiquidacion tipo, Guid? envaseProductoId)
        : base(id, empresaId)
    {
        Tipo = tipo;
        EnvaseProductoId = envaseProductoId;
        CampanaId = campanaId;
        ProductoId = productoId;
        CategoriaId = categoriaId;
        Desde = desde;
        Hasta = hasta;
        PrecioKg = precioKg;
    }

    public Guid CampanaId { get; private set; }

    public Guid ProductoId { get; private set; }

    public Guid? CategoriaId { get; private set; }

    public DateOnly Desde { get; private set; }

    public DateOnly Hasta { get; private set; }

    public decimal PrecioKg { get; private set; }

    public TipoPrecioLiquidacion Tipo { get; private set; } = TipoPrecioLiquidacion.Periodo;

    /// <summary>Envase de la entrega al que se limita (null: cualquiera).</summary>
    public Guid? EnvaseProductoId { get; private set; }

    public bool Vigente(DateOnly fecha) => fecha >= Desde && fecha <= Hasta;

    /// <summary>Prioridad al valorar: día &gt; periodo &gt; general, y con envase antes que sin él.</summary>
    public int Prioridad => (int)Tipo * 2 + (EnvaseProductoId is null ? 0 : 1);

    /// <summary>¿Compite con otro por las mismas entregas (mismo artículo, categoría, envase y tipo, y fechas que se pisan)?</summary>
    public bool Solapa(Guid productoId, Guid? categoriaId, Guid? envaseProductoId, TipoPrecioLiquidacion tipo, DateOnly desde, DateOnly hasta) =>
        ProductoId == productoId && CategoriaId == categoriaId && EnvaseProductoId == envaseProductoId && Tipo == tipo && Desde <= hasta && desde <= Hasta;

    /// <summary>Cambia el periodo o el importe (la base de datos lo impide si ya se aplicó en una liquidación emitida).</summary>
    public Resultado Actualizar(DateOnly desde, DateOnly hasta, decimal precioKg)
    {
        var validado = Crear(EmpresaId, CampanaId, ProductoId, CategoriaId, desde, hasta, precioKg, Tipo, EnvaseProductoId);
        if (validado.EsFallo)
        {
            return Resultado.Fallo(validado.Error);
        }

        Desde = desde;
        Hasta = hasta;
        PrecioKg = precioKg;
        return Resultado.Ok();
    }

    public static Resultado<PrecioLiquidacion> Crear(Guid empresaId, Guid campanaId, Guid productoId, Guid? categoriaId, DateOnly desde, DateOnly hasta, decimal precioKg,
        TipoPrecioLiquidacion tipo = TipoPrecioLiquidacion.Periodo, Guid? envaseProductoId = null)
    {
        if (hasta < desde)
        {
            return Resultado.Fallo<PrecioLiquidacion>(Error.Validacion("precio.fechas", "El periodo del precio termina antes de empezar."));
        }

        if (!Enum.IsDefined(tipo))
        {
            return Resultado.Fallo<PrecioLiquidacion>(Error.Validacion("precio.tipo", "Tipo de precio no válido: General, Periodo o Dia."));
        }

        if (tipo == TipoPrecioLiquidacion.Dia && desde != hasta)
        {
            return Resultado.Fallo<PrecioLiquidacion>(Error.Validacion("precio.dia", "Un precio del día vale para un solo día (desde = hasta)."));
        }

        return precioKg < 0m || decimal.Round(precioKg, 6) != precioKg
            ? Resultado.Fallo<PrecioLiquidacion>(Error.Validacion("precio.importe", "El precio por kilo no puede ser negativo (hasta 6 decimales)."))
            : Resultado.Ok(new PrecioLiquidacion(Guid.NewGuid(), empresaId, campanaId, productoId, categoriaId, desde, hasta, precioKg, tipo, envaseProductoId));
    }
}

/// <summary>Cómo se calcula un descuento de la liquidación.</summary>
public enum TipoConceptoLiquidacion
{
    /// <summary>Tantos euros por kilo liquidado (transporte, manipulación…).</summary>
    PorKilo = 1,

    /// <summary>Porcentaje sobre el importe bruto de la fruta (comisión…).</summary>
    PorcentajeBruto = 2,

    /// <summary>Importe fijo por liquidación (cuota, seguro…).</summary>
    Fijo = 3,

    /// <summary>Tantos euros por envase (bulto) recibido: palot, caja… (Hispatec: importe por unidad en envases).</summary>
    PorEnvase = 4,
}

/// <summary>
/// Concepto de las liquidaciones al agricultor: un descuento (o, con <see cref="Abono"/>, una bonificación que suma).
/// Como los cargos de las recepciones de Hispatec, puede valer solo para un agricultor, un artículo o un envase; entonces
/// se calcula solo sobre las entregas que encajan (sus kilos, su importe o sus envases).
/// </summary>
public sealed class ConceptoLiquidacion : RaizAgregadoEmpresa<Guid>
{
    private ConceptoLiquidacion(Guid id)
        : base(id, Guid.Empty)
    {
        Codigo = null!;
        Nombre = null!;
    }

    private ConceptoLiquidacion(Guid id, Guid empresaId, string codigo, string nombre, TipoConceptoLiquidacion tipo, decimal valor)
        : base(id, empresaId)
    {
        Codigo = codigo;
        Nombre = nombre;
        Tipo = tipo;
        Valor = valor;
        Activo = true;
    }

    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    /// <summary>Solo para este agricultor (null: para todos).</summary>
    public Guid? AgricultorId { get; private set; }

    /// <summary>Solo sobre las entregas de este artículo (null: de todos).</summary>
    public Guid? ProductoId { get; private set; }

    /// <summary>Solo sobre las entregas en este envase (null: en cualquiera).</summary>
    public Guid? EnvaseProductoId { get; private set; }

    /// <summary>Bonificación: suma al importe de la fruta en vez de descontar.</summary>
    public bool Abono { get; private set; }

    public bool ValeParaAgricultor(Guid agricultorId) => AgricultorId is null || AgricultorId == agricultorId;

    public bool ValeParaEntrega(Guid productoId, Guid? envaseProductoId) =>
        (ProductoId is null || ProductoId == productoId) && (EnvaseProductoId is null || EnvaseProductoId == envaseProductoId);

    /// <summary>Si el concepto se limita a unas entregas (artículo o envase).</summary>
    public bool Filtrado => ProductoId is not null || EnvaseProductoId is not null;

    public TipoConceptoLiquidacion Tipo { get; private set; }

    /// <summary>€/kg, porcentaje o euros, según el tipo.</summary>
    public decimal Valor { get; private set; }

    public bool Activo { get; private set; }

    public static Resultado<ConceptoLiquidacion> Crear(Guid empresaId, string? codigo, string? nombre, TipoConceptoLiquidacion tipo, decimal valor,
        Guid? agricultorId = null, Guid? productoId = null, Guid? envaseProductoId = null, bool abono = false)
    {
        var error = ReglasAgro.CodigoNombre(ref codigo, ref nombre, "concepto");
        if (error is null && (!Enum.IsDefined(tipo) || valor < 0m || (tipo == TipoConceptoLiquidacion.PorcentajeBruto && valor > 100m)))
        {
            error = Error.Validacion("concepto.valor", "El tipo o el valor del concepto no son válidos.");
        }

        if (error is not null)
        {
            return Resultado.Fallo<ConceptoLiquidacion>(error);
        }

        var c = new ConceptoLiquidacion(Guid.NewGuid(), empresaId, codigo!, nombre!, tipo, valor)
        {
            AgricultorId = agricultorId, ProductoId = productoId, EnvaseProductoId = envaseProductoId, Abono = abono,
        };
        return Resultado.Ok(c);
    }

    public void FijarActivo(bool activo) => Activo = activo;
}

/// <summary>Recurso que se valora con una tarifa en los partes de confección.</summary>
public enum RecursoCoste
{
    ManoObra = 1,
    Maquina = 2,
}

/// <summary>Tipo de hora de la mano de obra. El destajo se paga por piezas (cajas, kilos…).</summary>
public enum TipoHora
{
    Normal = 1,
    Extra = 2,
    Nocturna = 3,
    Festiva = 4,
    Destajo = 5,
}

/// <summary>Tarifa de coste (€/hora, o €/pieza en destajo) de una categoría de mano de obra o maquinaria, con vigencia.</summary>
public sealed class TarifaCoste : RaizAgregadoEmpresa<Guid>
{
    private TarifaCoste(Guid id)
        : base(id, Guid.Empty)
    {
        Categoria = null!;
    }

    private TarifaCoste(Guid id, Guid empresaId, RecursoCoste recurso, string categoria, TipoHora tipoHora, DateOnly desde, DateOnly? hasta, decimal coste)
        : base(id, empresaId)
    {
        Recurso = recurso;
        Categoria = categoria;
        TipoHora = tipoHora;
        Desde = desde;
        Hasta = hasta;
        CosteUnitario = coste;
    }

    public RecursoCoste Recurso { get; private set; }

    /// <summary>Categoría (peón, encargado, carretilla…).</summary>
    public string Categoria { get; private set; }

    public TipoHora TipoHora { get; private set; }

    public DateOnly Desde { get; private set; }

    public DateOnly? Hasta { get; private set; }

    public decimal CosteUnitario { get; private set; }

    public bool Vigente(DateOnly fecha) => fecha >= Desde && (Hasta is null || fecha <= Hasta);

    /// <summary>
    /// Cambia vigencia y coste. Si ya valoró partes (<paramref name="enUso"/>), el coste y el inicio no cambian (los
    /// partes validados conservan su coste): solo se cierra la vigencia para dar paso a una tarifa nueva.
    /// </summary>
    public Resultado Actualizar(DateOnly desde, DateOnly? hasta, decimal coste, bool enUso)
    {
        if (hasta is { } h && h < desde)
        {
            return Resultado.Fallo(Error.Validacion("tarifa.fechas", "La vigencia termina antes de empezar."));
        }

        if (coste < 0m)
        {
            return Resultado.Fallo(Error.Validacion("tarifa.coste", "El coste no puede ser negativo."));
        }

        if (enUso && (desde != Desde || coste != CosteUnitario))
        {
            return Resultado.Fallo(Error.Conflicto("tarifa.en_uso",
                "La tarifa ya valoró partes: solo se puede cerrar su vigencia. Para otro coste, ciérrala y crea una nueva."));
        }

        Desde = desde;
        Hasta = hasta;
        CosteUnitario = coste;
        return Resultado.Ok();
    }

    public static Resultado<TarifaCoste> Crear(Guid empresaId, RecursoCoste recurso, string? categoria, TipoHora tipoHora, DateOnly desde, DateOnly? hasta, decimal coste)
    {
        var cat = categoria?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(cat) || cat.Length > ReglasAgro.LongitudCodigo)
        {
            return Resultado.Fallo<TarifaCoste>(Error.Validacion("tarifa.categoria", "La categoría es obligatoria."));
        }

        if (!Enum.IsDefined(recurso) || !Enum.IsDefined(tipoHora) || (recurso == RecursoCoste.Maquina && tipoHora != TipoHora.Normal))
        {
            return Resultado.Fallo<TarifaCoste>(Error.Validacion("tarifa.tipo", "La maquinaria solo tiene tarifa por hora normal."));
        }

        if (hasta is { } h && h < desde)
        {
            return Resultado.Fallo<TarifaCoste>(Error.Validacion("tarifa.fechas", "La vigencia termina antes de empezar."));
        }

        return coste < 0m
            ? Resultado.Fallo<TarifaCoste>(Error.Validacion("tarifa.coste", "El coste no puede ser negativo."))
            : Resultado.Ok(new TarifaCoste(Guid.NewGuid(), empresaId, recurso, cat, tipoHora, desde, hasta, coste));
    }
}

/// <summary>
/// Rendimiento teórico de confección, como el de Hispatec: cajas por hora de un producto en un envase (sin envase: el de
/// cualquier envase). Da el tiempo teórico de cada salida del parte: cajas × 3600 / rendimiento.
/// </summary>
public sealed class RendimientoConfeccion : RaizAgregadoEmpresa<Guid>
{
    private RendimientoConfeccion(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private RendimientoConfeccion(Guid id, Guid empresaId, Guid productoId, Guid? envaseProductoId, decimal cajasHora)
        : base(id, empresaId)
    {
        ProductoId = productoId;
        EnvaseProductoId = envaseProductoId;
        CajasHora = cajasHora;
    }

    public Guid ProductoId { get; private set; }

    public Guid? EnvaseProductoId { get; private set; }

    public decimal CajasHora { get; private set; }

    public static Resultado<RendimientoConfeccion> Crear(Guid empresaId, Guid productoId, Guid? envaseProductoId, decimal cajasHora) =>
        Validar(cajasHora) is { } e ? Resultado.Fallo<RendimientoConfeccion>(e)
            : Resultado.Ok(new RendimientoConfeccion(Guid.NewGuid(), empresaId, productoId, envaseProductoId, cajasHora));

    public Resultado Cambiar(decimal cajasHora)
    {
        if (Validar(cajasHora) is { } e)
        {
            return Resultado.Fallo(e);
        }

        CajasHora = cajasHora;
        return Resultado.Ok();
    }

    private static Error? Validar(decimal cajasHora) => cajasHora <= 0m || decimal.Round(cajasHora, 2) != cajasHora
        ? Error.Validacion("rendimiento.cajas_hora", "El rendimiento debe ser mayor que cero (cajas por hora, hasta 2 decimales).")
        : null;
}
