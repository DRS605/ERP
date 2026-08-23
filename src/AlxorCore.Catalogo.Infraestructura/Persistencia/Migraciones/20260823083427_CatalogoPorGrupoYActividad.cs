using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Catalogo.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class CatalogoPorGrupoYActividad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Existencias por empresa (nueva tabla) — se rellena desde el stock simple del producto
            //    ANTES de borrar la columna. El catálogo pasa a ser del grupo, pero el stock es de la
            //    empresa que lo tenía (la columna empresa_id del producto aún existe en este punto).
            migrationBuilder.CreateTable(
                name: "existencia_simple",
                schema: "catalogo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cantidad = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    actualizado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_existencia_simple", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_existencia_simple_empresa_producto",
                schema: "catalogo",
                table: "existencia_simple",
                columns: new[] { "empresa_id", "producto_id" },
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO catalogo.existencia_simple (id, empresa_id, producto_id, cantidad, actualizado_en)
                SELECT gen_random_uuid(), p.empresa_id, p.id, p.stock, p.actualizado_en
                FROM catalogo.producto p
                WHERE p.controlar_stock = true AND p.stock <> 0;
                """);

            migrationBuilder.DropColumn(
                name: "stock",
                schema: "catalogo",
                table: "producto");

            // 2) El catálogo pasa a ser del grupo: renombramos empresa_id -> grupo_id y rellenamos el
            //    grupo de cada empresa (no disruptivo: por defecto cada empresa es su propio grupo).
            migrationBuilder.RenameColumn(
                name: "empresa_id",
                schema: "catalogo",
                table: "producto",
                newName: "grupo_id");

            migrationBuilder.RenameIndex(
                name: "ix_producto_empresa_nombre",
                schema: "catalogo",
                table: "producto",
                newName: "ix_producto_grupo_nombre");

            migrationBuilder.RenameColumn(
                name: "empresa_id",
                schema: "catalogo",
                table: "historico_precio",
                newName: "grupo_id");

            migrationBuilder.RenameColumn(
                name: "empresa_id",
                schema: "catalogo",
                table: "familia",
                newName: "grupo_id");

            migrationBuilder.RenameIndex(
                name: "ix_familia_empresa_padre",
                schema: "catalogo",
                table: "familia",
                newName: "ix_familia_grupo_padre");

            migrationBuilder.Sql("""
                UPDATE catalogo.producto p SET grupo_id = e.grupo_id FROM organizacion.empresa e WHERE e.id = p.grupo_id;
                UPDATE catalogo.historico_precio h SET grupo_id = e.grupo_id FROM organizacion.empresa e WHERE e.id = h.grupo_id;
                UPDATE catalogo.familia f SET grupo_id = e.grupo_id FROM organizacion.empresa e WHERE e.id = f.grupo_id;
                """);

            migrationBuilder.AddColumn<Guid>(
                name: "actividad_negocio_id",
                schema: "catalogo",
                table: "producto",
                type: "uuid",
                nullable: true);

            // 3) RLS: el catálogo pasa a aislarse por grupo; las existencias, por empresa.
            migrationBuilder.Sql("""DROP POLICY IF EXISTS "pol_empresa_producto" ON "catalogo"."producto";""");
            migrationBuilder.Sql("""DROP POLICY IF EXISTS "pol_empresa_familia" ON "catalogo"."familia";""");
            migrationBuilder.Sql("""DROP POLICY IF EXISTS "pol_empresa_historico_precio" ON "catalogo"."historico_precio";""");
            migrationBuilder.Sql(RlsSql.ActivarPorGrupo("catalogo", "producto"));
            migrationBuilder.Sql(RlsSql.ActivarPorGrupo("catalogo", "familia"));
            migrationBuilder.Sql(RlsSql.ActivarPorGrupo("catalogo", "historico_precio"));
            migrationBuilder.Sql(RlsSql.Activar("catalogo", "existencia_simple"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(RlsSql.Desactivar("catalogo", "existencia_simple"));
            migrationBuilder.Sql("""DROP POLICY IF EXISTS "pol_grupo_producto" ON "catalogo"."producto";""");
            migrationBuilder.Sql("""DROP POLICY IF EXISTS "pol_grupo_familia" ON "catalogo"."familia";""");
            migrationBuilder.Sql("""DROP POLICY IF EXISTS "pol_grupo_historico_precio" ON "catalogo"."historico_precio";""");

            migrationBuilder.DropColumn(
                name: "actividad_negocio_id",
                schema: "catalogo",
                table: "producto");

            migrationBuilder.RenameColumn(
                name: "grupo_id",
                schema: "catalogo",
                table: "producto",
                newName: "empresa_id");

            migrationBuilder.RenameIndex(
                name: "ix_producto_grupo_nombre",
                schema: "catalogo",
                table: "producto",
                newName: "ix_producto_empresa_nombre");

            migrationBuilder.RenameColumn(
                name: "grupo_id",
                schema: "catalogo",
                table: "historico_precio",
                newName: "empresa_id");

            migrationBuilder.RenameColumn(
                name: "grupo_id",
                schema: "catalogo",
                table: "familia",
                newName: "empresa_id");

            migrationBuilder.RenameIndex(
                name: "ix_familia_grupo_padre",
                schema: "catalogo",
                table: "familia",
                newName: "ix_familia_empresa_padre");

            migrationBuilder.AddColumn<decimal>(
                name: "stock",
                schema: "catalogo",
                table: "producto",
                type: "numeric(14,3)",
                nullable: false,
                defaultValue: 0m);

            // Restauramos el stock simple desde las existencias por empresa (una por producto).
            migrationBuilder.Sql("""
                UPDATE catalogo.producto p SET stock = e.cantidad
                FROM catalogo.existencia_simple e WHERE e.producto_id = p.id;
                """);

            migrationBuilder.Sql(RlsSql.Activar("catalogo", "producto"));
            migrationBuilder.Sql(RlsSql.Activar("catalogo", "familia"));
            migrationBuilder.Sql(RlsSql.Activar("catalogo", "historico_precio"));

            migrationBuilder.DropTable(
                name: "existencia_simple",
                schema: "catalogo");
        }
    }
}
