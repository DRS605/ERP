using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Produccion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class LoteOrdenFabricacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "fecha_caducidad",
                schema: "produccion",
                table: "orden_fabricacion",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "lote",
                schema: "produccion",
                table: "orden_fabricacion",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fecha_caducidad",
                schema: "produccion",
                table: "orden_fabricacion");

            migrationBuilder.DropColumn(
                name: "lote",
                schema: "produccion",
                table: "orden_fabricacion");
        }
    }
}
