using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Terceros.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class CentrosDir3Cliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "dir3_oficina_contable",
                schema: "terceros",
                table: "cliente",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dir3_organo_gestor",
                schema: "terceros",
                table: "cliente",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dir3_unidad_tramitadora",
                schema: "terceros",
                table: "cliente",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "es_administracion_publica",
                schema: "terceros",
                table: "cliente",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "dir3_oficina_contable",
                schema: "terceros",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "dir3_organo_gestor",
                schema: "terceros",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "dir3_unidad_tramitadora",
                schema: "terceros",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "es_administracion_publica",
                schema: "terceros",
                table: "cliente");
        }
    }
}
