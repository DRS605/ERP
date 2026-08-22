using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Contabilidad.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ContabilizacionDiferida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "contabilizacion_automatica",
                schema: "contabilidad",
                table: "config_contabilidad",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "documento_pendiente",
                schema: "contabilidad",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sentido = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    origen_tipo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    origen_id = table.Column<Guid>(type: "uuid", nullable: false),
                    referencia = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    tercero_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tercero_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    fecha_documento = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_registro = table.Column<DateOnly>(type: "date", nullable: false),
                    base_imponible = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    codigo_iva = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    cuota_iva = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    porcentaje_irpf = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    retencion_irpf = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    total = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    familia = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    tipo_tercero = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    asiento_id = table.Column<Guid>(type: "uuid", nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documento_pendiente", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_documento_pendiente_empresa_estado",
                schema: "contabilidad",
                table: "documento_pendiente",
                columns: new[] { "empresa_id", "estado" });

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("contabilidad", "documento_pendiente"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "documento_pendiente",
                schema: "contabilidad");

            migrationBuilder.DropColumn(
                name: "contabilizacion_automatica",
                schema: "contabilidad",
                table: "config_contabilidad");
        }
    }
}
