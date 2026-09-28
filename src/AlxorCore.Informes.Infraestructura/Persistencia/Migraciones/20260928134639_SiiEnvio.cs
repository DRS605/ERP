using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Informes.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class SiiEnvio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "fiscal");

            migrationBuilder.CreateTable(
                name: "certificado",
                schema: "fiscal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pfx_cifrado = table.Column<byte[]>(type: "bytea", nullable: false),
                    clave_cifrada = table.Column<string>(type: "text", nullable: false),
                    titular = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    nif = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    caduca_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    entorno = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    actualizado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_certificado", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "envio_sii",
                schema: "fiscal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    libro = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    periodo = table.Column<int>(type: "integer", nullable: false),
                    tipo_comunicacion = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    entorno = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    registros = table.Column<int>(type: "integer", nullable: false),
                    correctos = table.Column<int>(type: "integer", nullable: false),
                    con_errores = table.Column<int>(type: "integer", nullable: false),
                    incorrectos = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    csv = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    peticion = table.Column<string>(type: "text", nullable: false),
                    respuesta = table.Column<string>(type: "text", nullable: true),
                    enviado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_envio_sii", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "registro_sii",
                schema: "fiscal",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    libro = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    fecha_expedicion = table.Column<DateOnly>(type: "date", nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    periodo = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    aceptado = table.Column<bool>(type: "boolean", nullable: false),
                    codigo_error = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    descripcion_error = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    csv = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    huella = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ultimo_envio_id = table.Column<Guid>(type: "uuid", nullable: true),
                    enviado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registro_sii", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_certificado_empresa",
                schema: "fiscal",
                table: "certificado",
                column: "empresa_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_envio_sii_empresa_fecha",
                schema: "fiscal",
                table: "envio_sii",
                columns: new[] { "empresa_id", "enviado_en" });

            migrationBuilder.CreateIndex(
                name: "ix_registro_sii_periodo",
                schema: "fiscal",
                table: "registro_sii",
                columns: new[] { "empresa_id", "libro", "ejercicio", "periodo" });

            migrationBuilder.CreateIndex(
                name: "ux_registro_sii_documento",
                schema: "fiscal",
                table: "registro_sii",
                columns: new[] { "empresa_id", "libro", "documento_id" },
                unique: true);

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("fiscal", "certificado"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("fiscal", "envio_sii"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("fiscal", "registro_sii"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "certificado",
                schema: "fiscal");

            migrationBuilder.DropTable(
                name: "envio_sii",
                schema: "fiscal");

            migrationBuilder.DropTable(
                name: "registro_sii",
                schema: "fiscal");
        }
    }
}
