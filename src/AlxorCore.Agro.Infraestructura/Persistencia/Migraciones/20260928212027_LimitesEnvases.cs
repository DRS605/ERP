using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlxorCore.Agro.Infraestructura.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class LimitesEnvases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "control_limite",
                schema: "agro",
                table: "cuenta_envases",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Aviso");

            migrationBuilder.CreateTable(
                name: "configuracion_envases",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_cierre = table.Column<DateOnly>(type: "date", nullable: true),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracion_envases", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "limite_envase",
                schema: "agro",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    envase_producto_id = table.Column<Guid>(type: "uuid", nullable: false),
                    limite = table.Column<int>(type: "integer", nullable: true),
                    minimo = table.Column<int>(type: "integer", nullable: true),
                    cuenta_envases_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_limite_envase", x => x.id);
                    table.ForeignKey(
                        name: "FK_limite_envase_cuenta_envases_cuenta_envases_id",
                        column: x => x.cuenta_envases_id,
                        principalSchema: "agro",
                        principalTable: "cuenta_envases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_configuracion_envases_empresa",
                schema: "agro",
                table: "configuracion_envases",
                column: "empresa_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_limite_envase",
                schema: "agro",
                table: "limite_envase",
                columns: new[] { "cuenta_envases_id", "envase_producto_id" },
                unique: true);

            migrationBuilder.Sql(AlxorCore.Persistencia.RlsSql.Activar("agro", "configuracion_envases"));
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.RlsPorPadre("agro", "limite_envase", "cuenta_envases_id", "agro", "cuenta_envases"));
            migrationBuilder.Sql("""
                ALTER TABLE agro.limite_envase ADD CONSTRAINT ck_limite_envase CHECK (coalesce(limite, 0) >= 0 AND coalesce(minimo, 0) >= 0
                    AND (limite IS NULL OR minimo IS NULL OR minimo <= limite));
                ALTER TABLE agro.cuenta_envases ADD CONSTRAINT ck_cuenta_envases_control CHECK (control_limite IN ('Aviso','Bloqueo'));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(AlxorCore.Persistencia.GarantiasSql.QuitarRlsPorPadre("agro", "limite_envase"));
            migrationBuilder.Sql("ALTER TABLE agro.cuenta_envases DROP CONSTRAINT IF EXISTS ck_cuenta_envases_control;");
            migrationBuilder.DropTable(
                name: "configuracion_envases",
                schema: "agro");

            migrationBuilder.DropTable(
                name: "limite_envase",
                schema: "agro");

            migrationBuilder.DropColumn(
                name: "control_limite",
                schema: "agro",
                table: "cuenta_envases");
        }
    }
}
