using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Facturacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class CertificadosFitosanitarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "certificado_fitosanitario",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    carta_porte_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    numero = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    fecha_emision = table.Column<DateOnly>(type: "date", nullable: false),
                    pais_destino = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    organismo = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    mercancia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    observaciones = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    documento_nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    documento_tipo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    documento = table.Column<byte[]>(type: "bytea", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_certificado_fitosanitario", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_certificado_fitosanitario_carta",
                schema: "facturacion",
                table: "certificado_fitosanitario",
                column: "carta_porte_id");

            migrationBuilder.CreateIndex(
                name: "ix_certificado_fitosanitario_numero",
                schema: "facturacion",
                table: "certificado_fitosanitario",
                columns: new[] { "empresa_id", "numero" });

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("facturacion", "certificado_fitosanitario"));
            migrationBuilder.Sql("""
                ALTER TABLE facturacion.certificado_fitosanitario
                    ADD CONSTRAINT fk_certificado_carta_porte FOREIGN KEY (carta_porte_id) REFERENCES facturacion.carta_porte (id),
                    ADD CONSTRAINT ck_certificado_pais CHECK (pais_destino IS NULL OR pais_destino ~ '^[A-Z]{2}$'),
                    ADD CONSTRAINT ck_certificado_documento CHECK ((documento IS NULL) = (documento_tipo IS NULL) AND (documento IS NULL OR octet_length(documento) <= 5242880));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE facturacion.certificado_fitosanitario DROP CONSTRAINT IF EXISTS fk_certificado_carta_porte;");

            migrationBuilder.DropTable(
                name: "certificado_fitosanitario",
                schema: "facturacion");
        }
    }
}
