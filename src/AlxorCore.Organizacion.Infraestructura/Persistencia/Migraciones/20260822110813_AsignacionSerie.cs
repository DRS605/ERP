using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Organizacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AsignacionSerie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "asignacion_serie",
                schema: "organizacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_documento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ambito = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tercero_id = table.Column<Guid>(type: "uuid", nullable: false),
                    prefijo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asignacion_serie", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_asignacion_serie",
                schema: "organizacion",
                table: "asignacion_serie",
                columns: new[] { "empresa_id", "tipo_documento", "ambito", "tercero_id" },
                unique: true);

            // Red de seguridad a nivel de BD: aislamiento por empresa (RLS).
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("organizacion", "asignacion_serie"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asignacion_serie",
                schema: "organizacion");
        }
    }
}
