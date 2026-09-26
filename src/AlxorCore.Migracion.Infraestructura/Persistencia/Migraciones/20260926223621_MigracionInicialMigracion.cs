using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Migracion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class MigracionInicialMigracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "migracion");

            migrationBuilder.CreateTable(
                name: "correspondencia",
                schema: "migracion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    origen = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    entidad = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    origen_id = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    destino_id = table.Column<Guid>(type: "uuid", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_correspondencia", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ejecucion",
                schema: "migracion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    origen = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    fecha_corte = table.Column<DateOnly>(type: "date", nullable: false),
                    resumen = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ejecucion", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_correspondencia_destino",
                schema: "migracion",
                table: "correspondencia",
                columns: new[] { "empresa_id", "destino_id" });

            migrationBuilder.CreateIndex(
                name: "ux_correspondencia_origen",
                schema: "migracion",
                table: "correspondencia",
                columns: new[] { "empresa_id", "origen", "entidad", "origen_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ejecucion_empresa",
                schema: "migracion",
                table: "ejecucion",
                columns: new[] { "empresa_id", "creado_en" });

            migrationBuilder.Sql(GarantiasSql.FuncionesComunes);
            migrationBuilder.Sql(RlsSql.Activar("migracion", "correspondencia"));
            migrationBuilder.Sql(RlsSql.Activar("migracion", "ejecucion"));
            // La trazabilidad de la migración no se reescribe: qué se trajo de dónde queda como se hizo.
            migrationBuilder.Sql(GarantiasSql.SoloInsercion("migracion", "correspondencia", "migracion.inalterable",
                "La correspondencia con el sistema de origen no se modifica ni se borra."));
            migrationBuilder.Sql(GarantiasSql.SoloInsercion("migracion", "ejecucion", "migracion.inalterable",
                "El registro de las cargas no se modifica ni se borra."));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "correspondencia",
                schema: "migracion");

            migrationBuilder.DropTable(
                name: "ejecucion",
                schema: "migracion");
        }
    }
}
