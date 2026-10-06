using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Inventario.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ReaprovisionamientoStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "regla_reaprovisionamiento",
                schema: "inventario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    almacen_id = table.Column<Guid>(type: "uuid", nullable: true),
                    minimo = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    maximo = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    multiplo = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    proveedor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regla_reaprovisionamiento", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_regla_reaprovisionamiento_almacen",
                schema: "inventario",
                table: "regla_reaprovisionamiento",
                column: "almacen_id");

            migrationBuilder.CreateIndex(
                name: "ix_regla_reaprovisionamiento_producto",
                schema: "inventario",
                table: "regla_reaprovisionamiento",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "ix_regla_reaprovisionamiento_proveedor",
                schema: "inventario",
                table: "regla_reaprovisionamiento",
                column: "proveedor_id");

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("inventario", "regla_reaprovisionamiento"));
            migrationBuilder.Sql("""
                ALTER TABLE inventario.regla_reaprovisionamiento
                    ADD CONSTRAINT ck_regla_reaprovisionamiento_valores CHECK (minimo >= 0 AND multiplo >= 0 AND maximo > 0 AND maximo >= minimo),
                    ADD CONSTRAINT fk_regla_reaprovisionamiento_almacen FOREIGN KEY (almacen_id) REFERENCES inventario.almacen (id) DEFERRABLE INITIALLY DEFERRED;
                CREATE UNIQUE INDEX ux_regla_reaprovisionamiento ON inventario.regla_reaprovisionamiento
                    (empresa_id, producto_id, coalesce(almacen_id, '00000000-0000-0000-0000-000000000000'::uuid));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS inventario.ux_regla_reaprovisionamiento;");
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Desactivar("inventario", "regla_reaprovisionamiento"));

            migrationBuilder.DropTable(
                name: "regla_reaprovisionamiento",
                schema: "inventario");
        }
    }
}
