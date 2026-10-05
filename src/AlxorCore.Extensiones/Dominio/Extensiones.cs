using System.Globalization;
using System.Security.Cryptography;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Extensiones.Dominio;

/// <summary>Registro del ERP al que se le pueden poner campos personalizados y adjuntos, con los permisos para verlos y cambiarlos.</summary>
public sealed record EntidadExtensible(string Codigo, string Nombre, string Grupo, string PermisoLeer, string PermisoEditar);

/// <summary>Catálogo de registros extensibles (el código es estable: se guarda con cada valor y cada adjunto).</summary>
public static class EntidadesExtensibles
{
    public static IReadOnlyList<EntidadExtensible> Todas { get; } =
    [
        new("cliente", "Cliente", "Terceros", Nucleo.Autorizacion.Permisos.FacturaLeer, Nucleo.Autorizacion.Permisos.ClienteGestionar),
        new("proveedor", "Proveedor", "Terceros", Nucleo.Autorizacion.Permisos.GastoLeer, Nucleo.Autorizacion.Permisos.GastoGestionar),
        new("producto", "Artículo", "Catálogo", Nucleo.Autorizacion.Permisos.InventarioLeer, Nucleo.Autorizacion.Permisos.ProductoGestionar),
        new("presupuesto", "Presupuesto", "Ventas", Nucleo.Autorizacion.Permisos.FacturaLeer, Nucleo.Autorizacion.Permisos.FacturaCrear),
        new("pedido_venta", "Pedido de venta", "Ventas", Nucleo.Autorizacion.Permisos.FacturaLeer, Nucleo.Autorizacion.Permisos.FacturaCrear),
        new("albaran_venta", "Albarán de venta", "Ventas", Nucleo.Autorizacion.Permisos.FacturaLeer, Nucleo.Autorizacion.Permisos.FacturaCrear),
        new("factura", "Factura", "Ventas", Nucleo.Autorizacion.Permisos.FacturaLeer, Nucleo.Autorizacion.Permisos.FacturaCrear),
        new("reclamacion", "Reclamación", "Ventas", Nucleo.Autorizacion.Permisos.FacturaLeer, Nucleo.Autorizacion.Permisos.FacturaCrear),
        new("gasto", "Factura de proveedor", "Compras", Nucleo.Autorizacion.Permisos.GastoLeer, Nucleo.Autorizacion.Permisos.GastoGestionar),
        new("pedido_compra", "Pedido de compra", "Compras", Nucleo.Autorizacion.Permisos.CompraLeer, Nucleo.Autorizacion.Permisos.CompraGestionar),
        new("asiento", "Asiento", "Contabilidad", Nucleo.Autorizacion.Permisos.ContabilidadLeer, Nucleo.Autorizacion.Permisos.ContabilidadGestionar),
        new("agricultor", "Agricultor", "Agro", Nucleo.Autorizacion.Permisos.AgroLeer, Nucleo.Autorizacion.Permisos.AgroGestionar),
        new("recepcion", "Recepción de fruta", "Agro", Nucleo.Autorizacion.Permisos.AgroLeer, Nucleo.Autorizacion.Permisos.AgroGestionar),
        new("partida", "Partida", "Agro", Nucleo.Autorizacion.Permisos.AgroLeer, Nucleo.Autorizacion.Permisos.AgroGestionar),
        new("pale", "Palé", "Agro", Nucleo.Autorizacion.Permisos.AgroLeer, Nucleo.Autorizacion.Permisos.AgroGestionar),
        new("parte_confeccion", "Parte de confección", "Agro", Nucleo.Autorizacion.Permisos.AgroLeer, Nucleo.Autorizacion.Permisos.AgroGestionar),
        new("liquidacion_agro", "Liquidación al agricultor", "Agro", Nucleo.Autorizacion.Permisos.AgroLeer, Nucleo.Autorizacion.Permisos.AgroLiquidar),
        new("socio", "Socio de la cooperativa", "Cooperativa", Nucleo.Autorizacion.Permisos.CooperativaLeer, Nucleo.Autorizacion.Permisos.CooperativaGestionar),
        new("sesion_subasta", "Sesión de subasta", "Subasta", Nucleo.Autorizacion.Permisos.SubastaLeer, Nucleo.Autorizacion.Permisos.SubastaGestionar),
        new("deposito", "Depósito de bodega", "Bodega", Nucleo.Autorizacion.Permisos.BodegaLeer, Nucleo.Autorizacion.Permisos.BodegaGestionar),
        new("entrada_uva", "Entrada de uva", "Bodega", Nucleo.Autorizacion.Permisos.BodegaLeer, Nucleo.Autorizacion.Permisos.BodegaGestionar),
        new("lote_planta", "Lote de planta", "Vivero", Nucleo.Autorizacion.Permisos.ViveroLeer, Nucleo.Autorizacion.Permisos.ViveroGestionar),
        new("encargo_planta", "Encargo de planta", "Vivero", Nucleo.Autorizacion.Permisos.ViveroLeer, Nucleo.Autorizacion.Permisos.ViveroGestionar),
    ];

