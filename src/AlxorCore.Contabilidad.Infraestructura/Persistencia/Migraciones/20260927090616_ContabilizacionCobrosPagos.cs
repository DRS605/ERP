using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Contabilidad.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ContabilizacionCobrosPagos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cuenta_tercero",
                schema: "contabilidad",
                table: "documento_pendiente",
                type: "character varying(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cuenta_tesoreria",
                schema: "contabilidad",
                table: "documento_pendiente",
                type: "character varying(12)",
                maxLength: 12,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cuenta_tercero",
                schema: "contabilidad",
                table: "documento_pendiente");

            migrationBuilder.DropColumn(
                name: "cuenta_tesoreria",
                schema: "contabilidad",
                table: "documento_pendiente");
        }
    }
}
