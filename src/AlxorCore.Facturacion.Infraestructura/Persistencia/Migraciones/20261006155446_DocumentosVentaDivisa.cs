using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Facturacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class DocumentosVentaDivisa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "moneda",
                schema: "facturacion",
                table: "presupuesto",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "moneda",
                schema: "facturacion",
                table: "pedido_venta",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "moneda",
                schema: "facturacion",
                table: "albaran_venta",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.Sql("""
                ALTER TABLE facturacion.presupuesto ADD CONSTRAINT ck_presupuesto_moneda CHECK (moneda IS NULL OR (moneda ~ '^[A-Z]{3}$' AND moneda <> 'EUR'));
                ALTER TABLE facturacion.pedido_venta ADD CONSTRAINT ck_pedido_venta_moneda CHECK (moneda IS NULL OR (moneda ~ '^[A-Z]{3}$' AND moneda <> 'EUR'));
                ALTER TABLE facturacion.albaran_venta ADD CONSTRAINT ck_albaran_venta_moneda CHECK (moneda IS NULL OR (moneda ~ '^[A-Z]{3}$' AND moneda <> 'EUR'));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE facturacion.presupuesto DROP CONSTRAINT ck_presupuesto_moneda;
                ALTER TABLE facturacion.pedido_venta DROP CONSTRAINT ck_pedido_venta_moneda;
                ALTER TABLE facturacion.albaran_venta DROP CONSTRAINT ck_albaran_venta_moneda;
                """);
            migrationBuilder.DropColumn(
                name: "moneda",
                schema: "facturacion",
                table: "presupuesto");

            migrationBuilder.DropColumn(
                name: "moneda",
                schema: "facturacion",
                table: "pedido_venta");

            migrationBuilder.DropColumn(
                name: "moneda",
                schema: "facturacion",
                table: "albaran_venta");
        }
    }
}
