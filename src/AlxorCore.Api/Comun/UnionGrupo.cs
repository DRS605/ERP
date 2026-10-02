using System.Data.Common;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Organizacion.Aplicacion.CasosDeUso;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Organizacion.Infraestructura.Persistencia;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace AlxorCore.Api.Comun;

/// <summary>Empresa del usuario que se puede unir al grupo activo.</summary>
public sealed record CandidataUnionDto(Guid Id, string Nif, string RazonSocial, bool Unible, string? Motivo);

/// <summary>Un tipo de maestro en la unión: cuántos pasan al grupo y cuáles coinciden con uno que el grupo ya tiene.</summary>
public sealed record MaestroUnionDto(string Clave, string Nombre, int Incorporados, IReadOnlyList<string> Duplicados);

/// <summary>Vista previa (o resultado) de unir una empresa al grupo activo.</summary>
public sealed record ResultadoUnionDto(Guid EmpresaId, string Empresa, string Grupo, bool Ejecutada, IReadOnlyList<MaestroUnionDto> Maestros);

/// <summary>Qué empresa unir al grupo activo.</summary>
public sealed record PeticionUnion(Guid EmpresaId);

/// <summary>
/// Une una empresa que ya existe (sola en su grupo) al grupo de la empresa activa. Sus maestros compartidos —clientes,
/// proveedores, artículos, familias, tarifas, conceptos, analítica y actividades— pasan al grupo con el mismo
/// identificador, así que todos sus documentos siguen apuntando a ellos. Los que coinciden con uno del grupo (mismo NIF,
/// referencia o código) se dan de baja: los documentos antiguos los conservan y los nuevos usan la ficha del grupo; los
/// que tienen un código único en el grupo cambian de código para no chocar.
/// La base de datos solo deja sacar filas de un grupo si la transacción declara el grupo de destino
/// (<see cref="RlsSql.ParametroFusionGrupo"/>); esta es la única operación que lo hace.
/// </summary>
public sealed class UnionGrupo
{
    /// <summary>Maestros que se comparan: tabla, nombre para el usuario, columna de activo, cómo se compara y si el código es único.</summary>
    private sealed record Maestro(string Clave, string Nombre, string Tabla, string Activo, string Comparar, string Etiqueta, bool CodigoUnico);

    private static readonly Maestro[] Maestros =
    [
        new("clientes", "Clientes", "terceros.cliente", "activo", "upper(regexp_replace(coalesce(nif_fiscal, ''), '[^A-Za-z0-9]', '', 'g'))", "nombre", false),
        new("proveedores", "Proveedores", "terceros.proveedor", "activo", "upper(regexp_replace(coalesce(nif_fiscal, ''), '[^A-Za-z0-9]', '', 'g'))", "nombre", false),
        new("articulos", "Artículos", "catalogo.producto", "activo", "upper(trim(coalesce(referencia, '')))", "coalesce(referencia || ' · ', '') || nombre", false),
        new("familias", "Familias", "catalogo.familia", "activo", "upper(trim(coalesce(nullif(codigo, ''), nombre)))", "nombre", false),
        new("tarifas", "Tarifas", "catalogo.tarifa", "activa", "upper(codigo)", "codigo || ' · ' || nombre", true),
        new("conceptos", "Conceptos de línea", "catalogo.concepto_linea", "activo", "upper(codigo)", "codigo || ' · ' || nombre", true),
        new("centros", "Centros analíticos", "contabilidad.centro_analitico", "activo", "upper(codigo)", "codigo || ' · ' || nombre", true),
        new("partidas", "Partidas analíticas", "contabilidad.partida_analitica", "activo", "upper(codigo)", "codigo || ' · ' || nombre", true),
        new("claves", "Claves de reparto", "contabilidad.clave_reparto", "activa", "upper(codigo)", "codigo || ' · ' || nombre", true),
        new("actividades", "Actividades de negocio", "organizacion.actividad_negocio", "activa", "upper(trim(nombre))", "nombre", false),
    ];

    private const int LargoCodigo = 20;

    private readonly IServiceScopeFactory _ambitos;

    public UnionGrupo(IServiceScopeFactory ambitos) => _ambitos = ambitos;

