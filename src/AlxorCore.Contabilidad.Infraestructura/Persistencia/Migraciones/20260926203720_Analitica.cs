using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Contabilidad.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class Analitica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "centro_analitico",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    padre_id = table.Column<Guid>(type: "uuid", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    grupo_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_centro_analitico", x => x.id);
                    table.ForeignKey(
                        name: "FK_centro_analitico_centro_analitico_padre_id",
                        column: x => x.padre_id,
                        principalSchema: "contabilidad",
                        principalTable: "centro_analitico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "clave_reparto",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    grupo_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clave_reparto", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "partida_analitica",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    naturaleza = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    padre_id = table.Column<Guid>(type: "uuid", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    grupo_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_partida_analitica", x => x.id);
                    table.ForeignKey(
                        name: "FK_partida_analitica_partida_analitica_padre_id",
                        column: x => x.padre_id,
                        principalSchema: "contabilidad",
                        principalTable: "partida_analitica",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "periodo_analitico",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    desde = table.Column<DateOnly>(type: "date", nullable: false),
                    hasta = table.Column<DateOnly>(type: "date", nullable: false),
                    cerrado = table.Column<bool>(type: "boolean", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_periodo_analitico", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ejecucion_analitica",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    desde = table.Column<DateOnly>(type: "date", nullable: false),
                    hasta = table.Column<DateOnly>(type: "date", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    centro_origen_id = table.Column<Guid>(type: "uuid", nullable: true),
                    clave_reparto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ejecucion_analitica", x => x.id);
                    table.ForeignKey(
                        name: "FK_ejecucion_analitica_centro_analitico_centro_origen_id",
                        column: x => x.centro_origen_id,
                        principalSchema: "contabilidad",
                        principalTable: "centro_analitico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ejecucion_analitica_clave_reparto_clave_reparto_id",
                        column: x => x.clave_reparto_id,
                        principalSchema: "contabilidad",
                        principalTable: "clave_reparto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "linea_clave_reparto",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    centro_id = table.Column<Guid>(type: "uuid", nullable: false),
                    porcentaje = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    clave_reparto_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_clave_reparto", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_clave_reparto_centro_analitico_centro_id",
                        column: x => x.centro_id,
                        principalSchema: "contabilidad",
                        principalTable: "centro_analitico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_linea_clave_reparto_clave_reparto_clave_reparto_id",
                        column: x => x.clave_reparto_id,
                        principalSchema: "contabilidad",
                        principalTable: "clave_reparto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "regla_analitica",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    prefijo_cuenta = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    tercero_id = table.Column<Guid>(type: "uuid", nullable: true),
                    actividad_negocio_id = table.Column<Guid>(type: "uuid", nullable: true),
                    familia = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    vigente_desde = table.Column<DateOnly>(type: "date", nullable: true),
                    vigente_hasta = table.Column<DateOnly>(type: "date", nullable: true),
                    centro_id = table.Column<Guid>(type: "uuid", nullable: true),
                    clave_reparto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    partida_id = table.Column<Guid>(type: "uuid", nullable: true),
                    prioridad = table.Column<int>(type: "integer", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regla_analitica", x => x.id);
                    table.ForeignKey(
                        name: "FK_regla_analitica_centro_analitico_centro_id",
                        column: x => x.centro_id,
                        principalSchema: "contabilidad",
                        principalTable: "centro_analitico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_regla_analitica_clave_reparto_clave_reparto_id",
                        column: x => x.clave_reparto_id,
                        principalSchema: "contabilidad",
                        principalTable: "clave_reparto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_regla_analitica_partida_analitica_partida_id",
                        column: x => x.partida_id,
                        principalSchema: "contabilidad",
                        principalTable: "partida_analitica",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "imputacion_analitica",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    apunte_id = table.Column<Guid>(type: "uuid", nullable: true),
                    asiento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    cuenta_codigo = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    naturaleza = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    centro_id = table.Column<Guid>(type: "uuid", nullable: false),
                    partida_id = table.Column<Guid>(type: "uuid", nullable: true),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    origen = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ejecucion_id = table.Column<Guid>(type: "uuid", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_imputacion_analitica", x => x.id);
                    table.ForeignKey(
                        name: "FK_imputacion_analitica_centro_analitico_centro_id",
                        column: x => x.centro_id,
                        principalSchema: "contabilidad",
                        principalTable: "centro_analitico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_imputacion_analitica_ejecucion_analitica_ejecucion_id",
                        column: x => x.ejecucion_id,
                        principalSchema: "contabilidad",
                        principalTable: "ejecucion_analitica",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_imputacion_analitica_partida_analitica_partida_id",
                        column: x => x.partida_id,
                        principalSchema: "contabilidad",
                        principalTable: "partida_analitica",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_centro_analitico_padre_id",
                schema: "contabilidad",
                table: "centro_analitico",
                column: "padre_id");

            migrationBuilder.CreateIndex(
                name: "ux_centro_analitico_grupo_codigo",
                schema: "contabilidad",
                table: "centro_analitico",
                columns: new[] { "grupo_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_clave_reparto_grupo_codigo",
                schema: "contabilidad",
                table: "clave_reparto",
                columns: new[] { "grupo_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ejecucion_analitica_centro_origen_id",
                schema: "contabilidad",
                table: "ejecucion_analitica",
                column: "centro_origen_id");

            migrationBuilder.CreateIndex(
                name: "IX_ejecucion_analitica_clave_reparto_id",
                schema: "contabilidad",
                table: "ejecucion_analitica",
                column: "clave_reparto_id");

            migrationBuilder.CreateIndex(
                name: "ix_ejecucion_analitica_empresa",
                schema: "contabilidad",
                table: "ejecucion_analitica",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "IX_imputacion_analitica_centro_id",
                schema: "contabilidad",
                table: "imputacion_analitica",
                column: "centro_id");

            migrationBuilder.CreateIndex(
                name: "IX_imputacion_analitica_ejecucion_id",
                schema: "contabilidad",
                table: "imputacion_analitica",
                column: "ejecucion_id");

            migrationBuilder.CreateIndex(
                name: "IX_imputacion_analitica_partida_id",
                schema: "contabilidad",
                table: "imputacion_analitica",
                column: "partida_id");

            migrationBuilder.CreateIndex(
                name: "ix_imputacion_analitica_apunte",
                schema: "contabilidad",
                table: "imputacion_analitica",
                column: "apunte_id");

            migrationBuilder.CreateIndex(
                name: "ix_imputacion_analitica_asiento",
                schema: "contabilidad",
                table: "imputacion_analitica",
                column: "asiento_id");

            migrationBuilder.CreateIndex(
                name: "ix_imputacion_analitica_empresa_fecha",
                schema: "contabilidad",
                table: "imputacion_analitica",
                columns: new[] { "empresa_id", "fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_linea_clave_reparto_centro_id",
                schema: "contabilidad",
                table: "linea_clave_reparto",
                column: "centro_id");

            migrationBuilder.CreateIndex(
                name: "ix_linea_clave_reparto_clave",
                schema: "contabilidad",
                table: "linea_clave_reparto",
                column: "clave_reparto_id");

            migrationBuilder.CreateIndex(
                name: "IX_partida_analitica_padre_id",
                schema: "contabilidad",
                table: "partida_analitica",
                column: "padre_id");

            migrationBuilder.CreateIndex(
                name: "ux_partida_analitica_grupo_codigo",
                schema: "contabilidad",
                table: "partida_analitica",
                columns: new[] { "grupo_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_periodo_analitico_empresa_codigo",
                schema: "contabilidad",
                table: "periodo_analitico",
                columns: new[] { "empresa_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_regla_analitica_centro_id",
                schema: "contabilidad",
                table: "regla_analitica",
                column: "centro_id");

            migrationBuilder.CreateIndex(
                name: "IX_regla_analitica_clave_reparto_id",
                schema: "contabilidad",
                table: "regla_analitica",
                column: "clave_reparto_id");

            migrationBuilder.CreateIndex(
                name: "IX_regla_analitica_partida_id",
                schema: "contabilidad",
                table: "regla_analitica",
                column: "partida_id");

            migrationBuilder.CreateIndex(
                name: "ix_regla_analitica_empresa",
                schema: "contabilidad",
                table: "regla_analitica",
                column: "empresa_id");

            // ----- Aislamiento: maestros del grupo, datos de la empresa, líneas heredan de su clave -----
            migrationBuilder.Sql(GarantiasSql.FuncionesComunes);
            migrationBuilder.Sql(RlsSql.ActivarPorGrupo("contabilidad", "centro_analitico"));
            migrationBuilder.Sql(RlsSql.ActivarPorGrupo("contabilidad", "partida_analitica"));
            migrationBuilder.Sql(RlsSql.ActivarPorGrupo("contabilidad", "clave_reparto"));
            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("contabilidad", "linea_clave_reparto", "clave_reparto_id", "contabilidad", "clave_reparto"));
            migrationBuilder.Sql(RlsSql.Activar("contabilidad", "regla_analitica"));
            migrationBuilder.Sql(RlsSql.Activar("contabilidad", "periodo_analitico"));
            migrationBuilder.Sql(RlsSql.Activar("contabilidad", "ejecucion_analitica"));
            migrationBuilder.Sql(RlsSql.Activar("contabilidad", "imputacion_analitica"));

            // ----- Claves foráneas reales al apunte y al asiento (en Hispatec, origen polimórfico sin FK) -----
            migrationBuilder.Sql("""
                ALTER TABLE contabilidad.imputacion_analitica
                    ADD CONSTRAINT fk_imputacion_analitica_apunte FOREIGN KEY (apunte_id) REFERENCES contabilidad.apunte (id)
                        ON DELETE CASCADE DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_imputacion_analitica_asiento FOREIGN KEY (asiento_id) REFERENCES contabilidad.asiento (id)
                        ON DELETE CASCADE DEFERRABLE INITIALLY DEFERRED;
                """);

            // ----- Restricciones de valores -----
            migrationBuilder.Sql("""
                ALTER TABLE contabilidad.centro_analitico
                    ADD CONSTRAINT ck_centro_analitico_tipo CHECK (tipo IN ('CentroCoste', 'Proyecto', 'Departamento', 'Campana', 'Finca', 'Otro')),
                    ADD CONSTRAINT ck_centro_analitico_padre CHECK (padre_id IS DISTINCT FROM id);
                ALTER TABLE contabilidad.partida_analitica
                    ADD CONSTRAINT ck_partida_analitica_naturaleza CHECK (naturaleza IN ('Ingreso', 'Gasto')),
                    ADD CONSTRAINT ck_partida_analitica_padre CHECK (padre_id IS DISTINCT FROM id);
                ALTER TABLE contabilidad.linea_clave_reparto
                    ADD CONSTRAINT ck_linea_clave_reparto_porcentaje CHECK (porcentaje > 0 AND porcentaje <= 100);
                ALTER TABLE contabilidad.regla_analitica
                    ADD CONSTRAINT ck_regla_analitica_destino CHECK ((centro_id IS NULL) <> (clave_reparto_id IS NULL)),
                    ADD CONSTRAINT ck_regla_analitica_criterio CHECK (prefijo_cuenta IS NOT NULL OR tercero_id IS NOT NULL OR actividad_negocio_id IS NOT NULL OR familia IS NOT NULL),
                    ADD CONSTRAINT ck_regla_analitica_prefijo CHECK (prefijo_cuenta IS NULL OR prefijo_cuenta ~ '^[67][0-9]*$'),
                    ADD CONSTRAINT ck_regla_analitica_vigencia CHECK (vigente_hasta IS NULL OR vigente_desde IS NULL OR vigente_hasta >= vigente_desde);
                ALTER TABLE contabilidad.periodo_analitico
                    ADD CONSTRAINT ck_periodo_analitico_fechas CHECK (hasta >= desde);
                ALTER TABLE contabilidad.ejecucion_analitica
                    ADD CONSTRAINT ck_ejecucion_analitica_tipo CHECK (tipo IN ('ImputacionPendientes', 'RepartoSecundario')),
                    ADD CONSTRAINT ck_ejecucion_analitica_reparto CHECK ((tipo = 'RepartoSecundario') = (centro_origen_id IS NOT NULL AND clave_reparto_id IS NOT NULL));
                ALTER TABLE contabilidad.imputacion_analitica
                    ADD CONSTRAINT ck_imputacion_analitica_naturaleza CHECK (
                        (naturaleza = 'Gasto' AND cuenta_codigo LIKE '6%') OR (naturaleza = 'Ingreso' AND cuenta_codigo LIKE '7%')),
                    ADD CONSTRAINT ck_imputacion_analitica_origen CHECK (origen IN ('Manual', 'Regla', 'Proceso', 'Reparto')),
                    ADD CONSTRAINT ck_imputacion_analitica_apunte CHECK ((apunte_id IS NULL) = (origen = 'Reparto') AND (apunte_id IS NULL) = (asiento_id IS NULL)),
                    ADD CONSTRAINT ck_imputacion_analitica_ejecucion CHECK ((origen IN ('Proceso', 'Reparto')) = (ejecucion_id IS NOT NULL)),
                    ADD CONSTRAINT ck_imputacion_analitica_importe CHECK (importe <> 0);
                """);

            // ----- Centros y partidas del propio grupo (la RLS del maestro los oculta si son de otro) -----
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION contabilidad.analitica_maestros_visibles() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    fila jsonb := to_jsonb(NEW);
                    columna text;
                BEGIN
                    FOREACH columna IN ARRAY ARRAY['centro_id', 'centro_origen_id'] LOOP
                        IF fila ? columna AND fila ->> columna IS NOT NULL AND NOT EXISTS (
                            SELECT 1 FROM contabilidad.centro_analitico WHERE id = (fila ->> columna)::uuid) THEN
                            PERFORM public.alxor_error('analitica.centro', 'El centro analítico no existe en este grupo.');
                        END IF;
                    END LOOP;
                    IF fila ? 'partida_id' AND fila ->> 'partida_id' IS NOT NULL AND NOT EXISTS (
                        SELECT 1 FROM contabilidad.partida_analitica WHERE id = (fila ->> 'partida_id')::uuid) THEN
                        PERFORM public.alxor_error('analitica.partida', 'La partida analítica no existe en este grupo.');
                    END IF;
                    RETURN NEW;
                END $f$;

                CREATE TRIGGER tg_imputacion_analitica_maestros BEFORE INSERT OR UPDATE ON contabilidad.imputacion_analitica
                    FOR EACH ROW EXECUTE FUNCTION contabilidad.analitica_maestros_visibles();
                CREATE TRIGGER tg_regla_analitica_maestros BEFORE INSERT OR UPDATE ON contabilidad.regla_analitica
                    FOR EACH ROW EXECUTE FUNCTION contabilidad.analitica_maestros_visibles();
                CREATE TRIGGER tg_linea_clave_reparto_maestros BEFORE INSERT OR UPDATE ON contabilidad.linea_clave_reparto
                    FOR EACH ROW EXECUTE FUNCTION contabilidad.analitica_maestros_visibles();
                CREATE TRIGGER tg_ejecucion_analitica_maestros BEFORE INSERT OR UPDATE ON contabilidad.ejecucion_analitica
                    FOR EACH ROW EXECUTE FUNCTION contabilidad.analitica_maestros_visibles();
                """);

            // ----- La imputación de un apunte es coherente con él y no lo supera -----
            // Se comprueba al confirmar (y las FK son diferidas): EF puede insertar la imputación antes
            // que el asiento y sus apuntes, porque no conoce esa relación.
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION contabilidad.imputacion_coherente() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    ap record;
                BEGIN
                    IF NEW.apunte_id IS NULL OR NOT EXISTS (SELECT 1 FROM contabilidad.imputacion_analitica WHERE id = NEW.id) THEN
                        RETURN NULL;
                    END IF;
                    SELECT p.cuenta_codigo, p.asiento_id, a.fecha, a.empresa_id INTO ap
                      FROM contabilidad.apunte p JOIN contabilidad.asiento a ON a.id = p.asiento_id WHERE p.id = NEW.apunte_id;
                    IF NOT FOUND OR ap.asiento_id <> NEW.asiento_id OR ap.cuenta_codigo <> NEW.cuenta_codigo
                       OR ap.fecha <> NEW.fecha OR ap.empresa_id <> NEW.empresa_id THEN
                        PERFORM public.alxor_error('imputacion.apunte', 'La imputación no coincide con su apunte (cuenta, fecha, asiento o empresa).');
                    END IF;
                    RETURN NULL;
                END $f$;

                CREATE CONSTRAINT TRIGGER tg_imputacion_analitica_coherente AFTER INSERT OR UPDATE ON contabilidad.imputacion_analitica
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION contabilidad.imputacion_coherente();

                -- Al confirmar: lo imputado de un apunte tiene su signo y no pasa de su importe; un
                -- reparto secundario suma cero (lo que sale del centro origen llega a los destinos).
                CREATE OR REPLACE FUNCTION contabilidad.imputacion_cuadra() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    fila contabilidad.imputacion_analitica;
                    importe_apunte numeric;
                    suma numeric;
                    opuestas int;
                BEGIN
                    fila := CASE WHEN TG_OP = 'DELETE' THEN OLD ELSE NEW END;

                    IF fila.apunte_id IS NOT NULL THEN
                        SELECT CASE WHEN p.cuenta_codigo LIKE '7%' THEN p.haber - p.debe ELSE p.debe - p.haber END INTO importe_apunte
                          FROM contabilidad.apunte p WHERE p.id = fila.apunte_id;
                        IF FOUND THEN
                            SELECT coalesce(sum(importe), 0), count(*) FILTER (WHERE sign(importe) <> sign(importe_apunte))
                              INTO suma, opuestas FROM contabilidad.imputacion_analitica WHERE apunte_id = fila.apunte_id;
                            IF opuestas > 0 OR abs(suma) > abs(importe_apunte) THEN
                                PERFORM public.alxor_error('imputacion.exceso',
                                    format('Lo imputado del apunte (%s) supera su importe (%s) o tiene otro signo.', suma, importe_apunte));
                            END IF;
                        END IF;
                    END IF;

                    IF fila.origen = 'Reparto' AND EXISTS (SELECT 1 FROM contabilidad.ejecucion_analitica WHERE id = fila.ejecucion_id) THEN
                        SELECT coalesce(sum(importe), 0) INTO suma FROM contabilidad.imputacion_analitica WHERE ejecucion_id = fila.ejecucion_id;
                        IF suma <> 0 THEN
                            PERFORM public.alxor_error('reparto.descuadrado', format('El reparto secundario no suma cero (%s).', suma));
                        END IF;
                    END IF;
                    RETURN NULL;
                END $f$;

                CREATE CONSTRAINT TRIGGER tg_imputacion_analitica_cuadra AFTER INSERT OR UPDATE OR DELETE ON contabilidad.imputacion_analitica
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION contabilidad.imputacion_cuadra();
                """);

            // ----- Periodos analíticos: sin solaparse, y cerrados no se tocan -----
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION contabilidad.periodo_analitico_sin_solape() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                BEGIN
                    IF EXISTS (SELECT 1 FROM contabilidad.periodo_analitico
                                WHERE empresa_id = NEW.empresa_id AND id <> NEW.id AND desde <= NEW.hasta AND NEW.desde <= hasta) THEN
                        PERFORM public.alxor_error('periodo.solapado', 'El periodo analítico se solapa con otro de la empresa.');
                    END IF;
                    RETURN NEW;
                END $f$;

                CREATE TRIGGER tg_periodo_analitico_sin_solape BEFORE INSERT OR UPDATE ON contabilidad.periodo_analitico
                    FOR EACH ROW EXECUTE FUNCTION contabilidad.periodo_analitico_sin_solape();

                CREATE OR REPLACE FUNCTION contabilidad.imputacion_periodo_abierto() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    fila contabilidad.imputacion_analitica;
                BEGIN
                    FOREACH fila IN ARRAY (CASE TG_OP WHEN 'INSERT' THEN ARRAY[NEW] WHEN 'DELETE' THEN ARRAY[OLD] ELSE ARRAY[OLD, NEW] END) LOOP
                        IF TG_OP = 'DELETE' AND public.alxor_borrando_empresa(fila.empresa_id) THEN
                            CONTINUE;
                        END IF;
                        IF EXISTS (SELECT 1 FROM contabilidad.periodo_analitico
                                    WHERE empresa_id = fila.empresa_id AND cerrado AND fila.fecha BETWEEN desde AND hasta) THEN
                            PERFORM public.alxor_error('analitica.periodo_cerrado', 'El periodo analítico está cerrado: sus imputaciones no se pueden cambiar.');
                        END IF;
                    END LOOP;
                    RETURN CASE WHEN TG_OP = 'DELETE' THEN OLD ELSE NEW END;
                END $f$;

                CREATE TRIGGER tg_imputacion_analitica_periodo BEFORE INSERT OR UPDATE OR DELETE ON contabilidad.imputacion_analitica
                    FOR EACH ROW EXECUTE FUNCTION contabilidad.imputacion_periodo_abierto();
                """);

            // ----- Una clave de reparto suma exactamente 100 -----
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION contabilidad.clave_reparto_suma_100() RETURNS trigger
                LANGUAGE plpgsql AS $f$
                DECLARE
                    clave uuid := CASE WHEN TG_OP = 'DELETE' THEN OLD.clave_reparto_id ELSE NEW.clave_reparto_id END;
                    suma numeric;
                BEGIN
                    IF EXISTS (SELECT 1 FROM contabilidad.clave_reparto WHERE id = clave) THEN
                        SELECT coalesce(sum(porcentaje), 0) INTO suma FROM contabilidad.linea_clave_reparto WHERE clave_reparto_id = clave;
                        IF suma <> 100 THEN
                            PERFORM public.alxor_error('clave.reparto', format('Los porcentajes de la clave de reparto deben sumar 100 (suman %s).', suma));
                        END IF;
                    END IF;
                    RETURN NULL;
                END $f$;

                CREATE CONSTRAINT TRIGGER tg_linea_clave_reparto_suma AFTER INSERT OR UPDATE OR DELETE ON contabilidad.linea_clave_reparto
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION contabilidad.clave_reparto_suma_100();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP FUNCTION IF EXISTS contabilidad.analitica_maestros_visibles() CASCADE;
                DROP FUNCTION IF EXISTS contabilidad.imputacion_coherente() CASCADE;
                DROP FUNCTION IF EXISTS contabilidad.imputacion_cuadra() CASCADE;
                DROP FUNCTION IF EXISTS contabilidad.periodo_analitico_sin_solape() CASCADE;
                DROP FUNCTION IF EXISTS contabilidad.imputacion_periodo_abierto() CASCADE;
                DROP FUNCTION IF EXISTS contabilidad.clave_reparto_suma_100() CASCADE;
                """);

            migrationBuilder.DropTable(
                name: "imputacion_analitica",
                schema: "contabilidad");

            migrationBuilder.DropTable(
                name: "linea_clave_reparto",
                schema: "contabilidad");

            migrationBuilder.DropTable(
                name: "periodo_analitico",
                schema: "contabilidad");

            migrationBuilder.DropTable(
                name: "regla_analitica",
                schema: "contabilidad");

            migrationBuilder.DropTable(
                name: "ejecucion_analitica",
                schema: "contabilidad");

            migrationBuilder.DropTable(
                name: "partida_analitica",
                schema: "contabilidad");

            migrationBuilder.DropTable(
                name: "centro_analitico",
                schema: "contabilidad");

            migrationBuilder.DropTable(
                name: "clave_reparto",
                schema: "contabilidad");
        }
    }
}
