using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Contabilidad.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AfectacionYActividadPendiente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "actividad_negocio_id",
                schema: "contabilidad",
                table: "documento_pendiente",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "afectacion",
                schema: "contabilidad",
                table: "documento_pendiente",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "actividad_negocio_id",
                schema: "contabilidad",
                table: "documento_pendiente");

            migrationBuilder.DropColumn(
                name: "afectacion",
                schema: "contabilidad",
                table: "documento_pendiente");
        }
    }
}
