using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Agro.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ReflejoInventarioAgro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "reflejar_envases_inventario",
                schema: "agro",
                table: "configuracion",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "reflejar_partidas_inventario",
                schema: "agro",
                table: "configuracion",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "reflejar_envases_inventario",
                schema: "agro",
                table: "configuracion");

            migrationBuilder.DropColumn(
                name: "reflejar_partidas_inventario",
                schema: "agro",
                table: "configuracion");
        }
    }
}
