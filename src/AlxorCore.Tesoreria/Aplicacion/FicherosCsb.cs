using System.Globalization;
using System.Text;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Terceros.Aplicacion;
using AlxorCore.Tesoreria.Dominio;

namespace AlxorCore.Tesoreria.Aplicacion;

/// <summary>
/// Constructor de registros de ancho fijo para los cuadernos AEB clásicos (texto plano). Cada línea
/// tiene una longitud exacta; los alfanuméricos se ajustan a la izquierda (rellenando con espacios) y
/// los numéricos a la derecha (rellenando con ceros). Se conserva la Ñ y se quitan los acentos.
/// </summary>
internal sealed class RegistroCsb
{
    private readonly StringBuilder _sb = new();

    public RegistroCsb Alfa(string? valor, int longitud)
    {
        var limpio = TextoAeat.Mayusculas(valor ?? string.Empty);
        limpio = limpio.Length > longitud ? limpio[..longitud] : limpio.PadRight(longitud);
        _sb.Append(limpio);
        return this;
    }

    /// <summary>Añade un importe en céntimos, sin signo, ajustado con ceros a la izquierda.</summary>
    public RegistroCsb Importe(decimal valor, int longitud)
    {
        var centimos = (long)Math.Round(valor * 100m, MidpointRounding.AwayFromZero);
        return Num(centimos, longitud);
    }

    public RegistroCsb Num(long valor, int longitud)
    {
        var s = valor.ToString(CultureInfo.InvariantCulture);
        s = s.Length > longitud ? s[^longitud..] : s.PadLeft(longitud, '0');
        _sb.Append(s);
        return this;
    }

    public RegistroCsb Ceros(int longitud) => Num(0, longitud);

    public RegistroCsb Espacios(int longitud)
    {
        _sb.Append(new string(' ', longitud));
        return this;
    }

    public string Construir(int longitud)
    {
        var s = _sb.ToString();
        return s.Length > longitud ? s[..longitud] : s.PadRight(longitud);
    }
}

/// <summary>Utilidades de texto para los ficheros AEB (mayúsculas sin acentos, conservando la Ñ).</summary>
internal static class TextoAeat
{
    public static string Mayusculas(string valor)
    {
        var sb = new StringBuilder(valor.Length);
        foreach (var c in valor.ToUpperInvariant())
        {
            sb.Append(c switch
            {
                'Á' => 'A', 'À' => 'A', 'Ä' => 'A', 'Â' => 'A',
                'É' => 'E', 'È' => 'E', 'Ë' => 'E', 'Ê' => 'E',
                'Í' => 'I', 'Ì' => 'I', 'Ï' => 'I', 'Î' => 'I',
                'Ó' => 'O', 'Ò' => 'O', 'Ö' => 'O', 'Ô' => 'O',
                'Ú' => 'U', 'Ù' => 'U', 'Ü' => 'U', 'Û' => 'U',
                'Ç' => 'C',
                _ => c,
            });
        }

        return sb.ToString();
    }
}

/// <summary>Resultado de generar un fichero bancario de ancho fijo (CSB/AEB).</summary>
public sealed record FicheroCsbDto(string Fichero, string NombreArchivo, int NumeroRegistros, decimal Total, IReadOnlyList<string> Omitidos);

/// <summary>
/// Genera el <b>Cuaderno 19 clásico</b> (adeudos por domiciliación, texto AEB de 162 posiciones) para
/// las facturas indicadas. Es un formato heredado y sus offsets pueden variar por entidad: valídalo
/// con el servicio de pruebas de tu banco antes de usarlo en producción (para la mayoría de bancos el
/// formato vigente es el SEPA XML pain.008 / Norma 19.14, disponible en «Remesas SEPA»).
/// </summary>
public sealed class GenerarCuaderno19
{
    private const int Longitud = 162;

    private readonly IConsultaFacturas _facturas;
    private readonly IConsultaClientes _clientes;
    private readonly IConsultaEmpresas _empresas;
    private readonly IRepositorioMovimientos _movimientos;
    private readonly IReloj _reloj;

