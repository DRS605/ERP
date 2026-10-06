using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Compras.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class DevolucionesCompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codigo_iva_factura",
                schema: "compras",
                table: "pedido_compra",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "fecha_factura_proveedor",
                schema: "compras",
                table: "pedido_compra",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "gasto_id",
                schema: "compras",
                table: "pedido_compra",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "numero_factura_proveedor",
                schema: "compras",
                table: "pedido_compra",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "porcentaje_irpf_factura",
                schema: "compras",
                table: "pedido_compra",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "devolucion_compra",
                schema: "compras",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pedido_id = table.Column<Guid>(type: "uuid", nullable: false),
                    proveedor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    proveedor_texto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    motivo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    almacen_id = table.Column<Guid>(type: "uuid", nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    gasto_abono_id = table.Column<Guid>(type: "uuid", nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_devolucion_compra", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "linea_devolucion_compra",
                schema: "compras",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    linea_pedido_id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cantidad = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric(14,4)", nullable: false),
                    lote = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    devolucion_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_devolucion_compra", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_devolucion_compra_devolucion_compra_devolucion_id",
                        column: x => x.devolucion_id,
                        principalSchema: "compras",
                        principalTable: "devolucion_compra",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_devolucion_compra_almacen",
                schema: "compras",
                table: "devolucion_compra",
                column: "almacen_id");

            migrationBuilder.CreateIndex(
                name: "ix_devolucion_compra_gasto",
                schema: "compras",
                table: "devolucion_compra",
                column: "gasto_abono_id");

            migrationBuilder.CreateIndex(
                name: "ix_devolucion_compra_pedido",
                schema: "compras",
                table: "devolucion_compra",
                column: "pedido_id");

            migrationBuilder.CreateIndex(
                name: "ix_devolucion_compra_proveedor",
                schema: "compras",
                table: "devolucion_compra",
                column: "proveedor_id");

            migrationBuilder.CreateIndex(
                name: "ux_devolucion_compra_numero",
                schema: "compras",
                table: "devolucion_compra",
                columns: new[] { "empresa_id", "ejercicio", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_linea_devolucion_compra_devolucion",
                schema: "compras",
                table: "linea_devolucion_compra",
                column: "devolucion_id");

            migrationBuilder.CreateIndex(
                name: "ix_linea_devolucion_compra_producto",
                schema: "compras",
                table: "linea_devolucion_compra",
                column: "producto_id");

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("compras", "devolucion_compra"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("compras", "linea_devolucion_compra", "devolucion_id", "compras", "devolucion_compra"));
            migrationBuilder.Sql("""
                ALTER TABLE compras.devolucion_compra
                    ADD CONSTRAINT ck_devolucion_compra_valores CHECK (estado IN ('Registrada', 'EnFactura', 'Abonada', 'SinAbono', 'Anulada')
                        AND ((estado = 'Abonada') = (gasto_abono_id IS NOT NULL))),
                    ADD CONSTRAINT fk_devolucion_compra_pedido FOREIGN KEY (pedido_id) REFERENCES compras.pedido_compra (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE compras.linea_devolucion_compra
                    ADD CONSTRAINT ck_linea_devolucion_compra_valores CHECK (cantidad > 0 AND precio_unitario >= 0);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.QuitarRlsPorPadre("compras", "linea_devolucion_compra"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Desactivar("compras", "devolucion_compra"));

            migrationBuilder.DropTable(
                name: "linea_devolucion_compra",
                schema: "compras");

            migrationBuilder.DropTable(
                name: "devolucion_compra",
                schema: "compras");

            migrationBuilder.DropColumn(
                name: "codigo_iva_factura",
                schema: "compras",
                table: "pedido_compra");

            migrationBuilder.DropColumn(
                name: "fecha_factura_proveedor",
                schema: "compras",
                table: "pedido_compra");

            migrationBuilder.DropColumn(
                name: "gasto_id",
                schema: "compras",
                table: "pedido_compra");

            migrationBuilder.DropColumn(
                name: "numero_factura_proveedor",
                schema: "compras",
                table: "pedido_compra");

            migrationBuilder.DropColumn(
                name: "porcentaje_irpf_factura",
                schema: "compras",
                table: "pedido_compra");
        }
    }
}
