using System.Security.Claims;
using AlxorCore.Api.Comun;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Consultas;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Organizacion.Aplicacion.CasosDeUso;
using AlxorCore.Organizacion.Dominio;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Api.Endpoints;

/// <summary>Endpoints REST del módulo Terceros (clientes).</summary>
public static class EndpointsTerceros
{
    public static IEndpointRouteBuilder MapearTerceros(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        var clientes = rutas.MapGroup("/clientes").WithTags("Clientes");

        clientes.MapGet("", ListarAsync)
            .WithSummary("Lista los clientes de la empresa activa.")
            .RequireAuthorization();

        clientes.MapGet("/buscar", BuscarClientesAsync)
            .WithSummary("Busca clientes por texto (nombre, NIF, email) con paginación.")
            .RequireAuthorization();

        clientes.MapGet("/{id:guid}", ObtenerAsync)
            .WithSummary("Obtiene un cliente.")
            .RequireAuthorization();

        clientes.MapPost("", CrearAsync)
            .WithSummary("Crea un cliente.")
            .RequierePermiso(Permisos.ClienteGestionar);

        clientes.MapPut("/{id:guid}", ActualizarAsync)
            .WithSummary("Actualiza un cliente.")
            .RequierePermiso(Permisos.ClienteGestionar);

        clientes.MapPost("/importar", ImportarClientesAsync)
            .WithSummary("Importa clientes desde CSV (previsualiza o confirma).")
            .RequierePermiso(Permisos.ClienteGestionar);

        var proveedores = rutas.MapGroup("/proveedores").WithTags("Proveedores");

        proveedores.MapGet("", ListarProvAsync)
            .WithSummary("Lista los proveedores de la empresa activa.")
            .RequireAuthorization();

        proveedores.MapGet("/buscar", BuscarProvAsync)
            .WithSummary("Busca proveedores por texto (nombre, NIF, email) con paginación.")
            .RequireAuthorization();

        proveedores.MapGet("/{id:guid}", ObtenerProvAsync)
            .WithSummary("Obtiene un proveedor.")
            .RequireAuthorization();

        proveedores.MapPost("", CrearProvAsync)
            .WithSummary("Crea un proveedor.")
            .RequierePermiso(Permisos.GastoGestionar);

        proveedores.MapPut("/{id:guid}", ActualizarProvAsync)
            .WithSummary("Actualiza un proveedor.")
            .RequierePermiso(Permisos.GastoGestionar);

        return rutas;
    }

    // Resuelve las actividades que el usuario puede ver en un área (null = todas / sin restricción).
    private static async Task<IReadOnlyCollection<Guid>?> ActividadesPermitidasAsync(
        ClaimsPrincipal usuario, IConsultaVisibilidad visibilidad, AreaVisibilidad area, CancellationToken ct)
    {
        var usuarioId = usuario.ObtenerUsuarioId();
        return usuarioId is null ? null : await visibilidad.ActividadesPermitidasAsync(usuarioId.Value, area, ct).ConfigureAwait(false);
    }

