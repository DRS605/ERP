using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Contabilidad.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class MigracionInicialContabilidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "contabilidad");

            migrationBuilder.CreateTable(
                name: "asiento",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    concepto = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    origen = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asiento", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "config_contabilidad",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    modo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_config_contabilidad", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cuenta",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    grupo = table.Column<int>(type: "integer", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cuenta", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "apunte",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cuenta_codigo = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    concepto = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    debe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    haber = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    asiento_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apunte", x => x.id);
                    table.ForeignKey(
                        name: "FK_apunte_asiento_asiento_id",
                        column: x => x.asiento_id,
                        principalSchema: "contabilidad",
                        principalTable: "asiento",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_apunte_asiento_id",
                schema: "contabilidad",
                table: "apunte",
                column: "asiento_id");

            migrationBuilder.CreateIndex(
                name: "ux_asiento_empresa_ejercicio_numero",
                schema: "contabilidad",
                table: "asiento",
                columns: new[] { "empresa_id", "ejercicio", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_cuenta_empresa_codigo",
                schema: "contabilidad",
                table: "cuenta",
                columns: new[] { "empresa_id", "codigo" },
                unique: true);

            // Row-Level Security por empresa (el apunte se protege a través de su asiento).
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("contabilidad", "cuenta"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("contabilidad", "asiento"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("contabilidad", "config_contabilidad"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Desactivar("contabilidad", "config_contabilidad"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Desactivar("contabilidad", "asiento"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Desactivar("contabilidad", "cuenta"));

            migrationBuilder.DropTable(
                name: "apunte",
                schema: "contabilidad");

            migrationBuilder.DropTable(
                name: "config_contabilidad",
                schema: "contabilidad");

            migrationBuilder.DropTable(
                name: "cuenta",
                schema: "contabilidad");

            migrationBuilder.DropTable(
                name: "asiento",
                schema: "contabilidad");
        }
    }
}
