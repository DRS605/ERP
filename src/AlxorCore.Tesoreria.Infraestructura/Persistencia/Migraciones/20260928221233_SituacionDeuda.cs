using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Tesoreria.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class SituacionDeuda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "vencimiento",
                schema: "tesoreria",
                table: "linea_remesa",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cuenta_contable",
                schema: "tesoreria",
                table: "efecto_cartera",
                type: "character varying(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "configuracion_cartera",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    impagados_a_4315 = table.Column<bool>(type: "boolean", nullable: false),
                    porcentaje_renovacion = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracion_cartera", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "regularizacion_deuda",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    situacion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    movimiento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cuenta = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    dotacion = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    deshecha = table.Column<bool>(type: "boolean", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regularizacion_deuda", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "renovacion_efecto",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_documento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tercero_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tercero_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    gastos = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    movimiento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    creada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    anulada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_renovacion_efecto", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "situacion_deuda",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_documento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tercero_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tercero_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cuenta_origen = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    cuenta = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    clasificacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_clasificacion = table.Column<DateOnly>(type: "date", nullable: true),
                    dotado = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    incobrable_el = table.Column<DateOnly>(type: "date", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_situacion_deuda", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_configuracion_cartera_empresa",
                schema: "tesoreria",
                table: "configuracion_cartera",
                column: "empresa_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_regularizacion_deuda_movimiento",
                schema: "tesoreria",
                table: "regularizacion_deuda",
                column: "movimiento_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_renovacion_efecto_documento",
                schema: "tesoreria",
                table: "renovacion_efecto",
                columns: new[] { "empresa_id", "tipo_documento", "documento_id" });

            migrationBuilder.CreateIndex(
                name: "ux_situacion_deuda_documento",
                schema: "tesoreria",
                table: "situacion_deuda",
                columns: new[] { "empresa_id", "tipo_documento", "documento_id" },
                unique: true);

            foreach (var tabla in new[] { "configuracion_cartera", "situacion_deuda", "regularizacion_deuda", "renovacion_efecto" })
            {
                migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("tesoreria", tabla));
            }

            migrationBuilder.Sql("""
                ALTER TABLE tesoreria.situacion_deuda ADD CONSTRAINT ck_situacion_deuda_importes CHECK (importe >= 0 AND dotado >= 0 AND dotado <= importe);
                ALTER TABLE tesoreria.renovacion_efecto ADD CONSTRAINT ck_renovacion_importes CHECK (importe > 0 AND gastos >= 0);
                ALTER TABLE tesoreria.configuracion_cartera ADD CONSTRAINT ck_configuracion_cartera_porcentaje CHECK (porcentaje_renovacion BETWEEN 0 AND 100);
                ALTER TABLE tesoreria.movimiento DROP CONSTRAINT IF EXISTS ck_movimiento_cuenta_puente;
                ALTER TABLE tesoreria.movimiento
                    ADD CONSTRAINT ck_movimiento_cuenta_puente CHECK (cuenta_puente IS NULL OR (cuenta_puente IN ('407', '555', '4311', '4310', '650') AND cuenta_bancaria_id IS NULL));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE tesoreria.movimiento DROP CONSTRAINT IF EXISTS ck_movimiento_cuenta_puente;
                ALTER TABLE tesoreria.movimiento
                    ADD CONSTRAINT ck_movimiento_cuenta_puente CHECK (cuenta_puente IS NULL OR (cuenta_puente IN ('407', '555', '4311') AND cuenta_bancaria_id IS NULL));
                """);
            migrationBuilder.DropTable(
                name: "configuracion_cartera",
                schema: "tesoreria");

            migrationBuilder.DropTable(
                name: "regularizacion_deuda",
                schema: "tesoreria");

            migrationBuilder.DropTable(
                name: "renovacion_efecto",
                schema: "tesoreria");

            migrationBuilder.DropTable(
                name: "situacion_deuda",
                schema: "tesoreria");

            migrationBuilder.DropColumn(
                name: "vencimiento",
                schema: "tesoreria",
                table: "linea_remesa");

            migrationBuilder.DropColumn(
                name: "cuenta_contable",
                schema: "tesoreria",
                table: "efecto_cartera");
        }
    }
}
