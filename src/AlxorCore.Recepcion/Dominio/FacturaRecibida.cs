using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Recepcion.Dominio;

/// <summary>Cómo ha llegado la factura de proveedor al sistema.</summary>
public enum OrigenRecepcion
{
    /// <summary>Llegó automáticamente al buzón de correo configurado.</summary>
    Correo = 1,

    /// <summary>La subió manualmente una persona.</summary>
    Manual = 2,
}

/// <summary>
/// Ciclo de vida de una factura recibida. Nunca se contabiliza algo que no se ha validado
/// por completo: el paso a <see cref="EstadoRecepcion.Contabilizada"/> exige pasar antes por
/// <see cref="EstadoRecepcion.Validada"/>.
/// </summary>
public enum EstadoRecepcion
{
    /// <summary>Está en la bandeja de entrada, pendiente de revisión.</summary>
    Recibida = 1,

    /// <summary>Una persona revisó y confirmó sus datos; lista para contabilizar.</summary>
    Validada = 2,

    /// <summary>Contabilizada: generó un gasto (y su IVA soportado).</summary>
    Contabilizada = 3,

    /// <summary>Descartada (duplicada, no es una factura, error…).</summary>
    Rechazada = 4,
}

/// <summary>
/// Factura de proveedor recibida (por correo o subida a mano) que espera en la bandeja de
/// entrada hasta que una persona la valida y la contabiliza. La contabilización delega en el
/// puerto de aplicación <c>IContabilizador</c> (hoy crea un gasto con IVA soportado; mañana
/// podrá generar además el asiento de partida doble sin cambiar este agregado).
/// </summary>
public sealed class FacturaRecibida : RaizAgregadoEmpresa<Guid>
{
    /// <summary>Longitud máxima del nombre del archivo adjunto.</summary>
    public const int LongitudMaximaArchivo = 260;

    private FacturaRecibida(Guid id)
        : base(id, Guid.Empty)
    {
        NombreArchivo = null!;
        TipoContenido = null!;
        Contenido = Array.Empty<byte>();
    }

    private FacturaRecibida(Guid id, Guid empresaId, OrigenRecepcion origen, string? remitenteCorreo,
        string? asuntoCorreo, string nombreArchivo, string tipoContenido, byte[] contenido, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Origen = origen;
        RemitenteCorreo = remitenteCorreo;
        AsuntoCorreo = asuntoCorreo;
        NombreArchivo = nombreArchivo;
        TipoContenido = tipoContenido;
        Contenido = contenido;
        FechaRecepcion = ahora;
        Estado = EstadoRecepcion.Recibida;
    }

    public OrigenRecepcion Origen { get; private set; }

    public DateTimeOffset FechaRecepcion { get; private set; }

    public string? RemitenteCorreo { get; private set; }

    public string? AsuntoCorreo { get; private set; }

    public string NombreArchivo { get; private set; }

    public string TipoContenido { get; private set; }

    /// <summary>Contenido binario del adjunto (el PDF de la factura).</summary>
    public byte[] Contenido { get; private set; }

    public EstadoRecepcion Estado { get; private set; }

    // --- Datos de la factura (nulos hasta que se validan) ---
    public Guid? ProveedorId { get; private set; }

    public string? ProveedorTexto { get; private set; }

    public string? NumeroFactura { get; private set; }

    public DateOnly? FechaFactura { get; private set; }

    public decimal? BaseImponible { get; private set; }

    public string? CodigoIva { get; private set; }

    public decimal? PorcentajeIrpf { get; private set; }

    /// <summary>Gasto generado al contabilizar (enlace, no dependencia).</summary>
    public Guid? GastoId { get; private set; }

    public string? MotivoRechazo { get; private set; }

    /// <summary>Da de alta una factura en la bandeja de entrada.</summary>
    public static Resultado<FacturaRecibida> Recibir(Guid empresaId, OrigenRecepcion origen,
        string? remitenteCorreo, string? asuntoCorreo, string? nombreArchivo, string? tipoContenido,
        byte[]? contenido, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (string.IsNullOrWhiteSpace(nombreArchivo))
        {
            return Resultado.Fallo<FacturaRecibida>(Error.Validacion("recepcion.archivo_vacio", "El nombre del archivo es obligatorio."));
        }

        if (nombreArchivo.Length > LongitudMaximaArchivo)
        {
            return Resultado.Fallo<FacturaRecibida>(Error.Validacion("recepcion.archivo_largo", $"El nombre del archivo supera {LongitudMaximaArchivo} caracteres."));
        }

        if (contenido is null || contenido.Length == 0)
        {
            return Resultado.Fallo<FacturaRecibida>(Error.Validacion("recepcion.contenido_vacio", "El documento adjunto está vacío."));
        }

        var factura = new FacturaRecibida(Guid.NewGuid(), empresaId, origen, remitenteCorreo?.Trim(),
            asuntoCorreo?.Trim(), nombreArchivo.Trim(), string.IsNullOrWhiteSpace(tipoContenido) ? "application/octet-stream" : tipoContenido.Trim(),
            contenido, reloj.AhoraUtc);
        factura.RegistrarEvento(new FacturaRecibidaRegistrada(factura.Id, empresaId, factura.FechaRecepcion));
        return Resultado.Ok(factura);
    }

