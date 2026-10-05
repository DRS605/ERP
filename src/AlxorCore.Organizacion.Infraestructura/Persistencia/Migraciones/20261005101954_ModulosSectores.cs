using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Organizacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ModulosSectores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Módulos sectoriales: subasta (alhóndiga, sobre agro), bodegas y viveros. Se contratan aparte.
            migrationBuilder.Sql("""
                ALTER TABLE organizacion.empresa DROP CONSTRAINT ck_empresa_modulos_adicionales;
                ALTER TABLE organizacion.empresa ADD CONSTRAINT ck_empresa_modulos_adicionales
                    CHECK (modulos_adicionales <@ ARRAY['ventas', 'compras', 'inventario', 'produccion', 'personal', 'proyectos',
                        'contabilidad', 'inmovilizado', 'analitica', 'tesoreria_avanzada', 'divisas', 'aprobaciones', 'integraciones', 'agro', 'logistica', 'cooperativa', 'subasta', 'bodega', 'vivero']::text[]);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE organizacion.empresa SET modulos_adicionales = array_remove(array_remove(array_remove(modulos_adicionales, 'subasta'), 'bodega'), 'vivero');
                ALTER TABLE organizacion.empresa DROP CONSTRAINT ck_empresa_modulos_adicionales;
                ALTER TABLE organizacion.empresa ADD CONSTRAINT ck_empresa_modulos_adicionales
                    CHECK (modulos_adicionales <@ ARRAY['ventas', 'compras', 'inventario', 'produccion', 'personal', 'proyectos',
                        'contabilidad', 'inmovilizado', 'analitica', 'tesoreria_avanzada', 'divisas', 'aprobaciones', 'integraciones', 'agro', 'logistica', 'cooperativa']::text[]);
                """);
        }
    }
}
