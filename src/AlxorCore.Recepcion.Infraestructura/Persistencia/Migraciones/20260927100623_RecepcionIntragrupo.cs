using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Recepcion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class RecepcionIntragrupo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "empresa_origen_id",
                schema: "recepcion",
                table: "factura_recibida",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "factura_origen_id",
                schema: "recepcion",
                table: "factura_recibida",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ux_factura_recibida_origen",
                schema: "recepcion",
                table: "factura_recibida",
                columns: new[] { "empresa_id", "factura_origen_id" },
                unique: true,
                filter: "factura_origen_id IS NOT NULL");

            // Solo las intragrupo llevan empresa y factura de origen (y las dos).
            migrationBuilder.Sql("""
                ALTER TABLE recepcion.factura_recibida ADD CONSTRAINT ck_factura_recibida_intragrupo
                    CHECK ((origen = 'Intragrupo') = (empresa_origen_id IS NOT NULL) AND (empresa_origen_id IS NULL) = (factura_origen_id IS NULL));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE recepcion.factura_recibida DROP CONSTRAINT IF EXISTS ck_factura_recibida_intragrupo;");

            migrationBuilder.DropIndex(
                name: "ux_factura_recibida_origen",
                schema: "recepcion",
                table: "factura_recibida");

            migrationBuilder.DropColumn(
                name: "empresa_origen_id",
                schema: "recepcion",
                table: "factura_recibida");

            migrationBuilder.DropColumn(
                name: "factura_origen_id",
                schema: "recepcion",
                table: "factura_recibida");
        }
    }
}
