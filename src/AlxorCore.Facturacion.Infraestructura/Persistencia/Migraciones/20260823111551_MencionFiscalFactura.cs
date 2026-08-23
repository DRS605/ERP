using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Facturacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class MencionFiscalFactura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "mencion_fiscal",
                schema: "facturacion",
                table: "factura",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "mencion_fiscal",
                schema: "facturacion",
                table: "factura");
        }
    }
}
