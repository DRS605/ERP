using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Agro.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AutoevaluacionesGlobalGap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "autoevaluacion",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lista_control_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lista_codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    agricultor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    auditor = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    observaciones = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    cerrada = table.Column<bool>(type: "boolean", nullable: false),
                    cerrada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    creada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_autoevaluacion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lista_control",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lista_control", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "respuesta_autoevaluacion",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    texto = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    nivel = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    resultado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    comentario = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    accion_correctiva = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    fecha_limite = table.Column<DateOnly>(type: "date", nullable: true),
                    autoevaluacion_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_respuesta_autoevaluacion", x => x.id);
                    table.ForeignKey(
                        name: "FK_respuesta_autoevaluacion_autoevaluacion_autoevaluacion_id",
                        column: x => x.autoevaluacion_id,
                        principalSchema: "agro",
                        principalTable: "autoevaluacion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "punto_control",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    texto = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    nivel = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    lista_control_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_punto_control", x => x.id);
                    table.ForeignKey(
                        name: "FK_punto_control_lista_control_lista_control_id",
                        column: x => x.lista_control_id,
                        principalSchema: "agro",
                        principalTable: "lista_control",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_autoevaluacion_agricultor",
                schema: "agro",
                table: "autoevaluacion",
                column: "agricultor_id");

            migrationBuilder.CreateIndex(
                name: "ix_autoevaluacion_fecha",
                schema: "agro",
                table: "autoevaluacion",
                columns: new[] { "empresa_id", "fecha" });

            migrationBuilder.CreateIndex(
                name: "ix_autoevaluacion_lista",
                schema: "agro",
                table: "autoevaluacion",
                column: "lista_control_id");

            migrationBuilder.CreateIndex(
                name: "ux_lista_control_codigo",
                schema: "agro",
                table: "lista_control",
                columns: new[] { "empresa_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_punto_control_lista_control_id",
                schema: "agro",
                table: "punto_control",
                column: "lista_control_id");

            migrationBuilder.CreateIndex(
                name: "IX_respuesta_autoevaluacion_autoevaluacion_id",
                schema: "agro",
                table: "respuesta_autoevaluacion",
                column: "autoevaluacion_id");
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("agro", "lista_control"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("agro", "autoevaluacion"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("agro", "punto_control", "lista_control_id", "agro", "lista_control"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("agro", "respuesta_autoevaluacion", "autoevaluacion_id", "agro", "autoevaluacion"));
            migrationBuilder.Sql("""
                ALTER TABLE agro.autoevaluacion
                    ADD CONSTRAINT fk_autoevaluacion_lista FOREIGN KEY (lista_control_id) REFERENCES agro.lista_control (id),
                    ADD CONSTRAINT ck_autoevaluacion_cierre CHECK (cerrada = (cerrada_en IS NOT NULL)),
                    ADD CONSTRAINT ck_autoevaluacion_tipo CHECK (tipo IN ('Autoevaluacion', 'AuditoriaInterna'));
                ALTER TABLE agro.punto_control ADD CONSTRAINT ck_punto_control_nivel CHECK (nivel IN ('Mayor', 'Menor', 'Recomendacion'));
                ALTER TABLE agro.respuesta_autoevaluacion
                    ADD CONSTRAINT ck_respuesta_autoevaluacion CHECK (nivel IN ('Mayor', 'Menor', 'Recomendacion')
                        AND (resultado IS NULL OR resultado IN ('Cumple', 'NoCumple', 'NoAplica'))
                        AND (resultado = 'NoCumple' OR (accion_correctiva IS NULL AND fecha_limite IS NULL)));

                -- Una evaluación cerrada es el registro de la auditoría: ni ella ni sus respuestas cambian ni se borran.
                CREATE FUNCTION agro.fn_autoevaluacion_cerrada() RETURNS trigger LANGUAGE plpgsql AS $$
                DECLARE v_id uuid;
                BEGIN
                    IF TG_TABLE_NAME = 'autoevaluacion' THEN
                        IF OLD.cerrada THEN
                            RAISE EXCEPTION 'La evaluación está cerrada: no se modifica ni se borra.' USING ERRCODE = 'P0001';
                        END IF;
                    ELSE
                        v_id := CASE WHEN TG_OP = 'INSERT' THEN NEW.autoevaluacion_id ELSE OLD.autoevaluacion_id END;
                        IF EXISTS (SELECT 1 FROM agro.autoevaluacion a WHERE a.id = v_id AND a.cerrada) THEN
                            RAISE EXCEPTION 'La evaluación está cerrada: sus respuestas no cambian.' USING ERRCODE = 'P0001';
                        END IF;
                    END IF;
                    RETURN CASE WHEN TG_OP = 'DELETE' THEN OLD ELSE NEW END;
                END $$;
                CREATE TRIGGER tg_autoevaluacion_cerrada BEFORE UPDATE OR DELETE ON agro.autoevaluacion
                    FOR EACH ROW EXECUTE FUNCTION agro.fn_autoevaluacion_cerrada();
                CREATE TRIGGER tg_respuesta_autoevaluacion_cerrada BEFORE INSERT OR UPDATE OR DELETE ON agro.respuesta_autoevaluacion
                    FOR EACH ROW EXECUTE FUNCTION agro.fn_autoevaluacion_cerrada();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS tg_respuesta_autoevaluacion_cerrada ON agro.respuesta_autoevaluacion;
                DROP TRIGGER IF EXISTS tg_autoevaluacion_cerrada ON agro.autoevaluacion;
                DROP FUNCTION IF EXISTS agro.fn_autoevaluacion_cerrada();
                """);
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.QuitarRlsPorPadre("agro", "respuesta_autoevaluacion"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.QuitarRlsPorPadre("agro", "punto_control"));
            migrationBuilder.DropTable(
                name: "punto_control",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "respuesta_autoevaluacion",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "lista_control",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "autoevaluacion",
                schema: "agro");
        }
    }
}
