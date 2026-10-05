using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Tesoreria.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class SeguroCredito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "poliza_seguro",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    aseguradora = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    numero_poliza = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    porcentaje_cobertura = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    plazo_aviso_dias = table.Column<int>(type: "integer", nullable: false),
                    desde = table.Column<DateOnly>(type: "date", nullable: false),
                    hasta = table.Column<DateOnly>(type: "date", nullable: true),
                    bloquear_sin_cobertura = table.Column<bool>(type: "boolean", nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_poliza_seguro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "aviso_impago",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    poliza_id = table.Column<Guid>(type: "uuid", nullable: false),
                    factura_id = table.Column<Guid>(type: "uuid", nullable: false),
                    factura = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    vencimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    referencia = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_cierre = table.Column<DateOnly>(type: "date", nullable: true),
                    indemnizacion = table.Column<decimal>(type: "numeric(14,2)", nullable: true),
                    nota = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aviso_impago", x => x.id);
                    table.ForeignKey(
                        name: "fk_aviso_impago_poliza",
                        column: x => x.poliza_id,
                        principalSchema: "tesoreria",
                        principalTable: "poliza_seguro",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "clasificacion_seguro",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    poliza_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    referencia = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    solicitado = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    concedido = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    efecto = table.Column<DateOnly>(type: "date", nullable: false),
                    vence = table.Column<DateOnly>(type: "date", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clasificacion_seguro", x => x.id);
                    table.ForeignKey(
                        name: "fk_clasificacion_poliza",
                        column: x => x.poliza_id,
                        principalSchema: "tesoreria",
                        principalTable: "poliza_seguro",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cambio_clasificacion",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    concedido = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    nota = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    registrado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    clasificacion_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cambio_clasificacion", x => x.id);
                    table.ForeignKey(
                        name: "FK_cambio_clasificacion_clasificacion_seguro_clasificacion_id",
                        column: x => x.clasificacion_id,
                        principalSchema: "tesoreria",
                        principalTable: "clasificacion_seguro",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_aviso_impago_poliza_id",
                schema: "tesoreria",
                table: "aviso_impago",
                column: "poliza_id");

            migrationBuilder.CreateIndex(
                name: "ix_aviso_impago_cliente",
                schema: "tesoreria",
                table: "aviso_impago",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ux_aviso_impago_abierto",
                schema: "tesoreria",
                table: "aviso_impago",
                column: "factura_id",
                unique: true,
                filter: "estado = 'Comunicado'");

            migrationBuilder.CreateIndex(
                name: "IX_cambio_clasificacion_clasificacion_id",
                schema: "tesoreria",
                table: "cambio_clasificacion",
                column: "clasificacion_id");

            migrationBuilder.CreateIndex(
                name: "ix_clasificacion_cliente",
                schema: "tesoreria",
                table: "clasificacion_seguro",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ux_clasificacion_poliza_cliente",
                schema: "tesoreria",
                table: "clasificacion_seguro",
                columns: new[] { "poliza_id", "cliente_id" },
                unique: true);

            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.FuncionesComunes);
            foreach (var tabla in new[] { "poliza_seguro", "clasificacion_seguro", "aviso_impago" })
            {
                migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("tesoreria", tabla));
            }

            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("tesoreria", "cambio_clasificacion", "clasificacion_id", "tesoreria", "clasificacion_seguro"));
            migrationBuilder.Sql("""
                ALTER TABLE tesoreria.poliza_seguro ADD CONSTRAINT ck_poliza_seguro_valores CHECK (
                    porcentaje_cobertura > 0 AND porcentaje_cobertura <= 100 AND plazo_aviso_dias BETWEEN 1 AND 365 AND (hasta IS NULL OR hasta >= desde));
                ALTER TABLE tesoreria.clasificacion_seguro ADD CONSTRAINT ck_clasificacion_concedido CHECK (
                    concedido >= 0 AND (estado IN ('Concedida', 'Reducida') OR concedido = 0) AND (vence IS NULL OR vence >= efecto));
                ALTER TABLE tesoreria.aviso_impago ADD CONSTRAINT ck_aviso_impago_valores CHECK (
                    importe > 0 AND fecha >= vencimiento AND (estado = 'Comunicado') = (fecha_cierre IS NULL)
                    AND (estado = 'Indemnizado') = (indemnizacion IS NOT NULL));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.QuitarRlsPorPadre("tesoreria", "cambio_clasificacion"));
            migrationBuilder.DropTable(
                name: "aviso_impago",
                schema: "tesoreria");

            migrationBuilder.DropTable(
                name: "cambio_clasificacion",
                schema: "tesoreria");

            migrationBuilder.DropTable(
                name: "clasificacion_seguro",
                schema: "tesoreria");

            migrationBuilder.DropTable(
                name: "poliza_seguro",
                schema: "tesoreria");
        }
    }
}
