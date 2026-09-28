using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Facturacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class FacturasAnticipo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "anticipo_id",
                schema: "facturacion",
                table: "linea_factura",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cuenta_contable",
                schema: "facturacion",
                table: "linea_factura",
                type: "character varying(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_linea_factura_anticipo",
                schema: "facturacion",
                table: "linea_factura",
                column: "anticipo_id");

            // Solo la línea que descuenta un anticipo facturado lleva precio negativo.
            migrationBuilder.Sql("""
                ALTER TABLE facturacion.linea_factura
                    DROP CONSTRAINT IF EXISTS ck_linea_factura_precio,
                    ADD CONSTRAINT ck_linea_factura_precio CHECK ((precio_unitario >= 0 OR anticipo_id IS NOT NULL) AND coste_unitario >= 0);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE facturacion.linea_factura
                    DROP CONSTRAINT IF EXISTS ck_linea_factura_precio,
                    ADD CONSTRAINT ck_linea_factura_precio CHECK (precio_unitario >= 0 AND coste_unitario >= 0);
                """);

            migrationBuilder.DropIndex(
                name: "ix_linea_factura_anticipo",
                schema: "facturacion",
                table: "linea_factura");

            migrationBuilder.DropColumn(
                name: "anticipo_id",
                schema: "facturacion",
                table: "linea_factura");

            migrationBuilder.DropColumn(
                name: "cuenta_contable",
                schema: "facturacion",
                table: "linea_factura");
        }
    }
}
