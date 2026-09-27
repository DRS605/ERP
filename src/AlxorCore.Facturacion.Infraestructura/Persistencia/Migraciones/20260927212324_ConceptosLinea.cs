using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Facturacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ConceptosLinea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "conceptos",
                schema: "facturacion",
                table: "linea_presupuesto",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb");

            migrationBuilder.AddColumn<decimal>(
                name: "coste_conceptos",
                schema: "facturacion",
                table: "linea_presupuesto",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "importe_conceptos",
                schema: "facturacion",
                table: "linea_presupuesto",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "conceptos",
                schema: "facturacion",
                table: "linea_pedido_venta",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb");

            migrationBuilder.AddColumn<decimal>(
                name: "coste_conceptos",
                schema: "facturacion",
                table: "linea_pedido_venta",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "importe_conceptos",
                schema: "facturacion",
                table: "linea_pedido_venta",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "conceptos",
                schema: "facturacion",
                table: "linea_factura",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb");

            migrationBuilder.AddColumn<decimal>(
                name: "coste_conceptos",
                schema: "facturacion",
                table: "linea_factura",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "importe_conceptos",
                schema: "facturacion",
                table: "linea_factura",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            // La base de la factura incluye los conceptos que cambian el importe, y los totales de conceptos cuadran
            // con la copia jsonb de cada concepto.
            migrationBuilder.Sql(GarantiasConceptosSql.Funcion);
            migrationBuilder.Sql("""
                ALTER TABLE facturacion.linea_factura
                    DROP CONSTRAINT ck_linea_factura_base,
                    ADD CONSTRAINT ck_linea_factura_base CHECK (base = round(cantidad * precio_unitario * (1 - descuento / 100), 2) + importe_conceptos);
                """);
            migrationBuilder.Sql(GarantiasConceptosSql.Comprobar("facturacion", "linea_factura"));
            migrationBuilder.Sql(GarantiasConceptosSql.Comprobar("facturacion", "linea_presupuesto"));
            migrationBuilder.Sql(GarantiasConceptosSql.Comprobar("facturacion", "linea_pedido_venta"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(GarantiasConceptosSql.QuitarComprobacion("facturacion", "linea_factura"));
            migrationBuilder.Sql(GarantiasConceptosSql.QuitarComprobacion("facturacion", "linea_presupuesto"));
            migrationBuilder.Sql(GarantiasConceptosSql.QuitarComprobacion("facturacion", "linea_pedido_venta"));
            migrationBuilder.Sql("""
                ALTER TABLE facturacion.linea_factura
                    DROP CONSTRAINT ck_linea_factura_base,
                    ADD CONSTRAINT ck_linea_factura_base CHECK (base = round(cantidad * precio_unitario * (1 - descuento / 100), 2));
                """);

            migrationBuilder.DropColumn(
                name: "conceptos",
                schema: "facturacion",
                table: "linea_presupuesto");

            migrationBuilder.DropColumn(
                name: "coste_conceptos",
                schema: "facturacion",
                table: "linea_presupuesto");

            migrationBuilder.DropColumn(
                name: "importe_conceptos",
                schema: "facturacion",
                table: "linea_presupuesto");

            migrationBuilder.DropColumn(
                name: "conceptos",
                schema: "facturacion",
                table: "linea_pedido_venta");

            migrationBuilder.DropColumn(
                name: "coste_conceptos",
                schema: "facturacion",
                table: "linea_pedido_venta");

            migrationBuilder.DropColumn(
                name: "importe_conceptos",
                schema: "facturacion",
                table: "linea_pedido_venta");

            migrationBuilder.DropColumn(
                name: "conceptos",
                schema: "facturacion",
                table: "linea_factura");

            migrationBuilder.DropColumn(
                name: "coste_conceptos",
                schema: "facturacion",
                table: "linea_factura");

            migrationBuilder.DropColumn(
                name: "importe_conceptos",
                schema: "facturacion",
                table: "linea_factura");
        }
    }
}
