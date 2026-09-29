using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Agro.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class LaboresCuaderno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "fosforo_kg_ha",
                schema: "agro",
                table: "tratamiento_parcela",
                type: "numeric(10,3)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "nitrogeno_kg_ha",
                schema: "agro",
                table: "tratamiento_parcela",
                type: "numeric(10,3)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "potasio_kg_ha",
                schema: "agro",
                table: "tratamiento_parcela",
                type: "numeric(10,3)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tipo",
                schema: "agro",
                table: "tratamiento_parcela",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Fitosanitario");

            migrationBuilder.AddColumn<decimal>(
                name: "volumen_m3",
                schema: "agro",
                table: "tratamiento_parcela",
                type: "numeric(12,3)",
                nullable: true);
            migrationBuilder.Sql("""
                ALTER TABLE agro.tratamiento_parcela ADD CONSTRAINT ck_tratamiento_parcela_tipo CHECK (
                    tipo IN ('Fitosanitario', 'Abonado', 'Riego', 'Otra') AND (tipo = 'Fitosanitario' OR plazo_seguridad_dias = 0)
                    AND coalesce(nitrogeno_kg_ha, 0) >= 0 AND coalesce(fosforo_kg_ha, 0) >= 0 AND coalesce(potasio_kg_ha, 0) >= 0 AND coalesce(volumen_m3, 0) >= 0);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE agro.tratamiento_parcela DROP CONSTRAINT IF EXISTS ck_tratamiento_parcela_tipo;");
            migrationBuilder.DropColumn(
                name: "fosforo_kg_ha",
                schema: "agro",
                table: "tratamiento_parcela");

            migrationBuilder.DropColumn(
                name: "nitrogeno_kg_ha",
                schema: "agro",
                table: "tratamiento_parcela");

            migrationBuilder.DropColumn(
                name: "potasio_kg_ha",
                schema: "agro",
                table: "tratamiento_parcela");

            migrationBuilder.DropColumn(
                name: "tipo",
                schema: "agro",
                table: "tratamiento_parcela");

            migrationBuilder.DropColumn(
                name: "volumen_m3",
                schema: "agro",
                table: "tratamiento_parcela");
        }
    }
}
