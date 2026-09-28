using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Compras.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class OrdenLineasCompras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "orden",
                schema: "compras",
                table: "linea_solicitud",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "orden",
                schema: "compras",
                table: "linea_pedido",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "orden",
                schema: "compras",
                table: "linea_albaran",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Las líneas existentes reciben su número en el orden en que se guardaron (empresa a empresa por la RLS).
            migrationBuilder.Sql("""
                DO $m$
                DECLARE e record;
                BEGIN
                    IF to_regclass('organizacion.empresa') IS NULL THEN RETURN; END IF;
                    FOR e IN SELECT id FROM organizacion.empresa LOOP
                        PERFORM set_config('app.empresa_actual', e.id::text, true);
                        UPDATE compras.linea_pedido l SET orden = n.orden
                        FROM (SELECT id, row_number() OVER (PARTITION BY pedido_id ORDER BY ctid) AS orden FROM compras.linea_pedido WHERE orden = 0) n
                        WHERE l.id = n.id;
                        UPDATE compras.linea_albaran l SET orden = n.orden
                        FROM (SELECT id, row_number() OVER (PARTITION BY albaran_id ORDER BY ctid) AS orden FROM compras.linea_albaran WHERE orden = 0) n
                        WHERE l.id = n.id;
                        UPDATE compras.linea_solicitud l SET orden = n.orden
                        FROM (SELECT id, row_number() OVER (PARTITION BY solicitud_id ORDER BY ctid) AS orden FROM compras.linea_solicitud WHERE orden = 0) n
                        WHERE l.id = n.id;
                    END LOOP;
                    PERFORM set_config('app.empresa_actual', '', true);
                END $m$;
                CREATE INDEX IF NOT EXISTS ix_linea_pedido_orden ON compras.linea_pedido (pedido_id, orden);
                CREATE INDEX IF NOT EXISTS ix_linea_albaran_orden ON compras.linea_albaran (albaran_id, orden);
                CREATE INDEX IF NOT EXISTS ix_linea_solicitud_orden ON compras.linea_solicitud (solicitud_id, orden);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS compras.ix_linea_pedido_orden;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS compras.ix_linea_albaran_orden;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS compras.ix_linea_solicitud_orden;");

            migrationBuilder.DropColumn(
                name: "orden",
                schema: "compras",
                table: "linea_solicitud");

            migrationBuilder.DropColumn(
                name: "orden",
                schema: "compras",
                table: "linea_pedido");

            migrationBuilder.DropColumn(
                name: "orden",
                schema: "compras",
                table: "linea_albaran");
        }
    }
}
