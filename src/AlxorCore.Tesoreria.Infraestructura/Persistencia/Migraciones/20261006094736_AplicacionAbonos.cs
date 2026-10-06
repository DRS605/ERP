using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Tesoreria.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AplicacionAbonos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "es_aplicacion_abono",
                schema: "tesoreria",
                table: "movimiento",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Solo la aplicación de un abono (y la anulación de esa aplicación) rompe la regla del signo.
            migrationBuilder.Sql("""
                ALTER TABLE tesoreria.movimiento DROP CONSTRAINT ck_movimiento_importe;
                ALTER TABLE tesoreria.movimiento ADD CONSTRAINT ck_movimiento_importe CHECK (
                    (anula_movimiento_id IS NULL AND ((NOT es_aplicacion_abono AND importe > 0) OR (es_aplicacion_abono AND importe < 0 AND cuenta_puente IS NOT NULL)))
                    OR (anula_movimiento_id IS NOT NULL AND ((NOT es_aplicacion_abono AND importe < 0) OR (es_aplicacion_abono AND importe > 0))));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE tesoreria.movimiento DROP CONSTRAINT ck_movimiento_importe;
                ALTER TABLE tesoreria.movimiento ADD CONSTRAINT ck_movimiento_importe
                    CHECK ((anula_movimiento_id IS NULL AND importe > 0) OR (anula_movimiento_id IS NOT NULL AND importe < 0));
                """);

            migrationBuilder.DropColumn(
                name: "es_aplicacion_abono",
                schema: "tesoreria",
                table: "movimiento");
        }
    }
}
