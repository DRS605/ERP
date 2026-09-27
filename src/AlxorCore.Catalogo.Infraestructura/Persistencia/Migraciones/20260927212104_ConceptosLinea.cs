using AlxorCore.Persistencia;
﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Catalogo.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ConceptosLinea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "concepto_linea",
                schema: "catalogo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    texto_documento = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ambito = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    efecto = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    sentido = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    calculo = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(14,4)", nullable: false),
                    reparto = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    actualizado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    grupo_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_concepto_linea", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "asignacion_concepto",
                schema: "catalogo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tercero_id = table.Column<Guid>(type: "uuid", nullable: true),
                    familia_id = table.Column<Guid>(type: "uuid", nullable: true),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    valor = table.Column<decimal>(type: "numeric(14,4)", nullable: true),
                    concepto_linea_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asignacion_concepto", x => x.id);
                    table.ForeignKey(
                        name: "FK_asignacion_concepto_concepto_linea_concepto_linea_id",
                        column: x => x.concepto_linea_id,
                        principalSchema: "catalogo",
                        principalTable: "concepto_linea",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_asignacion_concepto_concepto",
                schema: "catalogo",
                table: "asignacion_concepto",
                column: "concepto_linea_id");

            migrationBuilder.CreateIndex(
                name: "ux_concepto_linea_grupo_codigo",
                schema: "catalogo",
                table: "concepto_linea",
                columns: new[] { "grupo_id", "codigo" },
                unique: true);

            // Aislamiento por grupo (RLS) y reglas del concepto también en la base de datos.
            migrationBuilder.Sql(RlsSql.ActivarPorGrupo("catalogo", "concepto_linea"));
            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("catalogo", "asignacion_concepto", "concepto_linea_id", "catalogo", "concepto_linea"));
            migrationBuilder.Sql("""
                ALTER TABLE catalogo.concepto_linea
                    ADD CONSTRAINT ck_concepto_linea_ambito CHECK (ambito IN ('Ventas', 'Compras', 'Ambos')),
                    ADD CONSTRAINT ck_concepto_linea_efecto CHECK (efecto IN ('Precio', 'Coste')),
                    ADD CONSTRAINT ck_concepto_linea_sentido CHECK (sentido IN ('Suma', 'Resta')),
                    ADD CONSTRAINT ck_concepto_linea_calculo CHECK (calculo IN ('Porcentaje', 'PorUnidad', 'PorKilo', 'Importe')),
                    ADD CONSTRAINT ck_concepto_linea_reparto CHECK (reparto IN ('PorImporte', 'PorCantidad', 'PorPeso')),
                    ADD CONSTRAINT ck_concepto_linea_valor CHECK (valor >= 0 AND (calculo <> 'Porcentaje' OR valor <= 100));
                ALTER TABLE catalogo.asignacion_concepto
                    ADD CONSTRAINT ck_asignacion_concepto_ambito CHECK (producto_id IS NULL OR familia_id IS NULL),
                    ADD CONSTRAINT ck_asignacion_concepto_valor CHECK (valor IS NULL OR valor >= 0);

                -- ¿Algún documento de alguna empresa del grupo lleva el concepto? Los documentos guardan una copia de
                -- sus conceptos en la columna jsonb «conceptos» de cada línea.
                CREATE OR REPLACE FUNCTION catalogo.concepto_linea_en_uso(p_id uuid) RETURNS boolean
                LANGUAGE plpgsql VOLATILE SET search_path = pg_catalog, public AS $f$
                DECLARE
                    original text := coalesce(current_setting('app.empresa_actual', true), '');
                    grupo uuid := NULLIF(current_setting('app.grupo_actual', true), '')::uuid;
                    tablas CONSTANT text[] := ARRAY['facturacion.linea_factura', 'facturacion.linea_presupuesto', 'facturacion.linea_pedido_venta', 'compras.linea_pedido'];
                    emp uuid;
                    t text;
                    hay boolean := false;
                BEGIN
                    IF grupo IS NULL THEN
                        RAISE EXCEPTION 'concepto_linea_en_uso necesita un grupo seleccionado (app.grupo_actual).';
                    END IF;
                    FOR emp IN SELECT e.id FROM organizacion.empresa e WHERE e.grupo_id = grupo LOOP
                        PERFORM set_config('app.empresa_actual', emp::text, true);
                        FOREACH t IN ARRAY tablas LOOP
                            IF EXISTS (SELECT 1 FROM information_schema.columns
                                        WHERE table_schema = split_part(t, '.', 1) AND table_name = split_part(t, '.', 2) AND column_name = 'conceptos') THEN
                                EXECUTE format('SELECT EXISTS (SELECT 1 FROM %I.%I WHERE conceptos @> $1)', split_part(t, '.', 1), split_part(t, '.', 2))
                                    INTO hay USING jsonb_build_array(jsonb_build_object('conceptoId', p_id));
                                IF hay THEN
                                    PERFORM set_config('app.empresa_actual', original, true);
                                    RETURN true;
                                END IF;
                            END IF;
                        END LOOP;
                    END LOOP;
                    PERFORM set_config('app.empresa_actual', original, true);
                    RETURN false;
                END $f$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS catalogo.concepto_linea_en_uso(uuid);");

            migrationBuilder.DropTable(
                name: "asignacion_concepto",
                schema: "catalogo");

            migrationBuilder.DropTable(
                name: "concepto_linea",
                schema: "catalogo");
        }
    }
}
