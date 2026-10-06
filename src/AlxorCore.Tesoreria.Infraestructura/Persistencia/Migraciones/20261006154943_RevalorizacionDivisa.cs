using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Tesoreria.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class RevalorizacionDivisa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "revalorizacion_divisa",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    anulada = table.Column<bool>(type: "boolean", nullable: false),
                    creada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_revalorizacion_divisa", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "linea_revalorizacion_divisa",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_documento = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    referencia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tercero_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tercero_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    pendiente_divisa = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    valor_libros = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    tasa_cierre = table.Column<decimal>(type: "numeric(18,8)", nullable: false),
                    valor_cierre = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    revalorizacion_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_revalorizacion_divisa", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_revalorizacion_divisa_revalorizacion_divisa_revaloriz~",
                        column: x => x.revalorizacion_id,
                        principalSchema: "tesoreria",
                        principalTable: "revalorizacion_divisa",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_linea_revalorizacion_divisa_revalorizacion_id",
                schema: "tesoreria",
                table: "linea_revalorizacion_divisa",
                column: "revalorizacion_id");

            migrationBuilder.CreateIndex(
                name: "ix_linea_revalorizacion_documento",
                schema: "tesoreria",
                table: "linea_revalorizacion_divisa",
                column: "documento_id");

            migrationBuilder.CreateIndex(
                name: "ix_revalorizacion_divisa_ejercicio",
                schema: "tesoreria",
                table: "revalorizacion_divisa",
                columns: new[] { "empresa_id", "ejercicio" });
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("tesoreria", "revalorizacion_divisa"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("tesoreria", "linea_revalorizacion_divisa", "revalorizacion_id", "tesoreria", "revalorizacion_divisa"));
            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX ux_revalorizacion_divisa_vigente ON tesoreria.revalorizacion_divisa (empresa_id, ejercicio) WHERE NOT anulada;
                ALTER TABLE tesoreria.linea_revalorizacion_divisa ADD CONSTRAINT ck_linea_revalorizacion CHECK (
                    tipo_documento IN ('Factura', 'Gasto') AND pendiente_divisa > 0 AND valor_libros > 0 AND tasa_cierre > 0 AND valor_cierre >= 0);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.QuitarRlsPorPadre("tesoreria", "linea_revalorizacion_divisa"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Desactivar("tesoreria", "revalorizacion_divisa"));
            migrationBuilder.DropTable(
                name: "linea_revalorizacion_divisa",
                schema: "tesoreria");

            migrationBuilder.DropTable(
                name: "revalorizacion_divisa",
                schema: "tesoreria");
        }
    }
}
