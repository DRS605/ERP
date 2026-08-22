using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Organizacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class EmpresaMetodoValoracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "metodo_valoracion",
                schema: "organizacion",
                table: "empresa",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Estandar");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "metodo_valoracion",
                schema: "organizacion",
                table: "empresa");
        }
    }
}
