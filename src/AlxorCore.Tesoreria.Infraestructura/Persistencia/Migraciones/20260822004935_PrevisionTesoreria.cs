using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Tesoreria.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class PrevisionTesoreria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "prevision",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sentido = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    concepto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prevision", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_prevision_empresa_fecha",
                schema: "tesoreria",
                table: "prevision",
                columns: new[] { "empresa_id", "fecha" });

            // Row-Level Security por empresa (defensa en profundidad), como el resto de tablas de negocio.
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("tesoreria", "prevision"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prevision",
                schema: "tesoreria");
        }
    }
}
