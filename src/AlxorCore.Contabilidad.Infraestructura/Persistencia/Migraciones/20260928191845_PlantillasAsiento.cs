using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Contabilidad.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class PlantillasAsiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "plantilla_asiento",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sentido = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    origen_tipo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    concepto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    diario = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    lineas = table.Column<string>(type: "jsonb", nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plantilla_asiento", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_plantilla_asiento_sentido_origen",
                schema: "contabilidad",
                table: "plantilla_asiento",
                columns: new[] { "empresa_id", "sentido", "origen_tipo" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("contabilidad", "plantilla_asiento"));
            migrationBuilder.Sql("""
                ALTER TABLE contabilidad.plantilla_asiento
                    ADD CONSTRAINT ck_plantilla_asiento_sentido CHECK (sentido IN ('Venta', 'Compra', 'Cobro', 'Pago')),
                    ADD CONSTRAINT ck_plantilla_asiento_lineas CHECK (jsonb_typeof(lineas) = 'array');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "plantilla_asiento",
                schema: "contabilidad");
        }
    }
}
