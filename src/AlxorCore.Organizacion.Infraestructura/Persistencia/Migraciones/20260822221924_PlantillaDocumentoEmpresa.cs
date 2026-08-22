using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Organizacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class PlantillaDocumentoEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "color_principal",
                schema: "organizacion",
                table: "empresa",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email_contacto",
                schema: "organizacion",
                table: "empresa",
                type: "character varying(254)",
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "logo_png",
                schema: "organizacion",
                table: "empresa",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "telefono",
                schema: "organizacion",
                table: "empresa",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "texto_pie",
                schema: "organizacion",
                table: "empresa",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "web",
                schema: "organizacion",
                table: "empresa",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "color_principal",
                schema: "organizacion",
                table: "empresa");

            migrationBuilder.DropColumn(
                name: "email_contacto",
                schema: "organizacion",
                table: "empresa");

            migrationBuilder.DropColumn(
                name: "logo_png",
                schema: "organizacion",
                table: "empresa");

            migrationBuilder.DropColumn(
                name: "telefono",
                schema: "organizacion",
                table: "empresa");

            migrationBuilder.DropColumn(
                name: "texto_pie",
                schema: "organizacion",
                table: "empresa");

            migrationBuilder.DropColumn(
                name: "web",
                schema: "organizacion",
                table: "empresa");
        }
    }
}
