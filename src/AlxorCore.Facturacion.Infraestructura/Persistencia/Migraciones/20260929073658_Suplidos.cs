using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Facturacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class Suplidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "suplidos_conceptos",
                schema: "facturacion",
                table: "linea_factura",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "suplidos",
                schema: "facturacion",
                table: "factura",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);
            migrationBuilder.Sql("""
                ALTER TABLE facturacion.factura DROP CONSTRAINT ck_factura_total;
                ALTER TABLE facturacion.factura ADD CONSTRAINT ck_factura_total CHECK (total = base_imponible + cuota_iva + recargo_total - retencion_irpf + suplidos),
                    ADD CONSTRAINT ck_factura_suplidos CHECK (suplidos >= 0);
                ALTER TABLE facturacion.linea_factura ADD CONSTRAINT ck_linea_factura_suplidos CHECK (suplidos_conceptos = public.alxor_suma_conceptos(conceptos, 'Suplido'));

                CREATE OR REPLACE FUNCTION facturacion.factura_comprobar_alta() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    anterior date;
                    lineas record;
                BEGIN
                    IF NEW.numero > 1 THEN
                        SELECT fecha_emision INTO anterior FROM facturacion.factura
                         WHERE empresa_id = NEW.empresa_id AND prefijo = NEW.prefijo AND ejercicio = NEW.ejercicio AND numero = NEW.numero - 1;
                        IF NOT FOUND THEN
                            PERFORM public.alxor_error('factura.numeracion', format('La factura %s deja un hueco en la numeración: falta la número %s.', NEW.numero_completo, NEW.numero - 1));
                        END IF;
                        IF NEW.fecha_emision < anterior THEN
                            PERFORM public.alxor_error('factura.fecha_no_correlativa', format('La factura %s tiene fecha anterior (%s) a la de la factura anterior de su serie (%s).', NEW.numero_completo, NEW.fecha_emision, anterior));
                        END IF;
                    END IF;

                    SELECT count(*) AS n, coalesce(sum(base), 0) AS base, coalesce(sum(cuota_iva), 0) AS cuota, coalesce(sum(cuota_recargo), 0) AS recargo, coalesce(sum(suplidos_conceptos), 0) AS suplidos
                      INTO lineas FROM facturacion.linea_factura WHERE factura_id = NEW.id;
                    IF lineas.n = 0 THEN
                        PERFORM public.alxor_error('factura.sin_lineas', format('La factura %s no tiene líneas.', NEW.numero_completo));
                    END IF;
                    IF (lineas.base, lineas.cuota, lineas.recargo, lineas.suplidos) IS DISTINCT FROM (NEW.base_imponible, NEW.cuota_iva, NEW.recargo_total, NEW.suplidos) THEN
                        PERFORM public.alxor_error('factura.descuadrada', format('Los importes de la factura %s no coinciden con la suma de sus líneas.', NEW.numero_completo));
                    END IF;
                    RETURN NULL;
                END $f$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE facturacion.linea_factura DROP CONSTRAINT IF EXISTS ck_linea_factura_suplidos;
                ALTER TABLE facturacion.factura DROP CONSTRAINT IF EXISTS ck_factura_suplidos;
                ALTER TABLE facturacion.factura DROP CONSTRAINT ck_factura_total;
                ALTER TABLE facturacion.factura ADD CONSTRAINT ck_factura_total CHECK (total = base_imponible + cuota_iva + recargo_total - retencion_irpf);

                CREATE OR REPLACE FUNCTION facturacion.factura_comprobar_alta() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    anterior date;
                    lineas record;
                BEGIN
                    IF NEW.numero > 1 THEN
                        SELECT fecha_emision INTO anterior FROM facturacion.factura
                         WHERE empresa_id = NEW.empresa_id AND prefijo = NEW.prefijo AND ejercicio = NEW.ejercicio AND numero = NEW.numero - 1;
                        IF NOT FOUND THEN
                            PERFORM public.alxor_error('factura.numeracion', format('La factura %s deja un hueco en la numeración: falta la número %s.', NEW.numero_completo, NEW.numero - 1));
                        END IF;
                        IF NEW.fecha_emision < anterior THEN
                            PERFORM public.alxor_error('factura.fecha_no_correlativa', format('La factura %s tiene fecha anterior (%s) a la de la factura anterior de su serie (%s).', NEW.numero_completo, NEW.fecha_emision, anterior));
                        END IF;
                    END IF;

                    SELECT count(*) AS n, coalesce(sum(base), 0) AS base, coalesce(sum(cuota_iva), 0) AS cuota, coalesce(sum(cuota_recargo), 0) AS recargo
                      INTO lineas FROM facturacion.linea_factura WHERE factura_id = NEW.id;
                    IF lineas.n = 0 THEN
                        PERFORM public.alxor_error('factura.sin_lineas', format('La factura %s no tiene líneas.', NEW.numero_completo));
                    END IF;
                    IF (lineas.base, lineas.cuota, lineas.recargo) IS DISTINCT FROM (NEW.base_imponible, NEW.cuota_iva, NEW.recargo_total) THEN
                        PERFORM public.alxor_error('factura.descuadrada', format('Los importes de la factura %s no coinciden con la suma de sus líneas.', NEW.numero_completo));
                    END IF;
                    RETURN NULL;
                END $f$;
                """);
            migrationBuilder.DropColumn(
                name: "suplidos_conceptos",
                schema: "facturacion",
                table: "linea_factura");

            migrationBuilder.DropColumn(
                name: "suplidos",
                schema: "facturacion",
                table: "factura");
        }
    }
}
