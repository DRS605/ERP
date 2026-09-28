using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Tesoreria.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AnticiposFacturados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "movimiento_id",
                schema: "tesoreria",
                table: "aplicacion_anticipo",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<decimal>(
                name: "base",
                schema: "tesoreria",
                table: "aplicacion_anticipo",
                type: "numeric(14,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "base_facturada",
                schema: "tesoreria",
                table: "anticipo",
                type: "numeric(14,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "codigo_iva",
                schema: "tesoreria",
                table: "anticipo",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "factura_id",
                schema: "tesoreria",
                table: "anticipo",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "factura_numero",
                schema: "tesoreria",
                table: "anticipo",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_anticipo_factura",
                schema: "tesoreria",
                table: "anticipo",
                column: "factura_id");

            // Una aplicación es un cobro de la factura (movimiento) o un descuento en la factura final (base).
            migrationBuilder.Sql("""
                ALTER TABLE tesoreria.aplicacion_anticipo
                    ADD CONSTRAINT ck_aplicacion_anticipo_tipo CHECK (movimiento_id IS NOT NULL OR base IS NOT NULL);
                ALTER TABLE tesoreria.anticipo
                    ADD CONSTRAINT ck_anticipo_factura CHECK ((factura_id IS NULL) = (base_facturada IS NULL));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE tesoreria.aplicacion_anticipo DROP CONSTRAINT IF EXISTS ck_aplicacion_anticipo_tipo;
                ALTER TABLE tesoreria.anticipo DROP CONSTRAINT IF EXISTS ck_anticipo_factura;
                """);

            migrationBuilder.DropIndex(
                name: "ix_anticipo_factura",
                schema: "tesoreria",
                table: "anticipo");

            migrationBuilder.DropColumn(
                name: "base",
                schema: "tesoreria",
                table: "aplicacion_anticipo");

            migrationBuilder.DropColumn(
                name: "base_facturada",
                schema: "tesoreria",
                table: "anticipo");

            migrationBuilder.DropColumn(
                name: "codigo_iva",
                schema: "tesoreria",
                table: "anticipo");

            migrationBuilder.DropColumn(
                name: "factura_id",
                schema: "tesoreria",
                table: "anticipo");

            migrationBuilder.DropColumn(
                name: "factura_numero",
                schema: "tesoreria",
                table: "anticipo");

            migrationBuilder.AlterColumn<Guid>(
                name: "movimiento_id",
                schema: "tesoreria",
                table: "aplicacion_anticipo",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
