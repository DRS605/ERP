using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Catalogo.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class CodigoIgicProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codigo_igic",
                schema: "catalogo",
                table: "producto",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "codigo_igic",
                schema: "catalogo",
                table: "producto");
        }
    }
}
