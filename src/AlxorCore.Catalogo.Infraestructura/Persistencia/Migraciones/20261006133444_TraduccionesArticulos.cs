using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Catalogo.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class TraduccionesArticulos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "traduccion_articulo",
                schema: "catalogo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    idioma = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_traduccion_articulo", x => x.id);
                    table.ForeignKey(
                        name: "FK_traduccion_articulo_producto_producto_id",
                        column: x => x.producto_id,
                        principalSchema: "catalogo",
                        principalTable: "producto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_traduccion_articulo_idioma",
                schema: "catalogo",
                table: "traduccion_articulo",
                columns: new[] { "producto_id", "idioma" },
                unique: true);
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("catalogo", "traduccion_articulo", "producto_id", "catalogo", "producto"));
            migrationBuilder.Sql("ALTER TABLE catalogo.traduccion_articulo ADD CONSTRAINT ck_traduccion_articulo_idioma CHECK (idioma IN ('en', 'fr', 'de', 'it', 'pt') AND btrim(nombre) <> '');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.QuitarRlsPorPadre("catalogo", "traduccion_articulo"));
            migrationBuilder.DropTable(
                name: "traduccion_articulo",
                schema: "catalogo");
        }
    }
}
