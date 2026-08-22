using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Contabilidad.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class Inmovilizado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inmovilizado",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cuenta_activo = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    cuenta_amortizacion = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    cuenta_dotacion = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    fecha_adquisicion = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_alta = table.Column<DateOnly>(type: "date", nullable: false),
                    valor_adquisicion = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    valor_residual = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    periodicidad = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    metodo_contable = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    vida_util_contable = table.Column<int>(type: "integer", nullable: false),
                    porcentaje_degresivo_contable = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    metodo_fiscal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    vida_util_fiscal = table.Column<int>(type: "integer", nullable: false),
                    porcentaje_degresivo_fiscal = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_baja = table.Column<DateOnly>(type: "date", nullable: true),
                    valor_enajenacion = table.Column<decimal>(type: "numeric(14,2)", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inmovilizado", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ajuste_fiscal_amortizacion",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    diferencia_temporaria = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    tipo_impositivo = table.Column<decimal>(type: "numeric(6,4)", nullable: false),
                    asiento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    inmovilizado_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ajuste_fiscal_amortizacion", x => x.id);
                    table.ForeignKey(
                        name: "FK_ajuste_fiscal_amortizacion_inmovilizado_inmovilizado_id",
                        column: x => x.inmovilizado_id,
                        principalSchema: "contabilidad",
                        principalTable: "inmovilizado",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dotacion_amortizacion",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    mes = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    asiento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    inmovilizado_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dotacion_amortizacion", x => x.id);
                    table.ForeignKey(
                        name: "FK_dotacion_amortizacion_inmovilizado_inmovilizado_id",
                        column: x => x.inmovilizado_id,
                        principalSchema: "contabilidad",
                        principalTable: "inmovilizado",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ajuste_fiscal_amortizacion_inmovilizado_id",
                schema: "contabilidad",
                table: "ajuste_fiscal_amortizacion",
                column: "inmovilizado_id");

            migrationBuilder.CreateIndex(
                name: "IX_dotacion_amortizacion_inmovilizado_id",
                schema: "contabilidad",
                table: "dotacion_amortizacion",
                column: "inmovilizado_id");

            migrationBuilder.CreateIndex(
                name: "ux_inmovilizado_empresa_codigo",
                schema: "contabilidad",
                table: "inmovilizado",
                columns: new[] { "empresa_id", "codigo" },
                unique: true);

            // RLS sobre el inmovilizado (lleva empresa_id). Las dotaciones y ajustes fiscales se
            // protegen a través de su inmovilizado (filtro global de EF Core), igual que el apunte.
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("contabilidad", "inmovilizado"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ajuste_fiscal_amortizacion",
                schema: "contabilidad");

            migrationBuilder.DropTable(
                name: "dotacion_amortizacion",
                schema: "contabilidad");

            migrationBuilder.DropTable(
                name: "inmovilizado",
                schema: "contabilidad");
        }
    }
}
