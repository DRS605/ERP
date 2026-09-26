using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Organizacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ModuloAgro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Nuevo módulo contratable «agro» (sectorial: no entra en ninguna edición).
            migrationBuilder.Sql("""
                ALTER TABLE organizacion.empresa DROP CONSTRAINT ck_empresa_modulos_adicionales;
                ALTER TABLE organizacion.empresa ADD CONSTRAINT ck_empresa_modulos_adicionales
                    CHECK (modulos_adicionales <@ ARRAY['ventas', 'compras', 'inventario', 'produccion', 'personal', 'proyectos',
                        'contabilidad', 'inmovilizado', 'analitica', 'tesoreria_avanzada', 'divisas', 'aprobaciones', 'integraciones', 'agro']::text[]);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE organizacion.empresa SET modulos_adicionales = array_remove(modulos_adicionales, 'agro');
                ALTER TABLE organizacion.empresa DROP CONSTRAINT ck_empresa_modulos_adicionales;
                ALTER TABLE organizacion.empresa ADD CONSTRAINT ck_empresa_modulos_adicionales
                    CHECK (modulos_adicionales <@ ARRAY['ventas', 'compras', 'inventario', 'produccion', 'personal', 'proyectos',
                        'contabilidad', 'inmovilizado', 'analitica', 'tesoreria_avanzada', 'divisas', 'aprobaciones', 'integraciones']::text[]);
                """);
        }
    }
}
