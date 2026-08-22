using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Catalogo.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class UnidadesYEnvasesProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "factor_compra",
                schema: "catalogo",
                table: "producto",
                type: "numeric(14,4)",
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<decimal>(
                name: "factor_venta",
                schema: "catalogo",
                table: "producto",
                type: "numeric(14,4)",
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<string>(
                name: "unidad_compra",
                schema: "catalogo",
                table: "producto",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unidad_venta",
                schema: "catalogo",
                table: "producto",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "factor_compra",
                schema: "catalogo",
                table: "producto");

            migrationBuilder.DropColumn(
                name: "factor_venta",
                schema: "catalogo",
                table: "producto");

            migrationBuilder.DropColumn(
                name: "unidad_compra",
                schema: "catalogo",
                table: "producto");

            migrationBuilder.DropColumn(
                name: "unidad_venta",
                schema: "catalogo",
                table: "producto");
        }
    }
}
