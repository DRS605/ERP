using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Facturacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgentesComisiones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agente_comercial",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    proveedor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    porcentaje = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    devengo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    porcentaje_irpf = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agente_comercial", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "asignacion_agente",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    desde = table.Column<DateOnly>(type: "date", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asignacion_agente", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "liquidacion_agente",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    desde = table.Column<DateOnly>(type: "date", nullable: false),
                    hasta = table.Column<DateOnly>(type: "date", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    gasto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_liquidacion_agente", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "regla_comision",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    familia_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    porcentaje = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regla_comision", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "linea_liquidacion_agente",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    factura_id = table.Column<Guid>(type: "uuid", nullable: false),
                    factura = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    cliente = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    base_venta = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    porcentaje_devengado = table.Column<decimal>(type: "numeric(7,4)", nullable: false),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    liquidacion_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_liquidacion_agente", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_liquidacion_agente_liquidacion_agente_liquidacion_id",
                        column: x => x.liquidacion_id,
                        principalSchema: "facturacion",
                        principalTable: "liquidacion_agente",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_agente_comercial_proveedor",
                schema: "facturacion",
                table: "agente_comercial",
                column: "proveedor_id");

            migrationBuilder.CreateIndex(
                name: "ix_asignacion_agente_agente",
                schema: "facturacion",
                table: "asignacion_agente",
                column: "agente_id");

            migrationBuilder.CreateIndex(
                name: "ix_asignacion_agente_cliente",
                schema: "facturacion",
                table: "asignacion_agente",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ux_asignacion_agente",
                schema: "facturacion",
                table: "asignacion_agente",
                columns: new[] { "empresa_id", "cliente_id", "desde" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_linea_liquidacion_agente_factura",
                schema: "facturacion",
                table: "linea_liquidacion_agente",
                column: "factura_id");

            migrationBuilder.CreateIndex(
                name: "ix_linea_liquidacion_agente_liquidacion",
                schema: "facturacion",
                table: "linea_liquidacion_agente",
                column: "liquidacion_id");

            migrationBuilder.CreateIndex(
                name: "ix_liquidacion_agente_agente",
                schema: "facturacion",
                table: "liquidacion_agente",
                column: "agente_id");

            migrationBuilder.CreateIndex(
                name: "ix_liquidacion_agente_gasto",
                schema: "facturacion",
                table: "liquidacion_agente",
                column: "gasto_id");

            migrationBuilder.CreateIndex(
                name: "ux_liquidacion_agente_numero",
                schema: "facturacion",
                table: "liquidacion_agente",
                columns: new[] { "empresa_id", "ejercicio", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_regla_comision_agente",
                schema: "facturacion",
                table: "regla_comision",
                column: "agente_id");

            migrationBuilder.CreateIndex(
                name: "ix_regla_comision_cliente",
                schema: "facturacion",
                table: "regla_comision",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_regla_comision_familia",
                schema: "facturacion",
                table: "regla_comision",
                column: "familia_id");

            foreach (var tabla in new[] { "agente_comercial", "asignacion_agente", "regla_comision", "liquidacion_agente" })
            {
                migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("facturacion", tabla));
            }

            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("facturacion", "linea_liquidacion_agente", "liquidacion_id", "facturacion", "liquidacion_agente"));
            migrationBuilder.Sql("""
                ALTER TABLE facturacion.agente_comercial
                    ADD CONSTRAINT ck_agente_comercial_valores CHECK (length(trim(nombre)) > 0 AND porcentaje BETWEEN 0 AND 100 AND porcentaje_irpf BETWEEN 0 AND 50
                        AND devengo IN ('Facturado', 'Cobrado'));
                ALTER TABLE facturacion.asignacion_agente
                    ADD CONSTRAINT fk_asignacion_agente_agente FOREIGN KEY (agente_id) REFERENCES facturacion.agente_comercial (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE facturacion.regla_comision
                    ADD CONSTRAINT ck_regla_comision_valores CHECK (porcentaje BETWEEN 0 AND 100 AND (familia_id IS NOT NULL OR cliente_id IS NOT NULL)),
                    ADD CONSTRAINT fk_regla_comision_agente FOREIGN KEY (agente_id) REFERENCES facturacion.agente_comercial (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE facturacion.liquidacion_agente
                    ADD CONSTRAINT ck_liquidacion_agente_valores CHECK (hasta >= desde AND estado IN ('Emitida', 'Anulada')),
                    ADD CONSTRAINT fk_liquidacion_agente_agente FOREIGN KEY (agente_id) REFERENCES facturacion.agente_comercial (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE facturacion.linea_liquidacion_agente
                    ADD CONSTRAINT ck_linea_liquidacion_agente_valores CHECK (importe <> 0 AND porcentaje_devengado BETWEEN 0 AND 100);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.QuitarRlsPorPadre("facturacion", "linea_liquidacion_agente"));
            foreach (var tabla in new[] { "agente_comercial", "asignacion_agente", "regla_comision", "liquidacion_agente" })
            {
                migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Desactivar("facturacion", tabla));
            }

            migrationBuilder.DropTable(
                name: "agente_comercial",
                schema: "facturacion");

            migrationBuilder.DropTable(
                name: "asignacion_agente",
                schema: "facturacion");

            migrationBuilder.DropTable(
                name: "linea_liquidacion_agente",
                schema: "facturacion");

            migrationBuilder.DropTable(
                name: "regla_comision",
                schema: "facturacion");

            migrationBuilder.DropTable(
                name: "liquidacion_agente",
                schema: "facturacion");
        }
    }
}
