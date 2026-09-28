using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Tesoreria.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class EntregasCuentaYLiquidacionesPagos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cuenta_puente",
                schema: "tesoreria",
                table: "movimiento",
                type: "character varying(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "entrega_cuenta_proveedor",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    proveedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    concepto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    metodo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    cuenta_bancaria_id = table.Column<Guid>(type: "uuid", nullable: true),
                    creada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    anulada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    motivo_anulacion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entrega_cuenta_proveedor", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "liquidacion_pagos",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    hasta = table.Column<DateOnly>(type: "date", nullable: false),
                    proveedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    a_pagar = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    entregas_canceladas = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    compensado = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    liquido = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    forma_pago = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cuenta_bancaria_id = table.Column<Guid>(type: "uuid", nullable: true),
                    remesa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    lote_id = table.Column<Guid>(type: "uuid", nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    creada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    anulada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    motivo_anulacion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_liquidacion_pagos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cancelacion_entrega_cuenta",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    gasto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    movimiento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    liquidacion_id = table.Column<Guid>(type: "uuid", nullable: true),
                    entrega_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cancelacion_entrega_cuenta", x => x.id);
                    table.ForeignKey(
                        name: "FK_cancelacion_entrega_cuenta_entrega_cuenta_proveedor_entrega~",
                        column: x => x.entrega_id,
                        principalSchema: "tesoreria",
                        principalTable: "entrega_cuenta_proveedor",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "linea_liquidacion_pagos",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    movimiento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    entrega_id = table.Column<Guid>(type: "uuid", nullable: true),
                    liquidacion_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_liquidacion_pagos", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_liquidacion_pagos_liquidacion_pagos_liquidacion_id",
                        column: x => x.liquidacion_id,
                        principalSchema: "tesoreria",
                        principalTable: "liquidacion_pagos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_cancelacion_entrega_cuenta_entrega",
                schema: "tesoreria",
                table: "cancelacion_entrega_cuenta",
                column: "entrega_id");

            migrationBuilder.CreateIndex(
                name: "ux_cancelacion_entrega_cuenta_movimiento",
                schema: "tesoreria",
                table: "cancelacion_entrega_cuenta",
                column: "movimiento_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_entrega_cuenta_proveedor_empresa_proveedor",
                schema: "tesoreria",
                table: "entrega_cuenta_proveedor",
                columns: new[] { "empresa_id", "proveedor_id" });

            migrationBuilder.CreateIndex(
                name: "ix_linea_liquidacion_pagos_liquidacion",
                schema: "tesoreria",
                table: "linea_liquidacion_pagos",
                column: "liquidacion_id");

            migrationBuilder.CreateIndex(
                name: "ux_linea_liquidacion_pagos_movimiento",
                schema: "tesoreria",
                table: "linea_liquidacion_pagos",
                column: "movimiento_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_liquidacion_pagos_lote",
                schema: "tesoreria",
                table: "liquidacion_pagos",
                column: "lote_id");

            migrationBuilder.CreateIndex(
                name: "ix_liquidacion_pagos_proveedor",
                schema: "tesoreria",
                table: "liquidacion_pagos",
                columns: new[] { "empresa_id", "proveedor_id" });

            migrationBuilder.CreateIndex(
                name: "ux_liquidacion_pagos_numero",
                schema: "tesoreria",
                table: "liquidacion_pagos",
                columns: new[] { "empresa_id", "ejercicio", "numero" },
                unique: true);

            // Garantías: cada empresa ve lo suyo; las cancelaciones y las líneas no se tocan (se deshacen con otra en negativo
            // o anulando la liquidación); la cuenta puente solo en movimientos sin banco.
            migrationBuilder.Sql(RlsSql.Activar("tesoreria", "entrega_cuenta_proveedor"));
            migrationBuilder.Sql(RlsSql.Activar("tesoreria", "liquidacion_pagos"));
            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("tesoreria", "cancelacion_entrega_cuenta", "entrega_id", "tesoreria", "entrega_cuenta_proveedor"));
            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("tesoreria", "linea_liquidacion_pagos", "liquidacion_id", "tesoreria", "liquidacion_pagos"));
            migrationBuilder.Sql("""
                ALTER TABLE tesoreria.movimiento
                    ADD CONSTRAINT ck_movimiento_cuenta_puente CHECK (cuenta_puente IS NULL OR (cuenta_puente IN ('407', '555') AND cuenta_bancaria_id IS NULL));
                ALTER TABLE tesoreria.entrega_cuenta_proveedor
                    ADD CONSTRAINT ck_entrega_cuenta_proveedor_importe CHECK (importe > 0);
                ALTER TABLE tesoreria.cancelacion_entrega_cuenta
                    ADD CONSTRAINT ck_cancelacion_entrega_cuenta_importe CHECK (importe <> 0),
                    ADD CONSTRAINT fk_cancelacion_entrega_cuenta_movimiento FOREIGN KEY (movimiento_id) REFERENCES tesoreria.movimiento (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE tesoreria.liquidacion_pagos
                    ADD CONSTRAINT ck_liquidacion_pagos_estado CHECK (estado IN ('Emitida', 'Anulada')),
                    ADD CONSTRAINT ck_liquidacion_pagos_forma CHECK (forma_pago IN ('Pendiente', 'Directo', 'Remesa')),
                    ADD CONSTRAINT ck_liquidacion_pagos_importes CHECK (a_pagar >= 0 AND entregas_canceladas >= 0 AND compensado >= 0 AND liquido >= 0
                        AND liquido = a_pagar - entregas_canceladas - compensado),
                    ADD CONSTRAINT ck_liquidacion_pagos_anulada CHECK ((estado = 'Anulada') = (anulada_en IS NOT NULL));
                ALTER TABLE tesoreria.linea_liquidacion_pagos
                    ADD CONSTRAINT ck_linea_liquidacion_pagos_tipo CHECK (tipo IN ('EntregaCuenta', 'Compensacion', 'CobroCompensado', 'Pago')),
                    ADD CONSTRAINT ck_linea_liquidacion_pagos_importe CHECK (importe >= 0),
                    ADD CONSTRAINT fk_linea_liquidacion_pagos_movimiento FOREIGN KEY (movimiento_id) REFERENCES tesoreria.movimiento (id) DEFERRABLE INITIALLY DEFERRED;
                """);
            migrationBuilder.Sql(GarantiasSql.SoloInsercion("tesoreria", "cancelacion_entrega_cuenta", "entregacuenta.cancelacion_inalterable",
                "Una cancelación de entrega a cuenta no se modifica ni se borra: se deshace con otra en negativo."));
            migrationBuilder.Sql(GarantiasSql.SoloInsercion("tesoreria", "linea_liquidacion_pagos", "liquidacionpagos.linea_inalterable",
                "Las líneas de una liquidación de pagos no se modifican ni se borran: anula la liquidación."));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(GarantiasSql.QuitarTrigger("tesoreria", "linea_liquidacion_pagos", "tg_linea_liquidacion_pagos_solo_insercion"));
            migrationBuilder.Sql(GarantiasSql.QuitarTrigger("tesoreria", "cancelacion_entrega_cuenta", "tg_cancelacion_entrega_cuenta_solo_insercion"));
            migrationBuilder.Sql("ALTER TABLE tesoreria.movimiento DROP CONSTRAINT IF EXISTS ck_movimiento_cuenta_puente;");
            migrationBuilder.DropTable(
                name: "cancelacion_entrega_cuenta",
                schema: "tesoreria");

            migrationBuilder.DropTable(
                name: "linea_liquidacion_pagos",
                schema: "tesoreria");

            migrationBuilder.DropTable(
                name: "entrega_cuenta_proveedor",
                schema: "tesoreria");

            migrationBuilder.DropTable(
                name: "liquidacion_pagos",
                schema: "tesoreria");

            migrationBuilder.DropColumn(
                name: "cuenta_puente",
                schema: "tesoreria",
                table: "movimiento");
        }
    }
}
