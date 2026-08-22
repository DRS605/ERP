using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Proyectos.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class MigracionInicialProyectos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "proyectos");

            migrationBuilder.CreateTable(
                name: "proyecto",
                schema: "proyectos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cliente_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    presupuesto = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    fecha_inicio = table.Column<DateOnly>(type: "date", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    actualizado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    cerrado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proyecto", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "imputacion",
                schema: "proyectos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    referencia_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cantidad = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    coste_unitario = table.Column<decimal>(type: "numeric(14,4)", nullable: false),
                    importe = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    proyecto_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_imputacion", x => x.id);
                    table.ForeignKey(
                        name: "FK_imputacion_proyecto_proyecto_id",
                        column: x => x.proyecto_id,
                        principalSchema: "proyectos",
                        principalTable: "proyecto",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_imputacion_proyecto_id",
                schema: "proyectos",
                table: "imputacion",
                column: "proyecto_id");

            migrationBuilder.CreateIndex(
                name: "ux_proyecto_serie",
                schema: "proyectos",
                table: "proyecto",
                columns: new[] { "empresa_id", "ejercicio", "numero" },
                unique: true);

            // Red de seguridad a nivel de BD: aislamiento por empresa (RLS). La tabla imputacion es
            // una colección propietaria del proyecto (sin empresa_id): queda protegida por su FK.
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("proyectos", "proyecto"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "imputacion",
                schema: "proyectos");

            migrationBuilder.DropTable(
                name: "proyecto",
                schema: "proyectos");
        }
    }
}
