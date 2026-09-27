using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Tesoreria.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AnulacionMovimientos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "anula_movimiento_id",
                schema: "tesoreria",
                table: "movimiento",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ux_movimiento_anula",
                schema: "tesoreria",
                table: "movimiento",
                column: "anula_movimiento_id",
                unique: true,
                filter: "anula_movimiento_id IS NOT NULL");

            // Una anulación apunta a un movimiento que existe y lleva el importe en negativo; el resto, en positivo.
            migrationBuilder.Sql("""
                ALTER TABLE tesoreria.movimiento ADD CONSTRAINT fk_movimiento_anula
                    FOREIGN KEY (anula_movimiento_id) REFERENCES tesoreria.movimiento (id);
                ALTER TABLE tesoreria.movimiento ADD CONSTRAINT ck_movimiento_importe
                    CHECK ((anula_movimiento_id IS NULL AND importe > 0) OR (anula_movimiento_id IS NOT NULL AND importe < 0));

                -- Al anular el cobro de un anticipo se anota su aplicación en negativo (el anticipo recupera el saldo).
                ALTER TABLE tesoreria.aplicacion_anticipo DROP CONSTRAINT ck_aplicacion_anticipo_importe;
                ALTER TABLE tesoreria.aplicacion_anticipo ADD CONSTRAINT ck_aplicacion_anticipo_importe CHECK (importe <> 0);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE tesoreria.movimiento DROP CONSTRAINT IF EXISTS ck_movimiento_importe;
                ALTER TABLE tesoreria.movimiento DROP CONSTRAINT IF EXISTS fk_movimiento_anula;
                ALTER TABLE tesoreria.aplicacion_anticipo DROP CONSTRAINT IF EXISTS ck_aplicacion_anticipo_importe;
                ALTER TABLE tesoreria.aplicacion_anticipo ADD CONSTRAINT ck_aplicacion_anticipo_importe CHECK (importe > 0);
                """);
            migrationBuilder.DropIndex(
                name: "ux_movimiento_anula",
                schema: "tesoreria",
                table: "movimiento");

            migrationBuilder.DropColumn(
                name: "anula_movimiento_id",
                schema: "tesoreria",
                table: "movimiento");
        }
    }
}
