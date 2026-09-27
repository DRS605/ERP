using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Compras.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class TraspasoParcialAlmacen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_pedido_compra_albaran_origen",
                schema: "compras",
                table: "pedido_compra");

            migrationBuilder.RenameColumn(
                name: "albaran_venta_origen_id",
                schema: "compras",
                table: "pedido_compra",
                newName: "pedido_venta_origen_id");

            migrationBuilder.AddColumn<Guid>(
                name: "linea_venta_origen_id",
                schema: "compras",
                table: "linea_pedido",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "albaran_venta_origen_id",
                schema: "compras",
                table: "albaran_compra",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "almacen_traspaso",
                schema: "compras",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_origen_id = table.Column<Guid>(type: "uuid", nullable: true),
                    almacen_id = table.Column<Guid>(type: "uuid", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_almacen_traspaso", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_pedido_compra_pedido_venta_origen",
                schema: "compras",
                table: "pedido_compra",
                columns: new[] { "empresa_id", "pedido_venta_origen_id" },
                unique: true,
                filter: "pedido_venta_origen_id IS NOT NULL AND estado <> 'Cancelado'");

            migrationBuilder.CreateIndex(
                name: "ux_albaran_compra_albaran_venta_origen",
                schema: "compras",
                table: "albaran_compra",
                columns: new[] { "empresa_id", "albaran_venta_origen_id" },
                unique: true,
                filter: "albaran_venta_origen_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_almacen_traspaso_origen",
                schema: "compras",
                table: "almacen_traspaso",
                columns: new[] { "empresa_id", "empresa_origen_id" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("compras", "almacen_traspaso"));

            // Los traspasos ya hechos (un pedido por albarán): el albarán de venta pasa al albarán de compra y el pedido
            // apunta a su pedido de venta. Con RLS forzada, empresa a empresa.
            migrationBuilder.Sql("""
                DO $m$
                DECLARE e record; r record;
                BEGIN
                    IF to_regclass('organizacion.empresa') IS NULL THEN RETURN; END IF;
                    CREATE TEMP TABLE tmp_albaran_pedido (albaran_id uuid PRIMARY KEY, pedido_id uuid NOT NULL) ON COMMIT DROP;
                    FOR e IN SELECT id FROM organizacion.empresa LOOP
                        PERFORM set_config('app.empresa_actual', e.id::text, true);
                        UPDATE compras.albaran_compra a SET albaran_venta_origen_id = p.pedido_venta_origen_id
                            FROM compras.pedido_compra p
                            WHERE a.pedido_id = p.id AND p.pedido_venta_origen_id IS NOT NULL AND a.albaran_venta_origen_id IS NULL;
                        IF to_regclass('facturacion.albaran_venta') IS NOT NULL THEN
                            INSERT INTO tmp_albaran_pedido SELECT id, pedido_id FROM facturacion.albaran_venta ON CONFLICT DO NOTHING;
                        END IF;
                    END LOOP;
                    FOR e IN SELECT id FROM organizacion.empresa LOOP
                        PERFORM set_config('app.empresa_actual', e.id::text, true);
                        -- Si ya hay otro pedido vivo del mismo pedido de venta, este se queda como estaba.
                        FOR r IN SELECT p.id, p.empresa_id, t.pedido_id FROM compras.pedido_compra p
                                 JOIN tmp_albaran_pedido t ON t.albaran_id = p.pedido_venta_origen_id LOOP
                            UPDATE compras.pedido_compra SET pedido_venta_origen_id = r.pedido_id
                                WHERE id = r.id AND NOT EXISTS (SELECT 1 FROM compras.pedido_compra q
                                    WHERE q.empresa_id = r.empresa_id AND q.pedido_venta_origen_id = r.pedido_id AND q.estado <> 'Cancelado');
                        END LOOP;
                    END LOOP;
                    PERFORM set_config('app.empresa_actual', '', true);
                END $m$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Los pedidos espejo con varias entregas no caben en el esquema anterior (un pedido por albarán).
            migrationBuilder.DropTable(
                name: "almacen_traspaso",
                schema: "compras");

            migrationBuilder.DropIndex(
                name: "ux_pedido_compra_pedido_venta_origen",
                schema: "compras",
                table: "pedido_compra");

            migrationBuilder.DropIndex(
                name: "ux_albaran_compra_albaran_venta_origen",
                schema: "compras",
                table: "albaran_compra");

            migrationBuilder.DropColumn(
                name: "linea_venta_origen_id",
                schema: "compras",
                table: "linea_pedido");

            migrationBuilder.DropColumn(
                name: "albaran_venta_origen_id",
                schema: "compras",
                table: "albaran_compra");

            migrationBuilder.RenameColumn(
                name: "pedido_venta_origen_id",
                schema: "compras",
                table: "pedido_compra",
                newName: "albaran_venta_origen_id");

            migrationBuilder.CreateIndex(
                name: "ux_pedido_compra_albaran_origen",
                schema: "compras",
                table: "pedido_compra",
                columns: new[] { "empresa_id", "albaran_venta_origen_id" },
                unique: true,
                filter: "albaran_venta_origen_id IS NOT NULL");
        }
    }
}
