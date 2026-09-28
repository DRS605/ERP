using System.Security.Claims;
using AlxorCore.Api.Comun;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Consultas;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Organizacion.Aplicacion.CasosDeUso;
using AlxorCore.Organizacion.Aplicacion.Puertos;
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

        clientes.MapDelete("/{id:guid}", async (Guid id, BajasTerceros caso, CancellationToken ct) =>
                (await caso.EliminarClienteAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Elimina un cliente que no se ha usado (409 «cliente.en_uso» si ya tiene documentos: darlo de baja).")
            .RequierePermiso(Permisos.ClienteGestionar);

        clientes.MapPost("/{id:guid}/baja", async (Guid id, BajasTerceros caso, CancellationToken ct) =>
                (await caso.CambiarEstadoClienteAsync(id, false, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Da de baja un cliente: deja de ofrecerse en las altas nuevas y conserva su histórico.")
            .RequierePermiso(Permisos.ClienteGestionar);

        clientes.MapPost("/{id:guid}/alta", async (Guid id, BajasTerceros caso, CancellationToken ct) =>
                (await caso.CambiarEstadoClienteAsync(id, true, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Reactiva un cliente dado de baja.")
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

        proveedores.MapDelete("/{id:guid}", async (Guid id, BajasTerceros caso, CancellationToken ct) =>
                (await caso.EliminarProveedorAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Elimina un proveedor que no se ha usado (409 «proveedor.en_uso» si ya tiene documentos: darlo de baja).")
            .RequierePermiso(Permisos.GastoGestionar);

        proveedores.MapPost("/{id:guid}/baja", async (Guid id, BajasTerceros caso, CancellationToken ct) =>
                (await caso.CambiarEstadoProveedorAsync(id, false, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Da de baja un proveedor: deja de ofrecerse en las altas nuevas y conserva su histórico.")
            .RequierePermiso(Permisos.GastoGestionar);

        proveedores.MapPost("/{id:guid}/alta", async (Guid id, BajasTerceros caso, CancellationToken ct) =>
                (await caso.CambiarEstadoProveedorAsync(id, true, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Reactiva un proveedor dado de baja.")
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

    private static async Task<IResult> ListarProvAsync(IContextoEmpresa contexto, ClaimsPrincipal usuario, IConsultaVisibilidad visibilidad, ListarProveedores caso, CancellationToken ct, bool bajas = false)
    {
        if (contexto.GrupoId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var permitidas = await ActividadesPermitidasAsync(usuario, visibilidad, AreaVisibilidad.Compras, ct).ConfigureAwait(false);
        return Results.Ok(await caso.EjecutarAsync(contexto.GrupoId.Value, permitidas, bajas, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> BuscarProvAsync(IContextoEmpresa contexto, ClaimsPrincipal usuario, IConsultaVisibilidad visibilidad, BuscarProveedores caso,
        IConsultaProveedores consulta, CifrasMaestros cifras, string? texto, bool? incluirInactivos, int? pagina, int? tamanoPagina, int? ejercicio, CancellationToken ct)
    {
        if (contexto.GrupoId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var permitidas = await ActividadesPermitidasAsync(usuario, visibilidad, AreaVisibilidad.Compras, ct).ConfigureAwait(false);
        var filtro = new FiltroTerceros(texto, incluirInactivos ?? false, permitidas);
        var resultado = await caso.EjecutarAsync(contexto.GrupoId.Value, filtro, Paginacion.Normalizar(pagina, tamanoPagina), ct).ConfigureAwait(false);
        if (!usuario.HasClaim(AlxorCore.Nucleo.Seguridad.ClaimsAlxor.Permiso, Permisos.GastoLeer))
        {
            return Results.Ok(resultado);
        }

        var año = CifrasMaestros.Ejercicio(ejercicio);
        var todos = await consulta.IdsFiltradosAsync(contexto.GrupoId.Value, filtro, ct).ConfigureAwait(false);
        var c = await cifras.ProveedoresAsync(todos, resultado.Elementos.Select(e => e.Id), año, ct).ConfigureAwait(false);
        return Results.Ok(PaginaConCifras<ProveedorDto>.Desde(resultado, año, c));
    }

    private static async Task<IResult> BuscarClientesAsync(IContextoEmpresa contexto, ClaimsPrincipal usuario, IConsultaVisibilidad visibilidad, BuscarClientes caso,
        IConsultaClientes consulta, CifrasMaestros cifras, string? texto, bool? incluirInactivos, int? pagina, int? tamanoPagina, int? ejercicio, CancellationToken ct)
    {
        if (contexto.GrupoId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var permitidas = await ActividadesPermitidasAsync(usuario, visibilidad, AreaVisibilidad.Ventas, ct).ConfigureAwait(false);
        var filtro = new FiltroTerceros(texto, incluirInactivos ?? false, permitidas);
        var resultado = await caso.EjecutarAsync(contexto.GrupoId.Value, filtro, Paginacion.Normalizar(pagina, tamanoPagina), ct).ConfigureAwait(false);
        if (!usuario.HasClaim(AlxorCore.Nucleo.Seguridad.ClaimsAlxor.Permiso, Permisos.FacturaLeer))
        {
            return Results.Ok(resultado);
        }

        // Cifras de la empresa activa: las de la página y los totales de todo el filtro (no solo de la página).
        var año = CifrasMaestros.Ejercicio(ejercicio);
        var todos = await consulta.IdsFiltradosAsync(contexto.GrupoId.Value, filtro, ct).ConfigureAwait(false);
        var c = await cifras.ClientesAsync(todos, resultado.Elementos.Select(e => e.Id), año, ct).ConfigureAwait(false);
        return Results.Ok(PaginaConCifras<ClienteDto>.Desde(resultado, año, c));
    }

    private static async Task<IResult> ObtenerProvAsync(Guid id, ObtenerProveedor caso, CancellationToken ct) =>
        (await caso.EjecutarAsync(id, ct).ConfigureAwait(false)).AOk();

    /// <summary>La empresa enlazada a un tercero tiene que ser del mismo grupo.</summary>
    private static async Task<IResult?> EmpresaVinculadaInvalidaAsync(Guid? empresaId, IContextoEmpresa contexto, IConsultaEmpresas empresas, CancellationToken ct)
    {
        if (empresaId is not { } id || id == Guid.Empty)
        {
            return null;
        }

        var delGrupo = contexto.GrupoId is { } grupo ? await empresas.EmpresasDelGrupoAsync(grupo, ct).ConfigureAwait(false) : [];
        return delGrupo.Any(e => e.Id == id)
            ? null
            : ResultadosHttp.AProblema(Error.Validacion("tercero.empresa_vinculada", "La empresa enlazada no es de este grupo."));
    }

    private static async Task<IResult> CrearProvAsync(DatosProveedor datos, IContextoEmpresa contexto, IConsultaEmpresas empresas, CrearProveedor caso, CancellationToken ct)
    {
        if (contexto.GrupoId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        if (await EmpresaVinculadaInvalidaAsync(datos.EmpresaVinculadaId, contexto, empresas, ct).ConfigureAwait(false) is { } invalida)
        {
            return invalida;
        }

        var resultado = await caso.EjecutarAsync(contexto.GrupoId.Value, datos, ct).ConfigureAwait(false);
        return resultado.EsCorrecto ? resultado.ACreado($"/proveedores/{resultado.Valor.Id}") : ResultadosHttp.AProblema(resultado.Error);
    }

    private static async Task<IResult> ActualizarProvAsync(Guid id, DatosProveedor datos, IContextoEmpresa contexto, IConsultaEmpresas empresas, ActualizarProveedor caso, CancellationToken ct) =>
        await EmpresaVinculadaInvalidaAsync(datos.EmpresaVinculadaId, contexto, empresas, ct).ConfigureAwait(false)
            ?? (await caso.EjecutarAsync(id, datos, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> ListarAsync(IContextoEmpresa contexto, ClaimsPrincipal usuario, IConsultaVisibilidad visibilidad, ListarClientes caso, CancellationToken ct, bool bajas = false)
    {
        if (contexto.GrupoId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var permitidas = await ActividadesPermitidasAsync(usuario, visibilidad, AreaVisibilidad.Ventas, ct).ConfigureAwait(false);
        return Results.Ok(await caso.EjecutarAsync(contexto.GrupoId.Value, permitidas, bajas, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> ObtenerAsync(Guid id, ObtenerCliente caso, CancellationToken ct) =>
        (await caso.EjecutarAsync(id, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> CrearAsync(DatosCliente datos, IContextoEmpresa contexto, IConsultaEmpresas empresas, CrearCliente caso, CancellationToken ct)
    {
        if (contexto.GrupoId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        if (await EmpresaVinculadaInvalidaAsync(datos.EmpresaVinculadaId, contexto, empresas, ct).ConfigureAwait(false) is { } invalida)
        {
            return invalida;
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

    private static async Task<IResult> ActualizarAsync(Guid id, DatosCliente datos, IContextoEmpresa contexto, IConsultaEmpresas empresas, ActualizarCliente caso, CancellationToken ct) =>
        await EmpresaVinculadaInvalidaAsync(datos.EmpresaVinculadaId, contexto, empresas, ct).ConfigureAwait(false)
            ?? (await caso.EjecutarAsync(id, datos, ct).ConfigureAwait(false)).AOk();
}
