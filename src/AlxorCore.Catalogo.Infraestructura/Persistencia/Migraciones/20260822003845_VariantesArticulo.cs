using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Catalogo.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class VariantesArticulo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "es_plantilla",
                schema: "catalogo",
                table: "producto",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "producto_padre_id",
                schema: "catalogo",
                table: "producto",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "atributo_variante",
                schema: "catalogo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    valor = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_atributo_variante", x => x.id);
                    table.ForeignKey(
                        name: "FK_atributo_variante_producto_producto_id",
                        column: x => x.producto_id,
                        principalSchema: "catalogo",
                        principalTable: "producto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_atributo_variante_producto_id",
                schema: "catalogo",
                table: "atributo_variante",
                column: "producto_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "atributo_variante",
                schema: "catalogo");

            migrationBuilder.DropColumn(
                name: "es_plantilla",
                schema: "catalogo",
                table: "producto");

            migrationBuilder.DropColumn(
                name: "producto_padre_id",
                schema: "catalogo",
                table: "producto");
        }
    }
}
