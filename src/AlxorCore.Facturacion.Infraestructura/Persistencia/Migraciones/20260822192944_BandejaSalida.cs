using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Facturacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class BandejaSalida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mensaje_salida",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    carga = table.Column<string>(type: "jsonb", nullable: false),
                    procesado = table.Column<bool>(type: "boolean", nullable: false),
                    intentos = table.Column<int>(type: "integer", nullable: false),
                    ultimo_error = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    procesado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mensaje_salida", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_mensaje_salida_empresa_procesado",
                schema: "facturacion",
                table: "mensaje_salida",
                columns: new[] { "empresa_id", "procesado" });

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("facturacion", "mensaje_salida"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mensaje_salida",
                schema: "facturacion");
        }
    }
}
