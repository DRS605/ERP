using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Agro.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class Subastas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sesion_subasta",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    creada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    cerrada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    motivo_anulacion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sesion_subasta", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lote_subasta",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    partida_id = table.Column<Guid>(type: "uuid", nullable: false),
                    partida_codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    agricultor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    kilos = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    envases = table.Column<int>(type: "integer", nullable: false),
                    precio_salida = table.Column<decimal>(type: "numeric(12,6)", nullable: true),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    comprador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    comprador_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    precio_kg = table.Column<decimal>(type: "numeric(12,6)", nullable: true),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    adjudicado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    albaran_id = table.Column<Guid>(type: "uuid", nullable: true),
                    albaran_numero = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    sesion_subasta_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lote_subasta", x => x.id);
                    table.ForeignKey(
                        name: "FK_lote_subasta_sesion_subasta_sesion_subasta_id",
                        column: x => x.sesion_subasta_id,
                        principalSchema: "agro",
                        principalTable: "sesion_subasta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "puja_subasta",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lote_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comprador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comprador_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    precio_kg = table.Column<decimal>(type: "numeric(12,6)", nullable: false),
                    creada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    sesion_subasta_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_puja_subasta", x => x.id);
                    table.ForeignKey(
                        name: "FK_puja_subasta_sesion_subasta_sesion_subasta_id",
                        column: x => x.sesion_subasta_id,
                        principalSchema: "agro",
                        principalTable: "sesion_subasta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_lote_subasta_comprador",
                schema: "agro",
                table: "lote_subasta",
                column: "comprador_id");

            migrationBuilder.CreateIndex(
                name: "ix_lote_subasta_partida",
                schema: "agro",
                table: "lote_subasta",
                column: "partida_id");

            migrationBuilder.CreateIndex(
                name: "ux_lote_subasta_orden",
                schema: "agro",
                table: "lote_subasta",
                columns: new[] { "sesion_subasta_id", "orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_puja_subasta_sesion_subasta_id",
                schema: "agro",
                table: "puja_subasta",
                column: "sesion_subasta_id");

            migrationBuilder.CreateIndex(
                name: "ix_puja_subasta_lote",
                schema: "agro",
                table: "puja_subasta",
                column: "lote_id");

            migrationBuilder.CreateIndex(
                name: "ix_sesion_subasta_fecha",
                schema: "agro",
                table: "sesion_subasta",
                columns: new[] { "empresa_id", "fecha" });

            migrationBuilder.CreateIndex(
                name: "ux_sesion_subasta_numero",
                schema: "agro",
                table: "sesion_subasta",
                columns: new[] { "empresa_id", "ejercicio", "numero" },
                unique: true);

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("agro", "sesion_subasta"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("agro", "lote_subasta", "sesion_subasta_id", "agro", "sesion_subasta"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("agro", "puja_subasta", "sesion_subasta_id", "agro", "sesion_subasta"));
            migrationBuilder.Sql("""
                ALTER TABLE agro.sesion_subasta ADD CONSTRAINT ck_sesion_subasta_tipo CHECK (tipo IN ('Baja','Alza'));
                ALTER TABLE agro.sesion_subasta ADD CONSTRAINT ck_sesion_subasta_estado CHECK (estado IN ('Abierta','Cerrada','Anulada'));
                ALTER TABLE agro.lote_subasta ADD CONSTRAINT ck_lote_subasta_estado CHECK (estado IN ('Pendiente','Adjudicado','Desierto'));
                ALTER TABLE agro.lote_subasta ADD CONSTRAINT ck_lote_subasta_kilos CHECK (kilos > 0 AND envases >= 0 AND (precio_salida IS NULL OR precio_salida >= 0));
                -- Adjudicado: con comprador, precio positivo e importe; si no, sin ellos.
                ALTER TABLE agro.lote_subasta ADD CONSTRAINT ck_lote_subasta_adjudicacion CHECK (
                    (estado = 'Adjudicado' AND comprador_id IS NOT NULL AND precio_kg > 0 AND importe = round(kilos * precio_kg, 2))
                    OR (estado <> 'Adjudicado' AND comprador_id IS NULL AND precio_kg IS NULL AND importe = 0 AND albaran_id IS NULL));
                ALTER TABLE agro.puja_subasta ADD CONSTRAINT ck_puja_subasta_precio CHECK (precio_kg > 0);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.QuitarRlsPorPadre("agro", "lote_subasta"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.QuitarRlsPorPadre("agro", "puja_subasta"));
            migrationBuilder.DropTable(
                name: "lote_subasta",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "puja_subasta",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "sesion_subasta",
                schema: "agro");
        }
    }
}
