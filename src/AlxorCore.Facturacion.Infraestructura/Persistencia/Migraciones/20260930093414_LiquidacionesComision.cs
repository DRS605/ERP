using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Facturacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class LiquidacionesComision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "liquidacion_comision",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: true),
                    referencia_cliente = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    modo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    proveedor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    codigo_iva_gastos = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    gasto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    motivo_anulacion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_liquidacion_comision", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "gasto_liquidacion_comision",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    porcentaje = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    liquidacion_comision_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gasto_liquidacion_comision", x => x.id);
                    table.ForeignKey(
                        name: "FK_gasto_liquidacion_comision_liquidacion_comision_liquidacion~",
                        column: x => x.liquidacion_comision_id,
                        principalSchema: "facturacion",
                        principalTable: "liquidacion_comision",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "linea_liquidacion_comision",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    albaran_venta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    albaran_numero = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    albaran_fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    orden_albaran = table.Column<int>(type: "integer", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    cantidad_enviada = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    cantidad_vendida = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    precio_bruto = table.Column<decimal>(type: "numeric(14,4)", nullable: false),
                    importe_bruto = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    gastos = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    precio_albaran = table.Column<decimal>(type: "numeric(14,4)", nullable: false),
                    precio_estimado = table.Column<decimal>(type: "numeric(14,4)", nullable: false),
                    descuento_anterior = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    liquidacion_comision_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_liquidacion_comision", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_liquidacion_comision_liquidacion_comision_liquidacion~",
                        column: x => x.liquidacion_comision_id,
                        principalSchema: "facturacion",
                        principalTable: "liquidacion_comision",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_gasto_liquidacion_comision_liquidacion",
                schema: "facturacion",
                table: "gasto_liquidacion_comision",
                column: "liquidacion_comision_id");

            migrationBuilder.CreateIndex(
                name: "ix_linea_liquidacion_comision_albaran",
                schema: "facturacion",
                table: "linea_liquidacion_comision",
                columns: new[] { "albaran_venta_id", "orden_albaran" });

            migrationBuilder.CreateIndex(
                name: "ix_linea_liquidacion_comision_liquidacion",
                schema: "facturacion",
                table: "linea_liquidacion_comision",
                column: "liquidacion_comision_id");

            migrationBuilder.CreateIndex(
                name: "ix_linea_liquidacion_comision_producto",
                schema: "facturacion",
                table: "linea_liquidacion_comision",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "ix_liquidacion_comision_cliente",
                schema: "facturacion",
                table: "liquidacion_comision",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_liquidacion_comision_fecha",
                schema: "facturacion",
                table: "liquidacion_comision",
                columns: new[] { "empresa_id", "fecha" });

            migrationBuilder.CreateIndex(
                name: "ix_liquidacion_comision_gasto",
                schema: "facturacion",
                table: "liquidacion_comision",
                column: "gasto_id");

            migrationBuilder.CreateIndex(
                name: "ix_liquidacion_comision_numero",
                schema: "facturacion",
                table: "liquidacion_comision",
                columns: new[] { "empresa_id", "numero" });

            migrationBuilder.CreateIndex(
                name: "ix_liquidacion_comision_proveedor",
                schema: "facturacion",
                table: "liquidacion_comision",
                column: "proveedor_id");

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("facturacion", "liquidacion_comision"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("facturacion", "linea_liquidacion_comision", "liquidacion_comision_id", "facturacion", "liquidacion_comision"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("facturacion", "gasto_liquidacion_comision", "liquidacion_comision_id", "facturacion", "liquidacion_comision"));
            migrationBuilder.Sql("""
                ALTER TABLE facturacion.liquidacion_comision
                    ADD CONSTRAINT ck_liquidacion_comision_valores CHECK (modo IN ('PrecioNeto', 'BrutoConFacturaGastos') AND estado IN ('Borrador', 'Confirmada', 'Anulada')
                        AND ((estado = 'Borrador') = (numero IS NULL)) AND ((estado = 'Anulada') = (motivo_anulacion IS NOT NULL))
                        AND (gasto_id IS NULL OR (modo = 'BrutoConFacturaGastos' AND estado <> 'Borrador'))
                        AND (proveedor_id IS NULL OR modo = 'BrutoConFacturaGastos'));
                CREATE UNIQUE INDEX ux_liquidacion_comision_numero ON facturacion.liquidacion_comision (empresa_id, (extract(year FROM fecha)), numero) WHERE numero IS NOT NULL;
                ALTER TABLE facturacion.linea_liquidacion_comision
                    ADD CONSTRAINT ck_linea_liquidacion_comision_valores CHECK (cantidad_vendida >= 0 AND cantidad_vendida <= cantidad_enviada AND precio_bruto >= 0
                        AND importe_bruto >= 0 AND gastos >= 0 AND precio_albaran >= 0 AND precio_estimado >= 0),
                    ADD CONSTRAINT fk_linea_liquidacion_comision_albaran FOREIGN KEY (albaran_venta_id) REFERENCES facturacion.albaran_venta (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE facturacion.gasto_liquidacion_comision
                    ADD CONSTRAINT ck_gasto_liquidacion_comision_valores CHECK (importe >= 0 AND (porcentaje IS NULL OR porcentaje BETWEEN 0 AND 100)
                        AND tipo IN ('Comision', 'Transporte', 'Aduanas', 'Manipulacion', 'Frio', 'Publicidad', 'Otros'));

                -- Borrador → confirmada → anulada; confirmada no cambia sus datos; solo se borra un borrador.
                CREATE OR REPLACE FUNCTION facturacion.liquidacion_comision_valida() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF TG_OP = 'DELETE' THEN
                        IF OLD.estado <> 'Borrador' AND NOT public.alxor_borrando_empresa(OLD.empresa_id) THEN
                            PERFORM public.alxor_error('liquidacion_comision.no_borrador', 'Solo se borra una liquidación en borrador; la confirmada se anula.');
                        END IF;
                        RETURN OLD;
                    END IF;
                    IF NEW.empresa_id <> OLD.empresa_id OR NEW.cliente_id <> OLD.cliente_id
                       OR (NEW.estado <> OLD.estado AND (OLD.estado || '>' || NEW.estado) NOT IN ('Borrador>Confirmada', 'Confirmada>Anulada'))
                       OR (OLD.estado <> 'Borrador' AND (NEW.fecha <> OLD.fecha OR NEW.modo <> OLD.modo OR NEW.numero IS DISTINCT FROM OLD.numero
                           OR NEW.gasto_id IS DISTINCT FROM OLD.gasto_id)) THEN
                        PERFORM public.alxor_error('liquidacion_comision.transicion', 'Una liquidación confirmada no se cambia: se anula y se hace otra.');
                    END IF;
                    RETURN NEW;
                END $f$;
                CREATE TRIGGER tg_liquidacion_comision_valida BEFORE UPDATE OR DELETE ON facturacion.liquidacion_comision
                    FOR EACH ROW EXECUTE FUNCTION facturacion.liquidacion_comision_valida();

                CREATE OR REPLACE FUNCTION facturacion.detalle_liquidacion_comision_modificable() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    padre uuid := (to_jsonb(CASE WHEN TG_OP = 'DELETE' THEN OLD ELSE NEW END)->>'liquidacion_comision_id')::uuid;
                    est text;
                    emp uuid;
                BEGIN
                    SELECT l.estado, l.empresa_id INTO est, emp FROM facturacion.liquidacion_comision l WHERE l.id = padre;
                    IF FOUND AND est <> 'Borrador' AND NOT public.alxor_borrando_empresa(emp) THEN
                        PERFORM public.alxor_error('liquidacion_comision.no_borrador', 'Las líneas y gastos de una liquidación confirmada no se cambian.');
                    END IF;
                    RETURN CASE WHEN TG_OP = 'DELETE' THEN OLD ELSE NEW END;
                END $f$;
                CREATE TRIGGER tg_linea_liquidacion_comision_modificable BEFORE INSERT OR UPDATE OR DELETE ON facturacion.linea_liquidacion_comision
                    FOR EACH ROW EXECUTE FUNCTION facturacion.detalle_liquidacion_comision_modificable();
                CREATE TRIGGER tg_gasto_liquidacion_comision_modificable BEFORE INSERT OR UPDATE OR DELETE ON facturacion.gasto_liquidacion_comision
                    FOR EACH ROW EXECUTE FUNCTION facturacion.detalle_liquidacion_comision_modificable();

                -- Al confirmar la transacción, para los albaranes tocados: una línea de albarán está en una sola liquidación viva, es
                -- del mismo cliente, y la de una liquidación confirmada tiene el precio que le puso (y su albarán sigue vigente).
                CREATE OR REPLACE FUNCTION facturacion.liquidacion_comision_coherente_albaran(albaran uuid) RETURNS void
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF EXISTS (SELECT 1 FROM facturacion.linea_liquidacion_comision x JOIN facturacion.liquidacion_comision l ON l.id = x.liquidacion_comision_id
                                WHERE x.albaran_venta_id = albaran AND l.estado <> 'Anulada'
                                GROUP BY x.orden_albaran HAVING count(*) > 1) THEN
                        PERFORM public.alxor_error('liquidacion_comision.linea_en_otra', 'Una línea de albarán está en dos liquidaciones de venta en comisión.');
                    END IF;
                    IF EXISTS (SELECT 1 FROM facturacion.linea_liquidacion_comision x
                                JOIN facturacion.liquidacion_comision l ON l.id = x.liquidacion_comision_id
                                JOIN facturacion.albaran_venta a ON a.id = x.albaran_venta_id
                                LEFT JOIN facturacion.linea_albaran_venta y ON y.albaran_venta_id = a.id AND y.orden = x.orden_albaran
                                WHERE x.albaran_venta_id = albaran AND l.estado <> 'Anulada'
                                  AND (a.cliente_id <> l.cliente_id
                                       OR (l.estado = 'Confirmada' AND (a.anulado_en IS NOT NULL OR y.id IS NULL OR NOT y.precio_fijado OR y.precio_unitario <> x.precio_albaran)))) THEN
                        PERFORM public.alxor_error('liquidacion_comision.albaran',
                            'El albarán no cuadra con su liquidación de venta en comisión: otro cliente, anulado o con otro precio. Anula antes la liquidación.');
                    END IF;
                END $f$;

                CREATE OR REPLACE FUNCTION facturacion.liquidacion_comision_coherente() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    fila jsonb := to_jsonb(CASE WHEN TG_OP = 'DELETE' THEN OLD ELSE NEW END);
                    a uuid;
                BEGIN
                    IF TG_TABLE_NAME = 'liquidacion_comision' THEN
                        FOR a IN SELECT DISTINCT albaran_venta_id FROM facturacion.linea_liquidacion_comision WHERE liquidacion_comision_id = (fila->>'id')::uuid LOOP
                            PERFORM facturacion.liquidacion_comision_coherente_albaran(a);
                        END LOOP;
                    ELSIF TG_TABLE_NAME = 'albaran_venta' THEN
                        PERFORM facturacion.liquidacion_comision_coherente_albaran((fila->>'id')::uuid);
                    ELSE
                        PERFORM facturacion.liquidacion_comision_coherente_albaran((fila->>'albaran_venta_id')::uuid);
                    END IF;
                    RETURN NULL;
                END $f$;
                CREATE CONSTRAINT TRIGGER tg_liquidacion_comision_coherente AFTER INSERT OR UPDATE ON facturacion.liquidacion_comision
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION facturacion.liquidacion_comision_coherente();
                CREATE CONSTRAINT TRIGGER tg_linea_liquidacion_comision_coherente AFTER INSERT OR UPDATE ON facturacion.linea_liquidacion_comision
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION facturacion.liquidacion_comision_coherente();
                CREATE CONSTRAINT TRIGGER tg_albaran_venta_liquidacion_comision AFTER UPDATE ON facturacion.albaran_venta
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION facturacion.liquidacion_comision_coherente();
                CREATE CONSTRAINT TRIGGER tg_linea_albaran_venta_liquidacion_comision AFTER UPDATE ON facturacion.linea_albaran_venta
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION facturacion.liquidacion_comision_coherente();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS tg_linea_albaran_venta_liquidacion_comision ON facturacion.linea_albaran_venta;
                DROP TRIGGER IF EXISTS tg_albaran_venta_liquidacion_comision ON facturacion.albaran_venta;
                DROP TRIGGER IF EXISTS tg_linea_liquidacion_comision_coherente ON facturacion.linea_liquidacion_comision;
                DROP TRIGGER IF EXISTS tg_liquidacion_comision_coherente ON facturacion.liquidacion_comision;
                DROP FUNCTION IF EXISTS facturacion.liquidacion_comision_coherente();
                DROP FUNCTION IF EXISTS facturacion.liquidacion_comision_coherente_albaran(uuid);
                DROP TRIGGER IF EXISTS tg_gasto_liquidacion_comision_modificable ON facturacion.gasto_liquidacion_comision;
                DROP TRIGGER IF EXISTS tg_linea_liquidacion_comision_modificable ON facturacion.linea_liquidacion_comision;
                DROP FUNCTION IF EXISTS facturacion.detalle_liquidacion_comision_modificable();
                DROP TRIGGER IF EXISTS tg_liquidacion_comision_valida ON facturacion.liquidacion_comision;
                DROP FUNCTION IF EXISTS facturacion.liquidacion_comision_valida();
                """);
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.QuitarRlsPorPadre("facturacion", "gasto_liquidacion_comision"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.QuitarRlsPorPadre("facturacion", "linea_liquidacion_comision"));

            migrationBuilder.DropTable(
                name: "gasto_liquidacion_comision",
                schema: "facturacion");

            migrationBuilder.DropTable(
                name: "linea_liquidacion_comision",
                schema: "facturacion");

            migrationBuilder.DropTable(
                name: "liquidacion_comision",
                schema: "facturacion");
        }
    }
}
