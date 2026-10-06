using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Inventario.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class RecuentosInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recuento_inventario",
                schema: "inventario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    almacen_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ubicacion_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    abierto_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    cerrado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    no_contados_a_cero = table.Column<bool>(type: "boolean", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recuento_inventario", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "linea_recuento",
                schema: "inventario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ubicacion_id = table.Column<Guid>(type: "uuid", nullable: true),
                    lote = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    teorico = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    contado = table.Column<decimal>(type: "numeric(14,3)", nullable: true),
                    anadida = table.Column<bool>(type: "boolean", nullable: false),
                    diferencia = table.Column<decimal>(type: "numeric(14,3)", nullable: true),
                    recuento_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_recuento", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_recuento_recuento_inventario_recuento_id",
                        column: x => x.recuento_id,
                        principalSchema: "inventario",
                        principalTable: "recuento_inventario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_linea_recuento_producto",
                schema: "inventario",
                table: "linea_recuento",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "ix_linea_recuento_recuento",
                schema: "inventario",
                table: "linea_recuento",
                column: "recuento_id");

            migrationBuilder.CreateIndex(
                name: "ix_linea_recuento_ubicacion",
                schema: "inventario",
                table: "linea_recuento",
                column: "ubicacion_id");

            migrationBuilder.CreateIndex(
                name: "ix_recuento_inventario_almacen",
                schema: "inventario",
                table: "recuento_inventario",
                column: "almacen_id");

            migrationBuilder.CreateIndex(
                name: "ix_recuento_inventario_ubicacion",
                schema: "inventario",
                table: "recuento_inventario",
                column: "ubicacion_id");

            migrationBuilder.CreateIndex(
                name: "ux_recuento_inventario_codigo",
                schema: "inventario",
                table: "recuento_inventario",
                columns: new[] { "empresa_id", "codigo" },
                unique: true);

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("inventario", "recuento_inventario"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("inventario", "linea_recuento", "recuento_id", "inventario", "recuento_inventario"));
            migrationBuilder.Sql("""
                ALTER TABLE inventario.recuento_inventario
                    ADD CONSTRAINT ck_recuento_inventario_valores CHECK (estado IN ('Abierto', 'Cerrado', 'Anulado') AND ((estado = 'Cerrado') = (cerrado_en IS NOT NULL))),
                    ADD CONSTRAINT fk_recuento_inventario_almacen FOREIGN KEY (almacen_id) REFERENCES inventario.almacen (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE inventario.linea_recuento
                    ADD CONSTRAINT ck_linea_recuento_valores CHECK ((contado IS NULL OR contado >= 0) AND (NOT anadida OR teorico = 0));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.QuitarRlsPorPadre("inventario", "linea_recuento"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Desactivar("inventario", "recuento_inventario"));

            migrationBuilder.DropTable(
                name: "linea_recuento",
                schema: "inventario");

            migrationBuilder.DropTable(
                name: "recuento_inventario",
                schema: "inventario");
        }
    }
}
