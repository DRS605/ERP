using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Compras.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class TraspasoIntragrupo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "albaran_venta_origen_id",
                schema: "compras",
                table: "pedido_compra",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "empresa_origen_id",
                schema: "compras",
                table: "pedido_compra",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "almacen_id",
                schema: "compras",
                table: "albaran_compra",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "anulado_en",
                schema: "compras",
                table: "albaran_compra",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo_anulacion",
                schema: "compras",
                table: "albaran_compra",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ux_pedido_compra_albaran_origen",
                schema: "compras",
                table: "pedido_compra",
                columns: new[] { "empresa_id", "albaran_venta_origen_id" },
                unique: true,
                filter: "albaran_venta_origen_id IS NOT NULL");

            migrationBuilder.Sql("""
                ALTER TABLE compras.pedido_compra ADD CONSTRAINT ck_pedido_compra_intragrupo
                    CHECK ((empresa_origen_id IS NULL) = (albaran_venta_origen_id IS NULL));
                CREATE OR REPLACE FUNCTION compras.albaran_compra_anulacion() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF OLD.anulado_en IS NOT NULL AND (NEW.anulado_en IS DISTINCT FROM OLD.anulado_en OR NEW.motivo_anulacion IS DISTINCT FROM OLD.motivo_anulacion) THEN
                        PERFORM public.alxor_error('albaran.anulado', 'El albarán ya está anulado.');
                    END IF;
                    RETURN NEW;
                END $f$;
                CREATE TRIGGER tg_albaran_compra_anulacion BEFORE UPDATE ON compras.albaran_compra
                    FOR EACH ROW EXECUTE FUNCTION compras.albaran_compra_anulacion();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS tg_albaran_compra_anulacion ON compras.albaran_compra;
                DROP FUNCTION IF EXISTS compras.albaran_compra_anulacion();
                ALTER TABLE compras.pedido_compra DROP CONSTRAINT IF EXISTS ck_pedido_compra_intragrupo;
                """);

            migrationBuilder.DropIndex(
                name: "ux_pedido_compra_albaran_origen",
                schema: "compras",
                table: "pedido_compra");

            migrationBuilder.DropColumn(
                name: "albaran_venta_origen_id",
                schema: "compras",
                table: "pedido_compra");

            migrationBuilder.DropColumn(
                name: "empresa_origen_id",
                schema: "compras",
                table: "pedido_compra");

            migrationBuilder.DropColumn(
                name: "almacen_id",
                schema: "compras",
                table: "albaran_compra");

            migrationBuilder.DropColumn(
                name: "anulado_en",
                schema: "compras",
                table: "albaran_compra");

            migrationBuilder.DropColumn(
                name: "motivo_anulacion",
                schema: "compras",
                table: "albaran_compra");
        }
    }
}
