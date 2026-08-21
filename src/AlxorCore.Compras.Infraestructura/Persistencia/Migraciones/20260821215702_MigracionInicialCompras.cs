using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Compras.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class MigracionInicialCompras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "compras");

            migrationBuilder.CreateTable(
                name: "albaran_compra",
                schema: "compras",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pedido_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    referencia = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_albaran_compra", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pedido_compra",
                schema: "compras",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    proveedor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    proveedor_texto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    solicitud_origen_id = table.Column<Guid>(type: "uuid", nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pedido_compra", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "solicitud_compra",
                schema: "compras",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    proveedor_sugerido = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    notas = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solicitud_compra", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "linea_albaran",
                schema: "compras",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    linea_pedido_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cantidad = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    albaran_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_albaran", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_albaran_albaran_compra_albaran_id",
                        column: x => x.albaran_id,
                        principalSchema: "compras",
                        principalTable: "albaran_compra",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "linea_pedido",
                schema: "compras",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cantidad = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    cantidad_recibida = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    cantidad_facturada = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    pedido_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_pedido", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_pedido_pedido_compra_pedido_id",
                        column: x => x.pedido_id,
                        principalSchema: "compras",
                        principalTable: "pedido_compra",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "linea_solicitud",
                schema: "compras",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cantidad = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    solicitud_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_solicitud", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_solicitud_solicitud_compra_solicitud_id",
                        column: x => x.solicitud_id,
                        principalSchema: "compras",
                        principalTable: "solicitud_compra",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_albaran_empresa_pedido",
                schema: "compras",
                table: "albaran_compra",
                columns: new[] { "empresa_id", "pedido_id" });

            migrationBuilder.CreateIndex(
                name: "IX_linea_albaran_albaran_id",
                schema: "compras",
                table: "linea_albaran",
                column: "albaran_id");

            migrationBuilder.CreateIndex(
                name: "IX_linea_pedido_pedido_id",
                schema: "compras",
                table: "linea_pedido",
                column: "pedido_id");

            migrationBuilder.CreateIndex(
                name: "IX_linea_solicitud_solicitud_id",
                schema: "compras",
                table: "linea_solicitud",
                column: "solicitud_id");

            migrationBuilder.CreateIndex(
                name: "ix_pedido_empresa_estado",
                schema: "compras",
                table: "pedido_compra",
                columns: new[] { "empresa_id", "estado" });

            migrationBuilder.CreateIndex(
                name: "ix_solicitud_empresa_estado",
                schema: "compras",
                table: "solicitud_compra",
                columns: new[] { "empresa_id", "estado" });

            // Row-Level Security por empresa (las líneas se protegen a través de su documento padre).
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("compras", "solicitud_compra"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("compras", "pedido_compra"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("compras", "albaran_compra"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Desactivar("compras", "albaran_compra"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Desactivar("compras", "pedido_compra"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Desactivar("compras", "solicitud_compra"));

            migrationBuilder.DropTable(
                name: "linea_albaran",
                schema: "compras");

            migrationBuilder.DropTable(
                name: "linea_pedido",
                schema: "compras");

            migrationBuilder.DropTable(
                name: "linea_solicitud",
                schema: "compras");

            migrationBuilder.DropTable(
                name: "albaran_compra",
                schema: "compras");

            migrationBuilder.DropTable(
                name: "pedido_compra",
                schema: "compras");

            migrationBuilder.DropTable(
                name: "solicitud_compra",
                schema: "compras");
        }
    }
}
