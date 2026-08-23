using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Terceros.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class MaestrosPorGrupo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "empresa_id",
                schema: "terceros",
                table: "proveedor",
                newName: "grupo_id");

            migrationBuilder.RenameIndex(
                name: "ix_proveedor_empresa_nombre",
                schema: "terceros",
                table: "proveedor",
                newName: "ix_proveedor_grupo_nombre");

            migrationBuilder.RenameColumn(
                name: "empresa_id",
                schema: "terceros",
                table: "cliente",
                newName: "grupo_id");

            migrationBuilder.RenameIndex(
                name: "ix_cliente_empresa_nombre",
                schema: "terceros",
                table: "cliente",
                newName: "ix_cliente_grupo_nombre");

            // Tras el renombrado, grupo_id contiene aún el antiguo empresa_id: se traduce al grupo de esa empresa.
            migrationBuilder.Sql("UPDATE terceros.cliente c SET grupo_id = e.grupo_id FROM organizacion.empresa e WHERE e.id = c.grupo_id;");
            migrationBuilder.Sql("UPDATE terceros.proveedor p SET grupo_id = e.grupo_id FROM organizacion.empresa e WHERE e.id = p.grupo_id;");

            // La RLS pasa de aislar por empresa a aislar por grupo (maestros compartidos).
            migrationBuilder.Sql("DROP POLICY IF EXISTS \"pol_empresa_cliente\" ON terceros.cliente;");
            migrationBuilder.Sql("DROP POLICY IF EXISTS \"pol_empresa_proveedor\" ON terceros.proveedor;");
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.ActivarPorGrupo("terceros", "cliente"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.ActivarPorGrupo("terceros", "proveedor"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "grupo_id",
                schema: "terceros",
                table: "proveedor",
                newName: "empresa_id");

            migrationBuilder.RenameIndex(
                name: "ix_proveedor_grupo_nombre",
                schema: "terceros",
                table: "proveedor",
                newName: "ix_proveedor_empresa_nombre");

            migrationBuilder.RenameColumn(
                name: "grupo_id",
                schema: "terceros",
                table: "cliente",
                newName: "empresa_id");

            migrationBuilder.RenameIndex(
                name: "ix_cliente_grupo_nombre",
                schema: "terceros",
                table: "cliente",
                newName: "ix_cliente_empresa_nombre");
        }
    }
}
