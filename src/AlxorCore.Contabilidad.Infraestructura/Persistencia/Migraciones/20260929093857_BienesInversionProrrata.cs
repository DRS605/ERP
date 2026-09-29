using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Contabilidad.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class BienesInversionProrrata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "bien_inmueble",
                schema: "contabilidad",
                table: "inmovilizado",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "cuota_impuesto_soportada",
                schema: "contabilidad",
                table: "inmovilizado",
                type: "numeric(14,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "porcentaje_deduccion_inicial",
                schema: "contabilidad",
                table: "inmovilizado",
                type: "integer",
                nullable: true);
            migrationBuilder.Sql("""
                ALTER TABLE contabilidad.inmovilizado ADD CONSTRAINT ck_inmovilizado_bien_inversion CHECK (
                    (cuota_impuesto_soportada IS NULL AND porcentaje_deduccion_inicial IS NULL)
                    OR (cuota_impuesto_soportada > 0 AND porcentaje_deduccion_inicial BETWEEN 0 AND 100));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE contabilidad.inmovilizado DROP CONSTRAINT IF EXISTS ck_inmovilizado_bien_inversion;");
            migrationBuilder.DropColumn(
                name: "bien_inmueble",
                schema: "contabilidad",
                table: "inmovilizado");

            migrationBuilder.DropColumn(
                name: "cuota_impuesto_soportada",
                schema: "contabilidad",
                table: "inmovilizado");

            migrationBuilder.DropColumn(
                name: "porcentaje_deduccion_inicial",
                schema: "contabilidad",
                table: "inmovilizado");
        }
    }
}