    public static EntidadExtensible? Buscar(string? codigo) =>
        Todas.FirstOrDefault(e => string.Equals(e.Codigo, codigo?.Trim(), StringComparison.OrdinalIgnoreCase));
}

public enum TipoCampo
{
    Texto = 1,
    Numero = 2,
    Fecha = 3,

    /// <summary>Sí o no.</summary>
    SiNo = 4,

    /// <summary>Uno de una lista de valores.</summary>
    Lista = 5,
}

/// <summary>Opción de un campo de tipo lista.</summary>
public sealed class OpcionCampo
{
    private OpcionCampo()
    {
        Valor = null!;
    }

    internal OpcionCampo(int orden, string valor)
    {
        Id = Guid.NewGuid();
        Orden = orden;
        Valor = valor;
    }

    public Guid Id { get; private set; }

    public int Orden { get; private set; }

    public string Valor { get; private set; }
}

/// <summary>
/// Campo personalizado (dato complementario) que la empresa añade a un tipo de registro: código, etiqueta, tipo
/// (texto, número, fecha, sí/no o lista), si es obligatorio, valor por defecto y orden. Los valores se guardan
/// normalizados (número y fecha en formato invariante), así que se pueden buscar y comparar.
/// </summary>
public sealed class DefinicionCampo : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudCodigo = 40;
    public const int LongitudEtiqueta = 120;
    public const int LongitudValor = 1000;
    public const int MaximoOpciones = 200;

    private readonly List<OpcionCampo> _opciones = [];

    private DefinicionCampo(Guid id)
        : base(id, Guid.Empty)
    {
        Entidad = null!;
        Codigo = null!;
        Etiqueta = null!;
    }

    private DefinicionCampo(Guid id, Guid empresaId, string entidad, string codigo, TipoCampo tipo)
        : base(id, empresaId)
    {
        Entidad = entidad;
        Codigo = codigo;
        Etiqueta = codigo;
        Tipo = tipo;
        Activo = true;
    }

    public string Entidad { get; private set; }

    public string Codigo { get; private set; }

    public string Etiqueta { get; private set; }

    public TipoCampo Tipo { get; private set; }

    public bool Obligatorio { get; private set; }

    public string? ValorPorDefecto { get; private set; }

    public string? Ayuda { get; private set; }

    public int Orden { get; private set; }

    public bool Activo { get; private set; }

    public IReadOnlyList<OpcionCampo> Opciones => _opciones;

    public static Resultado<DefinicionCampo> Crear(Guid empresaId, string? entidad, string? codigo, TipoCampo tipo, string? etiqueta, bool obligatorio,
        string? valorPorDefecto, IReadOnlyList<string>? opciones, int orden, string? ayuda)
    {
        var e = EntidadesExtensibles.Buscar(entidad);
        if (e is null)
        {
            return Resultado.Fallo<DefinicionCampo>(Error.Validacion("campo.entidad", "Ese tipo de registro no admite campos personalizados."));
        }

        var c = Texto(codigo)?.ToLowerInvariant();
        if (c is null || c.Length > LongitudCodigo || !c.All(ch => char.IsAsciiLetterOrDigit(ch) || ch == '_') || !char.IsAsciiLetter(c[0]))
        {
            return Resultado.Fallo<DefinicionCampo>(Error.Validacion("campo.codigo",
                $"El código empieza por una letra y lleva solo letras, números y guiones bajos (hasta {LongitudCodigo})."));
        }

        if (!Enum.IsDefined(tipo))
        {
            return Resultado.Fallo<DefinicionCampo>(Error.Validacion("campo.tipo", "Tipo de campo no válido."));
        }

        var d = new DefinicionCampo(Guid.NewGuid(), empresaId, e.Codigo, c, tipo);
        var r = d.Cambiar(etiqueta, obligatorio, valorPorDefecto, opciones, orden, ayuda, true);
        return r.EsFallo ? Resultado.Fallo<DefinicionCampo>(r.Error) : Resultado.Ok(d);
    }

    /// <summary>Cambia todo menos el registro, el código y el tipo (los valores ya guardados dependen de ellos).</summary>
    public Resultado Cambiar(string? etiqueta, bool obligatorio, string? valorPorDefecto, IReadOnlyList<string>? opciones, int orden, string? ayuda, bool activo)
    {
        var et = Texto(etiqueta) ?? Codigo;
        if (et.Length > LongitudEtiqueta)
        {
            return Resultado.Fallo(Error.Validacion("campo.etiqueta", $"La etiqueta admite hasta {LongitudEtiqueta} caracteres."));
        }

        var lista = (opciones ?? []).Select(Texto).Where(o => o is not null).Select(o => o!).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (Tipo == TipoCampo.Lista && lista.Count == 0)
        {
            return Resultado.Fallo(Error.Validacion("campo.opciones", "Un campo de lista necesita sus opciones."));
        }

        if (lista.Count > MaximoOpciones || lista.Any(o => o.Length > LongitudEtiqueta))
        {
            return Resultado.Fallo(Error.Validacion("campo.opciones", $"Hasta {MaximoOpciones} opciones de hasta {LongitudEtiqueta} caracteres."));
        }

        var anteriores = _opciones.ToList();
        _opciones.Clear();
        _opciones.AddRange(Tipo == TipoCampo.Lista ? lista.Select((o, i) => new OpcionCampo(i + 1, o)) : []);
        string? defecto = null;
        if (Texto(valorPorDefecto) is { } v)
        {
            var normal = Normalizar(v);
            if (normal.EsFallo)
            {
                _opciones.Clear();
                _opciones.AddRange(anteriores);
                return Resultado.Fallo(Error.Validacion("campo.defecto", $"Valor por defecto: {normal.Error.Mensaje}"));
            }

            defecto = normal.Valor;
        }

        var a = Texto(ayuda);
        Etiqueta = et;
        Obligatorio = obligatorio;
        ValorPorDefecto = defecto;
        Orden = orden;
        Ayuda = a?.Length > 300 ? a[..300] : a;
        Activo = activo;
        return Resultado.Ok();
    }

    /// <summary>
    /// Valida y normaliza un valor del campo: número con punto decimal, fecha AAAA-MM-DD, sí/no como true/false y la
    /// opción tal como está en la lista. Se aceptan también la coma decimal y la fecha DD/MM/AAAA.
    /// </summary>
    public Resultado<string> Normalizar(string valor)
    {
        ArgumentNullException.ThrowIfNull(valor);
        var v = valor.Trim();
        switch (Tipo)
        {
            case TipoCampo.Texto:
                return v.Length > LongitudValor
                    ? Resultado.Fallo<string>(Error.Validacion("campo.valor", $"«{Etiqueta}» admite hasta {LongitudValor} caracteres."))
                    : Resultado.Ok(v);
            case TipoCampo.Numero:
                var texto = v.Contains(',', StringComparison.Ordinal) && !v.Contains('.', StringComparison.Ordinal) ? v.Replace(',', '.') : v;
                return decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out var n)
                    ? Resultado.Ok(n.ToString(CultureInfo.InvariantCulture))
                    : Resultado.Fallo<string>(Error.Validacion("campo.valor", $"«{Etiqueta}» es un número."));
            case TipoCampo.Fecha:
                return DateOnly.TryParseExact(v, ["yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var f)
                    ? Resultado.Ok(f.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
                    : Resultado.Fallo<string>(Error.Validacion("campo.valor", $"«{Etiqueta}» es una fecha (AAAA-MM-DD)."));
            case TipoCampo.SiNo:
                return v.ToLowerInvariant() switch
                {
                    "true" or "si" or "sí" or "1" or "s" => Resultado.Ok("true"),
                    "false" or "no" or "0" or "n" => Resultado.Ok("false"),
                    _ => Resultado.Fallo<string>(Error.Validacion("campo.valor", $"«{Etiqueta}» es sí o no.")),
                };
            default:
                var opcion = _opciones.FirstOrDefault(o => string.Equals(o.Valor, v, StringComparison.OrdinalIgnoreCase));
                return opcion is null
                    ? Resultado.Fallo<string>(Error.Validacion("campo.valor", $"«{Etiqueta}» tiene que ser una de: {string.Join(", ", _opciones.Select(o => o.Valor))}."))
                    : Resultado.Ok(opcion.Valor);
        }
    }

    private static string? Texto(string? t) => string.IsNullOrWhiteSpace(t) ? null : t.Trim();
}

/// <summary>Valor de un campo personalizado en un registro concreto.</summary>
public sealed class ValorCampo : RaizAgregadoEmpresa<Guid>
{
    private ValorCampo(Guid id)
        : base(id, Guid.Empty)
    {
        Entidad = null!;
        Valor = null!;
    }

    private ValorCampo(Guid id, Guid empresaId, Guid definicionId, string entidad, Guid entidadId, string valor, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        DefinicionId = definicionId;
        Entidad = entidad;
        EntidadId = entidadId;
        Valor = valor;
        ActualizadoEn = ahora;
    }

    public Guid DefinicionId { get; private set; }

    public string Entidad { get; private set; }

    public Guid EntidadId { get; private set; }

    public string Valor { get; private set; }

    public DateTimeOffset ActualizadoEn { get; private set; }

    public static ValorCampo Nuevo(DefinicionCampo definicion, Guid entidadId, string valorNormalizado, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(definicion);
        ArgumentNullException.ThrowIfNull(reloj);
        return new ValorCampo(Guid.NewGuid(), definicion.EmpresaId, definicion.Id, definicion.Entidad, entidadId, valorNormalizado, reloj.AhoraUtc);
    }

    public void Cambiar(string valorNormalizado, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        Valor = valorNormalizado;
        ActualizadoEn = reloj.AhoraUtc;
    }
}

/// <summary>Contenido de un adjunto (aparte, para que los listados no carguen los ficheros).</summary>
public sealed class ContenidoAdjunto
{
    private ContenidoAdjunto()
    {
        Datos = null!;
    }

    internal ContenidoAdjunto(byte[] datos)
    {
        Datos = datos;
    }

    public Guid AdjuntoId { get; private set; }

    public byte[] Datos { get; private set; }
}

/// <summary>Fichero adjunto a un registro (foto de un palé, albarán firmado, certificado…).</summary>
public sealed class Adjunto : RaizAgregadoEmpresa<Guid>
{
    public const int TamanoMaximo = 10 * 1024 * 1024;
    public const int LongitudNombre = 200;

    private static readonly Dictionary<string, string> Tipos = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf",
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".webp"] = "image/webp",
        [".gif"] = "image/gif",
        [".txt"] = "text/plain",
        [".csv"] = "text/csv",
        [".xml"] = "application/xml",
        [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        [".msg"] = "application/vnd.ms-outlook",
        [".eml"] = "message/rfc822",
    };

    private Adjunto(Guid id)
        : base(id, Guid.Empty)
    {
        Entidad = null!;
        Nombre = null!;
        TipoMime = null!;
        Huella = null!;
        Contenido = null!;
    }

    private Adjunto(Guid id, Guid empresaId, string entidad, Guid entidadId, string nombre, string tipoMime, byte[] datos, string? descripcion, Guid? usuarioId,
        string? usuario, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Entidad = entidad;
        EntidadId = entidadId;
        Nombre = nombre;
        TipoMime = tipoMime;
        Tamano = datos.Length;
        Huella = Convert.ToHexString(SHA256.HashData(datos));
        Descripcion = descripcion;
        SubidoPorId = usuarioId;
        SubidoPor = usuario;
        SubidoEn = ahora;
        Contenido = new ContenidoAdjunto(datos);
    }

    public string Entidad { get; private set; }

    public Guid EntidadId { get; private set; }

    public string Nombre { get; private set; }

    public string TipoMime { get; private set; }

    public long Tamano { get; private set; }

    /// <summary>SHA-256 del contenido: dice si dos adjuntos son el mismo fichero.</summary>
    public string Huella { get; private set; }

    public string? Descripcion { get; private set; }

    public Guid? SubidoPorId { get; private set; }

    public string? SubidoPor { get; private set; }

    public DateTimeOffset SubidoEn { get; private set; }

    public ContenidoAdjunto Contenido { get; private set; }

    public static Resultado<Adjunto> Crear(Guid empresaId, string? entidad, Guid entidadId, string? nombre, byte[]? datos, string? descripcion, Guid? usuarioId,
        string? usuario, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        var e = EntidadesExtensibles.Buscar(entidad);
        if (e is null)
        {
            return Resultado.Fallo<Adjunto>(Error.Validacion("adjunto.entidad", "Ese tipo de registro no admite adjuntos."));
        }

        if (entidadId == Guid.Empty)
        {
            return Resultado.Fallo<Adjunto>(Error.Validacion("adjunto.registro", "Indica el registro al que se adjunta."));
        }

        var n = string.IsNullOrWhiteSpace(nombre) ? null : Path.GetFileName(nombre.Trim().Replace('\\', '/'));
        if (n is null || n.Length > LongitudNombre)
        {
            return Resultado.Fallo<Adjunto>(Error.Validacion("adjunto.nombre", $"Indica el nombre del fichero (hasta {LongitudNombre} caracteres)."));
        }

        if (!Tipos.TryGetValue(Path.GetExtension(n), out var mime))
        {
            return Resultado.Fallo<Adjunto>(Error.Validacion("adjunto.tipo", $"Tipo de fichero no admitido. Se admiten: {string.Join(", ", Tipos.Keys)}."));
        }

        if (datos is null || datos.Length == 0)
        {
            return Resultado.Fallo<Adjunto>(Error.Validacion("adjunto.vacio", "El fichero está vacío."));
        }

        if (datos.Length > TamanoMaximo)
        {
            return Resultado.Fallo<Adjunto>(Error.Validacion("adjunto.tamano", $"El fichero pasa de {TamanoMaximo / 1024 / 1024} MB."));
        }

        var d = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();
        return Resultado.Ok(new Adjunto(Guid.NewGuid(), empresaId, e.Codigo, entidadId, n, mime, datos, d?.Length > 300 ? d[..300] : d, usuarioId, usuario,
            reloj.AhoraUtc));
    }
}

