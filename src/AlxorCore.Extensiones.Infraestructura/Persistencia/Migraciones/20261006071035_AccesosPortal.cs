using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Extensiones.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AccesosPortal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "acceso_portal",
                schema: "extensiones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    tercero_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    huella = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    caduca = table.Column<DateOnly>(type: "date", nullable: true),
                    revocado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ultimo_acceso = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    accesos = table.Column<int>(type: "integer", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acceso_portal", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_acceso_portal_tercero",
                schema: "extensiones",
                table: "acceso_portal",
                columns: new[] { "tipo", "tercero_id" });

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("extensiones", "acceso_portal"));
            migrationBuilder.Sql("""
                ALTER TABLE extensiones.acceso_portal ADD CONSTRAINT ck_acceso_portal_huella CHECK (huella ~ '^[0-9A-F]{64}$' AND accesos >= 0);
                CREATE UNIQUE INDEX ux_acceso_portal_vigente ON extensiones.acceso_portal (empresa_id, tipo, tercero_id) WHERE revocado_en IS NULL AND caduca IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS extensiones.ux_acceso_portal_vigente;");
            migrationBuilder.DropTable(
                name: "acceso_portal",
                schema: "extensiones");
        }
    }
}
