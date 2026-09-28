using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Agro.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class PreciosLiquidacionTipoEnvase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "envase_producto_id",
                schema: "agro",
                table: "precio_liquidacion",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tipo",
                schema: "agro",
                table: "precio_liquidacion",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Periodo");

            // Los precios de distinto tipo (día, periodo, general) o envase conviven: gana el más concreto al valorar.
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION agro.precio_liquidacion_valido() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF TG_OP IN ('UPDATE', 'DELETE') AND EXISTS (
                        SELECT 1 FROM agro.linea_liquidacion l JOIN agro.liquidacion q ON q.id = l.liquidacion_id
                         WHERE l.precio_id = OLD.id AND q.estado = 'Emitida') THEN
                        PERFORM public.alxor_error('precio.aplicado', 'El precio ya se aplicó en una liquidación emitida: no se cambia ni se borra.');
                    END IF;
                    IF TG_OP = 'DELETE' THEN
                        RETURN OLD;
                    END IF;
                    IF EXISTS (SELECT 1 FROM agro.precio_liquidacion p
                                WHERE p.id <> NEW.id AND p.campana_id = NEW.campana_id AND p.producto_id = NEW.producto_id
                                  AND p.categoria_id IS NOT DISTINCT FROM NEW.categoria_id AND p.desde <= NEW.hasta AND NEW.desde <= p.hasta
                                  AND p.tipo = NEW.tipo AND p.envase_producto_id IS NOT DISTINCT FROM NEW.envase_producto_id) THEN
                        PERFORM public.alxor_error('precio.solapado', 'Ya hay un precio de ese tipo para el artículo, la categoría y el envase en esas fechas.');
                    END IF;
                    RETURN NEW;
                END $f$;
                ALTER TABLE agro.precio_liquidacion
                    ADD CONSTRAINT ck_precio_liquidacion_tipo CHECK (tipo IN ('General', 'Periodo', 'Dia') AND (tipo <> 'Dia' OR desde = hasta));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION agro.precio_liquidacion_valido() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF TG_OP IN ('UPDATE', 'DELETE') AND EXISTS (
                        SELECT 1 FROM agro.linea_liquidacion l JOIN agro.liquidacion q ON q.id = l.liquidacion_id
                         WHERE l.precio_id = OLD.id AND q.estado = 'Emitida') THEN
                        PERFORM public.alxor_error('precio.aplicado', 'El precio ya se aplicó en una liquidación emitida: no se cambia ni se borra.');
                    END IF;
                    IF TG_OP = 'DELETE' THEN
                        RETURN OLD;
                    END IF;
                    IF EXISTS (SELECT 1 FROM agro.precio_liquidacion p
                                WHERE p.id <> NEW.id AND p.campana_id = NEW.campana_id AND p.producto_id = NEW.producto_id
                                  AND p.categoria_id IS NOT DISTINCT FROM NEW.categoria_id AND p.desde <= NEW.hasta AND NEW.desde <= p.hasta) THEN
                        PERFORM public.alxor_error('precio.solapado', 'Ya hay un precio de ese artículo y categoría en esas fechas.');
                    END IF;
                    RETURN NEW;
                END $f$;
                ALTER TABLE agro.precio_liquidacion DROP CONSTRAINT IF EXISTS ck_precio_liquidacion_tipo;
                """);

            migrationBuilder.DropColumn(
                name: "envase_producto_id",
                schema: "agro",
                table: "precio_liquidacion");

            migrationBuilder.DropColumn(
                name: "tipo",
                schema: "agro",
                table: "precio_liquidacion");
        }
    }
}
