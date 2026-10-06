using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Terceros.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class IdiomaDocumentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "idioma",
                schema: "terceros",
                table: "proveedor",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "idioma",
                schema: "terceros",
                table: "cliente",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);
            migrationBuilder.Sql("""
                ALTER TABLE terceros.cliente ADD CONSTRAINT ck_cliente_idioma CHECK (idioma IN ('en', 'fr', 'de', 'it', 'pt'));
                ALTER TABLE terceros.proveedor ADD CONSTRAINT ck_proveedor_idioma CHECK (idioma IN ('en', 'fr', 'de', 'it', 'pt'));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE terceros.cliente DROP CONSTRAINT ck_cliente_idioma; ALTER TABLE terceros.proveedor DROP CONSTRAINT ck_proveedor_idioma;");
            migrationBuilder.DropColumn(
                name: "idioma",
                schema: "terceros",
                table: "proveedor");

            migrationBuilder.DropColumn(
                name: "idioma",
                schema: "terceros",
                table: "cliente");
        }
    }
}
