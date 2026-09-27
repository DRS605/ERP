using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Contabilidad.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class LineasDocumentoPendiente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "lineas",
                schema: "contabilidad",
                table: "documento_pendiente",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "lineas",
                schema: "contabilidad",
                table: "documento_pendiente");
        }
    }
}
