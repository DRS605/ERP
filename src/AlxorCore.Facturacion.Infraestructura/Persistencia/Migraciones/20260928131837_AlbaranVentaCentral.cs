using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Facturacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AlbaranVentaCentral : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "albaran_venta_id",
                schema: "facturacion",
                table: "linea_factura",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "linea_pedido_id",
                schema: "facturacion",
                table: "linea_albaran_venta",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "codigo_iva",
                schema: "facturacion",
                table: "linea_albaran_venta",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "IVA21");

            migrationBuilder.AddColumn<decimal>(
                name: "porcentaje_descuento",
                schema: "facturacion",
                table: "linea_albaran_venta",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "precio_fijado",
                schema: "facturacion",
                table: "linea_albaran_venta",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<decimal>(
                name: "precio_unitario",
                schema: "facturacion",
                table: "linea_albaran_venta",
                type: "numeric(14,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<Guid>(
                name: "pedido_id",
                schema: "facturacion",
                table: "albaran_venta",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "factura_id",
                schema: "facturacion",
                table: "albaran_venta",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "observaciones",
                schema: "facturacion",
                table: "albaran_venta",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "stock_descontado",
                schema: "facturacion",
                table: "albaran_venta",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_linea_factura_albaran_venta",
                schema: "facturacion",
                table: "linea_factura",
                column: "albaran_venta_id");

            migrationBuilder.CreateIndex(
                name: "ix_albaran_venta_cliente_fecha",
                schema: "facturacion",
                table: "albaran_venta",
                columns: new[] { "empresa_id", "cliente_id", "fecha" });

            migrationBuilder.CreateIndex(
                name: "ix_albaran_venta_factura",
                schema: "facturacion",
                table: "albaran_venta",
                column: "factura_id");

            // Los albaranes existentes toman el precio de su línea de pedido (valorados) y, si su pedido está facturado, esa
            // factura. No sacaron la mercancía (lo hacía la factura), así que stock_descontado queda a false.
            migrationBuilder.Sql("""
                DO $m$
                DECLARE e record;
                BEGIN
                    IF to_regclass('organizacion.empresa') IS NULL THEN RETURN; END IF;
                    FOR e IN SELECT id FROM organizacion.empresa LOOP
                        PERFORM set_config('app.empresa_actual', e.id::text, true);
                        UPDATE facturacion.linea_albaran_venta l
                        SET precio_unitario = lp.precio_unitario, porcentaje_descuento = lp.descuento, codigo_iva = lp.codigo_iva
                        FROM facturacion.linea_pedido_venta lp WHERE lp.id = l.linea_pedido_id;
                        UPDATE facturacion.albaran_venta a SET factura_id = p.factura_id
                        FROM facturacion.pedido_venta p WHERE p.id = a.pedido_id AND p.factura_id IS NOT NULL AND a.factura_id IS NULL;
                    END LOOP;
                    PERFORM set_config('app.empresa_actual', '', true);
                END $m$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_linea_factura_albaran_venta",
                schema: "facturacion",
                table: "linea_factura");

            migrationBuilder.DropIndex(
                name: "ix_albaran_venta_cliente_fecha",
                schema: "facturacion",
                table: "albaran_venta");

            migrationBuilder.DropIndex(
                name: "ix_albaran_venta_factura",
                schema: "facturacion",
                table: "albaran_venta");

            migrationBuilder.DropColumn(
                name: "albaran_venta_id",
                schema: "facturacion",
                table: "linea_factura");

            migrationBuilder.DropColumn(
                name: "codigo_iva",
                schema: "facturacion",
                table: "linea_albaran_venta");

            migrationBuilder.DropColumn(
                name: "porcentaje_descuento",
                schema: "facturacion",
                table: "linea_albaran_venta");

            migrationBuilder.DropColumn(
                name: "precio_fijado",
                schema: "facturacion",
                table: "linea_albaran_venta");

            migrationBuilder.DropColumn(
                name: "precio_unitario",
                schema: "facturacion",
                table: "linea_albaran_venta");

            migrationBuilder.DropColumn(
                name: "factura_id",
                schema: "facturacion",
                table: "albaran_venta");

            migrationBuilder.DropColumn(
                name: "observaciones",
                schema: "facturacion",
                table: "albaran_venta");

            migrationBuilder.DropColumn(
                name: "stock_descontado",
                schema: "facturacion",
                table: "albaran_venta");

            migrationBuilder.AlterColumn<Guid>(
                name: "linea_pedido_id",
                schema: "facturacion",
                table: "linea_albaran_venta",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "pedido_id",
                schema: "facturacion",
                table: "albaran_venta",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