    public GenerarCuaderno19(IConsultaFacturas facturas, IConsultaClientes clientes, IConsultaEmpresas empresas, IRepositorioMovimientos movimientos, IReloj reloj)
    {
        _facturas = facturas;
        _clientes = clientes;
        _empresas = empresas;
        _movimientos = movimientos;
        _reloj = reloj;
    }

    public async Task<Resultado<FicheroCsbDto>> EjecutarAsync(Guid empresaId, GenerarRemesaComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        if (comando.FacturaIds is null || comando.FacturaIds.Count == 0)
        {
            return Resultado.Fallo<FicheroCsbDto>(Error.Validacion("csb19.sin_facturas", "Selecciona al menos una factura."));
        }

        var empresa = await _empresas.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        if (empresa is null || string.IsNullOrWhiteSpace(empresa.Iban) || string.IsNullOrWhiteSpace(empresa.IdentificadorAcreedor))
        {
            return Resultado.Fallo<FicheroCsbDto>(Error.Validacion("csb19.empresa_sin_datos_cobro", "Configura el IBAN y el identificador de acreedor de la empresa (Ajustes → Datos de cobro)."));
        }

        var adeudos = new List<(string Nombre, string Iban, string Referencia, decimal Importe)>();
        var omitidos = new List<string>();
        foreach (var id in comando.FacturaIds)
        {
            var f = await _facturas.ObtenerAsync(id, ct).ConfigureAwait(false);
            if (f is null || f.ClienteId is null)
            {
                omitidos.Add($"{id}: factura no encontrada o sin cliente.");
                continue;
            }

            var liquidado = await _movimientos.SumaAsync(TipoDocumentoTesoreria.Factura, id, ct).ConfigureAwait(false);
            var pendiente = Redondeo.Dos(f.Total - liquidado);
            if (pendiente <= 0m)
            {
                omitidos.Add($"{f.NumeroCompleto}: ya está cobrada.");
                continue;
            }

            var cliente = await _clientes.ObtenerAsync(f.ClienteId.Value, ct).ConfigureAwait(false);
            if (cliente is null || string.IsNullOrWhiteSpace(cliente.Iban))
            {
                omitidos.Add($"{f.NumeroCompleto}: el cliente no tiene IBAN.");
                continue;
            }

            adeudos.Add((cliente.Nombre, cliente.Iban!, f.NumeroCompleto, pendiente));
        }

        if (adeudos.Count == 0)
        {
            return Resultado.Fallo<FicheroCsbDto>(Error.Validacion("csb19.sin_adeudos", "Ninguna factura se puede domiciliar. " + string.Join(" ", omitidos)));
        }

        var hoy = DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var fecha = hoy.ToString("ddMMyy", CultureInfo.InvariantCulture);
        var total = Redondeo.Dos(adeudos.Sum(a => a.Importe));
        var nif = empresa.IdentificadorAcreedor!;

        var lineas = new List<string>
        {
            // Cabecera de presentador (código 51 · dato 80).
            new RegistroCsb().Alfa("5180", 4).Alfa(nif, 12).Espacios(6).Alfa(empresa.RazonSocial, 40).Espacios(20).Alfa(fecha, 6).Espacios(74).Construir(Longitud),
            // Cabecera de ordenante (código 51 · dato 70).
            new RegistroCsb().Alfa("5170", 4).Alfa(nif, 12).Espacios(6).Alfa(empresa.RazonSocial, 40).Alfa(fecha, 6).Espacios(4).Alfa(empresa.Iban, 34).Espacios(56).Construir(Longitud),
        };

        var orden = 1;
        foreach (var a in adeudos)
        {
            // Registro individual de adeudo (código 56 · dato 70).
            lineas.Add(new RegistroCsb()
                .Alfa("5670", 4).Alfa(nif, 12)
                .Alfa(orden.ToString("D12", CultureInfo.InvariantCulture), 12)
                .Alfa(a.Nombre, 40).Alfa(a.Iban, 34)
                .Importe(a.Importe, 10)
                .Alfa(a.Referencia, 16)
                .Espacios(34).Construir(Longitud));
            orden++;
        }

        // Total de ordenante (código 58) y total general (código 59).
        lineas.Add(new RegistroCsb().Alfa("5870", 4).Alfa(nif, 12).Espacios(72).Importe(total, 10).Num(adeudos.Count, 10).Espacios(54).Construir(Longitud));
        lineas.Add(new RegistroCsb().Alfa("5980", 4).Alfa(nif, 12).Espacios(72).Importe(total, 10).Num(adeudos.Count, 10).Num(adeudos.Count + 3, 10).Espacios(44).Construir(Longitud));

        var fichero = string.Join("\r\n", lineas) + "\r\n";
        return Resultado.Ok(new FicheroCsbDto(fichero, $"cuaderno19-{hoy:yyyyMMdd}.txt", adeudos.Count, total, omitidos));
    }
}

