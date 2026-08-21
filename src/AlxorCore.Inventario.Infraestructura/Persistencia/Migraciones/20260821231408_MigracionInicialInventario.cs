using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Inventario.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class MigracionInicialInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "inventario");

            migrationBuilder.CreateTable(
                name: "almacen",
                schema: "inventario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_almacen", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "existencia",
                schema: "inventario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    almacen_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ubicacion_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cantidad = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_existencia", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "movimiento_inventario",
                schema: "inventario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    almacen_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ubicacion_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cantidad = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    motivo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    referencia = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimiento_inventario", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ubicacion",
                schema: "inventario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    almacen_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ubicacion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ubicacion_defecto",
                schema: "inventario",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    almacen_id = table.Column<Guid>(type: "uuid", nullable: false),
                    proveedor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ubicacion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ubicacion_defecto", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_almacen_empresa_codigo",
                schema: "inventario",
                table: "almacen",
                columns: new[] { "empresa_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_existencia_clave",
                schema: "inventario",
                table: "existencia",
                columns: new[] { "empresa_id", "producto_id", "almacen_id", "ubicacion_id" });

            migrationBuilder.CreateIndex(
                name: "ix_movimiento_empresa_producto",
                schema: "inventario",
                table: "movimiento_inventario",
                columns: new[] { "empresa_id", "producto_id" });

            migrationBuilder.CreateIndex(
                name: "ux_ubicacion_almacen_codigo",
                schema: "inventario",
                table: "ubicacion",
                columns: new[] { "empresa_id", "almacen_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ubidef_clave",
                schema: "inventario",
                table: "ubicacion_defecto",
                columns: new[] { "empresa_id", "producto_id", "almacen_id", "proveedor_id" });

            // Row-Level Security por empresa.
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("inventario", "almacen"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("inventario", "ubicacion"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("inventario", "existencia"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("inventario", "movimiento_inventario"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("inventario", "ubicacion_defecto"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var t in new[] { "almacen", "ubicacion", "existencia", "movimiento_inventario", "ubicacion_defecto" })
            {
                migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Desactivar("inventario", t));
            }

            migrationBuilder.DropTable(
                name: "almacen",
                schema: "inventario");

            migrationBuilder.DropTable(
                name: "existencia",
                schema: "inventario");

            migrationBuilder.DropTable(
                name: "movimiento_inventario",
                schema: "inventario");

            migrationBuilder.DropTable(
                name: "ubicacion",
                schema: "inventario");

            migrationBuilder.DropTable(
                name: "ubicacion_defecto",
                schema: "inventario");
        }
    }
}
