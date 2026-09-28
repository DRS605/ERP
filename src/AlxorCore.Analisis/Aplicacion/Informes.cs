using AlxorCore.Analisis.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Analisis.Aplicacion;

public interface IRepositorioInformesAnalisis
{
    /// <summary>Los del usuario y los compartidos de la empresa.</summary>
    Task<IReadOnlyList<InformeAnalisis>> ListarAsync(Guid empresaId, Guid usuarioId, CancellationToken ct = default);

    Task<InformeAnalisis?> ObtenerAsync(Guid id, CancellationToken ct = default);

    void Agregar(InformeAnalisis informe);

    void Eliminar(InformeAnalisis informe);
}

public interface IUnidadDeTrabajoAnalisis : IUnidadDeTrabajo;

public sealed record InformeAnalisisDto(Guid Id, string Nombre, string Dataset, string Definicion, bool Compartido, bool Favorito, bool Propio, DateTimeOffset ActualizadoEn)
{
    public static InformeAnalisisDto Desde(InformeAnalisis i, Guid usuarioId) =>
        new(i.Id, i.Nombre, i.Dataset, i.Definicion, i.Compartido, i.Favorito, i.UsuarioId == usuarioId, i.ActualizadoEn);
}

public sealed record GuardarInformeComando(string? Nombre, string? Dataset, string? Definicion, bool Compartido = false, bool Favorito = false);

/// <summary>Informes de análisis guardados: los propios y los compartidos; solo el autor los cambia o borra.</summary>
public sealed class GestionInformesAnalisis
{
    private readonly IRepositorioInformesAnalisis _repo;
    private readonly IUnidadDeTrabajoAnalisis _unidad;
    private readonly IReloj _reloj;

