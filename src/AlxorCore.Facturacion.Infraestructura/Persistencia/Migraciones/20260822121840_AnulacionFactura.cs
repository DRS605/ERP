using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Facturacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AnulacionFactura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "fecha_hora_anulacion",
                schema: "facturacion",
                table: "factura",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "huella_anulacion",
                schema: "facturacion",
                table: "factura",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo_anulacion",
                schema: "facturacion",
                table: "factura",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fecha_hora_anulacion",
                schema: "facturacion",
                table: "factura");

            migrationBuilder.DropColumn(
                name: "huella_anulacion",
                schema: "facturacion",
                table: "factura");

            migrationBuilder.DropColumn(
                name: "motivo_anulacion",
                schema: "facturacion",
                table: "factura");
        }
    }
}
