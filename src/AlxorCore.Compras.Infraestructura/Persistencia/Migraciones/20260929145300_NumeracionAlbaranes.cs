using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Compras.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class NumeracionAlbaranes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Numeración de los albaranes de compra: un número por empresa y año, nunca repetido (antes era MAX+1 sin índice).
            migrationBuilder.Sql("""
                DO $d$
                BEGIN
                    IF EXISTS (SELECT 1 FROM compras.albaran_compra GROUP BY empresa_id, extract(year FROM fecha), numero HAVING count(*) > 1) THEN
                        RAISE EXCEPTION 'Hay albaranes de compra con el número repetido en el mismo año: corrígelos antes de migrar.';
                    END IF;
                END $d$;
                CREATE UNIQUE INDEX ux_albaran_compra_numero ON compras.albaran_compra (empresa_id, (extract(year FROM fecha)), numero);
                ALTER TABLE compras.albaran_compra ADD CONSTRAINT ck_albaran_compra_numero CHECK (numero > 0);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE compras.albaran_compra DROP CONSTRAINT IF EXISTS ck_albaran_compra_numero;
                DROP INDEX IF EXISTS compras.ux_albaran_compra_numero;
                """);
        }
    }
}
