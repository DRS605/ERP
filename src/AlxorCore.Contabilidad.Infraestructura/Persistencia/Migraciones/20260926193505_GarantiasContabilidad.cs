using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Contabilidad.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class GarantiasContabilidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(GarantiasSql.FuncionesComunes);

            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("contabilidad", "apunte", "asiento_id", "contabilidad", "asiento"));
            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("contabilidad", "dotacion_amortizacion", "inmovilizado_id", "contabilidad", "inmovilizado"));
            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("contabilidad", "ajuste_fiscal_amortizacion", "inmovilizado_id", "contabilidad", "inmovilizado"));

            migrationBuilder.Sql("""
                ALTER TABLE contabilidad.asiento ADD CONSTRAINT ck_asiento_numero CHECK (numero >= 1);
                ALTER TABLE contabilidad.apunte
                    ADD CONSTRAINT ck_apunte_importes CHECK (debe >= 0 AND haber >= 0),
                    ADD CONSTRAINT ck_apunte_un_lado CHECK ((debe > 0) <> (haber > 0));
                """);

            // Un asiento registrado no se modifica ni se borra (se corrige con otro asiento), y sus
            // apuntes solo se insertan al darlo de alta.
            migrationBuilder.Sql(GarantiasSql.MarcarAlta("contabilidad", "asiento"));
            migrationBuilder.Sql(GarantiasSql.SoloInsercion("contabilidad", "asiento",
                "asiento.inalterable", "Un asiento registrado no se puede modificar ni borrar: regístralo corregido con un asiento de rectificación."));
            migrationBuilder.Sql(GarantiasSql.HijaDeAlta("contabilidad", "apunte", "asiento_id", "contabilidad", "asiento",
                "asiento.inalterable", "Un asiento registrado no admite apuntes nuevos."));
            migrationBuilder.Sql(GarantiasSql.SoloInsercion("contabilidad", "apunte",
                "asiento.inalterable", "Los apuntes de un asiento registrado no se pueden modificar ni borrar."));

            // Al confirmar: numeración sin huecos por ejercicio y asiento cuadrado (debe = haber).
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION contabilidad.asiento_comprobar_alta() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    totales record;
                BEGIN
                    IF NEW.numero > 1 AND NOT EXISTS (
                        SELECT 1 FROM contabilidad.asiento
                         WHERE empresa_id = NEW.empresa_id AND ejercicio = NEW.ejercicio AND numero = NEW.numero - 1) THEN
                        PERFORM public.alxor_error('asiento.numeracion', format('El asiento %s de %s deja un hueco en el diario: falta el %s.', NEW.numero, NEW.ejercicio, NEW.numero - 1));
                    END IF;

                    SELECT count(*) AS n, coalesce(sum(debe), 0) AS debe, coalesce(sum(haber), 0) AS haber
                      INTO totales FROM contabilidad.apunte WHERE asiento_id = NEW.id;
                    IF totales.n < 2 OR totales.debe <> totales.haber OR totales.debe = 0 THEN
                        PERFORM public.alxor_error('asiento.descuadrado', format('El asiento %s de %s no cuadra: debe %s, haber %s.', NEW.numero, NEW.ejercicio, totales.debe, totales.haber));
                    END IF;
                    RETURN NULL;
                END $f$;

                CREATE CONSTRAINT TRIGGER tg_asiento_comprobar_alta AFTER INSERT ON contabilidad.asiento
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION contabilidad.asiento_comprobar_alta();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS tg_asiento_comprobar_alta ON contabilidad.asiento;
                DROP FUNCTION IF EXISTS contabilidad.asiento_comprobar_alta();
                """);
            migrationBuilder.Sql(GarantiasSql.QuitarTrigger("contabilidad", "apunte", "tg_apunte_solo_insercion"));
            migrationBuilder.Sql(GarantiasSql.QuitarTrigger("contabilidad", "apunte", "tg_apunte_hija"));
            migrationBuilder.Sql(GarantiasSql.QuitarTrigger("contabilidad", "asiento", "tg_asiento_solo_insercion"));
            migrationBuilder.Sql(GarantiasSql.QuitarTrigger("contabilidad", "asiento", "tg_asiento_alta"));
            migrationBuilder.Sql("""
                ALTER TABLE contabilidad.asiento DROP COLUMN IF EXISTS tx_alta, DROP CONSTRAINT IF EXISTS ck_asiento_numero;
                ALTER TABLE contabilidad.apunte DROP CONSTRAINT IF EXISTS ck_apunte_importes, DROP CONSTRAINT IF EXISTS ck_apunte_un_lado;
                """);
            migrationBuilder.Sql(GarantiasSql.QuitarRlsPorPadre("contabilidad", "ajuste_fiscal_amortizacion"));
            migrationBuilder.Sql(GarantiasSql.QuitarRlsPorPadre("contabilidad", "dotacion_amortizacion"));
            migrationBuilder.Sql(GarantiasSql.QuitarRlsPorPadre("contabilidad", "apunte"));
        }
    }
}
