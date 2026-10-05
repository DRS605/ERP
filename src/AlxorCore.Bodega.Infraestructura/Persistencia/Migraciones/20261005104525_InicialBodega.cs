using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Bodega.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class InicialBodega : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "bodega");

            migrationBuilder.CreateTable(
                name: "deposito",
                schema: "bodega",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    capacidad_litros = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    litros = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    producto = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    calificacion = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    ultima_operacion_id = table.Column<Guid>(type: "uuid", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deposito", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entrada_uva",
                schema: "bodega",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    anada = table.Column<int>(type: "integer", nullable: false),
                    viticultor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    viticultor_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    variedad = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    kilos = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    grado_baume = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    parcela = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    calificacion = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    operacion_id = table.Column<Guid>(type: "uuid", nullable: true),
                    liquidacion_id = table.Column<Guid>(type: "uuid", nullable: true),
                    anulada = table.Column<bool>(type: "boolean", nullable: false),
                    motivo_anulacion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    creada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entrada_uva", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "liquidacion_uva",
                schema: "bodega",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    viticultor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    viticultor_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    desde = table.Column<DateOnly>(type: "date", nullable: false),
                    hasta = table.Column<DateOnly>(type: "date", nullable: false),
                    codigo_impuesto = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    porcentaje_retencion = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    kilos = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    base_imponible = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    gasto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    total_factura = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    motivo_anulacion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    creada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_liquidacion_uva", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "operacion",
                schema: "bodega",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    merma_litros = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    kilos_uva = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    botellas = table.Column<int>(type: "integer", nullable: true),
                    formato_litros = table.Column<decimal>(type: "numeric(6,3)", nullable: true),
                    lote = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    albaran_id = table.Column<Guid>(type: "uuid", nullable: true),
                    albaran_numero = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    anulada = table.Column<bool>(type: "boolean", nullable: false),
                    motivo_anulacion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    creada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operacion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "precio_uva",
                schema: "bodega",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    anada = table.Column<int>(type: "integer", nullable: false),
                    variedad = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    precio_kg = table.Column<decimal>(type: "numeric(12,6)", nullable: false),
                    grado_referencia = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    porcentaje_por_grado = table.Column<decimal>(type: "numeric(7,2)", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_precio_uva", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "componente_deposito",
                schema: "bodega",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    variedad = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    anada = table.Column<int>(type: "integer", nullable: false),
                    litros = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    deposito_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_componente_deposito", x => x.id);
                    table.ForeignKey(
                        name: "FK_componente_deposito_deposito_deposito_id",
                        column: x => x.deposito_id,
                        principalSchema: "bodega",
                        principalTable: "deposito",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "linea_liquidacion_uva",
                schema: "bodega",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entrada_uva_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entrada = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    variedad = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    kilos = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    grado_baume = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    precio_kg = table.Column<decimal>(type: "numeric(12,6)", nullable: false),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    liquidacion_uva_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_liquidacion_uva", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_liquidacion_uva_liquidacion_uva_liquidacion_uva_id",
                        column: x => x.liquidacion_uva_id,
                        principalSchema: "bodega",
                        principalTable: "liquidacion_uva",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "linea_operacion",
                schema: "bodega",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    deposito_id = table.Column<Guid>(type: "uuid", nullable: false),
                    deposito_codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    clase = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    litros = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    producto = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    calificacion = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    estado_anterior = table.Column<string>(type: "jsonb", nullable: true),
                    operacion_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_operacion", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_operacion_operacion_operacion_id",
                        column: x => x.operacion_id,
                        principalSchema: "bodega",
                        principalTable: "operacion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_linea_operacion_deposito",
                        column: x => x.deposito_id,
                        principalSchema: "bodega",
                        principalTable: "deposito",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ux_componente_deposito",
                schema: "bodega",
                table: "componente_deposito",
                columns: new[] { "deposito_id", "variedad", "anada" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_deposito_codigo",
                schema: "bodega",
                table: "deposito",
                columns: new[] { "empresa_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_entrada_uva_liquidacion",
                schema: "bodega",
                table: "entrada_uva",
                column: "liquidacion_id");

            migrationBuilder.CreateIndex(
                name: "ix_entrada_uva_operacion",
                schema: "bodega",
                table: "entrada_uva",
                column: "operacion_id");

            migrationBuilder.CreateIndex(
                name: "ix_entrada_uva_proveedor",
                schema: "bodega",
                table: "entrada_uva",
                column: "viticultor_id");

            migrationBuilder.CreateIndex(
                name: "ix_entrada_uva_viticultor",
                schema: "bodega",
                table: "entrada_uva",
                columns: new[] { "empresa_id", "viticultor_id", "fecha" });

            migrationBuilder.CreateIndex(
                name: "ux_entrada_uva_numero",
                schema: "bodega",
                table: "entrada_uva",
                columns: new[] { "empresa_id", "ejercicio", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_linea_liquidacion_uva_liquidacion_uva_id",
                schema: "bodega",
                table: "linea_liquidacion_uva",
                column: "liquidacion_uva_id");

            migrationBuilder.CreateIndex(
                name: "ix_linea_liquidacion_uva_entrada",
                schema: "bodega",
                table: "linea_liquidacion_uva",
                column: "entrada_uva_id");

            migrationBuilder.CreateIndex(
                name: "ix_linea_operacion_deposito",
                schema: "bodega",
                table: "linea_operacion",
                column: "deposito_id");

            migrationBuilder.CreateIndex(
                name: "ux_linea_operacion_orden",
                schema: "bodega",
                table: "linea_operacion",
                columns: new[] { "operacion_id", "orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_liquidacion_uva_proveedor",
                schema: "bodega",
                table: "liquidacion_uva",
                column: "viticultor_id");

            migrationBuilder.CreateIndex(
                name: "ux_liquidacion_uva_numero",
                schema: "bodega",
                table: "liquidacion_uva",
                columns: new[] { "empresa_id", "ejercicio", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_operacion_bodega_cliente",
                schema: "bodega",
                table: "operacion",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_operacion_bodega_fecha",
                schema: "bodega",
                table: "operacion",
                columns: new[] { "empresa_id", "fecha" });

            migrationBuilder.CreateIndex(
                name: "ix_operacion_bodega_producto",
                schema: "bodega",
                table: "operacion",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "ux_operacion_bodega_numero",
                schema: "bodega",
                table: "operacion",
                columns: new[] { "empresa_id", "ejercicio", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_precio_uva",
                schema: "bodega",
                table: "precio_uva",
                columns: new[] { "empresa_id", "anada", "variedad" },
                unique: true);

            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.FuncionesComunes);
            foreach (var tabla in new[] { "deposito", "entrada_uva", "precio_uva", "liquidacion_uva", "operacion" })
            {
                migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("bodega", tabla));
            }

            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("bodega", "componente_deposito", "deposito_id", "bodega", "deposito"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("bodega", "linea_liquidacion_uva", "liquidacion_uva_id", "bodega", "liquidacion_uva"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("bodega", "linea_operacion", "operacion_id", "bodega", "operacion"));
            migrationBuilder.Sql("""
                ALTER TABLE bodega.deposito
                    ADD CONSTRAINT ck_deposito_tipo CHECK (tipo IN ('Acero','Hormigon','Barrica','Tinaja','Otro')),
                    ADD CONSTRAINT ck_deposito_producto CHECK (producto IS NULL OR producto IN ('Mosto','VinoBlanco','VinoRosado','VinoTinto','Otro')),
                    -- Nunca más de lo que cabe ni menos de nada; vacío, sin producto ni calificación; con vino, con producto.
                    ADD CONSTRAINT ck_deposito_litros CHECK (capacidad_litros > 0 AND litros >= 0 AND litros <= capacidad_litros),
                    ADD CONSTRAINT ck_deposito_contenido CHECK ((litros = 0 AND producto IS NULL AND calificacion IS NULL) OR (litros > 0 AND producto IS NOT NULL)),
                    ADD CONSTRAINT ck_deposito_baja CHECK (activo OR litros = 0);
                ALTER TABLE bodega.componente_deposito ADD CONSTRAINT ck_componente_deposito_litros CHECK (litros > 0);
                ALTER TABLE bodega.entrada_uva
                    ADD CONSTRAINT ck_entrada_uva_datos CHECK (kilos > 0 AND grado_baume BETWEEN 0 AND 30 AND anada BETWEEN 1900 AND 2200);
                ALTER TABLE bodega.precio_uva
                    ADD CONSTRAINT ck_precio_uva_datos CHECK (precio_kg >= 0 AND grado_referencia > 0 AND porcentaje_por_grado BETWEEN 0 AND 100);
                ALTER TABLE bodega.liquidacion_uva ADD CONSTRAINT ck_liquidacion_uva_estado CHECK (estado IN ('Emitida','Anulada'));
                ALTER TABLE bodega.linea_liquidacion_uva ADD CONSTRAINT ck_linea_liquidacion_uva_importe CHECK (importe = round(kilos * precio_kg, 2));
                ALTER TABLE bodega.operacion
                    ADD CONSTRAINT ck_operacion_tipo CHECK (tipo IN ('Elaboracion','Trasiego','Coupage','Merma','Embotellado','SalidaGranel')),
                    ADD CONSTRAINT ck_operacion_merma CHECK (merma_litros >= 0);
                ALTER TABLE bodega.linea_operacion
                    ADD CONSTRAINT ck_linea_operacion_clase CHECK (clase IN ('Sale','Entra','Reclasifica','Merma')),
                    ADD CONSTRAINT ck_linea_operacion_signo CHECK (litros <> 0 AND (clase = 'Reclasifica' OR (clase = 'Entra') = (litros > 0)));

                """);
            migrationBuilder.Sql("""
                -- Al confirmar, la composición de cada depósito suma exactamente sus litros.
                CREATE OR REPLACE FUNCTION bodega.deposito_cuadra_deposito() RETURNS trigger LANGUAGE plpgsql AS $f$
                DECLARE suma numeric;
                BEGIN
                    IF TG_OP = 'DELETE' THEN RETURN NULL; END IF;
                    SELECT coalesce(sum(litros), 0) INTO suma FROM bodega.componente_deposito WHERE deposito_id = NEW.id;
                    IF suma <> NEW.litros THEN
                        PERFORM public.alxor_error('deposito.composicion', format('La composición del depósito %s suma %s l y contiene %s l.', NEW.codigo, suma, NEW.litros));
                    END IF;
                    RETURN NULL;
                END $f$;
                CREATE OR REPLACE FUNCTION bodega.componente_cuadra() RETURNS trigger LANGUAGE plpgsql AS $f$
                DECLARE d record; suma numeric;
                BEGIN
                    SELECT id, codigo, litros INTO d FROM bodega.deposito WHERE id = coalesce(NEW.deposito_id, OLD.deposito_id);
                    IF NOT FOUND THEN RETURN NULL; END IF;
                    SELECT coalesce(sum(litros), 0) INTO suma FROM bodega.componente_deposito WHERE deposito_id = d.id;
                    IF suma <> d.litros THEN
                        PERFORM public.alxor_error('deposito.composicion', format('La composición del depósito %s suma %s l y contiene %s l.', d.codigo, suma, d.litros));
                    END IF;
                    RETURN NULL;
                END $f$;
                CREATE CONSTRAINT TRIGGER tr_deposito_cuadra AFTER INSERT OR UPDATE ON bodega.deposito
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION bodega.deposito_cuadra_deposito();
                CREATE CONSTRAINT TRIGGER tr_componente_cuadra AFTER INSERT OR UPDATE OR DELETE ON bodega.componente_deposito
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION bodega.componente_cuadra();

                -- Las operaciones no se borran ni se cambian sus movimientos: se anulan.
                CREATE OR REPLACE FUNCTION bodega.linea_operacion_fija() RETURNS trigger LANGUAGE plpgsql AS $f$
                BEGIN
                    IF public.alxor_borrando_empresa(NULL) THEN RETURN coalesce(NEW, OLD); END IF;
                    PERFORM public.alxor_error('operacion_bodega.inmutable', 'Los movimientos de una operación de bodega no se cambian ni se borran: la operación se anula.');
                    RETURN NULL;
                END $f$;
                CREATE TRIGGER tr_linea_operacion_fija BEFORE UPDATE OR DELETE ON bodega.linea_operacion
                    FOR EACH ROW EXECUTE FUNCTION bodega.linea_operacion_fija();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP FUNCTION IF EXISTS bodega.deposito_cuadra_deposito() CASCADE;
                DROP FUNCTION IF EXISTS bodega.componente_cuadra() CASCADE;
                DROP FUNCTION IF EXISTS bodega.linea_operacion_fija() CASCADE;
                """);
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.QuitarRlsPorPadre("bodega", "componente_deposito"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.QuitarRlsPorPadre("bodega", "linea_liquidacion_uva"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.QuitarRlsPorPadre("bodega", "linea_operacion"));
            migrationBuilder.DropTable(
                name: "componente_deposito",
                schema: "bodega");

            migrationBuilder.DropTable(
                name: "entrada_uva",
                schema: "bodega");

            migrationBuilder.DropTable(
                name: "linea_liquidacion_uva",
                schema: "bodega");

            migrationBuilder.DropTable(
                name: "linea_operacion",
                schema: "bodega");

            migrationBuilder.DropTable(
                name: "precio_uva",
                schema: "bodega");

            migrationBuilder.DropTable(
                name: "liquidacion_uva",
                schema: "bodega");

            migrationBuilder.DropTable(
                name: "operacion",
                schema: "bodega");

            migrationBuilder.DropTable(
                name: "deposito",
                schema: "bodega");
        }
    }
}
