using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Recepcion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class MigracionInicialRecepcion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "recepcion");

            migrationBuilder.CreateTable(
                name: "factura_recibida",
                schema: "recepcion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    origen = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_recepcion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    remitente_correo = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    asunto_correo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    nombre_archivo = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    tipo_contenido = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    contenido = table.Column<byte[]>(type: "bytea", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    proveedor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    proveedor_texto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    numero_factura = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    fecha_factura = table.Column<DateOnly>(type: "date", nullable: true),
                    base_imponible = table.Column<decimal>(type: "numeric(14,2)", nullable: true),
                    codigo_iva = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    porcentaje_irpf = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    gasto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    motivo_rechazo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_factura_recibida", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_factura_recibida_empresa_estado",
                schema: "recepcion",
                table: "factura_recibida",
                columns: new[] { "empresa_id", "estado" });

            // Row-Level Security por empresa (segunda barrera de aislamiento multiempresa).
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("recepcion", "factura_recibida"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Desactivar("recepcion", "factura_recibida"));

            migrationBuilder.DropTable(
                name: "factura_recibida",
                schema: "recepcion");
        }
    }
}
