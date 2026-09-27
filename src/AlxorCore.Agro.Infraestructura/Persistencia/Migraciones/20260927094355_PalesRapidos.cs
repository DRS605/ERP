using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Agro.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class PalesRapidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "carta_porte_id",
                schema: "agro",
                table: "pale",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "plantilla_id",
                schema: "agro",
                table: "pale",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "cajas",
                schema: "agro",
                table: "movimiento_partida",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "plantilla_pale",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    tipo_pale = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    marca = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    cajas_por_pale = table.Column<int>(type: "integer", nullable: false),
                    kilos_por_caja = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    filas = table.Column<int>(type: "integer", nullable: true),
                    columnas = table.Column<int>(type: "integer", nullable: true),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plantilla_pale", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_pale_carta_porte",
                schema: "agro",
                table: "pale",
                column: "carta_porte_id");

            migrationBuilder.CreateIndex(
                name: "ix_pale_plantilla",
                schema: "agro",
                table: "pale",
                column: "plantilla_id");

            migrationBuilder.CreateIndex(
                name: "ux_plantilla_pale_codigo",
                schema: "agro",
                table: "plantilla_pale",
                columns: new[] { "empresa_id", "codigo" },
                unique: true);

            migrationBuilder.Sql(RlsSql.Activar("agro", "plantilla_pale"));
            migrationBuilder.Sql("""
                ALTER TABLE agro.plantilla_pale
                    ADD CONSTRAINT ck_plantilla_pale_cajas CHECK (cajas_por_pale BETWEEN 1 AND 10000 AND kilos_por_caja > 0),
                    ADD CONSTRAINT ck_plantilla_pale_mosaico CHECK ((filas IS NULL) = (columnas IS NULL)
                        AND (filas IS NULL OR (filas > 0 AND columnas > 0 AND cajas_por_pale % (filas * columnas) = 0)));
                ALTER TABLE agro.pale ADD CONSTRAINT fk_pale_plantilla FOREIGN KEY (plantilla_id) REFERENCES agro.plantilla_pale (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE agro.movimiento_partida ADD CONSTRAINT ck_movimiento_partida_cajas CHECK (cajas = 0 OR sign(cajas) = sign(kilos));

                -- El palé: el SSCC y la plantilla no cambian; un expedido solo vuelve a cerrado (anulación de su expedición,
                -- que suelta cliente, fecha, referencia y carta de porte), y la carta de porte solo la lleva un palé expedido.
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE agro.pale DROP CONSTRAINT IF EXISTS fk_pale_plantilla;
                ALTER TABLE agro.movimiento_partida DROP CONSTRAINT IF EXISTS ck_movimiento_partida_cajas;
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

            migrationBuilder.DropTable(
                name: "plantilla_pale",
                schema: "agro");

            migrationBuilder.DropIndex(
                name: "ix_pale_carta_porte",
                schema: "agro",
                table: "pale");

            migrationBuilder.DropIndex(
                name: "ix_pale_plantilla",
                schema: "agro",
                table: "pale");

            migrationBuilder.DropColumn(
                name: "carta_porte_id",
                schema: "agro",
                table: "pale");

            migrationBuilder.DropColumn(
                name: "plantilla_id",
                schema: "agro",
                table: "pale");

            migrationBuilder.DropColumn(
                name: "cajas",
                schema: "agro",
                table: "movimiento_partida");
        }
    }
}
