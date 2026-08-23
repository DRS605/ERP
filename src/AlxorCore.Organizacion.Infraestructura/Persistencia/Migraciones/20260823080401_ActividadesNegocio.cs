using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Organizacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ActividadesNegocio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "actividad_negocio",
                schema: "organizacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    actualizado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    grupo_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_actividad_negocio", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "visibilidad_actividad",
                schema: "organizacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    area = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    actividad_negocio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    grupo_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visibilidad_actividad", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_actividad_negocio_grupo",
                schema: "organizacion",
                table: "actividad_negocio",
                column: "grupo_id");

            migrationBuilder.CreateIndex(
                name: "ix_visibilidad_usuario_area",
                schema: "organizacion",
                table: "visibilidad_actividad",
                columns: new[] { "usuario_id", "area" });

            migrationBuilder.CreateIndex(
                name: "ux_visibilidad_grupo_usuario_area_actividad",
                schema: "organizacion",
                table: "visibilidad_actividad",
                columns: new[] { "grupo_id", "usuario_id", "area", "actividad_negocio_id" },
                unique: true);

            // RLS por grupo (segunda barrera): los maestros de actividad y su visibilidad son del grupo.
            migrationBuilder.Sql(RlsSql.ActivarPorGrupo("organizacion", "actividad_negocio"));
            migrationBuilder.Sql(RlsSql.ActivarPorGrupo("organizacion", "visibilidad_actividad"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "actividad_negocio",
                schema: "organizacion");

            migrationBuilder.DropTable(
                name: "visibilidad_actividad",
                schema: "organizacion");
        }
    }
}