// ============================================================================================== Alertas

/// <summary>Qué vigila una regla de alerta.</summary>
public enum TipoReglaAlerta
{
    /// <summary>Facturas de clientes vencidas hace más de N días con algo pendiente.</summary>
    FacturasVencidas = 1,

    /// <summary>Clientes cuyo riesgo vivo supera su límite (o el porcentaje indicado de él).</summary>
    RiesgoSuperado = 2,

    /// <summary>Un campo personalizado de tipo fecha que vence dentro de N días (ITV, certificado, revisión…).</summary>
    FechaCampo = 3,

    /// <summary>El certificado electrónico del SII caduca dentro de N días.</summary>
    CertificadoCaducidad = 4,

    /// <summary>Ocurre algo en el ERP (se emite una factura, se registra un gasto, se crea un cliente…).</summary>
    Evento = 5,
}

/// <summary>
/// Regla de alerta: qué se vigila, con qué umbral (días, porcentaje) y quién la ve (los usuarios con un permiso). Las
/// reglas de comprobación se evalúan periódicamente y al pedirlo; las de evento, cuando ocurre.
/// </summary>
public sealed class ReglaAlerta : RaizAgregadoEmpresa<Guid>
{
    /// <summary>Eventos de dominio que pueden disparar una alerta (nombre del evento → descripción).</summary>
    public static IReadOnlyDictionary<string, string> Eventos { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["FacturaEmitida"] = "Se emite una factura",
        ["FacturaAnulada"] = "Se anula una factura",
        ["AlbaranVentaEmitido"] = "Se emite un albarán de venta",
        ["AlbaranVentaAnulado"] = "Se anula un albarán de venta",
        ["GastoRegistrado"] = "Se registra una factura de proveedor",
        ["MovimientoRegistrado"] = "Se registra un cobro o un pago",
        ["ClienteCreado"] = "Se da de alta un cliente",
        ["ProveedorCreado"] = "Se da de alta un proveedor",
        ["ProductoCreado"] = "Se da de alta un artículo",
    };

