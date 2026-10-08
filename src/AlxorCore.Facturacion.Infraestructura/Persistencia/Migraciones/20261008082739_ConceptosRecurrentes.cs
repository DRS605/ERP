using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Facturacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ConceptosRecurrentes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "conceptos",
                schema: "facturacion",
                table: "linea_recurrente",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "conceptos_documento",
                schema: "facturacion",
                table: "factura_recurrente",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "conceptos",
                schema: "facturacion",
                table: "linea_recurrente");

            migrationBuilder.DropColumn(
                name: "conceptos_documento",
                schema: "facturacion",
                table: "factura_recurrente");
        }
    }
}
