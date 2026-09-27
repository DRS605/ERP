using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Gastos.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class DuaImportacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dua_importacion",
                schema: "gastos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    gasto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mrn = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: false),
                    fecha_admision = table.Column<DateOnly>(type: "date", nullable: false),
                    aduana = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    base_iva = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    aranceles = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    cuota_iva = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    observaciones = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dua_importacion", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_dua_importacion_gasto",
                schema: "gastos",
                table: "dua_importacion",
                column: "gasto_id");

            migrationBuilder.CreateIndex(
                name: "ux_dua_importacion_empresa_mrn",
                schema: "gastos",
                table: "dua_importacion",
                columns: new[] { "empresa_id", "mrn" },
                unique: true);

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("gastos", "dua_importacion"));
            migrationBuilder.Sql("""
                ALTER TABLE gastos.dua_importacion
                    ADD CONSTRAINT fk_dua_importacion_gasto FOREIGN KEY (gasto_id) REFERENCES gastos.gasto (id),
                    ADD CONSTRAINT ck_dua_importacion_mrn CHECK (mrn ~ '^[0-9]{2}[A-Z]{2}[A-Z0-9]{14}$'),
                    ADD CONSTRAINT ck_dua_importacion_importes CHECK (base_iva >= 0 AND aranceles >= 0 AND cuota_iva >= 0);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE gastos.dua_importacion DROP CONSTRAINT IF EXISTS fk_dua_importacion_gasto;");

            migrationBuilder.DropTable(
                name: "dua_importacion",
                schema: "gastos");
        }
    }
}
