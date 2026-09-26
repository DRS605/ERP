using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Gastos.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AfectacionGasto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "afectacion",
                schema: "gastos",
                table: "gasto",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Comun");

            migrationBuilder.Sql("""
                ALTER TABLE gastos.gasto ADD CONSTRAINT ck_gasto_afectacion CHECK (afectacion IN ('Comun', 'ConDerecho', 'SinDerecho'));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "afectacion",
                schema: "gastos",
                table: "gasto");
        }
    }
}
