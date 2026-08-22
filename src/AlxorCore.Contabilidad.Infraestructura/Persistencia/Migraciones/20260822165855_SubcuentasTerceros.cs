using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Contabilidad.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class SubcuentasTerceros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "tercero_id",
                schema: "contabilidad",
                table: "cuenta",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tipo_tercero",
                schema: "contabilidad",
                table: "cuenta",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "longitud_subcuenta",
                schema: "contabilidad",
                table: "config_contabilidad",
                type: "integer",
                nullable: false,
                defaultValue: 8);

            migrationBuilder.CreateIndex(
                name: "ix_cuenta_empresa_tercero",
                schema: "contabilidad",
                table: "cuenta",
                columns: new[] { "empresa_id", "tercero_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_cuenta_empresa_tercero",
                schema: "contabilidad",
                table: "cuenta");

            migrationBuilder.DropColumn(
                name: "tercero_id",
                schema: "contabilidad",
                table: "cuenta");

            migrationBuilder.DropColumn(
                name: "tipo_tercero",
                schema: "contabilidad",
                table: "cuenta");

            migrationBuilder.DropColumn(
                name: "longitud_subcuenta",
                schema: "contabilidad",
                table: "config_contabilidad");
        }
    }
}
