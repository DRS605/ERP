using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Agro.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class CuadernoCampo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tratamiento_parcela",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parcela_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    producto = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    numero_registro = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    materia_activa = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    motivo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    dosis = table.Column<decimal>(type: "numeric(12,4)", nullable: true),
                    unidad_dosis = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    superficie_tratada_ha = table.Column<decimal>(type: "numeric(10,4)", nullable: true),
                    plazo_seguridad_dias = table.Column<int>(type: "integer", nullable: false),
                    aplicador = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    observaciones = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    anulado = table.Column<bool>(type: "boolean", nullable: false),
                    motivo_anulacion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tratamiento_parcela", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tratamiento_parcela_fecha",
                schema: "agro",
                table: "tratamiento_parcela",
                columns: new[] { "parcela_id", "fecha" });
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("agro", "tratamiento_parcela"));
            migrationBuilder.Sql("""
                ALTER TABLE agro.tratamiento_parcela
                    ADD CONSTRAINT fk_tratamiento_parcela_parcela FOREIGN KEY (parcela_id) REFERENCES agro.parcela (id),
                    ADD CONSTRAINT ck_tratamiento_parcela_valores CHECK (plazo_seguridad_dias BETWEEN 0 AND 365 AND (dosis IS NULL OR dosis >= 0)
                        AND (superficie_tratada_ha IS NULL OR superficie_tratada_ha >= 0) AND anulado = (motivo_anulacion IS NOT NULL));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tratamiento_parcela",
                schema: "agro");
        }
    }
}
