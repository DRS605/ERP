using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Contabilidad.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class Financiacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "operacion_financiacion",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    entidad = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    estado = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    fecha_formalizacion = table.Column<DateOnly>(type: "date", nullable: false),
                    capital = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    tipo_interes = table.Column<decimal>(type: "numeric(7,4)", nullable: false),
                    sistema = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    periodicidad = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    numero_cuotas = table.Column<int>(type: "integer", nullable: false),
                    cuotas_carencia = table.Column<int>(type: "integer", nullable: false),
                    fecha_primera_cuota = table.Column<DateOnly>(type: "date", nullable: true),
                    valor_residual = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    porcentaje_iva = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    fecha_vencimiento = table.Column<DateOnly>(type: "date", nullable: true),
                    comision_no_disponible = table.Column<decimal>(type: "numeric(6,4)", nullable: false),
                    contabilizar_desde = table.Column<DateOnly>(type: "date", nullable: true),
                    cuenta_largo_plazo = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    cuenta_corto_plazo = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    cuenta_intereses = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    cuenta_tesoreria = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    cuenta_comisiones = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    cuenta_activo = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operacion_financiacion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "evento_financiacion",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    tipo = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: true),
                    ejercicio = table.Column<int>(type: "integer", nullable: true),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    intereses = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    comision = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    iva = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    tipo_interes = table.Column<decimal>(type: "numeric(7,4)", nullable: true),
                    desde = table.Column<DateOnly>(type: "date", nullable: true),
                    asiento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    operacion_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evento_financiacion", x => x.id);
                    table.ForeignKey(
                        name: "FK_evento_financiacion_operacion_financiacion_operacion_id",
                        column: x => x.operacion_id,
                        principalSchema: "contabilidad",
                        principalTable: "operacion_financiacion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_evento_financiacion_asiento",
                schema: "contabilidad",
                table: "evento_financiacion",
                column: "asiento_id");

            migrationBuilder.CreateIndex(
                name: "ux_evento_financiacion_orden",
                schema: "contabilidad",
                table: "evento_financiacion",
                columns: new[] { "operacion_id", "orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_operacion_financiacion_codigo",
                schema: "contabilidad",
                table: "operacion_financiacion",
                columns: new[] { "empresa_id", "codigo" },
                unique: true);
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("contabilidad", "operacion_financiacion"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("contabilidad", "evento_financiacion", "operacion_id", "contabilidad", "operacion_financiacion"));
            migrationBuilder.Sql("""
                ALTER TABLE contabilidad.operacion_financiacion
                    ADD CONSTRAINT ck_operacion_financiacion_tipo CHECK (tipo IN ('Prestamo', 'Leasing', 'Poliza')),
                    ADD CONSTRAINT ck_operacion_financiacion_capital CHECK (capital > 0 AND tipo_interes BETWEEN 0 AND 50),
                    ADD CONSTRAINT ck_operacion_financiacion_cuotas CHECK (
                        (tipo = 'Poliza' AND fecha_vencimiento > fecha_formalizacion AND cuenta_largo_plazo IS NULL)
                        OR (tipo <> 'Poliza' AND numero_cuotas BETWEEN 1 AND 600 AND cuotas_carencia BETWEEN 0 AND numero_cuotas - 1
                            AND fecha_primera_cuota > fecha_formalizacion AND cuenta_largo_plazo IS NOT NULL)),
                    ADD CONSTRAINT ck_operacion_financiacion_leasing CHECK (
                        tipo = 'Leasing' AND cuenta_activo LIKE '2%' OR tipo <> 'Leasing' AND valor_residual = 0 AND porcentaje_iva = 0),
                    ADD CONSTRAINT ck_operacion_financiacion_residual CHECK (valor_residual >= 0 AND valor_residual < capital);
                ALTER TABLE contabilidad.evento_financiacion
                    ADD CONSTRAINT ck_evento_financiacion_importes CHECK (importe >= 0 AND intereses >= 0 AND comision >= 0 AND iva >= 0);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.QuitarRlsPorPadre("contabilidad", "evento_financiacion"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Desactivar("contabilidad", "operacion_financiacion"));
            migrationBuilder.DropTable(
                name: "evento_financiacion",
                schema: "contabilidad");

            migrationBuilder.DropTable(
                name: "operacion_financiacion",
                schema: "contabilidad");
        }
    }
}
