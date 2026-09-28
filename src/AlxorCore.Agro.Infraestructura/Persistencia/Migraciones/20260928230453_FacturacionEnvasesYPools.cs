using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Agro.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class FacturacionEnvasesYPools : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "gestion",
                schema: "agro",
                table: "cuenta_envases",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Retornar");

            migrationBuilder.CreateTable(
                name: "envase_pool",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    envase_producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cuenta_envases_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_envase_pool", x => x.id);
                    table.ForeignKey(
                        name: "FK_envase_pool_cuenta_envases_cuenta_envases_id",
                        column: x => x.cuenta_envases_id,
                        principalSchema: "agro",
                        principalTable: "cuenta_envases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_envase_pool",
                schema: "agro",
                table: "envase_pool",
                columns: new[] { "cuenta_envases_id", "envase_producto_id" },
                unique: true);
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("agro", "envase_pool", "cuenta_envases_id", "agro", "cuenta_envases"));
            migrationBuilder.Sql("ALTER TABLE agro.cuenta_envases ADD CONSTRAINT ck_cuenta_envases_gestion CHECK (gestion IN ('Retornar','Facturar','FacturarExceso') AND (gestion = 'Retornar' OR tipo = 'Cliente'));");

            // El libro del agricultor pasa al libro por tercero: un movimiento de regularización por agricultor con su saldo
            // de cada envase, en la cuenta de su proveedor (que se abre si no la tiene).
            migrationBuilder.Sql("""
                DO $$
                DECLARE e record; a record; cuenta uuid; mov uuid; num int; anio int := extract(year FROM now())::int;
                BEGIN
                    IF to_regclass('organizacion.empresa') IS NULL THEN
                        RETURN;
                    END IF;
                    FOR e IN SELECT id FROM organizacion.empresa LOOP
                        PERFORM set_config('app.empresa_actual', e.id::text, true);
                        FOR a IN SELECT ag.id, ag.proveedor_id, ag.nombre FROM agro.agricultor ag
                                 WHERE ag.empresa_id = e.id AND EXISTS (
                                     SELECT 1 FROM agro.movimiento_envase m WHERE m.agricultor_id = ag.id
                                     GROUP BY m.envase_producto_id HAVING sum(m.cantidad) <> 0) LOOP
                            SELECT c.id INTO cuenta FROM agro.cuenta_envases c WHERE c.empresa_id = e.id AND c.tipo = 'Proveedor' AND c.tercero_id = a.proveedor_id;
                            IF cuenta IS NULL THEN
                                cuenta := gen_random_uuid();
                                INSERT INTO agro.cuenta_envases (id, empresa_id, tipo, tercero_id, nombre, imputar_a_transportista, bloqueo, activa, creada_en, control_limite, gestion)
                                VALUES (cuenta, e.id, 'Proveedor', a.proveedor_id, left(a.nombre, 200), false, 'Ninguno', true, now(), 'Aviso', 'Retornar');
                            END IF;
                            SELECT coalesce(max(numero), 0) + 1 INTO num FROM agro.movimiento_envases WHERE empresa_id = e.id AND ejercicio = anio;
                            mov := gen_random_uuid();
                            INSERT INTO agro.movimiento_envases (id, empresa_id, ejercicio, numero, fecha, cuenta_id, cuenta_solicitada_id, origen, documento_id, observaciones, creado_en)
                            VALUES (mov, e.id, anio, num, current_date, cuenta, cuenta,
                                    'Regularizacion', a.id, 'Saldo del libro del agricultor', now());
                            INSERT INTO agro.linea_movimiento_envases (id, movimiento_envases_id, envase_producto_id, cantidad)
                            SELECT gen_random_uuid(), mov, m.envase_producto_id, sum(m.cantidad) FROM agro.movimiento_envase m
                            WHERE m.agricultor_id = a.id GROUP BY m.envase_producto_id HAVING sum(m.cantidad) <> 0;
                        END LOOP;
                    END LOOP;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE agro.cuenta_envases DROP CONSTRAINT IF EXISTS ck_cuenta_envases_gestion;");
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.QuitarRlsPorPadre("agro", "envase_pool"));
            migrationBuilder.DropTable(
                name: "envase_pool",
                schema: "agro");

            migrationBuilder.DropColumn(
                name: "gestion",
                schema: "agro",
                table: "cuenta_envases");
        }
    }
}
