using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Agro.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class RespuestasTrazabilidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "bruto_camion_kg",
                schema: "agro",
                table: "pesada",
                type: "numeric(12,3)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "grupo_camion",
                schema: "agro",
                table: "pesada",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "tolerancia_merma_pct",
                schema: "agro",
                table: "parte_confeccion",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "cajas_por_pale",
                schema: "agro",
                table: "pale",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "correccion_expedicion",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pale_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cliente_anterior_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cliente_nuevo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    referencia_anterior = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    referencia_nueva = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    motivo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_correccion_expedicion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "etiqueta_campo",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sscc = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: false),
                    agricultor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parcela_id = table.Column<Guid>(type: "uuid", nullable: true),
                    emitida_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    pale_id = table.Column<Guid>(type: "uuid", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_etiqueta_campo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tolerancia_merma_familia",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    familia_id = table.Column<Guid>(type: "uuid", nullable: false),
                    merma_maxima_pct = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tolerancia_merma_familia", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_correccion_expedicion_cliente_anterior",
                schema: "agro",
                table: "correccion_expedicion",
                column: "cliente_anterior_id");

            migrationBuilder.CreateIndex(
                name: "ix_correccion_expedicion_cliente_nuevo",
                schema: "agro",
                table: "correccion_expedicion",
                column: "cliente_nuevo_id");

            migrationBuilder.CreateIndex(
                name: "ix_correccion_expedicion_pale",
                schema: "agro",
                table: "correccion_expedicion",
                column: "pale_id");

            migrationBuilder.CreateIndex(
                name: "ix_correccion_expedicion_usuario",
                schema: "agro",
                table: "correccion_expedicion",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ix_etiqueta_campo_agricultor",
                schema: "agro",
                table: "etiqueta_campo",
                column: "agricultor_id");

            migrationBuilder.CreateIndex(
                name: "ix_etiqueta_campo_parcela",
                schema: "agro",
                table: "etiqueta_campo",
                column: "parcela_id");

            migrationBuilder.CreateIndex(
                name: "ux_etiqueta_campo_pale",
                schema: "agro",
                table: "etiqueta_campo",
                column: "pale_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_etiqueta_campo_sscc",
                schema: "agro",
                table: "etiqueta_campo",
                columns: new[] { "empresa_id", "sscc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tolerancia_merma_familia_familia",
                schema: "agro",
                table: "tolerancia_merma_familia",
                column: "familia_id");

            migrationBuilder.CreateIndex(
                name: "ux_tolerancia_merma_familia",
                schema: "agro",
                table: "tolerancia_merma_familia",
                columns: new[] { "empresa_id", "familia_id" },
                unique: true);

            // =============================== Garantías de la base de datos ===============================
            foreach (var tabla in new[] { "etiqueta_campo", "tolerancia_merma_familia", "correccion_expedicion" })
            {
                migrationBuilder.Sql(RlsSql.Activar("agro", tabla));
            }

            migrationBuilder.Sql(GarantiasSql.SoloInsercion("agro", "correccion_expedicion", "correccion_expedicion.inmutable", "Las correcciones de expedición no se modifican ni se borran."));
            migrationBuilder.Sql(GarantiasSql.MarcarAlta("agro", "repaletizado"));

            migrationBuilder.Sql("""
                -- Sin cascada: al quitar una pesada, la aplicación borra también sus envases y la clave se comprueba al confirmar.
                ALTER TABLE agro.pesada_envase DROP CONSTRAINT fk_pesada_envase_pesada,
                    ADD CONSTRAINT fk_pesada_envase_pesada FOREIGN KEY (pesada_id) REFERENCES agro.pesada (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.pale ADD CONSTRAINT ck_pale_cajas_por_pale CHECK (cajas_por_pale IS NULL OR cajas_por_pale BETWEEN 1 AND 10000);
                ALTER TABLE agro.pesada ADD CONSTRAINT ck_pesada_camion CHECK ((grupo_camion IS NULL) = (bruto_camion_kg IS NULL) AND (bruto_camion_kg IS NULL OR bruto_camion_kg >= bruto_kg));
                ALTER TABLE agro.tolerancia_merma_familia ADD CONSTRAINT ck_tolerancia_merma_familia CHECK (merma_maxima_pct BETWEEN 0 AND 100);
                ALTER TABLE agro.parte_confeccion ADD CONSTRAINT ck_parte_confeccion_tolerancia CHECK (tolerancia_merma_pct IS NULL OR tolerancia_merma_pct BETWEEN 0 AND 100);
                ALTER TABLE agro.correccion_expedicion
                    ADD CONSTRAINT ck_correccion_expedicion_tipo CHECK (tipo IN ('Anulacion', 'Datos') AND (tipo = 'Anulacion' OR length(trim(coalesce(motivo, ''))) > 0)),
                    ADD CONSTRAINT fk_correccion_expedicion_pale FOREIGN KEY (pale_id) REFERENCES agro.pale (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.etiqueta_campo
                    ADD CONSTRAINT ck_etiqueta_campo_sscc CHECK (sscc ~ '^[0-9]{18}$'),
                    ADD CONSTRAINT fk_etiqueta_campo_agricultor FOREIGN KEY (agricultor_id) REFERENCES agro.agricultor (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_etiqueta_campo_parcela FOREIGN KEY (parcela_id) REFERENCES agro.parcela (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_etiqueta_campo_pale FOREIGN KEY (pale_id) REFERENCES agro.pale (id) DEFERRABLE INITIALLY DEFERRED;

                -- Etiqueta de campo: su SSCC, agricultor y parcela no cambian; solo se usa una vez y, usada, ya no se borra.
                CREATE OR REPLACE FUNCTION agro.etiqueta_campo_inmutable() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF TG_OP = 'DELETE' THEN
                        IF OLD.pale_id IS NOT NULL AND NOT public.alxor_borrando_empresa(OLD.empresa_id) THEN
                            PERFORM public.alxor_error('etiqueta_campo.usada', 'La etiqueta ya está en un palé de entrada: no se borra.');
                        END IF;
                        RETURN OLD;
                    END IF;
                    IF NEW.sscc <> OLD.sscc OR NEW.agricultor_id <> OLD.agricultor_id OR NEW.parcela_id IS DISTINCT FROM OLD.parcela_id
                       OR NEW.empresa_id <> OLD.empresa_id OR NEW.emitida_en <> OLD.emitida_en
                       OR (OLD.pale_id IS NOT NULL AND NEW.pale_id IS DISTINCT FROM OLD.pale_id) THEN
                        PERFORM public.alxor_error('etiqueta_campo.inmutable', 'Una etiqueta de campo emitida no cambia y solo se usa una vez.');
                    END IF;
                    RETURN NEW;
                END $f$;
                CREATE TRIGGER tg_etiqueta_campo_inmutable BEFORE UPDATE OR DELETE ON agro.etiqueta_campo
                    FOR EACH ROW EXECUTE FUNCTION agro.etiqueta_campo_inmutable();

                -- Al confirmar: la etiqueta usada va en un palé de entrada con su mismo SSCC, y ningún otro palé lleva ese SSCC.
                CREATE OR REPLACE FUNCTION agro.etiqueta_campo_valida() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF NEW.pale_id IS NOT NULL AND NOT EXISTS (
                        SELECT 1 FROM agro.pale p WHERE p.id = NEW.pale_id AND p.sscc = NEW.sscc AND p.linea_recepcion_id IS NOT NULL) THEN
                        PERFORM public.alxor_error('etiqueta_campo.pale', 'La etiqueta de campo tiene que ir en un palé de entrada con su mismo SSCC.');
                    END IF;
                    IF EXISTS (SELECT 1 FROM agro.pale p WHERE p.empresa_id = NEW.empresa_id AND p.sscc = NEW.sscc AND p.id IS DISTINCT FROM NEW.pale_id) THEN
                        PERFORM public.alxor_error('etiqueta_campo.sscc_usado', format('El SSCC %s de la etiqueta ya lo lleva otro palé.', NEW.sscc));
                    END IF;
                    RETURN NULL;
                END $f$;
                CREATE CONSTRAINT TRIGGER tg_etiqueta_campo_valida AFTER INSERT OR UPDATE ON agro.etiqueta_campo
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION agro.etiqueta_campo_valida();

                CREATE OR REPLACE FUNCTION agro.pale_sscc_de_etiqueta() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF EXISTS (SELECT 1 FROM agro.etiqueta_campo e WHERE e.empresa_id = NEW.empresa_id AND e.sscc = NEW.sscc AND e.pale_id IS DISTINCT FROM NEW.id) THEN
                        PERFORM public.alxor_error('pale.sscc_de_etiqueta', format('El SSCC %s es de una etiqueta de campo: solo lo lleva el palé de entrada que la trae.', NEW.sscc));
                    END IF;
                    RETURN NULL;
                END $f$;
                CREATE CONSTRAINT TRIGGER tg_pale_sscc_de_etiqueta AFTER INSERT ON agro.pale
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION agro.pale_sscc_de_etiqueta();

                -- Pesada del camión repartida entre líneas: los brutos de sus pesadas suman el bruto de la báscula.
                CREATE OR REPLACE FUNCTION agro.pesada_camion_cuadra() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    grupo uuid := (CASE WHEN TG_OP = 'DELETE' THEN OLD.grupo_camion ELSE NEW.grupo_camion END);
                BEGIN
                    IF grupo IS NOT NULL AND EXISTS (
                        SELECT 1 FROM agro.pesada p WHERE p.grupo_camion = grupo GROUP BY p.grupo_camion
                        HAVING count(*) < 2 OR count(DISTINCT p.bruto_camion_kg) > 1 OR sum(p.bruto_kg) <> max(p.bruto_camion_kg)
                            OR count(DISTINCT p.recepcion_id) > 1) THEN
                        PERFORM public.alxor_error('pesada_camion.no_cuadra', 'Las pesadas del camión no suman su bruto: se quitan o se ponen todas juntas.');
                    END IF;
                    RETURN NULL;
                END $f$;
                CREATE CONSTRAINT TRIGGER tg_pesada_camion_cuadra AFTER INSERT OR UPDATE OR DELETE ON agro.pesada
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION agro.pesada_camion_cuadra();
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
                               AND NOT (est = 'Cerrado' AND NEW.documento_tipo = 'Repaletizado')
                               -- Cajas sueltas vendidas: entran en su bulto y el bulto sale en la misma transacción que el repaletizado.
                               AND NOT (est = 'Expedido' AND NEW.kilos > 0 AND NEW.documento_tipo = 'Repaletizado' AND EXISTS (
                                   SELECT 1 FROM agro.repaletizado r WHERE r.id = NEW.documento_id AND r.destino_pale_id = NEW.pale_id
                                      AND r.tx_alta = pg_current_xact_id())))
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
                    -- Balance de masas: la tolerancia aplicada (la de la transformación, la familia o la general) no puede ser más
                    -- alta que todas las configuradas, y por encima de ella solo con la merma aprobada (quién y por qué).
                    IF NEW.tolerancia_merma_pct IS NOT NULL AND NEW.tolerancia_merma_pct > coalesce(greatest(
                           (SELECT c.tolerancia_merma_pct FROM agro.configuracion c WHERE c.empresa_id = NEW.empresa_id),
                           (SELECT max(t.merma_maxima_pct) FROM agro.tolerancia_merma_familia t WHERE t.empresa_id = NEW.empresa_id),
                           (SELECT max(r.merma_maxima_pct) FROM agro.regla_transformacion r WHERE r.empresa_id = NEW.empresa_id)), -1) THEN
                        PERFORM public.alxor_error('parte.tolerancia_incoherente', 'La tolerancia de merma del parte no es ninguna de las configuradas.');
                    END IF;
                    IF EXISTS (SELECT 1 FROM (SELECT coalesce(NEW.tolerancia_merma_pct,
                                                (SELECT c.tolerancia_merma_pct FROM agro.configuracion c WHERE c.empresa_id = NEW.empresa_id)) AS tol) t,
                                      (SELECT coalesce(sum(kilos), 0) AS k FROM agro.consumo_parte WHERE parte_id = NEW.id) co,
                                      (SELECT coalesce(sum(kilos), 0) AS k FROM agro.salida_parte WHERE parte_id = NEW.id) sa
                                WHERE t.tol IS NOT NULL AND co.k > 0
                                  AND round((co.k - sa.k) * 100 / co.k, 2) > t.tol
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

            migrationBuilder.Sql(GarantiasSql.MarcarAlta("agro", "correccion_expedicion"));
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION agro.pale_valido() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF TG_OP = 'DELETE' THEN
                        IF NOT public.alxor_borrando_empresa(OLD.empresa_id) THEN
                            PERFORM public.alxor_error('pale.inmutable', 'Los palés no se borran.');
                        END IF;
                        RETURN OLD;
                    END IF;
                    IF NEW.sscc <> OLD.sscc OR NEW.plantilla_id IS DISTINCT FROM OLD.plantilla_id
                       OR (OLD.estado = 'Expedido' AND NEW.estado <> 'Expedido' AND NOT (NEW.estado = 'Cerrado' AND NEW.cliente_id IS NULL AND NEW.fecha_expedicion IS NULL
                           AND NEW.referencia_expedicion IS NULL AND NEW.carta_porte_id IS NULL AND NEW.albaran_id IS NULL))
                       -- El cliente y la referencia de un palé expedido solo se corrigen si salió sin albarán ni carta de porte
                       -- (y con su corrección registrada, que se comprueba al confirmar).
                       OR (OLD.estado = 'Expedido' AND NEW.estado = 'Expedido' AND (
                           ((NEW.cliente_id IS DISTINCT FROM OLD.cliente_id
                             OR (OLD.referencia_expedicion IS NOT NULL AND NEW.referencia_expedicion IS DISTINCT FROM OLD.referencia_expedicion))
                            AND (OLD.albaran_id IS NOT NULL OR OLD.carta_porte_id IS NOT NULL OR NEW.albaran_id IS NOT NULL OR NEW.carta_porte_id IS NOT NULL))
                           OR NEW.fecha_expedicion IS DISTINCT FROM OLD.fecha_expedicion
                           OR (OLD.carta_porte_id IS NOT NULL AND NEW.carta_porte_id IS DISTINCT FROM OLD.carta_porte_id)
                           OR (OLD.albaran_id IS NOT NULL AND NEW.albaran_id IS DISTINCT FROM OLD.albaran_id)))
                       OR (NEW.estado <> OLD.estado AND (OLD.estado || '>' || NEW.estado) NOT IN ('Abierto>Cerrado', 'Cerrado>Abierto', 'Cerrado>Expedido', 'Expedido>Cerrado'))
                       OR (NEW.estado <> 'Expedido' AND (NEW.cliente_id IS NOT NULL OR NEW.referencia_expedicion IS NOT NULL OR NEW.carta_porte_id IS NOT NULL OR NEW.albaran_id IS NOT NULL)) THEN
                        PERFORM public.alxor_error('pale.transicion', 'Cambio de palé no permitido: el SSCC y la plantilla no cambian y un palé expedido solo vuelve a cerrado al anular su expedición.');
                    END IF;
                    RETURN NEW;
                END $f$;

                -- La corrección del cliente o la referencia de un palé expedido queda registrada en la misma transacción.
                CREATE OR REPLACE FUNCTION agro.pale_correccion_registrada() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF OLD.estado = 'Expedido' AND NEW.estado = 'Expedido'
                       AND (NEW.cliente_id IS DISTINCT FROM OLD.cliente_id OR NEW.referencia_expedicion IS DISTINCT FROM OLD.referencia_expedicion)
                       AND NOT EXISTS (SELECT 1 FROM agro.correccion_expedicion c
                                        WHERE c.pale_id = NEW.id AND c.tipo = 'Datos' AND c.tx_alta = pg_current_xact_id()
                                          AND c.cliente_nuevo_id IS NOT DISTINCT FROM NEW.cliente_id AND c.referencia_nueva IS NOT DISTINCT FROM NEW.referencia_expedicion) THEN
                        PERFORM public.alxor_error('correccion_expedicion.sin_registro', 'El cliente o la referencia de un palé expedido solo cambian con su corrección registrada.');
                    END IF;
                    RETURN NULL;
                END $f$;
                CREATE CONSTRAINT TRIGGER tg_pale_correccion_registrada AFTER UPDATE ON agro.pale
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION agro.pale_correccion_registrada();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS tg_pale_correccion_registrada ON agro.pale;
                DROP FUNCTION IF EXISTS agro.pale_correccion_registrada();
                CREATE OR REPLACE FUNCTION agro.pale_valido() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF TG_OP = 'DELETE' THEN
                        IF NOT public.alxor_borrando_empresa(OLD.empresa_id) THEN
                            PERFORM public.alxor_error('pale.inmutable', 'Los palés no se borran.');
                        END IF;
                        RETURN OLD;
                    END IF;
                    IF NEW.sscc <> OLD.sscc OR NEW.plantilla_id IS DISTINCT FROM OLD.plantilla_id
                       OR (OLD.estado = 'Expedido' AND NEW.estado <> 'Expedido' AND NOT (NEW.estado = 'Cerrado' AND NEW.cliente_id IS NULL AND NEW.fecha_expedicion IS NULL
                           AND NEW.referencia_expedicion IS NULL AND NEW.carta_porte_id IS NULL AND NEW.albaran_id IS NULL))
                       OR (OLD.estado = 'Expedido' AND NEW.estado = 'Expedido' AND (NEW.cliente_id IS DISTINCT FROM OLD.cliente_id
                           OR NEW.fecha_expedicion IS DISTINCT FROM OLD.fecha_expedicion
                           OR (OLD.referencia_expedicion IS NOT NULL AND NEW.referencia_expedicion IS DISTINCT FROM OLD.referencia_expedicion)
                           OR (OLD.carta_porte_id IS NOT NULL AND NEW.carta_porte_id IS DISTINCT FROM OLD.carta_porte_id)
                           OR (OLD.albaran_id IS NOT NULL AND NEW.albaran_id IS DISTINCT FROM OLD.albaran_id)))
                       OR (NEW.estado <> OLD.estado AND (OLD.estado || '>' || NEW.estado) NOT IN ('Abierto>Cerrado', 'Cerrado>Abierto', 'Cerrado>Expedido', 'Expedido>Cerrado'))
                       OR (NEW.estado <> 'Expedido' AND (NEW.cliente_id IS NOT NULL OR NEW.referencia_expedicion IS NOT NULL OR NEW.carta_porte_id IS NOT NULL OR NEW.albaran_id IS NOT NULL)) THEN
                        PERFORM public.alxor_error('pale.transicion', 'Cambio de palé no permitido: el SSCC y la plantilla no cambian y un palé expedido solo vuelve a cerrado al anular su expedición.');
                    END IF;
                    RETURN NEW;
                END $f$;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE agro.pesada_envase DROP CONSTRAINT fk_pesada_envase_pesada,
                    ADD CONSTRAINT fk_pesada_envase_pesada FOREIGN KEY (pesada_id) REFERENCES agro.pesada (id) ON DELETE CASCADE DEFERRABLE INITIALLY DEFERRED;
                DROP TRIGGER IF EXISTS tg_pesada_camion_cuadra ON agro.pesada;
                DROP FUNCTION IF EXISTS agro.pesada_camion_cuadra();
                DROP TRIGGER IF EXISTS tg_pale_sscc_de_etiqueta ON agro.pale;
                DROP FUNCTION IF EXISTS agro.pale_sscc_de_etiqueta();
                DROP TRIGGER IF EXISTS tg_etiqueta_campo_valida ON agro.etiqueta_campo;
                DROP FUNCTION IF EXISTS agro.etiqueta_campo_valida();
                DROP TRIGGER IF EXISTS tg_etiqueta_campo_inmutable ON agro.etiqueta_campo;
                DROP FUNCTION IF EXISTS agro.etiqueta_campo_inmutable();
                DROP TRIGGER IF EXISTS tg_repaletizado_alta ON agro.repaletizado;
                ALTER TABLE agro.repaletizado DROP COLUMN IF EXISTS tx_alta;
                ALTER TABLE agro.pale DROP CONSTRAINT IF EXISTS ck_pale_cajas_por_pale;
                ALTER TABLE agro.pesada DROP CONSTRAINT IF EXISTS ck_pesada_camion;
                ALTER TABLE agro.parte_confeccion DROP CONSTRAINT IF EXISTS ck_parte_confeccion_tolerancia;
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


            migrationBuilder.DropTable(
                name: "correccion_expedicion",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "etiqueta_campo",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "tolerancia_merma_familia",
                schema: "agro");

            migrationBuilder.DropColumn(
                name: "bruto_camion_kg",
                schema: "agro",
                table: "pesada");

            migrationBuilder.DropColumn(
                name: "grupo_camion",
                schema: "agro",
                table: "pesada");

            migrationBuilder.DropColumn(
                name: "tolerancia_merma_pct",
                schema: "agro",
                table: "parte_confeccion");

            migrationBuilder.DropColumn(
                name: "cajas_por_pale",
                schema: "agro",
                table: "pale");
        }
    }
}