    private ReglaAlerta(Guid id)
        : base(id, Guid.Empty)
    {
        Nombre = null!;
        PermisoDestino = null!;
    }

    private ReglaAlerta(Guid id, Guid empresaId, TipoReglaAlerta tipo)
        : base(id, empresaId)
    {
        Tipo = tipo;
        Nombre = tipo.ToString();
        PermisoDestino = Nucleo.Autorizacion.Permisos.EmpresaAjustes;
        Activa = true;
    }

    public string Nombre { get; private set; }

    public TipoReglaAlerta Tipo { get; private set; }

    /// <summary>Días: de retraso (facturas), de antelación (fechas, certificado).</summary>
    public int? Dias { get; private set; }

    /// <summary>Porcentaje del límite de riesgo a partir del que avisa (100 = al superarlo).</summary>
    public decimal? Porcentaje { get; private set; }

    /// <summary>Campo de fecha vigilado (en las de <see cref="TipoReglaAlerta.FechaCampo"/>).</summary>
    public Guid? CampoId { get; private set; }

    /// <summary>Evento vigilado (en las de <see cref="TipoReglaAlerta.Evento"/>).</summary>
    public string? Evento { get; private set; }

    /// <summary>Permiso con que se ve la alerta: la ven los usuarios de la empresa que lo tienen.</summary>
    public string PermisoDestino { get; private set; }

