using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Contabilidad.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class RegularizacionExistencias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cuenta_existencias",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    familia = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cuenta_stock = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    cuenta_variacion = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cuenta_existencias", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_cuenta_existencias_empresa",
                schema: "contabilidad",
                table: "cuenta_existencias",
                column: "empresa_id");
            migrationBuilder.Sql(RlsSql.Activar("contabilidad", "cuenta_existencias"));
            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX ux_cuenta_existencias_familia ON contabilidad.cuenta_existencias (empresa_id, coalesce(lower(familia), ''));
                ALTER TABLE contabilidad.cuenta_existencias
                    ADD CONSTRAINT ck_cuenta_existencias_stock CHECK (cuenta_stock ~ '^3[0-8][0-9]+$'),
                    ADD CONSTRAINT ck_cuenta_existencias_variacion CHECK (cuenta_variacion ~ '^(61|71)[0-9]+$');
                """);

            // La regularización de existencias (origen Existencias) va al diario de cierre y, como la regularización y
            // el cierre del ejercicio, se admite a 31/12 aunque diciembre esté cerrado.
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION contabilidad.diario_de_origen(origen text) RETURNS text
                LANGUAGE sql IMMUTABLE AS $f$
                    SELECT CASE origen
                        WHEN 'Venta' THEN 'VEN'
                        WHEN 'Compra' THEN 'COM'
                        WHEN 'Cobro' THEN 'TES' WHEN 'Pago' THEN 'TES'
                        WHEN 'Amortizacion' THEN 'INM' WHEN 'BajaInmovilizado' THEN 'INM' WHEN 'Enajenacion' THEN 'INM' WHEN 'ImpuestoDiferido' THEN 'INM'
                        WHEN 'Periodificacion' THEN 'PER'
                        WHEN 'Regularizacion' THEN 'CIE' WHEN 'Cierre' THEN 'CIE' WHEN 'Existencias' THEN 'CIE'
                        WHEN 'Apertura' THEN 'APE'
                        ELSE 'GEN' END
                $f$;

                CREATE OR REPLACE FUNCTION contabilidad.asiento_diario() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    cerrado date;
                BEGIN
                    IF coalesce(NEW.diario, '') = '' THEN
                        NEW.diario := NULL;
                        IF NEW.anula_asiento_id IS NOT NULL THEN
                            SELECT diario INTO NEW.diario FROM contabilidad.asiento WHERE id = NEW.anula_asiento_id;
                        END IF;
                        IF NEW.diario IS NULL THEN
                            SELECT codigo INTO NEW.diario FROM contabilidad.diario_contable
                             WHERE empresa_id = NEW.empresa_id AND activo AND NEW.origen = ANY (origenes) ORDER BY codigo LIMIT 1;
                        END IF;
                        IF NEW.diario IS NULL THEN
                            NEW.diario := contabilidad.diario_de_origen(NEW.origen);
                        END IF;
                    ELSIF NEW.diario <> ALL (contabilidad.diarios_sistema()) AND NOT EXISTS (
                        SELECT 1 FROM contabilidad.diario_contable WHERE empresa_id = NEW.empresa_id AND codigo = NEW.diario AND activo) THEN
                        PERFORM public.alxor_error('asiento.diario', format('El diario «%s» no existe o está de baja.', NEW.diario));
                    END IF;

                    IF NEW.origen NOT IN ('Regularizacion', 'Cierre', 'Existencias') THEN
                        SELECT cerrado_hasta INTO cerrado FROM contabilidad.config_contabilidad WHERE empresa_id = NEW.empresa_id;
                        IF cerrado IS NOT NULL AND NEW.fecha <= cerrado THEN
                            PERFORM public.alxor_error('asiento.periodo_cerrado',
                                format('El mes de %s está cerrado (cerrado hasta el %s): usa una fecha posterior o reabre el mes.', to_char(NEW.fecha, 'MM/YYYY'), to_char(cerrado, 'DD/MM/YYYY')));
                        END IF;
                    END IF;
                    RETURN NEW;
                END $f$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION contabilidad.diario_de_origen(origen text) RETURNS text
                LANGUAGE sql IMMUTABLE AS $f$
                    SELECT CASE origen
                        WHEN 'Venta' THEN 'VEN'
                        WHEN 'Compra' THEN 'COM'
                        WHEN 'Cobro' THEN 'TES' WHEN 'Pago' THEN 'TES'
                        WHEN 'Amortizacion' THEN 'INM' WHEN 'BajaInmovilizado' THEN 'INM' WHEN 'Enajenacion' THEN 'INM' WHEN 'ImpuestoDiferido' THEN 'INM'
                        WHEN 'Periodificacion' THEN 'PER'
                        WHEN 'Regularizacion' THEN 'CIE' WHEN 'Cierre' THEN 'CIE'
                        WHEN 'Apertura' THEN 'APE'
                        ELSE 'GEN' END
                $f$;

                CREATE OR REPLACE FUNCTION contabilidad.asiento_diario() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    cerrado date;
                BEGIN
                    IF coalesce(NEW.diario, '') = '' THEN
                        NEW.diario := NULL;
                        IF NEW.anula_asiento_id IS NOT NULL THEN
                            SELECT diario INTO NEW.diario FROM contabilidad.asiento WHERE id = NEW.anula_asiento_id;
                        END IF;
                        IF NEW.diario IS NULL THEN
                            SELECT codigo INTO NEW.diario FROM contabilidad.diario_contable
                             WHERE empresa_id = NEW.empresa_id AND activo AND NEW.origen = ANY (origenes) ORDER BY codigo LIMIT 1;
                        END IF;
                        IF NEW.diario IS NULL THEN
                            NEW.diario := contabilidad.diario_de_origen(NEW.origen);
                        END IF;
                    ELSIF NEW.diario <> ALL (contabilidad.diarios_sistema()) AND NOT EXISTS (
                        SELECT 1 FROM contabilidad.diario_contable WHERE empresa_id = NEW.empresa_id AND codigo = NEW.diario AND activo) THEN
                        PERFORM public.alxor_error('asiento.diario', format('El diario «%s» no existe o está de baja.', NEW.diario));
                    END IF;

                    IF NEW.origen NOT IN ('Regularizacion', 'Cierre') THEN
                        SELECT cerrado_hasta INTO cerrado FROM contabilidad.config_contabilidad WHERE empresa_id = NEW.empresa_id;
                        IF cerrado IS NOT NULL AND NEW.fecha <= cerrado THEN
                            PERFORM public.alxor_error('asiento.periodo_cerrado',
                                format('El mes de %s está cerrado (cerrado hasta el %s): usa una fecha posterior o reabre el mes.', to_char(NEW.fecha, 'MM/YYYY'), to_char(cerrado, 'DD/MM/YYYY')));
                        END IF;
                    END IF;
                    RETURN NEW;
                END $f$;
                """);
            migrationBuilder.Sql(RlsSql.Desactivar("contabilidad", "cuenta_existencias"));

            migrationBuilder.DropTable(
                name: "cuenta_existencias",
                schema: "contabilidad");
        }
    }
}
