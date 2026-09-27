using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Agro.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class PaleAlbaranVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "albaran_id",
                schema: "agro",
                table: "pale",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_pale_albaran",
                schema: "agro",
                table: "pale",
                column: "albaran_id");

            // El albarán, como la carta de porte, solo lo lleva un palé expedido y no cambia una vez puesto.
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                           AND NEW.referencia_expedicion IS NULL AND NEW.carta_porte_id IS NULL))
                       OR (OLD.estado = 'Expedido' AND NEW.estado = 'Expedido' AND (NEW.cliente_id IS DISTINCT FROM OLD.cliente_id
                           OR NEW.fecha_expedicion IS DISTINCT FROM OLD.fecha_expedicion
                           OR (OLD.referencia_expedicion IS NOT NULL AND NEW.referencia_expedicion IS DISTINCT FROM OLD.referencia_expedicion)
                           OR (OLD.carta_porte_id IS NOT NULL AND NEW.carta_porte_id IS DISTINCT FROM OLD.carta_porte_id)))
                       OR (NEW.estado <> OLD.estado AND (OLD.estado || '>' || NEW.estado) NOT IN ('Abierto>Cerrado', 'Cerrado>Abierto', 'Cerrado>Expedido', 'Expedido>Cerrado'))
                       OR (NEW.estado <> 'Expedido' AND (NEW.cliente_id IS NOT NULL OR NEW.referencia_expedicion IS NOT NULL OR NEW.carta_porte_id IS NOT NULL)) THEN
                        PERFORM public.alxor_error('pale.transicion', 'Cambio de palé no permitido: el SSCC y la plantilla no cambian y un palé expedido solo vuelve a cerrado al anular su expedición.');
                    END IF;
                    RETURN NEW;
                END $f$;
                """);

            migrationBuilder.DropIndex(
                name: "ix_pale_albaran",
                schema: "agro",
                table: "pale");

            migrationBuilder.DropColumn(
                name: "albaran_id",
                schema: "agro",
                table: "pale");
        }
    }
}
