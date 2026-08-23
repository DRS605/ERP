using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Organizacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class GrupoHolding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "grupo",
                schema: "organizacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grupo", x => x.id);
                });

            // Se añade nullable para poder rellenar las empresas existentes antes de hacerla obligatoria.
            migrationBuilder.AddColumn<Guid>(
                name: "grupo_id",
                schema: "organizacion",
                table: "empresa",
                type: "uuid",
                nullable: true);

            // Retrocompatibilidad: cada empresa existente pasa a su PROPIO grupo (mismo aislamiento que antes).
            // Para compartir maestros, después se pueden mover varias empresas al mismo grupo.
            migrationBuilder.Sql("""
                DO $$
                DECLARE r RECORD; gid uuid;
                BEGIN
                    FOR r IN SELECT id, razon_social FROM organizacion.empresa WHERE grupo_id IS NULL LOOP
                        gid := gen_random_uuid();
                        INSERT INTO organizacion.grupo (id, nombre, creado_en) VALUES (gid, r.razon_social, now());
                        UPDATE organizacion.empresa SET grupo_id = gid WHERE id = r.id;
                    END LOOP;
                END $$;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "grupo_id",
                schema: "organizacion",
                table: "empresa",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_empresa_grupo",
                schema: "organizacion",
                table: "empresa",
                column: "grupo_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "grupo",
                schema: "organizacion");

            migrationBuilder.DropIndex(
                name: "ix_empresa_grupo",
                schema: "organizacion",
                table: "empresa");

            migrationBuilder.DropColumn(
                name: "grupo_id",
                schema: "organizacion",
                table: "empresa");
        }
    }
}
