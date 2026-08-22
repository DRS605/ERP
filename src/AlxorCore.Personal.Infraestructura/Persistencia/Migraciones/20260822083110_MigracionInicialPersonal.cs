using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Personal.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class MigracionInicialPersonal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "personal");

            migrationBuilder.CreateTable(
                name: "persona",
                schema: "personal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    puesto = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    tarifa_hora = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    actualizado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_persona", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_persona_empresa",
                schema: "personal",
                table: "persona",
                columns: new[] { "empresa_id", "activo" });

            // Red de seguridad a nivel de BD: aislamiento por empresa (RLS).
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("personal", "persona"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "persona",
                schema: "personal");
        }
    }
}
