using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Facturacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class FacturasDivisa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "base_divisa",
                schema: "facturacion",
                table: "linea_factura",
                type: "numeric(14,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "precio_divisa",
                schema: "facturacion",
                table: "linea_factura",
                type: "numeric(14,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "base_divisa",
                schema: "facturacion",
                table: "factura",
                type: "numeric(14,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "cuota_divisa",
                schema: "facturacion",
                table: "factura",
                type: "numeric(14,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "moneda",
                schema: "facturacion",
                table: "factura",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "tasa_cambio",
                schema: "facturacion",
                table: "factura",
                type: "numeric(18,8)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_divisa",
                schema: "facturacion",
                table: "factura",
                type: "numeric(14,2)",
                nullable: true);
            // En divisa, manda la base en la divisa (la de euros es su contravalor al tipo de la factura).
            migrationBuilder.Sql("""
                ALTER TABLE facturacion.linea_factura DROP CONSTRAINT ck_linea_factura_base;
                ALTER TABLE facturacion.linea_factura ADD CONSTRAINT ck_linea_factura_base CHECK (
                    (precio_divisa IS NULL AND base_divisa IS NULL AND base = round(cantidad * precio_unitario * (1 - descuento / 100), 2) + importe_conceptos)
                    OR (precio_divisa IS NOT NULL AND importe_conceptos = 0 AND base_divisa = round(cantidad * precio_divisa * (1 - descuento / 100), 2)));
                ALTER TABLE facturacion.factura ADD CONSTRAINT ck_factura_divisa CHECK (
                    (moneda IS NULL AND tasa_cambio IS NULL AND base_divisa IS NULL AND cuota_divisa IS NULL AND total_divisa IS NULL)
                    OR (moneda ~ '^[A-Z]{3}$' AND moneda <> 'EUR' AND tasa_cambio > 0 AND base_divisa IS NOT NULL AND cuota_divisa IS NOT NULL AND total_divisa IS NOT NULL));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE facturacion.factura DROP CONSTRAINT ck_factura_divisa;
                ALTER TABLE facturacion.linea_factura DROP CONSTRAINT ck_linea_factura_base;
                ALTER TABLE facturacion.linea_factura ADD CONSTRAINT ck_linea_factura_base CHECK (base = round(cantidad * precio_unitario * (1 - descuento / 100), 2) + importe_conceptos);
                """);
            migrationBuilder.DropColumn(
                name: "base_divisa",
                schema: "facturacion",
                table: "linea_factura");

            migrationBuilder.DropColumn(
                name: "precio_divisa",
                schema: "facturacion",
                table: "linea_factura");

            migrationBuilder.DropColumn(
                name: "base_divisa",
                schema: "facturacion",
                table: "factura");

            migrationBuilder.DropColumn(
                name: "cuota_divisa",
                schema: "facturacion",
                table: "factura");

            migrationBuilder.DropColumn(
                name: "moneda",
                schema: "facturacion",
                table: "factura");

            migrationBuilder.DropColumn(
                name: "tasa_cambio",
                schema: "facturacion",
                table: "factura");

            migrationBuilder.DropColumn(
                name: "total_divisa",
                schema: "facturacion",
                table: "factura");
        }
    }
}
