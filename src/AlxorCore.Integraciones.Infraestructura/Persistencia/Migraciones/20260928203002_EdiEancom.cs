using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Integraciones.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class EdiEancom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "configuracion_edi",
                schema: "integraciones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    gln_empresa = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracion_edi", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pedido_edi",
                schema: "integraciones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_cliente = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: false),
                    gln_comprador = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    pedido_venta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recibido_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pedido_edi", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "socio_edi",
                schema: "integraciones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gln_comprador = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    gln_facturacion = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    gln_entrega = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_socio_edi", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_configuracion_edi_empresa",
                schema: "integraciones",
                table: "configuracion_edi",
                column: "empresa_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pedido_edi_venta",
                schema: "integraciones",
                table: "pedido_edi",
                column: "pedido_venta_id");

            migrationBuilder.CreateIndex(
                name: "ux_pedido_edi_numero",
                schema: "integraciones",
                table: "pedido_edi",
                columns: new[] { "empresa_id", "gln_comprador", "numero_cliente" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_socio_edi_cliente",
                schema: "integraciones",
                table: "socio_edi",
                columns: new[] { "empresa_id", "cliente_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_socio_edi_gln",
                schema: "integraciones",
                table: "socio_edi",
                columns: new[] { "empresa_id", "gln_comprador" },
                unique: true);

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("integraciones", "configuracion_edi"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("integraciones", "socio_edi"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("integraciones", "pedido_edi"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "configuracion_edi",
                schema: "integraciones");

            migrationBuilder.DropTable(
                name: "pedido_edi",
                schema: "integraciones");

            migrationBuilder.DropTable(
                name: "socio_edi",
                schema: "integraciones");
        }
    }
}
