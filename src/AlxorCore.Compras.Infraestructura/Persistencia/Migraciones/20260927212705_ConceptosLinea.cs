using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Compras.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ConceptosLinea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "conceptos",
                schema: "compras",
                table: "linea_pedido",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb");

            migrationBuilder.AddColumn<decimal>(
                name: "coste_conceptos",
                schema: "compras",
                table: "linea_pedido",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "importe_conceptos",
                schema: "compras",
                table: "linea_pedido",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql(GarantiasConceptosSql.Funcion);
            migrationBuilder.Sql(GarantiasConceptosSql.Comprobar("compras", "linea_pedido"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(GarantiasConceptosSql.QuitarComprobacion("compras", "linea_pedido"));

            migrationBuilder.DropColumn(
                name: "conceptos",
                schema: "compras",
                table: "linea_pedido");

            migrationBuilder.DropColumn(
                name: "coste_conceptos",
                schema: "compras",
                table: "linea_pedido");

            migrationBuilder.DropColumn(
                name: "importe_conceptos",
                schema: "compras",
                table: "linea_pedido");
        }
    }
}