    public GestionInformesAnalisis(IRepositorioInformesAnalisis repo, IUnidadDeTrabajoAnalisis unidad, IReloj reloj)
    {
        _repo = repo;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<IReadOnlyList<InformeAnalisisDto>> ListarAsync(Guid empresaId, Guid usuarioId, CancellationToken ct = default) =>
        (await _repo.ListarAsync(empresaId, usuarioId, ct).ConfigureAwait(false))
            .OrderByDescending(i => i.Favorito).ThenBy(i => i.Nombre, StringComparer.CurrentCulture)
            .Select(i => InformeAnalisisDto.Desde(i, usuarioId)).ToList();

    public async Task<Resultado<InformeAnalisisDto>> CrearAsync(Guid empresaId, Guid usuarioId, GuardarInformeComando c, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(c);
        if (CatalogoDatasets.Buscar(c.Dataset) is null)
        {
            return Resultado.Fallo<InformeAnalisisDto>(Error.Validacion("informe.dataset", $"No existe el conjunto de datos «{c.Dataset}»."));
        }

        var r = InformeAnalisis.Crear(empresaId, usuarioId, c.Nombre, c.Dataset, c.Definicion, c.Compartido, c.Favorito, _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<InformeAnalisisDto>(r.Error);
        }

        _repo.Agregar(r.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(InformeAnalisisDto.Desde(r.Valor, usuarioId));
    }

    public async Task<Resultado<InformeAnalisisDto>> ActualizarAsync(Guid id, Guid usuarioId, GuardarInformeComando c, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(c);
        var informe = await PropioAsync(id, usuarioId, ct).ConfigureAwait(false);
        if (informe.EsFallo)
        {
            return Resultado.Fallo<InformeAnalisisDto>(informe.Error);
        }

        if (CatalogoDatasets.Buscar(c.Dataset) is null)
        {
            return Resultado.Fallo<InformeAnalisisDto>(Error.Validacion("informe.dataset", $"No existe el conjunto de datos «{c.Dataset}»."));
        }

        var r = informe.Valor.Actualizar(c.Nombre, c.Dataset, c.Definicion, c.Compartido, c.Favorito, _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<InformeAnalisisDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(InformeAnalisisDto.Desde(informe.Valor, usuarioId));
    }

    public async Task<Resultado> EliminarAsync(Guid id, Guid usuarioId, CancellationToken ct = default)
    {
        var informe = await PropioAsync(id, usuarioId, ct).ConfigureAwait(false);
        if (informe.EsFallo)
        {
            return Resultado.Fallo(informe.Error);
        }

        _repo.Eliminar(informe.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    private async Task<Resultado<InformeAnalisis>> PropioAsync(Guid id, Guid usuarioId, CancellationToken ct)
    {
        var informe = await _repo.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (informe is null || (informe.UsuarioId != usuarioId && !informe.Compartido))
        {
            return Resultado.Fallo<InformeAnalisis>(Error.NoEncontrado("informe.no_encontrado", "No se encontró el informe."));
        }

        return informe.UsuarioId != usuarioId
            ? Resultado.Fallo<InformeAnalisis>(Error.Prohibido("informe.ajeno", "Solo quien creó el informe puede cambiarlo o borrarlo (puedes guardarlo como uno nuevo)."))
            : Resultado.Ok(informe);
    }
}

/// <summary>Informe listo para usar (galería): una consulta con un nombre y un periodo relativo sugerido.</summary>
public sealed record PlantillaAnalisis(string Clave, string Nombre, string Descripcion, string Periodo, ConsultaAnalisis Consulta, string? Grafico = null);

/// <summary>Informes predefinidos de la galería del análisis.</summary>
public static class PlantillasAnalisis
{
    public static IReadOnlyList<PlantillaAnalisis> Todas { get; } =
    [
        new("ventas_cliente_mes", "Ventas por cliente y mes", "Base facturada de cada cliente, mes a mes, este año frente al anterior.", "este_anio",
            new ConsultaAnalisis("ventas", ["cliente"], "mes", ["base"], Comparar: "anio_anterior"), "barras"),
        new("ventas_evolucion", "Evolución de ventas y margen", "Base, margen y margen % por mes, comparado con el año anterior.", "ultimos_12_meses",
            new ConsultaAnalisis("ventas", ["mes"], null, ["base", "margen", "margen_pct"], Comparar: "anio_anterior"), "lineas"),
        new("margen_familia", "Margen por familia y artículo", "Qué familias y artículos dejan más margen (y cuáles pierden).", "este_anio",
            new ConsultaAnalisis("ventas", ["familia", "articulo"], null, ["cantidad", "base", "coste", "margen", "margen_pct"], OrdenarPor: "margen")),
        new("top_articulos_kilos", "Top 20 artículos por kilos", "Los artículos más vendidos en kilos, con su precio medio por kilo.", "este_anio",
            new ConsultaAnalisis("ventas", ["articulo"], null, ["kilos", "base", "precio_kilo"], OrdenarPor: "kilos", Limite: 20), "barras"),
        new("ventas_pais", "Ventas por país y cliente", "Mercados: base facturada por país y, dentro, por cliente.", "este_anio",
            new ConsultaAnalisis("ventas", ["pais", "cliente"], null, ["base", "kilos", "margen_pct"])),
        new("compras_cuenta", "Gastos por cuenta y proveedor", "A dónde va el dinero: bases por cuenta de gasto y proveedor.", "este_anio",
            new ConsultaAnalisis("compras", ["cuenta_gasto", "proveedor"], null, ["base", "cuota", "total"])),
        new("compras_mes", "Compras por proveedor y mes", "Base comprada a cada proveedor mes a mes.", "este_anio",
            new ConsultaAnalisis("compras", ["proveedor"], "mes", ["base"]), "barras"),
        new("deuda_antiguedad", "Deuda de clientes por antigüedad", "Lo pendiente de cobrar por tramos de antigüedad (aging) y cliente.", "todo",
            new ConsultaAnalisis("deuda", ["cliente"], "antiguedad", ["pendiente"], [new FiltroAnalisis("situacion", "no_en", ["Cobrada"])], FiltrosMedida: [new FiltroMedida("pendiente", "mayor", 0m)])),
        new("tesoreria_banco_mes", "Cobros y pagos por banco y mes", "Entradas y salidas de dinero de cada cuenta, mes a mes.", "este_anio",
            new ConsultaAnalisis("tesoreria", ["banco", "mes"], null, ["cobros", "pagos", "neto"]), "barras"),
        new("pedidos_pendientes", "Cartera de pedidos pendiente", "Lo que queda por servir y facturar, por cliente y artículo.", "todo",
            new ConsultaAnalisis("pedidos", ["cliente", "articulo"], null, ["pendiente_servir", "importe_pendiente", "servicio_pct"],
                FiltrosMedida: [new FiltroMedida("importe_pendiente", "mayor", 0m)])),
        new("contabilidad_grupos", "Saldos por grupo y cuenta", "Debe, haber y saldo por grupo del plan contable y cuenta.", "este_anio",
            new ConsultaAnalisis("contabilidad", ["grupo", "cuenta3"], null, ["debe", "haber", "saldo"])),
        new("gastos_ingresos_mes", "Gastos e ingresos por mes (grupos 6 y 7)", "Cuenta de resultados mensual a partir de los apuntes.", "este_anio",
            new ConsultaAnalisis("contabilidad", ["grupo"], "mes", ["saldo_acreedor"], [new FiltroAnalisis("subgrupo", "desde", ["6"]), new FiltroAnalisis("subgrupo", "hasta", ["79"])])),
        new("almacen_articulo", "Movimientos por almacén y artículo", "Entradas, salidas y variación de stock y valor.", "este_mes",
            new ConsultaAnalisis("almacen", ["almacen", "articulo"], null, ["entradas", "salidas", "neto", "valor"])),
        new("agro_agricultor_calibre", "Entradas de fruta por agricultor y calibre", "Kilos entregados por cada agricultor y su reparto por calibre.", "este_anio",
            new ConsultaAnalisis("agro", ["agricultor"], "calibre", ["kilos"])),
    ];
}
