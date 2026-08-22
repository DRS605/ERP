using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Terceros.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class TerceroLimiteRiesgo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "limite_riesgo",
                schema: "terceros",
                table: "proveedor",
                type: "numeric(14,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "limite_riesgo",
                schema: "terceros",
                table: "cliente",
                type: "numeric(14,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "limite_riesgo",
                schema: "terceros",
                table: "proveedor");

            migrationBuilder.DropColumn(
                name: "limite_riesgo",
                schema: "terceros",
                table: "cliente");
        }
    }
}