    /// <summary>
    /// Confirma o corrige los datos de la factura. Deja la factura <see cref="EstadoRecepcion.Validada"/>,
    /// lista para contabilizar. Se puede validar de nuevo (editar) mientras no esté contabilizada.
    /// </summary>
    public Resultado Validar(Guid? proveedorId, string? proveedorTexto, string? numeroFactura,
        DateOnly? fechaFactura, decimal baseImponible, string? codigoIva, decimal porcentajeIrpf)
    {
        if (Estado is EstadoRecepcion.Contabilizada)
        {
            return Resultado.Fallo(Error.Conflicto("recepcion.ya_contabilizada", "La factura ya está contabilizada."));
        }

        if (Estado is EstadoRecepcion.Rechazada)
        {
            return Resultado.Fallo(Error.Conflicto("recepcion.rechazada", "La factura está rechazada."));
        }

        if (proveedorId is null && string.IsNullOrWhiteSpace(proveedorTexto))
        {
            return Resultado.Fallo(Error.Validacion("recepcion.proveedor_vacio", "Indica el proveedor de la factura."));
        }

        if (fechaFactura is null)
        {
            return Resultado.Fallo(Error.Validacion("recepcion.fecha_vacia", "La fecha de la factura es obligatoria."));
        }

        if (baseImponible <= 0m)
        {
            return Resultado.Fallo(Error.Validacion("recepcion.base_invalida", "La base imponible debe ser mayor que cero."));
        }

        if (porcentajeIrpf is < 0m or > 60m)
        {
            return Resultado.Fallo(Error.Validacion("recepcion.irpf_invalido", "La retención de IRPF debe estar entre 0 % y 60 %."));
        }

        var impuesto = Impuesto.PorCodigoImpuesto(string.IsNullOrWhiteSpace(codigoIva) ? Impuesto.IvaGeneral.Codigo : codigoIva);
        if (impuesto.EsFallo)
        {
            return Resultado.Fallo(impuesto.Error);
        }

        ProveedorId = proveedorId;
        ProveedorTexto = string.IsNullOrWhiteSpace(proveedorTexto) ? ProveedorTexto : proveedorTexto.Trim();
        NumeroFactura = string.IsNullOrWhiteSpace(numeroFactura) ? null : numeroFactura.Trim();
        FechaFactura = fechaFactura;
        BaseImponible = Redondeo.Dos(baseImponible);
        CodigoIva = impuesto.Valor.Codigo;
        PorcentajeIrpf = Redondeo.Dos(porcentajeIrpf);
        Estado = EstadoRecepcion.Validada;
        return Resultado.Ok();
    }

    /// <summary>Marca la factura como contabilizada, enlazando el gasto generado.</summary>
    public Resultado Contabilizar(Guid gastoId, DateTimeOffset ahora)
    {
        if (Estado is EstadoRecepcion.Contabilizada)
        {
            return Resultado.Fallo(Error.Conflicto("recepcion.ya_contabilizada", "La factura ya está contabilizada."));
        }

        if (Estado is not EstadoRecepcion.Validada)
        {
            return Resultado.Fallo(Error.Conflicto("recepcion.no_validada", "Valida la factura antes de contabilizarla."));
        }

        GastoId = gastoId;
        Estado = EstadoRecepcion.Contabilizada;
        RegistrarEvento(new FacturaRecibidaContabilizada(Id, EmpresaId, gastoId, ahora));
        return Resultado.Ok();
    }

    /// <summary>Descarta la factura (duplicada, ilegible, no es una factura…).</summary>
    public Resultado Rechazar(string? motivo)
    {
        if (Estado is EstadoRecepcion.Contabilizada)
        {
            return Resultado.Fallo(Error.Conflicto("recepcion.ya_contabilizada", "No puedes rechazar una factura ya contabilizada."));
        }

        MotivoRechazo = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();
        Estado = EstadoRecepcion.Rechazada;
        return Resultado.Ok();
    }
}

/// <summary>Se registró una factura en la bandeja de entrada.</summary>
public sealed record FacturaRecibidaRegistrada(Guid FacturaRecibidaId, Guid EmpresaId, DateTimeOffset OcurridoEn) : IEventoDominio;

/// <summary>Una factura recibida se contabilizó (generó un gasto).</summary>
public sealed record FacturaRecibidaContabilizada(Guid FacturaRecibidaId, Guid EmpresaId, Guid GastoId, DateTimeOffset OcurridoEn) : IEventoDominio;
