using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Catalogo.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ComposicionesAvanzadas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // tipo_iva.codigo sigue siendo varchar(15) en la base de datos aunque el modelo valide 10: no se acorta aquí
            // para no romper bases con códigos antiguos más largos.

            migrationBuilder.AddColumn<decimal>(
                name: "ajuste_precio_componentes",
                schema: "catalogo",
                table: "producto",
                type: "numeric(7,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "peso_kg",
                schema: "catalogo",
                table: "producto",
                type: "numeric(12,3)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "precio_segun_componentes",
                schema: "catalogo",
                table: "producto",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "tipo_composicion",
                schema: "catalogo",
                table: "producto",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Fabricacion");

            migrationBuilder.Sql("""
                ALTER TABLE catalogo.producto
                    ADD CONSTRAINT ck_producto_tipo_composicion CHECK (tipo_composicion IN ('Fabricacion', 'Kit')),
                    ADD CONSTRAINT ck_producto_ajuste_componentes CHECK (ajuste_precio_componentes BETWEEN -100 AND 1000),
                    ADD CONSTRAINT ck_producto_peso CHECK (peso_kg IS NULL OR peso_kg >= 0),
                    ADD CONSTRAINT ck_producto_kit_compuesto CHECK (tipo_composicion = 'Fabricacion' OR es_compuesto);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE catalogo.producto
                    DROP CONSTRAINT IF EXISTS ck_producto_tipo_composicion,
                    DROP CONSTRAINT IF EXISTS ck_producto_ajuste_componentes,
                    DROP CONSTRAINT IF EXISTS ck_producto_peso,
                    DROP CONSTRAINT IF EXISTS ck_producto_kit_compuesto;
                """);

            migrationBuilder.DropColumn(
                name: "ajuste_precio_componentes",
                schema: "catalogo",
                table: "producto");

            migrationBuilder.DropColumn(
                name: "peso_kg",
                schema: "catalogo",
                table: "producto");

            migrationBuilder.DropColumn(
                name: "precio_segun_componentes",
                schema: "catalogo",
                table: "producto");

            migrationBuilder.DropColumn(
                name: "tipo_composicion",
                schema: "catalogo",
                table: "producto");

        }
    }
}
