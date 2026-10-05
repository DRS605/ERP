using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Extensiones.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class EtiquetasCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "plantilla_etiqueta",
                schema: "extensiones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    marca = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    campos = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    texto_libre = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    formato = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plantilla_etiqueta", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "referencia_cliente",
                schema: "extensiones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    gtin = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_referencia_cliente", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_plantilla_etiqueta_cliente",
                schema: "extensiones",
                table: "plantilla_etiqueta",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_referencia_cliente_producto",
                schema: "extensiones",
                table: "referencia_cliente",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "ux_referencia_cliente_producto",
                schema: "extensiones",
                table: "referencia_cliente",
                columns: new[] { "empresa_id", "cliente_id", "producto_id" },
                unique: true);

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("extensiones", "plantilla_etiqueta"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("extensiones", "referencia_cliente"));
            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX ux_plantilla_etiqueta_activa ON extensiones.plantilla_etiqueta
                    (empresa_id, COALESCE(cliente_id, '00000000-0000-0000-0000-000000000000'::uuid)) WHERE activa;
                ALTER TABLE extensiones.referencia_cliente ADD CONSTRAINT ck_referencia_cliente_gtin CHECK (gtin IS NULL OR gtin ~ '^[0-9]{14}$');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS extensiones.ux_plantilla_etiqueta_activa;");
            migrationBuilder.DropTable(
                name: "plantilla_etiqueta",
                schema: "extensiones");

            migrationBuilder.DropTable(
                name: "referencia_cliente",
                schema: "extensiones");
        }
    }
}
