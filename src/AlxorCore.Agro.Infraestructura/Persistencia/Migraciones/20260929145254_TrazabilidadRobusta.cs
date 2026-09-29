using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Agro.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class TrazabilidadRobusta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "motivo_descalificacion",
                schema: "agro",
                table: "salida_parte",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "certificaciones",
                schema: "agro",
                table: "partida",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "motivo_descalificacion",
                schema: "agro",
                table: "linea_recepcion",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "certificado_agro",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agricultor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parcela_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo = table.Column<int>(type: "integer", nullable: false),
                    codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    organismo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    desde = table.Column<DateOnly>(type: "date", nullable: false),
                    hasta = table.Column<DateOnly>(type: "date", nullable: true),
                    baja = table.Column<bool>(type: "boolean", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_certificado_agro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "declaracion_articulo",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    exige = table.Column<int>(type: "integer", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_declaracion_articulo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "descalificacion_partida",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    partida_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quitadas = table.Column<int>(type: "integer", nullable: false),
                    motivo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    documento_tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_descalificacion_partida", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_certificado_agro_agricultor",
                schema: "agro",
                table: "certificado_agro",
                columns: new[] { "empresa_id", "agricultor_id" });

            migrationBuilder.CreateIndex(
                name: "ix_certificado_agro_agricultor_id",
                schema: "agro",
                table: "certificado_agro",
                column: "agricultor_id");

            migrationBuilder.CreateIndex(
                name: "ix_certificado_agro_parcela",
                schema: "agro",
                table: "certificado_agro",
                column: "parcela_id");

            migrationBuilder.CreateIndex(
                name: "ix_declaracion_articulo_producto",
                schema: "agro",
                table: "declaracion_articulo",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "ux_declaracion_articulo_producto",
                schema: "agro",
                table: "declaracion_articulo",
                columns: new[] { "empresa_id", "producto_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_descalificacion_partida_partida",
                schema: "agro",
                table: "descalificacion_partida",
                column: "partida_id");

            migrationBuilder.CreateIndex(
                name: "ix_descalificacion_partida_usuario",
                schema: "agro",
                table: "descalificacion_partida",
                column: "usuario_id");

            // =============================== Garantías de la base de datos ===============================
            migrationBuilder.Sql(GarantiasSql.FuncionesComunes);
            foreach (var tabla in new[] { "certificado_agro", "declaracion_articulo", "descalificacion_partida" })
            {
                migrationBuilder.Sql(RlsSql.Activar("agro", tabla));
            }

            migrationBuilder.Sql(GarantiasSql.SoloInsercion("agro", "descalificacion_partida", "descalificacion.inmutable", "Las descalificaciones no se modifican ni se borran."));
            migrationBuilder.Sql(GarantiasSql.MarcarAlta("agro", "descalificacion_partida"));

            migrationBuilder.Sql("""
                ALTER TABLE agro.certificado_agro
                    ADD CONSTRAINT ck_certificado_agro_tipo CHECK (tipo IN (1, 2, 4)),
                    ADD CONSTRAINT ck_certificado_agro_vigencia CHECK (hasta IS NULL OR hasta >= desde),
                    ADD CONSTRAINT fk_certificado_agro_agricultor FOREIGN KEY (agricultor_id) REFERENCES agro.agricultor (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_certificado_agro_parcela FOREIGN KEY (parcela_id) REFERENCES agro.parcela (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.declaracion_articulo ADD CONSTRAINT ck_declaracion_articulo_exige CHECK (exige BETWEEN 0 AND 7);
                ALTER TABLE agro.partida ADD CONSTRAINT ck_partida_certificaciones CHECK (certificaciones BETWEEN 0 AND 7);
                ALTER TABLE agro.descalificacion_partida
                    ADD CONSTRAINT ck_descalificacion_partida_quitadas CHECK (quitadas BETWEEN 1 AND 7),
                    ADD CONSTRAINT ck_descalificacion_partida_motivo CHECK (length(trim(motivo)) > 0),
                    ADD CONSTRAINT fk_descalificacion_partida_partida FOREIGN KEY (partida_id) REFERENCES agro.partida (id) DEFERRABLE INITIALLY DEFERRED;
                """);

            // ----- Partidas: las certificaciones solo se pierden (nunca se ganan), y solo con su descalificación registrada -----
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION agro.partida_inmutable() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF TG_OP = 'DELETE' THEN
                        IF NOT public.alxor_borrando_empresa(OLD.empresa_id) THEN
                            PERFORM public.alxor_error('partida.inmutable', 'Las partidas no se borran: se anulan.');
                        END IF;
                        RETURN OLD;
                    END IF;
                    IF (to_jsonb(NEW) - ARRAY['coste_kg', 'anulada', 'certificaciones']) <> (to_jsonb(OLD) - ARRAY['coste_kg', 'anulada', 'certificaciones'])
                       OR (OLD.anulada AND NOT NEW.anulada) THEN
                        PERFORM public.alxor_error('partida.inmutable', 'El origen y los kilos de una partida no cambian, y una partida anulada no se recupera.');
                    END IF;
                    IF (NEW.certificaciones & ~OLD.certificaciones) <> 0 THEN
                        PERFORM public.alxor_error('partida.certificacion', 'Una partida no gana certificaciones: solo las tiene la fruta que llega certificada.');
                    END IF;
                    RETURN NEW;
                END $f$;

                CREATE OR REPLACE FUNCTION agro.partida_descalificada() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF NEW.certificaciones <> OLD.certificaciones AND NOT EXISTS (
                        SELECT 1 FROM agro.descalificacion_partida d
                         WHERE d.partida_id = NEW.id AND d.tx_alta = pg_current_xact_id() AND d.quitadas = (OLD.certificaciones & ~NEW.certificaciones)) THEN
                        PERFORM public.alxor_error('partida.descalificacion', 'La partida solo pierde una certificación con su descalificación registrada (motivo y usuario).');
                    END IF;
                    RETURN NULL;
                END $f$;
                CREATE CONSTRAINT TRIGGER tg_partida_descalificada AFTER UPDATE ON agro.partida
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION agro.partida_descalificada();
                """);

            // ----- Movimientos: nada antes del alta de la partida, y el saldo día a día nunca negativo (no se vende lo que aún no ha entrado) -----
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
                    IF partida_anulada AND NEW.tipo <> 'Anulacion' THEN
                        PERFORM public.alxor_error('partida.anulada', 'La partida está anulada.');
                    END IF;
                    IF NEW.pale_id IS NOT NULL THEN
                        SELECT p.estado, p.sscc INTO est, sscc FROM agro.pale p WHERE p.id = NEW.pale_id;
                        IF (NEW.tipo = 'Expedicion' AND est <> 'Expedido')
                           OR (NEW.tipo IN ('Entrada', 'Paletizado', 'Consumo', 'Ajuste') AND est <> 'Abierto')
                           OR (NEW.tipo = 'Anulacion' AND NEW.kilos > 0 AND est = 'Expedido') THEN
                            PERFORM public.alxor_error('pale.estado', format('El palé %s está %s: no admite ese movimiento.', sscc, lower(est)));
                        END IF;
                    END IF;
                    RETURN NULL;
                END $f$;
                """);

            // ----- Genealogía: lo consumido de cada partida se reparte entre las salidas (antes cada salida recibía todo) -----
            migrationBuilder.Sql("""
                ALTER TABLE agro.genealogia DISABLE TRIGGER tg_genealogia_solo_insercion;
                UPDATE agro.genealogia g
                   SET kilos_origen = round(c.kilos * d.kilos_iniciales / t.total, 3)
                  FROM (SELECT parte_id, partida_id, sum(kilos) AS kilos FROM agro.consumo_parte GROUP BY parte_id, partida_id) c,
                       agro.partida d,
                       (SELECT parte_id, sum(kilos) AS total FROM agro.salida_parte GROUP BY parte_id) t
                 WHERE c.parte_id = g.parte_id AND c.partida_id = g.origen_id AND d.id = g.destino_id AND t.parte_id = g.parte_id AND t.total > 0;
                WITH suma AS (
                    SELECT parte_id, origen_id, sum(kilos_origen) AS s, (array_agg(id ORDER BY kilos_origen DESC, id))[1] AS mayor
                      FROM agro.genealogia GROUP BY parte_id, origen_id),
                     c AS (SELECT parte_id, partida_id, sum(kilos) AS k FROM agro.consumo_parte GROUP BY parte_id, partida_id)
                UPDATE agro.genealogia g SET kilos_origen = g.kilos_origen + (c.k - suma.s)
                  FROM suma JOIN c ON c.parte_id = suma.parte_id AND c.partida_id = suma.origen_id
                 WHERE g.id = suma.mayor AND c.k <> suma.s;
                ALTER TABLE agro.genealogia ENABLE TRIGGER tg_genealogia_solo_insercion;
                ALTER TABLE agro.genealogia ADD CONSTRAINT ck_genealogia_kilos CHECK (kilos_origen > 0);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS tg_partida_descalificada ON agro.partida;
                DROP FUNCTION IF EXISTS agro.partida_descalificada();
                ALTER TABLE agro.genealogia DROP CONSTRAINT IF EXISTS ck_genealogia_kilos;
                ALTER TABLE agro.partida DROP CONSTRAINT IF EXISTS ck_partida_certificaciones;
                """);

            migrationBuilder.DropTable(
                name: "certificado_agro",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "declaracion_articulo",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "descalificacion_partida",
                schema: "agro");

            migrationBuilder.DropColumn(
                name: "motivo_descalificacion",
                schema: "agro",
                table: "salida_parte");

            migrationBuilder.DropColumn(
                name: "certificaciones",
                schema: "agro",
                table: "partida");

            migrationBuilder.DropColumn(
                name: "motivo_descalificacion",
                schema: "agro",
                table: "linea_recepcion");
        }
    }
}
