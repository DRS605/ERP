using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Facturacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class OrdenLineasFactura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "orden",
                schema: "facturacion",
                table: "linea_factura",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Las líneas ya emitidas reciben su número en el orden en que se guardaron (la tabla es de solo inserción:
            // se suspende la protección solo para este relleno, empresa a empresa por la RLS).
            migrationBuilder.Sql("""
                ALTER TABLE facturacion.linea_factura DISABLE TRIGGER tg_linea_factura_solo_insercion;
                DO $m$
                DECLARE e record;
                BEGIN
                    IF to_regclass('organizacion.empresa') IS NULL THEN RETURN; END IF;
                    FOR e IN SELECT id FROM organizacion.empresa LOOP
                        PERFORM set_config('app.empresa_actual', e.id::text, true);
                        UPDATE facturacion.linea_factura l SET orden = n.orden
                        FROM (SELECT id, row_number() OVER (PARTITION BY factura_id ORDER BY ctid) AS orden FROM facturacion.linea_factura WHERE orden = 0) n
                        WHERE l.id = n.id;
                    END LOOP;
                    PERFORM set_config('app.empresa_actual', '', true);
                END $m$;
                ALTER TABLE facturacion.linea_factura ENABLE TRIGGER tg_linea_factura_solo_insercion;
                CREATE INDEX IF NOT EXISTS ix_linea_factura_orden ON facturacion.linea_factura (factura_id, orden);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS facturacion.ix_linea_factura_orden;");

            migrationBuilder.DropColumn(
                name: "orden",
                schema: "facturacion",
                table: "linea_factura");
        }
    }
}
