using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Informes.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class FichaPlastico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ficha_plastico",
                schema: "fiscal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    clave = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    kg_por_unidad = table.Column<decimal>(type: "numeric(14,6)", nullable: false),
                    kg_reciclado_por_unidad = table.Column<decimal>(type: "numeric(14,6)", nullable: false),
                    exento = table.Column<bool>(type: "boolean", nullable: false),
                    motivo_exencion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ficha_plastico", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_ficha_plastico_producto",
                schema: "fiscal",
                table: "ficha_plastico",
                columns: new[] { "empresa_id", "producto_id" },
                unique: true);

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("fiscal", "ficha_plastico"));
            migrationBuilder.Sql("""
                ALTER TABLE fiscal.ficha_plastico ADD CONSTRAINT ck_ficha_plastico_kg CHECK (
                    kg_por_unidad > 0 AND kg_reciclado_por_unidad >= 0 AND kg_reciclado_por_unidad <= kg_por_unidad AND (exento = (motivo_exencion IS NOT NULL)));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ficha_plastico",
                schema: "fiscal");
        }
    }
}
