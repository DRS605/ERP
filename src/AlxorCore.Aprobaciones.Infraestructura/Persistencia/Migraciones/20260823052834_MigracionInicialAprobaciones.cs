using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Aprobaciones.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class MigracionInicialAprobaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "aprobaciones");

            migrationBuilder.CreateTable(
                name: "regla_aprobacion",
                schema: "aprobaciones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_documento = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    umbral_importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    actualizado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regla_aprobacion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "solicitud_aprobacion",
                schema: "aprobaciones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_documento = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    documento_id = table.Column<Guid>(type: "uuid", nullable: false),
                    referencia = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    solicitante_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    aprobador_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    motivo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    resuelta_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solicitud_aprobacion", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_regla_aprobacion_empresa_tipo",
                schema: "aprobaciones",
                table: "regla_aprobacion",
                columns: new[] { "empresa_id", "tipo_documento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_aprobacion_empresa_estado",
                schema: "aprobaciones",
                table: "solicitud_aprobacion",
                columns: new[] { "empresa_id", "estado", "creado_en" });

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("aprobaciones", "regla_aprobacion"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("aprobaciones", "solicitud_aprobacion"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "regla_aprobacion",
                schema: "aprobaciones");

            migrationBuilder.DropTable(
                name: "solicitud_aprobacion",
                schema: "aprobaciones");
        }
    }
}
