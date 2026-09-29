using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Agro.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class CuadernoDigitalSiex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "asesor",
                schema: "agro",
                table: "tratamiento_parcela",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "carne_aplicador",
                schema: "agro",
                table: "tratamiento_parcela",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "eficacia",
                schema: "agro",
                table: "tratamiento_parcela",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "equipo_roma",
                schema: "agro",
                table: "tratamiento_parcela",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "metodo_aplicacion",
                schema: "agro",
                table: "tratamiento_parcela",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tipo_fertilizante",
                schema: "agro",
                table: "tratamiento_parcela",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "modo",
                schema: "agro",
                table: "parcela",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "produccion",
                schema: "agro",
                table: "parcela",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Convencional");

            migrationBuilder.AddColumn<string>(
                name: "sistema",
                schema: "agro",
                table: "parcela",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "analisis_agro",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parcela_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    laboratorio = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    boletin = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    nitrogeno_kg_ha = table.Column<decimal>(type: "numeric(10,3)", nullable: true),
                    materia_organica_pct = table.Column<decimal>(type: "numeric(6,3)", nullable: true),
                    ph = table.Column<decimal>(type: "numeric(4,2)", nullable: true),
                    supera_limites = table.Column<bool>(type: "boolean", nullable: true),
                    conclusion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analisis_agro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "explotacion_siex",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agricultor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_regepa = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    asesor_nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    asesor_ropo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    carne_aplicador = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    equipo_roma = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    plan_abonado_obligatorio = table.Column<bool>(type: "boolean", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_explotacion_siex", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plan_abonado",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parcela_id = table.Column<Guid>(type: "uuid", nullable: false),
                    anio = table.Column<int>(type: "integer", nullable: false),
                    produccion_esperada_kg_ha = table.Column<decimal>(type: "numeric(12,3)", nullable: true),
                    nitrogeno_kg_ha = table.Column<decimal>(type: "numeric(10,3)", nullable: false),
                    fosforo_kg_ha = table.Column<decimal>(type: "numeric(10,3)", nullable: false),
                    potasio_kg_ha = table.Column<decimal>(type: "numeric(10,3)", nullable: false),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan_abonado", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_analisis_agro_parcela",
                schema: "agro",
                table: "analisis_agro",
                columns: new[] { "parcela_id", "fecha" });

            migrationBuilder.CreateIndex(
                name: "ux_explotacion_siex_agricultor",
                schema: "agro",
                table: "explotacion_siex",
                column: "agricultor_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_plan_abonado_parcela_anio",
                schema: "agro",
                table: "plan_abonado",
                columns: new[] { "parcela_id", "anio" },
                unique: true);

            // =============================== Garantías de la base de datos ===============================
            foreach (var tabla in new[] { "explotacion_siex", "analisis_agro", "plan_abonado" })
            {
                migrationBuilder.Sql(RlsSql.Activar("agro", tabla));
            }

            migrationBuilder.Sql("""
                ALTER TABLE agro.parcela
                    ADD CONSTRAINT ck_parcela_siex CHECK ((sistema IS NULL OR sistema IN ('Secano', 'Regadio')) AND (modo IS NULL OR modo IN ('AireLibre', 'Invernadero', 'Malla'))
                        AND produccion IN ('Convencional', 'Integrada', 'Ecologica'));
                ALTER TABLE agro.tratamiento_parcela
                    ADD CONSTRAINT ck_tratamiento_parcela_siex CHECK ((eficacia IS NULL OR eficacia IN ('Buena', 'Regular', 'Mala'))
                        AND (tipo_fertilizante IS NULL OR tipo_fertilizante IN ('Mineral', 'Organico', 'OrganoMineral', 'Estiercol', 'Enmienda'))
                        AND (tipo = 'Fitosanitario' OR (carne_aplicador IS NULL AND asesor IS NULL AND eficacia IS NULL))
                        AND (tipo = 'Abonado' OR tipo_fertilizante IS NULL));
                ALTER TABLE agro.explotacion_siex
                    ADD CONSTRAINT fk_explotacion_siex_agricultor FOREIGN KEY (agricultor_id) REFERENCES agro.agricultor (id);
                ALTER TABLE agro.analisis_agro
                    ADD CONSTRAINT fk_analisis_agro_parcela FOREIGN KEY (parcela_id) REFERENCES agro.parcela (id),
                    ADD CONSTRAINT ck_analisis_agro_tipo CHECK (tipo IN ('Suelo', 'Agua', 'Foliar', 'Residuos', 'Abono')),
                    ADD CONSTRAINT ck_analisis_agro_valores CHECK ((nitrogeno_kg_ha IS NULL OR nitrogeno_kg_ha >= 0)
                        AND (materia_organica_pct IS NULL OR materia_organica_pct BETWEEN 0 AND 100) AND (ph IS NULL OR ph BETWEEN 0 AND 14)
                        AND (supera_limites IS NULL OR tipo = 'Residuos'));
                ALTER TABLE agro.plan_abonado
                    ADD CONSTRAINT fk_plan_abonado_parcela FOREIGN KEY (parcela_id) REFERENCES agro.parcela (id),
                    ADD CONSTRAINT ck_plan_abonado_valores CHECK (anio BETWEEN 2000 AND 2100 AND nitrogeno_kg_ha >= 0 AND fosforo_kg_ha >= 0 AND potasio_kg_ha >= 0
                        AND (produccion_esperada_kg_ha IS NULL OR produccion_esperada_kg_ha >= 0));

                -- El cuaderno es un registro: una labor no se borra ni se cambia; solo se anula (sin vuelta atrás) o se anota su eficacia.
                CREATE OR REPLACE FUNCTION agro.tratamiento_parcela_valido() RETURNS trigger LANGUAGE plpgsql AS $f$
                BEGIN
                    IF TG_OP = 'DELETE' THEN
                        IF NOT public.alxor_borrando_empresa(OLD.empresa_id) THEN
                            PERFORM public.alxor_error('tratamiento.inmutable', 'Una labor del cuaderno no se borra: se anula.');
                        END IF;
                        RETURN OLD;
                    END IF;
                    IF (to_jsonb(NEW) - 'anulado' - 'motivo_anulacion' - 'eficacia') <> (to_jsonb(OLD) - 'anulado' - 'motivo_anulacion' - 'eficacia')
                       OR (OLD.anulado AND (NOT NEW.anulado OR NEW.motivo_anulacion IS DISTINCT FROM OLD.motivo_anulacion OR NEW.eficacia IS DISTINCT FROM OLD.eficacia)) THEN
                        PERFORM public.alxor_error('tratamiento.inmutable', 'Una labor del cuaderno no se cambia: se anula y se registra de nuevo (solo se puede anotar su eficacia).');
                    END IF;
                    RETURN NEW;
                END $f$;
                DROP TRIGGER IF EXISTS tg_tratamiento_parcela_valido ON agro.tratamiento_parcela;
                CREATE TRIGGER tg_tratamiento_parcela_valido BEFORE UPDATE OR DELETE ON agro.tratamiento_parcela FOR EACH ROW EXECUTE FUNCTION agro.tratamiento_parcela_valido();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS tg_tratamiento_parcela_valido ON agro.tratamiento_parcela;
                DROP FUNCTION IF EXISTS agro.tratamiento_parcela_valido();
                ALTER TABLE agro.parcela DROP CONSTRAINT IF EXISTS ck_parcela_siex;
                ALTER TABLE agro.tratamiento_parcela DROP CONSTRAINT IF EXISTS ck_tratamiento_parcela_siex;
                """);

            migrationBuilder.DropTable(
                name: "analisis_agro",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "explotacion_siex",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "plan_abonado",
                schema: "agro");

            migrationBuilder.DropColumn(
                name: "asesor",
                schema: "agro",
                table: "tratamiento_parcela");

            migrationBuilder.DropColumn(
                name: "carne_aplicador",
                schema: "agro",
                table: "tratamiento_parcela");

            migrationBuilder.DropColumn(
                name: "eficacia",
                schema: "agro",
                table: "tratamiento_parcela");

            migrationBuilder.DropColumn(
                name: "equipo_roma",
                schema: "agro",
                table: "tratamiento_parcela");

            migrationBuilder.DropColumn(
                name: "metodo_aplicacion",
                schema: "agro",
                table: "tratamiento_parcela");

            migrationBuilder.DropColumn(
                name: "tipo_fertilizante",
                schema: "agro",
                table: "tratamiento_parcela");

            migrationBuilder.DropColumn(
                name: "modo",
                schema: "agro",
                table: "parcela");

            migrationBuilder.DropColumn(
                name: "produccion",
                schema: "agro",
                table: "parcela");

            migrationBuilder.DropColumn(
                name: "sistema",
                schema: "agro",
                table: "parcela");
        }
    }
}
