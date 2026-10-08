using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Facturacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ConceptosDivisa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Conceptos en facturas en divisa: cada concepto guarda su importe en la divisa (importeDivisa) y su contravalor
            // en euros (importe). La base en divisa de la línea suma los de importe en la divisa.
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION public.alxor_suma_conceptos_divisa(p_conceptos jsonb, p_efecto text) RETURNS numeric
                LANGUAGE sql IMMUTABLE AS $f$
                    SELECT coalesce(sum((e ->> 'importeDivisa')::numeric), 0)
                      FROM jsonb_array_elements(coalesce(p_conceptos, '[]'::jsonb)) e
                     WHERE e ->> 'efecto' = p_efecto
                $f$;
                ALTER TABLE facturacion.linea_factura DROP CONSTRAINT ck_linea_factura_base;
                ALTER TABLE facturacion.linea_factura ADD CONSTRAINT ck_linea_factura_base CHECK (
                    (precio_divisa IS NULL AND base_divisa IS NULL AND base = round(cantidad * precio_unitario * (1 - descuento / 100), 2) + importe_conceptos)
                    OR (precio_divisa IS NOT NULL AND base_divisa = round(cantidad * precio_divisa * (1 - descuento / 100), 2)
                        + public.alxor_suma_conceptos_divisa(conceptos, 'Precio')));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE facturacion.linea_factura DROP CONSTRAINT ck_linea_factura_base;
                ALTER TABLE facturacion.linea_factura ADD CONSTRAINT ck_linea_factura_base CHECK (
                    (precio_divisa IS NULL AND base_divisa IS NULL AND base = round(cantidad * precio_unitario * (1 - descuento / 100), 2) + importe_conceptos)
                    OR (precio_divisa IS NOT NULL AND importe_conceptos = 0 AND base_divisa = round(cantidad * precio_divisa * (1 - descuento / 100), 2)));
                DROP FUNCTION IF EXISTS public.alxor_suma_conceptos_divisa(jsonb, text);
                """);
        }
    }
}
