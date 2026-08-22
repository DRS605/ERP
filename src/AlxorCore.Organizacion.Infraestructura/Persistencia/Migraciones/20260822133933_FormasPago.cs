using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Organizacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class FormasPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "forma_pago",
                schema: "organizacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    genera_vencimiento = table.Column<bool>(type: "boolean", nullable: false),
                    dias_vencimiento = table.Column<int>(type: "integer", nullable: false),
                    registrar_pago_automatico = table.Column<bool>(type: "boolean", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forma_pago", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_forma_pago_empresa",
                schema: "organizacion",
                table: "forma_pago",
                column: "empresa_id");

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("organizacion", "forma_pago"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "forma_pago",
                schema: "organizacion");
        }
    }
}
