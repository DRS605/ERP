using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Catalogo.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class SuplidosCompras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // El suplido también vale en compras (tasas o aranceles que paga el proveedor por la empresa).
            migrationBuilder.Sql("""
                ALTER TABLE catalogo.concepto_linea DROP CONSTRAINT ck_concepto_linea_efecto;
                ALTER TABLE catalogo.concepto_linea ADD CONSTRAINT ck_concepto_linea_efecto CHECK (efecto IN ('Precio', 'Coste', 'Suplido')
                    AND (efecto <> 'Suplido' OR (sentido = 'Suma' AND cuenta_contable IS NOT NULL)));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE catalogo.concepto_linea DROP CONSTRAINT ck_concepto_linea_efecto;
                ALTER TABLE catalogo.concepto_linea ADD CONSTRAINT ck_concepto_linea_efecto CHECK (efecto IN ('Precio', 'Coste', 'Suplido')
                    AND (efecto <> 'Suplido' OR (ambito = 'Ventas' AND sentido = 'Suma' AND cuenta_contable IS NOT NULL)));
                """);
        }
    }
}
