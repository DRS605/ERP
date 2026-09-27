using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Contabilidad.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class DiariosYCierreMensual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "cerrado_hasta",
                schema: "contabilidad",
                table: "config_contabilidad",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "diario",
                schema: "contabilidad",
                table: "asiento",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "numero_diario",
                schema: "contabilidad",
                table: "asiento",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "diario_contable",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    origenes = table.Column<List<string>>(type: "text[]", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_diario_contable", x => x.id);
                });



            migrationBuilder.CreateIndex(
                name: "ux_diario_empresa_codigo",
                schema: "contabilidad",
                table: "diario_contable",
                columns: new[] { "empresa_id", "codigo" },
                unique: true);
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("contabilidad", "diario_contable"));

            // Diario de sistema de cada origen (debe coincidir con DiariosContables.Sistema).
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION contabilidad.diario_de_origen(origen text) RETURNS text
                LANGUAGE sql IMMUTABLE AS $f$
                    SELECT CASE origen
                        WHEN 'Venta' THEN 'VEN'
                        WHEN 'Compra' THEN 'COM'
                        WHEN 'Cobro' THEN 'TES' WHEN 'Pago' THEN 'TES'
                        WHEN 'Amortizacion' THEN 'INM' WHEN 'BajaInmovilizado' THEN 'INM' WHEN 'Enajenacion' THEN 'INM' WHEN 'ImpuestoDiferido' THEN 'INM'
                        WHEN 'Regularizacion' THEN 'CIE' WHEN 'Cierre' THEN 'CIE'
                        WHEN 'Apertura' THEN 'APE'
                        ELSE 'GEN' END
                $f$;
                """);

            // Asientos existentes: diario por su origen (el contraasiento, el del asiento que anula) y su número en él.
            migrationBuilder.Sql("""
                ALTER TABLE contabilidad.asiento DISABLE TRIGGER tg_asiento_solo_insercion;
                DO $m$
                DECLARE e record;
                BEGIN
                    IF to_regclass('organizacion.empresa') IS NULL THEN RETURN; END IF;
                    FOR e IN SELECT id FROM organizacion.empresa LOOP
                        PERFORM set_config('app.empresa_actual', e.id::text, true);
                        UPDATE contabilidad.asiento SET diario = contabilidad.diario_de_origen(origen) WHERE diario IS NULL;
                        UPDATE contabilidad.asiento a SET diario = o.diario FROM contabilidad.asiento o WHERE a.anula_asiento_id = o.id;
                        UPDATE contabilidad.asiento a SET numero_diario = x.n
                          FROM (SELECT id, row_number() OVER (PARTITION BY empresa_id, ejercicio, diario ORDER BY numero) AS n FROM contabilidad.asiento) x
                         WHERE x.id = a.id;
                    END LOOP;
                    PERFORM set_config('app.empresa_actual', '', true);
                END $m$;
                ALTER TABLE contabilidad.asiento ENABLE TRIGGER tg_asiento_solo_insercion;
                ALTER TABLE contabilidad.asiento ALTER COLUMN diario SET NOT NULL, ALTER COLUMN numero_diario DROP DEFAULT;
                """);

            migrationBuilder.CreateIndex(
                name: "ux_asiento_diario_numero",
                schema: "contabilidad",
                table: "asiento",
                columns: new[] { "empresa_id", "ejercicio", "diario", "numero_diario" },
                unique: true);

            // Al dar de alta un asiento: su diario (el indicado, el del asiento que anula, el propio que recoge su
            // origen o el de sistema), su número en el diario, y que su fecha no esté en un mes cerrado (salvo la
            // regularización y el cierre del ejercicio). El número es correlativo porque el alta ya tiene el bloqueo
            // del ejercicio (SiguienteNumeroAsync).
            migrationBuilder.Sql("""
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
                    ELSIF NEW.diario NOT IN ('GEN', 'VEN', 'COM', 'TES', 'INM', 'CIE', 'APE') AND NOT EXISTS (
                        SELECT 1 FROM contabilidad.diario_contable WHERE empresa_id = NEW.empresa_id AND codigo = NEW.diario AND activo) THEN
                        PERFORM public.alxor_error('asiento.diario', format('El diario «%s» no existe o está de baja.', NEW.diario));
                    END IF;

                    SELECT coalesce(max(numero_diario), 0) + 1 INTO NEW.numero_diario FROM contabilidad.asiento
                     WHERE empresa_id = NEW.empresa_id AND ejercicio = NEW.ejercicio AND diario = NEW.diario;

                    IF NEW.origen NOT IN ('Regularizacion', 'Cierre') THEN
                        SELECT cerrado_hasta INTO cerrado FROM contabilidad.config_contabilidad WHERE empresa_id = NEW.empresa_id;
                        IF cerrado IS NOT NULL AND NEW.fecha <= cerrado THEN
                            PERFORM public.alxor_error('asiento.periodo_cerrado',
                                format('El mes de %s está cerrado (cerrado hasta el %s): usa una fecha posterior o reabre el mes.', to_char(NEW.fecha, 'MM/YYYY'), to_char(cerrado, 'DD/MM/YYYY')));
                        END IF;
                    END IF;
                    RETURN NEW;
                END $f$;

                CREATE TRIGGER tg_asiento_diario BEFORE INSERT ON contabilidad.asiento
                    FOR EACH ROW EXECUTE FUNCTION contabilidad.asiento_diario();
                ALTER TABLE contabilidad.asiento ADD CONSTRAINT ck_asiento_numero_diario CHECK (numero_diario >= 1);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS tg_asiento_diario ON contabilidad.asiento;
                DROP FUNCTION IF EXISTS contabilidad.asiento_diario();
                DROP FUNCTION IF EXISTS contabilidad.diario_de_origen(text);
                ALTER TABLE contabilidad.asiento DROP CONSTRAINT IF EXISTS ck_asiento_numero_diario;
                """);

            migrationBuilder.DropTable(
                name: "diario_contable",
                schema: "contabilidad");

            migrationBuilder.DropIndex(
                name: "ux_asiento_diario_numero",
                schema: "contabilidad",
                table: "asiento");

            migrationBuilder.DropColumn(
                name: "cerrado_hasta",
                schema: "contabilidad",
                table: "config_contabilidad");

            migrationBuilder.DropColumn(
                name: "diario",
                schema: "contabilidad",
                table: "asiento");

            migrationBuilder.DropColumn(
                name: "numero_diario",
                schema: "contabilidad",
                table: "asiento");
        }
    }
}
