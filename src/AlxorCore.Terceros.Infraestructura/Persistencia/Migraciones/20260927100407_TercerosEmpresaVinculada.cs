using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Terceros.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class TercerosEmpresaVinculada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "empresa_vinculada_id",
                schema: "terceros",
                table: "proveedor",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "empresa_vinculada_id",
                schema: "terceros",
                table: "cliente",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_proveedor_empresa_vinculada",
                schema: "terceros",
                table: "proveedor",
                column: "empresa_vinculada_id");

            migrationBuilder.CreateIndex(
                name: "ix_cliente_empresa_vinculada",
                schema: "terceros",
                table: "cliente",
                column: "empresa_vinculada_id");

            // La empresa enlazada existe y es del grupo del tercero; si se da de baja la empresa, el tercero queda externo.
            migrationBuilder.Sql("""
                ALTER TABLE terceros.cliente ADD CONSTRAINT fk_cliente_empresa_vinculada
                    FOREIGN KEY (empresa_vinculada_id) REFERENCES organizacion.empresa (id) ON DELETE SET NULL;
                ALTER TABLE terceros.proveedor ADD CONSTRAINT fk_proveedor_empresa_vinculada
                    FOREIGN KEY (empresa_vinculada_id) REFERENCES organizacion.empresa (id) ON DELETE SET NULL;
                CREATE OR REPLACE FUNCTION terceros.empresa_vinculada_valida() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF NEW.empresa_vinculada_id IS NOT NULL AND NOT EXISTS (
                        SELECT 1 FROM organizacion.empresa e WHERE e.id = NEW.empresa_vinculada_id AND e.grupo_id = NEW.grupo_id) THEN
                        PERFORM public.alxor_error('tercero.empresa_vinculada', 'La empresa enlazada no es de este grupo.');
                    END IF;
                    RETURN NEW;
                END $f$;
                CREATE TRIGGER tg_cliente_empresa_vinculada BEFORE INSERT OR UPDATE OF empresa_vinculada_id, grupo_id ON terceros.cliente
                    FOR EACH ROW EXECUTE FUNCTION terceros.empresa_vinculada_valida();
                CREATE TRIGGER tg_proveedor_empresa_vinculada BEFORE INSERT OR UPDATE OF empresa_vinculada_id, grupo_id ON terceros.proveedor
                    FOR EACH ROW EXECUTE FUNCTION terceros.empresa_vinculada_valida();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS tg_cliente_empresa_vinculada ON terceros.cliente;
                DROP TRIGGER IF EXISTS tg_proveedor_empresa_vinculada ON terceros.proveedor;
                DROP FUNCTION IF EXISTS terceros.empresa_vinculada_valida();
                ALTER TABLE terceros.cliente DROP CONSTRAINT IF EXISTS fk_cliente_empresa_vinculada;
                ALTER TABLE terceros.proveedor DROP CONSTRAINT IF EXISTS fk_proveedor_empresa_vinculada;
                """);

            migrationBuilder.DropIndex(
                name: "ix_proveedor_empresa_vinculada",
                schema: "terceros",
                table: "proveedor");

            migrationBuilder.DropIndex(
                name: "ix_cliente_empresa_vinculada",
                schema: "terceros",
                table: "cliente");

            migrationBuilder.DropColumn(
                name: "empresa_vinculada_id",
                schema: "terceros",
                table: "proveedor");

            migrationBuilder.DropColumn(
                name: "empresa_vinculada_id",
                schema: "terceros",
                table: "cliente");
        }
    }
}
