using System.Security.Claims;
using AlxorCore.Api.Comun;
using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Catalogo.Dominio;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Consultas;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Organizacion.Aplicacion.CasosDeUso;
using AlxorCore.Organizacion.Dominio;

namespace AlxorCore.Api.Endpoints;

/// <summary>Endpoints REST del módulo Catálogo (productos e impuestos).</summary>
public static class EndpointsCatalogo
{
    public static IEndpointRouteBuilder MapearCatalogo(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        var productos = rutas.MapGroup("/productos").WithTags("Productos");

        productos.MapGet("", ListarAsync)
            .WithSummary("Lista los productos de la empresa activa.")
            .RequireAuthorization();

        productos.MapGet("/buscar", BuscarAsync)
            .WithSummary("Busca productos con filtros (texto, familia, incluir inactivos) y paginación.")
            .RequireAuthorization();

        productos.MapGet("/{id:guid}", ObtenerAsync)
            .WithSummary("Obtiene un producto.")
            .RequireAuthorization();

        productos.MapGet("/{id:guid}/precios", HistoricoAsync)
            .WithSummary("Histórico de precios (compra y venta) del producto.")
            .RequireAuthorization();

        productos.MapPost("", CrearAsync)
            .WithSummary("Crea un producto.")
            .RequierePermiso(Permisos.ProductoGestionar);

        productos.MapPut("/{id:guid}", ActualizarAsync)
            .WithSummary("Actualiza un producto.")
            .RequierePermiso(Permisos.ProductoGestionar);

        productos.MapPost("/importar", ImportarAsync)
            .WithSummary("Importa productos desde CSV (previsualiza o confirma).")
            .RequierePermiso(Permisos.ProductoGestionar);

        productos.MapGet("/{id:guid}/stock", MovimientosStockAsync)
            .WithSummary("Histórico de movimientos de stock del producto.")
            .RequireAuthorization();

        productos.MapPost("/{id:guid}/stock", RegistrarStockAsync)
            .WithSummary("Registra un movimiento de stock (entrada, salida o ajuste).")
            .RequierePermiso(Permisos.ProductoGestionar);

        productos.MapGet("/{id:guid}/composicion", ComposicionAsync)
            .WithSummary("Lista de materiales (escandallo) de un artículo compuesto.")
            .RequireAuthorization();

        productos.MapPut("/{id:guid}/composicion", DefinirComposicionAsync)
            .WithSummary("Define (o vacía) la lista de materiales de un artículo compuesto.")
            .RequierePermiso(Permisos.ProductoGestionar);

        productos.MapGet("/{id:guid}/variantes", VariantesAsync)
            .WithSummary("Lista las variantes de un artículo plantilla.")
            .RequireAuthorization();

        productos.MapPost("/{id:guid}/variantes", CrearVarianteAsync)
            .WithSummary("Crea una variante (talla/color/…) de un artículo.")
            .RequierePermiso(Permisos.ProductoGestionar);

        rutas.MapGet("/impuestos", () => Results.Ok(ListarImpuestos.Ejecutar()))
            .WithTags("Impuestos")
            .WithSummary("Lista los tipos de IVA disponibles.")
            .RequireAuthorization();

        var familias = rutas.MapGroup("/familias").WithTags("Familias");

        familias.MapGet("", ListarFamiliasAsync)
            .WithSummary("Lista las familias de artículos (en plano, con su ruta completa).")
            .RequireAuthorization();

        familias.MapGet("/arbol", ArbolFamiliasAsync)
            .WithSummary("Devuelve el árbol de familias (raíces con sus subfamilias anidadas).")
            .RequireAuthorization();

        familias.MapPost("", CrearFamiliaAsync)
            .WithSummary("Crea una familia (o subfamilia si se indica padre).")
            .RequierePermiso(Permisos.ProductoGestionar);

        familias.MapPut("/{id:guid}", ActualizarFamiliaAsync)
            .WithSummary("Actualiza una familia (nombre, código, padre y estado).")
            .RequierePermiso(Permisos.ProductoGestionar);

        familias.MapDelete("/{id:guid}", EliminarFamiliaAsync)
            .WithSummary("Elimina una familia (solo si no tiene subfamilias ni artículos).")
            .RequierePermiso(Permisos.ProductoGestionar);

        return rutas;
    }

