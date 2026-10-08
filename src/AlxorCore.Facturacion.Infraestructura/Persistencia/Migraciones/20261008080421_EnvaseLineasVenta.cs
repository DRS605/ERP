using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Facturacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class EnvaseLineasVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "envase_producto_id",
                schema: "facturacion",
                table: "linea_presupuesto",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "envase_producto_id",
                schema: "facturacion",
                table: "linea_pedido_venta",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "envase_producto_id",
                schema: "facturacion",
                table: "linea_factura",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "envase_producto_id",
                schema: "facturacion",
                table: "linea_albaran_venta",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "envase_producto_id",
                schema: "facturacion",
                table: "linea_presupuesto");

            migrationBuilder.DropColumn(
                name: "envase_producto_id",
                schema: "facturacion",
                table: "linea_pedido_venta");

            migrationBuilder.DropColumn(
                name: "envase_producto_id",
                schema: "facturacion",
                table: "linea_factura");

            migrationBuilder.DropColumn(
                name: "envase_producto_id",
                schema: "facturacion",
                table: "linea_albaran_venta");
        }
    }
}
