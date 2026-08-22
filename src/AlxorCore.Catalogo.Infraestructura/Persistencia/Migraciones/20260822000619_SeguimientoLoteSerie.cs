using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Catalogo.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class SeguimientoLoteSerie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "seguimiento",
                schema: "catalogo",
                table: "producto",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Ninguno");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "seguimiento",
                schema: "catalogo",
                table: "producto");
        }
    }
}
