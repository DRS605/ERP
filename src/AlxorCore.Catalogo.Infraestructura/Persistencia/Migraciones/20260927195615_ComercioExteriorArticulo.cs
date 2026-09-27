using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Catalogo.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ComercioExteriorArticulo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codigo_arancelario",
                schema: "catalogo",
                table: "producto",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pais_origen",
                schema: "catalogo",
                table: "producto",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.Sql("""
                ALTER TABLE catalogo.producto
                    ADD CONSTRAINT ck_producto_codigo_arancelario CHECK (codigo_arancelario IS NULL OR codigo_arancelario ~ '^([0-9]{8}|[0-9]{10})$'),
                    ADD CONSTRAINT ck_producto_pais_origen CHECK (pais_origen IS NULL OR pais_origen ~ '^[A-Z]{2}$');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE catalogo.producto DROP CONSTRAINT IF EXISTS ck_producto_codigo_arancelario, DROP CONSTRAINT IF EXISTS ck_producto_pais_origen;");

            migrationBuilder.DropColumn(
                name: "codigo_arancelario",
                schema: "catalogo",
                table: "producto");

            migrationBuilder.DropColumn(
                name: "pais_origen",
                schema: "catalogo",
                table: "producto");
        }
    }
}
