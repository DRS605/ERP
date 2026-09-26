using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Facturacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class GarantiasFacturacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(GarantiasSql.FuncionesComunes);

            // --- Aislamiento de las líneas de pedidos, albaranes y cartas de porte (heredan el de su cabecera) ---
            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("facturacion", "linea_pedido_venta", "pedido_venta_id", "facturacion", "pedido_venta"));
            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("facturacion", "linea_albaran_venta", "albaran_venta_id", "facturacion", "albaran_venta"));
            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("facturacion", "linea_carta_porte", "carta_porte_id", "facturacion", "carta_porte"));

            // --- Coherencia de los importes y del estado de cada factura ---
            migrationBuilder.Sql("""
                ALTER TABLE facturacion.factura
                    ADD CONSTRAINT ck_factura_numero CHECK (numero >= 1),
                    ADD CONSTRAINT ck_factura_ejercicio CHECK (ejercicio = extract(year FROM fecha_emision)),
                    ADD CONSTRAINT ck_factura_numero_completo CHECK (numero_completo = prefijo || ejercicio::text || '/' ||
                        CASE WHEN numero < 1000000 THEN lpad(numero::text, 6, '0') ELSE numero::text END),
                    ADD CONSTRAINT ck_factura_estado CHECK (estado IN ('Emitida', 'Anulada', 'Rectificada')),
                    ADD CONSTRAINT ck_factura_tipo CHECK (tipo_factura IN ('Ordinaria', 'Rectificativa', 'Simplificada')),
                    ADD CONSTRAINT ck_factura_rectificativa CHECK ((tipo_factura = 'Rectificativa') = (rectifica_factura_id IS NOT NULL)),
                    ADD CONSTRAINT ck_factura_irpf CHECK (porcentaje_irpf BETWEEN 0 AND 60),
                    ADD CONSTRAINT ck_factura_retencion CHECK (retencion_irpf = round(base_imponible * porcentaje_irpf / 100, 2)),
                    ADD CONSTRAINT ck_factura_total CHECK (total = base_imponible + cuota_iva + recargo_total - retencion_irpf),
                    ADD CONSTRAINT ck_factura_anulacion CHECK ((estado = 'Anulada') =
                        (fecha_hora_anulacion IS NOT NULL AND huella_anulacion IS NOT NULL AND motivo_anulacion IS NOT NULL)),
                    ADD CONSTRAINT ck_factura_vencimiento CHECK (fecha_vencimiento >= fecha_emision);

                ALTER TABLE facturacion.linea_factura
                    ADD CONSTRAINT ck_linea_factura_cantidad CHECK (cantidad > 0),
                    ADD CONSTRAINT ck_linea_factura_precio CHECK (precio_unitario >= 0 AND coste_unitario >= 0),
                    ADD CONSTRAINT ck_linea_factura_porcentajes CHECK (descuento BETWEEN 0 AND 100 AND porcentaje_iva >= 0 AND porcentaje_recargo >= 0),
                    ADD CONSTRAINT ck_linea_factura_base CHECK (base = round(cantidad * precio_unitario * (1 - descuento / 100), 2)),
                    ADD CONSTRAINT ck_linea_factura_cuotas CHECK (cuota_iva = round(base * porcentaje_iva / 100, 2)
                        AND cuota_recargo = round(base * porcentaje_recargo / 100, 2));
                """);

            // --- Una factura emitida es inalterable; sus líneas, también ---
            migrationBuilder.Sql(GarantiasSql.MarcarAlta("facturacion", "factura"));
            migrationBuilder.Sql(GarantiasSql.HijaDeAlta("facturacion", "linea_factura", "factura_id", "facturacion", "factura",
                "factura.inalterable", "Una factura emitida no admite líneas nuevas: emite una rectificativa."));
            migrationBuilder.Sql(GarantiasSql.SoloInsercion("facturacion", "linea_factura",
                "factura.inalterable", "Las líneas de una factura emitida no se pueden modificar ni borrar: emite una rectificativa."));
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION facturacion.factura_inalterable() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    cambiables CONSTANT text[] := ARRAY['estado', 'motivo_anulacion', 'fecha_hora_anulacion', 'huella_anulacion', 'estado_envio_aeat'];
                    anulacion_cambia boolean;
                BEGIN
                    IF TG_OP = 'DELETE' THEN
                        IF public.alxor_borrando_empresa(OLD.empresa_id) THEN
                            RETURN OLD;
                        END IF;
                        PERFORM public.alxor_error('factura.inalterable', 'Una factura emitida no se puede borrar: anúlala o emite una rectificativa.');
                    END IF;

                    IF (to_jsonb(NEW) - cambiables) IS DISTINCT FROM (to_jsonb(OLD) - cambiables) THEN
                        PERFORM public.alxor_error('factura.inalterable', 'Una factura emitida no se puede modificar: emite una rectificativa.');
                    END IF;

                    IF NEW.estado IS DISTINCT FROM OLD.estado
                       AND NOT (OLD.estado = 'Emitida' AND NEW.estado IN ('Anulada', 'Rectificada')) THEN
                        PERFORM public.alxor_error('factura.estado', format('Una factura %s no puede pasar a %s.', lower(OLD.estado), lower(NEW.estado)));
                    END IF;

                    anulacion_cambia := (NEW.motivo_anulacion, NEW.fecha_hora_anulacion, NEW.huella_anulacion)
                        IS DISTINCT FROM (OLD.motivo_anulacion, OLD.fecha_hora_anulacion, OLD.huella_anulacion);
                    IF anulacion_cambia AND NOT (OLD.estado = 'Emitida' AND NEW.estado = 'Anulada') THEN
                        PERFORM public.alxor_error('factura.inalterable', 'El registro de anulación de una factura no se puede cambiar.');
                    END IF;
                    RETURN NEW;
                END $f$;

                CREATE TRIGGER tg_factura_inalterable BEFORE UPDATE OR DELETE ON facturacion.factura
                    FOR EACH ROW EXECUTE FUNCTION facturacion.factura_inalterable();
                """);

            // --- Numeración correlativa sin huecos y con fechas en orden, y cabecera = suma de líneas ---
            // Se comprueba al confirmar (diferido): EF puede insertar en cualquier orden dentro de la
            // transacción. Como las facturas no se borran, «existe la n-1» basta para que no haya huecos.
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION facturacion.factura_comprobar_alta() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    anterior date;
                    lineas record;
                BEGIN
                    IF NEW.numero > 1 THEN
                        SELECT fecha_emision INTO anterior FROM facturacion.factura
                         WHERE empresa_id = NEW.empresa_id AND prefijo = NEW.prefijo AND ejercicio = NEW.ejercicio AND numero = NEW.numero - 1;
                        IF NOT FOUND THEN
                            PERFORM public.alxor_error('factura.numeracion', format('La factura %s deja un hueco en la numeración: falta la número %s.', NEW.numero_completo, NEW.numero - 1));
                        END IF;
                        IF NEW.fecha_emision < anterior THEN
                            PERFORM public.alxor_error('factura.fecha_no_correlativa', format('La factura %s tiene fecha anterior (%s) a la de la factura anterior de su serie (%s).', NEW.numero_completo, NEW.fecha_emision, anterior));
                        END IF;
                    END IF;

                    SELECT count(*) AS n, coalesce(sum(base), 0) AS base, coalesce(sum(cuota_iva), 0) AS cuota, coalesce(sum(cuota_recargo), 0) AS recargo
                      INTO lineas FROM facturacion.linea_factura WHERE factura_id = NEW.id;
                    IF lineas.n = 0 THEN
                        PERFORM public.alxor_error('factura.sin_lineas', format('La factura %s no tiene líneas.', NEW.numero_completo));
                    END IF;
                    IF (lineas.base, lineas.cuota, lineas.recargo) IS DISTINCT FROM (NEW.base_imponible, NEW.cuota_iva, NEW.recargo_total) THEN
                        PERFORM public.alxor_error('factura.descuadrada', format('Los importes de la factura %s no coinciden con la suma de sus líneas.', NEW.numero_completo));
                    END IF;
                    RETURN NULL;
                END $f$;

                CREATE CONSTRAINT TRIGGER tg_factura_comprobar_alta AFTER INSERT ON facturacion.factura
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION facturacion.factura_comprobar_alta();
                """);

            // --- La serie de numeración (que muestra el «próximo número») sigue a las facturas guardadas ---
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION facturacion.factura_actualizar_serie() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    INSERT INTO organizacion.serie_numeracion (id, empresa_id, tipo_documento, ejercicio, prefijo, siguiente_numero, creado_en)
                    VALUES (gen_random_uuid(), NEW.empresa_id, 'Factura', NEW.ejercicio, NEW.prefijo, NEW.numero + 1, now())
                    ON CONFLICT (empresa_id, tipo_documento, ejercicio, prefijo)
                    DO UPDATE SET siguiente_numero = GREATEST(organizacion.serie_numeracion.siguiente_numero, EXCLUDED.siguiente_numero);
                    RETURN NULL;
                END $f$;

                CREATE TRIGGER tg_factura_actualizar_serie AFTER INSERT ON facturacion.factura
                    FOR EACH ROW EXECUTE FUNCTION facturacion.factura_actualizar_serie();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS tg_factura_actualizar_serie ON facturacion.factura;
                DROP FUNCTION IF EXISTS facturacion.factura_actualizar_serie();
                DROP TRIGGER IF EXISTS tg_factura_comprobar_alta ON facturacion.factura;
                DROP FUNCTION IF EXISTS facturacion.factura_comprobar_alta();
                DROP TRIGGER IF EXISTS tg_factura_inalterable ON facturacion.factura;
                DROP FUNCTION IF EXISTS facturacion.factura_inalterable();
                """);
            migrationBuilder.Sql(GarantiasSql.QuitarTrigger("facturacion", "linea_factura", "tg_linea_factura_solo_insercion"));
            migrationBuilder.Sql(GarantiasSql.QuitarTrigger("facturacion", "linea_factura", "tg_linea_factura_hija"));
            migrationBuilder.Sql(GarantiasSql.QuitarTrigger("facturacion", "factura", "tg_factura_alta"));
            migrationBuilder.Sql("""
                ALTER TABLE facturacion.factura DROP COLUMN IF EXISTS tx_alta,
                    DROP CONSTRAINT IF EXISTS ck_factura_numero, DROP CONSTRAINT IF EXISTS ck_factura_ejercicio,
                    DROP CONSTRAINT IF EXISTS ck_factura_numero_completo, DROP CONSTRAINT IF EXISTS ck_factura_estado,
                    DROP CONSTRAINT IF EXISTS ck_factura_tipo, DROP CONSTRAINT IF EXISTS ck_factura_rectificativa,
                    DROP CONSTRAINT IF EXISTS ck_factura_irpf, DROP CONSTRAINT IF EXISTS ck_factura_retencion,
                    DROP CONSTRAINT IF EXISTS ck_factura_total, DROP CONSTRAINT IF EXISTS ck_factura_anulacion,
                    DROP CONSTRAINT IF EXISTS ck_factura_vencimiento;
                ALTER TABLE facturacion.linea_factura
                    DROP CONSTRAINT IF EXISTS ck_linea_factura_cantidad, DROP CONSTRAINT IF EXISTS ck_linea_factura_precio,
                    DROP CONSTRAINT IF EXISTS ck_linea_factura_porcentajes, DROP CONSTRAINT IF EXISTS ck_linea_factura_base,
                    DROP CONSTRAINT IF EXISTS ck_linea_factura_cuotas;
                """);
            migrationBuilder.Sql(GarantiasSql.QuitarRlsPorPadre("facturacion", "linea_carta_porte"));
            migrationBuilder.Sql(GarantiasSql.QuitarRlsPorPadre("facturacion", "linea_albaran_venta"));
            migrationBuilder.Sql(GarantiasSql.QuitarRlsPorPadre("facturacion", "linea_pedido_venta"));
        }
    }
}
