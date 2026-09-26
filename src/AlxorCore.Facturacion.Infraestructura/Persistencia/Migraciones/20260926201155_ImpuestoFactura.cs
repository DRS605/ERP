using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Facturacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ImpuestoFactura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "impuesto",
                schema: "facturacion",
                table: "factura",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Iva");

            // El IGIC no tiene recargo de equivalencia. La columna queda protegida por el trigger de
            // inalterabilidad de la factura emitida (toda columna no listada como cambiable lo está).
            migrationBuilder.Sql("""
                ALTER TABLE facturacion.factura
                    ADD CONSTRAINT ck_factura_impuesto CHECK (impuesto IN ('Iva', 'Igic')),
                    ADD CONSTRAINT ck_factura_igic_sin_recargo CHECK (impuesto <> 'Igic' OR recargo_total = 0);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "impuesto",
                schema: "facturacion",
                table: "factura");
        }
    }
}
