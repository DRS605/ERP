using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Agro.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class EnvasesTercerosYReservas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "envase_producto_id",
                schema: "agro",
                table: "plantilla_pale",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "pale_producto_id",
                schema: "agro",
                table: "plantilla_pale",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "cuenta_envases",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    tercero_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    agrupadora_id = table.Column<Guid>(type: "uuid", nullable: true),
                    imputar_a_transportista = table.Column<bool>(type: "boolean", nullable: false),
                    bloqueo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    motivo_bloqueo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    limite = table.Column<int>(type: "integer", nullable: true),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    creada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cuenta_envases", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "movimiento_envases",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    cuenta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cuenta_solicitada_id = table.Column<Guid>(type: "uuid", nullable: false),
                    origen = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    transportista_id = table.Column<Guid>(type: "uuid", nullable: true),
                    matricula = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    observaciones = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    anula_movimiento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimiento_envases", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reserva_pale",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pale_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pedido_venta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    linea_pedido_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kilos = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    cajas = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    creada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    consumida_el = table.Column<DateOnly>(type: "date", nullable: true),
                    motivo_anulacion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reserva_pale", x => x.id);
                    table.ForeignKey(
                        name: "FK_reserva_pale_pale_pale_id",
                        column: x => x.pale_id,
                        principalSchema: "agro",
                        principalTable: "pale",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "linea_movimiento_envases",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    envase_producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    movimiento_envases_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_movimiento_envases", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_movimiento_envases_movimiento_envases_movimiento_enva~",
                        column: x => x.movimiento_envases_id,
                        principalSchema: "agro",
                        principalTable: "movimiento_envases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_cuenta_envases_tercero",
                schema: "agro",
                table: "cuenta_envases",
                columns: new[] { "empresa_id", "tipo", "tercero_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_linea_movimiento_envases_movimiento_envases_id",
                schema: "agro",
                table: "linea_movimiento_envases",
                column: "movimiento_envases_id");

            migrationBuilder.CreateIndex(
                name: "ix_linea_movimiento_envases_envase",
                schema: "agro",
                table: "linea_movimiento_envases",
                column: "envase_producto_id");

            migrationBuilder.CreateIndex(
                name: "ix_movimiento_envases_cuenta",
                schema: "agro",
                table: "movimiento_envases",
                columns: new[] { "cuenta_id", "fecha" });

            migrationBuilder.CreateIndex(
                name: "ix_movimiento_envases_documento",
                schema: "agro",
                table: "movimiento_envases",
                column: "documento_id");

            migrationBuilder.CreateIndex(
                name: "ux_movimiento_envases_anula",
                schema: "agro",
                table: "movimiento_envases",
                column: "anula_movimiento_id",
                unique: true,
                filter: "anula_movimiento_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_movimiento_envases_numero",
                schema: "agro",
                table: "movimiento_envases",
                columns: new[] { "empresa_id", "ejercicio", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_reserva_pale_pedido",
                schema: "agro",
                table: "reserva_pale",
                column: "pedido_venta_id");

            migrationBuilder.CreateIndex(
                name: "ux_reserva_pale_activa",
                schema: "agro",
                table: "reserva_pale",
                column: "pale_id",
                unique: true,
                filter: "estado = 'Activa'");

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("agro", "cuenta_envases"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("agro", "movimiento_envases"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("agro", "linea_movimiento_envases", "movimiento_envases_id", "agro", "movimiento_envases"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.SoloInsercion("agro", "movimiento_envases", "envases.movimiento_inmutable",
                "Los movimientos de envases no se modifican ni se borran: se anulan con su contrario."));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.SoloInsercion("agro", "linea_movimiento_envases", "envases.movimiento_inmutable",
                "Los movimientos de envases no se modifican ni se borran: se anulan con su contrario."));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("agro", "reserva_pale"));
            migrationBuilder.Sql("""
                ALTER TABLE agro.linea_movimiento_envases ADD CONSTRAINT ck_linea_movimiento_envases_cantidad CHECK (cantidad <> 0);
                ALTER TABLE agro.cuenta_envases ADD CONSTRAINT ck_cuenta_envases_limite CHECK (limite IS NULL OR limite >= 0);
                ALTER TABLE agro.reserva_pale ADD CONSTRAINT ck_reserva_pale_estado CHECK (estado IN ('Activa', 'Consumida', 'Anulada'));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cuenta_envases",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "linea_movimiento_envases",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "reserva_pale",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "movimiento_envases",
                schema: "agro");

            migrationBuilder.DropColumn(
                name: "envase_producto_id",
                schema: "agro",
                table: "plantilla_pale");

            migrationBuilder.DropColumn(
                name: "pale_producto_id",
                schema: "agro",
                table: "plantilla_pale");
        }
    }
}
