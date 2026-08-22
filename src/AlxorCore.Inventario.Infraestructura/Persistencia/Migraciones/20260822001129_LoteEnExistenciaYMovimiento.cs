using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Inventario.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class LoteEnExistenciaYMovimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_existencia_clave",
                schema: "inventario",
                table: "existencia");

            migrationBuilder.AddColumn<string>(
                name: "lote",
                schema: "inventario",
                table: "movimiento_inventario",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "lote",
                schema: "inventario",
                table: "existencia",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_existencia_clave",
                schema: "inventario",
                table: "existencia",
                columns: new[] { "empresa_id", "producto_id", "almacen_id", "ubicacion_id", "lote" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_existencia_clave",
                schema: "inventario",
                table: "existencia");

            migrationBuilder.DropColumn(
                name: "lote",
                schema: "inventario",
                table: "movimiento_inventario");

            migrationBuilder.DropColumn(
                name: "lote",
                schema: "inventario",
                table: "existencia");

            migrationBuilder.CreateIndex(
                name: "ix_existencia_clave",
                schema: "inventario",
                table: "existencia",
                columns: new[] { "empresa_id", "producto_id", "almacen_id", "ubicacion_id" });
        }
    }
}
