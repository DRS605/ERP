using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Agro.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class CalidadRecepcion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "muestreo_calidad",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    recepcion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    linea_recepcion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plantilla_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    peso_muestra_kg = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    definitivo = table.Column<bool>(type: "boolean", nullable: false),
                    descuento_pct = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    observaciones = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    anulado = table.Column<bool>(type: "boolean", nullable: false),
                    motivo_anulacion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_muestreo_calidad", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plantilla_calidad",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    familia_id = table.Column<Guid>(type: "uuid", nullable: true),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plantilla_calidad", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "resultado_muestreo",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    defecto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    defecto = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    kilos = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    porcentaje = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    descuenta_peso = table.Column<bool>(type: "boolean", nullable: false),
                    muestreo_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resultado_muestreo", x => x.id);
                    table.ForeignKey(
                        name: "FK_resultado_muestreo_muestreo_calidad_muestreo_id",
                        column: x => x.muestreo_id,
                        principalSchema: "agro",
                        principalTable: "muestreo_calidad",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "defecto_calidad",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    descuenta_peso = table.Column<bool>(type: "boolean", nullable: false),
                    tolerancia_pct = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    maximo_pct = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    plantilla_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_defecto_calidad", x => x.id);
                    table.ForeignKey(
                        name: "FK_defecto_calidad_plantilla_calidad_plantilla_id",
                        column: x => x.plantilla_id,
                        principalSchema: "agro",
                        principalTable: "plantilla_calidad",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_defecto_calidad_plantilla",
                schema: "agro",
                table: "defecto_calidad",
                column: "plantilla_id");

            migrationBuilder.CreateIndex(
                name: "ix_muestreo_calidad_plantilla",
                schema: "agro",
                table: "muestreo_calidad",
                column: "plantilla_id");

            migrationBuilder.CreateIndex(
                name: "ix_muestreo_calidad_recepcion",
                schema: "agro",
                table: "muestreo_calidad",
                column: "recepcion_id");

            migrationBuilder.CreateIndex(
                name: "ix_muestreo_calidad_usuario",
                schema: "agro",
                table: "muestreo_calidad",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ux_muestreo_calidad_definitivo",
                schema: "agro",
                table: "muestreo_calidad",
                column: "linea_recepcion_id",
                unique: true,
                filter: "definitivo AND NOT anulado");

            migrationBuilder.CreateIndex(
                name: "ix_plantilla_calidad_familia",
                schema: "agro",
                table: "plantilla_calidad",
                column: "familia_id");

            migrationBuilder.CreateIndex(
                name: "ix_plantilla_calidad_producto",
                schema: "agro",
                table: "plantilla_calidad",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "ux_plantilla_calidad_codigo",
                schema: "agro",
                table: "plantilla_calidad",
                columns: new[] { "empresa_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_resultado_muestreo_defecto",
                schema: "agro",
                table: "resultado_muestreo",
                column: "defecto_id");

            migrationBuilder.CreateIndex(
                name: "ix_resultado_muestreo_muestreo",
                schema: "agro",
                table: "resultado_muestreo",
                column: "muestreo_id");

            // =============================== Garantías de la base de datos ===============================
            foreach (var tabla in new[] { "plantilla_calidad", "muestreo_calidad" })
            {
                migrationBuilder.Sql(RlsSql.Activar("agro", tabla));
            }

            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("agro", "defecto_calidad", "plantilla_id", "agro", "plantilla_calidad"));
            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("agro", "resultado_muestreo", "muestreo_id", "agro", "muestreo_calidad"));
            migrationBuilder.Sql(GarantiasSql.MarcarAlta("agro", "muestreo_calidad"));
            migrationBuilder.Sql(GarantiasSql.HijaDeAlta("agro", "resultado_muestreo", "muestreo_id", "agro", "muestreo_calidad", "muestreo.inmutable",
                "Los resultados de un muestreo se registran con él y no se cambian."));
            migrationBuilder.Sql(GarantiasSql.SoloInsercion("agro", "resultado_muestreo", "muestreo.inmutable", "Los resultados de un muestreo se registran con él y no se cambian."));

            migrationBuilder.Sql("""
                ALTER TABLE agro.defecto_calidad
                    ADD CONSTRAINT ck_defecto_calidad_limites CHECK ((tolerancia_pct IS NULL OR tolerancia_pct BETWEEN 0 AND 100) AND (maximo_pct IS NULL OR maximo_pct BETWEEN 0 AND 100)
                        AND (tolerancia_pct IS NULL OR maximo_pct IS NULL OR maximo_pct >= tolerancia_pct));
                ALTER TABLE agro.plantilla_calidad ADD CONSTRAINT ck_plantilla_calidad_ambito CHECK (producto_id IS NULL OR familia_id IS NULL);
                ALTER TABLE agro.muestreo_calidad
                    ADD CONSTRAINT ck_muestreo_calidad_valores CHECK (peso_muestra_kg > 0 AND descuento_pct BETWEEN 0 AND 100 AND (definitivo OR descuento_pct >= 0)
                        AND (NOT anulado OR length(trim(coalesce(motivo_anulacion, ''))) > 0)),
                    ADD CONSTRAINT fk_muestreo_calidad_recepcion FOREIGN KEY (recepcion_id) REFERENCES agro.recepcion (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_muestreo_calidad_linea FOREIGN KEY (linea_recepcion_id) REFERENCES agro.linea_recepcion (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_muestreo_calidad_plantilla FOREIGN KEY (plantilla_id) REFERENCES agro.plantilla_calidad (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.resultado_muestreo
                    ADD CONSTRAINT ck_resultado_muestreo_valores CHECK (kilos >= 0 AND porcentaje BETWEEN 0 AND 100),
                    ADD CONSTRAINT fk_resultado_muestreo_defecto FOREIGN KEY (defecto_id) REFERENCES agro.defecto_calidad (id) DEFERRABLE INITIALLY DEFERRED;

                -- Un muestreo no cambia: solo se anula (con motivo), y el definitivo no se anula si la entrega está liquidada.
                CREATE OR REPLACE FUNCTION agro.muestreo_calidad_inmutable() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF TG_OP = 'DELETE' THEN
                        IF NOT public.alxor_borrando_empresa(OLD.empresa_id) THEN
                            PERFORM public.alxor_error('muestreo.inmutable', 'Los muestreos no se borran: se anulan con motivo.');
                        END IF;
                        RETURN OLD;
                    END IF;
                    IF (to_jsonb(NEW) - 'anulado' - 'motivo_anulacion') <> (to_jsonb(OLD) - 'anulado' - 'motivo_anulacion') OR (OLD.anulado AND NOT NEW.anulado) THEN
                        PERFORM public.alxor_error('muestreo.inmutable', 'Un muestreo no se cambia: se anula con motivo y se hace otro.');
                    END IF;
                    RETURN NEW;
                END $f$;
                CREATE TRIGGER tg_muestreo_calidad_inmutable BEFORE UPDATE OR DELETE ON agro.muestreo_calidad
                    FOR EACH ROW EXECUTE FUNCTION agro.muestreo_calidad_inmutable();

                -- Coherente al confirmar: la línea es de la recepción, cada porcentaje es el de sus kilos, los defectos no pesan más
                -- que la muestra y el descuento es la suma de los que descuentan; un definitivo vivo por línea y fuera de liquidaciones.
                CREATE OR REPLACE FUNCTION agro.muestreo_calidad_valido() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM agro.linea_recepcion l WHERE l.id = NEW.linea_recepcion_id AND l.recepcion_id = NEW.recepcion_id) THEN
                        PERFORM public.alxor_error('muestreo.incoherente', 'El muestreo tiene que ser de una línea de su recepción.');
                    END IF;
                    IF EXISTS (SELECT 1 FROM agro.resultado_muestreo r WHERE r.muestreo_id = NEW.id
                                  AND (r.porcentaje <> round(r.kilos * 100 / NEW.peso_muestra_kg, 2)
                                       OR NOT EXISTS (SELECT 1 FROM agro.defecto_calidad d WHERE d.id = r.defecto_id AND d.plantilla_id = NEW.plantilla_id)))
                       OR (SELECT coalesce(sum(kilos), 0) FROM agro.resultado_muestreo WHERE muestreo_id = NEW.id) > NEW.peso_muestra_kg
                       OR NEW.descuento_pct <> least(100, (SELECT coalesce(sum(porcentaje), 0) FROM agro.resultado_muestreo WHERE muestreo_id = NEW.id AND descuenta_peso)) THEN
                        PERFORM public.alxor_error('muestreo.no_cuadra', 'Los resultados del muestreo no cuadran con su muestra o su descuento.');
                    END IF;
                    IF NEW.definitivo AND EXISTS (
                        SELECT 1 FROM agro.linea_liquidacion l JOIN agro.liquidacion q ON q.id = l.liquidacion_id
                         WHERE l.linea_recepcion_id = NEW.linea_recepcion_id AND q.estado <> 'Anulada') THEN
                        PERFORM public.alxor_error('muestreo.liquidada', 'La entrega está en una liquidación: su muestreo definitivo no cambia.');
                    END IF;
                    RETURN NULL;
                END $f$;
                CREATE CONSTRAINT TRIGGER tg_muestreo_calidad_valido AFTER INSERT OR UPDATE ON agro.muestreo_calidad
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION agro.muestreo_calidad_valido();
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
                               -- El neto real rectificado, menos el descuento del muestreo de calidad definitivo.
                               round((lr.neto_kg + (SELECT coalesce(sum(r.diferencia_kg), 0) FROM agro.rectificacion_recepcion r WHERE r.linea_recepcion_id = lr.id))
                                     * (100 - coalesce((SELECT m.descuento_pct FROM agro.muestreo_calidad m
                                                         WHERE m.linea_recepcion_id = lr.id AND m.definitivo AND NOT m.anulado), 0)) / 100, 3))
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
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS tg_muestreo_calidad_valido ON agro.muestreo_calidad;
                DROP FUNCTION IF EXISTS agro.muestreo_calidad_valido();
                DROP TRIGGER IF EXISTS tg_muestreo_calidad_inmutable ON agro.muestreo_calidad;
                DROP FUNCTION IF EXISTS agro.muestreo_calidad_inmutable();
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
            migrationBuilder.Sql(GarantiasSql.QuitarRlsPorPadre("agro", "defecto_calidad"));
            migrationBuilder.Sql(GarantiasSql.QuitarRlsPorPadre("agro", "resultado_muestreo"));


            migrationBuilder.DropTable(
                name: "defecto_calidad",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "resultado_muestreo",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "plantilla_calidad",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "muestreo_calidad",
                schema: "agro");
        }
    }
}
