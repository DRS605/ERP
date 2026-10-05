using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Extensiones.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class InicialExtensiones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "extensiones");

            migrationBuilder.CreateTable(
                name: "adjunto",
                schema: "extensiones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidad = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    entidad_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tipo_mime = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tamano = table.Column<long>(type: "bigint", nullable: false),
                    huella = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    subido_por_id = table.Column<Guid>(type: "uuid", nullable: true),
                    subido_por = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    subido_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_adjunto", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "definicion_campo",
                schema: "extensiones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidad = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    etiqueta = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    obligatorio = table.Column<bool>(type: "boolean", nullable: false),
                    valor_por_defecto = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ayuda = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_definicion_campo", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "adjunto_contenido",
                schema: "extensiones",
                columns: table => new
                {
                    adjunto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    datos = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_adjunto_contenido", x => x.adjunto_id);
                    table.ForeignKey(
                        name: "fk_adjunto_contenido_adjunto",
                        column: x => x.adjunto_id,
                        principalSchema: "extensiones",
                        principalTable: "adjunto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "opcion_campo",
                schema: "extensiones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    valor = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    definicion_campo_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_opcion_campo", x => x.id);
                    table.ForeignKey(
                        name: "FK_opcion_campo_definicion_campo_definicion_campo_id",
                        column: x => x.definicion_campo_id,
                        principalSchema: "extensiones",
                        principalTable: "definicion_campo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "regla_alerta",
                schema: "extensiones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    dias = table.Column<int>(type: "integer", nullable: true),
                    porcentaje = table.Column<decimal>(type: "numeric(7,2)", nullable: true),
                    campo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    evento = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    permiso_destino = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    ultima_evaluacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regla_alerta", x => x.id);
                    table.ForeignKey(
                        name: "fk_regla_alerta_campo",
                        column: x => x.campo_id,
                        principalSchema: "extensiones",
                        principalTable: "definicion_campo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "valor_campo",
                schema: "extensiones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    definicion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entidad = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    entidad_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    actualizado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_valor_campo", x => x.id);
                    table.ForeignKey(
                        name: "fk_valor_campo_definicion",
                        column: x => x.definicion_id,
                        principalSchema: "extensiones",
                        principalTable: "definicion_campo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "alerta",
                schema: "extensiones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    regla_id = table.Column<Guid>(type: "uuid", nullable: false),
                    clave = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    detalle = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    entidad = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    entidad_id = table.Column<Guid>(type: "uuid", nullable: true),
                    permiso_destino = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    creada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    resuelta_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    resuelta_por = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alerta", x => x.id);
                    table.ForeignKey(
                        name: "fk_alerta_regla",
                        column: x => x.regla_id,
                        principalSchema: "extensiones",
                        principalTable: "regla_alerta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "lectura_alerta",
                schema: "extensiones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    alerta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    leida_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lectura_alerta", x => x.id);
                    table.ForeignKey(
                        name: "fk_lectura_alerta_alerta",
                        column: x => x.alerta_id,
                        principalSchema: "extensiones",
                        principalTable: "alerta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_adjunto_entidad",
                schema: "extensiones",
                table: "adjunto",
                columns: new[] { "entidad", "entidad_id" });

            migrationBuilder.CreateIndex(
                name: "ix_alerta_empresa_fecha",
                schema: "extensiones",
                table: "alerta",
                columns: new[] { "empresa_id", "creada_en" });

            migrationBuilder.CreateIndex(
                name: "ux_alerta_viva_clave",
                schema: "extensiones",
                table: "alerta",
                columns: new[] { "regla_id", "clave" },
                unique: true,
                filter: "resuelta_en IS NULL");

            migrationBuilder.CreateIndex(
                name: "ux_definicion_campo_codigo",
                schema: "extensiones",
                table: "definicion_campo",
                columns: new[] { "empresa_id", "entidad", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lectura_alerta_usuario",
                schema: "extensiones",
                table: "lectura_alerta",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "ux_lectura_alerta_usuario",
                schema: "extensiones",
                table: "lectura_alerta",
                columns: new[] { "alerta_id", "usuario_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_opcion_campo_orden",
                schema: "extensiones",
                table: "opcion_campo",
                columns: new[] { "definicion_campo_id", "orden" });

            migrationBuilder.CreateIndex(
                name: "ix_regla_alerta_campo",
                schema: "extensiones",
                table: "regla_alerta",
                column: "campo_id");

            migrationBuilder.CreateIndex(
                name: "ix_valor_campo_entidad",
                schema: "extensiones",
                table: "valor_campo",
                columns: new[] { "entidad", "entidad_id" });

            migrationBuilder.CreateIndex(
                name: "ux_valor_campo_registro",
                schema: "extensiones",
                table: "valor_campo",
                columns: new[] { "definicion_id", "entidad_id" },
                unique: true);

            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.FuncionesComunes);
            foreach (var tabla in new[] { "definicion_campo", "valor_campo", "adjunto", "regla_alerta", "alerta", "lectura_alerta" })
            {
                migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("extensiones", tabla));
            }

            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("extensiones", "opcion_campo", "definicion_campo_id", "extensiones", "definicion_campo"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("extensiones", "adjunto_contenido", "adjunto_id", "extensiones", "adjunto"));
            migrationBuilder.Sql("""
                ALTER TABLE extensiones.definicion_campo ADD CONSTRAINT ck_definicion_campo_codigo CHECK (codigo ~ '^[a-z][a-z0-9_]*$');
                ALTER TABLE extensiones.adjunto ADD CONSTRAINT ck_adjunto_tamano CHECK (tamano > 0 AND tamano <= 10485760);
                ALTER TABLE extensiones.adjunto ADD CONSTRAINT ck_adjunto_huella CHECK (huella ~ '^[0-9A-F]{64}$');
                ALTER TABLE extensiones.regla_alerta ADD CONSTRAINT ck_regla_alerta_umbral CHECK ((dias IS NULL OR dias BETWEEN 0 AND 3650) AND (porcentaje IS NULL OR (porcentaje > 0 AND porcentaje <= 1000)));
                ALTER TABLE extensiones.regla_alerta ADD CONSTRAINT ck_regla_alerta_tipo CHECK (
                    (tipo <> 'FechaCampo' OR campo_id IS NOT NULL) AND (tipo <> 'Evento' OR evento IS NOT NULL));
                ALTER TABLE extensiones.alerta ADD CONSTRAINT ck_alerta_resolucion CHECK (resuelta_por IS NULL OR resuelta_en IS NOT NULL);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.QuitarRlsPorPadre("extensiones", "opcion_campo"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.QuitarRlsPorPadre("extensiones", "adjunto_contenido"));
            migrationBuilder.DropTable(
                name: "adjunto_contenido",
                schema: "extensiones");

            migrationBuilder.DropTable(
                name: "lectura_alerta",
                schema: "extensiones");

            migrationBuilder.DropTable(
                name: "opcion_campo",
                schema: "extensiones");

            migrationBuilder.DropTable(
                name: "valor_campo",
                schema: "extensiones");

            migrationBuilder.DropTable(
                name: "adjunto",
                schema: "extensiones");

            migrationBuilder.DropTable(
                name: "alerta",
                schema: "extensiones");

            migrationBuilder.DropTable(
                name: "regla_alerta",
                schema: "extensiones");

            migrationBuilder.DropTable(
                name: "definicion_campo",
                schema: "extensiones");
        }
    }
}
