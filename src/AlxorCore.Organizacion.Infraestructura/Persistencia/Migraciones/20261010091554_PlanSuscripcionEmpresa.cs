using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Organizacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class PlanSuscripcionEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "plan",
                schema: "organizacion",
                table: "empresa",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Autonomo");

            // Las empresas ya existentes conservan todo el menú (plan Empresa). Las nuevas que se
            // creen a partir de ahora nacen en plan Autónomo (valor por defecto del dominio).
            migrationBuilder.Sql("UPDATE organizacion.empresa SET plan = 'Empresa';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "plan",
                schema: "organizacion",
                table: "empresa");
        }
    }
}
