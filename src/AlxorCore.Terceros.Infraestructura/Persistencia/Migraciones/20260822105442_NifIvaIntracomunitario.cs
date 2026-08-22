using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Terceros.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class NifIvaIntracomunitario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "nif_iva",
                schema: "terceros",
                table: "proveedor",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nif_iva",
                schema: "terceros",
                table: "cliente",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "nif_iva",
                schema: "terceros",
                table: "proveedor");

            migrationBuilder.DropColumn(
                name: "nif_iva",
                schema: "terceros",
                table: "cliente");
        }
    }
}
