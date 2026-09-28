using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Tesoreria.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class RemesasDescuentoGestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "comision",
                schema: "tesoreria",
                table: "remesa",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "dias_minimos",
                schema: "tesoreria",
                table: "remesa",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "gastos",
                schema: "tesoreria",
                table: "remesa",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "gastos_fijos",
                schema: "tesoreria",
                table: "remesa",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "gastos_por_efecto",
                schema: "tesoreria",
                table: "remesa",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "intereses",
                schema: "tesoreria",
                table: "remesa",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "iva_comision",
                schema: "tesoreria",
                table: "remesa",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "liquido",
                schema: "tesoreria",
                table: "remesa",
                type: "numeric(14,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "modalidad",
                schema: "tesoreria",
                table: "remesa",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Vencimiento");

            migrationBuilder.AddColumn<decimal>(
                name: "otros_gastos",
                schema: "tesoreria",
                table: "remesa",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "porcentaje_comision",
                schema: "tesoreria",
                table: "remesa",
                type: "numeric(7,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "porcentaje_interes",
                schema: "tesoreria",
                table: "remesa",
                type: "numeric(7,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "porcentaje_iva_comision",
                schema: "tesoreria",
                table: "remesa",
                type: "numeric(7,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateOnly>(
                name: "riesgo_cancelado_en",
                schema: "tesoreria",
                table: "remesa",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "timbres",
                schema: "tesoreria",
                table: "remesa",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql("""
                ALTER TABLE tesoreria.movimiento DROP CONSTRAINT IF EXISTS ck_movimiento_cuenta_puente;
                ALTER TABLE tesoreria.movimiento
                    ADD CONSTRAINT ck_movimiento_cuenta_puente CHECK (cuenta_puente IS NULL OR (cuenta_puente IN ('407', '555', '4311') AND cuenta_bancaria_id IS NULL));
                ALTER TABLE tesoreria.remesa
                    ADD CONSTRAINT ck_remesa_modalidad CHECK (modalidad IN ('Vencimiento', 'GestionCobro', 'Descuento') AND (modalidad = 'Vencimiento' OR tipo = 'Cobro')),
                    ADD CONSTRAINT ck_remesa_condiciones CHECK (porcentaje_interes >= 0 AND dias_minimos >= 0 AND gastos_fijos >= 0 AND gastos_por_efecto >= 0
                        AND timbres >= 0 AND otros_gastos >= 0 AND porcentaje_comision >= 0 AND porcentaje_iva_comision >= 0),
                    ADD CONSTRAINT ck_remesa_riesgo CHECK (riesgo_cancelado_en IS NULL OR (modalidad = 'Descuento' AND estado = 'Liquidada'));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE tesoreria.remesa DROP CONSTRAINT IF EXISTS ck_remesa_modalidad, DROP CONSTRAINT IF EXISTS ck_remesa_condiciones, DROP CONSTRAINT IF EXISTS ck_remesa_riesgo;
                ALTER TABLE tesoreria.movimiento DROP CONSTRAINT IF EXISTS ck_movimiento_cuenta_puente;
                ALTER TABLE tesoreria.movimiento
                    ADD CONSTRAINT ck_movimiento_cuenta_puente CHECK (cuenta_puente IS NULL OR (cuenta_puente IN ('407', '555') AND cuenta_bancaria_id IS NULL));
                """);

            migrationBuilder.DropColumn(
                name: "comision",
                schema: "tesoreria",
                table: "remesa");

            migrationBuilder.DropColumn(
                name: "dias_minimos",
                schema: "tesoreria",
                table: "remesa");

            migrationBuilder.DropColumn(
                name: "gastos",
                schema: "tesoreria",
                table: "remesa");

            migrationBuilder.DropColumn(
                name: "gastos_fijos",
                schema: "tesoreria",
                table: "remesa");

            migrationBuilder.DropColumn(
                name: "gastos_por_efecto",
                schema: "tesoreria",
                table: "remesa");

            migrationBuilder.DropColumn(
                name: "intereses",
                schema: "tesoreria",
                table: "remesa");

            migrationBuilder.DropColumn(
                name: "iva_comision",
                schema: "tesoreria",
                table: "remesa");

            migrationBuilder.DropColumn(
                name: "liquido",
                schema: "tesoreria",
                table: "remesa");

            migrationBuilder.DropColumn(
                name: "modalidad",
                schema: "tesoreria",
                table: "remesa");

            migrationBuilder.DropColumn(
                name: "otros_gastos",
                schema: "tesoreria",
                table: "remesa");

            migrationBuilder.DropColumn(
                name: "porcentaje_comision",
                schema: "tesoreria",
                table: "remesa");

            migrationBuilder.DropColumn(
                name: "porcentaje_interes",
                schema: "tesoreria",
                table: "remesa");

            migrationBuilder.DropColumn(
                name: "porcentaje_iva_comision",
                schema: "tesoreria",
                table: "remesa");

            migrationBuilder.DropColumn(
                name: "riesgo_cancelado_en",
                schema: "tesoreria",
                table: "remesa");

            migrationBuilder.DropColumn(
                name: "timbres",
                schema: "tesoreria",
                table: "remesa");
        }
    }
}
