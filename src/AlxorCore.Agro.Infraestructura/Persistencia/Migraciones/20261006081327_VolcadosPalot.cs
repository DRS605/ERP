using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Agro.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class VolcadosPalot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "volcado_palot",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    linea_id = table.Column<Guid>(type: "uuid", nullable: false),
                    orden_linea_id = table.Column<Guid>(type: "uuid", nullable: true),
                    pale_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sscc = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: false),
                    kilos = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    volcado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    registrado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    clave_terminal = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    terminal = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    parte_confeccion_id = table.Column<Guid>(type: "uuid", nullable: true),
                    motivo_anulacion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_volcado_palot", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_volcado_palot_fecha",
                schema: "agro",
                table: "volcado_palot",
                columns: new[] { "empresa_id", "fecha" });

            migrationBuilder.CreateIndex(
                name: "ix_volcado_palot_linea",
                schema: "agro",
                table: "volcado_palot",
                column: "linea_id");

            migrationBuilder.CreateIndex(
                name: "ix_volcado_palot_orden",
                schema: "agro",
                table: "volcado_palot",
                column: "orden_linea_id");

            migrationBuilder.CreateIndex(
                name: "ix_volcado_palot_pale",
                schema: "agro",
                table: "volcado_palot",
                column: "pale_id");

            migrationBuilder.CreateIndex(
                name: "ix_volcado_palot_parte",
                schema: "agro",
                table: "volcado_palot",
                column: "parte_confeccion_id");

            migrationBuilder.CreateIndex(
                name: "ux_volcado_palot_clave",
                schema: "agro",
                table: "volcado_palot",
                columns: new[] { "empresa_id", "clave_terminal" },
                unique: true);

            migrationBuilder.Sql(RlsSql.Activar("agro", "volcado_palot"));
            migrationBuilder.Sql("""
                ALTER TABLE agro.volcado_palot
                    ADD CONSTRAINT ck_volcado_palot_valores CHECK (kilos > 0 AND length(trim(clave_terminal)) > 0 AND sscc ~ '^[0-9]{18}$'
                        AND estado IN ('Registrado', 'EnParte', 'Anulado') AND ((estado = 'EnParte') = (parte_confeccion_id IS NOT NULL))
                        AND ((estado = 'Anulado') = (motivo_anulacion IS NOT NULL))),
                    ADD CONSTRAINT fk_volcado_palot_linea FOREIGN KEY (linea_id) REFERENCES agro.linea_planta (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_volcado_palot_pale FOREIGN KEY (pale_id) REFERENCES agro.pale (id) DEFERRABLE INITIALLY DEFERRED;
                CREATE UNIQUE INDEX ux_volcado_palot_pendiente ON agro.volcado_palot (pale_id) WHERE estado = 'Registrado';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS agro.ux_volcado_palot_pendiente;");
            migrationBuilder.Sql(RlsSql.Desactivar("agro", "volcado_palot"));

            migrationBuilder.DropTable(
                name: "volcado_palot",
                schema: "agro");
        }
    }
}
