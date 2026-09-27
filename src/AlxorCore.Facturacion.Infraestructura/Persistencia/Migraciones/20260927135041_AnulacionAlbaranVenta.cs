using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Facturacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AnulacionAlbaranVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "anulado_en",
                schema: "facturacion",
                table: "albaran_venta",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo_anulacion",
                schema: "facturacion",
                table: "albaran_venta",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            // Un albarán anulado no se «desanula» ni cambia su motivo.
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION facturacion.albaran_venta_anulacion() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF OLD.anulado_en IS NOT NULL AND (NEW.anulado_en IS DISTINCT FROM OLD.anulado_en OR NEW.motivo_anulacion IS DISTINCT FROM OLD.motivo_anulacion) THEN
                        PERFORM public.alxor_error('albaranventa.anulado', 'El albarán ya está anulado.');
                    END IF;
                    RETURN NEW;
                END $f$;
                CREATE TRIGGER tg_albaran_venta_anulacion BEFORE UPDATE ON facturacion.albaran_venta
                    FOR EACH ROW EXECUTE FUNCTION facturacion.albaran_venta_anulacion();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS tg_albaran_venta_anulacion ON facturacion.albaran_venta; DROP FUNCTION IF EXISTS facturacion.albaran_venta_anulacion();");

            migrationBuilder.DropColumn(
                name: "anulado_en",
                schema: "facturacion",
                table: "albaran_venta");

            migrationBuilder.DropColumn(
                name: "motivo_anulacion",
                schema: "facturacion",
                table: "albaran_venta");
        }
    }
}
