namespace AlxorCore.Persistencia;

/// <summary>
/// SQL de las <b>garantías de la base de datos</b>: reglas que no dependen de que la aplicación
/// se comporte bien, porque las impone PostgreSQL. Se usa desde las migraciones de cada módulo.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>RLS en las tablas hijas (líneas, apuntes…): solo se ve la fila si se ve su cabecera.</item>
/// <item>Tablas de <b>solo inserción</b> (movimientos, apuntes, auditoría…): ni UPDATE ni DELETE.</item>
/// <item>Documentos cerrados (facturas emitidas, asientos): las líneas solo se insertan en la misma
/// transacción que su cabecera (columna <c>tx_alta</c>), después ya no se pueden añadir.</item>
/// <item>La única excepción al borrado es la baja de una empresa, que lo declara de forma explícita
/// en su transacción (<see cref="ParametroBorradoEmpresa"/>).</item>
/// </list>
/// Las violaciones se lanzan con el SQLSTATE <see cref="CodigoError"/>, el mensaje para el usuario y
/// el código del error en el HINT; la API las devuelve como 409.
/// </remarks>
public static class GarantiasSql
{
    /// <summary>SQLSTATE propio de las garantías (clase «AX», libre en PostgreSQL).</summary>
    public const string CodigoError = "AX001";

    /// <summary>Parámetro de sesión (local a la transacción) con el que la baja de una empresa autoriza sus borrados.</summary>
    public const string ParametroBorradoEmpresa = "app.borrado_empresa";

    /// <summary>Funciones comunes a todas las garantías. Idempotente: cada módulo que las usa las (re)crea.</summary>
    public static string FuncionesComunes => $$"""
        CREATE OR REPLACE FUNCTION public.alxor_error(codigo text, mensaje text) RETURNS void
        LANGUAGE plpgsql AS $f$
        BEGIN
            RAISE EXCEPTION USING ERRCODE = '{{CodigoError}}', MESSAGE = mensaje, HINT = codigo;
        END $f$;

        CREATE OR REPLACE FUNCTION public.alxor_borrando_empresa(empresa uuid) RETURNS boolean
        LANGUAGE sql STABLE AS $f$
            SELECT coalesce(current_setting('{{ParametroBorradoEmpresa}}', true), '') <> ''
               AND (empresa IS NULL OR current_setting('{{ParametroBorradoEmpresa}}', true) = empresa::text)
        $f$;

        -- Tabla de solo inserción. TG_ARGV[0] = código de error, TG_ARGV[1] = mensaje.
        CREATE OR REPLACE FUNCTION public.alxor_solo_insercion() RETURNS trigger
        LANGUAGE plpgsql AS $f$
        BEGIN
            IF TG_OP = 'DELETE' AND public.alxor_borrando_empresa((to_jsonb(OLD) ->> 'empresa_id')::uuid) THEN
                RETURN OLD;
            END IF;
            PERFORM public.alxor_error(TG_ARGV[0], TG_ARGV[1]);
            RETURN NULL;
        END $f$;

        -- Marca la transacción en la que se da de alta la cabecera.
        CREATE OR REPLACE FUNCTION public.alxor_marcar_alta() RETURNS trigger
        LANGUAGE plpgsql AS $f$
        BEGIN
            NEW.tx_alta := pg_current_xact_id();
            RETURN NEW;
        END $f$;

        -- Hija de un documento cerrado: solo se inserta en la transacción del alta de su cabecera.
        -- TG_ARGV[0] = tabla padre cualificada, [1] = columna que la referencia, [2] = código, [3] = mensaje.
        CREATE OR REPLACE FUNCTION public.alxor_hija_de_alta() RETURNS trigger
        LANGUAGE plpgsql AS $f$
        DECLARE
            misma boolean;
        BEGIN
            EXECUTE format('SELECT tx_alta = pg_current_xact_id() FROM %s WHERE id = $1', TG_ARGV[0])
                INTO misma USING (to_jsonb(NEW) ->> TG_ARGV[1])::uuid;
            IF misma IS NOT TRUE THEN
                PERFORM public.alxor_error(TG_ARGV[2], TG_ARGV[3]);
            END IF;
            RETURN NEW;
        END $f$;
        """;

