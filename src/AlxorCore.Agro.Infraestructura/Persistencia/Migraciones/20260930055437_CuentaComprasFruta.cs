using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Agro.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class CuentaComprasFruta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cuenta_compras_fruta",
                schema: "agro",
                table: "configuracion",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "600");

            migrationBuilder.Sql("""
                ALTER TABLE agro.configuracion ADD CONSTRAINT ck_configuracion_cuenta_compras_fruta CHECK (cuenta_compras_fruta ~ '^6[0-9]*$');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE agro.configuracion DROP CONSTRAINT IF EXISTS ck_configuracion_cuenta_compras_fruta;");

            migrationBuilder.DropColumn(
                name: "cuenta_compras_fruta",
                schema: "agro",
                table: "configuracion");
        }
    }
}
