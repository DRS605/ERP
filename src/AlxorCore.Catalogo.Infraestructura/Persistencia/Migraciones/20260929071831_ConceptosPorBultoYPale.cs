using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Catalogo.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ConceptosPorBultoYPale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE catalogo.concepto_linea DROP CONSTRAINT ck_concepto_linea_calculo;
                ALTER TABLE catalogo.concepto_linea ADD CONSTRAINT ck_concepto_linea_calculo
                    CHECK (calculo IN ('Porcentaje', 'PorUnidad', 'PorKilo', 'Importe', 'PorBulto', 'PorPale'));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE catalogo.concepto_linea DROP CONSTRAINT ck_concepto_linea_calculo;
                ALTER TABLE catalogo.concepto_linea ADD CONSTRAINT ck_concepto_linea_calculo
                    CHECK (calculo IN ('Porcentaje', 'PorUnidad', 'PorKilo', 'Importe'));
                """);
        }
    }
}
