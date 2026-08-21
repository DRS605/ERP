using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Compras.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class NumeracionYProductoCompras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ejercicio",
                schema: "compras",
                table: "pedido_compra",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "numero",
                schema: "compras",
                table: "pedido_compra",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "producto_id",
                schema: "compras",
                table: "linea_pedido",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "producto_id",
                schema: "compras",
                table: "linea_albaran",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "numero",
                schema: "compras",
                table: "albaran_compra",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ux_pedido_serie_proveedor",
                schema: "compras",
                table: "pedido_compra",
                columns: new[] { "empresa_id", "ejercicio", "proveedor_id", "numero" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_pedido_serie_proveedor",
                schema: "compras",
                table: "pedido_compra");

            migrationBuilder.DropColumn(
                name: "ejercicio",
                schema: "compras",
                table: "pedido_compra");

            migrationBuilder.DropColumn(
                name: "numero",
                schema: "compras",
                table: "pedido_compra");

            migrationBuilder.DropColumn(
                name: "producto_id",
                schema: "compras",
                table: "linea_pedido");

            migrationBuilder.DropColumn(
                name: "producto_id",
                schema: "compras",
                table: "linea_albaran");

            migrationBuilder.DropColumn(
                name: "numero",
                schema: "compras",
                table: "albaran_compra");
        }
    }
}
