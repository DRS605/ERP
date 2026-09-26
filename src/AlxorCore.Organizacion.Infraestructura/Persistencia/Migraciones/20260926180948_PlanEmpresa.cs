using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Organizacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class PlanEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "edicion",
                schema: "organizacion",
                table: "empresa",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "completa");

            migrationBuilder.AddColumn<string[]>(
                name: "modulos_adicionales",
                schema: "organizacion",
                table: "empresa",
                type: "text[]",
                nullable: false,
                defaultValueSql: "'{}'::text[]");

            // Garantía en la base de datos (no solo en el dominio): solo ediciones y módulos del catálogo.
            migrationBuilder.Sql("""
                ALTER TABLE organizacion.empresa ADD CONSTRAINT ck_empresa_edicion
                    CHECK (edicion IN ('start', 'gestion', 'finanzas', 'gestion_finanzas', 'completa'));
                ALTER TABLE organizacion.empresa ADD CONSTRAINT ck_empresa_modulos_adicionales
                    CHECK (modulos_adicionales <@ ARRAY['ventas', 'compras', 'inventario', 'produccion', 'personal', 'proyectos',
                        'contabilidad', 'inmovilizado', 'tesoreria_avanzada', 'divisas', 'aprobaciones', 'integraciones']::text[]);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE organizacion.empresa DROP CONSTRAINT ck_empresa_edicion; " +
                "ALTER TABLE organizacion.empresa DROP CONSTRAINT ck_empresa_modulos_adicionales;");

            migrationBuilder.DropColumn(
                name: "edicion",
                schema: "organizacion",
                table: "empresa");

            migrationBuilder.DropColumn(
                name: "modulos_adicionales",
                schema: "organizacion",
                table: "empresa");
        }
    }
}
