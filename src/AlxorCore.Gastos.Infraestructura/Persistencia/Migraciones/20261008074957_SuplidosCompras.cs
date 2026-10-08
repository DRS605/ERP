using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Gastos.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class SuplidosCompras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "suplido",
                schema: "gastos",
                table: "linea_gasto",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "suplidos",
                schema: "gastos",
                table: "gasto",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            // Un suplido va sin impuesto ni recargo y con su cuenta.
            migrationBuilder.Sql("""
                ALTER TABLE gastos.linea_gasto ADD CONSTRAINT ck_linea_gasto_suplido CHECK (
                    NOT suplido OR (cuota = 0 AND porcentaje_iva = 0 AND cuota_recargo = 0 AND NOT autoliquidada AND cuenta_gasto IS NOT NULL));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE gastos.linea_gasto DROP CONSTRAINT IF EXISTS ck_linea_gasto_suplido;");

            migrationBuilder.DropColumn(
                name: "suplido",
                schema: "gastos",
                table: "linea_gasto");

            migrationBuilder.DropColumn(
                name: "suplidos",
                schema: "gastos",
                table: "gasto");
        }
    }
}
