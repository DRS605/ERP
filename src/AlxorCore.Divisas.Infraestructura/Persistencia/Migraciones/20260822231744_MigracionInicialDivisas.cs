using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Divisas.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class MigracionInicialDivisas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "divisas");

            migrationBuilder.CreateTable(
                name: "tipo_cambio",
                schema: "divisas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    divisa = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    tasa_eur = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    actualizado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipo_cambio", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_tipo_cambio_empresa_divisa_fecha",
                schema: "divisas",
                table: "tipo_cambio",
                columns: new[] { "empresa_id", "divisa", "fecha" },
                unique: true);

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("divisas", "tipo_cambio"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tipo_cambio",
                schema: "divisas");
        }
    }
}