    private async Task<AsyncServiceScope> AmbitoAsync(Guid empresaId, CancellationToken ct)
    {
        var ambito = _ambitos.CreateAsyncScope();
        var contexto = ambito.ServiceProvider.GetRequiredService<IContextoEmpresaMutable>();
        contexto.Fijar(empresaId);
        if (await ambito.ServiceProvider.GetRequiredService<IRepositorioEmpresas>().ObtenerGrupoIdAsync(empresaId, ct).ConfigureAwait(false) is { } grupo)
        {
            contexto.FijarGrupo(grupo);
        }

        return ambito;
    }

    private static T S<T>(AsyncServiceScope a) where T : notnull => a.ServiceProvider.GetRequiredService<T>();

    /// <summary>Si el usuario tiene el permiso en la empresa (con su rol, propio o del sistema).</summary>
    public async Task<bool> TienePermisoAsync(Guid usuarioId, Guid empresaId, string permiso, CancellationToken ct = default)
    {
        await using var a = await AmbitoAsync(empresaId, ct).ConfigureAwait(false);
        var membresia = await S<IRepositorioMembresias>(a).ObtenerAsync(usuarioId, empresaId, ct).ConfigureAwait(false);
        if (membresia is null || !membresia.EstaActiva)
        {
            return false;
        }

        var rol = await RolesEmpresa.ResolverAsync(S<IRepositorioRolesEmpresa>(a), empresaId, membresia.RolCodigo, ct).ConfigureAwait(false);
        return rol.EsCorrecto && rol.Valor.PermisosConcedidos.Contains(permiso);
    }