/// <summary>
/// Genera un fichero de <b>Confirming (Cuaderno 68)</b> para que el banco gestione el pago a los
/// proveedores de los gastos indicados (texto AEB de 100 posiciones). Formato heredado y dependiente
/// de la entidad: valídalo con el servicio de pruebas de tu banco antes de usarlo en producción.
/// </summary>
public sealed class GenerarConfirming
{
    private const int Longitud = 100;

    private readonly IConsultaGastos _gastos;
    private readonly IConsultaProveedores _proveedores;
    private readonly IConsultaEmpresas _empresas;
    private readonly IRepositorioMovimientos _movimientos;
    private readonly IReloj _reloj;

    public GenerarConfirming(IConsultaGastos gastos, IConsultaProveedores proveedores, IConsultaEmpresas empresas, IRepositorioMovimientos movimientos, IReloj reloj)
    {
        _gastos = gastos;
        _proveedores = proveedores;
        _empresas = empresas;
        _movimientos = movimientos;
        _reloj = reloj;
    }

    public async Task<Resultado<FicheroCsbDto>> EjecutarAsync(Guid empresaId, GenerarPagosComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var recogida = await Pagos.RecopilarAsync(empresaId, comando, _gastos, _proveedores, _empresas, _movimientos, exigeIban: false, ct).ConfigureAwait(false);
        if (recogida.EsFallo)
        {
            return Resultado.Fallo<FicheroCsbDto>(recogida.Error);
        }

        var (empresa, pagos, omitidos) = recogida.Valor;
        var hoy = DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var fecha = (comando.FechaPago ?? hoy.AddDays(1)).ToString("ddMMyy", CultureInfo.InvariantCulture);
        var total = Redondeo.Dos(pagos.Sum(p => p.Importe));
        var nif = empresa.Nif;

        var lineas = new List<string>
        {
            // Registro de cabecera (tipo 01).
            new RegistroCsb().Alfa("01", 2).Alfa("68", 2).Alfa(nif, 12).Alfa(empresa.RazonSocial, 40).Alfa(fecha, 6).Alfa(empresa.Iban, 34).Construir(Longitud),
        };

        foreach (var p in pagos)
        {
            // Registro de detalle de proveedor/factura (tipo 02).
            lineas.Add(new RegistroCsb()
                .Alfa("02", 2).Alfa(nif, 12)
                .Alfa(p.ProveedorNombre, 40)
                .Alfa(p.Iban, 24)
                .Importe(p.Importe, 12)
                .Alfa(p.Concepto, 10).Construir(Longitud));
        }

        // Registro de totales (tipo 09).
        lineas.Add(new RegistroCsb().Alfa("09", 2).Alfa("68", 2).Importe(total, 14).Num(pagos.Count, 8).Espacios(74).Construir(Longitud));

        var fichero = string.Join("\r\n", lineas) + "\r\n";
        return Resultado.Ok(new FicheroCsbDto(fichero, $"confirming-{hoy:yyyyMMdd}.txt", pagos.Count, total, omitidos));
    }
}
