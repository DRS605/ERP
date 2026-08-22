using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Organizacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ControlRiesgoEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "control_riesgo",
                schema: "organizacion",
                table: "empresa",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Aviso");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "control_riesgo",
                schema: "organizacion",
                table: "empresa");
        }
    }
}
