using System;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Organizacion.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ConsolidacionGrupo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "correspondencia_cuentas",
                schema: "organizacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_a_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cuenta_a = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    empresa_b_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cuenta_b = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    grupo_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_correspondencia_cuentas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "perimetro_consolidacion",
                schema: "organizacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    porcentaje = table.Column<decimal>(type: "numeric(8,4)", nullable: false),
                    metodo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    actualizado_en = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    grupo_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_perimetro_consolidacion", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_correspondencia_empresa_a",
                schema: "organizacion",
                table: "correspondencia_cuentas",
                column: "empresa_a_id");

            migrationBuilder.CreateIndex(
                name: "ix_correspondencia_empresa_b",
                schema: "organizacion",
                table: "correspondencia_cuentas",
                column: "empresa_b_id");

            migrationBuilder.CreateIndex(
                name: "ix_correspondencia_grupo",
                schema: "organizacion",
                table: "correspondencia_cuentas",
                column: "grupo_id");

            migrationBuilder.CreateIndex(
                name: "ix_perimetro_empresa",
                schema: "organizacion",
                table: "perimetro_consolidacion",
                column: "empresa_id");

            migrationBuilder.CreateIndex(
                name: "ux_perimetro_grupo_empresa",
                schema: "organizacion",
                table: "perimetro_consolidacion",
                columns: new[] { "grupo_id", "empresa_id" },
                unique: true);

            migrationBuilder.Sql(RlsSql.ActivarPorGrupo("organizacion", "perimetro_consolidacion"));
            migrationBuilder.Sql(RlsSql.ActivarPorGrupo("organizacion", "correspondencia_cuentas"));
            migrationBuilder.Sql("""
                ALTER TABLE organizacion.perimetro_consolidacion
                    ADD CONSTRAINT fk_perimetro_empresa FOREIGN KEY (empresa_id) REFERENCES organizacion.empresa (id) ON DELETE CASCADE,
                    ADD CONSTRAINT ck_perimetro_porcentaje CHECK (porcentaje > 0 AND porcentaje <= 100),
                    ADD CONSTRAINT ck_perimetro_metodo CHECK (metodo IN ('Global', 'Proporcional', 'Excluida'));
                ALTER TABLE organizacion.correspondencia_cuentas
                    ADD CONSTRAINT fk_correspondencia_empresa_a FOREIGN KEY (empresa_a_id) REFERENCES organizacion.empresa (id) ON DELETE CASCADE,
                    ADD CONSTRAINT fk_correspondencia_empresa_b FOREIGN KEY (empresa_b_id) REFERENCES organizacion.empresa (id) ON DELETE CASCADE,
                    ADD CONSTRAINT ck_correspondencia_empresas CHECK (empresa_a_id <> empresa_b_id),
                    ADD CONSTRAINT ck_correspondencia_cuentas CHECK (cuenta_a ~ '^[0-9]{3,12}$' AND cuenta_b ~ '^[0-9]{3,12}$');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "correspondencia_cuentas",
                schema: "organizacion");

            migrationBuilder.DropTable(
                name: "perimetro_consolidacion",
                schema: "organizacion");
        }
    }
}
