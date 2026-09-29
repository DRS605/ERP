using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Organizacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class RolesPropios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "rol_codigo",
                schema: "organizacion",
                table: "membresia",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.CreateTable(
                name: "rol_empresa",
                schema: "organizacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    actualizado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    permisos = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rol_empresa", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_rol_empresa_nombre",
                schema: "organizacion",
                table: "rol_empresa",
                columns: new[] { "empresa_id", "nombre" },
                unique: true);

            // Un miembro tiene un rol fijo o uno propio de su misma empresa; un rol propio con miembros activos no se borra.
            migrationBuilder.Sql("""
                ALTER TABLE organizacion.rol_empresa
                    ADD CONSTRAINT ck_rol_empresa_valores CHECK (length(trim(nombre)) > 0 AND cardinality(permisos) > 0);
                ALTER TABLE organizacion.membresia
                    ADD CONSTRAINT ck_membresia_rol CHECK (rol_codigo IN ('propietario', 'usuario', 'solo_lectura') OR rol_codigo ~ '^rol_[0-9a-f]{32}$');

                CREATE OR REPLACE FUNCTION organizacion.membresia_rol_valido() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF NEW.rol_codigo LIKE 'rol\_%' AND NOT EXISTS (
                        SELECT 1 FROM organizacion.rol_empresa r WHERE 'rol_' || replace(r.id::text, '-', '') = NEW.rol_codigo AND r.empresa_id = NEW.empresa_id) THEN
                        PERFORM public.alxor_error('rol.desconocido', 'El rol del miembro no existe en su empresa.');
                    END IF;
                    RETURN NULL;
                END $f$;
                CREATE CONSTRAINT TRIGGER tg_membresia_rol_valido AFTER INSERT OR UPDATE ON organizacion.membresia
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION organizacion.membresia_rol_valido();

                CREATE OR REPLACE FUNCTION organizacion.rol_empresa_en_uso() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF (TG_OP = 'DELETE' OR NEW.empresa_id <> OLD.empresa_id) AND NOT public.alxor_borrando_empresa(OLD.empresa_id) AND EXISTS (
                        SELECT 1 FROM organizacion.membresia m WHERE m.rol_codigo = 'rol_' || replace(OLD.id::text, '-', '') AND m.estado = 'Activa') THEN
                        PERFORM public.alxor_error('rol.en_uso', 'Hay miembros activos con ese rol: cámbiales antes el rol.');
                    END IF;
                    RETURN CASE WHEN TG_OP = 'DELETE' THEN OLD ELSE NEW END;
                END $f$;
                CREATE TRIGGER tg_rol_empresa_en_uso BEFORE UPDATE OR DELETE ON organizacion.rol_empresa
                    FOR EACH ROW EXECUTE FUNCTION organizacion.rol_empresa_en_uso();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS tg_rol_empresa_en_uso ON organizacion.rol_empresa;
                DROP FUNCTION IF EXISTS organizacion.rol_empresa_en_uso();
                DROP TRIGGER IF EXISTS tg_membresia_rol_valido ON organizacion.membresia;
                DROP FUNCTION IF EXISTS organizacion.membresia_rol_valido();
                ALTER TABLE organizacion.membresia DROP CONSTRAINT IF EXISTS ck_membresia_rol;
                """);


            migrationBuilder.DropTable(
                name: "rol_empresa",
                schema: "organizacion");

            migrationBuilder.AlterColumn<string>(
                name: "rol_codigo",
                schema: "organizacion",
                table: "membresia",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40);
        }
    }
}