    /// <summary>
    /// RLS de una tabla hija sin <c>empresa_id</c>/<c>grupo_id</c>: una fila es visible (y se puede
    /// escribir) solo si su cabecera lo es, de modo que hereda el aislamiento de la cabecera.
    /// </summary>
    public static string RlsPorPadre(string esquema, string tabla, string columna, string esquemaPadre, string tablaPadre)
    {
        var cualificada = $"\"{esquema}\".\"{tabla}\"";
        var condicion = $"EXISTS (SELECT 1 FROM \"{esquemaPadre}\".\"{tablaPadre}\" p WHERE p.id = {cualificada}.\"{columna}\")";
        return $"""
            ALTER TABLE {cualificada} ENABLE ROW LEVEL SECURITY;
            ALTER TABLE {cualificada} FORCE ROW LEVEL SECURITY;
            DROP POLICY IF EXISTS "pol_padre_{tabla}" ON {cualificada};
            CREATE POLICY "pol_padre_{tabla}" ON {cualificada} USING ({condicion}) WITH CHECK ({condicion});
            """;
    }

    /// <summary>Revierte <see cref="RlsPorPadre"/>.</summary>
    public static string QuitarRlsPorPadre(string esquema, string tabla) => $"""
        DROP POLICY IF EXISTS "pol_padre_{tabla}" ON "{esquema}"."{tabla}";
        ALTER TABLE "{esquema}"."{tabla}" NO FORCE ROW LEVEL SECURITY;
        ALTER TABLE "{esquema}"."{tabla}" DISABLE ROW LEVEL SECURITY;
        """;

    /// <summary>Hace la tabla de solo inserción (ni UPDATE ni DELETE, salvo la baja de la empresa).</summary>
    public static string SoloInsercion(string esquema, string tabla, string codigo, string mensaje) => $"""
        DROP TRIGGER IF EXISTS tg_{tabla}_solo_insercion ON "{esquema}"."{tabla}";
        CREATE TRIGGER tg_{tabla}_solo_insercion BEFORE UPDATE OR DELETE ON "{esquema}"."{tabla}"
            FOR EACH ROW EXECUTE FUNCTION public.alxor_solo_insercion('{codigo}', '{Literal(mensaje)}');
        """;

    /// <summary>Añade la columna <c>tx_alta</c> (transacción del alta) a una cabecera y la rellena al insertar.</summary>
    public static string MarcarAlta(string esquema, string tabla) => $"""
        ALTER TABLE "{esquema}"."{tabla}" ADD COLUMN IF NOT EXISTS tx_alta xid8;
        DROP TRIGGER IF EXISTS tg_{tabla}_alta ON "{esquema}"."{tabla}";
        CREATE TRIGGER tg_{tabla}_alta BEFORE INSERT ON "{esquema}"."{tabla}"
            FOR EACH ROW EXECUTE FUNCTION public.alxor_marcar_alta();
        """;

    /// <summary>Las filas de la hija solo se insertan en la misma transacción que su cabecera.</summary>
    public static string HijaDeAlta(string esquema, string tabla, string columna, string esquemaPadre, string tablaPadre, string codigo, string mensaje) => $"""
        DROP TRIGGER IF EXISTS tg_{tabla}_hija ON "{esquema}"."{tabla}";
        CREATE TRIGGER tg_{tabla}_hija BEFORE INSERT ON "{esquema}"."{tabla}"
            FOR EACH ROW EXECUTE FUNCTION public.alxor_hija_de_alta('"{esquemaPadre}"."{tablaPadre}"', '{columna}', '{codigo}', '{Literal(mensaje)}');
        """;

    /// <summary>Elimina un trigger (para las migraciones de vuelta atrás).</summary>
    public static string QuitarTrigger(string esquema, string tabla, string trigger) =>
        $"DROP TRIGGER IF EXISTS {trigger} ON \"{esquema}\".\"{tabla}\";";

    private static string Literal(string texto) => texto.Replace("'", "''", StringComparison.Ordinal);
}