    public bool Activa { get; private set; }

    public DateTimeOffset? UltimaEvaluacion { get; private set; }

    public static Resultado<ReglaAlerta> Crear(Guid empresaId, TipoReglaAlerta tipo, string? nombre, int? dias, decimal? porcentaje, Guid? campoId, string? evento,
        string? permisoDestino)
    {
        if (!Enum.IsDefined(tipo))
        {
            return Resultado.Fallo<ReglaAlerta>(Error.Validacion("alerta.tipo", "Tipo de regla no válido."));
        }

        var r = new ReglaAlerta(Guid.NewGuid(), empresaId, tipo);
        var c = r.Cambiar(nombre, dias, porcentaje, campoId, evento, permisoDestino, true);
        return c.EsFallo ? Resultado.Fallo<ReglaAlerta>(c.Error) : Resultado.Ok(r);
    }

    public Resultado Cambiar(string? nombre, int? dias, decimal? porcentaje, Guid? campoId, string? evento, string? permisoDestino, bool activa)
    {
        var n = string.IsNullOrWhiteSpace(nombre) ? null : nombre.Trim();
        if (n is null || n.Length > 120)
        {
            return Resultado.Fallo(Error.Validacion("alerta.nombre", "Ponle un nombre a la regla (hasta 120 caracteres)."));
        }

        if (dias is < 0 or > 3650)
        {
            return Resultado.Fallo(Error.Validacion("alerta.dias", "Los días van de 0 a 3650."));
        }

        if (porcentaje is <= 0m or > 1000m)
        {
            return Resultado.Fallo(Error.Validacion("alerta.porcentaje", "El porcentaje va de 1 a 1000."));
        }

        var permiso = string.IsNullOrWhiteSpace(permisoDestino) ? Nucleo.Autorizacion.Permisos.EmpresaAjustes : permisoDestino.Trim();
        if (!Nucleo.Autorizacion.Permisos.Todos.Contains(permiso))
        {
            return Resultado.Fallo(Error.Validacion("alerta.permiso", "El permiso de destino no existe."));
        }

        switch (Tipo)
        {
            case TipoReglaAlerta.FechaCampo when campoId is null:
                return Resultado.Fallo(Error.Validacion("alerta.campo", "Elige el campo de fecha que se vigila."));
            case TipoReglaAlerta.Evento when evento is null || !Eventos.ContainsKey(evento):
                return Resultado.Fallo(Error.Validacion("alerta.evento", $"Elige el evento: {string.Join(", ", Eventos.Keys)}."));
        }

        Nombre = n;
        Dias = Tipo is TipoReglaAlerta.FechaCampo or TipoReglaAlerta.CertificadoCaducidad or TipoReglaAlerta.FacturasVencidas ? dias ?? 30 : null;
        Porcentaje = Tipo == TipoReglaAlerta.RiesgoSuperado ? porcentaje ?? 100m : null;
        CampoId = Tipo == TipoReglaAlerta.FechaCampo ? campoId : null;
        Evento = Tipo == TipoReglaAlerta.Evento ? evento : null;
        PermisoDestino = permiso;
        Activa = activa;
        return Resultado.Ok();
    }

