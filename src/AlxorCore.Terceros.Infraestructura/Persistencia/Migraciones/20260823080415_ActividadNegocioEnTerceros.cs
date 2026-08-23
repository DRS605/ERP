using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Terceros.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ActividadNegocioEnTerceros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "actividad_negocio_id",
                schema: "terceros",
                table: "proveedor",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "actividad_negocio_id",
                schema: "terceros",
                table: "cliente",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "actividad_negocio_id",
                schema: "terceros",
                table: "proveedor");

            migrationBuilder.DropColumn(
                name: "actividad_negocio_id",
                schema: "terceros",
                table: "cliente");
        }
    }
}
