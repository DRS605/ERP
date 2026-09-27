using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Contabilidad.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AnulacionAsientos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "anula_asiento_id",
                schema: "contabilidad",
                table: "asiento",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ux_asiento_anula",
                schema: "contabilidad",
                table: "asiento",
                column: "anula_asiento_id",
                unique: true,
                filter: "anula_asiento_id IS NOT NULL");

            // El contraasiento apunta a un asiento que existe (y solo uno lo puede anular: índice único).
            migrationBuilder.Sql("""
                ALTER TABLE contabilidad.asiento ADD CONSTRAINT fk_asiento_anula
                    FOREIGN KEY (anula_asiento_id) REFERENCES contabilidad.asiento (id);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE contabilidad.asiento DROP CONSTRAINT IF EXISTS fk_asiento_anula;");
            migrationBuilder.DropIndex(
                name: "ux_asiento_anula",
                schema: "contabilidad",
                table: "asiento");

            migrationBuilder.DropColumn(
                name: "anula_asiento_id",
                schema: "contabilidad",
                table: "asiento");
        }
    }
}