    public void Evaluada(DateTimeOffset ahora) => UltimaEvaluacion = ahora;
}

/// <summary>
/// Una alerta: lo que encontró una regla (un hallazgo con su clave, para no repetirlo mientras siga vivo). Se resuelve
/// a mano o sola, cuando la regla deja de encontrarlo; cada usuario la marca como leída.
/// </summary>
public sealed class Alerta : RaizAgregadoEmpresa<Guid>
{
    private Alerta(Guid id)
        : base(id, Guid.Empty)
    {
        Clave = null!;
        Titulo = null!;
        PermisoDestino = null!;
    }

    private Alerta(Guid id, Guid empresaId, Guid reglaId, string clave, string titulo, string? detalle, string? entidad, Guid? entidadId, string permiso, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        ReglaId = reglaId;
        Clave = clave;
        Titulo = titulo;
        Detalle = detalle;
        Entidad = entidad;
        EntidadId = entidadId;
        PermisoDestino = permiso;
        CreadaEn = ahora;
    }

    public Guid ReglaId { get; private set; }

    /// <summary>Lo que identifica el hallazgo (p. ej. la factura): mientras haya una alerta viva con esa clave no se crea otra.</summary>
    public string Clave { get; private set; }

    public string Titulo { get; private set; }

    public string? Detalle { get; private set; }