    private static async Task<IResult> ListarProvAsync(IContextoEmpresa contexto, ClaimsPrincipal usuario, IConsultaVisibilidad visibilidad, ListarProveedores caso, CancellationToken ct)
    {
        if (contexto.GrupoId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var permitidas = await ActividadesPermitidasAsync(usuario, visibilidad, AreaVisibilidad.Compras, ct).ConfigureAwait(false);
        return Results.Ok(await caso.EjecutarAsync(contexto.GrupoId.Value, permitidas, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> BuscarProvAsync(IContextoEmpresa contexto, ClaimsPrincipal usuario, IConsultaVisibilidad visibilidad, BuscarProveedores caso,
        string? texto, bool? incluirInactivos, int? pagina, int? tamanoPagina, CancellationToken ct)
    {
        if (contexto.GrupoId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var permitidas = await ActividadesPermitidasAsync(usuario, visibilidad, AreaVisibilidad.Compras, ct).ConfigureAwait(false);
        var filtro = new FiltroTerceros(texto, incluirInactivos ?? false, permitidas);
        return Results.Ok(await caso.EjecutarAsync(contexto.GrupoId.Value, filtro, Paginacion.Normalizar(pagina, tamanoPagina), ct).ConfigureAwait(false));
    }

    private static async Task<IResult> BuscarClientesAsync(IContextoEmpresa contexto, ClaimsPrincipal usuario, IConsultaVisibilidad visibilidad, BuscarClientes caso,
        string? texto, bool? incluirInactivos, int? pagina, int? tamanoPagina, CancellationToken ct)
    {
        if (contexto.GrupoId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var permitidas = await ActividadesPermitidasAsync(usuario, visibilidad, AreaVisibilidad.Ventas, ct).ConfigureAwait(false);
        var filtro = new FiltroTerceros(texto, incluirInactivos ?? false, permitidas);
        return Results.Ok(await caso.EjecutarAsync(contexto.GrupoId.Value, filtro, Paginacion.Normalizar(pagina, tamanoPagina), ct).ConfigureAwait(false));
    }

    private static async Task<IResult> ObtenerProvAsync(Guid id, ObtenerProveedor caso, CancellationToken ct) =>
        (await caso.EjecutarAsync(id, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> CrearProvAsync(DatosProveedor datos, IContextoEmpresa contexto, CrearProveedor caso, CancellationToken ct)
    {
        if (contexto.GrupoId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var resultado = await caso.EjecutarAsync(contexto.GrupoId.Value, datos, ct).ConfigureAwait(false);
        return resultado.EsCorrecto ? resultado.ACreado($"/proveedores/{resultado.Valor.Id}") : ResultadosHttp.AProblema(resultado.Error);
    }

    private static async Task<IResult> ActualizarProvAsync(Guid id, DatosProveedor datos, ActualizarProveedor caso, CancellationToken ct) =>
        (await caso.EjecutarAsync(id, datos, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> ListarAsync(IContextoEmpresa contexto, ClaimsPrincipal usuario, IConsultaVisibilidad visibilidad, ListarClientes caso, CancellationToken ct)
    {
        if (contexto.GrupoId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var permitidas = await ActividadesPermitidasAsync(usuario, visibilidad, AreaVisibilidad.Ventas, ct).ConfigureAwait(false);
        return Results.Ok(await caso.EjecutarAsync(contexto.GrupoId.Value, permitidas, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> ObtenerAsync(Guid id, ObtenerCliente caso, CancellationToken ct) =>
        (await caso.EjecutarAsync(id, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> CrearAsync(DatosCliente datos, IContextoEmpresa contexto, CrearCliente caso, CancellationToken ct)
    {
        if (contexto.GrupoId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var resultado = await caso.EjecutarAsync(contexto.GrupoId.Value, datos, ct).ConfigureAwait(false);
        return resultado.EsCorrecto ? resultado.ACreado($"/clientes/{resultado.Valor.Id}") : ResultadosHttp.AProblema(resultado.Error);
    }

    private static async Task<IResult> ImportarClientesAsync(ImportarCsvPeticion peticion, IContextoEmpresa contexto, ImportarClientes caso, CancellationToken ct)
    {
        if (contexto.GrupoId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var filas = new List<FilaImportacionCliente>();
        foreach (var fila in LectorCsv.Parsear(peticion.Contenido ?? string.Empty))
        {
            var datos = new DatosCliente(
                Nombre: fila.Campo("nombre", "razon social", "cliente") ?? string.Empty,
                NifFiscal: fila.Campo("nif", "cif", "dni", "nif fiscal"),
                Email: fila.Campo("email", "correo", "e-mail"),
                Calle: fila.Campo("direccion", "calle"),
                CodigoPostal: fila.Campo("cp", "codigo postal"),
                Poblacion: fila.Campo("poblacion", "ciudad", "localidad"),
                Provincia: fila.Campo("provincia"),
                PorcentajeIrpfDefecto: ImportacionCsv.Numero(fila.Campo("irpf")));
            filas.Add(new FilaImportacionCliente(fila.Numero, datos));
        }

        var resultado = await caso.EjecutarAsync(contexto.GrupoId.Value, filas, peticion.Previsualizar, ct).ConfigureAwait(false);
        return Results.Ok(resultado);
    }

    private static async Task<IResult> ActualizarAsync(Guid id, DatosCliente datos, ActualizarCliente caso, CancellationToken ct) =>
        (await caso.EjecutarAsync(id, datos, ct).ConfigureAwait(false)).AOk();
}
