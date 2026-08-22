using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Produccion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class MigracionInicialProduccion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "produccion");

            migrationBuilder.CreateTable(
                name: "orden_fabricacion",
                schema: "produccion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cantidad = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    almacen_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    terminada_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orden_fabricacion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "componente_plan",
                schema: "produccion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    componente_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cantidad_unitaria = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    cantidad_total = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    orden_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_componente_plan", x => x.id);
                    table.ForeignKey(
                        name: "FK_componente_plan_orden_fabricacion_orden_id",
                        column: x => x.orden_id,
                        principalSchema: "produccion",
                        principalTable: "orden_fabricacion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_componente_plan_orden_id",
                schema: "produccion",
                table: "componente_plan",
                column: "orden_id");

            migrationBuilder.CreateIndex(
                name: "ux_orden_serie",
                schema: "produccion",
                table: "orden_fabricacion",
                columns: new[] { "empresa_id", "ejercicio", "numero" },
                unique: true);

            // RLS por empresa (defensa en profundidad) en la tabla raíz.
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("produccion", "orden_fabricacion"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "componente_plan",
                schema: "produccion");

            migrationBuilder.DropTable(
                name: "orden_fabricacion",
                schema: "produccion");
        }
    }
}
