using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Agro.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class PaleDesdeRecepcionYTaras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "tara_camion_kg",
                schema: "agro",
                table: "pesada",
                type: "numeric(12,3)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "tara_envases_kg",
                schema: "agro",
                table: "pesada",
                type: "numeric(12,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "linea_recepcion_id",
                schema: "agro",
                table: "pale",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "serie_origen",
                schema: "agro",
                table: "pale",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "kilos_liquidacion",
                schema: "agro",
                table: "linea_recepcion",
                type: "numeric(12,3)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo_kilos_liquidacion",
                schema: "agro",
                table: "linea_recepcion",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "pale_entrada",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    linea_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    serie_origen = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    envase_producto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    envases = table.Column<int>(type: "integer", nullable: false),
                    kilos_netos = table.Column<decimal>(type: "numeric(12,3)", nullable: true),
                    pale_id = table.Column<Guid>(type: "uuid", nullable: true),
                    kilos_asignados = table.Column<decimal>(type: "numeric(12,3)", nullable: true),
                    recepcion_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pale_entrada", x => x.id);
                    table.ForeignKey(
                        name: "FK_pale_entrada_recepcion_recepcion_id",
                        column: x => x.recepcion_id,
                        principalSchema: "agro",
                        principalTable: "recepcion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pesada_envase",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pesada_id = table.Column<Guid>(type: "uuid", nullable: false),
                    envase_producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    tara_unitaria_kg = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    tara_envase_id = table.Column<Guid>(type: "uuid", nullable: true),
                    recepcion_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pesada_envase", x => x.id);
                    table.ForeignKey(
                        name: "FK_pesada_envase_recepcion_recepcion_id",
                        column: x => x.recepcion_id,
                        principalSchema: "agro",
                        principalTable: "recepcion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tara_envase",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    envase_producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tara_kg = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    desde = table.Column<DateOnly>(type: "date", nullable: false),
                    hasta = table.Column<DateOnly>(type: "date", nullable: true),
                    observaciones = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tara_envase", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_pale_linea_recepcion",
                schema: "agro",
                table: "pale",
                column: "linea_recepcion_id");

            migrationBuilder.CreateIndex(
                name: "ix_pale_entrada_envase",
                schema: "agro",
                table: "pale_entrada",
                column: "envase_producto_id");

            migrationBuilder.CreateIndex(
                name: "ix_pale_entrada_recepcion",
                schema: "agro",
                table: "pale_entrada",
                column: "recepcion_id");

            migrationBuilder.CreateIndex(
                name: "ux_pale_entrada_numero",
                schema: "agro",
                table: "pale_entrada",
                columns: new[] { "linea_id", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_pale_entrada_pale",
                schema: "agro",
                table: "pale_entrada",
                column: "pale_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pesada_envase_envase",
                schema: "agro",
                table: "pesada_envase",
                column: "envase_producto_id");

            migrationBuilder.CreateIndex(
                name: "ix_pesada_envase_recepcion",
                schema: "agro",
                table: "pesada_envase",
                column: "recepcion_id");

            migrationBuilder.CreateIndex(
                name: "ix_pesada_envase_tara",
                schema: "agro",
                table: "pesada_envase",
                column: "tara_envase_id");

            migrationBuilder.CreateIndex(
                name: "ux_pesada_envase_tipo",
                schema: "agro",
                table: "pesada_envase",
                columns: new[] { "pesada_id", "envase_producto_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tara_envase_envase",
                schema: "agro",
                table: "tara_envase",
                column: "envase_producto_id");

            migrationBuilder.CreateIndex(
                name: "ux_tara_envase_desde",
                schema: "agro",
                table: "tara_envase",
                columns: new[] { "empresa_id", "envase_producto_id", "desde" },
                unique: true);

            // =============================== Garantías de la base de datos ===============================
            migrationBuilder.Sql(RlsSql.Activar("agro", "tara_envase"));
            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("agro", "pesada_envase", "recepcion_id", "agro", "recepcion"));
            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("agro", "pale_entrada", "recepcion_id", "agro", "recepcion"));

            migrationBuilder.Sql("""
                CREATE TRIGGER tg_pesada_envase_modificable BEFORE INSERT OR UPDATE OR DELETE ON agro.pesada_envase FOR EACH ROW
                    EXECUTE FUNCTION agro.linea_modificable('agro.recepcion', 'recepcion_id', 'Borrador', 'recepcion.confirmada', 'Una recepción confirmada no se modifica: se anula.');
                CREATE TRIGGER tg_pale_entrada_modificable BEFORE INSERT OR UPDATE OR DELETE ON agro.pale_entrada FOR EACH ROW
                    EXECUTE FUNCTION agro.linea_modificable('agro.recepcion', 'recepcion_id', 'Borrador', 'recepcion.confirmada', 'Una recepción confirmada no se modifica: se anula.');

                ALTER TABLE agro.pesada
                    ADD CONSTRAINT ck_pesada_tara_desglose CHECK (tara_envases_kg >= 0 AND (tara_camion_kg IS NULL OR (tara_camion_kg >= 0 AND tara_kg = tara_camion_kg + tara_envases_kg)));
                ALTER TABLE agro.pesada_envase
                    ADD CONSTRAINT ck_pesada_envase_valores CHECK (cantidad > 0 AND tara_unitaria_kg >= 0),
                    ADD CONSTRAINT fk_pesada_envase_pesada FOREIGN KEY (pesada_id) REFERENCES agro.pesada (id) ON DELETE CASCADE DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_pesada_envase_tara FOREIGN KEY (tara_envase_id) REFERENCES agro.tara_envase (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.pale_entrada
                    ADD CONSTRAINT ck_pale_entrada_valores CHECK (envases >= 0 AND numero > 0 AND (kilos_netos IS NULL OR kilos_netos > 0) AND (kilos_asignados IS NULL OR kilos_asignados >= 0)),
                    ADD CONSTRAINT fk_pale_entrada_pale FOREIGN KEY (pale_id) REFERENCES agro.pale (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.pale
                    ADD CONSTRAINT fk_pale_linea_recepcion FOREIGN KEY (linea_recepcion_id) REFERENCES agro.linea_recepcion (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.linea_recepcion
                    ADD CONSTRAINT ck_linea_recepcion_kilos_liquidacion
                        CHECK (kilos_liquidacion IS NULL OR (kilos_liquidacion > 0 AND length(trim(coalesce(motivo_kilos_liquidacion, ''))) > 0));
                ALTER TABLE agro.tara_envase
                    ADD CONSTRAINT ck_tara_envase_valores CHECK (tara_kg >= 0 AND (hasta IS NULL OR hasta >= desde));

                -- Taras: sin solapes por envase, y una versión aplicada en alguna pesada no cambia ni se borra.
                CREATE OR REPLACE FUNCTION agro.tara_envase_valida() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF TG_OP IN ('UPDATE', 'DELETE') AND EXISTS (SELECT 1 FROM agro.pesada_envase e WHERE e.tara_envase_id = OLD.id)
                       AND NOT public.alxor_borrando_empresa(OLD.empresa_id)
                       AND (TG_OP = 'DELETE' OR NEW.tara_kg <> OLD.tara_kg OR NEW.desde <> OLD.desde OR NEW.envase_producto_id <> OLD.envase_producto_id
                            OR (NEW.hasta IS NOT NULL AND EXISTS (SELECT 1 FROM agro.pesada_envase e JOIN agro.recepcion r ON r.id = e.recepcion_id
                                                                   WHERE e.tara_envase_id = OLD.id AND r.fecha > NEW.hasta))) THEN
                        PERFORM public.alxor_error('tara.aplicada', 'Una tara aplicada en pesadas no cambia: da de alta una versión nueva desde la fecha en que cambia.');
                    END IF;
                    IF TG_OP = 'DELETE' THEN
                        RETURN OLD;
                    END IF;
                    IF EXISTS (SELECT 1 FROM agro.tara_envase t
                                WHERE t.empresa_id = NEW.empresa_id AND t.envase_producto_id = NEW.envase_producto_id AND t.id <> NEW.id
                                  AND t.desde <= coalesce(NEW.hasta, 'infinity'::date) AND coalesce(t.hasta, 'infinity'::date) >= NEW.desde) THEN
                        PERFORM public.alxor_error('tara.solapada', 'Ya hay una tara de ese envase en esas fechas.');
                    END IF;
                    RETURN NEW;
                END $f$;
                CREATE CONSTRAINT TRIGGER tg_tara_envase_solape AFTER INSERT OR UPDATE ON agro.tara_envase
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION agro.tara_envase_valida();
                CREATE TRIGGER tg_tara_envase_aplicada BEFORE UPDATE OR DELETE ON agro.tara_envase
                    FOR EACH ROW EXECUTE FUNCTION agro.tara_envase_valida();

                -- Recepción confirmada: además, la tara de cada pesada es la de sus envases (con la versión vigente ese día) y
                -- los palés de entrada llevan exactamente el neto de su línea, cada uno con su palé y su entrada de kilos.
                CREATE OR REPLACE FUNCTION agro.recepcion_pales_cuadran() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    mal record;
                BEGIN
                    IF NEW.estado <> 'Confirmada' THEN
                        RETURN NULL;
                    END IF;
                    SELECT p.secuencia INTO mal FROM agro.pesada p
                     WHERE p.recepcion_id = NEW.id AND p.tara_camion_kg IS NOT NULL
                       AND p.tara_envases_kg <> (SELECT coalesce(sum(e.cantidad * e.tara_unitaria_kg), 0) FROM agro.pesada_envase e WHERE e.pesada_id = p.id)
                     LIMIT 1;
                    IF FOUND THEN
                        PERFORM public.alxor_error('recepcion.tara_no_cuadra', format('La tara de envases de la pesada %s no es la de sus envases contados.', mal.secuencia));
                    END IF;
                    IF EXISTS (SELECT 1 FROM agro.pesada_envase e JOIN agro.tara_envase t ON t.id = e.tara_envase_id
                                WHERE e.recepcion_id = NEW.id AND (t.tara_kg <> e.tara_unitaria_kg OR NEW.fecha < t.desde OR NEW.fecha > coalesce(t.hasta, 'infinity'::date)
                                                                  OR t.envase_producto_id <> e.envase_producto_id)) THEN
                        PERFORM public.alxor_error('recepcion.tara_version', 'Un envase de la recepción no lleva la tara vigente en su fecha.');
                    END IF;
                    SELECT l.numero_linea INTO mal FROM agro.linea_recepcion l
                     WHERE l.recepcion_id = NEW.id AND EXISTS (SELECT 1 FROM agro.pale_entrada e WHERE e.linea_id = l.id)
                       AND (l.neto_kg <> (SELECT coalesce(sum(e.kilos_asignados), 0) FROM agro.pale_entrada e WHERE e.linea_id = l.id)
                            OR EXISTS (SELECT 1 FROM agro.pale_entrada e LEFT JOIN agro.pale pa ON pa.id = e.pale_id
                                        WHERE e.linea_id = l.id AND (e.pale_id IS NULL OR e.kilos_asignados IS NULL OR pa.linea_recepcion_id IS DISTINCT FROM l.id
                                              OR e.kilos_asignados <> (SELECT coalesce(sum(m.kilos), 0) FROM agro.movimiento_partida m
                                                                        WHERE m.pale_id = e.pale_id AND m.partida_id = l.partida_id AND m.tipo = 'Entrada'))))
                     LIMIT 1;
                    IF FOUND THEN
                        PERFORM public.alxor_error('recepcion.pales_no_cuadran', format('Los palés de entrada de la línea %s no llevan exactamente su neto.', mal.numero_linea));
                    END IF;
                    RETURN NULL;
                END $f$;
                CREATE CONSTRAINT TRIGGER tg_recepcion_pales_cuadran AFTER INSERT OR UPDATE ON agro.recepcion
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION agro.recepcion_pales_cuadran();
                """);

            // ----- Palés de entrada: la fruta entra ya en su palé cerrado y se consume de él sin abrirlo -----
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
                    IF partida_anulada AND NEW.tipo <> 'Anulacion' THEN
                        PERFORM public.alxor_error('partida.anulada', 'La partida está anulada.');
                    END IF;
                    IF NEW.pale_id IS NOT NULL THEN
                        SELECT p.estado, p.sscc, p.linea_recepcion_id INTO est, sscc, linea_pale FROM agro.pale p WHERE p.id = NEW.pale_id;
                        IF (NEW.tipo = 'Expedicion' AND est <> 'Expedido')
                           OR (NEW.tipo IN ('Paletizado', 'Ajuste') AND est <> 'Abierto')
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

            // ----- Liquidación: por los kilos de liquidación si se fijaron (con motivo); si no, por el neto -----
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
                     WHERE s.kilos <> coalesce(lr.kilos_liquidacion, lr.neto_kg)
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
            migrationBuilder.Sql(GarantiasSql.QuitarRlsPorPadre("agro", "pesada_envase"));
            migrationBuilder.Sql(GarantiasSql.QuitarRlsPorPadre("agro", "pale_entrada"));
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS tg_recepcion_pales_cuadran ON agro.recepcion;
                DROP FUNCTION IF EXISTS agro.recepcion_pales_cuadran();
                DROP TRIGGER IF EXISTS tg_tara_envase_solape ON agro.tara_envase;
                DROP TRIGGER IF EXISTS tg_tara_envase_aplicada ON agro.tara_envase;
                DROP FUNCTION IF EXISTS agro.tara_envase_valida();
                ALTER TABLE agro.pale DROP CONSTRAINT IF EXISTS fk_pale_linea_recepcion;
                ALTER TABLE agro.pesada DROP CONSTRAINT IF EXISTS ck_pesada_tara_desglose;
                ALTER TABLE agro.linea_recepcion DROP CONSTRAINT IF EXISTS ck_linea_recepcion_kilos_liquidacion;
                """);

            migrationBuilder.DropTable(
                name: "pale_entrada",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "pesada_envase",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "tara_envase",
                schema: "agro");

            migrationBuilder.DropIndex(
                name: "ix_pale_linea_recepcion",
                schema: "agro",
                table: "pale");

            migrationBuilder.DropColumn(
                name: "tara_camion_kg",
                schema: "agro",
                table: "pesada");

            migrationBuilder.DropColumn(
                name: "tara_envases_kg",
                schema: "agro",
                table: "pesada");

            migrationBuilder.DropColumn(
                name: "linea_recepcion_id",
                schema: "agro",
                table: "pale");

            migrationBuilder.DropColumn(
                name: "serie_origen",
                schema: "agro",
                table: "pale");

            migrationBuilder.DropColumn(
                name: "kilos_liquidacion",
                schema: "agro",
                table: "linea_recepcion");

            migrationBuilder.DropColumn(
                name: "motivo_kilos_liquidacion",
                schema: "agro",
                table: "linea_recepcion");
        }
    }
}
