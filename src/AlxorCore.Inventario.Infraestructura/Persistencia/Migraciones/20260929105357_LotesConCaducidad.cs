using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Inventario.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class LotesConCaducidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "lote_articulo",
                schema: "inventario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    fecha_caducidad = table.Column<DateOnly>(type: "date", nullable: true),
                    fecha_fabricacion = table.Column<DateOnly>(type: "date", nullable: true),
                    observaciones = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lote_articulo", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_lote_articulo",
                schema: "inventario",
                table: "lote_articulo",
                columns: new[] { "empresa_id", "producto_id", "codigo" },
                unique: true);
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("inventario", "lote_articulo"));
            migrationBuilder.Sql("""
                ALTER TABLE inventario.lote_articulo ADD CONSTRAINT ck_lote_articulo_fechas
                    CHECK (fecha_caducidad IS NULL OR fecha_fabricacion IS NULL OR fecha_caducidad >= fecha_fabricacion);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lote_articulo",
                schema: "inventario");
        }
    }
}
