using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Gastos.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class CargosAcreedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cargo_acreedor_liquidado",
                schema: "gastos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    gasto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    acreedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    origen = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    linea_orden = table.Column<int>(type: "integer", nullable: false),
                    concepto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cargo_acreedor_liquidado", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_cargo_acreedor_documento",
                schema: "gastos",
                table: "cargo_acreedor_liquidado",
                columns: new[] { "empresa_id", "origen", "documento_id" });

            migrationBuilder.CreateIndex(
                name: "ix_cargo_acreedor_gasto",
                schema: "gastos",
                table: "cargo_acreedor_liquidado",
                column: "gasto_id");

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("gastos", "cargo_acreedor_liquidado"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.SoloInsercion("gastos", "cargo_acreedor_liquidado", "cargo_acreedor.inmutable", "Un cargo de acreedor liquidado no se modifica: anula la factura del acreedor."));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cargo_acreedor_liquidado",
                schema: "gastos");
        }
    }
}
