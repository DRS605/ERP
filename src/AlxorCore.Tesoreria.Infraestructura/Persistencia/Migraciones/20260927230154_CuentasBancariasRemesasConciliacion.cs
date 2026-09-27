using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Tesoreria.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class CuentasBancariasRemesasConciliacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "cuenta_bancaria_id",
                schema: "tesoreria",
                table: "movimiento",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "cuenta_bancaria",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    iban = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: true),
                    bic = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    subcuenta = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    predeterminada = table.Column<bool>(type: "boolean", nullable: false),
                    saldo_inicial = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    fecha_saldo_inicial = table.Column<DateOnly>(type: "date", nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cuenta_bancaria", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "extracto_bancario",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cuenta_bancaria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cuenta_fichero = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    desde = table.Column<DateOnly>(type: "date", nullable: true),
                    hasta = table.Column<DateOnly>(type: "date", nullable: true),
                    saldo_inicial = table.Column<decimal>(type: "numeric(14,2)", nullable: true),
                    saldo_final = table.Column<decimal>(type: "numeric(14,2)", nullable: true),
                    nombre_archivo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    huella = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    numero_apuntes = table.Column<int>(type: "integer", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extracto_bancario", x => x.id);
                    table.ForeignKey(
                        name: "FK_extracto_bancario_cuenta_bancaria_cuenta_bancaria_id",
                        column: x => x.cuenta_bancaria_id,
                        principalSchema: "tesoreria",
                        principalTable: "cuenta_bancaria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "remesa",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_cargo = table.Column<DateOnly>(type: "date", nullable: false),
                    cuenta_bancaria_id = table.Column<Guid>(type: "uuid", nullable: true),
                    esquema = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    secuencia = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    estado = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    total = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    fichero = table.Column<string>(type: "text", nullable: false),
                    nombre_archivo = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    mensaje_id = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    presentada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    fecha_liquidacion = table.Column<DateOnly>(type: "date", nullable: true),
                    anulada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_remesa", x => x.id);
                    table.ForeignKey(
                        name: "FK_remesa_cuenta_bancaria_cuenta_bancaria_id",
                        column: x => x.cuenta_bancaria_id,
                        principalSchema: "tesoreria",
                        principalTable: "cuenta_bancaria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "apunte_bancario",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    extracto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cuenta_bancaria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_valor = table.Column<DateOnly>(type: "date", nullable: true),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    concepto = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    concepto_comun = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    documento = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    referencia1 = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    referencia2 = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    estado = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    cuenta_asiento = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    concepto_asiento = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    asiento_origen_id = table.Column<Guid>(type: "uuid", nullable: true),
                    conciliado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apunte_bancario", x => x.id);
                    table.ForeignKey(
                        name: "FK_apunte_bancario_cuenta_bancaria_cuenta_bancaria_id",
                        column: x => x.cuenta_bancaria_id,
                        principalSchema: "tesoreria",
                        principalTable: "cuenta_bancaria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_apunte_bancario_extracto_bancario_extracto_id",
                        column: x => x.extracto_id,
                        principalSchema: "tesoreria",
                        principalTable: "extracto_bancario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "devolucion_recibo",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    movimiento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    anulacion_movimiento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_documento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    remesa_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cuenta_bancaria_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    motivo = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    gastos = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    gastos_repercutidos = table.Column<bool>(type: "boolean", nullable: false),
                    efecto_gastos_id = table.Column<Guid>(type: "uuid", nullable: true),
                    nota = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_devolucion_recibo", x => x.id);
                    table.ForeignKey(
                        name: "FK_devolucion_recibo_cuenta_bancaria_cuenta_bancaria_id",
                        column: x => x.cuenta_bancaria_id,
                        principalSchema: "tesoreria",
                        principalTable: "cuenta_bancaria",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_devolucion_recibo_efecto_cartera_efecto_gastos_id",
                        column: x => x.efecto_gastos_id,
                        principalSchema: "tesoreria",
                        principalTable: "efecto_cartera",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_devolucion_recibo_movimiento_anulacion_movimiento_id",
                        column: x => x.anulacion_movimiento_id,
                        principalSchema: "tesoreria",
                        principalTable: "movimiento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_devolucion_recibo_movimiento_movimiento_id",
                        column: x => x.movimiento_id,
                        principalSchema: "tesoreria",
                        principalTable: "movimiento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_devolucion_recibo_remesa_remesa_id",
                        column: x => x.remesa_id,
                        principalSchema: "tesoreria",
                        principalTable: "remesa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "linea_remesa",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_documento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    documento = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    tercero_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    iban = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: true),
                    mandato = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    mandato_fecha = table.Column<DateOnly>(type: "date", nullable: true),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    movimiento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    viva = table.Column<bool>(type: "boolean", nullable: false),
                    remesa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_remesa", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_remesa_remesa_remesa_id",
                        column: x => x.remesa_id,
                        principalSchema: "tesoreria",
                        principalTable: "remesa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "casacion_apunte",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    movimiento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_documento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    movimiento_creado = table.Column<bool>(type: "boolean", nullable: false),
                    apunte_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_casacion_apunte", x => x.id);
                    table.ForeignKey(
                        name: "FK_casacion_apunte_apunte_bancario_apunte_id",
                        column: x => x.apunte_id,
                        principalSchema: "tesoreria",
                        principalTable: "apunte_bancario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_movimiento_cuenta_bancaria",
                schema: "tesoreria",
                table: "movimiento",
                column: "cuenta_bancaria_id");

            migrationBuilder.CreateIndex(
                name: "ix_apunte_bancario_cuenta_estado",
                schema: "tesoreria",
                table: "apunte_bancario",
                columns: new[] { "cuenta_bancaria_id", "estado" });

            migrationBuilder.CreateIndex(
                name: "ux_apunte_bancario_orden",
                schema: "tesoreria",
                table: "apunte_bancario",
                columns: new[] { "extracto_id", "orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_casacion_apunte_apunte",
                schema: "tesoreria",
                table: "casacion_apunte",
                column: "apunte_id");

            migrationBuilder.CreateIndex(
                name: "ux_casacion_apunte_movimiento",
                schema: "tesoreria",
                table: "casacion_apunte",
                column: "movimiento_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_cuenta_bancaria_iban",
                schema: "tesoreria",
                table: "cuenta_bancaria",
                columns: new[] { "empresa_id", "iban" },
                unique: true,
                filter: "iban IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_cuenta_bancaria_predeterminada",
                schema: "tesoreria",
                table: "cuenta_bancaria",
                column: "empresa_id",
                unique: true,
                filter: "predeterminada");

            migrationBuilder.CreateIndex(
                name: "ux_cuenta_bancaria_subcuenta",
                schema: "tesoreria",
                table: "cuenta_bancaria",
                columns: new[] { "empresa_id", "subcuenta" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_devolucion_recibo_cuenta_bancaria",
                schema: "tesoreria",
                table: "devolucion_recibo",
                column: "cuenta_bancaria_id");

            migrationBuilder.CreateIndex(
                name: "ix_devolucion_recibo_documento",
                schema: "tesoreria",
                table: "devolucion_recibo",
                columns: new[] { "empresa_id", "tipo_documento", "documento_id" });

            migrationBuilder.CreateIndex(
                name: "ix_devolucion_recibo_efecto_gastos",
                schema: "tesoreria",
                table: "devolucion_recibo",
                column: "efecto_gastos_id");

            migrationBuilder.CreateIndex(
                name: "ix_devolucion_recibo_remesa",
                schema: "tesoreria",
                table: "devolucion_recibo",
                column: "remesa_id");

            migrationBuilder.CreateIndex(
                name: "ux_devolucion_recibo_anulacion",
                schema: "tesoreria",
                table: "devolucion_recibo",
                column: "anulacion_movimiento_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_devolucion_recibo_movimiento",
                schema: "tesoreria",
                table: "devolucion_recibo",
                column: "movimiento_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_extracto_bancario_huella",
                schema: "tesoreria",
                table: "extracto_bancario",
                columns: new[] { "cuenta_bancaria_id", "huella" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_linea_remesa_remesa",
                schema: "tesoreria",
                table: "linea_remesa",
                column: "remesa_id");

            migrationBuilder.CreateIndex(
                name: "ux_linea_remesa_documento_viva",
                schema: "tesoreria",
                table: "linea_remesa",
                columns: new[] { "tipo_documento", "documento_id" },
                unique: true,
                filter: "viva");

            migrationBuilder.CreateIndex(
                name: "ux_linea_remesa_movimiento",
                schema: "tesoreria",
                table: "linea_remesa",
                column: "movimiento_id",
                unique: true,
                filter: "movimiento_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_remesa_cuenta_bancaria",
                schema: "tesoreria",
                table: "remesa",
                column: "cuenta_bancaria_id");

            migrationBuilder.CreateIndex(
                name: "ux_remesa_numero",
                schema: "tesoreria",
                table: "remesa",
                columns: new[] { "empresa_id", "tipo", "ejercicio", "numero" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_movimiento_cuenta_bancaria_cuenta_bancaria_id",
                schema: "tesoreria",
                table: "movimiento",
                column: "cuenta_bancaria_id",
                principalSchema: "tesoreria",
                principalTable: "cuenta_bancaria",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // Aislamiento por empresa (RLS forzada) y, en las tablas hijas, heredado de su cabecera.
            migrationBuilder.Sql(GarantiasSql.FuncionesComunes);
            foreach (var tabla in new[] { "cuenta_bancaria", "remesa", "devolucion_recibo", "extracto_bancario", "apunte_bancario" })
            {
                migrationBuilder.Sql(RlsSql.Activar("tesoreria", tabla));
            }

            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("tesoreria", "linea_remesa", "remesa_id", "tesoreria", "remesa"));
            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("tesoreria", "casacion_apunte", "apunte_id", "tesoreria", "apunte_bancario"));

            // Enlaces de las tablas hijas con los movimientos (el modelo no los declara: son tipos propios).
            migrationBuilder.Sql("""
                ALTER TABLE tesoreria.linea_remesa
                    ADD CONSTRAINT fk_linea_remesa_movimiento FOREIGN KEY (movimiento_id) REFERENCES tesoreria.movimiento (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT ck_linea_remesa_importe CHECK (importe > 0),
                    ADD CONSTRAINT ck_linea_remesa_viva CHECK (NOT (viva AND movimiento_id IS NOT NULL));
                ALTER TABLE tesoreria.casacion_apunte
                    ADD CONSTRAINT fk_casacion_apunte_movimiento FOREIGN KEY (movimiento_id) REFERENCES tesoreria.movimiento (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE tesoreria.cuenta_bancaria
                    ADD CONSTRAINT ck_cuenta_bancaria_tipo CHECK (tipo IN ('Banco', 'Caja')),
                    ADD CONSTRAINT ck_cuenta_bancaria_iban CHECK (tipo = 'Caja' OR iban IS NOT NULL),
                    ADD CONSTRAINT ck_cuenta_bancaria_subcuenta CHECK (subcuenta ~ '^57[02][0-9]+$'),
                    ADD CONSTRAINT ck_cuenta_bancaria_predeterminada CHECK (NOT predeterminada OR (activa AND tipo = 'Banco'));
                ALTER TABLE tesoreria.remesa
                    ADD CONSTRAINT ck_remesa_tipo CHECK (tipo IN ('Cobro', 'Pago')),
                    ADD CONSTRAINT ck_remesa_estado CHECK (estado IN ('Generada', 'Presentada', 'Liquidada', 'Anulada')),
                    ADD CONSTRAINT ck_remesa_liquidada CHECK ((estado = 'Liquidada') = (fecha_liquidacion IS NOT NULL));
                ALTER TABLE tesoreria.devolucion_recibo
                    ADD CONSTRAINT ck_devolucion_recibo_gastos CHECK (gastos >= 0 AND importe > 0);
                ALTER TABLE tesoreria.apunte_bancario
                    ADD CONSTRAINT ck_apunte_bancario_estado CHECK (estado IN ('Pendiente', 'ConciliadoAutomatico', 'ConciliadoManual', 'ConAsiento')),
                    ADD CONSTRAINT ck_apunte_bancario_asiento CHECK ((estado = 'ConAsiento') = (cuenta_asiento IS NOT NULL));
                """);
            migrationBuilder.Sql(GarantiasSql.SoloInsercion("tesoreria", "devolucion_recibo", "devolucion.inalterable",
                "Una devolución registrada no se modifica ni se borra: vuelve a cobrar el recibo."));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(GarantiasSql.QuitarTrigger("tesoreria", "devolucion_recibo", "tg_devolucion_recibo_solo_insercion"));
            migrationBuilder.Sql(GarantiasSql.QuitarRlsPorPadre("tesoreria", "casacion_apunte"));
            migrationBuilder.Sql(GarantiasSql.QuitarRlsPorPadre("tesoreria", "linea_remesa"));
            foreach (var tabla in new[] { "cuenta_bancaria", "remesa", "devolucion_recibo", "extracto_bancario", "apunte_bancario" })
            {
                migrationBuilder.Sql(RlsSql.Desactivar("tesoreria", tabla));
            }

            migrationBuilder.DropForeignKey(
                name: "FK_movimiento_cuenta_bancaria_cuenta_bancaria_id",
                schema: "tesoreria",
                table: "movimiento");

            migrationBuilder.DropTable(
                name: "casacion_apunte",
                schema: "tesoreria");

            migrationBuilder.DropTable(
                name: "devolucion_recibo",
                schema: "tesoreria");

            migrationBuilder.DropTable(
                name: "linea_remesa",
                schema: "tesoreria");

            migrationBuilder.DropTable(
                name: "apunte_bancario",
                schema: "tesoreria");

            migrationBuilder.DropTable(
                name: "remesa",
                schema: "tesoreria");

            migrationBuilder.DropTable(
                name: "extracto_bancario",
                schema: "tesoreria");

            migrationBuilder.DropTable(
                name: "cuenta_bancaria",
                schema: "tesoreria");

            migrationBuilder.DropIndex(
                name: "ix_movimiento_cuenta_bancaria",
                schema: "tesoreria",
                table: "movimiento");

            migrationBuilder.DropColumn(
                name: "cuenta_bancaria_id",
                schema: "tesoreria",
                table: "movimiento");
        }
    }
}
