using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Facturacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class NumeracionAlbaranes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Numeración de los albaranes de venta: un número por empresa y año, nunca repetido (antes era MAX+1 sin índice).
            migrationBuilder.Sql("""
                DO $d$
                BEGIN
                    IF EXISTS (SELECT 1 FROM facturacion.albaran_venta GROUP BY empresa_id, extract(year FROM fecha), numero HAVING count(*) > 1) THEN
                        RAISE EXCEPTION 'Hay albaranes de venta con el número repetido en el mismo año: corrígelos antes de migrar.';
                    END IF;
                END $d$;
                CREATE UNIQUE INDEX ux_albaran_venta_numero ON facturacion.albaran_venta (empresa_id, (extract(year FROM fecha)), numero);
                ALTER TABLE facturacion.albaran_venta ADD CONSTRAINT ck_albaran_venta_numero CHECK (numero > 0);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE facturacion.albaran_venta DROP CONSTRAINT IF EXISTS ck_albaran_venta_numero;
                DROP INDEX IF EXISTS facturacion.ux_albaran_venta_numero;
                """);
        }
    }
}
