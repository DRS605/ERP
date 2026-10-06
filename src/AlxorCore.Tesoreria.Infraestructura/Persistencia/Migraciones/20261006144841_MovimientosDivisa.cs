using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Tesoreria.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class MovimientosDivisa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "diferencia_cambio",
                schema: "tesoreria",
                table: "movimiento",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "importe_divisa",
                schema: "tesoreria",
                table: "movimiento",
                type: "numeric(14,2)",
                nullable: true);
            migrationBuilder.Sql("ALTER TABLE tesoreria.movimiento ADD CONSTRAINT ck_movimiento_divisa CHECK (importe_divisa IS NOT NULL OR diferencia_cambio = 0);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE tesoreria.movimiento DROP CONSTRAINT ck_movimiento_divisa;");
            migrationBuilder.DropColumn(
                name: "diferencia_cambio",
                schema: "tesoreria",
                table: "movimiento");

            migrationBuilder.DropColumn(
                name: "importe_divisa",
                schema: "tesoreria",
                table: "movimiento");
        }
    }
}
