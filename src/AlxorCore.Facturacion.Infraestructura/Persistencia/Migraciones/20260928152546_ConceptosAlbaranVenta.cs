using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Facturacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ConceptosAlbaranVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "conceptos",
                schema: "facturacion",
                table: "linea_albaran_venta",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb");

            migrationBuilder.AddColumn<decimal>(
                name: "coste_conceptos",
                schema: "facturacion",
                table: "linea_albaran_venta",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "importe_conceptos",
                schema: "facturacion",
                table: "linea_albaran_venta",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasConceptosSql.Comprobar("facturacion", "linea_albaran_venta"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasConceptosSql.QuitarComprobacion("facturacion", "linea_albaran_venta"));

            migrationBuilder.DropColumn(
                name: "conceptos",
                schema: "facturacion",
                table: "linea_albaran_venta");

            migrationBuilder.DropColumn(
                name: "coste_conceptos",
                schema: "facturacion",
                table: "linea_albaran_venta");

            migrationBuilder.DropColumn(
                name: "importe_conceptos",
                schema: "facturacion",
                table: "linea_albaran_venta");
        }
    }
}
