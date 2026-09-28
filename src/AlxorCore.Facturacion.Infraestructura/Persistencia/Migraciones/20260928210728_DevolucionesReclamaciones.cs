using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Facturacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class DevolucionesReclamaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "concepto_reclamacion",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_concepto_reclamacion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "devolucion_venta",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    albaran_venta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    albaran_numero = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    motivo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    reclamacion_id = table.Column<Guid>(type: "uuid", nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    forma_abono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    factura_abono_id = table.Column<Guid>(type: "uuid", nullable: true),
                    abonada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    anulada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    motivo_anulacion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_devolucion_venta", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reclamacion_venta",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    albaran_venta_id = table.Column<Guid>(type: "uuid", nullable: true),
                    factura_id = table.Column<Guid>(type: "uuid", nullable: true),
                    concepto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    importe_reclamado = table.Column<decimal>(type: "numeric(14,2)", nullable: true),
                    responsable = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    resolucion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    importe_reconocido = table.Column<decimal>(type: "numeric(14,2)", nullable: true),
                    texto_resolucion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    resuelta_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    devolucion_venta_id = table.Column<Guid>(type: "uuid", nullable: true),
                    factura_abono_id = table.Column<Guid>(type: "uuid", nullable: true),
                    anulada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reclamacion_venta", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "linea_devolucion_venta",
                schema: "facturacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    orden_albaran = table.Column<int>(type: "integer", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    cantidad = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric(14,4)", nullable: false),
                    porcentaje_descuento = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    codigo_iva = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    reingresa = table.Column<bool>(type: "boolean", nullable: false),
                    devolucion_venta_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_devolucion_venta", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_devolucion_venta_devolucion_venta_devolucion_venta_id",
                        column: x => x.devolucion_venta_id,
                        principalSchema: "facturacion",
                        principalTable: "devolucion_venta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_concepto_reclamacion_codigo",
                schema: "facturacion",
                table: "concepto_reclamacion",
                columns: new[] { "empresa_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_devolucion_venta_albaran",
                schema: "facturacion",
                table: "devolucion_venta",
                column: "albaran_venta_id");

            migrationBuilder.CreateIndex(
                name: "ix_devolucion_venta_factura",
                schema: "facturacion",
                table: "devolucion_venta",
                column: "factura_abono_id");

            migrationBuilder.CreateIndex(
                name: "ix_devolucion_venta_numero",
                schema: "facturacion",
                table: "devolucion_venta",
                columns: new[] { "empresa_id", "fecha", "numero" });

            migrationBuilder.CreateIndex(
                name: "IX_linea_devolucion_venta_devolucion_venta_id",
                schema: "facturacion",
                table: "linea_devolucion_venta",
                column: "devolucion_venta_id");

            migrationBuilder.CreateIndex(
                name: "ix_reclamacion_venta_cliente",
                schema: "facturacion",
                table: "reclamacion_venta",
                columns: new[] { "empresa_id", "cliente_id" });

            migrationBuilder.CreateIndex(
                name: "ix_reclamacion_venta_numero",
                schema: "facturacion",
                table: "reclamacion_venta",
                columns: new[] { "empresa_id", "fecha", "numero" });

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("facturacion", "devolucion_venta"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("facturacion", "reclamacion_venta"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("facturacion", "concepto_reclamacion"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("facturacion", "linea_devolucion_venta", "devolucion_venta_id", "facturacion", "devolucion_venta"));
            migrationBuilder.Sql("""
                ALTER TABLE facturacion.linea_devolucion_venta ADD CONSTRAINT ck_linea_devolucion_cantidad CHECK (cantidad > 0);
                ALTER TABLE facturacion.devolucion_venta ADD CONSTRAINT ck_devolucion_estado CHECK (estado IN ('Registrada','Abonada','Anulada'));
                ALTER TABLE facturacion.devolucion_venta ADD CONSTRAINT ck_devolucion_abono CHECK (estado <> 'Abonada' OR forma_abono = 'SinAbono' OR factura_abono_id IS NOT NULL);
                ALTER TABLE facturacion.reclamacion_venta ADD CONSTRAINT ck_reclamacion_estado CHECK (estado IN ('Abierta','EnTramite','Resuelta','Anulada'));
                ALTER TABLE facturacion.reclamacion_venta ADD CONSTRAINT ck_reclamacion_importes CHECK (coalesce(importe_reclamado, 0) >= 0 AND coalesce(importe_reconocido, 0) >= 0);
                CREATE UNIQUE INDEX ux_devolucion_venta_numero ON facturacion.devolucion_venta (empresa_id, (extract(year from fecha)), numero);
                CREATE UNIQUE INDEX ux_reclamacion_venta_numero ON facturacion.reclamacion_venta (empresa_id, (extract(year from fecha)), numero);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.QuitarRlsPorPadre("facturacion", "linea_devolucion_venta"));
            migrationBuilder.DropTable(
                name: "concepto_reclamacion",
                schema: "facturacion");

            migrationBuilder.DropTable(
                name: "linea_devolucion_venta",
                schema: "facturacion");

            migrationBuilder.DropTable(
                name: "reclamacion_venta",
                schema: "facturacion");

            migrationBuilder.DropTable(
                name: "devolucion_venta",
                schema: "facturacion");
        }
    }
}
