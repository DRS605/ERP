using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Organizacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class CentrosEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "centro",
                schema: "organizacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    direccion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    almacen_id = table.Column<Guid>(type: "uuid", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_centro", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "acceso_centro",
                schema: "organizacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    centro_id = table.Column<Guid>(type: "uuid", nullable: false),
                    creado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acceso_centro", x => x.id);
                    table.ForeignKey(
                        name: "fk_acceso_centro_centro",
                        column: x => x.centro_id,
                        principalSchema: "organizacion",
                        principalTable: "centro",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "caja_centro",
                schema: "organizacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    codigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    activa = table.Column<bool>(type: "boolean", nullable: false),
                    centro_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_caja_centro", x => x.id);
                    table.ForeignKey(
                        name: "FK_caja_centro_centro_centro_id",
                        column: x => x.centro_id,
                        principalSchema: "organizacion",
                        principalTable: "centro",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_acceso_centro_centro",
                schema: "organizacion",
                table: "acceso_centro",
                column: "centro_id");

            migrationBuilder.CreateIndex(
                name: "ux_acceso_centro_usuario",
                schema: "organizacion",
                table: "acceso_centro",
                columns: new[] { "empresa_id", "usuario_id", "centro_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_caja_centro_codigo",
                schema: "organizacion",
                table: "caja_centro",
                columns: new[] { "centro_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_centro_codigo",
                schema: "organizacion",
                table: "centro",
                columns: new[] { "empresa_id", "codigo" },
                unique: true);

            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.FuncionesComunes);
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("organizacion", "centro"));
            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("organizacion", "acceso_centro"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("organizacion", "caja_centro", "centro_id", "organizacion", "centro"));
            migrationBuilder.Sql("""
                ALTER TABLE organizacion.centro ADD CONSTRAINT ck_centro_codigo CHECK (codigo ~ '^[A-Z0-9_-]{1,10}$');
                ALTER TABLE organizacion.caja_centro ADD CONSTRAINT ck_caja_centro_codigo CHECK (codigo ~ '^[A-Z0-9_-]{1,10}$');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.QuitarRlsPorPadre("organizacion", "caja_centro"));
            migrationBuilder.DropTable(
                name: "acceso_centro",
                schema: "organizacion");

            migrationBuilder.DropTable(
                name: "caja_centro",
                schema: "organizacion");

            migrationBuilder.DropTable(
                name: "centro",
                schema: "organizacion");
        }
    }
}
