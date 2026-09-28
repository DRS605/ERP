using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Analisis.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class InformesAnalisis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "analisis");

            migrationBuilder.CreateTable(
                name: "informe",
                schema: "analisis",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    dataset = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    definicion = table.Column<string>(type: "jsonb", nullable: false),
                    compartido = table.Column<bool>(type: "boolean", nullable: false),
                    favorito = table.Column<bool>(type: "boolean", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    actualizado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_informe", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_informe_empresa_usuario",
                schema: "analisis",
                table: "informe",
                columns: new[] { "empresa_id", "usuario_id" });

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("analisis", "informe"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "informe",
                schema: "analisis");
        }
    }
}
