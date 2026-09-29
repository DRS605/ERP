using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Logistica.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class InicialLogistica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "logistica");

            migrationBuilder.CreateTable(
                name: "configuracion",
                schema: "logistica",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    prefijo_gs1 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    digito_extension = table.Column<int>(type: "integer", nullable: false),
                    ultima_serie = table.Column<long>(type: "bigint", nullable: false),
                    mezclar_lotes = table.Column<bool>(type: "boolean", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ficha_logistica",
                schema: "logistica",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gtin = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    gtin_caja = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    unidades_por_caja = table.Column<int>(type: "integer", nullable: false),
                    peso_neto_unidad_kg = table.Column<decimal>(type: "numeric(12,4)", nullable: true),
                    peso_bruto_caja_kg = table.Column<decimal>(type: "numeric(12,4)", nullable: true),
                    largo_caja_mm = table.Column<int>(type: "integer", nullable: true),
                    ancho_caja_mm = table.Column<int>(type: "integer", nullable: true),
                    alto_caja_mm = table.Column<int>(type: "integer", nullable: true),
                    cajas_por_capa = table.Column<int>(type: "integer", nullable: true),
                    capas = table.Column<int>(type: "integer", nullable: true),
                    soporte_id = table.Column<Guid>(type: "uuid", nullable: true),
                    altura_max_pale_mm = table.Column<int>(type: "integer", nullable: true),
                    peso_max_pale_kg = table.Column<decimal>(type: "numeric(10,3)", nullable: true),
                    remontable = table.Column<bool>(type: "boolean", nullable: false),
                    temperatura_min_c = table.Column<int>(type: "integer", nullable: true),
                    temperatura_max_c = table.Column<int>(type: "integer", nullable: true),
                    vida_util_dias = table.Column<int>(type: "integer", nullable: true),
                    vida_minima_entrega_dias = table.Column<int>(type: "integer", nullable: true),
                    gestion_lotes = table.Column<bool>(type: "boolean", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ficha_logistica", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plantilla_paletizado",
                schema: "logistica",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    soporte_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cajas_por_capa = table.Column<int>(type: "integer", nullable: false),
                    capas = table.Column<int>(type: "integer", nullable: false),
                    altura_max_mm = table.Column<int>(type: "integer", nullable: true),
                    peso_max_kg = table.Column<decimal>(type: "numeric(10,3)", nullable: true),
                    instrucciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plantilla_paletizado", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tipo_soporte",
                schema: "logistica",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    largo_mm = table.Column<int>(type: "integer", nullable: false),
                    ancho_mm = table.Column<int>(type: "integer", nullable: false),
                    alto_mm = table.Column<int>(type: "integer", nullable: false),
                    tara_kg = table.Column<decimal>(type: "numeric(10,3)", nullable: false),
                    carga_max_kg = table.Column<decimal>(type: "numeric(10,3)", nullable: true),
                    envase_producto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipo_soporte", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "unidad_logistica",
                schema: "logistica",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sscc = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: false),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    origen = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    soporte_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tara_kg = table.Column<decimal>(type: "numeric(10,3)", nullable: false),
                    padre_id = table.Column<Guid>(type: "uuid", nullable: true),
                    almacen_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ubicacion_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    pedido_venta_id = table.Column<Guid>(type: "uuid", nullable: true),
                    orden_fabricacion_id = table.Column<Guid>(type: "uuid", nullable: true),
                    plantilla_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cajas_completa = table.Column<int>(type: "integer", nullable: true),
                    altura_mm = table.Column<int>(type: "integer", nullable: true),
                    peso_bruto_kg = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    motivo_anulacion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    creada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    cerrada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    fecha_expedicion = table.Column<DateOnly>(type: "date", nullable: true),
                    referencia_expedicion = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_unidad_logistica", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "linea_unidad_logistica",
                schema: "logistica",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lote = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    fecha_caducidad = table.Column<DateOnly>(type: "date", nullable: true),
                    cajas = table.Column<int>(type: "integer", nullable: false),
                    unidades = table.Column<decimal>(type: "numeric(14,4)", nullable: false),
                    peso_neto_kg = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    peso_bruto_kg = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    unidad_logistica_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_unidad_logistica", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_unidad_logistica_unidad_logistica_unidad_logistica_id",
                        column: x => x.unidad_logistica_id,
                        principalSchema: "logistica",
                        principalTable: "unidad_logistica",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_configuracion_logistica_empresa",
                schema: "logistica",
                table: "configuracion",
                column: "empresa_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ficha_logistica_gtin",
                schema: "logistica",
                table: "ficha_logistica",
                columns: new[] { "empresa_id", "gtin" });

            migrationBuilder.CreateIndex(
                name: "ix_ficha_logistica_gtin_caja",
                schema: "logistica",
                table: "ficha_logistica",
                columns: new[] { "empresa_id", "gtin_caja" });

            migrationBuilder.CreateIndex(
                name: "ix_ficha_logistica_soporte",
                schema: "logistica",
                table: "ficha_logistica",
                column: "soporte_id");

            migrationBuilder.CreateIndex(
                name: "ux_ficha_logistica_producto",
                schema: "logistica",
                table: "ficha_logistica",
                columns: new[] { "empresa_id", "producto_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_linea_unidad_logistica_unidad_logistica_id",
                schema: "logistica",
                table: "linea_unidad_logistica",
                column: "unidad_logistica_id");

            migrationBuilder.CreateIndex(
                name: "ix_linea_unidad_logistica_producto",
                schema: "logistica",
                table: "linea_unidad_logistica",
                columns: new[] { "producto_id", "lote" });

            migrationBuilder.CreateIndex(
                name: "ix_plantilla_paletizado_cliente",
                schema: "logistica",
                table: "plantilla_paletizado",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_plantilla_paletizado_producto",
                schema: "logistica",
                table: "plantilla_paletizado",
                columns: new[] { "empresa_id", "producto_id" });

            migrationBuilder.CreateIndex(
                name: "ix_plantilla_paletizado_soporte",
                schema: "logistica",
                table: "plantilla_paletizado",
                column: "soporte_id");

            migrationBuilder.CreateIndex(
                name: "ix_tipo_soporte_envase",
                schema: "logistica",
                table: "tipo_soporte",
                column: "envase_producto_id");

            migrationBuilder.CreateIndex(
                name: "ux_tipo_soporte_codigo",
                schema: "logistica",
                table: "tipo_soporte",
                columns: new[] { "empresa_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_unidad_logistica_almacen",
                schema: "logistica",
                table: "unidad_logistica",
                column: "almacen_id");

            migrationBuilder.CreateIndex(
                name: "ix_unidad_logistica_cliente",
                schema: "logistica",
                table: "unidad_logistica",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_unidad_logistica_estado",
                schema: "logistica",
                table: "unidad_logistica",
                columns: new[] { "empresa_id", "estado", "almacen_id" });

            migrationBuilder.CreateIndex(
                name: "ix_unidad_logistica_orden",
                schema: "logistica",
                table: "unidad_logistica",
                column: "orden_fabricacion_id");

            migrationBuilder.CreateIndex(
                name: "ix_unidad_logistica_padre",
                schema: "logistica",
                table: "unidad_logistica",
                column: "padre_id");

            migrationBuilder.CreateIndex(
                name: "ix_unidad_logistica_pedido",
                schema: "logistica",
                table: "unidad_logistica",
                column: "pedido_venta_id");

            migrationBuilder.CreateIndex(
                name: "ix_unidad_logistica_plantilla",
                schema: "logistica",
                table: "unidad_logistica",
                column: "plantilla_id");

            migrationBuilder.CreateIndex(
                name: "ix_unidad_logistica_soporte",
                schema: "logistica",
                table: "unidad_logistica",
                column: "soporte_id");

            migrationBuilder.CreateIndex(
                name: "ix_unidad_logistica_ubicacion",
                schema: "logistica",
                table: "unidad_logistica",
                column: "ubicacion_id");

            migrationBuilder.CreateIndex(
                name: "ux_unidad_logistica_sscc",
                schema: "logistica",
                table: "unidad_logistica",
                columns: new[] { "empresa_id", "sscc" },
                unique: true);

            // =============================== Garantías de la base de datos ===============================
            foreach (var tabla in new[] { "configuracion", "tipo_soporte", "ficha_logistica", "plantilla_paletizado", "unidad_logistica" })
            {
                migrationBuilder.Sql(RlsSql.Activar("logistica", tabla));
            }

            migrationBuilder.Sql(GarantiasSql.RlsPorPadre("logistica", "linea_unidad_logistica", "unidad_logistica_id", "logistica", "unidad_logistica"));

            migrationBuilder.Sql("""
                ALTER TABLE logistica.configuracion
                    ADD CONSTRAINT ck_configuracion_prefijo CHECK (prefijo_gs1 ~ '^[0-9]{7,10}$'),
                    ADD CONSTRAINT ck_configuracion_extension CHECK (digito_extension BETWEEN 0 AND 9),
                    ADD CONSTRAINT ck_configuracion_serie CHECK (ultima_serie >= 0);
                ALTER TABLE logistica.tipo_soporte
                    ADD CONSTRAINT ck_tipo_soporte_medidas CHECK (largo_mm > 0 AND ancho_mm > 0 AND alto_mm >= 0),
                    ADD CONSTRAINT ck_tipo_soporte_pesos CHECK (tara_kg >= 0 AND (carga_max_kg IS NULL OR carga_max_kg > 0));
                ALTER TABLE logistica.ficha_logistica
                    ADD CONSTRAINT ck_ficha_logistica_gtin CHECK ((gtin IS NULL OR gtin ~ '^[0-9]{14}$') AND (gtin_caja IS NULL OR gtin_caja ~ '^[0-9]{14}$')),
                    ADD CONSTRAINT ck_ficha_logistica_unidades CHECK (unidades_por_caja > 0),
                    ADD CONSTRAINT ck_ficha_logistica_mosaico CHECK ((cajas_por_capa IS NULL OR cajas_por_capa > 0) AND (capas IS NULL OR capas > 0)),
                    ADD CONSTRAINT ck_ficha_logistica_pesos CHECK ((peso_neto_unidad_kg IS NULL OR peso_neto_unidad_kg >= 0) AND (peso_bruto_caja_kg IS NULL OR peso_bruto_caja_kg >= 0)),
                    ADD CONSTRAINT ck_ficha_logistica_temperatura CHECK (temperatura_min_c IS NULL OR temperatura_max_c IS NULL OR temperatura_min_c <= temperatura_max_c),
                    ADD CONSTRAINT fk_ficha_logistica_soporte FOREIGN KEY (soporte_id) REFERENCES logistica.tipo_soporte (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE logistica.plantilla_paletizado
                    ADD CONSTRAINT ck_plantilla_paletizado_mosaico CHECK (cajas_por_capa > 0 AND capas > 0),
                    ADD CONSTRAINT fk_plantilla_paletizado_soporte FOREIGN KEY (soporte_id) REFERENCES logistica.tipo_soporte (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE logistica.unidad_logistica
                    ADD CONSTRAINT ck_unidad_logistica_sscc CHECK (sscc ~ '^[0-9]{18}$'),
                    ADD CONSTRAINT ck_unidad_logistica_tipo CHECK (tipo IN ('Pale', 'Caja', 'Contenedor')),
                    ADD CONSTRAINT ck_unidad_logistica_estado CHECK (estado IN ('Abierta', 'Cerrada', 'Expedida', 'Anulada')),
                    ADD CONSTRAINT ck_unidad_logistica_origen CHECK (origen IN ('Almacen', 'Fabricacion', 'Manual')),
                    ADD CONSTRAINT ck_unidad_logistica_pesos CHECK (tara_kg >= 0 AND peso_bruto_kg >= 0),
                    ADD CONSTRAINT ck_unidad_logistica_padre CHECK (padre_id IS NULL OR padre_id <> id),
                    ADD CONSTRAINT fk_unidad_logistica_padre FOREIGN KEY (padre_id) REFERENCES logistica.unidad_logistica (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_unidad_logistica_soporte FOREIGN KEY (soporte_id) REFERENCES logistica.tipo_soporte (id) DEFERRABLE INITIALLY DEFERRED,
                    ADD CONSTRAINT fk_unidad_logistica_plantilla FOREIGN KEY (plantilla_id) REFERENCES logistica.plantilla_paletizado (id) DEFERRABLE INITIALLY DEFERRED;
                ALTER TABLE logistica.linea_unidad_logistica
                    ADD CONSTRAINT ck_linea_unidad_logistica_cantidades CHECK (cajas >= 0 AND unidades >= 0 AND (cajas > 0 OR unidades > 0)),
                    ADD CONSTRAINT ck_linea_unidad_logistica_pesos CHECK (peso_neto_kg >= 0 AND peso_bruto_kg >= 0);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(GarantiasSql.QuitarRlsPorPadre("logistica", "linea_unidad_logistica"));

            migrationBuilder.DropTable(
                name: "configuracion",
                schema: "logistica");

            migrationBuilder.DropTable(
                name: "ficha_logistica",
                schema: "logistica");

            migrationBuilder.DropTable(
                name: "linea_unidad_logistica",
                schema: "logistica");

            migrationBuilder.DropTable(
                name: "plantilla_paletizado",
                schema: "logistica");

            migrationBuilder.DropTable(
                name: "tipo_soporte",
                schema: "logistica");

            migrationBuilder.DropTable(
                name: "unidad_logistica",
                schema: "logistica");
        }
    }
}
