using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Facturacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class CicloVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "albaran_venta",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pedido_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    serie = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    referencia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_albaran_venta", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pedido_venta",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    serie = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    presupuesto_origen_id = table.Column<Guid>(type: "uuid", nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    factura_id = table.Column<Guid>(type: "uuid", nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pedido_venta", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "linea_albaran_venta",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    linea_pedido_id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    cantidad = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    albaran_venta_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_albaran_venta", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_albaran_venta_albaran_venta_albaran_venta_id",
                        column: x => x.albaran_venta_id,
                        principalSchema: "facturacion",
                        principalTable: "albaran_venta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "linea_pedido_venta",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    cantidad = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric(14,4)", nullable: false),
                    descuento = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    codigo_iva = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    cantidad_servida = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    cantidad_facturada = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    pedido_venta_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_pedido_venta", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_pedido_venta_pedido_venta_pedido_venta_id",
                        column: x => x.pedido_venta_id,
                        principalSchema: "facturacion",
                        principalTable: "pedido_venta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_albaran_venta_empresa_pedido",
                schema: "facturacion",
                table: "albaran_venta",
                columns: new[] { "empresa_id", "pedido_id" });

            migrationBuilder.CreateIndex(
                name: "IX_linea_albaran_venta_albaran_venta_id",
                schema: "facturacion",
                table: "linea_albaran_venta",
                column: "albaran_venta_id");

            migrationBuilder.CreateIndex(
                name: "IX_linea_pedido_venta_pedido_venta_id",
                schema: "facturacion",
                table: "linea_pedido_venta",
                column: "pedido_venta_id");

            migrationBuilder.CreateIndex(
                name: "ux_pedido_venta_empresa_ejercicio_numero",
                schema: "facturacion",
                table: "pedido_venta",
                columns: new[] { "empresa_id", "ejercicio", "numero" },
                unique: true);

            // RLS sobre los cabeceros (llevan empresa_id); las líneas se protegen a través de su padre.
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("facturacion", "pedido_venta"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("facturacion", "albaran_venta"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "linea_albaran_venta",
                schema: "facturacion");

            migrationBuilder.DropTable(
                name: "linea_pedido_venta",
                schema: "facturacion");

            migrationBuilder.DropTable(
                name: "albaran_venta",
                schema: "facturacion");

            migrationBuilder.DropTable(
                name: "pedido_venta",
                schema: "facturacion");
        }
    }
}
