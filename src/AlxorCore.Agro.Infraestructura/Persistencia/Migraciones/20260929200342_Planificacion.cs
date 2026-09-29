using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Agro.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class Planificacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "plan",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    campana_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    aprobado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "linea_plan",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    desde = table.Column<DateOnly>(type: "date", nullable: false),
                    hasta = table.Column<DateOnly>(type: "date", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kilos = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    agricultor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    parcela_id = table.Column<Guid>(type: "uuid", nullable: true),
                    linea_confeccion = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    cajas = table.Column<int>(type: "integer", nullable: true),
                    precio_kg = table.Column<decimal>(type: "numeric(12,6)", nullable: true),
                    notas = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_plan", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_plan_plan_plan_id",
                        column: x => x.plan_id,
                        principalSchema: "agro",
                        principalTable: "plan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_linea_plan_agricultor",
                schema: "agro",
                table: "linea_plan",
                column: "agricultor_id");

            migrationBuilder.CreateIndex(
                name: "ix_linea_plan_cliente",
                schema: "agro",
                table: "linea_plan",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_linea_plan_parcela",
                schema: "agro",
                table: "linea_plan",
                column: "parcela_id");

            migrationBuilder.CreateIndex(
                name: "ix_linea_plan_plan",
                schema: "agro",
                table: "linea_plan",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_linea_plan_producto",
                schema: "agro",
                table: "linea_plan",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "ix_plan_campana",
                schema: "agro",
                table: "plan",
                column: "campana_id");

            migrationBuilder.CreateIndex(
                name: "ix_plan_tipo_campana",
                schema: "agro",
                table: "plan",
                columns: new[] { "empresa_id", "tipo", "campana_id" });

            migrationBuilder.CreateIndex(
                name: "ux_plan_version",
                schema: "agro",
                table: "plan",
                columns: new[] { "empresa_id", "tipo", "campana_id", "version" },
                unique: true);

            // =============================== Garantías de la base de datos ===============================
            migrationBuilder.Sql(RlsSql.Activar("agro", "plan"));
            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("agro", "linea_plan", "plan_id", "agro", "plan"));
            migrationBuilder.Sql("""
                ALTER TABLE agro.plan
                    ADD CONSTRAINT ck_plan_valores CHECK (tipo IN ('Comercial', 'Produccion', 'Entradas') AND estado IN ('Borrador', 'Aprobado', 'Cerrado')
                        AND version > 0 AND length(trim(nombre)) > 0 AND ((estado = 'Borrador') = (aprobado_en IS NULL) OR estado = 'Cerrado')),
                    ADD CONSTRAINT fk_plan_campana FOREIGN KEY (campana_id) REFERENCES agro.campana (id) DEFERRABLE INITIALLY DEFERRED,
                    -- Un solo plan aprobado por tipo y campaña (comprobado al confirmar: al aprobar uno se cierra el anterior).
                    ADD CONSTRAINT ex_plan_aprobado EXCLUDE USING btree (empresa_id WITH =, tipo WITH =, campana_id WITH =) WHERE (estado = 'Aprobado')
                        DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.linea_plan
                    ADD CONSTRAINT ck_linea_plan_valores CHECK (hasta >= desde AND kilos >= 0 AND (cajas IS NULL OR cajas >= 0) AND (precio_kg IS NULL OR precio_kg >= 0)),
                    ADD CONSTRAINT fk_linea_plan_agricultor FOREIGN KEY (agricultor_id) REFERENCES agro.agricultor (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_linea_plan_parcela FOREIGN KEY (parcela_id) REFERENCES agro.parcela (id) DEFERRABLE INITIALLY DEFERRED;

                -- Un plan no cambia de tipo, campaña ni versión; va de borrador a aprobado y a cerrado; solo se borra en borrador.
                CREATE OR REPLACE FUNCTION agro.plan_valido() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF TG_OP = 'DELETE' THEN
                        IF OLD.estado <> 'Borrador' AND NOT public.alxor_borrando_empresa(OLD.empresa_id) THEN
                            PERFORM public.alxor_error('plan.no_borrador', 'Solo se borra un plan en borrador.');
                        END IF;
                        RETURN OLD;
                    END IF;
                    IF NEW.tipo <> OLD.tipo OR NEW.campana_id <> OLD.campana_id OR NEW.version <> OLD.version OR NEW.empresa_id <> OLD.empresa_id
                       OR (NEW.estado <> OLD.estado AND (OLD.estado || '>' || NEW.estado) NOT IN ('Borrador>Aprobado', 'Aprobado>Cerrado'))
                       OR (OLD.estado <> 'Borrador' AND NEW.nombre <> OLD.nombre) THEN
                        PERFORM public.alxor_error('plan.transicion', 'Un plan aprobado no se cambia: se hace una versión nueva.');
                    END IF;
                    RETURN NEW;
                END $f$;
                CREATE TRIGGER tg_plan_valido BEFORE UPDATE OR DELETE ON agro.plan FOR EACH ROW EXECUTE FUNCTION agro.plan_valido();

                -- Las líneas solo cambian con el plan en borrador.
                CREATE OR REPLACE FUNCTION agro.linea_plan_modificable() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    fila agro.linea_plan%ROWTYPE := CASE WHEN TG_OP = 'DELETE' THEN OLD ELSE NEW END;
                    est text;
                    emp uuid;
                BEGIN
                    SELECT p.estado, p.empresa_id INTO est, emp FROM agro.plan p WHERE p.id = fila.plan_id;
                    IF FOUND AND est <> 'Borrador' AND NOT public.alxor_borrando_empresa(emp) THEN
                        PERFORM public.alxor_error('plan.no_borrador', 'El plan ya está aprobado: haz una versión nueva para cambiarlo.');
                    END IF;
                    RETURN CASE WHEN TG_OP = 'DELETE' THEN OLD ELSE NEW END;
                END $f$;
                CREATE TRIGGER tg_linea_plan_modificable BEFORE INSERT OR UPDATE OR DELETE ON agro.linea_plan
                    FOR EACH ROW EXECUTE FUNCTION agro.linea_plan_modificable();

                -- Al confirmar: dentro de la campaña y con los datos que admite su tipo de plan.
                CREATE OR REPLACE FUNCTION agro.linea_plan_coherente() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF EXISTS (SELECT 1 FROM agro.plan p JOIN agro.campana c ON c.id = p.campana_id
                                WHERE p.id = NEW.plan_id
                                  AND (NEW.desde < c.desde OR NEW.hasta > c.hasta
                                       OR (p.tipo = 'Comercial' AND (NEW.agricultor_id IS NOT NULL OR NEW.parcela_id IS NOT NULL))
                                       OR (p.tipo = 'Produccion' AND (NEW.agricultor_id IS NOT NULL OR NEW.parcela_id IS NOT NULL OR NEW.cliente_id IS NOT NULL))
                                       OR (p.tipo = 'Entradas' AND (NEW.cliente_id IS NOT NULL OR NEW.linea_confeccion IS NOT NULL)))) THEN
                        PERFORM public.alxor_error('plan.linea_incoherente', 'La línea del plan está fuera de la campaña o lleva datos que no son de su tipo de plan.');
                    END IF;
                    RETURN NULL;
                END $f$;
                CREATE CONSTRAINT TRIGGER tg_linea_plan_coherente AFTER INSERT OR UPDATE ON agro.linea_plan
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION agro.linea_plan_coherente();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS tg_linea_plan_coherente ON agro.linea_plan;
                DROP FUNCTION IF EXISTS agro.linea_plan_coherente();
                DROP TRIGGER IF EXISTS tg_linea_plan_modificable ON agro.linea_plan;
                DROP FUNCTION IF EXISTS agro.linea_plan_modificable();
                DROP TRIGGER IF EXISTS tg_plan_valido ON agro.plan;
                DROP FUNCTION IF EXISTS agro.plan_valido();
                """);
            migrationBuilder.Sql(GarantiasSql.QuitarRlsPorPadre("agro", "linea_plan"));


            migrationBuilder.DropTable(
                name: "linea_plan",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "plan",
                schema: "agro");
        }
    }
}
