using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Contabilidad.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class PresupuestosContables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "presupuesto_contable",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    desde = table.Column<DateOnly>(type: "date", nullable: false),
                    meses = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_presupuesto_contable", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "linea_presupuesto",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cuenta_codigo = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    centro_id = table.Column<Guid>(type: "uuid", nullable: true),
                    partida_id = table.Column<Guid>(type: "uuid", nullable: true),
                    importes = table.Column<decimal[]>(type: "numeric(14,2)[]", nullable: false),
                    presupuesto_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_presupuesto", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_presupuesto_centro_analitico_centro_id",
                        column: x => x.centro_id,
                        principalSchema: "contabilidad",
                        principalTable: "centro_analitico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_linea_presupuesto_partida_analitica_partida_id",
                        column: x => x.partida_id,
                        principalSchema: "contabilidad",
                        principalTable: "partida_analitica",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_linea_presupuesto_presupuesto_contable_presupuesto_id",
                        column: x => x.presupuesto_id,
                        principalSchema: "contabilidad",
                        principalTable: "presupuesto_contable",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_linea_presupuesto_centro_id",
                schema: "contabilidad",
                table: "linea_presupuesto",
                column: "centro_id");

            migrationBuilder.CreateIndex(
                name: "IX_linea_presupuesto_partida_id",
                schema: "contabilidad",
                table: "linea_presupuesto",
                column: "partida_id");

            migrationBuilder.CreateIndex(
                name: "ix_linea_presupuesto_presupuesto",
                schema: "contabilidad",
                table: "linea_presupuesto",
                column: "presupuesto_id");

            migrationBuilder.CreateIndex(
                name: "ux_presupuesto_contable_empresa_codigo",
                schema: "contabilidad",
                table: "presupuesto_contable",
                columns: new[] { "empresa_id", "codigo" },
                unique: true);

            migrationBuilder.Sql(GarantiasSql.FuncionesComunes);
            migrationBuilder.Sql(RlsSql.Activar("contabilidad", "presupuesto_contable"));
            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("contabilidad", "linea_presupuesto", "presupuesto_id", "contabilidad", "presupuesto_contable"));
            migrationBuilder.Sql("""
                ALTER TABLE contabilidad.presupuesto_contable
                    ADD CONSTRAINT ck_presupuesto_contable_meses CHECK (meses BETWEEN 1 AND 24),
                    ADD CONSTRAINT ck_presupuesto_contable_desde CHECK (extract(day FROM desde) = 1),
                    ADD CONSTRAINT ck_presupuesto_contable_estado CHECK (estado IN ('Borrador', 'Aprobado'));
                ALTER TABLE contabilidad.linea_presupuesto
                    ADD CONSTRAINT ck_linea_presupuesto_cuenta CHECK (cuenta_codigo ~ '^[67][0-9]*$'),
                    ADD CONSTRAINT ck_linea_presupuesto_importes CHECK (cardinality(importes) BETWEEN 1 AND 24 AND 0 <= ALL (importes)
                        AND array_position(importes, NULL) IS NULL);
                CREATE UNIQUE INDEX ux_linea_presupuesto_clave ON contabilidad.linea_presupuesto
                    (presupuesto_id, cuenta_codigo, centro_id, partida_id) NULLS NOT DISTINCT;

                -- Las líneas tienen tantos importes como meses el presupuesto, y un presupuesto aprobado
                -- queda congelado (cabecera y líneas), salvo la baja de la empresa.
                CREATE OR REPLACE FUNCTION contabilidad.linea_presupuesto_valida() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    fila contabilidad.linea_presupuesto := CASE WHEN TG_OP = 'DELETE' THEN OLD ELSE NEW END;
                    cab record;
                BEGIN
                    SELECT meses, estado, empresa_id INTO cab FROM contabilidad.presupuesto_contable WHERE id = fila.presupuesto_id;
                    IF NOT FOUND THEN
                        RETURN fila;   -- la cabecera se está borrando (baja de la empresa)
                    END IF;
                    IF cab.estado = 'Aprobado' AND NOT public.alxor_borrando_empresa(cab.empresa_id) THEN
                        PERFORM public.alxor_error('presupuesto.aprobado', 'El presupuesto está aprobado: sus líneas no se pueden cambiar.');
                    END IF;
                    IF TG_OP <> 'DELETE' AND cardinality(NEW.importes) <> cab.meses THEN
                        PERFORM public.alxor_error('presupuesto.importes', format('La línea %s tiene %s importes y el presupuesto %s meses.', NEW.cuenta_codigo, cardinality(NEW.importes), cab.meses));
                    END IF;
                    RETURN fila;
                END $f$;

                CREATE TRIGGER tg_linea_presupuesto_valida BEFORE INSERT OR UPDATE OR DELETE ON contabilidad.linea_presupuesto
                    FOR EACH ROW EXECUTE FUNCTION contabilidad.linea_presupuesto_valida();

                CREATE OR REPLACE FUNCTION contabilidad.presupuesto_aprobado_inalterable() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF OLD.estado = 'Aprobado' THEN
                        IF TG_OP = 'DELETE' AND public.alxor_borrando_empresa(OLD.empresa_id) THEN
                            RETURN OLD;
                        END IF;
                        PERFORM public.alxor_error('presupuesto.aprobado', 'Un presupuesto aprobado no se modifica ni se borra: cópialo a una versión nueva.');
                    END IF;
                    RETURN CASE WHEN TG_OP = 'DELETE' THEN OLD ELSE NEW END;
                END $f$;

                CREATE TRIGGER tg_presupuesto_contable_aprobado BEFORE UPDATE OR DELETE ON contabilidad.presupuesto_contable
                    FOR EACH ROW EXECUTE FUNCTION contabilidad.presupuesto_aprobado_inalterable();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP FUNCTION IF EXISTS contabilidad.linea_presupuesto_valida() CASCADE;
                DROP FUNCTION IF EXISTS contabilidad.presupuesto_aprobado_inalterable() CASCADE;
                """);

            migrationBuilder.DropTable(
                name: "linea_presupuesto",
                schema: "contabilidad");

            migrationBuilder.DropTable(
                name: "presupuesto_contable",
                schema: "contabilidad");
        }
    }
}
