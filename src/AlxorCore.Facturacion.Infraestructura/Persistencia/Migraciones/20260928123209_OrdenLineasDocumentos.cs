using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Facturacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class OrdenLineasDocumentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "orden",
                schema: "facturacion",
                table: "linea_recurrente",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "orden",
                schema: "facturacion",
                table: "linea_presupuesto",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "orden",
                schema: "facturacion",
                table: "linea_pedido_venta",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "orden",
                schema: "facturacion",
                table: "linea_carta_porte",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "orden",
                schema: "facturacion",
                table: "linea_albaran_venta",
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
                        UPDATE facturacion.linea_presupuesto l SET orden = n.orden
                        FROM (SELECT id, row_number() OVER (PARTITION BY presupuesto_id ORDER BY ctid) AS orden FROM facturacion.linea_presupuesto WHERE orden = 0) n
                        WHERE l.id = n.id;
                        UPDATE facturacion.linea_pedido_venta l SET orden = n.orden
                        FROM (SELECT id, row_number() OVER (PARTITION BY pedido_venta_id ORDER BY ctid) AS orden FROM facturacion.linea_pedido_venta WHERE orden = 0) n
                        WHERE l.id = n.id;
                        UPDATE facturacion.linea_albaran_venta l SET orden = n.orden
                        FROM (SELECT id, row_number() OVER (PARTITION BY albaran_venta_id ORDER BY ctid) AS orden FROM facturacion.linea_albaran_venta WHERE orden = 0) n
                        WHERE l.id = n.id;
                        UPDATE facturacion.linea_recurrente l SET orden = n.orden
                        FROM (SELECT id, row_number() OVER (PARTITION BY factura_recurrente_id ORDER BY ctid) AS orden FROM facturacion.linea_recurrente WHERE orden = 0) n
                        WHERE l.id = n.id;
                        UPDATE facturacion.linea_carta_porte l SET orden = n.orden
                        FROM (SELECT id, row_number() OVER (PARTITION BY carta_porte_id ORDER BY ctid) AS orden FROM facturacion.linea_carta_porte WHERE orden = 0) n
                        WHERE l.id = n.id;
                    END LOOP;
                    PERFORM set_config('app.empresa_actual', '', true);
                END $m$;
                CREATE INDEX IF NOT EXISTS ix_linea_presupuesto_orden ON facturacion.linea_presupuesto (presupuesto_id, orden);
                CREATE INDEX IF NOT EXISTS ix_linea_pedido_venta_orden ON facturacion.linea_pedido_venta (pedido_venta_id, orden);
                CREATE INDEX IF NOT EXISTS ix_linea_albaran_venta_orden ON facturacion.linea_albaran_venta (albaran_venta_id, orden);
                CREATE INDEX IF NOT EXISTS ix_linea_recurrente_orden ON facturacion.linea_recurrente (factura_recurrente_id, orden);
                CREATE INDEX IF NOT EXISTS ix_linea_carta_porte_orden ON facturacion.linea_carta_porte (carta_porte_id, orden);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS facturacion.ix_linea_presupuesto_orden;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS facturacion.ix_linea_pedido_venta_orden;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS facturacion.ix_linea_albaran_venta_orden;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS facturacion.ix_linea_recurrente_orden;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS facturacion.ix_linea_carta_porte_orden;");

            migrationBuilder.DropColumn(
                name: "orden",
                schema: "facturacion",
                table: "linea_recurrente");

            migrationBuilder.DropColumn(
                name: "orden",
                schema: "facturacion",
                table: "linea_presupuesto");

            migrationBuilder.DropColumn(
                name: "orden",
                schema: "facturacion",
                table: "linea_pedido_venta");

            migrationBuilder.DropColumn(
                name: "orden",
                schema: "facturacion",
                table: "linea_carta_porte");

            migrationBuilder.DropColumn(
                name: "orden",
                schema: "facturacion",
                table: "linea_albaran_venta");
        }
    }
}
