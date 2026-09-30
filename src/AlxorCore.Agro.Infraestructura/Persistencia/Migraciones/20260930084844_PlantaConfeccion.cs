using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Agro.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class PlantaConfeccion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "calibrado",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    linea_id = table.Column<Guid>(type: "uuid", nullable: false),
                    partida_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    kilos_entrada = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    referencia = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    clasificacion_id = table.Column<Guid>(type: "uuid", nullable: true),
                    motivo_anulacion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calibrado", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "linea_planta",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    capacidad_kg_hora = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    horas_turno = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    turnos = table.Column<int>(type: "integer", nullable: false),
                    centro_analitico_id = table.Column<Guid>(type: "uuid", nullable: true),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_planta", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "orden_linea",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    linea_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    turno = table.Column<int>(type: "integer", nullable: false),
                    secuencia = table.Column<int>(type: "integer", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kilos = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    cajas = table.Column<int>(type: "integer", nullable: true),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    pedido_venta_id = table.Column<Guid>(type: "uuid", nullable: true),
                    linea_plan_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notas = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    parte_confeccion_id = table.Column<Guid>(type: "uuid", nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orden_linea", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "parada_linea",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    linea_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    turno = table.Column<int>(type: "integer", nullable: false),
                    minutos = table.Column<decimal>(type: "numeric(7,2)", nullable: false),
                    motivo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    notas = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parada_linea", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "linea_calibrado",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    calibre = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    categoria_id = table.Column<Guid>(type: "uuid", nullable: true),
                    destrio = table.Column<bool>(type: "boolean", nullable: false),
                    kilos = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    piezas = table.Column<int>(type: "integer", nullable: true),
                    calibrado_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_calibrado", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_calibrado_calibrado_calibrado_id",
                        column: x => x.calibrado_id,
                        principalSchema: "agro",
                        principalTable: "calibrado",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "salida_calibradora",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    calibre = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    categoria_id = table.Column<Guid>(type: "uuid", nullable: true),
                    destrio = table.Column<bool>(type: "boolean", nullable: false),
                    linea_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_salida_calibradora", x => x.id);
                    table.ForeignKey(
                        name: "FK_salida_calibradora_linea_planta_linea_id",
                        column: x => x.linea_id,
                        principalSchema: "agro",
                        principalTable: "linea_planta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_calibrado_clasificacion",
                schema: "agro",
                table: "calibrado",
                column: "clasificacion_id");

            migrationBuilder.CreateIndex(
                name: "ix_calibrado_fecha",
                schema: "agro",
                table: "calibrado",
                columns: new[] { "empresa_id", "fecha" });

            migrationBuilder.CreateIndex(
                name: "ix_calibrado_linea",
                schema: "agro",
                table: "calibrado",
                column: "linea_id");

            migrationBuilder.CreateIndex(
                name: "ix_calibrado_partida",
                schema: "agro",
                table: "calibrado",
                column: "partida_id");

            migrationBuilder.CreateIndex(
                name: "ix_linea_calibrado_calibrado",
                schema: "agro",
                table: "linea_calibrado",
                column: "calibrado_id");

            migrationBuilder.CreateIndex(
                name: "ix_linea_calibrado_categoria",
                schema: "agro",
                table: "linea_calibrado",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "ux_linea_planta_codigo",
                schema: "agro",
                table: "linea_planta",
                columns: new[] { "empresa_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_orden_linea_cliente",
                schema: "agro",
                table: "orden_linea",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_orden_linea_fecha",
                schema: "agro",
                table: "orden_linea",
                columns: new[] { "empresa_id", "fecha" });

            migrationBuilder.CreateIndex(
                name: "ix_orden_linea_linea",
                schema: "agro",
                table: "orden_linea",
                column: "linea_id");

            migrationBuilder.CreateIndex(
                name: "ix_orden_linea_pedido",
                schema: "agro",
                table: "orden_linea",
                column: "pedido_venta_id");

            migrationBuilder.CreateIndex(
                name: "ix_orden_linea_plan",
                schema: "agro",
                table: "orden_linea",
                column: "linea_plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_orden_linea_producto",
                schema: "agro",
                table: "orden_linea",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "ux_orden_linea_parte",
                schema: "agro",
                table: "orden_linea",
                column: "parte_confeccion_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_parada_linea_fecha",
                schema: "agro",
                table: "parada_linea",
                columns: new[] { "empresa_id", "fecha" });

            migrationBuilder.CreateIndex(
                name: "ix_parada_linea_linea",
                schema: "agro",
                table: "parada_linea",
                column: "linea_id");

            migrationBuilder.CreateIndex(
                name: "ix_salida_calibradora_categoria",
                schema: "agro",
                table: "salida_calibradora",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "ux_salida_calibradora_numero",
                schema: "agro",
                table: "salida_calibradora",
                columns: new[] { "linea_id", "numero" },
                unique: true);

            foreach (var tabla in new[] { "linea_planta", "calibrado", "orden_linea", "parada_linea" })
            {
                migrationBuilder.Sql(RlsSql.Activar("agro", tabla));
            }

            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("agro", "salida_calibradora", "linea_id", "agro", "linea_planta"));
            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("agro", "linea_calibrado", "calibrado_id", "agro", "calibrado"));
            migrationBuilder.Sql("""
                ALTER TABLE agro.linea_planta
                    ADD CONSTRAINT ck_linea_planta_valores CHECK (tipo IN ('Calibradora', 'Confeccion', 'Envasado') AND length(trim(codigo)) > 0
                        AND capacidad_kg_hora > 0 AND horas_turno > 0 AND turnos BETWEEN 1 AND 3 AND horas_turno * turnos <= 24);
                ALTER TABLE agro.salida_calibradora
                    ADD CONSTRAINT ck_salida_calibradora_valores CHECK (numero BETWEEN 1 AND 999 AND (calibre IS NOT NULL OR categoria_id IS NOT NULL OR destrio)),
                    ADD CONSTRAINT fk_salida_calibradora_categoria FOREIGN KEY (categoria_id) REFERENCES agro.categoria (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.calibrado
                    ADD CONSTRAINT ck_calibrado_valores CHECK (kilos_entrada > 0 AND estado IN ('Borrador', 'Confirmado', 'Anulado')
                        AND ((estado = 'Anulado') = (motivo_anulacion IS NOT NULL)) AND (clasificacion_id IS NULL OR estado <> 'Borrador')),
                    ADD CONSTRAINT fk_calibrado_linea FOREIGN KEY (linea_id) REFERENCES agro.linea_planta (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_calibrado_partida FOREIGN KEY (partida_id) REFERENCES agro.partida (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_calibrado_clasificacion FOREIGN KEY (clasificacion_id) REFERENCES agro.clasificacion (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.linea_calibrado
                    ADD CONSTRAINT ck_linea_calibrado_valores CHECK (kilos >= 0 AND (piezas IS NULL OR piezas >= 0) AND (calibre IS NOT NULL OR categoria_id IS NOT NULL OR destrio)),
                    ADD CONSTRAINT fk_linea_calibrado_categoria FOREIGN KEY (categoria_id) REFERENCES agro.categoria (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.orden_linea
                    ADD CONSTRAINT ck_orden_linea_valores CHECK (kilos > 0 AND turno BETWEEN 1 AND 3 AND secuencia >= 0 AND (cajas IS NULL OR cajas >= 0)
                        AND estado IN ('Planificada', 'EnCurso', 'Terminada', 'Cancelada') AND (parte_confeccion_id IS NULL OR estado = 'Terminada')),
                    ADD CONSTRAINT fk_orden_linea_linea FOREIGN KEY (linea_id) REFERENCES agro.linea_planta (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_orden_linea_parte FOREIGN KEY (parte_confeccion_id) REFERENCES agro.parte_confeccion (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_orden_linea_plan FOREIGN KEY (linea_plan_id) REFERENCES agro.linea_plan (id) ON DELETE SET NULL DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.parada_linea
                    ADD CONSTRAINT ck_parada_linea_valores CHECK (minutos > 0 AND turno BETWEEN 1 AND 3
                        AND motivo IN ('Averia', 'Limpieza', 'CambioFormato', 'FaltaFruta', 'FaltaPersonal', 'Mantenimiento', 'Otro')),
                    ADD CONSTRAINT fk_parada_linea_linea FOREIGN KEY (linea_id) REFERENCES agro.linea_planta (id) DEFERRABLE INITIALLY DEFERRED;

                -- Un calibrado va de borrador a confirmado o anulado, y de confirmado a anulado; confirmado, solo cambia al anularse.
                CREATE OR REPLACE FUNCTION agro.calibrado_valido() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF TG_OP = 'DELETE' THEN
                        IF NOT public.alxor_borrando_empresa(OLD.empresa_id) THEN
                            PERFORM public.alxor_error('calibrado.no_se_borra', 'Un calibrado no se borra: se anula.');
                        END IF;
                        RETURN OLD;
                    END IF;
                    IF NEW.partida_id <> OLD.partida_id OR NEW.linea_id <> OLD.linea_id OR NEW.empresa_id <> OLD.empresa_id
                       OR (NEW.estado <> OLD.estado AND (OLD.estado || '>' || NEW.estado) NOT IN ('Borrador>Confirmado', 'Borrador>Anulado', 'Confirmado>Anulado'))
                       OR (OLD.estado <> 'Borrador' AND (NEW.fecha <> OLD.fecha OR NEW.kilos_entrada <> OLD.kilos_entrada
                           OR NEW.clasificacion_id IS DISTINCT FROM OLD.clasificacion_id)) THEN
                        PERFORM public.alxor_error('calibrado.transicion', 'Un calibrado confirmado no se cambia: se anula y se hace otro.');
                    END IF;
                    RETURN NEW;
                END $f$;
                CREATE TRIGGER tg_calibrado_valido BEFORE UPDATE OR DELETE ON agro.calibrado FOR EACH ROW EXECUTE FUNCTION agro.calibrado_valido();

                CREATE OR REPLACE FUNCTION agro.linea_calibrado_modificable() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    padre uuid := CASE WHEN TG_OP = 'DELETE' THEN OLD.calibrado_id ELSE NEW.calibrado_id END;
                    est text;
                    emp uuid;
                BEGIN
                    SELECT c.estado, c.empresa_id INTO est, emp FROM agro.calibrado c WHERE c.id = padre;
                    IF FOUND AND est <> 'Borrador' AND NOT public.alxor_borrando_empresa(emp) THEN
                        PERFORM public.alxor_error('calibrado.no_borrador', 'Las salidas de un calibrado confirmado no se cambian.');
                    END IF;
                    RETURN CASE WHEN TG_OP = 'DELETE' THEN OLD ELSE NEW END;
                END $f$;
                CREATE TRIGGER tg_linea_calibrado_modificable BEFORE INSERT OR UPDATE OR DELETE ON agro.linea_calibrado
                    FOR EACH ROW EXECUTE FUNCTION agro.linea_calibrado_modificable();

                -- Al confirmar la transacción: lo que sale no pasa de lo que entró, lo que entró no pasa de la partida, la línea es una
                -- calibradora y la clasificación es de la misma partida.
                CREATE OR REPLACE FUNCTION agro.calibrado_cuadra() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    fila jsonb := to_jsonb(CASE WHEN TG_OP = 'DELETE' THEN OLD ELSE NEW END);
                    id_calibrado uuid := CASE WHEN TG_TABLE_NAME = 'calibrado' THEN (fila->>'id')::uuid ELSE (fila->>'calibrado_id')::uuid END;
                BEGIN
                    IF EXISTS (SELECT 1 FROM agro.calibrado c
                                JOIN agro.partida p ON p.id = c.partida_id
                                JOIN agro.linea_planta l ON l.id = c.linea_id
                                WHERE c.id = id_calibrado
                                  AND (c.kilos_entrada > p.kilos_iniciales OR l.tipo <> 'Calibradora'
                                       OR (SELECT coalesce(sum(x.kilos), 0) FROM agro.linea_calibrado x WHERE x.calibrado_id = c.id) > c.kilos_entrada
                                       OR (c.clasificacion_id IS NOT NULL AND NOT EXISTS (
                                           SELECT 1 FROM agro.clasificacion k WHERE k.id = c.clasificacion_id AND k.partida_id = c.partida_id)))) THEN
                        PERFORM public.alxor_error('calibrado.no_cuadra',
                            'El calibrado no cuadra: sale más de lo que entró, entra más que la partida, la línea no es una calibradora o la clasificación es de otra partida.');
                    END IF;
                    RETURN NULL;
                END $f$;
                CREATE CONSTRAINT TRIGGER tg_calibrado_cuadra AFTER INSERT OR UPDATE ON agro.calibrado
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION agro.calibrado_cuadra();
                CREATE CONSTRAINT TRIGGER tg_linea_calibrado_cuadra AFTER INSERT OR UPDATE ON agro.linea_calibrado
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION agro.calibrado_cuadra();

                -- Transiciones de la orden; sus datos solo cambian planificada; el parte que la cierra está validado y no es anterior.
                CREATE OR REPLACE FUNCTION agro.orden_linea_valida() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF TG_OP = 'DELETE' THEN
                        IF OLD.estado <> 'Planificada' AND NOT public.alxor_borrando_empresa(OLD.empresa_id) THEN
                            PERFORM public.alxor_error('orden_linea.no_planificada', 'Solo se borra una orden planificada; la empezada se cancela.');
                        END IF;
                        RETURN OLD;
                    END IF;
                    IF TG_OP = 'UPDATE' THEN
                        IF NEW.empresa_id <> OLD.empresa_id
                           OR (NEW.estado <> OLD.estado AND (OLD.estado || '>' || NEW.estado) NOT IN ('Planificada>EnCurso', 'Planificada>Terminada', 'Planificada>Cancelada',
                               'EnCurso>Terminada', 'EnCurso>Cancelada', 'EnCurso>Planificada', 'Terminada>Planificada'))
                           OR (OLD.estado <> 'Planificada' AND (NEW.linea_id <> OLD.linea_id OR NEW.fecha <> OLD.fecha OR NEW.turno <> OLD.turno
                               OR NEW.producto_id <> OLD.producto_id OR NEW.kilos <> OLD.kilos)) THEN
                            PERFORM public.alxor_error('orden_linea.transicion', 'La orden ya empezó: no se cambia ni se mueve.');
                        END IF;
                    END IF;
                    IF NEW.parte_confeccion_id IS NOT NULL AND (TG_OP = 'INSERT' OR NEW.parte_confeccion_id IS DISTINCT FROM OLD.parte_confeccion_id)
                       AND NOT EXISTS (SELECT 1 FROM agro.parte_confeccion p WHERE p.id = NEW.parte_confeccion_id AND p.estado = 'Validado' AND p.fecha >= NEW.fecha) THEN
                        PERFORM public.alxor_error('orden_linea.parte', 'El parte que cierra la orden tiene que estar validado y no ser anterior a ella.');
                    END IF;
                    RETURN NEW;
                END $f$;
                CREATE TRIGGER tg_orden_linea_valida BEFORE INSERT OR UPDATE OR DELETE ON agro.orden_linea FOR EACH ROW EXECUTE FUNCTION agro.orden_linea_valida();

                -- El turno de la orden o la parada existe en su línea, y las paradas de un turno no pasan de su duración.
                CREATE OR REPLACE FUNCTION agro.turno_linea_valido() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    l agro.linea_planta%ROWTYPE;
                BEGIN
                    SELECT * INTO l FROM agro.linea_planta WHERE id = NEW.linea_id;
                    IF FOUND AND NEW.turno > l.turnos THEN
                        PERFORM public.alxor_error('linea_planta.turno', format('La línea %s trabaja %s turno(s).', l.codigo, l.turnos));
                    END IF;
                    IF TG_TABLE_NAME = 'parada_linea' AND FOUND AND (SELECT coalesce(sum(p.minutos), 0) FROM agro.parada_linea p
                           WHERE p.linea_id = NEW.linea_id AND p.fecha = NEW.fecha AND p.turno = NEW.turno) > l.horas_turno * 60 THEN
                        PERFORM public.alxor_error('parada.minutos', 'Las paradas del turno pasan de su duración.');
                    END IF;
                    RETURN NULL;
                END $f$;
                CREATE CONSTRAINT TRIGGER tg_orden_linea_turno AFTER INSERT OR UPDATE ON agro.orden_linea
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION agro.turno_linea_valido();
                CREATE CONSTRAINT TRIGGER tg_parada_linea_turno AFTER INSERT OR UPDATE ON agro.parada_linea
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION agro.turno_linea_valido();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS tg_parada_linea_turno ON agro.parada_linea;
                DROP TRIGGER IF EXISTS tg_orden_linea_turno ON agro.orden_linea;
                DROP FUNCTION IF EXISTS agro.turno_linea_valido();
                DROP TRIGGER IF EXISTS tg_orden_linea_valida ON agro.orden_linea;
                DROP FUNCTION IF EXISTS agro.orden_linea_valida();
                DROP TRIGGER IF EXISTS tg_linea_calibrado_cuadra ON agro.linea_calibrado;
                DROP TRIGGER IF EXISTS tg_calibrado_cuadra ON agro.calibrado;
                DROP FUNCTION IF EXISTS agro.calibrado_cuadra();
                DROP TRIGGER IF EXISTS tg_linea_calibrado_modificable ON agro.linea_calibrado;
                DROP FUNCTION IF EXISTS agro.linea_calibrado_modificable();
                DROP TRIGGER IF EXISTS tg_calibrado_valido ON agro.calibrado;
                DROP FUNCTION IF EXISTS agro.calibrado_valido();
                """);
            migrationBuilder.Sql(GarantiasSql.QuitarRlsPorPadre("agro", "linea_calibrado"));
            migrationBuilder.Sql(GarantiasSql.QuitarRlsPorPadre("agro", "salida_calibradora"));

            migrationBuilder.DropTable(
                name: "linea_calibrado",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "orden_linea",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "parada_linea",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "salida_calibradora",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "calibrado",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "linea_planta",
                schema: "agro");
        }
    }
}
