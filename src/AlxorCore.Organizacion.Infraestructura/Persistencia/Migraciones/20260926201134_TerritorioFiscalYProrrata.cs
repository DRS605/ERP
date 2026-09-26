using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Organizacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class TerritorioFiscalYProrrata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "territorio_fiscal",
                schema: "organizacion",
                table: "empresa",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Comun");

            migrationBuilder.CreateTable(
                name: "prorrata_ejercicio",
                schema: "organizacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    regimen = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    porcentaje_provisional = table.Column<int>(type: "integer", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prorrata_ejercicio", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_prorrata_empresa_ejercicio",
                schema: "organizacion",
                table: "prorrata_ejercicio",
                columns: new[] { "empresa_id", "ejercicio" },
                unique: true);

            migrationBuilder.Sql(RlsSql.Activar("organizacion", "prorrata_ejercicio"));
            migrationBuilder.Sql("""
                ALTER TABLE organizacion.empresa
                    ADD CONSTRAINT ck_empresa_territorio_fiscal CHECK (territorio_fiscal IN ('Comun', 'Canarias'));
                ALTER TABLE organizacion.prorrata_ejercicio
                    ADD CONSTRAINT ck_prorrata_regimen CHECK (regimen IN ('General', 'Especial')),
                    ADD CONSTRAINT ck_prorrata_porcentaje CHECK (porcentaje_provisional BETWEEN 0 AND 100),
                    ADD CONSTRAINT ck_prorrata_ejercicio CHECK (ejercicio BETWEEN 2000 AND 2100);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prorrata_ejercicio",
                schema: "organizacion");

            migrationBuilder.DropColumn(
                name: "territorio_fiscal",
                schema: "organizacion",
                table: "empresa");
        }
    }
}
