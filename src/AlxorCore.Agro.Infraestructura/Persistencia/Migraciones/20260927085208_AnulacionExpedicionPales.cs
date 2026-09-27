using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Agro.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AnulacionExpedicionPales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Un palé expedido puede volver a cerrado (anulación de la expedición), sin cliente ni fecha de salida.
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
                    IF NEW.sscc <> OLD.sscc
                       OR (OLD.estado = 'Expedido' AND NOT (NEW.estado = 'Cerrado' AND NEW.cliente_id IS NULL AND NEW.fecha_expedicion IS NULL AND NEW.referencia_expedicion IS NULL))
                       OR (NEW.estado <> OLD.estado AND (OLD.estado || '>' || NEW.estado) NOT IN ('Abierto>Cerrado', 'Cerrado>Abierto', 'Cerrado>Expedido', 'Expedido>Cerrado'))
                       OR (NEW.estado <> 'Expedido' AND (NEW.cliente_id IS NOT NULL OR NEW.referencia_expedicion IS NOT NULL)) THEN
                        PERFORM public.alxor_error('pale.transicion', 'Cambio de palé no permitido: el SSCC no cambia y un palé expedido solo vuelve a cerrado al anular su expedición.');
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
                    IF NEW.sscc <> OLD.sscc OR OLD.estado = 'Expedido'
                       OR (NEW.estado <> OLD.estado AND (OLD.estado || '>' || NEW.estado) NOT IN ('Abierto>Cerrado', 'Cerrado>Abierto', 'Cerrado>Expedido'))
                       OR (NEW.estado <> 'Expedido' AND (NEW.cliente_id IS NOT NULL OR NEW.referencia_expedicion IS NOT NULL)) THEN
                        PERFORM public.alxor_error('pale.transicion', 'Cambio de palé no permitido: el SSCC no cambia y un palé expedido ya no se toca.');
                    END IF;
                    RETURN NEW;
                END $f$;
                """);
        }
    }
}
