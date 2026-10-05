using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Vivero.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class InicialVivero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "vivero");

            migrationBuilder.CreateTable(
                name: "configuracion",
                schema: "vivero",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo_registro = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    pais_origen = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lote_planta",
                schema: "vivero",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    especie = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    variedad = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    portainjerto = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    origen_material = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    fecha_siembra = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_prevista_lista = table.Column<DateOnly>(type: "date", nullable: true),
                    fase = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ubicacion = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    plantas_iniciales = table.Column<int>(type: "integer", nullable: false),
                    plantas_vivas = table.Column<int>(type: "integer", nullable: false),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    anulado = table.Column<bool>(type: "boolean", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lote_planta", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "encargo",
                schema: "vivero",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plantas = table.Column<int>(type: "integer", nullable: false),
                    fecha_entrega = table.Column<DateOnly>(type: "date", nullable: false),
                    precio_planta = table.Column<decimal>(type: "numeric(12,4)", nullable: true),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    lote_id = table.Column<Guid>(type: "uuid", nullable: true),
                    movimiento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    albaran_id = table.Column<Guid>(type: "uuid", nullable: true),
                    albaran_numero = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_encargo", x => x.id);
                    table.ForeignKey(
                        name: "fk_encargo_lote",
                        column: x => x.lote_id,
                        principalSchema: "vivero",
                        principalTable: "lote_planta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "movimiento_lote",
                schema: "vivero",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    plantas = table.Column<int>(type: "integer", nullable: false),
                    fase = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ubicacion = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    concepto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    anula_id = table.Column<Guid>(type: "uuid", nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    lote_planta_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimiento_lote", x => x.id);
                    table.ForeignKey(
                        name: "FK_movimiento_lote_lote_planta_lote_planta_id",
                        column: x => x.lote_planta_id,
                        principalSchema: "vivero",
                        principalTable: "lote_planta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_configuracion_vivero_empresa",
                schema: "vivero",
                table: "configuracion",
                column: "empresa_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_encargo_cliente",
                schema: "vivero",
                table: "encargo",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_encargo_lote",
                schema: "vivero",
                table: "encargo",
                column: "lote_id");

            migrationBuilder.CreateIndex(
                name: "ix_encargo_producto",
                schema: "vivero",
                table: "encargo",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "ux_encargo_numero",
                schema: "vivero",
                table: "encargo",
                columns: new[] { "empresa_id", "ejercicio", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lote_planta_producto",
                schema: "vivero",
                table: "lote_planta",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "ux_lote_planta_numero",
                schema: "vivero",
                table: "lote_planta",
                columns: new[] { "empresa_id", "ejercicio", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_movimiento_lote_fecha",
                schema: "vivero",
                table: "movimiento_lote",
                column: "fecha");

            migrationBuilder.CreateIndex(
                name: "ux_movimiento_lote_anula",
                schema: "vivero",
                table: "movimiento_lote",
                column: "anula_id",
                unique: true,
                filter: "anula_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_movimiento_lote_orden",
                schema: "vivero",
                table: "movimiento_lote",
                columns: new[] { "lote_planta_id", "orden" },
                unique: true);

            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.FuncionesComunes);
            foreach (var tabla in new[] { "configuracion", "lote_planta", "encargo" })
            {
                migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("vivero", tabla));
            }

            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("vivero", "movimiento_lote", "lote_planta_id", "vivero", "lote_planta"));
            migrationBuilder.Sql("""
                ALTER TABLE vivero.configuracion ADD CONSTRAINT ck_configuracion_vivero_pais CHECK (pais_origen ~ '^[A-Z]{2}$');
                ALTER TABLE vivero.lote_planta
                    ADD CONSTRAINT ck_lote_planta_fase CHECK (fase IN ('Semillero','Injerto','Crecimiento','Lista')),
                    ADD CONSTRAINT ck_lote_planta_plantas CHECK (plantas_iniciales > 0 AND plantas_vivas >= 0 AND plantas_vivas <= plantas_iniciales);
                ALTER TABLE vivero.movimiento_lote
                    ADD CONSTRAINT ck_movimiento_lote_tipo CHECK (tipo IN ('Siembra','Baja','Entrega','PasoExistencias','Anulacion','Cambio')),
                    ADD CONSTRAINT ck_movimiento_lote_signo CHECK (
                        (tipo = 'Siembra' AND plantas > 0) OR (tipo IN ('Baja','Entrega','PasoExistencias') AND plantas < 0)
                        OR (tipo = 'Cambio' AND plantas = 0) OR (tipo = 'Anulacion' AND plantas > 0 AND anula_id IS NOT NULL));
                ALTER TABLE vivero.encargo
                    ADD CONSTRAINT ck_encargo_estado CHECK (estado IN ('Pendiente','Reservado','Servido','Anulado')),
                    ADD CONSTRAINT ck_encargo_plantas CHECK (plantas > 0 AND (precio_planta IS NULL OR precio_planta >= 0) AND fecha_entrega >= fecha),
                    ADD CONSTRAINT ck_encargo_reserva CHECK ((estado = 'Pendiente' AND lote_id IS NULL) OR (estado IN ('Reservado','Servido') AND lote_id IS NOT NULL)
                        OR estado = 'Anulado'),
                    ADD CONSTRAINT ck_encargo_servido CHECK ((estado = 'Servido') = (albaran_id IS NOT NULL AND movimiento_id IS NOT NULL) OR estado = 'Anulado');

                -- El libro del lote solo crece; al confirmar, las plantas vivas son la suma de sus movimientos.
                CREATE OR REPLACE FUNCTION vivero.movimiento_lote_fijo() RETURNS trigger LANGUAGE plpgsql AS $f$
                BEGIN
                    IF public.alxor_borrando_empresa(NULL) THEN RETURN coalesce(NEW, OLD); END IF;
                    PERFORM public.alxor_error('lote_planta.inmutable', 'Los movimientos de un lote de planta no se cambian ni se borran: se anulan.');
                    RETURN NULL;
                END $f$;
                CREATE TRIGGER tr_movimiento_lote_fijo BEFORE UPDATE OR DELETE ON vivero.movimiento_lote
                    FOR EACH ROW EXECUTE FUNCTION vivero.movimiento_lote_fijo();
                CREATE OR REPLACE FUNCTION vivero.lote_planta_cuadra() RETURNS trigger LANGUAGE plpgsql AS $f$
                DECLARE l record; suma integer; lote uuid;
                BEGIN
                    IF TG_TABLE_NAME = 'lote_planta' THEN lote := NEW.id; ELSE lote := NEW.lote_planta_id; END IF;
                    SELECT id, ejercicio, numero, plantas_vivas INTO l FROM vivero.lote_planta WHERE id = lote;
                    IF NOT FOUND THEN RETURN NULL; END IF;
                    SELECT coalesce(sum(plantas), 0) INTO suma FROM vivero.movimiento_lote WHERE lote_planta_id = l.id;
                    IF suma <> l.plantas_vivas THEN
                        PERFORM public.alxor_error('lote_planta.descuadre', format('El lote LP%s-%s tiene %s plantas vivas y sus movimientos suman %s.',
                            l.ejercicio, lpad(l.numero::text, 4, '0'), l.plantas_vivas, suma));
                    END IF;
                    RETURN NULL;
                END $f$;
                CREATE CONSTRAINT TRIGGER tr_lote_planta_cuadra AFTER INSERT OR UPDATE ON vivero.lote_planta
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION vivero.lote_planta_cuadra();
                CREATE CONSTRAINT TRIGGER tr_movimiento_lote_cuadra AFTER INSERT ON vivero.movimiento_lote
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION vivero.lote_planta_cuadra();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP FUNCTION IF EXISTS vivero.movimiento_lote_fijo() CASCADE;
                DROP FUNCTION IF EXISTS vivero.lote_planta_cuadra() CASCADE;
                """);
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.QuitarRlsPorPadre("vivero", "movimiento_lote"));
            migrationBuilder.DropTable(
                name: "configuracion",
                schema: "vivero");

            migrationBuilder.DropTable(
                name: "encargo",
                schema: "vivero");

            migrationBuilder.DropTable(
                name: "movimiento_lote",
                schema: "vivero");

            migrationBuilder.DropTable(
                name: "lote_planta",
                schema: "vivero");
        }
    }
}