    /// <summary>Cuerpo de la petición para crear o actualizar una familia.</summary>
    public sealed record PeticionFamilia(string Nombre, string? Codigo = null, Guid? PadreId = null, bool Activo = true);

    private static async Task<IResult> ListarFamiliasAsync(IContextoEmpresa contexto, ListarFamilias caso, CancellationToken ct)
    {
        if (contexto.GrupoId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return Results.Ok(await caso.EjecutarAsync(contexto.GrupoId.Value, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> ArbolFamiliasAsync(IContextoEmpresa contexto, ListarArbolFamilias caso, CancellationToken ct)
    {
        if (contexto.GrupoId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return Results.Ok(await caso.EjecutarAsync(contexto.GrupoId.Value, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> CrearFamiliaAsync(PeticionFamilia peticion, IContextoEmpresa contexto, CrearFamilia caso, CancellationToken ct)
    {
        if (contexto.GrupoId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var r = await caso.EjecutarAsync(contexto.GrupoId.Value, new DatosFamilia(peticion.Nombre, peticion.Codigo, peticion.PadreId), ct).ConfigureAwait(false);
        return r.EsCorrecto ? r.ACreado($"/familias/{r.Valor.Id}") : ResultadosHttp.AProblema(r.Error);
    }

    private static async Task<IResult> ActualizarFamiliaAsync(Guid id, PeticionFamilia peticion, ActualizarFamilia caso, CancellationToken ct) =>
        (await caso.EjecutarAsync(id, new DatosFamilia(peticion.Nombre, peticion.Codigo, peticion.PadreId), peticion.Activo, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> EliminarFamiliaAsync(Guid id, EliminarFamilia caso, CancellationToken ct)
    {
        var r = await caso.EjecutarAsync(id, ct).ConfigureAwait(false);
        return r.EsCorrecto ? Results.NoContent() : ResultadosHttp.AProblema(r.Error);
    }

    // Resuelve las actividades que el usuario puede ver en Artículos (null = todas / sin restricción).
    private static async Task<IReadOnlyCollection<Guid>?> ActividadesArticulosAsync(ClaimsPrincipal usuario, IConsultaVisibilidad visibilidad, CancellationToken ct)
    {
        var usuarioId = usuario.ObtenerUsuarioId();
        return usuarioId is null ? null : await visibilidad.ActividadesPermitidasAsync(usuarioId.Value, AreaVisibilidad.Articulos, ct).ConfigureAwait(false);
    }

    private static async Task<IResult> ListarAsync(IContextoEmpresa contexto, ClaimsPrincipal usuario, IConsultaVisibilidad visibilidad, ListarProductos caso, CancellationToken ct)
    {
        if (contexto.GrupoId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var permitidas = await ActividadesArticulosAsync(usuario, visibilidad, ct).ConfigureAwait(false);
        return Results.Ok(await caso.EjecutarAsync(contexto.GrupoId.Value, permitidas, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> BuscarAsync(IContextoEmpresa contexto, ClaimsPrincipal usuario, IConsultaVisibilidad visibilidad, BuscarProductos caso,
        string? texto, Guid? familiaId, bool? incluirInactivos, int? pagina, int? tamanoPagina, CancellationToken ct)
    {
        if (contexto.GrupoId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var permitidas = await ActividadesArticulosAsync(usuario, visibilidad, ct).ConfigureAwait(false);
        var filtro = new FiltroProductos(texto, familiaId, incluirInactivos ?? false, permitidas);
        return Results.Ok(await caso.EjecutarAsync(contexto.GrupoId.Value, filtro, Paginacion.Normalizar(pagina, tamanoPagina), ct).ConfigureAwait(false));
    }

    private static async Task<IResult> ObtenerAsync(Guid id, ObtenerProducto caso, CancellationToken ct) =>
        (await caso.EjecutarAsync(id, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> HistoricoAsync(Guid id, ListarHistoricoPrecios caso, CancellationToken ct) =>
        Results.Ok(await caso.EjecutarAsync(id, ct).ConfigureAwait(false));

    private static async Task<IResult> MovimientosStockAsync(Guid id, ListarMovimientosStock caso, CancellationToken ct) =>
        Results.Ok(await caso.EjecutarAsync(id, ct).ConfigureAwait(false));

    private static async Task<IResult> RegistrarStockAsync(Guid id, DatosMovimientoStock datos, IContextoEmpresa contexto, RegistrarMovimientoStock caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return (await caso.EjecutarAsync(contexto.EmpresaId.Value, id, datos, ct).ConfigureAwait(false)).AOk();
    }

    private static async Task<IResult> CrearAsync(DatosProducto datos, IContextoEmpresa contexto, CrearProducto caso, CancellationToken ct)
    {
        if (contexto.GrupoId is null || contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var resultado = await caso.EjecutarAsync(contexto.GrupoId.Value, contexto.EmpresaId.Value, datos, ct).ConfigureAwait(false);
        return resultado.EsCorrecto ? resultado.ACreado($"/productos/{resultado.Valor.Id}") : ResultadosHttp.AProblema(resultado.Error);
    }

    private static async Task<IResult> ActualizarAsync(Guid id, DatosProducto datos, ActualizarProducto caso, CancellationToken ct) =>
        (await caso.EjecutarAsync(id, datos, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> ComposicionAsync(Guid id, ObtenerComposicion caso, CancellationToken ct) =>
        (await caso.EjecutarAsync(id, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> DefinirComposicionAsync(Guid id, DatosComposicion datos, DefinirComposicion caso, CancellationToken ct) =>
        (await caso.EjecutarAsync(id, datos, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> VariantesAsync(Guid id, ListarVariantes caso, CancellationToken ct) =>
        Results.Ok(await caso.EjecutarAsync(id, ct).ConfigureAwait(false));

    private static async Task<IResult> CrearVarianteAsync(Guid id, DatosVariante datos, CrearVariante caso, CancellationToken ct)
    {
        var r = await caso.EjecutarAsync(id, datos, ct).ConfigureAwait(false);
        return r.EsCorrecto ? r.ACreado($"/productos/{r.Valor.Id}") : ResultadosHttp.AProblema(r.Error);
    }

    private static async Task<IResult> ImportarAsync(ImportarCsvPeticion peticion, IContextoEmpresa contexto, ImportarProductos caso, CancellationToken ct)
    {
        if (contexto.GrupoId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var filas = new List<FilaImportacionProducto>();
        foreach (var fila in LectorCsv.Parsear(peticion.Contenido ?? string.Empty))
        {
            var tipoTexto = LectorCsv.Normalizar(fila.Campo("tipo") ?? string.Empty);
            var tipo = tipoTexto is "bien" or "producto" or "articulo" ? TipoProducto.Bien : TipoProducto.Servicio;
            var datos = new DatosProducto(
                Nombre: fila.Campo("nombre", "producto", "articulo", "descripcion") ?? string.Empty,
                PrecioUnitario: ImportacionCsv.Numero(fila.Campo("precio", "precio unitario", "importe", "pvp")),
                Referencia: fila.Campo("referencia", "codigo", "ean", "sku", "código"),
                Tipo: tipo,
                CodigoIva: ImportacionCsv.CodigoIva(fila.Campo("iva", "codigo iva", "tipo iva")),
                Unidad: fila.Campo("unidad"),
                PrecioCompra: ImportacionCsv.Numero(fila.Campo("precio compra", "coste", "compra", "precio de compra")));
            filas.Add(new FilaImportacionProducto(fila.Numero, datos));
        }

        var resultado = await caso.EjecutarAsync(contexto.GrupoId.Value, filas, peticion.Previsualizar, ct).ConfigureAwait(false);
        return Results.Ok(resultado);
    }
}
