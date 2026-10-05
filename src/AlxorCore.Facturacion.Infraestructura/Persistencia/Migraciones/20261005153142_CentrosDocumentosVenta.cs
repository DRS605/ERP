using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Facturacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class CentrosDocumentosVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "centro_id",
                schema: "facturacion",
                table: "presupuesto",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "centro_id",
                schema: "facturacion",
                table: "pedido_venta",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "caja_id",
                schema: "facturacion",
                table: "factura",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "centro_id",
                schema: "facturacion",
                table: "factura",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "centro_id",
                schema: "facturacion",
                table: "albaran_venta",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_factura_centro",
                schema: "facturacion",
                table: "factura",
                column: "centro_id");

            migrationBuilder.Sql("ALTER TABLE facturacion.factura ADD CONSTRAINT ck_factura_caja_centro CHECK (caja_id IS NULL OR centro_id IS NOT NULL);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE facturacion.factura DROP CONSTRAINT IF EXISTS ck_factura_caja_centro;");
            migrationBuilder.DropIndex(
                name: "ix_factura_centro",
                schema: "facturacion",
                table: "factura");

            migrationBuilder.DropColumn(
                name: "centro_id",
                schema: "facturacion",
                table: "presupuesto");

            migrationBuilder.DropColumn(
                name: "centro_id",
                schema: "facturacion",
                table: "pedido_venta");

            migrationBuilder.DropColumn(
                name: "caja_id",
                schema: "facturacion",
                table: "factura");

            migrationBuilder.DropColumn(
                name: "centro_id",
                schema: "facturacion",
                table: "factura");

            migrationBuilder.DropColumn(
                name: "centro_id",
                schema: "facturacion",
                table: "albaran_venta");
        }
    }
}