    public string? Entidad { get; private set; }

    public Guid? EntidadId { get; private set; }

    public string PermisoDestino { get; private set; }

    public DateTimeOffset CreadaEn { get; private set; }

    public DateTimeOffset? ResueltaEn { get; private set; }

    /// <summary>Quién la resolvió (null: se resolvió sola).</summary>
    public string? ResueltaPor { get; private set; }

    public bool Viva => ResueltaEn is null;

    public static Alerta Nueva(ReglaAlerta regla, string clave, string titulo, string? detalle, string? entidad, Guid? entidadId, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(regla);
        ArgumentNullException.ThrowIfNull(reloj);
        static string Corta(string t, int n) => t.Length > n ? t[..n] : t;
        return new Alerta(Guid.NewGuid(), regla.EmpresaId, regla.Id, Corta(clave, 200), Corta(titulo, 200), detalle is null ? null : Corta(detalle, 1000), entidad, entidadId,
            regla.PermisoDestino, reloj.AhoraUtc);
    }

    public Resultado Resolver(string? quien, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (!Viva)
        {
            return Resultado.Fallo(Error.Conflicto("alerta.resuelta", "La alerta ya está resuelta."));
        }

        ResueltaEn = reloj.AhoraUtc;
        ResueltaPor = string.IsNullOrWhiteSpace(quien) ? null : quien.Trim();
        return Resultado.Ok();
    }
}

/// <summary>Lectura de una alerta por un usuario.</summary>
public sealed class LecturaAlerta : RaizAgregadoEmpresa<Guid>
{
    private LecturaAlerta(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private LecturaAlerta(Guid id, Guid empresaId, Guid alertaId, Guid usuarioId, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        AlertaId = alertaId;
        UsuarioId = usuarioId;
        LeidaEn = ahora;
    }

    public Guid AlertaId { get; private set; }

    public Guid UsuarioId { get; private set; }

    public DateTimeOffset LeidaEn { get; private set; }

    public static LecturaAlerta Nueva(Alerta alerta, Guid usuarioId, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(alerta);
        ArgumentNullException.ThrowIfNull(reloj);
        return new LecturaAlerta(Guid.NewGuid(), alerta.EmpresaId, alerta.Id, usuarioId, reloj.AhoraUtc);
    }
}
