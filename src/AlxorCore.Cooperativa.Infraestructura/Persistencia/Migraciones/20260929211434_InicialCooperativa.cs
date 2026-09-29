using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Cooperativa.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class InicialCooperativa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cooperativa");

            migrationBuilder.CreateTable(
                name: "acta",
                schema: "cooperativa",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: true),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    caracter = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    lugar = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    presentes = table.Column<int>(type: "integer", nullable: false),
                    representados = table.Column<int>(type: "integer", nullable: false),
                    presidente = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    secretario = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    orden_del_dia = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: false),
                    acuerdos = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    aprobada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acta", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "configuracion",
                schema: "cooperativa",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    forma = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    aportacion_obligatoria = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    porcentaje_fro_minimo = table.Column<decimal>(type: "numeric(7,2)", nullable: false),
                    porcentaje_fep_minimo = table.Column<decimal>(type: "numeric(7,2)", nullable: false),
                    interes_maximo_capital = table.Column<decimal>(type: "numeric(7,2)", nullable: false),
                    porcentaje_retencion = table.Column<decimal>(type: "numeric(7,2)", nullable: false),
                    base_retorno = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    deduccion_maxima_expulsion = table.Column<decimal>(type: "numeric(7,2)", nullable: false),
                    deduccion_maxima_no_justificada = table.Column<decimal>(type: "numeric(7,2)", nullable: false),
                    contabilizar = table.Column<bool>(type: "boolean", nullable: false),
                    cuenta_capital = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cuenta_desembolsos_pendientes = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cuenta_tesoreria = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cuenta_reembolsos = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cuenta_resultado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cuenta_fro = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cuenta_fep = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cuenta_reservas_voluntarias = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cuenta_retornos = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cuenta_retenciones = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reparto",
                schema: "cooperativa",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_asamblea = table.Column<DateOnly>(type: "date", nullable: true),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    base_retorno = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    excedente = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    porcentaje_fro = table.Column<decimal>(type: "numeric(7,2)", nullable: false),
                    porcentaje_fep = table.Column<decimal>(type: "numeric(7,2)", nullable: false),
                    importe_fro = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    importe_fep = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    reservas_voluntarias = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    porcentaje_intereses = table.Column<decimal>(type: "numeric(7,2)", nullable: false),
                    importe_intereses = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    importe_retorno = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    porcentaje_retencion = table.Column<decimal>(type: "numeric(7,2)", nullable: false),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    asiento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    motivo_anulacion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reparto", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "socio",
                schema: "cooperativa",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    proveedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    nif = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    fecha_alta = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_baja = table.Column<DateOnly>(type: "date", nullable: true),
                    motivo_baja = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_socio", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "linea_reparto",
                schema: "cooperativa",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    socio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    actividad = table.Column<decimal>(type: "numeric(16,3)", nullable: false),
                    capital = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    intereses = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    retorno = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    retencion = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    capitalizado = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    neto = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    reparto_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_reparto", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_reparto_reparto_reparto_id",
                        column: x => x.reparto_id,
                        principalSchema: "cooperativa",
                        principalTable: "reparto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "movimiento_capital",
                schema: "cooperativa",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    socio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    clase = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    suscrito = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    desembolsado = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    deduccion = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    concepto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    grupo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reparto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    anula_id = table.Column<Guid>(type: "uuid", nullable: true),
                    asiento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimiento_capital", x => x.id);
                    table.ForeignKey(
                        name: "fk_movimiento_capital_socio",
                        column: x => x.socio_id,
                        principalSchema: "cooperativa",
                        principalTable: "socio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ux_acta_numero",
                schema: "cooperativa",
                table: "acta",
                columns: new[] { "empresa_id", "organo", "numero" },
                unique: true,
                filter: "numero IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_configuracion_cooperativa_empresa",
                schema: "cooperativa",
                table: "configuracion",
                column: "empresa_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_linea_reparto_socio",
                schema: "cooperativa",
                table: "linea_reparto",
                column: "socio_id");

            migrationBuilder.CreateIndex(
                name: "ux_linea_reparto_socio",
                schema: "cooperativa",
                table: "linea_reparto",
                columns: new[] { "reparto_id", "socio_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_movimiento_capital_grupo",
                schema: "cooperativa",
                table: "movimiento_capital",
                column: "grupo_id");

            migrationBuilder.CreateIndex(
                name: "ix_movimiento_capital_reparto",
                schema: "cooperativa",
                table: "movimiento_capital",
                column: "reparto_id");

            migrationBuilder.CreateIndex(
                name: "ix_movimiento_capital_socio",
                schema: "cooperativa",
                table: "movimiento_capital",
                columns: new[] { "socio_id", "fecha" });

            migrationBuilder.CreateIndex(
                name: "ux_movimiento_capital_anula",
                schema: "cooperativa",
                table: "movimiento_capital",
                column: "anula_id",
                unique: true,
                filter: "anula_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_reparto_ejercicio_vivo",
                schema: "cooperativa",
                table: "reparto",
                columns: new[] { "empresa_id", "ejercicio" },
                unique: true,
                filter: "estado <> 'Anulado'");

            migrationBuilder.CreateIndex(
                name: "ix_socio_proveedor",
                schema: "cooperativa",
                table: "socio",
                column: "proveedor_id");

            migrationBuilder.CreateIndex(
                name: "ux_socio_numero",
                schema: "cooperativa",
                table: "socio",
                columns: new[] { "empresa_id", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_socio_proveedor_activo",
                schema: "cooperativa",
                table: "socio",
                columns: new[] { "empresa_id", "proveedor_id" },
                unique: true,
                filter: "fecha_baja IS NULL");

            // =============================== Garantías de la base de datos ===============================
            migrationBuilder.Sql(GarantiasSql.FuncionesComunes);
            foreach (var tabla in new[] { "configuracion", "socio", "movimiento_capital", "reparto", "acta" })
            {
                migrationBuilder.Sql(RlsSql.Activar("cooperativa", tabla));
            }

            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("cooperativa", "linea_reparto", "reparto_id", "cooperativa", "reparto"));
            migrationBuilder.Sql("""
                ALTER TABLE cooperativa.configuracion
                    ADD CONSTRAINT ck_configuracion_forma CHECK (forma IN ('Cooperativa', 'Sat')),
                    ADD CONSTRAINT ck_configuracion_base CHECK (base_retorno IN ('Kilos', 'Importe', 'Capital')),
                    ADD CONSTRAINT ck_configuracion_aportacion CHECK (aportacion_obligatoria >= 0),
                    ADD CONSTRAINT ck_configuracion_porcentajes CHECK (
                        porcentaje_fro_minimo BETWEEN 0 AND 100 AND porcentaje_fep_minimo BETWEEN 0 AND 100 AND porcentaje_fro_minimo + porcentaje_fep_minimo <= 100
                        AND interes_maximo_capital BETWEEN 0 AND 100 AND porcentaje_retencion BETWEEN 0 AND 100
                        AND deduccion_maxima_expulsion BETWEEN 0 AND 100 AND deduccion_maxima_no_justificada BETWEEN 0 AND 100),
                    ADD CONSTRAINT ck_configuracion_cuentas CHECK (
                        cuenta_capital ~ '^[0-9]+$' AND cuenta_desembolsos_pendientes ~ '^[0-9]+$' AND cuenta_tesoreria ~ '^[0-9]+$' AND cuenta_reembolsos ~ '^[0-9]+$'
                        AND cuenta_resultado ~ '^[0-9]+$' AND cuenta_fro ~ '^[0-9]+$' AND cuenta_fep ~ '^[0-9]+$' AND cuenta_reservas_voluntarias ~ '^[0-9]+$'
                        AND cuenta_retornos ~ '^[0-9]+$' AND cuenta_retenciones ~ '^[0-9]+$');

                ALTER TABLE cooperativa.socio
                    ADD CONSTRAINT ck_socio_numero CHECK (numero > 0),
                    ADD CONSTRAINT ck_socio_tipo CHECK (tipo IN ('Comun', 'DeTrabajo', 'Colaborador', 'Inactivo')),
                    ADD CONSTRAINT ck_socio_motivo CHECK (motivo_baja IS NULL OR motivo_baja IN ('VoluntariaJustificada', 'VoluntariaNoJustificada', 'Obligatoria', 'Expulsion', 'Fallecimiento')),
                    ADD CONSTRAINT ck_socio_baja CHECK ((fecha_baja IS NULL) = (motivo_baja IS NULL) AND (fecha_baja IS NULL OR fecha_baja >= fecha_alta));

                -- El número, el tercero y el alta del socio no cambian; la baja es definitiva.
                CREATE OR REPLACE FUNCTION cooperativa.socio_valido() RETURNS trigger LANGUAGE plpgsql AS $f$
                BEGIN
                    IF NEW.numero <> OLD.numero OR NEW.proveedor_id <> OLD.proveedor_id OR NEW.fecha_alta <> OLD.fecha_alta OR NEW.empresa_id <> OLD.empresa_id THEN
                        PERFORM public.alxor_error('socio.inmutable', 'El número, el tercero y la fecha de alta del socio no cambian.');
                    END IF;
                    IF OLD.fecha_baja IS NOT NULL AND (NEW.fecha_baja IS DISTINCT FROM OLD.fecha_baja OR NEW.motivo_baja IS DISTINCT FROM OLD.motivo_baja) THEN
                        PERFORM public.alxor_error('socio.baja_definitiva', 'La baja de un socio no se deshace: si vuelve, dale de alta con otro número.');
                    END IF;
                    RETURN NEW;
                END $f$;
                DROP TRIGGER IF EXISTS tg_socio_valido ON cooperativa.socio;
                CREATE TRIGGER tg_socio_valido BEFORE UPDATE ON cooperativa.socio FOR EACH ROW EXECUTE FUNCTION cooperativa.socio_valido();

                ALTER TABLE cooperativa.movimiento_capital
                    ADD CONSTRAINT ck_movimiento_capital_tipo CHECK (tipo IN ('Suscripcion', 'Desembolso', 'Reembolso', 'TransmisionSalida', 'TransmisionEntrada', 'RetornoCapitalizado', 'Anulacion')),
                    ADD CONSTRAINT ck_movimiento_capital_clase CHECK (clase IN ('Obligatoria', 'Voluntaria')),
                    ADD CONSTRAINT ck_movimiento_capital_anula CHECK ((tipo = 'Anulacion') = (anula_id IS NOT NULL)),
                    ADD CONSTRAINT ck_movimiento_capital_importes CHECK (CASE tipo
                        WHEN 'Suscripcion' THEN suscrito > 0 AND desembolsado >= 0 AND desembolsado <= suscrito AND deduccion = 0
                        WHEN 'Desembolso' THEN suscrito = 0 AND desembolsado > 0 AND deduccion = 0
                        WHEN 'Reembolso' THEN suscrito < 0 AND desembolsado <= 0 AND desembolsado >= suscrito AND deduccion >= 0 AND deduccion <= -desembolsado
                        WHEN 'TransmisionSalida' THEN suscrito < 0 AND desembolsado = suscrito AND deduccion = 0 AND grupo_id IS NOT NULL
                        WHEN 'TransmisionEntrada' THEN suscrito > 0 AND desembolsado = suscrito AND deduccion = 0 AND grupo_id IS NOT NULL
                        WHEN 'RetornoCapitalizado' THEN suscrito > 0 AND desembolsado = suscrito AND deduccion = 0 AND reparto_id IS NOT NULL AND clase = 'Voluntaria'
                        ELSE true END),
                    ADD CONSTRAINT fk_movimiento_capital_anula FOREIGN KEY (anula_id) REFERENCES cooperativa.movimiento_capital (id),
                    ADD CONSTRAINT fk_movimiento_capital_reparto FOREIGN KEY (reparto_id) REFERENCES cooperativa.reparto (id);
                """);
            migrationBuilder.Sql(GarantiasSql.SoloInsercion("cooperativa", "movimiento_capital", "capital.inmutable",
                "Los movimientos de capital no se modifican ni se borran: se anulan."));
            migrationBuilder.Sql("""
                -- Al confirmar: el movimiento encaja con el socio (de baja solo se reembolsa o se cede; el obligatorio solo se reembolsa de
                -- baja), la anulación es el movimiento anulado con el signo cambiado, la transmisión está completa, el retorno capitalizado
                -- es de un reparto contabilizado (y su anulación, de uno anulado) y el capital del socio en esa clase cuadra.
                CREATE OR REPLACE FUNCTION cooperativa.movimiento_capital_valido() RETURNS trigger LANGUAGE plpgsql AS $f$
                DECLARE
                    baja date;
                    o cooperativa.movimiento_capital%ROWTYPE;
                    estado_reparto text;
                    suscrito numeric;
                    desembolsado numeric;
                    n int;
                    socios int;
                    suma numeric;
                BEGIN
                    SELECT s.fecha_baja INTO baja FROM cooperativa.socio s WHERE s.id = NEW.socio_id;
                    IF baja IS NOT NULL AND NEW.tipo IN ('Suscripcion', 'Desembolso', 'TransmisionEntrada', 'RetornoCapitalizado') THEN
                        PERFORM public.alxor_error('capital.socio_de_baja', 'El socio está de baja: solo se le puede reembolsar o ceder el capital.');
                    END IF;
                    IF NEW.tipo = 'Reembolso' AND NEW.clase = 'Obligatoria' AND baja IS NULL THEN
                        PERFORM public.alxor_error('capital.obligatorio_sin_baja', 'El capital obligatorio solo se reembolsa al dar de baja al socio.');
                    END IF;
                    IF NEW.tipo = 'Anulacion' THEN
                        SELECT * INTO o FROM cooperativa.movimiento_capital m WHERE m.id = NEW.anula_id;
                        IF o.id IS NULL OR o.tipo = 'Anulacion' OR o.socio_id <> NEW.socio_id OR o.clase <> NEW.clase
                           OR NEW.suscrito <> -o.suscrito OR NEW.desembolsado <> -o.desembolsado OR NEW.deduccion <> -o.deduccion
                           OR NEW.fecha < o.fecha OR NEW.grupo_id IS DISTINCT FROM o.grupo_id OR NEW.reparto_id IS DISTINCT FROM o.reparto_id THEN
                            PERFORM public.alxor_error('capital.anulacion', 'La anulación deshace exactamente otro movimiento del mismo socio (y no es anterior a él).');
                        END IF;
                    END IF;
                    IF NEW.reparto_id IS NOT NULL THEN
                        SELECT r.estado INTO estado_reparto FROM cooperativa.reparto r WHERE r.id = NEW.reparto_id;
                        IF (NEW.tipo = 'RetornoCapitalizado' AND estado_reparto IS DISTINCT FROM 'Contabilizado')
                           OR (NEW.tipo = 'Anulacion' AND estado_reparto IS DISTINCT FROM 'Anulado') THEN
                            PERFORM public.alxor_error('capital.reparto', 'El retorno se capitaliza al contabilizar el reparto y se deshace al anularlo.');
                        END IF;
                    END IF;
                    IF NEW.grupo_id IS NOT NULL THEN
                        SELECT count(*) FILTER (WHERE m.tipo IN ('TransmisionSalida', 'TransmisionEntrada')), count(DISTINCT m.socio_id) FILTER (WHERE m.tipo IN ('TransmisionSalida', 'TransmisionEntrada')),
                               coalesce(sum(m.suscrito), 0)
                          INTO n, socios, suma
                          FROM cooperativa.movimiento_capital m WHERE m.grupo_id = NEW.grupo_id;
                        IF n <> 2 OR socios <> 2 OR suma <> 0 THEN
                            PERFORM public.alxor_error('capital.transmision', 'Una transmisión sale de un socio y entra en otro por el mismo importe.');
                        END IF;
                    END IF;
                    SELECT coalesce(sum(m.suscrito), 0), coalesce(sum(m.desembolsado), 0) INTO suscrito, desembolsado
                      FROM cooperativa.movimiento_capital m WHERE m.socio_id = NEW.socio_id AND m.clase = NEW.clase;
                    IF desembolsado < 0 OR desembolsado > suscrito THEN
                        PERFORM public.alxor_error('capital.descuadre', 'El capital del socio no cuadra: lo desembolsado no puede quedar en negativo ni pasar de lo suscrito.');
                    END IF;
                    RETURN NULL;
                END $f$;
                DROP TRIGGER IF EXISTS tg_movimiento_capital_valido ON cooperativa.movimiento_capital;
                CREATE CONSTRAINT TRIGGER tg_movimiento_capital_valido AFTER INSERT ON cooperativa.movimiento_capital
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION cooperativa.movimiento_capital_valido();

                ALTER TABLE cooperativa.reparto
                    ADD CONSTRAINT ck_reparto_estado CHECK (estado IN ('Borrador', 'Contabilizado', 'Anulado')),
                    ADD CONSTRAINT ck_reparto_base CHECK (base_retorno IN ('Kilos', 'Importe', 'Capital')),
                    ADD CONSTRAINT ck_reparto_importes CHECK (excedente > 0 AND importe_fro >= 0 AND importe_fep >= 0 AND reservas_voluntarias >= 0 AND importe_intereses >= 0
                        AND importe_retorno >= 0 AND porcentaje_fro BETWEEN 0 AND 100 AND porcentaje_fep BETWEEN 0 AND 100 AND porcentaje_intereses BETWEEN 0 AND 100
                        AND porcentaje_retencion BETWEEN 0 AND 100),
                    ADD CONSTRAINT ck_reparto_cuadra CHECK (excedente = importe_fro + importe_fep + reservas_voluntarias + importe_intereses + importe_retorno),
                    ADD CONSTRAINT ck_reparto_anulacion CHECK ((estado = 'Anulado') = (motivo_anulacion IS NOT NULL)),
                    ADD CONSTRAINT ck_reparto_fecha CHECK (extract(year FROM fecha) >= ejercicio);

                ALTER TABLE cooperativa.linea_reparto
                    ADD CONSTRAINT ck_linea_reparto_importes CHECK (actividad >= 0 AND capital >= 0 AND intereses >= 0 AND retorno >= 0 AND retencion >= 0 AND capitalizado >= 0
                        AND capitalizado <= retorno AND neto >= 0),
                    ADD CONSTRAINT ck_linea_reparto_cuadra CHECK (retorno + intereses = neto + retencion + capitalizado),
                    ADD CONSTRAINT fk_linea_reparto_socio FOREIGN KEY (socio_id) REFERENCES cooperativa.socio (id) DEFERRABLE INITIALLY DEFERRED;

                -- Un reparto contabilizado no cambia (solo se anula); uno anulado, nada. El borrador se recalcula o se elimina.
                CREATE OR REPLACE FUNCTION cooperativa.reparto_valido() RETURNS trigger LANGUAGE plpgsql AS $f$
                BEGIN
                    IF TG_OP = 'DELETE' THEN
                        IF OLD.estado <> 'Borrador' AND NOT public.alxor_borrando_empresa(OLD.empresa_id) THEN
                            PERFORM public.alxor_error('reparto.no_borrador', 'Un reparto contabilizado no se elimina: se anula.');
                        END IF;
                        RETURN OLD;
                    END IF;
                    IF OLD.estado = 'Borrador' AND NEW.estado = 'Anulado' THEN
                        PERFORM public.alxor_error('reparto.no_contabilizado', 'Solo se anula un reparto contabilizado (el borrador se elimina).');
                    END IF;
                    IF OLD.estado = 'Contabilizado' AND (NEW.estado <> 'Anulado'
                        OR (to_jsonb(NEW) - 'estado' - 'motivo_anulacion') <> (to_jsonb(OLD) - 'estado' - 'motivo_anulacion')) THEN
                        PERFORM public.alxor_error('reparto.contabilizado', 'El reparto está contabilizado: no cambia, solo se anula.');
                    END IF;
                    IF OLD.estado = 'Anulado' THEN
                        PERFORM public.alxor_error('reparto.anulado', 'El reparto está anulado.');
                    END IF;
                    RETURN NEW;
                END $f$;
                DROP TRIGGER IF EXISTS tg_reparto_valido ON cooperativa.reparto;
                CREATE TRIGGER tg_reparto_valido BEFORE UPDATE OR DELETE ON cooperativa.reparto FOR EACH ROW EXECUTE FUNCTION cooperativa.reparto_valido();

                -- Las líneas solo cambian con el reparto en borrador.
                CREATE OR REPLACE FUNCTION cooperativa.linea_reparto_valida() RETURNS trigger LANGUAGE plpgsql AS $f$
                DECLARE
                    estado text;
                    empresa uuid;
                BEGIN
                    SELECT r.estado, r.empresa_id INTO estado, empresa FROM cooperativa.reparto r
                     WHERE r.id = CASE WHEN TG_OP = 'DELETE' THEN OLD.reparto_id ELSE NEW.reparto_id END;
                    IF estado IS NOT NULL AND estado <> 'Borrador' AND NOT (TG_OP = 'DELETE' AND public.alxor_borrando_empresa(empresa)) THEN
                        PERFORM public.alxor_error('reparto.contabilizado', 'El reparto está contabilizado: sus líneas no cambian.');
                    END IF;
                    RETURN coalesce(NEW, OLD);
                END $f$;
                DROP TRIGGER IF EXISTS tg_linea_reparto_valida ON cooperativa.linea_reparto;
                CREATE TRIGGER tg_linea_reparto_valida BEFORE INSERT OR UPDATE OR DELETE ON cooperativa.linea_reparto FOR EACH ROW EXECUTE FUNCTION cooperativa.linea_reparto_valida();

                -- Al confirmar: las líneas suman el retorno y los intereses del reparto.
                CREATE OR REPLACE FUNCTION cooperativa.reparto_cuadra() RETURNS trigger LANGUAGE plpgsql AS $f$
                DECLARE
                    fila jsonb := CASE WHEN TG_OP = 'DELETE' THEN to_jsonb(OLD) ELSE to_jsonb(NEW) END;
                    id_reparto uuid := (CASE WHEN TG_TABLE_NAME = 'reparto' THEN fila ->> 'id' ELSE fila ->> 'reparto_id' END)::uuid;
                    r cooperativa.reparto%ROWTYPE;
                    retorno numeric;
                    intereses numeric;
                BEGIN
                    SELECT * INTO r FROM cooperativa.reparto x WHERE x.id = id_reparto;
                    IF r.id IS NULL THEN
                        RETURN NULL;
                    END IF;
                    SELECT coalesce(sum(l.retorno), 0), coalesce(sum(l.intereses), 0) INTO retorno, intereses FROM cooperativa.linea_reparto l WHERE l.reparto_id = r.id;
                    IF retorno <> r.importe_retorno OR intereses <> r.importe_intereses THEN
                        PERFORM public.alxor_error('reparto.descuadre', 'Las líneas del reparto no suman el retorno y los intereses repartidos.');
                    END IF;
                    RETURN NULL;
                END $f$;
                DROP TRIGGER IF EXISTS tg_reparto_cuadra ON cooperativa.reparto;
                CREATE CONSTRAINT TRIGGER tg_reparto_cuadra AFTER INSERT OR UPDATE ON cooperativa.reparto
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION cooperativa.reparto_cuadra();
                DROP TRIGGER IF EXISTS tg_linea_reparto_cuadra ON cooperativa.linea_reparto;
                CREATE CONSTRAINT TRIGGER tg_linea_reparto_cuadra AFTER INSERT OR UPDATE OR DELETE ON cooperativa.linea_reparto
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION cooperativa.reparto_cuadra();

                ALTER TABLE cooperativa.acta
                    ADD CONSTRAINT ck_acta_organo CHECK (organo IN ('AsambleaGeneral', 'ConsejoRector', 'JuntaRectora')),
                    ADD CONSTRAINT ck_acta_caracter CHECK (caracter IN ('Ordinaria', 'Extraordinaria', 'Universal')),
                    ADD CONSTRAINT ck_acta_estado CHECK (estado IN ('Borrador', 'Aprobada')),
                    ADD CONSTRAINT ck_acta_numero CHECK ((estado = 'Aprobada') = (numero IS NOT NULL) AND (numero IS NULL OR numero > 0) AND ((estado = 'Aprobada') = (aprobada_en IS NOT NULL))),
                    ADD CONSTRAINT ck_acta_asistentes CHECK (presentes >= 0 AND representados >= 0);

                -- Un acta aprobada forma parte del libro: ni cambia ni se borra.
                CREATE OR REPLACE FUNCTION cooperativa.acta_valida() RETURNS trigger LANGUAGE plpgsql AS $f$
                BEGIN
                    IF OLD.estado = 'Aprobada' AND NOT (TG_OP = 'DELETE' AND public.alxor_borrando_empresa(OLD.empresa_id)) THEN
                        PERFORM public.alxor_error('acta.aprobada', 'El acta está aprobada: forma parte del libro y no cambia.');
                    END IF;
                    RETURN coalesce(NEW, OLD);
                END $f$;
                DROP TRIGGER IF EXISTS tg_acta_valida ON cooperativa.acta;
                CREATE TRIGGER tg_acta_valida BEFORE UPDATE OR DELETE ON cooperativa.acta FOR EACH ROW EXECUTE FUNCTION cooperativa.acta_valida();

                -- Al confirmar: el libro de cada órgano va numerado sin huecos.
                CREATE OR REPLACE FUNCTION cooperativa.acta_numerada() RETURNS trigger LANGUAGE plpgsql AS $f$
                DECLARE
                    maximo int;
                    total int;
                BEGIN
                    IF NEW.numero IS NULL THEN
                        RETURN NULL;
                    END IF;
                    SELECT max(a.numero), count(*) INTO maximo, total FROM cooperativa.acta a WHERE a.empresa_id = NEW.empresa_id AND a.organo = NEW.organo AND a.numero IS NOT NULL;
                    IF maximo <> total THEN
                        PERFORM public.alxor_error('acta.numeracion', 'El libro de actas de cada órgano va numerado sin huecos.');
                    END IF;
                    RETURN NULL;
                END $f$;
                DROP TRIGGER IF EXISTS tg_acta_numerada ON cooperativa.acta;
                CREATE CONSTRAINT TRIGGER tg_acta_numerada AFTER INSERT OR UPDATE ON cooperativa.acta
                    DEFERRABLE INITIALLY DEFERRED FOR EACH ROW EXECUTE FUNCTION cooperativa.acta_numerada();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(GarantiasSql.QuitarRlsPorPadre("cooperativa", "linea_reparto"));
            migrationBuilder.Sql("""
                DROP FUNCTION IF EXISTS cooperativa.socio_valido() CASCADE;
                DROP FUNCTION IF EXISTS cooperativa.movimiento_capital_valido() CASCADE;
                DROP FUNCTION IF EXISTS cooperativa.reparto_valido() CASCADE;
                DROP FUNCTION IF EXISTS cooperativa.linea_reparto_valida() CASCADE;
                DROP FUNCTION IF EXISTS cooperativa.reparto_cuadra() CASCADE;
                DROP FUNCTION IF EXISTS cooperativa.acta_valida() CASCADE;
                DROP FUNCTION IF EXISTS cooperativa.acta_numerada() CASCADE;
                """);

            migrationBuilder.DropTable(
                name: "acta",
                schema: "cooperativa");

            migrationBuilder.DropTable(
                name: "configuracion",
                schema: "cooperativa");

            migrationBuilder.DropTable(
                name: "linea_reparto",
                schema: "cooperativa");

            migrationBuilder.DropTable(
                name: "movimiento_capital",
                schema: "cooperativa");

            migrationBuilder.DropTable(
                name: "reparto",
                schema: "cooperativa");

            migrationBuilder.DropTable(
                name: "socio",
                schema: "cooperativa");
        }
    }
}
