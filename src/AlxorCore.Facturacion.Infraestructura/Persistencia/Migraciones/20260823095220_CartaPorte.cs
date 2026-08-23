using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Facturacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class CartaPorte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "carta_porte",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    serie = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    fecha_expedicion = table.Column<DateOnly>(type: "date", nullable: false),
                    remitente_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    remitente_nif = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    destinatario_cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    destinatario_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    destinatario_nif = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    transportista_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    transportista_nif = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    matricula = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    lugar_origen = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    lugar_destino = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    fecha_carga = table.Column<DateOnly>(type: "date", nullable: true),
                    observaciones = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    albaran_id = table.Column<Guid>(type: "uuid", nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carta_porte", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "linea_carta_porte",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    bultos = table.Column<int>(type: "integer", nullable: false),
                    peso_kg = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    carta_porte_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_carta_porte", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_carta_porte_carta_porte_carta_porte_id",
                        column: x => x.carta_porte_id,
                        principalSchema: "facturacion",
                        principalTable: "carta_porte",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_carta_porte_empresa_serie_ejercicio_numero",
                schema: "facturacion",
                table: "carta_porte",
                columns: new[] { "empresa_id", "serie", "ejercicio", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_linea_carta_porte_carta_porte_id",
                schema: "facturacion",
                table: "linea_carta_porte",
                column: "carta_porte_id");

            // RLS por empresa (documento operativo por empresa), como el resto de documentos.
            migrationBuilder.Sql(RlsSql.Activar("facturacion", "carta_porte"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "linea_carta_porte",
                schema: "facturacion");

            migrationBuilder.DropTable(
                name: "carta_porte",
                schema: "facturacion");
        }
    }
}
