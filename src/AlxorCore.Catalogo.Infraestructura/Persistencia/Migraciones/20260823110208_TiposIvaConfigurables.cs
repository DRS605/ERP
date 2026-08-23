using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Catalogo.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class TiposIvaConfigurables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tipo_iva",
                schema: "catalogo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    porcentaje = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    recargo_equivalencia = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    clase = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    mencion_factura = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    actualizado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipo_iva", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_tipo_iva_empresa_codigo",
                schema: "catalogo",
                table: "tipo_iva",
                columns: new[] { "empresa_id", "codigo" },
                unique: true);

            migrationBuilder.Sql(RlsSql.Activar("catalogo", "tipo_iva"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tipo_iva",
                schema: "catalogo");
        }
    }
}
