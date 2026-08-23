using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Identidad.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class DobleFactorUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codigos_recuperacion",
                schema: "identidad",
                table: "usuario",
                type: "character varying(600)",
                maxLength: 600,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "doble_factor_activo",
                schema: "identidad",
                table: "usuario",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "doble_factor_secreto",
                schema: "identidad",
                table: "usuario",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "codigos_recuperacion",
                schema: "identidad",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "doble_factor_activo",
                schema: "identidad",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "doble_factor_secreto",
                schema: "identidad",
                table: "usuario");
        }
    }
}
