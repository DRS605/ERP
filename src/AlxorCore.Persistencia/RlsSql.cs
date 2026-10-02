namespace AlxorCore.Persistencia;

/// <summary>
/// Genera el SQL para activar Row-Level Security por empresa en una tabla. Se usa desde las
/// migraciones. La política compara <c>empresa_id</c> con el parámetro de sesión
/// <c>app.empresa_actual</c> (fijado por <see cref="InterceptorEmpresa"/>).
/// </summary>
/// <remarks>
/// La RLS solo se aplica si la aplicación se conecta con un rol <b>sin</b> privilegios de
/// superusuario y sin BYPASSRLS. Los tests de integración ya se conectan así (rol <c>alxor_app</c>),
/// y un test guardián comprueba que toda tabla de negocio la tiene forzada. En producción debe usarse
/// también un rol restringido (ver <c>docs/garantias-base-datos.md</c>).
/// </remarks>
public static class RlsSql
{
    /// <summary>SQL que activa (y fuerza) la RLS y crea la política de aislamiento por empresa.</summary>
    public static string Activar(string esquema, string tabla)
    {
        var cualificada = $"\"{esquema}\".\"{tabla}\"";
        var nombrePolitica = $"pol_empresa_{tabla}";

        return $"""
            ALTER TABLE {cualificada} ENABLE ROW LEVEL SECURITY;
            ALTER TABLE {cualificada} FORCE ROW LEVEL SECURITY;
            DROP POLICY IF EXISTS "{nombrePolitica}" ON {cualificada};
            CREATE POLICY "{nombrePolitica}" ON {cualificada}
                USING (empresa_id = NULLIF(current_setting('app.empresa_actual', true), '')::uuid)
                WITH CHECK (empresa_id = NULLIF(current_setting('app.empresa_actual', true), '')::uuid);
            """;
    }

    /// <summary>
    /// SQL que activa (y fuerza) la RLS de una tabla de <b>maestros compartidos</b>, aislando por
    /// <c>grupo_id</c> contra el parámetro de sesión <c>app.grupo_actual</c>.
    /// </summary>
    public static string ActivarPorGrupo(string esquema, string tabla)
    {
        var cualificada = $"\"{esquema}\".\"{tabla}\"";
        var nombrePolitica = $"pol_grupo_{tabla}";

        return $"""
            ALTER TABLE {cualificada} ENABLE ROW LEVEL SECURITY;
            ALTER TABLE {cualificada} FORCE ROW LEVEL SECURITY;
            DROP POLICY IF EXISTS "{nombrePolitica}" ON {cualificada};
            CREATE POLICY "{nombrePolitica}" ON {cualificada}
                USING (grupo_id = NULLIF(current_setting('app.grupo_actual', true), '')::uuid
                    OR grupo_id = NULLIF(current_setting('{ParametroFusionGrupo}', true), '')::uuid)
                WITH CHECK (grupo_id = NULLIF(current_setting('app.grupo_actual', true), '')::uuid
                    OR grupo_id = NULLIF(current_setting('{ParametroFusionGrupo}', true), '')::uuid);
            """;
    }

    /// <summary>
    /// Grupo de destino de una fusión de grupos. Solo lo fija, dentro de su transacción, la operación que une una empresa
    /// a otro grupo: deja pasar las filas del grupo activo a ese grupo. Sin él, nada sale del grupo ni se ve el de otro.
    /// </summary>
    public const string ParametroFusionGrupo = "app.grupo_fusion";

    /// <summary>
    /// Recrea la condición de escritura de las políticas por grupo que ya existen (las <c>pol_grupo_*</c>), con o sin el
    /// grupo de fusión (en la lectura, porque PostgreSQL también comprueba la fila movida, y en la escritura). Idempotente.
    /// </summary>
    public static string ActualizarPoliticasPorGrupo(bool conFusion)
    {
        var fusion = conFusion ? $" OR grupo_id = NULLIF(current_setting(''{ParametroFusionGrupo}'', true), '''')::uuid" : "";
        return $$"""
            DO $f$
            DECLARE p record;
            BEGIN
                FOR p IN SELECT schemaname, tablename, policyname FROM pg_policies WHERE policyname LIKE 'pol_grupo_%' LOOP
                    EXECUTE format('ALTER POLICY %I ON %I.%I USING (%s) WITH CHECK (%s)', p.policyname, p.schemaname, p.tablename,
                        'grupo_id = NULLIF(current_setting(''app.grupo_actual'', true), '''')::uuid{{fusion}}',
                        'grupo_id = NULLIF(current_setting(''app.grupo_actual'', true), '''')::uuid{{fusion}}');
                END LOOP;
            END $f$;
            """;
    }

    /// <summary>SQL que desactiva la RLS de la tabla (para revertir la migración).</summary>
    public static string Desactivar(string esquema, string tabla)
    {
        var cualificada = $"\"{esquema}\".\"{tabla}\"";
        var nombrePolitica = $"pol_empresa_{tabla}";

        return $"""
            DROP POLICY IF EXISTS "{nombrePolitica}" ON {cualificada};
            ALTER TABLE {cualificada} DISABLE ROW LEVEL SECURITY;
            """;
    }
}
