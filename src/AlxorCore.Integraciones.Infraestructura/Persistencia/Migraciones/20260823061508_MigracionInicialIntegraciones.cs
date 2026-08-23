using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Integraciones.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class MigracionInicialIntegraciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "integraciones");

            migrationBuilder.CreateTable(
                name: "clave_api",
                schema: "integraciones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    prefijo = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    hash_secreto = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ultimo_uso_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    revocada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clave_api", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "entrega_webhook",
                schema: "integraciones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    suscripcion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    url = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    secreto_firma = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    evento = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    intentos = table.Column<int>(type: "integer", nullable: false),
                    proximo_intento = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ultima_respuesta = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    entregada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entrega_webhook", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "suscripcion_webhook",
                schema: "integraciones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    url = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    secreto = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    eventos = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suscripcion_webhook", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_clave_api_hash",
                schema: "integraciones",
                table: "clave_api",
                column: "hash_secreto",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_entrega_webhook_estado",
                schema: "integraciones",
                table: "entrega_webhook",
                columns: new[] { "estado", "proximo_intento" });

            migrationBuilder.CreateIndex(
                name: "ix_suscripcion_webhook_empresa",
                schema: "integraciones",
                table: "suscripcion_webhook",
                columns: new[] { "empresa_id", "activa" });

            // RLS por empresa en las tablas de webhooks (siempre se acceden dentro del contexto de una
            // empresa). La tabla «clave_api» NO lleva RLS a propósito: es la tabla de autenticación de la
            // API pública y se consulta por hash ANTES de resolver el tenant.
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("integraciones", "suscripcion_webhook"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("integraciones", "entrega_webhook"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clave_api",
                schema: "integraciones");

            migrationBuilder.DropTable(
                name: "entrega_webhook",
                schema: "integraciones");

            migrationBuilder.DropTable(
                name: "suscripcion_webhook",
                schema: "integraciones");
        }
    }
}
