using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Agro.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class RectificacionYTransformacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "merma_aprobada_en",
                schema: "agro",
                table: "parte_confeccion",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "merma_aprobada_pct",
                schema: "agro",
                table: "parte_confeccion",
                type: "numeric(7,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "merma_aprobada_por",
                schema: "agro",
                table: "parte_confeccion",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "merma_aprobada_por_id",
                schema: "agro",
                table: "parte_confeccion",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo_merma",
                schema: "agro",
                table: "parte_confeccion",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "tolerancia_merma_pct",
                schema: "agro",
                table: "configuracion",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "rectificacion_recepcion",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    recepcion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    linea_recepcion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    partida_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    neto_anterior_kg = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    diferencia_kg = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    kilos_liquidacion = table.Column<decimal>(type: "numeric(12,3)", nullable: true),
                    bruto_kg = table.Column<decimal>(type: "numeric(12,3)", nullable: true),
                    tara_kg = table.Column<decimal>(type: "numeric(12,3)", nullable: true),
                    motivo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    creada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rectificacion_recepcion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "regla_transformacion",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_origen_id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_destino_id = table.Column<Guid>(type: "uuid", nullable: false),
                    merma_maxima_pct = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regla_transformacion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "repaletizado",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    destino_pale_id = table.Column<Guid>(type: "uuid", nullable: false),
                    motivo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_repaletizado", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "linea_repaletizado",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    origen_pale_id = table.Column<Guid>(type: "uuid", nullable: false),
                    partida_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kilos = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    cajas = table.Column<int>(type: "integer", nullable: false),
                    repaletizado_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_repaletizado", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_repaletizado_repaletizado_repaletizado_id",
                        column: x => x.repaletizado_id,
                        principalSchema: "agro",
                        principalTable: "repaletizado",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_parte_confeccion_aprobador",
                schema: "agro",
                table: "parte_confeccion",
                column: "merma_aprobada_por_id");

            migrationBuilder.CreateIndex(
                name: "IX_linea_repaletizado_repaletizado_id",
                schema: "agro",
                table: "linea_repaletizado",
                column: "repaletizado_id");

            migrationBuilder.CreateIndex(
                name: "ix_linea_repaletizado_origen",
                schema: "agro",
                table: "linea_repaletizado",
                column: "origen_pale_id");

            migrationBuilder.CreateIndex(
                name: "ix_linea_repaletizado_partida",
                schema: "agro",
                table: "linea_repaletizado",
                column: "partida_id");

            migrationBuilder.CreateIndex(
                name: "ix_rectificacion_recepcion_linea",
                schema: "agro",
                table: "rectificacion_recepcion",
                column: "linea_recepcion_id");

            migrationBuilder.CreateIndex(
                name: "ix_rectificacion_recepcion_partida",
                schema: "agro",
                table: "rectificacion_recepcion",
                column: "partida_id");

            migrationBuilder.CreateIndex(
                name: "ix_rectificacion_recepcion_recepcion",
                schema: "agro",
                table: "rectificacion_recepcion",
                column: "recepcion_id");

            migrationBuilder.CreateIndex(
                name: "ix_rectificacion_recepcion_usuario",
                schema: "agro",
                table: "rectificacion_recepcion",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_regla_transformacion_destino",
                schema: "agro",
                table: "regla_transformacion",
                column: "producto_destino_id");

            migrationBuilder.CreateIndex(
                name: "ix_regla_transformacion_origen",
                schema: "agro",
                table: "regla_transformacion",
                column: "producto_origen_id");

            migrationBuilder.CreateIndex(
                name: "ux_regla_transformacion",
                schema: "agro",
                table: "regla_transformacion",
                columns: new[] { "empresa_id", "producto_origen_id", "producto_destino_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_repaletizado_destino",
                schema: "agro",
                table: "repaletizado",
                column: "destino_pale_id");

            migrationBuilder.CreateIndex(
                name: "ix_repaletizado_usuario",
                schema: "agro",
                table: "repaletizado",
                column: "usuario_id");

            // =============================== Garantías de la base de datos ===============================
            foreach (var tabla in new[] { "rectificacion_recepcion", "regla_transformacion", "repaletizado" })
            {
                migrationBuilder.Sql(RlsSql.Activar("agro", tabla));
            }

            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("agro", "linea_repaletizado", "repaletizado_id", "agro", "repaletizado"));
            migrationBuilder.Sql(GarantiasSql.SoloInsercion("agro", "rectificacion_recepcion", "rectificacion.inmutable", "Las rectificaciones no se modifican ni se borran: se rectifica otra vez."));
            migrationBuilder.Sql(GarantiasSql.SoloInsercion("agro", "repaletizado", "repaletizado.inmutable", "Los repaletizados no se modifican ni se borran: se repaletiza en sentido contrario."));
            migrationBuilder.Sql(GarantiasSql.SoloInsercion("agro", "linea_repaletizado", "repaletizado.inmutable", "Los repaletizados no se modifican ni se borran: se repaletiza en sentido contrario."));

            migrationBuilder.Sql("""
                ALTER TABLE agro.movimiento_partida DROP CONSTRAINT ck_movimiento_partida_tipo;
                ALTER TABLE agro.movimiento_partida
                    ADD CONSTRAINT ck_movimiento_partida_tipo CHECK (tipo IN ('Entrada', 'Consumo', 'Paletizado', 'Expedicion', 'Ajuste', 'Anulacion', 'Rectificacion'));
                ALTER TABLE agro.rectificacion_recepcion
                    ADD CONSTRAINT ck_rectificacion_recepcion_valores CHECK (length(trim(motivo)) > 0 AND (diferencia_kg <> 0 OR kilos_liquidacion IS NOT NULL)
                        AND (kilos_liquidacion IS NULL OR kilos_liquidacion > 0) AND neto_anterior_kg + diferencia_kg > 0),
                    ADD CONSTRAINT fk_rectificacion_recepcion_recepcion FOREIGN KEY (recepcion_id) REFERENCES agro.recepcion (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_rectificacion_recepcion_linea FOREIGN KEY (linea_recepcion_id) REFERENCES agro.linea_recepcion (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_rectificacion_recepcion_partida FOREIGN KEY (partida_id) REFERENCES agro.partida (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.regla_transformacion ADD CONSTRAINT ck_regla_transformacion_merma CHECK (merma_maxima_pct IS NULL OR merma_maxima_pct BETWEEN 0 AND 100);
                ALTER TABLE agro.configuracion ADD CONSTRAINT ck_configuracion_tolerancia_merma CHECK (tolerancia_merma_pct IS NULL OR tolerancia_merma_pct BETWEEN 0 AND 100);
                ALTER TABLE agro.parte_confeccion
                    ADD CONSTRAINT ck_parte_confeccion_merma_aprobada CHECK ((merma_aprobada_pct IS NULL) = (motivo_merma IS NULL));
                ALTER TABLE agro.repaletizado
                    ADD CONSTRAINT fk_repaletizado_destino FOREIGN KEY (destino_pale_id) REFERENCES agro.pale (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.linea_repaletizado
                    ADD CONSTRAINT ck_linea_repaletizado_valores CHECK (kilos > 0 AND cajas >= 0),
                    ADD CONSTRAINT fk_linea_repaletizado_origen FOREIGN KEY (origen_pale_id) REFERENCES agro.pale (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_linea_repaletizado_partida FOREIGN KEY (partida_id) REFERENCES agro.partida (id) DEFERRABLE INITIALLY DEFERRED;

                -- Rectificación: de una línea confirmada con su partida, fuera de liquidaciones vivas, y con sus movimientos exactos.
                CREATE OR REPLACE FUNCTION agro.rectificacion_valida() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM agro.linea_recepcion l JOIN agro.recepcion r ON r.id = l.recepcion_id
                                    WHERE l.id = NEW.linea_recepcion_id AND r.id = NEW.recepcion_id AND r.estado = 'Confirmada' AND l.partida_id = NEW.partida_id) THEN
                        PERFORM public.alxor_error('rectificacion.incoherente', 'La rectificación tiene que ser de una línea confirmada y de su partida.');
                    END IF;
                    IF EXISTS (SELECT 1 FROM agro.linea_liquidacion ll JOIN agro.liquidacion q ON q.id = ll.liquidacion_id
                                WHERE ll.linea_recepcion_id = NEW.linea_recepcion_id AND q.estado <> 'Anulada') THEN
                        PERFORM public.alxor_error('rectificacion.liquidada', 'La entrega está en una liquidación viva: anúlala antes de rectificar.');
                    END IF;
                    IF NEW.diferencia_kg <> (SELECT coalesce(sum(m.kilos), 0) FROM agro.movimiento_partida m
                                              WHERE m.documento_tipo = 'Rectificacion' AND m.documento_id = NEW.id AND m.partida_id = NEW.partida_id AND m.tipo = 'Rectificacion') THEN
                        PERFORM public.alxor_error('rectificacion.no_cuadra', 'Los movimientos de la rectificación no suman su diferencia de kilos.');
                    END IF;
                    RETURN NULL;
                END $f$;
                CREATE CONSTRAINT TRIGGER tg_rectificacion_valida AFTER INSERT ON agro.rectificacion_recepcion
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION agro.rectificacion_valida();

                -- Repaletizado: cada arista sale de su palé de origen y entra, entera, en el de destino.
                CREATE OR REPLACE FUNCTION agro.repaletizado_cuadra() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF EXISTS (SELECT 1 FROM (SELECT origen_pale_id, partida_id, sum(kilos) AS k FROM agro.linea_repaletizado WHERE repaletizado_id = NEW.id
                                               GROUP BY origen_pale_id, partida_id) l
                                WHERE l.k <> -(SELECT coalesce(sum(m.kilos), 0) FROM agro.movimiento_partida m
                                                WHERE m.documento_tipo = 'Repaletizado' AND m.documento_id = NEW.id AND m.pale_id = l.origen_pale_id AND m.partida_id = l.partida_id))
                       OR (SELECT coalesce(sum(kilos), 0) FROM agro.linea_repaletizado WHERE repaletizado_id = NEW.id)
                          <> (SELECT coalesce(sum(m.kilos), 0) FROM agro.movimiento_partida m
                               WHERE m.documento_tipo = 'Repaletizado' AND m.documento_id = NEW.id AND m.pale_id = NEW.destino_pale_id)
                       OR NOT EXISTS (SELECT 1 FROM agro.linea_repaletizado WHERE repaletizado_id = NEW.id) THEN
                        PERFORM public.alxor_error('repaletizado.no_cuadra', 'Lo que sale de los palés de origen no es lo que entra en el de destino.');
                    END IF;
                    RETURN NULL;
                END $f$;
                CREATE CONSTRAINT TRIGGER tg_repaletizado_cuadra AFTER INSERT ON agro.repaletizado
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION agro.repaletizado_cuadra();
                """);

            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION agro.movimiento_partida_valido() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    saldo numeric;
                    dia date;
                    est text;
                    sscc text;
                    partida_anulada boolean;
                    alta date;
                    codigo text;
                    linea_pale uuid;
                BEGIN
                    SELECT p.anulada, p.fecha, p.codigo INTO partida_anulada, alta, codigo FROM agro.partida p WHERE p.id = NEW.partida_id;
                    IF NEW.fecha < alta THEN
                        PERFORM public.alxor_error('partida.fecha_anterior',
                            format('La partida %s es del %s: no admite movimientos del %s.', codigo, to_char(alta, 'DD/MM/YYYY'), to_char(NEW.fecha, 'DD/MM/YYYY')));
                    END IF;
                    SELECT coalesce(sum(kilos), 0) INTO saldo FROM agro.movimiento_partida
                     WHERE partida_id = NEW.partida_id AND pale_id IS NOT DISTINCT FROM NEW.pale_id;
                    IF saldo < 0 THEN
                        PERFORM public.alxor_error('partida.saldo_negativo',
                            'La partida no tiene tantos kilos ' || CASE WHEN NEW.pale_id IS NULL THEN 'sueltos.' ELSE 'en ese palé.' END);
                    END IF;
                    SELECT d.fecha INTO dia FROM (
                        SELECT m.fecha, sum(sum(m.kilos)) OVER (ORDER BY m.fecha) AS acumulado
                          FROM agro.movimiento_partida m
                         WHERE m.partida_id = NEW.partida_id AND m.pale_id IS NOT DISTINCT FROM NEW.pale_id
                         GROUP BY m.fecha) d
                     WHERE d.acumulado < 0 ORDER BY d.fecha LIMIT 1;
                    IF dia IS NOT NULL THEN
                        PERFORM public.alxor_error('partida.saldo_fecha',
                            format('El %s la partida %s no tenía esos kilos %s: no puede salir, consumirse ni moverse antes de haber entrado.',
                                   to_char(dia, 'DD/MM/YYYY'), codigo, CASE WHEN NEW.pale_id IS NULL THEN 'sueltos' ELSE 'en ese palé' END));
                    END IF;
                    IF NEW.tipo = 'Rectificacion' AND NOT EXISTS (
                        SELECT 1 FROM agro.rectificacion_recepcion r WHERE r.id = NEW.documento_id AND r.partida_id = NEW.partida_id) THEN
                        PERFORM public.alxor_error('rectificacion.sin_documento', 'Un movimiento de rectificación tiene que salir de una rectificación de su recepción.');
                    END IF;
                    IF NEW.documento_tipo = 'Repaletizado' AND NOT EXISTS (SELECT 1 FROM agro.repaletizado r WHERE r.id = NEW.documento_id) THEN
                        PERFORM public.alxor_error('repaletizado.sin_documento', 'Un movimiento de repaletizado tiene que salir de un repaletizado registrado.');
                    END IF;
                    IF partida_anulada AND NEW.tipo <> 'Anulacion' THEN
                        PERFORM public.alxor_error('partida.anulada', 'La partida está anulada.');
                    END IF;
                    IF NEW.pale_id IS NOT NULL THEN
                        SELECT p.estado, p.sscc, p.linea_recepcion_id INTO est, sscc, linea_pale FROM agro.pale p WHERE p.id = NEW.pale_id;
                        IF (NEW.tipo = 'Expedicion' AND est <> 'Expedido')
                           OR (NEW.tipo = 'Ajuste' AND est <> 'Abierto')
                           OR (NEW.tipo = 'Paletizado' AND est <> 'Abierto'
                               AND NOT (est = 'Cerrado' AND NEW.documento_tipo = 'Repaletizado'))
                           OR (NEW.tipo = 'Rectificacion' AND est <> 'Abierto'
                               AND NOT (est = 'Cerrado' AND linea_pale IS NOT NULL AND linea_pale = (SELECT pa.linea_recepcion_id FROM agro.partida pa WHERE pa.id = NEW.partida_id)))
                           OR (NEW.tipo = 'Consumo' AND est NOT IN ('Abierto', 'Cerrado'))
                           OR (NEW.tipo = 'Entrada' AND est <> 'Abierto'
                               AND NOT (est = 'Cerrado' AND linea_pale IS NOT NULL AND linea_pale = (SELECT pa.linea_recepcion_id FROM agro.partida pa WHERE pa.id = NEW.partida_id)))
                           OR (NEW.tipo = 'Anulacion' AND NEW.kilos > 0 AND est = 'Expedido') THEN
                            PERFORM public.alxor_error('pale.estado', format('El palé %s está %s: no admite ese movimiento.', sscc, lower(est)));
                        END IF;
                    END IF;
                    RETURN NULL;
                END $f$;
                """);

            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION agro.parte_cuadra() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF NEW.estado <> 'Validado' THEN
                        RETURN NULL;
                    END IF;
                    IF NEW.coste_total <> (SELECT coalesce(sum(coste), 0) FROM agro.salida_parte WHERE parte_id = NEW.id)
                       OR NEW.coste_fruta <> (SELECT coalesce(sum(coste), 0) FROM agro.consumo_parte WHERE parte_id = NEW.id)
                       OR NEW.coste_materiales <> (SELECT coalesce(sum(coste), 0) FROM agro.material_parte WHERE parte_id = NEW.id)
                       OR NEW.coste_mano_obra <> (SELECT coalesce(sum(coste), 0) FROM agro.mano_obra_parte WHERE parte_id = NEW.id)
                       OR NEW.coste_maquinaria <> (SELECT coalesce(sum(coste), 0) FROM agro.maquina_parte WHERE parte_id = NEW.id) THEN
                        PERFORM public.alxor_error('parte.no_cuadra', 'El coste del parte no cuadra con sus líneas o no se ha repartido entero entre las salidas.');
                    END IF;
                    IF (SELECT coalesce(sum(kilos), 0) FROM agro.salida_parte WHERE parte_id = NEW.id) > (SELECT coalesce(sum(kilos), 0) FROM agro.consumo_parte WHERE parte_id = NEW.id)
                       OR EXISTS (SELECT 1 FROM agro.salida_parte s LEFT JOIN agro.partida p ON p.id = s.partida_id
                                   WHERE s.parte_id = NEW.id AND (p.id IS NULL OR p.parte_confeccion_id <> NEW.id OR p.kilos_iniciales <> s.kilos)) THEN
                        PERFORM public.alxor_error('parte.salidas', 'Las salidas del parte no cuadran con sus partidas o superan los kilos consumidos.');
                    END IF;
                    -- Balance de masas: por encima de la tolerancia de merma, solo con la merma aprobada (quién y por qué).
                    IF EXISTS (SELECT 1 FROM agro.configuracion c,
                                      (SELECT coalesce(sum(kilos), 0) AS k FROM agro.consumo_parte WHERE parte_id = NEW.id) co,
                                      (SELECT coalesce(sum(kilos), 0) AS k FROM agro.salida_parte WHERE parte_id = NEW.id) sa
                                WHERE c.empresa_id = NEW.empresa_id AND c.tolerancia_merma_pct IS NOT NULL AND co.k > 0
                                  AND round((co.k - sa.k) * 100 / co.k, 2) > c.tolerancia_merma_pct
                                  AND (NEW.merma_aprobada_pct IS NULL OR round((co.k - sa.k) * 100 / co.k, 2) > NEW.merma_aprobada_pct)) THEN
                        PERFORM public.alxor_error('parte.merma_excesiva', 'La merma del parte supera la tolerancia y no está aprobada.');
                    END IF;
                    -- Genealogía completa: cada partida consumida reparte exactamente sus kilos entre las salidas, y cada salida viene de alguna.
                    IF EXISTS (SELECT 1 FROM (SELECT partida_id, sum(kilos) AS k FROM agro.consumo_parte WHERE parte_id = NEW.id GROUP BY partida_id) c
                                 LEFT JOIN (SELECT origen_id, sum(kilos_origen) AS k FROM agro.genealogia WHERE parte_id = NEW.id GROUP BY origen_id) g
                                   ON g.origen_id = c.partida_id
                                WHERE coalesce(g.k, 0) <> c.k)
                       OR EXISTS (SELECT 1 FROM agro.salida_parte s WHERE s.parte_id = NEW.id AND s.kilos > 0
                                     AND NOT EXISTS (SELECT 1 FROM agro.genealogia g WHERE g.parte_id = NEW.id AND g.destino_id = s.partida_id)) THEN
                        PERFORM public.alxor_error('genealogia.incompleta', 'La genealogía del parte no reparte exactamente lo consumido entre sus salidas.');
                    END IF;
                    RETURN NULL;
                END $f$;
                """);

            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION agro.liquidacion_cuadra() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    liq agro.liquidacion%ROWTYPE;
                    mal record;
                    id_liq uuid := (to_jsonb(CASE WHEN TG_OP = 'DELETE' THEN OLD ELSE NEW END)
                                    ->> CASE WHEN TG_TABLE_NAME = 'liquidacion' THEN 'id' ELSE 'liquidacion_id' END)::uuid;
                BEGIN
                    SELECT * INTO liq FROM agro.liquidacion WHERE id = id_liq;
                    IF NOT FOUND OR liq.estado = 'Anulada' THEN
                        RETURN NULL;
                    END IF;

                    -- Una entrega (línea de recepción) solo está en una liquidación viva.
                    SELECT l.linea_recepcion_id INTO mal FROM agro.linea_liquidacion l
                     WHERE l.liquidacion_id = liq.id AND EXISTS (
                        SELECT 1 FROM agro.linea_liquidacion o JOIN agro.liquidacion q ON q.id = o.liquidacion_id
                         WHERE o.linea_recepcion_id = l.linea_recepcion_id AND o.liquidacion_id <> liq.id AND q.estado <> 'Anulada')
                     LIMIT 1;
                    IF FOUND THEN
                        PERFORM public.alxor_error('liquidacion.entrega_duplicada', 'Una entrega ya está en otra liquidación.');
                    END IF;

                    -- Coherente con la recepción: confirmada, del agricultor y la campaña, en el periodo, con su partida.
                    SELECT l.id INTO mal FROM agro.linea_liquidacion l
                      JOIN agro.linea_recepcion lr ON lr.id = l.linea_recepcion_id
                      JOIN agro.recepcion r ON r.id = lr.recepcion_id
                     WHERE l.liquidacion_id = liq.id
                       AND (r.id <> l.recepcion_id OR r.estado <> 'Confirmada' OR r.agricultor_id <> liq.agricultor_id OR r.campana_id <> liq.campana_id
                            OR r.fecha NOT BETWEEN liq.desde AND liq.hasta OR r.fecha <> l.fecha_recepcion OR lr.partida_id IS DISTINCT FROM l.partida_id)
                     LIMIT 1;
                    IF FOUND THEN
                        PERFORM public.alxor_error('liquidacion.linea_incoherente', 'Una línea de la liquidación no corresponde a una entrega confirmada del agricultor en el periodo.');
                    END IF;

                    -- Se liquidan los kilos de cada entrega (los de liquidación si se fijaron; si no, el neto), ni más ni menos.
                    SELECT lr.id INTO mal FROM agro.linea_recepcion lr
                      JOIN (SELECT linea_recepcion_id, sum(kilos) AS kilos FROM agro.linea_liquidacion WHERE liquidacion_id = liq.id GROUP BY linea_recepcion_id) s
                        ON s.linea_recepcion_id = lr.id
                     WHERE s.kilos <> coalesce(
                               (SELECT r.kilos_liquidacion FROM agro.rectificacion_recepcion r
                                 WHERE r.linea_recepcion_id = lr.id AND r.kilos_liquidacion IS NOT NULL ORDER BY r.creada_en DESC LIMIT 1),
                               lr.kilos_liquidacion,
                               lr.neto_kg + (SELECT coalesce(sum(r.diferencia_kg), 0) FROM agro.rectificacion_recepcion r WHERE r.linea_recepcion_id = lr.id))
                     LIMIT 1;
                    IF FOUND THEN
                        PERFORM public.alxor_error('liquidacion.kilos', 'Los kilos liquidados de una entrega no coinciden con sus kilos a liquidar (el neto, o los de liquidación fijados con motivo).');
                    END IF;

                    -- El precio aplicado es el vigente del artículo (y la categoría) en la fecha de la entrega.
                    SELECT l.id INTO mal FROM agro.linea_liquidacion l
                      JOIN agro.precio_liquidacion p ON p.id = l.precio_id
                      JOIN agro.linea_recepcion lr ON lr.id = l.linea_recepcion_id
                     WHERE l.liquidacion_id = liq.id
                       AND (p.precio_kg <> l.precio_kg OR p.campana_id <> liq.campana_id OR p.producto_id <> lr.producto_id
                            OR p.categoria_id IS DISTINCT FROM l.categoria_id OR l.fecha_recepcion NOT BETWEEN p.desde AND p.hasta)
                     LIMIT 1;
                    IF FOUND THEN
                        PERFORM public.alxor_error('liquidacion.precio', 'Una línea no lleva el precio vigente de su artículo y categoría.');
                    END IF;

                    -- Totales de la cabecera.
                    IF liq.kilos <> (SELECT coalesce(sum(kilos), 0) FROM agro.linea_liquidacion WHERE liquidacion_id = liq.id)
                       OR liq.bruto <> (SELECT coalesce(sum(importe), 0) FROM agro.linea_liquidacion WHERE liquidacion_id = liq.id)
                       OR liq.total_descuentos <> (SELECT coalesce(sum(importe), 0) FROM agro.descuento_liquidacion WHERE liquidacion_id = liq.id) THEN
                        PERFORM public.alxor_error('liquidacion.totales', 'Los totales de la liquidación no cuadran con sus líneas y descuentos.');
                    END IF;

                    -- Emitida: el agricultor autorizó la autofacturación antes de la fecha.
                    IF liq.estado = 'Emitida' AND NOT EXISTS (
                        SELECT 1 FROM agro.agricultor a WHERE a.id = liq.agricultor_id AND a.autofacturacion_desde <= liq.fecha) THEN
                        PERFORM public.alxor_error('liquidacion.sin_autofacturacion', 'El agricultor no ha autorizado la autofacturación en esa fecha.');
                    END IF;
                    RETURN NULL;
                END $f$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(GarantiasSql.QuitarRlsPorPadre("agro", "linea_repaletizado"));
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS tg_rectificacion_valida ON agro.rectificacion_recepcion;
                DROP FUNCTION IF EXISTS agro.rectificacion_valida();
                DROP TRIGGER IF EXISTS tg_repaletizado_cuadra ON agro.repaletizado;
                DROP FUNCTION IF EXISTS agro.repaletizado_cuadra();
                ALTER TABLE agro.configuracion DROP CONSTRAINT IF EXISTS ck_configuracion_tolerancia_merma;
                ALTER TABLE agro.parte_confeccion DROP CONSTRAINT IF EXISTS ck_parte_confeccion_merma_aprobada;
                DELETE FROM agro.movimiento_partida WHERE tipo = 'Rectificacion';
                ALTER TABLE agro.movimiento_partida DROP CONSTRAINT ck_movimiento_partida_tipo;
                ALTER TABLE agro.movimiento_partida
                    ADD CONSTRAINT ck_movimiento_partida_tipo CHECK (tipo IN ('Entrada', 'Consumo', 'Paletizado', 'Expedicion', 'Ajuste', 'Anulacion'));
                """);

            migrationBuilder.DropTable(
                name: "linea_repaletizado",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "rectificacion_recepcion",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "regla_transformacion",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "repaletizado",
                schema: "agro");

            migrationBuilder.DropIndex(
                name: "ix_parte_confeccion_aprobador",
                schema: "agro",
                table: "parte_confeccion");

            migrationBuilder.DropColumn(
                name: "merma_aprobada_en",
                schema: "agro",
                table: "parte_confeccion");

            migrationBuilder.DropColumn(
                name: "merma_aprobada_pct",
                schema: "agro",
                table: "parte_confeccion");

            migrationBuilder.DropColumn(
                name: "merma_aprobada_por",
                schema: "agro",
                table: "parte_confeccion");

            migrationBuilder.DropColumn(
                name: "merma_aprobada_por_id",
                schema: "agro",
                table: "parte_confeccion");

            migrationBuilder.DropColumn(
                name: "motivo_merma",
                schema: "agro",
                table: "parte_confeccion");

            migrationBuilder.DropColumn(
                name: "tolerancia_merma_pct",
                schema: "agro",
                table: "configuracion");
        }
    }
}
