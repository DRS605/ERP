using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Agro.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class Fitosanitarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "almacen_id",
                schema: "agro",
                table: "tratamiento_parcela",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "articulo_id",
                schema: "agro",
                table: "tratamiento_parcela",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "cantidad_consumida",
                schema: "agro",
                table: "tratamiento_parcela",
                type: "numeric(14,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cultivo",
                schema: "agro",
                table: "tratamiento_parcela",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "fitosanitario_id",
                schema: "agro",
                table: "tratamiento_parcela",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "lote",
                schema: "agro",
                table: "tratamiento_parcela",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "cambio_fitosanitario",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fitosanitario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_registro = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    producto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    detalle = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    detectado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    revisado = table.Column<bool>(type: "boolean", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cambio_fitosanitario", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fitosanitario",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_registro = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    titular = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    fecha_caducidad = table.Column<DateOnly>(type: "date", nullable: true),
                    fecha_cancelacion = table.Column<DateOnly>(type: "date", nullable: true),
                    formulado = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    fecha_limite_venta = table.Column<DateOnly>(type: "date", nullable: true),
                    fecha_limite_uso = table.Column<DateOnly>(type: "date", nullable: true),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    actualizado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fitosanitario", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fitosanitario_materia_activa",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    riqueza = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    fitosanitario_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fitosanitario_materia_activa", x => x.id);
                    table.ForeignKey(
                        name: "FK_fitosanitario_materia_activa_fitosanitario_fitosanitario_id",
                        column: x => x.fitosanitario_id,
                        principalSchema: "agro",
                        principalTable: "fitosanitario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fitosanitario_uso",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cultivo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    plaga = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    dosis_minima = table.Column<decimal>(type: "numeric(12,4)", nullable: true),
                    dosis_maxima = table.Column<decimal>(type: "numeric(12,4)", nullable: true),
                    unidad_dosis = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    plazo_seguridad_dias = table.Column<int>(type: "integer", nullable: true),
                    aplicaciones = table.Column<int>(type: "integer", nullable: true),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    fitosanitario_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fitosanitario_uso", x => x.id);
                    table.ForeignKey(
                        name: "FK_fitosanitario_uso_fitosanitario_fitosanitario_id",
                        column: x => x.fitosanitario_id,
                        principalSchema: "agro",
                        principalTable: "fitosanitario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tratamiento_parcela_fitosanitario",
                schema: "agro",
                table: "tratamiento_parcela",
                column: "fitosanitario_id");

            migrationBuilder.CreateIndex(
                name: "ix_tratamiento_parcela_lote",
                schema: "agro",
                table: "tratamiento_parcela",
                columns: new[] { "articulo_id", "lote" });

            migrationBuilder.CreateIndex(
                name: "ix_cambio_fitosanitario_fecha",
                schema: "agro",
                table: "cambio_fitosanitario",
                columns: new[] { "empresa_id", "detectado_en" });

            migrationBuilder.CreateIndex(
                name: "ix_cambio_fitosanitario_producto",
                schema: "agro",
                table: "cambio_fitosanitario",
                column: "fitosanitario_id");

            migrationBuilder.CreateIndex(
                name: "ix_fitosanitario_producto",
                schema: "agro",
                table: "fitosanitario",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "ux_fitosanitario_numero",
                schema: "agro",
                table: "fitosanitario",
                columns: new[] { "empresa_id", "numero_registro" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fitosanitario_materia_activa_fitosanitario_id",
                schema: "agro",
                table: "fitosanitario_materia_activa",
                column: "fitosanitario_id");

            migrationBuilder.CreateIndex(
                name: "IX_fitosanitario_uso_fitosanitario_id",
                schema: "agro",
                table: "fitosanitario_uso",
                column: "fitosanitario_id");
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("agro", "fitosanitario"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("agro", "cambio_fitosanitario"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("agro", "fitosanitario_materia_activa", "fitosanitario_id", "agro", "fitosanitario"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("agro", "fitosanitario_uso", "fitosanitario_id", "agro", "fitosanitario"));
            migrationBuilder.Sql("""
                ALTER TABLE agro.fitosanitario
                    ADD CONSTRAINT ck_fitosanitario_estado CHECK (estado IN ('Autorizado', 'Suspendido', 'Caducado', 'Cancelado'));
                ALTER TABLE agro.fitosanitario_uso
                    ADD CONSTRAINT ck_fitosanitario_uso CHECK ((dosis_minima IS NULL OR dosis_minima >= 0) AND (dosis_maxima IS NULL OR dosis_maxima >= 0)
                        AND (dosis_minima IS NULL OR dosis_maxima IS NULL OR dosis_minima <= dosis_maxima)
                        AND (plazo_seguridad_dias IS NULL OR plazo_seguridad_dias BETWEEN 0 AND 365) AND (aplicaciones IS NULL OR aplicaciones >= 0));
                ALTER TABLE agro.cambio_fitosanitario
                    ADD CONSTRAINT fk_cambio_fitosanitario_producto FOREIGN KEY (fitosanitario_id) REFERENCES agro.fitosanitario (id) ON DELETE CASCADE;
                ALTER TABLE agro.tratamiento_parcela
                    ADD CONSTRAINT fk_tratamiento_parcela_fitosanitario FOREIGN KEY (fitosanitario_id) REFERENCES agro.fitosanitario (id),
                    ADD CONSTRAINT ck_tratamiento_parcela_consumo CHECK (cantidad_consumida IS NULL
                        OR (cantidad_consumida > 0 AND articulo_id IS NOT NULL AND almacen_id IS NOT NULL));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE agro.tratamiento_parcela DROP CONSTRAINT IF EXISTS fk_tratamiento_parcela_fitosanitario, DROP CONSTRAINT IF EXISTS ck_tratamiento_parcela_consumo;
                """);
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.QuitarRlsPorPadre("agro", "fitosanitario_uso"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.QuitarRlsPorPadre("agro", "fitosanitario_materia_activa"));
            migrationBuilder.DropTable(
                name: "cambio_fitosanitario",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "fitosanitario_materia_activa",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "fitosanitario_uso",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "fitosanitario",
                schema: "agro");

            migrationBuilder.DropIndex(
                name: "ix_tratamiento_parcela_fitosanitario",
                schema: "agro",
                table: "tratamiento_parcela");

            migrationBuilder.DropIndex(
                name: "ix_tratamiento_parcela_lote",
                schema: "agro",
                table: "tratamiento_parcela");

            migrationBuilder.DropColumn(
                name: "almacen_id",
                schema: "agro",
                table: "tratamiento_parcela");

            migrationBuilder.DropColumn(
                name: "articulo_id",
                schema: "agro",
                table: "tratamiento_parcela");

            migrationBuilder.DropColumn(
                name: "cantidad_consumida",
                schema: "agro",
                table: "tratamiento_parcela");

            migrationBuilder.DropColumn(
                name: "cultivo",
                schema: "agro",
                table: "tratamiento_parcela");

            migrationBuilder.DropColumn(
                name: "fitosanitario_id",
                schema: "agro",
                table: "tratamiento_parcela");

            migrationBuilder.DropColumn(
                name: "lote",
                schema: "agro",
                table: "tratamiento_parcela");
        }
    }
}
