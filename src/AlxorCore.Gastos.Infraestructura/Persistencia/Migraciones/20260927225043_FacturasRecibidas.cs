using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Gastos.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class FacturasRecibidas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "fecha_factura",
                schema: "gastos",
                table: "gasto",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "numero_factura",
                schema: "gastos",
                table: "gasto",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "recargo_total",
                schema: "gastos",
                table: "gasto",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "revision",
                schema: "gastos",
                table: "gasto",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "linea_gasto",
                schema: "gastos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    cuenta_gasto = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    @base = table.Column<decimal>(name: "base", type: "numeric(14,2)", nullable: false),
                    codigo_iva = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    porcentaje_iva = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    cuota = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    autoliquidada = table.Column<bool>(type: "boolean", nullable: false),
                    porcentaje_recargo = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    cuota_recargo = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    porcentaje_deducible = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    cuota_deducible = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    gasto_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_gasto", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_gasto_gasto_gasto_id",
                        column: x => x.gasto_id,
                        principalSchema: "gastos",
                        principalTable: "gasto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vencimiento_gasto",
                schema: "gastos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    gasto_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vencimiento_gasto", x => x.id);
                    table.ForeignKey(
                        name: "FK_vencimiento_gasto_gasto_gasto_id",
                        column: x => x.gasto_id,
                        principalSchema: "gastos",
                        principalTable: "gasto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_linea_gasto_gasto",
                schema: "gastos",
                table: "linea_gasto",
                column: "gasto_id");

            migrationBuilder.CreateIndex(
                name: "ix_vencimiento_gasto_gasto",
                schema: "gastos",
                table: "vencimiento_gasto",
                column: "gasto_id");

            // Las líneas y los vencimientos son de su factura (RLS por la factura) y cumplen sus reglas.
            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("gastos", "linea_gasto", "gasto_id", "gastos", "gasto"));
            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("gastos", "vencimiento_gasto", "gasto_id", "gastos", "gasto"));
            migrationBuilder.Sql("""
                ALTER TABLE gastos.linea_gasto
                    ADD CONSTRAINT ck_linea_gasto_porcentajes CHECK (porcentaje_iva >= 0 AND porcentaje_recargo >= 0 AND porcentaje_deducible BETWEEN 0 AND 100),
                    ADD CONSTRAINT ck_linea_gasto_recargo CHECK (cuota_recargo = round(base * porcentaje_recargo / 100, 2)),
                    ADD CONSTRAINT ck_linea_gasto_deducible CHECK (cuota_deducible = round(cuota * porcentaje_deducible / 100, 2));
                ALTER TABLE gastos.gasto
                    ADD CONSTRAINT ck_gasto_fecha_factura CHECK (fecha_factura IS NULL OR fecha_factura <= fecha);

                -- Una factura viva de un proveedor no se registra dos veces (mismo número, mismo año).
                CREATE UNIQUE INDEX ux_gasto_factura_proveedor ON gastos.gasto
                    (empresa_id, proveedor_id, upper(numero_factura), date_part('year', coalesce(fecha_factura, fecha)))
                    WHERE numero_factura IS NOT NULL AND proveedor_id IS NOT NULL AND estado = 'Registrado';

                -- Los gastos ya registrados pasan a tener su línea (la de la cabecera) y un vencimiento por el total.
                DO $m$
                DECLARE e record;
                BEGIN
                    IF to_regclass('organizacion.empresa') IS NULL THEN RETURN; END IF;
                    FOR e IN SELECT id FROM organizacion.empresa LOOP
                        PERFORM set_config('app.empresa_actual', e.id::text, true);
                        INSERT INTO gastos.linea_gasto (id, gasto_id, orden, base, codigo_iva, porcentaje_iva, cuota, autoliquidada, porcentaje_recargo,
                                                        cuota_recargo, porcentaje_deducible, cuota_deducible)
                            SELECT gen_random_uuid(), g.id, 1, g.base_imponible, g.codigo_iva, g.porcentaje_iva, g.cuota_iva, false, 0, 0, 100, g.cuota_iva
                              FROM gastos.gasto g WHERE NOT EXISTS (SELECT 1 FROM gastos.linea_gasto l WHERE l.gasto_id = g.id);
                        INSERT INTO gastos.vencimiento_gasto (id, gasto_id, fecha, importe)
                            SELECT gen_random_uuid(), g.id, g.fecha, g.total
                              FROM gastos.gasto g WHERE NOT EXISTS (SELECT 1 FROM gastos.vencimiento_gasto v WHERE v.gasto_id = g.id);
                    END LOOP;
                    PERFORM set_config('app.empresa_actual', '', true);
                END $m$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS gastos.ux_gasto_factura_proveedor;
                ALTER TABLE gastos.gasto DROP CONSTRAINT IF EXISTS ck_gasto_fecha_factura;
                """);

            migrationBuilder.DropTable(
                name: "linea_gasto",
                schema: "gastos");

            migrationBuilder.DropTable(
                name: "vencimiento_gasto",
                schema: "gastos");

            migrationBuilder.DropColumn(
                name: "fecha_factura",
                schema: "gastos",
                table: "gasto");

            migrationBuilder.DropColumn(
                name: "numero_factura",
                schema: "gastos",
                table: "gasto");

            migrationBuilder.DropColumn(
                name: "recargo_total",
                schema: "gastos",
                table: "gasto");

            migrationBuilder.DropColumn(
                name: "revision",
                schema: "gastos",
                table: "gasto");
        }
    }
}
