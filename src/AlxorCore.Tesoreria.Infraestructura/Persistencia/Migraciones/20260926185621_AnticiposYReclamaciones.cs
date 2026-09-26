using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Tesoreria.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AnticiposYReclamaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "anticipo",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    concepto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    metodo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_anticipo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "configuracion_reclamaciones",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    niveles = table.Column<string>(type: "jsonb", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracion_reclamaciones", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reclamacion",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    factura_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nivel = table.Column<int>(type: "integer", nullable: false),
                    canal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    pendiente = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    dias_retraso = table.Column<int>(type: "integer", nullable: false),
                    nota = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    realizada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reclamacion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "aplicacion_anticipo",
                schema: "tesoreria",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    factura_id = table.Column<Guid>(type: "uuid", nullable: false),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    movimiento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    anticipo_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aplicacion_anticipo", x => x.id);
                    table.ForeignKey(
                        name: "FK_aplicacion_anticipo_anticipo_anticipo_id",
                        column: x => x.anticipo_id,
                        principalSchema: "tesoreria",
                        principalTable: "anticipo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_anticipo_empresa_cliente",
                schema: "tesoreria",
                table: "anticipo",
                columns: new[] { "empresa_id", "cliente_id" });

            migrationBuilder.CreateIndex(
                name: "ix_aplicacion_anticipo_anticipo",
                schema: "tesoreria",
                table: "aplicacion_anticipo",
                column: "anticipo_id");

            migrationBuilder.CreateIndex(
                name: "ux_aplicacion_anticipo_movimiento",
                schema: "tesoreria",
                table: "aplicacion_anticipo",
                column: "movimiento_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_configuracion_reclamaciones_empresa",
                schema: "tesoreria",
                table: "configuracion_reclamaciones",
                column: "empresa_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_reclamacion_empresa_factura",
                schema: "tesoreria",
                table: "reclamacion",
                columns: new[] { "empresa_id", "factura_id" });

            // Aislamiento por empresa (RLS) y reglas básicas también en la base de datos.
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("tesoreria", "anticipo"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("tesoreria", "reclamacion"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("tesoreria", "configuracion_reclamaciones"));
            migrationBuilder.Sql("""
                ALTER TABLE tesoreria.anticipo ADD CONSTRAINT ck_anticipo_importe CHECK (importe > 0);
                ALTER TABLE tesoreria.aplicacion_anticipo ADD CONSTRAINT ck_aplicacion_anticipo_importe CHECK (importe > 0);
                ALTER TABLE tesoreria.reclamacion ADD CONSTRAINT ck_reclamacion_nivel CHECK (nivel >= 1),
                    ADD CONSTRAINT ck_reclamacion_pendiente CHECK (pendiente > 0);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aplicacion_anticipo",
                schema: "tesoreria");

            migrationBuilder.DropTable(
                name: "configuracion_reclamaciones",
                schema: "tesoreria");

            migrationBuilder.DropTable(
                name: "reclamacion",
                schema: "tesoreria");

            migrationBuilder.DropTable(
                name: "anticipo",
                schema: "tesoreria");
        }
    }
}
