using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Contabilidad.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ReglasContabilizacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "regla_contabilizacion",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sentido = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    familia = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    tipo_tercero = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    cuenta_codigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regla_contabilizacion", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_regla_contabilizacion_empresa_sentido",
                schema: "contabilidad",
                table: "regla_contabilizacion",
                columns: new[] { "empresa_id", "sentido" });

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("contabilidad", "regla_contabilizacion"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "regla_contabilizacion",
                schema: "contabilidad");
        }
    }
}
