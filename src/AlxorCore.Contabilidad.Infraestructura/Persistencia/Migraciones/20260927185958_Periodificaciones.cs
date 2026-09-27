using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Contabilidad.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class Periodificaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_asiento_diario_numero",
                schema: "contabilidad",
                table: "asiento");

            migrationBuilder.DropColumn(
                name: "numero_diario",
                schema: "contabilidad",
                table: "asiento");

            migrationBuilder.CreateTable(
                name: "periodificacion",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    cuenta_resultado = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    cuenta_periodificacion = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    ejercicio_inicio = table.Column<int>(type: "integer", nullable: false),
                    mes_inicio = table.Column<int>(type: "integer", nullable: false),
                    meses = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    asiento_reclasificacion_id = table.Column<Guid>(type: "uuid", nullable: true),
                    asiento_cancelacion_id = table.Column<Guid>(type: "uuid", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_periodificacion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cuota_periodificacion",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    mes = table.Column<int>(type: "integer", nullable: false),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    asiento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    periodificacion_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cuota_periodificacion", x => x.id);
                    table.ForeignKey(
                        name: "FK_cuota_periodificacion_periodificacion_periodificacion_id",
                        column: x => x.periodificacion_id,
                        principalSchema: "contabilidad",
                        principalTable: "periodificacion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_asiento_diario",
                schema: "contabilidad",
                table: "asiento",
                columns: new[] { "empresa_id", "ejercicio", "diario", "numero" });

            migrationBuilder.CreateIndex(
                name: "ix_cuota_periodificacion_asiento",
                schema: "contabilidad",
                table: "cuota_periodificacion",
                column: "asiento_id");

            migrationBuilder.CreateIndex(
                name: "ux_cuota_periodificacion_mes",
                schema: "contabilidad",
                table: "cuota_periodificacion",
                columns: new[] { "periodificacion_id", "ejercicio", "mes" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_periodificacion_asiento_cancelacion",
                schema: "contabilidad",
                table: "periodificacion",
                column: "asiento_cancelacion_id");

            migrationBuilder.CreateIndex(
                name: "ix_periodificacion_asiento_reclasificacion",
                schema: "contabilidad",
                table: "periodificacion",
                column: "asiento_reclasificacion_id");

            migrationBuilder.CreateIndex(
                name: "ix_periodificacion_empresa_estado",
                schema: "contabilidad",
                table: "periodificacion",
                columns: new[] { "empresa_id", "estado" });

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("contabilidad", "periodificacion"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("contabilidad", "cuota_periodificacion", "periodificacion_id", "contabilidad", "periodificacion"));
            migrationBuilder.Sql("""
                ALTER TABLE contabilidad.periodificacion
                    ADD CONSTRAINT ck_periodificacion_importe CHECK (importe > 0),
                    ADD CONSTRAINT ck_periodificacion_meses CHECK (meses BETWEEN 2 AND 120),
                    ADD CONSTRAINT fk_periodificacion_asiento_reclasificacion FOREIGN KEY (asiento_reclasificacion_id) REFERENCES contabilidad.asiento (id),
                    ADD CONSTRAINT fk_periodificacion_asiento_cancelacion FOREIGN KEY (asiento_cancelacion_id) REFERENCES contabilidad.asiento (id);
                ALTER TABLE contabilidad.cuota_periodificacion
                    ADD CONSTRAINT ck_cuota_periodificacion_importe CHECK (importe > 0),
                    ADD CONSTRAINT fk_cuota_periodificacion_asiento FOREIGN KEY (asiento_id) REFERENCES contabilidad.asiento (id);
                """);

            // Diario de sistema de las periodificaciones (PER); la lista de diarios de sistema pasa a una función. El número
            // dentro del diario ya no se guarda (se calcula por el orden del asiento en su diario): el trigger solo asigna el
            // diario y comprueba el mes cerrado.
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION contabilidad.diarios_sistema() RETURNS text[]
                LANGUAGE sql IMMUTABLE AS $f$ SELECT ARRAY['GEN', 'VEN', 'COM', 'TES', 'INM', 'PER', 'CIE', 'APE'] $f$;

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Las funciones de diario (con PER) se dejan: siguen siendo válidas sin las periodificaciones.
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.QuitarRlsPorPadre("contabilidad", "cuota_periodificacion"));

            migrationBuilder.DropTable(
                name: "cuota_periodificacion",
                schema: "contabilidad");

            migrationBuilder.DropTable(
                name: "periodificacion",
                schema: "contabilidad");

            migrationBuilder.DropIndex(
                name: "ix_asiento_diario",
                schema: "contabilidad",
                table: "asiento");

            migrationBuilder.AddColumn<int>(
                name: "numero_diario",
                schema: "contabilidad",
                table: "asiento",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Vuelve el número guardado: se rellena por orden y el trigger vuelve a asignarlo.
            migrationBuilder.Sql("""
                ALTER TABLE contabilidad.asiento DISABLE TRIGGER tg_asiento_solo_insercion;
                DO $m$
                DECLARE e record;
                BEGIN
                    -- Empresa a empresa (RLS forzada); sin el esquema de organización, las de los propios asientos.
                    FOR e IN EXECUTE CASE WHEN to_regclass('organizacion.empresa') IS NOT NULL THEN 'SELECT id FROM organizacion.empresa UNION ' ELSE '' END
                                     || 'SELECT DISTINCT empresa_id AS id FROM contabilidad.asiento' LOOP
                        PERFORM set_config('app.empresa_actual', e.id::text, true);
                        UPDATE contabilidad.asiento a SET numero_diario = x.n
                          FROM (SELECT id, row_number() OVER (PARTITION BY empresa_id, ejercicio, diario ORDER BY numero) AS n FROM contabilidad.asiento) x
                         WHERE x.id = a.id;
                    END LOOP;
                    PERFORM set_config('app.empresa_actual', '', true);
                END $m$;
                ALTER TABLE contabilidad.asiento ENABLE TRIGGER tg_asiento_solo_insercion;
                ALTER TABLE contabilidad.asiento ALTER COLUMN numero_diario DROP DEFAULT,
                    ADD CONSTRAINT ck_asiento_numero_diario CHECK (numero_diario >= 1);

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

                """);

            migrationBuilder.CreateIndex(
                name: "ux_asiento_diario_numero",
                schema: "contabilidad",
                table: "asiento",
                columns: new[] { "empresa_id", "ejercicio", "diario", "numero_diario" },
                unique: true);
        }
    }
}
