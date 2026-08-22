using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Catalogo.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class Familias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "familia_id",
                schema: "catalogo",
                table: "producto",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "familia",
                schema: "catalogo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    padre_id = table.Column<Guid>(type: "uuid", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    actualizado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_familia", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_producto_familia",
                schema: "catalogo",
                table: "producto",
                column: "familia_id");

            migrationBuilder.CreateIndex(
                name: "ix_familia_empresa_padre",
                schema: "catalogo",
                table: "familia",
                columns: new[] { "empresa_id", "padre_id" });

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("catalogo", "familia"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "familia",
                schema: "catalogo");

            migrationBuilder.DropIndex(
                name: "ix_producto_familia",
                schema: "catalogo",
                table: "producto");

            migrationBuilder.DropColumn(
                name: "familia_id",
                schema: "catalogo",
                table: "producto");
        }
    }
}