    /// <summary>Si el usuario puede gestionar alguna empresa del grupo (para crear una empresa nueva dentro de él).</summary>
    public async Task<bool> PuedeGestionarGrupoAsync(Guid usuarioId, Guid grupoId, CancellationToken ct = default)
    {
        IReadOnlyList<Guid> empresas;
        await using (var a = _ambitos.CreateAsyncScope())
        {
            empresas = (await S<IConsultaEmpresas>(a).EmpresasDelGrupoAsync(grupoId, ct).ConfigureAwait(false)).Select(e => e.Id).ToList();
        }

        foreach (var e in empresas)
        {
            if (await TienePermisoAsync(usuarioId, e, Permisos.EmpresaAjustes, ct).ConfigureAwait(false))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Empresas del usuario de otros grupos, y si se pueden unir al grupo activo.</summary>
    public async Task<IReadOnlyList<CandidataUnionDto>> CandidatasAsync(Guid destinoId, Guid usuarioId, CancellationToken ct = default)
    {
        await using var a = await AmbitoAsync(destinoId, ct).ConfigureAwait(false);
        var grupo = S<IContextoEmpresa>(a).GrupoRequerido;
        var repo = S<IRepositorioEmpresas>(a);
        var consulta = S<IConsultaEmpresas>(a);
        var lista = new List<CandidataUnionDto>();
        foreach (var e in await S<IConsultasOrganizacion>(a).ListarEmpresasDeUsuarioAsync(usuarioId, ct).ConfigureAwait(false))
        {
            if (await repo.ObtenerGrupoIdAsync(e.Id, ct).ConfigureAwait(false) is not { } suyo || suyo == grupo)
            {
                continue;
            }

            var companeras = (await consulta.EmpresasDelGrupoAsync(suyo, ct).ConfigureAwait(false)).Where(x => x.Id != e.Id).Select(x => x.RazonSocial).ToList();
            string? motivo = null;
            if (companeras.Count > 0)
            {
                motivo = $"Comparte grupo con {string.Join(", ", companeras)}: solo se puede unir una empresa que esté sola en su grupo.";
            }
            else if (!await TienePermisoAsync(usuarioId, e.Id, Permisos.EmpresaAjustes, ct).ConfigureAwait(false))
            {
                motivo = "No tienes permiso para cambiar los ajustes de esa empresa.";
            }

            lista.Add(new CandidataUnionDto(e.Id, e.Nif, e.RazonSocial, motivo is null, motivo));
        }

        return lista.OrderBy(c => c.RazonSocial, StringComparer.CurrentCulture).ToList();
    }

    /// <summary>Vista previa (<paramref name="ejecutar"/> = false) o unión de la empresa al grupo de la empresa activa.</summary>
    public async Task<Resultado<ResultadoUnionDto>> UnirAsync(Guid destinoId, Guid usuarioId, PeticionUnion peticion, bool ejecutar, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(peticion);
        var candidata = (await CandidatasAsync(destinoId, usuarioId, ct).ConfigureAwait(false)).FirstOrDefault(c => c.Id == peticion.EmpresaId);
        if (candidata is null)
        {
            return Resultado.Fallo<ResultadoUnionDto>(Error.NoEncontrado("union.empresa", "No tienes acceso a esa empresa o ya es de este grupo."));
        }

        if (!candidata.Unible)
        {
            return Resultado.Fallo<ResultadoUnionDto>(Error.Conflicto("union.no_unible", candidata.Motivo!));
        }

        if (!await TienePermisoAsync(usuarioId, destinoId, Permisos.EmpresaAjustes, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<ResultadoUnionDto>(Error.Prohibido("union.sin_permiso", "No tienes permiso para cambiar los ajustes de esta empresa."));
        }

        // Lo que ya tiene el grupo de destino, visto desde la empresa activa.
        Guid grupoDestino;
        string nombreGrupo;
        var delDestino = new Dictionary<string, (HashSet<string> Claves, HashSet<string> Codigos)>();
        await using (var d = await AmbitoAsync(destinoId, ct).ConfigureAwait(false))
        {
            grupoDestino = S<IContextoEmpresa>(d).GrupoRequerido;
            nombreGrupo = (await S<IConsultaEmpresas>(d).ObtenerAsync(destinoId, ct).ConfigureAwait(false))?.RazonSocial ?? "el grupo";
            var cx = await ConexionAsync(d, ct).ConfigureAwait(false);
            foreach (var m in Maestros)
            {
                var filas = await LeerAsync(cx, null, $"SELECT {m.Comparar}, {(m.CodigoUnico ? "upper(codigo)" : "''")}, {m.Activo} FROM {m.Tabla} WHERE grupo_id = '{grupoDestino:D}'", ct).ConfigureAwait(false);
                delDestino[m.Clave] = (filas.Where(f => f.Activo && f.Clave.Length > 0).Select(f => f.Clave).ToHashSet(StringComparer.Ordinal),
                    filas.Select(f => f.Codigo).ToHashSet(StringComparer.Ordinal));
            }
        }

        await using var o = await AmbitoAsync(candidata.Id, ct).ConfigureAwait(false);
        var grupoOrigen = S<IContextoEmpresa>(o).GrupoRequerido;
        var conexion = await ConexionAsync(o, ct).ConfigureAwait(false);
        await using var tx = await conexion.BeginTransactionAsync(ct).ConfigureAwait(false);
        await EjecutarAsync(conexion, tx, $"SELECT set_config('app.grupo_actual', '{grupoOrigen:D}', true), set_config('{RlsSql.ParametroFusionGrupo}', '{grupoDestino:D}', true)", ct)
            .ConfigureAwait(false);

        var resumen = new List<MaestroUnionDto>();
        foreach (var m in Maestros)
        {
            var (claves, codigos) = delDestino[m.Clave];
            var filas = await LeerAsync(conexion, tx, $"SELECT {m.Comparar}, {(m.CodigoUnico ? "upper(codigo)" : "''")}, {m.Activo}, id::text, {m.Etiqueta} FROM {m.Tabla} WHERE grupo_id = '{grupoOrigen:D}'", ct)
                .ConfigureAwait(false);
            var duplicados = new List<string>();
            foreach (var f in filas)
            {
                // Un código único que ya está en el grupo obliga a cambiarlo, aunque la ficha del grupo esté de baja.
                var choca = m.CodigoUnico && codigos.Contains(f.Codigo);
                if (!choca && !(f.Clave.Length > 0 && claves.Contains(f.Clave)))
                {
                    continue;
                }

                duplicados.Add(f.Etiqueta);
                if (!ejecutar)
                {
                    continue;
                }

                var cambio = $"UPDATE {m.Tabla} SET {m.Activo} = false";
                if (choca)
                {
                    var nuevo = CodigoLibre(f.Codigo, codigos);
                    codigos.Add(nuevo);
                    cambio += $", codigo = '{nuevo.Replace("'", "''", StringComparison.Ordinal)}'";
                }

                await EjecutarAsync(conexion, tx, $"{cambio} WHERE id = '{f.Id}' AND grupo_id = '{grupoOrigen:D}'", ct).ConfigureAwait(false);
            }

            resumen.Add(new MaestroUnionDto(m.Clave, m.Nombre, filas.Count, duplicados));
        }

        if (!ejecutar)
        {
            await tx.RollbackAsync(ct).ConfigureAwait(false);
            return Resultado.Ok(new ResultadoUnionDto(candidata.Id, candidata.RazonSocial, nombreGrupo, false, resumen));
        }

        // La empresa primero (los clientes enlazados con ella comprueban que es de su grupo); luego todo lo del grupo.
        await EjecutarAsync(conexion, tx, $"UPDATE organizacion.empresa SET grupo_id = '{grupoDestino:D}' WHERE id = '{candidata.Id:D}'", ct).ConfigureAwait(false);
        var tablas = await LeerTablasPorGrupoAsync(conexion, tx, ct).ConfigureAwait(false);
        foreach (var t in tablas)
        {
            await EjecutarAsync(conexion, tx, $"UPDATE {t} SET grupo_id = '{grupoDestino:D}' WHERE grupo_id = '{grupoOrigen:D}'", ct).ConfigureAwait(false);
        }

        await EjecutarAsync(conexion, tx, $"DELETE FROM organizacion.grupo WHERE id = '{grupoOrigen:D}'", ct).ConfigureAwait(false);
        await tx.CommitAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new ResultadoUnionDto(candidata.Id, candidata.RazonSocial, nombreGrupo, true, resumen));
    }

    /// <summary>Código libre en el grupo: el original con un sufijo «-2», «-3»… sin pasar del largo de la columna.</summary>
    private static string CodigoLibre(string codigo, HashSet<string> usados)
    {
        for (var n = 2; ; n++)
        {
            var sufijo = "-" + n.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var nuevo = (codigo.Length + sufijo.Length > LargoCodigo ? codigo[..(LargoCodigo - sufijo.Length)] : codigo) + sufijo;
            if (!usados.Contains(nuevo.ToUpperInvariant()))
            {
                return nuevo;
            }
        }
    }

    private static async Task<DbConnection> ConexionAsync(AsyncServiceScope ambito, CancellationToken ct)
    {
        var db = S<OrganizacionDbContext>(ambito).Database;
        await db.OpenConnectionAsync(ct).ConfigureAwait(false);
        return db.GetDbConnection();
    }

    private sealed record Fila(string Clave, string Codigo, bool Activo, string Id, string Etiqueta);

    private static async Task<List<Fila>> LeerAsync(DbConnection cx, DbTransaction? tx, string sql, CancellationToken ct)
    {
        await using var cmd = cx.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        await using var r = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        var filas = new List<Fila>();
        while (await r.ReadAsync(ct).ConfigureAwait(false))
        {
            filas.Add(new Fila(r.GetString(0), r.GetString(1), r.GetBoolean(2), r.FieldCount > 3 ? r.GetString(3) : "", r.FieldCount > 4 ? r.GetString(4) : ""));
        }

        return filas;
    }

    /// <summary>Tablas con política por grupo (las de los maestros compartidos), sea cual sea su módulo.</summary>
    private static async Task<List<string>> LeerTablasPorGrupoAsync(DbConnection cx, DbTransaction tx, CancellationToken ct)
    {
        await using var cmd = cx.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = "SELECT DISTINCT quote_ident(schemaname) || '.' || quote_ident(tablename) FROM pg_policies WHERE policyname LIKE 'pol_grupo_%' ORDER BY 1";
        await using var r = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        var tablas = new List<string>();
        while (await r.ReadAsync(ct).ConfigureAwait(false))
        {
            tablas.Add(r.GetString(0));
        }

        return tablas;
    }

    private static async Task EjecutarAsync(DbConnection cx, DbTransaction tx, string sql, CancellationToken ct)
    {
        await using var cmd = cx.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }
}
