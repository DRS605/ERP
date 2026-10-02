using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Organizacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ProrrataPorImpuesto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_prorrata_empresa_ejercicio",
                schema: "organizacion",
                table: "prorrata_ejercicio");

            migrationBuilder.AddColumn<string>(
                name: "impuesto",
                schema: "organizacion",
                table: "prorrata_ejercicio",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Iva");

            // Las prorratas ya fijadas en empresas de Canarias son del IGIC. La RLS por empresa se suspende solo para este cambio.
            migrationBuilder.Sql("""
                ALTER TABLE organizacion.prorrata_ejercicio NO FORCE ROW LEVEL SECURITY;
                ALTER TABLE organizacion.prorrata_ejercicio DISABLE ROW LEVEL SECURITY;
                UPDATE organizacion.prorrata_ejercicio p SET impuesto = 'Igic'
                  FROM organizacion.empresa e WHERE e.id = p.empresa_id AND e.territorio_fiscal = 'Canarias';
                ALTER TABLE organizacion.prorrata_ejercicio ENABLE ROW LEVEL SECURITY;
                ALTER TABLE organizacion.prorrata_ejercicio FORCE ROW LEVEL SECURITY;
                """);

            migrationBuilder.CreateIndex(
                name: "ux_prorrata_empresa_ejercicio_impuesto",
                schema: "organizacion",
                table: "prorrata_ejercicio",
                columns: new[] { "empresa_id", "ejercicio", "impuesto" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_prorrata_empresa_ejercicio_impuesto",
                schema: "organizacion",
                table: "prorrata_ejercicio");

            migrationBuilder.DropColumn(
                name: "impuesto",
                schema: "organizacion",
                table: "prorrata_ejercicio");

            migrationBuilder.CreateIndex(
                name: "ux_prorrata_empresa_ejercicio",
                schema: "organizacion",
                table: "prorrata_ejercicio",
                columns: new[] { "empresa_id", "ejercicio" },
                unique: true);
        }
    }
}
