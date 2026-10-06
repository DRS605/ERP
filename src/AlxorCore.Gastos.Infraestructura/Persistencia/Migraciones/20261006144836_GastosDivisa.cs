using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Gastos.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class GastosDivisa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "moneda",
                schema: "gastos",
                table: "gasto",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "tasa_cambio",
                schema: "gastos",
                table: "gasto",
                type: "numeric(18,8)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_divisa",
                schema: "gastos",
                table: "gasto",
                type: "numeric(14,2)",
                nullable: true);
            migrationBuilder.Sql("""
                ALTER TABLE gastos.gasto ADD CONSTRAINT ck_gasto_divisa CHECK (
                    (moneda IS NULL AND tasa_cambio IS NULL AND total_divisa IS NULL)
                    OR (moneda ~ '^[A-Z]{3}$' AND moneda <> 'EUR' AND tasa_cambio > 0 AND total_divisa IS NOT NULL));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE gastos.gasto DROP CONSTRAINT ck_gasto_divisa;");
            migrationBuilder.DropColumn(
                name: "moneda",
                schema: "gastos",
                table: "gasto");

            migrationBuilder.DropColumn(
                name: "tasa_cambio",
                schema: "gastos",
                table: "gasto");

            migrationBuilder.DropColumn(
                name: "total_divisa",
                schema: "gastos",
                table: "gasto");
        }
    }
}
