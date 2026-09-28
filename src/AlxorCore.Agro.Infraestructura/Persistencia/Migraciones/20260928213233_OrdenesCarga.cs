using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Agro.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class OrdenesCarga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "orden_carga",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    fecha_carga = table.Column<DateOnly>(type: "date", nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    muelle = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    transportista_id = table.Column<Guid>(type: "uuid", nullable: true),
                    vehiculo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    matricula = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    conductor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    temperatura_consigna = table.Column<decimal>(type: "numeric(5,1)", nullable: true),
                    filas = table.Column<int>(type: "integer", nullable: true),
                    columnas = table.Column<int>(type: "integer", nullable: true),
                    carta_porte = table.Column<bool>(type: "boolean", nullable: false),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    creada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    finalizada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    motivo_anulacion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orden_carga", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "linea_orden_carga",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    pedido_venta_id = table.Column<Guid>(type: "uuid", nullable: false),
                    linea_pedido_id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: true),
                    descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    pales_previstos = table.Column<int>(type: "integer", nullable: false),
                    fila = table.Column<int>(type: "integer", nullable: true),
                    columna = table.Column<int>(type: "integer", nullable: true),
                    orden_carga_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_linea_orden_carga", x => x.id);
                    table.ForeignKey(
                        name: "FK_linea_orden_carga_orden_carga_orden_carga_id",
                        column: x => x.orden_carga_id,
                        principalSchema: "agro",
                        principalTable: "orden_carga",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pale_orden_carga",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    linea_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pale_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sscc = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    kilos = table.Column<decimal>(type: "numeric(12,3)", nullable: false),
                    fila = table.Column<int>(type: "integer", nullable: true),
                    columna = table.Column<int>(type: "integer", nullable: true),
                    cargado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    albaran_id = table.Column<Guid>(type: "uuid", nullable: true),
                    orden_carga_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pale_orden_carga", x => x.id);
                    table.ForeignKey(
                        name: "FK_pale_orden_carga_orden_carga_orden_carga_id",
                        column: x => x.orden_carga_id,
                        principalSchema: "agro",
                        principalTable: "orden_carga",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_linea_orden_carga_linea_pedido",
                schema: "agro",
                table: "linea_orden_carga",
                columns: new[] { "orden_carga_id", "linea_pedido_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_orden_carga_numero",
                schema: "agro",
                table: "orden_carga",
                columns: new[] { "empresa_id", "ejercicio", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pale_orden_carga_pale",
                schema: "agro",
                table: "pale_orden_carga",
                column: "pale_id");

            migrationBuilder.CreateIndex(
                name: "ux_pale_orden_carga_pale",
                schema: "agro",
                table: "pale_orden_carga",
                columns: new[] { "orden_carga_id", "pale_id" },
                unique: true);

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("agro", "orden_carga"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("agro", "linea_orden_carga", "orden_carga_id", "agro", "orden_carga"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("agro", "pale_orden_carga", "orden_carga_id", "agro", "orden_carga"));
            migrationBuilder.Sql("""
                ALTER TABLE agro.orden_carga ADD CONSTRAINT ck_orden_carga_estado CHECK (estado IN ('Propuesta','Pendiente','EnCarga','Finalizada','Anulada'));
                ALTER TABLE agro.orden_carga ADD CONSTRAINT ck_orden_carga_camion CHECK ((filas IS NULL AND columnas IS NULL) OR (filas BETWEEN 1 AND 20 AND columnas BETWEEN 1 AND 6));
                ALTER TABLE agro.linea_orden_carga ADD CONSTRAINT ck_linea_orden_carga_pales CHECK (pales_previstos > 0);
                ALTER TABLE agro.pale_orden_carga ADD CONSTRAINT ck_pale_orden_carga_kilos CHECK (kilos >= 0);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.QuitarRlsPorPadre("agro", "linea_orden_carga"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.QuitarRlsPorPadre("agro", "pale_orden_carga"));
            migrationBuilder.DropTable(
                name: "linea_orden_carga",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "pale_orden_carga",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "orden_carga",
                schema: "agro");
        }
    }
}
