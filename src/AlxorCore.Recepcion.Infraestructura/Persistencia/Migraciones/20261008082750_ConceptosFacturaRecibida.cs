using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Recepcion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ConceptosFacturaRecibida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "conceptos",
                schema: "recepcion",
                table: "factura_recibida",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "conceptos",
                schema: "recepcion",
                table: "factura_recibida");
        }
    }
}
